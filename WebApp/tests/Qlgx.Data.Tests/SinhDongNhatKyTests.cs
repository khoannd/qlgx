using FluentAssertions;
using Qlgx.Data.NhatKy;
using Qlgx.Domain.Entities;

namespace Qlgx.Data.Tests;

public class SinhDongNhatKyTests(CoSoDuLieuFixture db) : IClassFixture<CoSoDuLieuFixture>
{
    [Fact]
    public async Task Them_ban_ghi_sinh_dung_mot_dong_loai_tao()
    {
        await using var ctx = db.TaoContext();
        ctx.GiaoDan.Add(new GiaoDan { GiaoXuId = db.GiaoXuId, MaGiaoDanCu = 9101, HoTen = "Nguyen Van A" });

        var dong = SinhDongNhatKy.Tu(ctx.ChangeTracker, null, DateTimeOffset.UtcNow, Guid.NewGuid());

        dong.Should().ContainSingle();
        dong[0].Loai.Should().Be("tao");
        dong[0].Bang.Should().Be("GiaoDan");
        dong[0].Truong.Should().BeEmpty();
        dong[0].GiaTri.Should().Contain("Nguyen Van A");
    }

    [Fact]
    public async Task Sua_hai_o_sinh_dung_hai_dong_moi_dong_mot_o()
    {
        Guid id;
        await using (var ctx = db.TaoContext())
        {
            var g = new GiaoDan { GiaoXuId = db.GiaoXuId, MaGiaoDanCu = 9102, HoTen = "Cu" };
            ctx.GiaoDan.Add(g);
            await ctx.SaveChangesAsync();
            id = g.Id;
        }

        await using var ctx2 = db.TaoContext();
        var gd = await ctx2.GiaoDan.FindAsync(id);
        gd!.HoTen = "Moi";
        gd.DienThoai = "0900000000";

        var dong = SinhDongNhatKy.Tu(ctx2.ChangeTracker, null, DateTimeOffset.UtcNow, Guid.NewGuid());

        dong.Should().HaveCount(2);
        dong.Select(d => d.Truong).Should().BeEquivalentTo(["HoTen", "DienThoai"]);
        dong.Should().OnlyContain(d => d.Loai == "sua" && d.BanGhiId == id);
    }

    [Fact]
    public async Task Tao_giao_dan_co_anh_thi_jsonb_khong_chua_anh()
    {
        await using var ctx = db.TaoContext();
        ctx.GiaoDan.Add(new GiaoDan
        {
            GiaoXuId = db.GiaoXuId, MaGiaoDanCu = 9105, HoTen = "Co anh luc tao",
            AnhDaiDienDuLieu = new byte[4096], AnhDaiDienLoaiNoiDung = "image/png",
        });

        var dong = SinhDongNhatKy.Tu(ctx.ChangeTracker, null, DateTimeOffset.UtcNow, Guid.NewGuid());

        // Khac voi test "sua" ben duoi: nhanh "tao" gop CA BAN GHI vao mot jsonb (xem
        // SinhDongNhatKy.Tu), nen neu ai do sau nay sua nhanh nay cho gon (vi du doi sang
        // muc.CurrentValues.ToObject()) ma quen loai anh, anh base64 se lot vao ma test "sua"
        // khong bat duoc — phai co test rieng cho nhanh nay.
        dong.Should().ContainSingle();
        dong[0].GiaTri.Should().NotContain("AnhDaiDienDuLieu",
            "nhanh tao gop ca ban ghi vao jsonb — neu khong loai truoc, anh se nam trong do");
        dong[0].GiaTri!.Length.Should().BeLessThan(2000,
            "jsonb cua dong tao khong duoc phinh len vi anh base64 (anh that la 4096 byte)");
    }

    [Fact]
    public async Task Tao_gia_dinh_co_anh_thi_jsonb_khong_chua_anh()
    {
        await using var ctx = db.TaoContext();
        ctx.GiaDinh.Add(new GiaDinh
        {
            GiaoXuId = db.GiaoXuId, TenGiaDinh = "Gia dinh co anh",
            AnhDaiDienDuLieu = new byte[4096], AnhDaiDienLoaiNoiDung = "image/png",
        });

        var dong = SinhDongNhatKy.Tu(ctx.ChangeTracker, null, DateTimeOffset.UtcNow, Guid.NewGuid());

        dong.Should().ContainSingle();
        dong[0].GiaTri.Should().NotContain("AnhDaiDienDuLieu");
        dong[0].GiaTri!.Length.Should().BeLessThan(2000,
            "jsonb cua dong tao khong duoc phinh len vi anh base64 (anh that la 4096 byte)");
    }

    [Fact]
    public async Task Anh_dai_dien_khong_bao_gio_vao_nhat_ky()
    {
        Guid id;
        await using (var ctx = db.TaoContext())
        {
            var g = new GiaoDan { GiaoXuId = db.GiaoXuId, MaGiaoDanCu = 9103, HoTen = "Co anh" };
            ctx.GiaoDan.Add(g);
            await ctx.SaveChangesAsync();
            id = g.Id;
        }

        await using var ctx2 = db.TaoContext();
        var gd = await ctx2.GiaoDan.FindAsync(id);
        gd!.AnhDaiDienDuLieu = new byte[4096];
        gd.AnhDaiDienLoaiNoiDung = "image/png";

        var dong = SinhDongNhatKy.Tu(ctx2.ChangeTracker, null, DateTimeOffset.UtcNow, Guid.NewGuid());

        dong.Should().BeEmpty("anh la byte[] trong bang, dua vao jsonb se phinh nhat ky");
    }

    [Fact]
    public async Task Dat_o_ve_rong_van_sinh_dong_voi_gia_tri_null()
    {
        Guid id;
        await using (var ctx = db.TaoContext())
        {
            var g = new GiaoDan
            {
                GiaoXuId = db.GiaoXuId, MaGiaoDanCu = 9104, HoTen = "Co dien thoai",
                DienThoai = "0911111111",
            };
            ctx.GiaoDan.Add(g);
            await ctx.SaveChangesAsync();
            id = g.Id;
        }

        await using var ctx2 = db.TaoContext();
        var gd = await ctx2.GiaoDan.FindAsync(id);
        gd!.DienThoai = null;

        var dong = SinhDongNhatKy.Tu(ctx2.ChangeTracker, null, DateTimeOffset.UtcNow, Guid.NewGuid());

        dong.Should().ContainSingle();
        dong[0].Truong.Should().Be("DienThoai");
        dong[0].GiaTri.Should().BeNull(
            "o rong la MOT GIA TRI hop le, khong phai 'khong co gi' — thieu dong nay thi gia " +
            "tri nhap nham khong bao gio xoa duoc, no se song lai o lan gop sau");
    }
}
