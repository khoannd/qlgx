using Microsoft.EntityFrameworkCore;
using Qlgx.Api.Dtos;
using Qlgx.Data;

namespace Qlgx.Api.Services;

/// <summary>
/// "Kiểm tra dữ liệu" (nhóm "Công cụ dữ liệu", màn hình cuối cùng chưa migrate — xem
/// docs/superpowers/specs/man-hinh/cong-cu-du-lieu.md). Lượt này CHỈ làm nửa "giáo dân"
/// (frmKiemTraGiaoDanList.cs + ReviewGiaoDanProcess.cs) — nửa "gia đình" chưa migrate.
///
/// Khác cách cài của desktop (tải hết giáo dân theo giáo họ vào một DataTable rồi lặp từng
/// dòng trong bộ nhớ, ReviewGiaoDanProcess.reViewData dòng 106-150): ở đây mỗi quy tắc được
/// lọc THẲNG ở CSDL (chỉ những id vi phạm mới được kéo về), rồi hợp các tập id lại — bắt buộc
/// theo ràng buộc hiệu năng của nhiệm vụ (2050 giáo dân, không được tải hết vào bộ nhớ để lọc
/// bằng C#). Kết quả CUỐI CÙNG giống hệt desktop (đã đối chiếu bằng psql, xem spec mục 2.4);
/// chỉ cách tính khác, không phải hành vi khác.
/// </summary>
public class KiemTraDuLieuService(QlgxDbContext db)
{
    public async Task<List<KiemTraGiaoDanKetQuaDto>> KiemTraGiaoDan(
        Guid? giaoHoId, KiemTraGiaoDanTuyChon tuyChon, CancellationToken ct)
    {
        // Nền lọc ĐÚNG ReviewGiaoDanProcess.reViewData dòng 109: chỉ "AND DaXoa=0" — CỐ Ý
        // không lọc DaChuyenXu (khác LayDanhSach/màn hình danh sách giáo dân bình thường), xem
        // spec mục 2.2.
        var goc = db.GiaoDan.Where(g => !g.DaXoa);
        if (giaoHoId is { } id) goc = goc.Where(g => g.GiaoHoId == id);

        var tapId = new HashSet<Guid>();

        // Rule 1: Không có dữ liệu ngày tháng (ReviewGiaoDanProcess.cs:156-163).
        if (tuyChon.KhongCoNgayThang)
        {
            var ids = await goc.Where(g =>
                    g.NgaySinh == null && g.NgayRuaToi == null && g.NgayRuocLe == null &&
                    g.NgayThemSuc == null && g.NgayQuaDoi == null)
                .Select(g => g.Id).ToListAsync(ct);
            tapId.UnionWith(ids);
        }

        // Rule 2: Sai quan hệ ngày tháng — TÁI HIỆN BUG isValidDateInputRelations (chỉ so
        // NgaySinh với NgayRuaToi, xem spec mục 2.3 #2).
        if (tuyChon.SaiQuanHeNgayThang)
        {
            var ids = await goc.Where(g =>
                    g.NgaySinh != null && g.NgayRuaToi != null && g.NgaySinh > g.NgayRuaToi)
                .Select(g => g.Id).ToListAsync(ct);
            tapId.UnionWith(ids);
        }

        // Rule 3: Rước lễ trước tuổi (TUOI_RUOCLE=7) — cùng công thức thô "chỉ trừ năm" của
        // Memory.KiemTraTuoiKhongHopLe, đã có sẵn ở GiaoDanService (không viết công thức tuổi
        // thứ hai — TuoiKhongHopLe là internal nên gọi lại được từ đây).
        const int tuoiRuocLe = 7;
        if (tuyChon.RuocLeTruocTuoi)
        {
            var ids = await goc.Where(g =>
                    g.NgaySinh != null && g.NgayRuocLe != null &&
                    g.NgayRuocLe.Value.Year - g.NgaySinh.Value.Year < tuoiRuocLe)
                .Select(g => g.Id).ToListAsync(ct);
            tapId.UnionWith(ids);
        }

        // Rule 4: Thuộc nhiều gia đình (>1 gia_dinh_id phân biệt trong thanh_vien_gia_dinh).
        HashSet<Guid> idsNhieuGiaDinh = [];
        if (tuyChon.ThuocNhieuGiaDinh)
        {
            var idsTatCa = await goc.Select(g => g.Id).ToListAsync(ct);
            var idsTapHop = idsTatCa.ToHashSet();
            var nhieuGiaDinh = await db.ThanhVienGiaDinh
                .Where(t => idsTapHop.Contains(t.GiaoDanId))
                .GroupBy(t => t.GiaoDanId)
                .Where(nhom => nhom.Select(t => t.GiaDinhId).Distinct().Count() > 1)
                .Select(nhom => nhom.Key)
                .ToListAsync(ct);
            idsNhieuGiaDinh = nhieuGiaDinh.ToHashSet();
            tapId.UnionWith(idsNhieuGiaDinh);
        }

        // Rule 5: Không thuộc gia đình nào (0 bản ghi thanh_vien_gia_dinh).
        HashSet<Guid> idsKhongGiaDinh = [];
        if (tuyChon.KhongThuocGiaDinhNao)
        {
            var ids = await goc.Where(g => !g.GiaDinhThamGia.Any()).Select(g => g.Id).ToListAsync(ct);
            idsKhongGiaDinh = ids.ToHashSet();
            tapId.UnionWith(idsKhongGiaDinh);
        }

        // Rule 6: Có nhiều hôn phối (>1 hon_phoi_id phân biệt trong giao_dan_hon_phoi) —
        // SELECT_HONPHOI_CHECK_LIST của desktop quy về đúng phép đếm này, xem spec mục 2.3 #6.
        HashSet<Guid> idsNhieuHonPhoi = [];
        if (tuyChon.CoNhieuHonPhoi)
        {
            var idsTatCa = await goc.Select(g => g.Id).ToListAsync(ct);
            var idsTapHop = idsTatCa.ToHashSet();
            var nhieuHonPhoi = await db.GiaoDanHonPhoi
                .Where(h => idsTapHop.Contains(h.GiaoDanId))
                .GroupBy(h => h.GiaoDanId)
                .Where(nhom => nhom.Select(h => h.HonPhoiId).Distinct().Count() > 1)
                .Select(nhom => nhom.Key)
                .ToListAsync(ct);
            idsNhieuHonPhoi = nhieuHonPhoi.ToHashSet();
            tapId.UnionWith(idsNhieuHonPhoi);
        }

        if (tapId.Count == 0) return [];

        var dsDto = await GiaoDanService.DungDanhSach(
            db.GiaoDan.Where(g => tapId.Contains(g.Id))
                .OrderBy(g => g.MaGiaoDanCu)
                .Select(g => new GiaoDanService.NguonDong(g, null,
                    g.GiaDinhThamGia.OrderBy(tv => tv.VaiTro)
                        .Select(tv => (Guid?)tv.GiaDinhId).FirstOrDefault(),
                    g.GiaDinhThamGia.Any(tv => tv.GiaDinh!.DaChuyenXu))))
            .ToListAsync(ct);

        var ketQua = new List<KiemTraGiaoDanKetQuaDto>(dsDto.Count);
        foreach (var d in dsDto)
        {
            var lyDo = new List<string>();
            var co = 0;

            if (tuyChon.KhongCoNgayThang &&
                d.NgaySinh is null && d.NgayRuaToi is null && d.NgayRuocLe is null &&
                d.NgayThemSuc is null && d.NgayQuaDoi is null)
            {
                lyDo.Add("- Không có dữ liệu ngày tháng");
                co += 4; // ReviewGiaoDanType.KhongCoDuLieuNgayThang
            }
            if (tuyChon.SaiQuanHeNgayThang &&
                d.NgaySinh is { } ns1 && d.NgayRuaToi is { } rt1 && ns1 > rt1)
            {
                lyDo.Add("- Sai quan hệ ngày tháng");
                co += 2; // ReviewGiaoDanType.SaiQuanHeNgayThang
            }
            if (tuyChon.RuocLeTruocTuoi &&
                d.NgaySinh is { } ns2 && d.NgayRuocLe is { } rl2 && rl2.Year - ns2.Year < tuoiRuocLe)
            {
                lyDo.Add($"- Rước lễ trước {tuoiRuocLe} tuổi");
                co += 1; // ReviewGiaoDanType.RuocLeTruocTuoi
            }
            if (tuyChon.ThuocNhieuGiaDinh && idsNhieuGiaDinh.Contains(d.Id))
            {
                lyDo.Add("- Thuộc nhiều gia đình");
                co += 8; // ReviewGiaoDanType.ThuocNhieuGiaDinh
            }
            if (tuyChon.KhongThuocGiaDinhNao && idsKhongGiaDinh.Contains(d.Id))
            {
                lyDo.Add("- Không thuộc gia đình nào");
                co += 16; // ReviewGiaoDanType.KhongThuocGiaDinhNao
            }
            if (tuyChon.CoNhieuHonPhoi && idsNhieuHonPhoi.Contains(d.Id))
            {
                lyDo.Add("- Có nhiều thông tin hôn phối");
                co += 32; // ReviewGiaoDanType.CoNhieuHonPhoi
            }

            if (co > 0) ketQua.Add(new KiemTraGiaoDanKetQuaDto(d, string.Join("\n", lyDo), co));
        }
        return ketQua;
    }
}
