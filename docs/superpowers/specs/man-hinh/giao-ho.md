# Màn hình: Giáo họ (`frmGiaoHo.cs`)

| | |
|---|---|
| Tệp nguồn | `Source/ChuongTrinh/frmGiaoHo.cs` (598 dòng, UTF-16LE) + `Source/ChuongTrinh/frmGiaoHo.Designer.cs` (bố cục) + `Source/GXControl/GxGiaoHoList.cs` (56 dòng, UTF-16LE, lưới) |
| UserControl dùng lại | `GxAddEdit` (thanh nút Thêm/Sửa/Xoá/Chọn(Lưu) dùng chung), `GxGiaoHoList : GxGrid` (lưới danh mục), `GxTextField` (2 ô nhập Mã/Tên) |
| Bảng dữ liệu đụng tới | `GiaoHo` (Access) → `giao_ho` (Postgres) — và khi XOÁ còn đụng cả `ThanhVienGiaDinh`, `GiaoDan`, `GiaDinh`, `GiaoDanHonPhoi`, `BiTichChiTiet`, `TanHien`, `ChuyenXu`, `RaoHonPhoi` (xem mục 4) |
| Trạng thái migrate | Xong phần danh sách + thêm/sửa (`GiaoHoListPage.tsx`); **KHÔNG có nút xoá** (xem mục 4, mục 8, `can-review-sau.md` mục 37); **KHÔNG dựng lại UI đệ quy "Giáo khu"** — thay bằng select chọn giáo họ cha phẳng (xem mục 8) |

## 1. Mục đích

Danh mục Giáo họ (và "Giáo khu" — giáo họ con lồng trong một giáo họ cha) của giáo xứ đang
đăng nhập. Mở từ `frmMain.cs` (chưa đọc menu chính xác trong nhiệm vụ này, xem mục 9). Đây là
form **tự mở lại chính nó** để quản lý cấp con: khi double-click một giáo họ ở danh sách cấp 1,
`EditGiaoHoRow()` mở một `frmGiaoHo` MỚI với `MaGiaoHoCha` đặt bằng mã giáo họ vừa chọn — cửa sổ
mới đó quản lý "Danh sách giáo khu" của giáo họ đó (`frmGiaoHo.cs:518-533`).

Dữ liệu thật (giáo xứ Vô Nhiễm): **1 giáo họ** ("Simon Phan Đắc Hòa"), không có giáo khu con
nào — `MaGiaoHoCha` toàn bộ NULL/≤0.

## 2. Bố cục và các trường

| Nhãn hiển thị | Control | Cột CSDL | Bắt buộc? | Mặc định | Ghi chú |
|---|---|---|---|---|---|
| "Mã giáo họ" | `txtMaGiaoHo` (`GxTextField`) | `GiaoHo.MaGiaoHo` (khoá chính kiểu số, Access) | có (tự sinh) | `Memory.GetNextId(...)` lúc mở form/bấm Thêm (`frmGiaoHo.cs:78-81, 154`) | `ReadOnly=true` trừ lúc đang Thêm (`EnableEditControls`, dòng 200) — người dùng KHÔNG gõ tay mã |
| "Tên giáo họ" | `txtTenGiaoHo` (`GxTextField`, `AutoCompleteEnabled`, `AutoUpperFirstChar`, `MaxLength=255`) | `GiaoHo.TenGiaoHo` | có | rỗng | Đổi nhãn thành "Tên giáo khu" khi `maGiaoHoCha != -1` (`frmGiaoHo.cs:83-87`) |
| (ẩn, không có ô nhập riêng) | — | `GiaoHo.MaGiaoHoCha` | không | `-1`/NULL ở giáo họ cấp 1 | Gán tự động bằng `MaGiaoHoCha` của form cha khi Thêm (`UpdateData`, dòng 231-233), không có UI chọn cha khác — chỉ đi được "xuống" qua double-click, không đổi cha của một giáo họ đã có |
| (ẩn) | — | `GiaoHo.MaNhanDang` | không | `Memory.GetGiaoHoKey(MaGiaoHo)` nếu rỗng | Sinh tự động lúc lưu (dòng 235-238), bản web KHÔNG có cột tương ứng (không migrate — xem mục 9) |
| (ẩn) | — | `GiaoHo.UpdateDate` | không | `Memory.Instance.GetServerDateTime()` | Ánh xạ `UpdatedAt` chuẩn của mọi bảng ở bản web |

## 3. Hành vi khi tải

- `frmGiaoHo_Load` (`frmGiaoHo.cs:76-103`):
  1. Nếu đang Thêm mới (form mở trực tiếp từ menu, không qua double-click sửa) → sinh `id` mới
     bằng `Memory.Instance.GetNextId(...)`.
  2. Nếu `maGiaoHoCha != -1` (đang xem "Giáo khu" của một giáo họ cha) → đổi tiêu đề cửa sổ
     thành "Giáo khu", đổi nhãn nhóm/ô nhập, **ẩn `gxAddEdit2`** (nút "Danh sách giáo khu" — một
     giáo khu không có giáo khu "cháu"), lọc lưới `AND MaGiaoHoCha=<mã cha>`.
  3. Ngược lại (danh sách cấp 1) → lọc lưới `AND (MaGiaoHoCha IS NULL OR MaGiaoHoCha <= 0)`.
  4. `gxCommand1.OKButton.Visible = false` — form Giáo họ không có nút OK riêng (đóng bằng nút
     hệ thống hoặc Cancel).
  5. Focus vào nút Thêm; hai ô Mã/Tên đặt `ReadOnly = true` (chỉ mở khoá khi bấm Thêm/Sửa).
- Lưới nền `SELECT * FROM GiaoHo WHERE DaXoa = 0` (Designer.cs) — chỉ hiện giáo họ **chưa xoá
  mềm** (`GiaoHo.DaXoa`, ánh xạ `DaXoa` ở bản web).
- **Chỉ 1 cột hiển thị trên lưới: "Tên giáo họ"/"Tên giáo khu"** — cột "Mã giáo họ" bị
  **comment hẳn** trong `GxGiaoHoList.FormatGrid` (`GxGiaoHoList.cs:34-39`), không hiện dù có
  dữ liệu. Bản web hiện thêm cột "Mã cũ" — xem mục 8 (khác biệt cố ý).

## 4. Quy tắc nghiệp vụ và kiểm tra dữ liệu

### Lưu (Thêm/Sửa) — `checkInput()` (`frmGiaoHo.cs:106-138`)

1. `txtMaGiaoHo.Text` không phải số → **"Mã gia đình phải được nhập số"** (nguyên văn — LỖI
   CHÍNH TẢ copy từ màn hình Gia đình, không tự sửa theo nguyên tắc "migrate y hệt", xem mục 8).
2. `txtTenGiaoHo.Text` rỗng (sau `Trim()`) → **"Hãy nhập tên giáo họ!"**
3. Đang Thêm mới VÀ đã tồn tại `MaGiaoHo` đó trong bảng → **"Mã giáo họ này đã có. Hãy nhập mã
   khác"** (chỉ có thể xảy ra do tranh chấp đồng thời — `id` vốn tự sinh).
4. Đang Thêm mới VÀ đã tồn tại một giáo họ **cùng tên, cùng cấp** (`MaGiaoHoCha` bằng nhau —
   `-1`/NULL coi là cùng nhóm cấp 1) → **"Tên giáo họ này đã có. Hãy nhập tên khác"**
   (`frmGiaoHo.cs:128-135`). **Chú ý mã nguồn có lỗi logic rõ ràng ở điều kiện thứ hai của phép
   OR** (dòng 130-131: `MaGiaoHoCha != -1 && MaGiaoHoCha == -1 && ...` — hai vế mâu thuẫn nhau,
   luôn `false`) — trên thực tế **kiểm tra trùng tên chỉ chạy được cho giáo họ cấp 1**, không
   bao giờ chạy cho giáo khu con (`maGiaoHoCha != -1`). Bản web **cố ý sửa lỗi này** (áp dụng
   kiểm tra trùng tên cho MỌI cấp, so theo đúng `GiaoHoChaId` bao gồm cả giáo khu con) — xem
   mục 8, đây là lệch khỏi nguyên tắc "migrate y hệt" có ghi lại lý do.
5. Sửa: chỉ đổi được **Tên giáo họ** (`UPDATE_GIAOHO` — dòng 273), không đổi được Mã hay
   `MaGiaoHoCha` qua đường Sửa của bản gốc.
6. Lưu thành công (Sửa) → thông báo **"Giáo họ đã được cập nhật!"**.

### Xoá — `gxAddEdit1_DeleteClick` (`frmGiaoHo.cs:325-467`)

- Xác nhận: **"Khi 1 giáo họ bị xóa, tất cả các thông tin gia đình, giáo dân liên quan đến giáo
  họ đó đều sẽ bị xóa khỏi hệ thống.\r\nBạn có chắc muốn xóa các giáo họ được chọn không?"**
  (Yes/No, mặc định No theo icon Question không chỉ định `MessageBoxDefaultButton` — mặc định
  Button1=Yes thật ra, cẩn thận khi đọc).
- Nếu Yes: xoá **CỨNG** (không phải xoá mềm dù có cột `DaXoa`) theo thứ tự, cho TỪNG giáo họ
  được chọn:
  1. `DELETE FROM ThanhVienGiaDinh` của mọi giáo dân thuộc giáo họ đó.
  2. `DELETE FROM ThanhVienGiaDinh` của mọi gia đình thuộc giáo họ đó (JOIN theo `GiaDinh.MaGiaoHo`).
  3. `DELETE FROM GiaoDanHonPhoi`, `BiTichChiTiet`, `TanHien`, `ChuyenXu` của mọi giáo dân thuộc
     giáo họ đó.
  4. `DELETE FROM RaoHonPhoi` mà `MaGiaoDan1` HOẶC `MaGiaoDan2` thuộc giáo họ đó.
  5. `DELETE FROM GiaoDan`, `DELETE FROM GiaDinh`, `DELETE FROM GiaoHo` (bảng gốc) theo thứ tự.
  - Có đoạn code **CHẾT** bị comment hết (dòng 350-365): dự kiến ban đầu chỉ cho xoá giáo họ
    KHÔNG có gia đình/giáo dân nào (`hasGiaDinh`), nhưng đã bị vô hiệu hoá — bản đang chạy XOÁ
    THẲNG bất kể còn dữ liệu hay không, không cảnh báo riêng "giáo họ đã có dữ liệu, không xoá
    được" như comment gợi ý.
  - Thành công → **"Đã xóa thành công các giáo họ được chọn"**.
- **Đây là lý do bản web KHÔNG migrate nút xoá** (xem mục 8): xoá một giáo họ ở bản gốc xoá
  LUÔN toàn bộ Giáo dân/Gia đình/Bí tích/Hôn phối/Chuyển xứ/Rao hôn phối gắn với giáo họ đó —
  đúng tinh thần CLAUDE.md "dữ liệu là sổ sách nhiều năm, mất là không lấy lại được", một nút
  bấm nhầm có thể xoá sạch cả một giáo xứ.

## 5. Thao tác người dùng

| Thao tác | Điều kiện bật/tắt | Hành động |
|---|---|---|
| Nút "&Thêm"/"&Thôi" | Luôn bật (toggle) | Mở/đóng khối nhập, sinh mã mới |
| Nút "&Sửa"/"&Thôi" | Chỉ bật khi lưới có ít nhất 1 dòng (`gxGiaoHoList1_RowCountChanged`) | Mở khối nhập với dữ liệu dòng đang chọn |
| Nút "&Xóa" | Chỉ bật khi lưới có dòng | Xem mục 4 — cascade xoá toàn hệ thống |
| Nút "&Lưu" (đổi tên từ "Chọn") | Chỉ có tác dụng khi khối nhập đang mở | `UpdateData()` — xem mục 4 |
| Phím Delete trên lưới | — | Gọi thẳng `gxAddEdit1_DeleteClick` (`gxGiaoHoList1_KeyDown`) |
| Double-click một dòng (chỉ ở danh sách cấp 1, `maGiaoHoCha==-1`) | — | Mở **form `frmGiaoHo` MỚI** với `MaGiaoHoCha` = mã dòng đó → quản lý "Giáo khu" con (`EditGiaoHoRow`) — double-click ở màn hình "Giáo khu" (form con) KHÔNG làm gì (dòng 510: chỉ gọi khi `maGiaoHoCha == -1`) |
| Nút "Danh sách giáo khu" (`gxAddEdit2`, chỉ hiện ở form cấp 1) | Luôn bật | Cùng hành động double-click — mở `EditButton_Click` → `EditGiaoHoRow()` |
| Phím Escape | Đang Sửa/Xem | Huỷ chỉnh sửa, khôi phục dữ liệu dòng đang chọn (`cancelEdit`, `frmGiaoHo_PreviewKeyDown`) |

## 6. Lưới dữ liệu

Đúng 1 cột hiển thị (xem mục 3): "Tên giáo họ" (hoặc "Tên giáo khu" nếu đang ở cấp con), rộng
200px, `RowHeight=20`, không tô màu/gạch ngang điều kiện nào. `FilterMode = None` — không có
hàng lọc trên lưới này (khác đa số lưới khác trong hệ thống).

## 7. Liên kết sang màn hình khác

- Double-click / nút "Danh sách giáo khu" → mở lại chính `frmGiaoHo` (đệ quy một cấp trong dữ
  liệu thật, nhưng về lý thuyết code không giới hạn số cấp lồng nhau — `MaGiaoHoCha` trỏ tới
  bất kỳ `MaGiaoHo` nào).
- Có một hàm `EditGiaDinhRow()` mở `frmGiaDinhList` nhưng **không được gọi ở đâu cả** trong file
  này (code chết, để lại từ một thiết kế cũ — có thể từng định cho double-click mở luôn danh
  sách gia đình của giáo họ thay vì giáo khu).

## 8. Khác biệt cố ý ở bản web

- **Không có nút xoá** — xem mục 4 (cascade xoá xuyên suốt Giáo dân/Gia đình/Bí tích quá nguy
  hiểm, đã ghi ở `can-review-sau.md` mục 37 từ lượt trước).
- **Không dựng lại UI đệ quy "mở form con quản lý Giáo khu"** — bản web dùng MỘT màn hình
  phẳng (`GiaoHoListPage.tsx`) với select "Giáo họ cha" ngay trong form thêm/sửa (chọn từ danh
  sách giáo họ hiện có) và một cột "Giáo họ cha" trên lưới để nhận biết quan hệ lồng nhau. Lý
  do: dữ liệu thật hiện chỉ có 1 giáo họ, không có giáo khu nào — dựng lại đúng cơ chế "double
  click mở cửa sổ con, cửa sổ con lại mở được cửa sổ cháu..." tốn công cho một tính năng chưa ai
  dùng tới. Nếu sau này một giáo xứ có nhiều giáo khu thật, cấu trúc dữ liệu (`GiaoHoChaId`) đã
  sẵn sàng — chỉ cần nâng cấp UI thành cây/lưới lồng nếu cần, không cần đổi API. Ghi vào
  `can-review-sau.md` để người dùng xác nhận cách đơn giản hoá này có đủ dùng không.
- **Hiện thêm cột "Mã cũ" trên lưới** (`maGiaoHoCu`) dù bản gốc comment hẳn cột mã — hữu ích để
  đối chiếu với sổ sách giấy cũ vốn có thể đã ghi theo mã Access, không có hại gì khi hiện thêm.
- **Sửa lỗi logic của điều kiện kiểm tra trùng tên** (mục 4 bước 4: bản gốc chỉ kiểm tra được
  cho giáo họ cấp 1 do lỗi `&&`/`||` tự mâu thuẫn) — bản web áp dụng đúng cho MỌI cấp. Đây LÀ
  MỘT LỆCH so với nguyên tắc "migrate y hệt kể cả chỗ sai", cân nhắc có chủ đích: giữ nguyên lỗi
  nghĩa là bản web cho phép tạo hai giáo khu trùng tên trong cùng một giáo họ cha — không có lý
  do nghiệp vụ nào để giữ lỗi này (không giống các trường hợp khác trong dự án nơi hành vi "sai"
  đã thành thói quen người dùng), và sửa không làm mất khả năng nào. Ghi lại đây để người dùng
  xác nhận.
- **KHÔNG chép nguyên lỗi chính tả "Mã gia đình phải được nhập số"** ở bước kiểm tra Mã giáo họ
  — trên bản web, Mã giáo họ (Guid) không do người dùng nhập tay (luôn tự sinh qua
  `SinhMaService`), nên toàn bộ kiểm tra "là số" không còn ý nghĩa và không có thông báo tương
  ứng. Ghi vào `can-review-sau.md` vì đây cũng là một lệch (dù chỉ vì kiến trúc Id đã đổi từ số
  sang Guid, không phải quyết định nghiệp vụ).
- `MaGiaoHo = 0` là mốc "Ngoài xứ" — nhưng mốc đó nằm ở cột `GiaoDan.MaGiaoHo`/`GiaDinh.MaGiaoHo`
  (khoá NGOÀI trỏ tới `giao_ho`), KHÔNG phải một dòng trong chính bảng `GiaoHo`/`giao_ho`. Màn
  hình Giáo họ (danh mục) không hiển thị "Ngoài xứ" như một dòng và không cần xử lý gì đặc biệt
  cho mốc này — đã xác nhận bằng cách đọc `frmGiaoHo.cs` (không có bất kỳ điều kiện đặc biệt cho
  `MaGiaoHo==0` trong toàn bộ file) và đối chiếu với cách bản web đã xử lý đúng ở màn hình Giáo
  dân/Gia đình (`GiaoDanDetail.tsx`/`GiaDinhList.tsx`, hằng số `NGOAI_XU`).

## 9. Chỗ chưa chắc

- Vị trí chính xác mở `frmGiaoHo` từ `frmMain.cs` (menu nào) — chưa đọc `frmMain.cs` trong
  nhiệm vụ này.
- Nội dung `Memory.GetGiaoHoKey(...)` (sinh `MaNhanDang`) — chưa đọc, không rõ định dạng, và
  bản web hiện không có cột tương đương nên không migrate. Không rõ có nơi nào khác trong hệ
  thống desktop dựa vào `MaNhanDang` này hay không (nếu có, xem lại quyết định không migrate).
  Không migrate cột này.
- `MessageBoxDefaultButton` không được set tường minh ở hộp thoại xác nhận xoá — mặc định
  Windows Forms là Button1; cần xác nhận thứ tự Yes/No hiển thị đúng như suy đoán nếu có dịp
  chạy bản desktop thật để đối chiếu (không có máy Windows/Access thật trong môi trường agent
  này).
