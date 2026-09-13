using FluentAssertions;
using Qlgx.Data.DongBo;

namespace Qlgx.Data.Tests;

public class DongHoLaiTests
{
    private static readonly DateTimeOffset Moc = new(2026, 9, 13, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Moc_vat_ly_moi_hon_thi_thang()
    {
        var cu = new DauDongHo(Moc, 0, null, Guid.NewGuid());
        var moi = new DauDongHo(Moc.AddSeconds(1), 0, null, Guid.NewGuid());

        DongHoLai.SoSanh(moi, cu).Should().BePositive();
    }

    [Fact]
    public void Cung_moc_vat_ly_thi_dong_ho_logic_pha_hoa()
    {
        var a = new DauDongHo(Moc, 5, null, Guid.NewGuid());
        var b = new DauDongHo(Moc, 7, null, Guid.NewGuid());

        DongHoLai.SoSanh(b, a).Should().BePositive();
    }

    [Fact]
    public void Cung_moc_va_cung_logic_thi_thiet_bi_roi_ma_thao_tac_pha_hoa_tat_dinh()
    {
        var thietBiNho = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var thietBiLon = Guid.Parse("00000000-0000-0000-0000-000000000002");
        var a = new DauDongHo(Moc, 0, thietBiNho, Guid.NewGuid());
        var b = new DauDongHo(Moc, 0, thietBiLon, Guid.NewGuid());

        // Không quan trọng bên nào thắng — quan trọng là MỌI máy đều kết luận GIỐNG NHAU và
        // kết quả không đổi giữa hai lần so.
        var lan1 = DongHoLai.SoSanh(a, b);
        var lan2 = DongHoLai.SoSanh(a, b);
        lan1.Should().Be(lan2);
        lan1.Should().NotBe(0, "hai dau dong ho khac nhau khong duoc coi la bang nhau");
    }

    [Fact]
    public void Hai_dau_giong_het_nhau_thi_bang_nhau()
    {
        var ma = Guid.NewGuid();
        var tb = Guid.NewGuid();
        DongHoLai.SoSanh(new DauDongHo(Moc, 3, tb, ma), new DauDongHo(Moc, 3, tb, ma)).Should().Be(0);
    }

    [Fact]
    public void Hieu_chinh_dich_moc_may_con_ve_gio_may_chu()
    {
        // Máy con chạy nhanh 3 ngày. Một thao tác nó ghi lúc "16/9 10:00" theo đồng hồ của nó
        // thực ra xảy ra lúc 13/9 10:00 theo giờ máy chủ.
        var gioMayCon = Moc.AddDays(3);
        var doLech = DongHoLai.TinhDoLech(gioMayCon, Moc);

        DongHoLai.HieuChinh(gioMayCon, doLech).Should().BeCloseTo(Moc, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Nang_logic_khi_nhan_moc_bang_hoac_lon_hon_gio_hien_tai()
    {
        // Nhận một dấu có mốc vật lý ở tương lai gần so với đồng hồ máy chủ: phải nâng đồng hồ
        // logic lên để lần ghi kế tiếp của chính máy chủ xếp SAU dấu vừa nhận, không hoà.
        var nhan = new DauDongHo(Moc.AddSeconds(5), 4, null, Guid.NewGuid());

        DongHoLai.NangLogic(logicDangGiu: 0, nhanDuoc: nhan, gioHienTai: Moc)
            .Should().BeGreaterThan(4);
    }

    [Fact]
    public void Gio_hien_tai_da_vuot_qua_moc_nhan_duoc_thi_logic_ve_khong()
    {
        var nhan = new DauDongHo(Moc, 9, null, Guid.NewGuid());

        DongHoLai.NangLogic(logicDangGiu: 9, nhanDuoc: nhan, gioHienTai: Moc.AddMinutes(1))
            .Should().Be(0, "dong ho vat ly da di toi truoc, khong can dem logic nua");
    }
}
