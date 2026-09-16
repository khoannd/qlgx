using FluentAssertions;
using Qlgx.Domain;
using Qlgx.Domain.Entities;

namespace Qlgx.Data.Tests;

public class ThucTheTests
{
    [Fact]
    public void Thuc_the_moi_tu_sinh_khoa_chinh_uuid()
    {
        var a = new GiaDinh();
        var b = new GiaDinh();

        a.Id.Should().NotBe(Guid.Empty);
        a.Id.Should().NotBe(b.Id, "hai giao xu doc lap phai sinh duoc khoa khong dung nhau");
    }

    [Fact]
    public void Vai_tro_gia_dinh_dung_dung_ba_gia_tri_cua_ban_desktop()
    {
        ((int)VaiTroGiaDinh.Chong).Should().Be(0);
        ((int)VaiTroGiaDinh.Vo).Should().Be(1);
        ((int)VaiTroGiaDinh.Con).Should().Be(2);
        Enum.GetValues<VaiTroGiaDinh>().Should().HaveCount(3);
    }

    [Fact]
    public void Giao_dan_giu_du_cac_truong_bi_tich_cua_ban_desktop()
    {
        var gd = new GiaoDan
        {
            HoTen = "Tran Van Binh",
            TenThanh = "Giuse",
            Phai = "Nam",
            NgayRuaToi = new DateOnly(1972, 5, 20),
            NgayRuocLe = new DateOnly(1980, 6, 12),
            NgayThemSuc = new DateOnly(1986, 10, 18),
            NgayXucDau = null
        };

        gd.NgayRuaToi.Should().Be(new DateOnly(1972, 5, 20));
        gd.NgayRuocLe.Should().NotBeNull();
        gd.NgayThemSuc.Should().NotBeNull();
        gd.NgayXucDau.Should().BeNull();
    }
}
