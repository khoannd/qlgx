using System.Net;
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

    /// <summary>
    /// Vòng sửa 1 của Task 7: một giáo dân có thể có nhiều bản ghi hôn phối theo thời gian
    /// (goá rồi tái hôn — xem chú thích SELECT_HONPHOI_THEO_MAGIAODAN trong
    /// Source/DBAccess/SqlConstants.cs của bản Access). Trước bản sửa, cột HonPhoiId của lưới
    /// dùng FirstOrDefault() trên MỘT correlated subquery lấy hôn phối theo "chồng HOẶC vợ",
    /// nên khi chồng từng có một hôn phối trước đó, subquery này có thể ÂM THẦM CHỌN BỪA bản
    /// ghi sai (thứ tự không xác định) thay vì bản ghi mà cả hai vợ chồng cùng nối tới.
    /// </summary>
    [Fact]
    public async Task Chong_tung_co_hon_phoi_truoc_thi_luoi_van_chon_dung_hon_phoi_cua_ca_hai_vo_chong()
    {
        var (_, chongId, voId) = await TaoGiaDinhMau(ma: 43);
        Guid honPhoiHienTaiId;
        await using (var db = app.TaoContextThuan())
        {
            var hpCu = new HonPhoi { GiaoXuId = app.GiaoXuId, MaHonPhoiCu = 430,
                NgayHonPhoi = new DateOnly(2000, 1, 1) };
            db.HonPhoi.Add(hpCu);
            db.GiaoDanHonPhoi.Add(
                new GiaoDanHonPhoi { GiaoXuId = app.GiaoXuId, HonPhoi = hpCu, GiaoDanId = chongId, SoThuTu = 1 });

            var hpHienTai = new HonPhoi { GiaoXuId = app.GiaoXuId, MaHonPhoiCu = 431,
                NgayHonPhoi = new DateOnly(2015, 5, 5) };
            db.HonPhoi.Add(hpHienTai);
            db.GiaoDanHonPhoi.AddRange(
                new GiaoDanHonPhoi { GiaoXuId = app.GiaoXuId, HonPhoi = hpHienTai, GiaoDanId = chongId, SoThuTu = 1 },
                new GiaoDanHonPhoi { GiaoXuId = app.GiaoXuId, HonPhoi = hpHienTai, GiaoDanId = voId, SoThuTu = 2 });
            await db.SaveChangesAsync();
            honPhoiHienTaiId = hpHienTai.Id;
        }

        var res = await app.CreateClient().GetAsync("/api/gia-dinh");

        res.StatusCode.Should().Be(HttpStatusCode.OK, "khong duoc gay loi voi du lieu hop le nay");
        var ds = await res.Content.ReadFromJsonAsync<List<Item>>();
        var dong = ds!.Single(x => x.MaGiaDinhCu == 43);
        dong.HonPhoiId.Should().Be(honPhoiHienTaiId,
            "phai la hon phoi ca hai vo chong cung noi toi, khong phai hon phoi rieng truoc day cua chong");
        dong.NgayHonPhoiHienThi.Should().Be("05/05/2015");
    }

    [Fact]
    public async Task Gia_dinh_chua_co_chong_tra_ve_TenChong_va_DTChong_null()
    {
        var ma = 50;
        await using (var db = app.TaoContextThuan())
        {
            var gd = new GiaDinh { GiaoXuId = app.GiaoXuId, MaGiaDinhCu = ma, TenGiaDinh = "Lan" };
            var vo = new GiaoDan { GiaoXuId = app.GiaoXuId, MaGiaoDanCu = ma * 10 + 2, HoTen = "Nguyen Thi Lan",
                TenThanh = "Maria", Phai = "Nu" };
            db.AddRange(gd, vo);
            db.ThanhVienGiaDinh.Add(
                new ThanhVienGiaDinh { GiaoXuId = app.GiaoXuId, GiaDinh = gd, GiaoDan = vo, VaiTro = VaiTroGiaDinh.Vo });
            await db.SaveChangesAsync();
        }

        var ds = await app.CreateClient().GetFromJsonAsync<List<Item>>("/api/gia-dinh");

        var dong = ds!.Single(x => x.MaGiaDinhCu == ma);
        dong.TenChong.Should().BeNull();
        dong.DTChong.Should().BeNull();
        dong.TenVo.Should().Be("Maria Nguyen Thi Lan");
    }

    [Fact]
    public async Task Gia_dinh_chua_co_vo_tra_ve_TenVo_va_DTVo_null()
    {
        var ma = 51;
        await using (var db = app.TaoContextThuan())
        {
            var gd = new GiaDinh { GiaoXuId = app.GiaoXuId, MaGiaDinhCu = ma, TenGiaDinh = "Binh" };
            var chong = new GiaoDan { GiaoXuId = app.GiaoXuId, MaGiaoDanCu = ma * 10 + 1, HoTen = "Tran Van Binh",
                TenThanh = "Giuse", Phai = "Nam" };
            db.AddRange(gd, chong);
            db.ThanhVienGiaDinh.Add(
                new ThanhVienGiaDinh { GiaoXuId = app.GiaoXuId, GiaDinh = gd, GiaoDan = chong, VaiTro = VaiTroGiaDinh.Chong });
            await db.SaveChangesAsync();
        }

        var ds = await app.CreateClient().GetFromJsonAsync<List<Item>>("/api/gia-dinh");

        var dong = ds!.Single(x => x.MaGiaDinhCu == ma);
        dong.TenVo.Should().BeNull();
        dong.DTVo.Should().BeNull();
        dong.TenChong.Should().Be("Giuse Tran Van Binh");
    }

    [Fact]
    public async Task Gia_dinh_khong_co_thanh_vien_nao_tra_ve_SoLuong_0_va_Gach_am_1()
    {
        var ma = 52;
        await using (var db = app.TaoContextThuan())
        {
            db.GiaDinh.Add(new GiaDinh { GiaoXuId = app.GiaoXuId, MaGiaDinhCu = ma, TenGiaDinh = "Gia dinh trong" });
            await db.SaveChangesAsync();
        }

        var ds = await app.CreateClient().GetFromJsonAsync<List<Item>>("/api/gia-dinh");

        var dong = ds!.Single(x => x.MaGiaDinhCu == ma);
        dong.SoLuong.Should().Be(0);
        dong.TenChong.Should().BeNull();
        dong.TenVo.Should().BeNull();
        dong.Gach.Should().Be(-1);
    }

    [Fact]
    public async Task Giao_dan_ten_thanh_rong_hien_thi_ho_ten_tron_khong_khoang_trang_thua()
    {
        var ma = 53;
        await using (var db = app.TaoContextThuan())
        {
            var gd = new GiaDinh { GiaoXuId = app.GiaoXuId, MaGiaDinhCu = ma, TenGiaDinh = "Khong ten thanh" };
            var chong = new GiaoDan { GiaoXuId = app.GiaoXuId, MaGiaoDanCu = ma * 10 + 1, HoTen = "Le Van Cuong",
                TenThanh = "", Phai = "Nam" };
            db.AddRange(gd, chong);
            db.ThanhVienGiaDinh.Add(
                new ThanhVienGiaDinh { GiaoXuId = app.GiaoXuId, GiaDinh = gd, GiaoDan = chong, VaiTro = VaiTroGiaDinh.Chong });
            await db.SaveChangesAsync();
        }

        var ds = await app.CreateClient().GetFromJsonAsync<List<Item>>("/api/gia-dinh");

        var dong = ds!.Single(x => x.MaGiaDinhCu == ma);
        dong.TenChong.Should().Be("Le Van Cuong");
        dong.TenChong.Should().NotStartWith(" ");
    }
}
