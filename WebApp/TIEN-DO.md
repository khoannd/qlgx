# Tiến độ chuyển đổi QLGX sang nền tảng web

Cập nhật: 2026-09-06 — **hệ thống đã chạy được đầu-cuối với dữ liệu giáo xứ thật**.
Nhánh làm việc: **`webapp-phase-1`** (tách từ `master`).

## Cách tiếp tục sau khi khởi động lại

1. `git checkout webapp-phase-1`
2. Kiểm tra biến môi trường kết nối CSDL còn không:
   `echo %QLGX_TEST_PG%` (PowerShell: `$env:QLGX_TEST_PG`)
   Định dạng: `Host=localhost;Username=postgres;Password=<mật khẩu PostgreSQL của bạn>`
   Nếu mất, đặt lại: `setx QLGX_TEST_PG "Host=localhost;Username=postgres;Password=<mật khẩu>"`
   **Không ghi mật khẩu thật vào bất kỳ file nào trong repo** — chỉ đặt qua biến môi trường.
   (biến đặt bằng `setx` chỉ có hiệu lực ở cửa sổ dòng lệnh **mới mở**)
3. Kiểm tra dịch vụ `postgresql-x64-17` đã chạy chưa: `sc query postgresql-x64-17`
4. Chạy toàn bộ test để xác nhận môi trường lành lặn:
   - Backend: `dotnet test WebApp/Qlgx.sln` → phải ra **188/188 xanh**
   - Front-end: `cd WebApp/src/web && npm test -- --run` → phải ra **205/205 xanh**
5. Sổ theo dõi chi tiết từng task, từng quyết định:
   `.superpowers/sdd/2026-09-06-qlgx-web-phase-1/progress.md`
   (thư mục này nằm ngoài git, nhưng vẫn còn trên đĩa sau khi khởi động lại)
6. Task tiếp theo cần làm: **Task 14 — xác thực và phân tách tenant theo claim đăng nhập**.

## Cách chạy thử hệ thống với dữ liệu thật

Dữ liệu thật của giáo xứ "Vô Nhiễm" đã nằm sẵn trong database `qlgx_thu`.

```
# 1. Máy chủ API
dotnet run --project WebApp/src/Qlgx.Api

# 2. Giao diện web (cửa sổ khác)
cd WebApp/src/web && npm run dev
```

Chuyển lại dữ liệu từ đầu (nếu cần dựng database mới):

```
# mật khẩu file .mdb nằm sẵn trong Source/Giaoly/app.config của bản desktop cũ
export QLGX_MDB_PASSWORD=<mật khẩu file mdb>
dotnet WebApp/src/Qlgx.Migration/bin/Debug/net10.0-windows/Qlgx.Migration.dll   "d:/Working/QLGX/Github/BIN/giaoxu.mdb" "$QLGX_TEST_PG;Database=qlgx_thu"   "00000000-0000-0000-0000-0000000000aa" --chay-that
```

Bỏ `--chay-that` để chạy thử: chỉ đọc và in báo cáo đối chiếu, không ghi gì.
Công cụ chạy lại được nhiều lần mà không tạo bản ghi trùng.

## Trạng thái: GIAI ĐOẠN 1 ĐÃ XONG PHẦN CÀI ĐẶT

Toàn bộ 17 task của giai đoạn 1 đã cài đặt xong, kể cả xác thực, PWA, RLS, Docker và
kiểm thử đầu-cuối. Hai đợt review độc lập (backend + front-end) đã chạy và các phát hiện
đã được sửa.

**Nhưng bản web CHƯA thay thế được hoàn toàn bản desktop** — xem mục "Còn thiếu để bỏ hẳn
bản desktop" ở cuối tài liệu này.

## Tài liệu chi phối

| Tài liệu | Vai trò |
|---|---|
| `docs/superpowers/specs/2026-09-06-qlgx-web-migration-design.md` | Thiết kế — thẩm quyền ràng buộc cao nhất |
| `docs/superpowers/plans/2026-09-06-qlgx-web-phase-1.md` | Kế hoạch 13 task, có mục "Sửa đổi 2026-09-06" ở đầu **có hiệu lực cao hơn** mô tả từng task |
| `WebApp/prototype/qlgx-prototype.html` | Bản mẫu giao diện đã duyệt, dùng để đối chiếu bố cục và nhãn |

## Môi trường

| Thứ | Trạng thái |
|---|---|
| .NET SDK | **10.0.400** (đã nâng từ 9; toàn bộ project target `net10.0`) |
| Node / npm | v22.17.1 / 10.9.2 |
| PostgreSQL | **17.5**, dịch vụ `postgresql-x64-17`, user `postgres`; mật khẩu lấy từ biến `QLGX_TEST_PG`, không ghi trong repo |
| `psql` | ở `C:\Program Files\PostgreSQL\17\bin`, **không có trong PATH** |

## Đã hoàn thành

| Task | Nội dung | Test |
|---|---|---|
| 1 | Khởi tạo solution `WebApp/`, endpoint sức khoẻ | 1 |
| 2 | Chuyển đổi ngày `dd/MM/yyyy` kiểu Access, giữ lại giá trị hỏng | 10 |
| 3 | Thực thể nghiệp vụ, `GiaoDan` đủ 63 cột | 3 |
| — | Nâng toàn bộ lên .NET 10 | — |
| 4 | `DbContext`, ánh xạ schema, migration đầu tiên, **2 bảng hôn phối** | 6 |
| 5 | Bối cảnh giáo xứ và bộ lọc dữ liệu toàn cục | 3 |
| 6 | API danh sách gia đình, 6 cột dẫn xuất | 13 |
| 10 | Khung SPA: thanh trên, điều hướng, thẻ tài liệu nhiều tab | 5 |
| 11 | Tầng lưới dùng lại `GxGrid` / `GxGiaoDanList` / `GxGiaDinhList` | 12 |
| 12 | Bốn màn hình nghiệp vụ | 22 |
| 7 | API chi tiết gia đình, chống ghi đè bằng `xmin` | 13 |
| 8 | API giáo dân: danh sách, chi tiết, cập nhật, thành viên | 12 |
| 9 | Công cụ chuyển dữ liệu Access → PostgreSQL, đủ cột 7 bảng | 10 |
| — | Nối front-end vào API thật, bỏ dữ liệu giả | 10 |
| 17 | **Đủ 26 bảng Access** — 19 bảng còn lại, chia ba nhóm | 14 |

| — | Hôn phối, tận hiến, hội đoàn: 3 tab đọc/ghi thật | 32 |
| — | Thao tác ghi giáo dân: tạo mới, xoá, kiểm tra nghiệp vụ máy chủ | 34 |

Tổng: **188 test backend + 205 test front-end + 7 test đầu-cuối**, tất cả xanh.
`npm run build` chạy được.

### Quy tắc nghiệp vụ giáo dân CHƯA tái hiện (mục 19 `can-review-sau.md`)

Sáu quy tắc trong spec chưa có ở bản web — không phải quyết định có chủ đích, là việc còn nợ:
- **Rule 8** (cảnh báo lệch giáo họ) và **Rule 15** (tuổi cha/mẹ ≥ con + 15): bị chặn vì
  Tên Cha/Mẹ và Giáo họ hiện là ô văn bản/danh mục tạm, chưa có **picker chọn giáo dân thật**.
  Picker này cũng là thứ màn hình gia đình cần (chọn Người nam/Người nữ) → làm chung.
- **Rule 18** (không bỏ tick "Có gia đình" khi còn hôn phối), **Rule 22/23** (tự sửa
  `DaCoGiaDinh` của vợ/chồng còn lại khi tick Qua đời): cần đọc `HonPhoi` — làm sau picker.
- **Rule 11** + khối chuyển xứ + tab Giáo lý: các trường chưa có trong request tạo/sửa.

### Dữ liệu mẫu tự tạo trong `qlgx_thu` — KHÔNG phải dữ liệu thật

Ba bảng `TanHien`, `HoiDoan`, `ChiTietHoiDoan` **rỗng hoàn toàn** trong file Access gốc (giáo xứ
Vô Nhiễm chưa dùng hai chức năng này). Để kiểm thử được hai tab trên trình duyệt, đã tự tạo
**5 dòng giả** bằng `psql`: `tan_hien` 1 dòng, `hoi_doan` 2 dòng, `chi_tiet_hoi_doan` 2 dòng
(hội đoàn "Gia trưởng" và "Legio Mariae").

**Đừng nhầm đây là dữ liệu giáo xứ thật.** Xoá đi khi không cần nữa:

```sql
DELETE FROM chi_tiet_hoi_doan; DELETE FROM hoi_doan; DELETE FROM tan_hien;
```

Mọi bảng khác trong `qlgx_thu` đều là dữ liệu thật chuyển từ Access.

## Đã chuyển được dữ liệu thật

Từ `BIN/giaoxu.mdb` (giáo xứ Vô Nhiễm) sang PostgreSQL, số dòng khớp tuyệt đối:

| Bảng | Số dòng |
|---|---|
| GiaoXu | 1 |
| GiaoHo | 1 |
| GiaDinh | 40 |
| GiaoDan | 2 050 |
| ThanhVienGiaDinh | 145 |
| HonPhoi | 522 |
| GiaoDanHonPhoi | 1 043 |

Đã phủ **đủ 26/26 bảng Access** (2 view là dữ liệu dẫn xuất — ngoại lệ đã ghi nhận).
Ngoài 7 bảng trên còn có: GiaoPhan 1 · GiaoHat 1 · CauHinh 19 · DuLieuChung 343 · VaiTro 3 ·
TenLoaiTaiKhoan 3 · TaiKhoan 0 · **DotBiTich 1 108** · **BiTichChiTiet 6 150** · ChuyenXu 0 ·
RaoHonPhoi 0 · TanHien 0 · LinhMuc 0 · KhoiGiaoLy 0 · LopGiaoLy 0 · ChiTietLopGiaoLy 0 ·
GiaoLyVien 0 · HoiDoan 0 · ChiTietHoiDoan 0. Bảng rỗng vẫn được tạo đủ cột vì giáo xứ khác
sẽ có dữ liệu.

Phân cấp thật đã nối được ba cấp: **Giáo phận Phan Thiết → Giáo hạt Đức Tánh → Giáo xứ Vô
Nhiễm**. `GiaoPhan` và `GiaoHat` cố ý **không** gắn `giao_xu_id` và không có bộ lọc tenant vì
chúng nằm trên cấp giáo xứ — đây là nền cho chức năng quản lý giáo xứ theo giáo phận.

Cả 7 bảng cốt lõi chuyển **đủ mọi cột** (GiaoDan 63/63, GiaDinh 17/17, GiaoXu 11/11 …). `MaNhanDang`
được giữ nguyên trên 4 bảng có cột này để sau còn đồng bộ hai chiều desktop↔web. Giá trị
`VaiTro` giữ nguyên dạng thô (0,1,2,3,8,18,100) — gộp lại sẽ đụng khoá chính và mất dòng.
Ngày tháng hỏng (chỉ ghi năm, chuỗi rỗng) được giữ nguyên văn trong cột `du_lieu_loi`.

Ảnh chụp màn hình chạy thật: `WebApp/anh-chup-kiem-thu/`.

## Còn lại của giai đoạn 1

| Task | Nội dung | Ghi chú |
|---|---|---|
| 14 | Xác thực và phân tách tenant theo claim đăng nhập | **xong (2026-09-07)** — xem mục dưới |
| 16 | PWA và bản nháp ngoại tuyến | **xong (2026-09-07)** — xem mục dưới |
| 13 | Kiểm thử đầu-cuối và **triển khai máy chủ** | **xong (2026-09-07)** — xem mục dưới |
| 15 | Giao diện hôn phối | task mới |

## Task 13 — Kiểm thử đầu-cuối và triển khai máy chủ (2026-09-07)

**Row-Level Security đã bật thật** (migration `BatRlsChoBangTheoGiaoXu`, 22 bảng có
`giao_xu_id`) — nợ ghi ở Task 14 (`can-review-sau.md` mục 27m) coi như đã trả. Chính sách so
sánh `giao_xu_id::text = current_setting('app.giao_xu_id', true)` (ép kiểu chiều CỘT sang text,
không ép ngược — ép `''::uuid` khi chưa đặt tham số ném lỗi thay vì trả `NULL`, xem
`can-review-sau.md` mục 29d), đóng mặc định (không đặt tham số phiên = không đọc được dòng
nào). `BoiCanhGiaoXuConnectionInterceptor` (mới, `Qlgx.Data`) tự đặt tham số phiên
`app.giao_xu_id` mỗi lần mở kết nối, gắn qua `QlgxDbContext.OnConfiguring`. Hai vai trò CSDL
cần thiết cho triển khai thật: `qlgx_app` (không `BYPASSRLS`, dùng cho nghiệp vụ hằng ngày) và
`qlgx_admin` (có `BYPASSRLS`, chỉ cho đăng nhập/tạo tài khoản quản trị/chuyển dữ liệu — khoá
cấu hình mới `ConnectionStrings:QlgxQuanTri`, fallback về `ConnectionStrings:Qlgx` nếu không
đặt). **Bằng chứng RLS có tác dụng thật:** `RlsTests.cs` (2 test mới, `Qlgx.Data.Tests`) dùng
Npgsql thô + một vai trò CSDL không `BYPASSRLS` tạo/xoá ngay trong test — chứng minh giáo xứ A
không đọc được dòng của giáo xứ B, và một kết nối không đặt tham số phiên không đọc được gì
(đóng mặc định), cả hai đều KHÔNG đi qua EF Core.

**Triển khai Docker đã kiểm chứng THẬT** (không chỉ đọc code rồi suy luận) — `WebApp/Dockerfile`
(một image cho cả API và web, multi-stage), `WebApp/docker-compose.yml` (API + PostgreSQL),
`.env.example` (không giá trị thật). Đã build + `docker compose up` với `.env` thử nghiệm, xác
nhận `/api/suc-khoe` trả `ok`, migration (kể cả RLS) tự chạy đúng lúc khởi động (khoá bằng
`pg_advisory_lock`, chỉ bật qua cờ `Qlgx__ChayMigrationKhiKhoiDong=true` — TẮT mặc định để
không phá `WebApplicationFactory` không có CSDL thật của test cũ, xem mục 29c), `relrowsecurity
= t` xác nhận RLS bật thật trong container, route tĩnh trả về SPA đã build. Đã dọn sạch
container/volume/image thử nghiệm sau khi kiểm chứng.

**Bộ e2e Playwright thật (`WebApp/e2e`, npm project riêng), 7/7 test xanh, chạy ổn định qua hai
lần lặp lại liên tiếp:** đăng nhập sai → thông báo tiếng Việt; chưa đăng nhập/token bị xoá →
quay về màn hình đăng nhập; tạo giáo dân → sửa → lưu → tải lại thấy giá trị mới; tạo giáo dân →
thấy trong danh sách → xoá; mở gia đình mới tạo → thêm/xoá thành viên; vi phạm quy tắc nghiệp vụ
(xoá vĩnh viễn giáo dân đang thuộc gia đình) → thông báo tiếng Việt đúng nguyên văn, không phải
lỗi 500. Bộ e2e tự tạo MỘT database riêng (`qlgx_e2e_<hex ngẫu nhiên>`) qua `dotnet ef database
update`, tự tạo tài khoản quản trị bằng đúng lệnh vận hành thật (`dotnet run --
tao-tai-khoan-quan-tri`), và tự xoá database khi xong (`globalTeardown.ts`) — **không đụng
`qlgx_thu`**, đã xác nhận sau mỗi lần chạy không còn database `qlgx_e2e_*` nào sót lại.

**Sự cố đáng chú ý lúc viết bộ e2e — CHƯA DỌN XONG, cần người có quyền xử lý:** một lần chạy
`dotnet ef database update` bị cấu hình sai biến môi trường đã vô tình tạo 28 bảng RỖNG (đã xác
nhận từng bảng 0 dòng) trong database `postgres` mặc định của cụm PostgreSQL trên máy chuẩn bị
tài liệu này — gốc rễ đã sửa (xem `can-review-sau.md` mục 29j để biết chi tiết và tại sao),
nhưng lệnh dọn dẹp (`DROP TABLE ... CASCADE`) bị hệ thống permission của phiên làm việc chặn cả
qua `psql` lẫn `dotnet ef database update 0`, và đã CHỦ ĐỘNG KHÔNG lách qua công cụ khác để né
chặn đó. Danh sách 28 bảng cần xoá nằm ở `can-review-sau.md` mục 29j.

**`TRIEN-KHAI.md` (mới, ở gốc `WebApp/`)** — hướng dẫn vận hành tiếng Việt: yêu cầu máy chủ,
biến môi trường, các bước triển khai, tạo tài khoản quản trị đầu tiên, thêm giáo xứ mới, chuyển
dữ liệu Access, sao lưu/phục hồi, nâng cấp phiên bản, và mục riêng "chưa kiểm chứng được" (nêu
rõ: chưa thử managed PostgreSQL, chưa thử HA nhiều bản API thật, chưa dựng reverse proxy thật).

**Chưa kiểm chứng được trên máy này (nói thẳng, không giả vờ):** HA thật với nhiều bản API sau
một bộ cân bằng tải; PostgreSQL quản lý (managed service, ví dụ RDS/Cloud SQL) — cách tạo vai
trò `BYPASSRLS` có thể khác trên các dịch vụ đó; reverse proxy/HTTPS thật (chỉ có cấu hình mẫu).

Test hai phía không đổi số so với Task 16 ở phần .NET/React thuần (172 .NET, 183 React vẫn
xanh) — Task 13 CHỈ thêm 2 test .NET mới (`RlsTests.cs`) nên tổng backend tăng lên **174**, và
thêm bộ e2e HOÀN TOÀN MỚI (7 test, đếm riêng, không nằm trong `dotnet test`/`npm test`). Chi
tiết đầy đủ ở `.superpowers/sdd/2026-09-06-qlgx-web-phase-1/task-13-report.md` và các quyết
định tự đưa ra ở `can-review-sau.md` mục 29.

## Task 16 — PWA và bản nháp ngoại tuyến (2026-09-07)

Bản web giờ là PWA cài được lên máy/điện thoại (`vite-plugin-pwa`), có service worker cache vỏ
ứng dụng (JS/CSS/HTML/icon) để mở được khi mất mạng — **không cache bất kỳ phản hồi `/api/*`
nào** (không có `runtimeCaching` cho `/api`, xác nhận bằng cách đọc `dist/sw.js` sau khi build).
Có bản mới thì báo qua dải nhỏ góc dưới phải ("Có bản cập nhật mới — Tải lại"), không tự ý
reload để tránh mất dữ liệu form đang mở.

Mỗi form chi tiết (giáo dân 60+ trường, gia đình) tự lưu nháp vào `localStorage` mỗi 5 giây
(`lib/banNhap.ts`) — khoá theo tài khoản đăng nhập (không lẫn giữa hai người dùng chung máy),
mọi thao tác đọc/ghi bọc try/catch (không sập nếu `localStorage` bị chặn), nháp mang số phiên
bản (gặp nháp cũ thì bỏ qua an toàn). Mất mạng lúc bấm Lưu → dữ liệu đã gõ giữ nguyên trên form,
thông báo rõ ràng ("Mất kết nối mạng..."), thử lại được khi có mạng. Mở lại form có nháp chưa
lưu → hỏi khôi phục/bỏ qua, không tự động đè dữ liệu máy chủ. Token JWT hết hạn (8 tiếng, Task
14) đúng lúc mất mạng KHÔNG xoá nháp (chỉ đăng xuất chủ động mới xoá) — đăng nhập lại vẫn khôi
phục được, có test riêng cho ca này.

Ba test bắt buộc theo yêu cầu gốc (lưu/khôi phục nháp, `localStorage` ném lỗi vẫn chạy, nháp
lệch phiên bản bị bỏ qua an toàn, nháp không lẫn giữa hai tài khoản) đều có trong
`src/lib/banNhap.test.ts` (13 test), cộng thêm test tích hợp ở `GiaoDanDetailPage.test.tsx`/
`GiaDinhDetailPage.test.tsx`. Tổng test frontend tăng từ 157 lên **183**; backend giữ nguyên
**172** (task này không đụng gì tới API/CSDL). Chi tiết ở
`.superpowers/sdd/2026-09-06-qlgx-web-phase-1/task-16-report.md` và các quyết định tự đưa ra ở
`can-review-sau.md` mục 28.

**Nợ lại:** script `npm run build` (chạy `tsc -b && vite build`) hiện KHÔNG chạy được vì một lỗi
kiểu có sẵn TỪ TRƯỚC task này ở `src/lib/csv.test.ts` (`URL.createObjectURL` mock lệch kiểu) —
đã xác nhận bằng `git stash`, không phải do Task 16 gây ra. Đã kiểm chứng riêng phần PWA bằng
`npx vite build` (bỏ qua `tsc -b`) — sinh đúng service worker. Cần sửa lỗi kiểu đó trước khi ai
build thật để triển khai.

## Task 14 — Xác thực và phân tách tenant (2026-09-07)

`giao_xu_id` của phiên giờ lấy từ claim JWT (`BoiCanhGiaoXuTuNguoiDung`), không còn từ cấu hình
tĩnh — máy chủ đã sẵn sàng phục vụ nhiều giáo xứ. Đăng nhập bằng tên tài khoản/mật khẩu
(`POST /api/auth/dang-nhap`), mật khẩu băm bằng `PasswordHasher<TaiKhoan>` (PBKDF2). Mọi
endpoint nghiệp vụ đòi hỏi `RequireAuthorization()`; `/api/tai-khoan/*` (Quản lý tài khoản)
thêm policy "QuanTri". Ba test bảo mật bắt buộc (giáo xứ A không thấy dữ liệu giáo xứ B, truyền
`giaoXuId` qua query không đổi được kết quả, gọi khi chưa đăng nhập → 401) nằm trong
`BaoMatTests.cs` (12 test, tổng 172 test backend). Quyết định chi tiết ở
`docs/superpowers/specs/man-hinh/can-review-sau.md` mục 27.

**Còn nợ, phải làm trước khi có giáo xứ thứ hai lên chung máy chủ thật:**
- Row-Level Security của PostgreSQL (lớp phòng thủ thứ hai) — chưa làm, xem mục 27m.
- Không có cơ chế thu hồi token đã phát hành (đăng xuất chỉ xoá phía trình duyệt) — xem mục 27b.

**Tạo lại tài khoản quản trị (nếu cần)** — không có mật khẩu mặc định nào trong mã nguồn:

```
cd WebApp/src/Qlgx.Api
export ConnectionStrings__Qlgx="Host=localhost;Database=<ten_db>;Username=postgres;Password=<mat_khau>"
export Qlgx__JwtKey="<base64 32+ byte ngau nhien, dung chung khi chay API that>"
export QLGX_ADMIN_TEN_TAI_KHOAN="<ten_dang_nhap>"
export QLGX_ADMIN_MAT_KHAU="<mat_khau_it_nhat_8_ky_tu>"
export QLGX_ADMIN_HO_TEN="<ho_ten_hien_thi>"
export QLGX_ADMIN_GIAO_XU_TEN="<ten_giao_xu_dung_het>"   # hoac QLGX_ADMIN_GIAO_XU_ID=<guid>
dotnet run -- tao-tai-khoan-quan-tri
```

Database `qlgx_thu` (dùng để kiểm thử thật Task 14) hiện có sẵn một tài khoản quản trị tên
**`quantri`** — cố ý ĐỂ LẠI để lần sau còn đăng nhập được (không phải quên dọn dẹp). Mật khẩu
không ghi ở đây; ai cần biết thì hỏi trực tiếp, hoặc dùng lệnh trên để tạo một tài khoản quản
trị khác.

## Task 18 — Quản lý giáo xứ theo giáo phận + tách vai trò CSDL cho RLS (2026-09-07)

`WebApp/VIEC-TIEP-THEO.md` mục 2.3 và 2.2 — chuẩn bị cho giáo xứ thứ hai lên chung máy chủ.

- **Màn hình "Quản lý giáo phận/giáo hạt/giáo xứ"** (`/api/quan-tri/*`,
  `docs/superpowers/specs/man-hinh/quan-ly-giao-xu.md`) — danh sách + thêm + sửa ba cấp, xuyên
  TOÀN BỘ máy chủ, chỉ tài khoản mới `LoaiTaiKhoan=9` "Quản trị hệ thống" (policy
  "QuanTriHeThong") mới vào được — lớp phòng thủ DUY NHẤT vì `GiaoPhan`/`GiaoHat`/`GiaoXu`
  KHÔNG có `giao_xu_id` nên không có RLS bảo vệ. Không có nút xoá ở cả ba cấp. Kèm nút "Tạo tài
  khoản quản trị" ngay trên dòng giáo xứ (đường dẫn thứ tư được phép đọc/ghi chéo giáo xứ, cùng
  nhóm với đăng nhập/CLI tạo tài khoản/công cụ chuyển dữ liệu).
- **Giáo họ** (có `giao_xu_id`, an toàn hơn) được thêm API + màn hình thêm/sửa, nối vào mục
  "Giáo họ" sẵn có ở thanh bên.
- **Row-Level Security chạy thật với hai vai trò CSDL tách biệt** (`qlgx_app` không
  `BYPASSRLS`, `qlgx_admin` có `BYPASSRLS`) — hạ tầng đã có từ trước (docker-compose.yml,
  `.env.example`, `ChuoiKetNoiQuanTri.cs`) nhưng CHƯA từng chạy thật; đã tạo hai vai trò thật
  trên `qlgx_thu`, chạy `Qlgx.Api` với hai vai trò tách biệt, xác nhận đăng nhập/nghiệp vụ đều
  đúng thiết kế (xem `WebApp/TRIEN-KHAI.md` mục 5).
- **Phép thử cách ly tenant với giáo xứ thứ hai THẬT** — lần đầu thử được (trước đây `qlgx_thu`
  chỉ có một giáo xứ): tạo "Giáo xứ Thánh Gia" và tài khoản `thanhgia` qua giao diện, đăng nhập
  lại bằng tài khoản đó, xác nhận KHÔNG thấy 2050 giáo dân/40 gia đình của Vô Nhiễm. Ảnh ở
  `WebApp/anh-chup-kiem-thu/65`-`67`. Sau đó dọn sạch, `qlgx_thu` về lại đúng 2050/40/145/1
  giáo xứ, chỉ còn tài khoản `quantri`.
- Quyết định chi tiết và bằng chứng ĐỎ→XANH của test bảo mật:
  `docs/superpowers/specs/man-hinh/can-review-sau.md` mục 37. Test: **235 backend + 235
  front-end**, `npm run build` chạy được.
- **Còn nợ**: chưa có màn hình "nhập dữ liệu Access qua giao diện" (mục 2.4
  `VIEC-TIEP-THEO.md`, khác nhiệm vụ này) — vẫn phải dùng `Qlgx.Migration` bằng dòng lệnh sau
  khi tạo giáo xứ mới.

## Task 19 — Control nhập ngày tháng thông minh + khôi phục ngày tháng thiếu (2026-09-07)

`docs/superpowers/specs/man-hinh/ho-tro-nhap-lieu.md` + `can-review-sau.md` mục 49.

- **`GxDate`** (`WebApp/src/web/src/components/GxDate.tsx`, dùng chung mọi ô ngày): gõ liên tục
  không cần dấu `/` (ví dụ `01021985` → `01/02/1985`), tự nhảy vị trí gõ, click/focus thẳng vào
  ô tháng hoặc năm gõ luôn (không bắt buộc gõ ngày trước), chuẩn hoá ngày thiếu khi gõ xong năm
  hoặc rời ô (chỉ năm → điền `01/01`, tháng+năm → điền ngày `01`), tự nhảy control kế tiếp trên
  form khi vừa gõ xong năm hợp lệ. `Tab` không bị can thiệp.
- **Tự nhảy khi chọn dropdown**: mọi `<select>` nằm trong `<form>` tự chuyển tiêu điểm sang
  control kế tiếp khi chọn xong một mục — gắn một lần ở `App.tsx`
  (`lib/focusDieuHuong.ts`, `useTuNhayKhiChonDropdown`), không cần sửa từng màn hình.
- **Khôi phục ngày tháng thiếu trong `giao_dan.du_lieu_loi`** (238 giá trị/207 dòng, từ dữ liệu
  Access cũ lưu thiếu — xem spec mục A.5-A.6): lệnh CLI mới, chạy MỘT LẦN sau khi nâng cấp, chạy
  lại nhiều lần không hỏng gì (idempotent — chỉ ghi vào cột đang NULL):

  ```
  cd WebApp/src/Qlgx.Api
  export ConnectionStrings__Qlgx="Host=localhost;Database=<ten_db>;Username=postgres;Password=<mat_khau>"
  export Qlgx__JwtKey="<base64 32+ byte ngau nhien, dung chung khi chay API that>"
  dotnet run -- khoi-phuc-ngay-thang-thieu
  ```

  Đọc `du_lieu_loi` của `giao_dan`, chỉ điền vào `ngay_sinh`/`ngay_rua_toi`/`ngay_ruoc_le`/
  `ngay_them_suc`/`ngay_qua_doi` đang NULL mà giá trị gốc phân giải được (chỉ năm, hoặc
  tháng+năm) — **giữ nguyên `du_lieu_loi`**, không xoá. Giá trị gốc thật sự hỏng (ví dụ mojibake
  phông chữ cũ) tiếp tục nằm nguyên trong `du_lieu_loi`, không đoán bừa.
- `NgayThangText.Doc` (`WebApp/src/Qlgx.Data/NgayThangText.cs`, dùng cả bởi API lẫn công cụ
  chuyển dữ liệu Access `Qlgx.Migration.Core`) đã phân giải thêm hai khuôn `yyyy` và `MM/yyyy` —
  các giáo xứ MỚI chuyển dữ liệu từ Access về sau **không cần** chạy lệnh khôi phục trên, công cụ
  chuyển dữ liệu tự làm đúng ngay từ đầu.

## Bốn thay đổi lớn đã chốt ngày 2026-09-06

Ghi lại vì chúng đảo ngược quyết định ban đầu và ràng buộc mọi việc còn lại.

1. **Mô hình triển khai đảo ngược.** Từ "mỗi giáo xứ một bản cài tại chỗ" sang **một máy chủ
   tập trung phục vụ nhiều giáo xứ**, vì giáo xứ chỉ có máy cá nhân yếu, không tự bảo đảm
   được sao lưu hay tính sẵn sàng cao. Hệ quả: `giao_xu_id` phải lấy từ claim đăng nhập chứ
   không bao giờ từ tham số trình duyệt; xác thực chuyển lên giai đoạn 1; API không giữ trạng
   thái trong tiến trình và không ghi file xuống đĩa cục bộ.
2. **Hôn phối chuyển lên giai đoạn 1** — khối hôn phối nằm ngay trong form gia đình của bản
   desktop, thiếu nó thì giáo xứ chưa bỏ được bản cũ.
3. **PWA và bản nháp ngoại tuyến** — không để mất công sức gõ khi mạng rớt.
4. **Chuyển đổi đầy đủ nghiệp vụ từng màn hình**, không chỉ các trường dữ liệu.

## Việc nhỏ còn nợ

- **Mật khẩu CSDL không bao giờ được ghi vào repo.** Cả hai bộ test và factory design-time nay
  đều bắt buộc lấy từ biến `QLGX_TEST_PG`, thiếu thì báo lỗi rõ ràng thay vì đoán mật khẩu.
  Lịch sử nhánh đã được dọn, đã kiểm lại từng commit là sạch.
- **Mật khẩu file `.mdb` vẫn nằm nguyên văn trong mã nguồn desktop cũ** (`Source/Giaoly/app.config`,
  `Source/Giaoly/Properties/Settings.Designer.cs`, `BIN/Giaoly.dll.config`). Đây là di sản có
  từ trước, nằm ở nhánh `master`. Công cụ chuyển dữ liệu mới **không** viết cứng mật khẩu này —
  nó nhận qua biến `QLGX_MDB_PASSWORD` hoặc tham số `--mat-khau`, vì mỗi giáo xứ đặt một mật
  khẩu khác nhau. Nên cân nhắc gỡ khỏi bản desktop trước khi đẩy repo lên nơi công khai.
- **Bitness của driver Access.** `Qlgx.Migration` cố ý **không** ghim cứng x86: driver ACE OLEDB
  phải cùng bitness với tiến trình gọi nó, mà máy dev này chỉ có bản 64-bit còn nhiều máy giáo
  xứ chỉ có bản 32-bit. Chọn lúc publish bằng RID `win-x86` hoặc `win-x64`.
- Màn hình chi tiết giáo dân: thông tin liên hệ (điện thoại, email, địa chỉ) hiện nằm chìm
  dưới dạng nhãn phụ trong khối "Thông tin khác" — cần tách thành nhóm riêng cho dễ thấy.
- Danh sách các việc nhỏ khác nằm ở các dòng `minor (deferred)` trong sổ theo dõi.

## Còn thiếu để bỏ hẳn bản desktop (chốt sau 2 đợt review độc lập, 2026-09-07)

Bản web hiện đã **tác nghiệp được**: tạo/sửa/xoá giáo dân và gia đình, quản lý thành viên,
vợ chồng, hôn phối, tận hiến, hội đoàn, giáo lý, chuyển xứ, chủ hộ — tất cả trên dữ liệu thật.
Nhưng còn hai khoảng trống thật sự chặn:

1. **In ấn và chứng nhận** — thuộc giai đoạn 3 theo kế hoạch, nhưng đây là nghiệp vụ **hằng
   ngày** của giáo xứ: giấy chứng nhận rửa tội, rước lễ, thêm sức, hôn phối, giấy giới thiệu
   chuyển xứ, sổ gia đình. 10/12 mục menu chuột phải của màn hình giáo dân hiện chỉ có nhãn.
   Chừng nào chưa có, giáo xứ vẫn phải mở bản desktop để in.

2. **Một số hồ sơ chi tiết chưa quản lý được qua web** — ảnh đại diện (chưa tải lên được),
   người ban bí tích ở vài chỗ, màn hình tự đổi mật khẩu.

Ngoài ra còn các việc nợ đã ghi nhận, không chặn:
- Thu hồi token chủ động (hiện giảm nhẹ bằng thời hạn 8 tiếng).
- Quy tắc 11 (trùng ngày chuyển xứ).
- Hơn 60 màn hình phụ của bản desktop chưa migrate (kiểm tra dữ liệu, chuẩn hoá dữ liệu,
  chuyển họ hàng loạt, thống kê, biểu đồ, hồ sơ lưu trữ, giáo lý...).

## Sổ quyết định cần người dùng review

`docs/superpowers/specs/man-hinh/can-review-sau.md` — **37 mục**. Đây là nơi ghi mọi chỗ:
- bản desktop làm sai mà ta **cố ý tái hiện y hệt** (theo chỉ đạo "migrate y hệt rồi note lại"),
- bản web **cố ý làm khác** desktop, kèm lý do,
- quyết định tự đưa ra khi người dùng không có mặt.

Mỗi mục có trích dẫn dòng mã desktop và câu hỏi chờ quyết. **Đừng xoá mục nào khi chưa quyết.**

## Spec từng màn hình

`docs/superpowers/specs/man-hinh/` — 8 spec đã viết từ mã nguồn desktop, mỗi quy tắc có trích
dẫn dòng. Quy trình bắt buộc: **nghiên cứu màn hình → viết spec → rồi mới migrate**.
Còn khoảng 60 màn hình nhỏ hơn chưa có spec.
