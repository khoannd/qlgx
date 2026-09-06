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

## Chỗ bản WEB đang lệch so với desktop (khác loại với các mục trên)

Các mục trên là *lỗi của bản desktop* mà ta cố ý tái hiện. Mục dưới đây ngược lại: **bản web
đang làm khác desktop mà không cố ý** — cần sửa để đúng nguyên tắc "giống hệt bản hiện tại".

### W1. Ngày tháng hiển thị sai định dạng ở màn hình chi tiết

- **Phát hiện**: 2026-09-06, khi xem ảnh chụp kiểm thử tab Hôn phối. Ô "Ngày hôn phối" hiện
  `04/25/2015` (định dạng Mỹ MM/DD/YYYY) thay vì `25/04/2015`.
- **Nguyên nhân**: hai màn hình chi tiết dùng tổng cộng **19 ô `<input type="date">` gốc của
  trình duyệt** (`GiaoDanDetail.tsx` 18 ô, `GiaDinhDetail.tsx` 1 ô). Ô ngày gốc **luôn hiển thị
  theo locale của trình duyệt người dùng**, lập trình viên không kiểm soát được. Phía web hiện
  **không có hàm định dạng `dd/MM/yyyy` nào cả**.
- **Bản desktop**: luôn hiển thị `dd/MM/yyyy`, không phụ thuộc máy người dùng. Toàn bộ dữ liệu
  ngày trong Access cũng lưu dạng chuỗi `dd/MM/yyyy`.
- **Rủi ro thật**: người dùng Việt Nam đọc `04/25/2015` sẽ hiểu nhầm, hoặc tệ hơn là **gõ vào
  theo thứ tự sai**. Với ngày mơ hồ như `03/04/2015` thì không ai biết là 3 tháng 4 hay 4 tháng 3
  — sai âm thầm, không có cách phát hiện.
- **Cần quyết**: đây là đánh đổi giao diện. Hai hướng:
  1. Giữ ô ngày gốc (có lịch bấm chọn, hợp với điện thoại) và chấp nhận định dạng theo máy.
  2. Tự làm ô nhập `dd/MM/yyyy` (giống desktop, chắc chắn đúng) nhưng mất lịch bấm chọn gốc.
  Đề xuất: hướng 2 kèm nút mở lịch riêng — nhưng **chờ người dùng quyết**.
