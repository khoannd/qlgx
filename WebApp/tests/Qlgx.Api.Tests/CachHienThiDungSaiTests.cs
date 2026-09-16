using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Qlgx.Api.Dtos;
using Qlgx.Api.Printing;
using Qlgx.Domain;
using Qlgx.Domain.Entities;

namespace Qlgx.Api.Tests;

/// <summary>
/// Khu vực "Cách hiển thị dữ liệu đúng/sai" của màn hình "Quản lý mẫu in" (xem
/// docs/superpowers/specs/man-hinh/quan-ly-mau-in.md): giáo xứ tự đặt câu chữ in ra cho các biến
/// đúng/sai thay vì <c>[x]</c>/<c>[  ]</c> nằm cứng.
///
/// Kiểm ở tầng HTML CUỐI CÙNG (thay <see cref="BoTrinhDuyet"/> trong DI bằng lớp con ghi lại
/// chuỗi HTML) chứ không phải tầng service — cùng lý do đã ghi ở MauInDayDuBienTests: PDF là nhị
/// phân đã nén nên không tìm chuỗi trong đó được, còn <c>BoTrinhDuyet.XuatPdfAsync</c> là chỗ hẹp
/// nhất mà MỌI bản in đều đi qua. Tác dụng phụ tốt: không khởi động Chromium nên chạy nhanh.
/// </summary>
public class CachHienThiDungSaiTests(QlgxApiFactory app) : IClassFixture<QlgxApiFactory>
{
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

    /// <summary>Đếm số lượt TRUY VẤN bảng cach_hien_thi_dung_sai trong một lượt in — bằng chứng
    /// cho cạm bẫy hiệu năng ở <c>InAnService.LayBangCachHienThi</c>. Chặn ở tầng SQL (interceptor
    /// của EF) thay vì đếm gián tiếp, vì đây chính là con số cần khẳng định.</summary>
    private sealed class DemTruyVanBang : Microsoft.EntityFrameworkCore.Diagnostics.DbCommandInterceptor
    {
        /// <summary>Số câu lệnh chạm bảng cach_hien_thi_dung_sai.</summary>
        public int SoLan;

        /// <summary>Tổng số câu lệnh bất kỳ — chỉ để bài test phân biệt "nạp đúng 1 lần" với
        /// "interceptor không hề được gắn nên đếm được 0". Không có nó thì một lỗi cấu hình DI sẽ
        /// giả dạng thành một kết quả tốt (0 &lt; 1... nhưng nếu ai đổi thành Should().BeLessThan
        /// thì 0 lại "đạt").</summary>
        public int TongSoLenh;

        private void Dem(System.Data.Common.DbCommand lenh)
        {
            Interlocked.Increment(ref TongSoLenh);
            if (lenh.CommandText.Contains("cach_hien_thi_dung_sai", StringComparison.Ordinal))
                Interlocked.Increment(ref SoLan);
        }

        // Bắt ở ReaderExecuting(+Async) chứ KHÔNG phải CommandCreated: đã thử CommandCreated
        // trước, bộ đếm đứng im ở 0. Mọi truy vấn SELECT của EF đều đi qua hai hàm này (đường
        // async là đường thật sự chạy ở đây vì toàn bộ service dùng ToListAsync).
        public override Microsoft.EntityFrameworkCore.Diagnostics.InterceptionResult<System.Data.Common.DbDataReader>
            ReaderExecuting(
                System.Data.Common.DbCommand command,
                Microsoft.EntityFrameworkCore.Diagnostics.CommandEventData eventData,
                Microsoft.EntityFrameworkCore.Diagnostics.InterceptionResult<System.Data.Common.DbDataReader> result)
        {
            Dem(command);
            return base.ReaderExecuting(command, eventData, result);
        }

        public override ValueTask<Microsoft.EntityFrameworkCore.Diagnostics.InterceptionResult<System.Data.Common.DbDataReader>>
            ReaderExecutingAsync(
                System.Data.Common.DbCommand command,
                Microsoft.EntityFrameworkCore.Diagnostics.CommandEventData eventData,
                Microsoft.EntityFrameworkCore.Diagnostics.InterceptionResult<System.Data.Common.DbDataReader> result,
                CancellationToken cancellationToken = default)
        {
            Dem(command);
            return base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
        }
    }

    private (WebApplicationFactory<Program> Factory, HttpClient Client, BoTrinhDuyetGhiHtml Html)
        MayChuThu(int loaiTaiKhoan = 0, Guid? giaoXuId = null, DemTruyVanBang? dem = null)
    {
        var html = new BoTrinhDuyetGhiHtml();
        var factory = app.WithWebHostBuilder(b => b.ConfigureTestServices(s =>
        {
            s.RemoveAll<BoTrinhDuyet>();
            s.AddSingleton<BoTrinhDuyet>(html);
            if (dem is not null)
            {
                // Gắn interceptor đếm bằng cách ĐĂNG KÝ LẠI DbContext kèm AddInterceptors, KHÔNG
                // phải bằng cách thả một IInterceptor vào DI: dự án này gắn interceptor trong
                // QlgxDbContext.OnConfiguring (xem ghi chú ở đó), nên đường auto-discover qua DI
                // không có tác dụng — đã thử, bộ đếm đứng im ở 0. OnConfiguring vẫn chạy và GỘP
                // thêm interceptor RLS vào, nên cách này không làm mất lớp phòng thủ nào.
                s.RemoveAll<Microsoft.EntityFrameworkCore.DbContextOptions<Qlgx.Data.QlgxDbContext>>();
                s.RemoveAll<Microsoft.EntityFrameworkCore.DbContextOptions>();
                s.AddDbContext<Qlgx.Data.QlgxDbContext>(opt => opt
                    .UseNpgsql(app.ChuoiKetNoi)
                    .AddInterceptors(dem));
            }
        }));
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Bearer", app.PhatHanhToken(giaoXuId, loaiTaiKhoan));
        return (factory, client, html);
    }

    /// <summary>Câu chữ đi vào mẫu qua <c>duLieu</c> nên <c>BoDoMauIn.ApDung</c> cho chạy qua
    /// <c>HtmlEncoder.Default.Encode</c> — bộ mã hoá này thoát CẢ ký tự tiếng Việt có dấu thành
    /// thực thể số ("Tân" → "T&amp;#xE2;n"). Vì vậy MỌI phép so chuỗi tiếng Việt trên HTML in ra
    /// phải đi qua hàm này, không so thẳng chuỗi gõ vào (đã vấp đúng chỗ này lúc viết bài test).
    /// Ký tự ASCII như "[", "]", "[x]" không bị đổi nên so thẳng vẫn đúng.</summary>
    private static string Ma(string s) => System.Text.Encodings.Web.HtmlEncoder.Default.Encode(s);

    private static int _daiMa;

    private sealed record NguCanh(Guid GiaoDanId, Guid GiaDinhId);

    /// <summary>Một giáo dân BẬT cả 4 cờ đúng/sai và một gia đình BẬT DaChuyenXu — để mọi biến
    /// trong danh mục đều có vế "đúng" thật sự in ra được. Mỗi lượt gọi lấy một dải mã cũ riêng
    /// (các bài test trong lớp này dùng chung một giáo xứ, trùng mã sẽ đụng chỉ mục duy nhất).</summary>
    private async Task<NguCanh> DungDuLieu(Guid? giaoXuId = null)
    {
        var gx = giaoXuId ?? app.GiaoXuId;
        var goc = 8200 + Interlocked.Increment(ref _daiMa) * 10;
        await using var db = app.TaoContextThuan();

        var g = new GiaoDan
        {
            GiaoXuId = gx, MaGiaoDanCu = goc + 1, HoTen = "Nguyen Van Dung Sai", TenThanh = "Maria",
            Phai = "Nam", ConHoc = true, TanTong = true, DaCoGiaDinh = true, QuaDoi = true,
        };
        db.GiaoDan.Add(g);

        var giaDinh = new GiaDinh
        {
            GiaoXuId = gx, MaGiaDinhCu = goc + 2, TenGiaDinh = "Gia đình Đúng Sai",
            DaChuyenXu = true,
        };
        db.GiaDinh.Add(giaDinh);
        db.ThanhVienGiaDinh.Add(new ThanhVienGiaDinh
        {
            GiaoXuId = gx, GiaDinh = giaDinh, GiaoDan = g, VaiTro = VaiTroGiaDinh.Chong, ChuHo = true,
        });

        await db.SaveChangesAsync();
        return new NguCanh(g.Id, giaDinh.Id);
    }

    /// <summary>Dọn sạch mọi tuỳ chỉnh của CẢ HAI cấp, CẢ HAI bảng, ở ĐẦU mỗi bài test — các bài
    /// test trong lớp này dùng chung một giáo xứ, cùng bộ 5 biến và cùng vài tên mẫu in.
    ///
    /// Phải dọn cả mau_in_tuy_chinh: các bài dưới đây cài mẫu in thử bằng
    /// <c>PUT .../rieng</c> với RowVersion 0 ("chưa từng tuỳ chỉnh"), nên một dòng còn sót lại từ
    /// bài trước sẽ làm lượt PUT sau trả 409 Conflict. Dọn ở ĐẦU (không chỉ ở cuối) để một bài đỏ
    /// giữa chừng không kéo theo cả loạt bài sau đỏ oan — che mất lỗi thật.</summary>
    private async Task Don()
    {
        await using var db = app.TaoContextThuan();
        db.CachHienThiDungSai.RemoveRange(db.CachHienThiDungSai);
        db.MauInTuyChinh.RemoveRange(db.MauInTuyChinh);
        await db.SaveChangesAsync();
    }

    // --- Danh mục -------------------------------------------------------------------------

    /// <summary>Danh mục phải khớp ĐÚNG tập biến mà InAnService thật sự dựng bằng bảng câu chữ —
    /// khẳng định bằng HÀNH VI (đặt câu chữ rồi in thật, thấy đổi) ở bài
    /// <see cref="Moi_bien_trong_danh_muc_deu_that_su_doi_duoc_ban_in"/> bên dưới, còn ở đây chỉ
    /// chốt cứng danh sách để không ai lặng lẽ thêm/bớt một dòng trên màn hình mà không nghĩ.</summary>
    [Fact]
    public void Danh_muc_dung_sai_dung_5_bien_da_chot()
    {
        BienDungSaiCatalog.TatCa.Select(b => b.Key).Should().BeEquivalentTo(
            ["ConHoc", "TanTong", "DaCoGiaDinh", "QuaDoi", "DaChuyenXu"]);
        BienDungSaiCatalog.TatCa.Should().OnlyContain(b => !string.IsNullOrWhiteSpace(b.Nhan));
    }

    /// <summary>Mọi biến công bố ở đây phải là biến in THẬT (có trong MauInCatalog.BienKhaDung của
    /// ít nhất một mẫu) — nếu không, màn hình mời quý cha/quý sơ đặt câu chữ cho một chỗ không hề
    /// tồn tại trên tờ giấy nào.</summary>
    [Fact]
    public void Moi_bien_dung_sai_deu_la_bien_in_that()
    {
        var moiBienIn = MauInCatalog.TatCa
            .SelectMany(m => m.BienKhaDung).Select(b => b.Key).ToHashSet(StringComparer.Ordinal);
        foreach (var b in BienDungSaiCatalog.TatCa)
            moiBienIn.Should().Contain(b.Key, $"biến đúng/sai '{b.Key}' phải chèn được vào mẫu in");
    }

    // --- Tương thích ngược ------------------------------------------------------------------

    /// <summary>ĐIỀU QUAN TRỌNG NHẤT của cả tính năng: giáo xứ chưa tuỳ chỉnh gì thì bản in KHÔNG
    /// đổi một ly nào so với trước — vẫn đúng "[x]"/"[  ]" của <c>Dau(bool)</c> cũ và của bản
    /// desktop (ReportLyLichCaNhan.cs dòng 122-124).</summary>
    [Fact]
    public async Task Chua_tuy_chinh_gi_thi_ban_in_khong_doi_van_la_dau_ngoac_vuong()
    {
        await Don();
        var nc = await DungDuLieu();
        var (factory, client, ghiHtml) = MayChuThu();
        using var _ = factory;

        (await client.PutAsJsonAsync("/api/mau-in/LyLichCaNhan/rieng", new LuuMauInRequest(
            "<html><body><p>ConHoc={{ConHoc}}|TanTong={{TanTong}}|QuaDoi={{QuaDoi}}</p></body></html>", 0)))
            .EnsureSuccessStatusCode();

        (await client.GetAsync($"/api/giao-dan/{nc.GiaoDanId}/in/ly-lich-ca-nhan"))
            .StatusCode.Should().Be(HttpStatusCode.OK);

        // Giáo dân này bật cả 3 cờ nên cả 3 phải là "[x]" — đúng hành vi trước khi có tính năng.
        ghiHtml.DaVe[^1].Should().Contain("ConHoc=[x]|TanTong=[x]|QuaDoi=[x]");

        (await client.DeleteAsync("/api/mau-in/LyLichCaNhan/rieng")).EnsureSuccessStatusCode();
    }

    /// <summary>Cùng ý trên nhưng ở phía API danh mục: chưa ai đặt gì thì màn hình phải hiện đúng
    /// mặc định gốc và cấp "MacDinh".</summary>
    [Fact]
    public async Task Danh_sach_khi_chua_tuy_chinh_tra_ve_mac_dinh_goc()
    {
        await Don();
        var client = app.CreateAuthClient();

        var ds = await client.GetFromJsonAsync<List<CachHienThiDungSaiItemDto>>("/api/cach-hien-thi");

        ds.Should().HaveCount(5);
        ds!.Should().OnlyContain(x => x.CapDangDung == "MacDinh");
        ds.Should().OnlyContain(x => x.KhiDungDangDung == "[x]" && x.KhiSaiDangDung == "[  ]");
        ds.Should().OnlyContain(x => !x.Rieng.DaTuyChinh && !x.HeThong.DaTuyChinh);
        ds.Should().OnlyContain(x => x.Rieng.RowVersion == 0 && x.HeThong.RowVersion == 0);
    }

    // --- Thứ tự phân giải ba cấp -------------------------------------------------------------

    /// <summary>Thứ tự phân giải ĐẦY ĐỦ, kiểm trên chính bản in: mặc định gốc → cấp hệ thống đè
    /// lên → cấp giáo xứ đè lên tiếp → xoá cấp giáo xứ thì rơi NGƯỢC về cấp hệ thống → xoá nốt
    /// thì về mặc định gốc.</summary>
    [Fact]
    public async Task Rieng_thang_he_thong_thang_mac_dinh_va_roi_nguoc_dung_thu_tu()
    {
        await Don();
        var nc = await DungDuLieu();
        var (factory, client, ghiHtml) = MayChuThu();
        using var _ = factory;
        var clientHeThong = MayChuThu(loaiTaiKhoan: 9).Client;

        (await client.PutAsJsonAsync("/api/mau-in/LyLichCaNhan/rieng", new LuuMauInRequest(
            "<html><body><p>[{{TanTong}}]</p></body></html>", 0))).EnsureSuccessStatusCode();

        async Task<string> InRoiLay()
        {
            (await client.GetAsync($"/api/giao-dan/{nc.GiaoDanId}/in/ly-lich-ca-nhan"))
                .StatusCode.Should().Be(HttpStatusCode.OK);
            return ghiHtml.DaVe[^1];
        }

        // 1. Mặc định gốc.
        (await InRoiLay()).Should().Contain("[[x]]");

        // 2. Quản trị hệ thống đặt câu chữ chung.
        (await clientHeThong.PutAsJsonAsync("/api/cach-hien-thi/TanTong/he-thong",
            new LuuCachHienThiRequest("Tân tòng (hệ thống)", "", 0))).EnsureSuccessStatusCode();
        (await InRoiLay()).Should().Contain(Ma("[Tân tòng (hệ thống)]"));

        // 3. Giáo xứ đặt riêng — ĐÈ LÊN cấp hệ thống.
        (await client.PutAsJsonAsync("/api/cach-hien-thi/TanTong/rieng",
            new LuuCachHienThiRequest("Tân tòng", "", 0))).EnsureSuccessStatusCode();
        (await InRoiLay()).Should().Contain(Ma("[Tân tòng]"));

        var ds = await client.GetFromJsonAsync<List<CachHienThiDungSaiItemDto>>("/api/cach-hien-thi");
        var dong = ds!.Single(x => x.TenBien == "TanTong");
        dong.CapDangDung.Should().Be("TuyChinhGiaoXu");
        dong.KhiDungDangDung.Should().Be("Tân tòng");
        dong.Rieng.DaTuyChinh.Should().BeTrue();
        dong.HeThong.DaTuyChinh.Should().BeTrue();
        // Các biến khác KHÔNG bị ảnh hưởng — phân giải theo TỪNG biến, không theo cả bảng.
        ds.Single(x => x.TenBien == "ConHoc").CapDangDung.Should().Be("MacDinh");

        // 4. Khôi phục cấp giáo xứ — rơi NGƯỢC về cấp hệ thống, không nhảy thẳng về mặc định.
        (await client.DeleteAsync("/api/cach-hien-thi/TanTong/rieng")).EnsureSuccessStatusCode();
        (await InRoiLay()).Should().Contain(Ma("[Tân tòng (hệ thống)]"));

        // 5. Khôi phục nốt cấp hệ thống — về mặc định gốc.
        (await clientHeThong.DeleteAsync("/api/cach-hien-thi/TanTong/he-thong")).EnsureSuccessStatusCode();
        (await InRoiLay()).Should().Contain("[[x]]");

        (await client.DeleteAsync("/api/mau-in/LyLichCaNhan/rieng")).EnsureSuccessStatusCode();
        await Don();
    }

    /// <summary>Ô để TRỐNG nghĩa là "không in ra chữ gì" — đây là cách dùng chính mà người dùng
    /// mô tả (biến TanTong: đúng thì hiện "Tân tòng", sai thì để trắng), nên phải có bài riêng:
    /// vế sai KHÔNG được rơi xuống mặc định "[  ]" chỉ vì ô rỗng.</summary>
    [Fact]
    public async Task O_de_trong_thi_khong_in_ra_chu_gi_chu_khong_roi_ve_mac_dinh()
    {
        await Don();
        var nc = await DungDuLieu();
        var (factory, client, ghiHtml) = MayChuThu();
        using var _ = factory;

        // Giáo dân dựng sẵn bật TanTong=true, nên dùng biến ConHoc... cũng true. Dùng một giáo
        // dân KHÁC với mọi cờ tắt để kiểm đúng vế SAI.
        Guid giaoDanTat;
        await using (var db = app.TaoContextThuan())
        {
            var g = new GiaoDan
            {
                GiaoXuId = app.GiaoXuId, MaGiaoDanCu = 8900 + Interlocked.Increment(ref _daiMa),
                HoTen = "Nguoi Khong Tan Tong", TenThanh = "Anna", Phai = "Nữ", TanTong = false,
            };
            db.GiaoDan.Add(g);
            await db.SaveChangesAsync();
            giaoDanTat = g.Id;
        }

        (await client.PutAsJsonAsync("/api/mau-in/LyLichCaNhan/rieng", new LuuMauInRequest(
            "<html><body><p>[{{TanTong}}]</p></body></html>", 0))).EnsureSuccessStatusCode();
        (await client.PutAsJsonAsync("/api/cach-hien-thi/TanTong/rieng",
            new LuuCachHienThiRequest("Tân tòng", null, 0))).EnsureSuccessStatusCode();

        (await client.GetAsync($"/api/giao-dan/{giaoDanTat}/in/ly-lich-ca-nhan"))
            .StatusCode.Should().Be(HttpStatusCode.OK);
        ghiHtml.DaVe[^1].Should().Contain("[]", "để trống ô 'khi sai' thì chỗ đó không in chữ nào");
        ghiHtml.DaVe[^1].Should().NotContain("[  ]");

        // Và người bật cờ vẫn ra câu chữ đã đặt.
        (await client.GetAsync($"/api/giao-dan/{nc.GiaoDanId}/in/ly-lich-ca-nhan"))
            .StatusCode.Should().Be(HttpStatusCode.OK);
        ghiHtml.DaVe[^1].Should().Contain(Ma("[Tân tòng]"));

        (await client.DeleteAsync("/api/mau-in/LyLichCaNhan/rieng")).EnsureSuccessStatusCode();
        await Don();
    }

    /// <summary>Mỗi biến trong danh mục phải THẬT SỰ đổi được bản in — bằng chứng hành vi cho
    /// việc danh mục khớp đúng các chỗ InAnService dựng câu chữ đúng/sai. Biến gia đình
    /// (DaChuyenXu) đi qua mẫu "Phiếu gia đình", 4 biến giáo dân đi qua "Lý lịch cá nhân".</summary>
    [Theory]
    [InlineData("ConHoc", "LyLichCaNhan")]
    [InlineData("TanTong", "LyLichCaNhan")]
    [InlineData("DaCoGiaDinh", "LyLichCaNhan")]
    [InlineData("QuaDoi", "LyLichCaNhan")]
    [InlineData("DaChuyenXu", "PhieuGiaDinh")]
    public async Task Moi_bien_trong_danh_muc_deu_that_su_doi_duoc_ban_in(string tenBien, string tenMau)
    {
        await Don();
        var nc = await DungDuLieu();
        var (factory, client, ghiHtml) = MayChuThu();
        using var _ = factory;

        (await client.PutAsJsonAsync($"/api/mau-in/{tenMau}/rieng", new LuuMauInRequest(
            $"<html><body><p>[{{{{{tenBien}}}}}]</p></body></html>", 0))).EnsureSuccessStatusCode();
        (await client.PutAsJsonAsync($"/api/cach-hien-thi/{tenBien}/rieng",
            new LuuCachHienThiRequest($"CO-{tenBien}", "KHONG", 0))).EnsureSuccessStatusCode();

        var duongDan = tenMau == "PhieuGiaDinh"
            ? $"/api/gia-dinh/{nc.GiaDinhId}/in/phieu-gia-dinh"
            : $"/api/giao-dan/{nc.GiaoDanId}/in/ly-lich-ca-nhan";
        (await client.GetAsync(duongDan)).StatusCode.Should().Be(HttpStatusCode.OK);

        ghiHtml.DaVe[^1].Should().Contain($"[CO-{tenBien}]");

        (await client.DeleteAsync($"/api/mau-in/{tenMau}/rieng")).EnsureSuccessStatusCode();
        await Don();
    }

    // --- Khử trùng / XSS ---------------------------------------------------------------------

    /// <summary>Giáo xứ gõ "&lt;script&gt;alert(1)&lt;/script&gt;" làm câu chữ thì bản in KHÔNG
    /// được có thẻ script SỐNG. KIỂM THẬT thay vì tin suông rằng
    /// <c>BoDoMauIn.ApDung</c> đã HtmlEncode mọi giá trị trong <c>duLieu</c> — đúng yêu cầu
    /// "tự kiểm chứng bằng một bài test thật".</summary>
    [Fact]
    public async Task Cau_chu_co_the_script_bi_thoat_html_khong_thanh_the_song()
    {
        await Don();
        var nc = await DungDuLieu();
        var (factory, client, ghiHtml) = MayChuThu();
        using var _ = factory;

        const string doc = "<script>alert(1)</script>";
        (await client.PutAsJsonAsync("/api/mau-in/LyLichCaNhan/rieng", new LuuMauInRequest(
            "<html><body><p>{{TanTong}}</p></body></html>", 0))).EnsureSuccessStatusCode();
        (await client.PutAsJsonAsync("/api/cach-hien-thi/TanTong/rieng",
            new LuuCachHienThiRequest(doc, "", 0))).EnsureSuccessStatusCode();

        (await client.GetAsync($"/api/giao-dan/{nc.GiaoDanId}/in/ly-lich-ca-nhan"))
            .StatusCode.Should().Be(HttpStatusCode.OK);

        var html = ghiHtml.DaVe[^1];
        html.Should().NotContain("<script>alert(1)</script>", "câu chữ phải được thoát HTML");
        html.Should().Contain("alert(1)", "nội dung vẫn hiện nguyên văn trên giấy, chỉ là không sống");
        html.Should().Contain("&lt;script&gt;");

        (await client.DeleteAsync("/api/mau-in/LyLichCaNhan/rieng")).EnsureSuccessStatusCode();
        await Don();
    }

    /// <summary>Câu chữ quá dài bị từ chối — một ô bị dán nhầm cả trang văn bản không được phá vỡ
    /// bố cục tờ giấy in.</summary>
    [Fact]
    public async Task Cau_chu_qua_dai_bi_tu_choi()
    {
        var client = app.CreateAuthClient();
        var res = await client.PutAsJsonAsync("/api/cach-hien-thi/TanTong/rieng",
            new LuuCachHienThiRequest(new string('a', 201), "", 0));
        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Ten_bien_khong_co_that_tra_404()
    {
        var client = app.CreateAuthClient();
        (await client.PutAsJsonAsync("/api/cach-hien-thi/BienKhongCoThat/rieng",
            new LuuCachHienThiRequest("x", "y", 0))).StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await client.DeleteAsync("/api/cach-hien-thi/BienKhongCoThat/rieng"))
            .StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // --- Phân quyền --------------------------------------------------------------------------

    [Fact]
    public async Task Chua_dang_nhap_khong_xem_duoc()
    {
        (await app.CreateClient().GetAsync("/api/cach-hien-thi"))
            .StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    /// <summary>Tài khoản giáo xứ thường (kể cả Quản trị viên giáo xứ, loaiTaiKhoan=0) KHÔNG sửa
    /// được cấp hệ thống — nếu lọt thì một giáo xứ đổi được câu chữ của MỌI giáo xứ khác.</summary>
    [Fact]
    public async Task Tai_khoan_giao_xu_khong_sua_duoc_cap_he_thong()
    {
        var client = app.CreateAuthClient(loaiTaiKhoan: 0);
        (await client.PutAsJsonAsync("/api/cach-hien-thi/TanTong/he-thong",
            new LuuCachHienThiRequest("Lén sửa", "", 0))).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await client.DeleteAsync("/api/cach-hien-thi/TanTong/he-thong"))
            .StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    /// <summary>Tài khoản KHÔNG phải quản trị (loaiTaiKhoan=1) chỉ xem được, không sửa được cả
    /// cấp riêng.</summary>
    [Fact]
    public async Task Tai_khoan_thuong_khong_sua_duoc_cap_rieng()
    {
        var client = app.CreateAuthClient(loaiTaiKhoan: 1);
        (await client.GetAsync("/api/cach-hien-thi")).StatusCode.Should().Be(HttpStatusCode.OK);
        (await client.PutAsJsonAsync("/api/cach-hien-thi/TanTong/rieng",
            new LuuCachHienThiRequest("x", "y", 0))).StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    /// <summary>Giáo xứ A KHÔNG đụng được tới câu chữ của giáo xứ B: GiaoXuId luôn lấy từ claim,
    /// không bao giờ từ tham số, nên B đặt gì thì A không thấy và cũng không sửa được.</summary>
    [Fact]
    public async Task Giao_xu_A_khong_thay_va_khong_sua_duoc_cau_chu_cua_giao_xu_B()
    {
        await Don();
        var gxB = Guid.NewGuid();
        await using (var db = app.TaoContextThuan())
        {
            db.GiaoXu.Add(new GiaoXu
            {
                Id = gxB, TenGiaoXu = "GX B cach hien thi",
                MaGiaoXuCu = new Random().Next(60000, 69999),
            });
            await db.SaveChangesAsync();
        }

        var clientB = app.CreateAuthClient(gxB);
        (await clientB.PutAsJsonAsync("/api/cach-hien-thi/TanTong/rieng",
            new LuuCachHienThiRequest("Chữ của B", "", 0))).EnsureSuccessStatusCode();

        var clientA = app.CreateAuthClient();
        var dsA = await clientA.GetFromJsonAsync<List<CachHienThiDungSaiItemDto>>("/api/cach-hien-thi");
        var dongA = dsA!.Single(x => x.TenBien == "TanTong");
        dongA.CapDangDung.Should().Be("MacDinh", "giáo xứ A không được thấy tuỳ chỉnh của giáo xứ B");
        dongA.Rieng.DaTuyChinh.Should().BeFalse();
        dongA.KhiDungDangDung.Should().Be("[x]");

        // A lưu của mình — KHÔNG được ghi đè lên dòng của B.
        (await clientA.PutAsJsonAsync("/api/cach-hien-thi/TanTong/rieng",
            new LuuCachHienThiRequest("Chữ của A", "", 0))).EnsureSuccessStatusCode();

        await using (var db = app.TaoContextThuan())
        {
            db.CachHienThiDungSai.Single(x => x.GiaoXuId == gxB && x.TenBien == "TanTong")
                .KhiDung.Should().Be("Chữ của B", "dòng của giáo xứ B phải còn nguyên");
            db.CachHienThiDungSai.Single(x => x.GiaoXuId == app.GiaoXuId && x.TenBien == "TanTong")
                .KhiDung.Should().Be("Chữ của A");
        }

        await Don();
    }

    // --- RowVersion ---------------------------------------------------------------------------

    /// <summary>Chống ghi đè âm thầm: hai người cùng mở màn hình, người sau lưu bằng RowVersion đã
    /// lỗi thời thì phải bị báo xung đột chứ không lặng lẽ đè mất công sức người trước.</summary>
    [Fact]
    public async Task RowVersion_sai_bao_xung_dot_khong_ghi_de_am_tham()
    {
        await Don();
        var client = app.CreateAuthClient();

        (await client.PutAsJsonAsync("/api/cach-hien-thi/ConHoc/rieng",
            new LuuCachHienThiRequest("Bản 1", "", 0))).EnsureSuccessStatusCode();

        // RowVersion=0 nay đã lỗi thời (dòng đã tồn tại) — đúng tình huống một tab cũ lưu lại.
        var res = await client.PutAsJsonAsync("/api/cach-hien-thi/ConHoc/rieng",
            new LuuCachHienThiRequest("Bản 2 từ tab cũ", "", 0));
        res.StatusCode.Should().Be(HttpStatusCode.Conflict);

        await using (var db = app.TaoContextThuan())
            db.CachHienThiDungSai.Single(x => x.GiaoXuId == app.GiaoXuId && x.TenBien == "ConHoc")
                .KhiDung.Should().Be("Bản 1", "bản ghi cũ không được bị đè");

        await Don();
    }

    /// <summary>Lưu ĐÚNG RowVersion vừa đọc thì thành công — quy ước "0 = chưa từng tuỳ chỉnh,
    /// đây là lần lưu đầu" không được làm hỏng lần lưu thứ hai hợp lệ.</summary>
    [Fact]
    public async Task RowVersion_dung_thi_luu_duoc_lan_thu_hai()
    {
        await Don();
        var client = app.CreateAuthClient();

        (await client.PutAsJsonAsync("/api/cach-hien-thi/ConHoc/rieng",
            new LuuCachHienThiRequest("Bản 1", "", 0))).EnsureSuccessStatusCode();

        var ds = await client.GetFromJsonAsync<List<CachHienThiDungSaiItemDto>>("/api/cach-hien-thi");
        var rv = ds!.Single(x => x.TenBien == "ConHoc").Rieng.RowVersion;
        rv.Should().NotBe(0);

        (await client.PutAsJsonAsync("/api/cach-hien-thi/ConHoc/rieng",
            new LuuCachHienThiRequest("Bản 2", "", rv))).EnsureSuccessStatusCode();

        await using (var db = app.TaoContextThuan())
            db.CachHienThiDungSai.Single(x => x.GiaoXuId == app.GiaoXuId && x.TenBien == "ConHoc")
                .KhiDung.Should().Be("Bản 2");

        await Don();
    }

    // --- Hiệu năng ---------------------------------------------------------------------------

    /// <summary>CẠM BẪY HIỆU NĂNG đã nêu ở <c>InAnService.LayBangCachHienThi</c>: in lý lịch cho
    /// CẢ GIA ĐÌNH gọi lặp <c>DungHtmlLyLichCaNhan</c> cho từng thành viên. Bảng câu chữ phải nạp
    /// ĐÚNG MỘT LẦN cho cả lượt in, không phải một lần mỗi người (và càng không phải một lần mỗi
    /// biến). Đếm thẳng số câu lệnh SQL chạm bảng để không phải tin vào ghi chú.</summary>
    [Fact]
    public async Task In_ca_gia_dinh_chi_nap_bang_cau_chu_dung_mot_lan()
    {
        await Don();

        // Gia đình 3 người — nếu nạp theo từng người thì sẽ thấy 3 truy vấn.
        Guid giaDinhId;
        var goc = 8600 + Interlocked.Increment(ref _daiMa) * 10;
        await using (var db = app.TaoContextThuan())
        {
            var giaDinh = new GiaDinh
            {
                GiaoXuId = app.GiaoXuId, MaGiaDinhCu = goc, TenGiaDinh = "Gia đình Ba Người",
            };
            db.GiaDinh.Add(giaDinh);
            for (var i = 0; i < 3; i++)
            {
                var g = new GiaoDan
                {
                    GiaoXuId = app.GiaoXuId, MaGiaoDanCu = goc + 1 + i, HoTen = $"Thanh Vien {i}",
                    TenThanh = "Giuse", Phai = "Nam", TanTong = true,
                };
                db.GiaoDan.Add(g);
                db.ThanhVienGiaDinh.Add(new ThanhVienGiaDinh
                {
                    GiaoXuId = app.GiaoXuId, GiaDinh = giaDinh, GiaoDan = g,
                    VaiTro = i == 0 ? VaiTroGiaDinh.Chong : VaiTroGiaDinh.Con,
                });
            }
            await db.SaveChangesAsync();
            giaDinhId = giaDinh.Id;
        }

        var dem = new DemTruyVanBang();
        var (factory, client, ghiHtml) = MayChuThu(dem: dem);
        using var _ = factory;

        // CỐ Ý dùng mẫu GỐC nhúng cứng, KHÔNG cài mẫu tuỳ chỉnh: mẫu gốc LyLichCaNhan.html đã có
        // sẵn {{TanTong}}, nên bài test vừa đo được đúng cạm bẫy hiệu năng vừa chạy trên đường đi
        // thật của người dùng phổ biến nhất (giáo xứ chưa sửa mẫu). Ngoài ra "In lý lịch cả gia
        // đình" hiện KHÔNG chạy đúng với mẫu ĐÃ tuỳ chỉnh — xem ghi chú ở cuối lớp này.
        const string dauRieng = "DAU-TAN-TONG"; // ASCII thuần: không bị HtmlEncoder đổi, không đụng
                                                // chữ "Tân tòng" vốn đã có sẵn trong mẫu gốc.
        (await client.PutAsJsonAsync("/api/cach-hien-thi/TanTong/rieng",
            new LuuCachHienThiRequest(dauRieng, "", 0))).EnsureSuccessStatusCode();

        Interlocked.Exchange(ref dem.SoLan, 0);
        (await client.GetAsync($"/api/gia-dinh/{giaDinhId}/in/ly-lich-ca-nhan"))
            .StatusCode.Should().Be(HttpStatusCode.OK);

        dem.TongSoLenh.Should().BeGreaterThan(0,
            "nếu bằng 0 thì interceptor chưa được gắn và con số dưới đây vô nghĩa");
        dem.SoLan.Should().Be(1,
            "bảng câu chữ phải nạp một lần cho cả lượt in, dù in cho 3 thành viên");
        // Và vẫn in đúng cho TỪNG người: 3 lần xuất hiện -> Split cho 4 mảnh.
        ghiHtml.DaVe[^1].Split(dauRieng).Length.Should().Be(4, "cả 3 thành viên đều hiện câu chữ");

        await Don();
    }
}
