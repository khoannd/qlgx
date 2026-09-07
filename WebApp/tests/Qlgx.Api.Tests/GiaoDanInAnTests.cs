using System.Net;
using System.Text;
using FluentAssertions;
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
