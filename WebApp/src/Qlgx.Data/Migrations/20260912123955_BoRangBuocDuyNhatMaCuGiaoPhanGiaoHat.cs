using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Qlgx.Data.Migrations
{
    /// <inheritdoc />
    public partial class BoRangBuocDuyNhatMaCuGiaoPhanGiaoHat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_giao_phan_ma_giao_phan_cu",
                table: "giao_phan");

            migrationBuilder.DropIndex(
                name: "ix_giao_hat_ma_giao_hat_cu",
                table: "giao_hat");

            migrationBuilder.CreateIndex(
                name: "ix_giao_phan_ma_giao_phan_cu",
                table: "giao_phan",
                column: "ma_giao_phan_cu");

            migrationBuilder.CreateIndex(
                name: "ix_giao_hat_ma_giao_hat_cu",
                table: "giao_hat",
                column: "ma_giao_hat_cu");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_giao_phan_ma_giao_phan_cu",
                table: "giao_phan");

            migrationBuilder.DropIndex(
                name: "ix_giao_hat_ma_giao_hat_cu",
                table: "giao_hat");

            migrationBuilder.CreateIndex(
                name: "ix_giao_phan_ma_giao_phan_cu",
                table: "giao_phan",
                column: "ma_giao_phan_cu",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_giao_hat_ma_giao_hat_cu",
                table: "giao_hat",
                column: "ma_giao_hat_cu",
                unique: true);
        }
    }
}
