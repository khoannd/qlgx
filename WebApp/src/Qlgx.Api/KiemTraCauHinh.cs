using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Qlgx.Api;

/// <summary>
/// Kiểm những điều kiện cấu hình mà môi trường SẢN XUẤT bắt buộc phải có, ngay lúc khởi động.
///
/// Vì sao phải chặn cứng: <see cref="ChuoiKetNoiQuanTri"/> mặc định dùng lại
/// "ConnectionStrings:Qlgx" khi không có khoá riêng, và docker-compose.yml cũng đặt
/// QLGX_ADMIN_DB_USER mặc định bằng QLGX_APP_DB_USER. Hai lớp mặc định đó cộng lại nghĩa là:
/// quên cấu hình vai trò quản trị thì hệ thống VẪN CHẠY BÌNH THƯỜNG, chỉ khác là vai trò nghiệp
/// vụ phải có BYPASSRLS mới đăng nhập được — tức Row-Level Security bị vô hiệu cho MỌI truy vấn
/// mà không có dấu hiệu gì. Đó là kiểu hỏng tệ nhất: không làm gì sai hôm nay, chỉ âm thầm tháo
/// bỏ lớp phòng thủ cho tới ngày cần tới. Ở dev/test thì fallback vẫn tiện và vô hại (một vai
/// trò superuser duy nhất), nên chỉ chặn khi laSanXuat = true.
/// </summary>
public static class KiemTraCauHinh
{
    public static string? LoiCauHinhSanXuat(IConfiguration cauHinh, bool laSanXuat)
    {
        if (!laSanXuat) return null;

        var nghiepVu = cauHinh.GetConnectionString("Qlgx");
        var quanTri = cauHinh.GetConnectionString("QlgxQuanTri");

        if (string.IsNullOrWhiteSpace(nghiepVu))
            return "Thiếu ConnectionStrings__Qlgx. Đặt biến này trong tệp .env cạnh docker-compose.yml.";

        if (string.IsNullOrWhiteSpace(quanTri))
            return "Thiếu ConnectionStrings__QlgxQuanTri. Môi trường sản xuất BẮT BUỘC có hai vai " +
                   "trò CSDL riêng: một vai trò nghiệp vụ KHÔNG có BYPASSRLS và một vai trò quản " +
                   "trị CÓ BYPASSRLS. Xem WebApp/docs/CAI-DAT-MAY-CHU.md.";

        if (VaiTroCua(nghiepVu) == VaiTroCua(quanTri))
            return "ConnectionStrings__Qlgx và ConnectionStrings__QlgxQuanTri đang dùng cùng một " +
                   "vai trò CSDL. Như vậy Row-Level Security bị vô hiệu hoàn toàn. Tạo hai vai trò " +
                   "riêng — xem WebApp/docs/CAI-DAT-MAY-CHU.md.";

        return null;
    }

    /// <summary>So sánh theo tên vai trò đã phân tích, không so chuỗi thô: hai chuỗi kết nối
    /// khác thứ tự tham số hoặc khoảng trắng vẫn có thể là cùng một vai trò.</summary>
    private static string VaiTroCua(string chuoiKetNoi)
    {
        try
        {
            return new NpgsqlConnectionStringBuilder(chuoiKetNoi).Username ?? "";
        }
        catch (ArgumentException)
        {
            // Chuỗi không phân tích được thì trả về chính nó — để hai chuỗi hỏng giống hệt nhau
            // vẫn bị coi là trùng vai trò, thay vì lọt lưới.
            return chuoiKetNoi;
        }
    }
}
