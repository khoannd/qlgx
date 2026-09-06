# Màn hình: Danh sách giáo dân (`frmGiaoDanList.cs`)

| | |
|---|---|
| Tệp nguồn | `Source/ChuongTrinh/frmGiaoDanList.cs` (357 dòng) + `.Designer.cs` |
| UserControl dùng lại | `GxControl.GxGiaoDanList` (lưới, `Source/GXControl/GxGiaoDanList.cs`, kế thừa `GxGrid`), `GxControl.GxGiaoHo` (combo chọn giáo họ kèm tự tải lưới, `Source/GXControl/GxGiaoHo.cs`), `GxControl.GxAddEdit` (thanh nút Thêm/Sửa/Xóa/In…), `GxControl.GxCommand` (thanh Đóng) |
| Bảng dữ liệu đụng tới | `GiaoDan`, `GiaoHo` (JOIN lấy `TenGiaoHo`), `ChuyenXu` (JOIN suy `DaChuyenXu`), gián tiếp `ThanhVienGiaDinh`, `BiTichChiTiet`, `ChiTietLopGiaoLy` (khi xóa vĩnh viễn) |
| Trạng thái migrate | xong (web: `GiaoDanList.tsx` + `GxGiaoDanList.tsx`) — nhưng nhiều hành vi desktop chưa có, xem mục 10 |

## 1. Mục đích

Liệt kê toàn bộ giáo dân của giáo xứ, lọc theo giáo họ, cho phép thêm/sửa/xóa, xem gia đình,
in các loại chứng nhận bí tích và giấy giới thiệu, xem vị trí địa chỉ trên bản đồ. Đây là màn
hình trung tâm quản lý con người, mở từ menu chính (`frmMain.cs`, chưa migrate ở đợt này) và có
thể được nhúng làm popup chọn giáo dân (`gxAddEdit1.DisplayMode = Full`, `SelectButton.Visible
= false` ở đây nên chế độ chọn bị tắt trong cách dùng hiện tại).

## 2. Bố cục và các trường

Phần đầu màn hình chỉ có bộ lọc, không phải trường dữ liệu nhập:

| Điều khiển | Nhãn hiển thị | Ghi chú |
|---|---|---|
| `cbGiaoHo` (GxGiaoHo) | "Giáo họ" | Combo, mặc định rỗng cho tới khi `OnLoad` gán; có mục "Tất cả" (`HasShowAll=true`, `MaGiaoHo=-1`) |
| `chkGiaoDanAo` | "Chỉ xem giáo dân không được thống kê" | Checkbox, tooltip: "Giáo dân không được thống kê là giáo dân ngoài xứ, không được tính trong các mục thống kê" (`frmGiaoDanList.Designer.cs:167`) — **tooltip này không chính xác**, xem mục 4 |
| `lblTotal` | "Tổng cộng:" rồi được ghi đè bằng "`<n>` giáo dân" | Cập nhật ở `RowCountChanged`/`LoadDataFinished` (`frmGiaoDanList.cs:113,135`) |

Lưới `gxGiaoDanList1` chiếm phần lớn màn hình — xem mục 6.

## 3. Hành vi khi tải

- `OnLoad` (`frmGiaoDanList.cs:153-171`): gọi `FormatGrid()` để dựng 29 cột, bật `cbGiaoHo.HasShowAll`
  và `AutoLoadGrid`. Nếu bộ nhớ toàn cục `Memory.GetMemory(GxConstants.DangTimKiemGiaDinhGiaoDan)`
  là `null` (tức không phải vừa quay về từ màn hình Tìm giáo dân/Tìm gia đình — `frmTimGiaoDan.cs`,
  `frmTimGiaDinh.cs`) thì gán `cbGiaoHo.MaGiaoHo = Memory.CurrentGiaoHo` nếu > 0 — **giáo họ được
  chọn lần mở màn hình trước được nhớ lại** (biến tĩnh `Memory.CurrentGiaoHo`, cập nhật ở
  `GxGiaoHo.cs:238-239` mỗi khi người dùng đổi combo). Ngược lại (đang có kết quả tìm kiếm) thì gọi
  thẳng `gxGiaoDanList1.LoadData()` không tham số.
- **Việc gán `cbGiaoHo.MaGiaoHo` kích hoạt tải dữ liệu thật sự**, không phải hàm
  `LoadGiaoDanList()` định nghĩa ở dòng 173-182 — hàm này **không hề được gọi ở đâu trong file**
  (đã kiểm bằng grep toàn bộ mã nguồn) → là code chết. Logic tải thật nằm trong
  `GxGiaoHo.LoadGridData()` (`Source/GXControl/GxGiaoHo.cs:261-286`), được kích hoạt qua sự kiện
  đổi combo/`AutoLoadGrid`.
- Câu lệnh và điều kiện lọc mặc định thực sự dùng (từ `GxGiaoHo.LoadGridData`, dòng 266-285):
  - Nền: `" AND DaXoa=0 AND DaChuyenXu=0 AND QuaDoi=0 "` (giáo dân chưa xóa, chưa chuyển xứ, **và
    chưa qua đời** — khác với where trong hàm chết `LoadGiaoDanList()` vốn không loại `QuaDoi`).
  - Nếu đã chọn một giáo họ cụ thể (`MaGiaoHo > -1`): thêm
    `" AND (MaGiaoHo={0} OR MaGiaoHoCha={0}) "` — **gồm cả giáo dân thuộc giáo xóm con của giáo
    họ đó**, không chỉ giáo dân gán trực tiếp giáo họ này.
  - Nếu giáo họ đang chọn khác "Ngoài xứ" (`MaGiaoHo != 0`): thêm lọc theo `chkGiaoDanAo` —
    `IsAo=False` (mặc định, checkbox chưa tick) → `AND GiaoDanAo=0`; tick vào → `AND GiaoDanAo=-1`.
  - Câu SELECT gốc: `SqlConstants.SELECT_GIAODAN_LIST_CO_GIAOHO`
    (`Source/DBAccess/SqlConstants.cs:134-144`). Truy vấn con có `ORDER BY GiaoDan.MaGiaoDan ASC`
    nhưng truy vấn ngoài (`SELECT GiaoDan.* FROM (...) AS GiaoDan WHERE 1`) **không lặp lại
    ORDER BY** — trong Access điều này thường (không đảm bảo theo chuẩn SQL) giữ thứ tự của
    subquery, nên **thứ tự mặc định trên lưới là theo Mã giáo dân tăng dần**, không phải theo
    tên. Người dùng có thể click tiêu đề cột để sắp lại (GridEX cho phép sort trên lưới).
  - Cột `NamSinh` được tính trong SQL (`RIGHT(NgaySinh,4)`, dòng 136) nhưng **không map vào cột
    lưới nào cả** — xác nhận: 29 cột của `FormatGrid()` (mục 6) không có `NamSinh`. Đây là cột
    SQL chết, chỉ tốn công tính mà không hiển thị.
- Con trỏ: không có `.Focus()` cho lưới hay ô lọc khi mở form — `frmGiaoDanList_Load` để trống
  (`frmGiaoDanList.cs:348-351`).

## 4. Quy tắc nghiệp vụ và kiểm tra dữ liệu

- **Xóa giáo dân** (`gxAddEdit1_DeleteClick`, `frmGiaoDanList.cs:223-286`): hộp thoại 3 lựa chọn
  `MessageBoxButtons.YesNoCancel` với nội dung nguyên văn:
  > "Bạn muốn xóa vĩnh viễn giáo dân được chọn không?\r\nChọn [Yes] để xóa vĩnh viễn.\r\nChọn [No]
  > để đưa vào hồ sơ lưu trữ.\r\nChọn [Cancel] hủy bỏ việc xóa." (dòng 227)
  - **[No] = xóa mềm**: chạy `SqlConstants.DELETE_GIAODAN` (thực chất là `UPDATE ... SET DaXoa=-1`,
    chưa xác nhận nội dung SQL chính xác — xem "Chỗ chưa chắc").
  - **[Yes] = xóa vĩnh viễn**: trước tiên gọi `checkGiaoDanTrongGiaDinh(maGiaoDan, tenGiaoDan)`
    (`GxGiaoDanList.cs:915-945`) — nếu giáo dân này đang là thành viên (chồng/vợ/con) của bất kỳ
    gia đình nào thì **chặn xóa** và hiện thông báo liệt kê từng gia đình:
    > "Giáo dân `<tên>`\r\nGiữ vai trò là người nam/người nữ/thành viên gia đình trong gia đình
    > [`<tên gia đình>`] có mã gia đình là [`<mã>`] \r\n...\r\nVui lòng xóa giáo dân ra khỏi gia
    > đình trước khi xóa giáo dân này" (tiêu đề "Thông báo lỗi", `GxGiaoDanList.cs:918-941`).
    Nếu không vướng gia đình nào, xóa tuần tự ở 4 bảng: `GiaoDan`, `ThanhVienGiaDinh`,
    `BiTichChiTiet`, `ChiTietLopGiaoLy` (`frmGiaoDanList.cs:251-274`) — **không dùng transaction**,
    nếu một lệnh giữa chừng lỗi thì dữ liệu có thể xóa dở dang.
  - Sau khi xóa, gọi `CurrentRow.Delete()` + `Refetch()` + `Refresh()` + đặt `Row = -2`
    (dòng 277-280) — bỏ chọn dòng hiện tại.
- **Thêm giáo dân** (`gxAddEdit1_AddClick`, dòng 184-211): mở `frmGiaoDan` mới; nếu đang lọc theo
  một giáo họ cụ thể thì gán sẵn `MaGiaoHo` cho form con. Sau khi lưu (`DialogResult.OK` và có
  `DataReturn`), dùng `goto cont` để **lặp lại vòng thêm mới ngay lập tức** (mở tiếp một
  `frmGiaoDan` trống) — cho phép nhập liên tục nhiều giáo dân không cần bấm lại nút Thêm mỗi lần.
  Dòng mới được `ImportRow` vào `DataTable` rồi `Refetch()` và `FindAll` để cuộn tới đúng dòng vừa
  thêm theo `MaGiaoDan`.
- **Sửa giáo dân**: double-click dòng hoặc nút Sửa → `gxGiaoDanList1.EditRow()`
  (`GxGiaoDanList.cs:550-588`) mở `frmGiaoDan` ở chế độ `EDIT`.
- **In danh sách** (nút In trên `gxAddEdit1`, đã đổi nhãn thành xuất Excel): xuất toàn bộ lưới
  hiện tại (theo bộ lọc/sort đang áp dụng) ra file `.xls` tạm rồi mở bằng chương trình mặc định
  của hệ điều hành (`btnInDanhSach_Click`, dòng 329-341) — phụ thuộc máy trạm có Excel/trình đọc
  xls cài sẵn, không phù hợp với mô hình máy chủ tập trung.
- **Tooltip "chỉ xem giáo dân không được thống kê"** ghi "giáo dân ngoài xứ" nhưng bản thân code
  tách biệt hai khái niệm: `GiaoDanAo` (không thống kê) và `MaGiaoHo=0` (Ngoài xứ) là hai cột độc
  lập — một giáo dân có thể ở giáo họ thật nhưng vẫn đánh dấu "ảo", hoặc ngược lại. Đây là **tooltip
  sai/gây hiểu lầm** trong bản desktop (`frmGiaoDanList.Designer.cs:167-168`).

## 5. Thao tác người dùng

Thanh `gxAddEdit1` (constructor `frmGiaoDanList.cs:50-93`):

| Nút | Nhãn/Tooltip | Hành động |
|---|---|---|
| Thêm | Tooltip "Thêm giáo dân" | `gxAddEdit1_AddClick` |
| Sửa | Tooltip "Sửa giáo dân được chọn" | `gxAddEdit1_EditClick` |
| Xóa | Tooltip "Xóa giáo dân được chọn" | `gxAddEdit1_DeleteClick` |
| In (đổi nhãn) | Xuất Excel, không có tooltip cấu hình | `btnInDanhSach_Click` |
| Button1 | "In chứng nhận &bí tích" | `btnInChungNhanBiTich_Click` → `gxGiaoDanList1.XuatChungNhanBiTich()` |
| Button2 | "In giới thiệu &hôn phối" | `btnInGioiThieuHonPhoi_Click` → `gxGiaoDanList1.XuatGioiThieuHonPhoi()` |
| Tìm (FindButton) | ẩn hoàn toàn (`Enabled=false` dòng 64, code tìm trên lưới bị comment hết dòng 138-146) | không hoạt động |
| Tải lại (ReloadButton) | hiện, nhưng `Enabled=false` cho tới khi người dùng đổi combo giáo họ (`Combo_SelectedIndexChanged`, dòng 127-131) | tải lại dữ liệu, bỏ qua tìm kiếm đang áp dụng |
| Xem vị trí (MapButton, thêm động ở constructor) | "Xem vị trí" | `BtnMap_Click` → `gxGiaoDanList1.ViewMapGiaoDan()` — lấy `SelectedItems[0]`, nếu không có địa chỉ thì báo "Giáo dân này không có địa chỉ để xem bản đồ" (`GxGiaoDanList.cs:911`) |
| Chọn (SelectButton) | ẩn (`Visible=false` dòng 75) | — |

Nút "Đóng" (`gxCommand1`): chỉ hiện Cancel với nhãn "Đó&ng" (OK ẩn) — đóng form, không hỏi xác nhận.

**Menu chuột phải trên lưới** (đúng 12 mục, khởi tạo ở `GxGiaoDanList` constructor,
`GxGiaoDanList.cs:48-98`, theo đúng thứ tự add vào `contextMenu`):

1. Xem chi tiết → `EditRow()`
2. In lý lịch cá nhân → `XuatLyLichCaNhan()` — nếu chọn nhiều dòng (`SelectedItems.Count>1`) thì
   mở `frmPrint` hỏi kiểu in "một file riêng mỗi người" hay "gộp nhiều người một file" trước khi
   xuất (dòng 152-195)
3. In chứng nhận bí tích → hỏi chọn loại bí tích qua `frmGoiChungNhan`
4. In giới thiệu hôn phối — nếu `DaCoGiaDinh=true` thì cảnh báo:
   > "Giáo dân này đã từng lập gia đình.\r\nBạn có chắc muốn giới thiệu hôn phối cho giáo dân này
   > không?" (Yes/No, `GxGiaoDanList.cs:402-403`)
5. In chứng nhận rửa tội → `XuatChungNhanMotBiTich(LoaiBiTich.RuaToi)`
6. In chứng nhận xưng tội - rước lễ → `LoaiBiTich.RuocLe`
7. In chứng nhận thêm sức → `LoaiBiTich.ThemSuc`
8. Xem gia đình → `showGiaDinh(maGiaoDan)`: nếu giáo dân không thuộc gia đình nào thì báo "Giáo
   dân này không thuộc gia đình nào"; nếu thuộc đúng 1 gia đình thì mở thẳng `frmGiaDinh`; nếu
   thuộc nhiều hơn 1 thì mở `frmXemGiaDinhGiaoDan` cho người dùng chọn (`GxGiaoDanList.cs:298-336`)
9. In giấy giới thiệu chứng nhận rửa tội → `frmReport` với `TypeExport.GioiThieuRuaToi`
10. In giấy giới thiệu giáo lý hôn phối → `TypeExport.GioiThieuGiaoLyPhonPhoi`
11. In giấy giới thiệu chứng nhận thêm sức → `TypeExport.GioiThieuThemSuc`
12. Xem vị trí → `ViewMapGiaoDan()` (cùng hành động với nút MapButton)

Mục "In điều tra và rao hôn phối" (item7 → `XuatRaoHonPhoi()`) **được định nghĩa nhưng bị comment,
không đưa vào menu** (`GxGiaoDanList.cs:56,68-69,87`) — logic vẫn còn (cảnh báo "Giáo dân này đã
từng lập gia đình...", dòng 384-393) nhưng không ai gọi tới được từ đây.

Double-click dòng: mở `EditRow()` giống nút Sửa (`GXGiaoDanList_RowDoubleClick`,
`GxGiaoDanList.cs:542-548`), có thể tắt qua cờ `AllowShowForm`.

## 6. Lưới dữ liệu

**Đã kiểm chứng lại bằng cách đọc trực tiếp `GxGiaoDanList.FormatGrid()`
(`Source/GXControl/GxGiaoDanList.cs:596-810`): đúng 29 cột, đúng thứ tự sau. `NamSinh` không nằm
trong danh sách này (xem mục 3).**

| # | Cột CSDL (`GiaoDanConst`/`GiaDinhConst`) | Tiêu đề | Rộng | Kiểu / định dạng | Ghi chú |
|---|---|---|---|---|---|
| 1 | MaGiaoDan | Mã GD | 50 | Text, `FilterEditType.Combo` | |
| 2 | TenThanh | Tên thánh | 80 | Text, Combo | |
| 3 | HoTen | Họ tên | 150 | Text, Combo | |
| 4 | Phai | Phái | 50 | Text, DropDownList | |
| 5 | NgaySinh | Ngày sinh | 80 | Text, căn phải, Combo | |
| 6 | NgayRuaToi | Ngày rửa tội | 80 | Text, căn phải, Combo | |
| 7 | NgayRuocLe | Ngày XTRL | 80 | Text, căn phải, Combo | |
| 8 | NgayThemSuc | Ngày Th.Sức | 80 | Text, căn phải, Combo | |
| 9 | DaCoGiaDinh | Lập GĐ | 50 | CheckBox, filter CheckBox | |
| 10 | HoTenCha | Cha | 100 | Text, Combo | |
| 11 | HoTenMe | Mẹ | 100 | Text, Combo | |
| 12 | TanTong | Tân tòng | 50 | CheckBox | |
| 13 | ConHoc | Còn học | 50 | CheckBox | |
| 14 | NgheNghiep | Nghề nghiệp | 100 | Text, Combo | |
| 15 | GhiChu | Ghi chú | 200 | Text, Combo | |
| 16 | DienThoai | Điện thoại | 80 | Text, Combo | |
| 17 | DiaChi | Địa chỉ | 200 | Text, Combo | |
| 18 | TenGiaoHo (GiaDinhConst) | Giáo họ | 80 | Text, Combo | |
| 19 | DaChuyenXu | Đã chuyển đi | 50 | CheckBox, `TrueValue=-1/FalseValue=0` | cột "tạm" theo comment mã nguồn |
| 20 | TrinhDoVanHoa | Văn hóa | 100 | Text, Combo | |
| 21 | TrinhDoChuyenMon | Chuyên môn | 80 | Text, Combo | thêm 2018-07-17 |
| 22 | BietNgoaiNgu | Ngoại ngữ | 50 | Text, Combo | thêm 2018-07-17 |
| 23 | QuaDoi | Qua đời | 50 | CheckBox, `Visible=true` (khai rõ) | |
| 24 | NgayQuaDoi | Ngày qua đời | 80 | Text, căn phải, Combo | |
| 25 | NoiAnTang | Nơi an táng | 80 | Text | không set `FilterEditType` (khác các cột khác) |
| 26 | NoiSinh | Nơi sinh | 60 | Text, Combo | thêm qua `AddColumn()` |
| 27 | NoiRuaToi | Nơi rửa tội | 60 | Text, Combo | |
| 28 | NoiRuocLe | Nơi XTRL | 60 | Text, Combo | |
| 29 | NoiThemSuc | Nơi thêm sức | 60 | Text, Combo | |

Sau khi dựng cột, `SetGridColumnWidth()` (`GxGrid.cs:157-177`) đọc độ rộng đã lưu trước đó của
từng người dùng từ bảng nhớ cục bộ (`GridColumns`, ghi bởi `UpdateColumnWidthToMemory`) và ghi đè
lên độ rộng mặc định ở trên nếu có — **độ rộng thực tế nhìn thấy có thể khác bảng trên tùy máy**,
vì đây là tùy biến lưu trên máy trạm (file XML cục bộ, `GxGrid.XmlPath`), không đồng bộ giữa các
máy — cách này không áp dụng được cho web (không có khái niệm "máy trạm" ghi file cục bộ).

Không có tô màu/gạch ngang có điều kiện: sự kiện `FormattingRow` được đăng ký
(`gxGiaoDanList1.FormattingRow += ...`) nhưng **thân hàm xử lý để trống**
(`frmGiaoDanList.cs:353-356`). Đã tìm kiếm toàn bộ `Source/` (loại trừ thư mục build) và không có
nơi nào khác gán màu/kiểu chữ theo `QuaDoi`/`DaChuyenXu`/`DaCoGiaDinh` cho lưới này — **bản desktop
không gạch ngang dòng nào cả**, khác với suy đoán ban đầu.

Cho phép nhiều lựa chọn (`SelectionMode.MultipleSelection`), có `RecordNavigator`, filter tự động
theo cột (`FilterMode.Automatic`, `DynamicFiltering=true` — người dùng gõ vào ô lọc dưới tiêu đề
mỗi cột để lọc ngay trên lưới, độc lập với bộ lọc giáo họ ở trên).

## 7. Liên kết sang màn hình khác

- Thêm/Sửa/double-click → `frmGiaoDan` (Chi tiết giáo dân) — xem `giao-dan-chi-tiet.md`.
- "Xem gia đình" (nút MapButton **không** — đây là qua menu chuột phải) → `frmGiaDinh` (1 gia
  đình) hoặc `frmXemGiaDinhGiaoDan` (chọn khi nhiều gia đình) → `Chi tiết gia đình`.
  `GxGiaoDanList.showGiaDinh` cũng được gọi trực tiếp từ `frmGiaoDan.gxCommand1_Button1Click`
  (nút "&Xem gia đinh" trên chính form chi tiết giáo dân).
- In giới thiệu hôn phối → `frmReportGioiThieuHP`.
- In chứng nhận bí tích → `frmGoiChungNhan` rồi xuất qua `ExcelReport.ReportChungNhanBT`.
- Xem vị trí → `Memory.ViewMap(address)` (mở bản đồ ngoài, có thể là trình duyệt/Google Maps —
  chưa xác nhận chi tiết cài đặt của `Memory.ViewMap`).

## 8. Khác biệt cố ý ở bản web

- Xuất Excel qua ghi file tạm + mở bằng ứng dụng máy trạm không khả thi trên mô hình web tập
  trung; nếu cần xuất, bản web nên trả file để tải xuống trình duyệt thay vì mở app cục bộ. (Hiện
  bản web **chưa làm chức năng xuất này**, xem mục 10.)
- Độ rộng cột do người dùng tùy biến, lưu cục bộ theo máy trong bản desktop — bản web (AG Grid)
  có thể lưu theo tài khoản trên máy chủ nếu muốn, nhưng đây là quyết định thiết kế chưa được đưa
  ra, không phải điều đã "cố ý khác" — ghi vào mục 9.

## 9. Chỗ chưa chắc

- Nội dung SQL thật của `SqlConstants.DELETE_GIAODAN` (dùng khi xóa mềm) chưa được đọc trực tiếp
  trong phiên làm việc này — giả định là `UPDATE GiaoDan SET DaXoa=-1 WHERE MaGiaoDan=?` dựa theo
  tên hằng số và ngữ cảnh, nhưng chưa trích dẫn được dòng mã xác nhận.
- `Memory.ViewMap(string address)` mở bản đồ bằng cách nào (trình duyệt ngoài, ứng dụng bản đồ cài
  sẵn, hay gọi API) chưa được lần theo mã nguồn của lớp `Memory`.
- `GxConstants.DangTimKiemGiaDinhGiaoDan` được set bởi `frmTimGiaoDan.cs`/`frmTimGiaDinh.cs`
  (không thuộc phạm vi 2 màn hình được giao) — chưa đọc để biết chính xác khi nào cờ này bật/tắt
  và dữ liệu tìm kiếm được truyền qua bằng cách nào.
- Định dạng hiển thị chính xác của các cột ngày (`NgaySinh`, `NgayRuaToi`...) trên lưới — cột khai
  báo kiểu `ColumnType.Text` (không phải `Date`) nên định dạng chuỗi ngày phụ thuộc vào cách dữ
  liệu được `Memory.GetData` trả về (có thể đã format sẵn thành chuỗi "dd/MM/yyyy" ở tầng dữ liệu)
  — chưa xác minh tầng đó.

## 10. Đối chiếu bản web hiện tại

Đã đọc: `WebApp/src/web/src/screens/GiaoDanList.tsx`, `GiaoDanListPage.tsx`,
`WebApp/src/web/src/components/GxGiaoDanList.tsx`, `WebApp/src/web/src/cot/cotGiaoDan.ts`,
`WebApp/src/Qlgx.Api/Services/GiaoDanService.cs`, `Endpoints/GiaoDanEndpoints.cs`,
`Dtos/GiaoDanDtos.cs`.

| Hành vi bản desktop | Bản web đã có? | Ghi chú |
|---|---|---|
| 29 cột đúng thứ tự | **Một phần — thiếu 5 cột** | `cotGiaoDan.ts` chỉ định nghĩa **25 cột**. Thiếu hẳn 5 cột đánh dấu: `DaCoGiaDinh` (Lập GĐ), `TanTong` (Tân tòng), `ConHoc` (Còn học), `DaChuyenXu` (Chuyển xứ), `QuaDoi` (Qua đời). Web có thêm 1 cột `quanHe` không có ở desktop (dùng cho ngữ cảnh lưới thành viên gia đình). 29 − 5 + 1 = 25. **Đã đếm lại trực tiếp bằng `grep -c "field: '"` trên `cotGiaoDan.ts` và đếm `Columns.Add` + `AddColumn` trong `FormatGrid()`.** Năm cột thiếu đều là thông tin trạng thái quan trọng — thiếu chúng người dùng không phân biệt được ai đã qua đời, ai đã chuyển xứ ngay trên lưới, càng nghiêm trọng khi kết hợp với lỗi ở dòng dưới (web không lọc sẵn hai nhóm này) |
| Lọc theo giáo họ (kể cả giáo xóm con) | Một phần | Web lọc theo `tenGiaoHo` so khớp chuỗi ở client (`data/giaoHoTam.ts` — danh mục giáo họ **tạm, hard-code**, chưa có bảng `GiaoHo` thật kèm quan hệ cha/con); backend `LayDanhSach` hỗ trợ `giaoHoId` nhưng không lọc theo `MaGiaoHoCha` (giáo xóm con) như desktop |
| Mặc định ẩn giáo dân đã qua đời/chuyển xứ/đã xóa | **Thiếu** | `GiaoDanService.LayDanhSach` chỉ lọc `!DaXoa`; không loại `QuaDoi`/`DaChuyenXu` như `GxGiaoHo.LoadGridData` (`AND DaXoa=0 AND DaChuyenXu=0 AND QuaDoi=0`) — danh sách web sẽ **hiện cả người đã mất và đã chuyển xứ** lẫn với người đang sinh hoạt, không giống desktop |
| Checkbox "Chỉ xem giáo dân không được thống kê" | Có | `chiKhongThongKe` lọc theo `khongThongKe`, đúng tinh thần tách biệt khỏi "Ngoài xứ" mà bản desktop có ghi tooltip sai |
| Ghi nhớ giáo họ đã chọn giữa các lần mở (`Memory.CurrentGiaoHo`) | **Thiếu** | Web luôn mặc định "Tất cả" (`giaoHo = '-1'`) khi vào lại màn hình |
| Nút Thêm giáo dân | **Thiếu chức năng thật** | Có nút "Thêm giáo dân" mở `GiaoDanDetail` rỗng, nhưng **không có API tạo mới** (`GiaoDanEndpoints` chỉ có GET/GET/PUT) — nút Cập nhật bị `disabled` khi `moi=true`; thông báo "Chưa hỗ trợ tạo mới giáo dân qua web ở giai đoạn này" |
| Xóa giáo dân (mềm/vĩnh viễn) + chặn xóa khi đang trong gia đình | **Thiếu hoàn toàn** | Không có endpoint `DELETE`, không có nút xóa nào trong `GiaoDanList.tsx`/`GxGiaoDanList.tsx` |
| Sửa giáo dân | Có | Qua `GiaoDanDetailPage` (GET rồi PUT), có xử lý xung đột `RowVersion` (409) — desktop không có khái niệm này (single-user Access) nên đây là cải tiến hợp lý cho môi trường nhiều người dùng |
| Menu chuột phải 12 mục | Có đủ nhãn, đúng thứ tự | `menuGiaoDanMacDinh` liệt kê đủ 12 mục |
| — nhưng 10/12 mục có hành động thật | **Thiếu** | Chỉ "Xem chi tiết" và "Xem gia đình" có `chay` (handler); "In lý lịch cá nhân", "In chứng nhận bí tích", "In giới thiệu hôn phối", "In chứng nhận rửa tội", "In chứng nhận xưng tội - rước lễ", "In chứng nhận thêm sức", "In giấy giới thiệu..." (x3), "Xem vị trí" đều là **mục menu không làm gì** khi bấm |
| Xuất Excel danh sách | **Thiếu** | Không có nút/luồng xuất Excel ở `GiaoDanList.tsx` |
| Double-click mở chi tiết | Có | `onMo` truyền xuống `GxGrid`, gọi `moGiaoDan(d.id)` |
| Tô màu/gạch ngang theo điều kiện | **Bản web tự thêm, desktop không có** | `GxGiaoDanList.tsx` truyền `toDo={(d) => d.quaDoi || d.daChuyenDi || d.lapGd}` để gạch ngang đỏ — đây là tính năng **mới, không tồn tại ở bản desktop** (đã xác minh `FormattingRow` để trống, không có coloring nào khác trong `Source/`). Cần người dùng xác nhận có muốn giữ tính năng mới này không, vì nó thay đổi trực quan quen thuộc (mục "Có gia đình" gạch ngang có thể gây hiểu lầm là "không còn hoạt động" trong khi đây là trạng thái bình thường của đa số giáo dân trưởng thành) |
| Lọc động trên từng cột lưới (`DynamicFiltering`) | Chưa xác nhận | Cần kiểm tra `GxGrid.tsx` (ngoài phạm vi buổi này) xem AG Grid có bật lọc theo cột tương đương không |
| Độ rộng cột tùy biến theo người dùng, lưu cục bộ | Không áp dụng / chưa có tương đương | Hợp lý bỏ theo mô hình web, nhưng nếu muốn giữ trải nghiệm thì nên lưu theo tài khoản ở server (AG Grid State) — chưa thấy triển khai |

### Ưu tiên khắc phục (ảnh hưởng tới việc bỏ hẳn bản desktop)

- **Cao — chặn hoàn toàn việc bỏ bản desktop:**
  1. Không có chức năng **Thêm giáo dân** thật (không có API tạo mới) — giáo xứ không thể nhập
     giáo dân mới nếu chỉ dùng web.
  2. Không có chức năng **Xóa giáo dân** (mềm lẫn vĩnh viễn), kể cả điều kiện chặn xóa khi đang
     thuộc gia đình.
  3. Danh sách mặc định **không loại người đã qua đời/đã chuyển xứ** — sai lệch nghiệp vụ, gây khó
     dùng ngay khi số liệu giáo xứ đủ lớn (danh sách sẽ lẫn lộn người còn/mất).
- **Trung bình:**
  4. 10/12 mục in ấn (chứng nhận bí tích, giới thiệu hôn phối, giấy giới thiệu...) chưa hoạt
     động — đây là các thao tác giáo xứ dùng thường xuyên khi làm hồ sơ cho giáo dân.
  5. "Xem vị trí" (bản đồ) chưa hoạt động.
  6. Giáo họ đang là danh mục tạm hard-code phía client, chưa lọc được theo quan hệ giáo họ
     cha/giáo xóm con như desktop.
- **Thấp:**
  7. Không nhớ giáo họ đã lọc giữa các lần mở màn hình.
  8. Chưa có xuất Excel danh sách.
  9. Tính năng gạch ngang đỏ mới cần được xác nhận là chủ đích, không phải ngẫu nhiên đưa vào.
