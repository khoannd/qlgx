using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Qlgx.Api.Dtos;
using Qlgx.Domain;
using Qlgx.Domain.Entities;

namespace Qlgx.Api.Tests;

/// <summary>
/// "Tạo danh sách bí tích tự động" (docs/superpowers/specs/man-hinh/cong-cu-du-lieu.md mục 5.2)
/// — công cụ SINH dữ liệu hàng loạt trên khối lớn nhất CSDL. Test đúng 4 nguyên tắc: xem trước
/// có số liệu thật, ghi thật gộp đúng nhóm (linh mục + ngày, không xét nơi), không tạo trùng
/// chi tiết đã có sẵn, và chỉ hỗ trợ 3 loại bí tích.
/// </summary>
public class TaoDotBiTichTuDongTests(QlgxApiFactory app) : IClassFixture<QlgxApiFactory>
{
    [Fact]
    public async Task Xem_truoc_dem_dung_so_dot_moi_va_so_giao_dan_khong_ghi_gi()
    {
        await using var db = app.TaoContextThuan();
        db.GiaoDan.AddRange(
            new GiaoDan { GiaoXuId = app.GiaoXuId, MaGiaoDanCu = 95001, HoTen = "GD1", NgayRuaToi = new DateOnly(2020, 1, 5), ChaRuaToi = "Cha A", NoiRuaToi = "Nha tho X" },
            new GiaoDan { GiaoXuId = app.GiaoXuId, MaGiaoDanCu = 95002, HoTen = "GD2", NgayRuaToi = new DateOnly(2020, 1, 5), ChaRuaToi = "cha a", NoiRuaToi = "Nha tho X" });
        await db.SaveChangesAsync();

        var client = app.CreateAuthClient();
        var res = await client.PostAsJsonAsync("/api/cong-cu-du-lieu/tao-dot-bi-tich/xem-truoc",
            new TaoDotBiTichTuDongRequest(LoaiBiTich.RuaToi, null, null, new DateOnly(2020, 1, 1), new DateOnly(2020, 1, 31)));
        res.EnsureSuccessStatusCode();
        var kq = await res.Content.ReadFromJsonAsync<TaoDotBiTichXemTruocKetQua>();

        kq!.TongGiaoDanKhopDieuKien.Should().Be(2);
        kq.SoDotMoiSeTao.Should().Be(1, "hai giao dan cung linh muc (khong phan biet hoa/thuong) + cung ngay phai gop 1 dot");
        kq.SoGiaoDanMoiSeThem.Should().Be(2);

        await using var kiemTra = app.TaoContextThuan();
        (await kiemTra.DotBiTich.CountAsync(d => d.LinhMuc == "Cha A")).Should().Be(0, "xem truoc khong duoc ghi gi");
    }

    [Fact]
    public async Task Ghi_that_tao_dung_1_dot_gop_2_giao_dan_cung_linh_muc_ngay()
    {
        await using var db = app.TaoContextThuan();
        var gd1 = new GiaoDan { GiaoXuId = app.GiaoXuId, MaGiaoDanCu = 95010, HoTen = "GD1", NgayRuaToi = new DateOnly(2021, 3, 10), ChaRuaToi = "Cha B", NoiRuaToi = "Noi A" };
        var gd2 = new GiaoDan { GiaoXuId = app.GiaoXuId, MaGiaoDanCu = 95011, HoTen = "GD2", NgayRuaToi = new DateOnly(2021, 3, 10), ChaRuaToi = "Cha B", NoiRuaToi = "Noi B" };
        db.GiaoDan.AddRange(gd1, gd2);
        await db.SaveChangesAsync();

        var client = app.CreateAuthClient();
        var res = await client.PostAsJsonAsync("/api/cong-cu-du-lieu/tao-dot-bi-tich",
            new TaoDotBiTichTuDongRequest(LoaiBiTich.RuaToi, null, null, new DateOnly(2021, 3, 1), new DateOnly(2021, 3, 31)));
        res.EnsureSuccessStatusCode();
        var kq = await res.Content.ReadFromJsonAsync<TaoDotBiTichTuDongKetQua>();
        kq!.SoDotDaTao.Should().Be(1);
        kq.SoGiaoDanDaThem.Should().Be(2);

        await using var kiemTra = app.TaoContextThuan();
        var dot = await kiemTra.DotBiTich.Include(d => d.ChiTiet).SingleAsync(d => d.LinhMuc == "Cha B");
        dot.ChiTiet.Select(c => c.GiaoDanId).Should().BeEquivalentTo([gd1.Id, gd2.Id]);
        dot.NoiBiTich.Should().Be("Noi A", "khong xet Noi khi gop nhom - dot moi mang Noi cua giao dan dau tien");
    }

    [Fact]
    public async Task Chay_lai_lan_2_khong_tao_trung_dot_va_khong_tao_trung_chi_tiet()
    {
        await using var db = app.TaoContextThuan();
        var gd = new GiaoDan { GiaoXuId = app.GiaoXuId, MaGiaoDanCu = 95020, HoTen = "GD", NgayRuaToi = new DateOnly(2021, 5, 1), ChaRuaToi = "Cha C" };
        db.GiaoDan.Add(gd);
        await db.SaveChangesAsync();

        var client = app.CreateAuthClient();
        var yc = new TaoDotBiTichTuDongRequest(LoaiBiTich.RuaToi, null, null, new DateOnly(2021, 5, 1), new DateOnly(2021, 5, 31));
        (await client.PostAsJsonAsync("/api/cong-cu-du-lieu/tao-dot-bi-tich", yc)).EnsureSuccessStatusCode();

        var lan2 = await client.PostAsJsonAsync("/api/cong-cu-du-lieu/tao-dot-bi-tich", yc);
        var kq2 = await lan2.Content.ReadFromJsonAsync<TaoDotBiTichTuDongKetQua>();
        kq2!.SoDotDaTao.Should().Be(0);
        kq2.SoGiaoDanDaThem.Should().Be(0);

        await using var kiemTra = app.TaoContextThuan();
        (await kiemTra.DotBiTich.CountAsync(d => d.LinhMuc == "Cha C")).Should().Be(1);
        (await kiemTra.BiTichChiTiet.CountAsync(c => c.GiaoDanId == gd.Id)).Should().Be(1);
    }

    [Fact]
    public async Task Loc_theo_noi_bi_tich_chi_lay_dung_giao_dan_khop()
    {
        await using var db = app.TaoContextThuan();
        var khop = new GiaoDan { GiaoXuId = app.GiaoXuId, MaGiaoDanCu = 95030, HoTen = "Khop", NgayRuaToi = new DateOnly(2021, 6, 1), NoiRuaToi = "Nha Tho Chinh" };
        var khongKhop = new GiaoDan { GiaoXuId = app.GiaoXuId, MaGiaoDanCu = 95031, HoTen = "Khong khop", NgayRuaToi = new DateOnly(2021, 6, 1), NoiRuaToi = "Noi khac" };
        db.GiaoDan.AddRange(khop, khongKhop);
        await db.SaveChangesAsync();

        var client = app.CreateAuthClient();
        var res = await client.PostAsJsonAsync("/api/cong-cu-du-lieu/tao-dot-bi-tich/xem-truoc",
            new TaoDotBiTichTuDongRequest(LoaiBiTich.RuaToi, null, "nha tho chinh", new DateOnly(2021, 6, 1), new DateOnly(2021, 6, 30)));
        var kq = await res.Content.ReadFromJsonAsync<TaoDotBiTichXemTruocKetQua>();

        kq!.TongGiaoDanKhopDieuKien.Should().Be(1);
    }

    [Fact]
    public async Task Loai_bi_tich_khong_ho_tro_bi_tu_choi()
    {
        var client = app.CreateAuthClient();
        var res = await client.PostAsJsonAsync("/api/cong-cu-du-lieu/tao-dot-bi-tich/xem-truoc",
            new TaoDotBiTichTuDongRequest(LoaiBiTich.HonPhoi, null, null, new DateOnly(2021, 1, 1), new DateOnly(2021, 1, 31)));

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Tu_ngay_lon_hon_den_ngay_bi_tu_choi()
    {
        var client = app.CreateAuthClient();
        var res = await client.PostAsJsonAsync("/api/cong-cu-du-lieu/tao-dot-bi-tich/xem-truoc",
            new TaoDotBiTichTuDongRequest(LoaiBiTich.RuaToi, null, null, new DateOnly(2021, 2, 1), new DateOnly(2021, 1, 1)));

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// Lưới an toàn CSDL cuối cho race condition (review-toan-nhanh-dulieu.md mục C1): kể cả
    /// khi hai đợt được tạo qua hai <c>QlgxDbContext</c> KHÁC NHAU (không đi qua
    /// TaoTuDong/TinhToan — mô phỏng đúng tình huống 2 transaction song song, mỗi bên không
    /// thấy commit của bên kia), ràng buộc UNIQUE cấp CSDL vẫn phải chặn — không được chỉ dựa
    /// vào SELECT-rồi-INSERT ở tầng ứng dụng. Cũng chứng minh chỉ mục BIỂU THỨC bắt đúng cả
    /// khác biệt hoa/thường và khoảng trắng đầu/cuối của LinhMuc (đúng khoá nhóm mà
    /// TaoDotBiTichTuDongService.TinhToan dùng).
    /// </summary>
    [Fact]
    public async Task Rang_buoc_CSDL_chan_hai_dot_trung_nhom_du_tao_qua_hai_context_rieng()
    {
        var ngay = new DateOnly(2023, 7, 1);
        await using (var ctx1 = app.TaoContextThuan())
        {
            ctx1.DotBiTich.Add(new DotBiTich
            {
                GiaoXuId = app.GiaoXuId, LoaiBiTich = LoaiBiTich.RuaToi, NgayBiTich = ngay,
                LinhMuc = "Cha Rang Buoc", MaDotBiTichCu = 900001,
            });
            await ctx1.SaveChangesAsync();
        }

        await using var ctx2 = app.TaoContextThuan();
        ctx2.DotBiTich.Add(new DotBiTich
        {
            // Khác hoa/thường + có khoảng trắng đầu/cuối - vẫn phải bị coi là CÙNG nhóm.
            GiaoXuId = app.GiaoXuId, LoaiBiTich = LoaiBiTich.RuaToi, NgayBiTich = ngay,
            LinhMuc = "  cha rang buoc  ", MaDotBiTichCu = 900002,
        });

        var hanhDong = async () => await ctx2.SaveChangesAsync();

        (await hanhDong.Should().ThrowAsync<DbUpdateException>())
            .Which.InnerException.Should().BeOfType<PostgresException>()
            .Which.SqlState.Should().Be(PostgresErrorCodes.UniqueViolation);
    }

    /// <summary>
    /// Kịch bản thật của mục C1: người dùng bấm "Xác nhận tạo" nhiều lần liên tiếp (tưởng bị
    /// treo) khiến nhiều request chạy chồng lấn. Dù có request nào "thua cuộc đua" hay không
    /// (phụ thuộc lịch chạy của hệ điều hành/DB, không đảm bảo lúc nào cũng xảy ra), khẳng định
    /// PHẢI đúng luôn: CSDL không bao giờ có 2 đợt trùng nhóm, và request thua cuộc (nếu có)
    /// nhận đúng 409 kèm thông báo tiếng Việt — không phải 500 thô lộ chi tiết ngoại lệ .NET.
    /// </summary>
    [Fact]
    public async Task Nhieu_yeu_cau_dong_thoi_khong_bao_gio_tao_trung_dot_va_khong_lo_loi_500_tho()
    {
        await using var db = app.TaoContextThuan();
        db.GiaoDan.AddRange(
            new GiaoDan { GiaoXuId = app.GiaoXuId, MaGiaoDanCu = 95040, HoTen = "GD1", NgayRuaToi = new DateOnly(2022, 4, 10), ChaRuaToi = "Cha Dong Thoi" },
            new GiaoDan { GiaoXuId = app.GiaoXuId, MaGiaoDanCu = 95041, HoTen = "GD2", NgayRuaToi = new DateOnly(2022, 4, 10), ChaRuaToi = "Cha Dong Thoi" });
        await db.SaveChangesAsync();

        var yc = new TaoDotBiTichTuDongRequest(LoaiBiTich.RuaToi, null, null, new DateOnly(2022, 4, 1), new DateOnly(2022, 4, 30));

        const int soLuong = 8;
        var tasks = Enumerable.Range(0, soLuong)
            .Select(_ => app.CreateAuthClient().PostAsJsonAsync("/api/cong-cu-du-lieu/tao-dot-bi-tich", yc))
            .ToArray();
        var responses = await Task.WhenAll(tasks);

        foreach (var res in responses)
            res.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Conflict);

        foreach (var res in responses.Where(r => r.StatusCode == HttpStatusCode.Conflict))
        {
            var noiDung = await res.Content.ReadAsStringAsync();
            noiDung.Should().Contain("thongBao");
            noiDung.Should().NotContain("PostgresException").And.NotContain("23505")
                .And.NotContain("Exception");
        }

        await using var kiemTra = app.TaoContextThuan();
        (await kiemTra.DotBiTich.CountAsync(d => d.LinhMuc == "Cha Dong Thoi")).Should().Be(1,
            "khong duoc phep co 2 dot trung linh muc/ngay du bao nhieu request chay dong thoi");
        (await kiemTra.BiTichChiTiet.CountAsync(c => c.DotBiTich!.LinhMuc == "Cha Dong Thoi"))
            .Should().Be(2, "ca 2 giao dan khop dieu kien deu phai duoc them, khong thieu khong trung");
    }

    [Fact]
    public async Task Nguoi_chua_dang_nhap_bi_chan_401()
    {
        var res = await app.CreateClient().PostAsJsonAsync("/api/cong-cu-du-lieu/tao-dot-bi-tich/xem-truoc",
            new TaoDotBiTichTuDongRequest(LoaiBiTich.RuaToi, null, null, new DateOnly(2021, 1, 1), new DateOnly(2021, 1, 31)));

        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
