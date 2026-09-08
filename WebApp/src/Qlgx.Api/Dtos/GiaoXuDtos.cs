namespace Qlgx.Api.Dtos;

/// <summary>
/// Màn hình "Giáo xứ" tự sửa thông tin xứ mình — thay <c>frmGiaoXu.cs</c> (bản desktop). KHÁC
/// <c>QuanLyGiaoXuDtos</c>: đây CHỈ đọc/ghi đúng giáo xứ trong claim đăng nhập, không nhận
/// GiaoXuId từ trình duyệt. Xem docs/superpowers/specs/man-hinh/giao-xu.md.
/// </summary>
public record GiaoXuHienTaiResponse(
    Guid Id, string TenGiaoXu, string? DiaChi, string? DienThoai,
    string? Email, string? Website, string? GhiChu);

public record CapNhatGiaoXuHienTaiRequest(
    string TenGiaoXu, string? DiaChi, string? DienThoai,
    string? Email, string? Website, string? GhiChu);
