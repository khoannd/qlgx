using Microsoft.EntityFrameworkCore;
using Qlgx.Api.Dtos;
using Qlgx.Data;
using Qlgx.Domain.Entities;

namespace Qlgx.Api.Services;

public enum KetQuaQuanLyGiaoXu
{
    ThanhCong,
    KhongTimThay,
    TrungTenTaiKhoan,
}

/// <summary>
/// Màn hình "Quản lý giáo phận/giáo hạt/giáo xứ" — xem
/// docs/superpowers/specs/man-hinh/quan-ly-giao-xu.md. ĐƯỜNG DẪN THỨ TƯ được phép đọc/ghi
/// CHÉO GIÁO XỨ trong toàn hệ thống (cùng nhóm với đăng nhập/tạo tài khoản quản trị đầu
/// tiên/công cụ chuyển dữ liệu — xem ChuoiKetNoiQuanTri.cs), vì đối tượng làm việc CHÍNH của
/// màn hình này là các giáo xứ khác nhau. Vì vậy service này KHÔNG dùng QlgxDbContext tiêm qua
/// DI (vốn bị lọc theo BoiCanhGiaoXuTuNguoiDung của người gọi và bị RLS chặn ghi chéo giáo
/// xứ cho bảng TaiKhoan) mà tự mở một QlgxDbContext riêng bằng chuỗi kết nối QUẢN TRỊ
/// (BYPASSRLS). Việc CHỈ endpoint dùng policy "QuanTriHeThong" mới gọi được service này là
/// lớp phòng thủ DUY NHẤT — GiaoPhan/GiaoHat/GiaoXu không có cột giao_xu_id nên KHÔNG có RLS
/// bảo vệ, xem mục 4 của spec.
/// </summary>
public class QuanLyGiaoXuService(IConfiguration cauHinh)
{
    private QlgxDbContext MoContextQuanTri()
    {
        var options = new DbContextOptionsBuilder<QlgxDbContext>()
            .UseNpgsql(ChuoiKetNoiQuanTri.Doc(cauHinh))
            .Options;
        return new QlgxDbContext(options);
    }

    // --- Giáo phận ---

    public async Task<List<GiaoPhanDto>> LayDanhSachGiaoPhan(CancellationToken ct)
    {
        await using var db = MoContextQuanTri();
        return await db.GiaoPhan
            .OrderBy(g => g.TenGiaoPhan)
            .Select(g => new GiaoPhanDto(g.Id, g.TenGiaoPhan, g.GhiChu))
            .ToListAsync(ct);
    }

    public async Task<Guid> TaoGiaoPhan(TaoGiaoPhanRequest yc, CancellationToken ct)
    {
        await using var db = MoContextQuanTri();
        var giaoPhan = new GiaoPhan { TenGiaoPhan = yc.TenGiaoPhan, GhiChu = yc.GhiChu };
        db.GiaoPhan.Add(giaoPhan);
        await db.SaveChangesAsync(ct);
        return giaoPhan.Id;
    }

    public async Task<KetQuaQuanLyGiaoXu> CapNhatGiaoPhan(Guid id, CapNhatGiaoPhanRequest yc, CancellationToken ct)
    {
        await using var db = MoContextQuanTri();
        var giaoPhan = await db.GiaoPhan.FirstOrDefaultAsync(g => g.Id == id, ct);
        if (giaoPhan is null) return KetQuaQuanLyGiaoXu.KhongTimThay;
        giaoPhan.TenGiaoPhan = yc.TenGiaoPhan;
        giaoPhan.GhiChu = yc.GhiChu;
        await db.SaveChangesAsync(ct);
        return KetQuaQuanLyGiaoXu.ThanhCong;
    }

    // --- Giáo hạt ---

    public async Task<List<GiaoHatDto>> LayDanhSachGiaoHat(CancellationToken ct)
    {
        await using var db = MoContextQuanTri();
        return await db.GiaoHat
            .Include(h => h.GiaoPhan)
            .OrderBy(h => h.TenGiaoHat)
            .Select(h => new GiaoHatDto(h.Id, h.GiaoPhanId, h.GiaoPhan!.TenGiaoPhan, h.TenGiaoHat, h.GhiChu))
            .ToListAsync(ct);
    }

    public async Task<Guid> TaoGiaoHat(TaoGiaoHatRequest yc, CancellationToken ct)
    {
        await using var db = MoContextQuanTri();
        var giaoHat = new GiaoHat { GiaoPhanId = yc.GiaoPhanId, TenGiaoHat = yc.TenGiaoHat, GhiChu = yc.GhiChu };
        db.GiaoHat.Add(giaoHat);
        await db.SaveChangesAsync(ct);
        return giaoHat.Id;
    }

    public async Task<KetQuaQuanLyGiaoXu> CapNhatGiaoHat(Guid id, CapNhatGiaoHatRequest yc, CancellationToken ct)
    {
        await using var db = MoContextQuanTri();
        var giaoHat = await db.GiaoHat.FirstOrDefaultAsync(h => h.Id == id, ct);
        if (giaoHat is null) return KetQuaQuanLyGiaoXu.KhongTimThay;
        giaoHat.GiaoPhanId = yc.GiaoPhanId;
        giaoHat.TenGiaoHat = yc.TenGiaoHat;
        giaoHat.GhiChu = yc.GhiChu;
        await db.SaveChangesAsync(ct);
        return KetQuaQuanLyGiaoXu.ThanhCong;
    }

    // --- Giáo xứ ---

    public async Task<List<GiaoXuDto>> LayDanhSachGiaoXu(CancellationToken ct)
    {
        await using var db = MoContextQuanTri();
        var giaoXu = await db.GiaoXu
            .Include(x => x.GiaoHat!).ThenInclude(h => h.GiaoPhan)
            .OrderBy(x => x.TenGiaoXu)
            .ToListAsync(ct);
        var soTaiKhoanTheoGiaoXu = await db.TaiKhoan
            .Where(t => !t.DaXoa)
            .GroupBy(t => t.GiaoXuId)
            .Select(g => new { g.Key, SoLuong = g.Count() })
            .ToDictionaryAsync(g => g.Key, g => g.SoLuong, ct);
        var tenDaDung = giaoXu.Select(x => x.TenGiaoXu).ToList();

        return giaoXu.Select(x => new GiaoXuDto(
            x.Id, x.GiaoHatId, x.GiaoHat?.TenGiaoHat, x.GiaoHat?.GiaoPhan?.TenGiaoPhan,
            x.TenGiaoXu, x.DiaChi, x.DienThoai, x.Email, x.Website, x.GhiChu,
            tenDaDung.Count(t => t == x.TenGiaoXu) > 1,
            soTaiKhoanTheoGiaoXu.GetValueOrDefault(x.Id))).ToList();
    }

    public async Task<Guid> TaoGiaoXu(TaoGiaoXuRequest yc, CancellationToken ct)
    {
        await using var db = MoContextQuanTri();
        var giaoXu = new GiaoXu
        {
            GiaoHatId = yc.GiaoHatId,
            TenGiaoXu = yc.TenGiaoXu,
            DiaChi = yc.DiaChi,
            DienThoai = yc.DienThoai,
            Email = yc.Email,
            Website = yc.Website,
            GhiChu = yc.GhiChu,
        };
        db.GiaoXu.Add(giaoXu);
        await db.SaveChangesAsync(ct);
        return giaoXu.Id;
    }

    public async Task<KetQuaQuanLyGiaoXu> CapNhatGiaoXu(Guid id, CapNhatGiaoXuRequest yc, CancellationToken ct)
    {
        await using var db = MoContextQuanTri();
        var giaoXu = await db.GiaoXu.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (giaoXu is null) return KetQuaQuanLyGiaoXu.KhongTimThay;
        giaoXu.GiaoHatId = yc.GiaoHatId;
        giaoXu.TenGiaoXu = yc.TenGiaoXu;
        giaoXu.DiaChi = yc.DiaChi;
        giaoXu.DienThoai = yc.DienThoai;
        giaoXu.Email = yc.Email;
        giaoXu.Website = yc.Website;
        giaoXu.GhiChu = yc.GhiChu;
        await db.SaveChangesAsync(ct);
        return KetQuaQuanLyGiaoXu.ThanhCong;
    }

    /// <summary>Tạo tài khoản quản trị đầu tiên cho MỘT giáo xứ khác — luôn LoaiTaiKhoan=0,
    /// GiaoXuId lấy từ {id} trên đường dẫn, không từ thân yêu cầu. Trùng tên trong PHẠM VI
    /// giáo xứ đó (giống TaiKhoanService.Tao, TenTaiKhoan chỉ duy nhất theo (GiaoXuId,
    /// TenTaiKhoan)).</summary>
    public async Task<(KetQuaQuanLyGiaoXu Ket, Guid? Id)> TaoTaiKhoanChoGiaoXu(
        Guid giaoXuId, TaoTaiKhoanChoGiaoXuRequest yc, AuthService auth, CancellationToken ct)
    {
        await using var db = MoContextQuanTri();
        var giaoXuTonTai = await db.GiaoXu.AnyAsync(x => x.Id == giaoXuId, ct);
        if (!giaoXuTonTai) return (KetQuaQuanLyGiaoXu.KhongTimThay, null);

        var daTrung = await db.TaiKhoan.AnyAsync(
            t => t.GiaoXuId == giaoXuId && t.TenTaiKhoan == yc.TenTaiKhoan && !t.DaXoa, ct);
        if (daTrung) return (KetQuaQuanLyGiaoXu.TrungTenTaiKhoan, null);

        var taiKhoan = new TaiKhoan
        {
            GiaoXuId = giaoXuId,
            TenTaiKhoan = yc.TenTaiKhoan,
            HoTenNguoiDung = yc.HoTenNguoiDung,
            Email = yc.Email,
            SoDienThoai = yc.SoDienThoai,
            LoaiTaiKhoan = 0,
        };
        taiKhoan.MatKhauBam = auth.Bam(taiKhoan, yc.MatKhau);
        db.TaiKhoan.Add(taiKhoan);
        await db.SaveChangesAsync(ct);
        return (KetQuaQuanLyGiaoXu.ThanhCong, taiKhoan.Id);
    }
}
