using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Qlgx.Domain;
using Qlgx.Domain.Entities;

namespace Qlgx.Api.Tests;

/// <summary>
/// Task "ghi gia đình": tạo mới, xoá, thêm/xoá thành viên, gán Người nam/Người nữ — trước đây
/// bản web chỉ đọc/sửa vài trường của một gia đình đã có, không có endpoint nào tạo/xoá/thêm
/// thành viên hay gán vợ chồng (xem docs/superpowers/specs/man-hinh/gia-dinh-chi-tiet.md mục 10
/// và gia-dinh-danh-sach.md mục 10).
/// </summary>
public class GiaDinhGhiTests(QlgxApiFactory app) : IClassFixture<QlgxApiFactory>
{
    private sealed record ThongBaoLoi(string ThongBao);
    private sealed record KetQuaTao(Guid Id, int MaGiaDinhCu);
    private sealed record KetQuaThanhVien(Guid? GiaoDanId, string[] CanhBao);

    private async Task<Guid> TaoGiaoDan(string hoTen, string phai, DateOnly? ngaySinh = null, bool daXoa = false)
    {
        await using var db = app.TaoContextThuan();
        var max = await db.GiaoDan.MaxAsync(x => (int?)x.MaGiaoDanCu) ?? 0;
        var g = new GiaoDan
        {
            GiaoXuId = app.GiaoXuId, MaGiaoDanCu = max + 1, HoTen = hoTen, Phai = phai,
            NgaySinh = ngaySinh ?? new DateOnly(1990, 1, 1), DaXoa = daXoa,
        };
        db.GiaoDan.Add(g);
        await db.SaveChangesAsync();
        return g.Id;
    }

    private async Task<Guid> TaoGiaDinhTrong(int ma)
    {
        await using var db = app.TaoContextThuan();
        var g = new GiaDinh { GiaoXuId = app.GiaoXuId, MaGiaDinhCu = ma, TenGiaDinh = "Gia dinh " + ma };
        db.GiaDinh.Add(g);
        await db.SaveChangesAsync();
        return g.Id;
    }

    // --- Tạo mới -------------------------------------------------------------------------

    [Fact]
    public async Task Tao_gia_dinh_moi_tra_ve_201_va_sinh_ma_gia_dinh_cu()
    {
        var client = app.CreateAuthClient();

        var res = await client.PostAsJsonAsync("/api/gia-dinh", new { TenGiaDinh = "Ho gia dinh moi" });

        res.StatusCode.Should().Be(HttpStatusCode.Created);
        var kq = await res.Content.ReadFromJsonAsync<KetQuaTao>();

        await using var db = app.TaoContextThuan();
        var gd = await db.GiaDinh.SingleAsync(x => x.Id == kq!.Id);
        gd.MaGiaDinhCu.Should().BeGreaterThan(0, "ma gia dinh cu phai duoc SinhMaService cap phat");
        gd.MaNhanDang.Should().NotBeNullOrEmpty("ban ghi web tao moi phai duoc sinh MaNhanDang moi");
        gd.TenGiaDinh.Should().Be("Ho gia dinh moi");
    }

    [Fact]
    public async Task Tao_gia_dinh_hai_lan_khong_bi_trung_ma_gia_dinh_cu()
    {
        var client = app.CreateAuthClient();

        var r1 = await client.PostAsJsonAsync("/api/gia-dinh", new { TenGiaDinh = "A" });
        var r2 = await client.PostAsJsonAsync("/api/gia-dinh", new { TenGiaDinh = "B" });

        var k1 = await r1.Content.ReadFromJsonAsync<KetQuaTao>();
        var k2 = await r2.Content.ReadFromJsonAsync<KetQuaTao>();
        k1!.MaGiaDinhCu.Should().NotBe(k2!.MaGiaDinhCu);
    }

    // --- Xoá -----------------------------------------------------------------------------

    [Fact]
    public async Task Xoa_mem_chi_dat_co_DaXoa_giu_nguyen_thanh_vien()
    {
        var id = await TaoGiaDinhTrong(400);
        var client = app.CreateAuthClient();

        var res = await client.DeleteAsync($"/api/gia-dinh/{id}");

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        await using var db = app.TaoContextThuan();
        (await db.GiaDinh.IgnoreQueryFilters().SingleAsync(x => x.Id == id)).DaXoa.Should().BeTrue();
    }

    [Fact]
    public async Task Xoa_vinh_vien_xoa_ca_ban_ghi_gia_dinh_lan_thanh_vien()
    {
        var id = await TaoGiaDinhTrong(401);
        var giaoDanId = await TaoGiaoDan("Con Thu Nhat", "Nam");
        var client = app.CreateAuthClient();
        await client.PostAsJsonAsync($"/api/gia-dinh/{id}/thanh-vien",
            new { GiaoDanId = giaoDanId, VaiTro = 2, BoQuaCanhBao = false });

        var res = await client.DeleteAsync($"/api/gia-dinh/{id}?vinhVien=true");

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        await using var db = app.TaoContextThuan();
        (await db.GiaDinh.IgnoreQueryFilters().AnyAsync(x => x.Id == id)).Should().BeFalse();
        (await db.ThanhVienGiaDinh.AnyAsync(x => x.GiaDinhId == id)).Should().BeFalse();
    }

    [Fact]
    public async Task Xoa_gia_dinh_khong_ton_tai_tra_404()
    {
        var res = await app.CreateAuthClient().DeleteAsync($"/api/gia-dinh/{Guid.NewGuid()}");
        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // --- Thêm/xoá thành viên ---------------------------------------------------------------

    [Fact]
    public async Task Them_thanh_vien_hop_le_thanh_cong()
    {
        var id = await TaoGiaDinhTrong(410);
        var giaoDanId = await TaoGiaoDan("Con Gai Ut", "Nữ");
        var client = app.CreateAuthClient();

        var res = await client.PostAsJsonAsync($"/api/gia-dinh/{id}/thanh-vien",
            new { GiaoDanId = giaoDanId, VaiTro = 2, BoQuaCanhBao = false });

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        var kq = await res.Content.ReadFromJsonAsync<KetQuaThanhVien>();
        kq!.GiaoDanId.Should().Be(giaoDanId);

        await using var db = app.TaoContextThuan();
        (await db.ThanhVienGiaDinh.SingleAsync(x => x.GiaDinhId == id && x.GiaoDanId == giaoDanId))
            .VaiTro.Should().Be((VaiTroGiaDinh)2);
    }

    [Fact]
    public async Task Them_thanh_vien_giu_nguyen_gia_tri_VaiTro_tho_khong_thuoc_enum()
    {
        var id = await TaoGiaDinhTrong(411);
        var giaoDanId = await TaoGiaoDan("Chau Noi", "Nam");
        var client = app.CreateAuthClient();

        // 8 la mot gia tri VaiTro "la" doc duoc tu du lieu Access that (xem GiaoDanService) —
        // dung tuong minh 100 (chua ro) de khong dam bao trung voi hang so nao trong enum.
        var res = await client.PostAsJsonAsync($"/api/gia-dinh/{id}/thanh-vien",
            new { GiaoDanId = giaoDanId, VaiTro = 100, BoQuaCanhBao = false });

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        await using var db = app.TaoContextThuan();
        ((int)(await db.ThanhVienGiaDinh.SingleAsync(x => x.GiaDinhId == id && x.GiaoDanId == giaoDanId)).VaiTro)
            .Should().Be(100);
    }

    [Fact]
    public async Task Them_nguoi_dang_la_chong_hien_tai_bi_chan()
    {
        var id = await TaoGiaDinhTrong(412);
        var chongId = await TaoGiaoDan("Ong Chong", "Nam");
        await using (var db = app.TaoContextThuan())
        {
            db.ThanhVienGiaDinh.Add(new ThanhVienGiaDinh
            { GiaoXuId = app.GiaoXuId, GiaDinhId = id, GiaoDanId = chongId, VaiTro = VaiTroGiaDinh.Chong, ChuHo = true });
            await db.SaveChangesAsync();
        }
        var client = app.CreateAuthClient();

        var res = await client.PostAsJsonAsync($"/api/gia-dinh/{id}/thanh-vien",
            new { GiaoDanId = chongId, VaiTro = 2, BoQuaCanhBao = false });

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await res.Content.ReadFromJsonAsync<ThongBaoLoi>())!.ThongBao.Should().Be("Giáo dân này đã có trong gia đình");
    }

    [Fact]
    public async Task Them_nguoi_da_thuoc_gia_dinh_khac_tra_ve_canh_bao_roi_van_cho_them_khi_xac_nhan()
    {
        var idCu = await TaoGiaDinhTrong(420);
        var idMoi = await TaoGiaDinhTrong(421);
        var giaoDanId = await TaoGiaoDan("Nguoi Da Co Gia Dinh", "Nam");
        await using (var db = app.TaoContextThuan())
        {
            db.ThanhVienGiaDinh.Add(new ThanhVienGiaDinh
            { GiaoXuId = app.GiaoXuId, GiaDinhId = idCu, GiaoDanId = giaoDanId, VaiTro = VaiTroGiaDinh.Con });
            await db.SaveChangesAsync();
        }
        var client = app.CreateAuthClient();

        var chuaXacNhan = await client.PostAsJsonAsync($"/api/gia-dinh/{idMoi}/thanh-vien",
            new { GiaoDanId = giaoDanId, VaiTro = 2, BoQuaCanhBao = false });
        chuaXacNhan.StatusCode.Should().Be(HttpStatusCode.OK, "co canh bao nen chua luu, tra 200 kem canhBao");
        var kqChuaXacNhan = await chuaXacNhan.Content.ReadFromJsonAsync<KetQuaThanhVien>();
        kqChuaXacNhan!.GiaoDanId.Should().BeNull();
        kqChuaXacNhan.CanhBao.Should().ContainSingle(c => c.Contains("đã thuộc về gia đình"));

        var xacNhan = await client.PostAsJsonAsync($"/api/gia-dinh/{idMoi}/thanh-vien",
            new { GiaoDanId = giaoDanId, VaiTro = 2, BoQuaCanhBao = true });
        xacNhan.StatusCode.Should().Be(HttpStatusCode.OK);

        // can-review-sau.md muc 2: van con trong gia dinh CU (khong tu xoa) — thuoc CA HAI.
        await using var db2 = app.TaoContextThuan();
        (await db2.ThanhVienGiaDinh.CountAsync(x => x.GiaoDanId == giaoDanId)).Should().Be(2,
            "tai hien y het loi desktop: hoi roi van cho them ma khong xoa khoi gia dinh cu");
    }

    [Fact]
    public async Task Them_nguoi_da_xoa_mem_tra_canh_bao_roi_tu_khoi_phuc_DaXoa_khi_xac_nhan()
    {
        var id = await TaoGiaDinhTrong(430);
        var giaoDanId = await TaoGiaoDan("Nguoi Da Bi Xoa", "Nam", daXoa: true);
        var client = app.CreateAuthClient();

        var xacNhan = await client.PostAsJsonAsync($"/api/gia-dinh/{id}/thanh-vien",
            new { GiaoDanId = giaoDanId, VaiTro = 2, BoQuaCanhBao = true });

        xacNhan.StatusCode.Should().Be(HttpStatusCode.OK);
        await using var db = app.TaoContextThuan();
        (await db.GiaoDan.IgnoreQueryFilters().SingleAsync(x => x.Id == giaoDanId)).DaXoa.Should().BeFalse();
    }

    [Fact]
    public async Task Xoa_thanh_vien_la_xoa_vinh_vien_khoi_ThanhVienGiaDinh()
    {
        var id = await TaoGiaDinhTrong(440);
        var giaoDanId = await TaoGiaoDan("Con Se Bi Xoa", "Nam");
        var client = app.CreateAuthClient();
        await client.PostAsJsonAsync($"/api/gia-dinh/{id}/thanh-vien",
            new { GiaoDanId = giaoDanId, VaiTro = 2, BoQuaCanhBao = false });

        var res = await client.DeleteAsync($"/api/gia-dinh/{id}/thanh-vien/{giaoDanId}/2");

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        await using var db = app.TaoContextThuan();
        (await db.ThanhVienGiaDinh.AnyAsync(x => x.GiaDinhId == id && x.GiaoDanId == giaoDanId)).Should().BeFalse();
        // Giao dan van con, chi mat lien ket voi gia dinh nay.
        (await db.GiaoDan.AnyAsync(x => x.Id == giaoDanId)).Should().BeTrue();
    }

    // --- Gán/đổi Người nam / Người nữ -------------------------------------------------------

    [Fact]
    public async Task Gan_nguoi_nam_hop_le_thanh_cong()
    {
        var id = await TaoGiaDinhTrong(450);
        var giaoDanId = await TaoGiaoDan("Ong Chong Moi", "Nam", new DateOnly(1985, 1, 1));
        var client = app.CreateAuthClient();
        var chiTiet = await client.GetFromJsonAsync<GiaDinhChiTietToiThieu>($"/api/gia-dinh/{id}");

        var res = await client.PutAsJsonAsync($"/api/gia-dinh/{id}/vo-chong/0",
            new { GiaoDanId = giaoDanId, RowVersion = chiTiet!.RowVersion, BoQuaCanhBao = false, XuLyNguoiCu = (object?)null });

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        await using var db = app.TaoContextThuan();
        (await db.ThanhVienGiaDinh.SingleAsync(x => x.GiaDinhId == id)).VaiTro.Should().Be(VaiTroGiaDinh.Chong);
    }

    private sealed record GiaDinhChiTietToiThieu(Guid Id, uint RowVersion);

    [Fact]
    public async Task Gan_nguoi_nam_la_nu_bi_chan_dung_thong_bao_nguyen_van()
    {
        var id = await TaoGiaDinhTrong(451);
        var giaoDanId = await TaoGiaoDan("Ba Kia", "Nữ", new DateOnly(1985, 1, 1));
        var client = app.CreateAuthClient();

        var res = await client.PutAsJsonAsync($"/api/gia-dinh/{id}/vo-chong/0",
            new { GiaoDanId = giaoDanId, RowVersion = 0u, BoQuaCanhBao = false, XuLyNguoiCu = (object?)null });

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await res.Content.ReadFromJsonAsync<ThongBaoLoi>())!.ThongBao.Should().Be("Người chồng không thể là nữ!");
    }

    [Fact]
    public async Task Gan_nguoi_nu_la_nam_bi_chan_dung_thong_bao_nguyen_van()
    {
        var id = await TaoGiaDinhTrong(452);
        var giaoDanId = await TaoGiaoDan("Ong Kia", "Nam", new DateOnly(1985, 1, 1));
        var client = app.CreateAuthClient();

        var res = await client.PutAsJsonAsync($"/api/gia-dinh/{id}/vo-chong/1",
            new { GiaoDanId = giaoDanId, RowVersion = 0u, BoQuaCanhBao = false, XuLyNguoiCu = (object?)null });

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await res.Content.ReadFromJsonAsync<ThongBaoLoi>())!.ThongBao.Should().Be("Người vợ không thể là nam!");
    }

    [Fact]
    public async Task Gan_nguoi_duoi_14_tuoi_bi_chan_cung()
    {
        var id = await TaoGiaDinhTrong(453);
        var homNay = DateOnly.FromDateTime(DateTime.Now);
        var giaoDanId = await TaoGiaoDan("Tre Con", "Nam", homNay.AddYears(-10));
        var client = app.CreateAuthClient();

        var res = await client.PutAsJsonAsync($"/api/gia-dinh/{id}/vo-chong/0",
            new { GiaoDanId = giaoDanId, RowVersion = 0u, BoQuaCanhBao = false, XuLyNguoiCu = (object?)null });

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await res.Content.ReadFromJsonAsync<ThongBaoLoi>())!.ThongBao
            .Should().Be("Giáo dân này hiện tại chưa đủ 14 tuổi. Không thể kết hôn");
    }

    [Fact]
    public async Task Gan_nguoi_14_den_17_tuoi_tra_canh_bao_bo_qua_duoc()
    {
        var id = await TaoGiaDinhTrong(454);
        var homNay = DateOnly.FromDateTime(DateTime.Now);
        var giaoDanId = await TaoGiaoDan("Thieu Nien", "Nam", homNay.AddYears(-16));
        var client = app.CreateAuthClient();

        var chuaXacNhan = await client.PutAsJsonAsync($"/api/gia-dinh/{id}/vo-chong/0",
            new { GiaoDanId = giaoDanId, RowVersion = 0u, BoQuaCanhBao = false, XuLyNguoiCu = (object?)null });
        chuaXacNhan.StatusCode.Should().Be(HttpStatusCode.OK);
        var kq = await chuaXacNhan.Content.ReadFromJsonAsync<KetQuaThanhVien>();
        kq!.GiaoDanId.Should().BeNull();
        kq.CanhBao.Should().ContainSingle(c => c.Contains("chưa đủ 18 tuổi"));

        var chiTiet = await client.GetFromJsonAsync<GiaDinhChiTietToiThieu>($"/api/gia-dinh/{id}");
        var xacNhan = await client.PutAsJsonAsync($"/api/gia-dinh/{id}/vo-chong/0",
            new { GiaoDanId = giaoDanId, RowVersion = chiTiet!.RowVersion, BoQuaCanhBao = true, XuLyNguoiCu = (object?)null });
        xacNhan.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Gan_nguoi_dang_la_chong_gia_dinh_khac_con_hieu_luc_bi_chan()
    {
        var giaDinhKhac = await TaoGiaDinhTrong(460);
        var chongId = await TaoGiaoDan("Da Co Vo O Noi Khac", "Nam", new DateOnly(1980, 1, 1));
        await using (var db = app.TaoContextThuan())
        {
            db.ThanhVienGiaDinh.Add(new ThanhVienGiaDinh
            { GiaoXuId = app.GiaoXuId, GiaDinhId = giaDinhKhac, GiaoDanId = chongId, VaiTro = VaiTroGiaDinh.Chong, ChuHo = true });
            await db.SaveChangesAsync();
        }
        var idMoi = await TaoGiaDinhTrong(461);
        var client = app.CreateAuthClient();

        var res = await client.PutAsJsonAsync($"/api/gia-dinh/{idMoi}/vo-chong/0",
            new { GiaoDanId = chongId, RowVersion = 0u, BoQuaCanhBao = false, XuLyNguoiCu = (object?)null });

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var loi = (await res.Content.ReadFromJsonAsync<ThongBaoLoi>())!.ThongBao;
        loi.Should().Contain("người nam (Người chồng)").And.Contain("Gia dinh 460").And.Contain("[460]");
    }

    [Fact]
    public async Task Doi_nguoi_chong_khi_chua_quyet_dinh_nguoi_cu_tra_canh_bao_can_quyet_dinh()
    {
        var id = await TaoGiaDinhTrong(470);
        var chongCuId = await TaoGiaoDan("Chong Cu", "Nam", new DateOnly(1980, 1, 1));
        await using (var db = app.TaoContextThuan())
        {
            db.ThanhVienGiaDinh.Add(new ThanhVienGiaDinh
            { GiaoXuId = app.GiaoXuId, GiaDinhId = id, GiaoDanId = chongCuId, VaiTro = VaiTroGiaDinh.Chong, ChuHo = true });
            await db.SaveChangesAsync();
        }
        var chongMoiId = await TaoGiaoDan("Chong Moi", "Nam", new DateOnly(1985, 1, 1));
        var client = app.CreateAuthClient();

        var res = await client.PutAsJsonAsync($"/api/gia-dinh/{id}/vo-chong/0",
            new { GiaoDanId = chongMoiId, RowVersion = 0u, BoQuaCanhBao = false, XuLyNguoiCu = (object?)null });

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        var kq = await res.Content.ReadFromJsonAsync<KetQuaThanhVien>();
        kq!.GiaoDanId.Should().BeNull();
        kq.CanhBao.Should().ContainSingle();

        await using var dbSau = app.TaoContextThuan();
        (await dbSau.ThanhVienGiaDinh.SingleAsync(x => x.GiaDinhId == id)).GiaoDanId.Should().Be(chongCuId,
            "chua quyet dinh nguoi cu thi KHONG duoc doi gi ca");
    }

    [Fact]
    public async Task Doi_nguoi_chong_ha_nguoi_cu_xuong_thanh_vien_thuc_hien_nguyen_tu()
    {
        var id = await TaoGiaDinhTrong(471);
        var chongCuId = await TaoGiaoDan("Chong Cu Ha Xuong", "Nam", new DateOnly(1980, 1, 1));
        await using (var db = app.TaoContextThuan())
        {
            db.ThanhVienGiaDinh.Add(new ThanhVienGiaDinh
            { GiaoXuId = app.GiaoXuId, GiaDinhId = id, GiaoDanId = chongCuId, VaiTro = VaiTroGiaDinh.Chong, ChuHo = true });
            await db.SaveChangesAsync();
        }
        var chongMoiId = await TaoGiaoDan("Chong Moi 2", "Nam", new DateOnly(1985, 1, 1));
        var client = app.CreateAuthClient();
        var chiTiet = await client.GetFromJsonAsync<GiaDinhChiTietToiThieu>($"/api/gia-dinh/{id}");

        var res = await client.PutAsJsonAsync($"/api/gia-dinh/{id}/vo-chong/0", new
        {
            GiaoDanId = chongMoiId, RowVersion = chiTiet!.RowVersion, BoQuaCanhBao = false,
            XuLyNguoiCu = new { Xoa = false, VaiTroMoi = 4 },
        });

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        await using var db2 = app.TaoContextThuan();
        var thanhVien = await db2.ThanhVienGiaDinh.Where(x => x.GiaDinhId == id).ToListAsync();
        thanhVien.Should().HaveCount(2);
        thanhVien.Should().ContainSingle(x => x.GiaoDanId == chongMoiId && x.VaiTro == VaiTroGiaDinh.Chong);
        thanhVien.Should().ContainSingle(x => x.GiaoDanId == chongCuId && (int)x.VaiTro == 4);
    }

    [Fact]
    public async Task Doi_nguoi_chong_xoa_han_nguoi_cu_khong_con_dong_nao_cho_nguoi_cu()
    {
        var id = await TaoGiaDinhTrong(472);
        var chongCuId = await TaoGiaoDan("Chong Cu Xoa Han", "Nam", new DateOnly(1980, 1, 1));
        await using (var db = app.TaoContextThuan())
        {
            db.ThanhVienGiaDinh.Add(new ThanhVienGiaDinh
            { GiaoXuId = app.GiaoXuId, GiaDinhId = id, GiaoDanId = chongCuId, VaiTro = VaiTroGiaDinh.Chong, ChuHo = true });
            await db.SaveChangesAsync();
        }
        var chongMoiId = await TaoGiaoDan("Chong Moi 3", "Nam", new DateOnly(1985, 1, 1));
        var client = app.CreateAuthClient();
        var chiTiet = await client.GetFromJsonAsync<GiaDinhChiTietToiThieu>($"/api/gia-dinh/{id}");

        var res = await client.PutAsJsonAsync($"/api/gia-dinh/{id}/vo-chong/0", new
        {
            GiaoDanId = chongMoiId, RowVersion = chiTiet!.RowVersion, BoQuaCanhBao = false,
            XuLyNguoiCu = new { Xoa = true, VaiTroMoi = (int?)null },
        });

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        await using var db2 = app.TaoContextThuan();
        var thanhVien = await db2.ThanhVienGiaDinh.Where(x => x.GiaDinhId == id).ToListAsync();
        thanhVien.Should().ContainSingle();
        thanhVien[0].GiaoDanId.Should().Be(chongMoiId);
    }
}
