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
    string? HoTenCha, string? HoTenMe, Guid? ChaId, Guid? MeId,
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
    uint RowVersion,
    /// <summary>Khối "Thông tin chuyển xứ" (uiGroupBox6, ẩn khi Ngoài xứ) — null khi giáo dân
    /// chưa có bản ghi ChuyenXu nào (đúng "Ở tại xứ" mặc định, xem <see cref="ChuyenXuDto"/>).
    /// </summary>
    ChuyenXuDto? ChuyenXu = null);

/// <summary>
/// Bản ghi ChuyenXu hiện có của một giáo dân — bản desktop chỉ giữ NHIỀU NHẤT một dòng/giáo dân
/// hiệu lực tại một thời điểm (GetChuyenXuInfo cập nhật tại chỗ thay vì luôn insert mới, xem
/// frmGiaoDan.cs:892-945), nên bản web cũng chỉ đọc/ghi một dòng "hiện tại", không phải danh
/// sách lịch sử đầy đủ (khác Hôn phối/Tận hiến, nơi desktop vốn đã hỗ trợ nhiều dòng).
/// `LoaiChuyen`: 0 = Ở tại xứ (TaiXu, xem Qlgx.Domain.LoaiChuyenXu), 1 = Chuyển đến, 2 = Chuyển
/// đi — CÙNG chỉ số dùng làm value của `<select>` phía web, không cần ánh xạ lại.
/// </summary>
public record ChuyenXuDto(
    Guid Id, int LoaiChuyen, DateOnly? NgayChuyen, string? NoiChuyen, string? GhiChuChuyen,
    uint RowVersion);

/// <summary>
/// Các trường sửa được của khối "Thông tin chuyển xứ" — gửi `null` (không gửi trường `ChuyenXu`
/// trong <see cref="CapNhatGiaoDanRequest"/>) nghĩa là "không đụng gì" (cùng quy ước với
/// <see cref="CapNhatHonPhoiRequest"/> ở GiaDinhDtos.cs); một khi ĐÃ gửi khối này thì
/// `LoaiChuyen=0` (Ở tại xứ) xoá hẳn bản ghi ChuyenXu hiện có — đúng hành vi
/// `cbChuyenXu.SelectedValue==0` của desktop (frmGiaoDan.cs:719-727: xoá dòng ChuyenXu khi
/// người dùng chọn lại "Ở tại xứ"). `RowVersion` chỉ có ý nghĩa khi ĐANG SỬA một bản ghi đã có
/// (client đọc lại từ <see cref="ChuyenXuDto"/>); bỏ qua khi tạo mới.
/// </summary>
public record CapNhatChuyenXuRequest(
    int LoaiChuyen, DateOnly? NgayChuyen, string? NoiChuyen, string? GhiChuChuyen, uint? RowVersion);

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
    string? HoTenCha, string? HoTenMe, Guid? ChaId, Guid? MeId,
    string? SoRuaToi, DateOnly? NgayRuaToi, string? NoiRuaToi, string? ChaRuaToi, string? NguoiDoDauRuaToi,
    string? SoRuocLe, DateOnly? NgayRuocLe, string? NoiRuocLe, string? ChaRuocLe,
    string? SoThemSuc, DateOnly? NgayThemSuc, string? NoiThemSuc, string? ChaThemSuc, string? NguoiDoDauThemSuc,
    DateOnly? NgayXucDau, string? NguoiXucDau, string? TinhTrangXucDau, string? GhiChuXucDau,
    string? TrinhDoVanHoa, string? TrinhDoChuyenMon, string? BietNgoaiNgu, string? NgheNghiep,
    bool ConHoc, bool DaCoGiaDinh, bool TanTong, bool KhongThongKe,
    bool QuaDoi, DateOnly? NgayQuaDoi, string? NoiQuaDoi, string? SoAnTang, string? NoiAnTang,
    string? GhiChu, uint RowVersion,
    /// <summary>Xem TaoGiaoDanRequest.BoQuaCanhBao — cùng ý nghĩa, áp cho PUT.</summary>
    bool BoQuaCanhBao = false,
    /// <summary>Tab "Giáo lý" (Bao đồng 1/2, Vào đời, Hôn nhân) — trước đây UI hoàn toàn tĩnh,
    /// không có trong request nào nên gõ vào rồi mất (xem review-frontend, can-review-sau.md
    /// mục 19). Các trường này đã có sẵn trên `GiaoDan` (đọc được từ Task trước, xem
    /// <see cref="GiaoDanDetailDto"/>) — chỉ còn thiếu đường ghi.</summary>
    DateOnly? NgayBD1 = null, string? NoiBD1 = null,
    DateOnly? NgayBD2 = null, string? NoiBD2 = null,
    DateOnly? NgayTHVaoDoi = null, string? NoiTHVaoDoi = null,
    DateOnly? NgayGLHN1 = null, DateOnly? NgayGLHN2 = null,
    string? NoiGLHN = null, string? NguoiChungNhanGLHN = null, string? XepLoaiGLHN = null,
    /// <summary>Khối "Thông tin chuyển xứ" — xem <see cref="CapNhatChuyenXuRequest"/>. Trước
    /// đây UI hoàn toàn tĩnh (review-frontend), nay nối vào request.</summary>
    CapNhatChuyenXuRequest? ChuyenXu = null);

/// <summary>Tạo mới một giáo dân qua web (Task "ghi cho giáo dân"). Không có RowVersion (bản
/// ghi chưa tồn tại) và không có MaGiaoDan (sinh tự động bằng SinhMaService, xem
/// GiaoDanService.Tao — KHÔNG dùng MAX+1 tự viết, đã có tiền lệ gây trùng khoá).</summary>
public record TaoGiaoDanRequest(
    string HoTen, string? TenThanh, string? Phai, DateOnly? NgaySinh, string? NoiSinh,
    string? CMND, string? DanToc, Guid? GiaoHoId, string? DiaChi, string? DienThoai, string? Email,
    string? HoTenCha, string? HoTenMe, Guid? ChaId, Guid? MeId,
    string? SoRuaToi, DateOnly? NgayRuaToi, string? NoiRuaToi, string? ChaRuaToi, string? NguoiDoDauRuaToi,
    string? SoRuocLe, DateOnly? NgayRuocLe, string? NoiRuocLe, string? ChaRuocLe,
    string? SoThemSuc, DateOnly? NgayThemSuc, string? NoiThemSuc, string? ChaThemSuc, string? NguoiDoDauThemSuc,
    DateOnly? NgayXucDau, string? NguoiXucDau, string? TinhTrangXucDau, string? GhiChuXucDau,
    string? TrinhDoVanHoa, string? TrinhDoChuyenMon, string? BietNgoaiNgu, string? NgheNghiep,
    bool ConHoc, bool DaCoGiaDinh, bool TanTong, bool KhongThongKe,
    bool QuaDoi, DateOnly? NgayQuaDoi, string? NoiQuaDoi, string? SoAnTang, string? NoiAnTang,
    string? GhiChu,
    /// <summary>Bản desktop hỏi từng cảnh báo một bằng hộp thoại Yes/No liên tiếp
    /// (checkInput() của frmGiaoDan.cs); bản web gộp MỌI cảnh báo áp dụng được vào một lượt
    /// (xem GiaoDanService.KiemTraNghiepVu) rồi để người dùng xác nhận một lần — cùng tinh
    /// thần "chặn cho tới khi được xác nhận rõ ràng" của desktop, chỉ khác ở chỗ gộp nhiều hộp
    /// thoại tuần tự thành một lần xác nhận (không đổi kết quả nghiệp vụ: hard error vẫn luôn
    /// chặn, warning vẫn luôn cần xác nhận trước khi lưu). Ghi ở đây thay vì
    /// can-review-sau.md vì đây không phải hành vi sai của desktop được tái hiện nguyên vẹn,
    /// mà là cách thích nghi hộp thoại đồng bộ của WinForms sang một lượt gọi HTTP.</summary>
    bool BoQuaCanhBao = false);

/// <summary>Kết quả tạo/sửa một giáo dân — endpoint tự ánh xạ sang mã HTTP (xem
/// GiaoDanEndpoints). `Id` chỉ có giá trị khi đã LƯU THẬT; nếu còn cảnh báo chưa xác nhận thì
/// `Id` là null và `CanhBao` liệt kê các cảnh báo (client hiện cho người dùng xác nhận rồi gọi
/// lại với `BoQuaCanhBao = true`).</summary>
public record KetQuaLuuGiaoDanDto(Guid? Id, IReadOnlyList<string> CanhBao);

/// <summary>Một kết quả tìm kiếm giáo dân — dùng cho `GxPicker` thật (gõ để tìm, chọn từ danh
/// sách), KHÔNG phải bộ cột đầy đủ của lưới danh sách (29 cột) vì mục đích chỉ là chọn đúng một
/// người. Xem GiaoDanService.TimKiem.</summary>
public record GiaoDanTimKiemDto(Guid Id, int MaGiaoDanCu, string? TenThanh, string HoTen, string? Phai, DateOnly? NgaySinh);
