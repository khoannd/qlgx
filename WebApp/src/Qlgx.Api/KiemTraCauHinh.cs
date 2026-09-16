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

        return LoiKhoaKyJwt(cauHinh["Qlgx:JwtKey"]);
    }

    /// <summary>Số byte tối thiểu của khoá ký JWT. 32 byte = 256 bit, đúng bằng độ dài đầu ra
    /// của HMAC-SHA256 (thuật toán TokenService dùng) — ngắn hơn là tự làm yếu chữ ký, và
    /// install.sh vốn đã sinh đúng 32 byte.</summary>
    private const int SoByteKhoaKyToiThieu = 32;

    /// <summary>
    /// TB-8 (review-bao-mat.md) — cổng gác sản xuất trước đây kiểm rất kỹ hai chuỗi kết nối
    /// nhưng KHÔNG kiểm <c>Qlgx:JwtKey</c>. Thiếu khoá (hoặc khoá không phải base64 hợp lệ) thì
    /// máy chủ vẫn khởi động bình thường, readiness trả 200, install.sh kết luận "đã lên được,
    /// không quay lui" — nhưng MỌI lần đăng nhập đều 500 vì TokenService ném lỗi lúc phát hành
    /// token. Đúng kiểu hỏng mà lớp kiểm tra này sinh ra để chặn: tín hiệu nói khoẻ, thực tế
    /// không ai dùng được.
    /// </summary>
    private static string? LoiKhoaKyJwt(string? khoa)
    {
        const string huongDan =
            " Sinh một khoá mới (PowerShell): " +
            "[Convert]::ToBase64String((1..32 | %{ Get-Random -Max 256 })) — rồi đặt vào biến " +
            "QLGX_JWT_KEY trong tệp .env cạnh docker-compose.yml.";

        if (string.IsNullOrWhiteSpace(khoa))
            return "Thiếu Qlgx__JwtKey (khoá ký token đăng nhập). Không có khoá này thì máy chủ " +
                   "vẫn khởi động nhưng KHÔNG ai đăng nhập được." + huongDan;

        byte[] byteKhoa;
        try
        {
            byteKhoa = Convert.FromBase64String(khoa);
        }
        catch (FormatException)
        {
            return "Qlgx__JwtKey không phải chuỗi base64 hợp lệ, nên không ai đăng nhập được." + huongDan;
        }

        if (byteKhoa.Length < SoByteKhoaKyToiThieu)
            return $"Qlgx__JwtKey chỉ dài {byteKhoa.Length} byte, tối thiểu phải " +
                   $"{SoByteKhoaKyToiThieu} byte. Khoá ngắn làm chữ ký token yếu đi — kẻ tấn công " +
                   "dò ra khoá là tự phát hành được token của bất kỳ ai, ở bất kỳ giáo xứ nào." + huongDan;

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

        return await LoiChuBangChuaForceRls(cauHinh.GetConnectionString("Qlgx"), ct);
    }

    /// <summary>
    /// NT-2 (review-bao-mat.md) — lỗ hổng tệ nhất mà hai lớp kiểm tra phía trên KHÔNG bắt được.
    ///
    /// Migration EF Core chạy bằng chính <c>ConnectionStrings__Qlgx</c> (xem
    /// docker-compose.yml: <c>Qlgx__ChayMigrationKhiKhoiDong=true</c>), nên vai trò NGHIỆP VỤ
    /// <c>qlgx_app</c> là CHỦ của toàn bộ bảng. PostgreSQL KHÔNG áp policy RLS cho chủ bảng trừ
    /// khi bảng bật <c>FORCE ROW LEVEL SECURITY</c> — nghĩa là trước migration
    /// BatForceRlsChoBangTheoGiaoXu, mọi policy <c>loc_theo_giao_xu</c> đều bị bỏ qua cho đúng
    /// cái vai trò phục vụ toàn bộ nghiệp vụ. Lớp phòng thủ thứ hai không tồn tại, trong khi
    /// tài liệu lẫn mã nguồn đều tin rằng nó đang chạy.
    ///
    /// Kiểm ở đây (không chỉ dựa vào migration) vì migration chỉ chạy một lần: một bảng thêm
    /// tay, một lần phục hồi từ bản sao lưu cũ, một migration tương lai quên FORCE — đều đưa hệ
    /// thống về đúng trạng thái hỏng cũ mà không có dấu hiệu nào. Truy vấn chạy bằng CHÍNH vai
    /// trò nghiệp vụ nên nó nói sự thật về vai trò đó, không nói về cấu hình.
    /// </summary>
    private static async Task<string?> LoiChuBangChuaForceRls(string? chuoiNghiepVu, CancellationToken ct)
    {
        List<string> bangHong;
        try
        {
            await using var kn = new NpgsqlConnection(chuoiNghiepVu);
            await kn.OpenAsync(ct);
            await using var lenh = new NpgsqlCommand(
                """
                SELECT c.relname
                FROM pg_class c
                JOIN pg_namespace n ON n.oid = c.relnamespace
                WHERE c.relkind = 'r'
                  AND c.relrowsecurity
                  AND NOT c.relforcerowsecurity
                  AND n.nspname NOT IN ('pg_catalog', 'information_schema')
                  AND pg_get_userbyid(c.relowner) = current_user
                ORDER BY c.relname
                """, kn);
            await using var doc = await lenh.ExecuteReaderAsync(ct);
            bangHong = [];
            while (await doc.ReadAsync(ct)) bangHong.Add(doc.GetString(0));
        }
        catch (Exception ex)
        {
            return "Không kiểm được quyền sở hữu bảng bằng vai trò CSDL nghiệp vụ: " + ex.Message;
        }

        if (bangHong.Count == 0) return null;

        var vaiDongDau = string.Join(", ", bangHong.Take(5));
        return $"Vai trò CSDL nghiệp vụ đang là CHỦ của {bangHong.Count} bảng có Row-Level " +
               $"Security nhưng chưa bật FORCE ROW LEVEL SECURITY (ví dụ: {vaiDongDau}). " +
               "PostgreSQL không áp chính sách RLS cho chủ bảng, nên lớp phòng thủ ngăn giáo xứ " +
               "này đọc dữ liệu giáo xứ khác đang KHÔNG hoạt động dù mọi policy đều có mặt. " +
               "Chạy migration BatForceRlsChoBangTheoGiaoXu (khởi động lại API với " +
               "Qlgx__ChayMigrationKhiKhoiDong=true là đủ), hoặc chuyển chủ sở hữu các bảng này " +
               "sang vai trò quản trị.";
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
