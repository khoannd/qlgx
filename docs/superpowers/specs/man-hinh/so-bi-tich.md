# Màn hình: Danh sách sổ bí tích (`frmDotBiTichList.cs` + `frmBiTichChiTiet.cs`)

| | |
|---|---|
| Tệp nguồn | `Source/ChuongTrinh/frmDotBiTichList.cs` (176 dòng) + `.Designer.cs`; `Source/ChuongTrinh/frmBiTichChiTiet.cs` (574 dòng) + `.Designer.cs` |
| UserControl dùng lại | `GxDotBiTichList` (`Source/GXControl/GxDotBiTichList.cs`, 113 dòng, lưới đợt), `GxBiTichChiTiet` (`Source/GXControl/GxBiTichChiTiet.cs`, 454 dòng, lưới người nhận trong một đợt), `GxHonPhoiList`/`GxLoaiBiTich`/`GxAddEdit`/`GxCommand` |
| Bảng dữ liệu đụng tới | `DotBiTich` (7 cột), `BiTichChiTiet` (4 cột — chỉ `MaDotBiTich`, `MaGiaoDan`, `GhiChu`, `UpdateDate`), **và `GiaoDan`** (10-13 cột theo loại: `SoRuaToi/NgayRuaToi/NoiRuaToi/ChaRuaToi/NguoiDoDauRuaToi`, tương tự cho `RuocLe`/`ThemSuc`) |
| Trạng thái migrate | một phần — xem mục 10 |

## 0. Có HAI đường trong một màn hình danh sách — chỉ nhánh Rửa tội/Rước lễ/Thêm sức được migrate ở đây

`frmDotBiTichList` xử lý **bốn** loại bí tích qua combo `cbLoaiBiTich` (Rửa tội=0, Rước lễ=1,
Thêm sức=2, **Hôn phối=3** — `SearchDotBiTich`, `frmDotBiTichList.cs:47-74`): khi chọn Hôn
phối, toàn bộ màn hình chuyển sang lưới **`gxHonPhoiList1`** (bảng `HonPhoi`/`GiaoDanHonPhoi`,
khác hẳn `DotBiTich`) — nhánh này đã có màn hình + spec riêng (`hon-phoi.md`), **không thuộc
phạm vi spec này**. An táng (4) và Xức dầu (5) tồn tại trong enum `LoaiBiTich` nhưng KHÔNG có
mặt trong combo `cbLoaiBiTich` của bản desktop (chỉ 3 mục đầu, xem `GxLoaiBiTich.cs` — chưa đọc
sâu, suy từ dữ liệu thật chỉ có 0/1/2) và **không có cột `GiaoDan` tương ứng** trong CSDL đã di
trú (không có `NgayAnTang`, `SoXucDau`…) — bản web migrate đúng phạm vi desktop: chỉ Rửa
tội/Rước lễ/Thêm sức.

## 1. Mục đích

Quản lý các **đợt cử hành bí tích** (một đợt = một buổi lễ, có ngày/linh mục/nơi chốn chung)
và **danh sách giáo dân nhận bí tích** trong đợt đó. Đây là khối dữ liệu lớn nhất CSDL: 1108
đợt (`DotBiTich`), 6150 lượt nhận (`BiTichChiTiet`) trong dữ liệu khảo sát (giáo xứ Vô Nhiễm).

Mở từ menu chính (điểm gọi cụ thể ngoài phạm vi hai tệp được giao — mục 9).

## 2. Bố cục và các trường

### 2.1 `frmDotBiTichList` — màn hình danh sách, chọn loại + khoảng năm

| Nhãn hiển thị | Control | Cột CSDL | Bắt buộc? | Mặc định | Ghi chú |
|---|---|---|---|---|---|
| (không nhãn, combo loại) | `cbLoaiBiTich` | lọc `DotBiTich.LoaiBiTich` | có (để tìm) | không chọn sẵn | Chưa chọn mà bấm "Tìm kiếm" → chặn, xem mục 4 |
| Từ năm | `cbTuNam` | lọc năm của `NgayBiTich` | không | rỗng | Liệt kê năm hiện tại lùi về 1900 (`frmDotBiTichList.cs:27-31`) |
| Đến năm | `cbDenNam` | lọc năm của `NgayBiTich` | không | **năm hiện tại** | Gán ở `frmDotBiTichList_Load` (dòng 81) |

### 2.2 Lưới `GxDotBiTichList` — đúng 5 cột, thứ tự (`GxDotBiTichList.cs:62-91`)

| Cột | Nhãn | Rộng | Định dạng |
|---|---|---|---|
| `NgayBiTich` | Ngày | 100 | `dd/MM/yyyy` |
| `MoTa` | Mô tả | 200 | text |
| `LinhMuc` | Người ban bí tích | 200 | text tự do |
| `NoiBiTich` | Nơi nhận bí tích | 200 | text |
| (tính) `SoLuong` | Số lượng GD | 200 | số nguyên — subquery `SL` đếm `BiTichChiTiet` (`SqlConstants.SELECT_DOTBITICH_LIST`, dòng 310 trở đi) |

### 2.3 `frmBiTichChiTiet` — chi tiết một đợt

| Nhãn hiển thị | Control | Cột CSDL | Bắt buộc? | Mặc định | Ghi chú |
|---|---|---|---|---|---|
| Mã đợt bí tích | `txtMaDotBiTich` | `DotBiTich.MaDotBiTich` | tự sinh | `Memory.Instance.GetNextId(...)` khi Thêm mới (dòng 87) | `ReadOnly=true` khi Sửa (dòng 89) |
| Mô tả | `txtMoTa` | `DotBiTich.MoTa` | **có** | tự gợi ý khi chọn xong Ngày, xem mục 4 | |
| Ngày bí tích | `dtNgayBiTich` | `DotBiTich.NgayBiTich` | không | `IsNullDate=true` (dòng 67) | |
| Linh mục | `txtLinhMuc` | `DotBiTich.LinhMuc` | không | | text tự do, KHÔNG phải khoá ngoại tới bảng `LinhMuc` |
| Nơi nhận bí tích | `txtNoiBiTich` | `DotBiTich.NoiBiTich` | không | **Tên giáo xứ hiện tại** (`Memory.GiaoXuInfo`, dòng 74) | |

### 2.4 Lưới `GxBiTichChiTiet` (người nhận trong đợt) — cột đổi theo loại (`GxBiTichChiTiet.cs:284-395`)

| Cột | Nhãn hiển thị theo loại | Cột CSDL thật lưu | Rộng |
|---|---|---|---|
| Số bí tích | "Số rửa tội" / "Số XTRL" / "Số thêm sức" | `GiaoDan.SoRuaToi` / `SoRuocLe` / `SoThemSuc` | 80 |
| Tên thánh | Tên thánh | `GiaoDan.TenThanh` | 100 |
| Họ tên | Họ tên | `GiaoDan.HoTen` | 150 (không cho sửa) |
| Phái | Phái | `GiaoDan.Phai` | 50 (không cho sửa) |
| Ngày sinh | Ngày sinh | `GiaoDan.NgaySinh` | 80, `dd/MM/yyyy` (không cho sửa) |
| Mã GĐ / Tên GĐ | Mã GĐ / Tên GĐ | gán tạm qua `SelectGiaDinh()`, không lưu cột nào ở `BiTichChiTiet` | 50/80 — **không migrate**, xem mục 8 |
| Người đỡ đầu | Người đỡ đầu | `GiaoDan.NguoiDoDauRuaToi` / `NguoiDoDauThemSuc` — **chỉ có ở Rửa tội, Thêm sức** (`loaiBiTich == RuaToi \|\| ThemSuc`, dòng 379-388); **Rước lễ không có cột này** | 150 |
| Ghi chú | Ghi chú | `BiTichChiTiet.GhiChu` | 200 |

**Điểm quan trọng nhất của toàn màn hình:** "Số bí tích"/"Người đỡ đầu" **KHÔNG nằm trong bảng
`BiTichChiTiet`** — chúng là các cột trên chính bảng `GiaoDan`, khớp đúng loại bí tích. Bảng
`BiTichChiTiet` chỉ có 4 cột: `MaDotBiTich`, `MaGiaoDan`, `GhiChu`, `UpdateDate`. Cách bố trí
này khớp với comment sẵn có trong entity `BiTichChiTiet.cs` ("bảng lớn nhất CSDL — ai nhận bí
tích trong đợt nào") và đã kiểm chứng bằng đoạn code xoá bí tích (mục 4).

## 3. Hành vi khi tải

- `frmDotBiTichList`: KHÔNG tự tải lưới lúc mở — `gxAddEdit1.Enabled = false`,
  `uiGroupBox1.Visible` mặc định ẩn (constructor, dòng 20-24). Phải chọn Loại bí tích rồi bấm
  nút "Tìm kiếm" (`btnSearch`, gắn `SearchDotBiTich`) mới tải (dòng 26, 35-74). Đến năm mặc
  định = năm hiện tại (`frmDotBiTichList_Load`, dòng 77-82).
- `SearchDotBiTich` chặn NGAY nếu chưa chọn `cbLoaiBiTich` (mục 4) — không gọi API nào.
- Lọc năm dùng biểu thức Access `INT(IIF(LEN([NgayBiTich])>=1, RIGHT([NgayBiTich],4), "0000"))`
  so sánh với năm nhập — nghĩa là đợt **CHƯA có ngày** được coi là năm **"0000"**:
  - Với "Từ năm" (`>=`): năm 0 hầu như không bao giờ thoả `>= tuNam` (trừ khi người dùng nhập
    "Từ năm" = 0, không xảy ra trên UI) → đợt chưa có ngày bị loại khi có lọc "Từ năm".
  - Với "Đến năm" (`<=`): năm 0 **LUÔN LUÔN** thoả `<= denNam` → đợt chưa có ngày **VẪN được
    giữ lại** khi chỉ có lọc "Đến năm" (trường hợp mặc định, vì "Đến năm" luôn có giá trị =
    năm hiện tại). Bản web `DotBiTichService.LayDanhSach` cố tình tái tạo đúng quy tắc bất đối
    xứng này (`d.NgayBiTich == null || d.NgayBiTich.Value.Year <= den` cho vế "Đến năm", còn vế
    "Từ năm" loại thẳng NULL) — lần viết đầu tiên đã vô tình loại cả các đợt NULL ở cả hai vế,
    làm mất 26 đợt "Rửa tội" không có ngày (754 thay vì 780 đợt thật) khi kiểm tra bằng trình
    duyệt thật; đã sửa và xác nhận lại đúng 780 đợt trước khi bàn giao.
- `frmBiTichChiTiet` (Sửa): `AssignControlData()` tra theo `MaDotBiTich`, nạp 4 trường đầu +
  gọi `gxBiTichChiTiet1.MaDotBiTich = ...` để tự tải lưới người nhận
  (`SqlConstants.SELECT_BITICH_CHITIET_THEODOT`, sắp theo cột Số bí tích tương ứng, TĂNG DẦN).
- `frmBiTichChiTiet` (Thêm mới): sinh `MaDotBiTich` kế tiếp ngay lúc `Load` (dòng 87); tiêu đề
  cửa sổ nối thêm `" - " + TenBiTich` (dòng 96, ví dụ "... - Rửa tội").

## 4. Quy tắc nghiệp vụ và kiểm tra dữ liệu

### Khi tìm kiếm (`frmDotBiTichList.SearchDotBiTich`)

- Chưa chọn Loại bí tích → **"Xin vui lòng chọn một loại bí tích cần xem"** (dòng 38), focus lại
  combo, không tải gì (`frmDotBiTichList.cs:37-42`).

### Khi thêm một giáo dân vào đợt (`frmBiTichChiTiet.addGiaoDan`, dòng 139-239)

Thứ tự kiểm tra:
1. Đã có trong lưới hiện tại (cùng đợt) → **"Đã tồn tại giáo dân này trong danh sách. Vui lòng
   nhập giáo dân khác"** (dòng 170, tiêu đề "Thông báo", icon Error).
2. Đã có ở **đợt khác cùng loại bí tích** (`SELECT_BITICH_CHITIET_THEOLOAI` lọc thêm
   `MaGiaoDan` + `LoaiBiTich`) → **"Đã tồn tại giáo dân này trong đợt bí tích {TenBiTich} khác.
   Vui lòng nhập giáo dân khác"** (dòng 179) — ví dụ "...trong đợt bí tích Rửa tội khác...".
   Nói cách khác: **một giáo dân chỉ được nhận MỘT loại bí tích (Rửa tội/Rước lễ/Thêm sức)
   đúng MỘT lần trong toàn hệ thống**, không phân biệt đợt.
3. Cảnh báo (không chặn) nếu giáo dân đang ở trạng thái đặc biệt — gộp nhiều cờ vào MỘT câu,
   nối bằng dấu phẩy, thứ tự đã qua đời → đã chuyển xứ → đã xoá (dòng 187-221):
   > "Giáo dân {Tên} hiện tại {đã qua đời[, đã chuyển xứ][, đã xóa]}. Bạn có muốn tiếp tục thêm
   > giáo dân này.\r\nChọn [Yes] để tiếp tục.\r\nChọn [No] để hủy."
   Chọn **No** → huỷ thêm.

### Khi lưu cả đợt (`frmBiTichChiTiet.checkInput`, dòng 277-308)

Thứ tự kiểm tra, dừng ở lỗi đầu:
1. Mã đợt không phải số (chỉ khi Sửa) → **"Mã gia đình phải được nhập số"** (dòng 281 — **lỗi
   chính tả trong bản gốc: thông báo ghi "gia đình" nhưng đang kiểm tra Mã đợt BÍ TÍCH**, chép
   nguyên văn theo yêu cầu migrate y hệt).
2. Mô tả trống → **"Hãy mô tả cho đợt bí tích này!"** (dòng 288).
3. Chưa có ai trong danh sách người nhận (`RowCount == 0`) → **"Hãy nhập ít nhất 1 giáo dân
   trong danh sách những người chịu bí tích"** (dòng 295).
4. Đang Thêm mới và Mã đợt đã tồn tại (kiểm tra lại CSDL) → **"Mã gia đình này đã tồn tại. Hãy
   nhập mã khác!"** (dòng 302 — **cùng lỗi chính tả "gia đình"**).

### Khi bấm "Cập nhật" (`gxCommand1_OnOK`, dòng 435-543)

- Sau `checkInput()`, quét toàn bộ lưới người nhận: nếu có dòng còn **trống "Số bí tích"** →
  hỏi 3 lựa chọn (dòng 462-479):
  > "Có giáo dân chưa được nhập [{nhãn theo loại}].\r\nBạn có muốn tiếp tục cập nhật danh sách
  > bí tích không?\r\nNhấp nút [Yes] để cập nhật và đóng mà hình\r\nNhấp nút [No] để đóng mà
  > hình mà không cập nhật danh sách\r\nNhấp nút [Cancel] để quay lại nhập dữ liệu, không cập
  > nhật và cũng không đóng màn hình\r\n"
  (**lỗi chính tả trong bản gốc: "mà hình" đúng ra "màn hình", lặp lại 2 lần** — chép nguyên
  văn). Yes → tiếp tục lưu; No → đóng, huỷ (`DialogResult.Cancel`); Cancel → ở lại, không lưu.
- **Đồng bộ ngược:** khi lưu, Ngày/Linh mục/Nơi của **đợt** được copy xuống cột tương ứng trên
  `GiaoDan` (`NgayRuaToi`/`ChaRuaToi`/`NoiRuaToi`…) của **MỌI người trong lưới**, kể cả người
  cũ đã có sẵn (`row[gxBiTichChiTiet1.NgayBiTichColumnName] = dtNgayBiTich.Value`, dòng
  385-387) — sửa ngày của đợt sẽ **ghi đè** ngày bí tích cá nhân của tất cả thành viên đợt đó,
  không hỏi xác nhận riêng.
- Xoá một người khỏi lưới (dòng 356-382): xoá `BiTichChiTiet` tương ứng; NẾU người dùng đã xác
  nhận "xoá cả thông tin" ở hộp thoại tại `gxAddEdit1_DeleteClick` (mục 5) thì **null hoá toàn
  bộ cột bí tích liên quan trên `GiaoDan`** (ví dụ Rửa tội: `NgayRuaToi, SoRuaToi,
  NguoiDoDauRuaToi, ChaRuaToi, NoiRuaToi` — dòng 365-379).
- Lỗi ngoại lệ khi lưu → `MessageBox` tiêu đề **"Lỗi Exception (frmBiTichChiTiet,
  gxCommand1_OnOK)"** kèm `ex.Message` (dòng 540).

### Khi xoá một người khỏi lưới (`gxAddEdit1_DeleteClick`, dòng 246-264)

Hai hộp thoại liên tiếp:
1. **"Bạn có chắc muốn loại bỏ giáo dân này ra khỏi danh sách không?"** (Yes/No) — No thì dừng.
2. Yes ở bước 1 → hỏi tiếp **"Bạn có muốn xóa cả thông tin [{TenBiTich}] của giáo dân này
   không?"** (ví dụ "...xóa cả thông tin [Rửa tội]...") — Yes thì đánh dấu null hoá cột GiaoDan
   khi lưu (xem mục trên); No thì chỉ xoá liên kết `BiTichChiTiet`, giữ nguyên dữ liệu bí tích
   cá nhân trên `GiaoDan`.

### Khi xoá cả một đợt (`gxAddEdit1_DeleteClick` của `frmDotBiTichList`, dòng 117-127)

- **"Bạn có chắc muốn loại bỏ đợt bí tích này ra khỏi danh sách?"** (Yes/No, icon Question).
- Yes → xoá **CỨNG** (`Memory.DeleteRows`) cả `BiTichChiTiet` của đợt lẫn `DotBiTich` — **KHÔNG
  null hoá lại** các cột bí tích trên `GiaoDan` của những người từng ở trong đợt (khác hẳn xoá
  từng người — mục trên). Dữ liệu cá nhân "mồ côi", không còn liên kết về đợt lễ nào.

## 5. Thao tác người dùng

| Thao tác | Điều kiện bật/tắt | Hành động |
|---|---|---|
| Nút "Tìm kiếm" (`btnSearch`) | luôn bật | Tải lưới theo Loại/Từ năm/Đến năm; bật `gxAddEdit1`, hiện `uiGroupBox1` |
| Thêm đợt (`gxAddEdit1_AddClick`, `frmDotBiTichList`) | | Mở `frmBiTichChiTiet` (`Operation=ADD`), nhập xong `ImportRow` vào lưới |
| Sửa đợt / nhấp đúp dòng | cần đã chọn dòng | Mở `frmBiTichChiTiet` (`Operation=EDIT`), gọi `AssignControlData()` trước khi hiện |
| Xoá đợt | cần đã chọn dòng | Xem mục 4 — xoá cứng |
| "Chọn &gia đình" (`gxAddEdit1.Button1`, `frmBiTichChiTiet`) | chỉ bật khi dòng đang chọn CHƯA có `MaGiaDinhCo` (`gxDotBiTichChiTiet1_SelectionChanged`, dòng 556-573) | `SelectGiaDinh()` — **không migrate**, xem mục 8 |
| "&Xem chi tiết" / nhấp đúp một người | | Mở `frmGiaoDan` (`Operation=EDIT`, chỉ xem) cho giáo dân đó |
| "&Chọn giáo dân" (`gxAddEdit1.SelectClick`) | | Mở `frmChonDuLieu` tìm giáo dân có sẵn — nếu cấu hình `CF_SOBITICH_HIENTATCAGIAODAN` bật thì lọc chỉ hiện người CHƯA có ngày bí tích loại này (dòng 121-124) |
| "&Thêm" người mới (`gxAddEdit1_AddClick`, `frmBiTichChiTiet`) | | Mở `frmGiaoDan` trống để tạo giáo dân mới rồi thêm luôn vào đợt |
| Xoá 1 người khỏi lưới | cần đã chọn dòng | Xem mục 4 — hai hộp thoại liên tiếp |
| "In danh sách bí tích" (`gxAddEdit1.PrintButton`) | | `gxBiTichChiTiet1.Print()` — chưa đọc sâu, không migrate (chưa có in ấn danh sách ở web) |
| "In chứng &nhận" (`gxAddEdit1.Button2`) | | `gxBiTichChiTiet1.InChungNhan()` → `GxGiaoDanList.InChungNhanBiTich` — **CÓ hạ tầng in tương đương ở web** (`/api/giao-dan/{id}/in/chung-nhan-bi-tich`, xem `in-an.md`), nhưng chưa nối vào màn hình này |
| Chuột phải trên lưới người nhận | | "Xem chi tiết" / "In chứng nhận bí tích" (`GxBiTichChiTiet.cs:104-118`) |

## 6. Lưới dữ liệu

Xem mục 2.2 (danh sách đợt) và 2.4 (người nhận trong đợt) — đã liệt kê đủ cột/thứ tự/định dạng.
Không có tô màu/gạch ngang có điều kiện trên hai lưới này (khác lưới gia đình/giáo dân).

## 7. Liên kết sang màn hình khác

- Mở `frmBiTichChiTiet` (Thêm/Sửa một đợt), trả `DataReturn` (DataRow đợt) cho danh sách.
- Mở `frmGiaoDan` (xem/thêm giáo dân) từ nút "Thêm"/"Xem chi tiết" trên lưới người nhận.
- Mở `frmChonDuLieu` (tìm giáo dân có sẵn) từ nút "Chọn giáo dân".
- `SelectGiaDinh()` mở `frmChonDuLieu` (tìm gia đình) — không migrate, mục 8.
- In chứng nhận gọi `GxGiaDinhList.ChungNhanHonPhoi`/`GxGiaoDanList.InChungNhanBiTich` (xuất
  ngoài, không mở màn hình).

## 8. Khác biệt cố ý ở bản web

- **Không migrate "Chọn gia đình" cho từng người nhận** (cột Mã GĐ/Tên GĐ, nút "Chọn &gia
  đình", `SelectGiaDinh()`/`isValidGiaDinh()`) — luồng này gắn giáo dân mới rửa tội vào một
  ThanhVienGiaDinh ("con cái") của một gia đình có sẵn, đòi hỏi toàn bộ luồng chọn gia đình +
  kiểm tra trùng vai trò vợ/chồng của `GxBiTichChiTiet.isValidGiaDinh`. Bản web hiện chỉ thêm
  người đã có sẵn trong CSDL vào đợt bí tích, không tạo liên kết gia đình mới qua màn hình này
  — người dùng cần liên kết gia đình thì làm ở màn hình Gia đình/Giáo dân riêng. Xem
  `can-review-sau.md` để người dùng xác nhận có cần bổ sung sau không.
- **Không migrate nút "Chọn &gia đình"/"&Thêm" tạo giáo dân mới ngay tại màn hình này** — bản
  web dùng picker tìm giáo dân có sẵn (`GxPicker`, tương đương `frmChonDuLieu`) để thêm vào
  đợt; tạo giáo dân hoàn toàn mới thì làm ở màn hình "Danh sách giáo dân" trước, rồi quay lại
  đây thêm vào đợt.
- **Không migrate cảnh báo "đã qua đời/đã chuyển xứ/đã xoá" khi thêm người** (mục 4, quy tắc 3)
  — bản web thêm thẳng không cảnh báo. Ghi vào `can-review-sau.md`.
- **Không migrate hộp thoại "Có giáo dân chưa nhập Số bí tích..."** trước khi lưu — bản web
  không chặn lưu khi thiếu Số bí tích (không có cảnh báo tương đương). Ghi vào
  `can-review-sau.md`.
- **Chép nguyên văn hai lỗi chính tả** "Mã gia đình phải được nhập số"/"Mã gia đình này đã tồn
  tại" khi đang thao tác với **đợt bí tích** (không phải gia đình) — xem mục 4. Đây là bản
  desktop copy-paste nhầm thông báo từ màn hình gia đình, migrate y hệt theo yêu cầu người dùng.
- **Không migrate In danh sách/In chứng nhận** ở màn hình này (Task này) — hạ tầng in chứng
  nhận bí tích đã có sẵn cho màn hình Giáo dân (`in-an.md`), có thể nối vào đây ở lượt sau.
- **RowVersion chống ghi đè** cho từng đợt bí tích khi sửa — bản desktop không có cơ chế này.

## 9. Chỗ chưa chắc

- Điểm gọi mở `frmDotBiTichList` từ menu chính — ngoài phạm vi hai tệp được giao.
- Nội dung chính xác combo `cbLoaiBiTich` (`GxLoaiBiTich.cs`, chưa đọc) — suy đoán chỉ có 3 mục
  Rửa tội/Rước lễ/Thêm sức + Hôn phối dựa trên dữ liệu thật (0/1/2/3) và logic rẽ nhánh
  `SearchDotBiTich`, chưa xác nhận trực tiếp trong `GxLoaiBiTich.cs`.
- Ý nghĩa/hành vi chính xác của `SelectGiaDinh()` và `isValidGiaDinh()` (`GxBiTichChiTiet.cs`) —
  đã đọc đủ để biết KHÔNG migrate (mục 8) nhưng chưa lần theo hết `GxGiaDinhList.GetRowGiaDinhVoChong`.
- Cấu hình `CF_SOBITICH_HIENTATCAGIAODAN` (lọc "chỉ hiện giáo dân chưa có ngày bí tích") — chưa
  đọc `GxConstants` để biết giá trị mặc định/nơi bật tắt.

## 10. Đối chiếu bản web hiện tại (sau khi migrate)

| Hành vi bản desktop | Bản web đã có? | Ghi chú |
|---|---|---|
| Danh sách đợt theo loại + khoảng năm | **Có** | `GET /api/dot-bi-tich?loaiBiTich=&tuNam=&denNam=` — đã kiểm chứng trên trình duyệt thật: 780 đợt Rửa tội (đúng số liệu thật đã xác minh), 2050 người |
| Chặn tìm kiếm khi chưa chọn loại | **Có** | 400 Bad Request khi thiếu `loaiBiTich` — client tự chặn trước khi gọi |
| Xem danh sách người nhận trong đợt (đọc số bí tích/người đỡ đầu từ GiaoDan) | **Có** | `GET /api/dot-bi-tich/{id}` — đã xem thật 12 người trong đợt 02/04/2000 (Số rửa tội, Tên thánh, Ngày sinh, Người đỡ đầu đúng dữ liệu) |
| Thêm/Sửa/Xoá đợt | **Có** | `POST`/`PUT`/`DELETE /api/dot-bi-tich/{id}`, RowVersion chống ghi đè |
| Thêm người nhận + 2 lớp kiểm tra trùng (trong đợt / đợt khác cùng loại) | **Có** | `POST /api/dot-bi-tich/{id}/nguoi-nhan`, thông báo tiếng Việt y hệt |
| Đồng bộ Ngày/Linh mục/Nơi của đợt xuống GiaoDan mọi người khi Cập nhật đợt | **Có** | `DotBiTichService.CapNhat` — có test riêng xác nhận |
| Sửa Số bí tích/Người đỡ đầu/Ghi chú của một người | **Có** | `PUT /api/dot-bi-tich/{id}/nguoi-nhan/{giaoDanId}` |
| Xoá một người, tuỳ chọn null hoá cột GiaoDan | **Có** | `DELETE .../nguoi-nhan/{giaoDanId}?xoaThongTinBiTich=` |
| "Chọn gia đình" cho người nhận (liên kết ThanhVienGiaDinh) | **Thiếu** (cố ý, mục 8) | |
| Cảnh báo đã qua đời/chuyển xứ/xoá khi thêm người | **Thiếu** (cố ý, mục 8) | |
| Cảnh báo thiếu Số bí tích trước khi lưu | **Thiếu** (cố ý, mục 8) | |
| In danh sách / In chứng nhận | **Thiếu** ở màn hình này | Hạ tầng in chứng nhận bí tích đã có ở `in-an.md`, chưa nối vào đây |
