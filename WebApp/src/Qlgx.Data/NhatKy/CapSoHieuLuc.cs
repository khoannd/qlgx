using Microsoft.EntityFrameworkCore;
using Qlgx.Data.DongBo;
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
    /// <remarks>
    /// Trả thêm <c>DauCuoi</c> — đồng hồ lai của CHÍNH máy chủ cho giáo xứ này, đọc từ hai cột
    /// <c>dau_cuoi_vat_ly</c>/<c>dau_cuoi_logic</c> trên đúng dòng vừa khoá (xem
    /// <see cref="BoDemHieuLuc.DauCuoiVatLy"/>). <c>ThietBiId = null</c> vì đây là mốc do máy chủ
    /// phát (null xếp TRƯỚC mọi thiết bị khi hoà — xem <c>DongHoLai.SoSanh</c>);
    /// <c>MaThaoTac = Guid.Empty</c> vì dòng đếm không thuộc về một thao tác nào.
    ///
    /// VÌ SAO đổi chữ ký thay vì thêm một hàm đọc riêng: <c>LayDaiSo</c> là điểm DUY NHẤT mọi
    /// đường ghi đi qua. Nếu đồng hồ nằm chỗ khác, sẽ có đường ghi quên nâng nó — và lỗi đó
    /// không có test nào bắt được một cách tự nhiên (nó chỉ hiện ra thành "bản sửa của quý cha
    /// bị máy con đè" nhiều tháng sau). Đổi chữ ký buộc mọi nơi gọi phải nhìn lại; đó là mục đích.
    /// </remarks>
    public static async Task<(long SoDau, Guid Epoch, DauDongHo DauCuoi)> LayDaiSo(
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

        return (soDau, bd.Epoch, new DauDongHo(bd.DauCuoiVatLy, bd.DauCuoiLogic, null, Guid.Empty));
    }

    /// <summary>
    /// Chốt lại dòng đếm ở CUỐI giao dịch đã gọi <see cref="LayDaiSo"/>: ghi mốc mới nhất máy
    /// chủ vừa phát, và (tuỳ chọn) TRẢ LẠI phần dải số chưa dùng tới.
    ///
    /// Vì sao cần trả lại số: người gọi phải giành khoá dòng đếm NGAY khi mở giao dịch (nếu khoá
    /// bản ghi nghiệp vụ trước rồi mới khoá dòng đếm thì hai giao dịch ngược thứ tự sẽ deadlock —
    /// xem <see cref="LayDaiSo"/>), nhưng lúc đó chưa biết trong lô có bao nhiêu thao tác sẽ
    /// THẮNG cuộc gộp, nên phải xin theo CẬN TRÊN. Không trả lại thì chuỗi <c>so_thu_tu</c> thủng
    /// lỗ đúng bằng số thao tác thua. Trả lại là an toàn tuyệt đối vì khoá <c>FOR UPDATE</c> vẫn
    /// đang giữ: không ai khác có thể đã lấy số trong khoảng đó.
    ///
    /// <c>LEAST</c> chứ không gán thẳng: hàm này chỉ được phép ĐẨY LÙI con số về phần chưa dùng,
    /// không bao giờ đẩy tới. Người gọi tính nhầm theo hướng lớn hơn sẽ không âm thầm cấp trùng
    /// số cho lần sau.
    /// </summary>
    public static async Task ChotDaiSo(
        QlgxDbContext db, Guid giaoXuId, DauDongHo dauCuoi, long? soTiepTheoMoi, CancellationToken ct)
    {
        if (db.Database.CurrentTransaction is null)
            throw new InvalidOperationException(
                "ChotDaiSo phai chay trong chinh giao dich dang giu khoa dong dem (goi sau LayDaiSo).");

        if (soTiepTheoMoi is { } so)
        {
            await db.Database.ExecuteSqlInterpolatedAsync($"""
                UPDATE bo_dem_hieu_luc
                   SET so_tiep_theo   = LEAST(so_tiep_theo, {so}),
                       dau_cuoi_vat_ly = {dauCuoi.VatLy},
                       dau_cuoi_logic  = {dauCuoi.Logic}
                 WHERE giao_xu_id = {giaoXuId}
                """, ct);
            return;
        }

        await db.Database.ExecuteSqlInterpolatedAsync($"""
            UPDATE bo_dem_hieu_luc
               SET dau_cuoi_vat_ly = {dauCuoi.VatLy},
                   dau_cuoi_logic  = {dauCuoi.Logic}
             WHERE giao_xu_id = {giaoXuId}
            """, ct);
    }
}
