using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Qlgx.Data.Migrations
{
    /// <inheritdoc />
    /// <summary>
    /// NT-2 (.superpowers/review-sao-luu/review-bao-mat.md) — làm cho Row-Level Security THẬT SỰ
    /// có hiệu lực ở môi trường sản xuất.
    ///
    /// Migration BatRlsChoBangTheoGiaoXu ghi rằng không cần FORCE vì "chủ bảng là vai trò quản
    /// trị CSDL". Giả định đó SAI với cách dự án này triển khai thật: migration chạy lúc container
    /// API khởi động (docker-compose.yml, Qlgx__ChayMigrationKhiKhoiDong=true) bằng chính
    /// ConnectionStrings__Qlgx, tức bằng vai trò NGHIỆP VỤ `qlgx_app`. Vậy `qlgx_app` là CHỦ của
    /// toàn bộ bảng — và PostgreSQL KHÔNG áp policy RLS cho chủ bảng nếu bảng chưa bật FORCE.
    /// Hậu quả: mọi policy `loc_theo_giao_xu` trên các bảng nghiệp vụ đều bị bỏ qua cho đúng cái
    /// vai trò phục vụ 100% nghiệp vụ hằng ngày. Lớp phòng thủ thứ hai không tồn tại, trong khi
    /// tài liệu lẫn mã nguồn đều tin rằng nó đang chạy.
    ///
    /// Vì sao FORCE không phá các đường hợp lệ cần truy vấn CHÉO giáo xứ: cả ba đường đó (đăng
    /// nhập — AuthService; tạo tài khoản quản trị đầu tiên — TaoTaiKhoanQuanTri; quản lý giáo xứ
    /// và công cụ chuyển dữ liệu — QuanLyGiaoXuService/Qlgx.Migration) đều mở kết nối bằng chuỗi
    /// QUẢN TRỊ riêng (ChuoiKetNoiQuanTri) trỏ tới vai trò có BYPASSRLS, và BYPASSRLS thắng cả
    /// FORCE. Ba dịch vụ sao lưu cũng không đụng tới các bảng này (chúng chỉ ghi hàng đợi, và ba
    /// bảng hàng đợi không có giao_xu_id nên không bật RLS). Điểm gác khởi động
    /// KiemTraCauHinh.LoiChuBangChuaForceRls canh để trạng thái hỏng cũ không lặng lẽ quay lại.
    ///
    /// Quét theo catalog (relrowsecurity) thay vì chép tay danh sách bảng: RLS hiện được bật ở
    /// năm migration khác nhau, chép tay là chắc chắn sót — và sót một bảng ở đây nghĩa là bảng
    /// đó vẫn rò dữ liệu chéo giáo xứ y như trước. Bảng thêm SAU migration này được canh bằng
    /// RlsTests.Moi_bang_co_giao_xu_id_deu_duoc_bat_force_row_level_security.
    /// </summary>
    public partial class BatForceRlsChoBangTheoGiaoXu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder) =>
            migrationBuilder.Sql(CauLenhDoiForce("FORCE ROW LEVEL SECURITY"));

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) =>
            migrationBuilder.Sql(CauLenhDoiForce("NO FORCE ROW LEVEL SECURITY"));

        /// <summary>Áp <paramref name="menhDe"/> cho MỌI bảng đang bật RLS trong schema hiện
        /// hành. Dùng format(%I) để tên bảng luôn được trích dẫn đúng chuẩn định danh — tên bảng
        /// ở đây đến từ catalog của chính CSDL, không phải từ người dùng, nhưng nối chuỗi thô vào
        /// DDL là thói quen không nên có ở bất cứ đâu.</summary>
        private static string CauLenhDoiForce(string menhDe) => $"""
            DO $$
            DECLARE bang record;
            BEGIN
                FOR bang IN
                    SELECT c.oid::regclass AS ten
                    FROM pg_class c
                    JOIN pg_namespace n ON n.oid = c.relnamespace
                    WHERE c.relkind = 'r'
                      AND c.relrowsecurity
                      AND n.nspname NOT IN ('pg_catalog', 'information_schema')
                LOOP
                    EXECUTE format('ALTER TABLE %s {menhDe}', bang.ten);
                END LOOP;
            END $$;
            """;
    }
}
