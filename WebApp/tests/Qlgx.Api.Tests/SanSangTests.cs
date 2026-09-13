using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

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
