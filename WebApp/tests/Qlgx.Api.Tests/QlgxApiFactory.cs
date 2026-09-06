using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Qlgx.Data;
using Qlgx.Domain.Entities;

namespace Qlgx.Api.Tests;

public class QlgxApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly string _tenDb = "qlgx_api_" + Guid.NewGuid().ToString("N")[..12];
    private string _goc = "";
    public string ChuoiKetNoi { get; private set; } = "";
    public Guid GiaoXuId { get; } = Guid.Parse("00000000-0000-0000-0000-0000000000aa");

    public async Task InitializeAsync()
    {
        // KHÔNG ghi mật khẩu thật vào mã nguồn — file này nằm trong git. Bộ test cần một
        // PostgreSQL thật nên thà báo lỗi rõ ràng còn hơn âm thầm thử một mật khẩu đoán được.
        _goc = Environment.GetEnvironmentVariable("QLGX_TEST_PG")
            ?? throw new InvalidOperationException(
                "Chưa đặt biến môi trường QLGX_TEST_PG. Bộ test cần một PostgreSQL thật. " +
                "Ví dụ: setx QLGX_TEST_PG \"Host=localhost;Username=postgres;Password=<mật khẩu của bạn>\" " +
                "rồi mở lại cửa sổ dòng lệnh.");

        await using (var kn = new NpgsqlConnection(_goc + ";Database=postgres"))
        {
            await kn.OpenAsync();
            await using var lenh = new NpgsqlCommand($"CREATE DATABASE \"{_tenDb}\"", kn);
            await lenh.ExecuteNonQueryAsync();
        }
        ChuoiKetNoi = $"{_goc};Database={_tenDb}";

        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<QlgxDbContext>();
        await db.Database.MigrateAsync();
        db.GiaoXu.Add(new GiaoXu { Id = GiaoXuId, TenGiaoXu = "Giao xu Thanh Tam", MaGiaoXuCu = 1 });
        await db.SaveChangesAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:Qlgx", ChuoiKetNoi);
        builder.UseSetting("Qlgx:GiaoXuId", GiaoXuId.ToString());
    }

    /// <summary>Mở một DbContext trỏ thẳng vào database test, bỏ qua bộ lọc giáo xứ.</summary>
    public QlgxDbContext TaoContextThuan() =>
        new(new DbContextOptionsBuilder<QlgxDbContext>().UseNpgsql(ChuoiKetNoi).Options);

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
        NpgsqlConnection.ClearAllPools();
        await using var kn = new NpgsqlConnection(_goc + ";Database=postgres");
        await kn.OpenAsync();
        await using var lenh = new NpgsqlCommand($"DROP DATABASE IF EXISTS \"{_tenDb}\" WITH (FORCE)", kn);
        await lenh.ExecuteNonQueryAsync();
    }
}
