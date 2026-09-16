namespace Qlgx.Api.Dtos;

/// <summary>"Chuẩn hoá dữ liệu" (nhóm Công cụ dữ liệu) — xem
/// docs/superpowers/specs/man-hinh/cong-cu-du-lieu.md mục 5.1/9.</summary>
public record TruongThayDoiDto(string TenTruong, string? GiaTriCu, string? GiaTriMoi);

public record DongThayDoiDto(Guid Id, string NhanDien, List<TruongThayDoiDto> Truong);

/// <summary>Kết quả bước "Xem trước" (chạy thử, KHÔNG ghi gì) — <c>SoBanGhiSeDoi</c> là con số
/// THẬT (đếm toàn bộ), <c>MauThayDoi</c> chỉ là MẪU (tối đa 30 dòng đầu tiên có thay đổi) để
/// người dùng thấy "đổi thành gì" mà không phải tải hết hàng nghìn dòng về trình duyệt.</summary>
public record ChuanHoaXemTruocKetQua(int TongSoBanGhiKiemTra, int SoBanGhiSeDoi, List<DongThayDoiDto> MauThayDoi);

public record ChuanHoaKetQua(int SoBanGhiDaDoi);
