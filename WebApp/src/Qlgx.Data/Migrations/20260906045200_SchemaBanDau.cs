using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Qlgx.Data.Migrations
{
    /// <inheritdoc />
    public partial class SchemaBanDau : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "giao_ho",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ma_giao_ho_cu = table.Column<int>(type: "integer", nullable: false),
                    ten_giao_ho = table.Column<string>(type: "text", nullable: false),
                    giao_ho_cha_id = table.Column<Guid>(type: "uuid", nullable: true),
                    da_xoa = table.Column<bool>(type: "boolean", nullable: false),
                    ma_nhan_dang = table.Column<string>(type: "text", nullable: true),
                    giao_xu_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    source_system = table.Column<string>(type: "text", nullable: true),
                    du_lieu_loi = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_giao_ho", x => x.id);
                    table.ForeignKey(
                        name: "fk_giao_ho_giao_ho_giao_ho_cha_id",
                        column: x => x.giao_ho_cha_id,
                        principalTable: "giao_ho",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "giao_xu",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ma_giao_xu_cu = table.Column<int>(type: "integer", nullable: false),
                    ma_giao_xu_rieng = table.Column<int>(type: "integer", nullable: true),
                    ten_giao_xu = table.Column<string>(type: "text", nullable: false),
                    ten_giao_hat = table.Column<string>(type: "text", nullable: true),
                    ten_giao_phan = table.Column<string>(type: "text", nullable: true),
                    dia_chi = table.Column<string>(type: "text", nullable: true),
                    dien_thoai = table.Column<string>(type: "text", nullable: true),
                    email = table.Column<string>(type: "text", nullable: true),
                    website = table.Column<string>(type: "text", nullable: true),
                    ghi_chu = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_giao_xu", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "hon_phoi",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ma_hon_phoi_cu = table.Column<int>(type: "integer", nullable: false),
                    ten_hon_phoi = table.Column<string>(type: "text", nullable: true),
                    so_hon_phoi = table.Column<string>(type: "text", nullable: true),
                    noi_hon_phoi = table.Column<string>(type: "text", nullable: true),
                    ngay_hon_phoi = table.Column<DateOnly>(type: "date", nullable: true),
                    linh_muc_chung = table.Column<string>(type: "text", nullable: true),
                    nguoi_chung1 = table.Column<string>(type: "text", nullable: true),
                    nguoi_chung2 = table.Column<string>(type: "text", nullable: true),
                    cach_thuc_hon_phoi = table.Column<string>(type: "text", nullable: true),
                    ghi_chu = table.Column<string>(type: "text", nullable: true),
                    ma_nhan_dang = table.Column<string>(type: "text", nullable: true),
                    giao_xu_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    source_system = table.Column<string>(type: "text", nullable: true),
                    du_lieu_loi = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_hon_phoi", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "gia_dinh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ma_gia_dinh_cu = table.Column<int>(type: "integer", nullable: false),
                    ma_gia_dinh_rieng = table.Column<string>(type: "text", nullable: true),
                    giao_ho_id = table.Column<Guid>(type: "uuid", nullable: true),
                    ten_gia_dinh = table.Column<string>(type: "text", nullable: true),
                    ghi_chu = table.Column<string>(type: "text", nullable: true),
                    dien_thoai = table.Column<string>(type: "text", nullable: true),
                    dia_chi = table.Column<string>(type: "text", nullable: true),
                    so_ho_khau = table.Column<string>(type: "text", nullable: true),
                    dien_gia_dinh = table.Column<string>(type: "text", nullable: true),
                    anh_dai_dien = table.Column<string>(type: "text", nullable: true),
                    da_xoa = table.Column<bool>(type: "boolean", nullable: false),
                    da_chuyen_xu = table.Column<bool>(type: "boolean", nullable: false),
                    ngay_chuyen = table.Column<DateOnly>(type: "date", nullable: true),
                    noi_chuyen = table.Column<string>(type: "text", nullable: true),
                    khong_thong_ke = table.Column<bool>(type: "boolean", nullable: false),
                    ma_nhan_dang = table.Column<string>(type: "text", nullable: true),
                    giao_xu_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    source_system = table.Column<string>(type: "text", nullable: true),
                    du_lieu_loi = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_gia_dinh", x => x.id);
                    table.ForeignKey(
                        name: "fk_gia_dinh__giao_ho_giao_ho_id",
                        column: x => x.giao_ho_id,
                        principalTable: "giao_ho",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "giao_dan",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ma_giao_dan_cu = table.Column<int>(type: "integer", nullable: false),
                    ho_ten = table.Column<string>(type: "text", nullable: false),
                    ten_thanh = table.Column<string>(type: "text", nullable: true),
                    phai = table.Column<string>(type: "text", nullable: true),
                    ngay_sinh = table.Column<DateOnly>(type: "date", nullable: true),
                    noi_sinh = table.Column<string>(type: "text", nullable: true),
                    cmnd = table.Column<string>(type: "text", nullable: true),
                    dan_toc = table.Column<string>(type: "text", nullable: true),
                    giao_ho_id = table.Column<Guid>(type: "uuid", nullable: true),
                    thuoc_giao_xu = table.Column<string>(type: "text", nullable: true),
                    thuoc_giao_phan = table.Column<string>(type: "text", nullable: true),
                    dia_chi = table.Column<string>(type: "text", nullable: true),
                    dien_thoai = table.Column<string>(type: "text", nullable: true),
                    email = table.Column<string>(type: "text", nullable: true),
                    anh_dai_dien = table.Column<string>(type: "text", nullable: true),
                    ho_ten_cha = table.Column<string>(type: "text", nullable: true),
                    ho_ten_me = table.Column<string>(type: "text", nullable: true),
                    so_rua_toi = table.Column<string>(type: "text", nullable: true),
                    ngay_rua_toi = table.Column<DateOnly>(type: "date", nullable: true),
                    noi_rua_toi = table.Column<string>(type: "text", nullable: true),
                    cha_rua_toi = table.Column<string>(type: "text", nullable: true),
                    nguoi_do_dau_rua_toi = table.Column<string>(type: "text", nullable: true),
                    so_ruoc_le = table.Column<string>(type: "text", nullable: true),
                    ngay_ruoc_le = table.Column<DateOnly>(type: "date", nullable: true),
                    noi_ruoc_le = table.Column<string>(type: "text", nullable: true),
                    cha_ruoc_le = table.Column<string>(type: "text", nullable: true),
                    so_them_suc = table.Column<string>(type: "text", nullable: true),
                    ngay_them_suc = table.Column<DateOnly>(type: "date", nullable: true),
                    noi_them_suc = table.Column<string>(type: "text", nullable: true),
                    cha_them_suc = table.Column<string>(type: "text", nullable: true),
                    nguoi_do_dau_them_suc = table.Column<string>(type: "text", nullable: true),
                    ngay_xuc_dau = table.Column<DateOnly>(type: "date", nullable: true),
                    nguoi_xuc_dau = table.Column<string>(type: "text", nullable: true),
                    tinh_trang_xuc_dau = table.Column<string>(type: "text", nullable: true),
                    ghi_chu_xuc_dau = table.Column<string>(type: "text", nullable: true),
                    ngay_bd1 = table.Column<DateOnly>(type: "date", nullable: true),
                    noi_bd1 = table.Column<string>(type: "text", nullable: true),
                    ngay_bd2 = table.Column<DateOnly>(type: "date", nullable: true),
                    noi_bd2 = table.Column<string>(type: "text", nullable: true),
                    ngay_th_vao_doi = table.Column<DateOnly>(type: "date", nullable: true),
                    noi_th_vao_doi = table.Column<string>(type: "text", nullable: true),
                    ngay_glhn1 = table.Column<DateOnly>(type: "date", nullable: true),
                    ngay_glhn2 = table.Column<DateOnly>(type: "date", nullable: true),
                    noi_glhn = table.Column<string>(type: "text", nullable: true),
                    nguoi_chung_nhan_glhn = table.Column<string>(type: "text", nullable: true),
                    xep_loai_glhn = table.Column<string>(type: "text", nullable: true),
                    trinh_do_van_hoa = table.Column<string>(type: "text", nullable: true),
                    trinh_do_chuyen_mon = table.Column<string>(type: "text", nullable: true),
                    biet_ngoai_ngu = table.Column<string>(type: "text", nullable: true),
                    nghe_nghiep = table.Column<string>(type: "text", nullable: true),
                    con_hoc = table.Column<bool>(type: "boolean", nullable: false),
                    da_co_gia_dinh = table.Column<bool>(type: "boolean", nullable: false),
                    tan_tong = table.Column<bool>(type: "boolean", nullable: false),
                    khong_thong_ke = table.Column<bool>(type: "boolean", nullable: false),
                    qua_doi = table.Column<bool>(type: "boolean", nullable: false),
                    ngay_qua_doi = table.Column<DateOnly>(type: "date", nullable: true),
                    noi_qua_doi = table.Column<string>(type: "text", nullable: true),
                    so_an_tang = table.Column<string>(type: "text", nullable: true),
                    noi_an_tang = table.Column<string>(type: "text", nullable: true),
                    da_xoa = table.Column<bool>(type: "boolean", nullable: false),
                    ghi_chu = table.Column<string>(type: "text", nullable: true),
                    ma_nhan_dang = table.Column<string>(type: "text", nullable: true),
                    giao_xu_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    source_system = table.Column<string>(type: "text", nullable: true),
                    du_lieu_loi = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_giao_dan", x => x.id);
                    table.ForeignKey(
                        name: "fk_giao_dan__giao_ho_giao_ho_id",
                        column: x => x.giao_ho_id,
                        principalTable: "giao_ho",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "giao_dan_hon_phoi",
                columns: table => new
                {
                    giao_dan_id = table.Column<Guid>(type: "uuid", nullable: false),
                    hon_phoi_id = table.Column<Guid>(type: "uuid", nullable: false),
                    so_thu_tu = table.Column<int>(type: "integer", nullable: false),
                    giao_xu_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_giao_dan_hon_phoi", x => new { x.giao_dan_id, x.hon_phoi_id });
                    table.ForeignKey(
                        name: "fk_giao_dan_hon_phoi__hon_phoi_hon_phoi_id",
                        column: x => x.hon_phoi_id,
                        principalTable: "hon_phoi",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_giao_dan_hon_phoi_giao_dan_giao_dan_id",
                        column: x => x.giao_dan_id,
                        principalTable: "giao_dan",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "thanh_vien_gia_dinh",
                columns: table => new
                {
                    gia_dinh_id = table.Column<Guid>(type: "uuid", nullable: false),
                    giao_dan_id = table.Column<Guid>(type: "uuid", nullable: false),
                    vai_tro = table.Column<int>(type: "integer", nullable: false),
                    chu_ho = table.Column<bool>(type: "boolean", nullable: false),
                    giao_xu_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_thanh_vien_gia_dinh", x => new { x.gia_dinh_id, x.giao_dan_id, x.vai_tro });
                    table.ForeignKey(
                        name: "fk_thanh_vien_gia_dinh_gia_dinh_gia_dinh_id",
                        column: x => x.gia_dinh_id,
                        principalTable: "gia_dinh",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_thanh_vien_gia_dinh_giao_dan_giao_dan_id",
                        column: x => x.giao_dan_id,
                        principalTable: "giao_dan",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_gia_dinh_giao_ho_id",
                table: "gia_dinh",
                column: "giao_ho_id");

            migrationBuilder.CreateIndex(
                name: "ix_gia_dinh_giao_xu_id_da_xoa",
                table: "gia_dinh",
                columns: new[] { "giao_xu_id", "da_xoa" });

            migrationBuilder.CreateIndex(
                name: "ix_gia_dinh_giao_xu_id_ma_gia_dinh_cu",
                table: "gia_dinh",
                columns: new[] { "giao_xu_id", "ma_gia_dinh_cu" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_giao_dan_giao_ho_id",
                table: "giao_dan",
                column: "giao_ho_id");

            migrationBuilder.CreateIndex(
                name: "ix_giao_dan_giao_xu_id_da_xoa",
                table: "giao_dan",
                columns: new[] { "giao_xu_id", "da_xoa" });

            migrationBuilder.CreateIndex(
                name: "ix_giao_dan_giao_xu_id_ho_ten",
                table: "giao_dan",
                columns: new[] { "giao_xu_id", "ho_ten" });

            migrationBuilder.CreateIndex(
                name: "ix_giao_dan_giao_xu_id_ma_giao_dan_cu",
                table: "giao_dan",
                columns: new[] { "giao_xu_id", "ma_giao_dan_cu" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_giao_dan_hon_phoi_giao_xu_id",
                table: "giao_dan_hon_phoi",
                column: "giao_xu_id");

            migrationBuilder.CreateIndex(
                name: "ix_giao_dan_hon_phoi_hon_phoi_id",
                table: "giao_dan_hon_phoi",
                column: "hon_phoi_id");

            migrationBuilder.CreateIndex(
                name: "ix_giao_ho_giao_ho_cha_id",
                table: "giao_ho",
                column: "giao_ho_cha_id");

            migrationBuilder.CreateIndex(
                name: "ix_giao_ho_giao_xu_id_ma_giao_ho_cu",
                table: "giao_ho",
                columns: new[] { "giao_xu_id", "ma_giao_ho_cu" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_hon_phoi_giao_xu_id",
                table: "hon_phoi",
                column: "giao_xu_id");

            migrationBuilder.CreateIndex(
                name: "ix_hon_phoi_giao_xu_id_ma_hon_phoi_cu",
                table: "hon_phoi",
                columns: new[] { "giao_xu_id", "ma_hon_phoi_cu" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_thanh_vien_gia_dinh_giao_dan_id",
                table: "thanh_vien_gia_dinh",
                column: "giao_dan_id");

            migrationBuilder.CreateIndex(
                name: "ix_thanh_vien_gia_dinh_giao_xu_id_gia_dinh_id",
                table: "thanh_vien_gia_dinh",
                columns: new[] { "giao_xu_id", "gia_dinh_id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "giao_dan_hon_phoi");

            migrationBuilder.DropTable(
                name: "giao_xu");

            migrationBuilder.DropTable(
                name: "thanh_vien_gia_dinh");

            migrationBuilder.DropTable(
                name: "hon_phoi");

            migrationBuilder.DropTable(
                name: "gia_dinh");

            migrationBuilder.DropTable(
                name: "giao_dan");

            migrationBuilder.DropTable(
                name: "giao_ho");
        }
    }
}
