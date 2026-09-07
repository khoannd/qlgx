using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace Qlgx.Api.Tests;

/// <summary>
/// Màn hình "Giáo họ" (frmGiaoHo.cs) — xem docs/superpowers/specs/man-hinh/giao-ho.md.
/// </summary>
public class GiaoHoTests(QlgxApiFactory app) : IClassFixture<QlgxApiFactory>
{
    private sealed record ThongBaoLoi(string ThongBao);
    private sealed record GiaoHoDto(Guid Id, int MaGiaoHoCu, string TenGiaoHo, Guid? GiaoHoChaId);

    [Fact]
    public async Task Thieu_ten_thi_bao_loi_dung_nguyen_van()
    {
        var res = await app.CreateAuthClient().PostAsJsonAsync("/api/giao-ho", new { tenGiaoHo = "  ", giaoHoChaId = (Guid?)null });
        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var loi = await res.Content.ReadFromJsonAsync<ThongBaoLoi>();
        loi!.ThongBao.Should().Be("Hãy nhập tên giáo họ!");
    }

    [Fact]
    public async Task Trung_ten_o_cung_cap_thi_bao_loi_dung_nguyen_van()
    {
        var client = app.CreateAuthClient();
        var ten = "Giáo họ Trùng Tên " + Guid.NewGuid();
        (await client.PostAsJsonAsync("/api/giao-ho", new { tenGiaoHo = ten, giaoHoChaId = (Guid?)null }))
            .StatusCode.Should().Be(HttpStatusCode.Created);

        var res = await client.PostAsJsonAsync("/api/giao-ho", new { tenGiaoHo = ten, giaoHoChaId = (Guid?)null });
        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var loi = await res.Content.ReadFromJsonAsync<ThongBaoLoi>();
        loi!.ThongBao.Should().Be("Tên giáo họ này đã có. Hãy nhập tên khác");
    }

    [Fact]
    public async Task Trung_ten_nhung_khac_giao_ho_cha_thi_khong_chan()
    {
        var client = app.CreateAuthClient();
        var ten = "Giáo họ Cùng Tên Khác Cha " + Guid.NewGuid();
        (await client.PostAsJsonAsync("/api/giao-ho", new { tenGiaoHo = ten, giaoHoChaId = (Guid?)null }))
            .StatusCode.Should().Be(HttpStatusCode.Created);

        // Lấy id giáo họ vừa tạo làm cha cho bước sau (đọc lại danh mục vì POST không trả id).
        var ds = await client.GetFromJsonAsync<List<GiaoHoDto>>("/api/giao-ho");
        var giaoHoChaId = ds!.First(h => h.TenGiaoHo == ten).Id;

        var resConLong = await client.PostAsJsonAsync("/api/giao-ho", new { tenGiaoHo = ten, giaoHoChaId });
        resConLong.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Sua_trung_ten_voi_giao_ho_khac_thi_chan()
    {
        var client = app.CreateAuthClient();
        var tenA = "Giáo họ A " + Guid.NewGuid();
        var tenB = "Giáo họ B " + Guid.NewGuid();
        await client.PostAsJsonAsync("/api/giao-ho", new { tenGiaoHo = tenA, giaoHoChaId = (Guid?)null });
        await client.PostAsJsonAsync("/api/giao-ho", new { tenGiaoHo = tenB, giaoHoChaId = (Guid?)null });
        var ds = await client.GetFromJsonAsync<List<GiaoHoDto>>("/api/giao-ho");
        var idB = ds!.First(h => h.TenGiaoHo == tenB).Id;

        var suaRes = await client.PutAsJsonAsync($"/api/giao-ho/{idB}", new { tenGiaoHo = tenA, giaoHoChaId = (Guid?)null });
        suaRes.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // Sửa về ĐÚNG tên của chính nó (không đổi gì) thì KHÔNG bị chặn — không tự đụng chính mình.
        var suaGiuNguyen = await client.PutAsJsonAsync($"/api/giao-ho/{idB}", new { tenGiaoHo = tenB, giaoHoChaId = (Guid?)null });
        suaGiuNguyen.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
