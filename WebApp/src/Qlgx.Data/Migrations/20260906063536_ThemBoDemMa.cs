using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Qlgx.Data.Migrations
{
    /// <inheritdoc />
    public partial class ThemBoDemMa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "bo_dem_ma",
                columns: table => new
                {
                    giao_xu_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ten_bang = table.Column<string>(type: "text", nullable: false),
                    gia_tri_cuoi = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_bo_dem_ma", x => new { x.giao_xu_id, x.ten_bang });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "bo_dem_ma");
        }
    }
}
