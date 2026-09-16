using Microsoft.EntityFrameworkCore;
using Qlgx.Api.Dtos;
using Qlgx.Data;
using Qlgx.Data.NhatKy;
using Qlgx.Domain.Entities;

namespace Qlgx.Api.Services;

public enum KetQuaLinhMuc { ThanhCong, KhongTimThay }

/// <summary>
/// "Danh sách các cha quản xứ" — thêm/sửa/xoá trong PHẠM VI giáo xứ của người gọi, giống hoàn
/// toàn quy ước của <see cref="GiaoHoService"/>: QlgxDbContext tiêm qua DI đã tự lọc theo
/// GiaoXuId của phiên (bộ lọc EF) và bị RLS bảo vệ ở tầng CSDL, service không cần tự kiểm tra
/// GiaoXuId thủ công khi đọc/sửa/xoá — chỉ cần gán đúng lúc TẠO (không có cột đó sẵn).
/// </summary>
public class LinhMucService(QlgxDbContext db, SinhMaService sinhMa, IBoiCanhGiaoXu boiCanh)
{
    /// <summary>Sắp theo mã cũ tăng dần — cùng quy ước hiển thị của mọi danh sách khác trong hệ
    /// thống (xem GiaoHoEndpoints), KHÔNG theo Từ ngày vì nhiều cha chưa điền ngày nhận xứ.</summary>
    public Task<List<LinhMucDto>> DanhSach(CancellationToken ct) =>
        db.LinhMuc.Where(l => !l.DaXoa)
            .OrderBy(l => l.MaLinhMucCu)
            .Select(l => new LinhMucDto(
                l.Id, l.MaLinhMucCu, l.TenThanh, l.HoTen, l.NgaySinh, l.ChucVu,
                l.TuNgay, l.DenNgay, l.GhiChu, l.DienThoai, l.Email))
            .ToListAsync(ct);

    public async Task<Guid> Them(TaoLinhMucRequest yc, CancellationToken ct)
    {
        var maHienCo = await db.LinhMuc
            .Select(l => (int?)l.MaLinhMucCu)
            .MaxAsync(ct) ?? 0;
        var maMoi = await sinhMa.LayMaTiepTheo(boiCanh.GiaoXuId, "LinhMuc", maHienCo, ct);

        var linhMuc = new LinhMuc
        {
            GiaoXuId = boiCanh.GiaoXuId,
            MaLinhMucCu = maMoi,
            TenThanh = yc.TenThanh,
            HoTen = yc.HoTen,
            NgaySinh = yc.NgaySinh,
            ChucVu = yc.ChucVu,
            TuNgay = yc.TuNgay,
            DenNgay = yc.DenNgay,
            GhiChu = yc.GhiChu,
            DienThoai = yc.DienThoai,
            Email = yc.Email,
        };
        db.LinhMuc.Add(linhMuc);
        await db.LuuCoNhatKy(ct);
        return linhMuc.Id;
    }

    public async Task<KetQuaLinhMuc> Sua(Guid id, CapNhatLinhMucRequest yc, CancellationToken ct)
    {
        var linhMuc = await db.LinhMuc.FirstOrDefaultAsync(l => l.Id == id && !l.DaXoa, ct);
        if (linhMuc is null) return KetQuaLinhMuc.KhongTimThay;

        linhMuc.TenThanh = yc.TenThanh;
        linhMuc.HoTen = yc.HoTen;
        linhMuc.NgaySinh = yc.NgaySinh;
        linhMuc.ChucVu = yc.ChucVu;
        linhMuc.TuNgay = yc.TuNgay;
        linhMuc.DenNgay = yc.DenNgay;
        linhMuc.GhiChu = yc.GhiChu;
        linhMuc.DienThoai = yc.DienThoai;
        linhMuc.Email = yc.Email;
        linhMuc.UpdatedAt = DateTimeOffset.UtcNow;
        await db.LuuCoNhatKy(ct);
        return KetQuaLinhMuc.ThanhCong;
    }

    /// <summary>Xoá mềm (DaXoa=true), giống mọi bảng khác đã có cột này — giữ lại lịch sử các
    /// cha quản xứ trong CSDL thay vì xoá vĩnh viễn, khớp tinh thần "dữ liệu là sổ sách nhiều
    /// năm, mất là không lấy lại được" của toàn phần mềm.</summary>
    public async Task<KetQuaLinhMuc> Xoa(Guid id, CancellationToken ct)
    {
        var linhMuc = await db.LinhMuc.FirstOrDefaultAsync(l => l.Id == id && !l.DaXoa, ct);
        if (linhMuc is null) return KetQuaLinhMuc.KhongTimThay;

        linhMuc.DaXoa = true;
        linhMuc.UpdatedAt = DateTimeOffset.UtcNow;
        await db.LuuCoNhatKy(ct);
        return KetQuaLinhMuc.ThanhCong;
    }
}
