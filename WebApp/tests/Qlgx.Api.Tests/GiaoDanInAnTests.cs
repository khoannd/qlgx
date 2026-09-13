using System.Net;
using System.Text;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Qlgx.Domain.Entities;

namespace Qlgx.Api.Tests;

/// <summary>Kiểm thử hạ tầng in ấn (VIEC-TIEP-THEO.md mục 1.1) qua mẫu đầu tiên "Lý lịch cá
/// nhân" — xem docs/superpowers/specs/man-hinh/in-an.md. Không kiểm tra nội dung PDF chi tiết
/// (nhị phân, không thực tế để so khớp bằng test), chỉ kiểm tra: trả đúng loại nội dung, đúng
/// giáo xứ (không bị chéo dữ liệu), và 404 khi không tìm thấy.</summary>
public class GiaoDanInAnTests(QlgxApiFactory app) : IClassFixture<QlgxApiFactory>
{
    private async Task<Guid> TaoGiaoDan(Guid giaoXuId, int ma, string hoTen)
    {
        await using var db = app.TaoContextThuan();
        var gd = new GiaoDan
        {
            GiaoXuId = giaoXuId, MaGiaoDanCu = ma, HoTen = hoTen, TenThanh = "Maria",
            Phai = "Nữ", NgaySinh = new DateOnly(1990, 5, 1), NoiSinh = "Phan Thiết",
            NgayRuaToi = new DateOnly(1990, 6, 1), NoiRuaToi = "GX Vô Nhiễm", SoRuaToi = "12/1990",
        };
        db.GiaoDan.Add(gd);
        await db.SaveChangesAsync();
        return gd.Id;
    }

    [Fact]
    public async Task Xuat_pdf_thanh_cong_cho_giao_dan_ton_tai()
    {
        var id = await TaoGiaoDan(app.GiaoXuId, 9101, "Nguyen Thi In An");

        var res = await app.CreateAuthClient().GetAsync($"/api/giao-dan/{id}/in/ly-lich-ca-nhan");

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        res.Content.Headers.ContentType!.MediaType.Should().Be("application/pdf");
        var bytes = await res.Content.ReadAsByteArrayAsync();
        bytes.Length.Should().BeGreaterThan(500);
        // Chữ ký tệp PDF chuẩn ("%PDF-") — đủ để khẳng định Playwright thật sự xuất ra PDF hợp
        // lệ, không phải trả nhầm HTML thô.
        Encoding.ASCII.GetString(bytes, 0, 5).Should().Be("%PDF-");
    }

    [Fact]
    public async Task Tra_404_khi_khong_tim_thay_giao_dan()
    {
        var res = await app.CreateAuthClient().GetAsync($"/api/giao-dan/{Guid.NewGuid()}/in/ly-lich-ca-nhan");

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    /// <summary>Ảnh đại diện (Task 1.2 VIEC-TIEP-THEO.md) phải vào PDF khi giáo dân có ảnh —
    /// so PDF có ảnh với PDF không ảnh, PDF có ảnh phải NẶNG HƠN đáng kể (ảnh nhúng base64
    /// trong <c>&lt;img&gt;</c> — xem InAnService.KhoiAnhDaiDien) vì không thực tế để so khớp
    /// nội dung PDF nhị phân theo pixel trong bài test.</summary>
    [Fact]
    public async Task Anh_dai_dien_duoc_nhung_vao_pdf_ly_lich_ca_nhan()
    {
        var idKhongAnh = await TaoGiaoDan(app.GiaoXuId, 9110, "Nguoi Khong Co Anh In");
        var idCoAnh = await TaoGiaoDan(app.GiaoXuId, 9111, "Nguoi Co Anh In");
        await using (var db = app.TaoContextThuan())
        {
            var g = db.GiaoDan.Single(x => x.Id == idCoAnh);
            // Ảnh JPEG thật (không phải chuỗi giả) với nhiễu ngẫu nhiên từng điểm ảnh — cố ý
            // KHÔNG dùng một màu đặc (nén JPEG một màu đặc chỉ còn vài trăm byte, không đủ tạo
            // khác biệt kích thước PDF rõ ràng so với biến động font/metadata bình thường giữa
            // hai tệp PDF). 120×160 đã đủ để chênh lệch vượt xa nhiễu đó.
            using var bmp = new SkiaSharp.SKBitmap(120, 160);
            var ngauNhien = new Random(42);
            for (var x = 0; x < bmp.Width; x++)
                for (var y = 0; y < bmp.Height; y++)
                    bmp.SetPixel(x, y, new SkiaSharp.SKColor(
                        (byte)ngauNhien.Next(256), (byte)ngauNhien.Next(256), (byte)ngauNhien.Next(256)));
            using var anh = SkiaSharp.SKImage.FromBitmap(bmp);
            using var duLieu = anh.Encode(SkiaSharp.SKEncodedImageFormat.Jpeg, 90);
            g.AnhDaiDienDuLieu = duLieu.ToArray();
            g.AnhDaiDienLoaiNoiDung = "image/jpeg";
            await db.SaveChangesAsync();
        }

        var client = app.CreateAuthClient();
        var resKhongAnh = await client.GetAsync($"/api/giao-dan/{idKhongAnh}/in/ly-lich-ca-nhan");
        var resCoAnh = await client.GetAsync($"/api/giao-dan/{idCoAnh}/in/ly-lich-ca-nhan");

        resKhongAnh.StatusCode.Should().Be(HttpStatusCode.OK);
        resCoAnh.StatusCode.Should().Be(HttpStatusCode.OK);
        var bytesKhongAnh = await resKhongAnh.Content.ReadAsByteArrayAsync();
        var bytesCoAnh = await resCoAnh.Content.ReadAsByteArrayAsync();
        bytesCoAnh.Length.Should().BeGreaterThan(bytesKhongAnh.Length + 500);
    }

    /// <summary>Vá lỗ hổng so với bản desktop (xem so-sanh-mau-in-desktop.md): Website giáo xứ,
    /// giáo họ cha (giáo họ lẻ trực thuộc giáo họ chính, GiaoHo.GiaoHoChaId — KHÔNG có navigation
    /// property CLR, xem GiaoHoConfig), linh mục chánh xứ đương nhiệm (bảng LinhMuc, ChucVu +
    /// DenNgay), và ghi chú hôn phối đều là truy vấn MỚI thêm vào DungHtmlLyLichCaNhan — bài test
    /// này không so khớp nội dung PDF (nhị phân, xem class doc) mà xác nhận các truy vấn EF mới
    /// không ném lỗi lúc chạy thật (rủi ro thật: sai kiểu Include/so sánh enum, lệch tên cột).</summary>
    [Fact]
    public async Task In_ly_lich_ca_nhan_khong_loi_khi_co_giao_ho_cha_chanh_xu_website_va_ghi_chu_hon_phoi()
    {
        await using (var db = app.TaoContextThuan())
        {
            var gx = await db.GiaoXu.SingleAsync(x => x.Id == app.GiaoXuId);
            gx.Website = "https://gxvonhiem.example";
            db.LinhMuc.Add(new LinhMuc
            {
                GiaoXuId = app.GiaoXuId, MaLinhMucCu = 9001, TenThanh = "Gioan", HoTen = "Nguyen Van Chanh",
                ChucVu = "Chánh xứ", TuNgay = new DateOnly(2020, 1, 1), DenNgay = null,
            });
            await db.SaveChangesAsync();
        }

        Guid giaoHoConId;
        await using (var db = app.TaoContextThuan())
        {
            var cha = new GiaoHo { GiaoXuId = app.GiaoXuId, MaGiaoHoCu = 9501, TenGiaoHo = "Giáo họ Chính" };
            db.GiaoHo.Add(cha);
            await db.SaveChangesAsync();
            var con = new GiaoHo
            {
                GiaoXuId = app.GiaoXuId, MaGiaoHoCu = 9502, TenGiaoHo = "Giáo họ Lẻ", GiaoHoChaId = cha.Id,
            };
            db.GiaoHo.Add(con);
            await db.SaveChangesAsync();
            giaoHoConId = con.Id;
        }

        var id = await TaoGiaoDan(app.GiaoXuId, 9120, "Nguoi Co Giao Ho Le");
        var chongId = await TaoGiaoDan(app.GiaoXuId, 9121, "Vo Chong Cua Nguoi In");
        await using (var db = app.TaoContextThuan())
        {
            var g = db.GiaoDan.Single(x => x.Id == id);
            g.GiaoHoId = giaoHoConId;
            var hp = new HonPhoi
            {
                GiaoXuId = app.GiaoXuId, MaHonPhoiCu = 9001, SoHonPhoi = "HP-9001",
                GhiChu = "Ghi chú thử nghiệm hôn phối",
            };
            db.HonPhoi.Add(hp);
            db.GiaoDanHonPhoi.AddRange(
                new GiaoDanHonPhoi { GiaoXuId = app.GiaoXuId, HonPhoi = hp, GiaoDanId = id, SoThuTu = 1 },
                new GiaoDanHonPhoi { GiaoXuId = app.GiaoXuId, HonPhoi = hp, GiaoDanId = chongId, SoThuTu = 2 });
            await db.SaveChangesAsync();
        }

        var res = await app.CreateAuthClient().GetAsync($"/api/giao-dan/{id}/in/ly-lich-ca-nhan");

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        var bytes = await res.Content.ReadAsByteArrayAsync();
        Encoding.ASCII.GetString(bytes, 0, 5).Should().Be("%PDF-");
    }

    [Fact]
    public async Task Khong_in_duoc_giao_dan_cua_giao_xu_khac()
    {
        var giaoXuKhac = Guid.NewGuid();
        await using (var db = app.TaoContextThuan())
        {
            db.GiaoXu.Add(new GiaoXu { Id = giaoXuKhac, TenGiaoXu = "Giao xu khac", MaGiaoXuCu = 999 });
            await db.SaveChangesAsync();
        }
        var idNguoiXuKhac = await TaoGiaoDan(giaoXuKhac, 9102, "Nguoi Xu Khac");

        // Token phát hành cho app.GiaoXuId (giáo xứ mặc định) — không phải giáo xứ vừa tạo.
        var res = await app.CreateAuthClient().GetAsync($"/api/giao-dan/{idNguoiXuKhac}/in/ly-lich-ca-nhan");

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
