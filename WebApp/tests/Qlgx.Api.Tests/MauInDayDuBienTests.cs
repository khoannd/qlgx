using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Qlgx.Api.Dtos;
using Qlgx.Api.Printing;
using Qlgx.Domain;
using Qlgx.Domain.Entities;

namespace Qlgx.Api.Tests;

/// <summary>
/// BẰNG CHỨNG TỰ ĐỘNG rằng <c>InAnService</c> cấp giá trị cho ĐỦ MỌI biến mà
/// <see cref="MauInCatalog"/> công bố trong <c>BienKhaDung</c>: với TỪNG mẫu, bài test dựng một
/// mẫu tuỳ chỉnh riêng của giáo xứ chứa TẤT CẢ <c>{{Key}}</c> của mẫu đó (lưu qua chính endpoint
/// <c>PUT /api/mau-in/{tenMau}/rieng</c> — đi qua đủ khử trùng HTML/RowVersion như người dùng
/// thật), rồi gọi ĐÚNG endpoint in thật và khẳng định kết quả KHÔNG còn chuỗi "{{" nào sót.
///
/// Nếu ai đó thêm một biến vào danh mục mà quên gán giá trị trong InAnService, bài test này ĐỎ —
/// đúng điều cần tránh: combobox "Chèn chỗ trống" mời quý cha/quý sơ chèn một biến rồi in ra tờ
/// giấy có chữ "{{NgayXucDau}}" nằm giữa trang.
///
/// VÌ SAO KIỂM Ở TẦNG HTML, KHÔNG PHẢI PDF: PDF là nhị phân (đã nén FlateDecode) nên không thể
/// tìm chuỗi "{{" trong đó một cách đáng tin. Chỗ hẹp nhất để chặn lấy HTML cuối cùng là
/// <see cref="BoTrinhDuyet.XuatPdfAsync"/> — MỌI mẫu của mọi màn hình đều đi qua đúng hàm đó.
/// Bài test thay <see cref="BoTrinhDuyet"/> trong DI bằng một lớp con ghi lại HTML (xem ghi chú
/// lý do chọn cách này thay vì mở public API InAnService, ở đầu BoTrinhDuyet.cs). Tác dụng phụ
/// tốt: không phải khởi động Chromium nên bài test chạy rất nhanh dù in đủ 13 mẫu.
/// </summary>
public class MauInDayDuBienTests(QlgxApiFactory app) : IClassFixture<QlgxApiFactory>
{
    /// <summary>Lớp con CHỈ dùng trong test: ghi lại chuỗi HTML rồi trả về một tệp PDF giả (chữ
    /// ký "%PDF-" để phía endpoint/HTTP vẫn là một phản hồi bình thường). Không gọi Playwright.</summary>
    private sealed class BoTrinhDuyetGhiHtml : BoTrinhDuyet
    {
        public List<string> DaVe { get; } = [];

        public override Task<byte[]> XuatPdfAsync(
            string html, CancellationToken ct, bool landscape = false, string khoGiay = "A4")
        {
            lock (DaVe) DaVe.Add(html);
            return Task.FromResult(System.Text.Encoding.ASCII.GetBytes("%PDF-gia-lap"));
        }
    }

    /// <summary>Lớp con CHỈ dùng trong test: ghi lại TẬP KHOÁ mà InAnService trao vào cho mỗi
    /// lượt dựng mẫu. BẮT BUỘC phải có bên cạnh việc soi HTML: <c>BoDoMauIn.ApDung</c> XOÁ TRẮNG
    /// mọi <c>{{Key}}</c> không có trong dictionary, nên một biến bị QUÊN HẲN cũng biến mất khỏi
    /// HTML y như một biến có giá trị rỗng hợp lệ — nhìn HTML không phân biệt được. Chỉ tập khoá
    /// mới trả lời đúng câu hỏi "InAnService có gán giá trị cho biến này không".</summary>
    private sealed class BoDoMauInGhiKhoa : BoDoMauIn
    {
        public List<HashSet<string>> KhoaDaTrao { get; } = [];

        public override string ApDung(string mauHtml, IReadOnlyDictionary<string, string?> duLieu,
            IReadOnlyDictionary<string, string?>? khoiHtmlAnToan = null)
        {
            var khoa = new HashSet<string>(duLieu.Keys, StringComparer.Ordinal);
            if (khoiHtmlAnToan is not null) khoa.UnionWith(khoiHtmlAnToan.Keys);
            lock (KhoaDaTrao) KhoaDaTrao.Add(khoa);
            return base.ApDung(mauHtml, duLieu, khoiHtmlAnToan);
        }
    }

    private sealed record NguCanh(Guid GiaoDanNam, Guid GiaoDanNu, Guid GiaDinh, Guid RaoHonPhoi);

    /// <summary>Mỗi lần gọi <see cref="DungDuLieu"/> lấy một dải mã cũ riêng — hai bài test
    /// trong lớp này đều dựng dữ liệu trên CÙNG một giáo xứ, dùng chung mã sẽ đụng chỉ mục
    /// duy nhất.</summary>
    private static int _daiMa;

    /// <summary>Dựng một bộ dữ liệu ĐỦ để cả 13 mẫu đều in ra được (nhiều mẫu trả 404 khi thiếu
    /// dữ liệu: "Chứng nhận hôn phối" cần một hôn phối thật, "Xin điều tra và rao hôn phối" cần
    /// một đôi rao…). Điền giá trị cho nhiều cột nhất có thể — bài test vẫn đúng khi cột rỗng
    /// (chỗ trống được thay bằng chuỗi rỗng), nhưng dữ liệu thật làm HTML dựng ra giống bản in
    /// thật hơn.</summary>
    private async Task<NguCanh> DungDuLieu()
    {
        var goc = 7000 + Interlocked.Increment(ref _daiMa) * 100;
        await using var db = app.TaoContextThuan();

        var giaoHo = new GiaoHo { GiaoXuId = app.GiaoXuId, MaGiaoHoCu = goc + 1, TenGiaoHo = "Giáo họ Đủ Biến" };
        db.GiaoHo.Add(giaoHo);

        GiaoDan TaoNguoi(int ma, string hoTen, string phai) => new()
        {
            GiaoXuId = app.GiaoXuId, MaGiaoDanCu = ma, HoTen = hoTen, TenThanh = "Maria", Phai = phai,
            NgaySinh = new DateOnly(1990, 3, 4), NoiSinh = "Phan Thiết", CMND = "0601234567",
            DanToc = "Kinh", ThuocGiaoXu = "Vô Nhiễm", ThuocGiaoPhan = "Phan Thiết",
            DiaChi = "12 Trần Hưng Đạo", DienThoai = "0900000001", Email = "a@b.c",
            HoTenCha = "Nguyen Van Cha", HoTenMe = "Tran Thi Me", GhiChu = "Ghi chú giáo dân",
            NgheNghiep = "Giáo viên", TrinhDoVanHoa = "12/12", TrinhDoChuyenMon = "Cử nhân",
            BietNgoaiNgu = "Anh", GiaoHo = giaoHo,
            SoRuaToi = "12/1990", NgayRuaToi = new DateOnly(1990, 4, 1), NoiRuaToi = "GX Vô Nhiễm",
            ChaRuaToi = "Gioan", NguoiDoDauRuaToi = "Anna",
            SoRuocLe = "5/1998", NgayRuocLe = new DateOnly(1998, 5, 1), NoiRuocLe = "GX Vô Nhiễm",
            ChaRuocLe = "Phaolo",
            SoThemSuc = "8/2002", NgayThemSuc = new DateOnly(2002, 6, 1), NoiThemSuc = "GX Vô Nhiễm",
            ChaThemSuc = "Giuse", NguoiDoDauThemSuc = "Teresa",
            NgayXucDau = new DateOnly(2024, 1, 2), NguoiXucDau = "Cha Phero",
            TinhTrangXucDau = "Ổn định", GhiChuXucDau = "Ghi chú xức dầu",
            NgayBD1 = new DateOnly(2000, 1, 1), NoiBD1 = "GX A",
            NgayBD2 = new DateOnly(2001, 1, 1), NoiBD2 = "GX B",
            NgayTHVaoDoi = new DateOnly(2003, 1, 1), NoiTHVaoDoi = "GX C",
            NgayGLHN1 = new DateOnly(2015, 1, 1), NgayGLHN2 = new DateOnly(2015, 6, 1),
            NoiGLHN = "GX D", NguoiChungNhanGLHN = "Cha Giuse", XepLoaiGLHN = "Giỏi",
            NoiQuaDoi = "", SoAnTang = "", NoiAnTang = "",
        };

        var nam = TaoNguoi(goc + 11, "Nguyen Van Nam", "Nam");
        var nu = TaoNguoi(goc + 12, "Tran Thi Nu", "Nữ");
        db.GiaoDan.AddRange(nam, nu);

        var giaDinh = new GiaDinh
        {
            GiaoXuId = app.GiaoXuId, MaGiaDinhCu = goc + 21, TenGiaDinh = "Gia đình Đủ Biến",
            GiaoHo = giaoHo, DiaChi = "12 Trần Hưng Đạo", DienThoai = "0900000009",
            SoHoKhau = "HK-123", DienGiaDinh = "Thường", GhiChu = "Ghi chú gia đình",
            DaChuyenXu = true, NgayChuyen = new DateOnly(2023, 7, 7), NoiChuyen = "GX Nơi Đến",
        };
        db.GiaDinh.Add(giaDinh);
        db.ThanhVienGiaDinh.AddRange(
            new ThanhVienGiaDinh { GiaoXuId = app.GiaoXuId, GiaDinh = giaDinh, GiaoDan = nam, VaiTro = VaiTroGiaDinh.Chong, ChuHo = true },
            new ThanhVienGiaDinh { GiaoXuId = app.GiaoXuId, GiaDinh = giaDinh, GiaoDan = nu, VaiTro = VaiTroGiaDinh.Vo });

        var honPhoi = new HonPhoi
        {
            GiaoXuId = app.GiaoXuId, MaHonPhoiCu = goc + 31, TenHonPhoi = "Nam - Nữ",
            SoHonPhoi = "31/2016", NgayHonPhoi = new DateOnly(2016, 2, 2), NoiHonPhoi = "GX Vô Nhiễm",
            LinhMucChung = "Cha Gioan", NguoiChung1 = "Ong A", NguoiChung2 = "Ba B",
            CachThucHonPhoi = "Trong lễ", GhiChu = "Ghi chú hôn phối",
        };
        db.HonPhoi.Add(honPhoi);
        db.GiaoDanHonPhoi.AddRange(
            new GiaoDanHonPhoi { GiaoXuId = app.GiaoXuId, HonPhoi = honPhoi, GiaoDan = nam, SoThuTu = 1 },
            new GiaoDanHonPhoi { GiaoXuId = app.GiaoXuId, HonPhoi = honPhoi, GiaoDan = nu, SoThuTu = 2 });

        var rao = new RaoHonPhoi
        {
            GiaoXuId = app.GiaoXuId, MaRaoHonPhoiCu = goc + 41, TenRaoHonPhoi = "Đôi rao Đủ Biến",
            GiaoDan1 = nam, GiaoDan2 = nu,
            NgayRaoLan1 = new DateOnly(2016, 1, 3), NgayRaoLan2 = new DateOnly(2016, 1, 10),
            NgayRaoLan3 = new DateOnly(2016, 1, 17),
            GiaoXu1 = "GX 1", GiaoPhan1 = "GP 1", GiaoXuTruoc1 = "GX Truoc 1", GiaoPhanTruoc1 = "GP Truoc 1",
            GiaoXu2 = "GX 2", GiaoPhan2 = "GP 2", GiaoXuTruoc2 = "GX Truoc 2", GiaoPhanTruoc2 = "GP Truoc 2",
            GiaoXuNQ1 = "GX NQ 1", GiaoPhanNQ1 = "GP NQ 1", GiaoXuNQ2 = "GX NQ 2", GiaoPhanNQ2 = "GP NQ 2",
            LinhMucNhan = "Cha Nhan", GiaoXuNhan = "GX Nhan", GhiChu = "Ghi chú rao",
        };
        db.RaoHonPhoi.Add(rao);

        await db.SaveChangesAsync();
        return new NguCanh(nam.Id, nu.Id, giaDinh.Id, rao.Id);
    }

    /// <summary>Đường dẫn endpoint in THẬT ứng với từng TenMau của danh mục. Cố ý viết tay (không
    /// suy ra tự động) — đây chính là chỗ bài test phải biết chắc "mẫu này được dùng ở màn hình
    /// nào"; bài test cuối cùng khẳng định danh sách này phủ ĐỦ 13 mẫu, nên thêm mẫu mới mà quên
    /// khai ở đây cũng đỏ.</summary>
    private static Dictionary<string, string> DuongDanIn(NguCanh nc)
    {
        const string benNhan = "giaoPhan2=GP%20Nhan&giaoXu2=GX%20Nhan&tenLinhMuc=Cha%20Ky";
        return new Dictionary<string, string>
        {
            ["LyLichCaNhan"] = $"/api/giao-dan/{nc.GiaoDanNam}/in/ly-lich-ca-nhan",
            ["ChungNhanBiTich"] = $"/api/giao-dan/{nc.GiaoDanNam}/in/chung-nhan-bi-tich",
            ["ChungNhanHonPhoi"] = $"/api/gia-dinh/{nc.GiaDinh}/in/chung-nhan-hon-phoi",
            ["PhieuGiaDinh"] = $"/api/gia-dinh/{nc.GiaDinh}/in/phieu-gia-dinh",
            ["GioiThieuRuaToi"] = $"/api/giao-dan/{nc.GiaoDanNam}/in/gioi-thieu-rua-toi?{benNhan}",
            ["GioiThieuThemSuc"] = $"/api/giao-dan/{nc.GiaoDanNam}/in/gioi-thieu-them-suc?{benNhan}",
            ["GioiThieuGiaoLyHonPhoi"] = $"/api/giao-dan/{nc.GiaoDanNam}/in/gioi-thieu-giao-ly-hon-phoi?{benNhan}",
            ["GioiThieuChuyenXu"] = $"/api/gia-dinh/{nc.GiaDinh}/in/gioi-thieu-chuyen-xu?{benNhan}",
            ["RaoHonPhoi"] = $"/api/giao-dan/{nc.GiaoDanNam}/in/gioi-thieu-hon-phoi",
            ["KQRaoHonPhoi"] = $"/api/rao-hon-phoi/{nc.RaoHonPhoi}/in/ket-qua",
            ["DanhSachGiaoDan"] = "/api/giao-dan/in/danh-sach",
            ["DanhSachGiaDinh"] = "/api/gia-dinh/in/danh-sach",
            ["DanhSachRaoHonPhoi"] = "/api/rao-hon-phoi/in/danh-sach",
        };
    }

    /// <summary>Dựng một máy chủ thử có CẢ HAI lớp chặn (ghi HTML cuối cùng + ghi tập khoá
    /// InAnService trao vào) và một HttpClient đã đăng nhập Quản trị viên giáo xứ.</summary>
    private (WebApplicationFactory<Program> Factory, HttpClient Client, BoTrinhDuyetGhiHtml Html,
        BoDoMauInGhiKhoa Khoa) MayChuThu()
    {
        var html = new BoTrinhDuyetGhiHtml();
        var khoa = new BoDoMauInGhiKhoa();
        var factory = app.WithWebHostBuilder(b => b.ConfigureTestServices(s =>
        {
            s.RemoveAll<BoTrinhDuyet>();
            s.AddSingleton<BoTrinhDuyet>(html);
            s.RemoveAll<BoDoMauIn>();
            s.AddSingleton<BoDoMauIn>(khoa);
        }));
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", app.PhatHanhToken());
        return (factory, client, html, khoa);
    }

    [Fact]
    public async Task Moi_bien_kha_dung_deu_duoc_InAnService_gan_gia_tri_that()
    {
        var nc = await DungDuLieu();
        var duongDan = DuongDanIn(nc);
        duongDan.Keys.Should().BeEquivalentTo(MauInCatalog.TatCa.Select(m => m.TenMau),
            "mọi mẫu trong danh mục phải có một endpoint in thật để kiểm — thêm mẫu mới thì khai ở đây");

        var (factory, client, ghiHtml, ghiKhoa) = MayChuThu();
        using var _ = factory;

        var thieuGiaTri = new List<string>();
        var conNgoacNhon = new List<string>();
        foreach (var moTa in MauInCatalog.TatCa)
        {
            // Mẫu tuỳ chỉnh chứa TẤT CẢ biến khả dụng của đúng mẫu này, mỗi biến một dòng.
            var than = string.Concat(moTa.BienKhaDung.Select(b => $"<p>{b.Key}: {{{{{b.Key}}}}}</p>"));
            var luu = await client.PutAsJsonAsync($"/api/mau-in/{moTa.TenMau}/rieng",
                new LuuMauInRequest($"<html><body>{than}</body></html>", 0));
            luu.StatusCode.Should().Be(HttpStatusCode.OK, $"phải lưu được mẫu thử của '{moTa.TenMau}'");

            var truoc = ghiHtml.DaVe.Count;
            var res = await client.GetAsync(duongDan[moTa.TenMau]);
            res.StatusCode.Should().Be(HttpStatusCode.OK,
                $"'{moTa.TenMau}' phải in được với bộ dữ liệu thử (nếu 404 thì dữ liệu dựng còn thiếu)");
            ghiHtml.DaVe.Count.Should().BeGreaterThan(truoc, $"'{moTa.TenMau}' phải thật sự đi qua bước vẽ PDF");

            // (1) Phép kiểm CHÍNH: mọi Key công bố phải CÓ MẶT trong dictionary InAnService trao
            // vào. Đây là thứ duy nhất phân biệt được "quên gán" với "giá trị rỗng hợp lệ".
            var khoaDaTrao = ghiKhoa.KhoaDaTrao[^1];
            foreach (var bien in moTa.BienKhaDung)
                if (!khoaDaTrao.Contains(bien.Key))
                    thieuGiaTri.Add($"{moTa.TenMau}.{bien.Key}");

            // (2) Phép kiểm đầu-cuối: bản in cuối cùng không được còn dấu ngoặc nhọn nào.
            if (ghiHtml.DaVe[^1].Contains("{{", StringComparison.Ordinal))
                conNgoacNhon.Add(moTa.TenMau);

            // Dọn để mẫu thử không ảnh hưởng bài test khác dùng chung giáo xứ này.
            (await client.DeleteAsync($"/api/mau-in/{moTa.TenMau}/rieng")).EnsureSuccessStatusCode();
        }

        thieuGiaTri.Should().BeEmpty(
            "mọi Key công bố trong MauInCatalog.BienKhaDung PHẢI được InAnService gán giá trị; " +
            "các Key dưới đây được mời chèn trong combobox nhưng không có dữ liệu nào phía sau");
        conNgoacNhon.Should().BeEmpty("bản in không được còn sót \"{{\" nào");
    }

    /// <summary>Bài test trên chỉ có ý nghĩa nếu nó BIẾT BÁO LỖI. Kiểm chứng ngược trên CẢ HAI
    /// phép kiểm, dùng chính máy chủ thử đó:
    /// (1) một Key không được InAnService gán thì KHÔNG có trong tập khoá đã trao — nếu tập khoá
    ///     chứa sẵn mọi thứ thì phép kiểm chính là vô nghĩa;
    /// (2) một "{{...}}" mà bộ thay thế không đụng tới thì THẬT SỰ còn nguyên trên bản in — nếu
    ///     bản in không bao giờ chứa "{{" thì phép kiểm đầu-cuối cũng vô nghĩa.</summary>
    [Fact]
    public async Task Hai_phep_kiem_deu_biet_bao_loi()
    {
        var nc = await DungDuLieu();
        var (factory, client, ghiHtml, ghiKhoa) = MayChuThu();
        using var _ = factory;

        // "{{ }}" quanh một tên KHÔNG khớp cú pháp \w+ của BoDoMauIn.ApDung (có dấu chấm) —
        // đúng thứ Regex thay thế KHÔNG đụng tới, nên phải còn nguyên trên bản in.
        (await client.PutAsJsonAsync("/api/mau-in/LyLichCaNhan/rieng",
            new LuuMauInRequest("<html><body><p>{{Khong.Co.That}}</p></body></html>", 0)))
            .EnsureSuccessStatusCode();

        (await client.GetAsync($"/api/giao-dan/{nc.GiaoDanNam}/in/ly-lich-ca-nhan"))
            .StatusCode.Should().Be(HttpStatusCode.OK);

        ghiKhoa.KhoaDaTrao[^1].Should().NotContain("KhongHeCoBienNayTrongInAnService",
            "nếu tập khoá đã trao chứa được cả một tên bịa ra thì phép kiểm chính luôn xanh vô nghĩa");
        ghiKhoa.KhoaDaTrao[^1].Should().Contain("HoTen", "và nó phải thật sự chứa các khoá có thật");

        ghiHtml.DaVe[^1].Should().Contain("{{",
            "nếu bản in KHÔNG bao giờ chứa \"{{\" thì phép kiểm đầu-cuối là vô nghĩa");

        (await client.DeleteAsync("/api/mau-in/LyLichCaNhan/rieng")).EnsureSuccessStatusCode();
    }
}
