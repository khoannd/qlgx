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
///   3. Sau khi giải mã thành công, LUÔN thu nhỏ về một khung tối đa rồi nén lại thành JPEG —
///      ảnh chân dung 3x4 in ra chỉ cần vài trăm pixel, trong khi ảnh chụp điện thoại có thể
///      tới 12MP/vài chục MB; không thu nhỏ thì CSDL phình rất nhanh (2050 giáo dân × nhiều MB)
///      và tải trang chậm. Chuẩn hoá về CHUNG một định dạng (JPEG) đơn giản hoá nơi dùng (data
///      URI trong mẫu in, Content-Type khi trả về) — không cần giữ định dạng gốc.
/// </summary>
public static class XuLyAnh
{
    /// <summary>8 MB — đủ rộng cho ảnh chân dung chụp bằng điện thoại hiện đại (thường 3-6MB ở
    /// JPEG chất lượng cao) nhưng vẫn chặn được tệp cố tình rất lớn trước khi giải mã.</summary>
    public const int GioiHanDungLuongGoc = 8 * 1024 * 1024;

    /// <summary>Khung tối đa sau khi thu nhỏ (giữ tỉ lệ) — 480×640px thừa đủ nét cho ảnh 3x4 in
    /// ở 300dpi (kích thước in thật chỉ khoảng 354×472px) và cho khung xem trên màn hình.</summary>
    private const int ChieuDaiToiDa = 640;

    private const int ChatLuongJpeg = 85;

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

        using var anhMaHoa = SKImage.FromBitmap(mat);
        using var duLieuNen = anhMaHoa.Encode(SKEncodedImageFormat.Jpeg, ChatLuongJpeg);
        if (duLieuNen is null)
            return (null, new LoiXuLyAnh("Không nén được ảnh sau khi xử lý — vui lòng thử một ảnh khác."));

        return (new AnhDaXuLy(duLieuNen.ToArray(), "image/jpeg"), null);
    }
}
