using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Qlgx.Data.Migrations
{
    /// <inheritdoc />
    public partial class ThemBangGiaoThucDongBo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "cho_phep_bu_lai",
                table: "bo_dem_hieu_luc",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "xoay_epoch_luc",
                table: "bo_dem_hieu_luc",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "moc_o",
                columns: table => new
                {
                    giao_xu_id = table.Column<Guid>(type: "uuid", nullable: false),
                    bang = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ban_ghi_id = table.Column<Guid>(type: "uuid", nullable: false),
                    truong = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    dong_ho_vat_ly = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    dong_ho_logic = table.Column<long>(type: "bigint", nullable: false),
                    thiet_bi_id = table.Column<Guid>(type: "uuid", nullable: true),
                    ma_thao_tac = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_moc_o", x => new { x.giao_xu_id, x.bang, x.ban_ghi_id, x.truong });
                });

            migrationBuilder.CreateTable(
                name: "thao_tac_da_nhan",
                columns: table => new
                {
                    giao_xu_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ma_thao_tac = table.Column<Guid>(type: "uuid", nullable: false),
                    nguon_goc_epoch = table.Column<Guid>(type: "uuid", nullable: true),
                    nguon_goc_so_thu_tu = table.Column<long>(type: "bigint", nullable: true),
                    ket_qua = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    phan_hoi = table.Column<string>(type: "jsonb", nullable: true),
                    nhan_luc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_thao_tac_da_nhan", x => new { x.giao_xu_id, x.ma_thao_tac });
                });

            migrationBuilder.CreateIndex(
                name: "ix_thao_tac_da_nhan_giao_xu_id_nguon_goc_epoch_nguon_goc_so_th~",
                table: "thao_tac_da_nhan",
                columns: new[] { "giao_xu_id", "nguon_goc_epoch", "nguon_goc_so_thu_tu" },
                unique: true,
                filter: "nguon_goc_epoch IS NOT NULL");

            // Lớp phòng thủ thứ hai, y hệt các bảng nghiệp vụ. moc_o không chứa giá trị nhưng
            // chứa BẢN ĐỒ dữ liệu (bảng nào, bản ghi nào, ô nào tồn tại và đổi lúc nào) — rò rỉ
            // nó cho giáo xứ khác vẫn là rò rỉ.
            foreach (var bang in new[] { "moc_o", "thao_tac_da_nhan" })
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
            foreach (var bang in new[] { "moc_o", "thao_tac_da_nhan" })
            {
                migrationBuilder.Sql($"DROP POLICY IF EXISTS loc_theo_giao_xu ON {bang};");
                migrationBuilder.Sql($"ALTER TABLE {bang} DISABLE ROW LEVEL SECURITY;");
            }

            migrationBuilder.DropTable(
                name: "moc_o");

            migrationBuilder.DropTable(
                name: "thao_tac_da_nhan");

            migrationBuilder.DropColumn(
                name: "cho_phep_bu_lai",
                table: "bo_dem_hieu_luc");

            migrationBuilder.DropColumn(
                name: "xoay_epoch_luc",
                table: "bo_dem_hieu_luc");
        }
    }
}
