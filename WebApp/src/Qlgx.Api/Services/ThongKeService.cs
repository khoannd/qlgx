using Microsoft.EntityFrameworkCore;
using Qlgx.Api.Dtos;
using Qlgx.Data;
using Qlgx.Domain;
using Qlgx.Domain.Entities;

namespace Qlgx.Api.Services;

/// <summary>
/// Tab "Thống kê chung" (`GxThongKeChung.cs`) — xem
/// docs/superpowers/specs/man-hinh/thong-ke-bieu-do.md cho công thức đầy đủ của 16 điều kiện,
/// TRÍCH DẪN dòng mã gốc cho từng nhánh bên dưới. Nguyên tắc migrate: Y HỆT bản gốc kể cả những
/// chỗ desktop tính sai (cận tuổi đảo ngược ở mục 4.3) — KHÔNG tự sửa, chỉ ghi vào
/// can-review-sau.md.
/// </summary>
public class ThongKeService(QlgxDbContext db, GiaDinhService giaDinhService)
{
    // ---- Hằng số tuổi cố định (GxConstants.cs, đã đọc trực tiếp UTF-16LE) ----
    public const int TuoiCaoNien = 60;
    public const int TuoiTreTu = 18;
    public const int TuoiTreDen = 30;
    public const int TuoiThieuNhiTu = 5;
    public const int TuoiThieuNhiDen = 17;

    public async Task<ThongKeChungKetQua> LayThongKe(
        DieuKienThongKe dieuKien, Guid? giaoHoId, DateOnly? tuNgay, DateOnly? denNgay,
        int? tuTuoi, int? denTuoi, bool luuTru, bool khongCoNgay,
        TrangThaiHonPhoiThongKe? trangThaiHonPhoi, CancellationToken ct)
    {
        switch (dieuKien)
        {
            case DieuKienThongKe.TongSoGiaDinh:
                return await ThongKeGiaDinh(giaoHoId, luuTru, ct);

            case DieuKienThongKe.HonPhoi:
                return await ThongKeHonPhoi(giaoHoId, tuNgay, denNgay, khongCoNgay, trangThaiHonPhoi,
                    theoNgayThangKhongTheoNam: false, ct);
            case DieuKienThongKe.KyNiemHonPhoi:
                return await ThongKeHonPhoi(giaoHoId, tuNgay, denNgay, khongCoNgay, trangThaiHonPhoi,
                    theoNgayThangKhongTheoNam: true, ct);

            case DieuKienThongKe.ChuHo:
            case DieuKienThongKe.GiaTruong:
            case DieuKienThongKe.HienMau:
                return await ThongKeTheoVaiTroTuoi(dieuKien, giaoHoId, tuTuoi, denTuoi, luuTru, khongCoNgay, ct);

            case DieuKienThongKe.CaoNien:
            case DieuKienThongKe.GioiTre:
            case DieuKienThongKe.ThieuNhi:
                return await ThongKeTheoTuoi(dieuKien, giaoHoId, tuTuoi, denTuoi, luuTru, ct);

            default:
                return await ThongKeGiaoDanTheoNgay(dieuKien, giaoHoId, tuNgay, denNgay, luuTru, khongCoNgay, ct);
        }
    }

    // ================= Sinh ra / Rửa tội / XTRL / Thêm sức / Qua đời / Tổng số giáo dân / Tân tòng =================
    // GxThongKeChung.cs:199-283
    private async Task<ThongKeChungKetQua> ThongKeGiaoDanTheoNgay(
        DieuKienThongKe dieuKien, Guid? giaoHoId, DateOnly? tuNgay, DateOnly? denNgay,
        bool luuTru, bool khongCoNgay, CancellationToken ct)
    {
        IQueryable<GiaoDan> truyVan;
        string nhan;
        bool coTheKhongCoNgay;

        switch (dieuKien)
        {
            // Nền RIÊNG cho Qua đời: "WHERE QuaDoi<>0", bỏ DaChuyenXu=0/QuaDoi=0 của nền chung
            // (GxThongKeChung.cs:223-229).
            case DieuKienThongKe.QuaDoi:
                truyVan = db.GiaoDan.Where(g => g.QuaDoi);
                if (!luuTru) truyVan = truyVan.Where(g => !g.DaXoa);
                nhan = " người qua đời";
                coTheKhongCoNgay = true;
                break;
            // Nền RIÊNG cho Tân tòng: "WHERE TanTong<>0" (GxThongKeChung.cs:250-256), lọc THEO
            // NgayRuaToi (không phải một cột "ngày tân tòng" riêng).
            case DieuKienThongKe.TanTong:
                truyVan = db.GiaoDan.Where(g => g.TanTong);
                if (!luuTru) truyVan = truyVan.Where(g => !g.DaXoa);
                nhan = " tân tòng được rửa tội";
                coTheKhongCoNgay = true;
                break;
            // Nền CHUNG (GxThongKeChung.cs:201-202) — CÙNG công thức DaChuyenXu mà
            // GiaoDanService.LayDanhSach đã dùng: "DaChuyenXu" của một GIA ĐÌNH, không phải cột
            // riêng trên GiaoDan.
            default:
                truyVan = db.GiaoDan.Where(g => !g.DaXoa);
                if (!luuTru)
                    truyVan = truyVan.Where(g => !g.QuaDoi && !g.GiaDinhThamGia.Any(tv => tv.GiaDinh!.DaChuyenXu));
                nhan = dieuKien switch
                {
                    DieuKienThongKe.SinhRa => " người được sinh ra",
                    DieuKienThongKe.RuaToi => " người được rửa tội",
                    DieuKienThongKe.RuocLeLanDau => " người được xưng tội rước lễ lần đầu",
                    DieuKienThongKe.ThemSuc => " người được thêm sức",
                    DieuKienThongKe.TongSoGiaoDan => " giáo dân đến thời điểm được nhập",
                    _ => "",
                };
                // Chỉ enable được ở SinhRa/TongSoGiaoDan trong EnableChk — RuaToi/RuocLe/ThemSuc
                // không (GxThongKeChung.cs:764-779).
                coTheKhongCoNgay = dieuKien is DieuKienThongKe.SinhRa or DieuKienThongKe.TongSoGiaoDan;
                break;
        }

        bool apDungKhongCoNgay = khongCoNgay && coTheKhongCoNgay;

        truyVan = dieuKien switch
        {
            // "Tổng số giáo dân": GHI ĐÈ từ ngày = đến ngày, so sánh "<=" (luỹ kế), không BETWEEN
            // — GxThongKeChung.cs:241-249.
            DieuKienThongKe.TongSoGiaoDan => denNgay is { } d
                ? truyVan.Where(g => (g.NgaySinh != null && g.NgaySinh <= d) || (apDungKhongCoNgay && g.NgaySinh == null))
                : truyVan.Where(_ => false),
            DieuKienThongKe.SinhRa when tuNgay is { } t1 && denNgay is { } d1 =>
                truyVan.Where(g => (g.NgaySinh != null && g.NgaySinh >= t1 && g.NgaySinh <= d1)
                    || (apDungKhongCoNgay && g.NgaySinh == null)),
            (DieuKienThongKe.RuaToi or DieuKienThongKe.TanTong) when tuNgay is { } t2 && denNgay is { } d2 =>
                truyVan.Where(g => (g.NgayRuaToi != null && g.NgayRuaToi >= t2 && g.NgayRuaToi <= d2)
                    || (apDungKhongCoNgay && g.NgayRuaToi == null)),
            DieuKienThongKe.RuocLeLanDau when tuNgay is { } t3 && denNgay is { } d3 =>
                truyVan.Where(g => (g.NgayRuocLe != null && g.NgayRuocLe >= t3 && g.NgayRuocLe <= d3)
                    || (apDungKhongCoNgay && g.NgayRuocLe == null)),
            DieuKienThongKe.ThemSuc when tuNgay is { } t4 && denNgay is { } d4 =>
                truyVan.Where(g => (g.NgayThemSuc != null && g.NgayThemSuc >= t4 && g.NgayThemSuc <= d4)
                    || (apDungKhongCoNgay && g.NgayThemSuc == null)),
            DieuKienThongKe.QuaDoi when tuNgay is { } t5 && denNgay is { } d5 =>
                truyVan.Where(g => (g.NgayQuaDoi != null && g.NgayQuaDoi >= t5 && g.NgayQuaDoi <= d5)
                    || (apDungKhongCoNgay && g.NgayQuaDoi == null)),
            _ => truyVan.Where(_ => false),
        };

        // Giáo họ (gồm giáo xóm con) + loại "giáo dân ảo" CHỈ khi đã chọn giáo họ cụ thể HOẶC
        // điều kiện là Tổng số giáo dân (GxThongKeChung.cs:268-275) — xem mục 4.2, cố ý migrate y
        // hệt (kể cả chỗ có vẻ không nhất quán).
        if (giaoHoId is { } id)
        {
            truyVan = truyVan.Where(g => g.GiaoHoId == id || (g.GiaoHo != null && g.GiaoHo.GiaoHoChaId == id));
            truyVan = truyVan.Where(g => !g.KhongThongKe);
        }
        else if (dieuKien == DieuKienThongKe.TongSoGiaoDan)
        {
            truyVan = truyVan.Where(g => !g.KhongThongKe);
        }

        var nguon = truyVan.OrderBy(g => g.MaGiaoDanCu).Select(g => new GiaoDanService.NguonDong(g, null,
            g.GiaDinhThamGia.OrderBy(tv => tv.VaiTro).Select(tv => (Guid?)tv.GiaDinhId).FirstOrDefault(),
            g.GiaDinhThamGia.Any(tv => tv.GiaDinh!.DaChuyenXu)));
        var ds = await GiaoDanService.DungDanhSach(nguon).ToListAsync(ct);
        return new ThongKeChungKetQua(ds.Count, nhan, ds, null, null);
    }

    // ================= Tổng số gia đình =================
    private async Task<ThongKeChungKetQua> ThongKeGiaDinh(Guid? giaoHoId, bool luuTru, CancellationToken ct)
    {
        var ds = await giaDinhService.LayThongKeTongSoGiaDinh(giaoHoId, luuTru, ct);
        return new ThongKeChungKetQua(ds.Count, " gia đình", null, ds, null);
    }

    // ================= Hôn phối / Kỷ niệm hôn phối =================
    // GxThongKeChung.cs:286-368. "Kỷ niệm hôn phối" so theo THÁNG+NGÀY, không theo năm.
    private async Task<ThongKeChungKetQua> ThongKeHonPhoi(
        Guid? giaoHoId, DateOnly? tuNgay, DateOnly? denNgay, bool khongCoNgay,
        TrangThaiHonPhoiThongKe? trangThai, bool theoNgayThangKhongTheoNam, CancellationToken ct)
    {
        var truyVan = db.HonPhoi.AsQueryable();
        if (theoNgayThangKhongTheoNam && tuNgay is { } t1 && denNgay is { } d1)
        {
            var tu = t1.Month * 100 + t1.Day;
            var den = d1.Month * 100 + d1.Day;
            truyVan = truyVan.Where(h => h.NgayHonPhoi != null
                && (h.NgayHonPhoi.Value.Month * 100 + h.NgayHonPhoi.Value.Day) >= tu
                && (h.NgayHonPhoi.Value.Month * 100 + h.NgayHonPhoi.Value.Day) <= den);
        }
        else if (tuNgay is { } tu2 && denNgay is { } den2)
        {
            truyVan = khongCoNgay
                ? truyVan.Where(h => h.NgayHonPhoi == null || (h.NgayHonPhoi >= tu2 && h.NgayHonPhoi <= den2))
                : truyVan.Where(h => h.NgayHonPhoi != null && h.NgayHonPhoi >= tu2 && h.NgayHonPhoi <= den2);
        }

        if (trangThai is { } ts && ts != TrangThaiHonPhoiThongKe.KhongPhanLoai)
        {
            var nhan = ts switch
            {
                TrangThaiHonPhoiThongKe.Chuan => "Chuẩn",
                TrangThaiHonPhoiThongKe.HopThucHoa => "Hợp thức hóa",
                TrangThaiHonPhoiThongKe.HopPhap => "Hợp pháp",
                TrangThaiHonPhoiThongKe.LyDi => "Ly dị",
                TrangThaiHonPhoiThongKe.LyThan => "Ly thân",
                _ => "",
            };
            truyVan = truyVan.Where(h => h.CachThucHonPhoi == nhan);
        }

        // Giáo họ: bản Access lọc trên view pivot (cột MaGiaoHo suy từ join, không rõ chính xác
        // vế nào — mục 9 thong-ke-bieu-do.md). Bản web CHỌN lọc theo giáo họ của MỘT TRONG HAI
        // người (chồng HOẶC vợ) — quyết định MỚI có ghi chú, xem can-review-sau.md.
        if (giaoHoId is { } gh)
        {
            truyVan = truyVan.Where(h => h.GiaoDanThamGia.Any(k =>
                k.GiaoDan!.GiaoHoId == gh || (k.GiaoDan.GiaoHo != null && k.GiaoDan.GiaoHo.GiaoHoChaId == gh)));
        }

        var ds = await truyVan.OrderBy(h => h.MaHonPhoiCu).Select(h => new HonPhoiThongKeDto(
            h.Id, h.MaHonPhoiCu, h.TenHonPhoi, h.SoHonPhoi, h.NoiHonPhoi, h.NgayHonPhoi,
            h.LinhMucChung, h.CachThucHonPhoi, h.GhiChu,
            h.GiaoDanThamGia.Where(k => k.GiaoDan!.Phai == "Nam").Select(k => k.GiaoDan!.HoTen).FirstOrDefault(),
            h.GiaoDanThamGia.Where(k => k.GiaoDan!.Phai == "Nữ").Select(k => k.GiaoDan!.HoTen).FirstOrDefault(),
            h.GiaoDanThamGia.Select(k => k.GiaoDan!.GiaoHo == null ? null : k.GiaoDan.GiaoHo.TenGiaoHo)
                .FirstOrDefault(t => t != null)))
            .ToListAsync(ct);
        return new ThongKeChungKetQua(ds.Count, " đôi chịu phép hôn phối", null, null, ds);
    }

    // ================= Chủ hộ / Gia trưởng / Hiền mẫu (Extract.SelectHeadByAge/SelectByVaiTro) =================
    private async Task<ThongKeChungKetQua> ThongKeTheoVaiTroTuoi(
        DieuKienThongKe dieuKien, Guid? giaoHoId, int? tuTuoi, int? denTuoi, bool luuTru, bool khongCoNgay,
        CancellationToken ct)
    {
        if (tuTuoi is null || denTuoi is null)
            throw new ArgumentException("Thiếu Từ tuổi/Đến tuổi.");

        // Extract.cs:33-37/121-125 — CỐ Ý KHÔNG hoán đổi, xem mục 4.3 thong-ke-bieu-do.md: với
        // Từ tuổi < Đến tuổi, fromYear > toYear luôn ra danh sách rỗng (bug gốc, migrate y hệt).
        var nay = DateTime.Now.Year;
        int fromYear = nay - tuTuoi.Value, toYear = nay - denTuoi.Value;

        var truyVan = db.GiaoDan.Where(g => luuTru || (!g.DaXoa
            && !g.GiaDinhThamGia.Any(tv => tv.GiaDinh!.DaChuyenXu) && !g.QuaDoi));
        if (giaoHoId is { } id)
            truyVan = truyVan.Where(g => g.GiaoHoId == id || (g.GiaoHo != null && g.GiaoHo.GiaoHoChaId == id));

        truyVan = dieuKien switch
        {
            DieuKienThongKe.ChuHo => truyVan.Where(g => g.GiaDinhThamGia.Any(tv => tv.ChuHo)),
            DieuKienThongKe.GiaTruong => truyVan.Where(g => g.GiaDinhThamGia.Any(tv => tv.VaiTro == VaiTroGiaDinh.Chong)),
            DieuKienThongKe.HienMau => truyVan.Where(g => g.GiaDinhThamGia.Any(tv => tv.VaiTro == VaiTroGiaDinh.Vo)),
            _ => truyVan,
        };

        truyVan = truyVan.Where(g =>
            (g.NgaySinh != null && g.NgaySinh.Value.Year >= fromYear && g.NgaySinh.Value.Year <= toYear)
            || (khongCoNgay && g.NgaySinh == null));

        var nhan = dieuKien switch
        {
            DieuKienThongKe.ChuHo => " chủ hộ",
            DieuKienThongKe.GiaTruong => " gia trưởng",
            DieuKienThongKe.HienMau => " hiền mẫu",
            _ => "",
        };
        var nguon = truyVan.OrderBy(g => g.MaGiaoDanCu).Select(g => new GiaoDanService.NguonDong(g, null,
            g.GiaDinhThamGia.OrderBy(tv => tv.VaiTro).Select(tv => (Guid?)tv.GiaDinhId).FirstOrDefault(),
            g.GiaDinhThamGia.Any(tv => tv.GiaDinh!.DaChuyenXu)));
        var ds = await GiaoDanService.DungDanhSach(nguon).ToListAsync(ct);
        return new ThongKeChungKetQua(ds.Count, nhan, ds, null, null);
    }

    // ================= Cao niên / Giới trẻ / Thiếu nhi (Extract.SelectByTuoi) =================
    private async Task<ThongKeChungKetQua> ThongKeTheoTuoi(
        DieuKienThongKe dieuKien, Guid? giaoHoId, int? tuTuoi, int? denTuoi, bool luuTru, CancellationToken ct)
    {
        var nay = DateTime.Now.Year;
        var truyVan = db.GiaoDan.Where(g => luuTru || (!g.DaXoa
            && !g.GiaDinhThamGia.Any(tv => tv.GiaDinh!.DaChuyenXu) && !g.QuaDoi));
        if (giaoHoId is { } id)
            truyVan = truyVan.Where(g => g.GiaoHoId == id || (g.GiaoHo != null && g.GiaoHo.GiaoHoChaId == id));

        string nhan;
        switch (dieuKien)
        {
            case DieuKienThongKe.CaoNien:
            {
                // SelectByTuoi(CAO_NIEN): condition = BETWEEN 1 AND fromYear — bỏ qua Đến tuổi
                // hoàn toàn (Extract.cs:160-163).
                var toa = tuTuoi ?? TuoiCaoNien;
                var fromYear = nay - toa;
                truyVan = truyVan.Where(g => g.NgaySinh != null && g.NgaySinh.Value.Year >= 1 && g.NgaySinh.Value.Year <= fromYear);
                nhan = " cao niên";
                break;
            }
            case DieuKienThongKe.GioiTre:
            {
                var tu = tuTuoi ?? TuoiTreTu; var den = denTuoi ?? TuoiTreDen;
                int fromYear = nay - tu, toYear = nay - den; // cùng bug đảo cận, mục 4.3
                truyVan = truyVan.Where(g => g.NgaySinh != null && g.NgaySinh.Value.Year >= fromYear
                    && g.NgaySinh.Value.Year <= toYear && !g.DaCoGiaDinh);
                nhan = " giới trẻ";
                break;
            }
            default: // ThieuNhi
            {
                var tu = tuTuoi ?? TuoiThieuNhiTu; var den = denTuoi ?? TuoiThieuNhiDen;
                int fromYear = nay - tu, toYear = nay - den; // cùng bug đảo cận, mục 4.3
                truyVan = truyVan.Where(g => g.NgaySinh != null && g.NgaySinh.Value.Year >= fromYear && g.NgaySinh.Value.Year <= toYear);
                nhan = " thiếu nhi";
                break;
            }
        }

        var nguon = truyVan.OrderBy(g => g.MaGiaoDanCu).Select(g => new GiaoDanService.NguonDong(g, null,
            g.GiaDinhThamGia.OrderBy(tv => tv.VaiTro).Select(tv => (Guid?)tv.GiaDinhId).FirstOrDefault(),
            g.GiaDinhThamGia.Any(tv => tv.GiaDinh!.DaChuyenXu)));
        var ds = await GiaoDanService.DungDanhSach(nguon).ToListAsync(ct);
        return new ThongKeChungKetQua(ds.Count, nhan, ds, null, null);
    }
}
