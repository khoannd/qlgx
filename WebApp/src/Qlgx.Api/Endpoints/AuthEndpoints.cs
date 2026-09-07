using System.Security.Claims;
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
            var kq = await dv.DangNhap(yc.TenTaiKhoan, yc.MatKhau, yc.GiaoXuId, ct);
            return kq.Ket switch
            {
                KetQuaDangNhap.ThanhCong => Results.Ok(new
                {
                    token = kq.Token,
                    hetHanSau = TokenService.ThoiGianSong.TotalSeconds,
                    nguoiDung = kq.NguoiDung,
                }),
                // M1 (review-cuoi.md): tên đăng nhập trùng ở nhiều giáo xứ — client phải cho
                // người dùng chọn đúng giáo xứ rồi gửi lại kèm GiaoXuId, KHÔNG được đoán/thử
                // lần lượt (đó chính là nguyên nhân lỗi khoá chéo giáo xứ cũ).
                KetQuaDangNhap.CanChonGiaoXu => Results.Json(new
                {
                    thongBao = "Tên đăng nhập này có ở nhiều giáo xứ — hãy chọn đúng giáo xứ của bạn rồi đăng nhập lại",
                    canChonGiaoXu = true,
                    giaoXu = kq.DanhSachGiaoXu,
                }, statusCode: StatusCodes.Status400BadRequest),
                _ => Results.Json(new { thongBao = "Tên đăng nhập hoặc mật khẩu không chính xác" },
                    statusCode: StatusCodes.Status401Unauthorized),
            };
        }).AllowAnonymous();

        // Tự đổi mật khẩu của chính mình (VIEC-TIEP-THEO.md mục 1.3) — chỉ cần đăng nhập
        // (không cần quyền Quản trị, khác hẳn /api/tai-khoan chỉ Quản trị viên mới vào được).
        // TaiKhoanId LUÔN lấy từ claim "sub" của token (ClaimTypes.NameIdentifier sau khi ánh
        // xạ mặc định của JwtSecurityTokenHandler), không bao giờ từ tham số — không ai đổi
        // được mật khẩu người khác qua đường này.
        nhom.MapPut("/mat-khau", async (HttpContext http, AuthService dv, DoiMatKhauRequest yc, CancellationToken ct) =>
        {
            var idThoi = http.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(idThoi, out var taiKhoanId)) return Results.Unauthorized();

            var ket = await dv.DoiMatKhauCuaToi(taiKhoanId, yc.MatKhauHienTai, yc.MatKhauMoi, ct);
            return ket switch
            {
                KetQuaDoiMatKhau.ThanhCong => Results.Ok(),
                KetQuaDoiMatKhau.MatKhauMoiQuaNgan => Results.Json(
                    new { thongBao = "Mật khẩu mới phải có ít nhất 8 ký tự" },
                    statusCode: StatusCodes.Status400BadRequest),
                _ => Results.Json(new { thongBao = "Mật khẩu hiện tại không đúng" },
                    statusCode: StatusCodes.Status400BadRequest),
            };
        }).RequireAuthorization();

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
