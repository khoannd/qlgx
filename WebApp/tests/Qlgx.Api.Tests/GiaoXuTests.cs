using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Qlgx.Api.Dtos;
using Qlgx.Domain.Entities;

namespace Qlgx.Api.Tests;

/// <summary>
/// Màn hình "Giáo xứ" tự sửa thông tin xứ mình (xem
/// docs/superpowers/specs/man-hinh/giao-xu.md) — thay <c>frmGiaoXu.cs</c>. KHÁC
/// <see cref="QuanLyGiaoXuTests"/>: đây là màn hình cho MỌI tài khoản của MỘT giáo xứ tự sửa
/// đúng giáo xứ của mình, không nhận GiaoXuId từ trình duyệt/route. GiaoXu không có giao_xu_id
/// nên KHÔNG có RLS bảo vệ — bài test dưới đây CHỨNG MINH lớp lọc tường minh ở GiaoXuService là
/// đủ chặn việc sửa giáo xứ khác.
/// </summary>
public class GiaoXuTests(QlgxApiFactory app) : IClassFixture<QlgxApiFactory>
{
    [Fact]
    public async Task Nguoi_chua_dang_nhap_bi_chan_401()
    {
        var res = await app.CreateClient().GetAsync("/api/giao-xu");

        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Xem_duoc_dung_thong_tin_giao_xu_cua_minh()
    {
        var client = app.CreateAuthClient(app.GiaoXuId, loaiTaiKhoan: 0);

        var res = await client.GetAsync("/api/giao-xu");

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        var tt = await res.Content.ReadFromJsonAsync<GiaoXuHienTaiResponse>();
        tt!.Id.Should().Be(app.GiaoXuId);
    }

    [Fact]
    public async Task Sua_duoc_thong_tin_giao_xu_cua_minh_va_luu_dung_xuong_csdl()
    {
        var client = app.CreateAuthClient(app.GiaoXuId, loaiTaiKhoan: 0);

        var res = await client.PutAsJsonAsync("/api/giao-xu",
            new CapNhatGiaoXuHienTaiRequest("Ten giao xu da sua", "Dia chi moi", "0909", "a@b.com", "http://x", "ghi chu"));

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        await using var db = app.TaoContextThuan();
        var giaoXu = await db.GiaoXu.SingleAsync(x => x.Id == app.GiaoXuId);
        giaoXu.TenGiaoXu.Should().Be("Ten giao xu da sua");
        giaoXu.DiaChi.Should().Be("Dia chi moi");
    }

    /// <summary>
    /// Bài test bảo mật cốt lõi của màn hình: dựng 2 giáo xứ (A = app.GiaoXuId, B = giaoXuKhac),
    /// đăng nhập giáo xứ A, sửa thông tin qua API "của mình" — giáo xứ B TUYỆT ĐỐI không được
    /// đổi. API không hề nhận GiaoXuId làm tham số nên không có "đường tấn công" truyền id B vào
    /// — bài test này xác nhận đúng điều đó: dù DB có nhiều giáo xứ, thao tác của A chỉ chạm
    /// đúng 1 dòng của A.
    /// </summary>
    [Fact]
    public async Task Dang_nhap_giao_xu_A_sua_khong_lam_doi_thong_tin_giao_xu_B()
    {
        var giaoXuB = Guid.NewGuid();
        await using (var db = app.TaoContextThuan())
        {
            db.GiaoXu.Add(new GiaoXu
            {
                Id = giaoXuB, TenGiaoXu = "Giao xu B nguyen ban", DiaChi = "Dia chi B goc", MaGiaoXuCu = 88901,
            });
            await db.SaveChangesAsync();
        }
        var clientA = app.CreateAuthClient(app.GiaoXuId, loaiTaiKhoan: 0);

        // A xem thông tin "của mình" — không được thấy/đổi được B qua đường này.
        var xem = await clientA.GetAsync("/api/giao-xu");
        var ttA = await xem.Content.ReadFromJsonAsync<GiaoXuHienTaiResponse>();
        ttA!.Id.Should().Be(app.GiaoXuId);
        ttA.Id.Should().NotBe(giaoXuB);

        var sua = await clientA.PutAsJsonAsync("/api/giao-xu",
            new CapNhatGiaoXuHienTaiRequest("A da doi ten", "A da doi dia chi", null, null, null, null));
        sua.StatusCode.Should().Be(HttpStatusCode.OK);

        await using var db2 = app.TaoContextThuan();
        var b = await db2.GiaoXu.SingleAsync(x => x.Id == giaoXuB);
        b.TenGiaoXu.Should().Be("Giao xu B nguyen ban", "sua thong tin cua A tuyet doi khong duoc dam vao B");
        b.DiaChi.Should().Be("Dia chi B goc");
    }

    [Fact]
    public async Task Ten_giao_xu_rong_bi_tu_choi()
    {
        var client = app.CreateAuthClient(app.GiaoXuId, loaiTaiKhoan: 0);

        var res = await client.PutAsJsonAsync("/api/giao-xu",
            new CapNhatGiaoXuHienTaiRequest("", "Dia chi", null, null, null, null));

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// Tên Giáo phận/Giáo hạt hiện CHỈ ĐỌC (xem GiaoXuHienTaiResponse) — đọc đúng qua điều
    /// hướng GiaoHat!.GiaoPhan!.TenGiaoPhan, không có ô sửa tương ứng ở
    /// <see cref="CapNhatGiaoXuHienTaiRequest"/> nên không cần test riêng cho việc "không sửa
    /// được": kiểu C# đã không cho gửi hai trường đó lên khi PUT.
    /// </summary>
    /// <summary>
    /// Dựng một giáo xứ RIÊNG (không đụng app.GiaoXuId dùng chung cho cả lớp test) rồi đăng
    /// nhập đúng giáo xứ đó — tránh mọi phụ thuộc vào THỨ TỰ chạy test so với
    /// <see cref="Chua_gan_giao_hat_thi_ten_giao_phan_va_giao_hat_la_null"/> vốn kiểm tra
    /// app.GiaoXuId còn nguyên chưa gán giáo hạt.
    /// </summary>
    [Fact]
    public async Task Doc_duoc_ten_giao_phan_va_giao_hat_qua_dieu_huong_giao_hat()
    {
        var giaoXuRieng = Guid.NewGuid();
        var giaoPhanId = Guid.NewGuid();
        var giaoHatId = Guid.NewGuid();
        await using (var db = app.TaoContextThuan())
        {
            db.GiaoPhan.Add(new GiaoPhan { Id = giaoPhanId, TenGiaoPhan = "Giáo phận Thử Nghiệm", MaGiaoPhanCu = 1 });
            db.GiaoHat.Add(new GiaoHat { Id = giaoHatId, GiaoPhanId = giaoPhanId, TenGiaoHat = "Giáo hạt Thử Nghiệm", MaGiaoHatCu = 1 });
            db.GiaoXu.Add(new GiaoXu
            {
                Id = giaoXuRieng, TenGiaoXu = "Giao xu co giao hat", MaGiaoXuCu = 88922, GiaoHatId = giaoHatId,
            });
            await db.SaveChangesAsync();
        }

        var tt = await app.CreateAuthClient(giaoXuRieng).GetFromJsonAsync<GiaoXuHienTaiResponse>("/api/giao-xu");

        tt!.TenGiaoPhan.Should().Be("Giáo phận Thử Nghiệm");
        tt.TenGiaoHat.Should().Be("Giáo hạt Thử Nghiệm");
    }

    [Fact]
    public async Task Chua_gan_giao_hat_thi_ten_giao_phan_va_giao_hat_la_null()
    {
        var tt = await app.CreateAuthClient().GetFromJsonAsync<GiaoXuHienTaiResponse>("/api/giao-xu");

        tt!.TenGiaoPhan.Should().BeNull();
        tt.TenGiaoHat.Should().BeNull();
    }

    [Fact]
    public async Task Dia_chi_rong_bi_tu_choi()
    {
        var client = app.CreateAuthClient(app.GiaoXuId, loaiTaiKhoan: 0);

        var res = await client.PutAsJsonAsync("/api/giao-xu",
            new CapNhatGiaoXuHienTaiRequest("Ten giao xu", "", null, null, null, null));

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
