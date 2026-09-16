# Màn hình: Quản lý mẫu in (`MauInListPage.tsx`)

| | |
|---|---|
| Tệp nguồn | `WebApp/src/web/src/screens/MauInListPage.tsx` (mới hoàn toàn) |
| Backend | `WebApp/src/Qlgx.Api/Endpoints/MauInEndpoints.cs`, `Services/MauInService.cs`, `Printing/MauInCatalog.cs`, `Printing/MauInHtmlSanitizer.cs` |
| Bảng dữ liệu mới | `mau_in_tuy_chinh` (migration `ThemMauInTuyChinh`) |
| Bảng dữ liệu đụng tới (đọc) | không bảng nào — chỉ đọc `EmbeddedResource` gốc và bảng mới `mau_in_tuy_chinh` |
| Trạng thái migrate | không áp dụng — đây là **năng lực mới**, xem mục 8 |

## 1. Mục đích

Cho phép **Quản trị viên giáo xứ** (loaiTaiKhoan=0) tự sửa nội dung 13 mẫu in nghiệp vụ
(lý lịch cá nhân, chứng nhận bí tích, phiếu gia đình, giấy giới thiệu, rao hôn phối, danh
sách…) áp dụng RIÊNG cho giáo xứ mình, và cho phép **Quản trị hệ thống** (loaiTaiKhoan=9) sửa
một bộ mẫu TUỲ CHỈNH CHUNG áp dụng cho mọi giáo xứ chưa tự tuỳ chỉnh riêng — thay cho việc phải
nhờ đội phát triển sửa mã nguồn mỗi khi một giáo xứ muốn đổi cách trình bày giấy tờ (thêm dòng,
đổi câu chữ, chỉnh logo/kiểu chữ).

Mở từ mục "Thông tin giáo xứ" trên thanh điều hướng, hiện cho **mọi tài khoản đã đăng nhập**
(kể cả tài khoản nhập liệu thường chỉ xem được, không sửa).

## 2. Bố cục và các trường

**Danh sách 13 mẫu** (bảng, không phải AG Grid — chỉ 12 dòng, không cần lọc/sắp xếp phức tạp):

| Cột | Nguồn | Ghi chú |
|---|---|---|
| Mẫu in | `MauInDanhSachItemDto.tenHienThi` | Tên tiếng Việt dễ hiểu, không phải `TenMau` kỹ thuật |
| Đang dùng | `capDangDung` | Huy hiệu màu: xám "Mặc định gốc" / vàng "Đã tuỳ chỉnh (hệ thống)" / xanh thương hiệu "Đã tuỳ chỉnh (riêng giáo xứ)" |
| (nút) | — | "Sửa" — chỉ hiện khi tài khoản có quyền sửa (QuanTri hoặc QuanTriHeThong) |

**Trình soạn mẫu** (mở dưới danh sách khi bấm "Sửa"):
- Dropdown "Chèn chỗ trống" — liệt kê toàn bộ **biến khả dụng** CHO ĐÚNG mẫu đang sửa (`BienKhaDung`,
  SIÊU TẬP của các chỗ trống mẫu gốc đang dùng — xem mục 10), kèm nhãn tiếng Việt, gom thành
  `<optgroup>` theo nhóm ("Giáo xứ", "Giáo dân", "Bí tích", "Giáo lý", "Gia đình"…) vì danh sách dài
  tới 87 mục ở mẫu "Lý lịch cá nhân"; chọn một mục chèn `{{Key}}` vào đúng vị trí con trỏ trong vùng
  soạn thảo.
- Vùng soạn thảo rich-text (`react-simple-wysiwyg`) — xem mục 8.
- Nút "Xem thử" — vẽ PDF ngay từ nội dung NHÁP, mở tab mới.
- Nút "Lưu".
- Nút "Khôi phục về mặc định" — chỉ hiện khi đã có tuỳ chỉnh (`daTuyChinh=true`).

**Khu vực "Cách hiển thị dữ liệu đúng/sai"** (bảng riêng, ngay dưới danh sách mẫu): cho giáo xứ đặt
câu chữ in ra cho các mục chỉ có hai trạng thái (tân tòng, còn đi học…) thay cho dấu `[x]`/`[  ]`
mặc định — xem **mục 12**.

## 3. Hành vi khi tải

- Danh sách 13 mẫu tải ngay khi mở màn hình (`GET /api/mau-in`), không phân trang.
- Mở "Sửa" một mẫu tải `GET /api/mau-in/{tenMau}/rieng` (Quản trị viên giáo xứ) hoặc
  `/he-thong` (Quản trị hệ thống) — nếu CHƯA có dòng tuỳ chỉnh nào, trả về **nội dung mẫu gốc
  nhúng cứng** (không phải ô trống) kèm `rowVersion=0` (giá trị canh dấu "chưa tồn tại, Lưu lần
  đầu sẽ TẠO MỚI").

## 4. Quy tắc nghiệp vụ và kiểm tra dữ liệu

- **Kích thước tối đa một mẫu**: 300KB (`MauInHtmlSanitizer.KichThuocToiDa`) — kiểm ở tầng
  endpoint TRƯỚC khi khử trùng. Vượt quá → 400 `"Nội dung mẫu quá lớn (tối đa 300KB), hãy rút
  gọn bớt."`
- **Khử trùng HTML server-side BẮT BUỘC** trước khi lưu (thư viện HtmlSanitizer/Ganss.Xss, MIT)
  — loại `<script>`, thuộc tính `on*`, và giới hạn `href`/`src` CHỈ còn lược đồ `data:` (không
  http/https/javascript:) — xem mục 8. Không tin trình soạn thảo phía trình duyệt.
- **Chống ghi đè đồng thời bằng RowVersion** (khoá lạc quan `xmin`) — sai `rowVersion` khi Lưu
  → 409 `"Mẫu này vừa được người khác cập nhật. Hãy tải lại màn hình để xem thay đổi mới nhất
  rồi sửa lại."`
- **Tên mẫu không hợp lệ** (không khớp 12 `TenMau` đã biết) → 404 ở mọi endpoint theo tên mẫu.
- **`GiaoXuId` không bao giờ nhận từ tham số/thân yêu cầu** cho nhóm `/rieng/*` — luôn lấy từ
  claim đăng nhập (`IBoiCanhGiaoXu`).

## 5. Thao tác người dùng

| Thao tác | Điều kiện | Kết quả |
|---|---|---|
| Bấm "Sửa" một dòng | Chỉ hiện nếu `loaiTaiKhoan` là 0 hoặc 9 | Mở trình soạn mẫu đúng cấp (riêng/hệ thống) tương ứng |
| Chọn một mục trong dropdown "Chèn chỗ trống" | Đang mở trình soạn | Chèn `{{Key}}` vào vị trí con trỏ, dropdown tự về rỗng |
| Bấm "Xem thử" | — | Gọi `POST /api/mau-in/{tenMau}/xem-thu` với nội dung NHÁP hiện tại, mở PDF ở tab mới — dùng DỮ LIỆU MẪU (chuỗi `[Nhãn]` cho mỗi chỗ trống, một dòng minh hoạ cho khối lặp), KHÔNG cần chọn một bản ghi thật, KHÔNG lưu gì |
| Bấm "Lưu" | — | `PUT` đúng endpoint rieng/he-thong; báo "Đã lưu." hoặc lỗi 409/400 nguyên văn |
| Bấm "Khôi phục về mặc định" | Chỉ hiện khi `daTuyChinh=true` | Xác nhận (`confirm`) rồi `DELETE` — xoá hẳn dòng tuỳ chỉnh, quay về đường phân giải cũ (hệ thống nếu có, không thì mẫu gốc) |
| Bấm "Đóng" | — | Đóng trình soạn, không lưu gì thêm |

## 6. Lưới dữ liệu

Không có AG Grid — bảng HTML tĩnh 12 dòng (xem mục 2), không cần `.fixed-h-grid`.

## 7. Liên kết sang màn hình khác

Không mở màn hình nào khác. Ảnh hưởng NGƯỢC tới mọi màn hình có nút "In..." (Lý lịch cá nhân,
Chứng nhận bí tích, Phiếu gia đình...) — thứ tự phân giải mẫu (`InAnService.DungMau`) tự động
ưu tiên mẫu tuỳ chỉnh nếu có, không cần các màn hình đó biết gì về "Quản lý mẫu in".

## 8. Vì sao đây là năng lực mới, không phải migrate

Bản desktop **không có màn hình này**. Cách bản desktop "sửa mẫu in" là mở trực tiếp tệp
`.doc`/`.xls` trong `BIN/Template/` bằng Microsoft Word/Excel trên chính máy tính đang chạy
phần mềm — `Source/DBAccess/WordEngine.cs` chỉ làm việc "tìm-thay-thế placeholder" trên một tệp
Office đã có sẵn trên đĩa, không có khái niệm "nhiều giáo xứ, mỗi giáo xứ một mẫu riêng, lưu ở
máy chủ".

Mô hình máy chủ tập trung phục vụ nhiều giáo xứ (dùng chung một database, xem
`WebApp/TIEN-DO.md`) không cho phép cách đó: không có "máy của giáo xứ" nào để mở Word lên sửa
nữa, và một giáo xứ sửa file trên đĩa dùng chung sẽ ảnh hưởng tới MỌI giáo xứ khác. Vì vậy đây
là **năng lực hoàn toàn mới cần thiết cho đúng mô hình web**, không phải "migrate hành vi cũ
sang giao diện mới" — không có hành vi cũ nào để đối chiếu.

Thiết kế mới quan trọng nhất, tự quyết và ghi lại lý do:

- **Lưu trữ**: bảng `mau_in_tuy_chinh` (`Id`, `GiaoXuId` cho phép NULL, `TenMau`, `NoiDungHtml`,
  `CreatedAt`/`UpdatedAt`, `RowVersion`=`xmin`). `GiaoXuId IS NULL` nghĩa là mẫu tuỳ chỉnh CẤP
  HỆ THỐNG. Ràng buộc duy nhất bằng **hai chỉ mục một phần** (`ix_mau_in_tuy_chinh_giao_xu` trên
  `(giao_xu_id, ten_mau)` lọc `giao_xu_id IS NOT NULL`, và `ix_mau_in_tuy_chinh_he_thong` trên
  `ten_mau` lọc `giao_xu_id IS NULL`) — PostgreSQL coi hai NULL là khác nhau trong chỉ mục
  thường, một `UNIQUE(GiaoXuId, TenMau)` bình thường KHÔNG chặn được hai dòng hệ thống trùng
  `TenMau` (cả hai đều NULL).
- **Không có bộ lọc toàn cục theo GiaoXuId**: bảng này CỐ Ý không nằm trong danh sách
  `HasQueryFilter` của `QlgxDbContext` (giống `GiaoPhan`/`GiaoHat`/`NhapDuLieuJob`) — trộn ý
  nghĩa "NULL = không giáo xứ nào lọc" của bộ lọc toàn cục với ý nghĩa nghiệp vụ "NULL = mẫu hệ
  thống" của bảng này sẽ làm rò rỉ dữ liệu giữa hai khái niệm khác nhau. `MauInService` tự lọc
  tường minh theo đúng ngữ cảnh gọi. `LocTheoGiaoXuTests` có một ngoại lệ CÓ CHỦ ĐÍCH ghi rõ lý
  do cho bảng này (bài test duyệt toàn bộ model, không phải danh sách cứng, nên vẫn tự bắt lỗi
  nếu ai thêm bảng MỚI khác quên gắn bộ lọc).
- **Thứ tự phân giải khi in** (`InAnService.DungMau`, giữ tương thích ngược tuyệt đối): (1) mẫu
  riêng của giáo xứ đăng nhập, (2) mẫu tuỳ chỉnh cấp hệ thống, (3) mẫu gốc nhúng cứng
  (`EmbeddedResource`, PHAO CỨU SINH VĨNH VIỄN — không bao giờ xoá khỏi mã nguồn, không phụ
  thuộc CSDL). Hai bước tra CSDL trả `null` (không có dòng) thì rơi thẳng về đường đi CŨ trước
  khi có bảng này — không ai chưa tuỳ chỉnh gì bị ảnh hưởng.
- **Phân quyền**: sửa mẫu riêng giới hạn policy `"QuanTri"` (giống "Quản lý tài khoản"); sửa mẫu
  hệ thống giới hạn `"QuanTriHeThong"`. Mọi endpoint `RequireAuthorization()`, không dùng
  `Find()`/`FindAsync()`.
- **"Xem thử" dùng DỮ LIỆU MẪU, không phải một bản ghi thật**: mỗi chỗ trống hiện chuỗi
  `[Nhãn tiếng Việt]`, mỗi khối lặp (`HangThanhVien`, `DanhSachBiTich`...) hiện một dòng minh
  hoạ cố định — người dùng thấy NGAY kết quả mà không phải rời màn hình đi tìm một giáo dân/gia
  đình cụ thể trước. Đây là lựa chọn ĐƠN GIẢN HOÁ có chủ đích so với đặc tả gốc (đặc tả có nhắc
  tuỳ chọn "hoặc chọn một bản ghi thật") — chưa làm nhánh "chọn bản ghi thật" vì mỗi mẫu cần một
  loại bản ghi khác nhau (giáo dân/gia đình/đôi rao hôn phối), việc đó cần thêm một bộ chọn
  bản ghi riêng cho từng loại mẫu; xem mục 9.
- **Trình soạn thảo rich-text**: `react-simple-wysiwyg` (MIT, `https://github.com/megahertz/
  react-simple-wysiwyg`, bản `3.4.1` lúc thêm) — theo đúng tiền lệ dự án kiểm giấy phép trước
  khi thêm thư viện mới (Chart.js 4 MIT đã kiểm cho biểu đồ). Chọn vì rất nhẹ (bọc quanh
  `contentEditable`/`execCommand`, không kéo theo ProseMirror hay framework soạn thảo nặng),
  API đơn giản (`<Editor value onChange>` + `<Toolbar>` các nút `Btn*`), đủ dùng cho nhu cầu
  "chữ đậm/nghiêng/gạch chân/danh sách" của một tờ giấy in — người dùng mục tiêu là quý cha,
  quý sơ, không cần và không nên thấy mã HTML thô.
- **Lỗ hổng bảo mật đã vá TRƯỚC KHI mẫu trở nên sửa được** — xem `BoTrinhDuyet.cs`: trước lượt
  này, `NewPageAsync()` không tắt JavaScript, không chặn mạng — vô hại khi mẫu chỉ do đội phát
  triển kiểm soát, nhưng NGUY HIỂM ngay khi ~40 giáo xứ + 1 quản trị hệ thống tự nhập được nội
  dung HTML: một quản trị viên (vô tình hay cố ý) có thể chèn `<img src="http://...">` (SSRF từ
  máy chủ) hoặc `<script>` (thực thi trong lúc vẽ PDF). Hai lớp phòng thủ BẮT BUỘC, áp dụng cho
  **MỌI** lần vẽ PDF (không chỉ mẫu tuỳ chỉnh, phòng thủ theo chiều sâu):
  1. `NewPageAsync(new BrowserNewPageOptions { JavaScriptEnabled = false })`.
  2. `page.RouteAsync("**/*", route => ...)` — chặn (`AbortAsync`) mọi yêu cầu trừ `data:`/
     `about:`.
  Chứng minh bằng thực nghiệm (không chỉ đọc mã rồi tin) ở
  `BoTrinhDuyetBaoMatTests.cs`: một `HttpListener` thật lắng nghe cổng loopback do chính bài
  test cấp phát, dựng PDF từ HTML chứa `<img>`/`<link>` trỏ tới cổng đó, xác nhận **0 lượt gọi**
  lọt tới listener; và một script bơm 100.000 ký tự giả-ngẫu-nhiên (khó nén, để không bị
  FlateDecode của PDF che mất chênh lệch kích thước) vào trang — PDF gần như KHÔNG đổi kích
  thước khi JS tắt, còn một bài test ĐỐI CHỨNG (dựng riêng một trang Playwright bật JS, không
  qua `BoTrinhDuyet`) xác nhận CÙNG kịch bản đó THẬT SỰ làm PDF phình to hàng nghìn byte khi JS
  bật — chứng minh bài test phía trên không phải một ngưỡng số vô nghĩa luôn xanh.

## 9. Chỗ chưa chắc / chưa làm

- "Xem thử" chưa có nhánh "chọn một bản ghi thật" (giáo dân/gia đình/đôi rao hôn phối cụ thể) —
  chỉ có dữ liệu mẫu minh hoạ, xem mục 8.
- Chưa có lịch sử phiên bản mẫu (mỗi lần Lưu ghi đè, không giữ bản cũ) — "Khôi phục về mặc định"
  chỉ đưa về đúng mẫu GỐC nhúng cứng, không phải "hoàn tác lần sửa gần nhất".
- Trình soạn thảo dùng `document.execCommand` (API đã bị đánh dấu "deprecated" trong đặc tả
  HTML, nhưng vẫn được mọi engine trình duyệt hiện tại hỗ trợ đầy đủ, kể cả Chromium mà bản web
  này nhắm tới) — đúng kiến trúc mà bản thân thư viện `react-simple-wysiwyg` dùng nội bộ cho mọi
  nút định dạng, không phải lựa chọn riêng của tính năng này.

## 10. Hai khái niệm khác nhau: "chỗ trống mẫu gốc" và "biến khả dụng để chèn thêm"

Người dùng thật nêu vấn đề: mẫu gốc "Lý lịch cá nhân" chỉ dùng 46 chỗ trống, nhưng hồ sơ giáo dân
còn nhiều dữ liệu KHÁC mà mẫu gốc không in ra (CMND, ngày xức dầu, các mốc giáo lý, và đặc biệt là
số sổ / ngày / nơi rửa tội TÁCH RIÊNG thay vì một câu ghép sẵn `{{MoTaRuaToi}}`). Giáo xứ muốn tự
thêm những dòng đó vào giấy của mình.

Vì vậy `MauInCatalog.cs` phân biệt HAI tập hợp — cố ý KHÔNG gộp thành một:

| | `ChoTrong` | `BienKhaDung` |
|---|---|---|
| Nghĩa | các `{{Key}}` **có mặt thật** trong tệp mẫu gốc `.html` | **mọi** biến người dùng được phép chèn |
| Quan hệ | tập con | **siêu tập** của `ChoTrong` |
| Dùng ở đâu | tài liệu này, bài test đối chiếu với tệp `.html` | combobox "Chèn chỗ trống", "Xem thử" |
| Bài test canh giữ | `MauInCatalogTests.Danh_muc_khop_dung_tap_hop_cho_trong_that_trong_tep_html` (khớp CHÍNH XÁC) + `Tong_so_cho_trong_toan_bo_danh_muc_la_252` | `MauInDayDuBienTests` (xem 10.1) |

**Vì sao tách:** mẫu gốc không nên phình to vô ích — thêm 40 dòng dữ liệu mà phần lớn giáo xứ không
dùng chỉ làm tờ giấy rối và khó sửa. Nhưng nếu combobox chỉ liệt kê đúng những gì mẫu gốc đang dùng
thì người dùng **không có cách nào biết** còn dữ liệu gì để thêm.

**Vì sao vô hại với bản in hiện tại:** `BoDoMauIn.ApDung` chỉ thay những `{{Key}}` **CÓ MẶT trong
HTML**. Việc `InAnService` bỏ thêm nhiều khoá vào dictionary `duLieu` không đổi một ký tự nào của
13 mẫu gốc — chỉ khiến biến người dùng tự chèn có dữ liệu thật phía sau.

### 10.1 Ràng buộc bắt buộc và bài test chứng minh

**Mọi Key công bố trong `BienKhaDung` PHẢI được `InAnService` gán một giá trị thật.** Không được
công bố biến "cho đẹp" rồi để người dùng chèn vào và in ra tờ giấy có chữ `{{NgayXucDau}}` giữa
trang. `MauInDayDuBienTests` chứng minh điều đó tự động: với TỪNG mẫu, nó dựng một mẫu tuỳ chỉnh
riêng của giáo xứ chứa TẤT CẢ `{{Key}}` của mẫu đó (lưu qua chính `PUT /api/mau-in/{tenMau}/rieng`),
gọi ĐÚNG endpoint in thật, rồi kiểm hai điều:

1. **(chính)** mọi Key có mặt trong dictionary mà `InAnService` trao cho `BoDoMauIn`;
2. **(đầu-cuối)** bản in cuối cùng không còn chuỗi `{{` nào.

Phép kiểm (1) là phép kiểm thật sự có giá trị: `ApDung` **xoá trắng** cả những `{{Key}}` KHÔNG có
trong dictionary, nên nhìn HTML thì "quên gán hẳn" trông y hệt "giá trị rỗng hợp lệ" — chỉ soi tập
khoá mới phân biệt được. Một bài test đối chứng (`Hai_phep_kiem_deu_biet_bao_loi`) xác nhận cả hai
phép kiểm **biết báo lỗi**, không phải luôn xanh.

Kiểm ở tầng HTML/dictionary chứ không phải PDF vì PDF là nhị phân đã nén, không tìm chuỗi trong đó
một cách đáng tin được. Chỗ chặn: `BoTrinhDuyet.XuatPdfAsync` (HTML cuối cùng) và
`BoDoMauIn.ApDung` (tập khoá) — hai hàm mà MỌI mẫu của mọi màn hình đều đi qua. Cả hai lớp được bỏ
`sealed` và hai hàm đó thành `virtual` CHỈ để bài test thay được chúng trong DI; **không thêm thành
viên công khai nào mới**, và cố ý KHÔNG mở `public` các hàm dựng HTML của `InAnService` cũng như
KHÔNG tách một `IBoTrinhDuyet`/`IBoDoMauIn` mà sản phẩm không cần. Lợi ích kèm theo: bài test không
khởi động Chromium nên in đủ 13 mẫu chỉ mất dưới một giây.

### 10.2 Quy tắc đặt tên khoá cho biến thêm

Dùng **tên tự nhiên trùng tên thuộc tính thực thể** (`SoRuaToi`, `NgayRuaToi`, `NoiRuaToi`,
`NgayXucDau`, `SoHoKhau`…) — đã đối chiếu và KHÔNG khoá nào trong số đó đụng khoá sẵn có của bất kỳ
mẫu nào (mẫu gốc chỉ có câu ghép `MoTaRuaToi`; các khoá ngày sẵn có chỉ là `NgaySinh`,
`NgayHonPhoi`, `NgayQuaDoi`, `NgayThangNamIn`). Vì vậy **không cần hậu tố "Rieng"** nào.

Hai chỗ buộc phải đổi tên vì tên tự nhiên đã bị chiếm nghĩa khác:

| Thuộc tính | Khoá dùng | Vì sao không dùng tên tự nhiên |
|---|---|---|
| `GiaoDan.GhiChu` | `GhiChuGiaoDan` | `GhiChuGiaDinh`/`GhiChuHonPhoi` đã có nghĩa khác trong cùng mẫu |
| `RaoHonPhoi.GhiChu` | `GhiChuRao` | như trên |

Lưu ý riêng ở "Lý lịch cá nhân": `{{NgayQuaDoi}}`, `{{NoiAnTang}}`, `{{SoAnTang}}` sẵn có là các
**cụm câu đã ghép sẵn** ("— ngày …", "— an táng tại …", " (số …)"), KHÔNG phải giá trị thô — giữ
nguyên ngữ nghĩa đó; biến thô bổ sung chỉ có `{{NoiQuaDoi}}` (chưa từng tồn tại).

### 10.3 Số lượng theo từng mẫu

| Mẫu | Chỗ trống mẫu gốc đang dùng | Biến thêm (chèn được) | Tổng biến khả dụng |
|---|---|---|---|
| Lý lịch cá nhân | 46 | 41 | **87** |
| Chứng nhận bí tích | 17 | 27 | 44 |
| Chứng nhận hôn phối | 31 | 30 | 61 |
| Phiếu gia đình (A4/A3) | 13 | 19 | 32 |
| Giới thiệu chứng nhận rửa tội | 18 | 16 | 34 |
| Giới thiệu chứng nhận thêm sức | 17 | 23 | 40 |
| Giới thiệu giáo lý hôn phối | 21 | 30 | 51 |
| Giới thiệu chuyển xứ | 15 | 11 | 26 |
| Xin điều tra và rao hôn phối | 26 | 26 | 52 |
| Kết quả rao hôn phối | 33 | 21 | 54 |
| In danh sách giáo dân | 5 | 6 | 11 |
| In danh sách gia đình | 5 | 6 | 11 |
| In danh sách rao hôn phối | 5 | 6 | 11 |
| **Tổng** | **252** | **262** | **514** |

### 10.4 Bảng biến thêm theo nhóm

Nhóm (`BienMauIn.Nhom`) chỉ để gom mục thành `<optgroup>` trong combobox — 87 mục liệt kê phẳng thì
quý cha/quý sơ không tìm nổi. Thứ tự nhóm giữ đúng **lần xuất hiện đầu tiên** từ máy chủ (trình tự
đọc của tờ giấy: giáo xứ → giáo dân → bí tích → …), frontend KHÔNG sắp lại theo bảng chữ cái.

**Nhóm "Giáo xứ"** — thêm cho mẫu nào chưa in: `{{DiaChiGiaoXu}}`, `{{DienThoaiGiaoXu}}`,
`{{EmailGiaoXu}}`, `{{WebsiteGiaoXu}}` (Phiếu gia đình, hai mẫu rao hôn phối, 3 mẫu danh sách),
`{{TenGiaoPhan}}`/`{{TenGiaoHat}}` (3 mẫu danh sách, Kết quả rao), `{{TenGiaoHo}}` (3 giấy giới
thiệu cá nhân, Giới thiệu chuyển xứ).

**Nhóm "Giáo dân"** — có ở mọi mẫu theo một giáo dân:

| Biến | Nguồn dữ liệu |
|---|---|
| `{{MaGiaoDan}}` | `GiaoDan.MaGiaoDanCu` |
| `{{Phai}}` | `GiaoDan.Phai` |
| `{{CMND}}` | `GiaoDan.CMND` |
| `{{DanToc}}` | `GiaoDan.DanToc` |
| `{{NgheNghiep}}` | `GiaoDan.NgheNghiep` |
| `{{ThuocGiaoXu}}` | `GiaoDan.ThuocGiaoXu` |
| `{{ThuocGiaoPhan}}` | `GiaoDan.ThuocGiaoPhan` |
| `{{GhiChuGiaoDan}}` | `GiaoDan.GhiChu` |
| `{{DienThoaiGiaoDan}}` / `{{EmailGiaoDan}}` / `{{DiaChiGiaoDan}}` | `GiaoDan.DienThoai` / `Email` / `DiaChi` |
| `{{NoiQuaDoi}}` | `GiaoDan.NoiQuaDoi` (chỉ Lý lịch cá nhân) |

Ở "Chứng nhận hôn phối" các biến trên có dạng cặp hậu tố `Nam`/`Nu` (`{{MaGiaoDanNam}}`,
`{{CMNDNu}}`, `{{DiaChiNam}}`, `{{DienThoaiNu}}`…). Ở hai mẫu rao hôn phối là hậu tố `1`/`2`
(`{{Phai1}}`, `{{NgaySinh2}}`, `{{DienThoai1}}`, `{{DiaChi2}}`, `{{Tuoi1}}`…).

**Nhóm "Bí tích"** — bí tích dạng RỜI, đúng như bản desktop in từng ô
(`Source/ExcelReport/ReportLyLichCaNhan.cs`), thay vì chỉ có câu ghép `{{MoTaRuaToi}}`:

| Rửa tội | Rước lễ lần đầu | Thêm sức | Xức dầu bệnh nhân |
|---|---|---|---|
| `{{SoRuaToi}}` | `{{SoRuocLe}}` | `{{SoThemSuc}}` | — |
| `{{NgayRuaToi}}` | `{{NgayRuocLe}}` | `{{NgayThemSuc}}` | `{{NgayXucDau}}` |
| `{{NoiRuaToi}}` | `{{NoiRuocLe}}` | `{{NoiThemSuc}}` | — |
| `{{ChaRuaToi}}` | `{{ChaRuocLe}}` | `{{ChaThemSuc}}` | `{{NguoiXucDau}}` |
| `{{NguoiDoDauRuaToi}}` | — | `{{NguoiDoDauThemSuc}}` | `{{TinhTrangXucDau}}`, `{{GhiChuXucDau}}` |

Mọi ngày đi qua đúng helper `VanBanInAn.Ngay(...)` (định dạng `dd/MM/yyyy`, rỗng khi null) — KHÔNG
định dạng lại ở từng nơi. Ở "Chứng nhận hôn phối" các biến này cũng có dạng cặp `Nam`/`Nu`
(`{{SoRuaToiNam}}`, `{{NgayThemSucNu}}`…); ở hai mẫu rao là `{{MoTaRuaToi1}}`/`{{MoTaThemSuc2}}`.

**Nhóm "Giáo lý"** (Lý lịch cá nhân, Giới thiệu giáo lý hôn phối): `{{NgayBD1}}`, `{{NoiBD1}}`,
`{{NgayBD2}}`, `{{NoiBD2}}`, `{{NgayTHVaoDoi}}`, `{{NoiTHVaoDoi}}`, `{{NgayGLHN1}}`,
`{{NgayGLHN2}}`, `{{NoiGLHN}}`, `{{NguoiChungNhanGLHN}}`, `{{XepLoaiGLHN}}`.

**Nhóm "Gia đình"**:

| Biến | Nguồn dữ liệu | Có ở mẫu |
|---|---|---|
| `{{TenGiaDinh}}`, `{{MaGiaDinh}}`, `{{DiaChiGiaDinh}}`, `{{DienThoaiGiaDinh}}`, `{{GhiChuGiaDinh}}` | `GiaDinh` | Lý lịch cá nhân (gia đình đang tham gia), Giới thiệu chuyển xứ |
| `{{SoHoKhau}}`, `{{DienGiaDinh}}` | `GiaDinh.SoHoKhau` / `DienGiaDinh` | Lý lịch cá nhân, Phiếu gia đình, Giới thiệu chuyển xứ |
| `{{MaGiaDinhCu}}`, `{{SoLuongThanhVien}}`, `{{DaChuyenXu}}`, `{{NgayChuyen}}`, `{{NoiChuyen}}` | `GiaDinh` + số thành viên | Phiếu gia đình, Giới thiệu chuyển xứ |

`{{MaGiaDinh}}` ưu tiên `MaGiaDinhRieng` (mã giáo xứ tự nhập, bật bằng cấu hình
`TUNHAP_MAGIADINH`) rồi mới tới mã gốc; `{{MaGiaDinhCu}}` luôn là mã gốc.

**Nhóm "Hôn phối"** (Phiếu gia đình — mẫu gốc chỉ in câu ghép `{{MoTaHonPhoi}}`): `{{SoHonPhoi}}`,
`{{NgayHonPhoi}}`, `{{NoiHonPhoi}}`, `{{ChaHonPhoi}}`, `{{CachThucHonPhoi}}`, `{{NguoiChung1}}`,
`{{NguoiChung2}}`, `{{GhiChuHonPhoi}}`. "Chứng nhận hôn phối" thêm `{{TenHonPhoi}}` và
`{{GhiChuHonPhoi}}`.

**Nhóm "Rao hôn phối"**: `{{MaRaoHonPhoi}}`, `{{TenRaoHonPhoi}}`, `{{NgayRaoLan1}}`,
`{{NgayRaoLan2}}`, `{{NgayRaoLan3}}`, `{{GhiChuRao}}`, và các cột giáo xứ/giáo phận hiện tại /
nguyên quán / trước đây của hai người (`{{TenGiaoPhan1}}`, `{{TenGiaoXuNQ2}}`,
`{{TenGiaoPhanTruoc1}}`…) cho mẫu nào chưa in hết.

### 10.5 Hiệu năng: không thêm truy vấn N+1

`DungHtmlLyLichCaNhan` được gọi **lặp cho từng thành viên** khi in lý lịch cả gia đình
(`XuatLyLichCaNhanGiaDinh`), nên mọi biến thêm đều lấy từ thực thể **đã nạp**, không hàm helper nào
chạm CSDL. Ba thay đổi truy vấn duy nhất đều là **thêm phép nối vào truy vấn đã có**, không phải
truy vấn mới:

- `DungHtmlLyLichCaNhan`: `.Include(GiaDinhThamGia).ThenInclude(tv => tv.GiaDinh)` — để có nhóm
  "Gia đình".
- `XuatGioiThieuChuyenXu`: `.Include(x => x.GiaoHo)` — để có `{{TenGiaoHo}}`.
- `XuatChungNhanHonPhoi`: thêm 4 cột (`MaGiaoDanCu`, `CMND`, `DiaChi`, `DienThoai`) vào **cùng một**
  phép chiếu `NguoiHonPhoi` đã có.

## 11. Danh mục 252 chỗ trống mẫu gốc theo từng mẫu

Đọc trực tiếp từ đối chiếu `InAnService.cs` (nơi gán giá trị cho từng `Key`) với các tệp
`PrintTemplates/Chung/*.html` (nơi dùng `Key`) — có bài kiểm tra tự động
(`MauInCatalogTests.Danh_muc_khop_dung_tap_hop_cho_trong_that_trong_tep_html`) đối chiếu lại
danh mục này với đúng tập `{{Key}}` thật trong từng tệp `.html`, tự báo lỗi nếu ai gõ nhầm tên
hay bỏ sót khi sửa `MauInCatalog.cs` sau này.

### Lý lịch cá nhân (`LyLichCaNhan`) — 46 chỗ trống

| Chỗ trống | Ý nghĩa |
|---|---|
| `{{TenGiaoPhan}}` | Tên giáo phận |
| `{{TenGiaoHat}}` | Tên giáo hạt |
| `{{TenGiaoXu}}` | Tên giáo xứ |
| `{{DiaChiGiaoXu}}` | Địa chỉ giáo xứ |
| `{{DienThoaiGiaoXu}}` | Điện thoại giáo xứ |
| `{{EmailGiaoXu}}` | Email giáo xứ |
| `{{WebsiteGiaoXu}}` | Website giáo xứ |
| `{{TenGiaoHo}}` | Tên giáo họ |
| `{{MaGiaoDan}}` | Mã giáo dân (số cũ) |
| `{{HoTen}}` | Họ và tên (kèm tên thánh) |
| `{{NgaySinh}}` | Ngày sinh |
| `{{NoiSinh}}` | Nơi sinh |
| `{{Phai}}` | Giới tính |
| `{{VaiTro}}` | Vai trò trong gia đình (Chồng/Vợ/Con) |
| `{{TenCha}}` | Họ tên cha |
| `{{TenMe}}` | Họ tên mẹ |
| `{{DiaChiGiaoDan}}` | Địa chỉ |
| `{{DienThoaiGiaoDan}}` | Điện thoại |
| `{{EmailGiaoDan}}` | Email |
| `{{DanToc}}` | Dân tộc |
| `{{NgheNghiep}}` | Nghề nghiệp |
| `{{TrinhDoVanHoa}}` | Trình độ văn hoá |
| `{{TrinhDoChuyenMon}}` | Trình độ chuyên môn |
| `{{BietNgoaiNgu}}` | Biết ngoại ngữ |
| `{{MoTaRuaToi}}` | Dòng mô tả bí tích Rửa tội (số sổ/ngày/nơi/cha rửa/người đỡ đầu) |
| `{{MoTaRuocLe}}` | Dòng mô tả bí tích Rước lễ lần đầu |
| `{{MoTaThemSuc}}` | Dòng mô tả bí tích Thêm sức |
| `{{SoHonPhoi}}` | Số sổ hôn phối |
| `{{VoChong}}` | Họ tên vợ/chồng |
| `{{NgayHonPhoi}}` | Ngày hôn phối |
| `{{NoiHonPhoi}}` | Nơi hôn phối |
| `{{ChaHonPhoi}}` | Linh mục chứng hôn |
| `{{CachThucHonPhoi}}` | Cách thức hôn phối |
| `{{NguoiChung1}}` | Người chứng thứ nhất |
| `{{NguoiChung2}}` | Người chứng thứ hai |
| `{{GhiChuHonPhoi}}` | Ghi chú hôn phối |
| `{{TenChanhXu}}` | Tên linh mục chánh xứ đương nhiệm |
| `{{ConHoc}}` | Dấu [x]/[ ] — còn học |
| `{{TanTong}}` | Dấu [x]/[ ] — tân tòng |
| `{{DaCoGiaDinh}}` | Dấu [x]/[ ] — đã có gia đình |
| `{{QuaDoi}}` | Dấu [x]/[ ] — đã qua đời |
| `{{NgayQuaDoi}}` | Cụm "— ngày qua đời" (rỗng nếu còn sống) |
| `{{NoiAnTang}}` | Cụm "— an táng tại..." (rỗng nếu còn sống) |
| `{{SoAnTang}}` | Cụm "(số mộ...)" (rỗng nếu còn sống) |
| `{{NgayThangNamIn}}` | Ngày tháng năm in phiếu |
| `{{KhoiAnh}}` | [Khối ảnh đại diện — không gõ tay, hệ thống tự chèn] |

### Chứng nhận bí tích (`ChungNhanBiTich`) — 17 chỗ trống

| Chỗ trống | Ý nghĩa |
|---|---|
| `{{TenGiaoPhan}}` | Tên giáo phận |
| `{{TenGiaoHat}}` | Tên giáo hạt |
| `{{TenGiaoXu}}` | Tên giáo xứ |
| `{{DiaChiGiaoXu}}` | Địa chỉ giáo xứ |
| `{{DienThoaiGiaoXu}}` | Điện thoại giáo xứ |
| `{{EmailGiaoXu}}` | Email giáo xứ |
| `{{WebsiteGiaoXu}}` | Website giáo xứ |
| `{{TieuDeBiTich}}` | Tiêu đề (Rửa tội / Xưng tội-Rước lễ / Thêm sức / Các bí tích) |
| `{{TenGiaoHo}}` | Tên giáo họ |
| `{{HoTen}}` | Họ và tên |
| `{{NgaySinh}}` | Ngày sinh |
| `{{NoiSinh}}` | Nơi sinh |
| `{{TenCha}}` | Họ tên cha |
| `{{TenMe}}` | Họ tên mẹ |
| `{{DiaChiGiaoDan}}` | Địa chỉ |
| `{{DanhSachBiTich}}` | [Khối các dòng bí tích được chứng nhận — hệ thống tự dựng] |
| `{{NgayThangNamIn}}` | Ngày tháng năm in |

### Chứng nhận hôn phối (`ChungNhanHonPhoi`) — 31 chỗ trống

| Chỗ trống | Ý nghĩa |
|---|---|
| `{{TenGiaoPhan}}` | Tên giáo phận |
| `{{TenGiaoHat}}` | Tên giáo hạt |
| `{{TenGiaoXu}}` | Tên giáo xứ |
| `{{DiaChiGiaoXu}}` | Địa chỉ giáo xứ |
| `{{DienThoaiGiaoXu}}` | Điện thoại giáo xứ |
| `{{EmailGiaoXu}}` | Email giáo xứ |
| `{{WebsiteGiaoXu}}` | Website giáo xứ |
| `{{HoTenNam}}` | Họ tên người nam |
| `{{HoTenNu}}` | Họ tên người nữ |
| `{{NgaySinhNam}}` | Ngày sinh người nam |
| `{{NgaySinhNu}}` | Ngày sinh người nữ |
| `{{NoiSinhNam}}` | Nơi sinh người nam |
| `{{NoiSinhNu}}` | Nơi sinh người nữ |
| `{{TenChaNam}}` | Họ tên cha người nam |
| `{{TenChaNu}}` | Họ tên cha người nữ |
| `{{TenMeNam}}` | Họ tên mẹ người nam |
| `{{TenMeNu}}` | Họ tên mẹ người nữ |
| `{{GiaoHoNam}}` | Giáo họ người nam |
| `{{GiaoHoNu}}` | Giáo họ người nữ |
| `{{MoTaRuaToiNam}}` | Mô tả rửa tội người nam |
| `{{MoTaRuaToiNu}}` | Mô tả rửa tội người nữ |
| `{{MoTaThemSucNam}}` | Mô tả thêm sức người nam |
| `{{MoTaThemSucNu}}` | Mô tả thêm sức người nữ |
| `{{SoHonPhoi}}` | Số sổ hôn phối |
| `{{NgayHonPhoi}}` | Ngày hôn phối |
| `{{NoiHonPhoi}}` | Nơi hôn phối |
| `{{ChaHonPhoi}}` | Linh mục chứng hôn |
| `{{CachThucHonPhoi}}` | Cách thức hôn phối |
| `{{NguoiChung1}}` | Người chứng thứ nhất |
| `{{NguoiChung2}}` | Người chứng thứ hai |
| `{{NgayThangNamIn}}` | Ngày tháng năm in |

### Phiếu gia đình — khổ A4 và khổ A3 dùng chung mẫu này (`PhieuGiaDinh`) — 13 chỗ trống

| Chỗ trống | Ý nghĩa |
|---|---|
| `{{TenGiaoPhan}}` | Tên giáo phận |
| `{{TenGiaoHat}}` | Tên giáo hạt |
| `{{TenGiaoXu}}` | Tên giáo xứ |
| `{{TenGiaoHo}}` | Tên giáo họ |
| `{{MaGiaDinh}}` | Mã gia đình (số riêng hoặc số cũ) |
| `{{TenGiaDinh}}` | Tên gia đình |
| `{{DienThoaiGiaDinh}}` | Điện thoại gia đình |
| `{{DiaChiGiaDinh}}` | Địa chỉ gia đình |
| `{{MoTaHonPhoi}}` | Mô tả hôn phối của cặp vợ chồng |
| `{{GhiChuGiaDinh}}` | Ghi chú |
| `{{NgayThangNamIn}}` | Ngày tháng năm in |
| `{{HangThanhVien}}` | [Khối các dòng thành viên — hệ thống tự dựng theo số người thật] |
| `{{KhoiAnh}}` | [Khối ảnh đại diện gia đình — không gõ tay, hệ thống tự chèn] |

### Giấy giới thiệu chứng nhận rửa tội (`GioiThieuRuaToi`) — 18 chỗ trống

| Chỗ trống | Ý nghĩa |
|---|---|
| `{{TenGiaoPhan}}` | Tên giáo phận |
| `{{TenGiaoHat}}` | Tên giáo hạt |
| `{{TenGiaoXu}}` | Tên giáo xứ |
| `{{DiaChiGiaoXu}}` | Địa chỉ giáo xứ |
| `{{DienThoaiGiaoXu}}` | Điện thoại giáo xứ |
| `{{EmailGiaoXu}}` | Email giáo xứ |
| `{{WebsiteGiaoXu}}` | Website giáo xứ |
| `{{HoTen}}` | Họ và tên |
| `{{NgaySinh}}` | Ngày sinh |
| `{{NoiSinh}}` | Nơi sinh |
| `{{TenCha}}` | Họ tên cha |
| `{{TenMe}}` | Họ tên mẹ |
| `{{DiaChiGiaoDan}}` | Địa chỉ |
| `{{DienThoaiGiaoDan}}` | Điện thoại |
| `{{TenGiaoPhan2}}` | Tên giáo phận nơi nhận (nhập tay lúc in) |
| `{{TenGiaoXu2}}` | Tên giáo xứ nơi nhận (nhập tay lúc in) |
| `{{TenLinhMuc}}` | Tên linh mục ký giấy giới thiệu (nhập tay lúc in) |
| `{{NgayThangNamIn}}` | Ngày tháng năm in |

### Giấy giới thiệu chứng nhận thêm sức (`GioiThieuThemSuc`) — 17 chỗ trống

| Chỗ trống | Ý nghĩa |
|---|---|
| `{{TenGiaoPhan}}` | Tên giáo phận |
| `{{TenGiaoHat}}` | Tên giáo hạt |
| `{{TenGiaoXu}}` | Tên giáo xứ |
| `{{DiaChiGiaoXu}}` | Địa chỉ giáo xứ |
| `{{DienThoaiGiaoXu}}` | Điện thoại giáo xứ |
| `{{EmailGiaoXu}}` | Email giáo xứ |
| `{{WebsiteGiaoXu}}` | Website giáo xứ |
| `{{HoTen}}` | Họ và tên |
| `{{NgaySinh}}` | Ngày sinh |
| `{{NoiSinh}}` | Nơi sinh |
| `{{TenCha}}` | Họ tên cha |
| `{{TenMe}}` | Họ tên mẹ |
| `{{MoTaRuaToi}}` | Mô tả đã rửa tội (bỏ trống nếu thiếu dữ liệu) |
| `{{TenGiaoPhan2}}` | Tên giáo phận nơi nhận (nhập tay lúc in) |
| `{{TenGiaoXu2}}` | Tên giáo xứ nơi nhận (nhập tay lúc in) |
| `{{TenLinhMuc}}` | Tên linh mục ký giấy giới thiệu (nhập tay lúc in) |
| `{{NgayThangNamIn}}` | Ngày tháng năm in |

### Giấy giới thiệu giáo lý hôn phối (`GioiThieuGiaoLyHonPhoi`) — 21 chỗ trống

| Chỗ trống | Ý nghĩa |
|---|---|
| `{{TenGiaoPhan}}` | Tên giáo phận |
| `{{TenGiaoHat}}` | Tên giáo hạt |
| `{{TenGiaoXu}}` | Tên giáo xứ |
| `{{DiaChiGiaoXu}}` | Địa chỉ giáo xứ |
| `{{DienThoaiGiaoXu}}` | Điện thoại giáo xứ |
| `{{EmailGiaoXu}}` | Email giáo xứ |
| `{{WebsiteGiaoXu}}` | Website giáo xứ |
| `{{TenGiaoHo}}` | Tên giáo họ |
| `{{HoTen}}` | Họ và tên |
| `{{NgaySinh}}` | Ngày sinh |
| `{{NoiSinh}}` | Nơi sinh |
| `{{TenCha}}` | Họ tên cha |
| `{{TenMe}}` | Họ tên mẹ |
| `{{DiaChiGiaoDan}}` | Địa chỉ |
| `{{DienThoaiGiaoDan}}` | Điện thoại |
| `{{MoTaRuaToi}}` | Mô tả rửa tội |
| `{{MoTaThemSuc}}` | Mô tả thêm sức |
| `{{TenGiaoPhan2}}` | Tên giáo phận nơi nhận (nhập tay lúc in) |
| `{{TenGiaoXu2}}` | Tên giáo xứ nơi nhận (nhập tay lúc in) |
| `{{TenLinhMuc}}` | Tên linh mục ký giấy giới thiệu (nhập tay lúc in) |
| `{{NgayThangNamIn}}` | Ngày tháng năm in |

### Giấy giới thiệu chuyển xứ (`GioiThieuChuyenXu`) — 15 chỗ trống

| Chỗ trống | Ý nghĩa |
|---|---|
| `{{TenGiaoPhan}}` | Tên giáo phận |
| `{{TenGiaoHat}}` | Tên giáo hạt |
| `{{TenGiaoXu}}` | Tên giáo xứ |
| `{{DiaChiGiaoXu}}` | Địa chỉ giáo xứ |
| `{{DienThoaiGiaoXu}}` | Điện thoại giáo xứ |
| `{{EmailGiaoXu}}` | Email giáo xứ |
| `{{WebsiteGiaoXu}}` | Website giáo xứ |
| `{{TenChuHo}}` | Họ tên chủ hộ |
| `{{DienThoaiGiaDinh}}` | Điện thoại gia đình |
| `{{DiaChiGiaDinh}}` | Địa chỉ gia đình |
| `{{TenGiaoPhan2}}` | Tên giáo phận nơi nhận (nhập tay lúc in) |
| `{{TenGiaoXu2}}` | Tên giáo xứ nơi nhận (nhập tay lúc in) |
| `{{TenLinhMuc}}` | Tên linh mục ký giấy giới thiệu (nhập tay lúc in) |
| `{{NgayThangNamIn}}` | Ngày tháng năm in |
| `{{HangThanhVien}}` | [Khối các dòng thành viên — hệ thống tự dựng] |

### Xin điều tra và rao hôn phối (`RaoHonPhoi`) — 26 chỗ trống

| Chỗ trống | Ý nghĩa |
|---|---|
| `{{TenGiaoPhan}}` | Tên giáo phận |
| `{{TenGiaoHat}}` | Tên giáo hạt |
| `{{TenGiaoXu}}` | Tên giáo xứ |
| `{{AnhChi1}}` | "Anh"/"Chị" theo giới tính người thứ nhất |
| `{{AnhChi2}}` | "Anh"/"Chị" theo giới tính người thứ hai |
| `{{HoTen1}}` | Họ tên người thứ nhất |
| `{{HoTen2}}` | Họ tên người thứ hai |
| `{{Tuoi1}}` | Tuổi người thứ nhất |
| `{{Tuoi2}}` | Tuổi người thứ hai |
| `{{TenCha1}}` | Họ tên cha người thứ nhất |
| `{{TenCha2}}` | Họ tên cha người thứ hai |
| `{{TenMe1}}` | Họ tên mẹ người thứ nhất |
| `{{TenMe2}}` | Họ tên mẹ người thứ hai |
| `{{TenGiaoXu1}}` | Giáo xứ hiện tại người thứ nhất |
| `{{TenGiaoXu2}}` | Giáo xứ hiện tại người thứ hai |
| `{{TenGiaoXuNQ1}}` | Giáo xứ nguyên quán người thứ nhất |
| `{{TenGiaoPhanNQ1}}` | Giáo phận nguyên quán người thứ nhất |
| `{{TenGiaoXuNQ2}}` | Giáo xứ nguyên quán người thứ hai |
| `{{TenGiaoPhanNQ2}}` | Giáo phận nguyên quán người thứ hai |
| `{{TenGiaoXuTruoc1}}` | Giáo xứ trước đây người thứ nhất |
| `{{TenGiaoPhanTruoc1}}` | Giáo phận trước đây người thứ nhất |
| `{{TenGiaoXuTruoc2}}` | Giáo xứ trước đây người thứ hai |
| `{{TenGiaoPhanTruoc2}}` | Giáo phận trước đây người thứ hai |
| `{{TenGiaoXuNhan}}` | Cha xứ nơi nhận đơn (dữ liệu lấy từ cột "Linh mục nhận" — sai khác nhãn cố ý migrate nguyên trạng từ bản desktop, xem `can-review-sau.md`) |
| `{{TenGiaoPhanNhan}}` | Giáo xứ nơi nhận đơn (dữ liệu lấy từ cột "Giáo xứ nhận" — cùng ghi chú trên) |
| `{{NgayThangNamIn}}` | Ngày tháng năm in |

### Kết quả rao hôn phối (`KQRaoHonPhoi`) — 33 chỗ trống

| Chỗ trống | Ý nghĩa |
|---|---|
| `{{TenGiaoPhan}}` | Tên giáo phận |
| `{{TenGiaoXu}}` | Tên giáo xứ |
| `{{TenGiaoXuNhan}}` | Cha xứ nơi nhận đơn (cùng ghi chú sai khác nhãn ở mẫu "Xin điều tra và rao hôn phối") |
| `{{TenGiaoPhanNhan}}` | Giáo xứ nơi nhận đơn (cùng ghi chú trên) |
| `{{TenLinhMucGui}}` | Tên linh mục gửi (hiện luôn để trống — chưa có màn hình nhập) |
| `{{Phai1}}` | Giới tính người thứ nhất |
| `{{Phai2}}` | Giới tính người thứ hai |
| `{{AnhChi1}}` | "Anh"/"Chị" người thứ nhất |
| `{{AnhChi2}}` | "Anh"/"Chị" người thứ hai |
| `{{HoTen1}}` | Họ tên người thứ nhất |
| `{{HoTen2}}` | Họ tên người thứ hai |
| `{{DienThoai1}}` | Điện thoại người thứ nhất |
| `{{DienThoai2}}` | Điện thoại người thứ hai |
| `{{NgaySinh1}}` | Ngày sinh người thứ nhất |
| `{{NgaySinh2}}` | Ngày sinh người thứ hai |
| `{{NoiSinh1}}` | Nơi sinh người thứ nhất |
| `{{NoiSinh2}}` | Nơi sinh người thứ hai |
| `{{MoTaRuaToi1}}` | Mô tả rửa tội người thứ nhất |
| `{{MoTaRuaToi2}}` | Mô tả rửa tội người thứ hai |
| `{{MoTaThemSuc1}}` | Mô tả thêm sức người thứ nhất |
| `{{MoTaThemSuc2}}` | Mô tả thêm sức người thứ hai |
| `{{TenCha1}}` | Họ tên cha người thứ nhất |
| `{{TenCha2}}` | Họ tên cha người thứ hai |
| `{{TenMe1}}` | Họ tên mẹ người thứ nhất |
| `{{TenMe2}}` | Họ tên mẹ người thứ hai |
| `{{TenGiaoXu1}}` | Giáo xứ người thứ nhất |
| `{{TenGiaoXu2}}` | Giáo xứ người thứ hai |
| `{{TenGiaoPhan1}}` | Giáo phận người thứ nhất |
| `{{TenGiaoPhan2}}` | Giáo phận người thứ hai |
| `{{DiaChi1}}` | Địa chỉ người thứ nhất |
| `{{DiaChi2}}` | Địa chỉ người thứ hai |
| `{{RaoHonPhoi}}` | [Khối 3 dòng ngày rao lần 1/2/3 — hệ thống tự dựng] |
| `{{NgayThangNamIn}}` | Ngày tháng năm in |

### In danh sách giáo dân (`DanhSachGiaoDan`) — 5 chỗ trống

| Chỗ trống | Ý nghĩa |
|---|---|
| `{{TenGiaoXu}}` | Tên giáo xứ |
| `{{SoLuong}}` | Tổng số giáo dân trong danh sách |
| `{{DieuKienLoc}}` | Mô tả điều kiện lọc đang áp dụng |
| `{{NgayThangNamIn}}` | Ngày giờ in |
| `{{HangDanhSach}}` | [Khối các dòng danh sách — hệ thống tự dựng, 29 cột] |

### In danh sách gia đình (`DanhSachGiaDinh`) — 5 chỗ trống

| Chỗ trống | Ý nghĩa |
|---|---|
| `{{TenGiaoXu}}` | Tên giáo xứ |
| `{{SoLuong}}` | Tổng số gia đình trong danh sách |
| `{{DieuKienLoc}}` | Mô tả điều kiện lọc đang áp dụng |
| `{{NgayThangNamIn}}` | Ngày giờ in |
| `{{HangDanhSach}}` | [Khối các dòng danh sách — hệ thống tự dựng, 12 cột] |

### In danh sách rao hôn phối (`DanhSachRaoHonPhoi`) — 5 chỗ trống

| Chỗ trống | Ý nghĩa |
|---|---|
| `{{TenGiaoXu}}` | Tên giáo xứ |
| `{{SoLuong}}` | Tổng số đôi rao trong danh sách |
| `{{DieuKienLoc}}` | Mô tả điều kiện lọc đang áp dụng |
| `{{NgayThangNamIn}}` | Ngày giờ in |
| `{{HangDanhSach}}` | [Khối các dòng danh sách — hệ thống tự dựng, 8 cột] |

## 12. Cách hiển thị dữ liệu đúng/sai

### 12.1 Vấn đề và cách giải

Một số mục trên giấy tờ chỉ có hai trạng thái (có/không): "còn đi học", "tân tòng", "đã có gia
đình", "đã qua đời", "gia đình đã chuyển xứ". Bản desktop in cứng dấu `[x]` khi đúng và `[  ]`
khi sai (`Source/ExcelReport/ReportLyLichCaNhan.cs` dòng 122-124), bản web ban đầu migrate nguyên
trạng bằng `InAnService.Dau(bool)`.

Nhiều giáo xứ muốn in **câu chữ** thay cho dấu ngoặc vuông — ví dụ mục tân tòng: khi đúng in chữ
"Tân tòng", khi sai **để trắng**; hoặc in "Là tân tòng: đúng". Vì vậy màn hình này có thêm khu vực
**"Cách hiển thị dữ liệu đúng/sai"**: với mỗi mục, giáo xứ gõ đúng **hai ô** — "Khi có" và "Khi
không".

CỐ Ý **không** làm bằng cú pháp điều kiện trong mẫu (`{{#if}}`…): người dùng là quý cha/quý sơ
phần lớn không rành máy tính, một bảng hai ô để gõ không cần học cú pháp nào. Hướng dẫn trên màn
hình nói rõ: *"Để trống ô nào thì chỗ đó không in ra chữ gì."*

### 12.2 Thứ tự phân giải — GIỐNG HỆT mẫu in

Đúng ba cấp, xét **theo từng biến một** (không phải theo cả bảng), cùng thứ tự với
`InAnService.DungMau` để người dùng chỉ phải hiểu MỘT quy tắc ưu tiên cho cả màn hình:

1. Câu chữ RIÊNG của giáo xứ đang đăng nhập (`GiaoXuId` = claim).
2. Câu chữ cấp HỆ THỐNG do Quản trị hệ thống đặt (`GiaoXuId IS NULL`).
3. **Mặc định gốc** `[x]` / `[  ]` — giữ nguyên xi hành vi cũ.

Giáo xứ chưa tuỳ chỉnh gì thì bản in **không đổi một ly nào** — tương thích ngược tuyệt đối, có
bài test khẳng định (`CachHienThiDungSaiTests.Chua_tuy_chinh_gi_thi_ban_in_khong_doi_van_la_dau_ngoac_vuong`).

Một dòng tuỳ chỉnh ghi đè **cả hai vế** của đúng biến đó; ô để trống (NULL/rỗng) nghĩa là "không
in gì" và **không** rơi tiếp xuống cấp dưới — nếu không thì giáo xứ sẽ không có cách nào làm cho
một vế im lặng, mà đó chính là cách dùng chính.

### 12.3 Danh mục 5 biến đúng/sai

Đúng tập khoá mà `InAnService` dựng bằng bảng câu chữ, không hơn không kém (`BienDungSaiCatalog`):

| Biến | Nhãn trên màn hình | Nhóm | Mẫu in có dùng |
|---|---|---|---|
| `ConHoc` | Còn đi học | Giáo dân | Lý lịch cá nhân |
| `TanTong` | Tân tòng | Giáo dân | Lý lịch cá nhân |
| `DaCoGiaDinh` | Đã có gia đình | Giáo dân | Lý lịch cá nhân |
| `QuaDoi` | Đã qua đời | Giáo dân | Lý lịch cá nhân |
| `DaChuyenXu` | Gia đình đã chuyển xứ | Gia đình | Phiếu gia đình, Giới thiệu chuyển xứ, Giới thiệu rửa tội |

CỐ Ý **không** liệt kê mọi thuộc tính `bool` của thực thể: những cờ như `GiaDinh.KhongThongKe` hay
`DaXoa` không được công bố thành biến in nào trong `MauInCatalog`, nên không có chỗ nào trên tờ
giấy để tuỳ chỉnh — đưa vào chỉ tạo ra những dòng bấm vào không có tác dụng gì. Có bài test khẳng
định mọi biến trong danh mục đều là biến in thật **và** thật sự đổi được bản in.

Cũng CỐ Ý **không** áp dụng cho các cột đúng/sai trong "In danh sách giáo dân/gia đình"
(`InAnService.BoolDs`, in `✓`/`—`): đó là ô của một bảng 29 cột khổ ngang, không phải biến
`{{Key}}` người dùng chèn được, và nhét một cụm từ dài vào đó sẽ phá vỡ bố cục.

### 12.4 Lưu trữ

Bảng `cach_hien_thi_dung_sai`, dựng **đúng khuôn** `mau_in_tuy_chinh` (đọc ghi chú ở
`CachHienThiDungSai.cs` và `MauInTuyChinh.cs`):

- `GiaoXuId` (`uuid`, **NULL được**) — NULL = dòng cấp hệ thống.
- `TenBien`, `KhiDung` (≤200 ký tự), `KhiSai` (≤200), `CreatedAt`, `UpdatedAt`, `xmin` (RowVersion).
- **Hai chỉ mục MỘT PHẦN** thay cho một `UNIQUE(GiaoXuId, TenBien)` thường, vì PostgreSQL coi hai
  NULL là khác nhau trong chỉ mục duy nhất nên chỉ mục thường KHÔNG chặn được hai dòng hệ thống
  trùng `TenBien`.
- **KHÔNG** nằm trong bộ lọc toàn cục theo `GiaoXuId` của `QlgxDbContext`, và **KHÔNG** bật RLS:
  policy chung của dự án là `giao_xu_id::text = current_setting('app.giao_xu_id', true)`, mà với
  dòng cấp hệ thống `giao_xu_id IS NULL` nên vế so sánh trả NULL — policy đó sẽ **giấu mất** chính
  những dòng hệ thống cần mọi giáo xứ đọc được. Hai bảng "có dòng cấp hệ thống" này vì vậy lọc
  **tay, tường minh** ở tầng service. Xem ghi chú ở migration `ThemCachHienThiDungSai` để biết cách
  bật RLS đúng cho cả hai bảng nếu sau này muốn thêm lớp phòng thủ CSDL.

### 12.5 API

Đối xứng với `MauInEndpoints`, cùng cách đặt policy:

| Route | Policy | Ghi chú |
|---|---|---|
| `GET /api/cach-hien-thi` | đã đăng nhập | Trả 5 biến + câu chữ đang áp dụng + `capDangDung` + **cả hai cấp** |
| `PUT`/`DELETE` `/api/cach-hien-thi/{tenBien}/rieng` | `QuanTri` | `GiaoXuId` LUÔN từ claim, KHÔNG BAO GIỜ qua tham số |
| `PUT`/`DELETE` `/api/cach-hien-thi/{tenBien}/he-thong` | `QuanTriHeThong` | — |

Khác một điểm có chủ đích so với mẫu in: **không** có `GET` riêng theo cấp. Màn hình mẫu in mở
từng mẫu MỘT trong trình soạn thảo nên một GET mỗi lần là hợp lý; ở đây cả 5 biến hiện cùng lúc
trên một bảng, tách route theo cấp sẽ thành 5 lượt gọi chỉ để vẽ xong một bảng.

`RowVersion` chống ghi đè âm thầm y như `MauInService.Luu`, kể cả quy ước **`RowVersion: 0` nghĩa
là "chưa từng tuỳ chỉnh, đây là lần lưu đầu"** (tạo mới thay vì báo xung đột).

### 12.6 Khử trùng: dựa vào HtmlEncoder, không lọc thêm

Câu chữ đi vào mẫu qua từ điển `duLieu`, mà `BoDoMauIn.ApDung` cho **mọi** giá trị trong `duLieu`
chạy qua `HtmlEncoder.Default.Encode`. Nên giáo xứ gõ `<script>alert(1)</script>` thì trên giấy in
ra đúng mấy chữ đó, **không** thành thẻ sống — đã kiểm bằng bài test thật
(`Cau_chu_co_the_script_bi_thoat_html_khong_thanh_the_song`), không tin suông.

Vì vậy service **cố ý KHÔNG** gọi `MauInHtmlSanitizer` cho hai ô này (khác hẳn nội dung mẫu HTML):
lọc thêm chỉ làm hỏng câu chữ hợp lệ có dấu `<` `>` `&`. Chỉ chuẩn hoá (cắt khoảng trắng hai đầu,
quy chuỗi rỗng về NULL) và giới hạn 200 ký tự.

Lưu ý khi viết test: `HtmlEncoder` thoát **cả ký tự tiếng Việt có dấu** thành thực thể số
("Tân" → `T&#xE2;n`), nên mọi phép so chuỗi tiếng Việt trên HTML in ra phải đi qua cùng bộ mã hoá.

### 12.7 Hiệu năng: nạp một lần cho mỗi lượt in

`DungHtmlLyLichCaNhan` chạy **lặp cho từng thành viên** khi in lý lịch cả gia đình
(`XuatLyLichCaNhanGiaDinh`). Nếu tra CSDL bên trong đó thì tốn (số thành viên) truy vấn, và nếu
tra theo từng biến thì còn nhân thêm 5 lần nữa.

Cách giải: `InAnService` giữ một trường `_bangCachHienThi` nhớ lại bảng đã phân giải. `InAnService`
đăng ký **Scoped** nên một thực thể phục vụ đúng một yêu cầu HTTP, tức đúng một lượt in — nhớ ở đây
chính là "nạp một lần cho mỗi lượt in", không rò rỉ bảng của giáo xứ này sang yêu cầu của giáo xứ
khác. Cả hai cấp lấy trong **một** truy vấn (`GiaoXuId == gx || GiaoXuId == null`) rồi phân giải
trong bộ nhớ.

Có bài test **đếm thẳng số câu lệnh SQL** chạm bảng qua một `DbCommandInterceptor` và khẳng định
đúng **1** truy vấn khi in cho gia đình 3 người
(`In_ca_gia_dinh_chi_nap_bang_cau_chu_dung_mot_lan`) — kèm một bộ đếm tổng số câu lệnh để bài test
không "đạt" nhầm khi interceptor chưa được gắn. Lưu ý: interceptor phải gắn qua
`AddDbContext(...).AddInterceptors(...)`, **không** phải bằng cách thả `IInterceptor` vào DI — dự án
này gắn interceptor trong `QlgxDbContext.OnConfiguring` nên đường auto-discover qua DI không có
tác dụng.

### 12.8 Giao diện

Khu vực riêng ngay dưới danh sách mẫu in (cùng màn hình, vì hai việc luôn đi cùng nhau: chèn biến
`{{TanTong}}` vào mẫu ở phần trên rồi quyết định nó in ra chữ gì ở phần này). Bảng gồm: nhãn mục
tiếng Việt, ô "Khi có", ô "Khi không", huy hiệu cấp đang dùng (dùng lại `HuyHieuCap` của phần mẫu
in), nút "Lưu", nút "Khôi phục mặc định" (chỉ hiện khi **cấp đang sửa** đã tuỳ chỉnh).

Tài khoản Quản trị hệ thống sửa cấp hệ thống, tài khoản Quản trị viên giáo xứ sửa cấp riêng, tài
khoản thường **chỉ đọc** câu chữ đang áp dụng (không có ô nhập, không có nút Lưu) — đúng cách màn
hình mẫu in đang phân biệt.

Hai ô nhập khởi tạo từ câu chữ của **đúng cấp đang sửa** (không phải câu chữ đang áp dụng): quản
trị hệ thống mở màn hình phải thấy ô trống khi chính cấp hệ thống chưa đặt gì, dù giáo xứ đã đè
riêng — nếu không, bấm Lưu sẽ vô tình sao chép câu chữ của cấp khác sang cấp mình.
