using SkiaSharp;

namespace Qlgx.Api.Anh;

/// <summary>Kết quả xử lý một lượt tải ảnh lên — đủ để lưu thẳng vào cột nhị phân
/// (AnhDaiDienDuLieu/AnhDaiDienLoaiNoiDung).</summary>
public sealed record AnhDaXuLy(byte[] DuLieu, string LoaiNoiDung);

/// <summary>
/// Kiểm tra và chuẩn hoá một ảnh đại diện (3x4) trước khi lưu vào CSDL — dùng chung cho giáo
/// dân và gia đình (Task 1.2 VIEC-TIEP-THEO.md).
///
/// BA lớp phòng thủ, không lớp nào tin dữ liệu do trình duyệt tự khai:
///   1. Giới hạn dung lượng tệp gốc TRƯỚC KHI giải mã — chặn ảnh khổng lồ/tệp cố tình to để
///      tốn tài nguyên máy chủ (không đợi giải mã xong mới biết tệp quá to).
///   2. GIẢI MÃ THẬT bằng SkiaSharp thay vì tin phần mở rộng hay Content-Type trình duyệt gửi
///      — một tệp .exe/.zip đổi tên thành ảnh.jpg sẽ giải mã THẤT BẠI ở đây, bị từ chối bất kể
///      tên tệp. `SKCodec.Create` còn cho biết ĐỊNH DẠNG THẬT đã giải mã (EncodedFormat), dùng
///      để chặn định dạng ảnh hợp lệ nhưng KHÔNG nằm trong danh sách cho phép (Gif/Bmp/Ico…).
///   3. Sau khi giải mã thành công, LUÔN thu nhỏ về một khung tối đa rồi nén lại thành WebP —
///      ảnh chân dung 3x4 in ra chỉ cần vài trăm pixel, trong khi ảnh chụp điện thoại có thể
///      tới 12MP/vài chục MB; không thu nhỏ thì CSDL phình rất nhanh (2050 giáo dân × nhiều MB)
///      và tải trang chậm. Chuẩn hoá về CHUNG một định dạng (WebP, Task 4B — nhỏ hơn JPEG cùng
///      chất lượng thị giác, xem lý do chọn ở chỗ mã hoá bên dưới) đơn giản hoá nơi dùng (data
///      URI trong mẫu in, Content-Type khi trả về) cho ảnh MỚI — ảnh cũ đã lưu dạng JPEG vẫn giữ
///      nguyên định dạng của nó (không migration), không cần giữ định dạng gốc của tệp tải lên.
/// </summary>
public static class XuLyAnh
{
    /// <summary>8 MB — đủ rộng cho ảnh chân dung chụp bằng điện thoại hiện đại (thường 3-6MB ở
    /// JPEG chất lượng cao) nhưng vẫn chặn được tệp cố tình rất lớn trước khi giải mã.</summary>
    public const int GioiHanDungLuongGoc = 8 * 1024 * 1024;

    /// <summary>Khung tối đa sau khi thu nhỏ (giữ tỉ lệ) — 480×640px thừa đủ nét cho ảnh 3x4 in
    /// ở 300dpi (kích thước in thật chỉ khoảng 354×472px) và cho khung xem trên màn hình.</summary>
    private const int ChieuDaiToiDa = 640;

    private const int ChatLuongWebp = 80;

    /// <summary>Kiểu nội dung (Content-Type/MIME) của ảnh MỚI sau xử lý — dùng cả khi lưu CSDL
    /// lẫn khi test cần khẳng định mà không viết cứng chuỗi "image/webp" ở nhiều nơi.</summary>
    public const string LoaiNoiDungDauRa = "image/webp";

    /// <summary>Ngân sách dung lượng cho MỘT ảnh sau xử lý. Có test giữ ngưỡng này
    /// (XuLyAnhNganSachTests) — ảnh chiếm khoảng 95% khối lượng sao lưu ở quy mô lớn, nên một
    /// thay đổi vô tình ở tham số nén sẽ nhân đôi kích thước CSDL mà không ai thấy.</summary>
    public const int NganSachByteMoiAnh = 60 * 1024;

    public sealed record LoiXuLyAnh(string ThongBao);

    /// <summary>Kiểm tra + chuẩn hoá một luồng dữ liệu ảnh tải lên. Trả về lỗi (không ném
    /// exception) cho MỌI trường hợp dữ liệu người dùng có vấn đề — chỉ lỗi hạ tầng thật (hết
    /// bộ nhớ...) mới ném exception, để endpoint trả 400 kèm thông báo tiếng Việt rõ ràng thay
    /// vì 500 chung chung.</summary>
    public static (AnhDaXuLy? ketQua, LoiXuLyAnh? loi) XuLy(byte[] duLieuGoc)
    {
        if (duLieuGoc.Length == 0)
            return (null, new LoiXuLyAnh("Tệp tải lên rỗng."));

        if (duLieuGoc.Length > GioiHanDungLuongGoc)
            return (null, new LoiXuLyAnh(
                $"Ảnh quá lớn ({duLieuGoc.Length / 1024 / 1024} MB). Kích thước tối đa cho phép là " +
                $"{GioiHanDungLuongGoc / 1024 / 1024} MB."));

        // Giải mã THẬT — KHÔNG tin phần mở rộng tệp hay Content-Type trình duyệt gửi lên. Một
        // tệp không phải ảnh (hoặc ảnh hỏng) sẽ khiến SKCodec.Create trả null hoặc Bitmap.Decode
        // trả null, cả hai đều bị coi là "không phải ảnh hợp lệ".
        using var luongDuLieu = new SKMemoryStream(duLieuGoc);
        using var codec = SKCodec.Create(luongDuLieu);
        if (codec is null)
            return (null, new LoiXuLyAnh(
                "Tệp này không phải ảnh hợp lệ (đã kiểm tra nội dung thật, không chỉ tên tệp). " +
                "Chỉ nhận ảnh JPEG, PNG hoặc WebP."));

        var dinhDangChoPhep = codec.EncodedFormat is SKEncodedImageFormat.Jpeg
            or SKEncodedImageFormat.Png or SKEncodedImageFormat.Webp;
        if (!dinhDangChoPhep)
            return (null, new LoiXuLyAnh(
                $"Định dạng ảnh '{codec.EncodedFormat}' không được hỗ trợ. " +
                "Chỉ nhận ảnh JPEG, PNG hoặc WebP."));

        using var goc = SKBitmap.Decode(codec);
        if (goc is null)
            return (null, new LoiXuLyAnh("Không đọc được nội dung ảnh — tệp có thể đã hỏng."));

        var tiLe = Math.Min(1.0, (double)ChieuDaiToiDa / Math.Max(goc.Width, goc.Height));
        var rongMoi = Math.Max(1, (int)Math.Round(goc.Width * tiLe));
        var caoMoi = Math.Max(1, (int)Math.Round(goc.Height * tiLe));

        using var daThuNho = tiLe < 1.0
            ? goc.Resize(new SKImageInfo(rongMoi, caoMoi), new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear))
            : goc.Copy();
        if (daThuNho is null)
            return (null, new LoiXuLyAnh("Không xử lý được ảnh này — vui lòng thử một ảnh khác."));

        // Nền trắng trước khi ép JPEG (JPEG không có kênh alpha) — ảnh PNG/WebP có nền trong
        // suốt sẽ không bị biến thành ô vuông đen khi ép định dạng. Dùng Bgra8888 (không phải
        // Rgb888x) vì bộ mã hoá JPEG của SkiaSharp không nhận trực tiếp Rgb888x — đã kiểm
        // chứng: SKBitmap(..., SKColorType.Rgb888x, ...).Encode(Jpeg,...) trả về null trên
        // build này dù dữ liệu ảnh hợp lệ.
        using var mat = new SKBitmap(new SKImageInfo(daThuNho.Width, daThuNho.Height, SKColorType.Bgra8888, SKAlphaType.Premul));
        using (var canvas = new SKCanvas(mat))
        {
            canvas.Clear(SKColors.White);
            canvas.DrawBitmap(daThuNho, 0, 0);
        }

        // WebP thay cho JPEG: đã ĐO THẬT trên ảnh mẫu 1200x1600 (chuyển sắc + nhiễu, cùng đường
        // xử lý thu nhỏ 640px + nền trắng ở trên) trước khi chọn, không giả định — vì bộ mã hoá
        // SkiaSharp trên build này đã từng trả null cho JPEG với Rgb888x (xem ghi chú ở khối nền
        // trắng phía trên), nên WebP hoàn toàn có thể vấp lỗi tương tự. Kết quả: JPEG85=52473B,
        // JPEG80=42500B, WebP80=31036B, WebP85=38654B — WebP80 KHÔNG trả null và nhỏ hơn JPEG85
        // ~41%, nhỏ hơn JPEG80 ~27%, nằm gọn dưới ngân sách 60KB/ảnh. Số byte này ghi trong commit
        // giới thiệu thay đổi này (Task 4B).
        //
        // KHÔNG đổi ảnh cũ đã lưu: cột AnhDaiDienLoaiNoiDung vốn lưu kiểu nội dung theo TỪNG bản
        // ghi, nên ảnh JPEG cũ vẫn đọc và in bình thường bên cạnh ảnh WebP mới — không cần
        // migration, không cần chuyển đổi hàng loạt.
        //
        // Nền trắng ở bước trên vẫn cần giữ dù WebP hỗ trợ kênh alpha: ảnh chân dung 3x4 luôn in
        // trên nền giấy trắng của mẫu in, giữ nền trắng cho nhất quán với ảnh JPEG cũ và tránh nền
        // trong suốt hiển thị khác nhau tuỳ trình xem/trình duyệt.
        using var anhMaHoa = SKImage.FromBitmap(mat);
        using var duLieuNen = anhMaHoa.Encode(SKEncodedImageFormat.Webp, ChatLuongWebp);
        if (duLieuNen is null)
            return (null, new LoiXuLyAnh("Không nén được ảnh sau khi xử lý — vui lòng thử một ảnh khác."));

        return (new AnhDaXuLy(duLieuNen.ToArray(), LoaiNoiDungDauRa), null);
    }
}
