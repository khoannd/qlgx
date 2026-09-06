# Những chỗ cần review sau

## Quyết định chi phối (người dùng chốt ngày 2026-09-06)

> *"hãy cứ migrate logic hoàn toàn giống app hiện tại rồi note lại để review sau"*

Nghĩa là: bản web **tái hiện trung thành** hành vi bản desktop, **kể cả những chỗ sai hoặc kỳ
quặc**. Không được âm thầm "sửa cho đúng" trong lúc migrate — người dùng đã quen bản cũ, và
một thay đổi tưởng là sửa lỗi có thể phá vỡ quy trình làm việc thật của giáo xứ.

Mọi chỗ như vậy phải:
1. Được migrate **giống hệt**.
2. Được ghi vào file này, kèm trích dẫn dòng mã bản desktop và vị trí tương ứng ở bản web.
3. Chờ người dùng review và quyết định sau.

**Đừng xoá mục nào khỏi file này nếu người dùng chưa quyết.**

---

## Danh sách chờ review

### 1. `isValidDateInputRelations` chỉ kiểm tra 1 trong 4 mốc ngày

- **Bản desktop**: `Source/GXControl/frmGiaoDan.cs:476-479`
- **Hiện tượng**: thông báo lỗi nói kiểm tra quan hệ giữa Ngày sinh, Ngày rửa tội, Ngày rước lễ
  và Ngày thêm sức, nhưng mã chỉ thực sự so **Ngày sinh với Ngày rửa tội**. Hai tham số
  `ngayRuocLe` và `ngayThemSuc` được truyền vào nhưng **không bao giờ được dùng** (lỗi copy-paste).
- **Hệ quả thật**: giáo dân có thể được nhập ngày rước lễ **trước** ngày sinh mà không bị chặn.
- **Bản web phải làm**: giống hệt — chỉ kiểm tra Ngày sinh vs Ngày rửa tội.
- **Câu hỏi cho người dùng**: có muốn bản web kiểm tra đủ cả 4 mốc không? Nếu có thì dữ liệu cũ
  đang sai sẽ bắt đầu báo lỗi khi mở ra sửa — cần quyết cách xử lý dữ liệu cũ.

### 2. Nhánh "tự xoá giáo dân khỏi gia đình cũ" bị comment

- **Bản desktop**: `Source/GXControl/frmGiaDinh.cs:1085-1089`
- **Hiện tượng**: khi thêm một giáo dân đã thuộc gia đình khác, chương trình hỏi *"Bạn có muốn
  chương trình tự xóa giáo dân [X] ra khỏi gia đình [Y] không?"*. Chọn **Yes** thì nhánh xoá
  **đã bị comment lại**, nên thực tế **không xoá gì cả** mà vẫn cho thêm.
- **Hệ quả thật**: giáo dân có thể thuộc **hai gia đình cùng lúc**. Đây là nguồn gốc của lỗi
  `ThuocNhieuGiaDinh` mà chính bản desktop có chức năng "Kiểm tra dữ liệu" để dò ra.
- **Bản web phải làm**: giống hệt — hỏi rồi vẫn cho thêm, không xoá.
- **Câu hỏi cho người dùng**: đây gần như chắc chắn là lỗi. Có muốn bản web thật sự xoá khỏi gia
  đình cũ khi chọn Yes không?

### 3. Cây quyết định `NguoiCu` khi đổi vợ/chồng

- **Bản desktop**: `Source/GXControl/frmGiaDinh.cs:649-919` (đoạn phức tạp nhất màn hình)
- **Hiện tượng**: khi thay Người nam/Người nữ, chương trình hỏi liên tiếp nhiều hộp thoại Yes/No
  để **đoán** vai trò mới của người cũ (cha / mẹ / ông bà / chưa rõ), qua nhiều tầng `if` lồng nhau.
- **Bản web phải làm**: giống hệt, kể cả trình tự và nội dung từng hộp thoại.
- **Câu hỏi cho người dùng**: có muốn rút gọn còn **một** câu hỏi rõ ràng ("Chuyển [X] xuống danh
  sách thành viên hay xoá khỏi gia đình?") rồi để người dùng tự chọn vai trò trong lưới không?

### 4. Lỗi chọn người làm đóng luôn cả form

- **Bản desktop**: `Source/GXControl/frmGiaDinh.cs:540, 609`
- **Hiện tượng**: khi có ngoại lệ lúc chọn Người nam/Người nữ, chương trình hiện *"Lỗi chọn người
  nam"* / *"Lỗi chọn người nữ"* rồi **đóng cả màn hình nhập liệu** → mất toàn bộ dữ liệu đang gõ dở.
- **Bản web phải làm**: giống hệt.
- **Câu hỏi cho người dùng**: bản web nên chỉ báo lỗi tại chỗ và giữ nguyên form không? Trên web
  việc mất dữ liệu đang gõ còn khó chịu hơn desktop.

### 5. Xoá thành viên gia đình là xoá vĩnh viễn

- **Bản desktop**: `Source/GXControl/frmGiaDinh.cs:1198`
- **Hiện tượng**: xoá một thành viên khỏi lưới là `DELETE` thật khỏi bảng `ThanhVienGiaDinh`,
  **không phải xoá mềm**, dù các bảng khác đều dùng cờ `DaXoa`.
- **Bản web phải làm**: giống hệt.
- **Câu hỏi cho người dùng**: có muốn chuyển sang xoá mềm để khôi phục được không?

### 6. Nút "Xem lịch sử chuyển xứ" bị ẩn hẳn

- **Bản desktop**: `frmGiaoDan.cs` — nút tồn tại nhưng `Visible = false`.
- **Bản web phải làm**: **không** hiện nút này.
- **Câu hỏi cho người dùng**: bảng `ChuyenXu` đã được chuyển sang PostgreSQL đầy đủ. Có muốn bật
  lại chức năng xem lịch sử chuyển xứ trên bản web không?

### 7. Bản desktop không gạch ngang dòng nào trên lưới giáo dân

- **Bản desktop**: `GxGiaoDanList.FormattingRow` để trống.
- **Ghi chú**: khác với lưới **gia đình** (có gạch ngang đỏ cho người đã qua đời/chuyển xứ).
  Bản mẫu giao diện web đã duyệt có gạch ngang ở lưới gia đình — đúng. Lưới giáo dân thì không.
- **Bản web phải làm**: giữ đúng như vậy.

### 8. Tooltip gán nhầm nút

- **Bản desktop**: `frmGiaDinh.cs` — một tooltip có vẻ được gán cho nút khác với ý nghĩa của nó.
  Chưa xác minh chắc chắn, xem mục "Chỗ chưa chắc" trong `gia-dinh-chi-tiet.md`.

### 9. `frmHonPhoi.GetHonPhoi(maGiaoDan)` lấy bừa một bản ghi hôn phối khi người có nhiều hơn 1

- **Bản desktop**: `Source/GXControl/frmHonPhoi.cs:384-395`.
- **Hiện tượng**: `SELECT MaHonPhoi FROM GiaoDanHonPhoi WHERE MaGiaoDan=?` không `ORDER BY` gì,
  lấy `Rows[0]` — nếu người này có nhiều hôn phối (goá rồi tái hôn), kết quả phụ thuộc thứ tự vật
  lý CSDL trả về, không đảm bảo là hôn phối mới nhất/hiện tại.
- **Bản web đã làm khác (cố ý, không phải bug tương tự)**: `GET /api/giao-dan/{id}/hon-phoi` trả
  **toàn bộ danh sách** thay vì đoán 1 bản ghi — xem `hon-phoi.md` mục 8. Ghi vào đây vì đây là
  chỗ bản web **cố ý không migrate y hệt** hành vi lấy-bừa của desktop.
- **Câu hỏi cho người dùng**: xác nhận cách tiếp cận "hiện danh sách" là đúng ý muốn, không cần
  quay lại hành vi "một bản ghi" của desktop.

### 10. `GxHonPhoiGiaDinh.checkInput` im lặng không lưu khi Thêm mới mà để trống hết

- **Bản desktop**: `Source/GXControl/GxHonPhoiGiaDinh.cs:271-298`.
- **Hiện tượng**: nếu đang **Thêm mới** hôn phối (chưa từng có) và người dùng để trống tất cả các
  trường, hàm trả `false` (không lưu) mà **không báo lỗi gì** cho người dùng biết vì sao form gia
  đình không lưu được phần hôn phối. Chỉ khi đang **Sửa** một hôn phối đã có mới báo lỗi "Hãy nhập
  ít nhất một thông tin hôn phối".
- **Bản web phải làm**: `GiaDinhService.GhiHonPhoi` hiện KHÔNG có validate "không được để trống
  hết" ở cả hai trường hợp Thêm/Sửa — tức là bản web hiện **lỏng hơn** desktop (cho lưu bản ghi
  hôn phối trống hoàn toàn ở cả hai trường hợp). Task 15 giữ nguyên endpoint hôn phối theo giáo
  dân (`PUT /api/giao-dan/hon-phoi/{honPhoiId}`) không thêm validate này, để nhất quán với endpoint
  gia đình đã có từ trước — không tự ý thêm validate riêng cho một trong hai endpoint.
- **Câu hỏi cho người dùng**: có muốn thêm lại validate "không được để trống hết khi Sửa" cho cả
  hai endpoint (gia đình lẫn giáo dân) không?

### 11. `SoThuTu` của bảng nối `GiaoDanHonPhoi` được cài đặt khác nhau ở hai nơi lưu

- **Bản desktop**: `Source/GXControl/frmHonPhoi.cs:333-343` (không gán `SoThuTu`, để mặc định)
  so với `Source/GXControl/GxHonPhoiGiaDinh.cs:238-250` (gán `GxHonPhoi.GetNextSoThuTu(mã)`).
- **Hiện tượng**: cùng một bảng `GiaoDanHonPhoi`, hai nơi lưu trong bản desktop tính cột `SoThuTu`
  khác nhau — một nơi bỏ trống/mặc định, một nơi tính số thứ tự hôn phối của người đó.
- **Bản web đã làm**: `GiaDinhService.GhiHonPhoi` luôn gán `SoThuTu` theo thứ tự chèn trong cùng
  một hôn phối (1 cho chồng, 2 cho vợ — xem `GiaDinhService.cs:172-178`), không theo ý nghĩa
  "số thứ tự hôn phối thứ mấy của người này" như `GxHonPhoiGiaDinh.GetNextSoThuTu`. Task 15 không
  đọc lại cột này khi hiển thị (front-end không hiện `SoThuTu`) nên chưa phát sinh sai lệch quan
  sát được, nhưng cần lưu ý nếu sau này có báo cáo/thống kê dựa vào cột này.
- **Câu hỏi cho người dùng**: `SoThuTu` có ý nghĩa nghiệp vụ cụ thể nào cần bản web tính đúng
  không, hay chỉ là cột phụ trợ nội bộ của Access?

### 12. Danh sách "Tình trạng hôn phối" khác nhau giữa `frmHonPhoi` và `GxHonPhoiGiaDinh`

- **Bản desktop**: `Source/GXControl/frmHonPhoi.cs:53-58` (6 giá trị: rỗng, Hợp pháp, Hợp thức
  hóa, Chuẩn, Không theo phép đạo, Không xác định) so với `Source/GXControl/GxCachThucHonPhoi.cs`
  dùng trong `GxHonPhoiGiaDinh` (9 giá trị: thêm Ly thân, Ly dị, Đã được tháo gỡ).
- **Bản web đã làm (cố ý)**: dùng danh sách 9 giá trị đầy đủ hơn (`hon-phoi.md` mục 8), vì màn
  hình web migrate theo luồng `GxHonPhoiGiaDinh`.
- **Câu hỏi cho người dùng**: xác nhận dùng danh sách 9 giá trị là đúng ý muốn.

---

## Ghi chú của người dùng cần xử lý

### A. Tab Hôn phối trong màn hình chi tiết giáo dân chưa hoạt động

- **Người dùng báo ngày 2026-09-06**: *"tab hôn phối trong màn hình chi tiết giáo dân chưa work"*.
- **Nguyên nhân đã xác định**: đúng như người dùng phỏng đoán — phần hôn phối **chưa làm xong**.
  Backend đã có logic chọn hôn phối hiện tại (`GiaDinhService.ChonHonPhoiHienTai`) và hai bảng
  `HonPhoi` / `GiaoDanHonPhoi` đã chuyển đủ dữ liệu thật (522 và 1043 dòng), nhưng:
  - Chưa có endpoint đọc/ghi hôn phối theo giáo dân.
  - Tab Hôn phối ở `GiaoDanDetail.tsx` mới chỉ là **khung giao diện tĩnh**, không gọi API.
- **Đây là Task 15**, không phải lỗi hồi quy.
- Cùng tình trạng: tab **Ơn gọi tận hiến** và tab **Hội đoàn** cũng là khung tĩnh.
