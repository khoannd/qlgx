using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Qlgx.Api.Dtos;
using Qlgx.Data;
using Qlgx.Domain;

namespace Qlgx.Api.Services;

public class GiaDinhService(QlgxDbContext db)
{
    public async Task<List<GiaDinhListItemDto>> LayDanhSach(
        Guid? giaoHoId, bool chiKhongThongKe, CancellationToken ct)
    {
        var truyVan = db.GiaDinh.Where(g => !g.DaXoa);

        if (giaoHoId is { } id) truyVan = truyVan.Where(g => g.GiaoHoId == id);
        if (chiKhongThongKe) truyVan = truyVan.Where(g => g.KhongThongKe);

        // EF Core không dịch được lời gọi phương thức tự viết (vd. GhepTen(...), TinhGach(...))
        // trên thực thể bên trong biểu thức LINQ — mọi cột dẫn xuất dưới đây đều viết bằng
        // biểu thức nội tuyến an toàn null thay vì gọi ra phương thức riêng. Việc định dạng
        // ngày hôn phối "dd/MM/yyyy" cũng không dịch được nên được để lại, thực hiện trên
        // danh sách trung gian HangTho sau khi đã ToListAsync (bước LINQ-to-Objects).
        var tho = await truyVan
            .OrderBy(g => g.MaGiaDinhCu)
            .Select(g => new HangTho(
                g.Id,
                g.MaGiaDinhCu,
                g.MaGiaDinhRieng,
                g.TenGiaDinh,
                g.ThanhVien.Where(tv => tv.VaiTro == VaiTroGiaDinh.Chong)
                    .Select(tv => string.IsNullOrWhiteSpace(tv.GiaoDan!.TenThanh)
                        ? tv.GiaoDan.HoTen
                        : tv.GiaoDan.TenThanh + " " + tv.GiaoDan.HoTen)
                    .FirstOrDefault(),
                g.ThanhVien.Where(tv => tv.VaiTro == VaiTroGiaDinh.Vo)
                    .Select(tv => string.IsNullOrWhiteSpace(tv.GiaoDan!.TenThanh)
                        ? tv.GiaoDan.HoTen
                        : tv.GiaoDan.TenThanh + " " + tv.GiaoDan.HoTen)
                    .FirstOrDefault(),
                // TAM THOI: dem toan bo thanh vien gia dinh. Ban Access dem "so nhan khau con
                // song, dang o xu" (loai nguoi da qua doi hoac da chuyen xu) — se sua lai cho
                // dung nghia nay o giai doan sau, khi co du du lieu de loc.
                g.ThanhVien.Count,
                g.DienThoai,
                g.ThanhVien.Where(tv => tv.VaiTro == VaiTroGiaDinh.Chong)
                    .Select(tv => tv.GiaoDan!.DienThoai).FirstOrDefault(),
                g.ThanhVien.Where(tv => tv.VaiTro == VaiTroGiaDinh.Vo)
                    .Select(tv => tv.GiaoDan!.DienThoai).FirstOrDefault(),
                g.DiaChi,
                g.GiaoHo == null ? "Ngoài xứ" : g.GiaoHo.TenGiaoHo,
                g.DienGiaDinh,
                g.GhiChu,
                // Gach: 0 = chong mat, 1 = vo mat, 2 = ca hai, -1 = khong gach.
                g.ThanhVien.Any(tv => tv.VaiTro == VaiTroGiaDinh.Chong && tv.GiaoDan!.QuaDoi)
                    && g.ThanhVien.Any(tv => tv.VaiTro == VaiTroGiaDinh.Vo && tv.GiaoDan!.QuaDoi)
                    ? 2
                    : g.ThanhVien.Any(tv => tv.VaiTro == VaiTroGiaDinh.Vo && tv.GiaoDan!.QuaDoi)
                        ? 1
                        : g.ThanhVien.Any(tv => tv.VaiTro == VaiTroGiaDinh.Chong && tv.GiaoDan!.QuaDoi)
                            ? 0
                            : -1,
                g.KhongThongKe,
                // HonPhoiId: gia đình được coi là "đã có hôn phối" khi chồng hoặc vợ có mặt
                // trong bảng nối GiaoDanHonPhoi. Lấy bản ghi đầu tiên tìm được — chồng và vợ
                // của cùng một gia đình luôn trỏ tới cùng một hôn phối.
                db.GiaoDanHonPhoi
                    .Where(gdhp => g.ThanhVien.Any(tv =>
                        (tv.VaiTro == VaiTroGiaDinh.Chong || tv.VaiTro == VaiTroGiaDinh.Vo)
                        && tv.GiaoDanId == gdhp.GiaoDanId))
                    .Select(gdhp => (Guid?)gdhp.HonPhoiId)
                    .FirstOrDefault(),
                db.GiaoDanHonPhoi
                    .Where(gdhp => g.ThanhVien.Any(tv =>
                        (tv.VaiTro == VaiTroGiaDinh.Chong || tv.VaiTro == VaiTroGiaDinh.Vo)
                        && tv.GiaoDanId == gdhp.GiaoDanId))
                    .Select(gdhp => (DateOnly?)gdhp.HonPhoi!.NgayHonPhoi)
                    .FirstOrDefault()))
            .ToListAsync(ct);

        return tho.Select(h => new GiaDinhListItemDto(
                h.Id, h.MaGiaDinhCu, h.MaGiaDinhRieng, h.TenGiaDinh, h.TenChong, h.TenVo,
                h.SoLuong, h.DienThoai, h.DTChong, h.DTVo, h.DiaChi, h.TenGiaoHo, h.DienGiaDinh,
                h.GhiChu, h.Gach, h.KhongThongKe, h.HonPhoiId,
                h.NgayHonPhoi?.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)))
            .ToList();
    }

    /// <summary>
    /// Hàng trung gian lấy thẳng từ SQL, trước khi định dạng NgayHonPhoi thành chuỗi hiển thị
    /// (bước định dạng ngày không dịch được sang SQL nên phải làm ở tầng LINQ-to-Objects).
    /// </summary>
    private sealed record HangTho(
        Guid Id, int MaGiaDinhCu, string? MaGiaDinhRieng, string? TenGiaDinh,
        string? TenChong, string? TenVo, int SoLuong, string? DienThoai,
        string? DTChong, string? DTVo, string? DiaChi, string? TenGiaoHo,
        string? DienGiaDinh, string? GhiChu, int Gach, bool KhongThongKe,
        Guid? HonPhoiId, DateOnly? NgayHonPhoi);
}
