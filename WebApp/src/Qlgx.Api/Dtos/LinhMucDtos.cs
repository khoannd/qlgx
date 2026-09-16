namespace Qlgx.Api.Dtos;

/// <summary>
/// "Danh sách các cha quản xứ" ở màn hình "Giáo xứ" — thay lưới <c>gxLinhMucList1</c> của
/// <c>frmGiaoXu.cs</c> (bản desktop). LUÔN trong phạm vi giáo xứ của người gọi: bảng LinhMuc kế
/// thừa ThucTheCoSo nên có GiaoXuId, được cả bộ lọc EF Core (QlgxDbContext.OnModelCreating) và
/// Row-Level Security PostgreSQL (BatRlsChoBangTheoGiaoXu) bảo vệ — khác GiaoXu/GiaoHat/GiaoPhan
/// (xem giao-xu.md mục 4), không cần service tự lọc tường minh theo GiaoXuId khi ĐỌC/SỬA/XOÁ.
/// </summary>
public record LinhMucDto(
    Guid Id, int MaLinhMucCu, string? TenThanh, string HoTen, DateOnly? NgaySinh,
    string? ChucVu, DateOnly? TuNgay, DateOnly? DenNgay, string? GhiChu,
    string? DienThoai, string? Email);

public record TaoLinhMucRequest(
    string? TenThanh, string HoTen, DateOnly? NgaySinh, string? ChucVu,
    DateOnly? TuNgay, DateOnly? DenNgay, string? GhiChu, string? DienThoai, string? Email);

public record CapNhatLinhMucRequest(
    string? TenThanh, string HoTen, DateOnly? NgaySinh, string? ChucVu,
    DateOnly? TuNgay, DateOnly? DenNgay, string? GhiChu, string? DienThoai, string? Email);
