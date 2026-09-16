namespace Qlgx.Api.Dtos;

/// <summary>
/// "Chuyển họ hàng loạt — giáo dân" (frmChuyenHoGiaoDan.cs, xem
/// docs/superpowers/specs/man-hinh/cong-cu-du-lieu.md mục 4). Danh sách Id đã chọn trên lưới ở
/// client (tương đương cột "Chọn" desktop thêm động vào DataTable) + giáo họ đích. Bản web
/// KHÔNG nhận lại toàn bộ DataTable như desktop — chỉ Id, máy chủ tự tra lại dữ liệu, tránh
/// client có thể gửi dữ liệu giả mạo cho các cột khác.
/// </summary>
public record ChuyenHoGiaoDanRequest(List<Guid> GiaoDanIds, Guid GiaoHoDichId);

/// <summary>Kết quả chuyển họ giáo dân — con số CHÍNH XÁC để client hiện lại sau khi ghi,
/// đối chiếu với con số đã xác nhận trước khi ghi (nguyên tắc bắt buộc #2 của nhiệm vụ).</summary>
public record ChuyenHoGiaoDanKetQua(int SoLuongDaChuyen);

/// <summary>
/// "Chuyển họ hàng loạt — gia đình" (frmChuyenHoGiaDinh.cs + UpdateProcess.chuyenHoGiaDinh).
/// KHÁC bên giáo dân: chuyển một gia đình kéo theo chuyển TẤT CẢ thành viên
/// (ThanhVienGiaDinh) của gia đình đó — đúng cảnh báo desktop "các thành viên trong các gia
/// đình này cũng sẽ bị chuyển theo" (frmChuyenHoGiaDinh.cs:143).
/// </summary>
public record ChuyenHoGiaDinhRequest(List<Guid> GiaDinhIds, Guid GiaoHoDichId);

public record ChuyenHoGiaDinhKetQua(int SoLuongGiaDinhDaChuyen, int SoLuongThanhVienDaChuyen);

/// <summary>
/// "Xem trước" trước khi ghi — nguyên tắc bắt buộc #1 của nhiệm vụ (KHÔNG có ở bản desktop:
/// desktop chỉ có lưới chọn sẵn rồi ghi thẳng, không có bước xác nhận số liệu tách riêng). Trả
/// đúng những gì SẼ đổi để client hiện hộp thoại xác nhận có con số cụ thể trước khi gọi endpoint
/// ghi thật.
/// </summary>
public record ChuyenHoGiaDinhXemTruoc(int SoLuongGiaDinh, int SoLuongThanhVien, string TenGiaoHoDich);

public record ChuyenHoGiaoDanXemTruoc(int SoLuongGiaoDan, string TenGiaoHoDich);
