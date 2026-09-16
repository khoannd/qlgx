namespace Qlgx.Api.Dtos;

/// <summary>
/// DTO cho màn hình "Danh sách hội đoàn" (cấp quản lý danh mục — khác
/// <see cref="HoiDoanDanhMucDto"/>/<see cref="HoiDoanCuaGiaoDanDto"/> dùng cho tab "Hội đoàn"
/// trong chi tiết giáo dân). Khớp `GxListHoiDoan.FormatGrid`
/// (Source/GXControl/GxListHoiDoan.cs:41-90): Mã hội đoàn, Tên hội đoàn, Thánh bổn mạng, Ngày
/// bổn mạng, Ngày thành lập, Ghi chú. Xem docs/superpowers/specs/man-hinh/hoi-doan-danh-sach.md.
/// </summary>
public record HoiDoanQuanLyDto(
    Guid Id, int MaHoiDoanCu, string TenHoiDoan, string? ThanhBonMang,
    DateOnly? NgayBonMang, DateOnly? NgayThanhLap, string? GhiChu,
    int SoHoiVienDangHoatDong, uint RowVersion);

/// <summary>Thêm/sửa một hội đoàn — RowVersion null khi tạo mới (khớp quy ước
/// LuuRaoHonPhoiRequest/CapNhatDotBiTichRequest).</summary>
public record LuuHoiDoanRequest(
    string TenHoiDoan, string? ThanhBonMang, DateOnly? NgayBonMang,
    DateOnly? NgayThanhLap, string? GhiChu, uint? RowVersion);

/// <summary>Một hội viên trên lưới của `frmHoiDoan` (`gxGiaoDanList1` + 3 cột chèn thêm —
/// frmHoiDoan.cs:63-98): Họ tên (từ GiaoDan), Ngày vào/ra hội đoàn, Vai trò. `DaRaKhoiHoiDoan`
/// khớp điều kiện tô đỏ/gạch ngang của `GridEXFormatCondition DaRa` (frmHoiDoan.cs:101-107).
/// </summary>
public record ThanhVienHoiDoanDto(
    Guid ChiTietId, Guid GiaoDanId, string HoTen, string? TenThanh,
    DateOnly? NgayVaoHoiDoan, DateOnly? NgayRaHoiDoan, string? VaiTro,
    bool DaRaKhoiHoiDoan, uint RowVersion);

/// <summary>Thêm một giáo dân có sẵn làm hội viên — VaiTro mặc định "Hội viên" nếu để trống,
/// khớp `frm.DataReturn[ChiTietHoiDoanConst.VaiTro] = "Hội viên"` (frmHoiDoan.cs:496).</summary>
public record ThemThanhVienHoiDoanRequest(
    Guid GiaoDanId, DateOnly? NgayVaoHoiDoan, DateOnly? NgayRaHoiDoan, string? VaiTro);

/// <summary>Sửa một hội viên đã có (ngày vào/ra, vai trò) — bản web CHO PHÉP sửa trực tiếp,
/// khớp đúng cơ chế sửa-trên-lưới của `frmHoiDoan` (khác `GxHistoryHoiDoan`, nơi không sửa
/// được — xem hoi-doan.md mục 8).</summary>
public record SuaThanhVienHoiDoanRequest(
    DateOnly? NgayVaoHoiDoan, DateOnly? NgayRaHoiDoan, string? VaiTro, uint RowVersion);
