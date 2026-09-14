# Giao thức đồng bộ (Kế hoạch 4) — Kế hoạch thi công

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Máy chủ có hai đầu vào để một máy con nói chuyện được — nhận về những thay đổi mới, và gửi lên những thay đổi của mình — với luật gộp mức trường, chống xử lý trùng, và khả năng bù lại dữ liệu khi máy chủ bị lùi về bản sao lưu.

**Architecture:** Kế hoạch 1 đã có nhật ký mức ô (`thay_doi` là sổ kiểm toán, `hieu_luc` là chuỗi phát xuống có số thứ tự liên tục). Kế hoạch này thêm phần *giao thức*: một bảng mốc từng ô (`moc_o`) giữ "ai ghi ô này sau cùng, lúc nào" để quyết định thắng thua mà không phải quét lại `hieu_luc`; một bảng chống trùng (`thao_tac_da_nhan`); đồng hồ lai để so thứ tự giữa các máy; hai đầu vào HTTP; hộp cần xem lại cho xung đột thật; và cơ chế xoay `epoch` + bù dữ liệu sau khôi phục. Không có mã phía trình duyệt trong kế hoạch này — đó là kế hoạch 5.

**Tech Stack:** .NET 10, EF Core + Npgsql, PostgreSQL 17, xUnit + FluentAssertions.

**Spec:** `docs/superpowers/specs/2026-09-13-dong-bo-offline-design.md` — mục 4.3 (đồng hồ), 4.5 (epoch), **4.8 (khôi phục sau restore)**, 5 (ràng buộc bắt buộc), 7.2/7.5 (nhận về / gửi lên), 8 (gộp và xung đột), 9.2 (hộp cần xem lại).

## Global Constraints

- **Nhánh:** `webapp-phase-1`. Có thể có phiên Claude khác làm việc song song trên cùng thư mục — kiểm tra `git branch --show-current` trước khi làm, **không `git checkout`**, `git add` từng file cụ thể, **tuyệt đối không `git add -A`/`git add .`**, sau commit chạy `git show --stat` tự kiểm chứng.
- **Thư mục làm việc:** `WebApp/`. **Không đụng `Source/`** (bản desktop).
- **Máy chủ dev có thể đang chạy ở cổng 5096 — ĐỪNG dừng nó.** Vấp lỗi khoá file thì build/test ra thư mục riêng bằng `-o <thư mục tạm>`.
- **Bộ test cần PostgreSQL thật** qua `QLGX_TEST_PG`, hệ thống đặt sẵn `Host=localhost;Username=postgres;Password=primas` — **cố ý không có `Database=`** vì fixture tự nối thêm tên database ngẫu nhiên. **Đừng thêm `Database=`.**
- **Bẫy migration đã biết:** `Microsoft.EntityFrameworkCore.Design` có `<PrivateAssets>all</PrivateAssets>` trong `Qlgx.Data.csproj` khiến `dotnet ef` báo thiếu gói. Cách xử lý: tạm xoá đúng dòng `<PrivateAssets>all</PrivateAssets>`, chạy lệnh, rồi **hoàn nguyên** và xác nhận `git diff --stat` cho file đó rỗng.
- **Lệnh migration:** `cd WebApp/src/Qlgx.Data && dotnet ef migrations add <Ten> --startup-project ../Qlgx.Api`. **Luôn `--startup-project ../Qlgx.Api`.**
- **Bẫy tên cột `xmin`:** `RowVersion` ánh xạ tới cột hệ thống `xmin` của PostgreSQL. EF có thể sinh `AddColumn<uint>("xmin", type:"xid")` — phải bỏ khỏi migration. Tìm theo chữ `xmin`, không phải `row_version`.
- **Bảng mới có `giao_xu_id` phải được thêm vào CẢ HAI:** `HasQueryFilter` trong `QlgxDbContext.OnModelCreating` **và** migration bật RLS. Bỏ sót một trong hai là rò rỉ dữ liệu giữa các giáo xứ. Ngoại lệ: bảng chỉ đọc/ghi qua kết nối quản trị (như `thiet_bi`, xem spec 6.5).
- **Đặt tên tiếng Việt không dấu** cho lớp/hàm/biến; **chú thích tiếng Việt có dấu** giải thích **vì sao**, khớp mật độ của `QlgxDbContext.cs` và `CapSoHieuLuc.cs`.
- **Không ghi mật khẩu CSDL vào mã nguồn.**
- **Điều 4 của `CLAUDE.md`:** mỗi kiểm thử bảo vệ một ràng buộc sống còn phải được **chứng minh biết báo lỗi** — chạy thử với một bản cố tình hỏng, xác nhận nó FAIL, rồi khôi phục. Các bước dưới ghi rõ chỗ nào bắt buộc.
- **Chia lô mọi thao tác hàng loạt.** Khoá dòng đếm `bo_dem_hieu_luc` xếp hàng **mọi** việc ghi của giáo xứ đó, và `lock_timeout` là 5 giây. Đã có ba tiền lệ được duyệt: `ChuanHoaTheoLo`, `ThayTheTheoLo`, `ChuyenTheoLo`. Chọn đúng kiểu: tập hợp **co lại** sau khi xử lý thì lặp `Take(CoLo)` **không** `Skip`; tập hợp **không đổi kích thước** thì `OrderBy(x => x.Id).Skip(daXet).Take(CoLo)`. **Nhầm kiểu là bỏ sót bản ghi âm thầm** — đã xảy ra hai lần trong kế hoạch 1.

## Giao diện đã có từ kế hoạch 1 (dùng nguyên, không viết lại)

| Thành phần | Chữ ký / nội dung |
|---|---|
| `Qlgx.Data.NhatKy.CapSoHieuLuc` | `LayDaiSo(QlgxDbContext db, Guid giaoXuId, int soLuong, CancellationToken ct) -> Task<(long SoDau, Guid Epoch)>` — **ném `InvalidOperationException` nếu gọi ngoài giao dịch**; khoá dòng đếm `FOR UPDATE` giữ tới lúc commit |
| `Qlgx.Data.NhatKy.QlgxDbContextNhatKyExtensions` | `LuuCoNhatKy(this QlgxDbContext db, CancellationToken ct, IBoiCanhGhiNhatKy? boiCanh = null, Guid? giaoDichIdBenNgoai = null) -> Task<int>`; `GhiDongTuongMinh(QlgxDbContext db, IReadOnlyList<ThayDoi> dong, CancellationToken ct)` |
| `Qlgx.Data.NhatKy.SinhDongNhatKy` | `Tu(ChangeTracker theoDoi, IBoiCanhGhiNhatKy? boiCanh, DateTimeOffset moc, Guid giaoDichId) -> List<ThayDoi>` — bỏ qua `EntityState.Deleted` **có chủ ý** |
| `Qlgx.Data.NhatKy.PhanLoaiThucThe` | `DuocGhi` (22 tên lớp CLR), `KhongGhi` (`"TaiKhoan"`), `DuocGhiNhatKy(string tenBang)` |
| `Qlgx.Data.NhatKy.CotLoaiTru` | `BiLoai(string tenCot)` — ảnh `byte[]`, cột hệ thống, ba cột bí mật của `TaiKhoan` |
| `Qlgx.Domain.Entities.ThayDoi` | `Id, GiaoXuId, Bang, BanGhiId, Truong, GiaTri (string? jsonb), Loai, DongHoVatLy, DongHoLogic, ThietBiId, TaiKhoanId, MaThaoTac, GiaoDichId, Thang` |
| `Qlgx.Domain.Entities.HieuLuc` | `Id, GiaoXuId, SoThuTu, Epoch, Bang, BanGhiId, Truong, GiaTri, DongHoVatLy, DongHoLogic, ThietBiId, GiaoDichId` |
| `Qlgx.Domain.Entities.BoDemHieuLuc` | `GiaoXuId` (khoá chính), `SoTiepTheo`, `Epoch` |

---

## Cấu trúc file

| File | Trách nhiệm |
|---|---|
| `src/Qlgx.Domain/Entities/MocO.cs` | Mốc từng ô: ai ghi sau cùng, lúc nào — nguồn sự thật để quyết thắng thua |
| `src/Qlgx.Domain/Entities/ThaoTacDaNhan.cs` | Sổ chống xử lý trùng, giữ cả kết quả để trả lại nguyên văn |
| `src/Qlgx.Domain/Entities/CanXemLai.cs` | Hộp cần xem lại — xung đột người phải quyết |
| `src/Qlgx.Data/Configurations/*Config.cs` | Ánh xạ ba bảng trên |
| `src/Qlgx.Data/DongBo/DongHoLai.cs` | Đồng hồ lai: so sánh, phá hoà, hiệu chỉnh độ lệch |
| `src/Qlgx.Data/DongBo/LuatGop.cs` | Quyết định thắng/thua cho một ô, và ô nào là "nhạy cảm" |
| `src/Qlgx.Data/DongBo/ApThaoTac.cs` | Áp một thao tác đã thắng vào bảng nghiệp vụ thật |
| `src/Qlgx.Data/DongBo/KiemBatBien.cs` | Bộ kiểm bất biến sau khi gộp |
| `src/Qlgx.Api/Dtos/DongBoDtos.cs` | DTO của hai đầu vào |
| `src/Qlgx.Api/Services/DongBoService.cs` | Nhận về / gửi lên / tải toàn bộ |
| `src/Qlgx.Api/Services/KhoiPhucDongBoService.cs` | Xoay `epoch`, bù dữ liệu sau khôi phục (spec 4.8) |
| `src/Qlgx.Api/Endpoints/DongBoEndpoints.cs` | Định tuyến |
| `tests/Qlgx.Data.Tests/DongHoLaiTests.cs` | Thứ tự, phá hoà, hiệu chỉnh |
| `tests/Qlgx.Data.Tests/LuatGopTests.cs` | Bảng quyết định mục 8.1 |
| `tests/Qlgx.Api.Tests/DongBoNhanVeTests.cs` | Con trỏ, epoch, 410, chia lô |
| `tests/Qlgx.Api.Tests/DongBoGuiLenTests.cs` | Idempotency, gộp, piggyback |
| `tests/Qlgx.Api.Tests/KhoiPhucDongBoTests.cs` | Toàn bộ kịch bản mục 4.8 |

---

## Task 1: Bảng mốc từng ô và sổ chống trùng

**Files:**
- Create: `src/Qlgx.Domain/Entities/MocO.cs`, `src/Qlgx.Domain/Entities/ThaoTacDaNhan.cs`
- Create: `src/Qlgx.Data/Configurations/MocOConfig.cs`, `ThaoTacDaNhanConfig.cs`
- Modify: `src/Qlgx.Data/QlgxDbContext.cs`
- Create: migration `ThemBangGiaoThucDongBo`
- Modify: `tests/Qlgx.Data.Tests/RlsTests.cs`

**Interfaces:**
- Produces: `MocO { Guid GiaoXuId; string Bang; Guid BanGhiId; string Truong; DateTimeOffset DongHoVatLy; long DongHoLogic; Guid? ThietBiId; Guid MaThaoTac; }` khoá chính tổ hợp bốn cột đầu. `ThaoTacDaNhan { Guid GiaoXuId; Guid MaThaoTac; Guid? NguonGocEpoch; long? NguonGocSoThuTu; string KetQua; string? PhanHoi; DateTimeOffset NhanLuc; }`

- [ ] **Step 1: Viết hai thực thể**

`src/Qlgx.Domain/Entities/MocO.cs`:

```csharp
namespace Qlgx.Domain.Entities;

/// <summary>
/// Mốc của TỪNG Ô: ai đã ghi ô này sau cùng, vào lúc nào theo đồng hồ lai.
///
/// Không có bảng này thì mỗi lần một máy con gửi lên một ô, máy chủ phải quét ngược
/// <c>hieu_luc</c> để tìm dòng gần nhất của đúng ô đó — với một giáo xứ chạy vài năm thì đó là
/// quét hàng trăm nghìn dòng cho MỘT lần sửa. Bảng này là bản tóm tắt "trạng thái hiện hành của
/// cuộc đua", đọc một dòng là đủ quyết thắng thua.
///
/// Chỉ giữ MỐC, không giữ giá trị: giá trị thật nằm ở chính bảng nghiệp vụ. Trộn hai thứ vào đây
/// sẽ tạo ra nguồn sự thật thứ hai và sớm muộn hai bên lệch nhau.
/// </summary>
public class MocO
{
    public Guid GiaoXuId { get; set; }
    public string Bang { get; set; } = "";
    public Guid BanGhiId { get; set; }
    public string Truong { get; set; } = "";

    public DateTimeOffset DongHoVatLy { get; set; }
    public long DongHoLogic { get; set; }

    /// <summary>Máy nào ghi — tham gia luật phá hoà khi hai mốc bằng nhau.</summary>
    public Guid? ThietBiId { get; set; }
    /// <summary>Mã thao tác — thành phần cuối cùng của luật phá hoà, bảo đảm thứ tự tất định.</summary>
    public Guid MaThaoTac { get; set; }
}
```

`src/Qlgx.Domain/Entities/ThaoTacDaNhan.cs`:

```csharp
namespace Qlgx.Domain.Entities;

/// <summary>
/// Sổ những thao tác máy chủ ĐÃ xử lý, kèm kết quả đã trả về.
///
/// Mạng chập chờn thì máy con gửi lại cả lô là chuyện chắc chắn xảy ra. Không có sổ này, một lần
/// gửi lại sẽ tạo bản ghi thứ hai cho cùng một người, hoặc sinh mục "cần xem lại" trùng — và
/// người dùng bấm "hai người khác nhau" hai lần thì thành BA bản ghi cho một người.
///
/// Ghi kết quả cho MỌI thao tác kể cả thao tác bị TỪ CHỐI: nếu chỉ ghi khi thành công thì lần
/// gửi lại của một thao tác bị từ chối vẫn chạy lại từ đầu, đúng cái bẫy vừa nói.
///
/// <see cref="NguonGocEpoch"/>/<see cref="NguonGocSoThuTu"/> chỉ có giá trị với thao tác BÙ LẠI
/// sau khi máy chủ bị lùi về bản sao lưu (thiết kế mục 4.8): nhiều máy con cùng giữ một dòng đã
/// mất sẽ cùng gửi lại, và chống trùng lúc đó phải theo DANH TÍNH GỐC của dòng chứ không theo
/// mã thao tác (mỗi máy sinh một mã khác nhau cho cùng một dòng gốc).
/// </summary>
public class ThaoTacDaNhan
{
    public Guid GiaoXuId { get; set; }
    public Guid MaThaoTac { get; set; }

    public Guid? NguonGocEpoch { get; set; }
    public long? NguonGocSoThuTu { get; set; }

    /// <summary>"ap" | "thua" | "tu_choi" | "trung"</summary>
    public string KetQua { get; set; } = "";
    /// <summary>Phản hồi đã trả về cho máy con, dạng JSON — trả lại nguyên văn khi gặp lại.</summary>
    public string? PhanHoi { get; set; }

    public DateTimeOffset NhanLuc { get; set; }
}
```

- [ ] **Step 2: Viết cấu hình ánh xạ**

`src/Qlgx.Data/Configurations/MocOConfig.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Qlgx.Domain.Entities;

namespace Qlgx.Data.Configurations;

public class MocOConfig : IEntityTypeConfiguration<MocO>
{
    public void Configure(EntityTypeBuilder<MocO> b)
    {
        // Khoá chính tổ hợp đúng bằng "địa chỉ của một ô". Không thêm cột Id thay thế: bảng này
        // luôn được tra bằng đúng bốn cột này, một khoá thay thế chỉ tạo thêm một chỉ mục thừa.
        b.HasKey(x => new { x.GiaoXuId, x.Bang, x.BanGhiId, x.Truong });
        b.Property(x => x.Bang).HasMaxLength(64).IsRequired();
        b.Property(x => x.Truong).HasMaxLength(64).IsRequired();
    }
}
```

`src/Qlgx.Data/Configurations/ThaoTacDaNhanConfig.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Qlgx.Domain.Entities;

namespace Qlgx.Data.Configurations;

public class ThaoTacDaNhanConfig : IEntityTypeConfiguration<ThaoTacDaNhan>
{
    public void Configure(EntityTypeBuilder<ThaoTacDaNhan> b)
    {
        b.HasKey(x => new { x.GiaoXuId, x.MaThaoTac });
        b.Property(x => x.KetQua).HasMaxLength(16).IsRequired();
        b.Property(x => x.PhanHoi).HasColumnType("jsonb");

        // Chống trùng cho thao tác BÙ LẠI: nhiều máy con cùng gửi lại một dòng gốc, mỗi máy một
        // MaThaoTac khác nhau — chỉ danh tính gốc mới nhận ra chúng là một. Chỉ mục lọc để các
        // thao tác thường (không phải bù lại) không chiếm chỗ.
        b.HasIndex(x => new { x.GiaoXuId, x.NguonGocEpoch, x.NguonGocSoThuTu })
            .IsUnique()
            .HasFilter("nguon_goc_epoch IS NOT NULL");
    }
}
```

- [ ] **Step 3: Thêm DbSet và bộ lọc giáo xứ**

Trong `QlgxDbContext.cs`, thêm cạnh `HieuLuc`:

```csharp
    public DbSet<MocO> MocO => Set<MocO>();
    public DbSet<ThaoTacDaNhan> ThaoTacDaNhan => Set<ThaoTacDaNhan>();
```

Và trong `OnModelCreating`, thêm vào khối `HasQueryFilter`:

```csharp
        b.Entity<MocO>().HasQueryFilter(x => BoiCanhGiaoXuId == null || x.GiaoXuId == BoiCanhGiaoXuId);
        b.Entity<ThaoTacDaNhan>().HasQueryFilter(x => BoiCanhGiaoXuId == null || x.GiaoXuId == BoiCanhGiaoXuId);
```

- [ ] **Step 4: Migration kèm RLS**

```bash
cd WebApp/src/Qlgx.Data
dotnet ef migrations add ThemBangGiaoThucDongBo --startup-project ../Qlgx.Api
```

Thêm vào **cuối** `Up` của migration vừa sinh:

```csharp
            // Lớp phòng thủ thứ hai, y hệt các bảng nghiệp vụ. moc_o không chứa giá trị nhưng
            // chứa BẢN ĐỒ dữ liệu (bảng nào, bản ghi nào, ô nào tồn tại và đổi lúc nào) — rò rỉ
            // nó cho giáo xứ khác vẫn là rò rỉ.
            foreach (var bang in new[] { "moc_o", "thao_tac_da_nhan" })
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
            foreach (var bang in new[] { "moc_o", "thao_tac_da_nhan" })
            {
                migrationBuilder.Sql($"DROP POLICY IF EXISTS loc_theo_giao_xu ON {bang};");
                migrationBuilder.Sql($"ALTER TABLE {bang} DISABLE ROW LEVEL SECURITY;");
            }
```

Kiểm tra migration **không** có `AddColumn` nào tên `xmin`.

- [ ] **Step 5: Mở rộng test RLS cho hai bảng mới**

Trong `tests/Qlgx.Data.Tests/RlsTests.cs`, mở rộng fact `Bang_nhat_ky_chiu_rls_nhu_bang_nghiep_vu` (hoặc thêm fact mới cùng khuôn) để `GRANT` và kiểm chứng cả `moc_o` — dùng đúng khuôn vai trò `NOSUPERUSER NOBYPASSRLS` đã có trong file.

Run: `dotnet test WebApp/tests/Qlgx.Data.Tests --filter Rls`
Expected: PASS.

- [ ] **Step 6: Commit**

```bash
git add WebApp/src/Qlgx.Domain/Entities/MocO.cs \
        WebApp/src/Qlgx.Domain/Entities/ThaoTacDaNhan.cs \
        WebApp/src/Qlgx.Data/Configurations/MocOConfig.cs \
        WebApp/src/Qlgx.Data/Configurations/ThaoTacDaNhanConfig.cs \
        WebApp/src/Qlgx.Data/QlgxDbContext.cs \
        WebApp/src/Qlgx.Data/Migrations/ \
        WebApp/tests/Qlgx.Data.Tests/RlsTests.cs
git commit -m "Them bang moc_o va thao_tac_da_nhan kem RLS"
```

---

## Task 2: Đồng hồ lai

**Files:**
- Create: `src/Qlgx.Data/DongBo/DongHoLai.cs`
- Test: `tests/Qlgx.Data.Tests/DongHoLaiTests.cs`

**Interfaces:**
- Produces: `readonly record struct DauDongHo(DateTimeOffset VatLy, long Logic, Guid? ThietBiId, Guid MaThaoTac)` với `CompareTo`; `DongHoLai.SoSanh(DauDongHo a, DauDongHo b) -> int`; `DongHoLai.HieuChinh(DateTimeOffset mocMayCon, TimeSpan doLech) -> DateTimeOffset`; `DongHoLai.TinhDoLech(DateTimeOffset gioMayCon, DateTimeOffset gioMayChu) -> TimeSpan`; `DongHoLai.NangDau(DauDongHo dauCuoiCuaTa, DauDongHo? nhanDuoc, DateTimeOffset gioHienTai) -> (DateTimeOffset VatLy, long Logic)`; `DongHoLai.CatMicroGiay(DateTimeOffset moc) -> DateTimeOffset`; `DongHoLai.SoSanhGuid(Guid? a, Guid? b) -> int`.

- [ ] **Step 1: Viết test trước — chạy để thấy fail**

`tests/Qlgx.Data.Tests/DongHoLaiTests.cs`:

```csharp
using FluentAssertions;
using Qlgx.Data.DongBo;

namespace Qlgx.Data.Tests;

public class DongHoLaiTests
{
    private static readonly DateTimeOffset Moc = new(2026, 9, 13, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Moc_vat_ly_moi_hon_thi_thang()
    {
        var cu = new DauDongHo(Moc, 0, null, Guid.NewGuid());
        var moi = new DauDongHo(Moc.AddSeconds(1), 0, null, Guid.NewGuid());

        DongHoLai.SoSanh(moi, cu).Should().BePositive();
    }

    [Fact]
    public void Cung_moc_vat_ly_thi_dong_ho_logic_pha_hoa()
    {
        var a = new DauDongHo(Moc, 5, null, Guid.NewGuid());
        var b = new DauDongHo(Moc, 7, null, Guid.NewGuid());

        DongHoLai.SoSanh(b, a).Should().BePositive();
    }

    [Fact]
    public void Cung_moc_va_cung_logic_thi_thiet_bi_roi_ma_thao_tac_pha_hoa_tat_dinh()
    {
        var thietBiNho = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var thietBiLon = Guid.Parse("00000000-0000-0000-0000-000000000002");
        var a = new DauDongHo(Moc, 0, thietBiNho, Guid.NewGuid());
        var b = new DauDongHo(Moc, 0, thietBiLon, Guid.NewGuid());

        // Không quan trọng bên nào thắng — quan trọng là MỌI máy đều kết luận GIỐNG NHAU và
        // kết quả không đổi giữa hai lần so.
        var lan1 = DongHoLai.SoSanh(a, b);
        var lan2 = DongHoLai.SoSanh(a, b);
        lan1.Should().Be(lan2);
        lan1.Should().NotBe(0, "hai dau dong ho khac nhau khong duoc coi la bang nhau");
    }

    [Fact]
    public void Hai_dau_giong_het_nhau_thi_bang_nhau()
    {
        var ma = Guid.NewGuid();
        var tb = Guid.NewGuid();
        DongHoLai.SoSanh(new DauDongHo(Moc, 3, tb, ma), new DauDongHo(Moc, 3, tb, ma)).Should().Be(0);
    }

    [Fact]
    public void Hieu_chinh_dich_moc_may_con_ve_gio_may_chu()
    {
        // Máy con chạy nhanh 3 ngày. Một thao tác nó ghi lúc "16/9 10:00" theo đồng hồ của nó
        // thực ra xảy ra lúc 13/9 10:00 theo giờ máy chủ.
        var gioMayCon = Moc.AddDays(3);
        var doLech = DongHoLai.TinhDoLech(gioMayCon, Moc);

        DongHoLai.HieuChinh(gioMayCon, doLech).Should().BeCloseTo(Moc, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Nang_dau_khi_nhan_moc_o_tuong_lai_thi_bam_theo_moc_do_va_dem_logic_len()
    {
        // Nhận một dấu có mốc vật lý ở tương lai gần so với đồng hồ máy chủ: mốc phát ra phải
        // BÁM THEO mốc nhận được (không lùi về giờ máy), và phần logic nâng lên để lần ghi kế
        // tiếp của chính máy chủ xếp SAU dấu vừa nhận, không hoà.
        var dauCuoiCuaTa = new DauDongHo(Moc, 0, null, Guid.Empty);
        var nhan = new DauDongHo(Moc.AddSeconds(5), 4, null, Guid.NewGuid());

        var phat = DongHoLai.NangDau(dauCuoiCuaTa, nhan, gioHienTai: Moc);

        phat.VatLy.Should().Be(Moc.AddSeconds(5));
        phat.Logic.Should().Be(5, "phai lon hon 4 de xep sau dau vua nhan");
    }

    [Fact]
    public void Gio_hien_tai_da_vuot_qua_moi_moc_thi_logic_ve_khong()
    {
        var dauCuoiCuaTa = new DauDongHo(Moc, 9, null, Guid.Empty);
        var nhan = new DauDongHo(Moc, 9, null, Guid.NewGuid());

        var phat = DongHoLai.NangDau(dauCuoiCuaTa, nhan, gioHienTai: Moc.AddMinutes(1));

        phat.VatLy.Should().Be(Moc.AddMinutes(1));
        phat.Logic.Should().Be(0, "dong ho vat ly da di toi truoc, khong can dem logic nua");
    }

    [Fact]
    public void Nang_dau_khong_nhan_gi_van_khong_duoc_lui_sau_moc_cuoi_cua_chinh_minh()
    {
        // Đồng hồ máy chủ bị chỉnh LÙI (NTP kéo về, hoặc admin sửa tay). Mốc phát ra vẫn phải
        // tiến — nếu không, hai lần ghi liên tiếp của chính máy chủ sẽ đảo thứ tự.
        var dauCuoiCuaTa = new DauDongHo(Moc, 3, null, Guid.Empty);

        var phat = DongHoLai.NangDau(dauCuoiCuaTa, nhanDuoc: null,
            gioHienTai: Moc.AddSeconds(-30));

        phat.VatLy.Should().Be(Moc);
        phat.Logic.Should().Be(4);
    }

    [Fact]
    public void Nang_dau_tai_bien_bang_nhau_van_phai_tien_mot_buoc()
    {
        var dauCuoiCuaTa = new DauDongHo(Moc, 2, null, Guid.Empty);

        var phat = DongHoLai.NangDau(dauCuoiCuaTa, nhanDuoc: null, gioHienTai: Moc);

        phat.VatLy.Should().Be(Moc);
        phat.Logic.Should().Be(3, "bang nhau khong phai la di toi truoc");
    }

    [Fact]
    public void So_sanh_thiet_bi_dung_dang_chuoi_thuong_chu_khong_phai_Guid_CompareTo()
    {
        // Ràng buộc liên ngôn ngữ. ĐÃ ĐO (300.000 cặp ngẫu nhiên): Guid.CompareFo của .NET,
        // memcmp của Postgres trên byte canonical, và so chuỗi của TypeScript đều KHỚP nhau.
        // Cách duy nhất lệch là so MẢNG BYTE THÔ (Guid.ToByteArray / Uint8Array) — lệch
        // 149.958/300.000 cặp, vì .NET đảo little-endian ba trường đầu trong mảng byte.
        // Hàm SoSanhGuid tồn tại để chốt một thứ tự có tên và có test, chống việc ai đó thay
        // bằng so mảng byte với lý do "tương đương mà nhanh hơn". Xem test kế bên.
        var a = Guid.Parse("00000002-0000-0000-0000-000000000000");
        var b = Guid.Parse("00000100-0000-0000-0000-000000000000");

        Math.Sign(DongHoLai.SoSanhGuid(a, b))
            .Should().Be(-1, "dang chuoi thi 00000002... di truoc 00000100...");
    }

    [Fact]
    public void Thiet_bi_null_xep_truoc_moi_thiet_bi_co_danh_tinh()
    {
        DongHoLai.SoSanhGuid(null, Guid.Empty).Should().BeNegative();
        DongHoLai.SoSanhGuid(Guid.Empty, null).Should().BePositive();
        DongHoLai.SoSanhGuid(null, null).Should().Be(0);
    }

    [Fact]
    public void So_sanh_bang_khong_thi_hai_dau_phai_that_su_bang_nhau()
    {
        // Bất biến MocO dựa vào: SoSanh(a,b)==0 <=> a.Equals(b). Quy null về Guid.Empty phá
        // bất biến này.
        var a = new DauDongHo(Moc, 0, null, Guid.Empty);
        var b = new DauDongHo(Moc, 0, Guid.Empty, Guid.Empty);

        a.Should().NotBe(b);
        DongHoLai.SoSanh(a, b).Should().NotBe(0);
    }

    [Fact]
    public void Ma_thao_tac_la_tang_pha_hoa_cuoi_cung_va_no_phai_duoc_dung()
    {
        var tb = Guid.NewGuid();
        var nho = Guid.Parse("00000000-0000-0000-0000-00000000000a");
        var lon = Guid.Parse("00000000-0000-0000-0000-0000000000b0");

        DongHoLai.SoSanh(new DauDongHo(Moc, 0, tb, nho), new DauDongHo(Moc, 0, tb, lon))
            .Should().BeNegative("hoa het ba tang tren, chi con MaThaoTac phan xu");
    }

    [Fact]
    public void So_sanh_doi_xung_nghich_tren_moi_cap()
    {
        var tb1 = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var tb2 = Guid.Parse("00000000-0000-0000-0000-000000000002");
        var m1 = Guid.Parse("00000000-0000-0000-0000-0000000000a1");
        var m2 = Guid.Parse("00000000-0000-0000-0000-0000000000a2");
        var mau = new[]
        {
            new DauDongHo(Moc, 0, null, m1),
            new DauDongHo(Moc, 0, tb1, m1),
            new DauDongHo(Moc, 0, tb1, m2),
            new DauDongHo(Moc, 0, tb2, m1),
            new DauDongHo(Moc, 1, tb1, m1),
            new DauDongHo(Moc.AddSeconds(1), 0, tb1, m1),
        };

        foreach (var a in mau)
        foreach (var b in mau)
        {
            Math.Sign(DongHoLai.SoSanh(a, b))
                .Should().Be(-Math.Sign(DongHoLai.SoSanh(b, a)));
            (DongHoLai.SoSanh(a, b) == 0).Should().Be(a.Equals(b));
        }
    }

    [Fact]
    public void Cat_micro_giay_bo_phan_le_duoi_micro()
    {
        // timestamptz cua Postgres chi giu toi micro giay; DateTimeOffset giu toi 100ns. Neu
        // khong cat NGAY luc dung dau, cai so trong bo nho va cai so sau khi doc lai tu CSDL se
        // khac nhau -> hoa gia, hai may ket luan khac nhau.
        var tho = new DateTimeOffset(2026, 9, 13, 10, 0, 0, TimeSpan.Zero).AddTicks(1237);

        DongHoLai.CatMicroGiay(tho).Ticks.Should().Be(
            new DateTimeOffset(2026, 9, 13, 10, 0, 0, TimeSpan.Zero).AddTicks(1230).Ticks);
    }

    [Fact]
    public void CompareTo_cua_DauDongHo_khop_voi_SoSanh()
    {
        var a = new DauDongHo(Moc, 0, null, Guid.NewGuid());
        var b = new DauDongHo(Moc.AddSeconds(1), 0, null, Guid.NewGuid());

        a.CompareTo(b).Should().Be(DongHoLai.SoSanh(a, b));
    }
}
```

Run: `dotnet test WebApp/tests/Qlgx.Data.Tests --filter DongHoLaiTests`
Expected: FAIL — `DongHoLai` không tồn tại.

- [ ] **Step 2: Viết `DongHoLai`**

`src/Qlgx.Data/DongBo/DongHoLai.cs`:

```csharp
namespace Qlgx.Data.DongBo;

/// <summary>
/// Dấu thời gian của một thao tác, đủ để mọi máy xếp thứ tự GIỐNG NHAU.
/// Bốn thành phần theo đúng thứ tự ưu tiên khi so.
/// </summary>
public readonly record struct DauDongHo(
    DateTimeOffset VatLy, long Logic, Guid? ThietBiId, Guid MaThaoTac)
    : IComparable<DauDongHo>
{
    /// <summary>
    /// Cắt về micro giây NGAY TẠI ĐÂY, không phải ở nơi dùng — đây là chỗ duy nhất cưỡng chế được.
    /// Nếu để `NangDau` cắt rồi mới lấy max, kết quả có thể NHỎ HƠN chính đầu vào chưa cắt tới 9
    /// tick (đo thật: 111.563/200.000 ca). `VatLy` là khoá so hàng đầu, nên dấu mới xếp TRƯỚC dấu
    /// nó vừa thấy, và lần đồng bộ sau bản cũ đè ngược lên bản đã hợp nhất.
    /// </summary>
    public DateTimeOffset VatLy { get; init; } = DongHoLai.CatMicroGiay(VatLy);

    public int CompareTo(DauDongHo khac) => DongHoLai.SoSanh(this, khac);
}

/// <summary>
/// Đồng hồ lai (hybrid logical clock) — phần vật lý cho người đọc hiểu được, phần logic để phá
/// hoà và giữ đúng quan hệ nhân quả.
///
/// Vì sao không dùng thẳng đồng hồ máy: máy ở giáo xứ chạy nhiều năm không ai chỉnh giờ. Nếu lấy
/// giờ máy con làm căn cứ "ai mới hơn", một máy lệch ba ngày sẽ LUÔN thắng và âm thầm đè lên dữ
/// liệu đúng. Nên mọi mốc từ máy con đều được hiệu chỉnh về giờ máy chủ trước khi so.
///
/// Vì sao cần phần logic: chỉ đồng hồ vật lý (dù đã hiệu chỉnh) vẫn đảo được quan hệ nhân quả —
/// máy B ĐỌC thay đổi của máy A rồi sửa đè, nhưng đồng hồ B chỉ số nhỏ hơn nên sửa-sau lại thua
/// sửa-trước, người dùng thấy việc mình vừa làm bị hoàn tác. Mỗi máy nâng đồng hồ logic của mình
/// lên khi nhận một dấu lớn hơn, nên "đã thấy rồi mới sửa" luôn xếp sau.
/// </summary>
public static class DongHoLai
{
    /// <summary>
    /// So hai dấu. Dương nghĩa là <paramref name="a"/> mới hơn.
    ///
    /// Ba mức phá hoà sau mốc vật lý là BẮT BUỘC: thiếu chúng thì hai thay đổi cùng mốc sẽ được
    /// hai máy con kết luận khác nhau, và hai bản sao phân kỳ vĩnh viễn mà không ai báo lỗi.
    /// </summary>
    public static int SoSanh(DauDongHo a, DauDongHo b)
    {
        var theoVatLy = a.VatLy.CompareTo(b.VatLy);
        if (theoVatLy != 0) return theoVatLy;

        var theoLogic = a.Logic.CompareTo(b.Logic);
        if (theoLogic != 0) return theoLogic;

        // null nghĩa là "ghi từ chính máy chủ" — xếp TRƯỚC mọi thiết bị có danh tính. KHÔNG quy
        // null về Guid.Empty: làm vậy thì hai dấu mà record `Equals` nói là khác nhau lại so ra
        // bằng nhau, phá bất biến `SoSanh == 0 <=> Equals` mà MocO dựa vào để biết "đã thấy mốc
        // này chưa".
        var theoThietBi = SoSanhGuid(a.ThietBiId, b.ThietBiId);
        if (theoThietBi != 0) return theoThietBi;

        return SoSanhGuid(a.MaThaoTac, b.MaThaoTac);
    }

    /// <summary>
    /// Thứ tự Guid DUY NHẤT của hệ thống — dạng chuỗi "D" chữ thường, so ordinal.
    ///
    /// Ba cách so KHỚP nhau và một cách LỆCH — biết rõ cái nào là cái nào mới tránh được bẫy:
    ///
    /// - <c>Guid.CompareTo</c> của .NET: KHỚP. Nó so theo TRƯỜNG và ép <c>uint</c>, nên đồng
    ///   nhất với chuỗi, kể cả ở biên bit dấu (7fffffff / 80000000). Đo thật: 0/300.000 cặp lệch.
    /// - Postgres so <c>uuid</c> bằng memcmp trên byte CANONICAL (big-endian): KHỚP.
    /// - TypeScript so chuỗi: KHỚP (đây chính là dạng máy con lưu trong IndexedDB).
    /// - **So MẢNG BYTE THÔ** (<c>Guid.ToByteArray()</c>, hoặc <c>Uint8Array</c> dựng tương
    ///   đương trong JS): **LỆCH ~50%** — đo thật 149.958/300.000 cặp. .NET lưu ba trường đầu
    ///   little-endian trong mảng byte, còn dạng chuỗi in chúng big-endian.
    ///
    /// Vậy hàm này KHÔNG tồn tại để sửa <c>Guid.CompareTo</c> (cái đó vốn đúng), mà để chốt MỘT
    /// thứ tự tường minh, có tên, có test — để không ai thay nó bằng so mảng byte với lý do
    /// "tương đương mà nhanh hơn". Lệch thứ tự phá hoà = hai bản sao phân kỳ vĩnh viễn, không
    /// một lỗi nào hiện ra, và chỉ lộ sau nhiều tháng khi đã hết cách biết bên nào đúng.
    /// Ràng buộc này áp cho cả bản TypeScript ở kế hoạch 5.
    /// </summary>
    public static int SoSanhGuid(Guid? a, Guid? b)
    {
        if (a is null) return b is null ? 0 : -1;
        if (b is null) return 1;
        return string.CompareOrdinal(a.Value.ToString("D"), b.Value.ToString("D"));
    }

    /// <summary>Độ lệch = giờ máy con trừ giờ máy chủ. Dương nghĩa là máy con chạy nhanh.</summary>
    public static TimeSpan TinhDoLech(DateTimeOffset gioMayCon, DateTimeOffset gioMayChu)
        => gioMayCon - gioMayChu;

    /// <summary>Dịch một mốc do máy con ghi về hệ quy chiếu của máy chủ.</summary>
    public static DateTimeOffset HieuChinh(DateTimeOffset mocMayCon, TimeSpan doLech)
        => mocMayCon - doLech;

    /// <summary>Cắt mốc về micro giây — độ phân giải của <c>timestamptz</c> Postgres.</summary>
    public static DateTimeOffset CatMicroGiay(DateTimeOffset moc)
        => moc.AddTicks(-(moc.Ticks % 10));

    /// <summary>
    /// Sinh mốc kế tiếp mà máy này sẽ PHÁT RA, sau khi (tuỳ chọn) nhận một dấu từ nơi khác.
    /// Đây là quy tắc HLC đầy đủ; nó cần cả ba đầu vào và không thể thiếu cái nào:
    ///
    /// - <paramref name="dauCuoiCuaTa"/>: mốc gần nhất chính máy này đã phát. Thiếu nó thì đồng
    ///   hồ máy bị chỉnh LÙI (NTP kéo về, admin sửa tay) sẽ làm hai lần ghi liên tiếp của cùng
    ///   một máy đảo thứ tự.
    /// - <paramref name="nhanDuoc"/>: dấu vừa nhận, hoặc null nếu đây là lần phát nội bộ. Nâng
    ///   theo nó là cách duy nhất giữ nhân quả: "đã thấy rồi mới sửa" phải xếp SAU. Hiệu chỉnh
    ///   độ lệch vật lý KHÔNG thay được việc này — độ lệch đo qua mạng luôn sai vài trăm ms tới
    ///   vài giây, đủ để bản sửa của quý sơ thua chính bản ghi mà nó dựa vào.
    /// - <paramref name="gioHienTai"/>: để phần logic có đường về 0 khi thời gian thật đã vượt
    ///   qua mọi mốc, nếu không nó chỉ tăng mãi.
    ///
    /// Máy chủ giữ <c>dauCuoiCuaTa</c> ở hai cột <c>dau_cuoi_vat_ly</c> / <c>dau_cuoi_logic</c>
    /// trên dòng <c>bo_dem_hieu_luc</c> của giáo xứ — đúng dòng mà mọi đường ghi sinh
    /// <c>so_thu_tu</c> đã giành khoá qua <c>CapSoHieuLuc.LayDaiSo</c>, nên không cần khoá mới.
    /// </summary>
    public static (DateTimeOffset VatLy, long Logic) NangDau(
        DauDongHo dauCuoiCuaTa, DauDongHo? nhanDuoc, DateTimeOffset gioHienTai)
    {
        var bayGio = CatMicroGiay(gioHienTai);
        var cuaTa = CatMicroGiay(dauCuoiCuaTa.VatLy);
        var cuaHo = nhanDuoc is { } n ? CatMicroGiay(n.VatLy) : DateTimeOffset.MinValue;

        var vatLy = bayGio;
        if (cuaTa > vatLy) vatLy = cuaTa;
        if (cuaHo > vatLy) vatLy = cuaHo;

        // Đồng hồ vật lý đã đi tới trước cả hai mốc: đếm logic hết việc, về 0.
        if (vatLy == bayGio && bayGio > cuaTa && bayGio > cuaHo) return (vatLy, 0);

        long logic = 0;
        if (cuaTa == vatLy) logic = dauCuoiCuaTa.Logic;
        if (nhanDuoc is { } m && cuaHo == vatLy) logic = Math.Max(logic, m.Logic);
        return (vatLy, logic + 1);
    }
}
```

- [ ] **Step 3: Chạy test**

Run: `dotnet test WebApp/tests/Qlgx.Data.Tests --filter DongHoLaiTests`
Expected: PASS (7 test).

- [ ] **Step 4: Chứng minh test phá hoà biết báo lỗi**

Đây là ràng buộc sống còn (thiếu phá hoà = hai bản sao phân kỳ vĩnh viễn). Tạm sửa `SoSanh` bỏ hai mức phá hoà cuối:

```csharp
        // BẢN CỐ TÌNH HỎNG — chỉ để chứng minh test biết báo lỗi, XOÁ ngay sau khi xác nhận
        return a.Logic.CompareTo(b.Logic);
```

Run: `dotnet test WebApp/tests/Qlgx.Data.Tests --filter Cung_moc_va_cung_logic`
Expected: **FAIL** — hai dấu khác nhau bị coi là bằng nhau.

Hoàn nguyên, chạy lại, xác nhận PASS.

- [ ] **Step 5: Commit**

```bash
git add WebApp/src/Qlgx.Data/DongBo/DongHoLai.cs WebApp/tests/Qlgx.Data.Tests/DongHoLaiTests.cs
git commit -m "Them dong ho lai: so sanh co pha hoa tat dinh va hieu chinh do lech"
```

---

## Task 3: Luật gộp mức trường

**Files:**
- Create: `src/Qlgx.Data/DongBo/LuatGop.cs`
- Test: `tests/Qlgx.Data.Tests/LuatGopTests.cs`

**Interfaces:**
- Consumes: `DauDongHo`, `DongHoLai.SoSanh` (Task 2); `MocO` (Task 1).
- Produces: `enum KetQuaGop { Thang, Thua, ThangCanXemLai }`; `LuatGop.Quyet(MocO? mocDangCo, DauDongHo dauMoi, string bang, string truong, string? giaTriCu, string? giaTriMoi) -> KetQuaGop`; `LuatGop.LaONhayCam(string bang, string truong) -> bool`; `LuatGop.NhomGop(string bang, string truong) -> string` (đơn vị gộp, spec 8.2).

- [ ] **Step 1: Viết test trước — chạy để thấy fail**

`tests/Qlgx.Data.Tests/LuatGopTests.cs`:

```csharp
using FluentAssertions;
using Qlgx.Data.DongBo;
using Qlgx.Domain.Entities;

namespace Qlgx.Data.Tests;

public class LuatGopTests
{
    private static readonly DateTimeOffset Moc = new(2026, 9, 13, 10, 0, 0, TimeSpan.Zero);

    private static MocO MocCu(DateTimeOffset luc) => new()
    {
        GiaoXuId = Guid.NewGuid(), Bang = "GiaoDan", BanGhiId = Guid.NewGuid(), Truong = "DienThoai",
        DongHoVatLy = luc, DongHoLogic = 0, ThietBiId = Guid.NewGuid(), MaThaoTac = Guid.NewGuid(),
    };

    [Fact]
    public void O_chua_ai_dung_toi_thi_thang()
    {
        var ketQua = LuatGop.Quyet(null, new DauDongHo(Moc, 0, null, Guid.NewGuid()),
            "GiaoDan", "DienThoai", null, "\"0900\"");

        ketQua.Should().Be(KetQuaGop.Thang);
    }

    [Fact]
    public void Dau_cu_hon_moc_dang_co_thi_thua()
    {
        var ketQua = LuatGop.Quyet(MocCu(Moc), new DauDongHo(Moc.AddMinutes(-5), 0, null, Guid.NewGuid()),
            "GiaoDan", "DienThoai", "\"0900\"", "\"0911\"");

        ketQua.Should().Be(KetQuaGop.Thua);
    }

    [Fact]
    public void O_khong_nhay_cam_moi_hon_thi_thang_im_lang()
    {
        var ketQua = LuatGop.Quyet(MocCu(Moc), new DauDongHo(Moc.AddMinutes(5), 0, null, Guid.NewGuid()),
            "GiaoDan", "DienThoai", "\"0900\"", "\"0911\"");

        ketQua.Should().Be(KetQuaGop.Thang, "so dien thoai thuoc nhom moi hon thi dung hon");
    }

    [Fact]
    public void O_nhay_cam_hai_gia_tri_khac_nhau_thi_thang_nhung_vao_hop_xem_lai()
    {
        var ketQua = LuatGop.Quyet(MocCu(Moc), new DauDongHo(Moc.AddMinutes(5), 0, null, Guid.NewGuid()),
            "GiaoDan", "NgaySinh", "\"1985-03-12\"", "\"1986-03-12\"");

        ketQua.Should().Be(KetQuaGop.ThangCanXemLai,
            "ngay sinh la so sach — may khong duoc tu quyet ma khong bao ai");
    }

    [Fact]
    public void O_nhay_cam_nhung_cung_mot_gia_tri_thi_khong_phai_xung_dot()
    {
        var ketQua = LuatGop.Quyet(MocCu(Moc), new DauDongHo(Moc.AddMinutes(5), 0, null, Guid.NewGuid()),
            "GiaoDan", "NgaySinh", "\"1985-03-12\"", "\"1985-03-12\"");

        ketQua.Should().Be(KetQuaGop.Thang, "hai nguoi ghi cung mot gia tri thi khong co gi de hoi");
    }

    [Fact]
    public void O_rong_la_mot_gia_tri_binh_thuong_khong_co_luat_dien_cho_trong()
    {
        // Cha xoá trắng một ngày qua đời nhập nhầm. Máy khác còn giữ giá trị cũ nhưng CŨ HƠN.
        // Nếu có luật "lấy bên có giá trị" thì giá trị sai sống lại vĩnh viễn.
        var ketQua = LuatGop.Quyet(MocCu(Moc), new DauDongHo(Moc.AddMinutes(5), 0, null, Guid.NewGuid()),
            "GiaoDan", "NgayQuaDoi", "\"2020-01-01\"", null);

        ketQua.Should().NotBe(KetQuaGop.Thua, "xoa trang phai xoa duoc, khong duoc bi coi la 'khong co gi'");
    }

    [Fact]
    public void Don_vi_gop_gom_hai_o_phai_nhat_quan_vao_mot_nhom()
    {
        LuatGop.NhomGop("GiaoDan", "QuaDoi").Should().Be(LuatGop.NhomGop("GiaoDan", "NgayQuaDoi"),
            "danh dau qua doi va ngay qua doi phai gop nhu MOT o, neu khong se ra nguoi con song " +
            "ma co ngay qua doi");
    }

    [Fact]
    public void O_khong_thuoc_nhom_nao_thi_tu_no_la_mot_nhom()
    {
        LuatGop.NhomGop("GiaoDan", "DienThoai").Should().NotBe(LuatGop.NhomGop("GiaoDan", "DiaChi"));
    }
}
```

Run: `dotnet test WebApp/tests/Qlgx.Data.Tests --filter LuatGopTests`
Expected: FAIL — `LuatGop` không tồn tại.

- [ ] **Step 2: Viết `LuatGop`**

`src/Qlgx.Data/DongBo/LuatGop.cs`:

```csharp
using Qlgx.Domain.Entities;

namespace Qlgx.Data.DongBo;

public enum KetQuaGop
{
    /// <summary>Áp vào dữ liệu, không làm phiền ai.</summary>
    Thang,
    /// <summary>Có cái mới hơn rồi — chỉ ghi vào sổ kiểm toán, không phát xuống.</summary>
    Thua,
    /// <summary>Áp vào dữ liệu NHƯNG đưa vào hộp cần xem lại để người kiểm lại sau.</summary>
    ThangCanXemLai,
}

/// <summary>
/// Quyết định thắng thua cho MỘT ô, theo bảng quyết định ở thiết kế mục 8.1.
///
/// Nguyên tắc nền: hệ thống KHÔNG BAO GIỜ đứng chờ người dùng trả lời. Kể cả khi có xung đột
/// thật, nó vẫn chọn giá trị mới hơn để phần mềm chạy tiếp, và hộp cần xem lại chỉ là lời mời
/// kiểm lại sau. Bắt giải quyết xung đột mới dùng tiếp được thì quý sơ sẽ bấm bừa cho xong —
/// kết quả tệ hơn hẳn.
/// </summary>
public static class LuatGop
{
    /// <summary>
    /// Những ô mà "mới hơn thì đúng hơn" KHÔNG chắc đúng, nên phải để người kiểm lại.
    ///
    /// Tiêu chí phân nhóm: số điện thoại, địa chỉ, ghi chú đổi là vì đời sống thay đổi — bản mới
    /// gần như luôn đúng. Còn ngày sinh, ngày bí tích, họ tên là SỰ KIỆN ĐÃ XẢY RA, không thay
    /// đổi theo thời gian; hai người ghi khác nhau nghĩa là một trong hai đọc sai sổ, và máy
    /// không có cách nào biết ai đúng.
    ///
    /// Đây là bảng cấu hình, sửa được mà không phải sửa logic.
    /// </summary>
    private static readonly HashSet<string> ONhayCam = new(StringComparer.Ordinal)
    {
        "GiaoDan.HoTen", "GiaoDan.TenThanh", "GiaoDan.NgaySinh", "GiaoDan.Phai",
        "GiaoDan.NgayRuaToi", "GiaoDan.NgayRuocLe", "GiaoDan.NgayThemSuc",
        "GiaoDan.NgayQuaDoi", "GiaoDan.QuaDoi",
        "GiaDinh.TenGiaDinh",
        "ThanhVienGiaDinh.VaiTro", "ThanhVienGiaDinh.ChuHo",
        "HonPhoi.NgayHonPhoi",
        "BiTichChiTiet.DotBiTichId",
    };

    /// <summary>
    /// Đơn vị gộp: những ô PHẢI nhất quán với nhau thì gộp như một ô duy nhất.
    ///
    /// Nếu để chúng gộp độc lập: máy A đánh dấu qua đời kèm ngày 12/9; máy B (mới hơn) bỏ dấu
    /// qua đời nhưng không đụng ô ngày. Gộp xong ra một người CÒN SỐNG MÀ CÓ NGÀY QUA ĐỜI —
    /// lọt hay không lọt thống kê tuỳ chỗ nào đọc ô nào.
    /// </summary>
    private static readonly Dictionary<string, string> NhomCuaO = new(StringComparer.Ordinal)
    {
        ["GiaoDan.QuaDoi"] = "GiaoDan#quadoi",
        ["GiaoDan.NgayQuaDoi"] = "GiaoDan#quadoi",
        ["GiaoDan.NoiQuaDoi"] = "GiaoDan#quadoi",
    };

    public static bool LaONhayCam(string bang, string truong) => ONhayCam.Contains($"{bang}.{truong}");

    /// <summary>
    /// Tên nhóm gộp của một ô. Ô không thuộc nhóm nào thì chính nó là một nhóm — nhờ vậy chỗ gọi
    /// không phải phân biệt hai trường hợp.
    /// </summary>
    public static string NhomGop(string bang, string truong)
        => NhomCuaO.TryGetValue($"{bang}.{truong}", out var nhom) ? nhom : $"{bang}.{truong}";

    public static KetQuaGop Quyet(
        MocO? mocDangCo, DauDongHo dauMoi, string bang, string truong,
        string? giaTriCu, string? giaTriMoi)
    {
        // Chưa ai đụng tới ô này — không có gì để tranh.
        if (mocDangCo is null) return KetQuaGop.Thang;

        var dauCu = new DauDongHo(
            mocDangCo.DongHoVatLy, mocDangCo.DongHoLogic, mocDangCo.ThietBiId, mocDangCo.MaThaoTac);

        if (DongHoLai.SoSanh(dauMoi, dauCu) <= 0) return KetQuaGop.Thua;

        // Mới hơn. Hai người ghi cùng một giá trị thì không có gì để hỏi, dù ô có nhạy cảm.
        // Lưu ý: null (ô rỗng) là MỘT GIÁ TRỊ bình thường, không phải "không có gì" — nên so
        // bằng string.Equals chứ không kiểm null rồi bỏ qua.
        if (string.Equals(giaTriCu, giaTriMoi, StringComparison.Ordinal)) return KetQuaGop.Thang;

        return LaONhayCam(bang, truong) ? KetQuaGop.ThangCanXemLai : KetQuaGop.Thang;
    }
}
```

- [ ] **Step 3: Chạy test**

Run: `dotnet test WebApp/tests/Qlgx.Data.Tests --filter LuatGopTests`
Expected: PASS (8 test).

- [ ] **Step 4: Chứng minh test "ô rỗng" biết báo lỗi**

Đây là ràng buộc đã bị bắt lỗi ngay từ bản thiết kế đầu (luật "điền chỗ trống" phá hội tụ). Tạm thêm vào `Quyet`, ngay trước dòng `return LaONhayCam(...)`:

```csharp
        // BẢN CỐ TÌNH HỎNG — luật "một bên trống thì lấy bên có giá trị". XOÁ sau khi xác nhận.
        if (giaTriMoi is null && giaTriCu is not null) return KetQuaGop.Thua;
```

Run: `dotnet test WebApp/tests/Qlgx.Data.Tests --filter O_rong_la_mot_gia_tri`
Expected: **FAIL**.

Hoàn nguyên, chạy lại, xác nhận PASS.

- [ ] **Step 5: Commit**

```bash
git add WebApp/src/Qlgx.Data/DongBo/LuatGop.cs WebApp/tests/Qlgx.Data.Tests/LuatGopTests.cs
git commit -m "Them luat gop muc truong: bang quyet dinh, o nhay cam, don vi gop"
```

---

## Task 4: Áp một thao tác vào bảng nghiệp vụ

**Files:**
- Create: `src/Qlgx.Data/DongBo/ApThaoTac.cs`
- Test: `tests/Qlgx.Data.Tests/ApThaoTacTests.cs`

**Interfaces:**
- Consumes: `PhanLoaiThucThe`, `CotLoaiTru` (kế hoạch 1).
- Produces: `ApThaoTac.ApMotO(QlgxDbContext db, string bang, Guid banGhiId, string truong, string? giaTriJson, CancellationToken ct) -> Task<string?>` trả về giá trị **cũ** dạng JSON (để so trong luật gộp và ghi vào hộp xem lại); `ApThaoTac.TaoBanGhi(QlgxDbContext db, string bang, Guid banGhiId, Guid giaoXuId, string giaTriJson, CancellationToken ct) -> Task`; `ApThaoTac.DocO(QlgxDbContext db, string bang, Guid banGhiId, string truong, CancellationToken ct) -> Task<string?>`.

**Điểm khó của task này:** áp một thao tác đến từ máy con nghĩa là đặt giá trị cho **một cột được gọi tên bằng chuỗi**, trên **một bảng được gọi tên bằng chuỗi**. Dùng `EntityEntry.Property(string)` của EF Core chứ **không** dùng reflection thô — EF kiểm tra tên cột có thật, biết kiểu CLR để chuyển đổi JSON, và đi đúng đường theo dõi thay đổi.

**Cạm bẫy phải tránh:** đường này **không** được đi qua `LuuCoNhatKy`. Nhật ký cho các thao tác đồng bộ được ghi **tường minh** với đồng hồ của MÁY CON (xem Task 6), còn `LuuCoNhatKy` sẽ sinh thêm một bộ dòng nữa mang đồng hồ của máy chủ — thành hai dòng cho một thay đổi, và dòng sai đồng hồ sẽ thắng ở lần gộp sau. Vì vậy `DongBoService` phải nằm trong danh sách miễn trừ của kiểm thử kiến trúc, kèm chú thích nêu đúng lý do này.

- [ ] **Step 1: Viết test trước — chạy để thấy fail**

`tests/Qlgx.Data.Tests/ApThaoTacTests.cs`:

```csharp
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Qlgx.Data.DongBo;
using Qlgx.Domain.Entities;

namespace Qlgx.Data.Tests;

public class ApThaoTacTests(CoSoDuLieuFixture db) : IClassFixture<CoSoDuLieuFixture>
{
    private async Task<Guid> TaoGiaoDan(int maCu, string hoTen)
    {
        await using var ctx = db.TaoContext();
        var g = new GiaoDan { GiaoXuId = db.GiaoXuId, MaGiaoDanCu = maCu, HoTen = hoTen };
        ctx.GiaoDan.Add(g);
        await ctx.SaveChangesAsync();
        return g.Id;
    }

    [Fact]
    public async Task Ap_mot_o_doi_dung_gia_tri_va_tra_ve_gia_tri_cu()
    {
        var id = await TaoGiaoDan(9701, "Ten Cu");

        await using var ctx = db.TaoContext();
        var cu = await ApThaoTac.ApMotO(ctx, "GiaoDan", id, "HoTen", "\"Ten Moi\"", default);
        await ctx.SaveChangesAsync();

        cu.Should().Be("\"Ten Cu\"");
        await using var doc = db.TaoContext();
        (await doc.GiaoDan.SingleAsync(x => x.Id == id)).HoTen.Should().Be("Ten Moi");
    }

    [Fact]
    public async Task Ap_gia_tri_null_xoa_trang_duoc_o()
    {
        var id = await TaoGiaoDan(9702, "Co so dien thoai");
        await using (var ctx = db.TaoContext())
        {
            var g = await ctx.GiaoDan.SingleAsync(x => x.Id == id);
            g.DienThoai = "0900000000";
            await ctx.SaveChangesAsync();
        }

        await using var ctx2 = db.TaoContext();
        await ApThaoTac.ApMotO(ctx2, "GiaoDan", id, "DienThoai", null, default);
        await ctx2.SaveChangesAsync();

        await using var doc = db.TaoContext();
        (await doc.GiaoDan.SingleAsync(x => x.Id == id)).DienThoai.Should().BeNull();
    }

    [Fact]
    public async Task Ap_o_kieu_ngay_chuyen_doi_dung_tu_json()
    {
        var id = await TaoGiaoDan(9703, "Kiem kieu ngay");

        await using var ctx = db.TaoContext();
        await ApThaoTac.ApMotO(ctx, "GiaoDan", id, "NgaySinh", "\"1985-03-12\"", default);
        await ctx.SaveChangesAsync();

        await using var doc = db.TaoContext();
        (await doc.GiaoDan.SingleAsync(x => x.Id == id)).NgaySinh
            .Should().Be(new DateOnly(1985, 3, 12));
    }

    [Fact]
    public async Task Tu_choi_bang_khong_duoc_phep_ghi_nhat_ky()
    {
        var hanhDong = async () =>
        {
            await using var ctx = db.TaoContext();
            await ApThaoTac.ApMotO(ctx, "TaiKhoan", Guid.NewGuid(), "MatKhauBam", "\"x\"", default);
        };

        await hanhDong.Should().ThrowAsync<InvalidOperationException>(
            "TaiKhoan nam ngoai dong bo — mot may con khong duoc phep dat mat khau qua duong nay");
    }

    [Fact]
    public async Task Tu_choi_cot_nam_trong_danh_sach_loai_tru()
    {
        var id = await TaoGiaoDan(9704, "Co anh");

        var hanhDong = async () =>
        {
            await using var ctx = db.TaoContext();
            await ApThaoTac.ApMotO(ctx, "GiaoDan", id, "AnhDaiDienDuLieu", "\"AAAA\"", default);
        };

        await hanhDong.Should().ThrowAsync<InvalidOperationException>(
            "anh khong di qua nhat ky nen cung khong duoc di qua duong ap thao tac");
    }

    [Fact]
    public async Task Tu_choi_ten_cot_khong_ton_tai()
    {
        var id = await TaoGiaoDan(9705, "Cot la");

        var hanhDong = async () =>
        {
            await using var ctx = db.TaoContext();
            await ApThaoTac.ApMotO(ctx, "GiaoDan", id, "CotKhongCoThat", "\"x\"", default);
        };

        await hanhDong.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Tao_ban_ghi_moi_tu_json_toan_bo()
    {
        var id = Guid.NewGuid();
        var json = JsonSerializer.Serialize(new Dictionary<string, object?>
        {
            ["Id"] = id,
            ["GiaoXuId"] = db.GiaoXuId,
            ["MaGiaoDanCu"] = 9706,
            ["HoTen"] = "Nguoi Tu May Con",
            ["NgaySinh"] = "1990-01-01",
        });

        await using var ctx = db.TaoContext();
        await ApThaoTac.TaoBanGhi(ctx, "GiaoDan", id, db.GiaoXuId, json, default);
        await ctx.SaveChangesAsync();

        await using var doc = db.TaoContext();
        var g = await doc.GiaoDan.SingleAsync(x => x.Id == id);
        g.HoTen.Should().Be("Nguoi Tu May Con");
        g.NgaySinh.Should().Be(new DateOnly(1990, 1, 1));
    }
}
```

Run: `dotnet test WebApp/tests/Qlgx.Data.Tests --filter ApThaoTacTests`
Expected: FAIL — `ApThaoTac` không tồn tại.

- [ ] **Step 2: Viết `ApThaoTac`**

`src/Qlgx.Data/DongBo/ApThaoTac.cs`:

```csharp
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Qlgx.Data.NhatKy;
using Qlgx.Domain.Entities;

namespace Qlgx.Data.DongBo;

/// <summary>
/// Áp một thao tác đến từ máy con vào bảng nghiệp vụ thật.
///
/// Thao tác đến dưới dạng "bảng X, bản ghi Y, ô Z, giá trị JSON" — nghĩa là tên bảng và tên cột
/// đều là CHUỖI lúc chạy. Dùng <c>EntityEntry.Property(string)</c> của EF Core chứ không dùng
/// reflection thô: EF kiểm tra tên cột có thật (sai tên là ném ngay chứ không âm thầm bỏ qua),
/// biết kiểu CLR để chuyển đổi JSON, và đi đúng đường theo dõi thay đổi nên phần còn lại của hệ
/// thống không phải biết dữ liệu này đến từ đâu.
///
/// CẢNH BÁO cho người sửa sau: đường này KHÔNG được gọi <c>LuuCoNhatKy</c>. Nhật ký cho thao tác
/// đồng bộ được ghi tường minh với đồng hồ của MÁY CON; <c>LuuCoNhatKy</c> sẽ sinh thêm một bộ
/// dòng nữa mang đồng hồ MÁY CHỦ, thành hai dòng cho một thay đổi và dòng sai đồng hồ sẽ thắng ở
/// lần gộp sau.
/// </summary>
public static class ApThaoTac
{
    private static Microsoft.EntityFrameworkCore.Metadata.IEntityType LayKieu(
        QlgxDbContext db, string bang)
    {
        if (!PhanLoaiThucThe.DuocGhiNhatKy(bang))
            throw new InvalidOperationException(
                $"Bang '{bang}' khong nam trong danh sach duoc dong bo (PhanLoaiThucThe.DuocGhi).");

        var kieu = db.Model.GetEntityTypes().FirstOrDefault(t => t.ClrType.Name == bang)
            ?? throw new InvalidOperationException($"Khong tim thay thuc the '{bang}'.");
        return kieu;
    }

    private static void KiemCot(string bang, string truong)
    {
        if (CotLoaiTru.BiLoai(truong))
            throw new InvalidOperationException(
                $"Cot '{bang}.{truong}' nam trong CotLoaiTru — khong di qua nhat ky nen cung khong " +
                "duoc di qua duong dong bo.");
    }

    private static async Task<object> LayThucThe(
        QlgxDbContext db, string bang, Guid banGhiId, CancellationToken ct)
    {
        var kieu = LayKieu(db, bang);
        var thucThe = await db.FindAsync(kieu.ClrType, [banGhiId], ct)
            ?? throw new InvalidOperationException(
                $"Khong tim thay ban ghi {banGhiId} trong bang '{bang}'.");
        return thucThe;
    }

    /// <summary>Đọc giá trị hiện tại của một ô, dạng JSON — dùng để so trong luật gộp.</summary>
    public static async Task<string?> DocO(
        QlgxDbContext db, string bang, Guid banGhiId, string truong, CancellationToken ct)
    {
        KiemCot(bang, truong);
        var thucThe = await LayThucThe(db, bang, banGhiId, ct);
        var o = db.Entry(thucThe).Property(truong);
        return o.CurrentValue is null ? null : JsonSerializer.Serialize(o.CurrentValue);
    }

    /// <summary>
    /// Đặt giá trị cho một ô. Trả về giá trị CŨ dạng JSON (null nghĩa là ô vốn rỗng — một giá trị
    /// hợp lệ, không phải "không có gì").
    /// </summary>
    public static async Task<string?> ApMotO(
        QlgxDbContext db, string bang, Guid banGhiId, string truong, string? giaTriJson,
        CancellationToken ct)
    {
        KiemCot(bang, truong);
        var thucThe = await LayThucThe(db, bang, banGhiId, ct);
        var o = db.Entry(thucThe).Property(truong);

        var cu = o.CurrentValue is null ? null : JsonSerializer.Serialize(o.CurrentValue);
        o.CurrentValue = giaTriJson is null
            ? null
            : JsonSerializer.Deserialize(giaTriJson, o.Metadata.ClrType);
        return cu;
    }

    /// <summary>
    /// Tạo một bản ghi mới từ JSON toàn bộ thực thể (dòng nhật ký loại "tao").
    ///
    /// Bỏ qua cột không tồn tại và cột nằm trong CotLoaiTru thay vì ném: dòng "tao" có thể đến từ
    /// một máy con chạy bản phần mềm cũ hơn, và từ chối cả bản ghi chỉ vì một cột lạ sẽ làm mất
    /// nguyên một hồ sơ giáo dân.
    /// </summary>
    public static async Task TaoBanGhi(
        QlgxDbContext db, string bang, Guid banGhiId, Guid giaoXuId, string giaTriJson,
        CancellationToken ct)
    {
        var kieu = LayKieu(db, bang);
        var daCo = await db.FindAsync(kieu.ClrType, [banGhiId], ct);
        if (daCo is not null) return; // đã tạo rồi (gửi lại lô) — không phải lỗi

        var thucThe = Activator.CreateInstance(kieu.ClrType)
            ?? throw new InvalidOperationException($"Khong tao duoc thuc the '{bang}'.");
        var muc = db.Entry(thucThe);

        var cacO = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(giaTriJson)
            ?? throw new InvalidOperationException("Gia tri dong 'tao' khong phai mot doi tuong JSON.");

        foreach (var (ten, giaTri) in cacO)
        {
            if (CotLoaiTru.BiLoai(ten)) continue;
            var o = muc.Properties.FirstOrDefault(p => p.Metadata.Name == ten);
            if (o is null) continue;
            o.CurrentValue = giaTri.ValueKind == JsonValueKind.Null
                ? null
                : giaTri.Deserialize(o.Metadata.ClrType);
        }

        // Ép hai cột định danh: máy con có thể gửi thiếu, và một bản ghi lạc giáo xứ thì RLS sẽ
        // từ chối ở tầng CSDL với một thông báo khó hiểu hơn nhiều.
        muc.Property("Id").CurrentValue = banGhiId;
        muc.Property("GiaoXuId").CurrentValue = giaoXuId;

        muc.State = EntityState.Added;
    }
}
```

- [ ] **Step 3: Chạy test**

Run: `dotnet test WebApp/tests/Qlgx.Data.Tests --filter ApThaoTacTests`
Expected: PASS (7 test).

- [ ] **Step 4: Commit**

```bash
git add WebApp/src/Qlgx.Data/DongBo/ApThaoTac.cs WebApp/tests/Qlgx.Data.Tests/ApThaoTacTests.cs
git commit -m "Them ApThaoTac: dat gia tri cho o goi ten bang chuoi, co rao chan"
```

---

## Task 5: Đầu vào nhận về và tải toàn bộ

**Files:**
- Create: `src/Qlgx.Api/Dtos/DongBoDtos.cs`
- Create: `src/Qlgx.Api/Services/DongBoService.cs`
- Create: `src/Qlgx.Api/Endpoints/DongBoEndpoints.cs`
- Modify: `src/Qlgx.Api/Program.cs`
- Modify: `tests/Qlgx.Data.Tests/MoiDuongGhiDeuGhiNhatKyTests.cs` (thêm `DongBoService.cs` vào danh sách miễn trừ, kèm lý do)
- Test: `tests/Qlgx.Api.Tests/DongBoNhanVeTests.cs`

**Interfaces:**
- Produces: `GET /api/dong-bo/thay-doi?epoch={guid}&tu={long}&toiDa={int}` → `NhanVeKetQua`; `GET /api/dong-bo/toan-bo` → `ToanBoKetQua`.

```csharp
public record DongHieuLucDto(long SoThuTu, string Bang, Guid BanGhiId, string Truong,
    string? GiaTri, DateTimeOffset DongHoVatLy, long DongHoLogic, Guid? ThietBiId, Guid GiaoDichId);

public record NhanVeKetQua(Guid Epoch, long ConTroMoi, bool ConNua, List<DongHieuLucDto> Dong);

public record ToanBoKetQua(Guid Epoch, long ConTro, DateTimeOffset ChupLuc, string DuLieuNen);
// DuLieuNen = JSON của ảnh chụp, nén **gzip** rồi mã hoá **base64**.
// Chốt gzip chứ không phải Brotli vì trình duyệt giải nén gzip nguyên bản bằng
// `DecompressionStream('gzip')` — không phải kéo thêm một thư viện JS nào vào PWA.
// `DecompressionStream` không nhận 'br', nên chọn Brotli là ép kế hoạch 5 thêm một
// phụ thuộc và tăng kích thước vỏ ứng dụng. Tỉ số nén kém hơn chút không đáng đổi.
```

- [ ] **Step 1: Viết test trước — chạy để thấy fail**

`tests/Qlgx.Api.Tests/DongBoNhanVeTests.cs`:

```csharp
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Qlgx.Api.Dtos;

namespace Qlgx.Api.Tests;

public class DongBoNhanVeTests(QlgxApiFactory f) : IClassFixture<QlgxApiFactory>
{
    [Fact]
    public async Task Con_tro_dung_epoch_tra_ve_cac_dong_sau_moc()
    {
        var client = f.CreateAuthClient();

        // Sinh vài thay đổi qua đường nghiệp vụ bình thường.
        await using (var db = f.TaoContextThuan())
        {
            db.GiaoDan.Add(new Domain.Entities.GiaoDan
            { GiaoXuId = f.GiaoXuId, MaGiaoDanCu = 9801, HoTen = "Nhan Ve A" });
            await db.SaveChangesAsync();
        }

        var dau = await client.GetFromJsonAsync<NhanVeKetQua>("/api/dong-bo/thay-doi?tu=0");
        dau.Should().NotBeNull();
        dau!.Epoch.Should().NotBe(Guid.Empty);

        // Sửa thêm một ô rồi hỏi tiếp từ con trỏ vừa nhận — chỉ được trả phần MỚI.
        var ds = await client.GetFromJsonAsync<List<Dtos.GiaoDanTimKiemDto>>(
            "/api/giao-dan/tim-kiem?tuKhoa=Nhan Ve A");
        ds!.Should().NotBeEmpty();

        var tiep = await client.GetFromJsonAsync<NhanVeKetQua>(
            $"/api/dong-bo/thay-doi?epoch={dau.Epoch}&tu={dau.ConTroMoi}");
        tiep!.Dong.Should().OnlyContain(d => d.SoThuTu > dau.ConTroMoi);
    }

    [Fact]
    public async Task Epoch_khong_khop_thi_tra_410_bao_tai_lai_toan_bo()
    {
        var client = f.CreateAuthClient();
        var phanHoi = await client.GetAsync($"/api/dong-bo/thay-doi?epoch={Guid.NewGuid()}&tu=5");

        phanHoi.StatusCode.Should().Be(HttpStatusCode.Gone,
            "con tro mo coi phai bi tu choi TUONG MINH — tra rong se lam may con im lang thieu " +
            "du lieu mai mai ma thanh trang thai van xanh");
    }

    [Fact]
    public async Task Chia_lo_va_bao_con_nua_khi_vuot_gioi_han()
    {
        var client = f.CreateAuthClient();
        var ketQua = await client.GetFromJsonAsync<NhanVeKetQua>("/api/dong-bo/thay-doi?tu=0&toiDa=2");

        ketQua!.Dong.Count.Should().BeLessThanOrEqualTo(2);
    }

    [Fact]
    public async Task Khong_doc_duoc_thay_doi_cua_giao_xu_khac()
    {
        var giaoXuKhac = Guid.NewGuid();
        await using (var db = f.TaoContextThuan())
        {
            db.GiaoXu.Add(new Domain.Entities.GiaoXu
            { Id = giaoXuKhac, TenGiaoXu = "Xu khac dong bo", MaGiaoXuCu = 91 });
            await db.SaveChangesAsync();
        }

        var client = f.CreateAuthClient();
        var ketQua = await client.GetFromJsonAsync<NhanVeKetQua>("/api/dong-bo/thay-doi?tu=0");

        ketQua!.Dong.Should().OnlyContain(d => d.Bang != "GiaoXu");
    }

    [Fact]
    public async Task Tai_toan_bo_tra_ve_con_tro_dung_thoi_diem_chup()
    {
        var client = f.CreateAuthClient();
        var toanBo = await client.GetFromJsonAsync<ToanBoKetQua>("/api/dong-bo/toan-bo");

        toanBo.Should().NotBeNull();
        toanBo!.Epoch.Should().NotBe(Guid.Empty);
        toanBo.DuLieuNen.Should().NotBeNullOrEmpty();

        // Hỏi ngay từ con trỏ đó phải không còn gì mới — nếu có, ảnh chụp và con trỏ đã lệch
        // nhau và máy con sẽ bỏ sót đúng khoảng lệch đó.
        var tiep = await client.GetFromJsonAsync<NhanVeKetQua>(
            $"/api/dong-bo/thay-doi?epoch={toanBo.Epoch}&tu={toanBo.ConTro}");
        tiep!.Dong.Should().BeEmpty();
    }
}
```

Run: `dotnet test WebApp/tests/Qlgx.Api.Tests --filter DongBoNhanVeTests`
Expected: FAIL — chưa có endpoint.

- [ ] **Step 2: Viết DTO, service, endpoint**

`DongBoService.NhanVe`: kiểm `epoch` (nếu client gửi và khác `epoch` hiện tại → trả `null` để endpoint đổi thành 410); đọc `hieu_luc` của giáo xứ hiện tại `WHERE so_thu_tu > tu ORDER BY so_thu_tu LIMIT toiDa+1`; **không được cắt giữa một `GiaoDichId`** (spec 7.6) — nếu dòng cuối cùng của lô cùng `GiaoDichId` với dòng đầu của phần bị cắt thì lùi lại tới ranh giới giao dịch; `ConNua` = còn dòng sau lô này.

`DongBoService.ToanBo`: mở **một** giao dịch, đọc `bo_dem_hieu_luc` để lấy `epoch` và số hiện tại, rồi chụp toàn bộ bảng nghiệp vụ trong **cùng** giao dịch đó — nếu chụp ngoài giao dịch, con trỏ và ảnh chụp lệch nhau và máy con bỏ sót đúng khoảng lệch. Nén bằng gzip rồi Base64 (giáo xứ 4074 giáo dân ≈ 500 KB nén, xem spec 1.2).

Đường dẫn nhóm theo đúng lối các endpoint khác: `app.MapGroup("/api/dong-bo").RequireAuthorization()`.

- [ ] **Step 3: Thêm `DongBoService.cs` vào danh sách miễn trừ của kiểm thử kiến trúc**

Trong `MoiDuongGhiDeuGhiNhatKyTests.cs`, thêm `"DongBoService.cs"` vào `DuocPhepGoiThang` kèm chú thích:

```csharp
        // DongBoService: nhật ký cho thao tác đồng bộ được ghi TƯỜNG MINH với đồng hồ của MÁY
        // CON. Nếu đi qua LuuCoNhatKy thì sẽ có thêm một bộ dòng mang đồng hồ MÁY CHỦ — hai dòng
        // cho một thay đổi, và dòng sai đồng hồ thắng ở lần gộp sau.
        "DongBoService.cs",
```

- [ ] **Step 4: Chạy test**

Run: `dotnet test WebApp/tests/Qlgx.Api.Tests --filter DongBoNhanVeTests` rồi `dotnet test WebApp/tests/Qlgx.Data.Tests`
Expected: PASS cả hai.

- [ ] **Step 5: Chứng minh test 410 biết báo lỗi**

Tạm đổi endpoint trả `Results.Ok(rỗng)` thay vì 410 khi `epoch` lệch.
Run: `dotnet test WebApp/tests/Qlgx.Api.Tests --filter Epoch_khong_khop`
Expected: **FAIL**. Hoàn nguyên, xác nhận PASS.

- [ ] **Step 6: Commit**

```bash
git add WebApp/src/Qlgx.Api/Dtos/DongBoDtos.cs \
        WebApp/src/Qlgx.Api/Services/DongBoService.cs \
        WebApp/src/Qlgx.Api/Endpoints/DongBoEndpoints.cs \
        WebApp/src/Qlgx.Api/Program.cs \
        WebApp/tests/Qlgx.Data.Tests/MoiDuongGhiDeuGhiNhatKyTests.cs \
        WebApp/tests/Qlgx.Api.Tests/DongBoNhanVeTests.cs
git commit -m "Them dau vao nhan ve va tai toan bo cho may con"
```

---

## Task 6: Đầu vào gửi lên

**Files:**
- Modify: `src/Qlgx.Api/Dtos/DongBoDtos.cs`, `src/Qlgx.Api/Services/DongBoService.cs`, `src/Qlgx.Api/Endpoints/DongBoEndpoints.cs`
- Create: `src/Qlgx.Domain/Entities/CanXemLai.cs`, `src/Qlgx.Data/Configurations/CanXemLaiConfig.cs`, migration
- Test: `tests/Qlgx.Api.Tests/DongBoGuiLenTests.cs`

**Interfaces:**
- Produces: `POST /api/dong-bo/gui-len` nhận `GuiLenYeuCau`, trả `GuiLenKetQua`.

```csharp
public record ThaoTacDto(Guid MaThaoTac, Guid GiaoDichId, string Loai, string Bang, Guid BanGhiId,
    string Truong, string? GiaTri, DateTimeOffset DongHoVatLy, long DongHoLogic,
    Guid? NguonGocEpoch, long? NguonGocSoThuTu);

public record GuiLenYeuCau(Guid ThietBiId, DateTimeOffset GioMayCon, Guid? Epoch,
    long ConTro, List<ThaoTacDto> ThaoTac);

public record KetQuaThaoTacDto(Guid MaThaoTac, string KetQua, string? ThongBao);

public record GuiLenKetQua(Guid Epoch, long ConTroMoi, List<KetQuaThaoTacDto> KetQua,
    List<DongHieuLucDto> DongMoi);
```

**Bốn quy tắc bắt buộc của task này:**

1. **Một thao tác hỏng không được chặn cả hàng** (spec 7.5). Từng thao tác được xử lý và ghi kết quả riêng; lỗi của một cái không làm hỏng lô.
2. **Thao tác đã nhận thì trả nguyên phản hồi cũ**, không xử lý lại (bảng `thao_tac_da_nhan`, ràng buộc 5.9).
3. **Trả kèm dòng mới sau con trỏ của máy con** (`DongMoi`) — spec 7.3 điểm 1: ai đang nhập liệu thì dữ liệu luôn mới, miễn phí.
4. **Thứ tự khoá:** khoá dòng đếm `bo_dem_hieu_luc` phải giành **ngay sau** khi mở giao dịch, trước mọi thao tác ghi dữ liệu — ngược thứ tự là deadlock thật (bài học kế hoạch 1, Task 6).

- [ ] **Step 1: Viết thực thể `CanXemLai` và cấu hình**

```csharp
namespace Qlgx.Domain.Entities;

/// <summary>
/// Một việc máy không tự quyết được, chờ người xem lại. Nằm ở MÁY CHỦ chứ không ở máy con: ai xử
/// lý cũng được, không phải đúng người ở đúng máy đã gây ra nó, và xử xong thì mọi máy thấy ngay.
/// </summary>
public class CanXemLai
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid GiaoXuId { get; set; }

    /// <summary>"o_nhay_cam" | "nghi_trung" | "quan_he" | "bat_bien" | "tham_chieu_chet"</summary>
    public string Loai { get; set; } = "";
    public string Bang { get; set; } = "";
    public Guid BanGhiId { get; set; }
    public string Truong { get; set; } = "";

    public string? GiaTriA { get; set; }
    public string? GiaTriB { get; set; }
    /// <summary>Giá trị đang được dùng — luôn là một trong hai, để người xem biết hiện trạng.</summary>
    public string? GiaTriDangDung { get; set; }

    public Guid? ThietBiA { get; set; }
    public Guid? ThietBiB { get; set; }
    public DateTimeOffset LucA { get; set; }
    public DateTimeOffset LucB { get; set; }

    public DateTimeOffset TaoLuc { get; set; }
    public DateTimeOffset? DaXuLyLuc { get; set; }
    public Guid? NguoiXuLy { get; set; }
}
```

Cấu hình: khoá chính `Id`; `HasIndex(x => new { x.GiaoXuId, x.DaXuLyLuc })` để lấy nhanh danh sách chưa xử lý; `GiaTri*` kiểu `jsonb`. Thêm `DbSet`, `HasQueryFilter`, và RLS trong migration — **cả hai**.

- [ ] **Step 2: Viết test trước — chạy để thấy fail**

`tests/Qlgx.Api.Tests/DongBoGuiLenTests.cs` — sáu fact:

```csharp
    [Fact] public async Task Gui_mot_thao_tac_sua_thi_du_lieu_doi_va_sinh_dong_hieu_luc() { }
    [Fact] public async Task Gui_lai_cung_ma_thao_tac_khong_xu_ly_lai_va_tra_nguyen_phan_hoi_cu() { }
    [Fact] public async Task Thao_tac_cu_hon_thi_thua_va_khong_doi_du_lieu() { }
    [Fact] public async Task Xung_dot_o_nhay_cam_van_ap_gia_tri_moi_nhung_sinh_muc_can_xem_lai() { }
    [Fact] public async Task Mot_thao_tac_hong_khong_chan_cac_thao_tac_con_lai_trong_lo() { }
    [Fact] public async Task Phan_hoi_kem_theo_cac_dong_moi_sau_con_tro_cua_may_con() { }
```

Viết đầy đủ từng fact theo khuôn các test API đã có (`f.CreateAuthClient()`, `f.TaoContextThuan()`). Với fact "một thao tác hỏng không chặn cả hàng": gửi lô ba thao tác trong đó cái thứ hai trỏ tới `BanGhiId` không tồn tại, khẳng định thao tác 1 và 3 vẫn `"ap"` còn thao tác 2 là `"tu_choi"`.

Run: `dotnet test WebApp/tests/Qlgx.Api.Tests --filter DongBoGuiLenTests`
Expected: FAIL.

- [ ] **Step 3: Viết `DongBoService.GuiLen`**

Khung xử lý một lô:

```
mở giao dịch tường minh
  giành khoá dòng đếm NGAY (CapSoHieuLuc.LayDaiSo cho ước lượng số dòng sẽ sinh)
  tính độ lệch đồng hồ = GioMayCon - giờ máy chủ; ghi cảnh báo nếu lệch quá 1 giờ
  với mỗi thao tác trong lô, theo đúng thứ tự máy con gửi:
     nếu đã có trong thao_tac_da_nhan (theo MaThaoTac, hoặc theo danh tính gốc nếu là bù lại)
        -> trả nguyên phản hồi cũ, sang thao tác kế
     hiệu chỉnh mốc về giờ máy chủ, KẸP `min(mốc đã hiệu chỉnh, giờ máy chủ)`,
        CẮT VỀ MICRO GIÂY (DongHoLai.CatMicroGiay), rồi dựng DauDongHo
     nâng đồng hồ giáo xứ: NangDau(dấu cuối đọc từ dòng đếm, dấu vừa dựng, giờ máy chủ)
        -> ghi lại `dau_cuoi_vat_ly` / `dau_cuoi_logic` trên dòng đếm ĐANG GIỮ KHOÁ
     nếu Loai == "tao"  -> ApThaoTac.TaoBanGhi
     nếu Loai == "sua"  -> CHIA CÁC Ô THÀNH NHÓM bằng LuatGop.NhomGop, rồi VỚI TỪNG NHÓM:
                            đọc MocO của MỌI ô thuộc nhóm, lấy mốc MỚI NHẤT làm mốc của nhóm
                            LuatGop.Quyet MỘT LẦN cho cả nhóm bằng mốc đó
                            Thua -> CẢ NHÓM thua, ghi ThayDoi(Thang=false), không ô nào áp lẻ
                            Thang / ThangCanXemLai -> ApThaoTac.ApMotO cho mọi ô của nhóm CÓ
                                                      trong thao tác, ghi ThayDoi(Thang=true)
                                                      + HieuLuc, và cập nhật MocO cho TOÀN BỘ ô
                                                      của nhóm — kể cả ô không đổi
                            ThangCanXemLai -> thêm một dòng CanXemLai
     ghi kết quả vào thao_tac_da_nhan
  SaveChanges một lần cho cả lô
commit
đọc các dòng hieu_luc sau ConTro của máy con để trả kèm
```

**Hai cột đồng hồ giáo xứ — T6 tạo, vì T6 là nơi duy nhất tiêu thụ (R6 phần 2).**

Thêm vào cùng migration mà task này đã tạo cho `CanXemLai`:

```
ALTER TABLE bo_dem_hieu_luc
  ADD COLUMN dau_cuoi_vat_ly timestamptz NOT NULL DEFAULT '-infinity',
  ADD COLUMN dau_cuoi_logic  bigint      NOT NULL DEFAULT 0;
```

Và mở rộng `CapSoHieuLuc.LayDaiSo` (file của kế hoạch 1) thành:

```csharp
public static async Task<(long SoDau, Guid Epoch, DauDongHo DauCuoi)> LayDaiSo(
    QlgxDbContext db, Guid giaoXuId, int soLuong, CancellationToken ct)
```

`DauCuoi` dựng từ hai cột mới (`ThietBiId = null`, `MaThaoTac = Guid.Empty`). Sau khi xử lý xong
cả lô, ghi lại mốc cao nhất đã phát vào hai cột đó **trong cùng giao dịch đang giữ khoá**.

Vì sao đặt ở dòng đếm chứ không phải bảng riêng: mọi đường ghi sinh `so_thu_tu` đã giành khoá
`FOR UPDATE` trên đúng dòng này rồi. Đặt đồng hồ ở đây thì tính nguyên tử có sẵn — không khoá mới,
không tranh chấp mới, không thêm một thứ tự khoá nào có thể gây deadlock.

**Giữ nguyên chữ ký cũ không được.** `LayDaiSo` là điểm duy nhất mọi đường ghi đi qua; nếu đồng hồ
nằm chỗ khác thì sẽ có đường ghi quên nâng nó, và lỗi đó không có test nào bắt được một cách tự
nhiên. Đổi chữ ký buộc mọi nơi gọi phải nhìn lại — đó là mục đích.

**Chuẩn hoá `giaTriMoi` TRƯỚC khi so (R33 trong sổ thi công).**

Đã kiểm chứng khứ hồi `DocO -> ApMotO -> DocO` cho chữ Việt có dấu, `DateOnly`, `bool`, chuỗi rỗng,
`int`, `null`: đúng 100%, và chuỗi sinh ra khớp từng ký tự với `SinhDongNhatKy`. Nhưng đó là khi
**cả hai đầu đều là .NET**. Máy con là TypeScript, và `JSON.stringify` **không** escape ký tự ngoài
ASCII trong khi `System.Text.Json` thì có:

| Nguồn | `"Nguyễn Thị Bưởi"` được lưu thành |
|---|---|
| .NET (`SinhDongNhatKy`, `DocO`) | `"Nguyễn Thị Bưởi"` |
| JavaScript (`JSON.stringify`) | `"Nguyễn Thị Bưởi"` |

Luật gộp so **chuỗi**, không so giá trị. Bỏ qua chuyện này thì **mọi tên người Việt có dấu sinh một
xung đột giả**, tức gần như mọi giáo dân, ở mỗi lần đồng bộ. Hộp cần xem lại ngập hàng nghìn mục vô
nghĩa và quý sơ sẽ quen tay bấm bỏ qua — rồi bỏ qua luôn mục thật.

Vì vậy T6 phải đưa `giaTriMoi` nhận từ máy con qua đúng cặp `JsonSerializer.Deserialize` →
`JsonSerializer.Serialize` của .NET **trước** khi truyền vào `LuatGop.Quyet`. Test bắt buộc: một ô
`HoTen` nhận chuỗi chữ Việt **không** escape phải được coi là **bằng** giá trị đang có, không sinh
mục cần xem lại.

**Vì sao phải gộp theo NHÓM chứ không quyết từng ô (R16 trong sổ thi công).**

`LuatGop.NhomGop` do Task 3 sinh ra nhưng **bản kế hoạch đầu không có task nào gọi nó** — T6 quyết
từng ô độc lập. Đó là một lỗ thật, và doc-comment trong `LuatGop.cs` mô tả đúng tai hoạ nó gây ra:

> Máy A đánh dấu qua đời kèm ngày 12/9. Máy B (mới hơn) bỏ dấu qua đời nhưng không đụng ô ngày.
> Gộp từng ô độc lập xong ra **một người còn sống mà có ngày qua đời**.

Với sổ sách giáo xứ, đây là loại sai không ai phát hiện được bằng mắt cho tới khi in sổ ra — lọt
hay không lọt thống kê tuỳ chỗ nào đọc ô nào.

**Điểm mấu chốt là cập nhật `MocO` cho TOÀN BỘ ô của nhóm, kể cả ô không đổi.** Nếu chỉ cập nhật ô
có thay đổi, thì một thao tác cũ hơn đến sau vẫn sửa lẻ được ô `NgayQuaDoi` (mốc của nó chưa tiến)
và trạng thái lai quay lại. Đừng thay bước này bằng "bắt máy con luôn gửi đủ cả nhóm": máy con là
form nên nó vốn gửi cả bản ghi, **nhưng ta không được dựa vào đó** — một bản máy con cũ, một lỗi,
hay một lần bù lại sau khôi phục đều có thể gửi thiếu. Cập nhật mốc cả nhóm thì không cần tin ai.

**Test bắt buộc cho bất biến này** (hai test `NhomGop` của Task 3 chỉ kiểm bảng tra, không kiểm bất
biến — chúng xanh kể cả khi không ai gọi hàm):

```csharp
[Fact]
public async Task Bo_dau_qua_doi_thi_ngay_qua_doi_KHONG_duoc_o_lai()
{
    // Máy A: đánh dấu qua đời kèm ngày. Máy B mới hơn: chỉ bỏ dấu qua đời, không đụng ô ngày.
    // Nếu gộp từng ô độc lập, ô ngày sống sót và ta có người còn sống mang ngày qua đời.
    await GuiLen(mayA, moc: T0, o: [("QuaDoi", "true"), ("NgayQuaDoi", "2026-09-12")]);
    await GuiLen(mayB, moc: T0.AddMinutes(5), o: [("QuaDoi", "false")]);

    var gd = await DocGiaoDan();
    gd.QuaDoi.Should().BeFalse();
    gd.NgayQuaDoi.Should().BeNull("cung nhom voi QuaDoi nen phai theo ca nhom");
}

[Fact]
public async Task Thao_tac_CU_HON_den_SAU_khong_duoc_sua_le_mot_o_trong_nhom()
{
    // Đây là ca mà "chỉ cập nhật MocO của ô có đổi" sẽ hỏng.
    await GuiLen(mayB, moc: T0.AddMinutes(5), o: [("QuaDoi", "false")]);
    await GuiLen(mayA, moc: T0, o: [("NgayQuaDoi", "2026-09-12")]);   // cũ hơn, tới sau

    (await DocGiaoDan()).NgayQuaDoi.Should().BeNull("moc CA NHOM da tien, thao tac cu phai thua");
}
```

**Vì sao kẹp và nâng đồng hồ phải đi CÙNG NHAU (R6/R10 trong sổ thi công):**

Kẹp một mình tạo ra một lỗ mới. Mốc máy con bị kẹp sẽ **bằng đúng** giờ máy chủ, nên phân xử rơi
xuống tầng Logic — mà đường ghi thường của web luôn phát `Logic = 0`, còn máy con phát `Logic >= 0`.
Kết quả: mọi thao tác máy con bị kẹp đều **luôn thắng** thao tác quý cha vừa gõ trên web ở cùng
giây đó. Chỉ khi máy chủ cũng giữ đồng hồ logic riêng (`dau_cuoi_*` trên dòng đếm) thì kẹp mới an
toàn. Không được làm một nửa.

Nâng đồng hồ một mình cũng không đủ: thiếu kẹp thì một máy con báo sai giờ (đồng hồ CMOS hỏng,
hoặc cố tình) gửi mốc ở năm 2030, và mốc đó thắng mọi bản ghi hợp lệ về sau **vĩnh viễn**.

**Cắt micro giây phải làm lúc DỰNG dấu, không phải lúc lưu.** `timestamptz` của Postgres giữ tới
micro giây, `DateTimeOffset` tới 100ns. Cắt lúc lưu thì cái so trong bộ nhớ và cái so sau khi đọc
lại từ CSDL là hai thứ khác nhau — sinh ra hoà giả, và hai máy kết luận khác nhau.

Chia lô nếu `ThaoTac.Count` lớn: **tối đa 200 thao tác một giao dịch**, đúng ngưỡng đã dùng ở kế hoạch 1 — giữ khoá dòng đếm ngắn.

- [ ] **Step 4: Chạy test**

Run: `dotnet test WebApp/tests/Qlgx.Api.Tests --filter DongBoGuiLenTests`
Expected: PASS (6 test).

- [ ] **Step 5: Chứng minh test chống trùng biết báo lỗi**

Tạm bỏ đoạn tra `thao_tac_da_nhan`.
Run: `dotnet test WebApp/tests/Qlgx.Api.Tests --filter Gui_lai_cung_ma_thao_tac`
Expected: **FAIL**. Hoàn nguyên, xác nhận PASS.

- [ ] **Step 6: Commit**

```bash
git add WebApp/src/Qlgx.Domain/Entities/CanXemLai.cs \
        WebApp/src/Qlgx.Data/Configurations/CanXemLaiConfig.cs \
        WebApp/src/Qlgx.Data/QlgxDbContext.cs \
        WebApp/src/Qlgx.Data/Migrations/ \
        WebApp/src/Qlgx.Api/Dtos/DongBoDtos.cs \
        WebApp/src/Qlgx.Api/Services/DongBoService.cs \
        WebApp/src/Qlgx.Api/Endpoints/DongBoEndpoints.cs \
        WebApp/tests/Qlgx.Api.Tests/DongBoGuiLenTests.cs
git commit -m "Them dau vao gui len: gop muc truong, chong trung, hop can xem lai"
```

---

## Task 6b: Ghi `MocO` ở đường ghi thường của web

**Vì sao có task này.** Phát hiện khi thi công Task 6. `MocO` được ghi ở **đúng một chỗ**:
`DongBoService`. Mọi lần quý cha sửa trên web đều **không để lại mốc nào**, nên
`LuatGop.Quyet(mocDangCo: null, …)` trả `Thang` vô điều kiện và **máy con luôn thắng** — kể cả một
thao tác cũ hơn ba ngày.

Kịch bản hỏng: cha sửa ngày rửa tội của giáo dân trên web lúc 10g. Một laptop offline ba ngày đẩy
lên bản sửa cũ của đúng ô đó. Bản sửa của cha bị đè **im lặng**, không một dấu hiệu nào. Đây chính
xác là loại hỏng mà toàn bộ thiết kế luật gộp sinh ra để ngăn.

Đây là lỗ của kế hoạch, không phải của ai thi công: kế hoạch 1 dựng nhật ký, `MocO` sinh ra ở kế
hoạch 4 Task 1, và không ai nối dây giữa hai cái. Task 6 đã nối được một nửa — đường ghi web **đã**
nâng đồng hồ máy chủ trên dòng đếm. Chỉ còn thiếu đúng mảnh `MocO`.

Tách thành task riêng thay vì gộp vào Task 6 vì nó đụng vào file của kế hoạch 1 (`LuuCoNhatKy`) và
ảnh hưởng **mọi** đường ghi của web — nó xứng đáng một mặt review riêng.

**Files:**
- Modify: `src/Qlgx.Data/NhatKy/LuuCoNhatKy.cs`
- Test: `tests/Qlgx.Data.Tests/MocOTheoDuongGhiWebTests.cs`

**Interfaces:**
- Consumes: `MocO` (Task 1); `CapSoHieuLuc.LayDaiSo` trả `DauCuoi` (Task 6); `DongHoLai.NangDau` (Task 2).
- Produces: không có kiểu mới — `LuuCoNhatKy` thêm việc ghi `MocO` cho mỗi ô nó sinh dòng nhật ký.

**Nguyên tắc bắt buộc: một ô sinh dòng `hieu_luc` thì PHẢI sinh `MocO` tương ứng, cùng giao dịch.**
Hai thứ này là hai mặt của một việc — dòng `hieu_luc` nói "ô này vừa đổi", `MocO` nói "ô này đổi
lúc nào và bởi ai". Thiếu mặt sau thì luật gộp mù. Đừng để chúng thành hai danh sách phải nhớ khớp
nhau: **suy ra cái sau từ chính tập ô đã sinh cái trước**, trong cùng một vòng lặp.

- [ ] **Step 1: Viết test trước**

```csharp
[Fact]
public async Task Sua_tren_web_roi_may_con_gui_ban_CU_hon_thi_may_con_THUA()
{
    // Đây là bất biến cả task này tồn tại vì nó. Trước khi sửa, máy con thắng vô điều kiện.
    var idGiaoDan = await TaoGiaoDan(9800, "Nguoi bi de mat sua");

    // Quý cha sửa trên web lúc 10g (đường ghi thường).
    await SuaTrenWeb(idGiaoDan, "HoTen", "Ten cha vua sua");

    // Laptop offline ba ngày đẩy lên bản sửa CŨ HƠN của đúng ô đó.
    var moc = await DocMocO("GiaoDan", idGiaoDan, "HoTen");
    moc.Should().NotBeNull("duong ghi web PHAI de lai moc, neu khong may con thang vo dieu kien");

    var ketQua = LuatGop.Quyet(moc, new DauDongHo(moc!.DongHoVatLy.AddDays(-3), 0,
        Guid.NewGuid(), Guid.NewGuid()), "GiaoDan", "HoTen",
        "\"Ten cha vua sua\"", "\"Ten cu tren laptop\"");

    ketQua.Should().Be(KetQuaGop.Thua, "thao tac cu hon khong duoc de len ban sua cua cha");
}

[Fact]
public async Task Moi_o_sinh_dong_hieu_luc_deu_co_MocO_tuong_ung()
{
    // Lưới an toàn: quét theo chính tập dòng đã sinh, không liệt kê tay.
    var idGiaoDan = await TaoGiaoDan(9801, "Nguoi kiem du moc");
    await SuaNhieuO(idGiaoDan);

    var cacDong = await DocHieuLucMoiNhat(idGiaoDan);
    cacDong.Should().NotBeEmpty("khong co dong nao thi test nay rong nghia");

    foreach (var dong in cacDong)
    {
        var moc = await DocMocO(dong.Bang, dong.BanGhiId, dong.Truong);
        moc.Should().NotBeNull($"o {dong.Bang}.{dong.Truong} sinh dong hieu luc ma khong co moc");
        moc!.DongHoVatLy.Should().Be(dong.DongHoVatLy);
        moc.MaThaoTac.Should().Be(dong.MaThaoTac);
    }
}

[Fact]
public async Task Ghi_web_va_ghi_MocO_nam_trong_CUNG_mot_giao_dich()
{
    // Nếu tách hai giao dịch, một lần sập giữa chừng để lại dòng hieu_luc không có mốc — và ô đó
    // vĩnh viễn để máy con thắng vô điều kiện, đúng lỗi mà task này sinh ra để sửa.
    // Chứng minh bằng cách ép SaveChanges ném sau khi đã ghi hieu_luc.
}
```

- [ ] **Step 2: Chạy test để thấy fail**

Run: `dotnet test WebApp/tests/Qlgx.Data.Tests --filter MocOTheoDuongGhiWebTests -o "<thư mục tạm riêng>"`
Expected: FAIL — `moc.Should().NotBeNull()` đỏ, vì đường ghi web chưa ghi `MocO`.

- [ ] **Step 3: Nối `MocO` vào `LuuCoNhatKy`**

**Mã thật đã thay đổi kể từ khi task này được viết lần đầu** (Task 6 đã cài xong việc nâng đồng hồ
máy chủ) — đọc `WebApp/src/Qlgx.Data/NhatKy/LuuCoNhatKy.cs` trước khi sửa. Vị trí cần sửa là hàm
`GhiDongTuongMinh`, bên trong vòng lặp theo giáo xứ, ngay sau đoạn tính `(vatLy, logic)` qua
`DongHoLai.NangDau`.

Chỉ ghi `MocO` cho ô có `Truong` khác rỗng (`Loai == "sua"`) — bản ghi vừa **tạo mới**
(`Loai == "tao"`) không đặt `MocO`, đúng quy ước Task 6 đã dùng ở `ApThaoTacTao`: "bản ghi vừa ra
đời thì chưa có cuộc đua nào để ghi lại". Với mỗi ô đủ điều kiện: khoá `(GiaoXuId, Bang, BanGhiId,
Truong)`, bốn cột mốc lấy đúng dấu vừa dùng cho dòng `HieuLuc` của ô đó (`vatLy`, `logic`,
`ThietBiId = null`, `MaThaoTac = Guid.Empty` — khớp `DauDongHo` dùng cho `ChotDaiSo`). Ghi đè
không điều kiện nếu đã có. Cùng giao dịch, không `SaveChanges` riêng.

- [ ] **Step 4: Chạy test — PASS**

- [ ] **Step 5: Chứng minh test biết báo lỗi**

Tạm bỏ đoạn ghi `MocO`. Xác nhận cả ba test đỏ. Hoàn nguyên, xác nhận xanh.

- [ ] **Step 6: Commit**

---

## Task 7: Đường đọc và xử lý hộp cần xem lại

**Brief này viết lại toàn bộ sau khi Task 6 hoàn tất** — bản đầu viết trước khi `CanXemLai` có
hình dạng thật, quá mỏng và bỏ sót việc phân biệt ba loại mục. Đọc kỹ trước khi thi công, đừng chỉ
lấy phần khung việc rồi tự đoán chi tiết.

**Files:**
- Modify: `src/Qlgx.Api/Dtos/DongBoDtos.cs`, `src/Qlgx.Api/Endpoints/DongBoEndpoints.cs`,
  `tests/Qlgx.Data.Tests/MoiDuongGhiDeuGhiNhatKyTests.cs` (thêm `CanXemLaiService.cs` vào miễn trừ,
  kèm một fact canh chính miễn trừ đó — xem "Vì sao cần miễn trừ" bên dưới)
- Create: `src/Qlgx.Api/Services/CanXemLaiService.cs`
- Test: `tests/Qlgx.Api.Tests/CanXemLaiTests.cs`

**Interfaces:**
- Produces: `GET /api/can-xem-lai` (danh sách chưa xử lý của giáo xứ hiện tại);
  `POST /api/can-xem-lai/{id}/chon` thân `{ "chon": "A" | "B" }`;
  `POST /api/can-xem-lai/{id}/danh-dau-da-xu-ly` (không thân).

```csharp
public record CanXemLaiDto(Guid Id, string Loai, string Bang, Guid BanGhiId, string Truong,
    string? LyDo, string? GiaTriA, string? GiaTriB, string? GiaTriDangDung, DateTimeOffset TaoLuc);
public record ChonGiaTriYeuCau(string Chon);
```

**Đọc `WebApp/src/Qlgx.Domain/Entities/CanXemLai.cs` TRƯỚC KHI VIẾT GÌ** — đặc biệt chú thích của
`LyDo` và `Loai`. Sáu giá trị `Loai` khả dĩ, nhưng hôm nay chỉ ba loại thật sự được sinh ra (Task 6):
`"o_nhay_cam"`, `"bat_bien"`, `"khong_luu_duoc"`. Ba loại còn lại (`"nghi_trung"`, `"quan_he"`,
`"tham_chieu_chet"`) là tên dành sẵn cho việc chưa làm — **đừng cố xử lý chúng**, chỉ cần không vỡ
nếu gặp (coi như "không thể quyết bằng /chon").

**KHÔNG dịch `Truong` sang nhãn tiếng Việt ở đây.** Đã phán quyết ở Task 6 (xem chú thích trong
chính `CanXemLai.cs`): máy chủ không có bản đồ "tên cột → nhãn" dùng chung, và dựng một bản đồ chỉ
cho task này sẽ là danh sách thứ năm phải nhớ khớp với thứ gì đó khác — mẫu đã cắn kế hoạch này
nhiều lần. Việc dịch nhãn thuộc về màn hình hiển thị thật (`CanXemLaiPage.tsx`, kế hoạch 5). Trả về
nguyên `Bang`/`Truong` là đủ và đúng phạm vi.

**Hai loại quyết định, KHÔNG được trộn vào một endpoint:**

1. **Loại có cặp giá trị thật để chọn** (`"o_nhay_cam"`, `"bat_bien"`) → `POST .../chon`. Với
   `Loai` khác, endpoint này trả **400** — không được âm thầm coi như đã xử lý.
2. **Loại không có gì để chọn** (`"khong_luu_duoc"` — dữ liệu đã mất, người dùng phải **nhập lại
   qua màn hình sửa bình thường**, không có "giá trị B đang dùng" nào để giữ) → chỉ
   `POST .../danh-dau-da-xu-ly`. Gọi endpoint này cho `"o_nhay_cam"`/`"bat_bien"` cũng phải trả
   **400** — không cho phép "bỏ qua" một xung đột thật mà không ghi quyết định, vì làm vậy để lộ
   đúng lỗ mà spec 8.6 cảnh báo (mục dưới đây).

**Quy tắc cốt lõi cho `/chon` (spec 8.6):** quyết định của người dùng là **một thao tác ghi bình
thường** mang **mốc hiện tại**. Nếu chỉ đánh dấu mục đã xử lý mà không ghi một thay đổi mới, thì
lần đồng bộ sau giá trị kia (mốc mới hơn) vẫn thắng và **tự đổi lại** — người dùng thấy phần mềm
"không nghe lời". Ghi **thống nhất cho cả hai nhánh A và B** (đừng tối ưu "B đang dùng rồi thì khỏi
ghi" — một nhánh có điều kiện riêng là thêm một đường chưa được test, và giá thành của ghi thừa một
dòng `hieu_luc` rẻ hơn nhiều so với một nhánh không ai kiểm).

**Vì sao cần miễn trừ `MoiDuongGhiDeuGhiNhatKyTests`.** `/chon` cần một mốc **cụ thể** (giờ máy chủ
hiện tại, không phải mốc tự tính từ so sánh ChangeTracker) và phải cập nhật `MocO` — giống hệt lý
do `DongBoService.cs` đã được miễn trừ ở Task 6. Không đi qua `LuuCoNhatKy`/`GhiDongTuongMinh`;
dựng `ThayDoi`+`HieuLuc` tường minh theo đúng khuôn `DongBoService.GhiCapNhatKy` đã dùng (đọc hàm
đó trước khi viết hàm tương tự ở đây — đừng chép dán, khuôn giống nhưng bối cảnh khác).

- [ ] **Step 1: Viết test — đủ mã, không placeholder**

```csharp
[Fact]
public async Task Chon_gia_tri_cu_hon_thi_no_phai_THANG_o_lan_dong_bo_sau()
{
    // Đây là fact quan trọng nhất của task — đúng cái bẫy spec 8.6 cảnh báo.
    // Dựng một mục cần xem lại kiểu "o_nhay_cam": A cũ hơn, B mới hơn và đang được dùng.
    var idGiaoDan = await TaoGiaoDan(9900, "Nguoi co xung dot ngay sinh");
    var idMuc = await TaoMucCanXemLai(idGiaoDan, "GiaoDan", "NgaySinh",
        giaTriA: "\"1985-03-12\"", giaTriB: "\"1985-03-13\"", dangDung: "B");

    // Người dùng chọn A.
    var phanHoi = await client.PostAsync($"/api/can-xem-lai/{idMuc}/chon",
        JsonContent.Create(new ChonGiaTriYeuCau("A")));
    phanHoi.StatusCode.Should().Be(HttpStatusCode.OK);

    // Sau đó gửi lại một lô đồng bộ có chứa B (như một máy con đến muộn, mốc của B CŨ HƠN
    // quyết định vừa ghi vì quyết định mang giờ máy chủ HIỆN TẠI).
    await GuiLoDongBoChuaGiaTri(idGiaoDan, "NgaySinh", "\"1985-03-13\"", mocCu: true);

    var giaoDan = await DocGiaoDan(idGiaoDan);
    giaoDan.NgaySinh.Should().Be(new DateOnly(1985, 3, 12),
        "quyet dinh cua nguoi dung mang moc HIEN TAI, phai thang moi thao tac den sau");
}

[Fact]
public async Task Danh_sach_chi_tra_ve_muc_cua_dung_giao_xu_va_chua_xu_ly()
{
    // Dựng: một mục của giáo xứ hiện tại (chưa xử lý), một mục của giáo xứ khác, một mục của
    // giáo xứ hiện tại NHƯNG đã xử lý (DaXuLyLuc != null). Cả ba đều có mặt trong CSDL.
    // Chỉ mục đầu tiên được trả về.
}

[Fact]
public async Task Chon_voi_loai_khong_luu_duoc_bi_tu_choi_400()
{
    // Loai "khong_luu_duoc" không có cặp giá trị thật — /chon phải trả 400, không được coi
    // như đã xử lý một cách âm thầm.
}

[Fact]
public async Task Danh_dau_da_xu_ly_voi_loai_o_nhay_cam_bi_tu_choi_400()
{
    // Không được phép "bỏ qua" một xung đột thật mà không ghi quyết định — nếu cho qua, giá trị
    // đang dùng hôm nay có thể vẫn thua một thao tác cũ hơn đến sau, đúng lỗ spec 8.6 cảnh báo.
}

[Fact]
public async Task Xu_ly_hai_lan_lien_tiep_khong_ghi_them_lan_thu_hai()
{
    // Chọn A xong, gọi /chon lần nữa (dù A hay B) trên CÙNG mục -> 409 hoặc idempotent rõ ràng,
    // KHÔNG được ghi thêm một dòng hieu_luc nữa. Quyết định lại được thì phải đi qua giao dịch
    // bình thường (sửa trên web), không phải bấm lại nút cũ.
}
```

Viết đủ mã cho cả năm test, không để thân trống — vi phạm "không placeholder" của writing-plans.

- [ ] **Step 2: Viết `CanXemLaiService`**

`LayDanhSach(giaoXuId)`: lọc `GiaoXuId` **và** `DaXuLyLuc == null`, sắp theo `TaoLuc`.

`ChonGiaTri(id, chon)`: 404 nếu không tìm thấy mục thuộc đúng giáo xứ; 409 nếu `DaXuLyLuc != null`;
400 nếu `Loai` không thuộc `{"o_nhay_cam", "bat_bien"}`. Mở giao dịch tường minh, giành khoá dòng
đếm (`CapSoHieuLuc.LayDaiSo`), lấy giá trị theo `chon` (`"A"` → `GiaTriA`, `"B"` → `GiaTriB`), áp
qua `ApThaoTac.ApMotO` với dấu `(giờ máy chủ hiện tại, logic đã nâng qua `NangDau`, `ThietBiId =
null`, `MaThaoTac = Guid.NewGuid())`, ghi `ThayDoi` + `HieuLuc` (khuôn `GhiCapNhatKy`), cập nhật
`MocO` cho đúng `(Bang, BanGhiId, Truong)`, đặt `DaXuLyLuc`, `GiaTriDangDung = giá trị vừa chọn`.
`NguoiXuLy` để `null` — khoảng trống đã biết giống `tai_khoan_id` của `ThayDoi` (xem
`LuuCoNhatKy.cs`), chưa có chỗ nào dựng `IBoiCanhGhiNhatKy` từ claim người đăng nhập.

`DanhDauDaXuLy(id)`: 404/409 như trên; 400 nếu `Loai` thuộc `{"o_nhay_cam", "bat_bien"}`. Chỉ đặt
`DaXuLyLuc`, không ghi `ThayDoi`/`HieuLuc`/`MocO` nào — không có giá trị nào được quyết định.

- [ ] **Step 3: Chạy test**

Run: `dotnet test WebApp/tests/Qlgx.Api.Tests --filter CanXemLaiTests -o "<thư mục tạm riêng>"`
Expected: PASS.

- [ ] **Step 4: Chứng minh test biết báo lỗi cho fact quan trọng nhất**

Tạm bỏ đoạn ghi `HieuLuc`/`MocO` trong `ChonGiaTri` (chỉ giữ `DaXuLyLuc`). Xác nhận
`Chon_gia_tri_cu_hon_thi_no_phai_THANG_o_lan_dong_bo_sau` đỏ. Hoàn nguyên, xác nhận xanh.

- [ ] **Step 5: Commit**

`git add` từng file cụ thể — không `git add -A`.

---

## Task 8: Bộ kiểm bất biến

**Brief này viết lại toàn bộ, chi tiết hơn hẳn bản gốc — bản gốc chỉ có khung việc, không đủ để
thi công mà không phải đoán.**

**Phần "kiểm RLS bằng vai trò không superuser" ĐÃ CÓ SẴN — không thuộc phạm vi task này nữa.**
`WebApp/tests/Qlgx.Data.Tests/RlsTests.cs` (661 dòng, 8 fact, do một phiên khác làm việc trên cùng
nhánh dựng cho nhu cầu riêng của họ) đã kiểm đúng cơ chế: vai trò `NOSUPERUSER NOBYPASSRLS`, xác
nhận không đặt `app.giao_xu_id` thì trả rỗng (đóng mặc định), kèm hai fact riêng cho đúng hai bảng
của kế hoạch này (`Bang_moc_o_chiu_rls_nhu_bang_nghiep_vu`,
`Bang_nhat_ky_chiu_rls_nhu_bang_nghiep_vu`). Đã chạy lại độc lập: 8/8 xanh. Đừng làm lại.

**Files:**
- Create: `src/Qlgx.Data/DongBo/KiemBatBien.cs`
- Modify: `src/Qlgx.Api/Services/DongBoService.cs`
- Test: `tests/Qlgx.Data.Tests/KiemBatBienTests.cs`

**Interfaces:**
- Produces: `KiemBatBien.Kiem(QlgxDbContext db, Guid giaoXuId, string bang, Guid banGhiId, CancellationToken ct) -> Task<List<string>>` — trả danh sách câu tiếng Việt mô tả từng bất biến bị vi phạm (rỗng nghĩa là sạch). Mỗi câu đã sẵn sàng đi thẳng vào `CanXemLai.LyDo` — viết bằng lời thường, không thuật ngữ kỹ thuật, đúng tinh thần chú thích trong `CanXemLai.cs`.

**Ba bất biến bắt buộc (spec 8.2), CHỈ áp cho đúng bảng của nó — `Kiem` phải tự dispatch theo `bang`, không quét mù:**

1. **`bang == "GiaoDan"`: ngày rửa tội / rước lễ / thêm sức không được trước ngày sinh.** Cột thật:
   `GiaoDan.NgaySinh` (`DateOnly?`), `NgayRuaToi`, `NgayRuocLe`, `NgayThemSuc` (cùng kiểu). Bỏ qua
   cặp nào có một vế `null` — chưa đủ dữ liệu để so, không phải vi phạm. Câu mẫu: *"Ngày rửa tội
   (12/03/1984) sớm hơn ngày sinh (12/03/1985) của {HoTen}."*
2. **`bang == "ThanhVienGiaDinh"`: một `GiaDinhId` phải có đúng một dòng `ChuHo = true`.** Đọc
   `WebApp/src/Qlgx.Domain/Entities/ThanhVienGiaDinh.cs` trước khi viết — đã có chỉ mục lọc riêng
   giữ luật khác (`(GiaDinhId, VaiTro) WHERE VaiTro IN (0,1)` — tối đa một Chồng, một Vợ), **đừng
   lẫn hai luật**. Khi `Kiem` được gọi với `bang == "ThanhVienGiaDinh"` và `banGhiId` là một dòng
   thành viên, tra `GiaDinhId` của dòng đó rồi đếm số dòng `ChuHo = true` trong cùng gia đình:
   `0` hoặc `>= 2` đều vi phạm. Câu mẫu (0): *"Gia đình {TenGiaDinh} hiện không có ai được đánh dấu
   chủ hộ."* Câu mẫu (≥2): *"Gia đình {TenGiaDinh} đang có {n} người cùng được đánh dấu chủ hộ."*
3. **`bang == "GiaoDan"`: `QuaDoi = false` thì `NgayQuaDoi` phải rỗng.** Đơn vị gộp (Task 6,
   `XoaOConLaiCuaNhom`) đã chặn phần lớn ca này khi *chính lần sửa đó* đụng tới `QuaDoi` — bộ kiểm
   là **lưới thứ hai**, bắt cả ca dữ liệu vào sai đường khác (nhập từ Access, sửa tay CSDL, một
   lỗi tương lai ở tầng khác). Câu mẫu: *"{HoTen} được ghi còn sống nhưng vẫn còn ngày qua đời
   (12/03/1984)."*

**CẢNH BÁO ĐẶT TÊN — bản brief gốc của task này định dùng `Loai = "bat_bien"`, nhưng Task 6 đã
CHIẾM tên đó cho một nghĩa khác** (`DongBoService.cs:976` — "biên nhận xoá ô cùng nhóm, giữ giá trị
cũ trong `GiaTriA` để phục hồi", loại CÓ cặp giá trị A/B thật, Task 7 đã xếp vào
`LoaiCoCapGiaTri` và xử lý qua `/chon`). Vi phạm bất biến ở TASK NÀY là loại khác hẳn — không có gì
để "chọn A hay B", chỉ có một câu cảnh báo. Dùng chung tên `"bat_bien"` cho hai nghĩa sẽ làm một
mục do `KiemBatBien` sinh ra đi lạc vào nhánh `/chon` của Task 7 (không có `GiaTriA`/`GiaTriB` thật
để chọn — lỗi hoặc hành vi vô nghĩa ở đó).

**Dùng tên RIÊNG, KHÔNG chứa gốc "bat_bien" để không ai nhầm lẫn khi đọc lướt: `"mau_thuan_du_lieu"`.**
Sinh một mục `CanXemLai { Loai = "mau_thuan_du_lieu", Bang, BanGhiId, LyDo = <câu vi phạm>,
GiaTriA = null, GiaTriB = null, GiaTriDangDung = null, TaoLuc = giờ máy chủ }` — vi phạm KHÔNG làm
hỏng lô (đúng triết lý "hệ thống không bao giờ đứng chờ người dùng trả lời", spec 9.2). Thêm
`"mau_thuan_du_lieu"` vào chú thích liệt kê `Loai` khả dĩ ở `CanXemLai.cs`. Task 7's
`LoaiCoCapGiaTri` KHÔNG cần sửa — danh sách đó là danh sách trắng, loại mới không nằm trong đó thì
tự động chỉ xử lý được qua `/danh-dau-da-xu-ly`, đúng ý.

**Điểm tích hợp vào `DongBoService.ChayLo`:** ngay sau dòng `await db.SaveChangesAsync(ct);` của
Bước 3 (biến `giaoDich` vẫn mở, `db.SaveChangesAsync` vừa flush mọi thay đổi của lô xuống CSDL —
đọc lại từ đây thấy đúng trạng thái sau khi áp, không phải trạng thái đang chờ trong bộ nhớ), **và
trước** `CapSoHieuLuc.ChotDaiSo`/`giaoDich.CommitAsync`. Thu thập tập `(Bang, BanGhiId)` **duy
nhất** từ các thao tác có `ketQua[...].KetQua == "ap"` trong lô này (thao tác `"thua"`/`"tu_choi"`
không cần kiểm — giá trị của chúng chưa hề chạm vào dữ liệu), gọi `KiemBatBien.Kiem` cho từng cặp,
thêm `CanXemLai` cho mỗi câu vi phạm trả về, rồi một `SaveChangesAsync` nữa (vẫn trong cùng giao
dịch) trước khi chốt dải số. **Không mở giao dịch mới, không giành lại khoá dòng đếm** — dùng
đúng giao dịch đang có.

- [ ] **Step 1: Viết test — đủ mã cho cả sáu fact, không placeholder**

```csharp
[Fact]
public async Task Ngay_rua_toi_som_hon_ngay_sinh_bi_bao_bang_cau_de_hieu()
{
    var giaoDan = new GiaoDan { GiaoXuId = giaoXuId, MaGiaoDanCu = 9910, HoTen = "Maria Nguyen Thi A",
        NgaySinh = new DateOnly(1985, 3, 12), NgayRuaToi = new DateOnly(1984, 3, 12) };
    // ... lưu, gọi KiemBatBien.Kiem trực tiếp (không cần qua GuiLen cho test đơn vị này)
    var viPham = await KiemBatBien.Kiem(db, giaoXuId, "GiaoDan", giaoDan.Id, default);
    viPham.Should().ContainSingle(s => s.Contains("Maria Nguyen Thi A") && !s.Contains("NgayRuaToi"),
        "cau phai bang loi thuong, khong duoc chua ten cot ky thuat");
}

[Fact]
public async Task Thieu_du_lieu_mot_ve_thi_khong_bi_bao_vi_pham()
{
    // NgaySinh null, NgayRuaToi có giá trị -> KHÔNG đủ để kết luận, không được báo vi phạm giả.
}

[Fact]
public async Task Gia_dinh_khong_ai_la_chu_ho_bi_bao()
{
}

[Fact]
public async Task Gia_dinh_hai_nguoi_cung_la_chu_ho_bi_bao()
{
}

[Fact]
public async Task Con_song_ma_co_ngay_qua_doi_bi_bao()
{
}

[Fact]
public async Task Gui_len_thao_tac_vi_pham_bat_bien_van_ap_gia_tri_VA_sinh_CanXemLai()
{
    // Kiểm TÍCH HỢP qua endpoint gui-len thật: gửi một thao tác làm QuaDoi=false trong khi
    // NgayQuaDoi vẫn còn (kịch bản không đụng QuaDoi nên đơn vị gộp Task 6 không bắt được) ->
    // giá trị VẪN được áp (spec 9.2: không đứng chờ), VÀ một CanXemLai Loai="mau_thuan_du_lieu"
    // xuất hiện. Đây là ca CHỨNG MINH bộ kiểm là lưới THỨ HAI, không phải lưới duy nhất.
}
```

- [ ] **Step 2: Chạy test — thấy fail**

Run: `dotnet test WebApp/tests/Qlgx.Data.Tests --filter KiemBatBienTests -o "<thư mục tạm riêng>"`

- [ ] **Step 3: Viết `KiemBatBien`, nối vào `DongBoService`**

Theo đúng mô tả điểm tích hợp ở trên.

- [ ] **Step 4: Chạy test — xanh; chứng minh test biết báo lỗi** cho fact tích hợp (Step 1's cuối)
bằng cách tạm bỏ đoạn gọi `KiemBatBien` trong `ChayLo`, xác nhận đỏ, hoàn nguyên, xác nhận xanh.

- [ ] **Step 5: Commit** — `git add` từng file cụ thể.

---

## Task 9: Xoay `epoch` và bù dữ liệu sau khôi phục

**Files:**
- Create: `src/Qlgx.Api/Services/KhoiPhucDongBoService.cs`
- Modify: `src/Qlgx.Api/Dtos/DongBoDtos.cs`, `src/Qlgx.Api/Endpoints/DongBoEndpoints.cs`, `src/Qlgx.Api/Services/DongBoService.cs`
- Test: `tests/Qlgx.Api.Tests/KhoiPhucDongBoTests.cs`

**Đây là task quan trọng nhất của kế hoạch — thiết kế đầy đủ ở spec mục 4.8. Đọc mục đó trước khi viết một dòng nào.**

**Interfaces:**
- Produces:
  - `POST /api/quan-tri/dong-bo/xoay-epoch` (policy `QuanTriHeThong`) nhận `{ giaoXuId?, cheDo }` với `cheDo` ∈ `"lay_lai"` | `"bo_han"`; xoay `epoch` cho một hoặc mọi giáo xứ.
  - Đầu vào nhận-về trả thêm `CheDoKhoiPhuc` khi phát hiện `epoch` lệch, để máy con biết nên bù lại hay bỏ.
  - Đầu vào gửi-lên nhận thao tác **bù lại** (có `NguonGocEpoch`/`NguonGocSoThuTu`) và chống trùng theo danh tính gốc.

- [ ] **Step 1: Viết test trước — năm kịch bản của mục 4.8**

`tests/Qlgx.Api.Tests/KhoiPhucDongBoTests.cs`:

```csharp
    [Fact]
    public async Task Xoay_epoch_lam_con_tro_cu_bi_tu_choi_410() { }

    [Fact]
    public async Task Che_do_lay_lai_chap_nhan_thao_tac_bu_va_khoi_phuc_du_lieu_da_mat()
    {
        // Dựng: giáo dân X có HoTen = "Ten Sau Khoi Phuc" ở trạng thái "đã khôi phục" (giả lập
        // máy chủ lùi). Máy con giữ dòng hieu_luc cũ nói HoTen = "Ten That" với so_thu_tu 5000
        // thuộc epoch cũ. Sau khi xoay epoch ở chế độ lay_lai, máy con gửi thao tác bù mang
        // NguonGocEpoch/NguonGocSoThuTu. Khẳng định dữ liệu quay về "Ten That".
    }

    [Fact]
    public async Task Nhieu_may_con_cung_bu_mot_dong_goc_thi_chi_ap_mot_lan()
    {
        // Hai lô gửi lên từ hai ThietBiId khác nhau, cùng NguonGocEpoch + NguonGocSoThuTu,
        // MaThaoTac khác nhau. Khẳng định chỉ sinh MỘT dòng hieu_luc mới.
    }

    [Fact]
    public async Task Che_do_bo_han_tu_choi_thao_tac_bu()
    {
        // Quay lui có chủ ý: máy con gửi bù, máy chủ phải TỪ CHỐI (ket qua "tu_choi") — nếu
        // chấp nhận thì thao tác quay lui của quản trị viên bị vô hiệu hoá.
    }

    [Fact]
    public async Task Thao_tac_bu_giu_nguyen_moc_goc_nen_thua_nguoi_da_sua_sau_khoi_phuc()
    {
        // Sau khôi phục có người sửa ô đó trên máy chủ (mốc mới). Dòng bù mang mốc GỐC (cũ hơn)
        // nên thua — nhưng vì là ô nhạy cảm nên PHẢI sinh một mục cần xem lại.
    }
```

Run: FAIL.

- [ ] **Step 2: Viết `KhoiPhucDongBoService`**

```csharp
/// <summary>
/// Xoay epoch và điều phối việc bù lại dữ liệu sau khi máy chủ bị đưa về bản sao lưu.
/// Thiết kế đầy đủ ở spec mục 4.8 — đọc trước khi sửa.
///
/// Vì sao BẮT BUỘC có tham số chế độ, không có mặc định: máy chủ không thể tự đoán đang ở
/// trường hợp nào trong hai trường hợp ngược nhau —
///   - khôi phục sau sự cố: MUỐN máy con đẩy dữ liệu đã mất trở lên;
///   - quay lui có chủ ý (ai đó làm hỏng dữ liệu, quản trị viên lùi để BỎ HẲN): tuyệt đối
///     KHÔNG được để máy con tự đẩy đống hỏng đó ngược lên, làm thế là vô hiệu hoá chính thao
///     tác quay lui.
/// Đoán sai thì hỏng theo hai kiểu ngược nhau, nên đây là một trong rất ít chỗ trong hệ thống
/// buộc con người phải quyết định.
/// </summary>
```

Nội dung: cập nhật `bo_dem_hieu_luc.epoch = gen_random_uuid()` và ghi chế độ vào một bảng/cột nhỏ (`cho_phep_bu_lai boolean` trên `bo_dem_hieu_luc`, kèm `xoay_epoch_luc`). Đầu vào nhận-về đọc cờ này để trả về cho máy con.

Trong `DongBoService.GuiLen`, khi thao tác có `NguonGocEpoch`:
- nếu `cho_phep_bu_lai == false` → `KetQua = "tu_choi"`, thông báo *"Máy chủ đang ở chế độ quay lui có chủ ý — không nhận dữ liệu bù."*
- nếu đã có dòng `thao_tac_da_nhan` với cùng `(GiaoXuId, NguonGocEpoch, NguonGocSoThuTu)` → `"trung"`.
- ngược lại xử lý như thao tác thường **nhưng giữ nguyên mốc gốc** (không hiệu chỉnh theo độ lệch đồng hồ của máy con hiện tại — mốc gốc đã ở hệ quy chiếu máy chủ từ lần đầu).

- [ ] **Step 3: Lưới an toàn lớp 2 — phát hiện `so_thu_tu` đi lùi**

Trong đầu vào nhận-về: nếu `tu` (con trỏ máy con) **lớn hơn** số lớn nhất hiện có mà `epoch` **vẫn khớp** → trả 409 kèm mã `"may_chu_di_lui"`. Đây là trường hợp ai đó khôi phục CSDL bằng tay mà quên xoay `epoch`; lớp 1 im lặng nên phải có lớp 2.

Viết một fact riêng cho tình huống này.

- [ ] **Step 4: Chạy test, chứng minh test chế độ `bo_han` biết báo lỗi**

Tạm cho `GuiLen` chấp nhận thao tác bù bất kể chế độ.
Run: `--filter Che_do_bo_han_tu_choi`
Expected: **FAIL**. Hoàn nguyên.

- [ ] **Step 5: Commit**

```bash
git add WebApp/src/Qlgx.Api/Services/KhoiPhucDongBoService.cs \
        WebApp/src/Qlgx.Api/Services/DongBoService.cs \
        WebApp/src/Qlgx.Api/Dtos/DongBoDtos.cs \
        WebApp/src/Qlgx.Api/Endpoints/DongBoEndpoints.cs \
        WebApp/src/Qlgx.Data/ \
        WebApp/tests/Qlgx.Api.Tests/KhoiPhucDongBoTests.cs
git commit -m "Xoay epoch va bu du lieu sau khoi phuc (spec 4.8)"
```

---

## Task 10: Kiểm chứng cả giao thức bằng một kịch bản liền mạch

**Files:**
- Test: `tests/Qlgx.Api.Tests/DongBoKichBanTests.cs`

Một fact duy nhất, dài, mô phỏng **hai máy con** nói chuyện với máy chủ qua đúng các đầu vào HTTP thật — không gọi tắt vào service:

1. Máy A tải toàn bộ, nhận con trỏ.
2. Máy B tải toàn bộ, nhận cùng con trỏ.
3. Máy A sửa số điện thoại của giáo dân X, gửi lên.
4. Máy B sửa **địa chỉ** của cùng giáo dân X (ô khác), gửi lên.
5. Cả hai nhận về → **cả hai thay đổi cùng tồn tại** (gộp mức trường hoạt động).
6. Máy A và máy B cùng sửa **ngày sinh** của X thành hai giá trị khác nhau, B gửi sau.
7. Kết quả: giá trị của B được dùng, và có **đúng một** mục cần xem lại.
8. Người dùng chọn giá trị của A qua `POST /api/can-xem-lai/{id}/chon`.
9. Máy B nhận về → thấy ngày sinh đã đổi về giá trị của A (lựa chọn của người thắng vì mốc hiện tại).
10. Gửi lại nguyên lô của bước 6 (mô phỏng mạng chập) → không thay đổi gì thêm, không sinh mục xem lại thứ hai.

Fact này là **lưới an toàn cuối cùng** của cả kế hoạch: nó bắt được mọi lỗi tích hợp giữa chín task trên mà từng test đơn lẻ bỏ sót.

Run: `dotnet test WebApp/tests/Qlgx.Api.Tests --filter DongBoKichBanTests`

```bash
git add WebApp/tests/Qlgx.Api.Tests/DongBoKichBanTests.cs
git commit -m "Them kich ban lien mach hai may con cho toan bo giao thuc dong bo"
```

---

## Ngoài phạm vi kế hoạch này

| Việc | Kế hoạch |
|---|---|
| Kho IndexedDB, sổ đã nhận, hàng chờ, thanh trạng thái, file dự phòng, bầu chủ một tab | 5 |
| Giao diện hộp cần xem lại (ở đây chỉ có đường API) | 5 |
| Xoá mềm toàn diện — tới khi có, nhật ký vẫn thiếu thao tác xoá | 2 |
| Bảng `thiet_bi`, vé dài hạn, cờ offline | 3 |
| Xoay `epoch` khi nhập dữ liệu Access đè lên giáo xứ đang dùng web | 6 |
