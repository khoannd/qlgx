using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Qlgx.Data.Migrations
{
    /// <inheritdoc />
    public partial class ThemAnhDaiDienNhiPhan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "anh_dai_dien",
                table: "giao_dan",
                newName: "anh_dai_dien_loai_noi_dung");

            migrationBuilder.RenameColumn(
                name: "anh_dai_dien",
                table: "gia_dinh",
                newName: "anh_dai_dien_loai_noi_dung");

            migrationBuilder.AddColumn<byte[]>(
                name: "anh_dai_dien_du_lieu",
                table: "giao_dan",
                type: "bytea",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "anh_dai_dien_du_lieu",
                table: "gia_dinh",
                type: "bytea",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "anh_dai_dien_du_lieu",
                table: "giao_dan");

            migrationBuilder.DropColumn(
                name: "anh_dai_dien_du_lieu",
                table: "gia_dinh");

            migrationBuilder.RenameColumn(
                name: "anh_dai_dien_loai_noi_dung",
                table: "giao_dan",
                newName: "anh_dai_dien");

            migrationBuilder.RenameColumn(
                name: "anh_dai_dien_loai_noi_dung",
                table: "gia_dinh",
                newName: "anh_dai_dien");
        }
    }
}
