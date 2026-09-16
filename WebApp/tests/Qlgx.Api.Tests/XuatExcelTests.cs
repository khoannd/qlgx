using System.Net;
using ClosedXML.Excel;
using FluentAssertions;
using Qlgx.Domain;
using Qlgx.Domain.Entities;

namespace Qlgx.Api.Tests;

/// <summary>
/// Kiểm thử "Xuất Excel" (thay nút "Xuất dữ liệu (CSV)" cũ) — xem XuatExcelService.cs. Đọc lại
/// tệp .xlsx THẬT bằng ClosedXML (thư viện sinh ra nó cũng đọc lại được) để kiểm nội dung thay
/// vì chỉ kiểm mã trạng thái/loại nội dung, đúng tinh thần "chứng minh bằng chạy thật".
/// </summary>
public class XuatExcelTests(QlgxApiFactory app) : IClassFixture<QlgxApiFactory>
{
    private const string LoaiNoiDungXlsx =
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    private static XLWorkbook DocWorkbook(byte[] bytes)
    {
        var luong = new MemoryStream(bytes);
        return new XLWorkbook(luong);
    }

    [Fact]
    public async Task Xuat_giao_dan_tra_ve_dung_loai_noi_dung_va_tieu_de_in_dam()
    {
        await using var db = app.TaoContextThuan();
        db.GiaoDan.Add(new GiaoDan
        {
            GiaoXuId = app.GiaoXuId, MaGiaoDanCu = 7001, HoTen = "Nguyen Van Excel",
            TenThanh = "Giuse", Phai = "Nam", NgaySinh = new DateOnly(1985, 3, 10),
        });
        await db.SaveChangesAsync();

        var res = await app.CreateAuthClient().GetAsync("/api/giao-dan/xuat-excel");

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        res.Content.Headers.ContentType!.MediaType.Should().Be(LoaiNoiDungXlsx);
        var bytes = await res.Content.ReadAsByteArrayAsync();
        using var wb = DocWorkbook(bytes);
        var ws = wb.Worksheet(1);

        // 29 cột đúng cotGiaoDan.ts — kiểm vài mốc đầu/cuối, không chép lại toàn bộ 29 tên.
        ws.Cell(1, 1).GetString().Should().Be("Mã GD");
        ws.Cell(1, 2).GetString().Should().Be("Tên thánh");
        ws.Cell(1, 3).GetString().Should().Be("Họ tên");
        ws.Cell(1, 29).GetString().Should().Be("Nơi thêm sức");
        ws.Cell(1, 1).Style.Font.Bold.Should().BeTrue("tiêu đề cột phải in đậm");
        ws.Cell(1, 3).Style.Font.Bold.Should().BeTrue();

        var hangDuLieu = Enumerable.Range(2, ws.LastRowUsed()!.RowNumber() - 1)
            .Select(h => ws.Cell(h, 1).GetValue<int>()).ToList();
        hangDuLieu.Should().Contain(7001);
        var hang = hangDuLieu.IndexOf(7001) + 2;
        ws.Cell(hang, 3).GetString().Should().Be("Nguyen Van Excel");
        ws.Cell(hang, 5).GetString().Should().Be("10/03/1985", "cột ngày phải hiển thị dd/MM/yyyy giống lưới");
    }

    [Fact]
    public async Task Xuat_giao_dan_khong_gach_ngang_dong_nao()
    {
        // Đúng hành vi lưới GIÁO DÂN đứng một mình trên màn hình danh sách (khác lưới nhúng
        // trong form gia đình) — xem chú thích ở XuatExcelService.XuatGiaoDan.
        await using var db = app.TaoContextThuan();
        db.GiaoDan.Add(new GiaoDan
        {
            GiaoXuId = app.GiaoXuId, MaGiaoDanCu = 7002, HoTen = "Nguoi Da Qua Doi",
            TenThanh = "Maria", Phai = "Nu", QuaDoi = true,
        });
        await db.SaveChangesAsync();

        var res = await app.CreateAuthClient().GetAsync("/api/giao-dan/xuat-excel?hienCaDaMat=true");
        var bytes = await res.Content.ReadAsByteArrayAsync();
        using var wb = DocWorkbook(bytes);
        var ws = wb.Worksheet(1);

        var soHang = ws.LastRowUsed()!.RowNumber();
        for (var h = 2; h <= soHang; h++)
            ws.Row(h).Cells().Should().OnlyContain(o => !o.Style.Font.Strikethrough);
    }

    [Fact]
    public async Task Xuat_giao_dan_ton_trong_bo_loc_giao_ho_va_khong_thong_ke()
    {
        await using var db = app.TaoContextThuan();
        var giaoHo = new GiaoHo { GiaoXuId = app.GiaoXuId, TenGiaoHo = "Giao ho Excel A", MaGiaoHoCu = 71 };
        db.GiaoHo.Add(giaoHo);
        db.GiaoDan.AddRange(
            new GiaoDan { GiaoXuId = app.GiaoXuId, MaGiaoDanCu = 7010, HoTen = "Trong Giao Ho A", GiaoHo = giaoHo },
            new GiaoDan { GiaoXuId = app.GiaoXuId, MaGiaoDanCu = 7011, HoTen = "Ngoai Giao Ho A" });
        await db.SaveChangesAsync();

        var res = await app.CreateAuthClient().GetAsync($"/api/giao-dan/xuat-excel?giaoHoId={giaoHo.Id}");
        using var wb = DocWorkbook(await res.Content.ReadAsByteArrayAsync());
        var ws = wb.Worksheet(1);

        var hoTen = Enumerable.Range(2, ws.LastRowUsed()!.RowNumber() - 1)
            .Select(h => ws.Cell(h, 3).GetString()).ToList();
        hoTen.Should().Contain("Trong Giao Ho A");
        hoTen.Should().NotContain("Ngoai Giao Ho A");
    }

    [Fact]
    public async Task Xuat_giao_dan_cua_giao_xu_khac_khong_lot_vao_tep()
    {
        var giaoXuKhac = Guid.NewGuid();
        await using (var db = app.TaoContextThuan())
        {
            db.GiaoXu.Add(new GiaoXu { Id = giaoXuKhac, TenGiaoXu = "Giao xu khac Excel", MaGiaoXuCu = 998 });
            db.GiaoDan.Add(new GiaoDan { GiaoXuId = giaoXuKhac, MaGiaoDanCu = 7020, HoTen = "Nguoi Xu Khac Excel" });
            await db.SaveChangesAsync();
        }

        var res = await app.CreateAuthClient().GetAsync("/api/giao-dan/xuat-excel");
        using var wb = DocWorkbook(await res.Content.ReadAsByteArrayAsync());
        var ws = wb.Worksheet(1);

        var soHang = ws.LastRowUsed()?.RowNumber() ?? 1;
        for (var h = 2; h <= soHang; h++)
            ws.Cell(h, 3).GetString().Should().NotBe("Nguoi Xu Khac Excel");
    }

    [Fact]
    public async Task Xuat_gia_dinh_gach_ngang_dung_o_nguoi_da_qua_doi()
    {
        await using var db = app.TaoContextThuan();
        var gd = new GiaDinh { GiaoXuId = app.GiaoXuId, MaGiaDinhCu = 81, TenGiaDinh = "Excel - GachNgang" };
        var chong = new GiaoDan { GiaoXuId = app.GiaoXuId, MaGiaoDanCu = 811, HoTen = "Chong Da Mat",
            TenThanh = "Giuse", Phai = "Nam", QuaDoi = true };
        var vo = new GiaoDan { GiaoXuId = app.GiaoXuId, MaGiaoDanCu = 812, HoTen = "Vo Con Song",
            TenThanh = "Maria", Phai = "Nu", QuaDoi = false };
        db.AddRange(gd, chong, vo);
        db.ThanhVienGiaDinh.AddRange(
            new ThanhVienGiaDinh { GiaoXuId = app.GiaoXuId, GiaDinh = gd, GiaoDan = chong, VaiTro = VaiTroGiaDinh.Chong },
            new ThanhVienGiaDinh { GiaoXuId = app.GiaoXuId, GiaDinh = gd, GiaoDan = vo, VaiTro = VaiTroGiaDinh.Vo });
        await db.SaveChangesAsync();

        var res = await app.CreateAuthClient().GetAsync("/api/gia-dinh/xuat-excel");

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        res.Content.Headers.ContentType!.MediaType.Should().Be(LoaiNoiDungXlsx);
        using var wb = DocWorkbook(await res.Content.ReadAsByteArrayAsync());
        var ws = wb.Worksheet(1);
        ws.Cell(1, 1).GetString().Should().Be("Mã GĐ");
        ws.Cell(1, 3).GetString().Should().Be("Người nam");
        ws.Cell(1, 4).GetString().Should().Be("Người nữ");
        ws.Cell(1, 1).Style.Font.Bold.Should().BeTrue();

        var hangDuLieu = Enumerable.Range(2, ws.LastRowUsed()!.RowNumber() - 1)
            .Select(h => ws.Cell(h, 1).GetValue<int>()).ToList();
        var hang = hangDuLieu.IndexOf(81) + 2;
        ws.Cell(hang, 3).GetString().Should().Contain("Chong Da Mat");
        ws.Cell(hang, 3).Style.Font.Strikethrough.Should().BeTrue("Người nam đã qua đời phải gạch ngang");
        ws.Cell(hang, 4).GetString().Should().Contain("Vo Con Song");
        ws.Cell(hang, 4).Style.Font.Strikethrough.Should().BeFalse("Người nữ còn sống không gạch ngang");
    }

    [Fact]
    public async Task Xuat_gia_dinh_ton_trong_bo_loc_chi_khong_thong_ke()
    {
        await using var db = app.TaoContextThuan();
        db.GiaDinh.AddRange(
            new GiaDinh { GiaoXuId = app.GiaoXuId, MaGiaDinhCu = 91, TenGiaDinh = "Excel Duoc Thong Ke", KhongThongKe = false },
            new GiaDinh { GiaoXuId = app.GiaoXuId, MaGiaDinhCu = 92, TenGiaDinh = "Excel Khong Thong Ke", KhongThongKe = true });
        await db.SaveChangesAsync();

        var res = await app.CreateAuthClient().GetAsync("/api/gia-dinh/xuat-excel?chiKhongThongKe=true");
        using var wb = DocWorkbook(await res.Content.ReadAsByteArrayAsync());
        var ws = wb.Worksheet(1);

        var ten = Enumerable.Range(2, ws.LastRowUsed()!.RowNumber() - 1)
            .Select(h => ws.Cell(h, 2).GetString()).ToList();
        ten.Should().Contain("Excel Khong Thong Ke");
        ten.Should().NotContain("Excel Duoc Thong Ke");
    }
}
