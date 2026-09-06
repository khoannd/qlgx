# Màn hình: Danh sách gia đình (`frmGiaDinhList.cs`)

| | |
|---|---|
| Tệp nguồn | `Source/ChuongTrinh/frmGiaDinhList.cs` (386 dòng) + `Source/ChuongTrinh/frmGiaDinhList.Designer.cs` (240 dòng) |
| UserControl dùng lại | `GxGiaDinhList` (`Source/GXControl/GxGiaDinhList.cs`, 1097 dòng — lưới chính, chứa phần lớn logic thật của màn hình), `GxGiaoHo` (combo Giáo họ `cbGiaoHo`), `GxAddEdit` (thanh nút Thêm/Sửa/Xóa/In/Tải lại), `GxCommand`, `GxGroupBox`, `GxCheckBox` |
| Bảng dữ liệu đụng tới | `GiaDinh` (qua view Access `SELECT_GIADINH_LIST`), `ThanhVienGiaDinh`, `GiaoDan`, `HonPhoi`, `GiaoDanHonPhoi`, `GiaoHo`, `RaoHonPhoi`, `LinhMuc`, `GiaoXu` (khi in chứng nhận) |
| Trạng thái migrate | xong → `WebApp/src/web/src/screens/GiaDinhList.tsx` (thiếu nhiều thao tác, xem mục 10) |

Khác với màn hình Chi tiết gia đình (xem `gia-dinh-chi-tiet.md` mục 0), file `frmGiaDinhList.cs` **không** có bản trùng tên gây nhầm lẫn — chỉ có một file, nằm trong project thật `Source/ChuongTrinh/GiaoXu.csproj` (đã xác nhận có `<Compile Include="frmGiaDinhList.cs">`).

## 1. Mục đích

Liệt kê toàn bộ gia đình trong xứ (hoặc lọc theo giáo họ), cho phép thêm/sửa/xóa gia đình, in ấn hàng loạt (sổ gia đình, chứng nhận hôn phối, lý lịch cá nhân, giới thiệu chuyển xứ), và xem vị trí trên bản đồ. Mở từ menu chính của ứng dụng; cũng dùng lại được ở chế độ chọn giáo họ cố định (`Operation == EDIT`, ví dụ khi mở từ màn hình Giáo họ để xem danh sách gia đình thuộc giáo họ đó — `cbGiaoHo.Enabled = false` trong trường hợp này, dòng 158).

## 2. Bố cục và các trường

| Nhãn hiển thị | Control | Ghi chú |
|---|---|---|
| Giáo họ | `cbGiaoHo` (GxGiaoHo) | Có tùy chọn "Tất cả" (`HasShowAll = true`, đặt trong `Load`, dòng 153) |
| Chỉ xem gia đình không được thống kê | `chkGiaDinhAo` (GxCheckBox) | Tooltip: "Gia đình không được thống kê là gia đình ngoài xứ, không được tính trong các mục thống kê" (Designer dòng 167-168) |
| Tổng cộng: N gia đình | `lblTotal` (label) | Cập nhật động theo `RowCount` |
| Lưới danh sách | `gxGiaDinhList1` (GxGiaDinhList) | Xem mục 6 |

Không có form nhập liệu riêng ở màn hình này — Thêm/Sửa đều mở `frmGiaDinh` (xem spec `gia-dinh-chi-tiet.md`).

## 3. Hành vi khi tải

- `Load` (`frmGiaDinhList_Load`, dòng 150-177):
  1. Bật `cbGiaoHo.HasShowAll = true` và gọi `gxGiaDinhList1.FormatGrid()` (dựng cột).
  2. Nếu form được mở ở chế độ **EDIT với `id > -1`** (tức được truyền sẵn một giáo họ cụ thể từ nơi khác) → khóa combo Giáo họ (`Enabled = false`) và gán `cbGiaoHo.MaGiaoHo = id`.
  3. Ngược lại: nếu bộ nhớ toàn cục không có cờ `DangTimKiemGiaDinhGiaoDan` → dùng giáo họ hiện tại của người dùng (`Memory.CurrentGiaoHo`) làm bộ lọc mặc định, nếu > 0.
  4. Nếu có cờ `DangTimKiemGiaDinhGiaoDan` (đến từ một tìm kiếm/điều hướng trước đó — **chưa rõ nơi đặt cờ này, xem mục 9**) → gọi thẳng `gxGiaDinhList1.LoadData()` không lọc giáo họ theo field trên.
  5. Nếu `Operation != EDIT` → ẩn hẳn thanh `gxCommand1` (thanh OK/Cancel dưới cùng — dùng khi màn hình được mở như hộp thoại chọn, không phải màn hình chính).
  6. Xóa cờ `DangTimKiemGiaDinhGiaoDan` sau khi dùng xong.
- Khi đổi Giáo họ trên combo → tự động load lại lưới qua cơ chế `AutoLoadGrid` của `GxGiaoHo` (không thấy code gọi `LoadGiaDinhList()` trực tiếp trong sự kiện đổi combo ở file này — suy luận từ việc `GxGiaoHo` có thuộc tính `GridGiaDinh` được gán, xem mục 9 về giới hạn đọc mã).
- Tải xong → `lblTotal.Text` = `"{RowCount} gia đình"` (`gxGiaDinhList1_LoadDataFinished`, dòng 137, và `gxGiaDinhList1_RowCountChanged`, dòng 105).
- Câu lệnh tải mặc định (`LoadGiaDinhList`, dòng 179-188): `SELECT_GIADINH_LIST` + `WHERE (MaGiaoHo = {id})` nếu có lọc giáo họ — không có `ORDER BY` tường minh trong C#; thứ tự thật phụ thuộc định nghĩa view Access `SELECT_GIADINH_LIST` (xem mục 9 của spec Chi tiết gia đình — không đọc được từ mã nguồn).
- Nhấn combo Giáo họ (đổi lựa chọn) → bật `gxAddEdit1.ReloadButton.Enabled = true` (dòng 132) — nút "Tải lại" ban đầu bị khóa (`Enabled = false`, dòng 91) cho tới khi người dùng đổi bộ lọc.

## 4. Quy tắc nghiệp vụ và kiểm tra dữ liệu

Màn hình danh sách hầu như không tự kiểm tra dữ liệu (việc đó nằm ở `frmGiaDinh`); các quy tắc riêng của màn hình này:

- **Xóa gia đình** (`gxAddEdit1_DeleteClick`, dòng 261-314) — hỏi 3 lựa chọn:
  > "Bạn muốn xóa vĩnh viễn gia đình được chọn không?\r\nChọn [Yes] để xóa vĩnh viễn.\r\nChọn [No] để đưa vào hồ sơ lưu trữ.\r\nChọn [Cancel] hủy bỏ việc xóa." (dòng 265)
  - **Yes** → `DELETE FROM GiaDinh WHERE MaGiaDinh=?` và `DELETE FROM ThanhVienGiaDinh WHERE MaGiaDinh=?` — xóa **vĩnh viễn**, kể cả liên kết thành viên.
  - **No** → chạy `SqlConstants.DELETE_GIADINH_THEO_ID`, thực chất là `UPDATE GiaDinh SET DaXoa=-1, UpdateDate=Now()` — **xóa mềm** (đưa vào lưu trữ), giữ nguyên liên kết `ThanhVienGiaDinh`.
  - **Cancel** → không làm gì, thoát ngay.
  - Áp dụng cho **toàn bộ dòng đang chọn** (hỗ trợ chọn nhiều dòng, `SelectedItems`), lặp qua từng dòng theo cùng một lựa chọn Yes/No đã chọn 1 lần cho cả lô — không hỏi lại từng dòng.
- **In phiếu gia đình** khi bấm nút Button2 trên thanh công cụ mà **chưa chọn dòng nào** (`Row < 0`) → báo lỗi: **"Vui lòng chọn lại gia đình cho đúng"** (dòng 121, tiêu đề "Thông báo lỗi").
- **Xem vị trí trên bản đồ** (`BtnMap_Click` → `GxGiaDinhList.ViewMapFamily`, `GxGiaDinhList.cs:1083-1095`) — lấy địa chỉ của dòng **đầu tiên trong `SelectedItems`** (`SelectedItems[0]`, không kiểm tra `Count > 0` trước — **nếu không chọn dòng nào sẽ némIndexOutOfRange, xem mục 9**); nếu địa chỉ rỗng → **"Gia đình này không có địa chỉ để xem bản đồ"** (dòng 1093).
- **Xuất chứng nhận hôn phối** (`GxGiaDinhList.XuatChungNhanHonPhoi`, dòng 121-138): nếu chọn nhiều dòng, xuất lần lượt cho từng dòng (không gộp); mỗi lần gọi `ChungNhanHonPhoi(maGiaDinh)`:
  - Chưa có thông tin giáo xứ trong hệ thống → **"Không tìm thấy thông tin giáo xứ. Vui lòng nhập thông tin giáo xứ trước khi sử dụng chức năng này."** (dòng 468).
  - Không lấy được thông tin gia đình → **"Không lấy được thông tin gia đình.\r\nXuất chứng nhận thất bại."** (dòng 493).
  - Gia đình chưa đủ cả tên chồng lẫn tên vợ → **"Gia đình được chọn không có đầy đủ thông tin vợ chồng nên không thể xuất chứng nhận hôn phối.\r\nXin vui lòng xem lại."** (dòng 499).
  - Không lấy được thông tin vợ chồng (ít hơn 2 dòng) → **"Không lấy được thông tin vợ chồng.\r\nXuất chứng nhận thất bại."** (dòng 531/540).
  - Không tìm thấy bản ghi hôn phối tương ứng → gọi `Memory.ShowError("Không tìm thấy thông tin hôn phối!")` (dòng 714).
  - Trước khi xuất, mở hộp thoại `frmGoiChungNhan` để người dùng chọn người nhận + tick "Rao hôn phối" — nếu tick, chèn thêm đoạn văn bản ngày rao (3 lần rao) vào chứng nhận; nếu không tick, để trống; nếu tick nhưng không tìm thấy dữ liệu rao hôn phối, dùng câu mẫu có dấu chấm để điền tay: *"Với thời gian rao hôn phối:\rNgày rao lần 1: ..............., Ngày rao lần 2: ..............., Ngày rao lần 3: ..............."* (dòng 707).
- **In lý lịch cá nhân** (menu chuột phải, `item4_Click`, dòng 84-119): lấy toàn bộ thành viên (kể cả chồng/vợ) của gia đình đang chọn; không có thành viên nào → **"hông có thành viên nào trong gia đình"** (dòng 107 — **lỗi chính tả nguyên văn trong mã nguồn: thiếu chữ "K" ở đầu "Không"**, giữ nguyên để đối chiếu, đề xuất bản web sửa lỗi chính tả này).
- **In sổ/phiếu gia đình** (`InPhieuGiaDinh`, dòng 151-162): nếu chỉ chọn 1 dòng → xuất trực tiếp 1 phiếu; nếu chọn nhiều dòng → mở `frmPrint` để chọn kiểu in (in riêng từng gia đình một file, hoặc gộp chung một file Word nhiều trang).
  - Gia đình đã chuyển xứ (`DaChuyenXu = true`) → không cho in, báo **"Gia đình này đã chuyển xứ."** (`GxGiaDinhList.cs:875`).
  - Gia đình không có thành viên nào → **"Gia đình này không có thành viên nào."** (dòng 893, icon Stop, không phải Error).
  - Không lấy được thông tin gia đình/thành viên → **"Không tìm thấy thông tin gia đình.\r\nXuất sổ gia đình thất bại."** / **"Không lấy được thông tin các thành viên trong gia đình.\r\nXuất sổ gia đình thất bại."** (dòng 870, 887).
  - Định dạng xuất (Word hay Excel) tùy config `CF_MAU_SOGIADINH` (dòng 946-962).

## 5. Thao tác người dùng

Thanh công cụ `gxAddEdit1` (từ Designer + constructor `frmGiaDinhList()`):

| Nút | Tooltip / Nhãn | Hành động |
|---|---|---|
| Thêm | "Thêm gia đình" | `gxAddEdit1_AddClick` → mở `frmGiaDinh` (Thêm mới), gán sẵn `MaGiaoHo` theo giáo họ đang lọc; lưu OK thì chèn dòng mới vào lưới và focus tới dòng đó (`FindAll`) |
| Sửa | "Sửa gia đình được chọn" | `gxAddEdit1_EditClick` → gọi thẳng `gxGiaDinhList1.EditRow()` (không có code riêng ở `frmGiaDinhList`, xử lý nằm trong `GxGiaDinhList`) |
| Xóa | "Xóa gia đình được chọn" | Xem mục 4 |
| In (PrintButton) | "In danh sách gia đình trong lưới hiện tại" (tooltip gán ở `ToolTipSelect`, dòng 125 — **tên tooltip không khớp ý nghĩa, có vẻ nhầm property, xem mục 9**) | `btnInDanhSach_Click` → xuất toàn bộ lưới hiện tại ra file Excel tạm rồi mở bằng ứng dụng mặc định (`Process.Start`) |
| Tải lại (ReloadButton) | | Bật lên chỉ sau khi đổi bộ lọc giáo họ; tải lại theo `GxAddEdit` chuẩn (không có code riêng ở form này) |
| Button1: "In chứng nhận hôn phối" | | `gxAddEdit1_Button1Click` → `XuatChungNhanHonPhoi()` |
| Button2: "In phiếu gia đình" | | `gxAddEdit1_Button2Click` → kiểm tra có dòng chọn rồi in, xem mục 4 |
| MapButton: "Xem vị trí" | | `BtnMap_Click` → xem mục 4 |
| Nhấp đúp dòng trong lưới | | Xử lý ở tầng `GxGiaDinhList.GxGiaDinhList_RowDoubleClick` → gọi `EditRow()` — **mở form Sửa**, dù `frmGiaDinhList.gxGiaDinhList1_RowDoubleClick` ở tầng form (dòng 316-319) để trống/không làm gì thêm |
| Nhấp phải (menu chuột phải trên lưới) | | Xem mục 6 |
| Nút OK / Cancel (`gxCommand1`, chỉ hiện khi `Operation == EDIT`) | | Cả hai đều chỉ `this.Close()` — dùng khi màn hình mở như hộp thoại chọn |

## 6. Lưới dữ liệu

Cột (định nghĩa trong `GxGiaDinhList.FormatGrid`, `GxGiaDinhList.cs:247-432`), đúng thứ tự dựng:

| # | Cột | Nguồn | Rộng | Ghi chú |
|---|---|---|---|---|
| — | GACH | nội bộ | 80 | **Ẩn** (`Visible=false`) — cờ 0/1/2 dùng để tô đỏ+gạch ngang riêng từng ô Người nam/Người nữ |
| 1 | Mã gia đình | `GiaDinh.MaGiaDinh` (hoặc `MaGiaDinhRieng` nếu bật cấu hình `CF_TUNHAP_MAGIADINH`) | 70 | |
| 2 | Tên gia đình | `TenGiaDinh` | 100 | |
| 3 | Người nam | `TenChong` | 200 | Gạch đỏ khi `GACH = 0` hoặc `2` |
| 4 | Người nữ | `TenVo` | 200 | Gạch đỏ khi `GACH = 1` hoặc `2` |
| 5 | Số người | `SoLuong` | 80 | |
| 6 | Điện thoại | `DienThoai` (số điện thoại gia đình) | 80 | |
| 7 | Điện thoại Chồng | `DTChong` | 80 | |
| 8 | Điện thoại Vợ | `DTVo` | 80 | |
| 9 | Địa chỉ | `DiaChi` | 100 | |
| 10 | Giáo họ | `TenGiaoHo` | 80 | |
| 11 | Diện gia đình | `DienGiaDinh` | 100 | Lọc kiểu DropDownList |
| 12 | Ghi chú | `GhiChu` | 80 | |

Ba cột đã bị vô hiệu hóa (comment lại trong mã, không hiển thị): "Ngày HP" (`NgayHonPhoi`), "Tình trạng HP" (`CachThucHonPhoi`, từng có value-list "Hợp pháp/Hợp thức hóa/Chuẩn/Không theo phép đạo/Không xác định"), và "GĐ ảo" (`GiaDinhAo`, checkbox) — **các cột này đã bị gỡ khỏi bản build, không hiển thị cho người dùng, chỉ còn dấu vết trong code**.

Menu chuột phải trên lưới (`GxGiaDinhList` constructor, dòng 40-60), đúng thứ tự:
1. "Chứng nhận hôn phối"
2. ~~"Xem chi tiết"~~ — có tạo `MenuItem` nhưng **bị comment không add vào `ContextMenu`** (dòng 56) — không xuất hiện trong menu thật.
3. "In phiếu gia đình"
4. "In lý lịch cá nhân"
5. "In giới thiệu chuyển xứ"
6. "Xem vị trí"

## 7. Liên kết sang màn hình khác

- Mở `frmGiaDinh` (Thêm/Sửa) — xem `gia-dinh-chi-tiet.md`.
- Mở `frmGoiChungNhan` (chọn người nhận khi in chứng nhận hôn phối).
- Mở `frmPrint` (chọn kiểu in khi in nhiều gia đình cùng lúc).
- Mở `frmReport` với `EType = TypeExport.GioiThieuChuyenXu` (in giới thiệu chuyển xứ).
- Gọi `GxGiaoDanList.XuatLyLichCaNhan`, `ExcelReport.ReportSoGiaDinh`, `ExcelReport.ReportChungNhanHP` (các engine xuất tài liệu ngoài).

## 8. Khác biệt cố ý ở bản web

*(Để điền khi migrate — xem mục 10 cho hiện trạng thực tế.)*

## 9. Chỗ chưa chắc

- Nơi đặt cờ bộ nhớ toàn cục `GxConstants.DangTimKiemGiaDinhGiaoDan` (dùng để bỏ qua lọc giáo họ mặc định khi mở màn hình) — không tìm thấy trong 2 file được giao, chỉ thấy nơi đọc/xóa cờ.
- Cơ chế chính xác khiến đổi combo Giáo họ tự tải lại lưới — không thấy `Combo_SelectedIndexChanged` gọi `LoadGiaDinhList()` trực tiếp; suy đoán `GxGiaoHo.GridGiaDinh` tự động gọi lại khi đổi giá trị, nhưng chưa đọc mã `GxGiaoHo` để xác nhận.
- Thứ tự sắp xếp mặc định thật của lưới — phụ thuộc định nghĩa view Access `SELECT_GIADINH_LIST`, không có trong mã nguồn (file .accdb không đọc được ở đây).
- `ToolTipSelect = "In danh sách gia đình trong lưới hiện tại"` được gán cho nút mà theo tên thuộc tính là nút "Thêm" (Select) — nghi đây là gán nhầm property tooltip giữa các nút trong `GxAddEdit`, nhưng chưa xác nhận được (có thể `GxAddEdit` có cách ánh xạ nút khác với suy đoán từ tên).
- `BtnMap_Click` gọi `SelectedItems[0]` không kiểm tra `Count > 0` trước — khả năng ném lỗi nếu bấm "Xem vị trí" mà chưa chọn dòng nào; chưa xác nhận được có lớp bảo vệ nào khác ở tầng `GxAddEdit`/`MapButton` tự động vô hiệu hoá nút khi chưa chọn dòng hay không.
- Lỗi chính tả "hông có thành viên nào trong gia đình" (thiếu chữ K) — giữ nguyên trong bảng quy tắc để đối chiếu, chưa rõ có phải lỗi gõ vô tình hay có phiên bản khác đã sửa mà spec này chưa thấy.

## 10. Đối chiếu bản web hiện tại

So với `WebApp/src/web/src/screens/GiaDinhList.tsx`, `WebApp/src/web/src/components/GxGiaDinhList.tsx`, `WebApp/src/web/src/cot/cotGiaDinh.ts`, `WebApp/src/Qlgx.Api/Services/GiaDinhService.cs`, `WebApp/src/Qlgx.Api/Endpoints/GiaDinhEndpoints.cs`.

| Hành vi bản desktop | Bản web đã có? | Ghi chú |
|---|---|---|
| Liệt kê gia đình, đếm tổng số | **Có** | `GiaDinhList.tsx` hiện `count-pill` số dòng sau lọc; API `GET /api/gia-dinh` |
| Lọc theo Giáo họ (kể cả "Tất cả"/"Ngoài xứ") | **Có, nhưng lọc phía client** | `GiaDinhList.tsx` dùng `giaoHo` state lọc bằng JS trên toàn bộ `rows` đã tải, không gọi lại API theo `giaoHoId` thật — do "backend chưa có danh mục Giáo họ để truyền `giaoHoId` thật" (chú thích trong code). Khác desktop: desktop lọc bằng SQL `WHERE MaGiaoHo=?`. Với dữ liệu lớn, cách lọc client có thể chậm/tải thừa, nhưng endpoint đã có tham số `giaoHoId` sẵn sàng dùng khi có danh mục giáo họ thật |
| "Chỉ xem gia đình không được thống kê" | **Có** | Cả UI (`chiKhongThongKe` state, lọc client) lẫn API (`chiKhongThongKe` query param, đã cài trong `GiaDinhService.LayDanhSach`) — nhưng UI hiện tại lọc client thay vì gọi lại API với tham số đó |
| 12 cột đúng thứ tự desktop | **Có** | `cotGiaDinh.ts` khớp gần như 1:1 tên cột và thứ tự với `FormatGrid` (trừ cột GACH ẩn) |
| Gạch đỏ riêng ô Người nam/Người nữ theo qua đời/chuyển xứ | **Có** | `cellClass` trong `cotGiaDinh.ts` dùng đúng công thức `gach` (0/1/2) khớp với backend (`GiaDinhService.cs` dòng 56-58, công thức `2*voMat + chongMat - 1`) |
| Thêm gia đình mới | **Thiếu (dẫn tới ngõ cụt)** | Nút "Thêm gia đình" điều hướng sang `GiaDinhDetail` với `duLieu=undefined`, nhưng màn hình đó khóa cứng nút Lưu cho bản ghi mới — xem `gia-dinh-chi-tiet.md` mục 10 |
| Sửa gia đình (mở chi tiết) | **Có** | Nhấp đúp dòng / menu "In phiếu gia đình" hiện đang **dùng nhầm** để mở chi tiết (xem dưới) |
| Xóa gia đình (mềm/vĩnh viễn, chọn nhiều dòng) | **Thiếu hoàn toàn** | Không có nút Xóa nào trong `GiaDinhList.tsx`/`GxGiaDinhList.tsx`; không có endpoint DELETE |
| In danh sách ra Excel | **Thiếu** | Không có nút/hành động tương ứng |
| In chứng nhận hôn phối | **Thiếu** | Mục menu "In chứng nhận hôn phối" tồn tại trong `menuGiaDinhMacDinh` (`GxGiaDinhList.tsx:17`) nhưng **không có `chay` (handler)** — bấm vào không làm gì |
| In phiếu gia đình | **Sai/thiếu** | Mục menu "In phiếu gia đình" có `chay: moChiTiet` — tức bấm vào lại **mở màn hình chi tiết**, không in gì cả. Đây là hành vi hiển nhiên sai (nhãn ghi "in" nhưng chức năng là "mở chi tiết") — cần sửa nếu muốn giữ nhãn, hoặc đổi nhãn nếu cố ý dùng làm lối tắt mở chi tiết |
| In lý lịch cá nhân | **Thiếu** | Có mục menu nhưng không có `chay` |
| In giới thiệu chuyển xứ | **Thiếu** | Có mục menu nhưng không có `chay` |
| Xem vị trí trên bản đồ | **Thiếu** | Có mục menu "Xem vị trí" nhưng không có `chay` |
| Chọn nhiều dòng để in hàng loạt / xóa hàng loạt | **Thiếu** | Không thấy cơ chế chọn nhiều dòng nào trong `GxGiaDinhList.tsx`/`GxGrid` được dùng ở đây (ngoài phạm vi đọc sâu `GxGrid`) |
| Hộp thoại chọn kiểu in khi in nhiều gia đình (`frmPrint`) | **Thiếu** | Không có tương đương |
| Danh sách dùng như hộp thoại chọn (Operation=EDIT, có OK/Cancel) | **Không áp dụng / chưa rõ có cần** | Kiến trúc web dùng điều hướng thẻ (`useTabDocs`) khác hẳn mô hình dialog modal của desktop — đây nhiều khả năng là khác biệt kiến trúc cố ý, không phải thiếu sót, nhưng chưa thấy tài liệu xác nhận chính thức |

### Ưu tiên các thiếu sót (ảnh hưởng tới việc bỏ hẳn bản desktop)

**Cao:**
1. Không thể xóa gia đình qua web (mềm hoặc vĩnh viễn) — nghiệp vụ dọn dẹp dữ liệu định kỳ của giáo xứ không thực hiện được.
2. Menu "In phiếu gia đình" bấm vào lại mở màn hình chi tiết thay vì in — gây hiểu lầm/mất niềm tin vào phần mềm nếu không sửa trước khi bàn giao.
3. Không in được bất kỳ loại giấy tờ nào từ danh sách (chứng nhận hôn phối, phiếu gia đình, lý lịch cá nhân, giới thiệu chuyển xứ) — đây là nhu cầu vận hành hàng ngày của giáo xứ.

**Trung bình:**
4. Lọc theo Giáo họ đang thực hiện phía client (tải hết rồi lọc) thay vì gọi API lọc — chấp nhận được ở quy mô nhỏ nhưng sẽ chậm dần khi số gia đình tăng, và không tận dụng được tham số `chiKhongThongKe`/`giaoHoId` mà backend đã hỗ trợ sẵn.
5. Không có chức năng "Xem vị trí trên bản đồ" — tiện ích nhỏ nhưng đã có ở bản desktop.
6. Không có xuất Excel danh sách hiện tại.

**Thấp:**
7. Chưa rõ có cần giữ mô hình "màn hình danh sách dùng như hộp thoại chọn" (Operation=EDIT) hay kiến trúc thẻ mới đã thay thế hoàn toàn nhu cầu đó — cần xác nhận với người dùng.
