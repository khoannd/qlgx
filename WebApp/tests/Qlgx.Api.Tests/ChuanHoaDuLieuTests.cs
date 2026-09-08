using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Qlgx.Api.Dtos;
using Qlgx.Domain.Entities;

namespace Qlgx.Api.Tests;

/// <summary>
/// "Chuẩn hoá dữ liệu" (docs/superpowers/specs/man-hinh/cong-cu-du-lieu.md mục 5.1) — công cụ
/// sửa dữ liệu hàng loạt. Test đúng 4 nguyên tắc: xem trước có số liệu thật, ghi thật đúng
/// thuật toán viết-hoa-chữ-cái-đầu, không đụng cột mới của web, và MỘT transaction.
/// </summary>
public class ChuanHoaDuLieuTests(QlgxApiFactory app) : IClassFixture<QlgxApiFactory>
{
    [Fact]
    public async Task Xem_truoc_giao_dan_dem_dung_so_ban_ghi_se_doi_khong_ghi_gi()
    {
        await using var db = app.TaoContextThuan();
        var dungRoi = new GiaoDan { GiaoXuId = app.GiaoXuId, MaGiaoDanCu = 94001, HoTen = "Nguyễn Văn An" };
        var saiThuongHoa = new GiaoDan { GiaoXuId = app.GiaoXuId, MaGiaoDanCu = 94002, HoTen = "nguyễn VĂN an" };
        db.GiaoDan.AddRange(dungRoi, saiThuongHoa);
        await db.SaveChangesAsync();

        var client = app.CreateAuthClient();
        var res = await client.PostAsync("/api/cong-cu-du-lieu/chuan-hoa/giao-dan/xem-truoc", null);
        res.EnsureSuccessStatusCode();
        var kq = await res.Content.ReadFromJsonAsync<ChuanHoaXemTruocKetQua>();

        kq!.SoBanGhiSeDoi.Should().Be(1);
        var mau = kq.MauThayDoi.Should().ContainSingle().Subject;
        mau.Truong.Should().ContainSingle(t => t.TenTruong == "HoTen" && t.GiaTriMoi == "Nguyễn Văn An");

        // Xem truoc khong duoc ghi gi xuong CSDL
        await using var kiemTra = app.TaoContextThuan();
        (await kiemTra.GiaoDan.SingleAsync(g => g.Id == saiThuongHoa.Id)).HoTen.Should().Be("nguyễn VĂN an");
    }

    [Fact]
    public async Task Ghi_that_doi_dung_ho_ten_va_tra_dung_so_ban_ghi_da_doi()
    {
        await using var db = app.TaoContextThuan();
        var gd = new GiaoDan { GiaoXuId = app.GiaoXuId, MaGiaoDanCu = 94010, HoTen = "trần   thị   bình" };
        db.GiaoDan.Add(gd);
        await db.SaveChangesAsync();

        var client = app.CreateAuthClient();
        var res = await client.PostAsync("/api/cong-cu-du-lieu/chuan-hoa/giao-dan", null);
        res.EnsureSuccessStatusCode();
        var kq = await res.Content.ReadFromJsonAsync<ChuanHoaKetQua>();
        kq!.SoBanGhiDaDoi.Should().Be(1);

        await using var kiemTra = app.TaoContextThuan();
        // Khoang trang thua giua cac tu bi gop lai dung 1 dau cach (dung ham goc cua desktop).
        (await kiemTra.GiaoDan.SingleAsync(g => g.Id == gd.Id)).HoTen.Should().Be("Trần Thị Bình");
    }

    [Fact]
    public async Task Ghi_chu_khong_bi_dam_vao_nhung_ghi_chu_xuc_dau_thi_co_dung_bug_ban_goc()
    {
        await using var db = app.TaoContextThuan();
        var gd = new GiaoDan
        {
            GiaoXuId = app.GiaoXuId, MaGiaoDanCu = 94020, HoTen = "Le Van C",
            GhiChu = "ghi chú kiểu thường không được đổi",
            GhiChuXucDau = "ghi chú xức dầu kiểu thường",
        };
        db.GiaoDan.Add(gd);
        await db.SaveChangesAsync();

        var client = app.CreateAuthClient();
        (await client.PostAsync("/api/cong-cu-du-lieu/chuan-hoa/giao-dan", null)).EnsureSuccessStatusCode();

        await using var kiemTra = app.TaoContextThuan();
        var sau = await kiemTra.GiaoDan.SingleAsync(g => g.Id == gd.Id);
        sau.GhiChu.Should().Be("ghi chú kiểu thường không được đổi", "GhiChu bi loai tru dung ten (khop chinh xac ban goc)");
        sau.GhiChuXucDau.Should().Be("Ghi Chú Xức Dầu Kiểu Thường",
            "GhiChuXucDau KHONG duoc loai tru o ban goc (chi \"GhiChu\" moi khop chinh xac) - tai hien dung bug nay");
    }

    [Fact]
    public async Task Khong_dung_tay_vao_cot_moi_chi_co_o_web()
    {
        await using var db = app.TaoContextThuan();
        var gd = new GiaoDan
        {
            GiaoXuId = app.GiaoXuId, MaGiaoDanCu = 94030, HoTen = "Test",
            AnhDaiDienLoaiNoiDung = "image/jpeg",
        };
        db.GiaoDan.Add(gd);
        await db.SaveChangesAsync();

        var client = app.CreateAuthClient();
        (await client.PostAsync("/api/cong-cu-du-lieu/chuan-hoa/giao-dan", null)).EnsureSuccessStatusCode();

        await using var kiemTra = app.TaoContextThuan();
        (await kiemTra.GiaoDan.SingleAsync(g => g.Id == gd.Id)).AnhDaiDienLoaiNoiDung.Should().Be("image/jpeg");
    }

    [Fact]
    public async Task Tu_bat_dau_bang_ky_tu_dac_biet_duoc_giu_nguyen()
    {
        await using var db = app.TaoContextThuan();
        var gd = new GiaoDan { GiaoXuId = app.GiaoXuId, MaGiaoDanCu = 94040, HoTen = "nguyễn (chị) lan" };
        db.GiaoDan.Add(gd);
        await db.SaveChangesAsync();

        var client = app.CreateAuthClient();
        (await client.PostAsync("/api/cong-cu-du-lieu/chuan-hoa/giao-dan", null)).EnsureSuccessStatusCode();

        await using var kiemTra = app.TaoContextThuan();
        (await kiemTra.GiaoDan.SingleAsync(g => g.Id == gd.Id)).HoTen.Should().Be("Nguyễn (chị) Lan");
    }

    [Fact]
    public async Task Gia_dinh_xem_truoc_va_ghi_that_doi_dung_ten_gia_dinh()
    {
        await using var db = app.TaoContextThuan();
        var gdinh = new GiaDinh { GiaoXuId = app.GiaoXuId, MaGiaDinhCu = 94050, TenGiaDinh = "gia đình VĂN a" };
        db.GiaDinh.Add(gdinh);
        await db.SaveChangesAsync();

        var client = app.CreateAuthClient();
        var xemTruoc = await client.PostAsync("/api/cong-cu-du-lieu/chuan-hoa/gia-dinh/xem-truoc", null);
        var kqXem = await xemTruoc.Content.ReadFromJsonAsync<ChuanHoaXemTruocKetQua>();
        kqXem!.SoBanGhiSeDoi.Should().Be(1);

        var ghiThat = await client.PostAsync("/api/cong-cu-du-lieu/chuan-hoa/gia-dinh", null);
        var kqGhi = await ghiThat.Content.ReadFromJsonAsync<ChuanHoaKetQua>();
        kqGhi!.SoBanGhiDaDoi.Should().Be(1);

        await using var kiemTra = app.TaoContextThuan();
        (await kiemTra.GiaDinh.SingleAsync(g => g.Id == gdinh.Id)).TenGiaDinh.Should().Be("Gia Đình Văn A");
    }

    [Fact]
    public async Task Nguoi_chua_dang_nhap_bi_chan_401()
    {
        var res = await app.CreateClient().PostAsync("/api/cong-cu-du-lieu/chuan-hoa/giao-dan/xem-truoc", null);

        res.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }
}
