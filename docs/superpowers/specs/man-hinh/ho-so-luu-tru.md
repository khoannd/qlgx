# Màn hình: Hồ sơ lưu trữ giáo dân / gia đình (`frmGiaoDanLuuTruList.cs`, `frmGiaDinhLuuTruList.cs`)

| | |
|---|---|
| Tệp nguồn | `Source/ChuongTrinh/frmGiaoDanLuuTruList.cs` (263 dòng) + `.Designer.cs`; `Source/ChuongTrinh/frmGiaDinhLuuTruList.cs` (261 dòng) + `.Designer.cs` |
| UserControl dùng lại | `GxControl.GxGiaoDanList` / `GxControl.GxGiaDinhList` (CÙNG lớp mà màn hình danh sách đang hoạt động dùng — không có UserControl lưới riêng cho "lưu trữ"), `GxControl.GxGiaoHo` (combo Giáo họ, cờ `IsLuuTru=true` đổi hẳn câu WHERE khi tải), `GxControl.GxAddEdit`, `GxControl.GxCommand` |
| Bảng dữ liệu đụng tới | `GiaoDan`, `GiaDinh`, `GiaoHo` (JOIN lấy tên), gián tiếp `ThanhVienGiaDinh`, `BiTichChiTiet`, `ChuyenXu`, `GiaoDanHonPhoi`, `TanHien`, `RaoHonPhoi` (khi xóa vĩnh viễn) |
| Trạng thái migrate | xong (web: `GiaoDanLuuTruList.tsx` + `GiaDinhLuuTruList.tsx`, tái dùng `GxGiaoDanList`/`GxGiaDinhList`) |

## 0. "Hồ sơ lưu trữ" nghĩa là gì — bằng chứng từ mã

Hai màn hình này **không phải** một khái niệm nghiệp vụ riêng có bảng CSDL riêng — chúng là
**cùng một lưới** (`GxGiaoDanList`/`GxGiaDinhList`) mà bản chính (`frmGiaoDanList`/
`frmGiaDinhList`) dùng, chỉ khác ở **điều kiện WHERE khi tải dữ liệu**, chuyển đổi bằng một cờ
duy nhất trên combo Giáo họ:

```csharp
// frmGiaoDanLuuTruList.cs:47-48 (constructor)
cbGiaoHo.GridGiaoDan = gxGiaoDanList1;
cbGiaoHo.IsLuuTru = true;
```

```csharp
// frmGiaDinhLuuTruList.cs:43-44 (constructor)
cbGiaoHo.GridGiaDinh = gxGiaDinhList1;
cbGiaoHo.IsLuuTru = true;
```

Cờ `IsLuuTru` được đọc trong `GxGiaoHo.LoadGridData()` (`Source/GXControl/GxGiaoHo.cs:261-315`,
hàm THẬT chạy khi đổi giáo họ hoặc khi form gán `MaGiaoHo` lúc mở — xem mục 3 dưới đây để biết
vì sao đây là hàm thật, không phải hàm `LoadGiaoDanList()`/`LoadGiaDinhList()` khai trong chính
hai file form, vốn là **code chết** giống hệt màn hình danh sách chính):

```csharp
// GxGiaoHo.cs:266-268 (nhánh GIÁO DÂN)
string where = "";
if(!isHasLuuTru)
    where = isLuuTru ? " AND (DaXoa=-1 OR DaChuyenXu=-1 OR QuaDoi=-1) " : " AND DaXoa=0 AND DaChuyenXu=0 AND QuaDoi=0 ";
```

```csharp
// GxGiaoHo.cs:292-293 (nhánh GIA ĐÌNH)
string where = "";
if (!isHasLuuTru)
    where = isLuuTru ? " AND (DaXoa=-1 OR DaChuyenXu=-1 ) " : " AND DaXoa=0 AND DaChuyenXu=0";
```

Vậy **"hồ sơ lưu trữ"** = phần bù chính xác của "danh sách đang hoạt động": một giáo dân/gia
đình xuất hiện ở đây nếu **thoả ÍT NHẤT MỘT** trong các điều kiện (OR, không phải AND như danh
sách chính):

- **Giáo dân**: đã xóa mềm (`DaXoa=-1`), HOẶC đã chuyển xứ (`DaChuyenXu=-1`, cột suy từ JOIN
  bảng `ChuyenXu` cho CHÍNH giáo dân đó — khác `DaChuyenXu` của một GIA ĐÌNH), HOẶC đã qua đời
  (`QuaDoi=-1`).
- **Gia đình**: đã xóa mềm (`DaXoa=-1`) HOẶC đã chuyển xứ (`DaChuyenXu=-1`, cột trực tiếp trên
  bảng `GiaDinh`). **Không có điều kiện qua đời** — bảng `GiaDinh` không có cột này.

Đây đúng như dự đoán ban đầu của nhiệm vụ: hồ sơ lưu trữ giữ lại các bản ghi "không còn sinh
hoạt" (đã mất/chuyển đi/xóa) nhưng **không xóa vĩnh viễn** khỏi CSDL, để còn tra cứu/cấp giấy
chứng nhận sau này — và số liệu thật xác nhận đúng giả thuyết "**2050 − 2039 = 11 người đã đi
đâu**" (xem mục 3).

Giáo họ lọc **chính xác** (`MaGiaoHo={0} OR MaGiaoHoCha={0}`, gồm cả giáo xóm con) — giống hệt
danh sách chính, không có gì khác biệt riêng cho màn hình lưu trữ ở khoản này.

## 1. Mục đích

Xem lại và quản lý các hồ sơ **không còn xuất hiện** ở danh sách giáo dân/gia đình đang hoạt
động (đã xóa mềm, đã qua đời, đã chuyển xứ) — vẫn cần tra cứu, sửa thông tin, hoặc cuối cùng
**xóa vĩnh viễn** khỏi chương trình khi chắc chắn không còn cần nữa. Mở từ menu chính
(`frmMain.cs`, chưa migrate ở đợt này), nhóm menu "Hồ sơ lưu trữ".

## 2. Bố cục và các trường

Giống hệt bố cục màn hình danh sách chính tương ứng (xem `giao-dan-danh-sach.md` mục 2,
`gia-dinh-danh-sach.md` mục 2) — chỉ có bộ lọc phía trên (combo Giáo họ, checkbox "chỉ xem
không được thống kê", nhãn tổng số) và lưới bên dưới, không có trường nhập liệu riêng nào ở
chính màn hình danh sách.

## 3. Hành vi khi tải

- `frmGiaoDanLuuTruList_Load`/`frmGiaDinhLuuTruList_Load` (`frmGiaoDanLuuTruList.cs:100-107`,
  `frmGiaDinhLuuTruList.cs:102-118`): gọi `FormatGrid()` (29 cột cho giáo dân, 12 cột cho gia
  đình — đúng bảng cột của màn hình danh sách chính, xem `giao-dan-danh-sach.md`/
  `gia-dinh-danh-sach.md` mục 6), bật `cbGiaoHo.HasShowAll=true` và `AutoLoadGrid=true`, rồi
  nếu `Memory.CurrentGiaoHo > 0` thì gán `cbGiaoHo.MaGiaoHo = Memory.CurrentGiaoHo` — **kích
  hoạt tải dữ liệu thật qua `GxGiaoHo.LoadGridData()`**, đúng cơ chế của màn hình danh sách
  chính (xem `giao-dan-danh-sach.md` mục 3).
- **Hàm `LoadGiaoDanList()`/`LoadGiaDinhList()` khai trong chính hai file form
  (`frmGiaoDanLuuTruList.cs:109-118`, `frmGiaDinhLuuTruList.cs:120-128`) là CODE CHẾT** — không
  hề được gọi ở đâu (đã grep toàn bộ file), giống hệt phát hiện đã ghi ở
  `giao-dan-danh-sach.md` mục 3 cho màn hình danh sách chính. Đáng chú ý: WHERE trong hàm chết
  của `frmGiaoDanLuuTruList` (`" AND (DaXoa=-1 OR DaChuyenXu=-1) "`, dòng 111) **thiếu điều kiện
  `QuaDoi`** so với WHERE thật dùng trong `GxGiaoHo.LoadGridData` — nếu hàm chết này từng được
  gọi (ví dụ ở một phiên bản cũ hơn) thì hành vi đã khác hẳn. Không ảnh hưởng gì tới hành vi
  thật hiện tại vì hàm không bao giờ chạy.
- `cbGiaoHo.IsAo` khởi tạo `TriState.False` trong constructor (dòng 56/51) — mặc định lọc
  `GiaoDanAo=0`/`GiaDinhAo=0` (chỉ hiện bản ghi ĐƯỢC thống kê) cho tới khi người dùng tick
  checkbox `chkGiaoDanAo`.
- Không có `.Focus()` nào cho lưới hay ô lọc khi mở form.

### Số liệu thật (giáo xứ Vô Nhiễm, CSDL `qlgx_thu`)

Đếm trực tiếp bằng `psql` (2026-09-08):

```
select count(*) from giao_dan;                       -- 2050
select count(*) from giao_dan where da_xoa;           -- 0
select count(*) from giao_dan where qua_doi;          -- 11
select count(*) from chuyen_xu;                       -- 0 (bảng rỗng ở giáo xứ này)
select count(*) from giao_dan where da_xoa or qua_doi;-- 11

select count(*) from gia_dinh;                        -- 40
select count(*) from gia_dinh where da_xoa;           -- 0
select count(*) from gia_dinh where da_chuyen_xu;     -- 0
select count(*) from gia_dinh where da_xoa or da_chuyen_xu; -- 0
```

Xác nhận đúng giả thuyết nêu trong nhiệm vụ: **2050 − 11 = 2039**, đúng khớp con số màn hình
"Danh sách giáo dân" vẫn hiển thị. Cả 11 người "biến mất" khỏi danh sách chính đều là do
`qua_doi=true`, không ai do `da_xoa`/chuyển xứ (bảng `chuyen_xu` rỗng ở giáo xứ này — đúng ghi
chú sẵn có trên entity `ChuyenXu`: "Rỗng ở giáo xứ khảo sát (Vô Nhiễm)").

**Vậy màn hình "Hồ sơ lưu trữ giáo dân" hiện đúng 11 dòng; "Hồ sơ lưu trữ gia đình" hiện đúng 0
dòng** (giáo xứ này chưa có gia đình nào bị xóa mềm hoặc chuyển xứ) ở dữ liệu hiện có. Đã gọi
lại `GET /api/giao-dan/luu-tru` và `GET /api/gia-dinh/luu-tru` qua API thật (không chỉ psql) và
nhận đúng 11/0 — khớp tuyệt đối.

## 4. Quy tắc nghiệp vụ và kiểm tra dữ liệu

**Không có nút "Thêm"** ở cả hai màn hình lưu trữ — `gxAddEdit1.AddButton.Visible = false`
(`frmGiaoDanLuuTruList.cs:43`, `frmGiaDinhLuuTruList.cs:25`). Hợp lý: không có lý do tạo mới
trực tiếp một hồ sơ "đã lưu trữ".

**Sửa**: nút Sửa/double-click mở **CÙNG** `frmGiaoDan`/`frmGiaDinh` mà màn hình danh sách chính
dùng — không có form chi tiết riêng nào cho hồ sơ lưu trữ, không có điều kiện chặn sửa nào theo
`DaXoa`/`QuaDoi`/`DaChuyenXu`.

**Xóa — điểm khác biệt LỚN NHẤT so với danh sách chính**: nút đổi nhãn thành "&Xóa bỏ"
(`gxAddEdit1.DeleteButton.Text = "&Xóa bỏ"`, cả hai file dòng 21) và hộp thoại xác nhận
**CHỈ CÓ 2 LỰA CHỌN (YesNo)**, không phải 3 lựa chọn (YesNoCancel) như danh sách chính — vì bản
ghi **đã ở trong hồ sơ lưu trữ rồi**, không còn lựa chọn "đưa vào lưu trữ" (xóa mềm) nào nữa,
chỉ còn "xóa vĩnh viễn" hoặc "hủy":

- **Giáo dân** (`gxAddEdit1_DeleteClick`, `frmGiaoDanLuuTruList.cs:157-202`), thông báo nguyên
  văn:
  > "Nếu bạn chọn xóa giáo dân khỏi hồ sơ lưu trữ, giáo dân này sẽ vĩnh viễn bị xóa khỏi chương
  > trình\r\nBạn có chắc muốn xóa giáo dân này?" (tiêu đề "Xác nhận xóa", `MessageBoxButtons.YesNo`)

  Trước khi xóa, gọi **CÙNG** `checkGiaoDanTrongGiaDinh(maGiaoDan, tenGiaoDan)`
  (`GxGiaoDanList.cs:915-945`) như danh sách chính — chặn nếu giáo dân đang thuộc gia đình nào,
  thông báo liệt kê từng gia đình y hệt (xem `giao-dan-danh-sach.md` mục 4). Nếu không vướng,
  xóa tuần tự (KHÔNG transaction) qua **7 bảng, NHIỀU HƠN** danh sách chính (vốn chỉ xóa
  `GiaoDan`+`ThanhVienGiaDinh`+`BiTichChiTiet`+`ChiTietLopGiaoLy`):
  `BiTichChiTiet` → `ThanhVienGiaDinh` → `ChuyenXu` → `GiaoDanHonPhoi` → `TanHien` →
  `RaoHonPhoi` (SQL thô, theo cả `MaGiaoDan1` lẫn `MaGiaoDan2`) → `GiaoDan`
  (`frmGiaoDanLuuTruList.cs:172-196`). **Đây là một lệch pha thật giữa hai màn hình desktop** —
  xóa vĩnh viễn từ màn hình lưu trữ dọn nhiều bảng hơn xóa vĩnh viễn từ danh sách chính (thiếu
  `ChiTietLopGiaoLy` ở đây, nhưng có thêm 4 bảng kia mà danh sách chính không đụng tới). Ghi vào
  `can-review-sau.md` — bản web tái dùng lại **một** endpoint xóa duy nhất
  (`GiaoDanService.Xoa(vinhVien:true)`, dọn `GiaoDan`+`BiTichChiTiet`+`ChiTietLopGiaoLy` trong
  transaction) cho cả hai màn hình thay vì viết thêm một luồng xóa thứ hai chỉ khác nhau ở tập
  bảng — xem mục 8.

- **Gia đình** (`gxAddEdit1_DeleteClick`, `frmGiaDinhLuuTruList.cs:170-192`), thông báo nguyên
  văn:
  > "Nếu bạn chọn xóa gia đình khỏi hồ sơ lưu trữ, gia đình này sẽ vĩnh viễn bị xóa khỏi chương
  > trình\r\nBạn có chắc muốn xóa gia đình này?" (tiêu đề "Xác nhận xóa", `MessageBoxButtons.YesNo`)

  Không có điều kiện chặn nào (giống danh sách chính). Xóa `ThanhVienGiaDinh` rồi `GiaDinh` —
  **ĐÚNG Y HỆT** tập bảng mà danh sách chính xóa vĩnh viễn (`GiaDinhService.Xoa(vinhVien:true)`)
  — không có lệch pha nào ở phía gia đình.

**KHÔNG có chức năng "khôi phục"** (đưa một hồ sơ lưu trữ trở lại danh sách đang hoạt động) ở
cả hai màn hình — đã đọc toàn bộ `frmGiaoDanLuuTruList.cs`/`frmGiaDinhLuuTruList.cs` và
`GxGiaoDanList.cs`/`GxGiaDinhList.cs`, không có nút/menu/hàm nào tên `KhoiPhuc`/"khôi phục" gắn
với hai màn hình này. Cách DUY NHẤT một giáo dân đã xóa mềm quay lại "chưa xóa" trong toàn bộ mã
nguồn là **gián tiếp, từ một màn hình khác hẳn**: `frmGiaDinh.cs:1112-1120` — khi thêm một giáo
dân đã `DaXoa=true` vào lưới thành viên của MỘT GIA ĐÌNH, hộp thoại hỏi:
> "Giáo dân [...] đã bị xóa.\r\nNếu thêm giáo dân này vào gia đình thì sẽ khôi phục giáo dân này
> thành chưa xóa.\r\nChọn [Yes] để tiếp tục thêm giáo dân này vào thành viên gia đình.\r\nChọn
> [No] để hủy."

Đây không phải một nút "khôi phục" của màn hình lưu trữ — là tác dụng phụ của một thao tác khác
hoàn toàn (thêm thành viên gia đình), và **KHÔNG áp dụng được cho `QuaDoi`/`DaChuyenXu`, cũng
không áp dụng cho gia đình** (không tìm thấy cơ chế tương tự nào cho `frmGiaDinh` tự nó). Bản
web đã có cơ chế tương đương này ở `GiaDinhService.ThemThanhVien` (xem `gia-dinh-chi-tiet.md`),
ngoài phạm vi hai màn hình được giao ở đây.

**Kết luận: bản web KHÔNG cần dựng nút "Khôi phục" nào cho hai màn hình này** — làm vậy sẽ là
một tính năng MỚI không có ở bản gốc (vi phạm nguyên tắc "migrate y hệt"). Nút Xóa của hai màn
hình lưu trữ chỉ có một hành động — xóa vĩnh viễn.

## 5. Thao tác người dùng

Thanh `gxAddEdit1`:

| Nút | Giáo dân | Gia đình |
|---|---|---|
| Thêm | ẨN | ẨN |
| Sửa | mở `frmGiaoDan` (EditRow) | mở `frmGiaDinh` (EditRow) |
| Xóa bỏ | xem mục 4 (permanent, YesNo) | xem mục 4 (permanent, YesNo) |
| In (PrintButton→Excel) | `btnInDanhSach_Click` — xuất `.xls` tạm, `Process.Start` | như giáo dân |
| Button1 | "In chứng nhận &bí tích" → `XuatChungNhanBiTich()` | "In &chứng nhận hôn phối" → `XuatChungNhanHonPhoi()` |
| Button2 | "In giới thiệu &hôn phối" → `XuatGioiThieuHonPhoi()` | "In &sổ gia đình" → `XuatSoGiaDinh()` |
| Tìm (FindButton) | `Enabled=false` (ẩn tác dụng) | hiện, không có handler gán (`FindButton_Click` bị comment) |
| Tải lại (ReloadButton) | hiện, khóa tới khi đổi combo | như giáo dân |

Menu chuột phải trên lưới: **ĐÚNG Y HỆT** 12 mục (giáo dân)/6 mục (gia đình, gồm cả "Xem chi
tiết" bị comment không add vào menu) của màn hình danh sách chính — vì đây là **CÙNG MỘT
UserControl** `GxGiaoDanList`/`GxGiaDinhList`, không định nghĩa lại constructor. Xem
`giao-dan-danh-sach.md` mục 5, `gia-dinh-danh-sach.md` mục 6 để biết đầy đủ.

Double-click dòng: mở form Sửa — `gxGiaoDanList1_RowDoubleClick` gọi thẳng `EditRow()`
(`frmGiaoDanLuuTruList.cs:204-207`); phía gia đình, `gxGiaDinhList1_RowDoubleClick` ở tầng form
để trống (dòng 194-197) nhưng double-click vẫn mở Sửa qua xử lý nội bộ của
`GxGiaDinhList.GxGiaDinhList_RowDoubleClick` (đúng ghi chú đã có ở `gia-dinh-danh-sach.md` mục 5).

Nút "Đóng" (`gxCommand1`): chỉ hiện Cancel (giáo dân, nhãn "Đó&ng") hoặc ẩn hẳn khi
`Operation != EDIT` (gia đình) — đóng form không hỏi xác nhận.

## 6. Lưới dữ liệu

**ĐÚNG Y HỆT** bảng 29 cột (giáo dân)/12 cột (gia đình) của màn hình danh sách chính tương ứng
— cùng `FormatGrid()`. Xem `giao-dan-danh-sach.md` mục 6, `gia-dinh-danh-sach.md` mục 6.

## 7. Liên kết sang màn hình khác

Giống hệt màn hình danh sách chính (xem mục 7 của hai spec đó) — `frmGiaoDan`/`frmGiaDinh`,
`frmGoiChungNhan`, `frmReportGioiThieuHP`, v.v. Không có liên kết nào riêng cho "lưu trữ".

## 8. Khác biệt cố ý ở bản web

- **Xóa vĩnh viễn giáo dân dùng lại nguyên endpoint `DELETE /api/giao-dan/{id}?vinhVien=true`
  đã có cho danh sách chính** (dọn `GiaoDan`+`BiTichChiTiet`+`ChiTietLopGiaoLy` trong một
  transaction), **KHÔNG** tái hiện đúng 7-bảng mà `frmGiaoDanLuuTruList.cs` xóa (xem mục 4).
  Lý do có chủ đích: (1) CSDL Postgres của bản web dùng khóa ngoại thật — nếu để rác ở
  `ChuyenXu`/`GiaoDanHonPhoi`/`TanHien`/`RaoHonPhoi` sau khi xóa `GiaoDan`, các ràng buộc FK sẽ
  chặn (desktop dùng Access không ràng buộc FK nên "xóa thiếu" không lộ ra ngay); dựng đúng một
  luồng xóa dọn đủ các bảng liên quan một lần rồi dùng chung cho cả hai màn hình an toàn hơn hai
  luồng xóa lệch nhau; (2) viết thêm một service xóa thứ hai chỉ khác ở tập bảng là nhân đôi một
  logic đã có, dễ lệch dần theo thời gian. Vì bảng `ChuyenXu`/`GiaoDanHonPhoi`/`TanHien`/
  `RaoHonPhoi` ở giáo xứ khảo sát hiện đều rỗng hoặc không liên quan tới 11 giáo dân lưu trữ,
  quyết định này chưa quan sát được khác biệt thật trên dữ liệu hiện có — **ghi vào
  `can-review-sau.md` để người dùng xác nhận**: nếu một giáo xứ có dữ liệu `ChuyenXu`/
  `GiaoDanHonPhoi`/`TanHien`/`RaoHonPhoi` thật gắn với một giáo dân trong hồ sơ lưu trữ, xóa
  vĩnh viễn qua bản web sẽ để lại các dòng đó (không xóa theo), khác desktop.
- **Không dựng nút "Khôi phục"** — đúng vì bản gốc không có (xem mục 4). Nếu người dùng thực sự
  cần khôi phục một hồ sơ đã xóa mềm về active mà không đi vòng qua "thêm vào một gia đình"
  (cách duy nhất desktop hỗ trợ, và chỉ áp dụng cho `DaXoa`, không áp dụng `QuaDoi`), đây là một
  TÍNH NĂNG MỚI cần người dùng yêu cầu rõ ràng — không tự ý thêm.
- Xuất Excel: tái dùng hạ tầng `XuatExcelService`/ClosedXML sẵn có (thay vì ghi file `.xls` tạm
  + `Process.Start` như desktop, không khả thi trên mô hình máy chủ tập trung) — hai endpoint
  mới `GET /api/giao-dan/luu-tru/xuat-excel` và `GET /api/gia-dinh/luu-tru/xuat-excel`, cùng bộ
  cột với xuất Excel của danh sách chính.
- "In chứng nhận bí tích" (giáo dân) và "In chứng nhận hôn phối" (gia đình) trên thanh công cụ
  của hai màn hình lưu trữ gọi lại đúng API in ấn đã có (`InAnService`, xem `in-an.md`) — không
  phải tính năng mới, chỉ nối nút. "In giới thiệu hôn phối" (giáo dân) và "In sổ gia đình" (gia
  đình) CHƯA có hạ tầng in tương ứng ở bản web (giống tình trạng của danh sách chính, xem
  `giao-dan-danh-sach.md`/`gia-dinh-danh-sach.md` mục 10) — hiện thông báo "chưa hỗ trợ", đúng
  quy ước `chuaHoTro` dùng xuyên suốt dự án.
- Bộ lọc Giáo họ/"chỉ xem không được thống kê" thực hiện ở CLIENT (không gọi lại API mỗi lần đổi
  bộ lọc) — cùng cách tiếp cận đã chọn cho danh sách chính, hợp lý ở quy mô hiện tại (11/0 dòng).

## 9. Chỗ chưa chắc

- `ChuyenXu` (cột "đã chuyển xứ" của MỘT GIÁO DÂN, dùng trong điều kiện lưu trữ giáo dân) suy từ
  bản ghi CHUYENXU MỚI NHẤT có `LoaiChuyen=ChuyenDi`, cùng công thức
  `GiaDinhService.ThemThanhVien` đã dùng cho `daChuyenDi` — nhưng bản Access gốc tính "DaChuyenXu"
  qua một JOIN trong `SELECT_GIAODAN_LIST_CO_GIAOHO` (`Source/DBAccess/SqlConstants.cs:134-144`)
  chưa đọc trực tiếp trong phiên này để xác nhận công thức JOIN đó có đúng "LoaiChuyen=ChuyenDi
  mới nhất" hay là "có BẤT KỲ dòng ChuyenXu nào" (kể cả ChuyenDen). Vì bảng `ChuyenXu` rỗng ở dữ
  liệu khảo sát, không kiểm chứng được bằng số liệu thật ở giáo xứ Vô Nhiễm — cần xác nhận lại
  nếu triển khai cho một giáo xứ có dữ liệu chuyển xứ.
- Nội dung SQL chính xác của việc "xóa `RaoHonPhoi` theo `MaGiaoDan1`/`MaGiaoDan2`" trong
  `frmGiaoDanLuuTruList.cs:186-189` đã đọc trực tiếp và trích dẫn đúng, nhưng chưa xác nhận bảng
  `RaoHonPhoi` phía web (`Qlgx.Domain.Entities`) có đúng hai cột tương ứng
  `GiaoDan1Id`/`GiaoDan2Id` hay tên khác — không quan trọng cho quyết định ở mục 8 (đã chọn
  không tái hiện xóa bảng này) nhưng cần biết nếu sau này người dùng yêu cầu tái hiện đủ 7 bảng.
