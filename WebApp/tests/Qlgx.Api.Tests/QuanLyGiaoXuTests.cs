using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Qlgx.Api.Dtos;
using Qlgx.Domain.Entities;

namespace Qlgx.Api.Tests;

/// <summary>
/// Màn hình "Quản lý giáo phận/giáo hạt/giáo xứ" (xem
/// docs/superpowers/specs/man-hinh/quan-ly-giao-xu.md mục 4) — GiaoPhan/GiaoHat/GiaoXu KHÔNG
/// có giao_xu_id nên KHÔNG có RLS bảo vệ; policy "QuanTriHeThong" ở tầng ứng dụng là lớp
/// phòng thủ DUY NHẤT chống một Quản trị viên giáo xứ (LoaiTaiKhoan=0) xem/sửa được giáo xứ
/// khác. Test then_choi này CHỨNG MINH ranh giới đó — trước khi thêm policy riêng, các route
/// /api/quan-tri/* nếu chỉ bọc bằng policy "QuanTri" cũ thì Quản trị viên giáo xứ A sẽ vào
/// được thẳng (xem ghi chú "ĐỎ TRƯỚC KHI SỬA" trong báo cáo nhiệm vụ — đã tự kiểm bằng cách
/// đổi tạm RequireAuthorization("QuanTriHeThong") thành ("QuanTri") ở
/// QuanLyGiaoXuEndpoints.cs, chạy lại đúng bộ test này, thấy 403 kỳ vọng ở dưới CHUYỂN thành
/// 200 — tức Quản trị viên giáo xứ A xem/sửa được giáo xứ B — rồi trả lại "QuanTriHeThong").
/// </summary>
public class QuanLyGiaoXuTests(QlgxApiFactory app) : IClassFixture<QlgxApiFactory>
{
    [Theory]
    [InlineData("/api/quan-tri/giao-phan")]
    [InlineData("/api/quan-tri/giao-hat")]
    [InlineData("/api/quan-tri/giao-xu")]
    public async Task Quan_tri_vien_thuong_mot_giao_xu_bi_chan_403_khong_vao_duoc_man_hinh_quan_ly_giao_xu(string duong)
    {
        // LoaiTaiKhoan=0 — Quan tri vien THUONG cua giao xu cua chinh minh (app.GiaoXuId), KHONG
        // phai LoaiTaiKhoan=9 (Quan tri he thong). Day la nguoi dung nguy hiem nhat can chan:
        // ho la quan tri vien THAT, chi khong duoc phep xem/sua giao xu KHAC.
        var clientQuanTriThuong = app.CreateAuthClient(app.GiaoXuId, loaiTaiKhoan: 0);

        var res = await clientQuanTriThuong.GetAsync(duong);

        res.StatusCode.Should().Be(HttpStatusCode.Forbidden,
            $"Quan tri vien mot giao xu KHONG duoc phep vao man hinh quan ly xuyen giao xu ({duong})");
    }

    [Fact]
    public async Task Nguoi_chua_dang_nhap_bi_chan_401_o_man_hinh_quan_ly_giao_xu()
    {
        var res = await app.CreateClient().GetAsync("/api/quan-tri/giao-xu");

        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Quan_tri_he_thong_xem_duoc_danh_sach_giao_xu_xuyen_toan_may_chu()
    {
        var giaoXuKhac = Guid.NewGuid();
        await using (var db = app.TaoContextThuan())
        {
            db.GiaoXu.Add(new GiaoXu { Id = giaoXuKhac, TenGiaoXu = "Giao xu quan ly test", MaGiaoXuCu = 88801 });
            await db.SaveChangesAsync();
        }
        var clientHeThong = app.CreateAuthClient(app.GiaoXuId, loaiTaiKhoan: 9);

        var res = await clientHeThong.GetAsync("/api/quan-tri/giao-xu");

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        var ds = await res.Content.ReadFromJsonAsync<List<GiaoXuDto>>();
        ds!.Should().Contain(x => x.Id == giaoXuKhac);
        ds!.Should().Contain(x => x.Id == app.GiaoXuId);
    }

    [Fact]
    public async Task Quan_tri_he_thong_tao_duoc_giao_phan_giao_hat_giao_xu_moi()
    {
        var client = app.CreateAuthClient(app.GiaoXuId, loaiTaiKhoan: 9);

        var resPhan = await client.PostAsJsonAsync("/api/quan-tri/giao-phan",
            new TaoGiaoPhanRequest("Giao phan test", null));
        resPhan.StatusCode.Should().Be(HttpStatusCode.Created);
        var idPhan = Guid.Parse(resPhan.Headers.Location!.ToString().Split('/').Last());

        var resHat = await client.PostAsJsonAsync("/api/quan-tri/giao-hat",
            new TaoGiaoHatRequest(idPhan, "Giao hat test", null));
        resHat.StatusCode.Should().Be(HttpStatusCode.Created);
        var idHat = Guid.Parse(resHat.Headers.Location!.ToString().Split('/').Last());

        var resXu = await client.PostAsJsonAsync("/api/quan-tri/giao-xu",
            new TaoGiaoXuRequest(idHat, "Giao xu test moi", "Dia chi test", null, null, null, null));
        resXu.StatusCode.Should().Be(HttpStatusCode.Created);

        await using var db = app.TaoContextThuan();
        (await db.GiaoXu.AnyAsync(x => x.TenGiaoXu == "Giao xu test moi")).Should().BeTrue();
    }

    [Fact]
    public async Task Quan_tri_he_thong_tao_duoc_tai_khoan_quan_tri_dau_tien_cho_giao_xu_khac_va_tai_khoan_do_dang_nhap_duoc()
    {
        var giaoXuMoi = Guid.NewGuid();
        await using (var db = app.TaoContextThuan())
        {
            db.GiaoXu.Add(new GiaoXu { Id = giaoXuMoi, TenGiaoXu = "Giao xu can tao tai khoan", MaGiaoXuCu = 88802 });
            await db.SaveChangesAsync();
        }
        var clientHeThong = app.CreateAuthClient(app.GiaoXuId, loaiTaiKhoan: 9);

        var res = await clientHeThong.PostAsJsonAsync($"/api/quan-tri/giao-xu/{giaoXuMoi}/tai-khoan",
            new TaoTaiKhoanChoGiaoXuRequest("quantri_xu_moi", "MatKhauManh123!", "Quan tri Xu Moi", null, null));

        res.StatusCode.Should().Be(HttpStatusCode.Created);

        var dangNhap = await app.CreateClient().PostAsJsonAsync("/api/auth/dang-nhap",
            new DangNhapRequest("quantri_xu_moi", "MatKhauManh123!"));
        dangNhap.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Quan_tri_vien_thuong_khong_tao_duoc_tai_khoan_cho_giao_xu_khac()
    {
        var giaoXuKhac = Guid.NewGuid();
        await using (var db = app.TaoContextThuan())
        {
            db.GiaoXu.Add(new GiaoXu { Id = giaoXuKhac, TenGiaoXu = "Giao xu B tao tai khoan test", MaGiaoXuCu = 88803 });
            await db.SaveChangesAsync();
        }
        var clientQuanTriThuong = app.CreateAuthClient(app.GiaoXuId, loaiTaiKhoan: 0);

        var res = await clientQuanTriThuong.PostAsJsonAsync($"/api/quan-tri/giao-xu/{giaoXuKhac}/tai-khoan",
            new TaoTaiKhoanChoGiaoXuRequest("khong_duoc_tao", "MatKhauManh123!", "X", null, null));

        res.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
