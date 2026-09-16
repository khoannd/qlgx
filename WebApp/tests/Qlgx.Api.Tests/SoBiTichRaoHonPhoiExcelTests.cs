using System.Net;
using ClosedXML.Excel;
using FluentAssertions;
using Qlgx.Domain;
using Qlgx.Domain.Entities;

namespace Qlgx.Api.Tests;

/// <summary>
/// Kiểm thử "Xuất Excel" của hai màn hình "Danh sách sổ bí tích"/"Danh sách rao hôn phối" —
/// hoãn lại ở lượt migrate màn hình (commit aa4a2af), làm ở lượt này (xem
/// docs/superpowers/specs/man-hinh/in-an.md mục 8). Đọc lại tệp .xlsx THẬT bằng ClosedXML, cùng
/// tinh thần với XuatExcelTests.cs.
/// </summary>
public class SoBiTichRaoHonPhoiExcelTests(QlgxApiFactory app) : IClassFixture<QlgxApiFactory>
{
    private const string LoaiNoiDungXlsx =
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    private static XLWorkbook DocWorkbook(byte[] bytes) => new(new MemoryStream(bytes));

    [Fact]
    public async Task Xuat_so_bi_tich_tra_ve_dung_loai_noi_dung_va_tieu_de_in_dam()
    {
        await using var db = app.TaoContextThuan();
        db.DotBiTich.Add(new DotBiTich
        {
            GiaoXuId = app.GiaoXuId, MaDotBiTichCu = 501, LoaiBiTich = LoaiBiTich.RuaToi,
            NgayBiTich = new DateOnly(2024, 12, 25), MoTa = "Dot Excel Giang Sinh",
            LinhMuc = "Lm. Excel A", NoiBiTich = "Nha tho xu Excel",
        });
        await db.SaveChangesAsync();

        var res = await app.CreateAuthClient().GetAsync("/api/dot-bi-tich/xuat-excel?loaiBiTich=RuaToi");

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        res.Content.Headers.ContentType!.MediaType.Should().Be(LoaiNoiDungXlsx);
        using var wb = DocWorkbook(await res.Content.ReadAsByteArrayAsync());
        var ws = wb.Worksheet(1);

        ws.Cell(1, 1).GetString().Should().Be("Ngày");
        ws.Cell(1, 2).GetString().Should().Be("Mô tả");
        ws.Cell(1, 3).GetString().Should().Be("Người ban bí tích");
        ws.Cell(1, 4).GetString().Should().Be("Nơi nhận bí tích");
        ws.Cell(1, 5).GetString().Should().Be("Số lượng GD");
        ws.Cell(1, 1).Style.Font.Bold.Should().BeTrue("tiêu đề cột phải in đậm");

        var moTa = Enumerable.Range(2, ws.LastRowUsed()!.RowNumber() - 1)
            .Select(h => ws.Cell(h, 2).GetString()).ToList();
        moTa.Should().Contain(m => m.Contains("Dot Excel Giang Sinh"));
        var hang = moTa.FindIndex(m => m.Contains("Dot Excel Giang Sinh")) + 2;
        ws.Cell(hang, 1).GetString().Should().Be("25/12/2024");
        ws.Cell(hang, 3).GetString().Should().Be("Lm. Excel A");
        ws.Cell(hang, 4).GetString().Should().Be("Nha tho xu Excel");
    }

    [Fact]
    public async Task Xuat_so_bi_tich_ton_trong_bo_loc_loai_bi_tich_va_khoang_nam()
    {
        await using var db = app.TaoContextThuan();
        db.DotBiTich.AddRange(
            new DotBiTich { GiaoXuId = app.GiaoXuId, MaDotBiTichCu = 502, LoaiBiTich = LoaiBiTich.RuaToi,
                NgayBiTich = new DateOnly(2020, 1, 1), MoTa = "Excel Rua Toi 2020" },
            new DotBiTich { GiaoXuId = app.GiaoXuId, MaDotBiTichCu = 503, LoaiBiTich = LoaiBiTich.RuaToi,
                NgayBiTich = new DateOnly(2024, 1, 1), MoTa = "Excel Rua Toi 2024" },
            new DotBiTich { GiaoXuId = app.GiaoXuId, MaDotBiTichCu = 504, LoaiBiTich = LoaiBiTich.ThemSuc,
                NgayBiTich = new DateOnly(2024, 1, 1), MoTa = "Excel Them Suc 2024" });
        await db.SaveChangesAsync();

        var res = await app.CreateAuthClient()
            .GetAsync("/api/dot-bi-tich/xuat-excel?loaiBiTich=RuaToi&tuNam=2023&denNam=2025");
        using var wb = DocWorkbook(await res.Content.ReadAsByteArrayAsync());
        var ws = wb.Worksheet(1);

        var moTa = Enumerable.Range(2, ws.LastRowUsed()!.RowNumber() - 1)
            .Select(h => ws.Cell(h, 2).GetString()).ToList();
        moTa.Should().Contain(m => m.Contains("Excel Rua Toi 2024"));
        moTa.Should().NotContain(m => m.Contains("Excel Rua Toi 2020"), "ngoài khoảng tuNam/denNam");
        moTa.Should().NotContain(m => m.Contains("Excel Them Suc 2024"), "khác loaiBiTich đang lọc");
    }

    [Fact]
    public async Task Xuat_so_bi_tich_cua_giao_xu_khac_khong_lot_vao_tep()
    {
        var giaoXuKhac = Guid.NewGuid();
        await using (var db = app.TaoContextThuan())
        {
            db.GiaoXu.Add(new GiaoXu { Id = giaoXuKhac, TenGiaoXu = "Giao xu khac SoBiTich", MaGiaoXuCu = 997 });
            db.DotBiTich.Add(new DotBiTich { GiaoXuId = giaoXuKhac, MaDotBiTichCu = 505,
                LoaiBiTich = LoaiBiTich.RuaToi, MoTa = "Dot Cua Xu Khac" });
            await db.SaveChangesAsync();
        }

        var res = await app.CreateAuthClient().GetAsync("/api/dot-bi-tich/xuat-excel?loaiBiTich=RuaToi");
        using var wb = DocWorkbook(await res.Content.ReadAsByteArrayAsync());
        var ws = wb.Worksheet(1);

        var soHang = ws.LastRowUsed()?.RowNumber() ?? 1;
        for (var h = 2; h <= soHang; h++)
            ws.Cell(h, 2).GetString().Should().NotContain("Dot Cua Xu Khac");
    }

    [Fact]
    public async Task Xuat_rao_hon_phoi_tra_ve_dung_loai_noi_dung_va_tieu_de_in_dam()
    {
        await using var db = app.TaoContextThuan();
        var g1 = new GiaoDan { GiaoXuId = app.GiaoXuId, MaGiaoDanCu = 601, HoTen = "Nguoi Rao Excel Mot" };
        var g2 = new GiaoDan { GiaoXuId = app.GiaoXuId, MaGiaoDanCu = 602, HoTen = "Nguoi Rao Excel Hai" };
        db.AddRange(g1, g2);
        db.RaoHonPhoi.Add(new RaoHonPhoi
        {
            GiaoXuId = app.GiaoXuId, MaRaoHonPhoiCu = 71, TenRaoHonPhoi = "Doi Rao Excel",
            GiaoDan1 = g1, GiaoDan2 = g2, NgayRaoLan1 = new DateOnly(2024, 6, 1),
            NgayRaoLan2 = new DateOnly(2024, 6, 8), NgayRaoLan3 = new DateOnly(2024, 6, 15),
            GhiChu = "Ghi chu Excel",
        });
        await db.SaveChangesAsync();

        var res = await app.CreateAuthClient().GetAsync("/api/rao-hon-phoi/xuat-excel?xemTatCa=true");

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        res.Content.Headers.ContentType!.MediaType.Should().Be(LoaiNoiDungXlsx);
        using var wb = DocWorkbook(await res.Content.ReadAsByteArrayAsync());
        var ws = wb.Worksheet(1);

        ws.Cell(1, 1).GetString().Should().Be("Mã rao");
        ws.Cell(1, 2).GetString().Should().Be("Đôi rao");
        ws.Cell(1, 3).GetString().Should().Be("Người thứ nhất");
        ws.Cell(1, 4).GetString().Should().Be("Người thứ hai");
        ws.Cell(1, 5).GetString().Should().Be("Rao lần 1");
        ws.Cell(1, 8).GetString().Should().Be("Ghi chú");
        ws.Cell(1, 1).Style.Font.Bold.Should().BeTrue("tiêu đề cột phải in đậm");

        var maRao = Enumerable.Range(2, ws.LastRowUsed()!.RowNumber() - 1)
            .Select(h => ws.Cell(h, 1).GetValue<int>()).ToList();
        maRao.Should().Contain(71);
        var hang = maRao.IndexOf(71) + 2;
        ws.Cell(hang, 2).GetString().Should().Be("Doi Rao Excel");
        ws.Cell(hang, 3).GetString().Should().Be("Nguoi Rao Excel Mot");
        ws.Cell(hang, 4).GetString().Should().Be("Nguoi Rao Excel Hai");
        ws.Cell(hang, 5).GetString().Should().Be("01/06/2024");
        ws.Cell(hang, 8).GetString().Should().Be("Ghi chu Excel");
    }

    [Fact]
    public async Task Xuat_rao_hon_phoi_mac_dinh_chi_lay_doi_chua_hoan_tat()
    {
        await using var db = app.TaoContextThuan();
        var homNay = DateOnly.FromDateTime(DateTime.Now);
        db.RaoHonPhoi.AddRange(
            new RaoHonPhoi { GiaoXuId = app.GiaoXuId, MaRaoHonPhoiCu = 72, TenRaoHonPhoi = "Doi Da Hoan Tat",
                NgayRaoLan3 = homNay.AddDays(-30) },
            new RaoHonPhoi { GiaoXuId = app.GiaoXuId, MaRaoHonPhoiCu = 73, TenRaoHonPhoi = "Doi Chua Hoan Tat",
                NgayRaoLan3 = null });
        await db.SaveChangesAsync();

        var res = await app.CreateAuthClient().GetAsync("/api/rao-hon-phoi/xuat-excel");
        using var wb = DocWorkbook(await res.Content.ReadAsByteArrayAsync());
        var ws = wb.Worksheet(1);

        var ten = Enumerable.Range(2, ws.LastRowUsed()!.RowNumber() - 1)
            .Select(h => ws.Cell(h, 2).GetString()).ToList();
        ten.Should().Contain("Doi Chua Hoan Tat");
        ten.Should().NotContain("Doi Da Hoan Tat");
    }

    [Fact]
    public async Task Xuat_rao_hon_phoi_cua_giao_xu_khac_khong_lot_vao_tep()
    {
        var giaoXuKhac = Guid.NewGuid();
        await using (var db = app.TaoContextThuan())
        {
            db.GiaoXu.Add(new GiaoXu { Id = giaoXuKhac, TenGiaoXu = "Giao xu khac RaoHP", MaGiaoXuCu = 996 });
            db.RaoHonPhoi.Add(new RaoHonPhoi { GiaoXuId = giaoXuKhac, MaRaoHonPhoiCu = 74,
                TenRaoHonPhoi = "Doi Cua Xu Khac" });
            await db.SaveChangesAsync();
        }

        var res = await app.CreateAuthClient().GetAsync("/api/rao-hon-phoi/xuat-excel?xemTatCa=true");
        using var wb = DocWorkbook(await res.Content.ReadAsByteArrayAsync());
        var ws = wb.Worksheet(1);

        var soHang = ws.LastRowUsed()?.RowNumber() ?? 1;
        for (var h = 2; h <= soHang; h++)
            ws.Cell(h, 2).GetString().Should().NotBe("Doi Cua Xu Khac");
    }
}
