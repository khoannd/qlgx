namespace Qlgx.Api.Dtos;

/// <summary>
/// Một dòng trên lưới "Danh sách rao hôn phối" — khớp cột của
/// <c>GxRaoHonPhoiList.FormatGrid</c> (Source/GXControl/GxRaoHonPhoiList.cs:95-164): Mã rao,
/// Đôi rao, Người thứ nhất, Người thứ hai, Rao lần 1/2/3, Ghi chú. Nguoi1/Nguoi2 là tên gõ
/// tay từ picker giáo dân lúc chọn (không phải lấy động từ GiaoDan.HoTen mỗi lần hiển thị) —
/// bản web lấy trực tiếp GiaoDan1/GiaoDan2.HoTen khi có liên kết, để trống khi không có.
/// </summary>
public record RaoHonPhoiListItemDto(
    Guid Id, int MaRaoHonPhoiCu, string? TenRaoHonPhoi, string? Nguoi1, string? Nguoi2,
    DateOnly? NgayRaoLan1, DateOnly? NgayRaoLan2, DateOnly? NgayRaoLan3, string? GhiChu);

/// <summary>Chi tiết đầy đủ 26 cột — khớp <c>frmRaoHonPhoi</c> (Source/GXControl/frmRaoHonPhoi.cs).</summary>
public record RaoHonPhoiDetailDto(
    Guid Id, int MaRaoHonPhoiCu, string? TenRaoHonPhoi,
    Guid? GiaoDan1Id, string? TenGiaoDan1, Guid? GiaoDan2Id, string? TenGiaoDan2,
    DateOnly? NgayRaoLan1, DateOnly? NgayRaoLan2, DateOnly? NgayRaoLan3,
    string? GiaoXu1, string? GiaoPhan1, string? GiaoXuTruoc1, string? GiaoPhanTruoc1,
    string? GiaoXu2, string? GiaoPhan2, string? GiaoXuTruoc2, string? GiaoPhanTruoc2,
    string? LinhMucNhan, string? GiaoXuNhan, string? GhiChu,
    string? Tam1, string? Tam2, string? Tam3,
    string? GiaoXuNQ1, string? GiaoPhanNQ1, string? GiaoXuNQ2, string? GiaoPhanNQ2,
    uint RowVersion);

/// <summary>Tạo/sửa một đôi rao hôn phối — cùng bộ trường với <see cref="RaoHonPhoiDetailDto"/>
/// (trừ Id/MaRaoHonPhoiCu tự sinh, TenGiaoDan1/2 tự tra từ GiaoDan1Id/GiaoDan2Id).
/// RowVersion chỉ dùng khi sửa (null khi tạo mới).</summary>
public record LuuRaoHonPhoiRequest(
    string TenRaoHonPhoi, Guid? GiaoDan1Id, Guid? GiaoDan2Id,
    DateOnly? NgayRaoLan1, DateOnly? NgayRaoLan2, DateOnly? NgayRaoLan3,
    string? GiaoXu1, string? GiaoPhan1, string? GiaoXuTruoc1, string? GiaoPhanTruoc1,
    string? GiaoXu2, string? GiaoPhan2, string? GiaoXuTruoc2, string? GiaoPhanTruoc2,
    string? LinhMucNhan, string? GiaoXuNhan, string? GhiChu,
    string? Tam1, string? Tam2, string? Tam3,
    string? GiaoXuNQ1, string? GiaoPhanNQ1, string? GiaoXuNQ2, string? GiaoPhanNQ2,
    uint? RowVersion);
