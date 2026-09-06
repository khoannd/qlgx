using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Qlgx.Domain.Entities;

namespace Qlgx.Api.Tests;

/// <summary>
/// Tab "Hội đoàn" trong màn hình chi tiết giáo dân. Xem
/// docs/superpowers/specs/man-hinh/hoi-doan.md — bản desktop (GxHistoryHoiDoan) chỉ cho xem lịch
/// sử và thêm mới (toàn bộ cột lưới NoEdit, nút Sửa/Xoá luôn ẩn); bản web mở rộng có chủ đích
/// cho sửa Ngày vào/Ngày ra/Vai trò của một lượt tham gia đã có.
/// </summary>
public class GiaoDanHoiDoanTests(QlgxApiFactory app) : IClassFixture<QlgxApiFactory>
{
    private sealed record HoiDoanDanhMuc(Guid Id, string TenHoiDoan);

    private sealed record HoiDoanCuaGiaoDan(Guid Id, Guid HoiDoanId, string? TenHoiDoan,
        DateOnly? NgayVaoHoiDoan, DateOnly? NgayRaHoiDoan, string? VaiTro, uint RowVersion);

    private sealed record ThemHoiDoan(Guid HoiDoanId, DateOnly? NgayVaoHoiDoan, DateOnly? NgayRaHoiDoan);

    private sealed record CapNhatHoiDoan(DateOnly? NgayVaoHoiDoan, DateOnly? NgayRaHoiDoan,
        string? VaiTro, uint RowVersion);

    private sealed record ThongBaoLoi(string ThongBao);

    private async Task<Guid> TaoGiaoDan(int ma, string hoTen)
    {
        await using var db = app.TaoContextThuan();
        var gd = new GiaoDan { GiaoXuId = app.GiaoXuId, MaGiaoDanCu = ma, HoTen = hoTen, Phai = "Nam" };
        db.GiaoDan.Add(gd);
        await db.SaveChangesAsync();
        return gd.Id;
    }

    private async Task<Guid> TaoHoiDoan(int ma, string ten)
    {
        await using var db = app.TaoContextThuan();
        var hd = new HoiDoan { GiaoXuId = app.GiaoXuId, MaHoiDoanCu = ma, TenHoiDoan = ten };
        db.HoiDoan.Add(hd);
        await db.SaveChangesAsync();
        return hd.Id;
    }

    [Fact]
    public async Task Danh_muc_hoi_doan_tra_dung_ten_sap_theo_bang_chu_cai()
    {
        // KHÔNG kiểm tra tổng số dòng: QlgxApiFactory dùng chung MỘT database cho mọi [Fact]
        // trong lớp test này (IClassFixture không reset giữa các test, giống cách
        // GiaoDanHonPhoiTests dùng MaGiaoDanCu riêng cho từng test để tránh đụng nhau) — các
        // test khác trong lớp cũng tạo HoiDoan riêng của chúng. Chỉ kiểm tra thứ tự tương đối
        // giữa hai bản ghi vừa tạo ở đây, tìm đúng bằng Id (không phải chỉ số cố định).
        var idLegio = await TaoHoiDoan(9200, "Legio Mariae Rieng Cho Test Nay");
        var idGiaTruong = await TaoHoiDoan(9201, "Gia truong Rieng Cho Test Nay");

        var ds = await app.CreateAuthClient().GetFromJsonAsync<List<HoiDoanDanhMuc>>("/api/hoi-doan");

        ds.Should().NotBeNull();
        var viTriGiaTruong = ds!.FindIndex(x => x.Id == idGiaTruong);
        var viTriLegio = ds.FindIndex(x => x.Id == idLegio);
        viTriGiaTruong.Should().BeGreaterThanOrEqualTo(0);
        viTriLegio.Should().BeGreaterThanOrEqualTo(0);
        viTriGiaTruong.Should().BeLessThan(viTriLegio, "sap theo TenHoiDoan tang dan (G truoc L)");
    }

    [Fact]
    public async Task Giao_dan_chua_tham_gia_hoi_doan_nao_thi_tra_danh_sach_rong()
    {
        var id = await TaoGiaoDan(9210, "Chua tham gia");

        var ds = await app.CreateAuthClient()
            .GetFromJsonAsync<List<HoiDoanCuaGiaoDan>>($"/api/giao-dan/{id}/hoi-doan");

        ds.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public async Task Them_luot_tham_gia_moi_thi_vai_tro_mac_dinh_la_hoi_vien()
    {
        var id = await TaoGiaoDan(9220, "Hoi vien moi");
        var hoiDoanId = await TaoHoiDoan(9220, "Legio Mariae");
        var client = app.CreateAuthClient();

        var res = await client.PostAsJsonAsync($"/api/giao-dan/{id}/hoi-doan",
            new ThemHoiDoan(hoiDoanId, new DateOnly(2020, 1, 1), null));

        var than = await res.Content.ReadAsStringAsync();
        res.StatusCode.Should().Be(HttpStatusCode.Created, than);
        var ds = await client.GetFromJsonAsync<List<HoiDoanCuaGiaoDan>>($"/api/giao-dan/{id}/hoi-doan");
        ds.Should().ContainSingle();
        var hd = ds![0];
        hd.HoiDoanId.Should().Be(hoiDoanId);
        hd.TenHoiDoan.Should().Be("Legio Mariae");
        hd.NgayVaoHoiDoan.Should().Be(new DateOnly(2020, 1, 1));
        hd.NgayRaHoiDoan.Should().BeNull();
        hd.VaiTro.Should().Be("Hội viên", "GxHistoryHoiDoan.UpdateHoiDoan hard-code Vai tro nay khi them moi");
        hd.RowVersion.Should().BeGreaterThan(0u);
    }

    [Fact]
    public async Task Them_hoi_doan_khong_ton_tai_thi_tra_404()
    {
        var id = await TaoGiaoDan(9230, "Chon hoi doan sai");

        var res = await app.CreateAuthClient().PostAsJsonAsync($"/api/giao-dan/{id}/hoi-doan",
            new ThemHoiDoan(Guid.NewGuid(), null, null));

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Them_hoi_doan_cho_giao_dan_khong_ton_tai_thi_tra_404()
    {
        var hoiDoanId = await TaoHoiDoan(9240, "Hien mau");

        var res = await app.CreateAuthClient().PostAsJsonAsync($"/api/giao-dan/{Guid.NewGuid()}/hoi-doan",
            new ThemHoiDoan(hoiDoanId, null, null));

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Nguoi_tham_gia_nhieu_hoi_doan_thi_tra_ca_hai_sap_ngay_vao_moi_nhat_truoc()
    {
        var id = await TaoGiaoDan(9250, "Nhieu hoi doan");
        var hd1 = await TaoHoiDoan(9250, "Legio Mariae");
        var hd2 = await TaoHoiDoan(9251, "Gia trưởng");
        var client = app.CreateAuthClient();
        var r1 = await client.PostAsJsonAsync($"/api/giao-dan/{id}/hoi-doan", new ThemHoiDoan(hd1, new DateOnly(2010, 1, 1), null));
        var r2 = await client.PostAsJsonAsync($"/api/giao-dan/{id}/hoi-doan", new ThemHoiDoan(hd2, new DateOnly(2022, 1, 1), null));
        r1.StatusCode.Should().Be(HttpStatusCode.Created, await r1.Content.ReadAsStringAsync());
        r2.StatusCode.Should().Be(HttpStatusCode.Created, await r2.Content.ReadAsStringAsync());

        var ds = await client.GetFromJsonAsync<List<HoiDoanCuaGiaoDan>>($"/api/giao-dan/{id}/hoi-doan");

        ds.Should().HaveCount(2);
        ds![0].TenHoiDoan.Should().Be("Gia trưởng", "sap xep ngay vao moi nhat truoc");
        ds[1].TenHoiDoan.Should().Be("Legio Mariae");
    }

    [Fact]
    public async Task Sua_ngay_ra_va_vai_tro_thanh_cong_khi_dung_phien_ban()
    {
        var id = await TaoGiaoDan(9260, "Sua duoc");
        var hoiDoanId = await TaoHoiDoan(9260, "Legio Mariae");
        var client = app.CreateAuthClient();
        await client.PostAsJsonAsync($"/api/giao-dan/{id}/hoi-doan",
            new ThemHoiDoan(hoiDoanId, new DateOnly(2015, 1, 1), null));
        var truoc = (await client.GetFromJsonAsync<List<HoiDoanCuaGiaoDan>>($"/api/giao-dan/{id}/hoi-doan"))!.Single();

        var res = await client.PutAsJsonAsync($"/api/giao-dan/hoi-doan/{truoc.Id}",
            new CapNhatHoiDoan(new DateOnly(2015, 1, 1), new DateOnly(2023, 12, 31), "Trưởng hội đoàn", truoc.RowVersion));

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        var sau = (await client.GetFromJsonAsync<List<HoiDoanCuaGiaoDan>>($"/api/giao-dan/{id}/hoi-doan"))!.Single();
        sau.NgayRaHoiDoan.Should().Be(new DateOnly(2023, 12, 31));
        sau.VaiTro.Should().Be("Trưởng hội đoàn");
        sau.RowVersion.Should().BeGreaterThan(truoc.RowVersion);
    }

    [Fact]
    public async Task Hai_nguoi_cung_sua_thi_nguoi_sau_nhan_409_voi_thong_bao_nhac_hoi_doan()
    {
        var id = await TaoGiaoDan(9270, "Xung dot");
        var hoiDoanId = await TaoHoiDoan(9270, "Legio Mariae");
        var client = app.CreateAuthClient();
        await client.PostAsJsonAsync($"/api/giao-dan/{id}/hoi-doan", new ThemHoiDoan(hoiDoanId, null, null));
        var banA = (await client.GetFromJsonAsync<List<HoiDoanCuaGiaoDan>>($"/api/giao-dan/{id}/hoi-doan"))!.Single();
        var banB = (await client.GetFromJsonAsync<List<HoiDoanCuaGiaoDan>>($"/api/giao-dan/{id}/hoi-doan"))!.Single();

        var luuA = await client.PutAsJsonAsync($"/api/giao-dan/hoi-doan/{banA.Id}",
            new CapNhatHoiDoan(null, null, "A sua", banA.RowVersion));
        var luuB = await client.PutAsJsonAsync($"/api/giao-dan/hoi-doan/{banB.Id}",
            new CapNhatHoiDoan(null, null, "B sua", banB.RowVersion));

        luuA.StatusCode.Should().Be(HttpStatusCode.OK);
        luuB.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var loi = await luuB.Content.ReadFromJsonAsync<ThongBaoLoi>();
        loi!.ThongBao.Should().Contain("hội đoàn");
    }

    [Fact]
    public async Task Sua_hoi_doan_khong_ton_tai_thi_tra_404()
    {
        var res = await app.CreateAuthClient().PutAsJsonAsync($"/api/giao-dan/hoi-doan/{Guid.NewGuid()}",
            new CapNhatHoiDoan(null, null, null, 0));

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
