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

    /// <summary>
    /// Kiểm THUỘC TÍNH THẬT của hai vai trò trên chính máy chủ CSDL, thay vì chỉ so tên như
    /// <see cref="LoiCauHinhSanXuat"/>.
    ///
    /// Vì sao cần thêm: so tên chỉ bắt được trường hợp hai chuỗi trỏ cùng một vai trò. Nó KHÔNG
    /// bắt được trường hợp dễ xảy ra hơn nhiều — <c>ConnectionStrings__Qlgx</c> bị trỏ nhầm về
    /// <c>Username=postgres</c> (siêu người dùng, tự động bỏ qua MỌI chính sách RLS) trong khi
    /// chuỗi quản trị vẫn trỏ <c>qlgx_admin</c>. Hai tên khác nhau → kiểm tra tĩnh ĐẠT, API khởi
    /// động bình thường, và Row-Level Security vô hiệu cho mọi truy vấn nghiệp vụ mà không có
    /// một dấu hiệu nào. Bảng tự kiểm chứng của install.sh có kiểm rolbypassrls nhưng chỉ chạy
    /// lúc cài — không chạy khi ai đó sửa .env rồi "docker compose up -d".
    ///
    /// Đây là kiểm tra rẻ (một truy vấn mỗi vai trò, một lần lúc khởi động) và là kiểm tra duy
    /// nhất nói được SỰ THẬT thay vì nói về cấu hình.
    /// </summary>
    public static async Task<string?> LoiThuocTinhVaiTro(
        IConfiguration cauHinh, bool laSanXuat, CancellationToken ct = default)
    {
        if (!laSanXuat) return null;

        var (nv, loiNghiepVu) = await DocThuocTinhAnToan(cauHinh.GetConnectionString("Qlgx"), ct);
        if (loiNghiepVu is { } ln)
            return "Không kiểm được vai trò CSDL nghiệp vụ (ConnectionStrings__Qlgx): " + ln;

        var (qt, loiQuanTri) = await DocThuocTinhAnToan(ChuoiKetNoiQuanTri.Doc(cauHinh), ct);
        if (loiQuanTri is { } lq)
            return "Không kiểm được vai trò CSDL quản trị (ConnectionStrings__QlgxQuanTri): " + lq;

        if (nv.sieuNguoiDung || nv.boQuaRls)
            return $"Vai trò CSDL nghiệp vụ '{nv.ten}' đang là siêu người dùng hoặc có BYPASSRLS. " +
                   "Như vậy Row-Level Security bị vô hiệu cho MỌI truy vấn nghiệp vụ — mất hẳn lớp " +
                   "phòng thủ ngăn giáo xứ này đọc dữ liệu giáo xứ khác. Tạo lại vai trò nghiệp vụ " +
                   "KHÔNG có SUPERUSER và KHÔNG có BYPASSRLS — xem WebApp/docs/CAI-DAT-MAY-CHU.md.";

        if (!qt.sieuNguoiDung && !qt.boQuaRls)
            return $"Vai trò CSDL quản trị '{qt.ten}' không có BYPASSRLS. Đăng nhập và các màn hình " +
                   "quản trị cần đọc chéo giáo xứ sẽ không hoạt động — xem WebApp/docs/CAI-DAT-MAY-CHU.md.";

        return null;
    }

    private static async Task<(VaiTroCsdl vaiTro, string? loi)> DocThuocTinhAnToan(
        string? chuoiKetNoi, CancellationToken ct)
    {
        var rong = new VaiTroCsdl("", false, false);
        if (string.IsNullOrWhiteSpace(chuoiKetNoi)) return (rong, "thiếu chuỗi kết nối.");
        try
        {
            return (await DocThuocTinh(chuoiKetNoi, ct), null);
        }
        catch (Exception ex)
        {
            // Không nuốt lỗi: trả nguyên văn thông báo của Npgsql vào lỗi khởi động. Một máy chủ
            // không kết nối được CSDL bằng vai trò nào đó thì thà DỪNG ngay với câu nói rõ lý do,
            // còn hơn chạy tiếp rồi hỏng lúc người dùng đăng nhập.
            return (rong, ex.Message);
        }
    }

    /// <summary>Thuộc tính thật của một vai trò CSDL, đọc từ pg_roles.</summary>
    public sealed record VaiTroCsdl(string ten, bool sieuNguoiDung, bool boQuaRls);

    private static async Task<VaiTroCsdl> DocThuocTinh(string chuoiKetNoi, CancellationToken ct)
    {
        await using var kn = new NpgsqlConnection(chuoiKetNoi);
        await kn.OpenAsync(ct);
        await using var lenh = new NpgsqlCommand(
            "SELECT rolname, rolsuper, rolbypassrls FROM pg_roles WHERE rolname = current_user", kn);
        await using var doc = await lenh.ExecuteReaderAsync(ct);
        if (!await doc.ReadAsync(ct))
            throw new InvalidOperationException("Không đọc được thông tin vai trò hiện tại (pg_roles).");
        return new VaiTroCsdl(doc.GetString(0), doc.GetBoolean(1), doc.GetBoolean(2));
    }

    /// <summary>So sánh theo tên vai trò đã phân tích, không so chuỗi thô: hai chuỗi kết nối
    /// khác thứ tự tham số hoặc khoảng trắng vẫn có thể là cùng một vai trò.</summary>
    private static string VaiTroCua(string chuoiKetNoi)
    {
        try
        {
            return new NpgsqlConnectionStringBuilder(chuoiKetNoi).Username ?? "";
        }
        catch (Exception ex) when (ex is ArgumentException or FormatException or KeyNotFoundException)
        {
            // Chuỗi không phân tích được thì trả về chính nó — để hai chuỗi hỏng giống hệt nhau
            // vẫn bị coi là trùng vai trò, thay vì lọt lưới. Bắt rộng vì NpgsqlConnectionStringBuilder
            // ném nhiều loại ngoại lệ khác nhau tuỳ kiểu dị dạng của chuỗi (ví dụ "===" ném
            // KeyNotFoundException chứ không phải ArgumentException) — mục tiêu ở đây là KHÔNG bao
            // giờ để việc kiểm tra cấu hình tự nó crash, nên bắt theo nhóm ngoại lệ do phân tích cú
            // pháp gây ra thay vì chỉ một loại cụ thể.
            return chuoiKetNoi;
        }
    }
}
