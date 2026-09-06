using Microsoft.EntityFrameworkCore;
using Qlgx.Api.Dtos;
using Qlgx.Data;
using Qlgx.Domain.Entities;

namespace Qlgx.Api.Services;

public enum KetQuaLuuTaiKhoan
{
    ThanhCong,
    KhongTimThay,
    DungPhienBan,
    /// <summary>Tên tài khoản đã tồn tại trong CÙNG giáo xứ (đúng thông báo desktop
    /// "Tên tài khoản đã tồn tại, thử một tên khác" — xem frmAccoutList.cs:155).</summary>
    TrungTenTaiKhoan,
}

/// <summary>
/// Quản lý tài khoản đăng nhập trong PHẠM VI giáo xứ đang đăng nhập — không có thao tác nào ở
/// đây được phép nhận GiaoXuId từ bên ngoài, `db` (QlgxDbContext tiêm qua DI) đã tự lọc theo
/// BoiCanhGiaoXuTuNguoiDung nên mọi truy vấn/ghi ở lớp này tự động giới hạn đúng giáo xứ của
/// người gọi. Xem docs/superpowers/specs/man-hinh/quan-ly-tai-khoan.md.
/// </summary>
public class TaiKhoanService(QlgxDbContext db, IBoiCanhGiaoXu boiCanh, AuthService auth)
{
    public async Task<List<TaiKhoanItemDto>> LayDanhSach(CancellationToken ct) =>
        await (from t in db.TaiKhoan
               where !t.DaXoa
               join l in db.TenLoaiTaiKhoan on t.LoaiTaiKhoan equals l.MaLoaiTaiKhoanCu into loai
               from l in loai.DefaultIfEmpty()
               orderby t.TenTaiKhoan
               select new TaiKhoanItemDto(t.Id, t.TenTaiKhoan, t.HoTenNguoiDung, t.Email,
                   t.SoDienThoai, t.LoaiTaiKhoan, l != null ? l.TenLoai : null, t.RowVersion))
            .ToListAsync(ct);

    public async Task<(KetQuaLuuTaiKhoan Ket, Guid? Id)> Tao(TaoTaiKhoanRequest yc, CancellationToken ct)
    {
        var daTrung = await db.TaiKhoan.AnyAsync(t => t.TenTaiKhoan == yc.TenTaiKhoan && !t.DaXoa, ct);
        if (daTrung) return (KetQuaLuuTaiKhoan.TrungTenTaiKhoan, null);

        var taiKhoan = new TaiKhoan
        {
            GiaoXuId = boiCanh.GiaoXuId,
            TenTaiKhoan = yc.TenTaiKhoan,
            HoTenNguoiDung = yc.HoTenNguoiDung,
            Email = yc.Email,
            SoDienThoai = yc.SoDienThoai,
            LoaiTaiKhoan = yc.LoaiTaiKhoan,
        };
        taiKhoan.MatKhauBam = auth.Bam(taiKhoan, yc.MatKhau);
        db.TaiKhoan.Add(taiKhoan);
        await db.SaveChangesAsync(ct);
        return (KetQuaLuuTaiKhoan.ThanhCong, taiKhoan.Id);
    }

    public async Task<KetQuaLuuTaiKhoan> CapNhat(Guid id, CapNhatTaiKhoanRequest yc, CancellationToken ct)
    {
        var taiKhoan = await db.TaiKhoan.FirstOrDefaultAsync(t => t.Id == id && !t.DaXoa, ct);
        if (taiKhoan is null) return KetQuaLuuTaiKhoan.KhongTimThay;

        taiKhoan.HoTenNguoiDung = yc.HoTenNguoiDung;
        taiKhoan.Email = yc.Email;
        taiKhoan.SoDienThoai = yc.SoDienThoai;
        taiKhoan.LoaiTaiKhoan = yc.LoaiTaiKhoan;
        if (!string.IsNullOrEmpty(yc.MatKhauMoi))
            taiKhoan.MatKhauBam = auth.Bam(taiKhoan, yc.MatKhauMoi);

        db.Entry(taiKhoan).Property(x => x.RowVersion).OriginalValue = yc.RowVersion;
        try
        {
            await db.SaveChangesAsync(ct);
            return KetQuaLuuTaiKhoan.ThanhCong;
        }
        catch (DbUpdateConcurrencyException) { return KetQuaLuuTaiKhoan.DungPhienBan; }
    }

    /// <summary>Xoá mềm — đúng mô hình DaXoa của các bảng khác. Bản desktop xoá cứng
    /// (SqlConstants.DELETE_ACCOUNT) nhưng bản web ưu tiên an toàn dữ liệu và nhất quán với
    /// GiaoDan/GiaDinh/GiaoHo — quyết định ghi ở can-review-sau.md.</summary>
    public async Task<bool> Xoa(Guid id, CancellationToken ct)
    {
        var taiKhoan = await db.TaiKhoan.FirstOrDefaultAsync(t => t.Id == id && !t.DaXoa, ct);
        if (taiKhoan is null) return false;
        taiKhoan.DaXoa = true;
        await db.SaveChangesAsync(ct);
        return true;
    }
}
