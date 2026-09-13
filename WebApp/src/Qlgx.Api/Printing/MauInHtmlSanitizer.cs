using Ganss.Xss;

namespace Qlgx.Api.Printing;

/// <summary>
/// Khử trùng nội dung HTML của mẫu in tuỳ chỉnh Ở TẦNG MÁY CHỦ — ranh giới tin cậy thật, xem
/// quan-ly-mau-in.md mục "Lỗ hổng bảo mật". KHÔNG bao giờ chỉ tin trình soạn thảo rich-text phía
/// trình duyệt: một yêu cầu HTTP thủ công (curl/Postman) hoàn toàn có thể bỏ qua trình soạn
/// thảo và gửi thẳng HTML độc lên endpoint lưu mẫu.
///
/// Dùng thư viện HtmlSanitizer (mganss/HtmlSanitizer, giấy phép MIT) thay vì tự viết regex loại
/// bỏ &lt;script&gt;/on*/javascript: — regex tự chế rất dễ bị vượt qua bằng HTML dị dạng
/// (viết hoa thường lẫn lộn, thẻ lồng nhau, thực thể HTML mã hoá...); một thư viện chuyên dụng
/// dựng cây DOM thật (AngleSharp) rồi lọc mới đáng tin cho một ranh giới bảo mật thật.
///
/// Cấu hình CHỈ cho phép thẻ/thuộc tính cần cho một mẫu in tĩnh (văn bản định dạng + bảng +
/// ảnh nhúng data: URI) — KHÔNG cho &lt;script&gt;, &lt;iframe&gt;, &lt;object&gt;, thuộc tính
/// on*, hay href/src bắt đầu bằng "javascript:"/"http(s)://" (chỉ "data:" được phép, khớp đúng
/// lớp phòng thủ Playwright chặn mạng ở BoTrinhDuyet — hai lớp cùng một chủ đích "mẫu in không
/// được gọi ra ngoài").
/// </summary>
public static class MauInHtmlSanitizer
{
    /// <summary>Cỡ tối đa một mẫu in tuỳ chỉnh được lưu — 300KB, đủ rộng cho một mẫu HTML/CSS
    /// giàu định dạng (mẫu gốc lớn nhất hiện tại ~6KB) nhưng chặn được việc nhồi nhét nội dung
    /// khổng lồ (ảnh base64 quá lớn, DoS bộ nhớ khi Playwright vẽ PDF) — kiểm ở tầng endpoint
    /// TRƯỚC khi khử trùng (khử trùng cây DOM lớn cũng tốn tài nguyên).</summary>
    public const int KichThuocToiDa = 300 * 1024;

    private static readonly HtmlSanitizer BoLoc = TaoBoLoc();

    private static HtmlSanitizer TaoBoLoc()
    {
        // CỐ Ý dùng nguyên danh sách AllowedTags/AllowedAttributes/AllowedCssProperties MẶC
        // ĐỊNH của thư viện thay vì tự chế một danh sách hẹp: 12 mẫu gốc là TÀI LIỆU HTML ĐẦY
        // ĐỦ (<!doctype><html><head><style>...</style></head><body>...) với CSS layout khá
        // phong phú (table-layout, @page qua page-break-*, flexbox ở một số mẫu) — một danh
        // sách tự chế hẹp (thử ban đầu) đã ÂM THẦM XOÁ SẠCH khối <style> (tag "style" không có
        // trong AllowedTags tự chế) khiến bản tuỳ chỉnh ĐẦU TIÊN của mọi mẫu mất hết định dạng
        // (phát hiện nhờ MauInTests.Giao_xu_tu_sua_mau_rieng_uu_tien_hon_mau_he_thong — PDF sau
        // "tuỳ chỉnh" lại NHỎ HƠN PDF gốc, dấu hiệu mất nội dung chứ không phải thêm nội dung).
        // Danh sách mặc định của HtmlSanitizer đã LOẠI BỎ script/iframe/object/on*/style="behavior"
        // theo đúng mục đích của thư viện — chỉ cần SIẾT THÊM đúng hai điểm quan trọng với mẫu in:
        //   1. AllowedSchemes: mặc định cho phép http/https — XIẾT xuống CHỈ "data" (ảnh nhúng
        //      base64) để href/src không bao giờ trỏ ra ngoài, khớp lớp phòng thủ chặn mạng của
        //      BoTrinhDuyet (phòng thủ theo chiều sâu: chặn cả lúc lưu lẫn lúc vẽ PDF).
        //   2. Thêm "style"/"meta"/"title" vào AllowedTags (mặc định KHÔNG có ba thẻ này) để
        //      giữ được cấu trúc tài liệu đầy đủ của mẫu gốc khi người dùng sửa rồi lưu lại.
        //   3. Thêm thuộc tính "class" vào AllowedAttributes — mặc định KHÔNG có "class" (kiểm
        //      bằng cách in ra danh sách mặc định lúc viết lớp này), nhưng CẢ 12 mẫu gốc định
        //      dạng HOÀN TOÀN bằng CSS chọn theo class (".quoc-hieu", ".giao-xu-info"...) — thiếu
        //      "class" thì khối <style> vẫn còn nguyên nhưng KHÔNG CÒN PHẦN TỬ NÀO KHỚP chọn lọc
        //      CSS đó nữa, mẫu mất sạch định dạng ngay lần lưu đầu tiên dù không ai chạm tới
        //      trình soạn thảo rich-text (phát hiện nhờ so sánh tệp .html gốc với kết quả
        //      KhuTrung bằng mắt thường — kích thước byte gần như không đổi nhưng "class" biến
        //      mất khỏi MỌI thẻ, xem lịch sử sửa lỗi commit này).
        var loc = new HtmlSanitizer();
        loc.AllowedTags.Add("style");
        loc.AllowedTags.Add("meta");
        loc.AllowedTags.Add("title");
        loc.AllowedAttributes.Add("class");

        loc.AllowedSchemes.Clear();
        loc.AllowedSchemes.Add("data"); // CHỈ data: URI (ảnh nhúng base64) — không http(s)/javascript.

        return loc;
    }

    /// <summary>Khử trùng và trả về HTML an toàn để lưu/vẽ PDF. Không ném lỗi — HTML sau khử
    /// trùng luôn hợp lệ dù đầu vào có dị dạng đến đâu (HtmlSanitizer tự phục hồi qua
    /// AngleSharp), chỉ mất phần không hợp lệ.</summary>
    public static string KhuTrung(string htmlTho) => BoLoc.Sanitize(htmlTho);
}
