# Tiến độ chuyển đổi QLGX sang nền tảng web

Cập nhật: 2026-09-06, tạm dừng để khởi động lại máy.
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
   - Backend: `dotnet test WebApp/Qlgx.sln` → phải ra **36/36 xanh**
   - Front-end: `cd WebApp/src/web && npm test -- --run` → phải ra **39/39 xanh**
5. Sổ theo dõi chi tiết từng task, từng quyết định:
   `.superpowers/sdd/2026-09-06-qlgx-web-phase-1/progress.md`
   (thư mục này nằm ngoài git, nhưng vẫn còn trên đĩa sau khi khởi động lại)
6. Task tiếp theo cần làm: **Task 7 — API chi tiết gia đình, có chống ghi đè**.

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

Tổng: **36 test backend + 39 test front-end**, tất cả xanh.

## Còn lại của giai đoạn 1

| Task | Nội dung | Ghi chú |
|---|---|---|
| 7 | API chi tiết gia đình, chống ghi đè khi nhiều người cùng sửa | **làm tiếp ngay** |
| 8 | API giáo dân: danh sách, chi tiết, cập nhật, thành viên gia đình | |
| 9 | Công cụ chuyển dữ liệu Access → PostgreSQL | tách 2 project theo Ruling 3 |
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
  Trước đây mật khẩu thật đã lỡ lọt vào ba commit cục bộ (`67093c8`, `8e54c3b`, `319e5a2`) —
  nhánh này **chưa được đẩy lên GitHub** nên chưa rò ra ngoài, nhưng cần dọn lịch sử trước khi
  push lần đầu, hoặc đổi mật khẩu PostgreSQL.
- Màn hình chi tiết giáo dân: thông tin liên hệ (điện thoại, email, địa chỉ) hiện nằm chìm
  dưới dạng nhãn phụ trong khối "Thông tin khác" — cần tách thành nhóm riêng cho dễ thấy.
- Danh sách các việc nhỏ khác nằm ở các dòng `minor (deferred)` trong sổ theo dõi.
