# Hỗ trợ nhập liệu — control ngày tháng, tự nhảy ô, gợi ý theo tần suất

| | |
|---|---|
| Loại tài liệu | Đặc tả **xuyên màn hình** (không phải một `frmX.cs` cụ thể) — control dùng chung |
| Tệp nguồn chính | `Source/GXControl/GxDateInput.cs` (633 dòng), `GxDateField.cs` (192 dòng), `GxMaskInput.cs`, `GxDayMonthField.cs`, `GxTextField.cs`, `GxBaseField.cs`, `GxTenThanh.cs`, `GxLinhMuc.cs` |
| Hàm tiện ích | `Source/DBAccess/CMemory.cs` (lớp `Memory`) — `GetDateString`, `SplitDatePart`, `GetDateFromString`, `CompareTwoStringDate`, `AutoUpperFirstChar` |
| Bảng dữ liệu đụng tới | `DuLieuChung` (Access) → `du_lieu_chung` (PostgreSQL, đã có 343 dòng); tệp cục bộ `autocomplete.xml` (không có ở bản web) |
| Trạng thái migrate | chưa (đây là bước nghiên cứu + đề xuất trước khi làm) |
| Vì sao viết | Yêu cầu trực tiếp của người chủ dự án — coi đây là "điểm đặc biệt làm nên giá trị của phần mềm desktop". Xem trích dẫn nguyên văn trong lịch sử trao đổi. |

## Tóm tắt cho người đọc vội

Câu hỏi quan trọng nhất: **khi người dùng nhập thiếu ngày tháng (ví dụ chỉ gõ "1985"), bản
desktop lưu xuống Access cái gì?**

Trả lời: lưu **đúng nguyên chuỗi thiếu đó** — `"1985"` chứ **không** tự điền thành
`"01/01/1985"`. Bằng chứng: `Memory.GetDateString(day, month, year)` chỉ nối thêm phần
`day`/`month` vào chuỗi kết quả **nếu người dùng đã nhập phần đó**, còn lại chỉ có `year` là
bắt buộc (`Source/DBAccess/CMemory.cs:755-763`), và `frmGiaoDan.cs:963` gán thẳng
`dtNgaySinh.Value` (chuỗi này) vào `DataRow` để ghi xuống Access — không có bước chuẩn hoá nào ở
giữa. Đây là phát hiện **ngược lại** với giả thiết ban đầu của người dùng rằng desktop "tự fill
01/01" — desktop **cố ý giữ nguyên độ thiếu**, và toàn bộ các hàm so sánh/kiểm tra ngày tháng
trong `Memory` đều được viết để xử lý đúng ngày-tháng-thiếu đó (xem mục A dưới).

Đây cũng chính là lý do 207 bản ghi thật trong `qlgx_thu` rơi vào `du_lieu_loi`: hàm đọc dữ
liệu Access phía web (`WebApp/src/Qlgx.Data/NgayThangText.cs:6-8`) viết trong doc-comment rằng
*"Bản Access lưu mọi ngày dưới dạng chuỗi dd/MM/yyyy"* — giả thiết đó **sai** so với những gì
mã nguồn desktop thực sự làm.

## A. Control nhập ngày tháng

### A.1 Cấu tạo — ba ô riêng, không phải một ô có mask

`GxDateField` (`Source/GXControl/GxDateField.cs:12-24`) chỉ là lớp bọc ngoài
(`GxBaseField`), bên trong dùng `GxDateInput` (`GxDateField.cs:22,36-39`).

`GxDateInput` **không phải** một ô văn bản có mask kiểu `__/__/____`, mà là **ba `TextBox` con
riêng biệt** cộng hai `Label` dấu `/` ở giữa, khai báo trong
`Source/GXControl/GxDateInput.Designer.cs`:

- `txtDay`: `MaxLength = 2`, rộng 16px, canh phải (dòng 80-87)
- `label1`: `Text = "/"` (dòng 108-114)
- `txtMonth`: `MaxLength = 2`, rộng 16px, canh phải (dòng 62-69)
- `label2`: `Text = "/"` (dòng 98-104)
- `txtYear`: `MaxLength = 4`, rộng 27px, canh phải (dòng 42-49)

Giá trị `Text = "05"/"05"/"2009"` ở Designer chỉ là dữ liệu mẫu lúc thiết kế (design-time),
**không phải placeholder hiển thị lúc chạy**. Lúc chạy, ô trống thì các `TextBox` con **trống
trơn** (không có chữ gạch dưới `__` nào) — `IsNullDate` được định nghĩa là cả ba ô đều rỗng
(`GxDateInput.cs:84-97`). **Đây là điểm khác với mô tả của người dùng** (họ nhớ là có hiện sẵn
`__/__/____`) — mã nguồn không cho thấy placeholder đó tồn tại trong `GxDateInput`. Có thể trí
nhớ người dùng lẫn với hành vi hiển thị khác (ví dụ ô đã có sẵn dữ liệu mẫu lúc thiết kế); ghi
vào mục "Chỗ chưa chắc".

Ngoài ba ô, control còn có một `dateTimePicker1` (lịch xổ xuống) dùng để **chọn nhanh ngày đầy
đủ bằng chuột** — khi mở lịch, nó nạp giá trị hiện tại qua `Memory.GetDateFromString(...)`
(`GxDateInput.cs:497-509`), và khi chọn ngày trên lịch thì gán ngược lại `Value`
(`GxDateInput.cs:491-495`). Lịch này **luôn cho ngày đầy đủ** — không dùng được để nhập ngày
thiếu.

### A.2 Nhập liên tục không cần gõ dấu `/`

Cơ chế tự nhảy nằm trong `KeyPress` của từng ô (`GxDateInput.cs:430-489`):

- `txtDay_KeyPress` (dòng 430-438): nếu ô ngày đã có 1 chữ số và người dùng gõ tiếp một chữ số
  nữa, tự nối số đó vào `txtDay.Text` rồi `txtMonth.Focus()` — tức gõ đủ 2 số là tự nhảy sang ô
  tháng, không cần gõ `/`.
- `txtMonth_KeyPress` (dòng 440-448): y hệt, nhảy sang `txtYear` khi đủ 2 số.
- `txtYear_KeyPress` (dòng 450-489): khi đã gõ 3 số và gõ thêm số thứ 4 (hoặc gõ Tab), nối số
  vào `txtYear.Text`, rồi kiểm tra: nếu có ngày mà chưa có tháng thì báo lỗi
  "Hãy nhập tháng." và focus vào `txtMonth`; nếu đủ cả ba mà không hợp lệ thì báo
  "Hãy nhập ngày/tháng/năm hợp lệ." và focus lại `txtDay`; nếu hợp lệ thì gọi
  `this.FindForm().GetNextControl(this, true)` để nhảy sang **control tiếp theo trên form**
  (không chỉ trong nội bộ ô ngày tháng — nhảy hẳn ra control kế tiếp theo `TabIndex`).

Vậy ví dụ gõ liên tục `01021985`: `0`→`1` vào `txtDay` rồi tự nhảy `txtMonth`, `0`→`2` vào
`txtMonth` rồi tự nhảy `txtYear`, rồi gõ `1985` (4 số) — số thứ 4 kích hoạt luôn việc kiểm tra
và nhảy control tiếp theo. Khớp với mô tả của người dùng.

`txtDay.SelectAll()` / `txtMonth.SelectAll()` / `txtYear.SelectAll()` được gọi khi `Enter` vào
từng ô (`GxDateInput.cs:391-413`) — nghĩa là khi người dùng bấm chuột/Tab vào một ô đã có sẵn số,
toàn bộ số đó được bôi đen, gõ số mới sẽ **ghi đè** chứ không chèn thêm. Đồng thời
`ImeMode = ImeMode.Off` được ép cho cả ba ô (cùng dòng) để bàn phím gõ tiếng Việt (nếu đang bật)
không nuốt mất phím số.

### A.3 Focus thẳng vào ô tháng hoặc ô năm

Người dùng có thể bấm chuột hoặc Tab thẳng vào `txtMonth` hay `txtYear` rồi gõ luôn — không có
đoạn mã nào ép quay lại `txtDay` trước, ngoại trừ khi **cả control `GxDateInput` nhận focus từ
control khác** (ví dụ Tab từ control trước đó vào) thì `OnGotFocus` ép `txtDay.Focus()`
(`GxDateInput.cs:362-365`), và khi click chuột vào vùng nền control (không trúng ô nào) thì cũng
về `txtDay` (`textBox1_MouseClick`, dòng 611-615). Nhưng nếu người dùng bấm chuột **trực tiếp**
vào `txtMonth` hoặc `txtYear`, `Enter` event của chính ô đó xảy ra bình thường và focus ở lại ô
được bấm.

### A.4 Nhập thiếu — quy tắc kiểm tra khi rời ô

`CheckInput(bool isShowMsg)` (`GxDateInput.cs:192-246`) là nơi giữ quy tắc:

- Nếu `day != ""` thì **bắt buộc** `month != ""` (nếu không: MessageBox
  `"Hãy nhập tháng."`, dòng 204-206) và **bắt buộc** `year != ""` (nếu không: MessageBox
  `"Hãy nhập năm."`, dòng 213-215).
- Nếu `month != "" && year == ""`: báo `"Hãy nhập năm."` (dòng 221-227).
- Nếu đủ cả ba mà không phải ngày hợp lệ (`Validator.IsDate`): báo
  `"Hãy nhập ngày/tháng/năm hợp lệ."` (dòng 229-236).
- Nếu `fullInputRequired == true` và còn thiếu bất kỳ phần nào: báo
  `"Hãy nhập đầy đủ ngày/tháng/năm."` (dòng 238-243) — **nhưng mặc định `fullInputRequired`
  luôn là `false`** cho mọi ô ngày tháng ở màn hình Chi tiết giáo dân (xem danh sách dưới),
  nên trong thực tế người dùng luôn được phép nhập thiếu.

→ **Kết luận: chỉ cần năm** (không tháng không ngày) **hoặc tháng+năm** là hợp lệ; **ngày một
mình không hợp lệ** — có ngày thì bắt buộc kèm tháng và năm. Đúng như mô tả của người dùng
("chỉ có năm, hoặc tháng năm").

Xác nhận `FullInputRequired = false` cho toàn bộ 12 ô ngày tháng trong màn hình Chi tiết giáo
dân, `Source/GXControl/frmGiaoDan.Designer.cs`: dòng 337 (`dtNgayQuaDoi`), 568 (`dtNgayChuyen`),
718 (`dtNgayXucDau`), 769 (`dtNgayThemSuc`), 903 (`dtNgayRuaToi`), 989 (`dtNgayRuocLe`), 1290
(`dtNgaySinh`), 1434 (`dtGLHNDenNgay`), 1453 (`dtGLHNTuNgay`), 1512 (`dtNgayVaoDoi`), 1572
(`dtNgayBD2`), 1621 (`dtNgayBD1`).

### A.5 Giá trị cuối cùng ghi xuống Access

- `GxDateInput.Value` getter (`GxDateInput.cs:99-111`): nếu `year` không hợp lệ trả về
  `DBNull.Value`; ngược lại gọi `Memory.GetDateString(day, month, year)`.
- `Memory.GetDateString(string day, string month, string year)`
  (`Source/DBAccess/CMemory.cs:755-763`):

  ```csharp
  public static string GetDateString(string day, string month, string year)
  {
      string date = "";
      if (day.Trim() != "") date += int.Parse(day).ToString("00") + "/";
      if (month.Trim() != "") date += int.Parse(month).ToString("00") + "/";
      return date + Memory.GetYear(year).ToString("0000");
  }
  ```

  → nhập chỉ `1985` (chỉ điền ô năm) cho kết quả chuỗi **`"1985"`**; nhập `05` + `1985` (tháng
  + năm) cho kết quả **`"05/1985"`**; nhập đủ cả ba cho **`"05/01/1985"`**. **Không có bước nào
  chèn `01` vào phần thiếu.**
- `Source/GXControl/frmGiaoDan.cs:963`: `row[GiaoDanConst.NgaySinh] = dtNgaySinh.Value;` — gán
  thẳng chuỗi trên (hoặc `DBNull.Value`) vào cột Access, không qua bước chuẩn hoá nào khác.
  Tương tự lúc đọc lại: `frmGiaoDan.cs:1093`: `dtNgaySinh.Value = row[GiaoDanConst.NgaySinh];`.

### A.6 Phần thiếu được xử lý "thông minh" ở khắp nơi khác trong `Memory`, không chỉ lúc nhập

Đây là điểm quan trọng cho phần đề xuất bên dưới: desktop **không** biến ngày thiếu thành ngày
đầy đủ rồi quên đi độ chính xác — toàn bộ tầng nghiệp vụ phía sau tiếp tục phân biệt "chỉ có
năm" với "có đủ ngày":

- `Memory.GetDateFromString(string sDate, bool maxMonthIfNull)`
  (`Source/DBAccess/CMemory.cs:689-736`, xem docstring dòng 693-703): chuyển một chuỗi ngày
  thiếu thành `DateTime` để tính toán/so sánh, nhưng **chọn ngày đầu kỳ hay cuối kỳ tuỳ tham
  số** `maxMonthIfNull` — nếu `true`, tháng thiếu → 12, ngày thiếu → ngày cuối tháng
  (`dòng 706-712`, và `DaysInMonth` ở dòng 725-728); nếu `false`, tháng thiếu → 1, ngày thiếu
  → theo dòng 709-711 (`day = "0"`) rồi cũng được `DaysInMonth`-hoá thành cuối tháng của tháng
  đó khi `day=="0"` (dòng 727-731) — tức là hàm này tính "biên sớm nhất có thể" và "biên muộn
  nhất có thể" của một ngày thiếu, **không mặc định 01/01**.
- `Memory.KiemTraTuoiKhongHopLe(ngayTruoc, ngaySau, khoangCach)` (`CMemory.cs:1418-1440`) gọi
  `GetDateFromString(ngayTruoc, false)` (biên sớm) và `GetDateFromString(ngaySau, true)` (biên
  muộn) — cố ý lấy khoảng rộng nhất có thể để **tránh báo sai** khi ngày tháng bị thiếu.
- `Memory.CompareTwoStringDate(string, string)` (`CMemory.cs:2053-2085`, docstring ngay phía
  trên dòng 2053): so sánh hai chuỗi ngày theo từng cấp — năm trước, rồi mới đến tháng (chỉ khi
  **cả hai bên đều có tháng**), rồi mới đến ngày (chỉ khi **cả hai bên đều có ngày**). Trích
  nguyên văn comment: *"Nếu 1 trong 2 stringdate chỉ có year thì chỉ so sánh theo year. Tương tự
  nếu chỉ có month/year thì chỉ so sánh theo month year."*
- Toàn bộ ràng buộc "ngày sinh ≤ ngày rửa tội ≤ ..." ở `frmGiaoDan.cs` (hàm `checkInput`, dòng
  255 trở đi; ví dụ dòng 375-390) đều dùng `Memory.CompareTwoStringDate`/`CheckDateConstraint`
  trên các chuỗi ngày-có-thể-thiếu này, không ép chúng thành ngày đầy đủ trước.

Nói cách khác: **desktop không "chuẩn hoá 01/01" ở bất kỳ đâu tôi tìm thấy** — độ chính xác của
ngày tháng được giữ nguyên vẹn trong chuỗi text và được tôn trọng xuyên suốt các hàm so sánh,
kiểm tra tuổi, ràng buộc thứ tự ngày. Việc bản web hiện dùng `DateOnly?` (một giá trị ngày luôn
đầy đủ) làm mất khả năng này ngay từ tầng dữ liệu.

## B. Tự nhảy sang ô/control tiếp theo

- Cơ chế `this.FindForm().GetNextControl(this, true)` (nhảy theo `TabIndex` của form cha) xuất
  hiện ở **hai nơi trong toàn bộ `Source/GXControl`**: `GxDateInput.cs` (dòng 477, 485, 579) và
  `GxMaskInput.cs` (dòng 169, 177) — `GxMaskInput` là control ngày/tháng-không-năm dùng trong
  `GxDayMonthField` (ví dụ ngày sinh hoạt định kỳ ở `Source/GXControl/frmHoiDoan.Designer.cs`,
  tìm thấy qua `grep GxDayMonthField`). Cùng một khuôn logic KeyPress-đếm-đủ-số-rồi-nhảy được
  lặp lại y hệt giữa hai control này.
- **Không tìm thấy** cơ chế tương tự gắn vào sự kiện chọn item trong `ComboBox`/dropdown/gợi ý
  autocomplete ở các form đã đọc. Ví dụ `frmGiaoDan.cs` có các handler
  `cbGiaoHo_SelectedIndexChanged` (dòng 1319), `cbChuyenXu_SelectedIndexChanged` (dòng 1231),
  `cbPhai_SelectedIndexChanged` (dòng 1482) — không handler nào gọi `.Focus()` sang control
  khác, chỉ xử lý nghiệp vụ (lọc dữ liệu liên quan). → **Chỗ chưa chắc**: hành vi "chọn dropdown
  tự nhảy" người dùng mô tả có thể đến từ hành vi mặc định của control `AutoCompleteTextBox`
  (thư viện `Femiani.Forms.UI.Input`, biên dịch sẵn, không có mã nguồn trong `Source/`) hoặc
  `UIComboBox` (thư viện Janus, cũng đóng gói sẵn) khi nhấn Enter/Tab để chọn gợi ý — tôi không
  đọc được mã nguồn của hai thư viện này nên không khẳng định được cơ chế cụ thể. Không nên
  khẳng định bản desktop có "auto-jump khi chọn dropdown" do code tự viết — chưa có bằng chứng.

## C. Bộ nhớ gợi ý các mục hay nhập

Có **hai cơ chế hoàn toàn khác nhau và không liên quan tới nhau** — không cơ chế nào khớp đúng
100% với "cache theo tần suất dùng nhiều nhất" mà người dùng mô tả.

### C.1 `DuLieuChung` — danh sách "Tên thánh" cố định, không đếm tần suất

- Định nghĩa cột: `Source/DBAccess/GxConstants.cs:1066-1073` — bảng `DuLieuChung` chỉ có
  `ID, LoaiDuLieu, MaDuLieu, DuLieu1, DuLieu2`. **Không có cột đếm số lần dùng.**
- `enum LoaiDuLieuChung` (`GxConstants.cs:537-541`) chỉ có **hai** giá trị:
  `ChungChung = 0`, `TenThanh = 1`. Trong toàn bộ `Source/`, chỉ `LoaiDuLieu = 1` (Tên thánh)
  thực sự được dùng ở đường dẫn chạy (runtime) — không tìm thấy nơi nào ghi/đọc với
  `LoaiDuLieu = 0` (ChungChung) ngoài khai báo enum.
- Nguồn dữ liệu: bảng được nạp **một lần duy nhất** lúc nâng cấp phiên bản 2.1.1.8, đọc từ tệp
  tĩnh `Resources\TenThanh.txt` cạnh chương trình
  (`Source/ChuongTrinh/UpdateProcess.cs:965-980`, vùng `#region Import ten thanh`) — một danh
  sách tên thánh có sẵn, không phải tự học từ thao tác gõ của người dùng.
- Truy vấn hiển thị: `SqlConstants.SELECT_TENTHANH`
  (`Source/DBAccess/SqlConstants.cs:274-275`):
  `SELECT DISTINCT DuLieu1 AS TenThanh FROM DuLieuChung WHERE LoaiDuLieu=1` — **không có
  `ORDER BY`**, tức truy vấn này không tự sắp theo tần suất hay theo bảng chữ cái; thứ tự trả
  về phụ thuộc engine Access (Jet/ACE), không đảm bảo. Dòng comment ngay phía trên
  (`SqlConstants.cs:274`) là bản cũ bị comment lại:
  `//SELECT DISTINCT TenThanh FROM GiaoDan` — cho thấy trước đây danh sách này từng lấy trực
  tiếp từ dữ liệu giáo dân đã nhập (tự học), sau đổi sang lấy từ `DuLieuChung` (danh sách tĩnh).
- Nơi dùng: `Source/GXControl/GxTenThanh.cs:41-51` — nạp `SELECT_TENTHANH` vào
  `Janus.Windows.EditControls.UIComboBox`, bật `AutoComplete = true`
  (`GxTenThanh.cs:55`, thư viện Janus đóng gói sẵn, không đọc được mã nguồn xử lý gõ-để-lọc).
- Các hàm `Memory.UpdateDulieuChung`/`GetDulieuChung`/`DeleteDulieuChung`
  (`Source/DBAccess/CMemory.cs:1919-1963`) cho phép thêm/sửa/xoá dòng trong `DuLieuChung`, và
  chấp nhận `LoaiDuLieuChung` bất kỳ — nhưng qua rà soát toàn `Source/`, **chỉ có
  `UpdateProcess.cs` gọi các hàm này** (đường migrate một-lần nêu trên); không có màn hình
  nhập liệu nào gọi các hàm này lúc người dùng đang gõ để tự thêm giá trị mới vào
  `DuLieuChung`.

→ **Kết luận C.1**: `DuLieuChung`/`LoaiDuLieu=1` là một **danh sách tra cứu tĩnh, được soạn sẵn
một lần**, không phải "cache học theo thao tác nhập của người dùng", và không sắp theo tần suất.

### C.2 `autocomplete.xml` — cache thật sự "nhớ những gì đã gõ", nhưng KHÔNG đếm tần suất, và lưu cục bộ trên máy

Cơ chế này nằm trong `GxTextField` — lớp cơ sở của **rất nhiều** ô nhập liệu tự do (nơi sinh,
nơi rửa tội, người ban bí tích, nghề nghiệp, ghi chú, …), có `AutoCompleteEnabled` bật ở
Designer (ví dụ `txtNoiThemSuc.AutoCompleteEnabled = true`,
`Source/GXControl/frmGiaoDan.Designer.cs:821`; `txtLinhMucRuaToi.AutoCompleteEnabled = true`,
dòng 881).

- Tệp lưu: `Memory.AppPath + "autocomplete.xml"` — **một tệp XML cục bộ cạnh chương trình trên
  từng máy**, không phải bảng trong Access
  (`Source/GXControl/GxTextField.cs:16-18`, hằng số `XML_AUTOCOMPLETE_FILENAME`).
- Cấu trúc XML tự tạo lúc chưa có tệp
  (`GxTextField.cs:41-56`, hàm `CreateXmlFile`):
  `<application><forms><frmXxx><TenControl caption="..."><item>giá trị</item>...</TenControl>
  </frmXxx></forms></application>` — khoá theo **cặp (tên form, tên control)**, mỗi ô nhập có
  danh sách gợi ý riêng của chính nó, không dùng chung giữa các ô khác nhau dù cùng ý nghĩa
  (ví dụ `txtNoiRuaToi` và `txtNoiThemSuc` có hai danh sách "nơi" tách biệt).
- Nạp lúc mở form: `GxTextField_Load` (`GxTextField.cs:150-162`) gọi
  `GetAutoCompleteItems(formName, controlName)` (`GxTextField.cs:177-214`) — duyệt cây XML theo
  đúng tên form/control, trả về `AutoCompleteEntryCollection` theo **đúng thứ tự các thẻ
  `<item>` xuất hiện trong XML** — tức thứ tự **được thêm vào lúc nào thì nằm ở đó**, không có
  bước sắp xếp lại theo tần suất.
- Ghi lúc đóng form: `frmParent_FormClosing` (`GxTextField.cs:169-175`) gọi
  `SaveAutoCompleteItem(formName, controlName, Label, this.Text)`
  (`GxTextField.cs:216-279`): tìm trong danh sách `<item>` hiện có xem giá trị vừa nhập (so
  sánh không phân biệt hoa/thường, dòng 259-266) đã tồn tại chưa; **nếu chưa có thì thêm mới
  vào cuối** (dòng 268-272); **nếu đã có thì không làm gì cả** — không có cơ chế tăng bộ đếm,
  không có cột đếm số lần dùng ở bất kỳ đâu trong tệp XML này.
- **Không giới hạn** số lượng mục lưu, **không bao giờ xoá bớt** mục cũ (không tìm thấy hàm xoá
  item), và **chỉ tồn tại trên máy đang chạy** — không đồng bộ giữa các máy, không nằm trong
  `giaoxu.mdb`.

`GxLinhMuc` (ô "Người ban bí tích", kế thừa `GxTextField`,
`Source/GXControl/GxLinhMuc.cs:14`) từng có một cơ chế **thông minh hơn hẳn**: hàm tĩnh
`LoadAutoCompleteData()` gộp tên linh mục từ **bốn nguồn khác nhau** —
`SELECT DISTINCT ChaRuaToi`, `ChaRuocLe`, `ChaThemSuc FROM GiaoDan` và
`LinhMucChung FROM HonPhoi` — thành một danh sách gợi ý dùng chung cho mọi ô linh mục trên toàn
ứng dụng (`GxLinhMuc.cs`, toàn bộ vùng từ dòng "protected override void OnLoad" tới hết hàm
`MergeTables`). **Toàn bộ đoạn này đã bị comment (`//`) hết**, kể cả lời gọi nó trong
constructor — tức là tính năng này **tồn tại trong mã nguồn nhưng đã bị tắt**, và `GxLinhMuc`
hiện tại chỉ còn hành vi autocomplete-theo-XML-cục-bộ chung của `GxTextField` (mục trên).

→ **Kết luận C.2**: cơ chế "nhớ giá trị hay gõ" có thật, nhưng là **danh sách các giá trị đã
từng gõ, theo đúng thứ tự lần đầu gõ, không đếm tần suất, không giới hạn, lưu trong một tệp XML
riêng của từng máy tính** — khác với mô tả "sắp xếp theo tần suất dùng nhiều nhất" của người
dùng. Có thể trí nhớ người dùng đang nhớ về **ý định thiết kế** (đoạn `GxLinhMuc` đã comment) chứ
không phải hành vi thực tế đang chạy — hoặc một cơ chế khác tôi chưa tìm ra. Ghi rõ vào "Chỗ
chưa chắc".

Tôi **không có công cụ `psql`** trong môi trường này nên **chưa đối chiếu được** trực tiếp dữ
liệu thật `qlgx_thu.du_lieu_chung` (ví dụ để xem `LoaiDuLieu` có giá trị nào ngoài 0/1 trong dữ
liệu thật, phòng trường hợp `enum` trong mã không phản ánh hết dữ liệu đã có) — đây là việc nên
làm thêm trước khi thiết kế bảng gợi ý phía web, chỉ cần chạy `SELECT loai_du_lieu, COUNT(*)
FROM du_lieu_chung GROUP BY loai_du_lieu` (chỉ đọc).

## D. Các hỗ trợ nhập liệu khác tìm được

- **Tự viết hoa chữ đầu mỗi từ khi rời ô** (`AutoUpperFirstChar`): thuộc tính trên
  `GxBaseField` (`Source/GXControl/GxBaseField.cs:35-51`, xử lý ở `editBase_Leave`, dòng
  151-158), gọi `Memory.AutoUpperFirstChar(editBase.Text)`
  (`Source/DBAccess/CMemory.cs:1079-1088`, hàm chi tiết 1091-1160) — viết hoa chữ cái đầu từng
  từ cách nhau bởi khoảng trắng (`"nguyễn văn an"` → `"Nguyễn Văn An"`), có xử lý riêng cho từ
  bắt đầu bằng ký tự đặc biệt trong ngoặc (`CMemory.cs:1099-1141`). Bật/tắt qua cấu hình
  `CHUANHOA_TUCHUANHOA` (`Source/DBAccess/GxConstants.cs:87`), kiểu chữ trong ngoặc qua
  `CHUANHOA_TRONGNGOAC` (`GxConstants.cs:85`). Xác nhận nhiều ô bật tính năng này, ví dụ
  `txtNoiThemSuc.AutoUpperFirstChar = true` (`frmGiaoDan.Designer.cs:822`) — nhưng
  `dtNgaySinh` (control ngày) **không có** thuộc tính này (không áp dụng cho ngày tháng).
- **Tự chuyển bảng mã phông chữ tiếng Việt cũ (VNI/TCVN...) sang Unicode**:
  `convertFont.Convert(ref word, FontIndex.iUTH, FontIndex.iUNI)`
  (`CMemory.cs:1157-1160`), bật/tắt qua cấu hình `CHUANHOA_TUCHUYENMA` (`GxConstants.cs:88`) —
  liên quan tới dự án `Source/ConvertFont/` và `vnConvert.dll` nêu trong bố cục repo.
- **Tự sửa vị trí dấu thanh tiếng Việt** ("hoà" → "hòa" theo quy ước mới):
  `Memory.ChuanHoaDau(s)` gọi `convertFont.ConvertVietnameseSign(s)`
  (`CMemory.cs:1163-1166`), bật/tắt qua cấu hình `CHUANHOA_TUDOIDAU` (`GxConstants.cs:86`).
- **Ô số tự sửa về "0" khi rời ô nếu gõ không phải số**: `GxTextField` khi `NumberMode &&
  NumberInputRequired` và nội dung không phải số hợp lệ thì gán lại `txt.Text = "0"`
  (`Source/GXControl/GxTextField.cs`, hàm `txt_Leave`, khoảng dòng 118-127).
- **Tắt IME (bộ gõ tiếng Việt) khi vào ô ngày tháng**: `ImeMode = ImeMode.Off` ép khi `Enter`
  vào `txtDay/txtMonth/txtYear` (`GxDateInput.cs:391-413`) — cần thiết để phím số gõ liên tục
  không bị bộ gõ tiếng Việt can thiệp.
- **Bôi đen toàn bộ nội dung ô khi focus vào** (`SelectAll()` trên `Enter`, cùng vị trí trên) —
  hỗ trợ việc gõ đè số mới mà không cần xoá tay.
- **Phím tắt**: chỉ tìm thấy một phím tắt toàn cục, `F1` mở trợ giúp
  (`Source/ChuongTrinh/frmMain.Designer.cs:676`,
  `hướngDẫnSửDụngToolStripMenuItem.ShortcutKeys = Keys.F1`). Không tìm thấy phím tắt nhập liệu
  nhanh nào khác (mã giáo dân, sao chép dòng trước, …) trong các form đã đọc
  (`frmGiaoDan.cs`, `frmGiaDinh.cs`, danh sách giáo dân/gia đình) — **không có nghĩa là chắc
  chắn không có ở màn hình khác**; còn khoảng 60 màn hình nhỏ hơn chưa soát (xem
  `docs/superpowers/specs/man-hinh/README.md`).
- **Không tìm thấy** cơ chế "sao chép giá trị từ dòng trước" (kiểu Excel fill-down) ở bất kỳ
  đâu trong `Source/GXControl` hay `Source/ChuongTrinh` — tìm theo các từ khoá
  `CopyFromPreviousRow`, `sao chép`, không ra kết quả. Không khẳng định là hoàn toàn không có
  (có thể đặt tên khác), nhưng không có bằng chứng.
- **Giới hạn ràng buộc ngày động theo ngữ cảnh (`MinDate`/`MaxDate`)**: `frmGiaoDan.cs` hàm
  `checkInput()` (dòng 255 trở đi) gọi `setMaxDate`/`setMinDate` để ép, ví dụ, "ngày rửa tội"
  không được trước "ngày sinh", "ngày qua đời" là trần trên cho mọi ngày khác (dòng 261-284) —
  đây không hẳn là "hỗ trợ nhập liệu" nhưng là ngữ cảnh quan trọng nếu bản web muốn giữ đúng
  ràng buộc nghiệp vụ khi làm control ngày tháng mới.

## Đề xuất thiết kế cho bản web

### 1. Ngày tháng thiếu — cần người dùng quyết định, dưới đây là phân tích hai hướng

**Bối cảnh quan trọng từ nghiên cứu ở mục A.6**: bản desktop **không hề** "chuẩn hoá 01/01" ở
bất kỳ đâu — nó cố tình giữ nguyên độ thiếu trong một chuỗi text, và xây cả một tầng hàm
(`GetDateFromString` với `maxMonthIfNull`, `CompareTwoStringDate`) để so sánh/tính tuổi đúng
đắn trên ngày thiếu đó. Nếu bản web ép ngày thiếu thành ngày đầy đủ ngay lúc nhập, đó sẽ là một
bước lùi so với những gì desktop đã làm cẩn thận trong hơn chục năm dữ liệu thật, không phải
"giữ nguyên hành vi cũ".

- **Hướng (a) — chuẩn hoá 01/01, chỉ giữ cột `date`.**
  Nhập `1985` → lưu `01/01/1985`. Đơn giản, không đổi schema, khớp với `DateOnly?` đang có sẵn
  trong `WebApp/src/Qlgx.Domain/Entities/GiaoDan.cs`. **Nhược điểm nặng**: mất vĩnh viễn thông
  tin "chỉ biết năm" — không có cách nào phân biệt lại với "sinh đúng ngày 1/1" sau khi đã lưu.
  Hậu quả cụ thể: in Giấy chứng nhận/Lý lịch cá nhân (`docs/superpowers/specs/man-hinh/in-an.md`)
  sẽ in ngày 01/01 như một sự thật, sai lệch sổ sách gốc — với sổ giáo xứ, đây là vấn đề tính
  xác thực, không chỉ thẩm mỹ. Không thể khôi phục đúng nghĩa 207 bản ghi `du_lieu_loi` — chỉ
  "nhét được vào cột `date`" chứ không phục hồi được thông tin gốc là "chỉ biết năm".
- **Hướng (b) — giữ cột `date` + thêm cột đánh dấu độ chính xác.**
  Ví dụ mỗi trường ngày (`NgaySinh`, `NgayRuaToi`, …) có thêm một cột nhỏ kiểu enum/byte:
  `Day` (đủ ngày) / `Month` (chỉ tháng-năm) / `Year` (chỉ năm). Giá trị `date` lưu **biên dưới**
  của khoảng đó (ví dụ 1985 → `1985-01-01`) để mọi phép so sánh/sắp xếp SQL vẫn hoạt động bình
  thường, còn cột độ chính xác quyết định cách **hiển thị/in** ("năm 1985" thay vì "01/01/1985").
  Khôi phục được đúng nghĩa cả 207 bản ghi `du_lieu_loi`: parser chỉ cần chấp nhận thêm hai định
  dạng `"yyyy"` và `"MM/yyyy"` (hiện `NgayThangText.cs:12` chỉ có 4 dạng `dd/MM/yyyy` đầy đủ),
  gắn đúng cờ độ chính xác, rồi ghi vào `(ngay, do_chinh_xac)` thay vì `du_lieu_loi`.
  **Nhược điểm**: đổi schema (thêm cột cho mỗi trường ngày liên quan — khá nhiều trường:
  `NgaySinh, NgayRuaToi, NgayRuocLe, NgayThemSuc, NgayQuaDoi, NgayChuyen`, …), thêm việc ở mọi
  nơi hiển thị/xuất báo cáo/API phải biết đọc cờ này thay vì chỉ đọc `date`.
- Có thể có hướng lai: chỉ thêm cột độ chính xác cho các trường "nhạy cảm" nhất về mặt lịch sử
  (ít nhất `NgaySinh` — sinh ra ngay từ nhu cầu gốc người dùng nêu), giữ (a) cho các trường ít
  quan trọng hơn — nhưng đây cũng là quyết định cần người dùng chốt, không tự chọn thay.

**Tôi không tự quyết định hướng nào** — đây đúng là quyết định "hệ trọng" mà brief yêu cầu phải
hỏi lại. Khuyến nghị cá nhân (không phải quyết định): hướng (b), vì bằng chứng mục A.6 cho thấy
chính desktop coi độ chính xác là thông tin cần giữ, và 207 bản ghi thật đang chờ khôi phục đúng
nghĩa chứ không chỉ "nhét vừa cột".

### 2. Control ngày tháng cho web

Đối chiếu từng điểm với desktop:

| Hành vi desktop | Đề xuất web | Ghi chú |
|---|---|---|
| Ba ô rời `dd`/`mm`/`yyyy` + dấu `/` (mục A.1) | Giữ y hệt bố cục ba ô — dễ áp dụng cho HTML (`<input>` 3 ô hoặc 1 component ghép) | |
| Ô trống là trống trơn, không có `__/__/____` (mục A.1) | Có thể chọn hiện placeholder mờ `dd/mm/yyyy` trong mỗi ô con — cải tiến khả dụng, **không phải chép nguyên từ desktop** vì desktop không có | Cần nói rõ đây là quyết định thiết kế mới, không phải "giữ nguyên hành vi cũ" |
| Gõ liên tục không cần `/`, tự nhảy ô khi đủ số (mục A.2) | Làm y hệt: `onInput` đếm đủ 2 chữ số ở ô ngày/tháng thì tự chuyển focus sang ô kế; ô năm đủ 4 số thì coi là xong | Cần thêm: cho phép Backspace ở ô trống quay lại ô trước — desktop không có (mục B), có thể coi là cải tiến khả dụng bàn phím |
| `SelectAll()` khi focus vào ô có sẵn số (mục A.2) | Làm y hệt bằng `onFocus` gọi `select()` | |
| Ép tắt IME khi vào ô (mục A.2) | Không áp dụng được y hệt trên web (trình duyệt không cho JS tắt IME hệ điều hành) — có thể bù bằng `inputMode="numeric"` để mời bàn phím số trên thiết bị chạm | Khác biệt cố ý — do giới hạn nền tảng, không phải chọn khác |
| Rời ô kiểm tra hợp lệ bằng `MessageBox` chặn (modal) (mục A.4) | Web nên báo lỗi **tại chỗ** (inline, dưới ô nhập), không dùng dialog chặn thao tác | Khác biệt cố ý — mẫu UX web hiện đại, tránh gián đoạn luồng gõ liên tục hàng nghìn bản ghi |
| Chỉ năm hoặc tháng+năm hợp lệ, ngày đơn lẻ không hợp lệ (mục A.4) | Giữ nguyên quy tắc | Đây chính là yêu cầu cốt lõi ban đầu |

### 3. Tự nhảy ô tiếp theo — và cảnh báo khả năng tiếp cận

- Trong nội bộ control ngày tháng (3 ô con) và trong các control dạng mask tương tự (nếu web
  có, ví dụ số điện thoại chia nhóm): tự nhảy khi đã gõ đủ ký tự là hợp lý và có tiền lệ rõ ràng
  từ desktop (mục A.2, B).
- **Tự nhảy sang control HOÀN TOÀN KHÁC trên form** (như `GetNextControl` desktop làm ở cuối ô
  năm, mục B) — nên cân nhắc kỹ trên web: một số người dùng bàn phím/đọc màn hình dựa vào việc
  Tab luôn di chuyển focus theo cách họ kiểm soát; tự động nhảy focus **ngoài ý muốn người dùng**
  (đặc biệt nếu xảy ra ngay khi họ còn đang gõ, chưa chủ động rời ô) là một anti-pattern khả
  năng tiếp cận được biết tới (WCAG khuyến cáo không thay đổi ngữ cảnh khi người dùng không chủ
  động yêu cầu). Đề xuất: chỉ tự nhảy ra control khác **sau khi ô năm đã đủ 4 số HỢP LỆ VÀ
  người dùng vừa gõ ký tự cuối cùng** (tức là hành động gõ đó tự nhiên dẫn tới việc "xong"),
  không tự nhảy khi rời ô bằng cách khác (Tab thủ công, click chuột nơi khác) — để không đá văng
  người dùng khỏi nơi họ chủ động click tới.
- **Chọn dropdown/gợi ý rồi tự nhảy** (mục B): desktop **không có bằng chứng code tự viết** cho
  hành vi này (có thể là hành vi ẩn của thư viện đóng gói). Với web, nên coi đây là **quyết định
  UX mới**, không phải migrate nguyên trạng — và nếu làm, chỉ nên tự nhảy khi người dùng **chọn
  bằng Enter hoặc click chuột vào gợi ý** (hành động xác nhận rõ ràng), không tự nhảy khi chỉ
  di chuột qua hoặc dùng phím mũi tên để duyệt danh sách (đó chỉ là xem trước, chưa chọn).

### 4. Bộ nhớ gợi ý theo tần suất

- **Với "Tên thánh"**: dùng thẳng bảng `du_lieu_chung` đã có sẵn 343 dòng trong PostgreSQL
  (tương ứng `LoaiDuLieu=1` bên Access, mục C.1) — đây vốn là danh sách tra cứu tĩnh nên **không
  cần** cơ chế đếm tần suất, chỉ cần liệt kê theo bảng chữ cái là đủ khớp với những gì desktop
  đang làm.
- **Với các trường tự do khác** (nơi sinh, nơi rửa tội, người ban bí tích, nghề nghiệp, …):
  desktop dùng tệp XML cục bộ không đếm tần suất (mục C.2) — đây **không phải mô hình nên chép
  nguyên** cho web (không hợp với kiến trúc máy chủ tập trung, và bản thân nó cũng không làm
  đúng điều người dùng mong muốn — "sắp theo tần suất"). Đề xuất thiết kế **mới, tốt hơn cả
  desktop**, đúng với ý định người dùng mô tả:
  - Một bảng dùng chung, ví dụ `goi_y_nhap_lieu (giao_xu_id, ten_truong, gia_tri, so_lan_dung,
    lan_cuoi_dung)`.
  - Mỗi lần lưu form thành công, `UPSERT` giá trị đã nhập: nếu đã có (theo `giao_xu_id` +
    `ten_truong` + `gia_tri`, so sánh không phân biệt hoa/thường như desktop đang làm ở mục
    C.2) thì `so_lan_dung = so_lan_dung + 1`; nếu chưa có thì thêm mới với `so_lan_dung = 1`.
  - Truy vấn gợi ý: `ORDER BY so_lan_dung DESC` (kèm giới hạn số lượng hiển thị, ví dụ top 10) —
    **đây mới đúng là "sắp theo tần suất dùng nhiều nhất"** mà người dùng mô tả, khác với những
    gì desktop code hiện có (chỉ theo thứ tự lần đầu gõ).
  - **Ràng buộc bắt buộc**: mọi truy vấn/ghi vào `goi_y_nhap_lieu` phải lọc theo `giao_xu_id`
    lấy từ **claim đăng nhập** (theo đúng kiến trúc RLS đã có, xem
    `WebApp/src/Qlgx.Data/BoiCanhGiaoXuConnectionInterceptor.cs`), **không bao giờ** nhận
    `giao_xu_id` từ tham số client — nếu không, gợi ý của giáo xứ A sẽ rò rỉ sang giáo xứ B (ví
    dụ tên linh mục, địa danh riêng của một giáo xứ xuất hiện gợi ý ở giáo xứ khác — vừa sai dữ
    liệu vừa lộ thông tin không thuộc về họ).
  - `ten_truong` nên là một khoá ổn định theo ý nghĩa nghiệp vụ (ví dụ `"noi_rua_toi"`), **không**
    theo cặp (tên-form-desktop, tên-control-desktop) như XML cũ — vì bản web có thể tổ chức màn
    hình khác desktop, và gộp gợi ý theo Ý NGHĨA trường (mọi ô "nơi rửa tội" dùng chung một danh
    sách gợi ý) hữu ích hơn tách theo từng form như desktop đang làm.

### 5. Ước lượng khối lượng công việc và thứ tự đề xuất

1. **Quyết định hướng xử lý ngày thiếu (mục 1)** — việc của người dùng/người điều phối trước
   tiên, vì nó quyết định toàn bộ các bước sau. Không có ước lượng công (là quyết định, không
   phải code).
2. **Đổi schema + parser + migration cho 207 bản ghi lỗi** — làm ngay sau khi có quyết định ở
   bước 1. Việc này nền tảng, càng để lâu càng nhiều màn hình phải sửa lại lần hai. Cỡ: vừa (sửa
   entity, `NgayThangText.cs`, viết script backfill từ `du_lieu_loi`, viết test cho các trường
   hợp `"1985"`, `"05/1985"`, `"dd/MM/yyyy"`).
3. **Component control ngày tháng dùng chung cho web** (3 ô, tự nhảy, kiểm tra thiếu theo đúng
   quy tắc mục A.4) — phụ thuộc bước 2 để biết API/kiểu dữ liệu nó cần trả về. Cỡ: vừa, làm một
   lần, dùng lại cho mọi màn hình có trường ngày (nhiều: giáo dân, gia đình, hôn phối, hội đoàn…).
4. **Gắn tự-nhảy-focus vào từng màn hình** — làm dần theo từng màn hình đang migrate, không cần
   làm hết một lượt. Cỡ nhỏ mỗi màn, cộng dồn thì nhiều vì có hàng chục màn hình.
5. **Bảng `goi_y_nhap_lieu` + API đọc/ghi gợi ý + gắn vào ô nhập tự do** — độc lập với việc ngày
   tháng, có thể làm song song với bước 3-4. Cỡ: vừa (thêm bảng, RLS theo `giao_xu_id`, API,
   component gợi ý ở frontend).
6. **Các hỗ trợ nhỏ khác** (tự viết hoa đầu từ, v.v. — mục D) — có thể làm sau cùng, độc lập,
   từng cái nhỏ, không phụ thuộc các bước trên; tự-chuyển-mã-phông-cũ và tự-sửa-dấu (VNI/TCVN,
   `CHUANHOA_TUCHUYENMA`/`CHUANHOA_TUDOIDAU`) có thể **không còn cần thiết** ở bản web nếu người
   dùng web gõ tiếng Việt Unicode chuẩn ngay từ đầu (không còn gõ bằng font cũ) — nên hỏi lại
   người dùng có còn cần tính năng này không trước khi bỏ công làm.

## Chỗ chưa chắc

- Placeholder `__/__/____` mà người dùng mô tả: không tìm thấy trong `GxDateInput` — có thể
  người dùng nhớ nhầm hoặc tôi đã bỏ sót một biến thể control khác (`GxDateField` được dùng ở
  ~60 màn hình chưa soát hết); nên hỏi lại hoặc chạy thử bản desktop hiện tại để quan sát trực
  tiếp trước khi thiết kế web.
- Hành vi "chọn dropdown tự nhảy" (mục B): không tìm được trong mã tự viết; có thể do thư viện
  đóng gói `Femiani.Forms.UI.Input`/Janus `UIComboBox`. Không đọc được mã nguồn hai thư viện
  này (đóng gói dạng DLL biên dịch sẵn).
- `LoaiDuLieu` trong dữ liệu thật `qlgx_thu.du_lieu_chung` có giá trị nào ngoài 0/1 hay không:
  chưa kiểm chứng (không có `psql` trong môi trường làm việc này).
- Cơ chế gợi ý "theo tần suất" người dùng mô tả không khớp với bất kỳ đoạn code nào tìm được
  (mục C) — không loại trừ khả năng có ở nơi khác chưa soát (mã đóng gói DLL, hoặc một control
  chưa đọc tới trong ~60 màn hình còn lại).
- Chưa xác nhận bằng dữ liệu thật việc `Memory.GetDateString` không zero-pad có đúng 100% với
  mọi bản ghi cũ trong `giaoxu.mdb` hay không (có thể một số bản ghi cũ được nhập/sửa bằng công
  cụ khác ngoài `GxDateInput`, ví dụ import Excel — xem `ImportData.cs`, chưa soát trong lần
  nghiên cứu này).
