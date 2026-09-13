using System.Net;
using System.Net.Http.Json;
using System.Text;
using FluentAssertions;
using Qlgx.Api.Dtos;
using Qlgx.Domain.Entities;

namespace Qlgx.Api.Tests;

/// <summary>Kiểm thử màn hình "Quản lý mẫu in" — năng lực MỚI (xem
/// docs/superpowers/specs/man-hinh/quan-ly-mau-in.md). Trọng tâm: thứ tự phân giải mẫu (giáo
/// xứ → hệ thống → gốc), phân quyền (giáo xứ A không sửa được mẫu giáo xứ B, tài khoản thường
/// không sửa được mẫu hệ thống), khử trùng HTML server-side, chống ghi đè bằng RowVersion.</summary>
public class MauInTests(QlgxApiFactory app) : IClassFixture<QlgxApiFactory>
{
    private const string TenMauThu = "ChungNhanBiTich";

    private async Task<Guid> TaoGiaoDan(Guid giaoXuId, int ma)
    {
        await using var db = app.TaoContextThuan();
        var gd = new GiaoDan
        {
            GiaoXuId = giaoXuId, MaGiaoDanCu = ma, HoTen = "Nguoi Mau In", TenThanh = "Maria", Phai = "Nữ",
        };
        db.GiaoDan.Add(gd);
        await db.SaveChangesAsync();
        return gd.Id;
    }

    [Fact]
    public async Task Danh_sach_co_du_13_mau_va_bao_dung_cap_dang_dung()
    {
        var res = await app.CreateAuthClient().GetAsync("/api/mau-in");
        res.StatusCode.Should().Be(HttpStatusCode.OK);
        var ds = await res.Content.ReadFromJsonAsync<List<MauInDanhSachItemDto>>();

        ds!.Should().HaveCount(12); // 12 TenMau — "Phiếu gia đình A3" dùng chung TenMau "PhieuGiaDinh".
        ds!.Should().OnlyContain(m => m.CapDangDung == "MacDinh");
        ds!.Sum(m => m.ChoTrong.Count).Should().Be(238, "đúng tổng 238 chỗ trống đã đếm bằng grep");
    }

    [Fact]
    public async Task Giao_xu_tu_sua_mau_rieng_uu_tien_hon_mau_he_thong()
    {
        var gxId = Guid.NewGuid();
        await using (var db = app.TaoContextThuan())
        {
            db.GiaoXu.Add(new GiaoXu { Id = gxId, TenGiaoXu = "GX thu tu tien mau", MaGiaoXuCu = new Random().Next(80000, 89999) });
            await db.SaveChangesAsync();
        }
        var giaoDanId = await TaoGiaoDan(gxId, 55001);
        var client = app.CreateAuthClient(gxId);
        var clientHeThong = app.CreateAuthClient(gxId, loaiTaiKhoan: 9);

        var pdfGoc = await client.GetByteArrayAsync($"/api/giao-dan/{giaoDanId}/in/chung-nhan-bi-tich");

        // Hệ thống thêm một đoạn dài — dùng làm "mốc hệ thống".
        var heThongLay = await clientHeThong.GetAsync($"/api/mau-in/{TenMauThu}/he-thong");
        var heThongDto = await heThongLay.Content.ReadFromJsonAsync<MauInChiTietDto>();
        var htmlHeThong = heThongDto!.NoiDungHtml.Replace("</body>",
            $"<p>{new string('H', 3000)}</p></body>");
        (await clientHeThong.PutAsJsonAsync($"/api/mau-in/{TenMauThu}/he-thong",
            new LuuMauInRequest(htmlHeThong, heThongDto.RowVersion))).EnsureSuccessStatusCode();

        var pdfHeThong = await client.GetByteArrayAsync($"/api/giao-dan/{giaoDanId}/in/chung-nhan-bi-tich");
        pdfHeThong.Length.Should().BeGreaterThan(pdfGoc.Length + 500,
            "chưa có mẫu riêng thì phải rơi xuống mẫu tuỳ chỉnh hệ thống");

        // Giáo xứ tự đặt mẫu riêng, đoạn NGẮN HƠN đoạn hệ thống — nếu giáo xứ ưu tiên đúng, PDF
        // phải NHỎ HƠN pdfHeThong (không phải cộng dồn cả hai).
        var riengLay = await client.GetAsync($"/api/mau-in/{TenMauThu}/rieng");
        var riengDto = await riengLay.Content.ReadFromJsonAsync<MauInChiTietDto>();
        var htmlRieng = riengDto!.NoiDungHtml.Replace("</body>", "<p>Rieng</p></body>");
        (await client.PutAsJsonAsync($"/api/mau-in/{TenMauThu}/rieng",
            new LuuMauInRequest(htmlRieng, riengDto.RowVersion))).EnsureSuccessStatusCode();

        var pdfRieng = await client.GetByteArrayAsync($"/api/giao-dan/{giaoDanId}/in/chung-nhan-bi-tich");
        pdfRieng.Length.Should().BeLessThan(pdfHeThong.Length,
            "mẫu RIÊNG của giáo xứ phải ưu tiên cao hơn mẫu hệ thống, không phải cộng dồn");

        // Khôi phục cả hai — phải về đúng kích thước gốc ban đầu.
        (await client.DeleteAsync($"/api/mau-in/{TenMauThu}/rieng")).EnsureSuccessStatusCode();
        (await clientHeThong.DeleteAsync($"/api/mau-in/{TenMauThu}/he-thong")).EnsureSuccessStatusCode();
        var pdfSauKhoiPhuc = await client.GetByteArrayAsync($"/api/giao-dan/{giaoDanId}/in/chung-nhan-bi-tich");
        pdfSauKhoiPhuc.Length.Should().Be(pdfGoc.Length, "khôi phục cả hai cấp phải về đúng PDF gốc ban đầu");
    }

    [Fact]
    public async Task Giao_xu_A_khong_sua_duoc_mau_rieng_cua_giao_xu_B()
    {
        var gxB = Guid.NewGuid();
        await using (var db = app.TaoContextThuan())
        {
            db.GiaoXu.Add(new GiaoXu { Id = gxB, TenGiaoXu = "GX B mau in", MaGiaoXuCu = new Random().Next(70000, 79999) });
            await db.SaveChangesAsync();
        }
        var clientB = app.CreateAuthClient(gxB);
        (await clientB.PutAsJsonAsync($"/api/mau-in/{TenMauThu}/rieng",
            new LuuMauInRequest("<html><body>Cua B</body></html>", 0))).EnsureSuccessStatusCode();

        var clientA = app.CreateAuthClient(); // giáo xứ mặc định của factory — khác gxB.
        var layA = await clientA.GetAsync($"/api/mau-in/{TenMauThu}/rieng");
        var dtoA = await layA.Content.ReadFromJsonAsync<MauInChiTietDto>();

        dtoA!.DaTuyChinh.Should().BeFalse("giáo xứ A không được thấy/động tới mẫu riêng của giáo xứ B");
    }

    [Fact]
    public async Task Tai_khoan_thuong_khong_sua_duoc_mau_he_thong()
    {
        var res = await app.CreateAuthClient(loaiTaiKhoan: 0)
            .PutAsJsonAsync($"/api/mau-in/{TenMauThu}/he-thong", new LuuMauInRequest("<html></html>", 0));
        res.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Tai_khoan_thuong_khong_qua_QuanTri_khong_sua_duoc_mau_rieng()
    {
        // LoaiTaiKhoan=1 (không phải 0="QuanTri" cũng không phải 9="QuanTriHeThong") — mô
        // phỏng một loại tài khoản khác chưa được cấp quyền quản trị giáo xứ.
        var res = await app.CreateAuthClient(loaiTaiKhoan: 1)
            .PutAsJsonAsync($"/api/mau-in/{TenMauThu}/rieng", new LuuMauInRequest("<html></html>", 0));
        res.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Khong_dang_nhap_thi_khong_goi_duoc_endpoint_nao()
    {
        var res = await app.CreateClient().GetAsync("/api/mau-in");
        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Luu_mau_khu_trung_bo_script_va_thuoc_tinh_on()
    {
        var client = app.CreateAuthClient();
        var doc = "<html><body><p onclick=\"alert(1)\">Xin chào</p>" +
                   "<script>alert(1)</script><a href=\"javascript:alert(2)\">Bấm</a></body></html>";

        (await client.PutAsJsonAsync($"/api/mau-in/{TenMauThu}/rieng", new LuuMauInRequest(doc, 0)))
            .EnsureSuccessStatusCode();

        await using var db = app.TaoContextThuan();
        var dong = db.MauInTuyChinh.Single(m => m.GiaoXuId == app.GiaoXuId && m.TenMau == TenMauThu);
        dong.NoiDungHtml.Should().NotContain("<script", "phải bị khử trùng ở server, không chỉ tin trình soạn thảo");
        dong.NoiDungHtml.Should().NotContain("onclick");
        dong.NoiDungHtml.Should().NotContain("javascript:");
        dong.NoiDungHtml.Should().Contain("Xin chào", "chỉ loại bỏ phần nguy hiểm, giữ nguyên nội dung hợp lệ");

        // Dọn — test khác trong lớp này cũng dùng chung TenMauThu/giáo xứ mặc định.
        db.MauInTuyChinh.Remove(dong);
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task Tu_choi_mau_qua_300kb()
    {
        var client = app.CreateAuthClient();
        var docQuaLon = "<html><body>" + new string('A', 310 * 1024) + "</body></html>";

        var res = await client.PutAsJsonAsync($"/api/mau-in/{TenMauThu}/rieng", new LuuMauInRequest(docQuaLon, 0));

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RowVersion_sai_bao_xung_dot_khong_ghi_de_am_tham()
    {
        var client = app.CreateAuthClient();
        var tenMau = "ChungNhanHonPhoi"; // mẫu riêng, tránh đụng test khác dùng TenMauThu.
        (await client.PutAsJsonAsync($"/api/mau-in/{tenMau}/rieng",
            new LuuMauInRequest("<html><body>Ban 1</body></html>", 0))).EnsureSuccessStatusCode();

        var res = await client.PutAsJsonAsync($"/api/mau-in/{tenMau}/rieng",
            new LuuMauInRequest("<html><body>Ban 2 tu mot tab cu</body></html>", 0)); // RowVersion=0 đã lỗi thời.

        res.StatusCode.Should().Be(HttpStatusCode.Conflict);

        await using var db = app.TaoContextThuan();
        var dong = db.MauInTuyChinh.Single(m => m.GiaoXuId == app.GiaoXuId && m.TenMau == tenMau);
        db.MauInTuyChinh.Remove(dong);
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task Ten_mau_khong_ton_tai_tra_404()
    {
        var client = app.CreateAuthClient();
        var res = await client.GetAsync("/api/mau-in/MauKhongCoThat/rieng");
        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Xem_thu_ve_pdf_tu_html_nhap_chua_can_luu()
    {
        var client = app.CreateAuthClient();
        var res = await client.PostAsJsonAsync($"/api/mau-in/{TenMauThu}/xem-thu",
            new XemThuMauInRequest("<html><body><h1>{{HoTen}}</h1></body></html>"));

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        res.Content.Headers.ContentType!.MediaType.Should().Be("application/pdf");
        var bytes = await res.Content.ReadAsByteArrayAsync();
        Encoding.ASCII.GetString(bytes, 0, 5).Should().Be("%PDF-");

        // Không được lưu gì vào CSDL — "xem thử" phải hoàn toàn không phụ thuộc trạng thái đã lưu.
        await using var db = app.TaoContextThuan();
        db.MauInTuyChinh.Any(m => m.GiaoXuId == app.GiaoXuId && m.TenMau == TenMauThu).Should().BeFalse();
    }
}
