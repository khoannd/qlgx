using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Qlgx.Data.Migrations
{
    /// <inheritdoc />
    public partial class ThemBoDemHieuLuc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "bo_dem_hieu_luc",
                columns: table => new
                {
                    giao_xu_id = table.Column<Guid>(type: "uuid", nullable: false),
                    so_tiep_theo = table.Column<long>(type: "bigint", nullable: false),
                    epoch = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_bo_dem_hieu_luc", x => x.giao_xu_id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "bo_dem_hieu_luc");
        }
    }
}
