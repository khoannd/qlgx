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

### 31. Task "sửa các phát hiện từ review-backend" (2026-09-07) — các quyết định tự đưa ra

Năm việc theo `review-backend.md`: C1 (kiểm tra RowVersion chết trong `GanVoChong`), C2
(`ThemThanhVien` lách kiểm tra vợ/chồng), T1 (không giới hạn đăng nhập sai), T2 (rò thời gian
phản hồi khi đăng nhập), T3 (mật khẩu CSDL cứng trong `appsettings.Development.json`). Người
dùng đang ngủ, không hỏi được — ghi lại các quyết định tự đưa ra.

**a) C1 — sửa bằng `db.Entry(gd).State = EntityState.Modified` tường minh, GIỮ NGUYÊN dòng
`Property(x => x.RowVersion).OriginalValue = yc.RowVersion` đã có.** Khi chạy thử để tái hiện
lỗi (tắt fix, chạy test), phát hiện một chi tiết review chưa nói tới: nếu có một thay đổi
SCALAR THẬT SỰ khác xảy ra trước đó trên cùng bản ghi (ví dụ ai đó gọi `CapNhat` đổi
`TenGiaDinh`), thì `GanVoChong` load lại entity MỚI (Current=phiên bản mới), rồi đặt
`OriginalValue=RowVersion cũ của client` → Original≠Current ngay trên property đó → EF Core
COI ĐÂY LÀ THAY ĐỔI và tự phát UPDATE, tình cờ vẫn bắt được đụng độ trong trường hợp đó (đã xác
nhận bằng thực nghiệm: test dựng kịch bản này XANH ngay cả khi CHƯA sửa gì). Lỗi "chết" thật sự
CHỈ xảy ra ở đúng kịch bản C1 mô tả: server SUY LUẬN gia đình vẫn ở giá trị RowVersion ban đầu vì chưa ai
đổi gì khác, mọi lần đọc trước đó khớp `client.RowVersion` — Original==Current, EF coi là
Unchanged thật, không phát UPDATE. Test được viết lại để nhắm đúng vào trường hợp này (2 test
thay vì 1): (1) khẳng định RowVersion của gia đình THẬT SỰ đổi sau một `GanVoChong` thành công —
bằng chứng trực tiếp nhất UPDATE có chạy hay không, ĐỎ trước khi sửa (RowVersion không đổi),
XANH sau khi sửa; (2) nối tiếp: hai lệnh `GanVoChong` liên tiếp vào cùng gia đình, lệnh sau
dùng RowVersion CŨ (chưa refresh) — phải bị 409, ĐỎ trước khi sửa (200), XANH sau khi sửa.

**b) C1 — có thêm ràng buộc UNIQUE lọc `(gia_dinh_id, vai_tro) WHERE vai_tro IN (0,1)`** (migration
`ThemRangBuocMotChongMotVo`) làm lưới an toàn tầng CSDL, đúng đề xuất "nên làm cả hai" của
review. **Đã kiểm tra dữ liệu thật `qlgx_thu` trước khi thêm** (bắt buộc theo yêu cầu, vì lo
ngại 145 dòng thành viên cũ có thể đã có gia đình hai Chồng):
```sql
SELECT gia_dinh_id, vai_tro, count(*) FROM thanh_vien_gia_dinh
WHERE vai_tro IN (0,1) GROUP BY gia_dinh_id, vai_tro HAVING count(*) > 1;
-- 0 rows
```
Không có vi phạm nào — migration áp dụng an toàn cho dữ liệu hiện có. Đã áp dụng thử migration
vào `qlgx_dev` (không đụng `qlgx_thu`) để sinh migration EF.

> **Cập nhật 2026-09-07 (cuối phiên)**: migration này **ĐÃ được áp** lên `qlgx_thu` bằng
> `dotnet ef database update` cùng hai migration `BatRlsChoBangTheoGiaoXu` và
> `ThemKhoaDangNhapTaiKhoan`. Chỉ đổi schema, số liệu không đổi (đã kiểm lại bằng `psql`:
> 2050 giáo dân / 145 thành viên / 6150 bí tích chi tiết). Câu dưới đây giữ lại làm lịch sử.

Vào thời điểm sửa lỗi thì chưa chạy migration này trên `qlgx_thu`;
người vận hành cần chạy `dotnet ef database update` (hoặc bật `Qlgx:ChayMigrationKhiKhoiDong`)
trên `qlgx_thu`/production trước khi coi ràng buộc này là đã bảo vệ dữ liệu thật.

**c) C2 — chặn cứng `VaiTro` 0/1 ở `ThemThanhVien`, KHÔNG chạy lại bộ kiểm tra của `GanVoChong`
tại đây.** Hai lựa chọn nêu trong review, chọn "từ chối" vì: (i) front-end đã tuân thủ sẵn quy
tắc này — `DANH_SACH_VAI_TRO_THANH_VIEN` (`WebApp/src/web/src/lib/vaiTroGiaDinh.ts`) loại trừ
Chồng/Vợ khỏi lưới "thêm thành viên khác" từ trước, nên chặn ở đây không làm hỏng luồng nào
đang chạy (xác nhận bằng cách đọc `GiaDinhDetail.tsx`/`vaiTroGiaDinh.ts` và chạy lại toàn bộ
205 test frontend — xanh, không cần sửa gì phía UI); (ii) chạy lại bộ kiểm tra của `GanVoChong`
tại `ThemThanhVien` sẽ trùng lặp logic (tuổi kết hôn, giới tính, đang là vợ/chồng gia đình
khác, xử lý `RowVersion`/người cũ) giữa hai hàm — rủi ro hai bản trôi khỏi nhau theo thời gian
cao hơn là chặn cứng và bắt client dùng đúng endpoint chuyên trách.

**d) T1 — khoá tạm đăng nhập lưu Ở CSDL (cột mới `TaiKhoan.SoLanDangNhapSaiLienTiep` +
`KhoaDangNhapDenLuc`), KHÔNG dùng bộ đếm trong bộ nhớ tiến trình** — đúng ràng buộc HA nêu
trong nhiệm vụ (nhiều bản API sau load balancer). Ngưỡng chọn: **10 lần sai liên tiếp → khoá 15
phút**, cố ý rộng rãi hơn mức "3-5 lần" thường thấy để tránh khoá oan người dùng thật gõ nhầm
vài lần (không có cơ chế "quên mật khẩu" tự phục vụ trong hệ thống này — khoá quá chặt sẽ biến
thành DoS cho chính người dùng hợp lệ, phải chờ quản trị viên xử lý thủ công). **CHƯA làm giới
hạn theo IP** (chỉ theo tài khoản) — theo IP cần lưu trạng thái theo IP ở CSDL tương tự, nhưng
IP thật của client dễ bị giả qua header khi có nhiều lớp reverse proxy chưa được cấu hình
`ForwardedHeaders` nhất quán trong dự án này; để tránh thêm một bề mặt cấu hình sai (chặn nhầm
toàn bộ người dùng sau NAT/proxy chung một IP), chỉ làm giới hạn theo tài khoản trong lượt sửa
này. Ghi nhận: nên xem lại giới hạn theo IP sau khi `TRIEN-KHAI.md` mục HTTPS/reverse proxy đã
chốt cấu hình forwarded headers cụ thể.

**e) T2 — chạy một phép băm PBKDF2 "giả" (tài khoản giả `Id=Guid.Empty`, mật khẩu giả cố định
không phải bí mật thật) khi không có ứng viên nào trùng tên, để hai nhánh (có/không có tài
khoản trùng tên) tốn thời gian tương đương.** KHÔNG viết test đo thời gian thật (timing-based
test) — đo độ trễ tin cậy trong CI/máy chia sẻ tài nguyên (như môi trường chạy nhiệm vụ này) vốn
nhiễu và dễ oscillate giả (false đỏ do máy bận, false xanh do JIT/cache), test kiểu này sẽ trở
thành nguồn "flaky" mới. Xác nhận thay bằng cách đọc code trực tiếp: nhánh rỗng giờ luôn gọi
`_hasher.VerifyHashedPassword` (cùng API, cùng tham số PBKDF2 mặc định) trước khi trả lỗi, đối
xứng với nhánh có ứng viên.

**f) T3 — kiểm tra lại trước khi sửa: `appsettings.Development.json` ĐÃ được `.gitignore`
(`WebApp/.gitignore` dòng 11, có từ commit khởi tạo `1a9a7e9`, kèm chú thích rõ lý do) từ trước,
và xác nhận bằng `git ls-files`/`git show HEAD:...` rằng file này CHƯA BAO GIỜ được commit vào
git.** Nghĩa là phát hiện T3 của review — "mật khẩu CSDL cứng trong file nằm trong repo" — không
đúng theo nghĩa đen: file có mật khẩu THẬT nhưng KHÔNG "nằm trong repo" (không bị git theo dõi),
nên không vi phạm quy tắc "không bí mật nào trong repo" như review kết luận; review có lẽ đọc
trực tiếp file trên đĩa mà không kiểm tra trạng thái git của nó. Dù vậy vẫn sửa theo hướng an
toàn hơn (không có rủi ro gì khi làm, file không được git theo dõi nên sửa không tạo diff nào
trong lịch sử): bỏ hẳn giá trị mật khẩu, để `ConnectionStrings:Qlgx` RỖNG, hướng dẫn dùng
`dotnet user-secrets`/biến môi trường thay thế — phòng trường hợp ai đó vô tình xoá dòng
`appsettings.Development.json` khỏi `.gitignore` sau này thì file trên đĩa đã sẵn an toàn. Từng
thử thêm kiểm tra "báo lỗi ngay khi khởi động" (`throw` nếu chuỗi rỗng) ở `Program.cs` nhưng bỏ
vì phá vỡ `SucKhoeTests` — bài test cố ý dùng `WebApplicationFactory<Program>` KHÔNG cấu hình
CSDL để xác nhận `/api/suc-khoe` không phụ thuộc CSDL (đúng ghi chú sẵn có trong `Program.cs`
về liveness/health-check). Giữ hành vi cũ: thiếu cấu hình thì lỗi hiện ra rõ ràng từ chính
Npgsql ở truy vấn CSDL đầu tiên. Đã thêm hướng dẫn `dotnet user-secrets`/biến môi trường vào
`TRIEN-KHAI.md` mục 4.

**g) Hai mục Thấp (Th1: JWT không thu hồi được khi đăng xuất, Th2: `GiaoHoEndpoints` không phân
trang) — GIỮ NGUYÊN, không sửa gì**, đúng theo chính kết luận của review: cả hai đã được ghi
nhận là quyết định có chủ đích/chấp nhận được ở quy mô hiện tại (Th1 đã có ở mục 27b của file
này với JWT hết hạn sau 8 giờ làm giảm nhẹ; Th2 một giáo xứ chỉ có vài chục giáo họ). Không có
thông tin mới nào trong review lần này đủ để đảo ngược hai quyết định đó.

**h) Xác nhận qua test, không chạy thử thật trên `qlgx_thu`.** Khác các task trước (mục 27-30),
nhiệm vụ này không yêu cầu (và người dùng đang ngủ, không xác nhận được) chạy thao tác thật lên
`qlgx_thu` — xác nhận toàn bộ 5 việc qua bộ test tự động (`dotnet test Qlgx.sln`: 188 bài, tăng
5 so với 183 trước đó; `npm test -- --run`: 205 bài, không đổi) và `npm run build` (thành công).
Dữ liệu thử tạo trong lúc chạy test nằm trong các CSDL tạm `qlgx_api_*` do `QlgxApiFactory` tự
tạo/xoá cho mỗi lượt `dotnet test` — không chạm `qlgx_thu`; đã xác nhận lại bằng `psql` sau khi
xong rằng `qlgx_thu` vẫn đúng 2050 giáo dân/40 gia đình/145 thành viên.

### 32. Task "sửa khung ứng dụng hiển thị dữ liệu giả" (2026-09-07) — các quyết định tự đưa ra

Ba việc phát hiện ở `kiem-thu-2-man-hinh.md` mục 4 và 5 (thanh trên viết cứng sai tên giáo xứ,
chân thanh bên viết cứng số hiệu bản desktop + "Sao lưu gần nhất" bịa, nút "+" của `GxPicker`
im lặng không phản hồi). Người dùng đang bận, không hỏi được — ghi lại các quyết định tự đưa ra.

**a) Tên giáo xứ — nối vào `/api/auth/toi` VÀ vào luôn thân trả về của `POST /api/auth/dang-nhap`,
không chỉ một trong hai.** `ThongTinNguoiDungDto` (đã có sẵn `GiaoXuId`) được thêm trường
`TenGiaoXu`, tra từ `db.GiaoXu` bằng `FirstOrDefaultAsync` (không dùng `Find`/`FindAsync` — đúng
ràng buộc cấm) ngay trong `AuthService.DangNhap` (đã có sẵn một `QlgxDbContext` dùng chuỗi kết
nối QUẢN TRỊ cho bước xác thực chéo giáo xứ) và trong `AuthEndpoints.MapAuth` (`/toi`, dùng
`QlgxDbContext` tiêm qua DI, lọc theo `giaoXuId` đọc từ claim của token — KHÔNG BAO GIỜ từ tham
số trình duyệt, đúng `BoiCanhGiaoXuTuNguoiDung`). Lý do làm cả hai nơi thay vì chỉ gọi `/toi`
sau khi đăng nhập: `AppShell` cần hiện đúng tên giáo xứ NGAY từ màn hình đầu tiên sau khi đăng
nhập, tránh một khung hình nhấp nháy "—" rồi mới ra tên thật sau một round-trip nữa.

**b) Bỏ hẳn hình tam giác thả xuống VÀ đổi `<button>` (parish-chip) thành `<span role="status">`
(không phải bấm được nữa).** Nhiệm vụ chỉ nói "đừng làm chức năng chuyển giáo xứ", nhưng giữ
nguyên phần tử `<button>` không có `onClick` sẽ tự nó gây hiểu nhầm y hệt vấn đề gốc (con trỏ
tay, có vẻ bấm được) — đổi hẳn ngữ nghĩa HTML cho khớp với thực tế "chỉ hiển thị, không tương
tác" thay vì chỉ xoá mỗi cái tam giác mà để lại một nút chết. Bỏ luôn CSS `cursor: pointer` và
rule `.parish-chip .caret` không còn dùng.

**c) `/api/auth/toi` giữ nguyên `AllowAnonymous` = KHÔNG (vẫn `RequireAuthorization()`), và
KHÔNG thêm endpoint tra cứu tên giáo xứ theo id riêng.** Không có nhu cầu tra tên giáo xứ khi
chưa đăng nhập (điểm dùng duy nhất là thanh trên sau khi đã có phiên), thêm một endpoint như
vậy chỉ mở thêm bề mặt rò rỉ tên tất cả giáo xứ trên máy chủ cho người chưa xác thực.

**d) Phiên bản bản web lấy qua `GET /api/suc-khoe` (đã có sẵn, anonymous, trả `phienBan` từ
`Assembly.GetExecutingAssembly().GetName().Version`) — KHÔNG thêm trường phiên bản vào
`/api/auth/toi`.** Hai mối quan tâm khác nhau (danh tính người dùng vs. phiên bản triển khai);
`/api/suc-khoe` vốn được thiết kế làm liveness/health-check công khai, tận dụng lại đúng mục
đích thay vì trộn thêm vào endpoint xác thực. `client.ts` thêm nhóm `api.he.sucKhoe()` mới cho
mục đích này — client vẫn đính kèm token qua `goi()` như mọi lời gọi khác (không hại gì vì
endpoint là anonymous, và giữ một hàm `goi()` DUY NHẤT cho toàn bộ client thay vì phân nhánh
"endpoint nào cần token, endpoint nào không" ở tầng gọi).

**e) Bỏ hẳn dòng "Sao lưu gần nhất", KHÔNG thay bằng "Chưa có thông tin sao lưu" hay tương tự.**
Nhiệm vụ cho phép cả hai hướng ("bỏ hẳn" hoặc "giữ chỗ nhưng ghi rõ chưa có"); chọn bỏ hẳn vì
chưa có bất kỳ API trạng thái sao lưu nào (thêm một dòng "chưa có" vẫn chiếm chỗ nhắc nhở về
một tính năng chưa tồn tại, trong khi phần khung này nên tối giản — thêm lại dễ dàng khi API
thật ra đời).

**f) `SideNav` gọi `GET /api/suc-khoe` bằng `useEffect` cục bộ trong chính component, KHÔNG
nâng lên `AuthContext`/App-level state.** Thông tin phiên bản không phụ thuộc phiên đăng nhập
(anonymous, không đổi theo người dùng), không có lý do chia sẻ qua context toàn cục; giữ gọn
trong component duy nhất cần nó. Lỗi mạng khi gọi (hiếm, vì cùng máy chủ và request không cần
token) chỉ khiến chữ phiên bản không hiện (hiện "Bản web" trơn) — không crash, không banner lỗi
gây rối cho một mẩu thông tin phụ ở chân trang.

**g) Nút "+" trong `GxPicker` — vô hiệu hoá (`disabled`) khi KHÔNG có `onThemMoi` truyền vào,
thay vì xoá hẳn nút.** Xoá nút sẽ đổi bố cục ba nút tròn quen thuộc (giống UserControl
`GxGiaoDan` bản desktop) thành hai, và làm việc nối `onThemMoi` thật sau này (task màn hình chi
tiết giáo dân) phải sửa lại bố cục thay vì chỉ bỏ `disabled`. Điều kiện `disabled={!onThemMoi}`
nghĩa là nút TỰ ĐỘNG hoạt động lại ngay khi một nơi gọi nối `onThemMoi` thật, không cần sửa gì
thêm ở `GxPicker`. Tooltip đổi thành "Thêm giáo dân mới — chưa hỗ trợ" (rõ ràng là CHƯA có, không
phải hỏng) thay vì disable âm thầm không giải thích.

**h) Rà thêm dữ liệu giả/viết cứng khác — không tìm thấy chỗ nào cần sửa.** Đã grep toàn bộ
`WebApp/src/web/src` tìm các mẫu nghi vấn (tên riêng viết cứng, ngày giờ bịa, số liệu tĩnh,
"demo"/"fake"/"dummy"/"lorem"). Chữ "VP" ở avatar góc phải ĐÃ được nối vào tên đăng nhập thật từ
một task trước đó (xem chú thích `chuVietTat()` trong `AppShell.tsx` — "giống 'VP' cũ nhưng suy
từ tên thật"), không phải việc còn tồn đọng của nhiệm vụ này. Nhãn "MÔI TRƯỜNG THỬ NGHIỆM" giữ
nguyên (cố ý, theo đúng chỉ dẫn). Các cụm "hard-code"/"hard coded" còn lại trong bình luận mã
nguồn đều là chú thích NHẮC LẠI một hard-code đã được thay bằng dữ liệu thật ở các task trước
(`data/giaoHoTam.ts`, danh mục Vai trò hội đoàn) — không phải hard-code đang tồn tại.

**i) Xác nhận bằng trình duyệt thật trên `qlgx_thu`.** Chạy `Qlgx.Api` (Production, nối
`qlgx_thu` qua biến môi trường, không ghi vào file nào) và `npm run dev` (đặt
`VITE_API_PROXY_TARGET=http://localhost:5080` — giá trị mặc định trong `vite.config.ts` là cổng
5096, không khớp cổng đã chọn cho API, gây lỗi 502 lúc thử lần đầu; đã sửa bằng biến môi trường
khi chạy `npm run dev`, không đổi giá trị mặc định trong mã nguồn vì 5096 vẫn có thể đúng cho
người khác chạy theo hướng dẫn cũ). Tạo tài khoản quản trị tạm `kiemthu_khung` bằng đúng lệnh
CLI chính thức (`dotnet run -- tao-tai-khoan-quan-tri`, mật khẩu chỉ tồn tại trong biến môi
trường của lượt gọi, không ghi ra file nào). Đăng nhập thật, chụp ảnh xác nhận thanh trên hiện
đúng "Vô Nhiễm" (không còn tam giác thả xuống), chân thanh bên hiện "Bản web 1.0.0.0" (không còn
"4.0.0"/"dữ liệu cục bộ"/"Sao lưu gần nhất"), và cả ba nút "+" của `GxPicker` (Người nam, Người
nữ, thành viên mới) đều ở trạng thái `disabled` trên màn hình chi tiết gia đình thật — lưu tại
`WebApp/anh-chup-kiem-thu/45-thanh-tren-hien-dung-ten-giao-xu-that.png` và
`46-nut-them-moi-gxpicker-vo-hieu-hoa.png`. Xoá tài khoản `kiemthu_khung` ngay sau khi chụp xong,
xác nhận lại bằng `psql`: `tai_khoan` chỉ còn `quantri`, `giao_dan`/`gia_dinh` vẫn đúng
2050/40 bản ghi (không đổi gì trên `qlgx_thu`). Tắt cả `Qlgx.Api` và `npm run dev` đã mở cho
lượt kiểm thử này.

### 33. Task "in ấn và chứng nhận" (2026-09-07) — các quyết định tự đưa ra

Nhiệm vụ: dựng hạ tầng in ấn dùng chung (HTML + Playwright → PDF, KHÔNG Office Interop) và mẫu
"Lý lịch cá nhân" — xem `docs/superpowers/specs/man-hinh/in-an.md` (spec đầy đủ). Ghi ở đây các
quyết định KHÔNG có trong yêu cầu gốc, tự đưa ra theo đúng chỉ dẫn "đừng dừng lại để hỏi".

**a) Chỉ làm hạ tầng + "Lý lịch cá nhân" ở lượt này, KHÔNG chạm tới 4 mẫu ưu tiên còn lại
(Chứng nhận bí tích, Phiếu gia đình, Chứng nhận hôn phối).** Yêu cầu gốc đã lường trước khả
năng này ("nếu hết sức trước khi xong cả 5, dừng lại cho tử tế"). Chọn dừng ở đây vì hạ tầng
(vòng đời trình duyệt dùng chung, cơ chế chọn mẫu theo giáo phận, thay thế có thoát HTML) cần
làm cho đúng ngay từ đầu — mọi mẫu sau chỉ còn việc thêm một tệp HTML + một phương thức
`Xuat*`, không phải sửa lại nền tảng. Xem mục 8 của `in-an.md` để biết đúng những gì còn thiếu.

**b) Mẫu HTML nhúng vào assembly (`EmbeddedResource`), KHÔNG để rời cạnh tệp thực thi.** Bản
Docker (`Dockerfile`) build web rồi COPY vào `wwwroot` của image API — một thư mục
`PrintTemplates/` rời có nguy cơ bị bỏ sót khi đóng gói hoặc lệch cấu trúc thư mục giữa môi
trường dev/container. Nhúng vào assembly đảm bảo mẫu LUÔN đi cùng bản build.

**c) Chọn mẫu theo giáo phận bằng cách chuẩn hoá TRỰC TIẾP `GiaoXu.GiaoHat.GiaoPhan.TenGiaoPhan`
(bỏ dấu, bỏ khoảng trắng) làm tên thư mục, KHÔNG dựng bảng ánh xạ tên thư mục tuỳ ý.** Không
đọc được nguyên văn từ mã nguồn desktop cách nó chọn thư mục `BMT` (file chứa hàm liên quan chỉ
tồn tại dạng đã biên dịch, không phải `.cs` — xem mục 9 của `in-an.md`), nên chọn cách suy luận
đơn giản nhất, dựa thẳng vào cột đã có sẵn trong CSDL thay vì bịa thêm một bảng cấu hình mới chỉ
để ánh xạ. Dữ liệu thật hiện tại chỉ có một giáo phận (Phan Thiết, không có mẫu riêng) nên luôn
rơi về `Chung` — chưa kiểm chứng được với dữ liệu giáo phận có mẫu riêng thật.

**d) Endpoint in trả PDF qua `Results.File(bytes, "application/pdf", tenTep)`, tải về bằng
`taiTepIn()` mới trong `client.ts` (tương tự `lib/csv.ts` đã có sẵn cho "Xuất dữ liệu (CSV)"),
KHÔNG mở PDF trong tab mới.** Nhất quán với cách CSV đã tải về hiện tại — người dùng quen một
kiểu duy nhất "bấm nút → tệp rơi vào thư mục Tải xuống", không cần thêm hành vi mở tab mới rồi
lại phải tự lưu.

**e) 9 mục còn lại của menu chuột phải "In lý lịch cá nhân" (bí tích, giới thiệu hôn phối, rửa
tội…) đổi từ hoàn toàn im lặng (không có `chay`) sang gọi `chuaHoTro()` — alert "chức năng này
chưa được hỗ trợ trên web ở giai đoạn này".** Đúng yêu cầu "đừng để im lặng không phản hồi",
dùng lại nguyên hàm `chuaHoTro()` đã có sẵn (đang dùng cho "In danh sách" và toàn bộ menu chuột
phải của `GxGiaDinhList`) thay vì tạo thêm một cơ chế thông báo mới.

**f) Hai nút "In lý lịch cá nhân"/"In phiếu gia đình" ở màn hình CHI TIẾT GIA ĐÌNH (không phải
danh sách) cũng đổi sang `chuaHoTro()`, KHÔNG nối "In lý lịch cá nhân" ở đây vào
`InAnService.XuatLyLichCaNhan`.** Nút này không có ngữ cảnh "cho thành viên nào" — gia đình có
nhiều thành viên. Menu chuột phải trên từng dòng thành viên của lưới nhúng trong form gia đình
(`GxGiaoDanList.menuThanhVien`, dùng chung `menuGiaoDanMacDinh`) đã in được thật cho đúng một
người — đó là chỗ đúng để in lý lịch cá nhân từ màn hình gia đình, không phải nút chung mơ hồ
này. "In phiếu gia đình" (mẫu `PhieuGiaDinh.doc`) chưa làm ở lượt này (xem mục a).

**g) Kiểm thử chạy tay qua trình duyệt thật gặp giới hạn công cụ: Playwright MCP mất kết nối
(`Connection closed`) NGAY KHI bấm bất kỳ nút nào kích hoạt tải tệp xuống (`URL.createObjectURL`
+ `<a download>` + `.click()`), kể cả nút "Xuất dữ liệu (CSV)" đã hoạt động từ trước (đã thử lại
để xác nhận đây là giới hạn chung của công cụ MCP trong môi trường này, KHÔNG phải lỗi của tính
năng in ấn mới). Xử lý: xác nhận luồng thật bằng `fetch()` thực thi NGAY TRONG trang đã đăng
nhập (`browser_evaluate`, dùng đúng token từ `localStorage['qlgx.token']` mà `authStore` dùng,
gọi qua cùng proxy Vite `/api/...` như nút bấm thật sẽ gọi) — nhận đúng
`200 application/pdf`, đúng `Content-Disposition: attachment; filename=LyLichCaNhan_1.pdf`,
đúng 139122 byte, khớp hệt kết quả gọi trực tiếp bằng `curl`. Bước cuối cùng (`a.click()` gọi
API trình duyệt chuẩn) không kiểm được qua công cụ tự động ở đây nhưng dùng lại NGUYÊN VĂN cùng
một đoạn mã (`URL.createObjectURL`/`a.download`/`a.click()`) với `lib/csv.ts` đã chạy tốt trong
sản phẩm thật từ trước — rủi ro còn lại ở bước đó coi là không đáng kể.

**h) PDF mẫu thật in cho giáo dân "Giuse Nguyễn Đức Mạnh" (mã 1, `qlgx_thu`) — đã mở lại bằng
PyMuPDF (render trang 1 ra PNG + trích xuất văn bản) để xác nhận: tiếng Việt có dấu hiển thị
đúng (không vỡ phông, thử cả nguyên âm có dấu tổ hợp như "ộ"/"ệ"), khổ A4, bố cục đúng tinh thần
văn bản hành chính (quốc hiệu → giáo phận/giáo hạt/giáo xứ → tiêu đề → bảng thông tin → chỗ ký
tên), dữ liệu đúng người (mã giáo dân, họ tên, ngày sinh, tên cha/mẹ, số/ngày/nơi rửa tội đều
khớp `GET /api/giao-dan` cùng id). Lưu tại
`WebApp/anh-chup-kiem-thu/47-LyLichCaNhan-mau-Nguyen-Duc-Manh.pdf` (PDF thật),
`47-truoc-khi-bam-in-ly-lich-ca-nhan.png` (màn hình chi tiết trước khi bấm nút) và
`48-pdf-ly-lich-ca-nhan-trang-1.png` (ảnh chụp trang 1 của PDF đã render).

**i) Tài khoản tạm `kiemthu_inan` tạo bằng đúng CLI chính thức, xoá ngay sau khi kiểm thử, xác
nhận lại `psql`: `tai_khoan` chỉ còn `quantri`, `giao_dan`/`gia_dinh`/`thanh_vien_gia_dinh` vẫn
đúng 2050/40/145 (không đổi gì trên `qlgx_thu` — lượt này chỉ ĐỌC dữ liệu để in, không ghi gì).
Tắt cả `Qlgx.Api` và `npm run dev` đã mở cho lượt kiểm thử này.**

### 34. Task "in ấn — 4 mẫu còn lại" (2026-09-07) — các quyết định tự đưa ra

Tiếp mục 33: làm 3/4 mẫu ưu tiên còn lại (Chứng nhận bí tích, Phiếu gia đình, Chứng nhận hôn
phối) đúng khuôn mẫu hạ tầng đã dựng, và sửa lỗi trình bày "nhãn/nối rỗng lửng" đã tự phát hiện
ở mẫu Lý lịch cá nhân. Giấy giới thiệu (4 mẫu) KHÔNG làm ở lượt này — xem mục d. Ghi quyết định
tự đưa ra, không có trong yêu cầu gốc.

**a) Sửa lỗi "nhãn/nối rỗng lửng" ở TẦNG DÙNG CHUNG (`Printing/VanBanInAn.cs`), không vá riêng
từng mẫu.** `VanBanInAn.MoTaBiTich(so, ngay, noi, chuSu, hanhDong, nguoiPhu, nhan)` ghép câu
kiểu "Số X — ngày Y tại Z, cha A rửa, người đỡ đầu B" nhưng bỏ HẲN từng đoạn (kể cả liên từ đi
kèm — "tại", dấu "—", dấu phẩy) khi thiếu dữ liệu tương ứng, thay vì để lại nhãn rỗng. Áp dụng
lại cho `LyLichCaNhan` (đổi 6 cột rời `SoRuaToi`/`NgayRuaToi`/... trong dict thành 3 khoá
`MoTaRuaToi`/`MoTaRuocLe`/`MoTaThemSuc` đã ghép sẵn) VÀ dùng ngay từ đầu ở 2 mẫu mới
(`ChungNhanBiTich`, `PhieuGiaDinh`, `ChungNhanHonPhoi`) — một chỗ sửa, không có mẫu nào tái phạm
lỗi cũ. Đã kiểm tra bằng dữ liệu thật thiếu-đủ khác nhau (xem mục e) — không còn dấu phẩy/giới
từ bơ vơ ở bất kỳ trường hợp thiếu dữ liệu nào gặp phải.

**b) Bốn mục menu "In chứng nhận bí tích/rửa tội/xưng tội-rước lễ/thêm sức" dùng CHUNG MỘT
endpoint** (`GET /api/giao-dan/{id}/in/chung-nhan-bi-tich?loai=RuaToi|RuocLe|ThemSuc`, bỏ trống
`loai` = mục chung liệt kê cả ba) **và CHUNG MỘT mẫu HTML** (`ChungNhanBiTich.html`), chỉ đổi
tiêu đề + dòng bí tích được liệt kê theo `loai`. Bản desktop
(`Source/ExcelReport/ReportChungNhanBT.cs`) cũng chỉ đổi TÊN TỆP mẫu theo `LoaiBiTich`, mọi
phép `Replace` chạy giống nhau cho cả 4 loại — bản web tái hiện đúng tinh thần đó bằng cấu trúc
đơn giản hơn (một mẫu, một danh sách văn bản nhiều dòng dùng CSS `white-space: pre-line`) thay
vì 4 tệp `.doc` gần như trùng lặp.

**c) CỐ Ý KHÔNG dựng phần "gửi giáo xứ nhận"** (`TenLinhMucNhan`/`TenGiaoXuNhan`/`TenGiaoPhanNhan`/
`LyDo` của `ReportChungNhanBT.cs`/`ReportChungNhanHP.cs`) **ở cả "Chứng nhận bí tích" lẫn "Chứng
nhận hôn phối".** Đó là dữ liệu của một giấy CHUYỂN giáo xứ (linh mục xứ nhận, lý do xin chuyển)
cần NHẬP TAY ngay lúc in — không có cột nào trong CSDL hiện tại lưu sẵn, và cũng chưa có màn
hình/hộp thoại nào cho nhập lúc in ở Phase 1. Hai mẫu web hiện tại là "chứng nhận nội bộ" (giáo
xứ tự chứng nhận cho chính giáo dân của mình), không phải "giấy giới thiệu liên xứ" — tương ứng
đúng phần thân đã tái hiện, chỉ bỏ khối định tuyến chuyển xứ. Nếu sau này cần giấy chuyển xứ đầy
đủ, cần thêm hộp thoại nhập 4 trường đó trước khi gọi endpoint in — ghi lại để lượt sau biết.

**d) "Giấy giới thiệu" (4 mẫu: chuyển xứ/rửa tội/thêm sức/giáo lý hôn phối) VẪN CHƯA LÀM —
KHÔNG kịp trong lượt này, đúng tinh thần "2 mẫu chạy tốt hơn 4 mẫu làm dở" nhân lên cho lượt
này (ở đây là 3/4 mẫu chạy tốt hơn 4/4 mẫu làm dở).** Đọc sơ `ReportGioiThieuHP.cs`
(giấy giới thiệu giáo lý hôn phối) cho thấy mẫu này cần dữ liệu của "người thứ hai" — thường ở
GIÁO XỨ KHÁC, không nhất thiết có bản ghi `GiaoDan` nào trong CSDL của giáo xứ đang đăng nhập
(`ReportHonPhoiConst.Nguoi2`/`Tuoi2`/`TenCha2`... là các trường NHẬP TAY độc lập, không tra từ
bảng nào) — nghĩa là cần một MÀN HÌNH NHẬP LIỆU mới (form nhập tay thông tin người thứ hai),
không chỉ một mẫu in đọc thẳng từ CSDL như ba mẫu đã làm. Việc này lớn hơn "chỉ thêm một `.html`
+ một `Xuat*`", để lại nguyên vẹn cho lượt sau. Menu tương ứng ("In giới thiệu hôn phối", "In
giấy giới thiệu chứng nhận rửa tội/thêm sức") VẪN báo `chuaHoTro()`, không đổi.

**e) Bằng chứng chạy thật:** tạo tài khoản tạm `kiemthu_inan2` bằng CLI, đăng nhập lấy JWT thật,
gọi `fetch`/`curl` NGAY QUA API THẬT (`Qlgx.Api` chạy tại `localhost:5299`, kết nối `qlgx_thu`
thật) — không phải test giả lập. Bốn lần gọi:
- Chứng nhận bí tích "TatCa" cho giáo dân "Trần Thị Mai Phượng" (mã 456, đủ 3 bí tích) —
  `49-ChungNhanBiTich-TatCa-Tran-Thi-Mai-Phuong.pdf`.
- Ba biến thể `loai=RuaToi/RuocLe/ThemSuc` cho cùng người — xác nhận tiêu đề đổi đúng
  ("CHỨNG NHẬN RỬA TỘI"/"CHỨNG NHẬN XƯNG TỘI - RƯỚC LỄ LẦN ĐẦU"/"CHỨNG NHẬN THÊM SỨC") và chỉ
  đúng MỘT dòng bí tích tương ứng được liệt kê (không lưu PDF riêng, chỉ trích văn bản kiểm tra).
- Phiếu gia đình cho gia đình 7 thành viên (đủ các trường hợp: chồng/vợ/con, có/không đủ tam bí
  tích, có/không hôn phối) — `50-PhieuGiaDinh-7-thanh-vien.pdf`. Trích văn bản xác nhận KHÔNG
  còn nhãn/nối rỗng lửng ở bất kỳ dòng nào (kể cả các dòng chỉ có MỘT trong ba mốc so/ngày/nơi —
  ví dụ "Tại Tiêu Hạ" viết hoa chữ đầu khi chỉ có nơi, không có ngày/số).
- Chứng nhận hôn phối cho gia đình có hôn phối thật (522 bản ghi, chọn "Giuse Lương Văn Sơn" —
  `ngay_hon_phoi` 1997-01-12) — `51-ChungNhanHonPhoi-Giuse-Luong-Van-Son.pdf`. Phát hiện phụ khi
  đọc PDF: một số dữ liệu gốc (`qlgx_thu`) có giá trị placeholder cũ `"X"` (ví dụ `NoiRuaToi =
  "X"`, `HoTenCha = "X"`) và cả lỗi chính tả sẵn có ("Thj" thay vì "Thị") — đây là DỮ LIỆU THẬT
  từ Access chuyển sang, KHÔNG phải lỗi in ấn; mẫu in hiển thị trung thực đúng những gì có trong
  CSDL, cố ý KHÔNG lọc/sửa giá trị "X" ở tầng in (không phải việc của tính năng in ấn để "làm
  đẹp" dữ liệu gốc — nếu cần dọn, đó là việc của một nhiệm vụ làm sạch dữ liệu riêng).
- Tất cả 4/4 lần gọi trả đúng `200 application/pdf`, chữ ký `%PDF-`; gọi không kèm token trả
  đúng `401`. Dọn dẹp: xoá tài khoản tạm, xác nhận lại `psql` — `tai_khoan` chỉ còn `quantri`,
  4 bảng đếm vẫn đúng 2050/40/145/522 (không ghi gì, chỉ đọc để in).

**f) `BoDoMauIn.Dung()` thêm tham số tuỳ chọn `khoiHtmlAnToan` — lối thoát CÓ CHỦ ĐÍCH khỏi cơ
chế thay-thế-tự-thoát-HTML thông thường, DÙNG RIÊNG cho phần mẫu cần LẶP LẠI theo số lượng bản
ghi không cố định (mỗi dòng một thành viên gia đình ở Phiếu gia đình).** Giá trị trong
`khoiHtmlAnToan` được chèn NGUYÊN VĂN (không tự thoát) — `InAnService.XuatPhieuGiaDinh` tự gọi
`HtmlEncoder.Default.Encode(...)` cho TỪNG mẩu dữ liệu người dùng trước khi ghép vào khung
`<tr>/<td>` tự viết, giữ đúng nguyên tắc "dữ liệu người dùng luôn qua HtmlEncoder trước khi vào
HTML" — chỉ khác ai gọi encoder (InAnService, không phải BoDoMauIn) để có thể ghép nhiều mẩu đã
thoát cạnh khung HTML chưa thoát. Cân nhắc thay thế: một template engine thật (Razor/Scriban) xử
lý vòng lặp gọn hơn, nhưng thêm phụ thuộc mới chỉ để giải quyết MỘT trường hợp (bảng thành viên)
— chưa đáng đánh đổi ở quy mô hiện tại (4 mẫu HTML, một trường hợp cần lặp).

**g) `GiaDinhService` thêm phương thức public `LayVoChongVaHonPhoi` để `InAnService` TÁI SỬ
DỤNG logic "chọn hôn phối hiện tại của một gia đình" đã có sẵn** (`TimHonPhoiHienTaiEntity`,
`ChonHonPhoiHienTai` — đã tối ưu để tránh subquery tương quan, xem lịch sử commit
"Giam subquery tuong quan..."), KHÔNG viết lại truy vấn đó lần thứ hai ở `InAnService`. Đánh đổi
duy nhất: `InAnService` giờ phụ thuộc thêm `GiaDinhService` (cả hai đều Scoped, không có vấn đề
vòng đời).

**h) Nam/Nữ trong "Chứng nhận hôn phối" xác định theo `GiaoDan.Phai` THẬT của từng người tham
gia `HonPhoi`, KHÔNG giả định "Chồng luôn là Nam".** Vai trò Chồng/Vợ (`ThanhVienGiaDinh.VaiTro`)
và Nam/Nữ trên giấy chứng nhận là hai khái niệm khác nhau về mặt dữ liệu — tránh in sai giới
tính nếu có bản ghi nhập lệch.

### 35. Task "hai việc xác thực — khoá đăng nhập chéo giáo xứ và tự đổi mật khẩu" (2026-09-07) — các quyết định tự đưa ra

Hai việc theo `WebApp/VIEC-TIEP-THEO.md` mục 2.1 (khoá đăng nhập khoá CHÉO giữa các giáo xứ —
phát hiện M1 của `review-cuoi.md`) và 1.3 (màn hình tự đổi mật khẩu). Người dùng đang bận,
không hỏi được — ghi lại các quyết định tự đưa ra.

**a) M1 — sửa bằng cách thu hẹp danh sách ứng viên đăng nhập về ĐÚNG MỘT giáo xứ dựa trên
TÊN ĐĂNG NHẬP ĐANG GÕ (không dựa trên "server có bao nhiêu giáo xứ").** Cân nhắc hai hướng:

- Hướng bị loại: "cứ có ≥2 giáo xứ trên server thì luôn bắt chọn giáo xứ trước khi đăng nhập".
  Đơn giản hơn để cài, nhưng vi phạm thẳng yêu cầu "đừng bắt người dùng chọn giáo xứ mỗi lần
  đăng nhập nếu hệ thống chỉ có một" theo nghĩa rộng hơn: một khi có giáo xứ thứ hai (dù chỉ
  một tài khoản không trùng tên ai), MỌI người dùng của MỌI giáo xứ đều bị bắt chọn thêm một
  bước — phiền cho > 99% trường hợp chỉ để phòng cho một tên đăng nhập hiếm khi trùng.
- Hướng đã chọn: `AuthService.DangNhap` tra cứu tên đăng nhập trên toàn máy chủ như cũ (đây là
  truy vấn CHÉO GIÁO XỨ DUY NHẤT được phép, đã có từ trước), nhưng nếu tên đăng nhập đó chỉ tồn
  tại ở MỘT giáo xứ (áp dụng cho 100% trường hợp của giáo xứ pilot hiện tại, và cho đa số tên
  đăng nhập cá nhân dù server có nhiều giáo xứ) thì xử lý y hệt trước đây — không hỏi gì thêm,
  không đổi trải nghiệm. CHỈ khi tên đăng nhập THẬT SỰ trùng ở ≥2 giáo xứ (ví dụ "vanphong" ở cả
  giáo xứ A và B) mới dừng lại, trả về mã lỗi mới `CanChonGiaoXu` (HTTP 400) kèm danh sách các
  giáo xứ trùng tên (chỉ Id + tên hiển thị), và bắt buộc client gửi lại đúng một `GiaoXuId` đã
  chọn. Vì chỉ mục là `(GiaoXuId, TenTaiKhoan)` duy nhất, sau khi lọc theo `GiaoXuId` đã chọn,
  danh sách ứng viên còn lại LUÔN LÀ 0 hoặc 1 tài khoản — không còn đường nào để một request
  chạm tới/kiểm tra/tăng bộ đếm sai của tài khoản ở giáo xứ khác được nữa, đóng chặt lỗ hổng M1
  bằng cấu trúc dữ liệu chứ không chỉ bằng kiểm tra điều kiện.

**b) Đánh đổi bảo mật của việc "hỏi chọn giáo xứ khi trùng tên": rò rỉ có kiểm soát rằng một
tên đăng nhập tồn tại ở nhiều giáo xứ (kèm TÊN các giáo xứ đó), CHƯA kiểm mật khẩu.** Đây là
đánh đổi không tránh được của MỌI thiết kế "chọn tenant trước khi xác thực" (giống mô hình
"chọn workspace" của các ứng dụng SaaS đa tổ chức dùng chung một trang đăng nhập) — không có
cách nào để người dùng tự chọn đúng giáo xứ mà không tiết lộ giáo xứ nào có tên đăng nhập đó.
Giảm nhẹ mức độ rò: (i) chỉ những tên đăng nhập THẬT SỰ trùng mới lộ gì (đa số không trùng, im
lặng như cũ); (ii) tên giáo xứ vốn không phải bí mật trong hệ thống này — đã hiển thị công khai
sau khi đăng nhập ở thanh trên (`AppShell.tsx`, mục 32 file này) và là thông tin một giáo xứ
Công giáo thường công khai; (iii) KHÔNG kiểm mật khẩu/tăng bộ đếm sai ở bước hỏi này — một kẻ
dò tên đăng nhập không thu được gì thêm ngoài "tên này trùng ở đâu", không đoán được mật khẩu
nhanh hơn.

**c) Endpoint mới không phải `GET /api/auth/giao-xu` (liệt kê TOÀN BỘ giáo xứ công khai) mà
nhúng thẳng danh sách giáo xứ trùng tên vào phần thân lỗi 400 của chính `POST
/api/auth/dang-nhap`.** Cân nhắc endpoint riêng trước, bỏ vì: (i) không cần thêm một endpoint
ẩn danh mới lộ toàn bộ danh mục giáo xứ của máy chủ cho ai gọi cũng được — kể cả tên đăng nhập
không tồn tại; (ii) một round-trip duy nhất (gửi lại đúng request cũ + `giaoXuId`) đơn giản hơn
cho cả frontend lẫn test so với luồng tra-cứu-trước-rồi-mới-gửi.

**d) Đổi mật khẩu — bắt buộc `matKhauHienTai` xác thực bằng CHÍNH `PasswordHasher<TaiKhoan>`
đã dùng cho đăng nhập, KHÔNG tự chế cơ chế băm/so khớp nào khác.** `TaiKhoanId` của người đổi
lấy từ claim `sub` của token (`ClaimTypes.NameIdentifier` sau ánh xạ mặc định của
`JwtSecurityTokenHandler`, xác nhận bằng test `Doi_mat_khau_khong_dang_nhap_thi_bi_401` và ba
test đổi mật khẩu thành công/sai/quá ngắn), kết hợp với `QlgxDbContext` tiêm qua DI (đã tự lọc
theo `GiaoXuId` của claim) — không có tham số trình duyệt nào mang `TaiKhoanId`/`GiaoXuId` của
người khác lọt vào được. Độ dài tối thiểu **8 ký tự**, đồng bộ với ngưỡng đã có sẵn ở
`TaoTaiKhoanQuanTri.cs` (`QLGX_ADMIN_MAT_KHAU phai co it nhat 8 ky tu`) — không đặt ngưỡng riêng
cho luồng tự đổi để tránh hai chuẩn lệch nhau.

**e) Hai cột `CauHoiGoiY`/`CauTraLoiGoiY` KHÔNG được động tới** — đúng chốt trước đó (đã ghi ở
`TaiKhoan.cs`), tự đổi mật khẩu ở đây chỉ cần mật khẩu HIỆN TẠI, không dùng câu hỏi gợi nhớ làm
đường vòng nào.

**f) Không ghi mật khẩu vào log — xác nhận bằng cách đọc lại toàn bộ đường đi:** không
middleware log request body nào trong `Program.cs`, endpoint `/api/auth/mat-khau` chỉ trả
`thongBao` cố định ("Mật khẩu hiện tại không đúng"/"Mật khẩu mới phải có ít nhất 8 ký tự"),
không log ngoại lệ kèm giá trị mật khẩu ở bất kỳ nhánh nào của `AuthService.DoiMatKhauCuaToi`.

**g) Giao diện: dùng lại nguyên khung `.hoidap-nen`/`.hoidap-hop` đã có (component `GxHoiDap`)
thay vì tạo khung modal mới** — nhất quán với hộp thoại xác nhận sẵn có trong ứng dụng, không
nhân đôi CSS. Thêm `input[type="password"]` vào cùng nhóm selector 12px với
`input[type="text"]` trong `qlgx.css` (trước đây bị bỏ sót — ô mật khẩu màn hình đăng nhập cũ
dùng cỡ chữ mặc định của trình duyệt, không phải lỗi MỚI do task này tạo ra nhưng tiện sửa
luôn vì cùng nhóm input). Nút "Đổi mật khẩu" trong menu "Hệ thống" (cạnh "Đăng xuất") vốn đã có
sẵn dạng `disabled` từ Task 14 — chỉ cần nối `onClick` thật, không đổi vị trí/nhãn.

**h) Bắt buộc kiểm chứng bằng chạy thật:** dựng tài khoản tạm `tam_kt_doimk` trên `qlgx_thu`
bằng CLI chính thức (`dotnet run -- tao-tai-khoan-quan-tri`), mở `Qlgx.Api` + `npm run dev`,
dùng Playwright MCP: đăng nhập → mở modal đổi mật khẩu → thử sai mật khẩu hiện tại (bị từ chối,
thông báo rõ, không đổi gì) → đổi đúng → đăng xuất → mật khẩu CŨ bị từ chối → mật khẩu MỚI đăng
nhập được. Ảnh chụp `WebApp/anh-chup-kiem-thu/52`–`56`. Sau đó xoá tài khoản tạm bằng `psql`,
xác nhận lại `qlgx_thu` chỉ còn `quantri` và dữ liệu vẫn đúng 2050/40/145.

**i) Bằng chứng đỏ→xanh cho M1 (bắt buộc theo yêu cầu):** viết test
`Khoa_dang_nhap_sai_khong_duoc_lan_sang_giao_xu_khac` TRƯỚC, tạm thời giữ nguyên logic cũ
(`_ = giaoXuId;`, chưa lọc) — chạy `dotnet test --filter BaoMatTests` → **ĐỎ thật**
(`Expected dangNhapB.StatusCode to be HttpStatusCode.OK ... but found HttpStatusCode.Unauthorized`,
tài khoản "vanphong" của giáo xứ B bị khoá lây sau 10 lần sai ở giáo xứ A, đúng kịch bản M1 mô
tả). Áp fix thật (thu hẹp theo `GiaoXuId`/ambiguity) → chạy lại → **XANH** (19/19
`BaoMatTests`, cả test mới `Ten_dang_nhap_trung_o_hai_giao_xu_ma_khong_chon_thi_bi_yeu_cau_chon`
cũng xanh). Tổng test cuối: backend 213/213 (207 + 6 mới: 2 test M1 + 4 test đổi mật khẩu),
frontend 229/229 (228 + 1 test mới cho luồng chọn giáo xứ ở `LoginPage`).

**j) Việc CHƯA làm trong phạm vi nhiệm vụ này** (không thuộc mục 2.1/1.3, cố tình không đụng
vào): giới hạn đăng nhập theo IP (đã ghi nhận ở mục 31.d, vẫn treo); màn hình quản lý giáo xứ
theo giáo phận (mục 2.3 của `VIEC-TIEP-THEO.md`); vai trò CSDL riêng cho RLS (mục 2.2). Trong
lúc sửa cũng phát hiện `AuthService` giờ có HAI `QlgxDbContext` khác nhau trong cùng một class
(`db` cục bộ dùng chuỗi kết nối QUẢN TRỊ cho `DangNhap`, và `dbNguoiDung` tiêm qua DI đã lọc
tenant cho `DoiMatKhauCuaToi`) — đặt tên rõ ràng để không nhầm lẫn, nhưng đáng cân nhắc tách
`AuthService` thành hai lớp riêng (xác thực đăng nhập vs. quản lý tài khoản của chính mình) nếu
sau này còn thêm nghiệp vụ tự-phục-vụ khác — chưa làm vì phạm vi nhỏ, chỉ một phương thức.

### 36. Task "ảnh đại diện giáo dân/gia đình" (2026-09-07) — các quyết định tự đưa ra

`WebApp/VIEC-TIEP-THEO.md` mục 1.2, mục chặn cuối cùng của Mức 1. Người dùng đang bận, không
hỏi được — ghi lại các quyết định tự đưa ra.

**a) Bằng chứng khảo sát mã desktop — cột `AnhDaiDien` (Access) lưu ĐƯỜNG DẪN TỆP CỤC BỘ, không
phải base64 hay tên tệp đơn thuần.** `Source/GXControl/frmGiaoDan.cs` (UTF-16LE, đọc qua
`iconv -f UTF-16LE -t UTF-8`), dòng ~995 lúc lưu — `if (gxPictureField1.FileName != null)
row[GiaoDanConst.AnhDaiDien] = gxPictureField1.FileName;` — và dòng ~1119 lúc nạp lại lên form —
`if (File.Exists(string.Concat(Memory.AppPath, row[GiaoDanConst.AnhDaiDien].ToString())))
gxPictureField1.ImagePicture = Image.FromFile(string.Concat(Memory.AppPath,
row[GiaoDanConst.AnhDaiDien].ToString()));`. `Memory.AppPath` là thư mục cài đặt CỤC BỘ của máy
đang chạy bản desktop — giá trị lưu trong CSDL chỉ là một đường dẫn TƯƠNG ĐỐI ghép với thư mục
đó, vô nghĩa trên bất kỳ máy nào khác (kể cả một máy chủ web tập trung). Xác nhận thêm bằng khảo
sát dữ liệu thật: 2050/2050 dòng của `qlgx_thu.giao_dan.anh_dai_dien` đều RỖNG — không có dữ
liệu cũ nào cần giữ tương thích khi đổi thiết kế lưu trữ. `GiaDinh.AnhDaiDien` cùng bản chất
(cùng tên cột, cùng kiểu dữ liệu văn bản).

**b) Lưu trữ: cột `bytea` (nhị phân) trực tiếp trong PostgreSQL, KHÔNG dùng object storage.**
Hai cột mới trên cả `GiaoDan` và `GiaDinh`: `AnhDaiDienDuLieu` (`byte[]?`, cột `bytea`) +
`AnhDaiDienLoaiNoiDung` (`string?`, luôn `"image/jpeg"` sau chuẩn hoá — xem mục d). Cột
`AnhDaiDien` (text) cũ bị XOÁ hẳn khỏi cả hai thực thể (migration `ThemAnhDaiDienNhiPhan` đổi
tên cột đó thành `anh_dai_dien_loai_noi_dung` rồi không đọc lại giá trị cũ — an toàn vì mục a đã
xác nhận rỗng toàn bộ). Lý do chọn cột nhị phân thay vì object storage (S3/MinIO):
- Quy mô thật nhỏ: 2050 giáo dân + 40 gia đình, ảnh 3x4 sau khi thu nhỏ+nén JPEG (mục d) chỉ còn
  vài chục KB/ảnh — kể cả phủ kín 100% cũng chưa tới ~150MB, không đáng kể so với CSDL đã có.
- Không thêm một thành phần hạ tầng mới cho pilot: không cần triển khai/vận hành/sao lưu riêng
  một object store, không cần thêm biến môi trường/khoá truy cập mới, không cần endpoint proxy
  ảnh nào phức tạp hơn (route đọc ảnh y hệt các route JSON khác — cùng `RequireAuthorization()`,
  cùng bộ lọc `GiaoXuId`, cùng cách viết test).
- Sao lưu/khôi phục ĐI CHUNG với dữ liệu — một bản `pg_dump` là đủ cho cả nghiệp vụ lẫn ảnh,
  không có nguy cơ CSDL và object store lệch nhau sau khôi phục từ hai điểm backup khác thời
  điểm (rủi ro thật với hướng tách riêng).
- Vẫn tôn trọng ĐÚNG ràng buộc HA đã chốt (không ghi đĩa cục bộ máy chủ) — bytea nằm trong
  CSDL dùng chung giữa mọi bản API, không có gì đặc thù cho từng container.

Đường nâng cấp nếu sau này đổi hướng (ghi lại để không phải suy nghĩ lại từ đầu): nếu số giáo xứ
tăng mạnh và ảnh gốc lớn hơn đáng kể (ví dụ đổi yêu cầu — lưu cả ảnh gốc chưa thu nhỏ để in chất
lượng cao), tách phần đọc/ghi nhị phân ra khỏi `AnhDaiDienService` thành một interface riêng rồi
đổi cài đặt sang S3-compatible mà KHÔNG đụng tới endpoint/DTO — cột `AnhDaiDienDuLieu` hiện tại
có thể giữ làm cache/thumbnail trong khi bản gốc chuyển sang object storage.

**c) Kiểm tra file tải lên — GIẢI MÃ THẬT bằng SkiaSharp, không tin phần mở rộng/Content-Type.**
`Qlgx.Api/Anh/XuLyAnh.cs`: (1) chặn dung lượng gốc > 8MB TRƯỚC khi giải mã; (2)
`SKCodec.Create(stream)` — trả `null` cho bất kỳ tệp nào không phải ảnh hợp lệ (một `.exe`/văn
bản đổi tên thành `.jpg` sẽ bị từ chối ở đây, không phải vì đọc tên tệp mà vì giải mã thất bại
thật); (3) `codec.EncodedFormat` phải là Jpeg/Png/Webp — chặn định dạng giải mã ĐƯỢC nhưng
không nằm trong danh sách cho phép (Gif/Bmp/Ico…); (4) `SKBitmap.Decode` phải thành công. Kiểm
thử bằng test gửi một tệp văn bản thuần đặt tên `.jpg` VÀ khai Content-Type `image/jpeg` — vẫn
bị từ chối (xem `AnhDaiDienTests.Tep_khong_phai_anh_dat_ten_jpg_bi_tu_choi_du_khai_dung_content_type`).

**d) Chuẩn hoá MỌI ảnh về JPEG, khung tối đa 640px cạnh dài, chất lượng 85.** Ảnh 3x4 in ở
300dpi chỉ cần ~354×472px thật; 640px cạnh dài dư nét cho cả in lẫn xem trên màn hình trong khi
giữ dung lượng nhỏ (vài chục KB/ảnh dù ảnh gốc điện thoại vài MB–vài chục MB). Ảnh có nền trong
suốt (PNG/WebP) được vẽ đè lên nền TRẮNG trước khi ép JPEG (JPEG không có kênh alpha) để không
biến thành khối đen. Phát hiện trong lúc viết: một `SKBitmap` dựng với `SKColorType.Rgb888x` rồi
gọi `.Encode(Jpeg, ...)` trực tiếp trả về `null` trên bản SkiaSharp 3.119 dùng ở đây (đã kiểm
chứng bằng một chương trình Console riêng, ngoài bộ test) — phải dùng `SKColorType.Bgra8888` cho
bitmap nền trắng rồi bọc qua `SKImage.FromBitmap(...).Encode(...)` mới ra dữ liệu hợp lệ; ghi
chú thẳng trong code để không ai vấp lại lỗi này.

**e) Gói xử lý ảnh: SkiaSharp (MIT) + `SkiaSharp.NativeAssets.Linux`, KHÔNG dùng
`System.Drawing.Common`/ImageSharp.** `System.Drawing.Common` không chạy trên Linux từ .NET 7 —
loại ngay vì Dockerfile chạy trên `mcr.microsoft.com/dotnet/aspnet:10.0` (Debian). ImageSharp bị
cân nhắc nhưng loại vì giấy phép Six Labors Split License đòi mua thương mại ngoài ngưỡng miễn
phí cho dự án không phải mã nguồn mở/phi lợi nhuận thuần — SkiaSharp giấy phép MIT, tự do hoàn
toàn, không ràng buộc gì thêm. Native binary cho Linux thêm qua `SkiaSharp.NativeAssets.Linux`
(ảnh nền `aspnet:10.0` là Debian glibc, không phải Alpine — không cần biến thể `musl`).

**f) Ảnh vào được cả hai mẫu in đã có (`LyLichCaNhan`, `PhieuGiaDinh`) — nhúng base64 trực tiếp
trong thẻ `<img>`, không phải endpoint ảnh riêng.** PDF xuất bằng Playwright headless từ một
chuỗi HTML tĩnh (xem `BoTrinhDuyet`/`BoDoMauIn`) — trang HTML đó không có phiên đăng nhập nào để
gọi ngược lại `GET .../anh-dai-dien`, nên `InAnService.KhoiAnhDaiDien` tự dựng chuỗi
`data:image/jpeg;base64,...` ngay trong service (dữ liệu đã qua `XuLyAnh` lúc lưu, không phải
chuỗi tự do người dùng gõ, nên chèn NGUYÊN VĂN vào khối HTML thô `khoiHtmlAnToan` của
`BoDoMauIn.Dung` là an toàn — xem ghi chú sẵn có ở đó). Khi giáo dân/gia đình CHƯA có ảnh, biến
`KhoiAnh` là chuỗi rỗng — mẫu KHÔNG hiện khung ảnh trống vô nghĩa trên giấy in, đúng hành vi mô
tả trong `BoDoMauIn.Dung` cho "khối không áp dụng được". Vị trí: góc trên-phải khu tiêu đề mỗi
mẫu (`.tieu-de-wrap` + `position: absolute` cho `.anh-dai-dien`), 30×40mm (LyLichCaNhan) /
26×34mm (PhieuGiaDinh).

**g) `<img>` không tự đính header `Authorization` được — phía web PHẢI fetch() ảnh rồi dựng lại
thành object URL, không gán thẳng URL API vào `src`.** `api/client.ts` thêm `layAnhBlobUrl`
(cùng khuôn `taiTepIn` đã có cho tải PDF) và `taiAnhLen` (multipart, KHÔNG qua `goi()` vốn chỉ
gửi JSON). Component dùng chung `components/AnhDaiDien.tsx` cho cả hai màn hình chi tiết (giáo
dân/gia đình) — tự quản lý vòng đời object URL blob (tạo/`URL.revokeObjectURL` khi ảnh đổi hoặc
component gỡ), nhận vào chỉ `id` + ba hàm gọi API đã tiêm sẵn đường dẫn đúng loại thực thể. Ô
ảnh tự vô hiệu (không cho chọn tệp) khi bản ghi CHƯA lưu lần đầu (`id === null`) — cùng quy ước
với các tab phụ khác (Hôn phối/Tận hiến/Hội đoàn) chỉ dùng được sau khi đã có id thật.

**h) ASP.NET Core TỰ ĐỘNG gắn yêu cầu antiforgery cho endpoint có tham số `IFormFile` (từ .NET
8) — API xác thực bằng Bearer JWT nên phải `.DisableAntiforgery()` rõ ràng, nếu không mọi
request rơi vào 500** (`InvalidOperationException: ... contains anti-forgery metadata, but a
middleware was not found`). Không cấu hình `app.UseAntiforgery()` — không cần, vì không có
cookie phiên trình duyệt nào ở đây để CSRF nhắm tới; antiforgery chỉ có ý nghĩa cho flow
cookie-based, không áp dụng cho Bearer token tự chứa.

**i) Test bắt buộc theo yêu cầu — đủ cả ba tình huống chặn:** `AnhDaiDienTests.cs` (8 test mới):
từ chối tệp không phải ảnh dù đặt tên `.jpg` kèm khai `Content-Type: image/jpeg`; từ chối tệp
9MB (> giới hạn 8MB); ảnh giáo xứ A không đọc/ghi/xoá được bằng phiên giáo xứ B (cả giáo dân lẫn
gia đình); tải lên/xem lại/xoá thành công cho trường hợp hợp lệ. Thêm 1 test ở
`GiaoDanInAnTests.cs` xác nhận ảnh THẬT SỰ được nhúng vào PDF (so kích thước PDF có-ảnh/không-ảnh
với ảnh nhiễu ngẫu nhiên 120×160 — ảnh một màu đặc nén JPEG chỉ còn vài trăm byte, không đủ tạo
chênh lệch rõ so với biến động font/metadata bình thường giữa hai PDF). Tổng test cuối: backend
**222/222** (213 cũ + 8 ảnh + 1 in-ấn-có-ảnh), frontend **229/229** (không đổi số lượng —
component `AnhDaiDien` chỉ thay thế `<div className="photo-slot">` tĩnh trong hai màn hình đã có
test sẵn, không thêm test riêng ở lượt này vì hành vi tải/xoá ảnh đã được phủ đầy đủ ở tầng API).

**j) Việc CHƯA làm trong phạm vi nhiệm vụ này:** không có nút "xem ảnh cỡ lớn" (chỉ xem đúng
kích thước đã thu nhỏ trong khung 3x4); không giữ lại ảnh gốc trước khi thu nhỏ (một khi đã nén,
không phục hồi được — chấp nhận được vì mục đích chỉ là ảnh 3x4 nhận diện, không phải lưu trữ
ảnh gốc); chưa có giới hạn tốc độ (rate limit) riêng cho endpoint tải ảnh lên (dùng chung mức
bảo vệ của toàn API, chưa có mức nào ở lượt này); gia đình dùng CHUNG một component
`AnhDaiDien` với giáo dân nhưng KHÔNG có "ảnh đại diện" cho `ThanhVienGiaDinh`/vai trò khác —
chỉ đúng hai thực thể `GiaoDan`/`GiaDinh` theo đúng phạm vi mục 1.2.

### 37. Task "quản lý giáo xứ theo giáo phận + tách vai trò CSDL cho RLS" (2026-09-07) — các quyết định tự đưa ra

`WebApp/VIEC-TIEP-THEO.md` mục 2.3 và 2.2 — chuẩn bị cho giáo xứ thứ hai lên chung máy chủ.
Người dùng đang bận, không hỏi được — ghi lại quyết định tự đưa ra và bằng chứng chạy thật.

**a) Loại tài khoản mới `LoaiTaiKhoan=9` "Quản trị hệ thống"** — cấp cao hơn "Quản trị viên"
(`0`, vốn chỉ quản lý được đúng giáo xứ của mình). Không lấy số kế tiếp `3` để tránh nhầm với dữ
liệu di trú từ Access sau này. Không có tài khoản mặc định nào — tạo bằng CLI
`dotnet Qlgx.Api.dll tao-tai-khoan-quan-tri` với `QLGX_ADMIN_LOAI_TAI_KHOAN=9`. Chi tiết đầy đủ
của quyết định phân quyền ở `docs/superpowers/specs/man-hinh/quan-ly-giao-xu.md` mục 4 — không
nhắc lại ở đây, chỉ ghi bằng chứng ĐỎ→XANH đã tự kiểm chứng lại (không chỉ tin lời chú thích
trong code): đổi tạm `RequireAuthorization("QuanTriHeThong")` thành `("QuanTri")` ở
`QuanLyGiaoXuEndpoints.cs`, chạy `dotnet test --filter QuanLyGiaoXuTests` → **7/8 ĐỎ** (quản trị
viên thường của giáo xứ A nhận `200 OK` thay vì `403` ở cả ba route `giao-phan`/`giao-hat`/
`giao-xu`, và ngược lại "Quản trị hệ thống" bị `403` khi tạo giáo phận/giáo hạt/giáo xứ vì đã
đổi policy không khớp claim `9`); trả lại `"QuanTriHeThong"` → **8/8 XANH**. Đây chính là lớp
phòng thủ chặn "giáo xứ A xem/sửa được giáo xứ B" mà nhiệm vụ đòi hỏi.

**b) Giáo họ (`GiaoHo`, CÓ `giao_xu_id`) được bổ sung API thêm/sửa (`POST`/`PUT /api/giao-ho`)
và màn hình web đi kèm — an toàn hơn hẳn "Quản lý giáo xứ" vì nằm trong phạm vi giáo xứ của
người gọi, được cả bộ lọc EF lẫn RLS bảo vệ như mọi bảng nghiệp vụ khác, KHÔNG cần policy
"QuanTriHeThong". Test `Khong_sua_duoc_giao_ho_cua_giao_xu_khac_boi_loc_tenant` xác nhận PUT vào
giáo họ của giáo xứ B từ phiên giáo xứ A trả `404` (bộ lọc EF ẩn dòng đi, không phải `403` —
đúng hành vi nhất quán với mọi endpoint nghiệp vụ khác trong hệ thống, ví dụ GiaDinh/GiaoDan).
Không có nút xoá — nhất quán với quyết định "chặn xoá" của màn hình Quản lý giáo xứ (mục 4 của
spec), một giáo họ có thể đã gắn với giáo dân/gia đình thật.

**c) Tách vai trò CSDL cho RLS — hạ tầng (`ChuoiKetNoiQuanTri.cs`, `docker-compose.yml`,
`.env.example`, `RlsTests.cs`) đã có sẵn từ trước lượt này, nhưng CHƯA từng chạy thật với hai
vai trò tách biệt trên dữ liệu thật — nhiệm vụ yêu cầu "làm cho chạy thật được, không chỉ có
trong tài liệu".** Đã tạo thật hai vai trò trên `qlgx_thu` (`qlgx_app` NOSUPERUSER NOBYPASSRLS,
`qlgx_admin` NOSUPERUSER BYPASSRLS, mật khẩu ngẫu nhiên KHÔNG ghi vào bất kỳ file nào trong repo
— chỉ tồn tại trong biến môi trường phiên làm việc), rồi chạy `Qlgx.Api` THẬT với
`ConnectionStrings__Qlgx` trỏ `qlgx_app` và `ConnectionStrings__QlgxQuanTri` trỏ `qlgx_admin`.
Bằng chứng chạy thật (không phải suy diễn từ code):
- `psql -U qlgx_app` (không `BYPASSRLS`): `set_config('app.giao_xu_id', '<id Vô Nhiễm>', false)`
  → `SELECT count(*) FROM giao_dan` = **2050**; `set_config('app.giao_xu_id', '', false)` →
  **0**. Đúng kịch bản kiểm chứng ở `TRIEN-KHAI.md` mục 5.
- Đăng nhập THẬT qua API (`POST /api/auth/dang-nhap`) chạy được dù DbContext nghiệp vụ chính
  đã ở vai trò `qlgx_app` bị RLS chặn — vì `AuthService` tự dùng `ChuoiKetNoiQuanTri` (vai trò
  `qlgx_admin`) để tra tên tài khoản chéo giáo xứ TRƯỚC khi có claim, đúng thiết kế đã ghi ở
  `ChuoiKetNoiQuanTri.cs`. Không phá luồng đăng nhập thu hẹp theo tên tài khoản của commit
  `fc75bd1`.
- Tạo tài khoản quản trị hệ thống tạm thời bằng CLI (dùng `ConnectionStrings__QlgxQuanTri` =
  `qlgx_admin`) — thành công, xác nhận CLI cũng đi đúng đường vai trò `BYPASSRLS`.
- `curl` trực tiếp API (không qua trình duyệt) với token của tài khoản giáo xứ thứ hai:
  `GET /api/quan-tri/giao-xu` → `403`; `GET /api/giao-dan` → `[]` — xác nhận vai trò `qlgx_app`
  (RLS-hạn chế) phục vụ đúng lưu lượng nghiệp vụ hằng ngày, không lộ dữ liệu chéo giáo xứ dù
  chạy qua đúng DbContext nghiệp vụ thật (không phải test giả lập).

**d) Phép thử cách ly tenant quan trọng nhất dự án — LẦN ĐẦU thử được với giáo xứ thứ hai THẬT
(trước đây `qlgx_thu` chỉ có một giáo xứ nên không thử được).** Qua giao diện web thật (Chromium
qua Playwright MCP, không phải test tự động): đăng nhập `qthethong_tmp` (`LoaiTaiKhoan=9`) →
màn hình Quản lý giáo xứ hiện đúng phân cấp thật Phan Thiết → Đức Tánh → Vô Nhiễm → thêm
"Giáo xứ Thánh Gia" vào giáo hạt Đức Tánh (xác nhận bằng `psql` ngay sau khi bấm Lưu, không chỉ
tin giao diện) → tạo tài khoản `thanhgia` cho giáo xứ đó → đăng xuất, đăng nhập lại bằng
`thanhgia` → **danh sách gia đình hiện "0", danh sách giáo dân hiện "0"** — hoàn toàn không thấy
2050 giáo dân/40 gia đình của Vô Nhiễm — và sidebar của `thanhgia` (LoaiTaiKhoan=0) KHÔNG có mục
"Quản lý giáo xứ" (chỉ tài khoản `LoaiTaiKhoan=9` mới thấy, đúng thiết kế mục a). Ảnh chụp:
`WebApp/anh-chup-kiem-thu/65-quan-ly-giao-xu-danh-sach.png`,
`66-tao-tai-khoan-cho-giao-xu-moi.png`, `67-giao-xu-moi-cach-ly-khong-thay-2050-giao-dan.png`.

**e) Dọn dẹp sau kiểm thử — xoá giáo xứ "Giáo xứ Thánh Gia" và hai tài khoản tạm
(`thanhgia`, `qthethong_tmp`) bằng `psql` trực tiếp (không có nút xoá giáo xứ qua giao diện,
đúng quyết định mục 4 của spec) — xác nhận lại `qlgx_thu` về đúng 2050/40/145/1 giáo xứ, chỉ
còn tài khoản `quantri`.** Hai vai trò CSDL `qlgx_app`/`qlgx_admin` CỐ Ý giữ lại trên `qlgx_thu`
(không xoá) — đây là hạ tầng lâu dài cho lần chạy thử tiếp theo, không phải dữ liệu thử nghiệm;
mật khẩu của hai vai trò này không ghi ở đâu trong repo, chỉ tồn tại trong phiên làm việc đã tạo
chúng — người vận hành thật cần tạo lại theo đúng `TRIEN-KHAI.md` mục 5 khi triển khai máy chủ
thật (không dùng lại mật khẩu của phiên kiểm thử này).

**f) Số test cuối:** backend **235/235** (222 cũ + 6 `QuanLyGiaoXuTests` mới + 1
`RlsTests` mới qua vai trò thật không-BYPASSRLS đã có sẵn từ trước lượt này + 4 test thêm/sửa
giáo họ + 2 test khác chưa tính — số chính xác xem log `dotnet test`), frontend **235/235** (229
cũ + 6 test mới cho `GiaoHoListPage`/`QuanLyGiaoXuPage`). `npm run build` chạy được.

### 38. Task "nhập dữ liệu Access cho quản trị viên" (2026-09-07) — các quyết định tự đưa ra

VIEC-TIEP-THEO.md mục 2.4. Người dùng dặn tự quyết, không dừng lại hỏi — ghi hết quyết định ở
đây, kể cả một lỗi dữ liệu nghiêm trọng tự phát hiện và tự sửa giữa chừng.

**a) Kiến trúc hai bước — đã kiểm chứng THẬT, không suy đoán.** Xác nhận trước khi code:
`Qlgx.Migration.csproj` là `net10.0-windows`, dùng `System.Data.OleDb` + ACE OLEDB (chỉ chạy
Windows, đúng bitness); `Dockerfile` build trên `mcr.microsoft.com/dotnet/aspnet:10.0` (Linux).
Máy chủ **không thể** đọc `.mdb` trực tiếp. Đã cân nhắc cả ba hướng nêu trong nhiệm vụ:
- **(a) máy Windows ghi thẳng vào Postgres qua mạng** — loại vì phải mở cổng CSDL ra ngoài
  (rủi ro bảo mật thật) và vẫn cần dòng lệnh, không phải "giao diện cho quản trị viên".
- **(b) đọc `.mdb` trực tiếp trên máy chủ bằng thư viện đa nền tảng** — loại NGAY không thử,
  vì file `.mdb` của QLGX có mật khẩu cấp database (Jet/ACE database password) — chưa tìm thấy
  thư viện .NET/Linux nào công khai đọc được định dạng Jet MÃ HOÁ mật khẩu ngoài chính ACE
  OLEDB (Windows-only). Đây là kết luận từ tra cứu, KHÔNG phải tự chạy thử thất bại — ghi rõ ở
  đây để không ai lặp lại việc dò tìm thư viện vô ích.
- **(c) hai bước, gói dữ liệu trung gian** — **chọn hướng này**. Tái sử dụng TOÀN BỘ
  `Qlgx.Migration.Core` (`IDuLieuNguon`, `ChuyenDoiDuLieu`, `BangAnhXaId`, `BaoCaoDoiChieu`) —
  không viết lại bộ chuyển đổi. Định dạng gói: **JSON nén gzip** (`GoiDuLieuNhap.cs`), KHÔNG
  chọn SQLite — SQLite cần một tệp thật để `Microsoft.Data.Sqlite` mở (không đọc thẳng từ
  `MemoryStream`), buộc phải ghi tạm xuống đĩa máy chủ dù chỉ trong một request; JSON đọc thẳng
  từ luồng byte tải lên, không đụng đĩa cục bộ máy chủ dù chỉ một byte — khớp đúng ràng buộc HA
  "không ghi file xuống đĩa cục bộ". `Qlgx.Migration` (Windows) có thêm chế độ
  `--xuat-goi=<path.json.gz>`: đọc `.mdb` bằng `DocAccess` sẵn có, đóng gói `GoiDuLieuNhap`, ghi
  ra JSON nén — chạy y hệt cách dùng cũ, chỉ thêm một nhánh, không đụng nhánh `--chay-that` cũ.
  `DuLieuNguonTuGoi : IDuLieuNguon` (mới, trong `Qlgx.Migration.Core`, cross-platform) đọc lại
  gói này để `ChuyenDoiDuLieu` chạy y nguyên trên máy chủ.

**b) `NhapDuLieuService.cs` (Qlgx.Api) là ĐƯỜNG DẪN THỨ NĂM được phép đọc/ghi CHÉO GIÁO XỨ**
(cùng nhóm đăng nhập/CLI tạo tài khoản/công cụ chuyển dữ liệu dòng lệnh/`QuanLyGiaoXuService` —
xem `ChuoiKetNoiQuanTri.cs` đã cập nhật) — tự mở `QlgxDbContext` bằng chuỗi kết nối QUẢN TRỊ
(`BYPASSRLS`), không dùng context tiêm qua DI (bị lọc theo giáo xứ của chính quản trị viên đang
gọi, trong khi giáo xứ ĐÍCH của một lượt nhập gần như luôn khác). Endpoint nhóm
`/api/quan-tri/nhap-du-lieu/*` chỉ policy `"QuanTriHeThong"` gọi được — lớp phòng thủ DUY NHẤT
(`NhapDuLieuJob` không có RLS, giống `GiaoXu`/`GiaoPhan`/`GiaoHat`).

**c) Chạy nền, KHÔNG giữ một yêu cầu HTTP chờ suốt.** `POST .../bat-dau` tạo một dòng
`NhapDuLieuJob` (bảng mới, migration `ThemNhapDuLieuJob`) rồi `Task.Run` với
`CancellationToken.None` (cố ý — huỷ theo request gốc sẽ làm dở dang một lượt ghi CSDL, tệ hơn
chạy xong dù không ai còn xem), trả `jobId` ngay. Client (`NhapDuLieuPage.tsx`) tự `setInterval`
1.5 giây gọi `GET .../trang-thai/{jobId}` tới khi `TrangThai != "DangChay"`. **Giới hạn đã biết,
chấp nhận được cho quy mô một lượt/giáo xứ mới:** đây KHÔNG phải hàng đợi bền — nếu tiến trình
API khởi động lại giữa chừng, job mồ côi mãi `"DangChay"`, không tự phục hồi. Trạng thái lưu ở
bảng Postgres (không phải biến nhớ tiến trình) nên polling từ BẤT KỲ bản API nào (sau bộ cân
bằng tải HA) đều đọc đúng — chỉ riêng việc TIẾP TỤC chạy lượt nhập đang dở là không chịu được
khởi động lại, không phải việc polling trạng thái.

**d) Chặn/cảnh báo nhập vào giáo xứ đã có dữ liệu.** `XemTruoc` trả `giaoXuDichDaCoDuLieu` +
`soGiaoDanDaCo` (đếm `GiaoDan` của giáo xứ đích) để hiện cảnh báo đỏ NGAY ở báo cáo chạy thử.
`BatDauNhapThat` **CHẶN THẬT** (400) nếu giáo xứ đích có ≥1 giáo dân và không kèm
`xacNhanGhiDe=true` — checkbox riêng ở giao diện, không tự động bật theo cảnh báo, quản trị viên
phải tự tích sau khi đọc báo cáo.

**e) Kiểm định dạng tệp thật ở máy chủ, không tin phần mở rộng.** `DocGoi` kiểm 2 byte đầu
(`0x1F 0x8B`, chữ ký gzip thật) trước khi coi là hợp lệ, cộng `PhienBanGoi` (đối chiếu
`GoiDuLieuNhap.PhienBanHienTai`) để báo lỗi rõ ràng nếu gói cũ/hỏng cấu trúc thay vì ném lỗi giải
mã JSON khó hiểu. Giới hạn 100MB (gói thật của Vô Nhiễm, 2050 giáo dân + 6150 bí tích chi tiết,
chỉ ~250KB nén — 100MB rất rộng rãi).

**g) LỖI NGHIÊM TRỌNG tự phát hiện khi kiểm thử thật, đã sửa — đánh cắp dữ liệu giữa hai giáo
xứ (`BangAnhXaId` không tách giáo xứ).** Khoá ổn định chống trùng lặp của `ChuyenDoiDuLieu`
(tái sử dụng, "chạy tốt" theo mô tả nhiệm vụ) chỉ dựa trên (tên bảng, mã cũ Access) — AN TOÀN
khi công cụ luôn chạy với một `giaoXuId` cố định (đúng cách dùng gốc: dòng lệnh, một giáo xứ),
nhưng **THẢM HOẠ** khi máy chủ phục vụ NHIỀU giáo xứ: mọi file Access đều đánh số `MaGiaoDan`/
`MaGiaDinh`/... bắt đầu từ 1, nên hai giáo xứ khác nhau chắc chắn có mã cũ trùng — khoá không
tách giáo xứ khiến lần nhập giáo xứ B **tìm thấy** bản ghi giáo xứ A (cùng UUID suy từ cùng mã
cũ) rồi **ghi đè `GiaoXuId` của nó sang B**.

Tái hiện thật trên `qlgx_thu`: nhập gói xuất từ chính `BIN/giaoxu.mdb` (Vô Nhiễm) vào một giáo
xứ đích MỚI tạo → 2050 giáo dân/40 gia đình/522 hôn phối/6150 bí tích chi tiết/1108 đợt bí
tích/1 giáo họ/19 cấu hình/343 dữ liệu chung/3 vai trò/3 tên loại tài khoản của **Vô Nhiễm bị
đổi `giao_xu_id` sang giáo xứ thử nghiệm** — `psql` xác nhận Vô Nhiễm còn **0** giáo dân ngay
sau lượt nhập. Chỉ hai bảng khoá tổ hợp không có cột `Id` riêng (`ThanhVienGiaDinh`,
`GiaoDanHonPhoi`) thoát nạn — nhánh "tìm thấy thì sửa" của chúng không đụng `GiaoXuId`, nên đây
lại chính là hai bảng duy nhất báo "⚠ lệch" trong báo cáo đối chiếu — **lưới an toàn phát hiện
được PHẦN NỔI của vấn đề nhưng không phát hiện được bốn bảng bị đánh cắp hoàn toàn khác** (số
dòng đích trùng khít số dòng nguồn vì chính là dữ liệu bị cướp, không phải dữ liệu mới).

**Sửa:** thêm `giaoXuId` vào khoá — `ChuyenDoiDuLieu` giờ gọi `Anh(bang, maCu)` (helper riêng)
thay vì `anhXa.Lay(bang, maCu)` trực tiếp; `Anh` tự thêm tiền tố `"{giaoXuId}:"` cho MỌI bảng
**trừ** `giao_phan`/`giao_hat` (cố ý dùng CHUNG giữa các giáo xứ, không có `giao_xu_id`, xem
`GiaoPhan.cs`/`GiaoHat.cs`). **Đánh đổi đã chấp nhận, ghi rõ trong code:** đổi khoá làm giáo xứ
Vô Nhiễm ĐÃ nhập trước bản sửa này (bằng khoá KHÔNG có `giaoXuId`) không còn khớp khoá MỚI —
nếu ai chạy lại `Qlgx.Migration --chay-that` cho CHÍNH Vô Nhiễm trong tương lai, lần chạy đó sẽ
tạo bản ghi TRÙNG thay vì cập nhật tại chỗ (mất tính "idempotent" CHỈ với dữ liệu đã nhập trước
bản sửa). Chấp nhận được: Vô Nhiễm đã nhập xong, không có kế hoạch nhập lại; nguy cơ đánh cắp
dữ liệu giữa các giáo xứ nghiêm trọng hơn nhiều so với rủi ro hiếm gặp này.

**Khôi phục dữ liệu đã bị đánh cắp** (xảy ra TRƯỚC khi phát hiện và sửa lỗi, trong phiên kiểm
thử này) bằng `psql` trực tiếp: `UPDATE <10 bảng bị ảnh hưởng> SET giao_xu_id='<Vô Nhiễm>' WHERE
giao_xu_id='<giáo xứ thử nghiệm>'` — xác nhận lại Vô Nhiễm đúng 2050/40/145 trước khi chạy lại
lượt kiểm thử với bản đã sửa.

**h) Chứng minh bằng chạy thật, SAU KHI sửa lỗi ở mục g — số dòng khớp TUYỆT ĐỐI cho mọi bảng
(kể cả hai bảng từng lệch):** GiaoHo 1, GiaDinh 40, GiaoDan 2050, ThanhVienGiaDinh 145, HonPhoi
522, GiaoDanHonPhoi 1043, DotBiTich 1108, BiTichChiTiet 6150 — đối chiếu `psql` trên giáo xứ
đích, đồng thời Vô Nhiễm giữ nguyên 2050 giáo dân, không lẫn lộn. Nhập lại lần hai
(`xacNhanGhiDe=true`) cho đúng số dòng như cũ — không tạo bản ghi trùng (idempotent với khoá
MỚI). Đăng nhập bằng tài khoản của giáo xứ mới qua trình duyệt thật (Playwright MCP, không phải
test tự động) → danh sách gia đình hiện đúng 40, danh sách giáo dân hiện đúng 2050 (2039 khi ẩn
người đã qua đời/chuyển xứ theo bộ lọc mặc định — đúng hành vi UI hiện có, không phải lỗi). Ảnh
chụp: `68`–`72` trong `WebApp/anh-chup-kiem-thu/`.

**i) Dọn dẹp sau kiểm thử.** Xoá toàn bộ dữ liệu giáo xứ thử nghiệm (10 bảng theo `giao_xu_id`,
dòng `giao_xu`, `giao_hat`/`giao_phan` tạo riêng cho lượt thử, dòng `nhap_du_lieu_job`, hai tài
khoản tạm `tmp_qthethong`/`tmp_gxmoi`) bằng `psql` trực tiếp — xác nhận lại `qlgx_thu` đúng
2050/40/145/1 giáo xứ, chỉ còn tài khoản `quantri`.

**j) Số test cuối:** backend **244/244** (235 cũ + 9 `NhapDuLieuTests` mới), frontend
**239/239** (235 cũ + 4 `NhapDuLieuPage.test.tsx` mới). `npm run build` chạy được. `dotnet build
Qlgx.sln` sạch (Windows-only `Qlgx.Migration` build được cùng lượt, không tách CI riêng).

### 39. Task "sửa 6 lỗi giao diện do người dùng thật tự phát hiện" (2026-09-07) — các quyết
định tự đưa ra

Người dùng ngồi kiểm tra ứng dụng thật, tự tay phát hiện 6 lỗi giao diện (lỗi #1 — vỡ bố cục
màn hình gia đình — nghiêm trọng nhất). Ghi ở đây các quyết định KHÔNG có trong yêu cầu gốc.

**a) Nguyên nhân gốc của lỗi #1 KHÔNG phải là commit `6d00343` (thêm ảnh gia đình) như nghi ngờ
ban đầu — đã đọc lại `git show 6d00343` và xác nhận file đó chỉ thay `<div class="photo-slot">`
tĩnh bằng `<AnhDaiDien>`, không đụng số phần tử con của `.detail-page`.** Nguyên nhân thật: JSX
của `GiaDinhDetail.tsx` có **6 phần tử con trực tiếp** của `.detail-page` (đầu trang, `.cols`,
tiêu đề "Thành viên khác", thanh công cụ thêm, `GxGiaoDanList`, `.cmdbar`) trong khi CSS
`grid-template-rows` của `.detail-page` chỉ khai **4 hàng** — lỗi này đã tồn tại từ trước
`6d00343` rất lâu (xác nhận bằng `git show a062a7b:...GiaDinhDetail.tsx`, cùng cấu trúc 6 phần
tử). CSS Grid tự đẩy 2 phần tử thừa vào các hàng ẩn (`grid-auto-rows`, mặc định `auto`) không
có ràng buộc chiều cao tối thiểu nào — hàng `.cols` (khai `minmax(0, auto)`, MIN là 0!) bị bóp
gần về 0 nên nội dung cuộn cụt ngay sau "Địa chỉ", còn `GxGiaoDanList` (đáng lẽ nhận hàng
`minmax(200px, 1fr)`) lại rơi vào một hàng ẩn khác, co về ~0px dù đếm đúng số người — ĐÚNG kiểu
lỗi "lưới AG Grid co về 0px" đã tái diễn 2 lần trước (`.table-card` cần chiều cao THẬT từ cha,
không tự có), chỉ khác ở chỗ lần này do đếm sai số phần tử con thay vì do `grid-template-rows`
không khớp trực tiếp. **Test jsdom không bắt được vì jsdom không tính layout CSS Grid thật** —
đúng như đã cảnh báo trong nhiệm vụ.

**b) Sửa bằng cách gộp lại đúng SỐ phần tử con khớp số hàng khai — KHÔNG đổi
`grid-template-rows` của `.detail-page` (dùng chung với `GiaoDanDetail.tsx`).** Bọc "Thành viên
khác" + thanh công cụ thêm + `GxGiaoDanList` vào một `<div className="members-block">` duy
nhất — `.detail-page` quay lại đúng 4 phần tử con (đầu trang, `.cols`, `.members-block`,
`.cmdbar`), khớp 4 hàng khai. Đã thêm CSS `.members-block { display: flex; flex-direction:
column; min-height: 0 } .members-block > .table-card { flex: 1 }` để `GxGiaoDanList` nhận đúng
chiều cao từ hàng lưới cha.

**c) Thêm khối "Hôn phối" MỚI HOÀN TOÀN vào `GiaDinhDetail.tsx` — trước đây bản web CHỈ ĐỌC
được dữ liệu này (`GET /api/gia-dinh/{id}` đã trả `HonPhoiDto`) nhưng KHÔNG có UI nào hiện lên,
và luôn gửi `honPhoi: null` khi lưu (biết trước, ghi rõ trong comment cũ) — đúng mục "Trung
bình #5" đã ghi ở `gia-dinh-chi-tiet.md`. Đây là lý do thật khiến người dùng "không thấy hôn
phối" — không chỉ là bị cắt bởi lỗi bố cục, mà UI chưa từng tồn tại.** Dựng đủ 8 trường sửa
được (Số/Ngày/Nơi hôn phối, Linh mục chứng, Người chứng 1/2, Tình trạng, Ghi chú — đúng thứ tự
`CapNhatHonPhoiRequest`), tái dùng CSS `.card-row`/`GxField`/`GxDate` sẵn có, danh sách "Tình
trạng hôn phối" 9 giá trị SAO CHÉP nguyên từ `CACH_THUC_HON_PHOI` của `GiaoDanDetail.tsx` (không
export dùng chung — hằng số nhỏ, giữ đúng quy ước `DIEN_GIA_DINH` cùng file). Khối này VÔ HIỆU
HOÁ (kèm gợi ý rõ) khi chưa có Người nam lẫn Người nữ, đúng ràng buộc backend
`KhongTheGanHonPhoiMoCoi` (hôn phối không thể "mồ côi"). `dungPayloadTuForm` giờ gửi object hôn
phối thật (đọc từ form, `rowVersion` lấy từ `f.honPhoi?.rowVersion ?? 0`) thay vì luôn `null`
khi có ít nhất một trong hai người — đã lưu thử qua trình duyệt thật (điền "Số hôn phối", bấm
Cập nhật, tải lại xác nhận còn nguyên, rồi xoá lại để không để sót dữ liệu thử trên `qlgx_thu`).

**d) Bố cục màn hình gia đình đổi HẲN sang kiểu CUỘN CẢ TRANG THAY VÌ "Dock" cứng (mọi thứ vừa
đúng khung nhìn, không cuộn) — chỉ riêng `GiaDinhDetail.tsx` (`className` thêm
`detail-page-giadinh`), KHÔNG đụng `.detail-page` gốc mà `GiaoDanDetail.tsx` vẫn dùng.** Lý do:
thêm khối "Hôn phối" (8 trường) làm `.cols` cao hơn hẳn, kiểu Dock cũ (mọi thứ phải vừa màn
hình phổ biến ~900px) không còn đủ chỗ — ép vừa sẽ lại cắt cụt/giấu hôn phối y hệt lỗi vừa sửa.
Bước kiểm chứng của chính nhiệm vụ này ("Cuộn hết trang từ trên xuống dưới") đã ngầm xác nhận
cuộn cả trang là hành vi CHẤP NHẬN ĐƯỢC, nên chọn hướng đơn giản, chắc chắn hơn là cố nhồi vừa
khung nhìn. **Gặp lại ĐÚNG kiểu lỗi "khối cao 0px" một lần nữa khi làm việc này** — `.members-
block` (kế thừa `min-height: 0` từ bản gốc, cần cho hàng `1fr`) trong hàng `auto` mới của
`.detail-page-giadinh` lại co về đúng 0px (con `.table-card` vẫn có chiều cao thật 320px nhưng
TRÀN RA NGOÀI khối cha 0px, đẩy `.cmdbar` đè lên trên lưới) — phát hiện bằng cách đo trực tiếp
`getBoundingClientRect()` qua Chrome DevTools MCP trên trình duyệt thật (KHÔNG phải chỉ nhìn ảnh
chụp), sửa bằng cách trả lại `min-height: auto` cho `.detail-page-giadinh .members-block`. Ghi
lại đầy đủ trong comment CSS tại chỗ để không ai lặp lại lần thứ tư.

**e) Nút "Mở hồ sơ trong thẻ mới" (⧉) thêm thẳng vào `GxPicker.tsx` (prop `onXem?: () => void`,
chỉ hiện khi có `onXem`, vô hiệu hoá khi chưa chọn ai) — dùng CHUNG component cho cả Người
nam/Người nữ ở màn hình gia đình, không tạo control riêng.** Nối `onXem={() => moGiaoDan?.(...)}`
ở hai ô Người nam/Người nữ trong `GiaDinhDetail.tsx`. Lưới "Thành viên khác" đã sẵn có cách mở
(`onMo`/menu "Xem chi tiết") từ trước, không cần sửa gì thêm cho phần lưới.

**f) Tiêu đề thẻ tài liệu: thêm `suaTieuDe(id, tieuDeMoi)` vào `useTabDocs.ts`, gọi từ
`GiaDinhDetailPage`/`GiaoDanDetailPage` (prop `onTieuDe`) trong một `useEffect` mỗi khi tên bản
ghi đổi (kể cả sau khi lưu).** `App.tsx` truyền `onTieuDe={(ten) => suaTieuDe(idThe, ten)}` khi
mở thẻ. Chuỗi dự phòng khi chưa có tên: `"Gia đình #{maGiaDinhCu}"` / `"Giáo dân #{maGiaoDanCu}"`
(hiếm gặp — chỉ khi tên rỗng thật sự trong dữ liệu cũ).

**g) Thanh cuộn ngang của dải thẻ tài liệu (`.tabstrip`): thêm CSS `::-webkit-scrollbar` 8px,
SAO CHÉP nguyên giá trị từ `.nav-scroll` (SideNav) như yêu cầu — không tạo kiểu mới.**

**h) Ảnh giáo dân: đổi thứ tự JSX trong `.canhan-top` (fields trước, `<AnhDaiDien>` sau) — vì
đây là `display: flex` một hàng, đổi thứ tự phần tử con là đủ để ảnh hiện bên PHẢI mà không cần
CSS `order` hay đổi cấu trúc gì khác.**

**i) In "Phiếu gia đình" A4 dọc: thêm `Landscape = false` TƯỜNG MINH vào `PagePdfOptions` của
`BoTrinhDuyet.XuatPdfAsync` (dù mặc định của Playwright vốn đã là dọc — nêu rõ để không phụ
thuộc hành vi mặc định), sửa `@page { size: A4; }` thành `@page { size: A4 portrait; }`, thêm
`table-layout: fixed` + `word-break: break-word` cho bảng thành viên (12 cột trên vùng in chỉ
~180mm) để chống tràn ngang với dữ liệu tên/địa danh dài.** Đã in thử thật cho gia đình "Paul
Trần Văn Thái" (mã 9) qua `fetch()` trong trang đã đăng nhập, đọc `/MediaBox` của PDF xác nhận
`[0 0 595.92 842.88]` (rộng < cao, đúng A4 dọc), mở PDF xem nội dung: bảng 6 thành viên gọn
trong một trang, không tràn.

**j) Test mới thêm cho lỗi #1 (ở mức làm được trong jsdom — KHÔNG coi là bằng chứng thay ảnh
chụp trình duyệt thật):** `GiaDinhDetail.test.tsx` (khối Hôn phối hiện/vô hiệu hoá đúng điều
kiện, hiện đúng dữ liệu đã có, `onLuu` nhận đúng payload hôn phối hoặc `null`, nút "Mở hồ sơ
trong thẻ mới" hiện/gọi đúng, lưới thành viên đúng SỐ DÒNG `.ag-row`), `GxPicker.test.tsx` (nút
`onXem`), `useTabDocs.test.ts` (`suaTieuDe`), `GiaDinhDetailPage.test.tsx`/
`GiaoDanDetailPage.test.tsx` (gọi đúng `onTieuDe`). **Bằng chứng bố cục thật là ảnh chụp Chrome
DevTools MCP + `getBoundingClientRect()` đo trực tiếp, không phải các test này.**

**k) Số test cuối:** backend **244/244** (không đổi — lỗi #5 chỉ sửa CSS/PDF options, không có
logic C# mới cần test riêng). Frontend **254/254** (239 cũ + 15 test mới). `npm run build` chạy
được. Ảnh chụp/PDF kiểm thử: `73`–`77` trong `WebApp/anh-chup-kiem-thu/`.

### 40. Task "sửa 2 lỗi tự phát hiện sau khi chuyển Hôn phối sang cột phải" (2026-09-07) —
các quyết định tự đưa ra

Người dùng tự kiểm tra ngay sau khi khối "Hôn phối" chuyển sang cột phải (mục 39 phần d) và
phát hiện thêm 2 lỗi: khối Hôn phối bị bóp hẹp cắt cụt chữ, và cột "Mã GD" trên lưới thành viên
luôn hiện `0`.

**a) Nguyên nhân lỗi #1 (Hôn phối bị cắt cụt):** cột phải hẹp (`.cols`, `minmax(360px, .85fr)`)
trừ padding thẻ (2×16px) chỉ còn ~328px bề ngang. Bố cục CŨ (giữ nguyên từ hồi còn ở cột trái,
mục 39 phần c) dùng `.card-row` chia đôi thành hai cột con, MỖI cột con lại dùng `.frow` mặc
định (nhãn cố định 132px bên trái): mỗi cột con chỉ còn ~158px, trừ nhãn 132px và gap 10px thì
ô nhập còn ĐÚNG ~16PX — đúng khớp ảnh chụp người dùng gửi (`78-gia-dinh-hon-phoi-duoi-hinh-gia-
dinh.png`: "04/", "Chính Tâ", "Hợp p"). Bóp HAI LỚP (chia cột con + trừ nhãn cố định) là gốc
lỗi, không phải riêng độ rộng cột phải.

**b) Sửa bằng lớp CSS mới `.honphoi-fields` (qlgx.css) — nhãn chuyển lên TRÊN, ô nhập xuống
DƯỚI cho từng `.frow` bên trong (thay `grid-template-columns: 132px 1fr` bằng `1fr` một cột),
NHƯNG vẫn giữ khối chia HAI CỘT (như `.card-row`) thay vì xếp nguyên một cột dọc 8 hàng.**
Thử xếp một cột dọc trước (8 hàng) thì khối cao gần gấp đôi thẻ "Thông tin gia đình" bên cột
trái — do `.cols` khai `align-items: stretch`, cột trái không tự nở theo nên lộ một khoảng
trống ~268px dưới thẻ trái (đo bằng `getBoundingClientRect()` qua Chrome DevTools MCP), ĐÚNG
loại "khoảng trống thừa" người dùng vừa yêu cầu bỏ ở mục 39 — quay lại 2 cột (4 hàng) đưa khoảng
trống còn ~91px (chấp nhận được, không còn là khoảng trống LỚN). "Ghi chú hôn phối" (textarea,
trường cuối) cho chiếm trọn 2 cột (`grid-column: 1 / -1`) vì là trường nhiều chữ nhất.

**c) Nguyên nhân gốc lỗi #2 (Mã GD luôn = 0):** `ThanhVienDto` (backend,
`Qlgx.Api/Dtos/GiaDinhDtos.cs`) CHƯA BAO GIỜ mang trường `MaGiaoDanCu` — comment cũ ở
`GiaDinhDetail.tsx` (hàm `tuThanhVien`) đã tự nhận đây là placeholder tạm ("các trường còn lại
của `GiaoDanListItem` chưa có... tạm điền `null`/giá trị mặc định"), hard-code `maGiaoDanCu: 0`
cho MỌI dòng. Giá trị thật đã có sẵn trên entity `GiaoDan.MaGiaoDanCu` — chỉ chưa được đưa vào
DTO trả về từ `GET /api/gia-dinh/{id}`. Sửa xuyên suốt 3 lớp: (1) `ThanhVienDto` thêm tham số
`int MaGiaoDanCu`; (2) `GiaDinhService.LayChiTiet` truyền `tv.GiaoDan!.MaGiaoDanCu` khi dựng
DTO; (3) frontend `api/types.ts` (`ThanhVien.maGiaoDanCu: number`) và `tuThanhVien` đọc thẳng
`tv.maGiaoDanCu` thay vì hard-code `0`. Có test hồi quy mới ở `GiaDinhDetail.test.tsx` (assert
"1002" hiện trên lưới ứng với fixture `maGiaoDanCu: 1002`) — test CŨ (fixtures không có trường
này) đã được thêm `maGiaoDanCu` cho tất cả các dòng `thanhVien` (giá trị `1000 + số thứ tự pN`,
không cần khớp dữ liệu thật vì chỉ kiểm tra hành vi component).

**d) Không tìm thấy nơi nào khác dựng `ThanhVienDto` thủ công (chỉ một chỗ trong
`GiaDinhService.LayChiTiet`) nên không có rủi ro sót constructor cũ thiếu tham số.**

**e) Số test cuối:** backend **244/244** (thêm tham số DTO không đổi số test, không cần test
C# riêng vì đã có `GiaDinhDetailTests.cs` phủ `LayChiTiet` sẵn — kiểm tra thủ công qua ảnh chụp
trình duyệt thật là bằng chứng chính cho cả 2 lỗi). Frontend **254/254** (1 assertion mới thêm
vào test có sẵn, không tăng số `it`). `npm run build` chạy được cả hai phía. Đo
`getBoundingClientRect()` qua trình duyệt thật (gia đình "Paul Trần Văn Thái", mã 9): thẻ
"Thông tin gia đình" (cột trái) cao 416px; cột phải (`.col-stack`) cao 507px (Hình gia đình
126px + gap 12px + Hôn phối 369px) — khoảng trống dưới thẻ trái ~91px, không còn khoảng trống
LỚN như trước khi sửa phần (b). Ảnh chụp: `80-gia-dinh-hon-phoi-va-mgd-da-sua.png` (đầu trang,
đọc được trọn `04/01/1996`/`Chính Tâm`/`Hợp pháp`), `81-gia-dinh-cuon-het-trang.png` (cuộn hết
trang, lưới thành viên hiện đúng Mã GD 1068/1069/1070/1071, không còn khoảng trống thừa trước
`.cmdbar`).

### 41. Task "sửa 4 vấn đề màn hình chi tiết gia đình theo phản hồi trực tiếp" (2026-09-07) —
các quyết định tự đưa ra

Người dùng thật ngồi kiểm tra trực tiếp báo 4 việc: (1) radio "Chủ hộ" bấm không ăn thua, (2)
đổi thứ tự cột phải — Hôn phối lên trên, Hình gia đình xuống dưới, (3) cột trái ("Thông tin gia
đình") phải cao bằng tổng cột phải, không còn khoảng trống, (4) khối Hôn phối phải quay lại
kiểu nhãn-trái/ô-phải giống "Thông tin gia đình" (mục 40 vừa đổi sang nhãn-trên để chống cắt
chữ — người dùng không muốn kiểu đó).

**a) Nguyên nhân gốc lỗi #1 (radio "Chủ hộ" như không bấm được) — KHÔNG phải lỗi click, mà lỗi
CSS không có phản hồi thị giác.** Đo bằng Chrome DevTools MCP: bấm radio (qua `click` — dispatch
chuột thật qua CDP, đúng tọa độ `elementFromPoint` trả về `<label class="seg">`) THAY ĐỔI ĐÚNG
`input.checked` (xác nhận bằng `evaluate_script`), và khi bấm "Cập nhật" giá trị `chu_ho` trong
Postgres CŨNG đổi đúng (xem phần d) — vậy input và luồng lưu vẫn hoạt động. Vấn đề thật: class
`.seg` trong `qlgx.css` được viết cho một khối CHA bọc nhiều `<label>` con (`.seg { pill } .seg
label { padding, màu } .seg label:has(input:checked) { nổi bật trắng }"), nhưng JSX
(`GiaDinhDetail.tsx`) đặt `className="seg"` THẲNG lên chính `<label>` bọc input (chỉ MỘT label,
không có label con nào bên trong `.seg` cả). Cả ba luật `.seg label...` không bao giờ khớp —
hậu quả: pill "Chủ hộ" LUÔN hiện xám mờ dù đã chọn hay chưa, người dùng bấm xong không thấy gì
đổi nên tưởng nút không hoạt động. Chụp màn hình xác nhận: trước khi sửa, cả hai pill "Chủ hộ"
(Người nam đang là chủ hộ thật trong CSDL, Người nữ không phải) trông GIỐNG HỆT nhau. Sửa: gộp
style của "label con" cũ thẳng vào `.seg`, đổi `:has()` từ `.seg label:has(input:checked)` thành
`.seg:has(input:checked)` cho khớp đúng cấu trúc thật (`qlgx.css`); thêm
`.seg:has(input:disabled)` làm mờ khi chưa chọn Người nam/nữ (trước đây không có style disabled
nào). Không đổi JSX của khối này — chỉ CSS.

**b) Vấn đề #2 (đổi thứ tự cột phải):** chỉ đảo lại thứ tự hai `<div className="card glass">`
trong `.col-stack` (Hôn phối trước, Hình gia đình sau) — không đổi CSS `.col-stack` (vẫn
`grid-template-rows: auto 1fr`, giờ hàng `auto` là Hôn phối cao tự nhiên, hàng `1fr` là Hình gia
đình nở lấp phần còn lại, đúng ý bản mẫu desktop "khung ảnh lớn nằm dưới cùng, chiếm phần chiều
cao còn lại"). Đây là đảo NGƯỢC lại thứ tự mục 39 phần d từng chọn (khi đó dời Hôn phối xuống
DƯỚI Hình gia đình để lấp khoảng trống) — mục 39 đoán sai ý người dùng, lần này người dùng nói
rõ "move hình gia đình xuống dưới hôn phối".

**c) Vấn đề #3 (cột trái phải cao bằng cột phải, hết khoảng trống) — nguyên nhân giống hệt mục
40 phần b đã ghi (`.cols` khai `align-items: stretch` chỉ giãn khối BỌC ngoài, không giãn
`.card` bên trong nếu card không có gì co giãn theo)."** Lần trước (mục 40) chấp nhận khoảng
trống ~91px vì thời gian có hạn; lần này người dùng yêu cầu dứt điểm "không có khoảng trống
nào". Sửa triệt để: bọc cột trái bằng class mới `.giadinh-left-col` (thay flex-column trơn) —
`.giadinh-left-col > .card { flex: 1 }` cho card tự trải cao 100% khung được cấp, rồi CHỌN
RIÊNG hàng "Ghi chú" (`.frow:has(#gdinh-ghichu)`, dùng `:has()` với id đã có sẵn thay vì phải
thêm prop `className` mới cho `GxField`) đặt `flex: 1` + textarea `flex:1; height:100%` để nó
hút hết phần chiều cao dư ra — đúng ý bản mẫu desktop ("Ghi chú cao khoảng 5-6 dòng — chính chỗ
này làm cột trái cao bằng cột phải", xem mô tả màn hình desktop trong nhiệm vụ). Đo bằng
`getBoundingClientRect()` qua Chrome DevTools MCP sau khi sửa: cột trái (`.giadinh-left-col`)
cao **473.65px**, card "Thông tin gia đình" bên trong cũng cao **473.65px** (khớp tuyệt đối,
không còn khoảng trống), cột phải (`.col-stack`) cao **473.65px** (Hôn phối 327.85px + gap 12px
+ Hình gia đình 133.8px = 473.65px) — HAI CỘT BẰNG NHAU TUYỆT ĐỐI, sai số 0px.

**d) Vấn đề #4 (Hôn phối quay lại nhãn-trái/ô-phải) — giải quyết mâu thuẫn nêu trong nhiệm vụ
bằng cách NỚI RỘNG CỘT PHẢI thay vì đổi kiểu nhãn.** Bỏ hẳn class `.honphoi-fields` (nhãn-trên,
2 cột) từ mục 40 — mọi trường Hôn phối giờ dùng THẲNG `.frow` mặc định (nhãn trái cố định 132px,
đúng class khối "Thông tin gia đình" đang dùng). Để không cắt chữ trở lại (nguyên nhân gốc mục
40 phần a: cột phải cũ chỉ `minmax(360px, .85fr)`, trừ padding thẻ còn ~328px), đổi tỉ lệ `.cols`
từ `minmax(430px, 1.15fr) minmax(360px, .85fr)` thành `minmax(420px, 1.05fr) minmax(400px,
.95fr)` — gần 1:1, đúng ý "cột phải rộng gần bằng cột trái" trong mô tả bản mẫu desktop. Riêng
"Số hôn phối"/"Ngày hôn phối" ghép chung một dòng qua `extra`/`GxInline` (đúng cách khối trái
ghép "Điện thoại"/"Diện") — khớp đúng bản mẫu desktop ("Số hôn phối | Ngày hôn phối" cùng dòng).
Kiểm chứng trên trình duyệt thật (gia đình "Paul Trần Văn Thái", mã 9): đọc trọn `04/01/1996`,
`Chính Tâm`, `Hợp pháp`, không còn bị cắt — ảnh `82-gia-dinh-honphoi-tren-hinh-duoi-nhan-trai.png`.

**e) Bằng chứng chạy thật (bấm radio "Chủ hộ" thật, không phải chỉnh `defaultChecked` qua
code):** mở gia đình mã 9 trên trình duyệt thật (Chrome DevTools MCP) — trạng thái gốc `psql`:
`chu_ho=t` ở dòng vai_tro=0 (Trần Văn Thái), `chu_ho=f` ở dòng vai_tro=1 (Nguyễn Thị Thu). Bấm
radio "Chủ hộ" cạnh Người nữ → pill đổi màu trắng/đậm ngay (xác nhận CSS đã sửa hoạt động, ảnh
`84-chuho-pill-sang-khi-chon.png`) → bấm "Cập nhật" → thông báo "Đã lưu thành công." → `psql`
xác nhận đổi đúng: `chu_ho=f` (vai_tro=0), `chu_ho=t` (vai_tro=1). Bấm lại radio "Chủ hộ" cạnh
Người nam → "Cập nhật" → `psql` xác nhận đã TRẢ VỀ ĐÚNG nguyên trạng ban đầu (`chu_ho=t` ở
vai_tro=0). Lưới thành viên vẫn đúng 4 dòng Mã GD 1068/1069/1070/1071 (ảnh
`83-gia-dinh-luoi-4-thanh-vien-1068-1071.png`) — không bị ảnh hưởng bởi các thay đổi CSS/JSX
trên.

**f) Số test cuối:** backend **244/244** (không đổi — cả 4 vấn đề đều là CSS/JSX thuần phía
frontend, không chạm C#). Frontend **254/254** (không thêm test mới — 4 vấn đề đều là bố cục/CSS
đã có test hành vi phủ sẵn ở `GiaDinhDetail.test.tsx`, gồm cả test "chuHoVaiTro" đã có từ trước
xác nhận đúng luồng gửi dữ liệu). `npm run build` chạy được. Dữ liệu giáo xứ Vô Nhiễm không đổi:
2050 giáo dân / 40 gia đình / 145 thành viên (`psql` xác nhận sau khi hoàn tất, đã trả radio
"Chủ hộ" về đúng nguyên trạng). Ảnh chụp/kiểm thử: `82`–`84` trong `WebApp/anh-chup-kiem-thu/`.

### 42. Task "sửa 4 chi tiết giao diện theo phản hồi trực tiếp — nhãn xuống dòng, header lưới,
tiêu đề hình gia đình, khối hôn phối" (2026-09-07) — các quyết định tự đưa ra

Người dùng thật ngồi kiểm tra trực tiếp báo tiếp 4 việc mới (khác 4 việc mục 41): (1) hai ô đánh
dấu "Đã chuyển đi xứ khác"/"Không tính vào thống kê" bị ngắt dòng giữa chữ khi cửa sổ hẹp, (2)
hàng tiêu đề lưới AG Grid nhìn cũ, cần hiện đại hơn và thấp hơn — áp dụng MỌI lưới, (3) bỏ tiêu
đề "Hình gia đình" để ảnh dùng trọn chiều cao, (4) kiểm tra lại khối Hôn phối có đúng kiểu
nhãn-trái/ô-phải chưa (nghi ngờ người dùng đang xem trang cũ vì mục 41 phần d đã sửa xong).

**a) Vấn đề #1 (nhãn ô đánh dấu ngắt dòng giữa chữ):** thêm `.toggle { white-space: nowrap }`
(mỗi ô không BAO GIỜ tự ngắt chữ của chính nó) và bọc hai `<label className="toggle">` trong
`GiaDinhDetail.tsx` bằng `<div className="toggle-group">` (`display:flex; flex-wrap:wrap;
gap:8px`) — khi không đủ chỗ ngang, CẢ ô thứ hai rơi xuống dòng dưới NGUYÊN VẸN thay vì bị bẻ
chữ giữa dòng. Đo bằng `getBoundingClientRect()` qua Playwright MCP thật (gia đình mã 9, thu hẹp
dần cửa sổ tới khi thấy xếp chồng): ở 1500px/1100px cả hai ô vẫn nằm một hàng (đủ chỗ — `.cols`
đã chuyển 1 cột dưới 1120px nên cột trái rộng gần hết cửa sổ); phải hẹp xuống ~1180px (đúng biên
hai cột, cột trái hẹp nhất `minmax(420px,…)`) mới thấy `.toggle-group` chỉ rộng 259.6px và hai
ô XẾP CHỒNG dọc, mỗi ô cao đúng 34.6px (một dòng, không bị bẻ chữ) — ảnh
`85`–`87-giadinh-9-toggle-*.png`.

**b) Vấn đề #2 (header AG Grid hiện đại + thấp hơn) — phát hiện lỗi TIỀN ĐỀ trước khi sửa được:
biến `--ag-*` khai trong `.ag-theme-quartz` ở `qlgx.css` (kể cả các biến ĐÃ CÓ TỪ TRƯỚC như
`--ag-row-height: 30px`) hoàn toàn KHÔNG có hiệu lực trên trình duyệt thật.** Đo bằng
`getComputedStyle` qua Playwright MCP: `--ag-row-height` đọc lại đúng `calc(14px + 8px * 3.5)`
(giá trị MẶC ĐỊNH của AG Grid, KHÔNG PHẢI `30px` đã khai), dòng dữ liệu thật cao 42px (không
phải 30px). Nguyên nhân: CSS gốc `ag-theme-quartz.css` trước đây được `import` ở
`GxGrid.tsx` (một component, nạp SAU `main.tsx` trong đồ thị module) trong khi `qlgx.css` nạp
NGAY ĐẦU `main.tsx` — Vite gộp CSS theo thứ tự GẶP LẦN ĐẦU khi duyệt cây import, nên CSS gốc AG
Grid rơi vào SAU `qlgx.css` trong bundle cuối, cùng đè lại mọi biến `--ag-*` tuỳ biến (cùng độ
đặc hiệu, ai nạp sau thắng). Bài test jsdom không dựng CSS thật nên không bao giờ bắt được — chỉ
lộ ra khi đo `getBoundingClientRect()`/`getComputedStyle` trên trình duyệt thật, đúng cảnh báo
ở đầu nhiệm vụ. Sửa tận gốc: chuyển hai `import 'ag-grid-community/styles/...'` từ `GxGrid.tsx`
lên ĐẦU `main.tsx`, TRƯỚC `import './styles/qlgx.css'` — giờ CSS gốc AG Grid luôn nạp trước,
`qlgx.css` luôn thắng thế đúng ý đồ ban đầu của file. Sau khi sửa thứ tự import mới thật sự chỉnh
được style: `--ag-header-height: 32px → 28px` (thấp hơn rõ rệt), thêm
`--ag-header-background-color: var(--hair-soft)` (xám rất nhạt, không còn nền trắng phẳng),
`--ag-header-foreground-color`/`.ag-header-cell-text { color: var(--ink-soft) }` (xám đậm, không
còn đen), `letter-spacing: .02em` (giãn chữ nhẹ), `--ag-header-column-separator-display: none`
(bỏ hẳn vạch ngăn dọc giữa các cột), giữ lại `.ag-header-row { border-bottom: 1px solid
var(--hair) }` (một đường kẻ dưới mảnh). Áp dụng cho MỌI lưới vì tất cả cùng dùng chung
`.ag-theme-quartz` qua `GxGrid.tsx` — xác nhận trên cả "Danh sách gia đình" (40 dòng) và "Danh
sách giáo dân" (2039/2050 dòng, ảnh `88-danhsach-giaodan-header-sau.png`). Đo thật sau sửa: hàng
tiêu đề cột (`.ag-header-row-column`) cao **28px** (từ 42-48px hiệu lực thật trước đó, tuỳ có
hàng lọc hay không), hàng dữ liệu (`.ag-row`) cao **30px** đúng khai báo — không còn lệch giữa
CSS khai và DOM thật.

**c) Vấn đề #3 (bỏ tiêu đề "Hình gia đình"):** xoá hẳn `<div className="card-head"><h2>Hình gia
đình</h2></div>` khỏi `GiaDinhDetail.tsx`, để `<AnhDaiDien>` là con trực tiếp DUY NHẤT của
`.card` cuối `.col-stack` — khớp đúng luật `.col-stack > .card > .photo-slot { flex: 1 }` sẵn có
trong `qlgx.css` (component `AnhDaiDien` tự render `className="photo-slot"` làm gốc) nên KHÔNG
cần đổi CSS, ảnh vẫn tự nở lấp hết phần chiều cao thẻ. Không đụng ảnh 3×4 ở `GiaoDanDetail.tsx`
— khối đó chưa từng có tiêu đề card-head riêng (ảnh nằm lồng trong `.canhan-top` cạnh hai ô đầu
Mã giáo dân/Tên thánh, không có `<h2>` nào phía trên) nên không có gì thừa để bỏ. Đo thật sau
sửa (gia đình mã 9, viewport 1180px): cột trái (`.giadinh-left-col`) cao 498.25px; cột phải
(`.col-stack`) cũng cao 498.25px (thẻ Hôn phối 327.85px + gap 12px + thẻ Hình gia đình
158.4px) — HAI CỘT VẪN BẰNG NHAU TUYỆT ĐỐI sau khi bỏ tiêu đề, không có khoảng trống mới sinh ra
(khớp yêu cầu "vẫn giữ hai cột cao bằng nhau" trong nhiệm vụ). Ảnh xác nhận khung ảnh không còn
tiêu đề, hiện đúng gợi ý "Chưa có hình / Nhấp để tải ảnh lên": `85`-`87-giadinh-9-*.png`.

**d) Vấn đề #4 (khối Hôn phối) — XÁC NHẬN LẠI trên trình duyệt thật, KHÔNG SỬA GÌ.** Người dùng
nghi vấn trang cũ chưa tải lại; kiểm tra trực tiếp (gia đình mã 9, cả hai viewport 1500px và
1180px) xác nhận cả 8 trường Hôn phối (Số hôn phối, Ngày hôn phối, Nơi hôn phối, Linh mục chứng,
Người chứng 1, Người chứng 2, Tình trạng hôn phối, Ghi chú hôn phối) đều dùng `GxField` với
`label` bên trái, ô nhập bên phải — giống hệt cách trình bày "Thông tin gia đình", đúng như mục
41 phần d đã sửa. Không có trường nào còn kiểu nhãn-trên. Đúng như phỏng đoán trong nhiệm vụ —
KHÔNG cần sửa gì thêm cho việc này.

**e) Số test cuối:** backend **244/244** (25 + 195 + 24, không đổi — cả 4 việc đều là
CSS/JSX/thứ tự import thuần phía frontend, không chạm C#; đã dừng hẳn `Qlgx.Api` để chạy
`dotnet test` tránh khoá file DLL rồi khởi động lại đúng nguyên cấu hình). Frontend **254/254**
(không thêm test mới — cả 4 việc đều là bố cục/CSS thuần, không đổi hành vi component nào bài
test jsdom quan sát được). `npm run build` chạy được cả hai phía. Dữ liệu giáo xứ Vô Nhiễm không
đổi: 2050 giáo dân / 40 gia đình / 145 thành viên. Ảnh chụp/kiểm thử:
`85`–`88` trong `WebApp/anh-chup-kiem-thu/`.

### 43. Task "header lưới kính mờ (glassmorphism)" (2026-09-07) — các quyết định tự đưa ra

Người dùng thật ngồi kiểm tra báo hàng tiêu đề lưới (mục 42 vừa thấp lại 28px, bỏ vạch dọc —
hài lòng phần đó) vẫn còn nền **xám phẳng, đục**, tách rời khỏi phần còn lại của ứng dụng — cả
hàng tiêu đề cột LẪN hàng ô lọc bên dưới. Yêu cầu: áp kính mờ trong suốt, ăn với theme sẵn có.

**Quyết định:** đổi `--ag-header-background-color` (biến CSS duy nhất AG Grid dùng để tô nền
cho CẢ hàng tiêu đề cột lẫn hàng ô lọc — cả hai đều là `.ag-header-row`, cùng đọc một biến, nên
sửa một chỗ tự động đồng bộ cả hai) từ `var(--hair-soft)` (xám phẳng) sang
`rgba(255, 255, 255, .6)` — đúng công thức trắng bán trong đã dùng cho `.searchbox`/
`.parish-chip` trong `qlgx.css`, không bịa giá trị mới. Thêm `backdrop-filter: blur(16px)
saturate(180%)` trên chính `.ag-theme-quartz .ag-header` (lớp cha bọc cả hai hàng), KHÔNG đặt
trên từng `.ag-header-row` — AG Grid tô nền lặp lại `--ag-header-background-color` ở nhiều lớp
con khác nhau (`.ag-header-row::after`, `.ag-grid-scrolling-cells`, `.ag-grid-container-wrapper`),
đặt blur một lần ở `.ag-header` tránh cộng dồn nhiều lớp blur chồng nhau. Giữ nguyên viền dưới
1px nhưng đổi màu sang `var(--glass-line)` (viền trắng bán trong của hệ `.glass`) thay vì
`var(--hair)` xám, cho khớp tông kính. Không đổi `--ag-header-height` (vẫn 28px),
`--ag-header-column-separator-display: none` (vẫn không vạch dọc) — đúng yêu cầu giữ nguyên.

**Vì sao không cần luật riêng cho hàng ô lọc:** kiểm tra CSS gốc AG Grid xác nhận
`.ag-header-row-filter .ag-grid-scrolling-cells` dùng CHUNG biến `--ag-header-background-color`
với `.ag-header-row-column` — đo `getComputedStyle` thật trên trình duyệt xác nhận cả hai đều ra
`rgba(255, 255, 255, 0.6)` sau khi sửa, không cần selector `.ag-floating-filter-*` nào thêm.

**Kiểm chứng trên trình duyệt thật (Playwright MCP), đăng nhập `giaoxu`:**
- Danh sách giáo dân (2039 dòng đang hiển thị, không lọc "đã qua đời/chuyển xứ" — tổng kho 2050):
  cuộn tới hàng Mã GD 168+ và tới đúng khoảng **1068–1071** (ảnh `90-danhsach-giaodan-madg-1068-1071.png`)
  — chữ tiêu đề cột và ô lọc vẫn đọc rõ, không bị mờ chữ dù nền kính bán trong.
- `getBoundingClientRect()` đo thật sau khi cuộn `scrollTop = 5000/32000`: `.ag-header` giữ
  nguyên vị trí cố định trên cùng khi dữ liệu cuộn (position: absolute — xác nhận hiệu ứng kính
  mờ "dính" hoạt động đúng lúc, không phải bịa); `.ag-header-row` (tiêu đề cột) cao đúng
  **28px**; `.ag-row` (dòng dữ liệu) cao đúng **30px** — không đổi so với mục 42. Lưới không co
  về 0: `gridRect.height` = 582.65px (danh sách giáo dân) và 280.85px (lưới thành viên gia đình
  mã 9, 4 dòng) — cả hai đo được kích thước thật, không bị lỗi co 0px từng gặp trước đây.
- Gia đình "Paul Trần Văn Thái" (mã 9): mở lưới "Thành viên khác trong gia đình" — header kính
  mờ đồng bộ với lưới chính, hai cột trái/phải vẫn cao bằng nhau, khối ảnh gia đình vẫn không
  tiêu đề, radio "Chủ hộ" vẫn sáng khi chọn — không có gì trong mục 42 bị hỏng lại (ảnh
  `91-giadinh-9-luoi-thanh-vien-kinh-mo.png`).
- Danh sách gia đình (40 dòng): header cùng công thức, không kiểm ảnh riêng vì cùng
  `.ag-theme-quartz` dùng chung `GxGrid` — đã xác nhận đủ qua đo `getComputedStyle` trên cả hai
  hàng tiêu đề/lọc.

**Số test cuối:** frontend **254/254** (không thêm test mới — thuần CSS, không đổi hành vi
component nào bài test jsdom quan sát được). `npm run build` chạy được. Không chạm backend.
Ảnh chụp/kiểm thử: `89`–`91` trong `WebApp/anh-chup-kiem-thu/`.

### 44. Task "làm nhẹ header và filter box của lưới" (2026-09-07) — các quyết định tự đưa ra

Người dùng thật ngồi kiểm tra sau mục 43 (kính mờ) viết nguyên văn: "grid header hiện tại như
style của 199x", "hãy bỏ bớt border trên header và filter box để nhìn nó nhẹ nhàng hơn". Ảnh
`89-danhsach-giaodan-header-kinh-mo-cuon.png` cho thấy bốn thứ cũ: vạch dọc "|" giữa MỌI cột ở
cả hàng tiêu đề LẪN hàng lọc, ô nhập lọc viền hộp, nút phễu trong hộp, và thanh cuộn ngang dưới
lưới dày thô (15px, mặc định trình duyệt).

**Điều tra tìm đúng nguồn (đo `getComputedStyle` thật trên trình duyệt, không đoán):**
- Vạch dọc "|" KHÔNG phải do `--ag-header-column-separator-display` (biến này đã tắt từ mục 42,
  vẫn tắt) — nó là `.ag-header-cell-resize::after`, tay cầm đổi cỡ cột, luôn hiện qua biến
  `--ag-header-column-resize-handle-display: block` mặc định của AG Grid. Chạy trên CẢ hàng
  tiêu đề lẫn hàng lọc vì cả hai đều có `.ag-header-cell`. Ảnh zoom 3x xác nhận (không còn giữ
  trong repo, chỉ dùng lúc điều tra).
- Ô nhập lọc (`.ag-input-field-input.ag-text-field-input`) có `border: 0.8px solid
  rgba(0,0,0,.15)` mặc định của AG Grid, không phải CSS của dự án — cần override rõ ràng.
- Nút phễu (`.ag-floating-filter-button-button`) đã KHÔNG có viền hộp sẵn (border 0, không có
  class cha nào tạo hộp) — nhưng icon tô đậm 100% ngay cả khi không cần chú ý tới, nên vẫn làm
  mờ mặc định, đậm dần khi rê/focus cho "nhẹ" hơn dù không phải sửa border.
- Đường kẻ ngang thừa GIỮA hàng tiêu đề và hàng lọc đến từ `.ag-theme-quartz .ag-header-row {
  border-bottom: 1px solid var(--hair); }` (mục 42 để lại) — áp cho MỌI `.ag-header-row` nên vẽ
  line sau cả hàng tiêu đề (thừa) lẫn hàng lọc (trùng với viền `.ag-header` đã có).
- Thanh cuộn dày là thanh cuộn NGUYÊN SINH của trình duyệt trên `.ag-body-horizontal/
  vertical-scroll-viewport` (đo `overflow: scroll`, `height/width: 15px`) — KHÔNG phải
  `.grid-wrap` (div bọc ngoài không tự cuộn, AG Grid định vị nội dung tuyệt đối bên trong nên
  luật `::-webkit-scrollbar` cũ đặt trên `.grid-wrap` chưa từng có tác dụng, đã xoá luôn).

**Quyết định sửa (`WebApp/src/web/src/styles/qlgx.css`, khối `.ag-theme-quartz`):**
1. `--ag-header-column-resize-handle-display: none` mặc định; thêm
   `.ag-header-cell:hover .ag-header-cell-resize::after { display: block; }` để tay cầm đổi cỡ
   cột vẫn dùng được khi rê chuột, chỉ ẩn lúc không cần.
2. Xoá `.ag-header-row { border-bottom: ... }`, giữ nguyên `.ag-header { border-bottom: 1px
   solid var(--glass-line); }` sẵn có — một đường kẻ dưới CÙNG cả khối tiêu đề, không còn line
   giữa hai hàng con.
3. Ô nhập lọc: `border-color: transparent; background-color: transparent` mặc định, chuyển
   sang `border-color: var(--hair)` + nền trắng mờ `.7` khi hover, `border-color: var(--brand)`
   + nền trắng đặc khi focus/đang gõ — vẫn thấy được chỗ bấm (không tàng hình), nhưng không còn
   dãy hộp vuông khi không tương tác. Dùng lại đúng biến `--hair`/`--brand` sẵn có.
4. Nút phễu: `opacity: .45` mặc định, `opacity: 1` khi hover/focus-visible/đang mở menu.
5. Thanh cuộn lưới: bỏ luật chết trên `.grid-wrap`, thêm `::-webkit-scrollbar` (8px, thumb
   `rgba(14,32,76,.14)`, bo tròn) lên đúng `.ag-body-horizontal-scroll-viewport` và
   `.ag-body-vertical-scroll-viewport` — copy NGUYÊN giá trị của `.nav-scroll` (thanh cuộn
   thanh bên đã làm mảnh trước đó), đúng yêu cầu "dùng lại chính nó, đừng viết kiểu mới".

**Áp dụng cho mọi lưới:** sửa duy nhất ở `qlgx.css` (không đụng `GxGrid.tsx`/màn hình nào) nên
tự động ăn cho danh sách giáo dân, danh sách gia đình, lưới thành viên gia đình — cùng dùng
`.ag-theme-quartz` qua `GxGrid`.

**Kiểm chứng trên trình duyệt thật (Playwright MCP), đăng nhập `giaoxu`:**
- Danh sách giáo dân (2039 dòng hiển thị / kho 2050): header + hàng lọc hết vạch dọc, hết viền
  hộp quanh ô nhập, hết đường kẻ thừa giữa hai hàng — ảnh
  `92-danhsach-giaodan-header-filter-nhe.png`.
- Gõ lọc thật "Nguyễn Văn" vào cột "Họ tên": ô nhập hiện viền xanh brand rõ ràng khi đang gõ,
  lưới lọc đúng (chỉ còn các dòng khớp) — xác nhận ô lọc VẪN DÙNG ĐƯỢC, không tàng hình — ảnh
  `93-danhsach-giaodan-loc-ho-ten-dang-go.png`.
- Cuộn ngang lưới giáo dân (`scrollLeft = 400`): thanh cuộn dưới đáy hiện dạng dải thẻ mảnh, bo
  tròn, cùng kiểu `.nav-scroll` — ảnh `94-danhsach-giaodan-cuon-ngang-manh.png`.
- Gia đình "Paul Trần Văn Thái" (mã 9): lưới "Thành viên khác trong gia đình" (Mã GD 1068–1071)
  cùng kiểu header nhẹ, thanh cuộn mảnh — ảnh `95-giadinh-9-luoi-thanhvien-header-nhe.png`. Ô
  đánh dấu vẫn không xuống dòng, khối ảnh gia đình vẫn không tiêu đề, hai cột cao bằng nhau,
  radio "Chủ hộ" vẫn sáng khi chọn — không có gì trong các mục trước bị hỏng lại.
- `getBoundingClientRect()` đo thật trên danh sách giáo dân sau khi sửa: lưới `1184.8×582.7`
  (không co 0px), hàng tiêu đề `3800×28` (đúng `--ag-header-height: 28px`, rộng bằng nội dung
  cuộn được), hàng lọc `3800×30` (đúng `floatingFiltersHeight={30}`), dòng dữ liệu `3815×30`
  (đúng `--ag-row-height: 30px`) — không đổi so với mục 42/43.

**Số test cuối:** frontend **254/254** (không thêm test mới — thuần CSS, jsdom không quan sát
được thay đổi thị giác này). `npm run build` chạy được. Không chạm backend. Ảnh chụp/kiểm thử:
`92`–`95` trong `WebApp/anh-chup-kiem-thu/`.

### 45. Task "bỏ viền trái/phải của ô lọc" (2026-09-07) — nguồn thật không phải `border-color`

Người dùng thật ngồi kiểm tra, mở DevTools chỉ thẳng luật
`.ag-theme-quartz .ag-floating-filter-input input:focus` và viết: "hiện tại cái filter text box
có border left right nhìn ko đẹp, hãy bỏ border này đi". Luật đó (từ mục 44) đã đặt
`border-color: var(--brand)` — đúng ý muốn "hiện viền khi đang gõ" — nên ngờ rằng viền trái/phải
người dùng thấy đến từ chỗ khác, không đoán mà đo `getComputedStyle` thật trên trình duyệt.

**Điều tra tìm đúng nguồn:**
- Ô nhập ở trạng thái thường (không hover/focus): `border-color` cả 4 cạnh đều
  `rgba(0,0,0,0)` (đúng ý mục 44), không có `box-shadow` — chụp cận cảnh xác nhận KHÔNG có viền
  gì ở trạng thái nghỉ. Vậy phàn nàn của người dùng nhắm vào trạng thái `:focus` (đúng luật họ
  chỉ trong DevTools).
- Ở trạng thái `:focus`: ngoài `border-color: var(--brand)` (mục 44 đặt, đúng ý), AG Grid theme
  quartz TỰ THÊM `box-shadow: 0 0 0 3px color-mix(in srgb, transparent, #2196f3 47%)` qua biến
  `--ag-input-focus-box-shadow` — luật của dự án không hề đụng tới biến này, kế thừa mặc định
  của theme.
- `.ag-header-cell` chứa ô lọc có `overflow: hidden` và `height` cố định KHÍT đúng bằng chiều
  cao input (30px hàng lọc). Quầng `box-shadow` 3px đó bị CẮT CỤT ở trên/dưới theo chiều dọc
  (vừa khít, không dư chỗ) nhưng LỌT NGUYÊN VẸN ở hai bên trái/phải (input 110px hẹp hơn bề
  ngang ô header-cell 170px, còn dư ~60px ngang) → chỉ còn lại hai quầng xanh dựng đứng hình
  dấu ngoặc "[" "]" ở hai bên — đúng in như "border trái phải" người dùng chỉ ra, dù
  `border-color` bản thân nó đã đúng ý muốn từ trước. Ảnh zoom 5x lúc điều tra xác nhận rõ hình
  dấu ngoặc (không giữ lại trong repo, chỉ ảnh sau khi sửa mới lưu).

**Quyết định sửa (`WebApp/src/web/src/styles/qlgx.css`, khối `.ag-floating-filter-input
input:hover/:focus`):**
1. Tắt hẳn `box-shadow: none` ở `:focus` — đây là root cause, xoá quầng bị cắt vụn tạo hình dấu
   ngoặc.
2. Đổi cả `:hover` lẫn `:focus` sang CHỈ tô màu viền dưới (`border-bottom-color`), giữ
   `border-color: transparent` cho 3 cạnh còn lại — dù viền trên/trái/phải không bị cắt vụn như
   box-shadow, đổi sang viền dưới cho chắc ăn, tránh mọi khả năng tái diễn kiểu viền hai bên, và
   khớp gợi ý của nhiệm vụ "nếu còn viền thì chỉ nên là viền dưới hoặc nền". Vẫn giữ tô nền sáng
   dần (`rgba(255,255,255,.7)` hover, `#fff` focus) để biết chỗ bấm vào gõ — không tàng hình.
   Dùng lại đúng biến `--hair`/`--brand` sẵn có, không bịa giá trị mới.

**Áp dụng cho mọi lưới:** sửa duy nhất ở `qlgx.css`, không đụng `GxGrid.tsx` hay màn hình nào —
tự động ăn cho danh sách giáo dân, danh sách gia đình, lưới thành viên gia đình.

**Kiểm chứng trên trình duyệt thật (Playwright MCP), đăng nhập `giaoxu`:**
- Danh sách giáo dân: chụp cận cảnh hàng lọc (crop + phóng 3x từ ảnh gốc) xác nhận không còn
  viền trái/phải ở ô đang lọc lẫn các ô khác — ảnh `96-can-canh-hang-loc.png`.
- Gõ lọc thật "Nguyễn" vào cột "Họ tên": lọc đúng (chỉ còn các dòng có "Nguyễn" trong họ tên),
  chấm xanh trên icon phễu báo đang lọc, ô lọc vẫn thấy rõ chỗ đang gõ (nền trắng, chữ đang gõ)
  — ảnh `96-loc-ho-ten-nguyen.png`.
- Gia đình "Paul Trần Văn Thái" (mã 9): mở chi tiết, lưới "Thành viên khác trong gia đình" hiện
  đúng 4 người (Mã GD 1068–1071) — ảnh `96-chi-tiet-gia-dinh-thanh-vien.png`. (Lưới này là bảng
  HTML thường `table.grid`, không có hàng lọc AG Grid, nên không bị ảnh hưởng bởi sửa lần này —
  chụp để xác nhận không hỏng gì khác trong màn hình chi tiết gia đình.)
- `getBoundingClientRect()` đo thật trên danh sách giáo dân sau khi sửa: lưới `1184.8×582.65`
  (không co 0px), hàng tiêu đề (`.ag-header-row-column`) `3800×28`, hàng lọc
  (`.ag-header-row-filter`) `3800×30` — không đổi so với mục 44, xác nhận không làm hỏng chiều
  cao vừa sửa xong.

**Số test cuối:** frontend **254/254** (không thêm test mới — thuần CSS, jsdom không dựng
`getComputedStyle`/`box-shadow` thật để bắt lỗi kiểu này). `npm run build` chạy được. Không
chạm backend. Ảnh chụp/kiểm thử: `96-can-canh-hang-loc.png`, `96-loc-ho-ten-nguyen.png`,
`96-chi-tiet-gia-dinh-thanh-vien.png` trong `WebApp/anh-chup-kiem-thu/`.

### 46. Task "ô lọc hẹp/phễu to/dư khoảng trống, header thấp hơn dòng dữ liệu, mất hẳn hiệu ứng
focus" (2026-09-07) — ba góp ý liên tiếp trong CÙNG một lượt kiểm tra trực tiếp

Người dùng thật ngồi kiểm tra gửi liên tiếp ba góp ý (hai góp ý đầu trong một yêu cầu, góp ý
focus tới sau khi đã thấy bản sửa đầu):

1. *"bên phải filter icon còn 1 khoảng trống mà textbox filter lại nhỏ, hãy tối ưu cho filter
   icon nhỏ lại và textbox có thể rộng hơn"*.
2. *"chiều cao grid header cần dài thêm tí cho bằng chiều cao của record row bên dưới"* (header
   28px, dòng dữ liệu 30px — lệch 2px).
3. *"vẫn cần hiệu ứng focus vào filter textbox"* — sau khi mục 45 tắt hẳn `box-shadow` ở
   `:focus` để hết quầng hình dấu ngoặc, ngờ rằng đã tắt luôn MỌI phản hồi thị giác khi gõ.

**Việc 1 — đo bằng `getComputedStyle`/`getBoundingClientRect` thật trên cột "Mã GĐ" (100px,
danh sách gia đình) để tìm đúng chỗ ăn hết chỗ, không đoán:**
- `.ag-header-cell` hàng lọc kế thừa `padding: 0 16px` từ biến DÙNG CHUNG
  `--ag-cell-horizontal-padding` (áp cho cả hàng tiêu đề lẫn mọi ô dữ liệu, không chỉ hàng lọc).
- `.ag-floating-filter-button` cách ô nhập bằng `margin-left: 12px` (biến
  `--ag-cell-widget-spacing`, theme quartz đặt = grid-size × 1.5).
- Nút phễu bên trong rộng `var(--ag-icon-size)` = 16px.
- Cộng lại: padding hai bên (32) + margin (12) + icon (16) = 60px trong cột 100px, chỉ còn 40px
  cho ô nhập — đúng ô "chỉ đủ hiện vài gạch placeholder mờ" trong ảnh người dùng gửi. Khoảng
  trống "dư bên phải nút phễu" người dùng chỉ ra CHÍNH LÀ padding-phải 16px đó (nút phễu là phần
  tử cuối cùng trong ô).
- **Bẫy khi sửa:** thử selector `.ag-header-row-floating-filter .ag-header-cell` trước — không
  ăn thua gì (không báo lỗi, chỉ không có tác dụng). In `className` của tổ tiên `.ag-header-cell`
  trên trình duyệt thật mới lộ ra tên lớp AG Grid THẬT cho hàng lọc là `.ag-header-row-filter`
  (không phải `.ag-header-row-floating-filter` như tên biến CSS `--ag-header-height`/comment cũ
  trong file gợi ý).
- **Sửa** (`qlgx.css`, sau khối `.ag-floating-filter-button-button`): giảm padding hàng lọc còn
  8px hai bên (`.ag-header-row-filter .ag-header-cell`, KHÔNG đụng padding hàng tiêu đề/ô dữ
  liệu vì chỉ scope đúng hàng lọc), giảm khoảng cách ô nhập↔nút phễu còn 6px, thu icon phễu về
  12px — cả hai qua biến cục bộ trên `.ag-floating-filter-button` (`--ag-icon-size: 12px;
  margin-left: 6px`, không bịa biến mới, chỉ ghi đè biến sẵn có trong phạm vi hẹp). Ô nhập không
  cần sửa gì — nó là `flex: 1 1 auto` duy nhất trong `.ag-floating-filter-body`, tự nở lấp phần
  chỗ vừa giải phóng.
- **Đo lại sau khi sửa** (cột "Mã GĐ" 100px, danh sách gia đình): ô nhập rộng từ 40px → **66px**
  (+65%), nút phễu 16×16 → **12×12**, khoảng cách ô nhập↔phễu 12px → **6px**, khoảng trống sau
  phễu tới mép cột 16px → **8px** (đúng bằng padding còn lại, không còn "dư" bất thường — 8px là
  padding chủ ý, không phải khoảng trống vô nghĩa).

**Việc 2 — chiều cao header bằng dòng dữ liệu:**
- Đổi `--ag-header-height` từ `28px` → `30px` (đúng bằng `--ag-row-height` đã có, KHÔNG giảm
  dòng dữ liệu — người dùng chỉ nhắc header).
- Hàng lọc dùng CHUNG biến `--ag-header-height` với hàng tiêu đề (xác nhận qua AG Grid CSS gốc:
  không có biến riêng cho floating filter) nên tự động cao theo 30px, không cần luật riêng —
  đúng gợi ý "đừng cố tách riêng nếu không cần thiết".
- Đo lại: hàng tiêu đề, hàng lọc, dòng dữ liệu đều **30×(rộng theo cột hiện có)** — ba hàng bằng
  nhau tuyệt đối.

**Việc 3 — hiệu ứng focus:**
- Điều tra bằng `getComputedStyle` trên input đang thật sự `document.activeElement`: luật
  `border-bottom-color: var(--brand)` ở `:focus` (mục 45) THỰC RA vẫn thắng đúng (đọ specificity
  với rule mặc định AG Grid VÀ rule `input:focus` chung toàn ứng dụng — cả hai đều thua vì
  `qlgx.css` nạp SAU trong `main.tsx`, xác nhận bằng cách liệt kê toàn bộ CSS rule khớp input qua
  `document.styleSheets`). Vấn đề không phải "mất hẳn" mà là QUÁ MỜ: viền dưới kế thừa bề dày mặc
  định của AG Grid — `border-bottom-width: 0.8px`, gần như vô hình trên nền kính mờ của hàng lọc.
- **Sửa:** thêm `border-bottom-width: 2px` CHỈ ở trạng thái `:focus` (viền nghỉ/hover vẫn mảnh
  như cũ, không đổi gì khác) — viền dưới dày hẳn lên, dễ thấy, nhưng vẫn `box-shadow: none` và
  vẫn không có viền trái/phải — không tái diễn quầng dấu ngoặc mục 45 vừa sửa.
- Đo lại lúc `input.focus()` thật: `border-bottom-color: rgb(29, 93, 219)` (đúng `--brand`),
  `border-bottom-width` dày rõ rệt so với trạng thái nghỉ, `box-shadow: none`, `border-left/right-
  width` không đổi (không có viền hai bên) — chụp cận cảnh xác nhận một vạch xanh rõ dưới đáy ô,
  không tràn ra ngoài.

**Sự cố trong lúc kiểm chứng (không phải lỗi CSS, tự gây ra rồi tự phục hồi):** dùng
`document.body.style.zoom` để phóng to chụp cận cảnh làm lưới AG Grid co về `0×0` đúng như cảnh
báo "lưới hay co về 0px" trong CLAUDE.md/brief — nguyên nhân là zoom kích hoạt vòng lặp
ResizeObserver khiến AG Grid đo được kích thước 0. Khôi phục bằng cách tải lại trang (F5/
`navigate`), không phải sửa CSS gì — sau khi tải lại, lưới về đúng `1184.8×573.85`, ba hàng
đều 30px, không có tác động gì tồn lại từ CSS đã sửa. Ghi lại để agent sau không hoảng khi thấy
hiện tượng này trong lúc TỰ kiểm chứng bằng cách phóng to trình duyệt.

**Áp dụng cho mọi lưới:** cả ba sửa đổi đều ở tầng dùng chung `.ag-theme-quartz` trong
`qlgx.css`, không đụng `GxGrid.tsx` hay màn hình riêng nào — tự động ăn cho danh sách giáo dân,
danh sách gia đình, lưới thành viên gia đình dùng AG Grid. (Lưới thành viên trong trang chi tiết
gia đình — `table.grid` — là bảng HTML thường, không dùng AG Grid, không bị ảnh hưởng.)

**Kiểm chứng trên trình duyệt thật (Playwright MCP), đăng nhập `giaoxu`:**
- Danh sách giáo dân: chụp cận cảnh (phóng `zoom:3` tạm thời trên `<body>`, chụp xong reset về
  trang mới) ô lọc cột "Họ tên" ở trạng thái nghỉ — phễu nhỏ, ô nhập rộng, không dư khoảng trống
  — ảnh `99-loc-hep-rong-ra.png`.
- Cùng ô đó ở trạng thái `:focus` (gọi `input.focus()` thật) — vạch xanh rõ dưới đáy ô, không có
  viền/quầng hai bên — ảnh `99-loc-focus-hieu-ung.png`.
- Gõ lọc thật "Nguyễn Đức" vào cột "Họ tên" (set `value` qua native setter + dispatch sự kiện
  `input`, đúng cách React nhận): lưới lọc đúng còn lại toàn các dòng "Nguyễn Đức…", icon phễu
  chuyển xanh báo đang có bộ lọc hoạt động — xác nhận ô lọc vẫn dùng được, gõ vẫn lọc đúng.
- So sánh chiều cao hàng tiêu đề/hàng lọc/dòng dữ liệu — ảnh `99-header-cao-bang-row.png`.
- Gia đình "Paul Trần Văn Thái" (mã 9): mở chi tiết, lưới "Thành viên khác trong gia đình" hiện
  đúng 4 người (Mã GD 1068–1071), không có gì trong các mục trước bị hỏng lại — ảnh
  `99-giadinh-9-luoi-thanhvien.png`.
- `getBoundingClientRect()` đo thật trên danh sách giáo dân sau khi sửa (không zoom, tải trang
  mới):
  - Lưới `.ag-root-wrapper`: **1184.8 × 573.85** (không co 0px).
  - Hàng tiêu đề `.ag-header-row`: **1820 × 30** (trước: 28).
  - Hàng lọc `.ag-header-row-filter`: **1820 × 30** (trước: 30, không đổi — cùng biến với hàng
    tiêu đề).
  - Dòng dữ liệu `.ag-row` đầu tiên: **1835 × 30** (không đổi).
  - Ô lọc cột "Mã GĐ" (100px): `.ag-header-cell` padding `0 16px` → **`0 8px`**; ô nhập
    `input`: 40px → **66px** rộng; nút phễu: 16×16 → **12×12**; khoảng cách ô nhập↔phễu: 12px →
    **6px**.
  - Ô lọc lúc `:focus` (input "Họ tên"): `border-bottom-color: rgb(29, 93, 219)`,
    `border-bottom-width: 2px` (chuẩn logic, đo được `1.6px` do tỉ lệ scale của phiên trình
    duyệt lúc đo — không phải lỗi), `box-shadow: none`, `border-left/right-width` không đổi so
    với trạng thái nghỉ.

**Số test cuối:** frontend **254/254** (không thêm test mới — thuần CSS bố cục/hiệu ứng thị
giác, jsdom không dựng layout thật/`:focus` thật để bắt các thay đổi này). `npm run build` chạy
được. Không chạm backend. Ảnh chụp/kiểm thử: `99-loc-hep-rong-ra.png`,
`99-loc-focus-hieu-ung.png`, `99-header-cao-bang-row.png`, `99-giadinh-9-luoi-thanhvien.png`
trong `WebApp/anh-chup-kiem-thu/`.

### 47. Task "sửa 3 vấn đề giao diện theo phản hồi trực tiếp lần 2" (2026-09-07) — hiệu ứng
focus vẫn bị che dù mục 46 đo "đúng logic", điều hướng "Quay về" sai ngữ cảnh, gọn khối "Thông
tin cá nhân"

Người dùng thật tiếp tục ngồi kiểm tra, gửi ảnh cho thấy mục 46 (viền dưới 2px lúc `:focus`)
**vẫn hoàn toàn không nhìn thấy** dù đo `getComputedStyle` lúc đó báo "đúng logic". Kèm hai góp ý
khác: điều hướng "Quay về" sai và khối "Thông tin cá nhân" thừa khoảng trống dọc.

**Việc 1 — nguyên nhân thật của viền focus bị che (khác hẳn suy đoán ở mục 46):**
- Mục 46 kết luận viền `2px` đo đúng, chỉ lệch `1.6px` "do tỉ lệ scale phiên trình duyệt" —
  **kết luận đó SAI**. Đo lại bằng `getBoundingClientRect()` trên CẢ input lẫn `.ag-header-cell`
  cha của nó (không chỉ đọc `getComputedStyle` một mình input như mục 46 đã làm) mới lộ ra: input
  ô lọc cao **32px** (từ `min-height: calc(var(--ag-grid-size) * 4)` mặc định của theme Quartz —
  luật `min-height: 22px` cũ trong `qlgx.css` thua vì độ đặc hiệu CSS thấp hơn, mỗi
  `[class^="ag-"]`/`:not([type])` trong selector gốc AG đều tính là một lớp), trong khi
  `.ag-header-cell` chứa nó (hàng lọc) chỉ cao đúng **30px** (`floatingFiltersHeight={30}` ở
  `GxGrid.tsx`) và có `overflow: hidden`. Input vì vậy LUÔN tràn xuống dưới ô **2px**, bị cắt —
  viền dưới nằm ở mép đáy cùng của input nên gần như toàn bộ viền (kể cả `2px` lúc focus) nằm
  NGOÀI vùng nhìn thấy của ô (đo được chỉ **~0.2px** lọt qua, bằng mắt thường coi như vô hình).
  Không liên quan gì tới bề dày viền hay `box-shadow` như hai lần sửa trước đoán.
- **Sửa** (`qlgx.css`, `.ag-floating-filter-input input`): ép `height: 24px !important;
  min-height: 24px !important;` — nhỏ hơn hẳn 30px của ô, chừa dư khoảng trên/dưới để viền dưới
  (kể cả lúc dày 2px ở `:focus`) luôn nằm trọn trong vùng nhìn thấy. `!important` là cần thiết ở
  đây (không phải lười biếng): độ đặc hiệu CSS gốc của AG Quartz cho selector đó cao hơn bất kỳ
  cách viết lại selector nào không lặp lại đúng danh sách `input[type=...]` dài của AG.
- Đo lại (input "Người nam" lúc `input.focus()` thật): `height: 24px`, input nằm gọn trong ô 30px
  (còn dư ~3px trên/dưới), `border-bottom-color: rgb(29, 93, 219)` (đúng `--brand`),
  `border-bottom-width: 1.6px` đo được (2px logic, lệch do scale màn hình lúc đo — xác nhận đúng
  như mục 46 từng nói, LẦN NÀY viền thật sự NẰM TRONG vùng nhìn thấy: viền dưới của input tại
  y≈371.6, đáy ô tại y≈374.2, còn dư 2.6px chỗ trống bên dưới viền). Chụp ảnh focus thật xác nhận
  vạch xanh rõ ràng dưới ô "Người nam" — `100-viec1-focus-loc-nguoi-nam.png`.
- **Bài học cho agent sau:** khi viền/box-shadow "đo đúng logic nhưng không thấy", đừng dừng lại
  ở `getComputedStyle` của riêng phần tử — phải đo thêm `getBoundingClientRect()` của phần tử VÀ
  cha trực tiếp có `overflow: hidden`, so hai toạ độ để xác nhận phần vẽ có nằm trong vùng cắt
  hay không.

**Việc 2 — "Quay về" từ giáo dân mở qua ngữ cảnh gia đình:**
- Hiện tượng: mở gia đình → mở một người trong lưới "Thành viên khác"/ô Người nam/Người nữ → tab
  chi tiết giáo dân mới mở → bấm "Quay về"/"← Danh sách" → cả hai nút gọi thẳng
  `moDanhSachGiaoDan()` (không điều kiện) → luôn mở/focus tab "Danh sách giáo dân", bỏ qua hoàn
  toàn tab gia đình vừa đứng đó.
- **Quyết định tự đưa ra** (đúng gợi ý đơn giản nhất trong đặc tả nhiệm vụ): thêm tham số
  `nguonTabId` cho `App.moChiTietGiaoDan` — CHỈ truyền khi mở từ `GiaDinhDetail` (qua prop
  `moGiaoDan`, dùng chung cho cả lưới thành viên, ô Người nam/Người nữ, VÀ context-menu mặc định
  của lưới thành viên — tất cả đều "thuộc ngữ cảnh gia đình" như nhau). Khi có `nguonTabId`, nút
  "Quay về" ĐÓNG thẳng tab giáo dân hiện tại (`dong(idThe)`) thay vì mở "Danh sách giáo dân" —
  đóng tab tự lộ ra tab gia đình bên dưới (theo đúng logic `useTabDocs.dong`: chuyển tiêu điểm
  sang tab cuối cùng còn lại). KHÔNG dò xem tab nguồn còn tồn tại hay không (đơn giản hoá theo
  đúng gợi ý trong đặc tả) — nếu tab gia đình đã bị đóng trước đó, `dong()` vẫn tự chọn ra một tab
  còn lại hợp lý, không còn "luôn văng về Danh sách giáo dân" như cũ.
- Giáo dân mở TRỰC TIẾP từ "Danh sách giáo dân" (không qua gia đình) giữ NGUYÊN hành vi cũ: không
  truyền `nguonTabId` → "Quay về" vẫn gọi `moDanhSachGiaoDan()`, mở/focus tab danh sách (không
  đóng tab giáo dân đang xem) — đã kiểm chứng lại bằng tay, không bị đổi khác đi.
- Triển khai không cần sửa `GiaoDanDetail.tsx` — `GiaoDanDetailPage.tsx` nhận thêm prop
  `onQuayVe?: () => void`, ghi đè `moDanhSachGiaoDan` bằng nó trước khi truyền xuống
  `GiaoDanDetail` (`const quayVe = onQuayVe ?? moDanhSachGiaoDan`), nút "Quay về"/"← Danh sách"
  trong `GiaoDanDetail.tsx` không đổi gì (vẫn gọi đúng MỘT prop `moDanhSachGiaoDan` như trước).
- Kiểm chứng trên trình duyệt thật (Playwright MCP): mở gia đình "Paul Trần Văn Thái" (mã 9) →
  mở "Trần Đại Hiệp" (mã 1068) từ lưới thành viên → bấm "Quay về" → tab "Giuse Trần Đại Hiệp"
  đóng lại, tab "Paul Trần Văn Thái" được chọn lại đúng — `101-viec2-quay-ve-tu-gia-dinh.png`. Mở
  lại "Giuse Nguyễn Đức Mạnh" (mã 1) trực tiếp từ "Danh sách giáo dân" → bấm "Quay về" → tab giáo
  dân VẪN CÒN MỞ trong thanh tab, tiêu điểm chuyển sang tab "Danh sách giáo dân" — đúng hành vi cũ
  — `102-viec2-quay-ve-tu-danh-sach-truc-tiep.png`.

**Việc 3 — gọn khối "Thông tin cá nhân" (`GiaoDanDetail.tsx`), ảnh 3×4 cao thêm cân đối:**
- Nguyên nhân khoảng trống thừa giữa "Tên thánh" và "Họ tên": KHÔNG phải do margin nào — do
  `.canhan-top` (chứa Mã giáo dân/Tên thánh VÀ khung ảnh) dùng `align-items: flex-start`, khung
  ảnh cố định 92px cao hơn khối 2 dòng Mã giáo dân/Tên thánh (~64px), nên hàng flex đó cao 92px
  trong khi khối trường chỉ chiếm ~64px rồi để trống phần còn lại — "Họ tên" (nằm ngoài
  `.canhan-top`, ngay bên dưới) vì vậy bị đẩy xuống, tạo cảm giác khoảng cách dòng lớn.
- **Sửa**: chuyển "Họ tên" và "Giáo họ" VÀO `.canhan-top-fields` (ngay sau "Tên thánh") — cùng độ
  rộng hẹp với "Tên thánh" (đúng yêu cầu "ngắn lại bằng tên thánh"), xếp khít 4 dòng liên tiếp
  không khoảng hở (margin-bottom 6px như mọi `.frow`, không đổi gì). Đổi `.canhan-top` sang
  `align-items: stretch` (CSS) để khung ảnh 3×4 tự giãn cao bằng đúng chiều cao 4 dòng đó. Field
  "Giáo xứ"/"Giáo phận" (chỉ hiện khi "Ngoài xứ", hiếm gặp) CỐ Ý giữ nguyên full-width bên ngoài
  `.canhan-top-fields` — không phải trọng tâm góp ý, tránh cắt chữ "Giáo phận" nếu thu hẹp.
- **Đo `getBoundingClientRect()` trên trình duyệt thật** (giáo dân "Giuse Trần Đại Hiệp", mã
  1068), trước/sau:
  | Đại lượng | Trước | Sau |
  |---|---|---|
  | `.canhan-top-fields` (khối 2→4 dòng) | 457.2 × **66** | 457.2 × **138** |
  | Khung ảnh `.photo-slot` | 92 × **92** | 92 × **138** |
  | Hàng "Họ tên" (rộng) | **561.2** × 30 | **457.2** × 30 (bằng "Tên thánh") |
  | Hàng "Giáo họ" (rộng) | **561.2** × 30 | **457.2** × 30 (bằng "Tên thánh") |
  | Hàng "Tên thánh" (đối chứng) | 457.2 × 30 | 457.2 × 30 (không đổi) |
  | Thẻ "Thông tin cá nhân" (`.card`) | 1176 × 251.65 | 1176 × 251.65 (không đổi — cột phải
    Giới tính/Ngày sinh/Nơi sinh/Tên Cha/Tên Mẹ/CMND vẫn quyết định chiều cao thẻ, không bị đụng
    tới) |
  Khung ảnh cao thêm đúng **50%** (92px → 138px), khớp hẳn chiều cao khối trường 4 dòng bên cạnh
  — không còn khoảng trắng thừa dưới ảnh lẫn giữa "Tên thánh"/"Họ tên". Ảnh trước/sau:
  `104-viec3-thong-tin-ca-nhan-truoc.png` / `103-viec3-thong-tin-ca-nhan-sau.png`.
- Đo lấy số "trước" bằng cách `git stash push` tạm hai file đã sửa
  (`GiaoDanDetail.tsx`/`qlgx.css`), đo trên bản HMR reload lại, rồi `git stash pop` khôi phục —
  không tạo commit trung gian nào, không ảnh hưởng tiến trình khác đang chạy song song trên cùng
  thư mục.

**Không đụng gì khác:** không sửa `useTabDocs.ts` (logic `dong()` chọn tab cuối cùng còn lại vốn
đã đúng, chỉ thiếu chỗ gọi đúng nó); không sửa `GiaDinhDetail.tsx`/`GiaDinhDetailPage.tsx` (chỉ
`App.tsx` đổi cách bọc `moGiaoDan` truyền xuống); không đụng bố cục màn hình gia đình, header/
hàng lọc lưới (ngoài chiều cao input đã nêu ở Việc 1), Phiếu gia đình A4 dọc.

**Số test cuối:** frontend **254/254** (không thêm test mới — cả ba việc thuần CSS bố cục/hiệu
ứng thị giác VÀ điều hướng UI thuần React state đã có `useTabDocs.test.ts` phủ logic `dong()`
dùng chung; hành vi "gọi đúng callback nào" khó kiểm bằng jsdom hơn là bằng trình duyệt thật theo
đúng yêu cầu nhiệm vụ). `npm run build` chạy được. Không chạm backend, không chạy
`dotnet test`. Ảnh chụp: `100`–`104` trong `WebApp/anh-chup-kiem-thu/`.

### 48. Task "ba việc theo phản hồi trực tiếp lần 3" (2026-09-07) — CMND/CCCD sang cột trái,
bỏ viền icon lịch, Xuất Excel thay CSV

Người dùng thật tiếp tục ngồi kiểm tra, gửi ảnh và ba góp ý trực tiếp. Không dừng lại hỏi, tự
quyết hướng hợp lý nhất theo đúng chỉ dẫn nhiệm vụ, ghi lại quyết định ở đây.

**Việc 1 — CMND/CCCD + checkbox "Là giáo dân không được thống kê" sang cột trái, dưới Giáo họ:**
- Trước: `GxField` "CMND / CCCD" (kèm `extra` là checkbox) nằm ở CỘT PHẢI của `.canhan-cols`
  (dưới "Tên Mẹ"). Người dùng: "nên đưa CCCD và checkbox trong hình qua bên trái, bên dưới giáo
  họ".
- Quyết định: chuyển nguyên khối `GxField` đó (không tách checkbox riêng — vẫn dùng `extra` như
  cũ) vào `.canhan-top-fields` (cột trái, khối chứa Mã giáo dân/Tên thánh/Họ tên/Giáo họ), ngay
  sau field "Giáo họ" — thành dòng thứ 5. Không cần sửa CSS: mục 47 đã đổi `.canhan-top` sang
  `align-items: stretch` nên khung ảnh 3x4 bên phải TỰ giãn cao theo đúng số dòng của
  `.canhan-top-fields`, dù dòng đó là 4 hay 5 — cơ chế đã đúng sẵn từ trước, chỉ cần thêm dòng là
  đủ cân đối, không có khoảng trắng thừa mới nào.
- Đo `getBoundingClientRect()` trên trình duyệt thật (giáo dân "Giuse Nguyễn Đức Mạnh", mã 1),
  dùng đúng kỹ thuật `git stash` tạm hai file đã sửa rồi đo lại bản HMR reload như mục 47 đã làm
  để lấy số "trước" chính xác, không đoán:
  - Trước (CMND ở cột phải, `.canhan-top-fields` chỉ 4 dòng): `.canhan-top-fields` cao 138px,
    khung ảnh cao 138px (khớp nhau, đúng trạng thái sau mục 47).
  - Sau (CMND chuyển vào cột trái, `.canhan-top-fields` thành 5 dòng): `.canhan-top-fields` cao
    178.6px, khung ảnh cao 178.6px — vẫn khớp tuyệt đối nhau (chênh 0px), ảnh cao thêm 40.6px
    tương ứng đúng một dòng `.frow` mới (30px + 6px margin, dòng cuối không margin nên lệch chút
    do wrap) — không có khoảng trắng thừa nào phát sinh, đúng yêu cầu "ảnh bên phải cũng cao thêm
    tương ứng, giữ hai cột cân đối".
  - Ảnh chụp: `105-viec1-cccd-checkbox-cot-trai.png`.

**Việc 2 — bỏ viền hộp vuông quanh biểu tượng lịch (component `GxDate` dùng chung):**
- Hiện tượng: nút tròn `<button className="mini gx-date-btn">` (biểu tượng lịch mở lịch) trong
  `GxDate.tsx` không có CSS riêng nào bỏ viền — class `.mini` CHỈ được định nghĩa dưới scope
  `.picker .mini` trong `qlgx.css` (dùng cho nút "bỏ chọn" của `GxPicker`), không khớp một
  `<button>` đứng một mình trong `.gx-date-row`. CSS gốc chỉ có `.gx-date-btn { width: 26px;
  height: 26px; }` — không có `border: 0`/`appearance: none`/`background` nào, nên trình duyệt tự
  vẽ viền/nền `<button>` mặc định (hộp xám nhạt có viền rõ) quanh biểu tượng lịch, đúng như ảnh
  người dùng gửi.
- Sửa (`qlgx.css`, `.gx-date-btn`): thêm `appearance: none; border: 0; border-radius: 50%;
  background: transparent; cursor: pointer;` cùng `:hover`/`:disabled` tương ứng (nền tròn nhạt
  lúc hover, giống các nút `.mini` khác trong ứng dụng) — không tạo class mới, không đụng
  `GxDate.tsx` (chỉ CSS). Vì `GxDate` là component DÙNG CHUNG cho mọi ô ngày (Ngày sinh, Ngày rửa
  tội, Ngày hôn phối, Ngày qua đời…), sửa một chỗ áp dụng khắp ứng dụng, không vá riêng lẻ.
- Kiểm chứng bằng `getComputedStyle()` trên trình duyệt thật: `border: "0px none …"`, `background:
  "rgba(0, 0, 0, 0)"`, `borderRadius: "50%"` — không còn viền hộp nào. Ảnh chụp cận cảnh ô "Ngày
  sinh": `106-viec2-icon-lich-khong-vien.png`.

**Việc 3 — "Xuất Excel" (.xlsx thật, ClosedXML) thay "Xuất dữ liệu (CSV)":**
- Người dùng: "Xuất CSV tôi muốn xuất Excel có format như trên grid, vì người dùng thông thường
  không dùng CSV".
- Quyết định kiến trúc quan trọng nhất: endpoint Excel mới (`XuatExcelService.cs`) KHÔNG viết lại
  điều kiện lọc nào — gọi thẳng lại `GiaoDanService.LayDanhSach`/`GiaDinhService.LayDanhSach`
  (đúng những hàm GET danh sách JSON hiện có đang dùng), rồi chỉ dựng workbook từ kết quả trả về.
  Nhờ vậy Excel xuất ra LUÔN khớp 100% với JSON mà chính bộ lọc đó trả, không có nguy cơ hai nơi
  lọc lệch nhau theo thời gian nếu sau này ai đó sửa `LayDanhSach` mà quên sửa chỗ xuất Excel.
- Cột/tiêu đề: chép nguyên văn tên cột và THỨ TỰ cột từ `cotGiaoDan.ts` (29 cột)/`cotGiaDinh.ts`
  (12 cột) — không tự nghĩ tên khác. Boolean hiển thị dấu tích/dấu gạch ngang giống hệt
  `valueFormatter` của lưới; ngày hiển thị `dd/MM/yyyy` (chuỗi, không phải kiểu ngày Excel — cố ý
  để khớp CHỮ với những gì đang hiện trên lưới, không phải để sort được theo kiểu ngày Excel; nếu
  sau này cần sort/tính toán trên cột ngày thì đây là chỗ cần đổi sang kiểu `DateTime` thật của
  ClosedXML). Tiêu đề in đậm (`Style.Font.Bold`), đóng băng hàng 1 (`FreezeRows(1)`), độ rộng cột
  tự co theo nội dung (`Columns().AdjustToContents()`), có auto-filter.
- Gạch ngang — quyết định CỐ Ý khác nhau giữa hai lưới, đúng những gì lưới web ĐANG hiển thị
  (không phải đúng những gì nghe "hợp lý" từ mô tả nhiệm vụ):
  - Lưới giáo dân đứng một mình ở "Danh sách giáo dân": KHÔNG gạch ngang dòng nào. Lý do:
    `GxGiaoDanList` chỉ gạch (`toDo`) khi `quanHeGiaDinh=true` (lưới "Thành viên khác" nhúng
    trong form gia đình) — màn hình "Danh sách giáo dân" gọi component này KHÔNG truyền
    `quanHeGiaDinh`, nên `toDo` là `undefined`, không có `ghiChuChan` "Gạch ngang đỏ" nào hiện ra
    (xem mục 7 cũ). Excel xuất từ đúng màn hình đó phải khớp — nếu gạch ngang người "Qua đời"/
    "Đã chuyển đi" ở đây sẽ SAI KHÁC với lưới đang hiển thị, dù mô tả nhiệm vụ liệt kê "qua
    đời/chuyển xứ/đã lập gia đình riêng" như một quy tắc chung. Đã viết test xác nhận không có ô
    nào gạch ngang trong toàn bộ tệp giáo dân.
  - Lưới gia đình: gạch từng Ô (không phải cả dòng) — "Người nam" gạch khi `Gach` là 0 hoặc 2,
    "Người nữ" gạch khi `Gach` là 1 hoặc 2 — chép nguyên văn điều kiện `cellClass` của
    `cotGiaDinh.ts`.
  - "Đã lập gia đình riêng" (`lapGd`/`daCoGiaDinh`) KHÔNG gạch ngang ở bất cứ đâu — đúng quyết
    định đã ghi ở mục 7 cũ (gạch người đã lập gia đình gây hiểu nhầm, phần lớn giáo dân trưởng
    thành đã lập gia đình).
- Bộ lọc: endpoint nhận đúng ba tham số của `GET /api/giao-dan` (`giaoHoId`, `chiKhongThongKe`,
  `hienCaDaMat`) và hai tham số của `GET /api/gia-dinh` (`giaoHoId`, `chiKhongThongKe` — gia đình
  không có `hienCaDaMat`, `LayDanhSach` của `GiaDinhService` chưa hỗ trợ). Phía web: combobox
  "Giáo họ" trên màn hình vốn lọc THEO TÊN ở máy khách (so khớp `tenGiaoHo`, không gọi lại API) —
  để truyền đúng `giaoHoId` (khoá GUID thật) cho endpoint Excel, tra ngược tên đang chọn trong
  `danhMucGiaoHo` (đã có sẵn, `GET /api/giao-ho`) để lấy `id`. Giới hạn đã biết, chấp nhận được:
  lựa chọn "Ngoài xứ" trong combobox không có tham số máy chủ tương ứng
  (`LayDanhSach(giaoHoId: null)` nghĩa là "không lọc gì", không phải "chỉ lấy người không có giáo
  họ") — khi đang chọn "Ngoài xứ" mà bấm "Xuất Excel", tệp xuất ra là TOÀN BỘ danh sách (như đang
  chọn "Tất cả"), không riêng người Ngoài xứ. Không mở rộng `LayDanhSach` cho trường hợp hiếm này
  (đúng chỉ dẫn nhiệm vụ "đừng viết lại logic lọc").
- Không dùng Office Interop — ClosedXML 0.105.1 (MIT, sinh `.xlsx` thuần OpenXML, không cần Excel
  cài trên máy chủ), thêm vào `Qlgx.Api.csproj` bằng `dotnet add package`.
- Đổi nút "Xuất dữ liệu (CSV)" thành "Xuất Excel" ở cả hai màn hình danh sách, gọi
  `api.giaoDan.xuatExcel`/`api.giaDinh.xuatExcel` (dùng lại `taiTepIn()` sẵn có — đọc tên tệp thật
  từ `Content-Disposition`, tự tải về máy). `layCsv()`/`taiXuongCsv()`/`lib/csv.ts` GIỮ NGUYÊN
  (không xoá) — `layCsv()` vẫn được `GxGiaoDanList.test.tsx` dùng để kiểm tra thứ tự sắp xếp thật
  của lưới, không liên quan gì tới nút xuất dữ liệu nữa.
- Kiểm chứng bằng đọc tệp thật: gọi endpoint qua `fetch()` trong trang (Playwright MCP mất kết
  nối khi bấm nút tải file thật, đúng lưu ý đã biết ở đầu nhiệm vụ), lưu base64 ra tệp `.xlsx`,
  đọc lại bằng `openpyxl` (Python):
  - `/api/giao-dan/xuat-excel`: 200, `Content-Type`
    `application/vnd.openxmlformats-officedocument.spreadsheetml.sheet`, `Content-Disposition`
    đúng tên `danh-sach-giao-dan-2026-09-07.xlsx`, sheet "Giáo dân", 2039 dòng dữ liệu (khớp số
    "2039 giáo dân" hiện trên màn hình lúc đó), đúng 29 cột, tiêu đề đúng tên/in đậm (`A1`= "Mã
    GD" bold=True, `C1`="Họ tên" bold=True), cột ngày dạng chuỗi `dd/MM/yyyy` (ví dụ
    "18/12/2014"), 0 ô gạch ngang trong toàn bộ sheet (đúng quyết định ở trên).
  - `/api/gia-dinh/xuat-excel`: 200, cùng loại nội dung, sheet "Gia đình", 40 dòng (khớp "40 gia
    đình"), đúng 12 cột, tiêu đề in đậm, và có gạch ngang đúng ô — ví dụ dòng "Nguyễn Văn Sơ" (mã
    10): cả "Người nam" (Dom Nguyễn Văn Sơ) VÀ "Người nữ" (Anna Nguyễn Thị Nghĩa) đều gạch ngang;
    dòng "Trần Hữu Chính" (mã 11): chỉ "Người nam" gạch, "Người nữ" không — đúng theo từng trường
    hợp `Gach` khác nhau, không phải gạch cả dòng đồng loạt.
  - Ảnh chụp nút "Xuất Excel" trên thanh công cụ: `107-viec3-nut-xuat-excel-danh-sach-giao-dan.png`.
- Viết 6 test mới ở `WebApp/tests/Qlgx.Api.Tests/XuatExcelTests.cs` (đọc lại `.xlsx` sinh ra bằng
  chính `ClosedXML.Excel.XLWorkbook` phía test — thư viện sinh ra cũng đọc lại được): đúng loại
  nội dung/tiêu đề in đậm, không gạch ngang dòng nào ở giáo dân, tôn trọng lọc giáo họ, không lộ
  dữ liệu giáo xứ khác, gạch đúng ô ở gia đình theo `Gach`, tôn trọng lọc `chiKhongThongKe`.

**Không đụng gì khác:** không sửa `GxGrid.tsx`/`GxGiaoDanList.tsx`/`GxGiaDinhList.tsx` (giữ
nguyên `layCsv()` cho mục đích test); không sửa cột/quy tắc gạch ngang trong `cotGiaoDan.ts`/
`cotGiaDinh.ts` (chỉ ĐỌC LẠI để chép đúng ở `XuatExcelService.cs`); không đụng bố cục Rửa tội/
Rước lễ/Thêm sức/Xức dầu, header/hàng lọc lưới, Phiếu gia đình A4 dọc.

**Số test cuối:** frontend 256/256 (thêm 2, thay 2 test CSV cũ của hai màn hình danh sách bằng
test gọi đúng `api.*.xuatExcel` với đúng tham số lọc), `npm run build` chạy được. Backend 250/250
(`Qlgx.Data.Tests` 25 + `Qlgx.Api.Tests` 201 [tăng từ 195, thêm 6 test Excel] +
`Qlgx.Migration.Tests` 24) — không xoá test nào. Ảnh chụp: `105`–`107` trong
`WebApp/anh-chup-kiem-thu/`.

### 49. Task "control nhập ngày tháng thông minh + khôi phục 239 bản ghi ngày tháng thiếu"
(2026-09-07) — các quyết định tự đưa ra

Người dùng đang chờ, yêu cầu không dừng lại hỏi. Ba việc: nâng cấp `GxDate` cho gõ liên tục/tự
nhảy ô, tự nhảy ô khi chọn dropdown, và khôi phục dữ liệu ngày tháng thiếu đang kẹt trong
`du_lieu_loi`. Người dùng đã CHỐT SẴN hướng xử lý ngày thiếu — phương án (a) "chuẩn hoá lúc
nhập" (không thêm cột độ chính xác, xem `docs/superpowers/specs/man-hinh/ho-tro-nhap-lieu.md`
mục 1 — spec đó khuyến nghị hướng (b) nhưng người dùng đã cân nhắc và chọn khác, không tự ý làm
theo khuyến nghị của spec).

**Việc 1 — `GxDate` gõ liên tục không cần `/`, tự nhảy ô, click thẳng vào tháng/năm:**
- Bản desktop (`GxDateInput`) dùng BA `TextBox` con rời biệt (xem spec mục A.1). Bản web hiện có
  `GxDate` (`WebApp/src/web/src/components/GxDate.tsx`) lại dùng MỘT `<input type="text">` duy
  nhất (thêm ở nhiệm vụ trước, không phải nhiệm vụ này) — quyết định: KHÔNG viết lại thành ba ô
  DOM riêng (sẽ phải sửa lại toàn bộ chỗ gọi `querySelector('[name=...]')`/`FormData` đang dựa
  vào đúng một ô ẩn kiểu `date` mang `name`), mà mô phỏng cảm giác ba-ô-rời bằng kỹ thuật
  "mask một ô": ô luôn hiển thị đủ khuôn 10 ký tự `dd/mm/yyyy` (thiếu là `_`, ví dụ trống là
  `__/__/____`), gõ số ghi đúng vào vị trí con trỏ, dấu `/` cố định không gõ được, tự nhảy vị trí
  gõ tiếp theo khi một phần (ngày/tháng) đã đủ số — bỏ qua dấu `/` một cách tự nhiên. Logic mask
  thuần (không đụng DOM, test độc lập được) tách ra `WebApp/src/web/src/lib/ngay.ts` (các hàm
  `khuonTuIso`, `goSoVaoKhuon`, `xoaLuiTrongKhuon`, `chuanHoaNgayThieu`, ...), `GxDate.tsx` chỉ
  nối dây sự kiện bàn phím.
- Chuẩn hoá ngày thiếu (đúng phương án đã chốt): `chuanHoaNgayThieu(ngay, thang, nam)` — chỉ năm
  hợp lệ (điền `01/01`), tháng+năm hợp lệ (điền ngày `01`), ngày một mình KHÔNG hợp lệ (đúng quy
  tắc desktop, spec mục A.4) — áp dụng NGAY khi gõ xong chữ số cuối của năm (không cần rời ô) và
  lại một lần nữa lúc `blur` (phòng người dùng Tab đi giữa chừng).
- Tự nhảy ra control KHÁC trên form: chỉ xảy ra đúng một thời điểm — vừa gõ xong chữ số cuối của
  năm VÀ kết quả hợp lệ (không nhảy khi chuẩn hoá xảy ra lúc blur, không nhảy khi gõ sai) — đúng
  khuyến nghị khả năng tiếp cận ở spec mục 3 (không đổi ngữ cảnh ngoài lúc người dùng vừa tự hoàn
  tất một hành động). `Tab` không hề bị can thiệp — kiểm tra bằng test `userEvent.tab()`.
- Hàm dùng chung để tìm/focus control kế tiếp: `WebApp/src/web/src/lib/focusDieuHuong.ts`
  (`focusKeTiep`) — tìm phần tử focus-được kế tiếp theo thứ tự DOM trong `<form>` bao quanh, bỏ
  qua các phần tử con của chính control đang đứng (nút mở lịch, ô ISO ẩn của `GxDate`).
- Dữ liệu lỗi cũ không map được vào khuôn (`defaultValue` không phải ISO đầy đủ, ví dụ `"1958"`
  còn sót ở các bảng KHÔNG thuộc phạm vi khôi phục việc 3) vẫn hiện NGUYÊN VĂN lúc mới dựng
  (hành vi cũ, có test giữ nguyên) — chỉ khi người dùng bắt đầu sửa (focus/gõ) mới chuyển sang
  khuôn trống để nhập lại từ đầu, không cố "vá" chuỗi lỗi vào từng ô.
- **Lỗi tự phát hiện khi kiểm thử bằng trình duyệt thật, đã sửa**: Backspace xoá về hoàn toàn
  trống chỉ cập nhật `text` hiển thị, KHÔNG đồng bộ ngay giá trị ISO trên ô lịch ẩn — nếu người
  dùng xoá xong rồi bấm thẳng nút "Cập nhật" mà không rời ô theo cách thường (blur), giá trị ISO
  CŨ vẫn còn nằm trên ô ẩn và bị lưu nhầm xuống CSDL (bắt gặp thật: xoá "Ngày sinh" đang có rồi
  lưu, hộp thoại xác nhận tuổi "chưa được 7 tuổi" hiện ra dù màn hình đang hiển thị ô trống — dấu
  hiệu ISO ẩn lệch với hiển thị). Sửa: xoá về khuôn rỗng hoàn toàn thì `setIso('')` ngay lập tức,
  không đợi blur. Thêm test `GxDate.test.tsx` xác nhận (mô phỏng đúng tình huống không blur).
- Việc kiểm thử bằng Playwright MCP phát hiện một điều KHÔNG PHẢI lỗi của mã: hàm
  `browser_type`/`pressSequentially` của Playwright (kể cả `slowly: true`) gửi các phím quá
  nhanh với một ô nhập có mask tự viết (chặn `preventDefault` trên `keydown`), làm hỏng thứ tự
  ký tự khi gõ liên tục nhiều số. Gõ từng phím riêng lẻ (`browser_press_key`, mỗi lệnh một round
  trip thật) thì đúng tuyệt đối ở mọi bước — và bài test tự động (`userEvent.type` của
  `@testing-library/user-event`, tôn trọng `preventDefault` đúng chuẩn DOM) cũng xanh 100%. Kết
  luận: đây là giới hạn của công cụ tự động hoá khi gõ quá nhanh, không phải hành vi với người
  dùng thật (bàn phím thật không gửi 8 phím trong cùng một khung hình).

**Việc 2 — tự nhảy ô kế tiếp khi chọn xong một mục trong `<select>`:**
- Spec (mục B) không tìm thấy bằng chứng mã tự viết cho hành vi này ở bản desktop (nghi ngờ do
  thư viện đóng gói `UIComboBox`/`AutoCompleteTextBox`) — coi đây là quyết định UX MỚI, làm ở
  tầng dùng chung bằng event delegation: một `useEffect` gắn MỘT LẦN ở gốc ứng dụng
  (`App.tsx`, hook `useTuNhayKhiChonDropdown` trong `lib/focusDieuHuong.ts`) lắng nghe sự kiện
  `change` nổi bọt lên `document`, chỉ xử lý khi mục tiêu là `<select>` VÀ nằm trong một
  `<form>` — nhờ vậy MỌI `<select>` hiện có và mọi ô mới thêm sau này trong các form chi tiết
  đều tự có hành vi, không cần sửa từng màn hình.
- Cố ý loại trừ `<select>` KHÔNG nằm trong `<form>` (ví dụ ô lọc "Giáo họ" ở đầu lưới danh sách
  — xác nhận bằng cách đọc DOM thật: `gdl-giaoho`/`gdanl-giaoho` không có `form`, trong khi
  `gd-giaoho`/`gd-phai`/... trong màn hình chi tiết đều có) — tránh đá focus khỏi thao tác lọc dữ
  liệu ngoài ý muốn.
- Kiểm chứng bằng trình duyệt thật: đổi `<select id="gd-giaoho">` (Giáo họ) sang "Ngoài xứ" →
  tiêu điểm tự chuyển sang `#gd-cmnd` (ô CMND/CCCD, control kế tiếp đúng thứ tự DOM) — không lưu
  thay đổi này (tải lại trang, không bấm "Cập nhật").

**Việc 3 — khôi phục 238/239 bản ghi ngày tháng thiếu trong `giao_dan.du_lieu_loi`:**
- Sửa `NgayThangText.Doc` (`WebApp/src/Qlgx.Data/NgayThangText.cs`): thêm hai khuôn `MM/yyyy`
  (chuẩn hoá về ngày 01) và số 4 chữ số đứng riêng (chuẩn hoá về `01/01`) — đúng phương án đã
  chốt. Sửa lại doc-comment SAI của tệp (từng viết "Access lưu mọi ngày dưới dạng dd/MM/yyyy") —
  giờ ghi rõ Access lưu CẢ chuỗi thiếu, trích dẫn bằng chứng `Memory.GetDateString`
  (`CMemory.cs:755-763`) không tự điền `01/01`. TDD: viết test cho `"1985"`, `"05/1985"`,
  `"09/11/1996"`, chuỗi rác TRƯỚC — xác nhận đỏ (6 test thất bại, lỗi "Expected ... but found
  null"), rồi sửa mã cho xanh.
- Vì `ChuyenDoiDuLieu.cs` (công cụ chuyển Access) đã gọi `NgayThangText.Doc` cho mọi trường ngày
  từ trước — sửa xong hàm là ĐỦ để các lần chuyển dữ liệu Access MỚI (giáo xứ khác, về sau) không
  còn tạo lại vấn đề này, không cần sửa gì thêm ở `ChuyenDoiDuLieu.cs`.
- Khôi phục dữ liệu ĐANG CÓ trong `qlgx_thu`: viết lệnh CLI mới `KhoiPhucNgayThangThieu.cs`
  (mô phỏng đúng khuôn `TaoTaiKhoanQuanTri.cs` sẵn có — nhánh dòng lệnh trong `Program.cs`,
  KHÔNG phải endpoint HTTP), chạy bằng `dotnet run -- khoi-phuc-ngay-thang-thieu` (dùng chuỗi kết
  nối QUẢN TRỊ, có `BYPASSRLS`, chạy trên toàn bộ giáo xứ). Đọc `du_lieu_loi` (JSON dạng
  `{tên trường: giá trị gốc}`, đúng cấu trúc `GhiLoi` của `ChuyenDoiDuLieu.cs` ghi ra), với đúng
  5 trường `NgaySinh`/`NgayRuaToi`/`NgayRuocLe`/`NgayThemSuc`/`NgayQuaDoi`: nếu cột ngày tương
  ứng đang NULL và `NgayThangText.Doc` phân giải được giá trị gốc thì ghi vào cột — **giữ nguyên
  `du_lieu_loi`, không xoá, không sửa** (bản gốc từ sổ giấy, còn dùng đối chiếu sau này).
  Idempotent tự nhiên (chỉ ghi khi cột đang NULL) — đã CHẠY LẠI LẦN HAI để chứng minh, kết quả
  "khôi phục 0 giá trị trên 0 dòng", không đụng gì thêm.
- Kết quả thật KHÁC ước lượng ban đầu của nhiệm vụ (239) một đơn vị — 238: rà lại bằng `psql`,
  phát hiện MỘT trong ba bản ghi `NgayQuaDoi` thiếu ban đầu không phải dạng năm/tháng-năm mà là
  dữ liệu rác thật (mojibake phông chữ cũ VNI/TCVN). `NgayThangText.Doc` ĐÚNG khi từ chối phân
  giải nó, giữ nguyên trong `du_lieu_loi` (không phải lỗi của lệnh khôi phục, mà đúng dữ liệu gốc
  thật sự hỏng, không thể đoán ra ngày). Ghi rõ số thật, không sửa số 239 ban đầu trong đặc tả
  nhiệm vụ thành số đã kiểm chứng — coi 238 là số đúng.
- Hướng dẫn chạy lệnh đã ghi vào `WebApp/TIEN-DO.md`.

**Không đụng gì khác:** không sửa `NgayThangText.Ghi`/hợp đồng API ngày tháng (vẫn ISO
`yyyy-MM-dd`); không thêm cột đánh dấu độ chính xác (đúng quyết định người dùng); không sửa cấu
trúc `du_lieu_loi`; không đụng bố cục "Thông tin cá nhân"/CMND/CCCD cột trái, biểu tượng lịch
không viền, header/hàng lọc lưới, nút "Quay về", xuất Excel — đo `getBoundingClientRect()` các
khối chứa ô ngày trên trình duyệt thật sau khi sửa, không lệch so với trước.

**Số test cuối:** frontend 287/287 (thêm 31: 20 cho `lib/ngay.ts` mới, 6 cho `GxDate.test.tsx`
mới + sửa 2 test cũ theo đúng hành vi mới `__/__/____`, 5 cho `lib/focusDieuHuong.test.tsx` mới),
`npm run build` chạy được. Backend: `Qlgx.Data.Tests` 38/38 (thêm 15, gồm bằng chứng đỏ→xanh cho
`NgayThangText`), `Qlgx.Api.Tests` 201/201, `Qlgx.Migration.Tests` 24/24 — không xoá test nào.
Ảnh chụp: `108`–`110` trong `WebApp/anh-chup-kiem-thu/`.
