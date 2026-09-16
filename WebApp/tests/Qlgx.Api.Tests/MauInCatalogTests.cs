using System.Reflection;
using System.Text.RegularExpressions;
using FluentAssertions;
using Qlgx.Api.Printing;

namespace Qlgx.Api.Tests;

/// <summary>
/// Đối chiếu TỰ ĐỘNG danh mục MauInCatalog (viết tay, đọc hiểu InAnService.cs) với các chỗ
/// trống <c>{{Key}}</c> THẬT SỰ có trong 13 tệp PrintTemplates/Chung/*.html — đúng yêu cầu
/// "phải chính xác, đừng đoán" của quan-ly-mau-in.md: một danh mục viết tay rất dễ gõ nhầm tên
/// Key hoặc bỏ sót, bài test này duyệt lại đúng những gì assembly nhúng thật để bắt lỗi đó,
/// không cần tin tưởng suông vào việc đọc mã thủ công.
/// </summary>
public class MauInCatalogTests
{
    private static string DocTemplate(string tenMau)
    {
        var asm = typeof(BoDoMauIn).Assembly;
        using var stream = asm.GetManifestResourceStream($"Qlgx.Api.PrintTemplates.Chung.{tenMau}.html")
            ?? throw new InvalidOperationException($"Không nhúng được tệp mẫu {tenMau}.html — kiểm tra .csproj.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    public static IEnumerable<object[]> TatCaTenMau() => MauInCatalog.TatCa.Select(m => new object[] { m.TenMau });

    [Theory]
    [MemberData(nameof(TatCaTenMau))]
    public void Danh_muc_khop_dung_tap_hop_cho_trong_that_trong_tep_html(string tenMau)
    {
        var moTa = MauInCatalog.Tim(tenMau)!;
        var html = DocTemplate(tenMau);
        var choTrongThat = Regex.Matches(html, @"\{\{([A-Za-z0-9_]+)\}\}")
            .Select(m => m.Groups[1].Value).Distinct().ToHashSet();
        var choTrongDanhMuc = moTa.ChoTrong.Select(c => c.Key).ToHashSet();

        choTrongDanhMuc.Should().BeEquivalentTo(choTrongThat,
            $"danh mục '{tenMau}' phải khớp CHÍNH XÁC với các {{{{Key}}}} thật trong tệp .html tương ứng");
    }

    /// <summary>238 gốc + 9 chỗ trống thêm khi vá lỗ hổng so với bản desktop (xem
    /// so-sanh-mau-in-desktop.md): WebsiteGiaoXu ở 7 mẫu dùng ThongTinGiaoXuChung, và
    /// GhiChuHonPhoi + TenChanhXu riêng ở Lý lịch cá nhân = 247, + 5 chỗ trống của mẫu mới
    /// "DanhSachRaoHonPhoi" (TenGiaoXu, SoLuong, DieuKienLoc, NgayThangNamIn, HangDanhSach).</summary>
    [Fact]
    public void Tong_so_cho_trong_toan_bo_danh_muc_la_252()
    {
        MauInCatalog.TatCa.Sum(m => m.ChoTrong.Count).Should().Be(252);
    }

    [Fact]
    public void Moi_cho_trong_co_nhan_tieng_viet_khong_rong()
    {
        MauInCatalog.TatCa.SelectMany(m => m.ChoTrong)
            .Should().OnlyContain(c => !string.IsNullOrWhiteSpace(c.Nhan));
    }

    /// <summary>"Biến khả dụng" (những gì người dùng ĐƯỢC PHÉP chèn) phải là SIÊU TẬP của "chỗ
    /// trống mẫu gốc" (những gì mẫu gốc ĐANG dùng) — nếu một chỗ trống của mẫu gốc lại không nằm
    /// trong danh sách chèn được, người dùng lỡ xoá nó khỏi mẫu sẽ không có cách nào chèn lại.</summary>
    [Theory]
    [MemberData(nameof(TatCaTenMau))]
    public void Bien_kha_dung_la_sieu_tap_cua_cho_trong_mau_goc(string tenMau)
    {
        var moTa = MauInCatalog.Tim(tenMau)!;
        moTa.BienKhaDung.Select(b => b.Key)
            .Should().Contain(moTa.ChoTrong.Select(c => c.Key));
        moTa.BienKhaDung.Count.Should().BeGreaterThanOrEqualTo(moTa.ChoTrong.Count);
    }

    /// <summary>Không Key nào được khai hai lần trong cùng một mẫu (dễ xảy ra khi "biến thêm"
    /// vô tình lặp lại một chỗ trống mẫu gốc đã có) — combobox sẽ hiện hai dòng y hệt nhau.</summary>
    [Theory]
    [MemberData(nameof(TatCaTenMau))]
    public void Khong_khai_trung_key_trong_cung_mot_mau(string tenMau)
    {
        var keys = MauInCatalog.Tim(tenMau)!.BienKhaDung.Select(b => b.Key).ToList();
        keys.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void Moi_bien_kha_dung_co_nhan_va_nhom_khong_rong()
    {
        MauInCatalog.TatCa.SelectMany(m => m.BienKhaDung)
            .Should().OnlyContain(b => !string.IsNullOrWhiteSpace(b.Nhan) && !string.IsNullOrWhiteSpace(b.Nhom));
    }
}
