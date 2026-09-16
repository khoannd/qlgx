using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Qlgx.Domain;
using Qlgx.Domain.Entities;

namespace Qlgx.Api.Tests;

/// <summary>
/// Màn hình "Danh sách sổ bí tích" (frmDotBiTichList.cs + frmBiTichChiTiet.cs) — xem
/// docs/superpowers/specs/man-hinh/so-bi-tich.md.
/// </summary>
public class DotBiTichTests(QlgxApiFactory app) : IClassFixture<QlgxApiFactory>
{
    private sealed record DsDto(Guid Id, int MaDotBiTichCu, LoaiBiTich LoaiBiTich,
        DateOnly? NgayBiTich, string? MoTa, string? LinhMuc, string? NoiBiTich, int SoLuong);

    private sealed record NguoiNhanDto(Guid GiaoDanId, int MaGiaoDanCu, string? TenThanh,
        string HoTen, string? Phai, DateOnly? NgaySinh, string? SoBiTich, string? NguoiDoDau, string? GhiChu);

    private sealed record ChiTietDto(Guid Id, int MaDotBiTichCu, LoaiBiTich LoaiBiTich,
        DateOnly? NgayBiTich, string? MoTa, string? LinhMuc, string? NoiBiTich, uint RowVersion,
        List<NguoiNhanDto> NguoiNhan);

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
    public async Task Danh_sach_khong_truyen_loai_bi_tich_thi_tra_400()
    {
        var res = await app.CreateAuthClient().GetAsync("/api/dot-bi-tich");
        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Tao_dot_roi_them_nguoi_nhan_thi_xem_lai_thay_du_thong_tin()
    {
        var client = app.CreateAuthClient();
        var giaoDanId = await TaoGiaoDan(20001, "Nguyen Van Rua Toi");

        var tao = await client.PostAsJsonAsync("/api/dot-bi-tich", new
        {
            loaiBiTich = LoaiBiTich.RuaToi, ngayBiTich = new DateOnly(2024, 1, 1),
            moTa = "Dot rua toi thu nghiem", linhMuc = "Lm. A", noiBiTich = "Nha tho xu",
        });
        tao.StatusCode.Should().Be(HttpStatusCode.Created);
        var dot = await tao.Content.ReadFromJsonAsync<ChiTietDto>();
        dot!.NguoiNhan.Should().BeEmpty();

        var them = await client.PostAsJsonAsync($"/api/dot-bi-tich/{dot.Id}/nguoi-nhan", new
        {
            giaoDanId, soBiTich = "123", nguoiDoDau = "Ong Do Dau", ghiChu = "Ghi chu A",
        });
        them.StatusCode.Should().Be(HttpStatusCode.OK);

        var chiTiet = await client.GetFromJsonAsync<ChiTietDto>($"/api/dot-bi-tich/{dot.Id}");
        chiTiet!.NguoiNhan.Should().ContainSingle();
        var nn = chiTiet.NguoiNhan[0];
        nn.SoBiTich.Should().Be("123");
        nn.NguoiDoDau.Should().Be("Ong Do Dau");
        nn.GhiChu.Should().Be("Ghi chu A");

        // Ngay/Linh muc/Noi cua dot phai duoc copy xuong GiaoDan — khop AssignDataSource +
        // vong lap getGridData (frmBiTichChiTiet.cs:385-387).
        await using var db = app.TaoContextThuan();
        var gd = db.GiaoDan.Single(g => g.Id == giaoDanId);
        gd.NgayRuaToi.Should().Be(new DateOnly(2024, 1, 1));
        gd.ChaRuaToi.Should().Be("Lm. A");
        gd.NoiRuaToi.Should().Be("Nha tho xu");
        gd.SoRuaToi.Should().Be("123");
        gd.NguoiDoDauRuaToi.Should().Be("Ong Do Dau");
    }

    [Fact]
    public async Task Them_cung_mot_nguoi_hai_lan_vao_cung_dot_thi_bao_loi()
    {
        var client = app.CreateAuthClient();
        var giaoDanId = await TaoGiaoDan(20010, "Trung Lap A");
        var tao = await client.PostAsJsonAsync("/api/dot-bi-tich", new
        {
            loaiBiTich = LoaiBiTich.RuocLe, ngayBiTich = (DateOnly?)null, moTa = "Dot XTRL", linhMuc = (string?)null, noiBiTich = (string?)null,
        });
        var dot = await tao.Content.ReadFromJsonAsync<ChiTietDto>();

        var lan1 = await client.PostAsJsonAsync($"/api/dot-bi-tich/{dot!.Id}/nguoi-nhan",
            new { giaoDanId, soBiTich = (string?)null, nguoiDoDau = (string?)null, ghiChu = (string?)null });
        lan1.StatusCode.Should().Be(HttpStatusCode.OK);

        var lan2 = await client.PostAsJsonAsync($"/api/dot-bi-tich/{dot.Id}/nguoi-nhan",
            new { giaoDanId, soBiTich = (string?)null, nguoiDoDau = (string?)null, ghiChu = (string?)null });
        lan2.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var loi = await lan2.Content.ReadFromJsonAsync<ThongBaoLoi>();
        loi!.ThongBao.Should().Contain("Đã tồn tại giáo dân này trong danh sách");
    }

    [Fact]
    public async Task Them_nguoi_da_co_o_dot_khac_cung_loai_thi_bao_loi()
    {
        var client = app.CreateAuthClient();
        var giaoDanId = await TaoGiaoDan(20020, "Trung Lap B");
        var dot1 = (await (await client.PostAsJsonAsync("/api/dot-bi-tich", new
        {
            loaiBiTich = LoaiBiTich.ThemSuc, ngayBiTich = (DateOnly?)null, moTa = "Dot 1", linhMuc = (string?)null, noiBiTich = (string?)null,
        })).Content.ReadFromJsonAsync<ChiTietDto>())!;
        var dot2 = (await (await client.PostAsJsonAsync("/api/dot-bi-tich", new
        {
            loaiBiTich = LoaiBiTich.ThemSuc, ngayBiTich = (DateOnly?)null, moTa = "Dot 2", linhMuc = (string?)null, noiBiTich = (string?)null,
        })).Content.ReadFromJsonAsync<ChiTietDto>())!;

        await client.PostAsJsonAsync($"/api/dot-bi-tich/{dot1.Id}/nguoi-nhan",
            new { giaoDanId, soBiTich = (string?)null, nguoiDoDau = (string?)null, ghiChu = (string?)null });

        var res = await client.PostAsJsonAsync($"/api/dot-bi-tich/{dot2.Id}/nguoi-nhan",
            new { giaoDanId, soBiTich = (string?)null, nguoiDoDau = (string?)null, ghiChu = (string?)null });
        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var loi = await res.Content.ReadFromJsonAsync<ThongBaoLoi>();
        loi!.ThongBao.Should().Contain("đợt bí tích khác");
    }

    [Fact]
    public async Task Xoa_nguoi_nhan_kem_xoa_thong_tin_thi_null_hoa_cot_giao_dan()
    {
        var client = app.CreateAuthClient();
        var giaoDanId = await TaoGiaoDan(20030, "Se Bi Xoa");
        var dot = (await (await client.PostAsJsonAsync("/api/dot-bi-tich", new
        {
            loaiBiTich = LoaiBiTich.RuaToi, ngayBiTich = new DateOnly(2020, 1, 1), moTa = "Dot xoa", linhMuc = "X", noiBiTich = "Y",
        })).Content.ReadFromJsonAsync<ChiTietDto>())!;
        await client.PostAsJsonAsync($"/api/dot-bi-tich/{dot.Id}/nguoi-nhan",
            new { giaoDanId, soBiTich = "99", nguoiDoDau = "D", ghiChu = (string?)null });

        var xoa = await client.DeleteAsync($"/api/dot-bi-tich/{dot.Id}/nguoi-nhan/{giaoDanId}?xoaThongTinBiTich=true");
        xoa.StatusCode.Should().Be(HttpStatusCode.OK);

        await using var db = app.TaoContextThuan();
        var gd = db.GiaoDan.Single(g => g.Id == giaoDanId);
        gd.SoRuaToi.Should().BeNull();
        gd.NgayRuaToi.Should().BeNull();
        gd.NguoiDoDauRuaToi.Should().BeNull();
        db.BiTichChiTiet.Any(c => c.DotBiTichId == dot.Id).Should().BeFalse();
    }

    [Fact]
    public async Task Xoa_ca_dot_thi_xoa_luon_bi_tich_chi_tiet()
    {
        var client = app.CreateAuthClient();
        var giaoDanId = await TaoGiaoDan(20040, "Trong Dot Bi Xoa");
        var dot = (await (await client.PostAsJsonAsync("/api/dot-bi-tich", new
        {
            loaiBiTich = LoaiBiTich.RuaToi, ngayBiTich = (DateOnly?)null, moTa = "Dot se bi xoa", linhMuc = (string?)null, noiBiTich = (string?)null,
        })).Content.ReadFromJsonAsync<ChiTietDto>())!;
        await client.PostAsJsonAsync($"/api/dot-bi-tich/{dot.Id}/nguoi-nhan",
            new { giaoDanId, soBiTich = (string?)null, nguoiDoDau = (string?)null, ghiChu = (string?)null });

        var xoa = await client.DeleteAsync($"/api/dot-bi-tich/{dot.Id}");
        xoa.StatusCode.Should().Be(HttpStatusCode.OK);

        (await client.GetAsync($"/api/dot-bi-tich/{dot.Id}")).StatusCode.Should().Be(HttpStatusCode.NotFound);
        await using var db = app.TaoContextThuan();
        db.BiTichChiTiet.Any(c => c.DotBiTichId == dot.Id).Should().BeFalse();
    }

    [Fact]
    public async Task Cap_nhat_dung_phien_ban_thi_bao_xung_dot()
    {
        var client = app.CreateAuthClient();
        var dot = (await (await client.PostAsJsonAsync("/api/dot-bi-tich", new
        {
            loaiBiTich = LoaiBiTich.RuaToi, ngayBiTich = (DateOnly?)null, moTa = "Dot RowVersion", linhMuc = (string?)null, noiBiTich = (string?)null,
        })).Content.ReadFromJsonAsync<ChiTietDto>())!;

        var res = await client.PutAsJsonAsync($"/api/dot-bi-tich/{dot.Id}", new
        {
            ngayBiTich = (DateOnly?)null, moTa = "Sua roi", linhMuc = (string?)null, noiBiTich = (string?)null,
            rowVersion = dot.RowVersion + 999,
        });
        res.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}
