using Qlgx.Api.Services;

namespace Qlgx.Api.Endpoints;

/// <summary>
/// Lịch sử thay đổi của một bản ghi. RequireAuthorization() trơn + bộ lọc GiaoXuId qua claim
/// là đủ: nhật ký chịu cùng bộ lọc toàn cục và cùng RLS như bảng nghiệp vụ.
/// </summary>
public static class NhatKyEndpoints
{
    public static void MapNhatKy(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/nhat-ky/{bang}/{banGhiId:guid}",
            async (NhatKyService dv, string bang, Guid banGhiId, int? gioiHan, CancellationToken ct) =>
                Results.Ok(await dv.LichSu(bang, banGhiId, gioiHan ?? 200, ct)))
           .RequireAuthorization();
    }
}
