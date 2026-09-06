using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Qlgx.Data;
using Qlgx.Domain.Entities;

namespace Qlgx.Data.Tests;

public class LocTheoGiaoXuTests(CoSoDuLieuFixture db) : IClassFixture<CoSoDuLieuFixture>
{
    private sealed record BoiCanh(Guid GiaoXuId) : IBoiCanhGiaoXu;

    [Fact]
    public async Task Truy_van_chi_thay_du_lieu_cua_giao_xu_hien_hanh()
    {
        var giaoXuKhac = Guid.NewGuid();
        await using (var ctx = db.TaoContext())
        {
            ctx.GiaoXu.Add(new GiaoXu { Id = giaoXuKhac, TenGiaoXu = "Giao xu khac", MaGiaoXuCu = 2 });
            ctx.GiaoDan.AddRange(
                new GiaoDan { GiaoXuId = db.GiaoXuId, MaGiaoDanCu = 7001, HoTen = "Nguoi cua xu minh" },
                new GiaoDan { GiaoXuId = giaoXuKhac, MaGiaoDanCu = 7002, HoTen = "Nguoi cua xu khac" });
            await ctx.SaveChangesAsync();
        }

        await using var ctxLoc = new QlgxDbContext(
            new DbContextOptionsBuilder<QlgxDbContext>().UseNpgsql(db.ChuoiKetNoi).Options,
            new BoiCanh(db.GiaoXuId));

        var danhSach = await ctxLoc.GiaoDan.Select(x => x.HoTen).ToListAsync();

        danhSach.Should().Contain("Nguoi cua xu minh");
        danhSach.Should().NotContain("Nguoi cua xu khac");
    }

    [Fact]
    public async Task Khong_co_boi_canh_thi_thay_toan_bo_du_lieu()
    {
        await using var ctx = db.TaoContext();

        var soGiaoXu = await ctx.GiaoDan.Select(x => x.GiaoXuId).Distinct().CountAsync();

        soGiaoXu.Should().BeGreaterThan(0, "cong cu chuyen doi du lieu can ghi cho nhieu giao xu");
    }

    /// <summary>
    /// Ranh gioi bao mat cua mo hinh nhieu giao xu dung chung mot database: bat ky thuc the
    /// nao co cot GiaoXuId ma quen gan HasQueryFilter se lam ro ri du lieu giua cac giao xu.
    /// Test nay khong phu thuoc danh sach cung — no duyet toan bo model, nen tu dong bat loi
    /// khi ai do them bang moi ma quen gan bo loc.
    /// </summary>
    [Fact]
    public void Moi_thuc_the_co_cot_GiaoXuId_deu_da_duoc_gan_bo_loc()
    {
        using var ctx = db.TaoContext();

        var thieuBoLoc = ctx.Model.GetEntityTypes()
            .Where(t => t.FindProperty("GiaoXuId") is not null)
            .Where(t => t.GetDeclaredQueryFilters().Count == 0)
            .Select(t => t.ClrType.Name)
            .ToList();

        thieuBoLoc.Should().BeEmpty(
            "moi bang co cot GiaoXuId phai duoc loc theo giao xu hien hanh, neu khong se ro ri du lieu giua cac giao xu");
    }
}
