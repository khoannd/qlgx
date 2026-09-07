using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Qlgx.Data.Migrations
{
    /// <inheritdoc />
    public partial class ThemNhapDuLieuJob : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "nhap_du_lieu_job",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    giao_xu_dich_id = table.Column<Guid>(type: "uuid", nullable: false),
                    trang_thai = table.Column<string>(type: "text", nullable: false),
                    chay_thu = table.Column<bool>(type: "boolean", nullable: false),
                    bao_cao_json = table.Column<string>(type: "text", nullable: true),
                    loi_thong_bao = table.Column<string>(type: "text", nullable: true),
                    bat_dau_luc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ket_thuc_luc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_nhap_du_lieu_job", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "nhap_du_lieu_job");
        }
    }
}
