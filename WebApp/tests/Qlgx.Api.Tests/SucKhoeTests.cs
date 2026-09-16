using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Qlgx.Api.Tests;

public class SucKhoeTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task Endpoint_suc_khoe_tra_ve_trang_thai_ok()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/suc-khoe");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<SucKhoeResponse>();
        body!.TrangThai.Should().Be("ok");
        body.PhienBan.Should().NotBeNullOrWhiteSpace();
    }

    private sealed record SucKhoeResponse(string TrangThai, string PhienBan);
}
