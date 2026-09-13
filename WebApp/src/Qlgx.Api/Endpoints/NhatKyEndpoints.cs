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
            {
                // Kẹp giá trị NGAY tại biên vào, trước khi nó tới .Take(). gioiHan là tham số
                // truy vấn do client tự đặt: ?gioiHan=-1 làm .Take() nhận số âm (hành vi lạ,
                // không phải lỗi rõ ràng), còn ?gioiHan=100000000 kéo nguyên nhật ký của một
                // bản ghi vào bộ nhớ máy chủ — một yêu cầu cũng đủ làm nghẽn máy giáo xứ cấu
                // hình thấp. 1000 dòng đã quá đủ cho màn hình lịch sử của một bản ghi.
                var soDong = Math.Clamp(gioiHan ?? 200, 1, 1000);
                return Results.Ok(await dv.LichSu(bang, banGhiId, soDong, ct));
            })
           .RequireAuthorization();
    }
}
