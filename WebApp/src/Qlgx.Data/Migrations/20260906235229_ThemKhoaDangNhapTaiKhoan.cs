using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Qlgx.Data.Migrations
{
    /// <inheritdoc />
    public partial class ThemKhoaDangNhapTaiKhoan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "khoa_dang_nhap_den_luc",
                table: "tai_khoan",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "so_lan_dang_nhap_sai_lien_tiep",
                table: "tai_khoan",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "khoa_dang_nhap_den_luc",
                table: "tai_khoan");

            migrationBuilder.DropColumn(
                name: "so_lan_dang_nhap_sai_lien_tiep",
                table: "tai_khoan");
        }
    }
}
