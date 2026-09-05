# Kế hoạch triển khai QLGX bản web — Phase 1

> **Dành cho người/agent thực thi:** BẮT BUỘC dùng kèm skill `superpowers:subagent-driven-development` (khuyến nghị) hoặc `superpowers:executing-plans` để làm từng task một. Các bước dùng ô đánh dấu `- [ ]` để theo dõi tiến độ.

**Mục tiêu:** Hoàn thành bốn màn hình lõi trên nền web — danh sách gia đình, chi tiết gia đình, danh sách giáo dân, chi tiết giáo dân — cùng toàn bộ nền tảng cần thiết để một giáo xứ thí điểm chạy được với dữ liệu thật.

**Kiến trúc:** ASP.NET Core (.NET 8) Web API + PostgreSQL qua EF Core, phục vụ luôn static file của SPA React/TypeScript trong cùng một tiến trình chạy như Windows Service tại văn phòng giáo xứ. Mọi bảng nghiệp vụ dùng khoá chính UUID và mang cột `giao_xu_id` ngay từ đầu để sau này gom nhiều giáo xứ vào một database mà không phải đánh số lại. Dữ liệu từ Access được đưa sang bằng một công cụ dòng lệnh chạy một lần, tách rời hoàn toàn khỏi backend.

**Tech Stack:** .NET 8, ASP.NET Core, EF Core 8 + Npgsql, PostgreSQL 16, xUnit + FluentAssertions, React 18 + TypeScript + Vite, AG Grid Community 32, Vitest + Testing Library, Playwright.

**Spec:** `docs/superpowers/specs/2026-09-06-qlgx-web-migration-design.md`

## Ràng buộc chung

Mọi task đều ngầm chịu các ràng buộc sau.

- Backend target **`net8.0`**. Không tham chiếu thư viện chỉ chạy Windows (`System.Drawing.Common`, Office Interop, `System.Data.OleDb`) trong bất kỳ project nào **trừ** `Qlgx.Migration`.
- PostgreSQL **16 trở lên**. Tên bảng và cột dùng **snake_case không dấu**: `gia_dinh`, `giao_dan`, `thanh_vien_gia_dinh`. Tên lớp C# giữ **PascalCase tiếng Việt**: `GiaDinh`, `GiaoDan`, `ThanhVienGiaDinh`.
- Mọi bảng nghiệp vụ bắt buộc có: `id uuid PK`, `giao_xu_id uuid NOT NULL`, `created_at timestamptz`, `updated_at timestamptz`, `row_version uint (xmin)`, `source_system text`.
- Mọi truy vấn nghiệp vụ lọc theo `giao_xu_id`, kể cả khi database hiện chỉ chứa một giáo xứ.
- Nhãn giao diện giữ **nguyên văn tiếng Việt** như bản desktop — không dịch lại, không đặt lại. Ngoại lệ duy nhất: caption `"&Xem gia đinh"` của `frmGiaoDan` thiếu dấu, bản web ghi đúng `"Xem gia đình"`.
- Ngày tháng trong Access là **TEXT(20) chuỗi `dd/MM/yyyy`**, chuỗi rỗng `''` dùng thay NULL. Sang PostgreSQL dùng `date NULL`; giá trị không phân giải được **không được vứt đi** mà ghi vào cột `du_lieu_loi jsonb` của chính bảng đó.
- Boolean trong Access là `-1`/`0`. Khi đọc phải coi mọi giá trị khác 0 là true.
- `MaGiaoHo = 0` là giá trị quy ước nghĩa là **"Ngoài xứ"**, không phải khoá ngoại hợp lệ.
- Xoá mềm bằng cột `da_xoa boolean` — chỉ có ở `giao_ho`, `gia_dinh`, `giao_dan`. Các bảng khác xoá cứng.
- Mọi commit dùng tiếng Việt không dấu ở dòng tiêu đề.

## Cấu trúc file

```
WebApp/
├── Qlgx.sln
├── src/
│   ├── Qlgx.Domain/                  # Thực thể nghiệp vụ, không phụ thuộc hạ tầng
│   │   ├── Entities/GiaoXu.cs, GiaoHo.cs, GiaDinh.cs, GiaoDan.cs, ThanhVienGiaDinh.cs
│   │   ├── Entities/ThucTheCoSo.cs   # Lớp cơ sở: Id, GiaoXuId, CreatedAt, UpdatedAt…
│   │   └── VaiTroGiaDinh.cs          # 0 = Chồng, 1 = Vợ, 2 = Con
│   ├── Qlgx.Data/                    # EF Core: DbContext, cấu hình ánh xạ, migration
│   │   ├── QlgxDbContext.cs
│   │   ├── Configurations/*.cs
│   │   ├── NgayThangText.cs          # Chuyển đổi chuỗi dd/MM/yyyy ↔ DateOnly
│   │   └── Migrations/
│   ├── Qlgx.Api/                     # Web API + phục vụ static file của SPA
│   │   ├── Program.cs
│   │   ├── BoiCanhGiaoXu.cs          # Xác định giao_xu_id của phiên hiện tại
│   │   ├── Endpoints/GiaDinhEndpoints.cs, GiaoDanEndpoints.cs, GiaoHoEndpoints.cs
│   │   ├── Dtos/*.cs
│   │   └── Services/GiaDinhService.cs, GiaoDanService.cs
│   ├── Qlgx.Migration/               # Console Windows-only, chạy một lần khi onboard
│   │   ├── Program.cs
│   │   ├── DocAccess.cs              # Đọc .mdb qua OleDb
│   │   ├── BangAnhXaId.cs            # Ánh xạ mã số cũ → UUID mới
│   │   ├── ChuyenDoi*.cs             # Từng bảng một
│   │   └── BaoCaoDoiChieu.cs         # So số dòng nguồn ↔ đích
│   └── web/                          # SPA
│       ├── src/api/client.ts, types.ts
│       ├── src/components/           # Tầng dùng lại, tương đương GXControl
│       │   ├── GxGrid.tsx, GxGiaoDanList.tsx, GxGiaDinhList.tsx
│       │   ├── GxFormTabs.tsx, GxField.tsx, GxPicker.tsx, GxToolbar.tsx
│       │   └── ThanhPhanKhung/ (AppShell.tsx, SideNav.tsx, TabDocs.tsx)
│       ├── src/screens/GiaDinhList.tsx, GiaDinhDetail.tsx,
│       │                GiaoDanList.tsx, GiaoDanDetail.tsx
│       └── src/cot/cotGiaDinh.ts, cotGiaoDan.ts   # Định nghĩa cột, tương đương FormatGrid
└── tests/
    ├── Qlgx.Data.Tests/
    ├── Qlgx.Api.Tests/
    ├── Qlgx.Migration.Tests/
    └── e2e/                          # Playwright
```

Nguyên tắc chia file: **định nghĩa cột tách khỏi màn hình** (`src/cot/*.ts`) vì cùng một bộ cột giáo dân được dùng ở hai nơi — danh sách giáo dân và lưới thành viên trong form gia đình. Đây chính là cách bản desktop tách `GxGiaoDanList.FormatGrid()` khỏi `frmGiaoDanList`.

---

## Task 1: Khởi tạo solution và bộ khung chạy được

**Files:**
- Create: `WebApp/Qlgx.sln`
- Create: `WebApp/src/Qlgx.Api/Qlgx.Api.csproj`, `WebApp/src/Qlgx.Api/Program.cs`
- Create: `WebApp/src/Qlgx.Domain/Qlgx.Domain.csproj`
- Create: `WebApp/src/Qlgx.Data/Qlgx.Data.csproj`
- Create: `WebApp/tests/Qlgx.Api.Tests/Qlgx.Api.Tests.csproj`
- Test: `WebApp/tests/Qlgx.Api.Tests/SucKhoeTests.cs`

**Interfaces:**
- Consumes: không có (task đầu tiên).
- Produces: lớp `Program` public để `WebApplicationFactory<Program>` dùng được trong test; endpoint `GET /api/suc-khoe` trả `{ "trangThai": "ok", "phienBan": "<version>" }`.

- [ ] **Bước 1: Tạo solution và các project**

```bash
cd WebApp
dotnet new sln -n Qlgx
dotnet new classlib -o src/Qlgx.Domain -f net8.0
dotnet new classlib -o src/Qlgx.Data -f net8.0
dotnet new web -o src/Qlgx.Api -f net8.0
dotnet new xunit -o tests/Qlgx.Api.Tests -f net8.0
rm src/Qlgx.Domain/Class1.cs src/Qlgx.Data/Class1.cs
dotnet sln add src/Qlgx.Domain src/Qlgx.Data src/Qlgx.Api tests/Qlgx.Api.Tests
dotnet add src/Qlgx.Data reference src/Qlgx.Domain
dotnet add src/Qlgx.Api reference src/Qlgx.Data
dotnet add tests/Qlgx.Api.Tests reference src/Qlgx.Api
dotnet add tests/Qlgx.Api.Tests package Microsoft.AspNetCore.Mvc.Testing
dotnet add tests/Qlgx.Api.Tests package FluentAssertions
```

- [ ] **Bước 2: Viết test thất bại**

Tạo `WebApp/tests/Qlgx.Api.Tests/SucKhoeTests.cs`:

```csharp
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
```

- [ ] **Bước 3: Chạy test để xác nhận thất bại**

Chạy: `dotnet test WebApp/tests/Qlgx.Api.Tests --filter Endpoint_suc_khoe_tra_ve_trang_thai_ok`
Kỳ vọng: FAIL — trả về 404 vì endpoint chưa tồn tại.

- [ ] **Bước 4: Viết code tối thiểu cho test pass**

Ghi đè `WebApp/src/Qlgx.Api/Program.cs`:

```csharp
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/api/suc-khoe", () => Results.Ok(new
{
    trangThai = "ok",
    phienBan = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "0.0.0"
}));

app.Run();

// Để WebApplicationFactory<Program> trong test nhìn thấy được lớp Program sinh tự động
public partial class Program;
```

- [ ] **Bước 5: Chạy test để xác nhận pass**

Chạy: `dotnet test WebApp/tests/Qlgx.Api.Tests`
Kỳ vọng: PASS, 1 test.

- [ ] **Bước 6: Commit**

```bash
git add WebApp/Qlgx.sln WebApp/src WebApp/tests
git commit -m "Khoi tao solution WebApp va endpoint suc khoe

Co-Authored-By: Claude Opus 5 (1M context) <noreply@anthropic.com>"
```

---

## Task 2: Chuyển đổi ngày tháng kiểu Access

Bản Access lưu **mọi** ngày dưới dạng chuỗi `dd/MM/yyyy` trong cột TEXT, và dùng chuỗi rỗng thay cho NULL. Đây là nguồn mất dữ liệu lớn nhất khi chuyển đổi nên được tách thành đơn vị riêng, kiểm thử kỹ trước khi bất kỳ thứ gì khác phụ thuộc vào nó.

**Files:**
- Create: `WebApp/src/Qlgx.Data/NgayThangText.cs`
- Create: `WebApp/tests/Qlgx.Data.Tests/Qlgx.Data.Tests.csproj`
- Test: `WebApp/tests/Qlgx.Data.Tests/NgayThangTextTests.cs`

**Interfaces:**
- Consumes: không có.
- Produces:
  - `static (DateOnly? Ngay, string? LoiGiuLai) NgayThangText.Doc(string? giaTriAccess)` — trả về ngày đã phân giải, hoặc `LoiGiuLai` chứa nguyên văn chuỗi gốc khi không phân giải được.
  - `static string Ghi(DateOnly? ngay)` — sinh lại chuỗi `dd/MM/yyyy`, trả `""` khi null.

- [ ] **Bước 1: Tạo project test**

```bash
cd WebApp
dotnet new xunit -o tests/Qlgx.Data.Tests -f net8.0
dotnet sln add tests/Qlgx.Data.Tests
dotnet add tests/Qlgx.Data.Tests reference src/Qlgx.Data
dotnet add tests/Qlgx.Data.Tests package FluentAssertions
```

- [ ] **Bước 2: Viết test thất bại**

Tạo `WebApp/tests/Qlgx.Data.Tests/NgayThangTextTests.cs`:

```csharp
using FluentAssertions;
using Qlgx.Data;

namespace Qlgx.Data.Tests;

public class NgayThangTextTests
{
    [Theory]
    [InlineData("14/03/2005", 2005, 3, 14)]
    [InlineData("01/01/1941", 1941, 1, 1)]
    [InlineData("9/9/2008", 2008, 9, 9)]      // Access không luôn đệm số 0
    public void Doc_phan_giai_duoc_chuoi_dung_dinh_dang(string vao, int nam, int thang, int ngay)
    {
        var (ketQua, loi) = NgayThangText.Doc(vao);

        ketQua.Should().Be(new DateOnly(nam, thang, ngay));
        loi.Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Doc_coi_chuoi_rong_la_khong_co_ngay(string? vao)
    {
        var (ketQua, loi) = NgayThangText.Doc(vao);

        ketQua.Should().BeNull();
        loi.Should().BeNull();
    }

    [Theory]
    [InlineData("32/13/2005")]     // ngày tháng không tồn tại
    [InlineData("khong ro")]       // người dùng gõ chữ vào ô ngày
    [InlineData("2005-03-14")]     // định dạng khác, không phải dd/MM/yyyy
    public void Doc_giu_lai_nguyen_van_khi_khong_phan_giai_duoc(string vao)
    {
        var (ketQua, loi) = NgayThangText.Doc(vao);

        ketQua.Should().BeNull();
        loi.Should().Be(vao);
    }

    [Fact]
    public void Ghi_sinh_lai_dung_dinh_dang_cua_ban_desktop()
    {
        NgayThangText.Ghi(new DateOnly(2005, 3, 14)).Should().Be("14/03/2005");
        NgayThangText.Ghi(null).Should().Be("");
    }
}
```

- [ ] **Bước 3: Chạy test để xác nhận thất bại**

Chạy: `dotnet test WebApp/tests/Qlgx.Data.Tests`
Kỳ vọng: FAIL — biên dịch lỗi vì `NgayThangText` chưa tồn tại.

- [ ] **Bước 4: Viết code tối thiểu cho test pass**

Tạo `WebApp/src/Qlgx.Data/NgayThangText.cs`:

```csharp
using System.Globalization;

namespace Qlgx.Data;

/// <summary>
/// Bản Access lưu mọi ngày dưới dạng chuỗi "dd/MM/yyyy" trong cột TEXT(20), và dùng
/// chuỗi rỗng thay cho NULL. Giá trị không phân giải được KHÔNG bị vứt đi mà trả về
/// trong <c>LoiGiuLai</c> để nơi gọi ghi vào cột du_lieu_loi.
/// </summary>
public static class NgayThangText
{
    private static readonly string[] DinhDang = ["dd/MM/yyyy", "d/M/yyyy", "dd/M/yyyy", "d/MM/yyyy"];

    public static (DateOnly? Ngay, string? LoiGiuLai) Doc(string? giaTriAccess)
    {
        if (string.IsNullOrWhiteSpace(giaTriAccess))
            return (null, null);

        var thoNguyen = giaTriAccess.Trim();
        if (DateOnly.TryParseExact(thoNguyen, DinhDang, CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var ngay))
            return (ngay, null);

        return (null, giaTriAccess);
    }

    public static string Ghi(DateOnly? ngay) =>
        ngay?.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) ?? "";
}
```

- [ ] **Bước 5: Chạy test để xác nhận pass**

Chạy: `dotnet test WebApp/tests/Qlgx.Data.Tests`
Kỳ vọng: PASS, 11 test.

- [ ] **Bước 6: Commit**

```bash
git add WebApp/src/Qlgx.Data/NgayThangText.cs WebApp/tests/Qlgx.Data.Tests WebApp/Qlgx.sln
git commit -m "Them bo chuyen doi ngay thang kieu Access

Access luu moi ngay la chuoi dd/MM/yyyy trong cot TEXT va dung chuoi rong thay NULL.
Gia tri khong phan giai duoc duoc giu nguyen van de ghi vao cot du_lieu_loi thay vi bi vut di.

Co-Authored-By: Claude Opus 5 (1M context) <noreply@anthropic.com>"
```

---

## Task 3: Thực thể nghiệp vụ và lớp cơ sở

**Files:**
- Create: `WebApp/src/Qlgx.Domain/Entities/ThucTheCoSo.cs`
- Create: `WebApp/src/Qlgx.Domain/Entities/GiaoXu.cs`, `GiaoHo.cs`, `GiaDinh.cs`, `GiaoDan.cs`, `ThanhVienGiaDinh.cs`
- Create: `WebApp/src/Qlgx.Domain/VaiTroGiaDinh.cs`
- Test: `WebApp/tests/Qlgx.Data.Tests/ThucTheTests.cs`

**Interfaces:**
- Consumes: không có.
- Produces: các lớp thực thể dùng ở mọi task sau:
  - `abstract class ThucTheCoSo { Guid Id; Guid GiaoXuId; DateTimeOffset CreatedAt; DateTimeOffset UpdatedAt; uint RowVersion; string? SourceSystem; string? DuLieuLoi; }`
  - `enum VaiTroGiaDinh { Chong = 0, Vo = 1, Con = 2 }`
  - `class GiaDinh : ThucTheCoSo` — 17 cột gốc, khoá ngoại `Guid? GiaoHoId`, `int MaGiaDinhCu`
  - `class GiaoDan : ThucTheCoSo` — 63 cột gốc, `int MaGiaoDanCu`
  - `class ThanhVienGiaDinh` — bảng nối, khoá bộ ba `(GiaDinhId, GiaoDanId, VaiTro)`

- [ ] **Bước 1: Viết test thất bại**

Tạo `WebApp/tests/Qlgx.Data.Tests/ThucTheTests.cs`:

```csharp
using FluentAssertions;
using Qlgx.Domain;
using Qlgx.Domain.Entities;

namespace Qlgx.Data.Tests;

public class ThucTheTests
{
    [Fact]
    public void Thuc_the_moi_tu_sinh_khoa_chinh_uuid()
    {
        var a = new GiaDinh();
        var b = new GiaDinh();

        a.Id.Should().NotBe(Guid.Empty);
        a.Id.Should().NotBe(b.Id, "hai giao xu doc lap phai sinh duoc khoa khong dung nhau");
    }

    [Fact]
    public void Vai_tro_gia_dinh_dung_dung_ba_gia_tri_cua_ban_desktop()
    {
        ((int)VaiTroGiaDinh.Chong).Should().Be(0);
        ((int)VaiTroGiaDinh.Vo).Should().Be(1);
        ((int)VaiTroGiaDinh.Con).Should().Be(2);
        Enum.GetValues<VaiTroGiaDinh>().Should().HaveCount(3);
    }

    [Fact]
    public void Giao_dan_giu_du_cac_truong_bi_tich_cua_ban_desktop()
    {
        var gd = new GiaoDan
        {
            HoTen = "Tran Van Binh",
            TenThanh = "Giuse",
            Phai = "Nam",
            NgayRuaToi = new DateOnly(1972, 5, 20),
            NgayRuocLe = new DateOnly(1980, 6, 12),
            NgayThemSuc = new DateOnly(1986, 10, 18),
            NgayXucDau = null
        };

        gd.NgayRuaToi.Should().Be(new DateOnly(1972, 5, 20));
        gd.NgayRuocLe.Should().NotBeNull();
        gd.NgayThemSuc.Should().NotBeNull();
        gd.NgayXucDau.Should().BeNull();
    }
}
```

- [ ] **Bước 2: Chạy test để xác nhận thất bại**

Chạy: `dotnet test WebApp/tests/Qlgx.Data.Tests --filter ThucTheTests`
Kỳ vọng: FAIL — biên dịch lỗi, các lớp chưa tồn tại.

- [ ] **Bước 3: Viết lớp cơ sở và enum**

Tạo `WebApp/src/Qlgx.Domain/Entities/ThucTheCoSo.cs`:

```csharp
namespace Qlgx.Domain.Entities;

/// <summary>
/// Cột chung của mọi bảng nghiệp vụ. GiaoXuId có mặt ngay cả khi mỗi database hiện chỉ
/// chứa một giáo xứ — nhờ vậy sau này gộp nhiều giáo xứ vào một database chỉ cần bật
/// Row-Level Security theo cột này, không phải sửa tầng truy cập dữ liệu.
/// </summary>
public abstract class ThucTheCoSo
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid GiaoXuId { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>Ánh xạ tới cột hệ thống xmin của PostgreSQL, dùng cho khoá lạc quan.</summary>
    public uint RowVersion { get; set; }

    /// <summary>Đánh dấu bản ghi đến từ đâu, ví dụ "access-2026-09-06".</summary>
    public string? SourceSystem { get; set; }

    /// <summary>
    /// Giá trị gốc không phân giải được khi chuyển đổi, dạng {"NgaySinh":"32/13/2005"}.
    /// Có để dữ liệu hỏng vẫn được giữ lại thay vì mất im lặng.
    /// </summary>
    public string? DuLieuLoi { get; set; }
}
```

Tạo `WebApp/src/Qlgx.Domain/VaiTroGiaDinh.cs`:

```csharp
namespace Qlgx.Domain;

/// <summary>
/// Đúng ba giá trị của cột ThanhVienGiaDinh.VaiTro trong bản Access
/// (GxConstants.cs: VAITRO_CHONG = 0, VAITRO_VO = 1, VAITRO_CON = 2).
/// </summary>
public enum VaiTroGiaDinh
{
    Chong = 0,
    Vo = 1,
    Con = 2
}
```

- [ ] **Bước 4: Viết các thực thể nhỏ**

Tạo `WebApp/src/Qlgx.Domain/Entities/GiaoXu.cs`:

```csharp
namespace Qlgx.Domain.Entities;

public class GiaoXu
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public int MaGiaoXuCu { get; set; }
    public int? MaGiaoXuRieng { get; set; }
    public string TenGiaoXu { get; set; } = "";
    public string? TenGiaoHat { get; set; }
    public string? TenGiaoPhan { get; set; }
    public string? DiaChi { get; set; }
    public string? DienThoai { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }
    public string? GhiChu { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
```

Tạo `WebApp/src/Qlgx.Domain/Entities/GiaoHo.cs`:

```csharp
namespace Qlgx.Domain.Entities;

public class GiaoHo : ThucTheCoSo
{
    public int MaGiaoHoCu { get; set; }
    public string TenGiaoHo { get; set; } = "";
    /// <summary>Giáo họ cha — bản Access thêm cột này ở phiên bản 2.1.1.2.</summary>
    public Guid? GiaoHoChaId { get; set; }
    public bool DaXoa { get; set; }
    public string? MaNhanDang { get; set; }
}
```

Tạo `WebApp/src/Qlgx.Domain/Entities/GiaDinh.cs`:

```csharp
namespace Qlgx.Domain.Entities;

public class GiaDinh : ThucTheCoSo
{
    public int MaGiaDinhCu { get; set; }
    /// <summary>Mã do người dùng tự nhập, bật bằng cấu hình TUNHAP_MAGIADINH.</summary>
    public string? MaGiaDinhRieng { get; set; }
    public Guid? GiaoHoId { get; set; }
    public string? TenGiaDinh { get; set; }
    public string? GhiChu { get; set; }
    public string? DienThoai { get; set; }
    public string? DiaChi { get; set; }
    public string? SoHoKhau { get; set; }
    public string? DienGiaDinh { get; set; }
    public string? AnhDaiDien { get; set; }
    public bool DaXoa { get; set; }
    public bool DaChuyenXu { get; set; }
    public DateOnly? NgayChuyen { get; set; }
    public string? NoiChuyen { get; set; }
    /// <summary>Bản gốc: GiaDinhAo — gia đình không được tính vào thống kê.</summary>
    public bool KhongThongKe { get; set; }
    public string? MaNhanDang { get; set; }

    public GiaoHo? GiaoHo { get; set; }
    public List<ThanhVienGiaDinh> ThanhVien { get; set; } = [];
}
```

Tạo `WebApp/src/Qlgx.Domain/Entities/ThanhVienGiaDinh.cs`:

```csharp
namespace Qlgx.Domain.Entities;

/// <summary>
/// Bảng nối giáo dân với gia đình. Bản Access không khai báo khoá chính và CHO PHÉP một
/// giáo dân thuộc nhiều gia đình (con ở nhà cha mẹ, đồng thời có gia đình riêng), nên
/// khoá ở đây là bộ ba (GiaDinhId, GiaoDanId, VaiTro).
/// </summary>
public class ThanhVienGiaDinh
{
    public Guid GiaDinhId { get; set; }
    public Guid GiaoDanId { get; set; }
    public VaiTroGiaDinh VaiTro { get; set; }
    public bool ChuHo { get; set; }
    public Guid GiaoXuId { get; set; }

    public GiaDinh? GiaDinh { get; set; }
    public GiaoDan? GiaoDan { get; set; }
}
```

- [ ] **Bước 5: Viết thực thể GiaoDan với đủ 63 cột gốc**

Tạo `WebApp/src/Qlgx.Domain/Entities/GiaoDan.cs`:

```csharp
namespace Qlgx.Domain.Entities;

public class GiaoDan : ThucTheCoSo
{
    public int MaGiaoDanCu { get; set; }

    // --- Nhân thân ---
    public string HoTen { get; set; } = "";
    public string? TenThanh { get; set; }
    public string? Phai { get; set; }
    public DateOnly? NgaySinh { get; set; }
    public string? NoiSinh { get; set; }
    public string? CMND { get; set; }
    public string? DanToc { get; set; }
    public Guid? GiaoHoId { get; set; }
    public string? ThuocGiaoXu { get; set; }
    public string? ThuocGiaoPhan { get; set; }
    public string? DiaChi { get; set; }
    public string? DienThoai { get; set; }
    public string? Email { get; set; }
    public string? AnhDaiDien { get; set; }
    public string? HoTenCha { get; set; }
    public string? HoTenMe { get; set; }

    // --- Rửa tội ---
    public string? SoRuaToi { get; set; }
    public DateOnly? NgayRuaToi { get; set; }
    public string? NoiRuaToi { get; set; }
    public string? ChaRuaToi { get; set; }
    public string? NguoiDoDauRuaToi { get; set; }

    // --- Rước lễ lần đầu ---
    public string? SoRuocLe { get; set; }
    public DateOnly? NgayRuocLe { get; set; }
    public string? NoiRuocLe { get; set; }
    public string? ChaRuocLe { get; set; }

    // --- Thêm sức ---
    public string? SoThemSuc { get; set; }
    public DateOnly? NgayThemSuc { get; set; }
    public string? NoiThemSuc { get; set; }
    public string? ChaThemSuc { get; set; }
    public string? NguoiDoDauThemSuc { get; set; }

    // --- Xức dầu ---
    public DateOnly? NgayXucDau { get; set; }
    public string? NguoiXucDau { get; set; }
    public string? TinhTrangXucDau { get; set; }
    public string? GhiChuXucDau { get; set; }

    // --- Giáo lý ---
    public DateOnly? NgayBD1 { get; set; }
    public string? NoiBD1 { get; set; }
    public DateOnly? NgayBD2 { get; set; }
    public string? NoiBD2 { get; set; }
    public DateOnly? NgayTHVaoDoi { get; set; }
    public string? NoiTHVaoDoi { get; set; }
    public DateOnly? NgayGLHN1 { get; set; }
    public DateOnly? NgayGLHN2 { get; set; }
    public string? NoiGLHN { get; set; }
    public string? NguoiChungNhanGLHN { get; set; }
    public string? XepLoaiGLHN { get; set; }

    // --- Học vấn, nghề nghiệp ---
    public string? TrinhDoVanHoa { get; set; }
    public string? TrinhDoChuyenMon { get; set; }
    public string? BietNgoaiNgu { get; set; }
    public string? NgheNghiep { get; set; }
    public bool ConHoc { get; set; }

    // --- Tình trạng ---
    public bool DaCoGiaDinh { get; set; }
    public bool TanTong { get; set; }
    /// <summary>Bản gốc: GiaoDanAo — không được tính vào thống kê.</summary>
    public bool KhongThongKe { get; set; }
    public bool QuaDoi { get; set; }
    public DateOnly? NgayQuaDoi { get; set; }
    public string? NoiQuaDoi { get; set; }
    public string? SoAnTang { get; set; }
    public string? NoiAnTang { get; set; }
    public bool DaXoa { get; set; }

    public string? GhiChu { get; set; }
    public string? MaNhanDang { get; set; }

    public GiaoHo? GiaoHo { get; set; }
    public List<ThanhVienGiaDinh> GiaDinhThamGia { get; set; } = [];
}
```

- [ ] **Bước 6: Chạy test để xác nhận pass**

Chạy: `dotnet test WebApp/tests/Qlgx.Data.Tests`
Kỳ vọng: PASS, 14 test (11 của Task 2 cộng 3 mới).

- [ ] **Bước 7: Commit**

```bash
git add WebApp/src/Qlgx.Domain WebApp/tests/Qlgx.Data.Tests
git commit -m "Them thuc the nghiep vu voi khoa chinh UUID va cot giao_xu_id

GiaoDan giu du 63 cot goc cua ban Access. ThanhVienGiaDinh dung khoa bo ba
(GiaDinhId, GiaoDanId, VaiTro) vi ban goc cho phep mot giao dan thuoc nhieu gia dinh.

Co-Authored-By: Claude Opus 5 (1M context) <noreply@anthropic.com>"
```

---

## Task 4: DbContext, ánh xạ schema và migration đầu tiên

**Files:**
- Create: `WebApp/src/Qlgx.Data/QlgxDbContext.cs`
- Create: `WebApp/src/Qlgx.Data/Configurations/GiaDinhConfig.cs`, `GiaoDanConfig.cs`, `GiaoHoConfig.cs`, `ThanhVienGiaDinhConfig.cs`
- Create: `WebApp/src/Qlgx.Data/Migrations/` (sinh tự động)
- Create: `WebApp/tests/Qlgx.Data.Tests/CoSoDuLieuFixture.cs`
- Test: `WebApp/tests/Qlgx.Data.Tests/SchemaTests.cs`

**Interfaces:**
- Consumes: các thực thể của Task 3.
- Produces:
  - `class QlgxDbContext(DbContextOptions<QlgxDbContext> options) : DbContext` với `DbSet<GiaoXu> GiaoXu`, `DbSet<GiaoHo> GiaoHo`, `DbSet<GiaDinh> GiaDinh`, `DbSet<GiaoDan> GiaoDan`, `DbSet<ThanhVienGiaDinh> ThanhVienGiaDinh`.
  - `class CoSoDuLieuFixture : IAsyncLifetime` — tạo database test riêng theo tên ngẫu nhiên, có `QlgxDbContext TaoContext()` và `Guid GiaoXuId`.

**Ghi chú về môi trường test:** không dùng Docker. Fixture tạo một database mới trên PostgreSQL cục bộ theo chuỗi kết nối lấy từ biến môi trường `QLGX_TEST_PG` (mặc định `Host=localhost;Username=postgres;Password=postgres`), rồi xoá đi sau khi chạy xong.

- [ ] **Bước 1: Thêm gói NuGet**

```bash
cd WebApp
dotnet add src/Qlgx.Data package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add src/Qlgx.Data package Microsoft.EntityFrameworkCore.Design
dotnet add tests/Qlgx.Data.Tests package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet tool install --global dotnet-ef
```

- [ ] **Bước 2: Viết fixture tạo database test**

Tạo `WebApp/tests/Qlgx.Data.Tests/CoSoDuLieuFixture.cs`:

```csharp
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
```

- [ ] **Bước 3: Viết test thất bại**

Tạo `WebApp/tests/Qlgx.Data.Tests/SchemaTests.cs`:

```csharp
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Qlgx.Domain;
using Qlgx.Domain.Entities;

namespace Qlgx.Data.Tests;

public class SchemaTests(CoSoDuLieuFixture db) : IClassFixture<CoSoDuLieuFixture>
{
    [Fact]
    public async Task Luu_va_doc_lai_duoc_gia_dinh_kem_thanh_vien()
    {
        await using var ctx = db.TaoContext();

        var giaoHo = new GiaoHo { GiaoXuId = db.GiaoXuId, TenGiaoHo = "Giao ho Thanh Tam", MaGiaoHoCu = 1 };
        var giaDinh = new GiaDinh
        {
            GiaoXuId = db.GiaoXuId, MaGiaDinhCu = 12,
            TenGiaDinh = "Binh - Lan", GiaoHo = giaoHo, DiaChi = "12/4 Nguyen Trai"
        };
        var chong = new GiaoDan { GiaoXuId = db.GiaoXuId, MaGiaoDanCu = 4401, HoTen = "Tran Van Binh", Phai = "Nam" };
        ctx.AddRange(giaoHo, giaDinh, chong);
        ctx.ThanhVienGiaDinh.Add(new ThanhVienGiaDinh
        {
            GiaoXuId = db.GiaoXuId, GiaDinh = giaDinh, GiaoDan = chong,
            VaiTro = VaiTroGiaDinh.Chong, ChuHo = true
        });
        await ctx.SaveChangesAsync();

        await using var ctx2 = db.TaoContext();
        var docLai = await ctx2.GiaDinh
            .Include(x => x.ThanhVien).ThenInclude(tv => tv.GiaoDan)
            .SingleAsync(x => x.MaGiaDinhCu == 12);

        docLai.TenGiaDinh.Should().Be("Binh - Lan");
        docLai.ThanhVien.Should().ContainSingle()
            .Which.GiaoDan!.HoTen.Should().Be("Tran Van Binh");
    }

    [Fact]
    public async Task Mot_giao_dan_duoc_phep_thuoc_nhieu_gia_dinh()
    {
        await using var ctx = db.TaoContext();

        var nhaChaMe = new GiaDinh { GiaoXuId = db.GiaoXuId, MaGiaDinhCu = 100, TenGiaDinh = "Nha cha me" };
        var nhaRieng = new GiaDinh { GiaoXuId = db.GiaoXuId, MaGiaDinhCu = 101, TenGiaDinh = "Nha rieng" };
        var nguoi = new GiaoDan { GiaoXuId = db.GiaoXuId, MaGiaoDanCu = 5000, HoTen = "Tran Thi Huong", Phai = "Nu" };
        ctx.AddRange(nhaChaMe, nhaRieng, nguoi);
        ctx.ThanhVienGiaDinh.AddRange(
            new ThanhVienGiaDinh { GiaoXuId = db.GiaoXuId, GiaDinh = nhaChaMe, GiaoDan = nguoi, VaiTro = VaiTroGiaDinh.Con },
            new ThanhVienGiaDinh { GiaoXuId = db.GiaoXuId, GiaDinh = nhaRieng, GiaoDan = nguoi, VaiTro = VaiTroGiaDinh.Vo });

        var luu = async () => await ctx.SaveChangesAsync();

        await luu.Should().NotThrowAsync("ban Access cho phep con cai vua o nha cha me vua co gia dinh rieng");
    }

    [Fact]
    public async Task Ngay_thang_luu_dung_kieu_date_khong_phai_chuoi()
    {
        await using var ctx = db.TaoContext();
        ctx.GiaoDan.Add(new GiaoDan
        {
            GiaoXuId = db.GiaoXuId, MaGiaoDanCu = 6000, HoTen = "Vu Minh Tri",
            NgaySinh = new DateOnly(1996, 4, 2)
        });
        await ctx.SaveChangesAsync();

        await using var ctx2 = db.TaoContext();
        var kieuCot = await ctx2.Database
            .SqlQuery<string>($@"SELECT data_type FROM information_schema.columns
                                 WHERE table_name = 'giao_dan' AND column_name = 'ngay_sinh'")
            .SingleAsync();

        kieuCot.Should().Be("date");
    }

    [Fact]
    public async Task Moi_bang_nghiep_vu_deu_co_cot_giao_xu_id()
    {
        await using var ctx = db.TaoContext();

        var bangThieu = await ctx.Database.SqlQuery<string>($@"
            SELECT t.table_name FROM information_schema.tables t
            WHERE t.table_schema = 'public'
              AND t.table_name IN ('giao_ho','gia_dinh','giao_dan','thanh_vien_gia_dinh')
              AND NOT EXISTS (SELECT 1 FROM information_schema.columns c
                              WHERE c.table_name = t.table_name AND c.column_name = 'giao_xu_id')
        ").ToListAsync();

        bangThieu.Should().BeEmpty("moi bang nghiep vu phai san sang cho viec gom cum ve sau");
    }
}
```

- [ ] **Bước 4: Chạy test để xác nhận thất bại**

Chạy: `dotnet test WebApp/tests/Qlgx.Data.Tests --filter SchemaTests`
Kỳ vọng: FAIL — biên dịch lỗi vì `QlgxDbContext` chưa tồn tại.

- [ ] **Bước 5: Viết DbContext**

Tạo `WebApp/src/Qlgx.Data/QlgxDbContext.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using Qlgx.Domain.Entities;

namespace Qlgx.Data;

public class QlgxDbContext(DbContextOptions<QlgxDbContext> options) : DbContext(options)
{
    public DbSet<GiaoXu> GiaoXu => Set<GiaoXu>();
    public DbSet<GiaoHo> GiaoHo => Set<GiaoHo>();
    public DbSet<GiaDinh> GiaDinh => Set<GiaDinh>();
    public DbSet<GiaoDan> GiaoDan => Set<GiaoDan>();
    public DbSet<ThanhVienGiaDinh> ThanhVienGiaDinh => Set<ThanhVienGiaDinh>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.ApplyConfigurationsFromAssembly(typeof(QlgxDbContext).Assembly);
        DatTenSnakeCase(b);
    }

    public override int SaveChanges()
    {
        DongDauThoiGian();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        DongDauThoiGian();
        return base.SaveChangesAsync(ct);
    }

    private void DongDauThoiGian()
    {
        foreach (var e in ChangeTracker.Entries<ThucTheCoSo>())
        {
            if (e.State == EntityState.Added) e.Entity.CreatedAt = DateTimeOffset.UtcNow;
            if (e.State is EntityState.Added or EntityState.Modified)
                e.Entity.UpdatedAt = DateTimeOffset.UtcNow;
        }
    }

    /// <summary>
    /// PostgreSQL phân biệt hoa thường khi tên có dấu nháy kép, nên toàn bộ tên bảng và cột
    /// được hạ về snake_case một lần ở đây thay vì đặt tên tay ở từng cấu hình.
    /// </summary>
    private static void DatTenSnakeCase(ModelBuilder b)
    {
        foreach (var thucThe in b.Model.GetEntityTypes())
        {
            thucThe.SetTableName(SangSnakeCase(thucThe.GetTableName()!));
            foreach (var thuocTinh in thucThe.GetProperties())
                thuocTinh.SetColumnName(SangSnakeCase(thuocTinh.GetColumnName()));
            foreach (var khoa in thucThe.GetKeys())
                khoa.SetName(SangSnakeCase(khoa.GetName()!));
            foreach (var fk in thucThe.GetForeignKeys())
                fk.SetConstraintName(SangSnakeCase(fk.GetConstraintName()!));
            foreach (var chiMuc in thucThe.GetIndexes())
                chiMuc.SetDatabaseName(SangSnakeCase(chiMuc.GetDatabaseName()!));
        }
    }

    private static string SangSnakeCase(string ten)
    {
        var kq = new System.Text.StringBuilder(ten.Length + 8);
        for (var i = 0; i < ten.Length; i++)
        {
            var c = ten[i];
            if (char.IsUpper(c) && i > 0 && (!char.IsUpper(ten[i - 1]) || (i + 1 < ten.Length && char.IsLower(ten[i + 1]))))
                kq.Append('_');
            kq.Append(char.ToLowerInvariant(c));
        }
        return kq.ToString();
    }
}
```

- [ ] **Bước 6: Viết cấu hình ánh xạ**

Tạo `WebApp/src/Qlgx.Data/Configurations/ThanhVienGiaDinhConfig.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Qlgx.Domain.Entities;

namespace Qlgx.Data.Configurations;

public class ThanhVienGiaDinhConfig : IEntityTypeConfiguration<ThanhVienGiaDinh>
{
    public void Configure(EntityTypeBuilder<ThanhVienGiaDinh> b)
    {
        // Khoá bộ ba, không phải cặp: một giáo dân có thể vừa là Con ở nhà cha mẹ
        // vừa là Vợ trong gia đình riêng.
        b.HasKey(x => new { x.GiaDinhId, x.GiaoDanId, x.VaiTro });
        b.Property(x => x.VaiTro).HasConversion<int>();
        b.HasOne(x => x.GiaDinh).WithMany(g => g.ThanhVien).HasForeignKey(x => x.GiaDinhId);
        b.HasOne(x => x.GiaoDan).WithMany(g => g.GiaDinhThamGia).HasForeignKey(x => x.GiaoDanId);
        b.HasIndex(x => new { x.GiaoXuId, x.GiaDinhId });
    }
}
```

Tạo `WebApp/src/Qlgx.Data/Configurations/GiaDinhConfig.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Qlgx.Domain.Entities;

namespace Qlgx.Data.Configurations;

public class GiaDinhConfig : IEntityTypeConfiguration<GiaDinh>
{
    public void Configure(EntityTypeBuilder<GiaDinh> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.RowVersion).IsRowVersion().HasColumnName("xmin").HasColumnType("xid");
        b.Property(x => x.DuLieuLoi).HasColumnType("jsonb");
        b.HasOne(x => x.GiaoHo).WithMany().HasForeignKey(x => x.GiaoHoId).OnDelete(DeleteBehavior.SetNull);
        // Mọi truy vấn danh sách đều lọc theo giáo xứ rồi loại bản ghi đã xoá mềm
        b.HasIndex(x => new { x.GiaoXuId, x.DaXoa });
        b.HasIndex(x => new { x.GiaoXuId, x.MaGiaDinhCu }).IsUnique();
    }
}
```

Tạo `WebApp/src/Qlgx.Data/Configurations/GiaoDanConfig.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Qlgx.Domain.Entities;

namespace Qlgx.Data.Configurations;

public class GiaoDanConfig : IEntityTypeConfiguration<GiaoDan>
{
    public void Configure(EntityTypeBuilder<GiaoDan> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.RowVersion).IsRowVersion().HasColumnName("xmin").HasColumnType("xid");
        b.Property(x => x.DuLieuLoi).HasColumnType("jsonb");
        b.Property(x => x.HoTen).IsRequired();
        b.HasOne(x => x.GiaoHo).WithMany().HasForeignKey(x => x.GiaoHoId).OnDelete(DeleteBehavior.SetNull);
        b.HasIndex(x => new { x.GiaoXuId, x.DaXoa });
        b.HasIndex(x => new { x.GiaoXuId, x.MaGiaoDanCu }).IsUnique();
        b.HasIndex(x => new { x.GiaoXuId, x.HoTen });
    }
}
```

Tạo `WebApp/src/Qlgx.Data/Configurations/GiaoHoConfig.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Qlgx.Domain.Entities;

namespace Qlgx.Data.Configurations;

public class GiaoHoConfig : IEntityTypeConfiguration<GiaoHo>
{
    public void Configure(EntityTypeBuilder<GiaoHo> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.RowVersion).IsRowVersion().HasColumnName("xmin").HasColumnType("xid");
        b.Property(x => x.DuLieuLoi).HasColumnType("jsonb");
        b.Property(x => x.TenGiaoHo).IsRequired();
        // Giáo họ cha - con, bản Access thêm ở phiên bản 2.1.1.2
        b.HasOne<GiaoHo>().WithMany().HasForeignKey(x => x.GiaoHoChaId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => new { x.GiaoXuId, x.MaGiaoHoCu }).IsUnique();
    }
}
```

- [ ] **Bước 7: Sinh migration đầu tiên**

```bash
cd WebApp
dotnet ef migrations add SchemaBanDau --project src/Qlgx.Data --startup-project src/Qlgx.Api
```

Mở file migration vừa sinh trong `src/Qlgx.Data/Migrations/` và **kiểm tra bằng mắt** ba điều trước khi chạy tiếp:
- tên bảng là `gia_dinh`, `giao_dan`, `thanh_vien_gia_dinh` (snake_case, không có dấu nháy kép chữ hoa);
- các cột ngày là `date`, không phải `text`;
- không có cột `xmin` nào được `CREATE` — nó là cột hệ thống sẵn có của PostgreSQL, EF chỉ đọc.

Nếu migration có lệnh tạo cột `xmin`, sửa cấu hình thành `.HasColumnName("xmin").HasColumnType("xid").ValueGeneratedOnAddOrUpdate()` rồi sinh lại migration.

- [ ] **Bước 8: Chạy test để xác nhận pass**

Chạy: `dotnet test WebApp/tests/Qlgx.Data.Tests`
Kỳ vọng: PASS, 18 test. Nếu lỗi kết nối, đặt biến môi trường `QLGX_TEST_PG` trỏ đúng PostgreSQL cục bộ.

- [ ] **Bước 9: Commit**

```bash
git add WebApp/src/Qlgx.Data WebApp/tests/Qlgx.Data.Tests
git commit -m "Them DbContext, anh xa schema va migration dau tien

Ten bang va cot ha ve snake_case mot lan trong OnModelCreating. Khoa lac quan dung
cot he thong xmin cua PostgreSQL. Khoa cua thanh_vien_gia_dinh la bo ba de mot giao dan
van thuoc duoc nhieu gia dinh nhu ban Access.

Co-Authored-By: Claude Opus 5 (1M context) <noreply@anthropic.com>"
```

---

## Task 5: Bối cảnh giáo xứ và bộ lọc dữ liệu

Bốn màn hình đều phải lọc theo `giao_xu_id`. Tách thành task riêng vì đây là chỗ quyết định
việc gom cụm sau này có làm được mà không sửa tầng dữ liệu hay không.

**Files:**
- Create: `WebApp/src/Qlgx.Api/BoiCanhGiaoXu.cs`
- Modify: `WebApp/src/Qlgx.Data/QlgxDbContext.cs` (thêm bộ lọc toàn cục)
- Modify: `WebApp/src/Qlgx.Api/Program.cs`
- Test: `WebApp/tests/Qlgx.Data.Tests/LocTheoGiaoXuTests.cs`

**Interfaces:**
- Consumes: `QlgxDbContext` của Task 4.
- Produces:
  - `interface IBoiCanhGiaoXu { Guid GiaoXuId { get; } }`
  - `class BoiCanhGiaoXuTuCauHinh(IConfiguration cfg) : IBoiCanhGiaoXu` — đọc `Qlgx:GiaoXuId` từ cấu hình khi chạy on-prem một giáo xứ.
  - `QlgxDbContext` nhận thêm tham số tuỳ chọn `IBoiCanhGiaoXu?` ở hàm dựng; khi có thì mọi truy vấn `GiaoHo`, `GiaDinh`, `GiaoDan`, `ThanhVienGiaDinh` tự lọc theo giáo xứ đó.

- [ ] **Bước 1: Viết test thất bại**

Tạo `WebApp/tests/Qlgx.Data.Tests/LocTheoGiaoXuTests.cs`:

```csharp
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Qlgx.Data;
using Qlgx.Domain.Entities;

namespace Qlgx.Data.Tests;

public class LocTheoGiaoXuTests(CoSoDuLieuFixture db) : IClassFixture<CoSoDuLieuFixture>
{
    private sealed record BoiCanh(Guid GiaoXuId) : IBoiCanhGiaoXu;

    [Fact]
    public async Task Truy_van_chi_thay_du_lieu_cua_giao_xu_hien_hanh()
    {
        var giaoXuKhac = Guid.NewGuid();
        await using (var ctx = db.TaoContext())
        {
            ctx.GiaoXu.Add(new GiaoXu { Id = giaoXuKhac, TenGiaoXu = "Giao xu khac", MaGiaoXuCu = 2 });
            ctx.GiaoDan.AddRange(
                new GiaoDan { GiaoXuId = db.GiaoXuId, MaGiaoDanCu = 7001, HoTen = "Nguoi cua xu minh" },
                new GiaoDan { GiaoXuId = giaoXuKhac, MaGiaoDanCu = 7002, HoTen = "Nguoi cua xu khac" });
            await ctx.SaveChangesAsync();
        }

        await using var ctxLoc = new QlgxDbContext(
            new DbContextOptionsBuilder<QlgxDbContext>().UseNpgsql(db.ChuoiKetNoi).Options,
            new BoiCanh(db.GiaoXuId));

        var danhSach = await ctxLoc.GiaoDan.Select(x => x.HoTen).ToListAsync();

        danhSach.Should().Contain("Nguoi cua xu minh");
        danhSach.Should().NotContain("Nguoi cua xu khac");
    }

    [Fact]
    public async Task Khong_co_boi_canh_thi_thay_toan_bo_du_lieu()
    {
        await using var ctx = db.TaoContext();

        var soGiaoXu = await ctx.GiaoDan.Select(x => x.GiaoXuId).Distinct().CountAsync();

        soGiaoXu.Should().BeGreaterThan(0, "cong cu chuyen doi du lieu can ghi cho nhieu giao xu");
    }
}
```

- [ ] **Bước 2: Chạy test để xác nhận thất bại**

Chạy: `dotnet test WebApp/tests/Qlgx.Data.Tests --filter LocTheoGiaoXuTests`
Kỳ vọng: FAIL — `IBoiCanhGiaoXu` chưa tồn tại.

- [ ] **Bước 3: Thêm giao diện bối cảnh vào tầng dữ liệu**

Tạo `WebApp/src/Qlgx.Data/IBoiCanhGiaoXu.cs`:

```csharp
namespace Qlgx.Data;

/// <summary>
/// Giáo xứ của phiên làm việc hiện tại. Khi chạy on-prem, giá trị lấy từ cấu hình vì mỗi
/// bản cài chỉ phục vụ một giáo xứ. Khi gom cụm về sau, chỉ cần đổi cách lấy giá trị này
/// sang claim của người đăng nhập — tầng truy vấn không phải sửa gì.
/// </summary>
public interface IBoiCanhGiaoXu
{
    Guid GiaoXuId { get; }
}
```

- [ ] **Bước 4: Thêm bộ lọc toàn cục vào DbContext**

Sửa `WebApp/src/Qlgx.Data/QlgxDbContext.cs` — đổi hàm dựng và thêm bộ lọc:

```csharp
public class QlgxDbContext(DbContextOptions<QlgxDbContext> options, IBoiCanhGiaoXu? boiCanh = null)
    : DbContext(options)
{
    private readonly IBoiCanhGiaoXu? _boiCanh = boiCanh;

    // … các DbSet giữ nguyên …

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.ApplyConfigurationsFromAssembly(typeof(QlgxDbContext).Assembly);

        // Bộ lọc toàn cục: khi có bối cảnh giáo xứ thì mọi truy vấn tự thêm điều kiện.
        // Công cụ chuyển đổi dữ liệu chạy KHÔNG có bối cảnh nên vẫn ghi được cho mọi giáo xứ.
        b.Entity<GiaoHo>().HasQueryFilter(x => _boiCanh == null || x.GiaoXuId == _boiCanh.GiaoXuId);
        b.Entity<GiaDinh>().HasQueryFilter(x => _boiCanh == null || x.GiaoXuId == _boiCanh.GiaoXuId);
        b.Entity<GiaoDan>().HasQueryFilter(x => _boiCanh == null || x.GiaoXuId == _boiCanh.GiaoXuId);
        b.Entity<ThanhVienGiaDinh>().HasQueryFilter(x => _boiCanh == null || x.GiaoXuId == _boiCanh.GiaoXuId);

        DatTenSnakeCase(b);
    }

    // … phần còn lại giữ nguyên …
}
```

- [ ] **Bước 5: Nối vào Program.cs**

Tạo `WebApp/src/Qlgx.Api/BoiCanhGiaoXu.cs`:

```csharp
using Qlgx.Data;

namespace Qlgx.Api;

public class BoiCanhGiaoXuTuCauHinh(IConfiguration cauHinh) : IBoiCanhGiaoXu
{
    public Guid GiaoXuId { get; } = Guid.Parse(
        cauHinh["Qlgx:GiaoXuId"]
        ?? throw new InvalidOperationException(
            "Thieu cau hinh Qlgx:GiaoXuId — moi ban cai phuc vu dung mot giao xu."));
}
```

Sửa `WebApp/src/Qlgx.Api/Program.cs`, thêm trước `var app = builder.Build();`:

```csharp
builder.Services.AddScoped<IBoiCanhGiaoXu, BoiCanhGiaoXuTuCauHinh>();
builder.Services.AddDbContext<QlgxDbContext>((sp, opt) =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("Qlgx")));
```

Thêm vào `WebApp/src/Qlgx.Api/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "Qlgx": "Host=localhost;Database=qlgx_dev;Username=postgres;Password=postgres"
  },
  "Qlgx": {
    "GiaoXuId": "00000000-0000-0000-0000-000000000001"
  }
}
```

- [ ] **Bước 6: Chạy test để xác nhận pass**

Chạy: `dotnet test WebApp/tests/Qlgx.Data.Tests`
Kỳ vọng: PASS, 20 test.

- [ ] **Bước 7: Commit**

```bash
git add WebApp/src/Qlgx.Data WebApp/src/Qlgx.Api WebApp/tests/Qlgx.Data.Tests
git commit -m "Them boi canh giao xu va bo loc du lieu toan cuc

Moi truy van nghiep vu tu loc theo giao_xu_id. Cong cu chuyen doi du lieu chay khong co
boi canh nen van ghi duoc cho nhieu giao xu. Khi gom cum ve sau chi doi cach lay gia tri
sang claim cua nguoi dang nhap, tang truy van khong phai sua.

Co-Authored-By: Claude Opus 5 (1M context) <noreply@anthropic.com>"
```

---

## Task 6: API danh sách gia đình

Màn hình danh sách hiển thị 12 cột, trong đó **6 cột không có thật trong bảng** mà do view
Access `SELECT_GIADINH_LIST` tính ra: `TenChong`, `TenVo`, `DTChong`, `DTVo`, `SoLuong`,
`GACH`. Bản web tính chúng ở tầng dịch vụ bằng LINQ thay vì mang theo view Access.

Ý nghĩa cột `GACH` (điều khiển gạch ngang đỏ trên lưới): `0` = người nam đã qua đời,
`1` = người nữ đã qua đời, `2` = cả hai, `-1` = không gạch.

**Files:**
- Create: `WebApp/src/Qlgx.Api/Dtos/GiaDinhDtos.cs`
- Create: `WebApp/src/Qlgx.Api/Services/GiaDinhService.cs`
- Create: `WebApp/src/Qlgx.Api/Endpoints/GiaDinhEndpoints.cs`
- Modify: `WebApp/src/Qlgx.Api/Program.cs`
- Test: `WebApp/tests/Qlgx.Api.Tests/GiaDinhListTests.cs`
- Test: `WebApp/tests/Qlgx.Api.Tests/QlgxApiFactory.cs`

**Interfaces:**
- Consumes: `QlgxDbContext`, `IBoiCanhGiaoXu` của Task 5.
- Produces:
  - `record GiaDinhListItemDto(Guid Id, int MaGiaDinhCu, string? MaGiaDinhRieng, string? TenGiaDinh, string? TenChong, string? TenVo, int SoLuong, string? DienThoai, string? DTChong, string? DTVo, string? DiaChi, string? TenGiaoHo, string? DienGiaDinh, string? GhiChu, int Gach, bool KhongThongKe)`
  - `class GiaDinhService(QlgxDbContext db)` với `Task<List<GiaDinhListItemDto>> LayDanhSach(Guid? giaoHoId, bool chiKhongThongKe, CancellationToken ct)`
  - `GET /api/gia-dinh?giaoHoId={uuid}&chiKhongThongKe={bool}`

- [ ] **Bước 1: Viết factory test dùng database thật**

Tạo `WebApp/tests/Qlgx.Api.Tests/QlgxApiFactory.cs`:

```csharp
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
        _goc = Environment.GetEnvironmentVariable("QLGX_TEST_PG")
            ?? "Host=localhost;Username=postgres;Password=postgres";

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
```

- [ ] **Bước 2: Viết test thất bại**

Tạo `WebApp/tests/Qlgx.Api.Tests/GiaDinhListTests.cs`:

```csharp
using System.Net.Http.Json;
using FluentAssertions;
using Qlgx.Domain;
using Qlgx.Domain.Entities;

namespace Qlgx.Api.Tests;

public class GiaDinhListTests(QlgxApiFactory app) : IClassFixture<QlgxApiFactory>
{
    private sealed record Item(Guid Id, int MaGiaDinhCu, string? TenGiaDinh, string? TenChong,
        string? TenVo, int SoLuong, string? DTChong, string? DTVo, string? TenGiaoHo, int Gach);

    private async Task<Guid> TaoGiaDinhMau(bool chongQuaDoi = false, bool voQuaDoi = false,
        string tenGiaoHo = "Giao ho Thanh Tam", int ma = 12)
    {
        await using var db = app.TaoContextThuan();
        var giaoHo = new GiaoHo { GiaoXuId = app.GiaoXuId, TenGiaoHo = tenGiaoHo, MaGiaoHoCu = ma };
        var gd = new GiaDinh { GiaoXuId = app.GiaoXuId, MaGiaDinhCu = ma, TenGiaDinh = "Binh - Lan", GiaoHo = giaoHo };
        var chong = new GiaoDan { GiaoXuId = app.GiaoXuId, MaGiaoDanCu = ma * 10 + 1, HoTen = "Tran Van Binh",
            TenThanh = "Giuse", Phai = "Nam", DienThoai = "0912 345 678", QuaDoi = chongQuaDoi };
        var vo = new GiaoDan { GiaoXuId = app.GiaoXuId, MaGiaoDanCu = ma * 10 + 2, HoTen = "Nguyen Thi Lan",
            TenThanh = "Maria", Phai = "Nu", DienThoai = "0987 114 220", QuaDoi = voQuaDoi };
        var con = new GiaoDan { GiaoXuId = app.GiaoXuId, MaGiaoDanCu = ma * 10 + 3, HoTen = "Tran Minh Khoi",
            TenThanh = "Giuse", Phai = "Nam" };
        db.AddRange(giaoHo, gd, chong, vo, con);
        db.ThanhVienGiaDinh.AddRange(
            new ThanhVienGiaDinh { GiaoXuId = app.GiaoXuId, GiaDinh = gd, GiaoDan = chong, VaiTro = VaiTroGiaDinh.Chong },
            new ThanhVienGiaDinh { GiaoXuId = app.GiaoXuId, GiaDinh = gd, GiaoDan = vo, VaiTro = VaiTroGiaDinh.Vo },
            new ThanhVienGiaDinh { GiaoXuId = app.GiaoXuId, GiaDinh = gd, GiaoDan = con, VaiTro = VaiTroGiaDinh.Con });
        await db.SaveChangesAsync();
        return gd.Id;
    }

    [Fact]
    public async Task Danh_sach_tra_ve_ten_chong_ten_vo_va_so_nhan_khau()
    {
        await TaoGiaDinhMau(ma: 12);

        var ds = await app.CreateClient().GetFromJsonAsync<List<Item>>("/api/gia-dinh");

        var dong = ds!.Single(x => x.MaGiaDinhCu == 12);
        dong.TenChong.Should().Be("Giuse Tran Van Binh");
        dong.TenVo.Should().Be("Maria Nguyen Thi Lan");
        dong.SoLuong.Should().Be(3, "chong, vo va mot nguoi con");
        dong.DTChong.Should().Be("0912 345 678");
        dong.TenGiaoHo.Should().Be("Giao ho Thanh Tam");
    }

    [Theory]
    [InlineData(false, false, -1)]
    [InlineData(true, false, 0)]
    [InlineData(false, true, 1)]
    [InlineData(true, true, 2)]
    public async Task Cot_gach_bao_dung_ai_da_qua_doi(bool chongMat, bool voMat, int gachMongDoi)
    {
        var ma = 20 + gachMongDoi + 2;
        await TaoGiaDinhMau(chongMat, voMat, ma: ma);

        var ds = await app.CreateClient().GetFromJsonAsync<List<Item>>("/api/gia-dinh");

        ds!.Single(x => x.MaGiaDinhCu == ma).Gach.Should().Be(gachMongDoi);
    }

    [Fact]
    public async Task Loc_theo_giao_ho_chi_tra_gia_dinh_cua_giao_ho_do()
    {
        await TaoGiaDinhMau(tenGiaoHo: "Giao ho Fatima", ma: 35);
        await using var db = app.TaoContextThuan();
        var giaoHoFatima = db.GiaoHo.Single(x => x.TenGiaoHo == "Giao ho Fatima");

        var ds = await app.CreateClient()
            .GetFromJsonAsync<List<Item>>($"/api/gia-dinh?giaoHoId={giaoHoFatima.Id}");

        ds!.Should().OnlyContain(x => x.TenGiaoHo == "Giao ho Fatima");
        ds.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Gia_dinh_da_xoa_mem_khong_hien_trong_danh_sach()
    {
        var id = await TaoGiaDinhMau(ma: 99);
        await using (var db = app.TaoContextThuan())
        {
            var gd = db.GiaDinh.Single(x => x.Id == id);
            gd.DaXoa = true;
            await db.SaveChangesAsync();
        }

        var ds = await app.CreateClient().GetFromJsonAsync<List<Item>>("/api/gia-dinh");

        ds!.Should().NotContain(x => x.MaGiaDinhCu == 99);
    }
}
```

- [ ] **Bước 3: Chạy test để xác nhận thất bại**

Chạy: `dotnet test WebApp/tests/Qlgx.Api.Tests --filter GiaDinhListTests`
Kỳ vọng: FAIL — 404 vì endpoint chưa có.

- [ ] **Bước 4: Viết DTO**

Tạo `WebApp/src/Qlgx.Api/Dtos/GiaDinhDtos.cs`:

```csharp
namespace Qlgx.Api.Dtos;

/// <summary>
/// Một dòng trên lưới danh sách gia đình. Sáu trường TenChong, TenVo, DTChong, DTVo,
/// SoLuong, Gach không có trong bảng gia_dinh mà được tính ở tầng dịch vụ — bản Access
/// tính chúng trong view SELECT_GIADINH_LIST.
/// </summary>
public record GiaDinhListItemDto(
    Guid Id,
    int MaGiaDinhCu,
    string? MaGiaDinhRieng,
    string? TenGiaDinh,
    string? TenChong,
    string? TenVo,
    int SoLuong,
    string? DienThoai,
    string? DTChong,
    string? DTVo,
    string? DiaChi,
    string? TenGiaoHo,
    string? DienGiaDinh,
    string? GhiChu,
    int Gach,
    bool KhongThongKe);
```

- [ ] **Bước 5: Viết dịch vụ**

Tạo `WebApp/src/Qlgx.Api/Services/GiaDinhService.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using Qlgx.Api.Dtos;
using Qlgx.Data;
using Qlgx.Domain;

namespace Qlgx.Api.Services;

public class GiaDinhService(QlgxDbContext db)
{
    public async Task<List<GiaDinhListItemDto>> LayDanhSach(
        Guid? giaoHoId, bool chiKhongThongKe, CancellationToken ct)
    {
        var truyVan = db.GiaDinh.Where(g => !g.DaXoa);

        if (giaoHoId is { } id) truyVan = truyVan.Where(g => g.GiaoHoId == id);
        if (chiKhongThongKe) truyVan = truyVan.Where(g => g.KhongThongKe);

        return await truyVan
            .OrderBy(g => g.MaGiaDinhCu)
            .Select(g => new GiaDinhListItemDto(
                g.Id,
                g.MaGiaDinhCu,
                g.MaGiaDinhRieng,
                g.TenGiaDinh,
                GhepTen(g.ThanhVien.FirstOrDefault(tv => tv.VaiTro == VaiTroGiaDinh.Chong)!.GiaoDan),
                GhepTen(g.ThanhVien.FirstOrDefault(tv => tv.VaiTro == VaiTroGiaDinh.Vo)!.GiaoDan),
                g.ThanhVien.Count,
                g.DienThoai,
                g.ThanhVien.Where(tv => tv.VaiTro == VaiTroGiaDinh.Chong)
                    .Select(tv => tv.GiaoDan!.DienThoai).FirstOrDefault(),
                g.ThanhVien.Where(tv => tv.VaiTro == VaiTroGiaDinh.Vo)
                    .Select(tv => tv.GiaoDan!.DienThoai).FirstOrDefault(),
                g.DiaChi,
                g.GiaoHo == null ? "Ngoài xứ" : g.GiaoHo.TenGiaoHo,
                g.DienGiaDinh,
                g.GhiChu,
                TinhGach(
                    g.ThanhVien.Any(tv => tv.VaiTro == VaiTroGiaDinh.Chong && tv.GiaoDan!.QuaDoi),
                    g.ThanhVien.Any(tv => tv.VaiTro == VaiTroGiaDinh.Vo && tv.GiaoDan!.QuaDoi)),
                g.KhongThongKe))
            .ToListAsync(ct);
    }

    /// <summary>Tên hiển thị trên lưới là "Tên thánh + Họ tên", đúng như bản desktop.</summary>
    private static string? GhepTen(Domain.Entities.GiaoDan? gd) =>
        gd == null ? null : string.IsNullOrWhiteSpace(gd.TenThanh) ? gd.HoTen : gd.TenThanh + " " + gd.HoTen;

    /// <summary>0 = chồng mất, 1 = vợ mất, 2 = cả hai, -1 = không gạch.</summary>
    private static int TinhGach(bool chongMat, bool voMat) =>
        chongMat && voMat ? 2 : voMat ? 1 : chongMat ? 0 : -1;
}
```

- [ ] **Bước 6: Viết endpoint và đăng ký dịch vụ**

Tạo `WebApp/src/Qlgx.Api/Endpoints/GiaDinhEndpoints.cs`:

```csharp
using Qlgx.Api.Services;

namespace Qlgx.Api.Endpoints;

public static class GiaDinhEndpoints
{
    public static void MapGiaDinh(this IEndpointRouteBuilder app)
    {
        var nhom = app.MapGroup("/api/gia-dinh");

        nhom.MapGet("", async (GiaDinhService dichVu, Guid? giaoHoId,
            bool? chiKhongThongKe, CancellationToken ct) =>
            Results.Ok(await dichVu.LayDanhSach(giaoHoId, chiKhongThongKe ?? false, ct)));
    }
}
```

Sửa `WebApp/src/Qlgx.Api/Program.cs`, thêm đăng ký và ánh xạ:

```csharp
builder.Services.AddScoped<GiaDinhService>();
// … sau khi build …
app.MapGiaDinh();
```

- [ ] **Bước 7: Chạy test để xác nhận pass**

Chạy: `dotnet test WebApp/tests/Qlgx.Api.Tests`
Kỳ vọng: PASS, 8 test (1 sức khoẻ + 7 danh sách gia đình).

Nếu LINQ báo không dịch được `GhepTen`, thay bằng biểu thức nội tuyến trong `Select` —
EF Core không dịch được lời gọi phương thức tự viết trên thực thể.

- [ ] **Bước 8: Commit**

```bash
git add WebApp/src/Qlgx.Api WebApp/tests/Qlgx.Api.Tests
git commit -m "Them API danh sach gia dinh

Sau cot dan xuat cua view SELECT_GIADINH_LIST (TenChong, TenVo, DTChong, DTVo, SoLuong,
GACH) duoc tinh o tang dich vu bang LINQ thay vi mang theo view dac thu cua Access.

Co-Authored-By: Claude Opus 5 (1M context) <noreply@anthropic.com>"
```

---

## Task 7: API chi tiết gia đình, có chống ghi đè

Đây là chỗ xử lý rủi ro **mới** phát sinh khi bỏ Access: nhiều người trong LAN cùng mở một
gia đình và cùng bấm Cập nhật. Bản desktop dùng `OleDbCommandBuilder` sinh câu UPDATE không
kiểm tra phiên bản nên người lưu sau ghi đè im lặng lên người lưu trước.

**Files:**
- Modify: `WebApp/src/Qlgx.Api/Dtos/GiaDinhDtos.cs`
- Modify: `WebApp/src/Qlgx.Api/Services/GiaDinhService.cs`
- Modify: `WebApp/src/Qlgx.Api/Endpoints/GiaDinhEndpoints.cs`
- Test: `WebApp/tests/Qlgx.Api.Tests/GiaDinhDetailTests.cs`

**Interfaces:**
- Consumes: `GiaDinhService` của Task 6.
- Produces:
  - `record GiaDinhDetailDto(Guid Id, int MaGiaDinhCu, string? MaGiaDinhRieng, string? TenGiaDinh, Guid? GiaoHoId, string? DienThoai, string? DiaChi, string? SoHoKhau, string? DienGiaDinh, string? GhiChu, bool DaChuyenXu, DateOnly? NgayChuyen, string? NoiChuyen, bool KhongThongKe, uint RowVersion, ThanhVienDto[] ThanhVien)`
  - `record ThanhVienDto(Guid GiaoDanId, int VaiTro, bool ChuHo, string? TenThanh, string HoTen, string? Phai, DateOnly? NgaySinh, bool QuaDoi, bool DaXoa)`
  - `record CapNhatGiaDinhRequest(string? TenGiaDinh, Guid? GiaoHoId, string? DienThoai, string? DiaChi, string? SoHoKhau, string? DienGiaDinh, string? GhiChu, bool DaChuyenXu, DateOnly? NgayChuyen, string? NoiChuyen, bool KhongThongKe, uint RowVersion)`
  - `GET /api/gia-dinh/{id}` · `PUT /api/gia-dinh/{id}` (409 khi đụng phiên bản)

- [ ] **Bước 1: Viết test thất bại**

Tạo `WebApp/tests/Qlgx.Api.Tests/GiaDinhDetailTests.cs`:

```csharp
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Qlgx.Domain;
using Qlgx.Domain.Entities;

namespace Qlgx.Api.Tests;

public class GiaDinhDetailTests(QlgxApiFactory app) : IClassFixture<QlgxApiFactory>
{
    private sealed record ThanhVien(Guid GiaoDanId, int VaiTro, bool ChuHo, string? TenThanh,
        string HoTen, string? Phai, DateOnly? NgaySinh, bool QuaDoi);
    private sealed record ChiTiet(Guid Id, int MaGiaDinhCu, string? TenGiaDinh, string? DiaChi,
        bool DaChuyenXu, uint RowVersion, ThanhVien[] ThanhVien);
    private sealed record CapNhat(string? TenGiaDinh, Guid? GiaoHoId, string? DienThoai,
        string? DiaChi, string? SoHoKhau, string? DienGiaDinh, string? GhiChu, bool DaChuyenXu,
        DateOnly? NgayChuyen, string? NoiChuyen, bool KhongThongKe, uint RowVersion);

    private async Task<Guid> TaoGiaDinh(int ma)
    {
        await using var db = app.TaoContextThuan();
        var gd = new GiaDinh { GiaoXuId = app.GiaoXuId, MaGiaDinhCu = ma, TenGiaDinh = "Dung - Thu" };
        var chong = new GiaoDan { GiaoXuId = app.GiaoXuId, MaGiaoDanCu = ma * 10, HoTen = "Vu Tien Dung",
            TenThanh = "Daminh", Phai = "Nam" };
        db.AddRange(gd, chong);
        db.ThanhVienGiaDinh.Add(new ThanhVienGiaDinh { GiaoXuId = app.GiaoXuId, GiaDinh = gd,
            GiaoDan = chong, VaiTro = VaiTroGiaDinh.Chong, ChuHo = true });
        await db.SaveChangesAsync();
        return gd.Id;
    }

    [Fact]
    public async Task Lay_chi_tiet_kem_danh_sach_thanh_vien()
    {
        var id = await TaoGiaDinh(300);

        var ct = await app.CreateClient().GetFromJsonAsync<ChiTiet>($"/api/gia-dinh/{id}");

        ct!.TenGiaDinh.Should().Be("Dung - Thu");
        ct.ThanhVien.Should().ContainSingle();
        ct.ThanhVien[0].HoTen.Should().Be("Vu Tien Dung");
        ct.ThanhVien[0].VaiTro.Should().Be(0);
        ct.ThanhVien[0].ChuHo.Should().BeTrue();
        ct.RowVersion.Should().BeGreaterThan(0u);
    }

    [Fact]
    public async Task Cap_nhat_thanh_cong_khi_dung_phien_ban()
    {
        var id = await TaoGiaDinh(301);
        var client = app.CreateClient();
        var truoc = await client.GetFromJsonAsync<ChiTiet>($"/api/gia-dinh/{id}");

        var res = await client.PutAsJsonAsync($"/api/gia-dinh/{id}", new CapNhat(
            "Dung - Thu (da sua)", null, "028 3775 1120", "7 Hem 24 Tran Phu",
            null, null, null, false, null, null, false, truoc!.RowVersion));

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        var sau = await client.GetFromJsonAsync<ChiTiet>($"/api/gia-dinh/{id}");
        sau!.TenGiaDinh.Should().Be("Dung - Thu (da sua)");
    }

    [Fact]
    public async Task Hai_nguoi_cung_sua_thi_nguoi_sau_nhan_409_thay_vi_ghi_de_im_lang()
    {
        var id = await TaoGiaDinh(302);
        var client = app.CreateClient();
        var banA = await client.GetFromJsonAsync<ChiTiet>($"/api/gia-dinh/{id}");
        var banB = await client.GetFromJsonAsync<ChiTiet>($"/api/gia-dinh/{id}");

        var luuA = await client.PutAsJsonAsync($"/api/gia-dinh/{id}", new CapNhat(
            "Nguoi A sua", null, null, null, null, null, null, false, null, null, false, banA!.RowVersion));
        var luuB = await client.PutAsJsonAsync($"/api/gia-dinh/{id}", new CapNhat(
            "Nguoi B sua", null, null, null, null, null, null, false, null, null, false, banB!.RowVersion));

        luuA.StatusCode.Should().Be(HttpStatusCode.OK);
        luuB.StatusCode.Should().Be(HttpStatusCode.Conflict,
            "ban Access ghi de im lang trong tinh huong nay, ban web phai bao cho nguoi dung");
        var cuoi = await client.GetFromJsonAsync<ChiTiet>($"/api/gia-dinh/{id}");
        cuoi!.TenGiaDinh.Should().Be("Nguoi A sua");
    }

    [Fact]
    public async Task Khong_tim_thay_thi_tra_404()
    {
        var res = await app.CreateClient().GetAsync($"/api/gia-dinh/{Guid.NewGuid()}");

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
```

- [ ] **Bước 2: Chạy test để xác nhận thất bại**

Chạy: `dotnet test WebApp/tests/Qlgx.Api.Tests --filter GiaDinhDetailTests`
Kỳ vọng: FAIL — 404 ở mọi test vì endpoint chi tiết chưa có.

- [ ] **Bước 3: Thêm DTO chi tiết**

Thêm vào cuối `WebApp/src/Qlgx.Api/Dtos/GiaDinhDtos.cs`:

```csharp
public record ThanhVienDto(
    Guid GiaoDanId, int VaiTro, bool ChuHo,
    string? TenThanh, string HoTen, string? Phai, DateOnly? NgaySinh, bool QuaDoi, bool DaXoa);

public record GiaDinhDetailDto(
    Guid Id, int MaGiaDinhCu, string? MaGiaDinhRieng, string? TenGiaDinh, Guid? GiaoHoId,
    string? DienThoai, string? DiaChi, string? SoHoKhau, string? DienGiaDinh, string? GhiChu,
    bool DaChuyenXu, DateOnly? NgayChuyen, string? NoiChuyen, bool KhongThongKe,
    uint RowVersion, ThanhVienDto[] ThanhVien);

public record CapNhatGiaDinhRequest(
    string? TenGiaDinh, Guid? GiaoHoId, string? DienThoai, string? DiaChi, string? SoHoKhau,
    string? DienGiaDinh, string? GhiChu, bool DaChuyenXu, DateOnly? NgayChuyen,
    string? NoiChuyen, bool KhongThongKe, uint RowVersion);
```

- [ ] **Bước 4: Thêm phương thức vào dịch vụ**

Thêm vào `WebApp/src/Qlgx.Api/Services/GiaDinhService.cs`:

```csharp
    public async Task<GiaDinhDetailDto?> LayChiTiet(Guid id, CancellationToken ct)
    {
        var g = await db.GiaDinh
            .Include(x => x.ThanhVien).ThenInclude(tv => tv.GiaoDan)
            .SingleOrDefaultAsync(x => x.Id == id && !x.DaXoa, ct);
        if (g is null) return null;

        return new GiaDinhDetailDto(
            g.Id, g.MaGiaDinhCu, g.MaGiaDinhRieng, g.TenGiaDinh, g.GiaoHoId,
            g.DienThoai, g.DiaChi, g.SoHoKhau, g.DienGiaDinh, g.GhiChu,
            g.DaChuyenXu, g.NgayChuyen, g.NoiChuyen, g.KhongThongKe, g.RowVersion,
            g.ThanhVien
                .OrderBy(tv => tv.VaiTro).ThenBy(tv => tv.GiaoDan!.NgaySinh)
                .Select(tv => new ThanhVienDto(
                    tv.GiaoDanId, (int)tv.VaiTro, tv.ChuHo,
                    tv.GiaoDan!.TenThanh, tv.GiaoDan.HoTen, tv.GiaoDan.Phai,
                    tv.GiaoDan.NgaySinh, tv.GiaoDan.QuaDoi, tv.GiaoDan.DaXoa))
                .ToArray());
    }

    /// <summary>Trả về false khi bản ghi đã bị người khác sửa từ lúc màn hình được mở.</summary>
    public async Task<bool?> CapNhat(Guid id, CapNhatGiaDinhRequest yeuCau, CancellationToken ct)
    {
        var g = await db.GiaDinh.SingleOrDefaultAsync(x => x.Id == id && !x.DaXoa, ct);
        if (g is null) return null;

        db.Entry(g).Property(x => x.RowVersion).OriginalValue = yeuCau.RowVersion;

        g.TenGiaDinh = yeuCau.TenGiaDinh;
        g.GiaoHoId = yeuCau.GiaoHoId;
        g.DienThoai = yeuCau.DienThoai;
        g.DiaChi = yeuCau.DiaChi;
        g.SoHoKhau = yeuCau.SoHoKhau;
        g.DienGiaDinh = yeuCau.DienGiaDinh;
        g.GhiChu = yeuCau.GhiChu;
        g.DaChuyenXu = yeuCau.DaChuyenXu;
        g.NgayChuyen = yeuCau.NgayChuyen;
        g.NoiChuyen = yeuCau.NoiChuyen;
        g.KhongThongKe = yeuCau.KhongThongKe;

        try
        {
            await db.SaveChangesAsync(ct);
            return true;
        }
        catch (DbUpdateConcurrencyException)
        {
            return false;
        }
    }
```

- [ ] **Bước 5: Thêm endpoint**

Thêm vào `MapGiaDinh` trong `WebApp/src/Qlgx.Api/Endpoints/GiaDinhEndpoints.cs`:

```csharp
        nhom.MapGet("/{id:guid}", async (GiaDinhService dichVu, Guid id, CancellationToken ct) =>
            await dichVu.LayChiTiet(id, ct) is { } ct2 ? Results.Ok(ct2) : Results.NotFound());

        nhom.MapPut("/{id:guid}", async (GiaDinhService dichVu, Guid id,
            CapNhatGiaDinhRequest yeuCau, CancellationToken ct) =>
            await dichVu.CapNhat(id, yeuCau, ct) switch
            {
                null => Results.NotFound(),
                false => Results.Conflict(new
                {
                    thongBao = "Gia đình này vừa được người khác cập nhật. " +
                               "Hãy tải lại màn hình để xem thay đổi mới nhất rồi sửa lại."
                }),
                true => Results.Ok()
            });
```

Nhớ thêm `using Qlgx.Api.Dtos;` ở đầu file.

- [ ] **Bước 6: Chạy test để xác nhận pass**

Chạy: `dotnet test WebApp/tests/Qlgx.Api.Tests`
Kỳ vọng: PASS, 12 test.

- [ ] **Bước 7: Commit**

```bash
git add WebApp/src/Qlgx.Api WebApp/tests/Qlgx.Api.Tests
git commit -m "Them API chi tiet gia dinh kem chong ghi de

Ban Access dung OleDbCommandBuilder sinh cau UPDATE khong kiem tra phien ban nen hai nguoi
cung sua thi nguoi luu sau ghi de im lang. Ban web tra 409 kem thong bao tieng Viet.

Co-Authored-By: Claude Opus 5 (1M context) <noreply@anthropic.com>"
```

---

## Task 8: API giáo dân — danh sách và chi tiết

Lưới giáo dân có **29 cột** theo đúng `GxGiaoDanList.FormatGrid()`. Ba cột không có trong
bảng mà do câu SQL `SELECT_GIAODAN_LIST_CO_GIAOHO` tính: `TenGiaoHo`, `NamSinh` (4 ký tự
cuối của chuỗi ngày sinh) và `DaChuyenXu`. Ở bản web `NamSinh` suy từ `NgaySinh` kiểu date,
còn `DaChuyenXu` lấy thẳng từ cột đã có trên gia đình của người đó.

**Files:**
- Create: `WebApp/src/Qlgx.Api/Dtos/GiaoDanDtos.cs`
- Create: `WebApp/src/Qlgx.Api/Services/GiaoDanService.cs`
- Create: `WebApp/src/Qlgx.Api/Endpoints/GiaoDanEndpoints.cs`
- Modify: `WebApp/src/Qlgx.Api/Program.cs`
- Test: `WebApp/tests/Qlgx.Api.Tests/GiaoDanTests.cs`

**Interfaces:**
- Consumes: `QlgxDbContext`, `QlgxApiFactory` của các task trước.
- Produces:
  - `record GiaoDanListItemDto` — 29 trường tương ứng 29 cột của lưới, cộng `Id` và `QuanHe`
    (`QuanHe` chỉ có giá trị khi lấy qua endpoint thành viên gia đình).
  - `record GiaoDanDetailDto` — toàn bộ trường của thực thể `GiaoDan`, cộng `RowVersion`, `GiaDinhId`, `TenGiaDinh`, `VaiTro`.
  - `class GiaoDanService(QlgxDbContext db)` với `LayDanhSach(Guid? giaoHoId, bool chiKhongThongKe, CancellationToken)`, `LayChiTiet(Guid, CancellationToken)`, `CapNhat(Guid, CapNhatGiaoDanRequest, CancellationToken)`.
  - `GET /api/giao-dan` · `GET /api/giao-dan/{id}` · `PUT /api/giao-dan/{id}` · `GET /api/gia-dinh/{id}/thanh-vien`

- [ ] **Bước 1: Viết test thất bại**

Tạo `WebApp/tests/Qlgx.Api.Tests/GiaoDanTests.cs`:

```csharp
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Qlgx.Domain;
using Qlgx.Domain.Entities;

namespace Qlgx.Api.Tests;

public class GiaoDanTests(QlgxApiFactory app) : IClassFixture<QlgxApiFactory>
{
    private sealed record Item(Guid Id, int MaGiaoDanCu, string? TenThanh, string HoTen,
        string? Phai, DateOnly? NgaySinh, string? NamSinh, DateOnly? NgayRuaToi,
        bool QuaDoi, bool DaChuyenDi, string? TenGiaoHo, bool LapGd);
    private sealed record ChiTiet(Guid Id, string HoTen, string? TenThanh, DateOnly? NgaySinh,
        DateOnly? NgayRuaToi, string? NoiRuaToi, bool QuaDoi, DateOnly? NgayQuaDoi,
        Guid? GiaDinhId, string? TenGiaDinh, int? VaiTro, uint RowVersion);

    private async Task<Guid> TaoGiaoDan(int ma, string hoTen, bool quaDoi = false,
        DateOnly? ngaySinh = null)
    {
        await using var db = app.TaoContextThuan();
        var gd = new GiaoDan
        {
            GiaoXuId = app.GiaoXuId, MaGiaoDanCu = ma, HoTen = hoTen, TenThanh = "Giuse",
            Phai = "Nam", QuaDoi = quaDoi, NgaySinh = ngaySinh,
            NgayRuaToi = new DateOnly(1996, 4, 21), NoiRuaToi = "GX Thanh Tam"
        };
        db.GiaoDan.Add(gd);
        await db.SaveChangesAsync();
        return gd.Id;
    }

    [Fact]
    public async Task Danh_sach_suy_ra_nam_sinh_tu_ngay_sinh()
    {
        await TaoGiaoDan(8001, "Vu Minh Tri", ngaySinh: new DateOnly(1996, 4, 2));

        var ds = await app.CreateClient().GetFromJsonAsync<List<Item>>("/api/giao-dan");

        ds!.Single(x => x.MaGiaoDanCu == 8001).NamSinh.Should().Be("1996");
    }

    [Fact]
    public async Task Danh_sach_de_trong_nam_sinh_khi_khong_co_ngay_sinh()
    {
        await TaoGiaoDan(8002, "Nguoi khong ro ngay sinh");

        var ds = await app.CreateClient().GetFromJsonAsync<List<Item>>("/api/giao-dan");

        ds!.Single(x => x.MaGiaoDanCu == 8002).NamSinh.Should().BeEmpty();
    }

    [Fact]
    public async Task Chi_tiet_tra_ve_gia_dinh_va_vai_tro_cua_nguoi_do()
    {
        var idNguoi = await TaoGiaoDan(8003, "Vu Tien Dung");
        await using (var db = app.TaoContextThuan())
        {
            var giaDinh = new GiaDinh { GiaoXuId = app.GiaoXuId, MaGiaDinhCu = 8003, TenGiaDinh = "Dung - Thu" };
            db.GiaDinh.Add(giaDinh);
            db.ThanhVienGiaDinh.Add(new ThanhVienGiaDinh
            {
                GiaoXuId = app.GiaoXuId, GiaDinhId = giaDinh.Id, GiaoDanId = idNguoi,
                VaiTro = VaiTroGiaDinh.Chong, ChuHo = true
            });
            await db.SaveChangesAsync();
        }

        var ct = await app.CreateClient().GetFromJsonAsync<ChiTiet>($"/api/giao-dan/{idNguoi}");

        ct!.TenGiaDinh.Should().Be("Dung - Thu");
        ct.VaiTro.Should().Be(0);
    }

    [Fact]
    public async Task Lay_duoc_thanh_vien_cua_mot_gia_dinh_qua_endpoint_rieng()
    {
        var idCon = await TaoGiaoDan(8004, "Vu Duc Duy");
        Guid idGiaDinh;
        await using (var db = app.TaoContextThuan())
        {
            var giaDinh = new GiaDinh { GiaoXuId = app.GiaoXuId, MaGiaDinhCu = 8004, TenGiaDinh = "Gia dinh co con" };
            db.GiaDinh.Add(giaDinh);
            db.ThanhVienGiaDinh.Add(new ThanhVienGiaDinh
            {
                GiaoXuId = app.GiaoXuId, GiaDinhId = giaDinh.Id, GiaoDanId = idCon,
                VaiTro = VaiTroGiaDinh.Con
            });
            await db.SaveChangesAsync();
            idGiaDinh = giaDinh.Id;
        }

        var ds = await app.CreateClient()
            .GetFromJsonAsync<List<Item>>($"/api/gia-dinh/{idGiaDinh}/thanh-vien");

        ds!.Should().ContainSingle().Which.HoTen.Should().Be("Vu Duc Duy");
    }

    [Fact]
    public async Task Cap_nhat_giao_dan_kiem_tra_phien_ban()
    {
        var id = await TaoGiaoDan(8005, "Nguoi se duoc sua");
        var client = app.CreateClient();
        var truoc = await client.GetFromJsonAsync<ChiTiet>($"/api/giao-dan/{id}");

        var lanDau = await client.PutAsJsonAsync($"/api/giao-dan/{id}",
            new { HoTen = "Ten da sua", RowVersion = truoc!.RowVersion });
        var lanHai = await client.PutAsJsonAsync($"/api/giao-dan/{id}",
            new { HoTen = "Ten sua lan hai", RowVersion = truoc.RowVersion });

        lanDau.StatusCode.Should().Be(HttpStatusCode.OK);
        lanHai.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}
```

- [ ] **Bước 2: Chạy test để xác nhận thất bại**

Chạy: `dotnet test WebApp/tests/Qlgx.Api.Tests --filter GiaoDanTests`
Kỳ vọng: FAIL — 404 ở mọi test.

- [ ] **Bước 3: Viết DTO**

Tạo `WebApp/src/Qlgx.Api/Dtos/GiaoDanDtos.cs`:

```csharp
namespace Qlgx.Api.Dtos;

/// <summary>Một dòng trên lưới giáo dân — 29 cột theo đúng GxGiaoDanList.FormatGrid().</summary>
public record GiaoDanListItemDto(
    Guid Id, int MaGiaoDanCu, string? TenThanh, string HoTen, string? Phai,
    DateOnly? NgaySinh, string NamSinh,
    DateOnly? NgayRuaToi, DateOnly? NgayRuocLe, DateOnly? NgayThemSuc,
    bool LapGd, string? HoTenCha, string? HoTenMe, bool TanTong, bool ConHoc,
    string? NgheNghiep, string? GhiChu, string? DienThoai, string? DiaChi,
    string? TenGiaoHo, bool DaChuyenDi, string? TrinhDoVanHoa, string? TrinhDoChuyenMon,
    string? BietNgoaiNgu, bool QuaDoi, DateOnly? NgayQuaDoi, string? NoiAnTang,
    string? NoiSinh, string? NoiRuaToi, string? NoiRuocLe, string? NoiThemSuc,
    /// <summary>Chỉ có giá trị khi lưới nhúng trong form gia đình.</summary>
    string? QuanHe);

public record GiaoDanDetailDto(
    Guid Id, int MaGiaoDanCu, string HoTen, string? TenThanh, string? Phai,
    DateOnly? NgaySinh, string? NoiSinh, string? CMND, string? DanToc,
    Guid? GiaoHoId, string? ThuocGiaoXu, string? ThuocGiaoPhan,
    string? DiaChi, string? DienThoai, string? Email,
    string? HoTenCha, string? HoTenMe,
    string? SoRuaToi, DateOnly? NgayRuaToi, string? NoiRuaToi, string? ChaRuaToi, string? NguoiDoDauRuaToi,
    string? SoRuocLe, DateOnly? NgayRuocLe, string? NoiRuocLe, string? ChaRuocLe,
    string? SoThemSuc, DateOnly? NgayThemSuc, string? NoiThemSuc, string? ChaThemSuc, string? NguoiDoDauThemSuc,
    DateOnly? NgayXucDau, string? NguoiXucDau, string? TinhTrangXucDau, string? GhiChuXucDau,
    DateOnly? NgayBD1, string? NoiBD1, DateOnly? NgayBD2, string? NoiBD2,
    DateOnly? NgayTHVaoDoi, string? NoiTHVaoDoi,
    DateOnly? NgayGLHN1, DateOnly? NgayGLHN2, string? NoiGLHN, string? NguoiChungNhanGLHN, string? XepLoaiGLHN,
    string? TrinhDoVanHoa, string? TrinhDoChuyenMon, string? BietNgoaiNgu, string? NgheNghiep, bool ConHoc,
    bool DaCoGiaDinh, bool TanTong, bool KhongThongKe,
    bool QuaDoi, DateOnly? NgayQuaDoi, string? NoiQuaDoi, string? SoAnTang, string? NoiAnTang,
    string? GhiChu,
    Guid? GiaDinhId, string? TenGiaDinh, int? VaiTro,
    uint RowVersion);

/// <summary>
/// Chỉ những trường màn hình chi tiết cho sửa. Các trường còn lại của thực thể không nhận
/// từ client để tránh sửa nhầm dữ liệu do công cụ chuyển đổi sinh ra.
/// </summary>
public record CapNhatGiaoDanRequest(
    string HoTen, string? TenThanh, string? Phai, DateOnly? NgaySinh, string? NoiSinh,
    string? CMND, string? DanToc, Guid? GiaoHoId, string? DiaChi, string? DienThoai, string? Email,
    string? HoTenCha, string? HoTenMe,
    string? SoRuaToi, DateOnly? NgayRuaToi, string? NoiRuaToi, string? ChaRuaToi, string? NguoiDoDauRuaToi,
    string? SoRuocLe, DateOnly? NgayRuocLe, string? NoiRuocLe, string? ChaRuocLe,
    string? SoThemSuc, DateOnly? NgayThemSuc, string? NoiThemSuc, string? ChaThemSuc, string? NguoiDoDauThemSuc,
    DateOnly? NgayXucDau, string? NguoiXucDau, string? TinhTrangXucDau, string? GhiChuXucDau,
    string? TrinhDoVanHoa, string? TrinhDoChuyenMon, string? BietNgoaiNgu, string? NgheNghiep,
    bool ConHoc, bool DaCoGiaDinh, bool TanTong, bool KhongThongKe,
    bool QuaDoi, DateOnly? NgayQuaDoi, string? NoiQuaDoi, string? SoAnTang, string? NoiAnTang,
    string? GhiChu, uint RowVersion);
```

- [ ] **Bước 4: Viết dịch vụ**

Tạo `WebApp/src/Qlgx.Api/Services/GiaoDanService.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using Qlgx.Api.Dtos;
using Qlgx.Data;
using Qlgx.Domain.Entities;

namespace Qlgx.Api.Services;

public class GiaoDanService(QlgxDbContext db)
{
    /// <summary>
    /// Một dòng nguồn trước khi dựng DTO. Có thêm QuanHe vì lưới thành viên trong form gia
    /// đình cần cột đó, còn danh sách giáo dân thì không.
    /// </summary>
    private record NguonDong(GiaoDan Gd, string? QuanHe);

    public Task<List<GiaoDanListItemDto>> LayDanhSach(
        Guid? giaoHoId, bool chiKhongThongKe, CancellationToken ct)
    {
        var truyVan = db.GiaoDan.Where(g => !g.DaXoa);
        if (giaoHoId is { } id) truyVan = truyVan.Where(g => g.GiaoHoId == id);
        if (chiKhongThongKe) truyVan = truyVan.Where(g => g.KhongThongKe);

        return DungDanhSach(truyVan
            .OrderBy(g => g.MaGiaoDanCu)
            .Select(g => new NguonDong(g, null))).ToListAsync(ct);
    }

    /// <summary>Thành viên của một gia đình — cùng bộ cột với danh sách giáo dân.</summary>
    public Task<List<GiaoDanListItemDto>> LayThanhVien(Guid giaDinhId, CancellationToken ct) =>
        DungDanhSach(db.ThanhVienGiaDinh
            .Where(tv => tv.GiaDinhId == giaDinhId)
            .OrderBy(tv => tv.VaiTro).ThenBy(tv => tv.GiaoDan!.NgaySinh)
            .Select(tv => new NguonDong(
                tv.GiaoDan!,
                tv.VaiTro == VaiTroGiaDinh.Chong ? "Chồng"
                    : tv.VaiTro == VaiTroGiaDinh.Vo ? "Vợ" : "Con"))).ToListAsync(ct);

    private static IQueryable<GiaoDanListItemDto> DungDanhSach(IQueryable<NguonDong> nguon) =>
        nguon.Select(n => new GiaoDanListItemDto(
            n.Gd.Id, n.Gd.MaGiaoDanCu, n.Gd.TenThanh, n.Gd.HoTen, n.Gd.Phai,
            n.Gd.NgaySinh,
            // Bản Access lấy 4 ký tự cuối của chuỗi ngày; ở đây suy thẳng từ kiểu date
            n.Gd.NgaySinh == null ? "" : n.Gd.NgaySinh.Value.Year.ToString(),
            n.Gd.NgayRuaToi, n.Gd.NgayRuocLe, n.Gd.NgayThemSuc,
            n.Gd.DaCoGiaDinh, n.Gd.HoTenCha, n.Gd.HoTenMe, n.Gd.TanTong, n.Gd.ConHoc,
            n.Gd.NgheNghiep, n.Gd.GhiChu, n.Gd.DienThoai, n.Gd.DiaChi,
            n.Gd.GiaoHo == null ? "Ngoài xứ" : n.Gd.GiaoHo.TenGiaoHo,
            n.Gd.GiaDinhThamGia.Any(tv => tv.GiaDinh!.DaChuyenXu),
            n.Gd.TrinhDoVanHoa, n.Gd.TrinhDoChuyenMon, n.Gd.BietNgoaiNgu,
            n.Gd.QuaDoi, n.Gd.NgayQuaDoi, n.Gd.NoiAnTang,
            n.Gd.NoiSinh, n.Gd.NoiRuaToi, n.Gd.NoiRuocLe, n.Gd.NoiThemSuc,
            n.QuanHe));

    public async Task<GiaoDanDetailDto?> LayChiTiet(Guid id, CancellationToken ct)
    {
        var g = await db.GiaoDan
            .Include(x => x.GiaDinhThamGia).ThenInclude(tv => tv.GiaDinh)
            .SingleOrDefaultAsync(x => x.Id == id && !x.DaXoa, ct);
        if (g is null) return null;

        // Một giáo dân có thể thuộc nhiều gia đình; màn hình chi tiết hiển thị gia đình mà
        // người đó là chồng hoặc vợ, nếu không có thì lấy gia đình đầu tiên.
        var thamGia = g.GiaDinhThamGia
            .OrderBy(tv => tv.VaiTro == Domain.VaiTroGiaDinh.Con ? 1 : 0)
            .FirstOrDefault();

        return new GiaoDanDetailDto(
            g.Id, g.MaGiaoDanCu, g.HoTen, g.TenThanh, g.Phai, g.NgaySinh, g.NoiSinh, g.CMND, g.DanToc,
            g.GiaoHoId, g.ThuocGiaoXu, g.ThuocGiaoPhan, g.DiaChi, g.DienThoai, g.Email,
            g.HoTenCha, g.HoTenMe,
            g.SoRuaToi, g.NgayRuaToi, g.NoiRuaToi, g.ChaRuaToi, g.NguoiDoDauRuaToi,
            g.SoRuocLe, g.NgayRuocLe, g.NoiRuocLe, g.ChaRuocLe,
            g.SoThemSuc, g.NgayThemSuc, g.NoiThemSuc, g.ChaThemSuc, g.NguoiDoDauThemSuc,
            g.NgayXucDau, g.NguoiXucDau, g.TinhTrangXucDau, g.GhiChuXucDau,
            g.NgayBD1, g.NoiBD1, g.NgayBD2, g.NoiBD2, g.NgayTHVaoDoi, g.NoiTHVaoDoi,
            g.NgayGLHN1, g.NgayGLHN2, g.NoiGLHN, g.NguoiChungNhanGLHN, g.XepLoaiGLHN,
            g.TrinhDoVanHoa, g.TrinhDoChuyenMon, g.BietNgoaiNgu, g.NgheNghiep, g.ConHoc,
            g.DaCoGiaDinh, g.TanTong, g.KhongThongKe,
            g.QuaDoi, g.NgayQuaDoi, g.NoiQuaDoi, g.SoAnTang, g.NoiAnTang, g.GhiChu,
            thamGia?.GiaDinhId, thamGia?.GiaDinh?.TenGiaDinh, thamGia is null ? null : (int)thamGia.VaiTro,
            g.RowVersion);
    }

    public async Task<bool?> CapNhat(Guid id, CapNhatGiaoDanRequest r, CancellationToken ct)
    {
        var g = await db.GiaoDan.SingleOrDefaultAsync(x => x.Id == id && !x.DaXoa, ct);
        if (g is null) return null;

        db.Entry(g).Property(x => x.RowVersion).OriginalValue = r.RowVersion;

        g.HoTen = r.HoTen; g.TenThanh = r.TenThanh; g.Phai = r.Phai;
        g.NgaySinh = r.NgaySinh; g.NoiSinh = r.NoiSinh; g.CMND = r.CMND; g.DanToc = r.DanToc;
        g.GiaoHoId = r.GiaoHoId; g.DiaChi = r.DiaChi; g.DienThoai = r.DienThoai; g.Email = r.Email;
        g.HoTenCha = r.HoTenCha; g.HoTenMe = r.HoTenMe;
        g.SoRuaToi = r.SoRuaToi; g.NgayRuaToi = r.NgayRuaToi; g.NoiRuaToi = r.NoiRuaToi;
        g.ChaRuaToi = r.ChaRuaToi; g.NguoiDoDauRuaToi = r.NguoiDoDauRuaToi;
        g.SoRuocLe = r.SoRuocLe; g.NgayRuocLe = r.NgayRuocLe; g.NoiRuocLe = r.NoiRuocLe; g.ChaRuocLe = r.ChaRuocLe;
        g.SoThemSuc = r.SoThemSuc; g.NgayThemSuc = r.NgayThemSuc; g.NoiThemSuc = r.NoiThemSuc;
        g.ChaThemSuc = r.ChaThemSuc; g.NguoiDoDauThemSuc = r.NguoiDoDauThemSuc;
        g.NgayXucDau = r.NgayXucDau; g.NguoiXucDau = r.NguoiXucDau;
        g.TinhTrangXucDau = r.TinhTrangXucDau; g.GhiChuXucDau = r.GhiChuXucDau;
        g.TrinhDoVanHoa = r.TrinhDoVanHoa; g.TrinhDoChuyenMon = r.TrinhDoChuyenMon;
        g.BietNgoaiNgu = r.BietNgoaiNgu; g.NgheNghiep = r.NgheNghiep; g.ConHoc = r.ConHoc;
        g.DaCoGiaDinh = r.DaCoGiaDinh; g.TanTong = r.TanTong; g.KhongThongKe = r.KhongThongKe;
        g.QuaDoi = r.QuaDoi; g.NgayQuaDoi = r.NgayQuaDoi; g.NoiQuaDoi = r.NoiQuaDoi;
        g.SoAnTang = r.SoAnTang; g.NoiAnTang = r.NoiAnTang; g.GhiChu = r.GhiChu;

        // Liên động của frmGiaoDan: tick "Qua đời" thì tự bỏ tick "Còn học"
        if (g.QuaDoi) g.ConHoc = false;

        try { await db.SaveChangesAsync(ct); return true; }
        catch (DbUpdateConcurrencyException) { return false; }
    }
}
```

- [ ] **Bước 5: Viết endpoint**

Tạo `WebApp/src/Qlgx.Api/Endpoints/GiaoDanEndpoints.cs`:

```csharp
using Qlgx.Api.Dtos;
using Qlgx.Api.Services;

namespace Qlgx.Api.Endpoints;

public static class GiaoDanEndpoints
{
    public static void MapGiaoDan(this IEndpointRouteBuilder app)
    {
        var nhom = app.MapGroup("/api/giao-dan");

        nhom.MapGet("", async (GiaoDanService dv, Guid? giaoHoId, bool? chiKhongThongKe,
            CancellationToken ct) =>
            Results.Ok(await dv.LayDanhSach(giaoHoId, chiKhongThongKe ?? false, ct)));

        nhom.MapGet("/{id:guid}", async (GiaoDanService dv, Guid id, CancellationToken ct) =>
            await dv.LayChiTiet(id, ct) is { } chiTiet ? Results.Ok(chiTiet) : Results.NotFound());

        nhom.MapPut("/{id:guid}", async (GiaoDanService dv, Guid id,
            CapNhatGiaoDanRequest yeuCau, CancellationToken ct) =>
            await dv.CapNhat(id, yeuCau, ct) switch
            {
                null => Results.NotFound(),
                false => Results.Conflict(new
                {
                    thongBao = "Giáo dân này vừa được người khác cập nhật. " +
                               "Hãy tải lại màn hình để xem thay đổi mới nhất rồi sửa lại."
                }),
                true => Results.Ok()
            });

        // Lưới thành viên trong form gia đình dùng chung bộ cột với danh sách giáo dân
        app.MapGet("/api/gia-dinh/{id:guid}/thanh-vien",
            async (GiaoDanService dv, Guid id, CancellationToken ct) =>
                Results.Ok(await dv.LayThanhVien(id, ct)));
    }
}
```

Sửa `Program.cs`: thêm `builder.Services.AddScoped<GiaoDanService>();` và `app.MapGiaoDan();`.

- [ ] **Bước 6: Chạy test để xác nhận pass**

Chạy: `dotnet test WebApp/tests/Qlgx.Api.Tests`
Kỳ vọng: PASS, 17 test.

- [ ] **Bước 7: Commit**

```bash
git add WebApp/src/Qlgx.Api WebApp/tests/Qlgx.Api.Tests
git commit -m "Them API giao dan: danh sach, chi tiet, cap nhat va thanh vien gia dinh

Endpoint thanh vien gia dinh dung chung ham dung DTO voi danh sach giao dan, dung nguyen
tac cua ban desktop: cung mot GxGiaoDanList duoc nhung o hai noi.

Co-Authored-By: Claude Opus 5 (1M context) <noreply@anthropic.com>"
```

---

## Task 9: Công cụ chuyển dữ liệu từ Access sang PostgreSQL

Chạy một lần khi đưa một giáo xứ lên bản web. Đây là project **duy nhất** được phép phụ
thuộc Windows và OleDb.

Ba yêu cầu bắt buộc, xuất phát từ đặc điểm dữ liệu đã khảo sát:
- **Chạy lại nhiều lần không sinh dữ liệu trùng** — giai đoạn thí điểm sẽ chạy đi chạy lại.
- **Chế độ chạy thử** xuất báo cáo trước khi ghi thật.
- **Không đánh mất dữ liệu ngày hỏng** — chuỗi không phân giải được ghi vào `du_lieu_loi`.

**Files:**
- Create: `WebApp/src/Qlgx.Migration/Qlgx.Migration.csproj`, `Program.cs`, `DocAccess.cs`, `BangAnhXaId.cs`, `ChuyenDoiDuLieu.cs`, `BaoCaoDoiChieu.cs`
- Create: `WebApp/tests/Qlgx.Migration.Tests/Qlgx.Migration.Tests.csproj`
- Test: `WebApp/tests/Qlgx.Migration.Tests/ChuyenDoiTests.cs`

**Interfaces:**
- Consumes: `QlgxDbContext` (không có bối cảnh giáo xứ), `NgayThangText` của Task 2.
- Produces:
  - `class BangAnhXaId` với `Guid Lay(string bang, int maCu)` — sinh UUID ổn định theo cặp (bảng, mã cũ) để chạy lại nhiều lần vẫn ra cùng khoá.
  - `class ChuyenDoiDuLieu(QlgxDbContext db, Guid giaoXuId, BangAnhXaId anhXa)` với `Task<KetQuaChuyenDoi> Chay(IDuLieuNguon nguon, bool chayThu, CancellationToken ct)`.
  - `record KetQuaChuyenDoi(Dictionary<string,int> SoDongNguon, Dictionary<string,int> SoDongDich, List<string> CanhBao)`.
  - `interface IDuLieuNguon` — trừu tượng hoá nguồn để test không cần file `.mdb`.

- [ ] **Bước 1: Tạo project**

```bash
cd WebApp
dotnet new console -o src/Qlgx.Migration -f net8.0
dotnet new xunit -o tests/Qlgx.Migration.Tests -f net8.0
dotnet sln add src/Qlgx.Migration tests/Qlgx.Migration.Tests
dotnet add src/Qlgx.Migration reference src/Qlgx.Data
dotnet add src/Qlgx.Migration package System.Data.OleDb
dotnet add tests/Qlgx.Migration.Tests reference src/Qlgx.Migration
dotnet add tests/Qlgx.Migration.Tests reference tests/Qlgx.Data.Tests
dotnet add tests/Qlgx.Migration.Tests package FluentAssertions
```

Thêm vào `src/Qlgx.Migration/Qlgx.Migration.csproj` để nói rõ đây là project Windows-only:

```xml
  <PropertyGroup>
    <TargetFramework>net8.0-windows</TargetFramework>
    <Platforms>x86</Platforms>
    <PlatformTarget>x86</PlatformTarget>
  </PropertyGroup>
```

Build x86 là bắt buộc vì driver Microsoft ACE OLEDB trên nhiều máy giáo xứ là bản 32-bit —
đúng lý do bản desktop hiện tại cũng buộc phải build x86.

- [ ] **Bước 2: Viết test thất bại**

Tạo `WebApp/tests/Qlgx.Migration.Tests/ChuyenDoiTests.cs`:

```csharp
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Qlgx.Data.Tests;
using Qlgx.Domain;

namespace Qlgx.Migration.Tests;

public class ChuyenDoiTests(CoSoDuLieuFixture db) : IClassFixture<CoSoDuLieuFixture>
{
    /// <summary>Nguồn giả lập, thay cho file .mdb để test chạy được trên máy không có Access.</summary>
    private sealed class NguonGia : IDuLieuNguon
    {
        public List<DongGiaoHo> GiaoHo { get; } = [];
        public List<DongGiaDinh> GiaDinh { get; } = [];
        public List<DongGiaoDan> GiaoDan { get; } = [];
        public List<DongThanhVien> ThanhVien { get; } = [];

        IEnumerable<DongGiaoHo> IDuLieuNguon.DocGiaoHo() => GiaoHo;
        IEnumerable<DongGiaDinh> IDuLieuNguon.DocGiaDinh() => GiaDinh;
        IEnumerable<DongGiaoDan> IDuLieuNguon.DocGiaoDan() => GiaoDan;
        IEnumerable<DongThanhVien> IDuLieuNguon.DocThanhVien() => ThanhVien;
    }

    private static NguonGia NguonMau()
    {
        var n = new NguonGia();
        n.GiaoHo.Add(new DongGiaoHo(1, "Giao ho Thanh Tam", null, false));
        n.GiaDinh.Add(new DongGiaDinh(12, 1, "Binh - Lan", "12/4 Nguyen Trai",
            "028 3891 4472", null, null, false, false, "", "", false));
        n.GiaoDan.Add(new DongGiaoDan(4401, "Tran Van Binh", "Giuse", "Nam", 1,
            "03/05/1972", "20/05/1972", "", "", false, "", false));
        n.ThanhVien.Add(new DongThanhVien(12, 4401, 0, true));
        return n;
    }

    [Fact]
    public async Task Chay_thu_khong_ghi_gi_vao_co_so_du_lieu()
    {
        await using var ctx = db.TaoContext();
        var chuyen = new ChuyenDoiDuLieu(ctx, db.GiaoXuId, new BangAnhXaId());

        var kq = await chuyen.Chay(NguonMau(), chayThu: true, CancellationToken.None);

        kq.SoDongNguon["gia_dinh"].Should().Be(1);
        (await ctx.GiaDinh.CountAsync(x => x.MaGiaDinhCu == 12)).Should().Be(0);
    }

    [Fact]
    public async Task Chuyen_doi_that_ghi_du_lieu_va_noi_dung_khoa_ngoai()
    {
        await using var ctx = db.TaoContext();
        var chuyen = new ChuyenDoiDuLieu(ctx, db.GiaoXuId, new BangAnhXaId());

        await chuyen.Chay(NguonMau(), chayThu: false, CancellationToken.None);

        var gd = await ctx.GiaDinh.Include(x => x.GiaoHo).Include(x => x.ThanhVien)
            .SingleAsync(x => x.MaGiaDinhCu == 12);
        gd.GiaoHo!.TenGiaoHo.Should().Be("Giao ho Thanh Tam");
        gd.ThanhVien.Should().ContainSingle().Which.VaiTro.Should().Be(VaiTroGiaDinh.Chong);
    }

    [Fact]
    public async Task Chay_lai_lan_hai_khong_sinh_du_lieu_trung()
    {
        await using var ctx = db.TaoContext();
        var anhXa = new BangAnhXaId();

        await new ChuyenDoiDuLieu(ctx, db.GiaoXuId, anhXa).Chay(NguonMau(), false, CancellationToken.None);
        await new ChuyenDoiDuLieu(ctx, db.GiaoXuId, anhXa).Chay(NguonMau(), false, CancellationToken.None);

        (await ctx.GiaDinh.CountAsync(x => x.MaGiaDinhCu == 12)).Should().Be(1);
        (await ctx.GiaoDan.CountAsync(x => x.MaGiaoDanCu == 4401)).Should().Be(1);
    }

    [Fact]
    public async Task Ngay_hong_duoc_giu_lai_thay_vi_mat_im_lang()
    {
        var nguon = NguonMau();
        nguon.GiaoDan[0] = nguon.GiaoDan[0] with { NgaySinh = "32/13/2005" };
        await using var ctx = db.TaoContext();

        var kq = await new ChuyenDoiDuLieu(ctx, db.GiaoXuId, new BangAnhXaId())
            .Chay(nguon, false, CancellationToken.None);

        var nguoi = await ctx.GiaoDan.SingleAsync(x => x.MaGiaoDanCu == 4401);
        nguoi.NgaySinh.Should().BeNull();
        nguoi.DuLieuLoi.Should().Contain("32/13/2005");
        kq.CanhBao.Should().Contain(c => c.Contains("4401"));
    }

    [Fact]
    public async Task Bao_cao_doi_chieu_so_khop_so_dong_nguon_va_dich()
    {
        await using var ctx = db.TaoContext();

        var kq = await new ChuyenDoiDuLieu(ctx, db.GiaoXuId, new BangAnhXaId())
            .Chay(NguonMau(), false, CancellationToken.None);

        kq.SoDongDich["gia_dinh"].Should().Be(kq.SoDongNguon["gia_dinh"]);
        kq.SoDongDich["giao_dan"].Should().Be(kq.SoDongNguon["giao_dan"]);
    }
}
```

- [ ] **Bước 3: Chạy test để xác nhận thất bại**

Chạy: `dotnet test WebApp/tests/Qlgx.Migration.Tests`
Kỳ vọng: FAIL — biên dịch lỗi, các kiểu chưa tồn tại.

- [ ] **Bước 4: Viết mô hình nguồn và bảng ánh xạ khoá**

Tạo `WebApp/src/Qlgx.Migration/DuLieuNguon.cs`:

```csharp
namespace Qlgx.Migration;

public record DongGiaoHo(int MaGiaoHo, string TenGiaoHo, int? MaGiaoHoCha, bool DaXoa);

public record DongGiaDinh(int MaGiaDinh, int? MaGiaoHo, string? TenGiaDinh, string? DiaChi,
    string? DienThoai, string? SoHoKhau, string? DienGiaDinh, bool DaXoa, bool DaChuyenXu,
    string? NgayChuyen, string? NoiChuyen, bool GiaDinhAo);

public record DongGiaoDan(int MaGiaoDan, string HoTen, string? TenThanh, string? Phai,
    int? MaGiaoHo, string? NgaySinh, string? NgayRuaToi, string? NgayRuocLe, string? NgayThemSuc,
    bool QuaDoi, string? NgayQuaDoi, bool DaXoa);

public record DongThanhVien(int MaGiaDinh, int MaGiaoDan, int VaiTro, bool ChuHo);

/// <summary>
/// Trừu tượng hoá nguồn để bộ chuyển đổi kiểm thử được mà không cần file .mdb và Access
/// Database Engine trên máy chạy test.
/// </summary>
public interface IDuLieuNguon
{
    IEnumerable<DongGiaoHo> DocGiaoHo();
    IEnumerable<DongGiaDinh> DocGiaDinh();
    IEnumerable<DongGiaoDan> DocGiaoDan();
    IEnumerable<DongThanhVien> DocThanhVien();
}
```

Tạo `WebApp/src/Qlgx.Migration/BangAnhXaId.cs`:

```csharp
using System.Security.Cryptography;
using System.Text;

namespace Qlgx.Migration;

/// <summary>
/// Sinh UUID ổn định theo cặp (tên bảng, mã số cũ). Vì cùng đầu vào luôn cho cùng UUID nên
/// chạy công cụ nhiều lần không tạo bản ghi trùng — điều kiện bắt buộc trong giai đoạn thí
/// điểm khi phải chuyển đi chuyển lại nhiều lần.
/// </summary>
public class BangAnhXaId
{
    private readonly Dictionary<string, Guid> _bo = [];

    public Guid Lay(string bang, int maCu)
    {
        var khoa = bang + "#" + maCu;
        if (_bo.TryGetValue(khoa, out var da)) return da;

        var bam = MD5.HashData(Encoding.UTF8.GetBytes("qlgx:" + khoa));
        var id = new Guid(bam);
        _bo[khoa] = id;
        return id;
    }
}
```

- [ ] **Bước 5: Viết bộ chuyển đổi**

Tạo `WebApp/src/Qlgx.Migration/ChuyenDoiDuLieu.cs`:

```csharp
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Qlgx.Data;
using Qlgx.Domain;
using Qlgx.Domain.Entities;

namespace Qlgx.Migration;

public record KetQuaChuyenDoi(
    Dictionary<string, int> SoDongNguon,
    Dictionary<string, int> SoDongDich,
    List<string> CanhBao);

public class ChuyenDoiDuLieu(QlgxDbContext db, Guid giaoXuId, BangAnhXaId anhXa)
{
    private const string Nguon = "access";
    private readonly List<string> _canhBao = [];

    public async Task<KetQuaChuyenDoi> Chay(IDuLieuNguon nguon, bool chayThu, CancellationToken ct)
    {
        var giaoHo = nguon.DocGiaoHo().ToList();
        var giaDinh = nguon.DocGiaDinh().ToList();
        var giaoDan = nguon.DocGiaoDan().ToList();
        var thanhVien = nguon.DocThanhVien().ToList();

        var soNguon = new Dictionary<string, int>
        {
            ["giao_ho"] = giaoHo.Count,
            ["gia_dinh"] = giaDinh.Count,
            ["giao_dan"] = giaoDan.Count,
            ["thanh_vien_gia_dinh"] = thanhVien.Count
        };

        if (chayThu)
            return new KetQuaChuyenDoi(soNguon, new Dictionary<string, int>(), _canhBao);

        // Thứ tự bắt buộc theo chiều phụ thuộc khoá ngoại
        await GhiGiaoHo(giaoHo, ct);
        await GhiGiaDinh(giaDinh, ct);
        await GhiGiaoDan(giaoDan, ct);
        await GhiThanhVien(thanhVien, ct);

        var soDich = new Dictionary<string, int>
        {
            ["giao_ho"] = await db.GiaoHo.CountAsync(x => x.GiaoXuId == giaoXuId, ct),
            ["gia_dinh"] = await db.GiaDinh.CountAsync(x => x.GiaoXuId == giaoXuId, ct),
            ["giao_dan"] = await db.GiaoDan.CountAsync(x => x.GiaoXuId == giaoXuId, ct),
            ["thanh_vien_gia_dinh"] = await db.ThanhVienGiaDinh.CountAsync(x => x.GiaoXuId == giaoXuId, ct)
        };

        return new KetQuaChuyenDoi(soNguon, soDich, _canhBao);
    }

    private async Task GhiGiaoHo(List<DongGiaoHo> dong, CancellationToken ct)
    {
        foreach (var d in dong)
        {
            var id = anhXa.Lay("giao_ho", d.MaGiaoHo);
            var e = await db.GiaoHo.FindAsync([id], ct) ?? Them(new GiaoHo { Id = id });
            e.GiaoXuId = giaoXuId;
            e.MaGiaoHoCu = d.MaGiaoHo;
            e.TenGiaoHo = d.TenGiaoHo;
            e.GiaoHoChaId = d.MaGiaoHoCha is > 0 ? anhXa.Lay("giao_ho", d.MaGiaoHoCha.Value) : null;
            e.DaXoa = d.DaXoa;
            e.SourceSystem = Nguon;
        }
        await db.SaveChangesAsync(ct);
    }

    private async Task GhiGiaDinh(List<DongGiaDinh> dong, CancellationToken ct)
    {
        foreach (var d in dong)
        {
            var id = anhXa.Lay("gia_dinh", d.MaGiaDinh);
            var e = await db.GiaDinh.FindAsync([id], ct) ?? Them(new GiaDinh { Id = id });
            var loi = new Dictionary<string, string>();

            e.GiaoXuId = giaoXuId;
            e.MaGiaDinhCu = d.MaGiaDinh;
            // MaGiaoHo = 0 nghĩa là "Ngoài xứ", không phải khoá ngoại hợp lệ
            e.GiaoHoId = d.MaGiaoHo is > 0 ? anhXa.Lay("giao_ho", d.MaGiaoHo.Value) : null;
            e.TenGiaDinh = d.TenGiaDinh;
            e.DiaChi = d.DiaChi;
            e.DienThoai = d.DienThoai;
            e.SoHoKhau = d.SoHoKhau;
            e.DienGiaDinh = d.DienGiaDinh;
            e.DaXoa = d.DaXoa;
            e.DaChuyenXu = d.DaChuyenXu;
            e.NgayChuyen = DocNgay(d.NgayChuyen, nameof(d.NgayChuyen), loi);
            e.NoiChuyen = d.NoiChuyen;
            e.KhongThongKe = d.GiaDinhAo;
            e.SourceSystem = Nguon;
            GhiLoi(e, loi, "gia_dinh", d.MaGiaDinh);
        }
        await db.SaveChangesAsync(ct);
    }

    private async Task GhiGiaoDan(List<DongGiaoDan> dong, CancellationToken ct)
    {
        foreach (var d in dong)
        {
            var id = anhXa.Lay("giao_dan", d.MaGiaoDan);
            var e = await db.GiaoDan.FindAsync([id], ct) ?? Them(new GiaoDan { Id = id });
            var loi = new Dictionary<string, string>();

            e.GiaoXuId = giaoXuId;
            e.MaGiaoDanCu = d.MaGiaoDan;
            e.HoTen = d.HoTen;
            e.TenThanh = d.TenThanh;
            e.Phai = d.Phai;
            e.GiaoHoId = d.MaGiaoHo is > 0 ? anhXa.Lay("giao_ho", d.MaGiaoHo.Value) : null;
            e.NgaySinh = DocNgay(d.NgaySinh, nameof(d.NgaySinh), loi);
            e.NgayRuaToi = DocNgay(d.NgayRuaToi, nameof(d.NgayRuaToi), loi);
            e.NgayRuocLe = DocNgay(d.NgayRuocLe, nameof(d.NgayRuocLe), loi);
            e.NgayThemSuc = DocNgay(d.NgayThemSuc, nameof(d.NgayThemSuc), loi);
            e.QuaDoi = d.QuaDoi;
            e.NgayQuaDoi = DocNgay(d.NgayQuaDoi, nameof(d.NgayQuaDoi), loi);
            e.DaXoa = d.DaXoa;
            e.SourceSystem = Nguon;
            GhiLoi(e, loi, "giao_dan", d.MaGiaoDan);
        }
        await db.SaveChangesAsync(ct);
    }

    private async Task GhiThanhVien(List<DongThanhVien> dong, CancellationToken ct)
    {
        foreach (var d in dong)
        {
            var maGiaDinh = anhXa.Lay("gia_dinh", d.MaGiaDinh);
            var maGiaoDan = anhXa.Lay("giao_dan", d.MaGiaoDan);
            var vaiTro = (VaiTroGiaDinh)d.VaiTro;

            var da = await db.ThanhVienGiaDinh.FindAsync([maGiaDinh, maGiaoDan, vaiTro], ct);
            if (da is not null) { da.ChuHo = d.ChuHo; continue; }

            db.ThanhVienGiaDinh.Add(new ThanhVienGiaDinh
            {
                GiaoXuId = giaoXuId, GiaDinhId = maGiaDinh, GiaoDanId = maGiaoDan,
                VaiTro = vaiTro, ChuHo = d.ChuHo
            });
        }
        await db.SaveChangesAsync(ct);
    }

    private T Them<T>(T thucThe) where T : class
    {
        db.Add(thucThe);
        return thucThe;
    }

    private DateOnly? DocNgay(string? giaTri, string tenTruong, Dictionary<string, string> loi)
    {
        var (ngay, giuLai) = NgayThangText.Doc(giaTri);
        if (giuLai is not null) loi[tenTruong] = giuLai;
        return ngay;
    }

    private void GhiLoi(ThucTheCoSo e, Dictionary<string, string> loi, string bang, int maCu)
    {
        if (loi.Count == 0) return;
        e.DuLieuLoi = JsonSerializer.Serialize(loi);
        _canhBao.Add($"{bang} mã cũ {maCu}: không đọc được {string.Join(", ", loi.Keys)}");
    }
}
```

- [ ] **Bước 6: Chạy test để xác nhận pass**

Chạy: `dotnet test WebApp/tests/Qlgx.Migration.Tests`
Kỳ vọng: PASS, 5 test.

- [ ] **Bước 7: Viết phần đọc file Access thật và giao diện dòng lệnh**

Tạo `WebApp/src/Qlgx.Migration/DocAccess.cs`:

```csharp
using System.Data.OleDb;
using System.Runtime.Versioning;

namespace Qlgx.Migration;

/// <summary>
/// Đọc file .mdb hoặc .accdb. Chỉ chạy trên Windows và cần Microsoft Access Database Engine.
/// Backend production không tham chiếu lớp này.
/// </summary>
[SupportedOSPlatform("windows")]
public class DocAccess(string duongDanFile) : IDuLieuNguon, IDisposable
{
    private readonly OleDbConnection _ketNoi = new(TaoChuoiKetNoi(duongDanFile));

    private static string TaoChuoiKetNoi(string duongDan) =>
        Path.GetExtension(duongDan).Equals(".mdb", StringComparison.OrdinalIgnoreCase)
            ? $"Provider=Microsoft.Jet.OLEDB.4.0;Data Source={duongDan};"
            : $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={duongDan};";

    public void Mo() => _ketNoi.Open();
    public void Dispose() => _ketNoi.Dispose();

    private IEnumerable<OleDbDataReader> Doc(string sql)
    {
        using var lenh = new OleDbCommand(sql, _ketNoi);
        using var doc = lenh.ExecuteReader();
        while (doc.Read()) yield return doc;
    }

    private static bool Bool(object giaTri) =>
        giaTri is not DBNull && Convert.ToInt32(giaTri) != 0;   // Access dùng -1/0

    private static string? Chuoi(object giaTri) =>
        giaTri is DBNull ? null : Convert.ToString(giaTri);

    private static int? SoNull(object giaTri) =>
        giaTri is DBNull ? null : Convert.ToInt32(giaTri);

    public IEnumerable<DongGiaoHo> DocGiaoHo() =>
        Doc("SELECT MaGiaoHo, TenGiaoHo, MaGiaoHoCha, DaXoa FROM GiaoHo")
            .Select(r => new DongGiaoHo(r.GetInt32(0), Chuoi(r[1]) ?? "", SoNull(r[2]), Bool(r[3])))
            .ToList();

    public IEnumerable<DongGiaDinh> DocGiaDinh() =>
        Doc(@"SELECT MaGiaDinh, MaGiaoHo, TenGiaDinh, DiaChi, DienThoai, SoHoKhau,
                     DienGiaDinh, DaXoa, DaChuyenXu, NgayChuyen, NoiChuyen, GiaDinhAo
              FROM GiaDinh")
            .Select(r => new DongGiaDinh(r.GetInt32(0), SoNull(r[1]), Chuoi(r[2]), Chuoi(r[3]),
                Chuoi(r[4]), Chuoi(r[5]), Chuoi(r[6]), Bool(r[7]), Bool(r[8]),
                Chuoi(r[9]), Chuoi(r[10]), Bool(r[11])))
            .ToList();

    public IEnumerable<DongGiaoDan> DocGiaoDan() =>
        Doc(@"SELECT MaGiaoDan, HoTen, TenThanh, Phai, MaGiaoHo, NgaySinh, NgayRuaToi,
                     NgayRuocLe, NgayThemSuc, QuaDoi, NgayQuaDoi, DaXoa
              FROM GiaoDan")
            .Select(r => new DongGiaoDan(r.GetInt32(0), Chuoi(r[1]) ?? "", Chuoi(r[2]), Chuoi(r[3]),
                SoNull(r[4]), Chuoi(r[5]), Chuoi(r[6]), Chuoi(r[7]), Chuoi(r[8]),
                Bool(r[9]), Chuoi(r[10]), Bool(r[11])))
            .ToList();

    public IEnumerable<DongThanhVien> DocThanhVien() =>
        Doc("SELECT MaGiaDinh, MaGiaoDan, VaiTro, ChuHo FROM ThanhVienGiaDinh")
            .Select(r => new DongThanhVien(r.GetInt32(0), r.GetInt32(1), r.GetInt32(2), Bool(r[3])))
            .ToList();
}
```

Ghi đè `WebApp/src/Qlgx.Migration/Program.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using Qlgx.Data;
using Qlgx.Migration;

if (args.Length < 3)
{
    Console.WriteLine("""
        Cách dùng:
          Qlgx.Migration <duong-dan-giaoxu.mdb> <chuoi-ket-noi-postgres> <ma-giao-xu-guid> [--chay-that]

        Mặc định là CHẠY THỬ: chỉ đọc và in báo cáo, không ghi gì vào PostgreSQL.
        Thêm --chay-that để ghi dữ liệu.
        """);
    return 1;
}

var (duongDan, chuoiKetNoi, maGiaoXu) = (args[0], args[1], Guid.Parse(args[2]));
var chayThat = args.Contains("--chay-that");

using var nguon = new DocAccess(duongDan);
nguon.Mo();

await using var db = new QlgxDbContext(
    new DbContextOptionsBuilder<QlgxDbContext>().UseNpgsql(chuoiKetNoi).Options);
await db.Database.MigrateAsync();

var ketQua = await new ChuyenDoiDuLieu(db, maGiaoXu, new BangAnhXaId())
    .Chay(nguon, chayThu: !chayThat, CancellationToken.None);

Console.WriteLine(chayThat ? "== ĐÃ CHUYỂN DỮ LIỆU ==" : "== CHẠY THỬ, chưa ghi gì ==");
Console.WriteLine($"{"Bảng",-24}{"Nguồn",8}{"Đích",8}");
foreach (var (bang, soNguon) in ketQua.SoDongNguon)
{
    var soDich = ketQua.SoDongDich.TryGetValue(bang, out var v) ? v.ToString() : "-";
    Console.WriteLine($"{bang,-24}{soNguon,8}{soDich,8}");
}

if (ketQua.CanhBao.Count > 0)
{
    Console.WriteLine($"\n{ketQua.CanhBao.Count} cảnh báo dữ liệu (đã giữ nguyên văn trong cột du_lieu_loi):");
    foreach (var c in ketQua.CanhBao.Take(50)) Console.WriteLine("  - " + c);
    if (ketQua.CanhBao.Count > 50) Console.WriteLine($"  … còn {ketQua.CanhBao.Count - 50} cảnh báo nữa");
}

var lech = ketQua.SoDongDich.Any(kv => ketQua.SoDongNguon[kv.Key] != kv.Value);
if (chayThat && lech)
{
    Console.WriteLine("\nCẢNH BÁO: số dòng nguồn và đích không khớp — hãy đối chiếu trước khi dùng thật.");
    return 2;
}
return 0;
```

- [ ] **Bước 8: Kiểm thử tay với dữ liệu thật**

Chạy chế độ chạy thử trên bản sao file `BIN/giaoxu.mdb` của repo:

```bash
dotnet run --project WebApp/src/Qlgx.Migration -- \
  BIN/giaoxu.mdb "Host=localhost;Database=qlgx_thu;Username=postgres;Password=postgres" \
  00000000-0000-0000-0000-000000000001
```

Kỳ vọng: in ra bảng số dòng nguồn, không ghi gì vào PostgreSQL. Ghi lại con số này để đối
chiếu với lần chạy thật.

- [ ] **Bước 9: Commit**

```bash
git add WebApp/src/Qlgx.Migration WebApp/tests/Qlgx.Migration.Tests WebApp/Qlgx.sln
git commit -m "Them cong cu chuyen du lieu tu Access sang PostgreSQL

Sinh UUID on dinh theo cap (bang, ma cu) nen chay lai nhieu lan khong tao ban ghi trung.
Co che do chay thu in bao cao truoc khi ghi that. Chuoi ngay khong phan giai duoc khong bi
vut di ma ghi vao cot du_lieu_loi kem canh bao.

Co-Authored-By: Claude Opus 5 (1M context) <noreply@anthropic.com>"
```

---

## Task 10: Khung ứng dụng phía trình duyệt

Chuyển bản mẫu `WebApp/prototype/qlgx-prototype.html` thành ứng dụng React. Task này chỉ
dựng khung: thanh trên, điều hướng trái, thẻ tài liệu nhiều tab. Toàn bộ CSS đã tinh chỉnh
qua nhiều vòng phản hồi được bê nguyên từ bản mẫu.

**Files:**
- Create: `WebApp/src/web/` (khởi tạo bằng Vite)
- Create: `WebApp/src/web/src/styles/qlgx.css` (chép từ khối `<style>` của bản mẫu)
- Create: `WebApp/src/web/src/components/ThanhPhanKhung/AppShell.tsx`, `SideNav.tsx`, `TabDocs.tsx`
- Create: `WebApp/src/web/src/tabs/useTabDocs.ts`
- Test: `WebApp/src/web/src/tabs/useTabDocs.test.ts`

**Interfaces:**
- Consumes: không có (phía trình duyệt độc lập).
- Produces:
  - `type TheTaiLieu = { id: string; tieuDe: string; noiDung: ReactNode; dongDuoc?: boolean }`
  - `function useTabDocs(): { danhSach: TheTaiLieu[]; dangChon: string; mo(the: TheTaiLieu): void; chon(id: string): void; dong(id: string): void }` — `mo` với id đã tồn tại thì **chuyển tiêu điểm** chứ không thêm thẻ trùng.

- [ ] **Bước 1: Khởi tạo dự án**

```bash
cd WebApp/src
npm create vite@latest web -- --template react-ts
cd web
npm install
npm install ag-grid-community ag-grid-react
npm install -D vitest @testing-library/react @testing-library/user-event jsdom
```

Thêm vào `WebApp/src/web/vite.config.ts` để test và proxy API về backend khi phát triển:

```ts
import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

export default defineConfig({
  plugins: [react()],
  server: {
    proxy: { '/api': 'http://localhost:5000' },
  },
  test: {
    environment: 'jsdom',
    globals: true,
  },
})
```

Thêm `"test": "vitest run"` vào `scripts` của `package.json`.

- [ ] **Bước 2: Viết test thất bại cho thẻ tài liệu**

Tạo `WebApp/src/web/src/tabs/useTabDocs.test.ts`:

```ts
import { act, renderHook } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { useTabDocs } from './useTabDocs'

const the = (id: string, tieuDe: string) => ({ id, tieuDe, noiDung: null })

describe('useTabDocs', () => {
  it('mo hai ban ghi khac nhau thi tao hai the', () => {
    const { result } = renderHook(() => useTabDocs())

    act(() => result.current.mo(the('giaDinh:00012', 'GĐ Bình - Lan')))
    act(() => result.current.mo(the('giaDinh:00027', 'GĐ Chính - Hạnh')))

    expect(result.current.danhSach.map((t) => t.id)).toEqual([
      'giaDinh:00012',
      'giaDinh:00027',
    ])
  })

  it('mo lai dung ban ghi dang mo thi chuyen tieu diem, khong tao the trung', () => {
    const { result } = renderHook(() => useTabDocs())
    act(() => result.current.mo(the('giaDinh:00012', 'GĐ Bình - Lan')))
    act(() => result.current.mo(the('giaDinh:00027', 'GĐ Chính - Hạnh')))

    act(() => result.current.mo(the('giaDinh:00012', 'GĐ Bình - Lan')))

    expect(result.current.danhSach).toHaveLength(2)
    expect(result.current.dangChon).toBe('giaDinh:00012')
  })

  it('dong the dang chon thi chuyen sang the con lai', () => {
    const { result } = renderHook(() => useTabDocs())
    act(() => result.current.mo(the('a', 'A')))
    act(() => result.current.mo(the('b', 'B')))

    act(() => result.current.dong('b'))

    expect(result.current.danhSach.map((t) => t.id)).toEqual(['a'])
    expect(result.current.dangChon).toBe('a')
  })
})
```

- [ ] **Bước 3: Chạy test để xác nhận thất bại**

Chạy: `cd WebApp/src/web && npm test`
Kỳ vọng: FAIL — không tìm thấy module `./useTabDocs`.

- [ ] **Bước 4: Viết hook quản lý thẻ**

Tạo `WebApp/src/web/src/tabs/useTabDocs.ts`:

```ts
import { useCallback, useState, type ReactNode } from 'react'

export type TheTaiLieu = {
  /** Khoá theo bản ghi, ví dụ "giaDinh:<uuid>". Mở lại cùng khoá thì chuyển tiêu điểm. */
  id: string
  tieuDe: string
  noiDung: ReactNode
  dongDuoc?: boolean
}

/**
 * Tương đương FATabStrip cùng dictionary dicShows của frmMain: mỗi bản ghi mở ra một thẻ
 * riêng, mở lại bản ghi đang mở thì chuyển tiêu điểm thay vì tạo thẻ trùng.
 */
export function useTabDocs() {
  const [danhSach, setDanhSach] = useState<TheTaiLieu[]>([])
  const [dangChon, setDangChon] = useState('')

  const mo = useCallback((the: TheTaiLieu) => {
    setDanhSach((truoc) =>
      truoc.some((t) => t.id === the.id) ? truoc : [...truoc, the],
    )
    setDangChon(the.id)
  }, [])

  const chon = useCallback((id: string) => setDangChon(id), [])

  const dong = useCallback((id: string) => {
    setDanhSach((truoc) => {
      const conLai = truoc.filter((t) => t.id !== id)
      setDangChon((hienTai) =>
        hienTai === id ? (conLai.at(-1)?.id ?? '') : hienTai,
      )
      return conLai
    })
  }, [])

  return { danhSach, dangChon, mo, chon, dong }
}
```

- [ ] **Bước 5: Chạy test để xác nhận pass**

Chạy: `cd WebApp/src/web && npm test`
Kỳ vọng: PASS, 3 test.

- [ ] **Bước 6: Chép CSS và dựng khung**

Chép toàn bộ nội dung giữa hai thẻ `<style>` của `WebApp/prototype/qlgx-prototype.html` vào
`WebApp/src/web/src/styles/qlgx.css`, rồi `import './styles/qlgx.css'` trong `src/main.tsx`.
**Không sửa CSS** — đây là kết quả của nhiều vòng phản hồi về màu chữ, chiều cao tấm, độ
đậm nhãn và khoảng cách, không có lý do đổi lại ở bước này.

Tạo `WebApp/src/web/src/components/ThanhPhanKhung/TabDocs.tsx`:

```tsx
import type { TheTaiLieu } from '../../tabs/useTabDocs'

type Props = {
  danhSach: TheTaiLieu[]
  dangChon: string
  onChon: (id: string) => void
  onDong: (id: string) => void
}

export function TabDocs({ danhSach, dangChon, onChon, onDong }: Props) {
  return (
    <>
      <div className="tabstrip" role="tablist">
        {danhSach.map((t) => (
          <div
            key={t.id}
            className="tab"
            role="tab"
            tabIndex={0}
            aria-selected={t.id === dangChon}
            onClick={() => onChon(t.id)}
          >
            <span className="lbl" title={t.tieuDe}>{t.tieuDe}</span>
            {t.dongDuoc !== false && (
              <button
                className="x"
                type="button"
                title="Đóng thẻ"
                onClick={(e) => { e.stopPropagation(); onDong(t.id) }}
              >
                ×
              </button>
            )}
          </div>
        ))}
      </div>
      <div className="tabpages">
        {danhSach.map((t) => (
          <div key={t.id} hidden={t.id !== dangChon} style={{ height: '100%' }}>
            {t.noiDung}
          </div>
        ))}
      </div>
    </>
  )
}
```

`AppShell.tsx` và `SideNav.tsx` chép cấu trúc thẻ HTML tương ứng từ bản mẫu (phần
`<header class="topbar">` và `<nav class="sidenav">`), đổi `class` thành `className` và
đưa danh sách mục điều hướng thành mảng dữ liệu để `SideNav` lặp qua.

- [ ] **Bước 7: Commit**

```bash
git add WebApp/src/web
git commit -m "Khoi tao SPA va khung ung dung

Chep nguyen CSS da tinh chinh tu ban mau. Hook useTabDocs giu dung hanh vi cua frmMain:
moi ban ghi mot the, mo lai thi chuyen tieu diem.

Co-Authored-By: Claude Opus 5 (1M context) <noreply@anthropic.com>"
```

---

## Task 11: Tầng lưới dùng lại

Đây là task quan trọng nhất về mặt kiến trúc. Mục tiêu: khi lưới giáo dân ở màn hình danh
sách đã kiểm thử xong thì lưới thành viên trong form gia đình **chỉ còn phải kiểm lại dữ
liệu**, đúng như bản desktop nhúng cùng một `GxGiaoDanList` ở hai nơi.

**Files:**
- Create: `WebApp/src/web/src/api/client.ts`, `types.ts`
- Create: `WebApp/src/web/src/components/GxGrid.tsx`
- Create: `WebApp/src/web/src/cot/cotGiaoDan.ts`, `cotGiaDinh.ts`
- Create: `WebApp/src/web/src/components/GxGiaoDanList.tsx`, `GxGiaDinhList.tsx`
- Test: `WebApp/src/web/src/components/GxGiaoDanList.test.tsx`

**Interfaces:**
- Consumes: DTO của Task 6 và 8 (`GiaDinhListItemDto`, `GiaoDanListItemDto`).
- Produces:
  - `function GxGrid<T>(props: { columnDefs: ColDef<T>[]; rowData: T[]; onMo?: (r: T) => void; onChon?: (r: T | null) => void; menuChuotPhai?: MucMenu<T>[]; toDo?: (r: T) => boolean; ghiChuChan?: string })`
  - `function GxGiaoDanList(props: { rows: GiaoDanListItem[]; quanHeGiaDinh?: boolean; onMo?: (dong: GiaoDanListItem) => void; onChon?: (dong: GiaoDanListItem | null) => void; menuChuotPhai?: MucMenu<GiaoDanListItem>[]; hangLoc?: boolean })`
  - `function GxGiaDinhList(props: { rows: GiaDinhListItem[]; onMo?: (dong: GiaDinhListItem) => void; onChon?: (dong: GiaDinhListItem | null) => void; menuChuotPhai?: MucMenu<GiaDinhListItem>[]; hangLoc?: boolean })`
  - `type GiaDinhListItem`, `GiaoDanListItem`, `GiaDinhDetail`, `GiaoDanDetail` trong `api/types.ts`; `api` và `LoiXungDot` trong `api/client.ts`
  - `const cotGiaoDan: ColDef<GiaoDanListItem>[]` — 29 cột; `const cotQuanHeGiaDinh` — cột chèn thêm khi nhúng trong form gia đình.

- [ ] **Bước 1: Khai báo kiểu và bộ gọi API**

Tạo `WebApp/src/web/src/api/types.ts`:

```ts
/** Ánh xạ 1-1 với GiaDinhListItemDto phía backend. */
export type GiaDinhListItem = {
  id: string
  maGiaDinhCu: number
  maGiaDinhRieng: string | null
  tenGiaDinh: string | null
  tenChong: string | null
  tenVo: string | null
  soLuong: number
  dienThoai: string | null
  dtChong: string | null
  dtVo: string | null
  diaChi: string | null
  tenGiaoHo: string | null
  dienGiaDinh: string | null
  ghiChu: string | null
  /** 0 = người nam đã qua đời, 1 = người nữ, 2 = cả hai, -1 = không gạch. */
  gach: number
  khongThongKe: boolean
}

/** Ánh xạ 1-1 với GiaoDanListItemDto phía backend. */
export type GiaoDanListItem = {
  id: string
  maGiaoDanCu: number
  tenThanh: string | null
  hoTen: string
  phai: string | null
  ngaySinh: string | null
  namSinh: string
  ngayRuaToi: string | null
  ngayRuocLe: string | null
  ngayThemSuc: string | null
  lapGd: boolean
  hoTenCha: string | null
  hoTenMe: string | null
  tanTong: boolean
  conHoc: boolean
  ngheNghiep: string | null
  ghiChu: string | null
  dienThoai: string | null
  diaChi: string | null
  tenGiaoHo: string | null
  daChuyenDi: boolean
  trinhDoVanHoa: string | null
  trinhDoChuyenMon: string | null
  bietNgoaiNgu: string | null
  quaDoi: boolean
  ngayQuaDoi: string | null
  noiAnTang: string | null
  noiSinh: string | null
  noiRuaToi: string | null
  noiRuocLe: string | null
  noiThemSuc: string | null
  /** Chỉ có giá trị khi lấy qua endpoint thành viên gia đình. */
  quanHe: string | null
}

export type ThanhVien = {
  giaoDanId: string
  vaiTro: number
  chuHo: boolean
  tenThanh: string | null
  hoTen: string
  phai: string | null
  ngaySinh: string | null
  quaDoi: boolean
  daXoa: boolean
}

export type GiaDinhDetail = {
  id: string
  maGiaDinhCu: number
  maGiaDinhRieng: string | null
  tenGiaDinh: string | null
  giaoHoId: string | null
  dienThoai: string | null
  diaChi: string | null
  soHoKhau: string | null
  dienGiaDinh: string | null
  ghiChu: string | null
  daChuyenXu: boolean
  ngayChuyen: string | null
  noiChuyen: string | null
  khongThongKe: boolean
  rowVersion: number
  thanhVien: ThanhVien[]
}

/** Các trường màn hình chi tiết giáo dân dùng tới; xem GiaoDanDetailDto phía backend. */
export type GiaoDanDetail = {
  id: string
  maGiaoDanCu: number
  hoTen: string
  tenThanh: string | null
  phai: string | null
  ngaySinh: string | null
  noiSinh: string | null
  cmnd: string | null
  danToc: string | null
  giaoHoId: string | null
  diaChi: string | null
  dienThoai: string | null
  email: string | null
  hoTenCha: string | null
  hoTenMe: string | null
  soRuaToi: string | null
  ngayRuaToi: string | null
  noiRuaToi: string | null
  chaRuaToi: string | null
  nguoiDoDauRuaToi: string | null
  soRuocLe: string | null
  ngayRuocLe: string | null
  noiRuocLe: string | null
  chaRuocLe: string | null
  soThemSuc: string | null
  ngayThemSuc: string | null
  noiThemSuc: string | null
  chaThemSuc: string | null
  nguoiDoDauThemSuc: string | null
  ngayXucDau: string | null
  nguoiXucDau: string | null
  tinhTrangXucDau: string | null
  ghiChuXucDau: string | null
  trinhDoVanHoa: string | null
  trinhDoChuyenMon: string | null
  bietNgoaiNgu: string | null
  ngheNghiep: string | null
  conHoc: boolean
  daCoGiaDinh: boolean
  tanTong: boolean
  khongThongKe: boolean
  quaDoi: boolean
  ngayQuaDoi: string | null
  noiQuaDoi: string | null
  soAnTang: string | null
  noiAnTang: string | null
  ghiChu: string | null
  giaDinhId: string | null
  tenGiaDinh: string | null
  vaiTro: number | null
  rowVersion: number
}
```

Tạo `WebApp/src/web/src/api/client.ts`:

```ts
import type {
  GiaDinhDetail, GiaDinhListItem, GiaoDanDetail, GiaoDanListItem,
} from './types'

class LoiXungDot extends Error {}
export { LoiXungDot }

async function goi<T>(duong: string, tuyChon?: RequestInit): Promise<T> {
  const res = await fetch(duong, {
    ...tuyChon,
    headers: { 'Content-Type': 'application/json', ...tuyChon?.headers },
  })
  if (res.status === 409) {
    const { thongBao } = await res.json()
    throw new LoiXungDot(thongBao)
  }
  if (!res.ok) throw new Error(`${res.status} khi gọi ${duong}`)
  return res.status === 204 ? (undefined as T) : ((await res.json()) as T)
}

const thamSo = (giaoHoId?: string, chiKhongThongKe?: boolean) => {
  const p = new URLSearchParams()
  if (giaoHoId) p.set('giaoHoId', giaoHoId)
  if (chiKhongThongKe) p.set('chiKhongThongKe', 'true')
  const s = p.toString()
  return s ? `?${s}` : ''
}

export const api = {
  giaDinh: {
    danhSach: (giaoHoId?: string, chiKhongThongKe?: boolean) =>
      goi<GiaDinhListItem[]>(`/api/gia-dinh${thamSo(giaoHoId, chiKhongThongKe)}`),
    chiTiet: (id: string) => goi<GiaDinhDetail>(`/api/gia-dinh/${id}`),
    capNhat: (id: string, than: unknown) =>
      goi<void>(`/api/gia-dinh/${id}`, { method: 'PUT', body: JSON.stringify(than) }),
    thanhVien: (id: string) => goi<GiaoDanListItem[]>(`/api/gia-dinh/${id}/thanh-vien`),
  },
  giaoDan: {
    danhSach: (giaoHoId?: string, chiKhongThongKe?: boolean) =>
      goi<GiaoDanListItem[]>(`/api/giao-dan${thamSo(giaoHoId, chiKhongThongKe)}`),
    chiTiet: (id: string) => goi<GiaoDanDetail>(`/api/giao-dan/${id}`),
    capNhat: (id: string, than: unknown) =>
      goi<void>(`/api/giao-dan/${id}`, { method: 'PUT', body: JSON.stringify(than) }),
  },
}
```

- [ ] **Bước 2: Viết test thất bại**

Tạo `WebApp/src/web/src/components/GxGiaoDanList.test.tsx`:

```tsx
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'
import { GxGiaoDanList } from './GxGiaoDanList'
import type { GiaoDanListItem } from '../api/types'

const nguoi = (p: Partial<GiaoDanListItem> = {}): GiaoDanListItem => ({
  id: 'a1', maGiaoDanCu: 4412, tenThanh: 'Maria', hoTen: 'Trần Thị Khánh Ngọc',
  phai: 'Nữ', ngaySinh: '2005-03-14', namSinh: '2005', ngayRuaToi: null,
  ngayRuocLe: null, ngayThemSuc: null, lapGd: false, hoTenCha: null, hoTenMe: null,
  tanTong: false, conHoc: true, ngheNghiep: null, ghiChu: null, dienThoai: null,
  diaChi: null, tenGiaoHo: 'Giáo họ Thánh Tâm', daChuyenDi: false,
  trinhDoVanHoa: null, trinhDoChuyenMon: null, bietNgoaiNgu: null,
  quaDoi: false, ngayQuaDoi: null, noiAnTang: null, noiSinh: null,
  noiRuaToi: null, noiRuocLe: null, noiThemSuc: null, quanHe: null, ...p,
})

describe('GxGiaoDanList', () => {
  it('hien du 29 cot cua ban desktop khi dung o man hinh danh sach', () => {
    render(<GxGiaoDanList rows={[nguoi()]} />)

    expect(screen.getByText('Mã GD')).toBeDefined()
    expect(screen.getByText('Tên thánh')).toBeDefined()
    expect(screen.getByText('Nơi thêm sức')).toBeDefined()
    expect(screen.queryByText('Quan hệ GĐ')).toBeNull()
  })

  it('them cot Quan he GD va bo cot Dien thoai khi nhung trong form gia dinh', () => {
    render(<GxGiaoDanList rows={[nguoi()]} quanHeGiaDinh />)

    expect(screen.getByText('Quan hệ GĐ')).toBeDefined()
    expect(screen.queryByText('Điện thoại')).toBeNull()
  })

  it('nhap dup mot dong thi goi onMo voi dung ban ghi', async () => {
    const onMo = vi.fn()
    render(<GxGiaoDanList rows={[nguoi()]} onMo={onMo} />)

    await userEvent.dblClick(screen.getByText('Trần Thị Khánh Ngọc'))

    expect(onMo).toHaveBeenCalledWith(expect.objectContaining({ maGiaoDanCu: 4412 }))
  })

  it('to do dong cua nguoi da qua doi, chuyen xu hoac lap gia dinh rieng', () => {
    const { container } = render(
      <GxGiaoDanList rows={[nguoi({ quaDoi: true }), nguoi({ id: 'a2', maGiaoDanCu: 1, quaDoi: false })]} />,
    )

    expect(container.querySelectorAll('.dong-gach-do')).toHaveLength(1)
  })
})
```

- [ ] **Bước 3: Chạy test để xác nhận thất bại**

Chạy: `cd WebApp/src/web && npm test`
Kỳ vọng: FAIL — không tìm thấy module `./GxGiaoDanList`.

- [ ] **Bước 4: Viết GxGrid**

Tạo `WebApp/src/web/src/components/GxGrid.tsx`:

```tsx
import { AgGridReact } from 'ag-grid-react'
import type { ColDef, GetRowIdParams, RowClassParams } from 'ag-grid-community'
import 'ag-grid-community/styles/ag-grid.css'
import 'ag-grid-community/styles/ag-theme-quartz.css'
import { useCallback, useMemo, useRef, useState } from 'react'

export type MucMenu<T> = { nhan: string; chay?: (dong: T) => void }

type Props<T> = {
  columnDefs: ColDef<T>[]
  rowData: T[]
  layId: (dong: T) => string
  onMo?: (dong: T) => void
  onChon?: (dong: T | null) => void
  menuChuotPhai?: MucMenu<T>[]
  /** Trả true để tô đỏ và gạch ngang cả dòng, giống quy tắc IsRedGiaoDan của bản desktop. */
  toDo?: (dong: T) => boolean
  ghiChuChan?: string
  hangLoc?: boolean
}

/**
 * Lớp lưới cơ sở, tương đương GxGrid : GridEX của bản desktop. Gói sẵn hàng lọc từng cột,
 * sắp xếp theo header, chọn dòng, mở bằng nhấp đúp và menu chuột phải để các lưới nghiệp vụ
 * bên trên không phải khai báo lại.
 */
export function GxGrid<T>({
  columnDefs, rowData, layId, onMo, onChon, menuChuotPhai, toDo, ghiChuChan, hangLoc = true,
}: Props<T>) {
  const [menu, setMenu] = useState<{ x: number; y: number; dong: T } | null>(null)
  const boc = useRef<HTMLDivElement>(null)

  const defaultColDef = useMemo<ColDef<T>>(
    () => ({ sortable: true, resizable: true, filter: hangLoc ? 'agTextColumnFilter' : false }),
    [hangLoc],
  )

  const getRowClass = useCallback(
    (p: RowClassParams<T>) => (p.data && toDo?.(p.data) ? 'dong-gach-do' : ''),
    [toDo],
  )

  return (
    <div className="table-card glass" ref={boc}>
      <div
        className="grid-wrap ag-theme-quartz"
        onContextMenu={(e) => {
          if (!menuChuotPhai?.length) return
          const dong = (e.target as HTMLElement).closest('.ag-row')
          if (!dong) return
          e.preventDefault()
          const id = dong.getAttribute('row-id')
          const banGhi = rowData.find((r) => layId(r) === id)
          if (banGhi) setMenu({ x: e.clientX, y: e.clientY, dong: banGhi })
        }}
      >
        <AgGridReact<T>
          columnDefs={columnDefs}
          rowData={rowData}
          defaultColDef={defaultColDef}
          floatingFiltersHeight={hangLoc ? 36 : 0}
          getRowId={(p: GetRowIdParams<T>) => layId(p.data)}
          getRowClass={getRowClass}
          rowSelection="single"
          onRowDoubleClicked={(e) => e.data && onMo?.(e.data)}
          onSelectionChanged={(e) => onChon?.(e.api.getSelectedRows()[0] ?? null)}
          localeText={{ noRowsToShow: 'Không có dòng nào khớp điều kiện lọc.' }}
        />
      </div>

      {ghiChuChan && (
        <div className="table-foot">
          <span className="legend"><i />{ghiChuChan}</span>
        </div>
      )}

      {menu && (
        <div
          id="ctxmenu"
          style={{ left: menu.x, top: menu.y }}
          onMouseLeave={() => setMenu(null)}
        >
          {menuChuotPhai!.map((m) => (
            <button key={m.nhan} type="button"
              onClick={() => { m.chay?.(menu.dong); setMenu(null) }}>
              {m.nhan}
            </button>
          ))}
        </div>
      )}
    </div>
  )
}
```

Thêm vào cuối `WebApp/src/web/src/styles/qlgx.css`:

```css
/* Dòng của người đã qua đời, chuyển xứ hoặc lập gia đình riêng — quy tắc IsRedGiaoDan */
.ag-theme-quartz .dong-gach-do,
.ag-theme-quartz .dong-gach-do .ag-cell {
  color: var(--rose-ink);
  text-decoration: line-through;
  text-decoration-thickness: 1px;
}
```

- [ ] **Bước 5: Viết định nghĩa cột**

Tạo `WebApp/src/web/src/cot/cotGiaoDan.ts`:

```ts
import type { ColDef } from 'ag-grid-community'
import type { GiaoDanListItem } from '../api/types'

const co = (field: keyof GiaoDanListItem, headerName: string, width = 110): ColDef<GiaoDanListItem> => ({
  field: field as string,
  headerName,
  width,
  valueFormatter: (p) => (p.value === true ? '✓' : p.value === false ? '—' : (p.value ?? '—')),
})

/**
 * 29 cột đúng thứ tự của GxGiaoDanList.FormatGrid(). Tách khỏi màn hình vì cùng bộ cột này
 * được dùng ở hai nơi: danh sách giáo dân và lưới thành viên trong form gia đình.
 */
export const cotGiaoDan: ColDef<GiaoDanListItem>[] = [
  { field: 'maGiaoDanCu', headerName: 'Mã GD', width: 100, cellClass: 'cell-code' },
  { field: 'tenThanh', headerName: 'Tên thánh', width: 110 },
  { field: 'hoTen', headerName: 'Họ tên', width: 190, cellClass: 'cell-strong' },
  { field: 'phai', headerName: 'Phái', width: 80 },
  { field: 'ngaySinh', headerName: 'Ngày sinh', width: 115 },
  { field: 'ngayRuaToi', headerName: 'Ngày rửa tội', width: 125 },
  { field: 'ngayRuocLe', headerName: 'Ngày XTRL', width: 120 },
  { field: 'ngayThemSuc', headerName: 'Ngày Th.Sức', width: 125 },
  co('lapGd', 'Lập GĐ', 95),
  { field: 'hoTenCha', headerName: 'Cha', width: 160 },
  { field: 'hoTenMe', headerName: 'Mẹ', width: 160 },
  co('tanTong', 'Tân tòng', 100),
  co('conHoc', 'Còn học', 95),
  { field: 'ngheNghiep', headerName: 'Nghề nghiệp', width: 140 },
  { field: 'ghiChu', headerName: 'Ghi chú', width: 200 },
  { field: 'dienThoai', headerName: 'Điện thoại', width: 130 },
  { field: 'diaChi', headerName: 'Địa chỉ', width: 200 },
  { field: 'tenGiaoHo', headerName: 'Giáo họ', width: 150 },
  co('daChuyenDi', 'Đã chuyển đi', 130),
  { field: 'trinhDoVanHoa', headerName: 'Văn hóa', width: 110 },
  { field: 'trinhDoChuyenMon', headerName: 'Chuyên môn', width: 130 },
  { field: 'bietNgoaiNgu', headerName: 'Ngoại ngữ', width: 120 },
  co('quaDoi', 'Qua đời', 95),
  { field: 'ngayQuaDoi', headerName: 'Ngày qua đời', width: 130 },
  { field: 'noiAnTang', headerName: 'Nơi an táng', width: 150 },
  { field: 'noiSinh', headerName: 'Nơi sinh', width: 130 },
  { field: 'noiRuaToi', headerName: 'Nơi rửa tội', width: 140 },
  { field: 'noiRuocLe', headerName: 'Nơi XTRL', width: 130 },
  { field: 'noiThemSuc', headerName: 'Nơi thêm sức', width: 140 },
]

/** Cột chỉ có khi lưới nhúng trong form gia đình — frmGiaDinh chèn ở vị trí 0. */
export const cotQuanHeGiaDinh: ColDef<GiaoDanListItem> = {
  field: 'quanHe',
  headerName: 'Quan hệ GĐ',
  width: 130,
  editable: true,
  cellEditor: 'agSelectCellEditor',
  cellEditorParams: { values: ['Chồng', 'Vợ', 'Con'] },
}
```

- [ ] **Bước 6: Viết lưới nghiệp vụ**

Tạo `WebApp/src/web/src/components/GxGiaoDanList.tsx`:

```tsx
import { useMemo } from 'react'
import type { GiaoDanListItem } from '../api/types'
import { cotGiaoDan, cotQuanHeGiaDinh } from '../cot/cotGiaoDan'
import { GxGrid, type MucMenu } from './GxGrid'

type Props = {
  rows: GiaoDanListItem[]
  /** Nhúng trong form gia đình: thêm cột Quan hệ GĐ, bỏ cột Điện thoại. */
  quanHeGiaDinh?: boolean
  onMo?: (dong: GiaoDanListItem) => void
  onChon?: (dong: GiaoDanListItem | null) => void
  menuChuotPhai?: MucMenu<GiaoDanListItem>[]
  hangLoc?: boolean
}

/** Đúng 12 mục và đúng thứ tự trong constructor của GxGiaoDanList bản desktop. */
export const menuGiaoDanMacDinh = (
  moChiTiet: (d: GiaoDanListItem) => void,
  xemGiaDinh: (d: GiaoDanListItem) => void,
): MucMenu<GiaoDanListItem>[] => [
  { nhan: 'Xem chi tiết', chay: moChiTiet },
  { nhan: 'In lý lịch cá nhân' },
  { nhan: 'In chứng nhận bí tích' },
  { nhan: 'In giới thiệu hôn phối' },
  { nhan: 'In chứng nhận rửa tội' },
  { nhan: 'In chứng nhận xưng tội - rước lễ' },
  { nhan: 'In chứng nhận thêm sức' },
  { nhan: 'Xem gia đình', chay: xemGiaDinh },
  { nhan: 'In giấy giới thiệu chứng nhận rửa tội' },
  { nhan: 'In giấy giới thiệu giáo lý hôn phối' },
  { nhan: 'In giấy giới thiệu chứng nhận thêm sức' },
  { nhan: 'Xem vị trí' },
]

/**
 * Tương đương UserControl GxGiaoDanList. Tự sở hữu bộ cột, quy tắc tô đỏ và ghi chú chân
 * lưới, nên nơi nhúng chỉ truyền dữ liệu. Nhờ vậy lưới ở màn hình danh sách và lưới thành
 * viên trong form gia đình là cùng một component — kiểm thử một lần dùng được cả hai.
 */
export function GxGiaoDanList({ rows, quanHeGiaDinh, ...phanConLai }: Props) {
  const columnDefs = useMemo(
    () =>
      quanHeGiaDinh
        ? [cotQuanHeGiaDinh, ...cotGiaoDan.filter((c) => c.field !== 'dienThoai')]
        : cotGiaoDan,
    [quanHeGiaDinh],
  )

  return (
    <GxGrid<GiaoDanListItem>
      columnDefs={columnDefs}
      rowData={rows}
      layId={(d) => d.id}
      toDo={(d) => d.quaDoi || d.daChuyenDi || d.lapGd}
      ghiChuChan="Gạch ngang đỏ: đã qua đời, chuyển xứ hoặc lập gia đình riêng"
      {...phanConLai}
    />
  )
}
```

`GxGiaDinhList.tsx` viết theo cùng khuôn: dùng `cotGiaDinh`, `toDo` trả về `false` (gia đình
không tô cả dòng — chỉ ô Người nam hoặc Người nữ bị gạch theo cột `gach`, xử lý bằng
`cellClass` trong `cotGiaDinh.ts`), ghi chú chân lưới là
`"Gạch ngang đỏ: người nam / người nữ đã qua đời hoặc chuyển xứ"`, và menu chuột phải 5 mục:
`In chứng nhận hôn phối`, `In phiếu gia đình`, `In lý lịch cá nhân`, `In giới thiệu chuyển xứ`,
`Xem vị trí`.

- [ ] **Bước 7: Chạy test để xác nhận pass**

Chạy: `cd WebApp/src/web && npm test`
Kỳ vọng: PASS, 7 test.

- [ ] **Bước 8: Commit**

```bash
git add WebApp/src/web
git commit -m "Them tang luoi dung lai GxGrid, GxGiaoDanList, GxGiaDinhList

Bo cot tach khoi man hinh nen cung mot luoi giao dan duoc dung o danh sach giao dan va
o luoi thanh vien trong form gia dinh, dung nguyen tac cua project GXControl.

Co-Authored-By: Claude Opus 5 (1M context) <noreply@anthropic.com>"
```

---

## Task 12: Bốn màn hình nghiệp vụ

**Files:**
- Create: `WebApp/src/web/src/screens/GiaDinhList.tsx`, `GiaDinhDetail.tsx`, `GiaoDanList.tsx`, `GiaoDanDetail.tsx`
- Create: `WebApp/src/web/src/components/GxFormTabs.tsx`, `GxField.tsx`, `GxPicker.tsx`, `GxToolbar.tsx`
- Modify: `WebApp/src/web/src/App.tsx`
- Test: `WebApp/src/web/src/screens/GiaDinhDetail.test.tsx`
- Test: `WebApp/src/web/src/screens/GiaoDanDetail.test.tsx`

**Interfaces:**
- Consumes: `GxGiaoDanList`, `GxGiaDinhList`, `useTabDocs`, các endpoint của Task 6–8.
- Produces: bốn component màn hình, mỗi cái nhận `{ id?: string; moGiaDinh(id): void; moGiaoDan(id): void }` để mở thẻ mới từ trong màn hình.

- [ ] **Bước 1: Viết test thất bại cho các liên động của form**

Tạo `WebApp/src/web/src/screens/GiaoDanDetail.test.tsx`:

```tsx
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it } from 'vitest'
import { GiaoDanDetail } from './GiaoDanDetail'
import type { GiaoDanDetail as ChiTiet } from '../api/types'

const chiTiet = (p: Partial<ChiTiet> = {}): ChiTiet => ({
  id: 'p1', maGiaoDanCu: 4511, hoTen: 'Vũ Minh Trí', tenThanh: 'Giuse', phai: 'Nam',
  ngaySinh: '1996-04-02', giaoHoId: null, quaDoi: false, conHoc: false,
  ngayQuaDoi: null, noiQuaDoi: null, soAnTang: null, noiAnTang: null,
  rowVersion: 1, giaDinhId: null, tenGiaDinh: null, vaiTro: null, ...p,
} as ChiTiet)

describe('GiaoDanDetail', () => {
  it('co du nam tab dung ten cua frmGiaoDan', () => {
    render(<GiaoDanDetail duLieu={chiTiet()} />)

    for (const ten of ['Cá nhân', 'Giáo lý', 'Hôn phối', 'Ơn gọi tận hiến', 'Hội đoàn'])
      expect(screen.getByRole('tab', { name: ten })).toBeDefined()
  })

  it('tick Qua doi thi hien them ngay qua doi va noi an tang', async () => {
    render(<GiaoDanDetail duLieu={chiTiet()} />)
    expect(screen.queryByLabelText('Ngày qua đời')).toBeNull()

    await userEvent.click(screen.getByLabelText('Qua đời'))

    expect(screen.getByLabelText('Ngày qua đời')).toBeDefined()
    expect(screen.getByLabelText('Nơi an táng')).toBeDefined()
  })

  it('tick Qua doi thi tu bo tick Con hoc', async () => {
    render(<GiaoDanDetail duLieu={chiTiet({ conHoc: true })} />)
    const conHoc = screen.getByLabelText('Còn học') as HTMLInputElement
    expect(conHoc.checked).toBe(true)

    await userEvent.click(screen.getByLabelText('Qua đời'))

    expect(conHoc.checked).toBe(false)
  })

  it('chon giao ho Ngoai xu thi hien Giao xu va Giao phan, an khoi chuyen xu', async () => {
    render(<GiaoDanDetail duLieu={chiTiet()} />)

    await userEvent.selectOptions(screen.getByLabelText('Giáo họ'), 'Ngoài xứ')

    expect(screen.getByLabelText('Giáo xứ')).toBeDefined()
    expect(screen.queryByText('Thông tin chuyển xứ')).toBeNull()
  })
})
```

- [ ] **Bước 2: Chạy test để xác nhận thất bại**

Chạy: `cd WebApp/src/web && npm test -- GiaoDanDetail`
Kỳ vọng: FAIL — không tìm thấy module `./GiaoDanDetail`.

- [ ] **Bước 3: Chuyển các component form từ bản mẫu**

Dịch sang React các hàm đã có trong `WebApp/prototype/qlgx-prototype.html`, giữ nguyên cấu
trúc thẻ và tên lớp CSS:

- `GxFormTabs` ← hàm `GxFormTabs` của bản mẫu, thêm `role="tab"` và `aria-selected` để test truy cập được.
- `GxField` ← hàm `GxField`; nhãn nối với ô nhập bằng `htmlFor`/`id` để `getByLabelText` tìm thấy.
- `GxPicker` ← hàm `GxPicker` (ô chọn giáo dân, ba nút tròn).
- `GxToolbar` ← hàm `GxToolbar`, giữ quy tắc nút `needSel` tự tắt khi lưới chưa chọn dòng.

- [ ] **Bước 4: Viết bốn màn hình**

Dựng theo đúng bố cục đã chốt trong bản mẫu:

- `GiaDinhList` — `page-head` với tiêu đề và nút "Thêm gia đình"; `filters-bar` có combobox
  Giáo họ (kèm hai mục "Tất cả" và "Ngoài xứ") và ô tick
  "Chỉ xem gia đình không được thống kê"; `GxGiaDinhList` bên dưới.
- `GiaDinhDetail` — bố cục neo: đầu trang một dòng, khối thông tin dùng `cols`, lưới thành
  viên `GxGiaoDanList` với `quanHeGiaDinh` chiếm phần dưới và tự cuộn, thanh nút dưới cùng
  gồm `In lý lịch cá nhân`, `In phiếu gia đình`, `Quay về`, `Cập nhật`.
- `GiaoDanList` — như `GiaDinhList` nhưng ô tick là
  "Chỉ xem giáo dân không được thống kê" và lưới là `GxGiaoDanList`.
- `GiaoDanDetail` — `GxFormTabs` năm tab, tab "Cá nhân" gồm các khối
  "Thông tin cá nhân", "Ảnh đại diện (ảnh 3x4)", "Thông tin chuyển xứ", "Rửa tội",
  "Rước lễ lần đầu", "Thêm sức", "Xức dầu", "Thông tin khác"; thanh nút dưới cùng gồm
  `Xem gia đình`, `In lý lịch cá nhân`, `Quay về`, `Cập nhật`.

Ba liên động bắt buộc trong `GiaoDanDetail`, lấy đúng từ `frmGiaoDan.cs`:

```tsx
// chkQuaDoi_CheckedChanged: hiện 4 trường và tự bỏ tick Còn học
const [quaDoi, setQuaDoi] = useState(duLieu.quaDoi)
const [conHoc, setConHoc] = useState(duLieu.conHoc)
const doiQuaDoi = (v: boolean) => { setQuaDoi(v); if (v) setConHoc(false) }
// chkConHoc_CheckedChanged: quan hệ loại trừ hai chiều
const doiConHoc = (v: boolean) => { setConHoc(v); if (v) setQuaDoi(false) }
// cbGiaoHo_SelectedIndexChanged: Ngoài xứ thì hiện Giáo xứ/Giáo phận, ẩn khối chuyển xứ
const ngoaiXu = giaoHo === 'Ngoài xứ'
```

Nút "Quay về" và "← Danh sách" gọi hàm mở danh sách tương ứng — **có thẻ thì chuyển tiêu
điểm, chưa có thì mở mới** — chứ không quay về thẻ Tổng quan.

- [ ] **Bước 5: Chạy test để xác nhận pass**

Chạy: `cd WebApp/src/web && npm test`
Kỳ vọng: PASS, 11 test trở lên.

- [ ] **Bước 6: Commit**

```bash
git add WebApp/src/web
git commit -m "Them bon man hinh nghiep vu

Form giao dan dung nam tab theo frmGiaoDan, kem ba lien dong: tick Qua doi hien them bon
truong va tu bo tick Con hoc; chon giao ho Ngoai xu thi hien Giao xu va Giao phan roi an
khoi chuyen xu.

Co-Authored-By: Claude Opus 5 (1M context) <noreply@anthropic.com>"
```

---

## Task 13: Kiểm thử đầu-cuối và đóng gói cài đặt

**Files:**
- Create: `WebApp/tests/e2e/package.json`, `playwright.config.ts`
- Create: `WebApp/tests/e2e/bonManHinh.spec.ts`
- Create: `WebApp/deploy/cai-dat.ps1`
- Modify: `WebApp/src/Qlgx.Api/Program.cs` (phục vụ static file của SPA)

**Interfaces:**
- Consumes: toàn bộ các task trước.
- Produces: lệnh `npm run e2e` chạy được; script `cai-dat.ps1` cài PostgreSQL, tạo database, chạy migration và đăng ký Windows Service.

- [ ] **Bước 1: Cho backend phục vụ luôn SPA**

Thêm vào `WebApp/src/Qlgx.Api/Program.cs`, sau các `app.Map…`:

```csharp
// Một tiến trình phục vụ cả API lẫn giao diện — máy giáo xứ không phải cài thêm web server
app.UseDefaultFiles();
app.UseStaticFiles();
app.MapFallbackToFile("index.html");
```

Thêm vào `Qlgx.Api.csproj` để bản phát hành gói sẵn giao diện đã build:

```xml
  <Target Name="BuildSpa" BeforeTargets="Publish">
    <Exec Command="npm ci &amp;&amp; npm run build" WorkingDirectory="../web" />
    <ItemGroup>
      <SpaFiles Include="../web/dist/**/*" />
    </ItemGroup>
    <Copy SourceFiles="@(SpaFiles)" DestinationFolder="$(PublishDir)wwwroot/%(RecursiveDir)" />
  </Target>
```

- [ ] **Bước 2: Viết kịch bản đầu-cuối**

Tạo `WebApp/tests/e2e/bonManHinh.spec.ts`:

```ts
import { expect, test } from '@playwright/test'

test('mo hai gia dinh thi duoc hai the, mo lai thi chuyen tieu diem', async ({ page }) => {
  await page.goto('/')
  await page.getByRole('button', { name: 'Danh sách gia đình' }).click()

  await page.getByRole('row').nth(1).dblclick()
  await page.getByRole('tab', { name: 'Danh sách gia đình' }).click()
  await page.getByRole('row').nth(2).dblclick()
  await expect(page.locator('.tabstrip .tab')).toHaveCount(4)  // Tổng quan + danh sách + 2 gia đình

  await page.getByRole('tab', { name: 'Danh sách gia đình' }).click()
  await page.getByRole('row').nth(1).dblclick()
  await expect(page.locator('.tabstrip .tab')).toHaveCount(4)
})

test('nut Danh sach mo lai the danh sach da bi dong', async ({ page }) => {
  await page.goto('/')
  await page.getByRole('button', { name: 'Danh sách gia đình' }).click()
  await page.getByRole('row').nth(1).dblclick()

  await page.getByRole('tab', { name: 'Danh sách gia đình' })
    .getByTitle('Đóng thẻ').click()
  await page.getByRole('button', { name: '← Danh sách' }).click()

  await expect(page.getByRole('tab', { name: 'Danh sách gia đình' })).toHaveAttribute(
    'aria-selected', 'true')
})

test('loc theo giao ho thu hep danh sach', async ({ page }) => {
  await page.goto('/')
  await page.getByRole('button', { name: 'Danh sách gia đình' }).click()
  const truoc = await page.getByRole('row').count()

  await page.getByLabel('Giáo họ').selectOption({ index: 2 })

  await expect(page.getByRole('row')).not.toHaveCount(truoc)
})

test('tick Qua doi trong form giao dan hien them truong ngay qua doi', async ({ page }) => {
  await page.goto('/')
  await page.getByRole('button', { name: 'Danh sách giáo dân' }).click()
  await page.getByRole('row').nth(1).dblclick()

  await page.getByLabel('Qua đời').check()

  await expect(page.getByLabel('Ngày qua đời')).toBeVisible()
})
```

- [ ] **Bước 3: Chạy kiểm thử đầu-cuối**

```bash
cd WebApp/tests/e2e
npm init -y && npm i -D @playwright/test && npx playwright install chromium
npx playwright test
```

Kỳ vọng: 4 kịch bản PASS. Cần backend đang chạy với dữ liệu đã chuyển đổi.

- [ ] **Bước 4: Viết script cài đặt**

Tạo `WebApp/deploy/cai-dat.ps1`:

```powershell
# Cài đặt QLGX bản web cho một giáo xứ. Chạy với quyền quản trị viên.
param(
    [Parameter(Mandatory)][string]$MaGiaoXu,       # GUID định danh giáo xứ
    [string]$ThuMuc = "C:\QuanLyGiaoXuWeb",
    [string]$MatKhauPg = "qlgx",
    [int]$Cong = 5000
)

$ErrorActionPreference = "Stop"

Write-Host "1/5 Cài PostgreSQL nếu chưa có…"
if (-not (Get-Service postgresql* -ErrorAction SilentlyContinue)) {
    winget install --id PostgreSQL.PostgreSQL.16 --silent --accept-package-agreements
}

Write-Host "2/5 Tạo database…"
$env:PGPASSWORD = $MatKhauPg
& psql -U postgres -c "CREATE DATABASE qlgx" 2>$null

Write-Host "3/5 Chép ứng dụng…"
New-Item -ItemType Directory -Force -Path $ThuMuc | Out-Null
Copy-Item -Recurse -Force "$PSScriptRoot\app\*" $ThuMuc

$cauHinh = @{
    ConnectionStrings = @{ Qlgx = "Host=localhost;Database=qlgx;Username=postgres;Password=$MatKhauPg" }
    Qlgx              = @{ GiaoXuId = $MaGiaoXu }
    Urls              = "http://0.0.0.0:$Cong"
} | ConvertTo-Json -Depth 5
Set-Content -Path "$ThuMuc\appsettings.Production.json" -Value $cauHinh -Encoding UTF8

Write-Host "4/5 Mở cổng trên tường lửa để các máy trong mạng nội bộ truy cập được…"
New-NetFirewallRule -DisplayName "QLGX Web" -Direction Inbound -LocalPort $Cong `
    -Protocol TCP -Action Allow -Profile Private -ErrorAction SilentlyContinue | Out-Null

Write-Host "5/5 Đăng ký dịch vụ chạy nền…"
sc.exe create QlgxWeb binPath= "$ThuMuc\Qlgx.Api.exe" start= auto | Out-Null
sc.exe start QlgxWeb | Out-Null

$ip = (Get-NetIPAddress -AddressFamily IPv4 |
       Where-Object { $_.IPAddress -notlike "127.*" } | Select-Object -First 1).IPAddress
Write-Host ""
Write-Host "Xong. Các máy trong văn phòng mở trình duyệt vào: http://${ip}:$Cong"
```

- [ ] **Bước 5: Kiểm thử tay quy trình cài đặt**

Trên một máy Windows sạch: chạy `cai-dat.ps1`, chạy công cụ chuyển dữ liệu ở chế độ chạy
thử rồi chạy thật, mở trình duyệt từ **một máy khác trong cùng mạng** và kiểm bốn màn hình.
Ghi lại số dòng đối chiếu giữa Access và PostgreSQL.

- [ ] **Bước 6: Commit**

```bash
git add WebApp/tests/e2e WebApp/deploy WebApp/src/Qlgx.Api
git commit -m "Them kiem thu dau-cuoi va script cai dat cho giao xu

Mot tien trinh phuc vu ca API lan giao dien nen may giao xu khong phai cai them web server.
Script cai dat mo san cong tren tuong lua cho mang noi bo va dang ky Windows Service.

Co-Authored-By: Claude Opus 5 (1M context) <noreply@anthropic.com>"
```

---

## Tiêu chí hoàn thành Phase 1

Phase 1 xong khi tất cả các điều sau đúng:

- [ ] Bốn màn hình chạy được với dữ liệu thật của giáo xứ thí điểm.
- [ ] Công cụ chuyển dữ liệu chạy thử rồi chạy thật, **số dòng nguồn và đích khớp nhau** ở cả bốn bảng; mọi cảnh báo dữ liệu ngày đã được xem và chấp nhận.
- [ ] Hai người dùng trên hai máy khác nhau trong LAN cùng mở một gia đình, người lưu sau nhận thông báo bằng tiếng Việt thay vì ghi đè im lặng.
- [ ] Toàn bộ test tự động xanh: `dotnet test` và `npm test` và `npx playwright test`.
- [ ] Nhân viên văn phòng giáo xứ thí điểm dùng thử một tuần với bản Access chạy song song ở chế độ chỉ đọc làm lưới an toàn.

## Việc để lại cho Phase 2

Những phần đã khảo sát nhưng **cố ý chưa làm** trong Phase 1, ghi ra đây để không bị quên:

- Tab "Hôn phối", "Giáo lý", "Ơn gọi tận hiến", "Hội đoàn" của form giáo dân hiện chỉ có giao diện, chưa nối API — bốn bảng `HonPhoi`, `GiaoDanHonPhoi`, `ChiTietHoiDoan`, `TanHien` chưa đưa vào schema.
- Bảng `ChuyenXu` chưa chuyển đổi; cột `DaChuyenDi` của lưới giáo dân hiện suy từ gia đình chứ chưa từ lịch sử chuyển xứ.
- Xác thực và tài khoản người dùng (bảng `TaiKhoan`, mật khẩu băm đúng chuẩn).
- Lưu bố cục cột theo người dùng (bảng `UserGridPreference`), thay cho `GridColumns.xml`.
- Sổ bí tích, thống kê, biểu đồ, in chứng chỉ.
