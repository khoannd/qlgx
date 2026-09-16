namespace Qlgx.Api.Dtos;

/// <summary>
/// Màn hình "Giáo xứ" tự sửa thông tin xứ mình — thay <c>frmGiaoXu.cs</c> (bản desktop). KHÁC
/// <c>QuanLyGiaoXuDtos</c>: đây CHỈ đọc/ghi đúng giáo xứ trong claim đăng nhập, không nhận
/// GiaoXuId từ trình duyệt. Xem docs/superpowers/specs/man-hinh/giao-xu.md.
/// </summary>
/// <summary>
/// <c>TenGiaoPhan</c>/<c>TenGiaoHat</c> CHỈ ĐỌC — lấy qua GiaoXu.GiaoHat!.GiaoPhan!.TenGiaoPhan,
/// KHÔNG có ô sửa tương ứng ở <see cref="CapNhatGiaoXuHienTaiRequest"/> (xem lý do multi-tenant
/// ở GiaoXuService.LayThongTin và giao-xu.md mục 3.1: một Giáo hạt có thể có NHIỀU Giáo xứ cùng
/// trỏ vào, cho một giáo xứ tự đổi tên sẽ đổi luôn tên hiển thị của giáo xứ khác dùng chung).
/// Cả hai null khi giáo xứ chưa được "Quản lý giáo xứ" gán GiaoHatId.
/// </summary>
public record GiaoXuHienTaiResponse(
    Guid Id, string TenGiaoXu, string? DiaChi, string? DienThoai,
    string? Email, string? Website, string? GhiChu,
    string? TenGiaoPhan, string? TenGiaoHat);

public record CapNhatGiaoXuHienTaiRequest(
    string TenGiaoXu, string? DiaChi, string? DienThoai,
    string? Email, string? Website, string? GhiChu);
