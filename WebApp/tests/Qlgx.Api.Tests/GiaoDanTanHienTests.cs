using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Qlgx.Domain.Entities;

namespace Qlgx.Api.Tests;

/// <summary>
/// Tab "Ơn gọi tận hiến" trong màn hình chi tiết giáo dân. Xem
/// docs/superpowers/specs/man-hinh/tan-hien.md — bản desktop (GxTanHien) chỉ hỗ trợ một bản ghi
/// mỗi giáo dân (lấy Rows[0] không ORDER BY); bản web mở rộng có chủ đích thành danh sách đầy đủ
/// CRUD, cùng cách tiếp cận đã dùng cho Hôn phối (Task 15).
/// </summary>
public class GiaoDanTanHienTests(QlgxApiFactory app) : IClassFixture<QlgxApiFactory>
{
    private sealed record TanHienCuaGiaoDan(Guid Id, DateOnly? NgayBatDau, string? ChucVu,
        string? NoiTu, string? DongTu, string? NoiPhucVu, string? DiaChiPhucVu,
        string? DienThoaiPhucVu, string? EmailPhucVu, string? GhiChu, bool DaHoiTuc,
        DateOnly? NgayVaoDCV, DateOnly? NgayVaoNhaThu, DateOnly? NgayVaoNhaTap,
        DateOnly? NgayVaoKhanLanDau, DateOnly? NgayVaoKhanTronDoi,
        DateOnly? NgayPhoTe, DateOnly? NgayThuPhongLM, DateOnly? NgayBonMang, uint RowVersion);

    private sealed record LuuTanHien(DateOnly? NgayBatDau, string? ChucVu,
        string? NoiTu, string? DongTu, string? NoiPhucVu, string? DiaChiPhucVu,
        string? DienThoaiPhucVu, string? EmailPhucVu, string? GhiChu, bool DaHoiTuc,
        DateOnly? NgayVaoDCV, DateOnly? NgayVaoNhaThu, DateOnly? NgayVaoNhaTap,
        DateOnly? NgayVaoKhanLanDau, DateOnly? NgayVaoKhanTronDoi,
        DateOnly? NgayPhoTe, DateOnly? NgayThuPhongLM, DateOnly? NgayBonMang, uint RowVersion);

    private sealed record ThongBaoLoi(string ThongBao);

    private async Task<Guid> TaoGiaoDan(int ma, string hoTen)
    {
        await using var db = app.TaoContextThuan();
        var gd = new GiaoDan { GiaoXuId = app.GiaoXuId, MaGiaoDanCu = ma, HoTen = hoTen, Phai = "Nam" };
        db.GiaoDan.Add(gd);
        await db.SaveChangesAsync();
        return gd.Id;
    }

    private static readonly LuuTanHien Rong = new(
        null, null, null, null, null, null, null, null, null, false,
        null, null, null, null, null, null, null, null, 0);

    [Fact]
    public async Task Giao_dan_chua_co_tan_hien_thi_tra_danh_sach_rong()
    {
        var id = await TaoGiaoDan(9101, "Chua tan hien");

        var ds = await app.CreateClient()
            .GetFromJsonAsync<List<TanHienCuaGiaoDan>>($"/api/giao-dan/{id}/tan-hien");

        ds.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public async Task Them_moi_tan_hien_thanh_cong_va_doc_lai_du_truong()
    {
        var id = await TaoGiaoDan(9110, "Thay Giuse");
        var client = app.CreateClient();

        var res = await client.PostAsJsonAsync($"/api/giao-dan/{id}/tan-hien", Rong with
        {
            NgayBatDau = new DateOnly(2010, 9, 1), ChucVu = "Chủng sinh", NoiTu = "Dòng Tên",
            DongTu = "Dòng Tên Việt Nam", NoiPhucVu = "Giáo xứ Thánh Tâm",
            DiaChiPhucVu = "123 Nguyen Trai", DienThoaiPhucVu = "0900000000",
            EmailPhucVu = "thay@doàngten.vn", GhiChu = "Dang hoc triet", DaHoiTuc = false,
            NgayVaoDCV = new DateOnly(2015, 9, 1), NgayVaoNhaThu = new DateOnly(2010, 9, 1),
            NgayVaoNhaTap = new DateOnly(2011, 9, 1), NgayVaoKhanLanDau = new DateOnly(2012, 9, 1),
            NgayVaoKhanTronDoi = new DateOnly(2020, 9, 1), NgayPhoTe = new DateOnly(2021, 6, 1),
            NgayThuPhongLM = new DateOnly(2022, 6, 1), NgayBonMang = new DateOnly(2023, 3, 19),
        });

        res.StatusCode.Should().Be(HttpStatusCode.Created);
        var ds = await client.GetFromJsonAsync<List<TanHienCuaGiaoDan>>($"/api/giao-dan/{id}/tan-hien");
        ds.Should().ContainSingle();
        var t = ds![0];
        t.ChucVu.Should().Be("Chủng sinh");
        t.NoiTu.Should().Be("Dòng Tên");
        t.DongTu.Should().Be("Dòng Tên Việt Nam");
        t.NoiPhucVu.Should().Be("Giáo xứ Thánh Tâm");
        t.DiaChiPhucVu.Should().Be("123 Nguyen Trai");
        t.DienThoaiPhucVu.Should().Be("0900000000");
        t.EmailPhucVu.Should().Be("thay@doàngten.vn");
        t.GhiChu.Should().Be("Dang hoc triet");
        t.DaHoiTuc.Should().BeFalse();
        t.NgayBatDau.Should().Be(new DateOnly(2010, 9, 1));
        t.NgayVaoDCV.Should().Be(new DateOnly(2015, 9, 1));
        t.NgayVaoNhaThu.Should().Be(new DateOnly(2010, 9, 1));
        t.NgayVaoNhaTap.Should().Be(new DateOnly(2011, 9, 1));
        t.NgayVaoKhanLanDau.Should().Be(new DateOnly(2012, 9, 1));
        t.NgayVaoKhanTronDoi.Should().Be(new DateOnly(2020, 9, 1));
        t.NgayPhoTe.Should().Be(new DateOnly(2021, 6, 1));
        t.NgayThuPhongLM.Should().Be(new DateOnly(2022, 6, 1));
        t.NgayBonMang.Should().Be(new DateOnly(2023, 3, 19));
        t.RowVersion.Should().BeGreaterThan(0u);
    }

    [Fact]
    public async Task Them_tan_hien_cho_giao_dan_khong_ton_tai_thi_tra_404()
    {
        var res = await app.CreateClient()
            .PostAsJsonAsync($"/api/giao-dan/{Guid.NewGuid()}/tan-hien", Rong);

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Nguoi_co_nhieu_giai_doan_tan_hien_thi_tra_ca_hai_sap_moi_nhat_truoc()
    {
        var id = await TaoGiaoDan(9120, "Nhieu giai doan");
        var client = app.CreateClient();
        await client.PostAsJsonAsync($"/api/giao-dan/{id}/tan-hien",
            Rong with { NgayBatDau = new DateOnly(2000, 1, 1), ChucVu = "Tu sĩ" });
        await client.PostAsJsonAsync($"/api/giao-dan/{id}/tan-hien",
            Rong with { NgayBatDau = new DateOnly(2020, 1, 1), ChucVu = "Linh mục" });

        var ds = await client.GetFromJsonAsync<List<TanHienCuaGiaoDan>>($"/api/giao-dan/{id}/tan-hien");

        ds.Should().HaveCount(2);
        ds![0].ChucVu.Should().Be("Linh mục", "sap xep moi nhat truoc theo NgayBatDau");
        ds[1].ChucVu.Should().Be("Tu sĩ");
    }

    [Fact]
    public async Task Cap_nhat_thanh_cong_khi_dung_phien_ban()
    {
        var id = await TaoGiaoDan(9130, "Sua duoc");
        var client = app.CreateClient();
        await client.PostAsJsonAsync($"/api/giao-dan/{id}/tan-hien", Rong with { ChucVu = "Tu sĩ" });
        var truoc = (await client.GetFromJsonAsync<List<TanHienCuaGiaoDan>>($"/api/giao-dan/{id}/tan-hien"))!.Single();

        var res = await client.PutAsJsonAsync($"/api/giao-dan/tan-hien/{truoc.Id}",
            Rong with { ChucVu = "Linh mục", DaHoiTuc = true, RowVersion = truoc.RowVersion });

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        var sau = (await client.GetFromJsonAsync<List<TanHienCuaGiaoDan>>($"/api/giao-dan/{id}/tan-hien"))!.Single();
        sau.ChucVu.Should().Be("Linh mục");
        sau.DaHoiTuc.Should().BeTrue();
        sau.RowVersion.Should().BeGreaterThan(truoc.RowVersion);
    }

    [Fact]
    public async Task Hai_nguoi_cung_sua_thi_nguoi_sau_nhan_409_voi_thong_bao_nhac_tan_hien()
    {
        var id = await TaoGiaoDan(9140, "Xung dot");
        var client = app.CreateClient();
        await client.PostAsJsonAsync($"/api/giao-dan/{id}/tan-hien", Rong with { ChucVu = "Tu sĩ" });
        var banA = (await client.GetFromJsonAsync<List<TanHienCuaGiaoDan>>($"/api/giao-dan/{id}/tan-hien"))!.Single();
        var banB = (await client.GetFromJsonAsync<List<TanHienCuaGiaoDan>>($"/api/giao-dan/{id}/tan-hien"))!.Single();

        var luuA = await client.PutAsJsonAsync($"/api/giao-dan/tan-hien/{banA.Id}",
            Rong with { ChucVu = "A sua", RowVersion = banA.RowVersion });
        var luuB = await client.PutAsJsonAsync($"/api/giao-dan/tan-hien/{banB.Id}",
            Rong with { ChucVu = "B sua", RowVersion = banB.RowVersion });

        luuA.StatusCode.Should().Be(HttpStatusCode.OK);
        luuB.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var loi = await luuB.Content.ReadFromJsonAsync<ThongBaoLoi>();
        loi!.ThongBao.Should().Contain("tận hiến");
    }

    [Fact]
    public async Task Sua_tan_hien_khong_ton_tai_thi_tra_404()
    {
        var res = await app.CreateClient().PutAsJsonAsync($"/api/giao-dan/tan-hien/{Guid.NewGuid()}", Rong);

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
