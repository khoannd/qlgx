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
   - Backend: `dotnet test WebApp/Qlgx.sln` → phải ra **133/133 xanh**
   - Front-end: `cd WebApp/src/web && npm test -- --run` → phải ra **87/87 xanh**
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

Tổng: **133 test backend + 87 test front-end**, tất cả xanh.

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
| 13 | Kiểm thử đầu-cuối và **triển khai máy chủ** | đã đổi mục tiêu, không còn cài lên máy giáo xứ; nợ RLS (xem `can-review-sau.md` mục 27m) |
| 15 | Giao diện hôn phối | task mới |
| 16 | PWA và bản nháp ngoại tuyến | task mới |

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
