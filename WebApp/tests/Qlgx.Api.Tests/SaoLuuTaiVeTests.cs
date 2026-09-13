using System.Net;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Qlgx.Domain.Entities;

namespace Qlgx.Api.Tests;

public class SaoLuuTaiVeTests(QlgxApiFactory factory) : IClassFixture<QlgxApiFactory>
{
    [Fact]
    public async Task Cong_viec_chua_xong_thi_khong_tai_duoc()
    {
        var id = await TaoCongViecTaiVe(factory, TrangThaiCongViec.DangChay, taoTep: false);

        var res = await factory.CreateAuthClient(loaiTaiKhoan: 9).GetAsync($"/api/sao-luu/tai-ve/{id}");

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Cong_viec_xong_nhung_tep_da_bi_don_thi_bao_loi_ro_rang()
    {
        var id = await TaoCongViecTaiVe(factory, TrangThaiCongViec.Xong, taoTep: false);

        var res = await factory.CreateAuthClient(loaiTaiKhoan: 9).GetAsync($"/api/sao-luu/tai-ve/{id}");

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await res.Content.ReadAsStringAsync()).Should().Contain("24 giờ");
    }

    [Fact]
    public async Task Tai_khoan_thuong_bi_tu_choi()
    {
        var id = await TaoCongViecTaiVe(factory, TrangThaiCongViec.Xong, taoTep: false);

        var res = await factory.CreateAuthClient(loaiTaiKhoan: 0).GetAsync($"/api/sao-luu/tai-ve/{id}");

        res.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Cong_viec_khong_phai_loai_tai_ve_thi_bi_tu_choi()
    {
        await using var db = factory.TaoContextThuan();
        var cv = new CongViecSaoLuu { Loai = LoaiCongViecSaoLuu.SaoLuu, TrangThai = TrangThaiCongViec.Xong };
        db.CongViecSaoLuu.Add(cv);
        await db.SaveChangesAsync();

        var res = await factory.CreateAuthClient(loaiTaiKhoan: 9).GetAsync($"/api/sao-luu/tai-ve/{cv.Id}");

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private static async Task<Guid> TaoCongViecTaiVe(QlgxApiFactory f, string trangThai, bool taoTep)
    {
        await using var db = f.TaoContextThuan();
        var cv = new CongViecSaoLuu { Loai = LoaiCongViecSaoLuu.TaiVe, TrangThai = trangThai };
        db.CongViecSaoLuu.Add(cv);
        await db.SaveChangesAsync();
        return cv.Id;
    }
}
