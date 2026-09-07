using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Qlgx.Data.Migrations
{
    /// <inheritdoc />
    public partial class ThemRangBuocMotChongMotVo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ux_thanh_vien_gia_dinh_mot_chong_mot_vo",
                table: "thanh_vien_gia_dinh",
                columns: new[] { "gia_dinh_id", "vai_tro" },
                unique: true,
                filter: "vai_tro IN (0, 1)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ux_thanh_vien_gia_dinh_mot_chong_mot_vo",
                table: "thanh_vien_gia_dinh");
        }
    }
}
