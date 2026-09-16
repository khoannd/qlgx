# Màn hình: Thống kê chung & Biểu đồ (`frmThongKeChung.cs`, `frmBieuDo.cs`)

| | |
|---|---|
| Tệp nguồn | `Source/ChuongTrinh/frmThongKeChung.cs` (28 dòng, UTF-16LE) + `Source/ChuongTrinh/frmThongKeChung.Designer.cs` (114 dòng) — chỉ là khung `TabControl` chứa 2 UserControl thật |
| UserControl dùng lại (Thống kê chung) | `Source/GXControl/GxThongKeChung.cs` (848 dòng, UTF-16LE) — tab "Thống kê chung"; `Source/GXControl/GxThongKeOnGoi.cs` (195 dòng, UTF-16LE) — tab "Thống kê ơn gọi tận hiến" |
| Lớp phụ trợ | `Source/GXControl/Extract.cs` (168 dòng, UTF-8) — trích xuất theo tuổi (Chủ hộ/Gia trưởng/Hiền mẫu/Cao niên/Giới trẻ/Thiếu nhi) |
| Tệp nguồn (Biểu đồ) | `Source/ChuongTrinh/frmBieuDo.cs` (400 dòng, UTF-8) + `Source/ChuongTrinh/frmBieuDo.Designer.cs` (234 dòng) |
| Mô-đun vẽ biểu đồ (desktop, Excel) | `Source/ExcelReport/ChartTongGiaoDan.cs`, `ChartTongHonPhoi.cs`, `ChartBiTich.cs`, `ChartDoTuoi.cs`, `ChartGiaoHo.cs` |
| Bảng dữ liệu đụng tới | `GiaoDan`, `GiaDinh`, `HonPhoi`/`GiaoDanHonPhoi`, `TanHien`, `GiaoHo`, `ChuyenXu`, `ThanhVienGiaDinh` |
| Trạng thái migrate | đang — xem mục 10 |

## 0. Vì sao gộp hai màn hình vào một spec

Người dùng giao cùng lúc "Thống kê chung" và "Biểu đồ" vì cả hai đọc **cùng một tập quy tắc lọc
dữ liệu** (điều kiện trích xuất theo ngày/tuổi) chỉ khác cách hiển thị kết quả — bảng số liệu so
với biểu đồ. Tách hai spec riêng sẽ trùng lặp gần hết nội dung.

## 1. Mục đích

- **Thống kê chung** (`frmThongKeChung`): màn hình có 2 tab.
  - Tab "Thống kê chung" (`GxThongKeChung`): chọn một trong 16 điều kiện trích xuất (sinh ra, rửa
    tội, hôn phối, qua đời, tổng số giáo dân/gia đình, chủ hộ, gia trưởng, hiền mẫu, cao niên,
    giới trẻ, thiếu nhi, kỷ niệm hôn phối…), nhập khoảng ngày hoặc khoảng tuổi tương ứng, rồi xem
    danh sách kết quả (giáo dân, gia đình, hoặc hôn phối) kèm tổng số dòng. Dùng để lập báo cáo
    năm gửi giáo phận (rửa tội bao nhiêu người, hôn phối bao nhiêu đôi…).
  - Tab "Thống kê ơn gọi tận hiến" (`GxThongKeOnGoi`): liệt kê giáo dân xuất thân từ giáo xứ đã đi
    tu/tận hiến (tu sĩ, chủng sinh, linh mục…), lọc theo ngày bắt đầu ơn gọi, chức vụ, nơi tu, dòng
    tu, nơi phục vụ.
- **Biểu đồ** (`frmBieuDo`): hộp thoại chọn 1 trong 5 loại biểu đồ (Tổng giáo dân theo năm, Tình
  hình bí tích theo năm, So sánh độ tuổi, So sánh giáo họ, Tổng hôn phối theo năm), bấm "Xem" xuất
  ra Excel kèm biểu đồ dựng sẵn (`ExcelReport.Chart*.Export`), Excel tự mở lên
  (`Process.Start(outputPath)`).

Cả hai đều mở từ menu chính (`frmMain`), không có tham số truyền vào.

## 2. Bố cục và các trường

### 2.1 Tab "Thống kê chung" (`GxThongKeChung`)

| Nhãn hiển thị | Control | Vai trò |
|---|---|---|
| (không nhãn, dropdown đầu) | `gxCbCondition` | 16 điều kiện trích xuất, xem mục 4.1 |
| Giáo họ | `cbGiaoHo` (`GxGiaoHo`) | lọc theo giáo họ, `-1`="Tất cả" |
| Từ ngày / Đến ngày | `dtDateFrom` / `dtDateTo` | hiện khi điều kiện dùng khoảng **ngày** |
| Từ tuổi / Đến tuổi | `gxCbAgeFrom` / `gxCbToAge` | hiện khi điều kiện dùng khoảng **tuổi** (1-149) |
| Trạng thái hôn phối | `gxCbMarried` | chỉ hiện khi điều kiện = Hôn phối/Kỷ niệm hôn phối |
| Tính cả trong hồ sơ lưu trữ | `chkLuuTru` | bỏ điều kiện `DaXoa=0 AND DaChuyenXu=0 AND QuaDoi=0` |
| Tính cả những dữ liệu không có ngày tháng | `chkNullAccept` | chỉ **Enabled** với một số điều kiện, xem mục 4.4 |
| (nút) Tìm kiếm | `btnSearch` | chạy trích xuất |
| (nút) In | `btnPrint` | in lưới đang hiện |
| (nút) Lọc | `btnFilter` | mở `frmFilter` lọc thêm trên lưới đã tải |
| (label) | `lblTotal` | "Tổng cộng: N \<hậu tố theo điều kiện\>" |
| Lưới kết quả | `gxGiaoDanList1` / `gxGiaDinhList1` / `gxHonPhoiList1` | ba lưới CHỒNG NHAU, chỉ 1 cái `Visible=true` tuỳ điều kiện |

### 2.2 Tab "Thống kê ơn gọi tận hiến" (`GxThongKeOnGoi`)

| Nhãn hiển thị | Control | Vai trò |
|---|---|---|
| Giáo họ | `cbGiaoHo` | có mặt nhưng **KHÔNG dùng để lọc** trong `btnSearch_Click` — xem mục 9 |
| Từ ngày / Đến ngày | `dtDateFrom` / `dtDateTo` | lọc theo `TanHien.NgayBatDau` |
| Chức vụ | `cbChucVu` | danh sách cứng: "", Tu sĩ, Chủng sinh, Phó tế, Linh mục, Giám mục, Khấn trọn, Khác (`GxThongKeOnGoi.cs:26-33`) — so khớp `LIKE '%value%'`, rỗng = mọi giá trị |
| Nơi tu | `txtNoiTu` | gõ tự do + gợi ý tần suất, so khớp `LIKE '%value%'` |
| Dòng tu | `txtDongTu` | như trên |
| Nơi phục vụ | `txtNoiPhucVu` | như trên |
| Tính cả trong hồ sơ lưu trữ | `chkLuuTru` | |
| Tính cả những dữ liệu không có ngày tháng | `chkNullAccept` | luôn Enabled (không có `EnableChk`) |
| (nút) Tìm kiếm/In/Lọc | như tab kia | |
| (label) | `lblTotal` | "Tổng cộng: N người theo ơn gọi" |
| Lưới kết quả | `gxGiaoDanList1` | cột giáo dân + cột tận hiến (`NgayBatDau`, `ChucVu`, `NoiTu`, `DongTu`, `NoiPhucVu`) |

### 2.3 `frmBieuDo`

| Nhãn hiển thị | Control | Vai trò |
|---|---|---|
| Tổng giáo dân | `rdTongGiaoDan` | biểu đồ cột, tổng giáo dân **luỹ kế** đến 31/12 mỗi năm trong khoảng chọn |
| Tình hình bí tích | `rdBiTich` | biểu đồ cột, 4 chuỗi: Sinh ra/Rửa tội/XTRL lần đầu/Thêm sức theo năm |
| So sánh độ tuổi (không phụ thuộc năm thống kê) | `rdDoTuoi` | biểu đồ vùng (`xlArea`), 7 nhóm tuổi cố định |
| So sánh Giáo họ (không phụ thuộc năm thống kê) | `rdGiaoHo` | biểu đồ tròn (≤7 giáo họ) hoặc cột (>7 giáo họ) |
| Tổng hôn phối | `rdHonPhoi` | biểu đồ cột, số đôi hôn phối theo từng năm (không luỹ kế) |
| Từ ngày / Đến ngày | `dtDateFrom` / `dtDateTo` | chỉ dùng cho Tổng giáo dân/Bí tích/Hôn phối — chỉ lấy **năm** (`iDateFrom/10000`) |
| Tính cả trong hồ sơ lưu trữ | `chkLuuTru` | **chỉ hiện/bật được khi chọn "Tổng giáo dân"** (`rdTongGiaoDan_CheckedChanged`, `frmBieuDo.cs:385-393`); constructor khoá `Checked=false; Enabled=false` (`frmBieuDo.cs:18-19`) |
| Tính cả dữ liệu không có ngày tháng | `chkNullAccept` | **`Visible=false` cố định trong Designer, không có nơi nào set lại `true`** — control chết, người dùng KHÔNG BAO GIỜ bật được nó, xem mục 4.5 |
| (nút) Xem | `gxCommand1` (label đổi thành "&Xem", `frmBieuDo.cs:397`) | xuất Excel + mở file |
| (nút) Hủy | `gxCommand1` | đóng form |

## 3. Hành vi khi tải

- `GxThongKeChung`: mặc định hiện lưới giáo dân (`gxGiaoDanList1.Visible=true`), `dtDateFrom.Text`
  = năm hiện tại (`GxThongKeChung.cs:22`), `cbGiaoHo.SelectedValue=-1` ("Tất cả",
  `GxThongKeChung.cs:748`/`frmThongKeChung_Load`), **chưa tự chạy tìm kiếm nào** — lưới trống tới
  khi bấm "Tìm kiếm". `SetCbCondition()` nạp 16 mục điều kiện + 6 mục trạng thái hôn phối
  (`GxThongKeChung.cs:530-561`); chưa chọn gì (`SelectedIndex=-1`) nên `gxComboField1_SelectedIndexChanged`
  chưa chạy lần nào cho tới khi người dùng tự chọn.
- `GxThongKeOnGoi`: tương tự, `dtDateFrom.Text` = năm hiện tại, nạp gợi ý tự động hoàn thành
  (autocomplete) cho Nơi tu/Dòng tu/Nơi phục vụ từ **giá trị DISTINCT đã có trong bảng `TanHien`**
  (`GxThongKeOnGoi.cs:145-171`) — bản web KHÔNG có tương đương (xem mục 8).
- `frmBieuDo`: không tải dữ liệu gì khi mở, chỉ khoá `chkLuuTru` (mục ở trên).

## 4. Quy tắc nghiệp vụ và kiểm tra dữ liệu

### 4.1 16 điều kiện trích xuất (enum `Condition`, `GxThongKeChung.cs:814-832`, đúng thứ tự combo)

| # | Điều kiện (nhãn) | Loại khoảng | Nguồn dữ liệu | Hậu tố `lblTotal` |
|---|---|---|---|---|
| 0 | Sinh ra | ngày (`NgaySinh`) | GiaoDan | " người được sinh ra" |
| 1 | Rửa tội | ngày (`NgayRuaToi`) | GiaoDan | " người được rửa tội" |
| 2 | Xưng tội - rước lễ lần đầu | ngày (`NgayRuocLe`) | GiaoDan | " người được xưng tội rước lễ lần đầu" |
| 3 | Thêm sức | ngày (`NgayThemSuc`) | GiaoDan | " người được thêm sức" |
| 4 | Hôn phối | ngày (`NgayHonPhoi`) | HonPhoi | " đôi chịu phép hôn phối" |
| 5 | Qua đời | ngày (`NgayQuaDoi`) | GiaoDan | " người qua đời" |
| 6 | Tổng số giáo dân | ngày (chỉ dùng `denNgay`, xem 4.2) | GiaoDan | " giáo dân đến thời điểm được nhập" |
| 7 | Tổng số gia đình (không phụ thuộc năm thống kê) | — (không lọc ngày) | GiaDinh | " gia đình" |
| 8 | Tân tòng (thống kê theo ngày rửa tội) | ngày (`NgayRuaToi`, điều kiện `TanTong<>0`) | GiaoDan | " tân tòng được rửa tội" |
| 9 | Chủ hộ | tuổi | GiaoDan (qua `ThanhVienGiaDinh.ChuHo`) | " chủ hộ" |
| 10 | Gia trưởng | tuổi | GiaoDan (`ThanhVienGiaDinh.VaiTro=0`, Chồng) | " gia trưởng" |
| 11 | Hiền mẫu | tuổi | GiaoDan (`ThanhVienGiaDinh.VaiTro=1`, Vợ) | " hiền mẫu" |
| 12 | Cao niên | tuổi (chỉ "từ tuổi", cố định 60) | GiaoDan | " cao niên" |
| 13 | Giới trẻ | tuổi (18-30 cố định) | GiaoDan | " giới trẻ" |
| 14 | Thiếu nhi | tuổi (5-17 cố định) | GiaoDan | " thiếu nhi" |
| 15 | Kỷ niệm hôn phối | ngày, so theo **ngày+tháng** không theo năm | HonPhoi | " đôi chịu phép hôn phối" |

Nguồn: `SetCbCondition` (`GxThongKeChung.cs:530-551`) khớp 1-1 thứ tự `enum Condition`.

### 4.2 Công thức lọc từng điều kiện (trích dòng mã)

Nền chung cho điều kiện 0-3, 5, 6, 8 (`GxThongKeChung.cs:199-283`):
```
where = " AND DaXoa=0 "
where += chkLuuTru.Checked ? "" : " AND DaChuyenXu=0 AND QuaDoi=0 "   // trừ riêng điều kiện 5 (Qua đời), xem dưới
```
- **Sinh ra/Rửa tội/XTRL/Thêm sức** (`GxThongKeChung.cs:203-222`): `WHERE <nền> AND (<cột ngày>
  BETWEEN từNgày VÀ đếnNgày [OR <cột ngày> IS NULL/rỗng nếu chkNullAccept])`.
- **Qua đời** (`GxThongKeChung.cs:223-240`): nền **RIÊNG** — `WHERE QuaDoi<>0` (bỏ hẳn
  `DaChuyenXu=0 AND QuaDoi=0` của nền chung, chỉ giữ `DaXoa=0` khi `!chkLuuTru`), cộng điều kiện
  ngày theo `NgayQuaDoi`.
- **Tổng số giáo dân** (`GxThongKeChung.cs:241-249`): **GHI ĐÈ** `đếnNgày` lên `từNgày` trước khi
  lọc (`dtDateTo.Text = dtDateFrom.Text; iDateFrom = iDateTo;`) — ô "Từ ngày" hiển thị coi như bị
  bỏ qua, chỉ "Đến ngày" có tác dụng. Điều kiện: `NgaySinh <= đếnNgày`
  (không phải BETWEEN) — tức đếm **luỹ kế** mọi giáo dân sinh ra trước hoặc trong thời điểm đó.
- **Tân tòng** (`GxThongKeChung.cs:250-267`): nền `WHERE TanTong<>0` (bỏ `DaChuyenXu=0 AND
  QuaDoi=0` của nền chung), cộng điều kiện theo `NgayRuaToi` (không phải ngày tân tòng riêng).
- **Tổng số gia đình** (`GxThongKeChung.cs:369-388`): KHÔNG lọc theo ngày tháng nào (đúng như tên
  "không phụ thuộc năm thống kê") — chỉ `DaXoa=0` [+ `DaChuyenXu=0` nếu `!chkLuuTru`] [+
  `GiaDinhAo=0` nếu đã chọn một giáo họ cụ thể].
- **Hôn phối** (`GxThongKeChung.cs:286-320`): lọc theo `NgayHonPhoi BETWEEN từNgày VÀ đếnNgày`,
  **bắt buộc** đã chọn "Trạng thái hôn phối" (`checkStatus()`, dòng 421-430 — thông báo "Vui lòng
  chọn 1 trạng thái hôn nhân" nếu `SelectedIndex==-1`), rồi lọc thêm `CachThucHonPhoi = <nhãn>` nếu
  khác "Không phân loại" (`SetWhereForHonPhoi`, dòng 396-420).
- **Kỷ niệm hôn phối** (`GxThongKeChung.cs:321-368`): **không so theo năm** — chỉ so
  `(tháng*100+ngày)` của `NgayHonPhoi` nằm trong khoảng `(tháng,ngày)` của Từ ngày/Đến ngày, bất kể
  năm hôn phối là năm nào (dùng để tìm "các đôi có ngày kỷ niệm rơi vào khoảng này trong năm").
  Cùng yêu cầu chọn trạng thái hôn phối như điều kiện Hôn phối.

**Giáo họ**: với 8/16 điều kiện trên (0,1,2,3,5,6,8, và ngầm cả "Tổng số gia đình"), nếu đã chọn
một giáo họ cụ thể (`cbGiaoHo.MaGiaoHo>-1`) thì cộng thêm `AND (MaGiaoHo=X OR MaGiaoHoCha=X)`
(gồm cả giáo xóm con). **Đặc biệt**: cờ loại "giáo dân ảo"
(`GiaoDanAo=0`/`GiaDinhAo=0`, ứng với `GiaoDan.KhongThongKe`/`GiaDinh.KhongThongKe` ở bản web) **chỉ
được áp dụng khi đã chọn một giáo họ cụ thể HOẶC điều kiện là "Tổng số giáo dân"**
(`GxThongKeChung.cs:272-275`: `if (cbGiaoHo.MaGiaoHo > 0 || con.Equals(Condition.TONGSOGIAODAN))`)
— nghĩa là khi xem "Tất cả giáo họ" với các điều kiện Sinh ra/Rửa tội/XTRL/Thêm sức/Qua đời/Tân
tòng, **giáo dân đánh dấu "không thống kê" (ảo) VẪN được đếm vào kết quả**. Rất có thể là sơ suất
của bản gốc (không nhất quán với chính "Tổng số giáo dân"), nhưng **migrate y hệt** — xem
`can-review-sau.md`.

### 4.3 Chủ hộ / Gia trưởng / Hiền mẫu / Cao niên / Giới trẻ / Thiếu nhi — lớp `Extract` (`Extract.cs`)

```csharp
// Extract.cs:33-37, 46-50 — setter đổi TUỔI thành NĂM SINH
public int FromYear { set { fromYear = DateTime.Now.Year - value; } }
public int ToYear   { set { toYear   = DateTime.Now.Year - value; } }
```
```csharp
// Extract.cs:113-125 — SetWhere() dùng chung cho Chủ hộ/Gia trưởng/Hiền mẫu
where = isLuuTru ? "" : " AND DaXoa=0 AND DaChuyenXu=0 AND QuaDoi=0 ";
condition = string.Format(" AND ((NamSinh BETWEEN '{0}' AND '{1}')", fromYear, toYear);
```
`fromYear` được gán từ **"Từ tuổi"** (tuổi nhỏ hơn) và `toYear` từ **"Đến tuổi"** (tuổi lớn hơn).
Vì năm sinh và tuổi tỉ lệ **nghịch**, `fromYear = Now - tuổiNhỏ` LỚN HƠN `toYear = Now - tuổiLớn`.
`BETWEEN` chuẩn SQL/Access yêu cầu cận dưới ≤ cận trên — ở đây `fromYear > toYear` nên
**`BETWEEN` luôn cho 0 dòng với mọi khoảng tuổi thật (Từ tuổi < Đến tuổi)**, trừ khi Từ tuổi =
Đến tuổi (một tuổi duy nhất, cận dưới = cận trên). `CheckAge()` (dòng 432-455) chỉ chặn *Từ tuổi >
Đến tuổi*, không chặn trường hợp bình đẳng — nên bug này gần như LUÔN xảy ra khi người dùng nhập
một khoảng tuổi thật.

**Đã kiểm chứng bằng dữ liệu thật `qlgx_thu`** (hôm nay 2026, tuổi Cao niên=60 cố định,
Giới trẻ=18-30, Thiếu nhi=5-17 — `Source/DBAccess/GxConstants.cs`:
`TUOI_CAO_NIEN=60; TUOI_TRE="18-30"; TUOI_THIEU_NHI="5-17"`):

| Điều kiện | Công thức năm sinh dùng trong `BETWEEN` | Số dòng thật |
|---|---|---|
| Cao niên (≥60 tuổi) | `SelectByTuoi` dùng cận cứng `(1, Now-60)` = `(1, 1966)` — **không** qua cặp fromYear/toYear thường, không bị bug | **90** |
| Giới trẻ (18-30 tuổi) | `BETWEEN 2008 AND 1996` (2026-18, 2026-30) | **0** (bug) |
| Thiếu nhi (5-17 tuổi) | `BETWEEN 2021 AND 2009` (2026-5, 2026-17) | **0** (bug) |
| Chủ hộ/Gia trưởng/Hiền mẫu với khoảng tuổi bất kỳ (Từ<Đến) | `BETWEEN (Now-Từ) AND (Now-Đến)`, `Now-Từ > Now-Đến` | **0** (bug), trừ khi Từ tuổi = Đến tuổi |

(Lệnh xác nhận: `SELECT count(*) FROM giao_dan WHERE NOT da_xoa AND NOT qua_doi AND
extract(year from ngay_sinh)::int BETWEEN <fromYear> AND <toYear>` cho từng cặp ở trên.)

**Quyết định migrate**: tái hiện ĐÚNG công thức (`fromYear = nay - tuổiTừ`, `toYear = nay -
tuổiĐến`, lọc `NamSinh >= fromYear AND NamSinh <= toYear`) — không tự đảo cận. Kết quả tự nhiên
khớp bảng trên (Cao niên có số liệu thật, ba điều kiện còn lại luôn ra danh sách rỗng). Ghi vào
`can-review-sau.md` vì đây là lỗi ảnh hưởng người dùng thật — quý cha bấm "Giới trẻ"/"Thiếu nhi"
trên bản desktop từ trước tới nay **luôn nhận danh sách trống**.

Ba nhánh phụ trong `Extract`:
- `SelectHeadByAge()` (Chủ hộ): cộng `ThanhVienGiaDinh.ChuHo = -1` (true).
- `SelectByVaiTro(sex)` (Gia trưởng/Hiền mẫu): `sex=true` → `VaiTro=0` (Chồng/Gia trưởng),
  `sex=false` → `VaiTro=1` (Vợ/Hiền mẫu).
- `SelectByTuoi(type)`: `THIEU_NHI`/`GIOI_TRE` dùng cặp fromYear/toYear thường (bug ở trên);
  `GIOI_TRE` cộng thêm `DaCoGiaDinh=0` (chỉ người **chưa lập gia đình**); còn lại (`CAO_NIEN`) dùng
  cận cứng `(1, fromYear)` — không bug.

### 4.4 `chkNullAccept` — "Tính cả dữ liệu không có ngày tháng" (`EnableChk`, `GxThongKeChung.cs:764-784`)

Chỉ **Enabled** (nên chỉ có tác dụng) khi điều kiện ∈ {Qua đời, Sinh ra, Hôn phối, Kỷ niệm hôn
phối, Tổng số giáo dân, Tân tòng, Chủ hộ, Gia trưởng, Hiền mẫu}. Với các điều kiện còn lại (Rửa
tội, XTRL, Thêm sức, Tổng số gia đình, Cao niên, Giới trẻ, Thiếu nhi), control bị `Enabled=false`
**và** `Checked` bị ép về `false` ngay khi đổi điều kiện — nên các điều kiện đó không bao giờ nhận
thêm dòng có ngày rỗng, dù có gọi API với cờ này bật.

### 4.5 `frmBieuDo` — hai công tắc chết một phần

- `chkLuuTru`: chỉ hiện/bật được ở biểu đồ "Tổng giáo dân" (`rdTongGiaoDan_CheckedChanged`,
  `frmBieuDo.cs:385-393`) — 4 biểu đồ còn lại (Bí tích, Độ tuổi, Giáo họ, Hôn phối) **luôn** áp
  dụng như `chkLuuTru=false`, tức luôn loại người đã qua đời (`QuaDoi=0`), kể cả biểu đồ "So sánh
  Giáo họ" — TRỪ chính "So sánh Giáo họ" hoàn toàn không lọc `QuaDoi` (xem 4.6).
- `chkNullAccept`: `Visible=false` cứng trong Designer (`frmBieuDo.Designer.cs:148`), không nơi
  nào trong `frmBieuDo.cs` set lại `true` — **control chết hoàn toàn**, người dùng không thể bật.
  Kết quả: biến `noDateSql` trong `exportTongGiaoDan`/`exportTongHonPhoi` luôn rỗng (luôn loại các
  dòng có ngày rỗng); `exportBiTich` thậm chí COMMENT HẲN đoạn code đọc `chkNullAccept.Checked`
  (`frmBieuDo.cs:159-163`), củng cố thêm rằng tính năng "chấp nhận dữ liệu không ngày" trong màn
  hình Biểu đồ hoàn toàn không truy cập được.

### 4.6 Công thức 5 biểu đồ (`frmBieuDo.cs`)

- **Tổng giáo dân** (`exportTongGiaoDan`, dòng 70-110): với mỗi năm `i` từ năm(Từ ngày) đến
  năm(Đến ngày), đếm **LUỸ KẾ** `COUNT(*) FROM GiaoDan WHERE DaXoa=0 AND GiaoDanAo=0 [AND
  QuaDoi=0 nếu !chkLuuTru] AND NgaySinh <= 31/12/i` — mỗi cột là "tổng giáo dân TÍNH ĐẾN cuối năm
  đó", không phải số sinh trong năm đó (giải thích tại sao đồ thị luôn đi lên hoặc đứng yên, không
  bao giờ xuống — trừ trường hợp `chkLuuTru` đổi trạng thái giữa các lần xem).
- **Tổng hôn phối** (`exportTongHonPhoi`, dòng 112-155): với mỗi năm, đếm số hôn phối có
  `NgayHonPhoi BETWEEN 01/01/i VÀ 31/12/i` — **theo từng năm riêng**, không luỹ kế (khác hẳn biểu
  đồ Tổng giáo dân dù cùng nhóm "theo năm").
- **Bí tích** (`exportBiTich`, dòng 157-236): 4 chuỗi (Sinh ra/Rửa tội/XTRL/Thêm sức), mỗi năm mỗi
  chuỗi là `COUNT(*) FROM GiaoDan WHERE DaXoa=0 AND GiaoDanAo=0 [AND QuaDoi=0] AND <cột ngày
  tương ứng> BETWEEN 01/01/i VÀ 31/12/i` — theo từng năm riêng.
- **Độ tuổi** (`exportDoTuoi`, dòng 238-351): 7 nhóm tuổi **cố định theo năm hiện tại lúc chạy**
  (không phụ thuộc Từ ngày/Đến ngày trên form): `<7`, `7-12`, `13-16`, `17-25`, `26-30`, `31-50`,
  `>50`. Bucket cuối (`>50`) dùng cận **CỐ ĐỊNH** `fromYear=1990` thay vì suy từ tuổi, và
  `toYear=NgayHienTại.Năm - 51` — với năm hiện tại 2026, `toYear=1975 < fromYear=1990` nên
  `BETWEEN 1990 AND 1975` **luôn cho 0 dòng** (đã kiểm chứng: `qlgx_thu` có 2031 giáo dân có ngày
  sinh, nhưng bucket "Trên 50 tuổi" đếm ra đúng 0). Bug này chỉ tự hết khi năm hiện tại ≥ 2041
  (`Now-51 >= 1990`) — tức mọi bản desktop đang chạy trước 2041 đều hiển thị cột cuối bằng 0. Sáu
  bucket đầu (đã kiểm chứng bằng `psql` trên `qlgx_thu`, lọc `DaXoa=0 AND GiaoDanAo=0 AND
  NgaySinh IS NOT NULL AND QuaDoi=0`): `<7`=230, `7-12`=114, `13-16`=148, `17-25`=321, `26-30`=104,
  `31-50`=169, `>50`=**0**.
- **Giáo họ** (`exportGiaoHo`, dòng 353-378): với mỗi giáo họ, đếm `COUNT(*) FROM GiaoDan WHERE
  DaXoa=0 AND GiaoDanAo=0 AND MaGiaoHo=<giáo họ đó>` — **KHÔNG lọc `QuaDoi`** (khác hẳn 4 biểu đồ
  kia) nên người đã qua đời vẫn được tính vào cột giáo họ của họ. Loại biểu đồ: **cột** nếu > 7
  giáo họ, **tròn 3D kèm % nhãn** nếu ≤ 7 giáo họ (`ChartGiaoHo.cs:58-65`).

## 5. Thao tác người dùng

- **Tìm kiếm** (`btnSearch_Click`): chạy công thức mục 4, đổ dữ liệu vào ĐÚNG MỘT trong ba lưới
  rồi set `Visible`/`Dock=Fill` cho lưới đó, ẩn hai lưới còn lại. Bật `btnPrint`/`btnFilter` sau
  khi có kết quả (trước đó `Enabled` mặc định theo Designer — không đọc được giá trị khởi tạo từ
  Designer trong phạm vi soát này, xem mục 9).
- **In** (`btnPrint_Click`, `GxThongKeChung.cs:756-761`): in lưới đang `Visible` VÀ có
  `RowCount>0` — gọi thẳng `Print()` của `GxGiaoDanList`/`GxGiaDinhList`/`GxHonPhoiList` (không
  qua hộp thoại xem trước riêng của màn hình này).
- **Lọc** (`btnFilter_Click`, dòng 785-806): mở `frmFilter` lọc **thêm** trên lưới đã tải (không
  gọi lại CSDL) — cột lọc mặc định là Họ tên/Tên gia đình/Năm tuỳ lưới đang hiện.
- **Đổi điều kiện** (`gxComboField1_SelectedIndexChanged`, dòng 466-521): chuyển control tuổi/ngày
  tương ứng qua `EnableAge()`/`EnableDate()`, hiện/ẩn `gxCbMarried`, tự điền sẵn Từ/Đến tuổi cho
  Cao niên/Giới trẻ/Thiếu nhi, rồi gọi `EnableChk()` (mục 4.4).
- `frmBieuDo`: nút "Xem" (`gxCommand1_OnOK`) validate đã chọn loại biểu đồ + đủ Từ/Đến ngày, rồi
  gọi đúng 1 trong 5 hàm `export...`, ghi ra file Excel tạm và `Process.Start` mở lên. Nút "Hủy"
  đóng form.

## 6. Lưới dữ liệu

- `GxThongKeChung`/`GxThongKeOnGoi` dùng lại NGUYÊN 3 lưới đã có spec riêng
  (`giao-dan-danh-sach.md`, `gia-dinh-danh-sach.md`) — không định nghĩa cột mới, trừ:
  - `GxThongKeOnGoi`: cột giáo dân chuẩn cộng thêm `NgayBatDau`, `ChucVu`, `NoiTu`, `DongTu`,
    `NoiPhucVu` từ `TanHien` (SQL join trong `btnSearch_Click`, dòng 86-96).
  - `GxThongKeChung`/Hôn phối: dùng view `SELECT_HONPHOI_LIST` (dựng runtime qua
    `Memory.CreateSELECT_HONPHOI_VIEW`, cột suy từ code đã COMMENT trong `CMemory.cs`:
    `MaHonPhoi, TenHonPhoi, SoHonPhoi, NoiHonPhoi, NgayHonPhoi, LinhMucChung, NguoiChung1,
    NguoiChung2, CachThucHonPhoi, GhiChu, MaNhanDang, UpdateDate` + cột PIVOT theo Phái cho
    "Nam"/"Nữ" (mã giáo dân từng bên) + `TenGiaoHo`) — bản web KHÔNG có endpoint danh sách hôn
    phối tổng quát nào để dùng lại (khác giáo dân/gia đình), phải tạo DTO/cột mới, xem mục 8.
- `frmBieuDo` không có lưới, chỉ xuất Excel.

## 7. Liên kết sang màn hình khác

- Không mở màn hình chi tiết nào trực tiếp từ đây trong mã đọc được (`gxGiaDinhList1_RowDoubleClick`
  bị để trống, dòng 741-744) — nghĩa là double-click dòng trong "Thống kê chung" **không làm gì**
  trên bản desktop (dù các lưới `Gx*List` thường hỗ trợ double-click mở chi tiết ở nơi khác).
- `frmBieuDo` không liên kết màn hình nào, chỉ mở file Excel ngoài ứng dụng.

## 8. Khác biệt cố ý ở bản web

1. **Vẽ biểu đồ trong trình duyệt, không xuất Excel** — máy chủ Linux không dùng được Office
   Interop (ràng buộc đã chốt của dự án). Chọn **Chart.js** (MIT, không ràng buộc thương mại — đã
   kiểm điều khoản giấy phép trước khi chọn, giống tiền lệ FluentAssertions 7.0.0 ở dự án này) vẽ
   phía trình duyệt bằng `<canvas>`, nhãn tiếng Việt render trực tiếp bằng DOM/canvas text nên
   không có rủi ro thiếu phông (khác PDF nhúng phông like ChartGiaoHo).
2. **Không có "In" riêng cho bảng thống kê ở lượt này** — nút "In" gọi `Print()` của lưới, tương
   đương xuất Excel qua hạ tầng `ClosedXML` đã có; **xuất PDF cho biểu đồ** dùng lại hạ tầng
   `Printing/` (HTML → Chromium headless) nếu người dùng cần gửi giáo phận, để task sau nếu cần —
   lượt này chỉ hiện biểu đồ trên màn hình, ghi vào `can-review-sau.md`.
3. **`GxThongKeOnGoi` không có gợi ý tự động hoàn thành** (autocomplete Nơi tu/Dòng tu/Nơi phục
   vụ từ dữ liệu `DISTINCT` có sẵn) — hạ tầng `GxGoiY` của bản web dựa trên tần suất nhập của
   CHÍNH người dùng (`lib/goiYNhapLieu.ts`), khác cơ chế "liệt kê giá trị đã tồn tại trong CSDL"
   của desktop; để task sau nếu cần, ghi vào `can-review-sau.md`.
4. **Danh sách hôn phối dùng DTO mới** (`HonPhoiThongKeDto`) vì bản web chưa có endpoint "danh
   sách hôn phối" tổng quát nào để dùng lại — cột chọn tối thiểu đủ để đối chiếu số liệu (tên hai
   người, ngày, nơi, cách thức, giáo họ), KHÔNG bắt buộc pivot y hệt view Access.
5. **`chkLuuTru`/`chkNullAccept` cho biểu đồ**: bản web GIỮ đúng chỗ nào các control này có tác
   dụng thật trên desktop (chỉ `chkLuuTru` cho "Tổng giáo dân") — không thêm chúng vào 4 biểu đồ
   còn lại (đúng như desktop, nơi các control này vô hiệu hoặc chết hẳn, mục 4.5).
6. **Các bug cận đảo ngược (mục 4.3, "Trên 50 tuổi" mục 4.6) được MIGRATE Y HỆT** — không tự sửa
   thành `Math.Min`/`Math.Max`. Ghi vào `can-review-sau.md` với đề xuất sửa cho một lượt sau (có
   sự đồng ý của người dùng).

## 9. Chỗ chưa chắc

- `GxThongKeOnGoi` có control `cbGiaoHo` nhưng `btnSearch_Click` không tham chiếu tới nó ở bất kỳ
  đâu trong 195 dòng đọc được — **có thể là control thừa/dự phòng chưa nối dây**, không chắc có
  đúng là "chết" hoàn toàn hay có nơi khác (ví dụ code Designer gán sự kiện) tham chiếu tới. Bản
  web bỏ qua bộ lọc giáo họ trên tab này, đúng như hành vi quan sát được.
  **-> đối chiếu Designer**: đã grep `cbGiaoHo` trong `GxThongKeOnGoi.Designer.cs`, chỉ thấy khai
  báo control + set vị trí/kích thước, không có `SelectedIndexChanged` hay tương tự — xác nhận là
  control không được nối logic, migrate bỏ qua là đúng.
- Trạng thái mặc định của `btnPrint.Enabled`/`btnFilter.Enabled` lúc form vừa mở chưa đọc được từ
  Designer (không thấy dòng gán tường minh trong đoạn đã trích) — giả định `false` cho tới khi có
  kết quả tìm kiếm đầu tiên (khớp cách các màn hình khác đã migrate xử lý nút hành động cần dữ
  liệu). Nếu sai, không ảnh hưởng số liệu, chỉ ảnh hưởng UX bật/tắt nút.
- View Access `SELECT_HONPHOI_LIST` được tạo **runtime bằng code đã bị comment hết** trong
  `CMemory.cs` (không chạy được nữa trong bản hiện hành, chỉ còn code cũ làm tài liệu) — không chắc
  liệu view này còn tồn tại sẵn trong file `.mdb` thật (tạo từ trước, không cần code tạo lại) hay
  đã hỏng; không ảnh hưởng migrate vì bản web tự viết LINQ tương đương trên PostgreSQL, không phụ
  thuộc view Access.
- Ý nghĩa chính xác của "Kỷ niệm hôn phối" khi khoảng Từ ngày/Đến ngày **vắt qua ranh giới năm**
  (ví dụ Từ 20/12 Đến 05/01) chưa kiểm chứng được bằng dữ liệu thật (`HonPhoi` trong `qlgx_thu` có
  522 dòng nhưng chưa thử trường hợp biên này) — công thức đọc từ mã (`GxThongKeChung.cs:330-337`)
  là so trực tiếp chuỗi "tháng+ngày" nên khoảng vắt năm gần như chắc chắn KHÔNG hoạt động đúng
  (cùng họ bug so sánh chuỗi/số bị đảo cận như mục 4.3), nhưng chưa thử nghiệm trên dữ liệu thật để
  khẳng định 100%.

## 10. Đối chiếu bản web hiện tại (sau khi migrate)

Cập nhật sau khi có backend + frontend + kiểm chứng bằng trình duyệt thật — xem báo cáo
`.superpowers/sdd/2026-09-06-qlgx-web-phase-1/task-thong-ke-bieu-do.md`.
