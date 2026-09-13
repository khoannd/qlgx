using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Qlgx.Api.Dtos;
using Qlgx.Api.Printing;
using Qlgx.Domain;
using Qlgx.Domain.Entities;

namespace Qlgx.Api.Tests;

/// <summary>
/// "In lý lịch cá nhân" bấm từ màn hình GIA ĐÌNH in cho MỌI thành viên, mỗi người một trang, gộp
/// vào một tệp PDF (xem InAnService.XuatLyLichCaNhanGiaDinh). Việc gộp phải tách được phần thân
/// của từng trang ra khỏi tài liệu HTML đã dựng — và đó là chỗ từng có lỗi thật:
///
/// Mẫu in tuỳ chỉnh do giáo xứ lưu KHÔNG còn vỏ <c>&lt;html&gt;/&lt;body&gt;</c> (HtmlSanitizer
/// trả về fragment — chỉ phần bên trong body), trong khi hàm tách lại tìm đúng
/// <c>&lt;body&gt;...&lt;/body&gt;</c>. Hệ quả với người dùng thật: giáo xứ nào đã tuỳ chỉnh mẫu
/// "Lý lịch cá nhân" thì bấm in cả gia đình ra tờ giấy TRẮNG (chỉ còn CSS, mất sạch nội dung),
/// trong khi in cho từng người vẫn đúng nên rất dễ tưởng là máy in hỏng.
/// </summary>
public class InAnGhepNhieuTrangTests(QlgxApiFactory app) : IClassFixture<QlgxApiFactory>
{
    /// <summary>Chặn đúng chỗ hẹp nhất để đọc chuỗi HTML cuối cùng trước khi vẽ PDF — PDF là nhị
    /// phân (đã nén) nên không kiểm được nội dung bằng cách tìm chuỗi. Cùng kỹ thuật với
    /// MauInDayDuBienTests (xem ghi chú lý do ở đầu BoTrinhDuyet.cs).</summary>
    private sealed class BoTrinhDuyetGhiHtml : BoTrinhDuyet
    {
        public List<string> DaVe { get; } = [];

        public override Task<byte[]> XuatPdfAsync(
            string html, CancellationToken ct, bool landscape = false, string khoGiay = "A4")
        {
            lock (DaVe) DaVe.Add(html);
            return Task.FromResult(System.Text.Encoding.ASCII.GetBytes("%PDF-gia-lap"));
        }
    }

    private static int _daiMa;

    private async Task<Guid> TaoGiaDinhHaiNguoi()
    {
        var goc = 6100 + Interlocked.Increment(ref _daiMa) * 10;
        await using var db = app.TaoContextThuan();

        var chong = new GiaoDan
        {
            GiaoXuId = app.GiaoXuId, MaGiaoDanCu = goc + 1, HoTen = "Nguyen Van Chong Ghep",
            TenThanh = "Giuse", Phai = "Nam",
        };
        var vo = new GiaoDan
        {
            GiaoXuId = app.GiaoXuId, MaGiaoDanCu = goc + 2, HoTen = "Tran Thi Vo Ghep",
            TenThanh = "Maria", Phai = "Nữ",
        };
        db.GiaoDan.AddRange(chong, vo);

        var giaDinh = new GiaDinh
        {
            GiaoXuId = app.GiaoXuId, MaGiaDinhCu = goc + 3, TenGiaDinh = "Gia đình Ghép Trang",
        };
        db.GiaDinh.Add(giaDinh);
        db.ThanhVienGiaDinh.AddRange(
            new ThanhVienGiaDinh
            {
                GiaoXuId = app.GiaoXuId, GiaDinh = giaDinh, GiaoDan = chong,
                VaiTro = VaiTroGiaDinh.Chong, ChuHo = true,
            },
            new ThanhVienGiaDinh
            {
                GiaoXuId = app.GiaoXuId, GiaDinh = giaDinh, GiaoDan = vo, VaiTro = VaiTroGiaDinh.Vo,
            });

        await db.SaveChangesAsync();
        return giaDinh.Id;
    }

    private (WebApplicationFactory<Program> Factory, HttpClient Client, BoTrinhDuyetGhiHtml Html) MayChuThu()
    {
        var ghi = new BoTrinhDuyetGhiHtml();
        var factory = app.WithWebHostBuilder(b => b.ConfigureTestServices(s =>
        {
            s.RemoveAll<BoTrinhDuyet>();
            s.AddSingleton<BoTrinhDuyet>(ghi);
        }));
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", app.PhatHanhToken());
        return (factory, client, ghi);
    }

    [Fact]
    public async Task In_ca_gia_dinh_van_ra_noi_dung_khi_giao_xu_da_tuy_chinh_mau()
    {
        const string dauHieu = "DAU-HIEU-MAU-TUY-CHINH-GHEP-TRANG";
        var giaDinhId = await TaoGiaDinhHaiNguoi();
        var (factory, client, ghi) = MayChuThu();
        using var _ = factory;

        // Giáo xứ lưu mẫu riêng qua ĐÚNG endpoint người dùng thật dùng — đi qua cả bước khử
        // trùng HTML, chính bước bóc mất vỏ <html>/<body> gây ra lỗi trang trắng.
        var luu = await client.PutAsJsonAsync("/api/mau-in/LyLichCaNhan/rieng",
            new LuuMauInRequest(
                $"<html><body><p>{dauHieu}</p><p>Ho ten: {{{{HoTen}}}}</p></body></html>", 0));
        luu.StatusCode.Should().Be(HttpStatusCode.OK);

        try
        {
            var res = await client.GetAsync($"/api/gia-dinh/{giaDinhId}/in/ly-lich-ca-nhan");
            res.StatusCode.Should().Be(HttpStatusCode.OK);

            var htmlGop = ghi.DaVe[^1];
            htmlGop.Should().Contain(dauHieu,
                "bản in gộp cả gia đình phải giữ nội dung mẫu tuỳ chỉnh — mất nội dung nghĩa là quý cha " +
                "bấm in ra tờ giấy trắng");
            System.Text.RegularExpressions.Regex.Matches(htmlGop, dauHieu).Count.Should().Be(2,
                "gia đình có 2 thành viên thì phải có đúng 2 trang, mỗi trang một bản lý lịch");
            htmlGop.Should().Contain("Nguyen Van Chong Ghep").And.Contain("Tran Thi Vo Ghep",
                "mỗi trang phải điền dữ liệu của đúng thành viên tương ứng");
        }
        finally
        {
            // Dọn trong finally: nếu phần khẳng định phía trên đỏ mà mẫu tuỳ chỉnh còn sót lại,
            // nó sẽ kéo theo mọi bài test khác dùng chung giáo xứ mặc định cùng đỏ và che mất
            // nguyên nhân thật.
            (await client.DeleteAsync("/api/mau-in/LyLichCaNhan/rieng")).EnsureSuccessStatusCode();
        }
    }

    /// <summary>Đường đi CŨ (chưa ai tuỳ chỉnh mẫu, dùng mẫu gốc nhúng cứng có đủ vỏ
    /// <c>&lt;html&gt;/&lt;body&gt;</c>) phải giữ nguyên hành vi — bản vá cho trường hợp fragment
    /// không được phép làm hỏng trường hợp đang chạy tốt.</summary>
    [Fact]
    public async Task In_ca_gia_dinh_voi_mau_goc_van_ghep_dung_so_trang()
    {
        var giaDinhId = await TaoGiaDinhHaiNguoi();
        var (factory, client, ghi) = MayChuThu();
        using var _ = factory;

        var res = await client.GetAsync($"/api/gia-dinh/{giaDinhId}/in/ly-lich-ca-nhan");
        res.StatusCode.Should().Be(HttpStatusCode.OK);

        var htmlGop = ghi.DaVe[^1];
        System.Text.RegularExpressions.Regex.Matches(htmlGop, "LÝ LỊCH CÁ NHÂN").Count.Should().Be(2,
            "hai thành viên thì đúng hai trang lý lịch");
        htmlGop.Should().Contain("Nguyen Van Chong Ghep").And.Contain("Tran Thi Vo Ghep");
        htmlGop.Should().Contain("page-break-after", "các trang phải được ngăn cách bằng ngắt trang");
    }
}
