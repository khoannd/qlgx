using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Qlgx.Domain.Entities;

namespace Qlgx.Api.Tests;

/// <summary>
/// "Hồ sơ lưu trữ giáo dân" / "Hồ sơ lưu trữ gia đình" (frmGiaoDanLuuTruList.cs,
/// frmGiaDinhLuuTruList.cs) — xem docs/superpowers/specs/man-hinh/ho-so-luu-tru.md mục 0 cho
/// bằng chứng đầy đủ về điều kiện WHERE (OR, không AND) lấy nguyên từ GxGiaoHo.LoadGridData khi
/// IsLuuTru=true.
/// </summary>
public class HoSoLuuTruTests(QlgxApiFactory app) : IClassFixture<QlgxApiFactory>
{
    private sealed record Item(Guid Id, int MaGiaoDanCu);
    private sealed record ItemGiaDinh(Guid Id, int MaGiaDinhCu);

    // --- Giáo dân: OR giữa DaXoa/QuaDoi/DaChuyenXu, không lẫn với danh sách đang hoạt động ---

    [Fact]
    public async Task Luu_tru_giao_dan_gom_ca_ba_dieu_kien_OR_va_khong_lan_nguoi_dang_hoat_dong()
    {
        Guid idXoaMem, idQuaDoi, idHoatDong;
        await using (var db = app.TaoContextThuan())
        {
            var xoaMem = new GiaoDan { GiaoXuId = app.GiaoXuId, MaGiaoDanCu = 93001, HoTen = "Da xoa mem", Phai = "Nam", DaXoa = true };
            var quaDoi = new GiaoDan { GiaoXuId = app.GiaoXuId, MaGiaoDanCu = 93002, HoTen = "Da qua doi", Phai = "Nam", QuaDoi = true };
            var hoatDong = new GiaoDan { GiaoXuId = app.GiaoXuId, MaGiaoDanCu = 93003, HoTen = "Dang hoat dong", Phai = "Nam" };
            db.GiaoDan.AddRange(xoaMem, quaDoi, hoatDong);
            await db.SaveChangesAsync();
            idXoaMem = xoaMem.Id; idQuaDoi = quaDoi.Id; idHoatDong = hoatDong.Id;
        }
        var client = app.CreateAuthClient();

        var ds = await client.GetFromJsonAsync<List<Item>>("/api/giao-dan/luu-tru") ?? [];

        ds.Select(x => x.Id).Should().Contain([idXoaMem, idQuaDoi]);
        ds.Select(x => x.Id).Should().NotContain(idHoatDong, "nguoi dang hoat dong khong duoc xuat hien o ho so luu tru");
    }

    [Fact]
    public async Task Luu_tru_giao_dan_va_danh_sach_dang_hoat_dong_la_hai_tap_khong_giao_nhau()
    {
        Guid idQuaDoi;
        await using (var db = app.TaoContextThuan())
        {
            var quaDoi = new GiaoDan { GiaoXuId = app.GiaoXuId, MaGiaoDanCu = 93010, HoTen = "Kiem tra khong giao nhau", Phai = "Nam", QuaDoi = true };
            db.GiaoDan.Add(quaDoi);
            await db.SaveChangesAsync();
            idQuaDoi = quaDoi.Id;
        }
        var client = app.CreateAuthClient();

        var dsHoatDong = await client.GetFromJsonAsync<List<Item>>("/api/giao-dan");
        var dsLuuTru = await client.GetFromJsonAsync<List<Item>>("/api/giao-dan/luu-tru");

        dsHoatDong!.Select(x => x.Id).Should().NotContain(idQuaDoi);
        dsLuuTru!.Select(x => x.Id).Should().Contain(idQuaDoi);
    }

    // --- Gia đình: OR giữa DaXoa/DaChuyenXu, KHÔNG có điều kiện qua đời ----------------------

    [Fact]
    public async Task Luu_tru_gia_dinh_gom_da_xoa_va_da_chuyen_xu_khong_lan_dang_hoat_dong()
    {
        Guid idXoaMem, idChuyenXu, idHoatDong;
        await using (var db = app.TaoContextThuan())
        {
            var xoaMem = new GiaDinh { GiaoXuId = app.GiaoXuId, MaGiaDinhCu = 94001, TenGiaDinh = "GD da xoa mem", DaXoa = true };
            var chuyenXu = new GiaDinh { GiaoXuId = app.GiaoXuId, MaGiaDinhCu = 94002, TenGiaDinh = "GD da chuyen xu", DaChuyenXu = true };
            var hoatDong = new GiaDinh { GiaoXuId = app.GiaoXuId, MaGiaDinhCu = 94003, TenGiaDinh = "GD dang hoat dong" };
            db.GiaDinh.AddRange(xoaMem, chuyenXu, hoatDong);
            await db.SaveChangesAsync();
            idXoaMem = xoaMem.Id; idChuyenXu = chuyenXu.Id; idHoatDong = hoatDong.Id;
        }
        var client = app.CreateAuthClient();

        var ds = await client.GetFromJsonAsync<List<ItemGiaDinh>>("/api/gia-dinh/luu-tru") ?? [];

        ds.Select(x => x.Id).Should().Contain([idXoaMem, idChuyenXu]);
        ds.Select(x => x.Id).Should().NotContain(idHoatDong);
    }

    // --- Sửa/xoá vĩnh viễn phải hoạt động được trên bản ghi ĐÃ DaXoa=true (chính nội dung của
    // hồ sơ lưu trữ) — trước khi sửa, LayChiTiet/CapNhat/Xoa đều lọc !DaXoa nên luôn "không tìm
    // thấy" cho mọi bản ghi trong hồ sơ lưu trữ. ---------------------------------------------

    [Fact]
    public async Task Mo_chi_tiet_mot_giao_dan_da_xoa_mem_van_tra_ve_200_khong_phai_404()
    {
        Guid id;
        await using (var db = app.TaoContextThuan())
        {
            var g = new GiaoDan { GiaoXuId = app.GiaoXuId, MaGiaoDanCu = 93020, HoTen = "Mo chi tiet nguoi da xoa", Phai = "Nam", DaXoa = true };
            db.GiaoDan.Add(g);
            await db.SaveChangesAsync();
            id = g.Id;
        }
        var client = app.CreateAuthClient();

        var res = await client.GetAsync($"/api/giao-dan/{id}");

        res.StatusCode.Should().Be(HttpStatusCode.OK,
            "man hinh Ho so luu tru phai mo duoc chi tiet giao dan da xoa mem de sua, dung nhu gxGiaoDanList1.EditRow() cua desktop");
    }

    [Fact]
    public async Task Xoa_vinh_vien_mot_giao_dan_da_xoa_mem_thanh_cong_khong_phai_KhongTimThay()
    {
        Guid id;
        await using (var db = app.TaoContextThuan())
        {
            var g = new GiaoDan { GiaoXuId = app.GiaoXuId, MaGiaoDanCu = 93021, HoTen = "Xoa vinh vien nguoi da xoa mem", Phai = "Nam", DaXoa = true };
            db.GiaoDan.Add(g);
            await db.SaveChangesAsync();
            id = g.Id;
        }
        var client = app.CreateAuthClient();

        var res = await client.DeleteAsync($"/api/giao-dan/{id}?vinhVien=true");

        res.StatusCode.Should().Be(HttpStatusCode.OK,
            "nut Xoa bo cua man hinh luu tru phai xoa vinh vien duoc chinh nhung ban ghi DaXoa=true nam trong no");
        await using var db2 = app.TaoContextThuan();
        (await db2.GiaoDan.AnyAsync(x => x.Id == id)).Should().BeFalse();
    }

    [Fact]
    public async Task Xoa_vinh_vien_mot_gia_dinh_da_xoa_mem_thanh_cong()
    {
        Guid id;
        await using (var db = app.TaoContextThuan())
        {
            var g = new GiaDinh { GiaoXuId = app.GiaoXuId, MaGiaDinhCu = 94020, TenGiaDinh = "Xoa vinh vien GD da xoa mem", DaXoa = true };
            db.GiaDinh.Add(g);
            await db.SaveChangesAsync();
            id = g.Id;
        }
        var client = app.CreateAuthClient();

        var res = await client.DeleteAsync($"/api/gia-dinh/{id}?vinhVien=true");

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        await using var db2 = app.TaoContextThuan();
        (await db2.GiaDinh.AnyAsync(x => x.Id == id)).Should().BeFalse();
    }
}
