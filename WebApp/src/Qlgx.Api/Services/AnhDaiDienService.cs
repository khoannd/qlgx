using Microsoft.EntityFrameworkCore;
using Qlgx.Api.Anh;
using Qlgx.Data;

namespace Qlgx.Api.Services;

/// <summary>Ảnh đã lưu, đủ để endpoint GET trả trực tiếp bằng Results.File.</summary>
public sealed record AnhDaiDienDaLuu(byte[] DuLieu, string LoaiNoiDung);

public enum KetQuaLuuAnh { ThanhCong, KhongTimThay, Loi }

/// <summary>
/// Ảnh đại diện giáo dân/gia đình (Task 1.2 VIEC-TIEP-THEO.md, xem
/// docs/superpowers/specs/man-hinh/can-review-sau.md mục 36) — lưu nhị phân TRỰC TIẾP trong
/// CSDL (AnhDaiDienDuLieu/AnhDaiDienLoaiNoiDung của GiaoDan/GiaDinh), KHÔNG ghi file lên đĩa
/// máy chủ (ràng buộc HA: nhiều bản Qlgx.Api chạy song song sau bộ cân bằng tải, ghi đĩa cục bộ
/// ở một máy thì máy khác không đọc được, container khởi động lại là mất trắng).
///
/// Dùng CHUNG một service cho cả hai loại thực thể vì logic kiểm tra/thu nhỏ/lưu/xoá giống hệt
/// nhau — chỉ khác bảng đích. Giống mọi service nghiệp vụ khác, KHÔNG bao giờ nhận GiaoXuId từ
/// tham số: mọi truy vấn GiaoDan/GiaDinh đều đi qua bộ lọc toàn cục theo claim đăng nhập của
/// QlgxDbContext (xem QlgxDbContext.OnModelCreating), cộng thêm RLS phía PostgreSQL làm lớp
/// phòng thủ thứ hai — một phiên đăng nhập của giáo xứ A không thể đọc/ghi/xoá ảnh của giáo xứ
/// B dù có đoán đúng Id bản ghi.
/// </summary>
public class AnhDaiDienService(QlgxDbContext db)
{
    private const int GioiHanDoDaiTepUpload = XuLyAnh.GioiHanDungLuongGoc;

    /// <summary>Đọc toàn bộ luồng vào bộ nhớ rồi giới hạn kích thước NGAY — không đợi đọc hết
    /// một tệp khổng lồ mới báo lỗi. IFormFile.Length đã có sẵn (form đã đọc hết vào bộ nhớ/đĩa
    /// tạm trước khi endpoint chạy) nên kiểm tra trước khi đọc OpenReadStream là đủ, không cần
    /// giới hạn luồng đọc kiểu dừng giữa chừng.</summary>
    private static async Task<byte[]> DocGioiHan(Stream nguon, CancellationToken ct)
    {
        using var ms = new MemoryStream();
        await nguon.CopyToAsync(ms, ct);
        return ms.ToArray();
    }

    public async Task<(KetQuaLuuAnh ketQua, string? loi)> LuuAnhGiaoDan(
        Guid giaoDanId, Stream noiDungTep, long doDaiKhaiBao, CancellationToken ct)
    {
        if (doDaiKhaiBao > GioiHanDoDaiTepUpload)
            return (KetQuaLuuAnh.Loi,
                $"Ảnh quá lớn. Kích thước tối đa cho phép là {GioiHanDoDaiTepUpload / 1024 / 1024} MB.");

        var g = await db.GiaoDan.FirstOrDefaultAsync(x => x.Id == giaoDanId && !x.DaXoa, ct);
        if (g is null) return (KetQuaLuuAnh.KhongTimThay, null);

        var duLieuGoc = await DocGioiHan(noiDungTep, ct);
        var (ketQua, loi) = XuLyAnh.XuLy(duLieuGoc);
        if (loi is not null) return (KetQuaLuuAnh.Loi, loi.ThongBao);

        g.AnhDaiDienDuLieu = ketQua!.DuLieu;
        g.AnhDaiDienLoaiNoiDung = ketQua.LoaiNoiDung;
        await db.SaveChangesAsync(ct);
        return (KetQuaLuuAnh.ThanhCong, null);
    }

    public async Task<AnhDaiDienDaLuu?> LayAnhGiaoDan(Guid giaoDanId, CancellationToken ct)
    {
        var g = await db.GiaoDan
            .Where(x => x.Id == giaoDanId && !x.DaXoa)
            .Select(x => new { x.AnhDaiDienDuLieu, x.AnhDaiDienLoaiNoiDung })
            .FirstOrDefaultAsync(ct);
        if (g?.AnhDaiDienDuLieu is null || g.AnhDaiDienLoaiNoiDung is null) return null;
        return new AnhDaiDienDaLuu(g.AnhDaiDienDuLieu, g.AnhDaiDienLoaiNoiDung);
    }

    public async Task<bool> XoaAnhGiaoDan(Guid giaoDanId, CancellationToken ct)
    {
        var g = await db.GiaoDan.FirstOrDefaultAsync(x => x.Id == giaoDanId && !x.DaXoa, ct);
        if (g is null) return false;
        g.AnhDaiDienDuLieu = null;
        g.AnhDaiDienLoaiNoiDung = null;
        await db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<(KetQuaLuuAnh ketQua, string? loi)> LuuAnhGiaDinh(
        Guid giaDinhId, Stream noiDungTep, long doDaiKhaiBao, CancellationToken ct)
    {
        if (doDaiKhaiBao > GioiHanDoDaiTepUpload)
            return (KetQuaLuuAnh.Loi,
                $"Ảnh quá lớn. Kích thước tối đa cho phép là {GioiHanDoDaiTepUpload / 1024 / 1024} MB.");

        var gd = await db.GiaDinh.FirstOrDefaultAsync(x => x.Id == giaDinhId && !x.DaXoa, ct);
        if (gd is null) return (KetQuaLuuAnh.KhongTimThay, null);

        var duLieuGoc = await DocGioiHan(noiDungTep, ct);
        var (ketQua, loi) = XuLyAnh.XuLy(duLieuGoc);
        if (loi is not null) return (KetQuaLuuAnh.Loi, loi.ThongBao);

        gd.AnhDaiDienDuLieu = ketQua!.DuLieu;
        gd.AnhDaiDienLoaiNoiDung = ketQua.LoaiNoiDung;
        await db.SaveChangesAsync(ct);
        return (KetQuaLuuAnh.ThanhCong, null);
    }

    public async Task<AnhDaiDienDaLuu?> LayAnhGiaDinh(Guid giaDinhId, CancellationToken ct)
    {
        var gd = await db.GiaDinh
            .Where(x => x.Id == giaDinhId && !x.DaXoa)
            .Select(x => new { x.AnhDaiDienDuLieu, x.AnhDaiDienLoaiNoiDung })
            .FirstOrDefaultAsync(ct);
        if (gd?.AnhDaiDienDuLieu is null || gd.AnhDaiDienLoaiNoiDung is null) return null;
        return new AnhDaiDienDaLuu(gd.AnhDaiDienDuLieu, gd.AnhDaiDienLoaiNoiDung);
    }

    public async Task<bool> XoaAnhGiaDinh(Guid giaDinhId, CancellationToken ct)
    {
        var gd = await db.GiaDinh.FirstOrDefaultAsync(x => x.Id == giaDinhId && !x.DaXoa, ct);
        if (gd is null) return false;
        gd.AnhDaiDienDuLieu = null;
        gd.AnhDaiDienLoaiNoiDung = null;
        await db.SaveChangesAsync(ct);
        return true;
    }
}
