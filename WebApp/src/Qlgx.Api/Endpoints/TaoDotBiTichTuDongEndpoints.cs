using Qlgx.Api.Dtos;
using Qlgx.Api.Services;

namespace Qlgx.Api.Endpoints;

/// <summary>"Tạo danh sách bí tích tự động" — xem TaoDotBiTichTuDongService và
/// docs/superpowers/specs/man-hinh/cong-cu-du-lieu.md mục 5.2.</summary>
public static class TaoDotBiTichTuDongEndpoints
{
    public static void MapTaoDotBiTichTuDong(this IEndpointRouteBuilder app)
    {
        var nhom = app.MapGroup("/api/cong-cu-du-lieu/tao-dot-bi-tich").RequireAuthorization();

        nhom.MapPost("/xem-truoc", async (TaoDotBiTichTuDongService dv, TaoDotBiTichTuDongRequest yc, CancellationToken ct) =>
        {
            var loi = KiemTraYeuCau(yc);
            if (loi is not null) return Results.BadRequest(new { thongBao = loi });
            return Results.Ok(await dv.XemTruoc(yc, ct));
        });

        nhom.MapPost("", async (TaoDotBiTichTuDongService dv, TaoDotBiTichTuDongRequest yc, CancellationToken ct) =>
        {
            var loi = KiemTraYeuCau(yc);
            if (loi is not null) return Results.BadRequest(new { thongBao = loi });
            return Results.Ok(await dv.TaoTuDong(yc, ct));
        });
    }

    // Nguyên văn 2 thông báo lỗi desktop (frmTaoDotBiTich.cs:31-42).
    private static string? KiemTraYeuCau(TaoDotBiTichTuDongRequest yc)
    {
        if (yc.LoaiBiTich is not (Qlgx.Domain.LoaiBiTich.RuaToi or Qlgx.Domain.LoaiBiTich.RuocLe or Qlgx.Domain.LoaiBiTich.ThemSuc))
            return "Xin vui lòng chọn loại bí tích cần tạo tự động.";
        if (yc.TuNgay > yc.DenNgay)
            return "Từ ngày phải nhỏ hơn hoặc bằng đến ngày.";
        return null;
    }
}
