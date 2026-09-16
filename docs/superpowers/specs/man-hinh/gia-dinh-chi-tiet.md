# Màn hình: Chi tiết gia đình (`frmGiaDinh.cs`)

| | |
|---|---|
| Tệp nguồn | `Source/GXControl/frmGiaDinh.cs` (2101 dòng) + `Source/GXControl/frmGiaDinh.Designer.cs` (644 dòng) |
| UserControl dùng lại | `GxGiaoDanList` (lưới thành viên), `GxGiaoDan` (ô chọn Người nam/Người nữ, x2), `GxGiaoHo` (combo Giáo họ), `GxHonPhoiGiaDinh` (khối hôn phối), `GxDienGiaDinh` (combo "Diện"), `GxPictureField` (ảnh gia đình), `GxAddEdit`, `GxCommand`, `GxRadioBox` (Chủ hộ), `GxCheckBox`, `GxTextField`, `GxDateField`, `GxGroupBox` |
| Bảng dữ liệu đụng tới | `GiaDinh`, `ThanhVienGiaDinh`, `GiaoDan`, `HonPhoi`, `GiaoDanHonPhoi`, `ChuyenXu`, `GiaoHo` |
| Trạng thái migrate | xong → `WebApp/src/web/src/screens/GiaDinhDetail.tsx` (nhưng thiếu nhiều hành vi, xem mục 10) |

## 0. Kết luận quan trọng: có HAI file `frmGiaDinh.cs` — chỉ một cái được dùng

Repo có hai file trùng tên:

- `Source/GXControl/frmGiaDinh.cs` — 2101 dòng, `namespace GxControl`.
- `Source/ChuongTrinh/frmGiaDinh.cs` — 965 dòng, `namespace GiaoXu`.

Đã xác minh bằng cách đọc `.csproj` và `.sln`:

- File `.sln` thật của ứng dụng (`Source/GiaoXu.sln`) chỉ liệt kê `ChuongTrinh\GiaoXu.csproj` (project tên "GiaoXu", `AssemblyName=GiaoXu`), **không** liệt kê `ChuongTrinh.csproj`.
- `Source/ChuongTrinh/GiaoXu.csproj` (project thật, đang được build) **không** có dòng `<Compile Include="frmGiaDinh.cs">` nào — nghĩa là `Source/ChuongTrinh/frmGiaDinh.cs` (965 dòng) **không được biên dịch vào ứng dụng thật**. Nó chỉ được liệt kê trong `Source/ChuongTrinh/ChuongTrinh.csproj`, một file project khác, mồ côi, không nằm trong bất kỳ `.sln` nào.
- `frmGiaDinhList.cs` (namespace `GiaoXu`, có `using GxControl;`) gọi `new frmGiaDinh()` không tiền tố — vì namespace `GiaoXu` không có lớp `frmGiaDinh` nào được biên dịch, tên này chỉ có thể phân giải tới `GxControl.frmGiaDinh` nhờ `using GxControl;`.

**Kết luận: `Source/GXControl/frmGiaDinh.cs` (2101 dòng) là màn hình thật đang chạy. `Source/ChuongTrinh/frmGiaDinh.cs` (965 dòng) là mã chết/mồ côi, không migrate.** Toàn bộ spec dưới đây viết từ file `GXControl/frmGiaDinh.cs`.

## 1. Mục đích

Nhập/sửa thông tin một gia đình: người nam (chồng), người nữ (vợ), tên gia đình, giáo họ, địa chỉ, điện thoại, và danh sách "thành viên khác" (con cái, ông bà, v.v.). Mở từ:
- `frmGiaDinhList` (danh sách gia đình) — nút Thêm (`gxAddEdit1_AddClick`), nút Sửa/nhấp đúp (`GxGiaDinhList.EditRow`, `Source/GXControl/GxGiaDinhList.cs:1026`).
- Có thể mở ở chế độ chỉ xem (`GxOperation.VIEW`) — khi đó ẩn nút Cập nhật/In lý lịch, lưới thành viên không cho sửa (`frmGiaDinh.cs:298-303`).

## 2. Bố cục và các trường

| Nhãn hiển thị | Control | Bắt buộc? | Mặc định | Ghi chú |
|---|---|---|---|---|
| Mã gia đình | `txtMaGiaDinh` (số, tự sinh) hoặc `txtMaGiaDinhRieng` (nhập tay) | có | `Memory.Instance.GetNextId(...)` khi thêm mới (`frmGiaDinh.cs:292`) | Hiện field nào tuỳ config `CF_TUNHAP_MAGIADINH` (`frmGiaDinh.cs:193, 315-325`) |
| Người nam | `txtNguoiChong` (GxGiaoDan, lọc `Phai=Nam`) | không bắt buộc riêng lẻ nhưng phải có ít nhất 1 trong 2 | trống | `ReadOnly=true` — chỉ chọn qua picker, không gõ tay (Designer dòng 493) |
| Người nữ | `txtNguoiVo` (GxGiaoDan, lọc `Phai=Nữ`) | như trên | trống | `ReadOnly=true` (Designer dòng 520) |
| Chủ hộ (2 radio cạnh Người nam/Người nữ) | `rdChuHoNam`, `rdChuHoNu` | có (được ép chọn tự động nếu bỏ trống, xem mục 4) | | |
| Tên gia đình | `txtTenGiaDinh` | có | tự ghép "Tên nam - Tên nữ" khi chọn xong 1 trong 2 người (`frmGiaDinh.cs:434,450`) | `Trim` bỏ ký tự `-` thừa khi lưu (dòng 1453) |
| Giáo họ | `cbGiaoHo` (GxGiaoHo) | có | | Đổi giáo họ ở đây → hỏi xác nhận chuyển giáo họ cho *toàn bộ thành viên* (mục 4) |
| Số hộ khẩu | `txtSoHoKhau` | không | | |
| Diện | `cbDienGiaDinh` (GxDienGiaDinh) | không | | Danh sách cứng 4 giá trị: "Nghèo", "Cận nghèo", "Neo đơn", "Khuyết tật" (`GxDienGiaDinh.cs:20-23`) |
| Điện thoại | `txtDienThoai` | không | | |
| Địa chỉ | `txtDiaChi` | không | | Có nút 🗺️ xem bản đồ cạnh field (`pictureBox1_Click`) |
| Ghi chú | `txtGhiChu` (multi-line) | không | | |
| Đã chuyển đi xứ khác | `chkChuyenXu` | không | false | Tick → hiện Ngày chuyển + Nơi chuyển, hỏi xác nhận (mục 4) |
| Ngày chuyển | `dtNgayChuyen` | có, nếu đã tick chuyển xứ | `IsNullDate=true` | Ẩn khi chưa tick chuyển xứ |
| Nơi chuyển | `txtNoiChuyen` | có, nếu đã tick chuyển xứ | | Ẩn khi chưa tick chuyển xứ |
| Là gia đình không được thống kê | `chkGiaDinhAo` | không | false | Tick → tự đặt Giáo họ = 0 (Ngoài xứ) nếu đang load xong (`frmGiaDinh.cs:2010-2017`) |
| Đã xóa | `chkDelete` | ẩn mặc định | | Chỉ `Visible=true` khi bản ghi đang tải có `DaXoa=true` (dòng 1561-1565) — không có cách tự tay hiện lên |
| Hình gia đình | `gxPictureField1` | không | ảnh mặc định `family_hope_cntr_icon` | Đọc/ghi file ảnh trên đĩa cục bộ (`Memory.AppPath` + tên file) |
| Khối hôn phối | `gxXemHonPhoi1` (GxHonPhoiGiaDinh) | | | Xem/nhập ngày hôn phối, nơi, người chứng, v.v. — UserControl riêng, không đọc sâu trong spec này |
| Ghi chú chân trang | `lblGhiChu` (label tĩnh) | | | "* Ghi chú: Thành viên bị gạch ngang là người đã qua đời, chuyển xứ hoặc lập GĐ riêng" (`frmGiaDinh.cs:43`) |

### Lưới "Các thành viên khác trong gia đình" (`gxGiaoDanList1`)

Thanh công cụ `gxAddEdit1` phía trên lưới có 4 nút với tooltip riêng (`Designer.cs:151-160`):
- **Thêm** (Select): "Thêm một người con được chọn từ danh sách giáo dân có sẵn" → mở `frmChonGiaoDan`.
- **Thêm mới** (Add): "Thêm một người con chưa có trong danh sách giáo dân" → mở `frmGiaoDan` (form giáo dân đầy đủ).
- **Sửa**: "Sửa người con được chọn" → mở `frmGiaoDan` để sửa giáo dân đang chọn.
- **Xóa**: "Xóa người con được chọn" — xóa vĩnh viễn khỏi gia đình (khác xóa mềm).

## 3. Hành vi khi tải

- Focus ban đầu đặt vào `txtMaGiaDinh` (`frmGiaDinh_Load`, dòng 192).
- `choNhapMaRieng` đọc từ config `CF_TUNHAP_MAGIADINH` mỗi lần mở form (dòng 193).
- Khi **thêm mới**: `id = Memory.Instance.GetNextId(GiaDinh, MaGiaDinh, true)` — sinh mã kế tiếp ngay khi mở form, trước khi lưu (dòng 292). Ẩn nút "In lý lịch cá nhân" (Button2) vì chưa có dữ liệu để in.
- Khi **sửa**: hiện nút In lý lịch; nếu `Operation == VIEW` thì ẩn cả nút Cập nhật lẫn nút In lý lịch, và khoá sửa lưới thành viên (`AllowEditGiaoDan = false`).
- Lưới thành viên tải bằng SQL `SELECT_THANHVIEN_GIADINH` lọc `MaGiaDinh = ? AND VaiTro <> 0 AND VaiTro <> 1` (loại chồng/vợ ra khỏi lưới, vì họ hiển thị riêng ở 2 ô Người nam/Người nữ), sắp xếp `ORDER BY VaiTro, NamSinh` (dòng 307).
- Dữ liệu Người nam/Người nữ **không** gán ngay ở `Load` mà gán ở sự kiện `Shown` (`frmGiaDinh_Shown`, dòng 2051) — đọc từ `tblVoChong` đã truy vấn sẵn khi gọi `AssignControlData`.
- Cột ẩn trong lưới thành viên: `DienThoai` (`GiaoDanConst.DienThoai`, dòng 224) và cột nội bộ `GACH` dùng để tô đỏ/gạch ngang (dòng 238) — không hiển thị cho người dùng.
- Cột "Quan hệ GĐ" (`ThanhVienGiaDinhConst.VaiTro`) là cột dropdown chèn vào vị trí đầu tiên (index 0), danh sách giá trị lấy từ `Memory.GetQuanHeList()` (qua `GxGiaDinhList.LoadQuanHeGDText`) — **không đọc được nội dung `Memory.GetQuanHeList()` trong phạm vi các file đã đọc, xem mục 9**.

## 4. Quy tắc nghiệp vụ và kiểm tra dữ liệu

### Khi chọn Người nam / Người nữ (`txtNguoiChong_OnSelecting`, `txtNguoiVo_OnSelecting`)

- Chọn cho ô Người nam mà giới tính là Nữ → chặn, thông báo **"Người chồng không thể là nữ!"** (`frmGiaDinh.cs:554`).
- Chọn cho ô Người nữ mà giới tính là Nam → chặn, thông báo **"Người vợ không thể là nam!"** (dòng 481).
- Người được chọn đang là chồng/vợ trong một gia đình khác còn hiệu lực (chưa xóa, chưa chuyển xứ) → chặn, thông báo lỗi nguyên văn:
  > "Giáo dân này đang làm {người nam (Người chồng)|người nữ (Người vợ)} trong gia đình [{TenGiaDinh}] có mã gia đình là [{MaGiaDinhRieng}] Vui lòng xem lại" (`checkNguoiNamNguoiNuTrongGiaDinhKhac`, dòng 463-466).
- Kiểm tra tuổi kết hôn hợp lệ qua `Memory.checkTuoiKetHon(NgaySinh)` — **logic tuổi cụ thể nằm trong `Memory` (GXGlobal), chưa đọc trong phạm vi màn hình này, xem mục 9**. Nếu không hợp lệ, huỷ chọn.
- Người được chọn đã có vợ/chồng khác (kiểm tra qua `Memory.KiemTraVoChong`) → huỷ chọn (không có thông báo cụ thể ở đây, `Memory.KiemTraVoChong` tự hiện thông báo — **nội dung thông báo chưa đọc, xem mục 9**).
- Có lỗi ngoại lệ khi chọn → **"Lỗi chọn người nữ"** / **"Lỗi chọn người nam"** rồi **đóng luôn cả form** (dòng 540, 609) — hành vi kỳ quặc: một lỗi chọn field lại đóng cả màn hình nhập liệu, có nguy cơ mất dữ liệu đã nhập. Đề xuất bản web: chỉ báo lỗi tại chỗ, không đóng form.

### Khi đổi Người nam/Người nữ mà người cũ đã có trong lưới thành viên hoặc từng có vai trò khác

Hàm `NguoiCu` (dòng 649-919) là logic phức tạp nhất của màn hình — khi thay người nam/nữ khác, hỏi người dùng có muốn:
- **Xóa hẳn** người cũ khỏi gia đình, hoặc
- **Hạ vai trò** người cũ xuống "thành viên" trong lưới bên dưới (và có thể tự đề xuất vai trò Cha/Mẹ/Ông/Bà tuỳ có con cái hay không).

Thông báo tiêu biểu (nguyên văn):
> "Bạn có muốn xóa giáo dân {Tên} ra khỏi gia đình không??.\r\nNếu có chọn [Yes] để xóa.\r\nNếu không chọn [No] để chuyển người này xuống làm thành viên gia đình." (dòng 687-689)

> "Bạn có muốn chương trình tự động cập nhập lại vai trò của {Tên} thành cha của gia đình không?\r\nNếu có chọn [Yes].\r\nNếu không chọn [No]." (dòng 702-704, và các biến thể tương tự cho "mẹ", "ông bà", "chưa rõ" ở các nhánh khác — dòng 722, 746, 765, 785, 812, 833, 859, 878, 897)

**Nhận xét:** đây là một cây quyết định rất sâu (nhiều tầng if lồng nhau, nhiều hộp thoại Yes/No liên tiếp) cố gắng "đoán" quan hệ gia đình mới khi đổi vợ/chồng. Đề xuất bản web: cân nhắc đơn giản hoá — chỉ hỏi 1 câu rõ ràng "Chuyển {Tên} xuống danh sách thành viên hay xoá khỏi gia đình?", để người dùng tự chọn lại vai trò trong lưới thay vì đoán tự động nhiều tầng. Đây là đề xuất, cần người dùng xác nhận trước khi bỏ hành vi cũ.

### Khi thêm thành viên vào lưới (`addGiaoDan`)

- Người được chọn đã là Người nam/Người nữ hiện tại → chặn, **"Giáo dân này đã có trong gia đình"** (dòng 1053).
- Người được chọn đã có trong lưới thành viên → chặn, **"Giáo dân này đã tồn tại trong danh sách thành viên"** (dòng 1061).
- Người được chọn đã thuộc một gia đình khác (còn hiệu lực) với vai trò >1 (tức đã là thành viên/con ở gia đình khác) → hỏi:
  > "Giáo dân [{Tên}] đã thuộc về gia đình [{TênGĐ}].\r\nVui lòng xóa giáo dân [{Tên}] ra khỏi gia đình [{TênGĐ}] trước khi thêm.\r\nBạn có muốn chương trình tự xóa giáo dân [{Tên}] ra khỏi gia đình [{TênGĐ}] không?\r\nChọn [Yes] để chương trình tự xóa.\r\nChọn [No] để xem lại" (dòng 1077-1080). Chọn **No** thì huỷ thêm. (Chọn Yes: xem code, nhánh xử lý xoá thực tế bị **comment lại** — dòng 1085-1089 — nghĩa là nút Yes ở hộp thoại này hiện **không làm gì thêm ngoài cho phép tiếp tục thêm** — có khả năng là lỗi/để dở của bản desktop. Ghi vào mục 9.)
- Người được chọn đã bị đánh dấu **Đã xóa** (`DaXoa=true`) → hỏi:
  > "Giáo dân [{Tên}] đã bị xóa.\r\nNếu thêm giáo dân này vào gia đình thì sẽ khôi phục giáo dân này thành chưa xóa.\r\nChọn [Yes] để tiếp tục thêm giáo dân này vào thành viên gia đình.\r\nChọn [No] để hủy." (dòng 1114-1117). Yes → tự động `UPDATE GiaoDan SET DaXoa=0`.
- Người được chọn đã **chuyển xứ đi** (`DaChuyenXu == -1`) → hỏi 3 lựa chọn Yes/No/Cancel:
  > "Giáo dân [{Tên}] đã chuyển đi xứ khác.\r\nBạn có muốn chuyển giáo dân này về lại xứ không.\r\nChọn [Yes] nếu có.\r\nChọn [No] nếu không.\r\nChọn [Cancel] để hủy bỏ thêm giáo dân." (dòng 1127-1131)

### Khi bấm Xóa một thành viên trong lưới (`gxAddEdit1_DeleteClick`)

> "Bạn có thực sự muốn xóa vĩnh viễn giáo dân này ra khỏi gia đình.\r\nChọn [Yes] để xóa.\r\nChọn [No] để thoát." (dòng 1198) — **xóa vĩnh viễn khỏi bảng `ThanhVienGiaDinh`, không phải xóa mềm.**

### Khi bấm Cập nhật (`checkInput`, gọi từ `gxCommand1_OnOK` → `updateGiaDinh`)

Thứ tự kiểm tra (mỗi lỗi dừng lại, focus vào field lỗi):
1. `txtMaGiaDinh` phải là số → **"Mã gia đình phải được nhập số"** (dòng 1231).
2. Cả Người nam lẫn Người nữ đều trống → **"Hãy nhập ít nhất người nam hoặc người nữ!"** (dòng 1237).
3. Chỉ có Người nam (không có Người nữ) mà người nam đã qua đời/chuyển xứ → hỏi xác nhận: **"Người nam (Người chồng) đã qua đời hoặc đã chuyển xứ. Bạn có muốn tiếp tục không?\r\nChọn [Yes] để tiếp tục.\r\nChọn [No] để kiểm tra lại."** (dòng 1246). Tương tự cho chỉ có Người nữ (dòng 1258) và cho cả hai người (dòng 1270) — **lưu ý: nhánh "chỉ có Người nữ" (dòng 1256) gọi nhầm `checkQuadoiOrChuyenxu(txtNguoiChong.MaGiaoDan...)` thay vì `txtNguoiVo` — có khả năng là lỗi copy-paste trong bản desktop.** Ghi rõ để bản web sửa đúng (kiểm tra người vợ, không phải người chồng) và cần xác nhận với người dùng trước khi coi đây là "sửa lỗi" chính thức.
4. Đang **Thêm mới** và cặp Người nam + Người nữ này đã từng được lập một gia đình khác → chặn: **"Người nam và người nữ này đã từng được lập thành một gia đình.\r\n(Mã gia đình: {Mã})\r\nKhông thể lập thêm 1 gia đình cho cùng 2 người này!"** (dòng 1285-1286).
5. Nếu cho nhập mã riêng mà bỏ trống → **"Hãy nhập mã gia đình!"** (dòng 1294).
6. Tên gia đình trống → **"Hãy nhập tên gia đình!"** (dòng 1301).
7. Chưa chọn Giáo họ → **"Hãy chọn một giáo họ!"** (dòng 1308).
8. Đang sửa và Giáo họ bị đổi khác giáo họ ban đầu → hỏi xác nhận: **"Nếu chọn chuyển họ cho gia đình này thì tất cả các thành viên trong gia đình cũng sẽ bị chuyển theo\r\nBạn có chắc chuyển họ cho gia đình này không?\r\nChọn [Yes]: Đóng màn hình và lưu thông tin được nhập\r\nChọn [No]: không đóng màn hình và nhập lại thông tin"** (dòng 1315-1319).
9. Chọn Giáo họ ≠ "Ngoài xứ" (id > 0) nhưng KHÔNG tick "gia đình không được thống kê" → hỏi xác nhận vì thường chỉ gia đình không thống kê mới chọn "Ngoài xứ" (**thông báo hơi ngược nghĩa với logic — xem nguyên văn dòng 1328**, chưa hoàn toàn khớp câu chữ với điều kiện, ghi vào mục 9 để hỏi lại người dùng).
10. Chọn Giáo họ = "Ngoài xứ" (id ≤ 0) NHƯNG có tick "gia đình không được thống kê" → hỏi xác nhận tương tự (dòng 1338).
11. Nếu không cho nhập mã riêng và mã gia đình đã tồn tại (đang Thêm) → tự động sinh mã mới, không báo lỗi (dòng 1349-1352).
12. Nếu cho nhập mã riêng và mã riêng trùng với gia đình khác → chặn: **"Mã gia đình này đã tồn tại. Hãy nhập mã khác!"** (dòng 1358).
13. **Xác định Chủ hộ** — logic khá tinh vi (dòng 1364-1443):
    - Nếu chỉ có 1 trong 2 người (nam hoặc nữ) → tự động chọn người đó làm chủ hộ, không hỏi.
    - Nếu người nam đã qua đời, người nữ còn sống, chưa ai được chọn/hoặc chọn sai → hỏi xác nhận 3 lựa chọn (Yes/No/Cancel):
      > "Vì người nam đã qua đời và người nữ còn sống, chương trình sẽ tự chọn chủ hộ là người nữ.\r\nChọn [Yes] để lưu chủ hộ là người nữ\r\nChọn [No] để lưu chủ hộ như đang chọn.\r\nChọn [Cancel] để giữ lại màn hình này và xem lại" (dòng 1382-1383)
    - Tương tự chiều ngược lại (dòng 1397-1398), và trường hợp cả hai đã qua đời (dòng 1415-1416), và trường hợp chưa chọn chủ hộ nào (dòng 1429-1430) — mỗi trường hợp một câu thông báo riêng, chọn **Cancel** đều huỷ lưu và giữ nguyên màn hình.

### Sau khi lưu thành công, còn hỏi thêm (không thuộc `checkInput`, nằm trong `updateGiaDinh`)

- Đang sửa, trước đó gia đình đã "chuyển xứ" mà giờ bỏ tick → hỏi: **"Tất cả các thành viên trong gia đình này sẽ được chuyển về lại xứ. Bạn có chắc không?"** (OKCancel, dòng 1609) — Cancel thì huỷ lưu toàn bộ.
- Đổi Địa chỉ khác địa chỉ cũ và gia đình có ≥1 thành viên trong lưới → hỏi: **"Bạn có muốn cập nhật địa chỉ gia đình cho tất cả các thành viên trong gia đình không?"** (Yes/No, dòng 1764) — Yes thì `UPDATE` địa chỉ hàng loạt cho mọi giáo dân thuộc gia đình.
- Đổi trạng thái "không được thống kê" → hỏi: **"Bạn có muốn cập nhật tất cả các thành viên trong gia đình thành {không được thống kê|được thống kê} không?"** (Yes/No, dòng 1770) — Yes thì `UPDATE GiaoDan SET GiaoDanAo=...` hàng loạt.
- Tick "Đã chuyển đi xứ khác" lần đầu (không phải đang tải dữ liệu cũ) → hỏi trước khi cho tick: **"Nếu chọn chuyển xứ cho gia đình này thì tất cả các thành viên trong gia đình cũng sẽ bị chuyển đi\r\nBạn có chắc chuyển xứ cho gia đình này không?"** (`chkChuyenXu_CheckedChanged`, dòng 1999) — No thì tự bỏ tick lại.
- Bấm nút bản đồ mà Địa chỉ trống → **"Xin vui lòng nhập địa chỉ để xem bản đồ"** (dòng 2034).
- Bấm Hủy (Cancel) khi đang **Thêm mới** mà đã nhập gì đó (chọn người nam/nữ hoặc có thành viên trong lưới) → hỏi 3 lựa chọn:
  > "Bạn có muốn lưu gia đình này không?\r\nChọn [Yes] để lưu và đóng màn hình.\r\nChọn [No] để đóng màn hình và không lưu.\r\nChọn [Cancel] để quay trở lại màn hình này và xem lại." (dòng 1978)
- Lỗi ngoại lệ khi lưu → **"Lỗi Exception (frmGiaDinh, gxCommand1_OnOK)"** kèm `ex.Message` (dòng 1785, không phải thông báo nghiệp vụ, là bẫy lỗi hệ thống).
- Lỗi cập nhật chuyển xứ hàng loạt thất bại → thông báo dài:
  > "Có lỗi không mong muốn xảy ra.\r\nCập nhật chuyển xứ cho các thành viên trong gia đình thất bại\r\nXin vui lòng thử lại lần nữa hoặc liên hệ với tác giả để được khắc phục\r\nThành thật xin lỗi quý vị!\r\n{Chi tiết lỗi}" (dòng 1968).

## 5. Thao tác người dùng

| Thao tác | Điều kiện bật/tắt | Hành động |
|---|---|---|
| Nút "Cập nhật" (OK) | Ẩn khi `Operation == VIEW` (dòng 300) | Chạy toàn bộ `checkInput` + `updateGiaDinh`, đóng form nếu OK |
| Nút "In phiếu G.Đ" (Button1) | Luôn hiện | Lưu trước rồi in sổ gia đình (`GxGiaDinhList.InSoGiaDinh`) — **chỉ in được sau khi đã lưu thành công** |
| Nút "In lý lịch cá nhân" (Button2) | Ẩn khi Thêm mới hoặc VIEW (dòng 293, 301) | Xuất lý lịch cá nhân của Người nam + Người nữ + toàn bộ thành viên trong lưới; nếu không có ai → **"Không có thành viên nào trong gia đình"** (dòng 127) |
| Nút bản đồ cạnh Địa chỉ | | Mở bản đồ ngoài (`Memory.ViewMap`) |
| Nút X (xoá) cạnh Người nam/Người nữ | | Xoá người đang chọn khỏi ô đó, refresh khối hôn phối |
| Nhấp đúp dòng trong lưới thành viên | | Mở `frmGiaoDan` để sửa giáo dân đó (`EditConCaiRow`), đồng thời set `Row = -3` sau đó (dòng 947 — **ý nghĩa của `-3` không rõ, xem mục 9**) |
| Đóng form (nút X / Cancel) | Chỉ hỏi xác nhận khi đang Thêm mới và đã có dữ liệu | Xem quy tắc "Bấm Hủy" ở mục 4 |

## 6. Lưới dữ liệu (nếu có)

Lưới thành viên (`gxGiaoDanList1`, kiểu `GxGiaoDanList` — không đọc riêng file này trong phạm vi nhiệm vụ, chỉ đọc phần cấu hình cột được chèn thêm trong `frmGiaDinh.cs`):

| Cột | Nguồn | Ghi chú |
|---|---|---|
| Quan hệ GĐ | `ThanhVienGiaDinh.VaiTro` | Dropdown, danh sách từ `Memory.GetQuanHeList()`, chèn ở vị trí đầu (index 0) |
| (các cột mặc định của `GxGiaoDanList`) | | Không đọc chi tiết trong spec màn hình này |
| Điện thoại | `GiaoDan.DienThoai` | **Ẩn** (`Visible = false`, dòng 224) |
| GACH | cột nội bộ, không bind DB thật | **Ẩn**, dùng để tô đỏ + gạch ngang dòng khi `GACH = -1` (người đã qua đời/chuyển xứ/lập gia đình riêng — theo `Memory.IsRedGiaoDan`) |

Không thấy sắp xếp lại cột theo yêu cầu người dùng được lưu riêng cho màn hình này (dựa trên `AutoLoadGridFormat = true` ở Designer — GridEX tự lưu/khôi phục bố cục theo cấu hình chung của control, không đọc sâu thêm).

## 7. Liên kết sang màn hình khác

- Mở `frmChonGiaoDan` (nút Thêm/Select ở lưới thành viên).
- Mở `frmGiaoDan` (nút Thêm mới, nút Sửa ở lưới thành viên; nhấp đúp dòng).
- Gọi `frmGiaoDan.GetGiaoDanRowByChuyenXu` / `GetChuyenXuInfo` (tĩnh) khi xử lý chuyển xứ hàng loạt cho thành viên.
- Gọi `GxGiaoDanList.XuatLyLichCaNhan` (in lý lịch cá nhân — export tài liệu ngoài).
- Gọi `GxGiaDinhList.InSoGiaDinh` (in sổ/phiếu gia đình — export tài liệu ngoài).
- Trả `DataReturn` (DataRow gia đình vừa lưu) cho form gọi nó (`frmGiaDinhList`) qua `DialogResult.OK`.

## 8. Khác biệt cố ý ở bản web

*(Phần này để điền khi migrate — xem mục 10 bên dưới cho hiện trạng thực tế đã migrate, một số khác biệt hiện tại là do CHƯA LÀM chứ không phải cố ý.)*

## 9. Chỗ chưa chắc

- Nội dung chính xác của `Memory.GetQuanHeList()` (danh sách vai trò trong gia đình: Con, Cháu, Cha, Mẹ, Ông, Bà, ...) — nằm trong `Source/DBAccess` (GXGlobal), chưa đọc trong nhiệm vụ này. Có một khối comment cũ trong `frmGiaDinh.cs` (dòng 992-1018) liệt kê 21 giá trị mẫu (Chồng=0, Vợ=1, Con=2, Cháu=3, Cha=4, Mẹ=5, Ông=6, Bà=7, Anh=8, Cô=9, Chú=10, Bác=11, Cậu=12, Dì=13, Mợ=14, Thím=15, Dượng=16, Chị=17, Em=18, Dâu=19, Rể=20, Chưa rõ=100) nhưng đây là **mã đã comment, không chắc còn khớp với dữ liệu thật hiện tại** (có ghi chú "hiepdv begin/end add" cho thấy đã bị sửa nhiều lần).
- Nội dung thông báo lỗi bên trong `Memory.KiemTraVoChong`, `Memory.checkTuoiKetHon`, `Memory.IsRedGiaoDan` — các hàm dùng chung nằm ngoài phạm vi 2 màn hình được giao, chưa đọc.
- Điều kiện dòng 1326-1334 ("Thường thì chỉ có gia đình không được thống kê mới chọn giáo họ là [Ngoài xứ]...") — câu chữ thông báo có vẻ không khớp hoàn toàn logic điều kiện xung quanh nó; cần hỏi người phụ trách nghiệp vụ trước khi khẳng định đây là chủ ý.
- Dòng 1256 gọi `checkQuadoiOrChuyenxu(txtNguoiChong.MaGiaoDan...)` trong nhánh xử lý "chỉ có Người nữ" — nghi là lỗi copy-paste (nên là `txtNguoiVo.MaGiaoDan`) nhưng chưa xác nhận được ý định ban đầu của tác giả.
- Ý nghĩa của `gxGiaoDanList1.Row = -3` sau khi sửa một dòng thành viên (dòng 947) — không rõ mục đích (có thể để bỏ chọn dòng, tương tự `Row = -2` dùng ở chỗ khác để refresh sau xoá).
- Nhánh Yes trong hộp thoại "Bạn có muốn chương trình tự xóa giáo dân [...] ra khỏi gia đình [...] không?" (dòng 1077-1090): mã xử lý thực tế (xóa bản ghi `ThanhVienGiaDinh` cũ) đã bị **comment lại**, nghĩa là chọn Yes ở đây trong bản build hiện tại **không tự xóa gì cả**, chỉ cho phép tiếp tục — cần xác nhận đây là bug đã biết hay đã được xử lý ở chỗ khác mà spec này chưa thấy.
- Định nghĩa SQL thật của view Access `SELECT_GIADINH_LIST` (dùng trong nhiều câu SELECT) nằm trong file .mdb/.accdb, không có trong mã nguồn — nên **không xác định được thứ tự sắp xếp mặc định thật sự** khi tải danh sách/chi tiết gia đình từ Access. `SqlConstants.cs` chỉ giữ một bản ghi chú (đã bị comment) của định nghĩa cũ dùng TRANSFORM/PIVOT, có thể không còn đúng.

## 10. Đối chiếu bản web hiện tại

So với `WebApp/src/web/src/screens/GiaDinhDetail.tsx`, `WebApp/src/Qlgx.Api/Services/GiaDinhService.cs`, `WebApp/src/Qlgx.Api/Endpoints/GiaDinhEndpoints.cs`.

| Hành vi bản desktop | Bản web đã có? | Ghi chú |
|---|---|---|
| Xem thông tin gia đình (tên, giáo họ, điện thoại, địa chỉ, ghi chú, diện, số hộ khẩu, chuyển xứ, không thống kê) | **Có** | `GiaDinhDetail.tsx` có đủ field tương ứng, `GET /api/gia-dinh/{id}` trả đủ |
| Sửa và lưu các trường trên (PUT) | **Có** | `PUT /api/gia-dinh/{id}` cập nhật đúng các trường, có kiểm soát `RowVersion` (đồng thời/concurrency) — desktop KHÔNG có cơ chế phát hiện đụng độ này, đây là điểm bản web LÀM TỐT HƠN |
| Thêm gia đình mới | **Backend có, UI chưa nối** | `POST /api/gia-dinh` tạo bản ghi trống (Tên + Giáo họ), sinh `MaGiaDinhCu` qua `SinhMaService` và `MaNhanDang` mới — xem `GiaDinhService.Tao`, `GiaDinhGhiTests`. Nút "Thêm gia đình" trên UI vẫn khoá cứng nút Cập nhật — nối luồng giao diện (chọn Người nam/nữ trước khi có gì để lưu) là việc của lượt sau |
| Xóa gia đình (mềm hoặc vĩnh viễn) | **Backend có, UI chưa nối** | `DELETE /api/gia-dinh/{id}?vinhVien=` — đúng 2 lựa chọn Yes(vĩnh viễn, xoá cả `ThanhVienGiaDinh`)/No(mềm) của hộp thoại gốc, KHÔNG có điều kiện chặn nào (khác giáo dân) — xem `GiaDinhService.Xoa`. Chưa có nút Xoá trên `GiaDinhList.tsx` (lượt sau) |
| Chọn/đổi Người nam, Người nữ qua picker thật (tìm giáo dân, kiểm tra giới tính, kiểm tra đã có gia đình khác, kiểm tra tuổi kết hôn...) | **Backend có đầy đủ, UI chưa nối** | `PUT /api/gia-dinh/{id}/vo-chong/{vaiTro}` kiểm đủ: giới tính đúng thông báo nguyên văn, đang là vợ/chồng gia đình khác còn hiệu lực (chặn, thông báo có tên+mã gia đình), tuổi kết hôn (<14 chặn/14-17 cảnh báo, đúng `checkTuoiKetHon`), đã từng kết hôn với ai còn sống (`KiemTraVoChong`, cảnh báo) — xem `GiaDinhService.GanVoChong`, `GiaDinhGhiTests`, can-review-sau.md mục 22. Cây quyết định `NguoiCu` (đổi người thì người cũ đi đâu) là logic giao diện của LƯỢT SAU — server chỉ nhận ý định cuối (`XuLyNguoiCuDto`) và thực hiện nguyên tử. `GxPicker` trong `GiaDinhDetail.tsx` (dùng cho Người nam/nữ) vẫn CHƯA nối các sự kiện chọn — đó là việc của lượt sau, chỉ hạ tầng picker dùng chung (component + `GET /api/giao-dan/tim`) đã có, đã nối xong ở Tên Cha/Mẹ của màn hình giáo dân |
| Đổi Giáo họ + cảnh báo "chuyển tất cả thành viên theo" | **Danh mục thật đã có, cảnh báo cascade vẫn thiếu** | `GET /api/giao-ho` trả danh mục thật; `GiaDinhDetail.tsx` nay dùng `giaoHoId` thật (state riêng, không còn giữ nguyên giá trị cũ) và gửi đúng lên `PUT /api/gia-dinh/{id}` khi lưu — xem can-review-sau.md mục 19. Vẫn KHÔNG có cảnh báo "chuyển tất cả thành viên theo" (cascade) như desktop — đó vẫn là việc chưa làm |
| Chọn Chủ hộ | **Thiếu** | Có radio "Chủ hộ" trên UI (`defaultChecked` theo dữ liệu tải về) nhưng `xuLySubmit` không đọc giá trị này để gửi lên — không thể đổi chủ hộ qua web |
| Thêm/sửa/xóa "thành viên khác" (con cái...) trong lưới | **Backend có, UI chưa nối** | `POST /api/gia-dinh/{id}/thanh-vien` (thêm, đúng thứ tự kiểm tra `addGiaoDan`: đã là vợ/chồng hiện tại → chặn; đã trong lưới → chặn; thuộc gia đình khác → cảnh báo rồi VẪN CHO THÊM không xoá, đúng lỗi gốc — can-review-sau.md mục 2; đã xoá mềm → cảnh báo rồi tự khôi phục; đã chuyển xứ → cần quyết định riêng) và `DELETE /api/gia-dinh/{id}/thanh-vien/{giaoDanId}/{vaiTro}` (xoá VĨNH VIỄN, đúng can-review-sau.md mục 5) — xem `GiaDinhService.ThemThanhVien`/`XoaThanhVien`, `GiaDinhGhiTests`. `GxGiaoDanList` trong `GiaDinhDetail.tsx` vẫn chỉ hiển thị, chưa có nút Thêm/Sửa/Xóa nối API — việc của lượt sau |
| Đổi vai trò (Quan hệ GĐ) của từng thành viên | **Thiếu** | Không có UI/endpoint |
| Cảnh báo tick "Đã chuyển đi xứ khác" → ảnh hưởng mọi thành viên | **Thiếu** | Checkbox chỉ toggle hiện/ẩn Ngày chuyển + Nơi chuyển, không có hộp thoại xác nhận, không có logic chuyển xứ hàng loạt cho thành viên ở backend |
| Cảnh báo đổi Địa chỉ → hỏi cập nhật địa chỉ hàng loạt cho thành viên | **Thiếu** | Không có |
| Cảnh báo đổi "Không thống kê" → hỏi cập nhật hàng loạt cho thành viên | **Thiếu** | Không có |
| In phiếu gia đình / In lý lịch cá nhân / In chứng nhận hôn phối | **Thiếu** | Các nút "In lý lịch cá nhân", "In phiếu gia đình" trên `GiaDinhDetail.tsx` không có `onClick` — chỉ là nút tĩnh |
| Xem bản đồ theo địa chỉ | **Thiếu** | Nút "Bản đồ" cạnh ô Địa chỉ không có `onClick` |
| Ảnh gia đình | **Thiếu** | Khối "Hình gia đình" chỉ hiện placeholder tĩnh "Chưa có hình / Nhấp để tải ảnh lên", không có input file, không upload |
| Khối hôn phối (ngày, nơi, người chứng, cách thức) | **Có ở tầng dữ liệu, thiếu UI sửa** | `GiaDinhService` đọc/ghi hôn phối đầy đủ (kể cả xử lý concurrency riêng cho hôn phối — `KetQuaCapNhatGiaDinh.DungPhienBanHonPhoi`) nhưng `GiaDinhDetail.tsx` luôn gửi `honPhoi: null` — nghĩa là **web hiện tại không có form nào để sửa khối hôn phối**, dù backend đã sẵn sàng |
| Mã gia đình riêng (`choNhapMaRieng`, cấu hình) | **Thiếu** | Web luôn hiển thị `maGiaDinhCu` (số cũ, disabled), không có chế độ nhập mã riêng theo config |
| Validate: bắt buộc ít nhất 1 trong 2 người, tên gia đình, giáo họ, mã gia đình... | **Thiếu ở frontend, một phần ở backend** | `GiaDinhDetail.tsx` không có validate client-side nào tương đương `checkInput`; chưa xác nhận được các ràng buộc này có được validate ở `GiaDinhService.CapNhat` hay không — đọc code không thấy kiểm tra tương đương, có khả năng backend chấp nhận cả tên gia đình rỗng, giáo họ null, v.v. |
| Xác nhận trước khi rời trang khi có thay đổi chưa lưu (khi Thêm mới) | **Thiếu** | Không thấy `beforeunload` hay xác nhận tương tự |
| Cột lưới danh sách gia đình (tham chiếu chéo, xem spec danh sách) | Xem `gia-dinh-danh-sach.md` mục 10 | |

### Ưu tiên các thiếu sót (ảnh hưởng tới việc bỏ hẳn bản desktop)

**Cao — chặn hẳn việc bỏ bản desktop cho nghiệp vụ "gia đình":**
1. ~~Không có picker chọn/đổi Người nam, Người nữ...~~ **Backend xong (2026-09-06)** —
   `PUT /api/gia-dinh/{id}/vo-chong/{vaiTro}` kiểm đủ mọi quy tắc; còn thiếu GIAO DIỆN (nối
   `GxPicker` vào ô Người nam/nữ + cây quyết định `NguoiCu`) — việc của lượt sau.
2. ~~Không có màn hình/luồng thêm mới gia đình...~~ **Backend xong (2026-09-06)** —
   `POST /api/gia-dinh`; còn thiếu GIAO DIỆN mở khoá nút "Cập nhật" cho bản ghi mới.
3. ~~Không thể thêm/sửa/xóa thành viên...~~ **Backend xong (2026-09-06)** —
   `POST`/`DELETE /api/gia-dinh/{id}/thanh-vien`; còn thiếu GIAO DIỆN (nút Thêm/Sửa/Xóa trên
   lưới thành viên) — việc của lượt sau. "Sửa" một thành viên (đổi vai trò tại chỗ, không xoá
   rồi thêm lại) CHƯA có endpoint riêng — cần thêm nếu lượt sau cần.
4. Không in được phiếu gia đình / lý lịch cá nhân / chứng nhận hôn phối — các bản in giấy tờ giáo xứ cần hàng ngày. Vẫn thiếu, ngoài phạm vi nhiệm vụ "ghi gia đình" (thuộc giai đoạn in ấn).

**Trung bình:**
5. Không có form sửa khối hôn phối dù backend đã hỗ trợ đầy đủ (chỉ thiếu UI) — phí phần đã làm.
6. Đổi Giáo họ qua UI không có tác dụng thật (do thiếu danh mục giáo họ có Id) — dễ gây hiểu lầm cho người dùng khi họ tưởng đã đổi.
7. Không cảnh báo/cascade khi đổi Địa chỉ, "Không thống kê", "Chuyển xứ" cho các thành viên — có thể dẫn tới dữ liệu con cái "lệch" so với dữ liệu cha mẹ mà không ai để ý.
8. Không có validate bắt buộc ở form (tên gia đình, giáo họ...) — rủi ro lưu dữ liệu rỗng/thiếu.

**Thấp:**
9. Không có upload ảnh gia đình.
10. Không có nút Xem bản đồ hoạt động.
11. Không hỗ trợ chế độ "mã gia đình riêng" theo cấu hình.
