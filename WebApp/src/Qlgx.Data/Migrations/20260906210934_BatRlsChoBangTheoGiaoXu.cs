using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Qlgx.Data.Migrations
{
    /// <inheritdoc />
    /// <summary>
    /// Lớp phòng thủ THỨ HAI cho mô hình nhiều giáo xứ dùng chung một database — Row-Level
    /// Security của PostgreSQL, theo đúng cột giao_xu_id mà bộ lọc toàn cục của EF Core
    /// (QlgxDbContext.OnModelCreating) đã dùng làm lớp thứ nhất. Áp dụng đúng 22 bảng có
    /// giao_xu_id (khớp danh sách HasQueryFilter trong QlgxDbContext) — KHÔNG áp dụng cho
    /// GiaoPhan/GiaoHat vì hai bảng đó không có giao_xu_id, nằm trên cấp giáo xứ.
    ///
    /// Chính sách so sánh giao_xu_id::text với current_setting('app.giao_xu_id', true) — tham
    /// số phiên do BoiCanhGiaoXuConnectionInterceptor đặt mỗi khi mở kết nối. CỐ Ý ép kiểu cột
    /// uuid SANG text (chiều ngược lại — ép current_setting() sang uuid) vì '' (chưa đặt tham
    /// số) không phải uuid hợp lệ: PostgreSQL NÉM LỖI ngay khi ép kiểu ''::uuid (không trả về
    /// NULL êm ái như tưởng tượng ban đầu — đã xác nhận bằng RlsTests khi viết migration này),
    /// còn so sánh text với '' thì chỉ đơn giản là không khớp, không lỗi. Không khớp -> không
    /// đọc được dòng nào -> đây là lựa chọn "đóng mặc định" (fail-closed) có chủ đích: vai trò
    /// CSDL phục vụ nghiệp vụ hằng ngày KHÔNG có BYPASSRLS, nên một kết nối không đặt tham số
    /// phiên (kể cả kết nối thô ngoài ứng dụng dùng cùng vai trò) sẽ không đọc được gì thay vì
    /// đọc được mọi giáo xứ. Ba đường dẫn hợp lệ cần truy vấn chéo giáo xứ (đăng nhập, công cụ
    /// chuyển dữ liệu, tạo tài khoản quản trị đầu tiên) dùng một chuỗi kết nối RIÊNG trỏ tới vai
    /// trò có BYPASSRLS — xem TRIEN-KHAI.md.
    ///
    /// KHÔNG dùng FORCE ROW LEVEL SECURITY: chủ bảng (vai trò chạy migration, thường là vai trò
    /// quản trị CSDL) vẫn cần đọc/ghi không giới hạn để chuyển dữ liệu và vận hành thủ công.
    /// PostgreSQL đã mặc định không áp policy cho chủ bảng — không cần FORCE để việc đó xảy ra;
    /// FORCE chỉ cần khi muốn RÀNG BUỘC cả chủ bảng, không phải trường hợp ở đây.
    /// </summary>
    public partial class BatRlsChoBangTheoGiaoXu : Migration
    {
        private static readonly string[] BangTheoGiaoXu =
        {
            "giao_ho", "gia_dinh", "giao_dan", "thanh_vien_gia_dinh", "hon_phoi",
            "giao_dan_hon_phoi", "bo_dem_ma", "cau_hinh", "du_lieu_chung", "vai_tro",
            "ten_loai_tai_khoan", "tai_khoan", "dot_bi_tich", "bi_tich_chi_tiet", "chuyen_xu",
            "rao_hon_phoi", "tan_hien", "linh_muc", "khoi_giao_ly", "lop_giao_ly",
            "chi_tiet_lop_giao_ly", "giao_ly_vien", "hoi_doan", "chi_tiet_hoi_doan",
        };

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            foreach (var bang in BangTheoGiaoXu)
            {
                migrationBuilder.Sql($"ALTER TABLE {bang} ENABLE ROW LEVEL SECURITY;");
                migrationBuilder.Sql($"""
                    CREATE POLICY loc_theo_giao_xu ON {bang}
                        USING (giao_xu_id::text = current_setting('app.giao_xu_id', true))
                        WITH CHECK (giao_xu_id::text = current_setting('app.giao_xu_id', true));
                    """);
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            foreach (var bang in BangTheoGiaoXu)
            {
                migrationBuilder.Sql($"DROP POLICY IF EXISTS loc_theo_giao_xu ON {bang};");
                migrationBuilder.Sql($"ALTER TABLE {bang} DISABLE ROW LEVEL SECURITY;");
            }
        }
    }
}
