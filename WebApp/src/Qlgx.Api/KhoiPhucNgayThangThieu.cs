using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Qlgx.Data;
using Qlgx.Domain.Entities;

namespace Qlgx.Api;

/// <summary>
/// Khôi phục ngày tháng thiếu (chỉ năm, hoặc tháng+năm) đang nằm kẹt trong cột
/// <c>du_lieu_loi</c> của bảng <c>giao_dan</c> — chạy bằng dòng lệnh
/// <c>dotnet run -- khoi-phuc-ngay-thang-thieu</c>, KHÔNG phải endpoint HTTP.
///
/// Bối cảnh: công cụ chuyển dữ liệu Access (<c>Qlgx.Migration.Core/ChuyenDoiDuLieu.cs</c>) gọi
/// <c>NgayThangText.Doc</c> cho mỗi trường ngày — trước khi sửa, hàm đó chỉ phân giải đúng bốn
/// dạng "dd/MM/yyyy" đầy đủ, nên các giá trị Access lưu thiếu ("1985", "05/1985" — xem
/// <c>Memory.GetDateString</c>, <c>Source/DBAccess/CMemory.cs:755-763</c>, không tự điền 01/01)
/// rơi vào <c>du_lieu_loi</c> thay vì cột ngày tương ứng. Sau khi <c>NgayThangText.Doc</c> được
/// sửa để phân giải thêm "yyyy" và "MM/yyyy" (chuẩn hoá về ngày 01, đúng quyết định "chuẩn hoá
/// lúc nhập" người dùng đã chốt), lệnh này đọc lại <c>du_lieu_loi</c> của các dòng đã chuyển từ
/// trước và điền vào đúng cột ngày — các lần chuyển dữ liệu MỚI (giáo xứ khác, về sau) không cần
/// chạy lệnh này vì <c>ChuyenDoiDuLieu</c> đã tự làm đúng ngay từ đầu.
///
/// CHẠY LẠI ĐƯỢC NHIỀU LẦN KHÔNG HỎNG GÌ (idempotent): chỉ ghi vào cột ngày đang NULL, không
/// bao giờ ghi đè cột đã có giá trị, và KHÔNG xoá/sửa <c>du_lieu_loi</c> — đó là bản gốc từ sổ
/// giấy, cần giữ để đối chiếu sau này (cũng là bằng chứng giá trị gốc chỉ có năm/tháng).
/// </summary>
public static class KhoiPhucNgayThangThieu
{
    // Đúng 5 trường ngày có dữ liệu lỗi thật trong qlgx_thu lúc viết lệnh này (xem
    // docs/superpowers/specs/man-hinh/can-review-sau.md) — nếu về sau phát hiện thêm trường
    // ngày khác của GiaoDan cũng rơi vào du_lieu_loi theo cùng cách, thêm tên vào đây.
    private static readonly (string Khoa, Action<GiaoDan, DateOnly?> Gan, Func<GiaoDan, DateOnly?> Doc)[] TruongNgay =
    [
        ("NgaySinh", (e, v) => e.NgaySinh = v, e => e.NgaySinh),
        ("NgayRuaToi", (e, v) => e.NgayRuaToi = v, e => e.NgayRuaToi),
        ("NgayRuocLe", (e, v) => e.NgayRuocLe = v, e => e.NgayRuocLe),
        ("NgayThemSuc", (e, v) => e.NgayThemSuc = v, e => e.NgayThemSuc),
        ("NgayQuaDoi", (e, v) => e.NgayQuaDoi = v, e => e.NgayQuaDoi),
    ];

    public static async Task Chay(IConfiguration cauHinh)
    {
        // Dùng chuỗi kết nối QUẢN TRỊ (vai trò có BYPASSRLS) — lệnh khôi phục chạy trên TOÀN BỘ
        // giáo xứ (bảo trì dữ liệu một lần), không phải trong ngữ cảnh một giáo xứ đăng nhập.
        var options = new DbContextOptionsBuilder<QlgxDbContext>()
            .UseNpgsql(ChuoiKetNoiQuanTri.Doc(cauHinh))
            .Options;
        await using var db = new QlgxDbContext(options);

        var ungVien = await db.GiaoDan.Where(g => g.DuLieuLoi != null).ToListAsync();

        var soDaSua = 0;
        var soDongDoi = 0;
        var soDongKhongDocDuoc = 0;
        foreach (var e in ungVien)
        {
            Dictionary<string, string>? loi;
            try
            {
                loi = JsonSerializer.Deserialize<Dictionary<string, string>>(e.DuLieuLoi!);
            }
            catch (JsonException)
            {
                soDongKhongDocDuoc++;
                continue; // du_lieu_loi không phải JSON dạng {tên trường: giá trị gốc} — bỏ qua
            }
            if (loi is null) continue;

            var doiDong = false;
            foreach (var (khoa, gan, docHienTai) in TruongNgay)
            {
                if (docHienTai(e) is not null) continue; // đã có ngày đầy đủ — không đụng vào
                if (!loi.TryGetValue(khoa, out var giaTriGoc)) continue;

                var (ngay, _) = NgayThangText.Doc(giaTriGoc);
                if (ngay is null) continue; // vẫn không phân giải được (dữ liệu rác thật) — giữ nguyên trong du_lieu_loi

                gan(e, ngay);
                doiDong = true;
                soDaSua++;
            }
            if (doiDong) soDongDoi++;
        }

        await db.SaveChangesAsync();

        Console.WriteLine(
            $"Da khoi phuc {soDaSua} gia tri ngay thang tren {soDongDoi} dong giao_dan " +
            $"(xet {ungVien.Count} dong co du_lieu_loi, {soDongKhongDocDuoc} dong khong doc duoc du_lieu_loi dang JSON).");
    }
}
