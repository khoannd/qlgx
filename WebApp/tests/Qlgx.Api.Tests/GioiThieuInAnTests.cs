using System.Net;
using System.Text;
using FluentAssertions;
using Qlgx.Domain;
using Qlgx.Domain.Entities;

namespace Qlgx.Api.Tests;

/// <summary>Bốn mẫu "Giấy giới thiệu" (hạng mục cuối cùng của in-an.md — xem mục 8/5e):
/// chuyển xứ (theo gia đình), rửa tội, thêm sức, giáo lý hôn phối (ba mẫu sau theo giáo dân).
/// Khác ba mẫu trước ở chỗ CẦN thông tin bên nhận (giáo phận/giáo xứ khác) nhập tự do lúc in —
/// xem ghi chú ở đầu khối "Giấy giới thiệu" trong InAnService.cs. Cùng cách kiểm thử: KHÔNG so
/// khớp nội dung PDF nhị phân, chỉ kiểm chữ ký tệp "%PDF-", mã trạng thái, và cách ly giáo xứ.</summary>
public class GioiThieuInAnTests(QlgxApiFactory app) : IClassFixture<QlgxApiFactory>
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

    private async Task VerifyPdf(HttpResponseMessage res)
    {
        res.StatusCode.Should().Be(HttpStatusCode.OK);
        res.Content.Headers.ContentType!.MediaType.Should().Be("application/pdf");
        var bytes = await res.Content.ReadAsByteArrayAsync();
        bytes.Length.Should().BeGreaterThan(500);
        Encoding.ASCII.GetString(bytes, 0, 5).Should().Be("%PDF-");
    }

    private const string ThamSoBenNhan = "giaoPhan2=Xuan+Loc&giaoXu2=Giao+xu+Bac+Hoa&tenLinhMuc=LM+Nguyen+Van+A";

    // --- Giới thiệu rửa tội -----------------------------------------------------------------

    [Fact]
    public async Task Gioi_thieu_rua_toi_xuat_thanh_cong()
    {
        var id = await TaoGiaoDan(9001, "Nguyen Van Du Tong");

        var res = await app.CreateAuthClient().GetAsync($"/api/giao-dan/{id}/in/gioi-thieu-rua-toi?{ThamSoBenNhan}");

        await VerifyPdf(res);
    }

    [Fact]
    public async Task Gioi_thieu_rua_toi_thieu_giao_xu_nhan_tra_400()
    {
        var id = await TaoGiaoDan(9002, "Nguyen Van Thieu");

        var res = await app.CreateAuthClient().GetAsync($"/api/giao-dan/{id}/in/gioi-thieu-rua-toi?giaoPhan2=Xuan+Loc");

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Gioi_thieu_rua_toi_tra_404_khi_khong_tim_thay()
    {
        var res = await app.CreateAuthClient().GetAsync($"/api/giao-dan/{Guid.NewGuid()}/in/gioi-thieu-rua-toi?{ThamSoBenNhan}");

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // --- Giới thiệu thêm sức ----------------------------------------------------------------

    [Fact]
    public async Task Gioi_thieu_them_suc_xuat_thanh_cong_du_du_lieu_rua_toi()
    {
        var id = await TaoGiaoDan(9010, "Nguyen Van Them Suc", g =>
        {
            g.NgayRuaToi = new DateOnly(2010, 5, 1);
            g.NoiRuaToi = "Giao xu Test";
            g.SoRuaToi = "5/2010";
            g.ChaRuaToi = "LM Test";
        });

        var res = await app.CreateAuthClient().GetAsync($"/api/giao-dan/{id}/in/gioi-thieu-them-suc?{ThamSoBenNhan}");

        await VerifyPdf(res);
    }

    [Fact]
    public async Task Gioi_thieu_them_suc_van_xuat_duoc_khi_thieu_du_lieu_rua_toi()
    {
        // Đúng trường hợp bản desktop (RpGioiThieuThemSuc.cs) in "..................." khi
        // thiếu NgayRuaToi — bản web dùng VanBanInAn.MoTaBiTich (bỏ hẳn đoạn thiếu) thay vì
        // literal dấu chấm, nhưng vẫn phải in được bình thường, không phải 404/500.
        var id = await TaoGiaoDan(9011, "Nguyen Van Chua Rua Toi");

        var res = await app.CreateAuthClient().GetAsync($"/api/giao-dan/{id}/in/gioi-thieu-them-suc?{ThamSoBenNhan}");

        await VerifyPdf(res);
    }

    // --- Giới thiệu giáo lý hôn phối ---------------------------------------------------------

    [Fact]
    public async Task Gioi_thieu_giao_ly_hon_phoi_xuat_thanh_cong()
    {
        var id = await TaoGiaoDan(9020, "Nguyen Van Hon Phoi", g =>
        {
            g.NgayRuaToi = new DateOnly(2000, 1, 1);
            g.NgayThemSuc = new DateOnly(2012, 1, 1);
        });

        var res = await app.CreateAuthClient().GetAsync($"/api/giao-dan/{id}/in/gioi-thieu-giao-ly-hon-phoi?{ThamSoBenNhan}");

        await VerifyPdf(res);
    }

    [Fact]
    public async Task Gioi_thieu_giao_ly_hon_phoi_tra_404_khi_khong_tim_thay()
    {
        var res = await app.CreateAuthClient().GetAsync($"/api/giao-dan/{Guid.NewGuid()}/in/gioi-thieu-giao-ly-hon-phoi?{ThamSoBenNhan}");

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // --- Giới thiệu chuyển xứ (theo GIA ĐÌNH) -------------------------------------------------

    [Fact]
    public async Task Gioi_thieu_chuyen_xu_xuat_thanh_cong_voi_nhieu_thanh_vien()
    {
        var chongId = await TaoGiaoDan(9101, "Nguyen Van Chuyen", g => g.Phai = "Nam");
        var voId = await TaoGiaoDan(9102, "Tran Thi Chuyen", g => g.Phai = "Nữ");
        var conId = await TaoGiaoDan(9103, "Nguyen Van Con Chuyen", g => g.Phai = "Nam");
        var giaDinhId = await TaoGiaDinhVoiThanhVien(9100,
            (chongId, VaiTroGiaDinh.Chong, true), (voId, VaiTroGiaDinh.Vo, false), (conId, VaiTroGiaDinh.Con, false));

        var res = await app.CreateAuthClient().GetAsync($"/api/gia-dinh/{giaDinhId}/in/gioi-thieu-chuyen-xu?{ThamSoBenNhan}");

        await VerifyPdf(res);
    }

    [Fact]
    public async Task Gioi_thieu_chuyen_xu_thieu_giao_xu_nhan_tra_400()
    {
        var chongId = await TaoGiaoDan(9104, "Nguyen Van Thieu Nhan");
        var giaDinhId = await TaoGiaDinhVoiThanhVien(9105, (chongId, VaiTroGiaDinh.Chong, true));

        var res = await app.CreateAuthClient().GetAsync($"/api/gia-dinh/{giaDinhId}/in/gioi-thieu-chuyen-xu?giaoPhan2=Xuan+Loc");

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Gioi_thieu_chuyen_xu_tra_404_khi_khong_tim_thay_gia_dinh()
    {
        var res = await app.CreateAuthClient().GetAsync($"/api/gia-dinh/{Guid.NewGuid()}/in/gioi-thieu-chuyen-xu?{ThamSoBenNhan}");

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // --- Cách ly giáo xứ (KHÔNG được in chéo) ------------------------------------------------

    [Fact]
    public async Task Khong_in_duoc_gioi_thieu_rua_toi_cua_giao_xu_khac()
    {
        var giaoXuKhac = Guid.NewGuid();
        Guid giaoDanIdXuKhac;
        await using (var db = app.TaoContextThuan())
        {
            db.GiaoXu.Add(new GiaoXu { Id = giaoXuKhac, TenGiaoXu = "Giao xu khac 3", MaGiaoXuCu = 997 });
            var gdXuKhac = new GiaoDan { GiaoXuId = giaoXuKhac, MaGiaoDanCu = 9401, HoTen = "Nguoi Xu Khac 3", Phai = "Nam" };
            db.GiaoDan.Add(gdXuKhac);
            await db.SaveChangesAsync();
            giaoDanIdXuKhac = gdXuKhac.Id;
        }

        var res = await app.CreateAuthClient().GetAsync($"/api/giao-dan/{giaoDanIdXuKhac}/in/gioi-thieu-rua-toi?{ThamSoBenNhan}");

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
