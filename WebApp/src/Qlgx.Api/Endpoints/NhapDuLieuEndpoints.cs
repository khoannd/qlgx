using Qlgx.Api.Services;

namespace Qlgx.Api.Endpoints;

/// <summary>
/// Màn hình "Nhập dữ liệu Access" (VIEC-TIEP-THEO.md mục 2.4, xem NhapDuLieuService.cs cho
/// kiến trúc hai bước). Cả nhóm route đòi hỏi policy "QuanTriHeThong" — nhập nhầm vào giáo xứ
/// khác là thảm hoạ, đây là lớp phòng thủ DUY NHẤT (NhapDuLieuJob không có RLS bảo vệ, giống
/// GiaoXu/GiaoPhan/GiaoHat — xem QuanLyGiaoXuEndpoints.cs).
///
/// .DisableAntiforgery(): hai route có IFormFile — cùng lý do đã ghi ở GiaoDanEndpoints.cs (xác
/// thực bằng Bearer JWT, không có cookie phiên nên không có antiforgery nào để bảo vệ).
/// </summary>
public static class NhapDuLieuEndpoints
{
    public static void MapNhapDuLieu(this IEndpointRouteBuilder app)
    {
        var nhom = app.MapGroup("/api/quan-tri/nhap-du-lieu").RequireAuthorization("QuanTriHeThong");

        nhom.MapPost("/{giaoXuId:guid}/xem-truoc", async (NhapDuLieuService dv, Guid giaoXuId,
            IFormFile? tep, CancellationToken ct) =>
        {
            var loiTep = KiemTraTep(tep);
            if (loiTep is not null) return Results.BadRequest(new { thongBao = loiTep });

            var noiDung = await DocToanBo(tep!, ct);
            var (ketQua, loi) = await dv.XemTruoc(giaoXuId, noiDung, ct);
            return loi is not null ? Results.BadRequest(new { thongBao = loi }) : Results.Ok(ketQua);
        }).DisableAntiforgery();

        nhom.MapPost("/{giaoXuId:guid}/bat-dau", async (NhapDuLieuService dv, Guid giaoXuId,
            IFormFile? tep, bool? xacNhanGhiDe, CancellationToken ct) =>
        {
            var loiTep = KiemTraTep(tep);
            if (loiTep is not null) return Results.BadRequest(new { thongBao = loiTep });

            var noiDung = await DocToanBo(tep!, ct);
            var (ketQua, loi) = await dv.BatDauNhapThat(giaoXuId, noiDung, xacNhanGhiDe ?? false, ct);
            return loi is not null ? Results.BadRequest(new { thongBao = loi }) : Results.Ok(ketQua);
        }).DisableAntiforgery();

        nhom.MapGet("/trang-thai/{jobId:guid}", async (NhapDuLieuService dv, Guid jobId, CancellationToken ct) =>
            await dv.LayTrangThai(jobId, ct) is { } tt ? Results.Ok(tt) : Results.NotFound());
    }

    private static string? KiemTraTep(IFormFile? tep)
    {
        if (tep is null || tep.Length == 0)
            return "Chưa chọn gói dữ liệu để tải lên.";
        if (tep.Length > NhapDuLieuService.GioiHanDoDaiTep)
            return $"Tệp quá lớn. Kích thước tối đa cho phép là {NhapDuLieuService.GioiHanDoDaiTep / 1024 / 1024} MB.";
        return null;
    }

    private static async Task<byte[]> DocToanBo(IFormFile tep, CancellationToken ct)
    {
        await using var luong = tep.OpenReadStream();
        using var ms = new MemoryStream();
        await luong.CopyToAsync(ms, ct);
        return ms.ToArray();
    }
}
