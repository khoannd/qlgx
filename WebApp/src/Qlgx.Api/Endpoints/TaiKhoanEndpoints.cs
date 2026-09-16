using Qlgx.Api.Dtos;
using Qlgx.Api.Services;

namespace Qlgx.Api.Endpoints;

/// <summary>
/// Màn hình "Quản lý tài khoản" (xem docs/superpowers/specs/man-hinh/quan-ly-tai-khoan.md).
/// Chỉ Quản trị viên (LoaiTaiKhoan=0) được vào — bản desktop KHÔNG chặn quyền này (bất kỳ ai
/// mở được menu đều tạo/sửa/xoá được tài khoản, kể cả tự cấp quyền quản trị cho mình), nhưng
/// đó là một lỗ hổng của bản cũ chứ không phải hành vi cố ý cần giữ nguyên — bản web chặn bằng
/// policy "QuanTri" (xem Program.cs). Quyết định ghi ở can-review-sau.md.
/// </summary>
public static class TaiKhoanEndpoints
{
    public static void MapTaiKhoan(this IEndpointRouteBuilder app)
    {
        var nhom = app.MapGroup("/api/tai-khoan").RequireAuthorization("QuanTri");

        nhom.MapGet("", async (TaiKhoanService dv, CancellationToken ct) =>
            Results.Ok(await dv.LayDanhSach(ct)));

        nhom.MapPost("", async (TaiKhoanService dv, TaoTaiKhoanRequest yc, CancellationToken ct) =>
        {
            var (ket, id) = await dv.Tao(yc, ct);
            return ket switch
            {
                KetQuaLuuTaiKhoan.TrungTenTaiKhoan => Results.Conflict(
                    new { thongBao = "Tên tài khoản đã tồn tại, thử một tên khác" }),
                KetQuaLuuTaiKhoan.LoaiTaiKhoanKhongHopLe => LoiLoaiTaiKhoan(),
                KetQuaLuuTaiKhoan.MatKhauQuaNgan => LoiMatKhauQuaNgan(),
                _ => Results.Created($"/api/tai-khoan/{id}", new { id }),
            };
        });

        nhom.MapPut("/{id:guid}", async (TaiKhoanService dv, Guid id,
            CapNhatTaiKhoanRequest yc, CancellationToken ct) =>
            await dv.CapNhat(id, yc, ct) switch
            {
                KetQuaLuuTaiKhoan.KhongTimThay => Results.NotFound(),
                KetQuaLuuTaiKhoan.LoaiTaiKhoanKhongHopLe => LoiLoaiTaiKhoan(),
                KetQuaLuuTaiKhoan.MatKhauQuaNgan => LoiMatKhauQuaNgan(),
                KetQuaLuuTaiKhoan.KhongTuDoiLoaiCuaMinh => Results.Json(new
                {
                    thongBao = "Không thể tự đổi loại tài khoản của chính mình. " +
                               "Hãy nhờ một quản trị viên khác thực hiện thay đổi này."
                }, statusCode: StatusCodes.Status403Forbidden),
                KetQuaLuuTaiKhoan.DungPhienBan => Results.Conflict(new
                {
                    thongBao = "Tài khoản này vừa được người khác cập nhật. " +
                               "Hãy tải lại màn hình để xem thay đổi mới nhất rồi sửa lại."
                }),
                _ => Results.Ok(),
            });

        nhom.MapDelete("/{id:guid}", async (TaiKhoanService dv, Guid id, CancellationToken ct) =>
            await dv.Xoa(id, ct) ? Results.Ok() : Results.NotFound());
    }

    /// <summary>NT-1: câu trả lời cho mọi giá trị LoaiTaiKhoan không được phép cấp qua HTTP —
    /// đặc biệt là 9 ("Quản trị hệ thống"), chỉ cấp được bằng dòng lệnh trên máy chủ.</summary>
    private static IResult LoiLoaiTaiKhoan() => Results.BadRequest(new
    {
        thongBao = "Loại tài khoản không hợp lệ. Chỉ nhận: 0 (Quản trị viên), 1 (Người nhập), " +
                   "2 (Người xem). Tài khoản \"Quản trị hệ thống\" chỉ được tạo trực tiếp trên " +
                   "máy chủ, không cấp qua màn hình này."
    });

    /// <summary>TB-7: ngưỡng độ dài mật khẩu dùng chung với AuthService.DoiMatKhauCuaToi và với
    /// dòng lệnh tạo tài khoản quản trị — một ngưỡng duy nhất cho toàn hệ thống.</summary>
    private static IResult LoiMatKhauQuaNgan() => Results.BadRequest(new
    {
        thongBao = $"Mật khẩu phải có ít nhất {Services.AuthService.DoDaiMatKhauToiThieu} ký tự"
    });
}
