using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Qlgx.Api;
using Qlgx.Domain.Entities;

namespace Qlgx.Api.Tests;

public class TaoTaiKhoanQuanTriTests(QlgxApiFactory factory) : IClassFixture<QlgxApiFactory>
{
    /// <summary>N5 — GUID gõ sai phải cho thông báo tiếng Việt nói rõ phải làm gì, không phải
    /// câu thô của .NET ("Guid should contain 32 digits with 4 dashes..."). Người đọc thông báo
    /// này là người đang cài máy chủ cho một giáo xứ, không phải lập trình viên.</summary>
    [Fact]
    public async Task Guid_giao_xu_go_sai_thi_bao_loi_bang_tieng_Viet()
    {
        await using var db = factory.TaoContextThuan();

        var hanhDong = async () => await TaoTaiKhoanQuanTri.LayHoacTaoGiaoXu(
            db, "khong-phai-guid", null, taoNeuChuaCo: false);

        (await hanhDong.Should().ThrowAsync<InvalidOperationException>())
            .Which.Message.Should().Contain("QLGX_ADMIN_GIAO_XU_ID")
            .And.Contain("QLGX_ADMIN_GIAO_XU_TEN",
                "thong bao phai chi duong thoat, khong chi bao la sai");
    }

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

    [Fact]
    public async Task Dong_thoi_hai_lan_voi_cung_ten_thi_cung_tra_ve_mot_id_va_chi_co_mot_giao_xu()
    {
        // Idempotent duoi dong thoi: hai tien trinh chay cung luc voi database connections
        // rieng — advisory lock phai bao ve tranh tao trung.
        var ten = "Giao xu Dong thoi " + Guid.NewGuid().ToString("N")[..8];

        // Goi hai lan song song, moi lan voi context rieng.
        var id1Task = TaoTaiKhoanQuanTri.LayHoacTaoGiaoXu(
            factory.TaoContextThuan(), null, ten, taoNeuChuaCo: true);
        var id2Task = TaoTaiKhoanQuanTri.LayHoacTaoGiaoXu(
            factory.TaoContextThuan(), null, ten, taoNeuChuaCo: true);

        var (id1, id2) = await Task.WhenAll(id1Task, id2Task).ContinueWith(
            t => (t.Result[0], t.Result[1]));

        // Phai la cung mot giao xu.
        id2.Should().Be(id1);

        // Va bang chi co dung mot dong voi ten nay.
        await using var dbKiemTra = factory.TaoContextThuan();
        (await dbKiemTra.GiaoXu.CountAsync(g => g.TenGiaoXu == ten)).Should().Be(1);
    }
}
