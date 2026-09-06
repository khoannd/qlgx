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
/// <summary>
/// Một bản ghi Ơn gọi tận hiến của một giáo dân — dùng cho tab "Ơn gọi tận hiến" ở màn hình chi
/// tiết giáo dân. Bản desktop (`GxTanHien`) chỉ hỗ trợ MỘT dòng/giáo dân (lấy `Rows[0]` không
/// `ORDER BY` — xem docs/superpowers/specs/man-hinh/tan-hien.md mục 3); bản web mở rộng có chủ ý
/// thành danh sách đầy đủ, cùng cách tiếp cận đã áp dụng cho Hôn phối.
/// </summary>
public record TanHienCuaGiaoDanDto(
    Guid Id, DateOnly? NgayBatDau, string? ChucVu, string? NoiTu, string? DongTu,
    string? NoiPhucVu, string? DiaChiPhucVu, string? DienThoaiPhucVu, string? EmailPhucVu,
    string? GhiChu, bool DaHoiTuc,
    DateOnly? NgayVaoDCV, DateOnly? NgayVaoNhaThu, DateOnly? NgayVaoNhaTap,
    DateOnly? NgayVaoKhanLanDau, DateOnly? NgayVaoKhanTronDoi,
    DateOnly? NgayPhoTe, DateOnly? NgayThuPhongLM, DateOnly? NgayBonMang,
    uint RowVersion);

/// <summary>Các trường sửa được của một bản ghi Ơn gọi tận hiến, dùng cho cả tạo mới
/// (POST /api/giao-dan/{id}/tan-hien) lẫn sửa (PUT /api/giao-dan/tan-hien/{tanHienId}).
/// `RowVersion` chỉ có ý nghĩa khi sửa (bỏ qua khi tạo mới).</summary>
public record LuuTanHienRequest(
    DateOnly? NgayBatDau, string? ChucVu, string? NoiTu, string? DongTu,
    string? NoiPhucVu, string? DiaChiPhucVu, string? DienThoaiPhucVu, string? EmailPhucVu,
    string? GhiChu, bool DaHoiTuc,
    DateOnly? NgayVaoDCV, DateOnly? NgayVaoNhaThu, DateOnly? NgayVaoNhaTap,
    DateOnly? NgayVaoKhanLanDau, DateOnly? NgayVaoKhanTronDoi,
    DateOnly? NgayPhoTe, DateOnly? NgayThuPhongLM, DateOnly? NgayBonMang,
    uint RowVersion);

/// <summary>Một hội đoàn trong danh mục — dùng để hiện danh sách chọn khi thêm một lượt tham gia
/// mới ở tab "Hội đoàn" (đúng danh sách `cbTenHoiDoan` của `GxHistoryHoiDoan`, xem
/// docs/superpowers/specs/man-hinh/hoi-doan.md mục 2).</summary>
public record HoiDoanDanhMucDto(Guid Id, string TenHoiDoan);

/// <summary>
/// Một lượt tham gia hội đoàn của một giáo dân — dùng cho tab "Hội đoàn" ở màn hình chi tiết
/// giáo dân. Bản desktop (`GxHistoryHoiDoan`) chỉ cho XEM lịch sử và THÊM MỚI (toàn bộ 4 cột lưới
/// đều NoEdit, nút Sửa/Xoá luôn ẩn — xem hoi-doan.md mục 5); bản web mở rộng có chủ ý cho sửa
/// Ngày vào/Ngày ra/Vai trò của một lượt đã có.
/// </summary>
public record HoiDoanCuaGiaoDanDto(
    Guid Id, Guid HoiDoanId, string? TenHoiDoan,
    DateOnly? NgayVaoHoiDoan, DateOnly? NgayRaHoiDoan, string? VaiTro,
    uint RowVersion);

/// <summary>Thêm một lượt tham gia hội đoàn mới. `VaiTro` không có trong request — bản desktop
/// hard-code "Hội viên" khi thêm qua `GxHistoryHoiDoan` (xem hoi-doan.md mục 2), bản web giữ
/// nguyên mặc định này ở tầng dịch vụ; muốn đổi vai trò thì sửa lại bằng
/// `CapNhatHoiDoanRequest` sau khi đã thêm.</summary>
public record ThemHoiDoanRequest(Guid HoiDoanId, DateOnly? NgayVaoHoiDoan, DateOnly? NgayRaHoiDoan);

/// <summary>Sửa một lượt tham gia hội đoàn đã có — không đổi được hội đoàn (`HoiDoanId`), chỉ
/// sửa ngày vào/ra và vai trò.</summary>
public record CapNhatHoiDoanRequest(
    DateOnly? NgayVaoHoiDoan, DateOnly? NgayRaHoiDoan, string? VaiTro, uint RowVersion);

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
