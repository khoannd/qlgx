using Qlgx.Api.Dtos;
using Qlgx.Api.Services;

namespace Qlgx.Api.Endpoints;

/// <summary>"Chuyển họ hàng loạt" — xem docs/superpowers/specs/man-hinh/cong-cu-du-lieu.md mục
/// 4 và ChuyenHoService (nguyên tắc an toàn bắt buộc: xem trước → xác nhận → một transaction).
/// </summary>
public static class ChuyenHoEndpoints
{
    public static void MapChuyenHo(this IEndpointRouteBuilder app)
    {
        var nhom = app.MapGroup("/api/cong-cu-du-lieu/chuyen-ho").RequireAuthorization();

        nhom.MapPost("/giao-dan/xem-truoc", async (ChuyenHoService dv, ChuyenHoGiaoDanRequest yc, CancellationToken ct) =>
        {
            if (yc.GiaoDanIds.Count == 0) return Results.BadRequest(new { thongBao = "Xin vui lòng chọn ít nhất 1 giáo dân để chuyển họ" });
            var kq = await dv.XemTruocGiaoDan(yc.GiaoDanIds, yc.GiaoHoDichId, ct);
            return kq is null ? Results.NotFound() : Results.Ok(kq);
        });

        nhom.MapPost("/giao-dan", async (ChuyenHoService dv, ChuyenHoGiaoDanRequest yc, CancellationToken ct) =>
        {
            if (yc.GiaoDanIds.Count == 0) return Results.BadRequest(new { thongBao = "Xin vui lòng chọn ít nhất 1 giáo dân để chuyển họ" });
            var kq = await dv.ChuyenHoGiaoDan(yc.GiaoDanIds, yc.GiaoHoDichId, ct);
            return kq is null ? Results.NotFound() : Results.Ok(kq);
        });

        nhom.MapPost("/gia-dinh/xem-truoc", async (ChuyenHoService dv, ChuyenHoGiaDinhRequest yc, CancellationToken ct) =>
        {
            if (yc.GiaDinhIds.Count == 0) return Results.BadRequest(new { thongBao = "Xin vui lòng chọn ít nhất 1 gia đình để chuyển họ" });
            var kq = await dv.XemTruocGiaDinh(yc.GiaDinhIds, yc.GiaoHoDichId, ct);
            return kq is null ? Results.NotFound() : Results.Ok(kq);
        });

        nhom.MapPost("/gia-dinh", async (ChuyenHoService dv, ChuyenHoGiaDinhRequest yc, CancellationToken ct) =>
        {
            if (yc.GiaDinhIds.Count == 0) return Results.BadRequest(new { thongBao = "Xin vui lòng chọn ít nhất 1 gia đình để chuyển họ" });
            var kq = await dv.ChuyenHoGiaDinh(yc.GiaDinhIds, yc.GiaoHoDichId, ct);
            return kq is null ? Results.NotFound() : Results.Ok(kq);
        });
    }
}
