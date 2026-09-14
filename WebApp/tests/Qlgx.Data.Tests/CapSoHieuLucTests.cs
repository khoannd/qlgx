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
        // LayDaiSo bat buoc chay trong giao dich (xem CapSoHieuLuc.cs) - ngoai giao dich thi
        // FOR UPDATE nha khoa ngay cuoi cau lenh, khong con bao ve gi ca.
        await using var ctx = db.TaoContext();
        await using var gd = await ctx.Database.BeginTransactionAsync();

        var (dau1, epoch1, _) = await CapSoHieuLuc.LayDaiSo(ctx, db.GiaoXuId, 3, default);
        var (dau2, epoch2, _) = await CapSoHieuLuc.LayDaiSo(ctx, db.GiaoXuId, 2, default);

        dau2.Should().Be(dau1 + 3, "dai so phai lien tiep, khong chong lan va khong bo trong");
        epoch2.Should().Be(epoch1);
    }

    [Fact]
    public async Task Giao_dich_bi_huy_khong_de_lai_lo_hong()
    {
        // Goi truoc mot lan (trong giao dich rieng, commit ngay) de bao dam dong dem da ton
        // tai (INSERT ... ON CONFLICT DO NOTHING ben trong LayDaiSo) - xUnit khong bao dam thu
        // tu chay test trong cung mot lop, nen khong the doc thang dong dem truoc khi chac
        // chan no da duoc tao.
        long truoc;
        await using (var ctx = db.TaoContext())
        await using (var gdMoi = await ctx.Database.BeginTransactionAsync())
        {
            await CapSoHieuLuc.LayDaiSo(ctx, db.GiaoXuId, 1, default);
            await gdMoi.CommitAsync();
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
        await using var gdSau = await sau.Database.BeginTransactionAsync();
        var (dau, _, _) = await CapSoHieuLuc.LayDaiSo(sau, db.GiaoXuId, 1, default);
        dau.Should().Be(truoc, "giao dich bi huy thi so KHONG duoc tieu ton");
        await gdSau.CommitAsync();
    }

    [Fact]
    public async Task Khong_mo_giao_dich_thi_bi_chan()
    {
        await using var ctx = db.TaoContext();
        var lamSai = async () => await CapSoHieuLuc.LayDaiSo(ctx, db.GiaoXuId, 1, default);

        await lamSai.Should().ThrowAsync<InvalidOperationException>(
            "goi LayDaiSo ngoai giao dich la dung cach gay trung so duoi tai, phai chan ngay");
    }

    [Fact]
    public async Task Khoa_dong_dem_giu_toi_luc_commit_khong_phai_toi_luc_doc()
    {
        // Day chinh la tinh chat khien du an bo sequence Postgres: thu tu CAP so phai trung
        // thu tu COMMIT, khong phai thu tu doc. Neu mot ban hong nao do cap so o giao dich
        // rieng roi commit ngay (con giao dich chinh con dang lam viec khac), giao dich B se
        // KHONG bi chan boi giao dich A nua - test nay bat truc tiep su chan do bang FOR UPDATE.
        await using var ctxA = db.TaoContext();
        await using var gdA = await ctxA.Database.BeginTransactionAsync();
        var (soA, _, _) = await CapSoHieuLuc.LayDaiSo(ctxA, db.GiaoXuId, 1, default);
        // CO Y chua commit A - giu khoa dong dem.

        await using var ctxB = db.TaoContext();
        await using var gdB = await ctxB.Database.BeginTransactionAsync();
        var tacVuB = CapSoHieuLuc.LayDaiSo(ctxB, db.GiaoXuId, 1, default);

        var xongTruoc500ms = await Task.WhenAny(tacVuB, Task.Delay(TimeSpan.FromMilliseconds(500)));
        xongTruoc500ms.Should().NotBe(tacVuB,
            "B phai bi FOR UPDATE chan lai vi A chua commit - neu B xong ngay la khoa khong con giu toi luc commit");

        await gdA.CommitAsync();

        var (soB, _, _) = await tacVuB.WaitAsync(TimeSpan.FromSeconds(5));
        soB.Should().Be(soA + 1, "B chi duoc cap so sau khi A commit, va phai la so ke tiep");
        await gdB.CommitAsync();
    }

    [Fact]
    public async Task Nhieu_tien_trinh_cap_so_dong_thoi_khong_trung_khong_ho()
    {
        const int soLuong = 20;
        var ketQua = await Task.WhenAll(Enumerable.Range(0, soLuong).Select(async _ =>
        {
            await using var ctx = db.TaoContext();
            await using var gd = await ctx.Database.BeginTransactionAsync();
            var (dau, _, _) = await CapSoHieuLuc.LayDaiSo(ctx, db.GiaoXuId, 1, default);
            await gd.CommitAsync();
            return dau;
        }));

        ketQua.Distinct().Should().HaveCount(soLuong, "khong duoc cap trung so");
        (ketQua.Max() - ketQua.Min()).Should().Be(soLuong - 1, "khong duoc bo trong so nao");
    }
}
