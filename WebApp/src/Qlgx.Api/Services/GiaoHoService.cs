using Microsoft.EntityFrameworkCore;
using Qlgx.Api.Dtos;
using Qlgx.Data;
using Qlgx.Domain.Entities;

namespace Qlgx.Api.Services;

public enum KetQuaGiaoHo { ThanhCong, KhongTimThay, TrungTen }

/// <summary>Thêm/sửa Giáo họ trong PHẠM VI giáo xứ của người gọi (GiaoXuId từ
/// <see cref="IBoiCanhGiaoXu"/>, đúng quy tắc chung của mọi màn hình nghiệp vụ — khác màn hình
/// "Quản lý giáo xứ" vốn cố ý xuyên giáo xứ). QlgxDbContext tiêm qua DI đã tự lọc theo
/// GiaoXuId của phiên (bộ lọc EF) và bị RLS bảo vệ ở tầng CSDL — hai lớp phòng thủ, không cần
/// service này tự kiểm tra GiaoXuId thủ công.</summary>
public class GiaoHoService(QlgxDbContext db, SinhMaService sinhMa, IBoiCanhGiaoXu boiCanh)
{
    /// <summary>Chặn trùng tên trong CÙNG một giáo họ cha — khớp `checkInput`
    /// (Source/ChuongTrinh/frmGiaoHo.cs:128-135: so theo `TenGiaoHo` + `MaGiaoHoCha` bằng
    /// nhau, hai giáo họ cấp 1 (GiaoHoChaId null) coi là "cùng nhóm"). Thông báo nguyên văn
    /// "Tên giáo họ này đã có. Hãy nhập tên khác" (frmGiaoHo.cs:132).</summary>
    public async Task<(KetQuaGiaoHo KetQua, Guid Id)> Them(TaoGiaoHoRequest yc, CancellationToken ct)
    {
        if (await TrungTen(yc.TenGiaoHo, yc.GiaoHoChaId, null, ct))
            return (KetQuaGiaoHo.TrungTen, Guid.Empty);

        var maHienCo = await db.GiaoHo
            .Select(h => (int?)h.MaGiaoHoCu)
            .MaxAsync(ct) ?? 0;
        var maMoi = await sinhMa.LayMaTiepTheo(boiCanh.GiaoXuId, "GiaoHo", maHienCo, ct);

        var giaoHo = new GiaoHo
        {
            GiaoXuId = boiCanh.GiaoXuId,
            MaGiaoHoCu = maMoi,
            TenGiaoHo = yc.TenGiaoHo,
            GiaoHoChaId = yc.GiaoHoChaId,
        };
        db.GiaoHo.Add(giaoHo);
        await db.SaveChangesAsync(ct);
        return (KetQuaGiaoHo.ThanhCong, giaoHo.Id);
    }

    public async Task<KetQuaGiaoHo> Sua(Guid id, CapNhatGiaoHoRequest yc, CancellationToken ct)
    {
        var giaoHo = await db.GiaoHo.FirstOrDefaultAsync(h => h.Id == id && !h.DaXoa, ct);
        if (giaoHo is null) return KetQuaGiaoHo.KhongTimThay;
        if (await TrungTen(yc.TenGiaoHo, yc.GiaoHoChaId, id, ct))
            return KetQuaGiaoHo.TrungTen;

        giaoHo.TenGiaoHo = yc.TenGiaoHo;
        giaoHo.GiaoHoChaId = yc.GiaoHoChaId;
        giaoHo.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
        return KetQuaGiaoHo.ThanhCong;
    }

    private Task<bool> TrungTen(string tenGiaoHo, Guid? giaoHoChaId, Guid? boQuaId, CancellationToken ct) =>
        db.GiaoHo.AnyAsync(h => !h.DaXoa && h.Id != (boQuaId ?? Guid.Empty)
            && h.TenGiaoHo == tenGiaoHo && h.GiaoHoChaId == giaoHoChaId, ct);
}
