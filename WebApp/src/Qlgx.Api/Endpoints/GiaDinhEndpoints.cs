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
                KetQuaCapNhatGiaDinh.KhongTimThay => Results.NotFound(),
                KetQuaCapNhatGiaDinh.KhongTheGanHonPhoiMoCoi => Results.BadRequest(new
                {
                    thongBao = "Gia đình chưa có chồng hoặc vợ nên không thể gắn hôn phối. " +
                               "Hãy thêm chồng hoặc vợ vào gia đình trước rồi thử lại."
                }),
                KetQuaCapNhatGiaDinh.DungPhienBanGiaDinh => Results.Conflict(new
                {
                    thongBao = "Gia đình này vừa được người khác cập nhật. " +
                               "Hãy tải lại màn hình để xem thay đổi mới nhất rồi sửa lại."
                }),
                KetQuaCapNhatGiaDinh.DungPhienBanHonPhoi => Results.Conflict(new
                {
                    thongBao = "Khối hôn phối này vừa được người khác cập nhật. " +
                               "Hãy tải lại màn hình để xem thay đổi mới nhất rồi sửa lại."
                }),
                _ => Results.Ok()
            });
    }
}
