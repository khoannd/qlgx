using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Qlgx.Api.Dtos;
using Qlgx.Domain.Entities;

namespace Qlgx.Api.Tests;

/// <summary>
/// Task 14 — kiểm chứng ranh giới bảo mật cốt lõi của mô hình nhiều giáo xứ dùng chung một
/// máy chủ: GiaoXuId của phiên CHỈ lấy từ claim của token, không bao giờ từ tham số trình
/// duyệt, và không endpoint nghiệp vụ nào phục vụ được người chưa đăng nhập.
/// </summary>
public class BaoMatTests(QlgxApiFactory app) : IClassFixture<QlgxApiFactory>
{
    private sealed record GiaoDanItem(Guid Id, string HoTen);

    private async Task<Guid> TaoGiaoXuKhac(string ten)
    {
        await using var db = app.TaoContextThuan();
        var giaoXu = new GiaoXu { TenGiaoXu = ten, MaGiaoXuCu = new Random().Next(90000, 99999) };
        db.GiaoXu.Add(giaoXu);
        await db.SaveChangesAsync();
        return giaoXu.Id;
    }

    private static int _maGiaoDanKeTiep = 70001;

    private async Task<Guid> TaoGiaoDan(Guid giaoXuId, string hoTen)
    {
        await using var db = app.TaoContextThuan();
        var gd = new GiaoDan
        {
            GiaoXuId = giaoXuId, HoTen = hoTen, TenThanh = "Test", Phai = "Nam",
            MaGiaoDanCu = Interlocked.Increment(ref _maGiaoDanKeTiep),
        };
        db.GiaoDan.Add(gd);
        await db.SaveChangesAsync();
        return gd.Id;
    }

    [Fact]
    public async Task Chua_dang_nhap_thi_bi_chan_401_khong_ro_du_lieu()
    {
        var client = app.CreateClient(); // KHÔNG có header Authorization

        var res = await client.GetAsync("/api/giao-dan");

        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData("/api/giao-dan")]
    [InlineData("/api/gia-dinh")]
    [InlineData("/api/giao-ho")]
    [InlineData("/api/hoi-doan")]
    public async Task Moi_endpoint_doc_deu_doi_hoi_xac_thuc(string duong)
    {
        var client = app.CreateClient();

        var res = await client.GetAsync(duong);

        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized, $"endpoint {duong} phai doi hoi dang nhap");
    }

    [Fact]
    public async Task Nguoi_dung_giao_xu_A_khong_thay_du_lieu_giao_xu_B()
    {
        var giaoXuB = await TaoGiaoXuKhac("Giao xu B (bao mat test)");
        await TaoGiaoDan(app.GiaoXuId, "Nguoi cua xu A");
        await TaoGiaoDan(giaoXuB, "Nguoi cua xu B");

        var clientA = app.CreateAuthClient(app.GiaoXuId);
        var ds = await clientA.GetFromJsonAsync<List<GiaoDanItem>>("/api/giao-dan");

        ds!.Should().Contain(x => x.HoTen == "Nguoi cua xu A");
        ds!.Should().NotContain(x => x.HoTen == "Nguoi cua xu B");
    }

    [Fact]
    public async Task Truyen_giaoXuId_cua_xu_khac_qua_query_khong_doi_duoc_ket_qua()
    {
        var giaoXuB = await TaoGiaoXuKhac("Giao xu C (bao mat test)");
        await TaoGiaoDan(app.GiaoXuId, "Nguoi xu A rieng biet");
        await TaoGiaoDan(giaoXuB, "Nguoi xu C rieng biet");

        // Dang nhap bang giao xu A nhung co gang nhet giaoXuId cua xu C qua query string —
        // khong endpoint nao doc tham so nay (da ra soat toan bo Endpoints/*.cs), nen phai
        // hoan toan khong anh huong, GiaoXuId van lay tu claim cua token.
        var clientA = app.CreateAuthClient(app.GiaoXuId);
        var ds = await clientA.GetFromJsonAsync<List<GiaoDanItem>>($"/api/giao-dan?giaoXuId={giaoXuB}");

        ds!.Should().Contain(x => x.HoTen == "Nguoi xu A rieng biet");
        ds!.Should().NotContain(x => x.HoTen == "Nguoi xu C rieng biet");
    }

    [Fact]
    public async Task Gia_dinh_va_thanh_vien_cung_khong_ro_giua_hai_giao_xu()
    {
        var giaoXuB = await TaoGiaoXuKhac("Giao xu D (bao mat test)");
        await using (var db = app.TaoContextThuan())
        {
            db.GiaDinh.Add(new GiaDinh { GiaoXuId = app.GiaoXuId, TenGiaDinh = "Gia dinh xu A" });
            db.GiaDinh.Add(new GiaDinh { GiaoXuId = giaoXuB, TenGiaDinh = "Gia dinh xu D" });
            await db.SaveChangesAsync();
        }

        var clientA = app.CreateAuthClient(app.GiaoXuId);
        var res = await clientA.GetAsync("/api/gia-dinh");
        var than = await res.Content.ReadAsStringAsync();

        than.Should().Contain("Gia dinh xu A");
        than.Should().NotContain("Gia dinh xu D");
    }

    [Fact]
    public async Task Dang_nhap_dung_mat_khau_tra_ve_token_dung_giao_xu()
    {
        var (tenTaiKhoan, matKhau) = ("nguoidung_baomat", "MatKhauManh123!");
        await using (var db = app.TaoContextThuan())
        {
            var taiKhoan = new TaiKhoan { GiaoXuId = app.GiaoXuId, TenTaiKhoan = tenTaiKhoan, HoTenNguoiDung = "Nguoi Dung Test" };
            taiKhoan.MatKhauBam = new PasswordHasher<TaiKhoan>().HashPassword(taiKhoan, matKhau);
            db.TaiKhoan.Add(taiKhoan);
            await db.SaveChangesAsync();
        }

        var res = await app.CreateClient().PostAsJsonAsync("/api/auth/dang-nhap",
            new DangNhapRequest(tenTaiKhoan, matKhau));

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        var than = await res.Content.ReadFromJsonAsync<DangNhapOkDto>();
        than!.NguoiDung.GiaoXuId.Should().Be(app.GiaoXuId);

        // Token that phai dung duoc de goi endpoint bao ve.
        var client = app.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", than.Token);
        var goiThu = await client.GetAsync("/api/giao-dan");
        goiThu.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private sealed record DangNhapOkDto(string Token, ThongTinNguoiDungDto NguoiDung);
    private sealed record ThongTinNguoiDungDto(Guid GiaoXuId, string TenGiaoXu);

    [Fact]
    public async Task Dang_nhap_thanh_cong_tra_ve_dung_ten_giao_xu_that()
    {
        // AppShell.tsx thanh tren truoc day viet cung "Giao xu Thanh Tam" — sau khi noi du
        // lieu that (can-review-sau.md muc 32), dang nhap phai tra ve dung ten giao xu cua
        // TAI KHOAN dang nhap, khong phai mot chuoi tinh nao khac.
        var giaoXuRieng = await TaoGiaoXuKhac("Giao xu Vo Nhiem (test ten that)");
        var (tenTaiKhoan, matKhau) = ("nguoidung_tengiaoxu", "MatKhauManh123!");
        await using (var db = app.TaoContextThuan())
        {
            var taiKhoan = new TaiKhoan { GiaoXuId = giaoXuRieng, TenTaiKhoan = tenTaiKhoan, HoTenNguoiDung = "Z" };
            taiKhoan.MatKhauBam = new PasswordHasher<TaiKhoan>().HashPassword(taiKhoan, matKhau);
            db.TaiKhoan.Add(taiKhoan);
            await db.SaveChangesAsync();
        }

        var res = await app.CreateClient().PostAsJsonAsync("/api/auth/dang-nhap",
            new DangNhapRequest(tenTaiKhoan, matKhau));

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        var than = await res.Content.ReadFromJsonAsync<DangNhapOkDto>();
        than!.NguoiDung.TenGiaoXu.Should().Be("Giao xu Vo Nhiem (test ten that)");
    }

    private sealed record ToiDto(string? GiaoXuId, string? TenGiaoXu);

    [Fact]
    public async Task Auth_toi_tra_ve_dung_ten_giao_xu_cua_claim_khong_phai_tham_so_trinh_duyet()
    {
        var client = app.CreateAuthClient(app.GiaoXuId);

        var toi = await client.GetFromJsonAsync<ToiDto>("/api/auth/toi");

        toi!.GiaoXuId.Should().Be(app.GiaoXuId.ToString());
        toi.TenGiaoXu.Should().Be("Giao xu Thanh Tam"); // seed trong QlgxApiFactory.InitializeAsync
    }

    [Fact]
    public async Task Dang_nhap_sai_mat_khau_bi_tu_choi_khong_lo_thong_tin()
    {
        var (tenTaiKhoan, matKhau) = ("nguoidung_saimk", "MatKhauManh123!");
        await using (var db = app.TaoContextThuan())
        {
            var taiKhoan = new TaiKhoan { GiaoXuId = app.GiaoXuId, TenTaiKhoan = tenTaiKhoan, HoTenNguoiDung = "X" };
            taiKhoan.MatKhauBam = new PasswordHasher<TaiKhoan>().HashPassword(taiKhoan, matKhau);
            db.TaiKhoan.Add(taiKhoan);
            await db.SaveChangesAsync();
        }

        var res = await app.CreateClient().PostAsJsonAsync("/api/auth/dang-nhap",
            new DangNhapRequest(tenTaiKhoan, "sai-mat-khau"));

        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Dang_nhap_sai_qua_nguong_thi_khoa_tam_du_dung_mat_khau_dung_sau_do()
    {
        // review-backend.md muc T1: khong gioi han so lan thu sai -> brute-force khong gioi
        // han qua mang. AuthService khoa tam sau SoLanSaiToiDa=10 lan sai lien tiep (o CSDL,
        // khong phai bo dem trong bo nho, vi nhieu ban API chay song song sau load balancer).
        var (tenTaiKhoan, matKhau) = ("nguoidung_bruteforce", "MatKhauManhThatSu456!");
        await using (var db = app.TaoContextThuan())
        {
            var taiKhoan = new TaiKhoan { GiaoXuId = app.GiaoXuId, TenTaiKhoan = tenTaiKhoan, HoTenNguoiDung = "Y" };
            taiKhoan.MatKhauBam = new PasswordHasher<TaiKhoan>().HashPassword(taiKhoan, matKhau);
            db.TaiKhoan.Add(taiKhoan);
            await db.SaveChangesAsync();
        }
        var client = app.CreateClient();

        for (var i = 0; i < 10; i++)
        {
            var sai = await client.PostAsJsonAsync("/api/auth/dang-nhap",
                new DangNhapRequest(tenTaiKhoan, "sai-mat-khau-" + i));
            sai.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        // Dung MAT KHAU DUNG sau khi da vuot nguong — phai VAN bi tu choi vi tai khoan dang
        // bi khoa tam (truoc khi sua T1: khong co khoa nao, dang nhap dung se thanh cong 200).
        var dungMatKhauNhungDangKhoa = await client.PostAsJsonAsync("/api/auth/dang-nhap",
            new DangNhapRequest(tenTaiKhoan, matKhau));
        dungMatKhauNhungDangKhoa.StatusCode.Should().Be(HttpStatusCode.Unauthorized,
            "tai khoan dang bi khoa tam sau qua nhieu lan sai, du dung mat khau cung khong duoc vao");

        await using var dbKiemTra = app.TaoContextThuan();
        var sau = await dbKiemTra.TaiKhoan.SingleAsync(t => t.TenTaiKhoan == tenTaiKhoan);
        sau.KhoaDangNhapDenLuc.Should().NotBeNull("phai duoc khoa o CSDL, khong phai chi tu choi tam thoi");
        sau.KhoaDangNhapDenLuc!.Value.Should().BeAfter(DateTimeOffset.UtcNow);
    }

    [Fact]
    public async Task Khoa_dang_nhap_sai_khong_duoc_lan_sang_giao_xu_khac()
    {
        // review-cuoi.md muc M1: TenTaiKhoan chi duy nhat TRONG mot giao xu, khong duy nhat
        // toan may chu. Dung hai giao xu, MOI ben mot tai khoan TRUNG TEN "vanphong" — ke tan
        // cong nham vao giao xu A (chon dung giaoXuId cua A, dung thiet ke moi) gui sai mat
        // khau 10 lan; tai khoan "vanphong" cua giao xu B phai VAN dang nhap binh thuong duoc
        // ngay sau do, khong bi khoa lay.
        var giaoXuB = await TaoGiaoXuKhac("Giao xu B (khoa cheo giao xu test)");
        const string tenTaiKhoan = "vanphong";
        var (matKhauA, matKhauB) = ("MatKhauVanPhongA_123!", "MatKhauVanPhongB_456!");
        await using (var db = app.TaoContextThuan())
        {
            var tkA = new TaiKhoan { GiaoXuId = app.GiaoXuId, TenTaiKhoan = tenTaiKhoan, HoTenNguoiDung = "Van phong xu A" };
            tkA.MatKhauBam = new PasswordHasher<TaiKhoan>().HashPassword(tkA, matKhauA);
            var tkB = new TaiKhoan { GiaoXuId = giaoXuB, TenTaiKhoan = tenTaiKhoan, HoTenNguoiDung = "Van phong xu B" };
            tkB.MatKhauBam = new PasswordHasher<TaiKhoan>().HashPassword(tkB, matKhauB);
            db.TaiKhoan.AddRange(tkA, tkB);
            await db.SaveChangesAsync();
        }
        var client = app.CreateClient();

        for (var i = 0; i < 10; i++)
        {
            var sai = await client.PostAsJsonAsync("/api/auth/dang-nhap",
                new DangNhapRequest(tenTaiKhoan, "sai-mat-khau-" + i, app.GiaoXuId));
            sai.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        var dangNhapB = await client.PostAsJsonAsync("/api/auth/dang-nhap",
            new DangNhapRequest(tenTaiKhoan, matKhauB, giaoXuB));

        dangNhapB.StatusCode.Should().Be(HttpStatusCode.OK,
            "tai khoan 'vanphong' cua giao xu B khong lien quan gi toi 10 lan sai o giao xu A, " +
            "khong duoc bi khoa lay theo (loi khoa cheo giao xu — review-cuoi.md muc M1)");
    }

    [Fact]
    public async Task Ten_dang_nhap_trung_o_hai_giao_xu_ma_khong_chon_thi_bi_yeu_cau_chon()
    {
        var giaoXuB = await TaoGiaoXuKhac("Giao xu E (can chon giao xu test)");
        const string tenTaiKhoan = "quantri_trung_ten";
        await using (var db = app.TaoContextThuan())
        {
            var tkA = new TaiKhoan { GiaoXuId = app.GiaoXuId, TenTaiKhoan = tenTaiKhoan, HoTenNguoiDung = "A" };
            tkA.MatKhauBam = new PasswordHasher<TaiKhoan>().HashPassword(tkA, "MatKhauA_Manh123!");
            var tkB = new TaiKhoan { GiaoXuId = giaoXuB, TenTaiKhoan = tenTaiKhoan, HoTenNguoiDung = "B" };
            tkB.MatKhauBam = new PasswordHasher<TaiKhoan>().HashPassword(tkB, "MatKhauB_Manh123!");
            db.TaiKhoan.AddRange(tkA, tkB);
            await db.SaveChangesAsync();
        }

        var res = await app.CreateClient().PostAsJsonAsync("/api/auth/dang-nhap",
            new DangNhapRequest(tenTaiKhoan, "MatKhauA_Manh123!"));

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest,
            "ten dang nhap trung o hai giao xu, chua chon giao xu nao thi khong duoc doan bat ky ai");
    }

    [Fact]
    public async Task Dang_nhap_voi_mat_khau_rong_tra_401_khong_phai_500()
    {
        // Phat hien khi kiem thu kham pha man hinh chi tiet giao dan/gia dinh 2026-09-07: goi
        // /api/auth/dang-nhap voi than JSON thieu truong MatKhau (hoac MatKhau rong) lam
        // PasswordHasher.VerifyHashedPassword nem ArgumentNullException chua duoc bat, tra ve
        // 500 thay vi 401 nhu moi truong hop sai mat khau khac.
        var res = await app.CreateClient().PostAsJsonAsync("/api/auth/dang-nhap",
            new DangNhapRequest("quantri", ""));

        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Dang_nhap_voi_ten_tai_khoan_rong_tra_401_khong_phai_500()
    {
        var res = await app.CreateClient().PostAsJsonAsync("/api/auth/dang-nhap",
            new DangNhapRequest("", "bat-ky"));

        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Nguoi_dung_khong_phai_quan_tri_khong_vao_duoc_quan_ly_tai_khoan()
    {
        var client = app.CreateAuthClient(app.GiaoXuId, loaiTaiKhoan: 1); // Nguoi nhap 1

        var res = await client.GetAsync("/api/tai-khoan");

        res.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Quan_tri_vien_vao_duoc_quan_ly_tai_khoan()
    {
        var client = app.CreateAuthClient(app.GiaoXuId, loaiTaiKhoan: 0);

        var res = await client.GetAsync("/api/tai-khoan");

        res.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // --- VIEC-TIEP-THEO.md muc 1.3: tu doi mat khau cua chinh minh ---

    private async Task<(HttpClient client, string token)> DangNhapThatVaGanToken(string tenTaiKhoan, string matKhau)
    {
        var client = app.CreateClient();
        var dangNhap = await client.PostAsJsonAsync("/api/auth/dang-nhap", new DangNhapRequest(tenTaiKhoan, matKhau));
        dangNhap.StatusCode.Should().Be(HttpStatusCode.OK, "setup dang nhap that phai thanh cong");
        var than = await dangNhap.Content.ReadFromJsonAsync<DangNhapOkDto>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", than!.Token);
        return (client, than.Token);
    }

    [Fact]
    public async Task Tu_doi_mat_khau_dung_thi_thanh_cong_va_dang_nhap_lai_duoc_bang_mat_khau_moi()
    {
        var (tenTaiKhoan, matKhauCu, matKhauMoi) = ("doimk_thanhcong", "MatKhauCu_Manh123!", "MatKhauMoi_Manh456!");
        await using (var db = app.TaoContextThuan())
        {
            var tk = new TaiKhoan { GiaoXuId = app.GiaoXuId, TenTaiKhoan = tenTaiKhoan, HoTenNguoiDung = "Doi MK" };
            tk.MatKhauBam = new PasswordHasher<TaiKhoan>().HashPassword(tk, matKhauCu);
            db.TaiKhoan.Add(tk);
            await db.SaveChangesAsync();
        }
        var (client, _) = await DangNhapThatVaGanToken(tenTaiKhoan, matKhauCu);

        var doiMk = await client.PutAsJsonAsync("/api/auth/mat-khau", new DoiMatKhauRequest(matKhauCu, matKhauMoi));

        doiMk.StatusCode.Should().Be(HttpStatusCode.OK);
        var dangNhapMoi = await app.CreateClient().PostAsJsonAsync("/api/auth/dang-nhap",
            new DangNhapRequest(tenTaiKhoan, matKhauMoi));
        dangNhapMoi.StatusCode.Should().Be(HttpStatusCode.OK, "mat khau moi phai dang nhap duoc");
        var dangNhapCu = await app.CreateClient().PostAsJsonAsync("/api/auth/dang-nhap",
            new DangNhapRequest(tenTaiKhoan, matKhauCu));
        dangNhapCu.StatusCode.Should().Be(HttpStatusCode.Unauthorized, "mat khau cu phai bi tu choi sau khi doi");
    }

    [Fact]
    public async Task Tu_doi_mat_khau_sai_mat_khau_hien_tai_thi_bi_tu_choi_va_khong_doi_gi()
    {
        var (tenTaiKhoan, matKhauCu) = ("doimk_saihientai", "MatKhauCu_Manh123!");
        await using (var db = app.TaoContextThuan())
        {
            var tk = new TaiKhoan { GiaoXuId = app.GiaoXuId, TenTaiKhoan = tenTaiKhoan, HoTenNguoiDung = "Y" };
            tk.MatKhauBam = new PasswordHasher<TaiKhoan>().HashPassword(tk, matKhauCu);
            db.TaiKhoan.Add(tk);
            await db.SaveChangesAsync();
        }
        var (client, _) = await DangNhapThatVaGanToken(tenTaiKhoan, matKhauCu);

        var doiMk = await client.PutAsJsonAsync("/api/auth/mat-khau",
            new DoiMatKhauRequest("mat-khau-hien-tai-sai", "MatKhauMoiHopLe123!"));

        doiMk.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var dangNhapVanBangMatKhauCu = await app.CreateClient().PostAsJsonAsync("/api/auth/dang-nhap",
            new DangNhapRequest(tenTaiKhoan, matKhauCu));
        dangNhapVanBangMatKhauCu.StatusCode.Should().Be(HttpStatusCode.OK, "doi mat khau that bai thi khong duoc doi gi ca");
    }

    [Fact]
    public async Task Tu_doi_mat_khau_qua_ngan_thi_bi_tu_choi()
    {
        var (tenTaiKhoan, matKhauCu) = ("doimk_quangan", "MatKhauCu_Manh123!");
        await using (var db = app.TaoContextThuan())
        {
            var tk = new TaiKhoan { GiaoXuId = app.GiaoXuId, TenTaiKhoan = tenTaiKhoan, HoTenNguoiDung = "Z" };
            tk.MatKhauBam = new PasswordHasher<TaiKhoan>().HashPassword(tk, matKhauCu);
            db.TaiKhoan.Add(tk);
            await db.SaveChangesAsync();
        }
        var (client, _) = await DangNhapThatVaGanToken(tenTaiKhoan, matKhauCu);

        var doiMk = await client.PutAsJsonAsync("/api/auth/mat-khau", new DoiMatKhauRequest(matKhauCu, "1234567"));

        doiMk.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Doi_mat_khau_khong_dang_nhap_thi_bi_401()
    {
        var res = await app.CreateClient().PutAsJsonAsync("/api/auth/mat-khau",
            new DoiMatKhauRequest("bat-ky", "bat-ky-1234"));

        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // --- Rà soát tự động cảnh báo "Cross-tenant IDOR" o HoiDoanQuanLyService.cs: cac truy van
    // dang FirstOrDefaultAsync(x => x.Id == id) khong co dieu kien GiaoXuId tuong minh. HoiDoan
    // va ChiTietHoiDoan deu co bo loc toan cuc theo GiaoXuId (QlgxDbContext.cs dong 103-104) nen
    // ve ly thuyet da an toan — cac test duoi day chung minh bang hanh vi that o tang endpoint,
    // khong chi dua vao lap luan. Neu bat ky test nao FAIL tuc la co lo hong that. -------------

    private async Task<(Guid hoiDoanId, Guid chiTietId, Guid giaoDanId)> TaoHoiDoanVaHoiVien(
        Guid giaoXuId, string tenHoiDoan)
    {
        await using var db = app.TaoContextThuan();
        var gd = new GiaoDan
        {
            GiaoXuId = giaoXuId, HoTen = "Hoi vien " + tenHoiDoan, TenThanh = "Test", Phai = "Nam",
            MaGiaoDanCu = Interlocked.Increment(ref _maGiaoDanKeTiep),
        };
        db.GiaoDan.Add(gd);
        var hd = new HoiDoan { GiaoXuId = giaoXuId, TenHoiDoan = tenHoiDoan, MaHoiDoanCu = new Random().Next(80000, 89999) };
        db.HoiDoan.Add(hd);
        await db.SaveChangesAsync();
        var ct2 = new ChiTietHoiDoan
        {
            GiaoXuId = giaoXuId, HoiDoanId = hd.Id, GiaoDanId = gd.Id,
            MaChiTietHoiDoanCu = new Random().Next(80000, 89999), VaiTro = "Hội viên",
        };
        db.ChiTietHoiDoan.Add(ct2);
        await db.SaveChangesAsync();
        return (hd.Id, ct2.Id, gd.Id);
    }

    [Fact]
    public async Task Xem_chi_tiet_thanh_vien_hoi_doan_cua_xu_khac_tra_ve_danh_sach_rong()
    {
        var giaoXuB = await TaoGiaoXuKhac("Giao xu IDOR-1");
        var (hoiDoanIdB, _, _) = await TaoHoiDoanVaHoiVien(giaoXuB, "Hoi doan cua xu B");
        var clientA = app.CreateAuthClient(app.GiaoXuId);

        var res = await clientA.GetAsync($"/api/hoi-doan/{hoiDoanIdB}/thanh-vien");

        res.StatusCode.Should().Be(HttpStatusCode.OK, "endpoint nay khong tra 404 cho hoi doan khong ton tai, ma tra danh sach rong");
        var ds = await res.Content.ReadFromJsonAsync<List<object>>();
        ds.Should().BeEmpty("bo loc toan cuc theo GiaoXuId phai chan khong cho thay hoi vien cua xu khac");
    }

    [Fact]
    public async Task Sua_hoi_doan_cua_xu_khac_tra_ve_404_khong_sua_duoc()
    {
        var giaoXuB = await TaoGiaoXuKhac("Giao xu IDOR-2");
        var (hoiDoanIdB, _, _) = await TaoHoiDoanVaHoiVien(giaoXuB, "Hoi doan xu B truoc khi sua");

        var clientA = app.CreateAuthClient(app.GiaoXuId);
        var res = await clientA.PutAsJsonAsync($"/api/hoi-doan/{hoiDoanIdB}",
            new LuuHoiDoanRequest("Bi doi ten boi xu A", null, null, null, null, null));

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
        await using var db = app.TaoContextThuan();
        var hd = await db.HoiDoan.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == hoiDoanIdB);
        hd!.TenHoiDoan.Should().Be("Hoi doan xu B truoc khi sua", "du lieu cua xu B khong duoc thay doi");
    }

    [Fact]
    public async Task Xoa_hoi_doan_cua_xu_khac_tra_ve_404_khong_xoa_duoc()
    {
        var giaoXuB = await TaoGiaoXuKhac("Giao xu IDOR-3");
        var (hoiDoanIdB, _, _) = await TaoHoiDoanVaHoiVien(giaoXuB, "Hoi doan xu B khong duoc xoa");

        var clientA = app.CreateAuthClient(app.GiaoXuId);
        var res = await clientA.DeleteAsync($"/api/hoi-doan/{hoiDoanIdB}");

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
        await using var db = app.TaoContextThuan();
        (await db.HoiDoan.IgnoreQueryFilters().AnyAsync(x => x.Id == hoiDoanIdB)).Should().BeTrue(
            "hoi doan cua xu B phai con nguyen, khong bi xu A xoa mat");
    }

    [Fact]
    public async Task Them_hoi_vien_vao_hoi_doan_cua_xu_khac_tra_ve_404()
    {
        var giaoXuB = await TaoGiaoXuKhac("Giao xu IDOR-4");
        var (hoiDoanIdB, _, _) = await TaoHoiDoanVaHoiVien(giaoXuB, "Hoi doan xu B them hoi vien");
        var giaoDanA = await TaoGiaoDan(app.GiaoXuId, "Giao dan xu A bi loi dung");

        var clientA = app.CreateAuthClient(app.GiaoXuId);
        var res = await clientA.PostAsJsonAsync($"/api/hoi-doan/{hoiDoanIdB}/thanh-vien",
            new ThemThanhVienHoiDoanRequest(giaoDanA, null, null, null));

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
        await using var db = app.TaoContextThuan();
        (await db.ChiTietHoiDoan.IgnoreQueryFilters().AnyAsync(x => x.HoiDoanId == hoiDoanIdB && x.GiaoDanId == giaoDanA)).Should().BeFalse(
            "khong duoc chen duoc hoi vien vao hoi doan cua xu khac");
    }

    [Fact]
    public async Task Sua_hoi_vien_cua_xu_khac_tra_ve_404_khong_sua_duoc()
    {
        var giaoXuB = await TaoGiaoXuKhac("Giao xu IDOR-5");
        var (_, chiTietIdB, _) = await TaoHoiDoanVaHoiVien(giaoXuB, "Hoi doan xu B sua hoi vien");

        var clientA = app.CreateAuthClient(app.GiaoXuId);
        var res = await clientA.PutAsJsonAsync($"/api/hoi-doan/thanh-vien/{chiTietIdB}",
            new SuaThanhVienHoiDoanRequest(null, null, "Bi doi vai tro boi xu A", 0));

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
        await using var db = app.TaoContextThuan();
        var ct2 = await db.ChiTietHoiDoan.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == chiTietIdB);
        ct2!.VaiTro.Should().Be("Hội viên", "du lieu hoi vien cua xu B khong duoc thay doi boi xu A");
    }

    [Fact]
    public async Task Xoa_hoi_vien_cua_xu_khac_tra_ve_404_khong_xoa_duoc()
    {
        var giaoXuB = await TaoGiaoXuKhac("Giao xu IDOR-6");
        var (_, chiTietIdB, _) = await TaoHoiDoanVaHoiVien(giaoXuB, "Hoi doan xu B xoa hoi vien");

        var clientA = app.CreateAuthClient(app.GiaoXuId);
        var res = await clientA.DeleteAsync($"/api/hoi-doan/thanh-vien/{chiTietIdB}");

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
        await using var db = app.TaoContextThuan();
        (await db.ChiTietHoiDoan.IgnoreQueryFilters().AnyAsync(x => x.Id == chiTietIdB)).Should().BeTrue(
            "hoi vien cua xu B phai con nguyen, khong bi xu A xoa mat");
    }

    [Fact]
    public async Task Danh_sach_hoi_doan_cua_xu_A_khong_lan_hoi_doan_xu_khac()
    {
        var giaoXuB = await TaoGiaoXuKhac("Giao xu IDOR-7");
        await TaoHoiDoanVaHoiVien(giaoXuB, "Hoi doan chi thuoc xu B");
        await TaoHoiDoanVaHoiVien(app.GiaoXuId, "Hoi doan chi thuoc xu A");

        var clientA = app.CreateAuthClient(app.GiaoXuId);
        var res = await clientA.GetAsync("/api/hoi-doan/danh-sach");
        var than = await res.Content.ReadAsStringAsync();

        than.Should().Contain("Hoi doan chi thuoc xu A");
        than.Should().NotContain("Hoi doan chi thuoc xu B");
    }
}
