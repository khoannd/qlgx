using Qlgx.Api.Dtos;
using Qlgx.Api.Services;

namespace Qlgx.Api.Endpoints;

public static class GiaoDanEndpoints
{
    public static void MapGiaoDan(this IEndpointRouteBuilder app)
    {
        var nhom = app.MapGroup("/api/giao-dan");

        nhom.MapGet("", async (GiaoDanService dv, Guid? giaoHoId, bool? chiKhongThongKe,
            CancellationToken ct) =>
            Results.Ok(await dv.LayDanhSach(giaoHoId, chiKhongThongKe ?? false, ct)));

        nhom.MapGet("/{id:guid}", async (GiaoDanService dv, Guid id, CancellationToken ct) =>
            await dv.LayChiTiet(id, ct) is { } chiTiet ? Results.Ok(chiTiet) : Results.NotFound());

        nhom.MapPut("/{id:guid}", async (GiaoDanService dv, Guid id,
            CapNhatGiaoDanRequest yeuCau, CancellationToken ct) =>
            await dv.CapNhat(id, yeuCau, ct) switch
            {
                null => Results.NotFound(),
                false => Results.Conflict(new
                {
                    thongBao = "Giáo dân này vừa được người khác cập nhật. " +
                               "Hãy tải lại màn hình để xem thay đổi mới nhất rồi sửa lại."
                }),
                true => Results.Ok()
            });

        // Lưới thành viên trong form gia đình dùng chung bộ cột với danh sách giáo dân
        app.MapGet("/api/gia-dinh/{id:guid}/thanh-vien",
            async (GiaoDanService dv, Guid id, CancellationToken ct) =>
                Results.Ok(await dv.LayThanhVien(id, ct)));
    }
}
