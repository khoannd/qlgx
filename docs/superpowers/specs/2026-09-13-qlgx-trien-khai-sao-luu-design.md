# Thiết kế: Triển khai máy chủ, tự động sao lưu và phục hồi (QLGX Web)

Ngày: 2026-09-13. Nhánh: `webapp-phase-1`. Trạng thái: đã thống nhất với người dùng, chờ lập kế
hoạch triển khai.

## 1. Mục tiêu và bối cảnh

### 1.1 Vấn đề

Bản web của QLGX (`WebApp/`) đã hoàn tất về mặt chức năng và vừa qua đợt kiểm thử chấp nhận
23/23 màn hình (xem `WebApp/anh-chup-kiem-thu/BAO-CAO-ACCEPTANCE-2026-09-12*.md`). Nhưng phần
triển khai còn ở mức "pilot": `TRIEN-KHAI.md` mục 5 yêu cầu quản trị viên **chép-dán SQL bằng
tay** để tạo hai vai trò RLS, mục 12 chỉ có **một dòng `pg_dump`** làm toàn bộ chiến lược sao
lưu, mục 14 tự liệt kê hàng loạt hạng mục chưa kiểm chứng. Không có cơ chế sao lưu tự động,
không có đường phục hồi, không có HTTPS tự động, không có cách nào để quản trị viên tự sao lưu
từ giao diện.

Dữ liệu ở đây là sổ sách giáo xứ nhiều năm — **mất là không lấy lại được**. Người vận hành phần
lớn là quý cha, quý sơ, không rành máy tính.

### 1.2 Mục tiêu

1. **Không mất dữ liệu.** RPO ≤ 6 giờ trong vận hành bình thường; bằng 0 ngay trước các thao tác
   rủi ro (cập nhật phần mềm, phục hồi).
2. **Phục hồi được thật.** Không chỉ có bản sao lưu, mà chứng minh được nó phục hồi được — bằng
   diễn tập tự động định kỳ, không phải bằng niềm tin. RTO ≤ 1 giờ.
3. **Bảo mật.** Bản sao lưu chứa họ tên, ngày sinh, số căn cước của hàng nghìn giáo dân. Phải mã
   hoá phía máy chủ trước khi rời máy. Ứng dụng web bị chiếm quyền không được kéo theo mất hoặc
   lộ bản sao lưu.
4. **Cài đặt một lệnh.** Từ VPS trắng tới hệ thống chạy được, HTTPS đầy đủ, sao lưu đã bật, bằng
   một lệnh duy nhất chạy lại được nhiều lần.
5. **Quản trị viên tự phục vụ.** Sao lưu và phục hồi thủ công ngay trên giao diện web.

### 1.3 Ngoài phạm vi

- Cao khả dụng (nhiều nút API, replica CSDL, tự động chuyển đổi dự phòng). Một máy chủ duy nhất.
- Point-in-time recovery cấp giây (WAL archiving). RPO 6 giờ đã được người dùng chọn.
- Kubernetes / Swarm. Docker Compose một nút.
- Sao lưu chéo nhiều nhà cung cấp. Một bucket R2 (khuyến nghị bật versioning + Object Lock).

### 1.4 Các quyết định nền đã chốt với người dùng

| Câu hỏi | Quyết định |
|---|---|
| Hạ tầng | Một VPS Linux tự quản |
| RTO mục tiêu | Tối đa ~1 giờ |
| Phạm vi phục hồi trên web | **Sao lưu + phục hồi đầy đủ trên web** (người dùng chọn phương án rủi ro hơn một cách có ý thức) |
| Phạm vi phục hồi | Toàn máy chủ, chỉ tài khoản `LoaiTaiKhoan=9` |
| Container | Có — Docker Compose |
| Bộ máy sao lưu | restic → Cloudflare R2 |
| Nơi chạy sao lưu/phục hồi | Trên host, ngoài container, điều phối qua bảng job trong CSDL |
| Nhịp sao lưu | 6 giờ/lần (4 lần/ngày) |
| Nguồn cài đặt | Tải trực tiếp từ GitHub (kho công khai); script cài kiêm luôn trình cập nhật |

## 2. Vì sao container, và ranh giới host/container

### 2.1 Vì sao Docker Compose

Ghim được đồng thời PostgreSQL 17, .NET 10 runtime và **Chromium + thư viện hệ thống của
Playwright** — thứ mà bản Dockerfile hiện tại đang thiếu và khiến toàn bộ tính năng in PDF hỏng
khi chạy trong container (xem mục 6.1). Cài trực tiếp lên máy (bare metal) đồng nghĩa với việc
cài tay .NET runtime, khoảng 20 thư viện hệ thống của Chromium, PostgreSQL và các unit systemd,
với sai khác giữa các bản phân phối Linux. Nâng cấp = đổi tag + `up -d`; quay lui = đổi tag về
cũ. Hạ tầng Compose đã có sẵn khoảng 80% và từng chạy được.

### 2.2 Ranh giới: cái gì trong container, cái gì trên host

**Trên host (ngoài container):**
- Bộ chạy công việc sao lưu/phục hồi (`qlgx-runner.sh`) và các systemd timer.
- Khoá R2, mật khẩu restic (`/etc/qlgx/backup.env`).
- Nhị phân `restic`.

**Trong container:** ứng dụng, CSDL, reverse proxy. Không có thành phần nào biết khoá R2.

Đây **không phải chọn lựa phong cách mà là ràng buộc kỹ thuật**: phục hồi toàn bộ CSDL đòi hỏi
ngắt mọi kết nối tới chính CSDL mà ứng dụng đang dùng và đổi tên nó — ứng dụng không thể tự làm
việc đó với chính mình. Ngoài ra, đường phục hồi phải chạy được **khi container không lên được**
và trên **một VPS hoàn toàn trắng**.

## 3. Bố cục trên máy chủ

```
/opt/qlgx/                     git clone thưa (sparse) kho khoannd/qlgx — chỉ thư mục WebApp
  WebApp/                      ← thư mục làm việc của mọi lệnh docker compose
    docker-compose.yml         postgres + api                       (đã có)
    docker-compose.prod.yml    overlay: caddy, chính sách restart, xoay vòng log
    .env                       chmod 600 — mật khẩu CSDL, khoá JWT   (script sinh)
    Caddyfile                  HTTPS tự động (Let's Encrypt)
    scripts/
      install.sh               cài đặt VÀ cập nhật (một script duy nhất)
      os_adapter.sh            lớp mỏng che khác biệt apt/dnf
      qlgx                     CLI vận hành: status|backup|restore|verify|update|rollback|logs
      qlgx-runner.sh           bộ chạy job — systemd timer gọi mỗi phút
      qlgx-restore.sh          phục hồi độc lập, chạy được trên VPS trắng
      sql/00-vai-tro-rls.sql   tạo hai vai trò RLS, idempotent
/usr/local/bin/qlgx            liên kết mềm tới scripts/qlgx — gõ `qlgx` ở đâu cũng được
/etc/qlgx/
  backup.env                   chmod 600 root — khoá R2 + mật khẩu restic (TÁCH khỏi /opt)
  the-phuc-hoi.txt             Thẻ phục hồi — in ra, cất ngoài máy chủ, rồi xoá
/var/lib/qlgx/spool/           runner đặt file cho admin tải; mount READ-ONLY vào container API
/var/log/qlgx/                 nhật ký job trên host (sống sót qua thao tác hoán đổi CSDL)
```

Ba nguyên tắc bố cục:

1. **Khoá R2 không nằm trong `/opt/qlgx`.** Thư mục ứng dụng bị lộ cũng không với tới bản sao lưu.
2. **Mọi lệnh `pg_dump`/`pg_restore`/`psql` chạy bên trong container postgres** (`docker compose
   exec -T postgres …`), không dùng client trên host — loại bỏ hoàn toàn nguy cơ lệch phiên bản
   giữa client và server.
3. **Không có `qlgx.service` riêng.** `restart: unless-stopped` cộng với Docker đã bật sẵn là đủ
   để hệ thống tự lên sau khi máy khởi động lại. Thêm một lớp systemd bọc Compose chỉ tạo ra hai
   nguồn sự thật về "hệ thống đang chạy hay không".

## 4. Script cài đặt

### 4.1 Một lệnh, một script, hai vai trò

```bash
curl -fsSL https://raw.githubusercontent.com/khoannd/qlgx/webapp-phase-1/WebApp/scripts/install.sh | sudo bash
```

Script tự nhận biết trạng thái bằng sự tồn tại của `/opt/qlgx/WebApp/.env`. Dùng chung một script cho
cài mới và cập nhật là có chủ đích: nó loại bỏ loại lỗi kinh điển "đường cài mới thì đúng, đường
nâng cấp thiếu bước" — đúng cái bẫy số 1 đã ghi trong `QUY_TRINH_PHAT_HANH.md` của bản desktop.

### 4.2 Luồng cài mới (12 bước)

1. **Kiểm tra tiền đề**: quyền root; họ bản phân phối (Ubuntu 22.04+/Debian 12+/Rocky 9 qua
   `os_adapter.sh`); tối thiểu 2 vCPU, 4 GB RAM, 40 GB trống. Thiếu thì dừng với thông báo rõ,
   không cài dở dang.
2. **Cài phụ thuộc**: Docker Engine + compose plugin, `restic`, `git`, `curl`. Bỏ qua cái đã có.
3. **Cấu hình tường lửa**: `ufw`/`firewalld` chỉ mở 22, 80, 443. Cổng PostgreSQL không bao giờ
   mở ra ngoài (compose cũng không map ra host).
4. **Lấy mã nguồn**: `git clone --depth 1 --filter=blob:none --sparse --branch <nhánh>` vào
   `/opt/qlgx`, rồi `git sparse-checkout set WebApp`. Chỉ lấy đúng thư mục cần: `BIN/`,
   `Release/`, `Source/` là bản desktop Windows, vô dụng trên máy chủ Linux và chiếm hàng trăm
   MB nhị phân.
5. **Sinh `.env` idempotent**: mật khẩu ba vai trò CSDL, `QLGX_JWT_KEY` (`openssl rand -base64
   32`) đều sinh ngẫu nhiên; **khoá đã tồn tại thì giữ nguyên tuyệt đối**. Ghi bằng hàm
   `set_env_kv` xử lý đúng trường hợp file không kết thúc bằng ký tự xuống dòng.
6. **Cấu hình sao lưu**: hỏi (hoặc đọc từ biến môi trường khi `--non-interactive`) endpoint R2,
   tên bucket, access key/secret; sinh `RESTIC_PASSWORD` ngẫu nhiên; ghi
   `/etc/qlgx/backup.env` chmod 600 root.
7. **Khởi động PostgreSQL**, chờ healthy, chạy `sql/00-vai-tro-rls.sql` tạo `qlgx_app`
   (NOBYPASSRLS) và `qlgx_admin` (BYPASSRLS). Dùng `psql` chứ **không dùng
   `/docker-entrypoint-initdb.d`**, vì thư mục đó chỉ chạy khi volume dữ liệu còn trống — sẽ bị
   bỏ qua ở mọi lần chạy lại, đúng lúc ta cần tính idempotent nhất.
8. **Khởi động API**; migration tự chạy lúc khởi động dưới `pg_advisory_lock`; chờ endpoint
   **readiness** (mục 6.3) trả 200.
9. **Khởi tạo giáo xứ + tài khoản quản trị hệ thống** qua CLI `tao-tai-khoan-quan-tri` có sẵn,
   mở rộng thêm khả năng tạo giáo xứ nếu chưa tồn tại (mục 6.4).
10. **HTTPS**: hỏi tên miền → sinh `Caddyfile` → bật Caddy (Let's Encrypt tự động, tự gia hạn).
    Không có tên miền thì chạy HTTP kèm cảnh báo rõ ràng và ghi lại trong bản tóm tắt cuối.
11. **Bật sao lưu**: **hỏi trước** rồi mới cài `qlgx-runner.timer` và `qlgx-verify.timer` (không
    lén cài dịch vụ chạy nền). Chạy **một lần sao lưu ngay lập tức + `restic check`** — chứng
    minh đường ống sống thật trước khi có dữ liệu thật.
12. **In Thẻ phục hồi** ra màn hình và ghi `/etc/qlgx/the-phuc-hoi.txt`, kèm hướng dẫn cất ra
    ngoài máy chủ rồi xoá file.

### 4.3 Luồng cập nhật

Chạy lại chính lệnh trên, hoặc `qlgx update`:

1. `git fetch`; so sánh commit hiện tại với remote. **Không có gì mới → in "Đã là bản mới nhất"
   và thoát**, không khởi động lại vô cớ.
2. **Sao lưu bắt buộc** `--tag truoc-cap-nhat` (chỉ bỏ qua được bằng `--skip-backup`).
3. Ghi commit hiện tại vào `/opt/qlgx/WebApp/.phien-ban-truoc`.
4. `git pull` → `docker compose build` → `up -d`.
5. Chờ readiness. **Thất bại → tự động quay lui** về commit cũ, `up -d` lại, báo rõ lý do quay
   lui. Nếu migration đã chạy và làm hỏng lược đồ, hướng dẫn dùng `qlgx restore` với snapshot
   `truoc-cap-nhat` vừa tạo ở bước 2.

`.env` và toàn bộ `/etc/qlgx/` **không bao giờ bị ghi đè** khi cập nhật; chỉ bổ sung khoá mới nếu
bản mới cần thêm biến, luôn kèm giá trị mặc định. Sinh lại `QLGX_JWT_KEY` sẽ đăng xuất toàn bộ
người dùng; sinh lại mật khẩu CSDL sẽ khiến API không kết nối được vào chính CSDL đang chạy.

Có `qlgx-update.timer` chạy hằng tuần nhưng **mặc định TẮT**, phải bật tay. Cập nhật tự động
không giám sát trên dữ liệu sổ sách giáo xứ là rủi ro không đáng.

### 4.4 Cờ dòng lệnh

`--non-interactive` (đọc toàn bộ từ biến môi trường, dùng cho CI/cài lại), `--domain=`,
`--branch=`, `--skip-backup`, `--dry-run` (in việc sẽ làm, không làm gì), `--status` (chỉ in
bảng kiểm chứng rồi thoát).

### 4.5 Tự kiểm chứng khi kết thúc

Script kết thúc bằng một bảng, mỗi dòng ĐẠT/KHÔNG ĐẠT, và **thoát với mã lỗi khác 0 nếu có bất
kỳ dòng nào không đạt**:

| Kiểm tra | Cách kiểm |
|---|---|
| Container `postgres`, `api`, `caddy` healthy | `docker compose ps --format json` |
| Migration đã áp dụng hết | `__EFMigrationsHistory` so với danh sách trong image |
| Hai vai trò RLS đúng thuộc tính | `rolbypassrls` phải là `false`/`true` tương ứng |
| RLS đang bật trên toàn bộ bảng nghiệp vụ | `pg_class.relrowsecurity` |
| `ConnectionStrings__QlgxQuanTri` khác `__Qlgx` | so sánh chuỗi trong `.env` |
| HTTPS trả 200 | `curl -fsS https://<tên miền>/api/suc-khoe` |
| Kho restic mở được và có ≥ 1 snapshot | `restic snapshots --json` |
| `/etc/qlgx/backup.env` quyền 600, chủ root | `stat` |

Bước kiểm chứng phải biết báo lỗi — nguyên tắc số 4 trong `CLAUDE.md`. Bộ kiểm thử của script sẽ
cố tình phá từng điều kiện để chứng minh bảng này không phải lúc nào cũng in "ĐẠT".

## 5. Cơ chế sao lưu

### 5.1 Sao lưu cái gì

Kết quả rà soát mã nguồn: **ảnh đại diện lưu dạng `bytea` trong PostgreSQL** (`AnhDaiDienService`
+ migration `20260907060641_ThemAnhDaiDienNhiPhan`), và ứng dụng **không ghi bất kỳ file nào
xuống đĩa máy chủ** (mẫu in là embedded resource trong `Qlgx.Api.csproj`; PDF sinh trong bộ nhớ
rồi trả thẳng về trình duyệt). Do đó **sao lưu CSDL gần như là sao lưu toàn bộ hệ thống**.

Mỗi snapshot gồm ba thành phần, luôn đi cùng nhau:

| Thành phần | Nội dung | Vì sao cần |
|---|---|---|
| `db` | `pg_dump -Fc` CSDL ứng dụng | Toàn bộ dữ liệu giáo xứ, gồm cả ảnh |
| `globals` | `pg_dumpall --globals-only` | Hai vai trò RLS và mật khẩu — thiếu thì phục hồi lên máy trắng sẽ hỏng RLS |
| `config` | `.env`, `docker-compose.yml`, `Caddyfile` | Khoá JWT, mật khẩu CSDL, tên miền |

`/etc/qlgx/backup.env` **cố ý không nằm trong bản sao lưu**: không tự mã hoá một tệp bằng chính
khoá chứa trong tệp đó. Đó chính là lý do phải có Thẻ phục hồi cất ngoài máy chủ.

### 5.2 Đường ống

```
systemd timer (00:00, 06:00, 12:00, 18:00 giờ VN, RandomizedDelaySec=300)
  └─ qlgx-runner.sh
       ├─ docker compose exec -T postgres pg_dump -Fc          ─┐
       ├─ docker compose exec -T postgres pg_dumpall --globals  ├─→ restic backup ─→ R2
       └─ /opt/qlgx/WebApp/{.env,docker-compose.yml,Caddyfile}        ─┘   (mã hoá phía máy chủ)
```

Ba ràng buộc bắt buộc của đường ống:

**Dump không nằm lâu ở dạng thô.** `pg_dump` ghi vào thư mục tạm `chmod 700` do runner tạo bằng
`mktemp -d`, xoá ngay sau khi restic đọc xong, bằng `trap … EXIT` để xoá cả khi lỗi hoặc bị
ngắt.

**Một snapshot chỉ hợp lệ khi cả ba thành phần cùng vào.** `pg_dump` lỗi thì không chạy `restic
backup` phần nào cả. Thà không có snapshot còn hơn có snapshot thiếu CSDL mà tới lúc phục hồi
mới phát hiện.

**Chỉ dọn bản cũ sau khi bản mới đã được xác nhận đọc được.** Thứ tự cứng: `backup` → `check
--read-data-subset=5%` → `forget --prune`. Không bao giờ đảo.

Chống chồng lấn: mỗi lượt chạy giữ một `flock` trên `/var/lock/qlgx-runner.lock`; timer nổ khi
lượt trước còn chạy thì thoát ngay, ghi log.

### 5.3 Lịch và vòng đời

- **4 lần/ngày** (mỗi 6 giờ) → RPO ≤ 6 giờ.
- **Sao lưu theo sự kiện** bù cho nhịp thưa: chạy ngay lập tức trước mỗi lần cập nhật phần mềm
  (`--tag truoc-cap-nhat`) và trước mỗi lần phục hồi (`--tag truoc-phuc-hoi`). Đây là hai thời
  điểm rủi ro nhất, phải có ảnh chụp sát giây.
- **Giữ**: `--keep-last 8 --keep-daily 30 --keep-weekly 12 --keep-monthly 24`. Dùng `--keep-last
  8` thay `--keep-hourly` vì với nhịp 6 giờ thì `--keep-hourly` vô nghĩa; `last 8` giữ đủ bốn mốc
  của hai ngày gần nhất. Giữ 24 tháng: sổ sách giáo xứ đáng giữ lâu, và nhờ khử trùng lặp thì
  chi phí gần như bằng không.
- **Diễn tập phục hồi hằng tuần** (`qlgx verify`, Chủ nhật 03:00): lấy snapshot mới nhất, phục
  hồi vào CSDL tạm `qlgx_dien_tap`, đếm `giao_dan`/`gia_dinh`/`bi_tich_chi_tiet`, kiểm RLS còn
  bật, rồi **xoá CSDL tạm**. Đây là ranh giới giữa "có sao lưu" và "có khả năng phục hồi": một
  bản sao lưu chưa từng được phục hồi thử chỉ là một giả định.
- **Cảnh báo khi im lặng**: quá 8 giờ không có snapshot mới, hoặc diễn tập thất bại, hoặc `restic
  check` thất bại → ghi trạng thái đỏ và hiện băng cảnh báo trên đầu giao diện quản trị. Kiểu
  hỏng nguy hiểm nhất của sao lưu là hỏng âm thầm sáu tháng rồi mới lộ ra đúng hôm cần dùng.

### 5.4 Bảo mật

- Khoá R2 và mật khẩu restic **chỉ nằm trong `/etc/qlgx/backup.env`** (chmod 600, chủ root), nạp
  bởi systemd unit chạy dưới quyền root trên host.
- Container API **không mount tệp đó, không biết khoá, không bao giờ gọi `restic`**. Ứng dụng bị
  chiếm quyền hoàn toàn vẫn không xoá được bản sao lưu trên R2.
- restic mã hoá phía máy chủ (AES-256, xác thực Poly1305) — R2 chỉ thấy các khối nhị phân không
  đọc được.
- **Khuyến nghị bật bucket versioning và Object Lock trên R2**: script in hướng dẫn nhưng không
  tự bật, vì cần quyền API cấp cao hơn quyền mà token sao lưu nên có. Token R2 dùng cho sao lưu
  chỉ cần quyền đọc/ghi trên đúng một bucket.
- Cổng PostgreSQL không map ra host; tường lửa chỉ mở 22/80/443.

## 6. Các lỗ hổng trong mã hiện tại mà thiết kế này phải bịt

Phát hiện từ bước rà soát mã, đều nằm trên đường tới hệ thống chạy thật:

### 6.1 Dockerfile thiếu Chromium — in PDF hỏng trong container

Image chạy dựa trên `mcr.microsoft.com/dotnet/aspnet:10.0` và chỉ cài thêm `curl`. Trong khi đó
toàn bộ đường in PDF là: mẫu HTML (embedded resource) → Chromium không giao diện qua Playwright →
PDF trong bộ nhớ. Không có Chromium thì **cả 13 mẫu in đều hỏng** ngay lần đầu người dùng bấm
in — dù đã chạy tốt khi phát triển trên Windows. Sửa: thêm vào giai đoạn runtime việc cài các
thư viện hệ thống Chromium cần và tải trình duyệt Playwright (`playwright install --with-deps
chromium`) ở đúng đường dẫn mà `PLAYWRIGHT_BROWSERS_PATH` trỏ tới, và **thêm một kiểm tra khói in
PDF vào bảng tự kiểm chứng ở mục 4.5**.

### 6.2 `ChuoiKetNoiQuanTri` âm thầm hạ cấp bảo mật

`GetConnectionString("QlgxQuanTri") ?? GetConnectionString("Qlgx")` cộng với mặc định
`QLGX_ADMIN_DB_USER:-${QLGX_APP_DB_USER}` trong compose nghĩa là: quên đặt vai trò quản trị thì
hệ thống vẫn chạy bình thường, chỉ là **không còn ranh giới BYPASSRLS**, và không có gì báo. Sửa:
khi `ASPNETCORE_ENVIRONMENT=Production`, hai chuỗi kết nối **trùng nhau thì từ chối khởi động**
với thông báo rõ. Fallback im lặng chỉ chấp nhận được ở môi trường phát triển.

### 6.3 `/api/suc-khoe` không chạm CSDL

Endpoint hiện tại trả "ok" ngay cả khi PostgreSQL đã chết — nó là liveness, không phải readiness.
Script cài và luồng cập nhật đều dựa vào tín hiệu "hệ thống đã sẵn sàng" để quyết định có quay
lui hay không, nên tín hiệu đó phải trung thực. Thêm `/api/suc-khoe/san-sang`: chạy `SELECT 1`,
kiểm tra migration đã áp dụng đủ, trả 503 khi chưa. Giữ nguyên `/api/suc-khoe` cho healthcheck
của Docker (nhẹ, gọi mỗi 10 giây).

### 6.4 Chưa có cách tạo giáo xứ đầu tiên bằng dòng lệnh

`tao-tai-khoan-quan-tri` yêu cầu `QLGX_ADMIN_GIAO_XU_ID` hoặc `_TEN` trỏ tới giáo xứ **đã tồn
tại**. Trên hệ thống mới cài thì chưa có giáo xứ nào. Bổ sung cờ tạo giáo xứ nếu chưa tồn tại,
để bước 9 của script cài chạy được không cần thao tác tay.

### 6.5 Chưa có reverse proxy / HTTPS

`TRIEN-KHAI.md` mục 14 tự ghi nhận là chưa kiểm chứng. Bổ sung dịch vụ Caddy trong
`docker-compose.prod.yml`: HTTPS tự động, tự gia hạn, thêm các header bảo mật (HSTS,
`X-Content-Type-Options`, `Referrer-Policy`), và giới hạn kích thước body cho endpoint tải ảnh.

## 7. Luồng phục hồi

Đây là phần quan trọng nhất của thiết kế. Vì người dùng đã chọn cho phép **phục hồi đầy đủ ngay
trên web**, toàn bộ rào an toàn tập trung ở đây.

### 7.1 Nguyên tắc: không bao giờ ghi đè tại chỗ

Cách sai — và là cách hầu hết hướng dẫn trên mạng chỉ — là `dropdb && createdb && pg_restore`.
Sai vì giữa `dropdb` và `pg_restore` thành công có một khoảng thời gian **không tồn tại dữ liệu
nào cả**; `pg_restore` lỗi giữa chừng là mất trắng.

Cách của thiết kế này — phục hồi sang bên rồi hoán đổi tên:

```
1. Sao lưu ngay trạng thái hiện tại  (--tag truoc-phuc-hoi)   ← bắt buộc, không tắt được
2. pg_restore snapshot đã chọn → CSDL MỚI  qlgx_phuc_hoi_<ts>
3. Kiểm chứng CSDL mới: đếm giao_dan/gia_dinh/bi_tich_chi_tiet, RLS còn bật đủ số bảng,
   số giáo xứ đúng, lược đồ khớp phiên bản migration hiện hành
   → KHÔNG ĐẠT: dừng, xoá qlgx_phuc_hoi_<ts>, hệ thống vẫn chạy nguyên vẹn
4. docker compose stop api                    ← ngắt kết nối
5. ALTER DATABASE qlgx               RENAME TO qlgx_truoc_phuc_hoi_<ts>;
   ALTER DATABASE qlgx_phuc_hoi_<ts> RENAME TO qlgx;
6. docker compose start api → chờ /api/suc-khoe/san-sang
   → không lên được: ĐẢO NGƯỢC hai lệnh RENAME, khởi động lại   ← quay lui trong vài giây
7. Giữ qlgx_truoc_phuc_hoi_<ts> thêm 7 ngày rồi tự xoá
```

Bước 5 là hai lệnh đổi tên siêu dữ liệu trong PostgreSQL — gần như tức thời và đảo ngược được.
Cửa sổ "hệ thống không có dữ liệu" bằng không: trước bước 5 dữ liệu cũ còn nguyên; sau bước 5 dữ
liệu mới đã sẵn sàng và đã kiểm chứng. Thời gian ngưng phục vụ chỉ bằng thời gian khởi động lại
container API (vài giây), không phải thời gian `pg_restore` (có thể vài phút).

Ở bước 5, các phiên còn sót được đóng bằng `pg_terminate_backend` sau khi API đã dừng, vì
`ALTER DATABASE … RENAME` không chạy được khi còn kết nối tới CSDL đó.

### 7.2 Rào an toàn trên giao diện web

Bốn lớp, cố ý gây khó chịu:

1. **Chỉ `LoaiTaiKhoan=9`** thấy được chức năng phục hồi. Kiểm ở tầng API, không chỉ ẩn nút.
2. **Chọn snapshot có ngữ cảnh**: bảng snapshot hiện thời điểm, nhãn, kích thước và **số bản ghi
   `giao_dan`/`gia_dinh` tại thời điểm đó** (runner ghi vào metadata mỗi lần sao lưu). Người dùng
   phải thấy rõ "phục hồi về đây nghĩa là quay lại 2050 giáo dân, mất 3 người mới nhập".
3. **Gõ tay chuỗi xác nhận** đúng `PHUC HOI <tên giáo xứ>`. Không dùng hộp thoại "bạn có chắc
   không?" — người dùng bấm OK theo phản xạ.
4. **Bảng đối chiếu trước–sau** ngay trước nút cuối: số bản ghi hiện tại cạnh số bản ghi trong
   snapshot, tô đỏ những dòng sẽ giảm.

Trong lúc phục hồi, ứng dụng hiện **màn hình chặn toàn trang** cho mọi người dùng, cập nhật trạng
thái theo từng bước. Xong thì **buộc tất cả đăng nhập lại** — dữ liệu đã đổi, phiên cũ giữ thông
tin đã lỗi thời.

### 7.3 Hai đường phục hồi song song

| | **Trên web** | **Dòng lệnh** (`qlgx-restore.sh`) |
|---|---|---|
| Dùng khi | Máy chủ còn sống; lỡ xoá hoặc nhập sai dữ liệu | Mất máy chủ, hỏng Docker, container không lên |
| Cần | Đăng nhập được, `LoaiTaiKhoan=9` | Thẻ phục hồi + kết nối mạng; chạy được trên VPS trắng |
| Cơ chế | Ghi job → runner thực thi | Chạy thẳng, không cần CSDL nào đang sống |

`qlgx-restore.sh` mặc định **chỉ in kế hoạch**, phải thêm `--apply` mới thực hiện — cùng triết lý
với `terraform plan`. Nhận `--card /đường/dẫn/the-phuc-hoi.txt` để lấy khoá. Khôi phục toàn bộ hệ
thống từ số không chỉ cần: một VPS trắng và tờ Thẻ phục hồi.

### 7.4 Bảng job

Bảng `cong_viec_sao_luu`, theo đúng tiền lệ `nhap_du_lieu_job` đã có trong mã:

| Cột | Kiểu | Ghi chú |
|---|---|---|
| `id` | uuid | |
| `loai` | text | `sao_luu` / `phuc_hoi` / `kiem_tra` / `dien_tap` / `tai_ve` / `dong_bo_danh_sach` |
| `trang_thai` | text | `cho` / `dang_chay` / `xong` / `loi` |
| `tham_so` | jsonb | id snapshot, nhãn… |
| `nhat_ky` | text | nối thêm theo từng bước |
| `buoc_hien_tai` | text | để giao diện hiện tiến trình |
| `nguoi_tao` | uuid | tài khoản đã bấm |
| `tao_luc`/`bat_dau_luc`/`ket_thuc_luc` | timestamptz | |

Runner giành job bằng:

```sql
UPDATE cong_viec_sao_luu SET trang_thai='dang_chay', bat_dau_luc=now()
WHERE id = (SELECT id FROM cong_viec_sao_luu WHERE trang_thai='cho'
            ORDER BY tao_luc LIMIT 1 FOR UPDATE SKIP LOCKED)
RETURNING id::text, loai, coalesce(tham_so::text,'{}');
```

`FOR UPDATE SKIP LOCKED` bảo đảm hai lượt chạy chồng nhau không thể lấy trùng một job. **API chỉ
INSERT và SELECT trên bảng này — không bao giờ tự chạy lệnh hệ thống.**

Bảng này thuộc phạm vi toàn máy chủ, không gắn `giao_xu_id`, và chỉ truy cập được qua vai trò
`qlgx_admin`; nó nằm ngoài mô hình RLS đa giáo xứ một cách có chủ đích, vì thao tác của nó tác
động tới toàn bộ máy chủ.

**Điểm tinh tế bắt buộc phải xử lý**: job `phuc_hoi` khiến chính CSDL chứa bảng job bị thay thế ở
bước 5. Runner do đó **ghi nhật ký phục hồi ra `/var/log/qlgx/phuc-hoi-<ts>.log` trên host** song
song với ghi vào bảng, và sau khi hoán đổi xong thì **ghi lại kết quả cuối cùng vào bảng job của
CSDL mới**. Không tính trước điều này thì bản ghi job biến mất cùng CSDL cũ, và người dùng không
bao giờ biết việc phục hồi kết thúc ra sao.

## 8. Giao diện quản trị

### 8.1 Vị trí và chuẩn giao diện

Menu **Hệ thống → Sao lưu & Phục hồi**, mở thành một tab như mọi màn hình khác (`useTabDocs`),
chỉ hiện với `LoaiTaiKhoan=9`. Thay thế đúng chỗ ba mục "Nhập/Sao lưu/Khôi phục dữ liệu" hiện
đang bị khoá trong menu — di sản của mô hình desktop, nay có ý nghĩa trở lại trong mô hình tập
trung.

Màn hình bám **chuẩn UX của Danh sách giáo dân** (chuẩn đã dùng trong đợt kiểm thử chấp nhận):
heading + pill đếm, thanh nút hành động, **`GxGrid`/AG-Grid** cho danh sách snapshot (không dùng
`<table>` thường — đúng điểm bất nhất đã ghi nhận ở báo cáo nhóm 1–2), ngày giờ định dạng
**dd/mm/yyyy HH:mm** (không ISO — đúng điểm bất nhất đã ghi nhận ở nhóm 3).

### 8.2 Bố cục

**Khối 1 — Tình trạng sao lưu** (trên cùng):

> 🟢 **Bình thường** — Sao lưu gần nhất: 13/09/2026 06:00 (2 giờ trước) · 47 bản sao ·
> Diễn tập phục hồi gần nhất: 08/09/2026 — Đạt

Ba trạng thái: xanh; vàng (quá 8 giờ chưa có bản mới); **đỏ** (sao lưu lỗi, `restic check` lỗi,
hoặc diễn tập thất bại). Trạng thái đỏ còn hiện thành **băng cảnh báo trên đầu mọi màn hình** cho
tài khoản quản trị hệ thống.

**Khối 2 — Danh sách bản sao lưu** (`GxGrid`): Thời điểm · Nhãn · Kích thước · Số giáo dân · Số
gia đình · Nguồn (tự động / thủ công / trước cập nhật / trước phục hồi). Mỗi dòng có **Tải về** và
**Phục hồi**.

**Khối 3 — Thanh hành động**: `Sao lưu ngay` · `Kiểm tra tính toàn vẹn` · `Diễn tập phục hồi` ·
`Tải lại`.

**Khối 4 — Nhật ký công việc**: job gần đây, trạng thái, thời lượng, nút xem nhật ký chi tiết.

### 8.3 Tương tác bất đồng bộ toàn phần

Mọi nút **chỉ tạo job rồi trả về ngay**. Không API nào chạy đồng bộ chờ `pg_dump` xong: dump một
CSDL có ảnh bytea có thể mất vài phút, vượt timeout của reverse proxy và làm treo một luồng của
ứng dụng.

Giao diện hỏi trạng thái mỗi 2 giây khi có job đang chạy, hiện tiến trình theo bước. Chọn polling
thay vì WebSocket/SignalR: một người dùng, vài lần một tháng — thêm hạ tầng realtime là phức tạp
không cần thiết, và polling vẫn hoạt động khi container API vừa khởi động lại sau bước hoán đổi,
trong khi WebSocket thì đứt.

### 8.4 Tải bản sao lưu về máy

Container API không có khoá R2, nên luồng đi vòng qua thư mục spool:

1. Admin bấm **Tải về** → tạo job `tai_ve`.
2. Runner trên host `restic restore` snapshot đó ra
   `/var/lib/qlgx/spool/<ma-job>/qlgx-<ts>.dump`, ghi kích thước và hạn dùng vào bảng job.
3. Spool được **mount READ-ONLY** vào container API; API stream file về trình duyệt.
4. File **tự xoá sau 24 giờ** (runner dọn ở mỗi lượt chạy).

File tải về là **bản dump chưa mã hoá chứa toàn bộ dữ liệu giáo dân**. Giao diện cảnh báo rõ điều
này ngay tại nút tải; tên file có dấu thời gian để không đè nhau trong thư mục Downloads.

### 8.5 Điểm cuối API

| Điểm cuối | Việc |
|---|---|
| `GET /api/sao-luu/tinh-trang` | khối 1 |
| `GET /api/sao-luu/danh-sach` | khối 2 — đọc **bảng đệm snapshot** do runner cập nhật |
| `POST /api/sao-luu/cong-viec` | tạo job (`loai` + `tham_so`) |
| `GET /api/sao-luu/cong-viec`, `/{id}` | khối 4 + polling |
| `GET /api/sao-luu/tai-ve/{maCongViec}` | stream file từ spool |

Tất cả yêu cầu `LoaiTaiKhoan=9`, kiểm ở tầng API. Job loại `phuc_hoi` bắt buộc kèm trường
`xacNhan` đúng chuỗi `PHUC HOI <tên giáo xứ>`, **kiểm lại ở server** — không tin rằng frontend đã
hỏi.

Bảng đệm danh sách snapshot là một đánh đổi có chủ đích: API *có thể* gọi `restic snapshots
--json` để lấy danh sách tươi, nhưng như vậy container API phải có khoá R2 và nhị phân restic,
phá vỡ toàn bộ ranh giới bảo mật ở mục 5.4 chỉ để tiết kiệm một bảng. Đổi lại, danh sách có thể
trễ tối đa 6 giờ; nút "Tải lại" tạo job `dong_bo_danh_sach` để cập nhật ngay khi cần.

## 9. Tài liệu

Bốn tài liệu tiếng Việt, viết cho người **không rành máy tính**, đặt trong `WebApp/docs/`:

1. **`CAI-DAT-MAY-CHU.md`** — yêu cầu máy chủ (2 vCPU / 4 GB RAM / 40 GB SSD, Ubuntu 24.04),
   chuẩn bị tên miền, tạo bucket R2 và khoá API theo từng bước, chạy một lệnh cài, và **việc cần
   làm ngay sau khi cài: in Thẻ phục hồi và cất ngoài máy chủ**. Kèm mục xử lý sự cố (cổng 80/443
   bận, DNS chưa trỏ, hết đĩa, R2 từ chối khoá).
2. **`SAO-LUU-PHUC-HOI.md`** — sao lưu chạy khi nào, cách đọc đèn trạng thái, khi nào nên và khi
   nào **không** nên phục hồi, hai đường phục hồi, và mục **"Cứu hộ khi mất máy chủ"** viết dạng
   danh sách đánh số dùng được trong lúc hoảng loạn.
3. **`THE-PHUC-HOI.md`** — mẫu Thẻ phục hồi và giải thích từng dòng, kèm cảnh báo thẳng: mất thẻ
   này thì **không ai** phục hồi được bản sao lưu, kể cả Cloudflare — đó là bản chất của mã hoá
   phía máy chủ.
4. Cập nhật **`WebApp/TRIEN-KHAI.md`**: thay mục 5 (SQL chép-dán tay) và mục 12 (một dòng
   `pg_dump`) bằng liên kết tới hai tài liệu trên; cập nhật mục 14 theo những gì thiết kế này đã
   giải quyết.

`install.sh` **tự in hướng dẫn rút gọn** khi chạy xong — người vừa cài xong thường không mở tài
liệu, họ đọc cái đang hiện trên màn hình.

## 10. Kiểm thử

| Hạng mục | Cách kiểm |
|---|---|
| `install.sh` cài mới | Container Ubuntu 24.04 sạch → chạy script → bảng tự kiểm chứng toàn ĐẠT |
| `install.sh` idempotent | Chạy lại lần hai → không đổi secret nào, không dựng lại container vô cớ |
| Luồng cập nhật | Cài ở commit cũ → chạy lại script → lên bản mới, dữ liệu còn nguyên |
| Tự quay lui khi cập nhật hỏng | Cố tình đưa migration lỗi → xác nhận đã quay lui, hệ thống lên lại |
| Bảng tự kiểm chứng biết báo lỗi | Phá từng điều kiện một → mỗi lần phải có đúng một dòng KHÔNG ĐẠT và mã thoát khác 0 |
| Sao lưu | Chạy `qlgx backup` → snapshot có đủ ba thành phần; `restic check` đạt |
| Snapshot thiếu thành phần bị từ chối | Cố tình cho `pg_dump` lỗi → xác nhận không snapshot nào được tạo |
| Phục hồi trên web | Xoá vài bản ghi trên CSDL thử → phục hồi → số liệu khớp lại |
| Phục hồi thất bại thì quay lui | Cho `pg_restore` lỗi ở bước 2 → hệ thống vẫn chạy, CSDL tạm đã bị xoá |
| Phục hồi từ máy trắng | VPS mới + Thẻ phục hồi + `qlgx-restore.sh --apply` → hệ thống chạy đủ |
| Ranh giới đặc quyền | `docker compose exec api env` **không** chứa khoá R2 nào |
| In PDF trong container | Gọi một mẫu in bất kỳ, kiểm PDF trả về hợp lệ và có tiếng Việt đúng dấu |
| Chặn fallback chuỗi kết nối | Đặt hai chuỗi trùng nhau ở Production → API phải từ chối khởi động |
| Phân quyền | Tài khoản `LoaiTaiKhoan≠9` gọi thẳng API sao lưu → 403 |

Test đơn vị cho logic thuần (phân tích metadata snapshot, tính trạng thái đèn, kiểm chuỗi xác
nhận) chạy trong bộ test .NET/Vitest sẵn có. Kịch bản end-to-end của installer chạy trong
container dùng một lần, **không bao giờ chạy trên máy người dùng** (`CLAUDE.md` nguyên tắc số 3).

## 11. Rủi ro đã biết và cách giảm nhẹ

| Rủi ro | Giảm nhẹ |
|---|---|
| Mất Thẻ phục hồi → mất vĩnh viễn khả năng đọc bản sao lưu | Script in thẻ và bắt xác nhận đã cất ra ngoài; tài liệu riêng cảnh báo; giao diện admin hiện nhắc nhở nếu `/etc/qlgx/the-phuc-hoi.txt` vẫn còn trên máy chủ sau 7 ngày |
| Admin bấm phục hồi nhầm | Bốn lớp rào ở mục 7.2, sao lưu bắt buộc trước, giữ CSDL cũ 7 ngày |
| Chỉ một bucket R2 → phụ thuộc một nhà cung cấp | Khuyến nghị bật versioning + Object Lock; `qlgx backup --repo <khác>` cho phép thêm đích thứ hai về sau mà không đổi thiết kế |
| Đĩa đầy do CSDL tạm khi phục hồi/diễn tập | Kiểm dung lượng trống trước bước 2; diễn tập xoá CSDL tạm bằng `trap` kể cả khi lỗi |
| Cập nhật tự động làm hỏng dữ liệu | Timer cập nhật mặc định tắt; sao lưu bắt buộc trước khi cập nhật; tự quay lui |
| Một phiên Claude khác làm việc song song trên kho này | `CLAUDE.md` — kiểm `git branch --show-current`, không `git checkout`, dùng `git worktree` |

## 12. Việc thiết kế này KHÔNG đụng tới

- Toàn bộ phần desktop (`Source/`, `BIN/`, `Release/`, các `.ps1` phát hành).
- Máy chủ cập nhật của bản desktop (`landing/`, các đường dẫn `/capnhat/*`, `/version.txt`,
  `/VersionConfig.xml`, `/download.asp`, `/help/thong_tin_cap_nhat.htm`, `/4.0/`) — giữ nguyên
  tuyệt đối, kể cả yêu cầu trả lời được qua `http://` thuần.
- Ba khiếm khuyết giao diện phát hiện trong đợt kiểm thử chấp nhận (cuộn ngang dùng chung giữa
  các tab, biểu đồ trống lúc mở, các điểm bất nhất định dạng ngày và kiểu lưới). Chúng được ghi
  nhận riêng và xử lý trong một đợt khác; thiết kế này chỉ cam kết **không lặp lại** chúng ở màn
  hình mới.
