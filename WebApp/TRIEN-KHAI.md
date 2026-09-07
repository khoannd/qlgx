# Triển khai QLGX — hướng dẫn cho người vận hành

Tài liệu này viết cho người phụ trách vận hành máy chủ (không nhất thiết là lập trình viên).
Nếu bạn cần hiểu vì sao hệ thống được thiết kế như vậy, xem
`docs/superpowers/specs/2026-09-06-qlgx-web-migration-design.md`; tài liệu này chỉ nói "làm thế
nào".

## 1. Mô hình vận hành — điều quan trọng nhất cần hiểu trước

QLGX bản web là **một máy chủ trung tâm phục vụ nhiều giáo xứ cùng lúc**, không phải "mỗi giáo
xứ cài một bản". Hệ quả:

- Chỉ có **một** cơ sở dữ liệu PostgreSQL cho mọi giáo xứ — dữ liệu tách nhau bằng cột
  `giao_xu_id` trong từng bảng, **không** tách bằng database hay schema riêng.
- Người vận hành **không** cài phần mềm này lên máy của giáo xứ. Giáo xứ chỉ cần trình duyệt
  và địa chỉ web do bạn cấp.
- Sao lưu, bảo mật, tính sẵn sàng là trách nhiệm của bạn (người vận hành máy chủ trung tâm),
  không phải của văn phòng giáo xứ.

## 2. Yêu cầu máy chủ

- Docker Engine + Docker Compose plugin (`docker compose version` chạy được). Đã kiểm chứng
  thật với Docker 29 trên Windows; trên Linux production dùng bản Docker Engine chính thức.
- Một PostgreSQL 15 trở lên — có thể chạy trong cùng `docker-compose.yml` (đủ cho pilot vài
  giáo xứ) hoặc một dịch vụ PostgreSQL quản lý riêng (khuyến nghị khi lên sản xuất thật, để
  sao lưu/tự động phục hồi tách khỏi vòng đời container ứng dụng).
- Một máy chủ Linux (hoặc Windows Server chạy Docker) với ổ đĩa đủ chỗ cho dữ liệu PostgreSQL —
  ước lượng thô: dữ liệu một giáo xứ cỡ giáo xứ Vô Nhiễm (2050 giáo dân) chiếm vài chục MB;
  nhân với số giáo xứ dự kiến pilot.
- **Reverse proxy đứng trước** (nginx, Caddy, Traefik…) để làm HTTPS — xem mục 7. Ứng dụng tự
  nó chỉ nói HTTP thuần, không tự tạo chứng chỉ.
- **KHÔNG** cần cài .NET SDK/Runtime hay Node.js trực tiếp lên máy chủ — mọi thứ đóng gói sẵn
  trong image Docker.

## 3. Kiến trúc và các quyết định đã chốt (đọc trước khi triển khai thật)

- **Một image Docker duy nhất** chứa cả API và tệp tĩnh của web (đã build sẵn) — xem
  `Dockerfile` ở thư mục `WebApp`. Quyết định gộp một image thay vì tách API/web riêng: pilot
  quy mô nhỏ, một container/một health check dễ vận hành hơn, đổi lại phải build lại cả hai
  khi chỉ sửa một phía — chấp nhận được ở quy mô này.
- **API không giữ trạng thái** — chạy được nhiều bản song song sau một bộ cân bằng tải mà
  không cần "sticky session". `docker-compose.yml` đi kèm chỉ chạy MỘT bản (đủ cho pilot); khi
  cần nhiều bản, chạy cùng image này trên Kubernetes/Swarm hoặc nhiều container Docker độc lập
  đứng sau cùng một reverse proxy.
- **Row-Level Security (RLS) của PostgreSQL đã bật** — lớp phòng thủ THỨ HAI chống rò dữ liệu
  giữa các giáo xứ (lớp thứ nhất là bộ lọc của EF Core trong chính mã nguồn). Vì vậy triển khai
  thật **bắt buộc** dùng đúng hai vai trò CSDL mô tả ở mục 5 — dùng một vai trò `postgres`
  superuser duy nhất cho mọi thứ (như máy phát triển) sẽ làm RLS **mất tác dụng hoàn toàn**
  (superuser luôn bỏ qua RLS).
- **Migration CSDL tự chạy lúc khởi động** — image API tự kiểm tra và áp dụng migration còn
  thiếu mỗi lần khởi động (biến `Qlgx__ChayMigrationKhiKhoiDong=true`, đã bật sẵn trong
  `docker-compose.yml`). Có khoá tự nhiên (`pg_advisory_lock` của PostgreSQL) nên nhiều bản API
  khởi động cùng lúc (nâng cấp có nhiều bản chạy song song) không đua nhau chạy migration — bản
  đến sau tự đợi, thấy migration đã đủ thì bỏ qua ngay. Nếu muốn tách migration thành một bước
  triển khai riêng (kiểm soát chặt hơn khi nào schema đổi), đặt biến này thành `false` ở MỌI
  nơi và tự chạy `dotnet Qlgx.Api.dll` một lần bằng tay/CI trước khi rollout — xem mục 9.

## 4. Biến môi trường — không có bí mật nào trong repo

Sao chép `WebApp/.env.example` thành `WebApp/.env` (cùng thư mục với `docker-compose.yml`),
điền giá trị THẬT. Tệp `.env` **không** được đưa lên git (đã có trong `.gitignore`).

| Biến | Ý nghĩa |
|---|---|
| `POSTGRES_DB`, `POSTGRES_USER`, `POSTGRES_PASSWORD` | Tài khoản superuser PostgreSQL — chỉ dùng để KHỞI TẠO container CSDL và tạo hai vai trò ở mục 5, không dùng cho ứng dụng chạy thường ngày. |
| `QLGX_APP_DB_USER`, `QLGX_APP_DB_PASSWORD` | Vai trò CSDL cho lưu lượng nghiệp vụ hằng ngày — **KHÔNG** có `BYPASSRLS`. |
| `QLGX_ADMIN_DB_USER`, `QLGX_ADMIN_DB_PASSWORD` | Vai trò CSDL **có** `BYPASSRLS` — chỉ dùng cho đăng nhập/tạo tài khoản quản trị/chuyển dữ liệu (xem mục 5). |
| `QLGX_API_PORT` | Cổng trên máy chủ trỏ vào container API (reverse proxy gọi vào cổng này). |
| `QLGX_JWT_KEY` | Khoá ký JWT — chuỗi base64 của ít nhất 32 byte ngẫu nhiên (`openssl rand -base64 32`). Đổi khoá này = đăng xuất toàn bộ người dùng đang có phiên. |
| `ASPNETCORE_ENVIRONMENT` | Để `Production` khi triển khai thật. |

**Không bao giờ** ghi giá trị thật của các biến này vào bất kỳ file nào trong git — kể cả
`docker-compose.yml` (file đó chỉ tham chiếu `${TEN_BIEN}`, không có giá trị).

**Chạy `dotnet run` cục bộ (không qua Docker)**: `ConnectionStrings:Qlgx` trong
`appsettings.Development.json` cố tình để RỖNG (review-backend.md mục T3 — không ghi mật khẩu
CSDL, kể cả quy ước `postgres`/`postgres` cho máy dev, vào file trong repo). Đặt một trong hai:

```
dotnet user-secrets set ConnectionStrings:Qlgx "Host=localhost;Database=qlgx_dev;Username=postgres;Password=<mật khẩu của bạn>" --project WebApp/src/Qlgx.Api
```

hoặc biến môi trường `ConnectionStrings__Qlgx` (hai dấu gạch dưới) trước khi chạy. Thiếu biến
này, host vẫn khởi động được (health-check `/api/suc-khoe` không cần CSDL) nhưng bất kỳ truy
vấn CSDL thật nào cũng sẽ báo lỗi rõ ràng từ Npgsql ngay lập tức.

## 5. Row-Level Security — tạo hai vai trò CSDL (bắt buộc trước khi chạy thật)

Migration `BatRlsChoBangTheoGiaoXu` đã bật RLS trên 22 bảng nghiệp vụ. Chính sách so sánh
`giao_xu_id` của mỗi dòng với tham số phiên `app.giao_xu_id` mà API tự đặt theo claim của
người đăng nhập (xem `BoiCanhGiaoXuConnectionInterceptor.cs`). Điều này chỉ có tác dụng thật
nếu vai trò CSDL của API **không phải** superuser và **không có** `BYPASSRLS`.

Sau khi container `postgres` chạy lần đầu (`docker compose up -d postgres`), tạo hai vai trò
bằng `psql` (thay `<mat_khau_...>` bằng mật khẩu thật, TRÙNG với giá trị đã điền trong `.env`):

```sql
-- Vai trò phục vụ nghiệp vụ hằng ngày — PHẢI không có BYPASSRLS.
CREATE ROLE qlgx_app LOGIN PASSWORD '<mat_khau_qlgx_app>' NOSUPERUSER NOBYPASSRLS;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO qlgx_app;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public TO qlgx_app;
ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL PRIVILEGES ON TABLES TO qlgx_app;

-- Vai trò chỉ dùng cho đăng nhập/tạo tài khoản quản trị/chuyển dữ liệu — CÓ BYPASSRLS
-- (đây là 3 đường dẫn hợp lệ DUY NHẤT trong toàn hệ thống cần đọc/ghi chéo giáo xứ).
CREATE ROLE qlgx_admin LOGIN PASSWORD '<mat_khau_qlgx_admin>' NOSUPERUSER BYPASSRLS;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO qlgx_admin;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public TO qlgx_admin;
ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL PRIVILEGES ON TABLES TO qlgx_admin;
```

Đặt `QLGX_APP_DB_USER=qlgx_app`/`QLGX_ADMIN_DB_USER=qlgx_admin` (cùng mật khẩu vừa tạo) trong
`.env` rồi khởi động API.

**Kiểm chứng RLS đang thật sự có tác dụng** (làm một lần sau khi triển khai, trước khi thêm
giáo xứ thứ hai): dùng `psql` đăng nhập bằng vai trò `qlgx_app`, chạy:

```sql
SELECT set_config('app.giao_xu_id', '<mot-uuid-giao-xu-bat-ky>', false);
SELECT count(*) FROM giao_dan;   -- chỉ đếm đúng giáo xứ đó
SELECT set_config('app.giao_xu_id', '', false);
SELECT count(*) FROM giao_dan;   -- phải ra 0 — KHÔNG đặt tham số = không đọc được dòng nào
```

Nếu dòng thứ hai KHÔNG ra 0, dừng lại và kiểm tra lại vai trò/chính sách — đừng đưa giáo xứ
thứ hai lên khi RLS chưa đúng. Bộ test `Qlgx.Data.Tests/RlsTests.cs` (chạy bằng
`dotnet test`) tự động hoá đúng phép thử này bằng kết nối Npgsql thô, không qua EF Core, VÀ qua
đúng `QlgxDbContext` nghiệp vụ thật (test
`QlgxDbContext_that_qua_vai_tro_khong_bypassrls_van_cach_ly_dung_giao_xu`).

**Đã chạy thật một lần** (2026-09-07, database `qlgx_thu`, xem
`docs/superpowers/specs/man-hinh/can-review-sau.md` mục 37c): tạo hai vai trò `qlgx_app`/
`qlgx_admin` thật, chạy `Qlgx.Api` với `ConnectionStrings__Qlgx` trỏ `qlgx_app` và
`ConnectionStrings__QlgxQuanTri` trỏ `qlgx_admin`, xác nhận đăng nhập vẫn hoạt động (đi qua
`qlgx_admin`), nghiệp vụ hằng ngày (giáo dân/gia đình) đi qua `qlgx_app` bị RLS đúng như thiết
kế, và một giáo xứ thứ hai tạo qua màn hình "Quản lý giáo xứ" (mục 9) hoàn toàn không thấy dữ
liệu của giáo xứ thứ nhất. Mật khẩu hai vai trò dùng cho lần chạy thử này KHÔNG được giữ lại —
người triển khai thật tạo mật khẩu MỚI theo đúng các bước ở mục này.

## 6. Các bước triển khai lần đầu

```bash
cd WebApp
cp .env.example .env        # rồi điền giá trị thật (xem mục 4)
docker compose up -d postgres
# (thực hiện mục 5 — tạo hai vai trò qlgx_app/qlgx_admin)
docker compose up -d api    # tự chạy migration lúc khởi động, xem log để chắc chắn
docker compose logs -f api  # theo dõi tới dòng "Now listening on: http://+:8080"
curl http://localhost:${QLGX_API_PORT:-8080}/api/suc-khoe   # phải trả {"trangThai":"ok",...}
```

## 7. HTTPS — luôn đặt sau reverse proxy

Ứng dụng **không** tự làm HTTPS/chứng chỉ. Đặt một reverse proxy (nginx, Caddy, Traefik) trước
container API, xử lý TLS ở đó rồi chuyển tiếp HTTP thuần vào cổng `QLGX_API_PORT`. Caddy tự
xin chứng chỉ Let's Encrypt là lựa chọn ít việc nhất cho một máy chủ pilot:

```
qlgx.viduten.vn {
    reverse_proxy localhost:8080
}
```

Với nginx, nhớ thêm `proxy_set_header X-Forwarded-Proto $scheme;` để ứng dụng biết yêu cầu gốc
là HTTPS (không bắt buộc với logic hiện tại nhưng là thói quen đúng, tránh lỗi redirect loop
nếu sau này thêm middleware bắt buộc HTTPS).

## 8. Tạo tài khoản quản trị đầu tiên

Không có tài khoản/mật khẩu mặc định nào trong mã nguồn. Tạo bằng lệnh chạy MỘT LẦN, bên trong
container API đang chạy:

```bash
docker compose exec api dotnet Qlgx.Api.dll tao-tai-khoan-quan-tri
```

Container cần các biến môi trường sau lúc chạy lệnh này (đặt tạm qua `docker compose exec -e`,
hoặc thêm tạm vào `.env` rồi bỏ lại sau khi tạo xong):

```
QLGX_ADMIN_TEN_TAI_KHOAN=<ten dang nhap>
QLGX_ADMIN_MAT_KHAU=<mat khau, it nhat 8 ky tu>
QLGX_ADMIN_HO_TEN=<ho ten hien thi>
QLGX_ADMIN_GIAO_XU_TEN=<ten giao xu — phai da co san trong CSDL, xem muc 9>
QLGX_ADMIN_LOAI_TAI_KHOAN=<tuy chon, mac dinh "0". Dat "9" de tao tai khoan "Quan tri he thong"
                            — chi tai khoan nay moi vao duoc man hinh "Quan ly giao xu" xuyen
                            toan may chu (xem muc 9). Chi cap cho 1-2 nguoi van hanh trung tam.>
```

Ví dụ đầy đủ:

```bash
docker compose exec \
  -e QLGX_ADMIN_TEN_TAI_KHOAN=vanphong \
  -e QLGX_ADMIN_MAT_KHAU='mat-khau-that-manh' \
  -e QLGX_ADMIN_HO_TEN='Văn phòng Giáo xứ ABC' \
  -e QLGX_ADMIN_GIAO_XU_TEN='Giáo xứ ABC' \
  api dotnet Qlgx.Api.dll tao-tai-khoan-quan-tri
```

## 9. Thêm một giáo xứ mới

**Cách khuyến nghị — màn hình web "Quản lý giáo xứ"** (từ 2026-09-07, xem
`docs/superpowers/specs/man-hinh/quan-ly-giao-xu.md`): đăng nhập bằng một tài khoản
`LoaiTaiKhoan=9` ("Quản trị hệ thống" — mục 8 dưới đây, thêm `QLGX_ADMIN_LOAI_TAI_KHOAN=9`), mở
mục "Quản lý giáo xứ" ở thanh bên, thêm Giáo phận/Giáo hạt (nếu chưa có) rồi thêm Giáo xứ, sau
đó bấm "Tạo tài khoản quản trị" ngay trên dòng giáo xứ vừa thêm — không cần chạm `psql`/CLI
nào cho bước này nữa. Màn hình này CỐ Ý xuyên giáo xứ (policy "QuanTriHeThong") — chỉ cấp
`LoaiTaiKhoan=9` cho 1-2 người vận hành trung tâm, KHÔNG cấp cho quản trị viên của từng giáo xứ.

**Cách dự phòng — chèn thẳng CSDL** (khi chưa có tài khoản `LoaiTaiKhoan=9` nào, ví dụ lần
triển khai đầu tiên):

```bash
docker compose exec postgres psql -U qlgx_admin -d ${POSTGRES_DB:-qlgx} -c \
  "INSERT INTO giao_xu (id, ma_giao_xu_cu, ten_giao_xu, created_at) VALUES (gen_random_uuid(), 0, 'Tên giáo xứ mới', now());"
```

Sau đó tạo tài khoản quản trị đầu tiên cho giáo xứ đó (mục 8, dùng đúng `ten_giao_xu` vừa
chèn). Từ lúc này, mọi dữ liệu người dùng đó tạo ra tự động gắn `giao_xu_id` đúng theo claim
đăng nhập — không cần thao tác gì thêm.

## 10. Chuyển dữ liệu từ file Access (`giaoxu.mdb`) của giáo xứ đó

Công cụ `Qlgx.Migration` đọc trực tiếp file `.mdb` gốc và ghi vào PostgreSQL. Máy chạy công cụ
này **phải cài Microsoft Access Database Engine (ACE OLEDB)** đúng bitness (32-bit nếu chạy
tiến trình x86, 64-bit nếu x64) — công cụ này chạy TRÊN MÁY VẬN HÀNH (Windows), không chạy
trong container Linux (driver ACE OLEDB không có bản Linux).

```powershell
cd WebApp/src/Qlgx.Migration
# Bước 1 — CHẠY THỬ trước, không ghi gì, chỉ xem báo cáo đối chiếu:
dotnet run -- "C:\đường\dẫn\giaoxu.mdb" "Host=may-chu-that;Database=qlgx;Username=qlgx_admin;Password=..." <guid-giao-xu-vua-tao> --mat-khau=<mat khau file mdb>

# Đọc kỹ báo cáo cảnh báo (dữ liệu ngày tháng hỏng, trùng lặp…), rồi chạy thật:
dotnet run -- "C:\đường\dẫn\giaoxu.mdb" "Host=may-chu-that;Database=qlgx;Username=qlgx_admin;Password=..." <guid-giao-xu-vua-tao> --mat-khau=<mat khau file mdb> --chay-that
```

Dùng đúng chuỗi kết nối của vai trò **`qlgx_admin`** (có `BYPASSRLS`) — công cụ này ghi dữ liệu
với `giao_xu_id` cho trước chứ không có phiên đăng nhập nào để tự đặt tham số RLS, vai trò
`qlgx_app` (bị RLS chặn khi không có tham số phiên) sẽ không ghi được gì.

Không muốn mật khẩu `.mdb` nằm trong lịch sử dòng lệnh: đặt biến `QLGX_MDB_PASSWORD` thay cho
`--mat-khau`.

## 11. Ghi nhật ký (logging)

Mặc định ứng dụng ghi log ra `stdout`/`stderr` của container (chuẩn ASP.NET Core) — dùng
`docker compose logs api` hoặc chuyển tiếp vào hệ thống log tập trung của bạn (Loki, ELK,
CloudWatch…) tuỳ hạ tầng. Điều **bắt buộc phải kiểm tra** trước khi bật log chi tiết hơn mặc
định:

- **Không ghi dữi liệu cá nhân giáo dân vào log.** Ứng dụng hiện KHÔNG có middleware ghi log
  thân yêu cầu/phản hồi (request/response body) — đừng thêm loại middleware này mà không che
  các trường nhạy cảm trước (họ tên, ngày sinh, CMND, địa chỉ…).
- **Không ghi mật khẩu.** `PasswordHasher` không bao giờ log mật khẩu thô; câu lệnh
  `tao-tai-khoan-quan-tri` chỉ in ra tên tài khoản và mã giáo xứ khi tạo xong (xem
  `TaoTaiKhoanQuanTri.cs`), không in mật khẩu.
- Log SQL chi tiết (`Microsoft.EntityFrameworkCore.Database.Command`) đang ở mức mặc định
  (`Information`/`Warning` theo `appsettings.json`) — mức này KHÔNG in ra giá trị tham số thật
  trong bản build Production (Npgsql chỉ in placeholder `'?'` trừ khi
  `EnableSensitiveDataLogging()` được bật rõ ràng trong mã nguồn — hiện KHÔNG bật ở đâu cả).
  Đừng bật cờ đó trên môi trường thật.
- Endpoint `/api/suc-khoe` dùng cho health check của bộ cân bằng tải/Docker — không cần xác
  thực, không trả thông tin nhạy cảm (chỉ trạng thái + số phiên bản).

## 12. Sao lưu và phục hồi CSDL

Sao lưu = sao lưu PostgreSQL, không có gì khác cần sao lưu (không ghi file xuống đĩa cục bộ ở
tầng ứng dụng — xem mục 3).

**Sao lưu** (chạy định kỳ bằng cron/Task Scheduler, ví dụ mỗi đêm):

```bash
docker compose exec -T postgres pg_dump -U qlgx_admin -Fc ${POSTGRES_DB:-qlgx} > "qlgx-$(date +%Y%m%d).dump"
```

Giữ file `.dump` ở nơi KHÁC máy chủ (một máy chủ bản sao lưu, một dịch vụ lưu trữ đối tượng) —
sao lưu nằm cùng ổ đĩa với dữ liệu gốc không bảo vệ được gì khi ổ đĩa hỏng.

**Phục hồi** (máy chủ mới hoặc khôi phục sau sự cố):

```bash
docker compose up -d postgres
# Doi postgres san sang, sau do:
docker compose exec -T postgres pg_restore -U qlgx_admin -d ${POSTGRES_DB:-qlgx} --clean --if-exists < qlgx-20260907.dump
docker compose up -d api
```

`--clean --if-exists` xoá các đối tượng đã có trước khi phục hồi — dùng khi phục hồi vào một
database ĐÃ CÓ SCHEMA (an toàn để chạy lại nhiều lần). Sau khi phục hồi, kiểm tra lại RLS còn
bật đúng (mục 5) — `pg_restore` phục hồi cả policy/`ENABLE ROW LEVEL SECURITY` vì chúng nằm
trong dump, nhưng vẫn nên kiểm lại một lần cho chắc trước khi cho người dùng vào lại.

**Diễn tập phục hồi** ít nhất một lần trước khi có giáo xứ thật đầu tiên — một bản sao lưu chưa
từng được phục hồi thử không đáng tin.

## 13. Nâng cấp phiên bản

1. Kéo/build image mới (`git pull` rồi `docker compose build api`, hoặc kéo image đã build sẵn
   nếu bạn dùng registry riêng).
2. **Sao lưu CSDL trước** (mục 12) — luôn luôn, kể cả khi bản nâng cấp "chỉ sửa giao diện".
3. `docker compose up -d api` — container mới tự chạy migration còn thiếu lúc khởi động (mục
   3), khoá bằng `pg_advisory_lock` nên an toàn dù bạn đang chạy nhiều bản API (rolling
   update): bản cũ vẫn phục vụ bình thường trong lúc bản mới migrate xong rồi mới nhận traffic.
4. Theo dõi `docker compose logs -f api` tới khi thấy "Application started" và
   `/api/suc-khoe` trả `ok` trước khi coi là xong.
5. Nếu migration mới có bước không thể lùi lại được (ví dụ xoá cột), đọc kỹ ghi chú của
   migration đó trước khi nâng cấp — hiện tại (Task 13) chưa có migration nào thuộc loại này.

## 14. Việc CHƯA kiểm chứng được trên máy chuẩn bị tài liệu này

Ghi rõ ở đây theo đúng yêu cầu — đừng coi các mục này là "đã xong":

- **Không có Postgres quản lý riêng (managed) nào được thử** — hướng dẫn mục 2 giả định
  PostgreSQL tự vận hành (trong `docker-compose.yml` hoặc một instance tự cài); nếu dùng dịch
  vụ managed (RDS, Cloud SQL…), cách tạo vai trò `BYPASSRLS` có thể khác (một số dịch vụ managed
  hạn chế quyền tạo vai trò có `BYPASSRLS` cho tài khoản không phải "chủ" instance) — kiểm tra
  tài liệu của dịch vụ đó trước.
- **Chưa triển khai thật lên nhiều máy (HA thật, nhiều bản API sau một bộ cân bằng tải)** —
  kiến trúc được THIẾT KẾ để làm được (không trạng thái, khoá migration đúng), nhưng chỉ mới
  chạy thử một nút duy nhất trên máy chuẩn bị tài liệu này.
- **Reverse proxy/HTTPS ở mục 7 chưa được dựng thật** — chỉ là cấu hình mẫu, chưa chạy qua.
