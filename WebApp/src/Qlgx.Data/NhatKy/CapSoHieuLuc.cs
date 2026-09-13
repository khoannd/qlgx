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
    ///    sẽ deadlock. Trước đây định khuyên "giao dịch chạm nhiều giáo xứ thì khoá theo
    ///    GiaoXuId tăng dần" để né kiểu deadlock này — nhưng dưới vai trò CSDL không BYPASSRLS
    ///    (vai trò THẬT lúc chạy sản phẩm), một phiên chỉ mang được ĐÚNG MỘT app.giao_xu_id tại
    ///    một thời điểm, nên một giao dịch không thể hợp lệ khoá dòng đếm của hai giáo xứ khác
    ///    nhau trong cùng một phiên — lời khuyên đó không còn khả thi trong mô hình RLS này.
    ///
    /// THỨ TỰ KHOÁ GIỮA HAI BỘ ĐẾM — quy ước phải giữ: nếu một giao dịch cần khoá CẢ
    /// bo_dem_ma (SinhMaService.LayMaTiepTheo, cấp MaGiaoDanCu / MaGiaDinhCu ...) LẪN
    /// bo_dem_hieu_luc (hàm này), thì phải khoá bo_dem_ma TRƯỚC, rồi mới tới bo_dem_hieu_luc.
    /// Hiện mọi chỗ gọi đều theo đúng thứ tự đó (sinh mã xong mới LuuCoNhatKy), nên chưa từng
    /// deadlock. Nhưng KHÔNG có gì cưỡng chế bằng máy: một người viết code sau này gọi
    /// LuuCoNhatKy trước rồi mới sinh mã sẽ tạo ra một giao dịch khoá ngược chiều, và hai giáo
    /// dân được tạo đồng thời ở hai máy sẽ deadlock thật — kiểu lỗi chỉ lộ khi có tải, đúng lúc
    /// giáo xứ đang nhập liệu nhiều nhất. Đây là ghi chú phòng ngừa: nếu buộc phải đổi thứ tự,
    /// hãy đổi ở MỌI chỗ gọi cùng lúc, đừng đổi lẻ một chỗ.
    /// </summary>
    public static async Task<(long SoDau, Guid Epoch)> LayDaiSo(
        QlgxDbContext db, Guid giaoXuId, int soLuong, CancellationToken ct)
    {
        if (soLuong <= 0) throw new ArgumentOutOfRangeException(nameof(soLuong));

        // Ngoài giao dịch, FOR UPDATE nhả khoá ngay cuối câu lệnh nên hai lời gọi đồng thời
        // cùng đọc một giá trị rồi cùng cấp trùng số — và lỗi đó chỉ lộ khi có tải. Thà đổ vỡ
        // ngay lúc gọi sai còn hơn để trùng số âm thầm.
        if (db.Database.CurrentTransaction is null)
            throw new InvalidOperationException(
                "LayDaiSo phai chay trong chinh giao dich se ghi hieu_luc. Mo BeginTransactionAsync truoc khi goi.");

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
