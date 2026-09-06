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
- **Khối "Thông tin chuyển xứ" và tab Giáo lý (BD1/BD2/Vào đời/GLHN) — ĐÃ NỐI (2026-09-07,
  task sửa review-frontend)**: cả hai đầu đọc/ghi đã hoạt động qua `PUT /api/giao-dan/{id}`
  (`CapNhatGiaoDanRequest.NgayBD1..XepLoaiGLHN` và `.ChuyenXu`) — xem mục 30 bên dưới để biết
  chi tiết quyết định (chỉ nối ở SỬA, không ở Tạo mới; bỏ hộp thoại cảnh báo Yes/No khi đổi về
  "Ở tại xứ"; sửa tại chỗ một dòng ChuyenXu thay vì luôn tạo dòng lịch sử mới).
- **Rule 11 (trùng ngày chuyển xứ) — VẪN CHƯA làm**: nay các trường `ChuyenXu.*` đã có trong
  request nên về mặt kỹ thuật có thể cài đặt được, nhưng việc này nằm ngoài phạm vi nhiệm vụ
  "sửa review-frontend" (chỉ yêu cầu nối dữ liệu, không yêu cầu thêm validate mới) — để lại cho
  lượt sau.
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

### 28. Task 16 — PWA và bản nháp ngoại tuyến: các quyết định tự đưa ra (2026-09-07)

Làm việc này trong lúc người dùng đi ngủ, tự quyết theo hướng an toàn nhất ở mọi chỗ mơ hồ, ghi
lại đây theo đúng yêu cầu. Xem báo cáo đầy đủ ở
`.superpowers/sdd/2026-09-06-qlgx-web-phase-1/task-16-report.md`.

**a) `useAuth()` KHÔNG được gọi trực tiếp trong `GiaoDanDetailPage`/`GiaDinhDetailPage`.** Cả
hai container này đã có bộ test dựng độc lập (không bọc `<AuthProvider>`) từ trước — gọi thẳng
`useAuth()` sẽ ném lỗi "phải gọi bên trong AuthProvider" và làm hỏng TOÀN BỘ các bài test đó.
Chọn phương án an toàn hơn: `App.tsx` (nơi đã có `useAuth()`) tính `tenTaiKhoan` MỘT LẦN rồi
truyền xuống hai container qua prop `tenTaiKhoan?: string | null` — không đổi hành vi thật (vẫn
lấy đúng tài khoản đang đăng nhập), chỉ đổi chỗ đọc, và giữ nguyên khả năng test độc lập.

**b) Khoá bản nháp gắn theo `(loại form, id bản ghi, tên tài khoản)`, KHÔNG gắn `giaoXuId`.**
Hai lý do: (1) một tài khoản đăng nhập chỉ thuộc một giáo xứ tại một thời điểm (xem Task 14),
`tenTaiKhoan` đã đủ để không lẫn; (2) tránh phình thêm một chiều khoá mà `AuthContext` hiện chưa
lộ `giaoXuId` ra props công khai. Nếu sau này một trình duyệt dùng để đăng nhập LUÂN PHIÊN vào
nhiều giáo xứ khác nhau bằng CÙNG một tên tài khoản trùng (xem mục 27a — tên đăng nhập trùng
giữa các giáo xứ là hợp lệ), nháp của giáo xứ này có thể hiện nhầm khi đăng nhập vào giáo xứ
kia cùng tên tài khoản trùng — rủi ro RẤT hẹp (trùng cả tên đăng nhập lẫn việc dùng chung máy),
chấp nhận được ở Phase 1; ghi lại để biết nếu cần xử lý sau.

**c) Bản nháp của bản ghi MỚI ("Thêm giáo dân"/"Thêm gia đình", `id = null`) dùng CHUNG một
khoá "moi" cho loại form đó — không phân biệt được nhiều thẻ "mới" đang mở song song.** Mở hai
thẻ "Thêm giáo dân" cùng lúc, gõ dở cả hai, chỉ thẻ gõ SAU CÙNG mới thắng trong `localStorage`
(thẻ kia vẫn hiển thị đúng nội dung nó đang gõ trên màn hình — chỉ mất nếu tải lại trang lúc
đó). Coi là đánh đổi chấp nhận được vì mở nhiều thẻ "mới" cùng loại cùng lúc là thao tác hiếm;
sửa đúng cần một id tạm sinh phía client cho mỗi thẻ nháp — để dành nếu có phàn nàn thật.

**d) Form "Thêm gia đình" (chỉ 2 trường: Tên gia đình + Giáo họ) KHÔNG bật tự động lưu nháp,
form giáo dân (60+ trường) VÀ form sửa gia đình đã có (nhiều trường hơn) THÌ có.** Rủi ro mất
công gõ tỉ lệ thuận với số trường — 2 trường gõ lại mất vài giây, không đáng thêm một luồng
autosave nữa cho một form đã đơn giản. Có thể bật thêm sau nếu cần.

**e) `useTuDongLuuBanNhap` CHỐT "mốc gốc" (baseline) bằng chính `layPayload()` ở lần chạy đầu
tiên sau khi mount, thay vì bắt đầu từ `null`.** Phát hiện bằng kiểm thử THẬT trên `qlgx_thu`
(Playwright): mở một giáo dân, KHÔNG sửa gì, đợi hơn 5 giây rồi tải lại trang — nếu mốc gốc là
`null`, lần hẹn giờ đầu tiên vẫn ghi một "bản nháp ma" giống hệt dữ liệu máy chủ (vì giá trị lúc
đó ≠ `null`), khiến người dùng chỉ MỞ RA XEM cũng bị hỏi "khôi phục bản nháp" một cách vô nghĩa
ở lần sau. Chốt mốc gốc bằng dữ liệu vừa tải giải quyết đúng gốc rễ: nháp chỉ thật sự được ghi
khi nội dung khác mốc gốc, tức là có sửa thật. Đã thêm lại một bài test hồi quy riêng cho ca
này (`GiaoDanDetailPage.test.tsx`, "ban nhap ma") — xem mục g bên dưới về khó khăn khi viết test
này dưới đồng hồ giả.

**f) Khôi phục bản nháp bằng CÁCH REMOUNT component (đổi `key`), KHÔNG bằng cách ghi thẳng vào
DOM.** `GxDate` (ô ngày) là input CÓ KIỂM SOÁT nội bộ (state `iso`/`text` riêng, xem
`GxDate.tsx`) — set `.value` thẳng lên phần tử DOM ẩn của nó sẽ bị React ghi đè lại ngay ở lần
render kế tiếp, không thật sự khôi phục được ngày tháng. Giải pháp: `GiaoDanDetail`/
`GiaDinhDetail` nhận thêm prop `banNhap` (đè lên `duLieu`/bản trống để dựng `p`/`f`), và
container (`...Page.tsx`) đổi `key` mỗi khi áp dụng một bản nháp — buộc React dựng lại toàn bộ
cây con từ đầu với giá trị mới, đúng cách `GxDate` (và mọi input không kiểm soát khác) đã dựa
vào để nhận `defaultValue` mới (xem chú thích gốc trong `GxDate.tsx`).

**g) Test tự động lưu nháp phải bật đồng hồ giả TRƯỚC `render()`, và chèn thêm một bước
`advanceTimersByTimeAsync(0)` giữa lúc chờ dựng xong form và lúc giả lập gõ.** Dưới đồng hồ giả
(`vi.useFakeTimers({ shouldAdvanceTime: true })`), hiệu ứng "chốt mốc gốc" (mục e) của
`GiaoDanDetail` có thể bị hoãn sang một tác vụ hẹn giờ mà `screen.findByRole` không chờ, khiến
nó vô tình chạy SAU bước giả lập gõ và chốt nhầm giá trị ĐÃ SỬA làm mốc gốc — chỉ là hiện tượng
riêng của kiểm thử dưới đồng hồ giả (đã xác nhận KHÔNG xảy ra khi kiểm thử thật trên trình
duyệt, xem ảnh `36`/`40` trong `WebApp/anh-chup-kiem-thu/`), không phải lỗi thật của ứng dụng.

**h) Cải wording thông báo lỗi mạng ở `client.ts` để phân biệt "mất mạng thật" và "máy chủ
Qlgx.Api chưa chạy" bằng `navigator.onLine`.** Trước Task 16, cả hai ca đều hiện chung một câu
hướng dẫn cho lập trình viên ("Kiểm tra Qlgx.Api đã chạy chưa") — không sai về mặt kỹ thuật
(request thật sự thất bại ở tầng mạng trong cả hai trường hợp) nhưng gây hoang mang cho người
dùng cuối lúc mất mạng thật ở giáo xứ vùng xa. Giữ nguyên câu cũ cho ca `navigator.onLine ===
true` (máy chủ không phản hồi dù có mạng — đúng tình huống dev hay gặp), thêm câu trấn an mới
cho ca `navigator.onLine === false`.

**i) KHÔNG cache bất kỳ phản hồi `/api/*` nào trong service worker — không khai báo
`runtimeCaching` cho `/api`.** Đây là quyết định AN TOÀN NHẤT theo đúng cảnh báo của người dùng
("đừng cache dữ liệu nghiệp vụ kiểu phục vụ dữ liệu cũ như thật"): mặc định của
`vite-plugin-pwa`/Workbox chỉ tiền tải (precache) các tệp build tĩnh (JS/CSS/HTML/icon), hoàn
toàn không đụng tới các lời gọi API lúc chạy trừ khi khai báo `runtimeCaching` — nên không khai
báo gì cho `/api` nghĩa là mọi lời gọi API luôn đi thẳng ra mạng, lỗi mạng vẫn ném lỗi thật (xem
mục h) thay vì âm thầm phục vụ dữ liệu cũ. Đã xác nhận bằng cách kiểm tra `dist/sw.js` sau khi
build: chuỗi `api` DUY NHẤT xuất hiện trong tệp là ở `navigateFallbackDenylist` (chặn service
worker trả `index.html` thay cho một request bắt đầu bằng `/api/`), không có route nào khác
nhắc tới `/api`.

**j) `registerType: 'prompt'` + `injectRegister: false`, tự đăng ký service worker bằng hook
`useRegisterSW` (`virtual:pwa-register/react`) trong `CapNhatPWA.tsx`.** Đúng yêu cầu "báo có
bản mới, tải lại" thay vì âm thầm chuyển bản — mặc định `registerType: 'autoUpdate'` của
`vite-plugin-pwa` sẽ tự activate service worker mới và có thể làm mất trạng thái/bản nháp đang
gõ dở ở một tab khác đang mở nếu tab đó tự reload theo. `CapNhatPWA` chỉ hiện dải nhỏ góc dưới
bên phải, người dùng chủ động bấm "Tải lại" khi sẵn sàng (hoặc "Để sau").

**k) Tạo icon PWA (`public/icons/*.png`) bằng script Python/Pillow tự vẽ (hình chữ thập trắng
trên nền `#1d5ddb`), KHÔNG dùng logo thật của giáo xứ.** Chưa có bộ nhận diện thương hiệu chính
thức nào cho QLGX ở giai đoạn này — icon tạm đủ để cài đặt PWA hoạt động đúng kỹ thuật (đúng
kích thước 192/512, có bản "maskable" cho Android), nên thay bằng icon thật khi có.

**l) Tài khoản kiểm thử `task16_kt` được tạo tạm trên `qlgx_thu` bằng
`dotnet run -- tao-tai-khoan-quan-tri` (mật khẩu ngẫu nhiên, chỉ tồn tại trong biến môi trường
của phiên chạy, KHÔNG ghi vào bất kỳ tệp nào) để không cần biết mật khẩu của tài khoản `quantri`
đã có sẵn (không được ghi ở đâu, đúng quy định). Đã XOÁ tài khoản này khỏi `tai_khoan` ngay sau
khi kiểm thử xong — xác nhận bằng truy vấn `select ten_tai_khoan from tai_khoan` chỉ còn lại
`quantri`.

**m) KHÔNG sửa lỗi `tsc` có sẵn từ trước ở `src/lib/csv.test.ts` (kiểu `Blob | MediaSource` của
`URL.createObjectURL`), dù lỗi này chặn đứng toàn bộ `npm run build` (script chạy `tsc -b &&
vite build`).** Xác nhận bằng `git stash` rồi chạy lại `tsc -b --noEmit`: lỗi đã tồn tại TRƯỚC
Task 16, không phải do các thay đổi của task này gây ra — nằm ngoài phạm vi được giao. Đã kiểm
chứng riêng phần Task 16 (PWA) bằng `npx vite build` (bỏ qua bước `tsc -b`), xác nhận
`vite-plugin-pwa` sinh đúng `dist/sw.js`/`dist/manifest.webmanifest`/`dist/workbox-*.js`. **Việc
cần làm trước khi ai đó thật sự chạy `npm run build` để triển khai:** sửa type test đó (đổi
chữ ký `mockImplementation` cho khớp `Blob | MediaSource`, hoặc ép kiểu tường minh).

**Cập nhật (2026-09-07, Task 13): lỗi type ở trên đã được sửa từ trước** (không rõ bởi task
nào) — `npm run build` chạy được bình thường, đã xác nhận lại bằng cách chạy thật trước khi bắt
đầu Task 13.

### 29. Task 13 — Kiểm thử đầu-cuối và triển khai máy chủ: các quyết định tự đưa ra (2026-09-07)

**a) Một image Docker DUY NHẤT cho cả API và web** (`WebApp/Dockerfile`, build nhiều giai đoạn:
build web bằng `node:22-alpine`, build API bằng `dotnet/sdk:10.0`, chạy bằng `dotnet/aspnet:10.0`
tối giản). Cân nhắc phương án tách hai image (API riêng, web riêng đứng sau một nginx/CDN) —
chọn GỘP vì ở quy mô pilot vài giáo xứ, một container/một health check/một phiên bản không thể
lệch nhau giữa API và web đơn giản hơn cho người vận hành, đổi lại là build lại cả hai khi chỉ
sửa một phía và không scale API/web độc lập được — chấp nhận được, có ghi rõ cách tách lại sau
nếu cần trong `TRIEN-KHAI.md`. `Qlgx.Api/Program.cs` chỉ bật `UseStaticFiles`/`MapFallbackToFile`
khi `wwwroot/index.html` THẬT SỰ tồn tại (kiểm tra bằng `File.Exists`) — môi trường dev/test
không có thư mục này nên hành vi không đổi (vẫn 172/183 test cũ), chỉ ảnh hưởng khi chạy từ
image Docker đã COPY `dist/` của web vào `wwwroot`. Route SPA fallback dùng ràng buộc regex phủ
định `{*duongDan:regex(^(?!api).*$)}` để một đường dẫn `/api/...` gõ sai vẫn trả 404 thật thay
vì âm thầm trả `index.html`.

**b) `.dockerignore` là bắt buộc, không phải tuỳ chọn** — thiếu nó, `obj/`/`bin/` build sẵn
trên máy Windows (chứa `project.assets.json` trỏ tới đường dẫn NuGet fallback CHỈ CÓ trên
Windows, `C:\Program Files (x86)\...`) bị copy thẳng vào build context Linux, khiến
`dotnet publish --no-restore` dùng nhầm cache hỏng và lỗi khó hiểu (`NuGet.Packaging.Core.
PackagingException`). Đã tái hiện lỗi này thật khi viết `Dockerfile`, sửa bằng
`.dockerignore` loại `**/bin/`, `**/obj/`, `**/node_modules/`, `**/dist/`.

**c) Migration CSDL tự chạy lúc khởi động, nhưng TẮT MẶC ĐỊNH, bật qua cấu hình
`Qlgx:ChayMigrationKhiKhoiDong`.** Thử để MẶC ĐỊNH BẬT trước — làm hỏng một test đã có từ
trước (`SucKhoeTests`, dùng `WebApplicationFactory<Program>` THUẦN không có CSDL thật, chỉ để
kiểm `/api/suc-khoe` không cần đụng CSDL) vì migration chạy vô điều kiện lúc khởi động host thử
kết nối một CSDL không tồn tại và làm sập ngay host thử nghiệm. Quyết định TẮT mặc định, chỉ
image Docker chính thức (`docker-compose.yml`) bật cờ này — đúng tinh thần "health-check/liveness
không nên phụ thuộc CSDL". Boc trong `pg_advisory_lock`/`pg_advisory_unlock` (khoá số cố định
`725_190_001`) để nhiều bản API khởi động cùng lúc (rolling update, HA) không đua nhau chạy DDL.

**d) Row-Level Security — chính sách so sánh KIỂU TEXT (`giao_xu_id::text = current_setting(...)`),
KHÔNG ép `current_setting(...)::uuid`.** Thử ép sang uuid trước (hướng "tự nhiên" hơn) và phát
hiện lỗi thật bằng chính bộ test viết cho migration này (`RlsTests.cs`): PostgreSQL NÉM LỖI
ngay khi ép `''::uuid` (chuỗi rỗng — trường hợp chưa đặt tham số phiên) thay vì trả về `NULL`
êm ái như dự đoán ban đầu, khiến MỌI truy vấn (kể cả không match dòng nào) sập với lỗi
`22P02: invalid input syntax for type uuid`. Ép chiều ngược lại (cột `uuid` sang `text`) không
bao giờ lỗi, và so với chuỗi rỗng chỉ đơn giản là không khớp — giữ đúng ý định "đóng mặc định"
(fail-closed: không đặt tham số phiên = không đọc được dòng nào) mà không cần thêm điều kiện
`IS NOT NULL` nào.

**e) Hai vai trò CSDL, không phải một.** Vai trò phục vụ nghiệp vụ hằng ngày (đăng ký ở
`Program.cs`, khoá cấu hình `ConnectionStrings:Qlgx`) PHẢI không có `BYPASSRLS` để RLS thật sự
có tác dụng. Nhưng ba đường dẫn hợp lệ cần truy vấn CHÉO GIÁO XỨ (đăng nhập — tra tên tài khoản
trên toàn máy chủ trước khi biết claim; tạo tài khoản quản trị đầu tiên; công cụ chuyển dữ
liệu) không thể hoạt động dưới một vai trò bị RLS chặn. Thêm khoá cấu hình riêng
`ConnectionStrings:QlgxQuanTri` (fallback về `ConnectionStrings:Qlgx` nếu không đặt — giữ
nguyên hành vi một-vai-trò-duy-nhất ở môi trường dev/test hiện tại, nơi `postgres` superuser
bỏ qua RLS bất kể chính sách gì) cho `AuthService`/`TaoTaiKhoanQuanTri` — gói lại thành một hàm
dùng chung `ChuoiKetNoiQuanTri.Doc()` thay vì lặp lại logic fallback ở hai nơi. `Qlgx.Migration`
(công cụ chuyển dữ liệu) đã nhận chuỗi kết nối qua tham số dòng lệnh từ trước, không cần sửa —
chỉ cần TRIEN-KHAI.md dặn dùng đúng vai trò `qlgx_admin` khi chạy.

**f) Interceptor đặt tham số phiên gắn ở `QlgxDbContext.OnConfiguring`, không phải ở
`Program.cs` lúc đăng ký DI.** Cần vậy vì interceptor phải đọc ĐÚNG `_boiCanh` của TỪNG
INSTANCE (mỗi request một giáo xứ khác nhau) — `AddDbContext` cấu hình một lần cho cả ứng dụng,
không có chỗ nào tự nhiên để "tiêm" một interceptor phụ thuộc instance vào đó. Đặt tham số
phiên lại ở MỌI lần mở kết nối (`ConnectionOpened`/`ConnectionOpenedAsync`), không chỉ một lần
— Npgsql có thể tái dùng một kết nối vật lý đã phục vụ một tenant/instance KHÁC trước đó mà
không tự xoá trạng thái phiên khi trả về pool, nên chỉ đặt một lần lúc khởi tạo có thể để lọt
giá trị cũ sang lượt dùng sau (rủi ro rò tenant thật sự nếu bỏ qua chi tiết này). Khi
`IBoiCanhGiaoXu` đã đăng ký qua DI nhưng đọc `GiaoXuId` NGOÀI một request HTTP thật (ví dụ mã
hạ tầng chạy migration lúc khởi động dùng chung `QlgxDbContext` qua `IServiceScope`) —
`BoiCanhGiaoXuTuNguoiDung` ném `InvalidOperationException` theo đúng thiết kế của nó; interceptor
BẮT lỗi này và coi như "không có bối cảnh" (tham số phiên rỗng, đóng mặc định) thay vì để lỗi
lan ra làm sập một đoạn mã hạ tầng không liên quan gì tới RLS.

**g) Bộ e2e Playwright là một npm project RIÊNG** (`WebApp/e2e`, `package.json`/
`node_modules` độc lập với `WebApp/src/web`) — không gộp vào `src/web` vì Playwright/`pg` là
phụ thuộc CHỈ CẦN lúc kiểm thử đầu-cuối, không nên kéo vào bundle hay `npm install` thường
ngày của ứng dụng thật.

**h) Chuẩn bị database e2e bằng TOP-LEVEL AWAIT ngay trong `playwright.config.ts`, KHÔNG dùng
tuỳ chọn `globalSetup` chuẩn của Playwright.** Thử `globalSetup` trước và phát hiện lỗi thật:
Playwright khởi động (spawn) các tiến trình khai báo trong mảng `webServer` TRƯỚC khi chạy
`globalSetup`, nên API cố kết nối một database chưa được tạo và sập ngay lúc khởi động
(`3D000: database "..." does not exist`). Gọi hàm chuẩn bị (`prepareDb()`) bằng top-level
`await` NGAY TRONG MODULE cấu hình đảm bảo nó chạy xong trước khi Playwright dù chỉ mới ĐỌC
xong mảng `webServer` để quyết định spawn tiến trình nào.

**i) Khoá tệp nguyên tử (`.runtime.lock`, tạo bằng cờ `wx` — thất bại nếu đã tồn tại) để đúng
MỘT tiến trình thực sự chạy `CREATE DATABASE`/migration/tạo tài khoản quản trị, dù
`playwright.config.ts` được NHIỀU tiến trình nạp lại cho cùng một lần chạy `npx playwright
test`** (tiến trình CLI chính lẫn từng worker — kể cả khi ép `workers: 1`, tiến trình worker
vẫn nạp lại file cấu hình). Phát hiện bằng lỗi thật `database "..." already exists` khi chạy
lần đầu không có khoá: nhiều tiến trình cùng sinh đúng một tên database (đọc lại từ
`.runtime.json` đã ghi) rồi cùng chạy `CREATE DATABASE` với tên đó. Tiến trình không giành được
khoá chỉ chờ `.runtime.json` xuất hiện (tối đa 120s) rồi dùng lại đúng bộ giá trị đã ghi.

**j) `dotnet ef database update` cho database e2e phải ghi đè biến `QLGX_TEST_PG` (không phải
`ConnectionStrings__Qlgx`) cho tiến trình con — VÀ VÔ TÌNH GÂY RA MỘT SỰ CỐ THẬT lúc viết task
này, ghi lại đầy đủ vì cần người vận hành/lập trình viên sau biết.** `QlgxDbContextFactory`
(design-time, dùng bởi lệnh `dotnet ef`) đọc THẲNG biến môi trường `QLGX_TEST_PG`, không đi qua
`IConfiguration`/`ConnectionStrings__*` như ứng dụng thật lúc chạy. Bản đầu của script chuẩn bị
database e2e (`prepareDb.ts`) truyền `ConnectionStrings__Qlgx` (SAI biến) cho tiến trình con
`dotnet ef database update` — biến này bị `dotnet ef` bỏ qua hoàn toàn, khiến nó rơi vào chuỗi
kết nối GỐC trong `QLGX_TEST_PG` (không có `;Database=...`), và Npgsql mặc định nối vào
database TRÙNG TÊN VỚI USERNAME khi thiếu `Database=` — tức là toàn bộ 28 bảng nghiệp vụ (RLS
+ tất cả migration) đã bị tạo NHẦM vào database mặc định **`postgres`** dùng chung của cả máy,
không phải một database e2e riêng. Đã phát hiện ngay (kiểm tra `\dt` trên database `postgres`
thấy 28 bảng lạ, toàn bộ RỖNG — xác nhận bằng `SELECT count(*)` trên từng bảng trước khi định
dọn) và sửa gốc rễ (đổi sang ghi đè đúng `QLGX_TEST_PG` kèm `;Database=<ten db e2e>`), đã kiểm
chứng lại bằng một lần chạy thử độc lập rằng bản sửa target ĐÚNG database. **CHƯA DỌN ĐƯỢC 28
bảng rỗng còn sót lại trong database `postgres` trên máy chuẩn bị tài liệu này** — hệ thống
permission của phiên làm việc chặn cả lệnh `DROP TABLE ... CASCADE` qua `psql` lẫn
`dotnet ef database update 0` (rollback bằng chính công cụ EF) nhắm vào database tên
`postgres`, và quyết định KHÔNG lách qua đường khác (ví dụ chạy DROP TABLE bằng một script
Node/`pg` thay vì `psql`) vì đó đúng là kiểu "đổi công cụ để né việc bị chặn" mà hướng dẫn của
hệ thống permission nói rõ là KHÔNG được làm — chỉ dừng lại, ghi rõ, và để người có quyền quyết
định dọn hay không. Việc cần làm: chạy `DROP TABLE IF EXISTS <28 bảng liệt kê bên dưới> CASCADE;`
trên database `postgres` (đã xác nhận cả 28 bảng đều 0 dòng, an toàn xoá) —
`__EFMigrationsHistory, bi_tich_chi_tiet, bo_dem_ma, cau_hinh, chi_tiet_hoi_doan,
chi_tiet_lop_giao_ly, chuyen_xu, dot_bi_tich, du_lieu_chung, gia_dinh, giao_dan,
giao_dan_hon_phoi, giao_hat, giao_ho, giao_ly_vien, giao_phan, giao_xu, hoi_doan, hon_phoi,
khoi_giao_ly, linh_muc, lop_giao_ly, rao_hon_phoi, tai_khoan, tan_hien, ten_loai_tai_khoan,
thanh_vien_gia_dinh, vai_tro`. **Bài học rút ra cho lần sau:** không bao giờ dùng một chuỗi kết
nối THIẾU `Database=` khi truyền cho bất kỳ công cụ nào, kể cả khi "chắc chắn nó không đọc biến
đó" — thà đặt cả hai biến (`QLGX_TEST_PG` VÀ `ConnectionStrings__Qlgx`) cùng trỏ đúng một
database, dư một biến không hại gì.

**k) `TabDocs.tsx` giữ MỌI thẻ đã mở trong DOM (ẩn bằng thuộc tính `hidden`, không unmount) —
gây ra một bẫy thật cho bộ e2e vì các trường trong form dùng `id`/`<label for>` TĨNH (`gd-hoten`,
`gd-ghichu`, `gdinh-them-thanhvien`, …).** Khi giáo dân/gia đình vừa tạo mở một thẻ MỚI (giáo
dân) trong khi thẻ nháp cũ vẫn còn (ẩn), hai thẻ cùng render một cặp `id` trùng nhau — trình
duyệt phân giải `<label for="...">` theo ID ĐẦU TIÊN trong toàn tài liệu bất kể ẩn/hiện, nên
`getByLabel(...)` có thể trúng đúng phần tử nằm trong thẻ ẨN, và chờ nó "hiện ra" là chờ vô hạn
(đã đo bằng debug thủ công: `getBoundingClientRect()` trả `{w:0,h:0}`, tổ tiên gần nhất có
`display:none`). Sửa bằng cách đóng thẻ nháp cũ ngay sau khi lưu thành công (hàm dùng chung
`themGiaoDanMoi` trong `dangNhap.ts`) — CHỈ áp dụng cho GIÁO DÂN, vì tạo GIA ĐÌNH mới lại hoạt
động khác hẳn: `GiaDinhDetailPage.taoMoi()` cập nhật `idThat` NGAY TRÊN CÙNG một thẻ, không mở
thẻ thứ hai — thử áp dụng cùng cách "đóng thẻ nháp" cho gia đình đã đóng NHẦM chính thẻ vừa tạo
(vì nó chính là thẻ đang mở, không phải một thẻ cũ còn sót), làm mất luôn nội dung vừa tạo.
Đây là gợi ý sửa THẬT SỰ nên cân nhắc cho ứng dụng (không thuộc phạm vi Task 13, không đổi logic
nghiệp vụ): `id` tĩnh lặp lại giữa các thẻ tài liệu đang mở đồng thời là HTML không hợp lệ (vi
phạm tính duy nhất của `id`) và có thể gây nhầm lẫn tương tự cho người dùng công cụ hỗ trợ tiếp
cận (screen reader), không chỉ cho Playwright.

**l) Nút "×" đóng thẻ tài liệu có TÊN TRUY CẬP (accessible name) là chính ký tự "×", KHÔNG phải
`title="Đóng thẻ"`.** Theo đặc tả tính tên truy cập (accname), nội dung text con của một phần
tử tương tác luôn được ưu tiên hơn thuộc tính `title` — `title` chỉ được dùng làm tên truy cập
khi phần tử KHÔNG có nội dung text nào khác. `getByRole('button', {name: 'Đóng thẻ'})` do đó
không bao giờ khớp; phải dùng `{name: '×'}`.

**m) Xoá thành viên khỏi gia đình qua menu chuột phải phải bắn thẳng sự kiện DOM
`contextmenu` (`locator.dispatchEvent('contextmenu', ...)`), KHÔNG dùng click chuột phải thật
(`locator.click({button: 'right'})`), kể cả kèm `force: true`.** Thanh công cụ "Thêm thành
viên" (ô picker + nút "Thêm vào gia đình") nằm NGAY TRÊN lưới thành viên trong cùng bố cục, và
ở đúng vị trí cuộn cần thiết để thấy dòng vừa thêm, trình duyệt định tuyến sự kiện CHUỘT THẬT
theo toạ độ màn hình cho bất cứ thứ gì thực sự vẽ đè lên trên tại toạ độ đó — `force: true` của
Playwright chỉ bỏ qua bước Playwright TỰ kiểm tra trước khi click, không đổi được cách Chrome
tự bắt sự kiện chuột thật, nên vẫn nhấp trúng lớp phủ chứ không trúng dòng lưới. Bắn thẳng sự
kiện DOM lên đúng phần tử né hoàn toàn việc dò toạ độ, và khớp đúng cách `GxGrid.tsx` đọc sự
kiện (bắt ở `.grid-wrap`, dò `e.target.closest('.ag-row')` — không cần toạ độ chuột thật, chỉ
cần `e.target` nằm trong đúng hàng). Sau khi menu chuột phải hiện, còn phải bấm thêm nút "Yes"
của một hộp thoại xác nhận riêng (`GxHoiDap`, tái hiện đúng `gxAddEdit1_DeleteClick` gốc — hỏi
Yes/No trước khi xoá vĩnh viễn) — click "Xoá khỏi gia đình" trên menu CHỈ MỞ hộp thoại này,
chưa xoá ngay.

**n) Danh sách (giáo dân/gia đình) không tự tải lại khi quay về một thẻ ĐÃ MỞ SẴN từ trước —
`useEffect(tai, [tai])` chỉ chạy một lần lúc mount, không chạy lại khi tab chỉ đổi hiển thị/ẩn
(TabDocs không unmount/remount khi chuyển thẻ, xem mục k).** Test tạo một giáo dân/gia đình rồi
quay lại thẻ danh sách ĐÃ MỞ TỪ TRƯỚC đó (mở lúc đăng nhập hoặc lúc bắt đầu tạo bản ghi) sẽ thấy
đúng ảnh chụp CŨ (trước khi tạo) — không phải bug ứng dụng (đúng hành vi lưới nạp một lần, giống
bản desktop), chỉ là điều bộ e2e phải biết: bấm "Tải lại" trên thanh công cụ trước khi tìm dòng
mới trên một thẻ danh sách có khả năng đã mở từ trước, hoặc dùng `page.reload()` (tải lại toàn
trang) nếu muốn kiểm luôn cả việc dữ liệu đã thật sự nằm trong CSDL (không chỉ còn trong state
React) — cả hai cách đều xuất hiện trong bộ test tuỳ tình huống.

**o) Trường "Ngày sinh" của giáo dân là BẮT BUỘC theo đúng nghiệp vụ gốc (Rule 6,
`frmGiaoDan.cs:308-313`, tái hiện ở `GiaoDanService.KiemTraNghiepVu`)** — `dtNgaySinh.
CheckInput(false)` của bản desktop nghĩa là "phải nhập", không phải tuỳ chọn như test ban đầu
giả định. Thiếu trường này, `POST /api/giao-dan` trả lỗi nghiệp vụ "Hãy nhập ngày sinh hợp lệ"
(hiển thị đúng ở thanh trạng thái cuối form, `role="status"`) và form không bao giờ chuyển sang
chế độ đã lưu — không phải lỗi ứng dụng, là bộ test ban đầu thiếu bước điền trường bắt buộc.

**p) Nhãn CHÍNH XÁC của ô ghi chú chung trên form giáo dân là "Ghi chú chung", không phải
"Ghi chú".** Không dùng `exact: true` (hoặc dùng sai nhãn), `getByLabel('Ghi chú')` khớp NHẦM
một ô khác ("Ghi chú xức dầu" ở tab Bí tích, cũng render sẵn trong DOM dù tab đó chưa được
chọn) đứng trước trong thứ tự DOM.

**q) Docker đã kiểm chứng THẬT trên máy chuẩn bị tài liệu này** (Docker 29, Docker Desktop trên
Windows) — không chỉ đọc `Dockerfile` rồi suy luận. Đã build image, chạy `docker compose up`
với một `.env` thử nghiệm (mật khẩu/khoá JWT giả, KHÔNG phải giá trị thật, KHÔNG commit), xác
nhận: `/api/suc-khoe` trả `ok` qua cổng map ra ngoài, migration (kể cả migration bật RLS) tự
chạy đúng lúc container khởi động và log rõ ràng, `relrowsecurity = t` trên bảng `giao_ho` bên
trong container xác nhận RLS thật sự bật trong môi trường Docker (không chỉ trên máy dev chạy
`dotnet run` trực tiếp), và route tĩnh (`/`) trả về `index.html` của SPA đã build. Đã dọn sạch
sau khi thử (`docker compose down -v`, xoá image, xoá `.env` thử nghiệm) — không để lại
container/volume/image nào chạy nền.

### 30. Task "sửa các phát hiện từ review-frontend" (2026-09-07) — các quyết định tự đưa ra

Bốn việc theo `review-frontend.md`: (1) `GxPicker` nuốt lỗi mạng, (2) độ phủ test của
`nguoiCu.ts`, (3) Chủ hộ chưa nối, (4) tab Giáo lý + khối chuyển xứ tĩnh. Ghi lại các chỗ phải
tự quyết vì không có ai hỏi được ngay lúc làm.

**a) `GxPicker` — chỉ phân biệt lỗi/rỗng ở chính nó, KHÔNG lan việc này ra mọi `.catch` khác
trong `src/`.** Rà toàn bộ `.catch` còn lại (`AuthContext.tsx:58`, các `.catch` tải danh mục
Giáo họ/Hội đoàn ở `GiaDinhDetailPage.tsx`/`GiaDinhListPage.tsx`/`GiaoDanDetailPage.tsx`/
`GiaoDanListPage.tsx`): tất cả đều CHỈ `console.error` rồi để state rỗng cho một DANH MỤC hỗ
trợ (dropdown Giáo họ/Hội đoàn) — nếu lỗi, dropdown hiện trống nhưng KHÔNG hiển thị một thông
điệp giả kiểu "0 kết quả hợp lệ" đánh lừa người dùng nghĩ đó là dữ liệu thật (khác hẳn kịch bản
`GxPicker` gây ra thao tác nghiệp vụ sai — tạo bản ghi trùng). `AuthContext.tsx:58`
(`.catch(() => authStore.xoaToken())`) có vẻ giống nhưng đổi ý sau khi đọc kỹ: một test đã có
từ trước (`token cu khong con hop le thi xoa token...`) cố ý mock `api.auth.toi()` reject bằng
`Error` chung (không phải lỗi mạng thật) và assert phải đăng xuất — đổi hành vi này (chỉ đăng
xuất khi lỗi thật sự là 401) sẽ phá vỡ một quyết định thiết kế đã chốt trước đó ("mọi lỗi khi
xác thực token cũ = coi như hết hạn, đăng xuất") mà không có bằng chứng nó sai. Quyết định: chỉ
sửa `GxPicker`, không đụng các `.catch` khác — nếu người dùng thấy còn chỗ khác cần sửa, xin
chỉ rõ để làm riêng, tránh đổi hành vi đã test mà không xin phép.

**b) `nguoiCu.ts` — CHỈ thêm test, KHÔNG sửa file nguồn.** Review chỉ ra thiếu độ phủ, không
chỉ ra lỗi logic; đối chiếu lại `Source/GXControl/frmGiaDinh.cs:649-919` (UTF-16LE) xác nhận
cấu trúc if/else của `chayCayQuyetDinhNguoiCu` khớp đúng bản gốc. **Bằng chứng đảo điều kiện**
(làm rồi khôi phục lại, không giữ trong commit): đảo `if (yes)` → `if (!yes)` lần lượt ở dòng
92, 99, 121, và vô hiệu hoá điều kiện nhánh 103-112 (đổi thành `if (false)`) — cả 4 lần chạy lại
`npx vitest run src/lib/nguoiCu.test.ts` đều cho ít nhất 1 test ĐỎ sau khi bổ sung test mới (2,
2, 2, 3 test lần lượt thất bại), tất cả xanh trở lại sau khi khôi phục nguyên văn dòng đã sửa
(xác nhận bằng `diff` với bản ở HEAD — không có sai khác).

**c) Chủ hộ — chỉ ràng buộc "chỉ Chồng/Vợ được làm chủ hộ" (đúng desktop: hai radio
`rdChuHoNam`/`rdChuHoNu` là cách DUY NHẤT gán `ChuHo`), KHÔNG migrate chuỗi hộp thoại Yes/No tự
động sửa chủ hộ khi một bên "Qua đời" (`cbChuHoNam_CheckedChanged` cùng khối kiểm tra ở nút Lưu,
`frmGiaDinh.cs:1360-1433`).** Đây là quyết định có chủ đích, không phải bỏ sót: chuỗi hộp thoại
đó là GỢI Ý tiện lợi (chọn [No] vẫn giữ nguyên lựa chọn hiện tại, không có gì bị chặn nếu bỏ
qua toàn bộ chuỗi hỏi), khác hẳn tính chất "chặn cứng" của chính việc có/không có chủ hộ. Ghi
lại để review sau nếu người dùng muốn có đủ. Giá trị `chuHoVaiTro` gửi lên PHẢN ÁNH ĐÚNG trạng
thái hiện tại của cặp radio (kể cả khi không ai được chọn) — máy chủ ghi lại y hệt, không tự
suy đoán/giữ giá trị cũ, khớp đúng cách desktop ghi `rdChuHoNam.Checked`/`rdChuHoNu.Checked`
KHÔNG ĐIỀU KIỆN vào mỗi lần Lưu (dòng 1731/1739).

**d) Tab Giáo lý — đổi ô "Người cấp chứng nhận" từ `GxPicker` (chọn giáo dân) sang ô nhập văn
bản thường.** Bản trước dựng nhầm bằng `GxPicker` dù `NguoiChungNhanGLHN` trên `GiaoDan`/
`GiaoDanDetailDto` là `string?` (đối chiếu `txtGLHNNguoiCap.Text` ở `frmGiaoDan.cs:1038` — ô
văn bản tự do, KHÔNG liên kết một giáo dân khác) — không có cách nào gắn `onChon` hợp lệ cho
một picker khi giá trị lưu là chuỗi, không phải khoá ngoại. Đổi sang `<input type="text">` để
nối được vào payload; đồng thời bổ sung ô "Xếp loại" (`cbGLHNXepLoai`: Trung Bình/Khá/Giỏi) mà
bản trước thiếu hẳn (không tìm thấy trong bố cục cũ, dù trường `XepLoaiGLHN` đã có trong DTO).

**e) Khối "Thông tin chuyển xứ" — chỉ nối SỬA (`PUT`), KHÔNG nối `TaoGiaoDanRequest` (tạo
mới).** Một giáo dân mới tạo chưa có lịch sử chuyển xứ; nối được sau bằng một lượt Sửa. Sửa
tại chỗ MỘT dòng `ChuyenXu` hiện có thay vì luôn tạo dòng lịch sử mới mỗi lần đổi loại — đúng
`GetChuyenXuInfo` (`frmGiaoDan.cs:892-923`, cập nhật tại chỗ nếu `currentRow` đã có
`MaChuyenXu`); chọn "Ở tại xứ" (`LoaiChuyen=0`) XOÁ HẲN dòng hiện có — đúng
`cbChuyenXu.SelectedValue==0` (dòng 719-727 xoá hẳn khỏi bảng `ChuyenXu`), không giữ lại một
dòng "rỗng". **Cố ý BỎ QUA** hộp thoại cảnh báo Yes/No/Cancel khi đổi TỪ một loại chuyển xứ đã
lưu VỀ "Ở tại xứ" (`cbChuyenXu_SelectedIndexChanged`, dòng 1231-1245) — tiện ích UX (chọn [No]
chỉ phục hồi lựa chọn cũ, không có gì bị mất nếu bỏ qua), không phải ràng buộc dữ liệu; xử lý
phía máy chủ đã đúng bất kể có hộp thoại này hay không. **Cố ý ĐƠN GIẢN HOÁ** cách hiện/ẩn ba ô
Ngày chuyển/Nơi chuyển/Ghi chú: cả ba cùng hiện khi khác "Ở tại xứ" — bản gốc có một lỗi/quái dị
là `dtNgayChuyen`/`txtGhiChuChuyenXu` không được set lại `Visible=true` khi đổi từ "Ở tại xứ"
sang loại khác (dòng lệnh bị comment, chỉ `txtGiaoXuChuyen.Visible=true` thật sự chạy) — tái
hiện đúng lỗi này sẽ khiến người dùng không nhập được Ngày chuyển/Ghi chú sau khi từng chọn "Ở
tại xứ" một lần, một cách hành xử gây khó chịu không có giá trị nghiệp vụ; chọn hành vi "hợp lý"
(hiện đủ cả ba) thay vì tái hiện lỗi vô nghĩa này.

**f) Xác nhận qua chạy thật (không chỉ test)**: đăng nhập bằng tài khoản quản trị TẠO RIÊNG cho
lượt kiểm thử này (`claudetest`, xoá ngay sau khi xong — KHÔNG dùng/đụng tới mật khẩu của
`quantri` có sẵn); đổi chủ hộ gia đình mã 4 (Giuse Phạm Văn Trường) từ Chồng sang Vợ, tải lại
trang, xác nhận qua `psql` cột `chu_ho` đã đổi đúng, rồi ĐẶT LẠI như cũ; nhập tab Giáo lý và
khối chuyển xứ cho giáo dân mã 1 (Nguyễn Đức Mạnh), xác nhận qua `psql`, rồi XÓA SẠCH dữ liệu
thử để trả `qlgx_thu` về đúng 2050 giáo dân/40 gia đình/145 thành viên (xác nhận lại bằng
`psql` sau khi dọn). Ảnh chụp: `WebApp/anh-chup-kiem-thu/42-44*.png`.
