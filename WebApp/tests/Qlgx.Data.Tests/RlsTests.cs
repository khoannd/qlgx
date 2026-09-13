using Microsoft.EntityFrameworkCore;
using Npgsql;
using Qlgx.Data.NhatKy;
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
    /// Hai bảng "có dòng cấp hệ thống" (<c>mau_in_tuy_chinh</c>, <c>cach_hien_thi_dung_sai</c>)
    /// dùng policy RIÊNG nhận biết NULL (migration BatRlsChoBangCoDongHeThong) vì
    /// <c>giao_xu_id IS NULL</c> ở đây mang nghĩa "áp dụng cho MỌI giáo xứ chưa tự tuỳ chỉnh".
    /// Bài test phải chứng minh ĐỒNG THỜI hai điều ngược chiều nhau — bỏ sót vế nào cũng hỏng:
    ///   (1) vẫn CÁCH LY được dòng riêng: giáo xứ A không đọc được dòng của giáo xứ B;
    ///   (2) vẫn ĐỌC ĐƯỢC dòng hệ thống: nếu policy chung <c>giao_xu_id::text = ...</c> được áp
    ///       nhầm vào đây thì dòng NULL bị giấu mất và tính năng "Quản trị hệ thống đặt mẫu in /
    ///       câu chữ dùng chung" sẽ im lặng ngừng hoạt động với mọi giáo xứ.
    /// </summary>
    [Fact]
    public async Task Bang_co_dong_he_thong_cach_ly_dong_rieng_nhung_van_cho_doc_dong_he_thong()
    {
        var giaoXuA = _fixture.GiaoXuId;
        var giaoXuB = Guid.NewGuid();

        await using (var ctx = _fixture.TaoContext())
        {
            ctx.GiaoXu.Add(new GiaoXu { Id = giaoXuB, TenGiaoXu = "Giao xu B (dong he thong)", MaGiaoXuCu = 4 });
            await ctx.SaveChangesAsync();
        }

        // Chuẩn bị dữ liệu + vai trò bằng superuser (bỏ qua RLS) — phần KIỂM CHỨNG bên dưới mới
        // dùng vai trò không BYPASSRLS.
        await using (var superuser = new NpgsqlConnection(_fixture.ChuoiKetNoi))
        {
            await superuser.OpenAsync();

            await using (var chen = new NpgsqlCommand(
                """
                INSERT INTO mau_in_tuy_chinh (id, giao_xu_id, ten_mau, noi_dung_html, created_at, updated_at)
                VALUES (gen_random_uuid(), @a,    'LyLichCaNhan', 'rieng-A',  now(), now()),
                       (gen_random_uuid(), @b,    'LyLichCaNhan', 'rieng-B',  now(), now()),
                       (gen_random_uuid(), NULL,  'LyLichCaNhan', 'he-thong', now(), now());
                INSERT INTO cach_hien_thi_dung_sai (id, giao_xu_id, ten_bien, khi_dung, khi_sai, created_at, updated_at)
                VALUES (gen_random_uuid(), @a,   'TanTong', 'rieng-A',  NULL, now(), now()),
                       (gen_random_uuid(), @b,   'TanTong', 'rieng-B',  NULL, now(), now()),
                       (gen_random_uuid(), NULL, 'TanTong', 'he-thong', NULL, now(), now());
                """, superuser))
            {
                chen.Parameters.AddWithValue("a", giaoXuA);
                chen.Parameters.AddWithValue("b", giaoXuB);
                await chen.ExecuteNonQueryAsync();
            }

            await using var taoVaiTro = new NpgsqlCommand(
                $"""
                CREATE ROLE "{_tenVaiTro}" LOGIN PASSWORD '{MatKhauVaiTro}' NOSUPERUSER NOBYPASSRLS;
                GRANT SELECT, INSERT, UPDATE, DELETE ON mau_in_tuy_chinh TO "{_tenVaiTro}";
                GRANT SELECT, INSERT, UPDATE, DELETE ON cach_hien_thi_dung_sai TO "{_tenVaiTro}";
                """, superuser);
            await taoVaiTro.ExecuteNonQueryAsync();
        }

        var chuoiVaiTro = new NpgsqlConnectionStringBuilder(_fixture.ChuoiKetNoi)
        {
            Username = _tenVaiTro,
            Password = MatKhauVaiTro,
        }.ConnectionString;

        async Task<List<string>> Doc(string bang, string cot, Guid? datThamSoPhien)
        {
            await using var ketNoi = new NpgsqlConnection(chuoiVaiTro);
            await ketNoi.OpenAsync();
            if (datThamSoPhien is { } id)
            {
                await using var datPhien = new NpgsqlCommand(
                    "SELECT set_config('app.giao_xu_id', @v, false)", ketNoi);
                datPhien.Parameters.AddWithValue("v", id.ToString("D"));
                await datPhien.ExecuteNonQueryAsync();
            }

            var ketQua = new List<string>();
            await using var truyVan = new NpgsqlCommand($"SELECT {cot} FROM {bang}", ketNoi);
            await using var reader = await truyVan.ExecuteReaderAsync();
            while (await reader.ReadAsync()) ketQua.Add(reader.GetString(0));
            return ketQua;
        }

        foreach (var (bang, cot) in new[]
                 { ("mau_in_tuy_chinh", "noi_dung_html"), ("cach_hien_thi_dung_sai", "khi_dung") })
        {
            var choA = await Doc(bang, cot, giaoXuA);
            Assert.Contains("rieng-A", choA);
            Assert.Contains("he-thong", choA); // vế (2) — dòng dùng chung phải đọc được
            Assert.DoesNotContain("rieng-B", choA); // vế (1) — vẫn cách ly giáo xứ

            var choB = await Doc(bang, cot, giaoXuB);
            Assert.Contains("rieng-B", choB);
            Assert.Contains("he-thong", choB);
            Assert.DoesNotContain("rieng-A", choB);

            // Kết nối thô quên đặt tham số phiên: chỉ còn thấy dòng dùng chung, TUYỆT ĐỐI không
            // thấy dòng riêng của giáo xứ nào — dòng hệ thống vốn không phải bí mật (mẫu in mặc
            // định dùng chung), nhưng dữ liệu riêng của giáo xứ thì vẫn phải đóng.
            var khongDatGi = await Doc(bang, cot, datThamSoPhien: null);
            Assert.Equal(["he-thong"], khongDatGi);
        }
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

    /// <summary>
    /// Nhật ký thay đổi (migration ThemBangNhatKyThayDoi) chứa NGUYÊN VĂN giá trị các ô dữ liệu,
    /// nên rò rỉ ở đây tương đương rò rỉ toàn bộ sổ sách giáo xứ — phải kiểm chứng ở TẦNG
    /// DATABASE bằng Npgsql thô, đúng lối các test RLS phía trên, không đi qua EF Core.
    ///
    /// Kiểm CẢ hai bảng thay_doi VÀ hieu_luc: hieu_luc là bảng DUY NHẤT máy con kéo về, nên rò
    /// rỉ nó tương đương rò rỉ toàn bộ sổ sách — và nếu sau này ai tách policy riêng cho từng
    /// bảng, test lặp trên cả hai vẫn bắt được lỗi thay vì chỉ xanh nhờ trùng policy hiện tại.
    /// </summary>
    [Fact]
    public async Task Bang_nhat_ky_chiu_rls_nhu_bang_nghiep_vu()
    {
        var giaoXuA = _fixture.GiaoXuId;
        var giaoXuB = Guid.NewGuid();

        await using (var ctx = _fixture.TaoContext())
        {
            ctx.GiaoXu.Add(new GiaoXu { Id = giaoXuB, TenGiaoXu = "Giao xu Thanh Gia", MaGiaoXuCu = 3 });
            ctx.ThayDoi.AddRange(
                new ThayDoi
                {
                    GiaoXuId = giaoXuA, Bang = "GiaoDan", BanGhiId = Guid.NewGuid(),
                    Truong = "HoTen", GiaTri = "\"Nguoi cua xu A\"", Loai = "sua",
                    DongHoVatLy = DateTimeOffset.UtcNow, MaThaoTac = Guid.NewGuid(),
                    GiaoDichId = Guid.NewGuid(),
                },
                new ThayDoi
                {
                    GiaoXuId = giaoXuB, Bang = "GiaoDan", BanGhiId = Guid.NewGuid(),
                    Truong = "HoTen", GiaTri = "\"Nguoi cua xu B\"", Loai = "sua",
                    DongHoVatLy = DateTimeOffset.UtcNow, MaThaoTac = Guid.NewGuid(),
                    GiaoDichId = Guid.NewGuid(),
                });
            ctx.HieuLuc.AddRange(
                new HieuLuc
                {
                    GiaoXuId = giaoXuA, SoThuTu = 1, Epoch = Guid.NewGuid(),
                    Bang = "GiaoDan", BanGhiId = Guid.NewGuid(), Truong = "HoTen",
                    GiaTri = "\"Nguoi cua xu A\"", DongHoVatLy = DateTimeOffset.UtcNow,
                    GiaoDichId = Guid.NewGuid(),
                },
                new HieuLuc
                {
                    GiaoXuId = giaoXuB, SoThuTu = 1, Epoch = Guid.NewGuid(),
                    Bang = "GiaoDan", BanGhiId = Guid.NewGuid(), Truong = "HoTen",
                    GiaTri = "\"Nguoi cua xu B\"", DongHoVatLy = DateTimeOffset.UtcNow,
                    GiaoDichId = Guid.NewGuid(),
                });
            await ctx.SaveChangesAsync();
        }

        await using (var superuser = new NpgsqlConnection(_fixture.ChuoiKetNoi))
        {
            await superuser.OpenAsync();
            await using var taoVaiTro = new NpgsqlCommand(
                $"""
                CREATE ROLE "{_tenVaiTro}" LOGIN PASSWORD '{MatKhauVaiTro}' NOSUPERUSER NOBYPASSRLS;
                GRANT SELECT, INSERT, UPDATE, DELETE ON thay_doi TO "{_tenVaiTro}";
                GRANT SELECT, INSERT, UPDATE, DELETE ON hieu_luc TO "{_tenVaiTro}";
                """, superuser);
            await taoVaiTro.ExecuteNonQueryAsync();
        }

        var builder = new NpgsqlConnectionStringBuilder(_fixture.ChuoiKetNoi)
        {
            Username = _tenVaiTro,
            Password = MatKhauVaiTro,
        };

        async Task<List<string>> DocGiaTri(string bang, Guid? datThamSoPhien)
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

            var ketQua = new List<string>();
            await using var truyVan = new NpgsqlCommand($"SELECT gia_tri::text FROM {bang}", ketNoi);
            await using var reader = await truyVan.ExecuteReaderAsync();
            while (await reader.ReadAsync()) ketQua.Add(reader.GetString(0));
            return ketQua;
        }

        foreach (var bang in new[] { "thay_doi", "hieu_luc" })
        {
            var choA = await DocGiaTri(bang, giaoXuA);
            Assert.Single(choA);
            Assert.Contains("xu A", choA[0]);

            var choB = await DocGiaTri(bang, giaoXuB);
            Assert.Single(choB);
            Assert.Contains("xu B", choB[0]);

            // Kết nối thô "quên" gọi set_config -> đóng mặc định, không rò một dòng nào.
            Assert.Empty(await DocGiaTri(bang, datThamSoPhien: null));
        }
    }

    /// <summary>
    /// Task 1 (CapSoHieuLuc.LayDaiSo) mới chỉ được kiểm bằng vai trò postgres (superuser →
    /// BYPASSRLS mặc định), nên chưa từng chứng minh nó chạy được dưới vai trò THẬT
    /// (qlgx_app, không BYPASSRLS) mà Program.cs dùng lúc sản xuất. Hàm này INSERT rồi
    /// SELECT ... FOR UPDATE trên bo_dem_hieu_luc — cả hai câu đều chịu policy loc_theo_giao_xu
    /// (bật ở migration ThemBangNhatKyThayDoi) nên phải chạy trong một QlgxDbContext có ĐÚNG
    /// app.giao_xu_id đặt qua interceptor, giống hệt cách Program.cs mở kết nối thật.
    /// </summary>
    [Fact]
    public async Task CapSoHieuLuc_chay_duoc_qua_vai_tro_khong_bypassrls_khi_boi_canh_dung_giao_xu()
    {
        var giaoXuA = _fixture.GiaoXuId;

        await using (var superuser = new NpgsqlConnection(_fixture.ChuoiKetNoi))
        {
            await superuser.OpenAsync();
            await using var taoVaiTro = new NpgsqlCommand(
                $"""
                CREATE ROLE "{_tenVaiTro}" LOGIN PASSWORD '{MatKhauVaiTro}' NOSUPERUSER NOBYPASSRLS;
                GRANT SELECT, INSERT, UPDATE, DELETE ON bo_dem_hieu_luc TO "{_tenVaiTro}";
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

        // Lần cấp ĐẦU (dòng đếm chưa tồn tại) -> nhánh INSERT ... ON CONFLICT phải qua được
        // WITH CHECK của policy vì giao_xu_id ghi đúng bằng app.giao_xu_id của phiên.
        await using (var ctx = TaoContextVaiTroThat(giaoXuA))
        {
            await using var giaoDich = await ctx.Database.BeginTransactionAsync();
            var (soDauLan1, _) = await CapSoHieuLuc.LayDaiSo(ctx, giaoXuA, 3, CancellationToken.None);
            await giaoDich.CommitAsync();
            Assert.Equal(1, soDauLan1);
        }

        // Lần cấp THỨ HAI (dòng đếm đã có) -> nhánh SELECT ... FOR UPDATE phải đọc được đúng
        // dòng của giáo xứ này qua policy, không bị lọc sạch thành "Sequence contains no
        // elements".
        await using (var ctx = TaoContextVaiTroThat(giaoXuA))
        {
            await using var giaoDich = await ctx.Database.BeginTransactionAsync();
            var (soDauLan2, _) = await CapSoHieuLuc.LayDaiSo(ctx, giaoXuA, 2, CancellationToken.None);
            await giaoDich.CommitAsync();
            Assert.Equal(4, soDauLan2); // 1 + 3 (da cap lan dau) = 4
        }
    }
}
