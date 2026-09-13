using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Qlgx.Data;
using Qlgx.Data.NhatKy;

namespace Qlgx.Data.Tests;

public class CapSoHieuLucTests(CoSoDuLieuFixture db) : IClassFixture<CoSoDuLieuFixture>
{
    [Fact]
    public async Task Cap_so_lien_tuc_tang_dan()
    {
        await using var ctx = db.TaoContext();

        var (dau1, epoch1) = await CapSoHieuLuc.LayDaiSo(ctx, db.GiaoXuId, 3, default);
        var (dau2, epoch2) = await CapSoHieuLuc.LayDaiSo(ctx, db.GiaoXuId, 2, default);

        dau2.Should().Be(dau1 + 3, "dai so phai lien tiep, khong chong lan va khong bo trong");
        epoch2.Should().Be(epoch1);
    }

    [Fact]
    public async Task Giao_dich_bi_huy_khong_de_lai_lo_hong()
    {
        // Goi truoc mot lan de bao dam dong dem da ton tai (INSERT ... ON CONFLICT DO NOTHING
        // ben trong LayDaiSo) - xUnit khong bao dam thu tu chay test trong cung mot lop, nen
        // khong the doc thang dong dem truoc khi chac chan no da duoc tao.
        long truoc;
        await using (var ctx = db.TaoContext())
        {
            await CapSoHieuLuc.LayDaiSo(ctx, db.GiaoXuId, 1, default);
            truoc = (await ctx.Set<Domain.Entities.BoDemHieuLuc>()
                .SingleAsync(x => x.GiaoXuId == db.GiaoXuId)).SoTiepTheo;
        }

        await using (var ctx = db.TaoContext())
        await using (var gd = await ctx.Database.BeginTransactionAsync())
        {
            await CapSoHieuLuc.LayDaiSo(ctx, db.GiaoXuId, 5, default);
            await gd.RollbackAsync();
        }

        await using var sau = db.TaoContext();
        var (dau, _) = await CapSoHieuLuc.LayDaiSo(sau, db.GiaoXuId, 1, default);
        dau.Should().Be(truoc, "giao dich bi huy thi so KHONG duoc tieu ton");
    }

    [Fact]
    public async Task Nhieu_tien_trinh_cap_so_dong_thoi_khong_trung_khong_ho()
    {
        const int soLuong = 20;
        var ketQua = await Task.WhenAll(Enumerable.Range(0, soLuong).Select(async _ =>
        {
            await using var ctx = db.TaoContext();
            await using var gd = await ctx.Database.BeginTransactionAsync();
            var (dau, _) = await CapSoHieuLuc.LayDaiSo(ctx, db.GiaoXuId, 1, default);
            await gd.CommitAsync();
            return dau;
        }));

        ketQua.Distinct().Should().HaveCount(soLuong, "khong duoc cap trung so");
        (ketQua.Max() - ketQua.Min()).Should().Be(soLuong - 1, "khong duoc bo trong so nao");
    }
}
