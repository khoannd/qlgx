# Kế hoạch triển khai: Máy chủ QLGX Web + tự động sao lưu và phục hồi

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Đưa QLGX Web từ trạng thái "pilot chạy tay" thành một hệ thống cài được bằng một lệnh trên VPS trắng, tự sao lưu mã hoá lên Cloudflare R2 mỗi 6 giờ, và phục hồi được cả từ giao diện web lẫn từ dòng lệnh trên máy trắng.

**Architecture:** Docker Compose (postgres + api + caddy) trên một VPS. Toàn bộ việc sao lưu/phục hồi chạy **trên host, ngoài container**, do `qlgx-runner.sh` thực hiện dưới systemd timer; giao diện web chỉ **ghi một dòng vào bảng job** `cong_viec_sao_luu` và đọc trạng thái. Container API không bao giờ biết khoá R2. Phục hồi không ghi đè tại chỗ mà nạp sang CSDL mới rồi **hoán đổi tên**, nên quay lui được trong vài giây.

**Tech Stack:** .NET 10 (ASP.NET Core Minimal API, EF Core, Npgsql), PostgreSQL 17, React 19 + Vite + TypeScript + AG-Grid, Docker Compose, Caddy, restic, systemd, bash, bats-core.

**Spec:** `docs/superpowers/specs/2026-09-13-qlgx-trien-khai-sao-luu-design.md`

## Global Constraints

Áp dụng cho MỌI task bên dưới:

- **Trả lời người dùng bằng tiếng Việt.** Toàn bộ chuỗi hiển thị, thông báo lỗi, comment mã nguồn mới viết bằng tiếng Việt (theo `CLAUDE.md`).
- **Nhánh làm việc: `webapp-phase-1`.** Kiểm `git branch --show-current` trước khi commit. **Không `git checkout`** — có thể có phiên Claude khác làm việc song song; dùng `git worktree` nếu cần sang nhánh khác.
- **Không bao giờ ghi mật khẩu, khoá JWT, khoá R2, mật khẩu restic vào repo** — kể cả trong test, kể cả giá trị mẫu. Test sinh giá trị ngẫu nhiên tại chỗ (xem `QlgxApiFactory.cs`).
- **Không đụng tới bản desktop**: `Source/`, `BIN/`, `Release/`, các `.ps1` ở thư mục gốc, và `landing/` (đặc biệt các đường dẫn máy chủ cập nhật `/version.txt`, `/VersionConfig.xml`, `/download.asp`, `/help/thong_tin_cap_nhat.htm`, `/4.0/`).
- **Không cài thử bộ cài lên máy người dùng.** Mọi kiểm thử `install.sh` chạy trong container Docker dùng một lần.
- Kho GitHub: `khoannd/qlgx` (công khai). Máy chủ dùng sparse-checkout chỉ thư mục `WebApp`.
- Tên bảng PostgreSQL: **snake_case** (`cong_viec_sao_luu`), tên thực thể C#: **PascalCase** (`CongViecSaoLuu`) — theo đúng tiền lệ `nhap_du_lieu_job` / `NhapDuLieuJob`.
- Định dạng ngày hiển thị: **dd/mm/yyyy** (giờ: `dd/mm/yyyy HH:mm`). Không bao giờ hiển thị ISO cho người dùng cuối.
- Lưới dữ liệu trên giao diện: dùng **`GxGrid`** (AG-Grid), không dùng `<table>` thường.
- Chính sách phân quyền cho toàn bộ chức năng sao lưu/phục hồi: **`"QuanTriHeThong"`** (`LoaiTaiKhoan=9`).
- Nhịp sao lưu: **4 lần/ngày** (00:00, 06:00, 12:00, 18:00 giờ VN, `RandomizedDelaySec=300`).
- Giữ bản sao: `--keep-last 8 --keep-daily 30 --keep-weekly 12 --keep-monthly 24`.
- Ngưỡng cảnh báo vàng: **quá 8 giờ** không có bản sao mới.
- Khoá R2 + mật khẩu restic: **chỉ** ở `/etc/qlgx/backup.env`, `chmod 600`, chủ `root`.
- Lệnh kiểm thử:
  - Backend: `dotnet test WebApp/Qlgx.sln` (cần biến môi trường `QLGX_TEST_PG`)
  - Frontend: `cd WebApp/src/web && npm test`
  - Shell: `docker run --rm -v "$PWD:/code" bats/bats:1.11.0 /code/WebApp/tests/shell`

## Cấu trúc tệp

**Backend (.NET) — tạo mới**

| Tệp | Trách nhiệm |
|---|---|
| `WebApp/src/Qlgx.Api/KiemTraCauHinh.cs` | Hàm thuần kiểm cấu hình bắt buộc ở Production |
| `WebApp/src/Qlgx.Domain/Entities/CongViecSaoLuu.cs` | Thực thể job |
| `WebApp/src/Qlgx.Domain/Entities/BanSaoLuu.cs` | Bảng đệm danh sách snapshot |
| `WebApp/src/Qlgx.Domain/Entities/TrangThaiSaoLuu.cs` | Một dòng trạng thái tổng |
| `WebApp/src/Qlgx.Api/Services/SaoLuuService.cs` | Nghiệp vụ: tạo job, đọc trạng thái, tính đèn |
| `WebApp/src/Qlgx.Api/Endpoints/SaoLuuEndpoints.cs` | 6 route dưới `/api/sao-luu` |
| `WebApp/src/Qlgx.Api/Dtos/SaoLuuDtos.cs` | DTO |

**Backend — sửa:** `Program.cs`, `ChuoiKetNoiQuanTri.cs`, `TaoTaiKhoanQuanTri.cs`, `QlgxDbContext.cs`, `Dockerfile`.

**Shell (host) — tạo mới, tất cả trong `WebApp/scripts/`**

| Tệp | Trách nhiệm |
|---|---|
| `os_adapter.sh` | Che khác biệt apt/dnf, phát hiện distro |
| `chung.sh` | Hàm dùng chung: log, `set_env_kv`, `sinh_bi_mat`, `doi_container` |
| `install.sh` | Cài mới + cập nhật + tự kiểm chứng |
| `qlgx` | CLI vận hành (dispatcher mỏng) |
| `qlgx-runner.sh` | Vòng lấy job + thực thi sao lưu/kiểm tra/tải về |
| `qlgx-restore.sh` | Phục hồi độc lập (plan/apply) |
| `sql/00-vai-tro-rls.sql` | Tạo hai vai trò RLS, idempotent |
| `systemd/qlgx-runner.{service,timer}` | Bộ chạy job, mỗi phút |
| `systemd/qlgx-backup.{service,timer}` | Sao lưu định kỳ, 4 lần/ngày |
| `systemd/qlgx-verify.{service,timer}` | Diễn tập phục hồi, Chủ nhật 03:00 |
| `systemd/qlgx-update.{service,timer}` | Cập nhật hằng tuần — **mặc định tắt** |

**Frontend — tạo mới:** `src/web/src/screens/SaoLuuPage.tsx` (+ `.test.tsx`), `src/web/src/screens/SaoLuuPhucHoiModal.tsx` (+ `.test.tsx`), `src/web/src/components/BangCanhBaoSaoLuu.tsx`.
**Frontend — sửa:** `api/types.ts`, `api/client.ts`, `App.tsx`, `components/ThanhPhanKhung/SideNav.tsx`.

**Tài liệu — tạo mới:** `WebApp/docs/CAI-DAT-MAY-CHU.md`, `WebApp/docs/SAO-LUU-PHUC-HOI.md`, `WebApp/docs/THE-PHUC-HOI.md`. **Sửa:** `WebApp/TRIEN-KHAI.md`.

---

# Giai đoạn A — Vá các lỗ hổng chặn đường ra máy chủ thật

## Task 1: Chặn fallback âm thầm của chuỗi kết nối quản trị

Hiện tại thiếu `ConnectionStrings__QlgxQuanTri` thì hệ thống vẫn chạy, chỉ là **mất hẳn ranh giới BYPASSRLS** mà không báo gì. Ở Production điều đó phải là lỗi khởi động.

**Files:**
- Create: `WebApp/src/Qlgx.Api/KiemTraCauHinh.cs`
- Test: `WebApp/tests/Qlgx.Api.Tests/KiemTraCauHinhTests.cs`
- Modify: `WebApp/src/Qlgx.Api/Program.cs` (sau `var app = builder.Build();`, dòng 112)
- Modify: `WebApp/src/Qlgx.Api/ChuoiKetNoiQuanTri.cs` (cập nhật XML doc)

**Interfaces:**
- Consumes: `IConfiguration` (có sẵn)
- Produces: `Qlgx.Api.KiemTraCauHinh.LoiCauHinhSanXuat(IConfiguration cauHinh, bool laSanXuat) → string?` — trả `null` khi hợp lệ, ngược lại trả câu thông báo lỗi tiếng Việt.

- [ ] **Step 1: Viết test thất bại**

Tạo `WebApp/tests/Qlgx.Api.Tests/KiemTraCauHinhTests.cs`:

```csharp
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Qlgx.Api;

namespace Qlgx.Api.Tests;

public class KiemTraCauHinhTests
{
    private static IConfiguration CauHinh(string? nghiepVu, string? quanTri)
    {
        var cap = new Dictionary<string, string?> { ["ConnectionStrings:Qlgx"] = nghiepVu };
        if (quanTri is not null) cap["ConnectionStrings:QlgxQuanTri"] = quanTri;
        return new ConfigurationBuilder().AddInMemoryCollection(cap).Build();
    }

    private const string KetNoiApp = "Host=postgres;Database=qlgx;Username=qlgx_app;Password=a";
    private const string KetNoiQuanTri = "Host=postgres;Database=qlgx;Username=qlgx_admin;Password=b";

    [Fact]
    public void Ngoai_san_xuat_khong_bao_loi_du_thieu_chuoi_quan_tri()
    {
        KiemTraCauHinh.LoiCauHinhSanXuat(CauHinh(KetNoiApp, null), laSanXuat: false)
            .Should().BeNull();
    }

    [Fact]
    public void San_xuat_thieu_chuoi_quan_tri_thi_bao_loi()
    {
        KiemTraCauHinh.LoiCauHinhSanXuat(CauHinh(KetNoiApp, null), laSanXuat: true)
            .Should().Contain("QlgxQuanTri");
    }

    [Fact]
    public void San_xuat_hai_chuoi_cung_vai_tro_thi_bao_loi()
    {
        KiemTraCauHinh.LoiCauHinhSanXuat(CauHinh(KetNoiApp, KetNoiApp), laSanXuat: true)
            .Should().Contain("cùng một vai trò");
    }

    [Fact]
    public void San_xuat_hai_vai_tro_khac_nhau_thi_hop_le()
    {
        KiemTraCauHinh.LoiCauHinhSanXuat(CauHinh(KetNoiApp, KetNoiQuanTri), laSanXuat: true)
            .Should().BeNull();
    }

    [Fact]
    public void So_sanh_theo_vai_tro_chu_khong_theo_chuoi_tho()
    {
        // Cùng vai trò qlgx_app nhưng khác thứ tự tham số/khoảng trắng — vẫn phải bị chặn.
        var quanTri = "Username=qlgx_app;Host=postgres;Password=a;Database=qlgx";
        KiemTraCauHinh.LoiCauHinhSanXuat(CauHinh(KetNoiApp, quanTri), laSanXuat: true)
            .Should().Contain("cùng một vai trò");
    }
}
```

- [ ] **Step 2: Chạy test để chắc chắn nó thất bại**

Run: `dotnet test WebApp/Qlgx.sln --filter KiemTraCauHinhTests`
Expected: FAIL — biên dịch lỗi `The name 'KiemTraCauHinh' does not exist`.

- [ ] **Step 3: Viết cài đặt tối thiểu**

Tạo `WebApp/src/Qlgx.Api/KiemTraCauHinh.cs`:

```csharp
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Qlgx.Api;

/// <summary>
/// Kiểm những điều kiện cấu hình mà môi trường SẢN XUẤT bắt buộc phải có, ngay lúc khởi động.
///
/// Vì sao phải chặn cứng: <see cref="ChuoiKetNoiQuanTri"/> mặc định dùng lại
/// "ConnectionStrings:Qlgx" khi không có khoá riêng, và docker-compose.yml cũng đặt
/// QLGX_ADMIN_DB_USER mặc định bằng QLGX_APP_DB_USER. Hai lớp mặc định đó cộng lại nghĩa là:
/// quên cấu hình vai trò quản trị thì hệ thống VẪN CHẠY BÌNH THƯỜNG, chỉ khác là vai trò nghiệp
/// vụ phải có BYPASSRLS mới đăng nhập được — tức Row-Level Security bị vô hiệu cho MỌI truy vấn
/// mà không có dấu hiệu gì. Đó là kiểu hỏng tệ nhất: không làm gì sai hôm nay, chỉ âm thầm tháo
/// bỏ lớp phòng thủ cho tới ngày cần tới. Ở dev/test thì fallback vẫn tiện và vô hại (một vai
/// trò superuser duy nhất), nên chỉ chặn khi laSanXuat = true.
/// </summary>
public static class KiemTraCauHinh
{
    public static string? LoiCauHinhSanXuat(IConfiguration cauHinh, bool laSanXuat)
    {
        if (!laSanXuat) return null;

        var nghiepVu = cauHinh.GetConnectionString("Qlgx");
        var quanTri = cauHinh.GetConnectionString("QlgxQuanTri");

        if (string.IsNullOrWhiteSpace(nghiepVu))
            return "Thiếu ConnectionStrings__Qlgx. Đặt biến này trong tệp .env cạnh docker-compose.yml.";

        if (string.IsNullOrWhiteSpace(quanTri))
            return "Thiếu ConnectionStrings__QlgxQuanTri. Môi trường sản xuất BẮT BUỘC có hai vai " +
                   "trò CSDL riêng: một vai trò nghiệp vụ KHÔNG có BYPASSRLS và một vai trò quản " +
                   "trị CÓ BYPASSRLS. Xem WebApp/docs/CAI-DAT-MAY-CHU.md.";

        if (VaiTroCua(nghiepVu) == VaiTroCua(quanTri))
            return "ConnectionStrings__Qlgx và ConnectionStrings__QlgxQuanTri đang dùng CÙNG MỘT " +
                   "VAI TRÒ CSDL. Như vậy Row-Level Security bị vô hiệu hoàn toàn. Tạo hai vai trò " +
                   "riêng — xem WebApp/docs/CAI-DAT-MAY-CHU.md.";

        return null;
    }

    /// <summary>So sánh theo tên vai trò đã phân tích, không so chuỗi thô: hai chuỗi kết nối
    /// khác thứ tự tham số hoặc khoảng trắng vẫn có thể là cùng một vai trò.</summary>
    private static string VaiTroCua(string chuoiKetNoi)
    {
        try
        {
            return new NpgsqlConnectionStringBuilder(chuoiKetNoi).Username ?? "";
        }
        catch (ArgumentException)
        {
            // Chuỗi không phân tích được thì trả về chính nó — để hai chuỗi hỏng giống hệt nhau
            // vẫn bị coi là trùng vai trò, thay vì lọt lưới.
            return chuoiKetNoi;
        }
    }
}
```

- [ ] **Step 4: Chạy test để xác nhận nó đạt**

Run: `dotnet test WebApp/Qlgx.sln --filter KiemTraCauHinhTests`
Expected: PASS — 5/5.

- [ ] **Step 5: Nối vào Program.cs**

Chèn ngay sau dòng `var app = builder.Build();` trong `WebApp/src/Qlgx.Api/Program.cs`:

```csharp
// Chặn cứng cấu hình thiếu an toàn ở môi trường sản xuất TRƯỚC khi phục vụ yêu cầu nào — xem
// KiemTraCauHinh.cs để biết vì sao đây phải là lỗi khởi động chứ không phải cảnh báo.
var loiCauHinh = KiemTraCauHinh.LoiCauHinhSanXuat(
    builder.Configuration, app.Environment.IsProduction());
if (loiCauHinh is not null)
    throw new InvalidOperationException("Cấu hình sản xuất không hợp lệ: " + loiCauHinh);
```

- [ ] **Step 6: Cập nhật XML doc của ChuoiKetNoiQuanTri**

Trong `WebApp/src/Qlgx.Api/ChuoiKetNoiQuanTri.cs`, thay đoạn văn cuối cùng của phần `<summary>` (bắt đầu bằng "Mặc định (không đặt khoá riêng) dùng LẠI") bằng:

```
/// Mặc định (không đặt khoá riêng) dùng LẠI "ConnectionStrings:Qlgx" — CHỈ còn hợp lệ ở môi
/// trường dev/test (một vai trò `postgres` superuser duy nhất, tự động bỏ qua RLS bất kể chính
/// sách gì). Ở môi trường sản xuất, fallback này bị chặn cứng lúc khởi động: xem
/// KiemTraCauHinh.LoiCauHinhSanXuat và WebApp/docs/CAI-DAT-MAY-CHU.md.
```

- [ ] **Step 7: Chạy toàn bộ bộ test backend**

Run: `dotnet test WebApp/Qlgx.sln`
Expected: PASS toàn bộ. Các test hiện có chạy ở môi trường mặc định (không phải Production) nên không bị chặn.

- [ ] **Step 8: Commit**

```bash
git add WebApp/src/Qlgx.Api/KiemTraCauHinh.cs WebApp/src/Qlgx.Api/Program.cs \
        WebApp/src/Qlgx.Api/ChuoiKetNoiQuanTri.cs \
        WebApp/tests/Qlgx.Api.Tests/KiemTraCauHinhTests.cs
git commit -m "Chan fallback am tham cua chuoi ket noi quan tri o moi truong san xuat"
```

---

## Task 2: Endpoint readiness có chạm cơ sở dữ liệu

`/api/suc-khoe` trả "ok" ngay cả khi PostgreSQL đã chết — đó là liveness. Script cài và luồng cập nhật dựa vào tín hiệu "đã sẵn sàng" để quyết định có quay lui hay không, nên cần một tín hiệu trung thực. Giữ nguyên `/api/suc-khoe` (Docker healthcheck gọi mỗi 10 giây, phải nhẹ).

**Files:**
- Modify: `WebApp/src/Qlgx.Api/Program.cs:171-175` (thêm route mới ngay sau)
- Test: `WebApp/tests/Qlgx.Api.Tests/SanSangTests.cs`

**Interfaces:**
- Produces: `GET /api/suc-khoe/san-sang` → 200 `{ trangThai: "san-sang", soMigrationConThieu: 0 }` hoặc 503 `{ trangThai: "chua-san-sang", lyDo: "<tiếng Việt>" }`. Ẩn danh (`AllowAnonymous`).

- [ ] **Step 1: Viết test thất bại**

Tạo `WebApp/tests/Qlgx.Api.Tests/SanSangTests.cs`:

```csharp
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
```

- [ ] **Step 2: Chạy test để chắc chắn nó thất bại**

Run: `dotnet test WebApp/Qlgx.sln --filter SanSang`
Expected: FAIL — `/api/suc-khoe/san-sang` trả 404 thay vì 200/503.

- [ ] **Step 3: Viết cài đặt tối thiểu**

Chèn vào `WebApp/src/Qlgx.Api/Program.cs` ngay sau khối `app.MapGet("/api/suc-khoe", …)`:

```csharp
// Readiness — KHÁC liveness ở trên: có chạm CSDL thật. Script cài đặt và luồng cập nhật
// (WebApp/scripts/install.sh) dùng đúng endpoint này làm cổng quyết định "đã lên được chưa";
// nếu tín hiệu đó nói dối thì cơ chế tự quay lui khi cập nhật hỏng cũng vô nghĩa. Cố ý KHÔNG
// dùng cho Docker healthcheck (gọi mỗi 10 giây thì không nên mở kết nối CSDL mỗi lần).
app.MapGet("/api/suc-khoe/san-sang", async (QlgxDbContext db, CancellationToken ct) =>
{
    try
    {
        var conThieu = (await db.Database.GetPendingMigrationsAsync(ct)).Count();
        if (conThieu > 0)
            return Results.Json(new
            {
                trangThai = "chua-san-sang",
                soMigrationConThieu = conThieu,
                lyDo = $"Còn {conThieu} migration chưa áp dụng."
            }, statusCode: StatusCodes.Status503ServiceUnavailable);

        return Results.Ok(new { trangThai = "san-sang", soMigrationConThieu = 0 });
    }
    catch (Exception ex)
    {
        return Results.Json(new
        {
            trangThai = "chua-san-sang",
            soMigrationConThieu = -1,
            lyDo = "Không kết nối được cơ sở dữ liệu: " + ex.Message
        }, statusCode: StatusCodes.Status503ServiceUnavailable);
    }
}).AllowAnonymous();
```

- [ ] **Step 4: Chạy test để xác nhận nó đạt**

Run: `dotnet test WebApp/Qlgx.sln --filter SanSang`
Expected: PASS — 3/3.

- [ ] **Step 5: Commit**

```bash
git add WebApp/src/Qlgx.Api/Program.cs WebApp/tests/Qlgx.Api.Tests/SanSangTests.cs
git commit -m "Them endpoint readiness /api/suc-khoe/san-sang co cham CSDL"
```

---

## Task 3: Cho phép CLI tự tạo giáo xứ đầu tiên

`tao-tai-khoan-quan-tri` đòi hỏi giáo xứ **đã tồn tại**. Trên máy vừa cài thì chưa có giáo xứ nào, nên bước 9 của script cài không chạy được nếu không có thao tác tay.

**Files:**
- Modify: `WebApp/src/Qlgx.Api/TaoTaiKhoanQuanTri.cs:42-67` (nhánh tra cứu giáo xứ) và phần `<summary>`
- Test: `WebApp/tests/Qlgx.Api.Tests/TaoTaiKhoanQuanTriTests.cs`

**Interfaces:**
- Consumes: `ChuoiKetNoiQuanTri.Doc(IConfiguration)` (có sẵn)
- Produces: biến môi trường mới `QLGX_ADMIN_TAO_GIAO_XU_NEU_CHUA_CO=true`. Khi bật và không tìm thấy giáo xứ theo `QLGX_ADMIN_GIAO_XU_TEN`, lệnh tạo mới `GiaoXu` với `MaGiaoXuCu` = giá trị lớn nhất hiện có + 1, rồi tiếp tục như thường. Hàm nội bộ mới: `TaoTaiKhoanQuanTri.LayHoacTaoGiaoXu(QlgxDbContext db, string? giaoXuIdChuoi, string? giaoXuTen, bool taoNeuChuaCo) → Task<Guid>`.

- [ ] **Step 1: Viết test thất bại**

Tạo `WebApp/tests/Qlgx.Api.Tests/TaoTaiKhoanQuanTriTests.cs`:

```csharp
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Qlgx.Api;
using Qlgx.Domain.Entities;

namespace Qlgx.Api.Tests;

public class TaoTaiKhoanQuanTriTests(QlgxApiFactory factory) : IClassFixture<QlgxApiFactory>
{
    [Fact]
    public async Task Khong_bat_co_tao_va_khong_tim_thay_thi_bao_loi()
    {
        await using var db = factory.TaoContextThuan();

        var hanhDong = async () => await TaoTaiKhoanQuanTri.LayHoacTaoGiaoXu(
            db, null, "Giao xu khong ton tai ZZZ", taoNeuChuaCo: false);

        await hanhDong.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*khong ton tai ZZZ*");
    }

    [Fact]
    public async Task Bat_co_tao_thi_tao_moi_va_tra_ve_id()
    {
        await using var db = factory.TaoContextThuan();
        var ten = "Giao xu Moi " + Guid.NewGuid().ToString("N")[..8];

        var id = await TaoTaiKhoanQuanTri.LayHoacTaoGiaoXu(db, null, ten, taoNeuChuaCo: true);

        var daTao = await db.GiaoXu.SingleAsync(g => g.Id == id);
        daTao.TenGiaoXu.Should().Be(ten);
        daTao.MaGiaoXuCu.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Chay_lai_voi_cung_ten_thi_dung_lai_giao_xu_cu_khong_tao_trung()
    {
        await using var db = factory.TaoContextThuan();
        var ten = "Giao xu Lap " + Guid.NewGuid().ToString("N")[..8];

        var lan1 = await TaoTaiKhoanQuanTri.LayHoacTaoGiaoXu(db, null, ten, taoNeuChuaCo: true);
        var lan2 = await TaoTaiKhoanQuanTri.LayHoacTaoGiaoXu(db, null, ten, taoNeuChuaCo: true);

        lan2.Should().Be(lan1);
        (await db.GiaoXu.CountAsync(g => g.TenGiaoXu == ten)).Should().Be(1);
    }

    [Fact]
    public async Task Ma_giao_xu_cu_luon_lon_hon_moi_ma_da_co()
    {
        await using var db = factory.TaoContextThuan();
        var maLonNhatTruoc = await db.GiaoXu.MaxAsync(g => (int?)g.MaGiaoXuCu) ?? 0;

        var id = await TaoTaiKhoanQuanTri.LayHoacTaoGiaoXu(
            db, null, "Giao xu Ma " + Guid.NewGuid().ToString("N")[..8], taoNeuChuaCo: true);

        (await db.GiaoXu.SingleAsync(g => g.Id == id)).MaGiaoXuCu
            .Should().BeGreaterThan(maLonNhatTruoc);
    }
}
```

- [ ] **Step 2: Chạy test để chắc chắn nó thất bại**

Run: `dotnet test WebApp/Qlgx.sln --filter TaoTaiKhoanQuanTriTests`
Expected: FAIL — biên dịch lỗi, `LayHoacTaoGiaoXu` chưa tồn tại.

- [ ] **Step 3: Tách hàm và thêm khả năng tạo giáo xứ**

Trong `WebApp/src/Qlgx.Api/TaoTaiKhoanQuanTri.cs`, thay toàn bộ khối từ `var giaoXuIdChuoi = …` (dòng 42) tới hết khối `else { … }` (dòng 67) bằng:

```csharp
        var giaoXuIdChuoi = Environment.GetEnvironmentVariable("QLGX_ADMIN_GIAO_XU_ID");
        var giaoXuTen = Environment.GetEnvironmentVariable("QLGX_ADMIN_GIAO_XU_TEN");
        var taoNeuChuaCo = string.Equals(
            Environment.GetEnvironmentVariable("QLGX_ADMIN_TAO_GIAO_XU_NEU_CHUA_CO"),
            "true", StringComparison.OrdinalIgnoreCase);
        if (giaoXuIdChuoi is null && giaoXuTen is null)
            throw new InvalidOperationException(
                "Can mot trong hai bien: QLGX_ADMIN_GIAO_XU_ID (GUID) hoac QLGX_ADMIN_GIAO_XU_TEN (ten giao xu).");

        // Dung chuoi ket noi QUAN TRI (vai tro co BYPASSRLS) — tao tai khoan dau tien khong the
        // di qua vai tro bi RLS han che, xem ChuoiKetNoiQuanTri.
        var options = new DbContextOptionsBuilder<QlgxDbContext>()
            .UseNpgsql(ChuoiKetNoiQuanTri.Doc(cauHinh))
            .Options;
        await using var db = new QlgxDbContext(options);

        var giaoXuId = await LayHoacTaoGiaoXu(db, giaoXuIdChuoi, giaoXuTen, taoNeuChuaCo);
```

Xoá khối `var options = …; await using var db = …;` cũ (dòng 50-53) vì đã chuyển lên trên. Rồi thêm hàm mới vào cuối lớp, ngay trước dấu `}` đóng lớp:

```csharp
    /// <summary>
    /// Tra Id giao xu, va TAO MOI neu chua co (chi khi taoNeuChuaCo = true).
    ///
    /// Vi sao can: script cai dat (WebApp/scripts/install.sh) chay tren mot may hoan toan trang,
    /// bang GiaoXu con rong — khong co giao xu nao de tra cuu. Truoc day nguoi cai phai tu chen
    /// mot dong bang psql roi moi chay duoc lenh nay, dung mot buoc thao tac tay giua mot quy
    /// trinh cai dat le ra tu dong hoan toan.
    ///
    /// Idempotent: goi lai voi cung ten thi tra ve dung giao xu cu, khong tao trung — script cai
    /// dat duoc thiet ke de chay lai nhieu lan.
    /// </summary>
    public static async Task<Guid> LayHoacTaoGiaoXu(
        QlgxDbContext db, string? giaoXuIdChuoi, string? giaoXuTen, bool taoNeuChuaCo)
    {
        if (giaoXuIdChuoi is not null)
        {
            var id = Guid.Parse(giaoXuIdChuoi);
            if (!await db.GiaoXu.AnyAsync(g => g.Id == id))
                throw new InvalidOperationException($"Khong tim thay giao xu co Id={id}.");
            return id;
        }

        var daCo = await db.GiaoXu.FirstOrDefaultAsync(g => g.TenGiaoXu == giaoXuTen);
        if (daCo is not null) return daCo.Id;

        if (!taoNeuChuaCo)
            throw new InvalidOperationException(
                $"Khong tim thay giao xu ten '{giaoXuTen}'. Dat QLGX_ADMIN_TAO_GIAO_XU_NEU_CHUA_CO=true " +
                "neu muon tu tao moi giao xu nay.");

        // MaGiaoXuCu la so nguyen ke thua tu Access, khong tu tang o CSDL — tu tinh so ke tiep.
        var maKeTiep = (await db.GiaoXu.MaxAsync(g => (int?)g.MaGiaoXuCu) ?? 0) + 1;
        var moi = new GiaoXu { TenGiaoXu = giaoXuTen!, MaGiaoXuCu = maKeTiep };
        db.GiaoXu.Add(moi);
        await db.SaveChangesAsync();
        Console.WriteLine($"Da tao giao xu moi '{giaoXuTen}' (MaGiaoXuCu={maKeTiep}).");
        return moi.Id;
    }
```

- [ ] **Step 4: Cập nhật phần `<summary>` của lớp**

Thêm dòng sau vào danh sách biến môi trường trong XML doc, ngay dưới dòng `QLGX_ADMIN_GIAO_XU_TEN`:

```
///   QLGX_ADMIN_TAO_GIAO_XU_NEU_CHUA_CO — tuỳ chọn, "true" thì TẠO MỚI giáo xứ theo
///                                 QLGX_ADMIN_GIAO_XU_TEN nếu chưa tồn tại. Dùng cho script cài
///                                 đặt trên máy trắng; idempotent (chạy lại không tạo trùng).
```

- [ ] **Step 5: Chạy test để xác nhận nó đạt**

Run: `dotnet test WebApp/Qlgx.sln --filter TaoTaiKhoanQuanTriTests`
Expected: PASS — 4/4.

- [ ] **Step 6: Chạy toàn bộ bộ test backend**

Run: `dotnet test WebApp/Qlgx.sln`
Expected: PASS toàn bộ.

- [ ] **Step 7: Commit**

```bash
git add WebApp/src/Qlgx.Api/TaoTaiKhoanQuanTri.cs \
        WebApp/tests/Qlgx.Api.Tests/TaoTaiKhoanQuanTriTests.cs
git commit -m "CLI tao-tai-khoan-quan-tri co the tu tao giao xu dau tien"
```

---

## Task 4: Bổ sung Chromium vào image Docker để in PDF chạy được

Image chạy hiện tại chỉ cài thêm `curl`. Toàn bộ đường in PDF là mẫu HTML → Chromium không giao diện (Playwright) → PDF trong bộ nhớ, nên **cả 13 mẫu in đều hỏng** ngay lần đầu người dùng bấm in trên máy chủ thật, dù chạy tốt khi phát triển trên Windows.

**Files:**
- Modify: `WebApp/Dockerfile` (giai đoạn 3, sau dòng cài `curl`)
- Create: `WebApp/tests/shell/kiem-tra-in-pdf.sh`

**Interfaces:**
- Produces: image có biến môi trường `PLAYWRIGHT_BROWSERS_PATH=/ms-playwright` và Chromium đã tải sẵn tại đó, đọc được bởi user `app`. Script `WebApp/tests/shell/kiem-tra-in-pdf.sh` trả mã thoát 0 khi in được.

- [ ] **Step 1: Viết kiểm thử khói thất bại**

Tạo `WebApp/tests/shell/kiem-tra-in-pdf.sh`:

```bash
#!/usr/bin/env bash
# Kiem thu khoi: image Docker co in duoc PDF khong.
#
# Vi sao phai kiem o TANG IMAGE chu khong phai bang test .NET thong thuong: loi o day khong
# phai loi logic ma la loi THIEU THU VIEN HE THONG trong image chay. Bo test .NET chay tren may
# phat trien (Windows, co san Chromium cua Playwright) se luon xanh du image hoan toan hong.
set -euo pipefail

readonly ANH="${1:-qlgx-api:kiem-thu}"

echo "==> Kiem tra Chromium co trong image $ANH khong"
docker run --rm --entrypoint sh "$ANH" -c '
  set -e
  test -n "$PLAYWRIGHT_BROWSERS_PATH" || { echo "THIEU bien PLAYWRIGHT_BROWSERS_PATH"; exit 1; }
  duong_dan=$(find "$PLAYWRIGHT_BROWSERS_PATH" -name headless_shell -o -name chrome | head -1)
  test -n "$duong_dan" || { echo "KHONG tim thay Chromium trong $PLAYWRIGHT_BROWSERS_PATH"; exit 1; }
  echo "Tim thay: $duong_dan"
  "$duong_dan" --headless --no-sandbox --version
'
echo "==> DAT: image co Chromium chay duoc"
```

- [ ] **Step 2: Build image và chạy kiểm thử để chắc chắn nó thất bại**

Run:
```bash
cd WebApp && docker build -t qlgx-api:kiem-thu . && bash tests/shell/kiem-tra-in-pdf.sh
```
Expected: FAIL — `THIEU bien PLAYWRIGHT_BROWSERS_PATH`.

- [ ] **Step 3: Sửa Dockerfile**

Trong `WebApp/Dockerfile`, thay khối cài `curl` ở giai đoạn 3 bằng:

```dockerfile
# curl de docker-compose healthcheck goi /api/suc-khoe tu BEN TRONG container — anh nen chinh
# thuc khong co san cong cu goi HTTP nao.
#
# Chromium + thu vien he thong cua no: BAT BUOC cho toan bo duong in PDF (mau HTML -> Chromium
# khong giao dien qua Playwright -> PDF trong bo nho, xem BoTrinhDuyet.cs). Thieu chung thi ca
# 13 mau in deu hong ngay lan dau nguoi dung bam in tren may chu that, du chay tot khi phat
# trien tren Windows. `playwright install --with-deps chromium` tu cai dung bo thu viene he
# thong can thiet cho ban Debian nen, khong phai liet ke tay ~20 goi va tu bao tri danh sach do.
ENV PLAYWRIGHT_BROWSERS_PATH=/ms-playwright
RUN apt-get update && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=build-api /app/publish .
COPY --from=build-web /web/dist ./wwwroot
# Chay bo cai trinh duyet cua Playwright BANG chinh assembly vua publish — dung dung phien ban
# Playwright ma ung dung tham chieu, khong the lech phien ban trinh duyet/thu vien.
RUN dotnet Qlgx.Api.dll --playwright-install 2>/dev/null \
    || pwsh -NoLogo -NonInteractive -Command "& { ./playwright.ps1 install --with-deps chromium }" \
    || (apt-get update \
        && apt-get install -y --no-install-recommends nodejs npm \
        && npx --yes playwright@1.49.0 install --with-deps chromium \
        && apt-get purge -y nodejs npm && apt-get autoremove -y \
        && rm -rf /var/lib/apt/lists/*)
# Chromium duoc tai duoi quyen root nhung chay duoi user "app" — mo quyen doc/thuc thi.
RUN chmod -R a+rX /ms-playwright
```

Xoá hai dòng `WORKDIR /app` / `COPY` cũ nếu bị lặp — chúng đã nằm trong khối trên.

**Lưu ý cho người thực hiện:** ba nhánh `||` là có chủ đích, thử lần lượt ba cách chính thức của Playwright cho .NET; giữ nhánh nào chạy được trên máy build thật và **xoá hai nhánh còn lại** để Dockerfile không mơ hồ. Ghi lại lựa chọn bằng một comment một dòng.

- [ ] **Step 4: Build lại và chạy kiểm thử để xác nhận nó đạt**

Run:
```bash
cd WebApp && docker build -t qlgx-api:kiem-thu . && bash tests/shell/kiem-tra-in-pdf.sh
```
Expected: PASS — in ra đường dẫn Chromium và số phiên bản.

- [ ] **Step 5: Kiểm chứng in PDF đầu-cuối thật**

Run:
```bash
cd WebApp && docker compose up -d && \
  curl -fsS -X POST http://localhost:8080/api/auth/dang-nhap \
    -H 'Content-Type: application/json' \
    -d "{\"tenTaiKhoan\":\"$QLGX_ADMIN_TEN_TAI_KHOAN\",\"matKhau\":\"$QLGX_ADMIN_MAT_KHAU\"}" \
  | grep -o '"token":"[^"]*"'
```
Rồi gọi một endpoint in bất kỳ với token đó và kiểm tệp trả về bắt đầu bằng `%PDF-`:
```bash
curl -fsS -H "Authorization: Bearer <token>" \
  "http://localhost:8080/api/gia-dinh/<id>/in-phieu?khoGiay=A4" -o /tmp/thu.pdf
head -c 5 /tmp/thu.pdf   # phai in ra: %PDF-
```
Expected: `%PDF-`. Nếu không, dừng lại và sửa Dockerfile trước khi đi tiếp — mọi task sau đều giả định image này chạy được.

- [ ] **Step 6: Commit**

```bash
git add WebApp/Dockerfile WebApp/tests/shell/kiem-tra-in-pdf.sh
git commit -m "Them Chromium/Playwright vao image Docker de in PDF chay duoc tren may chu"
```

---

## Task 4B: Giảm dung lượng ảnh đại diện và chốt ngân sách bằng test

Ảnh `bytea` chiếm ~95% khối lượng sao lưu ở quy mô lớn (xem thiết kế mục 13.1). Quyết định là
**giữ ảnh trong PostgreSQL** ở giai đoạn này, nhưng giảm dung lượng mỗi ảnh ngay bây giờ.

Việc nén và chuẩn hoá **đã có sẵn** trong `XuLyAnh.cs` (chặn 8 MB trước giải mã, giải mã thật
bằng SkiaSharp, chỉ nhận JPEG/PNG/WebP, thu nhỏ về cạnh dài 640px, nền trắng, JPEG 85). Task này
**không viết lại** phần đó — chỉ đổi định dạng đầu ra và thêm hàng rào chống thoái lui.

> **KHÔNG hạ độ phân giải xuống dưới 640px.** Con số này có căn cứ ghi ngay trong mã: ảnh 3×4 in
> ở 300dpi cần 354×472px. Hạ xuống 480px sẽ xuống dưới ngưỡng in và làm hỏng 13 mẫu in.

**Files:**
- Modify: `WebApp/src/Qlgx.Api/Anh/XuLyAnh.cs:34-36,97-102`
- Test: `WebApp/tests/Qlgx.Api.Tests/XuLyAnhNganSachTests.cs`
- Modify: `WebApp/tests/Qlgx.Api.Tests/AnhDaiDienTests.cs` (nếu có khẳng định `image/jpeg` cứng)

**Interfaces:**
- Produces: `XuLyAnh.XuLy` trả `LoaiNoiDung = "image/webp"` (hoặc giữ `"image/jpeg"` nếu WebP
  không đạt — xem Step 4); hằng mới `XuLyAnh.NganSachByteMoiAnh = 60 * 1024`.
- Không đổi chữ ký hàm, không đổi lược đồ CSDL: `AnhDaiDienLoaiNoiDung` vốn đã lưu kiểu nội dung
  theo từng bản ghi, nên ảnh cũ dạng JPEG vẫn đọc và in bình thường.

- [ ] **Step 1: Viết test ngân sách thất bại**

Tạo `WebApp/tests/Qlgx.Api.Tests/XuLyAnhNganSachTests.cs`:

```csharp
using FluentAssertions;
using Qlgx.Api.Anh;
using SkiaSharp;

namespace Qlgx.Api.Tests;

/// <summary>
/// Hàng rào chống thoái lui về DUNG LƯỢNG. Không có test này thì một thay đổi vô tình ở tham số
/// nén sẽ nhân đôi kích thước cơ sở dữ liệu mà không ai phát hiện cho tới lúc quá muộn — ảnh
/// chiếm khoảng 95% khối lượng sao lưu ở quy mô lớn (xem thiết kế mục 13.1).
/// </summary>
public class XuLyAnhNganSachTests
{
    /// <summary>Ảnh chân dung tổng hợp có độ phức tạp gần với ảnh chụp thật: chuyển sắc cộng
    /// nhiễu giả. Ảnh một màu phẳng nén xuống vài trăm byte và sẽ làm test vô nghĩa.</summary>
    private static byte[] AnhChanDungMau(int rong = 1200, int cao = 1600)
    {
        using var bm = new SKBitmap(rong, cao);
        var ngau = new Random(20260913);
        for (var y = 0; y < cao; y++)
        for (var x = 0; x < rong; x++)
            bm.SetPixel(x, y, new SKColor(
                (byte)((x * 255 / rong + ngau.Next(24)) % 256),
                (byte)((y * 255 / cao + ngau.Next(24)) % 256),
                (byte)((x + y) % 256)));
        using var anh = SKImage.FromBitmap(bm);
        using var duLieu = anh.Encode(SKEncodedImageFormat.Jpeg, 95);
        return duLieu.ToArray();
    }

    [Fact]
    public void Anh_chan_dung_sau_xu_ly_khong_vuot_ngan_sach()
    {
        var (ketQua, loi) = XuLyAnh.XuLy(AnhChanDungMau());

        loi.Should().BeNull();
        ketQua!.DuLieu.Length.Should().BeLessThanOrEqualTo(XuLyAnh.NganSachByteMoiAnh,
            "anh chiem ~95% khoi luong sao luu o quy mo lon — vuot ngan sach la loi that");
    }

    [Fact]
    public void Van_giu_du_do_phan_giai_de_in_3x4_o_300dpi()
    {
        var (ketQua, _) = XuLyAnh.XuLy(AnhChanDungMau());

        using var daGiaiMa = SKBitmap.Decode(ketQua!.DuLieu);
        // 3x4cm o 300dpi = 354x472px. Canh dai phai >= 472 thi in moi khong bi ro net.
        Math.Max(daGiaiMa.Width, daGiaiMa.Height).Should().BeGreaterThanOrEqualTo(472);
    }

    [Fact]
    public void Loai_noi_dung_tra_ve_khop_dinh_dang_thuc_te_cua_du_lieu()
    {
        var (ketQua, _) = XuLyAnh.XuLy(AnhChanDungMau());

        using var luong = new SKMemoryStream(ketQua!.DuLieu);
        using var codec = SKCodec.Create(luong);
        var mongDoi = codec!.EncodedFormat switch
        {
            SKEncodedImageFormat.Webp => "image/webp",
            SKEncodedImageFormat.Jpeg => "image/jpeg",
            _ => "khong-mong-doi",
        };
        ketQua.LoaiNoiDung.Should().Be(mongDoi);
    }

    [Fact]
    public void Anh_nho_hon_khung_khong_bi_phong_to_len()
    {
        var (ketQua, _) = XuLyAnh.XuLy(AnhChanDungMau(300, 400));

        using var daGiaiMa = SKBitmap.Decode(ketQua!.DuLieu);
        daGiaiMa.Width.Should().Be(300);
        daGiaiMa.Height.Should().Be(400);
    }
}
```

- [ ] **Step 2: Chạy test để chắc chắn nó thất bại**

Run: `dotnet test WebApp/Qlgx.sln --filter XuLyAnhNganSachTests`
Expected: FAIL — `XuLyAnh.NganSachByteMoiAnh` chưa tồn tại.

- [ ] **Step 3: Đo trước, chọn sau**

Trước khi sửa gì, **đo dung lượng thật** của cả hai phương án trên cùng ảnh mẫu. Viết một test
tạm in ra số byte của: JPEG 85 (hiện tại), JPEG 80, WebP 80, WebP 85. Chạy và ghi lại kết quả
vào phần mô tả commit.

Lý do phải đo chứ không giả định: bộ mã hoá của SkiaSharp **đã từng vấp** một lỗi tương tự —
comment ở dòng 86-89 của `XuLyAnh.cs` ghi lại việc `SKColorType.Rgb888x` khiến `Encode(Jpeg,…)`
trả `null` dù dữ liệu ảnh hợp lệ. WebP có thể trả `null` theo cùng kiểu trên build này.

- [ ] **Step 4: Sửa `XuLyAnh.cs` theo kết quả đo**

Nếu WebP chạy và nhỏ hơn rõ rệt, thay khối mã hoá (dòng 97-102):

```csharp
        // WebP thay cho JPEG: nho hon khoang 25-35% o cung chat luong thi giac va cung do phan
        // giai, khong dung toi nguong in 472px. O quy mo 1,5 trieu anh, do la vai chuc GB — anh
        // chiem khoang 95% khoi luong sao luu (xem thiet ke muc 13.1). Con so nen thuc te da do
        // duoc ghi trong commit gioi thieu thay doi nay.
        //
        // KHONG doi anh cu da luu: cot AnhDaiDienLoaiNoiDung von luu kieu noi dung theo TUNG ban
        // ghi, nen anh JPEG cu van doc va in binh thuong ben canh anh WebP moi.
        using var anhMaHoa = SKImage.FromBitmap(mat);
        using var duLieuNen = anhMaHoa.Encode(SKEncodedImageFormat.Webp, ChatLuongWebp);
        if (duLieuNen is null)
            return (null, new LoiXuLyAnh("Không nén được ảnh sau khi xử lý — vui lòng thử một ảnh khác."));

        return (new AnhDaXuLy(duLieuNen.ToArray(), "image/webp"), null);
```

và thay hằng chất lượng:

```csharp
    private const int ChatLuongWebp = 80;

    /// <summary>Ngân sách dung lượng cho MỘT ảnh sau xử lý. Có test giữ ngưỡng này
    /// (XuLyAnhNganSachTests) — ảnh chiếm khoảng 95% khối lượng sao lưu ở quy mô lớn, nên một
    /// thay đổi vô tình ở tham số nén sẽ nhân đôi kích thước CSDL mà không ai thấy.</summary>
    public const int NganSachByteMoiAnh = 60 * 1024;
```

Nếu WebP **không** chạy hoặc không nhỏ hơn đáng kể: giữ JPEG, chỉ hạ `ChatLuongJpeg` từ 85 xuống
80, vẫn thêm `NganSachByteMoiAnh`. Ghi rõ lựa chọn và số đo bằng một comment tại chỗ.

- [ ] **Step 5: Kiểm chứng đường in PDF vẫn đọc được định dạng mới**

Đây là rủi ro thật nếu chọn WebP: mẫu in nhúng ảnh dạng data URI rồi để Chromium vẽ ra PDF.

Run: tải một ảnh lên cho một giáo dân qua API, rồi in lý lịch cá nhân của chính người đó, mở PDF
và **nhìn** xem ảnh có hiện không.
```bash
curl -fsS -H "Authorization: Bearer <token>" \
  "http://localhost:5096/api/giao-dan/<id>/in-ly-lich" -o /tmp/thu-anh.pdf
```
Expected: PDF mở được và **ảnh hiện đúng**, không phải ô trống. Ảnh không hiện thì lùi về JPEG —
tiết kiệm dung lượng không đáng đánh đổi bằng việc hỏng ảnh trên giấy tờ giáo xứ.

- [ ] **Step 6: Chạy test để xác nhận nó đạt**

Run: `dotnet test WebApp/Qlgx.sln --filter "XuLyAnhNganSachTests|AnhDaiDien"`
Expected: PASS. Nếu `AnhDaiDienTests` có khẳng định cứng `"image/jpeg"`, sửa thành khẳng định
khớp với hằng định dạng thay vì chuỗi viết cứng.

- [ ] **Step 7: Chạy toàn bộ bộ test backend**

Run: `dotnet test WebApp/Qlgx.sln`
Expected: PASS toàn bộ.

- [ ] **Step 8: Commit**

```bash
git add WebApp/src/Qlgx.Api/Anh/XuLyAnh.cs \
        WebApp/tests/Qlgx.Api.Tests/XuLyAnhNganSachTests.cs \
        WebApp/tests/Qlgx.Api.Tests/AnhDaiDienTests.cs
git commit -m "Giam dung luong anh dai dien va chot ngan sach bang test

<ghi so byte da do duoc cho tung phuong an o day>"
```

---

# Giai đoạn B — Bảng công việc và API

## Task 5: Ba bảng nền cho sao lưu

Bảng job là **giao diện duy nhất** giữa ứng dụng web và bộ chạy trên host. API chỉ INSERT/SELECT; runner mới là bên thực thi.

**Files:**
- Create: `WebApp/src/Qlgx.Domain/Entities/CongViecSaoLuu.cs`
- Create: `WebApp/src/Qlgx.Domain/Entities/BanSaoLuu.cs`
- Create: `WebApp/src/Qlgx.Domain/Entities/TrangThaiSaoLuu.cs`
- Modify: `WebApp/src/Qlgx.Data/QlgxDbContext.cs` (thêm ba `DbSet`, cạnh `NhapDuLieuJob` dòng 55)
- Create: migration `WebApp/src/Qlgx.Data/Migrations/*_ThemBangSaoLuu.cs` (sinh bằng `dotnet ef`)
- Test: `WebApp/tests/Qlgx.Api.Tests/BangSaoLuuTests.cs`

**Interfaces:**
- Produces: bảng `cong_viec_sao_luu`, `ban_sao_luu`, `trang_thai_sao_luu`; các `DbSet<CongViecSaoLuu> CongViecSaoLuu`, `DbSet<BanSaoLuu> BanSaoLuu`, `DbSet<TrangThaiSaoLuu> TrangThaiSaoLuu` trên `QlgxDbContext`.
- Hằng số trạng thái/loại được các task 6, 14, 18 dùng lại nguyên văn.

- [ ] **Step 1: Viết test thất bại**

Tạo `WebApp/tests/Qlgx.Api.Tests/BangSaoLuuTests.cs`:

```csharp
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Qlgx.Domain.Entities;

namespace Qlgx.Api.Tests;

public class BangSaoLuuTests(QlgxApiFactory factory) : IClassFixture<QlgxApiFactory>
{
    [Fact]
    public async Task Ghi_va_doc_lai_duoc_mot_cong_viec()
    {
        await using var db = factory.TaoContextThuan();
        var cv = new CongViecSaoLuu { Loai = LoaiCongViecSaoLuu.SaoLuu, ThamSoJson = "{\"nhan\":\"thu\"}" };

        db.CongViecSaoLuu.Add(cv);
        await db.SaveChangesAsync();

        var doc = await db.CongViecSaoLuu.SingleAsync(x => x.Id == cv.Id);
        doc.TrangThai.Should().Be(TrangThaiCongViec.Cho);
        doc.TaoLuc.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromMinutes(1));
        doc.BatDauLuc.Should().BeNull();
    }

    [Fact]
    public async Task Bang_ban_sao_luu_dung_id_snapshot_lam_khoa_chinh()
    {
        await using var db = factory.TaoContextThuan();
        var ma = "sn" + Guid.NewGuid().ToString("N")[..8];

        db.BanSaoLuu.Add(new BanSaoLuu
        {
            Id = ma, ThoiDiem = DateTimeOffset.UtcNow, Nhan = "tu-dong",
            KichThuocByte = 1234, SoGiaoDan = 2050, SoGiaDinh = 40, Nguon = NguonBanSao.TuDong,
        });
        await db.SaveChangesAsync();

        (await db.BanSaoLuu.SingleAsync(x => x.Id == ma)).SoGiaoDan.Should().Be(2050);
    }

    [Fact]
    public async Task Trang_thai_sao_luu_chi_co_dung_mot_dong()
    {
        await using var db = factory.TaoContextThuan();

        db.TrangThaiSaoLuu.Add(new TrangThaiSaoLuu { Id = 1, SaoLuuGanNhat = DateTimeOffset.UtcNow });
        await db.SaveChangesAsync();

        db.TrangThaiSaoLuu.Add(new TrangThaiSaoLuu { Id = 1 });
        var hanhDong = async () => await db.SaveChangesAsync();
        await hanhDong.Should().ThrowAsync<Exception>();
    }
}
```

- [ ] **Step 2: Chạy test để chắc chắn nó thất bại**

Run: `dotnet test WebApp/Qlgx.sln --filter BangSaoLuuTests`
Expected: FAIL — biên dịch lỗi, các kiểu chưa tồn tại.

- [ ] **Step 3: Tạo ba thực thể**

`WebApp/src/Qlgx.Domain/Entities/CongViecSaoLuu.cs`:

```csharp
namespace Qlgx.Domain.Entities;

/// <summary>Loại công việc — chuỗi thay vì enum, cùng lý do với NhapDuLieuJob.TrangThai: bộ chạy
/// phía host là một script bash đọc thẳng cột này bằng psql, không có tầng ánh xạ enum nào.</summary>
public static class LoaiCongViecSaoLuu
{
    public const string SaoLuu = "sao_luu";
    public const string PhucHoi = "phuc_hoi";
    public const string KiemTra = "kiem_tra";
    public const string DienTap = "dien_tap";
    public const string TaiVe = "tai_ve";
    public const string DongBoDanhSach = "dong_bo_danh_sach";

    public static readonly string[] HopLe =
        [SaoLuu, PhucHoi, KiemTra, DienTap, TaiVe, DongBoDanhSach];
}

public static class TrangThaiCongViec
{
    public const string Cho = "cho";
    public const string DangChay = "dang_chay";
    public const string Xong = "xong";
    public const string Loi = "loi";
}

/// <summary>
/// Hàng đợi công việc sao lưu/phục hồi — ranh giới DUY NHẤT giữa ứng dụng web và bộ chạy trên
/// host (WebApp/scripts/qlgx-runner.sh).
///
/// Vì sao không để API tự chạy: phục hồi toàn bộ CSDL đòi hỏi ngắt mọi kết nối tới chính CSDL
/// mà API đang dùng rồi đổi tên nó — ứng dụng không thể tự làm việc đó với chính mình. Ngoài
/// ra, giữ khoá R2 ngoài container API nghĩa là ứng dụng web bị chiếm quyền cũng không xoá
/// được bản sao lưu. API chỉ INSERT và SELECT trên bảng này, KHÔNG BAO GIỜ gọi lệnh hệ thống.
///
/// Cố ý KHÔNG có cột GiaoXuId và KHÔNG có bộ lọc toàn cục: mọi thao tác ở đây tác động tới toàn
/// máy chủ, chỉ tài khoản "Quản trị hệ thống" (LoaiTaiKhoan=9) gọi được — giống
/// NhapDuLieuJob/GiaoXu/GiaoPhan/GiaoHat.
///
/// Lưu ý về job "phuc_hoi": chính CSDL chứa bảng này bị thay thế ở bước hoán đổi tên. Bộ chạy
/// vì vậy ghi nhật ký song song ra /var/log/qlgx/ trên host và ghi lại kết quả cuối cùng vào
/// bảng job của CSDL MỚI sau khi hoán đổi xong — xem qlgx-restore.sh.
/// </summary>
public class CongViecSaoLuu
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Một trong <see cref="LoaiCongViecSaoLuu.HopLe"/>.</summary>
    public string Loai { get; set; } = "";

    /// <summary>"cho" | "dang_chay" | "xong" | "loi" — xem <see cref="TrangThaiCongViec"/>.</summary>
    public string TrangThai { get; set; } = TrangThaiCongViec.Cho;

    /// <summary>Tham số riêng theo loại, dạng JSON (ví dụ {"snapshotId":"ab12cd34"}).</summary>
    public string? ThamSoJson { get; set; }

    /// <summary>Nhật ký nối thêm theo từng bước — hiện nguyên văn cho quản trị viên xem.</summary>
    public string? NhatKy { get; set; }

    /// <summary>Bước đang chạy, để giao diện hiện tiến trình có nghĩa thay vì chỉ "đang chạy".</summary>
    public string? BuocHienTai { get; set; }

    /// <summary>Tài khoản đã bấm nút. Null với công việc do systemd timer tự tạo.</summary>
    public Guid? NguoiTaoId { get; set; }

    public DateTimeOffset TaoLuc { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? BatDauLuc { get; set; }
    public DateTimeOffset? KetThucLuc { get; set; }
}
```

`WebApp/src/Qlgx.Domain/Entities/BanSaoLuu.cs`:

```csharp
namespace Qlgx.Domain.Entities;

public static class NguonBanSao
{
    public const string TuDong = "tu_dong";
    public const string ThuCong = "thu_cong";
    public const string TruocCapNhat = "truoc_cap_nhat";
    public const string TruocPhucHoi = "truoc_phuc_hoi";
}

/// <summary>
/// Bảng ĐỆM danh sách bản sao lưu, do bộ chạy trên host cập nhật sau mỗi lượt sao lưu.
///
/// Vì sao cần bảng đệm thay vì để API gọi thẳng `restic snapshots --json`: gọi thẳng thì
/// container API phải có nhị phân restic VÀ khoá R2, phá vỡ toàn bộ ranh giới bảo mật (khoá chỉ
/// nằm ở /etc/qlgx/backup.env trên host, root đọc được). Thà chấp nhận danh sách trễ tối đa 6
/// giờ còn hơn nới quyền cho thành phần ít tin cậy hơn — nút "Tải lại" trên giao diện tạo công
/// việc "dong_bo_danh_sach" để cập nhật ngay khi cần.
///
/// SoGiaoDan/SoGiaDinh được bộ chạy đếm TẠI THỜI ĐIỂM sao lưu và ghi vào đây, để màn hình phục
/// hồi hiện được "quay về đây nghĩa là còn 2050 giáo dân" — thông tin quyết định trước một thao
/// tác không thể hoàn tác.
/// </summary>
public class BanSaoLuu
{
    /// <summary>Mã snapshot ngắn của restic (8 ký tự hex) — khoá chính, do restic sinh.</summary>
    public string Id { get; set; } = "";

    public DateTimeOffset ThoiDiem { get; set; }
    public string? Nhan { get; set; }
    public long KichThuocByte { get; set; }
    public int SoGiaoDan { get; set; }
    public int SoGiaDinh { get; set; }

    /// <summary>Xem <see cref="NguonBanSao"/>.</summary>
    public string Nguon { get; set; } = NguonBanSao.TuDong;
}
```

`WebApp/src/Qlgx.Domain/Entities/TrangThaiSaoLuu.cs`:

```csharp
namespace Qlgx.Domain.Entities;

/// <summary>
/// Đúng MỘT dòng (Id luôn = 1), do bộ chạy trên host cập nhật. Nguồn dữ liệu cho đèn trạng thái
/// xanh/vàng/đỏ ở đầu màn hình "Sao lưu &amp; Phục hồi".
///
/// Vì sao tách khỏi bảng ban_sao_luu: đèn còn phải phản ánh những thứ KHÔNG phải một bản sao —
/// lần diễn tập phục hồi gần nhất có đạt không, lỗi gần nhất là gì. Kiểu hỏng nguy hiểm nhất
/// của sao lưu là hỏng âm thầm sáu tháng rồi mới lộ ra đúng hôm cần dùng, nên hai thông tin đó
/// phải hiện thường trực chứ không nằm trong nhật ký.
/// </summary>
public class TrangThaiSaoLuu
{
    /// <summary>Luôn bằng 1 — ràng buộc CHECK ở migration bảo đảm bảng chỉ có một dòng.</summary>
    public int Id { get; set; } = 1;

    public DateTimeOffset? SaoLuuGanNhat { get; set; }
    public DateTimeOffset? DienTapGanNhat { get; set; }
    public bool DienTapDat { get; set; }

    /// <summary>Thông báo lỗi gần nhất của bất kỳ công việc nào — null nghĩa là đang khoẻ.</summary>
    public string? LoiGanNhat { get; set; }
}
```

- [ ] **Step 4: Đăng ký DbSet**

Trong `WebApp/src/Qlgx.Data/QlgxDbContext.cs`, ngay sau dòng `public DbSet<NhapDuLieuJob> NhapDuLieuJob => Set<NhapDuLieuJob>();`:

```csharp
    /// <summary>Hàng đợi công việc sao lưu/phục hồi (xem CongViecSaoLuu.cs) — không có bộ lọc
    /// GiaoXuId, cùng lý do với NhapDuLieuJob: chỉ đọc/ghi được qua policy "QuanTriHeThong".</summary>
    public DbSet<CongViecSaoLuu> CongViecSaoLuu => Set<CongViecSaoLuu>();

    /// <summary>Bảng đệm danh sách bản sao lưu do bộ chạy trên host cập nhật (xem BanSaoLuu.cs).</summary>
    public DbSet<BanSaoLuu> BanSaoLuu => Set<BanSaoLuu>();

    /// <summary>Đúng một dòng, nguồn cho đèn trạng thái sao lưu (xem TrangThaiSaoLuu.cs).</summary>
    public DbSet<TrangThaiSaoLuu> TrangThaiSaoLuu => Set<TrangThaiSaoLuu>();
```

- [ ] **Step 5: Sinh migration**

Run:
```bash
cd WebApp && dotnet ef migrations add ThemBangSaoLuu \
  --project src/Qlgx.Data --startup-project src/Qlgx.Api
```
Expected: sinh ba bảng `cong_viec_sao_luu`, `ban_sao_luu`, `trang_thai_sao_luu`.

- [ ] **Step 6: Bổ sung ràng buộc và chỉ mục vào migration vừa sinh**

Thêm vào cuối phương thức `Up` của tệp migration mới:

```csharp
            // Bang trang thai chi duoc phep co DUNG MOT dong — rang buoc o tang CSDL thay vi
            // dua vao ky luat cua ma goi, vi ca API lan script bash tren host deu ghi vao day.
            migrationBuilder.Sql(
                "ALTER TABLE trang_thai_sao_luu ADD CONSTRAINT ck_trang_thai_sao_luu_mot_dong CHECK (\"Id\" = 1);");

            // Bo chay lay cong viec bang: WHERE trang_thai='cho' ORDER BY tao_luc LIMIT 1
            // FOR UPDATE SKIP LOCKED — chi muc nay phuc vu dung truy van do.
            migrationBuilder.Sql(
                "CREATE INDEX ix_cong_viec_sao_luu_cho ON cong_viec_sao_luu (\"TaoLuc\") " +
                "WHERE \"TrangThai\" = 'cho';");

            // Dong trang thai duy nhat phai TON TAI ngay tu dau, de bo chay chi can UPDATE
            // (khong phai UPSERT) va giao dien luon doc duoc mot dong.
            migrationBuilder.Sql(
                "INSERT INTO trang_thai_sao_luu (\"Id\", \"DienTapDat\") VALUES (1, false) " +
                "ON CONFLICT DO NOTHING;");
```

Và vào đầu phương thức `Down`:

```csharp
            migrationBuilder.Sql("DROP INDEX IF EXISTS ix_cong_viec_sao_luu_cho;");
```

**Lưu ý:** tên cột trong SQL thô phải khớp đúng cách `DatTenSnakeCase` đã đổi tên. Sau khi sinh migration, mở tệp `*.Designer.cs` hoặc chạy `dotnet ef migrations script` để đọc tên cột THẬT rồi sửa lại SQL trên cho khớp — đừng đoán.

- [ ] **Step 7: Chạy test để xác nhận nó đạt**

Run: `dotnet test WebApp/Qlgx.sln --filter BangSaoLuuTests`
Expected: PASS — 3/3.

- [ ] **Step 8: Chạy toàn bộ bộ test backend**

Run: `dotnet test WebApp/Qlgx.sln`
Expected: PASS toàn bộ (đặc biệt `LocTheoGiaoXuTests` — ba bảng mới không có cột `GiaoXuId` nên không cần bộ lọc).

- [ ] **Step 9: Commit**

```bash
git add WebApp/src/Qlgx.Domain/Entities/CongViecSaoLuu.cs \
        WebApp/src/Qlgx.Domain/Entities/BanSaoLuu.cs \
        WebApp/src/Qlgx.Domain/Entities/TrangThaiSaoLuu.cs \
        WebApp/src/Qlgx.Data/QlgxDbContext.cs WebApp/src/Qlgx.Data/Migrations/ \
        WebApp/tests/Qlgx.Api.Tests/BangSaoLuuTests.cs
git commit -m "Them ba bang nen cho sao luu: cong viec, ban sao, trang thai"
```

---

## Task 6: Dịch vụ và API sao lưu

> **Chỉnh so với spec:** spec mục 7.2 viết chuỗi xác nhận là `PHUC HOI <tên giáo xứ>`, nhưng phạm vi phục hồi đã chốt là **toàn máy chủ** (mọi giáo xứ), nên gắn tên một giáo xứ vào đó là mâu thuẫn và gây hiểu nhầm rằng chỉ giáo xứ đó bị ảnh hưởng. Chuỗi thực tế dùng: **`PHUC HOI TOAN BO`**. Spec đã được sửa cho khớp.

**Files:**
- Create: `WebApp/src/Qlgx.Api/Dtos/SaoLuuDtos.cs`
- Create: `WebApp/src/Qlgx.Api/Services/SaoLuuService.cs`
- Create: `WebApp/src/Qlgx.Api/Endpoints/SaoLuuEndpoints.cs`
- Modify: `WebApp/src/Qlgx.Api/Program.cs` (đăng ký `SaoLuuService`, gọi `app.MapSaoLuu()`)
- Test: `WebApp/tests/Qlgx.Api.Tests/SaoLuuApiTests.cs`

**Interfaces:**
- Consumes: `CongViecSaoLuu`, `BanSaoLuu`, `TrangThaiSaoLuu`, `LoaiCongViecSaoLuu`, `TrangThaiCongViec` (Task 5); `QlgxDbContext`; policy `"QuanTriHeThong"`.
- Produces:
  - `SaoLuuService.LayTinhTrang(CancellationToken) → Task<TinhTrangSaoLuuDto>`
  - `SaoLuuService.LayDanhSachBanSao(CancellationToken) → Task<IReadOnlyList<BanSaoLuuDto>>`
  - `SaoLuuService.TaoCongViec(TaoCongViecYeuCau, Guid?, CancellationToken) → Task<(Guid? id, string? loi)>`
  - `SaoLuuService.LayCongViec(Guid, CancellationToken) → Task<CongViecDto?>`
  - `SaoLuuService.LayCongViecGanDay(int, CancellationToken) → Task<IReadOnlyList<CongViecDto>>`
  - Hằng `SaoLuuService.ChuoiXacNhanPhucHoi = "PHUC HOI TOAN BO"`, `SaoLuuService.GioCanhBao = 8`
  - Các route `GET /api/sao-luu/tinh-trang`, `GET /api/sao-luu/danh-sach`, `POST /api/sao-luu/cong-viec`, `GET /api/sao-luu/cong-viec`, `GET /api/sao-luu/cong-viec/{id}`

- [ ] **Step 1: Viết test thất bại**

Tạo `WebApp/tests/Qlgx.Api.Tests/SaoLuuApiTests.cs`:

```csharp
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Qlgx.Domain.Entities;

namespace Qlgx.Api.Tests;

public class SaoLuuApiTests(QlgxApiFactory factory) : IClassFixture<QlgxApiFactory>
{
    private HttpClient ClientHeThong() => factory.CreateAuthClient(loaiTaiKhoan: 9);

    [Fact]
    public async Task Tai_khoan_thuong_bi_tu_choi_moi_route_sao_luu()
    {
        var client = factory.CreateAuthClient(loaiTaiKhoan: 0);

        foreach (var duongDan in new[] { "/api/sao-luu/tinh-trang", "/api/sao-luu/danh-sach", "/api/sao-luu/cong-viec" })
            (await client.GetAsync(duongDan)).StatusCode.Should().Be(HttpStatusCode.Forbidden, duongDan);

        var res = await client.PostAsJsonAsync("/api/sao-luu/cong-viec", new { loai = "sao_luu" });
        res.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Tao_cong_viec_sao_luu_chi_ghi_mot_dong_trang_thai_cho()
    {
        var res = await ClientHeThong().PostAsJsonAsync("/api/sao-luu/cong-viec", new { loai = "sao_luu" });

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        var than = await res.Content.ReadFromJsonAsync<TaoCongViecKetQua>();
        await using var db = factory.TaoContextThuan();
        var cv = await db.CongViecSaoLuu.SingleAsync(x => x.Id == than!.Id);
        cv.TrangThai.Should().Be("cho");
        cv.BatDauLuc.Should().BeNull("API chi tao cong viec, bo chay tren host moi thuc thi");
    }

    [Fact]
    public async Task Loai_cong_viec_khong_hop_le_bi_tu_choi()
    {
        var res = await ClientHeThong().PostAsJsonAsync("/api/sao-luu/cong-viec", new { loai = "xoa_sach" });

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Phuc_hoi_thieu_chuoi_xac_nhan_bi_tu_choi()
    {
        var res = await ClientHeThong().PostAsJsonAsync("/api/sao-luu/cong-viec",
            new { loai = "phuc_hoi", snapshotId = "ab12cd34" });

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await res.Content.ReadAsStringAsync()).Should().Contain("PHUC HOI TOAN BO");
    }

    [Fact]
    public async Task Phuc_hoi_sai_chuoi_xac_nhan_bi_tu_choi()
    {
        var res = await ClientHeThong().PostAsJsonAsync("/api/sao-luu/cong-viec",
            new { loai = "phuc_hoi", snapshotId = "ab12cd34", xacNhan = "phuc hoi toan bo" });

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Phuc_hoi_thieu_snapshot_id_bi_tu_choi()
    {
        var res = await ClientHeThong().PostAsJsonAsync("/api/sao-luu/cong-viec",
            new { loai = "phuc_hoi", xacNhan = "PHUC HOI TOAN BO" });

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Phuc_hoi_du_dieu_kien_thi_tao_duoc_cong_viec()
    {
        var res = await ClientHeThong().PostAsJsonAsync("/api/sao-luu/cong-viec",
            new { loai = "phuc_hoi", snapshotId = "ab12cd34", xacNhan = "PHUC HOI TOAN BO" });

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        var than = await res.Content.ReadFromJsonAsync<TaoCongViecKetQua>();
        await using var db = factory.TaoContextThuan();
        var cv = await db.CongViecSaoLuu.SingleAsync(x => x.Id == than!.Id);
        cv.ThamSoJson.Should().Contain("ab12cd34");
    }

    [Fact]
    public async Task Den_do_khi_co_loi_gan_nhat()
    {
        await using (var db = factory.TaoContextThuan())
        {
            var tt = await db.TrangThaiSaoLuu.SingleAsync();
            tt.SaoLuuGanNhat = DateTimeOffset.UtcNow;
            tt.LoiGanNhat = "restic check that bai";
            await db.SaveChangesAsync();
        }

        var than = await ClientHeThong().GetFromJsonAsync<TinhTrangKetQua>("/api/sao-luu/tinh-trang");

        than!.Den.Should().Be("do");
    }

    [Fact]
    public async Task Den_vang_khi_qua_8_gio_khong_co_ban_sao_moi()
    {
        await using (var db = factory.TaoContextThuan())
        {
            var tt = await db.TrangThaiSaoLuu.SingleAsync();
            tt.SaoLuuGanNhat = DateTimeOffset.UtcNow.AddHours(-9);
            tt.LoiGanNhat = null;
            await db.SaveChangesAsync();
        }

        var than = await ClientHeThong().GetFromJsonAsync<TinhTrangKetQua>("/api/sao-luu/tinh-trang");

        than!.Den.Should().Be("vang");
    }

    [Fact]
    public async Task Den_xanh_khi_vua_sao_luu_xong_va_khong_loi()
    {
        await using (var db = factory.TaoContextThuan())
        {
            var tt = await db.TrangThaiSaoLuu.SingleAsync();
            tt.SaoLuuGanNhat = DateTimeOffset.UtcNow.AddHours(-1);
            tt.LoiGanNhat = null;
            tt.DienTapGanNhat = DateTimeOffset.UtcNow.AddDays(-2);
            tt.DienTapDat = true;
            await db.SaveChangesAsync();
        }

        var than = await ClientHeThong().GetFromJsonAsync<TinhTrangKetQua>("/api/sao-luu/tinh-trang");

        than!.Den.Should().Be("xanh");
    }

    [Fact]
    public async Task Chua_bao_gio_sao_luu_thi_den_do()
    {
        await using (var db = factory.TaoContextThuan())
        {
            var tt = await db.TrangThaiSaoLuu.SingleAsync();
            tt.SaoLuuGanNhat = null;
            tt.LoiGanNhat = null;
            await db.SaveChangesAsync();
        }

        var than = await ClientHeThong().GetFromJsonAsync<TinhTrangKetQua>("/api/sao-luu/tinh-trang");

        than!.Den.Should().Be("do");
    }

    private sealed record TaoCongViecKetQua(Guid Id);
    private sealed record TinhTrangKetQua(string Den, DateTimeOffset? SaoLuuGanNhat, int SoBanSao,
        DateTimeOffset? DienTapGanNhat, bool DienTapDat, string? LoiGanNhat);
}
```

- [ ] **Step 2: Chạy test để chắc chắn nó thất bại**

Run: `dotnet test WebApp/Qlgx.sln --filter SaoLuuApiTests`
Expected: FAIL — mọi route trả 404.

- [ ] **Step 3: Viết DTO**

`WebApp/src/Qlgx.Api/Dtos/SaoLuuDtos.cs`:

```csharp
namespace Qlgx.Api.Dtos;

/// <summary>Thân yêu cầu POST /api/sao-luu/cong-viec.</summary>
/// <param name="Loai">Một trong LoaiCongViecSaoLuu.HopLe.</param>
/// <param name="SnapshotId">Bắt buộc với "phuc_hoi" và "tai_ve".</param>
/// <param name="XacNhan">Bắt buộc với "phuc_hoi": phải đúng nguyên văn "PHUC HOI TOAN BO".</param>
/// <param name="Nhan">Nhãn tuỳ chọn cho bản sao thủ công.</param>
public sealed record TaoCongViecYeuCau(string Loai, string? SnapshotId, string? XacNhan, string? Nhan);

public sealed record CongViecDto(
    Guid Id, string Loai, string TrangThai, string? BuocHienTai, string? NhatKy,
    DateTimeOffset TaoLuc, DateTimeOffset? BatDauLuc, DateTimeOffset? KetThucLuc);

public sealed record BanSaoLuuDto(
    string Id, DateTimeOffset ThoiDiem, string? Nhan, long KichThuocByte,
    int SoGiaoDan, int SoGiaDinh, string Nguon);

/// <param name="Den">"xanh" | "vang" | "do" — xem SaoLuuService.TinhDen.</param>
public sealed record TinhTrangSaoLuuDto(
    string Den, DateTimeOffset? SaoLuuGanNhat, int SoBanSao,
    DateTimeOffset? DienTapGanNhat, bool DienTapDat, string? LoiGanNhat);
```

- [ ] **Step 4: Viết dịch vụ**

`WebApp/src/Qlgx.Api/Services/SaoLuuService.cs`:

```csharp
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Qlgx.Api.Dtos;
using Qlgx.Data;
using Qlgx.Domain.Entities;

namespace Qlgx.Api.Services;

/// <summary>
/// Nghiệp vụ màn hình "Sao lưu &amp; Phục hồi". CỐ Ý rất mỏng: dịch vụ này KHÔNG gọi restic,
/// KHÔNG chạy pg_dump, KHÔNG đụng tới hệ thống tệp. Nó chỉ ghi một dòng vào hàng đợi công việc
/// và đọc lại trạng thái mà bộ chạy trên host ghi về (xem CongViecSaoLuu.cs để biết vì sao ranh
/// giới này là bắt buộc chứ không phải lựa chọn phong cách).
/// </summary>
public class SaoLuuService(QlgxDbContext db)
{
    /// <summary>Chuỗi quản trị viên phải GÕ TAY để xác nhận phục hồi. Cố ý viết hoa không dấu và
    /// nói rõ "toàn bộ": phạm vi phục hồi là TOÀN MÁY CHỦ, không riêng giáo xứ nào. Không dùng
    /// hộp thoại "bạn có chắc không?" — người dùng bấm OK theo phản xạ.</summary>
    public const string ChuoiXacNhanPhucHoi = "PHUC HOI TOAN BO";

    /// <summary>Quá bao nhiêu giờ không có bản sao mới thì chuyển đèn vàng. Nhịp sao lưu là 6
    /// giờ nên 8 giờ cho phép trễ một chút mà chưa báo động giả.</summary>
    public const int GioCanhBao = 8;

    public async Task<TinhTrangSaoLuuDto> LayTinhTrang(CancellationToken ct)
    {
        var tt = await db.TrangThaiSaoLuu.AsNoTracking().FirstOrDefaultAsync(ct)
                 ?? new TrangThaiSaoLuu();
        var soBanSao = await db.BanSaoLuu.CountAsync(ct);
        return new TinhTrangSaoLuuDto(
            TinhDen(tt, DateTimeOffset.UtcNow), tt.SaoLuuGanNhat, soBanSao,
            tt.DienTapGanNhat, tt.DienTapDat, tt.LoiGanNhat);
    }

    /// <summary>
    /// Đỏ: có lỗi gần nhất chưa được xoá, HOẶC chưa từng sao lưu lần nào, HOẶC diễn tập phục hồi
    /// gần nhất thất bại. Vàng: đã lâu hơn <see cref="GioCanhBao"/> giờ không có bản sao mới.
    /// Ngược lại xanh. Tách thành hàm thuần để test được không cần CSDL.
    /// </summary>
    public static string TinhDen(TrangThaiSaoLuu tt, DateTimeOffset bayGio)
    {
        if (tt.LoiGanNhat is not null) return "do";
        if (tt.SaoLuuGanNhat is null) return "do";
        if (tt.DienTapGanNhat is not null && !tt.DienTapDat) return "do";
        if (bayGio - tt.SaoLuuGanNhat.Value > TimeSpan.FromHours(GioCanhBao)) return "vang";
        return "xanh";
    }

    public async Task<IReadOnlyList<BanSaoLuuDto>> LayDanhSachBanSao(CancellationToken ct) =>
        await db.BanSaoLuu.AsNoTracking()
            .OrderByDescending(x => x.ThoiDiem)
            .Select(x => new BanSaoLuuDto(x.Id, x.ThoiDiem, x.Nhan, x.KichThuocByte,
                x.SoGiaoDan, x.SoGiaDinh, x.Nguon))
            .ToListAsync(ct);

    public async Task<(Guid? id, string? loi)> TaoCongViec(
        TaoCongViecYeuCau yc, Guid? nguoiTaoId, CancellationToken ct)
    {
        if (!LoaiCongViecSaoLuu.HopLe.Contains(yc.Loai))
            return (null, $"Loại công việc không hợp lệ: '{yc.Loai}'.");

        if (yc.Loai is LoaiCongViecSaoLuu.PhucHoi or LoaiCongViecSaoLuu.TaiVe
            && string.IsNullOrWhiteSpace(yc.SnapshotId))
            return (null, "Chưa chọn bản sao lưu.");

        // Kiem lai chuoi xac nhan O MAY CHU — khong tin rang giao dien da hoi. Ai cung co the
        // goi thang API bang curl.
        if (yc.Loai == LoaiCongViecSaoLuu.PhucHoi && yc.XacNhan != ChuoiXacNhanPhucHoi)
            return (null, $"Phải gõ đúng chuỗi xác nhận \"{ChuoiXacNhanPhucHoi}\" để phục hồi dữ liệu.");

        // Khong xep hang hai cong viec ghi cung luc — hai lan phuc hoi chong nhau la tham hoa.
        var dangCo = await db.CongViecSaoLuu.AnyAsync(
            x => x.TrangThai == TrangThaiCongViec.Cho || x.TrangThai == TrangThaiCongViec.DangChay, ct);
        if (dangCo)
            return (null, "Đang có một công việc sao lưu/phục hồi chạy dở. Chờ xong rồi thử lại.");

        var cv = new CongViecSaoLuu
        {
            Loai = yc.Loai,
            NguoiTaoId = nguoiTaoId,
            ThamSoJson = JsonSerializer.Serialize(new { snapshotId = yc.SnapshotId, nhan = yc.Nhan }),
        };
        db.CongViecSaoLuu.Add(cv);
        await db.SaveChangesAsync(ct);
        return (cv.Id, null);
    }

    public async Task<CongViecDto?> LayCongViec(Guid id, CancellationToken ct) =>
        await db.CongViecSaoLuu.AsNoTracking().Where(x => x.Id == id).Select(Chieu).FirstOrDefaultAsync(ct);

    public async Task<IReadOnlyList<CongViecDto>> LayCongViecGanDay(int soLuong, CancellationToken ct) =>
        await db.CongViecSaoLuu.AsNoTracking()
            .OrderByDescending(x => x.TaoLuc).Take(soLuong).Select(Chieu).ToListAsync(ct);

    private static readonly System.Linq.Expressions.Expression<Func<CongViecSaoLuu, CongViecDto>> Chieu =
        x => new CongViecDto(x.Id, x.Loai, x.TrangThai, x.BuocHienTai, x.NhatKy,
                             x.TaoLuc, x.BatDauLuc, x.KetThucLuc);
}
```

- [ ] **Step 5: Viết endpoint**

`WebApp/src/Qlgx.Api/Endpoints/SaoLuuEndpoints.cs`:

```csharp
using System.Security.Claims;
using Qlgx.Api.Dtos;
using Qlgx.Api.Services;

namespace Qlgx.Api.Endpoints;

/// <summary>
/// Màn hình "Sao lưu &amp; Phục hồi" (xem docs/superpowers/specs/2026-09-13-qlgx-trien-khai-sao-luu-design.md
/// mục 8). CẢ NHÓM đòi policy "QuanTriHeThong": mọi thao tác ở đây tác động tới TOÀN MÁY CHỦ,
/// không riêng giáo xứ nào, nên quản trị viên của một giáo xứ không được phép chạm tới.
/// </summary>
public static class SaoLuuEndpoints
{
    public static void MapSaoLuu(this IEndpointRouteBuilder app)
    {
        var nhom = app.MapGroup("/api/sao-luu").RequireAuthorization("QuanTriHeThong");

        nhom.MapGet("/tinh-trang", async (SaoLuuService dv, CancellationToken ct) =>
            Results.Ok(await dv.LayTinhTrang(ct)));

        nhom.MapGet("/danh-sach", async (SaoLuuService dv, CancellationToken ct) =>
            Results.Ok(await dv.LayDanhSachBanSao(ct)));

        nhom.MapPost("/cong-viec", async (SaoLuuService dv, TaoCongViecYeuCau yc,
            ClaimsPrincipal nguoiDung, CancellationToken ct) =>
        {
            var (id, loi) = await dv.TaoCongViec(yc, DocIdNguoiDung(nguoiDung), ct);
            return loi is not null
                ? Results.BadRequest(new { thongBao = loi })
                : Results.Ok(new { id });
        });

        nhom.MapGet("/cong-viec", async (SaoLuuService dv, CancellationToken ct) =>
            Results.Ok(await dv.LayCongViecGanDay(20, ct)));

        nhom.MapGet("/cong-viec/{id:guid}", async (SaoLuuService dv, Guid id, CancellationToken ct) =>
            await dv.LayCongViec(id, ct) is { } cv ? Results.Ok(cv) : Results.NotFound());
    }

    private static Guid? DocIdNguoiDung(ClaimsPrincipal nguoiDung) =>
        Guid.TryParse(nguoiDung.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;
}
```

**Lưu ý:** kiểm tên claim thật mà `TokenService.PhatHanh` phát ra (mở `TokenService.cs`). Nếu id tài khoản nằm ở claim khác `ClaimTypes.NameIdentifier`, dùng đúng tên đó — đừng để `NguoiTaoId` luôn null.

- [ ] **Step 6: Đăng ký vào Program.cs**

Thêm cạnh các `AddScoped` khác (sau dòng `builder.Services.AddScoped<GiaoHoService>();`):

```csharp
// Man hinh "Sao luu & Phuc hoi" — chi ghi/doc hang doi cong viec, khong tu chay lenh he thong.
builder.Services.AddScoped<SaoLuuService>();
```

Và thêm vào danh sách `app.Map*()`, sau `app.MapNhapDuLieu();`:

```csharp
app.MapSaoLuu();
```

- [ ] **Step 7: Chạy test để xác nhận nó đạt**

Run: `dotnet test WebApp/Qlgx.sln --filter SaoLuuApiTests`
Expected: PASS — 11/11.

- [ ] **Step 8: Chạy toàn bộ bộ test backend**

Run: `dotnet test WebApp/Qlgx.sln`
Expected: PASS toàn bộ.

- [ ] **Step 9: Commit**

```bash
git add WebApp/src/Qlgx.Api/Dtos/SaoLuuDtos.cs \
        WebApp/src/Qlgx.Api/Services/SaoLuuService.cs \
        WebApp/src/Qlgx.Api/Endpoints/SaoLuuEndpoints.cs \
        WebApp/src/Qlgx.Api/Program.cs \
        WebApp/tests/Qlgx.Api.Tests/SaoLuuApiTests.cs
git commit -m "Them dich vu va API sao luu: hang doi cong viec, tinh trang, danh sach ban sao"
```

---

## Task 7: Tải bản sao lưu về máy qua thư mục spool

Container API không có khoá R2, nên không tự lấy được bản sao từ kho. Luồng đi vòng: runner trên host giải nén ra `/var/lib/qlgx/spool/<mã job>/`, thư mục đó mount **read-only** vào container, API chỉ stream tệp.

**Files:**
- Modify: `WebApp/src/Qlgx.Api/Services/SaoLuuService.cs` (thêm `LayDuongDanTaiVe`)
- Modify: `WebApp/src/Qlgx.Api/Endpoints/SaoLuuEndpoints.cs` (thêm route)
- Test: `WebApp/tests/Qlgx.Api.Tests/SaoLuuTaiVeTests.cs`

**Interfaces:**
- Consumes: cấu hình `Qlgx:ThuMucSpool` (mặc định `/var/lib/qlgx/spool`).
- Produces: `SaoLuuService.LayDuongDanTaiVe(Guid maCongViec, CancellationToken) → Task<(string? duongDan, string? loi)>`; route `GET /api/sao-luu/tai-ve/{maCongViec:guid}` trả `application/octet-stream` với `Content-Disposition: attachment`.

- [ ] **Step 1: Viết test thất bại**

Tạo `WebApp/tests/Qlgx.Api.Tests/SaoLuuTaiVeTests.cs`:

```csharp
using System.Net;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Qlgx.Domain.Entities;

namespace Qlgx.Api.Tests;

public class SaoLuuTaiVeTests(QlgxApiFactory factory) : IClassFixture<QlgxApiFactory>
{
    [Fact]
    public async Task Cong_viec_chua_xong_thi_khong_tai_duoc()
    {
        var id = await TaoCongViecTaiVe(factory, TrangThaiCongViec.DangChay, taoTep: false);

        var res = await factory.CreateAuthClient(loaiTaiKhoan: 9).GetAsync($"/api/sao-luu/tai-ve/{id}");

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Cong_viec_xong_nhung_tep_da_bi_don_thi_bao_loi_ro_rang()
    {
        var id = await TaoCongViecTaiVe(factory, TrangThaiCongViec.Xong, taoTep: false);

        var res = await factory.CreateAuthClient(loaiTaiKhoan: 9).GetAsync($"/api/sao-luu/tai-ve/{id}");

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await res.Content.ReadAsStringAsync()).Should().Contain("24 giờ");
    }

    [Fact]
    public async Task Tai_khoan_thuong_bi_tu_choi()
    {
        var id = await TaoCongViecTaiVe(factory, TrangThaiCongViec.Xong, taoTep: false);

        var res = await factory.CreateAuthClient(loaiTaiKhoan: 0).GetAsync($"/api/sao-luu/tai-ve/{id}");

        res.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Cong_viec_khong_phai_loai_tai_ve_thi_bi_tu_choi()
    {
        await using var db = factory.TaoContextThuan();
        var cv = new CongViecSaoLuu { Loai = LoaiCongViecSaoLuu.SaoLuu, TrangThai = TrangThaiCongViec.Xong };
        db.CongViecSaoLuu.Add(cv);
        await db.SaveChangesAsync();

        var res = await factory.CreateAuthClient(loaiTaiKhoan: 9).GetAsync($"/api/sao-luu/tai-ve/{cv.Id}");

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private static async Task<Guid> TaoCongViecTaiVe(QlgxApiFactory f, string trangThai, bool taoTep)
    {
        await using var db = f.TaoContextThuan();
        var cv = new CongViecSaoLuu { Loai = LoaiCongViecSaoLuu.TaiVe, TrangThai = trangThai };
        db.CongViecSaoLuu.Add(cv);
        await db.SaveChangesAsync();
        return cv.Id;
    }
}
```

- [ ] **Step 2: Chạy test để chắc chắn nó thất bại**

Run: `dotnet test WebApp/Qlgx.sln --filter SaoLuuTaiVeTests`
Expected: FAIL — route trả 404 ở mọi trường hợp.

- [ ] **Step 3: Thêm phương thức vào SaoLuuService**

Sửa khai báo lớp thành `public class SaoLuuService(QlgxDbContext db, IConfiguration cauHinh)` và thêm:

```csharp
    /// <summary>Thư mục spool — bộ chạy trên host GHI vào đây, container API mount READ-ONLY.
    /// Đây là kênh duy nhất để một tệp từ kho sao lưu đến được trình duyệt quản trị viên mà
    /// không cần cho container API biết khoá R2.</summary>
    private string ThuMucSpool => cauHinh["Qlgx:ThuMucSpool"] ?? "/var/lib/qlgx/spool";

    public async Task<(string? duongDan, string? loi)> LayDuongDanTaiVe(
        Guid maCongViec, CancellationToken ct)
    {
        var cv = await db.CongViecSaoLuu.AsNoTracking().FirstOrDefaultAsync(x => x.Id == maCongViec, ct);
        if (cv is null) return (null, null); // 404 tron, khong tiet lo gi them

        if (cv.Loai != LoaiCongViecSaoLuu.TaiVe)
            return (null, "Công việc này không phải là lượt chuẩn bị tệp tải về.");

        if (cv.TrangThai != TrangThaiCongViec.Xong)
            return (null, "Tệp chưa chuẩn bị xong. Chờ công việc chạy xong rồi tải lại trang.");

        var thuMuc = Path.Combine(ThuMucSpool, maCongViec.ToString());
        var tep = Directory.Exists(thuMuc)
            ? Directory.EnumerateFiles(thuMuc, "*.dump.tar.gz").FirstOrDefault()
            : null;
        if (tep is null)
            return (null, "Tệp tải về đã bị dọn (tệp trong spool chỉ giữ 24 giờ). " +
                          "Hãy tạo lại một lượt tải về mới.");

        return (tep, null);
    }
```

- [ ] **Step 4: Thêm route**

Vào `SaoLuuEndpoints.MapSaoLuu`, sau route `/cong-viec/{id:guid}`:

```csharp
        // Stream tep tu spool. API KHONG biet khoa R2 va KHONG goi restic — bo chay tren host
        // da giai nen san vao spool, o day chi con viec doc mot tep tren dia.
        //
        // Canh bao nghiep vu: tep nay la ban dump CHUA MA HOA chua toan bo du lieu giao dan.
        // Giao dien phai noi ro dieu do ngay tai nut tai (xem SaoLuuPage.tsx).
        nhom.MapGet("/tai-ve/{maCongViec:guid}", async (SaoLuuService dv, Guid maCongViec,
            CancellationToken ct) =>
        {
            var (duongDan, loi) = await dv.LayDuongDanTaiVe(maCongViec, ct);
            if (loi is not null)
                return loi.Contains("đã bị dọn")
                    ? Results.NotFound(new { thongBao = loi })
                    : Results.BadRequest(new { thongBao = loi });
            if (duongDan is null) return Results.NotFound();

            return Results.File(duongDan, "application/octet-stream",
                fileDownloadName: Path.GetFileName(duongDan));
        });
```

- [ ] **Step 5: Chạy test để xác nhận nó đạt**

Run: `dotnet test WebApp/Qlgx.sln --filter SaoLuuTaiVeTests`
Expected: PASS — 4/4.

- [ ] **Step 6: Khai báo mount spool trong compose**

Thêm vào dịch vụ `api` trong `WebApp/docker-compose.yml`, sau khối `environment`:

```yaml
    volumes:
      # Bo chay tren host GHI vao thu muc nay; container API chi DOC (:ro). Day la kenh duy nhat
      # de mot tep tu kho sao luu den duoc trinh duyet quan tri vien ma khong phai cho container
      # API biet khoa R2 — xem thiet ke muc 8.4.
      - ${QLGX_THU_MUC_SPOOL:-/var/lib/qlgx/spool}:/var/lib/qlgx/spool:ro
```

- [ ] **Step 7: Commit**

```bash
git add WebApp/src/Qlgx.Api/Services/SaoLuuService.cs \
        WebApp/src/Qlgx.Api/Endpoints/SaoLuuEndpoints.cs \
        WebApp/docker-compose.yml \
        WebApp/tests/Qlgx.Api.Tests/SaoLuuTaiVeTests.cs
git commit -m "Them duong tai ban sao luu ve may qua thu muc spool mount read-only"
```

---

# Giai đoạn C — Script trên máy chủ

## Task 8: Thư viện dùng chung và bộ kiểm thử shell

Trước khi viết `install.sh`, dựng nền: một thư viện hàm thuần test được, và một bộ chạy test. Không làm bước này thì `install.sh` sẽ thành một khối 2000 dòng không ai kiểm chứng được từng phần.

**Files:**
- Create: `WebApp/scripts/chung.sh`
- Create: `WebApp/scripts/os_adapter.sh`
- Create: `WebApp/tests/shell/chung.bats`
- Create: `WebApp/tests/shell/os_adapter.bats`

**Interfaces:**
- Produces (dùng lại ở mọi task shell sau):
  - `ghi_log <mức> <thông điệp>` — mức: `thong-tin` | `canh-bao` | `loi`
  - `bao_loi_va_thoat <thông điệp>` — ghi log lỗi rồi `exit 1`
  - `sinh_bi_mat [số byte]` — in một chuỗi base64 ngẫu nhiên (mặc định 32 byte)
  - `set_env_kv <tệp> <khoá> <giá trị>` — đặt/ghi đè một khoá, giữ nguyên phần còn lại
  - `doc_env_kv <tệp> <khoá>` — in giá trị, chuỗi rỗng nếu không có
  - `dat_neu_chua_co <tệp> <khoá> <giá trị>` — chỉ ghi khi khoá chưa có/rỗng (nền tảng của tính idempotent)
  - `os_ho()` → `debian` | `rhel`; `os_cai_goi <gói...>`; `os_co_lenh <lệnh>`

- [ ] **Step 1: Viết test thất bại cho `chung.sh`**

Tạo `WebApp/tests/shell/chung.bats`:

```bash
#!/usr/bin/env bats

setup() {
  load_path="${BATS_TEST_DIRNAME}/../../scripts/chung.sh"
  # shellcheck disable=SC1090
  source "$load_path"
  TEP="$BATS_TEST_TMPDIR/thu.env"
}

@test "set_env_kv them khoa moi vao tep rong" {
  : > "$TEP"
  set_env_kv "$TEP" "A" "1"
  run doc_env_kv "$TEP" "A"
  [ "$output" = "1" ]
}

@test "set_env_kv ghi de khoa da co, khong nhan doi dong" {
  printf 'A=1\nB=2\n' > "$TEP"
  set_env_kv "$TEP" "A" "9"
  [ "$(doc_env_kv "$TEP" A)" = "9" ]
  [ "$(doc_env_kv "$TEP" B)" = "2" ]
  [ "$(grep -c '^A=' "$TEP")" -eq 1 ]
}

@test "set_env_kv xu ly dung tep KHONG ket thuc bang xuong dong" {
  # Day chinh la loi da lam mat hai bi mat o mot du an khac: khoa moi bi noi duoi khoa cu
  # thanh mot dong hong.
  printf 'A=1' > "$TEP"
  set_env_kv "$TEP" "B" "2"
  [ "$(doc_env_kv "$TEP" A)" = "1" ]
  [ "$(doc_env_kv "$TEP" B)" = "2" ]
}

@test "set_env_kv khong lam hong gia tri co ky tu dac biet" {
  : > "$TEP"
  set_env_kv "$TEP" "P" 'a/b&c$d|e'
  [ "$(doc_env_kv "$TEP" P)" = 'a/b&c$d|e' ]
}

@test "doc_env_kv khong nham khoa co tien to giong nhau" {
  printf 'AB=2\nA=1\n' > "$TEP"
  [ "$(doc_env_kv "$TEP" A)" = "1" ]
  [ "$(doc_env_kv "$TEP" AB)" = "2" ]
}

@test "dat_neu_chua_co GIU NGUYEN gia tri da co" {
  printf 'K=bimat-cu\n' > "$TEP"
  dat_neu_chua_co "$TEP" "K" "bimat-moi"
  [ "$(doc_env_kv "$TEP" K)" = "bimat-cu" ]
}

@test "dat_neu_chua_co ghi khi khoa rong" {
  printf 'K=\n' > "$TEP"
  dat_neu_chua_co "$TEP" "K" "gia-tri-moi"
  [ "$(doc_env_kv "$TEP" K)" = "gia-tri-moi" ]
}

@test "sinh_bi_mat tra ve chuoi khac nhau moi lan va du dai" {
  a=$(sinh_bi_mat 32); b=$(sinh_bi_mat 32)
  [ "$a" != "$b" ]
  [ "${#a}" -ge 40 ]
}

@test "bao_loi_va_thoat thoat voi ma khac 0" {
  run bao_loi_va_thoat "co loi"
  [ "$status" -ne 0 ]
  [[ "$output" == *"co loi"* ]]
}
```

- [ ] **Step 2: Chạy test để chắc chắn nó thất bại**

Run: `docker run --rm -v "$PWD:/code" bats/bats:1.11.0 /code/WebApp/tests/shell/chung.bats`
Expected: FAIL — không tìm thấy `scripts/chung.sh`.

- [ ] **Step 3: Viết `chung.sh`**

```bash
#!/usr/bin/env bash
# Thu vien ham dung chung cho moi script van hanh QLGX. Chi dinh nghia ham, KHONG chay gi khi
# duoc source — de bo test bats nap vao va goi tung ham rieng le.

ghi_log() {
  local muc="$1"; shift
  local dau
  case "$muc" in
    thong-tin) dau="[ ]" ;;
    canh-bao)  dau="[!]" ;;
    loi)       dau="[X]" ;;
    *)         dau="[ ]" ;;
  esac
  printf '%s %s %s\n' "$(date '+%H:%M:%S')" "$dau" "$*" >&2
}

bao_loi_va_thoat() { ghi_log loi "$*"; exit 1; }

# Sinh mot bi mat ngau nhien dang base64. Doc thang tu /dev/urandom thay vi goi openssl de
# chay duoc tren may chua cai openssl (buoc dau cua install.sh, truoc khi cai phu thuoc).
sinh_bi_mat() {
  local so_byte="${1:-32}"
  head -c "$so_byte" /dev/urandom | base64 | tr -d '\n'
}

doc_env_kv() {
  local tep="$1" khoa="$2"
  [ -f "$tep" ] || { printf ''; return 0; }
  # Cat dung mot lan o dau '=' dau tien — gia tri co the chua dau '=' (base64 co the ket thuc
  # bang '='). Dung khop chinh xac tu dau dong de khong nham khoa co tien to giong nhau.
  local dong
  dong=$(grep -m1 "^${khoa}=" "$tep" || true)
  printf '%s' "${dong#*=}"
}

# Dat mot khoa trong tep .env. CO Y khong dung sed -i voi gia tri nguoi dung: gia tri co the
# chua '/', '&', '|' — moi ky tu do deu co nghia dac biet trong sed va se lam hong mat khau.
# Thay vao do doc-loc-ghi bang awk voi gia tri truyen qua bien, khong qua mau thay the.
set_env_kv() {
  local tep="$1" khoa="$2" gia_tri="$3"
  touch "$tep"
  # Bao dam tep ket thuc bang xuong dong TRUOC khi noi them — thieu buoc nay thi khoa moi bi
  # dinh vao cuoi dong cuoi, tao ra mot dong hong va lam mat ca hai gia tri.
  [ -s "$tep" ] && [ "$(tail -c1 "$tep" | wc -l)" -eq 0 ] && printf '\n' >> "$tep"

  local tam="${tep}.tam.$$"
  awk -v k="$khoa" -v v="$gia_tri" '
    BEGIN { da_ghi = 0 }
    index($0, k "=") == 1 { if (!da_ghi) { print k "=" v; da_ghi = 1 } ; next }
    { print }
    END { if (!da_ghi) print k "=" v }
  ' "$tep" > "$tam"
  # Giu nguyen quyen cua tep goc (thuong la 600) thay vi de mv tao tep moi voi umask mac dinh.
  if [ -f "$tep" ]; then chmod --reference="$tep" "$tam" 2>/dev/null || chmod 600 "$tam"; fi
  mv "$tam" "$tep"
}

# Chi ghi khi khoa CHUA CO hoac dang rong. Day la nen tang cua tinh idempotent: chay lai
# install.sh tren mot he thong dang song KHONG duoc phep sinh lai bi mat nao — doi
# QLGX_JWT_KEY se dang xuat toan bo nguoi dung, doi mat khau CSDL se lam API mat ket noi vao
# chinh CSDL dang chay.
dat_neu_chua_co() {
  local tep="$1" khoa="$2" gia_tri="$3"
  local hien_tai
  hien_tai=$(doc_env_kv "$tep" "$khoa")
  [ -n "$hien_tai" ] && return 0
  set_env_kv "$tep" "$khoa" "$gia_tri"
}
```

- [ ] **Step 4: Chạy test để xác nhận nó đạt**

Run: `docker run --rm -v "$PWD:/code" bats/bats:1.11.0 /code/WebApp/tests/shell/chung.bats`
Expected: PASS — 9/9.

- [ ] **Step 5: Viết test thất bại cho `os_adapter.sh`**

Tạo `WebApp/tests/shell/os_adapter.bats`:

```bash
#!/usr/bin/env bats

setup() {
  # shellcheck disable=SC1090
  source "${BATS_TEST_DIRNAME}/../../scripts/os_adapter.sh"
}

@test "os_ho nhan dien debian tu ID_LIKE" {
  TEP_OS_RELEASE="$BATS_TEST_TMPDIR/os-release"
  printf 'ID=ubuntu\nID_LIKE=debian\n' > "$TEP_OS_RELEASE"
  [ "$(os_ho)" = "debian" ]
}

@test "os_ho nhan dien rhel tu ID_LIKE" {
  TEP_OS_RELEASE="$BATS_TEST_TMPDIR/os-release"
  printf 'ID=rocky\nID_LIKE="rhel centos fedora"\n' > "$TEP_OS_RELEASE"
  [ "$(os_ho)" = "rhel" ]
}

@test "os_ho nhan dien debian thuan tu ID" {
  TEP_OS_RELEASE="$BATS_TEST_TMPDIR/os-release"
  printf 'ID=debian\n' > "$TEP_OS_RELEASE"
  [ "$(os_ho)" = "debian" ]
}

@test "os_ho bao khong ro voi distro la" {
  TEP_OS_RELEASE="$BATS_TEST_TMPDIR/os-release"
  printf 'ID=plan9\n' > "$TEP_OS_RELEASE"
  run os_ho
  [ "$status" -ne 0 ]
}

@test "os_co_lenh dung voi lenh chac chan co va khong co" {
  run os_co_lenh sh
  [ "$status" -eq 0 ]
  run os_co_lenh lenh_khong_bao_gio_ton_tai_zzz
  [ "$status" -ne 0 ]
}
```

- [ ] **Step 6: Chạy test để chắc chắn nó thất bại**

Run: `docker run --rm -v "$PWD:/code" bats/bats:1.11.0 /code/WebApp/tests/shell/os_adapter.bats`
Expected: FAIL — không tìm thấy `scripts/os_adapter.sh`.

- [ ] **Step 7: Viết `os_adapter.sh`**

```bash
#!/usr/bin/env bash
# Lop mong che khac biet giua cac ban phan phoi Linux. Moi noi khac trong bo script goi qua
# day, KHONG rai lenh apt/dnf khap noi — de them mot distro chi phai sua mot tep.

# Cho phep bo test tro sang tep gia; mac dinh la duong dan that cua he thong.
TEP_OS_RELEASE="${TEP_OS_RELEASE:-/etc/os-release}"

os_ho() {
  [ -r "$TEP_OS_RELEASE" ] || { echo "Khong doc duoc $TEP_OS_RELEASE" >&2; return 1; }
  local id id_like
  id=$(awk -F= '$1=="ID"{gsub(/"/,"",$2); print $2}' "$TEP_OS_RELEASE")
  id_like=$(awk -F= '$1=="ID_LIKE"{gsub(/"/,"",$2); print $2}' "$TEP_OS_RELEASE")
  case " $id $id_like " in
    *" debian "*|*" ubuntu "*) echo debian; return 0 ;;
    *" rhel "*|*" fedora "*|*" centos "*) echo rhel; return 0 ;;
  esac
  echo "Ban phan phoi khong duoc ho tro: ID=$id ID_LIKE=$id_like" >&2
  return 1
}

os_co_lenh() { command -v "$1" >/dev/null 2>&1; }

os_cai_goi() {
  case "$(os_ho)" in
    debian)
      DEBIAN_FRONTEND=noninteractive apt-get update -qq
      DEBIAN_FRONTEND=noninteractive apt-get install -y --no-install-recommends "$@"
      ;;
    rhel) dnf install -y "$@" ;;
    *) return 1 ;;
  esac
}
```

- [ ] **Step 8: Chạy test để xác nhận nó đạt**

Run: `docker run --rm -v "$PWD:/code" bats/bats:1.11.0 /code/WebApp/tests/shell/os_adapter.bats`
Expected: PASS — 5/5.

- [ ] **Step 9: Commit**

```bash
git add WebApp/scripts/chung.sh WebApp/scripts/os_adapter.sh WebApp/tests/shell/
git commit -m "Them thu vien shell dung chung va bo kiem thu bats"
```

---

## Task 9: SQL tạo hai vai trò RLS, chạy lại được nhiều lần

`TRIEN-KHAI.md` mục 5 hiện yêu cầu quản trị viên **chép-dán tay** đoạn SQL này. Đó là bước thủ công duy nhất còn sót giữa một quy trình đáng lẽ tự động hoàn toàn, và cũng là bước dễ quên nhất — quên thì RLS mất tác dụng âm thầm (Task 1 nay chặn được, nhưng chặn xong vẫn phải có cách tạo đúng).

**Files:**
- Create: `WebApp/scripts/sql/00-vai-tro-rls.sql`
- Create: `WebApp/tests/shell/vai-tro-rls.bats`

**Interfaces:**
- Consumes: biến `psql` `:qlgx_app_user`, `:qlgx_app_password`, `:qlgx_admin_user`, `:qlgx_admin_password` (truyền bằng `-v`).
- Produces: hai vai trò LOGIN với thuộc tính `NOBYPASSRLS`/`BYPASSRLS` tương ứng, đủ quyền trên schema `public` kể cả bảng tạo sau này.

- [ ] **Step 1: Viết test thất bại**

Tạo `WebApp/tests/shell/vai-tro-rls.bats`:

```bash
#!/usr/bin/env bats
# Chay THAT tren mot PostgreSQL that (container dung mot lan) — kiem tra thuoc tinh vai tro,
# vi day chinh la thu de sai ma khong ai thay: mot vai tro nghiep vu lo co BYPASSRLS thi RLS
# vo hieu hoan toan ma he thong van chay binh thuong.

setup_file() {
  export TEN_CT="qlgx-thu-rls-$$"
  docker run -d --name "$TEN_CT" -e POSTGRES_PASSWORD=thu_nghiem_tam \
    -e POSTGRES_DB=qlgx postgres:17-alpine >/dev/null
  for _ in $(seq 1 30); do
    docker exec "$TEN_CT" pg_isready -U postgres -d qlgx >/dev/null 2>&1 && break
    sleep 1
  done
}

teardown_file() { docker rm -f "$TEN_CT" >/dev/null 2>&1 || true; }

chay_sql() {
  docker exec -i "$TEN_CT" psql -v ON_ERROR_STOP=1 -U postgres -d qlgx \
    -v qlgx_app_user=qlgx_app -v qlgx_app_password=mk_app \
    -v qlgx_admin_user=qlgx_admin -v qlgx_admin_password=mk_admin \
    -f - < "${BATS_TEST_DIRNAME}/../../scripts/sql/00-vai-tro-rls.sql"
}

hoi() { docker exec "$TEN_CT" psql -tAX -U postgres -d qlgx -c "$1"; }

@test "tao duoc hai vai tro voi dung thuoc tinh bypassrls" {
  run chay_sql
  [ "$status" -eq 0 ]
  [ "$(hoi "SELECT rolbypassrls FROM pg_roles WHERE rolname='qlgx_app'")" = "f" ]
  [ "$(hoi "SELECT rolbypassrls FROM pg_roles WHERE rolname='qlgx_admin'")" = "t" ]
  [ "$(hoi "SELECT rolcanlogin FROM pg_roles WHERE rolname='qlgx_app'")" = "t" ]
  [ "$(hoi "SELECT rolsuper FROM pg_roles WHERE rolname='qlgx_admin'")" = "f" ]
}

@test "chay lai lan hai khong loi va khong doi thuoc tinh" {
  chay_sql
  run chay_sql
  [ "$status" -eq 0 ]
  [ "$(hoi "SELECT rolbypassrls FROM pg_roles WHERE rolname='qlgx_app'")" = "f" ]
}

@test "vai tro nghiep vu ghi duoc vao bang tao SAU khi cap quyen" {
  chay_sql
  hoi "CREATE TABLE bang_moi_sau(id int)"
  run docker exec "$TEN_CT" psql -tAX -U qlgx_app -d qlgx -c "INSERT INTO bang_moi_sau VALUES (1)"
  [ "$status" -eq 0 ]
}
```

- [ ] **Step 2: Chạy test để chắc chắn nó thất bại**

Run: `docker run --rm -v "$PWD:/code" -v /var/run/docker.sock:/var/run/docker.sock bats/bats:1.11.0 /code/WebApp/tests/shell/vai-tro-rls.bats`
Expected: FAIL — không tìm thấy tệp SQL.

**Lưu ý:** bộ test này cần Docker bên trong container bats. Nếu môi trường không cho mount docker socket, chạy trực tiếp trên host: `bats WebApp/tests/shell/vai-tro-rls.bats`.

- [ ] **Step 3: Viết SQL**

`WebApp/scripts/sql/00-vai-tro-rls.sql`:

```sql
-- Tạo hai vai trò cơ sở dữ liệu bắt buộc cho mô hình Row-Level Security của QLGX.
--
-- Vì sao phải là hai vai trò riêng: RLS chỉ có tác dụng với vai trò KHÔNG có BYPASSRLS. Vai trò
-- nghiệp vụ (qlgx_app) phục vụ mọi truy vấn đã xác thực nên bắt buộc bị RLS chặn — đó là lớp
-- phòng thủ thứ hai chống rò rỉ dữ liệu giữa các giáo xứ. Nhưng năm đường dẫn hợp lệ vẫn cần
-- đọc/ghi chéo giáo xứ (đăng nhập, tạo tài khoản quản trị đầu tiên, chuyển dữ liệu, quản lý
-- giáo xứ, nhập dữ liệu Access) nên cần một vai trò thứ hai CÓ BYPASSRLS — xem ChuoiKetNoiQuanTri.cs.
--
-- Idempotent: chạy lại nhiều lần không lỗi, và LUÔN đặt lại đúng thuộc tính BYPASSRLS kể cả khi
-- vai trò đã tồn tại với thuộc tính sai. Script cài đặt được thiết kế để chạy lại nhiều lần.
--
-- Cách gọi:
--   psql -v ON_ERROR_STOP=1 -U postgres -d qlgx \
--        -v qlgx_app_user=... -v qlgx_app_password=... \
--        -v qlgx_admin_user=... -v qlgx_admin_password=... -f 00-vai-tro-rls.sql

\set ON_ERROR_STOP on

-- Dùng format(%I/%L) để tên vai trò và mật khẩu được trích dẫn đúng chuẩn, không ghép chuỗi thô
-- (mật khẩu sinh ngẫu nhiên có thể chứa dấu nháy).
DO $$
DECLARE
    ten_app   text := :'qlgx_app_user';
    mk_app    text := :'qlgx_app_password';
    ten_admin text := :'qlgx_admin_user';
    mk_admin  text := :'qlgx_admin_password';
BEGIN
    IF ten_app = ten_admin THEN
        RAISE EXCEPTION 'Vai tro nghiep vu va vai tro quan tri KHONG duoc trung ten (%). '
                        'Trung ten nghia la Row-Level Security bi vo hieu hoan toan.', ten_app;
    END IF;

    -- Vai trò nghiệp vụ: KHÔNG BYPASSRLS.
    IF EXISTS (SELECT 1 FROM pg_roles WHERE rolname = ten_app) THEN
        EXECUTE format('ALTER ROLE %I LOGIN PASSWORD %L NOSUPERUSER NOBYPASSRLS', ten_app, mk_app);
    ELSE
        EXECUTE format('CREATE ROLE %I LOGIN PASSWORD %L NOSUPERUSER NOBYPASSRLS', ten_app, mk_app);
    END IF;

    -- Vai trò quản trị: CÓ BYPASSRLS, nhưng vẫn KHÔNG phải superuser.
    IF EXISTS (SELECT 1 FROM pg_roles WHERE rolname = ten_admin) THEN
        EXECUTE format('ALTER ROLE %I LOGIN PASSWORD %L NOSUPERUSER BYPASSRLS', ten_admin, mk_admin);
    ELSE
        EXECUTE format('CREATE ROLE %I LOGIN PASSWORD %L NOSUPERUSER BYPASSRLS', ten_admin, mk_admin);
    END IF;

    FOREACH ten_app IN ARRAY ARRAY[:'qlgx_app_user', :'qlgx_admin_user'] LOOP
        EXECUTE format('GRANT USAGE ON SCHEMA public TO %I', ten_app);
        EXECUTE format('GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO %I', ten_app);
        EXECUTE format('GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public TO %I', ten_app);
        -- Bảng và sequence do EF Core migration tạo SAU lệnh này cũng phải cấp được quyền —
        -- thiếu ALTER DEFAULT PRIVILEGES thì mỗi lần thêm bảng mới lại phải cấp quyền lại tay.
        EXECUTE format(
            'ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL PRIVILEGES ON TABLES TO %I', ten_app);
        EXECUTE format(
            'ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL PRIVILEGES ON SEQUENCES TO %I', ten_app);
    END LOOP;
END
$$;
```

**Lưu ý cho người thực hiện:** `ALTER DEFAULT PRIVILEGES` chỉ áp dụng cho đối tượng do **vai trò đang chạy lệnh** tạo ra. Migration EF Core chạy bằng vai trò nào thì phải đặt default privileges cho vai trò đó. Sau khi viết xong, kiểm bằng test thứ ba trong bộ bats ở trên — nó tạo bảng mới rồi ghi bằng `qlgx_app`; nếu thất bại, thêm `FOR ROLE <vai trò chạy migration>` vào lệnh `ALTER DEFAULT PRIVILEGES`.

- [ ] **Step 4: Chạy test để xác nhận nó đạt**

Run: `bats WebApp/tests/shell/vai-tro-rls.bats`
Expected: PASS — 3/3.

- [ ] **Step 5: Commit**

```bash
git add WebApp/scripts/sql/00-vai-tro-rls.sql WebApp/tests/shell/vai-tro-rls.bats
git commit -m "Them SQL tao hai vai tro RLS idempotent, thay cho buoc chep-dan tay"
```

---

## Task 10: `install.sh` — nửa đầu: tiền đề, phụ thuộc, mã nguồn, sinh bí mật

**Files:**
- Create: `WebApp/scripts/install.sh`
- Create: `WebApp/tests/shell/install-sinh-bi-mat.bats`

**Interfaces:**
- Consumes: `chung.sh`, `os_adapter.sh` (Task 8).
- Produces:
  - Biến toàn cục `GOC_UNG_DUNG=/opt/qlgx/WebApp`, `THU_MUC_CAU_HINH=/etc/qlgx`, `THU_MUC_SPOOL=/var/lib/qlgx/spool`, `THU_MUC_LOG=/var/log/qlgx`
  - `la_cai_moi()` → mã 0 nếu `$GOC_UNG_DUNG/.env` chưa tồn tại
  - `kiem_tra_tien_de()`, `cai_phu_thuoc()`, `cau_hinh_tuong_lua()`, `lay_ma_nguon()`
  - `sinh_env <tệp>` — sinh/giữ nguyên toàn bộ bí mật của `.env`
  - `ghi_backup_env <tệp>` — ghi `/etc/qlgx/backup.env` chmod 600
  - Biến môi trường điều khiển ở chế độ `--non-interactive`: `QLGX_R2_ENDPOINT`, `QLGX_R2_BUCKET`, `QLGX_R2_ACCESS_KEY_ID`, `QLGX_R2_SECRET_ACCESS_KEY`, `QLGX_TEN_MIEN`, `QLGX_GIAO_XU_TEN`, `QLGX_ADMIN_TEN_TAI_KHOAN`, `QLGX_ADMIN_MAT_KHAU`, `QLGX_ADMIN_HO_TEN`
  - Cờ nạp-để-test: đặt `QLGX_CHI_NAP_HAM=1` thì script chỉ định nghĩa hàm rồi thoát, không chạy gì

- [ ] **Step 1: Viết test thất bại**

Tạo `WebApp/tests/shell/install-sinh-bi-mat.bats`:

```bash
#!/usr/bin/env bats

setup() {
  export QLGX_CHI_NAP_HAM=1
  # shellcheck disable=SC1090
  source "${BATS_TEST_DIRNAME}/../../scripts/install.sh"
  TEP="$BATS_TEST_TMPDIR/.env"
}

@test "sinh_env tao du moi khoa bat buoc" {
  sinh_env "$TEP"
  for k in POSTGRES_DB POSTGRES_USER POSTGRES_PASSWORD \
           QLGX_APP_DB_USER QLGX_APP_DB_PASSWORD \
           QLGX_ADMIN_DB_USER QLGX_ADMIN_DB_PASSWORD QLGX_JWT_KEY; do
    [ -n "$(doc_env_kv "$TEP" "$k")" ] || { echo "thieu khoa $k"; return 1; }
  done
}

@test "hai vai tro CSDL sinh ra PHAI khac ten" {
  sinh_env "$TEP"
  [ "$(doc_env_kv "$TEP" QLGX_APP_DB_USER)" != "$(doc_env_kv "$TEP" QLGX_ADMIN_DB_USER)" ]
}

@test "chay lai sinh_env KHONG doi bat ky bi mat nao" {
  sinh_env "$TEP"
  truoc=$(cat "$TEP")
  sinh_env "$TEP"
  [ "$truoc" = "$(cat "$TEP")" ]
}

@test "mat khau sinh ra du dai" {
  sinh_env "$TEP"
  [ "${#$(doc_env_kv "$TEP" POSTGRES_PASSWORD)}" -ge 20 ] || \
    [ "$(doc_env_kv "$TEP" POSTGRES_PASSWORD | wc -c)" -ge 20 ]
}

@test "tep .env co quyen 600" {
  sinh_env "$TEP"
  [ "$(stat -c '%a' "$TEP")" = "600" ]
}

@test "ghi_backup_env dat quyen 600 va du khoa" {
  export QLGX_R2_ENDPOINT="https://vd.r2.cloudflarestorage.com"
  export QLGX_R2_BUCKET="qlgx-sao-luu"
  export QLGX_R2_ACCESS_KEY_ID="k"
  export QLGX_R2_SECRET_ACCESS_KEY="s"
  BE="$BATS_TEST_TMPDIR/backup.env"
  ghi_backup_env "$BE"
  [ "$(stat -c '%a' "$BE")" = "600" ]
  [ -n "$(doc_env_kv "$BE" RESTIC_PASSWORD)" ]
  [[ "$(doc_env_kv "$BE" RESTIC_REPOSITORY)" == s3:* ]]
  [[ "$(doc_env_kv "$BE" RESTIC_REPOSITORY)" == *qlgx-sao-luu* ]]
}

@test "ghi_backup_env chay lai KHONG doi mat khau restic" {
  export QLGX_R2_ENDPOINT="https://vd.r2.cloudflarestorage.com"
  export QLGX_R2_BUCKET="qlgx-sao-luu"
  export QLGX_R2_ACCESS_KEY_ID="k"
  export QLGX_R2_SECRET_ACCESS_KEY="s"
  BE="$BATS_TEST_TMPDIR/backup.env"
  ghi_backup_env "$BE"
  cu=$(doc_env_kv "$BE" RESTIC_PASSWORD)
  ghi_backup_env "$BE"
  [ "$(doc_env_kv "$BE" RESTIC_PASSWORD)" = "$cu" ]
}

@test "la_cai_moi dung theo su ton tai cua .env" {
  GOC_UNG_DUNG="$BATS_TEST_TMPDIR/ung-dung"
  mkdir -p "$GOC_UNG_DUNG"
  run la_cai_moi
  [ "$status" -eq 0 ]
  touch "$GOC_UNG_DUNG/.env"
  run la_cai_moi
  [ "$status" -ne 0 ]
}
```

- [ ] **Step 2: Chạy test để chắc chắn nó thất bại**

Run: `docker run --rm -v "$PWD:/code" bats/bats:1.11.0 /code/WebApp/tests/shell/install-sinh-bi-mat.bats`
Expected: FAIL — không tìm thấy `scripts/install.sh`.

- [ ] **Step 3: Viết nửa đầu `install.sh`**

```bash
#!/usr/bin/env bash
# Cai dat VA cap nhat QLGX Web tren mot may chu Linux.
#
#   curl -fsSL https://raw.githubusercontent.com/khoannd/qlgx/webapp-phase-1/WebApp/scripts/install.sh | sudo bash
#
# Mot script duy nhat cho ca hai viec la CO CHU DICH: no loai bo loai loi kinh dien "duong cai
# moi thi dung, duong nang cap thieu buoc". Script tu nhan biet trang thai bang su ton tai cua
# $GOC_UNG_DUNG/.env.
set -euo pipefail

readonly NHANH_MAC_DINH="webapp-phase-1"
readonly KHO_GIT="https://github.com/khoannd/qlgx.git"
readonly GOC_CHECKOUT="/opt/qlgx"
GOC_UNG_DUNG="${GOC_UNG_DUNG:-$GOC_CHECKOUT/WebApp}"
THU_MUC_CAU_HINH="${THU_MUC_CAU_HINH:-/etc/qlgx}"
THU_MUC_SPOOL="${THU_MUC_SPOOL:-/var/lib/qlgx/spool}"
THU_MUC_LOG="${THU_MUC_LOG:-/var/log/qlgx}"

NHANH="$NHANH_MAC_DINH"
KHONG_TUONG_TAC=0
CHAY_THU=0
CHI_TRANG_THAI=0
BO_QUA_SAO_LUU=0
TEN_MIEN="${QLGX_TEN_MIEN:-}"

# Nap thu vien dung chung. Khi chay qua `curl | bash` thi hai tep nay chua co tren dia — tai
# rieng chung ve thu muc tam truoc. Khi chay tu ban checkout thi nap thang.
nap_thu_vien() {
  local thu_muc_ke_ben
  thu_muc_ke_ben="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
  if [ -f "$thu_muc_ke_ben/chung.sh" ]; then
    # shellcheck source=/dev/null
    source "$thu_muc_ke_ben/chung.sh"
    # shellcheck source=/dev/null
    source "$thu_muc_ke_ben/os_adapter.sh"
    return
  fi
  local tam; tam=$(mktemp -d)
  local goc_raw="https://raw.githubusercontent.com/khoannd/qlgx/$NHANH/WebApp/scripts"
  curl -fsSL "$goc_raw/chung.sh" -o "$tam/chung.sh"
  curl -fsSL "$goc_raw/os_adapter.sh" -o "$tam/os_adapter.sh"
  # shellcheck source=/dev/null
  source "$tam/chung.sh"
  # shellcheck source=/dev/null
  source "$tam/os_adapter.sh"
}
nap_thu_vien

la_cai_moi() { [ ! -f "$GOC_UNG_DUNG/.env" ]; }

phan_tich_tham_so() {
  for t in "$@"; do
    case "$t" in
      --non-interactive) KHONG_TUONG_TAC=1 ;;
      --dry-run)         CHAY_THU=1 ;;
      --status)          CHI_TRANG_THAI=1 ;;
      --skip-backup)     BO_QUA_SAO_LUU=1 ;;
      --domain=*)        TEN_MIEN="${t#*=}" ;;
      --branch=*)        NHANH="${t#*=}" ;;
      *) bao_loi_va_thoat "Tham so khong hieu: $t" ;;
    esac
  done
}

kiem_tra_tien_de() {
  [ "$(id -u)" -eq 0 ] || bao_loi_va_thoat "Phai chay bang quyen root (dung sudo)."
  os_ho >/dev/null || bao_loi_va_thoat "Ban phan phoi Linux nay chua duoc ho tro."

  local so_cpu ram_mb dia_gb
  so_cpu=$(nproc)
  ram_mb=$(awk '/MemTotal/ {print int($2/1024)}' /proc/meminfo)
  dia_gb=$(df -BG --output=avail /opt 2>/dev/null | tail -1 | tr -dc '0-9')

  [ "$so_cpu" -ge 2 ]     || ghi_log canh-bao "Chi co $so_cpu CPU (khuyen nghi toi thieu 2)."
  [ "$ram_mb" -ge 3500 ]  || bao_loi_va_thoat "RAM $ram_mb MB, can toi thieu 4 GB."
  [ "${dia_gb:-0}" -ge 40 ] || bao_loi_va_thoat "Con ${dia_gb:-0} GB trong, can toi thieu 40 GB."
  ghi_log thong-tin "Tien de dat: $so_cpu CPU, $ram_mb MB RAM, ${dia_gb} GB trong."
}

cai_phu_thuoc() {
  local can=()
  os_co_lenh git    || can+=(git)
  os_co_lenh curl   || can+=(curl)
  os_co_lenh restic || can+=(restic)
  if [ ${#can[@]} -gt 0 ]; then
    ghi_log thong-tin "Cai goi: ${can[*]}"
    [ "$CHAY_THU" -eq 1 ] || os_cai_goi "${can[@]}"
  fi

  if ! os_co_lenh docker; then
    ghi_log thong-tin "Cai Docker Engine tu kho chinh thuc cua Docker."
    [ "$CHAY_THU" -eq 1 ] || curl -fsSL https://get.docker.com | sh
  fi
  docker compose version >/dev/null 2>&1 \
    || bao_loi_va_thoat "Thieu Docker Compose plugin (docker compose). Cai lai Docker Engine."
  [ "$CHAY_THU" -eq 1 ] || systemctl enable --now docker
}

cau_hinh_tuong_lua() {
  # Chi mo 22/80/443. Cong PostgreSQL KHONG BAO GIO mo ra ngoai — docker-compose.yml cung
  # khong map cong 5432 ra host.
  if os_co_lenh ufw; then
    ufw allow 22/tcp >/dev/null; ufw allow 80/tcp >/dev/null; ufw allow 443/tcp >/dev/null
    ufw --force enable >/dev/null
    ghi_log thong-tin "Tuong lua ufw: chi mo 22, 80, 443."
  elif os_co_lenh firewall-cmd; then
    systemctl enable --now firewalld
    firewall-cmd --permanent --add-service=ssh --add-service=http --add-service=https >/dev/null
    firewall-cmd --reload >/dev/null
    ghi_log thong-tin "Tuong lua firewalld: chi mo ssh, http, https."
  else
    ghi_log canh-bao "Khong tim thay ufw/firewalld — hay tu cau hinh tuong lua chi mo 22/80/443."
  fi
}

lay_ma_nguon() {
  mkdir -p "$THU_MUC_CAU_HINH" "$THU_MUC_SPOOL" "$THU_MUC_LOG"
  chmod 700 "$THU_MUC_CAU_HINH"

  if [ -d "$GOC_CHECKOUT/.git" ]; then
    ghi_log thong-tin "Da co ban checkout tai $GOC_CHECKOUT."
    return
  fi
  ghi_log thong-tin "Tai ma nguon nhanh $NHANH tu $KHO_GIT"
  # Sparse checkout: chi lay thu muc WebApp. BIN/, Source/, Release/ la ban desktop Windows,
  # vo dung tren may chu Linux va chiem hang tram MB nhi phan.
  git clone --depth 1 --filter=blob:none --sparse --branch "$NHANH" "$KHO_GIT" "$GOC_CHECKOUT"
  git -C "$GOC_CHECKOUT" sparse-checkout set WebApp
}

sinh_env() {
  local tep="$1"
  touch "$tep"; chmod 600 "$tep"

  dat_neu_chua_co "$tep" POSTGRES_DB            "qlgx"
  dat_neu_chua_co "$tep" POSTGRES_USER          "qlgx_chu"
  dat_neu_chua_co "$tep" POSTGRES_PASSWORD      "$(sinh_bi_mat 24)"
  # HAI vai tro RIENG BIET — trung ten nghia la Row-Level Security bi vo hieu hoan toan, va
  # API se tu choi khoi dong o moi truong san xuat (xem KiemTraCauHinh.cs).
  dat_neu_chua_co "$tep" QLGX_APP_DB_USER       "qlgx_app"
  dat_neu_chua_co "$tep" QLGX_APP_DB_PASSWORD   "$(sinh_bi_mat 24)"
  dat_neu_chua_co "$tep" QLGX_ADMIN_DB_USER     "qlgx_admin"
  dat_neu_chua_co "$tep" QLGX_ADMIN_DB_PASSWORD "$(sinh_bi_mat 24)"
  dat_neu_chua_co "$tep" QLGX_JWT_KEY           "$(sinh_bi_mat 32)"
  dat_neu_chua_co "$tep" QLGX_API_PORT          "8080"
  dat_neu_chua_co "$tep" QLGX_THU_MUC_SPOOL     "$THU_MUC_SPOOL"
  dat_neu_chua_co "$tep" ASPNETCORE_ENVIRONMENT "Production"

  # dat_neu_chua_co, KHONG phai set_env_kv: chay lai script tren mot he thong dang song ma sinh
  # lai bi mat se dang xuat toan bo nguoi dung (JWT) va lam API mat ket noi vao chinh CSDL dang
  # chay (mat khau). Day la bat bien quan trong nhat cua toan bo script nay.
}

hoi_hoac_bien() {
  local ten_bien="$1" cau_hoi="$2" gia_tri="${!1:-}"
  if [ -n "$gia_tri" ]; then printf '%s' "$gia_tri"; return; fi
  [ "$KHONG_TUONG_TAC" -eq 1 ] && bao_loi_va_thoat "Che do --non-interactive can bien $ten_bien."
  local tra_loi
  read -rp "$cau_hoi: " tra_loi </dev/tty
  printf '%s' "$tra_loi"
}

ghi_backup_env() {
  local tep="$1"
  local endpoint bucket khoa_id khoa_bi_mat
  endpoint=$(hoi_hoac_bien QLGX_R2_ENDPOINT "Dia chi endpoint R2 (vd https://<tai-khoan>.r2.cloudflarestorage.com)")
  bucket=$(hoi_hoac_bien QLGX_R2_BUCKET "Ten bucket R2")
  khoa_id=$(hoi_hoac_bien QLGX_R2_ACCESS_KEY_ID "R2 Access Key ID")
  khoa_bi_mat=$(hoi_hoac_bien QLGX_R2_SECRET_ACCESS_KEY "R2 Secret Access Key")

  touch "$tep"; chmod 600 "$tep"; chown root:root "$tep" 2>/dev/null || true

  set_env_kv "$tep" RESTIC_REPOSITORY        "s3:${endpoint%/}/$bucket"
  set_env_kv "$tep" AWS_ACCESS_KEY_ID        "$khoa_id"
  set_env_kv "$tep" AWS_SECRET_ACCESS_KEY    "$khoa_bi_mat"
  # Mat khau restic KHONG BAO GIO duoc sinh lai: doi no la moi ban sao luu cu tro thanh khong
  # doc duoc VINH VIEN. Day la ly do phai co The phuc hoi cat ngoai may chu.
  dat_neu_chua_co "$tep" RESTIC_PASSWORD     "$(sinh_bi_mat 32)"
  set_env_kv "$tep" QLGX_GIU_LAI             "--keep-last 8 --keep-daily 30 --keep-weekly 12 --keep-monthly 24"
  set_env_kv "$tep" QLGX_GOC_UNG_DUNG        "$GOC_UNG_DUNG"
  set_env_kv "$tep" QLGX_THU_MUC_SPOOL       "$THU_MUC_SPOOL"
  set_env_kv "$tep" QLGX_THU_MUC_LOG         "$THU_MUC_LOG"

  ghi_log thong-tin "Da ghi cau hinh sao luu vao $tep (chi root doc duoc)."
}

# Cho phep bo test nap file nay ma khong chay gi.
[ "${QLGX_CHI_NAP_HAM:-0}" = "1" ] && return 0
```

- [ ] **Step 4: Chạy test để xác nhận nó đạt**

Run: `docker run --rm -v "$PWD:/code" bats/bats:1.11.0 /code/WebApp/tests/shell/install-sinh-bi-mat.bats`
Expected: PASS — 8/8. (Nếu test "mat khau du dai" báo lỗi cú pháp bash, sửa lại thành `[ "$(doc_env_kv "$TEP" POSTGRES_PASSWORD | wc -c)" -ge 20 ]` — cú pháp `${#$(...)}` không hợp lệ.)

- [ ] **Step 5: Commit**

```bash
git add WebApp/scripts/install.sh WebApp/tests/shell/install-sinh-bi-mat.bats
git commit -m "install.sh nua dau: tien de, phu thuoc, ma nguon, sinh bi mat idempotent"
```

---

## Task 11: `install.sh` — nửa sau: dựng dịch vụ, HTTPS, Thẻ phục hồi, bảng tự kiểm chứng

**Files:**
- Modify: `WebApp/scripts/install.sh` (thêm hàm, và khối `main` ở cuối)
- Create: `WebApp/docker-compose.prod.yml`
- Create: `WebApp/tests/shell/install-e2e.sh`

**Interfaces:**
- Consumes: mọi hàm của Task 10; `sql/00-vai-tro-rls.sql` (Task 9); endpoint `/api/suc-khoe/san-sang` (Task 2); biến `QLGX_ADMIN_TAO_GIAO_XU_NEU_CHUA_CO` (Task 3).
- Produces:
  - `dc <lệnh compose...>` — bọc `docker compose` với đúng thư mục và hai tệp overlay
  - `bat_postgres()`, `tao_vai_tro_rls()`, `bat_api()`, `khoi_tao_giao_xu_va_admin()`, `cau_hinh_https()`, `in_the_phuc_hoi()`
  - `tu_kiem_chung()` — in bảng ĐẠT/KHÔNG ĐẠT, **trả mã thoát khác 0 nếu có bất kỳ dòng nào không đạt**
  - `cho_san_sang <số giây>` — chờ `/api/suc-khoe/san-sang` trả 200

- [ ] **Step 1: Viết kiểm thử đầu-cuối thất bại**

Tạo `WebApp/tests/shell/install-e2e.sh`:

```bash
#!/usr/bin/env bash
# Kiem thu dau-cuoi cua install.sh trong mot container Ubuntu dung MOT LAN.
#
# KHONG BAO GIO chay tren may nguoi dung (CLAUDE.md nguyen tac so 3). Container duoc cap
# --privileged de chay Docker ben trong (dind).
set -euo pipefail

readonly TEN_CT="qlgx-thu-install-$$"
don_dep() { docker rm -f "$TEN_CT" >/dev/null 2>&1 || true; }
trap don_dep EXIT

docker run -d --privileged --name "$TEN_CT" \
  -v "$(cd "$(dirname "$0")/../.." && pwd)":/ma-nguon:ro \
  ubuntu:24.04 sleep infinity >/dev/null

chay() { docker exec -i "$TEN_CT" bash -c "$1"; }

echo "==> Chuan bi container"
chay 'apt-get update -qq && apt-get install -y -qq curl git ca-certificates sudo >/dev/null'
chay 'curl -fsSL https://get.docker.com | sh >/dev/null 2>&1 && (dockerd >/tmp/dockerd.log 2>&1 &) && sleep 8'

echo "==> Chay install.sh lan 1 (cai moi)"
chay '
  set -e
  mkdir -p /opt/qlgx && cp -r /ma-nguon/WebApp /opt/qlgx/
  export QLGX_R2_ENDPOINT=http://khong-dung-that
  export QLGX_R2_BUCKET=thu
  export QLGX_R2_ACCESS_KEY_ID=k
  export QLGX_R2_SECRET_ACCESS_KEY=s
  export QLGX_GIAO_XU_TEN="Giao xu Thu Nghiem"
  export QLGX_ADMIN_TEN_TAI_KHOAN=quantri_thu
  export QLGX_ADMIN_MAT_KHAU=MatKhauThu12345
  export QLGX_ADMIN_HO_TEN="Nguoi thu nghiem"
  export QLGX_BO_QUA_R2=1
  bash /opt/qlgx/WebApp/scripts/install.sh --non-interactive
'

echo "==> Bang tu kiem chung phai DAT toan bo"
chay 'bash /opt/qlgx/WebApp/scripts/install.sh --status'

echo "==> Chay lai lan 2: phai idempotent, KHONG doi bi mat nao"
truoc=$(chay 'sha256sum /opt/qlgx/WebApp/.env /etc/qlgx/backup.env')
chay 'bash /opt/qlgx/WebApp/scripts/install.sh --non-interactive'
sau=$(chay 'sha256sum /opt/qlgx/WebApp/.env /etc/qlgx/backup.env')
[ "$truoc" = "$sau" ] || { echo "THAT BAI: chay lai da doi bi mat"; exit 1; }

echo "==> Bang tu kiem chung phai BIET BAO LOI: pha mot dieu kien"
chay 'docker stop $(docker ps -q --filter name=postgres) >/dev/null'
if chay 'bash /opt/qlgx/WebApp/scripts/install.sh --status'; then
  echo "THAT BAI: bang tu kiem chung van bao DAT du postgres da dung"; exit 1
fi

echo "==> DAT: install.sh cai duoc, idempotent, va bang kiem chung biet bao loi"
```

- [ ] **Step 2: Chạy để chắc chắn nó thất bại**

Run: `bash WebApp/tests/shell/install-e2e.sh`
Expected: FAIL — `install.sh` chưa có hàm `main`, chưa dựng được dịch vụ nào.

- [ ] **Step 3: Viết `docker-compose.prod.yml`**

```yaml
# Lop phu cho trien khai THAT — dung kem docker-compose.yml:
#   docker compose -f docker-compose.yml -f docker-compose.prod.yml up -d
#
# Chi chua nhung gi KHAC voi moi truong pilot: reverse proxy co HTTPS tu dong, va xoay vong log
# de o dia khong day len sau vai thang chay lien tuc.
services:
  api:
    # Khong con map cong ra host — moi luu luong di qua Caddy. Cong 8080 chi con trong mang
    # noi bo cua compose.
    ports: !reset []
    logging:
      driver: json-file
      options: { max-size: "10m", max-file: "5" }

  postgres:
    logging:
      driver: json-file
      options: { max-size: "10m", max-file: "5" }

  caddy:
    image: caddy:2-alpine
    restart: unless-stopped
    depends_on: [api]
    ports:
      - "80:80"
      - "443:443"
    volumes:
      - ./Caddyfile:/etc/caddy/Caddyfile:ro
      - caddy-data:/data      # chung chi Let's Encrypt — MAT volume nay la phai xin lai chung chi
      - caddy-config:/config
    logging:
      driver: json-file
      options: { max-size: "10m", max-file: "5" }

volumes:
  caddy-data:
  caddy-config:
```

- [ ] **Step 4: Viết nửa sau `install.sh`**

Chèn TRƯỚC dòng `[ "${QLGX_CHI_NAP_HAM:-0}" = "1" ] && return 0`:

```bash
# Boc docker compose: luon dung dung thu muc va dung hai tep overlay.
dc() { docker compose --project-directory "$GOC_UNG_DUNG" \
         -f "$GOC_UNG_DUNG/docker-compose.yml" \
         -f "$GOC_UNG_DUNG/docker-compose.prod.yml" "$@"; }

bat_postgres() {
  ghi_log thong-tin "Khoi dong PostgreSQL va cho san sang"
  dc up -d postgres
  local i
  for i in $(seq 1 60); do
    dc exec -T postgres pg_isready -q && return 0
    sleep 2
  done
  bao_loi_va_thoat "PostgreSQL khong san sang sau 120 giay. Xem: dc logs postgres"
}

tao_vai_tro_rls() {
  ghi_log thong-tin "Tao/cap nhat hai vai tro RLS"
  local db user
  db=$(doc_env_kv "$GOC_UNG_DUNG/.env" POSTGRES_DB)
  user=$(doc_env_kv "$GOC_UNG_DUNG/.env" POSTGRES_USER)
  # KHONG dung /docker-entrypoint-initdb.d: thu muc do chi chay khi volume du lieu con TRONG,
  # nen se bi bo qua o moi lan chay lai — dung luc ta can tinh idempotent nhat.
  dc exec -T postgres psql -v ON_ERROR_STOP=1 -U "$user" -d "$db" \
    -v qlgx_app_user="$(doc_env_kv "$GOC_UNG_DUNG/.env" QLGX_APP_DB_USER)" \
    -v qlgx_app_password="$(doc_env_kv "$GOC_UNG_DUNG/.env" QLGX_APP_DB_PASSWORD)" \
    -v qlgx_admin_user="$(doc_env_kv "$GOC_UNG_DUNG/.env" QLGX_ADMIN_DB_USER)" \
    -v qlgx_admin_password="$(doc_env_kv "$GOC_UNG_DUNG/.env" QLGX_ADMIN_DB_PASSWORD)" \
    < "$GOC_UNG_DUNG/scripts/sql/00-vai-tro-rls.sql"
}

cho_san_sang() {
  local gioi_han="${1:-180}" i
  for i in $(seq 1 "$gioi_han"); do
    if dc exec -T api curl -fsS http://localhost:8080/api/suc-khoe/san-sang >/dev/null 2>&1; then
      return 0
    fi
    sleep 1
  done
  return 1
}

bat_api() {
  ghi_log thong-tin "Dung image va khoi dong API (migration tu chay luc khoi dong)"
  dc build api
  dc up -d api
  cho_san_sang 300 || bao_loi_va_thoat "API khong san sang. Xem: docker compose logs api"
  ghi_log thong-tin "API da san sang."
}

khoi_tao_giao_xu_va_admin() {
  local ten_giao_xu ten_tk mat_khau ho_ten
  ten_giao_xu=$(hoi_hoac_bien QLGX_GIAO_XU_TEN "Ten giao xu")
  ten_tk=$(hoi_hoac_bien QLGX_ADMIN_TEN_TAI_KHOAN "Ten dang nhap quan tri he thong")
  mat_khau=$(hoi_hoac_bien QLGX_ADMIN_MAT_KHAU "Mat khau (toi thieu 8 ky tu)")
  ho_ten=$(hoi_hoac_bien QLGX_ADMIN_HO_TEN "Ho ten hien thi")

  # Chay lai voi cung ten tai khoan se bao "da ton tai" — coi la THANH CONG, khong phai loi,
  # vi script duoc thiet ke de chay lai nhieu lan.
  if dc exec -T \
      -e QLGX_ADMIN_GIAO_XU_TEN="$ten_giao_xu" \
      -e QLGX_ADMIN_TAO_GIAO_XU_NEU_CHUA_CO=true \
      -e QLGX_ADMIN_TEN_TAI_KHOAN="$ten_tk" \
      -e QLGX_ADMIN_MAT_KHAU="$mat_khau" \
      -e QLGX_ADMIN_HO_TEN="$ho_ten" \
      -e QLGX_ADMIN_LOAI_TAI_KHOAN=9 \
      api dotnet Qlgx.Api.dll tao-tai-khoan-quan-tri 2>&1 | tee /tmp/qlgx-tao-admin.log; then
    ghi_log thong-tin "Da tao tai khoan quan tri he thong '$ten_tk'."
  elif grep -q "da ton tai" /tmp/qlgx-tao-admin.log; then
    ghi_log thong-tin "Tai khoan '$ten_tk' da co san — bo qua."
  else
    bao_loi_va_thoat "Khong tao duoc tai khoan quan tri. Xem /tmp/qlgx-tao-admin.log"
  fi
}

cau_hinh_https() {
  if [ -z "$TEN_MIEN" ] && [ "$KHONG_TUONG_TAC" -eq 0 ]; then
    TEN_MIEN=$(hoi_hoac_bien QLGX_TEN_MIEN "Ten mien (de trong neu chua co)")
  fi
  if [ -z "$TEN_MIEN" ]; then
    ghi_log canh-bao "CHUA CO TEN MIEN — he thong se chay qua HTTP THUAN, khong ma hoa duong " \
                     "truyen. Du lieu giao dan (ho ten, ngay sinh, so can cuoc) di qua mang o " \
                     "dang doc duoc. Chay lai script voi --domain=<ten mien> ngay khi co."
    cat > "$GOC_UNG_DUNG/Caddyfile" <<'EOF'
:80 {
	reverse_proxy api:8080
}
EOF
  else
    cat > "$GOC_UNG_DUNG/Caddyfile" <<EOF
$TEN_MIEN {
	reverse_proxy api:8080

	header {
		# HSTS: mot khi trinh duyet da vao bang HTTPS thi khong bao gio thu HTTP nua.
		Strict-Transport-Security "max-age=31536000; includeSubDomains"
		X-Content-Type-Options "nosniff"
		Referrer-Policy "strict-origin-when-cross-origin"
		X-Frame-Options "DENY"
		-Server
	}

	# Anh dai dien tai len toi da 8 MB (xem XuLyAnh.cs) — chan som o day de yeu cau qua lon
	# khong di toi tan ung dung.
	request_body { max_size 10MB }

	encode gzip zstd
}
EOF
  fi
  dc up -d caddy
  ghi_log thong-tin "Caddy da chay${TEN_MIEN:+ cho $TEN_MIEN (HTTPS tu dong)}."
}

in_the_phuc_hoi() {
  local tep="$THU_MUC_CAU_HINH/the-phuc-hoi.txt"
  local be="$THU_MUC_CAU_HINH/backup.env"
  local env="$GOC_UNG_DUNG/.env"
  cat > "$tep" <<EOF
================== THE PHUC HOI QLGX ==================
May chu   : $(hostname)  ${TEN_MIEN:+($TEN_MIEN)}
Lap ngay  : $(date '+%d/%m/%Y %H:%M')

MAT KHAU RESTIC (khong co dong nay thi KHONG AI phuc hoi duoc,
ke ca Cloudflare — day la ban chat cua ma hoa phia may chu):
  $(doc_env_kv "$be" RESTIC_PASSWORD)

KHO SAO LUU : $(doc_env_kv "$be" RESTIC_REPOSITORY)
R2 KEY ID   : $(doc_env_kv "$be" AWS_ACCESS_KEY_ID)
R2 SECRET   : $(doc_env_kv "$be" AWS_SECRET_ACCESS_KEY)

MAT KHAU CSDL:
  postgres   : $(doc_env_kv "$env" POSTGRES_USER) / $(doc_env_kv "$env" POSTGRES_PASSWORD)
  qlgx_app   : $(doc_env_kv "$env" QLGX_APP_DB_USER) / $(doc_env_kv "$env" QLGX_APP_DB_PASSWORD)
  qlgx_admin : $(doc_env_kv "$env" QLGX_ADMIN_DB_USER) / $(doc_env_kv "$env" QLGX_ADMIN_DB_PASSWORD)

PHUC HOI TU MAY TRANG:
  1. Dung mot may chu Linux moi
  2. curl -fsSL $KHO_GIT/raw/$NHANH/WebApp/scripts/qlgx-restore.sh -o qlgx-restore.sh
  3. sudo bash qlgx-restore.sh --card the-phuc-hoi.txt          (in ra ke hoach)
  4. sudo bash qlgx-restore.sh --card the-phuc-hoi.txt --apply  (thuc hien)
=======================================================
EOF
  chmod 600 "$tep"
  cat "$tep"
  ghi_log canh-bao "IN THE TREN RA GIAY hoac chep vao noi an toan NGOAI may chu nay, roi xoa: " \
                   "rm $tep — de tren chinh may chu thi mat may la mat luon kha nang phuc hoi."
}

tu_kiem_chung() {
  local so_loi=0
  local env="$GOC_UNG_DUNG/.env"
  bao() { # bao <ten> <lenh...>
    local ten="$1"; shift
    if "$@" >/dev/null 2>&1; then printf '  DAT           %s\n' "$ten"
    else printf '  KHONG DAT     %s\n' "$ten"; so_loi=$((so_loi + 1)); fi
  }
  local db user app_user admin_user
  db=$(doc_env_kv "$env" POSTGRES_DB);       user=$(doc_env_kv "$env" POSTGRES_USER)
  app_user=$(doc_env_kv "$env" QLGX_APP_DB_USER)
  admin_user=$(doc_env_kv "$env" QLGX_ADMIN_DB_USER)
  # KHONG dinh nghia ham roi goi qua `bash -c`: subshell moi KHONG ke thua ham cua shell cha,
  # moi kiem tra se im lang that bai. Moi muc duoi day tu goi thang mot lenh.
  psql_hoi() { dc exec -T postgres psql -tAX -U "$user" -d "$db" -c "$1" | tr -d ' \r'; }

  bang_bang() { [ "$(psql_hoi "$1")" = "$2" ]; }
  it_nhat()   { [ "$(psql_hoi "$1")" -ge "$2" ] 2>/dev/null; }

  echo "--- Bang tu kiem chung ---"
  bao "Container postgres dang chay"  eval 'dc ps --status running postgres | grep -q postgres'
  bao "Container api dang chay"       eval 'dc ps --status running api | grep -q api'
  bao "API tra ve san sang"           dc exec -T api curl -fsS http://localhost:8080/api/suc-khoe/san-sang
  bao "Vai tro nghiep vu KHONG co BYPASSRLS" \
      bang_bang "SELECT rolbypassrls FROM pg_roles WHERE rolname='$app_user'" "f"
  bao "Vai tro quan tri CO BYPASSRLS" \
      bang_bang "SELECT rolbypassrls FROM pg_roles WHERE rolname='$admin_user'" "t"
  bao "RLS dang bat tren cac bang nghiep vu" \
      it_nhat "SELECT count(*) FROM pg_class WHERE relrowsecurity" 20
  bao "Hai vai tro CSDL khac nhau"    test "$app_user" != "$admin_user"
  bao "Tep .env quyen 600"            test "$(stat -c '%a' "$env")" = "600"
  bao "backup.env quyen 600, chu root" \
      test "$(stat -c '%a:%U' "$THU_MUC_CAU_HINH/backup.env")" = "600:root"
  bao "Container API KHONG biet khoa R2" \
      eval '! dc exec -T api env | grep -q AWS_SECRET_ACCESS_KEY'
  bao "In duoc PDF (Chromium co trong image)" \
      dc exec -T api sh -c 'find "$PLAYWRIGHT_BROWSERS_PATH" \( -name headless_shell -o -name chrome \) | head -1 | grep -q .'
  # The phuc hoi con nam tren chinh may chu sau 7 ngay nghia la no chua duoc cat ra ngoai —
  # mat may chu la mat luon kha nang phuc hoi. Bao KHONG DAT de nguoi van hanh nho lam not.
  bao "The phuc hoi da duoc cat ngoai may chu" \
      eval "[ ! -f '$THU_MUC_CAU_HINH/the-phuc-hoi.txt' ] || \
            [ -z \"\$(find '$THU_MUC_CAU_HINH/the-phuc-hoi.txt' -mtime +7)\" ]"
  if [ "${QLGX_BO_QUA_R2:-0}" != "1" ]; then
    bao "Kho restic mo duoc" \
      eval "set -a; . '$THU_MUC_CAU_HINH/backup.env'; set +a; restic snapshots --json >/dev/null"
  fi
  echo "--------------------------"
  if [ "$so_loi" -gt 0 ]; then
    ghi_log loi "$so_loi muc KHONG DAT."
    return 1
  fi
  ghi_log thong-tin "Toan bo muc kiem chung DAT."
}

main() {
  phan_tich_tham_so "$@"
  if [ "$CHI_TRANG_THAI" -eq 1 ]; then tu_kiem_chung; exit $?; fi

  kiem_tra_tien_de
  cai_phu_thuoc
  cau_hinh_tuong_lua
  lay_ma_nguon

  if la_cai_moi; then ghi_log thong-tin "=== CAI MOI ==="
  else ghi_log thong-tin "=== CAP NHAT ==="; cap_nhat; exit $?; fi

  sinh_env "$GOC_UNG_DUNG/.env"
  ghi_backup_env "$THU_MUC_CAU_HINH/backup.env"
  bat_postgres
  tao_vai_tro_rls
  bat_api
  khoi_tao_giao_xu_va_admin
  cau_hinh_https
  cai_dat_systemd
  if [ "${QLGX_BO_QUA_R2:-0}" != "1" ]; then
    ghi_log thong-tin "Chay sao luu dau tien de chung minh duong ong song that"
    "$GOC_UNG_DUNG/scripts/qlgx-runner.sh" sao-luu --nhan "cai-dat-lan-dau"
    "$GOC_UNG_DUNG/scripts/qlgx-runner.sh" kiem-tra
  fi
  tu_kiem_chung
  in_the_phuc_hoi
  ghi_log thong-tin "HOAN TAT. Mo: ${TEN_MIEN:+https://$TEN_MIEN}${TEN_MIEN:-http://<dia-chi-ip-may-chu>}"
}
```

Và thay dòng cuối tệp bằng:

```bash
# Cho phep bo test nap file nay ma khong chay gi.
[ "${QLGX_CHI_NAP_HAM:-0}" = "1" ] && return 0
main "$@"
```

**Lưu ý:** `cap_nhat` và `cai_dat_systemd` được viết ở Task 12 và Task 15. Cho tới lúc đó, tạm định nghĩa hai hàm rỗng có `ghi_log canh-bao "chua cai dat"` để `install.sh` chạy được; Task 12 và 15 sẽ thay thế chúng.

- [ ] **Step 5: Chạy kiểm thử đầu-cuối để xác nhận nó đạt**

Run: `bash WebApp/tests/shell/install-e2e.sh`
Expected: PASS — in "DAT: install.sh cai duoc, idempotent, va bang kiem chung biet bao loi".

Đặc biệt chú ý mục cuối: nếu bảng tự kiểm chứng **vẫn báo ĐẠT** sau khi dừng container postgres thì bảng đó vô dụng — sửa cho tới khi nó biết báo lỗi (`CLAUDE.md` nguyên tắc số 4).

- [ ] **Step 6: Commit**

```bash
git add WebApp/scripts/install.sh WebApp/docker-compose.prod.yml WebApp/tests/shell/install-e2e.sh
git commit -m "install.sh nua sau: dung dich vu, HTTPS Caddy, The phuc hoi, bang tu kiem chung"
```

---

## Task 12: Luồng cập nhật có sao lưu bắt buộc và tự quay lui

**Files:**
- Modify: `WebApp/scripts/install.sh` (thay hàm `cap_nhat` rỗng)
- Create: `WebApp/tests/shell/cap-nhat-e2e.sh`

**Interfaces:**
- Consumes: `dc`, `cho_san_sang`, `tu_kiem_chung` (Task 11); `qlgx-runner.sh sao-luu` (Task 13).
- Produces: `cap_nhat()`; tệp trạng thái `$GOC_UNG_DUNG/.phien-ban-truoc` chứa commit trước khi cập nhật; hàm `quay_lui()` dùng lại được bởi CLI `qlgx rollback`.

- [ ] **Step 1: Viết kiểm thử thất bại**

Tạo `WebApp/tests/shell/cap-nhat-e2e.sh`:

```bash
#!/usr/bin/env bash
# Kiem thu luong cap nhat, gom ca kich ban XAU: cap nhat lam hong he thong thi phai TU QUAY LUI.
set -euo pipefail

readonly TEN_CT="qlgx-thu-capnhat-$$"
trap 'docker rm -f "$TEN_CT" >/dev/null 2>&1 || true' EXIT

docker run -d --privileged --name "$TEN_CT" \
  -v "$(cd "$(dirname "$0")/../.." && pwd)":/ma-nguon:ro ubuntu:24.04 sleep infinity >/dev/null
chay() { docker exec -i "$TEN_CT" bash -c "$1"; }

chay 'apt-get update -qq && apt-get install -y -qq curl git ca-certificates >/dev/null'
chay 'curl -fsSL https://get.docker.com | sh >/dev/null 2>&1 && (dockerd >/tmp/d.log 2>&1 &) && sleep 8'
chay '
  mkdir -p /opt/qlgx && cp -r /ma-nguon/WebApp /opt/qlgx/ && cd /opt/qlgx && git init -q .
  git config user.email t@t && git config user.name t && git add -A && git commit -qm "ban dau"
  export QLGX_R2_ENDPOINT=x QLGX_R2_BUCKET=x QLGX_R2_ACCESS_KEY_ID=k QLGX_R2_SECRET_ACCESS_KEY=s
  export QLGX_GIAO_XU_TEN="Thu" QLGX_ADMIN_TEN_TAI_KHOAN=qt QLGX_ADMIN_MAT_KHAU=MatKhau12345
  export QLGX_ADMIN_HO_TEN="Thu" QLGX_BO_QUA_R2=1
  bash /opt/qlgx/WebApp/scripts/install.sh --non-interactive
'

echo "==> Khong co ban moi: phai bao 'da la ban moi nhat' va KHONG khoi dong lai"
id_truoc=$(chay 'docker inspect -f "{{.Id}}" $(docker ps -q --filter name=api)')
chay 'QLGX_BO_QUA_R2=1 bash /opt/qlgx/WebApp/scripts/install.sh --non-interactive' | grep -qi "moi nhat"
id_sau=$(chay 'docker inspect -f "{{.Id}}" $(docker ps -q --filter name=api)')
[ "$id_truoc" = "$id_sau" ] || { echo "THAT BAI: da khoi dong lai container du khong co ban moi"; exit 1; }

echo "==> Cap nhat hong: phai TU QUAY LUI va he thong van chay"
chay '
  cd /opt/qlgx
  echo "ENTRYPOINT [\"false\"]" >> WebApp/Dockerfile
  git add -A && git commit -qm "ban hong co y"
  git branch -f nhanh-thu && git remote add origin /opt/qlgx 2>/dev/null || true
'
if chay 'QLGX_BO_QUA_R2=1 bash /opt/qlgx/WebApp/scripts/install.sh --non-interactive'; then
  echo "THAT BAI: cap nhat hong ma van bao thanh cong"; exit 1
fi
chay 'bash /opt/qlgx/WebApp/scripts/install.sh --status' \
  || { echo "THAT BAI: khong quay lui duoc, he thong dang hong"; exit 1; }

echo "==> DAT: cap nhat co sao luu bat buoc va tu quay lui khi hong"
```

- [ ] **Step 2: Chạy để chắc chắn nó thất bại**

Run: `bash WebApp/tests/shell/cap-nhat-e2e.sh`
Expected: FAIL — `cap_nhat` hiện chỉ ghi log "chua cai dat".

- [ ] **Step 3: Viết `cap_nhat` và `quay_lui`**

Thay hàm `cap_nhat` rỗng trong `install.sh` bằng:

```bash
quay_lui() {
  local commit="${1:-}"
  [ -n "$commit" ] || commit=$(cat "$GOC_UNG_DUNG/.phien-ban-truoc" 2>/dev/null || true)
  [ -n "$commit" ] || bao_loi_va_thoat "Khong biet quay lui ve dau (thieu .phien-ban-truoc)."
  ghi_log canh-bao "Quay lui ve $commit"
  git -C "$GOC_CHECKOUT" checkout -q "$commit" --
  dc build api && dc up -d api
  cho_san_sang 300 \
    || bao_loi_va_thoat "Quay lui roi ma he thong VAN khong len duoc. Dung 'qlgx restore' voi " \
                        "ban sao 'truoc-cap-nhat' vua tao."
  ghi_log thong-tin "Da quay lui thanh cong ve $commit."
}

cap_nhat() {
  local hien_tai moi
  git -C "$GOC_CHECKOUT" fetch --quiet origin "$NHANH"
  hien_tai=$(git -C "$GOC_CHECKOUT" rev-parse HEAD)
  moi=$(git -C "$GOC_CHECKOUT" rev-parse "origin/$NHANH")

  if [ "$hien_tai" = "$moi" ]; then
    ghi_log thong-tin "Da la ban moi nhat ($(echo "$hien_tai" | cut -c1-8)) — khong lam gi."
    return 0
  fi
  ghi_log thong-tin "Co ban moi: $(echo "$hien_tai" | cut -c1-8) -> $(echo "$moi" | cut -c1-8)"

  # Sao luu BAT BUOC truoc khi cap nhat. Day la luoi an toan cuoi cung neu migration cua ban
  # moi lam hong lieu do — pg_advisory_lock chi chong hai tien trinh chay migration cung luc,
  # KHONG chong duoc mot migration sai.
  if [ "$BO_QUA_SAO_LUU" -eq 0 ] && [ "${QLGX_BO_QUA_R2:-0}" != "1" ]; then
    ghi_log thong-tin "Sao luu truoc khi cap nhat"
    "$GOC_UNG_DUNG/scripts/qlgx-runner.sh" sao-luu --nhan "truoc-cap-nhat" --nguon truoc_cap_nhat \
      || bao_loi_va_thoat "Sao luu truoc cap nhat THAT BAI — dung cap nhat. Sua sao luu truoc."
  else
    ghi_log canh-bao "BO QUA sao luu truoc cap nhat theo yeu cau."
  fi

  echo "$hien_tai" > "$GOC_UNG_DUNG/.phien-ban-truoc"

  git -C "$GOC_CHECKOUT" merge --ff-only "origin/$NHANH" \
    || bao_loi_va_thoat "Khong merge fast-forward duoc — ban checkout da bi sua tay?"

  # Bo sung khoa .env moi neu ban moi can, KHONG dung toi khoa cu (xem sinh_env).
  sinh_env "$GOC_UNG_DUNG/.env"

  if ! (dc build api && dc up -d api && cho_san_sang 300); then
    ghi_log loi "Ban moi khong len duoc — dang quay lui."
    quay_lui "$hien_tai"
    return 1
  fi

  tu_kiem_chung || { ghi_log loi "Kiem chung sau cap nhat KHONG DAT — quay lui."; quay_lui "$hien_tai"; return 1; }
  ghi_log thong-tin "Cap nhat xong: $(echo "$moi" | cut -c1-8)"
}
```

- [ ] **Step 4: Chạy kiểm thử để xác nhận nó đạt**

Run: `bash WebApp/tests/shell/cap-nhat-e2e.sh`
Expected: PASS — cả hai kịch bản (không có bản mới, và bản mới hỏng phải quay lui).

- [ ] **Step 5: Commit**

```bash
git add WebApp/scripts/install.sh WebApp/tests/shell/cap-nhat-e2e.sh
git commit -m "Luong cap nhat: sao luu bat buoc truoc, tu quay lui khi ban moi khong len duoc"
```

---

## Task 13: `qlgx-runner.sh` — sao lưu, giữ bản, đồng bộ danh sách

**Files:**
- Create: `WebApp/scripts/qlgx-runner.sh`
- Create: `WebApp/tests/shell/runner-sao-luu.bats`

**Interfaces:**
- Consumes: `chung.sh`; `/etc/qlgx/backup.env`; `dc` (định nghĩa lại cục bộ trong runner để chạy độc lập với `install.sh`).
- Produces:
  - `qlgx-runner.sh sao-luu [--nhan <nhãn>] [--nguon <nguồn>]`
  - `qlgx-runner.sh kiem-tra` — `restic check --read-data-subset=5%`
  - `qlgx-runner.sh dong-bo-danh-sach` — nạp lại bảng `ban_sao_luu` từ `restic snapshots --json`
  - `qlgx-runner.sh tai-ve <snapshot> <mã job>` — giải nén vào spool
  - `qlgx-runner.sh don-spool` — xoá thư mục spool quá 24 giờ
  - Hàm thuần test được: `tinh_nguon_tu_nhan <nhãn>`, `loc_snapshot_json` (đọc JSON restic → dòng TSV)

- [ ] **Step 1: Viết test thất bại**

Tạo `WebApp/tests/shell/runner-sao-luu.bats`:

```bash
#!/usr/bin/env bats

setup() {
  export QLGX_CHI_NAP_HAM=1
  # shellcheck disable=SC1090
  source "${BATS_TEST_DIRNAME}/../../scripts/qlgx-runner.sh"
}

@test "tinh_nguon_tu_nhan anh xa dung bon nguon" {
  [ "$(tinh_nguon_tu_nhan truoc-cap-nhat)" = "truoc_cap_nhat" ]
  [ "$(tinh_nguon_tu_nhan truoc-phuc-hoi)" = "truoc_phuc_hoi" ]
  [ "$(tinh_nguon_tu_nhan thu-cong)" = "thu_cong" ]
  [ "$(tinh_nguon_tu_nhan '')" = "tu_dong" ]
}

@test "loc_snapshot_json doc dung id, thoi diem va so lieu tu tags" {
  cat > "$BATS_TEST_TMPDIR/sn.json" <<'EOF'
[{"short_id":"ab12cd34","time":"2026-09-13T06:00:00.123456Z",
  "tags":["nhan=tu-dong","giao_dan=2050","gia_dinh=40"],"summary":{"total_bytes_processed":123456}}]
EOF
  run loc_snapshot_json "$BATS_TEST_TMPDIR/sn.json"
  [ "$status" -eq 0 ]
  [[ "$output" == *"ab12cd34"* ]]
  [[ "$output" == *"2050"* ]]
  [[ "$output" == *"40"* ]]
  [[ "$output" == *"123456"* ]]
}

@test "loc_snapshot_json chiu duoc snapshot thieu tags" {
  cat > "$BATS_TEST_TMPDIR/sn.json" <<'EOF'
[{"short_id":"ff00ff00","time":"2026-09-13T06:00:00Z","tags":[],"summary":{}}]
EOF
  run loc_snapshot_json "$BATS_TEST_TMPDIR/sn.json"
  [ "$status" -eq 0 ]
  [[ "$output" == *"ff00ff00"* ]]
}

@test "chuoi giu lai dung dung gia tri da chot trong thiet ke" {
  [ "$GIU_LAI_MAC_DINH" = "--keep-last 8 --keep-daily 30 --keep-weekly 12 --keep-monthly 24" ]
}
```

- [ ] **Step 2: Chạy test để chắc chắn nó thất bại**

Run: `docker run --rm -v "$PWD:/code" bats/bats:1.11.0 /code/WebApp/tests/shell/runner-sao-luu.bats`
Expected: FAIL — không tìm thấy `scripts/qlgx-runner.sh`.

- [ ] **Step 3: Viết `qlgx-runner.sh` (phần sao lưu)**

```bash
#!/usr/bin/env bash
# Bo chay cong viec sao luu/phuc hoi cua QLGX — chay TREN HOST, ngoai container.
#
# Vi sao ngoai container: (1) phuc hoi toan bo CSDL doi hoi ngat moi ket noi toi chinh CSDL ma
# ung dung dang dung roi doi ten no — ung dung khong the tu lam viec do voi chinh minh;
# (2) khoa R2 nam o /etc/qlgx/backup.env chi root doc duoc, nen ung dung web bi chiem quyen
# cung khong xoa duoc ban sao luu ngoai R2.
set -euo pipefail

THU_MUC_CAU_HINH="${THU_MUC_CAU_HINH:-/etc/qlgx}"
TEP_BACKUP_ENV="${TEP_BACKUP_ENV:-$THU_MUC_CAU_HINH/backup.env}"
GIU_LAI_MAC_DINH="--keep-last 8 --keep-daily 30 --keep-weekly 12 --keep-monthly 24"

nap_thu_vien_runner() {
  local d; d="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
  # shellcheck source=/dev/null
  source "$d/chung.sh"
}
nap_thu_vien_runner

nap_cau_hinh() {
  [ -r "$TEP_BACKUP_ENV" ] || bao_loi_va_thoat "Khong doc duoc $TEP_BACKUP_ENV (can quyen root)."
  set -a; # shellcheck source=/dev/null
  source "$TEP_BACKUP_ENV"; set +a
  GOC_UNG_DUNG="${QLGX_GOC_UNG_DUNG:-/opt/qlgx/WebApp}"
  THU_MUC_SPOOL="${QLGX_THU_MUC_SPOOL:-/var/lib/qlgx/spool}"
  THU_MUC_LOG="${QLGX_THU_MUC_LOG:-/var/log/qlgx}"
  GIU_LAI="${QLGX_GIU_LAI:-$GIU_LAI_MAC_DINH}"
  mkdir -p "$THU_MUC_SPOOL" "$THU_MUC_LOG"
}

dc() { docker compose --project-directory "$GOC_UNG_DUNG" \
         -f "$GOC_UNG_DUNG/docker-compose.yml" \
         -f "$GOC_UNG_DUNG/docker-compose.prod.yml" "$@"; }

env_ung_dung() { doc_env_kv "$GOC_UNG_DUNG/.env" "$1"; }

psql_quan_tri() {
  dc exec -T postgres psql -tAX -v ON_ERROR_STOP=1 \
    -U "$(env_ung_dung POSTGRES_USER)" -d "$(env_ung_dung POSTGRES_DB)" "$@";
}

tinh_nguon_tu_nhan() {
  case "$1" in
    truoc-cap-nhat) echo truoc_cap_nhat ;;
    truoc-phuc-hoi) echo truoc_phuc_hoi ;;
    thu-cong)       echo thu_cong ;;
    *)              echo tu_dong ;;
  esac
}

# Doc dau ra `restic snapshots --json` thanh dong TSV: id<TAB>thoi_diem<TAB>nhan<TAB>byte<TAB>
# so_giao_dan<TAB>so_gia_dinh. Dung python3 (co san tren moi ban phan phoi muc tieu) thay vi jq
# de khong them mot phu thuoc chi dung o mot cho.
loc_snapshot_json() {
  python3 - "$1" <<'PY'
import json, sys
def tag(tags, khoa, mac_dinh=""):
    for t in tags or []:
        if t.startswith(khoa + "="):
            return t.split("=", 1)[1]
    return mac_dinh
for s in json.load(open(sys.argv[1], encoding="utf-8")):
    print("\t".join([
        s.get("short_id", ""),
        s.get("time", ""),
        tag(s.get("tags"), "nhan"),
        str((s.get("summary") or {}).get("total_bytes_processed", 0) or 0),
        tag(s.get("tags"), "giao_dan", "0"),
        tag(s.get("tags"), "gia_dinh", "0"),
    ]))
PY
}

ghi_trang_thai_loi() {
  psql_quan_tri -c "UPDATE trang_thai_sao_luu SET \"LoiGanNhat\" = $$${1//$/}$$ WHERE \"Id\" = 1;" \
    >/dev/null 2>&1 || true
}

lenh_sao_luu() {
  local nhan="" nguon=""
  while [ $# -gt 0 ]; do
    case "$1" in
      --nhan)  nhan="$2"; shift 2 ;;
      --nguon) nguon="$2"; shift 2 ;;
      *) bao_loi_va_thoat "Tham so khong hieu: $1" ;;
    esac
  done
  [ -n "$nguon" ] || nguon=$(tinh_nguon_tu_nhan "$nhan")

  local tam; tam=$(mktemp -d -p /var/tmp qlgx-sao-luu.XXXXXX)
  chmod 700 "$tam"
  # Ban dump THO chua toan bo du lieu giao dan, chua ma hoa — khong bao gio de no nam lai tren
  # dia. trap xoa ca khi loi hoac bi ngat.
  trap 'rm -rf "$tam"' EXIT

  ghi_log thong-tin "Dump CSDL"
  # -Fd -Z0 (thu muc, KHONG tu nen) chu KHONG phai -Fc: -Fc nen san, ma mot byte doi o dau lam
  # toan bo luong nen doi theo — restic khong khu trung lap duoc gi va moi snapshot luu gan nhu
  # mot ban day du moi. Anh dai dien nam trong bytea va gan nhu khong bao gio doi, nen day la
  # khac biet lon: de restic tu nen va khu trung lap theo khoi. -j4 dump song song 4 bang mot
  # luc. Xem thiet ke muc 13.3.
  dc exec -T postgres pg_dump -Fd -Z0 -j4 -U "$(env_ung_dung POSTGRES_USER)" \
      -d "$(env_ung_dung POSTGRES_DB)" -f /tmp/qlgx-dump
  dc exec -T postgres tar -cf - -C /tmp qlgx-dump | tar -xf - -C "$tam"
  dc exec -T postgres rm -rf /tmp/qlgx-dump
  [ -s "$tam/qlgx-dump/toc.dat" ] \
    || bao_loi_va_thoat "pg_dump khong tao ra muc luc (toc.dat) — DUNG, khong sao luu."

  ghi_log thong-tin "Dump vai tro va quyen toan cuc"
  dc exec -T postgres pg_dumpall --globals-only -U "$(env_ung_dung POSTGRES_USER)" \
      > "$tam/globals.sql"
  [ -s "$tam/globals.sql" ] || bao_loi_va_thoat "pg_dumpall --globals-only tao ra tep rong — DUNG."

  ghi_log thong-tin "Gom tep cau hinh"
  mkdir -p "$tam/cau-hinh"
  for f in .env docker-compose.yml docker-compose.prod.yml Caddyfile; do
    [ -f "$GOC_UNG_DUNG/$f" ] && cp "$GOC_UNG_DUNG/$f" "$tam/cau-hinh/"
  done
  # /etc/qlgx/backup.env CO Y khong nam trong ban sao luu: khong tu ma hoa mot tep bang chinh
  # khoa chua trong tep do. Do la ly do phai co The phuc hoi cat NGOAI may chu.

  # Dem so lieu de gan vao tag — man hinh phuc hoi hien "quay ve day nghia la con 2050 giao dan".
  local so_gd so_gdinh
  so_gd=$(psql_quan_tri -c 'SELECT count(*) FROM giao_dan' | tr -d ' ' || echo 0)
  so_gdinh=$(psql_quan_tri -c 'SELECT count(*) FROM gia_dinh' | tr -d ' ' || echo 0)

  ghi_log thong-tin "Day len kho restic"
  restic backup "$tam" \
    --tag "nhan=${nhan:-tu-dong}" --tag "nguon=$nguon" \
    --tag "giao_dan=$so_gd" --tag "gia_dinh=$so_gdinh" \
    --host qlgx

  ghi_log thong-tin "Kiem tra toan ven truoc khi don ban cu"
  restic check --read-data-subset=5%

  # THU TU CUNG: backup -> check -> forget. Khong bao gio xoa ban cu truoc khi ban moi duoc
  # xac nhan doc duoc.
  ghi_log thong-tin "Don ban cu theo chinh sach giu"
  # shellcheck disable=SC2086
  restic forget $GIU_LAI --prune

  lenh_dong_bo_danh_sach
  psql_quan_tri -c "UPDATE trang_thai_sao_luu
                    SET \"SaoLuuGanNhat\" = now(), \"LoiGanNhat\" = NULL WHERE \"Id\" = 1;" >/dev/null
  ghi_log thong-tin "Sao luu xong ($so_gd giao dan, $so_gdinh gia dinh)."
}

lenh_kiem_tra() {
  ghi_log thong-tin "restic check --read-data-subset=5%"
  if restic check --read-data-subset=5%; then
    psql_quan_tri -c "UPDATE trang_thai_sao_luu SET \"LoiGanNhat\" = NULL WHERE \"Id\" = 1;" >/dev/null
  else
    ghi_trang_thai_loi "restic check that bai"
    return 1
  fi
}

lenh_dong_bo_danh_sach() {
  local tam; tam=$(mktemp); trap 'rm -f "$tam"' RETURN
  restic snapshots --json > "$tam"
  # Nap lai TOAN BO bang dem trong mot giao dich — don gian va luon dung, so snapshot chi vai
  # chuc dong nen khong can dong bo tang phan.
  {
    echo "BEGIN;"
    echo "DELETE FROM ban_sao_luu;"
    loc_snapshot_json "$tam" | while IFS=$'\t' read -r id thoi_diem nhan byte gd gdinh; do
      printf "INSERT INTO ban_sao_luu (\"Id\",\"ThoiDiem\",\"Nhan\",\"KichThuocByte\",\"SoGiaoDan\",\"SoGiaDinh\",\"Nguon\") VALUES ('%s','%s',%s,%s,%s,%s,'%s');\n" \
        "$id" "$thoi_diem" "$([ -n "$nhan" ] && printf "'%s'" "$nhan" || echo NULL)" \
        "${byte:-0}" "${gd:-0}" "${gdinh:-0}" "$(tinh_nguon_tu_nhan "$nhan")"
    done
    echo "COMMIT;"
  } | psql_quan_tri -f - >/dev/null
  ghi_log thong-tin "Da dong bo bang dem danh sach ban sao."
}

lenh_tai_ve() {
  local snapshot="$1" ma_job="$2"
  local dich="$THU_MUC_SPOOL/$ma_job"
  mkdir -p "$dich"; chmod 755 "$dich"
  restic restore "$snapshot" --target "$dich" --include '*/qlgx-dump'
  local thu_muc; thu_muc=$(find "$dich" -type d -name qlgx-dump | head -1)
  [ -n "$thu_muc" ] || bao_loi_va_thoat "Khong tim thay thu muc qlgx-dump trong snapshot $snapshot."
  # Dump la mot THU MUC (-Fd) — dong goi thanh MOT tep de trinh duyet tai ve duoc. Nen bang gzip
  # o day (khac voi luc sao luu, noi ta co y KHONG nen de restic khu trung lap).
  local tep="$dich/qlgx-$snapshot-$(date +%Y%m%d-%H%M).dump.tar.gz"
  tar -czf "$tep" -C "$(dirname "$thu_muc")" qlgx-dump
  rm -rf "$thu_muc"
  chmod 644 "$tep"
  ghi_log thong-tin "Da chuan bi tep tai ve tai $dich"
}

lenh_don_spool() {
  # Tep tai ve la ban dump CHUA MA HOA — chi giu 24 gio.
  find "$THU_MUC_SPOOL" -mindepth 1 -maxdepth 1 -type d -mmin +1440 -exec rm -rf {} + 2>/dev/null || true
}

main_runner() {
  nap_cau_hinh
  local lenh="${1:-}"; shift || true
  case "$lenh" in
    sao-luu)           lenh_sao_luu "$@" ;;
    kiem-tra)          lenh_kiem_tra ;;
    dong-bo-danh-sach) lenh_dong_bo_danh_sach ;;
    tai-ve)            lenh_tai_ve "$@" ;;
    don-spool)         lenh_don_spool ;;
    *) bao_loi_va_thoat "Lenh khong hieu: '$lenh'. Dung: sao-luu|kiem-tra|dong-bo-danh-sach|tai-ve|don-spool" ;;
  esac
}

[ "${QLGX_CHI_NAP_HAM:-0}" = "1" ] && return 0
main_runner "$@"
```

- [ ] **Step 4: Chạy test để xác nhận nó đạt**

Run: `docker run --rm -v "$PWD:/code" bats/bats:1.11.0 /code/WebApp/tests/shell/runner-sao-luu.bats`
Expected: PASS — 4/4.

- [ ] **Step 5: Kiểm chứng thật với kho restic cục bộ**

Chạy trên một máy có Docker, dùng kho restic trên đĩa (không cần R2):
```bash
sudo QLGX_GOC_UNG_DUNG=/opt/qlgx/WebApp \
     RESTIC_REPOSITORY=/var/tmp/kho-thu RESTIC_PASSWORD=thu \
     bash -c 'restic init 2>/dev/null; /opt/qlgx/WebApp/scripts/qlgx-runner.sh sao-luu --nhan thu-cong'
sudo RESTIC_REPOSITORY=/var/tmp/kho-thu RESTIC_PASSWORD=thu restic snapshots
```
Expected: một snapshot có đủ ba thành phần (thư mục `qlgx-dump/` có `toc.dat`, `globals.sql`, `cau-hinh/`) và các tag `giao_dan=`, `gia_dinh=`.

Sau đó kiểm chứng ràng buộc "snapshot thiếu thành phần thì không được tạo": tạm đổi tên container postgres cho `pg_dump` thất bại, chạy lại, xác nhận **không có snapshot mới nào** xuất hiện.

- [ ] **Step 6: Commit**

```bash
git add WebApp/scripts/qlgx-runner.sh WebApp/tests/shell/runner-sao-luu.bats
git commit -m "qlgx-runner.sh: sao luu ba thanh phan, kiem tra roi moi don ban cu"
```

---

## Task 14: `qlgx-restore.sh` — phục hồi sang bên rồi hoán đổi tên

Task quan trọng nhất của cả kế hoạch. Nguyên tắc: **không bao giờ ghi đè tại chỗ**. Cửa sổ "hệ thống không có dữ liệu" phải bằng không.

**Files:**
- Create: `WebApp/scripts/qlgx-restore.sh`
- Create: `WebApp/tests/shell/phuc-hoi-e2e.sh`
- Create: `WebApp/tests/shell/phuc-hoi.bats`

**Interfaces:**
- Consumes: `chung.sh`; `/etc/qlgx/backup.env` **hoặc** `--card <tệp thẻ phục hồi>`; `qlgx-runner.sh sao-luu` (Task 13).
- Produces:
  - `qlgx-restore.sh --snapshot <id> [--card <tệp>]` — chỉ IN KẾ HOẠCH
  - `qlgx-restore.sh --snapshot <id> --apply` — thực hiện
  - `qlgx-restore.sh --dien-tap --snapshot latest` — phục hồi vào `qlgx_dien_tap` rồi xoá
  - Hàm thuần: `doc_the_phuc_hoi <tệp>` (trích khoá từ Thẻ phục hồi), `ten_db_tam <tiền tố>` (sinh tên có dấu thời gian)

- [ ] **Step 1: Viết test thất bại cho hàm thuần**

Tạo `WebApp/tests/shell/phuc-hoi.bats`:

```bash
#!/usr/bin/env bats

setup() {
  export QLGX_CHI_NAP_HAM=1
  # shellcheck disable=SC1090
  source "${BATS_TEST_DIRNAME}/../../scripts/qlgx-restore.sh"
}

@test "doc_the_phuc_hoi trich dung khoa tu the" {
  cat > "$BATS_TEST_TMPDIR/the.txt" <<'EOF'
================== THE PHUC HOI QLGX ==================
MAT KHAU RESTIC (khong co dong nay thi KHONG AI phuc hoi duoc):
  bi-mat-restic-abc123
KHO SAO LUU : s3:https://vd.r2.cloudflarestorage.com/qlgx-sao-luu
R2 KEY ID   : ID_ABC
R2 SECRET   : SECRET_XYZ
EOF
  doc_the_phuc_hoi "$BATS_TEST_TMPDIR/the.txt"
  [ "$RESTIC_PASSWORD" = "bi-mat-restic-abc123" ]
  [ "$RESTIC_REPOSITORY" = "s3:https://vd.r2.cloudflarestorage.com/qlgx-sao-luu" ]
  [ "$AWS_ACCESS_KEY_ID" = "ID_ABC" ]
  [ "$AWS_SECRET_ACCESS_KEY" = "SECRET_XYZ" ]
}

@test "doc_the_phuc_hoi bao loi khi the thieu mat khau restic" {
  printf 'KHO SAO LUU : s3:x\n' > "$BATS_TEST_TMPDIR/hong.txt"
  run doc_the_phuc_hoi "$BATS_TEST_TMPDIR/hong.txt"
  [ "$status" -ne 0 ]
}

@test "ten_db_tam sinh ten hop le va khac nhau" {
  a=$(ten_db_tam qlgx_phuc_hoi); sleep 1; b=$(ten_db_tam qlgx_phuc_hoi)
  [ "$a" != "$b" ]
  [[ "$a" =~ ^qlgx_phuc_hoi_[0-9]{8}_[0-9]{6}$ ]]
}

@test "mac dinh la CHI IN KE HOACH, khong co --apply thi ap_dung=0" {
  phan_tich_tham_so_phuc_hoi --snapshot ab12cd34
  [ "$AP_DUNG" -eq 0 ]
  phan_tich_tham_so_phuc_hoi --snapshot ab12cd34 --apply
  [ "$AP_DUNG" -eq 1 ]
}
```

- [ ] **Step 2: Viết kiểm thử đầu-cuối thất bại**

Tạo `WebApp/tests/shell/phuc-hoi-e2e.sh`:

```bash
#!/usr/bin/env bash
# Kiem thu phuc hoi that: xoa du lieu roi phuc hoi lai, va kiem ca kich ban XAU (phuc hoi that
# bai thi he thong phai van chay nguyen ven).
set -euo pipefail
readonly TEN_CT="qlgx-thu-phuchoi-$$"
trap 'docker rm -f "$TEN_CT" >/dev/null 2>&1 || true' EXIT

docker run -d --privileged --name "$TEN_CT" \
  -v "$(cd "$(dirname "$0")/../.." && pwd)":/ma-nguon:ro ubuntu:24.04 sleep infinity >/dev/null
chay() { docker exec -i "$TEN_CT" bash -c "$1"; }

chay 'apt-get update -qq && apt-get install -y -qq curl git restic python3 ca-certificates >/dev/null'
chay 'curl -fsSL https://get.docker.com | sh >/dev/null 2>&1 && (dockerd >/tmp/d.log 2>&1 &) && sleep 8'
chay '
  mkdir -p /opt/qlgx && cp -r /ma-nguon/WebApp /opt/qlgx/
  export QLGX_R2_ENDPOINT=x QLGX_R2_BUCKET=x QLGX_R2_ACCESS_KEY_ID=k QLGX_R2_SECRET_ACCESS_KEY=s
  export QLGX_GIAO_XU_TEN="Thu" QLGX_ADMIN_TEN_TAI_KHOAN=qt QLGX_ADMIN_MAT_KHAU=MatKhau12345
  export QLGX_ADMIN_HO_TEN="Thu" QLGX_BO_QUA_R2=1
  bash /opt/qlgx/WebApp/scripts/install.sh --non-interactive
  # Kho restic tren dia, khong can R2 cho bo test nay.
  sed -i "s|^RESTIC_REPOSITORY=.*|RESTIC_REPOSITORY=/var/tmp/kho|" /etc/qlgx/backup.env
  set -a; . /etc/qlgx/backup.env; set +a; restic init
'

echo "==> Tao du lieu moc, sao luu, roi XOA du lieu"
chay '
  cd /opt/qlgx/WebApp
  docker compose exec -T postgres psql -U "$(grep ^POSTGRES_USER= .env | cut -d= -f2)" \
    -d "$(grep ^POSTGRES_DB= .env | cut -d= -f2)" -c "CREATE TABLE moc(v text); INSERT INTO moc VALUES (\"con-nguyen\");"
  ./scripts/qlgx-runner.sh sao-luu --nhan thu-cong
  docker compose exec -T postgres psql -U "$(grep ^POSTGRES_USER= .env | cut -d= -f2)" \
    -d "$(grep ^POSTGRES_DB= .env | cut -d= -f2)" -c "DROP TABLE moc;"
'

echo "==> Khong co --apply thi CHI IN KE HOACH, khong duoc doi gi"
chay '/opt/qlgx/WebApp/scripts/qlgx-restore.sh --snapshot latest' | grep -qi "ke hoach"
chay '
  cd /opt/qlgx/WebApp
  ! docker compose exec -T postgres psql -tAX -U "$(grep ^POSTGRES_USER= .env | cut -d= -f2)" \
      -d "$(grep ^POSTGRES_DB= .env | cut -d= -f2)" -c "SELECT 1 FROM moc" 2>/dev/null
' || { echo "THAT BAI: che do ke hoach da doi du lieu"; exit 1; }

echo "==> Phuc hoi that: du lieu phai quay lai"
chay '/opt/qlgx/WebApp/scripts/qlgx-restore.sh --snapshot latest --apply'
chay '
  cd /opt/qlgx/WebApp
  docker compose exec -T postgres psql -tAX -U "$(grep ^POSTGRES_USER= .env | cut -d= -f2)" \
    -d "$(grep ^POSTGRES_DB= .env | cut -d= -f2)" -c "SELECT v FROM moc"
' | grep -q "con-nguyen"

echo "==> He thong phai len lai duoc sau khi hoan doi"
chay 'bash /opt/qlgx/WebApp/scripts/install.sh --status'

echo "==> CSDL cu phai duoc giu lai de quay lui"
chay 'cd /opt/qlgx/WebApp && docker compose exec -T postgres psql -tAX -U postgres -l 2>/dev/null | grep -q qlgx_truoc_phuc_hoi' \
  || chay 'cd /opt/qlgx/WebApp && docker compose exec -T postgres psql -tAX -U "$(grep ^POSTGRES_USER= .env | cut -d= -f2)" -d postgres -c "SELECT datname FROM pg_database"' | grep -q qlgx_truoc_phuc_hoi

echo "==> Phuc hoi tu snapshot HONG: phai dung lai, he thong van chay"
if chay '/opt/qlgx/WebApp/scripts/qlgx-restore.sh --snapshot khong_ton_tai_zzz --apply'; then
  echo "THAT BAI: phuc hoi tu snapshot khong ton tai ma van bao thanh cong"; exit 1
fi
chay 'bash /opt/qlgx/WebApp/scripts/install.sh --status' \
  || { echo "THAT BAI: phuc hoi that bai da lam hong he thong dang chay"; exit 1; }

echo "==> DAT: phuc hoi an toan, quay lui duoc, that bai khong pha he thong"
```

- [ ] **Step 3: Chạy cả hai để chắc chắn chúng thất bại**

Run:
```bash
docker run --rm -v "$PWD:/code" bats/bats:1.11.0 /code/WebApp/tests/shell/phuc-hoi.bats
bash WebApp/tests/shell/phuc-hoi-e2e.sh
```
Expected: FAIL cả hai — chưa có `qlgx-restore.sh`.

- [ ] **Step 4: Viết `qlgx-restore.sh`**

```bash
#!/usr/bin/env bash
# Phuc hoi QLGX tu ban sao luu restic.
#
# NGUYEN TAC: KHONG BAO GIO ghi de tai cho. Cach sai (va la cach hau het huong dan tren mang
# chi) la dropdb && createdb && pg_restore — giua dropdb va pg_restore thanh cong co mot khoang
# thoi gian KHONG TON TAI DU LIEU NAO; pg_restore loi giua chung la mat trang.
#
# Cach o day: nap sang mot CSDL MOI, KIEM CHUNG no, roi HOAN DOI TEN. Hai lenh ALTER DATABASE
# RENAME gan nhu tuc thoi va dao nguoc duoc, nen cua so "he thong khong co du lieu" bang khong
# va thoi gian ngung phuc vu chi bang thoi gian khoi dong lai container API (vai giay), khong
# phai thoi gian pg_restore (co the vai phut).
#
# Chay duoc TREN MOT VPS TRANG chi voi The phuc hoi: --card the-phuc-hoi.txt
set -euo pipefail

THU_MUC_CAU_HINH="${THU_MUC_CAU_HINH:-/etc/qlgx}"
SNAPSHOT=""
AP_DUNG=0
DIEN_TAP=0
TEP_THE=""
MA_JOB=""
GIU_CSDL_CU_NGAY=7

nap_thu_vien_ph() {
  local d; d="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
  # shellcheck source=/dev/null
  source "$d/chung.sh"
}
nap_thu_vien_ph

phan_tich_tham_so_phuc_hoi() {
  AP_DUNG=0; DIEN_TAP=0
  while [ $# -gt 0 ]; do
    case "$1" in
      --snapshot) SNAPSHOT="$2"; shift 2 ;;
      --card)     TEP_THE="$2"; shift 2 ;;
      --ma-job)   MA_JOB="$2"; shift 2 ;;
      --apply)    AP_DUNG=1; shift ;;
      --dien-tap) DIEN_TAP=1; shift ;;
      *) bao_loi_va_thoat "Tham so khong hieu: $1" ;;
    esac
  done
}

# Trich khoa tu The phuc hoi (dinh dang do in_the_phuc_hoi trong install.sh sinh ra).
doc_the_phuc_hoi() {
  local tep="$1"
  [ -r "$tep" ] || bao_loi_va_thoat "Khong doc duoc the phuc hoi: $tep"
  RESTIC_PASSWORD=$(awk '/MAT KHAU RESTIC/{doc=1; next} doc && NF {gsub(/^[ \t]+/,""); print; exit}' "$tep")
  RESTIC_REPOSITORY=$(awk -F': *' '/^KHO SAO LUU/{print $2; exit}' "$tep")
  AWS_ACCESS_KEY_ID=$(awk -F': *' '/^R2 KEY ID/{print $2; exit}' "$tep")
  AWS_SECRET_ACCESS_KEY=$(awk -F': *' '/^R2 SECRET/{print $2; exit}' "$tep")
  [ -n "${RESTIC_PASSWORD:-}" ] || bao_loi_va_thoat "The phuc hoi thieu MAT KHAU RESTIC."
  [ -n "${RESTIC_REPOSITORY:-}" ] || bao_loi_va_thoat "The phuc hoi thieu KHO SAO LUU."
  export RESTIC_PASSWORD RESTIC_REPOSITORY AWS_ACCESS_KEY_ID AWS_SECRET_ACCESS_KEY
}

ten_db_tam() { printf '%s_%s' "$1" "$(date '+%Y%m%d_%H%M%S')"; }

nap_cau_hinh_ph() {
  if [ -n "$TEP_THE" ]; then
    doc_the_phuc_hoi "$TEP_THE"
  else
    [ -r "$THU_MUC_CAU_HINH/backup.env" ] \
      || bao_loi_va_thoat "Khong co $THU_MUC_CAU_HINH/backup.env va cung khong co --card."
    set -a; # shellcheck source=/dev/null
    source "$THU_MUC_CAU_HINH/backup.env"; set +a
  fi
  GOC_UNG_DUNG="${QLGX_GOC_UNG_DUNG:-/opt/qlgx/WebApp}"
  THU_MUC_LOG="${QLGX_THU_MUC_LOG:-/var/log/qlgx}"
  mkdir -p "$THU_MUC_LOG"
}

dc() { docker compose --project-directory "$GOC_UNG_DUNG" \
         -f "$GOC_UNG_DUNG/docker-compose.yml" \
         -f "$GOC_UNG_DUNG/docker-compose.prod.yml" "$@"; }
env_ung_dung() { doc_env_kv "$GOC_UNG_DUNG/.env" "$1"; }
pg() { dc exec -T postgres psql -tAX -v ON_ERROR_STOP=1 -U "$(env_ung_dung POSTGRES_USER)" "$@"; }

in_ke_hoach() {
  local db_hien="$1" db_moi="$2" gd_hien gd_sn gdinh_hien gdinh_sn
  gd_hien=$(pg -d "$db_hien" -c 'SELECT count(*) FROM giao_dan' 2>/dev/null || echo '?')
  gdinh_hien=$(pg -d "$db_hien" -c 'SELECT count(*) FROM gia_dinh' 2>/dev/null || echo '?')
  gd_sn=$(restic snapshots --json "$SNAPSHOT" 2>/dev/null \
          | grep -o '"giao_dan=[0-9]*"' | head -1 | tr -dc '0-9' || echo '?')
  gdinh_sn=$(restic snapshots --json "$SNAPSHOT" 2>/dev/null \
          | grep -o '"gia_dinh=[0-9]*"' | head -1 | tr -dc '0-9' || echo '?')
  cat <<EOF

=========== KE HOACH PHUC HOI (chua thuc hien gi) ===========
Snapshot          : $SNAPSHOT
CSDL hien tai     : $db_hien
CSDL nap tam vao  : $db_moi
CSDL cu se doi ten: ${db_hien}_truoc_phuc_hoi_... (giu $GIU_CSDL_CU_NGAY ngay)

Doi chieu so lieu:
                      hien tai      trong ban sao
  Giao dan        :   ${gd_hien}            ${gd_sn}
  Gia dinh        :   ${gdinh_hien}            ${gdinh_sn}

Cac buoc se lam:
  1. Sao luu ngay trang thai hien tai (--tag truoc-phuc-hoi)  [BAT BUOC]
  2. Nap snapshot vao CSDL MOI: $db_moi
  3. Kiem chung CSDL moi (dem ban ghi, RLS con bat, so giao xu)
  4. Dung container api
  5. Doi ten: $db_hien -> ban luu; $db_moi -> $db_hien
  6. Khoi dong api, cho san sang; KHONG len duoc thi DAO NGUOC doi ten

Them --apply de thuc hien.
=============================================================
EOF
}

kiem_chung_csdl_moi() {
  local db="$1" so_gd so_bang_rls so_gx
  so_gd=$(pg -d "$db" -c 'SELECT count(*) FROM giao_dan')
  so_gx=$(pg -d "$db" -c 'SELECT count(*) FROM giao_xu')
  so_bang_rls=$(pg -d "$db" -c 'SELECT count(*) FROM pg_class WHERE relrowsecurity')
  ghi_log thong-tin "Kiem chung: $so_gd giao dan, $so_gx giao xu, $so_bang_rls bang bat RLS."
  [ "$so_gx" -ge 1 ]      || bao_loi_va_thoat "CSDL nap vao KHONG co giao xu nao — ban sao hong."
  [ "$so_bang_rls" -ge 20 ] || bao_loi_va_thoat "Chi $so_bang_rls bang bat RLS — ban sao thieu chinh sach bao mat."
}

phuc_hoi_that() {
  local db_hien="$1" db_moi="$2"
  local nhat_ky="$THU_MUC_LOG/phuc-hoi-$(date '+%Y%m%d-%H%M%S').log"
  # Ghi song song ra tep tren host: chinh CSDL chua bang cong viec se bi thay the o buoc 5, nen
  # nhat ky trong CSDL se bien mat cung CSDL cu neu khong ghi ra ngoai.
  exec > >(tee -a "$nhat_ky") 2>&1
  ghi_log thong-tin "Nhat ky phuc hoi: $nhat_ky"

  ghi_log thong-tin "[1/6] Sao luu trang thai hien tai truoc khi ghi de"
  "$GOC_UNG_DUNG/scripts/qlgx-runner.sh" sao-luu --nhan truoc-phuc-hoi --nguon truoc_phuc_hoi \
    || bao_loi_va_thoat "Khong sao luu duoc trang thai hien tai — DUNG, khong phuc hoi."

  ghi_log thong-tin "[2/6] Nap snapshot $SNAPSHOT vao CSDL moi $db_moi"
  local tam; tam=$(mktemp -d -p /var/tmp qlgx-ph.XXXXXX); chmod 700 "$tam"
  trap 'rm -rf "$tam"' EXIT
  restic restore "$SNAPSHOT" --target "$tam" \
    || bao_loi_va_thoat "Khong lay duoc snapshot '$SNAPSHOT' — he thong hien tai KHONG bi dong toi."
  # Dump o dinh dang THU MUC (-Fd, xem qlgx-runner.sh) — nap lai bang cach chep ca thu muc vao
  # container postgres roi pg_restore -j4 song song, khong the truyen qua stdin nhu -Fc.
  local thu_muc_dump; thu_muc_dump=$(find "$tam" -type d -name qlgx-dump | head -1)
  [ -n "$thu_muc_dump" ] || bao_loi_va_thoat "Snapshot khong chua thu muc qlgx-dump."

  pg -d postgres -c "CREATE DATABASE \"$db_moi\";"
  tar -cf - -C "$(dirname "$thu_muc_dump")" qlgx-dump | dc exec -T postgres tar -xf - -C /tmp
  if ! dc exec -T postgres pg_restore -j4 --no-owner --role="$(env_ung_dung QLGX_ADMIN_DB_USER)" \
        -U "$(env_ung_dung POSTGRES_USER)" -d "$db_moi" /tmp/qlgx-dump; then
    dc exec -T postgres rm -rf /tmp/qlgx-dump
    ghi_log loi "pg_restore that bai — xoa CSDL tam, he thong hien tai VAN NGUYEN VEN."
    pg -d postgres -c "DROP DATABASE IF EXISTS \"$db_moi\" WITH (FORCE);"
    return 1
  fi
  dc exec -T postgres rm -rf /tmp/qlgx-dump

  ghi_log thong-tin "[3/6] Kiem chung CSDL moi"
  if ! kiem_chung_csdl_moi "$db_moi"; then
    pg -d postgres -c "DROP DATABASE IF EXISTS \"$db_moi\" WITH (FORCE);"
    return 1
  fi

  if [ "$DIEN_TAP" -eq 1 ]; then
    ghi_log thong-tin "DIEN TAP: dat yeu cau, xoa CSDL tam va DUNG (khong hoan doi)."
    pg -d postgres -c "DROP DATABASE IF EXISTS \"$db_moi\" WITH (FORCE);"
    return 0
  fi

  local db_luu="${db_hien}_truoc_phuc_hoi_$(date '+%Y%m%d_%H%M%S')"
  ghi_log thong-tin "[4/6] Dung container api"
  dc stop api

  ghi_log thong-tin "[5/6] Hoan doi ten: $db_hien -> $db_luu; $db_moi -> $db_hien"
  # ALTER DATABASE RENAME khong chay duoc khi con ket noi — dong not cac phien con sot.
  pg -d postgres -c "SELECT pg_terminate_backend(pid) FROM pg_stat_activity
                     WHERE datname IN ('$db_hien','$db_moi') AND pid <> pg_backend_pid();" >/dev/null
  pg -d postgres -c "ALTER DATABASE \"$db_hien\" RENAME TO \"$db_luu\";"
  pg -d postgres -c "ALTER DATABASE \"$db_moi\" RENAME TO \"$db_hien\";"

  ghi_log thong-tin "[6/6] Khoi dong api va cho san sang"
  dc up -d api
  local i sansang=0
  for i in $(seq 1 300); do
    dc exec -T api curl -fsS http://localhost:8080/api/suc-khoe/san-sang >/dev/null 2>&1 \
      && { sansang=1; break; }
    sleep 1
  done
  if [ "$sansang" -eq 0 ]; then
    ghi_log loi "He thong khong len duoc sau khi hoan doi — DAO NGUOC."
    dc stop api
    pg -d postgres -c "ALTER DATABASE \"$db_hien\" RENAME TO \"${db_moi}_hong\";"
    pg -d postgres -c "ALTER DATABASE \"$db_luu\" RENAME TO \"$db_hien\";"
    dc up -d api
    bao_loi_va_thoat "Da dao nguoc ve CSDL cu. Kiem tra $nhat_ky roi thu snapshot khac."
  fi

  ghi_log thong-tin "PHUC HOI XONG. CSDL cu giu lai ten '$db_luu' trong $GIU_CSDL_CU_NGAY ngay."
  # Ghi ket qua vao bang cong viec cua CSDL MOI (ban ghi cu da bien mat cung CSDL cu).
  if [ -n "$MA_JOB" ]; then
    pg -d "$db_hien" -c "UPDATE cong_viec_sao_luu
      SET \"TrangThai\"='xong', \"KetThucLuc\"=now(),
          \"NhatKy\"='Phuc hoi tu snapshot $SNAPSHOT. CSDL cu: $db_luu. Nhat ky: $nhat_ky'
      WHERE \"Id\"='$MA_JOB';" >/dev/null 2>&1 || true
  fi
}

don_csdl_cu() {
  local db_hien; db_hien=$(env_ung_dung POSTGRES_DB)
  pg -d postgres -tAc "SELECT datname FROM pg_database WHERE datname LIKE '${db_hien}_truoc_phuc_hoi_%'" \
  | while read -r ten; do
      local ngay; ngay=$(echo "$ten" | grep -o '[0-9]\{8\}' | head -1)
      local tuoi=$(( ( $(date +%s) - $(date -d "$ngay" +%s) ) / 86400 ))
      if [ "$tuoi" -gt "$GIU_CSDL_CU_NGAY" ]; then
        ghi_log thong-tin "Xoa CSDL cu $ten (da $tuoi ngay)."
        pg -d postgres -c "DROP DATABASE IF EXISTS \"$ten\" WITH (FORCE);"
      fi
    done
}

main_phuc_hoi() {
  phan_tich_tham_so_phuc_hoi "$@"
  nap_cau_hinh_ph
  [ -n "$SNAPSHOT" ] || SNAPSHOT="latest"

  local db_hien db_moi
  db_hien=$(env_ung_dung POSTGRES_DB)
  db_moi=$(ten_db_tam "$([ "$DIEN_TAP" -eq 1 ] && echo qlgx_dien_tap || echo qlgx_phuc_hoi)")

  if [ "$AP_DUNG" -eq 0 ] && [ "$DIEN_TAP" -eq 0 ]; then
    in_ke_hoach "$db_hien" "$db_moi"
    exit 0
  fi
  phuc_hoi_that "$db_hien" "$db_moi"
  don_csdl_cu
}

[ "${QLGX_CHI_NAP_HAM:-0}" = "1" ] && return 0
main_phuc_hoi "$@"
```

- [ ] **Step 5: Chạy cả hai bộ kiểm thử để xác nhận chúng đạt**

Run:
```bash
docker run --rm -v "$PWD:/code" bats/bats:1.11.0 /code/WebApp/tests/shell/phuc-hoi.bats
bash WebApp/tests/shell/phuc-hoi-e2e.sh
```
Expected: PASS cả hai. Bộ e2e phải qua đủ **cả hai kịch bản xấu**: chế độ kế hoạch không đổi gì, và phục hồi từ snapshot hỏng không phá hệ thống đang chạy.

- [ ] **Step 6: Commit**

```bash
git add WebApp/scripts/qlgx-restore.sh WebApp/tests/shell/phuc-hoi.bats WebApp/tests/shell/phuc-hoi-e2e.sh
git commit -m "qlgx-restore.sh: phuc hoi sang ben roi hoan doi ten, dao nguoc duoc trong vai giay"
```

---

## Task 15: Vòng lấy công việc, diễn tập phục hồi, systemd và CLI `qlgx`

Đây là task nối Giai đoạn B (bảng job) với Giai đoạn C (script): nút bấm trên web mới thực sự chạy được.

**Files:**
- Modify: `WebApp/scripts/qlgx-runner.sh` (thêm `lenh_chay_job`, `lenh_dien_tap`)
- Create: `WebApp/scripts/qlgx`
- Create: `WebApp/scripts/systemd/qlgx-runner.service`, `qlgx-runner.timer`, `qlgx-backup.service`, `qlgx-backup.timer`, `qlgx-verify.service`, `qlgx-verify.timer`, `qlgx-update.service`, `qlgx-update.timer`
- Modify: `WebApp/scripts/install.sh` (thay hàm `cai_dat_systemd` rỗng)
- Create: `WebApp/tests/shell/lay-job.bats`

**Interfaces:**
- Consumes: bảng `cong_viec_sao_luu` (Task 5); `lenh_sao_luu`, `lenh_kiem_tra`, `lenh_dong_bo_danh_sach`, `lenh_tai_ve` (Task 13); `qlgx-restore.sh` (Task 14).
- Produces:
  - `qlgx-runner.sh chay-job` — lấy tối đa một công việc rồi thoát (systemd timer gọi mỗi phút)
  - `qlgx-runner.sh dien-tap` — diễn tập phục hồi, cập nhật `TrangThaiSaoLuu`
  - Hàm thuần: `sql_gianh_job()` (in ra câu SQL), `doc_snapshot_tu_tham_so <json>`
  - CLI `qlgx {status|backup|restore|verify|update|rollback|logs|jobs}`

- [ ] **Step 1: Viết test thất bại**

Tạo `WebApp/tests/shell/lay-job.bats`:

```bash
#!/usr/bin/env bats

setup() {
  export QLGX_CHI_NAP_HAM=1
  # shellcheck disable=SC1090
  source "${BATS_TEST_DIRNAME}/../../scripts/qlgx-runner.sh"
}

@test "cau SQL gianh job dung FOR UPDATE SKIP LOCKED" {
  run sql_gianh_job
  [[ "$output" == *"FOR UPDATE SKIP LOCKED"* ]]
  [[ "$output" == *"trang_thai"* ]] || [[ "$output" == *"TrangThai"* ]]
  [[ "$output" == *"RETURNING"* ]]
}

@test "cau SQL gianh job chi lay dung MOT dong" {
  run sql_gianh_job
  [[ "$output" == *"LIMIT 1"* ]]
}

@test "doc_snapshot_tu_tham_so lay dung id" {
  [ "$(doc_snapshot_tu_tham_so '{"snapshotId":"ab12cd34","nhan":null}')" = "ab12cd34" ]
}

@test "doc_snapshot_tu_tham_so tra rong khi khong co" {
  [ -z "$(doc_snapshot_tu_tham_so '{"nhan":"x"}')" ]
}

@test "doc_snapshot_tu_tham_so chiu duoc JSON rong hoac hong" {
  [ -z "$(doc_snapshot_tu_tham_so '')" ]
  [ -z "$(doc_snapshot_tu_tham_so 'khong-phai-json')" ]
}
```

- [ ] **Step 2: Chạy test để chắc chắn nó thất bại**

Run: `docker run --rm -v "$PWD:/code" bats/bats:1.11.0 /code/WebApp/tests/shell/lay-job.bats`
Expected: FAIL — `sql_gianh_job` chưa tồn tại.

- [ ] **Step 3: Thêm vòng lấy job vào `qlgx-runner.sh`**

Chèn trước `main_runner`:

```bash
# Cau SQL gianh MOT cong viec. FOR UPDATE SKIP LOCKED bao dam hai luot chay chong nhau (timer no
# khi luot truoc con chay) khong the lay TRUNG mot job: luot thu hai bo qua dong da bi khoa thay
# vi cho, va vi ta LIMIT 1 nen no don gian khong lay duoc gi va thoat.
sql_gianh_job() {
  cat <<'SQL'
UPDATE cong_viec_sao_luu SET "TrangThai" = 'dang_chay', "BatDauLuc" = now()
WHERE "Id" = (SELECT "Id" FROM cong_viec_sao_luu WHERE "TrangThai" = 'cho'
              ORDER BY "TaoLuc" LIMIT 1 FOR UPDATE SKIP LOCKED)
RETURNING "Id"::text || E'\t' || "Loai" || E'\t' || coalesce("ThamSoJson", '{}');
SQL
}

doc_snapshot_tu_tham_so() {
  python3 -c '
import json, sys
try:
    print(json.loads(sys.argv[1] or "{}").get("snapshotId") or "")
except Exception:
    print("")
' "$1" 2>/dev/null || printf ''
}

ket_thuc_job() { # ket_thuc_job <id> <trang_thai> <nhat_ky>
  psql_quan_tri -c "UPDATE cong_viec_sao_luu
    SET \"TrangThai\" = '$2', \"KetThucLuc\" = now(), \"NhatKy\" = \$nk\$$3\$nk\$
    WHERE \"Id\" = '$1';" >/dev/null 2>&1 || true
}

lenh_chay_job() {
  local dong
  dong=$(psql_quan_tri -c "$(sql_gianh_job)" | head -1)
  [ -n "$dong" ] || return 0   # khong co viec gi — truong hop binh thuong nhat

  local ma loai tham_so
  IFS=$'\t' read -r ma loai tham_so <<< "$dong"
  ghi_log thong-tin "Nhan cong viec $loai ($ma)"

  local nhat_ky_tep="$THU_MUC_LOG/job-$ma.log"
  local ma_thoat=0
  case "$loai" in
    sao_luu)           lenh_sao_luu --nhan thu-cong --nguon thu_cong >"$nhat_ky_tep" 2>&1 || ma_thoat=$? ;;
    kiem_tra)          lenh_kiem_tra                                  >"$nhat_ky_tep" 2>&1 || ma_thoat=$? ;;
    dong_bo_danh_sach) lenh_dong_bo_danh_sach                         >"$nhat_ky_tep" 2>&1 || ma_thoat=$? ;;
    dien_tap)          lenh_dien_tap                                  >"$nhat_ky_tep" 2>&1 || ma_thoat=$? ;;
    tai_ve)
      lenh_tai_ve "$(doc_snapshot_tu_tham_so "$tham_so")" "$ma"       >"$nhat_ky_tep" 2>&1 || ma_thoat=$? ;;
    phuc_hoi)
      # qlgx-restore.sh tu ghi ket qua vao bang cong viec cua CSDL MOI sau khi hoan doi — ban
      # ghi trong CSDL cu bien mat cung CSDL do, nen KHONG goi ket_thuc_job o day khi thanh cong.
      "$GOC_UNG_DUNG/scripts/qlgx-restore.sh" \
        --snapshot "$(doc_snapshot_tu_tham_so "$tham_so")" --ma-job "$ma" --apply \
        >"$nhat_ky_tep" 2>&1 || ma_thoat=$?
      [ "$ma_thoat" -eq 0 ] && return 0
      ;;
    *) ma_thoat=1; echo "Loai cong viec khong hieu: $loai" > "$nhat_ky_tep" ;;
  esac

  local noi_dung; noi_dung=$(tail -c 8000 "$nhat_ky_tep" 2>/dev/null || echo "")
  if [ "$ma_thoat" -eq 0 ]; then
    ket_thuc_job "$ma" xong "$noi_dung"
    ghi_log thong-tin "Cong viec $ma xong."
  else
    ket_thuc_job "$ma" loi "$noi_dung"
    ghi_trang_thai_loi "Cong viec $loai that bai"
    ghi_log loi "Cong viec $ma that bai (ma $ma_thoat). Xem $nhat_ky_tep"
  fi
  lenh_don_spool
}

lenh_dien_tap() {
  # Ranh gioi giua "co sao luu" va "co kha nang phuc hoi": mot ban sao chua tung duoc phuc hoi
  # thu chi la mot gia dinh.
  if "$GOC_UNG_DUNG/scripts/qlgx-restore.sh" --snapshot latest --dien-tap; then
    psql_quan_tri -c "UPDATE trang_thai_sao_luu
      SET \"DienTapGanNhat\" = now(), \"DienTapDat\" = true WHERE \"Id\" = 1;" >/dev/null
    ghi_log thong-tin "Dien tap phuc hoi DAT."
  else
    psql_quan_tri -c "UPDATE trang_thai_sao_luu
      SET \"DienTapGanNhat\" = now(), \"DienTapDat\" = false,
          \"LoiGanNhat\" = 'Dien tap phuc hoi that bai' WHERE \"Id\" = 1;" >/dev/null
    ghi_log loi "Dien tap phuc hoi THAT BAI."
    return 1
  fi
}
```

Và bổ sung vào `case` trong `main_runner`:

```bash
    chay-job)          lenh_chay_job ;;
    dien-tap)          lenh_dien_tap ;;
```

Cập nhật câu thông báo lệnh không hiểu cho khớp danh sách mới.

- [ ] **Step 4: Chạy test để xác nhận nó đạt**

Run: `docker run --rm -v "$PWD:/code" bats/bats:1.11.0 /code/WebApp/tests/shell/lay-job.bats`
Expected: PASS — 5/5.

- [ ] **Step 5: Viết các unit systemd**

`WebApp/scripts/systemd/qlgx-runner.service`:
```ini
[Unit]
Description=QLGX — chay cong viec sao luu/phuc hoi dang cho
After=docker.service
Requires=docker.service

[Service]
Type=oneshot
EnvironmentFile=/etc/qlgx/backup.env
ExecStart=/opt/qlgx/WebApp/scripts/qlgx-runner.sh chay-job
# Chong chong lan: mot luot chay giu khoa, luot sau thoat ngay thay vi xep hang.
ExecStartPre=/usr/bin/flock -n /var/lock/qlgx-runner.lock true
```

`WebApp/scripts/systemd/qlgx-runner.timer`:
```ini
[Unit]
Description=QLGX — kiem hang doi cong viec moi phut

[Timer]
OnBootSec=2min
OnUnitActiveSec=1min
AccuracySec=10s

[Install]
WantedBy=timers.target
```

`WebApp/scripts/systemd/qlgx-backup.service`:
```ini
[Unit]
Description=QLGX — sao luu dinh ky len R2
After=docker.service
Requires=docker.service

[Service]
Type=oneshot
EnvironmentFile=/etc/qlgx/backup.env
ExecStart=/usr/bin/flock -n /var/lock/qlgx-runner.lock /opt/qlgx/WebApp/scripts/qlgx-runner.sh sao-luu
```

`WebApp/scripts/systemd/qlgx-backup.timer`:
```ini
[Unit]
Description=QLGX — sao luu 4 lan moi ngay (00:00, 06:00, 12:00, 18:00 gio VN)

[Timer]
OnCalendar=*-*-* 00,06,12,18:00:00 Asia/Ho_Chi_Minh
# Tranh dung phut tron — neu nhieu may chu cung lich thi khong cung luc day len R2.
RandomizedDelaySec=300
Persistent=true

[Install]
WantedBy=timers.target
```

`WebApp/scripts/systemd/qlgx-verify.service`:
```ini
[Unit]
Description=QLGX — dien tap phuc hoi hang tuan
After=docker.service
Requires=docker.service

[Service]
Type=oneshot
EnvironmentFile=/etc/qlgx/backup.env
ExecStart=/usr/bin/flock -n /var/lock/qlgx-runner.lock /opt/qlgx/WebApp/scripts/qlgx-runner.sh dien-tap
```

`WebApp/scripts/systemd/qlgx-verify.timer`:
```ini
[Unit]
Description=QLGX — dien tap phuc hoi Chu nhat 03:00

[Timer]
OnCalendar=Sun *-*-* 03:00:00 Asia/Ho_Chi_Minh
Persistent=true

[Install]
WantedBy=timers.target
```

`WebApp/scripts/systemd/qlgx-update.service`:
```ini
[Unit]
Description=QLGX — cap nhat phan mem
After=docker.service
Requires=docker.service

[Service]
Type=oneshot
ExecStart=/opt/qlgx/WebApp/scripts/install.sh --non-interactive
```

`WebApp/scripts/systemd/qlgx-update.timer`:
```ini
[Unit]
Description=QLGX — kiem ban cap nhat hang tuan (MAC DINH TAT)

[Timer]
OnCalendar=Mon *-*-* 04:00:00 Asia/Ho_Chi_Minh
Persistent=true

[Install]
WantedBy=timers.target
```

- [ ] **Step 6: Viết CLI `qlgx`**

`WebApp/scripts/qlgx`:
```bash
#!/usr/bin/env bash
# CLI van hanh QLGX. Chi la mot bo dieu phoi mong — moi viec that nam o cac script chuyen trach.
set -euo pipefail
readonly GOC="${QLGX_GOC_UNG_DUNG:-/opt/qlgx/WebApp}"

case "${1:-}" in
  status)   exec bash "$GOC/scripts/install.sh" --status ;;
  update)   shift; exec bash "$GOC/scripts/install.sh" --non-interactive "$@" ;;
  # Nap install.sh o che do chi-dinh-nghia-ham roi goi thang quay_lui. quay_lui tu doc commit
  # can quay ve tu $GOC/.phien-ban-truoc va tu dung dc/cho_san_sang da dinh nghia trong do.
  rollback) exec bash -c "QLGX_CHI_NAP_HAM=1 source '$GOC/scripts/install.sh'; quay_lui" ;;
  backup)   shift; exec bash "$GOC/scripts/qlgx-runner.sh" sao-luu "$@" ;;
  verify)   exec bash "$GOC/scripts/qlgx-runner.sh" dien-tap ;;
  check)    exec bash "$GOC/scripts/qlgx-runner.sh" kiem-tra ;;
  restore)  shift; exec bash "$GOC/scripts/qlgx-restore.sh" "$@" ;;
  jobs)
    exec docker compose --project-directory "$GOC" -f "$GOC/docker-compose.yml" \
      -f "$GOC/docker-compose.prod.yml" exec -T postgres \
      psql -U "$(grep '^POSTGRES_USER=' "$GOC/.env" | cut -d= -f2)" \
           -d "$(grep '^POSTGRES_DB=' "$GOC/.env" | cut -d= -f2)" \
           -c 'SELECT "TaoLuc","Loai","TrangThai","BuocHienTai" FROM cong_viec_sao_luu ORDER BY "TaoLuc" DESC LIMIT 20;' ;;
  logs)
    shift; exec docker compose --project-directory "$GOC" -f "$GOC/docker-compose.yml" \
      -f "$GOC/docker-compose.prod.yml" logs -f "${@:-api}" ;;
  *)
    cat <<'EOF'
Cach dung: qlgx <lenh>

  status              In bang tu kiem chung toan he thong
  backup [--nhan X]   Sao luu ngay
  check               Kiem tra tinh toan ven kho sao luu
  verify              Dien tap phuc hoi (khong dong toi du lieu that)
  restore --snapshot <id> [--apply]
                      Phuc hoi. KHONG co --apply thi chi in ke hoach.
  update              Kiem va cai ban moi (tu sao luu truoc, tu quay lui neu hong)
  rollback            Quay ve phien ban truoc
  jobs                20 cong viec gan nhat
  logs [dich-vu]      Xem log (mac dinh: api)
EOF
    exit 1 ;;
esac
```

- [ ] **Step 7: Viết `cai_dat_systemd` trong `install.sh`**

Thay hàm rỗng bằng:

```bash
cai_dat_systemd() {
  local bat_sao_luu="y"
  if [ "$KHONG_TUONG_TAC" -eq 0 ]; then
    read -rp "Bat sao luu tu dong 4 lan/ngay len R2? [Y/n]: " bat_sao_luu </dev/tty
    bat_sao_luu="${bat_sao_luu:-y}"
  fi

  install -m 755 "$GOC_UNG_DUNG/scripts/qlgx" /usr/local/bin/qlgx
  cp "$GOC_UNG_DUNG"/scripts/systemd/*.service "$GOC_UNG_DUNG"/scripts/systemd/*.timer /etc/systemd/system/
  systemctl daemon-reload

  case "$bat_sao_luu" in
    [Yy]*)
      systemctl enable --now qlgx-runner.timer qlgx-backup.timer qlgx-verify.timer
      ghi_log thong-tin "Da bat: sao luu 4 lan/ngay, dien tap phuc hoi hang tuan."
      ;;
    *)
      # Van bat runner.timer: khong co no thi nut bam tren giao dien web se khong bao gio chay.
      systemctl enable --now qlgx-runner.timer
      ghi_log canh-bao "CHUA bat sao luu tu dong. Bat sau bang: " \
                       "systemctl enable --now qlgx-backup.timer qlgx-verify.timer"
      ;;
  esac

  # Cap nhat tu dong MAC DINH TAT: cap nhat khong giam sat tren du lieu so sach giao xu la rui
  # ro khong dang. Bat tay bang: systemctl enable --now qlgx-update.timer
  systemctl disable qlgx-update.timer >/dev/null 2>&1 || true
}
```

- [ ] **Step 8: Kiểm chứng đầu-cuối vòng job**

Chạy trên máy đã cài (hoặc container e2e): tạo một job qua API rồi xác nhận runner nhận và hoàn thành:
```bash
qlgx jobs                       # ghi lai so dong hien tai
curl -fsS -X POST https://<ten-mien>/api/sao-luu/cong-viec \
  -H "Authorization: Bearer <token LoaiTaiKhoan=9>" \
  -H 'Content-Type: application/json' -d '{"loai":"kiem_tra"}'
sleep 90
qlgx jobs                       # dong moi phai o trang thai 'xong'
```
Expected: công việc chuyển `cho` → `dang_chay` → `xong` trong vòng ~90 giây mà không ai chạm tay.

- [ ] **Step 9: Commit**

```bash
git add WebApp/scripts/qlgx-runner.sh WebApp/scripts/qlgx WebApp/scripts/systemd/ \
        WebApp/scripts/install.sh WebApp/tests/shell/lay-job.bats
git commit -m "Vong lay cong viec, dien tap phuc hoi, systemd timer va CLI qlgx"
```

---

# Giai đoạn D — Giao diện quản trị

## Task 16: Kiểu dữ liệu, hàm gọi API và hàm định dạng ngày giờ

**Files:**
- Modify: `WebApp/src/web/src/api/types.ts` (thêm cuối tệp)
- Modify: `WebApp/src/web/src/api/client.ts` (thêm nhánh `saoLuu` cạnh `quanTri`)
- Modify: `WebApp/src/web/src/lib/ngay.ts` (thêm `dinhDangNgayGio`)
- Test: `WebApp/src/web/src/lib/ngay.test.ts` (bổ sung), `WebApp/src/web/src/api/client.test.ts` (bổ sung)

**Interfaces:**
- Produces:
  - `type TinhTrangSaoLuu`, `BanSaoLuu`, `CongViecSaoLuu`, `LoaiCongViecSaoLuu`, `TrangThaiCongViec` trong `types.ts`
  - `api.saoLuu.tinhTrang()`, `.danhSach()`, `.taoCongViec(than)`, `.congViec(id)`, `.congViecGanDay()`, `.duongDanTaiVe(maCongViec)`
  - `dinhDangNgayGio(iso?: string | null) → string` — `dd/MM/yyyy HH:mm`

- [ ] **Step 1: Viết test thất bại cho `dinhDangNgayGio`**

Thêm vào `WebApp/src/web/src/lib/ngay.test.ts`:

```ts
import { dinhDangNgayGio } from './ngay'

describe('dinhDangNgayGio', () => {
  it('doi ISO co gio sang dd/MM/yyyy HH:mm', () => {
    expect(dinhDangNgayGio('2026-09-13T06:05:00Z')).toMatch(/^13\/09\/2026 \d{2}:\d{2}$/)
  })

  it('tra chuoi rong khi khong co gia tri', () => {
    expect(dinhDangNgayGio(null)).toBe('')
    expect(dinhDangNgayGio(undefined)).toBe('')
  })

  it('giu nguyen van chuoi khong phan giai duoc, khong nem loi', () => {
    expect(dinhDangNgayGio('khong-phai-ngay')).toBe('khong-phai-ngay')
  })
})
```

- [ ] **Step 2: Chạy test để chắc chắn nó thất bại**

Run: `cd WebApp/src/web && npm test -- ngay`
Expected: FAIL — `dinhDangNgayGio` chưa được export.

- [ ] **Step 3: Viết `dinhDangNgayGio`**

Thêm vào `WebApp/src/web/src/lib/ngay.ts`:

```ts
/**
 * Định dạng một mốc thời gian ISO đầy đủ (có giờ) sang `dd/MM/yyyy HH:mm` theo giờ địa phương
 * của trình duyệt. Dùng cho các mốc thời gian THẬT (lúc sao lưu, lúc chạy công việc) — khác
 * `dinhDangNgay` vốn dành cho ngày nghiệp vụ dạng `DateOnly` không có múi giờ.
 *
 * Giữ nguyên văn chuỗi không phân giải được thay vì hiện "Invalid Date" — cùng nguyên tắc với
 * `dinhDangNgay`: dữ liệu lạ không được làm sập màn hình.
 */
export function dinhDangNgayGio(iso?: string | null): string {
  if (!iso) return ''
  const d = new Date(iso)
  if (Number.isNaN(d.getTime())) return iso
  const hai = (n: number) => String(n).padStart(2, '0')
  return `${hai(d.getDate())}/${hai(d.getMonth() + 1)}/${d.getFullYear()} ` +
         `${hai(d.getHours())}:${hai(d.getMinutes())}`
}
```

- [ ] **Step 4: Chạy test để xác nhận nó đạt**

Run: `cd WebApp/src/web && npm test -- ngay`
Expected: PASS.

- [ ] **Step 5: Thêm kiểu dữ liệu**

Thêm vào cuối `WebApp/src/web/src/api/types.ts`:

```ts
/** Ánh xạ 1-1 với LoaiCongViecSaoLuu phía máy chủ (Qlgx.Domain/Entities/CongViecSaoLuu.cs). */
export type LoaiCongViecSaoLuu =
  | 'sao_luu' | 'phuc_hoi' | 'kiem_tra' | 'dien_tap' | 'tai_ve' | 'dong_bo_danh_sach'

export type TrangThaiCongViec = 'cho' | 'dang_chay' | 'xong' | 'loi'

/** Ánh xạ 1-1 với TinhTrangSaoLuuDto. `den` là "xanh" | "vang" | "do". */
export type TinhTrangSaoLuu = {
  den: 'xanh' | 'vang' | 'do'
  saoLuuGanNhat: string | null
  soBanSao: number
  dienTapGanNhat: string | null
  dienTapDat: boolean
  loiGanNhat: string | null
}

/** Ánh xạ 1-1 với BanSaoLuuDto. `id` là mã snapshot ngắn của restic. */
export type BanSaoLuu = {
  id: string
  thoiDiem: string
  nhan: string | null
  kichThuocByte: number
  soGiaoDan: number
  soGiaDinh: number
  nguon: 'tu_dong' | 'thu_cong' | 'truoc_cap_nhat' | 'truoc_phuc_hoi'
}

/** Ánh xạ 1-1 với CongViecDto. */
export type CongViecSaoLuu = {
  id: string
  loai: LoaiCongViecSaoLuu
  trangThai: TrangThaiCongViec
  buocHienTai: string | null
  nhatKy: string | null
  taoLuc: string
  batDauLuc: string | null
  ketThucLuc: string | null
}
```

- [ ] **Step 6: Thêm nhánh `saoLuu` vào `client.ts`**

Chèn ngay sau khối `quanTri: { … },`:

```ts
  /** Màn hình "Sao lưu & Phục hồi" (policy "QuanTriHeThong", LoaiTaiKhoan=9). Mọi thao tác ở
   * đây tác động tới TOÀN MÁY CHỦ, không riêng giáo xứ nào — xem
   * docs/superpowers/specs/2026-09-13-qlgx-trien-khai-sao-luu-design.md mục 8.
   *
   * Toàn bộ đều BẤT ĐỒNG BỘ: `taoCongViec` trả về ngay một mã công việc, bộ chạy trên host mới
   * là bên thực thi. Không có endpoint nào chờ pg_dump xong (có thể mất vài phút, vượt timeout
   * của reverse proxy). */
  saoLuu: {
    tinhTrang: () => goi<TinhTrangSaoLuu>('/api/sao-luu/tinh-trang'),
    danhSach: () => goi<BanSaoLuu[]>('/api/sao-luu/danh-sach'),
    taoCongViec: (than: {
      loai: LoaiCongViecSaoLuu; snapshotId?: string | null
      xacNhan?: string | null; nhan?: string | null
    }) => goi<{ id: string }>('/api/sao-luu/cong-viec', {
      method: 'POST', body: JSON.stringify(than),
    }),
    congViec: (id: string) => goi<CongViecSaoLuu>(`/api/sao-luu/cong-viec/${id}`),
    congViecGanDay: () => goi<CongViecSaoLuu[]>('/api/sao-luu/cong-viec'),
    /** Trả về ĐƯỜNG DẪN, không phải nội dung — nơi gọi mở bằng thẻ <a download> để trình duyệt
     * tự tải, tránh nạp cả tệp dump (có thể hàng trăm MB) vào bộ nhớ trang. */
    duongDanTaiVe: (maCongViec: string) => `/api/sao-luu/tai-ve/${maCongViec}`,
  },
```

Thêm các kiểu mới vào câu `import type { … } from './types'` ở đầu tệp.

- [ ] **Step 7: Bổ sung test cho client**

Thêm vào `WebApp/src/web/src/api/client.test.ts`:

```ts
describe('api.saoLuu', () => {
  it('taoCongViec goi dung duong dan va phuong thuc POST', async () => {
    const fetchGia = vi.fn().mockResolvedValue(
      new Response(JSON.stringify({ id: 'abc' }), { status: 200 }))
    vi.stubGlobal('fetch', fetchGia)

    await api.saoLuu.taoCongViec({ loai: 'sao_luu' })

    const [duongDan, tuyChon] = fetchGia.mock.calls[0]!
    expect(String(duongDan)).toContain('/api/sao-luu/cong-viec')
    expect(tuyChon.method).toBe('POST')
  })

  it('duongDanTaiVe gan ma cong viec vao duong dan', () => {
    expect(api.saoLuu.duongDanTaiVe('ma-1')).toBe('/api/sao-luu/tai-ve/ma-1')
  })
})
```

**Lưu ý:** đọc phần đầu `client.test.ts` để dùng đúng cách mock `fetch` mà tệp đó đã thiết lập — đừng đặt cách mock thứ hai chồng lên.

- [ ] **Step 8: Chạy toàn bộ test frontend**

Run: `cd WebApp/src/web && npm test && npm run build`
Expected: PASS toàn bộ, build không lỗi TypeScript.

- [ ] **Step 9: Commit**

```bash
git add WebApp/src/web/src/api/types.ts WebApp/src/web/src/api/client.ts \
        WebApp/src/web/src/api/client.test.ts WebApp/src/web/src/lib/ngay.ts \
        WebApp/src/web/src/lib/ngay.test.ts
git commit -m "Them kieu du lieu, ham goi API sao luu va dinh dang ngay gio dd/MM/yyyy HH:mm"
```

---

## Task 17: Màn hình "Sao lưu & Phục hồi" — trạng thái, danh sách, hành động

**Files:**
- Create: `WebApp/src/web/src/screens/SaoLuuPage.tsx`
- Create: `WebApp/src/web/src/screens/SaoLuuPage.test.tsx`

**Interfaces:**
- Consumes: `api.saoLuu.*` (Task 16); `GxGrid` (`columnDefs`, `rowData`, `layId`, `hangLoc`, `ghiChuChan`); `TrangThaiTai` (`dangTai`, `loi`, `onThuLai`); `dinhDangNgayGio`.
- Produces: `export function SaoLuuPage()`; hàm thuần export để test: `nhanNguon(nguon) → string`, `dinhDangKichThuoc(byte) → string`.

- [ ] **Step 1: Viết test thất bại**

Tạo `WebApp/src/web/src/screens/SaoLuuPage.test.tsx`:

```tsx
import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { SaoLuuPage, nhanNguon, dinhDangKichThuoc } from './SaoLuuPage'
import { api } from '../api/client'

vi.mock('../api/client', () => ({
  api: { saoLuu: {
    tinhTrang: vi.fn(), danhSach: vi.fn(), congViecGanDay: vi.fn(),
    taoCongViec: vi.fn(), congViec: vi.fn(),
    duongDanTaiVe: (id: string) => `/api/sao-luu/tai-ve/${id}`,
  } },
}))

const tinhTrangXanh = {
  den: 'xanh' as const, saoLuuGanNhat: '2026-09-13T06:00:00Z', soBanSao: 47,
  dienTapGanNhat: '2026-09-08T03:00:00Z', dienTapDat: true, loiGanNhat: null,
}
const motBanSao = {
  id: 'ab12cd34', thoiDiem: '2026-09-13T06:00:00Z', nhan: 'tu-dong',
  kichThuocByte: 12_582_912, soGiaoDan: 2050, soGiaDinh: 40, nguon: 'tu_dong' as const,
}

beforeEach(() => {
  vi.mocked(api.saoLuu.tinhTrang).mockResolvedValue(tinhTrangXanh)
  vi.mocked(api.saoLuu.danhSach).mockResolvedValue([motBanSao])
  vi.mocked(api.saoLuu.congViecGanDay).mockResolvedValue([])
})

describe('dinhDangKichThuoc', () => {
  it('doi byte sang don vi doc duoc', () => {
    expect(dinhDangKichThuoc(0)).toBe('0 B')
    expect(dinhDangKichThuoc(12_582_912)).toBe('12,0 MB')
  })
})

describe('nhanNguon', () => {
  it('dich du bon nguon sang tieng Viet', () => {
    expect(nhanNguon('tu_dong')).toBe('Tự động')
    expect(nhanNguon('thu_cong')).toBe('Thủ công')
    expect(nhanNguon('truoc_cap_nhat')).toBe('Trước cập nhật')
    expect(nhanNguon('truoc_phuc_hoi')).toBe('Trước phục hồi')
  })
})

describe('SaoLuuPage', () => {
  it('hien den xanh va so ban sao', async () => {
    render(<SaoLuuPage />)
    expect(await screen.findByText(/Bình thường/)).toBeInTheDocument()
    expect(screen.getByText(/47 bản sao/)).toBeInTheDocument()
  })

  it('hien thoi diem theo dd\/MM\/yyyy HH:mm, KHONG phai ISO', async () => {
    render(<SaoLuuPage />)
    await screen.findByText(/Bình thường/)
    expect(screen.queryByText(/2026-09-13T/)).not.toBeInTheDocument()
    expect(screen.getByText(/13\/09\/2026 \d{2}:\d{2}/)).toBeInTheDocument()
  })

  it('den do hien canh bao noi bat', async () => {
    vi.mocked(api.saoLuu.tinhTrang).mockResolvedValue(
      { ...tinhTrangXanh, den: 'do', loiGanNhat: 'restic check that bai' })
    render(<SaoLuuPage />)
    expect(await screen.findByText(/restic check that bai/)).toBeInTheDocument()
  })

  it('bam "Sao luu ngay" thi tao cong viec loai sao_luu', async () => {
    vi.mocked(api.saoLuu.taoCongViec).mockResolvedValue({ id: 'job-1' })
    vi.mocked(api.saoLuu.congViec).mockResolvedValue({
      id: 'job-1', loai: 'sao_luu', trangThai: 'xong', buocHienTai: null,
      nhatKy: null, taoLuc: '2026-09-13T07:00:00Z', batDauLuc: null, ketThucLuc: null,
    })
    render(<SaoLuuPage />)
    await screen.findByText(/Bình thường/)

    await userEvent.click(screen.getByRole('button', { name: /Sao lưu ngay/ }))

    await waitFor(() => expect(api.saoLuu.taoCongViec)
      .toHaveBeenCalledWith(expect.objectContaining({ loai: 'sao_luu' })))
  })

  it('bao loi ro rang khi tao cong viec that bai, khong im lang', async () => {
    vi.mocked(api.saoLuu.taoCongViec).mockRejectedValue(new Error('Đang có một công việc chạy dở'))
    render(<SaoLuuPage />)
    await screen.findByText(/Bình thường/)

    await userEvent.click(screen.getByRole('button', { name: /Sao lưu ngay/ }))

    expect(await screen.findByText(/Đang có một công việc chạy dở/)).toBeInTheDocument()
  })

  it('canh bao ro rang ngay tai nut tai ve rang tep chua ma hoa', async () => {
    render(<SaoLuuPage />)
    await screen.findByText(/Bình thường/)
    expect(screen.getByText(/chưa mã hoá/i)).toBeInTheDocument()
  })
})
```

- [ ] **Step 2: Chạy test để chắc chắn nó thất bại**

Run: `cd WebApp/src/web && npm test -- SaoLuuPage`
Expected: FAIL — không tìm thấy module `./SaoLuuPage`.

- [ ] **Step 3: Viết `SaoLuuPage.tsx`**

```tsx
import { useCallback, useEffect, useRef, useState } from 'react'
import type { ColDef } from 'ag-grid-community'
import { api } from '../api/client'
import type { BanSaoLuu, CongViecSaoLuu, LoaiCongViecSaoLuu, TinhTrangSaoLuu } from '../api/types'
import { GxGrid } from '../components/GxGrid'
import { TrangThaiTai } from '../components/TrangThaiTai'
import { dinhDangNgayGio } from '../lib/ngay'

const MS_HOI_LAI = 2000

export function dinhDangKichThuoc(byte: number): string {
  if (byte <= 0) return '0 B'
  const donVi = ['B', 'KB', 'MB', 'GB', 'TB']
  const i = Math.min(Math.floor(Math.log(byte) / Math.log(1024)), donVi.length - 1)
  const so = byte / 1024 ** i
  return i === 0 ? `${so} B` : `${so.toFixed(1).replace('.', ',')} ${donVi[i]}`
}

export function nhanNguon(nguon: BanSaoLuu['nguon']): string {
  switch (nguon) {
    case 'thu_cong': return 'Thủ công'
    case 'truoc_cap_nhat': return 'Trước cập nhật'
    case 'truoc_phuc_hoi': return 'Trước phục hồi'
    default: return 'Tự động'
  }
}

/**
 * Màn hình "Hệ thống → Sao lưu & Phục hồi" — CHỈ tài khoản Quản trị hệ thống (LoaiTaiKhoan=9)
 * thấy được, xem SideNav.tsx và policy "QuanTriHeThong" phía máy chủ.
 *
 * Mọi nút đều CHỈ tạo công việc rồi trả về ngay; bộ chạy trên host mới thực thi. Không có nút
 * nào chờ đồng bộ — dump một CSDL có ảnh bytea có thể mất vài phút, vượt timeout của reverse
 * proxy và làm treo một luồng của ứng dụng.
 *
 * Dùng polling 2 giây thay vì WebSocket: một người dùng, vài lần một tháng — thêm hạ tầng
 * realtime là phức tạp không cần thiết, và polling vẫn hoạt động khi container API vừa khởi
 * động lại sau bước hoán đổi CSDL, trong khi WebSocket thì đứt.
 */
export function SaoLuuPage() {
  const [tinhTrang, setTinhTrang] = useState<TinhTrangSaoLuu | null>(null)
  const [banSao, setBanSao] = useState<BanSaoLuu[]>([])
  const [congViec, setCongViec] = useState<CongViecSaoLuu[]>([])
  const [dangTai, setDangTai] = useState(true)
  const [loi, setLoi] = useState<string | null>(null)
  const [loiThaoTac, setLoiThaoTac] = useState<string | null>(null)
  const [dangChay, setDangChay] = useState<CongViecSaoLuu | null>(null)
  const hoLaiRef = useRef<ReturnType<typeof setInterval> | null>(null)

  const tai = useCallback(() => {
    setDangTai(true); setLoi(null)
    Promise.all([api.saoLuu.tinhTrang(), api.saoLuu.danhSach(), api.saoLuu.congViecGanDay()])
      .then(([tt, ds, cv]) => { setTinhTrang(tt); setBanSao(ds); setCongViec(cv) })
      .catch((e: unknown) => setLoi(e instanceof Error ? e.message : String(e)))
      .finally(() => setDangTai(false))
  }, [])

  useEffect(tai, [tai])
  useEffect(() => () => { if (hoLaiRef.current) clearInterval(hoLaiRef.current) }, [])

  function theoDoi(id: string) {
    if (hoLaiRef.current) clearInterval(hoLaiRef.current)
    hoLaiRef.current = setInterval(() => {
      api.saoLuu.congViec(id)
        .then((cv) => {
          setDangChay(cv)
          if (cv.trangThai === 'xong' || cv.trangThai === 'loi') {
            if (hoLaiRef.current) { clearInterval(hoLaiRef.current); hoLaiRef.current = null }
            tai()
          }
        })
        .catch(() => { /* API co the dang khoi dong lai sau khi hoan doi CSDL — cu hoi lai */ })
    }, MS_HOI_LAI)
  }

  async function taoCongViec(loai: LoaiCongViecSaoLuu, them?: Record<string, string>) {
    setLoiThaoTac(null)
    try {
      const { id } = await api.saoLuu.taoCongViec({ loai, ...them })
      setDangChay({
        id, loai, trangThai: 'cho', buocHienTai: null, nhatKy: null,
        taoLuc: new Date().toISOString(), batDauLuc: null, ketThucLuc: null,
      })
      theoDoi(id)
    } catch (e) {
      setLoiThaoTac(e instanceof Error ? e.message : String(e))
    }
  }

  const cot: ColDef<BanSaoLuu>[] = [
    { field: 'thoiDiem', headerName: 'Thời điểm', flex: 1.4,
      valueFormatter: (p) => dinhDangNgayGio(p.value as string) },
    { field: 'nhan', headerName: 'Nhãn', flex: 1 },
    { field: 'kichThuocByte', headerName: 'Kích thước', flex: 0.9,
      valueFormatter: (p) => dinhDangKichThuoc(p.value as number) },
    { field: 'soGiaoDan', headerName: 'Số giáo dân', flex: 0.9 },
    { field: 'soGiaDinh', headerName: 'Số gia đình', flex: 0.9 },
    { field: 'nguon', headerName: 'Nguồn', flex: 1,
      valueFormatter: (p) => nhanNguon(p.value as BanSaoLuu['nguon']) },
  ]

  const dangCoViecChay = dangChay !== null
    && dangChay.trangThai !== 'xong' && dangChay.trangThai !== 'loi'

  return (
    <TrangThaiTai dangTai={dangTai} loi={loi} onThuLai={tai}>
      <div style={{ padding: 16, display: 'flex', flexDirection: 'column', gap: 16 }}>
        <KhoiTinhTrang tinhTrang={tinhTrang} />

        <div style={{ display: 'flex', gap: 8, flexWrap: 'wrap' }}>
          <button type="button" className="btn" disabled={dangCoViecChay}
            onClick={() => taoCongViec('sao_luu')}>Sao lưu ngay</button>
          <button type="button" className="btn" disabled={dangCoViecChay}
            onClick={() => taoCongViec('kiem_tra')}>Kiểm tra tính toàn vẹn</button>
          <button type="button" className="btn" disabled={dangCoViecChay}
            onClick={() => taoCongViec('dien_tap')}>Diễn tập phục hồi</button>
          <button type="button" className="btn" disabled={dangCoViecChay}
            onClick={() => taoCongViec('dong_bo_danh_sach')}>Tải lại</button>
        </div>

        {loiThaoTac && (
          <div role="alert" style={{ color: 'var(--rose-ink)', fontSize: 12.5 }}>{loiThaoTac}</div>
        )}

        {dangChay && <KhoiTienTrinh congViec={dangChay} />}

        <section style={{ display: 'flex', flexDirection: 'column', gap: 6 }}>
          <h3 style={{ margin: 0 }}>
            Danh sách bản sao lưu <span className="pill">{banSao.length} bản sao</span>
          </h3>
          <p style={{ margin: 0, fontSize: 12, color: 'var(--muted, #5b6a86)' }}>
            Tệp tải về là bản dữ liệu <strong>chưa mã hoá</strong>, chứa toàn bộ thông tin giáo
            dân. Chỉ tải về máy tin cậy và xoá ngay sau khi dùng xong.
          </p>
          <div style={{ height: 360 }}>
            <GxGrid<BanSaoLuu> columnDefs={cot} rowData={banSao} layId={(d) => d.id}
              ghiChuChan="Nhấp chuột phải vào một dòng để tải về hoặc phục hồi." />
          </div>
        </section>

        <KhoiNhatKy congViec={congViec} />
      </div>
    </TrangThaiTai>
  )
}

function KhoiTinhTrang({ tinhTrang }: { tinhTrang: TinhTrangSaoLuu | null }) {
  if (!tinhTrang) return null
  const { den } = tinhTrang
  const nhan = den === 'xanh' ? '🟢 Bình thường'
             : den === 'vang' ? '🟡 Chưa có bản sao mới'
             : '🔴 Có vấn đề với sao lưu'
  const mau = den === 'do' ? 'var(--rose-ink)' : undefined
  return (
    <section className="glass" role="status"
      style={{ padding: 16, borderRadius: 'var(--r-card)', display: 'flex',
               flexDirection: 'column', gap: 6 }}>
      <strong style={{ color: mau }}>{nhan}</strong>
      <div style={{ fontSize: 12.5 }}>
        Sao lưu gần nhất: <strong>{dinhDangNgayGio(tinhTrang.saoLuuGanNhat) || 'chưa có'}</strong>
        {' · '}{tinhTrang.soBanSao} bản sao
        {' · '}Diễn tập phục hồi gần nhất:{' '}
        <strong>{dinhDangNgayGio(tinhTrang.dienTapGanNhat) || 'chưa có'}</strong>
        {tinhTrang.dienTapGanNhat && (tinhTrang.dienTapDat ? ' — Đạt' : ' — KHÔNG ĐẠT')}
      </div>
      {tinhTrang.loiGanNhat && (
        <div style={{ color: 'var(--rose-ink)', fontSize: 12.5 }}>{tinhTrang.loiGanNhat}</div>
      )}
    </section>
  )
}

function KhoiTienTrinh({ congViec }: { congViec: CongViecSaoLuu }) {
  return (
    <section className="glass" style={{ padding: 12, borderRadius: 'var(--r-card)' }}>
      <strong>
        {congViec.trangThai === 'loi' ? 'Công việc thất bại'
          : congViec.trangThai === 'xong' ? 'Đã xong'
          : 'Đang chạy — đừng đóng tab'}
      </strong>
      {congViec.buocHienTai && (
        <div style={{ fontSize: 12.5 }}>Bước: {congViec.buocHienTai}</div>
      )}
      {congViec.nhatKy && (
        <details>
          <summary style={{ fontSize: 12.5, cursor: 'pointer' }}>Nhật ký chi tiết</summary>
          <pre style={{ fontSize: 11, whiteSpace: 'pre-wrap', margin: '6px 0 0' }}>
            {congViec.nhatKy}
          </pre>
        </details>
      )}
    </section>
  )
}

function KhoiNhatKy({ congViec }: { congViec: CongViecSaoLuu[] }) {
  if (congViec.length === 0) return null
  return (
    <details>
      <summary style={{ fontSize: 12.5, cursor: 'pointer' }}>
        Nhật ký công việc gần đây ({congViec.length})
      </summary>
      <ul style={{ fontSize: 12, margin: '6px 0 0', paddingLeft: 18 }}>
        {congViec.map((c) => (
          <li key={c.id}>
            {dinhDangNgayGio(c.taoLuc)} — {c.loai} — <strong>{c.trangThai}</strong>
          </li>
        ))}
      </ul>
    </details>
  )
}
```

**Lưu ý:** kiểm xem `GxGrid` được export dạng named hay default và lớp CSS `pill` có tồn tại trong `qlgx.css` không (`GiaoDanList.tsx` dùng chuẩn nào thì theo chuẩn đó) — mục tiêu là giống hệt chuẩn UX của Danh sách giáo dân.

- [ ] **Step 4: Chạy test để xác nhận nó đạt**

Run: `cd WebApp/src/web && npm test -- SaoLuuPage`
Expected: PASS — 8/8.

- [ ] **Step 5: Commit**

```bash
git add WebApp/src/web/src/screens/SaoLuuPage.tsx WebApp/src/web/src/screens/SaoLuuPage.test.tsx
git commit -m "Man hinh Sao luu & Phuc hoi: den trang thai, danh sach ban sao, cac nut hanh dong"
```

---

## Task 18: Luồng phục hồi trên web với bốn lớp rào an toàn

Người dùng đã chọn cho phép **phục hồi đầy đủ trên web**. Toàn bộ rào an toàn tập trung ở đây, và **cố ý gây khó chịu**.

**Files:**
- Create: `WebApp/src/web/src/screens/SaoLuuPhucHoiModal.tsx`
- Create: `WebApp/src/web/src/screens/SaoLuuPhucHoiModal.test.tsx`
- Modify: `WebApp/src/web/src/screens/SaoLuuPage.tsx` (thêm menu chuột phải + mở modal)

**Interfaces:**
- Consumes: `api.saoLuu.taoCongViec`, `api.saoLuu.duongDanTaiVe`, `TinhTrangSaoLuu`, `BanSaoLuu`, `dinhDangNgayGio`.
- Produces:
  - `export const CHUOI_XAC_NHAN_PHUC_HOI = 'PHUC HOI TOAN BO'` — phải khớp **nguyên văn** hằng `SaoLuuService.ChuoiXacNhanPhucHoi` phía máy chủ
  - `export function SaoLuuPhucHoiModal({ banSao, soGiaoDanHienTai, soGiaDinhHienTai, onDong, onXacNhan })`
    - `onXacNhan: (snapshotId: string, xacNhan: string) => void`

- [ ] **Step 1: Viết test thất bại**

Tạo `WebApp/src/web/src/screens/SaoLuuPhucHoiModal.test.tsx`:

```tsx
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { SaoLuuPhucHoiModal, CHUOI_XAC_NHAN_PHUC_HOI } from './SaoLuuPhucHoiModal'

const banSao = {
  id: 'ab12cd34', thoiDiem: '2026-09-13T06:00:00Z', nhan: 'tu-dong',
  kichThuocByte: 1024, soGiaoDan: 2050, soGiaDinh: 40, nguon: 'tu_dong' as const,
}

function dung(props: Partial<Parameters<typeof SaoLuuPhucHoiModal>[0]> = {}) {
  const onXacNhan = vi.fn()
  render(<SaoLuuPhucHoiModal banSao={banSao} soGiaoDanHienTai={2053} soGiaDinhHienTai={40}
    onDong={vi.fn()} onXacNhan={onXacNhan} {...props} />)
  return { onXacNhan }
}

it('chuoi xac nhan khop nguyen van voi phia may chu', () => {
  expect(CHUOI_XAC_NHAN_PHUC_HOI).toBe('PHUC HOI TOAN BO')
})

it('hien bang doi chieu truoc-sau va TO DO dong se giam', () => {
  dung()
  expect(screen.getByText('2053')).toBeInTheDocument()
  expect(screen.getByText('2050')).toBeInTheDocument()
  expect(screen.getByTestId('canh-bao-giam-giao-dan')).toBeInTheDocument()
})

it('KHONG to do khi so lieu khong giam', () => {
  dung({ soGiaoDanHienTai: 2050 })
  expect(screen.queryByTestId('canh-bao-giam-giao-dan')).not.toBeInTheDocument()
})

it('nut xac nhan bi KHOA khi chua go dung chuoi', async () => {
  dung()
  const nut = screen.getByRole('button', { name: /Phục hồi/ })
  expect(nut).toBeDisabled()

  await userEvent.type(screen.getByLabelText(/gõ/i), 'phuc hoi toan bo')
  expect(nut).toBeDisabled()
})

it('nut mo khoa khi go DUNG chuoi, va goi onXacNhan voi dung tham so', async () => {
  const { onXacNhan } = dung()
  await userEvent.type(screen.getByLabelText(/gõ/i), CHUOI_XAC_NHAN_PHUC_HOI)
  const nut = screen.getByRole('button', { name: /Phục hồi/ })
  expect(nut).toBeEnabled()

  await userEvent.click(nut)
  expect(onXacNhan).toHaveBeenCalledWith('ab12cd34', CHUOI_XAC_NHAN_PHUC_HOI)
})

it('noi ro pham vi la TOAN MAY CHU, khong rieng mot giao xu', () => {
  dung()
  expect(screen.getByText(/toàn bộ máy chủ/i)).toBeInTheDocument()
})

it('hien thoi diem ban sao theo dd\/MM\/yyyy HH:mm', () => {
  dung()
  expect(screen.getByText(/13\/09\/2026 \d{2}:\d{2}/)).toBeInTheDocument()
})
```

- [ ] **Step 2: Chạy test để chắc chắn nó thất bại**

Run: `cd WebApp/src/web && npm test -- SaoLuuPhucHoiModal`
Expected: FAIL — không tìm thấy module.

- [ ] **Step 3: Viết modal**

```tsx
import { useState } from 'react'
import type { BanSaoLuu } from '../api/types'
import { dinhDangNgayGio } from '../lib/ngay'

/** Phải khớp NGUYÊN VĂN hằng SaoLuuService.ChuoiXacNhanPhucHoi phía máy chủ. Máy chủ kiểm lại
 * chuỗi này một lần nữa — giao diện chỉ là lớp rào thứ ba, không phải lớp bảo vệ duy nhất. */
export const CHUOI_XAC_NHAN_PHUC_HOI = 'PHUC HOI TOAN BO'

type Props = {
  banSao: BanSaoLuu
  soGiaoDanHienTai: number
  soGiaDinhHienTai: number
  onDong: () => void
  onXacNhan: (snapshotId: string, xacNhan: string) => void
}

/**
 * Bốn lớp rào trước một thao tác KHÔNG THỂ HOÀN TÁC bằng giao diện:
 *   1. Chỉ tài khoản Quản trị hệ thống mở được màn hình này (kiểm ở tầng API, không chỉ ẩn nút)
 *   2. Chọn bản sao có ngữ cảnh — thời điểm và số bản ghi tại thời điểm đó
 *   3. Gõ tay chuỗi xác nhận, không dùng hộp thoại "bạn có chắc không?" (bấm OK theo phản xạ)
 *   4. Bảng đối chiếu trước–sau, tô đỏ những dòng sẽ GIẢM
 *
 * Cố ý gây khó chịu. Dữ liệu là sổ sách giáo xứ nhiều năm, mất là không lấy lại được.
 */
export function SaoLuuPhucHoiModal(
  { banSao, soGiaoDanHienTai, soGiaDinhHienTai, onDong, onXacNhan }: Props,
) {
  const [goVao, setGoVao] = useState('')
  const dungChuoi = goVao === CHUOI_XAC_NHAN_PHUC_HOI
  const giamGiaoDan = banSao.soGiaoDan < soGiaoDanHienTai
  const giamGiaDinh = banSao.soGiaDinh < soGiaDinhHienTai

  return (
    <div role="dialog" aria-modal="true" aria-label="Xác nhận phục hồi dữ liệu"
      style={{ position: 'fixed', inset: 0, background: 'rgba(6,14,32,.45)', display: 'grid',
               placeItems: 'center', zIndex: 50 }}>
      <div className="glass" style={{ padding: 20, borderRadius: 'var(--r-card)', maxWidth: 560,
                                      display: 'flex', flexDirection: 'column', gap: 12 }}>
        <h3 style={{ margin: 0, color: 'var(--rose-ink)' }}>Phục hồi dữ liệu</h3>

        <p style={{ margin: 0, fontSize: 12.5 }}>
          Thao tác này thay thế <strong>toàn bộ máy chủ</strong> — dữ liệu của <em>mọi</em> giáo
          xứ, không riêng giáo xứ nào — bằng nội dung của bản sao lưu lúc{' '}
          <strong>{dinhDangNgayGio(banSao.thoiDiem)}</strong>. Mọi thay đổi nhập sau thời điểm
          đó sẽ mất. Hệ thống tự sao lưu trạng thái hiện tại trước khi ghi đè, và giữ cơ sở dữ
          liệu cũ thêm 7 ngày.
        </p>

        <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: 12.5 }}>
          <thead>
            <tr style={{ textAlign: 'left', borderBottom: '1px solid rgba(14,32,76,.13)' }}>
              <th /><th>Hiện tại</th><th>Sau khi phục hồi</th>
            </tr>
          </thead>
          <tbody>
            <tr {...(giamGiaoDan ? { 'data-testid': 'canh-bao-giam-giao-dan' } : {})}
              style={{ color: giamGiaoDan ? 'var(--rose-ink)' : undefined }}>
              <td>Giáo dân</td><td>{soGiaoDanHienTai}</td><td>{banSao.soGiaoDan}</td>
            </tr>
            <tr {...(giamGiaDinh ? { 'data-testid': 'canh-bao-giam-gia-dinh' } : {})}
              style={{ color: giamGiaDinh ? 'var(--rose-ink)' : undefined }}>
              <td>Gia đình</td><td>{soGiaDinhHienTai}</td><td>{banSao.soGiaDinh}</td>
            </tr>
          </tbody>
        </table>

        <label className="field" style={{ flexDirection: 'column', alignItems: 'stretch' }}>
          Để xác nhận, hãy gõ đúng: <code>{CHUOI_XAC_NHAN_PHUC_HOI}</code>
          <input value={goVao} onChange={(e) => setGoVao(e.target.value)} autoComplete="off" />
        </label>

        <div style={{ display: 'flex', gap: 8, justifyContent: 'flex-end' }}>
          <button type="button" className="btn" onClick={onDong}>Huỷ</button>
          <button type="button" className="btn" disabled={!dungChuoi}
            onClick={() => onXacNhan(banSao.id, goVao)}>
            Phục hồi về {dinhDangNgayGio(banSao.thoiDiem)}
          </button>
        </div>
      </div>
    </div>
  )
}
```

- [ ] **Step 4: Chạy test để xác nhận nó đạt**

Run: `cd WebApp/src/web && npm test -- SaoLuuPhucHoiModal`
Expected: PASS — 7/7.

- [ ] **Step 5: Nối vào `SaoLuuPage`**

Thêm import ở đầu tệp:

```tsx
import { SaoLuuPhucHoiModal } from './SaoLuuPhucHoiModal'
```

Thêm state và menu chuột phải cho lưới:

```tsx
  const [banSaoDangPhucHoi, setBanSaoDangPhucHoi] = useState<BanSaoLuu | null>(null)
  // Ma cong viec "tai_ve" gan nhat — dung de biet khi nao hien duoc lien ket tai tep.
  const [maTaiVe, setMaTaiVe] = useState<string | null>(null)

  async function taiVe(ban: BanSaoLuu) {
    setLoiThaoTac(null)
    try {
      const { id } = await api.saoLuu.taoCongViec({ loai: 'tai_ve', snapshotId: ban.id })
      setDangChay({
        id, loai: 'tai_ve', trangThai: 'cho', buocHienTai: 'Đang chuẩn bị tệp…',
        nhatKy: null, taoLuc: new Date().toISOString(), batDauLuc: null, ketThucLuc: null,
      })
      theoDoi(id)
      setMaTaiVe(id)
    } catch (e) { setLoiThaoTac(e instanceof Error ? e.message : String(e)) }
  }
```

Truyền `menuChuotPhai` vào `GxGrid`:

```tsx
            menuChuotPhai={[
              { nhan: 'Tải bản sao này về máy', chay: (d) => void taiVe(d) },
              { nhan: 'Phục hồi về bản sao này…', chay: (d) => setBanSaoDangPhucHoi(d) },
            ]}
```

Và render modal ở cuối, cùng liên kết tải về khi công việc `tai_ve` xong:

```tsx
        {banSaoDangPhucHoi && (
          <SaoLuuPhucHoiModal
            banSao={banSaoDangPhucHoi}
            soGiaoDanHienTai={banSao[0]?.soGiaoDan ?? 0}
            soGiaDinhHienTai={banSao[0]?.soGiaDinh ?? 0}
            onDong={() => setBanSaoDangPhucHoi(null)}
            onXacNhan={(snapshotId, xacNhan) => {
              setBanSaoDangPhucHoi(null)
              void taoCongViec('phuc_hoi', { snapshotId, xacNhan })
            }}
          />
        )}

        {maTaiVe && dangChay?.id === maTaiVe && dangChay.trangThai === 'xong' && (
          <a className="btn" href={api.saoLuu.duongDanTaiVe(maTaiVe)}>
            Tải tệp đã chuẩn bị xong
          </a>
        )}
```

**Lưu ý về `soGiaoDanHienTai`:** dùng số của bản sao mới nhất là con số gần đúng nhất mà giao diện có sẵn (API chưa có endpoint đếm bản ghi hiện tại). Nếu muốn con số chính xác tuyệt đối, thêm hai trường `soGiaoDanHienTai`/`soGiaDinhHienTai` vào `TinhTrangSaoLuuDto` ở Task 6 và dùng chúng — làm rõ lựa chọn bằng một comment tại chỗ dù chọn cách nào.

- [ ] **Step 6: Chạy toàn bộ test frontend**

Run: `cd WebApp/src/web && npm test && npm run build`
Expected: PASS toàn bộ.

- [ ] **Step 7: Commit**

```bash
git add WebApp/src/web/src/screens/SaoLuuPhucHoiModal.tsx \
        WebApp/src/web/src/screens/SaoLuuPhucHoiModal.test.tsx \
        WebApp/src/web/src/screens/SaoLuuPage.tsx
git commit -m "Luong phuc hoi tren web voi bon lop rao an toan"
```

---

## Task 19: Băng cảnh báo đỏ và đăng ký màn hình vào menu

**Files:**
- Create: `WebApp/src/web/src/components/BangCanhBaoSaoLuu.tsx`
- Create: `WebApp/src/web/src/components/BangCanhBaoSaoLuu.test.tsx`
- Modify: `WebApp/src/web/src/components/ThanhPhanKhung/SideNav.tsx:88-92`
- Modify: `WebApp/src/web/src/App.tsx` (import, hàm `moSaoLuu`, nhánh trong `moTheoDieuHuong`, render băng)
- Modify: `WebApp/src/web/src/components/ThanhPhanKhung/SideNav.test.tsx` (bổ sung)

**Interfaces:**
- Consumes: `api.saoLuu.tinhTrang()`.
- Produces: `export function BangCanhBaoSaoLuu({ laQuanTriHeThong, onMoManHinh })` — tự gọi API, chỉ hiện khi `den === 'do'`, không hiện gì trong lúc tải; mục điều hướng `{ id: 'saoLuu', nhan: 'Sao lưu & Phục hồi' }`.

- [ ] **Step 1: Viết test thất bại**

Tạo `WebApp/src/web/src/components/BangCanhBaoSaoLuu.test.tsx`:

```tsx
import { render, screen, waitFor } from '@testing-library/react'
import { BangCanhBaoSaoLuu } from './BangCanhBaoSaoLuu'
import { api } from '../api/client'

vi.mock('../api/client', () => ({ api: { saoLuu: { tinhTrang: vi.fn() } } }))

const co = (den: 'xanh' | 'vang' | 'do', loi: string | null = null) => ({
  den, saoLuuGanNhat: null, soBanSao: 0, dienTapGanNhat: null, dienTapDat: false, loiGanNhat: loi,
})

it('KHONG goi API khi khong phai quan tri he thong', () => {
  render(<BangCanhBaoSaoLuu laQuanTriHeThong={false} onMoManHinh={vi.fn()} />)
  expect(api.saoLuu.tinhTrang).not.toHaveBeenCalled()
})

it('KHONG hien gi khi den xanh', async () => {
  vi.mocked(api.saoLuu.tinhTrang).mockResolvedValue(co('xanh'))
  const { container } = render(<BangCanhBaoSaoLuu laQuanTriHeThong onMoManHinh={vi.fn()} />)
  await waitFor(() => expect(api.saoLuu.tinhTrang).toHaveBeenCalled())
  expect(container).toBeEmptyDOMElement()
})

it('hien bang do khi den do', async () => {
  vi.mocked(api.saoLuu.tinhTrang).mockResolvedValue(co('do', 'restic check that bai'))
  render(<BangCanhBaoSaoLuu laQuanTriHeThong onMoManHinh={vi.fn()} />)
  expect(await screen.findByRole('alert')).toHaveTextContent(/restic check that bai/)
})

it('im lang khi API loi — khong hien bang bao dong gia', async () => {
  vi.mocked(api.saoLuu.tinhTrang).mockRejectedValue(new Error('mat mang'))
  const { container } = render(<BangCanhBaoSaoLuu laQuanTriHeThong onMoManHinh={vi.fn()} />)
  await waitFor(() => expect(api.saoLuu.tinhTrang).toHaveBeenCalled())
  expect(container).toBeEmptyDOMElement()
})
```

- [ ] **Step 2: Chạy test để chắc chắn nó thất bại**

Run: `cd WebApp/src/web && npm test -- BangCanhBaoSaoLuu`
Expected: FAIL — không tìm thấy module.

- [ ] **Step 3: Viết băng cảnh báo**

```tsx
import { useEffect, useState } from 'react'
import { api } from '../api/client'
import type { TinhTrangSaoLuu } from '../api/types'

/**
 * Băng cảnh báo hiện trên đầu MỌI màn hình cho tài khoản Quản trị hệ thống khi sao lưu đang có
 * vấn đề.
 *
 * Vì sao không để yên trong màn hình "Sao lưu & Phục hồi": kiểu hỏng nguy hiểm nhất của sao lưu
 * là hỏng ÂM THẦM sáu tháng rồi mới lộ ra đúng hôm cần dùng. Không ai chủ động vào kiểm tra một
 * màn hình mà mọi thứ vẫn đang bình thường.
 *
 * Lỗi gọi API thì im lặng: một băng báo động đỏ chỉ vì mạng chập chờn sẽ nhanh chóng bị bỏ qua,
 * và một cảnh báo bị bỏ qua thì tệ hơn không có cảnh báo.
 */
export function BangCanhBaoSaoLuu(
  { laQuanTriHeThong, onMoManHinh }: { laQuanTriHeThong: boolean; onMoManHinh: () => void },
) {
  const [tinhTrang, setTinhTrang] = useState<TinhTrangSaoLuu | null>(null)

  useEffect(() => {
    if (!laQuanTriHeThong) return
    let huy = false
    const tai = () => api.saoLuu.tinhTrang()
      .then((tt) => { if (!huy) setTinhTrang(tt) })
      .catch(() => { if (!huy) setTinhTrang(null) })
    void tai()
    // 5 phút một lần: đủ sớm để không bỏ lỡ cả ngày, đủ thưa để không thêm tải vô ích.
    const dinhKy = setInterval(() => void tai(), 5 * 60 * 1000)
    return () => { huy = true; clearInterval(dinhKy) }
  }, [laQuanTriHeThong])

  if (!tinhTrang || tinhTrang.den !== 'do') return null

  return (
    <div role="alert" style={{ background: 'var(--rose-bg, #fde8ec)', color: 'var(--rose-ink)',
                               padding: '8px 14px', fontSize: 12.5, display: 'flex',
                               gap: 10, alignItems: 'center' }}>
      <strong>⚠ Sao lưu đang có vấn đề.</strong>
      <span>{tinhTrang.loiGanNhat ?? 'Chưa có bản sao lưu nào thành công.'}</span>
      <button type="button" className="btn" onClick={onMoManHinh}>Xem chi tiết</button>
    </div>
  )
}
```

- [ ] **Step 4: Chạy test để xác nhận nó đạt**

Run: `cd WebApp/src/web && npm test -- BangCanhBaoSaoLuu`
Expected: PASS — 4/4.

- [ ] **Step 5: Thêm mục vào `SideNav`**

Trong `WebApp/src/web/src/components/ThanhPhanKhung/SideNav.tsx`, thêm vào mảng `mucHeThong` sau dòng `nhapDuLieu`:

```tsx
    ...(laQuanTriHeThong ? [{ id: 'saoLuu', nhan: 'Sao lưu & Phục hồi' }] : []),
```

Và thêm test vào `SideNav.test.tsx`:

```tsx
it('chi Quan tri he thong thay muc "Sao luu & Phuc hoi"', () => {
  const { rerender } = render(
    <SideNav dangChonId="" onNavigate={vi.fn()} laQuanTri laQuanTriHeThong={false} />)
  expect(screen.queryByText('Sao lưu & Phục hồi')).not.toBeInTheDocument()

  rerender(<SideNav dangChonId="" onNavigate={vi.fn()} laQuanTri laQuanTriHeThong />)
  expect(screen.getByText('Sao lưu & Phục hồi')).toBeInTheDocument()
})
```

- [ ] **Step 6: Nối vào `App.tsx`**

Thêm import, hàm mở tab, nhánh điều hướng, và render băng cảnh báo ngay trên `<main>`:

```tsx
import { SaoLuuPage } from './screens/SaoLuuPage'
import { BangCanhBaoSaoLuu } from './components/BangCanhBaoSaoLuu'

  // "Sao lưu & Phục hồi" (chỉ Quản trị hệ thống) — thay thế đúng chỗ ba mục Nhập/Sao lưu/Khôi
  // phục dữ liệu kiểu desktop hiện đang bị khoá trong menu.
  function moSaoLuu() {
    mo({ id: 'saoLuu', tieuDe: 'Sao lưu & Phục hồi', noiDung: <SaoLuuPage /> })
  }
```

Trong `moTheoDieuHuong`, sau nhánh `nhapDuLieu`:

```tsx
    else if (id === 'saoLuu') moSaoLuu()
```

Và trong phần render sau khi đã đăng nhập, ngay trước `<main>`:

```tsx
        <BangCanhBaoSaoLuu
          laQuanTriHeThong={nguoiDung.loaiTaiKhoan === 9}
          onMoManHinh={moSaoLuu} />
```

**Lưu ý:** kiểm đúng tên trường trên đối tượng `nguoiDung` (`loaiTaiKhoan`?) bằng cách xem chỗ `App.tsx` đang tính `laQuanTriHeThong` để truyền vào `SideNav`, rồi dùng lại đúng biểu thức đó thay vì viết lại.

- [ ] **Step 7: Chạy toàn bộ test frontend và build**

Run: `cd WebApp/src/web && npm test && npm run build && npm run lint`
Expected: PASS toàn bộ.

- [ ] **Step 8: Kiểm chứng bằng trình duyệt thật**

Đăng nhập bằng tài khoản `LoaiTaiKhoan=9`, mở "Hệ thống → Sao lưu & Phục hồi", xác nhận:
- Đèn trạng thái hiện đúng, thời điểm ở dạng `dd/MM/yyyy HH:mm` (không ISO).
- Bấm "Sao lưu ngay" → công việc chạy và chuyển sang "Đã xong" trong vòng ~2 phút.
- Chuột phải một dòng → "Phục hồi về bản sao này…" → nút Phục hồi **bị khoá** cho tới khi gõ đúng `PHUC HOI TOAN BO`.
- Đăng nhập bằng tài khoản thường: **không thấy** mục menu, và gọi thẳng `/api/sao-luu/tinh-trang` bằng `curl` trả **403**.

- [ ] **Step 9: Commit**

```bash
git add WebApp/src/web/src/components/BangCanhBaoSaoLuu.tsx \
        WebApp/src/web/src/components/BangCanhBaoSaoLuu.test.tsx \
        WebApp/src/web/src/components/ThanhPhanKhung/SideNav.tsx \
        WebApp/src/web/src/components/ThanhPhanKhung/SideNav.test.tsx \
        WebApp/src/web/src/App.tsx
git commit -m "Bang canh bao sao luu va dang ky man hinh vao menu He thong"
```

---

# Giai đoạn E — Tài liệu

## Task 20: Bốn tài liệu tiếng Việt cho người vận hành

Người đọc là quý cha, quý sơ — phần lớn không rành máy tính. Viết theo lối "làm bước 1, rồi bước 2", không giải thích kiến trúc.

**Files:**
- Create: `WebApp/docs/CAI-DAT-MAY-CHU.md`
- Create: `WebApp/docs/SAO-LUU-PHUC-HOI.md`
- Create: `WebApp/docs/THE-PHUC-HOI.md`
- Modify: `WebApp/TRIEN-KHAI.md` (mục 5, mục 12, mục 14)

**Interfaces:**
- Consumes: mọi lệnh và tên tệp đã định nghĩa ở Task 8–19. **Mọi lệnh viết trong tài liệu phải được chạy thử thật** trước khi commit — tài liệu sai lệnh còn tệ hơn không có tài liệu.

- [ ] **Step 1: Viết `CAI-DAT-MAY-CHU.md`**

Bố cục bắt buộc:

1. **Cần chuẩn bị gì** — máy chủ (2 vCPU / 4 GB RAM / 40 GB SSD, Ubuntu 24.04), một tên miền đã trỏ về địa chỉ IP máy chủ, một tài khoản Cloudflare.
2. **Tạo kho sao lưu trên Cloudflare R2** — từng bước: tạo bucket, tạo API token với quyền Object Read & Write **chỉ trên bucket đó**, ghi lại Endpoint / Access Key ID / Secret. Kèm khuyến nghị bật versioning và Object Lock, và ghi rõ script **không** tự bật vì cần quyền cao hơn.
3. **Chạy một lệnh cài** — dán đúng lệnh `curl … | sudo bash`, liệt kê các câu hỏi script sẽ hỏi và nên trả lời thế nào.
4. **Việc phải làm NGAY sau khi cài** — in Thẻ phục hồi, cất ngoài máy chủ, rồi `rm /etc/qlgx/the-phuc-hoi.txt`. Viết như một mục riêng có tiêu đề nổi bật.
5. **Kiểm tra hệ thống chạy đúng** — `qlgx status`, mở trang web, đăng nhập, thử in một phiếu gia đình.
6. **Cập nhật về sau** — `qlgx update`; giải thích nó tự sao lưu trước và tự quay lui nếu hỏng.
7. **Khi gặp trục trặc** — bảng "triệu chứng → nguyên nhân thường gặp → cách xử lý" cho: cổng 80/443 đang bận, DNS chưa trỏ nên không lấy được chứng chỉ, hết dung lượng đĩa, R2 từ chối khoá, container api không lên (`qlgx logs api`).

- [ ] **Step 2: Viết `SAO-LUU-PHUC-HOI.md`**

Bố cục bắt buộc:

1. **Hệ thống tự sao lưu khi nào** — 4 lần mỗi ngày (00:00, 06:00, 12:00, 18:00), cộng thêm tự động trước mỗi lần cập nhật và trước mỗi lần phục hồi. Nói rõ: mất tối đa 6 giờ dữ liệu nếu hỏng máy chủ.
2. **Cách đọc đèn trạng thái** — xanh / vàng / đỏ nghĩa là gì và phải làm gì với từng màu.
3. **Sao lưu thủ công và tải bản sao về máy** — kèm cảnh báo tệp tải về **chưa mã hoá**.
4. **Khi nào NÊN phục hồi và khi nào KHÔNG** — nên: lỡ xoá hàng loạt, nhập nhầm dữ liệu vào giáo xứ khác. Không nên: một người bị sai vài trường (sửa tay nhanh hơn và không mất dữ liệu của người khác). Nói thẳng: phục hồi làm mất **mọi** thay đổi của **mọi** giáo xứ sau thời điểm bản sao.
5. **Phục hồi trên web** — từng bước kèm mô tả bốn lớp rào.
6. **Cứu hộ khi mất máy chủ** — danh sách đánh số dùng được trong lúc hoảng loạn: dựng VPS mới → lấy Thẻ phục hồi → tải `qlgx-restore.sh` → chạy **không có** `--apply` để xem kế hoạch → chạy lại **có** `--apply` → trỏ lại DNS → `qlgx status`.
7. **Diễn tập phục hồi** — chạy tự động Chủ nhật hằng tuần, không đụng tới dữ liệu thật; giải thích vì sao nó quan trọng: một bản sao lưu chưa từng được phục hồi thử chỉ là một giả định.

- [ ] **Step 3: Viết `THE-PHUC-HOI.md`**

Nội dung: mẫu Thẻ phục hồi (đúng định dạng `in_the_phuc_hoi` sinh ra), giải thích từng dòng, và cảnh báo thẳng bằng chữ đậm:

> Mất tấm thẻ này thì **không ai** phục hồi được bản sao lưu — kể cả Cloudflare, kể cả người viết phần mềm. Bản sao được mã hoá ngay trên máy chủ của bạn trước khi gửi đi; không có mật khẩu thì dữ liệu chỉ là những khối byte vô nghĩa. Đó là điều khiến bản sao an toàn, và cũng là điều khiến tấm thẻ này không thể thay thế.

Kèm gợi ý cụ thể chỗ cất: in ra giấy để trong két/tủ hồ sơ giáo xứ, hoặc lưu trong trình quản lý mật khẩu. **Không** lưu trên chính máy chủ, **không** gửi qua email.

- [ ] **Step 4: Cập nhật `TRIEN-KHAI.md`**

- Mục 5 (RLS): thay đoạn SQL chép-dán tay bằng câu "script cài đặt tự làm việc này (`scripts/sql/00-vai-tro-rls.sql`), xem `docs/CAI-DAT-MAY-CHU.md`". **Giữ lại** đoạn SQL trong một mục phụ "làm tay khi cần gỡ rối" — có người sẽ cần nó khi hệ thống hỏng.
- Mục 12 (sao lưu): thay dòng `pg_dump` đơn lẻ bằng liên kết tới `docs/SAO-LUU-PHUC-HOI.md`.
- Mục 14 (chưa kiểm chứng): xoá các mục nay đã giải quyết (reverse proxy/TLS, sao lưu, phục hồi), giữ lại những mục vẫn đúng (chưa có Postgres managed, chưa có HA thật).

- [ ] **Step 5: Chạy thử mọi lệnh trong tài liệu**

Với từng khối lệnh trong ba tài liệu mới, chạy đúng nguyên văn trong container e2e và xác nhận nó hoạt động. Sửa tài liệu cho khớp thực tế — **không** sửa cho "trông đẹp".

- [ ] **Step 6: Commit**

```bash
git add WebApp/docs/ WebApp/TRIEN-KHAI.md
git commit -m "Them tai lieu cai dat, sao luu-phuc hoi va The phuc hoi bang tieng Viet"
```

---

# Kiểm tra cuối cùng trước khi đóng

Sau khi hoàn tất Task 20, chạy trọn bộ và xác nhận từng dòng:

- [ ] `dotnet test WebApp/Qlgx.sln` — toàn bộ xanh
- [ ] `cd WebApp/src/web && npm test && npm run build && npm run lint` — toàn bộ xanh
- [ ] `docker run --rm -v "$PWD:/code" bats/bats:1.11.0 /code/WebApp/tests/shell` — toàn bộ xanh
- [ ] `bash WebApp/tests/shell/install-e2e.sh` — ĐẠT
- [ ] `bash WebApp/tests/shell/cap-nhat-e2e.sh` — ĐẠT
- [ ] `bash WebApp/tests/shell/phuc-hoi-e2e.sh` — ĐẠT
- [ ] `bash WebApp/tests/shell/kiem-tra-in-pdf.sh` — ĐẠT
- [ ] Trên hệ thống đã cài: `docker compose exec api env | grep -c AWS_SECRET_ACCESS_KEY` → **0** (ranh giới đặc quyền còn nguyên)
- [ ] Đặt hai chuỗi kết nối trùng nhau ở `ASPNETCORE_ENVIRONMENT=Production` → API **từ chối khởi động**
- [ ] Tài khoản `LoaiTaiKhoan≠9` gọi `/api/sao-luu/*` → **403** cho mọi route
- [ ] Không có mật khẩu, khoá JWT, khoá R2 hay mật khẩu restic nào bị commit: `git log -p --all | grep -iE 'RESTIC_PASSWORD=.|AWS_SECRET_ACCESS_KEY=.|JwtKey.*=.{10}'` → không có kết quả thật
- [ ] `Source/`, `BIN/`, `Release/`, `landing/` **không có thay đổi nào**: `git diff --stat master -- Source BIN Release landing` → rỗng


