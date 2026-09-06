using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Qlgx.Domain;
using Qlgx.Domain.Entities;

namespace Qlgx.Api.Tests;

public class GiaDinhDetailTests(QlgxApiFactory app) : IClassFixture<QlgxApiFactory>
{
    private sealed record ThanhVien(Guid GiaoDanId, int VaiTro, bool ChuHo, string? TenThanh,
        string HoTen, string? Phai, DateOnly? NgaySinh, bool QuaDoi);

    /// <summary>Đối chiếu với Qlgx.Api.Dtos.HonPhoiDto — đặt tên khác để không đụng
    /// Qlgx.Domain.Entities.HonPhoi vốn đã được using ở trên.</summary>
    private sealed record HonPhoiChiTiet(Guid Id, string? SoHonPhoi, DateOnly? NgayHonPhoi,
        string? NoiHonPhoi, string? LinhMucChung, string? NguoiChung1, string? NguoiChung2,
        string? CachThucHonPhoi, string? GhiChu, uint RowVersion);

    private sealed record ChiTiet(Guid Id, int MaGiaDinhCu, string? TenGiaDinh, string? DiaChi,
        bool DaChuyenXu, uint RowVersion, ThanhVien[] ThanhVien, HonPhoiChiTiet? HonPhoi);

    private sealed record CapNhatHonPhoi(string? SoHonPhoi, DateOnly? NgayHonPhoi,
        string? NoiHonPhoi, string? LinhMucChung, string? NguoiChung1, string? NguoiChung2,
        string? CachThucHonPhoi, string? GhiChu, uint RowVersion);

    private sealed record CapNhat(string? TenGiaDinh, Guid? GiaoHoId, string? DienThoai,
        string? DiaChi, string? SoHoKhau, string? DienGiaDinh, string? GhiChu, bool DaChuyenXu,
        DateOnly? NgayChuyen, string? NoiChuyen, bool KhongThongKe, uint RowVersion,
        CapNhatHonPhoi? HonPhoi = null);

    private sealed record ThongBaoLoi(string ThongBao);

    private async Task<Guid> TaoGiaDinh(int ma)
    {
        await using var db = app.TaoContextThuan();
        var gd = new GiaDinh { GiaoXuId = app.GiaoXuId, MaGiaDinhCu = ma, TenGiaDinh = "Dung - Thu" };
        var chong = new GiaoDan { GiaoXuId = app.GiaoXuId, MaGiaoDanCu = ma * 10, HoTen = "Vu Tien Dung",
            TenThanh = "Daminh", Phai = "Nam" };
        db.AddRange(gd, chong);
        db.ThanhVienGiaDinh.Add(new ThanhVienGiaDinh { GiaoXuId = app.GiaoXuId, GiaDinh = gd,
            GiaoDan = chong, VaiTro = VaiTroGiaDinh.Chong, ChuHo = true });
        await db.SaveChangesAsync();
        return gd.Id;
    }

    /// <summary>Tạo gia đình có chồng (luôn có) và vợ (tuỳ chọn) — dùng cho các test hôn phối,
    /// vốn cần biết mã giáo dân của từng bên để đối chiếu bảng nối GiaoDanHonPhoi.</summary>
    private async Task<(Guid GiaDinhId, Guid ChongId, Guid? VoId)> TaoGiaDinhVoChong(int ma, bool coVo)
    {
        await using var db = app.TaoContextThuan();
        var gd = new GiaDinh { GiaoXuId = app.GiaoXuId, MaGiaDinhCu = ma, TenGiaDinh = "Ho gia dinh " + ma };
        var chong = new GiaoDan { GiaoXuId = app.GiaoXuId, MaGiaoDanCu = ma * 10 + 1, HoTen = "Chong " + ma,
            Phai = "Nam" };
        db.AddRange(gd, chong);
        db.ThanhVienGiaDinh.Add(new ThanhVienGiaDinh { GiaoXuId = app.GiaoXuId, GiaDinh = gd,
            GiaoDan = chong, VaiTro = VaiTroGiaDinh.Chong, ChuHo = true });

        Guid? voId = null;
        if (coVo)
        {
            var vo = new GiaoDan { GiaoXuId = app.GiaoXuId, MaGiaoDanCu = ma * 10 + 2, HoTen = "Vo " + ma,
                Phai = "Nu" };
            db.Add(vo);
            db.ThanhVienGiaDinh.Add(new ThanhVienGiaDinh { GiaoXuId = app.GiaoXuId, GiaDinh = gd,
                GiaoDan = vo, VaiTro = VaiTroGiaDinh.Vo });
            voId = vo.Id;
        }

        await db.SaveChangesAsync();
        return (gd.Id, chong.Id, voId);
    }

    /// <summary>
    /// Mã cũ kế tiếp cho một bản ghi HonPhoi tạo thẳng trong test — KHÔNG được gán cứng theo
    /// tham số ma của từng test, vì GiaDinhService.GhiHonPhoi cũng tự sinh mã kế tiếp theo
    /// đúng công thức MAX+1 khi tạo hôn phối mới qua PUT (test
    /// Gui_khoi_hon_phoi_khi_gia_dinh_..._tao_moi). Các test trong lớp này dùng chung một
    /// database (IClassFixture), thứ tự chạy của xUnit không đảm bảo theo khai báo, nên một
    /// mã cứng có thể trùng với mã do một test khác tự sinh ra trước đó — tính lại y hệt công
    /// thức sản phẩm để tránh đụng ràng buộc duy nhất (GiaoXuId, MaHonPhoiCu).
    /// </summary>
    private static async Task<int> MaHonPhoiKeTiep(Qlgx.Data.QlgxDbContext db) =>
        (await db.HonPhoi.MaxAsync(h => (int?)h.MaHonPhoiCu)) is { } max ? max + 1 : 1;

    [Fact]
    public async Task Lay_chi_tiet_kem_danh_sach_thanh_vien()
    {
        var id = await TaoGiaDinh(300);

        var ct = await app.CreateClient().GetFromJsonAsync<ChiTiet>($"/api/gia-dinh/{id}");

        ct!.TenGiaDinh.Should().Be("Dung - Thu");
        ct.ThanhVien.Should().ContainSingle();
        ct.ThanhVien[0].HoTen.Should().Be("Vu Tien Dung");
        ct.ThanhVien[0].VaiTro.Should().Be(0);
        ct.ThanhVien[0].ChuHo.Should().BeTrue();
        ct.RowVersion.Should().BeGreaterThan(0u);
    }

    [Fact]
    public async Task Cap_nhat_thanh_cong_khi_dung_phien_ban()
    {
        var id = await TaoGiaDinh(301);
        var client = app.CreateClient();
        var truoc = await client.GetFromJsonAsync<ChiTiet>($"/api/gia-dinh/{id}");

        var res = await client.PutAsJsonAsync($"/api/gia-dinh/{id}", new CapNhat(
            "Dung - Thu (da sua)", null, "028 3775 1120", "7 Hem 24 Tran Phu",
            null, null, null, false, null, null, false, truoc!.RowVersion));

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        var sau = await client.GetFromJsonAsync<ChiTiet>($"/api/gia-dinh/{id}");
        sau!.TenGiaDinh.Should().Be("Dung - Thu (da sua)");
    }

    [Fact]
    public async Task Hai_nguoi_cung_sua_thi_nguoi_sau_nhan_409_thay_vi_ghi_de_im_lang()
    {
        var id = await TaoGiaDinh(302);
        var client = app.CreateClient();
        var banA = await client.GetFromJsonAsync<ChiTiet>($"/api/gia-dinh/{id}");
        var banB = await client.GetFromJsonAsync<ChiTiet>($"/api/gia-dinh/{id}");

        var luuA = await client.PutAsJsonAsync($"/api/gia-dinh/{id}", new CapNhat(
            "Nguoi A sua", null, null, null, null, null, null, false, null, null, false, banA!.RowVersion));
        var luuB = await client.PutAsJsonAsync($"/api/gia-dinh/{id}", new CapNhat(
            "Nguoi B sua", null, null, null, null, null, null, false, null, null, false, banB!.RowVersion));

        luuA.StatusCode.Should().Be(HttpStatusCode.OK);
        luuB.StatusCode.Should().Be(HttpStatusCode.Conflict,
            "ban Access ghi de im lang trong tinh huong nay, ban web phai bao cho nguoi dung");
        var loi = await luuB.Content.ReadFromJsonAsync<ThongBaoLoi>();
        loi!.ThongBao.Should().Contain("Gia đình",
            "thong bao 409 phai phan biet dung ghi de o ban than gia dinh, khong phai o khoi hon phoi");
        var cuoi = await client.GetFromJsonAsync<ChiTiet>($"/api/gia-dinh/{id}");
        cuoi!.TenGiaDinh.Should().Be("Nguoi A sua");
    }

    [Fact]
    public async Task Khong_tim_thay_thi_tra_404()
    {
        var res = await app.CreateClient().GetAsync($"/api/gia-dinh/{Guid.NewGuid()}");

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // --- Khối hôn phối (chuyển từ Phase 2 lên Phase 1) ---

    [Fact]
    public async Task Chi_tiet_gia_dinh_chua_co_hon_phoi_thi_HonPhoi_null()
    {
        var (id, _, _) = await TaoGiaDinhVoChong(310, coVo: true);

        var ct = await app.CreateClient().GetFromJsonAsync<ChiTiet>($"/api/gia-dinh/{id}");

        ct!.HonPhoi.Should().BeNull();
    }

    [Fact]
    public async Task Chi_tiet_gia_dinh_co_hon_phoi_tra_ve_du_cac_truong()
    {
        var (id, chongId, voId) = await TaoGiaDinhVoChong(311, coVo: true);
        Guid honPhoiId;
        await using (var db = app.TaoContextThuan())
        {
            var hp = new HonPhoi
            {
                GiaoXuId = app.GiaoXuId, MaHonPhoiCu = await MaHonPhoiKeTiep(db), SoHonPhoi = "12/2020",
                NgayHonPhoi = new DateOnly(2020, 1, 15), NoiHonPhoi = "Nha tho Chinh toa",
                LinhMucChung = "Lm. Nguyen Van A", NguoiChung1 = "Ong B", NguoiChung2 = "Ba C",
                CachThucHonPhoi = "Thanh su", GhiChu = "Khong co gi dac biet",
            };
            db.HonPhoi.Add(hp);
            db.GiaoDanHonPhoi.AddRange(
                new GiaoDanHonPhoi { GiaoXuId = app.GiaoXuId, HonPhoi = hp, GiaoDanId = chongId, SoThuTu = 1 },
                new GiaoDanHonPhoi { GiaoXuId = app.GiaoXuId, HonPhoi = hp, GiaoDanId = voId!.Value, SoThuTu = 2 });
            await db.SaveChangesAsync();
            honPhoiId = hp.Id;
        }

        var ct = await app.CreateClient().GetFromJsonAsync<ChiTiet>($"/api/gia-dinh/{id}");

        ct!.HonPhoi.Should().NotBeNull();
        ct.HonPhoi!.Id.Should().Be(honPhoiId);
        ct.HonPhoi.SoHonPhoi.Should().Be("12/2020");
        ct.HonPhoi.NgayHonPhoi.Should().Be(new DateOnly(2020, 1, 15));
        ct.HonPhoi.NoiHonPhoi.Should().Be("Nha tho Chinh toa");
        ct.HonPhoi.LinhMucChung.Should().Be("Lm. Nguyen Van A");
        ct.HonPhoi.NguoiChung1.Should().Be("Ong B");
        ct.HonPhoi.NguoiChung2.Should().Be("Ba C");
        ct.HonPhoi.CachThucHonPhoi.Should().Be("Thanh su");
        ct.HonPhoi.GhiChu.Should().Be("Khong co gi dac biet");
        ct.HonPhoi.RowVersion.Should().BeGreaterThan(0u);
    }

    [Fact]
    public async Task Gui_khoi_hon_phoi_khi_gia_dinh_chua_co_thi_tao_moi_va_noi_ca_hai_vo_chong()
    {
        var (id, chongId, voId) = await TaoGiaDinhVoChong(312, coVo: true);
        var client = app.CreateClient();
        var truoc = await client.GetFromJsonAsync<ChiTiet>($"/api/gia-dinh/{id}");
        truoc!.HonPhoi.Should().BeNull();

        var res = await client.PutAsJsonAsync($"/api/gia-dinh/{id}", new CapNhat(
            truoc.TenGiaDinh, null, null, truoc.DiaChi, null, null, null, false, null, null, false,
            truoc.RowVersion,
            new CapNhatHonPhoi("15/2021", new DateOnly(2021, 3, 20), "Nha tho X",
                "Lm. Tran Van B", "Ong D", "Ba E", "Thanh su", "Ghi chu moi", 0)));

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        var sau = await client.GetFromJsonAsync<ChiTiet>($"/api/gia-dinh/{id}");
        sau!.HonPhoi.Should().NotBeNull();
        sau.HonPhoi!.SoHonPhoi.Should().Be("15/2021");
        sau.HonPhoi.RowVersion.Should().BeGreaterThan(0u);

        await using var db = app.TaoContextThuan();
        var lienKet = db.GiaoDanHonPhoi.Where(x => x.HonPhoiId == sau.HonPhoi.Id)
            .Select(x => x.GiaoDanId).ToList();
        lienKet.Should().BeEquivalentTo([chongId, voId!.Value]);
    }

    [Fact]
    public async Task Gui_khoi_hon_phoi_khi_gia_dinh_chi_co_mot_ben_thi_chi_noi_ben_do()
    {
        var (id, chongId, _) = await TaoGiaDinhVoChong(313, coVo: false);
        var client = app.CreateClient();
        var truoc = await client.GetFromJsonAsync<ChiTiet>($"/api/gia-dinh/{id}");

        var res = await client.PutAsJsonAsync($"/api/gia-dinh/{id}", new CapNhat(
            truoc!.TenGiaDinh, null, null, null, null, null, null, false, null, null, false,
            truoc.RowVersion,
            new CapNhatHonPhoi(null, null, null, null, null, null, null, null, 0)));

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        var sau = await client.GetFromJsonAsync<ChiTiet>($"/api/gia-dinh/{id}");
        sau!.HonPhoi.Should().NotBeNull();

        await using var db = app.TaoContextThuan();
        var lienKet = db.GiaoDanHonPhoi.Where(x => x.HonPhoiId == sau.HonPhoi!.Id)
            .Select(x => x.GiaoDanId).ToList();
        lienKet.Should().BeEquivalentTo([chongId]);
    }

    [Fact]
    public async Task Cap_nhat_khoi_hon_phoi_da_co_thanh_cong_khi_dung_phien_ban()
    {
        var (id, chongId, voId) = await TaoGiaDinhVoChong(314, coVo: true);
        Guid honPhoiId;
        await using (var db = app.TaoContextThuan())
        {
            var hp = new HonPhoi { GiaoXuId = app.GiaoXuId, MaHonPhoiCu = await MaHonPhoiKeTiep(db), SoHonPhoi = "1/2015" };
            db.HonPhoi.Add(hp);
            db.GiaoDanHonPhoi.AddRange(
                new GiaoDanHonPhoi { GiaoXuId = app.GiaoXuId, HonPhoi = hp, GiaoDanId = chongId, SoThuTu = 1 },
                new GiaoDanHonPhoi { GiaoXuId = app.GiaoXuId, HonPhoi = hp, GiaoDanId = voId!.Value, SoThuTu = 2 });
            await db.SaveChangesAsync();
            honPhoiId = hp.Id;
        }
        var client = app.CreateClient();
        var truoc = await client.GetFromJsonAsync<ChiTiet>($"/api/gia-dinh/{id}");

        var res = await client.PutAsJsonAsync($"/api/gia-dinh/{id}", new CapNhat(
            truoc!.TenGiaDinh, null, null, null, null, null, null, false, null, null, false,
            truoc.RowVersion,
            new CapNhatHonPhoi("1/2015 (da sua)", truoc.HonPhoi!.NgayHonPhoi, truoc.HonPhoi.NoiHonPhoi,
                truoc.HonPhoi.LinhMucChung, truoc.HonPhoi.NguoiChung1, truoc.HonPhoi.NguoiChung2,
                truoc.HonPhoi.CachThucHonPhoi, truoc.HonPhoi.GhiChu, truoc.HonPhoi.RowVersion)));

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        var sau = await client.GetFromJsonAsync<ChiTiet>($"/api/gia-dinh/{id}");
        sau!.HonPhoi!.Id.Should().Be(honPhoiId);
        sau.HonPhoi.SoHonPhoi.Should().Be("1/2015 (da sua)");
    }

    [Fact]
    public async Task Khong_gui_khoi_hon_phoi_thi_khong_dung_toi_hon_phoi_hien_co()
    {
        var (id, chongId, voId) = await TaoGiaDinhVoChong(315, coVo: true);
        await using (var db = app.TaoContextThuan())
        {
            var hp = new HonPhoi { GiaoXuId = app.GiaoXuId, MaHonPhoiCu = await MaHonPhoiKeTiep(db), SoHonPhoi = "Giu nguyen" };
            db.HonPhoi.Add(hp);
            db.GiaoDanHonPhoi.AddRange(
                new GiaoDanHonPhoi { GiaoXuId = app.GiaoXuId, HonPhoi = hp, GiaoDanId = chongId, SoThuTu = 1 },
                new GiaoDanHonPhoi { GiaoXuId = app.GiaoXuId, HonPhoi = hp, GiaoDanId = voId!.Value, SoThuTu = 2 });
            await db.SaveChangesAsync();
        }
        var client = app.CreateClient();
        var truoc = await client.GetFromJsonAsync<ChiTiet>($"/api/gia-dinh/{id}");

        var res = await client.PutAsJsonAsync($"/api/gia-dinh/{id}", new CapNhat(
            "Ten moi khong dung hon phoi", null, null, null, null, null, null, false, null, null, false,
            truoc!.RowVersion));

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        var sau = await client.GetFromJsonAsync<ChiTiet>($"/api/gia-dinh/{id}");
        sau!.TenGiaDinh.Should().Be("Ten moi khong dung hon phoi");
        sau.HonPhoi!.SoHonPhoi.Should().Be("Giu nguyen");
    }

    [Fact]
    public async Task Hai_nguoi_cung_sua_hon_phoi_thi_nguoi_sau_nhan_409()
    {
        var (id, chongId, voId) = await TaoGiaDinhVoChong(316, coVo: true);
        await using (var db = app.TaoContextThuan())
        {
            var hp = new HonPhoi { GiaoXuId = app.GiaoXuId, MaHonPhoiCu = await MaHonPhoiKeTiep(db), SoHonPhoi = "Ban dau" };
            db.HonPhoi.Add(hp);
            db.GiaoDanHonPhoi.AddRange(
                new GiaoDanHonPhoi { GiaoXuId = app.GiaoXuId, HonPhoi = hp, GiaoDanId = chongId, SoThuTu = 1 },
                new GiaoDanHonPhoi { GiaoXuId = app.GiaoXuId, HonPhoi = hp, GiaoDanId = voId!.Value, SoThuTu = 2 });
            await db.SaveChangesAsync();
        }
        var client = app.CreateClient();
        var banA = await client.GetFromJsonAsync<ChiTiet>($"/api/gia-dinh/{id}");
        var banB = await client.GetFromJsonAsync<ChiTiet>($"/api/gia-dinh/{id}");

        var luuA = await client.PutAsJsonAsync($"/api/gia-dinh/{id}", new CapNhat(
            banA!.TenGiaDinh, null, null, null, null, null, null, false, null, null, false, banA.RowVersion,
            new CapNhatHonPhoi("Nguoi A sua", null, null, null, null, null, null, null, banA.HonPhoi!.RowVersion)));
        var luuB = await client.PutAsJsonAsync($"/api/gia-dinh/{id}", new CapNhat(
            banB!.TenGiaDinh, null, null, null, null, null, null, false, null, null, false, banB.RowVersion,
            new CapNhatHonPhoi("Nguoi B sua", null, null, null, null, null, null, null, banB.HonPhoi!.RowVersion)));

        luuA.StatusCode.Should().Be(HttpStatusCode.OK);
        luuB.StatusCode.Should().Be(HttpStatusCode.Conflict,
            "hon phoi cung phai chong ghi de im lang giong nhu ban than gia dinh");
        var loi = await luuB.Content.ReadFromJsonAsync<ThongBaoLoi>();
        loi!.ThongBao.Should().Contain("hôn phối",
            "thong bao 409 phai phan biet dung ghi de o khoi hon phoi, khong phai o ban than gia dinh");

        var cuoi = await app.CreateClient().GetFromJsonAsync<ChiTiet>($"/api/gia-dinh/{id}");
        cuoi!.HonPhoi!.SoHonPhoi.Should().Be("Nguoi A sua");
    }

    // --- Vong sua 1: mot nguoi co the co nhieu hon phoi theo thoi gian (goa roi tai hon) ---

    [Fact]
    public async Task Chong_tung_co_hon_phoi_truoc_khong_gay_loi_va_tra_dung_hon_phoi_cap_hien_tai()
    {
        var (id, chongId, voId) = await TaoGiaDinhVoChong(320, coVo: true);
        Guid honPhoiHienTaiId;
        await using (var db = app.TaoContextThuan())
        {
            // Hon phoi CU cua rieng nguoi chong, voi mot nguoi khac ngoai gia dinh nay (da qua
            // doi hoac ly di, khong con lien quan) — NgayHonPhoi co tinh som hon.
            var hpCu = new HonPhoi
            {
                GiaoXuId = app.GiaoXuId, MaHonPhoiCu = await MaHonPhoiKeTiep(db),
                SoHonPhoi = "Hon phoi truoc", NgayHonPhoi = new DateOnly(2000, 1, 1),
            };
            db.HonPhoi.Add(hpCu);
            db.GiaoDanHonPhoi.Add(new GiaoDanHonPhoi
                { GiaoXuId = app.GiaoXuId, HonPhoi = hpCu, GiaoDanId = chongId, SoThuTu = 1 });
            await db.SaveChangesAsync();

            // Hon phoi HIEN TAI cua ca hai vo chong trong gia dinh nay — moi hon.
            var hpHienTai = new HonPhoi
            {
                GiaoXuId = app.GiaoXuId, MaHonPhoiCu = await MaHonPhoiKeTiep(db),
                SoHonPhoi = "Hon phoi hien tai", NgayHonPhoi = new DateOnly(2020, 6, 6),
            };
            db.HonPhoi.Add(hpHienTai);
            db.GiaoDanHonPhoi.AddRange(
                new GiaoDanHonPhoi { GiaoXuId = app.GiaoXuId, HonPhoi = hpHienTai, GiaoDanId = chongId, SoThuTu = 1 },
                new GiaoDanHonPhoi { GiaoXuId = app.GiaoXuId, HonPhoi = hpHienTai, GiaoDanId = voId!.Value, SoThuTu = 2 });
            await db.SaveChangesAsync();
            honPhoiHienTaiId = hpHienTai.Id;
        }

        var res = await app.CreateClient().GetAsync($"/api/gia-dinh/{id}");

        res.StatusCode.Should().Be(HttpStatusCode.OK,
            "nguoi chong tung co mot hon phoi truoc la du lieu HOP LE (goa roi tai hon), khong duoc gay loi 500");
        var ct = await res.Content.ReadFromJsonAsync<ChiTiet>();
        ct!.HonPhoi.Should().NotBeNull();
        ct.HonPhoi!.Id.Should().Be(honPhoiHienTaiId,
            "hon phoi hien tai la ban ghi CA HAI vo chong cung noi toi, khong phai hon phoi rieng truoc day cua chong");
        ct.HonPhoi.SoHonPhoi.Should().Be("Hon phoi hien tai");
    }

    [Fact]
    public async Task Gia_dinh_khong_co_chong_lan_vo_ma_gui_khoi_hon_phoi_thi_tra_400()
    {
        var ma = 321;
        Guid id;
        await using (var db = app.TaoContextThuan())
        {
            var gd = new GiaDinh { GiaoXuId = app.GiaoXuId, MaGiaDinhCu = ma, TenGiaDinh = "Gia dinh trong" };
            db.GiaDinh.Add(gd);
            await db.SaveChangesAsync();
            id = gd.Id;
        }
        var client = app.CreateClient();
        var truoc = await client.GetFromJsonAsync<ChiTiet>($"/api/gia-dinh/{id}");

        var res = await client.PutAsJsonAsync($"/api/gia-dinh/{id}", new CapNhat(
            truoc!.TenGiaDinh, null, null, null, null, null, null, false, null, null, false,
            truoc.RowVersion,
            new CapNhatHonPhoi("Khong the co", null, null, null, null, null, null, null, 0)));

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest,
            "gia dinh khong co chong lan vo thi khong the noi hon phoi voi ai — tu choi thay vi tao ban ghi mo coi");
        var loi = await res.Content.ReadFromJsonAsync<ThongBaoLoi>();
        loi!.ThongBao.Should().NotBeNullOrWhiteSpace();

        await using var dbSau = app.TaoContextThuan();
        (await dbSau.HonPhoi.AnyAsync(h => h.MaHonPhoiCu == ma || h.SoHonPhoi == "Khong the co"))
            .Should().BeFalse("khong duoc tao ban ghi hon phoi mo coi");
    }

    [Fact]
    public async Task Cap_nhat_khong_duoc_dung_toi_MaNhanDang_cua_gia_dinh_va_hon_phoi()
    {
        var (id, chongId, voId) = await TaoGiaDinhVoChong(322, coVo: true);
        Guid honPhoiId;
        await using (var db = app.TaoContextThuan())
        {
            var gd = await db.GiaDinh.SingleAsync(x => x.Id == id);
            gd.MaNhanDang = "access-2026-09-06::gia_dinh::322";
            var hp = new HonPhoi
            {
                GiaoXuId = app.GiaoXuId, MaHonPhoiCu = await MaHonPhoiKeTiep(db),
                SoHonPhoi = "Ma nhan dang test", MaNhanDang = "access-2026-09-06::hon_phoi::322",
            };
            db.HonPhoi.Add(hp);
            db.GiaoDanHonPhoi.AddRange(
                new GiaoDanHonPhoi { GiaoXuId = app.GiaoXuId, HonPhoi = hp, GiaoDanId = chongId, SoThuTu = 1 },
                new GiaoDanHonPhoi { GiaoXuId = app.GiaoXuId, HonPhoi = hp, GiaoDanId = voId!.Value, SoThuTu = 2 });
            await db.SaveChangesAsync();
            honPhoiId = hp.Id;
        }
        var client = app.CreateClient();
        var truoc = await client.GetFromJsonAsync<ChiTiet>($"/api/gia-dinh/{id}");

        var res = await client.PutAsJsonAsync($"/api/gia-dinh/{id}", new CapNhat(
            "Ten da sua", null, null, null, null, null, null, false, null, null, false, truoc!.RowVersion,
            new CapNhatHonPhoi("So da sua", null, null, null, null, null, null, null, truoc.HonPhoi!.RowVersion)));

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        await using var dbSau = app.TaoContextThuan();
        (await dbSau.GiaDinh.SingleAsync(x => x.Id == id)).MaNhanDang
            .Should().Be("access-2026-09-06::gia_dinh::322",
                "MaNhanDang la khoa dong bo hai chieu voi ban desktop, API cap nhat khong duoc dong tay vao");
        (await dbSau.HonPhoi.SingleAsync(x => x.Id == honPhoiId)).MaNhanDang
            .Should().Be("access-2026-09-06::hon_phoi::322",
                "MaNhanDang cua hon phoi cung khong duoc dong tay vao");
    }
}
