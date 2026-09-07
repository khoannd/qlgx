using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Qlgx.Data;
using Qlgx.Domain.Entities;

namespace Qlgx.Api.Services;

public enum KetQuaDangNhap
{
    ThanhCong,
    SaiTenHoacMatKhau,
    /// <summary>Tên đăng nhập trùng ở NHIỀU giáo xứ khác nhau, client chưa gửi kèm GiaoXuId để
    /// biết chọn tài khoản nào — xem ghi chú M1 (review-cuoi.md) ở đầu file. Trả về TRƯỚC khi
    /// kiểm mật khẩu/tăng bộ đếm sai, và kèm sẵn danh sách giáo xứ trùng tên để client cho
    /// người dùng chọn rồi gửi lại đúng một GiaoXuId.</summary>
    CanChonGiaoXu,
}

/// <summary>Một giáo xứ có tài khoản trùng tên đăng nhập đang thử — chỉ Id + tên hiển thị, đủ
/// để người dùng nhận ra giáo xứ của mình trong hộp chọn.</summary>
public record GiaoXuLuaChonDto(Guid Id, string TenGiaoXu);

public record DangNhapKetQua(KetQuaDangNhap Ket, string? Token, ThongTinNguoiDungDto? NguoiDung,
    IReadOnlyList<GiaoXuLuaChonDto>? DanhSachGiaoXu = null);

/// <summary>TenGiaoXu đi kèm ngay từ lúc đăng nhập để thanh trên (AppShell.tsx) hiện đúng tên
/// giáo xứ ngay khi vào ứng dụng, không phải chờ thêm một round-trip tới /api/auth/toi.</summary>
public record ThongTinNguoiDungDto(Guid Id, string TenTaiKhoan, string? HoTen, int LoaiTaiKhoan, Guid GiaoXuId, string TenGiaoXu);

/// <summary>
/// Xác thực tên đăng nhập/mật khẩu. TenTaiKhoan chỉ duy nhất TRONG một giáo xứ (xem
/// TaiKhoanConfig — chỉ mục là (GiaoXuId, TenTaiKhoan)), không duy nhất toàn máy chủ, vì mỗi
/// giáo xứ trước đây là một bản cài độc lập nên có thể trùng tên đăng nhập ("vanphong" chẳng
/// hạn) khi gộp lên một máy chủ chung. Do đó bước đăng nhập PHẢI tra trên toàn bộ máy chủ
/// (không có giáo xứ nào biết trước — chưa xác thực thì chưa thể biết claim giao_xu_id) để
/// TÌM xem tên đăng nhập này thuộc (những) giáo xứ nào. Đây là truy vấn CHÉO GIÁO XỨ DUY NHẤT
/// được phép trong toàn hệ thống, và chỉ để xác định tài khoản nào đăng nhập — sau bước này,
/// mọi thứ đi qua BoiCanhGiaoXuTuNguoiDung như bình thường.
///
/// review-cuoi.md mục M1 (Trung bình): bản đầu của T1 (khoá đăng nhập sau nhiều lần sai) LẶP
/// qua TẤT CẢ tài khoản trùng tên ở MỌI giáo xứ khi kiểm/tăng bộ đếm sai — một kẻ tấn công gõ
/// sai mật khẩu 10 lần cho "vanphong" sẽ khoá luôn tài khoản "vanphong" ở MỌI giáo xứ khác,
/// dù chỉ đang nhắm một nơi. Sửa bằng cách THU HẸP danh sách ứng viên về ĐÚNG MỘT giáo xứ
/// trước khi kiểm mật khẩu/tăng bộ đếm: nếu tên đăng nhập chỉ tồn tại ở một giáo xứ (đúng
/// trường hợp máy chủ pilot hiện chỉ có một giáo xứ, và cả trường hợp phổ biến một tên đăng
/// nhập không trùng ai khác dù máy chủ có nhiều giáo xứ) thì xử lý y hệt trước đây — không bắt
/// người dùng chọn gì thêm. CHỈ khi tên đăng nhập THẬT SỰ trùng ở từ hai giáo xứ trở lên mới
/// dừng lại trả CanChonGiaoXu kèm danh sách, và client phải gửi lại đúng GiaoXuId đã chọn —
/// từ đó danh sách ứng viên chắc chắn còn tối đa MỘT tài khoản (chỉ mục (GiaoXuId,
/// TenTaiKhoan) là duy nhất), nên không còn đường nào tăng bộ đếm sai cho tài khoản của giáo
/// xứ khác được nữa. Cân nhắc đã bỏ: khoá cứng theo "server có bao nhiêu giáo xứ" (luôn bắt
/// chọn khi có ≥2 giáo xứ) — bị loại vì phần lớn tên đăng nhập không trùng ai, bắt chọn giáo
/// xứ mỗi lần đăng nhập chỉ vì server có nhiều giáo xứ là phiền không cần thiết (yêu cầu của
/// người giao việc), trong khi cách "theo đúng tên đang gõ" vẫn đóng chặt lỗ hổng M1 mà không
/// đổi trải nghiệm của đa số tài khoản.
/// </summary>
public class AuthService(IConfiguration cauHinh, TokenService tokenService, QlgxDbContext dbNguoiDung)
{
    private readonly PasswordHasher<TaiKhoan> _hasher = new();

    /// <summary>Đồng bộ với TaoTaiKhoanQuanTri.cs (`QLGX_ADMIN_MAT_KHAU phai co it nhat 8 ky
    /// tu`) — một ngưỡng tối thiểu duy nhất cho toàn hệ thống, không lệch giữa CLI và tự đổi
    /// mật khẩu.</summary>
    private const int DoDaiMatKhauToiThieu = 8;

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

    public async Task<DangNhapKetQua> DangNhap(string tenTaiKhoan, string matKhau, Guid? giaoXuId, CancellationToken ct)
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

        // Buoc TRA CUU (van chay tren toan may chu, xem doc XML o dau class) — nhung TU DAY
        // TRO DI moi buoc kiem mat khau/tang bo dem CHI con duoc phep dung tren mot giao xu
        // DUY NHAT, xem thu hep ben duoi (sua loi M1 — review-cuoi.md).
        var ungVien = await db.TaiKhoan
            .Where(t => t.TenTaiKhoan == tenTaiKhoan && !t.DaXoa && t.MatKhauBam != null)
            .ToListAsync(ct);

        if (giaoXuId is { } gxDaChon)
        {
            // Client da chon giao xu (tu luot dang nhap truoc bi CanChonGiaoXu, hoac he thong
            // chi co 1 giao xu nen frontend gan san) — thu hep ngay, chi muc
            // (GiaoXuId, TenTaiKhoan) la duy nhat nen ket qua chac chan con toi da 1 phan tu.
            ungVien = ungVien.Where(t => t.GiaoXuId == gxDaChon).ToList();
        }
        else
        {
            var giaoXuTrungTen = ungVien.Select(t => t.GiaoXuId).Distinct().ToList();
            if (giaoXuTrungTen.Count > 1)
            {
                // THAT SU trung ten o tu hai giao xu tro len — day la truong hop DUY NHAT bat
                // dung lai hoi, tranh doan bua roi khoa nham/khoa cheo tai khoan cua giao xu
                // khac (loi M1). Chua kiem mat khau, chua tang bo dem cho ai ca.
                var danhSach = await db.GiaoXu
                    .Where(g => giaoXuTrungTen.Contains(g.Id))
                    .Select(g => new GiaoXuLuaChonDto(g.Id, g.TenGiaoXu))
                    .ToListAsync(ct);
                return new DangNhapKetQua(KetQuaDangNhap.CanChonGiaoXu, null, null, danhSach);
            }
            // 0 hoac 1 giao xu trung ten — khong can hoi, xu ly y het truoc day (giu nguyen
            // trai nghiem cho giao xu pilot hien chi co mot giao xu, va cho da so ten dang
            // nhap khong trung ai du may chu co nhieu giao xu).
        }

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
                var tenGiaoXu = await db.GiaoXu
                    .Where(g => g.Id == taiKhoan.GiaoXuId)
                    .Select(g => g.TenGiaoXu)
                    .FirstOrDefaultAsync(ct) ?? "";
                return new DangNhapKetQua(KetQuaDangNhap.ThanhCong, token,
                    new ThongTinNguoiDungDto(taiKhoan.Id, taiKhoan.TenTaiKhoan, taiKhoan.HoTenNguoiDung,
                        taiKhoan.LoaiTaiKhoan, taiKhoan.GiaoXuId, tenGiaoXu));
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

    /// <summary>
    /// Người đang đăng nhập tự đổi mật khẩu CỦA CHÍNH MÌNH (VIEC-TIEP-THEO.md mục 1.3) — bắt
    /// buộc kiểm đúng mật khẩu HIỆN TẠI trước khi ghi mật khẩu mới, nếu không ai mượn được máy
    /// đang mở phiên đăng nhập là chiếm được tài khoản vĩnh viễn (đổi mật khẩu xong chủ tài
    /// khoản thật cũng bị khoá luôn). `dbNguoiDung` là QlgxDbContext tiêm qua DI, đã tự lọc
    /// theo GiaoXuId của claim (BoiCanhGiaoXuTuNguoiDung) — kết hợp với `taiKhoanId` cũng lấy
    /// từ claim (không phải tham số trình duyệt) nên không thể đổi mật khẩu tài khoản khác,
    /// kể cả tài khoản khác trong CÙNG giáo xứ.
    /// </summary>
    public async Task<KetQuaDoiMatKhau> DoiMatKhauCuaToi(
        Guid taiKhoanId, string matKhauHienTai, string matKhauMoi, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(matKhauMoi) || matKhauMoi.Length < DoDaiMatKhauToiThieu)
            return KetQuaDoiMatKhau.MatKhauMoiQuaNgan;

        var taiKhoan = await dbNguoiDung.TaiKhoan
            .FirstOrDefaultAsync(t => t.Id == taiKhoanId && !t.DaXoa, ct);
        // taiKhoan null hoac chua co mat khau (khong nen xay ra voi mot phien da dang nhap
        // thanh cong, nhung khong loai tru hoan toan — vd tai khoan bi xoa giua chung phien)
        // deu tra ve CUNG mot loi "sai mat khau hien tai", khong phan biet ly do — khong co gi
        // de lo them vi nguoi goi da xac thuc, khong phai do tham do tai khoan ai do ton tai.
        if (taiKhoan?.MatKhauBam is null)
            return KetQuaDoiMatKhau.SaiMatKhauHienTai;

        var ketQuaKiemTra = _hasher.VerifyHashedPassword(taiKhoan, taiKhoan.MatKhauBam, matKhauHienTai);
        if (ketQuaKiemTra is not (PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded))
            return KetQuaDoiMatKhau.SaiMatKhauHienTai;

        taiKhoan.MatKhauBam = _hasher.HashPassword(taiKhoan, matKhauMoi);
        await dbNguoiDung.SaveChangesAsync(ct);
        return KetQuaDoiMatKhau.ThanhCong;
    }
}

public enum KetQuaDoiMatKhau
{
    ThanhCong,
    SaiMatKhauHienTai,
    MatKhauMoiQuaNgan,
}
