using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Qlgx.Data.Migrations
{
    /// <summary>
    /// Chỉ mục riêng phần bảo đảm KHÔNG BAO GIỜ có quá một công việc sao lưu/phục hồi đang mở
    /// (trạng thái "cho" hoặc "dang_chay").
    ///
    /// Vì sao phải ở tầng CSDL: guard trong SaoLuuService.TaoCongViec là "đọc rồi ghi" — hai
    /// lượt đi CSDL riêng biệt, không nằm trong một giao dịch serializable. Hai yêu cầu POST tới
    /// cùng lúc (người dùng bấm hai lần trên mạng chậm, hoặc trình duyệt tự thử lại) đều thấy
    /// hàng đợi trống và cùng chèn được. Hậu quả nặng nhất: HAI công việc "phuc_hoi" vào hàng
    /// đợi, bộ chạy thi hành tuần tự → phục hồi xong rồi lập tức phục hồi đè lên chính nó lần
    /// nữa. Chỉ mục này là lớp duy nhất đúng bất kể thời điểm.
    ///
    /// Biểu thức chỉ mục là hằng số `(true)` — mọi dòng khớp điều kiện WHERE đều mang cùng một
    /// khoá, nên chỉ một dòng tồn tại được.
    /// </summary>
    public partial class ChanHaiCongViecSaoLuuDangMo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Dọn dữ liệu cũ TRƯỚC khi tạo chỉ mục: một máy chủ đang chạy có thể đã có nhiều
            // dòng đang mở (đúng lỗi mà chỉ mục này chặn). Nếu không dọn, lệnh CREATE UNIQUE
            // INDEX sẽ thất bại và cả lần cập nhật hỏng theo — không được phép, vì migration
            // chạy lúc khởi động container API.
            //
            // Giữ lại dòng CŨ NHẤT (đúng dòng mà bộ chạy sẽ nhặt trước: ORDER BY tao_luc), các
            // dòng còn lại chuyển sang "loi" kèm lý do bằng tiếng Việt để quản trị viên đọc
            // được trên màn hình thay vì thấy chúng biến mất không dấu vết.
            migrationBuilder.Sql(
                "UPDATE cong_viec_sao_luu SET trang_thai = 'loi', ket_thuc_luc = now(), " +
                "nhat_ky = coalesce(nhat_ky, '') || " +
                "' [Hệ thống] Công việc này bị huỷ khi nâng cấp: chỉ cho phép một công việc " +
                "sao lưu/phục hồi chạy một lúc.' " +
                "WHERE trang_thai IN ('cho', 'dang_chay') AND id <> (" +
                "  SELECT id FROM cong_viec_sao_luu WHERE trang_thai IN ('cho', 'dang_chay') " +
                "  ORDER BY tao_luc, id LIMIT 1);");

            migrationBuilder.Sql(
                "CREATE UNIQUE INDEX ux_cong_viec_sao_luu_dang_mo ON cong_viec_sao_luu ((true)) " +
                "WHERE trang_thai IN ('cho', 'dang_chay');");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX IF EXISTS ux_cong_viec_sao_luu_dang_mo;");
        }
    }
}
