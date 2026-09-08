using System.Net;
using System.Text;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Qlgx.Domain;
using Qlgx.Domain.Entities;

namespace Qlgx.Api.Tests;

/// <summary>Ba mẫu in mới của lượt "làm tiếp hệ thống in ấn" (sau mẫu "Lý lịch cá nhân" —
/// xem GiaoDanInAnTests.cs): Chứng nhận bí tích, Phiếu gia đình, Chứng nhận hôn phối. Xem
/// docs/superpowers/specs/man-hinh/in-an.md. Cùng cách kiểm thử: KHÔNG so khớp nội dung PDF
/// nhị phân, chỉ kiểm chữ ký tệp "%PDF-", mã trạng thái, và cách ly giáo xứ.</summary>
public class InAnMauMoiTests(QlgxApiFactory app) : IClassFixture<QlgxApiFactory>
{
    private async Task<Guid> TaoGiaoDan(int ma, string hoTen, Action<GiaoDan>? tuyChinh = null)
    {
        await using var db = app.TaoContextThuan();
        var gd = new GiaoDan { GiaoXuId = app.GiaoXuId, MaGiaoDanCu = ma, HoTen = hoTen, Phai = "Nam" };
        tuyChinh?.Invoke(gd);
        db.GiaoDan.Add(gd);
        await db.SaveChangesAsync();
        return gd.Id;
    }

    private async Task VerifyPdf(HttpResponseMessage res)
    {
        res.StatusCode.Should().Be(HttpStatusCode.OK);
        res.Content.Headers.ContentType!.MediaType.Should().Be("application/pdf");
        var bytes = await res.Content.ReadAsByteArrayAsync();
        bytes.Length.Should().BeGreaterThan(500);
        Encoding.ASCII.GetString(bytes, 0, 5).Should().Be("%PDF-");
    }

    // --- Chứng nhận bí tích -------------------------------------------------------------

    [Fact]
    public async Task Chung_nhan_bi_tich_TatCa_xuat_thanh_cong()
    {
        var id = await TaoGiaoDan(8001, "Nguyen Van Bi Tich", g =>
        {
            g.NgayRuaToi = new DateOnly(2000, 1, 1);
            g.NoiRuaToi = "GX Vo Nhiem";
            g.SoRuaToi = "1/2000";
            g.ChaRuaToi = "LM Test";
        });

        var res = await app.CreateAuthClient().GetAsync($"/api/giao-dan/{id}/in/chung-nhan-bi-tich");

        await VerifyPdf(res);
    }

    [Theory]
    [InlineData("RuaToi", 8010)]
    [InlineData("RuocLe", 8011)]
    [InlineData("ThemSuc", 8012)]
    public async Task Chung_nhan_bi_tich_tung_loai_xuat_thanh_cong(string loai, int ma)
    {
        var id = await TaoGiaoDan(ma, "Nguyen Van " + loai);

        var res = await app.CreateAuthClient().GetAsync($"/api/giao-dan/{id}/in/chung-nhan-bi-tich?loai={loai}");

        await VerifyPdf(res);
    }

    [Fact]
    public async Task Chung_nhan_bi_tich_khong_du_lieu_van_xuat_duoc_khong_nham_lam_404()
    {
        // Giáo dân không có bất kỳ dữ liệu bí tích nào — đúng trường hợp gây ra lỗi trình bày
        // đã sửa (dấu phẩy lửng, "tại" bơ vơ) — vẫn phải in được, chỉ là danh sách trống.
        var id = await TaoGiaoDan(8003, "Nguyen Van Trong");

        var res = await app.CreateAuthClient().GetAsync($"/api/giao-dan/{id}/in/chung-nhan-bi-tich");

        await VerifyPdf(res);
    }

    [Fact]
    public async Task Chung_nhan_bi_tich_tra_404_khi_khong_tim_thay()
    {
        var res = await app.CreateAuthClient()
            .GetAsync($"/api/giao-dan/{Guid.NewGuid()}/in/chung-nhan-bi-tich");

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // --- Phiếu gia đình ------------------------------------------------------------------

    private async Task<Guid> TaoGiaDinhVoiThanhVien(int maGiaDinh, params (Guid GiaoDanId, VaiTroGiaDinh VaiTro, bool ChuHo)[] thanhVien)
    {
        await using var db = app.TaoContextThuan();
        var gd = new GiaDinh { GiaoXuId = app.GiaoXuId, MaGiaDinhCu = maGiaDinh, TenGiaDinh = "Gia dinh " + maGiaDinh };
        db.GiaDinh.Add(gd);
        foreach (var (giaoDanId, vaiTro, chuHo) in thanhVien)
            db.ThanhVienGiaDinh.Add(new ThanhVienGiaDinh
                { GiaoXuId = app.GiaoXuId, GiaDinhId = gd.Id, GiaoDanId = giaoDanId, VaiTro = vaiTro, ChuHo = chuHo });
        await db.SaveChangesAsync();
        return gd.Id;
    }

    [Fact]
    public async Task Phieu_gia_dinh_xuat_thanh_cong_voi_nhieu_thanh_vien()
    {
        var chongId = await TaoGiaoDan(8101, "Nguyen Van Chong", g => g.Phai = "Nam");
        var voId = await TaoGiaoDan(8102, "Tran Thi Vo", g => g.Phai = "Nữ");
        var conId = await TaoGiaoDan(8103, "Nguyen Van Con", g => g.Phai = "Nam");
        var giaDinhId = await TaoGiaDinhVoiThanhVien(8100,
            (chongId, VaiTroGiaDinh.Chong, true), (voId, VaiTroGiaDinh.Vo, false), (conId, VaiTroGiaDinh.Con, false));

        var res = await app.CreateAuthClient().GetAsync($"/api/gia-dinh/{giaDinhId}/in/phieu-gia-dinh");

        await VerifyPdf(res);
    }

    [Fact]
    public async Task Phieu_gia_dinh_tra_404_khi_khong_tim_thay()
    {
        var res = await app.CreateAuthClient().GetAsync($"/api/gia-dinh/{Guid.NewGuid()}/in/phieu-gia-dinh");

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // --- Chứng nhận hôn phối ---------------------------------------------------------------

    [Fact]
    public async Task Chung_nhan_hon_phoi_xuat_thanh_cong_khi_gia_dinh_co_hon_phoi()
    {
        var chongId = await TaoGiaoDan(8201, "Nguyen Van Chong Hp", g => g.Phai = "Nam");
        var voId = await TaoGiaoDan(8202, "Tran Thi Vo Hp", g => g.Phai = "Nữ");
        var giaDinhId = await TaoGiaDinhVoiThanhVien(8200,
            (chongId, VaiTroGiaDinh.Chong, true), (voId, VaiTroGiaDinh.Vo, false));

        await using (var db = app.TaoContextThuan())
        {
            var hp = new HonPhoi { GiaoXuId = app.GiaoXuId, MaHonPhoiCu = 8200, SoHonPhoi = "HP/8200", NgayHonPhoi = new DateOnly(2020, 1, 1) };
            db.HonPhoi.Add(hp);
            db.GiaoDanHonPhoi.AddRange(
                new GiaoDanHonPhoi { GiaoXuId = app.GiaoXuId, HonPhoi = hp, GiaoDanId = chongId, SoThuTu = 1 },
                new GiaoDanHonPhoi { GiaoXuId = app.GiaoXuId, HonPhoi = hp, GiaoDanId = voId, SoThuTu = 2 });
            await db.SaveChangesAsync();
        }

        var res = await app.CreateAuthClient().GetAsync($"/api/gia-dinh/{giaDinhId}/in/chung-nhan-hon-phoi");

        await VerifyPdf(res);
    }

    [Fact]
    public async Task Chung_nhan_hon_phoi_tra_404_khi_gia_dinh_chua_co_hon_phoi()
    {
        var chongId = await TaoGiaoDan(8301, "Nguyen Van Doc Than", g => g.Phai = "Nam");
        var giaDinhId = await TaoGiaDinhVoiThanhVien(8300, (chongId, VaiTroGiaDinh.Chong, true));

        var res = await app.CreateAuthClient().GetAsync($"/api/gia-dinh/{giaDinhId}/in/chung-nhan-hon-phoi");

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Chung_nhan_hon_phoi_tra_404_khi_khong_tim_thay_gia_dinh()
    {
        var res = await app.CreateAuthClient().GetAsync($"/api/gia-dinh/{Guid.NewGuid()}/in/chung-nhan-hon-phoi");

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // --- Cách ly giáo xứ (KHÔNG được in chéo) ----------------------------------------------

    [Fact]
    public async Task Khong_in_duoc_phieu_gia_dinh_cua_giao_xu_khac()
    {
        var giaoXuKhac = Guid.NewGuid();
        Guid giaDinhIdXuKhac;
        await using (var db = app.TaoContextThuan())
        {
            db.GiaoXu.Add(new GiaoXu { Id = giaoXuKhac, TenGiaoXu = "Giao xu khac 2", MaGiaoXuCu = 998 });
            var gdXuKhac = new GiaoDan { GiaoXuId = giaoXuKhac, MaGiaoDanCu = 8401, HoTen = "Nguoi Xu Khac 2", Phai = "Nam" };
            db.GiaoDan.Add(gdXuKhac);
            await db.SaveChangesAsync();
            var giaDinh = new GiaDinh { GiaoXuId = giaoXuKhac, MaGiaDinhCu = 8400, TenGiaDinh = "GD Xu Khac" };
            db.GiaDinh.Add(giaDinh);
            db.ThanhVienGiaDinh.Add(new ThanhVienGiaDinh
                { GiaoXuId = giaoXuKhac, GiaDinhId = giaDinh.Id, GiaoDanId = gdXuKhac.Id, VaiTro = VaiTroGiaDinh.Chong, ChuHo = true });
            await db.SaveChangesAsync();
            giaDinhIdXuKhac = giaDinh.Id;
        }

        var res = await app.CreateAuthClient().GetAsync($"/api/gia-dinh/{giaDinhIdXuKhac}/in/phieu-gia-dinh");

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // --- "In lý lịch cá nhân" từ GIA ĐÌNH (in cả gia đình, một trang/người) -----------------

    [Fact]
    public async Task Ly_lich_ca_nhan_gia_dinh_xuat_thanh_cong_voi_nhieu_thanh_vien()
    {
        var chongId = await TaoGiaoDan(8501, "Nguyen Van Chong LLCN", g => g.Phai = "Nam");
        var voId = await TaoGiaoDan(8502, "Tran Thi Vo LLCN", g => g.Phai = "Nữ");
        var conId = await TaoGiaoDan(8503, "Nguyen Van Con LLCN", g => g.Phai = "Nam");
        var giaDinhId = await TaoGiaDinhVoiThanhVien(8500,
            (chongId, VaiTroGiaDinh.Chong, true), (voId, VaiTroGiaDinh.Vo, false), (conId, VaiTroGiaDinh.Con, false));

        var res = await app.CreateAuthClient().GetAsync($"/api/gia-dinh/{giaDinhId}/in/ly-lich-ca-nhan");

        await VerifyPdf(res);
        // PDF gộp 3 người phải nặng hơn hẳn PDF một người (mỗi người một trang đầy đủ nội
        // dung) — cùng cách so kích thước gián tiếp mà bài test ảnh đại diện đã dùng, không so
        // khớp byte PDF trực tiếp.
        var resMotNguoi = await app.CreateAuthClient().GetAsync($"/api/giao-dan/{chongId}/in/ly-lich-ca-nhan");
        var bytesGop = await res.Content.ReadAsByteArrayAsync();
        var bytesMotNguoi = await resMotNguoi.Content.ReadAsByteArrayAsync();
        bytesGop.Length.Should().BeGreaterThan(bytesMotNguoi.Length);
    }

    [Fact]
    public async Task Ly_lich_ca_nhan_gia_dinh_tra_404_khi_khong_tim_thay_gia_dinh()
    {
        var res = await app.CreateAuthClient().GetAsync($"/api/gia-dinh/{Guid.NewGuid()}/in/ly-lich-ca-nhan");

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Ly_lich_ca_nhan_gia_dinh_tra_404_khi_gia_dinh_khong_con_thanh_vien()
    {
        var giaDinhId = await TaoGiaDinhVoiThanhVien(8510);

        var res = await app.CreateAuthClient().GetAsync($"/api/gia-dinh/{giaDinhId}/in/ly-lich-ca-nhan");

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Ly_lich_ca_nhan_gia_dinh_khong_in_duoc_gia_dinh_cua_giao_xu_khac()
    {
        var giaoXuKhac = Guid.NewGuid();
        Guid giaDinhIdXuKhac;
        await using (var db = app.TaoContextThuan())
        {
            db.GiaoXu.Add(new GiaoXu { Id = giaoXuKhac, TenGiaoXu = "Giao xu khac LLCN", MaGiaoXuCu = 997 });
            var gdXuKhac = new GiaoDan { GiaoXuId = giaoXuKhac, MaGiaoDanCu = 8511, HoTen = "Nguoi Xu Khac LLCN", Phai = "Nam" };
            db.GiaoDan.Add(gdXuKhac);
            await db.SaveChangesAsync();
            var giaDinh = new GiaDinh { GiaoXuId = giaoXuKhac, MaGiaDinhCu = 8512, TenGiaDinh = "GD Xu Khac LLCN" };
            db.GiaDinh.Add(giaDinh);
            db.ThanhVienGiaDinh.Add(new ThanhVienGiaDinh
                { GiaoXuId = giaoXuKhac, GiaDinhId = giaDinh.Id, GiaoDanId = gdXuKhac.Id, VaiTro = VaiTroGiaDinh.Chong, ChuHo = true });
            await db.SaveChangesAsync();
            giaDinhIdXuKhac = giaDinh.Id;
        }

        var res = await app.CreateAuthClient().GetAsync($"/api/gia-dinh/{giaDinhIdXuKhac}/in/ly-lich-ca-nhan");

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // --- "In danh sách" (toolbar "Danh sách giáo dân"/"Danh sách gia đình") ----------------

    [Fact]
    public async Task In_danh_sach_giao_dan_xuat_thanh_cong_ke_ca_khi_khong_co_ai()
    {
        // Không tạo giáo dân nào riêng cho test này — chỉ cần xác nhận endpoint luôn trả 200
        // (không phải 404) dù danh sách rỗng, đúng tinh thần "xuất Excel" đã có (không có khái
        // niệm 404 cho một danh sách, chỉ có danh sách trống).
        var res = await app.CreateAuthClient().GetAsync("/api/giao-dan/in/danh-sach?chiKhongThongKe=true&hienCaDaMat=false");

        await VerifyPdf(res);
    }

    [Fact]
    public async Task In_danh_sach_giao_dan_co_du_lieu_xuat_thanh_cong()
    {
        await TaoGiaoDan(8601, "Nguyen Van Danh Sach A");
        await TaoGiaoDan(8602, "Nguyen Van Danh Sach B");

        var res = await app.CreateAuthClient().GetAsync("/api/giao-dan/in/danh-sach");

        await VerifyPdf(res);
    }

    [Fact]
    public async Task In_danh_sach_gia_dinh_xuat_thanh_cong()
    {
        var chongId = await TaoGiaoDan(8701, "Nguyen Van Chong DS", g => g.Phai = "Nam");
        await TaoGiaDinhVoiThanhVien(8700, (chongId, VaiTroGiaDinh.Chong, true));

        var res = await app.CreateAuthClient().GetAsync("/api/gia-dinh/in/danh-sach");

        await VerifyPdf(res);
    }

    /// <summary>"In danh sách" gọi thẳng <c>LayDanhSach</c> — bộ lọc chung của
    /// <c>QlgxDbContext</c> áp dụng bình thường nên giáo dân/gia đình của giáo xứ KHÁC không
    /// bao giờ lọt vào PDF của giáo xứ hiện tại. Kiểm bằng cách đếm số dòng gián tiếp qua kích
    /// thước PDF: thêm một bản ghi ở giáo xứ khác không được làm PDF nặng thêm.</summary>
    [Fact]
    public async Task In_danh_sach_giao_dan_khong_lan_sang_giao_xu_khac()
    {
        var resTruoc = await app.CreateAuthClient().GetAsync("/api/giao-dan/in/danh-sach");
        var bytesTruoc = await resTruoc.Content.ReadAsByteArrayAsync();

        var giaoXuKhac = Guid.NewGuid();
        await using (var db = app.TaoContextThuan())
        {
            db.GiaoXu.Add(new GiaoXu { Id = giaoXuKhac, TenGiaoXu = "Giao xu khac DS", MaGiaoXuCu = 996 });
            db.GiaoDan.Add(new GiaoDan { GiaoXuId = giaoXuKhac, MaGiaoDanCu = 8801, HoTen = "Nguoi Xu Khac DS", Phai = "Nam" });
            await db.SaveChangesAsync();
        }

        var resSau = await app.CreateAuthClient().GetAsync("/api/giao-dan/in/danh-sach");
        var bytesSau = await resSau.Content.ReadAsByteArrayAsync();

        // Không so bằng tuyệt đối (NgayThangNamIn có giờ:phút, PDF có thể lệch vài byte metadata
        // giữa hai lần gọi) — chỉ cần xác nhận KHÔNG có thêm một dòng đầy đủ (chênh > 50 byte,
        // đủ để phát hiện một dòng <tr> mới nếu vô tình lọt qua bộ lọc).
        Math.Abs(bytesSau.Length - bytesTruoc.Length).Should().BeLessThan(50);
    }
}
