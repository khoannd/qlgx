using System.Net.Http.Headers;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Qlgx.Api.Services;
using Qlgx.Data;
using Qlgx.Domain.Entities;

namespace Qlgx.Api.Tests;

public class QlgxApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly string _tenDb = "qlgx_api_" + Guid.NewGuid().ToString("N")[..12];
    private string _goc = "";
    public string ChuoiKetNoi { get; private set; } = "";
    public Guid GiaoXuId { get; } = Guid.Parse("00000000-0000-0000-0000-0000000000aa");

    // Khoa ky JWT random cho MOI lan chay bo test — khong ghi bat ky khoa co dinh nao vao mã
    // nguồn (xem TokenService.cs: khoa ky KHONG duoc la bi mat trong repo). Vì đây là một
    // tiến trình test ngắn hạn, dùng luôn seed ngẫu nhiên thay vì đọc biến môi trường riêng.
    private readonly string _jwtKey = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

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
        builder.UseSetting("Qlgx:JwtKey", _jwtKey);
    }

    /// <summary>Mở một DbContext trỏ thẳng vào database test, bỏ qua bộ lọc giáo xứ.</summary>
    public QlgxDbContext TaoContextThuan() =>
        new(new DbContextOptionsBuilder<QlgxDbContext>().UseNpgsql(ChuoiKetNoi).Options);

    /// <summary>Phát hành trực tiếp một token hợp lệ (không qua HTTP đăng nhập thật) — dùng
    /// cho phần lớn test chỉ cần "một người dùng đã đăng nhập của giáo xứ X", không kiểm tra
    /// luồng đăng nhập. Test luồng đăng nhập thật (AuthTests) gọi thẳng
    /// POST /api/auth/dang-nhap.</summary>
    public string PhatHanhToken(Guid? giaoXuId = null, int loaiTaiKhoan = 0, string tenTaiKhoan = "nguoi_test")
    {
        using var scope = Services.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<TokenService>();
        return tokenService.PhatHanh(new TaiKhoan
        {
            Id = Guid.NewGuid(),
            GiaoXuId = giaoXuId ?? GiaoXuId,
            LoaiTaiKhoan = loaiTaiKhoan,
            TenTaiKhoan = tenTaiKhoan,
            HoTenNguoiDung = "Nguoi dung test",
        });
    }

    /// <summary>HttpClient đã đăng nhập — thay cho <c>CreateClient()</c> ở MỌI test nghiệp vụ,
    /// vì mọi endpoint nghiệp vụ giờ đòi hỏi xác thực (Task 14). Test cố ý gọi không xác thực
    /// hoặc xác thực giáo xứ khác thì dùng <c>CreateClient()</c>/tham số riêng.</summary>
    public HttpClient CreateAuthClient(Guid? giaoXuId = null, int loaiTaiKhoan = 0)
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", PhatHanhToken(giaoXuId, loaiTaiKhoan));
        return client;
    }

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
