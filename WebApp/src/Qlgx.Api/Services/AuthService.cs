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

    /// <summary>Số lần sai liên tiếp tối đa trước khi khoá tạm một tài khoản — review-backend.md
    /// mục T1. Ngưỡng và thời gian khoá cố tình rộng rãi (không phải 3-5 lần) để không khoá oan
    /// người dùng thật gõ nhầm vài lần; mục tiêu là chặn brute-force tự động, không phải chặn
    /// người dùng vô ý.</summary>
    private const int SoLanSaiToiDa = 10;
    private static readonly TimeSpan ThoiGianKhoa = TimeSpan.FromMinutes(15);

    // T2: tài khoản "giả" dùng để chạy một phép băm PBKDF2 tốn thời gian tương đương khi không
    // tìm thấy tài khoản nào trùng tên — nếu không, nhánh "không có ứng viên" trả lời NGAY LẬP
    // TỨC trong khi nhánh "có ứng viên" luôn chạy verify tốn vài chục mili-giây, lộ qua thời
    // gian phản hồi việc một tên đăng nhập có tồn tại trên máy chủ hay không (không phải bí mật
    // thật, chỉ dùng nội bộ để cân bằng thời gian — không phải mật khẩu của ai).
    private static readonly TaiKhoan TaiKhoanGia = new() { Id = Guid.Empty, TenTaiKhoan = "__gia__" };
    private static readonly string BamGia =
        new PasswordHasher<TaiKhoan>().HashPassword(TaiKhoanGia, "mat-khau-gia-de-can-bang-thoi-gian-phan-hoi");

    public async Task<DangNhapKetQua> DangNhap(string tenTaiKhoan, string matKhau, CancellationToken ct)
    {
        // Than JSON thieu truong (hoac gui rong) khien ASP.NET model binding tao chuoi rong/null
        // thay vi bi tu choi o tang validate — PasswordHasher.VerifyHashedPassword nem
        // ArgumentNullException voi mat khau null/rong, se lam lo mot 500 khong bat thay vi 401
        // nhu moi truong hop sai mat khau khac. Chan som, khong chay bam gia can bang thoi gian
        // o day vi day la loi dau vao ro rang, khong phai do vet ten dang nhap ton tai hay khong.
        if (string.IsNullOrEmpty(tenTaiKhoan) || string.IsNullOrEmpty(matKhau))
            return new DangNhapKetQua(KetQuaDangNhap.SaiTenHoacMatKhau, null, null);

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

        if (ungVien.Count == 0)
        {
            // Chay mot phep bam gia de thoi gian phan hoi giong nhu khi CO ung vien nhung sai
            // mat khau — xem BamGia o tren.
            _hasher.VerifyHashedPassword(TaiKhoanGia, BamGia, matKhau);
            return new DangNhapKetQua(KetQuaDangNhap.SaiTenHoacMatKhau, null, null);
        }

        var homNay = DateTimeOffset.UtcNow;
        foreach (var taiKhoan in ungVien)
        {
            // T1: tai khoan dang bi khoa tam (qua nhieu lan sai lien tiep) — khong thu mat
            // khau nua, coi nhu sai, KHONG gia han them thoi gian khoa (tranh ke tan cong tu
            // khoa vinh vien mot tai khoan bang cach spam request trong luc dang bi khoa).
            if (taiKhoan.KhoaDangNhapDenLuc is { } khoaDenLuc && khoaDenLuc > homNay) continue;

            var ketQua = _hasher.VerifyHashedPassword(taiKhoan, taiKhoan.MatKhauBam!, matKhau);
            if (ketQua is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded)
            {
                if (taiKhoan.SoLanDangNhapSaiLienTiep != 0 || taiKhoan.KhoaDangNhapDenLuc is not null)
                    await db.TaiKhoan.Where(t => t.Id == taiKhoan.Id).ExecuteUpdateAsync(s => s
                        .SetProperty(t => t.SoLanDangNhapSaiLienTiep, 0)
                        .SetProperty(t => t.KhoaDangNhapDenLuc, (DateTimeOffset?)null), ct);

                var token = tokenService.PhatHanh(taiKhoan);
                return new DangNhapKetQua(KetQuaDangNhap.ThanhCong, token,
                    new ThongTinNguoiDungDto(taiKhoan.Id, taiKhoan.TenTaiKhoan, taiKhoan.HoTenNguoiDung,
                        taiKhoan.LoaiTaiKhoan, taiKhoan.GiaoXuId));
            }
        }

        // Sai mat khau (hoac tai khoan dang bi khoa) — tang bo dem O CSDL (khong phai bo nho
        // tien trinh, xem ghi chu SoLanSaiToiDa) cho tung ung vien trung ten, khoa tam khi vuot
        // nguong. Dung UPDATE nguyen tu (khong doc-roi-ghi) de an toan khi nhieu ban API chay
        // song song cung tang bo dem cho cung mot tai khoan.
        foreach (var taiKhoan in ungVien)
        {
            var thoiHanKhoaMoi = homNay + ThoiGianKhoa;
            await db.Database.ExecuteSqlInterpolatedAsync($"""
                UPDATE tai_khoan
                SET so_lan_dang_nhap_sai_lien_tiep = so_lan_dang_nhap_sai_lien_tiep + 1,
                    khoa_dang_nhap_den_luc = CASE
                        WHEN so_lan_dang_nhap_sai_lien_tiep + 1 >= {SoLanSaiToiDa} THEN {thoiHanKhoaMoi}
                        ELSE khoa_dang_nhap_den_luc
                    END
                WHERE id = {taiKhoan.Id}
                """, ct);
        }

        return new DangNhapKetQua(KetQuaDangNhap.SaiTenHoacMatKhau, null, null);
    }

    /// <summary>Băm mật khẩu bằng PasswordHasher&lt;TaiKhoan&gt; (PBKDF2, chuẩn ASP.NET Core
    /// Identity) — dùng khi tạo/đổi mật khẩu một tài khoản. KHÔNG bao giờ dùng MD5/SHA1 trần
    /// hay lưu mật khẩu thô (quyết định bảo mật đã chốt, xem TaiKhoan.cs).</summary>
    public string Bam(TaiKhoan taiKhoan, string matKhauMoi) => _hasher.HashPassword(taiKhoan, matKhauMoi);
}
