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
        var chuoiKetNoi = Environment.GetEnvironmentVariable("QLGX_TEST_PG")
            ?? "Host=localhost;Username=postgres;Password=postgres;Database=qlgx_design";
        var options = new DbContextOptionsBuilder<QlgxDbContext>()
            .UseNpgsql(chuoiKetNoi)
            .Options;
        return new QlgxDbContext(options);
    }
}
