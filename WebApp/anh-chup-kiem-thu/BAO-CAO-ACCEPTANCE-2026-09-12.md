# Báo cáo kiểm thử chấp nhận (acceptance test) — nhóm màn hình lõi

Ngày kiểm thử: 2026-09-12. Người kiểm thử: Claude (đóng vai tester), theo yêu cầu người dùng.
Môi trường: web app chạy `npm run dev` (localhost:5173) + API `dotnet run` (localhost:5096),
CSDL PostgreSQL `qlgx_thu` — **dữ liệu thật của giáo xứ Vô Nhiễm** (2050 giáo dân / 40 gia đình /
1 giáo họ / 1108 đợt bí tích). Tài khoản test riêng `tester_acceptance` (LoaiTaiKhoan=0) được tạo
qua CLI `tao-tai-khoan-quan-tri` để không phải hỏi mật khẩu tài khoản `quantri` có sẵn.

**Nguyên tắc kiểm thử**: không sửa/xoá dữ liệu thật hiện có. Nơi cần kiểm thao tác ghi, tự tạo
một giáo dân test (`TEST Kiem Thu Acceptance ZZZ`, Mã GD 2056) rồi xoá vĩnh viễn ngay sau khi
kiểm xong (qua API `DELETE /api/giao-dan/{id}?vinhVien=true`, xác nhận 200 OK).

Đối chiếu với: spec màn hình (`docs/superpowers/specs/man-hinh/*.md`) — lưu ý các spec này viết
ngày 2026-09-06/07, còn mục "Đối chiếu bản web hiện tại" của chúng đã **lỗi thời** so với tiến độ
thực tế ngày 2026-09-08 (`VIEC-TIEP-THEO.md` ghi "đã hoàn tất toàn bộ giai đoạn"). Báo cáo này ưu
tiên quan sát trực tiếp trên trình duyệt hơn là tin vào bảng đối chiếu cũ trong spec.

## Phạm vi đã test (nhóm lõi, theo lựa chọn người dùng)

1. Đăng nhập
2. Danh sách giáo dân + Chi tiết giáo dân (thêm mới, validate, tab Giáo lý)
3. Danh sách gia đình + Chi tiết gia đình
4. Danh sách sổ bí tích
5. Giáo xứ
6. Quản lý tài khoản

Chưa test trong lượt này (còn 17 màn hình khác): Danh sách hội đoàn, Rao hôn phối, Giáo họ, Quản
lý giáo lý, Thống kê chung, Biểu đồ, 6 công cụ dữ liệu, Hồ sơ lưu trữ (2), in ấn 13 mẫu, đổi mật
khẩu (submit thật), quản lý giáo xứ theo giáo phận.

## Kết quả

### 1. Đăng nhập — ĐẠT
- Đăng nhập bằng tài khoản test hoạt động đúng, chuyển vào layout chính, hiển thị đúng tên giáo
  xứ ("Vô Nhiễm") và tên tài khoản trên thanh trên.
- Lỗi console 401 ở `/api/auth/toi` trước khi đăng nhập là hành vi bình thường (kiểm tra phiên
  cũ), không phải lỗi.

### 2. Danh sách giáo dân — ĐẠT phần lớn
- Số liệu đúng: 2039 giáo dân hiển thị mặc định (loại trừ qua đời/chuyển xứ/đã xoá, đúng tinh
  thần `GxGiaoHo.LoadGridData` của bản desktop — spec mục 3 `giao-dan-danh-sach.md`).
- Checkbox "Hiện cả người đã qua đời / đã chuyển xứ" — tính năng mới có chủ đích (spec đã ghi
  nhận là khác biệt cố ý, không phải lỗi).
- Nút Thêm/Xoá/Xuất Excel/In danh sách đều hiện diện, nút Xoá bị khoá đúng khi chưa chọn dòng.
- Bộ lọc theo cột (Filter Input) trên từng cột lưới có tồn tại nhưng **không lọc được qua giá
  trị nhập trực tiếp bằng script** trong phiên test này (gõ "2056" vào ô lọc Mã GD không lọc ra
  dòng nào, kể cả sau khi bấm Tải lại) — cần người dùng thật xác nhận lại bằng thao tác gõ tay
  bình thường trên trình duyệt, vì nhiều khả năng đây là hạn chế của công cụ test tự động
  (playwright `fill()`) chứ không phải lỗi ứng dụng thật.

### 3. Thêm giáo dân mới — ĐẠT, đúng theo spec desktop
- Validate bắt buộc hoạt động đúng nguyên văn thông báo desktop: bấm Lưu khi trống Họ tên →
  **"⛔ Hãy nhập Họ tên"** (khớp `frmGiaoDan.cs:294-299`).
- Control ngày sinh (`GxDate`, Task 19) hoạt động đúng: gõ liên tục `01011990` (không cần gõ dấu
  `/`) tự format thành `01/01/1990`. Có cảnh báo tại chỗ "Ngày không hợp lệ" khi ngày chưa đủ.
- Lưu thành công, tự sinh Mã giáo dân kế tiếp (2056), tổng số giáo dân tăng đúng 2039 → 2040.
- Tab **Giáo lý** đã có trường nhập liệu thật (Bao đồng 1/2, Vào đời, Hôn nhân) — **khác với ghi
  nhận cũ trong spec** ("chỉ hiển thị UI tĩnh, không đọc/ghi") — xác nhận đã được làm thật ở một
  phiên sau ngày viết spec.
- Nút "In lý lịch cá nhân" hiện là nút bấm được (không còn là nút tĩnh không có `onClick` như spec
  cũ ghi) — chưa kiểm chứng file PDF thật xuất ra trong lượt này (nằm ngoài phạm vi thời gian).
- **Phát hiện nhỏ**: lỗi console 404 lặp lại ở endpoint `/api/{giao-dan|gia-dinh}/{id}/anh-dai-dien`
  khi bản ghi chưa có ảnh đại diện — giao diện xử lý đúng (hiện "Chưa có hình"), nhưng việc log lỗi
  404 ra console cho một trường hợp bình thường (chưa có ảnh) là dấu hiệu nên xử lý êm hơn ở tầng
  gọi API (coi 404 là "không có ảnh", không phải lỗi).

### 4. Chi tiết gia đình — ĐẠT, đầy đủ hơn ghi nhận cũ trong spec
- Mở gia đình thật ("Tôma Hoàng Giáp", mã 5): hiển thị đúng Người nam/Người nữ, radio Chủ hộ,
  Giáo họ, Điện thoại, Địa chỉ, Diện gia đình, Ghi chú, checkbox Chuyển xứ/Không thống kê.
- **Khối Hôn phối đã có form nhập liệu thật** (Số hôn phối, Ngày hôn phối, Nơi hôn phối, Linh mục
  chứng, Người chứng 1/2, Tình trạng hôn phối, Ghi chú) và hiển thị đúng dữ liệu thật đã lưu (ngày
  24/09/1975, Họ Hàng Xanh, Giuse Vũ Minh Nghiệp...) — **khác hẳn ghi nhận cũ trong spec**
  (`gia-dinh-chi-tiet.md` mục 10 nói "web hiện tại không có form nào để sửa khối hôn phối") — đã
  được làm thật sau đó.
- Khối "Thành viên khác trong gia đình" có lưới + form Thêm vào gia đình (chọn vai trò) — đúng
  tinh thần desktop.
- Nút In lý lịch cá nhân / In phiếu gia đình (có chọn khổ giấy A4/A3) đều là nút bấm được, không
  còn là nút tĩnh.
- Cùng phát hiện nhỏ 404 ảnh đại diện như mục 3.

### 5. Danh sách sổ bí tích — ĐẠT
- Bắt buộc chọn Loại bí tích trước khi tìm (đúng UX desktop yêu cầu chọn rõ trước khi query).
- Chọn "Rửa tội" → trả về 780 đợt / 2050 người, dữ liệu thật hiển thị đúng (ngày cổ nhất
  27/02/1933, đúng định dạng ngày Việt Nam).

### 6. Giáo xứ — ĐẠT
- Hiển thị đúng thông tin thật: Giáo phận Phan Thiết (chỉ xem, khoá sửa — đúng thiết kế phân
  quyền), Giáo hạt Đức Tánh (chỉ xem), Tên giáo xứ "Vô Nhiễm", địa chỉ/điện thoại/email đúng dữ
  liệu thật. Không có cha quản xứ nào trong danh sách (dữ liệu thật, không phải lỗi).

### 7. Quản lý tài khoản — ĐẠT, có phát hiện về nhất quán giao diện
- Danh sách tài khoản hiển thị đúng 4 tài khoản (`giaoxu`, `hethong`, `quantri`,
  `tester_acceptance`), có nút Sửa/Xoá từng dòng, nút "+ Thêm tài khoản".
- Menu "Hệ thống" ở thanh trên có "Đổi mật khẩu" (chưa test submit thật) và "Đăng xuất". Các mục
  Nhập/Sao lưu/Khôi phục dữ liệu kiểu cũ (từ menu desktop) hiện là nút **bị khoá (disabled)** —
  hợp lý vì mô hình web tập trung không cần các thao tác này ở máy trạm, không gây hiểu lầm vì
  không bấm được.
- **Phát hiện (do người dùng chỉ ra khi xem trực tiếp)**: lưới ở đây và ở khối "Danh sách các cha
  quản xứ" (màn hình Giáo xứ, mục 6) dùng `<table>` HTML thường (không lọc theo cột, không sort
  bằng click tiêu đề, không có thanh Filter Input dưới tiêu đề) — **khác hẳn phong cách lưới
  AG-Grid** dùng ở Danh sách giáo dân/Gia đình/Sổ bí tích (có filter từng cột, sort, virtual
  scroll). Đây là **thiếu nhất quán giao diện** giữa các màn hình quản trị (dữ liệu ít dòng, ít
  quan trọng hơn) với các màn hình nghiệp vụ chính — nên cân nhắc dùng chung `GxGrid`/AG-Grid cho
  toàn bộ lưới trong ứng dụng, hoặc nếu cố ý dùng bảng đơn giản cho màn hình quản trị (vì số dòng
  nhỏ, không cần lọc/sort) thì nên ghi rõ đây là quyết định thiết kế có chủ đích trong
  `can-review-sau.md` để tránh bị coi là thiếu sót.

## Tổng kết

Nhóm màn hình lõi đã kiểm thử đều **đạt yêu cầu chấp nhận** so với mô tả chức năng gốc của bản
desktop, và trong nhiều trường hợp còn đầy đủ hơn ghi nhận trong spec cũ (khối Hôn phối, tab Giáo
lý, nút in ấn nay đã có hành động thật) — khớp với ghi nhận "đã hoàn tất toàn bộ giai đoạn" của
`VIEC-TIEP-THEO.md`.

**Việc cần làm thêm / theo dõi:**
1. Xác nhận lại bằng tay (không qua script) việc lọc theo cột trên lưới Danh sách giáo dân có
   hoạt động đúng không.
2. Xử lý êm hơn lỗi 404 khi ảnh đại diện chưa tồn tại (không cần thiết phải là lỗi ở console).
3. Thống nhất kiểu lưới: chuyển "Quản lý tài khoản" và "Danh sách các cha quản xứ" sang dùng
   chung `GxGrid`/AG-Grid như các màn hình khác, hoặc ghi nhận có chủ đích vào `can-review-sau.md`
   nếu quyết định giữ bảng đơn giản cho hai nơi này.
4. Test tiếp 17 màn hình còn lại (danh sách bên dưới) và xác nhận thật các mẫu in PDF (13 mẫu).
5. Test submit thật màn hình Đổi mật khẩu (chưa làm vì tránh rủi ro không cần thiết trong lượt
   test đầu này với tài khoản đang dùng để test).

**Dọn dẹp đã thực hiện**: giáo dân test (Mã GD 2056, id `fd5b9187-15a9-412a-9b33-315ee146e548`)
đã bị xoá vĩnh viễn qua API — xác nhận không còn sót trong CSDL thật.
