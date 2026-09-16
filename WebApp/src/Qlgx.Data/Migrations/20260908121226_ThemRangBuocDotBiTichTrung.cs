using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Qlgx.Data.Migrations
{
    /// <inheritdoc />
    public partial class ThemRangBuocDotBiTichTrung : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Lưới an toàn cuối cấp CSDL cho race condition tạo trùng đợt bí tích (xem
            // review-toan-nhanh-dulieu.md mục C1): hai request "Tạo danh sách bí tích tự động"
            // chạy đồng thời có thể cùng SELECT thấy "chưa có đợt khớp" (do bên kia chưa
            // commit) rồi cùng INSERT một DotBiTich mới cho cùng nhóm. Chặn ở đây bằng đúng
            // khoá nhóm mà TaoDotBiTichTuDongService.TinhToan dùng để gộp
            // ((LinhMuc ?? "").Trim().ToLowerInvariant(), NgayBiTich) — dùng chỉ mục biểu thức
            // (lower(trim(coalesce(...)))) thay vì chỉ mục cột thường để bắt đúng cả trường hợp
            // hai request gộp cùng một nhóm nhưng khác hoa/thường hoặc khoảng trắng đầu/cuối.
            //
            // Đã kiểm dữ liệu thật qlgx_thu trước khi thêm (xem
            // task-sua-review-backend-2.md): 0 nhóm trùng trong số các đợt CÓ ngay_bi_tich (chỉ
            // 3 nhóm/42 dòng trùng đều có ngay_bi_tich NULL — NULL vốn không bao giờ đụng ràng
            // buộc UNIQUE ở PostgreSQL nên an toàn tuyệt đối, không cần WHERE loại trừ).
            migrationBuilder.Sql("""
                CREATE UNIQUE INDEX ux_dot_bi_tich_nhom_trung
                ON dot_bi_tich (giao_xu_id, loai_bi_tich, (lower(trim(coalesce(linh_muc, '')))), ngay_bi_tich);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX ux_dot_bi_tich_nhom_trung;");
        }
    }
}
