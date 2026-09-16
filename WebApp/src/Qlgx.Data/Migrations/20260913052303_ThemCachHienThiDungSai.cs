using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Qlgx.Data.Migrations
{
    /// <inheritdoc />
    /// <summary>
    /// Bảng câu chữ tuỳ chỉnh cho các biến in đúng/sai (xem CachHienThiDungSai.cs và
    /// quan-ly-mau-in.md). Dựng ĐÚNG KHUÔN bảng mau_in_tuy_chinh
    /// (20260913003611_ThemMauInTuyChinh): giao_xu_id cho phép NULL với nghĩa "dòng cấp hệ
    /// thống", và ràng buộc duy nhất đặt bằng HAI chỉ mục MỘT PHẦN thay vì một UNIQUE thường
    /// (PostgreSQL coi hai NULL là khác nhau trong chỉ mục duy nhất, nên UNIQUE thường KHÔNG
    /// chặn được hai dòng hệ thống trùng ten_bien).
    ///
    /// CỐ Ý KHÔNG bật Row-Level Security, giống hệt mau_in_tuy_chinh và vì đúng cùng một lý do
    /// kỹ thuật: policy chung của dự án (20260906210934_BatRlsChoBangTheoGiaoXu) là
    /// <c>giao_xu_id::text = current_setting('app.giao_xu_id', true)</c>, mà với dòng cấp hệ
    /// thống thì giao_xu_id IS NULL nên vế so sánh trả về NULL — tức KHÔNG khớp — và policy đó sẽ
    /// giấu mất chính những dòng hệ thống cần mọi giáo xứ đọc được. Hai bảng này vì vậy tự lọc
    /// TAY, tường minh, ở tầng service (CachHienThiDungSaiService, InAnService.LayBangCachHienThi,
    /// MauInService) — xem ghi chú dài ở CachHienThiDungSai.cs.
    ///
    /// Nếu sau này muốn có thêm lớp phòng thủ CSDL cho nhóm bảng "có dòng cấp hệ thống", hãy làm
    /// cho CẢ HAI bảng trong CÙNG một migration với một policy nhận biết NULL
    /// (<c>USING (giao_xu_id IS NULL OR giao_xu_id::text = current_setting(...))</c>) — đừng bật
    /// cho mỗi bảng này, vì hai bảng song sinh lệch nhau về bảo vệ là một cái bẫy bảo trì.
    /// </summary>
    public partial class ThemCachHienThiDungSai : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "cach_hien_thi_dung_sai",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    giao_xu_id = table.Column<Guid>(type: "uuid", nullable: true),
                    ten_bien = table.Column<string>(type: "text", nullable: false),
                    khi_dung = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    khi_sai = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cach_hien_thi_dung_sai", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_cach_hien_thi_dung_sai_giao_xu",
                table: "cach_hien_thi_dung_sai",
                columns: new[] { "giao_xu_id", "ten_bien" },
                unique: true,
                filter: "giao_xu_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_cach_hien_thi_dung_sai_he_thong",
                table: "cach_hien_thi_dung_sai",
                column: "ten_bien",
                unique: true,
                filter: "giao_xu_id IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "cach_hien_thi_dung_sai");
        }
    }
}
