using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Qlgx.Api;
using Qlgx.Domain.Entities;

namespace Qlgx.Api.Tests;

public class TaoTaiKhoanQuanTriTests(QlgxApiFactory factory) : IClassFixture<QlgxApiFactory>
{
    [Fact]
    public async Task Khong_bat_co_tao_va_khong_tim_thay_thi_bao_loi()
    {
        await using var db = factory.TaoContextThuan();

        var hanhDong = async () => await TaoTaiKhoanQuanTri.LayHoacTaoGiaoXu(
            db, null, "Giao xu khong ton tai ZZZ", taoNeuChuaCo: false);

        await hanhDong.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*khong ton tai ZZZ*");
    }

    [Fact]
    public async Task Bat_co_tao_thi_tao_moi_va_tra_ve_id()
    {
        await using var db = factory.TaoContextThuan();
        var ten = "Giao xu Moi " + Guid.NewGuid().ToString("N")[..8];

        var id = await TaoTaiKhoanQuanTri.LayHoacTaoGiaoXu(db, null, ten, taoNeuChuaCo: true);

        var daTao = await db.GiaoXu.SingleAsync(g => g.Id == id);
        daTao.TenGiaoXu.Should().Be(ten);
        daTao.MaGiaoXuCu.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Chay_lai_voi_cung_ten_thi_dung_lai_giao_xu_cu_khong_tao_trung()
    {
        await using var db = factory.TaoContextThuan();
        var ten = "Giao xu Lap " + Guid.NewGuid().ToString("N")[..8];

        var lan1 = await TaoTaiKhoanQuanTri.LayHoacTaoGiaoXu(db, null, ten, taoNeuChuaCo: true);
        var lan2 = await TaoTaiKhoanQuanTri.LayHoacTaoGiaoXu(db, null, ten, taoNeuChuaCo: true);

        lan2.Should().Be(lan1);
        (await db.GiaoXu.CountAsync(g => g.TenGiaoXu == ten)).Should().Be(1);
    }

    [Fact]
    public async Task Ma_giao_xu_cu_luon_lon_hon_moi_ma_da_co()
    {
        await using var db = factory.TaoContextThuan();
        var maLonNhatTruoc = await db.GiaoXu.MaxAsync(g => (int?)g.MaGiaoXuCu) ?? 0;

        var id = await TaoTaiKhoanQuanTri.LayHoacTaoGiaoXu(
            db, null, "Giao xu Ma " + Guid.NewGuid().ToString("N")[..8], taoNeuChuaCo: true);

        (await db.GiaoXu.SingleAsync(g => g.Id == id)).MaGiaoXuCu
            .Should().BeGreaterThan(maLonNhatTruoc);
    }
}
