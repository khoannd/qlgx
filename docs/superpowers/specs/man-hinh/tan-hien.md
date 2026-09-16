# Màn hình: Ơn gọi tận hiến (`GxTanHien.cs`)

| | |
|---|---|
| Tệp nguồn | `Source/GXControl/GxTanHien.cs` (291 dòng, mã hoá UTF-16LE) + `Source/GXControl/GxTanHien.Designer.cs` (386 dòng) |
| UserControl dùng lại | `GxComboField` (Chức vụ), `GxDateField` (9 ô ngày), `GxTextField` (6 ô văn bản), `CheckBox` chuẩn WinForms (Đã hồi tục) |
| Bảng dữ liệu đụng tới | `TanHien` (20 cột, không có bảng nối nào khác — khác với Hôn phối/Hội đoàn) |
| Trạng thái migrate | đang → xem mục 10. Bản web sắp thêm tab "Ơn gọi tận hiến" vào `GiaoDanDetail.tsx` |

## 0. Đây là `UserControl` nhúng trong `frmGiaoDan`, không có form độc lập

Khác với Hôn phối (có cả `frmHonPhoi` độc lập lẫn `GxHonPhoiGiaDinh` nhúng), Ơn gọi tận hiến
**chỉ có một nơi duy nhất**: `GxTanHien` — `UserControl` nhúng thẳng vào tab "Ơn gọi" của
`frmGiaoDan` (biến `gxTanHien1`, `Source/GXControl/frmGiaoDan.cs:228-230`). Không có picker chọn
người — control luôn thao tác trên giáo dân đang mở (`MaGiaoDan` gán từ `frmGiaoDan.Id`).

## 1. Mục đích

Ghi lại các mốc thời gian ơn gọi tận hiến (đi tu) của MỘT giáo dân: ngày nhập dòng, ngày vào nhà
thử/nhà tập/Đại chủng viện, ngày khấn lần đầu/vĩnh viễn, ngày chịu chức phó tế/linh mục, nơi tu,
dòng tu/chủng viện, nơi phục vụ hiện tại, và cờ "đã hồi tục". Đây là các mốc của **một** hành
trình ơn gọi — không phải nhiều "giai đoạn" tách rời như hôn phối/tái hôn.

## 2. Bố cục và các trường

| Nhãn hiển thị (Designer) | Control | Cột CSDL | Bắt buộc? | Mặc định | Ghi chú |
|---|---|---|---|---|---|
| "Ngày nhập dòng" | `dtNgayBatDau` | `TanHien.NgayBatDau` | không | `IsNullDate=true` | (`Designer.cs:215`) |
| "Ngày vào ĐCV" | `dtNgayVaoDaiChungVien` | `TanHien.NgayVaoDCV` | không | trống | (`Designer.cs:201`) |
| "Ngày vào nhà thử" | `dtNgayVaoNhaThu` | `TanHien.NgayVaoNhaThu` | không | trống | (`Designer.cs:187`) |
| "Ngày vào nhà tập" | `dtNgayVaoNhaTap` | `TanHien.NgayVaoNhaTap` | không | trống | (`Designer.cs:173`) |
| "Ngày khấn lần đầu" | `dtNgayKhanLanDau` | `TanHien.NgayVaoKhanLanDau` | không | trống | (`Designer.cs:159`) — chú ý tên control (`KhanLanDau`) khác tên cột CSDL (`NgayVaoKhanLanDau`) |
| "Ngày khấn vĩnh viễn" | `dtNgayKhanVinhVien` | `TanHien.NgayVaoKhanTronDoi` | không | trống | (`Designer.cs:145`) — **nhãn "vĩnh viễn"/tên control "KhanVinhVien" nhưng cột CSDL lại là `NgayVaoKhanTronDoi`** ("trọn đời") — đồng nghĩa nhưng chữ khác nhau, giữ nguyên khi migrate |
| "Ngày lãnh chức phó tế" | `dtNgayPhoTe` | `TanHien.NgayPhoTe` | không | trống | (`Designer.cs:131`) |
| "Ngày thụ phong LM" | `dtNgayThuPhongLM` | `TanHien.NgayThuPhongLM` | không | trống | (`Designer.cs:117`) |
| "Ngày mừng bổn mạng" | `dtNgayBonMang` | `TanHien.NgayBonMang` | không | trống | (`Designer.cs:103`) |
| **"Địa chỉ"** | `txtNoiTu` | `TanHien.NoiTu` | không | rỗng | (`Designer.cs:70`) — **nhãn hiển thị là "Địa chỉ" nhưng tên control và cột CSDL đều là "NoiTu" (nơi tu)** — khả năng cao là nhãn đặt sai/đổi ý giữa chừng lúc code, xem mục 4 |
| "Chức vụ" | `cbChucVu` | `TanHien.ChucVu` | không | rỗng, `SelectedIndex` không ép | 7 giá trị cố định — xem mục 4 |
| "Dòng tu/chủng viện" | `txtDongTu` | `TanHien.DongTu` | không | rỗng | (`Designer.cs:325`) |
| "Nơi phục vụ" | `txtNoiPhucVu` | `TanHien.NoiPhucVu` | không | rỗng | |
| "Địa chỉ nơi phục vụ" | `txtDiaChiPhucVu` | `TanHien.DiaChiPhucVu` | không | rỗng | |
| "Điện thoại nơi phục vụ" | `txtDienThoaiPhucVu` | `TanHien.DienThoaiPhucVu` | không | rỗng | |
| "Email  nơi phục vụ" (2 dấu cách) | `txtEmailPhucVu` | `TanHien.EmailPhucVu` | không | rỗng | nhãn gốc có 2 khoảng trắng liền giữa "Email" và "nơi" (`Designer.cs:249`) — lỗi chính tả nhỏ, chép nguyên văn |
| "Đã hồi tục" | `chkHoiTuc` (CheckBox thường) | `TanHien.DaHoiTuc` | — | `false` | |
| "Ghi chú" | `txtGhiChu` | `TanHien.GhiChu` | không | rỗng | |

Không có ô nhập cho `TanHien.MaTanHien`/`MaTanHienCu`/`MaGiaoDan` — sinh/gán tự động, không hiện
trên giao diện (khác Hôn phối, nơi `frmHonPhoi` có ô Mã hôn phối `ReadOnly`).

## 3. Hành vi khi tải

- `AssignControlData()` (`GxTanHien.cs:86-102`) gọi `GetData(maGiaoDan)`:
  `SELECT * FROM TanHien WHERE MaGiaoDan=<id>` (`GxTanHien.cs:77`) — **không `ORDER BY`**, không
  giới hạn `TOP 1`, nhưng chỉ dùng **`Rows[0]`** nếu có (`GxTanHien.cs:93`) — giống hệt kiểu lấy-
  bừa-hàng-đầu-tiên của `frmHonPhoi.GetHonPhoi` (đã ghi ở `can-review-sau.md` mục 9). Nếu giáo dân
  có nhiều hơn 1 dòng `TanHien` (không có ràng buộc DB nào cấm), control chỉ hiện/sửa **một**
  dòng bất kỳ theo thứ tự CSDL trả về; các dòng khác bị "ẩn" (không mất, chỉ không truy cập được
  qua UI này).
  - Có dữ liệu (`Rows.Count > 0`) → `AssignControlData(row)`, `operation = EDIT`.
  - Không có → `operation = ADD`, sinh sẵn `maTanHien` bằng
    `Memory.Instance.GetNextId(TanHienConst.TableName, TanHienConst.MaTanHien, false)`
    (`GxTanHien.cs:99`) — sinh ngay lúc tải, **trước khi** người dùng nhập gì, giống cách
    `frmHonPhoi` sinh mã lúc `Load`.
- Không có nút bấm nào để chọn xem "giai đoạn" nào nếu có nhiều — không có khái niệm nhiều giai
  đoạn ở UI desktop cho màn hình này (khác Hôn phối, nơi ý niệm "nhiều bản ghi theo thời gian" ít
  nhất còn có trong comment `ChonHonPhoiHienTai`). Xem mục 9 — chưa tìm thấy bằng chứng nào trong
  mã đã đọc cho thấy desktop dự định hỗ trợ nhiều dòng `TanHien` một người.

## 4. Quy tắc nghiệp vụ và kiểm tra dữ liệu

### Danh sách "Chức vụ" (`cbChucVu`, constructor `GxTanHien.cs:20-29`)

7 giá trị cố định theo đúng thứ tự thêm vào combo, KHÔNG đọc từ CSDL:
`""`, `"Tu sĩ"`, `"Chủng sinh"`, `"Phó tế"`, `"Linh mục"`, `"Giám mục"`, `"Khấn trọn"`, `"Khác"`.

### `checkInput()` (`GxTanHien.cs:238-262`) — chỉ chạy khi có dữ liệu để lưu

- Tra `GiaoDan` theo `MaGiaoDan` (dùng `SqlConstants.SELECT_GIAODAN_THEO_ID`, chưa đọc nội dung
  câu SQL này — mục 9).
- Chỉ kiểm tra khi **có dữ liệu để lưu** (`!isNull()`, xem định nghĩa `isNull()` dưới) **và**
  `GiaoDanRow` đã tải thành công. Nếu `GiaoDanRow[DaCoGiaDinh] == true` (giáo dân này đã có gia
  đình) → hỏi xác nhận nguyên văn:
  > "Giáo dân này đã có gia đình. Bạn có chắc muốn nhập thông tin tận hiến cho giáo dân này
  > không?" (`GxTanHien.cs:248`, `MessageBoxButtons.YesNoCancel`)
  - Chọn **No** → gọi `Clear()` (xoá sạch mọi ô trên control) rồi trả `false` (không lưu).
  - Chọn **Cancel** → trả `false` (không lưu), **không** `Clear()` — dữ liệu người dùng gõ vẫn
    còn nguyên trên control nhưng không lưu.
  - Chọn **Yes** (ngầm định, không có nhánh `else` xử lý) → tiếp tục lưu bình thường.
  - **Đây là một quy tắc kỳ quặc đáng chú ý**: hỏi "giáo dân đã có gia đình, chắc chưa" cho một
    control hoàn toàn không liên quan tới hôn nhân (ơn gọi tận hiến thường loại trừ lẫn nhau với
    có gia đình — có thể đây chính là ý đồ: cảnh báo mâu thuẫn dữ liệu "đã kết hôn nhưng lại khai
    đi tu"). Ghi vào `can-review-sau.md`.

### `isNull()` (`GxTanHien.cs:264-289`) — coi là "không có gì để lưu" khi TẤT CẢ các ô sau đều trống

9 ô ngày (`dtNgayBatDau`, `dtNgayVaoNhaTap`, `dtNgayVaoNhaThu`, `dtNgayVaoDaiChungVien`,
`dtNgayKhanLanDau`, `dtNgayKhanVinhVien`, `dtNgayPhoTe`, `dtNgayThuPhongLM`, `dtNgayBonMang`)
cộng 7 ô văn bản/combo (`txtNoiTu`, `txtDongTu`, `txtNoiPhucVu`, `cbChucVu`, `txtDiaChiPhucVu`,
`txtDienThoaiPhucVu`, `txtEmailPhucVu`, `txtGhiChu`). **Không tính** `chkHoiTuc` (checkbox "Đã
hồi tục") vào điều kiện rỗng — nghĩa là nếu người dùng CHỈ tick "Đã hồi tục" mà không nhập gì
khác, `isNull()` vẫn trả `true` (coi là rỗng) và **giá trị tick đó bị bỏ qua, không lưu được**.
Đây là lỗi/thiếu sót đáng chú ý — ghi vào `can-review-sau.md`.

### `UpdateData()` (`GxTanHien.cs:179-236`) — logic lưu

1. `checkInput()` thất bại → dừng, trả `false`.
2. Tải lại `GetData(maGiaoDan)` (truy vấn lại CSDL, không dùng dữ liệu đã tải lúc mở).
3. Có dòng đã tồn tại (`Rows.Count > 0`, dùng **`Rows[0]`** — vẫn kiểu "lấy bừa" như mục 3):
   - Nếu `isNull()` → **XOÁ CỨNG** dòng đó (`tblTanHien.Rows[0].Delete()`, `GxTanHien.cs:191`) —
     tức là: đã có bản ghi tận hiến, người dùng xoá hết nội dung trên form rồi bấm Cập nhật ⇒
     **toàn bộ bản ghi `TanHien` của giáo dân này bị xoá khỏi CSDL**. Không phải xoá mềm (không
     có cờ `DaXoa` trong 20 cột của bảng `TanHien`). Đây là hành vi mạnh — im lặng xoá vĩnh viễn
     chỉ vì để trống form — **ghi vào `can-review-sau.md`**.
   - Ngược lại → `AssignDataSource(row)` ghi đè trực tiếp lên dòng cũ (UPDATE).
4. Không có dòng nào tồn tại:
   - `isNull()` → không làm gì cả, trả `true` (coi như "lưu thành công" dù chẳng có gì để lưu).
   - Ngược lại → tạo dòng mới, sinh lại `maTanHien` một lần nữa qua `GetNextId(..., false)`
     (`GxTanHien.cs:203` — **sinh mã LẦN THỨ HAI**, đè lên giá trị đã sinh sẵn lúc
     `AssignControlData()` ở bước tải — hai lần gọi `GetNextId` cho cùng một lần thao tác có thể
     trả về hai mã khác nhau nếu có giao dịch khác xen giữa, dù trong luồng đơn giản thường trùng
     nhau; không có gì đảm bảo lần gọi thứ hai giữ nguyên giá trị lần đầu — mục 9).
5. `Memory.UpdateDataSet(ds)` — lỗi CSDL → `Memory.ShowError()` trả `true` (có lỗi) → hàm trả
   `false`. Không có lỗi tận hiến riêng cho trường hợp không tìm thấy giáo dân, chỉ có thông báo
   chung **"Cập nhật dữ liệu ơn gọi tận hiến không thành công"** (`GxTanHien.cs:226`) khi
   `GetData` trả về `null` (không phải khi rỗng — `null` khác `Rows.Count==0`).
6. Bắt `Exception` bao ngoài toàn bộ hàm → `MessageBox` tiêu đề **"Lỗi Exception (GxTanHien,
   UpdateData)"** kèm `ex.Message` (`GxTanHien.cs:233`) — bẫy lỗi hệ thống, không phải nghiệp vụ.

## 5. Thao tác người dùng

Không có nút riêng trên control này — được lưu gộp cùng lúc khi bấm "Cập nhật" ở `frmGiaoDan`:

```
if (updateDataGiaoDan())
{
    if (gxTanHien1.UpdateData())
        this.DialogResult = DialogResult.OK;
    else
        this.tabControl1.SelectedTab = tabOnGoi;   // nhảy về tab Ơn gọi nếu lưu thất bại
}
```
(`frmGiaoDan.cs:569-579`) — **Chỉ chạy `UpdateData()` sau khi `updateDataGiaoDan()` (lưu thông
tin cá nhân) thành công.** Nếu `UpdateData()` thất bại (ví dụ người dùng chọn "No"/"Cancel" ở hộp
thoại "đã có gia đình"), toàn bộ form **không đóng**, tự động chuyển về tab "Ơn gọi" — nhưng
thông tin cá nhân **đã được lưu rồi** (không rollback) vì `updateDataGiaoDan()` đã chạy trước và
thành công. Đây là hành vi lưu KHÔNG NGUYÊN TỬ giữa hai phần dữ liệu trên cùng một form — giống
nhận xét tương tự đã có ở `hon-phoi.md` cho `gxHonPhoi1`/`gxHistoryHoiDoan1`.

Khi chuyển sang tab "Ơn gọi" mà đang ở chế độ Thêm mới (`operation==ADD`) và giáo dân chưa lưu
(`!giaoDanDaLuu`), form hỏi trước:
> "Phải lưu thông tin cá nhân trước khi nhập thông tin ơn gọi. Bạn có muốn lưu thông tin cá nhân
> không?" (`frmGiaoDan.cs:1408`, Yes/No)
- Yes → gọi `updateDataGiaoDan()`; thất bại thì quay lại tab Cá nhân, huỷ việc chuyển tab.
- No → chuyển về tab Cá nhân (không cho vào tab Ơn gọi khi chưa lưu giáo dân).

## 6. Lưới dữ liệu

Không có — control chỉ có các ô nhập một bản ghi, không có lưới.

## 7. Liên kết sang màn hình khác

Không mở màn hình nào khác. Không có picker, không có nút "In".

## 8. Khác biệt cố ý ở bản web

- **GET trả về danh sách thay vì một bản ghi** — theo yêu cầu của người giao việc (đã dặn "một
  người có thể có nhiều giai đoạn"). Bản desktop chỉ hỗ trợ **một** dòng `TanHien`/giáo dân (mục
  3: lấy `Rows[0]` không `ORDER BY`, không có UI chọn giữa nhiều dòng). Bản web mở rộng thành
  danh sách đầy đủ CRUD (xem thêm, sửa từng bản ghi) — đây là điểm **cố ý làm rộng hơn** desktop,
  giống cách `hon-phoi.md` đã làm với Hôn phối, không phải sao chép y hệt hành vi "lấy bừa 1
  dòng". Xác nhận với người dùng ở `can-review-sau.md`.
- **KHÔNG migrate quy tắc "xoá cứng khi để trống form"** (mục 4, `Rows[0].Delete()`) — bản web để
  người dùng xoá một bản ghi tận hiến qua hành động xoá rõ ràng (nếu có), không suy luận ngầm từ
  "để trống hết rồi bấm Lưu". Ghi vào `can-review-sau.md` vì đây là một chỗ **cố ý không** làm y
  hệt (khác nguyên tắc chung của dự án) — cần người dùng xác nhận.
- **KHÔNG migrate quy tắc "chkHoiTuc bị bỏ qua trong `isNull()`"** — bản web coi tick "Đã hồi
  tục" là một giá trị có ý nghĩa như mọi trường khác (không bị `isNull()` bỏ qua). Ghi vào
  `can-review-sau.md`.
- **RowVersion chống ghi đè** cho từng bản ghi tận hiến — bản desktop không có cơ chế này.
- Danh sách "Chức vụ" giữ nguyên 7 giá trị của `cbChucVu` (mục 4) — không đổi.
- Không migrate hộp thoại "Giáo dân này đã có gia đình..." (mục 4) ở Task này — xem mục 9, cần
  quyết định có giữ cảnh báo này trên web hay bỏ hẳn.

## 9. Chỗ chưa chắc

- Ý nghĩa thật của hộp thoại "Giáo dân này đã có gia đình..." khi lưu Ơn gọi tận hiến — có phải
  cố ý cảnh báo mâu thuẫn dữ liệu (đã có gia đình nhưng lại khai đi tu) hay chỉ là đoạn code copy
  nhầm từ chỗ khác? Không đọc được ý đồ gốc chỉ từ mã nguồn.
- Nội dung câu SQL `SqlConstants.SELECT_GIAODAN_THEO_ID` — không đọc trong phạm vi 2 file được
  giao (nhưng suy đoán hợp lý là `SELECT * FROM GiaoDan WHERE MaGiaoDan=?`).
  chưa đọc lớp `SqlConstants.cs`.
- Ai/khi nào một giáo dân thật sự có nhiều hơn 1 dòng `TanHien` trong dữ liệu Access gốc (nếu
  có) — chưa có dữ liệu thật để kiểm chứng (bảng `TanHien` rỗng ở CSDL mẫu `qlgx_thu`).
- Có nơi nào khác trong `Source/` tạo/xoá dòng `TanHien` ngoài `GxTanHien.cs` không (ví dụ màn
  hình danh sách tận hiến riêng, tương tự `GxListHoiDoan`/`frmHoiDoanList` cho Hội đoàn) — chưa
  tìm thấy trong phạm vi được giao, có thể có mà chưa đọc tới.

## 10. Đối chiếu bản web hiện tại (điền sau khi migrate xong)

| Hành vi bản desktop | Bản web đã có? | Ghi chú |
|---|---|---|
| Xem/sửa một bản ghi tận hiến gắn với giáo dân | *(điền khi migrate xong)* | |
| Tạo mới khi chưa có bản ghi nào | | |
| Xoá cứng khi để trống hết form (mục 4) | Cố ý KHÔNG migrate | Xem mục 8, `can-review-sau.md` |
| Hộp thoại "đã có gia đình..." khi lưu | Cố ý KHÔNG migrate ở Task này | Xem mục 8, 9 |
| `chkHoiTuc` bị `isNull()` bỏ qua | Cố ý KHÔNG migrate | Xem mục 8 |
