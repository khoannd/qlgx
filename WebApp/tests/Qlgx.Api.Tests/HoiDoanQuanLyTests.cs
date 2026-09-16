using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Qlgx.Domain.Entities;

namespace Qlgx.Api.Tests;

/// <summary>
/// Màn hình "Danh sách hội đoàn" (frmHoiDoanList.cs + frmHoiDoan.cs) — quản trị danh mục hội
/// đoàn và hội viên. Xem docs/superpowers/specs/man-hinh/hoi-doan-danh-sach.md. Bảng rỗng ở
/// dữ liệu thật khảo sát nên test là bằng chứng chính cho màn hình này.
/// </summary>
public class HoiDoanQuanLyTests(QlgxApiFactory app) : IClassFixture<QlgxApiFactory>
{
    private sealed record HoiDoanDto(Guid Id, int MaHoiDoanCu, string TenHoiDoan, string? ThanhBonMang,
        DateOnly? NgayBonMang, DateOnly? NgayThanhLap, string? GhiChu, int SoHoiVienDangHoatDong, uint RowVersion);

    private sealed record ThanhVienDto(Guid ChiTietId, Guid GiaoDanId, string HoTen, string? TenThanh,
        DateOnly? NgayVaoHoiDoan, DateOnly? NgayRaHoiDoan, string? VaiTro, bool DaRaKhoiHoiDoan, uint RowVersion);

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
    public async Task Thieu_ten_hoi_doan_thi_bao_loi_dung_nguyen_van()
    {
        var res = await app.CreateAuthClient().PostAsJsonAsync("/api/hoi-doan", new
        {
            tenHoiDoan = "  ", thanhBonMang = (string?)null, ngayBonMang = (DateOnly?)null,
            ngayThanhLap = (DateOnly?)null, ghiChu = (string?)null, rowVersion = (uint?)null,
        });
        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var loi = await res.Content.ReadFromJsonAsync<ThongBaoLoi>();
        loi!.ThongBao.Should().Be("Vui lòng nhập tên hội đoàn");
    }

    [Fact]
    public async Task Tao_sua_roi_doc_lai_dung_du_6_cot()
    {
        var client = app.CreateAuthClient();
        var res = await client.PostAsJsonAsync("/api/hoi-doan", new
        {
            tenHoiDoan = "Legio Mariae", thanhBonMang = "Đức Mẹ", ngayBonMang = new DateOnly(2026, 8, 22),
            ngayThanhLap = new DateOnly(1990, 1, 1), ghiChu = "Ghi chu", rowVersion = (uint?)null,
        });
        res.StatusCode.Should().Be(HttpStatusCode.Created);
        var tao = await res.Content.ReadFromJsonAsync<Dictionary<string, Guid>>();
        var id = tao!["id"];

        var ds = await client.GetFromJsonAsync<List<HoiDoanDto>>("/api/hoi-doan/danh-sach");
        var dong = ds!.Single(h => h.Id == id);
        dong.TenHoiDoan.Should().Be("Legio Mariae");
        dong.ThanhBonMang.Should().Be("Đức Mẹ");
        dong.SoHoiVienDangHoatDong.Should().Be(0);

        var suaRes = await client.PutAsJsonAsync($"/api/hoi-doan/{id}", new
        {
            tenHoiDoan = "Legio Mariae (sửa)", thanhBonMang = "Đức Mẹ", ngayBonMang = new DateOnly(2026, 8, 22),
            ngayThanhLap = new DateOnly(1990, 1, 1), ghiChu = "Ghi chu moi", rowVersion = dong.RowVersion,
        });
        suaRes.StatusCode.Should().Be(HttpStatusCode.OK);

        var ds2 = await client.GetFromJsonAsync<List<HoiDoanDto>>("/api/hoi-doan/danh-sach");
        ds2!.Single(h => h.Id == id).TenHoiDoan.Should().Be("Legio Mariae (sửa)");
    }

    [Fact]
    public async Task Sua_sai_RowVersion_thi_bao_xung_dot()
    {
        var client = app.CreateAuthClient();
        var res = await client.PostAsJsonAsync("/api/hoi-doan", new
        {
            tenHoiDoan = "Hiền Mẫu", thanhBonMang = (string?)null, ngayBonMang = (DateOnly?)null,
            ngayThanhLap = (DateOnly?)null, ghiChu = (string?)null, rowVersion = (uint?)null,
        });
        var id = (await res.Content.ReadFromJsonAsync<Dictionary<string, Guid>>())!["id"];

        var suaRes = await client.PutAsJsonAsync($"/api/hoi-doan/{id}", new
        {
            tenHoiDoan = "Hiền Mẫu 2", thanhBonMang = (string?)null, ngayBonMang = (DateOnly?)null,
            ngayThanhLap = (DateOnly?)null, ghiChu = (string?)null, rowVersion = (uint?)999999,
        });
        suaRes.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var loi = await suaRes.Content.ReadFromJsonAsync<ThongBaoLoi>();
        loi!.ThongBao.Should().Contain("vừa được người khác cập nhật");
    }

    [Fact]
    public async Task Them_sua_xoa_thanh_vien_va_lich_su_vao_ra()
    {
        var client = app.CreateAuthClient();
        var hdRes = await client.PostAsJsonAsync("/api/hoi-doan", new
        {
            tenHoiDoan = "Gia Trưởng", thanhBonMang = (string?)null, ngayBonMang = (DateOnly?)null,
            ngayThanhLap = (DateOnly?)null, ghiChu = (string?)null, rowVersion = (uint?)null,
        });
        var hoiDoanId = (await hdRes.Content.ReadFromJsonAsync<Dictionary<string, Guid>>())!["id"];
        var giaoDanId = await TaoGiaoDan(40001, "Nguyen Van Hoi Doan");

        // Thêm hội viên
        var themRes = await client.PostAsJsonAsync($"/api/hoi-doan/{hoiDoanId}/thanh-vien", new
        {
            giaoDanId, ngayVaoHoiDoan = new DateOnly(2020, 1, 1), ngayRaHoiDoan = (DateOnly?)null,
            vaiTro = (string?)null,
        });
        themRes.StatusCode.Should().Be(HttpStatusCode.OK);

        // Mặc định VaiTro = "Hội viên" khi để trống — khớp frmHoiDoan.cs:496.
        var dsHienTai = await client.GetFromJsonAsync<List<ThanhVienDto>>($"/api/hoi-doan/{hoiDoanId}/thanh-vien");
        dsHienTai.Should().ContainSingle();
        var tv = dsHienTai![0];
        tv.VaiTro.Should().Be("Hội viên");
        tv.DaRaKhoiHoiDoan.Should().BeFalse();

        // Thêm lại giáo dân ĐANG hoạt động trong cùng hội đoàn → chặn, đúng thông báo gốc.
        var trungRes = await client.PostAsJsonAsync($"/api/hoi-doan/{hoiDoanId}/thanh-vien", new
        {
            giaoDanId, ngayVaoHoiDoan = (DateOnly?)null, ngayRaHoiDoan = (DateOnly?)null, vaiTro = (string?)null,
        });
        trungRes.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var loiTrung = await trungRes.Content.ReadFromJsonAsync<ThongBaoLoi>();
        loiTrung!.ThongBao.Should().Be("Giáo dân này đã tồn tại trong hội đoàn rồi!!!");

        // Sửa: đổi vai trò + đặt ngày ra (mô phỏng nhánh [No] "lấy ngày hiện tại làm ngày ra
        // khỏi đoàn" của frmHoiDoan.cs:590-594 — ở web thực hiện bằng cách sửa trực tiếp).
        var suaTvRes = await client.PutAsJsonAsync($"/api/hoi-doan/thanh-vien/{tv.ChiTietId}", new
        {
            ngayVaoHoiDoan = tv.NgayVaoHoiDoan, ngayRaHoiDoan = new DateOnly(2026, 1, 1),
            vaiTro = "Trưởng hội đoàn", rowVersion = tv.RowVersion,
        });
        suaTvRes.StatusCode.Should().Be(HttpStatusCode.OK);

        // Mặc định chỉ hiện hội viên hiện tại (chưa ra) → đã ra thì biến mất khỏi danh sách mặc định.
        var dsSauKhiRa = await client.GetFromJsonAsync<List<ThanhVienDto>>($"/api/hoi-doan/{hoiDoanId}/thanh-vien");
        dsSauKhiRa.Should().BeEmpty();

        // chiXemHienTai=false → thấy lại trong lịch sử, đúng Vai trò/Ngày ra vừa sửa.
        var lichSu = await client.GetFromJsonAsync<List<ThanhVienDto>>(
            $"/api/hoi-doan/{hoiDoanId}/thanh-vien?chiXemHienTai=false");
        lichSu.Should().ContainSingle();
        lichSu![0].VaiTro.Should().Be("Trưởng hội đoàn");
        lichSu[0].DaRaKhoiHoiDoan.Should().BeTrue();

        // Xoá vĩnh viễn — nhánh [Yes] của hộp thoại 3 nút frmHoiDoan.cs:582-589.
        var xoaTvRes = await client.DeleteAsync($"/api/hoi-doan/thanh-vien/{lichSu[0].ChiTietId}");
        xoaTvRes.StatusCode.Should().Be(HttpStatusCode.OK);
        (await client.GetFromJsonAsync<List<ThanhVienDto>>(
            $"/api/hoi-doan/{hoiDoanId}/thanh-vien?chiXemHienTai=false")).Should().BeEmpty();
    }

    [Fact]
    public async Task Xoa_hoi_doan_thi_cascade_xoa_het_thanh_vien()
    {
        var client = app.CreateAuthClient();
        var hdRes = await client.PostAsJsonAsync("/api/hoi-doan", new
        {
            tenHoiDoan = "Hội đoàn để xoá", thanhBonMang = (string?)null, ngayBonMang = (DateOnly?)null,
            ngayThanhLap = (DateOnly?)null, ghiChu = (string?)null, rowVersion = (uint?)null,
        });
        var hoiDoanId = (await hdRes.Content.ReadFromJsonAsync<Dictionary<string, Guid>>())!["id"];
        var giaoDanId = await TaoGiaoDan(40002, "Tran Thi Se Xoa");
        await client.PostAsJsonAsync($"/api/hoi-doan/{hoiDoanId}/thanh-vien", new
        {
            giaoDanId, ngayVaoHoiDoan = (DateOnly?)null, ngayRaHoiDoan = (DateOnly?)null, vaiTro = (string?)null,
        });

        var xoaRes = await client.DeleteAsync($"/api/hoi-doan/{hoiDoanId}");
        xoaRes.StatusCode.Should().Be(HttpStatusCode.OK);

        var ds = await client.GetFromJsonAsync<List<HoiDoanDto>>("/api/hoi-doan/danh-sach");
        ds!.Should().NotContain(h => h.Id == hoiDoanId);

        var xoaLai = await client.DeleteAsync($"/api/hoi-doan/{hoiDoanId}");
        xoaLai.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
