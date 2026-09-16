using System.Buffers.Binary;
using FluentAssertions;
using Qlgx.Api.Anh;
using SkiaSharp;

namespace Qlgx.Api.Tests;

/// <summary>
/// Chống "bom giải nén" (decompression bomb) — review I7.
///
/// Lớp phòng thủ theo DUNG LƯỢNG TỆP không đủ: một tệp PNG hoàn toàn hợp lệ, chỉ vài KB, có thể
/// khai báo 25 000 × 25 000 điểm ảnh. <c>SKBitmap.Decode</c> cấp phát khoảng 4 byte cho mỗi điểm
/// ảnh → ~2,5 GB trong MỘT lần gọi, chưa kể bản thu nhỏ và bản Bgra8888 sau đó.
///
/// Vì sao nghiêm trọng hơn vẻ ngoài: đường tải ảnh đại diện mở cho tài khoản THƯỜNG của BẤT KỲ
/// giáo xứ nào (không cần quyền gì đặc biệt), trong khi một container duy nhất phục vụ API lẫn
/// web tĩnh cho MỌI giáo xứ. Một người tải vài tệp như vậy làm sập cả hệ thống, không riêng
/// phiên của họ.
/// </summary>
public class XuLyAnhBomGiaiNenTests
{
    /// <summary>
    /// Dựng một tệp PNG hợp lệ về cấu trúc nhưng chỉ có phần đầu (IHDR + IEND) — KHÔNG có dữ
    /// liệu điểm ảnh. Đúng hình dạng của một quả bom giải nén: vài chục byte trên đĩa, khai báo
    /// kích thước khổng lồ. Test phải khẳng định hệ thống từ chối tệp này TRƯỚC khi giải mã, nên
    /// cố ý KHÔNG dựng ảnh thật 25 000 × 25 000 (dựng được thì chính test đã ngốn 2,5 GB).
    /// </summary>
    private static byte[] PngChiCoPhanDau(int rong, int cao)
    {
        var ms = new MemoryStream();
        ms.Write([0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]);

        var ihdr = new byte[13];
        BinaryPrimitives.WriteInt32BigEndian(ihdr.AsSpan(0), rong);
        BinaryPrimitives.WriteInt32BigEndian(ihdr.AsSpan(4), cao);
        ihdr[8] = 8;  // bit depth
        ihdr[9] = 2;  // color type: truecolour (RGB)
        ihdr[10] = 0; // compression
        ihdr[11] = 0; // filter
        ihdr[12] = 0; // interlace
        GhiKhoi(ms, "IHDR", ihdr);
        // Một khối IDAT rất ngắn, nén hợp lệ nhưng KHÔNG đủ dữ liệu cho kích thước đã khai báo.
        // Cần có nó thì bộ giải mã PNG mới chịu nhận tệp (không có IDAT thì SKCodec.Create trả
        // null và tệp bị chặn ở nhánh "không phải ảnh hợp lệ" — đúng mã trạng thái nhưng SAI
        // nhánh, không chứng minh được hàng rào số điểm ảnh).
        var nen = new MemoryStream();
        using (var zl = new System.IO.Compression.ZLibStream(nen, System.IO.Compression.CompressionLevel.Fastest, leaveOpen: true))
            zl.Write(new byte[64]);
        GhiKhoi(ms, "IDAT", nen.ToArray());
        GhiKhoi(ms, "IEND", []);
        return ms.ToArray();
    }

    private static void GhiKhoi(Stream ra, string loai, byte[] duLieu)
    {
        Span<byte> doDai = stackalloc byte[4];
        BinaryPrimitives.WriteInt32BigEndian(doDai, duLieu.Length);
        ra.Write(doDai);
        var kieu = System.Text.Encoding.ASCII.GetBytes(loai);
        ra.Write(kieu);
        ra.Write(duLieu);
        Span<byte> crc = stackalloc byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(crc, Crc32([.. kieu, .. duLieu]));
        ra.Write(crc);
    }

    /// <summary>CRC32 theo chuẩn PNG (đa thức 0xEDB88320) — tự cài vì .NET không có sẵn trong
    /// thư viện chuẩn và không đáng kéo thêm một gói NuGet cho vài dòng.</summary>
    private static uint Crc32(byte[] duLieu)
    {
        var crc = 0xFFFFFFFFu;
        foreach (var b in duLieu)
        {
            crc ^= b;
            for (var i = 0; i < 8; i++)
                crc = (crc & 1) != 0 ? (crc >> 1) ^ 0xEDB88320u : crc >> 1;
        }
        return crc ^ 0xFFFFFFFFu;
    }

    [Fact]
    public void Anh_khai_bao_qua_nhieu_diem_anh_bi_tu_choi_truoc_khi_giai_ma()
    {
        var bom = PngChiCoPhanDau(25_000, 25_000);
        bom.Length.Should().BeLessThan(1024,
            "day phai la mot tep NHO khai bao kich thuoc khong lo — dung hinh dang cua bom giai nen");

        var (ketQua, loi) = XuLyAnh.XuLy(bom);

        ketQua.Should().BeNull();
        loi.Should().NotBeNull();
        loi!.ThongBao.Should().Contain("điểm ảnh",
            "thong bao phai noi ro ly do bang tieng Viet nhu moi nhanh loi khac");
    }

    /// <summary>Mặt kia: hàng rào mới KHÔNG được chặn nhầm ảnh chụp điện thoại bình thường.
    /// 50 triệu điểm ảnh là thừa rộng — máy ảnh điện thoại cao cấp nhất hiện nay khoảng 50 MP,
    /// và ảnh chân dung 3x4 thực tế nhỏ hơn nhiều.</summary>
    [Fact]
    public void Anh_chup_dien_thoai_binh_thuong_van_qua()
    {
        using var bm = new SKBitmap(1200, 1600);
        using (var canvas = new SKCanvas(bm)) canvas.Clear(SKColors.Bisque);
        using var anh = SKImage.FromBitmap(bm);
        using var duLieu = anh.Encode(SKEncodedImageFormat.Jpeg, 90);

        var (ketQua, loi) = XuLyAnh.XuLy(duLieu.ToArray());

        loi.Should().BeNull();
        ketQua.Should().NotBeNull();
    }
}
