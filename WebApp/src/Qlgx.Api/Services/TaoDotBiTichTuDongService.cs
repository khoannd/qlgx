using Microsoft.EntityFrameworkCore;
using Npgsql;
using Qlgx.Api.Dtos;
using Qlgx.Data;
using Qlgx.Data.NhatKy;
using Qlgx.Domain;
using Qlgx.Domain.Entities;

namespace Qlgx.Api.Services;

/// <summary>
/// "Tạo danh sách bí tích tự động" (nhóm Công cụ dữ liệu, xem
/// docs/superpowers/specs/man-hinh/cong-cu-du-lieu.md mục 5.2) — thay
/// <c>frmTaoDotBiTich.cs</c> + <c>GenerateDotBiTichProcess.cs</c>. CÔNG CỤ SINH DỮ LIỆU HÀNG
/// LOẠT trên khối lớn nhất CSDL (1108 đợt bí tích, 6150 chi tiết) — áp dụng đúng 4 nguyên tắc
/// an toàn bắt buộc: xem trước → xác nhận với con số cụ thể → ghi có giao dịch → không mở rộng
/// phạm vi so với desktop. Nguyên tắc thứ ba trước đây là "MỘT transaction bao trọn"; nay ghi
/// theo TỪNG LÔ, mỗi lô một giao dịch — xem <see cref="TaoTuDong"/> để biết vì sao đổi.
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

    /// <summary>Số DÒNG (đợt + chi tiết) tối đa mỗi lô khi ghi thật — xem <see cref="TaoTuDong"/>.
    /// Cùng cỡ với ChuanHoaDuLieuService.CoLo / TimThayTheService.CoLo / ChuyenHoService.CoLo.</summary>
    internal const int CoLo = 200;

    /// <summary>
    /// Một đơn vị ghi KHÔNG ĐƯỢC CẮT NHỎ HƠN: hoặc một đợt MỚI kèm toàn bộ chi tiết của chính
    /// nó (<c>DotBiTichId</c> null), hoặc các chi tiết thêm vào một đợt ĐÃ CÓ. Đợt mới và chi
    /// tiết của nó phải cùng vào hoặc cùng không — chi tiết mồ côi trỏ tới một đợt không tồn
    /// tại là dữ liệu hỏng, khác hẳn "làm một nửa" vốn chấp nhận được giữa hai lô.
    /// </summary>
    private sealed record CongViecGhi(Guid? DotBiTichId, DateOnly Ngay, string? LinhMuc, string? Noi, List<Guid> GiaoDanIds);

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
    ///
    /// GHI THEO TỪNG LÔ, mỗi lô một giao dịch riêng — KHÔNG còn MỘT transaction bao trọn như
    /// nguyên tắc 3 ở tài liệu lớp mô tả (dòng đó nay phải đọc là "mỗi lô một transaction").
    /// VÌ SAO đổi: đây là khối lớn nhất CSDL (1108 đợt, 6150 chi tiết) và mỗi dòng ghi nay sinh
    /// nhật ký mức ô, nên ghi trọn một lượt nghĩa là giữ khoá dòng đếm <c>bo_dem_hieu_luc</c>
    /// của giáo xứ suốt thời gian đó. Vì LuuCoNhatKy đặt <c>lock_timeout = 5s</c>, MỌI người
    /// khác trong giáo xứ bấm Lưu ở bất cứ màn hình nào lúc ấy sẽ chờ rồi nhận lỗi 55P03 không
    /// hiểu nổi — cha xứ bấm một nút là cả văn phòng tê liệt. Đúng thắt cổ chai đã sửa ở
    /// ChuanHoaDuLieuService (Task 4), TimThayTheService (Task 5), ChuyenHoService (Task 6).
    ///
    /// KHÔNG dùng Take hay Skip/Take: khác ba chỗ trên, tập hợp cần ghi ở đây KHÔNG phải một
    /// truy vấn nạp lại từng lô mà là kết quả TinhToan đã tính TRỌN trong bộ nhớ trước khi ghi
    /// (phải vậy: gộp nhóm theo linh mục/ngày cần nhìn toàn bộ tập). Nên chia lô bằng cách đi
    /// theo CHỈ SỐ trên danh sách công việc, như ChuyenHoGiaDinh dùng GetRange — không có
    /// truy vấn nào để Skip cả. Biên lô rơi đúng giữa hai <see cref="CongViecGhi"/>.
    ///
    /// ĐÁNH ĐỔI ĐÃ BIẾT: lỗi giữa chừng để lại một phần đợt đã tạo, phần còn lại chưa. Chấp
    /// nhận được vì công cụ này CHẠY LẠI ĐƯỢC: nó CHỈ THÊM MỚI và bỏ qua mọi đợt/chi tiết đã
    /// tồn tại (xem TinhToan), nên bấm lại chỉ làm nốt phần còn thiếu — đúng điều test
    /// "Chay_lai_lan_2_khong_tao_trung_dot_va_khong_tao_trung_chi_tiet" khoá lại.
    /// </summary>
    public async Task<(TaoDotBiTichTuDongKetQua? KetQua, string? Loi)> TaoTuDong(TaoDotBiTichTuDongRequest yc, CancellationToken ct)
    {
        var giaoXuId = boiCanh.GiaoXuId;

        // Tính toàn bộ TRƯỚC, ngoài mọi giao dịch ghi: đây chỉ là đọc, không cần giữ khoá nào.
        var kq = await TinhToan(yc, ct);
        var maHienTai = await db.DotBiTich.MaxAsync(x => (int?)x.MaDotBiTichCu, ct) ?? 0;

        var congViec = kq.DotMoi
            .Select(d => new CongViecGhi(null, d.Ngay, d.LinhMuc, d.Noi, d.GiaoDanIds))
            .Concat(kq.DotDaCo.Select(d => new CongViecGhi(d.DotBiTichId, default, null, null, d.GiaoDanIds)))
            .ToList();

        var soDotDaTao = 0;
        var soGiaoDanDaThem = 0;
        var viTri = 0;

        while (viTri < congViec.Count)
        {
            // Giao dịch tường minh cho từng lô, KHÔNG để LuuCoNhatKy tự mở: trong lô này ta khoá
            // dòng đếm mã cũ (bo_dem_ma, qua SinhMaService) TRƯỚC rồi mới tới dòng đếm hiệu lực
            // (bo_dem_hieu_luc, qua LuuCoNhatKy) — đúng thứ tự khoá của mọi chỗ gọi khác, xem ghi
            // chú ở CapSoHieuLuc. Đặt chung một giao dịch còn để mã cũ đã cấp được quay lui cùng
            // lô khi lô đó hỏng, không để lại lỗ hổng trong chuỗi MaDotBiTichCu.
            await using var giaoTac = await db.Database.BeginTransactionAsync(ct);

            var soDotLo = 0;
            var soGiaoDanLo = 0;
            var soDongLo = 0;

            // Nhận thêm công việc tới khi đủ CoLo DÒNG. Điều kiện kiểm ở ĐẦU vòng nên một công
            // việc lớn hơn CoLo vẫn được nhận trọn (không bao giờ cắt đôi một đợt và chi tiết
            // của nó), chỉ khiến riêng lô đó to hơn dự kiến.
            while (viTri < congViec.Count && soDongLo < CoLo)
            {
                var cv = congViec[viTri];
                if (cv.DotBiTichId is { } dotDaCoId)
                {
                    foreach (var giaoDanId in cv.GiaoDanIds)
                        db.BiTichChiTiet.Add(new BiTichChiTiet { GiaoXuId = giaoXuId, DotBiTichId = dotDaCoId, GiaoDanId = giaoDanId });
                    soDongLo += cv.GiaoDanIds.Count;
                }
                else
                {
                    maHienTai = await sinhMa.LayMaTiepTheo(giaoXuId, "dot_bi_tich", maHienTai, ct);
                    var dot = new DotBiTich
                    {
                        GiaoXuId = giaoXuId,
                        LoaiBiTich = yc.LoaiBiTich,
                        NgayBiTich = cv.Ngay,
                        LinhMuc = cv.LinhMuc,
                        NoiBiTich = cv.Noi,
                        MoTa = TaoMoTa(cv.Ngay),
                        MaDotBiTichCu = maHienTai,
                    };
                    db.DotBiTich.Add(dot);
                    soDotLo++;
                    foreach (var giaoDanId in cv.GiaoDanIds)
                        db.BiTichChiTiet.Add(new BiTichChiTiet { GiaoXuId = giaoXuId, DotBiTichId = dot.Id, GiaoDanId = giaoDanId });
                    soDongLo += 1 + cv.GiaoDanIds.Count;
                }
                soGiaoDanLo += cv.GiaoDanIds.Count;
                viTri++;
            }

            try
            {
                await db.LuuCoNhatKy(ct);
                await giaoTac.CommitAsync(ct);
            }
            catch (DbUpdateException ex) when (LaViPhamRangBuocNhomTrung(ex))
            {
                await giaoTac.RollbackAsync(ct);
                return (null, "Không tạo được trọn vẹn: một yêu cầu khác vừa tạo đợt bí tích trùng cùng " +
                    "linh mục/ngày trong lúc thao tác này đang chạy (có thể do bấm \"Xác nhận tạo\" nhiều " +
                    "lần). Một phần đợt có thể đã được tạo xong. Hãy tải lại trang để xem đợt bí tích hiện " +
                    "có, rồi chạy lại nếu còn thiếu — chạy lại không tạo trùng.");
            }

            soDotDaTao += soDotLo;
            soGiaoDanDaThem += soGiaoDanLo;

            // Nhả thực thể của lô vừa xong khỏi ChangeTracker — giữ hết tới cuối chỉ làm mỗi lần
            // SaveChanges quét chậm dần.
            db.ChangeTracker.Clear();
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
