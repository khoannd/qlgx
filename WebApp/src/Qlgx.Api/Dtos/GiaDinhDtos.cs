namespace Qlgx.Api.Dtos;

/// <summary>
/// Một dòng trên lưới danh sách gia đình. Sáu trường TenChong, TenVo, DTChong, DTVo,
/// SoLuong, Gach không có trong bảng gia_dinh mà được tính ở tầng dịch vụ — bản Access
/// tính chúng trong view SELECT_GIADINH_LIST.
///
/// HonPhoiId và NgayHonPhoiHienThi không có trong brief gốc: màn hình chi tiết hôn phối
/// nay thuộc Phase 1 nên lưới cần biết gia đình đã có hôn phối chưa. Cả hai đều null khi
/// gia đình chưa có bản ghi hôn phối nào gắn với chồng hoặc vợ.
/// </summary>
public record GiaDinhListItemDto(
    Guid Id,
    int MaGiaDinhCu,
    string? MaGiaDinhRieng,
    string? TenGiaDinh,
    string? TenChong,
    string? TenVo,
    int SoLuong,
    string? DienThoai,
    string? DTChong,
    string? DTVo,
    string? DiaChi,
    string? TenGiaoHo,
    string? DienGiaDinh,
    string? GhiChu,
    int Gach,
    bool KhongThongKe,
    Guid? HonPhoiId,
    string? NgayHonPhoiHienThi);

public record ThanhVienDto(
    Guid GiaoDanId, int VaiTro, bool ChuHo,
    string? TenThanh, string HoTen, string? Phai, DateOnly? NgaySinh, bool QuaDoi, bool DaXoa);

/// <summary>
/// Khối hôn phối của gia đình — 8 cột sửa được của bảng HonPhoi (không gồm MaHonPhoiCu,
/// TenHonPhoi hay MaNhanDang, không dùng ở màn hình này) cộng RowVersion riêng để chống ghi
/// đè, đúng thứ tự và nhãn của form desktop GxHonPhoiGiaDinh. Null khi gia đình chưa có hôn
/// phối nào gắn với chồng hoặc vợ.
/// </summary>
public record HonPhoiDto(
    Guid Id, string? SoHonPhoi, DateOnly? NgayHonPhoi, string? NoiHonPhoi,
    string? LinhMucChung, string? NguoiChung1, string? NguoiChung2,
    string? CachThucHonPhoi, string? GhiChu, uint RowVersion);

public record GiaDinhDetailDto(
    Guid Id, int MaGiaDinhCu, string? MaGiaDinhRieng, string? TenGiaDinh, Guid? GiaoHoId,
    string? DienThoai, string? DiaChi, string? SoHoKhau, string? DienGiaDinh, string? GhiChu,
    bool DaChuyenXu, DateOnly? NgayChuyen, string? NoiChuyen, bool KhongThongKe,
    uint RowVersion, ThanhVienDto[] ThanhVien, HonPhoiDto? HonPhoi);

/// <summary>
/// Các trường sửa được của khối hôn phối cộng RowVersion riêng của bản ghi HonPhoi. Gửi lên
/// null nghĩa là "không đụng gì tới hôn phối hiện có" (không phải "xoá hôn phối") — xem
/// GiaDinhService.CapNhat.
/// </summary>
public record CapNhatHonPhoiRequest(
    string? SoHonPhoi, DateOnly? NgayHonPhoi, string? NoiHonPhoi,
    string? LinhMucChung, string? NguoiChung1, string? NguoiChung2,
    string? CachThucHonPhoi, string? GhiChu, uint RowVersion);

public record CapNhatGiaDinhRequest(
    string? TenGiaDinh, Guid? GiaoHoId, string? DienThoai, string? DiaChi, string? SoHoKhau,
    string? DienGiaDinh, string? GhiChu, bool DaChuyenXu, DateOnly? NgayChuyen,
    string? NoiChuyen, bool KhongThongKe, uint RowVersion, CapNhatHonPhoiRequest? HonPhoi);
