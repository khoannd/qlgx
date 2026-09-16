using FluentAssertions;
using Qlgx.Api.Anh;
using SkiaSharp;

namespace Qlgx.Api.Tests;

/// <summary>
/// Hàng rào chống thoái lui về DUNG LƯỢNG. Không có test này thì một thay đổi vô tình ở tham số
/// nén sẽ nhân đôi kích thước cơ sở dữ liệu mà không ai phát hiện cho tới lúc quá muộn — ảnh
/// chiếm khoảng 95% khối lượng sao lưu ở quy mô lớn (xem thiết kế mục 13.1).
/// </summary>
public class XuLyAnhNganSachTests
{
    /// <summary>Ảnh chân dung tổng hợp có độ phức tạp gần với ảnh chụp thật: chuyển sắc cộng
    /// nhiễu giả. Ảnh một màu phẳng nén xuống vài trăm byte và sẽ làm test vô nghĩa.</summary>
    private static byte[] AnhChanDungMau(int rong = 1200, int cao = 1600)
    {
        using var bm = new SKBitmap(rong, cao);
        var ngau = new Random(20260913);
        for (var y = 0; y < cao; y++)
        for (var x = 0; x < rong; x++)
            bm.SetPixel(x, y, new SKColor(
                (byte)((x * 255 / rong + ngau.Next(24)) % 256),
                (byte)((y * 255 / cao + ngau.Next(24)) % 256),
                (byte)((x + y) % 256)));
        using var anh = SKImage.FromBitmap(bm);
        using var duLieu = anh.Encode(SKEncodedImageFormat.Jpeg, 95);
        return duLieu.ToArray();
    }

    [Fact]
    public void Anh_chan_dung_sau_xu_ly_khong_vuot_ngan_sach()
    {
        var (ketQua, loi) = XuLyAnh.XuLy(AnhChanDungMau());

        loi.Should().BeNull();
        ketQua!.DuLieu.Length.Should().BeLessThanOrEqualTo(XuLyAnh.NganSachByteMoiAnh,
            "anh chiem ~95% khoi luong sao luu o quy mo lon — vuot ngan sach la loi that");
    }

    [Fact]
    public void Van_giu_du_do_phan_giai_de_in_3x4_o_300dpi()
    {
        var (ketQua, _) = XuLyAnh.XuLy(AnhChanDungMau());

        using var daGiaiMa = SKBitmap.Decode(ketQua!.DuLieu);
        // 3x4cm o 300dpi = 354x472px. Canh dai phai >= 472 thi in moi khong bi ro net.
        Math.Max(daGiaiMa.Width, daGiaiMa.Height).Should().BeGreaterThanOrEqualTo(472);
    }

    [Fact]
    public void Loai_noi_dung_tra_ve_khop_dinh_dang_thuc_te_cua_du_lieu()
    {
        var (ketQua, _) = XuLyAnh.XuLy(AnhChanDungMau());

        using var luong = new SKMemoryStream(ketQua!.DuLieu);
        using var codec = SKCodec.Create(luong);
        var mongDoi = codec!.EncodedFormat switch
        {
            SKEncodedImageFormat.Webp => "image/webp",
            SKEncodedImageFormat.Jpeg => "image/jpeg",
            _ => "khong-mong-doi",
        };
        ketQua.LoaiNoiDung.Should().Be(mongDoi);
    }

    [Fact]
    public void Anh_nho_hon_khung_khong_bi_phong_to_len()
    {
        var (ketQua, _) = XuLyAnh.XuLy(AnhChanDungMau(300, 400));

        using var daGiaiMa = SKBitmap.Decode(ketQua!.DuLieu);
        daGiaiMa.Width.Should().Be(300);
        daGiaiMa.Height.Should().Be(400);
    }
}
