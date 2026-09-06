using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Qlgx.Data.Migrations
{
    /// <inheritdoc />
    public partial class ThemGiaoLyVaHoiDoan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "hoi_doan",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ma_hoi_doan_cu = table.Column<int>(type: "integer", nullable: false),
                    ten_hoi_doan = table.Column<string>(type: "text", nullable: false),
                    thanh_bon_mang = table.Column<string>(type: "text", nullable: true),
                    ngay_bon_mang = table.Column<DateOnly>(type: "date", nullable: true),
                    ngay_thanh_lap = table.Column<DateOnly>(type: "date", nullable: true),
                    ghi_chu = table.Column<string>(type: "text", nullable: true),
                    giao_xu_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    source_system = table.Column<string>(type: "text", nullable: true),
                    du_lieu_loi = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_hoi_doan", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "khoi_giao_ly",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ma_khoi_cu = table.Column<int>(type: "integer", nullable: false),
                    ten_khoi = table.Column<string>(type: "text", nullable: false),
                    nguoi_quan_ly_id = table.Column<Guid>(type: "uuid", nullable: true),
                    ghi_chu = table.Column<string>(type: "text", nullable: true),
                    giao_xu_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    source_system = table.Column<string>(type: "text", nullable: true),
                    du_lieu_loi = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_khoi_giao_ly", x => x.id);
                    table.ForeignKey(
                        name: "fk_khoi_giao_ly_giao_dan_nguoi_quan_ly_id",
                        column: x => x.nguoi_quan_ly_id,
                        principalTable: "giao_dan",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "chi_tiet_hoi_doan",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ma_chi_tiet_hoi_doan_cu = table.Column<int>(type: "integer", nullable: false),
                    hoi_doan_id = table.Column<Guid>(type: "uuid", nullable: false),
                    giao_dan_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ngay_vao_hoi_doan = table.Column<DateOnly>(type: "date", nullable: true),
                    ngay_ra_hoi_doan = table.Column<DateOnly>(type: "date", nullable: true),
                    vai_tro = table.Column<string>(type: "text", nullable: true),
                    giao_xu_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    source_system = table.Column<string>(type: "text", nullable: true),
                    du_lieu_loi = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_chi_tiet_hoi_doan", x => x.id);
                    table.ForeignKey(
                        name: "fk_chi_tiet_hoi_doan__giao_dan_giao_dan_id",
                        column: x => x.giao_dan_id,
                        principalTable: "giao_dan",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_chi_tiet_hoi_doan__hoi_doan_hoi_doan_id",
                        column: x => x.hoi_doan_id,
                        principalTable: "hoi_doan",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "lop_giao_ly",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ma_lop_cu = table.Column<int>(type: "integer", nullable: false),
                    ten_lop = table.Column<string>(type: "text", nullable: false),
                    khoi_giao_ly_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nam = table.Column<int>(type: "integer", nullable: true),
                    phong_hoc = table.Column<string>(type: "text", nullable: true),
                    ghi_chu = table.Column<string>(type: "text", nullable: true),
                    giao_xu_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    source_system = table.Column<string>(type: "text", nullable: true),
                    du_lieu_loi = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_lop_giao_ly", x => x.id);
                    table.ForeignKey(
                        name: "fk_lop_giao_ly_khoi_giao_ly_khoi_giao_ly_id",
                        column: x => x.khoi_giao_ly_id,
                        principalTable: "khoi_giao_ly",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "chi_tiet_lop_giao_ly",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    lop_giao_ly_id = table.Column<Guid>(type: "uuid", nullable: false),
                    giao_dan_id = table.Column<Guid>(type: "uuid", nullable: false),
                    so_thu_tu = table.Column<int>(type: "integer", nullable: true),
                    hoan_thanh = table.Column<bool>(type: "boolean", nullable: false),
                    ghi_chu_g_ly = table.Column<string>(type: "text", nullable: true),
                    giao_xu_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    source_system = table.Column<string>(type: "text", nullable: true),
                    du_lieu_loi = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_chi_tiet_lop_giao_ly", x => x.id);
                    table.ForeignKey(
                        name: "fk_chi_tiet_lop_giao_ly__giao_dan_giao_dan_id",
                        column: x => x.giao_dan_id,
                        principalTable: "giao_dan",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_chi_tiet_lop_giao_ly__lop_giao_ly_lop_giao_ly_id",
                        column: x => x.lop_giao_ly_id,
                        principalTable: "lop_giao_ly",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "giao_ly_vien",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    lop_giao_ly_id = table.Column<Guid>(type: "uuid", nullable: false),
                    giao_dan_id = table.Column<Guid>(type: "uuid", nullable: false),
                    giao_xu_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    source_system = table.Column<string>(type: "text", nullable: true),
                    du_lieu_loi = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_giao_ly_vien", x => x.id);
                    table.ForeignKey(
                        name: "fk_giao_ly_vien__lop_giao_ly_lop_giao_ly_id",
                        column: x => x.lop_giao_ly_id,
                        principalTable: "lop_giao_ly",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_giao_ly_vien_giao_dan_giao_dan_id",
                        column: x => x.giao_dan_id,
                        principalTable: "giao_dan",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_chi_tiet_hoi_doan_giao_dan_id",
                table: "chi_tiet_hoi_doan",
                column: "giao_dan_id");

            migrationBuilder.CreateIndex(
                name: "ix_chi_tiet_hoi_doan_giao_xu_id_ma_chi_tiet_hoi_doan_cu",
                table: "chi_tiet_hoi_doan",
                columns: new[] { "giao_xu_id", "ma_chi_tiet_hoi_doan_cu" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_chi_tiet_hoi_doan_hoi_doan_id",
                table: "chi_tiet_hoi_doan",
                column: "hoi_doan_id");

            migrationBuilder.CreateIndex(
                name: "ix_chi_tiet_lop_giao_ly_giao_dan_id",
                table: "chi_tiet_lop_giao_ly",
                column: "giao_dan_id");

            migrationBuilder.CreateIndex(
                name: "ix_chi_tiet_lop_giao_ly_giao_xu_id_lop_giao_ly_id_giao_dan_id",
                table: "chi_tiet_lop_giao_ly",
                columns: new[] { "giao_xu_id", "lop_giao_ly_id", "giao_dan_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_chi_tiet_lop_giao_ly_lop_giao_ly_id",
                table: "chi_tiet_lop_giao_ly",
                column: "lop_giao_ly_id");

            migrationBuilder.CreateIndex(
                name: "ix_giao_ly_vien_giao_dan_id",
                table: "giao_ly_vien",
                column: "giao_dan_id");

            migrationBuilder.CreateIndex(
                name: "ix_giao_ly_vien_giao_xu_id_lop_giao_ly_id_giao_dan_id",
                table: "giao_ly_vien",
                columns: new[] { "giao_xu_id", "lop_giao_ly_id", "giao_dan_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_giao_ly_vien_lop_giao_ly_id",
                table: "giao_ly_vien",
                column: "lop_giao_ly_id");

            migrationBuilder.CreateIndex(
                name: "ix_hoi_doan_giao_xu_id_ma_hoi_doan_cu",
                table: "hoi_doan",
                columns: new[] { "giao_xu_id", "ma_hoi_doan_cu" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_khoi_giao_ly_giao_xu_id_ma_khoi_cu",
                table: "khoi_giao_ly",
                columns: new[] { "giao_xu_id", "ma_khoi_cu" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_khoi_giao_ly_nguoi_quan_ly_id",
                table: "khoi_giao_ly",
                column: "nguoi_quan_ly_id");

            migrationBuilder.CreateIndex(
                name: "ix_lop_giao_ly_giao_xu_id_ma_lop_cu",
                table: "lop_giao_ly",
                columns: new[] { "giao_xu_id", "ma_lop_cu" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_lop_giao_ly_khoi_giao_ly_id",
                table: "lop_giao_ly",
                column: "khoi_giao_ly_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "chi_tiet_hoi_doan");

            migrationBuilder.DropTable(
                name: "chi_tiet_lop_giao_ly");

            migrationBuilder.DropTable(
                name: "giao_ly_vien");

            migrationBuilder.DropTable(
                name: "hoi_doan");

            migrationBuilder.DropTable(
                name: "lop_giao_ly");

            migrationBuilder.DropTable(
                name: "khoi_giao_ly");
        }
    }
}
