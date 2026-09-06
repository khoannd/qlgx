using Qlgx.Api.Dtos;
using Qlgx.Api.Services;

namespace Qlgx.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuth(this IEndpointRouteBuilder app)
    {
        var nhom = app.MapGroup("/api/auth");

        // Anonymous CHỦ Ý — đây là nơi duy nhất trong hệ thống được phép nhận thông tin từ
        // trình duyệt trước khi có claim giáo_xu_id, vì mục đích của endpoint này chính là
        // TẠO ra claim đó. Không endpoint nghiệp vụ nào khác được AllowAnonymous.
        nhom.MapPost("/dang-nhap", async (AuthService dv, DangNhapRequest yc, CancellationToken ct) =>
        {
            var kq = await dv.DangNhap(yc.TenTaiKhoan, yc.MatKhau, ct);
            return kq.Ket switch
            {
                KetQuaDangNhap.ThanhCong => Results.Ok(new
                {
                    token = kq.Token,
                    hetHanSau = TokenService.ThoiGianSong.TotalSeconds,
                    nguoiDung = kq.NguoiDung,
                }),
                _ => Results.Json(new { thongBao = "Tên đăng nhập hoặc mật khẩu không chính xác" },
                    statusCode: StatusCodes.Status401Unauthorized),
            };
        }).AllowAnonymous();

        // Trả thông tin người dùng hiện tại — trang web dùng để hiện tên + vai trò trên thanh
        // trên sau khi tải lại trang (token đã có trong localStorage nhưng client cần xác
        // nhận nó còn hợp lệ và lấy lại họ tên/loại tài khoản mới nhất).
        nhom.MapGet("/toi", (HttpContext http) =>
        {
            var user = http.User;
            var giaoXuId = user.FindFirst(ClaimsQlgx.GiaoXuId)?.Value;
            var loai = user.FindFirst(ClaimsQlgx.LoaiTaiKhoan)?.Value;
            var tenTaiKhoan = user.FindFirst(ClaimsQlgx.TenTaiKhoan)?.Value;
            var hoTen = user.Identity?.Name;
            return Results.Ok(new
            {
                tenTaiKhoan,
                hoTen,
                loaiTaiKhoan = loai is null ? (int?)null : int.Parse(loai),
                giaoXuId,
            });
        }).RequireAuthorization();
    }
}
