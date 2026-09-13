using Qlgx.Api.Dtos;
using Qlgx.Api.Services;

namespace Qlgx.Api.Endpoints;

/// <summary>
/// Màn hình "Quản lý mẫu in" (xem docs/superpowers/specs/man-hinh/quan-ly-mau-in.md) — NĂNG
/// LỰC MỚI, không có ở bản desktop. Ba nhóm route:
///   - GET danh mục: mọi tài khoản đã đăng nhập xem được (RequireAuthorization() trơn) — cần
///     hiển thị cho cả tài khoản giáo xứ thường lẫn quản trị hệ thống, chỉ khác chỗ sửa được gì.
///   - "/rieng/*": policy "QuanTri" — giáo xứ tự sửa mẫu CỦA MÌNH, GiaoXuId luôn lấy từ claim
///     (MauInService.LuuRieng/KhoiPhucRieng), KHÔNG bao giờ nhận từ tham số.
///   - "/he-thong/*": policy "QuanTriHeThong" — chỉ Quản trị hệ thống sửa mẫu áp dụng chung.
/// "Xem thử" dùng chung một route, không phân biệt rieng/he-thong (chỉ cần vẽ PDF từ HTML nháp,
/// không đọc/ghi gì) — nhưng vẫn yêu cầu đăng nhập (RequireAuthorization trơn) để không biến
/// máy chủ thành dịch vụ "vẽ PDF từ HTML bất kỳ" công khai.
/// </summary>
public static class MauInEndpoints
{
    public static void MapMauIn(this IEndpointRouteBuilder app)
    {
        var nhom = app.MapGroup("/api/mau-in").RequireAuthorization();

        nhom.MapGet("", async (MauInService dv, CancellationToken ct) =>
            Results.Ok(await dv.LayDanhSach(ct)));

        nhom.MapPost("/{tenMau}/xem-thu", async (MauInService dv, string tenMau,
            XemThuMauInRequest yc, CancellationToken ct) =>
            await dv.XemThu(tenMau, yc.NoiDungHtml, ct) is { } pdf
                ? Results.File(pdf, "application/pdf", $"XemThu_{tenMau}.pdf")
                : Results.NotFound());

        var rieng = nhom.MapGroup("/{tenMau}/rieng").RequireAuthorization("QuanTri");
        rieng.MapGet("", async (MauInService dv, string tenMau, CancellationToken ct) =>
            await dv.LayRieng(tenMau, ct) is { } ct2 ? Results.Ok(ct2) : Results.NotFound());
        rieng.MapPut("", async (MauInService dv, string tenMau, LuuMauInRequest yc, CancellationToken ct) =>
            await dv.LuuRieng(tenMau, yc, ct) switch
            {
                KetQuaLuuMauIn.TenMauKhongHopLe => Results.NotFound(),
                KetQuaLuuMauIn.QuaLon => Results.BadRequest(new
                    { thongBao = "Nội dung mẫu quá lớn (tối đa 300KB), hãy rút gọn bớt." }),
                KetQuaLuuMauIn.DungPhienBan => Results.Conflict(new
                {
                    thongBao = "Mẫu này vừa được người khác cập nhật. " +
                               "Hãy tải lại màn hình để xem thay đổi mới nhất rồi sửa lại."
                }),
                _ => Results.Ok(),
            });
        rieng.MapDelete("", async (MauInService dv, string tenMau, CancellationToken ct) =>
            await dv.KhoiPhucRieng(tenMau, ct) ? Results.Ok() : Results.NotFound());

        var heThong = nhom.MapGroup("/{tenMau}/he-thong").RequireAuthorization("QuanTriHeThong");
        heThong.MapGet("", async (MauInService dv, string tenMau, CancellationToken ct) =>
            await dv.LayHeThong(tenMau, ct) is { } ct2 ? Results.Ok(ct2) : Results.NotFound());
        heThong.MapPut("", async (MauInService dv, string tenMau, LuuMauInRequest yc, CancellationToken ct) =>
            await dv.LuuHeThong(tenMau, yc, ct) switch
            {
                KetQuaLuuMauIn.TenMauKhongHopLe => Results.NotFound(),
                KetQuaLuuMauIn.QuaLon => Results.BadRequest(new
                    { thongBao = "Nội dung mẫu quá lớn (tối đa 300KB), hãy rút gọn bớt." }),
                KetQuaLuuMauIn.DungPhienBan => Results.Conflict(new
                {
                    thongBao = "Mẫu này vừa được người khác cập nhật. " +
                               "Hãy tải lại màn hình để xem thay đổi mới nhất rồi sửa lại."
                }),
                _ => Results.Ok(),
            });
        heThong.MapDelete("", async (MauInService dv, string tenMau, CancellationToken ct) =>
            await dv.KhoiPhucHeThong(tenMau, ct) ? Results.Ok() : Results.NotFound());
    }
}
