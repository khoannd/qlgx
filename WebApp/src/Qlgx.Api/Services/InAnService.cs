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
public class InAnService(QlgxDbContext db, IBoiCanhGiaoXu boiCanh, BoDoMauIn mau, BoTrinhDuyet trinhDuyet)
{
    private const string ChuoiTrong = "";

    private static string Ngay(DateOnly? d) => d?.ToString("dd/MM/yyyy") ?? ChuoiTrong;

    private static string Dau(bool coDau) => coDau ? "[x]" : "[  ]";

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

        var giaoXu = await db.GiaoXu
            .Include(x => x.GiaoHat).ThenInclude(h => h!.GiaoPhan)
            .FirstOrDefaultAsync(x => x.Id == boiCanh.GiaoXuId, ct);
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

        var duLieu = new Dictionary<string, string?>
        {
            ["TenGiaoPhan"] = giaoXu.GiaoHat?.GiaoPhan?.TenGiaoPhan,
            ["TenGiaoHat"] = giaoXu.GiaoHat?.TenGiaoHat,
            ["TenGiaoXu"] = giaoXu.TenGiaoXu,
            ["DiaChiGiaoXu"] = string.IsNullOrWhiteSpace(giaoXu.DiaChi) ? "" : $" — {giaoXu.DiaChi}",
            ["DienThoaiGiaoXu"] = string.IsNullOrWhiteSpace(giaoXu.DienThoai) ? "" : $" — ĐT: {giaoXu.DienThoai}",
            ["EmailGiaoXu"] = string.IsNullOrWhiteSpace(giaoXu.Email) ? "" : $" — Email: {giaoXu.Email}",

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

            ["SoRuaToi"] = g.SoRuaToi,
            ["NgayRuaToi"] = Ngay(g.NgayRuaToi),
            ["NoiRuaToi"] = g.NoiRuaToi,
            ["ChaRuaToi"] = g.ChaRuaToi,
            ["NguoiDoDauRuaToi"] = g.NguoiDoDauRuaToi,

            ["SoRuocLe"] = g.SoRuocLe,
            ["NgayRuocLe"] = Ngay(g.NgayRuocLe),
            ["NoiRuocLe"] = g.NoiRuocLe,
            ["ChaRuocLe"] = g.ChaRuocLe,

            ["SoThemSuc"] = g.SoThemSuc,
            ["NgayThemSuc"] = Ngay(g.NgayThemSuc),
            ["NoiThemSuc"] = g.NoiThemSuc,
            ["ChaThemSuc"] = g.ChaThemSuc,
            ["NguoiDoDauThemSuc"] = g.NguoiDoDauThemSuc,

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
}
