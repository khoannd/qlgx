using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Qlgx.Data.Migrations
{
    /// <inheritdoc />
    public partial class ThemChaMeIdChoGiaoDan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "cha_id",
                table: "giao_dan",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "me_id",
                table: "giao_dan",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_giao_dan_cha_id",
                table: "giao_dan",
                column: "cha_id");

            migrationBuilder.CreateIndex(
                name: "ix_giao_dan_me_id",
                table: "giao_dan",
                column: "me_id");

            migrationBuilder.AddForeignKey(
                name: "fk_giao_dan_giao_dan_cha_id",
                table: "giao_dan",
                column: "cha_id",
                principalTable: "giao_dan",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_giao_dan_giao_dan_me_id",
                table: "giao_dan",
                column: "me_id",
                principalTable: "giao_dan",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_giao_dan_giao_dan_cha_id",
                table: "giao_dan");

            migrationBuilder.DropForeignKey(
                name: "fk_giao_dan_giao_dan_me_id",
                table: "giao_dan");

            migrationBuilder.DropIndex(
                name: "ix_giao_dan_cha_id",
                table: "giao_dan");

            migrationBuilder.DropIndex(
                name: "ix_giao_dan_me_id",
                table: "giao_dan");

            migrationBuilder.DropColumn(
                name: "cha_id",
                table: "giao_dan");

            migrationBuilder.DropColumn(
                name: "me_id",
                table: "giao_dan");
        }
    }
}
