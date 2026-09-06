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
