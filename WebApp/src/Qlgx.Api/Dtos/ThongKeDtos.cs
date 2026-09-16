namespace Qlgx.Api.Dtos;

/// <summary>
/// 16 điều kiện trích xuất của tab "Thống kê chung" (`GxThongKeChung.cs`, `enum Condition`,
/// đúng thứ tự combo `SetCbCondition` — xem docs/superpowers/specs/man-hinh/thong-ke-bieu-do.md
/// mục 4.1). Thứ tự PHẢI giữ nguyên vì bản gốc so `SelectedIndex` với enum theo vị trí.
/// </summary>
public enum DieuKienThongKe
{
    SinhRa, RuaToi, RuocLeLanDau, ThemSuc, HonPhoi, QuaDoi, TongSoGiaoDan, TongSoGiaDinh,
    TanTong, ChuHo, GiaTruong, HienMau, CaoNien, GioiTre, ThieuNhi, KyNiemHonPhoi,
}

/// <summary>Đúng 6 mục của `gxCbMarried` (`GxThongKeChung.cs:553-558`), theo thứ tự combo.</summary>
public enum TrangThaiHonPhoiThongKe
{
    KhongPhanLoai, Chuan, HopThucHoa, HopPhap, LyDi, LyThan,
}

/// <summary>
/// Một dòng hôn phối cho mục đích thống kê — bản web KHÔNG có endpoint "danh sách hôn phối"
/// tổng quát nào sẵn để dùng lại (khác giáo dân/gia đình), phải tự tạo DTO mới thay cho view
/// Access `SELECT_HONPHOI_LIST` (xem thong-ke-bieu-do.md mục 6 và mục 8.4). Cột chọn tối thiểu
/// đủ để đối chiếu số liệu, KHÔNG bắt buộc pivot y hệt view Access (đã hỏng/không đọc lại được
/// từ mã nguồn — mục 9).
/// </summary>
public record HonPhoiThongKeDto(
    Guid Id, int MaHonPhoiCu, string? TenHonPhoi, string? SoHonPhoi, string? NoiHonPhoi,
    DateOnly? NgayHonPhoi, string? LinhMucChung, string? CachThucHonPhoi, string? GhiChu,
    string? TenChong, string? TenVo, string? TenGiaoHo);

/// <summary>
/// Kết quả tab "Thống kê chung" — CHỈ một trong ba mảng có giá trị (khác null), tương ứng đúng MỘT
/// trong ba lưới chồng nhau của bản desktop (`gxGiaoDanList1`/`gxGiaDinhList1`/`gxHonPhoiList1`,
/// chỉ một cái `Visible=true` — xem thong-ke-bieu-do.md mục 2.1/5).
/// </summary>
public record ThongKeChungKetQua(
    int TongCong, string Nhan,
    List<GiaoDanListItemDto>? GiaoDan,
    List<GiaDinhListItemDto>? GiaDinh,
    List<HonPhoiThongKeDto>? HonPhoi);

/// <summary>Một dòng của tab "Thống kê ơn gọi tận hiến" — cột giáo dân rút gọn (đủ để đối chiếu,
/// KHÔNG lặp lại đủ 29 cột của `cotGiaoDan`) cộng 5 cột từ `TanHien` (`GxThongKeOnGoi.cs:86-96`).</summary>
public record OnGoiListItemDto(
    Guid Id, int MaGiaoDanCu, string? TenThanh, string HoTen, string? Phai, DateOnly? NgaySinh,
    string? DienThoai, string? DiaChi, string? TenGiaoHo,
    DateOnly? NgayBatDau, string? ChucVu, string? NoiTu, string? DongTu, string? NoiPhucVu);

public record ThongKeOnGoiKetQua(int TongCong, List<OnGoiListItemDto> Rows);

/// <summary>Một năm của biểu đồ "Tổng giáo dân" (luỹ kế đến 31/12 năm đó) hoặc "Tổng hôn phối"
/// (theo riêng từng năm) — xem thong-ke-bieu-do.md mục 4.6.</summary>
public record BieuDoNamDto(int Nam, int SoLuong);

/// <summary>Một năm của biểu đồ "Tình hình bí tích" — 4 chuỗi Sinh ra/Rửa tội/XTRL/Thêm sức.</summary>
public record BieuDoBiTichNamDto(int Nam, int SinhRa, int RuaToi, int XtrlLanDau, int ThemSuc);

/// <summary>7 nhóm tuổi cố định của biểu đồ "So sánh độ tuổi" — `Tren50` LUÔN bằng 0 cho tới năm
/// 2041 (bug cận đảo ngược của bản gốc, xem thong-ke-bieu-do.md mục 4.6, migrate y hệt).</summary>
public record BieuDoDoTuoiDto(
    int Duoi7, int Tu7Den12, int Tu13Den16, int Tu17Den25, int Tu26Den30, int Tu31Den50, int Tren50);

public record BieuDoGiaoHoDto(string TenGiaoHo, int SoLuong);
