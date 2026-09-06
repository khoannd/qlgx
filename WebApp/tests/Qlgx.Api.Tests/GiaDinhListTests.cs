using System.Net.Http.Json;
using FluentAssertions;
using Qlgx.Domain;
using Qlgx.Domain.Entities;

namespace Qlgx.Api.Tests;

public class GiaDinhListTests(QlgxApiFactory app) : IClassFixture<QlgxApiFactory>
{
    private sealed record Item(Guid Id, int MaGiaDinhCu, string? TenGiaDinh, string? TenChong,
        string? TenVo, int SoLuong, string? DTChong, string? DTVo, string? TenGiaoHo, int Gach,
        Guid? HonPhoiId, string? NgayHonPhoiHienThi);

    private async Task<(Guid GiaDinhId, Guid ChongId, Guid VoId)> TaoGiaDinhMau(
        bool chongQuaDoi = false, bool voQuaDoi = false,
        string tenGiaoHo = "Giao ho Thanh Tam", int ma = 12)
    {
        await using var db = app.TaoContextThuan();
        var giaoHo = new GiaoHo { GiaoXuId = app.GiaoXuId, TenGiaoHo = tenGiaoHo, MaGiaoHoCu = ma };
        var gd = new GiaDinh { GiaoXuId = app.GiaoXuId, MaGiaDinhCu = ma, TenGiaDinh = "Binh - Lan", GiaoHo = giaoHo };
        var chong = new GiaoDan { GiaoXuId = app.GiaoXuId, MaGiaoDanCu = ma * 10 + 1, HoTen = "Tran Van Binh",
            TenThanh = "Giuse", Phai = "Nam", DienThoai = "0912 345 678", QuaDoi = chongQuaDoi };
        var vo = new GiaoDan { GiaoXuId = app.GiaoXuId, MaGiaoDanCu = ma * 10 + 2, HoTen = "Nguyen Thi Lan",
            TenThanh = "Maria", Phai = "Nu", DienThoai = "0987 114 220", QuaDoi = voQuaDoi };
        var con = new GiaoDan { GiaoXuId = app.GiaoXuId, MaGiaoDanCu = ma * 10 + 3, HoTen = "Tran Minh Khoi",
            TenThanh = "Giuse", Phai = "Nam" };
        db.AddRange(giaoHo, gd, chong, vo, con);
        db.ThanhVienGiaDinh.AddRange(
            new ThanhVienGiaDinh { GiaoXuId = app.GiaoXuId, GiaDinh = gd, GiaoDan = chong, VaiTro = VaiTroGiaDinh.Chong },
            new ThanhVienGiaDinh { GiaoXuId = app.GiaoXuId, GiaDinh = gd, GiaoDan = vo, VaiTro = VaiTroGiaDinh.Vo },
            new ThanhVienGiaDinh { GiaoXuId = app.GiaoXuId, GiaDinh = gd, GiaoDan = con, VaiTro = VaiTroGiaDinh.Con });
        await db.SaveChangesAsync();
        return (gd.Id, chong.Id, vo.Id);
    }

    [Fact]
    public async Task Danh_sach_tra_ve_ten_chong_ten_vo_va_so_nhan_khau()
    {
        await TaoGiaDinhMau(ma: 12);

        var ds = await app.CreateClient().GetFromJsonAsync<List<Item>>("/api/gia-dinh");

        var dong = ds!.Single(x => x.MaGiaDinhCu == 12);
        dong.TenChong.Should().Be("Giuse Tran Van Binh");
        dong.TenVo.Should().Be("Maria Nguyen Thi Lan");
        dong.SoLuong.Should().Be(3, "chong, vo va mot nguoi con");
        dong.DTChong.Should().Be("0912 345 678");
        dong.TenGiaoHo.Should().Be("Giao ho Thanh Tam");
    }

    [Theory]
    [InlineData(false, false, -1)]
    [InlineData(true, false, 0)]
    [InlineData(false, true, 1)]
    [InlineData(true, true, 2)]
    public async Task Cot_gach_bao_dung_ai_da_qua_doi(bool chongMat, bool voMat, int gachMongDoi)
    {
        var ma = 20 + gachMongDoi + 2;
        await TaoGiaDinhMau(chongMat, voMat, ma: ma);

        var ds = await app.CreateClient().GetFromJsonAsync<List<Item>>("/api/gia-dinh");

        ds!.Single(x => x.MaGiaDinhCu == ma).Gach.Should().Be(gachMongDoi);
    }

    [Fact]
    public async Task Loc_theo_giao_ho_chi_tra_gia_dinh_cua_giao_ho_do()
    {
        await TaoGiaDinhMau(tenGiaoHo: "Giao ho Fatima", ma: 35);
        await using var db = app.TaoContextThuan();
        var giaoHoFatima = db.GiaoHo.Single(x => x.TenGiaoHo == "Giao ho Fatima");

        var ds = await app.CreateClient()
            .GetFromJsonAsync<List<Item>>($"/api/gia-dinh?giaoHoId={giaoHoFatima.Id}");

        ds!.Should().OnlyContain(x => x.TenGiaoHo == "Giao ho Fatima");
        ds.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Gia_dinh_da_xoa_mem_khong_hien_trong_danh_sach()
    {
        var (id, _, _) = await TaoGiaDinhMau(ma: 99);
        await using (var db = app.TaoContextThuan())
        {
            var gd = db.GiaDinh.Single(x => x.Id == id);
            gd.DaXoa = true;
            await db.SaveChangesAsync();
        }

        var ds = await app.CreateClient().GetFromJsonAsync<List<Item>>("/api/gia-dinh");

        ds!.Should().NotContain(x => x.MaGiaDinhCu == 99);
    }

    [Fact]
    public async Task Gia_dinh_chua_co_hon_phoi_tra_ve_HonPhoiId_null()
    {
        await TaoGiaDinhMau(ma: 41);

        var ds = await app.CreateClient().GetFromJsonAsync<List<Item>>("/api/gia-dinh");

        var dong = ds!.Single(x => x.MaGiaDinhCu == 41);
        dong.HonPhoiId.Should().BeNull();
        dong.NgayHonPhoiHienThi.Should().BeNull();
    }

    [Fact]
    public async Task Gia_dinh_da_co_hon_phoi_tra_ve_HonPhoiId_va_ngay_dinh_dang_ddMMyyyy()
    {
        var (_, chongId, voId) = await TaoGiaDinhMau(ma: 42);
        Guid honPhoiId;
        await using (var db = app.TaoContextThuan())
        {
            var honPhoi = new HonPhoi
            {
                GiaoXuId = app.GiaoXuId,
                MaHonPhoiCu = 42,
                NgayHonPhoi = new DateOnly(2010, 2, 14),
            };
            db.HonPhoi.Add(honPhoi);
            db.GiaoDanHonPhoi.AddRange(
                new GiaoDanHonPhoi { GiaoXuId = app.GiaoXuId, HonPhoi = honPhoi, GiaoDanId = chongId, SoThuTu = 1 },
                new GiaoDanHonPhoi { GiaoXuId = app.GiaoXuId, HonPhoi = honPhoi, GiaoDanId = voId, SoThuTu = 2 });
            await db.SaveChangesAsync();
            honPhoiId = honPhoi.Id;
        }

        var ds = await app.CreateClient().GetFromJsonAsync<List<Item>>("/api/gia-dinh");

        var dong = ds!.Single(x => x.MaGiaDinhCu == 42);
        dong.HonPhoiId.Should().Be(honPhoiId);
        dong.NgayHonPhoiHienThi.Should().Be("14/02/2010");
    }
}
