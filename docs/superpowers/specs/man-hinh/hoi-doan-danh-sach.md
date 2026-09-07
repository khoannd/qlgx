# Màn hình: Danh sách hội đoàn (`frmHoiDoanList.cs` + `frmHoiDoan.cs`)

| | |
|---|---|
| Tệp nguồn | `Source/ChuongTrinh/frmHoiDoanList.cs` (89 dòng, UTF-8 BOM, danh mục) + `Source/GXControl/frmHoiDoan.cs` (663 dòng, UTF-8 BOM, sửa một hội đoàn + danh sách hội viên) + `Source/GXControl/GxListHoiDoan.cs` (120 dòng, UTF-8 BOM, lưới danh mục) |
| UserControl dùng lại | `GxAddEdit` (thanh nút), `GxListHoiDoan : GxGrid` (lưới danh mục hội đoàn), `gxGiaoDanList1` (`GxListGiaoDan`, lưới hội viên — sửa trực tiếp trên ô), `GxDateField`×2, `GxTextField` |
| Bảng dữ liệu đụng tới | `HoiDoan` (6 cột), `ChiTietHoiDoan` (6 cột — `VaiTro` kiểu **Text**, khác `ThanhVienGiaDinh.VaiTro` kiểu số) |
| Trạng thái migrate | Mới — trước task này chưa nối gì (mục "Danh sách hội đoàn" có sẵn trong `SideNav` nhưng không có `id`). Đã dựng `HoiDoanListPage.tsx` (danh mục) + `HoiDoanDetail.tsx` (chi tiết + hội viên). Bảng rỗng ở giáo xứ khảo sát → xem mục 10 cho bằng chứng kiểm thử bằng dữ liệu tự tạo qua chính giao diện. |

**Đây là màn hình QUẢN TRỊ danh mục hội đoàn**, khác hẳn tab "Hội đoàn" trong chi tiết một giáo
dân (`GxHistoryHoiDoan`, đã migrate trước — xem `hoi-doan.md` mục 0). Hai màn hình cùng đụng
bảng `ChiTietHoiDoan` nhưng góc nhìn khác nhau: tab giáo dân xem "MỘT NGƯỜI đã tham gia hội đoàn
nào", màn hình này xem "MỘT HỘI ĐOÀN có những ai".

## 1. Mục đích

Xem/thêm/sửa/xoá các hội đoàn của giáo xứ (Legio Mariae, Gia Trưởng, Hiền Mẫu...) và quản lý
toàn bộ danh sách hội viên của từng hội đoàn: thêm hội viên mới, sửa ngày vào/ra/vai trò, xoá
hội viên khỏi hội đoàn (vĩnh viễn hoặc đánh dấu "đã ra"). Mở từ `frmMain.cs` (chưa xác định vị
trí chính xác trong menu — chưa đọc file này ở nhiệm vụ này, xem mục 9).

## 2. Bố cục và các trường

### Khối "Danh sách hội đoàn" (`frmHoiDoanList`)

Lưới `gxListHoiDoan1` (`GxListHoiDoan.FormatGrid`, `GxListHoiDoan.cs:41-90`), đúng 6 cột theo
thứ tự: Mã hội đoàn, Tên hội đoàn, Thánh bổn mạng, Ngày bổn mạng, Ngày thành lập, Ghi chú — xem
mục 6.

### Khối "Sửa một hội đoàn" (`frmHoiDoan`)

| Nhãn hiển thị | Control | Cột CSDL | Bắt buộc? | Mặc định | Ghi chú |
|---|---|---|---|---|---|
| "Mã hội đoàn" | `txtMaHoiDoan` | `HoiDoan.MaHoiDoan` | có (tự sinh) | `Memory.Instance.GetNextId(...)` lúc Thêm mới (`frmHoiDoan.cs:125-126`) | `ReadOnly` (suy từ Designer, không đọc chi tiết) |
| "Tên hội đoàn" | `txtTenHoiDoan` | `HoiDoan.TenHoiDoan` | có | rỗng | `CheckInputData` bắt buộc (dòng 412-417) |
| "Thánh bổn mạng" | `txtThanhBonMang` | `HoiDoan.ThanhBonMang` | không | rỗng | |
| "Ngày bổn mạng" | `dmNgayBonMang` (`GxDateField`, `IsNullMask=true`) | `HoiDoan.NgayBonMang` (text `dd/MM/yyyy` ở Access) | không | rỗng | |
| "Ngày thành lập" | `dtNgayThanhLap` (`IsNullDate=true`) | `HoiDoan.NgayThanhLap` | không | rỗng | Dùng để kiểm tra "ngày vào/ra hội đoàn không được trước ngày thành lập" (mục 4) |
| "Ghi chú" | `txtGhiChu` | `HoiDoan.GhiChu` | không | rỗng | |
| Lưới hội viên | `gxGiaoDanList1` + 3 cột chèn (`col1`/`col2`/`col3`, `frmHoiDoan.cs:63-98`) | `ChiTietHoiDoan.NgayVaoHoiDoan`/`NgayRaHoiDoan`/`VaiTro` | Ngày vào/ra không bắt buộc (có cảnh báo mềm), Vai trò mặc định "Hội viên" khi thêm qua nút Thêm/Chọn | — | Sửa trực tiếp trên ô (`AllowEdit=True`), cột Vai trò là `DropDownList` lấy `Memory.GetVaiTroHoiDoan()` (chưa đọc nội dung, xem mục 9) |

## 3. Hành vi khi tải

- `frmHoiDoanList_Load` (`frmHoiDoanList.cs:50-55`): ẩn nút "Chọn", bật nút "Tải lại", gọi
  `reloadGrid()` — tải toàn bộ `HoiDoan` (`SqlConstants.SELECT_LIST_HOIDOAN`, chưa đọc SQL chi
  tiết, suy từ tên là `SELECT * FROM HoiDoan` không lọc/sắp xếp tường minh).
- `frmHoiDoan_Load` (`frmHoiDoan.cs:40-133`):
  1. Hai ô ngày `IsNullMask`/`IsNullDate = true`.
  2. Bật nút Tải lại + In.
  3. Định dạng lại lưới hội viên: chèn 3 cột Ngày vào/Ngày ra/Vai trò, khoá `NoEdit` mọi cột gốc
     của `gxGiaoDanList1` (chỉ 3 cột mới chèn cho sửa được).
  4. Tô đỏ + gạch ngang các dòng có `NgayRaHoiDoan` khác rỗng (`GridEXFormatCondition DaRa`,
     dòng 100-107) — hội viên đã ra khỏi hội đoàn.
  5. Nếu sửa (`oldrow != null`): điền dữ liệu hội đoàn từ dòng đã chọn ở danh mục.
  6. Nếu thêm mới: sinh `MaHoiDoan` mới.
  7. Tải lưới hội viên: mặc định `SELECT_LIST_HOIVIEN_HOIDOAN` (chỉ hội viên **hiện tại**, suy
     từ tên — chưa đọc SQL) — checkbox "Thống kê" (`cbThongKe`) BỎ TICK là trạng thái này.

## 4. Quy tắc nghiệp vụ và kiểm tra dữ liệu

### Xoá một hội đoàn từ danh mục (`frmHoiDoanList.gxAddEdit1_DeleteClick`, dòng 26-45)

- Xác nhận: **"Bạn có thật sự muốn xóa hội đoàn này!\r\nNếu có chọn [Yes].\r\nKhông chọn
  [No]"** (tiêu đề "Cảnh báo", mặc định Button2=No).
- Yes → `DELETE FROM ChiTietHoiDoan WHERE MaHoiDoan = ...` rồi `DELETE FROM HoiDoan WHERE
  MaHoiDoan = ...` (xoá CỨNG cả hội viên lẫn hội đoàn).
- Lỗi CSDL → **"Lỗi xóa hội đoàn"** rồi **ĐÓNG LUÔN CẢ MÀN HÌNH DANH SÁCH** (`this.Close()`,
  dòng 41) — hành vi kỳ quặc: một lỗi xoá làm đóng cả màn hình quản lý, không chỉ báo lỗi tại
  chỗ. Không migrate hành vi đóng màn hình này (xem mục 8).

### Lưu một hội đoàn (`gxCommand1_OnOK`, `frmHoiDoan.cs:159-407`) — chuỗi kiểm tra dài

1. `CheckInputData()`: Tên hội đoàn rỗng → **"Vui lòng nhập tên hội đoàn"**; lưới hội viên rỗng
   → **"Cần có ít nhất 1 hội viên trong hội đoàn"**; rồi gọi `ktHoiTruong()`.
2. `ktHoiTruong()` — kiểm tra số hội viên có Vai trò = "Trưởng hội đoàn" (`HoiDoanConst.TruongHoiDoan`)
   VÀ đang hoạt động (`NgayRaHoiDoan is null`):
   - **Nhiều hơn 1** → **"Mỗi hội đoàn chỉ được có một hội trưởng.\r\nChọn [Yes] để đóng màn
     hình và không cập nhập.\r\nChọn [No] để quay lại nhập dữ liệu, không đóng màn hình và
     không cập nhập.\r\nChọn [Cancel] để thoát."** — CẢ BA lựa chọn đều KHÔNG lưu (Yes còn đóng
     màn hình luôn) — thực chất là chặn cứng, chỉ khác nhau ở việc có đóng form hay không.
   - **Đúng 0** (không có hội trưởng nào) → chỉ là **cảnh báo, không chặn**: **"Mỗi hội đoàn cần
     có một hội trưởng.\r\nChọn [Yes] để tiếp tục.\r\nChọn [No] để đóng màn hình và không cập
     nhập.\r\nChọn [Cancel] để quay lại chỉnh sửa dữ liệu."** — Yes → tiếp tục lưu bình thường
     dù không có hội trưởng.
3. Tên hội đoàn trùng với MỘT hội đoàn KHÁC (so sánh chuỗi, không phân biệt hoa/thường theo
   `.Equals` mặc định) → hỏi Yes/No, Yes = vẫn lưu trùng tên, No = huỷ lưu. Không có ràng buộc
   UNIQUE nào ở tầng CSDL Access cho `TenHoiDoan`.
4. Có hội viên nào (mới thêm) đã tồn tại sẵn trong hội đoàn (theo `MaGiaoDan`, tra trên dữ liệu
   TRƯỚC khi sửa) → chặn cứng, báo lỗi nêu tên, không lưu gì.
5. Với TỪNG hội viên chưa xoá trong lưới:
   - Có cả Ngày vào lẫn Ngày ra, Ngày ra ≤ Ngày vào → chặn cứng, báo lỗi nêu tên.
   - Ngày vào SAU ngày hiện tại → hỏi Yes(tiếp tục lưu)/No(đóng form, không lưu)/Cancel(quay
     lại sửa).
   - Ngày vào TRƯỚC ngày thành lập hội đoàn (nếu hội đoàn có Ngày thành lập) → chặn cứng.
   - Ngày vào ≤ Ngày ra GẦN NHẤT của chính hội viên đó trong CHÍNH hội đoàn này (lịch sử vào/ra
     nhiều lần) → chặn cứng, báo lỗi nêu tên + ngày ra lần trước.
   - Lặp lại 3 kiểm tra tương tự cho Ngày ra hội đoàn (SAU ngày hiện tại → Yes/No/Cancel; trước
     ngày thành lập → chặn cứng; ngày ra ≤ ngày ra lần trước → chặn cứng).
6. Toàn bộ hội viên có Ngày vào VÀ Ngày ra đều rỗng → hỏi (YesNoCancel) trước khi lưu: **"Có hội
   viên chưa nhập ngày vào hội.\r\nBạn có muốn tiếp tục cập nhật danh sách hội đoàn
   không?\r\nNhấp nút [Yes] để tiếp tục.\r\nNhấp nút [No] để đóng màn hình mà không cập nhật
   danh sách\r\nNhấp nút [Cancel] để quay lại nhập dữ liệu, không cập nhật và cũng không đóng
   màn hình\r\n"**.
7. Qua hết kiểm tra → lưu `HoiDoan` (thêm hoặc cập nhật) VÀ toàn bộ `ChiTietHoiDoan` (thêm mới/
   cập nhật/xoá theo trạng thái từng dòng trong `DataTable`) trong MỘT `DataSet.UpdateDataSet`
   — tương đương một giao dịch trên Access. Lỗi bất kỳ → **"Update thất bại vui lòng kiểm tra
   lại hoặc liên hệ nhà cung cấp. Xin cảm ơn!"**.

### Thêm hội viên vào lưới (`insertDataGrid`, dòng 470-509)

- Hội viên (theo `MaGiaoDan`) đã có mặt trong lưới VÀ đang ở trạng thái sửa dở (`Deleted` cờ nội
  bộ, chưa nhập `NgayRaHoiDoan`) → **"Giáo dân {tên} đang được sửa đổi vui lòng cập nhật rồi làm
  việc tiếp!!!"**.
- Hội viên đã có mặt (đã ra hay chưa cũng vậy — chỉ cần có dòng nào khớp `MaGiaoDan`) →
  **"Giáo dân {tên} đã tồn tại trong hội đoàn rồi!!!"**.
- Ngược lại: thêm dòng mới, `VaiTro` mặc định **"Hội viên"**, chưa có Ngày vào/ra.
- Hai nguồn chọn hội viên: nút "Thêm" mở `frmGiaoDan` (tạo giáo dân MỚI rồi thêm luôn), nút
  "Chọn" mở `frmChonGiaoDan` (chọn giáo dân CÓ SẴN).

### Xoá một hội viên khỏi lưới (`gxAddEdit1_DeleteClick` của `frmHoiDoan`, dòng 559-605)

- Hội viên đã có `NgayRaHoiDoan` (đã ra hoặc đang sửa dở) → **"Giáo dân này đang được chỉnh sửa
  hoặc là đã được xóa ra khỏi hội đoàn. Vui lòng kiểm tra lại"**, không cho xoá tiếp.
- Ngược lại, hỏi (YesNoCancel): **"Bạn có thực sự muốn giáo dân [{tên}] ra khỏi hội đoàn vĩnh
  viễn.\r\nNếu có chọn [Yes] để xóa.\r\nNếu không chọn [No] để lấy ngày hiện tại làm ngày ra
  khỏi đoàn.\r\nChọn [Cancel] để thoát."**
  - Yes → xoá hẳn dòng khỏi lưới (chưa ghi CSDL cho tới khi bấm "Cập nhật"/OK chính).
  - No → gán `NgayRaHoiDoan = hôm nay` (đánh dấu đã ra, vẫn giữ trong lịch sử).
  - Cancel → không làm gì.

## 5. Thao tác người dùng

| Thao tác | Điều kiện bật/tắt | Hành động |
|---|---|---|
| `frmHoiDoanList`: Thêm | Luôn bật | Mở `frmHoiDoan` trống (Thêm mới) |
| `frmHoiDoanList`: Sửa | Chỉ bật khi có dòng chọn | Mở `frmHoiDoan` với dữ liệu dòng đó |
| `frmHoiDoanList`: Xoá | Chỉ bật khi có dòng chọn | Xem mục 4 |
| `frmHoiDoanList`: double-click dòng | — | Mở Sửa (`gxListHoiDoan_RowDoubleClick` → `EditRow()`) |
| `frmHoiDoan`: Thêm hội viên | Luôn bật | Mở `frmGiaoDan` (tạo giáo dân mới) |
| `frmHoiDoan`: Chọn hội viên | Luôn bật | Mở `frmChonGiaoDan` (chọn giáo dân có sẵn) |
| `frmHoiDoan`: Sửa hội viên | Chỉ bật khi có dòng chọn | `gxGiaoDanList1.EditRow()` — chưa đọc chi tiết mở gì (ngoài phạm vi, không migrate) |
| `frmHoiDoan`: Xoá hội viên | Chỉ bật khi có dòng chọn | Xem mục 4 |
| `frmHoiDoan`: checkbox "Thống kê" | Luôn bật | Tick = tải TOÀN BỘ lịch sử hội viên (kể cả đã ra); bỏ tick = chỉ hội viên hiện tại |
| `frmHoiDoan`: nút In | Luôn bật | Xuất lưới hội viên hiện tại ra file `.xls` tạm rồi mở bằng ứng dụng mặc định (`BtnPrint_Click`) |
| `frmHoiDoan`: nút OK/Cập nhật | — | Chạy toàn bộ chuỗi kiểm tra mục 4 rồi lưu |

## 6. Lưới dữ liệu

### Danh mục hội đoàn (`gxListHoiDoan1`, `GxListHoiDoan.cs:41-90`)

| Thứ tự | Cột | Cột CSDL | Độ rộng |
|---|---|---|---|
| 1 | Mã hội đoàn | `HoiDoan.MaHoiDoan` | 100 |
| 2 | Tên hội đoàn | `HoiDoan.TenHoiDoan` | 200 |
| 3 | Thánh bổn mạng | `HoiDoan.ThanhBonMang` | 200 |
| 4 | Ngày bổn mạng | `HoiDoan.NgayBonMang` | 180 |
| 5 | Ngày thành lập | `HoiDoan.NgayThanhLap` | 180 |
| 6 | Ghi chú | `HoiDoan.GhiChu` | 200 |

### Lưới hội viên (`gxGiaoDanList1` của `frmHoiDoan`)

Toàn bộ cột gốc của `GxListGiaoDan` (khoá `NoEdit`) + 3 cột chèn tại vị trí 3-4-5: "Ngày vào hội
đoàn", "Ngày ra hội đoàn" (cả hai `EditType.CalendarDropDown`, `InputMask="00/00/0000"`), "Vai
trò" (`EditType.DropDownList`, danh sách từ `Memory.GetVaiTroHoiDoan()`). Dòng có `NgayRaHoiDoan`
khác rỗng bị tô **đỏ + gạch ngang** toàn dòng (`GridEXFormatCondition`).

## 7. Liên kết sang màn hình khác

- `frmHoiDoan` → `frmGiaoDan` (thêm giáo dân mới trực tiếp làm hội viên).
- `frmHoiDoan` → `frmChonGiaoDan` (chọn giáo dân có sẵn làm hội viên).
- Không có liên kết ngược nào từ `frmGiaoDan`/`frmGiaDinh` tới màn hình này (khác tab "Hội đoàn"
  của `frmGiaoDan`, vốn dùng `GxHistoryHoiDoan` độc lập — xem `hoi-doan.md`).

## 8. Khác biệt cố ý ở bản web

**Phạm vi cố ý thu hẹp** — không migrate các hộp thoại xác nhận YesNo/YesNoCancel mập mờ ở mục
4 (kiểm tra hội trưởng duy nhất, ngày ở tương lai, trùng tên hội đoàn, hội viên chưa nhập ngày
vào). Backend (`HoiDoanQuanLyService`) chỉ giữ lại các kiểm tra **KHÔNG cần hỏi người** (dữ liệu
sai rõ ràng hoặc mất an toàn nếu bỏ qua):

- Tên hội đoàn bắt buộc (`"Vui lòng nhập tên hội đoàn"`, giữ nguyên văn).
- Một giáo dân không được có 2 lượt tham gia ĐANG HOẠT ĐỘNG (`NgayRaHoiDoan == null`) trong
  CÙNG một hội đoàn (`"Giáo dân này đã tồn tại trong hội đoàn rồi!!!"`, giữ nguyên văn kể cả 3
  dấu `!`) — khớp đúng tinh thần bước "Thêm hội viên" mục 4, nhưng không chặn nếu đã từng ở rồi
  RA (lịch sử vào/ra nhiều lần vẫn hợp lệ, giống `GxHistoryHoiDoan`).

**Không migrate** (ghi vào `can-review-sau.md` để người dùng xác nhận):

- Kiểm tra "đúng 1 hội trưởng" (mục 4 bước 2) — không chặn/không cảnh báo ở bản web. Lý do:
  toàn bộ chuỗi Yes/No/Cancel gốc không nhất quán (có nhánh chặn cứng, có nhánh chỉ cảnh báo,
  có nhánh đóng cả form) và giá trị "Trưởng hội đoàn" chỉ là MỘT chuỗi tự do trong cột `VaiTro`
  (không có ràng buộc CSDL) — kiểm tra lại đúng y hệt cần đọc thêm `Memory.GetVaiTroHoiDoan()`
  (chưa đọc, mục 9) để biết chính xác chuỗi hằng số dùng so sánh.
- Kiểm tra ngày vào/ra không được ở tương lai — không chặn ở bản web (nghiệp vụ hợp lệ: ghi
  trước một hội viên "sẽ vào" hội đoàn kể từ một ngày tương lai không phải điều vô lý, khác các
  trường hợp "ngày rửa tội sau ngày sinh" vốn chắc chắn sai).
- Kiểm tra trùng tên hội đoàn (mục 4 bước 3) — không chặn, cho phép trùng tên (không có ràng
  buộc UNIQUE ở CSDL Access gốc, giữ nguyên).
- Kiểm tra "cần ít nhất 1 hội viên" khi lưu hội đoàn (mục 4 bước 1) — bản web tách lưu hội đoàn
  và lưu hội viên thành hai bước ĐỘC LẬP (xem dưới), một hội đoàn mới tạo hợp lệ có 0 hội viên
  trong khoảnh khắc vừa tạo xong (trước khi thêm hội viên đầu tiên).
- Nút In (xuất `.xls` danh sách hội viên) — chưa có hạ tầng xuất báo cáo dạng bảng tạm này ở
  đây (hạ tầng ClosedXML hiện có phục vụ các màn hình khác qua endpoint riêng, chưa nối cho màn
  hình này). Ưu tiên: Thấp — có thể dùng chức năng in/xuất chung của trình duyệt tạm thời.
- Đóng cả màn hình danh sách khi xoá hội đoàn lỗi (mục 4) — bản web chỉ báo lỗi tại chỗ.

**Cố ý MỞ RỘNG** so với bản gốc:

- **Hội đoàn và hội viên được lưu RIÊNG, mỗi thao tác gọi API ngay** (không gộp thành một giao
  dịch "OK" duy nhất như `gxCommand1_OnOK`). Lý do: bảng trống ở dữ liệu khảo sát nên không có
  ràng buộc "phải sửa xong cả khối mới lưu" nào đang được người dùng dựa vào; tách riêng giúp
  không mất dữ liệu nếu người dùng đóng tab giữa chừng (mô hình tab của bản web khác hộp thoại
  modal của bản gốc). Đây là lệch có chủ đích, ghi vào `can-review-sau.md`.
- **Sửa trực tiếp Ngày vào/Ngày ra/Vai trò của một hội viên đã có** qua form riêng (không sửa
  trên ô lưới như bản gốc) — cùng tinh thần "cho sửa" đã áp dụng ở `hoi-doan.md` mục 8 cho tab
  giáo dân.
- **RowVersion chống ghi đè** cho cả `HoiDoan` lẫn `ChiTietHoiDoan` — bản desktop không có.
- **Có nút xoá hội đoàn** (khác màn hình Giáo họ) — vì cascade xoá ở đây chỉ chạm tới
  `ChiTietHoiDoan` (hội viên CỦA CHÍNH hội đoàn đó), KHÔNG đụng tới bản ghi gốc Giáo dân/Gia
  đình nào (khác giáo họ, nơi xoá kéo theo xoá cả Giáo dân/Gia đình/Bí tích — xem `giao-ho.md`
  mục 4). "Bán kính nổ" nhỏ hơn nhiều nên chấp nhận migrate y hệt tinh thần (xoá cả hội viên),
  chỉ đổi cơ chế cascade từ DELETE tay sang ràng buộc khoá ngoại `ON DELETE CASCADE`.
- Thêm cột "Hội viên" (đếm số hội viên đang hoạt động) trên lưới danh mục — bản gốc không có,
  giúp biết ngay hội đoàn có bao nhiêu người mà không cần mở từng hội đoàn.

## 9. Chỗ chưa chắc

- Vị trí mở `frmHoiDoanList` từ `frmMain.cs` — chưa đọc file này.
- Nội dung `Memory.GetVaiTroHoiDoan()` — danh sách đầy đủ các "Vai trò" hợp lệ trong hội đoàn
  (chỉ biết chắc có "Hội viên" mặc định và hằng số `HoiDoanConst.TruongHoiDoan` dùng để so sánh
  "Trưởng hội đoàn", suy từ tên biến — chưa đọc định nghĩa hằng số/hàm thật).
- Nội dung các câu SQL `SELECT_LIST_HOIDOAN`, `SELECT_LIST_HOIVIEN_HOIDOAN`,
  `SELECT_LIST_HISTORY_HOIVIEN_HOIDOAN`, `SELECT_CHITIETHOIDOAN_BY_MAHOIDOAN`,
  `SELECT_HOIDOAN_BY_MAHOIDOAN` — chưa đọc lớp `SqlConstants.cs`, chỉ suy từ tên và cách dùng.
- `gxGiaoDanList1.EditRow()` (nút Sửa hội viên) — chưa đọc `GxListGiaoDan.EditRow()` mở màn
  hình gì (có thể mở `frmGiaoDan` đầy đủ để sửa hồ sơ giáo dân, không liên quan trực tiếp tới
  thông tin hội đoàn) — không migrate vì ngoài phạm vi "quản lý hội đoàn".

## 10. Kiểm thử bằng dữ liệu tự tạo (bảng rỗng ở giáo xứ khảo sát)

Xem báo cáo kiểm thử trong `.superpowers/sdd/2026-09-06-qlgx-web-phase-1/task-giao-ho-hoi-doan.md`
và ảnh chụp `WebApp/anh-chup-kiem-thu/140-...` trở đi — tạo một hội đoàn mẫu, thêm hội viên, xem
lịch sử vào/ra, sửa, xoá, dọn sạch qua chính giao diện, đối chiếu `psql` trước/sau.
