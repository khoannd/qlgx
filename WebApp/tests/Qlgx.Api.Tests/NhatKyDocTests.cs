using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace Qlgx.Api.Tests;

public class NhatKyDocTests(QlgxApiFactory f) : IClassFixture<QlgxApiFactory>
{
    [Fact]
    public async Task Tra_ve_lich_su_sua_cua_mot_giao_dan_moi_nhat_truoc()
    {
        Guid id;
        await using (var db = f.TaoContextThuan())
        {
            var g = new Domain.Entities.GiaoDan { GiaoXuId = f.GiaoXuId, MaGiaoDanCu = 9401, HoTen = "Ban dau" };
            db.GiaoDan.Add(g);
            await db.SaveChangesAsync();
            id = g.Id;
        }

        var client = f.CreateAuthClient();
        var truoc = await client.GetFromJsonAsync<Dtos.GiaoDanDetailDto>($"/api/giao-dan/{id}");
        // KiemTraNghiepVu (GiaoDanService.CapNhat) bat buoc phai co "Phai" — brief mau chi gui
        // rieng HoTen se bi chan 400 "Hay nhap gioi tinh", nen phai gui them Phai va RowVersion
        // dung nhu cach cac test CapNhat khac trong GiaoDanTests.cs da lam.
        var sua = await client.PutAsJsonAsync($"/api/giao-dan/{id}",
            new { HoTen = "Da sua", Phai = "Nam", NgaySinh = "2000-01-01", RowVersion = truoc!.RowVersion });
        sua.EnsureSuccessStatusCode();

        var ds = await client.GetFromJsonAsync<List<Dtos.DongNhatKyDto>>($"/api/nhat-ky/GiaoDan/{id}");

        ds.Should().NotBeNull();
        ds!.Should().Contain(d => d.Truong == "HoTen" && d.GiaTri!.Contains("Da sua"));
    }

    [Fact]
    public async Task Khong_doc_duoc_nhat_ky_cua_giao_xu_khac()
    {
        var giaoXuKhac = Guid.NewGuid();
        Guid id;
        await using (var db = f.TaoContextThuan())
        {
            db.GiaoXu.Add(new Domain.Entities.GiaoXu { Id = giaoXuKhac, TenGiaoXu = "Xu khac", MaGiaoXuCu = 77 });
            var g = new Domain.Entities.GiaoDan { GiaoXuId = giaoXuKhac, MaGiaoDanCu = 9402, HoTen = "Nguoi xu khac" };
            db.GiaoDan.Add(g);
            await db.SaveChangesAsync();
            id = g.Id;
        }

        var client = f.CreateAuthClient();
        var ds = await client.GetFromJsonAsync<List<Dtos.DongNhatKyDto>>($"/api/nhat-ky/GiaoDan/{id}");

        ds.Should().BeEmpty("bo loc giao xu phai chan, nhat ky chua nguyen van gia tri cac o");
    }
}
