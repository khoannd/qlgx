using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Qlgx.Data.Migrations
{
    /// <inheritdoc />
    public partial class ThemBangNhatKyThayDoi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "hieu_luc",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    giao_xu_id = table.Column<Guid>(type: "uuid", nullable: false),
                    so_thu_tu = table.Column<long>(type: "bigint", nullable: false),
                    epoch = table.Column<Guid>(type: "uuid", nullable: false),
                    bang = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ban_ghi_id = table.Column<Guid>(type: "uuid", nullable: false),
                    truong = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    gia_tri = table.Column<string>(type: "jsonb", nullable: true),
                    dong_ho_vat_ly = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    dong_ho_logic = table.Column<long>(type: "bigint", nullable: false),
                    thiet_bi_id = table.Column<Guid>(type: "uuid", nullable: true),
                    giao_dich_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_hieu_luc", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "thay_doi",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    giao_xu_id = table.Column<Guid>(type: "uuid", nullable: false),
                    bang = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ban_ghi_id = table.Column<Guid>(type: "uuid", nullable: false),
                    truong = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    gia_tri = table.Column<string>(type: "jsonb", nullable: true),
                    loai = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    dong_ho_vat_ly = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    dong_ho_logic = table.Column<long>(type: "bigint", nullable: false),
                    thiet_bi_id = table.Column<Guid>(type: "uuid", nullable: true),
                    tai_khoan_id = table.Column<Guid>(type: "uuid", nullable: true),
                    ma_thao_tac = table.Column<Guid>(type: "uuid", nullable: false),
                    giao_dich_id = table.Column<Guid>(type: "uuid", nullable: false),
                    thang = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_thay_doi", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_hieu_luc_giao_xu_id_so_thu_tu",
                table: "hieu_luc",
                columns: new[] { "giao_xu_id", "so_thu_tu" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_thay_doi_giao_xu_id_bang_ban_ghi_id_dong_ho_vat_ly",
                table: "thay_doi",
                columns: new[] { "giao_xu_id", "bang", "ban_ghi_id", "dong_ho_vat_ly" });

            // Lớp phòng thủ thứ hai, y hệt 24 bảng nghiệp vụ (xem BatRlsChoBangTheoGiaoXu).
            // Bỏ sót là nhật ký của giáo xứ này lộ sang giáo xứ khác — mà nhật ký chứa NGUYÊN
            // VĂN giá trị các ô, nên rò rỉ ở đây tương đương rò rỉ toàn bộ dữ liệu.
            foreach (var bang in new[] { "thay_doi", "hieu_luc", "bo_dem_hieu_luc" })
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
            foreach (var bang in new[] { "thay_doi", "hieu_luc", "bo_dem_hieu_luc" })
            {
                migrationBuilder.Sql($"DROP POLICY IF EXISTS loc_theo_giao_xu ON {bang};");
                migrationBuilder.Sql($"ALTER TABLE {bang} DISABLE ROW LEVEL SECURITY;");
            }

            migrationBuilder.DropTable(
                name: "hieu_luc");

            migrationBuilder.DropTable(
                name: "thay_doi");
        }
    }
}
