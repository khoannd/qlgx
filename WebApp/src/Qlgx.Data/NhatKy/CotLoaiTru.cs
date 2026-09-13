namespace Qlgx.Data.NhatKy;

/// <summary>
/// Cột không bao giờ vào nhật ký.
///
/// Ảnh đại diện là byte[] NẰM NGAY TRONG BẢNG (xem AnhDaiDienService.cs) — một tấm ảnh vào
/// jsonb dưới dạng base64 sẽ phình nhật ký lên gấp nhiều lần dữ liệu thật và làm mọi lượt kéo
/// về của máy con trở nên vô dụng. Ảnh đồng bộ riêng theo mã băm, không đi qua đây.
///
/// Các cột hệ thống bị loại vì chúng đổi ở MỌI lần ghi mà không mang ý nghĩa nghiệp vụ nào —
/// ghi lại chỉ tạo nhiễu và làm mọi lần lưu trông như có thay đổi.
/// </summary>
public static class CotLoaiTru
{
    public static readonly HashSet<string> Ten = new(StringComparer.Ordinal)
    {
        "AnhDaiDienDuLieu",
        "AnhDaiDienLoaiNoiDung",
        "RowVersion",
        "UpdatedAt",
        "CreatedAt",
    };

    public static bool BiLoai(string tenCot) => Ten.Contains(tenCot);
}
