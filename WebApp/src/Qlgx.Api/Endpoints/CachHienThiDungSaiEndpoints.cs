using Qlgx.Api.Dtos;
using Qlgx.Api.Services;

namespace Qlgx.Api.Endpoints;

/// <summary>
/// Khu vực "Cách hiển thị dữ liệu đúng/sai" của màn hình "Quản lý mẫu in" (xem
/// docs/superpowers/specs/man-hinh/quan-ly-mau-in.md) — NĂNG LỰC MỚI, không có ở bản desktop.
///
/// Chia nhóm route ĐỐI XỨNG với <see cref="MauInEndpoints"/>, kể cả cách đặt policy:
///   - GET danh mục: mọi tài khoản đã đăng nhập xem được (RequireAuthorization() trơn) — cần
///     hiển thị cho cả tài khoản giáo xứ thường lẫn quản trị hệ thống, chỉ khác chỗ sửa được gì.
///   - "/rieng": policy "QuanTri" — giáo xứ tự đặt câu chữ CỦA MÌNH, GiaoXuId luôn lấy từ claim
///     (CachHienThiDungSaiService.LuuRieng/KhoiPhucRieng), KHÔNG bao giờ nhận từ tham số.
///   - "/he-thong": policy "QuanTriHeThong" — chỉ Quản trị hệ thống đặt câu chữ áp dụng chung.
///
/// Khác một điểm có chủ đích so với MauInEndpoints: KHÔNG có GET riêng cho từng cấp — một lượt
/// GET danh mục đã trả đủ cả hai cấp cho cả 5 biến (xem ghi chú ở CachHienThiDungSaiItemDto),
/// nên thêm hai route GET nữa chỉ là mã chết.
/// </summary>
public static class CachHienThiDungSaiEndpoints
{
    private static IResult TraKetQua(KetQuaLuuCachHienThi kq) => kq switch
    {
        KetQuaLuuCachHienThi.TenBienKhongHopLe => Results.NotFound(),
        KetQuaLuuCachHienThi.QuaDai => Results.BadRequest(new
        {
            thongBao = "Câu chữ quá dài (tối đa 200 ký tự). Đây là một cụm từ ngắn in trên giấy, " +
                       "hãy rút gọn lại."
        }),
        KetQuaLuuCachHienThi.DungPhienBan => Results.Conflict(new
        {
            thongBao = "Mục này vừa được người khác cập nhật. " +
                       "Hãy tải lại màn hình để xem thay đổi mới nhất rồi sửa lại."
        }),
        _ => Results.Ok(),
    };

    public static void MapCachHienThi(this IEndpointRouteBuilder app)
    {
        var nhom = app.MapGroup("/api/cach-hien-thi").RequireAuthorization();

        nhom.MapGet("", async (CachHienThiDungSaiService dv, CancellationToken ct) =>
            Results.Ok(await dv.LayDanhSach(ct)));

        var rieng = nhom.MapGroup("/{tenBien}/rieng").RequireAuthorization("QuanTri");
        rieng.MapPut("", async (CachHienThiDungSaiService dv, string tenBien,
            LuuCachHienThiRequest yc, CancellationToken ct) =>
            TraKetQua(await dv.LuuRieng(tenBien, yc, ct)));
        rieng.MapDelete("", async (CachHienThiDungSaiService dv, string tenBien, CancellationToken ct) =>
            await dv.KhoiPhucRieng(tenBien, ct) ? Results.Ok() : Results.NotFound());

        var heThong = nhom.MapGroup("/{tenBien}/he-thong").RequireAuthorization("QuanTriHeThong");
        heThong.MapPut("", async (CachHienThiDungSaiService dv, string tenBien,
            LuuCachHienThiRequest yc, CancellationToken ct) =>
            TraKetQua(await dv.LuuHeThong(tenBien, yc, ct)));
        heThong.MapDelete("", async (CachHienThiDungSaiService dv, string tenBien, CancellationToken ct) =>
            await dv.KhoiPhucHeThong(tenBien, ct) ? Results.Ok() : Results.NotFound());
    }
}
