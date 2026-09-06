namespace Qlgx.Api.Dtos;

public record DangNhapRequest(string TenTaiKhoan, string MatKhau);

public record TaiKhoanItemDto(Guid Id, string TenTaiKhoan, string? HoTenNguoiDung, string? Email,
    string? SoDienThoai, int LoaiTaiKhoan, string? TenLoai, uint RowVersion);

public record TaoTaiKhoanRequest(string TenTaiKhoan, string MatKhau, string HoTenNguoiDung,
    string? Email, string? SoDienThoai, int LoaiTaiKhoan);

public record CapNhatTaiKhoanRequest(string HoTenNguoiDung, string? Email, string? SoDienThoai,
    int LoaiTaiKhoan, string? MatKhauMoi, uint RowVersion);
