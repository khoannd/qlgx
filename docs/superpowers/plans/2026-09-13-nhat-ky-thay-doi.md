# Nhật ký thay đổi mức trường — Kế hoạch thi công

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Ghi lại mọi thay đổi dữ liệu ở mức **từng ô** vào hai bảng mới — một sổ kiểm toán và một chuỗi phát xuống có số thứ tự liên tục — mà không có đường ghi nào lọt lưới.

**Architecture:** Bảng `thay_doi` ghi mọi ý định sửa (sổ kiểm toán, không phát xuống). Bảng `hieu_luc` chỉ chứa thay đổi đã thắng cuộc gộp, mang `so_thu_tu` tăng dần theo từng giáo xứ, cấp bằng cách khoá một dòng đếm `FOR UPDATE` trong **chính** giao dịch ghi — nên thứ tự cấp số bằng thứ tự commit và máy con không bao giờ bỏ sót dòng. Ở kế hoạch này chưa có gộp (chưa có máy con), nên mọi thay đổi đều thắng và sinh cả hai loại dòng. Việc ghi nhật ký móc vào `SaveChanges` của `QlgxDbContext` cho đường ghi thông thường, và vào từng chỗ ghi hàng loạt cho 32 lời gọi `ExecuteUpdateAsync`/`ExecuteDeleteAsync` vốn đi vòng qua `SaveChanges`.

**Tech Stack:** .NET 10, EF Core + Npgsql, PostgreSQL 17, xUnit + FluentAssertions.

**Spec:** `docs/superpowers/specs/2026-09-13-dong-bo-offline-design.md` — mục 4.1, 4.2, 4.3, 4.7, 5.1, 6.5, và mục 12 "Đợt 1".

## Global Constraints

- **Nhánh:** `webapp-phase-1`. Có thể có phiên Claude khác làm việc song song trên cùng thư mục ở nhánh khác — kiểm tra `git branch --show-current` trước khi làm, **không `git checkout`**.
- **Thư mục làm việc:** `WebApp/`. Kế hoạch này **không đụng tới `Source/`** (bản desktop).
- **Bộ test cần PostgreSQL thật** qua biến môi trường `QLGX_TEST_PG`. Ví dụ: `Host=localhost;Username=postgres;Password=<mật khẩu>;Database=qlgx_thu`. **Không bao giờ ghi mật khẩu vào mã nguồn.**
- **Chạy test:** `dotnet test WebApp/tests/Qlgx.Data.Tests` và `dotnet test WebApp/tests/Qlgx.Api.Tests`.
- **Trước mỗi lần build lại**, nếu `Qlgx.Api` đang chạy thì phải dừng (nó khoá DLL). Tìm bằng `netstat -ano | grep ':5096'`.
- **Đặt tên bằng tiếng Việt không dấu** cho lớp, phương thức, biến — theo đúng lối đã có trong `WebApp/` (`ChuyenDoiDuLieu`, `LayCotThat`, `BoiCanhGiaoXu`).
- **Chú thích bằng tiếng Việt có dấu**, giải thích **vì sao** chứ không phải làm gì — theo mật độ đã có trong `QlgxDbContext.cs`.
- **Mọi bảng mới có `giao_xu_id` phải được thêm vào CẢ HAI:** `HasQueryFilter` trong `QlgxDbContext.OnModelCreating` **và** một migration bật RLS. Bỏ sót một trong hai là rò rỉ dữ liệu giữa các giáo xứ.
- **Theo điều 4 của `CLAUDE.md`:** với mỗi kiểm thử bảo vệ một ràng buộc sống còn, phải **chứng minh nó biết báo lỗi** — chạy thử với một phiên bản cố tình hỏng và xác nhận nó FAIL, trước khi viết bản đúng. Các bước dưới đây ghi rõ chỗ nào bắt buộc làm việc này.
- **Không bao giờ đưa `giaoxu.mdb` vào bất cứ đâu**, không gọi `unins000.exe`.

---

## Cấu trúc file

| File | Trách nhiệm |
|---|---|
| `src/Qlgx.Domain/Entities/ThayDoi.cs` | Thực thể sổ kiểm toán — mọi ý định sửa |
| `src/Qlgx.Domain/Entities/HieuLuc.cs` | Thực thể chuỗi phát xuống — thay đổi đã thắng, có `so_thu_tu` |
| `src/Qlgx.Domain/Entities/BoDemHieuLuc.cs` | Dòng đếm mỗi giáo xứ, giữ `SoTiepTheo` và `Epoch` |
| `src/Qlgx.Data/Configurations/ThayDoiConfig.cs` | Ánh xạ + chỉ mục cho `thay_doi` |
| `src/Qlgx.Data/Configurations/HieuLucConfig.cs` | Ánh xạ + chỉ mục duy nhất `(giao_xu_id, so_thu_tu)` |
| `src/Qlgx.Data/Configurations/BoDemHieuLucConfig.cs` | Ánh xạ, khoá chính `giao_xu_id` |
| `src/Qlgx.Data/NhatKy/CapSoHieuLuc.cs` | Khoá dòng đếm và cấp số — **chỉ một nơi duy nhất được cấp số** |
| `src/Qlgx.Data/NhatKy/CotLoaiTru.cs` | Danh sách cột không bao giờ vào nhật ký (ảnh `byte[]`, cột hệ thống) |
| `src/Qlgx.Data/NhatKy/SinhDongNhatKy.cs` | Đọc `ChangeTracker` sinh ra các dòng `thay_doi`/`hieu_luc` |
| `src/Qlgx.Data/NhatKy/IBoiCanhGhiNhatKy.cs` | Ai đang ghi (tài khoản, máy) — máy lấy sau, ở kế hoạch 3 |
| `src/Qlgx.Data/NhatKy/LuuCoNhatKy.cs` | Phương thức mở rộng gói giao dịch + cấp số + `SaveChanges` |
| `src/Qlgx.Data/QlgxDbContext.cs` | Thêm 3 `DbSet`, 2 `HasQueryFilter`, chặn `SaveChanges` trần |
| `src/Qlgx.Data/Migrations/*_ThemBangNhatKyThayDoi.cs` | Tạo bảng + RLS |
| `src/Qlgx.Api/Services/TimThayTheService.cs` | Ghi nhật ký cho thay thế hàng loạt |
| `src/Qlgx.Api/Services/ChuyenHoService.cs` | Ghi nhật ký cho chuyển giáo họ hàng loạt |
| `src/Qlgx.Api/Services/NhatKyService.cs` | Đọc nhật ký cho màn hình "Lịch sử thay đổi" |
| `src/Qlgx.Api/Endpoints/NhatKyEndpoints.cs` | `GET /api/nhat-ky` |
| `tests/Qlgx.Data.Tests/CapSoHieuLucTests.cs` | Liên tục, không lỗ hổng, đồng thời, thứ tự commit |
| `tests/Qlgx.Data.Tests/SinhDongNhatKyTests.cs` | Đúng ô, đúng giá trị, loại trừ ảnh |
| `tests/Qlgx.Data.Tests/MoiDuongGhiDeuGhiNhatKyTests.cs` | Kiểm thử kiến trúc — chặn đường ghi mới lọt lưới |
| `tests/Qlgx.Api.Tests/NhatKyGhiHangLoatTests.cs` | Thay thế hàng loạt và chuyển giáo họ có sinh nhật ký |
| `tests/Qlgx.Api.Tests/NhatKyDocTests.cs` | Endpoint đọc, lọc theo giáo xứ |

---

## Task 1: Dòng đếm và cấp số an toàn

**Files:**
- Create: `src/Qlgx.Domain/Entities/BoDemHieuLuc.cs`
- Create: `src/Qlgx.Data/Configurations/BoDemHieuLucConfig.cs`
- Create: `src/Qlgx.Data/NhatKy/CapSoHieuLuc.cs`
- Modify: `src/Qlgx.Data/QlgxDbContext.cs`
- Test: `tests/Qlgx.Data.Tests/CapSoHieuLucTests.cs`

**Interfaces:**
- Consumes: `QlgxDbContext`, `CoSoDuLieuFixture` (đã có).
- Produces: `BoDemHieuLuc { Guid GiaoXuId; long SoTiepTheo; Guid Epoch; }`; `CapSoHieuLuc.LayDaiSo(QlgxDbContext db, Guid giaoXuId, int soLuong, CancellationToken ct) -> Task<(long soDau, Guid epoch)>` — trả về số đầu của dải `soLuong` số liên tiếp đã được cấp.

- [ ] **Step 1: Viết thực thể và cấu hình**

`src/Qlgx.Domain/Entities/BoDemHieuLuc.cs`:

```csharp
namespace Qlgx.Domain.Entities;

/// <summary>
/// Dòng đếm cấp số thứ tự cho <see cref="HieuLuc"/>, mỗi giáo xứ một dòng.
///
/// KHÔNG dùng sequence của PostgreSQL: sequence cấp số TRƯỚC khi giao dịch commit, nên một
/// giao dịch bị huỷ để lại lỗ hổng vĩnh viễn — máy con nhận 4823, không bao giờ biết 4822
/// từng tồn tại, và bỏ sót dữ liệu mà không có dấu hiệu gì. Khoá dòng này bằng FOR UPDATE
/// thì khoá giữ tới lúc commit, nên thứ tự cấp số = thứ tự nhả khoá = thứ tự commit.
///
/// Epoch là danh tính của chuỗi số. Đổi khi khôi phục máy chủ hoặc dọn nhật ký, để con trỏ
/// cũ của máy con bị từ chối thẳng (410) thay vì âm thầm nhận rỗng mãi mãi.
/// </summary>
public class BoDemHieuLuc
{
    public Guid GiaoXuId { get; set; }
    public long SoTiepTheo { get; set; } = 1;
    public Guid Epoch { get; set; } = Guid.NewGuid();
}
```

`src/Qlgx.Data/Configurations/BoDemHieuLucConfig.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Qlgx.Domain.Entities;

namespace Qlgx.Data.Configurations;

public class BoDemHieuLucConfig : IEntityTypeConfiguration<BoDemHieuLuc>
{
    public void Configure(EntityTypeBuilder<BoDemHieuLuc> b)
    {
        b.HasKey(x => x.GiaoXuId);
        b.Property(x => x.SoTiepTheo).IsRequired();
        b.Property(x => x.Epoch).IsRequired();
    }
}
```

- [ ] **Step 2: Viết test cấp số — chạy để thấy nó fail vì chưa có lớp**

`tests/Qlgx.Data.Tests/CapSoHieuLucTests.cs`:

```csharp
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Qlgx.Data;
using Qlgx.Data.NhatKy;

namespace Qlgx.Data.Tests;

public class CapSoHieuLucTests(CoSoDuLieuFixture db) : IClassFixture<CoSoDuLieuFixture>
{
    [Fact]
    public async Task Cap_so_lien_tuc_tang_dan()
    {
        await using var ctx = db.TaoContext();

        var (dau1, epoch1) = await CapSoHieuLuc.LayDaiSo(ctx, db.GiaoXuId, 3, default);
        var (dau2, epoch2) = await CapSoHieuLuc.LayDaiSo(ctx, db.GiaoXuId, 2, default);

        dau2.Should().Be(dau1 + 3, "dai so phai lien tiep, khong chong lan va khong bo trong");
        epoch2.Should().Be(epoch1);
    }

    [Fact]
    public async Task Giao_dich_bi_huy_khong_de_lai_lo_hong()
    {
        long truoc;
        await using (var ctx = db.TaoContext())
        {
            truoc = (await ctx.Set<Domain.Entities.BoDemHieuLuc>()
                .SingleAsync(x => x.GiaoXuId == db.GiaoXuId)).SoTiepTheo;
        }

        await using (var ctx = db.TaoContext())
        await using (var gd = await ctx.Database.BeginTransactionAsync())
        {
            await CapSoHieuLuc.LayDaiSo(ctx, db.GiaoXuId, 5, default);
            await gd.RollbackAsync();
        }

        await using var sau = db.TaoContext();
        var (dau, _) = await CapSoHieuLuc.LayDaiSo(sau, db.GiaoXuId, 1, default);
        dau.Should().Be(truoc, "giao dich bi huy thi so KHONG duoc tieu ton");
    }
}
```

Run: `dotnet test WebApp/tests/Qlgx.Data.Tests --filter CapSoHieuLucTests`
Expected: FAIL — `CapSoHieuLuc` không tồn tại.

- [ ] **Step 3: Viết lớp cấp số**

`src/Qlgx.Data/NhatKy/CapSoHieuLuc.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using Qlgx.Domain.Entities;

namespace Qlgx.Data.NhatKy;

/// <summary>
/// NƠI DUY NHẤT được cấp số thứ tự cho bảng hieu_luc. Mọi chỗ khác phải gọi qua đây.
/// </summary>
public static class CapSoHieuLuc
{
    /// <summary>
    /// Khoá dòng đếm của giáo xứ rồi cấp một dải <paramref name="soLuong"/> số liên tiếp.
    ///
    /// BẮT BUỘC gọi trong CHÍNH giao dịch sẽ ghi hieu_luc, và phải là câu lệnh ĐẦU TIÊN của
    /// giao dịch đó. Hai lý do:
    ///  - Nếu mở một giao dịch riêng rồi commit để lấy số, giao dịch ghi bị huỷ sẽ để lại lỗ
    ///    hổng — đúng cái lỗi mà việc bỏ sequence sinh ra để tránh. Lỗi này CHỈ lộ khi có tải.
    ///  - Nếu khoá bản ghi nghiệp vụ trước rồi mới khoá dòng đếm, hai giao dịch ngược thứ tự
    ///    sẽ deadlock. Giao dịch chạm nhiều giáo xứ phải khoá theo GiaoXuId tăng dần.
    /// </summary>
    public static async Task<(long SoDau, Guid Epoch)> LayDaiSo(
        QlgxDbContext db, Guid giaoXuId, int soLuong, CancellationToken ct)
    {
        if (soLuong <= 0) throw new ArgumentOutOfRangeException(nameof(soLuong));

        // Tạo dòng đếm nếu chưa có. ON CONFLICT DO NOTHING để hai tiến trình cùng tạo không
        // đổ vỡ; dòng SELECT ... FOR UPDATE ngay dưới mới là chỗ giành quyền cấp số.
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO bo_dem_hieu_luc (giao_xu_id, so_tiep_theo, epoch)
            VALUES ({giaoXuId}, 1, gen_random_uuid())
            ON CONFLICT (giao_xu_id) DO NOTHING
            """, ct);

        var bd = await db.Set<BoDemHieuLuc>()
            .FromSql($"SELECT * FROM bo_dem_hieu_luc WHERE giao_xu_id = {giaoXuId} FOR UPDATE")
            .AsNoTracking()
            .SingleAsync(ct);

        var soDau = bd.SoTiepTheo;
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            UPDATE bo_dem_hieu_luc SET so_tiep_theo = so_tiep_theo + {(long)soLuong}
            WHERE giao_xu_id = {giaoXuId}
            """, ct);

        return (soDau, bd.Epoch);
    }
}
```

Thêm vào `QlgxDbContext` (sau `public DbSet<BoDemMa> BoDemMa => Set<BoDemMa>();`):

```csharp
    /// <summary>Dòng đếm cấp số thứ tự cho hieu_luc (xem BoDemHieuLuc.cs). CỐ Ý không có bộ
    /// lọc toàn cục: nó được đọc/ghi bằng SQL thô trong CapSoHieuLuc, luôn lọc tường minh theo
    /// giao_xu_id, và vẫn chịu RLS ở tầng CSDL.</summary>
    public DbSet<BoDemHieuLuc> BoDemHieuLuc => Set<BoDemHieuLuc>();
```

- [ ] **Step 4: Tạo migration và chạy test**

```bash
cd WebApp/src/Qlgx.Data
export QLGX_TEST_PG="Host=localhost;Username=postgres;Password=<mật khẩu>;Database=qlgx_thu"
dotnet ef migrations add ThemBoDemHieuLuc --startup-project ../Qlgx.Api
```

Run: `dotnet test WebApp/tests/Qlgx.Data.Tests --filter CapSoHieuLucTests`
Expected: PASS (2 test).

- [ ] **Step 5: Chứng minh test lỗ hổng biết báo lỗi**

Đây là ràng buộc sống còn, nên theo điều 4 của `CLAUDE.md` phải chứng minh test không phải lúc nào cũng "đạt".

Tạm sửa `LayDaiSo` sang bản cố tình hỏng — dùng sequence thay vì dòng đếm:

```csharp
        // BẢN CỐ TÌNH HỎNG — chỉ để chứng minh test biết báo lỗi, XOÁ ngay sau khi xác nhận
        var soDau = await db.Database.SqlQuery<long>(
            $"SELECT nextval('seq_thu_nghiem')::bigint").SingleAsync(ct);
        return (soDau, Guid.Empty);
```

Chạy trước đó: `CREATE SEQUENCE IF NOT EXISTS seq_thu_nghiem;` trong database test.

Run: `dotnet test WebApp/tests/Qlgx.Data.Tests --filter Giao_dich_bi_huy`
Expected: **FAIL** — sequence tiêu tốn số dù giao dịch bị huỷ.

Hoàn nguyên về bản đúng, chạy lại, xác nhận PASS.

- [ ] **Step 6: Viết test đồng thời**

Thêm vào `CapSoHieuLucTests.cs`:

```csharp
    [Fact]
    public async Task Nhieu_tien_trinh_cap_so_dong_thoi_khong_trung_khong_ho()
    {
        const int soLuong = 20;
        var ketQua = await Task.WhenAll(Enumerable.Range(0, soLuong).Select(async _ =>
        {
            await using var ctx = db.TaoContext();
            await using var gd = await ctx.Database.BeginTransactionAsync();
            var (dau, _) = await CapSoHieuLuc.LayDaiSo(ctx, db.GiaoXuId, 1, default);
            await gd.CommitAsync();
            return dau;
        }));

        ketQua.Distinct().Should().HaveCount(soLuong, "khong duoc cap trung so");
        (ketQua.Max() - ketQua.Min()).Should().Be(soLuong - 1, "khong duoc bo trong so nao");
    }
```

Run: `dotnet test WebApp/tests/Qlgx.Data.Tests --filter CapSoHieuLucTests`
Expected: PASS (3 test).

- [ ] **Step 7: Commit**

```bash
git add WebApp/src/Qlgx.Domain/Entities/BoDemHieuLuc.cs \
        WebApp/src/Qlgx.Data/Configurations/BoDemHieuLucConfig.cs \
        WebApp/src/Qlgx.Data/NhatKy/CapSoHieuLuc.cs \
        WebApp/src/Qlgx.Data/QlgxDbContext.cs \
        WebApp/src/Qlgx.Data/Migrations/ \
        WebApp/tests/Qlgx.Data.Tests/CapSoHieuLucTests.cs
git commit -m "Them dong dem va cap so thu tu lien tuc cho nhat ky thay doi"
```

---

## Task 2: Hai bảng nhật ký, bộ lọc giáo xứ và RLS

**Files:**
- Create: `src/Qlgx.Domain/Entities/ThayDoi.cs`, `src/Qlgx.Domain/Entities/HieuLuc.cs`
- Create: `src/Qlgx.Data/Configurations/ThayDoiConfig.cs`, `HieuLucConfig.cs`
- Modify: `src/Qlgx.Data/QlgxDbContext.cs`
- Create: `src/Qlgx.Data/Migrations/*_ThemBangNhatKyThayDoi.cs`
- Test: `tests/Qlgx.Data.Tests/LocTheoGiaoXuTests.cs` (đã có, sẽ tự bắt), `tests/Qlgx.Data.Tests/RlsTests.cs` (đã có)

**Interfaces:**
- Consumes: `BoDemHieuLuc`, `CapSoHieuLuc` từ Task 1.
- Produces: `ThayDoi`, `HieuLuc` với các thuộc tính dưới đây; `db.ThayDoi`, `db.HieuLuc`.

- [ ] **Step 1: Viết hai thực thể**

`src/Qlgx.Domain/Entities/ThayDoi.cs`:

```csharp
namespace Qlgx.Domain.Entities;

/// <summary>
/// Sổ kiểm toán — ghi MỌI ý định sửa, kể cả ý định thua cuộc gộp. KHÔNG phát xuống máy con.
///
/// Tách khỏi <see cref="HieuLuc"/> là điểm cốt lõi của thiết kế. Nếu dùng chung một bảng có
/// số thứ tự cho cả hai việc, thì khi một máy offline lâu gửi lên thay đổi cũ, máy chủ gộp
/// đúng luật nên không đổi giá trị — nhưng dòng đó vẫn vào chuỗi phát xuống, và mọi máy con
/// áp tuần tự sẽ hiển thị GIÁ TRỊ ĐÃ THUA, vĩnh viễn, không báo lỗi.
/// </summary>
public class ThayDoi
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid GiaoXuId { get; set; }

    public string Bang { get; set; } = "";
    public Guid BanGhiId { get; set; }
    /// <summary>Tên trường bị sửa. Rỗng với loại "tao".</summary>
    public string Truong { get; set; } = "";
    /// <summary>Giá trị mới, dạng JSON. Null nghĩa là ô được đặt về rỗng — một giá trị hợp lệ,
    /// không phải "không có gì".</summary>
    public string? GiaTri { get; set; }

    /// <summary>"tao" | "sua" | "gop".</summary>
    public string Loai { get; set; } = "sua";

    /// <summary>Phần vật lý của đồng hồ lai, đã hiệu chỉnh về giờ máy chủ.</summary>
    public DateTimeOffset DongHoVatLy { get; set; }
    /// <summary>Phần logic của đồng hồ lai — phá hoà khi hai thay đổi cùng mốc vật lý.</summary>
    public long DongHoLogic { get; set; }

    public Guid? ThietBiId { get; set; }
    public Guid? TaiKhoanId { get; set; }
    /// <summary>Mã do máy con sinh, dùng chống xử lý trùng khi gửi lại lô.</summary>
    public Guid MaThaoTac { get; set; }
    /// <summary>Gom các dòng của cùng một lần lưu — ranh giới lô không được cắt giữa.</summary>
    public Guid GiaoDichId { get; set; }

    /// <summary>Thay đổi này có thắng cuộc gộp không. False thì không có dòng HieuLuc tương ứng.</summary>
    public bool Thang { get; set; } = true;
}
```

`src/Qlgx.Domain/Entities/HieuLuc.cs`:

```csharp
namespace Qlgx.Domain.Entities;

/// <summary>
/// Chuỗi phát xuống — CHỈ những thay đổi đã thắng cuộc gộp, mang số thứ tự liên tục theo từng
/// giáo xứ. Đây là thứ duy nhất máy con kéo về.
/// </summary>
public class HieuLuc
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid GiaoXuId { get; set; }

    /// <summary>Tăng dần riêng theo từng giáo xứ. Cấp qua CapSoHieuLuc, không nơi nào khác.</summary>
    public long SoThuTu { get; set; }
    /// <summary>Danh tính của chuỗi số — đổi khi khôi phục hoặc dọn nhật ký.</summary>
    public Guid Epoch { get; set; }

    public string Bang { get; set; } = "";
    public Guid BanGhiId { get; set; }
    public string Truong { get; set; } = "";
    public string? GiaTri { get; set; }

    public DateTimeOffset DongHoVatLy { get; set; }
    public long DongHoLogic { get; set; }
    public Guid? ThietBiId { get; set; }

    public Guid GiaoDichId { get; set; }
}
```

- [ ] **Step 2: Viết cấu hình ánh xạ**

`src/Qlgx.Data/Configurations/ThayDoiConfig.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Qlgx.Domain.Entities;

namespace Qlgx.Data.Configurations;

public class ThayDoiConfig : IEntityTypeConfiguration<ThayDoi>
{
    public void Configure(EntityTypeBuilder<ThayDoi> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Bang).HasMaxLength(64).IsRequired();
        b.Property(x => x.Truong).HasMaxLength(64).IsRequired();
        b.Property(x => x.Loai).HasMaxLength(8).IsRequired();
        b.Property(x => x.GiaTri).HasColumnType("jsonb");

        // Màn hình "Lịch sử thay đổi" luôn hỏi theo một bản ghi cụ thể, mới nhất trước.
        b.HasIndex(x => new { x.GiaoXuId, x.Bang, x.BanGhiId, x.DongHoVatLy });
    }
}
```

`src/Qlgx.Data/Configurations/HieuLucConfig.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Qlgx.Domain.Entities;

namespace Qlgx.Data.Configurations;

public class HieuLucConfig : IEntityTypeConfiguration<HieuLuc>
{
    public void Configure(EntityTypeBuilder<HieuLuc> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Bang).HasMaxLength(64).IsRequired();
        b.Property(x => x.Truong).HasMaxLength(64).IsRequired();
        b.Property(x => x.GiaTri).HasColumnType("jsonb");

        // Chỉ mục DUY NHẤT: hai dòng cùng số thứ tự trong một giáo xứ nghĩa là cơ chế cấp số
        // đã hỏng, và hỏng theo kiểu máy con bỏ sót dữ liệu im lặng. Thà đổ vỡ lúc ghi.
        b.HasIndex(x => new { x.GiaoXuId, x.SoThuTu }).IsUnique();
    }
}
```

- [ ] **Step 3: Thêm DbSet và bộ lọc giáo xứ**

Trong `QlgxDbContext.cs`, thêm cạnh `BoDemHieuLuc`:

```csharp
    public DbSet<ThayDoi> ThayDoi => Set<ThayDoi>();
    public DbSet<HieuLuc> HieuLuc => Set<HieuLuc>();
```

Và trong `OnModelCreating`, thêm vào khối `HasQueryFilter`:

```csharp
        b.Entity<ThayDoi>().HasQueryFilter(x => BoiCanhGiaoXuId == null || x.GiaoXuId == BoiCanhGiaoXuId);
        b.Entity<HieuLuc>().HasQueryFilter(x => BoiCanhGiaoXuId == null || x.GiaoXuId == BoiCanhGiaoXuId);
```

- [ ] **Step 4: Chạy `LocTheoGiaoXuTests` để xác nhận không bỏ sót bộ lọc**

Bộ test đã có (`LocTheoGiaoXuTests.Moi_thuc_the_co_cot_GiaoXuId_deu_da_duoc_gan_bo_loc`) tự phát hiện thực thể mới có `GiaoXuId` mà chưa gắn bộ lọc.

Run: `dotnet test WebApp/tests/Qlgx.Data.Tests --filter LocTheoGiaoXu`
Expected: PASS. Nếu FAIL thì thiếu một dòng `HasQueryFilter` ở Step 3.

- [ ] **Step 5: Tạo migration và thêm RLS bằng tay**

```bash
cd WebApp/src/Qlgx.Data
dotnet ef migrations add ThemBangNhatKyThayDoi --startup-project ../Qlgx.Api
```

Mở file migration vừa sinh, thêm vào **cuối** `Up`:

```csharp
            // Lớp phòng thủ thứ hai, y hệt 24 bảng nghiệp vụ (xem BatRlsChoBangTheoGiaoXu).
            // Bỏ sót là nhật ký của giáo xứ này lộ sang giáo xứ khác — mà nhật ký chứa NGUYÊN
            // VĂN giá trị các ô, nên rò rỉ ở đây tương đương rò rỉ toàn bộ dữ liệu.
            foreach (var bang in new[] { "thay_doi", "hieu_luc", "bo_dem_hieu_luc" })
            {
                migrationBuilder.Sql($"ALTER TABLE {bang} ENABLE ROW LEVEL SECURITY;");
                migrationBuilder.Sql($"""
                    CREATE POLICY loc_theo_giao_xu ON {bang}
                        USING (giao_xu_id::text = current_setting('app.giao_xu_id', true))
                        WITH CHECK (giao_xu_id::text = current_setting('app.giao_xu_id', true));
                    """);
            }
```

Và vào **đầu** `Down`:

```csharp
            foreach (var bang in new[] { "thay_doi", "hieu_luc", "bo_dem_hieu_luc" })
            {
                migrationBuilder.Sql($"DROP POLICY IF EXISTS loc_theo_giao_xu ON {bang};");
                migrationBuilder.Sql($"ALTER TABLE {bang} DISABLE ROW LEVEL SECURITY;");
            }
```

- [ ] **Step 6: Viết test RLS cho bảng mới**

Thêm một `[Fact]` vào `tests/Qlgx.Data.Tests/RlsTests.cs`. Nhật ký chứa **nguyên văn giá trị các ô**, nên rò rỉ ở đây tương đương rò rỉ toàn bộ dữ liệu — phải kiểm chứng ở tầng CSDL bằng Npgsql thô, không qua EF, đúng lối hai test đã có trong file:

```csharp
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
            await ctx.SaveChangesAsync();
        }

        await using (var superuser = new NpgsqlConnection(_fixture.ChuoiKetNoi))
        {
            await superuser.OpenAsync();
            await using var taoVaiTro = new NpgsqlCommand(
                $"""
                CREATE ROLE "{_tenVaiTro}" LOGIN PASSWORD '{MatKhauVaiTro}' NOSUPERUSER NOBYPASSRLS;
                GRANT SELECT, INSERT, UPDATE, DELETE ON thay_doi TO "{_tenVaiTro}";
                """, superuser);
            await taoVaiTro.ExecuteNonQueryAsync();
        }

        var builder = new NpgsqlConnectionStringBuilder(_fixture.ChuoiKetNoi)
        {
            Username = _tenVaiTro,
            Password = MatKhauVaiTro,
        };

        async Task<List<string>> DocGiaTri(Guid? datThamSoPhien)
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
            await using var truyVan = new NpgsqlCommand("SELECT gia_tri::text FROM thay_doi", ketNoi);
            await using var reader = await truyVan.ExecuteReaderAsync();
            while (await reader.ReadAsync()) ketQua.Add(reader.GetString(0));
            return ketQua;
        }

        var choA = await DocGiaTri(giaoXuA);
        Assert.Single(choA);
        Assert.Contains("xu A", choA[0]);

        var choB = await DocGiaTri(giaoXuB);
        Assert.Single(choB);
        Assert.Contains("xu B", choB[0]);

        // Kết nối thô "quên" gọi set_config — đóng mặc định, không rò một dòng nào.
        Assert.Empty(await DocGiaTri(datThamSoPhien: null));
    }
```

**Lưu ý về dọn dẹp:** `DisposeAsync` của `RlsTests` gọi `DROP OWNED BY` nên vai trò đã được cấp quyền trên `thay_doi` vẫn xoá sạch, không phải sửa gì thêm.

Run: `dotnet test WebApp/tests/Qlgx.Data.Tests --filter Rls`
Expected: PASS (3 test).

- [ ] **Step 7: Commit**

```bash
git add WebApp/src/Qlgx.Domain/Entities/ThayDoi.cs \
        WebApp/src/Qlgx.Domain/Entities/HieuLuc.cs \
        WebApp/src/Qlgx.Data/Configurations/ \
        WebApp/src/Qlgx.Data/QlgxDbContext.cs \
        WebApp/src/Qlgx.Data/Migrations/ \
        WebApp/tests/Qlgx.Data.Tests/RlsTests.cs
git commit -m "Them bang thay_doi va hieu_luc kem bo loc giao xu va RLS"
```

---

## Task 3: Cột loại trừ và bộ sinh dòng nhật ký

**Files:**
- Create: `src/Qlgx.Data/NhatKy/CotLoaiTru.cs`
- Create: `src/Qlgx.Data/NhatKy/IBoiCanhGhiNhatKy.cs`
- Create: `src/Qlgx.Data/NhatKy/SinhDongNhatKy.cs`
- Test: `tests/Qlgx.Data.Tests/SinhDongNhatKyTests.cs`

**Interfaces:**
- Consumes: `ThayDoi`, `HieuLuc` từ Task 2.
- Produces: `IBoiCanhGhiNhatKy { Guid? TaiKhoanId; Guid? ThietBiId; }`; `SinhDongNhatKy.Tu(ChangeTracker ct, IBoiCanhGhiNhatKy? boiCanh, DateTimeOffset moc, Guid giaoDichId) -> List<ThayDoi>`.

- [ ] **Step 1: Viết danh sách cột loại trừ và bối cảnh**

`src/Qlgx.Data/NhatKy/CotLoaiTru.cs`:

```csharp
namespace Qlgx.Data.NhatKy;

/// <summary>
/// Cột không bao giờ vào nhật ký.
///
/// Ảnh đại diện là byte[] NẰM NGAY TRONG BẢNG (xem AnhDaiDienService.cs) — một tấm ảnh vào
/// jsonb dưới dạng base64 sẽ phình nhật ký lên gấp nhiều lần dữ liệu thật và làm mọi lượt kéo
/// về của máy con trở nên vô dụng. Ảnh đồng bộ riêng theo mã băm, không đi qua đây.
///
/// Các cột hệ thống bị loại vì chúng đổi ở MỌI lần ghi mà không mang ý nghĩa nghiệp vụ nào —
/// ghi lại chỉ tạo nhiễu và làm mọi lần lưu trông như có thay đổi.
/// </summary>
public static class CotLoaiTru
{
    public static readonly HashSet<string> Ten = new(StringComparer.Ordinal)
    {
        "AnhDaiDienDuLieu",
        "AnhDaiDienLoaiNoiDung",
        "RowVersion",
        "UpdatedAt",
        "CreatedAt",
    };

    public static bool BiLoai(string tenCot) => Ten.Contains(tenCot);
}
```

`src/Qlgx.Data/NhatKy/IBoiCanhGhiNhatKy.cs`:

```csharp
namespace Qlgx.Data.NhatKy;

/// <summary>
/// Ai đang ghi. ThietBiId còn null ở giai đoạn này — bảng thiet_bi thuộc kế hoạch sau; để sẵn
/// ở đây để khi có thiết bị thì không phải đổi chữ ký và không phải sửa lại migration.
/// </summary>
public interface IBoiCanhGhiNhatKy
{
    Guid? TaiKhoanId { get; }
    Guid? ThietBiId { get; }
}
```

- [ ] **Step 2: Viết test cho bộ sinh dòng — chạy để thấy fail**

`tests/Qlgx.Data.Tests/SinhDongNhatKyTests.cs`:

```csharp
using FluentAssertions;
using Qlgx.Data.NhatKy;
using Qlgx.Domain.Entities;

namespace Qlgx.Data.Tests;

public class SinhDongNhatKyTests(CoSoDuLieuFixture db) : IClassFixture<CoSoDuLieuFixture>
{
    [Fact]
    public async Task Them_ban_ghi_sinh_dung_mot_dong_loai_tao()
    {
        await using var ctx = db.TaoContext();
        ctx.GiaoDan.Add(new GiaoDan { GiaoXuId = db.GiaoXuId, MaGiaoDanCu = 9101, HoTen = "Nguyen Van A" });

        var dong = SinhDongNhatKy.Tu(ctx.ChangeTracker, null, DateTimeOffset.UtcNow, Guid.NewGuid());

        dong.Should().ContainSingle();
        dong[0].Loai.Should().Be("tao");
        dong[0].Bang.Should().Be("GiaoDan");
        dong[0].Truong.Should().BeEmpty();
        dong[0].GiaTri.Should().Contain("Nguyen Van A");
    }

    [Fact]
    public async Task Sua_hai_o_sinh_dung_hai_dong_moi_dong_mot_o()
    {
        Guid id;
        await using (var ctx = db.TaoContext())
        {
            var g = new GiaoDan { GiaoXuId = db.GiaoXuId, MaGiaoDanCu = 9102, HoTen = "Cu" };
            ctx.GiaoDan.Add(g);
            await ctx.SaveChangesAsync();
            id = g.Id;
        }

        await using var ctx2 = db.TaoContext();
        var gd = await ctx2.GiaoDan.FindAsync(id);
        gd!.HoTen = "Moi";
        gd.DienThoai = "0900000000";

        var dong = SinhDongNhatKy.Tu(ctx2.ChangeTracker, null, DateTimeOffset.UtcNow, Guid.NewGuid());

        dong.Should().HaveCount(2);
        dong.Select(d => d.Truong).Should().BeEquivalentTo(["HoTen", "DienThoai"]);
        dong.Should().OnlyContain(d => d.Loai == "sua" && d.BanGhiId == id);
    }

    [Fact]
    public async Task Anh_dai_dien_khong_bao_gio_vao_nhat_ky()
    {
        Guid id;
        await using (var ctx = db.TaoContext())
        {
            var g = new GiaoDan { GiaoXuId = db.GiaoXuId, MaGiaoDanCu = 9103, HoTen = "Co anh" };
            ctx.GiaoDan.Add(g);
            await ctx.SaveChangesAsync();
            id = g.Id;
        }

        await using var ctx2 = db.TaoContext();
        var gd = await ctx2.GiaoDan.FindAsync(id);
        gd!.AnhDaiDienDuLieu = new byte[4096];
        gd.AnhDaiDienLoaiNoiDung = "image/png";

        var dong = SinhDongNhatKy.Tu(ctx2.ChangeTracker, null, DateTimeOffset.UtcNow, Guid.NewGuid());

        dong.Should().BeEmpty("anh la byte[] trong bang, dua vao jsonb se phinh nhat ky");
    }

    [Fact]
    public async Task Dat_o_ve_rong_van_sinh_dong_voi_gia_tri_null()
    {
        Guid id;
        await using (var ctx = db.TaoContext())
        {
            var g = new GiaoDan
            {
                GiaoXuId = db.GiaoXuId, MaGiaoDanCu = 9104, HoTen = "Co dien thoai",
                DienThoai = "0911111111",
            };
            ctx.GiaoDan.Add(g);
            await ctx.SaveChangesAsync();
            id = g.Id;
        }

        await using var ctx2 = db.TaoContext();
        var gd = await ctx2.GiaoDan.FindAsync(id);
        gd!.DienThoai = null;

        var dong = SinhDongNhatKy.Tu(ctx2.ChangeTracker, null, DateTimeOffset.UtcNow, Guid.NewGuid());

        dong.Should().ContainSingle();
        dong[0].Truong.Should().Be("DienThoai");
        dong[0].GiaTri.Should().BeNull(
            "o rong la MOT GIA TRI hop le, khong phai 'khong co gi' — thieu dong nay thi gia " +
            "tri nhap nham khong bao gio xoa duoc, no se song lai o lan gop sau");
    }
}
```

Run: `dotnet test WebApp/tests/Qlgx.Data.Tests --filter SinhDongNhatKyTests`
Expected: FAIL — `SinhDongNhatKy` không tồn tại.

- [ ] **Step 3: Viết bộ sinh dòng**

`src/Qlgx.Data/NhatKy/SinhDongNhatKy.cs`:

```csharp
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Qlgx.Domain.Entities;

namespace Qlgx.Data.NhatKy;

/// <summary>
/// Đọc ChangeTracker sinh ra các dòng thay_doi. Ghi ở mức TỪNG Ô (trừ khi thêm mới) — đây là
/// điều làm nên khác biệt: hai người sửa hai ô khác nhau của cùng một hồ sơ sẽ gộp được tự
/// động, thay vì một người bị chặn như cơ chế xmin hiện tại.
/// </summary>
public static class SinhDongNhatKy
{
    public static List<ThayDoi> Tu(
        ChangeTracker theoDoi, IBoiCanhGhiNhatKy? boiCanh, DateTimeOffset moc, Guid giaoDichId)
    {
        var ketQua = new List<ThayDoi>();

        foreach (var muc in theoDoi.Entries<ThucTheCoSo>())
        {
            if (muc.State is not (EntityState.Added or EntityState.Modified)) continue;

            var bang = muc.Metadata.ClrType.Name;
            var thucThe = muc.Entity;

            if (muc.State == EntityState.Added)
            {
                // Ghi cả bản ghi trong MỘT dòng. Máy chủ khai triển nó thành từng ô ngay khi
                // nhận (xem thiết kế mục 4.1) — không bao giờ áp nguyên khối, vì áp nguyên
                // khối sẽ đè lên các ô đã gộp riêng.
                var cacO = muc.Properties
                    .Where(p => !CotLoaiTru.BiLoai(p.Metadata.Name))
                    .ToDictionary(p => p.Metadata.Name, p => p.CurrentValue);

                ketQua.Add(TaoDong(thucThe, bang, "", JsonSerializer.Serialize(cacO),
                    "tao", boiCanh, moc, giaoDichId));
                continue;
            }

            foreach (var o in muc.Properties)
            {
                if (!o.IsModified) continue;
                if (CotLoaiTru.BiLoai(o.Metadata.Name)) continue;
                if (Equals(o.OriginalValue, o.CurrentValue)) continue;

                ketQua.Add(TaoDong(thucThe, bang, o.Metadata.Name,
                    o.CurrentValue is null ? null : JsonSerializer.Serialize(o.CurrentValue),
                    "sua", boiCanh, moc, giaoDichId));
            }
        }

        return ketQua;
    }

    private static ThayDoi TaoDong(
        ThucTheCoSo thucThe, string bang, string truong, string? giaTri, string loai,
        IBoiCanhGhiNhatKy? boiCanh, DateTimeOffset moc, Guid giaoDichId) => new()
    {
        GiaoXuId = thucThe.GiaoXuId,
        Bang = bang,
        BanGhiId = thucThe.Id,
        Truong = truong,
        GiaTri = giaTri,
        Loai = loai,
        DongHoVatLy = moc,
        DongHoLogic = 0,
        TaiKhoanId = boiCanh?.TaiKhoanId,
        ThietBiId = boiCanh?.ThietBiId,
        MaThaoTac = Guid.NewGuid(),
        GiaoDichId = giaoDichId,
        Thang = true,
    };
}
```

- [ ] **Step 4: Chạy test**

Run: `dotnet test WebApp/tests/Qlgx.Data.Tests --filter SinhDongNhatKyTests`
Expected: PASS (4 test).

- [ ] **Step 5: Commit**

```bash
git add WebApp/src/Qlgx.Data/NhatKy/ WebApp/tests/Qlgx.Data.Tests/SinhDongNhatKyTests.cs
git commit -m "Them bo sinh dong nhat ky muc truong va danh sach cot loai tru"
```

---

## Task 3b: Khoá chính `Guid` cho hai bảng nối, để chúng vào được nhật ký

**Bối cảnh — vì sao task này được chèn thêm sau khi kế hoạch đã viết:** review Task 3 phát hiện
`ThanhVienGiaDinh` và `GiaoDanHonPhoi` **không kế thừa `ThucTheCoSo`** nên bộ sinh dòng nhật ký
(duyệt `ChangeTracker.Entries<ThucTheCoSo>()`) không bao giờ chạm tới chúng.

Kịch bản hỏng: sơ ở máy con offline chuyển một giáo dân sang gia đình khác và đặt lại chủ hộ. Thao
tác đó chỉ sinh `INSERT`/`DELETE` trên `thanh_vien_gia_dinh` — **không dòng nhật ký nào**. Khi máy
đó đồng bộ, máy chủ và các máy khác vẫn thấy giáo dân ở gia đình cũ, **không báo lỗi gì**. Sổ gia
đình phân kỳ vĩnh viễn giữa các máy.

Thiết kế (mục 4.4) đã lường trước bảng khoá phức nhưng đề xuất "dẫn xuất `ban_ghi_id` tất định từ
khoá phức". **Không dùng cách đó:** dẫn xuất là một chiều — khi áp một dòng `sua`/`xoa`, không
khôi phục được nó trỏ tới hai bản ghi nào. Thay bằng khoá chính `Guid` thật.

**Files:**
- Modify: `src/Qlgx.Domain/Entities/ThanhVienGiaDinh.cs`, `GiaoDanHonPhoi.cs`
- Modify: `src/Qlgx.Data/Configurations/ThanhVienGiaDinhConfig.cs`, `GiaoDanHonPhoiConfig.cs`
- Create: `src/Qlgx.Data/Migrations/*_ThemKhoaChinhChoBangNoi.cs`
- Modify: danh sách phân loại nhật ký trong `src/Qlgx.Data/NhatKy/`
- Test: `tests/Qlgx.Data.Tests/NhatKyBangNoiTests.cs`

**Interfaces:**
- Consumes: danh sách phân loại và `SinhDongNhatKy.Tu` từ Task 3.
- Produces: `ThanhVienGiaDinh` và `GiaoDanHonPhoi` kế thừa `ThucTheCoSo` (có `Id`, `GiaoXuId`,
  `CreatedAt`, `UpdatedAt`, `RowVersion`), khoá phức cũ trở thành **chỉ mục duy nhất**.

- [ ] **Step 1: Viết test — chạy để thấy fail**

`tests/Qlgx.Data.Tests/NhatKyBangNoiTests.cs`:

```csharp
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Qlgx.Data.NhatKy;
using Qlgx.Domain.Entities;

namespace Qlgx.Data.Tests;

public class NhatKyBangNoiTests(CoSoDuLieuFixture db) : IClassFixture<CoSoDuLieuFixture>
{
    [Fact]
    public async Task Them_thanh_vien_vao_gia_dinh_sinh_dong_nhat_ky()
    {
        Guid giaDinhId, giaoDanId;
        await using (var ctx = db.TaoContext())
        {
            var gd = new GiaDinh { GiaoXuId = db.GiaoXuId, MaGiaDinhCu = 9601, TenGiaDinh = "Ho Nguyen" };
            var nguoi = new GiaoDan { GiaoXuId = db.GiaoXuId, MaGiaoDanCu = 9601, HoTen = "Nguyen Van A" };
            ctx.GiaDinh.Add(gd);
            ctx.GiaoDan.Add(nguoi);
            await ctx.SaveChangesAsync();
            giaDinhId = gd.Id;
            giaoDanId = nguoi.Id;
        }

        await using var ctx2 = db.TaoContext();
        ctx2.ThanhVienGiaDinh.Add(new ThanhVienGiaDinh
        {
            GiaoXuId = db.GiaoXuId, GiaDinhId = giaDinhId, GiaoDanId = giaoDanId, ChuHo = true,
        });

        var dong = SinhDongNhatKy.Tu(ctx2.ChangeTracker, null, DateTimeOffset.UtcNow, Guid.NewGuid());

        dong.Should().ContainSingle(d => d.Bang == "ThanhVienGiaDinh",
            "chuyen gia dinh ma khong sinh nhat ky thi so gia dinh phan ky vinh vien giua cac may");
        dong.Single(d => d.Bang == "ThanhVienGiaDinh").Loai.Should().Be("tao");
    }

    [Fact]
    public async Task Khoa_phuc_cu_van_con_la_rang_buoc_duy_nhat()
    {
        Guid giaDinhId, giaoDanId;
        await using (var ctx = db.TaoContext())
        {
            var gd = new GiaDinh { GiaoXuId = db.GiaoXuId, MaGiaDinhCu = 9602, TenGiaDinh = "Ho Tran" };
            var nguoi = new GiaoDan { GiaoXuId = db.GiaoXuId, MaGiaoDanCu = 9602, HoTen = "Tran Thi B" };
            ctx.GiaDinh.Add(gd);
            ctx.GiaoDan.Add(nguoi);
            await ctx.SaveChangesAsync();
            giaDinhId = gd.Id;
            giaoDanId = nguoi.Id;

            ctx.ThanhVienGiaDinh.Add(new ThanhVienGiaDinh
            {
                GiaoXuId = db.GiaoXuId, GiaDinhId = giaDinhId, GiaoDanId = giaoDanId,
            });
            await ctx.SaveChangesAsync();
        }

        await using var ctx3 = db.TaoContext();
        ctx3.ThanhVienGiaDinh.Add(new ThanhVienGiaDinh
        {
            GiaoXuId = db.GiaoXuId, GiaDinhId = giaDinhId, GiaoDanId = giaoDanId,
        });

        var hanhDong = async () => await ctx3.SaveChangesAsync();
        await hanhDong.Should().ThrowAsync<DbUpdateException>(
            "them khoa chinh Guid khong duoc lam mat rang buoc mot nguoi chi thuoc mot gia dinh mot lan");
    }
}
```

Run: `dotnet test WebApp/tests/Qlgx.Data.Tests --filter NhatKyBangNoiTests`
Expected: FAIL — `ThanhVienGiaDinh` chưa có `Id`, chưa vào nhật ký.

- [ ] **Step 2: Cho hai thực thể kế thừa `ThucTheCoSo`**

Trong `ThanhVienGiaDinh.cs` và `GiaoDanHonPhoi.cs`, đổi khai báo lớp thành kế thừa `ThucTheCoSo`
và **xoá thuộc tính `GiaoXuId` đang khai báo riêng** (lớp cơ sở đã có). Giữ nguyên mọi thuộc tính
nghiệp vụ khác.

- [ ] **Step 3: Đổi cấu hình — khoá chính `Id`, khoá phức cũ thành chỉ mục duy nhất**

Trong `ThanhVienGiaDinhConfig.cs`:

```csharp
        b.HasKey(x => x.Id);

        // Khoá phức cũ trở thành ràng buộc duy nhất, KHÔNG được bỏ: nó là thứ giữ cho một giáo
        // dân không bị ghi hai lần vào cùng một gia đình. Thêm khoá chính Guid chỉ để nhật ký
        // đánh địa chỉ được từng dòng — không phải để nới lỏng ràng buộc nghiệp vụ.
        b.HasIndex(x => new { x.GiaDinhId, x.GiaoDanId }).IsUnique();
```

Trong `GiaoDanHonPhoiConfig.cs`, tương tự với `new { x.GiaoDanId, x.HonPhoiId }`.

- [ ] **Step 4: Migration có backfill**

```bash
cd WebApp/src/Qlgx.Data
dotnet ef migrations add ThemKhoaChinhChoBangNoi --startup-project ../Qlgx.Api
```

Mở file sinh ra và **kiểm tra kỹ thứ tự**: cột `id` phải được thêm và **điền giá trị cho mọi dòng
sẵn có** trước khi đặt làm khoá chính. EF thường sinh `AddColumn` với giá trị mặc định
`Guid.Empty` — điều đó làm mọi dòng cũ trùng khoá. Sửa tay thành:

```csharp
            migrationBuilder.Sql("ALTER TABLE thanh_vien_gia_dinh ADD COLUMN id uuid;");
            migrationBuilder.Sql("UPDATE thanh_vien_gia_dinh SET id = gen_random_uuid();");
            migrationBuilder.Sql("ALTER TABLE thanh_vien_gia_dinh ALTER COLUMN id SET NOT NULL;");
```

rồi mới bỏ khoá chính cũ và đặt khoá chính mới. Làm tương tự cho `giao_dan_hon_phoi`. Hai bảng này
cũng cần các cột của `ThucTheCoSo` (`created_at`, `updated_at`, `source_system`, `du_lieu_loi`) —
kiểm tra migration có thêm đủ, và `created_at`/`updated_at` phải có giá trị cho dòng cũ
(`SET created_at = now()` thay vì để NULL).

- [ ] **Step 5: Đưa hai bảng vào nhóm được ghi nhật ký**

Thêm `ThanhVienGiaDinh` và `GiaoDanHonPhoi` vào danh sách phân loại "được ghi nhật ký" tạo ở Task 3.
Test bắt buộc phân loại của Task 3 sẽ đỏ cho tới khi làm việc này.

- [ ] **Step 6: Chạy test**

Run: `dotnet test WebApp/tests/Qlgx.Data.Tests`
Expected: PASS toàn bộ, kể cả `SinhDongNhatKyTests` và test phân loại của Task 3.

Rồi: `dotnet test WebApp/tests/Qlgx.Api.Tests`
Expected: PASS. Hai bảng này được dùng nhiều ở `GiaDinhService` và `GiaoDanService`; đổi khoá chính
có thể làm lộ chỗ nào đó dựa vào khoá phức. Sửa những chỗ đó cho khớp.

- [ ] **Step 7: Commit**

```bash
git add WebApp/src/Qlgx.Domain/Entities/ThanhVienGiaDinh.cs \
        WebApp/src/Qlgx.Domain/Entities/GiaoDanHonPhoi.cs \
        WebApp/src/Qlgx.Data/Configurations/ThanhVienGiaDinhConfig.cs \
        WebApp/src/Qlgx.Data/Configurations/GiaoDanHonPhoiConfig.cs \
        WebApp/src/Qlgx.Data/Migrations/ \
        WebApp/src/Qlgx.Data/NhatKy/ \
        WebApp/tests/Qlgx.Data.Tests/NhatKyBangNoiTests.cs
git commit -m "Them khoa chinh Guid cho hai bang noi de chung vao duoc nhat ky"
```

---

## Task 4: Nối vào `SaveChanges` bằng giao dịch tường minh

**Files:**
- Create: `src/Qlgx.Data/NhatKy/LuuCoNhatKy.cs`
- Modify: `src/Qlgx.Data/QlgxDbContext.cs`
- Test: `tests/Qlgx.Data.Tests/LuuCoNhatKyTests.cs`

**Interfaces:**
- Consumes: `CapSoHieuLuc.LayDaiSo`, `SinhDongNhatKy.Tu`.
- Produces: `QlgxDbContextNhatKyExtensions.LuuCoNhatKy(this QlgxDbContext db, CancellationToken ct) -> Task<int>` — thay cho `SaveChangesAsync` ở mọi đường ghi nghiệp vụ.

- [ ] **Step 1: Viết test — chạy để thấy fail**

`tests/Qlgx.Data.Tests/LuuCoNhatKyTests.cs`:

```csharp
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Qlgx.Data.NhatKy;
using Qlgx.Domain.Entities;

namespace Qlgx.Data.Tests;

public class LuuCoNhatKyTests(CoSoDuLieuFixture db) : IClassFixture<CoSoDuLieuFixture>
{
    [Fact]
    public async Task Luu_sinh_ca_thay_doi_lan_hieu_luc()
    {
        await using var ctx = db.TaoContext();
        var g = new GiaoDan { GiaoXuId = db.GiaoXuId, MaGiaoDanCu = 9201, HoTen = "Nguoi moi" };
        ctx.GiaoDan.Add(g);

        await ctx.LuuCoNhatKy(default);

        await using var doc = db.TaoContext();
        (await doc.ThayDoi.CountAsync(x => x.BanGhiId == g.Id)).Should().Be(1);
        (await doc.HieuLuc.CountAsync(x => x.BanGhiId == g.Id)).Should().Be(1);
    }

    [Fact]
    public async Task Moi_dong_hieu_luc_cua_mot_lan_luu_dung_chung_mot_giao_dich_id()
    {
        await using var ctx = db.TaoContext();
        var g = await ctx.GiaoDan.FirstAsync(x => x.MaGiaoDanCu == 9201);
        g.HoTen = "Doi ten";
        g.DienThoai = "0922222222";

        await ctx.LuuCoNhatKy(default);

        await using var doc = db.TaoContext();
        var dong = await doc.HieuLuc.Where(x => x.BanGhiId == g.Id && x.Truong != "").ToListAsync();
        dong.Should().HaveCountGreaterThanOrEqualTo(2);
        dong.Select(x => x.GiaoDichId).Distinct().Should().HaveCount(1,
            "ranh gioi lo khong duoc cat giua mot lan luu");
    }

    [Fact]
    public async Task Ghi_that_bai_thi_khong_de_lai_dong_nhat_ky_mo_coi()
    {
        long soTruoc;
        await using (var doc = db.TaoContext())
            soTruoc = await doc.HieuLuc.CountAsync();

        await using var ctx = db.TaoContext();
        // MaGiaoDanCu trùng trong cùng giáo xứ vi phạm chỉ mục duy nhất -> SaveChanges ném.
        ctx.GiaoDan.Add(new GiaoDan { GiaoXuId = db.GiaoXuId, MaGiaoDanCu = 9201, HoTen = "Trung ma" });

        var hanhDong = async () => await ctx.LuuCoNhatKy(default);
        await hanhDong.Should().ThrowAsync<DbUpdateException>();

        await using var sau = db.TaoContext();
        (await sau.HieuLuc.CountAsync()).Should().Be(soTruoc,
            "nhat ky va du lieu phai cung commit hoac cung huy");
    }
}
```

Run: `dotnet test WebApp/tests/Qlgx.Data.Tests --filter LuuCoNhatKyTests`
Expected: FAIL — `LuuCoNhatKy` không tồn tại.

- [ ] **Step 2: Viết phương thức mở rộng**

`src/Qlgx.Data/NhatKy/LuuCoNhatKy.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using Qlgx.Domain.Entities;

namespace Qlgx.Data.NhatKy;

public static class QlgxDbContextNhatKyExtensions
{
    /// <summary>
    /// Thay cho SaveChangesAsync ở MỌI đường ghi nghiệp vụ. Mở một giao dịch tường minh nếu
    /// chưa có, khoá dòng đếm, ghi nhật ký, rồi lưu — tất cả trong cùng một giao dịch.
    ///
    /// Phải là giao dịch TƯỜNG MINH: nếu cấp số ở một giao dịch riêng rồi commit, giao dịch
    /// ghi bị huỷ sẽ để lại lỗ hổng trong chuỗi số, và máy con sẽ bỏ sót dữ liệu im lặng.
    ///
    /// Khoá dòng đếm là câu lệnh ĐẦU TIÊN — xem CapSoHieuLuc để biết vì sao thứ tự khoá quan
    /// trọng. Giao dịch chạm nhiều giáo xứ khoá theo GiaoXuId tăng dần.
    /// </summary>
    public static async Task<int> LuuCoNhatKy(
        this QlgxDbContext db, CancellationToken ct, IBoiCanhGhiNhatKy? boiCanh = null)
    {
        var giaoDichId = Guid.NewGuid();
        var moc = DateTimeOffset.UtcNow;

        var dong = SinhDongNhatKy.Tu(db.ChangeTracker, boiCanh, moc, giaoDichId);
        if (dong.Count == 0) return await db.SaveChangesAsync(ct);

        var daCoGiaoDich = db.Database.CurrentTransaction is not null;
        var giaoDich = daCoGiaoDich ? null : await db.Database.BeginTransactionAsync(ct);
        try
        {
            foreach (var nhom in dong.GroupBy(x => x.GiaoXuId).OrderBy(x => x.Key))
            {
                var cacDong = nhom.ToList();
                var (soDau, epoch) = await CapSoHieuLuc.LayDaiSo(db, nhom.Key, cacDong.Count, ct);

                db.ThayDoi.AddRange(cacDong);
                for (var i = 0; i < cacDong.Count; i++)
                {
                    var t = cacDong[i];
                    db.HieuLuc.Add(new HieuLuc
                    {
                        GiaoXuId = t.GiaoXuId,
                        SoThuTu = soDau + i,
                        Epoch = epoch,
                        Bang = t.Bang,
                        BanGhiId = t.BanGhiId,
                        Truong = t.Truong,
                        GiaTri = t.GiaTri,
                        DongHoVatLy = t.DongHoVatLy,
                        DongHoLogic = t.DongHoLogic,
                        ThietBiId = t.ThietBiId,
                        GiaoDichId = t.GiaoDichId,
                    });
                }
            }

            var soDong = await db.SaveChangesAsync(ct);
            if (giaoDich is not null) await giaoDich.CommitAsync(ct);
            return soDong;
        }
        catch
        {
            if (giaoDich is not null) await giaoDich.RollbackAsync(ct);
            throw;
        }
        finally
        {
            if (giaoDich is not null) await giaoDich.DisposeAsync();
        }
    }
}
```

- [ ] **Step 3: Chạy test**

Run: `dotnet test WebApp/tests/Qlgx.Data.Tests --filter LuuCoNhatKyTests`
Expected: PASS (3 test).

- [ ] **Step 4: Đổi mọi đường ghi nghiệp vụ sang `LuuCoNhatKy`**

Tìm mọi chỗ gọi `SaveChangesAsync` trong `src/Qlgx.Api/Services/`:

```bash
grep -rn "SaveChangesAsync" WebApp/src/Qlgx.Api/Services/ | wc -l
```

Đổi từng chỗ sang `LuuCoNhatKy(ct)`. **Ngoại lệ giữ nguyên `SaveChangesAsync`:** `NhapDuLieuService` và `Qlgx.Migration.Core` (nhập dữ liệu Access sinh hàng chục nghìn bản ghi; nhật ký cho đường này thuộc kế hoạch nhập-từ-desktop), và `AuthService` khi chỉ cập nhật thời điểm đăng nhập.

Sau mỗi nhóm 5 file, chạy: `dotnet test WebApp/tests/Qlgx.Api.Tests`
Expected: PASS toàn bộ. Nếu một test đỏ vì đếm số dòng, đó là dấu hiệu chỗ đó cần ngoại lệ — ghi chú lý do vào code.

- [ ] **Step 5: Chặn `SaveChangesAsync` trần bằng kiểm thử kiến trúc**

`tests/Qlgx.Data.Tests/MoiDuongGhiDeuGhiNhatKyTests.cs`:

```csharp
using FluentAssertions;

namespace Qlgx.Data.Tests;

/// <summary>
/// Kiểm thử kiến trúc: bắt đường ghi MỚI lọt lưới nhật ký. Không có nó, một người sửa mã sáu
/// tháng sau sẽ thêm một SaveChangesAsync và nhật ký thủng mà không ai biết — đúng loại lỗi
/// âm thầm mà phần mềm giữ sổ sách giáo xứ không chịu được.
/// </summary>
public class MoiDuongGhiDeuGhiNhatKyTests
{
    private static readonly string[] DuocPhepGoiThang =
    {
        "NhapDuLieuService.cs",   // nhập Access, nhật ký thuộc kế hoạch riêng
        "ChuyenDoiDuLieu.cs",     // cùng lý do
        "AuthService.cs",         // chỉ cập nhật thời điểm đăng nhập
    };

    [Fact]
    public void Khong_service_nao_goi_thang_SaveChangesAsync()
    {
        var goc = TimThuMucGoc();
        var viPham = Directory
            .EnumerateFiles(Path.Combine(goc, "src", "Qlgx.Api", "Services"), "*.cs")
            .Where(f => !DuocPhepGoiThang.Contains(Path.GetFileName(f)))
            .Where(f => File.ReadAllText(f).Contains("SaveChangesAsync("))
            .Select(Path.GetFileName)
            .ToList();

        viPham.Should().BeEmpty(
            "moi duong ghi nghiep vu phai goi LuuCoNhatKy de sinh nhat ky; neu that su can " +
            "goi thang thi them vao DuocPhepGoiThang kem ly do");
    }

    [Fact]
    public void Khong_service_nao_goi_ExecuteUpdate_hay_ExecuteDelete_ngoai_danh_sach_da_xu_ly()
    {
        var daXuLy = new[] { "TimThayTheService.cs", "ChuyenHoService.cs", "AuthService.cs" };
        var goc = TimThuMucGoc();
        var viPham = Directory
            .EnumerateFiles(Path.Combine(goc, "src", "Qlgx.Api", "Services"), "*.cs")
            .Where(f => !daXuLy.Contains(Path.GetFileName(f)))
            .Where(f => File.ReadAllText(f) is var noi
                && (noi.Contains("ExecuteUpdateAsync") || noi.Contains("ExecuteDeleteAsync")))
            .Select(Path.GetFileName)
            .ToList();

        viPham.Should().BeEmpty(
            "ExecuteUpdate/ExecuteDelete di VONG QUA SaveChanges nen khong sinh nhat ky; " +
            "moi cho dung chung phai tu ghi nhat ky va duoc liet ke o day");
    }

    private static string TimThuMucGoc()
    {
        var d = new DirectoryInfo(AppContext.BaseDirectory);
        while (d is not null && !File.Exists(Path.Combine(d.FullName, "Qlgx.sln"))) d = d.Parent;
        return d?.FullName ?? throw new InvalidOperationException("Khong tim thay thu muc goc WebApp");
    }
}
```

Run: `dotnet test WebApp/tests/Qlgx.Data.Tests --filter MoiDuongGhi`
Expected: test thứ hai **FAIL**, liệt kê các file còn dùng `ExecuteUpdate`/`ExecuteDelete` — đó chính là danh sách việc cho Task 5 và 6. Ghi lại danh sách đó.

Đánh dấu test thứ hai là `[Fact(Skip = "Mo lai o Task 6 Step 4")]` để commit được Task 4, và **bắt buộc bỏ `Skip` ở Task 6 Step 4**. Không thêm file nào vào `daXuLy` lúc này — danh sách đó chỉ ghi những file đã thật sự tự ghi nhật ký, thêm sớm là che mất chính việc còn phải làm.

- [ ] **Step 6: Commit**

```bash
git add WebApp/src/Qlgx.Data/NhatKy/LuuCoNhatKy.cs \
        WebApp/src/Qlgx.Api/Services/ \
        WebApp/tests/Qlgx.Data.Tests/
git commit -m "Noi ghi nhat ky vao moi duong ghi nghiep vu qua LuuCoNhatKy"
```

---

## Task 5: Ghi nhật ký cho thay thế hàng loạt

**Files:**
- Modify: `src/Qlgx.Api/Services/TimThayTheService.cs`
- Test: `tests/Qlgx.Api.Tests/NhatKyGhiHangLoatTests.cs`

**Interfaces:**
- Consumes: `LuuCoNhatKy` từ Task 4.
- Produces: không có API mới — `TimThayTheService.ThayThe` giữ nguyên chữ ký, chỉ đổi cách thực hiện.

- [ ] **Step 1: Viết test — chạy để thấy fail**

`tests/Qlgx.Api.Tests/NhatKyGhiHangLoatTests.cs`:

```csharp
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Qlgx.Api.Tests;

public class NhatKyGhiHangLoatTests(QlgxApiFactory f) : IClassFixture<QlgxApiFactory>
{
    [Fact]
    public async Task Thay_the_hang_loat_sinh_nhat_ky_cho_tung_ban_ghi()
    {
        await using (var db = f.TaoContextThuan())
        {
            db.GiaoDan.AddRange(
                new Domain.Entities.GiaoDan { GiaoXuId = f.GiaoXuId, MaGiaoDanCu = 9301, HoTen = "A", NoiSinh = "Ha Noi" },
                new Domain.Entities.GiaoDan { GiaoXuId = f.GiaoXuId, MaGiaoDanCu = 9302, HoTen = "B", NoiSinh = "Ha Noi" });
            await db.SaveChangesAsync();
        }

        var client = f.CreateAuthClient();
        var phanHoi = await client.PostAsJsonAsync("/api/cong-cu-du-lieu/tim-thay-the",
            new TimThayTheRequest(BangTimThayThe.GiaoDan, "NoiSinh", "Ha Noi", "Ha Noi cu"));
        phanHoi.EnsureSuccessStatusCode();

        await using var doc = f.TaoContextThuan();
        var dong = await doc.HieuLuc.Where(x => x.Truong == "NoiSinh").ToListAsync();
        dong.Should().HaveCount(2, "moi ban ghi bi sua phai co mot dong nhat ky rieng");
        dong.Should().OnlyContain(x => x.GiaTri!.Contains("Ha Noi cu"));
    }
}
```

Cần `using Qlgx.Api.Dtos;` — `TimThayTheRequest(BangTimThayThe Bang, string Truong, string GiaTriTim, string GiaTriThay)` và endpoint `POST /api/cong-cu-du-lieu/tim-thay-the` (route rỗng trong nhóm).

Run: `dotnet test WebApp/tests/Qlgx.Api.Tests --filter NhatKyGhiHangLoatTests`
Expected: FAIL — `dong` rỗng, vì `ExecuteUpdateAsync` không sinh nhật ký.

- [ ] **Step 2: Đổi `TimThayTheService` sang đọc-sửa-lưu theo lô**

Thay thế hai hàm `ThayTheGiaoDan` và `ThayTheGiaDinh`. Chỗ khó: `ExecuteUpdateAsync` nhanh vì làm hết trong một câu SQL; muốn có nhật ký mức ô thì phải nạp bản ghi lên. Giải bằng cách chia lô để không nuốt hết bộ nhớ và không giữ khoá dòng đếm quá lâu:

```csharp
    /// <summary>
    /// Nạp theo lô rồi sửa qua ChangeTracker thay vì ExecuteUpdateAsync.
    ///
    /// ExecuteUpdateAsync nhanh hơn vì làm hết trong một câu SQL, nhưng nó ĐI VÒNG QUA
    /// SaveChanges nên không sinh nhật ký, không đóng dấu UpdatedAt, không đụng xmin. Với màn
    /// hình sửa được hàng nghìn bản ghi trong một lần bấm, mất nhật ký ở đây nghĩa là không
    /// bao giờ truy lại được ai đã đổi gì.
    ///
    /// Chia lô 200 để giữ khoá dòng đếm ngắn — khoá đó xếp hàng MỌI việc ghi của giáo xứ đó
    /// (xem CapSoHieuLuc), nên một giao dịch dài sẽ làm người khác không lưu được.
    /// </summary>
    private const int CoLo = 200;

    private async Task<int> ThayTheTheoLo<T>(
        IQueryable<T> truyVan, Action<T> sua, CancellationToken ct) where T : class
    {
        var tong = 0;
        while (true)
        {
            var lo = await truyVan.Take(CoLo).ToListAsync(ct);
            if (lo.Count == 0) break;

            foreach (var muc in lo) sua(muc);
            await db.LuuCoNhatKy(ct);
            tong += lo.Count;

            if (lo.Count < CoLo) break;
            db.ChangeTracker.Clear();
        }
        return tong;
    }
```

Rồi đổi từng nhánh `switch`, ví dụ:

```csharp
        "TenThanh" => ThayTheTheoLo(LocGiaoDan(truong, giaTriTim), g => g.TenThanh = giaTriThay, ct),
        "HoTen" => ThayTheTheoLo(LocGiaoDan(truong, giaTriTim), g => g.HoTen = giaTriThay, ct),
```

Làm đủ cả 20 nhánh `GiaoDan` và 4 nhánh `GiaDinh`. Cần `using Qlgx.Data.NhatKy;` ở đầu file.

**Bẫy:** vòng lặp lọc theo chính giá trị đang bị thay, nên sau khi sửa lô đầu, các bản ghi đó không còn khớp điều kiện — dùng `Take(CoLo)` không `Skip` là đúng, và điều kiện dừng là lô rỗng.

- [ ] **Step 3: Chạy test**

Run: `dotnet test WebApp/tests/Qlgx.Api.Tests --filter "NhatKyGhiHangLoat|TimThayThe"`
Expected: PASS toàn bộ, kể cả các test `TimThayThe` đã có từ trước.

- [ ] **Step 4: Commit**

```bash
git add WebApp/src/Qlgx.Api/Services/TimThayTheService.cs \
        WebApp/tests/Qlgx.Api.Tests/NhatKyGhiHangLoatTests.cs
git commit -m "Tim va thay the hang loat sinh nhat ky muc truong theo lo"
```

---

## Task 6: Ghi nhật ký cho chuyển giáo họ và các chỗ hàng loạt còn lại

**Ràng buộc bắt buộc, phát hiện từ review Task 4 (tránh deadlock):** `GiaDinhService.Xoa:322` và
`GiaoDanService.Xoa:617` hiện gọi `ExecuteDeleteAsync` **trước** khi vào `LuuCoNhatKy`. Hôm nay vô
hại vì `SinhDongNhatKy` bỏ qua `EntityState.Deleted` nên không dòng nào được ghi. Nhưng khi task
này cho chúng ghi nhật ký, thứ tự khoá sẽ đảo ngược: khoá dòng nghiệp vụ (`ExecuteDelete`) **trước**
khoá dòng đếm — ngược với mọi đường khác trong hệ thống (khoá dòng đếm trước, ghi dữ liệu sau) —
và PostgreSQL sẽ deadlock thật khi hai giao dịch giành khoá theo hai thứ tự khác nhau. **Ở hai chỗ
này, phải giành khoá dòng đếm (gọi `CapSoHieuLuc.LayDaiSo` hoặc mở giao dịch qua `LuuCoNhatKy`)
NGAY SAU khi mở giao dịch, trước bất kỳ `ExecuteDelete`/`ExecuteUpdate` nào.**

**Files:**
- Modify: `src/Qlgx.Api/Services/ChuyenHoService.cs`
- Modify: `src/Qlgx.Api/Services/GiaoDanService.cs:617-618`, `GiaDinhService.cs:322`, `DotBiTichService.cs:159`
- Modify: `tests/Qlgx.Data.Tests/MoiDuongGhiDeuGhiNhatKyTests.cs` (bỏ `Skip`)
- Test: `tests/Qlgx.Api.Tests/NhatKyGhiHangLoatTests.cs` (thêm test)

**Interfaces:**
- Consumes: `ThayTheTheoLo` pattern từ Task 5 (lặp lại cách làm, không dùng chung code vì kiểu khác nhau).
- Produces: không có API mới.

- [ ] **Step 1: Viết test cho chuyển giáo họ**

Thêm vào `NhatKyGhiHangLoatTests.cs`:

```csharp
    [Fact]
    public async Task Chuyen_giao_ho_hang_loat_sinh_nhat_ky()
    {
        Guid hoNguon, hoDich;
        var giaDinhIds = new List<Guid>();

        await using (var db = f.TaoContextThuan())
        {
            var nguon = new Domain.Entities.GiaoHo { GiaoXuId = f.GiaoXuId, TenGiaoHo = "Ho nguon 9501" };
            var dich = new Domain.Entities.GiaoHo { GiaoXuId = f.GiaoXuId, TenGiaoHo = "Ho dich 9501" };
            db.GiaoHo.AddRange(nguon, dich);
            await db.SaveChangesAsync();
            hoNguon = nguon.Id;
            hoDich = dich.Id;

            for (var i = 0; i < 3; i++)
            {
                var gd = new Domain.Entities.GiaDinh
                {
                    GiaoXuId = f.GiaoXuId, MaGiaDinhCu = 9500 + i,
                    TenGiaDinh = $"Gia dinh {i}", GiaoHoId = hoNguon,
                };
                db.GiaDinh.Add(gd);
                giaDinhIds.Add(gd.Id);
            }
            await db.SaveChangesAsync();
        }

        var client = f.CreateAuthClient();
        var phanHoi = await client.PostAsJsonAsync("/api/cong-cu-du-lieu/chuyen-ho/gia-dinh",
            new ChuyenHoGiaDinhRequest(giaDinhIds, hoDich));
        phanHoi.EnsureSuccessStatusCode();

        await using var doc = f.TaoContextThuan();
        var dong = await doc.HieuLuc
            .Where(x => x.Truong == "GiaoHoId" && giaDinhIds.Contains(x.BanGhiId))
            .ToListAsync();

        dong.Should().HaveCount(3, "moi gia dinh bi chuyen phai co mot dong nhat ky rieng");
        dong.Should().OnlyContain(x => x.GiaTri!.Contains(hoDich.ToString()));
    }
```

Cần `using Qlgx.Api.Dtos;` — `ChuyenHoGiaDinhRequest(List<Guid> GiaDinhIds, Guid GiaoHoDichId)`.

**Lưu ý:** chuyển một gia đình kéo theo chuyển **tất cả thành viên** (xem chú thích ở `ChuyenHoDtos.cs`), nên `ChuyenHoService` sinh thêm dòng nhật ký cho `ThanhVienGiaDinh`. Test trên lọc theo `BanGhiId` của gia đình nên không bị ảnh hưởng; nếu muốn kiểm cả thành viên thì thêm một assertion riêng.

Run: `dotnet test WebApp/tests/Qlgx.Api.Tests --filter Chuyen_giao_ho_hang_loat`
Expected: FAIL — `dong` rỗng, vì `ExecuteUpdateAsync` không sinh nhật ký.

- [ ] **Step 2: Đổi `ChuyenHoService` sang đọc-sửa-lưu theo lô**

Ba lời gọi `ExecuteUpdateAsync` ở `ChuyenHoService.cs:54,94,97` đổi sang cùng lối chia lô 200 như Task 5. Giữ nguyên chữ ký công khai của service.

- [ ] **Step 3: Xử lý các `ExecuteDeleteAsync` xoá con**

`GiaoDanService.cs:617-618`, `GiaDinhService.cs:322`, `DotBiTichService.cs:159` xoá bản ghi con hàng loạt.

Ở kế hoạch này **chưa chuyển sang xoá mềm** (đó là kế hoạch 2). Việc phải làm bây giờ: nạp danh sách `Id` bị xoá **trước khi** xoá, rồi sinh dòng nhật ký `sua` trên ô `DaXoa` cho từng bản ghi:

```csharp
        // Nạp Id trước khi xoá để còn ghi nhật ký — sau khi ExecuteDeleteAsync chạy thì không
        // còn gì để đọc. Kế hoạch 2 sẽ chuyển hẳn sang xoá mềm; tới lúc đó đoạn này rút gọn
        // lại thành một lần sửa ô DaXoa bình thường.
        var idBiXoa = await truyVan.Select(x => x.Id).ToListAsync(ct);
```

Rồi thêm dòng nhật ký tường minh cho từng `Id` (dùng `db.ThayDoi.Add` + `db.HieuLuc.Add` qua `CapSoHieuLuc.LayDaiSo` trong cùng giao dịch — lặp lại đúng khuôn trong `LuuCoNhatKy`).

- [ ] **Step 4: Bỏ `Skip` ở kiểm thử kiến trúc và chạy toàn bộ**

Trong `MoiDuongGhiDeuGhiNhatKyTests.cs`, bỏ `Skip` khỏi `Khong_service_nao_goi_ExecuteUpdate_hay_ExecuteDelete_ngoai_danh_sach_da_xu_ly`, và cập nhật mảng `daXuLy` thành đúng danh sách file đã xử lý.

Run: `dotnet test WebApp/tests/Qlgx.Data.Tests && dotnet test WebApp/tests/Qlgx.Api.Tests`
Expected: PASS toàn bộ.

- [ ] **Step 5: Chứng minh kiểm thử kiến trúc biết báo lỗi**

Tạm thêm vào một service bất kỳ một dòng `// ExecuteUpdateAsync` (không phải chú thích — một lời gọi thật trong nhánh không bao giờ chạy tới), chạy lại test.
Expected: **FAIL**, nêu đúng tên file.
Gỡ ra, chạy lại, xác nhận PASS.

- [ ] **Step 6: Commit**

```bash
git add WebApp/src/Qlgx.Api/Services/ WebApp/tests/
git commit -m "Ghi nhat ky cho chuyen giao ho va cac duong ghi hang loat con lai"
```

---

## Task 7: Đọc nhật ký — màn hình "Lịch sử thay đổi"

**Files:**
- Create: `src/Qlgx.Api/Dtos/NhatKyDtos.cs`
- Create: `src/Qlgx.Api/Services/NhatKyService.cs`
- Create: `src/Qlgx.Api/Endpoints/NhatKyEndpoints.cs`
- Modify: `src/Qlgx.Api/Program.cs`
- Test: `tests/Qlgx.Api.Tests/NhatKyDocTests.cs`

**Interfaces:**
- Consumes: `db.ThayDoi`.
- Produces: `GET /api/nhat-ky/{bang}/{banGhiId}` trả `List<DongNhatKyDto>`; `DongNhatKyDto(string Truong, string? GiaTri, string Loai, DateTimeOffset Luc, string? TenNguoiSua)`.

- [ ] **Step 1: Viết test — chạy để thấy fail**

`tests/Qlgx.Api.Tests/NhatKyDocTests.cs`:

```csharp
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace Qlgx.Api.Tests;

public class NhatKyDocTests(QlgxApiFactory f) : IClassFixture<QlgxApiFactory>
{
    [Fact]
    public async Task Tra_ve_lich_su_sua_cua_mot_giao_dan_moi_nhat_truoc()
    {
        Guid id;
        await using (var db = f.TaoContextThuan())
        {
            var g = new Domain.Entities.GiaoDan { GiaoXuId = f.GiaoXuId, MaGiaoDanCu = 9401, HoTen = "Ban dau" };
            db.GiaoDan.Add(g);
            await db.SaveChangesAsync();
            id = g.Id;
        }

        var client = f.CreateAuthClient();
        var sua = await client.PutAsJsonAsync($"/api/giao-dan/{id}", new { HoTen = "Da sua" });
        sua.EnsureSuccessStatusCode();

        var ds = await client.GetFromJsonAsync<List<Dtos.DongNhatKyDto>>($"/api/nhat-ky/GiaoDan/{id}");

        ds.Should().NotBeNull();
        ds!.Should().Contain(d => d.Truong == "HoTen" && d.GiaTri!.Contains("Da sua"));
    }

    [Fact]
    public async Task Khong_doc_duoc_nhat_ky_cua_giao_xu_khac()
    {
        var giaoXuKhac = Guid.NewGuid();
        Guid id;
        await using (var db = f.TaoContextThuan())
        {
            db.GiaoXu.Add(new Domain.Entities.GiaoXu { Id = giaoXuKhac, TenGiaoXu = "Xu khac", MaGiaoXuCu = 77 });
            var g = new Domain.Entities.GiaoDan { GiaoXuId = giaoXuKhac, MaGiaoDanCu = 9402, HoTen = "Nguoi xu khac" };
            db.GiaoDan.Add(g);
            await db.SaveChangesAsync();
            id = g.Id;
        }

        var client = f.CreateAuthClient();
        var ds = await client.GetFromJsonAsync<List<Dtos.DongNhatKyDto>>($"/api/nhat-ky/GiaoDan/{id}");

        ds.Should().BeEmpty("bo loc giao xu phai chan, nhat ky chua nguyen van gia tri cac o");
    }
}
```

Run: `dotnet test WebApp/tests/Qlgx.Api.Tests --filter NhatKyDocTests`
Expected: FAIL — 404, chưa có endpoint.

- [ ] **Step 2: Viết DTO, service và endpoint**

`src/Qlgx.Api/Dtos/NhatKyDtos.cs`:

```csharp
namespace Qlgx.Api.Dtos;

/// <summary>Một dòng lịch sử để hiện trên màn hình — đã gộp tên người sửa, không lộ Id nội bộ.</summary>
public record DongNhatKyDto(
    string Truong, string? GiaTri, string Loai, DateTimeOffset Luc, string? TenNguoiSua);
```

`src/Qlgx.Api/Services/NhatKyService.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using Qlgx.Api.Dtos;
using Qlgx.Data;

namespace Qlgx.Api.Services;

public class NhatKyService(QlgxDbContext db)
{
    /// <summary>
    /// Lịch sử sửa của một bản ghi, mới nhất trước. Chỉ đọc thay_doi (sổ kiểm toán) chứ không
    /// đọc hieu_luc — người dùng muốn thấy MỌI lần sửa, kể cả lần bị luật gộp loại bỏ.
    /// </summary>
    public async Task<List<DongNhatKyDto>> LichSu(
        string bang, Guid banGhiId, int gioiHan, CancellationToken ct) =>
        await (from t in db.ThayDoi
               where t.Bang == bang && t.BanGhiId == banGhiId
               join tk in db.TaiKhoan on t.TaiKhoanId equals tk.Id into nguoi
               from tk in nguoi.DefaultIfEmpty()
               orderby t.DongHoVatLy descending, t.DongHoLogic descending
               select new DongNhatKyDto(
                   t.Truong, t.GiaTri, t.Loai, t.DongHoVatLy,
                   tk != null ? tk.HoTenNguoiDung ?? tk.TenTaiKhoan : null))
            .Take(gioiHan)
            .ToListAsync(ct);
}
```

`src/Qlgx.Api/Endpoints/NhatKyEndpoints.cs`:

```csharp
using Qlgx.Api.Services;

namespace Qlgx.Api.Endpoints;

/// <summary>
/// Lịch sử thay đổi của một bản ghi. RequireAuthorization() trơn + bộ lọc GiaoXuId qua claim
/// là đủ: nhật ký chịu cùng bộ lọc toàn cục và cùng RLS như bảng nghiệp vụ.
/// </summary>
public static class NhatKyEndpoints
{
    public static void MapNhatKy(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/nhat-ky/{bang}/{banGhiId:guid}",
            async (NhatKyService dv, string bang, Guid banGhiId, int? gioiHan, CancellationToken ct) =>
                Results.Ok(await dv.LichSu(bang, banGhiId, gioiHan ?? 200, ct)))
           .RequireAuthorization();
    }
}
```

Trong `Program.cs`, đăng ký service và ánh xạ endpoint theo đúng lối các service/endpoint khác đã làm.

- [ ] **Step 3: Chạy test**

Run: `dotnet test WebApp/tests/Qlgx.Api.Tests --filter NhatKyDocTests`
Expected: PASS (2 test).

- [ ] **Step 4: Chạy toàn bộ bộ test**

Run: `dotnet test WebApp/tests/Qlgx.Data.Tests && dotnet test WebApp/tests/Qlgx.Api.Tests && dotnet test WebApp/tests/Qlgx.Migration.Tests`
Expected: PASS toàn bộ.

- [ ] **Step 5: Commit**

```bash
git add WebApp/src/Qlgx.Api/ WebApp/tests/Qlgx.Api.Tests/NhatKyDocTests.cs
git commit -m "Them duong doc nhat ky: lich su thay doi cua mot ban ghi"
```

---

## Task 8: Kiểm chứng trên dữ liệu thật

**Files:**
- Không sửa mã. Chỉ chạy và ghi lại kết quả.

**Interfaces:**
- Consumes: toàn bộ Task 1-7.
- Produces: một đoạn ghi chú thêm vào cuối tài liệu thiết kế, mục "Đã kiểm chứng".

- [ ] **Step 1: Chạy migration trên database thử**

```bash
cd WebApp/src/Qlgx.Data
export QLGX_TEST_PG="Host=localhost;Username=postgres;Password=<mật khẩu>;Database=qlgx_thu"
dotnet ef database update --startup-project ../Qlgx.Api
```

**Cẩn thận:** `--startup-project ../Qlgx.Data` sẽ dùng `QlgxDbContextFactory` đọc `QLGX_TEST_PG` hệ thống, mà biến đó có thể **thiếu `Database=`** và migration sẽ chạy nhầm vào database `postgres` dùng chung. Luôn dùng `--startup-project ../Qlgx.Api` và luôn kiểm tra chuỗi kết nối có `Database=` trước khi chạy.

- [ ] **Step 2: Sửa vài bản ghi qua giao diện web và kiểm nhật ký**

Khởi động `Qlgx.Api` và web dev server, đăng nhập vào một giáo xứ thử, sửa họ tên một giáo dân, đổi số điện thoại một gia đình, thêm một giáo dân mới.

```sql
SELECT bang, truong, gia_tri, loai, dong_ho_vat_ly
FROM thay_doi ORDER BY dong_ho_vat_ly DESC LIMIT 20;

SELECT so_thu_tu, bang, truong FROM hieu_luc ORDER BY so_thu_tu DESC LIMIT 20;
```

Expected: mỗi ô sửa một dòng; thêm mới một dòng `tao`; `so_thu_tu` liên tục không hở.

- [ ] **Step 3: Kiểm tra không lỗ hổng sau một lần lưu thất bại**

Thử lưu một giáo dân với mã cũ trùng (vi phạm chỉ mục duy nhất), rồi lưu một bản ghi hợp lệ.

```sql
SELECT so_thu_tu FROM hieu_luc ORDER BY so_thu_tu DESC LIMIT 5;
```

Expected: số liên tục, lần lưu thất bại **không** tiêu tốn số nào.

- [ ] **Step 4: Kiểm tra dung lượng nhật ký sinh ra**

```sql
SELECT pg_size_pretty(pg_total_relation_size('thay_doi')) AS thay_doi,
       pg_size_pretty(pg_total_relation_size('hieu_luc')) AS hieu_luc;
```

Ghi lại con số. Nếu nó lớn bất thường so với số thay đổi đã làm, nghi ngờ một cột `byte[]` lọt lưới `CotLoaiTru`.

- [ ] **Step 5: Ghi kết quả vào tài liệu thiết kế và commit**

Thêm vào cuối `docs/superpowers/specs/2026-09-13-dong-bo-offline-design.md` một mục ngắn ghi: ngày kiểm chứng, số dòng nhật ký sinh ra cho mỗi loại thao tác, dung lượng đo được, và bất cứ điều gì khác với dự kiến.

```bash
git add docs/superpowers/specs/2026-09-13-dong-bo-offline-design.md
git commit -m "Ghi ket qua kiem chung nhat ky thay doi tren du lieu that"
```

---

## Ngoài phạm vi kế hoạch này

Những thứ sau thuộc thiết kế nhưng **không** làm ở đây, để mỗi kế hoạch tự cho ra phần mềm chạy được:

| Việc | Kế hoạch |
|---|---|
| **Ghi nhật ký cho thao tác XOÁ** — `SinhDongNhatKy` cố ý bỏ qua `EntityState.Deleted`, vì theo thiết kế mục 4.4 xoá là một **ô** `DaXoa` chịu luật gộp chứ không phải một loại dòng riêng. Cho tới khi 17 chỗ xoá cứng thành xoá mềm và hai bảng nối có cột `DaXoa`, **nhật ký còn thiếu thao tác xoá**: chuyển một giáo dân sang gia đình khác sẽ làm các máy khác thấy người đó ở **cả hai** gia đình | 2 (điều kiện tiên quyết) |
| Xoá mềm cho 17 chỗ xoá cứng, xoá tầng con, `da_xoa` là ô chịu luật gộp. **Bổ sung sau final review:** nhật ký hiện "nửa vời" cho xoá — có ở 3 chỗ (`GhiNhatKyXoaCung`), không có ở 14+ chỗ `Remove()` khác trên **cùng những bảng đó** (`GiaDinhService.XoaThanhVien`, `GiaDinhService.GanVoChong`, `GiaoLyService`...), và một số bảng bị xoá bằng **cascade khoá ngoại ở tầng CSDL** (`ChiTietLopGiaoLy`, `GiaoLyVien` khi xoá lớp/khối) mà EF không bao giờ thấy được dù có bật `EntityState.Deleted`. Kế hoạch xoá mềm phải rà **cả** đường `Remove()` qua ChangeTracker **lẫn** các ràng buộc `OnDelete(DeleteBehavior.Cascade)` trong `ChiTietLopGiaoLyConfig`, `GiaoLyVienConfig`, `BiTichChiTietConfig` — đổi `da_xoa` thành cờ không đủ nếu cascade vẫn xoá cứng dưới gầm | 2 |
| Bảng `thiet_bi`, vé dài hạn, sửa `AuthContext.tsx`, cờ offline | 3 |
| Hiệu chỉnh đồng hồ thật, đoạn đồng hồ, HLC (ở đây `DongHoLogic` luôn 0, mốc là giờ máy chủ) | 4 |
| `thao_tac_da_nhan`, đầu vào nhận-về/gửi-lên, luật gộp, hộp cần xem lại | 4 |
| Đơn vị gộp và bộ kiểm bất biến. **Bổ sung sau final review:** hiện `SinhDongNhatKy` ghi mỗi thuộc tính một dòng rời, kể cả các nhóm phải nhất quán với nhau (`GiaoDan.QuaDoi` + `GiaoDan.NgayQuaDoi`) — trái với `GhiNhatKyXoaCung` (Task 6) vốn đã cố ý ghi `DaXoa` theo đúng hình dạng "một ô" của thiết kế tương lai. Khi kế hoạch này cài "đơn vị gộp", nó sẽ phải **di trú** toàn bộ nhật ký lịch sử đã tích luỹ từ Đợt 1 (các dòng rời của `QuaDoi`/`NgayQuaDoi` sinh ra hôm nay), hoặc chấp nhận đổi `epoch` và bắt máy con tải lại toàn bộ — quyết định này nên chốt sớm trong kế hoạch 4, không để tới lúc cài mới phát hiện | 4 |
| **Mới — xoay `epoch` khi khôi phục sao lưu hoặc dọn nhật ký (điều kiện tiên quyết của đầu nhận-về).** `BoDemHieuLuc.Epoch` (Task 1) được sinh một lần lúc tạo dòng đếm và **không có đường nào đổi nó** — thiết kế mục 4.5 yêu cầu đổi khi khôi phục máy chủ hoặc dọn nhật ký, để con trỏ cũ của máy con bị từ chối tường minh (410) thay vì âm thầm nhận rỗng mãi mãi. Việc này phải **móc vào đường phục hồi sao lưu** (tính năng của một nhánh công việc khác, không thuộc kế hoạch nhật ký này) — cần thiết kế phối hợp riêng trước khi đầu nhận-về của Đợt 2 lên sóng. Cùng vấn đề: `Qlgx.Migration.Core/ChuyenDoiDuLieu` (nhập Access) ghi hàng nghìn dòng qua `SaveChangesAsync` trần, không đụng `bo_dem_hieu_luc`/`epoch` — một lần nhập đè lên giáo xứ đang chạy web sẽ làm mọi máy con không bao giờ biết có dữ liệu mới, không có dấu hiệu gì. Thiết kế mục 4.5 cũng yêu cầu "chính sách lưu giữ phải ghi thành con số" — chưa có con số nào được chốt | 4 |
| Kho IndexedDB, hàng chờ, thanh trạng thái, file dự phòng | 5 |
| Nhập từ desktop | phase sau |

### Đã đổi so với bản kế hoạch ban đầu

Trong lúc thi công, người điều phối đã ra một số ruling ghi đè nội dung brief gốc của từng task (chi
tiết đầy đủ nằm trong `.superpowers/sdd/2026-09-13-nhat-ky-thay-doi/progress.md`, xoá sau khi đóng kế
hoạch — bản tóm tắt dưới đây là bản lưu lâu dài):

- **Task 1:** `CapSoHieuLuc.LayDaiSo` phải **ném lỗi** nếu gọi ngoài giao dịch, không chỉ ghi chú
  — mã mẫu gốc của brief thiếu rào chắn này. Test đồng thời ban đầu chỉ chứng minh "không trùng,
  không hở" chứ chưa chứng minh "thứ tự commit = thứ tự số"; đã thêm test quan sát sự chặn.
- **Task 3:** `TaiKhoan` (mật khẩu, câu hỏi gợi nhớ) bị loại khỏi nhật ký bằng một danh sách phân
  loại tường minh (`PhanLoaiThucThe`) thay vì giả định "kế thừa `ThucTheCoSo`" = "cần nhật ký" —
  giả định đó sai cả hai chiều (kéo cả mật khẩu vào, đồng thời bỏ sót hai bảng nối).
- **Task 3b (chèn thêm, không có trong bản kế hoạch gốc):** thêm khoá chính `Guid` cho
  `ThanhVienGiaDinh`/`GiaoDanHonPhoi`. Chỉ mục duy nhất giữ nguyên **bộ ba** cũ
  `(GiaDinhId, GiaoDanId, VaiTro)`, không siết xuống bộ đôi như đề xuất đầu — siết sẽ mở một đường
  mất dòng mới ở luồng nhập Access.
- **Task 4:** phát hiện hai đường ghi tương tác (`ChuanHoaDuLieuService`, `NhapHocVienGiaoLyService`)
  giữ khoá dòng đếm quá lâu, gây thắt cổ chai toàn giáo xứ — phải chia lô ngay trong task này thay vì
  để tới sau.
- **Task 5, 6:** lặp lại đúng loại lỗi Task 4 vừa sửa (giữ khoá quá lâu) ở "Tìm và thay thế" và
  "Chuyển giáo họ hàng loạt" — cả hai đều phải chia lô, chọn đúng kiểu `Take` hay `Skip/Take` tuỳ
  tập hợp có co lại sau xử lý hay không. Task 5 còn có một hồi quy Critical (vòng lặp vô hạn khi
  giá trị tìm bằng giá trị thay) do chính việc chia lô sinh ra, đã sửa.
- **Sau final review (đợt sửa cuối, 7 việc):** chia lô nốt `TaoDotBiTichTuDongService`; mở rộng
  kiểm thử kiến trúc ra cả `src/Qlgx.Api` với danh sách miễn trừ tường minh; `giao_dich_id` cho
  thao tác xoá nay do người gọi truyền vào một lần thay vì tự sinh nhiều lần trong cùng giao dịch.
