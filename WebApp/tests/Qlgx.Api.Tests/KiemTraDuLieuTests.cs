using System.Net.Http.Json;
using FluentAssertions;
using Qlgx.Domain;
using Qlgx.Domain.Entities;

namespace Qlgx.Api.Tests;

/// <summary>
/// "Kiểm tra dữ liệu — giáo dân" (docs/superpowers/specs/man-hinh/cong-cu-du-lieu.md mục 2).
/// Mỗi test khoá đúng MỘT quy tắc trong 6 quy tắc của ReviewGiaoDanProcess.cs, tắt 5 quy tắc
/// còn lại qua tham số truy vấn để không lẫn kết quả — vì mọi giáo dân mới tạo (chỉ có HoTen)
/// đều thoả "Không có dữ liệu ngày tháng" (KetQua mặc định).
/// </summary>
public class KiemTraDuLieuTests(QlgxApiFactory app) : IClassFixture<QlgxApiFactory>
{
    private sealed record Ket(GiaoDanTom GiaoDan, string NguyenNhan, int KetQua);
    private sealed record GiaoDanTom(Guid Id, int MaGiaoDanCu, string HoTen);

    // Tắt cả 6 cờ rồi chỉ bật lại cờ cần test — tránh giáo dân "chỉ có HoTen" của các test khác
    // (tạo trong cùng CSDL fixture) lẫn vào vì luôn khớp "Không có dữ liệu ngày tháng".
    private const string TatCaCoFalse =
        "&khongCoNgayThang=false&saiQuanHeNgayThang=false&ruocLeTruocTuoi=false" +
        "&thuocNhieuGiaDinh=false&khongThuocGiaDinhNao=false&coNhieuHonPhoi=false";

    private async Task<List<Ket>> Goi(string co)
    {
        var client = app.CreateAuthClient();
        var res = await client.GetAsync(
            $"/api/cong-cu-du-lieu/kiem-tra-giao-dan?{TatCaCoFalse.Replace($"&{co}=false", "")}&{co}=true");
        var noiDung = await res.Content.ReadAsStringAsync();
        if (!res.IsSuccessStatusCode) throw new Exception($"{res.StatusCode}: {noiDung}");
        return System.Text.Json.JsonSerializer.Deserialize<List<Ket>>(
            noiDung, new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web))!;
    }

    [Fact]
    public async Task Khong_co_ngay_thang_duoc_bao_dung_ly_do()
    {
        await using var db = app.TaoContextThuan();
        var gd = new GiaoDan { GiaoXuId = app.GiaoXuId, MaGiaoDanCu = 91001, HoTen = "Khong Ngay Thang" };
        db.GiaoDan.Add(gd);
        await db.SaveChangesAsync();

        var ds = await Goi("khongCoNgayThang");

        var dong = ds.Single(x => x.GiaoDan.MaGiaoDanCu == 91001);
        dong.NguyenNhan.Should().Be("- Không có dữ liệu ngày tháng");
        dong.KetQua.Should().Be(4); // ReviewGiaoDanType.KhongCoDuLieuNgayThang
    }

    [Fact]
    public async Task Ngay_sinh_sau_ngay_rua_toi_bi_bao_sai_quan_he()
    {
        await using var db = app.TaoContextThuan();
        var gd = new GiaoDan
        {
            GiaoXuId = app.GiaoXuId, MaGiaoDanCu = 91002, HoTen = "Sai Quan He",
            NgaySinh = new DateOnly(2000, 1, 1), NgayRuaToi = new DateOnly(1999, 1, 1),
        };
        db.GiaoDan.Add(gd);
        await db.SaveChangesAsync();

        var ds = await Goi("saiQuanHeNgayThang");

        var dong = ds.Single(x => x.GiaoDan.MaGiaoDanCu == 91002);
        dong.NguyenNhan.Should().Be("- Sai quan hệ ngày tháng");
        dong.KetQua.Should().Be(2);
    }

    /// <summary>
    /// TÁI HIỆN BUG isValidDateInputRelations (frmGiaoDan.cs:467-479, xem spec mục 2.3 #2):
    /// chỉ so NgaySinh với NgayRuaToi — NgayThemSuc trước NgayRuocLe KHÔNG bị báo, dù nhãn ô
    /// tick nói "Sai quan hệ ngày tháng" một cách tổng quát. Test này khoá đúng hành vi lỗi,
    /// không phải hành vi "đúng" mà một người đọc nhãn ô tick sẽ kỳ vọng.
    /// </summary>
    [Fact]
    public async Task Ngay_them_suc_truoc_ngay_ruoc_le_KHONG_bi_bao_loi_dung_bug_goc()
    {
        await using var db = app.TaoContextThuan();
        var gd = new GiaoDan
        {
            GiaoXuId = app.GiaoXuId, MaGiaoDanCu = 91003, HoTen = "Bug Goc",
            NgaySinh = new DateOnly(1990, 1, 1), NgayRuaToi = new DateOnly(1990, 2, 1),
            NgayRuocLe = new DateOnly(2000, 1, 1), NgayThemSuc = new DateOnly(1995, 1, 1),
        };
        db.GiaoDan.Add(gd);
        await db.SaveChangesAsync();

        var ds = await Goi("saiQuanHeNgayThang");

        ds.Should().NotContain(x => x.GiaoDan.MaGiaoDanCu == 91003);
    }

    [Fact]
    public async Task Ruoc_le_truoc_7_tuoi_bi_bao()
    {
        await using var db = app.TaoContextThuan();
        var gd = new GiaoDan
        {
            GiaoXuId = app.GiaoXuId, MaGiaoDanCu = 91004, HoTen = "Ruoc Le Som",
            NgaySinh = new DateOnly(2000, 1, 1), NgayRuocLe = new DateOnly(2005, 1, 1), // 5 < 7
        };
        db.GiaoDan.Add(gd);
        await db.SaveChangesAsync();

        var ds = await Goi("ruocLeTruocTuoi");

        var dong = ds.Single(x => x.GiaoDan.MaGiaoDanCu == 91004);
        dong.NguyenNhan.Should().Be("- Rước lễ trước 7 tuổi");
        dong.KetQua.Should().Be(1);
    }

    [Fact]
    public async Task Ruoc_le_vua_du_7_tuoi_KHONG_bi_bao()
    {
        await using var db = app.TaoContextThuan();
        var gd = new GiaoDan
        {
            GiaoXuId = app.GiaoXuId, MaGiaoDanCu = 91005, HoTen = "Ruoc Le Du Tuoi",
            NgaySinh = new DateOnly(2000, 1, 1), NgayRuocLe = new DateOnly(2007, 1, 1), // dung 7
        };
        db.GiaoDan.Add(gd);
        await db.SaveChangesAsync();

        var ds = await Goi("ruocLeTruocTuoi");

        ds.Should().NotContain(x => x.GiaoDan.MaGiaoDanCu == 91005);
    }

    [Fact]
    public async Task Khong_thuoc_gia_dinh_nao_bi_bao()
    {
        await using var db = app.TaoContextThuan();
        var gd = new GiaoDan { GiaoXuId = app.GiaoXuId, MaGiaoDanCu = 91006, HoTen = "Mo Coi" };
        db.GiaoDan.Add(gd);
        await db.SaveChangesAsync();

        var ds = await Goi("khongThuocGiaDinhNao");

        var dong = ds.Single(x => x.GiaoDan.MaGiaoDanCu == 91006);
        dong.NguyenNhan.Should().Be("- Không thuộc gia đình nào");
        dong.KetQua.Should().Be(16);
    }

    [Fact]
    public async Task Thuoc_hai_gia_dinh_khac_nhau_bi_bao_thuoc_nhieu_gia_dinh()
    {
        await using var db = app.TaoContextThuan();
        var gd = new GiaoDan { GiaoXuId = app.GiaoXuId, MaGiaoDanCu = 91007, HoTen = "Hai Nha" };
        db.GiaoDan.Add(gd);
        var gd1 = new GiaoDan { GiaoXuId = app.GiaoXuId, MaGiaoDanCu = 91008, HoTen = "Chong 1" };
        var gd2 = new GiaoDan { GiaoXuId = app.GiaoXuId, MaGiaoDanCu = 91009, HoTen = "Chong 2" };
        db.GiaoDan.AddRange(gd1, gd2);
        var gdinh1 = new GiaDinh { GiaoXuId = app.GiaoXuId, MaGiaDinhCu = 91007, TenGiaDinh = "GD 1" };
        var gdinh2 = new GiaDinh { GiaoXuId = app.GiaoXuId, MaGiaDinhCu = 91008, TenGiaDinh = "GD 2" };
        db.GiaDinh.AddRange(gdinh1, gdinh2);
        await db.SaveChangesAsync();
        db.ThanhVienGiaDinh.AddRange(
            new ThanhVienGiaDinh { GiaoXuId = app.GiaoXuId, GiaDinhId = gdinh1.Id, GiaoDanId = gd.Id, VaiTro = VaiTroGiaDinh.Con },
            new ThanhVienGiaDinh { GiaoXuId = app.GiaoXuId, GiaDinhId = gdinh1.Id, GiaoDanId = gd1.Id, VaiTro = VaiTroGiaDinh.Chong },
            new ThanhVienGiaDinh { GiaoXuId = app.GiaoXuId, GiaDinhId = gdinh2.Id, GiaoDanId = gd.Id, VaiTro = VaiTroGiaDinh.Con },
            new ThanhVienGiaDinh { GiaoXuId = app.GiaoXuId, GiaDinhId = gdinh2.Id, GiaoDanId = gd2.Id, VaiTro = VaiTroGiaDinh.Chong });
        await db.SaveChangesAsync();

        var ds = await Goi("thuocNhieuGiaDinh");

        var dong = ds.Single(x => x.GiaoDan.MaGiaoDanCu == 91007);
        dong.NguyenNhan.Should().Be("- Thuộc nhiều gia đình");
        dong.KetQua.Should().Be(8);
        // Chồng 1/Chồng 2 chỉ thuộc đúng 1 gia đình mỗi người — không bị báo.
        ds.Should().NotContain(x => x.GiaoDan.MaGiaoDanCu == 91008 || x.GiaoDan.MaGiaoDanCu == 91009);
    }

    [Fact]
    public async Task Co_hai_hon_phoi_phan_biet_bi_bao_nhieu_hon_phoi()
    {
        await using var db = app.TaoContextThuan();
        var gd = new GiaoDan { GiaoXuId = app.GiaoXuId, MaGiaoDanCu = 91010, HoTen = "Nhieu Hon Phoi" };
        db.GiaoDan.Add(gd);
        var hp1 = new HonPhoi { GiaoXuId = app.GiaoXuId, MaHonPhoiCu = 91010, TenHonPhoi = "HP 1" };
        var hp2 = new HonPhoi { GiaoXuId = app.GiaoXuId, MaHonPhoiCu = 91011, TenHonPhoi = "HP 2" };
        db.HonPhoi.AddRange(hp1, hp2);
        await db.SaveChangesAsync();
        db.GiaoDanHonPhoi.AddRange(
            new GiaoDanHonPhoi { GiaoXuId = app.GiaoXuId, GiaoDanId = gd.Id, HonPhoiId = hp1.Id, SoThuTu = 1 },
            new GiaoDanHonPhoi { GiaoXuId = app.GiaoXuId, GiaoDanId = gd.Id, HonPhoiId = hp2.Id, SoThuTu = 1 });
        await db.SaveChangesAsync();

        var ds = await Goi("coNhieuHonPhoi");

        var dong = ds.Single(x => x.GiaoDan.MaGiaoDanCu == 91010);
        dong.NguyenNhan.Should().Be("- Có nhiều thông tin hôn phối");
        dong.KetQua.Should().Be(32);
    }

    [Fact]
    public async Task Loc_theo_giao_ho_chi_tra_giao_dan_cua_giao_ho_do()
    {
        await using var db = app.TaoContextThuan();
        var gh = new GiaoHo { GiaoXuId = app.GiaoXuId, MaGiaoHoCu = 91020, TenGiaoHo = "Giao Ho Test" };
        db.GiaoHo.Add(gh);
        await db.SaveChangesAsync();
        var trongGh = new GiaoDan { GiaoXuId = app.GiaoXuId, MaGiaoDanCu = 91021, HoTen = "Trong Ho", GiaoHoId = gh.Id };
        var ngoaiGh = new GiaoDan { GiaoXuId = app.GiaoXuId, MaGiaoDanCu = 91022, HoTen = "Ngoai Ho" };
        db.GiaoDan.AddRange(trongGh, ngoaiGh);
        await db.SaveChangesAsync();

        var client = app.CreateAuthClient();
        var ds = await client.GetFromJsonAsync<List<Ket>>(
            $"/api/cong-cu-du-lieu/kiem-tra-giao-dan?giaoHoId={gh.Id}{TatCaCoFalse.Replace("&khongThuocGiaDinhNao=false", "")}&khongThuocGiaDinhNao=true");

        ds!.Should().Contain(x => x.GiaoDan.MaGiaoDanCu == 91021);
        ds.Should().NotContain(x => x.GiaoDan.MaGiaoDanCu == 91022);
    }
}
