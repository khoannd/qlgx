using Microsoft.EntityFrameworkCore;
using Qlgx.Api.Dtos;
using Qlgx.Api.Services;
using Qlgx.Data;

namespace Qlgx.Api.Endpoints;

/// <summary>
/// Danh mục Giáo họ thật, sắp theo mã cũ (đúng thứ tự hiển thị của combo `GxGiaoHo` bản
/// desktop, vốn nạp theo `SELECT * FROM GiaoHo ORDER BY MaGiaoHo` — không đọc sâu control này
/// trong phạm vi nhiệm vụ, chỉ suy từ quy ước "Mã cũ" tăng dần đã dùng nhất quán ở mọi danh
/// sách khác của hệ thống). Không phân trang: một giáo xứ chỉ có vài chục giáo họ.
/// </summary>
public static class GiaoHoEndpoints
{
    public static void MapGiaoHo(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/giao-ho", async (QlgxDbContext db, CancellationToken ct) =>
            Results.Ok(await db.GiaoHo.Where(h => !h.DaXoa)
                .OrderBy(h => h.MaGiaoHoCu)
                .Select(h => new GiaoHoDto(h.Id, h.MaGiaoHoCu, h.TenGiaoHo, h.GiaoHoChaId))
                .ToListAsync(ct))).RequireAuthorization();

        app.MapPost("/api/giao-ho", async (GiaoHoService dv, TaoGiaoHoRequest yc, CancellationToken ct) =>
            Results.Created($"/api/giao-ho/{await dv.Them(yc, ct)}", (object?)null)).RequireAuthorization();

        app.MapPut("/api/giao-ho/{id:guid}", async (GiaoHoService dv, Guid id,
            CapNhatGiaoHoRequest yc, CancellationToken ct) =>
            await dv.Sua(id, yc, ct) == KetQuaGiaoHo.KhongTimThay
                ? Results.NotFound() : Results.Ok()).RequireAuthorization();
    }
}
