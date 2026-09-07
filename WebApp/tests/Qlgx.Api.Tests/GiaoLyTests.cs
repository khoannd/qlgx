using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Qlgx.Domain.Entities;

namespace Qlgx.Api.Tests;

/// <summary>
/// Phân hệ Giáo lý — Khối/Lớp/Học viên/Giáo lý viên (`frmKhoiGiaoLyList.cs` + `frmKhoiGiaoLy.cs`
/// + `frmLopGiaoLy.cs`). Xem docs/superpowers/specs/man-hinh/giao-ly.md. Bốn bảng rỗng ở dữ
/// liệu thật khảo sát nên test là bằng chứng chính cho phân hệ này.
/// </summary>
public class GiaoLyTests(QlgxApiFactory app) : IClassFixture<QlgxApiFactory>
{
    private sealed record KhoiDto(Guid Id, int MaKhoiCu, string TenKhoi, Guid? NguoiQuanLyId,
        string? TenNguoiQuanLy, string? GhiChu, int SoLop, uint RowVersion);
    private sealed record LopDto(Guid Id, int MaLopCu, string TenLop, Guid KhoiGiaoLyId, int? Nam,
        string? PhongHoc, string? GhiChu, int SoHocVien, string? TenGiaoLyVien, uint RowVersion);
    private sealed record HocVienDto(Guid ChiTietId, Guid GiaoDanId, int? SoThuTu, string HoTen,
        string? TenThanh, string? Phai, DateOnly? NgaySinh, bool HoanThanh, string? GhiChuGLy, uint RowVersion);
    private sealed record GiaoLyVienDto(Guid Id, Guid GiaoDanId, string HoTen, string? TenThanh, uint RowVersion);
    private sealed record ThongBaoLoi(string ThongBao);

    private static int _maGiaoDanKeTiep = 60001;

    private async Task<Guid> TaoGiaoDan(string hoTen)
    {
        await using var db = app.TaoContextThuan();
        var gd = new GiaoDan { GiaoXuId = app.GiaoXuId, MaGiaoDanCu = Interlocked.Increment(ref _maGiaoDanKeTiep), HoTen = hoTen };
        db.GiaoDan.Add(gd);
        await db.SaveChangesAsync();
        return gd.Id;
    }

    private async Task<Guid> TaoKhoi(HttpClient client, string ten, Guid nguoiQuanLyId)
    {
        var res = await client.PostAsJsonAsync("/api/giao-ly/khoi", new
        {
            tenKhoi = ten, nguoiQuanLyId, ghiChu = (string?)null, rowVersion = (uint?)null,
        });
        res.StatusCode.Should().Be(HttpStatusCode.Created);
        return (await res.Content.ReadFromJsonAsync<Dictionary<string, Guid>>())!["id"];
    }

    private async Task<Guid> TaoLop(HttpClient client, Guid khoiId, string ten, int? nam = 2026)
    {
        var res = await client.PostAsJsonAsync($"/api/giao-ly/khoi/{khoiId}/lop", new
        {
            tenLop = ten, nam, phongHoc = (string?)null, ghiChu = (string?)null, rowVersion = (uint?)null,
        });
        res.StatusCode.Should().Be(HttpStatusCode.Created);
        return (await res.Content.ReadFromJsonAsync<Dictionary<string, Guid>>())!["id"];
    }

    // --- Khối giáo lý -------------------------------------------------------------------

    [Fact]
    public async Task Thieu_ten_khoi_thi_bao_loi_dung_nguyen_van()
    {
        var client = app.CreateAuthClient();
        var nguoiQuanLyId = await TaoGiaoDan("Quan ly khoi test");
        var res = await client.PostAsJsonAsync("/api/giao-ly/khoi", new
        {
            tenKhoi = "  ", nguoiQuanLyId, ghiChu = (string?)null, rowVersion = (uint?)null,
        });
        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var loi = await res.Content.ReadFromJsonAsync<ThongBaoLoi>();
        loi!.ThongBao.Should().Be("Hãy nhập tên khối giáo lý");
    }

    [Fact]
    public async Task Thieu_nguoi_quan_ly_thi_bao_loi_dung_nguyen_van()
    {
        var client = app.CreateAuthClient();
        var res = await client.PostAsJsonAsync("/api/giao-ly/khoi", new
        {
            tenKhoi = "Khoi khong co nguoi quan ly", nguoiQuanLyId = Guid.NewGuid(),
            ghiChu = (string?)null, rowVersion = (uint?)null,
        });
        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var loi = await res.Content.ReadFromJsonAsync<ThongBaoLoi>();
        loi!.ThongBao.Should().Be("Hãy chọn người quản lý");
    }

    [Fact]
    public async Task Tao_sua_khoi_roi_doc_lai_dung_du_cot()
    {
        var client = app.CreateAuthClient();
        var nguoiQuanLyId = await TaoGiaoDan("Cha xu Khai Tam");
        var id = await TaoKhoi(client, "Khai Tâm", nguoiQuanLyId);

        var ds = await client.GetFromJsonAsync<List<KhoiDto>>("/api/giao-ly/khoi");
        var dong = ds!.Single(k => k.Id == id);
        dong.TenKhoi.Should().Be("Khai Tâm");
        dong.NguoiQuanLyId.Should().Be(nguoiQuanLyId);
        dong.TenNguoiQuanLy.Should().Contain("Cha xu Khai Tam");
        dong.SoLop.Should().Be(0);

        var nguoiQuanLyMoi = await TaoGiaoDan("Nguoi quan ly moi");
        var suaRes = await client.PutAsJsonAsync($"/api/giao-ly/khoi/{id}", new
        {
            tenKhoi = "Khai Tâm (sửa)", nguoiQuanLyId = nguoiQuanLyMoi, ghiChu = "Ghi chu moi",
            rowVersion = dong.RowVersion,
        });
        suaRes.StatusCode.Should().Be(HttpStatusCode.OK);
        var ds2 = await client.GetFromJsonAsync<List<KhoiDto>>("/api/giao-ly/khoi");
        ds2!.Single(k => k.Id == id).TenKhoi.Should().Be("Khai Tâm (sửa)");
    }

    [Fact]
    public async Task Sua_khoi_sai_RowVersion_thi_bao_xung_dot()
    {
        var client = app.CreateAuthClient();
        var nguoiQuanLyId = await TaoGiaoDan("Quan ly xung dot");
        var id = await TaoKhoi(client, "Khối xung đột", nguoiQuanLyId);

        var res = await client.PutAsJsonAsync($"/api/giao-ly/khoi/{id}", new
        {
            tenKhoi = "Khối xung đột 2", nguoiQuanLyId, ghiChu = (string?)null, rowVersion = (uint?)999999,
        });
        res.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    // --- Lớp giáo lý ---------------------------------------------------------------------

    [Fact]
    public async Task Thieu_ten_lop_thi_bao_loi_dung_nguyen_van()
    {
        var client = app.CreateAuthClient();
        var nguoiQuanLyId = await TaoGiaoDan("Quan ly cho lop");
        var khoiId = await TaoKhoi(client, "Khối cho lớp", nguoiQuanLyId);

        var res = await client.PostAsJsonAsync($"/api/giao-ly/khoi/{khoiId}/lop", new
        {
            tenLop = "", nam = 2026, phongHoc = (string?)null, ghiChu = (string?)null, rowVersion = (uint?)null,
        });
        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var loi = await res.Content.ReadFromJsonAsync<ThongBaoLoi>();
        loi!.ThongBao.Should().Be("Hãy nhập tên lớp giáo lý");
    }

    [Fact]
    public async Task Them_lop_vao_khoi_khong_ton_tai_tra_404()
    {
        var client = app.CreateAuthClient();
        var res = await client.PostAsJsonAsync($"/api/giao-ly/khoi/{Guid.NewGuid()}/lop", new
        {
            tenLop = "Lớp mồ côi", nam = 2026, phongHoc = (string?)null, ghiChu = (string?)null, rowVersion = (uint?)null,
        });
        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Tao_lop_roi_loc_theo_nam_dung()
    {
        var client = app.CreateAuthClient();
        var nguoiQuanLyId = await TaoGiaoDan("Quan ly loc nam");
        var khoiId = await TaoKhoi(client, "Khối lọc năm", nguoiQuanLyId);
        var lop2025 = await TaoLop(client, khoiId, "Lớp 2025", 2025);
        var lop2026 = await TaoLop(client, khoiId, "Lớp 2026", 2026);

        var dsTatCa = await client.GetFromJsonAsync<List<LopDto>>($"/api/giao-ly/khoi/{khoiId}/lop");
        dsTatCa!.Select(l => l.Id).Should().Contain([lop2025, lop2026]);

        var ds2026 = await client.GetFromJsonAsync<List<LopDto>>($"/api/giao-ly/khoi/{khoiId}/lop?nam=2026");
        ds2026!.Select(l => l.Id).Should().Contain(lop2026);
        ds2026.Select(l => l.Id).Should().NotContain(lop2025);
    }

    [Fact]
    public async Task Sua_nam_cua_lop_tu_do_khong_bi_khoa_theo_nam_luc_tao()
    {
        var client = app.CreateAuthClient();
        var nguoiQuanLyId = await TaoGiaoDan("Quan ly sua nam");
        var khoiId = await TaoKhoi(client, "Khối sửa năm", nguoiQuanLyId);
        var lopId = await TaoLop(client, khoiId, "Lớp sửa năm", 2025);
        var lop = (await client.GetFromJsonAsync<List<LopDto>>($"/api/giao-ly/khoi/{khoiId}/lop"))!.Single(l => l.Id == lopId);

        var res = await client.PutAsJsonAsync($"/api/giao-ly/lop/{lopId}", new
        {
            tenLop = "Lớp sửa năm", nam = 2027, phongHoc = (string?)null, ghiChu = (string?)null,
            rowVersion = lop.RowVersion,
        });
        res.StatusCode.Should().Be(HttpStatusCode.OK);
        var lopSauSua = (await client.GetFromJsonAsync<List<LopDto>>($"/api/giao-ly/khoi/{khoiId}/lop"))!.Single(l => l.Id == lopId);
        lopSauSua.Nam.Should().Be(2027);
    }

    // --- Học viên ----------------------------------------------------------------------

    [Fact]
    public async Task Them_sua_xoa_hoc_vien()
    {
        var client = app.CreateAuthClient();
        var nguoiQuanLyId = await TaoGiaoDan("Quan ly hoc vien");
        var khoiId = await TaoKhoi(client, "Khối học viên", nguoiQuanLyId);
        var lopId = await TaoLop(client, khoiId, "Lớp học viên");
        var hocVienId = await TaoGiaoDan("Em be hoc giao ly");

        var themRes = await client.PostAsJsonAsync($"/api/giao-ly/lop/{lopId}/hoc-vien", new { giaoDanId = hocVienId });
        themRes.StatusCode.Should().Be(HttpStatusCode.OK);

        var ds = await client.GetFromJsonAsync<List<HocVienDto>>($"/api/giao-ly/lop/{lopId}/hoc-vien");
        ds.Should().ContainSingle();
        var hv = ds![0];
        hv.HoanThanh.Should().BeFalse();
        hv.SoThuTu.Should().Be(1);

        // Thêm trùng → chặn, đúng thông báo gốc.
        var trungRes = await client.PostAsJsonAsync($"/api/giao-ly/lop/{lopId}/hoc-vien", new { giaoDanId = hocVienId });
        trungRes.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await trungRes.Content.ReadFromJsonAsync<ThongBaoLoi>())!.ThongBao.Should().Be("Giáo dân này đã tồn tại trong danh sách");

        // Sửa: đánh dấu hoàn thành.
        var suaRes = await client.PutAsJsonAsync($"/api/giao-ly/hoc-vien/{hv.ChiTietId}", new
        {
            soThuTu = hv.SoThuTu, hoanThanh = true, ghiChuGLy = "Đã hoàn thành khoá học", rowVersion = hv.RowVersion,
        });
        suaRes.StatusCode.Should().Be(HttpStatusCode.OK);
        var dsSauSua = await client.GetFromJsonAsync<List<HocVienDto>>($"/api/giao-ly/lop/{lopId}/hoc-vien");
        dsSauSua![0].HoanThanh.Should().BeTrue();

        // Xoá.
        var xoaRes = await client.DeleteAsync($"/api/giao-ly/hoc-vien/{hv.ChiTietId}");
        xoaRes.StatusCode.Should().Be(HttpStatusCode.OK);
        (await client.GetFromJsonAsync<List<HocVienDto>>($"/api/giao-ly/lop/{lopId}/hoc-vien")).Should().BeEmpty();
    }

    [Fact]
    public async Task Hoc_vien_da_thuoc_lop_khac_cung_khoi_thi_bi_chan()
    {
        var client = app.CreateAuthClient();
        var nguoiQuanLyId = await TaoGiaoDan("Quan ly cung khoi");
        var khoiId = await TaoKhoi(client, "Khối cùng khối", nguoiQuanLyId);
        var lop1 = await TaoLop(client, khoiId, "Lớp 1 cùng khối");
        var lop2 = await TaoLop(client, khoiId, "Lớp 2 cùng khối");
        var hocVienId = await TaoGiaoDan("Hoc vien hai lop cung khoi");

        (await client.PostAsJsonAsync($"/api/giao-ly/lop/{lop1}/hoc-vien", new { giaoDanId = hocVienId }))
            .StatusCode.Should().Be(HttpStatusCode.OK);

        var res = await client.PostAsJsonAsync($"/api/giao-ly/lop/{lop2}/hoc-vien", new { giaoDanId = hocVienId });
        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await res.Content.ReadFromJsonAsync<ThongBaoLoi>())!.ThongBao.Should().Be("Giáo dân này đã thuộc về lớp khác");
    }

    [Fact]
    public async Task Hoc_vien_thuoc_lop_o_khoi_khac_thi_KHONG_bi_chan()
    {
        var client = app.CreateAuthClient();
        var nguoiQuanLy1 = await TaoGiaoDan("Quan ly khoi 1");
        var nguoiQuanLy2 = await TaoGiaoDan("Quan ly khoi 2");
        var khoi1 = await TaoKhoi(client, "Khối A rieng biet", nguoiQuanLy1);
        var khoi2 = await TaoKhoi(client, "Khối B rieng biet", nguoiQuanLy2);
        var lop1 = await TaoLop(client, khoi1, "Lớp của khối A");
        var lop2 = await TaoLop(client, khoi2, "Lớp của khối B");
        var hocVienId = await TaoGiaoDan("Hoc vien hai khoi khac nhau");

        (await client.PostAsJsonAsync($"/api/giao-ly/lop/{lop1}/hoc-vien", new { giaoDanId = hocVienId }))
            .StatusCode.Should().Be(HttpStatusCode.OK);
        var res = await client.PostAsJsonAsync($"/api/giao-ly/lop/{lop2}/hoc-vien", new { giaoDanId = hocVienId });
        res.StatusCode.Should().Be(HttpStatusCode.OK, "mot giao dan duoc hoc dong thoi o hai khoi khac nhau, chi chan trong CUNG mot khoi");
    }

    // --- Giáo lý viên ------------------------------------------------------------------

    [Fact]
    public async Task Them_va_xoa_giao_ly_vien_khong_can_xac_nhan()
    {
        var client = app.CreateAuthClient();
        var nguoiQuanLyId = await TaoGiaoDan("Quan ly cho glv");
        var khoiId = await TaoKhoi(client, "Khối giáo lý viên", nguoiQuanLyId);
        var lopId = await TaoLop(client, khoiId, "Lớp giáo lý viên");
        var glvId = await TaoGiaoDan("Co giao day giao ly");

        var themRes = await client.PostAsJsonAsync($"/api/giao-ly/lop/{lopId}/giao-ly-vien", new { giaoDanId = glvId });
        themRes.StatusCode.Should().Be(HttpStatusCode.OK);

        var ds = await client.GetFromJsonAsync<List<GiaoLyVienDto>>($"/api/giao-ly/lop/{lopId}/giao-ly-vien");
        ds.Should().ContainSingle();

        var trungRes = await client.PostAsJsonAsync($"/api/giao-ly/lop/{lopId}/giao-ly-vien", new { giaoDanId = glvId });
        trungRes.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await trungRes.Content.ReadFromJsonAsync<ThongBaoLoi>())!.ThongBao.Should().Be("Giáo lý viên này đã tồn tại trong danh sách");

        var xoaRes = await client.DeleteAsync($"/api/giao-ly/giao-ly-vien/{ds![0].Id}");
        xoaRes.StatusCode.Should().Be(HttpStatusCode.OK);
        (await client.GetFromJsonAsync<List<GiaoLyVienDto>>($"/api/giao-ly/lop/{lopId}/giao-ly-vien")).Should().BeEmpty();
    }

    [Fact]
    public async Task Mot_giao_ly_vien_day_duoc_nhieu_lop()
    {
        var client = app.CreateAuthClient();
        var nguoiQuanLyId = await TaoGiaoDan("Quan ly nhieu lop");
        var khoiId = await TaoKhoi(client, "Khối nhiều lớp", nguoiQuanLyId);
        var lop1 = await TaoLop(client, khoiId, "Lớp X");
        var lop2 = await TaoLop(client, khoiId, "Lớp Y");
        var glvId = await TaoGiaoDan("Giao ly vien day 2 lop");

        (await client.PostAsJsonAsync($"/api/giao-ly/lop/{lop1}/giao-ly-vien", new { giaoDanId = glvId }))
            .StatusCode.Should().Be(HttpStatusCode.OK);
        (await client.PostAsJsonAsync($"/api/giao-ly/lop/{lop2}/giao-ly-vien", new { giaoDanId = glvId }))
            .StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // --- Cascade xoá ---------------------------------------------------------------------

    [Fact]
    public async Task Xoa_lop_thi_cascade_xoa_het_hoc_vien_va_giao_ly_vien()
    {
        var client = app.CreateAuthClient();
        var nguoiQuanLyId = await TaoGiaoDan("Quan ly cascade lop");
        var khoiId = await TaoKhoi(client, "Khối cascade lớp", nguoiQuanLyId);
        var lopId = await TaoLop(client, khoiId, "Lớp sẽ bị xoá");
        var hocVienId = await TaoGiaoDan("Hoc vien se mat theo lop");
        var glvId = await TaoGiaoDan("GLV se mat theo lop");
        await client.PostAsJsonAsync($"/api/giao-ly/lop/{lopId}/hoc-vien", new { giaoDanId = hocVienId });
        await client.PostAsJsonAsync($"/api/giao-ly/lop/{lopId}/giao-ly-vien", new { giaoDanId = glvId });

        var xoaRes = await client.DeleteAsync($"/api/giao-ly/lop/{lopId}");
        xoaRes.StatusCode.Should().Be(HttpStatusCode.OK);

        await using var db = app.TaoContextThuan();
        (await db.ChiTietLopGiaoLy.AnyAsync(c => c.LopGiaoLyId == lopId)).Should().BeFalse();
        (await db.GiaoLyVien.AnyAsync(g => g.LopGiaoLyId == lopId)).Should().BeFalse();
    }

    [Fact]
    public async Task Xoa_khoi_thi_cascade_xoa_het_lop_hoc_vien_giao_ly_vien()
    {
        var client = app.CreateAuthClient();
        var nguoiQuanLyId = await TaoGiaoDan("Quan ly cascade khoi");
        var khoiId = await TaoKhoi(client, "Khối sẽ bị xoá toàn bộ", nguoiQuanLyId);
        var lopId = await TaoLop(client, khoiId, "Lớp con của khối bị xoá");
        var hocVienId = await TaoGiaoDan("Hoc vien mat theo khoi");
        await client.PostAsJsonAsync($"/api/giao-ly/lop/{lopId}/hoc-vien", new { giaoDanId = hocVienId });

        var xoaRes = await client.DeleteAsync($"/api/giao-ly/khoi/{khoiId}");
        xoaRes.StatusCode.Should().Be(HttpStatusCode.OK);

        var dsKhoi = await client.GetFromJsonAsync<List<KhoiDto>>("/api/giao-ly/khoi");
        dsKhoi!.Should().NotContain(k => k.Id == khoiId);

        await using var db = app.TaoContextThuan();
        (await db.LopGiaoLy.AnyAsync(l => l.Id == lopId)).Should().BeFalse();
        (await db.ChiTietLopGiaoLy.AnyAsync(c => c.LopGiaoLyId == lopId)).Should().BeFalse();

        var xoaLai = await client.DeleteAsync($"/api/giao-ly/khoi/{khoiId}");
        xoaLai.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
