using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Qlgx.Data.Migrations
{
    /// <inheritdoc />
    public partial class ThemPhanCapVaTraCuu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ten_giao_hat",
                table: "giao_xu");

            migrationBuilder.DropColumn(
                name: "ten_giao_phan",
                table: "giao_xu");

            migrationBuilder.AddColumn<Guid>(
                name: "giao_hat_id",
                table: "giao_xu",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "cau_hinh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ma_cau_hinh = table.Column<string>(type: "text", nullable: false),
                    gia_tri = table.Column<string>(type: "text", nullable: true),
                    mo_ta = table.Column<string>(type: "text", nullable: true),
                    giao_xu_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    source_system = table.Column<string>(type: "text", nullable: true),
                    du_lieu_loi = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cau_hinh", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "du_lieu_chung",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ma_du_lieu_chung_cu = table.Column<int>(type: "integer", nullable: false),
                    loai_du_lieu = table.Column<int>(type: "integer", nullable: false),
                    ma_du_lieu = table.Column<string>(type: "text", nullable: true),
                    du_lieu1 = table.Column<string>(type: "text", nullable: true),
                    du_lieu2 = table.Column<string>(type: "text", nullable: true),
                    giao_xu_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    source_system = table.Column<string>(type: "text", nullable: true),
                    du_lieu_loi = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_du_lieu_chung", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "giao_phan",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ma_giao_phan_cu = table.Column<int>(type: "integer", nullable: false),
                    ten_giao_phan = table.Column<string>(type: "text", nullable: false),
                    ghi_chu = table.Column<string>(type: "text", nullable: true),
                    ma_giao_phan_rieng = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_giao_phan", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tai_khoan",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ho_ten_nguoi_dung = table.Column<string>(type: "text", nullable: true),
                    ten_tai_khoan = table.Column<string>(type: "text", nullable: false),
                    mat_khau_bam = table.Column<string>(type: "text", nullable: true),
                    email = table.Column<string>(type: "text", nullable: true),
                    so_dien_thoai = table.Column<string>(type: "text", nullable: true),
                    loai_tai_khoan = table.Column<int>(type: "integer", nullable: false),
                    cau_hoi_goi_y = table.Column<string>(type: "text", nullable: true),
                    cau_tra_loi_goi_y = table.Column<string>(type: "text", nullable: true),
                    da_xoa = table.Column<bool>(type: "boolean", nullable: false),
                    giao_xu_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    source_system = table.Column<string>(type: "text", nullable: true),
                    du_lieu_loi = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tai_khoan", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ten_loai_tai_khoan",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ma_loai_tai_khoan_cu = table.Column<int>(type: "integer", nullable: false),
                    ten_loai = table.Column<string>(type: "text", nullable: true),
                    giao_xu_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    source_system = table.Column<string>(type: "text", nullable: true),
                    du_lieu_loi = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ten_loai_tai_khoan", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "vai_tro",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ma_vai_tro_cu = table.Column<int>(type: "integer", nullable: false),
                    value = table.Column<string>(type: "text", nullable: true),
                    giao_xu_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    source_system = table.Column<string>(type: "text", nullable: true),
                    du_lieu_loi = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_vai_tro", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "giao_hat",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ma_giao_hat_cu = table.Column<int>(type: "integer", nullable: false),
                    giao_phan_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ten_giao_hat = table.Column<string>(type: "text", nullable: false),
                    ghi_chu = table.Column<string>(type: "text", nullable: true),
                    ma_giao_hat_rieng = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_giao_hat", x => x.id);
                    table.ForeignKey(
                        name: "fk_giao_hat__giao_phan_giao_phan_id",
                        column: x => x.giao_phan_id,
                        principalTable: "giao_phan",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_giao_xu_giao_hat_id",
                table: "giao_xu",
                column: "giao_hat_id");

            migrationBuilder.CreateIndex(
                name: "ix_cau_hinh_giao_xu_id_ma_cau_hinh",
                table: "cau_hinh",
                columns: new[] { "giao_xu_id", "ma_cau_hinh" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_du_lieu_chung_giao_xu_id_loai_du_lieu",
                table: "du_lieu_chung",
                columns: new[] { "giao_xu_id", "loai_du_lieu" });

            migrationBuilder.CreateIndex(
                name: "ix_du_lieu_chung_giao_xu_id_ma_du_lieu_chung_cu",
                table: "du_lieu_chung",
                columns: new[] { "giao_xu_id", "ma_du_lieu_chung_cu" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_giao_hat_giao_phan_id",
                table: "giao_hat",
                column: "giao_phan_id");

            migrationBuilder.CreateIndex(
                name: "ix_giao_hat_ma_giao_hat_cu",
                table: "giao_hat",
                column: "ma_giao_hat_cu",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_giao_phan_ma_giao_phan_cu",
                table: "giao_phan",
                column: "ma_giao_phan_cu",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_tai_khoan_giao_xu_id_ten_tai_khoan",
                table: "tai_khoan",
                columns: new[] { "giao_xu_id", "ten_tai_khoan" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_ten_loai_tai_khoan_giao_xu_id_ma_loai_tai_khoan_cu",
                table: "ten_loai_tai_khoan",
                columns: new[] { "giao_xu_id", "ma_loai_tai_khoan_cu" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_vai_tro_giao_xu_id_ma_vai_tro_cu",
                table: "vai_tro",
                columns: new[] { "giao_xu_id", "ma_vai_tro_cu" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_giao_xu_giao_hat_giao_hat_id",
                table: "giao_xu",
                column: "giao_hat_id",
                principalTable: "giao_hat",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_giao_xu_giao_hat_giao_hat_id",
                table: "giao_xu");

            migrationBuilder.DropTable(
                name: "cau_hinh");

            migrationBuilder.DropTable(
                name: "du_lieu_chung");

            migrationBuilder.DropTable(
                name: "giao_hat");

            migrationBuilder.DropTable(
                name: "tai_khoan");

            migrationBuilder.DropTable(
                name: "ten_loai_tai_khoan");

            migrationBuilder.DropTable(
                name: "vai_tro");

            migrationBuilder.DropTable(
                name: "giao_phan");

            migrationBuilder.DropIndex(
                name: "ix_giao_xu_giao_hat_id",
                table: "giao_xu");

            migrationBuilder.DropColumn(
                name: "giao_hat_id",
                table: "giao_xu");

            migrationBuilder.AddColumn<string>(
                name: "ten_giao_hat",
                table: "giao_xu",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ten_giao_phan",
                table: "giao_xu",
                type: "text",
                nullable: true);
        }
    }
}
