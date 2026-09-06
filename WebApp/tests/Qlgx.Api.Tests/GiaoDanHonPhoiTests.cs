using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Qlgx.Domain.Entities;

namespace Qlgx.Api.Tests;

/// <summary>
/// Task 15 — tab "Hôn phối" trong màn hình chi tiết giáo dân. Xem
/// docs/superpowers/specs/man-hinh/hon-phoi.md mục 0: bản web migrate theo hành vi của
/// GxHonPhoiGiaDinh (xem/sửa hôn phối đã gắn sẵn), KHÔNG theo picker chọn vợ/chồng của
/// frmHonPhoi. Khác cả hai control desktop, endpoint GET trả TOÀN BỘ danh sách hôn phối của
/// một giáo dân (không chỉ một bản ghi) vì dữ liệu thật có trường hợp goá rồi tái hôn.
/// </summary>
public class GiaoDanHonPhoiTests(QlgxApiFactory app) : IClassFixture<QlgxApiFactory>
{
    private sealed record HonPhoiCuaGiaoDan(Guid Id, string? TenHonPhoi, string? SoHonPhoi,
        DateOnly? NgayHonPhoi, string? NoiHonPhoi, string? LinhMucChung, string? NguoiChung1,
        string? NguoiChung2, string? CachThucHonPhoi, string? GhiChu,
        Guid? VoChongId, string? TenVoChong, uint RowVersion);

    private sealed record CapNhatHonPhoi(string? SoHonPhoi, DateOnly? NgayHonPhoi,
        string? NoiHonPhoi, string? LinhMucChung, string? NguoiChung1, string? NguoiChung2,
        string? CachThucHonPhoi, string? GhiChu, uint RowVersion);

    private sealed record ThongBaoLoi(string ThongBao);

    private async Task<Guid> TaoGiaoDan(int ma, string hoTen, string phai = "Nam")
    {
        await using var db = app.TaoContextThuan();
        var gd = new GiaoDan { GiaoXuId = app.GiaoXuId, MaGiaoDanCu = ma, HoTen = hoTen, Phai = phai };
        db.GiaoDan.Add(gd);
        await db.SaveChangesAsync();
        return gd.Id;
    }

    private async Task<int> MaHonPhoiKeTiep(Qlgx.Data.QlgxDbContext db) =>
        (await db.HonPhoi.MaxAsync(h => (int?)h.MaHonPhoiCu)) is { } max ? max + 1 : 1;

    private async Task<Guid> TaoHonPhoi(int ma, Guid chongId, Guid voId, DateOnly? ngay = null,
        string? soHonPhoi = null)
    {
        await using var db = app.TaoContextThuan();
        var hp = new HonPhoi
        {
            GiaoXuId = app.GiaoXuId, MaHonPhoiCu = await MaHonPhoiKeTiep(db),
            SoHonPhoi = soHonPhoi ?? ("HP" + ma), NgayHonPhoi = ngay,
        };
        db.HonPhoi.Add(hp);
        db.GiaoDanHonPhoi.AddRange(
            new GiaoDanHonPhoi { GiaoXuId = app.GiaoXuId, HonPhoi = hp, GiaoDanId = chongId, SoThuTu = 1 },
            new GiaoDanHonPhoi { GiaoXuId = app.GiaoXuId, HonPhoi = hp, GiaoDanId = voId, SoThuTu = 2 });
        await db.SaveChangesAsync();
        return hp.Id;
    }

    [Fact]
    public async Task Giao_dan_chua_co_hon_phoi_nao_thi_tra_danh_sach_rong()
    {
        var id = await TaoGiaoDan(9001, "Chua ket hon");

        var ds = await app.CreateClient()
            .GetFromJsonAsync<List<HonPhoiCuaGiaoDan>>($"/api/giao-dan/{id}/hon-phoi");

        ds.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public async Task Tra_ve_du_cac_truong_kem_ten_vo_chong()
    {
        var chongId = await TaoGiaoDan(9010, "Nguyen Van Chong", "Nam");
        var voId = await TaoGiaoDan(9011, "Tran Thi Vo", "Nu");
        var honPhoiId = await TaoHonPhoi(9010, chongId, voId, new DateOnly(2018, 5, 1), "12/2018");
        await using (var db = app.TaoContextThuan())
        {
            var hpEnt = await db.HonPhoi.SingleAsync(h => h.Id == honPhoiId);
            hpEnt.NoiHonPhoi = "Nha tho Chinh toa";
            hpEnt.LinhMucChung = "Lm. Nguyen Van A";
            hpEnt.NguoiChung1 = "Ong B";
            hpEnt.NguoiChung2 = "Ba C";
            hpEnt.CachThucHonPhoi = "Hợp pháp";
            hpEnt.GhiChu = "Khong co gi dac biet";
            await db.SaveChangesAsync();
        }

        var ds = await app.CreateClient()
            .GetFromJsonAsync<List<HonPhoiCuaGiaoDan>>($"/api/giao-dan/{chongId}/hon-phoi");

        ds.Should().ContainSingle();
        var hp = ds![0];
        hp.Should().NotBeNull();
        hp.Id.Should().Be(honPhoiId);
        hp.SoHonPhoi.Should().Be("12/2018");
        hp.NgayHonPhoi.Should().Be(new DateOnly(2018, 5, 1));
        hp.NoiHonPhoi.Should().Be("Nha tho Chinh toa");
        hp.LinhMucChung.Should().Be("Lm. Nguyen Van A");
        hp.NguoiChung1.Should().Be("Ong B");
        hp.NguoiChung2.Should().Be("Ba C");
        hp.CachThucHonPhoi.Should().Be("Hợp pháp");
        hp.GhiChu.Should().Be("Khong co gi dac biet");
        hp.VoChongId.Should().Be(voId, "xem tu phia nguoi chong thi phai tra ve thong tin nguoi vo");
        hp.TenVoChong.Should().Be("Tran Thi Vo");
        hp.RowVersion.Should().BeGreaterThan(0u);
    }

    [Fact]
    public async Task Xem_tu_phia_nguoi_vo_thi_tra_ve_ten_nguoi_chong()
    {
        var chongId = await TaoGiaoDan(9020, "Le Van Chong", "Nam");
        var voId = await TaoGiaoDan(9021, "Pham Thi Vo", "Nu");
        await TaoHonPhoi(9020, chongId, voId);

        var ds = await app.CreateClient()
            .GetFromJsonAsync<List<HonPhoiCuaGiaoDan>>($"/api/giao-dan/{voId}/hon-phoi");

        ds.Should().ContainSingle();
        ds![0].VoChongId.Should().Be(chongId);
        ds[0].TenVoChong.Should().Be("Le Van Chong");
    }

    [Fact]
    public async Task Nguoi_goa_roi_tai_hon_co_nhieu_hon_phoi_thi_tra_ve_ca_hai_sap_moi_nhat_truoc()
    {
        // Vòng sửa 1 của GiaDinhService.ChonHonPhoiHienTai đã lưu ý: một người có thể có NHIỀU
        // bản ghi hôn phối theo thời gian. Endpoint theo giáo dân khác cả hai control desktop ở
        // chỗ KHÔNG đoán "hôn phối hiện tại" mà trả về toàn bộ — xem hon-phoi.md mục 8.
        var chongId = await TaoGiaoDan(9030, "Da tung goa", "Nam");
        var voCu = await TaoGiaoDan(9031, "Vo dau (da mat)", "Nu");
        var voMoi = await TaoGiaoDan(9032, "Vo hien tai", "Nu");
        var hpCu = await TaoHonPhoi(9030, chongId, voCu, new DateOnly(2000, 1, 1), "HP-cu");
        var hpMoi = await TaoHonPhoi(9033, chongId, voMoi, new DateOnly(2020, 6, 6), "HP-moi");

        var ds = await app.CreateClient()
            .GetFromJsonAsync<List<HonPhoiCuaGiaoDan>>($"/api/giao-dan/{chongId}/hon-phoi");

        ds.Should().HaveCount(2);
        ds!.Select(x => x.Id).Should().BeEquivalentTo([hpCu, hpMoi]);
        ds[0].Id.Should().Be(hpMoi, "sap xep hon phoi moi nhat truoc theo NgayHonPhoi");
        ds[1].Id.Should().Be(hpCu);
    }

    [Fact]
    public async Task Cap_nhat_thanh_cong_khi_dung_phien_ban()
    {
        var chongId = await TaoGiaoDan(9040, "Chong sua duoc", "Nam");
        var voId = await TaoGiaoDan(9041, "Vo sua duoc", "Nu");
        await TaoHonPhoi(9040, chongId, voId);
        var client = app.CreateClient();
        var truoc = await client.GetFromJsonAsync<List<HonPhoiCuaGiaoDan>>($"/api/giao-dan/{chongId}/hon-phoi");
        var hp = truoc!.Single();

        var res = await client.PutAsJsonAsync($"/api/giao-dan/hon-phoi/{hp.Id}", new CapNhatHonPhoi(
            "So moi", new DateOnly(2019, 9, 9), "Noi moi", "Lm moi", "Chung 1 moi", "Chung 2 moi",
            "Ly thân", "Ghi chu moi", hp.RowVersion));

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        var sau = await client.GetFromJsonAsync<List<HonPhoiCuaGiaoDan>>($"/api/giao-dan/{chongId}/hon-phoi");
        var hpSau = sau!.Single();
        hpSau.SoHonPhoi.Should().Be("So moi");
        hpSau.NgayHonPhoi.Should().Be(new DateOnly(2019, 9, 9));
        hpSau.NoiHonPhoi.Should().Be("Noi moi");
        hpSau.LinhMucChung.Should().Be("Lm moi");
        hpSau.NguoiChung1.Should().Be("Chung 1 moi");
        hpSau.NguoiChung2.Should().Be("Chung 2 moi");
        hpSau.CachThucHonPhoi.Should().Be("Ly thân");
        hpSau.GhiChu.Should().Be("Ghi chu moi");
        hpSau.RowVersion.Should().BeGreaterThan(hp.RowVersion);
    }

    [Fact]
    public async Task Hai_nguoi_cung_sua_thi_nguoi_sau_nhan_409_voi_thong_bao_nhac_hon_phoi()
    {
        var chongId = await TaoGiaoDan(9050, "Chong 409", "Nam");
        var voId = await TaoGiaoDan(9051, "Vo 409", "Nu");
        await TaoHonPhoi(9050, chongId, voId);
        var client = app.CreateClient();
        var banA = (await client.GetFromJsonAsync<List<HonPhoiCuaGiaoDan>>($"/api/giao-dan/{chongId}/hon-phoi"))!.Single();
        var banB = (await client.GetFromJsonAsync<List<HonPhoiCuaGiaoDan>>($"/api/giao-dan/{chongId}/hon-phoi"))!.Single();

        var luuA = await client.PutAsJsonAsync($"/api/giao-dan/hon-phoi/{banA.Id}",
            new CapNhatHonPhoi("Nguoi A sua", null, null, null, null, null, null, null, banA.RowVersion));
        var luuB = await client.PutAsJsonAsync($"/api/giao-dan/hon-phoi/{banB.Id}",
            new CapNhatHonPhoi("Nguoi B sua", null, null, null, null, null, null, null, banB.RowVersion));

        luuA.StatusCode.Should().Be(HttpStatusCode.OK);
        luuB.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var loi = await luuB.Content.ReadFromJsonAsync<ThongBaoLoi>();
        loi!.ThongBao.Should().Contain("hôn phối");
    }

    [Fact]
    public async Task Sua_hon_phoi_khong_ton_tai_thi_tra_404()
    {
        var res = await app.CreateClient().PutAsJsonAsync($"/api/giao-dan/hon-phoi/{Guid.NewGuid()}",
            new CapNhatHonPhoi(null, null, null, null, null, null, null, null, 0));

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
