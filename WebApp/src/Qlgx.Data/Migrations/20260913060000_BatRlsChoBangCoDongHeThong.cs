using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Qlgx.Data.Migrations
{
    /// <inheritdoc />
    /// <summary>
    /// Bổ sung lớp phòng thủ THỨ HAI (Row-Level Security) cho hai bảng "có dòng cấp hệ thống" —
    /// <c>mau_in_tuy_chinh</c> và <c>cach_hien_thi_dung_sai</c> — là hai bảng duy nhất có
    /// <c>giao_xu_id</c> NHƯNG bị bỏ lại ngoài migration 20260906210934_BatRlsChoBangTheoGiaoXu.
    ///
    /// VÌ SAO CHÚNG TỪNG BỊ BỎ LẠI: policy chung của dự án là
    /// <c>giao_xu_id::text = current_setting('app.giao_xu_id', true)</c>. Ở hai bảng này,
    /// <c>giao_xu_id IS NULL</c> mang nghĩa "dòng cấp hệ thống, áp dụng cho MỌI giáo xứ chưa tự
    /// tuỳ chỉnh" — mà <c>NULL::text = '...'</c> trả về NULL (không khớp), nên policy chung sẽ
    /// GIẤU MẤT chính những dòng mọi giáo xứ đều phải đọc được, làm hỏng tính năng "Quản trị hệ
    /// thống đặt mẫu in / câu chữ dùng chung". Vì vậy policy ở đây phải NHẬN BIẾT NULL tường minh.
    ///
    /// GIỚI HẠN ĐÃ BIẾT, CỐ Ý CHẤP NHẬN: vế <c>WITH CHECK</c> cũng cho phép GHI dòng
    /// <c>giao_xu_id IS NULL</c>, nghĩa là ở tầng CSDL bất kỳ phiên đăng nhập nào cũng ghi được
    /// dòng cấp hệ thống. Không thể siết chặt hơn bằng RLS thuần: Quản trị hệ thống cũng đăng
    /// nhập DƯỚI một giáo xứ cụ thể (tham số phiên <c>app.giao_xu_id</c> luôn là một giáo xứ
    /// thật), nên CSDL không có cách nào phân biệt "quản trị hệ thống" với "quản trị giáo xứ
    /// thường" — siết <c>WITH CHECK</c> xuống chỉ giao_xu_id của mình sẽ chặn luôn cả đường ghi
    /// hợp lệ của Quản trị hệ thống và làm hỏng tính năng. Việc chặn đó thuộc TẦNG ỨNG DỤNG
    /// (policy "QuanTriHeThong" trên các route <c>/he-thong</c>, xem MauInEndpoints và
    /// CachHienThiDungSaiEndpoints, đều có bài test khẳng định tài khoản thường nhận 403).
    /// Muốn khoá cả ở tầng CSDL thì phải thêm MỘT tham số phiên mới (vd
    /// <c>app.quan_tri_he_thong</c>) do BoiCanhGiaoXuConnectionInterceptor đặt — đổi hành vi của
    /// MỌI kết nối trong hệ thống, nên cố ý KHÔNG gộp vào đây.
    ///
    /// Phần RLS thật sự có giá trị ngay ở migration này: dòng RIÊNG của từng giáo xứ
    /// (<c>giao_xu_id</c> khác NULL) nay được CSDL cách ly, đúng như 24 bảng còn lại — giáo xứ A
    /// không đọc/ghi được mẫu in hay câu chữ riêng của giáo xứ B kể cả khi tầng ứng dụng có lỗi.
    ///
    /// Giữ nguyên mọi quy ước của migration RLS gốc: cùng tên policy <c>loc_theo_giao_xu</c>, so
    /// sánh ép cột uuid SANG text (vì <c>''::uuid</c> ném lỗi khi tham số phiên chưa đặt), và
    /// KHÔNG dùng FORCE ROW LEVEL SECURITY (chủ bảng vẫn cần vận hành thủ công).
    /// </summary>
    public partial class BatRlsChoBangCoDongHeThong : Migration
    {
        /// <summary>Hai bảng có cột giao_xu_id NULL-được, nghĩa "dòng cấp hệ thống dùng chung".</summary>
        private static readonly string[] BangCoDongHeThong = { "mau_in_tuy_chinh", "cach_hien_thi_dung_sai" };

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            foreach (var bang in BangCoDongHeThong)
            {
                migrationBuilder.Sql($"ALTER TABLE {bang} ENABLE ROW LEVEL SECURITY;");
                migrationBuilder.Sql($"""
                    CREATE POLICY loc_theo_giao_xu ON {bang}
                        USING (giao_xu_id IS NULL
                               OR giao_xu_id::text = current_setting('app.giao_xu_id', true))
                        WITH CHECK (giao_xu_id IS NULL
                                    OR giao_xu_id::text = current_setting('app.giao_xu_id', true));
                    """);
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            foreach (var bang in BangCoDongHeThong)
            {
                migrationBuilder.Sql($"DROP POLICY IF EXISTS loc_theo_giao_xu ON {bang};");
                migrationBuilder.Sql($"ALTER TABLE {bang} DISABLE ROW LEVEL SECURITY;");
            }
        }
    }
}
