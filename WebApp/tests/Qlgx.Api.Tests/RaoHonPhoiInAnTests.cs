using System.Net;
using System.Text;
using FluentAssertions;
using Qlgx.Domain.Entities;

namespace Qlgx.Api.Tests;

/// <summary>
/// "In giới thiệu hôn phối" (GxGiaoDanList.tsx, tương đương Source/ExcelReport/ReportRaoHP.cs —
/// xem docs/superpowers/specs/man-hinh/in-an.md mục 8) và "In kết quả rao hôn phối"
/// (RaoHonPhoiDetail.tsx). Cùng cách kiểm thử với InAnMauMoiTests.cs: kiểm chữ ký tệp "%PDF-",
/// mã trạng thái, và cách ly giáo xứ — KHÔNG so khớp nội dung PDF nhị phân.
/// </summary>
public class RaoHonPhoiInAnTests(QlgxApiFactory app) : IClassFixture<QlgxApiFactory>
{
    private async Task VerifyPdf(HttpResponseMessage res)
    {
        res.StatusCode.Should().Be(HttpStatusCode.OK);
        res.Content.Headers.ContentType!.MediaType.Should().Be("application/pdf");
        var bytes = await res.Content.ReadAsByteArrayAsync();
        bytes.Length.Should().BeGreaterThan(500);
        Encoding.ASCII.GetString(bytes, 0, 5).Should().Be("%PDF-");
    }

    private async Task<(Guid GiaoDan1Id, Guid GiaoDan2Id, Guid RaoHonPhoiId)> TaoDoiRao(
        int maGd1, int maGd2, int maRao, Action<RaoHonPhoi>? tuyChinh = null)
    {
        await using var db = app.TaoContextThuan();
        var g1 = new GiaoDan { GiaoXuId = app.GiaoXuId, MaGiaoDanCu = maGd1, HoTen = "Nguoi Rao " + maGd1, Phai = "Nam" };
        var g2 = new GiaoDan { GiaoXuId = app.GiaoXuId, MaGiaoDanCu = maGd2, HoTen = "Nguoi Rao " + maGd2, Phai = "Nữ" };
        db.AddRange(g1, g2);
        var r = new RaoHonPhoi
        {
            GiaoXuId = app.GiaoXuId, MaRaoHonPhoiCu = maRao, TenRaoHonPhoi = "Doi rao " + maRao,
            GiaoDan1 = g1, GiaoDan2 = g2,
        };
        tuyChinh?.Invoke(r);
        db.RaoHonPhoi.Add(r);
        await db.SaveChangesAsync();
        return (g1.Id, g2.Id, r.Id);
    }

    [Fact]
    public async Task Gioi_thieu_hon_phoi_xuat_thanh_cong_khi_giao_dan_la_ben_thu_nhat()
    {
        var (gd1, _, _) = await TaoDoiRao(9001, 9002, 81, r =>
        {
            r.LinhMucNhan = "Lm. Nhan A"; r.GiaoXuNhan = "Giao Phan Nhan A";
            r.GiaoXuNQ1 = "Xu NQ 1"; r.GiaoPhanNQ1 = "Phan NQ 1";
        });

        var res = await app.CreateAuthClient().GetAsync($"/api/giao-dan/{gd1}/in/gioi-thieu-hon-phoi");

        await VerifyPdf(res);
    }

    [Fact]
    public async Task Gioi_thieu_hon_phoi_xuat_thanh_cong_khi_giao_dan_la_ben_thu_hai()
    {
        var (_, gd2, _) = await TaoDoiRao(9003, 9004, 82);

        var res = await app.CreateAuthClient().GetAsync($"/api/giao-dan/{gd2}/in/gioi-thieu-hon-phoi");

        await VerifyPdf(res);
    }

    [Fact]
    public async Task Gioi_thieu_hon_phoi_tra_404_khi_giao_dan_chua_co_doi_rao_nao()
    {
        await using var db = app.TaoContextThuan();
        var g = new GiaoDan { GiaoXuId = app.GiaoXuId, MaGiaoDanCu = 9005, HoTen = "Chua Co Rao" };
        db.GiaoDan.Add(g);
        await db.SaveChangesAsync();

        var res = await app.CreateAuthClient().GetAsync($"/api/giao-dan/{g.Id}/in/gioi-thieu-hon-phoi");

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Gioi_thieu_hon_phoi_khong_tim_thay_giao_dan_cua_giao_xu_khac()
    {
        var giaoXuKhac = Guid.NewGuid();
        Guid gd1Id;
        await using (var db = app.TaoContextThuan())
        {
            db.GiaoXu.Add(new GiaoXu { Id = giaoXuKhac, TenGiaoXu = "Giao xu khac Rao", MaGiaoXuCu = 995 });
            var g1 = new GiaoDan { GiaoXuId = giaoXuKhac, MaGiaoDanCu = 9006, HoTen = "Nguoi Xu Khac 1" };
            var g2 = new GiaoDan { GiaoXuId = giaoXuKhac, MaGiaoDanCu = 9007, HoTen = "Nguoi Xu Khac 2" };
            db.AddRange(g1, g2);
            db.RaoHonPhoi.Add(new RaoHonPhoi
            {
                GiaoXuId = giaoXuKhac, MaRaoHonPhoiCu = 83, TenRaoHonPhoi = "Doi Xu Khac",
                GiaoDan1 = g1, GiaoDan2 = g2,
            });
            await db.SaveChangesAsync();
            gd1Id = g1.Id;
        }

        var res = await app.CreateAuthClient().GetAsync($"/api/giao-dan/{gd1Id}/in/gioi-thieu-hon-phoi");

        // Route ánh xạ theo id giáo dân — bộ lọc GiaoXuId toàn cục của QlgxDbContext khiến
        // LayGiaoDanChoGioiThieu-kiểu truy vấn không thấy giáo dân này (thuộc giáo xứ khác của
        // client hiện tại) NÊN sẽ không tìm được đôi rao nào khớp GiaoDanId đó (chưa từng có id
        // trong dữ liệu của giáo xứ client) — cùng kết quả với "chưa có đôi rao": 404. Đây là
        // test cách ly gián tiếp qua chính bộ lọc, không có gì để lộ ra ngoài.
        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Ket_qua_rao_hon_phoi_xuat_thanh_cong()
    {
        var (_, _, raoId) = await TaoDoiRao(9010, 9011, 84, r =>
        {
            r.NgayRaoLan1 = new DateOnly(2024, 5, 1);
            r.NgayRaoLan2 = new DateOnly(2024, 5, 8);
            r.NgayRaoLan3 = new DateOnly(2024, 5, 15);
        });

        var res = await app.CreateAuthClient().GetAsync($"/api/rao-hon-phoi/{raoId}/in/ket-qua");

        await VerifyPdf(res);
    }

    [Fact]
    public async Task Ket_qua_rao_hon_phoi_tra_404_khi_khong_tim_thay()
    {
        var res = await app.CreateAuthClient().GetAsync($"/api/rao-hon-phoi/{Guid.NewGuid()}/in/ket-qua");

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Ket_qua_rao_hon_phoi_cua_giao_xu_khac_tra_404()
    {
        var giaoXuKhac = Guid.NewGuid();
        Guid raoId;
        await using (var db = app.TaoContextThuan())
        {
            db.GiaoXu.Add(new GiaoXu { Id = giaoXuKhac, TenGiaoXu = "Giao xu khac KQRao", MaGiaoXuCu = 994 });
            var g1 = new GiaoDan { GiaoXuId = giaoXuKhac, MaGiaoDanCu = 9012, HoTen = "KQ Xu Khac 1" };
            var g2 = new GiaoDan { GiaoXuId = giaoXuKhac, MaGiaoDanCu = 9013, HoTen = "KQ Xu Khac 2" };
            db.AddRange(g1, g2);
            var r = new RaoHonPhoi { GiaoXuId = giaoXuKhac, MaRaoHonPhoiCu = 85, TenRaoHonPhoi = "KQ Doi Xu Khac",
                GiaoDan1 = g1, GiaoDan2 = g2 };
            db.RaoHonPhoi.Add(r);
            await db.SaveChangesAsync();
            raoId = r.Id;
        }

        var res = await app.CreateAuthClient().GetAsync($"/api/rao-hon-phoi/{raoId}/in/ket-qua");

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
