using Microsoft.EntityFrameworkCore;
using Qlgx.Api.Dtos;
using Qlgx.Data;
using Qlgx.Domain;

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
public class KiemTraDuLieuService(QlgxDbContext db, GiaDinhService giaDinhService)
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
                    g.GiaDinhThamGia.Any(tv => tv.GiaDinh!.DaChuyenXu),
                    g.GiaoHo == null ? "Ngoài xứ" : g.GiaoHo.TenGiaoHo)))
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

    // Hai ngưỡng tuổi kết hôn tối thiểu theo giới — GxConstants.cs:110-111. Nhãn ô tick "Sai
    // tuổi con cái cha mẹ" trên form desktop nói tới hằng số KHOANGCACH_TUOI_CHAME_CONCAI=16
    // (GxConstants.cs:108), nhưng ReviewGiaDinhProcess.coNgayThangLoi dòng 186-221 (mã THẬT SỰ
    // chạy) lại dùng lại đúng 2 hằng số này (20 nam / 18 nữ) — nhãn nói một đằng, mã chạy một
    // nẻo. Migrate y hệt mã chạy, xem spec mục 3.4 và can-review-sau.md.
    private const int TuoiHonPhoiNam = 20;
    private const int TuoiHonPhoiNu = 18;

    /// <summary>
    /// "Kiểm tra dữ liệu — gia đình" (frmKiemTraGiaDinhList.cs + ReviewGiaDinhProcess.cs, xem
    /// spec mục 3). Khác desktop (tải cả bảng GiaDinh JOIN HonPhoi vào một DataTable rồi lặp
    /// từng dòng): ở đây gom bốn quy tắc bằng vài truy vấn theo lô (số gia đình luôn nhỏ — hộ
    /// gia đình, không phải giáo dân — nên gộp trong bộ nhớ ứng dụng sau khi đã lọc DaXoa/giáo
    /// họ ở CSDL là an toàn về hiệu năng).
    ///
    /// TÁI HIỆN NGUYÊN VĂN bug ghi đè NguyenNhan của ReviewGiaDinhProcess.nhieuVoChong (dòng
    /// 259): nếu quy tắc "nhiều vợ/chồng" khớp, toàn bộ NguyenNhan bị THAY THẾ bằng đúng câu
    /// của riêng quy tắc đó — lý do của 3 quy tắc kia (nếu có) bị xoá khỏi chuỗi hiển thị dù
    /// KetQua (cờ bit) vẫn cộng đủ. Ở CSDL PostgreSQL mới, quy tắc "nhiều vợ/chồng" luôn ra 0
    /// (ràng buộc UNIQUE ux_thanh_vien_gia_dinh_mot_chong_mot_vo chặn cứng) nên bug này hiện
    /// không quan sát được trên dữ liệu thật — vẫn migrate đúng mã để không lặng lẽ "sửa cho
    /// đúng" một hành vi desktop.
    /// </summary>
    public async Task<List<KiemTraGiaDinhKetQuaDto>> KiemTraGiaDinh(
        Guid? giaoHoId, KiemTraGiaDinhTuyChon tuyChon, CancellationToken ct)
    {
        // Nền lọc ĐÚNG reViewData dòng 92-96: "AND DaXoa=0" + (MaGiaoHo=x nếu chọn cụ thể).
        var goc = db.GiaDinh.Where(g => !g.DaXoa);
        if (giaoHoId is { } id) goc = goc.Where(g => g.GiaoHoId == id);
        var idsGiaDinh = await goc.Select(g => g.Id).ToListAsync(ct);
        if (idsGiaDinh.Count == 0) return [];
        var idSet = idsGiaDinh.ToHashSet();

        // Chồng/vợ (VaiTro<=1) của các gia đình đang xét, kèm giới tính + ngày sinh.
        var voChong = await db.ThanhVienGiaDinh
            .Where(tv => idSet.Contains(tv.GiaDinhId) &&
                (tv.VaiTro == VaiTroGiaDinh.Chong || tv.VaiTro == VaiTroGiaDinh.Vo))
            .Select(tv => new { tv.GiaDinhId, tv.GiaoDanId, tv.VaiTro, tv.GiaoDan!.Phai, tv.GiaoDan.NgaySinh })
            .ToListAsync(ct);
        var voChongTheoGiaDinh = voChong.GroupBy(v => v.GiaDinhId)
            .ToDictionary(nhom => nhom.Key, nhom => nhom.ToList());

        // Con cái (VaiTro=2), chỉ cần ngày sinh, gộp theo gia đình.
        var conCaiTheoGiaDinh = (await db.ThanhVienGiaDinh
                .Where(tv => idSet.Contains(tv.GiaDinhId) && tv.VaiTro == VaiTroGiaDinh.Con)
                .Select(tv => new { tv.GiaDinhId, tv.GiaoDan!.NgaySinh })
                .ToListAsync(ct))
            .GroupBy(c => c.GiaDinhId).ToDictionary(nhom => nhom.Key, nhom => nhom.Select(c => c.NgaySinh).ToList());

        // Hôn phối gắn với gia đình qua BẤT KỲ người chồng/vợ nào (SELECT_GIADINH_LIST_CO_HONPHOI
        // join TVGD(VaiTro 0/1) -> GiaoDanHonPhoi -> HonPhoi) — gộp theo gia đình, giữ mọi ngày
        // hôn phối tìm được (một gia đình có thể có ≥2 nếu chồng/vợ mỗi người từng có hôn phối
        // ghi nhận riêng — hiếm, xem can-review-sau.md).
        var idNguoiVoChong = voChong.Select(v => v.GiaoDanId).ToHashSet();
        var ngayHonPhoiTheoGiaDinh = idNguoiVoChong.Count == 0
            ? new Dictionary<Guid, List<DateOnly?>>()
            : (await db.GiaoDanHonPhoi
                    .Where(gdh => idNguoiVoChong.Contains(gdh.GiaoDanId))
                    .Select(gdh => new { gdh.GiaoDanId, gdh.HonPhoi!.NgayHonPhoi })
                    .ToListAsync(ct))
                .Join(voChong, h => h.GiaoDanId, v => v.GiaoDanId, (h, v) => new { v.GiaDinhId, h.NgayHonPhoi })
                .GroupBy(x => x.GiaDinhId)
                .ToDictionary(nhom => nhom.Key, nhom => nhom.Select(x => x.NgayHonPhoi).ToList());

        var lyDoTheoGiaDinh = new Dictionary<Guid, (List<string> LyDo, int KetQua)>();

        foreach (var giaDinhId in idsGiaDinh)
        {
            var lyDo = new List<string>();
            var co = 0;
            voChongTheoGiaDinh.TryGetValue(giaDinhId, out var vc);
            vc ??= [];
            ngayHonPhoiTheoGiaDinh.TryGetValue(giaDinhId, out var ngayHps);
            ngayHps ??= [];

            // Quy tắc 1: Không có ngày hôn phối (coNgayThangLoi dòng 151-158) — không có hôn
            // phối nào gắn với gia đình NÀY có NgayHonPhoi khác rỗng.
            if (tuyChon.KhongCoNgayHonPhoi && ngayHps.TrueForAll(n => n is null))
            {
                lyDo.Add("- Không có ngày hôn phối");
                co += 1; // ReviewGiaDinhType.KhongCoNgayHonPhoi
            }

            // Quy tắc 2: Hôn phối trước tuổi (dòng 160-184) — người chồng/vợ đầu tiên (theo thứ
            // tự Chồng rồi Vợ) hôn phối trước ngưỡng theo giới, so với BẤT KỲ ngày hôn phối nào
            // gắn với gia đình. Dừng ở người đầu tiên vi phạm — đúng "break" của vòng lặp gốc.
            if (tuyChon.HonPhoiTruocTuoi)
            {
                foreach (var nguoi in vc.OrderBy(v => v.VaiTro))
                {
                    if (nguoi.NgaySinh is not { } ns) continue;
                    var nguong = LaNam(nguoi.Phai) ? TuoiHonPhoiNam : TuoiHonPhoiNu;
                    var viPham = ngayHps.Any(nhp => nhp is { } n && n.Year - ns.Year < nguong);
                    if (viPham)
                    {
                        lyDo.Add($"- Người {(LaNam(nguoi.Phai) ? "nam" : "nữ")} hôn phối trước {nguong} tuổi");
                        co += 2; // ReviewGiaDinhType.HonPhoiTruocTuoi
                        break;
                    }
                }
            }

            // Quy tắc 3: Khoảng cách tuổi cha/mẹ – con cái (dòng 186-221) — ngưỡng theo giới của
            // NGƯỜI CHA/MẸ (không phải của con), dùng lại đúng 2 hằng số ở quy tắc 2 (xem ghi
            // chú TuoiHonPhoiNam/Nu ở trên).
            if (tuyChon.KhoangCachTuoiConCai && conCaiTheoGiaDinh.TryGetValue(giaDinhId, out var conCai) && conCai.Count > 0)
            {
                foreach (var nguoi in vc.OrderBy(v => v.VaiTro))
                {
                    if (nguoi.NgaySinh is not { } ns) continue;
                    var nguong = LaNam(nguoi.Phai) ? TuoiHonPhoiNam : TuoiHonPhoiNu;
                    var viPham = conCai.Any(c => c is { } nsCon && nsCon.Year - ns.Year < nguong);
                    if (viPham)
                    {
                        var chaMe = LaNam(nguoi.Phai) ? "người cha" : "người mẹ";
                        lyDo.Add($"- Khoảng cách tuổi giữa {chaMe} và con cái không hợp lý (nhỏ hơn {nguong} tuổi)");
                        co += 4; // ReviewGiaDinhType.KhoangCachTuoiKhongHopLe
                        break;
                    }
                }
            }

            // Quy tắc 4: Nhiều vợ/chồng (dòng 234-264) — ≥2 người VaiTro=0 hoặc ≥2 người
            // VaiTro=1. TRÊN CSDL POSTGRESQL MỚI LUÔN RA 0 (ràng buộc UNIQUE chặn cứng), giữ lại
            // đúng mã vì đây là quy tắc mã nguồn thật có, không phải suy đoán.
            if (tuyChon.CacVanDeKhac)
            {
                var soChong = vc.Count(v => v.VaiTro == VaiTroGiaDinh.Chong);
                var soVo = vc.Count(v => v.VaiTro == VaiTroGiaDinh.Vo);
                if (soChong > 1 || soVo > 1)
                {
                    var strNhieu = "- ";
                    if (soChong > 1) strNhieu += "Gia đình có nhiều chồng. ";
                    if (soVo > 1) strNhieu += "Gia đình có nhiều vợ. ";
                    strNhieu += "(do lỗi phiên bản trước. Hãy mở gia đình này lên, xem lại thông " +
                                "tin và bấm nút cập nhật để sửa lỗi)";
                    // BUG TÁI HIỆN Ở ĐÂY: ghi đè toàn bộ lyDo thay vì thêm vào — đúng
                    // nhieuVoChong dòng 259 (row[NGUYEN_NHAN] = str.ToString(), không nối thêm
                    // vào giá trị coNgayThangLoi đã ghi trước đó).
                    lyDo = [strNhieu];
                    co += 8; // ReviewGiaDinhType.NhieuVoChong
                }
            }

            if (co > 0) lyDoTheoGiaDinh[giaDinhId] = (lyDo, co);
        }

        if (lyDoTheoGiaDinh.Count == 0) return [];

        var dsDto = await giaDinhService.LayTheoDanhSachId(lyDoTheoGiaDinh.Keys, ct);
        return dsDto
            .Where(d => lyDoTheoGiaDinh.ContainsKey(d.Id))
            .Select(d =>
            {
                var (lyDo, ketQuaCo) = lyDoTheoGiaDinh[d.Id];
                return new KiemTraGiaDinhKetQuaDto(d, string.Join("\n", lyDo), ketQuaCo);
            })
            .ToList();
    }

    private static bool LaNam(string? phai) => string.Equals(phai, "Nam", StringComparison.OrdinalIgnoreCase);
}
