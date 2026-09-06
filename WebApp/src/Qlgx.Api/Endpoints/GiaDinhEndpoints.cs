using Qlgx.Api.Dtos;
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

        nhom.MapGet("/{id:guid}", async (GiaDinhService dichVu, Guid id, CancellationToken ct) =>
            await dichVu.LayChiTiet(id, ct) is { } ct2 ? Results.Ok(ct2) : Results.NotFound());

        nhom.MapPut("/{id:guid}", async (GiaDinhService dichVu, Guid id,
            CapNhatGiaDinhRequest yeuCau, CancellationToken ct) =>
            await dichVu.CapNhat(id, yeuCau, ct) switch
            {
                null => Results.NotFound(),
                false => Results.Conflict(new
                {
                    thongBao = "Gia đình này vừa được người khác cập nhật. " +
                               "Hãy tải lại màn hình để xem thay đổi mới nhất rồi sửa lại."
                }),
                true => Results.Ok()
            });
    }
}
