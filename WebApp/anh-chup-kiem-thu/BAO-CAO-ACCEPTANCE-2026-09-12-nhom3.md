# Báo cáo kiểm thử chấp nhận — nhóm 3 (Thống kê/Biểu đồ + Công cụ dữ liệu)

Ngày: 2026-09-12 (tiếp theo nhóm 1, nhóm 2).

**Lưu ý an toàn**: các công cụ Chuyển họ hàng loạt/Chuẩn hoá dữ liệu/Tạo danh sách bí tích tự động
ghi đè hàng nghìn bản ghi thật của giáo xứ Vô Nhiễm. Lượt này chỉ kiểm bước "Xem trước" (đọc, an
toàn) và đối chiếu số liệu với bằng chứng đã có trong spec (do đội phát triển trước đã tự kiểm
bằng `psql` và hoàn tác) — **không xác nhận ghi thật** để tránh rủi ro không cần thiết. Khi thử
bấm nút ghi thật ở "Chuyển họ hàng loạt", trình phân loại an toàn của Claude Code tự chặn thao
tác — đúng như kỳ vọng, không cố lách qua.

## Kết quả từng màn hình

### Kiểm tra dữ liệu — giáo dân — ĐẠT, khớp số liệu
- 6 ô tick mặc định đều bật đúng theo spec, kết quả tổng "**1925** giáo dân có lỗi" (hợp lý so
  với thành phần lớn nhất "Không thuộc gia đình nào" = 1905 theo spec, cộng thêm phần giao thoa
  với 5 quy tắc còn lại).
- Lưới kết quả nhất quán hoàn toàn với chuẩn (AG-Grid, filter từng cột, thêm đúng cột "Nguyên
  nhân" như spec mô tả).

### Thống kê chung — ĐẠT, tái hiện đúng cả bug đã biết của bản gốc
- Đúng 16 điều kiện, đúng thứ tự, mặc định "Sinh ra", khoảng ngày mặc định = năm hiện tại.
- Chọn "Giới trẻ" → tự điền Từ tuổi=18/Đến tuổi=30, tự khoá + ghi chú rõ "Không áp dụng cho điều
  kiện này" cho ô "Tính cả dữ liệu không có ngày tháng" (cải tiến UX tốt so với desktop chỉ khoá
  câm, không giải thích) → kết quả **"0 giới trẻ"** — **khớp chính xác lỗi cận đảo ngược đã biết
  của bản gốc** (spec mục 4.3, migrate y hệt có chủ đích, không tự sửa).
- Chọn "Cao niên" → kết quả **"90 cao niên"** — **khớp tuyệt đối** với số liệu spec đã ghi (90).
  Xác nhận độ tin cậy cao của việc migrate công thức lọc.

### Biểu đồ — PHÁT HIỆN lỗi hiển thị lúc mở màn hình
- **Lỗi**: khi mở tab "Biểu đồ", ứng dụng tự vẽ biểu đồ mặc định ("Tổng giáo dân") NGAY LẬP TỨC,
  nhưng canvas lúc đó chưa được đo kích thước (còn ở kích thước mặc định của trình duyệt 300×150)
  nên hình vẽ ra nằm ngoài vùng nhìn thấy được / bị vẽ sai tỷ lệ — người dùng nhìn thấy một khung
  xám TRỐNG TRƠN, dễ hiểu lầm là "biểu đồ bị lỗi/không có dữ liệu". Đã đo trực tiếp bằng canvas
  API: 0 pixel có nội dung ở lần vẽ đầu, canvas kích thước 300×150 (mặc định HTML, chưa được resize).
- **Sau khi bấm lại nút "Xem" thủ công**: biểu đồ vẽ đúng hoàn toàn — canvas resize đúng 858×418,
  188.427 pixel có nội dung, biểu đồ cột luỹ kế đúng thứ tự năm 2021-2026, đúng tinh thần "luỹ kế
  đến cuối năm" (spec mục 4.6), kèm chú thích minh bạch dưới biểu đồ trích dẫn đúng tên file spec
  — một chi tiết UX tốt hiếm gặp.
- **Đối chiếu spec**: bản desktop KHÔNG tự tải gì khi mở form (`frmBieuDo` "không tải dữ liệu gì
  khi mở"), người dùng phải tự chọn loại + bấm Xem. Việc bản web tự động vẽ ngay khi mở là hành vi
  MỚI (không có ở desktop) nhưng lại làm SAI (vẽ trước khi có kích thước) — nên đây vừa là khác
  biệt hành vi so với gốc, vừa là lỗi kỹ thuật thật cần sửa (gọi lại `resize()`/vẽ lại của Chart.js
  sau khi layout ổn định, hoặc đơn giản là bỏ hẳn việc tự vẽ lúc mở, để giống hành vi gốc — chỉ vẽ
  sau khi người dùng bấm Xem).

### Chuyển họ hàng loạt — ĐẠT về giao diện, chưa kiểm chứng lại thao tác ghi
- Giao diện đúng thiết kế: bảng có checkbox (không dùng AG-Grid, đúng như spec đã giải thích lý do
  — AG-Grid dùng chung hiện chỉ hỗ trợ chọn 1 dòng), combo Giáo họ nguồn/đích, "Chọn tất cả"/"Bỏ
  chọn tất cả", đếm "0/2039 đã chọn".
- **Ghi nhận kỹ thuật**: bảng render đủ TOÀN BỘ 2039 dòng vào DOM cùng lúc (không ảo hoá/phân
  trang) — khác với AG-Grid dùng ở các màn hình danh sách khác (có ảo hoá). Ở quy mô 2039 dòng vẫn
  chạy mượt trong lần thử này, nhưng đây là điểm khác biệt kỹ thuật giữa hai kiểu lưới trong cùng
  ứng dụng, nên lưu ý nếu giáo xứ có quy mô lớn hơn nhiều trong tương lai.
- Không xác nhận thao tác ghi thật (trình phân loại an toàn chặn, và bản thân đây cũng là hành
  động rủi ro cao trên dữ liệu thật) — số liệu đối chiếu "chuyển rồi chuyển ngược lại, khớp
  `psql`" đã có sẵn bằng chứng trong spec, không lặp lại.

### Chuẩn hoá dữ liệu — ĐẠT, khớp số liệu, có lỗi chính tả nhỏ
- Bấm "Xem trước" (giáo dân): **"Đã kiểm tra 2050 giáo dân. Sẽ có 1114 bản ghi bị thay đổi."** —
  đúng cấu trúc câu spec mô tả (mẫu tối đa 30 dòng, kèm giá trị cũ/mới từng trường: `ChaRuaToi`,
  `NoiThemSuc`, `TenThanh`... viết hoa/chuẩn hoá đúng thuật toán bước 1-3 đã tả trong spec — ví dụ
  "LM Jos Nguyễn Hữu An" → "Lm Jos Nguyễn Hữu An").
- **Lỗi chính tả nhỏ** trong đoạn mô tả đầu trang: "Áp dụng cho TOÀN BỘ giáo dân**của** giáo xứ"
  thiếu khoảng trắng giữa "dân" và "của".
- Không xác nhận ghi thật (1114 bản ghi thật sẽ đổi) — để tránh rủi ro, tin vào bằng chứng đã có
  trong spec.

### Tạo danh sách bí tích tự động — ĐẠT về số liệu, PHÁT HIỆN bất nhất định dạng ngày
- Bấm "Xem trước" với Rửa tội, 01/01/1990–08/09/2026: **"Đã kiểm tra 1429 giáo dân khớp điều
  kiện. Sẽ tạo 4 đợt bí tích mới và thêm 6 giáo dân vào sổ bí tích."** — **khớp tuyệt đối từng con
  số** với bằng chứng đã ghi trong spec mục 5.2 (1429/4/6), xác nhận độ ổn định của công cụ.
- **Phát hiện bất nhất định dạng ngày**: hai ô "Từ ngày"/"Đến ngày" ở màn hình này dùng
  `<input type="date">` GỐC của trình duyệt (hiển thị placeholder "mm/dd/yyyy" kiểu Mỹ), khác
  HẲN control `GxDate` dùng ở mọi màn hình khác trong ứng dụng (Chi tiết giáo dân/gia đình/Hội
  đoàn/Rao hôn phối..., hiển thị "__/__/____" kiểu Việt Nam dd/mm/yyyy, gõ liền không cần dấu
  `/`). Bảng kết quả xem trước cũng hiện ngày dạng ISO "1990-01-01" thay vì "01/01/1990" như mọi
  nơi khác hiển thị ngày trong ứng dụng. Đây là điểm THIẾU NHẤT QUÁN rõ ràng nhất về định dạng
  ngày tháng phát hiện được trong toàn bộ đợt kiểm thử — người dùng quen gõ dd/mm/yyyy ở các màn
  hình khác có thể bị nhầm khi gặp mm/dd/yyyy ở đúng màn hình này.
- Không xác nhận ghi thật (sẽ tạo 4 đợt bí tích mới + 6 bản ghi chi tiết thật) — bấm "Huỷ" sau
  khi xem trước, không tạo ra dữ liệu mới.

## Tổng kết nhóm 3

**Nghiệp vụ/số liệu**: rất tốt — mọi con số kiểm tra được (1925, 90, 0, 1114, 1429/4/6) đều khớp
đúng hoặc khớp đúng tinh thần với bằng chứng đã ghi trong spec, kể cả một bug cố ý migrate y hệt
từ bản gốc (Giới trẻ luôn ra 0). Không phát hiện sai lệch nghiệp vụ mới.

**UX/consistency — 2 phát hiện đáng chú ý mới**:
1. **Biểu đồ hiện trống khi mới mở tab** (tự vẽ trước khi có kích thước) — lỗi kỹ thuật thật, dễ
   khiến người dùng tưởng tính năng hỏng dù chỉ cần bấm lại "Xem" là thấy ngay.
2. **"Tạo danh sách bí tích tự động" dùng input ngày kiểu Mỹ (mm/dd/yyyy) và hiển thị ngày ISO
   trong bảng kết quả** — lệch hẳn khỏi chuẩn ngày Việt Nam (dd/mm/yyyy) dùng nhất quán ở toàn bộ
   phần còn lại của ứng dụng, kể cả các công cụ dữ liệu khác cùng nhóm (Chuyển họ, Chuẩn hoá đều
   không có ô ngày nên không lộ vấn đề này, chỉ riêng công cụ này có 2 ô ngày).

**Đề xuất ưu tiên**:
1. **Trung bình** — sửa "Biểu đồ" để không tự vẽ lúc mở (khớp đúng hành vi desktop: chỉ vẽ sau
   khi bấm Xem), hoặc nếu giữ tự vẽ thì phải đảm bảo canvas đã có kích thước đúng trước khi vẽ.
2. **Trung bình** — đổi 2 ô ngày ở "Tạo danh sách bí tích tự động" sang dùng chung control
   `GxDate` như mọi nơi khác, và hiển thị ngày dd/mm/yyyy trong bảng xem trước thay vì ISO.
3. **Thấp** — sửa lỗi thiếu khoảng trắng "giáo dâncủa" ở mô tả Chuẩn hoá dữ liệu.

Chưa test trong lượt này: Thống kê ơn gọi tận hiến (tab phụ), Kiểm tra dữ liệu — gia đình (đã có
bằng chứng số liệu khớp trong spec, chưa tự kiểm lại qua trình duyệt), Hồ sơ lưu trữ (giáo dân +
gia đình), Tìm và thay thế, 13 mẫu in ấn.
