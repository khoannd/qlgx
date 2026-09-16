using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Qlgx.Api.Dtos;
using Qlgx.Domain.Entities;

namespace Qlgx.Api.Tests;

/// <summary>
/// "Danh sách các cha quản xứ" ở màn hình "Giáo xứ" (xem giao-xu.md mục 3.2, khác
/// <see cref="GiaoXuTests"/> — LinhMuc kế thừa ThucTheCoSo nên CÓ RLS bảo vệ, không cần service
/// tự lọc GiaoXuId thủ công như GiaoXu/GiaoHat/GiaoPhan).
/// </summary>
public class LinhMucTests(QlgxApiFactory app) : IClassFixture<QlgxApiFactory>
{
    [Fact]
    public async Task Nguoi_chua_dang_nhap_bi_chan_401()
    {
        var res = await app.CreateClient().GetAsync("/api/linh-muc");

        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Thieu_ho_ten_thi_bao_loi_dung_nguyen_van()
    {
        var res = await app.CreateAuthClient().PostAsJsonAsync("/api/linh-muc", new TaoLinhMucRequest(
            "Giuse", "  ", null, "Chánh xứ", null, null, null, null, null));

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var loi = await res.Content.ReadFromJsonAsync<LoiThongBao>();
        loi!.ThongBao.Should().Be("Hãy nhập họ tên!");
    }

    [Fact]
    public async Task Them_roi_xem_lai_thay_dung_thong_tin_vua_them()
    {
        var client = app.CreateAuthClient();

        var res = await client.PostAsJsonAsync("/api/linh-muc", new TaoLinhMucRequest(
            "Phanxicô Xaviê", "Võ Quang Thanh", new DateOnly(1958, 9, 5), "Chánh xứ",
            new DateOnly(2004, 7, 12), null, "Ghi chú thử", "0123456789", "cha@gx.vn"));
        res.StatusCode.Should().Be(HttpStatusCode.Created);

        var ds = await client.GetFromJsonAsync<List<LinhMucDto>>("/api/linh-muc");
        var vuaThem = ds!.Single(l => l.HoTen == "Võ Quang Thanh");
        vuaThem.TenThanh.Should().Be("Phanxicô Xaviê");
        vuaThem.ChucVu.Should().Be("Chánh xứ");
        vuaThem.TuNgay.Should().Be(new DateOnly(2004, 7, 12));
        vuaThem.DenNgay.Should().BeNull();
    }

    [Fact]
    public async Task Sua_duoc_thong_tin_mot_cha_quan_xu()
    {
        var client = app.CreateAuthClient();
        await client.PostAsJsonAsync("/api/linh-muc", new TaoLinhMucRequest(
            "Giuse", "Nguyễn Văn A", null, "Chánh xứ", new DateOnly(1995, 1, 1), null, null, null, null));
        var ds = await client.GetFromJsonAsync<List<LinhMucDto>>("/api/linh-muc");
        var id = ds!.Single(l => l.HoTen == "Nguyễn Văn A").Id;

        var sua = await client.PutAsJsonAsync($"/api/linh-muc/{id}", new CapNhatLinhMucRequest(
            "Giuse", "Nguyễn Văn A", null, "Chánh xứ", new DateOnly(1995, 1, 1),
            new DateOnly(2004, 7, 12), "Đã mãn nhiệm", null, null));

        sua.StatusCode.Should().Be(HttpStatusCode.OK);
        var sauKhiSua = (await client.GetFromJsonAsync<List<LinhMucDto>>("/api/linh-muc"))!.Single(l => l.Id == id);
        sauKhiSua.DenNgay.Should().Be(new DateOnly(2004, 7, 12));
        sauKhiSua.GhiChu.Should().Be("Đã mãn nhiệm");
    }

    [Fact]
    public async Task Sua_khong_ton_tai_thi_bao_404()
    {
        var res = await app.CreateAuthClient().PutAsJsonAsync($"/api/linh-muc/{Guid.NewGuid()}",
            new CapNhatLinhMucRequest(null, "Ai do", null, null, null, null, null, null, null));

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Xoa_thi_khong_con_thay_trong_danh_sach_nua()
    {
        var client = app.CreateAuthClient();
        await client.PostAsJsonAsync("/api/linh-muc", new TaoLinhMucRequest(
            null, "Sẽ Bị Xoá", null, null, null, null, null, null, null));
        var ds = await client.GetFromJsonAsync<List<LinhMucDto>>("/api/linh-muc");
        var id = ds!.Single(l => l.HoTen == "Sẽ Bị Xoá").Id;

        var xoa = await client.DeleteAsync($"/api/linh-muc/{id}");

        xoa.StatusCode.Should().Be(HttpStatusCode.OK);
        var sauKhiXoa = await client.GetFromJsonAsync<List<LinhMucDto>>("/api/linh-muc");
        sauKhiXoa!.Should().NotContain(l => l.Id == id);
    }

    /// <summary>
    /// Bài test bảo mật cốt lõi: khác GiaoXuTests (phải tự lọc thủ công), LinhMuc kế thừa
    /// ThucTheCoSo nên được BẢO VỆ SẴN bởi bộ lọc EF Core + RLS PostgreSQL — chèn thẳng một cha
    /// quản xứ của giáo xứ KHÁC qua context thuần (bỏ qua bộ lọc), rồi xác nhận danh sách của
    /// giáo xứ đang đăng nhập không thấy được, giống hệt cách DanhMucTests chứng minh RLS.
    /// </summary>
    [Fact]
    public async Task Khong_thay_cha_quan_xu_cua_giao_xu_khac()
    {
        var giaoXuKhac = Guid.NewGuid();
        await using (var db = app.TaoContextThuan())
        {
            db.GiaoXu.Add(new GiaoXu { Id = giaoXuKhac, TenGiaoXu = "Giao xu B linh muc test", MaGiaoXuCu = 88921 });
            db.LinhMuc.Add(new LinhMuc { GiaoXuId = giaoXuKhac, MaLinhMucCu = 1, HoTen = "Cha Cua Xu B" });
            await db.SaveChangesAsync();
        }

        var ds = await app.CreateAuthClient().GetFromJsonAsync<List<LinhMucDto>>("/api/linh-muc");

        ds.Should().NotContain(l => l.HoTen == "Cha Cua Xu B");
    }

    private sealed record LoiThongBao(string ThongBao);
}
