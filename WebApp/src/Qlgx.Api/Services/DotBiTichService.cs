using Microsoft.EntityFrameworkCore;
using Qlgx.Api.Dtos;
using Qlgx.Data;
using Qlgx.Domain;
using Qlgx.Domain.Entities;

namespace Qlgx.Api.Services;

/// <summary>Kết quả thêm một giáo dân vào danh sách nhận bí tích của một đợt — xem
/// DotBiTichEndpoints để biết ánh xạ HTTP/thông báo.</summary>
public enum KetQuaThemNguoiNhan
{
    ThanhCong,
    KhongTimThayDotBiTich,
    KhongTimThayGiaoDan,
    /// <summary>Đã có trong CHÍNH đợt này (frmBiTichChiTiet.cs:167-172).</summary>
    DaTonTaiTrongDot,
    /// <summary>Đã có trong đợt KHÁC cùng loại bí tích (frmBiTichChiTiet.cs:174-181).</summary>
    DaTonTaiDotKhac,
}

public enum KetQuaCapNhatDotBiTich { ThanhCong, KhongTimThay, DungPhienBan }

/// <summary>
/// Dịch vụ cho màn hình "Danh sách sổ bí tích" (frmDotBiTichList.cs + frmBiTichChiTiet.cs) —
/// xem docs/superpowers/specs/man-hinh/so-bi-tich.md. Chỉ xử lý Rửa tội/Rước lễ/Thêm sức
/// (LoaiBiTich 0/1/2) — đúng ba giá trị có mặt trong dữ liệu thật và có cột GiaoDan tương
/// ứng (SoRuaToi.../SoRuocLe.../SoThemSuc...). Hôn phối có màn hình riêng (hon-phoi.md); An
/// táng/Xức dầu không có cột GiaoDan tương ứng trong CSDL đã di trú — xem can-review-sau.md.
/// </summary>
public class DotBiTichService(QlgxDbContext db, SinhMaService sinhMa, IBoiCanhGiaoXu boiCanh)
{
    public async Task<List<DotBiTichListItemDto>> LayDanhSach(
        LoaiBiTich loaiBiTich, int? tuNam, int? denNam, CancellationToken ct)
    {
        // Lọc theo năm dùng Year của NgayBiTich — bản desktop lọc trên chuỗi
        // IIF(LEN(NgayBiTich)>=1, RIGHT(NgayBiTich,4), "0000") (frmDotBiTichList.cs:96-108):
        // đợt chưa có ngày được coi như năm "0000". Với bộ lọc "Từ năm" (>=), năm 0 gần như
        // không bao giờ khớp (chỉ khớp khi người dùng nhập "Từ năm"=0, không xảy ra trên UI) —
        // nên loại các đợt NULL khi có tuNam. Với bộ lọc "Đến năm" (<=), năm 0 LUÔN khớp — nên
        // GIỮ LẠI các đợt NULL khi có denNam (khác lần đầu viết ở đây từng loại nhầm, xem
        // can-review-sau.md).
        var truyVan = db.DotBiTich.Where(d => d.LoaiBiTich == loaiBiTich);
        if (tuNam is { } tu) truyVan = truyVan.Where(d => d.NgayBiTich != null && d.NgayBiTich.Value.Year >= tu);
        if (denNam is { } den) truyVan = truyVan.Where(d => d.NgayBiTich == null || d.NgayBiTich.Value.Year <= den);

        return await truyVan
            .OrderBy(d => d.NgayBiTich)
            .Select(d => new DotBiTichListItemDto(
                d.Id, d.MaDotBiTichCu, d.LoaiBiTich, d.NgayBiTich, d.MoTa, d.LinhMuc, d.NoiBiTich,
                d.ChiTiet.Count))
            .ToListAsync(ct);
    }

    public async Task<DotBiTichDetailDto?> LayChiTiet(Guid id, CancellationToken ct)
    {
        var d = await db.DotBiTich
            .Include(x => x.ChiTiet).ThenInclude(c => c.GiaoDan)
            .SingleOrDefaultAsync(x => x.Id == id, ct);
        if (d is null) return null;
        return AnhXaChiTiet(d);
    }

    private static DotBiTichDetailDto AnhXaChiTiet(DotBiTich d) => new(
        d.Id, d.MaDotBiTichCu, d.LoaiBiTich, d.NgayBiTich, d.MoTa, d.LinhMuc, d.NoiBiTich, d.RowVersion,
        d.ChiTiet
            .OrderBy(c => SoBiTichCua(c.GiaoDan!, d.LoaiBiTich))
            .Select(c => new NguoiNhanBiTichDto(
                c.GiaoDanId, c.GiaoDan!.MaGiaoDanCu, c.GiaoDan.TenThanh, c.GiaoDan.HoTen, c.GiaoDan.Phai,
                c.GiaoDan.NgaySinh, SoBiTichCua(c.GiaoDan, d.LoaiBiTich), NguoiDoDauCua(c.GiaoDan, d.LoaiBiTich),
                c.GhiChu))
            .ToArray());

    // Tên cột "số bí tích"/"người đỡ đầu" đổi theo loại — khớp switch trong
    // GxBiTichChiTiet.FormatGrid (Source/GXControl/GxBiTichChiTiet.cs:284-312).
    private static string? SoBiTichCua(GiaoDan g, LoaiBiTich loai) => loai switch
    {
        LoaiBiTich.RuocLe => g.SoRuocLe,
        LoaiBiTich.ThemSuc => g.SoThemSuc,
        _ => g.SoRuaToi,
    };

    private static string? NguoiDoDauCua(GiaoDan g, LoaiBiTich loai) => loai switch
    {
        LoaiBiTich.ThemSuc => g.NguoiDoDauThemSuc,
        LoaiBiTich.RuaToi => g.NguoiDoDauRuaToi,
        _ => null, // Rước lễ không có cột người đỡ đầu trên GiaoDan lẫn trên lưới desktop.
    };

    public async Task<DotBiTichDetailDto> Tao(TaoDotBiTichRequest yc, CancellationToken ct)
    {
        var giaoXuId = boiCanh.GiaoXuId;
        var d = new DotBiTich
        {
            GiaoXuId = giaoXuId,
            LoaiBiTich = yc.LoaiBiTich,
            NgayBiTich = yc.NgayBiTich,
            MoTa = yc.MoTa,
            LinhMuc = yc.LinhMuc,
            NoiBiTich = yc.NoiBiTich,
            MaDotBiTichCu = await sinhMa.LayMaTiepTheo(giaoXuId, "dot_bi_tich",
                await db.DotBiTich.MaxAsync(x => (int?)x.MaDotBiTichCu, ct) ?? 0, ct),
        };
        db.DotBiTich.Add(d);
        await db.SaveChangesAsync(ct);
        return AnhXaChiTiet(d);
    }

    public async Task<KetQuaCapNhatDotBiTich> CapNhat(Guid id, CapNhatDotBiTichRequest yc, CancellationToken ct)
    {
        var d = await db.DotBiTich.SingleOrDefaultAsync(x => x.Id == id, ct);
        if (d is null) return KetQuaCapNhatDotBiTich.KhongTimThay;
        if (d.RowVersion != yc.RowVersion) return KetQuaCapNhatDotBiTich.DungPhienBan;

        d.NgayBiTich = yc.NgayBiTich;
        d.MoTa = yc.MoTa;
        d.LinhMuc = yc.LinhMuc;
        d.NoiBiTich = yc.NoiBiTich;

        // Ngày/Linh mục/Nơi của đợt được copy xuống GiaoDan của MỌI người đã có trong đợt —
        // khớp vòng lặp getGridData (frmBiTichChiTiet.cs:385-387): mỗi lần Cập nhật đợt, toàn
        // bộ người nhận trong đợt được đồng bộ lại theo giá trị mới nhất của đợt.
        var giaoDanIds = await db.BiTichChiTiet.Where(c => c.DotBiTichId == id).Select(c => c.GiaoDanId).ToListAsync(ct);
        if (giaoDanIds.Count > 0)
        {
            var giaoDanList = await db.GiaoDan.Where(g => giaoDanIds.Contains(g.Id)).ToListAsync(ct);
            foreach (var g in giaoDanList) GanNgayLinhMucNoi(g, d.LoaiBiTich, d.NgayBiTich, d.LinhMuc, d.NoiBiTich);
        }

        await db.SaveChangesAsync(ct);
        return KetQuaCapNhatDotBiTich.ThanhCong;
    }

    private static void GanNgayLinhMucNoi(GiaoDan g, LoaiBiTich loai, DateOnly? ngay, string? linhMuc, string? noi)
    {
        switch (loai)
        {
            case LoaiBiTich.RuocLe:
                g.NgayRuocLe = ngay; g.ChaRuocLe = linhMuc; g.NoiRuocLe = noi;
                break;
            case LoaiBiTich.ThemSuc:
                g.NgayThemSuc = ngay; g.ChaThemSuc = linhMuc; g.NoiThemSuc = noi;
                break;
            default:
                g.NgayRuaToi = ngay; g.ChaRuaToi = linhMuc; g.NoiRuaToi = noi;
                break;
        }
    }

    /// <summary>Xoá cả đợt — HẠ TẦNG XOÁ CỨNG, khớp <c>gxAddEdit1_DeleteClick</c>
    /// (frmDotBiTichList.cs:117-127): xoá BiTichChiTiet của đợt rồi xoá DotBiTich, KHÔNG null
    /// hoá lại các cột bí tích trên GiaoDan (khác việc xoá TỪNG người trong đợt — xem
    /// XoaNguoiNhan). Không hỏi lại người dùng ở tầng dịch vụ — endpoint/UI web tự hỏi xác nhận
    /// tương đương hộp thoại gốc.</summary>
    public async Task<bool> Xoa(Guid id, CancellationToken ct)
    {
        var d = await db.DotBiTich.SingleOrDefaultAsync(x => x.Id == id, ct);
        if (d is null) return false;
        await db.BiTichChiTiet.Where(c => c.DotBiTichId == id).ExecuteDeleteAsync(ct);
        db.DotBiTich.Remove(d);
        await db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<KetQuaThemNguoiNhan> ThemNguoiNhan(Guid dotBiTichId, ThemNguoiNhanRequest yc, CancellationToken ct)
    {
        var d = await db.DotBiTich.SingleOrDefaultAsync(x => x.Id == dotBiTichId, ct);
        if (d is null) return KetQuaThemNguoiNhan.KhongTimThayDotBiTich;
        var g = await db.GiaoDan.SingleOrDefaultAsync(x => x.Id == yc.GiaoDanId, ct);
        if (g is null) return KetQuaThemNguoiNhan.KhongTimThayGiaoDan;

        // Check trong đợt hiện tại — frmBiTichChiTiet.cs:167-172.
        if (await db.BiTichChiTiet.AnyAsync(c => c.DotBiTichId == dotBiTichId && c.GiaoDanId == yc.GiaoDanId, ct))
            return KetQuaThemNguoiNhan.DaTonTaiTrongDot;

        // Check trong đợt KHÁC cùng loại bí tích — frmBiTichChiTiet.cs:174-181
        // (SELECT_BITICH_CHITIET_THEOLOAI join DotBiTich.LoaiBiTich).
        var daO = await db.BiTichChiTiet
            .Include(c => c.DotBiTich)
            .AnyAsync(c => c.GiaoDanId == yc.GiaoDanId && c.DotBiTich!.LoaiBiTich == d.LoaiBiTich, ct);
        if (daO) return KetQuaThemNguoiNhan.DaTonTaiDotKhac;

        db.BiTichChiTiet.Add(new BiTichChiTiet
        {
            GiaoXuId = boiCanh.GiaoXuId, DotBiTichId = dotBiTichId, GiaoDanId = yc.GiaoDanId, GhiChu = yc.GhiChu,
        });
        GanNgayLinhMucNoi(g, d.LoaiBiTich, d.NgayBiTich, d.LinhMuc, d.NoiBiTich);
        GanSoVaNguoiDoDau(g, d.LoaiBiTich, yc.SoBiTich, yc.NguoiDoDau);

        await db.SaveChangesAsync(ct);
        return KetQuaThemNguoiNhan.ThanhCong;
    }

    private static void GanSoVaNguoiDoDau(GiaoDan g, LoaiBiTich loai, string? soBiTich, string? nguoiDoDau)
    {
        switch (loai)
        {
            case LoaiBiTich.RuocLe:
                g.SoRuocLe = soBiTich;
                break;
            case LoaiBiTich.ThemSuc:
                g.SoThemSuc = soBiTich; g.NguoiDoDauThemSuc = nguoiDoDau;
                break;
            default:
                g.SoRuaToi = soBiTich; g.NguoiDoDauRuaToi = nguoiDoDau;
                break;
        }
    }

    public async Task<bool> SuaNguoiNhan(Guid dotBiTichId, Guid giaoDanId, SuaNguoiNhanRequest yc, CancellationToken ct)
    {
        var d = await db.DotBiTich.SingleOrDefaultAsync(x => x.Id == dotBiTichId, ct);
        if (d is null) return false;
        var c = await db.BiTichChiTiet.SingleOrDefaultAsync(
            x => x.DotBiTichId == dotBiTichId && x.GiaoDanId == giaoDanId, ct);
        if (c is null) return false;
        var g = await db.GiaoDan.SingleOrDefaultAsync(x => x.Id == giaoDanId, ct);
        if (g is null) return false;

        c.GhiChu = yc.GhiChu;
        GanSoVaNguoiDoDau(g, d.LoaiBiTich, yc.SoBiTich, yc.NguoiDoDau);
        await db.SaveChangesAsync(ct);
        return true;
    }

    /// <summary>Xoá một người khỏi đợt — khớp <c>gxAddEdit1_DeleteClick</c>
    /// (frmBiTichChiTiet.cs:246-264): luôn xoá dòng BiTichChiTiet; nếu
    /// <paramref name="xoaThongTinBiTich"/>=true (lựa chọn [Yes] của hộp thoại thứ hai) thì
    /// đồng thời null hoá các cột bí tích tương ứng trên GiaoDan.</summary>
    public async Task<bool> XoaNguoiNhan(Guid dotBiTichId, Guid giaoDanId, bool xoaThongTinBiTich, CancellationToken ct)
    {
        var d = await db.DotBiTich.SingleOrDefaultAsync(x => x.Id == dotBiTichId, ct);
        if (d is null) return false;
        var c = await db.BiTichChiTiet.SingleOrDefaultAsync(
            x => x.DotBiTichId == dotBiTichId && x.GiaoDanId == giaoDanId, ct);
        if (c is null) return false;

        db.BiTichChiTiet.Remove(c);
        if (xoaThongTinBiTich)
        {
            var g = await db.GiaoDan.SingleOrDefaultAsync(x => x.Id == giaoDanId, ct);
            if (g is not null)
            {
                GanNgayLinhMucNoi(g, d.LoaiBiTich, null, null, null);
                GanSoVaNguoiDoDau(g, d.LoaiBiTich, null, null);
            }
        }

        await db.SaveChangesAsync(ct);
        return true;
    }
}
