namespace Qlgx.Api.Dtos;

/// <summary>Một dòng lịch sử để hiện trên màn hình — đã gộp tên người sửa, không lộ Id nội bộ.
/// <see cref="TenNguoiSua"/> null khi TaiKhoanId của dòng thay_doi là null (khoảng trống đã
/// biết: chưa nối IBoiCanhGhiNhatKy thật vào hệ xác thực — xem Task 4/6) HOẶC khi tài khoản đó
/// đã bị xoá — hai trường hợp này không phân biệt được ở tầng DTO, phía hiển thị chỉ cần biết
/// "không rõ ai sửa".</summary>
public record DongNhatKyDto(
    string Truong, string? GiaTri, string Loai, DateTimeOffset Luc, string? TenNguoiSua);
