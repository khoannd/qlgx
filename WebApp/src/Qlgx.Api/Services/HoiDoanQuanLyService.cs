using Microsoft.EntityFrameworkCore;
using Qlgx.Api.Dtos;
using Qlgx.Data;
using Qlgx.Domain.Entities;

namespace Qlgx.Api.Services;

public enum KetQuaLuuHoiDoan { ThanhCong, KhongTimThay, DungPhienBan }
public enum KetQuaThemThanhVienHoiDoan { ThanhCong, KhongTimThayHoiDoan, KhongTimThayGiaoDan, DaOTrongHoiDoan }
public enum KetQuaSuaThanhVienHoiDoan { ThanhCong, KhongTimThay, DungPhienBan }

/// <summary>
/// Dịch vụ cho màn hình "Danh sách hội đoàn" (quản trị danh mục — frmHoiDoanList.cs +
/// frmHoiDoan.cs), KHÁC hẳn tab "Hội đoàn" của chi tiết giáo dân (GiaoDanService.*HoiDoan*,
/// xem hoi-doan.md). Bảng HoiDoan/ChiTietHoiDoan rỗng ở giáo xứ khảo sát (Vô Nhiễm) — xem
/// docs/superpowers/specs/man-hinh/hoi-doan-danh-sach.md.
/// </summary>
public class HoiDoanQuanLyService(QlgxDbContext db, SinhMaService sinhMa, IBoiCanhGiaoXu boiCanh)
{
    public async Task<List<HoiDoanQuanLyDto>> LayDanhSach(CancellationToken ct)
    {
        var ds = await db.HoiDoan
            .Select(h => new
            {
                h.Id, h.MaHoiDoanCu, h.TenHoiDoan, h.ThanhBonMang, h.NgayBonMang, h.NgayThanhLap,
                h.GhiChu, h.RowVersion,
                SoHoiVien = db.ChiTietHoiDoan.Count(c => c.HoiDoanId == h.Id && c.NgayRaHoiDoan == null),
            })
            .OrderBy(h => h.MaHoiDoanCu)
            .ToListAsync(ct);
        return ds.Select(h => new HoiDoanQuanLyDto(
            h.Id, h.MaHoiDoanCu, h.TenHoiDoan, h.ThanhBonMang, h.NgayBonMang, h.NgayThanhLap,
            h.GhiChu, h.SoHoiVien, h.RowVersion)).ToList();
    }

    public async Task<Guid> Them(LuuHoiDoanRequest yc, CancellationToken ct)
    {
        var giaoXuId = boiCanh.GiaoXuId;
        var hd = new HoiDoan
        {
            GiaoXuId = giaoXuId,
            MaHoiDoanCu = await sinhMa.LayMaTiepTheo(giaoXuId, "hoi_doan",
                await db.HoiDoan.MaxAsync(x => (int?)x.MaHoiDoanCu, ct) ?? 0, ct),
        };
        GanTuYeuCau(hd, yc);
        db.HoiDoan.Add(hd);
        await db.SaveChangesAsync(ct);
        return hd.Id;
    }

    public async Task<KetQuaLuuHoiDoan> Sua(Guid id, LuuHoiDoanRequest yc, CancellationToken ct)
    {
        var hd = await db.HoiDoan.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (hd is null) return KetQuaLuuHoiDoan.KhongTimThay;
        if (yc.RowVersion is { } rv && hd.RowVersion != rv) return KetQuaLuuHoiDoan.DungPhienBan;
        GanTuYeuCau(hd, yc);
        await db.SaveChangesAsync(ct);
        return KetQuaLuuHoiDoan.ThanhCong;
    }

    private static void GanTuYeuCau(HoiDoan hd, LuuHoiDoanRequest yc)
    {
        hd.TenHoiDoan = yc.TenHoiDoan;
        hd.ThanhBonMang = yc.ThanhBonMang;
        hd.NgayBonMang = yc.NgayBonMang;
        hd.NgayThanhLap = yc.NgayThanhLap;
        hd.GhiChu = yc.GhiChu;
    }

    /// <summary>Xoá cả hội đoàn lẫn toàn bộ hội viên của nó — khớp `gxAddEdit1_DeleteClick`
    /// (frmHoiDoanList.cs:26-45: xoá ChiTietHoiDoan rồi HoiDoan). Ở web, việc xoá ChiTietHoiDoan
    /// đã do ràng buộc khoá ngoại ON DELETE CASCADE đảm nhiệm (ChiTietHoiDoanConfig.cs) — không
    /// cần xoá tay. Khác màn hình Giáo họ (không có nút xoá vì cascade chạm tới Giáo dân/Gia
    /// đình — rủi ro mất dữ liệu cốt lõi): ở đây "bán kính nổ" chỉ giới hạn trong chính hội
    /// đoàn/hội viên của nó, không đụng tới bản ghi Giáo dân/Gia đình gốc nào.</summary>
    public async Task<bool> Xoa(Guid id, CancellationToken ct)
    {
        var hd = await db.HoiDoan.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (hd is null) return false;
        db.HoiDoan.Remove(hd);
        await db.SaveChangesAsync(ct);
        return true;
    }

    /// <summary><paramref name="chiXemHienTai"/> khớp checkbox "Thống kê" ĐẢO NGƯỢC
    /// (`cbThongKe`, frmHoiDoan.cs:634-642): bỏ tick (mặc định) = chỉ hội viên hiện tại
    /// (`SELECT_LIST_HOIVIEN_HOIDOAN`), tick = toàn bộ lịch sử kể cả người đã ra
    /// (`SELECT_LIST_HISTORY_HOIVIEN_HOIDOAN`).</summary>
    public Task<List<ThanhVienHoiDoanDto>> LayThanhVien(Guid hoiDoanId, bool chiXemHienTai, CancellationToken ct)
    {
        var truyVan = db.ChiTietHoiDoan.Where(c => c.HoiDoanId == hoiDoanId);
        if (chiXemHienTai) truyVan = truyVan.Where(c => c.NgayRaHoiDoan == null);
        return truyVan.Include(c => c.GiaoDan)
            .OrderByDescending(c => c.NgayVaoHoiDoan)
            .Select(c => new ThanhVienHoiDoanDto(
                c.Id, c.GiaoDanId, c.GiaoDan!.HoTen, c.GiaoDan.TenThanh,
                c.NgayVaoHoiDoan, c.NgayRaHoiDoan, c.VaiTro,
                c.NgayRaHoiDoan != null, c.RowVersion))
            .ToListAsync(ct);
    }

    /// <summary>Thêm một giáo dân có sẵn làm hội viên — khớp `insertDataGrid`
    /// (frmHoiDoan.cs:470-509): chặn nếu giáo dân này ĐANG hoạt động (chưa ra) trong CHÍNH hội
    /// đoàn này (thông báo gốc "đã tồn tại trong hội đoàn rồi", frmHoiDoan.cs:488) — không chặn
    /// nếu đã từng ở rồi ra (lịch sử vào/ra nhiều lần là hợp lệ, xem GxHistoryHoiDoan/hoi-doan.md).</summary>
    public async Task<KetQuaThemThanhVienHoiDoan> ThemThanhVien(
        Guid hoiDoanId, ThemThanhVienHoiDoanRequest yc, CancellationToken ct)
    {
        var hd = await db.HoiDoan.FirstOrDefaultAsync(x => x.Id == hoiDoanId, ct);
        if (hd is null) return KetQuaThemThanhVienHoiDoan.KhongTimThayHoiDoan;
        var gd = await db.GiaoDan.FirstOrDefaultAsync(x => x.Id == yc.GiaoDanId && !x.DaXoa, ct);
        if (gd is null) return KetQuaThemThanhVienHoiDoan.KhongTimThayGiaoDan;

        var dangHoatDong = await db.ChiTietHoiDoan.AnyAsync(
            c => c.HoiDoanId == hoiDoanId && c.GiaoDanId == yc.GiaoDanId && c.NgayRaHoiDoan == null, ct);
        if (dangHoatDong) return KetQuaThemThanhVienHoiDoan.DaOTrongHoiDoan;

        var ct2 = new ChiTietHoiDoan
        {
            GiaoXuId = boiCanh.GiaoXuId, HoiDoanId = hoiDoanId, GiaoDanId = yc.GiaoDanId,
            MaChiTietHoiDoanCu = await sinhMa.LayMaTiepTheo(boiCanh.GiaoXuId, "chi_tiet_hoi_doan",
                await db.ChiTietHoiDoan.MaxAsync(x => (int?)x.MaChiTietHoiDoanCu, ct) ?? 0, ct),
            NgayVaoHoiDoan = yc.NgayVaoHoiDoan, NgayRaHoiDoan = yc.NgayRaHoiDoan,
            VaiTro = string.IsNullOrWhiteSpace(yc.VaiTro) ? "Hội viên" : yc.VaiTro,
        };
        db.ChiTietHoiDoan.Add(ct2);
        await db.SaveChangesAsync(ct);
        return KetQuaThemThanhVienHoiDoan.ThanhCong;
    }

    /// <summary>Sửa ngày vào/ra/vai trò của một hội viên đã có — bản web MỞ RỘNG có chủ đích so
    /// với `frmHoiDoan` (sửa trực tiếp trên lưới, không qua form riêng) để đơn giản hoá thao
    /// tác, xem can-review-sau.md.</summary>
    public async Task<KetQuaSuaThanhVienHoiDoan> SuaThanhVien(
        Guid chiTietId, SuaThanhVienHoiDoanRequest yc, CancellationToken ct)
    {
        var ct2 = await db.ChiTietHoiDoan.FirstOrDefaultAsync(x => x.Id == chiTietId, ct);
        if (ct2 is null) return KetQuaSuaThanhVienHoiDoan.KhongTimThay;
        if (ct2.RowVersion != yc.RowVersion) return KetQuaSuaThanhVienHoiDoan.DungPhienBan;

        ct2.NgayVaoHoiDoan = yc.NgayVaoHoiDoan;
        ct2.NgayRaHoiDoan = yc.NgayRaHoiDoan;
        ct2.VaiTro = yc.VaiTro;
        await db.SaveChangesAsync(ct);
        return KetQuaSuaThanhVienHoiDoan.ThanhCong;
    }

    /// <summary>Xoá vĩnh viễn một hội viên khỏi hội đoàn — khớp nhánh [Yes] của hộp thoại 3 nút
    /// `gxAddEdit1_DeleteClick` (frmHoiDoan.cs:576-589: "xóa" hẳn khỏi lưới, khác nhánh [No] chỉ
    /// gán Ngày ra hội đoàn = hôm nay — nhánh đó bản web thực hiện bằng cách SỬA
    /// NgayRaHoiDoan qua `SuaThanhVien` ở trên, không cần endpoint riêng).</summary>
    public async Task<bool> XoaThanhVien(Guid chiTietId, CancellationToken ct)
    {
        var ct2 = await db.ChiTietHoiDoan.FirstOrDefaultAsync(x => x.Id == chiTietId, ct);
        if (ct2 is null) return false;
        db.ChiTietHoiDoan.Remove(ct2);
        await db.SaveChangesAsync(ct);
        return true;
    }
}
