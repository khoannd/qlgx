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
- Dropdown "Chèn chỗ trống" — liệt kê đúng các `{{Key}}` khả dụng CHO ĐÚNG mẫu đang sửa, kèm
  nhãn tiếng Việt; chọn một mục chèn `{{Key}}` vào đúng vị trí con trỏ trong vùng soạn thảo.
- Vùng soạn thảo rich-text (`react-simple-wysiwyg`) — xem mục 8.
- Nút "Xem thử" — vẽ PDF ngay từ nội dung NHÁP, mở tab mới.
- Nút "Lưu".
- Nút "Khôi phục về mặc định" — chỉ hiện khi đã có tuỳ chỉnh (`daTuyChinh=true`).

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

## 10. Danh mục 238 chỗ trống theo từng mẫu

Đọc trực tiếp từ đối chiếu `InAnService.cs` (nơi gán giá trị cho từng `Key`) với các tệp
`PrintTemplates/Chung/*.html` (nơi dùng `Key`) — có bài kiểm tra tự động
(`MauInCatalogTests.Danh_muc_khop_dung_tap_hop_cho_trong_that_trong_tep_html`) đối chiếu lại
danh mục này với đúng tập `{{Key}}` thật trong từng tệp `.html`, tự báo lỗi nếu ai gõ nhầm tên
hay bỏ sót khi sửa `MauInCatalog.cs` sau này.

### Lý lịch cá nhân (`LyLichCaNhan`) — 43 chỗ trống

| Chỗ trống | Ý nghĩa |
|---|---|
| `{{TenGiaoPhan}}` | Tên giáo phận |
| `{{TenGiaoHat}}` | Tên giáo hạt |
| `{{TenGiaoXu}}` | Tên giáo xứ |
| `{{DiaChiGiaoXu}}` | Địa chỉ giáo xứ |
| `{{DienThoaiGiaoXu}}` | Điện thoại giáo xứ |
| `{{EmailGiaoXu}}` | Email giáo xứ |
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
| `{{ConHoc}}` | Dấu [x]/[ ] — còn học |
| `{{TanTong}}` | Dấu [x]/[ ] — tân tòng |
| `{{DaCoGiaDinh}}` | Dấu [x]/[ ] — đã có gia đình |
| `{{QuaDoi}}` | Dấu [x]/[ ] — đã qua đời |
| `{{NgayQuaDoi}}` | Cụm "— ngày qua đời" (rỗng nếu còn sống) |
| `{{NoiAnTang}}` | Cụm "— an táng tại..." (rỗng nếu còn sống) |
| `{{SoAnTang}}` | Cụm "(số mộ...)" (rỗng nếu còn sống) |
| `{{NgayThangNamIn}}` | Ngày tháng năm in phiếu |
| `{{KhoiAnh}}` | [Khối ảnh đại diện — không gõ tay, hệ thống tự chèn] |

### Chứng nhận bí tích (`ChungNhanBiTich`) — 16 chỗ trống

| Chỗ trống | Ý nghĩa |
|---|---|
| `{{TenGiaoPhan}}` | Tên giáo phận |
| `{{TenGiaoHat}}` | Tên giáo hạt |
| `{{TenGiaoXu}}` | Tên giáo xứ |
| `{{DiaChiGiaoXu}}` | Địa chỉ giáo xứ |
| `{{DienThoaiGiaoXu}}` | Điện thoại giáo xứ |
| `{{EmailGiaoXu}}` | Email giáo xứ |
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

### Chứng nhận hôn phối (`ChungNhanHonPhoi`) — 30 chỗ trống

| Chỗ trống | Ý nghĩa |
|---|---|
| `{{TenGiaoPhan}}` | Tên giáo phận |
| `{{TenGiaoHat}}` | Tên giáo hạt |
| `{{TenGiaoXu}}` | Tên giáo xứ |
| `{{DiaChiGiaoXu}}` | Địa chỉ giáo xứ |
| `{{DienThoaiGiaoXu}}` | Điện thoại giáo xứ |
| `{{EmailGiaoXu}}` | Email giáo xứ |
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

### Giấy giới thiệu chứng nhận rửa tội (`GioiThieuRuaToi`) — 17 chỗ trống

| Chỗ trống | Ý nghĩa |
|---|---|
| `{{TenGiaoPhan}}` | Tên giáo phận |
| `{{TenGiaoHat}}` | Tên giáo hạt |
| `{{TenGiaoXu}}` | Tên giáo xứ |
| `{{DiaChiGiaoXu}}` | Địa chỉ giáo xứ |
| `{{DienThoaiGiaoXu}}` | Điện thoại giáo xứ |
| `{{EmailGiaoXu}}` | Email giáo xứ |
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

### Giấy giới thiệu chứng nhận thêm sức (`GioiThieuThemSuc`) — 16 chỗ trống

| Chỗ trống | Ý nghĩa |
|---|---|
| `{{TenGiaoPhan}}` | Tên giáo phận |
| `{{TenGiaoHat}}` | Tên giáo hạt |
| `{{TenGiaoXu}}` | Tên giáo xứ |
| `{{DiaChiGiaoXu}}` | Địa chỉ giáo xứ |
| `{{DienThoaiGiaoXu}}` | Điện thoại giáo xứ |
| `{{EmailGiaoXu}}` | Email giáo xứ |
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

### Giấy giới thiệu giáo lý hôn phối (`GioiThieuGiaoLyHonPhoi`) — 20 chỗ trống

| Chỗ trống | Ý nghĩa |
|---|---|
| `{{TenGiaoPhan}}` | Tên giáo phận |
| `{{TenGiaoHat}}` | Tên giáo hạt |
| `{{TenGiaoXu}}` | Tên giáo xứ |
| `{{DiaChiGiaoXu}}` | Địa chỉ giáo xứ |
| `{{DienThoaiGiaoXu}}` | Điện thoại giáo xứ |
| `{{EmailGiaoXu}}` | Email giáo xứ |
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

### Giấy giới thiệu chuyển xứ (`GioiThieuChuyenXu`) — 14 chỗ trống

| Chỗ trống | Ý nghĩa |
|---|---|
| `{{TenGiaoPhan}}` | Tên giáo phận |
| `{{TenGiaoHat}}` | Tên giáo hạt |
| `{{TenGiaoXu}}` | Tên giáo xứ |
| `{{DiaChiGiaoXu}}` | Địa chỉ giáo xứ |
| `{{DienThoaiGiaoXu}}` | Điện thoại giáo xứ |
| `{{EmailGiaoXu}}` | Email giáo xứ |
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
