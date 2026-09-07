# Màn hình: Danh sách rao hôn phối (`frmRaoHonPhoiList.cs` + `frmRaoHonPhoi.cs`)

| | |
|---|---|
| Tệp nguồn | `Source/ChuongTrinh/frmRaoHonPhoiList.cs` (177 dòng) + `.Designer.cs`; `Source/GXControl/frmRaoHonPhoi.cs` (637 dòng) + `.Designer.cs`; `Source/GXControl/GxRaoHonPhoiList.cs` (461 dòng) |
| UserControl dùng lại | `GxRaoHonPhoiList` (lưới), `GxGiaoDan` (`txtNguoi1`/`txtNguoi2`, picker chọn người), `GxComboField` (`cbChaGui`), `GxDateField` (`dtNgayRao1/2/3`), `GxTextField` (các ô Giáo xứ/Giáo phận), `GxAddEdit`, `GxCommand` |
| Bảng dữ liệu đụng tới | `RaoHonPhoi` (26 cột) |
| Trạng thái migrate | một phần — xem mục 10. Bảng **rỗng** ở giáo xứ khảo sát (Vô Nhiễm) — test tự động + kiểm thử tạo/xoá dữ liệu mẫu qua chính chức năng là bằng chứng chính cho màn hình này |

## 1. Mục đích

Ghi "tờ rao hôn phối" — thủ tục Công giáo công bố ý định kết hôn của một đôi trước giáo dân ba
tuần liên tiếp (Rao lần 1/2/3) trước khi cử hành hôn phối chính thức (bảng `HonPhoi` riêng,
xem `hon-phoi.md`). Đồng thời có thể xuất "tờ điều tra hôn phối" và "kết quả rao hôn phối" ra
Excel để nộp/lưu hồ sơ giáo xứ (`ExcelReport.ReportRaoHP`, không migrate ở Task này — mục 8).

## 2. Bố cục và các trường

`frmRaoHonPhoi` có hai khối riêng biệt, chỉ khối dưới lưu vào bảng `RaoHonPhoi`:

### 2.1 Khối trên (`uiGroupBox1`) — chỉ hiện khi in ("In điều tra"/"In kết quả rao"), KHÔNG lưu vào CSDL

| Nhãn hiển thị thật | Control | Dùng để | Ghi chú |
|---|---|---|---|
| "Kính gửi cha xứ:" | `txtChaNhan` (`GxLinhMuc`) | Xuất báo cáo (`TenLinhMucNhan`) **và** lưu vào `RaoHonPhoi.LinhMucNhan` (`AssignDataSource`, dòng 404) | Tên biến gợi ý "nhận" nhưng nhãn hiển thị là "Kính gửi cha xứ:" |
| "Giáo phận:" | `txtGiaoXuNhan` (`GxTextField`) | Lưu vào `RaoHonPhoi.GiaoXuNhan` (dòng 401) | **Tên biến/tên cột đều nói "Giáo xứ nhận" nhưng nhãn hiển thị thật trên form là "Giáo phận:"** — lệch tên/nhãn, chép lại đúng hiện trạng, không tự "sửa cho đúng" |

Chỉ bắt buộc nhập khi `UsePrint=true` (đang thực hiện "In điều tra"/"In kết quả rao", xem mục
4) — nếu chỉ "Cập nhật" thường (không in) thì hai ô này không bị kiểm tra.

### 2.2 Khối dưới (`gxGroupBox1`) — lưu vào bảng `RaoHonPhoi`

| Nhãn hiển thị | Control | Cột CSDL | Bắt buộc? | Ghi chú |
|---|---|---|---|---|
| Đôi rao | `txtDoiRao` | `TenRaoHonPhoi` | có | Tự gợi ý "Tên1 - Tên2" khi vừa chọn xong Người thứ nhất/thứ hai lúc **Thêm mới** (`txtNguoi1_OnSelected`/`txtNguoi2_OnSelected`, dòng 489-494, 536-541) — không tự ghi đè khi đang Sửa |
| Người thứ nhất | `txtNguoi1` (`GxGiaoDan`, picker) | `MaGiaoDan1` | có | Chọn xong tự điền Giáo xứ/Giáo phận/Xứ trước của người 1 (mục 4); thu hẹp picker Người thứ hai theo GIỚI TÍNH ĐỐI LẬP |
| Người thứ hai | `txtNguoi2` (`GxGiaoDan`) | `MaGiaoDan2` | có | Đối xứng với Người thứ nhất |
| Hiện ở giáo xứ (người 1) | `txtGiaoXu1` | `GiaoXu1` | không | Tự điền khi chọn Người thứ nhất — xem mục 4 |
| Giáo phận (người 1) | `txtGiaoPhan1` | `GiaoPhan1` | không | Tự điền, có thể bị ép về giáo phận CỦA GIÁO XỨ HIỆN TẠI nếu người đó thuộc một giáo họ trong xứ |
| Trước đã ở xứ (người 1) | `txtGiaoXuTruoc1` | `GiaoXuTruoc1` | không | Tự điền từ lịch sử `ChuyenXu` (loại "Chuyển đến") nếu có |
| Giáo phận (xứ trước, người 1) | `txtGiaoPhanTruoc1` | `GiaoPhanTruoc1` | không | Không thấy code tự điền tương ứng cho ô này (chỉ `GiaoXuTruoc1` được tự điền, mục 9) |
| (tương tự 4 ô trên cho người 2) | `txtGiaoXu2/txtGiaoPhan2/txtGiaoXuTruoc2/txtGiaoPhanTruoc2` | `GiaoXu2/GiaoPhan2/GiaoXuTruoc2/GiaoPhanTruoc2` | không | |
| Ngày rao lần 1 | `dtNgayRao1` | `NgayRaoLan1` | **có khi lưu**, xem mục 4 | |
| rao lần 2 | `dtNgayRao2` | `NgayRaoLan2` | **có khi lưu** | Nhãn thật thiếu chữ "Ngày" so với lần 1 (Designer: "rao lần 2:") |
| rao lần 3 | `dtNgayRao3` | `NgayRaoLan3` | **có khi lưu** | Nhãn thật thiếu chữ "Ngày", giống lần 2 |
| Linh mục chứng | `cbChaGui` (`GxComboField`, danh sách Linh mục đang tại nhiệm — `DenNgay IS NULL`, dòng 15-27) | **KHÔNG lưu cột nào** | không | Chỉ dùng tức thời cho báo cáo Excel (`TenLinhMucGui`, dòng 325) khi in — chọn xong rồi Cập nhật (không in) thì **giá trị bị BỎ, không có nơi lưu trong `RaoHonPhoi`** — xem mục 9 |
| Ghi chú | `txtGhiChu` | `GhiChu` | không | |
| Nguyên quán xứ (người 1) | `txtGiaoXuNQ1` | `GiaoXuNQ1` | không | |
| Giáo phận (nguyên quán, người 1) | `txtGiaoPhanNQ1` | `GiaoPhanNQ1` | không | |
| (tương tự cho người 2) | `txtGiaoXuNQ2/txtGiaoPhanNQ2` | `GiaoXuNQ2/GiaoPhanNQ2` | không | |

Ba cột `Tam1/Tam2/Tam3` tồn tại trong bảng `RaoHonPhoi` (định nghĩa trong `GxConstants.cs`)
nhưng **KHÔNG tìm thấy control/nơi đọc-ghi nào trong `frmRaoHonPhoi.cs`** — có vẻ là cột Access
cũ không còn dùng ở bản UI hiện tại. Bản web đã thêm 3 ô nhập tự do "Tạm 1/2/3" cho các cột
này để không mất khả năng xem/sửa dữ liệu cũ nếu giáo xứ khác có ghi gì vào đó — đây là bổ sung
**KHÔNG có trên desktop**, ghi vào `can-review-sau.md` để người dùng xác nhận có cần giữ hay ẩn
hẳn.

### 2.3 Lưới `GxRaoHonPhoiList` (danh sách) — đúng 7 cột, thứ tự (`GxRaoHonPhoiList.cs:105-163`)

| Cột | Nhãn | Rộng | Định dạng |
|---|---|---|---|
| `MaRaoHonPhoi` | Mã rao | 50 | số, canh phải |
| `TenRaoHonPhoi` | Đôi rao | 100 | text |
| `Nguoi1` | Người thứ nhất | 150 | text (tên gõ/lưu tại thời điểm chọn qua picker desktop; bản web lấy trực tiếp `GiaoDan1.HoTen`) |
| `Nguoi2` | Người thứ hai | 150 | text |
| `NgayRaoLan1` | Rao lần 1 | 80 | `dd/MM/yyyy` |
| `NgayRaoLan2` | Rao lần 2 | 80 | `dd/MM/yyyy` |
| `NgayRaoLan3` | Rao lần 3 | 80 | `dd/MM/yyyy` |
| `GhiChu` | Ghi chú | 150 | text |

## 3. Hành vi khi tải

- `frmRaoHonPhoiList`: combo `cbOption` có 2 mục, mặc định chọn mục 0 = **"Chỉ xem những đôi
  rao chưa hoàn tất"** (`SelectedIndex = 0`, dòng 75); mục 1 = "Xem tất cả".
- `GxRaoHonPhoiList.LoadData()` (dòng 166-181):
  - Chưa hoàn tất (`ShowAll=false`): lọc `Int(Right(NgayRaoLan3,4) & Mid(NgayRaoLan3,4,2) &
    Left(NgayRaoLan3,2)) >= yyyyMMdd(hôm nay)`, tức **Rao lần 3 còn trong tương lai (hoặc hôm
    nay)** mới hiện; sắp theo `NgayRaoLan3` TĂNG DẦN. Với `NgayRaoLan3` RỖNG, biểu thức Access
    ghép chuỗi trên một giá trị rỗng — hành vi chính xác **chưa xác nhận được từ mã đọc được**
    (có thể lỗi hoặc trả chuỗi rỗng/0) — xem mục 9. Bản web coi `NgayRaoLan3` NULL là "chưa
    hoàn tất" (giữ lại trong danh sách mặc định) — đây là một LỰA CHỌN có ý thức khi không chắc
    hành vi gốc, không phải khẳng định đã khớp 100%; ghi vào `can-review-sau.md`.
  - Xem tất cả (`ShowAll=true`): không lọc, sắp theo `NgayRaoLan1` TĂNG DẦN.
- `gxRaoHonPhoiList1_RowCountChanged`/`LoadDataFinished`: label chân lưới hiện "{n} đôi rao"
  hoặc "{n} giáo dân" tuỳ sự kiện nào bắn sau cùng (có vẻ là hai chữ khác nhau cho cùng một số —
  chưa xác nhận có bug hiển thị hay không, mục 9).

## 4. Quy tắc nghiệp vụ và kiểm tra dữ liệu

### Khi chọn Người thứ nhất/thứ hai (`txtNguoiX_OnSelected`, dòng 453-541)

- Chọn Người thứ nhất → tự điền Giáo xứ1/Giáo phận1 từ hồ sơ giáo dân
  (`Memory.GetThuocXu`/`GetThuocGiaoPhan`); nếu người đó thuộc một Giáo họ CỦA GIÁO XỨ HIỆN TẠI
  (`MaGiaoHo > 0`) thì Giáo phận1 bị **ép về giáo phận của giáo xứ hiện tại**
  (`Memory.GiaoXuInfo`), bất kể `GetThuocGiaoPhan` trả gì. Tự điền "Xứ trước" từ lịch sử
  `ChuyenXu` loại "Chuyển đến" gần nhất, nếu có.
- Chọn Người thứ nhất giới tính X → **thu hẹp picker Người thứ hai chỉ còn giới tính đối lập**
  (`txtNguoi2.WhereSQL = " AND Phai=... "`) — và ngược lại khi chọn Người thứ hai trước. Đây là
  ràng buộc MỘT NAM MỘT NỮ, thực hiện bằng cách giới hạn kết quả tìm kiếm chứ không phải kiểm
  tra sau khi chọn.
- Khi đang **Thêm mới**: mỗi lần chọn xong một trong hai người, `txtDoiRao` được VIẾT ĐÈ thành
  "Tên1 - Tên2" (ghép `Memory.GetName`) — người dùng gõ tay Đôi rao trước đó sẽ bị mất nếu sau
  đó đổi lại Người thứ nhất/thứ hai. Khi đang **Sửa**, không tự ghi đè `txtDoiRao`.

### Khi lưu (`gxCommand1_OnOK`, dòng 97-175) — thứ tự kiểm tra, dừng ở lỗi đầu

1. **Chỉ khi `UsePrint=true`** (đang "In điều tra"/"In kết quả rao", không áp dụng cho "Cập
   nhật" thường): "Kính gửi cha xứ" trống → **"Hãy nhập cha nhận điều tra hôn phối!"** (dòng
   101), focus lại ô.
2. **Chỉ khi `UsePrint=true`**: "Giáo phận" (nhãn thật của `txtGiaoXuNhan`) trống →
   **"Hãy nhập giáo xứ nhận!"** (dòng 108).
3. Chưa chọn Người thứ nhất (`txtNguoi1.CurrentRow == null`) → **"Xin vui lòng nhập thông tin
   người thứ nhất cần rao"** (dòng 115).
4. Đôi rao trống → **"Xin vui lòng nhập [đôi rao]"** (dòng 121).
5. Chưa chọn Người thứ hai → **"Xin vui lòng nhập thông tin người thứ hai cần rao"** (dòng 127).
6. Ngày rao lần 1 **không hợp lệ/chưa nhập ĐỦ ngày-tháng-năm** (`isValidDate`, dòng 177-185,
   đòi cả ba phần Day/Month/Year đều khác rỗng VÀ hợp lệ) → **"Xin vui lòng kiểm tra lại ngày
   rao lần thứ nhất"** (dòng 133).
7. Tương tự cho Ngày rao lần 2 → **"...lần thứ hai"** (dòng 140).
8. Tương tự cho Ngày rao lần 3 → **"...lần thứ ba"** (dòng 147).

**Quy tắc kỳ quặc đáng chú ý:** mặc dù ba cột `NgayRaoLan1/2/3` trong CSDL là **nullable**
(hôn phối thường rao dần từng tuần, ba tuần liên tiếp — dữ liệu thực tế sẽ có lúc chỉ mới có
Rao lần 1, chưa tới Rao lần 2/3), `checkInput()` của `frmRaoHonPhoi` **BẮT BUỘC CẢ BA NGÀY phải
đầy đủ và hợp lệ mới cho lưu** — không có cách nào lưu một đôi rao mới chỉ với Rao lần 1. Đây
rất có thể là **thiết kế sai/quá chặt** của bản gốc (không khớp thực tế nghiệp vụ rao ba tuần
liên tiếp), nhưng theo đúng nguyên tắc "migrate y hệt kể cả chỗ sai", *bản web KHÔNG chép lại
y hệt* — xem lý do ở mục 8 (đây LÀ một trường hợp cân nhắc rõ ràng, không phải im lặng sửa)
và **ghi rõ vào `can-review-sau.md`** để người dùng quyết định giữ nguyên chặn cứng hay không.

### Lưu (`UpdateData`, dòng 351-386 + `AssignDataSource`, dòng 388-415)

- Thêm mới: sinh `MaRaoHonPhoi` kế tiếp qua `Memory.Instance.GetNextId(..., true)` (dòng 365).
- Gán toàn bộ 21 trường ở mục 2.2 (trừ `Tam1/2/3`, không được đọc/ghi ở đâu) vào `DataRow` rồi
  `Memory.UpdateDataSet`.
- Lỗi ngoại lệ khi lưu → `MessageBox` tiêu đề chung **"Lỗi Exception"** (không ghi rõ tên hàm,
  khác các form khác) kèm `ex.Message` (dòng 346).

### Xoá (`GxRaoHonPhoiList.DeleteRow`, dòng 242-265)

- **"Bạn có chắc muốn xóa (các) đôi rao được chọn trong danh sách?"** (Yes/No).
- Xoá CỨNG từng dòng đã chọn qua `SqlConstants.DELETE_RAOHONPHOI` — không có bước xoá mềm.

## 5. Thao tác người dùng

| Thao tác | Điều kiện bật/tắt | Hành động |
|---|---|---|
| Thêm đôi rao (`gxAddEdit1_AddClick`) | luôn bật | Mở `frmRaoHonPhoi` (`Operation=ADD`, `UsePrint=false`) |
| Sửa / nhấp đúp dòng | cần chọn dòng | Mở `frmRaoHonPhoi` (`Operation=EDIT`, `UsePrint=false`) |
| Xoá | cần chọn dòng | Xem mục 4 |
| "In điều &tra" (`gxAddEdit1.Button1`) | | Mở `frmRaoHonPhoi` với `UsePrint=true, PrintRs=false` — bắt buộc thêm 2 ô đầu mục 2.1, xuất "tờ điều tra" ra Excel |
| "In kết quả &rao hôn phối" (`gxAddEdit1.Button2`) | | Mở `frmRaoHonPhoi` với `UsePrint=true, PrintRs=true` — xuất "kết quả rao" ra Excel |
| "In danh &sách giáo dân" (`gxAddEdit1.PrintButton`) | | `gxRaoHonPhoiList1.Print()` — hộp thoại chọn "ngày rao" rồi xuất Excel toàn bộ danh sách đang đủ điều kiện rao vào ngày đó (bảng tạm `RaoHonPhoiTMP`, xem `getLanRao`/`isPrinted` — trong khoảng 0-6 ngày kể từ ngày rao được lưu) |
| Đổi combo "Hiển thị" | | Tải lại lưới theo `ShowAll` |
| Chuột phải trên lưới | | "Xem - &sửa" / "&Xóa" / "&In điều tra hôn phối" / "&In kết quả rao hôn phối" (`GxRaoHonPhoiList.cs:50-67`) |

## 6. Lưới dữ liệu

Xem mục 2.3. Không có tô màu/gạch ngang điều kiện.

## 7. Liên kết sang màn hình khác

- Mở `frmRaoHonPhoi` (Thêm/Sửa/In) — trả `DataReturn` là DataRow đôi rao vừa lưu.
- Chọn Người thứ nhất/thứ hai qua `GxGiaoDan` (picker tự quản lý, không mở form riêng).
- Xuất Excel qua `ExcelReport.ReportRaoHP` (`Export`/`ExportList`) — không mở màn hình, ghi
  file ngoài.

## 8. Khác biệt cố ý ở bản web

- **Không migrate khối "In điều tra"/"In kết quả rao"/"In danh sách"** (toàn bộ `UsePrint`,
  `cbChaGui`, xuất Excel `ReportRaoHP`) — chưa có tính năng in/xuất báo cáo dạng này ở bản web
  Task này. Vì vậy hai trường `txtChaNhan`/`txtGiaoXuNhan` (mục 2.1) chỉ còn ý nghĩa như hai ô
  nhập tự do lưu vào `LinhMucNhan`/`GiaoXuNhan`, KHÔNG có luồng bắt buộc nhập khi in (vì không
  có luồng in) — bản web đặt tên trường tiếng Việt trực diện hơn ("Cha nhận điều tra"/"Giáo xứ
  nhận") thay vì giữ nguyên nhãn gốc "Kính gửi cha xứ:"/"Giáo phận:" (vốn đã lệch tên biến từ
  desktop) — ghi rõ để người dùng biết nhãn đã đổi, không phải cùng chữ với bản cũ.
  `cbChaGui` ("Linh mục chứng") **không migrate** vì giá trị của nó trên desktop cũng không hề
  được lưu vào bảng `RaoHonPhoi` (mục 2.2) — bỏ qua không mất dữ liệu gì.
- **KHÔNG chép lại quy tắc "bắt buộc đủ cả 3 ngày rao mới cho lưu"** (mục 4) — ba trường
  `NgayRaoLan1/2/3` trên web là tuỳ chọn, khớp đúng ràng buộc CSDL (`DateOnly?`) và thực tế
  nghiệp vụ (rao dần theo tuần). Đây là lựa chọn CÓ Ý THỨC khác nguyên tắc "migrate y hệt kể cả
  chỗ sai" mặc định của dự án — ghi rõ vào `can-review-sau.md` để người dùng xác nhận lại, vì
  giữ nguyên chặn cứng sẽ khiến người dùng không tạo được đôi rao ngay từ tuần đầu tiên (chưa
  thể nhập trước Rao lần 2/3, theo đúng cách thực tế vẫn dùng).
- **Không tự động điền Giáo xứ/Giáo phận/Xứ trước khi chọn Người thứ nhất/thứ hai** (mục 4) —
  các trường này vẫn tồn tại và sửa được thủ công trên web, nhưng không có gợi ý tự động từ hồ
  sơ giáo dân/lịch sử chuyển xứ. Ghi vào `can-review-sau.md` — có thể bổ sung sau nếu cần.
- **Không tự giới hạn giới tính đối lập khi chọn Người thứ nhất/thứ hai** — `GxPicker` dùng
  chung cho toàn hệ thống, tìm theo tên/mã, không lọc theo giới tính của người còn lại. Ghi vào
  `can-review-sau.md`.
- **Không tự gợi ý "Đôi rao" = "Tên1 - Tên2"** khi chọn xong hai người — người dùng tự gõ trường
  "Đôi rao". Ghi vào `can-review-sau.md`.
- Thêm 3 ô "Tạm 1/2/3" cho các cột `Tam1/2/3` (mục 2.2) — desktop không có UI cho các cột này.
- **RowVersion chống ghi đè** khi sửa — bản desktop không có cơ chế này.
- Lỗi "Không tìm thấy dòng nào khớp điều kiện lọc" hiện bằng chữ trung tính của `GxGrid` dùng
  chung toàn hệ thống thay vì chữ "Chưa có dữ liệu" riêng — nhất quán với các lưới khác.

## 9. Chỗ chưa chắc

- Hành vi chính xác của biểu thức lọc "chưa hoàn tất" (`GxRaoHonPhoiList.LoadData`, dòng
  172-175) khi `NgayRaoLan3` rỗng — biểu thức Access ghép chuỗi `Right/Mid/Left` trên giá trị
  rỗng chưa được kiểm chứng bằng cách chạy thật trên Access; bản web coi NULL là "chưa hoàn
  tất" theo suy luận hợp lý nhất, không phải xác nhận trực tiếp.
  Ghi ở `can-review-sau.md`.
- `txtGiaoPhanTruoc1/2` không thấy có logic tự điền tương ứng (khác `txtGiaoXuTruoc1/2` được tự
  điền từ `ChuyenXu`) — có thể do đọc thiếu một đoạn code khác, hoặc đúng là bản gốc chỉ tự điền
  một nửa cặp trường "Xứ trước"/"Giáo phận trước".
  chưa đọc `GxLinhMuc.cs` để biết đủ hành vi.
- Vì sao `gxRaoHonPhoiList1_RowCountChanged` và `_LoadDataFinished` hiện hai nhãn khác nhau
  ("đôi rao" và "giáo dân") cho cùng một con số dòng trên lưới rao hôn phối — có thể là lỗi
  copy-paste từ màn hình giáo dân, chưa xác nhận được thứ tự sự kiện thật nào thắng.
- Ý nghĩa nghiệp vụ thật của các cột `Tam1/Tam2/Tam3` — không tìm thấy bất kỳ tham chiếu nào
  trong `frmRaoHonPhoi.cs`/`GxRaoHonPhoiList.cs`, nghi là cột Access cũ không còn dùng.
- Điểm gọi mở `frmRaoHonPhoiList` từ menu chính — ngoài phạm vi tệp được giao.

## 10. Đối chiếu bản web hiện tại (sau khi migrate)

| Hành vi bản desktop | Bản web đã có? | Ghi chú |
|---|---|---|
| Danh sách với combo "chưa hoàn tất"/"tất cả" | **Có** | `GET /api/rao-hon-phoi?xemTatCa=` — đã kiểm tra thật: đôi rao có Rao lần 3 trong quá khứ không hiện mặc định, hiện khi "Xem tất cả" |
| Đầy đủ 26 cột (kể cả Tam1-3, GiaoXuNQ...) | **Có** | `GET/POST/PUT /api/rao-hon-phoi` — đã tạo/đọc/sửa/xoá qua trình duyệt thật với đủ trường, xác nhận đọc lại đúng dữ liệu đã nhập |
| Chọn Người thứ nhất/thứ hai qua picker | **Có** | `GxPicker`, tìm theo tên thật — đã thử với 2 giáo dân thật (#493, #602) |
| 3 kiểm tra bắt buộc đầu (Người 1/Đôi rao/Người 2) | **Có** | Thông báo tiếng Việt y hệt bản gốc, có test |
| Bắt buộc đủ cả 3 ngày rao hợp lệ mới lưu | **Cố ý KHÔNG chép** (mục 8) | Ba ngày rao là tuỳ chọn trên web |
| Tự điền Giáo xứ/Giáo phận/Xứ trước khi chọn người | **Thiếu** (cố ý, mục 8) | |
| Giới hạn giới tính đối lập giữa hai người | **Thiếu** (cố ý, mục 8) | |
| Tự gợi ý "Đôi rao" = Tên1 - Tên2 | **Thiếu** (cố ý, mục 8) | |
| In điều tra / In kết quả rao / In danh sách | **Thiếu** | Chưa có hạ tầng xuất báo cáo dạng bảng tạm + Excel như `ReportRaoHP` |
| Xoá cứng | **Có** | `DELETE /api/rao-hon-phoi/{id}` |
