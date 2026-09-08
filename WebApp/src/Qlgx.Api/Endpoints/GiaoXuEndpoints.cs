using Qlgx.Api.Dtos;
using Qlgx.Api.Services;

namespace Qlgx.Api.Endpoints;

/// <summary>
/// Màn hình "Giáo xứ" — tự sửa thông tin giáo xứ CỦA MÌNH (xem GiaoXuService — GiaoXu không có
/// giao_xu_id nên không có RLS bảo vệ, phải tự lọc theo claim ở service). KHÁC
/// <see cref="QuanLyGiaoXuEndpoints"/> (policy "QuanTriHeThong", xem/sửa MỌI giáo xứ).
/// </summary>
public static class GiaoXuEndpoints
{
    public static void MapGiaoXu(this IEndpointRouteBuilder app)
    {
        var nhom = app.MapGroup("/api/giao-xu").RequireAuthorization();

        nhom.MapGet("", async (GiaoXuService dv, CancellationToken ct) =>
        {
            var kq = await dv.LayThongTin(ct);
            return kq is null ? Results.NotFound() : Results.Ok(kq);
        });

        nhom.MapPut("", async (GiaoXuService dv, CapNhatGiaoXuHienTaiRequest yc, CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(yc.TenGiaoXu))
                return Results.BadRequest(new { thongBao = "Hãy nhập tên giáo xứ!" });
            if (string.IsNullOrWhiteSpace(yc.DiaChi))
                return Results.BadRequest(new { thongBao = "Hãy nhập địa chỉ giáo xứ!" });
            var thanhCong = await dv.CapNhatThongTin(yc, ct);
            return thanhCong ? Results.Ok() : Results.NotFound();
        });
    }
}
