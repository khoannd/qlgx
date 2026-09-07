namespace Qlgx.Api.Dtos;

/// <summary>GiaoXuId chỉ cần điền khi máy chủ trả lời "cần chọn giáo xứ" ở lượt gửi trước
/// (tên đăng nhập trùng ở nhiều giáo xứ) — xem AuthService.DangNhap.</summary>
public record DangNhapRequest(string TenTaiKhoan, string MatKhau, Guid? GiaoXuId = null);

/// <summary>Người dùng tự đổi mật khẩu của chính mình — bắt buộc kèm mật khẩu HIỆN TẠI để xác
/// thực (VIEC-TIEP-THEO.md mục 1.3), không ai đổi được mật khẩu người khác qua endpoint này vì
/// TaiKhoanId luôn lấy từ claim của token, không phải tham số.</summary>
public record DoiMatKhauRequest(string MatKhauHienTai, string MatKhauMoi);

public record TaiKhoanItemDto(Guid Id, string TenTaiKhoan, string? HoTenNguoiDung, string? Email,
    string? SoDienThoai, int LoaiTaiKhoan, string? TenLoai, uint RowVersion);

public record TaoTaiKhoanRequest(string TenTaiKhoan, string MatKhau, string HoTenNguoiDung,
    string? Email, string? SoDienThoai, int LoaiTaiKhoan);

public record CapNhatTaiKhoanRequest(string HoTenNguoiDung, string? Email, string? SoDienThoai,
    int LoaiTaiKhoan, string? MatKhauMoi, uint RowVersion);
