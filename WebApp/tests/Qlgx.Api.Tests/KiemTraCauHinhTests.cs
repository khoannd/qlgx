using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Qlgx.Api;

namespace Qlgx.Api.Tests;

public class KiemTraCauHinhTests
{
    private static IConfiguration CauHinh(string? nghiepVu, string? quanTri)
    {
        var cap = new Dictionary<string, string?> { ["ConnectionStrings:Qlgx"] = nghiepVu };
        if (quanTri is not null) cap["ConnectionStrings:QlgxQuanTri"] = quanTri;
        return new ConfigurationBuilder().AddInMemoryCollection(cap).Build();
    }

    private const string KetNoiApp = "Host=postgres;Database=qlgx;Username=qlgx_app;Password=a";
    private const string KetNoiQuanTri = "Host=postgres;Database=qlgx;Username=qlgx_admin;Password=b";

    [Fact]
    public void Ngoai_san_xuat_khong_bao_loi_du_thieu_chuoi_quan_tri()
    {
        KiemTraCauHinh.LoiCauHinhSanXuat(CauHinh(KetNoiApp, null), laSanXuat: false)
            .Should().BeNull();
    }

    [Fact]
    public void San_xuat_thieu_chuoi_quan_tri_thi_bao_loi()
    {
        KiemTraCauHinh.LoiCauHinhSanXuat(CauHinh(KetNoiApp, null), laSanXuat: true)
            .Should().Contain("QlgxQuanTri");
    }

    [Fact]
    public void San_xuat_hai_chuoi_cung_vai_tro_thi_bao_loi()
    {
        KiemTraCauHinh.LoiCauHinhSanXuat(CauHinh(KetNoiApp, KetNoiApp), laSanXuat: true)
            .Should().Contain("cùng một vai trò");
    }

    [Fact]
    public void San_xuat_hai_vai_tro_khac_nhau_thi_hop_le()
    {
        KiemTraCauHinh.LoiCauHinhSanXuat(CauHinh(KetNoiApp, KetNoiQuanTri), laSanXuat: true)
            .Should().BeNull();
    }

    [Fact]
    public void So_sanh_theo_vai_tro_chu_khong_theo_chuoi_tho()
    {
        // Cùng vai trò qlgx_app nhưng khác thứ tự tham số/khoảng trắng — vẫn phải bị chặn.
        var quanTri = "Username=qlgx_app;Host=postgres;Password=a;Database=qlgx";
        KiemTraCauHinh.LoiCauHinhSanXuat(CauHinh(KetNoiApp, quanTri), laSanXuat: true)
            .Should().Contain("cùng một vai trò");
    }
}
