using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Qlgx.Data.Migrations
{
    /// <inheritdoc />
    public partial class ThemMauInTuyChinh : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "mau_in_tuy_chinh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    giao_xu_id = table.Column<Guid>(type: "uuid", nullable: true),
                    ten_mau = table.Column<string>(type: "text", nullable: false),
                    noi_dung_html = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mau_in_tuy_chinh", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_mau_in_tuy_chinh_giao_xu",
                table: "mau_in_tuy_chinh",
                columns: new[] { "giao_xu_id", "ten_mau" },
                unique: true,
                filter: "giao_xu_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_mau_in_tuy_chinh_he_thong",
                table: "mau_in_tuy_chinh",
                column: "ten_mau",
                unique: true,
                filter: "giao_xu_id IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "mau_in_tuy_chinh");
        }
    }
}
