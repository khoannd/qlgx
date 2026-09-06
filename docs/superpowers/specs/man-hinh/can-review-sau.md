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

### 8. Tooltip gán nhầm nút — ĐÃ XÁC NHẬN (2026-09-07)

- **Bản desktop**: `Source/GXControl/GXAddEdit.Designer.cs` (tệp UTF-16, đọc bằng
  `python -c "open(..., encoding='utf-16').read()"` vì công cụ text thường không đọc được).
  Toàn bộ tooltip của thanh nút `GxAddEdit` (`toolTip1.SetToolTip`, dòng 67/105/131/156/181/
  208/234):

  | Nút | Dòng | Tooltip nguyên văn |
  |---|---|---|
  | `btnMap` | 67 | `"In danh sách trên lưới"` |
  | `btnEdit` | 105 | `"Sửa"` |
  | `btnNew` | 131 | `"Thêm"` |
  | `btnDelete` | 156 | `"Loại bỏ khỏi danh sách trên lưới"` |
  | `btnReload` | 181 | `"Lấy lại dữ liệu trong chương trình và hiện lên lưới như khi chưa thực hiện tìm ki…"` (chuỗi nối dòng, bị cắt trong Designer) |
  | `btnPrint` | 208 | `"In danh sách trên lưới"` |
  | `btnSelect` | 234 | `"Chọn"` |

- **Xác nhận**: `btnMap` (dòng 67) và `btnPrint` (dòng 208) có **CÙNG MỘT chuỗi tooltip y hệt**
  `"In danh sách trên lưới"` — không phải suy đoán nữa, đây đúng là tooltip gán nhầm (rất có
  thể copy-paste khi thêm `btnMap` sau `btnPrint`, ý nghĩa hai nút chức năng khác nhau —
  `btnMap` dùng ảnh `map.ico`/phím tắt `&M`, hẳn là "Xem vị trí" chứ không phải "In danh sách").
- **Bản web đã làm**: không tái hiện lỗi này ở thanh công cụ `GxToolbar` mới (mục 4 vòng 2)
  — mỗi nút có tooltip riêng đúng chức năng: "Thêm", "Loại bỏ khỏi danh sách trên lưới", "Lấy
  lại dữ liệu…", "In danh sách trên lưới" (chỉ nút In dùng tooltip này, đúng nghĩa). Menu chuột
  phải mục "Xem vị trí" (tương ứng `btnMap`) vẫn hiện thông báo "chưa hỗ trợ" giống các mục in
  ấn khác, không gán nhầm tooltip.
- **Câu hỏi cho người dùng**: không còn — mục này đã xác nhận là lỗi bản desktop, bản web migrate
  đúng tinh thần từng nút (không tái hiện lỗi copy-paste tooltip vì đây thuộc UI mới hoàn toàn,
  không phải hành vi nghiệp vụ cần giữ nguyên).

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

### 13. `GxTanHien.checkInput` hỏi "đã có gia đình" trước khi lưu Ơn gọi tận hiến

- **Bản desktop**: `Source/GXControl/GxTanHien.cs:238-262`.
- **Hiện tượng**: nếu giáo dân đang xem có `DaCoGiaDinh==true` và người dùng có dữ liệu để lưu ở
  tab Ơn gọi tận hiến, chương trình hỏi *"Giáo dân này đã có gia đình. Bạn có chắc muốn nhập
  thông tin tận hiến cho giáo dân này không?"* (YesNoCancel). Chọn No thì `Clear()` toàn bộ form
  rồi không lưu; Cancel thì không lưu nhưng giữ nguyên dữ liệu đã gõ; Yes (ngầm định) thì lưu
  bình thường.
- **Bản web**: chưa migrate hộp thoại này ở lần này — xem `tan-hien.md` mục 8/9.
- **Câu hỏi cho người dùng**: có muốn giữ lại cảnh báo "mâu thuẫn dữ liệu" này trên web không
  (dạng banner/toast thay vì hộp thoại chặn), hay bỏ hẳn vì không rõ ý đồ gốc?

### 14. `GxTanHien.UpdateData` xoá cứng bản ghi khi để trống hết form

- **Bản desktop**: `Source/GXControl/GxTanHien.cs:187-197` (nhánh `isNull()` → `Rows[0].Delete()`).
- **Hiện tượng**: đã có bản ghi `TanHien`, người dùng xoá hết nội dung mọi ô trên tab Ơn gọi tận
  hiến rồi bấm "Cập nhật" ở form giáo dân → toàn bộ bản ghi `TanHien` bị XOÁ VĨNH VIỄN khỏi CSDL,
  không có xác nhận, không phải xoá mềm (bảng `TanHien` không có cột `DaXoa`).
- **Bản web**: KHÔNG migrate hành vi này — xoá một bản ghi tận hiến trên web phải qua thao tác
  xoá rõ ràng, không suy luận ngầm từ "để trống rồi lưu". Xem `tan-hien.md` mục 4/8.
- **Câu hỏi cho người dùng**: xác nhận cách tiếp cận "không tự xoá ngầm" là đúng ý muốn.

### 15. `GxTanHien.isNull()` bỏ qua `chkHoiTuc` (Đã hồi tục) khi xét "form có trống không"

- **Bản desktop**: `Source/GXControl/GxTanHien.cs:264-289` — điều kiện rỗng liệt kê 16 ô nhưng
  không có `chkHoiTuc`.
- **Hiện tượng**: nếu người dùng CHỈ tick "Đã hồi tục" mà không nhập ô nào khác, `isNull()` vẫn
  trả `true` (coi là rỗng) → theo logic `UpdateData()`, giá trị tick đó không bao giờ được lưu
  thành một bản ghi mới, và nếu đã có bản ghi thì nhánh xoá cứng (mục 14) còn có thể kích hoạt
  nhầm dù người dùng vừa tick "Đã hồi tục" — mất thao tác của người dùng một cách âm thầm.
- **Bản web**: coi `chkHoiTuc`/checkbox tương ứng là một giá trị có ý nghĩa như mọi trường khác.
  Xem `tan-hien.md` mục 4/8.
- **Câu hỏi cho người dùng**: xác nhận đây là lỗi cần sửa (không phải hành vi cố ý).

### 16. `GxTanHien.UpdateData` gọi `GetNextId` hai lần cho cùng một lần lưu

- **Bản desktop**: `Source/GXControl/GxTanHien.cs:99` (lúc tải, `AssignControlData`) và dòng 203
  (lúc lưu, `UpdateData`) — cùng gọi `Memory.Instance.GetNextId(TanHienConst.TableName,
  TanHienConst.MaTanHien, false)` nhưng ở hai thời điểm khác nhau.
- **Hiện tượng**: mã tận hiến được sinh ngay khi MỞ form (dù người dùng có lưu hay không), rồi
  sinh LẦN NỮA khi thật sự lưu — không có gì đảm bảo hai lần gọi trả cùng một giá trị nếu có giao
  dịch khác (mở form khác, thao tác khác) xen giữa hai lần gọi.
- **Bản web**: sinh Id bằng `Guid.NewGuid()` tiêu chuẩn của `ThucTheCoSo`, không có khái niệm
  "sinh mã kế tiếp" kiểu số nguyên tăng dần → vấn đề này không tái hiện được (và không cần tái
  hiện) trên bản web. Ghi lại để biết vì sao `MaTanHienCu` (mã cũ chuyển từ Access) có thể có
  khoảng trống/không liên tục nếu dữ liệu gốc từng gặp tình huống này.

### 17. `GxHistoryHoiDoan.UpdateHoiDoan` dùng Yes/No ngược trực giác ở nhiều bước, không nhất quán

- **Bản desktop**: `Source/GXControl/GxHistoryHoiDoan.cs:129-258` — xem chi tiết từng bước ở
  `hoi-doan.md` mục 4. Tóm tắt: bước 1, 2, 4, 5 dùng "Yes → không lưu, ở lại sửa" / "No → không
  lưu, đóng khối nhập" (hoặc `true` = coi như xong dù chẳng lưu gì); bước 6, 7 lại ĐẢO NGƯỢC:
  "No → không lưu, ở lại sửa" / "Yes → không lưu, đóng khối nhập". Không có bước nào trong cả 7
  bước mà "Yes" nghĩa là "lưu luôn" — toàn bộ nhánh Yes/No trong hàm này đều dẫn tới KHÔNG LƯU,
  chỉ khác nhau ở việc có đóng khối nhập hay không.
- **Bản web**: dùng nút "Huỷ"/"Lưu" tường minh, không dùng hộp thoại xác nhận kiểu Yes/No mập mờ
  cho luồng thêm mới lượt tham gia hội đoàn — xem `hoi-doan.md` mục 8.
- **Câu hỏi cho người dùng**: xác nhận đây là quyết định đúng (bỏ cách hỏi mập mờ, giữ tinh thần
  kiểm tra dữ liệu) — không phải bỏ sót tính năng.

### 18. `GxHistoryHoiDoan` không cho sửa/xoá một lượt tham gia hội đoàn đã có

- **Bản desktop**: `Source/GXControl/GxHistoryHoiDoan.cs:294-296` — `DeleteButton`, `EditButton`,
  `ReloadButton` đều bị ẩn vĩnh viễn; lưới lịch sử (`GxListHistoryHoiDoan`) toàn bộ cột đều
  `EditType.NoEdit`. Chỉ có thể XEM và THÊM MỚI, không sửa/xoá được lượt đã lưu qua tab này.
- **Bản web**: MỞ RỘNG có chủ đích — cho sửa Ngày vào/Ngày ra/Vai trò của một lượt tham gia đã
  có, có RowVersion chống ghi đè. Xem `hoi-doan.md` mục 8.
- **Câu hỏi cho người dùng**: xác nhận việc mở rộng này là mong muốn (không phải lỗi migrate).

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

---

### 19. Tạo/xoá giáo dân qua web — các quy tắc chưa (hoặc không thể) tái hiện đầy đủ

- **Việc đã làm**: `POST /api/giao-dan`, `DELETE /api/giao-dan/{id}` và
  `GiaoDanService.KiemTraNghiepVu` (kiểm tra nghiệp vụ khi tạo/sửa) — xem
  `giao-dan-chi-tiet.md` mục 4 và `giao-dan-danh-sach.md` mục 4.
- **Rule 15 (`CheckTuoiChaMe`, tuổi cha/mẹ so với con) — CHƯA làm được**: quy tắc này cần biết
  Tên Cha/Tên Mẹ là MỘT GIÁO DÂN CÓ SẴN (để tra `NgaySinh` của người đó), nhưng `HoTenCha`/
  `HoTenMe` ở bản web hiện chỉ là ô văn bản tự do (`GxPicker` chỉ hiển thị, chưa có picker chọn
  thật — xem `giao-dan-chi-tiet.md` mục 10, dòng "Tên Cha/Tên Mẹ"). Không có cách nào tra tuổi
  cha/mẹ từ một chuỗi văn bản. Khi nào picker chọn giáo dân thật được làm, cần bổ sung lại quy
  tắc 15 (chặn cứng, "Tuổi phụ huynh phải lớn hơn tuổi của giáo dân ít nhất là 15" —
  `GxConstants.TUOI_CHO_PHEP_CO_CON`).
- **Rule 8 (cảnh báo lệch Giáo họ/"không thống kê") — CHƯA làm**: cùng lý do — Giáo họ ở bản
  web hiện là danh mục tạm hard-code theo TÊN (`data/giaoHoTam.ts`), không phải chọn thật một
  `GiaoHoId`, và nút "Cập nhật"/"Thêm giáo dân" hiện tại còn giữ nguyên `giaoHoId` cũ (xem
  `xuLySubmit` của `GiaoDanDetail.tsx` — trường `giaoHo` trên UI chưa thực sự gửi đi được).
  Việc này thuộc phạm vi "Giáo họ chọn thật" đã ghi ở mục Ưu tiên khắc phục #6 của
  `giao-dan-danh-sach.md`, ngoài phạm vi nhiệm vụ ghi giáo dân này.
- **Rule 11 (trùng ngày chuyển xứ), khối "Thông tin chuyển xứ", tab Giáo lý (BD1/BD2/Vào
  đời/GLHN) — CHƯA làm**: các trường liên quan (`ChuyenXu.NgayChuyen`, `NgayBD1`...) chưa có
  trong `TaoGiaoDanRequest`/`CapNhatGiaoDanRequest` (xem `giao-dan-chi-tiet.md` mục 10, tab
  Giáo lý "chỉ hiển thị UI tĩnh"). Không kiểm tra được cho tới khi các trường này được đưa vào
  request.
- **Rule 18 (không cho bỏ tick "Có gia đình" khi còn hôn phối hiệu lực) — CHƯA làm**: cần đọc
  `HonPhoi`/`GiaoDanHonPhoi` và biết ai "còn sống" trong cặp vợ chồng — để trong phạm vi việc
  sau, không thuộc "tạo mới/xoá" trọng tâm của nhiệm vụ này.
- **Rule 22, 23 (tự động sửa `DaCoGiaDinh` của người vợ/chồng còn lại khi tick Qua đời) — CHƯA
  làm**: hành vi ngầm này chưa được tái hiện; ghi lại để không quên, không phải quyết định có
  chủ đích.

### 20. Nhiều cảnh báo Yes/No liên tiếp của desktop được gộp thành MỘT lượt xác nhận trên web

- **Bản desktop**: `checkInput()` hỏi từng cảnh báo (rule 8, 9, 10, 12, 13, 16) bằng một hộp
  thoại `MessageBox` riêng, tuần tự — người dùng có thể trả lời Yes cho cảnh báo này, No cho
  cảnh báo khác.
- **Bản web**: `GiaoDanService.KiemTraNghiepVu` thu thập MỌI cảnh báo áp dụng được trong một
  lần kiểm tra, trả về `KetQuaLuuGiaoDanDto.CanhBao` (mảng); front-end nối lại thành MỘT hộp
  `window.confirm`, xác nhận (`BoQuaCanhBao=true`) là chấp nhận TẤT CẢ cảnh báo cùng lúc, không
  chọn được "chấp nhận cảnh báo A nhưng không chấp nhận cảnh báo B".
- **Vì sao không tái hiện y hệt**: mô hình yêu cầu/phản hồi HTTP không có khái niệm "hộp thoại
  đồng bộ chờ người dùng trả lời giữa chừng một hàm đang chạy trên máy chủ" như WinForms — máy
  chủ phải kiểm tra xong toàn bộ trong một lượt rồi trả kết quả. Không đổi bản chất "phải xác
  nhận rõ ràng mới được lưu" của desktop, chỉ khác cách trình bày (một hộp gộp thay vì nhiều hộp
  tuần tự).
- **Câu hỏi cho người dùng**: xác nhận cách gộp này chấp nhận được, hay cần một luồng nhiều
  bước (mỗi cảnh báo một bước xác nhận riêng) để giữ đúng khả năng "đồng ý một phần" của
  desktop.

### 21. Backend "ghi gia đình" (2026-09-06) — xác nhận đã tái hiện các mục 2, 3, 4, 5

- **Mục 2** (tự xoá giáo dân khỏi gia đình cũ bị comment): `GiaDinhService.ThemThanhVien` hỏi
  cảnh báo rồi **vẫn cho thêm mà không xoá khỏi gia đình cũ** khi client xác nhận
  (`BoQuaCanhBao=true`) — kiểm bằng
  `GiaDinhGhiTests.Them_nguoi_da_thuoc_gia_dinh_khac_tra_ve_canh_bao_roi_van_cho_them_khi_xac_nhan`
  (giáo dân thuộc CẢ HAI gia đình sau khi xác nhận, đúng lỗi gốc).
- **Mục 3** (cây quyết định `NguoiCu`): server **không** tự đoán vai trò mới — endpoint
  `PUT /api/gia-dinh/{id}/vo-chong/{vaiTro}` nhận `XuLyNguoiCuDto { Xoa, VaiTroMoi }` là Ý ĐỊNH
  CUỐI CÙNG mà lượt sau (giao diện) đã hỏi xong qua cây quyết định nhiều bước, rồi thực hiện
  nguyên tử (xoá dòng cũ + thêm dòng mới cùng một giao dịch). Nếu vai trò đang có người mà
  client chưa gửi `XuLyNguoiCu`, server trả `CanQuyetDinhNguoiCu` (không tự đoán) — xem
  `GiaDinhGhiTests.Doi_nguoi_chong_khi_chua_quyet_dinh_nguoi_cu_tra_canh_bao_can_quyet_dinh`.
- **Mục 5** (xoá thành viên là xoá vĩnh viễn): `GiaDinhService.XoaThanhVien` gọi
  `db.ThanhVienGiaDinh.Remove` (xoá thật), không có cờ `DaXoa` nào trên bảng này — kiểm bằng
  `GiaDinhGhiTests.Xoa_thanh_vien_la_xoa_vinh_vien_khoi_ThanhVienGiaDinh`.
- **Mục 4** (không áp dụng trực tiếp ở đây — mục 4 nói về Ơn gọi tận hiến, không liên quan gia
  đình; bỏ qua).

### 22. `GiaDinhService.GanVoChong` — hai chỗ mô hình hoá gần đúng, không đọc được nguyên văn từ mã nguồn desktop

- **"Đã chuyển xứ" của một GIÁO DÂN** (dùng trong `ThemThanhVien` khi thêm thành viên): bảng
  `GiaoDan` KHÔNG có cột `DaChuyenXu` riêng (khác `GiaDinh`, có cột này) — desktop đọc giá trị
  này từ đâu (có thể tính từ bảng `ChuyenXu` qua một hàm `Memory`/`GxGiaoDan` chưa đọc trong
  nhiệm vụ này). Bản web suy "đã chuyển xứ" = bản ghi `ChuyenXu` MỚI NHẤT (theo `NgayChuyen`) của
  giáo dân đó có `LoaiChuyen = ChuyenDi`. Đây là suy diễn hợp lý dựa trên enum có sẵn, chưa đối
  chiếu được với mã nguồn thật.
- **Hành động "chuyển về lại xứ"** khi client trả lời Yes cho hộp thoại 3 lựa chọn (dòng
  1127-1131): bản web tạo một bản ghi `ChuyenXu` MỚI với `LoaiChuyen = ChuyenDen` (ghi chú
  "Chuyển về lại xứ khi được thêm vào gia đình") thay vì sửa/xoá gì trên bản ghi `ChuyenXu` cũ —
  chưa xác nhận đây có đúng ý nghĩa "trở về lại xứ" của bản gốc không (bản gốc có thể chỉ đổi
  một cờ, không thêm bản ghi lịch sử mới).
- **`Memory.KiemTraVoChong` (CMemory.cs:1691-1710)**: câu SQL gốc `SELECT_CHECK_VOCHONG` không
  nằm trong phạm vi đọc của nhiệm vụ này (nằm trong `SqlConstants.cs`, không đọc). Bản web tái
  hiện qua suy luận: lấy đôi hôn phối MỚI NHẤT theo `SoThuTu` của giáo dân, người kia còn sống
  (`!QuaDoi`), rồi hiện đúng nguyên văn thông báo `"Giáo dân này đã từng kết hôn với [...], thuộc
  đôi hôn phối [...]"`. Đã kiểm tra thật bằng dữ liệu `qlgx_thu` (một giáo dân từng kết hôn thật
  sự kích hoạt đúng cảnh báo này khi gán làm Người nam của một gia đình khác) — hành vi có vẻ
  đúng nhưng chưa đối chiếu được 100% với SQL gốc.
- **Câu hỏi cho người dùng**: xác nhận ba điểm suy diễn trên là chấp nhận được, hay cần đọc thêm
  mã nguồn (`GxGiaoDan.cs`, `SqlConstants.cs`) để làm đúng hơn.

### 23. Khối "Thông tin cá nhân" đổi từ BA cột (desktop) sang HAI cột (web) — khác biệt có chủ đích theo yêu cầu người dùng (2026-09-07)

- **Bản desktop**: `grbCaNhan` của `Source/GXControl/frmGiaoDan.Designer.cs` (dòng 1039-1337)
  chia BA cột: trái (Mã giáo dân/Tên thánh/Họ tên/Giáo họ), giữa (ảnh đại diện), phải (Giới
  tính+Ngày sinh/Nơi sinh/Tên Cha/Tên Mẹ/CMND).
- **Người dùng góp ý (2026-09-07)**: bố cục ba cột này lệch trực quan so với hai tấm 50/50
  ngay bên dưới cùng màn hình (`Rửa tội` ‖ `Rước lễ lần đầu`, `.card-row` với
  `grid-template-columns: 1fr 1fr`) — *"bạn có thể chia box này thành 2 column như 2 panel bên
  dưới cho cân đối"*.
- **Bản web đã làm (CỐ Ý khác desktop theo đúng yêu cầu trực tiếp)**: đổi `.canhan-cols` (
  `WebApp/src/web/src/styles/qlgx.css`) từ `1.15fr 148px 1.3fr` (ba cột) sang `1fr 1fr` (hai
  cột bằng nhau, cùng tỉ lệ với `.card-row`) — cột trái gồm Mã giáo dân/Tên thánh (cạnh ảnh đại
  diện thu nhỏ trong `.canhan-top`)/Họ tên/Giáo họ; cột phải gồm Giới tính+Ngày sinh/Nơi sinh/
  Tên Cha/Tên Mẹ/CMND. Ảnh đại diện chuyển từ cột giữa riêng vào ĐẦU cột trái (nhỏ lại 92×92px)
  thay vì có cả cột 148px riêng, để không tái tạo khoảng trống lớn dưới ảnh mà chính người dùng
  đã phàn nàn một lần trước đó (xem commit `8e34da6`). Xem `GiaoDanDetail.tsx` (khối `tabCaNhan`)
  và `giao-dan-chi-tiet.md` mục 2.
- **Vì sao ghi vào đây dù đã có xác nhận rõ ràng của người dùng**: theo đúng quy ước của tệp này
  — "hãy cứ migrate logic hoàn toàn giống app hiện tại rồi note lại để review sau" áp dụng cho
  logic NGHIỆP VỤ; đây là bố cục GIAO DIỆN nên được phép đổi theo góp ý trực tiếp, nhưng vẫn ghi
  lại vì nó khác `Designer.cs` gốc — để không ai sau này tưởng nhầm là bỏ sót khi đối chiếu bố
  cục ba cột của desktop.
- **Không cần review thêm**: đây không phải câu hỏi chờ quyết định — người dùng đã chốt trực
  tiếp trong yêu cầu này.

---

## Chỗ bản WEB đang lệch so với desktop (khác loại với các mục trên)

Các mục trên là *lỗi của bản desktop* mà ta cố ý tái hiện. Mục dưới đây ngược lại: **bản web
đang làm khác desktop mà không cố ý** — cần sửa để đúng nguyên tắc "giống hệt bản hiện tại".

### W1. Ngày tháng hiển thị sai định dạng ở màn hình chi tiết — ĐÃ XỬ LÝ (2026-09-06)

- **Phát hiện**: 2026-09-06, khi xem ảnh chụp kiểm thử tab Hôn phối. Ô "Ngày hôn phối" hiện
  `04/25/2015` (định dạng Mỹ MM/DD/YYYY) thay vì `25/04/2015`.
- **Nguyên nhân**: hai màn hình chi tiết dùng tổng cộng **26 ô `<input type="date">` gốc của
  trình duyệt** (`GiaoDanDetail.tsx` 25 ô, `GiaDinhDetail.tsx` 1 ô — nhiều hơn con số 19 ước
  lượng ban đầu vì tính thêm cả các ô tĩnh chưa nối API ở tab Giáo lý). Ô ngày gốc **luôn hiển
  thị theo locale của trình duyệt người dùng**, lập trình viên không kiểm soát được. Lưới danh
  sách cũng hiện ISO `yyyy-MM-dd` thay vì `dd/MM/yyyy` cho các cột ngày.
- **Bản desktop**: luôn hiển thị `dd/MM/yyyy`, không phụ thuộc máy người dùng. Toàn bộ dữ liệu
  ngày trong Access cũng lưu dạng chuỗi `dd/MM/yyyy` (xem `Qlgx.Data/NgayThangText.cs` phía máy
  chủ — không đổi, chỉ tầng hiển thị/nhập liệu ở client thay đổi).
- **Cách đã làm — hướng 2 (tự làm ô nhập, kèm nút mở lịch) đúng như đề xuất ban đầu**:
  1. Thêm `WebApp/src/web/src/lib/ngay.ts`: `dinhDangNgay()` (ISO → `dd/MM/yyyy`, giữ nguyên
     văn dữ liệu lỗi như `"1958"`/chuỗi rỗng, không ném lỗi) và `ngayTuHienThi()` (`dd/MM/yyyy`
     → ISO, phân biệt `null` = xoá ngày và `undefined` = gõ sai định dạng để không âm thầm nuốt
     giá trị).
  2. Thêm component dùng chung `WebApp/src/web/src/components/GxDate.tsx`: một ô văn bản hiển
     thị/nhập `dd/MM/yyyy` thật (placeholder `dd/mm/yyyy`), báo lỗi rõ ràng dưới ô khi gõ sai,
     kèm nút tròn 📅 mở lịch bấm chọn qua `showPicker()` của một `<input type="date">` ẩn khỏi
     mắt bằng kỹ thuật "clip" (không dùng `hidden`/`display:none` vì một số trình duyệt chặn
     `showPicker()` trên phần tử ẩn kiểu đó) — ô lịch ẩn này mang chính `name`/giá trị ISO nên
     `FormData`/`querySelector('[name="…"]')` ở nơi gọi không cần đổi gì.
  3. Thay toàn bộ 26 ô `<input type="date">` bằng `<GxDate>` ở cả `GiaoDanDetail.tsx` và
     `GiaDinhDetail.tsx` (kể cả các ô tĩnh chưa nối API ở tab Giáo lý, cho nhất quán); đổi dòng
     tóm tắt đầu trang chi tiết giáo dân (`sinh 2014-12-18` → `sinh 18/12/2014`).
  4. Cột ngày trên lưới (`WebApp/src/web/src/cot/cotGiaoDan.ts`): thêm hàm `ngay()` dùng
     `valueFormatter: (p) => dinhDangNgay(p.value)`, **giữ nguyên `field` là ISO gốc** để sắp
     xếp/lọc vẫn đúng (ISO so sánh chuỗi trùng thứ tự thời gian) — cố ý **không** dùng
     `valueGetter` trả chuỗi đã định dạng vì sẽ làm sai sắp xếp. Xuất CSV (`GxGrid.layCsv`)
     cũng đổi sang `getDataAsCsv({ processCellCallback: (p) => p.formatValue(p.value) })` để
     CSV xuất ra cũng là `dd/MM/yyyy`, không phải ISO.
  5. Test: `WebApp/src/web/src/lib/ngay.test.ts`, `components/GxDate.test.tsx` (component mới),
     và một bài test sắp xếp trong `components/GxGiaoDanList.test.tsx` chứng minh bấm sắp xếp
     cột "Ngày sinh" cho kết quả đúng theo THỜI GIAN THẬT (1990 < 2005 < 2015), không theo so
     sánh chuỗi `dd/MM/yyyy` đã định dạng.
  6. Kiểm chứng bằng chạy thật trên `qlgx_thu` (Playwright): sửa "Ngày rửa tội" của giáo dân
     mã 1 từ `28/02/2015` thành `25/04/2015`, lưu, tải lại — `psql` xác nhận cột
     `ngay_rua_toi = 2015-04-25` (đúng ngày 25, tháng 4, không bị đảo). Bấm sắp xếp cột "Ngày
     sinh" trên 2039 bản ghi thật cho thứ tự tăng/giảm đúng theo năm-tháng-ngày thật (ảnh chụp
     `WebApp/anh-chup-kiem-thu/26-*.png`).
  7. **Không đổi hợp đồng API**: dữ liệu gửi lên/nhận về vẫn nguyên ISO `yyyy-MM-dd`, chỉ đổi
     tầng hiển thị/nhập liệu ở client.

---

### 24. Giao diện form gia đình (2026-09-07) — nối GxPicker, cây quyết định NguoiCu, lưới thành viên, tạo mới

Hoàn thiện phần giao diện còn thiếu của `GiaDinhDetail.tsx`/`GiaDinhDetailPage.tsx` — backend đã
xong từ trước (xem `task-ghi-gia-dinh-backend-report.md`). Các quyết định tự đưa ra (người dùng
đã đi ngủ, theo đúng chỉ dẫn "tự quyết theo hướng hợp lý nhất"):

- **Mục 4 (lỗi chọn người làm đóng cả form) — QUYẾT ĐỊNH: KHÔNG tái hiện, báo lỗi tại chỗ.**
  Toàn bộ lỗi khi gán Người nam/Người nữ (sai giới tính, đang ở gia đình khác, RowVersion xung
  đột, lỗi mạng...) hiện qua hộp thoại trong ứng dụng (`useHoiDap.bao`) ngay tại thẻ đang mở,
  KHÔNG đóng thẻ, KHÔNG mất dữ liệu các trường khác đang gõ dở. Lý do (như đã ghi ở mục 4): trên
  web mất dữ liệu đang nhập khó chịu hơn desktop rất nhiều (không có "mất con trỏ focus" để cảnh
  báo sớm như WinForms), và hành vi đóng form của desktop được chính spec gốc gọi là "kỳ quặc".
  Không cần review thêm — đây thuộc nhóm khác biệt GIAO DIỆN được quyết trực tiếp trong yêu cầu.

- **Cây quyết định `NguoiCu` (mục 3) — migrate ĐẦY ĐỦ, không rút gọn.** Đọc trực tiếp
  `Source/GXControl/frmGiaDinh.cs:649-919` (UTF-16LE) và hàm phụ trợ `CheckVaiTroTrongGiaDinh`
  (dòng 621-646) để giải mã chính xác từng nhánh — bản spec `gia-dinh-chi-tiet.md` mục 4 chỉ
  trích một phần thông báo, không đủ để tái hiện đúng thứ tự/điều kiện. Cài tại
  `WebApp/src/web/src/lib/nguoiCu.ts` (`chayCayQuyetDinhNguoiCu`), giữ NGUYÊN các nhánh "có vẻ
  vô lý" của bản gốc — ví dụ kiểm tra vai trò `VAITRO_CON` trên chính ID của người-còn-lại
  (vợ/chồng) thay vì hỏi tổng quát "gia đình đã có con chưa" (dòng 700/717/827 bản gốc) — đúng
  quy tắc migrate y hệt. 12 bài test đơn vị (`nguoiCu.test.ts`) phủ mọi nhánh chính (xoá hẳn,
  hạ xuống thành viên với Cha/Mẹ/Chưa rõ, đổi hàng loạt sang Ông/Bà, đổi hàng loạt sang Chưa rõ,
  cả hai chiều Chồng/Vợ).
  - **Giới hạn đã biết**: nhánh "đổi hàng loạt vai trò các thành viên KHÁC" (Cha/Mẹ→Ông/Bà,
    hoặc mọi người→Chưa rõ, dòng 728-736/752-755/840-847/865-867) được áp dụng bằng các lệnh
    xoá+thêm lại tuần tự qua API hiện có (không có endpoint "sửa vai trò tại chỗ") — nếu một
    lệnh giữa chừng lỗi mạng, một phần đổi hàng loạt có thể dang dở (chỉ ghi log, không chặn
    thao tác chính người dùng đang chờ). Bản desktop làm việc này trên một `DataTable` trong bộ
    nhớ rồi lưu một lượt (atomic hơn). Chấp nhận được ở quy mô gia đình (thường <10 thành viên)
    nhưng cần biết nếu sau này thấy dữ liệu vai trò "nửa vời" ở một gia đình cụ thể.
  - **KHÔNG migrate** nhánh `chonNguoiConLai`/`ganNguoiConLai`/`KiemTraSuThayDoiNguoiConLai`
    (dòng 350-410) — tính năng "tự động đề nghị chọn luôn người còn lại nếu đã có hôn phối với
    ai đó" khi chọn MỘT trong hai vai trò Chồng/Vợ. Đây là một luồng RIÊNG, phức tạp tương đương
    (dùng lại `NguoiCu` với `rowmoi=null` ở một nhánh phụ), nằm NGOÀI phạm vi "cây quyết định
    NguoiCu khi đổi vợ/chồng" được giao lần này. Ghi lại để không quên — người dùng vẫn tự chọn
    tay cả hai vai trò, không bị chặn, chỉ là không được "gợi ý tự động" như desktop.

- **Danh sách 21 giá trị `VaiTro`** (`lib/vaiTroGiaDinh.ts`) lấy từ khối chú thích cũ
  `frmGiaDinh.cs:992-1018` (đã đánh dấu "không chắc còn khớp" ở `gia-dinh-chi-tiet.md` mục 9) vì
  không tìm được `Memory.GetQuanHeList()` thật trong phạm vi đã đọc. Dữ liệu thật `qlgx_thu` chỉ
  dùng tập con {0,1,2,3,8,18,100} — đúng khớp với danh sách này, tăng độ tin cậy nhưng CHƯA xác
  nhận 100%. Cột "Quan hệ GĐ" trên lưới thành viên đổi thành CHỈ ĐỌC (trước đây là dropdown sửa
  tại chỗ với 3 giá trị Chồng/Vợ/Con — không nối gì, không có tác dụng thật) — sửa vai trò một
  thành viên nay làm qua Xoá rồi Thêm lại với vai trò mới (chưa có endpoint "sửa tại chỗ").

- **"Bỏ chọn" (nút X) cạnh Người nam/Người nữ** cũng chạy qua ĐÚNG cây quyết định `NguoiCu` (với
  `idNguoiMoi=null`) trước khi xoá — đúng bản gốc gọi `NguoiCu(nguoicu, null)` từ
  `KiemTraSuThayDoiNguoiConLai` (dòng 393/401). Vì không có `giaoDanId` mới để gọi
  `PUT vo-chong`, thực hiện bằng `DELETE thanh-vien` (+ `POST thanh-vien` nếu hạ xuống thành
  viên) — hai endpoint đã có sẵn, không cần endpoint mới.

- **"Thêm gia đình mới" (mục 4 nhiệm vụ)**: khác thời điểm sinh mã so với desktop (đúng
  `Memory.Instance.GetNextId` gọi ngay khi MỞ form Thêm mới, dòng 292) — bản web gọi
  `POST /api/gia-dinh` khi người dùng bấm nút "Tạo gia đình" (sau khi nhập Tên gia đình) thay vì
  ngay lúc mở thẻ, vì không có state phía máy khách nào giữ được "bản ghi nháp có mã nhưng chưa
  lưu vào CSDL" kiểu WinForms (mọi lần tải lại trang sẽ mất). Sau khi có id thật, thẻ CHUYỂN
  SANG chế độ sửa bình thường trên CÙNG một thẻ (không đóng/mở lại) — Người nam/nữ và thành viên
  dùng được ngay. Chỉ validate tối thiểu "Tên gia đình" bắt buộc (đúng thông báo nguyên văn
  "Hãy nhập tên gia đình!", rule 6 `checkInput`) — các quy tắc còn lại của `checkInput` (Giáo họ
  bắt buộc, xác định Chủ hộ, kiểm tra cặp Chồng-Vợ đã từng lập gia đình khác...) CHƯA migrate ở
  lượt này, đã ghi trong `gia-dinh-chi-tiet.md` mục 10 ("Validate... Thiếu ở frontend") từ trước.

- **Chủ hộ (radio Người nam/Người nữ)** vẫn CHƯA nối (đã ghi từ trước ở `gia-dinh-chi-tiet.md`
  mục 10) — ngoài phạm vi 5 việc được giao lần này (picker, NguoiCu, lưới thành viên, tạo mới,
  gạch ngang), không tự ý mở rộng thêm.

### 25. Bỏ `lapGd` khỏi điều kiện gạch ngang, VÀ tách hẳn quy tắc theo màn hình (2026-09-07)

- **Bối cảnh**: `GxGiaoDanList` (component dùng CHUNG cho cả màn hình danh sách giáo dân lẫn
  lưới "Thành viên khác" trong form gia đình) trước đây LUÔN áp `toDo={(d) => d.quaDoi ||
  d.daChuyenDi || d.lapGd}` bất kể nhúng ở đâu — VI PHẠM mục 7 đã ghi từ trước ("Bản desktop
  không gạch ngang dòng nào trên lưới giáo dân") vì màn hình danh sách giáo dân dùng đúng
  component này.
- **Đã sửa hai việc cùng lúc**:
  1. Chỉ áp gạch ngang khi `quanHeGiaDinh=true` (tức đang nhúng trong form gia đình) — màn hình
     danh sách giáo dân (dùng mặc định, `quanHeGiaDinh` không truyền) nay ĐÚNG mục 7: không gạch
     ngang dòng nào, bất kể `quaDoi`/`daChuyenDi`/`lapGd`.
  2. **Bỏ `lapGd` khỏi điều kiện** ở nhánh `quanHeGiaDinh` (lưới gia đình) — QUYẾT ĐỊNH TỰ ĐƯA RA
     (người dùng đang ngủ): giữ `quaDoi`/`daChuyenDi`, bỏ `lapGd`. Lý do: gạch ngang người "đã
     lập gia đình" trong chính lưới liệt kê THÀNH VIÊN GIA ĐÌNH gây hiểu nhầm nghiêm trọng — con
     cái trưởng thành đã lập gia đình riêng vẫn thường được liệt kê ở đây (ví dụ để biết ai từng
     là con của gia đình), gạch ngang khiến họ trông như "không còn tồn tại"/lỗi dữ liệu. Ngược
     lại `quaDoi`/`daChuyenDi` đúng nghĩa "không còn sinh hoạt tại xứ", và mặc định các bản ghi
     này đã bị lọc ẨN khỏi lưới (chỉ hiện khi người dùng chủ động tick "Hiện cả người đã qua đời
     / đã chuyển xứ" ở màn hình liên quan) — lúc đó gạch ngang mới thực sự hữu ích để phân biệt.
  3. Sửa chú thích chân lưới tương ứng: còn "Gạch ngang đỏ: đã qua đời hoặc đã chuyển xứ" (bỏ
     "hoặc lập gia đình riêng"), và chú thích này CHỈ hiện khi `quanHeGiaDinh=true` (trước đây
     hiện cả ở màn hình danh sách giáo dân dù không gạch ngang gì — gây hiểu nhầm).
- **Bản desktop THẬT SỰ làm gì**: không xác định được (file `FormattingRow` của
  `GxGiaoDanList.cs` không đọc trong phạm vi 2 nhiệm vụ đã giao — chỉ biết CHẮC CHẮN màn hình
  DANH SÁCH GIÁO DÂN không gạch ngang, còn màn hình GIA ĐÌNH desktop dùng cột nội bộ `GACH` tính
  từ `Memory.IsRedGiaoDan` — logic của hàm đó chưa đọc). Quyết định trên là suy luận hợp lý nhất
  từ mục đích sử dụng, cần người dùng xác nhận lại khi thức dậy.
- **Câu hỏi cho người dùng**: xác nhận bỏ `lapGd` khỏi gạch ngang lưới gia đình là đúng ý muốn,
  hay muốn khôi phục lại (và nếu khôi phục, có cần đọc thêm mã `Memory.IsRedGiaoDan` để biết
  chính xác desktop tính "GACH" ra sao thay vì suy luận)?

### 26. Nút "Xoá khỏi gia đình" trên lưới thành viên — mở rộng có chủ đích (menu chuột phải)

Thêm mục menu chuột phải "Xoá khỏi gia đình" vào lưới "Thành viên khác trong gia đình" (nối
`DELETE /api/gia-dinh/{id}/thanh-vien/{giaoDanId}/{vaiTro}`, xoá VĨNH VIỄN đúng mục 5) — bản
desktop dùng nút riêng trên thanh công cụ `gxAddEdit1` phía trên lưới (nút "Xóa" theo
`gia-dinh-chi-tiet.md` mục 2 "Lưới..."), không phải menu chuột phải. Bản web dùng menu chuột
phải vì thanh công cụ `GxGiaoDanList` dùng chung không có sẵn chỗ cho nút thao tác riêng của
màn hình gia đình (thanh công cụ đó phục vụ Thêm/Sửa/Xóa/Lấy lại/In chung, không phải nút "chỉ
xoá thành viên gia đình" — xem mục 8 đã xác nhận tooltip). Hành vi nghiệp vụ (hỏi xác nhận đúng
nguyên văn, xoá vĩnh viễn) giữ nguyên y hệt — chỉ khác VỊ TRÍ nút bấm trên giao diện, không phải
khác biệt nghiệp vụ.

### 27. Task 14 — Xác thực và phân tách tenant theo claim: các quyết định tự đưa ra (2026-09-07)

Task 14 là task bảo mật quan trọng nhất của dự án (chuyển `giao_xu_id` từ cấu hình máy chủ
sang claim của người đăng nhập). Ghi lại các quyết định tự đưa ra ở chỗ mơ hồ, theo yêu cầu
"chọn phương án an toàn nhất, làm tiếp, ghi rõ quyết định + lý do":

**a) Đăng nhập cùng tên đăng nhập trùng giữa các giáo xứ khác nhau.** `TenTaiKhoan` chỉ duy
nhất TRONG một giáo xứ (`TaiKhoanConfig`: chỉ mục `(GiaoXuId, TenTaiKhoan)`), không duy nhất
toàn máy chủ — hợp lý vì trước đây mỗi giáo xứ là một bản cài độc lập, có thể trùng tên đăng
nhập kiểu "vanphong". Khi đăng nhập chưa biết claim giáo xứ (đó chính là thứ cần xác định), nên
`AuthService.DangNhap` tra CHÉO GIÁO XỨ theo tên đăng nhập (dùng `QlgxDbContext` dựng thủ công
với `boiCanh: null`, cùng kiểu với công cụ chuyển đổi dữ liệu), rồi thử khớp mật khẩu với TỪNG
ứng viên trùng tên cho tới khi khớp. Đây là truy vấn chéo giáo xứ DUY NHẤT được phép trong toàn
hệ thống, và chỉ dùng để xác định tài khoản nào đăng nhập — sau bước này mọi thứ đi qua
`BoiCanhGiaoXuTuNguoiDung` như bình thường. Rủi ro: nếu một giáo xứ có mật khẩu yếu và một
giáo xứ khác trùng tên đăng nhập, kẻ tấn công có mật khẩu đúng của MỘT trong các tài khoản
trùng tên sẽ đăng nhập vào ĐÚNG giáo xứ có mật khẩu đó khớp (không phải giáo xứ tuỳ chọn) —
không phải lỗ hổng vì mật khẩu vẫn phải khớp chính xác cho tài khoản cụ thể, chỉ là tên đăng
nhập không phải bí mật. Vẫn nên khuyến khích tên đăng nhập có tiền tố phân biệt giáo xứ khi
giáo xứ thứ hai lên hệ thống thật.

**b) Đăng xuất không thu hồi được token (JWT tự chứa, máy chủ không giữ trạng thái).** Theo
đúng ràng buộc "API không giữ trạng thái trong tiến trình", đăng xuất chỉ xoá token phía trình
duyệt (`authStore.xoaToken()`) — token cũ về mặt kỹ thuật vẫn hợp lệ tới khi hết hạn (8 tiếng,
xem `TokenService.ThoiGianSong`) nếu bị đánh cắp trước đó. Đã cân nhắc bảng "token bị thu hồi"
trong CSDL (không vi phạm "không giữ trạng thái trong tiến trình" vì trạng thái nằm ở CSDL) —
KHÔNG làm ở Task 14 vì phạm vi đã rất lớn; ghi lại đây làm việc cần làm nếu triển khai thật với
nhiều giáo xứ nhạy cảm. Giảm nhẹ bằng thời hạn token ngắn (8 tiếng thay vì vài ngày).

**c) Không có cơ chế làm mới token (refresh token) — token hết hạn giữa chừng bắt đăng nhập
lại.** `goi()` (client.ts) bắt mọi 401 và gọi `authStore.baoHet401()` → tự động về màn hình
đăng nhập, KHÔNG xoá dữ liệu form đang gõ (đó là việc của cơ chế nháp `localStorage` từng form
chi tiết, độc lập). Phương án an toàn hơn (refresh token xoay vòng) bị hoãn vì tăng đáng kể độ
phức tạp bảo mật (cần nơi lưu refresh token, cơ chế thu hồi) cho một task đã rất lớn; 8 tiếng đủ
một ca làm việc nên tần suất bị ngắt giữa chừng thấp.

**d) Phân quyền màn hình Quản lý tài khoản bị SIẾT LẠI so với desktop.** Bản desktop
`frmAccoutList.cs` không chặn quyền — bất kỳ ai mở được menu "Hệ thống" đều vào được, kể cả tự
cấp quyền Quản trị viên cho tài khoản bất kỳ (xác nhận bằng `grep -rn IsAdmin Source/` — cờ
`IsAdmin` được gán ở `frmLogin.cs` nhưng không nơi nào khác đọc lại để khoá chức năng). Đây là
lỗ hổng của bản cũ, không phải hành vi cố ý cần giữ nguyên (nguyên tắc "ghi cả những chỗ bản
desktop làm sai" áp dụng — xem `quan-ly-tai-khoan.md` mục 8). Bản web thêm policy "QuanTri"
(`RequireClaim(loai_tai_khoan, "0")`) cho toàn bộ `/api/tai-khoan/*` — chỉ Quản trị viên (loại
0) mới tạo/sửa/xoá được tài khoản. SideNav cũng chỉ hiện mục "Quản lý tài khoản" khi
`loaiTaiKhoan === 0` (ẩn phía giao diện, KHÔNG phải cơ chế bảo mật — bảo mật thật nằm ở
policy phía API, đã có test `Nguoi_dung_khong_phai_quan_tri_khong_vao_duoc_quan_ly_tai_khoan`).

**e) Xoá tài khoản đổi từ xoá cứng (desktop, `DELETE_ACCOUNT`) sang xoá mềm (`DaXoa=true`).**
Nhất quán với GiaoDan/GiaDinh/GiaoHo (mọi bảng có `DaXoa` đều xoá mềm ở bản web); tránh mất vết
khi tài khoản đã dùng để tạo/sửa dữ liệu khác (dù Phase 1 chưa có audit log theo người dùng).

**f) Kiểm trùng tên đăng nhập khi Thêm mới đổi phạm vi từ TOÀN CỤC (desktop) sang TRONG một
giáo xứ (web).** Hệ quả trực tiếp của (a) — bản desktop chỉ có một giáo xứ nên "trùng toàn cục"
và "trùng trong giáo xứ" là một; bản web tách hai khái niệm, chọn theo đúng chỉ mục CSDL đã có
sẵn (`(GiaoXuId, TenTaiKhoan)` — xem `TaiKhoanConfig`).

**g) Câu hỏi/câu trả lời bí mật (`CauHoiGoiY`/`CauTraLoiGoiY`) — GIỮ CỘT, KHÔNG dùng cho khôi
phục mật khẩu, KHÔNG có UI nhập.** Yêu cầu người dùng đã chốt trước Task 14 (ghi ở `TaiKhoan.cs`
và nhắc lại trong đề bài Task 14) — không phải quyết định mới, chỉ xác nhận đã tuân thủ: màn
hình Quản lý tài khoản bản web không có hai ô này (khác desktop, xem `quan-ly-tai-khoan.md` mục
8). Quên mật khẩu → nhờ Quản trị viên đặt mật khẩu mới trực tiếp qua form Sửa.

**h) Băm mật khẩu bằng `PasswordHasher<TaiKhoan>` (`Microsoft.AspNetCore.Identity`, PBKDF2 +
HMACSHA256, 100.000+ vòng lặp theo mặc định của thư viện).** Không dùng thuật toán tự viết —
đây là lựa chọn "thuật toán chuẩn có sẵn của .NET" theo đúng yêu cầu đề bài, tránh mọi rủi ro tự
implement PBKDF2/salt sai.

**i) Khoá ký JWT (`Qlgx__JwtKey`) và toàn bộ bí mật liên quan đăng nhập LUÔN đọc từ biến môi
trường, KHÔNG bao giờ có giá trị mặc định trong mã nguồn.** `TokenService` ném lỗi rõ ràng nếu
thiếu, cùng phong cách với `QLGX_TEST_PG` đã có. Bộ test KHÔNG dùng khoá cố định — mỗi lần chạy
`QlgxApiFactory` tự sinh khoá ngẫu nhiên 256-bit (`RandomNumberGenerator.GetBytes(32)`) để
không có bất kỳ chuỗi bí mật tĩnh nào trong repo, kể cả trong mã test.

**j) Tạo tài khoản quản trị đầu tiên bằng dòng lệnh (`dotnet run -- tao-tai-khoan-quan-tri`),
đọc toàn bộ tham số từ biến môi trường (`QLGX_ADMIN_*`), KHÔNG có endpoint HTTP tương ứng.** Một
endpoint "tạo admin" không cần xác thực là lỗ hổng nghiêm trọng (bất kỳ ai cũng tự cấp quyền
quản trị được) — loại bỏ hẳn khả năng đó bằng cách không có endpoint nào cả; chỉ người vận hành
có quyền truy cập biến môi trường của máy chủ mới chạy được. Xác định giáo xứ bằng
`QLGX_ADMIN_GIAO_XU_ID` (GUID, ưu tiên) hoặc `QLGX_ADMIN_GIAO_XU_TEN` (tra theo `TenGiaoXu`,
tiện cho giai đoạn thí điểm khi giáo xứ được chèn thẳng vào CSDL, xem mục "Thí điểm vài giáo xứ
trước" trong tài liệu thiết kế).

**k) Màn hình Quản lý tài khoản dùng bảng HTML thường thay vì AG Grid.** Danh sách tài khoản
của một giáo xứ chỉ vài dòng (Quản trị viên + vài người nhập liệu) — không cần lọc/sắp
xếp/nhóm/phân trang như các lưới nghiệp vụ chính (giáo dân, gia đình). Giảm độ phức tạp không
cần thiết; có thể nâng cấp sau nếu một giáo xứ thật sự có nhiều tài khoản.

**l) `/api/auth/toi` (lấy lại thông tin người dùng khi tải lại trang) đọc thẳng từ claim của
token, KHÔNG truy vấn lại bảng `TaiKhoan`.** Nghĩa là nếu Quản trị viên đổi họ tên/loại tài
khoản của một người đang có phiên đăng nhập, thay đổi đó chỉ có hiệu lực ở phiên đăng nhập MỚI
(sau khi token cũ hết hạn hoặc người dùng đăng nhập lại) — chấp nhận được vì token chỉ sống 8
tiếng và đây đúng tinh thần "token tự chứa, máy chủ không tra CSDL để xác thực mỗi request".

**m) Không triển khai Row-Level Security (RLS) của PostgreSQL ở Task 14 dù tài liệu thiết kế
nói "phải bật trước khi có giáo xứ thứ hai lên hệ thống".** Task 14 hoàn thành lớp phòng thủ
thứ nhất (bộ lọc toàn cục EF Core + claim JWT, đã có test bảo mật xác nhận). RLS là lớp phòng
thủ thứ hai, việc riêng cần thời gian khảo sát cách EF Core + Npgsql phối hợp với RLS (đặt
`app.current_giao_xu_id` mỗi kết nối, v.v.) — ghi lại đây làm việc BẮT BUỘC phải làm trước khi
onboard giáo xứ thứ hai lên cùng máy chủ thật, không phải "có thể làm sau nếu rảnh".
