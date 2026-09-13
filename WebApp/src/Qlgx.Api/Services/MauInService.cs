using Microsoft.EntityFrameworkCore;
using Qlgx.Api.Dtos;
using Qlgx.Api.Printing;
using Qlgx.Data;
using Qlgx.Data.NhatKy;
using Qlgx.Domain.Entities;

namespace Qlgx.Api.Services;

public enum KetQuaLuuMauIn { ThanhCong, TenMauKhongHopLe, QuaLon, DungPhienBan }

/// <summary>
/// Màn hình "Quản lý mẫu in" (xem docs/superpowers/specs/man-hinh/quan-ly-mau-in.md) — năng
/// lực MỚI cho phép giáo xứ tự sửa mẫu in RIÊNG của mình và Quản trị hệ thống sửa mẫu
/// TUỲ CHỈNH CHUNG áp dụng cho mọi giáo xứ chưa tự tuỳ chỉnh. Endpoint gọi đúng một trong hai
/// nhóm hàm *Rieng/*HeThong tuỳ policy đã kiểm ở tầng MauInEndpoints — lớp này KHÔNG tự kiểm
/// quyền (đúng quy ước chung: service tin endpoint đã RequireAuthorization/policy đúng), nhưng
/// KHÔNG BAO GIỜ nhận GiaoXuId từ tham số bên ngoài cho các hàm *Rieng — luôn lấy từ
/// <see cref="IBoiCanhGiaoXu"/> (claim đăng nhập).
/// </summary>
public class MauInService(QlgxDbContext db, IBoiCanhGiaoXu boiCanh, BoDoMauIn mau, BoTrinhDuyet trinhDuyet)
{
    public async Task<List<MauInDanhSachItemDto>> LayDanhSach(CancellationToken ct)
    {
        var gxId = boiCanh.GiaoXuId;
        var tenMauRieng = await db.MauInTuyChinh.AsNoTracking()
            .Where(m => m.GiaoXuId == gxId).Select(m => m.TenMau).ToListAsync(ct);
        var tenMauHeThong = await db.MauInTuyChinh.AsNoTracking()
            .Where(m => m.GiaoXuId == null).Select(m => m.TenMau).ToListAsync(ct);

        return MauInCatalog.TatCa.Select(m => new MauInDanhSachItemDto(
            m.TenMau, m.TenHienThi,
            tenMauRieng.Contains(m.TenMau) ? "TuyChinhGiaoXu"
                : tenMauHeThong.Contains(m.TenMau) ? "TuyChinhHeThong" : "MacDinh",
            m.ChoTrong, m.BienKhaDung)).ToList();
    }

    private async Task<MauInChiTietDto?> LayChiTiet(string tenMau, Guid? giaoXuId, CancellationToken ct)
    {
        var moTa = MauInCatalog.Tim(tenMau);
        if (moTa is null) return null;

        var dong = await db.MauInTuyChinh.AsNoTracking()
            .FirstOrDefaultAsync(m => m.GiaoXuId == giaoXuId && m.TenMau == tenMau, ct);
        if (dong is not null)
            return new MauInChiTietDto(tenMau, moTa.TenHienThi, true, dong.NoiDungHtml, dong.RowVersion, moTa.ChoTrong, moTa.BienKhaDung);

        // Chưa tuỳ chỉnh — hiển thị mẫu GỐC nhúng cứng (không phải ô trống) để người dùng thấy
        // đang dùng gì trước khi sửa. "Chung" — chưa có giáo phận riêng nào trong dữ liệu thật
        // hiện tại (xem BoDoMauIn.ChuanHoaTenGiaoPhan), và mẫu tuỳ chỉnh KHÔNG phân biệt theo
        // giáo phận (một giáo xứ chỉ có một bản tuỳ chỉnh mỗi TenMau, xem MauInTuyChinh.cs).
        var goc = mau.DocMauGocCongKhai("Chung", tenMau);
        return new MauInChiTietDto(tenMau, moTa.TenHienThi, false, goc, 0, moTa.ChoTrong, moTa.BienKhaDung);
    }

    public Task<MauInChiTietDto?> LayRieng(string tenMau, CancellationToken ct) =>
        LayChiTiet(tenMau, boiCanh.GiaoXuId, ct);

    public Task<MauInChiTietDto?> LayHeThong(string tenMau, CancellationToken ct) =>
        LayChiTiet(tenMau, null, ct);

    private async Task<KetQuaLuuMauIn> Luu(string tenMau, Guid? giaoXuId, LuuMauInRequest yc, CancellationToken ct)
    {
        if (MauInCatalog.Tim(tenMau) is null) return KetQuaLuuMauIn.TenMauKhongHopLe;
        if (System.Text.Encoding.UTF8.GetByteCount(yc.NoiDungHtml) > MauInHtmlSanitizer.KichThuocToiDa)
            return KetQuaLuuMauIn.QuaLon;

        var noiDungAnToan = MauInHtmlSanitizer.KhuTrung(yc.NoiDungHtml);

        var dong = await db.MauInTuyChinh
            .FirstOrDefaultAsync(m => m.GiaoXuId == giaoXuId && m.TenMau == tenMau, ct);
        if (dong is null)
        {
            // yc.RowVersion == 0 la gia tri canh dau "chua tuy chinh" ma LayChiTiet tra ve —
            // nguoi dung dang luu LAN DAU, tao dong moi thay vi coi la xung dot phien ban.
            db.MauInTuyChinh.Add(new MauInTuyChinh
            {
                GiaoXuId = giaoXuId,
                TenMau = tenMau,
                NoiDungHtml = noiDungAnToan,
            });
            await db.LuuCoNhatKy(ct);
            return KetQuaLuuMauIn.ThanhCong;
        }

        dong.NoiDungHtml = noiDungAnToan;
        dong.UpdatedAt = DateTimeOffset.UtcNow;
        db.Entry(dong).Property(x => x.RowVersion).OriginalValue = yc.RowVersion;
        try
        {
            await db.LuuCoNhatKy(ct);
            return KetQuaLuuMauIn.ThanhCong;
        }
        catch (DbUpdateConcurrencyException) { return KetQuaLuuMauIn.DungPhienBan; }
    }

    public Task<KetQuaLuuMauIn> LuuRieng(string tenMau, LuuMauInRequest yc, CancellationToken ct) =>
        Luu(tenMau, boiCanh.GiaoXuId, yc, ct);

    public Task<KetQuaLuuMauIn> LuuHeThong(string tenMau, LuuMauInRequest yc, CancellationToken ct) =>
        Luu(tenMau, null, yc, ct);

    /// <summary>"Khôi phục về mặc định" — xoá hẳn dòng tuỳ chỉnh (nếu có), rơi về đúng đường
    /// phân giải cũ (mẫu hệ thống nếu có, không thì mẫu gốc — xem InAnService.DungMau). Idempotent
    /// — gọi lại khi đã ở trạng thái mặc định vẫn trả true, không báo lỗi (không có gì để mất).</summary>
    private async Task<bool> KhoiPhuc(string tenMau, Guid? giaoXuId, CancellationToken ct)
    {
        if (MauInCatalog.Tim(tenMau) is null) return false;
        var dong = await db.MauInTuyChinh.FirstOrDefaultAsync(m => m.GiaoXuId == giaoXuId && m.TenMau == tenMau, ct);
        if (dong is not null)
        {
            db.MauInTuyChinh.Remove(dong);
            await db.LuuCoNhatKy(ct);
        }
        return true;
    }

    public Task<bool> KhoiPhucRieng(string tenMau, CancellationToken ct) => KhoiPhuc(tenMau, boiCanh.GiaoXuId, ct);
    public Task<bool> KhoiPhucHeThong(string tenMau, CancellationToken ct) => KhoiPhuc(tenMau, null, ct);

    /// <summary>"Xem thử" — vẽ PDF ngay từ nội dung NHÁP (chưa lưu) người dùng đang gõ, dùng DỮ
    /// LIỆU MẪU (mỗi chỗ trống hiện chuỗi "[Nhãn tiếng Việt]", khối lặp hiện một dòng minh hoạ)
    /// thay vì một bản ghi giáo dân/gia đình thật — người dùng phần lớn không rành máy tính, cần
    /// thấy NGAY kết quả trước khi lưu mà không phải rời màn hình đi tìm một bản ghi cụ thể để
    /// chọn. Không đọc/ghi CSDL mẫu — hoàn toàn không phụ thuộc trạng thái đã lưu.</summary>
    public async Task<byte[]?> XemThu(string tenMau, string noiDungHtmlNhap, CancellationToken ct)
    {
        var moTa = MauInCatalog.Tim(tenMau);
        if (moTa is null) return null;
        if (System.Text.Encoding.UTF8.GetByteCount(noiDungHtmlNhap) > MauInHtmlSanitizer.KichThuocToiDa) return null;

        var antoan = MauInHtmlSanitizer.KhuTrung(noiDungHtmlNhap);

        var duLieu = new Dictionary<string, string?>();
        var khoiHtml = new Dictionary<string, string?>();
        // Duyệt BienKhaDung (siêu tập) chứ KHÔNG phải ChoTrong: người dùng vừa chèn thêm một
        // biến mẫu gốc chưa dùng thì "Xem thử" cũng phải hiện "[Nhãn]" của biến đó, không để lại
        // {{Key}} thô trên bản xem trước.
        foreach (var choTrong in moTa.BienKhaDung)
        {
            if (choTrong.Nhom == NhomBienMauIn.KhoiHeThong)
            {
                // Khối lặp (HangThanhVien/HangDanhSach/DanhSachBiTich/RaoHonPhoi/KhoiAnh...) —
                // một dòng minh hoạ để trình xem trước không trống trơn, không phải dữ liệu thật.
                khoiHtml[choTrong.Key] = choTrong.Key == "KhoiAnh" ? ""
                    : "<tr><td colspan=\"99\">(Dữ liệu mẫu minh hoạ)</td></tr>";
            }
            else
            {
                duLieu[choTrong.Key] = $"[{choTrong.Nhan}]";
            }
        }

        var html = mau.ApDung(antoan, duLieu, khoiHtml);
        return await trinhDuyet.XuatPdfAsync(html, ct);
    }
}
