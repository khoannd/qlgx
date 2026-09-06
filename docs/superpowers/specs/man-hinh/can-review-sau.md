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
