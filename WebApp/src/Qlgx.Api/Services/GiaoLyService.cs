using Microsoft.EntityFrameworkCore;
using Qlgx.Api.Dtos;
using Qlgx.Data;
using Qlgx.Domain.Entities;

namespace Qlgx.Api.Services;

public enum KetQuaLuuKhoi { ThanhCong, KhongTimThay, DungPhienBan, KhongTimThayNguoiQuanLy }
public enum KetQuaLuuLop { ThanhCong, KhongTimThay, DungPhienBan }
public enum KetQuaThemHocVien { ThanhCong, KhongTimThayLop, KhongTimThayGiaoDan, DaCoTrongDanhSach, DaThuocLopKhac }
public enum KetQuaSuaHocVien { ThanhCong, KhongTimThay, DungPhienBan }
public enum KetQuaThemGiaoLyVien { ThanhCong, KhongTimThayLop, KhongTimThayGiaoDan, DaCoTrongDanhSach }

/// <summary>
/// Dịch vụ cho phân hệ Giáo lý (Khối → Lớp → Học viên/Giáo lý viên) — khớp
/// `Source/Giaoly/frmKhoiGiaoLyList.cs` + `frmKhoiGiaoLy.cs` + `frmLopGiaoLy.cs`. Bốn bảng rỗng
/// ở giáo xứ khảo sát (Vô Nhiễm) — xem docs/superpowers/specs/man-hinh/giao-ly.md.
/// </summary>
public class GiaoLyService(QlgxDbContext db, SinhMaService sinhMa, IBoiCanhGiaoXu boiCanh)
{
    // --- Khối giáo lý ---------------------------------------------------------------------

    public async Task<List<KhoiGiaoLyDto>> LayDanhSachKhoi(CancellationToken ct)
    {
        var ds = await db.KhoiGiaoLy
            .Select(k => new
            {
                k.Id, k.MaKhoiCu, k.TenKhoi, k.NguoiQuanLyId,
                TenNguoiQuanLy = k.NguoiQuanLy == null ? null
                    : (k.NguoiQuanLy.TenThanh != null ? k.NguoiQuanLy.TenThanh + " " : "") + k.NguoiQuanLy.HoTen,
                k.GhiChu, k.RowVersion,
                SoLop = db.LopGiaoLy.Count(l => l.KhoiGiaoLyId == k.Id),
            })
            .OrderBy(k => k.MaKhoiCu)
            .ToListAsync(ct);
        return ds.Select(k => new KhoiGiaoLyDto(
            k.Id, k.MaKhoiCu, k.TenKhoi, k.NguoiQuanLyId, k.TenNguoiQuanLy, k.GhiChu, k.SoLop, k.RowVersion)).ToList();
    }

    public async Task<(Guid? id, KetQuaLuuKhoi ketQua)> ThemKhoi(LuuKhoiGiaoLyRequest yc, CancellationToken ct)
    {
        var giaoXuId = boiCanh.GiaoXuId;
        var nguoiQuanLy = await db.GiaoDan.FirstOrDefaultAsync(x => x.Id == yc.NguoiQuanLyId, ct);
        if (nguoiQuanLy is null) return (null, KetQuaLuuKhoi.KhongTimThayNguoiQuanLy);
        var k = new KhoiGiaoLy
        {
            GiaoXuId = giaoXuId,
            MaKhoiCu = await sinhMa.LayMaTiepTheo(giaoXuId, "khoi_giao_ly",
                await db.KhoiGiaoLy.MaxAsync(x => (int?)x.MaKhoiCu, ct) ?? 0, ct),
        };
        GanKhoiTuYeuCau(k, yc);
        db.KhoiGiaoLy.Add(k);
        await db.SaveChangesAsync(ct);
        return (k.Id, KetQuaLuuKhoi.ThanhCong);
    }

    public async Task<KetQuaLuuKhoi> SuaKhoi(Guid id, LuuKhoiGiaoLyRequest yc, CancellationToken ct)
    {
        var k = await db.KhoiGiaoLy.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (k is null) return KetQuaLuuKhoi.KhongTimThay;
        if (yc.RowVersion is { } rv && k.RowVersion != rv) return KetQuaLuuKhoi.DungPhienBan;
        var nguoiQuanLy = await db.GiaoDan.FirstOrDefaultAsync(x => x.Id == yc.NguoiQuanLyId, ct);
        if (nguoiQuanLy is null) return KetQuaLuuKhoi.KhongTimThayNguoiQuanLy;
        GanKhoiTuYeuCau(k, yc);
        await db.SaveChangesAsync(ct);
        return KetQuaLuuKhoi.ThanhCong;
    }

    private static void GanKhoiTuYeuCau(KhoiGiaoLy k, LuuKhoiGiaoLyRequest yc)
    {
        k.TenKhoi = yc.TenKhoi;
        k.NguoiQuanLyId = yc.NguoiQuanLyId;
        k.GhiChu = yc.GhiChu;
    }

    /// <summary>Xoá cả khối lẫn toàn bộ lớp/học viên/giáo lý viên của nó — khớp Ý ĐỊNH của
    /// `gxAddEdit1_DeleteClick` (frmKhoiGiaoLyList.cs:120-152: xoá ChiTietLopGiaoLy rồi
    /// LopGiaoLy rồi KhoiGiaoLy). Khoá ngoại LopGiaoLy→KhoiGiaoLy là Restrict (không cascade,
    /// xem LopGiaoLyConfig.cs) nên phải tự xoá từng LopGiaoLy trước — mỗi lần xoá một LopGiaoLy
    /// tự kéo cascade xoá ChiTietLopGiaoLy+GiaoLyVien của riêng lớp đó (xem giao-ly.md mục 4).
    /// </summary>
    public async Task<bool> XoaKhoi(Guid id, CancellationToken ct)
    {
        var k = await db.KhoiGiaoLy.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (k is null) return false;
        await using var tran = await db.Database.BeginTransactionAsync(ct);
        var lopCuaKhoi = await db.LopGiaoLy.Where(l => l.KhoiGiaoLyId == id).ToListAsync(ct);
        db.LopGiaoLy.RemoveRange(lopCuaKhoi);
        db.KhoiGiaoLy.Remove(k);
        await db.SaveChangesAsync(ct);
        await tran.CommitAsync(ct);
        return true;
    }

    // --- Lớp giáo lý ------------------------------------------------------------------------

    public async Task<List<LopGiaoLyDto>> LayDanhSachLop(Guid khoiId, int? nam, CancellationToken ct)
    {
        var truyVan = db.LopGiaoLy.Where(l => l.KhoiGiaoLyId == khoiId);
        if (nam is { } n) truyVan = truyVan.Where(l => l.Nam == n);
        var ds = await truyVan
            .Select(l => new
            {
                l.Id, l.MaLopCu, l.TenLop, l.KhoiGiaoLyId, l.Nam, l.PhongHoc, l.GhiChu, l.RowVersion,
                SoHocVien = db.ChiTietLopGiaoLy.Count(c => c.LopGiaoLyId == l.Id),
                TenGiaoLyVien = string.Join(", ", db.GiaoLyVien.Where(g => g.LopGiaoLyId == l.Id)
                    .Select(g => (g.GiaoDan!.TenThanh != null ? g.GiaoDan.TenThanh + " " : "") + g.GiaoDan.HoTen)),
            })
            .OrderBy(l => l.MaLopCu)
            .ToListAsync(ct);
        return ds.Select(l => new LopGiaoLyDto(
            l.Id, l.MaLopCu, l.TenLop, l.KhoiGiaoLyId, l.Nam, l.PhongHoc, l.GhiChu, l.SoHocVien,
            string.IsNullOrEmpty(l.TenGiaoLyVien) ? null : l.TenGiaoLyVien, l.RowVersion)).ToList();
    }

    public async Task<(Guid? id, KetQuaLuuLop ketQua)> ThemLop(Guid khoiId, LuuLopGiaoLyRequest yc, CancellationToken ct)
    {
        var khoi = await db.KhoiGiaoLy.FirstOrDefaultAsync(x => x.Id == khoiId, ct);
        if (khoi is null) return (null, KetQuaLuuLop.KhongTimThay);
        var giaoXuId = boiCanh.GiaoXuId;
        var l = new LopGiaoLy
        {
            GiaoXuId = giaoXuId, KhoiGiaoLyId = khoiId,
            MaLopCu = await sinhMa.LayMaTiepTheo(giaoXuId, "lop_giao_ly",
                await db.LopGiaoLy.MaxAsync(x => (int?)x.MaLopCu, ct) ?? 0, ct),
        };
        GanLopTuYeuCau(l, yc);
        db.LopGiaoLy.Add(l);
        await db.SaveChangesAsync(ct);
        return (l.Id, KetQuaLuuLop.ThanhCong);
    }

    public async Task<KetQuaLuuLop> SuaLop(Guid id, LuuLopGiaoLyRequest yc, CancellationToken ct)
    {
        var l = await db.LopGiaoLy.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (l is null) return KetQuaLuuLop.KhongTimThay;
        if (yc.RowVersion is { } rv && l.RowVersion != rv) return KetQuaLuuLop.DungPhienBan;
        GanLopTuYeuCau(l, yc);
        await db.SaveChangesAsync(ct);
        return KetQuaLuuLop.ThanhCong;
    }

    private static void GanLopTuYeuCau(LopGiaoLy l, LuuLopGiaoLyRequest yc)
    {
        l.TenLop = yc.TenLop;
        l.Nam = yc.Nam;
        l.PhongHoc = yc.PhongHoc;
        l.GhiChu = yc.GhiChu;
    }

    /// <summary>Xoá lớp — cascade xoá ChiTietLopGiaoLy+GiaoLyVien qua khoá ngoại (đã cấu hình
    /// Cascade — xem ChiTietLopGiaoLyConfig.cs/GiaoLyVienConfig.cs), khớp ý định
    /// `frmKhoiGiaoLy.gxAddEdit1_DeleteClick` (dòng 199-224).</summary>
    public async Task<bool> XoaLop(Guid id, CancellationToken ct)
    {
        var l = await db.LopGiaoLy.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (l is null) return false;
        db.LopGiaoLy.Remove(l);
        await db.SaveChangesAsync(ct);
        return true;
    }

    // --- Học viên ---------------------------------------------------------------------------

    public Task<List<HocVienLopGiaoLyDto>> LayHocVien(Guid lopId, CancellationToken ct) =>
        db.ChiTietLopGiaoLy.Where(c => c.LopGiaoLyId == lopId)
            .Include(c => c.GiaoDan)
            .OrderBy(c => c.SoThuTu)
            .Select(c => new HocVienLopGiaoLyDto(
                c.Id, c.GiaoDanId, c.SoThuTu, c.GiaoDan!.HoTen, c.GiaoDan.TenThanh, c.GiaoDan.Phai,
                c.GiaoDan.NgaySinh, c.HoanThanh, c.GhiChuGLy, c.RowVersion))
            .ToListAsync(ct);

    /// <summary>Thêm một giáo dân có sẵn làm học viên — khớp `addGiaoDan`
    /// (frmLopGiaoLy.cs:243-322): chặn nếu đã có trong lớp NÀY, chặn nếu đã thuộc một lớp KHÁC
    /// TRONG CÙNG KHỐI (không chặn khác khối — xem giao-ly.md mục 4). Không migrate cảnh báo mềm
    /// qua đời/chuyển xứ/xoá mềm (thu hẹp phạm vi có chủ đích, cùng tinh thần hội đoàn — xem mục
    /// 8, ghi vào can-review-sau.md).</summary>
    public async Task<KetQuaThemHocVien> ThemHocVien(Guid lopId, ThemHocVienRequest yc, CancellationToken ct)
    {
        var lop = await db.LopGiaoLy.FirstOrDefaultAsync(x => x.Id == lopId, ct);
        if (lop is null) return KetQuaThemHocVien.KhongTimThayLop;
        var gd = await db.GiaoDan.FirstOrDefaultAsync(x => x.Id == yc.GiaoDanId, ct);
        if (gd is null) return KetQuaThemHocVien.KhongTimThayGiaoDan;

        if (await db.ChiTietLopGiaoLy.AnyAsync(c => c.LopGiaoLyId == lopId && c.GiaoDanId == yc.GiaoDanId, ct))
            return KetQuaThemHocVien.DaCoTrongDanhSach;

        var thuocLopKhacCungKhoi = await db.ChiTietLopGiaoLy
            .Where(c => c.GiaoDanId == yc.GiaoDanId)
            .Join(db.LopGiaoLy, c => c.LopGiaoLyId, l => l.Id, (c, l) => l)
            .AnyAsync(l => l.KhoiGiaoLyId == lop.KhoiGiaoLyId, ct);
        if (thuocLopKhacCungKhoi) return KetQuaThemHocVien.DaThuocLopKhac;

        var soThuTuKeTiep = (await db.ChiTietLopGiaoLy
            .Where(c => c.LopGiaoLyId == lopId)
            .MaxAsync(c => (int?)c.SoThuTu, ct) ?? 0) + 1;
        db.ChiTietLopGiaoLy.Add(new ChiTietLopGiaoLy
        {
            GiaoXuId = boiCanh.GiaoXuId, LopGiaoLyId = lopId, GiaoDanId = yc.GiaoDanId,
            SoThuTu = soThuTuKeTiep, HoanThanh = false,
        });
        await db.SaveChangesAsync(ct);
        return KetQuaThemHocVien.ThanhCong;
    }

    public async Task<KetQuaSuaHocVien> SuaHocVien(Guid chiTietId, SuaHocVienRequest yc, CancellationToken ct)
    {
        var c = await db.ChiTietLopGiaoLy.FirstOrDefaultAsync(x => x.Id == chiTietId, ct);
        if (c is null) return KetQuaSuaHocVien.KhongTimThay;
        if (c.RowVersion != yc.RowVersion) return KetQuaSuaHocVien.DungPhienBan;
        c.SoThuTu = yc.SoThuTu;
        c.HoanThanh = yc.HoanThanh;
        c.GhiChuGLy = yc.GhiChuGLy;
        await db.SaveChangesAsync(ct);
        return KetQuaSuaHocVien.ThanhCong;
    }

    /// <summary>Xoá vĩnh viễn một học viên khỏi lớp — khớp `gxAddEdit1_DeleteClick` của
    /// `frmLopGiaoLy` (dòng 421-442): chỉ VÀO/RA hẳn, không có "đánh dấu đã ra" giữ lịch sử như
    /// hội viên hội đoàn (xem giao-ly.md mục 4).</summary>
    public async Task<bool> XoaHocVien(Guid chiTietId, CancellationToken ct)
    {
        var c = await db.ChiTietLopGiaoLy.FirstOrDefaultAsync(x => x.Id == chiTietId, ct);
        if (c is null) return false;
        db.ChiTietLopGiaoLy.Remove(c);
        await db.SaveChangesAsync(ct);
        return true;
    }

    // --- Giáo lý viên -----------------------------------------------------------------------

    public Task<List<GiaoLyVienDto>> LayGiaoLyVien(Guid lopId, CancellationToken ct) =>
        db.GiaoLyVien.Where(g => g.LopGiaoLyId == lopId)
            .Include(g => g.GiaoDan)
            .Select(g => new GiaoLyVienDto(g.Id, g.GiaoDanId, g.GiaoDan!.HoTen, g.GiaoDan.TenThanh, g.RowVersion))
            .ToListAsync(ct);

    /// <summary>Thêm một giáo dân có sẵn làm giáo lý viên — khớp `addGiaoLyVien`
    /// (frmLopGiaoLy.cs:386-427): chỉ chặn trùng trong CHÍNH lớp này, không kiểm tra qua
    /// đời/chuyển xứ như học viên (không nhất quán ở bản gốc — giữ nguyên, xem giao-ly.md mục
    /// 8), không chặn một giáo lý viên dạy nhiều lớp.</summary>
    public async Task<KetQuaThemGiaoLyVien> ThemGiaoLyVien(Guid lopId, ThemGiaoLyVienRequest yc, CancellationToken ct)
    {
        var lop = await db.LopGiaoLy.FirstOrDefaultAsync(x => x.Id == lopId, ct);
        if (lop is null) return KetQuaThemGiaoLyVien.KhongTimThayLop;
        var gd = await db.GiaoDan.FirstOrDefaultAsync(x => x.Id == yc.GiaoDanId, ct);
        if (gd is null) return KetQuaThemGiaoLyVien.KhongTimThayGiaoDan;
        if (await db.GiaoLyVien.AnyAsync(g => g.LopGiaoLyId == lopId && g.GiaoDanId == yc.GiaoDanId, ct))
            return KetQuaThemGiaoLyVien.DaCoTrongDanhSach;

        db.GiaoLyVien.Add(new GiaoLyVien { GiaoXuId = boiCanh.GiaoXuId, LopGiaoLyId = lopId, GiaoDanId = yc.GiaoDanId });
        await db.SaveChangesAsync(ct);
        return KetQuaThemGiaoLyVien.ThanhCong;
    }

    /// <summary>Xoá giáo lý viên — khớp `gxAddEdit2_DeleteClick` (dòng 665-682): KHÔNG có hộp
    /// xác nhận ở bản gốc (khác học viên/khối/lớp) — giữ nguyên, xem giao-ly.md mục 4.</summary>
    public async Task<bool> XoaGiaoLyVien(Guid id, CancellationToken ct)
    {
        var g = await db.GiaoLyVien.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (g is null) return false;
        db.GiaoLyVien.Remove(g);
        await db.SaveChangesAsync(ct);
        return true;
    }

    // --- "Chuyển lớp" hàng loạt (frmChuyenLop.cs:140-202, hoãn từ commit d4c4b27) --------------

    /// <summary>Bước xem trước bắt buộc — bản gốc KHÔNG có bước này (lưới đã chọn sẵn rồi ghi
    /// thẳng khi bấm "&amp;Chuyển"), đây là yêu cầu an toàn của nhiệm vụ, cùng khuôn với
    /// ChuyenHoService. Không ghi gì, chỉ đếm số liệu THẬT tại thời điểm gọi. `chiTietIds` phải
    /// cùng thuộc một lớp nguồn (suy từ chính các dòng được chọn) — khác lớp nguồn thì coi như
    /// không hợp lệ (null, ánh xạ 404 ở endpoint).</summary>
    public async Task<ChuyenLopXemTruoc?> XemTruocChuyenLop(List<Guid> chiTietIds, Guid lopDichId, CancellationToken ct)
    {
        var lopDich = await db.LopGiaoLy.Include(l => l.KhoiGiaoLy).FirstOrDefaultAsync(l => l.Id == lopDichId, ct);
        if (lopDich is null) return null;

        var chon = await db.ChiTietLopGiaoLy.Where(c => chiTietIds.Contains(c.Id))
            .Select(c => new { c.LopGiaoLyId, c.GiaoDanId }).ToListAsync(ct);
        if (chon.Count == 0) return null;
        var lopNguonIds = chon.Select(c => c.LopGiaoLyId).Distinct().ToList();
        if (lopNguonIds.Count != 1) return null;
        var lopNguon = await db.LopGiaoLy.FirstOrDefaultAsync(l => l.Id == lopNguonIds[0], ct);
        if (lopNguon is null) return null;

        var giaoDanIdsChon = chon.Select(c => c.GiaoDanId).Distinct().ToList();
        var daCoODich = await db.ChiTietLopGiaoLy
            .Where(c => c.LopGiaoLyId == lopDichId && giaoDanIdsChon.Contains(c.GiaoDanId))
            .Select(c => c.GiaoDanId).Distinct().CountAsync(ct);

        return new ChuyenLopXemTruoc(
            chon.Count, giaoDanIdsChon.Count - daCoODich, daCoODich,
            lopNguon.TenLop, lopDich.TenLop, lopDich.KhoiGiaoLy!.TenKhoi, lopDich.Nam);
    }

    /// <summary>Ghi thật, trong MỘT transaction (yêu cầu an toàn bắt buộc của nhiệm vụ — bản gốc
    /// chỉ gọi <c>Memory.UpdateDataSet(ds)</c> một lần cho cả loạt dòng mới, không có transaction
    /// rõ ràng, có thể dở dang nếu lỗi giữa chừng). KHÔNG xoá học viên khỏi lớp NGUỒN — đúng
    /// bug-for-bug của <c>frmChuyenLop.cs</c>: tên chức năng là "Chuyển lớp" nhưng mã gốc
    /// (<c>gxCommand1_OnOK</c>, dòng 140-202) chỉ THÊM một dòng <c>ChiTietLopGiaoLy</c> mới vào
    /// lớp đích cho mỗi học viên được chọn, không hề đụng tới dòng ở lớp nguồn — một học viên
    /// "chuyển lớp" xong vẫn còn nguyên trong danh sách lớp cũ (ghi ở can-review-sau.md). Học
    /// viên nào đã có sẵn trong lớp đích (theo <c>GiaoDanId</c>) bị BỎ QUA — đúng nhánh
    /// <c>MessageBox.Show("... đã tồn tại trong lớp ...")</c> của bản gốc, chỉ bỏ người đó chứ
    /// không chặn cả thao tác. <c>SoThuTu</c> của các dòng mới nối tiếp từ MAX hiện có ở lớp
    /// đích, đúng biến <c>soThuTuNext</c> gốc (dòng 172-179). Không kiểm tra "đã thuộc lớp khác
    /// trong cùng khối" như <see cref="ThemHocVien"/> — bản gốc không kiểm tra gì ở đường ghi
    /// hàng loạt này (khác đường thêm-từng-người addGiaoDan), cố tình KHÔNG mở rộng phạm vi theo
    /// đúng chỉ đạo nhiệm vụ.
    ///
    /// SỬA (review-toan-nhanh-dulieu.md mục T1): bản gốc trước bản sửa này thiếu đúng phép kiểm
    /// "chiTietIds phải cùng thuộc MỘT lớp nguồn duy nhất" mà <see cref="XemTruocChuyenLop"/> đã
    /// có (dòng ~283-284) — gọi thẳng endpoint (không qua giao diện, vốn chỉ cho chọn trong một
    /// lớp) có thể trộn học viên từ nhiều lớp/khối khác nhau vào một lần ghi mà không qua đúng
    /// bước xác nhận số liệu đã xem trước. Thêm đúng cùng phép kiểm, tính từ chính danh sách vừa
    /// truy vấn (không phải từ tham số riêng) — khác lớp nguồn thì coi như yêu cầu không hợp lệ
    /// (null, ánh xạ 404 ở endpoint, giống XemTruocChuyenLop).</summary>
    public async Task<ChuyenLopKetQua?> ChuyenLop(List<Guid> chiTietIds, Guid lopDichId, CancellationToken ct)
    {
        await using var giaoTac = await db.Database.BeginTransactionAsync(ct);

        if (!await db.LopGiaoLy.AnyAsync(l => l.Id == lopDichId, ct)) return null;

        var chiTietChon = await db.ChiTietLopGiaoLy.Where(c => chiTietIds.Contains(c.Id))
            .Select(c => new { c.LopGiaoLyId, c.GiaoDanId }).ToListAsync(ct);
        if (chiTietChon.Count == 0) return new ChuyenLopKetQua(0);
        var lopNguonIds = chiTietChon.Select(c => c.LopGiaoLyId).Distinct().ToList();
        if (lopNguonIds.Count != 1) return null;

        var chon = chiTietChon.Select(c => c.GiaoDanId).Distinct().ToList();

        var daCoODich = await db.ChiTietLopGiaoLy
            .Where(c => c.LopGiaoLyId == lopDichId && chon.Contains(c.GiaoDanId))
            .Select(c => c.GiaoDanId).ToListAsync(ct);
        var canThem = chon.Except(daCoODich).ToList();

        var soThuTuKeTiep = await db.ChiTietLopGiaoLy
            .Where(c => c.LopGiaoLyId == lopDichId).MaxAsync(c => (int?)c.SoThuTu, ct) ?? 0;

        var giaoXuId = boiCanh.GiaoXuId;
        foreach (var giaoDanId in canThem)
        {
            soThuTuKeTiep++;
            db.ChiTietLopGiaoLy.Add(new ChiTietLopGiaoLy
            {
                GiaoXuId = giaoXuId, LopGiaoLyId = lopDichId, GiaoDanId = giaoDanId,
                SoThuTu = soThuTuKeTiep, HoanThanh = false, GhiChuGLy = "",
            });
        }
        await db.SaveChangesAsync(ct);
        await giaoTac.CommitAsync(ct);
        return new ChuyenLopKetQua(canThem.Count);
    }
}
