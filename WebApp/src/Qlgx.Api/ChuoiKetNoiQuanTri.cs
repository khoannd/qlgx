using Microsoft.Extensions.Configuration;

namespace Qlgx.Api;

/// <summary>
/// Chuỗi kết nối dùng cho BỐN đường dẫn hợp lệ truy vấn CHÉO GIÁO XỨ trong toàn hệ thống — đăng
/// nhập (AuthService, tra tên tài khoản trên toàn máy chủ trước khi biết giáo xứ), tạo tài
/// khoản quản trị đầu tiên (TaoTaiKhoanQuanTri), công cụ chuyển dữ liệu (Qlgx.Migration, dùng
/// chuỗi kết nối riêng truyền qua tham số dòng lệnh, không đọc từ đây) và màn hình "Quản lý
/// giáo phận/giáo hạt/giáo xứ" (QuanLyGiaoXuService, CHỈ gọi được qua policy "QuanTriHeThong" —
/// xem docs/superpowers/specs/man-hinh/quan-ly-giao-xu.md mục 4).
///
/// Kể từ khi bật Row-Level Security (migration BatRlsChoBangTheoGiaoXu — lớp phòng thủ thứ
/// hai), vai trò CSDL dùng cho DbContext nghiệp vụ chính (đăng ký ở Program.cs, khoá
/// "ConnectionStrings:Qlgx") KHÔNG còn được phép có BYPASSRLS — nếu có, RLS sẽ vô hiệu cho MỌI
/// truy vấn nghiệp vụ, mất hẳn lớp phòng thủ thứ hai. Ba đường dẫn ở trên vẫn cần đọc/ghi chéo
/// giáo xứ nên phải dùng một vai trò KHÁC, có BYPASSRLS, cấu hình ở khoá riêng
/// "ConnectionStrings:QlgxQuanTri".
///
/// Mặc định (không đặt khoá riêng) dùng LẠI "ConnectionStrings:Qlgx" — giữ nguyên hoạt động cho
/// môi trường dev/test hiện tại (một vai trò `postgres` superuser duy nhất, tự động bỏ qua RLS
/// bất kể chính sách gì). Triển khai thật với từ hai giáo xứ trở lên BẮT BUỘC đặt hai vai trò
/// khác nhau — xem TRIEN-KHAI.md.
/// </summary>
public static class ChuoiKetNoiQuanTri
{
    public static string? Doc(IConfiguration cauHinh) =>
        cauHinh.GetConnectionString("QlgxQuanTri") ?? cauHinh.GetConnectionString("Qlgx");
}
