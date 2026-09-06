using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Qlgx.Domain.Entities;

namespace Qlgx.Api.Services;

/// <summary>
/// Phát hành và cấu hình xác thực JWT tự chứa (self-contained) — máy chủ không giữ trạng thái
/// phiên trong tiến trình (yêu cầu HA, xem mục 1 của tài liệu thiết kế), nên đăng nhập không
/// tạo bản ghi "phiên" nào — mọi thông tin cần để xác định người dùng và giáo xứ nằm trong
/// chữ ký số của token.
/// </summary>
public class TokenService(IConfiguration cauHinh)
{
    // 8 tiếng — đủ một ngày làm việc, ngắn hơn thì người dùng phải đăng nhập lại giữa chừng
    // quá thường xuyên; dài hơn thì token bị lộ có thời gian sống nguy hiểm. Không có cơ chế
    // làm mới token (refresh token) ở Phase 1 — hết hạn thì đăng nhập lại; bản nháp form được
    // giữ ở localStorage nên không mất dữ liệu đang gõ (xem can-review-sau.md).
    public static readonly TimeSpan ThoiGianSong = TimeSpan.FromHours(8);

    private SymmetricSecurityKey KhoaKy()
    {
        var khoaChuoi = cauHinh["Qlgx:JwtKey"]
            ?? throw new InvalidOperationException(
                "Thieu cau hinh Qlgx:JwtKey (bien moi truong Qlgx__JwtKey). Day la khoa ky " +
                "token dang nhap — KHONG duoc ghi vao file trong repo. Vi du (PowerShell): " +
                "$env:Qlgx__JwtKey = [Convert]::ToBase64String((1..32 | %{ Get-Random -Max 256 }))");
        return new SymmetricSecurityKey(Convert.FromBase64String(khoaChuoi));
    }

    public string PhatHanh(TaiKhoan taiKhoan)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, taiKhoan.Id.ToString()),
            new(ClaimsQlgx.GiaoXuId, taiKhoan.GiaoXuId.ToString()),
            new(ClaimsQlgx.LoaiTaiKhoan, taiKhoan.LoaiTaiKhoan.ToString()),
            new(ClaimsQlgx.TenTaiKhoan, taiKhoan.TenTaiKhoan),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };
        if (!string.IsNullOrWhiteSpace(taiKhoan.HoTenNguoiDung))
            claims.Add(new Claim(ClaimTypes.Name, taiKhoan.HoTenNguoiDung));

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.Add(ThoiGianSong),
            signingCredentials: new SigningCredentials(KhoaKy(), SecurityAlgorithms.HmacSha256));
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Dùng lúc đăng ký AddJwtBearer, trước khi host dựng xong DI container nên không thể
    /// tiêm TokenService qua constructor — nhận thẳng IConfiguration. Khoá ký được tra CHẬM
    /// (IssuerSigningKeyResolver) thay vì đọc ngay lúc khởi động: một số bài test dựng
    /// WebApplicationFactory trần (không cấu hình Qlgx:JwtKey, ví dụ SucKhoeTests) chỉ gọi
    /// endpoint ẩn danh /api/suc-khoe — endpoint đó không kèm Bearer token nên middleware xác
    /// thực không bao giờ cần tới khoá ký; đọc khoá ngay khi đăng ký dịch vụ sẽ làm MỌI yêu
    /// cầu (kể cả endpoint ẩn danh) trả 500 chỉ vì thiếu cấu hình không liên quan.
    /// </summary>
    public static TokenValidationParameters ThamSoXacThuc(IConfiguration cauHinh) => new()
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKeyResolver = (_, _, _, _) => [new TokenService(cauHinh).KhoaKy()],
        ClockSkew = TimeSpan.FromSeconds(30),
    };
}
