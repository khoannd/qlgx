using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Qlgx.Data.Migrations
{
    /// <inheritdoc />
    /// <summary>
    /// Cho hai bảng nối (thanh_vien_gia_dinh, giao_dan_hon_phoi) một khoá chính uuid riêng để
    /// chúng vào được nhật ký thay đổi mức trường: bộ sinh nhật ký duyệt các thực thể kế thừa
    /// ThucTheCoSo và mỗi dòng nhật ký đánh địa chỉ bản ghi qua một BanGhiId DUY NHẤT. Trước
    /// đây hai bảng này đứng ngoài, nên việc chuyển một giáo dân sang gia đình khác chỉ sinh
    /// INSERT/DELETE mà không sinh dòng nhật ký nào — đồng bộ xong các máy khác vẫn thấy người
    /// đó ở gia đình cũ và không báo lỗi gì, sổ gia đình phân kỳ vĩnh viễn.
    ///
    /// VIẾT TAY, KHÔNG dùng nguyên bản EF sinh ra. Bản EF sinh có ba chỗ làm hỏng dữ liệu thật
    /// của các giáo xứ đã nhập sổ sách:
    ///
    /// 1. `AddColumn&lt;Guid&gt;("id", defaultValue: Guid.Empty)` — MỌI dòng cũ nhận cùng một
    ///    giá trị, đặt khoá chính lên đó là bất khả (hoặc tệ hơn: mất dòng). Ở đây thay bằng
    ///    ba bước tách rời: thêm cột CHO PHÉP NULL → UPDATE gen_random_uuid() cho từng dòng →
    ///    SET NOT NULL. Chỉ khi cột đã đầy giá trị khác nhau mới bỏ khoá chính cũ và đặt khoá
    ///    chính mới.
    /// 2. `created_at`/`updated_at` mặc định 0001-01-01 — dòng sổ sách nhập từ Access sẽ mang
    ///    mốc thời gian vô nghĩa vĩnh viễn. Ở đây backfill bằng now().
    /// 3. `AddColumn&lt;uint&gt;("xmin")` — xmin là CỘT HỆ THỐNG của PostgreSQL, không phải cột
    ///    thật; RowVersion chỉ ánh xạ tới nó. Bỏ hẳn, không tạo cột nào tên xmin.
    ///
    /// Khoá phức cũ KHÔNG bị bỏ mà trở thành CHỈ MỤC DUY NHẤT — nó là thứ giữ cho một giáo dân
    /// không bị ghi hai lần vào cùng một gia đình (và không bị nối hai lần vào cùng một hôn
    /// phối). Thêm khoá chính uuid chỉ để nhật ký đánh địa chỉ được từng dòng, không phải để
    /// nới lỏng ràng buộc nghiệp vụ.
    ///
    /// Chỉ mục duy nhất giữ ĐÚNG các cột của khoá chính cũ, không nhiều không ít:
    /// thanh_vien_gia_dinh là BỘ BA (gia_dinh_id, giao_dan_id, vai_tro), giao_dan_hon_phoi là
    /// BỘ ĐÔI (giao_dan_id, hon_phoi_id). Đã cân nhắc siết bộ ba xuống bộ đôi cho "đẹp" hơn về
    /// nghiệp vụ rồi BỎ: đổi phạm vi ràng buộc là đổi luật, không phải việc của một migration
    /// thêm khoá thay thế, và nó tạo ra một đường MẤT DÒNG mới ở luồng nhập dữ liệu Access
    /// (bản Access cho phép cùng cặp với vai trò khác nhau). Dữ liệu giáo xứ mất là không lấy
    /// lại được — không đánh đổi lấy một ràng buộc gọn hơn mà không ai yêu cầu.
    /// </summary>
    public partial class ThemKhoaChinhChoBangNoi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // gen_random_uuid() là hàm dựng sẵn của PostgreSQL từ bản 13 — không cần pgcrypto.
            foreach (var bang in new[] { "thanh_vien_gia_dinh", "giao_dan_hon_phoi" })
            {
                // Bước 1: cột id có giá trị RIÊNG cho từng dòng cũ TRƯỚC khi thành khoá chính.
                migrationBuilder.Sql($"ALTER TABLE {bang} ADD COLUMN id uuid;");
                migrationBuilder.Sql($"UPDATE {bang} SET id = gen_random_uuid();");
                migrationBuilder.Sql($"ALTER TABLE {bang} ALTER COLUMN id SET NOT NULL;");

                // Bước 2: các cột còn lại của ThucTheCoSo. Mốc thời gian phải có giá trị thật
                // cho dòng cũ — để NULL rồi SET NOT NULL sẽ thất bại, để 0001-01-01 thì sai.
                migrationBuilder.Sql($"ALTER TABLE {bang} ADD COLUMN created_at timestamp with time zone;");
                migrationBuilder.Sql($"ALTER TABLE {bang} ADD COLUMN updated_at timestamp with time zone;");
                migrationBuilder.Sql($"UPDATE {bang} SET created_at = now(), updated_at = now();");
                migrationBuilder.Sql($"ALTER TABLE {bang} ALTER COLUMN created_at SET NOT NULL;");
                migrationBuilder.Sql($"ALTER TABLE {bang} ALTER COLUMN updated_at SET NOT NULL;");

                // source_system và du_lieu_loi cho phép NULL theo đúng mô hình, không cần backfill.
                migrationBuilder.Sql($"ALTER TABLE {bang} ADD COLUMN source_system text;");
                migrationBuilder.Sql($"ALTER TABLE {bang} ADD COLUMN du_lieu_loi jsonb;");
            }

            // Bước 3: chỉ mục duy nhất thay cho khoá chính cũ — TẠO TRƯỚC khi bỏ khoá chính cũ
            // để không có khoảnh khắc nào bảng chạy mà thiếu ràng buộc chống trùng.
            migrationBuilder.CreateIndex(
                name: "ix_thanh_vien_gia_dinh_gia_dinh_id_giao_dan_id_vai_tro",
                table: "thanh_vien_gia_dinh",
                columns: new[] { "gia_dinh_id", "giao_dan_id", "vai_tro" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_giao_dan_hon_phoi_giao_dan_id_hon_phoi_id",
                table: "giao_dan_hon_phoi",
                columns: new[] { "giao_dan_id", "hon_phoi_id" },
                unique: true);

            // Bước 4: đổi khoá chính. Tên cũ và tên mới trùng nhau nên bắt buộc bỏ trước, đặt sau.
            migrationBuilder.DropPrimaryKey(
                name: "pk_thanh_vien_gia_dinh",
                table: "thanh_vien_gia_dinh");

            migrationBuilder.AddPrimaryKey(
                name: "pk_thanh_vien_gia_dinh",
                table: "thanh_vien_gia_dinh",
                column: "id");

            migrationBuilder.DropPrimaryKey(
                name: "pk_giao_dan_hon_phoi",
                table: "giao_dan_hon_phoi");

            migrationBuilder.AddPrimaryKey(
                name: "pk_giao_dan_hon_phoi",
                table: "giao_dan_hon_phoi",
                column: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Trả khoá chính về khoá phức cũ TRƯỚC, rồi mới bỏ chỉ mục duy nhất và các cột —
            // ngược đúng thứ tự của Up để bảng không lúc nào không có khoá chính.
            migrationBuilder.DropPrimaryKey(
                name: "pk_thanh_vien_gia_dinh",
                table: "thanh_vien_gia_dinh");

            migrationBuilder.AddPrimaryKey(
                name: "pk_thanh_vien_gia_dinh",
                table: "thanh_vien_gia_dinh",
                columns: new[] { "gia_dinh_id", "giao_dan_id", "vai_tro" });

            migrationBuilder.DropPrimaryKey(
                name: "pk_giao_dan_hon_phoi",
                table: "giao_dan_hon_phoi");

            migrationBuilder.AddPrimaryKey(
                name: "pk_giao_dan_hon_phoi",
                table: "giao_dan_hon_phoi",
                columns: new[] { "giao_dan_id", "hon_phoi_id" });

            migrationBuilder.DropIndex(
                name: "ix_thanh_vien_gia_dinh_gia_dinh_id_giao_dan_id_vai_tro",
                table: "thanh_vien_gia_dinh");

            migrationBuilder.DropIndex(
                name: "ix_giao_dan_hon_phoi_giao_dan_id_hon_phoi_id",
                table: "giao_dan_hon_phoi");

            foreach (var bang in new[] { "thanh_vien_gia_dinh", "giao_dan_hon_phoi" })
                migrationBuilder.Sql(
                    $"ALTER TABLE {bang} " +
                    "DROP COLUMN id, DROP COLUMN created_at, DROP COLUMN updated_at, " +
                    "DROP COLUMN source_system, DROP COLUMN du_lieu_loi;");
        }
    }
}
