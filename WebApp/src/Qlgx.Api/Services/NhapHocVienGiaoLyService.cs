using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using Qlgx.Api.Dtos;
using Qlgx.Data;
using Qlgx.Domain.Entities;

namespace Qlgx.Api.Services;

/// <summary>
/// "Nhập học viên hàng loạt" từ Excel — <c>frmImportHocVien.cs</c> (225 dòng, mở hộp thoại chọn
/// tệp rồi chạy nền) + logic thật ở <c>ImportData.ImportGiaoLy</c> (<c>Source/GXControl/
/// ImportData.cs:76-226</c>), hoãn từ commit d4c4b27. Đọc bằng ClosedXML (giữ nguyên hạ tầng đã
/// có, KHÔNG dùng Office Interop), xử lý HOÀN TOÀN TRONG BỘ NHỚ — không ghi tệp lên đĩa máy chủ
/// (ràng buộc HA, cùng lý do AnhDaiDienService/NhapDuLieuService). Kiểm định dạng THẬT bằng cách
/// thử mở workbook (<see cref="XLWorkbook"/> ném lỗi nếu không phải .xlsx thật) — không tin phần
/// mở rộng tên tệp.
///
/// Cột Excel bắt buộc/tuỳ chọn ĐÚNG TÊN LITERAL mà <c>ImportGiaoLy</c> đọc thẳng bằng chỉ mục
/// (KHÔNG dùng <c>ExportDataToExcel.GetColumnInfo()</c> dù có gọi — đó là biến chết, xem
/// giao-ly.md mục 8): "Họ tên", "Phái", "Ngày sinh" (bắt buộc), "Mã GD", "Tên thánh", "Giáo họ",
/// "Ghi chú", "Đã học xong" (tuỳ chọn). So khớp KHÔNG phân biệt hoa/thường và bỏ khoảng trắng đầu
/// cuối (giữ tinh thần <c>DataTable</c> mặc định không phân biệt hoa/thường của bản gốc).
///
/// BA CHỖ THU HẸP PHẠM VI CÓ CHỦ Ý so với bản gốc (ghi ở can-review-sau.md, không phải "sửa lại
/// cho đúng" — chỉ là những phần không thể tái hiện an toàn trên nền CSDL quan hệ có ràng buộc
/// khoá ngoại thật, khác Access/DataSet lỏng lẻo của bản gốc):
/// 1. "Mã GD" phải khớp một `GiaoDan.MaGiaoDanCu` CÓ THẬT — bản gốc dùng thẳng số nhập làm
///    `MaGiaoDan` khi tạo `ChiTietLopGiaoLy` mà KHÔNG kiểm tra tồn tại (Access không ép khoá
///    ngoại chặt như PostgreSQL ở đây) — có "Mã GD" nhưng không khớp ai thì bản web coi là dòng
///    lỗi, bản gốc sẽ tạo ra dữ liệu mồ côi (hoặc lỗi INSERT nếu Jet có ràng buộc, chưa xác nhận
///    được — xem giao-ly.md mục 9).
/// 2. Khi không tìm thấy giáo họ theo tên (`getMaGiaoHo`, `ImportData.cs:860-895`), bản gốc ÂM
///    THẦM TẠO MỚI một `GiaoHo` — bản web KHÔNG tạo giáo họ mới, chỉ cảnh báo dòng và vẫn tạo
///    giáo dân với `GiaoHoId = null` (tương đương "Ngoài xứ", KHÔNG chặn cả dòng — giữ tinh thần
///    "cảnh báo mềm, không chặn" của thông báo gốc `"Không tìm thấy giáo họ của [...]"`).
/// 3. Khi có &gt;1 giáo dân trùng Họ tên+Tên thánh+Phái+Ngày sinh, bản gốc CẢNH BÁO rồi vẫn dùng
///    `Rows[0]` (thứ tự không xác định của DataTable) — bản web dùng dòng có `Id` nhỏ nhất để có
///    kết quả ổn định giữa các lần chạy, vẫn giữ đúng hành vi "cảnh báo nhưng không chặn".
///
/// Giữ NGUYÊN các hành vi khác, kể cả chỗ có vẻ kỳ quặc: dòng thiếu Họ tên/Phái/Ngày sinh bị BỎ
/// QUA (không chặn cả tệp); giáo dân đã có sẵn trong LỚP ĐÍCH bị bỏ qua kèm cảnh báo; giáo dân
/// mới tạo chỉ có 6 cột (TenThanh/HoTen/Phai/NgaySinh/GiaoHoId/DaCoGiaDinh=false) — không suy ra
/// thêm trường nào khác từ ngữ cảnh.
/// </summary>
public class NhapHocVienGiaoLyService(QlgxDbContext db, SinhMaService sinhMa, IBoiCanhGiaoXu boiCanh)
{
    private static readonly string[] CotBatBuoc = ["Họ tên", "Phái", "Ngày sinh"];

    private sealed record HangDoc(
        int SoDong, string? MaGD, string? TenThanh, string HoTen, string? Phai,
        string NgaySinhTho, DateOnly? NgaySinh, string? GiaoHo, string? GhiChu, bool DaHocXong, string? LoiDoc);

    /// <summary>Đọc toàn bộ tệp vào danh sách hàng thô — KHÔNG đụng CSDL, chỉ phân tích Excel.
    /// Trả về <c>(null, "thông báo lỗi")</c> nếu bản thân tệp không đọc được hoặc thiếu cột bắt
    /// buộc (đúng thông báo gốc <c>ImportData.cs:88</c> khi <c>ExcelDataProvider.GetDataSet</c>
    /// thất bại, viết lại ngắn gọn hơn cho web).</summary>
    private static (List<HangDoc>? Hang, string? LoiTep) DocFile(Stream tep)
    {
        IXLWorksheet ws;
        try
        {
            using var wb = new XLWorkbook(tep);
            ws = wb.Worksheets.FirstOrDefault()
                 ?? throw new InvalidOperationException("Tệp Excel không có sheet nào.");
            return DocSheet(ws);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            return (null, "Tệp không đúng định dạng Excel (.xlsx), hoặc tệp bị hỏng. " +
                          "Xin vui lòng kiểm tra lại hoặc tải mẫu mới.");
        }
    }

    private static (List<HangDoc>? Hang, string? LoiTep) DocSheet(IXLWorksheet ws)
    {
        var hangTieuDe = ws.Row(1);
        var coT = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var oCuoi = hangTieuDe.LastCellUsed()?.Address.ColumnNumber ?? 0;
        for (var c = 1; c <= oCuoi; c++)
        {
            var ten = hangTieuDe.Cell(c).GetString().Trim();
            if (ten.Length > 0 && !coT.ContainsKey(ten)) coT[ten] = c;
        }

        var thieu = CotBatBuoc.Where(c => !coT.ContainsKey(c)).ToList();
        if (thieu.Count > 0)
            return (null, $"Tệp thiếu cột bắt buộc: {string.Join(", ", thieu)}. " +
                          "Xin vui lòng tải lại mẫu Excel và không đổi tên dòng tiêu đề.");

        string? Doc(IXLRow hang, string ten) =>
            coT.TryGetValue(ten, out var c) ? NullNeuRong(hang.Cell(c).GetString().Trim()) : null;

        var ds = new List<HangDoc>();
        var hangCuoi = ws.LastRowUsed()?.RowNumber() ?? 1;
        for (var r = 2; r <= hangCuoi; r++)
        {
            var hang = ws.Row(r);
            if (hang.IsEmpty()) continue;

            var hoTen = Doc(hang, "Họ tên");
            var phai = Doc(hang, "Phái");
            var (ngaySinh, ngaySinhTho, loiNgay) = DocNgay(hang, coT);
            var ghiChu = Doc(hang, "Ghi chú");

            string? loi = null;
            if (hoTen is null || phai is null || (ngaySinh is null && loiNgay is null))
                loi = "Không nhập được học viên vì không được nhập đủ thông tin Họ tên, Phái, Ngày sinh";
            else if (loiNgay is not null)
                loi = loiNgay;

            ds.Add(new HangDoc(
                r - 1, Doc(hang, "Mã GD"), Doc(hang, "Tên thánh"), hoTen ?? "", phai,
                ngaySinhTho, ngaySinh, Doc(hang, "Giáo họ"), ghiChu,
                !string.IsNullOrWhiteSpace(Doc(hang, "Đã học xong")), loi));
        }
        return (ds, null);
    }

    private static string? NullNeuRong(string s) => s.Length == 0 ? null : s;

    /// <summary>Ngày sinh: ClosedXML trả kiểu ngày thật nếu ô định dạng ngày, hoặc chuỗi thô nếu
    /// không — thử cả hai, chấp nhận "dd/MM/yyyy" (định dạng ngày Việt Nam dùng xuyên suốt dự
    /// án, xem GxDate).</summary>
    private static (DateOnly? Ngay, string Tho, string? Loi) DocNgay(IXLRow hang, Dictionary<string, int> coT)
    {
        if (!coT.TryGetValue("Ngày sinh", out var c)) return (null, "", null);
        var o = hang.Cell(c);
        if (o.IsEmpty()) return (null, "", null);
        if (o.DataType == XLDataType.DateTime)
        {
            var dt = o.GetDateTime();
            return (DateOnly.FromDateTime(dt), dt.ToString("dd/MM/yyyy"), null);
        }
        var tho = o.GetString().Trim();
        if (DateOnly.TryParseExact(tho, "dd/MM/yyyy", null,
                System.Globalization.DateTimeStyles.None, out var ngay))
            return (ngay, tho, null);
        return (null, tho, $"Không đọc được Ngày sinh \"{tho}\" (đúng định dạng dd/MM/yyyy)");
    }

    /// <summary>Đối chiếu một hàng đã đọc với CSDL hiện tại (đọc-thôi, dùng chung cho cả bước
    /// xem trước lẫn bước ghi thật ngay trước khi ghi) — trả về học viên sẽ dùng
    /// (<paramref name="giaoDanCoSanId"/> khác null) hoặc thông tin để tạo giáo dân mới, cùng
    /// thông báo lỗi/cảnh báo nếu có (không phải lúc nào lỗi cũng chặn dòng — xem `Loi` vs
    /// `BiBoQua` ở nơi gọi).</summary>
    private async Task<(Guid? GiaoDanCoSanId, Guid? GiaoHoIdChoGiaoDanMoi, string? CanhBao)>
        DoiChieuGiaoDan(HangDoc h, CancellationToken ct)
    {
        if (h.MaGD is { } maGDTho && int.TryParse(maGDTho, out var maGDCu))
        {
            var id = await db.GiaoDan.Where(g => g.MaGiaoDanCu == maGDCu).Select(g => (Guid?)g.Id).FirstOrDefaultAsync(ct);
            return id is null
                ? (null, null, $"Không tìm thấy giáo dân mã {maGDCu}")
                : (id, null, null);
        }

        var truyVan = db.GiaoDan.Where(g => g.HoTen == h.HoTen && g.Phai == h.Phai && g.NgaySinh == h.NgaySinh);
        if (h.TenThanh is not null) truyVan = truyVan.Where(g => g.TenThanh == h.TenThanh);
        var trung = await truyVan.OrderBy(g => g.Id).Select(g => g.Id).ToListAsync(ct);
        if (trung.Count > 0)
        {
            var canhBao = trung.Count > 1 ? $"Có {trung.Count} người [{h.HoTen}] trùng thông tin, đã dùng người đầu tiên." : null;
            return (trung[0], null, canhBao);
        }

        // Không tìm thấy giáo dân có sẵn — sẽ tạo mới. Tra giáo họ theo tên (KHÔNG tự tạo giáo
        // họ mới nếu không khớp — xem tóm tắt "thu hẹp phạm vi" ở đầu file).
        Guid? giaoHoId = null;
        string? canhBaoGiaoHo = null;
        if (!string.IsNullOrWhiteSpace(h.GiaoHo) && !h.GiaoHo.Trim().Equals("Ngoài xứ", StringComparison.OrdinalIgnoreCase))
        {
            giaoHoId = await db.GiaoHo.Where(g => !g.DaXoa && g.TenGiaoHo == h.GiaoHo.Trim())
                .Select(g => (Guid?)g.Id).FirstOrDefaultAsync(ct);
            if (giaoHoId is null) canhBaoGiaoHo = $"Không tìm thấy giáo họ của [{h.HoTen}]";
        }
        return (null, giaoHoId, canhBaoGiaoHo);
    }

    public async Task<NhapHocVienXemTruoc> XemTruoc(Guid lopId, Stream tep, CancellationToken ct)
    {
        var lop = await db.LopGiaoLy.FirstOrDefaultAsync(l => l.Id == lopId, ct);
        var tenLop = lop?.TenLop ?? "";
        if (lop is null) return new NhapHocVienXemTruoc(false, "Không tìm thấy lớp giáo lý này.", tenLop, [], 0, 0);

        var (hangDoc, loiTep) = DocFile(tep);
        if (loiTep is not null || hangDoc is null)
            return new NhapHocVienXemTruoc(false, loiTep, tenLop, [], 0, 0);

        var idDaCoTrongLop = (await db.ChiTietLopGiaoLy.Where(c => c.LopGiaoLyId == lopId)
            .Select(c => c.GiaoDanId).ToListAsync(ct)).ToHashSet();

        var ketQua = new List<DongNhapHocVien>();
        int seNhap = 0, boQua = 0;
        // Mô phỏng "đã tồn tại trong danh sách lớp" cho cả những giáo dân MỚI sẽ tạo TRONG chính
        // đợt nhập này nếu trùng nhau nhiều dòng — theo dõi bằng khoá (HoTen,TenThanh,Phai,NgaySinh).
        var seTaoTrongDot = new HashSet<(string, string?, string?, DateOnly?)>();
        foreach (var h in hangDoc)
        {
            if (h.LoiDoc is not null) { ketQua.Add(DongLoi(h, h.LoiDoc)); boQua++; continue; }

            var (idCoSan, _, canhBao) = await DoiChieuGiaoDan(h, ct);
            if (idCoSan is { } id)
            {
                if (idDaCoTrongLop.Contains(id))
                {
                    ketQua.Add(DongLoi(h, $"Không nhập [{h.HoTen}] vì đã tồn tại trong danh sách lớp"));
                    boQua++;
                    continue;
                }
                ketQua.Add(new DongNhapHocVien(h.SoDong, h.MaGD, h.TenThanh, h.HoTen, h.Phai, h.NgaySinhTho,
                    h.GiaoHo, h.GhiChu, h.DaHocXong, false, canhBao));
                seNhap++;
            }
            else
            {
                var khoa = (h.HoTen, h.TenThanh, h.Phai, h.NgaySinh);
                if (!seTaoTrongDot.Add(khoa))
                {
                    ketQua.Add(DongLoi(h, $"Không nhập [{h.HoTen}] vì trùng với một dòng khác vừa tạo mới trong tệp này"));
                    boQua++;
                    continue;
                }
                ketQua.Add(new DongNhapHocVien(h.SoDong, h.MaGD, h.TenThanh, h.HoTen, h.Phai, h.NgaySinhTho,
                    h.GiaoHo, h.GhiChu, h.DaHocXong, true, canhBao));
                seNhap++;
            }
        }
        return new NhapHocVienXemTruoc(true, null, tenLop, ketQua, seNhap, boQua);
    }

    private static DongNhapHocVien DongLoi(HangDoc h, string loi) => new(
        h.SoDong, h.MaGD, h.TenThanh, h.HoTen, h.Phai, h.NgaySinhTho, h.GiaoHo, h.GhiChu, h.DaHocXong, false, loi);

    /// <summary>Ghi thật — MỘT transaction cho toàn bộ đợt nhập (an toàn hơn bản gốc, nơi mỗi
    /// giáo dân mới được `INSERT` ngay lập tức trong vòng lặp còn `ChiTietLopGiaoLy` chỉ ghi một
    /// lần ở cuối qua `Memory.UpdateDataSet` — không transaction, có thể dở dang nếu lỗi giữa
    /// chừng). Chạy lại ĐÚNG logic đối chiếu của <see cref="XemTruoc"/> ngay trước khi ghi (dữ
    /// liệu có thể đã đổi khác giữa lúc xem trước và lúc bấm xác nhận).</summary>
    public async Task<NhapHocVienKetQua?> ThucHien(Guid lopId, Stream tep, CancellationToken ct)
    {
        await using var giaoTac = await db.Database.BeginTransactionAsync(ct);

        var lop = await db.LopGiaoLy.FirstOrDefaultAsync(l => l.Id == lopId, ct);
        if (lop is null) return null;

        var (hangDoc, loiTep) = DocFile(tep);
        if (loiTep is not null || hangDoc is null) return new NhapHocVienKetQua(0, hangDoc?.Count ?? 0);

        var idDaCoTrongLop = (await db.ChiTietLopGiaoLy.Where(c => c.LopGiaoLyId == lopId)
            .Select(c => c.GiaoDanId).ToListAsync(ct)).ToHashSet();
        var giaoXuId = boiCanh.GiaoXuId;
        var soThuTuKeTiep = await db.ChiTietLopGiaoLy.Where(c => c.LopGiaoLyId == lopId)
            .MaxAsync(c => (int?)c.SoThuTu, ct) ?? 0;

        var seTaoTrongDot = new HashSet<(string, string?, string?, DateOnly?)>();
        int daNhap = 0, boQua = 0;
        foreach (var h in hangDoc)
        {
            if (h.LoiDoc is not null) { boQua++; continue; }

            var (idCoSan, giaoHoIdMoi, _) = await DoiChieuGiaoDan(h, ct);
            Guid giaoDanId;
            if (idCoSan is { } id)
            {
                if (idDaCoTrongLop.Contains(id)) { boQua++; continue; }
                giaoDanId = id;
            }
            else
            {
                var khoa = (h.HoTen, h.TenThanh, h.Phai, h.NgaySinh);
                if (!seTaoTrongDot.Add(khoa)) { boQua++; continue; }

                var g = new GiaoDan
                {
                    GiaoXuId = giaoXuId,
                    MaGiaoDanCu = await sinhMa.LayMaTiepTheo(giaoXuId, "giao_dan",
                        await db.GiaoDan.MaxAsync(x => (int?)x.MaGiaoDanCu, ct) ?? 0, ct),
                    MaNhanDang = $"web::giao_dan::{Guid.NewGuid():N}",
                    HoTen = h.HoTen, TenThanh = h.TenThanh, Phai = h.Phai, NgaySinh = h.NgaySinh,
                    GiaoHoId = giaoHoIdMoi, DaCoGiaDinh = false,
                };
                db.GiaoDan.Add(g);
                await db.SaveChangesAsync(ct); // cần Id thật trước khi dùng làm khoá ngoại bên dưới
                giaoDanId = g.Id;
                idDaCoTrongLop.Add(giaoDanId); // phòng trường hợp Mã GD của một dòng sau trùng người vừa tạo
            }

            soThuTuKeTiep++;
            db.ChiTietLopGiaoLy.Add(new ChiTietLopGiaoLy
            {
                GiaoXuId = giaoXuId, LopGiaoLyId = lopId, GiaoDanId = giaoDanId,
                SoThuTu = soThuTuKeTiep, HoanThanh = h.DaHocXong, GhiChuGLy = h.GhiChu,
            });
            idDaCoTrongLop.Add(giaoDanId);
            daNhap++;
        }
        await db.SaveChangesAsync(ct);
        await giaoTac.CommitAsync(ct);
        return new NhapHocVienKetQua(daNhap, boQua);
    }

    /// <summary>Tệp mẫu Excel trống — thay "Xem mẫu Excel" (`Button1` của `frmImportHocVien`,
    /// vốn tải một `.xls` tĩnh đóng gói sẵn trong bản cài desktop) bằng tệp `.xlsx` sinh động chỉ
    /// có đúng dòng tiêu đề khớp <see cref="CotBatBuoc"/> + các cột tuỳ chọn, để người dùng điền
    /// vào rồi nộp lại qua "Nhập học viên hàng loạt".</summary>
    public static byte[] TaoTepMau()
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Sheet1");
        string[] cot = ["Mã GD", "Tên thánh", "Họ tên", "Phái", "Ngày sinh", "Giáo họ", "Ghi chú", "Đã học xong"];
        for (var i = 0; i < cot.Length; i++) ws.Cell(1, i + 1).Value = cot[i];
        ws.Columns().AdjustToContents();
        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }
}
