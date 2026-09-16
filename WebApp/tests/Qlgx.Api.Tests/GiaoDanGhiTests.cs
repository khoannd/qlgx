using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Qlgx.Domain;
using Qlgx.Domain.Entities;

namespace Qlgx.Api.Tests;

/// <summary>
/// Task "ghi giáo dân": tạo mới, xoá, và các kiểm tra nghiệp vụ phía máy chủ mà
/// GiaoDanService.KiemTraNghiepVu/Xoa mới thêm — trước đây bản web chỉ đọc/sửa được, không có
/// endpoint tạo mới hay xoá, và không kiểm tra nghiệp vụ nào phía server (xem
/// docs/superpowers/specs/man-hinh/giao-dan-chi-tiet.md mục 10 và giao-dan-danh-sach.md mục 10).
/// </summary>
public class GiaoDanGhiTests(QlgxApiFactory app) : IClassFixture<QlgxApiFactory>
{
    private sealed record ThongBaoLoi(string ThongBao);
    private sealed record KetQuaLuu(Guid? Id, string[] CanhBao);
    private sealed record Item(int MaGiaoDanCu, bool QuaDoi, bool DaChuyenDi);

    private static object YeuCauToiThieu(string hoTen = "Nguoi moi tao", string phai = "Nam",
        string ngaySinh = "2000-01-01", bool boQuaCanhBao = false) => new
    {
        HoTen = hoTen, Phai = phai, NgaySinh = ngaySinh, BoQuaCanhBao = boQuaCanhBao,
    };

    // --- Tạo mới ------------------------------------------------------------------------

    [Fact]
    public async Task Tao_giao_dan_hop_le_tra_ve_201_va_sinh_ma_giao_dan_cu()
    {
        var client = app.CreateAuthClient();

        var res = await client.PostAsJsonAsync("/api/giao-dan", YeuCauToiThieu("Nguoi Tao Moi 1"));

        res.StatusCode.Should().Be(HttpStatusCode.Created);
        var kq = await res.Content.ReadFromJsonAsync<KetQuaLuu>();
        kq!.Id.Should().NotBeNull();
        kq.CanhBao.Should().BeEmpty();

        await using var db = app.TaoContextThuan();
        var gd = await db.GiaoDan.SingleAsync(x => x.Id == kq.Id);
        gd.MaGiaoDanCu.Should().BeGreaterThan(0, "ma giao dan cu phai duoc SinhMaService cap phat, khong duoc de = 0");
        gd.HoTen.Should().Be("Nguoi Tao Moi 1");
        gd.MaNhanDang.Should().NotBeNullOrEmpty("ban ghi web tao moi phai duoc sinh MaNhanDang moi");
    }

    [Fact]
    public async Task Tao_giao_dan_hai_lan_khong_bi_trung_ma_giao_dan_cu()
    {
        var client = app.CreateAuthClient();

        var res1 = await client.PostAsJsonAsync("/api/giao-dan", YeuCauToiThieu("Nguoi A"));
        var res2 = await client.PostAsJsonAsync("/api/giao-dan", YeuCauToiThieu("Nguoi B"));
        var kq1 = await res1.Content.ReadFromJsonAsync<KetQuaLuu>();
        var kq2 = await res2.Content.ReadFromJsonAsync<KetQuaLuu>();

        await using var db = app.TaoContextThuan();
        var ma1 = (await db.GiaoDan.SingleAsync(x => x.Id == kq1!.Id)).MaGiaoDanCu;
        var ma2 = (await db.GiaoDan.SingleAsync(x => x.Id == kq2!.Id)).MaGiaoDanCu;
        ma1.Should().NotBe(ma2);
    }

    [Fact]
    public async Task Tao_giao_dan_thieu_ho_ten_tra_ve_400_voi_thong_bao_dung_nguyen_van_desktop()
    {
        var client = app.CreateAuthClient();

        var res = await client.PostAsJsonAsync("/api/giao-dan", new { Phai = "Nam", NgaySinh = "2000-01-01" });

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var loi = await res.Content.ReadFromJsonAsync<ThongBaoLoi>();
        loi!.ThongBao.Should().Be("Hãy nhập Họ tên");
    }

    [Fact]
    public async Task Tao_giao_dan_thieu_gioi_tinh_tra_ve_400()
    {
        var client = app.CreateAuthClient();

        var res = await client.PostAsJsonAsync("/api/giao-dan", new { HoTen = "Khong co gioi tinh", NgaySinh = "2000-01-01" });

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await res.Content.ReadFromJsonAsync<ThongBaoLoi>())!.ThongBao.Should().Be("Hãy nhập giới tính");
    }

    [Fact]
    public async Task Tao_giao_dan_thieu_ngay_sinh_tra_ve_400()
    {
        var client = app.CreateAuthClient();

        var res = await client.PostAsJsonAsync("/api/giao-dan", new { HoTen = "Khong co ngay sinh", Phai = "Nam" });

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await res.Content.ReadFromJsonAsync<ThongBaoLoi>())!.ThongBao.Should().Be("Hãy nhập ngày sinh hợp lệ");
    }

    [Fact]
    public async Task Tao_giao_dan_trung_ho_ten_ten_thanh_ngay_sinh_tra_ve_canh_bao_chua_luu()
    {
        var client = app.CreateAuthClient();
        await client.PostAsJsonAsync("/api/giao-dan", new
        {
            HoTen = "Nguyen Van Trung", TenThanh = "Giuse", NgaySinh = "1990-05-05", Phai = "Nam",
        });

        var res = await client.PostAsJsonAsync("/api/giao-dan", new
        {
            HoTen = "Nguyen Van Trung", TenThanh = "Giuse", NgaySinh = "1990-05-05", Phai = "Nam",
        });

        res.StatusCode.Should().Be(HttpStatusCode.OK, "co canh bao nen chua duoc luu, khong phai 201");
        var kq = await res.Content.ReadFromJsonAsync<KetQuaLuu>();
        kq!.Id.Should().BeNull();
        kq.CanhBao.Should().ContainSingle(c => c.Contains("Đã có giáo dân cùng họ tên"));

        await using var db = app.TaoContextThuan();
        (await db.GiaoDan.CountAsync(x => x.HoTen == "Nguyen Van Trung")).Should().Be(1,
            "chua xac nhan BoQuaCanhBao thi khong duoc luu ban ghi trung thu hai");
    }

    [Fact]
    public async Task Tao_giao_dan_trung_du_lieu_nhung_bo_qua_canh_bao_thi_van_luu_duoc()
    {
        var client = app.CreateAuthClient();
        await client.PostAsJsonAsync("/api/giao-dan", new
        {
            HoTen = "Tran Thi Trung Duoc Bo Qua", TenThanh = "Maria", NgaySinh = "1985-01-01", Phai = "Nữ",
        });

        var res = await client.PostAsJsonAsync("/api/giao-dan", new
        {
            HoTen = "Tran Thi Trung Duoc Bo Qua", TenThanh = "Maria", NgaySinh = "1985-01-01", Phai = "Nữ",
            BoQuaCanhBao = true,
        });

        res.StatusCode.Should().Be(HttpStatusCode.Created);
        await using var db = app.TaoContextThuan();
        (await db.GiaoDan.CountAsync(x => x.HoTen == "Tran Thi Trung Duoc Bo Qua")).Should().Be(2);
    }

    [Fact]
    public async Task Tao_giao_dan_ngay_rua_toi_truoc_ngay_sinh_tra_ve_canh_bao()
    {
        // Bug-for-bug: chỉ so Ngày sinh với Ngày rửa tội — xem can-review-sau.md mục 1.
        var client = app.CreateAuthClient();

        var res = await client.PostAsJsonAsync("/api/giao-dan", new
        {
            HoTen = "Ngay Rua Toi Sai", Phai = "Nam", NgaySinh = "2000-01-01", NgayRuaToi = "1999-01-01",
        });

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        var kq = await res.Content.ReadFromJsonAsync<KetQuaLuu>();
        kq!.CanhBao.Should().ContainSingle(c => c.Contains("Ngày sinh <= Ngày rửa tội"));
    }

    [Fact]
    public async Task Tao_giao_dan_ruoc_le_truoc_7_tuoi_tra_ve_canh_bao()
    {
        var client = app.CreateAuthClient();

        var res = await client.PostAsJsonAsync("/api/giao-dan", new
        {
            HoTen = "Ruoc Le Som", Phai = "Nam", NgaySinh = "2015-01-01", NgayRuocLe = "2020-01-01",
        });

        var kq = await res.Content.ReadFromJsonAsync<KetQuaLuu>();
        kq!.CanhBao.Should().ContainSingle(c => c.Contains("chưa được 7 tuổi"));
    }

    [Fact]
    public async Task Tao_giao_dan_da_rua_toi_ma_chua_ten_thanh_tra_ve_canh_bao()
    {
        var client = app.CreateAuthClient();

        var res = await client.PostAsJsonAsync("/api/giao-dan", new
        {
            HoTen = "Rua Toi Khong Ten Thanh", Phai = "Nam", NgaySinh = "2000-01-01", NgayRuaToi = "2000-02-01",
        });

        var kq = await res.Content.ReadFromJsonAsync<KetQuaLuu>();
        kq!.CanhBao.Should().ContainSingle(c => c.Contains("chưa được nhập Tên Thánh"));
    }

    [Fact]
    public async Task Tao_giao_dan_co_gia_dinh_duoi_14_tuoi_bi_chan_cung()
    {
        var client = app.CreateAuthClient();
        var namNay = DateTime.Now.Year;

        var res = await client.PostAsJsonAsync("/api/giao-dan", new
        {
            HoTen = "Qua Tre De Ket Hon", Phai = "Nam", NgaySinh = $"{namNay - 10}-01-01", DaCoGiaDinh = true,
        });

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await res.Content.ReadFromJsonAsync<ThongBaoLoi>())!.ThongBao
            .Should().Be("Giáo dân này hiện tại chưa đủ 14 tuổi. Không thể kết hôn");
    }

    [Fact]
    public async Task Tao_giao_dan_co_gia_dinh_tu_14_den_17_tuoi_chi_la_canh_bao()
    {
        var client = app.CreateAuthClient();
        var namNay = DateTime.Now.Year;

        var res = await client.PostAsJsonAsync("/api/giao-dan", new
        {
            HoTen = "Chua Du 18 De Ket Hon", Phai = "Nam", NgaySinh = $"{namNay - 16}-01-01", DaCoGiaDinh = true,
        });

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        var kq = await res.Content.ReadFromJsonAsync<KetQuaLuu>();
        kq!.Id.Should().BeNull();
        kq.CanhBao.Should().ContainSingle(c => c.Contains("chưa đủ 18 tuổi để kết hôn"));
    }

    // --- Cập nhật: chặn đổi giới tính khi đang là vợ/chồng -------------------------------

    [Fact]
    public async Task Doi_gioi_tinh_bi_chan_khi_dang_la_vo_chong_trong_gia_dinh()
    {
        Guid id;
        await using (var db = app.TaoContextThuan())
        {
            var gd = new GiaoDan
            {
                GiaoXuId = app.GiaoXuId, MaGiaoDanCu = 91000, HoTen = "Dang La Chong",
                Phai = "Nam", NgaySinh = new DateOnly(1990, 1, 1),
            };
            db.GiaoDan.Add(gd);
            var giaDinh = new GiaDinh { GiaoXuId = app.GiaoXuId, MaGiaDinhCu = 91000, TenGiaDinh = "GD test doi gioi tinh" };
            db.GiaDinh.Add(giaDinh);
            db.ThanhVienGiaDinh.Add(new ThanhVienGiaDinh
            {
                GiaoXuId = app.GiaoXuId, GiaDinhId = giaDinh.Id, GiaoDanId = gd.Id, VaiTro = VaiTroGiaDinh.Chong,
            });
            await db.SaveChangesAsync();
            id = gd.Id;
        }
        var client = app.CreateAuthClient();
        var truoc = await client.GetFromJsonAsync<System.Text.Json.JsonElement>($"/api/giao-dan/{id}");
        var rowVersion = truoc.GetProperty("rowVersion").GetUInt32();

        var res = await client.PutAsJsonAsync($"/api/giao-dan/{id}", new
        {
            HoTen = "Dang La Chong", Phai = "Nữ", NgaySinh = "1990-01-01", RowVersion = rowVersion,
        });

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await res.Content.ReadFromJsonAsync<ThongBaoLoi>())!.ThongBao
            .Should().Contain("đã được nhập là vợ/chồng trong một gia đình hoặc hôn phối");
    }

    // --- Xoá ------------------------------------------------------------------------------

    [Fact]
    public async Task Xoa_mem_dat_DaXoa_true_va_bien_mat_khoi_danh_sach()
    {
        Guid id;
        await using (var db = app.TaoContextThuan())
        {
            var gd = new GiaoDan { GiaoXuId = app.GiaoXuId, MaGiaoDanCu = 92000, HoTen = "Se Bi Xoa Mem", Phai = "Nam" };
            db.GiaoDan.Add(gd);
            await db.SaveChangesAsync();
            id = gd.Id;
        }
        var client = app.CreateAuthClient();

        var res = await client.DeleteAsync($"/api/giao-dan/{id}");

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        await using var db2 = app.TaoContextThuan();
        (await db2.GiaoDan.SingleAsync(x => x.Id == id)).DaXoa.Should().BeTrue();
        var ds = await client.GetFromJsonAsync<List<Item>>("/api/giao-dan?hienCaDaMat=true");
        ds!.Should().NotContain(x => x.MaGiaoDanCu == 92000);
    }

    [Fact]
    public async Task Xoa_vinh_vien_xoa_that_khoi_bang()
    {
        Guid id;
        await using (var db = app.TaoContextThuan())
        {
            var gd = new GiaoDan { GiaoXuId = app.GiaoXuId, MaGiaoDanCu = 93000, HoTen = "Se Bi Xoa Vinh Vien", Phai = "Nam" };
            db.GiaoDan.Add(gd);
            await db.SaveChangesAsync();
            id = gd.Id;
        }
        var client = app.CreateAuthClient();

        var res = await client.DeleteAsync($"/api/giao-dan/{id}?vinhVien=true");

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        await using var db2 = app.TaoContextThuan();
        (await db2.GiaoDan.AnyAsync(x => x.Id == id)).Should().BeFalse();
    }

    [Fact]
    public async Task Xoa_vinh_vien_bi_chan_khi_dang_thuoc_gia_dinh()
    {
        Guid id;
        await using (var db = app.TaoContextThuan())
        {
            var gd = new GiaoDan { GiaoXuId = app.GiaoXuId, MaGiaoDanCu = 94000, HoTen = "Dang O Trong Gia Dinh", Phai = "Nam" };
            db.GiaoDan.Add(gd);
            var giaDinh = new GiaDinh { GiaoXuId = app.GiaoXuId, MaGiaDinhCu = 94000, TenGiaDinh = "GD chan xoa" };
            db.GiaDinh.Add(giaDinh);
            db.ThanhVienGiaDinh.Add(new ThanhVienGiaDinh
            {
                GiaoXuId = app.GiaoXuId, GiaDinhId = giaDinh.Id, GiaoDanId = gd.Id, VaiTro = VaiTroGiaDinh.Chong,
            });
            await db.SaveChangesAsync();
            id = gd.Id;
        }
        var client = app.CreateAuthClient();

        var res = await client.DeleteAsync($"/api/giao-dan/{id}?vinhVien=true");

        res.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var loi = await res.Content.ReadFromJsonAsync<ThongBaoLoi>();
        loi!.ThongBao.Should().Contain("Vui lòng xóa giáo dân ra khỏi gia đình trước khi xóa giáo dân này");
        loi.ThongBao.Should().Contain("GD chan xoa");

        await using var db2 = app.TaoContextThuan();
        (await db2.GiaoDan.AnyAsync(x => x.Id == id && !x.DaXoa)).Should().BeTrue("khong duoc xoa gi ca khi bi chan");
    }

    [Fact]
    public async Task Xoa_giao_dan_khong_ton_tai_tra_ve_404()
    {
        var res = await app.CreateAuthClient().DeleteAsync($"/api/giao-dan/{Guid.NewGuid()}");
        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // --- Danh sách mặc định ẩn qua đời/chuyển xứ ------------------------------------------

    [Fact]
    public async Task Danh_sach_mac_dinh_an_nguoi_qua_doi_va_hien_lai_khi_hienCaDaMat()
    {
        await using (var db = app.TaoContextThuan())
        {
            db.GiaoDan.Add(new GiaoDan { GiaoXuId = app.GiaoXuId, MaGiaoDanCu = 95000, HoTen = "Da Qua Doi", Phai = "Nam", QuaDoi = true });
            await db.SaveChangesAsync();
        }
        var client = app.CreateAuthClient();

        var macDinh = await client.GetFromJsonAsync<List<Item>>("/api/giao-dan");
        var hienCa = await client.GetFromJsonAsync<List<Item>>("/api/giao-dan?hienCaDaMat=true");

        macDinh!.Should().NotContain(x => x.MaGiaoDanCu == 95000);
        hienCa!.Should().ContainSingle(x => x.MaGiaoDanCu == 95000);
    }

    [Fact]
    public async Task Danh_sach_mac_dinh_an_nguoi_thuoc_gia_dinh_da_chuyen_xu()
    {
        await using (var db = app.TaoContextThuan())
        {
            var gd = new GiaoDan { GiaoXuId = app.GiaoXuId, MaGiaoDanCu = 96000, HoTen = "Da Chuyen Xu", Phai = "Nam" };
            db.GiaoDan.Add(gd);
            var giaDinh = new GiaDinh
            {
                GiaoXuId = app.GiaoXuId, MaGiaDinhCu = 96000, TenGiaDinh = "GD da chuyen xu", DaChuyenXu = true,
            };
            db.GiaDinh.Add(giaDinh);
            db.ThanhVienGiaDinh.Add(new ThanhVienGiaDinh
            {
                GiaoXuId = app.GiaoXuId, GiaDinhId = giaDinh.Id, GiaoDanId = gd.Id, VaiTro = VaiTroGiaDinh.Chong,
            });
            await db.SaveChangesAsync();
        }
        var client = app.CreateAuthClient();

        var macDinh = await client.GetFromJsonAsync<List<Item>>("/api/giao-dan");
        var hienCa = await client.GetFromJsonAsync<List<Item>>("/api/giao-dan?hienCaDaMat=true");

        macDinh!.Should().NotContain(x => x.MaGiaoDanCu == 96000);
        hienCa!.Should().ContainSingle(x => x.MaGiaoDanCu == 96000);
    }
}
