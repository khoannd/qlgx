using Microsoft.EntityFrameworkCore;
using Qlgx.Api.Printing;
using Qlgx.Data;
using Qlgx.Domain;
using Qlgx.Domain.Entities;

namespace Qlgx.Api.Services;

/// <summary>Kết quả một lượt xuất PDF — đủ để endpoint trả về, không cần biết chi tiết cách
/// dựng bên trong.</summary>
public record KetQuaInAn(byte[] NoiDung, string TenTep);

/// <summary>
/// Hạ tầng in ấn dùng chung: nạp mẫu HTML, điền dữ liệu, xuất PDF qua Playwright (xem
/// docs/superpowers/specs/man-hinh/in-an.md). Mỗi loại giấy tờ có một phương thức Xuat* riêng
/// trong lớp này — Xuất lý lịch cá nhân là mẫu đầu tiên của lượt này.
///
/// Giống mọi service nghiệp vụ khác, GiaoXuId của phiên lấy từ <see cref="IBoiCanhGiaoXu"/>
/// (claim đăng nhập) — KHÔNG bao giờ nhận Giáo xứ nào khác qua tham số, và mọi truy vấn
/// GiaoDan/GiaDinh/HonPhoi đều đi qua bộ lọc chung của QlgxDbContext nên tự động chỉ thấy đúng
/// một giáo xứ.
/// </summary>
public class InAnService(
    QlgxDbContext db, IBoiCanhGiaoXu boiCanh, BoDoMauIn mau, BoTrinhDuyet trinhDuyet,
    GiaDinhService giaDinhDv, GiaoDanService giaoDanDv, RaoHonPhoiService raoHonPhoiDv)
{
    private static string Ngay(DateOnly? d) => VanBanInAn.Ngay(d);

    /// <summary>Bảng câu chữ đúng/sai đã phân giải, NHỚ LẠI cho cả lượt in. InAnService đăng ký
    /// Scoped (xem Program.cs) nên một thực thể phục vụ đúng MỘT yêu cầu HTTP, tức đúng một lượt
    /// in — nhớ ở đây chính là "nạp một lần cho mỗi lượt in", không có chuyện rò rỉ bảng của giáo
    /// xứ này sang yêu cầu của giáo xứ khác.
    ///
    /// CẠM BẪY HIỆU NĂNG, ĐỪNG BỎ TRƯỜNG NÀY: <see cref="DungHtmlLyLichCaNhan"/> chạy LẶP cho
    /// TỪNG thành viên khi in lý lịch cả gia đình (<see cref="XuatLyLichCaNhanGiaDinh"/>). Tra
    /// CSDL bên trong đó sẽ thành (số thành viên) truy vấn, và nếu tra theo từng biến thì còn
    /// nhân thêm 5 lần nữa. Với trường này, cả lượt in tốn đúng MỘT truy vấn dù in cho bao nhiêu
    /// người.</summary>
    private BangCachHienThi? _bangCachHienThi;

    /// <summary>Nạp và phân giải bảng câu chữ đúng/sai theo đúng thứ tự "giáo xứ đang đăng nhập →
    /// cấp hệ thống → mặc định gốc [x]/[  ]" — cùng một thứ tự với <see cref="DungMau"/>, cố ý
    /// giống hệt để người dùng chỉ phải hiểu MỘT quy tắc ưu tiên cho cả màn hình.
    ///
    /// Phân giải theo TỪNG BIẾN chứ không theo cả bảng: giáo xứ đặt riêng mỗi biến TanTong thì 4
    /// biến còn lại vẫn giữ nguyên ánh xạ hệ thống (hoặc mặc định gốc) — xem BangCachHienThi.
    ///
    /// Giáo xứ chưa tuỳ chỉnh gì thì cả hai truy vấn không trả dòng nào, bảng rỗng, và
    /// <c>BangCachHienThi.Dau</c> trả đúng "[x]"/"[  ]" như hàm <c>Dau(bool)</c> static cũ —
    /// tương thích ngược tuyệt đối, có bài test khẳng định (CachHienThiDungSaiTests).</summary>
    private async Task<BangCachHienThi> LayBangCachHienThi(CancellationToken ct)
    {
        if (_bangCachHienThi is not null) return _bangCachHienThi;

        var gxId = boiCanh.GiaoXuId;
        // MỘT truy vấn lấy cả hai cấp (dòng của giáo xứ này + dòng hệ thống GiaoXuId NULL), rồi
        // phân giải trong bộ nhớ. Bảng cach_hien_thi_dung_sai KHÔNG có bộ lọc toàn cục theo
        // GiaoXuId (xem CachHienThiDungSai.cs) nên điều kiện dưới đây là bộ lọc DUY NHẤT — phải
        // tường minh, và tuyệt đối không được nới rộng.
        var dong = await db.CachHienThiDungSai.AsNoTracking()
            .Where(x => x.GiaoXuId == gxId || x.GiaoXuId == null)
            .Select(x => new { x.GiaoXuId, x.TenBien, x.KhiDung, x.KhiSai })
            .ToListAsync(ct);

        // Giáo xứ THẮNG hệ thống, đặt bằng hai lượt ghi theo thứ tự: rải dòng hệ thống trước,
        // rồi để dòng riêng của giáo xứ ghi đè lên. Biến nào không có dòng nào thì vắng mặt khỏi
        // từ điển và BangCachHienThi.Dau tự rơi về mặc định gốc.
        var anhXa = new Dictionary<string, (string? KhiDung, string? KhiSai)>();
        foreach (var d in dong.Where(x => x.GiaoXuId is null))
            anhXa[d.TenBien] = (d.KhiDung, d.KhiSai);
        foreach (var d in dong.Where(x => x.GiaoXuId is not null))
            anhXa[d.TenBien] = (d.KhiDung, d.KhiSai);

        return _bangCachHienThi = new BangCachHienThi(anhXa);
    }

    /// <summary>Mã gia đình hiển thị trên giấy tờ: ưu tiên mã do giáo xứ tự nhập
    /// (<c>MaGiaDinhRieng</c>, bật bằng cấu hình TUNHAP_MAGIADINH), không có thì dùng mã gốc.
    /// Tách thành hàm dùng chung vì nay có ba mẫu cần (Phiếu gia đình, Giới thiệu chuyển xứ,
    /// và biến {{MaGiaDinh}} thêm vào Lý lịch cá nhân).</summary>
    private static string MaGiaDinh(GiaDinh gd) => gd.MaGiaDinhRieng ?? gd.MaGiaDinhCu.ToString();

    // --- Biến NGƯỜI DÙNG TỰ CHÈN (MauInCatalog.BienKhaDung) ---------------------------------
    //
    // Các nhóm giá trị dưới đây KHÔNG có trong mẫu gốc nào cả — thêm vào dictionary `duLieu` là
    // vô hại (BoDoMauIn.ApDung chỉ thay {{Key}} CÓ MẶT trong HTML, nên kết quả in của mọi mẫu
    // gốc không đổi một ký tự) nhưng BẮT BUỘC phải có: MauInCatalog công bố các khoá này cho
    // combobox "Chèn chỗ trống", và bài test MauInDayDuBienTests dựng một mẫu chứa TẤT CẢ biến
    // rồi in thật để chứng minh không khoá nào bị bỏ quên. Mọi giá trị đều lấy từ thực thể ĐÃ
    // NẠP ở nơi gọi — không hàm nào dưới đây chạm CSDL, nên không sinh truy vấn N+1.

    private static Dictionary<string, string?> NhanThanThem(GiaoDan g) => new()
    {
        ["MaGiaoDan"] = g.MaGiaoDanCu.ToString(),
        ["Phai"] = g.Phai,
        ["CMND"] = g.CMND,
        ["DanToc"] = g.DanToc,
        ["NgheNghiep"] = g.NgheNghiep,
        ["ThuocGiaoXu"] = g.ThuocGiaoXu,
        ["ThuocGiaoPhan"] = g.ThuocGiaoPhan,
        ["GhiChuGiaoDan"] = g.GhiChu,
        ["DienThoaiGiaoDan"] = g.DienThoai,
        ["EmailGiaoDan"] = g.Email,
    };

    private static Dictionary<string, string?> RuaToiRoi(GiaoDan g) => new()
    {
        ["SoRuaToi"] = g.SoRuaToi,
        ["NgayRuaToi"] = Ngay(g.NgayRuaToi),
        ["NoiRuaToi"] = g.NoiRuaToi,
        ["ChaRuaToi"] = g.ChaRuaToi,
        ["NguoiDoDauRuaToi"] = g.NguoiDoDauRuaToi,
    };

    private static Dictionary<string, string?> RuocLeRoi(GiaoDan g) => new()
    {
        ["SoRuocLe"] = g.SoRuocLe,
        ["NgayRuocLe"] = Ngay(g.NgayRuocLe),
        ["NoiRuocLe"] = g.NoiRuocLe,
        ["ChaRuocLe"] = g.ChaRuocLe,
    };

    private static Dictionary<string, string?> ThemSucRoi(GiaoDan g) => new()
    {
        ["SoThemSuc"] = g.SoThemSuc,
        ["NgayThemSuc"] = Ngay(g.NgayThemSuc),
        ["NoiThemSuc"] = g.NoiThemSuc,
        ["ChaThemSuc"] = g.ChaThemSuc,
        ["NguoiDoDauThemSuc"] = g.NguoiDoDauThemSuc,
    };

    private static Dictionary<string, string?> GiaoLyThem(GiaoDan g) => new()
    {
        ["NgayBD1"] = Ngay(g.NgayBD1), ["NoiBD1"] = g.NoiBD1,
        ["NgayBD2"] = Ngay(g.NgayBD2), ["NoiBD2"] = g.NoiBD2,
        ["NgayTHVaoDoi"] = Ngay(g.NgayTHVaoDoi), ["NoiTHVaoDoi"] = g.NoiTHVaoDoi,
        ["NgayGLHN1"] = Ngay(g.NgayGLHN1), ["NgayGLHN2"] = Ngay(g.NgayGLHN2),
        ["NoiGLHN"] = g.NoiGLHN, ["NguoiChungNhanGLHN"] = g.NguoiChungNhanGLHN,
        ["XepLoaiGLHN"] = g.XepLoaiGLHN,
    };

    /// <summary>Các biến thêm của một GIA ĐÌNH — dùng chung cho "Phiếu gia đình" và "Giới thiệu
    /// chuyển xứ" (hai mẫu duy nhất có ngữ cảnh gia đình đầy đủ).</summary>
    private static Dictionary<string, string?> GiaDinhThem(GiaDinh gd, int soThanhVien, BangCachHienThi bang) => new()
    {
        ["MaGiaDinhCu"] = gd.MaGiaDinhCu.ToString(),
        ["SoHoKhau"] = gd.SoHoKhau,
        ["DienGiaDinh"] = gd.DienGiaDinh,
        ["SoLuongThanhVien"] = soThanhVien.ToString(),
        ["DaChuyenXu"] = bang.Dau("DaChuyenXu", gd.DaChuyenXu),
        ["NgayChuyen"] = Ngay(gd.NgayChuyen),
        ["NoiChuyen"] = gd.NoiChuyen,
    };

    /// <summary>Hôn phối dạng RỜI (mẫu gốc "Phiếu gia đình" chỉ in câu ghép {{MoTaHonPhoi}}).</summary>
    private static Dictionary<string, string?> HonPhoiRoi(HonPhoi? hp) => new()
    {
        ["SoHonPhoi"] = hp?.SoHonPhoi,
        ["NgayHonPhoi"] = Ngay(hp?.NgayHonPhoi),
        ["NoiHonPhoi"] = hp?.NoiHonPhoi,
        ["ChaHonPhoi"] = hp?.LinhMucChung,
        ["CachThucHonPhoi"] = hp?.CachThucHonPhoi,
        ["NguoiChung1"] = hp?.NguoiChung1,
        ["NguoiChung2"] = hp?.NguoiChung2,
        ["GhiChuHonPhoi"] = hp?.GhiChu,
    };

    /// <summary>Khối HTML thô (KHÔNG qua HtmlEncoder — xem BoDoMauIn.Dung) cho ô ảnh đại diện
    /// trên mẫu in (Task 1.2 VIEC-TIEP-THEO.md) — an toàn để chèn nguyên văn vì tự dựng hoàn
    /// toàn ở đây bằng dữ liệu nhị phân + MIME đã được XuLyAnh kiểm/chuẩn hoá lúc lưu (không
    /// đi qua chuỗi tự do người dùng nhập). Trả chuỗi RỖNG khi chưa có ảnh — mẫu KHÔNG hiện
    /// khung ảnh trống, không phải ô vuông trống vô nghĩa trên giấy in.</summary>
    private static string KhoiAnhDaiDien(byte[]? duLieu, string? loaiNoiDung)
    {
        if (duLieu is null || duLieu.Length == 0 || string.IsNullOrEmpty(loaiNoiDung)) return "";
        var b64 = Convert.ToBase64String(duLieu);
        return $"<div class=\"anh-dai-dien\"><img src=\"data:{loaiNoiDung};base64,{b64}\" alt=\"Ảnh đại diện\" /></div>";
    }

    /// <summary>Nạp nội dung mẫu ĐÃ ĐIỀN DỮ LIỆU theo đúng thứ tự phân giải "giáo xứ → hệ thống
    /// → mẫu gốc nhúng cứng" (xem quan-ly-mau-in.md mục "Lưu trữ và thứ tự phân giải"): mẫu
    /// riêng của giáo xứ ĐANG ĐĂNG NHẬP nếu có, rồi mới tới mẫu tuỳ chỉnh cấp hệ thống (do Quản
    /// trị hệ thống đặt, GiaoXuId NULL), cuối cùng mới rơi về mẫu gốc nhúng cứng (phao cứu sinh
    /// vĩnh viễn, không phụ thuộc CSDL — xem BoDoMauIn.Dung). Đặt Ở ĐÂY (InAnService, có sẵn
    /// db/boiCanh scoped theo request) thay vì đưa DbContext vào BoDoMauIn — BoDoMauIn đăng ký
    /// Singleton (dùng chung một trình đọc mẫu cho toàn tiến trình, xem Program.cs), không tiêm
    /// được QlgxDbContext (Scoped) vào thẳng một Singleton.
    ///
    /// KHÔNG ai không có tuỳ chỉnh nào bị ảnh hưởng: hai bước tra CSDL trả null (không có dòng)
    /// thì rơi thẳng về mau.Dung (đúng đường đi CŨ trước khi có bảng mau_in_tuy_chinh) — tương
    /// thích ngược tuyệt đối.</summary>
    private async Task<string> DungMau(string giaoPhanSlug, string tenMau,
        IReadOnlyDictionary<string, string?> duLieu, IReadOnlyDictionary<string, string?>? khoiHtml,
        CancellationToken ct)
    {
        var gxId = boiCanh.GiaoXuId;
        var rieng = await db.MauInTuyChinh.AsNoTracking()
            .Where(m => m.GiaoXuId == gxId && m.TenMau == tenMau)
            .Select(m => m.NoiDungHtml)
            .FirstOrDefaultAsync(ct);
        if (rieng is not null) return mau.ApDung(rieng, duLieu, khoiHtml);

        var heThong = await db.MauInTuyChinh.AsNoTracking()
            .Where(m => m.GiaoXuId == null && m.TenMau == tenMau)
            .Select(m => m.NoiDungHtml)
            .FirstOrDefaultAsync(ct);
        if (heThong is not null) return mau.ApDung(heThong, duLieu, khoiHtml);

        return mau.Dung(giaoPhanSlug, tenMau, duLieu, khoiHtml);
    }

    /// <summary>Nạp Giáo xứ hiện tại (theo claim đăng nhập — KHÔNG bao giờ nhận id giáo xứ nào
    /// khác) cùng Giáo hạt/Giáo phận của nó, dùng chung cho mọi mẫu in (header quốc hiệu/giáo
    /// xứ giống nhau ở mọi mẫu).</summary>
    private async Task<GiaoXu?> LayGiaoXuHienTai(CancellationToken ct) =>
        await db.GiaoXu.Include(x => x.GiaoHat).ThenInclude(h => h!.GiaoPhan)
            .FirstOrDefaultAsync(x => x.Id == boiCanh.GiaoXuId, ct);

    /// <summary>Các chỗ trống header giống nhau ở MỌI mẫu (quốc hiệu tự viết thẳng trong từng
    /// tệp .html, phần này chỉ là khối "Giáo phận/Giáo hạt/Giáo xứ + liên hệ") — một chỗ sửa,
    /// áp dụng cho mọi mẫu thay vì lặp lại ở từng phương thức Xuat*.</summary>
    private static Dictionary<string, string?> ThongTinGiaoXu(GiaoXu giaoXu) => new()
    {
        ["TenGiaoPhan"] = giaoXu.GiaoHat?.GiaoPhan?.TenGiaoPhan,
        ["TenGiaoHat"] = giaoXu.GiaoHat?.TenGiaoHat,
        ["TenGiaoXu"] = giaoXu.TenGiaoXu,
        ["DiaChiGiaoXu"] = string.IsNullOrWhiteSpace(giaoXu.DiaChi) ? "" : $" — {giaoXu.DiaChi}",
        ["DienThoaiGiaoXu"] = string.IsNullOrWhiteSpace(giaoXu.DienThoai) ? "" : $" — ĐT: {giaoXu.DienThoai}",
        ["EmailGiaoXu"] = string.IsNullOrWhiteSpace(giaoXu.Email) ? "" : $" — Email: {giaoXu.Email}",
        ["WebsiteGiaoXu"] = string.IsNullOrWhiteSpace(giaoXu.Website) ? "" : $" — Website: {giaoXu.Website}",
    };

    /// <summary>Xuất PDF "Lý lịch cá nhân" cho một giáo dân — tương đương
    /// Source/ExcelReport/ReportLyLichCaNhan.cs của bản desktop, dựng lại bằng mẫu HTML thay vì
    /// Word. Trả null nếu không tìm thấy giáo dân (đã bị lọc theo GiaoXuId hiện tại, đã xoá
    /// mềm, hoặc id không tồn tại) — endpoint trả 404 trong trường hợp đó.</summary>
    public async Task<KetQuaInAn?> XuatLyLichCaNhan(Guid giaoDanId, CancellationToken ct)
    {
        var ket = await DungHtmlLyLichCaNhan(giaoDanId, ct);
        if (ket is null) return null;
        var (g, html) = ket.Value;
        var pdf = await trinhDuyet.XuatPdfAsync(html, ct);
        return new KetQuaInAn(pdf, $"LyLichCaNhan_{g.MaGiaoDanCu}.pdf");
    }

    /// <summary>"In lý lịch cá nhân" bấm từ lưới/màn hình GIA ĐÌNH (`GxGiaDinhList.tsx`,
    /// `GiaDinhDetail.tsx`) — tương đương `item4_Click`/`GxGiaoDanList.XuatLyLichCaNhan(Dictionary)`
    /// của bản desktop (`Source/GXControl/GxGiaDinhList.cs` dòng 81-113,
    /// `Source/GXControl/GxGiaoDanList.cs` dòng 227-260): KHÔNG in riêng "chủ hộ" và KHÔNG hỏi
    /// chọn ai — in lý lịch cá nhân của TẤT CẢ thành viên đang có trong gia đình, mỗi người một
    /// trang, gộp vào MỘT tệp PDF theo thứ tự VaiTro (Chồng/Vợ trước, Con sau — đúng
    /// `ORDER BY VaiTro ASC` gốc), ngăn cách bằng ngắt trang. Bản gốc dùng
    /// `word.InsertPage()` để nối nhiều "trang lý lịch cá nhân" Word vào cùng một tài liệu; bản
    /// web ghép HTML của từng người (tái dùng NGUYÊN VẸN <see cref="DungHtmlLyLichCaNhan"/> — cùng
    /// một hàm dựng dữ liệu/mẫu với "In lý lịch cá nhân" của MỘT giáo dân) rồi xuất PDF một
    /// lần duy nhất, xem can-review-sau.md.
    ///
    /// Trả null nếu gia đình không tồn tại (đã xoá mềm/id sai/thuộc giáo xứ khác) HOẶC không
    /// còn thành viên nào (không có gì để in).</summary>
    public async Task<KetQuaInAn?> XuatLyLichCaNhanGiaDinh(Guid giaDinhId, CancellationToken ct)
    {
        var giaDinh = await db.GiaDinh
            .Include(x => x.ThanhVien)
            .FirstOrDefaultAsync(x => x.Id == giaDinhId && !x.DaXoa, ct);
        if (giaDinh is null || giaDinh.ThanhVien.Count == 0) return null;

        var thanhVienThuTu = giaDinh.ThanhVien.OrderBy(tv => tv.VaiTro).ToList();

        string? kieuChung = null;
        var thanTrang = new List<string>();
        foreach (var tv in thanhVienThuTu)
        {
            var ket = await DungHtmlLyLichCaNhan(tv.GiaoDanId, ct);
            if (ket is null) continue; // giáo dân bị xoá mềm sau khi gắn vào ThanhVienGiaDinh — bỏ qua, không chặn cả gia đình
            var (_, html) = ket.Value;
            var (kieu, than) = TachKieuVaThan(html);
            kieuChung ??= kieu;
            thanTrang.Add(than);
        }
        if (thanTrang.Count == 0) return null;

        var noiDungGop = string.Join(
            "<div style=\"page-break-after: always;\"></div>", thanTrang);
        var htmlGop = $"<!doctype html><html lang=\"vi\"><head><meta charset=\"utf-8\" />" +
            $"<style>{kieuChung}</style></head><body>{noiDungGop}</body></html>";
        var pdf = await trinhDuyet.XuatPdfAsync(htmlGop, ct);

        return new KetQuaInAn(pdf, $"LyLichCaNhan_GiaDinh_{giaDinh.MaGiaDinhCu}.pdf");
    }

    /// <summary>Tách khối <c>&lt;style&gt;</c> và nội dung bên trong <c>&lt;body&gt;</c> khỏi
    /// một trang HTML đã dựng xong (mẫu LyLichCaNhan.html không có thuộc tính nào trên hai thẻ
    /// này) — dùng để ghép nhiều trang "lý lịch cá nhân" độc lập thành MỘT tài liệu nhiều trang
    /// khi in cho cả gia đình (xem <see cref="XuatLyLichCaNhanGiaDinh"/>), không cần thêm một
    /// mẫu HTML thứ hai chỉ để lặp lại toàn bộ CSS/bố cục đã có.
    ///
    /// KHÔNG phải mẫu nào cũng có vỏ <c>&lt;body&gt;</c>: mẫu tuỳ chỉnh do giáo xứ/quản trị hệ
    /// thống lưu đã đi qua HtmlSanitizer, và <c>Sanitize()</c> trả về FRAGMENT (chỉ phần bên
    /// trong body, không còn <c>&lt;html&gt;/&lt;head&gt;/&lt;body&gt;</c>). Với fragment thì
    /// chính cả chuỗi là phần thân — chỉ bỏ khối <c>&lt;style&gt;</c> đã tách ra để CSS không bị
    /// lặp lại ở mỗi trang. Thiếu nhánh này, mọi giáo xứ ĐÃ tuỳ chỉnh mẫu "Lý lịch cá nhân" bấm
    /// in cả gia đình sẽ nhận về tờ giấy TRẮNG (regex không khớp → phần thân rỗng), trong khi in
    /// từng người vẫn đúng nên rất dễ bị tưởng nhầm là lỗi máy in — xem
    /// InAnGhepNhieuTrangTests.</summary>
    private static (string Kieu, string Than) TachKieuVaThan(string htmlDayDu)
    {
        const System.Text.RegularExpressions.RegexOptions motDong =
            System.Text.RegularExpressions.RegexOptions.Singleline;
        var kieu = System.Text.RegularExpressions.Regex.Match(
            htmlDayDu, "<style>(.*?)</style>", motDong).Groups[1].Value;
        var khopThan = System.Text.RegularExpressions.Regex.Match(htmlDayDu, "<body>(.*?)</body>", motDong);
        var than = khopThan.Success
            ? khopThan.Groups[1].Value
            : System.Text.RegularExpressions.Regex.Replace(htmlDayDu, "<style>.*?</style>", "", motDong);
        return (kieu, than);
    }

    /// <summary>Dựng HTML "Lý lịch cá nhân" đầy đủ cho MỘT giáo dân (chưa xuất PDF) — tách khỏi
    /// <see cref="XuatLyLichCaNhan"/> để dùng lại được ở <see cref="XuatLyLichCaNhanGiaDinh"/>
    /// (in cho cả gia đình, nhiều người/nhiều trang) mà không lặp lại toàn bộ logic dựng dữ
    /// liệu. Trả null nếu không tìm thấy giáo dân hoặc giáo xứ (giữ nguyên điều kiện gốc).</summary>
    private async Task<(GiaoDan G, string Html)?> DungHtmlLyLichCaNhan(Guid giaoDanId, CancellationToken ct)
    {
        // FirstOrDefaultAsync với điều kiện tường minh — KHÔNG dùng Find()/FindAsync() (bị cấm
        // trong mã nghiệp vụ vì bỏ qua bộ lọc giáo xứ toàn cục, xem CLAUDE.md).
        // ThenInclude(GiaDinh): nạp luôn gia đình đang tham gia để cấp các biến {{TenGiaDinh}},
        // {{SoHoKhau}}… mà mẫu gốc chưa dùng nhưng người dùng được phép chèn (xem
        // MauInCatalog.BienKhaDung). KHÔNG phải truy vấn N+1 mới: vẫn đúng MỘT truy vấn cho mỗi
        // giáo dân như trước, chỉ thêm phép nối bảng — quan trọng vì hàm này chạy LẶP cho từng
        // thành viên khi in lý lịch cả gia đình (XuatLyLichCaNhanGiaDinh).
        var g = await db.GiaoDan
            .Include(x => x.GiaoHo)
            .Include(x => x.GiaDinhThamGia).ThenInclude(tv => tv.GiaDinh)
            .FirstOrDefaultAsync(x => x.Id == giaoDanId && !x.DaXoa, ct);
        if (g is null) return null;

        var giaoXu = await LayGiaoXuHienTai(ct);
        if (giaoXu is null) return null;

        var thamGia = g.GiaDinhThamGia
            .OrderBy(tv => tv.VaiTro > VaiTroGiaDinh.Vo ? 1 : 0)
            .FirstOrDefault();
        var vaiTro = thamGia is null ? ""
            : thamGia.VaiTro == VaiTroGiaDinh.Chong ? "Chồng"
            : thamGia.VaiTro == VaiTroGiaDinh.Vo ? "Vợ" : "Con";

        // Hôn phối mới nhất (nếu có) — cùng cách LayHonPhoi tra cứu (không Find/FindAsync):
        // lấy id các hôn phối giáo dân này tham gia, rồi lấy liên kết "người kia" trong CÙNG một
        // truy vấn để tránh N+1, đúng ghi chú đã có ở GiaoDanService.LayHonPhoi.
        var honPhoiIds = await db.GiaoDanHonPhoi
            .Where(x => x.GiaoDanId == giaoDanId)
            .Select(x => x.HonPhoiId)
            .ToListAsync(ct);
        HonPhoi? honPhoi = null;
        string? tenVoChong = null;
        if (honPhoiIds.Count > 0)
        {
            honPhoi = await db.HonPhoi
                .Where(h => honPhoiIds.Contains(h.Id))
                .OrderByDescending(h => h.NgayHonPhoi)
                .FirstOrDefaultAsync(ct);
            if (honPhoi is not null)
            {
                var voChong = await db.GiaoDanHonPhoi
                    .Where(x => x.HonPhoiId == honPhoi.Id && x.GiaoDanId != giaoDanId)
                    .Select(x => new { x.GiaoDan!.TenThanh, x.GiaoDan.HoTen })
                    .FirstOrDefaultAsync(ct);
                tenVoChong = voChong is null ? null
                    : string.IsNullOrWhiteSpace(voChong.TenThanh) ? voChong.HoTen : voChong.TenThanh + " " + voChong.HoTen;
            }
        }

        // "Giáo họ cha" — giáo họ lẻ trực thuộc một giáo họ chính (GiaoHo.GiaoHoChaId, xem
        // GiaoHoConfig — quan hệ tự tham chiếu KHÔNG có navigation property CLR nên phải tự
        // truy vấn thêm một bước). Đúng logic ghép "{TenGiaoHoCha} - {TenGiaoHo}" của bản
        // desktop (Source/ExcelReport/ReportLyLichCaNhan.cs dòng 64-71) — chỉ ghép khi có giáo
        // họ cha VÀ tên khác giáo họ hiện tại (không phân biệt hoa/thường), tránh lặp thừa kiểu
        // "Giáo họ A - Giáo họ A" khi dữ liệu tự tham chiếu chính nó.
        var tenGiaoHo = g.GiaoHo?.TenGiaoHo ?? "Ngoài xứ";
        if (g.GiaoHo?.GiaoHoChaId is { } giaoHoChaId)
        {
            var tenGiaoHoCha = await db.GiaoHo
                .Where(h => h.Id == giaoHoChaId)
                .Select(h => h.TenGiaoHo)
                .FirstOrDefaultAsync(ct);
            if (!string.IsNullOrWhiteSpace(tenGiaoHoCha) &&
                !tenGiaoHoCha.Equals(tenGiaoHo, StringComparison.OrdinalIgnoreCase))
                tenGiaoHo = $"{tenGiaoHoCha} - {tenGiaoHo}";
        }

        // Linh mục chánh xứ ĐƯƠNG NHIỆM — cùng điều kiện bản desktop
        // (Source/GXControl/GxGiaoDanList.cs dòng 435: "ChucVu='Chánh xứ' AND DenNgay IS NULL").
        // ChucVu là chuỗi tự do nhưng luôn nhập qua combobox 3 giá trị cố định (frmLinhMuc.cs),
        // nên so khớp chuỗi chính xác là an toàn. Nếu có nhiều dòng khớp (dữ liệu lỗi hiếm gặp)
        // lấy dòng nhận xứ gần nhất — vẫn tốt hơn báo lỗi hay bỏ trống khi in giấy tờ thật.
        var chanhXu = await db.LinhMuc
            .Where(l => !l.DaXoa && l.ChucVu == "Chánh xứ" && l.DenNgay == null)
            .OrderByDescending(l => l.TuNgay)
            .FirstOrDefaultAsync(ct);
        var tenChanhXu = chanhXu is null ? null
            : string.IsNullOrWhiteSpace(chanhXu.TenThanh) ? chanhXu.HoTen : $"{chanhXu.TenThanh} {chanhXu.HoTen}";

        var hoTenDayDu = string.IsNullOrWhiteSpace(g.TenThanh) ? g.HoTen : $"{g.TenThanh} {g.HoTen}";

        // Nạp MỘT LẦN cho cả lượt in, kể cả khi hàm này chạy lặp cho từng thành viên gia đình —
        // xem ghi chú cạm bẫy hiệu năng ở _bangCachHienThi/LayBangCachHienThi.
        var bangCachHienThi = await LayBangCachHienThi(ct);

        var duLieu = new Dictionary<string, string?>(ThongTinGiaoXu(giaoXu))
        {
            ["TenGiaoHo"] = tenGiaoHo,
            ["MaGiaoDan"] = g.MaGiaoDanCu.ToString(),
            ["HoTen"] = hoTenDayDu,
            ["NgaySinh"] = Ngay(g.NgaySinh),
            ["NoiSinh"] = g.NoiSinh,
            ["Phai"] = g.Phai,
            ["VaiTro"] = vaiTro,
            ["TenCha"] = g.HoTenCha,
            ["TenMe"] = g.HoTenMe,

            ["DiaChiGiaoDan"] = g.DiaChi,
            ["DienThoaiGiaoDan"] = g.DienThoai,
            ["EmailGiaoDan"] = g.Email,
            ["DanToc"] = g.DanToc,
            ["NgheNghiep"] = g.NgheNghiep,
            ["TrinhDoVanHoa"] = g.TrinhDoVanHoa,
            ["TrinhDoChuyenMon"] = g.TrinhDoChuyenMon,
            ["BietNgoaiNgu"] = g.BietNgoaiNgu,

            // Câu ghép "Số X — ngày Y tại Z, cha A rửa, người đỡ đầu B" — bỏ hẳn đoạn nào
            // thiếu dữ liệu, KHÔNG để lại nhãn/nối rỗng lửng (xem VanBanInAn.MoTaBiTich, quy
            // tắc trình bày chung áp dụng mọi mẫu).
            ["MoTaRuaToi"] = VanBanInAn.MoTaBiTich(g.SoRuaToi, g.NgayRuaToi, g.NoiRuaToi,
                g.ChaRuaToi, "rửa", g.NguoiDoDauRuaToi, "người đỡ đầu"),
            ["MoTaRuocLe"] = VanBanInAn.MoTaBiTich(g.SoRuocLe, g.NgayRuocLe, g.NoiRuocLe,
                g.ChaRuocLe, "chủ sự"),
            ["MoTaThemSuc"] = VanBanInAn.MoTaBiTich(g.SoThemSuc, g.NgayThemSuc, g.NoiThemSuc,
                g.ChaThemSuc, "ban", g.NguoiDoDauThemSuc, "người đỡ đầu"),

            ["SoHonPhoi"] = honPhoi?.SoHonPhoi,
            ["VoChong"] = tenVoChong,
            ["NgayHonPhoi"] = Ngay(honPhoi?.NgayHonPhoi),
            ["NoiHonPhoi"] = honPhoi?.NoiHonPhoi,
            ["ChaHonPhoi"] = honPhoi?.LinhMucChung,
            ["CachThucHonPhoi"] = honPhoi?.CachThucHonPhoi,
            ["NguoiChung1"] = honPhoi?.NguoiChung1,
            ["NguoiChung2"] = honPhoi?.NguoiChung2,
            ["GhiChuHonPhoi"] = honPhoi?.GhiChu,

            ["TenChanhXu"] = tenChanhXu,

            ["ConHoc"] = bangCachHienThi.Dau("ConHoc", g.ConHoc),
            ["TanTong"] = bangCachHienThi.Dau("TanTong", g.TanTong),
            ["DaCoGiaDinh"] = bangCachHienThi.Dau("DaCoGiaDinh", g.DaCoGiaDinh),
            ["QuaDoi"] = bangCachHienThi.Dau("QuaDoi", g.QuaDoi),
            ["NgayQuaDoi"] = g.QuaDoi && g.NgayQuaDoi is not null ? $"— ngày {Ngay(g.NgayQuaDoi)}" : "",
            ["NoiAnTang"] = g.QuaDoi && !string.IsNullOrWhiteSpace(g.NoiAnTang) ? $"— an táng tại {g.NoiAnTang}" : "",
            ["SoAnTang"] = g.QuaDoi && !string.IsNullOrWhiteSpace(g.SoAnTang) ? $" (số {g.SoAnTang})" : "",

            ["NgayThangNamIn"] = DateTime.Now.ToString("dd/MM/yyyy"),
        };

        // --- Biến NGƯỜI DÙNG TỰ CHÈN (MauInCatalog.BienKhaDung) ------------------------------
        // Xem khối ghi chú ở các hàm NhanThanThem/RuaToiRoi/… phía trên: mẫu gốc KHÔNG dùng khoá
        // nào dưới đây nên kết quả in hiện tại không đổi, nhưng chúng là dữ liệu thật để giáo xứ
        // tự thêm dòng "CMND", "Ngày xức dầu", hay tách riêng "số sổ/ngày/nơi rửa tội".
        // KHÔNG truy vấn thêm lần nào — mọi giá trị lấy từ `g`/`giaDinh` đã nạp sẵn ở trên, quan
        // trọng vì hàm này chạy lặp cho từng thành viên khi in lý lịch cả gia đình.
        var giaDinh = thamGia?.GiaDinh;
        foreach (var kv in NhanThanThem(g)) duLieu[kv.Key] = kv.Value;
        foreach (var kv in RuaToiRoi(g)) duLieu[kv.Key] = kv.Value;
        foreach (var kv in RuocLeRoi(g)) duLieu[kv.Key] = kv.Value;
        foreach (var kv in ThemSucRoi(g)) duLieu[kv.Key] = kv.Value;
        foreach (var kv in GiaoLyThem(g)) duLieu[kv.Key] = kv.Value;
        foreach (var kv in new Dictionary<string, string?>
        {
            ["NoiQuaDoi"] = g.NoiQuaDoi,

            ["NgayXucDau"] = Ngay(g.NgayXucDau), ["NguoiXucDau"] = g.NguoiXucDau,
            ["TinhTrangXucDau"] = g.TinhTrangXucDau, ["GhiChuXucDau"] = g.GhiChuXucDau,

            ["TenGiaDinh"] = giaDinh?.TenGiaDinh,
            ["MaGiaDinh"] = giaDinh is null ? "" : MaGiaDinh(giaDinh),
            ["DiaChiGiaDinh"] = giaDinh?.DiaChi,
            ["DienThoaiGiaDinh"] = giaDinh?.DienThoai,
            ["SoHoKhau"] = giaDinh?.SoHoKhau,
            ["DienGiaDinh"] = giaDinh?.DienGiaDinh,
            ["GhiChuGiaDinh"] = giaDinh?.GhiChu,
        }) duLieu[kv.Key] = kv.Value;

        var khoiHtml = new Dictionary<string, string?>
        {
            ["KhoiAnh"] = KhoiAnhDaiDien(g.AnhDaiDienDuLieu, g.AnhDaiDienLoaiNoiDung),
        };
        var slug = BoDoMauIn.ChuanHoaTenGiaoPhan(giaoXu.GiaoHat?.GiaoPhan?.TenGiaoPhan);
        var html = await DungMau(slug, "LyLichCaNhan", duLieu, khoiHtml, ct);
        return (g, html);
    }

    /// <summary>Bốn mục menu chuột phải "In chứng nhận bí tích / rửa tội / xưng tội-rước lễ /
    /// thêm sức" (GxGiaoDanList.tsx) — tương đương Source/ExcelReport/ReportChungNhanBT.cs.
    /// Bản desktop chỉ đổi TÊN TỆP mẫu theo LoaiBiTich (mọi phép Replace chạy giống nhau cho cả
    /// 4 loại); bản web dùng CHUNG một mẫu HTML, chỉ đổi tiêu đề và dòng nào được liệt kê.
    /// <paramref name="loai"/> nhận "RuaToi"/"RuocLe"/"ThemSuc", còn lại (kể cả null) coi là
    /// "TatCa" (mục "In chứng nhận bí tích" chung, liệt kê đủ ba bí tích).
    ///
    /// CỐ Ý KHÔNG dựng phần "gửi giáo xứ nhận" (TenLinhMucNhan/TenGiaoXuNhan/LyDo của bản gốc)
    /// — đó là giấy CHUYỂN giáo xứ cần nhập tay lúc in (linh mục xứ nhận, lý do), chưa có màn
    /// hình nhập dữ liệu đó ở lượt này (xem can-review-sau.md).</summary>
    public async Task<KetQuaInAn?> XuatChungNhanBiTich(Guid giaoDanId, string? loai, CancellationToken ct)
    {
        var g = await db.GiaoDan.Include(x => x.GiaoHo)
            .FirstOrDefaultAsync(x => x.Id == giaoDanId && !x.DaXoa, ct);
        if (g is null) return null;

        var giaoXu = await LayGiaoXuHienTai(ct);
        if (giaoXu is null) return null;

        var moTaRuaToi = VanBanInAn.MoTaBiTich(g.SoRuaToi, g.NgayRuaToi, g.NoiRuaToi,
            g.ChaRuaToi, "rửa", g.NguoiDoDauRuaToi, "người đỡ đầu");
        var moTaRuocLe = VanBanInAn.MoTaBiTich(g.SoRuocLe, g.NgayRuocLe, g.NoiRuocLe, g.ChaRuocLe, "chủ sự");
        var moTaThemSuc = VanBanInAn.MoTaBiTich(g.SoThemSuc, g.NgayThemSuc, g.NoiThemSuc,
            g.ChaThemSuc, "ban", g.NguoiDoDauThemSuc, "người đỡ đầu");

        string DongNeuCo(string nhan, string moTa) => string.IsNullOrEmpty(moTa) ? "" : $"{nhan}: {moTa}";

        var (tieuDe, danhSach) = loai switch
        {
            "RuaToi" => ("CHỨNG NHẬN RỬA TỘI", VanBanInAn.GhepDong(DongNeuCo("Rửa tội", moTaRuaToi))),
            "RuocLe" => ("CHỨNG NHẬN XƯNG TỘI - RƯỚC LỄ LẦN ĐẦU",
                VanBanInAn.GhepDong(DongNeuCo("Rước lễ lần đầu", moTaRuocLe))),
            "ThemSuc" => ("CHỨNG NHẬN THÊM SỨC", VanBanInAn.GhepDong(DongNeuCo("Thêm sức", moTaThemSuc))),
            _ => ("CHỨNG NHẬN CÁC BÍ TÍCH", VanBanInAn.GhepDong(
                DongNeuCo("Rửa tội", moTaRuaToi),
                DongNeuCo("Rước lễ lần đầu", moTaRuocLe),
                DongNeuCo("Thêm sức", moTaThemSuc))),
        };

        var hoTenDayDu = string.IsNullOrWhiteSpace(g.TenThanh) ? g.HoTen : $"{g.TenThanh} {g.HoTen}";
        var duLieu = new Dictionary<string, string?>(ThongTinGiaoXu(giaoXu))
        {
            ["TieuDeBiTich"] = tieuDe,
            ["TenGiaoHo"] = g.GiaoHo?.TenGiaoHo ?? "Ngoài xứ",
            ["HoTen"] = hoTenDayDu,
            ["NgaySinh"] = Ngay(g.NgaySinh),
            ["NoiSinh"] = g.NoiSinh,
            ["TenCha"] = g.HoTenCha,
            ["TenMe"] = g.HoTenMe,
            ["DiaChiGiaoDan"] = g.DiaChi,
            ["DanhSachBiTich"] = danhSach,
            ["NgayThangNamIn"] = DateTime.Now.ToString("dd/MM/yyyy"),
        };
        // Biến người dùng tự chèn — xem ghi chú ở DungHtmlLyLichCaNhan. Không truy vấn thêm.
        foreach (var kv in NhanThanThem(g)) duLieu[kv.Key] = kv.Value;
        foreach (var kv in RuaToiRoi(g)) duLieu[kv.Key] = kv.Value;
        foreach (var kv in RuocLeRoi(g)) duLieu[kv.Key] = kv.Value;
        foreach (var kv in ThemSucRoi(g)) duLieu[kv.Key] = kv.Value;
        duLieu["MoTaRuaToi"] = moTaRuaToi;
        duLieu["MoTaRuocLe"] = moTaRuocLe;
        duLieu["MoTaThemSuc"] = moTaThemSuc;

        var slug = BoDoMauIn.ChuanHoaTenGiaoPhan(giaoXu.GiaoHat?.GiaoPhan?.TenGiaoPhan);
        var html = await DungMau(slug, "ChungNhanBiTich", duLieu, null, ct);
        var pdf = await trinhDuyet.XuatPdfAsync(html, ct);

        return new KetQuaInAn(pdf, $"ChungNhanBiTich_{g.MaGiaoDanCu}.pdf");
    }

    /// <summary>"In phiếu gia đình" (nút ở GiaDinhDetail.tsx, mục cùng tên trên menu chuột phải
    /// của GxGiaDinhList.tsx) — tương đương Source/ExcelReport/ReportSoGiaDinh.cs. Khác bản
    /// desktop (một bảng Word cố định 8 dòng, chèn thêm dòng khi vượt quá): bản web dựng bảng
    /// HTML co giãn theo đúng số thành viên thật, không cần logic "AddRow" phức tạp.
    ///
    /// Mỗi dòng thành viên là một khối HTML tự dựng (KHÔNG qua {{Key}} thông thường vì số dòng
    /// không cố định) — MỌI dữ liệu người dùng chèn vào khối đó đều tự
    /// <c>HtmlEncoder.Default.Encode</c> trước, xem ghi chú ở BoDoMauIn.Dung.
    ///
    /// <paramref name="khoGiay"/> ("A4" mặc định, hoặc "A3") — bổ sung cho yêu cầu
    /// `PhieuGiaDinh-A3.doc` (mục 2 nhiệm vụ, gia đình đông người, khổ A4 không đủ chỗ, xem
    /// in-an.md mục 5c/8). CỐ Ý dùng lại NGUYÊN VẸN mẫu HTML "PhieuGiaDinh" (không dựng mẫu
    /// riêng cho A3) — bảng thành viên đã co giãn theo số người thật (không phải 8 dòng cố định
    /// như bản desktop), nên khác biệt A3 so với A4 chỉ nằm ở khổ giấy Playwright xuất ra
    /// (nhiều chỗ trống hơn, KHÔNG phải bố cục khác) — đúng tinh thần "chỉ khác khổ giấy và số
    /// cột" mà nhiệm vụ này chỉ đạo tái dùng tối đa.</summary>
    public async Task<KetQuaInAn?> XuatPhieuGiaDinh(Guid giaDinhId, string khoGiay, CancellationToken ct)
    {
        var giaDinh = await db.GiaDinh
            .Include(x => x.ThanhVien).ThenInclude(tv => tv.GiaoDan)
            .Include(x => x.GiaoHo)
            .FirstOrDefaultAsync(x => x.Id == giaDinhId && !x.DaXoa, ct);
        if (giaDinh is null) return null;

        var giaoXu = await LayGiaoXuHienTai(ct);
        if (giaoXu is null) return null;

        // Hôn phối "hiện tại" của cặp chồng/vợ trong gia đình — gọi lại đúng logic đã có ở
        // GiaDinhService (xem ghi chú LayVoChongVaHonPhoi), không tự dò lại từ đầu.
        var voChongHonPhoi = await giaDinhDv.LayVoChongVaHonPhoi(giaDinhId, ct);
        var honPhoi = voChongHonPhoi?.HonPhoi;
        var moTaHonPhoi = honPhoi is null ? "" : VanBanInAn.MoTaBiTich(
            honPhoi.SoHonPhoi, honPhoi.NgayHonPhoi, honPhoi.NoiHonPhoi, honPhoi.LinhMucChung, "chủ sự");

        var hangHtml = new System.Text.StringBuilder();
        var stt = 1;
        foreach (var tv in giaDinh.ThanhVien.OrderBy(x => x.VaiTro).ThenBy(x => x.GiaoDan!.NgaySinh))
        {
            var gd = tv.GiaoDan!;
            var vaiTroChu = tv.ChuHo ? "Chủ hộ"
                : tv.VaiTro == VaiTroGiaDinh.Chong ? "Chồng"
                : tv.VaiTro == VaiTroGiaDinh.Vo ? "Vợ" : "Con";
            var moTaRuaToi = VanBanInAn.MoTaBiTich(gd.SoRuaToi, gd.NgayRuaToi, gd.NoiRuaToi,
                gd.ChaRuaToi, "rửa", gd.NguoiDoDauRuaToi, "người đỡ đầu");
            var moTaRuocLe = VanBanInAn.MoTaBiTich(gd.SoRuocLe, gd.NgayRuocLe, gd.NoiRuocLe, gd.ChaRuocLe, "chủ sự");
            var moTaThemSuc = VanBanInAn.MoTaBiTich(gd.SoThemSuc, gd.NgayThemSuc, gd.NoiThemSuc,
                gd.ChaThemSuc, "ban", gd.NguoiDoDauThemSuc, "người đỡ đầu");
            var moTaHonPhoiDong = tv.VaiTro is VaiTroGiaDinh.Chong or VaiTroGiaDinh.Vo ? moTaHonPhoi : "";
            // Gạch ngang đỏ nhạt cho người đã qua đời — cùng tinh thần "Gạch ngang" của lưới
            // thành viên trên web (GxGiaoDanList quanHeGiaDinh) và bản desktop
            // (word.StrikeThroughRow khi QuaDoi=true).
            var kieuGach = gd.QuaDoi ? " style=\"text-decoration: line-through; color:#777;\"" : "";

            string E(string? s) => System.Text.Encodings.Web.HtmlEncoder.Default.Encode(s ?? "");

            hangHtml.Append("<tr").Append(kieuGach).Append('>')
                .Append("<td class=\"stt\">").Append(stt++).Append("</td>")
                .Append("<td>").Append(E(gd.TenThanh)).Append("</td>")
                .Append("<td>").Append(E(gd.HoTen)).Append("</td>")
                .Append("<td>").Append(E(Ngay(gd.NgaySinh))).Append("</td>")
                .Append("<td>").Append(E(gd.NoiSinh)).Append("</td>")
                .Append("<td>").Append(E(gd.Phai)).Append("</td>")
                .Append("<td>").Append(E(vaiTroChu)).Append("</td>")
                .Append("<td class=\"thong-tin-bi-tich\">").Append(E(moTaRuaToi)).Append("</td>")
                .Append("<td class=\"thong-tin-bi-tich\">").Append(E(moTaRuocLe)).Append("</td>")
                .Append("<td class=\"thong-tin-bi-tich\">").Append(E(moTaThemSuc)).Append("</td>")
                .Append("<td class=\"thong-tin-bi-tich\">").Append(E(moTaHonPhoiDong)).Append("</td>")
                .Append("<td></td>")
                .Append("</tr>");
        }

        var duLieu = new Dictionary<string, string?>(ThongTinGiaoXu(giaoXu))
        {
            ["MaGiaDinh"] = MaGiaDinh(giaDinh),
            ["TenGiaDinh"] = giaDinh.TenGiaDinh,
            ["TenGiaoHo"] = giaDinh.GiaoHo?.TenGiaoHo ?? "Ngoài xứ",
            ["DienThoaiGiaDinh"] = giaDinh.DienThoai,
            ["DiaChiGiaDinh"] = giaDinh.DiaChi,
            ["MoTaHonPhoi"] = moTaHonPhoi,
            ["GhiChuGiaDinh"] = giaDinh.GhiChu,
            ["NgayThangNamIn"] = DateTime.Now.ToString("dd/MM/yyyy"),
        };
        // Biến người dùng tự chèn — xem ghi chú ở khối helper "Biến NGƯỜI DÙNG TỰ CHÈN".
        foreach (var kv in GiaDinhThem(giaDinh, giaDinh.ThanhVien.Count, await LayBangCachHienThi(ct))) duLieu[kv.Key] = kv.Value;
        foreach (var kv in HonPhoiRoi(honPhoi)) duLieu[kv.Key] = kv.Value;

        var khoiHtml = new Dictionary<string, string?>
        {
            ["HangThanhVien"] = hangHtml.ToString(),
            ["KhoiAnh"] = KhoiAnhDaiDien(giaDinh.AnhDaiDienDuLieu, giaDinh.AnhDaiDienLoaiNoiDung),
        };

        var slug = BoDoMauIn.ChuanHoaTenGiaoPhan(giaoXu.GiaoHat?.GiaoPhan?.TenGiaoPhan);
        var html = await DungMau(slug, "PhieuGiaDinh", duLieu, khoiHtml, ct);
        var pdf = await trinhDuyet.XuatPdfAsync(html, ct, khoGiay: khoGiay);

        var hauTo = khoGiay == "A3" ? "_A3" : "";
        return new KetQuaInAn(pdf, $"PhieuGiaDinh_{giaDinh.MaGiaDinhCu}{hauTo}.pdf");
    }

    /// <summary>Bốn cột cuối (MaGiaoDanCu/CMND/DiaChi/DienThoai) thêm ở lượt công bố
    /// <see cref="Printing.MauInCatalog"/>.BienKhaDung — chỉ là thêm cột vào CÙNG một phép chiếu
    /// đã có, KHÔNG thêm truy vấn nào.</summary>
    private sealed record NguoiHonPhoi(
        Guid Id, string? Phai, string? TenThanh, string HoTen, DateOnly? NgaySinh, string? NoiSinh,
        string? HoTenCha, string? HoTenMe, DateOnly? NgayRuaToi, string? NoiRuaToi, string? SoRuaToi,
        string? ChaRuaToi, string? NguoiDoDauRuaToi, DateOnly? NgayThemSuc, string? NoiThemSuc,
        string? SoThemSuc, string? ChaThemSuc, string? NguoiDoDauThemSuc, string? TenGiaoHo,
        int MaGiaoDanCu, string? CMND, string? DiaChi, string? DienThoai);

    /// <summary>"In chứng nhận hôn phối" (menu chuột phải GxGiaDinhList.tsx) — tương đương
    /// Source/ExcelReport/ReportChungNhanHP.cs. Trả null nếu gia đình không tồn tại HOẶC chưa
    /// có hôn phối nào để chứng nhận (chưa gán Người nam/Người nữ, hoặc chưa ghi khối hôn
    /// phối) — không có gì để in trong trường hợp đó.
    ///
    /// Nam/Nữ xác định theo GiaoDan.Phai THẬT của từng người tham gia hôn phối (không giả định
    /// "Chồng" luôn ứng với "Nam" — vai trò Chồng/Vợ trong ThanhVienGiaDinh và Nam/Nữ trên giấy
    /// chứng nhận là hai khái niệm khác nhau, dữ liệu có thể lệch khi nhập tay).</summary>
    public async Task<KetQuaInAn?> XuatChungNhanHonPhoi(Guid giaDinhId, CancellationToken ct)
    {
        var giaoXu = await LayGiaoXuHienTai(ct);
        if (giaoXu is null) return null;

        var voChongHonPhoi = await giaDinhDv.LayVoChongVaHonPhoi(giaDinhId, ct);
        if (voChongHonPhoi is null) return null;
        var honPhoi = voChongHonPhoi.Value.HonPhoi;
        if (honPhoi is null) return null;

        var haiNguoi = await db.GiaoDanHonPhoi
            .Where(x => x.HonPhoiId == honPhoi.Id)
            .Select(x => new NguoiHonPhoi(
                x.GiaoDan!.Id, x.GiaoDan.Phai, x.GiaoDan.TenThanh, x.GiaoDan.HoTen, x.GiaoDan.NgaySinh,
                x.GiaoDan.NoiSinh, x.GiaoDan.HoTenCha, x.GiaoDan.HoTenMe,
                x.GiaoDan.NgayRuaToi, x.GiaoDan.NoiRuaToi, x.GiaoDan.SoRuaToi, x.GiaoDan.ChaRuaToi,
                x.GiaoDan.NguoiDoDauRuaToi, x.GiaoDan.NgayThemSuc, x.GiaoDan.NoiThemSuc,
                x.GiaoDan.SoThemSuc, x.GiaoDan.ChaThemSuc, x.GiaoDan.NguoiDoDauThemSuc,
                x.GiaoDan.GiaoHo != null ? x.GiaoDan.GiaoHo.TenGiaoHo : null,
                x.GiaoDan.MaGiaoDanCu, x.GiaoDan.CMND, x.GiaoDan.DiaChi, x.GiaoDan.DienThoai))
            .ToListAsync(ct);
        if (haiNguoi.Count == 0) return null;

        var nam = haiNguoi.FirstOrDefault(x => x.Phai == "Nam") ?? haiNguoi[0];
        var nu = haiNguoi.FirstOrDefault(x => x.Id != nam.Id);

        static string HoTen(NguoiHonPhoi? n) => n is null ? ""
            : string.IsNullOrWhiteSpace(n.TenThanh) ? n.HoTen : $"{n.TenThanh} {n.HoTen}";
        string MoTaRuaToi(NguoiHonPhoi? n) => n is null ? "" : VanBanInAn.MoTaBiTich(
            n.SoRuaToi, n.NgayRuaToi, n.NoiRuaToi, n.ChaRuaToi, "rửa", n.NguoiDoDauRuaToi, "người đỡ đầu");
        string MoTaThemSuc(NguoiHonPhoi? n) => n is null ? "" : VanBanInAn.MoTaBiTich(
            n.SoThemSuc, n.NgayThemSuc, n.NoiThemSuc, n.ChaThemSuc, "ban", n.NguoiDoDauThemSuc, "người đỡ đầu");

        var duLieu = new Dictionary<string, string?>(ThongTinGiaoXu(giaoXu))
        {
            ["HoTenNam"] = HoTen(nam), ["HoTenNu"] = HoTen(nu),
            ["NgaySinhNam"] = Ngay(nam.NgaySinh), ["NgaySinhNu"] = Ngay(nu?.NgaySinh),
            ["NoiSinhNam"] = nam.NoiSinh, ["NoiSinhNu"] = nu?.NoiSinh,
            ["TenChaNam"] = nam.HoTenCha, ["TenChaNu"] = nu?.HoTenCha,
            ["TenMeNam"] = nam.HoTenMe, ["TenMeNu"] = nu?.HoTenMe,
            ["GiaoHoNam"] = nam.TenGiaoHo ?? "Ngoài xứ", ["GiaoHoNu"] = nu?.TenGiaoHo ?? "Ngoài xứ",
            ["MoTaRuaToiNam"] = MoTaRuaToi(nam), ["MoTaRuaToiNu"] = MoTaRuaToi(nu),
            ["MoTaThemSucNam"] = MoTaThemSuc(nam), ["MoTaThemSucNu"] = MoTaThemSuc(nu),

            ["SoHonPhoi"] = honPhoi.SoHonPhoi,
            ["NgayHonPhoi"] = Ngay(honPhoi.NgayHonPhoi),
            ["NoiHonPhoi"] = honPhoi.NoiHonPhoi,
            ["ChaHonPhoi"] = honPhoi.LinhMucChung,
            ["CachThucHonPhoi"] = honPhoi.CachThucHonPhoi,
            ["NguoiChung1"] = honPhoi.NguoiChung1,
            ["NguoiChung2"] = honPhoi.NguoiChung2,
            ["NgayThangNamIn"] = DateTime.Now.ToString("dd/MM/yyyy"),
        };

        // Biến người dùng tự chèn — xem ghi chú ở khối helper "Biến NGƯỜI DÙNG TỰ CHÈN". Mọi giá
        // trị lấy từ phép chiếu `haiNguoi`/`honPhoi` đã nạp, không truy vấn thêm.
        foreach (var kv in new Dictionary<string, string?>
        {
            ["TenHonPhoi"] = honPhoi.TenHonPhoi,
            ["GhiChuHonPhoi"] = honPhoi.GhiChu,

            ["MaGiaoDanNam"] = nam.MaGiaoDanCu.ToString(),
            ["MaGiaoDanNu"] = nu?.MaGiaoDanCu.ToString(),
            ["CMNDNam"] = nam.CMND, ["CMNDNu"] = nu?.CMND,
            ["DiaChiNam"] = nam.DiaChi, ["DiaChiNu"] = nu?.DiaChi,
            ["DienThoaiNam"] = nam.DienThoai, ["DienThoaiNu"] = nu?.DienThoai,

            ["SoRuaToiNam"] = nam.SoRuaToi, ["SoRuaToiNu"] = nu?.SoRuaToi,
            ["NgayRuaToiNam"] = Ngay(nam.NgayRuaToi), ["NgayRuaToiNu"] = Ngay(nu?.NgayRuaToi),
            ["NoiRuaToiNam"] = nam.NoiRuaToi, ["NoiRuaToiNu"] = nu?.NoiRuaToi,
            ["ChaRuaToiNam"] = nam.ChaRuaToi, ["ChaRuaToiNu"] = nu?.ChaRuaToi,
            ["NguoiDoDauRuaToiNam"] = nam.NguoiDoDauRuaToi, ["NguoiDoDauRuaToiNu"] = nu?.NguoiDoDauRuaToi,

            ["SoThemSucNam"] = nam.SoThemSuc, ["SoThemSucNu"] = nu?.SoThemSuc,
            ["NgayThemSucNam"] = Ngay(nam.NgayThemSuc), ["NgayThemSucNu"] = Ngay(nu?.NgayThemSuc),
            ["NoiThemSucNam"] = nam.NoiThemSuc, ["NoiThemSucNu"] = nu?.NoiThemSuc,
            ["ChaThemSucNam"] = nam.ChaThemSuc, ["ChaThemSucNu"] = nu?.ChaThemSuc,
            ["NguoiDoDauThemSucNam"] = nam.NguoiDoDauThemSuc, ["NguoiDoDauThemSucNu"] = nu?.NguoiDoDauThemSuc,
        }) duLieu[kv.Key] = kv.Value;

        var slug = BoDoMauIn.ChuanHoaTenGiaoPhan(giaoXu.GiaoHat?.GiaoPhan?.TenGiaoPhan);
        var html = await DungMau(slug, "ChungNhanHonPhoi", duLieu, null, ct);
        var pdf = await trinhDuyet.XuatPdfAsync(html, ct);

        return new KetQuaInAn(pdf, $"ChungNhanHonPhoi_{honPhoi.MaHonPhoiCu}.pdf");
    }

    // --- Giấy giới thiệu (4 mẫu) --------------------------------------------------------
    //
    // Bản desktop giải quyết vấn đề "thông tin bên thứ hai" (giáo xứ/giáo phận NHẬN, thường
    // không có trong CSDL vì thuộc giáo xứ khác) bằng một MÀN HÌNH NHẬP TAY — `frmReport.cs`
    // (Source/GXControl/) — hai ô nhập tự do `txtGiaoPhan`/`txtGiaoXu` (KHÔNG chọn từ danh mục
    // Giáo xứ có sẵn) cộng một combobox chọn linh mục ký tên (`cbLinhMuc`, tải từ
    // `SqlConstants.SELECT_LINHMUC_LIST`) — xem `RpGioiThieuBase.Export`/`SetForGiaoXuNhan`.
    // Bản web tái hiện đúng cơ chế đó: hai ô nhập tự do (`giaoPhan2`/`giaoXu2`) + một ô nhập tên
    // linh mục (`tenLinhMuc`, cũng CHỈ nhập tự do — xem can-review-sau.md, các trường "cha …"
    // khác của GiaoDan như ChaRuaToi/ChaThemSuc cũng đều là chuỗi tự do, không có bảng LinhMuc
    // nào được chọn qua danh mục ở bản web hiện tại).
    //
    // CHÚ Ý: có HAI cài đặt "giấy giới thiệu giáo lý hôn phối" khác nhau trong mã nguồn desktop
    // — `frmReportGioiThieuHP.cs`/`ReportGioiThieuHP.cs` (cũ hơn, tự chèn một bản ghi GiaoDan
    // MỚI cho người thứ hai từ dữ liệu gõ tay) và `frmReport.cs`+`RpGThieuGlyHPhoi.cs` (nhất
    // quán với 3 mẫu giới thiệu còn lại, dùng CHUNG `RpGioiThieuBase`). ĐỐI CHIẾU TOÀN BỘ
    // `Source/` không tìm thấy bất kỳ chỗ nào gọi `new frmReportGioiThieuHP(...)` hay
    // `new frmReport(...)` — CẢ HAI đường dẫn đều là mã CHẾT, không có mục menu nào trên bản
    // desktop thật sự gọi tới (xem can-review-sau.md mục mới). Đã CHỌN mô hình `frmReport` (tự
    // nhập, không tạo GiaoDan giả) làm chuẩn để dựng lại — vừa nhất quán với 3 mẫu kia, vừa
    // tránh side-effect ghi một bản ghi GiaoDan "ma" vào CSDL chỉ để in một tờ giấy.

    private async Task<GiaoDan?> LayGiaoDanChoGioiThieu(Guid giaoDanId, CancellationToken ct) =>
        await db.GiaoDan.Include(x => x.GiaoHo).FirstOrDefaultAsync(x => x.Id == giaoDanId && !x.DaXoa, ct);

    /// <summary>Các chỗ trống giống nhau ở CẢ BỐN mẫu giấy giới thiệu (bên nhận + linh mục ký
    /// tên) — một chỗ sửa áp dụng chung, xem ghi chú ở khối "Giấy giới thiệu" phía trên.</summary>
    private static Dictionary<string, string?> ThongTinBenNhan(string? giaoPhan2, string? giaoXu2, string? tenLinhMuc) => new()
    {
        ["TenGiaoPhan2"] = giaoPhan2,
        ["TenGiaoXu2"] = giaoXu2,
        ["TenLinhMuc"] = tenLinhMuc,
        ["NgayThangNamIn"] = DateTime.Now.ToString("dd/MM/yyyy"),
    };

    /// <summary>"In giấy giới thiệu chứng nhận rửa tội" (menu chuột phải GxGiaoDanList.tsx) —
    /// tương đương Source/GXControl/ReportGioiThieuRuaToi.cs (qua RpGioiThieuBase.Export).</summary>
    public async Task<KetQuaInAn?> XuatGioiThieuRuaToi(
        Guid giaoDanId, string? giaoPhan2, string? giaoXu2, string? tenLinhMuc, CancellationToken ct)
    {
        var g = await LayGiaoDanChoGioiThieu(giaoDanId, ct);
        if (g is null) return null;
        var giaoXu = await LayGiaoXuHienTai(ct);
        if (giaoXu is null) return null;

        var hoTenDayDu = string.IsNullOrWhiteSpace(g.TenThanh) ? g.HoTen : $"{g.TenThanh} {g.HoTen}";
        var duLieu = new Dictionary<string, string?>(ThongTinGiaoXu(giaoXu))
        {
            ["HoTen"] = hoTenDayDu,
            ["NgaySinh"] = Ngay(g.NgaySinh),
            ["NoiSinh"] = g.NoiSinh,
            ["TenCha"] = g.HoTenCha,
            ["TenMe"] = g.HoTenMe,
            ["DiaChiGiaoDan"] = g.DiaChi,
            ["DienThoaiGiaoDan"] = g.DienThoai,
        };
        foreach (var kv in ThongTinBenNhan(giaoPhan2, giaoXu2, tenLinhMuc)) duLieu[kv.Key] = kv.Value;
        // Biến người dùng tự chèn — xem ghi chú ở khối helper "Biến NGƯỜI DÙNG TỰ CHÈN".
        foreach (var kv in NhanThanThem(g)) duLieu[kv.Key] = kv.Value;
        foreach (var kv in RuaToiRoi(g)) duLieu[kv.Key] = kv.Value;
        duLieu["TenGiaoHo"] = g.GiaoHo?.TenGiaoHo ?? "Ngoài xứ";
        duLieu["MoTaRuaToi"] = VanBanInAn.MoTaBiTich(g.SoRuaToi, g.NgayRuaToi, g.NoiRuaToi,
            g.ChaRuaToi, "rửa", g.NguoiDoDauRuaToi, "người đỡ đầu");

        var slug = BoDoMauIn.ChuanHoaTenGiaoPhan(giaoXu.GiaoHat?.GiaoPhan?.TenGiaoPhan);
        var html = await DungMau(slug, "GioiThieuRuaToi", duLieu, null, ct);
        var pdf = await trinhDuyet.XuatPdfAsync(html, ct);
        return new KetQuaInAn(pdf, $"GioiThieuRuaToi_{g.MaGiaoDanCu}.pdf");
    }

    /// <summary>"In giấy giới thiệu chứng nhận thêm sức" — tương đương
    /// Source/GXControl/RpGioiThieuThemSuc.cs. Dòng "Đã Rửa tội" dùng
    /// <see cref="VanBanInAn.MoTaBiTich"/> (bỏ hẳn đoạn thiếu dữ liệu) THAY vì literal
    /// "..................." mà bản desktop in khi thiếu NgayRuaToi — ĐÂY LÀ SAI KHÁC CÓ CHỦ
    /// ĐÍCH, không phải migrate y hệt: nhiệm vụ này chỉ đạo rõ dùng lại đúng cơ chế VanBanInAn
    /// để không tái diễn kiểu trình bày cẩu thả trên giấy tờ chính thức (xem can-review-sau.md),
    /// và literal "..." khi có NoiRuaToi nhưng thiếu NgayRuaToi sẽ tạo ra một dòng còn xấu hơn cả
    /// lỗi dấu phẩy lửng đã sửa ở LyLichCaNhan.</summary>
    public async Task<KetQuaInAn?> XuatGioiThieuThemSuc(
        Guid giaoDanId, string? giaoPhan2, string? giaoXu2, string? tenLinhMuc, CancellationToken ct)
    {
        var g = await LayGiaoDanChoGioiThieu(giaoDanId, ct);
        if (g is null) return null;
        var giaoXu = await LayGiaoXuHienTai(ct);
        if (giaoXu is null) return null;

        var hoTenDayDu = string.IsNullOrWhiteSpace(g.TenThanh) ? g.HoTen : $"{g.TenThanh} {g.HoTen}";
        var duLieu = new Dictionary<string, string?>(ThongTinGiaoXu(giaoXu))
        {
            ["HoTen"] = hoTenDayDu,
            ["NgaySinh"] = Ngay(g.NgaySinh),
            ["NoiSinh"] = g.NoiSinh,
            ["TenCha"] = g.HoTenCha,
            ["TenMe"] = g.HoTenMe,
            ["MoTaRuaToi"] = VanBanInAn.MoTaBiTich(g.SoRuaToi, g.NgayRuaToi, g.NoiRuaToi,
                g.ChaRuaToi, "rửa", g.NguoiDoDauRuaToi, "người đỡ đầu"),
        };
        foreach (var kv in ThongTinBenNhan(giaoPhan2, giaoXu2, tenLinhMuc)) duLieu[kv.Key] = kv.Value;
        // Biến người dùng tự chèn — xem ghi chú ở khối helper "Biến NGƯỜI DÙNG TỰ CHÈN".
        foreach (var kv in NhanThanThem(g)) duLieu[kv.Key] = kv.Value;
        foreach (var kv in RuaToiRoi(g)) duLieu[kv.Key] = kv.Value;
        foreach (var kv in ThemSucRoi(g)) duLieu[kv.Key] = kv.Value;
        duLieu["TenGiaoHo"] = g.GiaoHo?.TenGiaoHo ?? "Ngoài xứ";
        duLieu["DiaChiGiaoDan"] = g.DiaChi;
        duLieu["MoTaThemSuc"] = VanBanInAn.MoTaBiTich(g.SoThemSuc, g.NgayThemSuc, g.NoiThemSuc,
            g.ChaThemSuc, "ban", g.NguoiDoDauThemSuc, "người đỡ đầu");

        var slug = BoDoMauIn.ChuanHoaTenGiaoPhan(giaoXu.GiaoHat?.GiaoPhan?.TenGiaoPhan);
        var html = await DungMau(slug, "GioiThieuThemSuc", duLieu, null, ct);
        var pdf = await trinhDuyet.XuatPdfAsync(html, ct);
        return new KetQuaInAn(pdf, $"GioiThieuThemSuc_{g.MaGiaoDanCu}.pdf");
    }

    /// <summary>"In giấy giới thiệu giáo lý hôn phối" — tương đương
    /// Source/GXControl/RpGThieuGlyHPhoi.cs (được chọn làm chuẩn thay cho
    /// frmReportGioiThieuHP.cs/ReportGioiThieuHP.cs cũ hơn — xem ghi chú ở đầu khối "Giấy giới
    /// thiệu" phía trên, cả hai cài đặt gốc đều là mã chết trên bản desktop).</summary>
    public async Task<KetQuaInAn?> XuatGioiThieuGiaoLyHonPhoi(
        Guid giaoDanId, string? giaoPhan2, string? giaoXu2, string? tenLinhMuc, CancellationToken ct)
    {
        var g = await LayGiaoDanChoGioiThieu(giaoDanId, ct);
        if (g is null) return null;
        var giaoXu = await LayGiaoXuHienTai(ct);
        if (giaoXu is null) return null;

        var hoTenDayDu = string.IsNullOrWhiteSpace(g.TenThanh) ? g.HoTen : $"{g.TenThanh} {g.HoTen}";
        var duLieu = new Dictionary<string, string?>(ThongTinGiaoXu(giaoXu))
        {
            ["TenGiaoHo"] = g.GiaoHo?.TenGiaoHo ?? "Ngoài xứ",
            ["HoTen"] = hoTenDayDu,
            ["NgaySinh"] = Ngay(g.NgaySinh),
            ["NoiSinh"] = g.NoiSinh,
            ["TenCha"] = g.HoTenCha,
            ["TenMe"] = g.HoTenMe,
            ["DiaChiGiaoDan"] = g.DiaChi,
            ["DienThoaiGiaoDan"] = g.DienThoai,
            ["MoTaRuaToi"] = VanBanInAn.MoTaBiTich(g.SoRuaToi, g.NgayRuaToi, g.NoiRuaToi,
                g.ChaRuaToi, "rửa", g.NguoiDoDauRuaToi, "người đỡ đầu"),
            ["MoTaThemSuc"] = VanBanInAn.MoTaBiTich(g.SoThemSuc, g.NgayThemSuc, g.NoiThemSuc,
                g.ChaThemSuc, "ban", g.NguoiDoDauThemSuc, "người đỡ đầu"),
        };
        foreach (var kv in ThongTinBenNhan(giaoPhan2, giaoXu2, tenLinhMuc)) duLieu[kv.Key] = kv.Value;
        // Biến người dùng tự chèn — xem ghi chú ở khối helper "Biến NGƯỜI DÙNG TỰ CHÈN".
        foreach (var kv in NhanThanThem(g)) duLieu[kv.Key] = kv.Value;
        foreach (var kv in RuaToiRoi(g)) duLieu[kv.Key] = kv.Value;
        foreach (var kv in ThemSucRoi(g)) duLieu[kv.Key] = kv.Value;
        foreach (var kv in GiaoLyThem(g)) duLieu[kv.Key] = kv.Value;

        var slug = BoDoMauIn.ChuanHoaTenGiaoPhan(giaoXu.GiaoHat?.GiaoPhan?.TenGiaoPhan);
        var html = await DungMau(slug, "GioiThieuGiaoLyHonPhoi", duLieu, null, ct);
        var pdf = await trinhDuyet.XuatPdfAsync(html, ct);
        return new KetQuaInAn(pdf, $"GioiThieuGiaoLyHonPhoi_{g.MaGiaoDanCu}.pdf");
    }

    /// <summary>"In giới thiệu chuyển xứ" (menu chuột phải GxGiaDinhList.tsx, theo GIA ĐÌNH chứ
    /// không phải từng giáo dân) — tương đương Source/GXControl/RpGioiThieuChuyenXu.cs (bảng
    /// thành viên co giãn theo số người thật, cùng cách làm với XuatPhieuGiaDinh — khác bản
    /// desktop tính toán AddRow/DeleteRow cố định 7 dòng trên mẫu Word).</summary>
    public async Task<KetQuaInAn?> XuatGioiThieuChuyenXu(
        Guid giaDinhId, string? giaoPhan2, string? giaoXu2, string? tenLinhMuc, CancellationToken ct)
    {
        // Include(GiaoHo) thêm ở lượt này CHỈ để cấp biến {{TenGiaoHo}} người dùng được phép
        // chèn — vẫn đúng MỘT truy vấn như trước (thêm phép nối, không phải N+1).
        var giaDinh = await db.GiaDinh
            .Include(x => x.ThanhVien).ThenInclude(tv => tv.GiaoDan)
            .Include(x => x.GiaoHo)
            .FirstOrDefaultAsync(x => x.Id == giaDinhId && !x.DaXoa, ct);
        if (giaDinh is null) return null;

        var giaoXu = await LayGiaoXuHienTai(ct);
        if (giaoXu is null) return null;

        var chuHo = giaDinh.ThanhVien.FirstOrDefault(tv => tv.ChuHo) ?? giaDinh.ThanhVien.FirstOrDefault();
        var tenChuHo = chuHo?.GiaoDan is null ? ""
            : string.IsNullOrWhiteSpace(chuHo.GiaoDan.TenThanh) ? chuHo.GiaoDan.HoTen : $"{chuHo.GiaoDan.TenThanh} {chuHo.GiaoDan.HoTen}";

        var hangHtml = new System.Text.StringBuilder();
        var stt = 1;
        foreach (var tv in giaDinh.ThanhVien.OrderBy(x => x.VaiTro).ThenBy(x => x.GiaoDan!.NgaySinh))
        {
            var gd = tv.GiaoDan!;
            var vaiTroChu = tv.ChuHo ? "Chủ hộ"
                : tv.VaiTro == VaiTroGiaDinh.Chong ? "Chồng"
                : tv.VaiTro == VaiTroGiaDinh.Vo ? "Vợ" : "Con";

            string E(string? s) => System.Text.Encodings.Web.HtmlEncoder.Default.Encode(s ?? "");

            hangHtml.Append("<tr>")
                .Append("<td class=\"stt\">").Append(stt++).Append("</td>")
                .Append("<td>").Append(E(gd.TenThanh)).Append("</td>")
                .Append("<td>").Append(E(gd.HoTen)).Append("</td>")
                .Append("<td>").Append(E(Ngay(gd.NgaySinh))).Append("</td>")
                .Append("<td>").Append(E(gd.NoiSinh)).Append("</td>")
                .Append("<td>").Append(E(vaiTroChu)).Append("</td>")
                .Append("</tr>");
        }

        var duLieu = new Dictionary<string, string?>(ThongTinGiaoXu(giaoXu))
        {
            ["TenChuHo"] = tenChuHo,
            ["DienThoaiGiaDinh"] = giaDinh.DienThoai,
            ["DiaChiGiaDinh"] = giaDinh.DiaChi,
        };
        foreach (var kv in ThongTinBenNhan(giaoPhan2, giaoXu2, tenLinhMuc)) duLieu[kv.Key] = kv.Value;
        // Biến người dùng tự chèn — xem ghi chú ở khối helper "Biến NGƯỜI DÙNG TỰ CHÈN".
        foreach (var kv in GiaDinhThem(giaDinh, giaDinh.ThanhVien.Count, await LayBangCachHienThi(ct))) duLieu[kv.Key] = kv.Value;
        duLieu["TenGiaoHo"] = giaDinh.GiaoHo?.TenGiaoHo ?? "Ngoài xứ";
        duLieu["MaGiaDinh"] = MaGiaDinh(giaDinh);
        duLieu["TenGiaDinh"] = giaDinh.TenGiaDinh;
        duLieu["GhiChuGiaDinh"] = giaDinh.GhiChu;

        var khoiHtml = new Dictionary<string, string?> { ["HangThanhVien"] = hangHtml.ToString() };

        var slug = BoDoMauIn.ChuanHoaTenGiaoPhan(giaoXu.GiaoHat?.GiaoPhan?.TenGiaoPhan);
        var html = await DungMau(slug, "GioiThieuChuyenXu", duLieu, khoiHtml, ct);
        var pdf = await trinhDuyet.XuatPdfAsync(html, ct);
        return new KetQuaInAn(pdf, $"GioiThieuChuyenXu_{giaDinh.MaGiaDinhCu}.pdf");
    }

    // --- Rao hôn phối (Source/ExcelReport/ReportRaoHP.cs) --------------------------------
    //
    // Nút "In giới thiệu hôn phối" (GxGiaoDanList.tsx, toolbar Hồ sơ lưu trữ giáo dân) — trước
    // lượt này báo "chưa hỗ trợ" (xem in-an.md mục 5e cuối và mục 8). Đối chiếu mã nguồn desktop
    // (Source/GXControl/GxGiaoDanList.cs dòng 56/380-393: nút bấm thật trên UI, item3, gọi
    // `frmReportGioiThieuHP` — MỘT cài đặt "Giấy giới thiệu" cũ khác hẳn) VS phần bị comment
    // (item7, không bao giờ tạo trên UI) gọi `frmRaoHonPhoi`/`ReportRaoHP.Export` — xem
    // can-review-sau.md mục mới: nhiệm vụ này CHỈ ĐẠO RÕ coi nút web hiện tại tương ứng
    // `ReportRaoHP.cs` (rao hôn phối, KHÔNG phải giấy giới thiệu giáo lý hôn phối đã làm ở mục
    // 5e) — làm theo chỉ đạo, đã ghi rõ phát hiện trên vào can-review-sau.md để lượt sau xem lại
    // nếu cần.
    //
    // "Xin điều tra và rao hôn phối" luôn cần MỘT cặp giáo dân (RaoHonPhoi.GiaoDan1/GiaoDan2,
    // xem docs/superpowers/specs/man-hinh/rao-hon-phoi.md) — nút bấm từ MỘT giáo dân nên lấy
    // đôi rao MỚI NHẤT (theo CreatedAt) mà giáo dân đó là GiaoDan1 HOẶC GiaoDan2. 404 khi chưa
    // có đôi rao nào — không có gì để in (giống "Chứng nhận hôn phối" 404 khi chưa có hôn phối).

    private static string Tuoi(DateOnly? ngaySinh)
    {
        // Chép nguyên công thức `Memory.GetTuoi` (Source/DBAccess/CMemory.cs:2004-2011): tuổi =
        // năm hiện tại - năm sinh, để TRỐNG (không phải "0") khi sinh cùng năm hiện tại hoặc
        // không có ngày sinh — khác `VanBanInAn.Ngay` (chuỗi rỗng), giữ đúng khoảng trắng " " mà
        // bản gốc trả về để không lộ "0 tuổi" vô nghĩa trên giấy.
        if (ngaySinh is null) return " ";
        var namNay = DateTime.Now.Year;
        return namNay == ngaySinh.Value.Year ? " " : (namNay - ngaySinh.Value.Year).ToString();
    }

    private static string AnhChi(string? phai) => phai == "Nam" ? "Anh" : "Chị";

    /// <summary>Biến thêm dùng chung cho HAI mẫu rao hôn phối ("Xin điều tra và rao hôn phối" và
    /// "Kết quả rao hôn phối") — mỗi mẫu gốc chỉ in một phần dữ liệu của đôi rao, phần còn lại
    /// vẫn có sẵn trong tay nên công bố cho người dùng tự chèn. Giá trị nào mẫu gốc ĐÃ dùng thì
    /// ở đây gán ĐÚNG cùng một biểu thức (không đổi kết quả in hiện tại). Không truy vấn thêm —
    /// `r` đã Include sẵn GiaoDan1/GiaoDan2 ở nơi gọi.</summary>
    private static Dictionary<string, string?> ThongTinDoiRaoThem(RaoHonPhoi r)
    {
        static string MoTaRuaToi(GiaoDan? g) => g is null ? "" : VanBanInAn.MoTaBiTich(
            g.SoRuaToi, g.NgayRuaToi, g.NoiRuaToi, g.ChaRuaToi, "rửa", g.NguoiDoDauRuaToi, "người đỡ đầu");
        static string MoTaThemSuc(GiaoDan? g) => g is null ? "" : VanBanInAn.MoTaBiTich(
            g.SoThemSuc, g.NgayThemSuc, g.NoiThemSuc, g.ChaThemSuc, "ban", g.NguoiDoDauThemSuc, "người đỡ đầu");

        return new Dictionary<string, string?>
        {
            ["Phai1"] = r.GiaoDan1?.Phai, ["Phai2"] = r.GiaoDan2?.Phai,
            ["Tuoi1"] = Tuoi(r.GiaoDan1?.NgaySinh), ["Tuoi2"] = Tuoi(r.GiaoDan2?.NgaySinh),
            ["NgaySinh1"] = Ngay(r.GiaoDan1?.NgaySinh), ["NgaySinh2"] = Ngay(r.GiaoDan2?.NgaySinh),
            ["NoiSinh1"] = r.GiaoDan1?.NoiSinh, ["NoiSinh2"] = r.GiaoDan2?.NoiSinh,
            ["DienThoai1"] = r.GiaoDan1?.DienThoai, ["DienThoai2"] = r.GiaoDan2?.DienThoai,
            ["DiaChi1"] = r.GiaoDan1?.DiaChi, ["DiaChi2"] = r.GiaoDan2?.DiaChi,
            ["MoTaRuaToi1"] = MoTaRuaToi(r.GiaoDan1), ["MoTaRuaToi2"] = MoTaRuaToi(r.GiaoDan2),
            ["MoTaThemSuc1"] = MoTaThemSuc(r.GiaoDan1), ["MoTaThemSuc2"] = MoTaThemSuc(r.GiaoDan2),

            ["TenGiaoXu1"] = r.GiaoXu1, ["TenGiaoXu2"] = r.GiaoXu2,
            ["TenGiaoPhan1"] = r.GiaoPhan1, ["TenGiaoPhan2"] = r.GiaoPhan2,
            ["TenGiaoXuNQ1"] = r.GiaoXuNQ1, ["TenGiaoPhanNQ1"] = r.GiaoPhanNQ1,
            ["TenGiaoXuNQ2"] = r.GiaoXuNQ2, ["TenGiaoPhanNQ2"] = r.GiaoPhanNQ2,
            ["TenGiaoXuTruoc1"] = r.GiaoXuTruoc1, ["TenGiaoPhanTruoc1"] = r.GiaoPhanTruoc1,
            ["TenGiaoXuTruoc2"] = r.GiaoXuTruoc2, ["TenGiaoPhanTruoc2"] = r.GiaoPhanTruoc2,

            ["MaRaoHonPhoi"] = r.MaRaoHonPhoiCu.ToString(),
            ["TenRaoHonPhoi"] = r.TenRaoHonPhoi,
            ["NgayRaoLan1"] = Ngay(r.NgayRaoLan1),
            ["NgayRaoLan2"] = Ngay(r.NgayRaoLan2),
            ["NgayRaoLan3"] = Ngay(r.NgayRaoLan3),
            ["GhiChuRao"] = r.GhiChu,
        };
    }

    public async Task<KetQuaInAn?> XuatGioiThieuHonPhoi(Guid giaoDanId, CancellationToken ct)
    {
        var giaoXu = await LayGiaoXuHienTai(ct);
        if (giaoXu is null) return null;

        var r = await db.RaoHonPhoi
            .Include(x => x.GiaoDan1).Include(x => x.GiaoDan2)
            .Where(x => x.GiaoDan1Id == giaoDanId || x.GiaoDan2Id == giaoDanId)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(ct);
        if (r is null) return null;

        static string HoTen(GiaoDan? g) => g is null ? ""
            : string.IsNullOrWhiteSpace(g.TenThanh) ? g.HoTen : $"{g.TenThanh} {g.HoTen}";

        var duLieu = new Dictionary<string, string?>(ThongTinGiaoXu(giaoXu))
        {
            // Chép NGUYÊN VĂN cách bản gốc gán hai chỗ trống "Cha Xứ"/"Giáo Phận" của phần
            // "Kính gửi" — Source/ExcelReport/ReportRaoHP.cs dòng 93-94:
            //   word.Replace(TenGiaoXuNhan, rowData[TenLinhMucNhan]);
            //   word.Replace(TenGiaoPhanNhan, rowData[GiaoXuNhan]);
            // Bảng RaoHonPhoi chỉ có hai cột "LinhMucNhan"/"GiaoXuNhan" (không có cột
            // "GiaoPhanNhan" riêng) — bản gốc GHÉP LinhMucNhan vào chỗ trống "Cha Xứ" (hợp lý)
            // nhưng GhÉP GiaoXuNhan vào chỗ trống "Giáo Phận" (tên cột không khớp nhãn in ra) —
            // MỘT SAI KHÁC CÓ CHỦ Ý được migrate y hệt (xem can-review-sau.md), không "sửa cho
            // đúng" âm thầm.
            ["TenGiaoXuNhan"] = r.LinhMucNhan,
            ["TenGiaoPhanNhan"] = r.GiaoXuNhan,

            ["AnhChi1"] = AnhChi(r.GiaoDan1?.Phai), ["AnhChi2"] = AnhChi(r.GiaoDan2?.Phai),
            ["HoTen1"] = HoTen(r.GiaoDan1), ["HoTen2"] = HoTen(r.GiaoDan2),
            ["Tuoi1"] = Tuoi(r.GiaoDan1?.NgaySinh), ["Tuoi2"] = Tuoi(r.GiaoDan2?.NgaySinh),
            ["TenCha1"] = r.GiaoDan1?.HoTenCha, ["TenCha2"] = r.GiaoDan2?.HoTenCha,
            ["TenMe1"] = r.GiaoDan1?.HoTenMe, ["TenMe2"] = r.GiaoDan2?.HoTenMe,

            ["TenGiaoXuNQ1"] = r.GiaoXuNQ1, ["TenGiaoPhanNQ1"] = r.GiaoPhanNQ1,
            ["TenGiaoXuNQ2"] = r.GiaoXuNQ2, ["TenGiaoPhanNQ2"] = r.GiaoPhanNQ2,
            ["TenGiaoXu1"] = r.GiaoXu1, ["TenGiaoXu2"] = r.GiaoXu2,
            ["TenGiaoXuTruoc1"] = r.GiaoXuTruoc1, ["TenGiaoPhanTruoc1"] = r.GiaoPhanTruoc1,
            ["TenGiaoXuTruoc2"] = r.GiaoXuTruoc2, ["TenGiaoPhanTruoc2"] = r.GiaoPhanTruoc2,

            ["NgayThangNamIn"] = DateTime.Now.ToString("dd/MM/yyyy"),
        };
        // Biến người dùng tự chèn — xem ghi chú ở khối helper "Biến NGƯỜI DÙNG TỰ CHÈN".
        foreach (var kv in ThongTinDoiRaoThem(r)) duLieu[kv.Key] = kv.Value;

        var slug = BoDoMauIn.ChuanHoaTenGiaoPhan(giaoXu.GiaoHat?.GiaoPhan?.TenGiaoPhan);
        var html = await DungMau(slug, "RaoHonPhoi", duLieu, null, ct);
        var pdf = await trinhDuyet.XuatPdfAsync(html, ct);
        return new KetQuaInAn(pdf, $"RaoHonPhoi_{r.MaRaoHonPhoiCu}.pdf");
    }

    /// <summary>"In kết quả rao hôn phối" (`RaoHonPhoiDetail.tsx`) — tương đương
    /// `ReportRaoHP.Export(ds, printRS: true)`/`KQRaoHonPhoi.doc`. Khác giấy "xin điều tra" ở
    /// trên: đây là bản CHỨNG NHẬN đã rao xong, cần đủ thông tin bí tích/địa chỉ của cả hai
    /// người (giống "Chứng nhận hôn phối", xem <see cref="XuatChungNhanHonPhoi"/>), theo ĐÚNG
    /// đôi rao được chọn (không suy đoán "mới nhất" như trên vì đây luôn bấm từ trang chi tiết
    /// MỘT đôi rao cụ thể).</summary>
    public async Task<KetQuaInAn?> XuatKetQuaRaoHonPhoi(Guid raoHonPhoiId, CancellationToken ct)
    {
        var giaoXu = await LayGiaoXuHienTai(ct);
        if (giaoXu is null) return null;

        var r = await db.RaoHonPhoi
            .Include(x => x.GiaoDan1).Include(x => x.GiaoDan2)
            .FirstOrDefaultAsync(x => x.Id == raoHonPhoiId, ct);
        if (r is null) return null;

        static string HoTen(GiaoDan? g) => g is null ? ""
            : string.IsNullOrWhiteSpace(g.TenThanh) ? g.HoTen : $"{g.TenThanh} {g.HoTen}";
        string MoTaRuaToi(GiaoDan? g) => g is null ? "" : VanBanInAn.MoTaBiTich(
            g.SoRuaToi, g.NgayRuaToi, g.NoiRuaToi, g.ChaRuaToi, "rửa", g.NguoiDoDauRuaToi, "người đỡ đầu");
        string MoTaThemSuc(GiaoDan? g) => g is null ? "" : VanBanInAn.MoTaBiTich(
            g.SoThemSuc, g.NgayThemSuc, g.NoiThemSuc, g.ChaThemSuc, "ban", g.NguoiDoDauThemSuc, "người đỡ đầu");

        // Khối văn bản 3 dòng "1. .../2. .../3. ..." — chép nguyên công thức
        // `string.Format("Với thời gian rao hôn phối:\rNgày rao lần 1: {0}, ...")` mà
        // `GxGiaDinhList.cs` dùng cho "Chứng nhận hôn phối" (mục 5b), áp dụng lại ở đây cho
        // đúng placeholder {{RaoHonPhoi}} của KQRaoHonPhoi.doc.
        var raoHonPhoi = VanBanInAn.GhepDong(
            !string.IsNullOrEmpty(Ngay(r.NgayRaoLan1)) ? $"1. {Ngay(r.NgayRaoLan1)}" : null,
            !string.IsNullOrEmpty(Ngay(r.NgayRaoLan2)) ? $"2. {Ngay(r.NgayRaoLan2)}" : null,
            !string.IsNullOrEmpty(Ngay(r.NgayRaoLan3)) ? $"3. {Ngay(r.NgayRaoLan3)}" : null);

        var duLieu = new Dictionary<string, string?>(ThongTinGiaoXu(giaoXu))
        {
            ["TenGiaoXuNhan"] = r.LinhMucNhan, ["TenGiaoPhanNhan"] = r.GiaoXuNhan,
            ["TenLinhMucGui"] = null,

            ["Phai1"] = r.GiaoDan1?.Phai, ["Phai2"] = r.GiaoDan2?.Phai,
            ["AnhChi1"] = AnhChi(r.GiaoDan1?.Phai), ["AnhChi2"] = AnhChi(r.GiaoDan2?.Phai),
            ["HoTen1"] = HoTen(r.GiaoDan1), ["HoTen2"] = HoTen(r.GiaoDan2),
            ["DienThoai1"] = r.GiaoDan1?.DienThoai, ["DienThoai2"] = r.GiaoDan2?.DienThoai,
            ["NgaySinh1"] = Ngay(r.GiaoDan1?.NgaySinh), ["NgaySinh2"] = Ngay(r.GiaoDan2?.NgaySinh),
            ["NoiSinh1"] = r.GiaoDan1?.NoiSinh, ["NoiSinh2"] = r.GiaoDan2?.NoiSinh,
            ["MoTaRuaToi1"] = MoTaRuaToi(r.GiaoDan1), ["MoTaRuaToi2"] = MoTaRuaToi(r.GiaoDan2),
            ["MoTaThemSuc1"] = MoTaThemSuc(r.GiaoDan1), ["MoTaThemSuc2"] = MoTaThemSuc(r.GiaoDan2),
            ["TenCha1"] = r.GiaoDan1?.HoTenCha, ["TenCha2"] = r.GiaoDan2?.HoTenCha,
            ["TenMe1"] = r.GiaoDan1?.HoTenMe, ["TenMe2"] = r.GiaoDan2?.HoTenMe,
            ["TenGiaoXu1"] = r.GiaoXu1, ["TenGiaoXu2"] = r.GiaoXu2,
            ["TenGiaoPhan1"] = r.GiaoPhan1, ["TenGiaoPhan2"] = r.GiaoPhan2,
            ["DiaChi1"] = r.GiaoDan1?.DiaChi, ["DiaChi2"] = r.GiaoDan2?.DiaChi,

            ["RaoHonPhoi"] = raoHonPhoi,
            ["NgayThangNamIn"] = DateTime.Now.ToString("dd/MM/yyyy"),
        };
        // Biến người dùng tự chèn — xem ghi chú ở khối helper "Biến NGƯỜI DÙNG TỰ CHÈN".
        foreach (var kv in ThongTinDoiRaoThem(r)) duLieu[kv.Key] = kv.Value;

        var slug = BoDoMauIn.ChuanHoaTenGiaoPhan(giaoXu.GiaoHat?.GiaoPhan?.TenGiaoPhan);
        var html = await DungMau(slug, "KQRaoHonPhoi", duLieu, null, ct);
        var pdf = await trinhDuyet.XuatPdfAsync(html, ct);
        return new KetQuaInAn(pdf, $"KQRaoHonPhoi_{r.MaRaoHonPhoiCu}.pdf");
    }

    /// <summary>Như <see cref="ThongTinGiaoXu"/> nhưng chịu được <c>null</c> — ba mẫu "In danh
    /// sách" không bắt buộc tìm thấy giáo xứ (vẫn in được danh sách rỗng). Dùng để công bố các
    /// biến liên hệ giáo xứ ({{DiaChiGiaoXu}}, {{TenGiaoPhan}}…) cho phần đầu trang mà mẫu gốc
    /// chưa in — xem MauInCatalog.BienKhaDung của DanhSach*.</summary>
    private static Dictionary<string, string?> ThongTinGiaoXuCoTheRong(GiaoXu? giaoXu) =>
        giaoXu is null
            ? new Dictionary<string, string?>
            {
                ["TenGiaoPhan"] = "", ["TenGiaoHat"] = "", ["TenGiaoXu"] = "",
                ["DiaChiGiaoXu"] = "", ["DienThoaiGiaoXu"] = "", ["EmailGiaoXu"] = "",
                ["WebsiteGiaoXu"] = "",
            }
            : ThongTinGiaoXu(giaoXu);

    private static string ChuoiDs(string? v) => string.IsNullOrEmpty(v) ? "—" : v;
    private static string NgayDs(DateOnly? v) => v is null ? "—" : Ngay(v);
    private static string BoolDs(bool v) => v ? "✓" : "—";

    /// <summary>"In danh sách" (nút thanh công cụ, icon in, `GiaoDanList.tsx`) — bấm nào cũng
    /// XUẤT PDF cả danh sách hiện đang lọc, không phải một bản ghi. Bản desktop
    /// (`btnInDanhSach_Click`, `Source/ChuongTrinh/frmGiaoDanList.cs` dòng 329-341) xuất control
    /// GridEX ĐANG HIỂN THỊ (Janus `GridEXExporter`) ra một tệp .xls tạm rồi `Process.Start` mở
    /// nó — nghĩa là in đúng những gì SẮP XẾP/LỌC hiện có trên lưới tại thời điểm bấm. Bản web
    /// KHÔNG đọc lại trạng thái AG Grid phía máy khách (sắp xếp cột, ô lọc từng cột) — CỐ Ý
    /// dùng lại đúng ba tham số lọc (giaoHoId/chiKhongThongKe/hienCaDaMat) và gọi thẳng
    /// <see cref="GiaoDanService.LayDanhSach"/>, giống hệt cách "Xuất Excel" đã làm ở
    /// <see cref="XuatExcelService"/> (đã quyết định trước đó, xem ghi chú ở lớp đó) — để danh
    /// sách in ra LUÔN khớp với chính GET danh sách đã dựng nên lưới, không có nguy cơ máy
    /// khách/máy chủ lọc lệch nhau. Khác biệt so với thao tác gốc (không phải "sửa cho đúng"
    /// ngầm) ghi ở can-review-sau.md. 29 cột đúng thứ tự `cotGiaoDan.ts`/`XuatExcelService`,
    /// khổ NGANG (nhiều cột hơn khổ dọc chứa được).</summary>
    public async Task<KetQuaInAn> XuatDanhSachGiaoDan(
        Guid? giaoHoId, bool chiKhongThongKe, bool hienCaDaMat, CancellationToken ct)
    {
        var giaoXu = await LayGiaoXuHienTai(ct);
        var rows = await giaoDanDv.LayDanhSach(giaoHoId, chiKhongThongKe, hienCaDaMat, ct);

        var hangHtml = new System.Text.StringBuilder();
        foreach (var g in rows)
        {
            string E(string? s) => System.Text.Encodings.Web.HtmlEncoder.Default.Encode(s ?? "—");
            string EDs(string s) => System.Text.Encodings.Web.HtmlEncoder.Default.Encode(s);
            hangHtml.Append("<tr>")
                .Append("<td>").Append(g.MaGiaoDanCu).Append("</td>")
                .Append("<td>").Append(E(g.TenThanh)).Append("</td>")
                .Append("<td>").Append(E(g.HoTen)).Append("</td>")
                .Append("<td>").Append(E(g.Phai)).Append("</td>")
                .Append("<td>").Append(EDs(NgayDs(g.NgaySinh))).Append("</td>")
                .Append("<td>").Append(EDs(NgayDs(g.NgayRuaToi))).Append("</td>")
                .Append("<td>").Append(EDs(NgayDs(g.NgayRuocLe))).Append("</td>")
                .Append("<td>").Append(EDs(NgayDs(g.NgayThemSuc))).Append("</td>")
                .Append("<td>").Append(EDs(BoolDs(g.LapGd))).Append("</td>")
                .Append("<td>").Append(E(g.HoTenCha)).Append("</td>")
                .Append("<td>").Append(E(g.HoTenMe)).Append("</td>")
                .Append("<td>").Append(EDs(BoolDs(g.TanTong))).Append("</td>")
                .Append("<td>").Append(EDs(BoolDs(g.ConHoc))).Append("</td>")
                .Append("<td>").Append(E(g.NgheNghiep)).Append("</td>")
                .Append("<td>").Append(E(g.GhiChu)).Append("</td>")
                .Append("<td>").Append(E(g.DienThoai)).Append("</td>")
                .Append("<td>").Append(E(g.DiaChi)).Append("</td>")
                .Append("<td>").Append(E(g.TenGiaoHo)).Append("</td>")
                .Append("<td>").Append(EDs(BoolDs(g.DaChuyenDi))).Append("</td>")
                .Append("<td>").Append(E(g.TrinhDoVanHoa)).Append("</td>")
                .Append("<td>").Append(E(g.TrinhDoChuyenMon)).Append("</td>")
                .Append("<td>").Append(E(g.BietNgoaiNgu)).Append("</td>")
                .Append("<td>").Append(EDs(BoolDs(g.QuaDoi))).Append("</td>")
                .Append("<td>").Append(EDs(NgayDs(g.NgayQuaDoi))).Append("</td>")
                .Append("<td>").Append(E(g.NoiAnTang)).Append("</td>")
                .Append("<td>").Append(E(g.NoiSinh)).Append("</td>")
                .Append("<td>").Append(E(g.NoiRuaToi)).Append("</td>")
                .Append("<td>").Append(E(g.NoiRuocLe)).Append("</td>")
                .Append("<td>").Append(E(g.NoiThemSuc)).Append("</td>")
                .Append("</tr>");
        }

        // Khởi tạo từ ThongTinGiaoXuCoTheRong để công bố thêm các biến liên hệ giáo xứ cho người
        // dùng tự chèn vào đầu trang (mẫu gốc mới chỉ in {{TenGiaoXu}}) — xem khối helper "Biến
        // NGƯỜI DÙNG TỰ CHÈN". {{TenGiaoXu}} vẫn ra đúng giá trị cũ.
        var duLieu = new Dictionary<string, string?>(ThongTinGiaoXuCoTheRong(giaoXu))
        {
            ["SoLuong"] = rows.Count.ToString(),
            ["NgayThangNamIn"] = DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
            ["DieuKienLoc"] = hienCaDaMat ? "(hiện cả giáo dân đã mất/chuyển xứ)" : "",
        };
        var khoiHtml = new Dictionary<string, string?> { ["HangDanhSach"] = hangHtml.ToString() };
        var slug = BoDoMauIn.ChuanHoaTenGiaoPhan(giaoXu?.GiaoHat?.GiaoPhan?.TenGiaoPhan);
        var html = await DungMau(slug, "DanhSachGiaoDan", duLieu, khoiHtml, ct);
        var pdf = await trinhDuyet.XuatPdfAsync(html, ct, landscape: true);

        return new KetQuaInAn(pdf, $"DanhSachGiaoDan_{DateTime.Now:yyyy-MM-dd}.pdf");
    }

    /// <summary>"In danh sách" của màn hình "Danh sách gia đình" (`GiaDinhList.tsx`) — cùng lý
    /// do/thiết kế với <see cref="XuatDanhSachGiaoDan"/> ở trên (gọi thẳng
    /// <see cref="GiaDinhService.LayDanhSach"/>, KHÔNG viết lại điều kiện lọc). 12 cột đúng thứ
    /// tự `cotGiaDinh.ts`/`XuatExcelService`, gạch ngang riêng từng ô Người nam/Người nữ đúng
    /// quy tắc `Gach` gốc (không gạch cả dòng).</summary>
    public async Task<KetQuaInAn> XuatDanhSachGiaDinh(
        Guid? giaoHoId, bool chiKhongThongKe, CancellationToken ct)
    {
        var giaoXu = await LayGiaoXuHienTai(ct);
        var rows = await giaDinhDv.LayDanhSach(giaoHoId, chiKhongThongKe, ct);

        var hangHtml = new System.Text.StringBuilder();
        foreach (var g in rows)
        {
            string E(string? s) => System.Text.Encodings.Web.HtmlEncoder.Default.Encode(ChuoiDs(s));
            var kieuChong = g.Gach == 0 || g.Gach == 2 ? " class=\"struck\"" : "";
            var kieuVo = g.Gach == 1 || g.Gach == 2 ? " class=\"struck\"" : "";
            hangHtml.Append("<tr>")
                .Append("<td>").Append(g.MaGiaDinhCu).Append("</td>")
                .Append("<td>").Append(E(g.TenGiaDinh)).Append("</td>")
                .Append("<td").Append(kieuChong).Append(">").Append(E(g.TenChong)).Append("</td>")
                .Append("<td").Append(kieuVo).Append(">").Append(E(g.TenVo)).Append("</td>")
                .Append("<td>").Append(g.SoLuong).Append("</td>")
                .Append("<td>").Append(E(g.DienThoai)).Append("</td>")
                .Append("<td>").Append(E(g.DTChong)).Append("</td>")
                .Append("<td>").Append(E(g.DTVo)).Append("</td>")
                .Append("<td>").Append(E(g.DiaChi)).Append("</td>")
                .Append("<td>").Append(E(g.TenGiaoHo)).Append("</td>")
                .Append("<td>").Append(E(g.DienGiaDinh)).Append("</td>")
                .Append("<td>").Append(E(g.GhiChu)).Append("</td>")
                .Append("</tr>");
        }

        // Khởi tạo từ ThongTinGiaoXuCoTheRong để công bố thêm các biến liên hệ giáo xứ cho người
        // dùng tự chèn vào đầu trang (mẫu gốc mới chỉ in {{TenGiaoXu}}) — xem khối helper "Biến
        // NGƯỜI DÙNG TỰ CHÈN". {{TenGiaoXu}} vẫn ra đúng giá trị cũ.
        var duLieu = new Dictionary<string, string?>(ThongTinGiaoXuCoTheRong(giaoXu))
        {
            ["SoLuong"] = rows.Count.ToString(),
            ["NgayThangNamIn"] = DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
            ["DieuKienLoc"] = chiKhongThongKe ? "(chỉ gia đình không được thống kê)" : "",
        };
        var khoiHtml = new Dictionary<string, string?> { ["HangDanhSach"] = hangHtml.ToString() };
        var slug = BoDoMauIn.ChuanHoaTenGiaoPhan(giaoXu?.GiaoHat?.GiaoPhan?.TenGiaoPhan);
        var html = await DungMau(slug, "DanhSachGiaDinh", duLieu, khoiHtml, ct);
        var pdf = await trinhDuyet.XuatPdfAsync(html, ct, landscape: true);

        return new KetQuaInAn(pdf, $"DanhSachGiaDinh_{DateTime.Now:yyyy-MM-dd}.pdf");
    }

    /// <summary>"In danh sách" của màn hình "Danh sách rao hôn phối" (`RaoHonPhoiList.tsx`) —
    /// tương đương <c>ReportRaoHP.ExportList</c>/`DanhSachRaoHonPhoi.xls`
    /// (Source/ExcelReport/ReportRaoHP.cs:128), khác bản gốc ở một điểm CỐ Ý đơn giản hoá: bản
    /// desktop yêu cầu chọn MỘT ngày rao (hộp thoại `frmDateInput`) rồi chỉ giữ những đôi rao có
    /// một lần rao rơi trong tuần đó (`GxRaoHonPhoiList.isPrinted`, dòng 416-459); bản web KHÔNG
    /// có khái niệm "tuần đang chọn" ở màn hình danh sách — CÙNG lý do/thiết kế với
    /// <see cref="XuatDanhSachGiaoDan"/>: gọi lại đúng <see cref="RaoHonPhoiService.LayDanhSach"/>
    /// bằng tham số lọc `xemTatCa` đang áp dụng trên lưới (giống hệt "Xuất Excel" đã làm ở
    /// <see cref="XuatExcelService"/>), để danh sách in ra LUÔN khớp với chính GET danh sách đã
    /// dựng nên lưới. 8 cột đúng thứ tự <see cref="Dtos.RaoHonPhoiListItemDto"/>, khổ NGANG.</summary>
    public async Task<KetQuaInAn> XuatDanhSachRaoHonPhoi(bool xemTatCa, CancellationToken ct)
    {
        var giaoXu = await LayGiaoXuHienTai(ct);
        var rows = await raoHonPhoiDv.LayDanhSach(xemTatCa, ct);

        var hangHtml = new System.Text.StringBuilder();
        foreach (var r in rows)
        {
            string E(string? s) => System.Text.Encodings.Web.HtmlEncoder.Default.Encode(ChuoiDs(s));
            string EDs(string s) => System.Text.Encodings.Web.HtmlEncoder.Default.Encode(s);
            hangHtml.Append("<tr>")
                .Append("<td>").Append(r.MaRaoHonPhoiCu).Append("</td>")
                .Append("<td>").Append(E(r.TenRaoHonPhoi)).Append("</td>")
                .Append("<td>").Append(E(r.Nguoi1)).Append("</td>")
                .Append("<td>").Append(E(r.Nguoi2)).Append("</td>")
                .Append("<td>").Append(EDs(NgayDs(r.NgayRaoLan1))).Append("</td>")
                .Append("<td>").Append(EDs(NgayDs(r.NgayRaoLan2))).Append("</td>")
                .Append("<td>").Append(EDs(NgayDs(r.NgayRaoLan3))).Append("</td>")
                .Append("<td>").Append(E(r.GhiChu)).Append("</td>")
                .Append("</tr>");
        }

        // Khởi tạo từ ThongTinGiaoXuCoTheRong để công bố thêm các biến liên hệ giáo xứ cho người
        // dùng tự chèn vào đầu trang (mẫu gốc mới chỉ in {{TenGiaoXu}}) — xem khối helper "Biến
        // NGƯỜI DÙNG TỰ CHÈN". {{TenGiaoXu}} vẫn ra đúng giá trị cũ.
        var duLieu = new Dictionary<string, string?>(ThongTinGiaoXuCoTheRong(giaoXu))
        {
            ["SoLuong"] = rows.Count.ToString(),
            ["NgayThangNamIn"] = DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
            ["DieuKienLoc"] = xemTatCa ? "(xem tất cả)" : "(chỉ những đôi rao chưa hoàn tất)",
        };
        var khoiHtml = new Dictionary<string, string?> { ["HangDanhSach"] = hangHtml.ToString() };
        var slug = BoDoMauIn.ChuanHoaTenGiaoPhan(giaoXu?.GiaoHat?.GiaoPhan?.TenGiaoPhan);
        var html = await DungMau(slug, "DanhSachRaoHonPhoi", duLieu, khoiHtml, ct);
        var pdf = await trinhDuyet.XuatPdfAsync(html, ct, landscape: true);

        return new KetQuaInAn(pdf, $"DanhSachRaoHonPhoi_{DateTime.Now:yyyy-MM-dd}.pdf");
    }
}
