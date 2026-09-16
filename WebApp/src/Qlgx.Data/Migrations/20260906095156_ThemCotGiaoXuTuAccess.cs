using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Qlgx.Data.Migrations
{
    /// <inheritdoc />
    public partial class ThemCotGiaoXuTuAccess : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "hinh",
                table: "giao_xu",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "last_upload",
                table: "giao_xu",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ma_giao_hat_cu",
                table: "giao_xu",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "hinh",
                table: "giao_xu");

            migrationBuilder.DropColumn(
                name: "last_upload",
                table: "giao_xu");

            migrationBuilder.DropColumn(
                name: "ma_giao_hat_cu",
                table: "giao_xu");
        }
    }
}
