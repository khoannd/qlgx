using Microsoft.EntityFrameworkCore;
using Qlgx.Api.Dtos;
using Qlgx.Data;
using Qlgx.Domain.Entities;

namespace Qlgx.Api.Services;

public enum KetQuaLuuRaoHonPhoi { ThanhCong, KhongTimThay, DungPhienBan }

/// <summary>
/// Dịch vụ cho màn hình "Danh sách rao hôn phối" (frmRaoHonPhoiList.cs + frmRaoHonPhoi.cs) —
/// xem docs/superpowers/specs/man-hinh/rao-hon-phoi.md. Bảng rỗng ở giáo xứ khảo sát (Vô
/// Nhiễm) nhưng đủ 26 cột được migrate nguyên vẹn từ Access — giáo xứ khác có dữ liệu thật.
/// </summary>
public class RaoHonPhoiService(QlgxDbContext db, SinhMaService sinhMa, IBoiCanhGiaoXu boiCanh)
{
    /// <summary>
    /// <paramref name="chiChuaHoanTat"/> khớp mặc định của <c>cbOption</c>
    /// ("Chỉ xem những đôi rao chưa hoàn tất", GxRaoHonPhoiList.cs:171-175): còn hiệu lực khi
    /// NgayRaoLan3 null HOẶC còn trong tương lai/tương đương hôm nay theo phép so sánh chuỗi
    /// gốc. Bản gốc dùng phép so sánh chuỗi (Int(Right(...)&Mid(...)&Left(...))) trên cột text
    /// Access — quy về so sánh DateOnly &gt;= hôm nay có cùng ý nghĩa. NgayRaoLan3 null được
    /// coi là "chưa hoàn tất" (khác desktop — biểu thức Access với chuỗi rỗng cho kết quả
    /// không xác định rõ ràng khi đọc mã; xem can-review-sau.md).
    /// </summary>
    public async Task<List<RaoHonPhoiListItemDto>> LayDanhSach(bool xemTatCa, CancellationToken ct)
    {
        var homNay = DateOnly.FromDateTime(DateTime.Now);
        var truyVan = db.RaoHonPhoi.Include(r => r.GiaoDan1).Include(r => r.GiaoDan2).AsQueryable();
        if (!xemTatCa)
            truyVan = truyVan.Where(r => r.NgayRaoLan3 == null || r.NgayRaoLan3 >= homNay);

        var ds = xemTatCa
            ? await truyVan.OrderBy(r => r.NgayRaoLan1).ToListAsync(ct)
            : await truyVan.OrderBy(r => r.NgayRaoLan3).ToListAsync(ct);

        return ds.Select(r => new RaoHonPhoiListItemDto(
            r.Id, r.MaRaoHonPhoiCu, r.TenRaoHonPhoi,
            r.GiaoDan1?.HoTen, r.GiaoDan2?.HoTen,
            r.NgayRaoLan1, r.NgayRaoLan2, r.NgayRaoLan3, r.GhiChu)).ToList();
    }

    public async Task<RaoHonPhoiDetailDto?> LayChiTiet(Guid id, CancellationToken ct)
    {
        var r = await db.RaoHonPhoi.Include(x => x.GiaoDan1).Include(x => x.GiaoDan2)
            .SingleOrDefaultAsync(x => x.Id == id, ct);
        return r is null ? null : AnhXa(r);
    }

    private static RaoHonPhoiDetailDto AnhXa(RaoHonPhoi r) => new(
        r.Id, r.MaRaoHonPhoiCu, r.TenRaoHonPhoi,
        r.GiaoDan1Id, r.GiaoDan1?.HoTen, r.GiaoDan2Id, r.GiaoDan2?.HoTen,
        r.NgayRaoLan1, r.NgayRaoLan2, r.NgayRaoLan3,
        r.GiaoXu1, r.GiaoPhan1, r.GiaoXuTruoc1, r.GiaoPhanTruoc1,
        r.GiaoXu2, r.GiaoPhan2, r.GiaoXuTruoc2, r.GiaoPhanTruoc2,
        r.LinhMucNhan, r.GiaoXuNhan, r.GhiChu,
        r.Tam1, r.Tam2, r.Tam3,
        r.GiaoXuNQ1, r.GiaoPhanNQ1, r.GiaoXuNQ2, r.GiaoPhanNQ2,
        r.RowVersion);

    public async Task<RaoHonPhoiDetailDto> Tao(LuuRaoHonPhoiRequest yc, CancellationToken ct)
    {
        var giaoXuId = boiCanh.GiaoXuId;
        var r = new RaoHonPhoi
        {
            GiaoXuId = giaoXuId,
            MaRaoHonPhoiCu = await sinhMa.LayMaTiepTheo(giaoXuId, "rao_hon_phoi",
                await db.RaoHonPhoi.MaxAsync(x => (int?)x.MaRaoHonPhoiCu, ct) ?? 0, ct),
        };
        GanTuYeuCau(r, yc);
        db.RaoHonPhoi.Add(r);
        await db.SaveChangesAsync(ct);
        await db.Entry(r).Reference(x => x.GiaoDan1).LoadAsync(ct);
        await db.Entry(r).Reference(x => x.GiaoDan2).LoadAsync(ct);
        return AnhXa(r);
    }

    public async Task<KetQuaLuuRaoHonPhoi> CapNhat(Guid id, LuuRaoHonPhoiRequest yc, CancellationToken ct)
    {
        var r = await db.RaoHonPhoi.SingleOrDefaultAsync(x => x.Id == id, ct);
        if (r is null) return KetQuaLuuRaoHonPhoi.KhongTimThay;
        if (yc.RowVersion is { } rv && r.RowVersion != rv) return KetQuaLuuRaoHonPhoi.DungPhienBan;

        GanTuYeuCau(r, yc);
        await db.SaveChangesAsync(ct);
        return KetQuaLuuRaoHonPhoi.ThanhCong;
    }

    private static void GanTuYeuCau(RaoHonPhoi r, LuuRaoHonPhoiRequest yc)
    {
        r.TenRaoHonPhoi = yc.TenRaoHonPhoi;
        r.GiaoDan1Id = yc.GiaoDan1Id;
        r.GiaoDan2Id = yc.GiaoDan2Id;
        r.NgayRaoLan1 = yc.NgayRaoLan1;
        r.NgayRaoLan2 = yc.NgayRaoLan2;
        r.NgayRaoLan3 = yc.NgayRaoLan3;
        r.GiaoXu1 = yc.GiaoXu1; r.GiaoPhan1 = yc.GiaoPhan1;
        r.GiaoXuTruoc1 = yc.GiaoXuTruoc1; r.GiaoPhanTruoc1 = yc.GiaoPhanTruoc1;
        r.GiaoXu2 = yc.GiaoXu2; r.GiaoPhan2 = yc.GiaoPhan2;
        r.GiaoXuTruoc2 = yc.GiaoXuTruoc2; r.GiaoPhanTruoc2 = yc.GiaoPhanTruoc2;
        r.LinhMucNhan = yc.LinhMucNhan; r.GiaoXuNhan = yc.GiaoXuNhan; r.GhiChu = yc.GhiChu;
        r.Tam1 = yc.Tam1; r.Tam2 = yc.Tam2; r.Tam3 = yc.Tam3;
        r.GiaoXuNQ1 = yc.GiaoXuNQ1; r.GiaoPhanNQ1 = yc.GiaoPhanNQ1;
        r.GiaoXuNQ2 = yc.GiaoXuNQ2; r.GiaoPhanNQ2 = yc.GiaoPhanNQ2;
    }

    /// <summary>Xoá cứng — khớp <c>DeleteRow</c> (GxRaoHonPhoiList.cs:242-265), không có bước
    /// xoá mềm ở màn hình này trong bản desktop.</summary>
    public async Task<bool> Xoa(Guid id, CancellationToken ct)
    {
        var r = await db.RaoHonPhoi.SingleOrDefaultAsync(x => x.Id == id, ct);
        if (r is null) return false;
        db.RaoHonPhoi.Remove(r);
        await db.SaveChangesAsync(ct);
        return true;
    }
}
