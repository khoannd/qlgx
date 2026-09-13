using Microsoft.EntityFrameworkCore;
using Qlgx.Domain.Entities;

namespace Qlgx.Data.NhatKy;

/// <summary>
/// NƠI DUY NHẤT được cấp số thứ tự cho bảng hieu_luc. Mọi chỗ khác phải gọi qua đây.
/// </summary>
public static class CapSoHieuLuc
{
    /// <summary>
    /// Khoá dòng đếm của giáo xứ rồi cấp một dải <paramref name="soLuong"/> số liên tiếp.
    ///
    /// BẮT BUỘC gọi trong CHÍNH giao dịch sẽ ghi hieu_luc, và phải là câu lệnh ĐẦU TIÊN của
    /// giao dịch đó. Hai lý do:
    ///  - Nếu mở một giao dịch riêng rồi commit để lấy số, giao dịch ghi bị huỷ sẽ để lại lỗ
    ///    hổng — đúng cái lỗi mà việc bỏ sequence sinh ra để tránh. Lỗi này CHỈ lộ khi có tải.
    ///  - Nếu khoá bản ghi nghiệp vụ trước rồi mới khoá dòng đếm, hai giao dịch ngược thứ tự
    ///    sẽ deadlock. Giao dịch chạm nhiều giáo xứ phải khoá theo GiaoXuId tăng dần.
    /// </summary>
    public static async Task<(long SoDau, Guid Epoch)> LayDaiSo(
        QlgxDbContext db, Guid giaoXuId, int soLuong, CancellationToken ct)
    {
        if (soLuong <= 0) throw new ArgumentOutOfRangeException(nameof(soLuong));

        // Tạo dòng đếm nếu chưa có. ON CONFLICT DO NOTHING để hai tiến trình cùng tạo không
        // đổ vỡ; dòng SELECT ... FOR UPDATE ngay dưới mới là chỗ giành quyền cấp số.
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO bo_dem_hieu_luc (giao_xu_id, so_tiep_theo, epoch)
            VALUES ({giaoXuId}, 1, gen_random_uuid())
            ON CONFLICT (giao_xu_id) DO NOTHING
            """, ct);

        var bd = await db.Set<BoDemHieuLuc>()
            .FromSql($"SELECT * FROM bo_dem_hieu_luc WHERE giao_xu_id = {giaoXuId} FOR UPDATE")
            .AsNoTracking()
            .SingleAsync(ct);

        var soDau = bd.SoTiepTheo;
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            UPDATE bo_dem_hieu_luc SET so_tiep_theo = so_tiep_theo + {(long)soLuong}
            WHERE giao_xu_id = {giaoXuId}
            """, ct);

        return (soDau, bd.Epoch);
    }
}
