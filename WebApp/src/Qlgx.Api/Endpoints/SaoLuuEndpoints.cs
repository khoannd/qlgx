using System.Security.Claims;
using Qlgx.Api.Dtos;
using Qlgx.Api.Services;

namespace Qlgx.Api.Endpoints;

/// <summary>
/// Màn hình "Sao lưu &amp; Phục hồi" (xem docs/superpowers/specs/2026-09-13-qlgx-trien-khai-sao-luu-design.md
/// mục 8). CẢ NHÓM đòi policy "QuanTriHeThong": mọi thao tác ở đây tác động tới TOÀN MÁY CHỦ,
/// không riêng giáo xứ nào, nên quản trị viên của một giáo xứ không được phép chạm tới.
/// </summary>
public static class SaoLuuEndpoints
{
    public static void MapSaoLuu(this IEndpointRouteBuilder app)
    {
        var nhom = app.MapGroup("/api/sao-luu").RequireAuthorization("QuanTriHeThong");

        nhom.MapGet("/tinh-trang", async (SaoLuuService dv, CancellationToken ct) =>
            Results.Ok(await dv.LayTinhTrang(ct)));

        nhom.MapGet("/danh-sach", async (SaoLuuService dv, CancellationToken ct) =>
            Results.Ok(await dv.LayDanhSachBanSao(ct)));

        nhom.MapPost("/cong-viec", async (SaoLuuService dv, TaoCongViecYeuCau yc,
            ClaimsPrincipal nguoiDung, CancellationToken ct) =>
        {
            var (id, loi) = await dv.TaoCongViec(yc, DocIdNguoiDung(nguoiDung), ct);
            return loi is not null
                ? Results.BadRequest(new { thongBao = loi })
                : Results.Ok(new { id });
        });

        nhom.MapGet("/cong-viec", async (SaoLuuService dv, CancellationToken ct) =>
            Results.Ok(await dv.LayCongViecGanDay(20, ct)));

        nhom.MapGet("/cong-viec/{id:guid}", async (SaoLuuService dv, Guid id, CancellationToken ct) =>
            await dv.LayCongViec(id, ct) is { } cv ? Results.Ok(cv) : Results.NotFound());

        // Stream tep tu spool. API KHONG biet khoa R2 va KHONG goi restic — bo chay tren host
        // da giai nen san vao spool, o day chi con viec doc mot tep tren dia.
        //
        // Canh bao nghiep vu: tep nay la ban dump CHUA MA HOA chua toan bo du lieu giao dan.
        // Giao dien phai noi ro dieu do ngay tai nut tai (xem SaoLuuPage.tsx).
        nhom.MapGet("/tai-ve/{maCongViec:guid}", async (SaoLuuService dv, Guid maCongViec,
            CancellationToken ct) =>
        {
            var (duongDan, loi) = await dv.LayDuongDanTaiVe(maCongViec, ct);
            // Chon ma HTTP bang SWITCH TREN ENUM, khong bao gio so khop chuoi thong bao — thong
            // bao la van ban hien thi, co the doi cau chu bat cu luc nao; ma HTTP la hop dong API.
            if (loi is { } l)
            {
                var thongBao = SaoLuuService.ThongBaoLoiTaiVe(l);
                return l == LoiTaiVe.TepDaBiDon
                    ? Results.NotFound(new { thongBao })
                    : Results.BadRequest(new { thongBao });
            }
            if (duongDan is null) return Results.NotFound();

            return Results.File(duongDan, "application/octet-stream",
                fileDownloadName: Path.GetFileName(duongDan));
        });
    }

    private static Guid? DocIdNguoiDung(ClaimsPrincipal nguoiDung) =>
        Guid.TryParse(nguoiDung.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;
}
