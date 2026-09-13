using Microsoft.EntityFrameworkCore;
using Qlgx.Api.Dtos;
using Qlgx.Api.Printing;
using Qlgx.Data;
using Qlgx.Data.NhatKy;
using Qlgx.Data.Configurations;
using Qlgx.Domain.Entities;

namespace Qlgx.Api.Services;

public enum KetQuaLuuCachHienThi { ThanhCong, TenBienKhongHopLe, QuaDai, DungPhienBan }

/// <summary>
/// Khu vực "Cách hiển thị dữ liệu đúng/sai" của màn hình "Quản lý mẫu in" (xem
/// docs/superpowers/specs/man-hinh/quan-ly-mau-in.md) — cho giáo xứ tự đặt câu chữ in ra cho các
/// biến đúng/sai thay vì <c>[x]</c>/<c>[  ]</c> nằm cứng (vd biến TanTong: khi đúng in
/// "Tân tòng", khi sai để trống).
///
/// ĐẶT THEO ĐÚNG KHUÔN <see cref="MauInService"/>: endpoint gọi đúng một trong hai nhóm hàm
/// *Rieng/*HeThong tuỳ policy đã kiểm ở tầng CachHienThiDungSaiEndpoints — lớp này KHÔNG tự kiểm
/// quyền (đúng quy ước chung: service tin endpoint đã RequireAuthorization/policy đúng), nhưng
/// KHÔNG BAO GIỜ nhận GiaoXuId từ tham số bên ngoài cho các hàm *Rieng — luôn lấy từ
/// <see cref="IBoiCanhGiaoXu"/> (claim đăng nhập).
///
/// Bảng cach_hien_thi_dung_sai KHÔNG nằm trong bộ lọc toàn cục theo GiaoXuId (xem ghi chú ở
/// CachHienThiDungSai.cs) nên mọi truy vấn dưới đây lọc TAY, tường minh, theo đúng cấp cần đọc.
/// </summary>
public class CachHienThiDungSaiService(QlgxDbContext db, IBoiCanhGiaoXu boiCanh)
{
    /// <summary>Đọc toàn bộ hai cấp trong ĐÚNG HAI truy vấn (không phải một truy vấn mỗi biến) —
    /// danh mục chỉ có 5 biến nhưng nguyên tắc vẫn là không sinh N+1.</summary>
    private async Task<(Dictionary<string, CachHienThiDungSai> Rieng,
                        Dictionary<string, CachHienThiDungSai> HeThong)> DocHaiCap(CancellationToken ct)
    {
        var gxId = boiCanh.GiaoXuId;
        var rieng = await db.CachHienThiDungSai.AsNoTracking()
            .Where(x => x.GiaoXuId == gxId).ToDictionaryAsync(x => x.TenBien, ct);
        var heThong = await db.CachHienThiDungSai.AsNoTracking()
            .Where(x => x.GiaoXuId == null).ToDictionaryAsync(x => x.TenBien, ct);
        return (rieng, heThong);
    }

    private static CachHienThiCapDto Cap(CachHienThiDungSai? dong) =>
        dong is null
            ? new CachHienThiCapDto(false, null, null, 0)
            : new CachHienThiCapDto(true, dong.KhiDung, dong.KhiSai, dong.RowVersion);

    public async Task<List<CachHienThiDungSaiItemDto>> LayDanhSach(CancellationToken ct)
    {
        var (rieng, heThong) = await DocHaiCap(ct);

        return [.. BienDungSaiCatalog.TatCa.Select(b =>
        {
            // Phân giải theo TỪNG BIẾN, đúng thứ tự của InAnService.DungMau: giáo xứ → hệ thống →
            // mặc định gốc. Một dòng tuỳ chỉnh ghi đè CẢ HAI vế của đúng biến đó (KhiDung null
            // hiểu là "không in gì", một lựa chọn hợp lệ — KHÔNG rơi tiếp xuống cấp dưới, nếu
            // không thì giáo xứ sẽ không có cách nào làm cho một vế IM LẶNG).
            var apDung = rieng.GetValueOrDefault(b.Key) ?? heThong.GetValueOrDefault(b.Key);
            var capDangDung =
                rieng.ContainsKey(b.Key) ? "TuyChinhGiaoXu"
                : heThong.ContainsKey(b.Key) ? "TuyChinhHeThong"
                : "MacDinh";

            return new CachHienThiDungSaiItemDto(
                b.Key, b.Nhan, b.Nhom,
                apDung is null ? BienDungSaiCatalog.MacDinhKhiDung : apDung.KhiDung ?? "",
                apDung is null ? BienDungSaiCatalog.MacDinhKhiSai : apDung.KhiSai ?? "",
                capDangDung,
                Cap(rieng.GetValueOrDefault(b.Key)),
                Cap(heThong.GetValueOrDefault(b.Key)));
        })];
    }

    /// <summary>Chuẩn hoá ô người dùng gõ: cắt khoảng trắng thừa hai đầu và quy chuỗi rỗng về
    /// null (hai cách gõ "để trống" phải cho cùng một kết quả trên giấy).
    ///
    /// CỐ Ý KHÔNG khử trùng/lọc HTML ở đây, khác hẳn MauInService (nơi bắt buộc gọi
    /// MauInHtmlSanitizer): câu chữ này KHÔNG phải HTML mẫu, nó đi vào từ điển <c>duLieu</c> mà
    /// <c>BoDoMauIn.ApDung</c> cho chạy qua <c>HtmlEncoder.Default.Encode</c> — gõ
    /// "&lt;script&gt;" thì trên giấy in ra đúng mấy chữ đó, không thành thẻ sống. Tự lọc thêm ở
    /// đây sẽ chỉ làm hỏng câu chữ hợp lệ có dấu &lt; &gt; &amp; (vd "Tuổi &lt; 18"). Có bài test
    /// khẳng định điều này thay vì tin suông (CachHienThiDungSaiTests, phần XSS).</summary>
    private static string? ChuanHoa(string? s)
    {
        var g = s?.Trim();
        return string.IsNullOrEmpty(g) ? null : g;
    }

    private async Task<KetQuaLuuCachHienThi> Luu(
        string tenBien, Guid? giaoXuId, LuuCachHienThiRequest yc, CancellationToken ct)
    {
        if (BienDungSaiCatalog.Tim(tenBien) is null) return KetQuaLuuCachHienThi.TenBienKhongHopLe;

        var khiDung = ChuanHoa(yc.KhiDung);
        var khiSai = ChuanHoa(yc.KhiSai);
        if (khiDung?.Length > CachHienThiDungSaiConfig.DoDaiToiDa ||
            khiSai?.Length > CachHienThiDungSaiConfig.DoDaiToiDa)
            return KetQuaLuuCachHienThi.QuaDai;

        var dong = await db.CachHienThiDungSai
            .FirstOrDefaultAsync(x => x.GiaoXuId == giaoXuId && x.TenBien == tenBien, ct);
        if (dong is null)
        {
            // yc.RowVersion == 0 là giá trị canh dấu "chưa tuỳ chỉnh" mà LayDanhSach trả về —
            // người dùng đang lưu LẦN ĐẦU, tạo dòng mới thay vì coi là xung đột phiên bản.
            db.CachHienThiDungSai.Add(new CachHienThiDungSai
            {
                GiaoXuId = giaoXuId,
                TenBien = tenBien,
                KhiDung = khiDung,
                KhiSai = khiSai,
            });
            await db.LuuCoNhatKy(ct);
            return KetQuaLuuCachHienThi.ThanhCong;
        }

        dong.KhiDung = khiDung;
        dong.KhiSai = khiSai;
        dong.UpdatedAt = DateTimeOffset.UtcNow;
        db.Entry(dong).Property(x => x.RowVersion).OriginalValue = yc.RowVersion;
        try
        {
            await db.LuuCoNhatKy(ct);
            return KetQuaLuuCachHienThi.ThanhCong;
        }
        catch (DbUpdateConcurrencyException) { return KetQuaLuuCachHienThi.DungPhienBan; }
    }

    public Task<KetQuaLuuCachHienThi> LuuRieng(string tenBien, LuuCachHienThiRequest yc, CancellationToken ct) =>
        Luu(tenBien, boiCanh.GiaoXuId, yc, ct);

    public Task<KetQuaLuuCachHienThi> LuuHeThong(string tenBien, LuuCachHienThiRequest yc, CancellationToken ct) =>
        Luu(tenBien, null, yc, ct);

    /// <summary>"Khôi phục mặc định" — xoá hẳn dòng tuỳ chỉnh (nếu có), rơi về đúng đường phân
    /// giải cũ (ánh xạ hệ thống nếu có, không thì "[x]"/"[  ]" gốc). Idempotent — gọi lại khi đã
    /// ở trạng thái mặc định vẫn trả true, không báo lỗi (không có gì để mất).</summary>
    private async Task<bool> KhoiPhuc(string tenBien, Guid? giaoXuId, CancellationToken ct)
    {
        if (BienDungSaiCatalog.Tim(tenBien) is null) return false;
        var dong = await db.CachHienThiDungSai
            .FirstOrDefaultAsync(x => x.GiaoXuId == giaoXuId && x.TenBien == tenBien, ct);
        if (dong is not null)
        {
            db.CachHienThiDungSai.Remove(dong);
            await db.LuuCoNhatKy(ct);
        }
        return true;
    }

    public Task<bool> KhoiPhucRieng(string tenBien, CancellationToken ct) => KhoiPhuc(tenBien, boiCanh.GiaoXuId, ct);
    public Task<bool> KhoiPhucHeThong(string tenBien, CancellationToken ct) => KhoiPhuc(tenBien, null, ct);
}
