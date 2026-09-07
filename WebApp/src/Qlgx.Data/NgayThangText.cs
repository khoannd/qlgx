using System.Globalization;

namespace Qlgx.Data;

/// <summary>
/// Bản Access lưu ngày dưới dạng chuỗi TEXT(20), dùng chuỗi rỗng thay cho NULL — nhưng
/// KHÔNG phải mọi giá trị đều đủ "dd/MM/yyyy". Control nhập ngày desktop (<c>GxDateInput</c>)
/// cho phép nhập thiếu (chỉ năm, hoặc tháng+năm) và <c>Memory.GetDateString</c>
/// (<c>Source/DBAccess/CMemory.cs:755-763</c>) chỉ nối thêm phần người dùng ĐÃ nhập — tức là
/// desktop cố ý lưu nguyên chuỗi thiếu ("1985", "05/1985") xuống Access, không tự điền 01/01.
/// Xem <c>docs/superpowers/specs/man-hinh/ho-tro-nhap-lieu.md</c> mục A.5-A.6 (bằng chứng đầy
/// đủ, kèm trích dẫn dòng).
///
/// Bản web đã chốt phương án "chuẩn hoá lúc nhập/đọc": không thêm cột đánh dấu độ chính xác,
/// chỉ điền cho đủ ngay khi đọc dữ liệu Access cũ — thiếu ngày → điền 01, thiếu cả ngày lẫn
/// tháng → điền 01/01. Giá trị hoàn toàn không phân giải được (không đúng bất kỳ dạng nào dưới
/// đây) vẫn được giữ nguyên văn trong <c>LoiGiuLai</c> để nơi gọi ghi vào cột <c>du_lieu_loi</c>.
/// </summary>
public static class NgayThangText
{
    private static readonly string[] DinhDangDuNgay = ["dd/MM/yyyy", "d/M/yyyy", "dd/M/yyyy", "d/MM/yyyy"];
    private static readonly string[] DinhDangThangNam = ["MM/yyyy", "M/yyyy"];

    public static (DateOnly? Ngay, string? LoiGiuLai) Doc(string? giaTriAccess)
    {
        if (string.IsNullOrWhiteSpace(giaTriAccess))
            return (null, null);

        var thoNguyen = giaTriAccess.Trim();

        if (DateOnly.TryParseExact(thoNguyen, DinhDangDuNgay, CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var ngayDu))
            return (ngayDu, null);

        // Thiếu ngày (chỉ có tháng/năm) → chuẩn hoá về ngày 01 của tháng đó.
        if (DateOnly.TryParseExact(thoNguyen, DinhDangThangNam, CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var ngayThangNam))
            return (new DateOnly(ngayThangNam.Year, ngayThangNam.Month, 1), null);

        // Thiếu cả ngày lẫn tháng (chỉ có năm, đúng 4 chữ số) → chuẩn hoá về 01/01 của năm đó.
        // Chuỗi 3 chữ số trở xuống ("185") KHÔNG phải năm hợp lệ — giữ nguyên vào du_lieu_loi,
        // đúng dữ liệu lỗi thật thấy trong qlgx_thu (ví dụ NgaySinh = "185").
        if (thoNguyen.Length == 4 && int.TryParse(thoNguyen, NumberStyles.None,
                CultureInfo.InvariantCulture, out var nam) && nam is >= 1 and <= 9999)
            return (new DateOnly(nam, 1, 1), null);

        return (null, giaTriAccess);
    }

    public static string Ghi(DateOnly? ngay) =>
        ngay?.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) ?? "";
}
