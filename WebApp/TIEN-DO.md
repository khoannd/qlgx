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
   - Backend: `dotnet test WebApp/Qlgx.sln` → phải ra **77/77 xanh**
   - Front-end: `cd WebApp/src/web && npm test -- --run` → phải ra **49/49 xanh**
5. Sổ theo dõi chi tiết từng task, từng quyết định:
   `.superpowers/sdd/2026-09-06-qlgx-web-phase-1/progress.md`
   (thư mục này nằm ngoài git, nhưng vẫn còn trên đĩa sau khi khởi động lại)
6. Task tiếp theo cần làm: **Task 17 — thực thể và schema cho 19 bảng Access còn lại**.

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

Tổng: **77 test backend + 49 test front-end**, tất cả xanh.

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

Cả 7 bảng chuyển **đủ mọi cột** (GiaoDan 63/63, GiaDinh 17/17, GiaoXu 11/11 …). `MaNhanDang`
được giữ nguyên trên 4 bảng có cột này để sau còn đồng bộ hai chiều desktop↔web. Giá trị
`VaiTro` giữ nguyên dạng thô (0,1,2,3,8,18,100) — gộp lại sẽ đụng khoá chính và mất dòng.
Ngày tháng hỏng (chỉ ghi năm, chuỗi rỗng) được giữ nguyên văn trong cột `du_lieu_loi`.

Ảnh chụp màn hình chạy thật: `WebApp/anh-chup-kiem-thu/`.

## Còn lại của giai đoạn 1

| Task | Nội dung | Ghi chú |
|---|---|---|
| 17 | Thực thể và schema cho **19 bảng Access còn lại** | **làm tiếp ngay** |
| 13 | Kiểm thử đầu-cuối và **triển khai máy chủ** | đã đổi mục tiêu, không còn cài lên máy giáo xứ |
| 14 | Xác thực và phân tách tenant theo claim đăng nhập | task mới |
| 15 | Giao diện hôn phối | task mới |
| 16 | PWA và bản nháp ngoại tuyến | task mới |

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
