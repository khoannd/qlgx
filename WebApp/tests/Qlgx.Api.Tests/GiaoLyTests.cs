using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Qlgx.Api.Services;
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

    /// <summary>Tạo một giáo dân tối giản cho test. Cấp MaGiaoDanCu qua CHÍNH
    /// <see cref="SinhMaService"/> (bảng đếm nguyên tử <c>bo_dem_ma</c>, cùng cơ chế
    /// NhapHocVienGiaoLyService.ThucHien dùng khi tự tạo giáo dân mới từ Excel) — KHÔNG dùng một
    /// bộ đếm tĩnh riêng của test nữa: bộ đếm riêng cũ (Interlocked, độc lập MAX(MaGiaoDanCu)
    /// thật trong CSDL) từng đụng độ THẬT với SinhMaService — xunit chạy các [Fact] của lớp
    /// test này XEN KẼ nhau (không tuần tự như tưởng — đã xác nhận bằng log), nên khi
    /// SinhMaService đọc MAX tại đúng lúc bộ đếm test đã ghi xong giá trị NGAY TRƯỚC giá trị nó
    /// SẮP dùng, cả hai cùng ra đúng MỘT số → lỗi 23505 trên
    /// "ix_giao_dan_giao_xu_id_ma_giao_dan_cu". Dùng chung một nguồn cấp mã nguyên tử loại bỏ
    /// hẳn khe hở này.</summary>
    private async Task<Guid> TaoGiaoDan(string hoTen)
    {
        await using var db = app.TaoContextThuan();
        var sinhMa = new SinhMaService(db);
        var maxHienCo = await db.GiaoDan.Where(x => x.GiaoXuId == app.GiaoXuId).MaxAsync(x => (int?)x.MaGiaoDanCu, default) ?? 0;
        var ma = await sinhMa.LayMaTiepTheo(app.GiaoXuId, "giao_dan", maxHienCo, default);
        var gd = new GiaoDan { GiaoXuId = app.GiaoXuId, MaGiaoDanCu = ma, HoTen = hoTen };
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

    // --- "Chuyển lớp" hàng loạt (frmChuyenLop.cs) ---------------------------------------

    private sealed record ChuyenLopXemTruocDto(int SoLuongDaChon, int SoLuongSeChuyen,
        int SoLuongDaCoODichRoi, string TenLopNguon, string TenLopDich, string TenKhoiDich, int? NamDich);
    private sealed record ChuyenLopKetQuaDto(int SoLuongDaChuyen);

    [Fact]
    public async Task Chuyen_lop_xem_truoc_roi_ghi_khong_xoa_khoi_lop_nguon_va_bo_qua_trung()
    {
        var client = app.CreateAuthClient();
        var nguoiQuanLyId = await TaoGiaoDan("Quan ly chuyen lop");
        var khoiId = await TaoKhoi(client, "Khối chuyển lớp", nguoiQuanLyId);
        var lopNguonId = await TaoLop(client, khoiId, "Lớp nguồn");
        // Lớp đích ở một KHỐI KHÁC — ThemHocVien (frmLopGiaoLy.cs) chặn một giáo dân thuộc
        // hai lớp CÙNG khối cùng lúc (KetQuaThemHocVien.DaThuocLopKhac), nên để dựng được tình
        // huống "hv2 đã có sẵn trong lớp đích" bằng chính đường thêm-từng-người, lớp đích phải
        // khác khối với lớp nguồn — quy tắc đó không áp dụng cho ChuyenLop (xem chú thích dài ở
        // GiaoLyService.ChuyenLop, cố tình không kiểm tra).
        var khoiDichId = await TaoKhoi(client, "Khối chuyển lớp - đích", nguoiQuanLyId);
        var lopDichId = await TaoLop(client, khoiDichId, "Lớp đích");

        var hv1 = await TaoGiaoDan("Hoc vien chuyen 1");
        var hv2 = await TaoGiaoDan("Hoc vien chuyen 2 da co o dich");
        (await client.PostAsJsonAsync($"/api/giao-ly/lop/{lopNguonId}/hoc-vien", new { giaoDanId = hv1 }))
            .StatusCode.Should().Be(HttpStatusCode.OK);
        (await client.PostAsJsonAsync($"/api/giao-ly/lop/{lopNguonId}/hoc-vien", new { giaoDanId = hv2 }))
            .StatusCode.Should().Be(HttpStatusCode.OK);
        // hv2 đã có sẵn trong lớp đích — chuyển lớp phải BỎ QUA (đúng bản gốc), không lỗi.
        (await client.PostAsJsonAsync($"/api/giao-ly/lop/{lopDichId}/hoc-vien", new { giaoDanId = hv2 }))
            .StatusCode.Should().Be(HttpStatusCode.OK);

        var dsNguon = await client.GetFromJsonAsync<List<HocVienDto>>($"/api/giao-ly/lop/{lopNguonId}/hoc-vien");
        var chiTietIds = dsNguon!.Select(h => h.ChiTietId).ToList();

        var xemTruocRes = await client.PostAsJsonAsync("/api/giao-ly/chuyen-lop/xem-truoc",
            new { chiTietIds, lopDichId });
        xemTruocRes.StatusCode.Should().Be(HttpStatusCode.OK);
        var xemTruoc = await xemTruocRes.Content.ReadFromJsonAsync<ChuyenLopXemTruocDto>();
        xemTruoc!.SoLuongDaChon.Should().Be(2);
        xemTruoc.SoLuongSeChuyen.Should().Be(1);
        xemTruoc.SoLuongDaCoODichRoi.Should().Be(1);
        xemTruoc.TenLopNguon.Should().Be("Lớp nguồn");
        xemTruoc.TenLopDich.Should().Be("Lớp đích");

        var ghiRes = await client.PostAsJsonAsync("/api/giao-ly/chuyen-lop", new { chiTietIds, lopDichId });
        ghiRes.StatusCode.Should().Be(HttpStatusCode.OK);
        var ketQua = await ghiRes.Content.ReadFromJsonAsync<ChuyenLopKetQuaDto>();
        ketQua!.SoLuongDaChuyen.Should().Be(1);

        // KHÔNG xoá khỏi lớp nguồn — bug-for-bug đúng bản gốc (tên "Chuyển lớp" nhưng thực chất
        // là THÊM vào lớp đích).
        (await client.GetFromJsonAsync<List<HocVienDto>>($"/api/giao-ly/lop/{lopNguonId}/hoc-vien"))
            .Should().HaveCount(2);
        var dsDich = await client.GetFromJsonAsync<List<HocVienDto>>($"/api/giao-ly/lop/{lopDichId}/hoc-vien");
        dsDich!.Select(h => h.GiaoDanId).Should().BeEquivalentTo([hv1, hv2]);
    }

    [Fact]
    public async Task Chuyen_lop_khong_co_hoc_vien_nao_chon_thi_bao_loi()
    {
        var client = app.CreateAuthClient();
        var res = await client.PostAsJsonAsync("/api/giao-ly/chuyen-lop/xem-truoc",
            new { chiTietIds = Array.Empty<Guid>(), lopDichId = Guid.NewGuid() });
        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // --- "Nhập học viên hàng loạt" từ Excel (frmImportHocVien.cs) -----------------------

    private sealed record DongNhapDto(int SoDong, string? MaGD, string? TenThanh, string HoTen,
        string? Phai, string? NgaySinhHienThi, string? GiaoHo, string? GhiChu, bool DaHocXong,
        bool LaGiaoDanMoi, string? Loi);
    private sealed record NhapXemTruocDto(bool TepHopLe, string? LoiTep, string TenLop,
        List<DongNhapDto> Dong, int SoSeNhap, int SoBiBoQua);
    private sealed record NhapKetQuaDto(int SoDaNhap, int SoBiBoQua);

    private static byte[] TaoExcelHocVien(params (string? maGD, string? tenThanh, string hoTen,
        string phai, string ngaySinh, string? giaoHo, string? ghiChu, string? daHocXong)[] hang)
    {
        using var wb = new ClosedXML.Excel.XLWorkbook();
        var ws = wb.Worksheets.Add("Sheet1");
        string[] cot = ["Mã GD", "Tên thánh", "Họ tên", "Phái", "Ngày sinh", "Giáo họ", "Ghi chú", "Đã học xong"];
        for (var i = 0; i < cot.Length; i++) ws.Cell(1, i + 1).Value = cot[i];
        for (var r = 0; r < hang.Length; r++)
        {
            var h = hang[r];
            ws.Cell(r + 2, 1).Value = h.maGD ?? "";
            ws.Cell(r + 2, 2).Value = h.tenThanh ?? "";
            ws.Cell(r + 2, 3).Value = h.hoTen;
            ws.Cell(r + 2, 4).Value = h.phai;
            ws.Cell(r + 2, 5).Value = h.ngaySinh;
            ws.Cell(r + 2, 6).Value = h.giaoHo ?? "";
            ws.Cell(r + 2, 7).Value = h.ghiChu ?? "";
            ws.Cell(r + 2, 8).Value = h.daHocXong ?? "";
        }
        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }

    private static MultipartFormDataContent TaoFormTep(byte[] tep, string tenTep = "hoc-vien.xlsx")
    {
        var noiDung = new ByteArrayContent(tep);
        noiDung.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        var form = new MultipartFormDataContent { { noiDung, "tep", tenTep } };
        return form;
    }

    [Fact]
    public async Task Nhap_hoc_vien_tu_excel_xem_truoc_roi_ghi_that()
    {
        var client = app.CreateAuthClient();
        var nguoiQuanLyId = await TaoGiaoDan("Quan ly nhap hoc vien");
        var khoiId = await TaoKhoi(client, "Khối nhập Excel", nguoiQuanLyId);
        var lopId = await TaoLop(client, khoiId, "Lớp nhập Excel");

        // Một giáo dân có sẵn (khớp bằng Họ tên+Phái+Ngày sinh), một dòng thiếu Phái (lỗi, bị bỏ
        // qua), một giáo dân MỚI (không khớp ai, không có "Mã GD").
        var coSanId = await TaoGiaoDan("Nguyen Van Co San");
        await using (var db = app.TaoContextThuan())
        {
            var gd = await db.GiaoDan.SingleAsync(x => x.Id == coSanId);
            gd.Phai = "Nam"; gd.NgaySinh = new DateOnly(2010, 3, 15);
            await db.SaveChangesAsync();
        }

        var tep = TaoExcelHocVien(
            (null, null, "Nguyen Van Co San", "Nam", "15/03/2010", null, "Ghi chu co san", null),
            (null, null, "Thieu Phai", "", "01/01/2011", null, null, null),
            (null, "Giuse", "Nguyen Van Moi Tinh", "Nam", "20/05/2012", null, "Hoc vien moi", "x"));

        var xemTruocRes = await client.PostAsync($"/api/giao-ly/lop/{lopId}/nhap-hoc-vien/xem-truoc", TaoFormTep(tep));
        xemTruocRes.StatusCode.Should().Be(HttpStatusCode.OK);
        var xemTruoc = await xemTruocRes.Content.ReadFromJsonAsync<NhapXemTruocDto>();
        xemTruoc!.TepHopLe.Should().BeTrue();
        xemTruoc.SoSeNhap.Should().Be(2);
        xemTruoc.SoBiBoQua.Should().Be(1);
        xemTruoc.Dong.Should().HaveCount(3);
        xemTruoc.Dong[0].LaGiaoDanMoi.Should().BeFalse();
        xemTruoc.Dong[1].Loi.Should().NotBeNullOrEmpty();
        xemTruoc.Dong[2].LaGiaoDanMoi.Should().BeTrue();

        var ghiRes = await client.PostAsync($"/api/giao-ly/lop/{lopId}/nhap-hoc-vien", TaoFormTep(tep));
        ghiRes.StatusCode.Should().Be(HttpStatusCode.OK);
        var ketQua = await ghiRes.Content.ReadFromJsonAsync<NhapKetQuaDto>();
        ketQua!.SoDaNhap.Should().Be(2);
        ketQua.SoBiBoQua.Should().Be(1);

        var ds = await client.GetFromJsonAsync<List<HocVienDto>>($"/api/giao-ly/lop/{lopId}/hoc-vien");
        ds.Should().HaveCount(2);
        ds!.Should().Contain(h => h.GiaoDanId == coSanId && h.GhiChuGLy == "Ghi chu co san");
        var moi = ds.Single(h => h.GiaoDanId != coSanId);
        moi.HoTen.Should().Be("Nguyen Van Moi Tinh");
        moi.HoanThanh.Should().BeTrue();

        await using var dbSau = app.TaoContextThuan();
        (await dbSau.GiaoDan.CountAsync(g => g.HoTen == "Nguyen Van Moi Tinh")).Should().Be(1);
    }

    [Fact]
    public async Task Nhap_hoc_vien_tu_tep_khong_phai_excel_thi_bao_loi_dinh_dang()
    {
        var client = app.CreateAuthClient();
        var nguoiQuanLyId = await TaoGiaoDan("Quan ly nhap tep sai");
        var khoiId = await TaoKhoi(client, "Khối tệp sai", nguoiQuanLyId);
        var lopId = await TaoLop(client, khoiId, "Lớp tệp sai");

        var tepGia = "not an excel file"u8.ToArray();
        var res = await client.PostAsync($"/api/giao-ly/lop/{lopId}/nhap-hoc-vien/xem-truoc", TaoFormTep(tepGia));
        res.StatusCode.Should().Be(HttpStatusCode.OK);
        var kq = await res.Content.ReadFromJsonAsync<NhapXemTruocDto>();
        kq!.TepHopLe.Should().BeFalse();
        kq.LoiTep.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Tai_mau_excel_hoc_vien_tra_ve_tep()
    {
        var client = app.CreateAuthClient();
        var res = await client.GetAsync("/api/giao-ly/nhap-hoc-vien/mau-excel");
        res.StatusCode.Should().Be(HttpStatusCode.OK);
        (await res.Content.ReadAsByteArrayAsync()).Length.Should().BeGreaterThan(0);
    }
}
