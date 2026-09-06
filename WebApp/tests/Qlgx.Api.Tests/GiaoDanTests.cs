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

        var ds = await app.CreateAuthClient().GetFromJsonAsync<List<Item>>("/api/giao-dan");

        ds!.Single(x => x.MaGiaoDanCu == 8001).NamSinh.Should().Be("1996");
    }

    [Fact]
    public async Task Danh_sach_de_trong_nam_sinh_khi_khong_co_ngay_sinh()
    {
        await TaoGiaoDan(8002, "Nguoi khong ro ngay sinh");

        var ds = await app.CreateAuthClient().GetFromJsonAsync<List<Item>>("/api/giao-dan");

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

        var ct = await app.CreateAuthClient().GetFromJsonAsync<ChiTiet>($"/api/giao-dan/{idNguoi}");

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

        var ds = await app.CreateAuthClient()
            .GetFromJsonAsync<List<Item>>($"/api/gia-dinh/{idGiaDinh}/thanh-vien");

        ds!.Should().ContainSingle().Which.HoTen.Should().Be("Vu Duc Duy");
    }

    [Fact]
    public async Task Cap_nhat_giao_dan_kiem_tra_phien_ban()
    {
        var id = await TaoGiaoDan(8005, "Nguoi se duoc sua");
        var client = app.CreateAuthClient();
        var truoc = await client.GetFromJsonAsync<ChiTiet>($"/api/giao-dan/{id}");

        var lanDau = await client.PutAsJsonAsync($"/api/giao-dan/{id}",
            new { HoTen = "Ten da sua", Phai = "Nam", NgaySinh = "2000-01-01", RowVersion = truoc!.RowVersion });
        var lanHai = await client.PutAsJsonAsync($"/api/giao-dan/{id}",
            new { HoTen = "Ten sua lan hai", Phai = "Nam", NgaySinh = "2000-01-01", RowVersion = truoc.RowVersion });

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

        var ds = await app.CreateAuthClient()
            .GetFromJsonAsync<List<Item>>($"/api/gia-dinh/{idGiaDinh}/thanh-vien");

        ds!.Should().ContainSingle().Which.QuanHe.Should().Be("Con");
    }

    [Fact]
    public async Task Chi_tiet_uu_tien_gia_dinh_ma_nguoi_do_la_chong_hoac_vo_khi_thuoc_nhieu_gia_dinh()
    {
        // Một giáo dân có thể thuộc nhiều gia đình cùng lúc (con ở nhà cha mẹ, đồng thời lập
        // gia đình riêng) — bản Access cho phép điều này. Màn hình chi tiết phải chọn gia đình
        // mà người đó là chồng/vợ, dù bản ghi "con" được ghi trước.
        //
        // Vòng sửa 1: vai trò "còn lại" ở đây CỐ TÌNH dùng giá trị VaiTro LẠ (100), không phải
        // VaiTroGiaDinh.Con (2). Test bản đầu dùng == 2 làm vai trò còn lại nên không bắt được
        // lỗi so sánh cứng `tv.VaiTro == VaiTroGiaDinh.Con` trong mã gốc của brief: với giá trị
        // 2 thì == Con và > Vo cho CÙNG một kết quả, nên mã sai vẫn qua được test. Chỉ với giá
        // trị không phải 2 (như 100, đọc được từ .mdb thật) hai cách viết mới cho kết quả khác
        // nhau.
        //
        // Id của hai gia đình được gán CỐ ĐỊNH (không để Guid.NewGuid() ngẫu nhiên): đo được
        // rằng khi mã lỗi == Con tạo ra HAI bản ghi CÙNG khoá sắp xếp (vì cả VaiTro=100 lẫn
        // VaiTro=Chồng=0 đều so == Con(2) ra false), EF Core phá vỡ thế hoà bằng thứ tự Id của
        // GiaDinh — nếu để Guid.NewGuid() ngẫu nhiên, test này ĐỎ hay XANH tuỳ may rủi (đã đo
        // 5 lần chạy lặp lại: có lần đỏ có lần xanh với CÙNG một mã lỗi). Gán Id gia đình "vai
        // trò lạ" NHỎ HƠN Id gia đình "chồng/vợ" thì mã lỗi luôn thua thế hoà và bị bắt lỗi
        // 100% số lần — xem "Cách tự kiểm chứng" trong task-8-report.md để biết cách đã đo.
        var idNguoi = await TaoGiaoDan(8200, "Vu Van Con Rieng");
        Guid idGiaDinhRieng;
        await using (var db = app.TaoContextThuan())
        {
            var nhaChaMe = new GiaDinh { Id = Guid.Parse("00000000-0000-0000-0000-000000000001"), GiaoXuId = app.GiaoXuId, MaGiaDinhCu = 8201, TenGiaDinh = "Nha cha me" };
            var nhaRieng = new GiaDinh { Id = Guid.Parse("00000000-0000-0000-0000-000000000002"), GiaoXuId = app.GiaoXuId, MaGiaDinhCu = 8202, TenGiaDinh = "Nha rieng" };
            db.AddRange(nhaChaMe, nhaRieng);
            // Cố tình thêm bản ghi "vai trò lạ" TRƯỚC bản ghi "Chồng" để khẳng định việc chọn
            // không phụ thuộc thứ tự chèn.
            db.ThanhVienGiaDinh.Add(new ThanhVienGiaDinh
            {
                GiaoXuId = app.GiaoXuId, GiaDinhId = nhaChaMe.Id, GiaoDanId = idNguoi,
                VaiTro = (VaiTroGiaDinh)100
            });
            db.ThanhVienGiaDinh.Add(new ThanhVienGiaDinh
            {
                GiaoXuId = app.GiaoXuId, GiaDinhId = nhaRieng.Id, GiaoDanId = idNguoi,
                VaiTro = VaiTroGiaDinh.Chong, ChuHo = true
            });
            await db.SaveChangesAsync();
            idGiaDinhRieng = nhaRieng.Id;
        }

        var ct = await app.CreateAuthClient().GetFromJsonAsync<ChiTiet>($"/api/giao-dan/{idNguoi}");

        ct!.GiaDinhId.Should().Be(idGiaDinhRieng);
        ct.TenGiaDinh.Should().Be("Nha rieng");
        ct.VaiTro.Should().Be(0);
    }

    [Fact]
    public async Task Danh_sach_uu_tien_gia_dinh_ma_nguoi_do_la_chong_hoac_vo_khi_thuoc_nhieu_gia_dinh()
    {
        // Cùng lý do đã ghi ở test chi tiết phía trên: vai trò "còn lại" phải là giá trị LẠ
        // (100), không phải VaiTroGiaDinh.Con (2), và Id hai gia đình được gán cố định để tránh
        // phụ thuộc may rủi vào thứ tự phá thế hoà — dù mã LayDanhSach hiện tại (sắp tăng dần
        // theo VaiTro, không so == Con) vốn không có kiểu lỗi này, gán Id cố định vẫn giữ test
        // này đáng tin cậy 100% thay vì thỉnh thoảng ăn may.
        var idNguoi = await TaoGiaoDan(8210, "Vu Van Con Rieng Ds");
        Guid idGiaDinhRieng;
        await using (var db = app.TaoContextThuan())
        {
            var nhaChaMe = new GiaDinh { Id = Guid.Parse("00000000-0000-0000-0000-000000000003"), GiaoXuId = app.GiaoXuId, MaGiaDinhCu = 8211, TenGiaDinh = "Nha cha me ds" };
            var nhaRieng = new GiaDinh { Id = Guid.Parse("00000000-0000-0000-0000-000000000004"), GiaoXuId = app.GiaoXuId, MaGiaDinhCu = 8212, TenGiaDinh = "Nha rieng ds" };
            db.AddRange(nhaChaMe, nhaRieng);
            db.ThanhVienGiaDinh.Add(new ThanhVienGiaDinh
            {
                GiaoXuId = app.GiaoXuId, GiaDinhId = nhaChaMe.Id, GiaoDanId = idNguoi,
                VaiTro = (VaiTroGiaDinh)100
            });
            db.ThanhVienGiaDinh.Add(new ThanhVienGiaDinh
            {
                GiaoXuId = app.GiaoXuId, GiaDinhId = nhaRieng.Id, GiaoDanId = idNguoi,
                VaiTro = VaiTroGiaDinh.Vo
            });
            await db.SaveChangesAsync();
            idGiaDinhRieng = nhaRieng.Id;
        }

        var ds = await app.CreateAuthClient().GetFromJsonAsync<List<Item>>("/api/giao-dan");

        ds!.Single(x => x.MaGiaoDanCu == 8210).GiaDinhId.Should().Be(idGiaDinhRieng);
    }

    [Fact]
    public async Task Thanh_vien_da_xoa_khong_hien_trong_luoi_thanh_vien_lan_danh_sach_giao_dan()
    {
        // Vòng sửa 1: LayThanhVien thiếu !DaXoa trong khi LayDanhSach/LayChiTiet đều lọc —
        // hậu quả là một giáo dân đã bị đánh dấu xoá (chuyện xảy ra khi gộp trùng dữ liệu từ
        // Access) sẽ biến mất khỏi /api/giao-dan nhưng vẫn hiện trong lưới thành viên gia đình.
        var idChong = await TaoGiaoDan(8400, "Chong con song");
        var idVo = await TaoGiaoDan(8401, "Vo con song");
        var idDaXoa = await TaoGiaoDan(8402, "Con da bi xoa");
        Guid idGiaDinh;
        await using (var db = app.TaoContextThuan())
        {
            var giaDinh = new GiaDinh { GiaoXuId = app.GiaoXuId, MaGiaDinhCu = 8400, TenGiaDinh = "GD co nguoi da xoa" };
            db.GiaDinh.Add(giaDinh);
            db.ThanhVienGiaDinh.AddRange(
                new ThanhVienGiaDinh { GiaoXuId = app.GiaoXuId, GiaDinhId = giaDinh.Id, GiaoDanId = idChong, VaiTro = VaiTroGiaDinh.Chong, ChuHo = true },
                new ThanhVienGiaDinh { GiaoXuId = app.GiaoXuId, GiaDinhId = giaDinh.Id, GiaoDanId = idVo, VaiTro = VaiTroGiaDinh.Vo },
                new ThanhVienGiaDinh { GiaoXuId = app.GiaoXuId, GiaDinhId = giaDinh.Id, GiaoDanId = idDaXoa, VaiTro = VaiTroGiaDinh.Con });
            await db.SaveChangesAsync();
            idGiaDinh = giaDinh.Id;

            var nguoiDaXoa = await db.GiaoDan.SingleAsync(x => x.Id == idDaXoa);
            nguoiDaXoa.DaXoa = true;
            await db.SaveChangesAsync();
        }

        var client = app.CreateAuthClient();
        var thanhVien = await client.GetFromJsonAsync<List<Item>>($"/api/gia-dinh/{idGiaDinh}/thanh-vien");
        var danhSach = await client.GetFromJsonAsync<List<Item>>("/api/giao-dan");

        thanhVien!.Should().HaveCount(2, "nguoi da bi xoa khong duoc hien trong luoi thanh vien gia dinh");
        thanhVien!.Select(x => x.HoTen).Should().BeEquivalentTo(["Chong con song", "Vo con song"]);
        danhSach!.Should().NotContain(x => x.MaGiaoDanCu == 8402,
            "nguoi da bi xoa cung khong duoc hien trong danh sach giao dan");
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
        var client = app.CreateAuthClient();
        var truoc = await client.GetFromJsonAsync<ChiTiet>($"/api/giao-dan/{id}");

        var res = await client.PutAsJsonAsync($"/api/giao-dan/{id}",
            new { HoTen = "Ten da sua qua PUT", Phai = "Nam", NgaySinh = "2000-01-01", RowVersion = truoc!.RowVersion });

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        await using var dbSau = app.TaoContextThuan();
        (await dbSau.GiaoDan.SingleAsync(x => x.Id == id)).MaNhanDang
            .Should().Be("access-2026-09-06::giao_dan::8300",
                "MaNhanDang la khoa dong bo hai chieu voi ban desktop, API cap nhat khong duoc dong tay vao");
    }
}
