# Báo cáo kiểm thử chấp nhận — nhóm 2 (kèm kiểm tra UX/consistency)

Ngày: 2026-09-12 (tiếp theo báo cáo nhóm lõi `BAO-CAO-ACCEPTANCE-2026-09-12.md`).

**Chuẩn UX dùng để đối chiếu**: màn hình *Danh sách giáo dân* và *Chi tiết giáo dân* (đã kiểm ở
nhóm 1), gồm các đặc điểm:
- Danh sách: heading + số đếm dạng pill, thanh nút (Tải lại/Xuất Excel/Thêm/Xóa[disabled tới khi
  chọn dòng]/In danh sách), lưới AG-Grid có ô lọc riêng từng cột (Filter Input), dòng gợi ý thao
  tác ("Nhấp đúp... chuột phải...").
- Chi tiết: nút "← Danh sách" ở đầu, heading + dòng phụ đề (mã/trạng thái tóm tắt) + badge trạng
  thái (Đang hoạt động/Bản nháp), nội dung chia khối có tiêu đề phụ, thanh hành động CUỐI TRANG
  cố định gồm: trạng thái lưu ("Chưa có thay đổi"/"Bản nháp chưa lưu") + nút phụ + "Quay về" +
  nút chính (Cập nhật/Thêm).

## Phạm vi đã test lần này

Danh sách hội đoàn + chi tiết hội đoàn, Danh sách rao hôn phối + thêm đôi rao, Giáo họ, Quản lý
giáo lý (mức danh sách).

## PHÁT HIỆN QUAN TRỌNG NHẤT — lỗi hệ thống, không phải lỗi riêng từng màn hình

**Vùng nội dung chính (`<main class="work glass">`) dùng chung một khung cuộn ngang cho MỌI tab
đang mở cùng lúc.** Khi một tab bất kỳ có nội dung rộng (ví dụ lưới 29 cột của Danh sách giáo
dân, hay form "Đôi rao mới" có bố cục 2 cột song song rộng), `scrollWidth` của `<main>` bị đẩy
lên theo tab rộng nhất — và mọi tab KHÁC đang mở cùng lúc (kể cả tab có nội dung vốn dĩ vừa khít
màn hình như "Giáo họ") đều bị kéo theo, đẩy các nút/cột nằm bên phải ra NGOÀI khung nhìn, người
dùng phải tự biết cuộn ngang mới thấy.

**Bằng chứng đã đo trực tiếp (Playwright, không suy đoán):**
- Với 12 tab đang mở, nút "+ Thêm giáo họ" ở tab "Giáo họ" nằm tại toạ độ x=2450 trong khi khung
  nhìn chỉ rộng 1440px (`main.scrollWidth`=2328 / `clientWidth`=1150) — **nút hoàn toàn không
  nhìn thấy được** nếu không cuộn ngang thêm hơn 1000px.
- Nút "Cập nhật" ở form "Đôi rao mới" nằm tại x=2384, ngoài khung nhìn dù đã thử phóng cửa sổ
  trình duyệt lên tới 2000px — **người dùng có thể không tìm ra được nút lưu**.
- **Đã xác nhận nguyên nhân bằng thực nghiệm loại trừ**: bấm "Đóng tất cả" (chỉ còn 1-2 tab mở)
  → `main.scrollWidth` trở về đúng bằng `clientWidth` (1150=1150), nút "+ Thêm giáo họ" quay về
  đúng vị trí bình thường (x=1272, nằm gọn trong khung nhìn). Xác nhận 100% đây là lỗi cộng dồn
  theo SỐ TAB ĐANG MỞ, không phải lỗi riêng của từng màn hình.

**Vì sao đây là lỗi nghiêm trọng thực tế**: mô hình làm việc của bản web khuyến khích người dùng
mở nhiều tab song song (giống trình duyệt) và ứng dụng KHÔNG tự đóng tab cũ — người dùng thật
(quý cha/quý sơ, phần lớn không rành máy tính, theo đúng tinh thần CLAUDE.md) chắc chắn sẽ tích
luỹ nhiều tab trong một phiên làm việc dài. Khi đó các nút Lưu/Thêm/Cập nhật ở tab đang thao tác
có thể trôi ra ngoài màn hình mà KHÔNG có dấu hiệu gì báo cho người dùng biết (không có thanh
cuộn ngang rõ ràng, không có gợi ý "còn nội dung bên phải") — dễ khiến người dùng tưởng nhầm là
"không lưu được"/"mất nút" và không biết cách xử lý.

**Đề xuất khắc phục** (không tự sửa trong phạm vi kiểm thử, ghi lại để đội phát triển xử lý):
1. Cách đúng nhất: mỗi tab nên có vùng cuộn ngang RIÊNG (mỗi tab-panel tự quản lý `overflow-x`
   của chính nó), không dùng chung một `scrollWidth` cấp `<main>` cho tất cả tab.
2. Nếu giữ kiến trúc hiện tại, tối thiểu cần: đặt `overflow-x: hidden` (hoặc `max-width: 100%`)
   trên phần tử bọc TỪNG tab-panel không hiển thị, để nội dung ẩn không góp phần vào
   `scrollWidth` chung của `<main>`.
3. Rà lại các form có bố cục "2 cột song song" cố định độ rộng lớn (Đôi rao mới, có lẽ cả các
   form khác dùng chung khuôn `GxGroupBox` hai cột) — nên cho phép co giãn/xuống dòng như cách
   Chi tiết giáo dân/gia đình đã làm tốt, thay vì cố định chiều rộng.

## Kết quả từng màn hình

### Danh sách hội đoàn — ĐẠT về danh mục, PHÁT HIỆN về chi tiết
- Danh sách: nhất quán tốt với chuẩn (AG-Grid, filter từng cột, count pill "2 hội đoàn", nút Tải
  lại/Thêm/Xóa). Thiếu nút "Xuất Excel"/"In danh sách" so với chuẩn — chấp nhận được vì spec ghi
  nhận đây là hạn chế hạ tầng chưa làm (không phải lỗi), nhưng vẫn là điểm **kém nhất quán về bộ
  nút hành động** giữa các màn hình danh sách.
- Chi tiết ("Legio Mariae"): **thiếu nút "← Danh sách"**, **thiếu dòng phụ đề + badge trạng
  thái**, **thiếu thanh hành động cố định cuối trang kiểu chuẩn** (nút "Xóa hội đoàn"/"Cập nhật"
  nằm trôi nổi ngay dưới form thay vì trong thanh cố định như Chi tiết giáo dân/gia đình) — cách
  duy nhất để quay lại danh sách là bấm lại tab "Danh sách hội đoàn" hoặc đóng tab hiện tại (×),
  không có lối "Quay về" tường minh như chuẩn.
- Dữ liệu hiển thị đúng (2 hội đoàn mẫu tự tạo trước đó theo TIEN-DO.md, không phải dữ liệu thật
  — đã biết trước, không phải phát hiện mới).
- "Danh sách hội viên" hoạt động đúng, đếm đúng "(1)", hiển thị đúng hội viên mẫu.

### Danh sách rao hôn phối — ĐẠT về danh mục, có phát hiện quan trọng ở form Thêm
- Danh sách: nhất quán tốt với chuẩn (đủ Tải lại/Thêm/Xóa/Xuất Excel, filter từng cột, count pill
  "0 đôi rao", combo "Hiển thị" đúng 2 lựa chọn theo spec).
- Form "Đôi rao mới": bố cục 2 cột song song ("Người thứ nhất"/"Người thứ hai") rộng quá khổ, gây
  ra phát hiện quan trọng nhất ở trên (nút Cập nhật ngoài khung nhìn). Ngoài ra form này cũng
  **thiếu nút "← Danh sách" và thanh hành động cố định** giống nhận xét ở Hội đoàn.
- Chưa test lưu thật (không tạo dữ liệu vì cần chọn đúng 2 giáo dân thật khác giới tính — để dành
  cho lượt sau nếu cần, tránh rủi ro không cần thiết khi nút Lưu khó bấm do lỗi trên).

### Giáo họ — ĐẠT đúng spec (cố ý không có nút Xóa), nhưng cùng lỗi layout hệ thống
- Đúng thiết kế: chỉ 1 giáo họ thật ("Simon Phan Đắc Hòa"), có cột "Mã cũ" (bổ sung có chủ đích
  theo spec), có nút "Sửa" từng dòng, **không có nút Xóa** (đúng quyết định an toàn dữ liệu đã ghi
  trong spec — cascade xóa quá nguy hiểm).
- Dùng bảng `<table>` HTML đơn giản (không phải AG-Grid) — **cùng điểm bất nhất về kiểu lưới** đã
  ghi nhận ở nhóm 1 cho Quản lý tài khoản/Giáo xứ. Ba màn hình này (Giáo họ, Quản lý tài khoản,
  khối "Cha quản xứ" ở Giáo xứ) tạo thành một nhóm phong cách riêng, khác nhóm còn lại — nên ghi
  nhận có chủ đích hoặc thống nhất lại.
- Bị ảnh hưởng bởi lỗi hệ thống ở trên (nút "+ Thêm giáo họ" ra ngoài khung nhìn khi nhiều tab mở).

### Quản lý giáo lý (mức danh sách "Khối giáo lý") — ĐẠT, nhất quán tốt
- Đúng chuẩn AG-Grid, count pill "0 khối giáo lý" (đúng, giáo xứ Vô Nhiễm chưa có dữ liệu giáo
  lý), nút Tải lại/Thêm khối/Xóa[disabled], filter từng cột đầy đủ 5 cột đúng nhãn.
- Chưa kiểm sâu chi tiết một khối/lớp giáo lý trong lượt này (để dành nếu cần).

## Tổng kết nhóm 2

Về **nghiệp vụ** (đúng/sai so với mô tả chức năng gốc): cả 4 màn hình đều đạt yêu cầu cơ bản,
không phát hiện sai lệch nghiệp vụ nghiêm trọng.

Về **UX và tính nhất quán** (trọng tâm được yêu cầu lần này): phát hiện một lỗi **hệ thống** ảnh
hưởng tới mọi màn hình khi người dùng mở nhiều tab (mục "PHÁT HIỆN QUAN TRỌNG NHẤT"), và hai nhóm
bất nhất giao diện lặp lại nhiều nơi:
1. Màn hình chi tiết dạng "hộp thoại cũ" (Hội đoàn, Rao hôn phối) thiếu bộ khung chuẩn (nút quay
   lại, phụ đề trạng thái, thanh hành động cố định) mà Chi tiết giáo dân/gia đình đã làm tốt.
2. Ba màn hình quản trị (Giáo họ, Quản lý tài khoản, Cha quản xứ) dùng bảng HTML đơn giản thay vì
   AG-Grid như phần còn lại của hệ thống.

**Đề xuất ưu tiên xử lý** (theo mức độ ảnh hưởng người dùng thật):
1. **Cao** — sửa lỗi cuộn ngang cộng dồn theo số tab (mục "PHÁT HIỆN QUAN TRỌNG NHẤT"), vì có thể
   khiến người dùng không bấm được nút Lưu/Thêm ở bất kỳ màn hình nào, không riêng 2 màn hình vừa
   test.
2. **Trung bình** — chuẩn hoá khung chi tiết (back button + phụ đề + thanh hành động cố định) cho
   Hội đoàn và Rao hôn phối theo đúng mẫu Chi tiết giáo dân/gia đình.
3. **Thấp** — thống nhất kiểu lưới (bảng đơn giản vs AG-Grid) cho nhóm màn hình quản trị, hoặc ghi
   nhận có chủ đích nếu quyết định giữ khác biệt.

Chưa test trong lượt này: nội dung chi tiết một khối/lớp giáo lý, Thống kê chung, Biểu đồ, 6 công
cụ dữ liệu, Hồ sơ lưu trữ (2), 13 mẫu in.
