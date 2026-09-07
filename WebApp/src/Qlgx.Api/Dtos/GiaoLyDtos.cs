namespace Qlgx.Api.Dtos;

/// <summary>
/// DTO cho phân hệ Giáo lý (Khối → Lớp → Học viên/Giáo lý viên) — xem
/// docs/superpowers/specs/man-hinh/giao-ly.md.
/// </summary>
public record KhoiGiaoLyDto(
    Guid Id, int MaKhoiCu, string TenKhoi, Guid? NguoiQuanLyId, string? TenNguoiQuanLy,
    string? GhiChu, int SoLop, uint RowVersion);

/// <summary>Thêm/sửa một khối — NguoiQuanLyId bắt buộc khớp `checkInput()` của
/// `frmKhoiGiaoLy.cs:252-257` ("Hãy chọn người quản lý") dù cột CSDL nullable — xem giao-ly.md
/// mục 2 cho lý do mâu thuẫn này. RowVersion null khi tạo mới.</summary>
public record LuuKhoiGiaoLyRequest(string TenKhoi, Guid NguoiQuanLyId, string? GhiChu, uint? RowVersion);

public record LopGiaoLyDto(
    Guid Id, int MaLopCu, string TenLop, Guid KhoiGiaoLyId, int? Nam, string? PhongHoc,
    string? GhiChu, int SoHocVien, string? TenGiaoLyVien, uint RowVersion);

/// <summary>Thêm/sửa một lớp — TenLop bắt buộc (`"Hãy nhập tên lớp giáo lý"`,
/// `frmLopGiaoLy.cs:444-449`). Nam sửa được tự do ở bản web (khác bản gốc khoá cứng theo năm
/// đang chọn ở danh mục khối — xem giao-ly.md mục 8 "Cố ý MỞ RỘNG").</summary>
public record LuuLopGiaoLyRequest(string TenLop, int? Nam, string? PhongHoc, string? GhiChu, uint? RowVersion);

/// <summary>Một học viên trên lưới `gxHocSinhList1` (`GxHocSinh.cs:275-330`) — chỉ giữ các cột
/// còn hợp lý ở web (bỏ Ngày XTRLLĐ/Tên cha/Tên mẹ suy ra được từ hồ sơ giáo dân đầy đủ nếu cần
/// xem, không lặp lại ở đây).</summary>
public record HocVienLopGiaoLyDto(
    Guid ChiTietId, Guid GiaoDanId, int? SoThuTu, string HoTen, string? TenThanh, string? Phai,
    DateOnly? NgaySinh, bool HoanThanh, string? GhiChuGLy, uint RowVersion);

public record ThemHocVienRequest(Guid GiaoDanId);

/// <summary>Sửa Số thứ tự/Hoàn thành/Ghi chú của một học viên — khớp việc sửa trực tiếp trên ô
/// lưới `gxHocSinhList1` (`AllowEdit=True`) ở bản gốc, bản web dùng form riêng (cùng tinh thần
/// "cho sửa" đã áp dụng cho hội đoàn — xem giao-ly.md mục 9).</summary>
public record SuaHocVienRequest(int? SoThuTu, bool HoanThanh, string? GhiChuGLy, uint RowVersion);

public record GiaoLyVienDto(Guid Id, Guid GiaoDanId, string HoTen, string? TenThanh, uint RowVersion);

public record ThemGiaoLyVienRequest(Guid GiaoDanId);
