using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Qlgx.Data.Migrations
{
    /// <inheritdoc />
    public partial class ThemBangSaoLuu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ban_sao_luu",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    thoi_diem = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    nhan = table.Column<string>(type: "text", nullable: true),
                    kich_thuoc_byte = table.Column<long>(type: "bigint", nullable: false),
                    so_giao_dan = table.Column<int>(type: "integer", nullable: false),
                    so_gia_dinh = table.Column<int>(type: "integer", nullable: false),
                    nguon = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ban_sao_luu", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "cong_viec_sao_luu",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    loai = table.Column<string>(type: "text", nullable: false),
                    trang_thai = table.Column<string>(type: "text", nullable: false),
                    tham_so_json = table.Column<string>(type: "text", nullable: true),
                    nhat_ky = table.Column<string>(type: "text", nullable: true),
                    buoc_hien_tai = table.Column<string>(type: "text", nullable: true),
                    nguoi_tao_id = table.Column<Guid>(type: "uuid", nullable: true),
                    tao_luc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    bat_dau_luc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ket_thuc_luc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cong_viec_sao_luu", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "trang_thai_sao_luu",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    sao_luu_gan_nhat = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    dien_tap_gan_nhat = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    dien_tap_dat = table.Column<bool>(type: "boolean", nullable: false),
                    loi_gan_nhat = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_trang_thai_sao_luu", x => x.id);
                });

            // Bang trang thai chi duoc phep co DUNG MOT dong — rang buoc o tang CSDL thay vi
            // dua vao ky luat cua ma goi, vi ca API lan script bash tren host deu ghi vao day.
            migrationBuilder.Sql(
                "ALTER TABLE trang_thai_sao_luu ADD CONSTRAINT ck_trang_thai_sao_luu_mot_dong CHECK (id = 1);");

            // Bo chay lay cong viec bang: WHERE trang_thai='cho' ORDER BY tao_luc LIMIT 1
            // FOR UPDATE SKIP LOCKED — chi muc nay phuc vu dung truy van do.
            migrationBuilder.Sql(
                "CREATE INDEX ix_cong_viec_sao_luu_cho ON cong_viec_sao_luu (tao_luc) " +
                "WHERE trang_thai = 'cho';");

            // Dong trang thai duy nhat phai TON TAI ngay tu dau, de bo chay chi can UPDATE
            // (khong phai UPSERT) va giao dien luon doc duoc mot dong.
            migrationBuilder.Sql(
                "INSERT INTO trang_thai_sao_luu (id, dien_tap_dat) VALUES (1, false) " +
                "ON CONFLICT DO NOTHING;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX IF EXISTS ix_cong_viec_sao_luu_cho;");

            migrationBuilder.DropTable(
                name: "ban_sao_luu");

            migrationBuilder.DropTable(
                name: "cong_viec_sao_luu");

            migrationBuilder.DropTable(
                name: "trang_thai_sao_luu");
        }
    }
}
