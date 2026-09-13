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

    // Mật khẩu sinh ngẫu nhiên tại chỗ — không bao giờ ghi mật khẩu (kể cả giá trị mẫu) vào mã
    // nguồn. Nội dung không quan trọng vì test chỉ kiểm việc so sánh theo TÊN VAI TRÒ (Username).
    private static readonly string KetNoiApp =
        $"Host=postgres;Database=qlgx;Username=qlgx_app;Password={Guid.NewGuid():N}";
    private static readonly string KetNoiQuanTri =
        $"Host=postgres;Database=qlgx;Username=qlgx_admin;Password={Guid.NewGuid():N}";

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
        var quanTri = $"Username=qlgx_app;Host=postgres;Password={Guid.NewGuid():N};Database=qlgx";
        KiemTraCauHinh.LoiCauHinhSanXuat(CauHinh(KetNoiApp, quanTri), laSanXuat: true)
            .Should().Contain("cùng một vai trò");
    }

    [Fact]
    public void Chuoi_ket_noi_di_dang_khong_lam_sup_do_ma_van_bi_coi_la_trung_vai_tro()
    {
        // NpgsqlConnectionStringBuilder ném nhiều loại ngoại lệ khác nhau tuỳ kiểu dị dạng
        // (ví dụ "===" ném KeyNotFoundException, không phải ArgumentException) — việc kiểm tra
        // cấu hình TUYỆT ĐỐI không được tự nó crash vì một chuỗi kết nối gõ sai. Hai chuỗi dị
        // dạng giống hệt nhau vẫn phải bị coi là trùng vai trò (fallback trả về chính chuỗi đó
        // để so sánh), thay vì lọt lưới hay ném ngoại lệ ra ngoài.
        const string diDang = "===";
        var goi = () => KiemTraCauHinh.LoiCauHinhSanXuat(CauHinh(diDang, diDang), laSanXuat: true);
        goi.Should().NotThrow()
            .Which.Should().Contain("cùng một vai trò");
    }
}
