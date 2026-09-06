using Microsoft.EntityFrameworkCore;
using Npgsql;
using Qlgx.Data;
using Qlgx.Domain.Entities;

namespace Qlgx.Data.Tests;

/// <summary>
/// Mỗi lần chạy test tạo một database riêng rồi xoá đi, nên các bộ test không giẫm lên
/// nhau và không cần Docker.
/// </summary>
public class CoSoDuLieuFixture : IAsyncLifetime
{
    private readonly string _tenDb = "qlgx_test_" + Guid.NewGuid().ToString("N")[..12];
    private string _chuoiKetNoiGoc = "";
    public string ChuoiKetNoi { get; private set; } = "";
    public Guid GiaoXuId { get; } = Guid.NewGuid();

    public async Task InitializeAsync()
    {
        _chuoiKetNoiGoc = Environment.GetEnvironmentVariable("QLGX_TEST_PG")
            ?? "Host=localhost;Username=postgres;Password=postgres";

        await using (var ketNoi = new NpgsqlConnection(_chuoiKetNoiGoc + ";Database=postgres"))
        {
            await ketNoi.OpenAsync();
            await using var lenh = new NpgsqlCommand($"CREATE DATABASE \"{_tenDb}\"", ketNoi);
            await lenh.ExecuteNonQueryAsync();
        }

        ChuoiKetNoi = $"{_chuoiKetNoiGoc};Database={_tenDb}";

        await using var ctx = TaoContext();
        await ctx.Database.MigrateAsync();
        ctx.GiaoXu.Add(new GiaoXu { Id = GiaoXuId, TenGiaoXu = "Giao xu Thanh Tam", MaGiaoXuCu = 1 });
        await ctx.SaveChangesAsync();
    }

    public QlgxDbContext TaoContext() =>
        new(new DbContextOptionsBuilder<QlgxDbContext>().UseNpgsql(ChuoiKetNoi).Options);

    public async Task DisposeAsync()
    {
        NpgsqlConnection.ClearAllPools();
        await using var ketNoi = new NpgsqlConnection(_chuoiKetNoiGoc + ";Database=postgres");
        await ketNoi.OpenAsync();
        await using var lenh = new NpgsqlCommand(
            $"DROP DATABASE IF EXISTS \"{_tenDb}\" WITH (FORCE)", ketNoi);
        await lenh.ExecuteNonQueryAsync();
    }
}
