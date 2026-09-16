using System.Security.Cryptography;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Qlgx.Api;

namespace Qlgx.Api.Tests;

public class KiemTraCauHinhTests
{
    /// <summary>Khoá ký hợp lệ SINH NGẪU NHIÊN mỗi lần chạy — không ghi khoá cố định nào vào mã
    /// nguồn, kể cả trong test (xem QlgxApiFactory).</summary>
    private static string KhoaKyHopLe() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

    private static IConfiguration CauHinh(string? nghiepVu, string? quanTri, string? khoaKy = null)
    {
        var cap = new Dictionary<string, string?>
        {
            ["ConnectionStrings:Qlgx"] = nghiepVu,
            // TB-8: cổng gác nay kiểm cả khoá ký JWT, nên mọi ca kiểm phần chuỗi kết nối phải
            // mang sẵn một khoá hợp lệ, nếu không chúng sẽ báo đúng nhưng vì lý do sai.
            ["Qlgx:JwtKey"] = khoaKy ?? KhoaKyHopLe(),
        };
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

    // --- TB-8 (review-bao-mat.md): cổng gác sản xuất phải bắt buộc Qlgx:JwtKey ---------------
    // Thiếu khoá thì máy chủ vẫn khởi động, readiness trả 200, install.sh kết luận "đã lên được,
    // không quay lui" — nhưng MỌI lần đăng nhập đều 500. Đúng kiểu hỏng mà lớp kiểm tra này sinh
    // ra để chặn.

    [Fact]
    public void San_xuat_thieu_khoa_ky_jwt_thi_bao_loi()
    {
        var cauHinh = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:Qlgx"] = KetNoiApp,
            ["ConnectionStrings:QlgxQuanTri"] = KetNoiQuanTri,
        }).Build();

        KiemTraCauHinh.LoiCauHinhSanXuat(cauHinh, laSanXuat: true)
            .Should().Contain("JwtKey");
    }

    [Fact]
    public void San_xuat_khoa_ky_jwt_khong_phai_base64_thi_bao_loi()
    {
        KiemTraCauHinh.LoiCauHinhSanXuat(
                CauHinh(KetNoiApp, KetNoiQuanTri, khoaKy: "khong-phai-base64!!!"), laSanXuat: true)
            .Should().Contain("base64");
    }

    [Fact]
    public void San_xuat_khoa_ky_jwt_ngan_hon_32_byte_thi_bao_loi()
    {
        // 16 byte vẫn là base64 hợp lệ và TokenService vẫn ký được — nên đây đúng là ca mà chỉ
        // kiểm "có/không" hoặc "giải được base64 không" sẽ bỏ lọt.
        var khoaNgan = Convert.ToBase64String(RandomNumberGenerator.GetBytes(16));

        KiemTraCauHinh.LoiCauHinhSanXuat(CauHinh(KetNoiApp, KetNoiQuanTri, khoaNgan), laSanXuat: true)
            .Should().Contain("32 byte");
    }

    [Fact]
    public void Ngoai_san_xuat_thi_khong_doi_hoi_khoa_ky_jwt()
    {
        // Nhiều bài test dựng WebApplicationFactory trần chỉ để gọi /api/suc-khoe và không cấu
        // hình khoá ký nào — cổng gác chỉ được siết ở Production, đúng như hai lớp kiểm tra cũ.
        var cauHinh = new ConfigurationBuilder().AddInMemoryCollection(
            new Dictionary<string, string?> { ["ConnectionStrings:Qlgx"] = KetNoiApp }).Build();

        KiemTraCauHinh.LoiCauHinhSanXuat(cauHinh, laSanXuat: false).Should().BeNull();
    }
}
