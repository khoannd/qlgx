using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Qlgx.Data;

/// <summary>
/// Lớp phòng thủ THỨ HAI cho mô hình nhiều giáo xứ dùng chung một database (xem
/// <c>QlgxDbContext.OnModelCreating</c> cho lớp thứ nhất — bộ lọc toàn cục của EF Core). Mỗi
/// lần một kết nối vật lý/kết nối lấy từ pool được EF "mở" (kể cả khi Npgsql tái dùng một kết
/// nối vật lý đã có), interceptor này đặt tham số phiên PostgreSQL <c>app.giao_xu_id</c> —
/// Row-Level Security (bật ở migration <c>BatRlsChoBangTheoGiaoXu</c>) đọc đúng tham số này để
/// lọc dòng ở TẦNG DATABASE, không phụ thuộc câu LINQ có quên lọc hay không, và không phụ
/// thuộc EF Core có bug hay bị bỏ qua (<c>IgnoreQueryFilters</c>, SQL thô, …).
///
/// Đặt lại (kể cả về rỗng khi <see cref="IBoiCanhGiaoXu"/> là null) ở MỌI lần mở kết nối, không
/// chỉ lần đầu — Npgsql có thể tái sử dụng một kết nối vật lý đã mở cho một DbContext/tenant
/// KHÁC trước đó mà không tự xoá trạng thái phiên (không có DISCARD ALL mặc định khi trả về
/// pool), nên nếu chỉ đặt một lần lúc khởi tạo, tham số phiên cũ có thể rò sang lượt dùng sau.
/// Dùng <c>set_config(..., is_local := false)</c> qua tham số câu lệnh (không nối chuỗi) để
/// tránh mọi khả năng chèn SQL, dù giá trị chỉ là GUID.
///
/// Khi rỗng (chưa đăng nhập/không có bối cảnh — đường dẫn đăng nhập, công cụ chuyển dữ liệu,
/// tạo tài khoản quản trị đầu tiên), tham số phiên được đặt thành chuỗi rỗng. Chính sách RLS so
/// sánh <c>giao_xu_id::text = current_setting('app.giao_xu_id', true)</c> (ép kiểu CỘT sang
/// text, không ép chuỗi rỗng sang uuid — PostgreSQL ném lỗi ngay khi ép ''::uuid thay vì trả về
/// NULL êm ái) nên chuỗi rỗng không khớp giá trị uuid nào: đây là lựa chọn CỐ Ý "đóng mặc định"
/// (fail-closed) — vai trò CSDL của tiến trình chính (phục vụ nghiệp vụ đã xác thực) không có
/// BYPASSRLS, nên một kết nối không đặt tham số phiên (hoặc kết nối thô ngoài ứng dụng dùng
/// cùng vai trò) sẽ KHÔNG đọc được dòng nào thay vì đọc được mọi giáo xứ. Ba đường dẫn cần truy
/// vấn chéo giáo xứ (đăng nhập, công cụ chuyển dữ liệu, tạo tài khoản quản trị) phải dùng một
/// chuỗi kết nối RIÊNG trỏ tới vai trò có BYPASSRLS — xem "ConnectionStrings:QlgxQuanTri" trong
/// TRIEN-KHAI.md.
/// </summary>
internal sealed class BoiCanhGiaoXuConnectionInterceptor(IBoiCanhGiaoXu? boiCanh) : DbConnectionInterceptor
{
    private const string TenThamSo = "app.giao_xu_id";

    public override void ConnectionOpened(DbConnection connection, ConnectionEndEventData eventData)
        => DatThamSoPhien(connection);

    public override async Task ConnectionOpenedAsync(
        DbConnection connection, ConnectionEndEventData eventData, CancellationToken cancellationToken = default)
        => await DatThamSoPhienAsync(connection, cancellationToken);

    private void DatThamSoPhien(DbConnection connection)
    {
        using var cmd = TaoLenh(connection);
        cmd.ExecuteNonQuery();
    }

    private async Task DatThamSoPhienAsync(DbConnection connection, CancellationToken ct)
    {
        await using var cmd = TaoLenh(connection);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    private DbCommand TaoLenh(DbConnection connection)
    {
        var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT set_config(@ten, @gia_tri, false)";

        var thamSoTen = cmd.CreateParameter();
        thamSoTen.ParameterName = "ten";
        thamSoTen.Value = TenThamSo;
        cmd.Parameters.Add(thamSoTen);

        var thamSoGiaTri = cmd.CreateParameter();
        thamSoGiaTri.ParameterName = "gia_tri";
        thamSoGiaTri.Value = DocGiaoXuIdAnToan();
        cmd.Parameters.Add(thamSoGiaTri);

        return cmd;
    }

    /// <summary>
    /// <see cref="IBoiCanhGiaoXu"/> có thể ĐÃ ĐƯỢC ĐĂNG KÝ qua DI (khác null) nhưng getter
    /// <c>GiaoXuId</c> vẫn ném lỗi khi gọi ngoài một yêu cầu HTTP thật —
    /// BoiCanhGiaoXuTuNguoiDung (Qlgx.Api) làm đúng vậy có chủ đích. Ca này xảy ra khi mã hạ
    /// tầng mở kết nối NGOÀI vòng đời một request (ví dụ chạy migration lúc khởi động ứng dụng,
    /// health check nền). Coi như "không có bối cảnh" (chuỗi rỗng, đóng mặc định) là lựa chọn
    /// AN TOÀN — không rò dữ liệu giáo xứ nào — thay vì để ngoại lệ này làm sập hạ tầng không
    /// liên quan gì tới RLS.
    /// </summary>
    private string DocGiaoXuIdAnToan()
    {
        try
        {
            return boiCanh?.GiaoXuId.ToString("D") ?? "";
        }
        catch (InvalidOperationException)
        {
            return "";
        }
    }
}
