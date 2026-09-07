using Qlgx.Api.Dtos;
using Qlgx.Api.Services;

namespace Qlgx.Api.Endpoints;

public static class RaoHonPhoiEndpoints
{
    private const string DungPhienBanMsg =
        "Đôi rao này vừa được người khác cập nhật. Hãy tải lại màn hình để xem thay đổi mới nhất rồi sửa lại.";

    public static void MapRaoHonPhoi(this IEndpointRouteBuilder app)
    {
        var nhom = app.MapGroup("/api/rao-hon-phoi").RequireAuthorization();

        // xemTatCa khớp cbOption: mặc định false = "Chỉ xem những đôi rao chưa hoàn tất".
        nhom.MapGet("", async (RaoHonPhoiService dv, bool? xemTatCa, CancellationToken ct) =>
            Results.Ok(await dv.LayDanhSach(xemTatCa ?? false, ct)));

        nhom.MapGet("/{id:guid}", async (RaoHonPhoiService dv, Guid id, CancellationToken ct) =>
            await dv.LayChiTiet(id, ct) is { } ct2 ? Results.Ok(ct2) : Results.NotFound());

        nhom.MapPost("", async (RaoHonPhoiService dv, LuuRaoHonPhoiRequest yc, CancellationToken ct) =>
        {
            var loi = KiemTra(yc);
            if (loi is not null) return Results.BadRequest(new { thongBao = loi });
            var kq = await dv.Tao(yc, ct);
            return Results.Created($"/api/rao-hon-phoi/{kq.Id}", kq);
        });

        nhom.MapPut("/{id:guid}", async (RaoHonPhoiService dv, Guid id, LuuRaoHonPhoiRequest yc, CancellationToken ct) =>
        {
            var loi = KiemTra(yc);
            if (loi is not null) return Results.BadRequest(new { thongBao = loi });
            return await dv.CapNhat(id, yc, ct) switch
            {
                KetQuaLuuRaoHonPhoi.KhongTimThay => Results.NotFound(),
                KetQuaLuuRaoHonPhoi.DungPhienBan => Results.Conflict(new { thongBao = DungPhienBanMsg }),
                _ => Results.Ok(),
            };
        });

        // Hộp thoại xác nhận "Bạn có chắc muốn xóa (các) đôi rao được chọn..." chuyển sang
        // phía client (GxRaoHonPhoiList.cs:246).
        nhom.MapDelete("/{id:guid}", async (RaoHonPhoiService dv, Guid id, CancellationToken ct) =>
            await dv.Xoa(id, ct) ? Results.Ok() : Results.NotFound());
    }

    /// <summary>Khớp thứ tự kiểm tra của <c>gxCommand1_OnOK</c>
    /// (Source/GXControl/frmRaoHonPhoi.cs:113-129) cho phần KHÔNG phụ thuộc "In điều tra"
    /// (usePrint — chưa migrate ở Task này, không có tính năng in điều tra/kết quả rao ở web).
    /// </summary>
    private static string? KiemTra(LuuRaoHonPhoiRequest yc)
    {
        if (yc.GiaoDan1Id is null)
            return "Xin vui lòng nhập thông tin người thứ nhất cần rao";
        if (string.IsNullOrWhiteSpace(yc.TenRaoHonPhoi))
            return "Xin vui lòng nhập [đôi rao]";
        if (yc.GiaoDan2Id is null)
            return "Xin vui lòng nhập thông tin người thứ hai cần rao";
        return null;
    }
}
