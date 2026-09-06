using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Qlgx.Api.Dtos;
using Qlgx.Domain.Entities;

namespace Qlgx.Api.Tests;

/// <summary>
/// Task 14 — kiểm chứng ranh giới bảo mật cốt lõi của mô hình nhiều giáo xứ dùng chung một
/// máy chủ: GiaoXuId của phiên CHỈ lấy từ claim của token, không bao giờ từ tham số trình
/// duyệt, và không endpoint nghiệp vụ nào phục vụ được người chưa đăng nhập.
/// </summary>
public class BaoMatTests(QlgxApiFactory app) : IClassFixture<QlgxApiFactory>
{
    private sealed record GiaoDanItem(Guid Id, string HoTen);

    private async Task<Guid> TaoGiaoXuKhac(string ten)
    {
        await using var db = app.TaoContextThuan();
        var giaoXu = new GiaoXu { TenGiaoXu = ten, MaGiaoXuCu = new Random().Next(90000, 99999) };
        db.GiaoXu.Add(giaoXu);
        await db.SaveChangesAsync();
        return giaoXu.Id;
    }

    private static int _maGiaoDanKeTiep = 70001;

    private async Task<Guid> TaoGiaoDan(Guid giaoXuId, string hoTen)
    {
        await using var db = app.TaoContextThuan();
        var gd = new GiaoDan
        {
            GiaoXuId = giaoXuId, HoTen = hoTen, TenThanh = "Test", Phai = "Nam",
            MaGiaoDanCu = Interlocked.Increment(ref _maGiaoDanKeTiep),
        };
        db.GiaoDan.Add(gd);
        await db.SaveChangesAsync();
        return gd.Id;
    }

    [Fact]
    public async Task Chua_dang_nhap_thi_bi_chan_401_khong_ro_du_lieu()
    {
        var client = app.CreateClient(); // KHÔNG có header Authorization

        var res = await client.GetAsync("/api/giao-dan");

        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData("/api/giao-dan")]
    [InlineData("/api/gia-dinh")]
    [InlineData("/api/giao-ho")]
    [InlineData("/api/hoi-doan")]
    public async Task Moi_endpoint_doc_deu_doi_hoi_xac_thuc(string duong)
    {
        var client = app.CreateClient();

        var res = await client.GetAsync(duong);

        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized, $"endpoint {duong} phai doi hoi dang nhap");
    }

    [Fact]
    public async Task Nguoi_dung_giao_xu_A_khong_thay_du_lieu_giao_xu_B()
    {
        var giaoXuB = await TaoGiaoXuKhac("Giao xu B (bao mat test)");
        await TaoGiaoDan(app.GiaoXuId, "Nguoi cua xu A");
        await TaoGiaoDan(giaoXuB, "Nguoi cua xu B");

        var clientA = app.CreateAuthClient(app.GiaoXuId);
        var ds = await clientA.GetFromJsonAsync<List<GiaoDanItem>>("/api/giao-dan");

        ds!.Should().Contain(x => x.HoTen == "Nguoi cua xu A");
        ds!.Should().NotContain(x => x.HoTen == "Nguoi cua xu B");
    }

    [Fact]
    public async Task Truyen_giaoXuId_cua_xu_khac_qua_query_khong_doi_duoc_ket_qua()
    {
        var giaoXuB = await TaoGiaoXuKhac("Giao xu C (bao mat test)");
        await TaoGiaoDan(app.GiaoXuId, "Nguoi xu A rieng biet");
        await TaoGiaoDan(giaoXuB, "Nguoi xu C rieng biet");

        // Dang nhap bang giao xu A nhung co gang nhet giaoXuId cua xu C qua query string —
        // khong endpoint nao doc tham so nay (da ra soat toan bo Endpoints/*.cs), nen phai
        // hoan toan khong anh huong, GiaoXuId van lay tu claim cua token.
        var clientA = app.CreateAuthClient(app.GiaoXuId);
        var ds = await clientA.GetFromJsonAsync<List<GiaoDanItem>>($"/api/giao-dan?giaoXuId={giaoXuB}");

        ds!.Should().Contain(x => x.HoTen == "Nguoi xu A rieng biet");
        ds!.Should().NotContain(x => x.HoTen == "Nguoi xu C rieng biet");
    }

    [Fact]
    public async Task Gia_dinh_va_thanh_vien_cung_khong_ro_giua_hai_giao_xu()
    {
        var giaoXuB = await TaoGiaoXuKhac("Giao xu D (bao mat test)");
        await using (var db = app.TaoContextThuan())
        {
            db.GiaDinh.Add(new GiaDinh { GiaoXuId = app.GiaoXuId, TenGiaDinh = "Gia dinh xu A" });
            db.GiaDinh.Add(new GiaDinh { GiaoXuId = giaoXuB, TenGiaDinh = "Gia dinh xu D" });
            await db.SaveChangesAsync();
        }

        var clientA = app.CreateAuthClient(app.GiaoXuId);
        var res = await clientA.GetAsync("/api/gia-dinh");
        var than = await res.Content.ReadAsStringAsync();

        than.Should().Contain("Gia dinh xu A");
        than.Should().NotContain("Gia dinh xu D");
    }

    [Fact]
    public async Task Dang_nhap_dung_mat_khau_tra_ve_token_dung_giao_xu()
    {
        var (tenTaiKhoan, matKhau) = ("nguoidung_baomat", "MatKhauManh123!");
        await using (var db = app.TaoContextThuan())
        {
            var taiKhoan = new TaiKhoan { GiaoXuId = app.GiaoXuId, TenTaiKhoan = tenTaiKhoan, HoTenNguoiDung = "Nguoi Dung Test" };
            taiKhoan.MatKhauBam = new PasswordHasher<TaiKhoan>().HashPassword(taiKhoan, matKhau);
            db.TaiKhoan.Add(taiKhoan);
            await db.SaveChangesAsync();
        }

        var res = await app.CreateClient().PostAsJsonAsync("/api/auth/dang-nhap",
            new DangNhapRequest(tenTaiKhoan, matKhau));

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        var than = await res.Content.ReadFromJsonAsync<DangNhapOkDto>();
        than!.NguoiDung.GiaoXuId.Should().Be(app.GiaoXuId);

        // Token that phai dung duoc de goi endpoint bao ve.
        var client = app.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", than.Token);
        var goiThu = await client.GetAsync("/api/giao-dan");
        goiThu.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private sealed record DangNhapOkDto(string Token, ThongTinNguoiDungDto NguoiDung);
    private sealed record ThongTinNguoiDungDto(Guid GiaoXuId);

    [Fact]
    public async Task Dang_nhap_sai_mat_khau_bi_tu_choi_khong_lo_thong_tin()
    {
        var (tenTaiKhoan, matKhau) = ("nguoidung_saimk", "MatKhauManh123!");
        await using (var db = app.TaoContextThuan())
        {
            var taiKhoan = new TaiKhoan { GiaoXuId = app.GiaoXuId, TenTaiKhoan = tenTaiKhoan, HoTenNguoiDung = "X" };
            taiKhoan.MatKhauBam = new PasswordHasher<TaiKhoan>().HashPassword(taiKhoan, matKhau);
            db.TaiKhoan.Add(taiKhoan);
            await db.SaveChangesAsync();
        }

        var res = await app.CreateClient().PostAsJsonAsync("/api/auth/dang-nhap",
            new DangNhapRequest(tenTaiKhoan, "sai-mat-khau"));

        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Nguoi_dung_khong_phai_quan_tri_khong_vao_duoc_quan_ly_tai_khoan()
    {
        var client = app.CreateAuthClient(app.GiaoXuId, loaiTaiKhoan: 1); // Nguoi nhap 1

        var res = await client.GetAsync("/api/tai-khoan");

        res.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Quan_tri_vien_vao_duoc_quan_ly_tai_khoan()
    {
        var client = app.CreateAuthClient(app.GiaoXuId, loaiTaiKhoan: 0);

        var res = await client.GetAsync("/api/tai-khoan");

        res.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
