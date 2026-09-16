using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Qlgx.Domain;
using Qlgx.Domain.Entities;

namespace Qlgx.Api.Tests;

/// <summary>
/// Màn hình "Thống kê chung" + "Biểu đồ" — xem
/// docs/superpowers/specs/man-hinh/thong-ke-bieu-do.md. Trọng tâm: các công thức migrate Y HỆT
/// bản gốc KỂ CẢ chỗ desktop tính sai (cận tuổi đảo ngược, mục 4.3/4.6) — test này tồn tại chính
/// là để khoá lại hành vi đó, không để nó bị "sửa cho đúng" một cách âm thầm ở lượt sau.
/// </summary>
public class ThongKeTests(QlgxApiFactory app) : IClassFixture<QlgxApiFactory>
{
    private sealed record GiaoDanRow(Guid Id, int MaGiaoDanCu);
    private sealed record GiaDinhRow(Guid Id, int MaGiaDinhCu);
    private sealed record HonPhoiRow(Guid Id, int MaHonPhoiCu, string? TenChong, string? TenVo, string? CachThucHonPhoi);
    private sealed record ThongKeChungKetQua(int TongCong, string Nhan,
        List<GiaoDanRow>? GiaoDan, List<GiaDinhRow>? GiaDinh, List<HonPhoiRow>? HonPhoi);
    private sealed record BieuDoNam(int Nam, int SoLuong);
    private sealed record BieuDoDoTuoi(int Duoi7, int Tu7Den12, int Tu13Den16, int Tu17Den25,
        int Tu26Den30, int Tu31Den50, int Tren50);
    private sealed record BieuDoGiaoHo(string TenGiaoHo, int SoLuong);

    private async Task<Guid> TaoGiaoDan(int ma, string hoTen, DateOnly? ngaySinh = null,
        DateOnly? ngayRuaToi = null, bool quaDoi = false, Guid? giaoHoId = null, string phai = "Nam")
    {
        await using var db = app.TaoContextThuan();
        var gd = new GiaoDan
        {
            GiaoXuId = app.GiaoXuId, MaGiaoDanCu = ma, HoTen = hoTen, Phai = phai,
            NgaySinh = ngaySinh, NgayRuaToi = ngayRuaToi, QuaDoi = quaDoi, GiaoHoId = giaoHoId,
        };
        db.GiaoDan.Add(gd);
        await db.SaveChangesAsync();
        return gd.Id;
    }

    private async Task<Guid> TaoGiaoHo(int ma, string ten)
    {
        await using var db = app.TaoContextThuan();
        var gh = new GiaoHo { GiaoXuId = app.GiaoXuId, MaGiaoHoCu = ma, TenGiaoHo = ten };
        db.GiaoHo.Add(gh);
        await db.SaveChangesAsync();
        return gh.Id;
    }

    // ================= Sinh ra / Rửa tội / Qua đời =================

    [Fact]
    public async Task Sinh_ra_dem_dung_khoang_ngay_va_bo_qua_ngoai_khoang()
    {
        await TaoGiaoDan(20001, "Trong khoang", new DateOnly(2015, 6, 1));
        await TaoGiaoDan(20002, "Ngoai khoang", new DateOnly(2000, 1, 1));

        var res = await app.CreateAuthClient().GetFromJsonAsync<ThongKeChungKetQua>(
            "/api/thong-ke/chung?dieuKien=SinhRa&tuNgay=2015-01-01&denNgay=2015-12-31");

        res!.GiaoDan.Should().ContainSingle(x => x.MaGiaoDanCu == 20001);
        res.Nhan.Should().Be(" người được sinh ra");
    }

    [Fact]
    public async Task Qua_doi_dung_nen_rieng_khong_loai_nguoi_da_qua_doi()
    {
        // Nền riêng của "Qua đời" (GxThongKeChung.cs:223-229): KHÔNG áp QuaDoi=0 như nền chung.
        await TaoGiaoDan(20010, "Da mat", ngaySinh: null, quaDoi: true);
        await using (var db = app.TaoContextThuan())
        {
            var gd = db.GiaoDan.Single(x => x.MaGiaoDanCu == 20010);
            gd.NgayQuaDoi = new DateOnly(2020, 3, 3);
            await db.SaveChangesAsync();
        }

        var res = await app.CreateAuthClient().GetFromJsonAsync<ThongKeChungKetQua>(
            "/api/thong-ke/chung?dieuKien=QuaDoi&tuNgay=2020-01-01&denNgay=2020-12-31");

        res!.GiaoDan.Should().ContainSingle(x => x.MaGiaoDanCu == 20010);
        res.Nhan.Should().Be(" người qua đời");
    }

    [Fact]
    public async Task Tong_so_giao_dan_dem_luy_ke_den_ngay_ghi_de_tu_ngay()
    {
        // GxThongKeChung.cs:241-249: dtDateTo GHI ĐÈ lên dtDateFrom, so sánh "<=" (không BETWEEN).
        await TaoGiaoDan(20020, "Sinh truoc moc", new DateOnly(1990, 1, 1));
        await TaoGiaoDan(20021, "Sinh sau moc", new DateOnly(2030, 1, 1));

        var res = await app.CreateAuthClient().GetFromJsonAsync<ThongKeChungKetQua>(
            "/api/thong-ke/chung?dieuKien=TongSoGiaoDan&tuNgay=1900-01-01&denNgay=2020-12-31");

        res!.GiaoDan.Should().Contain(x => x.MaGiaoDanCu == 20020)
            .And.NotContain(x => x.MaGiaoDanCu == 20021);
    }

    // ================= Bug cận tuổi đảo ngược (Extract.cs, mục 4.3) =================

    [Fact]
    public async Task Gia_truong_voi_khoang_tuoi_that_luon_ra_danh_sach_rong_bug_can_dao_nguoc()
    {
        var nay = DateTime.Now.Year;
        // "Gia trưởng" 20-40 tuổi thật (sinh nay-20 tới nay-40) — Extract.cs:33-37 khiến
        // fromYear=nay-20 (lớn hơn) > toYear=nay-40 (nhỏ hơn) nên BETWEEN luôn rỗng.
        var giaDinh = new GiaDinh { GiaoXuId = app.GiaoXuId, MaGiaDinhCu = 30001, TenGiaDinh = "GD Gia Truong" };
        var chongId = await TaoGiaoDan(20030, "Ong Gia Truong", new DateOnly(nay - 30, 1, 1));
        await using (var db = app.TaoContextThuan())
        {
            db.GiaDinh.Add(giaDinh);
            await db.SaveChangesAsync();
            db.ThanhVienGiaDinh.Add(new ThanhVienGiaDinh
            { GiaoXuId = app.GiaoXuId, GiaDinhId = giaDinh.Id, GiaoDanId = chongId, VaiTro = VaiTroGiaDinh.Chong, ChuHo = true });
            await db.SaveChangesAsync();
        }

        var res = await app.CreateAuthClient().GetFromJsonAsync<ThongKeChungKetQua>(
            "/api/thong-ke/chung?dieuKien=GiaTruong&tuTuoi=20&denTuoi=40");

        res!.GiaoDan.Should().BeEmpty("Extract.cs tính fromYear/toYear đảo ngược khi Từ tuổi < Đến tuổi — bug gốc, migrate y hệt");
    }

    /// <summary>
    /// LỖ HỔNG TEST 1 (review-toan-nhanh-test.md mục 1a) — trước bản sửa này, đột biến hoán đổi
    /// vai trò Chồng ↔ Vợ ở ThongKeService.cs:238-239 không có test nào bắt được: test
    /// <see cref="Gia_truong_voi_khoang_tuoi_that_luon_ra_danh_sach_rong_bug_can_dao_nguoc"/> luôn
    /// kỳ vọng danh sách RỖNG (cố ý rơi vào bug cận tuổi đảo ngược để khoá bug gốc lại) nên không
    /// phân biệt được điều kiện lọc VaiTro đúng hay sai — bất kỳ điều kiện nào cũng cho cùng kết
    /// quả rỗng. "Hiền mẫu" không có test nào cả trong toàn bộ 50 commit trước đó.
    ///
    /// Test này NÉ bug cận tuổi đảo ngược bằng cách chọn tuTuoi=40 > denTuoi=20 (fromYear =
    /// nay-40 &lt;= toYear = nay-20 — khoảng NĂM SINH hợp lệ, không rỗng do BETWEEN đảo chiều),
    /// dựng MỘT gia đình có cả Chồng lẫn Vợ cùng nằm trong khoảng năm sinh đó, rồi khẳng định
    /// GiaTruong CHỈ trả về người Chồng và HienMau CHỈ trả về người Vợ — phân biệt thật được hai
    /// vai trò, độc lập với bug tuổi (không sửa bug đó — xem can-review-sau.md mục 59).
    ///
    /// BẰNG CHỨNG ĐỎ→XANH (task-sua-review-backend-2.md): hoán đổi VaiTro.Vo ↔ VaiTro.Chong ở
    /// ThongKeService.cs:238-239 (đúng đột biến của review-toan-nhanh-test.md) làm 2 assert dưới
    /// đây ĐỎ (GiaTruong trả về Vợ thay vì Chồng, HienMau trả về Chồng thay vì Vợ) — đã chạy thật
    /// để xác nhận, rồi khôi phục nguyên văn.
    /// </summary>
    [Fact]
    public async Task Gia_truong_chi_ra_chong_va_hien_mau_chi_ra_vo_khong_lan_vai_tro()
    {
        var nay = DateTime.Now.Year;
        // tuTuoi=40, denTuoi=20 (Từ tuổi > Đến tuổi) -> fromYear=nay-40 <= toYear=nay-20, tránh
        // đúng bug cận tuổi đảo ngược của mục 4.3 (khác test bên trên cố tình rơi vào bug đó).
        var giaDinh = new GiaDinh { GiaoXuId = app.GiaoXuId, MaGiaDinhCu = 30002, TenGiaDinh = "GD Chong Vo" };
        var chongId = await TaoGiaoDan(20031, "Ong Chong That", new DateOnly(nay - 30, 1, 1), phai: "Nam");
        var voId = await TaoGiaoDan(20032, "Ba Vo That", new DateOnly(nay - 28, 1, 1), phai: "Nữ");
        await using (var db = app.TaoContextThuan())
        {
            db.GiaDinh.Add(giaDinh);
            await db.SaveChangesAsync();
            db.ThanhVienGiaDinh.AddRange(
                new ThanhVienGiaDinh { GiaoXuId = app.GiaoXuId, GiaDinhId = giaDinh.Id, GiaoDanId = chongId, VaiTro = VaiTroGiaDinh.Chong, ChuHo = true },
                new ThanhVienGiaDinh { GiaoXuId = app.GiaoXuId, GiaDinhId = giaDinh.Id, GiaoDanId = voId, VaiTro = VaiTroGiaDinh.Vo, ChuHo = false });
            await db.SaveChangesAsync();
        }

        var client = app.CreateAuthClient();
        var giaTruong = await client.GetFromJsonAsync<ThongKeChungKetQua>(
            "/api/thong-ke/chung?dieuKien=GiaTruong&tuTuoi=40&denTuoi=20");
        var hienMau = await client.GetFromJsonAsync<ThongKeChungKetQua>(
            "/api/thong-ke/chung?dieuKien=HienMau&tuTuoi=40&denTuoi=20");

        giaTruong!.GiaoDan.Should().Contain(x => x.MaGiaoDanCu == 20031, "gia truong phai la CHONG")
            .And.NotContain(x => x.MaGiaoDanCu == 20032, "gia truong KHONG duoc lan sang vo");
        giaTruong.Nhan.Should().Be(" gia trưởng");

        hienMau!.GiaoDan.Should().Contain(x => x.MaGiaoDanCu == 20032, "hien mau phai la VO")
            .And.NotContain(x => x.MaGiaoDanCu == 20031, "hien mau KHONG duoc lan sang chong");
        hienMau.Nhan.Should().Be(" hiền mẫu");
    }

    [Fact]
    public async Task Gioi_tre_18_30_tuoi_luon_ra_danh_sach_rong_bug_can_dao_nguoc()
    {
        var nay = DateTime.Now.Year;
        await TaoGiaoDan(20040, "Nguoi 25 tuoi", new DateOnly(nay - 25, 1, 1));

        var res = await app.CreateAuthClient().GetFromJsonAsync<ThongKeChungKetQua>(
            "/api/thong-ke/chung?dieuKien=GioiTre&tuTuoi=18&denTuoi=30");

        res!.GiaoDan.Should().BeEmpty();
    }

    [Fact]
    public async Task Cao_nien_khong_bug_vi_dung_can_cung_1_denFromYear()
    {
        var nay = DateTime.Now.Year;
        // Cao niên >= 60 tuổi: Extract.SelectByTuoi dùng cận CỐ ĐỊNH (1, nay-tuoi) — không qua
        // cặp fromYear/toYear thường nên KHÔNG bug, ra kết quả thật.
        await TaoGiaoDan(20050, "Cu gia 70 tuoi", new DateOnly(nay - 70, 1, 1));
        await TaoGiaoDan(20051, "Nguoi 40 tuoi", new DateOnly(nay - 40, 1, 1));

        var res = await app.CreateAuthClient().GetFromJsonAsync<ThongKeChungKetQua>(
            "/api/thong-ke/chung?dieuKien=CaoNien&tuTuoi=60");

        res!.GiaoDan.Should().Contain(x => x.MaGiaoDanCu == 20050)
            .And.NotContain(x => x.MaGiaoDanCu == 20051);
    }

    // ================= Tổng số gia đình =================

    [Fact]
    public async Task Tong_so_gia_dinh_khong_loc_ngay_thang_nao()
    {
        await using var db = app.TaoContextThuan();
        db.GiaDinh.Add(new GiaDinh { GiaoXuId = app.GiaoXuId, MaGiaDinhCu = 30010, TenGiaDinh = "GD A" });
        await db.SaveChangesAsync();

        var res = await app.CreateAuthClient().GetFromJsonAsync<ThongKeChungKetQua>(
            "/api/thong-ke/chung?dieuKien=TongSoGiaDinh");

        res!.GiaDinh.Should().Contain(x => x.MaGiaDinhCu == 30010);
        res.Nhan.Should().Be(" gia đình");
    }

    // ================= Hôn phối =================

    [Fact]
    public async Task Hon_phoi_dem_dung_khoang_ngay_va_ghep_ten_chong_vo()
    {
        Guid honPhoiId;
        await using (var db = app.TaoContextThuan())
        {
            var chong = new GiaoDan { GiaoXuId = app.GiaoXuId, MaGiaoDanCu = 40001, HoTen = "Nguyen Van A", Phai = "Nam" };
            var vo = new GiaoDan { GiaoXuId = app.GiaoXuId, MaGiaoDanCu = 40002, HoTen = "Tran Thi B", Phai = "Nữ" };
            var hp = new HonPhoi { GiaoXuId = app.GiaoXuId, MaHonPhoiCu = 5001, NgayHonPhoi = new DateOnly(2018, 5, 5) };
            db.GiaoDan.AddRange(chong, vo);
            db.HonPhoi.Add(hp);
            await db.SaveChangesAsync();
            db.GiaoDanHonPhoi.AddRange(
                new GiaoDanHonPhoi { GiaoXuId = app.GiaoXuId, GiaoDanId = chong.Id, HonPhoiId = hp.Id, SoThuTu = 1 },
                new GiaoDanHonPhoi { GiaoXuId = app.GiaoXuId, GiaoDanId = vo.Id, HonPhoiId = hp.Id, SoThuTu = 1 });
            await db.SaveChangesAsync();
            honPhoiId = hp.Id;
        }

        var res = await app.CreateAuthClient().GetFromJsonAsync<ThongKeChungKetQua>(
            "/api/thong-ke/chung?dieuKien=HonPhoi&tuNgay=2018-01-01&denNgay=2018-12-31&trangThaiHonPhoi=KhongPhanLoai");

        res!.HonPhoi.Should().ContainSingle(x => x.Id == honPhoiId);
        var dong = res.HonPhoi!.Single(x => x.Id == honPhoiId);
        dong.TenChong.Should().Be("Nguyen Van A");
        dong.TenVo.Should().Be("Tran Thi B");
    }

    // ================= Biểu đồ =================

    [Fact]
    public async Task Bieu_do_tong_giao_dan_luy_ke_khong_giam_qua_cac_nam()
    {
        // Lớp test này CHIA SẺ một CSDL giữa các [Fact] (IClassFixture, không reset giữa từng
        // test) — so sánh CHÊNH LỆCH quanh mốc 2010/2012 thay vì số tuyệt đối ở 2009, để không
        // phụ thuộc dữ liệu do các test khác trong lớp tạo ra.
        await TaoGiaoDan(50001, "A", new DateOnly(2010, 1, 1));
        await TaoGiaoDan(50002, "B", new DateOnly(2012, 1, 1));

        var res = await app.CreateAuthClient().GetFromJsonAsync<List<BieuDoNam>>(
            "/api/thong-ke/bieu-do/tong-giao-dan?tuNam=2009&denNam=2015");

        var theoNam = res!.ToDictionary(x => x.Nam, x => x.SoLuong);
        theoNam[2010].Should().BeGreaterThanOrEqualTo(theoNam[2009] + 1, "vừa thêm một người sinh 2010");
        theoNam[2012].Should().BeGreaterThanOrEqualTo(theoNam[2010] + 1, "vừa thêm một người sinh 2012");
        theoNam[2015].Should().BeGreaterThanOrEqualTo(theoNam[2012], "luỹ kế không bao giờ giảm theo năm");
        theoNam[2009].Should().BeLessThanOrEqualTo(theoNam[2010], "luỹ kế không giảm theo năm");
    }

    [Fact]
    public async Task Bieu_do_do_tuoi_nhom_tren_50_luon_bang_0_bug_can_co_dinh_1990()
    {
        var nay = DateTime.Now.Year;
        // Người 70 tuổi thật (sinh nay-70) LẼ RA thuộc nhóm "Trên 50" nhưng bucket dùng cận cố
        // định fromYear=1990 nên chỉ đếm NgaySinh trong [1990, nay-51] — với nay<2041 khoảng này
        // rỗng (1990 > nay-51) nên KHÔNG đếm được ai, kể cả người 70 tuổi vừa tạo.
        await TaoGiaoDan(50010, "Cu gia 70 tuoi that", new DateOnly(nay - 70, 1, 1));

        var res = await app.CreateAuthClient().GetFromJsonAsync<BieuDoDoTuoi>("/api/thong-ke/bieu-do/do-tuoi");

        res!.Tren50.Should().Be(0, "cận cố định 1990 > nay-51 với mọi năm trước 2041 — bug gốc migrate y hệt");
    }

    [Fact]
    public async Task Bieu_do_do_tuoi_nhom_duoi_7_dem_dung()
    {
        var nay = DateTime.Now.Year;
        await TaoGiaoDan(50020, "Be 3 tuoi", new DateOnly(nay - 3, 1, 1));

        var res = await app.CreateAuthClient().GetFromJsonAsync<BieuDoDoTuoi>("/api/thong-ke/bieu-do/do-tuoi");

        res!.Duoi7.Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task Bieu_do_giao_ho_khong_loc_qua_doi()
    {
        // exportGiaoHo (frmBieuDo.cs:353-378) KHÔNG lọc QuaDoi — khác 4 biểu đồ còn lại.
        var ghId = await TaoGiaoHo(60001, "Giao ho Test");
        await TaoGiaoDan(50030, "Da mat nhung van tinh", new DateOnly(1950, 1, 1), quaDoi: true, giaoHoId: ghId);

        var res = await app.CreateAuthClient().GetFromJsonAsync<List<BieuDoGiaoHo>>("/api/thong-ke/bieu-do/giao-ho");

        res!.Single(x => x.TenGiaoHo == "Giao ho Test").SoLuong.Should().Be(1);
    }

    [Fact]
    public async Task Bieu_do_tong_hon_phoi_dem_theo_tung_nam_khong_luy_ke()
    {
        await using (var db = app.TaoContextThuan())
        {
            db.HonPhoi.Add(new HonPhoi { GiaoXuId = app.GiaoXuId, MaHonPhoiCu = 5010, NgayHonPhoi = new DateOnly(2019, 1, 1) });
            db.HonPhoi.Add(new HonPhoi { GiaoXuId = app.GiaoXuId, MaHonPhoiCu = 5011, NgayHonPhoi = new DateOnly(2019, 6, 1) });
            db.HonPhoi.Add(new HonPhoi { GiaoXuId = app.GiaoXuId, MaHonPhoiCu = 5012, NgayHonPhoi = new DateOnly(2020, 1, 1) });
            await db.SaveChangesAsync();
        }

        var res = await app.CreateAuthClient().GetFromJsonAsync<List<BieuDoNam>>(
            "/api/thong-ke/bieu-do/tong-hon-phoi?tuNam=2019&denNam=2020");

        var theoNam = res!.ToDictionary(x => x.Nam, x => x.SoLuong);
        theoNam[2019].Should().BeGreaterThanOrEqualTo(2);
        theoNam[2020].Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task Thieu_dieu_kien_tra_ve_400()
    {
        var res = await app.CreateAuthClient().GetAsync(
            "/api/thong-ke/chung?dieuKien=GiaTruong"); // thiếu tuTuoi/denTuoi

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
