using Qlgx.Api.Dtos;
using Qlgx.Api.Services;

namespace Qlgx.Api.Endpoints;

/// <summary>
/// "Danh sách các cha quản xứ" của màn hình "Giáo xứ" (xem giao-xu.md mục 3.2) — CHỈ trong
/// phạm vi giáo xứ của người gọi (RLS bảo vệ, khác GiaoXuEndpoints không có RLS). Không phân
/// trang: một giáo xứ chỉ có vài chục cha quản xứ trong suốt lịch sử.
/// </summary>
public static class LinhMucEndpoints
{
    public static void MapLinhMuc(this IEndpointRouteBuilder app)
    {
        var nhom = app.MapGroup("/api/linh-muc").RequireAuthorization();

        nhom.MapGet("", async (LinhMucService dv, CancellationToken ct) =>
            Results.Ok(await dv.DanhSach(ct)));

        nhom.MapPost("", async (LinhMucService dv, TaoLinhMucRequest yc, CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(yc.HoTen))
                return Results.BadRequest(new { thongBao = "Hãy nhập họ tên!" });
            var id = await dv.Them(yc, ct);
            return Results.Created($"/api/linh-muc/{id}", (object?)null);
        });

        nhom.MapPut("/{id:guid}", async (LinhMucService dv, Guid id,
            CapNhatLinhMucRequest yc, CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(yc.HoTen))
                return Results.BadRequest(new { thongBao = "Hãy nhập họ tên!" });
            return await dv.Sua(id, yc, ct) == KetQuaLinhMuc.KhongTimThay
                ? Results.NotFound()
                : Results.Ok();
        });

        nhom.MapDelete("/{id:guid}", async (LinhMucService dv, Guid id, CancellationToken ct) =>
            await dv.Xoa(id, ct) == KetQuaLinhMuc.KhongTimThay ? Results.NotFound() : Results.Ok());
    }
}
