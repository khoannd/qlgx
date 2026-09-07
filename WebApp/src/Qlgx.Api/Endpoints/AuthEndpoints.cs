using Microsoft.EntityFrameworkCore;
using Qlgx.Api.Dtos;
using Qlgx.Api.Services;
using Qlgx.Data;

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
        // nhận nó còn hợp lệ và lấy lại họ tên/loại tài khoản mới nhất), và TÊN GIÁO XỨ thật
        // (xem AppShell.tsx — trước đây viết cứng "Giáo xứ Thánh Tâm", xem
        // can-review-sau.md mục 32) để nhân viên biết chắc mình đang làm việc với dữ liệu của
        // giáo xứ nào trên một máy chủ phục vụ nhiều giáo xứ.
        nhom.MapGet("/toi", async (HttpContext http, QlgxDbContext db, CancellationToken ct) =>
        {
            var user = http.User;
            // giaoXuId LUÔN đọc từ claim của token đã xác thực (RequireAuthorization ở dưới),
            // KHÔNG BAO GIỜ từ tham số trình duyệt — xem BoiCanhGiaoXuTuNguoiDung.cs.
            var giaoXuIdThoi = user.FindFirst(ClaimsQlgx.GiaoXuId)?.Value;
            var loai = user.FindFirst(ClaimsQlgx.LoaiTaiKhoan)?.Value;
            var tenTaiKhoan = user.FindFirst(ClaimsQlgx.TenTaiKhoan)?.Value;
            var hoTen = user.Identity?.Name;

            string? tenGiaoXu = null;
            if (Guid.TryParse(giaoXuIdThoi, out var giaoXuId))
            {
                tenGiaoXu = await db.GiaoXu
                    .Where(g => g.Id == giaoXuId)
                    .Select(g => g.TenGiaoXu)
                    .FirstOrDefaultAsync(ct);
            }

            return Results.Ok(new
            {
                tenTaiKhoan,
                hoTen,
                loaiTaiKhoan = loai is null ? (int?)null : int.Parse(loai),
                giaoXuId = giaoXuIdThoi,
                tenGiaoXu,
            });
        }).RequireAuthorization();
    }
}
