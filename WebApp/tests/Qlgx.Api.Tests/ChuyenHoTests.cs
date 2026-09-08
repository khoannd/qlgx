using System.Net.Http.Json;
using FluentAssertions;
using Qlgx.Domain;
using Qlgx.Domain.Entities;

namespace Qlgx.Api.Tests;

/// <summary>
/// "Chuyển họ hàng loạt" (docs/superpowers/specs/man-hinh/cong-cu-du-lieu.md mục 4) — công cụ
/// sửa dữ liệu hàng loạt nguy hiểm nhất nhóm. Test đúng 4 nguyên tắc bắt buộc: có xem trước,
/// xem trước cho số liệu thật, ghi thật đổi đúng phạm vi cột, và gia đình kéo theo thành viên.
/// </summary>
public class ChuyenHoTests(QlgxApiFactory app) : IClassFixture<QlgxApiFactory>
{
    [Fact]
    public async Task Xem_truoc_giao_dan_tra_dung_so_luong_va_ten_giao_ho_dich()
    {
        await using var db = app.TaoContextThuan();
        var ghDich = new GiaoHo { GiaoXuId = app.GiaoXuId, MaGiaoHoCu = 93001, TenGiaoHo = "GH Dich 1" };
        db.GiaoHo.Add(ghDich);
        var gd1 = new GiaoDan { GiaoXuId = app.GiaoXuId, MaGiaoDanCu = 93001, HoTen = "GD1" };
        var gd2 = new GiaoDan { GiaoXuId = app.GiaoXuId, MaGiaoDanCu = 93002, HoTen = "GD2" };
        db.GiaoDan.AddRange(gd1, gd2);
        await db.SaveChangesAsync();

        var client = app.CreateAuthClient();
        var res = await client.PostAsJsonAsync("/api/cong-cu-du-lieu/chuyen-ho/giao-dan/xem-truoc",
            new { GiaoDanIds = new[] { gd1.Id, gd2.Id }, GiaoHoDichId = ghDich.Id });
        res.EnsureSuccessStatusCode();
        var kq = await res.Content.ReadFromJsonAsync<XemTruocGiaoDan>();

        kq!.SoLuongGiaoDan.Should().Be(2);
        kq.TenGiaoHoDich.Should().Be("GH Dich 1");
    }

    [Fact]
    public async Task Ghi_that_chi_doi_giaoHoId_cua_giao_dan_da_chon()
    {
        await using var db = app.TaoContextThuan();
        var ghNguon = new GiaoHo { GiaoXuId = app.GiaoXuId, MaGiaoHoCu = 93010, TenGiaoHo = "GH Nguon" };
        var ghDich = new GiaoHo { GiaoXuId = app.GiaoXuId, MaGiaoHoCu = 93011, TenGiaoHo = "GH Dich" };
        db.GiaoHo.AddRange(ghNguon, ghDich);
        await db.SaveChangesAsync();
        var gdChon = new GiaoDan { GiaoXuId = app.GiaoXuId, MaGiaoDanCu = 93010, HoTen = "Duoc Chon", GiaoHoId = ghNguon.Id };
        var gdKhongChon = new GiaoDan { GiaoXuId = app.GiaoXuId, MaGiaoDanCu = 93011, HoTen = "Khong Chon", GiaoHoId = ghNguon.Id };
        db.GiaoDan.AddRange(gdChon, gdKhongChon);
        await db.SaveChangesAsync();

        var client = app.CreateAuthClient();
        var res = await client.PostAsJsonAsync("/api/cong-cu-du-lieu/chuyen-ho/giao-dan",
            new { GiaoDanIds = new[] { gdChon.Id }, GiaoHoDichId = ghDich.Id });
        res.EnsureSuccessStatusCode();
        var kq = await res.Content.ReadFromJsonAsync<KetQuaGiaoDan>();
        kq!.SoLuongDaChuyen.Should().Be(1);

        await using var db2 = app.TaoContextThuan();
        (await db2.GiaoDan.FindAsync(gdChon.Id))!.GiaoHoId.Should().Be(ghDich.Id);
        (await db2.GiaoDan.FindAsync(gdKhongChon.Id))!.GiaoHoId.Should().Be(ghNguon.Id);
    }

    [Fact]
    public async Task Xem_truoc_gia_dinh_dem_ca_thanh_vien_se_bi_keo_theo()
    {
        await using var db = app.TaoContextThuan();
        var ghDich = new GiaoHo { GiaoXuId = app.GiaoXuId, MaGiaoHoCu = 93020, TenGiaoHo = "GH Dich GD" };
        db.GiaoHo.Add(ghDich);
        var gdinh = new GiaDinh { GiaoXuId = app.GiaoXuId, MaGiaDinhCu = 93020, TenGiaDinh = "Gia Dinh Chuyen" };
        db.GiaDinh.Add(gdinh);
        var chong = new GiaoDan { GiaoXuId = app.GiaoXuId, MaGiaoDanCu = 93020, HoTen = "Chong" };
        var vo = new GiaoDan { GiaoXuId = app.GiaoXuId, MaGiaoDanCu = 93021, HoTen = "Vo" };
        var con = new GiaoDan { GiaoXuId = app.GiaoXuId, MaGiaoDanCu = 93022, HoTen = "Con" };
        db.GiaoDan.AddRange(chong, vo, con);
        await db.SaveChangesAsync();
        db.ThanhVienGiaDinh.AddRange(
            new ThanhVienGiaDinh { GiaoXuId = app.GiaoXuId, GiaDinhId = gdinh.Id, GiaoDanId = chong.Id, VaiTro = VaiTroGiaDinh.Chong },
            new ThanhVienGiaDinh { GiaoXuId = app.GiaoXuId, GiaDinhId = gdinh.Id, GiaoDanId = vo.Id, VaiTro = VaiTroGiaDinh.Vo },
            new ThanhVienGiaDinh { GiaoXuId = app.GiaoXuId, GiaDinhId = gdinh.Id, GiaoDanId = con.Id, VaiTro = VaiTroGiaDinh.Con });
        await db.SaveChangesAsync();

        var client = app.CreateAuthClient();
        var res = await client.PostAsJsonAsync("/api/cong-cu-du-lieu/chuyen-ho/gia-dinh/xem-truoc",
            new { GiaDinhIds = new[] { gdinh.Id }, GiaoHoDichId = ghDich.Id });
        res.EnsureSuccessStatusCode();
        var kq = await res.Content.ReadFromJsonAsync<XemTruocGiaDinh>();

        kq!.SoLuongGiaDinh.Should().Be(1);
        kq.SoLuongThanhVien.Should().Be(3); // chồng + vợ + con — TẤT CẢ vai trò, không chỉ vợ chồng
    }

    [Fact]
    public async Task Ghi_that_gia_dinh_doi_giaoHoId_ca_gia_dinh_lan_toan_bo_thanh_vien()
    {
        await using var db = app.TaoContextThuan();
        var ghNguon = new GiaoHo { GiaoXuId = app.GiaoXuId, MaGiaoHoCu = 93030, TenGiaoHo = "GH Nguon GD" };
        var ghDich = new GiaoHo { GiaoXuId = app.GiaoXuId, MaGiaoHoCu = 93031, TenGiaoHo = "GH Dich GD 2" };
        db.GiaoHo.AddRange(ghNguon, ghDich);
        var gdinh = new GiaDinh { GiaoXuId = app.GiaoXuId, MaGiaDinhCu = 93031, TenGiaDinh = "GD Ghi That", GiaoHoId = ghNguon.Id };
        db.GiaDinh.Add(gdinh);
        var chong = new GiaoDan { GiaoXuId = app.GiaoXuId, MaGiaoDanCu = 93030, HoTen = "Chong2", GiaoHoId = ghNguon.Id };
        var con = new GiaoDan { GiaoXuId = app.GiaoXuId, MaGiaoDanCu = 93031, HoTen = "Con2", GiaoHoId = ghNguon.Id };
        db.GiaoDan.AddRange(chong, con);
        await db.SaveChangesAsync();
        db.ThanhVienGiaDinh.AddRange(
            new ThanhVienGiaDinh { GiaoXuId = app.GiaoXuId, GiaDinhId = gdinh.Id, GiaoDanId = chong.Id, VaiTro = VaiTroGiaDinh.Chong },
            new ThanhVienGiaDinh { GiaoXuId = app.GiaoXuId, GiaDinhId = gdinh.Id, GiaoDanId = con.Id, VaiTro = VaiTroGiaDinh.Con });
        await db.SaveChangesAsync();

        var client = app.CreateAuthClient();
        var res = await client.PostAsJsonAsync("/api/cong-cu-du-lieu/chuyen-ho/gia-dinh",
            new { GiaDinhIds = new[] { gdinh.Id }, GiaoHoDichId = ghDich.Id });
        res.EnsureSuccessStatusCode();
        var kq = await res.Content.ReadFromJsonAsync<KetQuaGiaDinh>();
        kq!.SoLuongGiaDinhDaChuyen.Should().Be(1);
        kq.SoLuongThanhVienDaChuyen.Should().Be(2);

        await using var db2 = app.TaoContextThuan();
        (await db2.GiaDinh.FindAsync(gdinh.Id))!.GiaoHoId.Should().Be(ghDich.Id);
        (await db2.GiaoDan.FindAsync(chong.Id))!.GiaoHoId.Should().Be(ghDich.Id);
        (await db2.GiaoDan.FindAsync(con.Id))!.GiaoHoId.Should().Be(ghDich.Id);
    }

    [Fact]
    public async Task Giao_ho_dich_khong_ton_tai_tra_ve_404_khong_doi_gi()
    {
        await using var db = app.TaoContextThuan();
        var gd = new GiaoDan { GiaoXuId = app.GiaoXuId, MaGiaoDanCu = 93040, HoTen = "Khong Doi" };
        db.GiaoDan.Add(gd);
        await db.SaveChangesAsync();

        var client = app.CreateAuthClient();
        var res = await client.PostAsJsonAsync("/api/cong-cu-du-lieu/chuyen-ho/giao-dan",
            new { GiaoDanIds = new[] { gd.Id }, GiaoHoDichId = Guid.NewGuid() });
        res.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);

        await using var db2 = app.TaoContextThuan();
        (await db2.GiaoDan.FindAsync(gd.Id))!.GiaoHoId.Should().BeNull();
    }

    private sealed record XemTruocGiaoDan(int SoLuongGiaoDan, string TenGiaoHoDich);
    private sealed record KetQuaGiaoDan(int SoLuongDaChuyen);
    private sealed record XemTruocGiaDinh(int SoLuongGiaDinh, int SoLuongThanhVien, string TenGiaoHoDich);
    private sealed record KetQuaGiaDinh(int SoLuongGiaDinhDaChuyen, int SoLuongThanhVienDaChuyen);
}
