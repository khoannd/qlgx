using Microsoft.EntityFrameworkCore;

namespace Qlgx.Data.DongBo;

/// <summary>
/// Bộ kiểm bất biến — lưới THỨ HAI, chạy SAU khi một thao tác đã thắng và được áp vào dữ liệu
/// (xem điểm gọi ở <c>DongBoService.ChayLo</c>, ngay sau <c>SaveChangesAsync</c> của Bước 3).
///
/// Đơn vị gộp (<see cref="LuatGop"/>, <c>XoaOConLaiCuaNhom</c>) là lưới THỨ NHẤT: nó chặn phần
/// lớn ca một thao tác VỪA SỬA ĐỤNG tới ô chủ của nhóm (vd. đặt <c>QuaDoi = false</c>) mà không
/// dọn các ô phụ thuộc cùng nhóm. Nhưng nó chỉ hoạt động khi CHÍNH thao tác đang xét đụng vào ô
/// chủ VÀ ô phụ thuộc đã từng có mốc (<c>MocO</c>) — một bản ghi nhập từ Access, sửa tay CSDL,
/// hay một lỗi tương lai ở tầng khác không đi qua đường đó thì lưới thứ nhất không thấy. Bộ kiểm
/// này KHÔNG chặn lô khi thấy vi phạm (đúng triết lý "hệ thống không bao giờ đứng chờ người dùng
/// trả lời", spec 9.2) — nó chỉ sinh một câu cảnh báo, đẩy vào hộp cần xem lại
/// (<c>CanXemLai</c>) với <c>Loai = "mau_thuan_du_lieu"</c>) để người xem sau quyết định.
///
/// CHỈ dispatch đúng bảng của bất biến — không quét mù toàn bộ CSDL mỗi lần gọi.
/// </summary>
public static class KiemBatBien
{
    public static Task<List<string>> Kiem(
        QlgxDbContext db, Guid giaoXuId, string bang, Guid banGhiId, CancellationToken ct)
        => bang switch
        {
            "GiaoDan" => KiemGiaoDan(db, giaoXuId, banGhiId, ct),
            "ThanhVienGiaDinh" => KiemThanhVienGiaDinh(db, giaoXuId, banGhiId, ct),
            _ => Task.FromResult(new List<string>()),
        };

    private static async Task<List<string>> KiemGiaoDan(
        QlgxDbContext db, Guid giaoXuId, Guid banGhiId, CancellationToken ct)
    {
        var gd = await db.GiaoDan.AsNoTracking()
            .FirstOrDefaultAsync(x => x.GiaoXuId == giaoXuId && x.Id == banGhiId, ct);
        // Bản ghi không còn (đã xoá bởi chính thao tác này, hoặc thao tác này là "tao" một bảng
        // khác vô tình trùng Id) — không có gì để kiểm.
        if (gd is null) return [];

        var viPham = new List<string>();

        // Bất biến 1: ngày rửa tội / rước lễ / thêm sức không được trước ngày sinh. Bỏ qua cặp có
        // một vế null — thiếu dữ liệu không phải vi phạm, chỉ là chưa đủ để kết luận.
        ThemNeuNgaySomHonNgaySinh(viPham, gd.NgaySinh, gd.NgayRuaToi, "Ngày rửa tội", gd.HoTen);
        ThemNeuNgaySomHonNgaySinh(viPham, gd.NgaySinh, gd.NgayRuocLe, "Ngày rước lễ", gd.HoTen);
        ThemNeuNgaySomHonNgaySinh(viPham, gd.NgaySinh, gd.NgayThemSuc, "Ngày thêm sức", gd.HoTen);

        // Bất biến 3: còn sống (QuaDoi = false) thì không được còn ngày qua đời — lưới thứ hai
        // cho trạng thái lai mà đơn vị gộp Task 6 đã cố chặn ngay khi ÁP nhưng có thể bỏ lọt
        // (xem chú thích lớp).
        if (!gd.QuaDoi && gd.NgayQuaDoi is { } ngayQuaDoi)
        {
            viPham.Add(
                $"{gd.HoTen} được ghi còn sống nhưng vẫn còn ngày qua đời ({ngayQuaDoi:dd/MM/yyyy}).");
        }

        return viPham;
    }

    private static void ThemNeuNgaySomHonNgaySinh(
        List<string> viPham, DateOnly? ngaySinh, DateOnly? ngayBiTich, string nhanBiTich, string hoTen)
    {
        if (ngaySinh is null || ngayBiTich is null) return;
        if (ngayBiTich.Value >= ngaySinh.Value) return;

        viPham.Add(
            $"{nhanBiTich} ({ngayBiTich:dd/MM/yyyy}) sớm hơn ngày sinh ({ngaySinh:dd/MM/yyyy}) của {hoTen}.");
    }

    /// <summary>
    /// Bất biến 2: một <c>GiaDinhId</c> phải có đúng một dòng <c>ChuHo = true</c>.
    ///
    /// KHÁC với chỉ mục lọc <c>(GiaDinhId, VaiTro) WHERE VaiTro IN (0,1)</c> giữ luật "tối đa
    /// một Chồng, một Vợ" (xem chú thích ở <c>ThanhVienGiaDinh</c>) — đây là luật riêng cho cờ
    /// <c>ChuHo</c>, không liên quan tới vai trò.
    /// </summary>
    private static async Task<List<string>> KiemThanhVienGiaDinh(
        QlgxDbContext db, Guid giaoXuId, Guid banGhiId, CancellationToken ct)
    {
        var thanhVien = await db.ThanhVienGiaDinh.AsNoTracking()
            .FirstOrDefaultAsync(x => x.GiaoXuId == giaoXuId && x.Id == banGhiId, ct);
        if (thanhVien is null) return [];

        var giaDinhId = thanhVien.GiaDinhId;
        var soChuHo = await db.ThanhVienGiaDinh.AsNoTracking()
            .CountAsync(x => x.GiaoXuId == giaoXuId && x.GiaDinhId == giaDinhId && x.ChuHo, ct);

        // 1 là bình thường — không vi phạm.
        if (soChuHo == 1) return [];

        var giaDinh = await db.GiaDinh.AsNoTracking()
            .FirstOrDefaultAsync(x => x.GiaoXuId == giaoXuId && x.Id == giaDinhId, ct);
        var tenGiaDinh = giaDinh?.TenGiaDinh ?? "(chưa đặt tên)";

        return soChuHo == 0
            ? [$"Gia đình {tenGiaDinh} hiện không có ai được đánh dấu chủ hộ."]
            : [$"Gia đình {tenGiaDinh} đang có {soChuHo} người cùng được đánh dấu chủ hộ."];
    }
}
