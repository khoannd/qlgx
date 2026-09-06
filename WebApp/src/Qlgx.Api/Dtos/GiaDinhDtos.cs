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

/// <summary>
/// Một hôn phối của một GIÁO DÂN cụ thể — dùng cho tab "Hôn phối" ở màn hình chi tiết giáo dân
/// (Task 15). Khác <see cref="HonPhoiDto"/> (chỉ trả hôn phối "hiện tại" của một GIA ĐÌNH): một
/// người có thể có NHIỀU bản ghi hôn phối theo thời gian (goá rồi tái hôn — xem chú thích
/// ChonHonPhoiHienTai trong GiaDinhService), nên GET /api/giao-dan/{id}/hon-phoi trả về DANH
/// SÁCH, không đoán "bản ghi hiện tại" như hai control desktop (frmHonPhoi, GxHonPhoiGiaDinh) —
/// xem docs/superpowers/specs/man-hinh/hon-phoi.md mục 8.
/// </summary>
public record HonPhoiCuaGiaoDanDto(
    Guid Id, string? TenHonPhoi, string? SoHonPhoi, DateOnly? NgayHonPhoi, string? NoiHonPhoi,
    string? LinhMucChung, string? NguoiChung1, string? NguoiChung2,
    string? CachThucHonPhoi, string? GhiChu,
    /// <summary>Mã giáo dân và tên hiển thị của NGƯỜI KIA trong hôn phối này (chồng nếu đang
    /// xem từ vợ, ngược lại) — null nếu vì lý do nào đó bản ghi GiaoDanHonPhoi chỉ có một người
    /// (không nên xảy ra với dữ liệu hợp lệ, nhưng không giả định).</summary>
    Guid? VoChongId, string? TenVoChong,
    uint RowVersion);

/// <summary>
/// <c>ChuHoVaiTro</c>: chủ hộ CHỈ có thể là Người nam (0/Chồng) hay Người nữ (1/Vợ) — đúng hai
/// radio `rdChuHoNam`/`rdChuHoNu` của bản desktop (frmGiaDinh.cs:1731,1739 chỉ ghi `ChuHo` vào
/// dòng Chồng/Vợ, không có UI đặt chủ hộ cho vai trò nào khác). <c>null</c> = không ai được
/// đánh dấu chủ hộ (cả hai radio đều bỏ trống) — hợp lệ, khớp trạng thái ban đầu của một gia
/// đình mới chưa từng lưu chủ hộ. Bản desktop còn có một loạt hộp thoại Yes/No/Cancel GỢI Ý tự
/// đổi chủ hộ khi người đang được chọn đã "Qua đời" (dòng 1377-1433) — đây là tiện ích UX, KHÔNG
/// phải ràng buộc cứng (người dùng luôn có thể chọn [No] để giữ nguyên), nên KHÔNG migrate ở
/// Phase 1 (xem can-review-sau.md mục 19 phần chủ hộ) — máy chủ ghi đúng y những gì client gửi,
/// không tự đoán/chặn gì thêm.
/// </summary>
public record CapNhatGiaDinhRequest(
    string? TenGiaDinh, Guid? GiaoHoId, string? DienThoai, string? DiaChi, string? SoHoKhau,
    string? DienGiaDinh, string? GhiChu, bool DaChuyenXu, DateOnly? NgayChuyen,
    string? NoiChuyen, bool KhongThongKe, uint RowVersion, CapNhatHonPhoiRequest? HonPhoi,
    int? ChuHoVaiTro = null);

// --- Ghi (tạo mới / xoá / thành viên / vợ-chồng) — xem GiaDinhService và can-review-sau.md
// mục 2, 3, 4, 5 (quyết định chi phối: migrate y hệt bản desktop, kể cả chỗ sai). ------------

/// <summary>Tạo một gia đình mới, tương đương mở `frmGiaDinh` ở chế độ Thêm mới rồi bấm Cập
/// nhật ngay (chỉ hai trường bắt buộc tối thiểu để có một bản ghi hợp lệ — Người nam/nữ và
/// thành viên được thêm bằng các endpoint riêng SAU KHI đã có Id gia đình, vì lưới/hai ô chọn
/// người của desktop chỉ hoạt động trên một `MaGiaDinh` đã tồn tại).</summary>
public record TaoGiaDinhRequest(string? TenGiaDinh, Guid? GiaoHoId);

public record KetQuaTaoGiaDinhDto(Guid Id, int MaGiaDinhCu);

/// <summary>Thêm một người vào lưới "Thành viên khác trong gia đình" (`addGiaoDan`,
/// frmGiaDinh.cs:1043-1140). `VaiTro` giữ nguyên giá trị thô (0..100, xem GiaDinhService) —
/// client chịu trách nhiệm không gửi 0/1 (Chồng/Vợ có endpoint riêng, xem
/// GanVoChongRequest). `BoQuaCanhBao=true` xác nhận MỌI cảnh báo áp dụng được cùng lúc, đúng
/// mẫu KiemTraNghiepVu của giáo dân (can-review-sau.md mục 20) — không phân biệt "đồng ý cảnh
/// báo A nhưng không đồng ý cảnh báo B". `MuonChuyenVeXu`: chỉ có ý nghĩa khi người được chọn
/// đã chuyển xứ đi — null = chưa quyết (server trả cảnh báo yêu cầu quyết định, tương đương
/// hộp thoại Yes/No/Cancel 3 lựa chọn của desktop, dòng 1127-1131), true = "Yes" (chuyển về lại
/// xứ, tự đặt lại GiaoXuId/xoá cờ chuyển xứ liên quan), false = "No" (vẫn thêm nhưng KHÔNG đổi
/// gì về tình trạng chuyển xứ của người đó) — chọn "Cancel" ở client thì đơn giản là KHÔNG gọi
/// endpoint này.</summary>
public record ThemThanhVienRequest(Guid GiaoDanId, int VaiTro, bool BoQuaCanhBao, bool? MuonChuyenVeXu);

public record KetQuaThemThanhVienDto(Guid? GiaoDanId, IReadOnlyList<string> CanhBao);

/// <summary>Gán hoặc đổi Người nam (VaiTro=0/Chồng) hay Người nữ (VaiTro=1/Vợ) của một gia
/// đình — tương đương `txtNguoiChong_OnSelecting`/`txtNguoiVo_OnSelecting`
/// (frmGiaDinh.cs:390-611) cộng phần chọn ban đầu của cây quyết định `NguoiCu` (dòng 649-919).
///
/// Cây quyết định NguoiCu (nhiều hộp thoại Yes/No liên tiếp để ĐOÁN vai trò mới của người cũ)
/// là logic GIAO DIỆN chạy tuần tự — thuộc phạm vi lượt sau (xây form gia đình). Máy chủ ở đây
/// chỉ nhận Ý ĐỊNH CUỐI CÙNG mà người dùng đã chốt qua <see cref="XuLyNguoiCu"/> và thực hiện
/// NGUYÊN TỬ trong cùng một giao dịch với việc gán người mới — không tự đoán gì thêm.</summary>
public record GanVoChongRequest(
    Guid GiaoDanId, uint RowVersion, bool BoQuaCanhBao, XuLyNguoiCuDto? XuLyNguoiCu);

/// <summary>Ý định cuối cùng về người đang giữ vai trò Chồng/Vợ trước khi bị thay — bắt buộc
/// phải có (khác null) nếu vai trò đó ĐANG có người, để tránh máy chủ tự "đoán" thay. `Xoa=true`
/// = xoá hẳn khỏi gia đình (không còn dòng ThanhVienGiaDinh nào cho người này ở gia đình này);
/// `Xoa=false` = hạ xuống thành viên với `VaiTroMoi` do client (đã hỏi người dùng qua cây quyết
/// định) chọn — bắt buộc phải có giá trị khi Xoa=false.</summary>
public record XuLyNguoiCuDto(bool Xoa, int? VaiTroMoi);
