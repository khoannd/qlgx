# Landing page — Quản Lý Giáo Xứ

Trang giới thiệu phần mềm Quản Lý Giáo Xứ. **Next.js 16 (App Router) + TypeScript +
Tailwind CSS v4.** Dựng như một template: nội dung thay đổi theo thời gian đã tách
sau một lớp adapter để gắn CMS mà không đụng tới giao diện.

## Chạy thử

```bash
npm install
npm run dev      # http://localhost:3000
```

| Lệnh | Việc |
| --- | --- |
| `npm run build` / `npm start` | Build và chạy bản production trên Node.js |
| `npm run typecheck` | Kiểm tra kiểu TypeScript |
| `npm run cf:build` | Build cho Cloudflare Workers |
| `npm run cf:preview` | Chạy thử trên runtime Workers ngay ở máy |
| `npm run cf:deploy` | Build rồi đưa lên Cloudflare |
| `npm run d1:migrate:local` / `:remote` | Tạo bảng D1 (cục bộ / thật) |
| `npm run d1:seed:generate` | Sinh lại `migrations/0004_seed.sql` từ `static-provider.ts` |
| `npm run huong-dan:import` | Nhúng lại `BIN/help/*.htm` vào `/huong-dan` |

---

## Ngôn ngữ thiết kế — kính mờ

Bốn nguyên tắc, viết đầy đủ trong [globals.css](src/app/globals.css):

1. **Không có viền đậm.** Mọi mặt phẳng nổi lên nhờ đổ bóng nhiều lớp và một viền
   sáng mảnh (trắng bán trong), không phải nhờ nét kẻ đen.
2. **Nền phải có màu thì kính mới ra kính.** Trang có một lớp chuyển sắc cố định
   (xanh dương – tím – ngọc) ở `body::before`; các tấm kính làm nhoè lớp đó. Bỏ nó
   đi là toàn bộ hiệu ứng biến mất.
3. **Phản quang tạo cảm giác gương**: vệt sáng ở mép trên mỗi tấm, cộng một vệt
   sáng chéo quét ngang khi rê chuột (`.sheen`).
4. **Xanh dương là màu chủ đạo**, lấy theo giao diện của chính phần mềm.

### Bẫy màu của giao diện kính

Mỗi màu phụ có **hai biến thể**, và phải dùng đúng biến thể:

| Dùng để | Token | Ví dụ |
| --- | --- | --- |
| Tô mảng, vẽ chấm, nền nhạt | `--color-amber`, `--color-violet`, `--color-mint` | `bg-amber/12` |
| Làm màu chữ trên nền sáng | `--color-amber-ink`, `--color-violet-ink`, `--color-mint-ink` | `text-amber-ink` |

Lý do: nền kính thật là **#E8EDF8** (trắng ~70% chồng lên lớp chuyển sắc), không
phải trắng tinh. Đo trên nền trắng thì tưởng đạt, nhưng nền thật luôn tối hơn.
`#D97706` làm chữ chỉ đạt **2,4:1** ở đó — bản `-ink` mới đạt 5,5:1.

### Chữ

| Vai trò | Font | Vì sao |
| --- | --- | --- |
| Tiêu đề | **Fraunces** (biến thiên, bật trục SOFT + WONK) | Chọn sau khi dựng bảng so sánh 11 font ở cỡ tiêu đề thật với chuỗi tiếng Việt khó nhất. Dấu dày dặn và cân ở cỡ lớn. |
| Nội dung | **Inter** | Trung tính, dấu rõ ở cỡ nhỏ. |

> **Khi sửa tiêu đề:** tiếng Việt có dấu nặng nằm **dưới** chữ (ọ, ộ, ụ). `<h1>` cố
> ý để `leading-[1.28]`; thu lại là dấu chạm vào dòng kế tiếp.

---

## Cấu trúc

```
src/
  app/
    layout.tsx              Metadata SEO, nạp font, JSON-LD dùng chung
    page.tsx                Trang chủ (server component, async)
    globals.css             @theme tokens + .glass / .sheen / .btn / .prose-legacy
    sitemap.ts robots.ts    SEO
    api/tai-ve/[kenh]/      ★ Điểm tải về — tự tìm bản mới nhất rồi chuyển hướng
    tin-tuc/                Danh sách + chi tiết bài viết (SSG + ISR 5 phút)
    huong-dan/              Danh sách + chi tiết trang hướng dẫn (từ BIN/help/)
    admin/                  ★ Trang quản trị — sửa bản phát hành & bài viết qua D1
  components/
    glass.tsx               Sheen, Eyebrow, ShotFrame, bảng màu theo tone
    SiteHeader / SiteFooter / Screenshots / ArticleCard / Reveal / Icon / JsonLd
  lib/
    site.ts                 Nội dung cố định: điều hướng, tính năng, ảnh, FAQ
    huong-dan.ts             Đọc huong-dan-data.json, nhóm theo mục lục
    cloudflare-image-loader.ts
    cloudflare-runtime.ts     ★ Chặn getCloudflareContext khỏi Node thường — xem mục "bẫy" bên dưới
    admin/auth.ts            Đăng nhập, phiên, mật khẩu — cho /admin
    content/
      types.ts              ★ Hợp đồng dữ liệu + interface ContentProvider
      static-provider.ts    Nguồn tĩnh / dữ liệu mẫu cho seed D1
      d1-provider.ts        ★ Đọc + ghi D1 (bản phát hành, bài viết)
      index.ts              ★ Điểm nối — D1 trước, tự rơi về tĩnh nếu lỗi
migrations/
  0001_init.sql             Lược đồ D1
  0004_seed.sql             Sinh tự động — xem npm run d1:seed:generate
scripts/
  import-huong-dan.mjs      Chuyển BIN/help/*.htm → huong-dan-data.json
  generate-d1-seed.mjs      Sinh migrations/0004_seed.sql từ static-provider.ts
public/
  images/                   Ảnh chụp màn hình đã xử lý
  images/help/              Ảnh minh hoạ lấy từ BIN/help/image/
  _headers                  Header cache/bảo mật cho tệp tĩnh trên Cloudflare
```

---

## Điểm tải về

Trang **không bao giờ** nhúng thẳng đường dẫn tệp. Nút tải trỏ vào
`/api/tai-ve/full`, và endpoint đó mới quyết định phiên bản mới nhất rồi chuyển
hướng (302) sang GitHub.

**Chỉ một bộ cài công khai, không phải hai.** Bộ cài (`.msi` qua
`Source/GXInstaller/GXInstaller.vdproj`) đã tự xử lý cả cài mới lẫn nâng cấp: máy
chưa có QLGX thì cài mới, máy đã có bản cũ thì chạy thẳng bộ cài này lên là nó tự
nâng cấp tại chỗ — không cần gỡ trước. Đây là hành vi "Major Upgrade" chuẩn của
Windows Installer: `UpgradeCode` giữ cố định qua mọi phiên bản trong khi
`ProductCode` đổi mới mỗi lần phát hành (`release.ps1` tự làm việc này và tự kiểm
tra lại `UpgradeCode` không đổi trước khi ghi), cộng với
`RemovePreviousVersions=TRUE` và `DetectNewerInstalledVersion=TRUE` trong
`.vdproj`.

Route vẫn còn nhận `/api/tai-ve/update` (trỏ tới gói `.zip`), nhưng **không hiện
trên giao diện**: gói đó chỉ dành cho chính `AutoUpdate.exe` của chương trình tự
tải khi kiểm tra cập nhật, không phải một trình cài đặt — người dùng tự tải về rồi
tự chạy sẽ không biết làm gì với nó. Bản nháp đầu tiên của trang này từng hiện cả
hai bộ tải như hai lựa chọn ngang nhau, gây hiểu lầm; đã gộp lại còn một.

**Đã xác minh thực tế, không suy đoán:** repo `khoannd/qlgx` là repo thật của phần
mềm này, nhưng **không có GitHub Releases nào được publish**
(`/repos/khoannd/qlgx/releases/latest` trả 404 — bản build đầu tiên của trang này
trỏ vào đó nên nút tải bị 404). Bản cài đặt thật nằm ngay trong thư mục `Release/`
của repo dưới dạng file thường, và `Release/VersionConfig.xml` — cùng tệp mà bản
desktop dùng để tự kiểm tra cập nhật — là nguồn xác định phiên bản mới nhất.

Thứ tự xác định phiên bản:

1. `QLGX_LATEST_VERSION` — chốt cứng, dùng khi muốn ghim một bản cụ thể.
2. Đọc `Release/VersionConfig.xml` trực tiếp từ nhánh chính trên GitHub (giới hạn
   3 giây bằng `AbortSignal.timeout` — GitHub chậm hay mạng lỗi cũng không được để
   nút tải quay vô hạn).
3. Phiên bản đang hiển thị trên trang, để không bao giờ trả về lỗi trắng.

Nhờ vậy **phát hành bản mới chỉ cần đẩy tệp mới vào `Release/` và cập nhật
`VersionConfig.xml`** — đúng quy trình đã có sẵn từ trước — không phải sửa gì ở
trang web. Dùng 302 chứ không phải 301: 301 sẽ khiến trình duyệt nhớ vĩnh viễn và
tải mãi bản cũ.

| Biến | Mặc định | Ý nghĩa |
| --- | --- | --- |
| `QLGX_GITHUB_REPO` | `khoannd/qlgx` | Repo chứa thư mục `Release/` |
| `QLGX_GITHUB_BRANCH` | `master` | Nhánh đọc file |
| `QLGX_TAG_PREFIX` | `v` | Dự phòng cho sau này nếu chuyển sang GitHub Releases thật |
| `QLGX_LATEST_VERSION` | *(trống)* | Đặt để chốt cứng phiên bản |

Tên tệp suy ra theo quy ước `qlgx_<phiên_bản>.exe` và `qlgx_<phiên_bản>_update.zip`
(dấu chấm đổi thành gạch dưới) — đúng quy ước đặt tên đã dùng từ bản 3.2 tới nay.

**Về lâu dài nên chuyển sang GitHub Releases thật** (mỗi bản phát hành là một
Release có đính kèm file, có ghi chú thay đổi, không làm phình dung lượng repo
theo thời gian như để file trong `Release/`). Khi đó sửa lại `resolveDownloadUrl`
trong route để ưu tiên gọi `/repos/{repo}/releases/latest` trước bước đọc
`VersionConfig.xml`.

### Bẫy đã gặp: nút tải chậm vì bộ nhớ đệm của Next không chạy trên Workers

Route trên gọi GitHub để tra `VersionConfig.xml` ở **mỗi lượt bấm tải** — bản đầu
dùng `fetch(..., { next: { revalidate: 300 } })` với kỳ vọng Next.js tự nhớ kết
quả trong 5 phút, nhưng tuỳ chọn đó **không có tác dụng gì trên Cloudflare
Workers**: dự án không cấu hình `incrementalCache` trong `open-next.config.ts`
(mặc định không có bộ nhớ đệm phân tán), nên mỗi request đều gọi thật sang
GitHub, cộng thêm độ trễ thấy rõ trước khi hộp thoại lưu tệp hiện ra.

Đã sửa bằng Cache API **gốc của Worker** (`caches.default`) — khác hẳn cache của
Next, luôn hoạt động bất kể có cấu hình `incrementalCache` hay không. Chỉ lưu
đúng chuỗi phiên bản đã trích ra (không lưu nguyên XML), TTL 5 phút qua header
`Cache-Control`. Xem `fetchVersionFromRepo` trong route.

### Tải phiên bản cũ: `/api/tai-ve/phien-ban-cu/[phienBan]`

Endpoint riêng cho các bản **không phải mới nhất** — khác endpoint ở trên vì
phiên bản ở đây cố định (không tự đổi theo thời gian) nên dùng 301 thay vì 302.
Danh sách phiên bản hợp lệ nằm trong `src/lib/version-history.ts`
(`downloadableVersions`, lọc ra từ `versionHistory`) — endpoint đối chiếu qua
danh sách trắng này trước khi dựng URL sang GitHub, không nhận thẳng tên tệp từ
tham số URL.

`REPO`/`BRANCH` dùng chung với endpoint ở trên qua `src/lib/github.ts`, để hai
endpoint không bao giờ lệch nhau khi đổi biến môi trường.

---

## Máy chủ cập nhật — API mà chính phần mềm desktop tự gọi

**Khác hẳn** hai mục ở trên: `/api/tai-ve/*` là nút bấm TAY của người dùng trên trang
web; nhóm dưới đây là API mà chương trình `GiaoXu.exe` tự động gọi mỗi lần mở lên để
tự kiểm tra bản mới, không ai bấm gì cả.

Hợp đồng đầy đủ nằm ở `HOP_DONG_MAY_CHU_CAP_NHAT.md` tại **gốc kho `qlgx`** (không phải
trong `landing/`) — đọc file đó trước khi sửa bất cứ gì ở `src/lib/update-server.ts` hay
các route liệt kê dưới đây. Tóm tắt nhanh:

| Đường dẫn (chính thức, từ 4.0.2) | Trả về |
| --- | --- |
| `GET /capnhat/version.txt` | số phiên bản 4 phần, ví dụ `4.0.2.0`, KHÔNG BOM |
| `GET /capnhat/VersionConfig.xml` | chép nguyên xi `BIN/VersionConfig.xml` mới nhất |
| `GET /capnhat/download-update` | 302 sang gói `.zip` đúng phiên bản trong `qlgx_bin` |
| `GET /capnhat/help/thong_tin_cap_nhat.htm` | chép nguyên xi `BIN/help/thong_tin_cap_nhat.htm` |

Cộng thêm **8 đường dẫn cũ**, nội dung y hệt bốn cái trên, bắt buộc giữ sống mãi cho các
đời phần mềm phát hành trước khi có `/capnhat/` (xem bảng đầy đủ trong hợp đồng):
`/version.txt`, `/VersionConfig.xml`, `/download.asp`, `/help/thong_tin_cap_nhat.htm` ở
gốc domain (đời 3.3.7 trở về trước), và bản sao dưới `/4.0/` (đời 4.0.0–4.0.1).

**Nguồn dữ liệu — Cách B, tự đọc GitHub, không có bước tải file lên tay:**
`src/lib/update-server.ts` đọc `BIN/VersionConfig.xml` và `BIN/help/thong_tin_cap_nhat.htm`
từ kho `qlgx` (qua `GITHUB_RAW_BASE`), và suy ra tên gói `.zip` (từ thuộc tính `display`,
theo đúng quy ước `qlgx_<phiên_bản>_update.zip`) để 302 sang kho **`qlgx_bin`** (qua
`QLGX_BIN_RAW_BASE` — một repo GitHub **khác**, chỉ chứa file nhị phân, tách khỏi `qlgx`
để không làm phình kho mã nguồn). Kết quả: phát hành một bản mới chỉ cần `git push` cả
hai kho, không cần đụng gì tới `landing/` — xem `QUY_TRINH_PHAT_HANH.md` mục 5.1 ở gốc
kho `qlgx`.

Có nhớ tạm 5 phút bằng Cache API của Worker (cùng kỹ thuật với endpoint tải ở trên) —
bắt buộc, vì `version.txt` bị gọi ở **mỗi lần mở chương trình** bởi mọi máy đã cài QLGX,
không cache là dội GitHub liên tục.

**Vì sao các route nằm ở những đường dẫn "lạ"** (`src/app/4.0/`, `src/app/download.asp/`,
`src/app/capnhat/help/thong_tin_cap_nhat.htm/`): Next.js App Router chấp nhận tên thư mục
có dấu chấm làm một đoạn đường dẫn tĩnh bình thường — đã build và kiểm chứng thật, không
cần đổi tên gì để né.

**Bẫy đã lường trước khi viết (chưa xảy ra thật, nhưng suýt):** nếu chỉ 302-redirect
`help/thong_tin_cap_nhat.htm` sang thẳng `raw.githubusercontent.com` thay vì đọc nội dung
về rồi tự trả lời, trình duyệt sẽ nhận `Content-Type: text/plain` từ GitHub (raw phục vụ
mọi `.htm` như vậy) và hiện thẳng mã HTML dạng chữ thay vì render trang — nên ba nội dung
văn bản (`version.txt`, `VersionConfig.xml`, HTML) đều tải nội dung về rồi tự đặt lại
`Content-Type`, chỉ có gói `.zip` là 302 thật (client `.NET WebClient.DownloadFile` không
quan tâm `Content-Type`).

Kiểm chứng đã chạy thật (không chỉ tạo — xem mục "Đã kiểm chứng bằng trình duyệt" cho quy
ước chung của trang):

```bash
curl -s   https://quanlygiaoxu.net/capnhat/version.txt                 # "4.0.2.0", không BOM
curl -sL  https://quanlygiaoxu.net/capnhat/download-update -o t.zip    # tải được, PK\x03\x04, giải nén ra GiaoXu.exe
curl -sI  http://quanlygiaoxu.net/version.txt                          # http THUẦN, không bị ép https — đã xác nhận thật
curl -sI  http://quanlygiaoxu.net/4.0/version.txt                      # tương tự
```

---

## Trang `/phien-ban` — lịch sử phiên bản, thay cho một chủ đề trên diễn đàn

Trang này thay thế chủ đề "Thông tin cập nhật — Tải phần mềm" trên
forum.quanlygiaoxu.net. Diễn đàn dự kiến ngừng hoạt động trong tương lai, nên
thông tin cần một nơi ở lâu dài không phụ thuộc vào diễn đàn còn sống hay không.

Dữ liệu nằm trong `src/lib/version-history.ts` — **tĩnh, không sửa qua
`/admin`**, cùng lý do với `screenshots` trong `site.ts`: thêm một bản mới vào
đây luôn đi kèm việc thêm tệp cài đặt thật vào `Release/`, một việc ở tầng mã
nguồn chứ không phải nội dung biên tập thuần tuý.

Nội dung mỗi bản chia làm hai loại, tuỳ có kiểm chứng được hay không:

- **5 bản còn tệp cài đặt** (`4.0.0`, `3.3.3`, `3.3.5`, `3.3.6`, `3.3.7`) — nội
  dung "cải tiến"/"sửa lỗi" lấy **nguyên văn** từ đúng bài đăng gốc trên diễn
  đàn (đọc trực tiếp lúc soạn, đối chiếu số phiên bản và ngày). Bản `3.3.4` cũng
  còn tệp cài đặt nhưng **không có bài đăng riêng** trên diễn đàn (đã tra cả hai
  trang của mục "Thông tin cập nhật", không phải bỏ sót) — hiển thị không kèm
  danh sách thay đổi, ghi rõ lý do thay vì bịa nội dung.
- **33 bản cũ hơn nữa** (`3.3.2` trở về `Ra mắt phần mềm` năm 2009) — không còn
  tệp cài đặt để cung cấp tải, chỉ còn số phiên bản, ngày và liên kết tới bài
  đăng gốc, lấy từ chính tiêu đề 39 chủ đề trong mục "Thông tin cập nhật" (2
  trang, `viewforum.php?f=24` và `&start=25`).

Liên kết tới bài đăng gốc (`sourceUrl`) sẽ hỏng khi diễn đàn ngừng hoạt động —
chấp nhận được vì đó chỉ là trích dẫn nguồn cho phần lịch sử xa, không phải
thông tin cốt lõi của trang (thông tin cốt lõi — số phiên bản, ngày, nội dung 5
bản gần nhất — đã nằm hẳn trong mã nguồn, không phụ thuộc diễn đàn nữa).

---

## Liên hệ &amp; kênh hỗ trợ

Ba kênh, mỗi kênh một vai trò khác nhau, không kênh nào thay được kênh kia:

- **Diễn đàn** (`forum.quanlygiaoxu.net`) — nơi hỏi đáp có lưu vết, tìm lại được
  câu trả lời cũ. Sẽ ngừng hoạt động trong tương lai (xem mục trên).
- **Email** (`hotro@quanlygiaoxu.net`, hằng số `SUPPORT_EMAIL` trong
  `src/lib/site.ts`) — kênh 1-1, không công khai.
- **Facebook** (`facebook.com/qlgx2013`, hằng số `FACEBOOK_URL`) — kênh thông
  báo nhanh, theo gợi ý chính tác giả từng đăng trên diễn đàn.

Hiện diện ở ba nơi với mức độ khác nhau: **header** (icon gọn, chỉ hiện từ
`lg:` trở lên — màn hình nhỏ ưu tiên logo và nút menu), **footer** (đầy đủ nhãn
chữ, mọi kích thước màn hình), và mục "Hỗ trợ" trên trang chủ (`helpLinks`
trong `static-provider.ts`, sửa được qua `/admin/trang-chu`).

---

## Nội dung: D1 + trang quản trị `/admin`

**Có** — trang này dùng Cloudflare D1, và **có** một trang quản trị để sửa bản
phát hành và bài viết mà không cần build lại. Cách chọn nguồn dữ liệu nằm ở
[src/lib/content/index.ts](src/lib/content/index.ts): mọi trang chỉ gọi
`content.xxx()`, không trang nào biết dữ liệu đến từ đâu.

- **Có D1 binding `DB`** (đã deploy Cloudflare, đã tạo D1) → đọc/ghi D1. Đây là
  nguồn sửa được qua `/admin`.
- **Không có** (`next dev`/`next start` trên Node, hoặc D1 lỗi thoáng qua) → rơi về
  dữ liệu tĩnh trong [static-provider.ts](src/lib/content/static-provider.ts). Mỗi
  lời gọi tự thử D1 lại từ đầu — một truy vấn lỗi không làm "khoá" cả trang vào chế
  độ tĩnh cho những lời gọi sau.

### Cài đặt lần đầu

```bash
# 1. Tạo D1 database thật (cần tài khoản Cloudflare, `wrangler login` trước)
npx wrangler d1 create qlgx-content
# Dán "database_id" nó trả về vào wrangler.jsonc, mục d1_databases

# 2. Tạo bảng
npm run d1:migrate:remote

# 3. Nạp nội dung mẫu (bản phát hành hiện tại + các bài viết có sẵn)
npm run d1:seed:remote

# 4. Đặt mật khẩu quản trị (không đặt trong wrangler.jsonc — đó là secret)
npx wrangler secret put ADMIN_PASSWORD
```

Muốn thử trước khi đụng tới tài khoản Cloudflare thật: `npm run d1:migrate:local`
mô phỏng D1 bằng SQLite ngay trên máy (không cần đăng nhập) — wrangler coi
`0004_seed.sql` là một migration nên chạy lệnh này là có luôn cả lược đồ lẫn dữ
liệu mẫu. Đặt mật khẩu quản trị cho môi trường cục bộ bằng tệp `.dev.vars` (đã có
trong `.gitignore`, không commit):

```
# .dev.vars
ADMIN_PASSWORD=mat-khau-thu-nghiem
```

rồi `npm run cf:preview` để thử toàn bộ, kể cả `/admin`, trên runtime Workers thật
ngay tại máy.

### Trang quản trị

`/admin` — đăng nhập bằng `ADMIN_PASSWORD`, phiên lưu token ngẫu nhiên trong D1 +
cookie httpOnly 12 giờ. Sửa được:

- **Bản phát hành** (`/admin/release`): số phiên bản, ngày, danh sách thay đổi,
  danh sách tệp tải. Có hiệu lực trên trang chủ trong tối đa 5 phút (`revalidate`).
- **Bài viết** (`/admin/articles`): thêm/sửa/xoá, thân bài sửa dưới dạng một ô JSON
  (mảng `ArticleBlock`) — đây là công cụ v1, chưa phải trình soạn thảo WYSIWYG.
- **Nội dung trang chủ** (`/admin/trang-chu`): con số tóm tắt, vấn đề của sổ giấy,
  tính năng, bước cài đặt, liên kết hỗ trợ và câu hỏi thường gặp — xem mục dưới.

### Nội dung trang chủ sửa được tới đâu, và vì sao không sửa được hết

`/admin/trang-chu` sửa được các **danh sách** của trang chủ (kiểu `LandingContent`
trong [types.ts](src/lib/content/types.ts)). Hai thứ CỐ Ý để lại trong mã nguồn:

- **Ảnh chụp màn hình** — cần tệp ảnh thật trong `public/images/`, đã qua khâu cắt
  bỏ thanh tiêu đề Windows và thay tên giáo xứ mẫu (xem mục Ảnh chụp màn hình).
  Form chỉ đổi được đường dẫn chứ không tải ảnh lên, nên rủi ro trỏ vào ảnh không
  tồn tại quá cao khi không có bản xem trước.
- **Tiêu đề lớn đầu trang (Hero)** — chữ ở đó ngắt dòng thủ công theo đúng số đo
  phông Lora (xem chú thích trong [page.tsx](src/app/page.tsx)); sửa qua form rất
  dễ phá bố cục mà người sửa không thấy ngay.

**Vì sao form không có nút "thêm dòng":** toàn bộ `/admin` cố ý chỉ dùng form HTML
thuần, POST thẳng tới route handler, không phụ thuộc JavaScript phía trình duyệt.
Thay cho nút thêm/bớt, mỗi danh sách luôn render thêm vài **dòng trống** ở cuối:
gõ vào một dòng trống là thêm mục mới, xoá trắng ô đầu tiên của một dòng là bỏ mục
đó. Route handler bỏ qua mọi dòng thiếu trường bắt buộc.

Id của từng mục (tính năng, liên kết, câu hỏi) **tự sinh** từ tiêu đề — người sửa
không phải nghĩ ra id. Khác với slug bài viết: slug là một phần của URL công khai
nên vẫn phải gõ tay và kiểm soát trực tiếp.

Icon chỉ chọn được từ danh sách đóng (khớp với `IconName` và `paths` trong
[Icon.tsx](src/components/Icon.tsx)), và route handler kiểm tra lại giá trị gửi lên
— form gửi icon lạ sẽ bị từ chối chứ không lưu vào rồi hỏng trang.

> **Bẫy đã gặp khi làm phần này:** ban đầu thêm `CREATE TABLE landing_content` vào
> thẳng `0001_init.sql`. Chạy `d1:migrate:local` thì được báo "No migrations to
> apply" — vì wrangler ghi nhớ migration **theo tên tệp đã chạy**, sửa nội dung một
> tệp cũ không làm nó chạy lại, nên bảng mới sẽ không bao giờ được tạo ở nơi đã
> migrate trước đó (kể cả database thật sau này). Đã tách ra thành
> `0002_landing_content.sql`. Quy tắc: **sau lần đầu, mọi thay đổi lược đồ phải là
> một tệp migration mới.**

**Vì sao thân bài là JSON khối có kiểu, không phải ô nhập HTML tự do:** khi nội
dung do người khác nhập qua `/admin`, dựng lại từ khối có kiểu (`paragraph | heading
| list | quote | note | image`) thì React tự thoát ký tự — không chỗ nào cần
`dangerouslySetInnerHTML` cho nội dung này, nên một bài bị chèn mã độc cũng không
thực thi được.

Giới hạn đã biết, chấp nhận được cho một công cụ nội bộ một người dùng: không giới
hạn số lần thử đăng nhập, không CSRF token riêng (giảm nhẹ một phần nhờ cookie
`SameSite=Lax`).

### Bẫy đã gặp: nút "Đọc bài viết đầy đủ" trỏ cứng vào một slug

Bản đầu `page.tsx` viết chết `href="/tin-tuc/phat-hanh-phien-ban-4-0-0"` cho nút
này ở mục "Phiên bản mới nhất". Phát hành bản 4.0.1 (từ một phiên làm việc khác,
không phải qua `/admin`) lộ ra ngay vấn đề: trang chủ đổi sang hiện bản 4.0.1
nhưng nút vẫn trỏ về bài của bản 4.0.0 — không ai sửa vì không có gì báo lỗi cả,
trang vẫn build và chạy bình thường, chỉ SAI Ý NGHĨA.

> **Bẫy đã gặp lúc triển khai thật:** `0002_seed.sql` (tự sinh) chèn cột
> `article_slug` trước khi migration thêm cột đó (khi ấy đánh số `0004`) kịp
> chạy — vì file seed đứng SỐ NHỎ HƠN file thêm cột. Trên database thật vừa
> tạo, `d1 migrations apply` báo lỗi ngay `table release has no column named
> article_slug`. Đã đánh số lại: mọi migration đổi LƯỢC ĐỒ (`0001`, `0002`
> landing_content, `0003` article_slug) phải đứng trước file SEED DỮ LIỆU
> (`0004_seed.sql`) — không phải chỉ "file mới thì số lớn hơn", còn phải đúng
> thứ tự phụ thuộc giữa lược đồ và dữ liệu.
>
> Bẫy thứ hai cùng lúc: script sinh seed xoá-rồi-chèn theo TỪNG slug đang có
> trong `static-provider.ts`, không xoá sạch bảng trước — một bài viết bị bỏ
> khỏi mã nguồn (`phat-hanh-phien-ban-4-0-1`, gộp lại thành một bài duy nhất
> sau khi 4.0.2 phát hành) vẫn nằm mồ côi trên D1 thật cho tới khi xoá tay.
> Đã sửa `generate-d1-seed.mjs` để `DELETE FROM articles;` (xoá sạch) trước
> khi chèn lại toàn bộ — bảng nhỏ, xoá sạch rồi chèn lại không tốn kém gì.

Đã thêm cột `article_slug` vào bảng `release` (migration `0003`, xem
`Release.articleSlug` trong `types.ts`) — sửa được qua `/admin/release`, để
trống thì nút tự ẩn thay vì trỏ vào một slug không còn đúng. Bài học: một link
"trông đúng" lúc viết có thể âm thầm sai khi dữ liệu nó phụ thuộc đổi sau này;
những chỗ nối hai nguồn nội dung độc lập (ở đây: số phiên bản và bài viết mô tả
bản đó) nên là một trường dữ liệu tường minh, không phải suy luận ngầm từ quy
ước đặt tên.

---

## Triển khai lên Cloudflare

**Đã triển khai thật, ngày 2026-09-07:** Worker `quanlygiaoxu-landing`, D1
`qlgx-content` (`database_id` thật đã điền vào `wrangler.jsonc`), domain
`quanlygiaoxu.net` gắn thẳng vào Worker (không qua CNAME — dùng tính năng
Custom Domains của Workers, tự cấp chứng chỉ). Địa chỉ dự phòng:
`quanlygiaoxu-landing.khoannd.workers.dev`. Mật khẩu `/admin` đặt bằng
`wrangler secret put ADMIN_PASSWORD`, không lưu ở đâu trong kho mã.

Dùng **Workers + Static Assets** qua bộ chuyển đổi `@opennextjs/cloudflare`.
Cloudflare Pages nay đã là hướng cũ; dự án mới nên đi thẳng Workers, và Workers
cũng là thứ chạy được endpoint `/api/tai-ve` cùng ISR cho bài viết.

```bash
npx wrangler login
npm run cf:preview      # chạy thử ngay trên runtime Workers ở máy
npm run cf:deploy       # đưa lên
```

Cấu hình nằm ở [wrangler.jsonc](wrangler.jsonc) và [open-next.config.ts](open-next.config.ts).
Bắt buộc có `nodejs_compat` và `compatibility_date` từ 2024-09-23 trở đi.

**Biến môi trường:** thứ công khai đặt trong `vars` của `wrangler.jsonc`; thứ bí mật
(ví dụ `GITHUB_TOKEN`) đặt bằng `npx wrangler secret put GITHUB_TOKEN` để không lọt
vào kho mã.

### Một điều phải biết: tối ưu ảnh

Bộ tối ưu ảnh có sẵn của Next.js dựa vào `sharp` — mã máy, **không chạy được trên
Workers**. Nếu để nguyên, `/_next/image` vẫn trả về 200 nhưng là ảnh gốc: không thu
nhỏ, không đổi sang WebP/AVIF. Tốn băng thông mà không ai biết. Đây là điều đã kiểm
chứng thực tế, không phải phỏng đoán.

Vì vậy `npm run cf:*` đặt `DEPLOY_TARGET=cloudflare`, và
[next.config.ts](next.config.ts) chuyển sang bộ nạp ảnh riêng. Có hai chế độ:

| Chế độ | Cách bật | Kết quả |
| --- | --- | --- |
| Mặc định | không cần làm gì | Ảnh phục vụ thẳng từ `/images/...`, không có vòng gọi `/_next/image` vô ích. Ảnh trong `public/` đã được thu nhỏ và nén sẵn nên vẫn dùng tốt. |
| Cloudflare Images | bật Transformations cho tên miền, rồi build với `NEXT_PUBLIC_CF_IMAGES=1` | Thu nhỏ theo từng kích thước màn hình và đổi định dạng tự động qua `/cdn-cgi/image/`. |

Header cache và bảo mật cho tệp tĩnh nằm trong [public/_headers](public/_headers) —
cần riêng vì phần `headers()` của `next.config.ts` chỉ áp dụng cho phản hồi do
Next.js sinh ra, còn ảnh và font do hạ tầng tệp tĩnh của Cloudflare phục vụ.

### Vì sao không dùng vinext

`npx vinext check` báo dự án tương thích 81%, nhưng hai điểm trừ rơi đúng vào chỗ
đã cố ý tối ưu: **font phải tải từ CDN Google** thay vì tự host, và **mất tối ưu
ảnh**. OpenNext giữ được cả hai nên chọn OpenNext.

---

## Trang hướng dẫn — nhúng từ HTML tĩnh cũ

Phần mềm có sẵn 21 trang hướng dẫn dạng HTML tĩnh đi kèm bộ cài
([BIN/help/](../BIN/help/)), viết từ thời Windows XP. Thay vì để riêng, chúng đã
được nhúng vào `/huong-dan` với kiểu dáng của trang, qua
[scripts/import-huong-dan.mjs](scripts/import-huong-dan.mjs):

1. Đọc từng tệp `.htm`, bỏ `<style>`/`<script>` và các dòng điều hướng cũ
   ("[Trở về mục lục] [Trang chủ]") — trang mới có điều hướng riêng.
2. Viết lại `href` nội bộ thành `/huong-dan/<slug>`; copy 50 ảnh minh hoạ vào
   `public/images/help/` kèm **kích thước thật** đọc thẳng từ phần đầu tệp PNG/JPEG
   (viết tay, không dùng thư viện `image-size` — thư viện đó có lỗ hổng từ chối
   dịch vụ trong bộ phân tích ICNS/JXL/HEIF chưa có bản vá; chỉ cần đọc 2 định
   dạng nên diện tấn công nhỏ hơn nhiều khi tự viết).
3. Khử trùng bằng `sanitize-html` (allowlist thẻ, bỏ `onXXX`/`iframe`/`script`),
   ghi ra `src/lib/content/huong-dan-data.json`.

Trang `/huong-dan/[slug]` dùng `dangerouslySetInnerHTML` cho HTML đã sinh —
**có chủ đích, đã cân nhắc**: nội dung đến từ tệp trong kho mã (không phải người
dùng nhập lúc chạy), đã khử trùng ở bước build. Đây là ranh giới rõ ràng với nội
dung `/admin` (D1) — nơi ĐÓ dùng khối JSON có kiểu thay vì HTML thô đúng vì có
người khác nhập nội dung lúc chạy.

**Đã lưu ý về tính chính xác, không im lặng nhúng nguyên văn:** một số trang gốc
ghi "Windows XP trở lên" / ".NET Framework 2.0" làm yêu cầu hệ thống — đã lỗi thời
so với bản 4.0.0 (chạy trên .NET 4.8, Windows 7–11). Không tự ý sửa lại lời văn gốc
của tác giả; thay vào đó mỗi trang `/huong-dan/[slug]` có một dòng cảnh báo ngắn
trỏ tới thông tin phiên bản mới nhất.

Cập nhật nội dung: sửa trực tiếp các tệp `.htm` trong `BIN/help/` rồi chạy lại
`npm run huong-dan:import`.

---

## Đã kiểm chứng bằng trình duyệt

Các số dưới đây đo trực tiếp trên bản production, không phải suy luận:

- **Tương phản:** 0 lỗi. Đo bằng cách quy mọi ký hiệu màu (kể cả `oklab()` và
  `color-mix()` mà Tailwind v4 sinh ra) về sRGB thật, rồi chồng từng lớp nền kính
  theo đúng thứ tự — vì nền kính bán trong nên không thể đọc một lớp là xong.
- **Vùng chạm** ≥ 24×24 CSS px (WCAG 2.2 AA); các nút chính ≥ 54px.
- **Không cuộn ngang** ở 375px. Ảnh chụp desktop trượt ngang trong khung riêng.
- **Bàn phím:** thư viện ảnh theo đúng mẫu Tabs của WAI-ARIA; menu đóng bằng Esc.
- **Tuỳ chọn người dùng:** tôn trọng cả `prefers-reduced-motion` lẫn
  `prefers-reduced-transparency` (bỏ nhoè, dùng nền đặc).
- **Cloudflare:** mọi route (kể cả 21 trang `/huong-dan`, `/admin`, `/api/tai-ve`)
  trả đúng mã trạng thái khi chạy thật trên runtime Workers qua `wrangler dev` —
  không chỉ đọc code, đã bật server và gọi từng route.
- **Điểm tải về:** theo dõi đến tận file thật trên GitHub, đúng kích thước byte
  (4.481.024 byte, khớp `Release/qlgx_4_0_0.exe`).
- **D1 đọc/ghi thật:** đăng nhập admin (sai mật khẩu bị chặn, đúng mật khẩu vào
  được), sửa bản phát hành qua form thật, đọc lại xác nhận đúng dữ liệu — kể cả
  tiếng Việt có dấu — rồi khôi phục lại dữ liệu mẫu ban đầu.
- **21/21 trang hướng dẫn** trả 200, **50/50 ảnh minh hoạ** tham chiếu đến đều tải
  được, không ảnh vỡ, không cuộn ngang trên bất kỳ trang nào.
- **Không JavaScript:** nội dung vẫn hiển thị đầy đủ.

### Những cái bẫy đã gặp khi làm

1. **Tailwind v4 — thứ tự biến thể.** `data-[open=false]:hidden lg:block` **không**
   hoạt động như mong đợi: `data-[...]` xếp sau `lg:` trong CSS sinh ra, nên
   `hidden` thắng và menu biến mất ở desktop. Viết đúng là "ẩn trước rồi bật lên":
   `hidden data-[open=true]:block lg:block`.
2. **Kính quá trong thì chữ xuyên qua.** Thanh điều hướng ở mức trắng 0,82 vẫn để
   lộ chữ của phần nội dung phía sau, đọc thành hai lớp chồng nhau. `.glass-solid`
   phải từ 0,94 trở lên.
3. **Đừng tin một cái tên repo đoán được là đúng.** Bản build đầu tiên trỏ nút tải
   vào `github.com/khoannd/qlgx/releases/latest` mà chưa xác minh Releases có tồn
   tại — repo có thật nhưng chưa từng publish Release nào, nút tải 404. Đã sửa
   bằng cách đọc thẳng `Release/VersionConfig.xml`, nguồn mà chính phần mềm desktop
   đang dùng.
4. **`getCloudflareContext` gọi từ Node thường từng làm sập cứng cả tiến trình —
   không phải lỗi JS bắt được bằng try/catch.** Ban đầu tưởng đây chỉ là
   "D1 cục bộ lỗi thoáng qua khi build song song" (thấy `D1_ERROR: Failed to parse
   body as JSON`), nhưng lỗi thật nặng hơn: `next start` sau đó **tự thoát tiến
   trình** với `*** Fatal uncaught kj::Exception ... SQLITE_BUSY`. Truy đến gốc:
   `@opennextjs/cloudflare`'s `getCloudflareContext({ async: true })` chỉ đọc
   nhanh, đồng bộ khi ngữ cảnh đã được chính `worker.js` gán sẵn (Worker thật,
   hoặc `wrangler dev`) — nếu KHÔNG (mọi trường hợp Node thường: `next dev`,
   `next build`, `next start`, vì các lệnh đó không hề chạy `worker.js`), thư viện
   tự `import("wrangler")` rồi gọi `getPlatformProxy()`, **khởi động một tiến
   trình Miniflare/workerd con** để mô phỏng D1 bằng SQLite cục bộ. Trên Windows,
   nhiều tiến trình Node (dev, build, start, các lệnh `wrangler d1 execute`) cùng
   lúc mở chung tệp đó gây crash ở tầng native (C++), vượt ngoài khả năng bắt lỗi
   của JavaScript.
   Sửa tận gốc bằng [src/lib/cloudflare-runtime.ts](src/lib/cloudflare-runtime.ts):
   tự kiểm tra symbol toàn cục mà `worker.js` dùng để gán ngữ cảnh (
   `Symbol.for("__cloudflare-context__")`) TRƯỚC khi gọi `getCloudflareContext` —
   nếu chưa có, coi như không có D1 và dừng ngay, không bao giờ chạm nhánh Miniflare
   nguy hiểm. `content/index.ts` và `admin/auth.ts` đều đi qua kiểm tra này trước.
   Đã xác minh cả hai chiều: chạy `next start` liên tục, gọi 30 request dồn dập,
   không còn crash; chạy `wrangler dev` thật, D1 và đăng nhập `/admin` vẫn hoạt
   động bình thường (context đã có sẵn trên global scope nên không đi vào nhánh
   Miniflare).
5. **Test bằng `curl` trên Windows có thể tự làm hỏng dữ liệu tiếng Việt.** Truyền
   thẳng chuỗi có dấu qua `--data-urlencode` trong Git Bash bị mã hoá sai trước khi
   tới `curl`, trông giống lỗi lưu trữ nhưng thực ra là lỗi ở cách gọi lệnh kiểm
   thử. Viết đúng bằng cách dựng sẵn phần thân `application/x-www-form-urlencoded`
   trong tệp UTF-8 rồi gửi bằng `--data-binary @file`.

---

## Ảnh chụp màn hình

Xử lý bằng [scripts/chuan-bi-anh.ps1](scripts/chuan-bi-anh.ps1) từ `BIN/help/image/` của kho mã
chính, gồm ba việc:

1. **Cắt bỏ thanh tiêu đề cửa sổ Windows** — thanh tiêu đề XP/Aero làm cả trang
   trông cũ hàng chục năm.
2. **Thay tên giáo xứ trong dữ liệu mẫu** bằng "Giáo xứ ABC".
3. Thu về tối đa 1200px, nén JPEG q82.

Thêm ảnh mới: bỏ tệp vào `public/images/` rồi khai báo trong mảng `screenshots` của
[src/lib/site.ts](src/lib/site.ts) kèm `width`/`height` **đúng bằng kích thước sau
khi cắt** — sai một pixel là trang bị nhảy layout.

---

## Nội dung bài viết: lấy thật từ diễn đàn, không phải chỗ nào cũng bịa

Sáu bài viết mẫu trong `static-provider.ts` (nguồn seed cho D1) đều bắt nguồn từ
nội dung thật trên forum.quanlygiaoxu.net, đọc trực tiếp lúc soạn:

- **"Lời ngỏ từ tác giả"** và **"Được nhiều giáo phận chọn dùng thống nhất"** —
  biên tập lại từ chính bài "Lời ngỏ" và "Tác giả" do tác giả phần mềm (Mátthêu
  Nguyễn Đức Khoan) đăng trong mục Giới thiệu phần mềm. Các mốc giáo phận chọn
  dùng chung (Phan Thiết 2010, Vinh 2011, Phú Cường 2011, Qui Nhơn 2012) kèm tên
  Đấng Bản quyền đều lấy nguyên từ đó.
- **"Phát hành phiên bản 4.0.0"** — dựa trên `Release/VersionConfig.xml` thật.
- Bản nháp đầu tiên có một bài với **trích dẫn bịa** — một câu nói gán cho "một
  giáo xứ tại Giáo phận Xuân Lộc" nhưng không có nguồn thật. Đã gỡ bỏ và thay bằng
  hai bài trên. Hai bài còn lại ("Sao lưu dữ liệu đúng cách", "Chuẩn bị số liệu
  thống kê cuối năm") là hướng dẫn thao tác chung, không gán cho ai, không thuộc
  diện rủi ro tương tự.

Muốn thêm bài viết thật khác: sửa `static-provider.ts` rồi chạy
`npm run d1:seed:generate` để sinh lại `migrations/0004_seed.sql`, hoặc thêm thẳng
qua `/admin/articles/new` nếu đã có D1.

### Một lỗi bịa ngày đã tìm ra khi rà lại nội dung

Đợt rà soát toàn bộ nội dung trang phát hiện `publishedAt` của hai bài "Lời ngỏ
từ tác giả" và "Được nhiều giáo phận chọn dùng thống nhất" ghi `2020-06-18` —
con số này **không có căn cứ**, nhiều khả năng nhớ lẫn với ngày sửa đổi của các
tệp trong `BIN/help/` (cũng là 18/06/2020). Refetch lại đúng bài trên phpBB thì
ngày đăng thật là **17/06/2011** ("Thứ 6, 17 Tháng 6, 2011 8:01 pm"). Đã sửa lại
cả hai bài và ghi chú thêm: bài gốc được tác giả cập nhật dần qua nhiều năm (nội
dung nhắc tới sự kiện tháng 9/2012, sau ngày đăng gốc), nên ngày này chỉ là mốc
khởi tạo, không phải ngày viết ra câu chữ cuối cùng.

Bài học: một ngày tháng nghe hợp lý không có nghĩa là đúng — với nội dung nhận
là "lấy thật từ nguồn", mọi trường dữ liệu (kể cả những trường tưởng như vô hại
như ngày đăng) đều cần verify lại bằng nguồn gốc, không suy luận từ trí nhớ.

### Ngôn ngữ trung tính, không giả định vai trò người dùng

Người trực tiếp gõ sổ sách có thể là quý Cha, cũng có thể là ban hành giáo hay
thư ký văn phòng — mỗi giáo xứ tự sắp xếp khác nhau. Rà lại toàn bộ nội dung
web (không đụng tới bản dịch từ `BIN/help/` — xem mục trên) và sửa các chỗ lỡ
mặc định người đọc là "cha xứ":

- `site.description`, một mục tính năng, một câu hỏi thường gặp và một câu
  callout ở trang chủ — đổi chủ ngữ từ "cha xứ" sang "giáo xứ" (chỉ tổ chức,
  không chỉ đích danh vai trò), hoặc dùng đúng tên vai trò thật trong phần
  mềm ("Người quản trị" / "Người nhập liệu" — xem
  `BIN/help/Quan_ly_tai_khoan.htm`).
- Giữ nguyên câu "quý Cha" trong bài "Lời ngỏ từ tác giả": đây là tường thuật
  lịch sử có thật (các Đấng Bản quyền và quý Cha từng trực tiếp giúp phổ biến
  phần mềm), không phải giả định về người đọc — sửa sẽ làm sai lệch lịch sử.
- Đổi xưng hô "bạn" (thân mật, ngang hàng) sang lối viết không cần đại từ ngôi
  hai ở 4 chỗ trong bài viết và trang hướng dẫn, cho giọng văn nhất quán, lịch
  sự với mọi lứa tuổi — không biết trước người đọc là ai nên tránh xưng hô thân
  mật kiểu blog cá nhân.
- Bỏ hai chỗ dùng "nhiều nhất" (lỗi được báo nhiều nhất, câu hỏi được hỏi nhiều
  nhất) vì không có số liệu đếm thật để khẳng định thứ hạng — vẫn giữ ý "nhiều
  giáo xứ từng gặp/hỏi", chỉ bỏ phần so sánh hơn nhất chưa kiểm chứng được.

Sau khi sửa `static-provider.ts`, nhớ chạy lại `npm run d1:seed:generate` để
`migrations/0004_seed.sql` không bị lệch với mã nguồn.
