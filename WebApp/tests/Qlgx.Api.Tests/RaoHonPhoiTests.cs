using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Qlgx.Domain.Entities;

namespace Qlgx.Api.Tests;

/// <summary>
/// Màn hình "Danh sách rao hôn phối" (frmRaoHonPhoiList.cs + frmRaoHonPhoi.cs) — xem
/// docs/superpowers/specs/man-hinh/rao-hon-phoi.md. Bảng rỗng ở dữ liệu thật khảo sát nên
/// test là bằng chứng chính cho màn hình này.
/// </summary>
public class RaoHonPhoiTests(QlgxApiFactory app) : IClassFixture<QlgxApiFactory>
{
    private sealed record DsDto(Guid Id, int MaRaoHonPhoiCu, string? TenRaoHonPhoi,
        string? Nguoi1, string? Nguoi2, DateOnly? NgayRaoLan1, DateOnly? NgayRaoLan2,
        DateOnly? NgayRaoLan3, string? GhiChu);

    private sealed record ChiTietDto(Guid Id, int MaRaoHonPhoiCu, string? TenRaoHonPhoi,
        Guid? GiaoDan1Id, string? TenGiaoDan1, Guid? GiaoDan2Id, string? TenGiaoDan2,
        DateOnly? NgayRaoLan1, DateOnly? NgayRaoLan2, DateOnly? NgayRaoLan3,
        string? GiaoXu1, string? GiaoPhan1, string? GiaoXuTruoc1, string? GiaoPhanTruoc1,
        string? GiaoXu2, string? GiaoPhan2, string? GiaoXuTruoc2, string? GiaoPhanTruoc2,
        string? LinhMucNhan, string? GiaoXuNhan, string? GhiChu,
        string? Tam1, string? Tam2, string? Tam3,
        string? GiaoXuNQ1, string? GiaoPhanNQ1, string? GiaoXuNQ2, string? GiaoPhanNQ2,
        uint RowVersion);

    private sealed record ThongBaoLoi(string ThongBao);

    private async Task<Guid> TaoGiaoDan(int ma, string hoTen)
    {
        await using var db = app.TaoContextThuan();
        var gd = new GiaoDan { GiaoXuId = app.GiaoXuId, MaGiaoDanCu = ma, HoTen = hoTen };
        db.GiaoDan.Add(gd);
        await db.SaveChangesAsync();
        return gd.Id;
    }

    [Fact]
    public async Task Thieu_nguoi_thu_nhat_thi_bao_loi_dung_nguyen_van()
    {
        var res = await app.CreateAuthClient().PostAsJsonAsync("/api/rao-hon-phoi", new
        {
            tenRaoHonPhoi = "A - B", giaoDan1Id = (Guid?)null, giaoDan2Id = (Guid?)null,
            ngayRaoLan1 = (DateOnly?)null, ngayRaoLan2 = (DateOnly?)null, ngayRaoLan3 = (DateOnly?)null,
            giaoXu1 = (string?)null, giaoPhan1 = (string?)null, giaoXuTruoc1 = (string?)null, giaoPhanTruoc1 = (string?)null,
            giaoXu2 = (string?)null, giaoPhan2 = (string?)null, giaoXuTruoc2 = (string?)null, giaoPhanTruoc2 = (string?)null,
            linhMucNhan = (string?)null, giaoXuNhan = (string?)null, ghiChu = (string?)null,
            tam1 = (string?)null, tam2 = (string?)null, tam3 = (string?)null,
            giaoXuNQ1 = (string?)null, giaoPhanNQ1 = (string?)null, giaoXuNQ2 = (string?)null, giaoPhanNQ2 = (string?)null,
            rowVersion = (uint?)null,
        });
        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var loi = await res.Content.ReadFromJsonAsync<ThongBaoLoi>();
        loi!.ThongBao.Should().Be("Xin vui lòng nhập thông tin người thứ nhất cần rao");
    }

    [Fact]
    public async Task Tao_du_26_cot_roi_doc_lai_dung_het()
    {
        var client = app.CreateAuthClient();
        var g1 = await TaoGiaoDan(30001, "Nguyen Van Rao 1");
        var g2 = await TaoGiaoDan(30002, "Tran Thi Rao 2");

        var yc = new
        {
            tenRaoHonPhoi = "Nguyen Van Rao 1 - Tran Thi Rao 2",
            giaoDan1Id = g1, giaoDan2Id = g2,
            ngayRaoLan1 = DateOnly.FromDateTime(DateTime.Now.AddYears(1)),
            ngayRaoLan2 = DateOnly.FromDateTime(DateTime.Now.AddYears(1).AddDays(7)),
            ngayRaoLan3 = DateOnly.FromDateTime(DateTime.Now.AddYears(1).AddDays(14)),
            giaoXu1 = "Vo Nhiem", giaoPhan1 = "GP A", giaoXuTruoc1 = "Xu Cu 1", giaoPhanTruoc1 = "GP Cu 1",
            giaoXu2 = "Vo Nhiem", giaoPhan2 = "GP A", giaoXuTruoc2 = "Xu Cu 2", giaoPhanTruoc2 = "GP Cu 2",
            linhMucNhan = "Lm. Nhan", giaoXuNhan = "Xu Nhan", ghiChu = "Ghi chu rao",
            tam1 = "T1", tam2 = "T2", tam3 = "T3",
            giaoXuNQ1 = "NQ1", giaoPhanNQ1 = "PNQ1", giaoXuNQ2 = "NQ2", giaoPhanNQ2 = "PNQ2",
            rowVersion = (uint?)null,
        };

        var tao = await client.PostAsJsonAsync("/api/rao-hon-phoi", yc);
        tao.StatusCode.Should().Be(HttpStatusCode.Created);
        var ct = await tao.Content.ReadFromJsonAsync<ChiTietDto>();

        var lay = await client.GetFromJsonAsync<ChiTietDto>($"/api/rao-hon-phoi/{ct!.Id}");
        lay!.TenGiaoDan1.Should().Be("Nguyen Van Rao 1");
        lay.TenGiaoDan2.Should().Be("Tran Thi Rao 2");
        lay.GiaoXuTruoc1.Should().Be("Xu Cu 1");
        lay.GiaoPhanNQ2.Should().Be("PNQ2");
        lay.Tam3.Should().Be("T3");
        lay.NgayRaoLan2.Should().Be((DateOnly)yc.ngayRaoLan2!);

        var dsChuaHoanTat = await client.GetFromJsonAsync<List<DsDto>>("/api/rao-hon-phoi");
        dsChuaHoanTat.Should().Contain(x => x.Id == ct.Id);
    }

    [Fact]
    public async Task Da_rao_lan_3_qua_khu_thi_khong_hien_trong_danh_sach_mac_dinh_nhung_hien_khi_xem_tat_ca()
    {
        var client = app.CreateAuthClient();
        var g1 = await TaoGiaoDan(30010, "Da Xong A");
        var g2 = await TaoGiaoDan(30011, "Da Xong B");
        var qua_khu = DateOnly.FromDateTime(DateTime.Now.AddYears(-1));

        var tao = await client.PostAsJsonAsync("/api/rao-hon-phoi", new
        {
            tenRaoHonPhoi = "Da Xong A - Da Xong B", giaoDan1Id = g1, giaoDan2Id = g2,
            ngayRaoLan1 = qua_khu, ngayRaoLan2 = qua_khu, ngayRaoLan3 = qua_khu,
            giaoXu1 = (string?)null, giaoPhan1 = (string?)null, giaoXuTruoc1 = (string?)null, giaoPhanTruoc1 = (string?)null,
            giaoXu2 = (string?)null, giaoPhan2 = (string?)null, giaoXuTruoc2 = (string?)null, giaoPhanTruoc2 = (string?)null,
            linhMucNhan = (string?)null, giaoXuNhan = (string?)null, ghiChu = (string?)null,
            tam1 = (string?)null, tam2 = (string?)null, tam3 = (string?)null,
            giaoXuNQ1 = (string?)null, giaoPhanNQ1 = (string?)null, giaoXuNQ2 = (string?)null, giaoPhanNQ2 = (string?)null,
            rowVersion = (uint?)null,
        });
        var ct = await tao.Content.ReadFromJsonAsync<ChiTietDto>();

        var macDinh = await client.GetFromJsonAsync<List<DsDto>>("/api/rao-hon-phoi");
        macDinh.Should().NotContain(x => x.Id == ct!.Id);

        var tatCa = await client.GetFromJsonAsync<List<DsDto>>("/api/rao-hon-phoi?xemTatCa=true");
        tatCa.Should().Contain(x => x.Id == ct!.Id);
    }

    [Fact]
    public async Task Xoa_roi_doc_lai_thi_404()
    {
        var client = app.CreateAuthClient();
        var g1 = await TaoGiaoDan(30020, "Se Xoa A");
        var g2 = await TaoGiaoDan(30021, "Se Xoa B");
        var tao = await client.PostAsJsonAsync("/api/rao-hon-phoi", new
        {
            tenRaoHonPhoi = "Se Xoa A - Se Xoa B", giaoDan1Id = g1, giaoDan2Id = g2,
            ngayRaoLan1 = (DateOnly?)null, ngayRaoLan2 = (DateOnly?)null, ngayRaoLan3 = (DateOnly?)null,
            giaoXu1 = (string?)null, giaoPhan1 = (string?)null, giaoXuTruoc1 = (string?)null, giaoPhanTruoc1 = (string?)null,
            giaoXu2 = (string?)null, giaoPhan2 = (string?)null, giaoXuTruoc2 = (string?)null, giaoPhanTruoc2 = (string?)null,
            linhMucNhan = (string?)null, giaoXuNhan = (string?)null, ghiChu = (string?)null,
            tam1 = (string?)null, tam2 = (string?)null, tam3 = (string?)null,
            giaoXuNQ1 = (string?)null, giaoPhanNQ1 = (string?)null, giaoXuNQ2 = (string?)null, giaoPhanNQ2 = (string?)null,
            rowVersion = (uint?)null,
        });
        var ct = await tao.Content.ReadFromJsonAsync<ChiTietDto>();

        (await client.DeleteAsync($"/api/rao-hon-phoi/{ct!.Id}")).StatusCode.Should().Be(HttpStatusCode.OK);
        (await client.GetAsync($"/api/rao-hon-phoi/{ct.Id}")).StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Cap_nhat_dung_phien_ban_thi_bao_xung_dot()
    {
        var client = app.CreateAuthClient();
        var g1 = await TaoGiaoDan(30030, "RowVer A");
        var g2 = await TaoGiaoDan(30031, "RowVer B");
        var tao = await client.PostAsJsonAsync("/api/rao-hon-phoi", new
        {
            tenRaoHonPhoi = "RowVer A - RowVer B", giaoDan1Id = g1, giaoDan2Id = g2,
            ngayRaoLan1 = (DateOnly?)null, ngayRaoLan2 = (DateOnly?)null, ngayRaoLan3 = (DateOnly?)null,
            giaoXu1 = (string?)null, giaoPhan1 = (string?)null, giaoXuTruoc1 = (string?)null, giaoPhanTruoc1 = (string?)null,
            giaoXu2 = (string?)null, giaoPhan2 = (string?)null, giaoXuTruoc2 = (string?)null, giaoPhanTruoc2 = (string?)null,
            linhMucNhan = (string?)null, giaoXuNhan = (string?)null, ghiChu = (string?)null,
            tam1 = (string?)null, tam2 = (string?)null, tam3 = (string?)null,
            giaoXuNQ1 = (string?)null, giaoPhanNQ1 = (string?)null, giaoXuNQ2 = (string?)null, giaoPhanNQ2 = (string?)null,
            rowVersion = (uint?)null,
        });
        var ct = await tao.Content.ReadFromJsonAsync<ChiTietDto>();

        var res = await client.PutAsJsonAsync($"/api/rao-hon-phoi/{ct!.Id}", new
        {
            tenRaoHonPhoi = "Sua roi", giaoDan1Id = g1, giaoDan2Id = g2,
            ngayRaoLan1 = (DateOnly?)null, ngayRaoLan2 = (DateOnly?)null, ngayRaoLan3 = (DateOnly?)null,
            giaoXu1 = (string?)null, giaoPhan1 = (string?)null, giaoXuTruoc1 = (string?)null, giaoPhanTruoc1 = (string?)null,
            giaoXu2 = (string?)null, giaoPhan2 = (string?)null, giaoXuTruoc2 = (string?)null, giaoPhanTruoc2 = (string?)null,
            linhMucNhan = (string?)null, giaoXuNhan = (string?)null, ghiChu = (string?)null,
            tam1 = (string?)null, tam2 = (string?)null, tam3 = (string?)null,
            giaoXuNQ1 = (string?)null, giaoPhanNQ1 = (string?)null, giaoXuNQ2 = (string?)null, giaoPhanNQ2 = (string?)null,
            rowVersion = (uint?)(ct.RowVersion + 999),
        });
        res.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}
