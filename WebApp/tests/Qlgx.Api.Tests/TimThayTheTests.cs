using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Qlgx.Api.Dtos;
using Qlgx.Domain.Entities;

namespace Qlgx.Api.Tests;

/// <summary>
/// "Tìm và thay thế" (docs/superpowers/specs/man-hinh/tim-thay-the.md) — công cụ sửa dữ liệu
/// hàng loạt thay thế CHÍNH XÁC (không phải LIKE một phần) giá trị của một cột. Test đúng 4
/// nguyên tắc: xem trước đếm đúng số khớp, ghi thật chỉ đổi đúng bản ghi khớp CHÍNH XÁC (không
/// đổi bản ghi gần giống), và không cho chọn trường ngoài danh sách cho phép.
/// </summary>
public class TimThayTheTests(QlgxApiFactory app) : IClassFixture<QlgxApiFactory>
{
    [Fact]
    public async Task Xem_truoc_dem_dung_so_ban_ghi_khop_chinh_xac_khong_ghi_gi()
    {
        await using var db = app.TaoContextThuan();
        var khop1 = new GiaoDan { GiaoXuId = app.GiaoXuId, MaGiaoDanCu = 96001, HoTen = "A", NoiSinh = "Sài Gòn" };
        var khop2 = new GiaoDan { GiaoXuId = app.GiaoXuId, MaGiaoDanCu = 96002, HoTen = "B", NoiSinh = "Sài Gòn" };
        var khongKhop = new GiaoDan { GiaoXuId = app.GiaoXuId, MaGiaoDanCu = 96003, HoTen = "C", NoiSinh = "Sài Gòn Q1" };
        db.GiaoDan.AddRange(khop1, khop2, khongKhop);
        await db.SaveChangesAsync();

        var client = app.CreateAuthClient();
        var res = await client.PostAsJsonAsync("/api/cong-cu-du-lieu/tim-thay-the/xem-truoc",
            new TimThayTheRequest(BangTimThayThe.GiaoDan, "NoiSinh", "Sài Gòn", "TP. Hồ Chí Minh"));
        res.EnsureSuccessStatusCode();
        var kq = await res.Content.ReadFromJsonAsync<TimThayTheXemTruocKetQua>();

        kq!.SoBanGhiKhop.Should().Be(2, "chi 'Sai Gon' khop chinh xac, 'Sai Gon Q1' khong khop");

        await using var kiemTra = app.TaoContextThuan();
        (await kiemTra.GiaoDan.SingleAsync(g => g.Id == khop1.Id)).NoiSinh.Should().Be("Sài Gòn", "xem truoc khong duoc ghi gi");
    }

    [Fact]
    public async Task Ghi_that_chi_doi_ban_ghi_khop_chinh_xac_khong_dam_vao_ban_ghi_gan_giong()
    {
        await using var db = app.TaoContextThuan();
        var khop = new GiaoDan { GiaoXuId = app.GiaoXuId, MaGiaoDanCu = 96010, HoTen = "A", NoiSinh = "Sài Gòn" };
        var khongKhop = new GiaoDan { GiaoXuId = app.GiaoXuId, MaGiaoDanCu = 96011, HoTen = "B", NoiSinh = "Sài Gòn Q1" };
        db.GiaoDan.AddRange(khop, khongKhop);
        await db.SaveChangesAsync();

        var client = app.CreateAuthClient();
        var res = await client.PostAsJsonAsync("/api/cong-cu-du-lieu/tim-thay-the",
            new TimThayTheRequest(BangTimThayThe.GiaoDan, "NoiSinh", "Sài Gòn", "TP. Hồ Chí Minh"));
        res.EnsureSuccessStatusCode();
        var kq = await res.Content.ReadFromJsonAsync<TimThayTheKetQua>();
        kq!.SoBanGhiDaThay.Should().Be(1);

        await using var kiemTra = app.TaoContextThuan();
        (await kiemTra.GiaoDan.SingleAsync(g => g.Id == khop.Id)).NoiSinh.Should().Be("TP. Hồ Chí Minh");
        (await kiemTra.GiaoDan.SingleAsync(g => g.Id == khongKhop.Id)).NoiSinh.Should().Be("Sài Gòn Q1", "khong duoc dam vao ban ghi gan giong");
    }

    [Fact]
    public async Task Chi_dung_dung_cot_duoc_chi_dinh_khong_dung_cot_khac()
    {
        await using var db = app.TaoContextThuan();
        var gd = new GiaoDan { GiaoXuId = app.GiaoXuId, MaGiaoDanCu = 96020, HoTen = "X", DiaChi = "X", NoiSinh = "X" };
        db.GiaoDan.Add(gd);
        await db.SaveChangesAsync();

        var client = app.CreateAuthClient();
        (await client.PostAsJsonAsync("/api/cong-cu-du-lieu/tim-thay-the",
            new TimThayTheRequest(BangTimThayThe.GiaoDan, "DiaChi", "X", "Y"))).EnsureSuccessStatusCode();

        await using var kiemTra = app.TaoContextThuan();
        var sau = await kiemTra.GiaoDan.SingleAsync(g => g.Id == gd.Id);
        sau.DiaChi.Should().Be("Y");
        sau.NoiSinh.Should().Be("X", "chi doi dung cot DiaChi, khong dam vao cot khac");
    }

    [Fact]
    public async Task Thay_the_tren_gia_dinh_hoat_dong_dung()
    {
        await using var db = app.TaoContextThuan();
        var gdinh = new GiaDinh { GiaoXuId = app.GiaoXuId, MaGiaDinhCu = 96030, TenGiaDinh = "Ho Nguyen" };
        db.GiaDinh.Add(gdinh);
        await db.SaveChangesAsync();

        var client = app.CreateAuthClient();
        var res = await client.PostAsJsonAsync("/api/cong-cu-du-lieu/tim-thay-the",
            new TimThayTheRequest(BangTimThayThe.GiaDinh, "TenGiaDinh", "Ho Nguyen", "Ho Nguyen Van"));
        var kq = await res.Content.ReadFromJsonAsync<TimThayTheKetQua>();
        kq!.SoBanGhiDaThay.Should().Be(1);

        await using var kiemTra = app.TaoContextThuan();
        (await kiemTra.GiaDinh.SingleAsync(g => g.Id == gdinh.Id)).TenGiaDinh.Should().Be("Ho Nguyen Van");
    }

    [Fact]
    public async Task Gia_tri_tim_rong_bi_tu_choi_nguyen_van_thong_bao_desktop()
    {
        var client = app.CreateAuthClient();
        var res = await client.PostAsJsonAsync("/api/cong-cu-du-lieu/tim-thay-the/xem-truoc",
            new TimThayTheRequest(BangTimThayThe.GiaoDan, "HoTen", "", "X"));

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var noiDung = await res.Content.ReadAsStringAsync();
        noiDung.Should().Contain("Hãy nhập giá trị cần tìm");
    }

    [Fact]
    public async Task Truong_khong_thuoc_danh_sach_cho_phep_bi_tu_choi()
    {
        var client = app.CreateAuthClient();
        var res = await client.PostAsJsonAsync("/api/cong-cu-du-lieu/tim-thay-the/xem-truoc",
            new TimThayTheRequest(BangTimThayThe.GiaoDan, "MaNhanDang", "x", "y"));

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Truong_cua_bang_khac_khong_ap_dung_duoc_cheo_bang()
    {
        var client = app.CreateAuthClient();
        // "TenGiaDinh" chi hop le cho GiaDinh, khong hop le cho GiaoDan.
        var res = await client.PostAsJsonAsync("/api/cong-cu-du-lieu/tim-thay-the/xem-truoc",
            new TimThayTheRequest(BangTimThayThe.GiaoDan, "TenGiaDinh", "x", "y"));

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Nguoi_chua_dang_nhap_bi_chan_401()
    {
        var res = await app.CreateClient().PostAsJsonAsync("/api/cong-cu-du-lieu/tim-thay-the/xem-truoc",
            new TimThayTheRequest(BangTimThayThe.GiaoDan, "HoTen", "x", "y"));

        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
