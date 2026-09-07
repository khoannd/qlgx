using Microsoft.EntityFrameworkCore;
using Qlgx.Api.Dtos;
using Qlgx.Data;

namespace Qlgx.Api.Services;

/// <summary>
/// Màn hình "Biểu đồ" (`frmBieuDo.cs`) — 5 biểu đồ, công thức trích dẫn dòng mã gốc trong
/// docs/superpowers/specs/man-hinh/thong-ke-bieu-do.md mục 4.6. Mọi phép đếm dùng `GROUP BY`
/// hoặc `COUNT` phía CSDL (PostgreSQL) — KHÔNG tải hết `GiaoDan`/`HonPhoi` về bộ nhớ rồi đếm
/// bằng LINQ-to-Objects (ràng buộc hiệu năng của task, dữ liệu khảo sát có 6150 bí tích chi
/// tiết + 2050 giáo dân).
/// </summary>
public class BieuDoService(QlgxDbContext db)
{
    /// <summary>"Tổng giáo dân" — LUỸ KẾ đến 31/12 mỗi năm (`exportTongGiaoDan`,
    /// frmBieuDo.cs:70-110). Một GROUP BY theo năm sinh (không phải N truy vấn như desktop) rồi
    /// cộng dồn (prefix sum) trong bộ nhớ trên vài chục số nguyên — không phải trên hàng nghìn
    /// dòng GiaoDan.</summary>
    public async Task<List<BieuDoNamDto>> TongGiaoDanTheoNam(int tuNam, int denNam, bool luuTru, CancellationToken ct)
    {
        var truyVan = db.GiaoDan.Where(g => !g.DaXoa && !g.KhongThongKe && g.NgaySinh != null && g.NgaySinh.Value.Year <= denNam);
        if (!luuTru) truyVan = truyVan.Where(g => !g.QuaDoi);

        var theoNam = await truyVan
            .GroupBy(g => g.NgaySinh!.Value.Year)
            .Select(gr => new { Nam = gr.Key, SoLuong = gr.Count() })
            .ToListAsync(ct);

        var ketQua = new List<BieuDoNamDto>();
        var luyKe = 0;
        // Cộng dồn TẤT CẢ năm sinh <= i (kể cả trước tuNam) — đúng công thức desktop
        // "NgaySinh <= 31/12/i", không có cận dưới nào cho năm sinh.
        for (var i = tuNam; i <= denNam; i++)
        {
            luyKe = theoNam.Where(x => x.Nam <= i).Sum(x => x.SoLuong);
            ketQua.Add(new BieuDoNamDto(i, luyKe));
        }
        return ketQua;
    }

    /// <summary>"Tổng hôn phối" — theo TỪNG năm riêng, không luỹ kế (`exportTongHonPhoi`,
    /// frmBieuDo.cs:112-155).</summary>
    public async Task<List<BieuDoNamDto>> TongHonPhoiTheoNam(int tuNam, int denNam, CancellationToken ct)
    {
        var theoNam = await db.HonPhoi
            .Where(h => h.NgayHonPhoi != null && h.NgayHonPhoi.Value.Year >= tuNam && h.NgayHonPhoi.Value.Year <= denNam)
            .GroupBy(h => h.NgayHonPhoi!.Value.Year)
            .Select(gr => new { Nam = gr.Key, SoLuong = gr.Count() })
            .ToListAsync(ct);
        return Enumerable.Range(tuNam, denNam - tuNam + 1)
            .Select(nam => new BieuDoNamDto(nam, theoNam.FirstOrDefault(x => x.Nam == nam)?.SoLuong ?? 0))
            .ToList();
    }

    /// <summary>"Tình hình bí tích" — 4 chuỗi theo từng năm (`exportBiTich`, frmBieuDo.cs:157-236),
    /// luôn loại người qua đời (chkLuuTru không thể bật cho biểu đồ này — mục 4.5). 4 GROUP BY
    /// riêng (một cho mỗi cột ngày) thay vì tải cả bảng GiaoDan về đếm bằng LINQ-to-Objects.</summary>
    public async Task<List<BieuDoBiTichNamDto>> BiTichTheoNam(int tuNam, int denNam, CancellationToken ct)
    {
        var nen = db.GiaoDan.Where(g => !g.DaXoa && !g.KhongThongKe && !g.QuaDoi);

        var sinhRa = await nen.Where(g => g.NgaySinh != null && g.NgaySinh.Value.Year >= tuNam && g.NgaySinh.Value.Year <= denNam)
            .GroupBy(g => g.NgaySinh!.Value.Year).Select(gr => new { gr.Key, C = gr.Count() }).ToListAsync(ct);
        var ruaToi = await nen.Where(g => g.NgayRuaToi != null && g.NgayRuaToi.Value.Year >= tuNam && g.NgayRuaToi.Value.Year <= denNam)
            .GroupBy(g => g.NgayRuaToi!.Value.Year).Select(gr => new { gr.Key, C = gr.Count() }).ToListAsync(ct);
        var ruocLe = await nen.Where(g => g.NgayRuocLe != null && g.NgayRuocLe.Value.Year >= tuNam && g.NgayRuocLe.Value.Year <= denNam)
            .GroupBy(g => g.NgayRuocLe!.Value.Year).Select(gr => new { gr.Key, C = gr.Count() }).ToListAsync(ct);
        var themSuc = await nen.Where(g => g.NgayThemSuc != null && g.NgayThemSuc.Value.Year >= tuNam && g.NgayThemSuc.Value.Year <= denNam)
            .GroupBy(g => g.NgayThemSuc!.Value.Year).Select(gr => new { gr.Key, C = gr.Count() }).ToListAsync(ct);

        return Enumerable.Range(tuNam, denNam - tuNam + 1).Select(nam => new BieuDoBiTichNamDto(
            nam,
            sinhRa.FirstOrDefault(x => x.Key == nam)?.C ?? 0,
            ruaToi.FirstOrDefault(x => x.Key == nam)?.C ?? 0,
            ruocLe.FirstOrDefault(x => x.Key == nam)?.C ?? 0,
            themSuc.FirstOrDefault(x => x.Key == nam)?.C ?? 0)).ToList();
    }

    /// <summary>"So sánh độ tuổi" — 7 nhóm CỐ ĐỊNH theo năm hiện tại (`exportDoTuoi`,
    /// frmBieuDo.cs:238-351), luôn loại người qua đời. Bucket cuối (`Tren50`) dùng cận CỐ ĐỊNH
    /// `fromYear=1990` (không suy từ tuổi) — với năm hiện tại &lt; 2041, `toYear = nay-51 &lt;
    /// 1990` nên luôn ra 0 (bug gốc, xem thong-ke-bieu-do.md mục 4.6 — ĐÃ kiểm chứng bằng
    /// `qlgx_thu`: 6 nhóm đầu ra 230/114/148/321/104/169, nhóm cuối ra đúng 0). Migrate y hệt,
    /// KHÔNG tự sửa cận.</summary>
    public async Task<BieuDoDoTuoiDto> DoTuoiTheoNhom(CancellationToken ct)
    {
        var nay = DateTime.Now.Year;
        var nen = db.GiaoDan.Where(g => !g.DaXoa && !g.KhongThongKe && !g.QuaDoi && g.NgaySinh != null);

        async Task<int> Dem(int tu, int den) =>
            await nen.CountAsync(g => g.NgaySinh!.Value.Year >= tu && g.NgaySinh.Value.Year <= den, ct);

        var duoi7 = await Dem(nay - 6, nay);
        var tu7den12 = await Dem(nay - 12, nay - 7);
        var tu13den16 = await Dem(nay - 16, nay - 13);
        var tu17den25 = await Dem(nay - 25, nay - 17);
        var tu26den30 = await Dem(nay - 30, nay - 26);
        var tu31den50 = await Dem(nay - 50, nay - 31);
        // Cận cố định 1990 (không phải nay-X) — xem ghi chú trên.
        var tren50 = await Dem(1990, nay - 51);

        return new BieuDoDoTuoiDto(duoi7, tu7den12, tu13den16, tu17den25, tu26den30, tu31den50, tren50);
    }

    /// <summary>"So sánh Giáo họ" — KHÔNG lọc `QuaDoi` (khác 4 biểu đồ kia, `exportGiaoHo`,
    /// frmBieuDo.cs:353-378: `WHERE DaXoa=0 AND GiaoDanAo=0 AND MaGiaoHo=?`, không có `QuaDoi`).
    /// Một GROUP BY duy nhất theo GiaoHoId + LEFT JOIN với danh mục giáo họ để giữ cả giáo họ có
    /// 0 giáo dân (đúng vòng lặp "for mỗi giáo họ" của desktop).</summary>
    public async Task<List<BieuDoGiaoHoDto>> GiaoHoSoSanh(CancellationToken ct)
    {
        var theoGiaoHo = await db.GiaoDan
            .Where(g => !g.DaXoa && !g.KhongThongKe && g.GiaoHoId != null)
            .GroupBy(g => g.GiaoHoId)
            .Select(gr => new { GiaoHoId = gr.Key, SoLuong = gr.Count() })
            .ToListAsync(ct);

        var danhMuc = await db.GiaoHo.Where(h => !h.DaXoa).OrderBy(h => h.MaGiaoHoCu).ToListAsync(ct);
        return danhMuc.Select(h => new BieuDoGiaoHoDto(
            h.TenGiaoHo, theoGiaoHo.FirstOrDefault(x => x.GiaoHoId == h.Id)?.SoLuong ?? 0)).ToList();
    }
}
