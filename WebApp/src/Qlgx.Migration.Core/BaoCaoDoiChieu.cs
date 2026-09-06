using System.Text;

namespace Qlgx.Migration;

/// <summary>
/// So sánh số dòng nguồn/đích theo từng bảng sau khi chuyển đổi. Đây là điều kiện nghiệm thu
/// bắt buộc của công cụ — lưới an toàn duy nhất phát hiện được bảng hay cột bị bỏ sót lúc viết
/// bộ chuyển đổi, vì Access và PostgreSQL không có ràng buộc nào buộc hai bên phải khớp nhau.
/// </summary>
public static class BaoCaoDoiChieu
{
    public record DongLech(string Bang, int SoDongNguon, int SoDongDich);

    /// <summary>
    /// Trả về đúng những bảng có số dòng đích khác số dòng nguồn. Danh sách rỗng nghĩa là mọi
    /// bảng đã khớp. Bảng có trong SoDongNguon nhưng chưa có trong SoDongDich (chế độ chạy thử,
    /// SoDongDich rỗng) cũng được coi là lệch để không bỏ sót trường hợp chưa ghi gì.
    /// </summary>
    public static IReadOnlyList<DongLech> TimBangLech(KetQuaChuyenDoi ketQua) =>
        ketQua.SoDongNguon
            .Select(kv => new DongLech(kv.Key, kv.Value, ketQua.SoDongDich.GetValueOrDefault(kv.Key, -1)))
            .Where(d => d.SoDongDich != d.SoDongNguon)
            .ToList();

    public static string InBang(KetQuaChuyenDoi ketQua)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"{"Bảng",-24}{"Nguồn",8}{"Đích",8}");
        foreach (var (bang, soNguon) in ketQua.SoDongNguon)
        {
            var soDich = ketQua.SoDongDich.TryGetValue(bang, out var v) ? v.ToString() : "-";
            sb.AppendLine($"{bang,-24}{soNguon,8}{soDich,8}");
        }
        return sb.ToString();
    }
}
