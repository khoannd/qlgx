namespace Qlgx.Api.Dtos;

/// <summary>Một dòng trên lưới giáo dân — 29 cột theo đúng GxGiaoDanList.FormatGrid(), cộng
/// hai trường front-end cần để lọc/điều hướng đúng (xem Task 12: mục menu "Xem gia đình" và
/// ô tick "không được thống kê" — cả hai từng bị lấy nhầm từ các trường khác).</summary>
public record GiaoDanListItemDto(
    Guid Id, int MaGiaoDanCu, string? TenThanh, string HoTen, string? Phai,
    DateOnly? NgaySinh, string NamSinh,
    DateOnly? NgayRuaToi, DateOnly? NgayRuocLe, DateOnly? NgayThemSuc,
    bool LapGd, string? HoTenCha, string? HoTenMe, bool TanTong, bool ConHoc,
    string? NgheNghiep, string? GhiChu, string? DienThoai, string? DiaChi,
    string? TenGiaoHo, bool DaChuyenDi, string? TrinhDoVanHoa, string? TrinhDoChuyenMon,
    string? BietNgoaiNgu, bool QuaDoi, DateOnly? NgayQuaDoi, string? NoiAnTang,
    string? NoiSinh, string? NoiRuaToi, string? NoiRuocLe, string? NoiThemSuc,
    /// <summary>Chỉ có giá trị khi lưới nhúng trong form gia đình.</summary>
    string? QuanHe,
    /// <summary>Mã gia đình giáo dân này thuộc về (null nếu chưa gắn với gia đình nào) —
    /// front-end dùng để mở đúng thẻ chi tiết gia đình từ mục menu "Xem gia đình", KHÔNG
    /// được dùng Id của chính giáo dân để tra gia đình.</summary>
    Guid? GiaDinhId,
    /// <summary>Khác khái niệm "Ngoài xứ": một giáo dân ngoài xứ vẫn có thể được thống kê,
    /// nên front-end không được suy trường này từ TenGiaoHo.</summary>
    bool KhongThongKe);

public record GiaoDanDetailDto(
    Guid Id, int MaGiaoDanCu, string HoTen, string? TenThanh, string? Phai,
    DateOnly? NgaySinh, string? NoiSinh, string? CMND, string? DanToc,
    Guid? GiaoHoId, string? ThuocGiaoXu, string? ThuocGiaoPhan,
    string? DiaChi, string? DienThoai, string? Email,
    string? HoTenCha, string? HoTenMe,
    string? SoRuaToi, DateOnly? NgayRuaToi, string? NoiRuaToi, string? ChaRuaToi, string? NguoiDoDauRuaToi,
    string? SoRuocLe, DateOnly? NgayRuocLe, string? NoiRuocLe, string? ChaRuocLe,
    string? SoThemSuc, DateOnly? NgayThemSuc, string? NoiThemSuc, string? ChaThemSuc, string? NguoiDoDauThemSuc,
    DateOnly? NgayXucDau, string? NguoiXucDau, string? TinhTrangXucDau, string? GhiChuXucDau,
    DateOnly? NgayBD1, string? NoiBD1, DateOnly? NgayBD2, string? NoiBD2,
    DateOnly? NgayTHVaoDoi, string? NoiTHVaoDoi,
    DateOnly? NgayGLHN1, DateOnly? NgayGLHN2, string? NoiGLHN, string? NguoiChungNhanGLHN, string? XepLoaiGLHN,
    string? TrinhDoVanHoa, string? TrinhDoChuyenMon, string? BietNgoaiNgu, string? NgheNghiep, bool ConHoc,
    bool DaCoGiaDinh, bool TanTong, bool KhongThongKe,
    bool QuaDoi, DateOnly? NgayQuaDoi, string? NoiQuaDoi, string? SoAnTang, string? NoiAnTang,
    string? GhiChu,
    Guid? GiaDinhId, string? TenGiaDinh, int? VaiTro,
    uint RowVersion);

/// <summary>
/// Chỉ những trường màn hình chi tiết cho sửa. Các trường còn lại của thực thể không nhận
/// từ client để tránh sửa nhầm dữ liệu do công cụ chuyển đổi sinh ra.
/// </summary>
public record CapNhatGiaoDanRequest(
    string HoTen, string? TenThanh, string? Phai, DateOnly? NgaySinh, string? NoiSinh,
    string? CMND, string? DanToc, Guid? GiaoHoId, string? DiaChi, string? DienThoai, string? Email,
    string? HoTenCha, string? HoTenMe,
    string? SoRuaToi, DateOnly? NgayRuaToi, string? NoiRuaToi, string? ChaRuaToi, string? NguoiDoDauRuaToi,
    string? SoRuocLe, DateOnly? NgayRuocLe, string? NoiRuocLe, string? ChaRuocLe,
    string? SoThemSuc, DateOnly? NgayThemSuc, string? NoiThemSuc, string? ChaThemSuc, string? NguoiDoDauThemSuc,
    DateOnly? NgayXucDau, string? NguoiXucDau, string? TinhTrangXucDau, string? GhiChuXucDau,
    string? TrinhDoVanHoa, string? TrinhDoChuyenMon, string? BietNgoaiNgu, string? NgheNghiep,
    bool ConHoc, bool DaCoGiaDinh, bool TanTong, bool KhongThongKe,
    bool QuaDoi, DateOnly? NgayQuaDoi, string? NoiQuaDoi, string? SoAnTang, string? NoiAnTang,
    string? GhiChu, uint RowVersion);
