using Qlgx.Api.Dtos;
using Qlgx.Api.Services;

namespace Qlgx.Api.Endpoints;

/// <summary>"Tìm và thay thế" — xem TimThayTheService và
/// docs/superpowers/specs/man-hinh/tim-thay-the.md.</summary>
public static class TimThayTheEndpoints
{
    public static void MapTimThayThe(this IEndpointRouteBuilder app)
    {
        var nhom = app.MapGroup("/api/cong-cu-du-lieu/tim-thay-the").RequireAuthorization();

        nhom.MapPost("/xem-truoc", async (TimThayTheService dv, TimThayTheRequest yc, CancellationToken ct) =>
        {
            var loi = KiemTraYeuCau(yc);
            if (loi is not null) return Results.BadRequest(new { thongBao = loi });
            var soLuong = await dv.DemKhop(yc.Bang, yc.Truong, yc.GiaTriTim, ct);
            return Results.Ok(new TimThayTheXemTruocKetQua(soLuong));
        });

        nhom.MapPost("", async (TimThayTheService dv, TimThayTheRequest yc, CancellationToken ct) =>
        {
            var loi = KiemTraYeuCau(yc);
            if (loi is not null) return Results.BadRequest(new { thongBao = loi });
            var soLuong = await dv.ThayThe(yc.Bang, yc.Truong, yc.GiaTriTim, yc.GiaTriThay, ct);
            return Results.Ok(new TimThayTheKetQua(soLuong));
        });
    }

    // Nguyên văn thông báo lỗi desktop (frmReplace.cs:97-101: "Hãy nhập giá trị cần tìm").
    private static string? KiemTraYeuCau(TimThayTheRequest yc)
    {
        if (string.IsNullOrEmpty(yc.GiaTriTim)) return "Hãy nhập giá trị cần tìm";
        if (!TimThayTheService.TruongHopLe(yc.Bang, yc.Truong)) return "Trường không hợp lệ cho bảng đã chọn";
        return null;
    }
}
