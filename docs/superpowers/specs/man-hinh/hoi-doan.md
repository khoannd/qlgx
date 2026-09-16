# Màn hình: Hội đoàn (`frmHoiDoan.cs` + `GxHistoryHoiDoan.cs`)

| | |
|---|---|
| Tệp nguồn | `Source/ChuongTrinh/frmHoiDoanList.cs` (89 dòng, danh mục) + `Source/GXControl/frmHoiDoan.cs` (663 dòng, sửa một hội đoàn + danh sách hội viên) + `Source/GXControl/GxListHoiDoan.cs` (120 dòng, lưới danh mục) + `Source/GXControl/GxHistoryHoiDoan.cs` (311 dòng, khối nhúng trong `frmGiaoDan`) + `Source/GXControl/GxListHistoryHoiDoan.cs` (86 dòng, lưới lịch sử) — tất cả UTF-8 |
| UserControl dùng lại | `GxListGiaoDan`/`gxGiaoDanList1` (lưới hội viên trong `frmHoiDoan`, chỉnh sửa trực tiếp trên ô), `GxComboField` (chọn tên hội đoàn trong `GxHistoryHoiDoan`), `GxDateField` (2 ô ngày), `GxAddEdit` (thanh nút Thêm/Sửa/Xoá/Chọn/Tải lại/In dùng chung) |
| Bảng dữ liệu đụng tới | `HoiDoan` (6 cột, danh mục hội đoàn), `ChiTietHoiDoan` (6 cột, bảng nối hội đoàn↔giáo dân — `VaiTro` ở đây là **Text**, khác hẳn `ThanhVienGiaDinh.VaiTro` là số) |
| Trạng thái migrate | chưa → tab "Hội đoàn" trong `GiaoDanDetail.tsx` hiện là khung tĩnh |

## 0. Có HAI nơi thao tác dữ liệu hội đoàn trong bản desktop, giống mô hình Hôn phối

- **`frmHoiDoanList.cs` + `frmHoiDoan.cs`** — màn hình **quản trị danh mục hội đoàn**: thêm/sửa/
  xoá một hội đoàn (`HoiDoan`) VÀ toàn bộ danh sách hội viên của nó cùng lúc (lưới `gxGiaoDanList1`
  cho phép sửa trực tiếp Ngày vào/Ngày ra/Vai trò của MỌI hội viên). Mở từ menu (chưa xác định vị
  trí chính xác trong `frmMain.cs`, xem mục 9). Không có điểm gọi nào từ `frmGiaoDan.cs`.
- **`GxHistoryHoiDoan.cs`** — `UserControl` nhúng thẳng trong tab "Hội Đoàn" của `frmGiaoDan`
  (biến `gxHistoryHoiDoan1`, `Source/GXControl/frmGiaoDan.cs:235-236`, `560-568`). Đây là nơi
  **thực sự đang chạy** khi người dùng xem/thêm hội đoàn từ màn hình chi tiết MỘT giáo dân —
  giống vai trò của `GxHonPhoiGiaDinh` so với `frmHonPhoi` trong `hon-phoi.md` mục 0.

**Kết luận dùng cho tab "Hội đoàn" ở `GiaoDanDetail.tsx`:** migrate theo hành vi của
`GxHistoryHoiDoan` (xem lịch sử hội đoàn của MỘT giáo dân, thêm mới một lượt tham gia), KHÔNG
theo `frmHoiDoan` (quản trị toàn bộ hội viên của MỘT hội đoàn, sửa trực tiếp trên lưới) — vì màn
hình đang làm là "chi tiết giáo dân", đối tượng trung tâm là NGƯỜI chứ không phải HỘI ĐOÀN. Quản
trị danh mục hội đoàn (`frmHoiDoanList`) là một màn hình khác, chưa nằm trong phạm vi việc này.

## 1. Mục đích

`GxHistoryHoiDoan`: xem toàn bộ lịch sử tham gia hội đoàn của một giáo dân (tên hội đoàn, ngày
vào, ngày ra, vai trò) và thêm một lượt tham gia mới (chọn hội đoàn có sẵn từ danh mục, nhập
ngày vào/ngày ra). KHÔNG sửa được vai trò hay ngày của các lượt tham gia đã có — chỉ thêm mới
(xem mục 5).

## 2. Bố cục và các trường

### Khối nhập "Thêm mới" (`panel1`, luôn ở trên cùng, `GxHistoryHoiDoan.Designer.cs:44-100`)

| Nhãn hiển thị | Control | Cột CSDL khi lưu | Bắt buộc? | Mặc định | Ghi chú |
|---|---|---|---|---|---|
| "Tên hội đoàn" | `cbTenHoiDoan` (`GxComboField`) | `ChiTietHoiDoan.HoiDoanId` (qua `MaHoiDoan`) | có (để lưu) | không chọn (`SelectedIndex=-1`) | `DataSource` = TOÀN BỘ bảng `HoiDoan` (`Select * from HoiDoan`, `GxHistoryHoiDoan.cs:300`), `ValueMember=MaHoiDoan`, `DisplayMember=TenHoiDoan`; gõ tay bị chặn (`Combo_KeyPress`/`Combo_KeyDown` đặt `e.Handled=true`, chỉ chọn từ danh sách) |
| "Ngày vào hội đoàn" | `dtNgayVaoHoiDoan` | `ChiTietHoiDoan.NgayVaoHoiDoan` | khuyến nghị (có cảnh báo, không chặn cứng) | `IsNullDate=true` | `DateInput.ReadOnly=true` khi tải xong (`GxHistoryHoiDoan.cs:292`) — chỉ chọn qua lịch, không gõ tay |
| "Ngày ra hội đoàn" | `dtNgayRaHoiDoan` | `ChiTietHoiDoan.NgayRaHoiDoan` | không | `IsNullDate=true` | `ReadOnly=true` cùng lý do |

Không có ô nhập "Vai trò" trong `GxHistoryHoiDoan` — **luôn hard-code `"Hội viên"`** khi lưu
(`GxHistoryHoiDoan.cs:265, 272`), không cho người dùng chọn giá trị khác qua control này (khác
`frmHoiDoan`, nơi cột Vai trò trên lưới hội viên CÓ combo chọn qua `GxListHoiDoan.LoadVaiTroHoiDoan`
— danh sách vai trò lấy từ `Memory.GetVaiTroHoiDoan()`, chưa đọc định nghĩa, xem mục 9).

### Lưới lịch sử (`gxListHistoryHoiDoan1`, chỉ để XEM — `GxListHistoryHoiDoan.cs:30-74`)

| Thứ tự | Cột | Cột CSDL | Độ rộng | `EditType` |
|---|---|---|---|---|
| 1 | "Tên hội đoàn" | `HoiDoan.TenHoiDoan` | 200 | `NoEdit` |
| 2 | "Ngày vào hội đoàn" | `ChiTietHoiDoan.NgayVaoHoiDoan` | 210 | `NoEdit` |
| 3 | "Ngày ra hội đoàn" | `ChiTietHoiDoan.NgayRaHoiDoan` | 210 | `NoEdit` |
| 4 | "Vai trò" | `ChiTietHoiDoan.VaiTro` | 226 | `NoEdit` |

Toàn bộ 4 cột đều `EditType.NoEdit` — lưới lịch sử **hoàn toàn chỉ đọc**, kể cả với các dòng vừa
thêm trong phiên làm việc hiện tại. Không có định dạng ngày `dd/MM/yyyy` áp cho các cột ngày ở
đây (đoạn `FormatString`/`FormatMode` bị **comment hết**, dòng 44-49 — khác `frmHoiDoan.cs` nơi
cột tương tự CÓ set `FormatString = "dd/MM/yyyy"`, xem mục 6) — cột hiện text thô theo cách CSDL
trả về (chuỗi `dd/MM/yyyy` vì `NgayVaoHoiDoan`/`NgayRaHoiDoan` là kiểu Text trong Access, đọc
bằng `NgayThangText.Doc` — xem ghi chú entity `ChiTietHoiDoan.cs` phía web).

## 3. Hành vi khi tải

- `GxHistoryHoiDoan_Load` (`GxHistoryHoiDoan.cs:290-309`):
  1. Đặt hai ô ngày `ReadOnly=true`.
  2. Ẩn nút Xoá/Tải lại/Sửa trên `gxAddEdit1` (`DeleteButton.Visible=false`,
     `ReloadButton.Visible=false`, `EditButton.Visible=false`) — chỉ còn Thêm + Chọn(Lưu).
  3. Đổi nhãn nút "Chọn" thành **"&Lưu"**.
  4. Tải TOÀN BỘ danh mục `HoiDoan` vào combo (không lọc gì, không sắp xếp tường minh — thứ tự
     phụ thuộc CSDL trả về).
  5. Chặn gõ tay vào combo.
  6. `EditEnableControl(false)` — khối nhập bị vô hiệu hoá (`panel1.Enabled=false`), nút Thêm
     hiện chữ **"&Thêm"**.
- `loaddata(maGiaoDan)` (gọi từ `frmGiaoDan.cs:236` khi vào chế độ Sửa) chạy câu truy vấn
  `SqlConstants.SELECT_LIST_HISTORY_HOIDOAN_BY_MAGIAODAN` (chưa đọc nội dung SQL — mục 9, nhưng
  tên gợi ý: lấy toàn bộ `ChiTietHoiDoan` JOIN `HoiDoan` theo `MaGiaoDan`, không giới hạn số
  dòng — đúng với việc hiển thị "lịch sử").
- Không tải gì khi `operation==ADD` (giáo dân mới, chưa có Id) — control không được gọi
  `loaddata` trong nhánh này của `frmGiaoDan.cs` (chỉ gọi ở nhánh `EDIT`, dòng 235-236).

## 4. Quy tắc nghiệp vụ và kiểm tra dữ liệu

### Bấm nút "Thêm" (`gxAddEdit1_AddClick`, `GxHistoryHoiDoan.cs:78-93`)

- Danh mục `HoiDoan` rỗng (`tblDanhSachHoiDoan.Rows.Count<=0`) → chặn, báo:
  > "Hiện tại chưa có hội đoàn nào !" (`MessageBoxIcon.Warning`, chỉ nút OK)
- Đang ở chế độ Thêm rồi (`operation==ADD`, tức người dùng bấm lại nút vừa đổi thành "&Thôi") →
  **huỷ chế độ thêm** (`EditEnableControl(false)`, `IsChanging=false`) — nút Thêm hoạt động như
  nút Thêm/Thôi bật-tắt (toggle), không phải luôn luôn mở form thêm mới.
- Ngược lại → bật chế độ thêm (`EditEnableControl(true)`): `panel1.Enabled=true`, đổi nhãn nút
  Thêm thành **"&Thôi"**, `operation=ADD`, `IsChanging=true`.

### Bấm nút "Lưu" (đã đổi tên từ "Chọn", `gxAddEdit1_SelectClick` → `UpdateHoiDoan()`, dòng 126-288)

Thứ tự kiểm tra, mỗi bước có thể dừng lại (trả `false`, không lưu) hoặc **âm thầm bỏ qua** (trả
`true` — coi như "không có gì sai" dù không lưu gì, y hệt kiểu "trả true khi không làm gì" đã
thấy ở `GxTanHien.UpdateData`/`GxHonPhoiGiaDinh.checkInput`):

1. Chưa chọn hội đoàn (`cbTenHoiDoan.Combo.SelectedIndex<0`) → hỏi:
   > "Vui lòng chọn tên hội đoàn !! Bạn có muốn chọn tên hội đoàn!!!\r\n Chọn [Yes] nếu có.\r\n
   > Chọn [No] để không cập nhật (không lưu)" (nguyên văn, kể cả 2 dấu `!` liền và khoảng trắng
   > thừa trước "Chọn [Yes]")
   - **Yes → trả `false`** (không lưu, ở lại form) — **No → trả `true`** (coi như xong, đóng
     khối nhập lại, KHÔNG lưu gì). Đây là cách dùng Yes/No **NGƯỢC TRỰC GIÁC**: người dùng chọn
     "Yes" (muốn chọn hội đoàn) lại là lựa chọn khiến hàm "thất bại" (`return false`), còn "No"
     (không muốn chọn) mới coi như "thành công". Ghi vào `can-review-sau.md`.
2. Giáo dân này đã ở trong đúng hội đoàn đó và **chưa ra** (`NgayRaHoiDoan is null`) — tra trên
   dữ liệu ĐÃ TẢI trong lưới lịch sử (`tblDanhSachLichSuHoiDoan`, không truy vấn CSDL lại) bằng
   `DataTable.Select("TenHoiDoan= '...' and NgayRaHoiDoan is null")` — **so khớp theo TÊN hội
   đoàn dạng chuỗi**, không theo Id, có nguy cơ khớp nhầm nếu hai hội đoàn trùng tên (không có
   ràng buộc UNIQUE nào đảm bảo tên hội đoàn là duy nhất) → hỏi:
   > "Hiện tại giáo dân đã ở trong hội đoàn {tên} rồi!! Bạn có muốn chỉnh sửa!!!\r\n Chọn [Yes]
   > nếu có.\r\n Chọn [No] để không cập nhật (không lưu)"
   - Cùng kiểu Yes/No ngược ở bước 1: **Yes → `false`**, **No → `true`** (không lưu).
3. Chưa nhập Ngày vào hội đoàn (`IsNullOrEmpty`) → hỏi (YesNoCancel):
   > "Chưa nhập ngày vào hội đoàn.Bạn có muốn tiếp tục không.\r\nChọn [Yes] để tiếp tục.\r\nChọn
   > [No] để quay lại nhập ngày vào hội đoàn.\r\nChọn [Cancel] hoặc tắt hộp thoại để thoát không
   > cập nhật" (thiếu dấu cách sau "không." — chép nguyên văn)
   - Cancel → trả `true` (không lưu, coi như xong). No → trả `false` (không lưu, ở lại form).
   - Yes → (không return ở đây) tiếp tục xuống các bước sau — nghĩa là **có thể lưu một lượt
     tham gia hoàn toàn không có Ngày vào hội đoàn** nếu người dùng chọn Yes ở đây.
4. Có cả Ngày vào lẫn Ngày ra và Ngày vào ≥ Ngày ra (`CompareTwoStringDate >= 0`) → hỏi (YesNo):
   > "Vui lòng kiểm tra lại. Ngày ra hội đoàn không thể trước ngày vào hội đoàn. Bạn có muốn
   > quay lại chỉnh sửa không?.\r\n Chọn [Yes] nếu có.\r\n Chọn [No] để không cập nhật (không
   > lưu)." — **Yes → `false`** (ở lại sửa), **No → `true`** (không lưu, thoát).
5. Có Ngày vào, hội đoàn đã chọn có `NgayThanhLap`, và Ngày thành lập > Ngày vào → hỏi tương tự
   (Yes → `false`, No → `true`), nội dung nêu rõ ngày thành lập.
6. Có Ngày vào, giáo dân này từng có một lượt tham gia ĐÃ RA khỏi hội đoàn nào đó trước đây (bất
   kỳ hội đoàn nào khớp điều kiện dòng đầu tiên của `Memory.GetData(SELECT_CHITIETHOIDOAN_BY_MAHOIDOAN
   + " and NgayRaHoiDoan is not null order by NgayRaHoiDoan is null", MaHoiDoan)` rồi lọc tiếp
   theo `MaGiaoDan` — **chú ý**: câu SQL lọc theo `MaHoiDoan` của hội đoàn ĐANG CHỌN, không phải
   mọi hội đoàn) và Ngày ra của lượt gần nhất ≥ Ngày vào mới → hỏi (YesNo), thông báo nêu tên
   giáo dân và ngày ra gần nhất — **No → `false`** (ở lại sửa), **Yes → `true`** (không lưu,
   thoát) — **đảo ngược Yes/No so với các bước 1, 2, 4, 5** (ở đây Yes lại là "thoát không lưu",
   No là "quay lại sửa") — thêm một điểm bất nhất Yes/No nữa, ghi vào `can-review-sau.md`.
7. Tương tự bước 5-6 nhưng cho Ngày ra hội đoàn (nếu có nhập): so ngày thành lập, so ngày ra gần
   nhất trước đó — cùng kiểu thông báo và cùng kiểu đảo Yes/No như bước 6.
8. Qua hết các kiểm tra → chèn 1 dòng vào lưới lịch sử tại chỗ (không tải lại từ CSDL) VÀ 1 dòng
   mới vào bảng `ChiTietHoiDoan` với `VaiTro` hard-code `"Hội viên"`, `MaGiaoDan`, `MaHoiDoan`,
   Ngày vào/ra như đã nhập → `Memory.UpdateDataSet(ds)`. Lỗi CSDL → trả `false` (không đóng khối
   nhập). Thành công → `EditEnableControl(false)`, `IsChanging=false` (đóng khối nhập lại).

## 5. Thao tác người dùng

| Thao tác | Điều kiện bật/tắt | Hành động |
|---|---|---|
| Nút "&Thêm"/"&Thôi" (toggle) | Luôn bật | Mở/đóng khối nhập "Thêm mới" — xem mục 4 |
| Nút "&Lưu" (đổi tên từ "Chọn") | Chỉ có tác dụng khi khối nhập đang mở | Chạy `UpdateHoiDoan()` — xem mục 4; thành công thì đóng khối nhập |
| Nút Xoá | **Luôn ẩn** (`DeleteButton.Visible=false`) | — không có cách xoá một lượt tham gia qua control này |
| Nút Sửa | **Luôn ẩn** | — không sửa được lượt tham gia đã có, chỉ xem |
| Nút Tải lại | **Luôn ẩn** | — |
| Double-click dòng trên lưới lịch sử | không gán handler nào | không có hành động |
| Lưu gộp cùng `frmGiaoDan` | `gxHistoryHoiDoan1.IsChanging` (đang mở khối nhập, đã set `true` khi bấm Thêm) | `frmGiaoDan.cs:560-568`: bấm "Cập nhật" ở form giáo dân gọi `gxHistoryHoiDoan1.UpdateHoiDoan()`; thất bại thì nhảy về tab Hội Đoàn, KHÔNG đóng form |

Khác với Hôn phối/Ơn gọi tận hiến (lưu tự động cùng "Cập nhật" chính không cần điều kiện), khối
Hội đoàn **chỉ được `UpdateHoiDoan()` gọi lại lần nữa từ `frmGiaoDan` NẾU `IsChanging==true`**
(tức người dùng đang có một khối "Thêm mới" dở dang chưa bấm Lưu) — nếu người dùng đã bấm Lưu
xong trong lúc ở tab Hội Đoàn (khối nhập đã đóng, `IsChanging=false`) thì nút "Cập nhật" chính
KHÔNG gọi lại `UpdateHoiDoan()` lần nữa (dữ liệu đã lưu ngay từ lúc bấm "Lưu" trong tab, không
đợi tới khi bấm "Cập nhật" cuối trang).

## 6. Lưới dữ liệu

Đã mô tả ở mục 2 (`gxListHistoryHoiDoan1`, 4 cột, toàn bộ `NoEdit`, không định dạng ngày).
Không tô màu/gạch ngang điều kiện nào trong `GxListHistoryHoiDoan.FormatGrid()` (khác `frmHoiDoan`
— nơi lưới hội viên CÓ gạch đỏ + gạch ngang cho hội viên đã ra khỏi hội đoàn, `frmHoiDoan.cs:101-107`
— không áp dụng cho phạm vi tab giáo dân đang migrate, xem mục 0).

## 7. Liên kết sang màn hình khác

`GxHistoryHoiDoan` không tự mở màn hình nào — chỉ được `frmGiaoDan` gọi `loaddata`/`UpdateHoiDoan`/
đọc `IsChanging`. (Ngược lại, `frmHoiDoan`/`frmHoiDoanList` — không thuộc phạm vi migrate lần này
— có mở `frmGiaoDan`, `frmChonGiaoDan` để thêm hội viên mới vào một hội đoàn, và
`GxGiaDinhList`/xuất Excel cho nút In — ghi lại để biết phạm vi, không migrate ở đây.)

## 8. Khác biệt cố ý ở bản web

- **Chỉ migrate hành vi của `GxHistoryHoiDoan`** (xem một giáo dân đã tham gia hội đoàn nào,
  thêm mới một lượt tham gia) — KHÔNG migrate màn hình quản trị danh mục hội đoàn
  (`frmHoiDoanList`/`frmHoiDoan`, sửa trực tiếp trên lưới hội viên của một hội đoàn, nút Xoá/Sửa
  hội viên, kiểm tra "phải có đúng một hội trưởng"...) — nằm ngoài phạm vi "màn hình chi tiết
  giáo dân". Đây là khối lượng công việc riêng, để dành cho một task khác (giống cách "chọn/đổi
  vợ chồng" của Hôn phối được để dành ở `hon-phoi.md` mục 8).
- **KHÔNG migrate các cặp Yes/No ngược trực giác** ở mục 4 bước 1, 2, 6, 7 — bản web dùng nút
  "Huỷ"/"Lưu" tường minh (không dùng hộp thoại xác nhận kiểu Yes/No mập mờ). Đây là **cải tiến
  có chủ đích**, ghi rõ vào `can-review-sau.md` vì đi ngược nguyên tắc "migrate y hệt" — cần
  người dùng xác nhận đây là điều họ muốn (giữ tinh thần kiểm tra, bỏ cách hỏi mập mờ) chứ không
  phải bỏ sót.
- **KHÔNG hard-code `VaiTro = "Hội viên"`** một cách cứng nhắc: bản web migrate y hệt giá trị mặc
  định là "Hội viên" khi thêm mới NHƯNG cho phép sửa "Vai trò" của một lượt tham gia đã có (một
  điểm bản desktop hoàn toàn không hỗ trợ, mục 5 — "không có nút Sửa"). Đây là điểm **cố ý mở
  rộng** để tab thật sự dùng được (một hội đoàn thường có "hội trưởng"/"phó" cần đổi vai trò theo
  thời gian) — ghi vào `can-review-sau.md`.
- **Cho phép sửa Ngày vào/Ngày ra của một lượt tham gia đã có** — cùng lý do trên, khác hẳn bản
  desktop (`NoEdit` toàn bộ 4 cột, mục 2).
- **RowVersion chống ghi đè** cho từng bản ghi `ChiTietHoiDoan` — bản desktop không có.
- Không migrate toàn bộ chuỗi kiểm tra nghiệp vụ phức tạp ở mục 4 (bước 3-7: cảnh báo ngày thành
  lập, ngày ra lần trước...) ở Task này — xem mục 9/`can-review-sau.md`; bản web hiện chỉ kiểm
  tra tối thiểu (hội đoàn hợp lệ, RowVersion đúng). Đây là **thiếu sót đã biết**, không phải cố ý
  bỏ vĩnh viễn.

## 9. Chỗ chưa chắc

- Điểm mở `frmHoiDoanList` từ `frmMain.cs` (menu nào) — chưa đọc `frmMain.cs` trong nhiệm vụ này.
- Nội dung `Memory.GetVaiTroHoiDoan()` (danh sách vai trò dùng trong `frmHoiDoan`, KHÔNG dùng
  trong `GxHistoryHoiDoan`) — chưa đọc, không rõ có bao nhiêu giá trị/gồm những gì ngoài "Hội
  viên" và `HoiDoanConst.TruongHoiDoan` ("Trưởng hội đoàn", suy từ tên hằng số dùng ở
  `frmHoiDoan.ktHoiTruong()`).
- Nội dung câu SQL `SqlConstants.SELECT_LIST_HISTORY_HOIDOAN_BY_MAGIAODAN` và
  `SELECT_CHITIETHOIDOAN_BY_MAHOIDOAN` — chưa đọc lớp `SqlConstants.cs`, chỉ suy từ tên và cách
  dùng.
- Vì sao thứ tự Yes/No bị đảo ngược giữa các bước kiểm tra (mục 4) — không tìm được lý do trong
  mã, nhiều khả năng là lỗi lập trình (copy-paste không nhất quán) chứ không phải chủ đích.

## 10. Đối chiếu bản web hiện tại (điền sau khi migrate xong)

| Hành vi bản desktop | Bản web đã có? | Ghi chú |
|---|---|---|
| Xem lịch sử hội đoàn của một giáo dân | *(điền khi migrate xong)* | |
| Thêm một lượt tham gia mới (chọn hội đoàn, ngày vào/ra, vai trò mặc định "Hội viên") | | |
| Sửa một lượt tham gia đã có | Cố ý MỞ RỘNG (desktop không hỗ trợ) | Xem mục 8 |
| Toàn bộ chuỗi kiểm tra nghiệp vụ phức tạp + hộp thoại Yes/No ngược (mục 4) | Cố ý KHÔNG migrate y hệt | Xem mục 8, `can-review-sau.md` |
| Quản trị danh mục hội đoàn (`frmHoiDoanList`/`frmHoiDoan`) | Ngoài phạm vi | Việc khác |
