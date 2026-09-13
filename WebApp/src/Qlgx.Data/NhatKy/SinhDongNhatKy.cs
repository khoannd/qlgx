using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Qlgx.Domain.Entities;

namespace Qlgx.Data.NhatKy;

/// <summary>
/// Đọc ChangeTracker sinh ra các dòng thay_doi. Ghi ở mức TỪNG Ô (trừ khi thêm mới) — đây là
/// điều làm nên khác biệt: hai người sửa hai ô khác nhau của cùng một hồ sơ sẽ gộp được tự
/// động, thay vì một người bị chặn như cơ chế xmin hiện tại.
/// </summary>
public static class SinhDongNhatKy
{
    public static List<ThayDoi> Tu(
        ChangeTracker theoDoi, IBoiCanhGhiNhatKy? boiCanh, DateTimeOffset moc, Guid giaoDichId)
    {
        var ketQua = new List<ThayDoi>();

        foreach (var muc in theoDoi.Entries<ThucTheCoSo>())
        {
            if (muc.State is not (EntityState.Added or EntityState.Modified)) continue;

            var bang = muc.Metadata.ClrType.Name;
            var thucThe = muc.Entity;

            if (muc.State == EntityState.Added)
            {
                // Ghi cả bản ghi trong MỘT dòng. Máy chủ khai triển nó thành từng ô ngay khi
                // nhận (xem thiết kế mục 4.1) — không bao giờ áp nguyên khối, vì áp nguyên
                // khối sẽ đè lên các ô đã gộp riêng.
                var cacO = muc.Properties
                    .Where(p => !CotLoaiTru.BiLoai(p.Metadata.Name))
                    .ToDictionary(p => p.Metadata.Name, p => p.CurrentValue);

                ketQua.Add(TaoDong(thucThe, bang, "", JsonSerializer.Serialize(cacO),
                    "tao", boiCanh, moc, giaoDichId));
                continue;
            }

            foreach (var o in muc.Properties)
            {
                if (!o.IsModified) continue;
                if (CotLoaiTru.BiLoai(o.Metadata.Name)) continue;
                if (Equals(o.OriginalValue, o.CurrentValue)) continue;

                ketQua.Add(TaoDong(thucThe, bang, o.Metadata.Name,
                    o.CurrentValue is null ? null : JsonSerializer.Serialize(o.CurrentValue),
                    "sua", boiCanh, moc, giaoDichId));
            }
        }

        return ketQua;
    }

    private static ThayDoi TaoDong(
        ThucTheCoSo thucThe, string bang, string truong, string? giaTri, string loai,
        IBoiCanhGhiNhatKy? boiCanh, DateTimeOffset moc, Guid giaoDichId) => new()
    {
        GiaoXuId = thucThe.GiaoXuId,
        Bang = bang,
        BanGhiId = thucThe.Id,
        Truong = truong,
        GiaTri = giaTri,
        Loai = loai,
        DongHoVatLy = moc,
        DongHoLogic = 0,
        TaiKhoanId = boiCanh?.TaiKhoanId,
        ThietBiId = boiCanh?.ThietBiId,
        MaThaoTac = Guid.NewGuid(),
        GiaoDichId = giaoDichId,
        Thang = true,
    };
}
