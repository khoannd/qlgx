using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Qlgx.Data.Migrations
{
    /// <inheritdoc />
    public partial class ThemCanXemLaiVaDongHoMayChu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "dau_cuoi_logic",
                table: "bo_dem_hieu_luc",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "dau_cuoi_vat_ly",
                table: "bo_dem_hieu_luc",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "'-infinity'");

            migrationBuilder.CreateTable(
                name: "can_xem_lai",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    giao_xu_id = table.Column<Guid>(type: "uuid", nullable: false),
                    loai = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    bang = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ban_ghi_id = table.Column<Guid>(type: "uuid", nullable: false),
                    truong = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    gia_tri_a = table.Column<string>(type: "jsonb", nullable: true),
                    gia_tri_b = table.Column<string>(type: "jsonb", nullable: true),
                    gia_tri_dang_dung = table.Column<string>(type: "jsonb", nullable: true),
                    thiet_bi_a = table.Column<Guid>(type: "uuid", nullable: true),
                    thiet_bi_b = table.Column<Guid>(type: "uuid", nullable: true),
                    luc_a = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    luc_b = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    tao_luc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    da_xu_ly_luc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    nguoi_xu_ly = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_can_xem_lai", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_can_xem_lai_giao_xu_id_da_xu_ly_luc",
                table: "can_xem_lai",
                columns: new[] { "giao_xu_id", "da_xu_ly_luc" });

            // Lớp phòng thủ thứ hai, y hệt moc_o/thao_tac_da_nhan. can_xem_lai chứa GIÁ TRỊ THẬT
            // của các ô đang tranh chấp (tên người, ngày bí tích) — rò rỉ sang giáo xứ khác là
            // rò rỉ chính sổ sách, không phải chỉ siêu dữ liệu.
            migrationBuilder.Sql("ALTER TABLE can_xem_lai ENABLE ROW LEVEL SECURITY;");
            migrationBuilder.Sql("""
                CREATE POLICY loc_theo_giao_xu ON can_xem_lai
                    USING (giao_xu_id::text = current_setting('app.giao_xu_id', true))
                    WITH CHECK (giao_xu_id::text = current_setting('app.giao_xu_id', true));
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP POLICY IF EXISTS loc_theo_giao_xu ON can_xem_lai;");
            migrationBuilder.Sql("ALTER TABLE can_xem_lai DISABLE ROW LEVEL SECURITY;");

            migrationBuilder.DropTable(
                name: "can_xem_lai");

            migrationBuilder.DropColumn(
                name: "dau_cuoi_logic",
                table: "bo_dem_hieu_luc");

            migrationBuilder.DropColumn(
                name: "dau_cuoi_vat_ly",
                table: "bo_dem_hieu_luc");
        }
    }
}
