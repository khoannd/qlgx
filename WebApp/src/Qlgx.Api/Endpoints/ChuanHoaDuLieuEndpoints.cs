using Qlgx.Api.Services;

namespace Qlgx.Api.Endpoints;

/// <summary>"Chuẩn hoá dữ liệu" — xem ChuanHoaDuLieuService và
/// docs/superpowers/specs/man-hinh/cong-cu-du-lieu.md mục 5.1. Không nhận tham số nào từ trình
/// duyệt (áp dụng cho toàn bộ giáo dân/gia đình của giáo xứ, đúng phạm vi "không lọc gì" của
/// desktop) — CHỈ hai endpoint mỗi loại: xem-truoc (chạy thử) và ghi thật.</summary>
public static class ChuanHoaDuLieuEndpoints
{
    public static void MapChuanHoaDuLieu(this IEndpointRouteBuilder app)
    {
        var nhom = app.MapGroup("/api/cong-cu-du-lieu/chuan-hoa").RequireAuthorization();

        nhom.MapPost("/giao-dan/xem-truoc", async (ChuanHoaDuLieuService dv, CancellationToken ct) =>
            Results.Ok(await dv.XemTruocGiaoDan(ct)));

        nhom.MapPost("/giao-dan", async (ChuanHoaDuLieuService dv, CancellationToken ct) =>
            Results.Ok(await dv.ChuanHoaGiaoDan(ct)));

        nhom.MapPost("/gia-dinh/xem-truoc", async (ChuanHoaDuLieuService dv, CancellationToken ct) =>
            Results.Ok(await dv.XemTruocGiaDinh(ct)));

        nhom.MapPost("/gia-dinh", async (ChuanHoaDuLieuService dv, CancellationToken ct) =>
            Results.Ok(await dv.ChuanHoaGiaDinh(ct)));
    }
}
