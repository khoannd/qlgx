using System.Reflection;
using System.Text;
using System.Text.Encodings.Web;

namespace Qlgx.Api.Printing;

/// <summary>
/// Nạp mẫu HTML nhúng sẵn trong assembly (EmbeddedResource, xem Qlgx.Api.csproj) và thay các
/// chỗ trống dạng <c>{{TenCot}}</c> — cùng tinh thần "tìm-thay theo tên cột" của
/// <c>Source/DBAccess/WordEngine.cs</c> (word.Replace(tenCot, giaTri)) nhưng không cần
/// Office Interop: chỉ là thay thế chuỗi trên văn bản HTML thuần rồi đưa cho Playwright vẽ
/// thành PDF (xem BoTrinhDuyet).
///
/// MỖI giá trị thay vào đều được thoát HTML (HtmlEncoder) trước khi chèn — dữ liệu giáo dân
/// (họ tên, ghi chú tự do…) do người dùng nhập, không thoát đúng cách thì một cái tên kiểu
/// <c>&lt;script&gt;</c> gõ nhầm/cố ý sẽ chèn được mã vào PDF xuất ra (dù rủi ro thấp vì PDF
/// chỉ hiển thị cho chính người in, vẫn phải chặn cho đúng nguyên tắc).
/// </summary>
public sealed class BoDoMauIn
{
    /// <summary>Tên giáo phận (đã chuẩn hoá — xem <see cref="ChuanHoaTenGiaoPhan"/>) ứng với
    /// mỗi thư mục mẫu riêng đã nhúng, để chọn mẫu theo giáo phận. Mặc định luôn là "Chung" khi
    /// giáo phận không có mẫu riêng — đúng hành vi "BMT ghi đè Chung, còn lại dùng Chung" của
    /// bản desktop (BIN/Template/BMT/ đè lên BIN/Template/Chung/, xem khảo sát ở đầu nhiệm vụ).</summary>
    private static readonly Assembly ChuaMau = typeof(BoDoMauIn).Assembly;

    /// <summary>Chuẩn hoá tên giáo phận thành một khoá thư mục an toàn (bỏ dấu, bỏ khoảng
    /// trắng) — ví dụ "Ban Mê Thuột" → "BanMeThuot". Chưa có mẫu riêng nào cho giáo phận cụ thể
    /// ở lượt này (dữ liệu thật hiện tại là "Phan Thiết", dùng mẫu Chung); cơ chế chọn theo
    /// thư mục vẫn dựng sẵn để lượt sau chỉ cần thêm thư mục
    /// <c>PrintTemplates/&lt;TenGiaoPhanChuanHoa&gt;/</c> là có mẫu riêng ngay, không cần sửa mã.</summary>
    public static string ChuanHoaTenGiaoPhan(string? tenGiaoPhan)
    {
        if (string.IsNullOrWhiteSpace(tenGiaoPhan)) return "Chung";
        var boDauChar = tenGiaoPhan.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (var c in boDauChar)
        {
            var loai = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
            if (loai != System.Globalization.UnicodeCategory.NonSpacingMark && char.IsLetterOrDigit(c))
                sb.Append(c);
        }
        var ketQua = sb.ToString().Replace("đ", "d").Replace("Đ", "D");
        return ketQua.Length == 0 ? "Chung" : ketQua;
    }

    /// <summary>Đọc nội dung mẫu <paramref name="tenMau"/>.html — thử thư mục riêng của giáo
    /// phận trước (<paramref name="giaoPhanDaChuanHoa"/>), rồi mới rơi về "Chung".</summary>
    private string DocMauGoc(string giaoPhanDaChuanHoa, string tenMau)
    {
        var uuTien = $"Qlgx.Api.PrintTemplates.{giaoPhanDaChuanHoa}.{tenMau}.html";
        var macDinh = $"Qlgx.Api.PrintTemplates.Chung.{tenMau}.html";

        using var stream = ChuaMau.GetManifestResourceStream(uuTien)
            ?? ChuaMau.GetManifestResourceStream(macDinh)
            ?? throw new InvalidOperationException(
                $"Không tìm thấy mẫu in '{tenMau}' (đã thử '{uuTien}' và '{macDinh}').");
        using var reader = new StreamReader(stream, Encoding.UTF8);
        return reader.ReadToEnd();
    }

    /// <summary>Nạp mẫu rồi thay mọi chỗ trống <c>{{Key}}</c> bằng giá trị tương ứng trong
    /// <paramref name="duLieu"/> (đã được thoát HTML). Chỗ trống không có trong
    /// <paramref name="duLieu"/> bị để trống, không ném lỗi — mẫu có thể có nhiều chỗ trống hơn
    /// dữ liệu một số trường hợp không áp dụng (ví dụ mẫu hôn phối trên giấy lý lịch một người
    /// độc thân).</summary>
    public string Dung(string giaoPhanDaChuanHoa, string tenMau, IReadOnlyDictionary<string, string?> duLieu)
    {
        var mau = DocMauGoc(giaoPhanDaChuanHoa, tenMau);
        return System.Text.RegularExpressions.Regex.Replace(mau, @"\{\{(\w+)\}\}", m =>
        {
            var key = m.Groups[1].Value;
            var gia = duLieu.GetValueOrDefault(key);
            return gia is null ? "" : HtmlEncoder.Default.Encode(gia);
        });
    }
}
