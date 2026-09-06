using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Qlgx.Data.Migrations
{
    /// <inheritdoc />
    public partial class ThemBiTichVaChuyenXu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "chuyen_xu",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ma_chuyen_xu_cu = table.Column<int>(type: "integer", nullable: false),
                    giao_dan_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ngay_chuyen = table.Column<DateOnly>(type: "date", nullable: true),
                    noi_chuyen = table.Column<string>(type: "text", nullable: true),
                    loai_chuyen = table.Column<int>(type: "integer", nullable: false),
                    ghi_chu_chuyen = table.Column<string>(type: "text", nullable: true),
                    giao_xu_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    source_system = table.Column<string>(type: "text", nullable: true),
                    du_lieu_loi = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_chuyen_xu", x => x.id);
                    table.ForeignKey(
                        name: "fk_chuyen_xu__giao_dan_giao_dan_id",
                        column: x => x.giao_dan_id,
                        principalTable: "giao_dan",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "dot_bi_tich",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ma_dot_bi_tich_cu = table.Column<int>(type: "integer", nullable: false),
                    ngay_bi_tich = table.Column<DateOnly>(type: "date", nullable: true),
                    mo_ta = table.Column<string>(type: "text", nullable: true),
                    linh_muc = table.Column<string>(type: "text", nullable: true),
                    loai_bi_tich = table.Column<int>(type: "integer", nullable: false),
                    noi_bi_tich = table.Column<string>(type: "text", nullable: true),
                    giao_xu_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    source_system = table.Column<string>(type: "text", nullable: true),
                    du_lieu_loi = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_dot_bi_tich", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "linh_muc",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ma_linh_muc_cu = table.Column<int>(type: "integer", nullable: false),
                    ten_thanh = table.Column<string>(type: "text", nullable: true),
                    ho_ten = table.Column<string>(type: "text", nullable: false),
                    ngay_sinh = table.Column<DateOnly>(type: "date", nullable: true),
                    chuc_vu = table.Column<string>(type: "text", nullable: true),
                    tu_ngay = table.Column<DateOnly>(type: "date", nullable: true),
                    den_ngay = table.Column<DateOnly>(type: "date", nullable: true),
                    ghi_chu = table.Column<string>(type: "text", nullable: true),
                    dien_thoai = table.Column<string>(type: "text", nullable: true),
                    email = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("pk_linh_muc", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rao_hon_phoi",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ma_rao_hon_phoi_cu = table.Column<int>(type: "integer", nullable: false),
                    ten_rao_hon_phoi = table.Column<string>(type: "text", nullable: true),
                    giao_dan1_id = table.Column<Guid>(type: "uuid", nullable: true),
                    giao_dan2_id = table.Column<Guid>(type: "uuid", nullable: true),
                    ngay_rao_lan1 = table.Column<DateOnly>(type: "date", nullable: true),
                    ngay_rao_lan2 = table.Column<DateOnly>(type: "date", nullable: true),
                    ngay_rao_lan3 = table.Column<DateOnly>(type: "date", nullable: true),
                    giao_xu1 = table.Column<string>(type: "text", nullable: true),
                    giao_phan1 = table.Column<string>(type: "text", nullable: true),
                    giao_xu_truoc1 = table.Column<string>(type: "text", nullable: true),
                    giao_phan_truoc1 = table.Column<string>(type: "text", nullable: true),
                    giao_xu2 = table.Column<string>(type: "text", nullable: true),
                    giao_phan2 = table.Column<string>(type: "text", nullable: true),
                    giao_xu_truoc2 = table.Column<string>(type: "text", nullable: true),
                    giao_phan_truoc2 = table.Column<string>(type: "text", nullable: true),
                    linh_muc_nhan = table.Column<string>(type: "text", nullable: true),
                    giao_xu_nhan = table.Column<string>(type: "text", nullable: true),
                    ghi_chu = table.Column<string>(type: "text", nullable: true),
                    tam1 = table.Column<string>(type: "text", nullable: true),
                    tam2 = table.Column<string>(type: "text", nullable: true),
                    tam3 = table.Column<string>(type: "text", nullable: true),
                    giao_xu_nq1 = table.Column<string>(type: "text", nullable: true),
                    giao_phan_nq1 = table.Column<string>(type: "text", nullable: true),
                    giao_xu_nq2 = table.Column<string>(type: "text", nullable: true),
                    giao_phan_nq2 = table.Column<string>(type: "text", nullable: true),
                    giao_xu_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    source_system = table.Column<string>(type: "text", nullable: true),
                    du_lieu_loi = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_rao_hon_phoi", x => x.id);
                    table.ForeignKey(
                        name: "fk_rao_hon_phoi_giao_dan_giao_dan1_id",
                        column: x => x.giao_dan1_id,
                        principalTable: "giao_dan",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_rao_hon_phoi_giao_dan_giao_dan2_id",
                        column: x => x.giao_dan2_id,
                        principalTable: "giao_dan",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "tan_hien",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ma_tan_hien_cu = table.Column<int>(type: "integer", nullable: false),
                    giao_dan_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ngay_bat_dau = table.Column<DateOnly>(type: "date", nullable: true),
                    chuc_vu = table.Column<string>(type: "text", nullable: true),
                    noi_tu = table.Column<string>(type: "text", nullable: true),
                    dong_tu = table.Column<string>(type: "text", nullable: true),
                    noi_phuc_vu = table.Column<string>(type: "text", nullable: true),
                    dia_chi_phuc_vu = table.Column<string>(type: "text", nullable: true),
                    dien_thoai_phuc_vu = table.Column<string>(type: "text", nullable: true),
                    email_phuc_vu = table.Column<string>(type: "text", nullable: true),
                    ghi_chu = table.Column<string>(type: "text", nullable: true),
                    da_hoi_tuc = table.Column<bool>(type: "boolean", nullable: false),
                    ngay_vao_dcv = table.Column<DateOnly>(type: "date", nullable: true),
                    ngay_vao_nha_thu = table.Column<DateOnly>(type: "date", nullable: true),
                    ngay_vao_nha_tap = table.Column<DateOnly>(type: "date", nullable: true),
                    ngay_vao_khan_lan_dau = table.Column<DateOnly>(type: "date", nullable: true),
                    ngay_vao_khan_tron_doi = table.Column<DateOnly>(type: "date", nullable: true),
                    ngay_pho_te = table.Column<DateOnly>(type: "date", nullable: true),
                    ngay_thu_phong_lm = table.Column<DateOnly>(type: "date", nullable: true),
                    ngay_bon_mang = table.Column<DateOnly>(type: "date", nullable: true),
                    giao_xu_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    source_system = table.Column<string>(type: "text", nullable: true),
                    du_lieu_loi = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tan_hien", x => x.id);
                    table.ForeignKey(
                        name: "fk_tan_hien_giao_dan_giao_dan_id",
                        column: x => x.giao_dan_id,
                        principalTable: "giao_dan",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "bi_tich_chi_tiet",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    dot_bi_tich_id = table.Column<Guid>(type: "uuid", nullable: false),
                    giao_dan_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("pk_bi_tich_chi_tiet", x => x.id);
                    table.ForeignKey(
                        name: "fk_bi_tich_chi_tiet__dot_bi_tich_dot_bi_tich_id",
                        column: x => x.dot_bi_tich_id,
                        principalTable: "dot_bi_tich",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_bi_tich_chi_tiet__giao_dan_giao_dan_id",
                        column: x => x.giao_dan_id,
                        principalTable: "giao_dan",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_bi_tich_chi_tiet_dot_bi_tich_id",
                table: "bi_tich_chi_tiet",
                column: "dot_bi_tich_id");

            migrationBuilder.CreateIndex(
                name: "ix_bi_tich_chi_tiet_giao_dan_id",
                table: "bi_tich_chi_tiet",
                column: "giao_dan_id");

            migrationBuilder.CreateIndex(
                name: "ix_bi_tich_chi_tiet_giao_xu_id_dot_bi_tich_id_giao_dan_id",
                table: "bi_tich_chi_tiet",
                columns: new[] { "giao_xu_id", "dot_bi_tich_id", "giao_dan_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_chuyen_xu_giao_dan_id",
                table: "chuyen_xu",
                column: "giao_dan_id");

            migrationBuilder.CreateIndex(
                name: "ix_chuyen_xu_giao_xu_id_ma_chuyen_xu_cu",
                table: "chuyen_xu",
                columns: new[] { "giao_xu_id", "ma_chuyen_xu_cu" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_dot_bi_tich_giao_xu_id_ma_dot_bi_tich_cu",
                table: "dot_bi_tich",
                columns: new[] { "giao_xu_id", "ma_dot_bi_tich_cu" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_linh_muc_giao_xu_id_ma_linh_muc_cu",
                table: "linh_muc",
                columns: new[] { "giao_xu_id", "ma_linh_muc_cu" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_rao_hon_phoi_giao_dan1_id",
                table: "rao_hon_phoi",
                column: "giao_dan1_id");

            migrationBuilder.CreateIndex(
                name: "ix_rao_hon_phoi_giao_dan2_id",
                table: "rao_hon_phoi",
                column: "giao_dan2_id");

            migrationBuilder.CreateIndex(
                name: "ix_rao_hon_phoi_giao_xu_id_ma_rao_hon_phoi_cu",
                table: "rao_hon_phoi",
                columns: new[] { "giao_xu_id", "ma_rao_hon_phoi_cu" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_tan_hien_giao_dan_id",
                table: "tan_hien",
                column: "giao_dan_id");

            migrationBuilder.CreateIndex(
                name: "ix_tan_hien_giao_xu_id_ma_tan_hien_cu",
                table: "tan_hien",
                columns: new[] { "giao_xu_id", "ma_tan_hien_cu" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "bi_tich_chi_tiet");

            migrationBuilder.DropTable(
                name: "chuyen_xu");

            migrationBuilder.DropTable(
                name: "linh_muc");

            migrationBuilder.DropTable(
                name: "rao_hon_phoi");

            migrationBuilder.DropTable(
                name: "tan_hien");

            migrationBuilder.DropTable(
                name: "dot_bi_tich");
        }
    }
}
