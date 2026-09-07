using System.Net;
using System.Net.Http.Headers;
using FluentAssertions;
using Qlgx.Domain.Entities;
using SkiaSharp;

namespace Qlgx.Api.Tests;

/// <summary>
/// Ảnh đại diện giáo dân/gia đình (VIEC-TIEP-THEO.md mục 1.2, xem
/// docs/superpowers/specs/man-hinh/can-review-sau.md mục 36) — tải lên, xem lại, xoá. Trọng
/// tâm là BA ràng buộc bắt buộc: (1) kiểm CHỮ KÝ THẬT của tệp chứ không tin phần mở rộng/
/// Content-Type trình duyệt khai, (2) chặn tệp quá lớn, (3) không đọc/ghi được ảnh của giáo xứ
/// khác dù đoán đúng Id bản ghi.
/// </summary>
public class AnhDaiDienTests(QlgxApiFactory app) : IClassFixture<QlgxApiFactory>
{
    private async Task<Guid> TaoGiaoDan(Guid giaoXuId, int ma, string hoTen)
    {
        await using var db = app.TaoContextThuan();
        var gd = new GiaoDan
        {
            GiaoXuId = giaoXuId, MaGiaoDanCu = ma, HoTen = hoTen, TenThanh = "Maria",
            Phai = "Nữ", NgaySinh = new DateOnly(1990, 5, 1), NoiSinh = "Phan Thiết",
        };
        db.GiaoDan.Add(gd);
        await db.SaveChangesAsync();
        return gd.Id;
    }

    private async Task<Guid> TaoGiaDinh(Guid giaoXuId, int ma, string ten)
    {
        await using var db = app.TaoContextThuan();
        var gd = new GiaDinh { GiaoXuId = giaoXuId, MaGiaDinhCu = ma, TenGiaDinh = ten };
        db.GiaDinh.Add(gd);
        await db.SaveChangesAsync();
        return gd.Id;
    }

    /// <summary>Một ảnh JPEG THẬT nhỏ (dựng bằng SkiaSharp, không phải chuỗi giả) — dùng để
    /// kiểm tra luồng "tải lên hợp lệ" đi qua được cả bước giải mã thật của XuLyAnh.</summary>
    private static byte[] TaoJpegThat(int canh = 20, SKColor? mau = null)
    {
        using var bmp = new SKBitmap(canh, canh);
        using (var canvas = new SKCanvas(bmp))
            canvas.Clear(mau ?? SKColors.CornflowerBlue);
        using var anh = SKImage.FromBitmap(bmp);
        using var duLieu = anh.Encode(SKEncodedImageFormat.Jpeg, 90);
        return duLieu.ToArray();
    }

    private static MultipartFormDataContent DungMultipart(byte[] duLieu, string tenTep, string loaiNoiDung)
    {
        var noiDungTep = new ByteArrayContent(duLieu);
        noiDungTep.Headers.ContentType = MediaTypeHeaderValue.Parse(loaiNoiDung);
        var form = new MultipartFormDataContent { { noiDungTep, "tep", tenTep } };
        return form;
    }

    // --- Giáo dân -------------------------------------------------------------------------

    [Fact]
    public async Task Tai_anh_hop_le_len_roi_xem_lai_thanh_cong()
    {
        var id = await TaoGiaoDan(app.GiaoXuId, 9401, "Nguoi Co Anh");
        var client = app.CreateAuthClient();

        var resTai = await client.PostAsync($"/api/giao-dan/{id}/anh-dai-dien",
            DungMultipart(TaoJpegThat(), "anh.jpg", "image/jpeg"));
        resTai.StatusCode.Should().Be(HttpStatusCode.OK);

        var resXem = await client.GetAsync($"/api/giao-dan/{id}/anh-dai-dien");
        resXem.StatusCode.Should().Be(HttpStatusCode.OK);
        resXem.Content.Headers.ContentType!.MediaType.Should().Be("image/jpeg");
        var bytes = await resXem.Content.ReadAsByteArrayAsync();
        // Chữ ký JPEG chuẩn (FF D8 FF) — bằng chứng ảnh đã được XuLyAnh giải mã + nén lại
        // thật sự, không phải lưu nguyên văn một chuỗi bất kỳ.
        bytes.Length.Should().BeGreaterThan(0);
        bytes[0].Should().Be(0xFF);
        bytes[1].Should().Be(0xD8);
        bytes[2].Should().Be(0xFF);
    }

    [Fact]
    public async Task Tep_khong_phai_anh_dat_ten_jpg_bi_tu_choi_du_khai_dung_content_type()
    {
        var id = await TaoGiaoDan(app.GiaoXuId, 9402, "Nguoi Tai Sai");
        var client = app.CreateAuthClient();

        // Nội dung THẬT là văn bản thuần, không phải ảnh — nhưng đặt tên .jpg VÀ khai
        // Content-Type image/jpeg, đúng kịch bản người dùng đổi tên một tệp độc hại.
        var duLieuGia = System.Text.Encoding.UTF8.GetBytes("Đây không phải là ảnh, chỉ là văn bản thuần.");
        var res = await client.PostAsync($"/api/giao-dan/{id}/anh-dai-dien",
            DungMultipart(duLieuGia, "anh-gia.jpg", "image/jpeg"));

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var than = await res.Content.ReadAsStringAsync();
        than.Should().Contain("không phải ảnh hợp lệ");

        // Không có gì được lưu — GET vẫn 404.
        var resXem = await client.GetAsync($"/api/giao-dan/{id}/anh-dai-dien");
        resXem.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Tep_qua_lon_bi_tu_choi()
    {
        var id = await TaoGiaoDan(app.GiaoXuId, 9403, "Nguoi Tai To");
        var client = app.CreateAuthClient();

        // 9MB — vượt giới hạn 8MB (Qlgx.Api.Anh.XuLyAnh.GioiHanDungLuongGoc). Nội dung không
        // cần là ảnh hợp lệ: kiểm tra kích thước chạy TRƯỚC bước giải mã.
        var duLieuTo = new byte[9 * 1024 * 1024];
        var res = await client.PostAsync($"/api/giao-dan/{id}/anh-dai-dien",
            DungMultipart(duLieuTo, "anh-to.jpg", "image/jpeg"));

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var than = await res.Content.ReadAsStringAsync();
        than.Should().Contain("quá lớn");
    }

    [Fact]
    public async Task Xoa_anh_xong_khong_con_xem_duoc_nua()
    {
        var id = await TaoGiaoDan(app.GiaoXuId, 9404, "Nguoi Xoa Anh");
        var client = app.CreateAuthClient();
        await client.PostAsync($"/api/giao-dan/{id}/anh-dai-dien", DungMultipart(TaoJpegThat(), "anh.jpg", "image/jpeg"));

        var resXoa = await client.DeleteAsync($"/api/giao-dan/{id}/anh-dai-dien");
        resXoa.StatusCode.Should().Be(HttpStatusCode.OK);

        var resXem = await client.GetAsync($"/api/giao-dan/{id}/anh-dai-dien");
        resXem.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Khong_xem_duoc_anh_giao_dan_cua_giao_xu_khac()
    {
        var giaoXuKhac = Guid.NewGuid();
        await using (var db = app.TaoContextThuan())
        {
            db.GiaoXu.Add(new GiaoXu { Id = giaoXuKhac, TenGiaoXu = "Giao xu khac anh", MaGiaoXuCu = 998 });
            await db.SaveChangesAsync();
        }
        var idNguoiXuKhac = await TaoGiaoDan(giaoXuKhac, 9405, "Nguoi Xu Khac Co Anh");
        await using (var db = app.TaoContextThuan())
        {
            var g = db.GiaoDan.Single(x => x.Id == idNguoiXuKhac);
            g.AnhDaiDienDuLieu = TaoJpegThat();
            g.AnhDaiDienLoaiNoiDung = "image/jpeg";
            await db.SaveChangesAsync();
        }

        // Token phát hành cho app.GiaoXuId (giáo xứ mặc định) — không phải giáo xứ vừa tạo.
        var client = app.CreateAuthClient();
        var resXem = await client.GetAsync($"/api/giao-dan/{idNguoiXuKhac}/anh-dai-dien");
        resXem.StatusCode.Should().Be(HttpStatusCode.NotFound);

        // Cũng không tải đè/xoá được ảnh giáo xứ khác dù biết đúng Id.
        var resTai = await client.PostAsync($"/api/giao-dan/{idNguoiXuKhac}/anh-dai-dien",
            DungMultipart(TaoJpegThat(), "anh.jpg", "image/jpeg"));
        resTai.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var resXoa = await client.DeleteAsync($"/api/giao-dan/{idNguoiXuKhac}/anh-dai-dien");
        resXoa.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Tra_404_khi_chua_co_anh()
    {
        var id = await TaoGiaoDan(app.GiaoXuId, 9406, "Nguoi Chua Co Anh");
        var res = await app.CreateAuthClient().GetAsync($"/api/giao-dan/{id}/anh-dai-dien");
        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // --- Gia đình (cùng hạ tầng, kiểm gọn hơn — logic dùng chung AnhDaiDienService) --------

    [Fact]
    public async Task Gia_dinh_tai_anh_len_xem_lai_va_xoa_duoc()
    {
        var id = await TaoGiaDinh(app.GiaoXuId, 9450, "Gia dinh co anh");
        var client = app.CreateAuthClient();

        var resTai = await client.PostAsync($"/api/gia-dinh/{id}/anh-dai-dien",
            DungMultipart(TaoJpegThat(), "anh.png", "image/png"));
        resTai.StatusCode.Should().Be(HttpStatusCode.OK);

        var resXem = await client.GetAsync($"/api/gia-dinh/{id}/anh-dai-dien");
        resXem.StatusCode.Should().Be(HttpStatusCode.OK);
        resXem.Content.Headers.ContentType!.MediaType.Should().Be("image/jpeg");

        var resXoa = await client.DeleteAsync($"/api/gia-dinh/{id}/anh-dai-dien");
        resXoa.StatusCode.Should().Be(HttpStatusCode.OK);
        (await client.GetAsync($"/api/gia-dinh/{id}/anh-dai-dien")).StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Gia_dinh_khong_xem_duoc_anh_cua_giao_xu_khac()
    {
        var giaoXuKhac = Guid.NewGuid();
        await using (var db = app.TaoContextThuan())
        {
            db.GiaoXu.Add(new GiaoXu { Id = giaoXuKhac, TenGiaoXu = "Giao xu khac anh gd", MaGiaoXuCu = 997 });
            await db.SaveChangesAsync();
        }
        Guid idGiaDinhKhac;
        await using (var db = app.TaoContextThuan())
        {
            var gd = new GiaDinh
            {
                GiaoXuId = giaoXuKhac, MaGiaDinhCu = 9451, TenGiaDinh = "Gia dinh xu khac",
                AnhDaiDienDuLieu = TaoJpegThat(), AnhDaiDienLoaiNoiDung = "image/jpeg",
            };
            db.GiaDinh.Add(gd);
            await db.SaveChangesAsync();
            idGiaDinhKhac = gd.Id;
        }

        var res = await app.CreateAuthClient().GetAsync($"/api/gia-dinh/{idGiaDinhKhac}/anh-dai-dien");
        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
