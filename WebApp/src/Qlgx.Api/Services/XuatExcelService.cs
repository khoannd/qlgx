using ClosedXML.Excel;
using Qlgx.Api.Dtos;
using Qlgx.Domain;

namespace Qlgx.Api.Services;

/// <summary>
/// Xuất "danh sách giáo dân"/"danh sách gia đình" ra tệp .xlsx THẬT (ClosedXML, không Office
/// Interop — cấm dùng trên máy chủ, xem CLAUDE.md) — thay nút "Xuất dữ liệu (CSV)" cũ theo góp ý
/// người dùng: "Xuất CSV tôi muốn xuất Excel có format như trên grid, vì người dùng thông thường
/// không dùng CSV".
///
/// CỐ Ý gọi thẳng <see cref="GiaoDanService.LayDanhSach"/>/<see cref="GiaDinhService.LayDanhSach"/>
/// (KHÔNG viết lại điều kiện WHERE nào ở đây) — endpoint nhận đúng bộ tham số lọc
/// (giaoHoId/chiKhongThongKe/hienCaDaMat) mà hai màn hình danh sách đã dùng cho GET danh sách
/// thường, đảm bảo Excel xuất ra LUÔN khớp với những gì LayDanhSach trả về cho cùng bộ lọc đó —
/// không có nguy cơ hai nơi lọc lệch nhau theo thời gian.
///
/// Tên cột/thứ tự cột chép NGUYÊN VĂN từ `cotGiaoDan.ts`/`cotGiaDinh.ts` phía web (xem chú thích
/// ở mỗi hàm) để khớp đúng lưới đang hiển thị — sửa cột ở một trong hai nơi mà quên nơi kia sẽ
/// làm Excel xuất ra lệch khỏi lưới, xem can-review-sau.md mục "Excel — đối chiếu cột".
/// </summary>
public class XuatExcelService(
    GiaoDanService giaoDan, GiaDinhService giaDinh, DotBiTichService dotBiTich, RaoHonPhoiService raoHonPhoi)
{
    private const string DauTich = "✓";
    private const string DauGach = "—";

    private static string Bool(bool v) => v ? DauTich : DauGach;
    private static string Ngay(DateOnly? v) => v?.ToString("dd/MM/yyyy") ?? DauGach;
    private static string Chuoi(string? v) => string.IsNullOrEmpty(v) ? DauGach : v;

    /// <summary>
    /// 29 cột đúng thứ tự `cotGiaoDan.ts` (GxGiaoDanList.FormatGrid() gốc). KHÔNG gạch ngang
    /// dòng nào — đúng hành vi lưới GIÁO DÂN đứng một mình trên màn hình "Danh sách giáo dân"
    /// (`GxGiaoDanList` gọi với `quanHeGiaDinh` không truyền ⇒ `toDo` là `undefined`, xem
    /// can-review-sau.md mục 7): gạch ngang chỉ có ý nghĩa khi lưới này nhúng trong form gia
    /// đình (lưới "Thành viên khác"), một ngữ cảnh không xuất Excel từ toolbar riêng.
    /// </summary>
    public async Task<byte[]> XuatGiaoDan(
        Guid? giaoHoId, bool chiKhongThongKe, bool hienCaDaMat, CancellationToken ct) =>
        DungExcelGiaoDan(await giaoDan.LayDanhSach(giaoHoId, chiKhongThongKe, hienCaDaMat, ct));

    /// <summary>"Hồ sơ lưu trữ giáo dân" (`btnInDanhSach_Click`/PrintButton của
    /// `frmGiaoDanLuuTruList.cs`, cùng cơ chế GridEXExporter xuất .xls tạm trên desktop) — cùng
    /// 29 cột, chỉ khác nguồn dữ liệu là <see cref="GiaoDanService.LayDanhSachLuuTru"/>.</summary>
    public async Task<byte[]> XuatGiaoDanLuuTru(
        Guid? giaoHoId, bool chiKhongThongKe, CancellationToken ct) =>
        DungExcelGiaoDan(await giaoDan.LayDanhSachLuuTru(giaoHoId, chiKhongThongKe, ct));

    private static byte[] DungExcelGiaoDan(List<GiaoDanListItemDto> rows)
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Giáo dân");

        string[] tieuDe =
        [
            "Mã GD", "Tên thánh", "Họ tên", "Phái", "Ngày sinh", "Ngày rửa tội", "Ngày XTRL",
            "Ngày Th.Sức", "Lập GĐ", "Cha", "Mẹ", "Tân tòng", "Còn học", "Nghề nghiệp",
            "Ghi chú", "Điện thoại", "Địa chỉ", "Giáo họ", "Đã chuyển đi", "Văn hóa",
            "Chuyên môn", "Ngoại ngữ", "Qua đời", "Ngày qua đời", "Nơi an táng", "Nơi sinh",
            "Nơi rửa tội", "Nơi XTRL", "Nơi thêm sức",
        ];
        GhiTieuDe(ws, tieuDe);

        var hang = 2;
        foreach (var g in rows)
        {
            var c = 1;
            ws.Cell(hang, c++).Value = g.MaGiaoDanCu;
            ws.Cell(hang, c++).Value = Chuoi(g.TenThanh);
            ws.Cell(hang, c++).Value = g.HoTen;
            ws.Cell(hang, c++).Value = Chuoi(g.Phai);
            ws.Cell(hang, c++).Value = Ngay(g.NgaySinh);
            ws.Cell(hang, c++).Value = Ngay(g.NgayRuaToi);
            ws.Cell(hang, c++).Value = Ngay(g.NgayRuocLe);
            ws.Cell(hang, c++).Value = Ngay(g.NgayThemSuc);
            ws.Cell(hang, c++).Value = Bool(g.LapGd);
            ws.Cell(hang, c++).Value = Chuoi(g.HoTenCha);
            ws.Cell(hang, c++).Value = Chuoi(g.HoTenMe);
            ws.Cell(hang, c++).Value = Bool(g.TanTong);
            ws.Cell(hang, c++).Value = Bool(g.ConHoc);
            ws.Cell(hang, c++).Value = Chuoi(g.NgheNghiep);
            ws.Cell(hang, c++).Value = Chuoi(g.GhiChu);
            ws.Cell(hang, c++).Value = Chuoi(g.DienThoai);
            ws.Cell(hang, c++).Value = Chuoi(g.DiaChi);
            ws.Cell(hang, c++).Value = Chuoi(g.TenGiaoHo);
            ws.Cell(hang, c++).Value = Bool(g.DaChuyenDi);
            ws.Cell(hang, c++).Value = Chuoi(g.TrinhDoVanHoa);
            ws.Cell(hang, c++).Value = Chuoi(g.TrinhDoChuyenMon);
            ws.Cell(hang, c++).Value = Chuoi(g.BietNgoaiNgu);
            ws.Cell(hang, c++).Value = Bool(g.QuaDoi);
            ws.Cell(hang, c++).Value = Ngay(g.NgayQuaDoi);
            ws.Cell(hang, c++).Value = Chuoi(g.NoiAnTang);
            ws.Cell(hang, c++).Value = Chuoi(g.NoiSinh);
            ws.Cell(hang, c++).Value = Chuoi(g.NoiRuaToi);
            ws.Cell(hang, c++).Value = Chuoi(g.NoiRuocLe);
            ws.Cell(hang, c++).Value = Chuoi(g.NoiThemSuc);
            hang++;
        }

        HoanTat(ws, tieuDe.Length, hang - 1);
        return DungThanhByteArray(wb);
    }

    /// <summary>
    /// 12 cột đúng thứ tự `cotGiaDinh.ts`. Gạch ngang từng Ô (không phải cả dòng) đúng quy tắc
    /// `cellClass` của cột — "Người nam" khi `Gach` là 0 hoặc 2, "Người nữ" khi `Gach` là 1 hoặc
    /// 2 — chép nguyên văn điều kiện đó, xem ghi chú "Gạch ngang đỏ" dưới lưới gia đình.
    /// </summary>
    public async Task<byte[]> XuatGiaDinh(Guid? giaoHoId, bool chiKhongThongKe, CancellationToken ct) =>
        DungExcelGiaDinh(await giaDinh.LayDanhSach(giaoHoId, chiKhongThongKe, ct));

    /// <summary>"Hồ sơ lưu trữ gia đình" (`btnInDanhSach_Click`/PrintButton của
    /// `frmGiaDinhLuuTruList.cs`) — cùng 12 cột, chỉ khác nguồn dữ liệu là
    /// <see cref="GiaDinhService.LayDanhSachLuuTru"/>.</summary>
    public async Task<byte[]> XuatGiaDinhLuuTru(Guid? giaoHoId, bool chiKhongThongKe, CancellationToken ct) =>
        DungExcelGiaDinh(await giaDinh.LayDanhSachLuuTru(giaoHoId, chiKhongThongKe, ct));

    private static byte[] DungExcelGiaDinh(List<GiaDinhListItemDto> rows)
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Gia đình");

        string[] tieuDe =
        [
            "Mã GĐ", "Tên gia đình", "Người nam", "Người nữ", "Số người", "Điện thoại",
            "ĐT chồng", "ĐT vợ", "Địa chỉ", "Giáo họ", "Diện gia đình", "Ghi chú",
        ];
        GhiTieuDe(ws, tieuDe);

        var hang = 2;
        foreach (var g in rows)
        {
            var c = 1;
            ws.Cell(hang, c++).Value = g.MaGiaDinhCu;
            ws.Cell(hang, c++).Value = Chuoi(g.TenGiaDinh);
            var oChong = ws.Cell(hang, c++);
            oChong.Value = Chuoi(g.TenChong);
            if (g.Gach == 0 || g.Gach == 2) oChong.Style.Font.Strikethrough = true;
            var oVo = ws.Cell(hang, c++);
            oVo.Value = Chuoi(g.TenVo);
            if (g.Gach == 1 || g.Gach == 2) oVo.Style.Font.Strikethrough = true;
            ws.Cell(hang, c++).Value = g.SoLuong;
            ws.Cell(hang, c++).Value = Chuoi(g.DienThoai);
            ws.Cell(hang, c++).Value = Chuoi(g.DTChong);
            ws.Cell(hang, c++).Value = Chuoi(g.DTVo);
            ws.Cell(hang, c++).Value = Chuoi(g.DiaChi);
            ws.Cell(hang, c++).Value = Chuoi(g.TenGiaoHo);
            ws.Cell(hang, c++).Value = Chuoi(g.DienGiaDinh);
            ws.Cell(hang, c++).Value = Chuoi(g.GhiChu);
            hang++;
        }

        HoanTat(ws, tieuDe.Length, hang - 1);
        return DungThanhByteArray(wb);
    }

    /// <summary>
    /// Xuất Excel "Danh sách sổ bí tích" — hoãn lại ở lượt migrate màn hình (commit aa4a2af, xem
    /// docs/superpowers/specs/man-hinh/in-an.md mục 8). CÙNG BA tham số lọc với GET "" của
    /// <see cref="DotBiTichService.LayDanhSach"/> (loaiBiTich/tuNam/denNam) — không viết lại điều
    /// kiện lọc, đúng nguyên tắc đã áp dụng cho Giáo dân/Gia đình ở trên.
    ///
    /// 5 cột đúng thứ tự `cotDotBiTich.ts` (khớp `GxDotBiTichList.FormatGrid`, Source/GXControl/
    /// GxDotBiTichList.cs:53-91): Ngày, Mô tả, Người ban bí tích, Nơi nhận bí tích, Số lượng GD —
    /// ở MỨC ĐỢT, không xuất chi tiết từng người nhận trong đợt (bảng
    /// <c>BiTichChiTiet</c>/`GxBiTichChiTiet.FormatGrid` không có màn hình danh sách/bộ lọc
    /// riêng để tái dùng — xem can-review-sau.md).
    /// </summary>
    public async Task<byte[]> XuatDotBiTich(LoaiBiTich loaiBiTich, int? tuNam, int? denNam, CancellationToken ct) =>
        DungExcelDotBiTich(await dotBiTich.LayDanhSach(loaiBiTich, tuNam, denNam, ct));

    private static readonly Dictionary<LoaiBiTich, string> s_tenLoaiBiTich = new()
    {
        [LoaiBiTich.RuaToi] = "Rửa tội",
        [LoaiBiTich.RuocLe] = "Rước lễ (XTRL lần đầu)",
        [LoaiBiTich.ThemSuc] = "Thêm sức",
    };

    private static byte[] DungExcelDotBiTich(List<DotBiTichListItemDto> rows)
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Sổ bí tích");

        string[] tieuDe = ["Ngày", "Mô tả", "Người ban bí tích", "Nơi nhận bí tích", "Số lượng GD"];
        GhiTieuDe(ws, tieuDe);

        var hang = 2;
        foreach (var d in rows)
        {
            var c = 1;
            ws.Cell(hang, c++).Value = Ngay(d.NgayBiTich);
            ws.Cell(hang, c++).Value = Chuoi(
                s_tenLoaiBiTich.TryGetValue(d.LoaiBiTich, out var ten) ? $"{ten} — {d.MoTa}" : d.MoTa);
            ws.Cell(hang, c++).Value = Chuoi(d.LinhMuc);
            ws.Cell(hang, c++).Value = Chuoi(d.NoiBiTich);
            ws.Cell(hang, c++).Value = d.SoLuong;
            hang++;
        }

        HoanTat(ws, tieuDe.Length, hang - 1);
        return DungThanhByteArray(wb);
    }

    /// <summary>
    /// Xuất Excel "Danh sách rao hôn phối" — hoãn lại cùng lượt với trên. CÙNG tham số lọc
    /// (<paramref name="xemTatCa"/>) với GET "" của <see cref="RaoHonPhoiService.LayDanhSach"/>.
    ///
    /// 8 cột đúng thứ tự `cotRaoHonPhoi.ts` (khớp `GxRaoHonPhoiList.FormatGrid`, Source/GXControl/
    /// GxRaoHonPhoiList.cs:95-164 — chú thích ở `cotRaoHonPhoi.ts` ghi "7 cột" nhưng liệt kê đủ
    /// 8, không sửa ở đây): Mã rao, Đôi rao, Người thứ nhất, Người thứ hai, Rao lần 1/2/3, Ghi
    /// chú — mức TÓM TẮT trên lưới danh sách, KHÔNG phải 26 cột chi tiết đầy đủ của
    /// `frmRaoHonPhoi` (mẫu Excel gốc `DanhSachRaoHonPhoi.xls`/`ExportGrid.cs` dùng cơ chế
    /// GridEX xuất nguyên lưới đang hiển thị — cùng lưới 7 cột này — xem can-review-sau.md).
    /// </summary>
    public async Task<byte[]> XuatRaoHonPhoi(bool xemTatCa, CancellationToken ct) =>
        DungExcelRaoHonPhoi(await raoHonPhoi.LayDanhSach(xemTatCa, ct));

    private static byte[] DungExcelRaoHonPhoi(List<RaoHonPhoiListItemDto> rows)
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Rao hôn phối");

        string[] tieuDe =
            ["Mã rao", "Đôi rao", "Người thứ nhất", "Người thứ hai", "Rao lần 1", "Rao lần 2", "Rao lần 3", "Ghi chú"];
        GhiTieuDe(ws, tieuDe);

        var hang = 2;
        foreach (var r in rows)
        {
            var c = 1;
            ws.Cell(hang, c++).Value = r.MaRaoHonPhoiCu;
            ws.Cell(hang, c++).Value = Chuoi(r.TenRaoHonPhoi);
            ws.Cell(hang, c++).Value = Chuoi(r.Nguoi1);
            ws.Cell(hang, c++).Value = Chuoi(r.Nguoi2);
            ws.Cell(hang, c++).Value = Ngay(r.NgayRaoLan1);
            ws.Cell(hang, c++).Value = Ngay(r.NgayRaoLan2);
            ws.Cell(hang, c++).Value = Ngay(r.NgayRaoLan3);
            ws.Cell(hang, c++).Value = Chuoi(r.GhiChu);
            hang++;
        }

        HoanTat(ws, tieuDe.Length, hang - 1);
        return DungThanhByteArray(wb);
    }

    private static void GhiTieuDe(IXLWorksheet ws, string[] tieuDe)
    {
        for (var i = 0; i < tieuDe.Length; i++)
        {
            var o = ws.Cell(1, i + 1);
            o.Value = tieuDe[i];
            o.Style.Font.Bold = true;
        }
    }

    /// <summary>Đóng băng hàng tiêu đề, tự co giãn độ rộng cột theo nội dung (đủ đọc, không quá
    /// hẹp/rộng) — <see cref="IXLWorksheet.Columns()"/> ở đây đã giới hạn đúng số cột thật của
    /// từng lưới (không lan ra hết `XFD` — cột trống theo mặc định của ClosedXML) nhờ tham số
    /// `soCot`.</summary>
    private static void HoanTat(IXLWorksheet ws, int soCot, int soDong)
    {
        ws.SheetView.FreezeRows(1);
        if (soDong > 0) ws.Range(1, 1, soDong, soCot).SetAutoFilter();
        ws.Columns(1, soCot).AdjustToContents();
    }

    private static byte[] DungThanhByteArray(XLWorkbook wb)
    {
        using var luong = new MemoryStream();
        wb.SaveAs(luong);
        return luong.ToArray();
    }
}
