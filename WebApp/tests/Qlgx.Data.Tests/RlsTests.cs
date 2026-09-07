using Microsoft.EntityFrameworkCore;
using Npgsql;
using Qlgx.Domain.Entities;

namespace Qlgx.Data.Tests;

/// <summary>Bối cảnh giáo xứ cố định dùng riêng cho <see cref="RlsTests"/> — mô phỏng đúng
/// những gì BoiCanhGiaoXuTuNguoiDung làm lúc chạy thật (đọc claim), nhưng không cần HttpContext
/// nào cả.</summary>
file sealed class BoiCanhGiaoXuCoDinh(Guid giaoXuId) : IBoiCanhGiaoXu
{
    public Guid GiaoXuId { get; } = giaoXuId;
}

/// <summary>
/// Chứng minh Row-Level Security (lớp phòng thủ THỨ HAI, migration BatRlsChoBangTheoGiaoXu)
/// thật sự chặn được rò rỉ dữ liệu giữa các giáo xứ ở TẦNG DATABASE — hoàn toàn KHÔNG đi qua
/// EF Core, không đi qua QlgxDbContext, không đi qua bộ lọc toàn cục (lớp phòng thủ thứ nhất).
/// Đây chính xác là kịch bản "một câu truy vấn thô quên lọc" mà RLS phải chặn được, kể cả khi
/// lập trình viên viết SQL tay hoặc dùng thư viện khác EF.
///
/// Dùng một VAI TRÒ CSDL RIÊNG không phải superuser và KHÔNG có BYPASSRLS — vai trò `postgres`
/// (dùng ở mọi bộ test khác qua QLGX_TEST_PG) là superuser nên LUÔN bỏ qua RLS bất kể chính
/// sách gì, không thể dùng để kiểm chứng. Vai trò kiểm thử được tạo và xoá trong chính test
/// này (không để lại rác — vai trò CSDL là tài nguyên CẤP CỤM, không mất khi xoá database).
/// </summary>
public class RlsTests : IAsyncLifetime
{
    private readonly CoSoDuLieuFixture _fixture = new();
    private readonly string _tenVaiTro = "qlgx_rls_test_" + Guid.NewGuid().ToString("N")[..12];
    private const string MatKhauVaiTro = "kiem-thu-rls-khong-dung-that";

    public async Task InitializeAsync() => await _fixture.InitializeAsync();

    public async Task DisposeAsync()
    {
        // Xoá vai trò TRƯỚC khi fixture xoá database — role còn quyền tham chiếu tới database
        // này (privilege đã cấp) nên xoá theo thứ tự ngược lại lúc tạo.
        await using (var superuser = new NpgsqlConnection(_fixture.ChuoiKetNoi))
        {
            await superuser.OpenAsync();
            await using var lenh = new NpgsqlCommand(
                $"DROP OWNED BY \"{_tenVaiTro}\"; DROP ROLE IF EXISTS \"{_tenVaiTro}\";", superuser);
            await lenh.ExecuteNonQueryAsync();
        }

        await _fixture.DisposeAsync();
    }

    [Fact]
    public async Task Vai_tro_khong_bypassrls_chi_thay_dong_cua_giao_xu_da_dat_trong_tham_so_phien()
    {
        var giaoXuA = _fixture.GiaoXuId;
        var giaoXuB = Guid.NewGuid();

        // Chèn dữ liệu và tạo vai trò kiểm thử qua chính EF (superuser, bo qua RLS) — chỉ dùng
        // EF ở bước CHUẨN BỊ dữ liệu, phần KIỂM CHỨNG bên dưới hoàn toàn dùng Npgsql thô.
        await using (var ctx = _fixture.TaoContext())
        {
            ctx.GiaoXu.Add(new GiaoXu { Id = giaoXuB, TenGiaoXu = "Giao xu Thanh Gia", MaGiaoXuCu = 2 });
            ctx.GiaoHo.Add(new GiaoHo { GiaoXuId = giaoXuA, TenGiaoHo = "Giao ho A" });
            ctx.GiaoHo.Add(new GiaoHo { GiaoXuId = giaoXuB, TenGiaoHo = "Giao ho B" });
            await ctx.SaveChangesAsync();
        }

        await using (var superuser = new NpgsqlConnection(_fixture.ChuoiKetNoi))
        {
            await superuser.OpenAsync();
            await using var taoVaiTro = new NpgsqlCommand(
                $"""
                CREATE ROLE "{_tenVaiTro}" LOGIN PASSWORD '{MatKhauVaiTro}' NOSUPERUSER NOBYPASSRLS;
                GRANT SELECT, INSERT, UPDATE, DELETE ON giao_ho TO "{_tenVaiTro}";
                """, superuser);
            await taoVaiTro.ExecuteNonQueryAsync();
        }

        var builder = new NpgsqlConnectionStringBuilder(_fixture.ChuoiKetNoi)
        {
            Username = _tenVaiTro,
            Password = MatKhauVaiTro,
        };

        async Task<List<Guid>> DocGiaoXuId(Guid? datThamSoPhien)
        {
            await using var ketNoi = new NpgsqlConnection(builder.ConnectionString);
            await ketNoi.OpenAsync();
            if (datThamSoPhien is { } id)
            {
                await using var datPhien = new NpgsqlCommand(
                    "SELECT set_config('app.giao_xu_id', @v, false)", ketNoi);
                datPhien.Parameters.AddWithValue("v", id.ToString("D"));
                await datPhien.ExecuteNonQueryAsync();
            }

            var ketQua = new List<Guid>();
            await using var truyVan = new NpgsqlCommand("SELECT giao_xu_id FROM giao_ho", ketNoi);
            await using var reader = await truyVan.ExecuteReaderAsync();
            while (await reader.ReadAsync()) ketQua.Add(reader.GetGuid(0));
            return ketQua;
        }

        // Đặt tham số phiên = giáo xứ A -> chỉ thấy đúng 1 dòng của A, KHÔNG thấy dòng của B.
        var choA = await DocGiaoXuId(giaoXuA);
        Assert.Single(choA);
        Assert.All(choA, id => Assert.Equal(giaoXuA, id));

        // Đổi sang giáo xứ B trên CÙNG vai trò -> chỉ thấy đúng 1 dòng của B.
        var choB = await DocGiaoXuId(giaoXuB);
        Assert.Single(choB);
        Assert.All(choB, id => Assert.Equal(giaoXuB, id));

        // Không đặt tham số phiên nào (kết nối thô "quên" gọi set_config, đúng kịch bản
        // nguy hiểm nhất) -> đóng mặc định, không thấy dòng nào, KHÔNG rò dữ liệu.
        var khongDatGi = await DocGiaoXuId(datThamSoPhien: null);
        Assert.Empty(khongDatGi);
    }

    [Fact]
    public async Task Vai_tro_khong_bypassrls_khong_the_ghi_du_lieu_gan_sai_giao_xu()
    {
        var giaoXuA = _fixture.GiaoXuId;
        var giaoXuB = Guid.NewGuid();

        await using (var ctx = _fixture.TaoContext())
        {
            ctx.GiaoXu.Add(new GiaoXu { Id = giaoXuB, TenGiaoXu = "Giao xu Thanh Gia", MaGiaoXuCu = 2 });
            await ctx.SaveChangesAsync();
        }

        await using (var superuser = new NpgsqlConnection(_fixture.ChuoiKetNoi))
        {
            await superuser.OpenAsync();
            await using var taoVaiTro = new NpgsqlCommand(
                $"""
                CREATE ROLE "{_tenVaiTro}" LOGIN PASSWORD '{MatKhauVaiTro}' NOSUPERUSER NOBYPASSRLS;
                GRANT SELECT, INSERT, UPDATE, DELETE ON giao_ho TO "{_tenVaiTro}";
                """, superuser);
            await taoVaiTro.ExecuteNonQueryAsync();
        }

        var builder = new NpgsqlConnectionStringBuilder(_fixture.ChuoiKetNoi)
        {
            Username = _tenVaiTro,
            Password = MatKhauVaiTro,
        };
        await using var ketNoi = new NpgsqlConnection(builder.ConnectionString);
        await ketNoi.OpenAsync();
        await using (var datPhien = new NpgsqlCommand("SELECT set_config('app.giao_xu_id', @v, false)", ketNoi))
        {
            datPhien.Parameters.AddWithValue("v", giaoXuA.ToString("D"));
            await datPhien.ExecuteNonQueryAsync();
        }

        // Phiên đang "là" giáo xứ A nhưng cố GHI một dòng ghi rõ GiaoXuId = B -> WITH CHECK
        // của policy phải chặn, không cho lách bằng cách tự ghi giao_xu_id của giáo xứ khác.
        // KHÔNG chèn cột xmin — đó là cột hệ thống PostgreSQL (ánh xạ RowVersion), không thể
        // ghi trực tiếp qua INSERT.
        await using var themSai = new NpgsqlCommand(
            "INSERT INTO giao_ho (id, giao_xu_id, ten_giao_ho, ma_giao_ho_cu, da_xoa, " +
            "created_at, updated_at) " +
            "VALUES (@id, @gx, 'Giao ho lach', 0, false, now(), now())", ketNoi);
        themSai.Parameters.AddWithValue("id", Guid.NewGuid());
        themSai.Parameters.AddWithValue("gx", giaoXuB);

        var ngoaiLe = await Assert.ThrowsAsync<PostgresException>(() => themSai.ExecuteNonQueryAsync());
        Assert.Equal("42501", ngoaiLe.SqlState); // insufficient_privilege — policy WITH CHECK chan
    }

    /// <summary>
    /// Task VIEC-TIEP-THEO.md mục 2.2 — hai test phía trên chứng minh RLS có tác dụng ở TẦNG
    /// DATABASE bằng Npgsql thô, nhưng "tách vai trò CSDL chạy thật" nghĩa là chính
    /// <c>QlgxDbContext</c> (DbContext nghiệp vụ THẬT dùng ở Program.cs/mọi Service) phải hoạt
    /// động đúng khi kết nối bằng vai trò KHÔNG BYPASSRLS — đây là con đường THẬT SỰ đi qua lúc
    /// chạy sản phẩm (Program.cs đăng ký ConnectionStrings:Qlgx trỏ vào vai trò `qlgx_app`,
    /// TRIEN-KHAI.md mục 5), khác với hai test trên vốn cố tình đi vòng qua EF Core để mô
    /// phỏng "một câu SQL thô quên lọc". Test này dựng thẳng hai QlgxDbContext (một cho mỗi
    /// giáo xứ, qua BoiCanhGiaoXuCoDinh — đúng cơ chế interceptor OnConfiguring dùng lúc chạy
    /// thật) trên CÙNG một kết nối vai trò không BYPASSRLS, và xác nhận CẢ bộ lọc EF (lớp
    /// phòng thủ thứ nhất) LẪN RLS (lớp phòng thủ thứ hai) cùng hoạt động — vai trò không
    /// BYPASSRLS + interceptor tự set_config đúng giáo xứ là đủ, không cần code nghiệp vụ nào
    /// khác phải biết tới RLS.
    /// </summary>
    [Fact]
    public async Task QlgxDbContext_that_qua_vai_tro_khong_bypassrls_van_cach_ly_dung_giao_xu()
    {
        var giaoXuA = _fixture.GiaoXuId;
        var giaoXuB = Guid.NewGuid();

        await using (var ctx = _fixture.TaoContext())
        {
            ctx.GiaoXu.Add(new GiaoXu { Id = giaoXuB, TenGiaoXu = "Giao xu Thanh Gia (EF that)", MaGiaoXuCu = 3 });
            await ctx.SaveChangesAsync();
        }

        await using (var superuser = new NpgsqlConnection(_fixture.ChuoiKetNoi))
        {
            await superuser.OpenAsync();
            await using var taoVaiTro = new NpgsqlCommand(
                $"""
                CREATE ROLE "{_tenVaiTro}" LOGIN PASSWORD '{MatKhauVaiTro}' NOSUPERUSER NOBYPASSRLS;
                GRANT SELECT, INSERT, UPDATE, DELETE ON giao_ho TO "{_tenVaiTro}";
                """, superuser);
            await taoVaiTro.ExecuteNonQueryAsync();
        }

        var chuoiVaiTro = new NpgsqlConnectionStringBuilder(_fixture.ChuoiKetNoi)
        {
            Username = _tenVaiTro,
            Password = MatKhauVaiTro,
        }.ConnectionString;

        QlgxDbContext TaoContextVaiTroThat(Guid giaoXu) => new(
            new DbContextOptionsBuilder<QlgxDbContext>().UseNpgsql(chuoiVaiTro).Options,
            new BoiCanhGiaoXuCoDinh(giaoXu));

        // Ghi qua ĐÚNG DbContext nghiệp vụ (giống mọi Service thật) — mỗi context tự đặt tham
        // số phiên đúng giáo xứ của nó lúc mở kết nối (BoiCanhGiaoXuConnectionInterceptor).
        await using (var ctxA = TaoContextVaiTroThat(giaoXuA))
        {
            ctxA.GiaoHo.Add(new GiaoHo { GiaoXuId = giaoXuA, TenGiaoHo = "Giao ho A (EF that)" });
            await ctxA.SaveChangesAsync();
        }
        await using (var ctxB = TaoContextVaiTroThat(giaoXuB))
        {
            ctxB.GiaoHo.Add(new GiaoHo { GiaoXuId = giaoXuB, TenGiaoHo = "Giao ho B (EF that)" });
            await ctxB.SaveChangesAsync();
        }

        // Đọc bằng context của giáo xứ A: chỉ thấy đúng giáo họ của A — cả bộ lọc EF lẫn RLS
        // đều đồng ý, dù kiểm chỉ MỘT trong hai cũng đủ chặn ở đây (phòng thủ theo chiều sâu).
        await using (var ctxDocA = TaoContextVaiTroThat(giaoXuA))
        {
            var ds = await ctxDocA.GiaoHo.Select(h => h.TenGiaoHo).ToListAsync();
            Assert.Contains("Giao ho A (EF that)", ds);
            Assert.DoesNotContain("Giao ho B (EF that)", ds);
        }
        await using (var ctxDocB = TaoContextVaiTroThat(giaoXuB))
        {
            var ds = await ctxDocB.GiaoHo.Select(h => h.TenGiaoHo).ToListAsync();
            Assert.Contains("Giao ho B (EF that)", ds);
            Assert.DoesNotContain("Giao ho A (EF that)", ds);
        }

        // Context KHÔNG có bối cảnh giáo xứ (boiCanh=null, đúng như công cụ chuyển đổi dữ
        // liệu) trên vai trò không BYPASSRLS: bộ lọc EF tắt (BoiCanhGiaoXuId==null cho qua),
        // nhưng RLS ở tầng database vẫn đóng — set_config('app.giao_xu_id','') từ interceptor,
        // KHÔNG đọc được dòng nào. Đây là bằng chứng RLS là lớp phòng thủ ĐỘC LẬP, không chỉ
        // "ăn theo" bộ lọc EF.
        await using var ctxKhongBoiCanh = new QlgxDbContext(
            new DbContextOptionsBuilder<QlgxDbContext>().UseNpgsql(chuoiVaiTro).Options);
        var khongThayGi = await ctxKhongBoiCanh.GiaoHo.Select(h => h.TenGiaoHo).ToListAsync();
        Assert.Empty(khongThayGi);
    }
}
