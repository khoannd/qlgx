using Microsoft.EntityFrameworkCore;
using Qlgx.Api.Dtos;
using Qlgx.Data;

namespace Qlgx.Api.Services;

/// <summary>
/// Tab "Thống kê ơn gọi tận hiến" (`GxThongKeOnGoi.cs:59-133`) — giáo dân xuất thân từ giáo xứ
/// đã đi tu/tận hiến. `cbGiaoHo` có mặt trên form nhưng KHÔNG được dùng trong `btnSearch_Click`
/// (đã đối chiếu `GxThongKeOnGoi.Designer.cs`, không có sự kiện nào nối control này) — bản web
/// cố ý KHÔNG nhận tham số giáo họ, xem docs/superpowers/specs/man-hinh/thong-ke-bieu-do.md mục 9.
/// </summary>
public class ThongKeOnGoiService(QlgxDbContext db)
{
    public async Task<ThongKeOnGoiKetQua> LayThongKe(
        DateOnly tuNgay, DateOnly denNgay, string? chucVu, string? noiTu, string? dongTu, string? noiPhucVu,
        bool luuTru, bool khongCoNgay, CancellationToken ct)
    {
        var truyVan = db.TanHien.Where(t => !t.GiaoDan!.DaXoa && !t.GiaoDan.KhongThongKe);
        if (!luuTru)
            truyVan = truyVan.Where(t => !t.GiaoDan!.QuaDoi
                && !t.GiaoDan.GiaDinhThamGia.Any(tv => tv.GiaDinh!.DaChuyenXu));

        truyVan = truyVan.Where(t =>
            (t.NgayBatDau != null && t.NgayBatDau >= tuNgay && t.NgayBatDau <= denNgay)
            || (khongCoNgay && t.NgayBatDau == null));

        if (!string.IsNullOrWhiteSpace(chucVu)) truyVan = truyVan.Where(t => t.ChucVu != null && t.ChucVu.Contains(chucVu));
        if (!string.IsNullOrWhiteSpace(noiTu)) truyVan = truyVan.Where(t => t.NoiTu != null && t.NoiTu.Contains(noiTu));
        if (!string.IsNullOrWhiteSpace(dongTu)) truyVan = truyVan.Where(t => t.DongTu != null && t.DongTu.Contains(dongTu));
        if (!string.IsNullOrWhiteSpace(noiPhucVu)) truyVan = truyVan.Where(t => t.NoiPhucVu != null && t.NoiPhucVu.Contains(noiPhucVu));

        var ds = await truyVan.OrderBy(t => t.GiaoDan!.MaGiaoDanCu).Select(t => new OnGoiListItemDto(
            t.GiaoDanId, t.GiaoDan!.MaGiaoDanCu, t.GiaoDan.TenThanh, t.GiaoDan.HoTen, t.GiaoDan.Phai,
            t.GiaoDan.NgaySinh, t.GiaoDan.DienThoai, t.GiaoDan.DiaChi,
            t.GiaoDan.GiaoHo == null ? "Ngoài xứ" : t.GiaoDan.GiaoHo.TenGiaoHo,
            t.NgayBatDau, t.ChucVu, t.NoiTu, t.DongTu, t.NoiPhucVu))
            .ToListAsync(ct);

        return new ThongKeOnGoiKetQua(ds.Count, ds);
    }
}
