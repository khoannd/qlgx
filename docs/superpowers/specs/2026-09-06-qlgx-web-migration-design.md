# Thiết kế chuyển đổi QLGX từ WinForms sang nền tảng web

Ngày chốt: 2026-09-06

## Mục tiêu

Đưa phần mềm quản lý giáo xứ (QLGX) từ ứng dụng WinForms .NET Framework 4.8 + MS Access
sang ứng dụng web, **giữ nguyên trải nghiệm mà nhân viên văn phòng giáo xứ đã quen** —
đặc biệt là lưới dữ liệu và các form nhập liệu — đồng thời giải quyết hai điểm nghẽn lớn
nhất của bản hiện tại: Access không phục vụ tốt nhiều người dùng đồng thời, và toàn bộ
báo cáo phụ thuộc Microsoft Office Interop.

## Hiện trạng (kết quả khảo sát mã nguồn)

| Hạng mục | Hiện trạng |
|---|---|
| Quy mô | ~90.000 dòng C#, 147 file `*.Designer.cs`, 42 form nghiệp vụ cấp cao |
| Nền tảng | .NET Framework 4.8, build **x86** (ràng buộc driver ACE OLEDB 32-bit) |
| CSDL | MS Access (`giaoxu.mdb`) đặt cạnh file exe, ~27 bảng nghiệp vụ |
| Truy cập dữ liệu | `System.Data.OleDb` thuần, SQL viết tay trong `SqlConstants.cs`, không ORM |
| Phân lớp | Không có tầng nghiệp vụ tách bạch; logic nằm trong form, user control và lớp `Memory` (2114 dòng) |
| Giao diện | Bộ điều khiển thương mại **Janus WinForms Suite v3.5** (đời 2008–2010): GridEX, UIPanelManager, ExplorerBar |
| Báo cáo | **Office Interop** (Word + Excel) — bắt buộc máy chạy phải cài Office |
| Mô hình triển khai | Mỗi giáo xứ một bản cài độc lập, CSDL riêng cạnh exe; đã có định danh `MaGiaoXuRieng` cấp qua API `qlgx.net` |
| Kiểm thử tự động | Không có |

Bốn rủi ro lớn nhất khi chuyển đổi, theo thứ tự:

1. **Office Interop không chạy được trên máy chủ** — phải viết lại toàn bộ báo cáo.
2. **Janus GridEX không có bản tương đương trên web** — phải dựng lại trải nghiệm lưới.
3. **SQL đặc thù Access** (`IIF`, subquery lồng nhiều tầng sinh cột phái sinh) — không dịch 1:1 được.
4. **Logic nghiệp vụ trộn trong giao diện** — không thể "nâng cấp tại chỗ", phải tách tầng.

## Các quyết định đã chốt

### 1. Mô hình triển khai

On-prem, **mỗi giáo xứ một instance độc lập** — giữ đúng tinh thần bản desktop:

```
[Trình duyệt trên các PC trong LAN văn phòng giáo xứ]
        │  HTTP/HTTPS nội bộ
        ▼
[ASP.NET Core (.NET 8) — Web API + phục vụ static file của SPA]   ← chạy như Windows Service
        │  EF Core + Npgsql
        ▼
[PostgreSQL cài cục bộ]  ← một database riêng cho giáo xứ đó
```

- Một máy trong văn phòng đóng vai trò máy chủ; các máy khác truy cập qua LAN. **Nhiều người
  dùng đồng thời** là lợi ích chính so với bản Access hiện tại.
- Không phụ thuộc internet để dùng hằng ngày. Server `qlgx.net` hiện có tiếp tục dùng cho
  cập nhật phiên bản và sao lưu đám mây tuỳ chọn.
- Backend target .NET 8 nên **chạy được cả Linux** — cần thiết cho kịch bản gom cụm ở mục 2.

### 2. CSDL sẵn sàng gom cụm về sau

PostgreSQL + EF Core (code-first). Bốn quy tắc bắt buộc từ ngày đầu để sau này gộp nhiều
giáo xứ vào một database mà không phải đánh số lại dữ liệu:

1. **Khoá chính là UUID**, không dùng số tự tăng. Hai giáo xứ độc lập đều có thể sinh
   `id = 105`; gộp lại sẽ đụng khoá và phải ánh xạ lại toàn bộ khoá ngoại. UUID sinh ở đâu
   cũng không trùng nhau nên việc gộp chỉ là `INSERT ... SELECT`.
2. **Mọi bảng nghiệp vụ có cột `giao_xu_id`**, kể cả khi mỗi database hiện chỉ chứa một
   giáo xứ. Giá trị lấy từ `MaGiaoXuRieng` đã có sẵn — không phải phát minh định danh mới.
3. **Mọi truy vấn nghiệp vụ lọc theo `giao_xu_id`**, kể cả khi dư thừa. Khi gộp cụm chỉ cần
   bật Row-Level Security theo cột này, không phải viết lại tầng truy cập dữ liệu.
4. **Giữ đủ trường định danh tự nhiên và cột nguồn gốc** (`created_at`, `source_system`) để
   sau này chạy được đối chiếu trùng lặp — một giáo dân có thể xuất hiện ở nhiều giáo xứ do
   chuyển xứ. Logic đối chiếu làm sau; dữ liệu để đối chiếu phải có từ bây giờ.

### 3. Giao diện

- **React + TypeScript SPA + AG Grid Community**, backend là **ASP.NET Core Web API** thuần.
- Lưới dùng **client-side row model**: API trả nguyên mảng, AG Grid lọc/sắp xếp/nhóm trong
  trình duyệt. Đây chính là cách bản desktop đang chạy (Janus GridEX cũng nạp cả `DataTable`
  vào bộ nhớ rồi lọc tại chỗ) nên không phải đánh đổi hiệu năng, chỉ đổi nơi chạy.
- Bắt đầu bằng **AG Grid Community (miễn phí)**; nếu sau pilot thấy thiếu kéo-thả nhóm hoặc
  set filter thì nâng cấp Enterprise sau, không phải làm lại.
- **Lưu bố cục cột theo người dùng** thay cho file `GridColumns.xml` hiện tại: một bảng
  `UserGridPreference(UserId, GridKey, StateJson)` lưu kết quả `columnApi.getColumnState()`.
- **Chống ghi đè khi nhiều người cùng sửa**: thêm `RowVersion` (optimistic concurrency của
  EF Core). Đây là rủi ro **mới** phát sinh khi bỏ Access — `OleDbCommandBuilder` hiện tại
  sinh câu UPDATE không kiểm tra phiên bản.

### 4. Tầng component dùng lại

Bản desktop đã tách đúng: `GxGrid : GridEX` là lớp cơ sở, `GxGiaoDanList : GxGrid` là
UserControl hoàn chỉnh tự sở hữu bộ cột, menu chuột phải và hành vi. Nhờ vậy `frmGiaDinh`
chỉ cần chèn thêm một cột là có ngay lưới thành viên gia đình.

**Bản web phải giữ đúng kiến trúc đó** — đây là điều kiện để kiểm thử một lần dùng được
nhiều nơi: nếu lưới giáo dân trong màn hình danh sách đã kiểm thử xong thì lưới thành viên
gia đình chỉ còn phải kiểm lại dữ liệu.

| Component web | Tương đương bản desktop | Nơi dùng lại |
|---|---|---|
| `GxGrid` | `GxGrid : GridEX` | mọi lưới |
| `GxGiaoDanList` | `GxGiaoDanList` | danh sách giáo dân · lưới thành viên trong form gia đình |
| `GxGiaDinhList` | `GxGiaDinhList` | danh sách gia đình |
| `GxHonPhoiList` | `GxHonPhoiList` | tab Hôn phối của form giáo dân |
| `GxHoiDoanList` | `GxListHistoryHoiDoan` | tab Hội đoàn của form giáo dân |
| `GxFormTabs` | `tabControl1` của `frmGiaoDan` | form giáo dân |
| `GxPicker` | UserControl `GxGiaoDan` | chọn chồng/vợ, cha/mẹ, linh mục |
| Thẻ tài liệu | `FATabStrip` + `dicShows` của `frmMain` | khung ứng dụng |

Bản mẫu giao diện đã dựng theo đúng cấu trúc này: `WebApp/prototype/qlgx-prototype.html`.

### 5. Báo cáo

| Loại | Cách làm | Lý do |
|---|---|---|
| Chứng nhận, chứng chỉ | **Template HTML có biến** (Scriban) → **Playwright headless Chromium** in ra PDF | Xem trước mẫu ngay trong trình duyệt; sửa mẫu không cần build lại; chạy được Linux |
| Bảng số liệu, sổ sách, thống kê | **ClosedXML** xuất `.xlsx` thật | Giáo xứ cần lọc/xử lý tiếp trên Excel |

Tuyệt đối không dùng Office Interop ở tầng máy chủ.

### 6. Chuyển dữ liệu từ Access

- Một **console app riêng, chỉ chạy trên Windows**, được phép phụ thuộc OleDb + Access
  Database Engine. **Backend production không đụng tới Access.**
- Đọc bằng OleDb, ghi bằng EF Core. Giữ bảng ánh xạ tạm `mã cũ (số) → UUID mới` để giải
  quyết khoá ngoại theo đúng thứ tự phụ thuộc: GiaoXu → GiaoHo → GiaDinh → GiaoDan →
  ThanhVienGiaDinh → HonPhoi.
- Có chế độ **chạy thử** (dry-run) xuất báo cáo số dòng từng bảng và cảnh báo dữ liệu bất
  thường trước khi ghi thật, **chạy lại nhiều lần không sinh dữ liệu trùng**, và bước
  **đối chiếu** số dòng giữa nguồn và đích sau khi chạy.

### 7. Lộ trình

Thí điểm một giáo xứ trước. Các giáo xứ khác tiếp tục dùng bản desktop cho đến khi bản thí
điểm chạy ổn định với dữ liệu thật.

- **Phase 1** — bốn màn hình lõi: danh sách gia đình, chi tiết gia đình, danh sách giáo dân,
  chi tiết giáo dân. Kèm nền tảng: schema, API, xác thực, tool chuyển dữ liệu, đóng gói cài đặt.
- **Phase 2** — bí tích và sổ bí tích, hôn phối và rao hôn phối, hội đoàn.
- **Phase 3** — thống kê, biểu đồ, báo cáo và chứng chỉ, công cụ dữ liệu (kiểm tra, chuẩn hoá,
  chuyển họ hàng loạt), giáo lý.

## Ràng buộc chung của dự án

- Backend: .NET 8, ASP.NET Core, EF Core + Npgsql. Không phụ thuộc thư viện Windows-only
  (`System.Drawing.Common`, Office Interop, OleDb) — trừ tool chuyển dữ liệu một lần.
- CSDL: PostgreSQL 16 trở lên.
- Frontend: React 18 + TypeScript, Vite, AG Grid Community.
- Mọi bảng nghiệp vụ: khoá chính `uuid`, có `giao_xu_id uuid`, `created_at`, `updated_at`,
  `row_version` và `source_system`.
- Toàn bộ nhãn giao diện giữ nguyên tiếng Việt như bản desktop, không dịch lại, không đặt lại.
- Ngoại lệ duy nhất về nhãn: caption `"&Xem gia đinh"` trong `frmGiaoDan` thiếu dấu — bản web
  ghi đúng chính tả `"Xem gia đình"`.

## Ngoài phạm vi giai đoạn này

- Gom nhiều giáo xứ vào một database dùng chung, và logic đối chiếu trùng lặp giáo dân giữa
  các giáo xứ. Schema chuẩn bị sẵn (mục 2) nhưng chưa xây dựng.
- Nâng cấp AG Grid Enterprise.
- Ứng dụng di động.
