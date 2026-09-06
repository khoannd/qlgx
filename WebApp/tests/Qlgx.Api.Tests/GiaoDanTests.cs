using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Qlgx.Domain;
using Qlgx.Domain.Entities;

namespace Qlgx.Api.Tests;

public class GiaoDanTests(QlgxApiFactory app) : IClassFixture<QlgxApiFactory>
{
    private sealed record Item(Guid Id, int MaGiaoDanCu, string? TenThanh, string HoTen,
        string? Phai, DateOnly? NgaySinh, string? NamSinh, DateOnly? NgayRuaToi,
        bool QuaDoi, bool DaChuyenDi, string? TenGiaoHo, bool LapGd, string? QuanHe,
        Guid? GiaDinhId, bool KhongThongKe);
    private sealed record ChiTiet(Guid Id, string HoTen, string? TenThanh, DateOnly? NgaySinh,
        DateOnly? NgayRuaToi, string? NoiRuaToi, bool QuaDoi, DateOnly? NgayQuaDoi,
        Guid? GiaDinhId, string? TenGiaDinh, int? VaiTro, uint RowVersion);
    private sealed record ThongBaoLoi(string ThongBao);

    private async Task<Guid> TaoGiaoDan(int ma, string hoTen, bool quaDoi = false,
        DateOnly? ngaySinh = null)
    {
        await using var db = app.TaoContextThuan();
        var gd = new GiaoDan
        {
            GiaoXuId = app.GiaoXuId, MaGiaoDanCu = ma, HoTen = hoTen, TenThanh = "Giuse",
            Phai = "Nam", QuaDoi = quaDoi, NgaySinh = ngaySinh,
            NgayRuaToi = new DateOnly(1996, 4, 21), NoiRuaToi = "GX Thanh Tam"
        };
        db.GiaoDan.Add(gd);
        await db.SaveChangesAsync();
        return gd.Id;
    }

    [Fact]
    public async Task Danh_sach_suy_ra_nam_sinh_tu_ngay_sinh()
    {
        await TaoGiaoDan(8001, "Vu Minh Tri", ngaySinh: new DateOnly(1996, 4, 2));

        var ds = await app.CreateClient().GetFromJsonAsync<List<Item>>("/api/giao-dan");

        ds!.Single(x => x.MaGiaoDanCu == 8001).NamSinh.Should().Be("1996");
    }

    [Fact]
    public async Task Danh_sach_de_trong_nam_sinh_khi_khong_co_ngay_sinh()
    {
        await TaoGiaoDan(8002, "Nguoi khong ro ngay sinh");

        var ds = await app.CreateClient().GetFromJsonAsync<List<Item>>("/api/giao-dan");

        ds!.Single(x => x.MaGiaoDanCu == 8002).NamSinh.Should().BeEmpty();
    }

    [Fact]
    public async Task Chi_tiet_tra_ve_gia_dinh_va_vai_tro_cua_nguoi_do()
    {
        var idNguoi = await TaoGiaoDan(8003, "Vu Tien Dung");
        await using (var db = app.TaoContextThuan())
        {
            var giaDinh = new GiaDinh { GiaoXuId = app.GiaoXuId, MaGiaDinhCu = 8003, TenGiaDinh = "Dung - Thu" };
            db.GiaDinh.Add(giaDinh);
            db.ThanhVienGiaDinh.Add(new ThanhVienGiaDinh
            {
                GiaoXuId = app.GiaoXuId, GiaDinhId = giaDinh.Id, GiaoDanId = idNguoi,
                VaiTro = VaiTroGiaDinh.Chong, ChuHo = true
            });
            await db.SaveChangesAsync();
        }

        var ct = await app.CreateClient().GetFromJsonAsync<ChiTiet>($"/api/giao-dan/{idNguoi}");

        ct!.TenGiaDinh.Should().Be("Dung - Thu");
        ct.VaiTro.Should().Be(0);
    }

    [Fact]
    public async Task Lay_duoc_thanh_vien_cua_mot_gia_dinh_qua_endpoint_rieng()
    {
        var idCon = await TaoGiaoDan(8004, "Vu Duc Duy");
        Guid idGiaDinh;
        await using (var db = app.TaoContextThuan())
        {
            var giaDinh = new GiaDinh { GiaoXuId = app.GiaoXuId, MaGiaDinhCu = 8004, TenGiaDinh = "Gia dinh co con" };
            db.GiaDinh.Add(giaDinh);
            db.ThanhVienGiaDinh.Add(new ThanhVienGiaDinh
            {
                GiaoXuId = app.GiaoXuId, GiaDinhId = giaDinh.Id, GiaoDanId = idCon,
                VaiTro = VaiTroGiaDinh.Con
            });
            await db.SaveChangesAsync();
            idGiaDinh = giaDinh.Id;
        }

        var ds = await app.CreateClient()
            .GetFromJsonAsync<List<Item>>($"/api/gia-dinh/{idGiaDinh}/thanh-vien");

        ds!.Should().ContainSingle().Which.HoTen.Should().Be("Vu Duc Duy");
    }

    [Fact]
    public async Task Cap_nhat_giao_dan_kiem_tra_phien_ban()
    {
        var id = await TaoGiaoDan(8005, "Nguoi se duoc sua");
        var client = app.CreateClient();
        var truoc = await client.GetFromJsonAsync<ChiTiet>($"/api/giao-dan/{id}");

        var lanDau = await client.PutAsJsonAsync($"/api/giao-dan/{id}",
            new { HoTen = "Ten da sua", RowVersion = truoc!.RowVersion });
        var lanHai = await client.PutAsJsonAsync($"/api/giao-dan/{id}",
            new { HoTen = "Ten sua lan hai", RowVersion = truoc.RowVersion });

        lanDau.StatusCode.Should().Be(HttpStatusCode.OK);
        lanHai.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    // --- Ngoài brief: dữ liệu thật đọc trực tiếp từ .mdb cho thấy ThanhVienGiaDinh.VaiTro có
    // BẢY giá trị (0,1,2,3,8,18,100), không chỉ ba giá trị enum đặt tên. Bản desktop coi
    // VaiTro > 1 là "con cái" — các test dưới đây khẳng định API web giữ đúng quy tắc đó thay
    // vì so sánh cứng với VaiTroGiaDinh.Con (== 2), để dữ liệu chuyển đổi từ Access không bị
    // xếp nhầm vai trò.

    [Theory]
    [InlineData(0, 3)]
    [InlineData(1, 8)]
    [InlineData(2, 18)]
    [InlineData(3, 100)]
    public async Task Thanh_vien_co_gia_tri_VaiTro_la_hoac_khac_thuong_van_duoc_bao_la_Con(
        int thuTu, int maVaiTroTho)
    {
        var ma = 8010 + thuTu;
        var idCon = await TaoGiaoDan(ma, "Nguoi vai tro " + maVaiTroTho);
        Guid idGiaDinh;
        await using (var db = app.TaoContextThuan())
        {
            var giaDinh = new GiaDinh
            {
                GiaoXuId = app.GiaoXuId, MaGiaDinhCu = ma, TenGiaDinh = "GD vai tro la"
            };
            db.GiaDinh.Add(giaDinh);
            db.ThanhVienGiaDinh.Add(new ThanhVienGiaDinh
            {
                GiaoXuId = app.GiaoXuId, GiaDinhId = giaDinh.Id, GiaoDanId = idCon,
                VaiTro = (VaiTroGiaDinh)maVaiTroTho
            });
            await db.SaveChangesAsync();
            idGiaDinh = giaDinh.Id;
        }

        var ds = await app.CreateClient()
            .GetFromJsonAsync<List<Item>>($"/api/gia-dinh/{idGiaDinh}/thanh-vien");

        ds!.Should().ContainSingle().Which.QuanHe.Should().Be("Con");
    }

    [Fact]
    public async Task Chi_tiet_uu_tien_gia_dinh_ma_nguoi_do_la_chong_hoac_vo_khi_thuoc_nhieu_gia_dinh()
    {
        // Một giáo dân có thể thuộc nhiều gia đình cùng lúc (con ở nhà cha mẹ, đồng thời lập
        // gia đình riêng) — bản Access cho phép điều này. Màn hình chi tiết phải chọn gia đình
        // mà người đó là chồng/vợ, dù bản ghi "con" được ghi trước.
        var idNguoi = await TaoGiaoDan(8200, "Vu Van Con Rieng");
        Guid idGiaDinhRieng;
        await using (var db = app.TaoContextThuan())
        {
            var nhaChaMe = new GiaDinh { GiaoXuId = app.GiaoXuId, MaGiaDinhCu = 8201, TenGiaDinh = "Nha cha me" };
            var nhaRieng = new GiaDinh { GiaoXuId = app.GiaoXuId, MaGiaDinhCu = 8202, TenGiaDinh = "Nha rieng" };
            db.AddRange(nhaChaMe, nhaRieng);
            // Cố tình thêm bản ghi "Con" TRƯỚC bản ghi "Chồng" để khẳng định việc chọn không
            // phụ thuộc thứ tự chèn.
            db.ThanhVienGiaDinh.Add(new ThanhVienGiaDinh
            {
                GiaoXuId = app.GiaoXuId, GiaDinhId = nhaChaMe.Id, GiaoDanId = idNguoi,
                VaiTro = VaiTroGiaDinh.Con
            });
            db.ThanhVienGiaDinh.Add(new ThanhVienGiaDinh
            {
                GiaoXuId = app.GiaoXuId, GiaDinhId = nhaRieng.Id, GiaoDanId = idNguoi,
                VaiTro = VaiTroGiaDinh.Chong, ChuHo = true
            });
            await db.SaveChangesAsync();
            idGiaDinhRieng = nhaRieng.Id;
        }

        var ct = await app.CreateClient().GetFromJsonAsync<ChiTiet>($"/api/giao-dan/{idNguoi}");

        ct!.GiaDinhId.Should().Be(idGiaDinhRieng);
        ct.TenGiaDinh.Should().Be("Nha rieng");
        ct.VaiTro.Should().Be(0);
    }

    [Fact]
    public async Task Danh_sach_uu_tien_gia_dinh_ma_nguoi_do_la_chong_hoac_vo_khi_thuoc_nhieu_gia_dinh()
    {
        var idNguoi = await TaoGiaoDan(8210, "Vu Van Con Rieng Ds");
        Guid idGiaDinhRieng;
        await using (var db = app.TaoContextThuan())
        {
            var nhaChaMe = new GiaDinh { GiaoXuId = app.GiaoXuId, MaGiaDinhCu = 8211, TenGiaDinh = "Nha cha me ds" };
            var nhaRieng = new GiaDinh { GiaoXuId = app.GiaoXuId, MaGiaDinhCu = 8212, TenGiaDinh = "Nha rieng ds" };
            db.AddRange(nhaChaMe, nhaRieng);
            db.ThanhVienGiaDinh.Add(new ThanhVienGiaDinh
            {
                GiaoXuId = app.GiaoXuId, GiaDinhId = nhaChaMe.Id, GiaoDanId = idNguoi,
                VaiTro = VaiTroGiaDinh.Con
            });
            db.ThanhVienGiaDinh.Add(new ThanhVienGiaDinh
            {
                GiaoXuId = app.GiaoXuId, GiaDinhId = nhaRieng.Id, GiaoDanId = idNguoi,
                VaiTro = VaiTroGiaDinh.Vo
            });
            await db.SaveChangesAsync();
            idGiaDinhRieng = nhaRieng.Id;
        }

        var ds = await app.CreateClient().GetFromJsonAsync<List<Item>>("/api/giao-dan");

        ds!.Single(x => x.MaGiaoDanCu == 8210).GiaDinhId.Should().Be(idGiaDinhRieng);
    }

    [Fact]
    public async Task Cap_nhat_khong_duoc_dong_toi_MaNhanDang()
    {
        var id = await TaoGiaoDan(8300, "Nguoi co ma nhan dang");
        await using (var db = app.TaoContextThuan())
        {
            var gd = await db.GiaoDan.SingleAsync(x => x.Id == id);
            gd.MaNhanDang = "access-2026-09-06::giao_dan::8300";
            await db.SaveChangesAsync();
        }
        var client = app.CreateClient();
        var truoc = await client.GetFromJsonAsync<ChiTiet>($"/api/giao-dan/{id}");

        var res = await client.PutAsJsonAsync($"/api/giao-dan/{id}",
            new { HoTen = "Ten da sua qua PUT", RowVersion = truoc!.RowVersion });

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        await using var dbSau = app.TaoContextThuan();
        (await dbSau.GiaoDan.SingleAsync(x => x.Id == id)).MaNhanDang
            .Should().Be("access-2026-09-06::giao_dan::8300",
                "MaNhanDang la khoa dong bo hai chieu voi ban desktop, API cap nhat khong duoc dong tay vao");
    }
}
