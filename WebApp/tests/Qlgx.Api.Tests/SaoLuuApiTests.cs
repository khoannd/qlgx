using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Qlgx.Domain.Entities;

namespace Qlgx.Api.Tests;

public class SaoLuuApiTests(QlgxApiFactory factory) : IClassFixture<QlgxApiFactory>
{
    private HttpClient ClientHeThong() => factory.CreateAuthClient(loaiTaiKhoan: 9);

    [Fact]
    public async Task Tai_khoan_thuong_bi_tu_choi_moi_route_sao_luu()
    {
        var client = factory.CreateAuthClient(loaiTaiKhoan: 0);

        foreach (var duongDan in new[] { "/api/sao-luu/tinh-trang", "/api/sao-luu/danh-sach", "/api/sao-luu/cong-viec" })
            (await client.GetAsync(duongDan)).StatusCode.Should().Be(HttpStatusCode.Forbidden, duongDan);

        var res = await client.PostAsJsonAsync("/api/sao-luu/cong-viec", new { loai = "sao_luu" });
        res.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Tao_cong_viec_sao_luu_chi_ghi_mot_dong_trang_thai_cho()
    {
        var res = await ClientHeThong().PostAsJsonAsync("/api/sao-luu/cong-viec", new { loai = "sao_luu" });

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        var than = await res.Content.ReadFromJsonAsync<TaoCongViecKetQua>();
        await using var db = factory.TaoContextThuan();
        var cv = await db.CongViecSaoLuu.SingleAsync(x => x.Id == than!.Id);
        cv.TrangThai.Should().Be("cho");
        cv.BatDauLuc.Should().BeNull("API chi tao cong viec, bo chay tren host moi thuc thi");

        // Don dep: cac test khac dung chung mot database (IClassFixture). Neu de nguyen dong
        // "cho" nay lai, no se chan vinh vien nhung test tao cong viec khac qua bao ve
        // "khong xep hang hai cong viec ghi cung luc" trong SaoLuuService.TaoCongViec.
        db.CongViecSaoLuu.Remove(cv);
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task Loai_cong_viec_khong_hop_le_bi_tu_choi()
    {
        var res = await ClientHeThong().PostAsJsonAsync("/api/sao-luu/cong-viec", new { loai = "xoa_sach" });

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Phuc_hoi_thieu_chuoi_xac_nhan_bi_tu_choi()
    {
        var res = await ClientHeThong().PostAsJsonAsync("/api/sao-luu/cong-viec",
            new { loai = "phuc_hoi", snapshotId = "ab12cd34" });

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await res.Content.ReadAsStringAsync()).Should().Contain("PHUC HOI TOAN BO");
    }

    [Fact]
    public async Task Phuc_hoi_sai_chuoi_xac_nhan_bi_tu_choi()
    {
        var res = await ClientHeThong().PostAsJsonAsync("/api/sao-luu/cong-viec",
            new { loai = "phuc_hoi", snapshotId = "ab12cd34", xacNhan = "phuc hoi toan bo" });

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Phuc_hoi_thieu_snapshot_id_bi_tu_choi()
    {
        var res = await ClientHeThong().PostAsJsonAsync("/api/sao-luu/cong-viec",
            new { loai = "phuc_hoi", xacNhan = "PHUC HOI TOAN BO" });

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Phuc_hoi_du_dieu_kien_thi_tao_duoc_cong_viec()
    {
        var res = await ClientHeThong().PostAsJsonAsync("/api/sao-luu/cong-viec",
            new { loai = "phuc_hoi", snapshotId = "ab12cd34", xacNhan = "PHUC HOI TOAN BO" });

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        var than = await res.Content.ReadFromJsonAsync<TaoCongViecKetQua>();
        await using var db = factory.TaoContextThuan();
        var cv = await db.CongViecSaoLuu.SingleAsync(x => x.Id == than!.Id);
        cv.ThamSoJson.Should().Contain("ab12cd34");
    }

    [Fact]
    public async Task Den_do_khi_co_loi_gan_nhat()
    {
        await using (var db = factory.TaoContextThuan())
        {
            var tt = await db.TrangThaiSaoLuu.SingleAsync();
            tt.SaoLuuGanNhat = DateTimeOffset.UtcNow;
            tt.LoiGanNhat = "restic check that bai";
            await db.SaveChangesAsync();
        }

        var than = await ClientHeThong().GetFromJsonAsync<TinhTrangKetQua>("/api/sao-luu/tinh-trang");

        than!.Den.Should().Be("do");
    }

    [Fact]
    public async Task Den_vang_khi_qua_8_gio_khong_co_ban_sao_moi()
    {
        await using (var db = factory.TaoContextThuan())
        {
            var tt = await db.TrangThaiSaoLuu.SingleAsync();
            tt.SaoLuuGanNhat = DateTimeOffset.UtcNow.AddHours(-9);
            tt.LoiGanNhat = null;
            await db.SaveChangesAsync();
        }

        var than = await ClientHeThong().GetFromJsonAsync<TinhTrangKetQua>("/api/sao-luu/tinh-trang");

        than!.Den.Should().Be("vang");
    }

    [Fact]
    public async Task Den_xanh_khi_vua_sao_luu_xong_va_khong_loi()
    {
        await using (var db = factory.TaoContextThuan())
        {
            var tt = await db.TrangThaiSaoLuu.SingleAsync();
            tt.SaoLuuGanNhat = DateTimeOffset.UtcNow.AddHours(-1);
            tt.LoiGanNhat = null;
            tt.DienTapGanNhat = DateTimeOffset.UtcNow.AddDays(-2);
            tt.DienTapDat = true;
            await db.SaveChangesAsync();
        }

        var than = await ClientHeThong().GetFromJsonAsync<TinhTrangKetQua>("/api/sao-luu/tinh-trang");

        than!.Den.Should().Be("xanh");
    }

    [Fact]
    public async Task Chua_bao_gio_sao_luu_thi_den_do()
    {
        await using (var db = factory.TaoContextThuan())
        {
            var tt = await db.TrangThaiSaoLuu.SingleAsync();
            tt.SaoLuuGanNhat = null;
            tt.LoiGanNhat = null;
            await db.SaveChangesAsync();
        }

        var than = await ClientHeThong().GetFromJsonAsync<TinhTrangKetQua>("/api/sao-luu/tinh-trang");

        than!.Den.Should().Be("do");
    }

    private sealed record TaoCongViecKetQua(Guid Id);
    private sealed record TinhTrangKetQua(string Den, DateTimeOffset? SaoLuuGanNhat, int SoBanSao,
        DateTimeOffset? DienTapGanNhat, bool DienTapDat, string? LoiGanNhat);
}
