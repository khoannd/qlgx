using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Qlgx.Data;

/// <summary>
/// Chỉ dùng ở design-time cho công cụ `dotnet ef` (sinh migration, cập nhật database từ CLI).
/// Chuỗi kết nối ở đây không cần trỏ tới database thật vì `migrations add` chỉ đọc mô hình,
/// không mở kết nối. Khi Api đăng ký QlgxDbContext trong DI ở giai đoạn sau, factory này
/// vẫn có thể giữ lại mà không xung đột.
/// </summary>
public class QlgxDbContextFactory : IDesignTimeDbContextFactory<QlgxDbContext>
{
    public QlgxDbContext CreateDbContext(string[] args)
    {
        // KHÔNG bao giờ ghi mật khẩu thật vào đây — file này nằm trong git. Chuỗi dự phòng
        // cố ý không có mật khẩu: `migrations add` chỉ đọc mô hình nên vẫn chạy được, còn
        // lệnh nào thật sự cần kết nối sẽ báo lỗi xác thực rõ ràng thay vì âm thầm dùng
        // thông tin đăng nhập lẫn trong mã nguồn. Đặt biến QLGX_TEST_PG khi cần kết nối.
        var chuoiKetNoi = Environment.GetEnvironmentVariable("QLGX_TEST_PG")
            ?? "Host=localhost;Username=postgres;Database=qlgx_design";
        var options = new DbContextOptionsBuilder<QlgxDbContext>()
            .UseNpgsql(chuoiKetNoi)
            .Options;
        return new QlgxDbContext(options);
    }
}
