using Microsoft.EntityFrameworkCore;
using Qlgx.Data;

namespace Qlgx.Api.Services;

/// <summary>
/// Xoay <c>epoch</c> sau khi máy chủ được khôi phục từ bản sao lưu — thiết kế đầy đủ ở spec mục
/// 4.8. Đây là RANH GIỚI giữa hai nhánh công việc (đồng bộ và sao lưu): quy trình khôi phục gọi
/// đúng MỘT thao tác này sau khi nạp xong bản sao lưu và TRƯỚC khi mở lại cho người dùng.
///
/// BẮT BUỘC có tham số chế độ, không có mặc định: máy chủ không thể tự đoán đang ở trường hợp
/// nào trong hai trường hợp ngược nhau — khôi phục sau sự cố (MUỐN máy con đẩy dữ liệu đã mất
/// trở lên) hay quay lui có chủ ý (TUYỆT ĐỐI không muốn máy con tự đẩy đống hỏng đó ngược lên,
/// làm thế là vô hiệu hoá chính thao tác quay lui). Đoán sai thì hỏng theo hai kiểu ngược nhau —
/// đây là một trong rất ít chỗ trong hệ thống buộc con người phải quyết định.
///
/// VÌ SAO tự mở kết nối QUẢN TRỊ (BYPASSRLS) thay vì dùng <c>QlgxDbContext</c> tiêm qua DI —
/// điểm này KHÔNG có trong brief và là một cái bẫy im lặng: bảng <c>bo_dem_hieu_luc</c> đã bật
/// Row-Level Security với chính sách <c>giao_xu_id::text = current_setting('app.giao_xu_id')</c>
/// (migration ThemBangNhatKyThayDoi). Một câu UPDATE "xoay cho MỌI giáo xứ" chạy trên kết nối
/// nghiệp vụ sẽ chỉ đụng được ĐÚNG giáo xứ trong phiên đăng nhập của quản trị viên, và im lặng
/// bỏ qua mọi giáo xứ khác — đúng lúc cả máy chủ vừa được khôi phục. Ở dev/test lỗi này còn
/// không lộ ra (vai trò <c>postgres</c> superuser tự bỏ qua RLS), nên nó chỉ hiện ra trên máy
/// thật. Cùng khuôn với <see cref="QuanLyGiaoXuService"/>, và lớp phòng thủ vẫn là policy
/// "QuanTriHeThong" ở endpoint.
///
/// KHÔNG mở giao dịch chồng lên giao dịch của <see cref="DongBoService"/>: đây là một hành động
/// quản trị độc lập, chạy khi chưa mở lại cho người dùng, và một câu UPDATE đơn đã là nguyên tử.
/// </summary>
public class KhoiPhucDongBoService(IConfiguration cauHinh)
{
    /// <summary>Khôi phục sau sự cố — MỞ cửa cho máy con bù lại dữ liệu máy chủ đã mất.</summary>
    public const string CheDoLayLai = "lay_lai";

    /// <summary>Quay lui có chủ ý — ĐÓNG cửa, máy con không được đẩy phần đã bị bỏ ngược lên.</summary>
    public const string CheDoBoHan = "bo_han";

    /// <summary>
    /// Xoay <c>epoch</c> cho một giáo xứ (<paramref name="giaoXuId"/> có giá trị) hoặc cho MỌI
    /// giáo xứ (<paramref name="giaoXuId"/> rỗng — khôi phục toàn bộ máy chủ).
    ///
    /// Trả về số dòng đếm đã xoay, hoặc <c>null</c> khi <paramref name="cheDo"/> không hợp lệ —
    /// endpoint biến thành 400. So khớp CHÍNH XÁC chuỗi, không bỏ qua hoa/thường và không đoán
    /// ý: một chuỗi lạ lọt qua thành "bo_han" sẽ âm thầm vứt 8 giờ dữ liệu của giáo xứ.
    /// </summary>
    public async Task<int?> XoayEpoch(Guid? giaoXuId, string cheDo, CancellationToken ct)
    {
        if (cheDo is not (CheDoLayLai or CheDoBoHan)) return null;

        // gen_random_uuid() nằm TRONG câu SQL để mỗi giáo xứ nhận một epoch RIÊNG. Sinh một Guid
        // ở C# rồi gán cho mọi dòng sẽ cho tất cả giáo xứ chung một epoch — hai chuỗi số thứ tự
        // khác nhau mang cùng một danh tính, đúng kiểu lẫn lộn mà epoch sinh ra để chặn.
        //
        // C2 (BẮT BUỘC, review vòng sửa 1 của Task 8): `so_thu_tu_luc_xoay = so_tiep_theo - 1`
        // ĐỌC `so_tiep_theo` NGAY TRONG câu UPDATE này, dưới khoá hàng đang được ĐÚNG câu UPDATE
        // này giữ — KHÔNG tách thành một SELECT riêng chạy TRƯỚC, để tránh race giữa đọc và ghi
        // (một giao dịch ghi khác chen vào giữa SELECT và UPDATE sẽ làm ngưỡng này SAI ngay từ
        // lúc xoay). Xem `BoDemHieuLuc.SoThuTuLucXoay` vì sao cột này BẮT BUỘC phải khác
        // `SoTiepTheo` (con trỏ hiện tại, tiếp tục tăng sau khi xoay) — dùng `SoTiepTheo` đọc lại
        // SAU NÀY (lúc máy con hỏi) làm ngưỡng lọc sổ đã nhận phía máy con (Task 8) sẽ bỏ sót các
        // dòng bị mất nếu có ai ghi thêm gì đó SAU khi xoay epoch nhưng TRƯỚC KHI máy con kịp hỏi.
        const string cauLenh = """
            UPDATE bo_dem_hieu_luc
               SET epoch = gen_random_uuid(),
                   cho_phep_bu_lai = {0},
                   xoay_epoch_luc = {1},
                   so_thu_tu_luc_xoay = so_tiep_theo - 1
            """;

        // TẠO TRƯỚC dòng đếm còn thiếu. Một giáo xứ chưa từng có lần ghi nào đi qua giao thức
        // đồng bộ thì chưa có dòng bo_dem_hieu_luc, và khi đó câu UPDATE ở dưới đụng 0 dòng: lệnh
        // xoay báo thành công nhưng KHÔNG đặt được chế độ nào cả. Lần đồng bộ kế tiếp tự tạo dòng
        // với mặc định cho_phep_bu_lai = false, nên máy con bù lại bị từ chối dù quản trị viên vừa
        // chọn "lấy lại" — im lặng, và đúng ngay sau một lần khôi phục. (Bẫy này lộ ra khi chạy
        // RIÊNG LẺ từng test: chạy cả lớp thì test trước đã tạo dòng hộ.)
        //
        // Lấy danh sách từ chính bảng giao_xu: một giaoXuId không có thật sẽ không tạo dòng nào,
        // và câu trả về "đã xoay 0 giáo xứ" nói đúng sự thật thay vì tạo một dòng đếm mồ côi.
        const string cauLenhTao = """
            INSERT INTO bo_dem_hieu_luc (giao_xu_id, so_tiep_theo, epoch)
            SELECT id, 1, gen_random_uuid() FROM giao_xu
            """;

        var choPhepBuLai = cheDo == CheDoLayLai;
        var bayGio = DateTimeOffset.UtcNow;

        await using var db = MoContextQuanTri();
        if (giaoXuId is { } gx)
        {
            await db.Database.ExecuteSqlRawAsync(
                cauLenhTao + " WHERE id = {0} ON CONFLICT (giao_xu_id) DO NOTHING", [gx], ct);
            return await db.Database.ExecuteSqlRawAsync(
                cauLenh + " WHERE giao_xu_id = {2}", [choPhepBuLai, bayGio, gx], ct);
        }

        await db.Database.ExecuteSqlRawAsync(
            cauLenhTao + " ON CONFLICT (giao_xu_id) DO NOTHING", [], ct);
        return await db.Database.ExecuteSqlRawAsync(cauLenh, [choPhepBuLai, bayGio], ct);
    }

    private QlgxDbContext MoContextQuanTri() => new(
        new DbContextOptionsBuilder<QlgxDbContext>()
            .UseNpgsql(ChuoiKetNoiQuanTri.Doc(cauHinh))
            .Options);
}
