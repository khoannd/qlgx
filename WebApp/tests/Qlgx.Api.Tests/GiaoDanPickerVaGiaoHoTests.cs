using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Qlgx.Domain.Entities;

namespace Qlgx.Api.Tests;

/// <summary>
/// Hai hạ tầng dùng chung phía web: GET /api/giao-dan/tim (cho GxPicker thật chọn Tên Cha/Mẹ,
/// Người nam/nữ) và GET /api/giao-ho (danh mục giáo họ thật, thay data/giaoHoTam.ts). Cộng Rule
/// 15 (CheckTuoiChaMe) nay tái hiện được nhờ GiaoDan.ChaId/MeId — xem
/// docs/superpowers/specs/man-hinh/can-review-sau.md mục 19.
/// </summary>
public class GiaoDanPickerVaGiaoHoTests(QlgxApiFactory app) : IClassFixture<QlgxApiFactory>
{
    private sealed record TimKiemItem(Guid Id, int MaGiaoDanCu, string? TenThanh, string HoTen, string? Phai, DateOnly? NgaySinh);
    private sealed record GiaoHoItem(Guid Id, int MaGiaoHoCu, string TenGiaoHo, Guid? GiaoHoChaId);
    private sealed record ThongBaoLoi(string ThongBao);
    private sealed record KetQuaLuu(Guid? Id, string[] CanhBao);

    private async Task<Guid> TaoGiaoDan(string hoTen, string phai, DateOnly? ngaySinh)
    {
        await using var db = app.TaoContextThuan();
        var max = await db.GiaoDan.MaxAsync(x => (int?)x.MaGiaoDanCu) ?? 0;
        var g = new GiaoDan { GiaoXuId = app.GiaoXuId, MaGiaoDanCu = max + 1, HoTen = hoTen, Phai = phai, NgaySinh = ngaySinh };
        db.GiaoDan.Add(g);
        await db.SaveChangesAsync();
        return g.Id;
    }

    // --- GET /api/giao-dan/tim ------------------------------------------------------------

    [Fact]
    public async Task Tim_kiem_theo_ten_tra_ve_dung_nguoi()
    {
        await TaoGiaoDan("Nguyen Van Tim Kiem Ung Vien", "Nam", new DateOnly(1970, 1, 1));
        await TaoGiaoDan("Nguoi Khac Khong Lien Quan", "Nam", new DateOnly(1970, 1, 1));

        var ket = await app.CreateAuthClient()
            .GetFromJsonAsync<List<TimKiemItem>>("/api/giao-dan/tim?tuKhoa=Tim Kiem Ung Vien");

        ket.Should().ContainSingle(x => x.HoTen == "Nguyen Van Tim Kiem Ung Vien");
    }

    [Fact]
    public async Task Tim_kiem_khong_tu_khoa_gioi_han_so_luong_ket_qua()
    {
        var res = await app.CreateAuthClient().GetAsync("/api/giao-dan/tim?limit=3");
        res.StatusCode.Should().Be(HttpStatusCode.OK);
        var ket = await res.Content.ReadFromJsonAsync<List<TimKiemItem>>();
        ket!.Count.Should().BeLessOrEqualTo(3);
    }

    // --- GET /api/giao-ho -------------------------------------------------------------------

    [Fact]
    public async Task Danh_muc_giao_ho_tra_ve_dung_du_lieu_that()
    {
        await using (var db = app.TaoContextThuan())
        {
            db.GiaoHo.Add(new GiaoHo { GiaoXuId = app.GiaoXuId, MaGiaoHoCu = 1, TenGiaoHo = "Giáo họ Kiểm Thử" });
            await db.SaveChangesAsync();
        }

        var ket = await app.CreateAuthClient().GetFromJsonAsync<List<GiaoHoItem>>("/api/giao-ho");

        ket.Should().ContainSingle(x => x.TenGiaoHo == "Giáo họ Kiểm Thử");
    }

    [Fact]
    public async Task Danh_muc_giao_ho_khong_tra_ve_ban_ghi_da_xoa_mem()
    {
        await using (var db = app.TaoContextThuan())
        {
            db.GiaoHo.Add(new GiaoHo
            { GiaoXuId = app.GiaoXuId, MaGiaoHoCu = 2, TenGiaoHo = "Giáo họ Đã Xoá", DaXoa = true });
            await db.SaveChangesAsync();
        }

        var ket = await app.CreateAuthClient().GetFromJsonAsync<List<GiaoHoItem>>("/api/giao-ho");

        ket.Should().NotContain(x => x.TenGiaoHo == "Giáo họ Đã Xoá");
    }

    // --- POST/PUT /api/giao-ho (thêm/sửa, luôn trong phạm vi giáo xứ của người gọi) ---------

    [Fact]
    public async Task Them_giao_ho_moi_thanh_cong_va_hien_trong_danh_sach()
    {
        var client = app.CreateAuthClient();

        var res = await client.PostAsJsonAsync("/api/giao-ho", new { TenGiaoHo = "Giáo họ Vừa Thêm" });

        res.StatusCode.Should().Be(HttpStatusCode.Created);
        var ds = await client.GetFromJsonAsync<List<GiaoHoItem>>("/api/giao-ho");
        ds.Should().Contain(x => x.TenGiaoHo == "Giáo họ Vừa Thêm");
    }

    [Fact]
    public async Task Sua_ten_giao_ho_thanh_cong()
    {
        Guid id;
        await using (var db = app.TaoContextThuan())
        {
            var gh = new GiaoHo { GiaoXuId = app.GiaoXuId, MaGiaoHoCu = 88903, TenGiaoHo = "Ten Cu" };
            db.GiaoHo.Add(gh);
            await db.SaveChangesAsync();
            id = gh.Id;
        }
        var client = app.CreateAuthClient();

        var res = await client.PutAsJsonAsync($"/api/giao-ho/{id}", new { TenGiaoHo = "Ten Moi" });

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        var ds = await client.GetFromJsonAsync<List<GiaoHoItem>>("/api/giao-ho");
        ds.Should().Contain(x => x.Id == id && x.TenGiaoHo == "Ten Moi");
    }

    [Fact]
    public async Task Sua_giao_ho_khong_ton_tai_tra_ve_404()
    {
        var client = app.CreateAuthClient();

        var res = await client.PutAsJsonAsync($"/api/giao-ho/{Guid.NewGuid()}", new { TenGiaoHo = "X" });

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Khong_sua_duoc_giao_ho_cua_giao_xu_khac_boi_loc_tenant()
    {
        var giaoXuKhac = Guid.NewGuid();
        Guid idGiaoHoXuKhac;
        await using (var db = app.TaoContextThuan())
        {
            db.GiaoXu.Add(new GiaoXu { Id = giaoXuKhac, TenGiaoXu = "Giao xu B giao ho test", MaGiaoXuCu = 88901 });
            var gh = new GiaoHo { GiaoXuId = giaoXuKhac, MaGiaoHoCu = 1, TenGiaoHo = "Giao ho cua Xu B" };
            db.GiaoHo.Add(gh);
            await db.SaveChangesAsync();
            idGiaoHoXuKhac = gh.Id;
        }
        // Dang nhap voi tu cach giao xu cua chinh app.GiaoXuId (KHONG phai giaoXuKhac).
        var client = app.CreateAuthClient();

        var res = await client.PutAsJsonAsync($"/api/giao-ho/{idGiaoHoXuKhac}", new { TenGiaoHo = "Bi sua trai phep" });

        res.StatusCode.Should().Be(HttpStatusCode.NotFound); // bo loc EF an di, khong phai 403
    }

    // --- Rule 15 (CheckTuoiChaMe) qua ChaId/MeId thật ---------------------------------------

    private static object YeuCauToiThieu(string hoTen, DateOnly ngaySinh, Guid? chaId = null, Guid? meId = null) => new
    {
        HoTen = hoTen, Phai = "Nam", NgaySinh = ngaySinh, ChaId = chaId, MeId = meId, BoQuaCanhBao = false,
    };

    [Fact]
    public async Task Tao_giao_dan_voi_cha_qua_tre_bi_chan_dung_thong_bao_nguyen_van()
    {
        var homNay = DateOnly.FromDateTime(DateTime.Now);
        // Cha chi hon con 10 tuoi ( < 15 nam toi thieu theo TUOI_CHO_PHEP_CO_CON ).
        var chaId = await TaoGiaoDan("Ong Cha Qua Tre", "Nam", homNay.AddYears(-25));
        var client = app.CreateAuthClient();

        var res = await client.PostAsJsonAsync("/api/giao-dan",
            YeuCauToiThieu("Con Cua Ong Cha Qua Tre", homNay.AddYears(-15), chaId));

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var loi = (await res.Content.ReadFromJsonAsync<ThongBaoLoi>())!.ThongBao;
        loi.Should().Contain("chưa đủ 15 tuổi để có con");
    }

    [Fact]
    public async Task Tao_giao_dan_voi_cha_du_tuoi_thanh_cong()
    {
        var homNay = DateOnly.FromDateTime(DateTime.Now);
        var chaId = await TaoGiaoDan("Ong Cha Du Tuoi", "Nam", homNay.AddYears(-45));
        var client = app.CreateAuthClient();

        var res = await client.PostAsJsonAsync("/api/giao-dan",
            YeuCauToiThieu("Con Cua Ong Cha Du Tuoi", homNay.AddYears(-15), chaId));

        res.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Tao_giao_dan_khong_chon_ChaId_khong_bi_anh_huong_boi_rule_15()
    {
        var homNay = DateOnly.FromDateTime(DateTime.Now);
        var client = app.CreateAuthClient();

        var res = await client.PostAsJsonAsync("/api/giao-dan",
            YeuCauToiThieu("Giao Dan Khong Co Cha Chon Qua Picker", homNay.AddYears(-20)));

        res.StatusCode.Should().Be(HttpStatusCode.Created);
    }
}
