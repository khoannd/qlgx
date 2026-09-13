using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Qlgx.Api.Dtos;

namespace Qlgx.Api.Tests;

public class NhatKyGhiHangLoatTests(QlgxApiFactory f) : IClassFixture<QlgxApiFactory>
{
    [Fact]
    public async Task Thay_the_hang_loat_sinh_nhat_ky_cho_tung_ban_ghi()
    {
        await using (var db = f.TaoContextThuan())
        {
            db.GiaoDan.AddRange(
                new Domain.Entities.GiaoDan { GiaoXuId = f.GiaoXuId, MaGiaoDanCu = 9301, HoTen = "A", NoiSinh = "Ha Noi" },
                new Domain.Entities.GiaoDan { GiaoXuId = f.GiaoXuId, MaGiaoDanCu = 9302, HoTen = "B", NoiSinh = "Ha Noi" });
            await db.SaveChangesAsync();
        }

        var client = f.CreateAuthClient();
        var phanHoi = await client.PostAsJsonAsync("/api/cong-cu-du-lieu/tim-thay-the",
            new TimThayTheRequest(BangTimThayThe.GiaoDan, "NoiSinh", "Ha Noi", "Ha Noi cu"));
        phanHoi.EnsureSuccessStatusCode();

        await using var doc = f.TaoContextThuan();
        var dong = await doc.HieuLuc.Where(x => x.Truong == "NoiSinh").ToListAsync();
        dong.Should().HaveCount(2, "moi ban ghi bi sua phai co mot dong nhat ky rieng");
        dong.Should().OnlyContain(x => x.GiaTri!.Contains("Ha Noi cu"));
    }
}
