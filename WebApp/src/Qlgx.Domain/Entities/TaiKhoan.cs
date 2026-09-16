namespace Qlgx.Domain.Entities;

/// <summary>
/// Tài khoản đăng nhập. Cột "MatKhau" của Access KHÔNG được chuyển sang đây — quyết định bảo
/// mật đã chốt là người dùng đặt lại mật khẩu ở lần đăng nhập đầu tiên (xem Task 14), không
/// mang mật khẩu cũ (rất có thể lưu dạng rõ hoặc băm yếu) sang hệ mới. MatKhauBam để trống khi
/// chuyển đổi, Task 14 sẽ là nơi đầu tiên ghi vào cột này.
///
/// CauHoiGoiY/CauTraLoiGoiY (câu hỏi/câu trả lời gợi nhớ mật khẩu) vẫn chuyển cấu trúc để
/// không mất dữ liệu, nhưng KHÔNG được dùng làm cơ chế khôi phục mật khẩu thật sự — cơ chế
/// "câu hỏi bí mật" bị xem là yếu về bảo mật.
/// </summary>
public class TaiKhoan : ThucTheCoSo
{
    public string? HoTenNguoiDung { get; set; }
    public string TenTaiKhoan { get; set; } = "";
    public string? MatKhauBam { get; set; }
    public string? Email { get; set; }
    public string? SoDienThoai { get; set; }
    public int LoaiTaiKhoan { get; set; }
    public string? CauHoiGoiY { get; set; }
    public string? CauTraLoiGoiY { get; set; }
    public bool DaXoa { get; set; }

    /// <summary>Số lần đăng nhập sai LIÊN TIẾP gần nhất — reset về 0 ngay khi đăng nhập đúng.
    /// Lưu ở CSDL (không phải bộ đếm trong bộ nhớ tiến trình) vì nhiều bản API chạy song song
    /// sau bộ cân bằng tải (review-backend.md mục T1) — bộ đếm trong bộ nhớ của một tiến trình
    /// sẽ vô dụng vì kẻ tấn công chỉ cần bị định tuyến sang bản khác.</summary>
    public int SoLanDangNhapSaiLienTiep { get; set; }

    /// <summary>Khoá tạm đăng nhập cho tài khoản này đến thời điểm này (null = không khoá) —
    /// đặt khi SoLanDangNhapSaiLienTiep vượt ngưỡng (xem AuthService.SoLanSaiToiDa).</summary>
    public DateTimeOffset? KhoaDangNhapDenLuc { get; set; }
}
