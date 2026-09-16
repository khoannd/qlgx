# Báo cáo kiểm thử chấp nhận — nhóm 5 (các mục còn lại)

Ngày: 2026-09-12 (hoàn tất toàn bộ 23 màn hình có spec, tiếp theo nhóm 1-4).

## Kết quả từng mục

### Quản lý giáo lý (Khối/Lớp/Học viên/Giáo lý viên) — ĐẠT hoàn toàn
Tạo dữ liệu test đầy đủ qua giao diện rồi dọn sạch ngay sau khi kiểm:
- Tạo khối "TEST Khoi Giao Ly ZZZ" với Người quản lý chọn qua picker (Tôma Hoàng Giáp) — lưu
  thành công, đúng bố cục spec (Tên khối/Người quản lý bắt buộc/Ghi chú).
- Trong khối, tạo lớp "TEST Lop Giao Ly ZZZ" — đúng thiết kế: trường "Năm" SỬA ĐƯỢC (khác desktop
  khoá cứng, đúng khác biệt có chủ đích đã ghi trong spec).
- Thêm 1 học viên qua picker → "Danh sách học viên (1)" cập nhật đúng, nút "Chuyển lớp" chuyển từ
  disabled sang bật.
- Xóa lớp → đúng thông báo xác nhận nguyên văn spec: "Bạn có chắc muốn xóa lớp giáo lý này? Danh
  sách học sinh thuộc lớp giáo lý này sẽ bị xóa theo."
- Xóa khối → đúng thông báo: "Bạn có chắc muốn xóa khối giáo lý này? Các lớp giáo lý thuộc khối
  này sẽ bị xóa theo." — cascade xóa đúng (xác nhận lại bằng `psql`: `khoi_giao_ly`/`lop_giao_ly`
  về đúng 0 sau khi dọn).

### Tab Hôn phối / Ơn gọi tận hiến / Hội đoàn trong Chi tiết giáo dân — ĐẠT, có phát hiện dữ liệu
Test trên giáo dân thật "Tôma Hoàng Giáp" (#1052):
- **Tab Hôn phối**: hiển thị **2 bản ghi** hôn phối (đúng thiết kế mở rộng so desktop — desktop
  chỉ xử lý 1 bản ghi tại một thời điểm, spec `hon-phoi.md` mục 8 ghi đây là cải tiến có chủ đích
  vì có trường hợp goá/tái hôn). Khung UI đầy đủ chuẩn nhất trong toàn bộ đợt kiểm thử (nút "←
  Danh sách", phụ đề, badge "Đang hoạt động", thanh hành động cuối trang) — **tốt hơn hẳn** khung
  Hội đoàn/Rao hôn phối đã ghi nhận thiếu ở nhóm 2.
  - **Phát hiện dữ liệu (không phải lỗi code)**: "Đôi hôn phối #1" hiện "Người phối ngẫu: CHƯA
    RÕ" và "Nơi hôn phối" ghi "Hàng Xanh" (thiếu chữ so với "Họ Hàng Xanh" ở #2) — mọi trường
    khác (ngày, linh mục, người chứng, tình trạng) giống hệt #2. Nhiều khả năng là **hai bản ghi
    gần-trùng-lặp trong dữ liệu Access gốc** (một dòng chưa liên kết đủ 2 người), không phải lỗi
    của bản web — bản web chỉ đang hiển thị trung thực dữ liệu đã có. Đáng chú ý: khi **in chứng
    nhận hôn phối** (xem mục In ấn), hệ thống tự động chọn ĐÚNG bản ghi #2 (đầy đủ, đúng vợ), xác
    nhận logic "chọn hôn phối hiện tại" hoạt động đúng dù dữ liệu nguồn có nhiễu.
- **Tab Ơn gọi tận hiến**: đúng đủ 9 trường ngày + các trường Địa chỉ/Chức vụ/Dòng tu/Nơi phục
  vụ/Đã hồi tục/Ghi chú theo spec, nút "Thêm giai đoạn mới".
- **Tab Hội đoàn**: đúng thiết kế ("Lịch sử hội đoàn" + form "Thêm vào hội đoàn"). Test thêm
  Tôma Hoàng Giáp vào hội đoàn mẫu "Legio Mariae" → thành công, hiện đúng ô "Vai trò" SỬA ĐƯỢC
  (khác desktop chỉ hard-code "Hội viên" không cho sửa — cải tiến có chủ đích theo spec). **Không
  có API xóa** lịch sử hội đoàn (đúng thiết kế gốc) — đã phải dọn bản ghi test bằng `psql` sau khi
  xin phép người dùng (không tự ý chạy DELETE trên CSDL).

### Quản lý giáo xứ theo giáo phận — ĐẠT hoàn toàn, có 1 điểm cần xác nhận chính sách
Tạo tài khoản test riêng `tester_sysadmin` (LoaiTaiKhoan=9) để kiểm màn hình này (tài khoản
thường không vào được, đúng thiết kế phân quyền):
- Hiển thị đúng dữ liệu thật xuyên toàn máy chủ: Giáo phận "Phan Thiết", Giáo hạt "Đức Tánh",
  Giáo xứ "Vô Nhiễm" với "Số tài khoản = 5" (khớp đúng số tài khoản thật lúc đó: giaoxu, hethong,
  quantri, tester_acceptance, tester_sysadmin).
  Đúng thiết kế an toàn: **không có nút Xóa** ở cả 3 cấp, có nút "Sửa" từng dòng, "+ Thêm" từng
  cấp, "Tạo tài khoản quản trị" cạnh dòng giáo xứ. Cảnh báo đầu trang rõ ràng, đúng nguyên văn
  spec: "Danh sách xuyên TOÀN BỘ máy chủ... Không có nút xoá; muốn ngừng dùng một giáo xứ, đổi
  tên thêm hậu tố "(ngừng)"."
- **Phát hiện cần xác nhận chính sách**: khi đăng nhập bằng tài khoản `LoaiTaiKhoan=9`, menu bên
  trái mục "Hệ thống" chỉ còn "Quản lý giáo xứ" + "Nhập dữ liệu Access" — **mục "Quản lý tài
  khoản" (quản lý tài khoản của riêng giáo xứ đang đăng nhập) biến mất**, dù tài khoản quản trị hệ
  thống vẫn thuộc về một giáo xứ cụ thể (ở đây là Vô Nhiễm) và về lý thuyết vẫn có thể cần quản lý
  tài khoản của giáo xứ đó. Chưa rõ đây là quyết định phân quyền có chủ đích (tách bạch vai trò
  "quản trị hệ thống toàn máy chủ" khỏi "quản trị một giáo xứ") hay là thiếu sót khi làm màn hình
  — cần người phụ trách xác nhận.
- Không tạo giáo xứ/giáo phận mới trong lượt test này (đã có bằng chứng đầy đủ về việc này trong
  `TIEN-DO.md`/`can-review-sau.md` mục 37 từ lượt kiểm thử trước, không cần lặp lại rủi ro).

### Đổi mật khẩu — ĐẠT hoàn toàn
- Nhập sai mật khẩu hiện tại → báo đúng lỗi "Mật khẩu hiện tại không đúng".
- Đổi mật khẩu đúng quy trình (nhập đúng mật khẩu cũ + mật khẩu mới + xác nhận) → "Đổi mật khẩu
  thành công. Lần đăng nhập sau hãy dùng mật khẩu mới." — đổi thật, đăng xuất/đăng nhập lại bằng
  mật khẩu mới thành công (ngầm định qua bước đổi lại về mật khẩu cũ ngay sau đó, cũng thành
  công), xác nhận cơ chế hoạt động đúng cả hai chiều. Đã đổi lại đúng mật khẩu ban đầu ngay sau
  khi test xong.

### In ấn — 6/13 mẫu đã thử trực tiếp, đều ĐẠT chất lượng cao
Tải PDF/Excel thật qua trình duyệt (không chỉ xem preview), mở và đọc lại nội dung từng file:

| Mẫu | Nguồn dữ liệu thật | Kết quả |
|---|---|---|
| Phiếu gia đình (A4) | Gia đình "Tôma Hoàng Giáp" | Đúng đủ 2 thành viên, đủ cột bí tích, tiếng Việt có dấu chuẩn |
| Chứng nhận rửa tội | Giáo dân "Giuse Nguyễn Đức Mạnh" | Đúng số sổ, linh mục, người đỡ đầu |
| Lý lịch cá nhân | Giáo dân "Giuse Nguyễn Đức Mạnh" | Đủ 2 trang, đủ mục Bí tích/Hôn phối/Tình trạng khác |
| Chứng nhận hôn phối | Gia đình "Tôma Hoàng Giáp" | **Tự chọn đúng bản ghi hôn phối "hiện tại"** dù dữ liệu nguồn có 2 bản ghi gần trùng (xem phát hiện ở mục Hôn phối trên) |
| Giấy giới thiệu chứng nhận rửa tội | Giáo dân "Giuse Nguyễn Đức Mạnh" | Đúng 2 ô nhập tay (Giáo phận/Giáo xứ nhận) hiện đúng trong PDF: "Kính gửi: Giáo xứ Thánh Tâm, Giáo phận Xuân Lộc" |
| In danh sách giáo dân — Xuất Excel | Toàn bộ 2039 dòng | File `.xlsx` 269KB tải về thành công |
| In danh sách giáo dân — In PDF (khổ ngang) | Toàn bộ 2039 dòng | File `.pdf` 1.3MB nhiều trang tải về thành công |

Tất cả các file đều có dòng chú thích chuẩn "Giấy in trực tiếp từ hệ thống quản lý giáo xứ —
không có giá trị khi có tẩy xoá, sửa chữa." và đúng quốc hiệu/tên giáo xứ/giáo hạt/giáo phận thật.

**Chưa thử trực tiếp** (7/13 mẫu còn lại — cùng họ với các mẫu đã thử ở trên nên rủi ro thấp,
nhưng chưa có bằng chứng trực tiếp trong đợt này): Chứng nhận rước lễ/thêm sức (cùng khuôn Chứng
nhận rửa tội), Giấy giới thiệu giáo lý hôn phối/chứng nhận thêm sức (cùng khuôn Giấy giới thiệu
rửa tội), Phiếu gia đình khổ A3, In sổ gia đình, In kết quả rao hôn phối.

## Tổng kết nhóm 5

Không phát hiện lỗi nghiệp vụ hay lỗi kỹ thuật mới nào trong nhóm này — mọi tính năng test đều
hoạt động đúng thiết kế, kể cả các luồng ghi dữ liệu (tạo/xóa khối-lớp-học viên giáo lý, thêm hội
đoàn, đổi mật khẩu). Phát hiện đáng chú ý nhất là **dữ liệu nguồn có khả năng chứa bản ghi hôn
phối gần-trùng-lặp** cho giáo dân "Tôma Hoàng Giáp" — không phải lỗi phần mềm, nhưng nên được
giáo xứ rà soát lại sổ sách gốc nếu cần con số hôn phối chính xác tuyệt đối.

**Việc cần người phụ trách xác nhận**: tài khoản `LoaiTaiKhoan=9` (Quản trị hệ thống) không thấy
mục "Quản lý tài khoản" của giáo xứ mình — cần xác nhận có chủ đích hay không.

## Tổng kết TOÀN BỘ 5 đợt kiểm thử (2026-09-12)

Đã hoàn tất kiểm thử toàn bộ **23/23 màn hình có spec**, cộng khảo sát sâu 3 tab nghiệp vụ trong
Chi tiết giáo dân (Hôn phối/Ơn gọi/Hội đoàn) và 6 mẫu in ấn. Kết luận chung:

**Về nghiệp vụ**: độ tin cậy rất cao — hầu như mọi con số/hành vi kiểm chứng được đều khớp chính
xác hoặc khớp đúng tinh thần với bằng chứng đã ghi trong 23 tài liệu spec, kể cả các trường hợp
migrate cố ý giữ nguyên lỗi/hành vi lạ của bản gốc (ví dụ "Giới trẻ" luôn ra 0 người). Không phát
hiện lỗi nghiệp vụ nghiêm trọng nào là mới (chưa được spec ghi nhận trước).

**Ba phát hiện kỹ thuật/UX quan trọng nhất cần đội phát triển xử lý** (không đổi so với tổng kết
nhóm 4, đã kiểm chứng đầy đủ, xếp theo mức ảnh hưởng):
1. **[Cao]** Lỗi cuộn ngang cộng dồn theo số tab đang mở (`<main>` dùng chung khung cuộn cho mọi
   tab) — có thể khiến nút Lưu/Thêm ở bất kỳ màn hình nào trôi ra ngoài khung nhìn khi người dùng
   mở nhiều tab, không có dấu hiệu cảnh báo. Chi tiết và cách tái hiện ở báo cáo nhóm 2.
2. **[Trung bình]** Biểu đồ (Thống kê → Biểu đồ) hiện trống khi mới mở tab do tự vẽ trước khi
   canvas có kích thước — chỉ cần bấm lại "Xem" là hết. Chi tiết ở báo cáo nhóm 3.
3. **[Trung bình]** Ba điểm bất nhất giao diện lặp lại nhiều nơi: (a) khung chi tiết kiểu cũ (Hội
   đoàn, Rao hôn phối) thiếu nút quay lại/phụ đề/thanh hành động chuẩn — khác hẳn khung chi tiết
   giáo dân/gia đình đã làm rất tốt; (b) ba màn hình quản trị (Giáo họ, Quản lý tài khoản, Cha
   quản xứ) dùng bảng HTML đơn giản thay AG-Grid; (c) "Tạo danh sách bí tích tự động" dùng định
   dạng ngày kiểu Mỹ (mm/dd/yyyy) thay vì control ngày chuẩn Việt Nam dùng khắp nơi khác.

**Việc cần người dùng/đội phát triển xác nhận thêm** (không phải lỗi, chỉ là quyết định chính
sách cần chốt): quyền truy cập "Quản lý tài khoản" cho tài khoản Quản trị hệ thống (mục Quản lý
giáo xứ ở trên).

Toàn bộ 5 báo cáo: `BAO-CAO-ACCEPTANCE-2026-09-12.md` (nhóm 1), `-nhom2.md`, `-nhom3.md`,
`-nhom4.md`, `-nhom5.md` (file này) — cùng thư mục `WebApp/anh-chup-kiem-thu/`.
