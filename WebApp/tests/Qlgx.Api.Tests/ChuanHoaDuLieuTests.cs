using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Qlgx.Api.Dtos;
using Qlgx.Domain.Entities;

namespace Qlgx.Api.Tests;

/// <summary>
/// "Chuẩn hoá dữ liệu" (docs/superpowers/specs/man-hinh/cong-cu-du-lieu.md mục 5.1) — công cụ
/// sửa dữ liệu hàng loạt. Test đúng 4 nguyên tắc: xem trước có số liệu thật, ghi thật đúng
/// thuật toán viết-hoa-chữ-cái-đầu, không đụng cột mới của web, và MỘT transaction.
/// </summary>
public class ChuanHoaDuLieuTests(QlgxApiFactory app) : IClassFixture<QlgxApiFactory>
{
    // Bản SAO độc lập của đúng danh sách cột CotGiaoDan/CotGiaDinh trong ChuanHoaDuLieuService
    // (private, không đọc được từ test qua InternalsVisibleTo) — dùng để dựng dữ liệu chạm ĐỦ
    // từng cột rồi khoá cứng số lượng/tên cột PHẢI khớp trong kết quả xem trước. Nếu ai vô tình
    // xoá một cột (ví dụ DiaChi) khỏi danh sách thật trong service, test dưới đây phải ĐỎ vì
    // dòng vừa dựng vẫn có DiaChi bị đổi giá trị thật nhưng service không còn báo cáo nó nữa.
    private static readonly string[] TenCotGiaoDan =
    [
        "HoTen", "TenThanh", "Phai", "NoiSinh", "CMND", "DanToc", "ThuocGiaoXu", "ThuocGiaoPhan",
        "DiaChi", "DienThoai", "Email", "HoTenCha", "HoTenMe", "NoiRuaToi", "ChaRuaToi",
        "NguoiDoDauRuaToi", "NoiRuocLe", "ChaRuocLe", "NoiThemSuc", "ChaThemSuc",
        "NguoiDoDauThemSuc", "NguoiXucDau", "TinhTrangXucDau", "GhiChuXucDau", "NoiBD1", "NoiBD2",
        "NoiTHVaoDoi", "NoiGLHN", "NguoiChungNhanGLHN", "XepLoaiGLHN", "TrinhDoVanHoa",
        "TrinhDoChuyenMon", "BietNgoaiNgu", "NgheNghiep", "NoiQuaDoi", "NoiAnTang",
    ];

    private static readonly string[] TenCotGiaDinh =
        ["MaGiaDinhRieng", "TenGiaDinh", "DienThoai", "DiaChi", "DienGiaDinh"];

    /// <summary>
    /// LỖ HỔNG TEST 2 (review-toan-nhanh-test.md mục 1c) — trước bản sửa này, 7/7 test đều xanh
    /// dù đột biến xoá hẳn cột DiaChi khỏi CotGiaoDan: không test nào lặp qua toàn bộ danh sách
    /// cột hay khẳng định riêng DiaChi. Dựng MỘT giáo dân có ĐỦ 36 cột trong TenCotGiaoDan bị
    /// đặt giá trị chưa chuẩn hoá (chữ thường), rồi khoá cứng: tập TÊN CỘT trả về trong xem
    /// trước phải khớp CHÍNH XÁC danh sách 36 cột — thiếu một cột nào (kể cả DiaChi) là ĐỎ.
    ///
    /// BẰNG CHỨNG ĐỎ→XANH (task-sua-review-backend-2.md): xoá dòng ("DiaChi", ...) khỏi
    /// CotGiaoDan trong ChuanHoaDuLieuService.cs (đúng đột biến của review-toan-nhanh-test.md)
    /// làm test này ĐỎ (tập tên cột trả về chỉ còn 35, thiếu "DiaChi") — đã chạy thật để xác
    /// nhận, rồi khôi phục nguyên văn.
    /// </summary>
    [Fact]
    public async Task Xem_truoc_giao_dan_khoa_cung_du_36_cot_bao_gom_dia_chi()
    {
        await using var db = app.TaoContextThuan();
        var gd = new GiaoDan { GiaoXuId = app.GiaoXuId, MaGiaoDanCu = 94060, HoTen = "" };
        var kieu = typeof(GiaoDan);
        foreach (var ten in TenCotGiaoDan)
            kieu.GetProperty(ten)!.SetValue(gd, "gia tri chua chuan hoa");
        db.GiaoDan.Add(gd);
        await db.SaveChangesAsync();

        var client = app.CreateAuthClient();
        var res = await client.PostAsync("/api/cong-cu-du-lieu/chuan-hoa/giao-dan/xem-truoc", null);
        res.EnsureSuccessStatusCode();
        var kq = await res.Content.ReadFromJsonAsync<ChuanHoaXemTruocKetQua>();

        var dong = kq!.MauThayDoi.Should().ContainSingle(d => d.Id == gd.Id).Subject;
        dong.Truong.Select(t => t.TenTruong).Should().BeEquivalentTo(TenCotGiaoDan,
            "khoa cung DUNG danh sach cot duoc chuan hoa - ai vo tinh xoa mot cot (vi du DiaChi) " +
            "khoi CotGiaoDan trong service phai bi test nay bat duoc, khong duoc am tham lot qua");
        dong.Truong.Should().Contain(t => t.TenTruong == "DiaChi" && t.GiaTriMoi == "Gia Tri Chua Chuan Hoa",
            "DiaChi phai nam trong pham vi cot duoc chuan hoa, dung nhu tai lieu lop cam ket");

        // Don dep: ghi that de chuan hoa lai dong vua tao - CSDL dung chung giua cac [Fact] trong
        // CUNG mot lop (IClassFixture), de lai dong CHUA chuan hoa se lam sai SoBanGhiSeDoi cua
        // cac test khac (dem TOAN BO giao dan cua giao xu, khong loc rieng theo dong vua tao).
        (await client.PostAsync("/api/cong-cu-du-lieu/chuan-hoa/giao-dan", null)).EnsureSuccessStatusCode();
    }

    /// <summary>Cùng dạng khoá cứng như trên nhưng cho CotGiaDinh (5 cột) — bao gồm DiaChi.</summary>
    [Fact]
    public async Task Xem_truoc_gia_dinh_khoa_cung_du_5_cot_bao_gom_dia_chi()
    {
        await using var db = app.TaoContextThuan();
        var gdinh = new GiaDinh { GiaoXuId = app.GiaoXuId, MaGiaDinhCu = 94061, TenGiaDinh = "" };
        var kieu = typeof(GiaDinh);
        foreach (var ten in TenCotGiaDinh)
            kieu.GetProperty(ten)!.SetValue(gdinh, "gia tri chua chuan hoa");
        db.GiaDinh.Add(gdinh);
        await db.SaveChangesAsync();

        var client = app.CreateAuthClient();
        var res = await client.PostAsync("/api/cong-cu-du-lieu/chuan-hoa/gia-dinh/xem-truoc", null);
        res.EnsureSuccessStatusCode();
        var kq = await res.Content.ReadFromJsonAsync<ChuanHoaXemTruocKetQua>();

        var dong = kq!.MauThayDoi.Should().ContainSingle(d => d.Id == gdinh.Id).Subject;
        dong.Truong.Select(t => t.TenTruong).Should().BeEquivalentTo(TenCotGiaDinh,
            "khoa cung DUNG danh sach cot duoc chuan hoa cua gia dinh, bao gom DiaChi");

        // Don dep: xem chu thich o test GiaoDan tuong ung o tren.
        (await client.PostAsync("/api/cong-cu-du-lieu/chuan-hoa/gia-dinh", null)).EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Xem_truoc_giao_dan_dem_dung_so_ban_ghi_se_doi_khong_ghi_gi()
    {
        await using var db = app.TaoContextThuan();
        var dungRoi = new GiaoDan { GiaoXuId = app.GiaoXuId, MaGiaoDanCu = 94001, HoTen = "Nguyễn Văn An" };
        var saiThuongHoa = new GiaoDan { GiaoXuId = app.GiaoXuId, MaGiaoDanCu = 94002, HoTen = "nguyễn VĂN an" };
        db.GiaoDan.AddRange(dungRoi, saiThuongHoa);
        await db.SaveChangesAsync();

        var client = app.CreateAuthClient();
        var res = await client.PostAsync("/api/cong-cu-du-lieu/chuan-hoa/giao-dan/xem-truoc", null);
        res.EnsureSuccessStatusCode();
        var kq = await res.Content.ReadFromJsonAsync<ChuanHoaXemTruocKetQua>();

        kq!.SoBanGhiSeDoi.Should().Be(1);
        var mau = kq.MauThayDoi.Should().ContainSingle().Subject;
        mau.Truong.Should().ContainSingle(t => t.TenTruong == "HoTen" && t.GiaTriMoi == "Nguyễn Văn An");

        // Xem truoc khong duoc ghi gi xuong CSDL
        await using var kiemTra = app.TaoContextThuan();
        (await kiemTra.GiaoDan.SingleAsync(g => g.Id == saiThuongHoa.Id)).HoTen.Should().Be("nguyễn VĂN an");
    }

    [Fact]
    public async Task Ghi_that_doi_dung_ho_ten_va_tra_dung_so_ban_ghi_da_doi()
    {
        await using var db = app.TaoContextThuan();
        var gd = new GiaoDan { GiaoXuId = app.GiaoXuId, MaGiaoDanCu = 94010, HoTen = "trần   thị   bình" };
        db.GiaoDan.Add(gd);
        await db.SaveChangesAsync();

        var client = app.CreateAuthClient();
        var res = await client.PostAsync("/api/cong-cu-du-lieu/chuan-hoa/giao-dan", null);
        res.EnsureSuccessStatusCode();
        var kq = await res.Content.ReadFromJsonAsync<ChuanHoaKetQua>();
        kq!.SoBanGhiDaDoi.Should().Be(1);

        await using var kiemTra = app.TaoContextThuan();
        // Khoang trang thua giua cac tu bi gop lai dung 1 dau cach (dung ham goc cua desktop).
        (await kiemTra.GiaoDan.SingleAsync(g => g.Id == gd.Id)).HoTen.Should().Be("Trần Thị Bình");
    }

    [Fact]
    public async Task Ghi_chu_khong_bi_dam_vao_nhung_ghi_chu_xuc_dau_thi_co_dung_bug_ban_goc()
    {
        await using var db = app.TaoContextThuan();
        var gd = new GiaoDan
        {
            GiaoXuId = app.GiaoXuId, MaGiaoDanCu = 94020, HoTen = "Le Van C",
            GhiChu = "ghi chú kiểu thường không được đổi",
            GhiChuXucDau = "ghi chú xức dầu kiểu thường",
        };
        db.GiaoDan.Add(gd);
        await db.SaveChangesAsync();

        var client = app.CreateAuthClient();
        (await client.PostAsync("/api/cong-cu-du-lieu/chuan-hoa/giao-dan", null)).EnsureSuccessStatusCode();

        await using var kiemTra = app.TaoContextThuan();
        var sau = await kiemTra.GiaoDan.SingleAsync(g => g.Id == gd.Id);
        sau.GhiChu.Should().Be("ghi chú kiểu thường không được đổi", "GhiChu bi loai tru dung ten (khop chinh xac ban goc)");
        sau.GhiChuXucDau.Should().Be("Ghi Chú Xức Dầu Kiểu Thường",
            "GhiChuXucDau KHONG duoc loai tru o ban goc (chi \"GhiChu\" moi khop chinh xac) - tai hien dung bug nay");
    }

    [Fact]
    public async Task Khong_dung_tay_vao_cot_moi_chi_co_o_web()
    {
        await using var db = app.TaoContextThuan();
        var gd = new GiaoDan
        {
            GiaoXuId = app.GiaoXuId, MaGiaoDanCu = 94030, HoTen = "Test",
            AnhDaiDienLoaiNoiDung = "image/jpeg",
        };
        db.GiaoDan.Add(gd);
        await db.SaveChangesAsync();

        var client = app.CreateAuthClient();
        (await client.PostAsync("/api/cong-cu-du-lieu/chuan-hoa/giao-dan", null)).EnsureSuccessStatusCode();

        await using var kiemTra = app.TaoContextThuan();
        (await kiemTra.GiaoDan.SingleAsync(g => g.Id == gd.Id)).AnhDaiDienLoaiNoiDung.Should().Be("image/jpeg");
    }

    [Fact]
    public async Task Tu_bat_dau_bang_ky_tu_dac_biet_duoc_giu_nguyen()
    {
        await using var db = app.TaoContextThuan();
        var gd = new GiaoDan { GiaoXuId = app.GiaoXuId, MaGiaoDanCu = 94040, HoTen = "nguyễn (chị) lan" };
        db.GiaoDan.Add(gd);
        await db.SaveChangesAsync();

        var client = app.CreateAuthClient();
        (await client.PostAsync("/api/cong-cu-du-lieu/chuan-hoa/giao-dan", null)).EnsureSuccessStatusCode();

        await using var kiemTra = app.TaoContextThuan();
        (await kiemTra.GiaoDan.SingleAsync(g => g.Id == gd.Id)).HoTen.Should().Be("Nguyễn (chị) Lan");
    }

    [Fact]
    public async Task Gia_dinh_xem_truoc_va_ghi_that_doi_dung_ten_gia_dinh()
    {
        await using var db = app.TaoContextThuan();
        var gdinh = new GiaDinh { GiaoXuId = app.GiaoXuId, MaGiaDinhCu = 94050, TenGiaDinh = "gia đình VĂN a" };
        db.GiaDinh.Add(gdinh);
        await db.SaveChangesAsync();

        var client = app.CreateAuthClient();
        var xemTruoc = await client.PostAsync("/api/cong-cu-du-lieu/chuan-hoa/gia-dinh/xem-truoc", null);
        var kqXem = await xemTruoc.Content.ReadFromJsonAsync<ChuanHoaXemTruocKetQua>();
        kqXem!.SoBanGhiSeDoi.Should().Be(1);

        var ghiThat = await client.PostAsync("/api/cong-cu-du-lieu/chuan-hoa/gia-dinh", null);
        var kqGhi = await ghiThat.Content.ReadFromJsonAsync<ChuanHoaKetQua>();
        kqGhi!.SoBanGhiDaDoi.Should().Be(1);

        await using var kiemTra = app.TaoContextThuan();
        (await kiemTra.GiaDinh.SingleAsync(g => g.Id == gdinh.Id)).TenGiaDinh.Should().Be("Gia Đình Văn A");
    }

    [Fact]
    public async Task Nguoi_chua_dang_nhap_bi_chan_401()
    {
        var res = await app.CreateClient().PostAsync("/api/cong-cu-du-lieu/chuan-hoa/giao-dan/xem-truoc", null);

        res.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }
}
