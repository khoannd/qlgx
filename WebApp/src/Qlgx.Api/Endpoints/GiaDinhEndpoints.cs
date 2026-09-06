using Qlgx.Api.Services;

namespace Qlgx.Api.Endpoints;

public static class GiaDinhEndpoints
{
    public static void MapGiaDinh(this IEndpointRouteBuilder app)
    {
        var nhom = app.MapGroup("/api/gia-dinh");

        nhom.MapGet("", async (GiaDinhService dichVu, Guid? giaoHoId,
            bool? chiKhongThongKe, CancellationToken ct) =>
            Results.Ok(await dichVu.LayDanhSach(giaoHoId, chiKhongThongKe ?? false, ct)));
    }
}
