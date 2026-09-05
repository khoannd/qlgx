using System.Globalization;

namespace Qlgx.Data;

/// <summary>
/// Bản Access lưu mọi ngày dưới dạng chuỗi "dd/MM/yyyy" trong cột TEXT(20), và dùng
/// chuỗi rỗng thay cho NULL. Giá trị không phân giải được KHÔNG bị vứt đi mà trả về
/// trong <c>LoiGiuLai</c> để nơi gọi ghi vào cột du_lieu_loi.
/// </summary>
public static class NgayThangText
{
    private static readonly string[] DinhDang = ["dd/MM/yyyy", "d/M/yyyy", "dd/M/yyyy", "d/MM/yyyy"];

    public static (DateOnly? Ngay, string? LoiGiuLai) Doc(string? giaTriAccess)
    {
        if (string.IsNullOrWhiteSpace(giaTriAccess))
            return (null, null);

        var thoNguyen = giaTriAccess.Trim();
        if (DateOnly.TryParseExact(thoNguyen, DinhDang, CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var ngay))
            return (ngay, null);

        return (null, giaTriAccess);
    }

    public static string Ghi(DateOnly? ngay) =>
        ngay?.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) ?? "";
}
