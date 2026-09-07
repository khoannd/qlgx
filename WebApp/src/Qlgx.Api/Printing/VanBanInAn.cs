namespace Qlgx.Api.Printing;

/// <summary>
/// Quy tắc trình bày dùng CHUNG cho mọi mẫu in — sửa một chỗ, áp dụng khắp nơi (thay vì vá
/// riêng từng mẫu). Bắt nguồn từ một lỗi tự phát hiện khi xem PDF mẫu "Lý lịch cá nhân": khi
/// dữ liệu bí tích trống, câu in ra thừa dấu phẩy lửng và chữ "tại" bơ vơ, ví dụ
/// "Số — ngày tại , cha  rửa" — trông cẩu thả trên giấy tờ chính thức của giáo xứ.
///
/// Cách sửa: KHÔNG in nhãn/nối rỗng — bỏ HẲN từng đoạn khi thiếu dữ liệu.
/// </summary>
public static class VanBanInAn
{
    public static string Ngay(DateOnly? d) => d?.ToString("dd/MM/yyyy") ?? "";

    private static string VietHoaChuDau(string s) => s.Length == 0 ? s : char.ToUpperInvariant(s[0]) + s[1..];

    /// <summary>
    /// Ghép câu mô tả một "khoảnh khắc bí tích" kiểu gốc
    /// "Số {so} — ngày {ngay} tại {noi}, cha {chuSu} {hanhDongChuSu}, {nhanNguoiPhu} {nguoiPhu}"
    /// — bỏ HẲN từng đoạn khi thiếu dữ liệu tương ứng, không để lại nhãn/nối rỗng lửng. Trả về
    /// chuỗi rỗng nếu KHÔNG có bất kỳ dữ liệu nào (mẫu gọi nơi dùng tự quyết định có ẩn hẳn dòng
    /// đó hay không).
    /// </summary>
    public static string MoTaBiTich(
        string? so, DateOnly? ngayBiTich, string? noi,
        string? chuSu, string hanhDongChuSu,
        string? nguoiPhu = null, string nhanNguoiPhu = "")
    {
        var ghep = "";
        if (!string.IsNullOrWhiteSpace(so))
            ghep = $"Số {so}";

        var ngayStr = Ngay(ngayBiTich);
        if (!string.IsNullOrWhiteSpace(ngayStr))
            ghep = ghep.Length == 0 ? $"Ngày {ngayStr}" : $"{ghep} — ngày {ngayStr}";

        if (!string.IsNullOrWhiteSpace(noi))
            ghep = ghep.Length == 0 ? $"Tại {noi}" : $"{ghep} tại {noi}";

        if (!string.IsNullOrWhiteSpace(chuSu))
        {
            var doan = $"cha {chuSu} {hanhDongChuSu}";
            ghep = ghep.Length == 0 ? VietHoaChuDau(doan) : $"{ghep}, {doan}";
        }

        if (!string.IsNullOrWhiteSpace(nguoiPhu))
        {
            var doan = $"{nhanNguoiPhu} {nguoiPhu}";
            ghep = ghep.Length == 0 ? VietHoaChuDau(doan) : $"{ghep}, {doan}";
        }

        return ghep;
    }

    /// <summary>Ghép nhiều dòng (đã chuẩn bị sẵn, có thể null/rỗng) thành MỘT khối văn bản nhiều
    /// dòng — bỏ hẳn dòng nào rỗng thay vì để lại dòng trắng. Dùng với CSS
    /// <c>white-space: pre-line</c> ở phía mẫu HTML để giữ xuống dòng (giá trị vẫn đi qua
    /// HtmlEncoder bình thường ở <see cref="BoDoMauIn"/> — chỉ có ký tự '\n' không bị mã hoá,
    /// nên KHÔNG mở đường chèn HTML).</summary>
    public static string GhepDong(params string?[] dong) =>
        string.Join("\n", Array.FindAll(dong, d => !string.IsNullOrWhiteSpace(d)));
}
