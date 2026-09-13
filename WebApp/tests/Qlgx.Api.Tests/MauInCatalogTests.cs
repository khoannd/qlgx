using System.Reflection;
using System.Text.RegularExpressions;
using FluentAssertions;
using Qlgx.Api.Printing;

namespace Qlgx.Api.Tests;

/// <summary>
/// Đối chiếu TỰ ĐỘNG danh mục MauInCatalog (viết tay, đọc hiểu InAnService.cs) với các chỗ
/// trống <c>{{Key}}</c> THẬT SỰ có trong 12 tệp PrintTemplates/Chung/*.html — đúng yêu cầu
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

    [Fact]
    public void Tong_so_cho_trong_toan_bo_danh_muc_la_238()
    {
        MauInCatalog.TatCa.Sum(m => m.ChoTrong.Count).Should().Be(238);
    }

    [Fact]
    public void Moi_cho_trong_co_nhan_tieng_viet_khong_rong()
    {
        MauInCatalog.TatCa.SelectMany(m => m.ChoTrong)
            .Should().OnlyContain(c => !string.IsNullOrWhiteSpace(c.Nhan));
    }
}
