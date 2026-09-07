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
    QlgxDbContext db, IBoiCanhGiaoXu boiCanh, BoDoMauIn mau, BoTrinhDuyet trinhDuyet, GiaDinhService giaDinhDv)
{
    private static string Ngay(DateOnly? d) => VanBanInAn.Ngay(d);

    private static string Dau(bool coDau) => coDau ? "[x]" : "[  ]";

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
    };

    /// <summary>Xuất PDF "Lý lịch cá nhân" cho một giáo dân — tương đương
    /// Source/ExcelReport/ReportLyLichCaNhan.cs của bản desktop, dựng lại bằng mẫu HTML thay vì
    /// Word. Trả null nếu không tìm thấy giáo dân (đã bị lọc theo GiaoXuId hiện tại, đã xoá
    /// mềm, hoặc id không tồn tại) — endpoint trả 404 trong trường hợp đó.</summary>
    public async Task<KetQuaInAn?> XuatLyLichCaNhan(Guid giaoDanId, CancellationToken ct)
    {
        // FirstOrDefaultAsync với điều kiện tường minh — KHÔNG dùng Find()/FindAsync() (bị cấm
        // trong mã nghiệp vụ vì bỏ qua bộ lọc giáo xứ toàn cục, xem CLAUDE.md).
        var g = await db.GiaoDan
            .Include(x => x.GiaoHo)
            .Include(x => x.GiaDinhThamGia)
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

        var tenGiaoHo = g.GiaoHo?.TenGiaoHo ?? "Ngoài xứ";
        var hoTenDayDu = string.IsNullOrWhiteSpace(g.TenThanh) ? g.HoTen : $"{g.TenThanh} {g.HoTen}";

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

            ["ConHoc"] = Dau(g.ConHoc),
            ["TanTong"] = Dau(g.TanTong),
            ["DaCoGiaDinh"] = Dau(g.DaCoGiaDinh),
            ["QuaDoi"] = Dau(g.QuaDoi),
            ["NgayQuaDoi"] = g.QuaDoi && g.NgayQuaDoi is not null ? $"— ngày {Ngay(g.NgayQuaDoi)}" : "",
            ["NoiAnTang"] = g.QuaDoi && !string.IsNullOrWhiteSpace(g.NoiAnTang) ? $"— an táng tại {g.NoiAnTang}" : "",
            ["SoAnTang"] = g.QuaDoi && !string.IsNullOrWhiteSpace(g.SoAnTang) ? $" (số {g.SoAnTang})" : "",

            ["NgayThangNamIn"] = DateTime.Now.ToString("dd/MM/yyyy"),
        };

        var slug = BoDoMauIn.ChuanHoaTenGiaoPhan(giaoXu.GiaoHat?.GiaoPhan?.TenGiaoPhan);
        var html = mau.Dung(slug, "LyLichCaNhan", duLieu);
        var pdf = await trinhDuyet.XuatPdfAsync(html, ct);

        return new KetQuaInAn(pdf, $"LyLichCaNhan_{g.MaGiaoDanCu}.pdf");
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

        var slug = BoDoMauIn.ChuanHoaTenGiaoPhan(giaoXu.GiaoHat?.GiaoPhan?.TenGiaoPhan);
        var html = mau.Dung(slug, "ChungNhanBiTich", duLieu);
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
    /// <c>HtmlEncoder.Default.Encode</c> trước, xem ghi chú ở BoDoMauIn.Dung.</summary>
    public async Task<KetQuaInAn?> XuatPhieuGiaDinh(Guid giaDinhId, CancellationToken ct)
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
            ["MaGiaDinh"] = giaDinh.MaGiaDinhRieng ?? giaDinh.MaGiaDinhCu.ToString(),
            ["TenGiaDinh"] = giaDinh.TenGiaDinh,
            ["TenGiaoHo"] = giaDinh.GiaoHo?.TenGiaoHo ?? "Ngoài xứ",
            ["DienThoaiGiaDinh"] = giaDinh.DienThoai,
            ["DiaChiGiaDinh"] = giaDinh.DiaChi,
            ["MoTaHonPhoi"] = moTaHonPhoi,
            ["GhiChuGiaDinh"] = giaDinh.GhiChu,
            ["NgayThangNamIn"] = DateTime.Now.ToString("dd/MM/yyyy"),
        };
        var khoiHtml = new Dictionary<string, string?> { ["HangThanhVien"] = hangHtml.ToString() };

        var slug = BoDoMauIn.ChuanHoaTenGiaoPhan(giaoXu.GiaoHat?.GiaoPhan?.TenGiaoPhan);
        var html = mau.Dung(slug, "PhieuGiaDinh", duLieu, khoiHtml);
        var pdf = await trinhDuyet.XuatPdfAsync(html, ct);

        return new KetQuaInAn(pdf, $"PhieuGiaDinh_{giaDinh.MaGiaDinhCu}.pdf");
    }

    private sealed record NguoiHonPhoi(
        Guid Id, string? Phai, string? TenThanh, string HoTen, DateOnly? NgaySinh, string? NoiSinh,
        string? HoTenCha, string? HoTenMe, DateOnly? NgayRuaToi, string? NoiRuaToi, string? SoRuaToi,
        string? ChaRuaToi, string? NguoiDoDauRuaToi, DateOnly? NgayThemSuc, string? NoiThemSuc,
        string? SoThemSuc, string? ChaThemSuc, string? NguoiDoDauThemSuc, string? TenGiaoHo);

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
                x.GiaoDan.GiaoHo != null ? x.GiaoDan.GiaoHo.TenGiaoHo : null))
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

        var slug = BoDoMauIn.ChuanHoaTenGiaoPhan(giaoXu.GiaoHat?.GiaoPhan?.TenGiaoPhan);
        var html = mau.Dung(slug, "ChungNhanHonPhoi", duLieu);
        var pdf = await trinhDuyet.XuatPdfAsync(html, ct);

        return new KetQuaInAn(pdf, $"ChungNhanHonPhoi_{honPhoi.MaHonPhoiCu}.pdf");
    }
}
