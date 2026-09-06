using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Qlgx.Data;
using Qlgx.Domain.Entities;

namespace Qlgx.Api.Services;

public enum KetQuaDangNhap
{
    ThanhCong,
    SaiTenHoacMatKhau,
}

public record DangNhapKetQua(KetQuaDangNhap Ket, string? Token, ThongTinNguoiDungDto? NguoiDung);

public record ThongTinNguoiDungDto(Guid Id, string TenTaiKhoan, string? HoTen, int LoaiTaiKhoan, Guid GiaoXuId);

/// <summary>
/// Xác thực tên đăng nhập/mật khẩu. TenTaiKhoan chỉ duy nhất TRONG một giáo xứ (xem
/// TaiKhoanConfig — chỉ mục là (GiaoXuId, TenTaiKhoan)), không duy nhất toàn máy chủ, vì mỗi
/// giáo xứ trước đây là một bản cài độc lập nên có thể trùng tên đăng nhập ("vanphong" chẳng
/// hạn) khi gộp lên một máy chủ chung. Do đó bước đăng nhập PHẢI tra trên toàn bộ máy chủ
/// (không có giáo xứ nào biết trước — chưa xác thực thì chưa thể biết claim giao_xu_id), thử
/// khớp mật khẩu với TỪNG tài khoản trùng tên cho tới khi khớp. Đây là truy vấn CHÉO GIÁO XỨ
/// DUY NHẤT được phép trong toàn hệ thống, và chỉ để xác định tài khoản nào đăng nhập — sau
/// bước này, mọi thứ đi qua BoiCanhGiaoXuTuNguoiDung như bình thường.
/// </summary>
public class AuthService(IConfiguration cauHinh, TokenService tokenService)
{
    private readonly PasswordHasher<TaiKhoan> _hasher = new();

    public async Task<DangNhapKetQua> DangNhap(string tenTaiKhoan, string matKhau, CancellationToken ct)
    {
        // Boi canh = null (khong dung DI) — day la truy van duy nhat trong he thong duoc phep
        // bo qua bo loc tenant, vi luc nay chua biet nguoi dung thuoc giao xu nao. Dung chuoi
        // ket noi QUAN TRI (vai tro co BYPASSRLS) — xem ChuoiKetNoiQuanTri.
        var options = new DbContextOptionsBuilder<QlgxDbContext>()
            .UseNpgsql(ChuoiKetNoiQuanTri.Doc(cauHinh))
            .Options;
        await using var db = new QlgxDbContext(options);

        var ungVien = await db.TaiKhoan
            .Where(t => t.TenTaiKhoan == tenTaiKhoan && !t.DaXoa && t.MatKhauBam != null)
            .ToListAsync(ct);

        foreach (var taiKhoan in ungVien)
        {
            var ketQua = _hasher.VerifyHashedPassword(taiKhoan, taiKhoan.MatKhauBam!, matKhau);
            if (ketQua is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded)
            {
                var token = tokenService.PhatHanh(taiKhoan);
                return new DangNhapKetQua(KetQuaDangNhap.ThanhCong, token,
                    new ThongTinNguoiDungDto(taiKhoan.Id, taiKhoan.TenTaiKhoan, taiKhoan.HoTenNguoiDung,
                        taiKhoan.LoaiTaiKhoan, taiKhoan.GiaoXuId));
            }
        }

        return new DangNhapKetQua(KetQuaDangNhap.SaiTenHoacMatKhau, null, null);
    }

    /// <summary>Băm mật khẩu bằng PasswordHasher&lt;TaiKhoan&gt; (PBKDF2, chuẩn ASP.NET Core
    /// Identity) — dùng khi tạo/đổi mật khẩu một tài khoản. KHÔNG bao giờ dùng MD5/SHA1 trần
    /// hay lưu mật khẩu thô (quyết định bảo mật đã chốt, xem TaiKhoan.cs).</summary>
    public string Bam(TaiKhoan taiKhoan, string matKhauMoi) => _hasher.HashPassword(taiKhoan, matKhauMoi);
}
