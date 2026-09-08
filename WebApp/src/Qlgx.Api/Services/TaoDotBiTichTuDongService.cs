using Microsoft.EntityFrameworkCore;
using Npgsql;
using Qlgx.Api.Dtos;
using Qlgx.Data;
using Qlgx.Domain;
using Qlgx.Domain.Entities;

namespace Qlgx.Api.Services;

/// <summary>
/// "Tạo danh sách bí tích tự động" (nhóm Công cụ dữ liệu, xem
/// docs/superpowers/specs/man-hinh/cong-cu-du-lieu.md mục 5.2) — thay
/// <c>frmTaoDotBiTich.cs</c> + <c>GenerateDotBiTichProcess.cs</c>. CÔNG CỤ SINH DỮ LIỆU HÀNG
/// LOẠT trên khối lớn nhất CSDL (1108 đợt bí tích, 6150 chi tiết) — áp dụng đúng 4 nguyên tắc
/// an toàn bắt buộc: xem trước → xác nhận với con số cụ thể → MỘT transaction → không mở rộng
/// phạm vi so với desktop.
///
/// CHỈ hỗ trợ Rửa tội/Rước lễ/Thêm sức (LoaiBiTich 0/1/2) — ĐÚNG giới hạn đã có sẵn của
/// <see cref="DotBiTichService"/> ("Danh sách sổ bí tích", xem so-bi-tich.md), không phải giới
/// hạn mới tự đặt ra: <c>GenerateDotBiTichProcess.reViewData</c> (`switch (loaiBiTich)`,
/// `GenerateDotBiTichProcess.cs:113-129`) cũng CHỈ xử lý đúng 3 giá trị này — chọn giá trị khác
/// khiến `colNameNgay` rỗng và câu SQL desktop hỏng. Bản web loại HẾT ba giá trị không hỗ trợ
/// (Hôn phối/An táng/Xức dầu) khỏi lựa chọn thay vì chỉ loại Hôn phối như combo desktop
/// (`frmTaoDotBiTich.cs:26`, `cbLoaiBiTich.Combo.Items.RemoveAt(3)`) — CỐ Ý an toàn hơn (tránh
/// để người dùng chọn được An táng/Xức dầu rồi gặp lỗi khó hiểu), xem can-review-sau.md mục 63.
///
/// THUẬT TOÁN THẬT (`GenerateDotBiTichProcess.reViewData`/`GetDotBiTich`,
/// `GenerateDotBiTichProcess.cs:99-231`):
/// 1. Lấy giáo dân có ngày bí tích tương ứng (Ngày Rửa tội/Rước lễ/Thêm sức) rơi vào khoảng
///    [TuNgay, DenNgay] — KHÔNG lọc `DaXoa`/`DaChuyenXu` (giống mọi công cụ hàng loạt khác).
/// 2. Lọc thêm theo Nơi/Linh mục NẾU form có điền (so khớp — bản web dùng so khớp CHÍNH XÁC
///    không phân biệt hoa/thường, xem "Chỗ chưa chắc" ở cong-cu-du-lieu.md — desktop dùng SQL
///    `LIKE "..."` không tự thêm ký tự đại diện, hành vi quan sát được coi như tương đương).
/// 3. Với mỗi giáo dân: tìm một `DotBiTich` đã có khớp CHÍNH XÁC (Linh mục so không phân biệt
///    hoa/thường + Loại bí tích + Ngày, `GetDotBiTich` dòng 196-208) — CHÚ Ý: gộp nhóm KHÔNG
///    xét Nơi bí tích, nên hai giáo dân cùng ngày/linh mục nhưng khác nơi (khi không lọc theo
///    Nơi) vẫn bị gộp vào CÙNG một đợt, đợt đó mang Nơi của giáo dân ĐẦU TIÊN tạo ra nó — tái
///    hiện đúng hành vi này (không tự thêm điều kiện Nơi vào gộp nhóm).
/// 4. Nếu chưa có, tạo MỚI một `DotBiTich` (MoTa tự sinh "Đợt bí tích ngày D tháng M năm Y").
/// 5. Thêm giáo dân vào `BiTichChiTiet` của đợt NẾU CHƯA CÓ (không bao giờ sửa/xoá chi tiết đã
///    có sẵn) — công cụ này CHỈ THÊM MỚI, không đụng bản ghi cũ, an toàn hơn hẳn Chuyển họ/Chuẩn
///    hoá dữ liệu (chỉ sửa) — vẫn migrate qua bốn nguyên tắc an toàn vì tạo sai hàng loạt cũng
///    khó dọn không kém.
///
/// KHÔNG migrate thứ tự xử lý theo "RIGHT(Ngay,4) ASC" (chỉ sắp theo NĂM, không đủ ngày/tháng,
/// `GenerateDotBiTichProcess.cs:145`) — bản web sắp theo ngày ĐẦY ĐỦ tăng dần, hợp lý hơn và chỉ
/// ảnh hưởng tới việc giáo dân nào "thắng" khi gán Nơi cho một đợt mới tạo trong trường hợp
/// hiếm gặp ở mục 3 — xem can-review-sau.md mục 63.
/// </summary>
public class TaoDotBiTichTuDongService(QlgxDbContext db, SinhMaService sinhMa, IBoiCanhGiaoXu boiCanh)
{
    private sealed record GiaoDanKhop(Guid Id, DateOnly Ngay, string? LinhMuc, string? Noi);

    private sealed record KetQuaTinhToan(
        int TongGiaoDanKhop,
        List<(DateOnly Ngay, string? LinhMuc, string? Noi, List<Guid> GiaoDanIds)> DotMoi,
        List<(Guid DotBiTichId, List<Guid> GiaoDanIds)> DotDaCo);

    public async Task<TaoDotBiTichXemTruocKetQua> XemTruoc(TaoDotBiTichTuDongRequest yc, CancellationToken ct)
    {
        var kq = await TinhToan(yc, ct);
        var soGiaoDanMoi = kq.DotMoi.Sum(d => d.GiaoDanIds.Count) + kq.DotDaCo.Sum(d => d.GiaoDanIds.Count);
        var mau = kq.DotMoi.Take(30)
            .Select(d => new DuKienDotMoiDto(d.Ngay, d.LinhMuc, d.Noi, d.GiaoDanIds.Count))
            .ToList();
        return new TaoDotBiTichXemTruocKetQua(kq.TongGiaoDanKhop, kq.DotMoi.Count, soGiaoDanMoi, mau);
    }

    /// <summary>
    /// Trả về (KetQua, Loi) thay vì chỉ KetQua: chống trùng ở TinhToan chỉ nằm ở tầng ứng dụng
    /// (SELECT rồi INSERT trong cùng transaction, mức cô lập mặc định READ COMMITTED) — hai
    /// request "Xác nhận tạo" chạy đồng thời (ví dụ người dùng bấm lại vì tưởng bị treo) đều có
    /// thể SELECT thấy "chưa có đợt khớp" trước khi bên kia commit, rồi cùng INSERT một
    /// <see cref="DotBiTich"/> trùng nhóm — xem review-toan-nhanh-dulieu.md mục C1. Ràng buộc
    /// UNIQUE cấp CSDL "ux_dot_bi_tich_nhom_trung" (migration ThemRangBuocDotBiTichTrung) là lưới
    /// an toàn CUỐI: request thua trong cuộc đua nhận PostgresException 23505 bọc trong
    /// DbUpdateException — bắt riêng để trả thông báo tiếng Việt rõ ràng thay vì lộ lỗi 500 thô,
    /// và để người dùng biết cần tải lại trang xem đợt bí tích thay vì bấm lại mù quáng.
    /// </summary>
    public async Task<(TaoDotBiTichTuDongKetQua? KetQua, string? Loi)> TaoTuDong(TaoDotBiTichTuDongRequest yc, CancellationToken ct)
    {
        var giaoXuId = boiCanh.GiaoXuId;
        await using var giaoTac = await db.Database.BeginTransactionAsync(ct);

        var kq = await TinhToan(yc, ct);
        var soDotDaTao = 0;
        var soGiaoDanDaThem = 0;
        var maHienTai = await db.DotBiTich.MaxAsync(x => (int?)x.MaDotBiTichCu, ct) ?? 0;

        foreach (var (ngay, linhMuc, noi, giaoDanIds) in kq.DotMoi)
        {
            maHienTai = await sinhMa.LayMaTiepTheo(giaoXuId, "dot_bi_tich", maHienTai, ct);
            var dot = new DotBiTich
            {
                GiaoXuId = giaoXuId,
                LoaiBiTich = yc.LoaiBiTich,
                NgayBiTich = ngay,
                LinhMuc = linhMuc,
                NoiBiTich = noi,
                MoTa = TaoMoTa(ngay),
                MaDotBiTichCu = maHienTai,
            };
            db.DotBiTich.Add(dot);
            soDotDaTao++;
            foreach (var giaoDanId in giaoDanIds)
            {
                db.BiTichChiTiet.Add(new BiTichChiTiet { GiaoXuId = giaoXuId, DotBiTichId = dot.Id, GiaoDanId = giaoDanId });
                soGiaoDanDaThem++;
            }
        }
        foreach (var (dotBiTichId, giaoDanIds) in kq.DotDaCo)
        {
            foreach (var giaoDanId in giaoDanIds)
            {
                db.BiTichChiTiet.Add(new BiTichChiTiet { GiaoXuId = giaoXuId, DotBiTichId = dotBiTichId, GiaoDanId = giaoDanId });
                soGiaoDanDaThem++;
            }
        }

        try
        {
            await db.SaveChangesAsync(ct);
            await giaoTac.CommitAsync(ct);
        }
        catch (DbUpdateException ex) when (LaViPhamRangBuocNhomTrung(ex))
        {
            await giaoTac.RollbackAsync(ct);
            return (null, "Không tạo được: một yêu cầu khác vừa tạo đợt bí tích trùng cùng linh mục/ngày " +
                "trong lúc thao tác này đang chạy (có thể do bấm \"Xác nhận tạo\" nhiều lần). " +
                "Hãy tải lại trang để xem đợt bí tích hiện có trước khi thử lại.");
        }
        return (new TaoDotBiTichTuDongKetQua(soDotDaTao, soGiaoDanDaThem), null);
    }

    private static bool LaViPhamRangBuocNhomTrung(DbUpdateException ex) =>
        ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation } pg
        && pg.ConstraintName == "ux_dot_bi_tich_nhom_trung";

    private static string TaoMoTa(DateOnly ngay) =>
        string.Format("Đợt bí tích ngày {0} tháng {1} năm {2}", ngay.Day, ngay.Month, ngay.Year);

    private async Task<KetQuaTinhToan> TinhToan(TaoDotBiTichTuDongRequest yc, CancellationToken ct)
    {
        var giaoDan = await LayGiaoDanKhop(yc, ct);

        var dotHienCo = await db.DotBiTich
            .Where(d => d.LoaiBiTich == yc.LoaiBiTich)
            .Select(d => new { d.Id, d.NgayBiTich, d.LinhMuc })
            .ToListAsync(ct);
        var chiTietHienCo = dotHienCo.Count == 0
            ? []
            : await db.BiTichChiTiet
                .Where(c => dotHienCo.Select(d => d.Id).Contains(c.DotBiTichId))
                .Select(c => new { c.DotBiTichId, c.GiaoDanId })
                .ToListAsync(ct);
        var capDaCo = chiTietHienCo.Select(c => (c.DotBiTichId, c.GiaoDanId)).ToHashSet();

        // Nhom theo (LinhMuc so khong phan biet hoa/thuong, Ngay chinh xac) - dung
        // GetDotBiTich (GenerateDotBiTichProcess.cs:196-208), KHONG xet Noi.
        var dotHienCoTheoKhoa = new Dictionary<(string LinhMuc, DateOnly Ngay), Guid>();
        foreach (var d in dotHienCo)
        {
            if (d.NgayBiTich is null) continue;
            var khoa = ((d.LinhMuc ?? "").Trim().ToLowerInvariant(), d.NgayBiTich.Value);
            dotHienCoTheoKhoa.TryAdd(khoa, d.Id);
        }

        var dotMoi = new Dictionary<(string LinhMuc, DateOnly Ngay), (DateOnly Ngay, string? LinhMuc, string? Noi, List<Guid> GiaoDanIds)>();
        var themVaoDotDaCo = new Dictionary<Guid, List<Guid>>();

        foreach (var g in giaoDan)
        {
            var khoa = ((g.LinhMuc ?? "").Trim().ToLowerInvariant(), g.Ngay);
            if (dotHienCoTheoKhoa.TryGetValue(khoa, out var dotBiTichId))
            {
                if (capDaCo.Contains((dotBiTichId, g.Id))) continue; // da co san, khong them lai
                if (!themVaoDotDaCo.TryGetValue(dotBiTichId, out var ds))
                    themVaoDotDaCo[dotBiTichId] = ds = [];
                ds.Add(g.Id);
                continue;
            }
            if (!dotMoi.TryGetValue(khoa, out var nhom))
                dotMoi[khoa] = nhom = (g.Ngay, g.LinhMuc, g.Noi, []);
            nhom.GiaoDanIds.Add(g.Id);
        }

        return new KetQuaTinhToan(
            giaoDan.Count,
            dotMoi.Values.OrderBy(d => d.Ngay).ToList(),
            themVaoDotDaCo.Select(kv => (kv.Key, kv.Value)).ToList());
    }

    private async Task<List<GiaoDanKhop>> LayGiaoDanKhop(TaoDotBiTichTuDongRequest yc, CancellationToken ct)
    {
        var (tu, den) = (yc.TuNgay, yc.DenNgay);
        var raw = yc.LoaiBiTich switch
        {
            LoaiBiTich.RuaToi => await db.GiaoDan
                .Where(g => g.NgayRuaToi != null && g.NgayRuaToi >= tu && g.NgayRuaToi <= den)
                .Select(g => new { g.Id, Ngay = g.NgayRuaToi!.Value, LinhMuc = g.ChaRuaToi, Noi = g.NoiRuaToi })
                .ToListAsync(ct),
            LoaiBiTich.RuocLe => await db.GiaoDan
                .Where(g => g.NgayRuocLe != null && g.NgayRuocLe >= tu && g.NgayRuocLe <= den)
                .Select(g => new { g.Id, Ngay = g.NgayRuocLe!.Value, LinhMuc = g.ChaRuocLe, Noi = g.NoiRuocLe })
                .ToListAsync(ct),
            LoaiBiTich.ThemSuc => await db.GiaoDan
                .Where(g => g.NgayThemSuc != null && g.NgayThemSuc >= tu && g.NgayThemSuc <= den)
                .Select(g => new { g.Id, Ngay = g.NgayThemSuc!.Value, LinhMuc = g.ChaThemSuc, Noi = g.NoiThemSuc })
                .ToListAsync(ct),
            _ => throw new ArgumentOutOfRangeException(nameof(yc), "Chi ho tro Rua toi/Ruoc le/Them suc"),
        };
        IEnumerable<(Guid Id, DateOnly Ngay, string? LinhMuc, string? Noi)> ds =
            raw.Select(x => (x.Id, x.Ngay, x.LinhMuc, x.Noi));
        if (!string.IsNullOrWhiteSpace(yc.NoiBiTich))
            ds = ds.Where(x => string.Equals((x.Noi ?? "").Trim(), yc.NoiBiTich.Trim(), StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrWhiteSpace(yc.LinhMuc))
            ds = ds.Where(x => string.Equals((x.LinhMuc ?? "").Trim(), yc.LinhMuc.Trim(), StringComparison.OrdinalIgnoreCase));
        return ds.OrderBy(x => x.Ngay).Select(x => new GiaoDanKhop(x.Id, x.Ngay, x.LinhMuc, x.Noi)).ToList();
    }
}
