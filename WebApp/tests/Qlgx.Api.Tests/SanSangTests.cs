using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Npgsql;

namespace Qlgx.Api.Tests;

public class SanSangTests(QlgxApiFactory factory) : IClassFixture<QlgxApiFactory>
{
    [Fact]
    public async Task Co_csdl_that_va_da_migrate_thi_tra_200_san_sang()
    {
        var client = factory.CreateClient();

        var res = await client.GetAsync("/api/suc-khoe/san-sang");

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        var than = await res.Content.ReadFromJsonAsync<SanSangDto>();
        than!.TrangThai.Should().Be("san-sang");
        than.SoMigrationConThieu.Should().Be(0);
    }

    private sealed record SanSangDto(string TrangThai, int SoMigrationConThieu, string? LyDo);
}

public class SanSangKhongCoCsdlTests
{
    [Fact]
    public async Task Khong_ket_noi_duoc_csdl_thi_tra_503()
    {
        // Cổng 1 chắc chắn không có PostgreSQL nào lắng nghe — mô phỏng CSDL chết.
        using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(b =>
        {
            b.UseSetting("ConnectionStrings:Qlgx",
                "Host=127.0.0.1;Port=1;Database=qlgx;Username=x;Password=y;Timeout=2");
            b.UseSetting("Qlgx:JwtKey", Convert.ToBase64String(
                System.Security.Cryptography.RandomNumberGenerator.GetBytes(32)));
        });
        var client = factory.CreateClient();

        var res = await client.GetAsync("/api/suc-khoe/san-sang");

        res.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task Liveness_van_tra_200_du_csdl_chet()
    {
        // Phân biệt rõ hai loại kiểm tra: liveness KHÔNG được phụ thuộc CSDL, nếu không
        // Docker sẽ giết container API chỉ vì PostgreSQL khởi động chậm hơn.
        using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(b =>
        {
            b.UseSetting("ConnectionStrings:Qlgx",
                "Host=127.0.0.1;Port=1;Database=qlgx;Username=x;Password=y;Timeout=2");
            b.UseSetting("Qlgx:JwtKey", Convert.ToBase64String(
                System.Security.Cryptography.RandomNumberGenerator.GetBytes(32)));
        });

        var res = await factory.CreateClient().GetAsync("/api/suc-khoe");

        res.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}

public class SanSangConMigrationChieuTests : IAsyncLifetime
{
    private readonly string _tenDb = "qlgx_api_" + Guid.NewGuid().ToString("N")[..12];
    private string _goc = "";
    private string _chuoiKetNoi = "";

    public async Task InitializeAsync()
    {
        // Tạo database trống, không chạy migration — để kiểm tra endpoint phát hiện migrations chưa áp dụng.
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
        _chuoiKetNoi = $"{_goc};Database={_tenDb}";
        // KHÔNG chạy migration — database sẽ trống, còn pending migrations.
    }

    [Fact]
    public async Task Con_migration_chua_ap_dung_thi_tra_503()
    {
        // Database trống (không có migration nào áp dụng) — readiness phải trả 503.
        using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(b =>
        {
            b.UseSetting("ConnectionStrings:Qlgx", _chuoiKetNoi);
            b.UseSetting("Qlgx:JwtKey", Convert.ToBase64String(
                System.Security.Cryptography.RandomNumberGenerator.GetBytes(32)));
        });
        var client = factory.CreateClient();

        var res = await client.GetAsync("/api/suc-khoe/san-sang");

        res.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
        var than = await res.Content.ReadFromJsonAsync<SanSangDto>();
        than!.TrangThai.Should().Be("chua-san-sang");
        than.SoMigrationConThieu.Should().BeGreaterThan(0);
    }

    public async Task DisposeAsync()
    {
        // Dọn database sau khi test xong.
        await using var kn = new NpgsqlConnection(_goc + ";Database=postgres");
        await kn.OpenAsync();
        await using var lenh = new NpgsqlCommand($"DROP DATABASE IF EXISTS \"{_tenDb}\" WITH (FORCE)", kn);
        await lenh.ExecuteNonQueryAsync();
    }

    private sealed record SanSangDto(string TrangThai, int SoMigrationConThieu, string? LyDo);
}
