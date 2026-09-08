# Màn hình: Công cụ dữ liệu — Kiểm tra dữ liệu, Chuẩn hoá dữ liệu, Chuyển họ hàng loạt, Tạo danh sách bí tích tự động

| | |
|---|---|
| Tệp nguồn | `Source/ChuongTrinh/frmKiemTraGiaoDanList.cs` (316d) + `ReviewGiaoDanProcess.cs` (257d) + `frmKiemTraGiaDinhList.cs` (346d) + `ReviewGiaDinhProcess.cs` (266d) + `frmChuyenHoGiaDinh.cs` (245d) + `frmChuyenHoGiaoDan.cs` (170d) + `UpdateProcess.cs` (chuyển họ, đã đọc phần liên quan); "Chuẩn hoá dữ liệu" = `frmMain.cs` (`chuanHoaDuLieu`, `ProcessOptions.AutoUpperFirstChar*`) + `UpdateProcess.cs` (chưa đọc hết phần này); "Tạo danh sách bí tích tự động" = `frmTaoDotBiTich.cs` (77d, UTF-8 BOM) + `Source/GXControl/GenerateDotBiTichProcess.cs` (233d) — cả hai **đã xác định được file nguồn, chưa migrate** (xem mục 5) |
| UserControl dùng lại | `GxGiaoDanList`/`GxGiaDinhList` (lưới), `GxGiaoHoComboBox` (`cbGiaoHo`, có "Tất cả") |
| Bảng dữ liệu đụng tới | Kiểm tra dữ liệu: `giao_dan`, `thanh_vien_gia_dinh`, `giao_dan_hon_phoi`, `hon_phoi` (chỉ đọc). Chuyển họ hàng loạt: `giao_dan.giao_ho_id`, `gia_dinh.giao_ho_id` (SỬA — đúng phạm vi desktop, không đụng cột nào khác) |
| Trạng thái migrate | Kiểm tra dữ liệu (cả giáo dân + gia đình) và Chuyển họ hàng loạt (cả giáo dân + gia đình) **đã xong**. "Chuẩn hoá dữ liệu", "Tạo danh sách bí tích tự động" **chưa migrate** (mục 5) |

## 1. Mục đích

Nhóm bốn công cụ "dọn dữ liệu" cho quản trị giáo xứ — không phải màn hình nghiệp vụ ngày
thường. Người dùng là quản trị viên xứ, dùng khi phát hiện dữ liệu bất thường (ví dụ sau khi
nhập liệu hàng loạt từ Access) hoặc định kỳ rà soát sổ sách. Bốn công cụ **sửa dữ liệu hàng
loạt** (trừ Kiểm tra dữ liệu — chỉ đọc), nên đây là nhóm nguy hiểm nhất dự án (xem đầu nhiệm vụ
gốc). Lượt này chỉ làm xong phần đọc (Kiểm tra dữ liệu giáo dân); ba công cụ còn lại — kể cả
nửa kia của Kiểm tra dữ liệu (gia đình) — vẫn ở dạng "đã đọc mã, chưa migrate".

## 2. Kiểm tra dữ liệu — giáo dân (`frmKiemTraGiaoDanList.cs` + `ReviewGiaoDanProcess.cs`)

### 2.1 Bố cục và hành vi khi tải

- Combo "Giáo họ cần kiểm tra" (`cbGiaoHo`) — có mục "Tất cả" (`HasShowAll=true`,
  `frmKiemTraGiaoDanList.cs:128`). Bắt buộc phải chọn một giá trị (kể cả "Tất cả") trước khi
  bấm "Bắt đầu kiểm tra" — nếu `cbGiaoHo.Combo.Text == ""` báo:
  > "Hãy chọn giáo họ cần kiểm tra" (Exclamation, dòng 264).
- 6 ô tick loại lỗi cần dò, **mặc định đều tick sẵn**
  (`frmKiemTraGiaoDanList.Designer.cs:213,225,237,249,261,289`):
  - "Không có dữ liệu ngày tháng" (`chkKhongCoNgayThang`)
  - "Sai quan hệ ngày tháng" (`chkSaiQuanHeNgayThang`)
  - "Không thuộc gia đình nào" (`chkKhongThuocGiaDinh`)
  - "Thuộc nhiều gia đình" (`chkThuocNhieuGiaDinh`)
  - "Rước lễ trước tuổi" (`chkRuocLeTruocTuoi`)
  - "Có nhiều hôn phối" (`chkNhieuHonPhoi`)
- Phải tick ít nhất 1 ô, không thì báo "Hãy chọn ít nhất 1 loại kiểm tra" (dòng 275).
- Nút "Bắt đầu &kiểm tra" (`gxButton1`, dòng 260-294) chạy `ReviewGiaoDanProcess` (nền, có
  hộp thoại tiến trình "Đang kiểm tra dữ liệu giáo dân...").
- Lưới ban đầu **trống** (không tự tải gì) — chỉ có dữ liệu sau khi bấm "Bắt đầu kiểm tra".
  Nếu không tìm thấy lỗi nào: xoá sạch lưới và báo
  > "Không tìm thấy lỗi dữ liệu của giáo dân nào" (Information, dòng 306).

### 2.2 Nguồn dữ liệu và phạm vi (đọc mã `ReviewGiaoDanProcess.reViewData`, dòng 106-150)

- Tập giáo dân được xét: `SELECT_GIAODAN_LIST_CO_GIAOHO ... AND DaXoa=0` (+ `AND MaGiaoHo=<x>`
  nếu chọn giáo họ cụ thể, `-1` = tất cả). **Chú ý: KHÔNG lọc `DaChuyenXu=0`** — khác màn hình
  danh sách giáo dân bình thường (`LoadGiaoDanList()` của cùng file, dòng 143-151, có lọc
  `AND DaChuyenXu=0`). Nghĩa là công cụ kiểm tra xét **cả giáo dân đã chuyển xứ**. Bản web tái
  hiện đúng — KHÔNG lọc theo trạng thái chuyển xứ khi kiểm tra.
- Với mỗi giáo dân, tính `NguyenNhan` (chuỗi lý do, nối bằng `\n`) và `KetQua` (tổng các cờ bit
  `ReviewGiaoDanType` vi phạm, `Source/DBAccess/GxConstants.cs:192-199`:
  `RuocLeTruocTuoi=1, SaiQuanHeNgayThang=2, KhongCoDuLieuNgayThang=4, ThuocNhieuGiaDinh=8,
  KhongThuocGiaDinhNao=16, CoNhieuHonPhoi=32`). Chỉ giáo dân có `KetQua > 0` mới vào lưới kết
  quả.

### 2.3 Sáu quy tắc — trích dẫn mã, nguyên văn lý do

1. **Không có dữ liệu ngày tháng** (`ReviewGiaoDanProcess.cs:156-163`) — cả 5 mốc `NgaySinh`,
   `NgayRuaToi`, `NgayRuocLe`, `NgayThemSuc`, `NgayQuaDoi` đều rỗng. Lý do ghi vào `NguyenNhan`:
   `"- Không có dữ liệu ngày tháng"`.
2. **Sai quan hệ ngày tháng** (dòng 165-173) — gọi
   `frmGiaoDan.isValidDateInputRelations(NgaySinh, NgayRuaToi, NgayRuocLe, NgayThemSuc,
   DBNull.Value, NgayQuaDoi)`. **BUG đã xác nhận** (đã ghi ở `giao-dan-chi-tiet.md` mục 9, cùng
   một hàm): dòng 476-479 của `frmGiaoDan.cs` copy-paste sai, chỉ thêm `ngayRuaToi` vào danh
   sách so sánh 4 lần thay vì `ngayRuaToi/ngayRuocLe/ngayThemSuc` — kết quả **quy tắc CHỈ THẬT
   SỰ so `NgaySinh` với `NgayRuaToi`** (`NgayRuocLe`, `NgayThemSuc`, `NgayQuaDoi` không hề được
   dùng dù được truyền vào, và tham số `ngayChuyenXu` bị gọi bằng `DBNull.Value` cứng nên nhánh
   so `NgayChuyenXu` cũng luôn bị bỏ qua ở lời gọi này). Bản web **tái hiện đúng bug**: chỉ báo
   lỗi khi `NgaySinh` và `NgayRuaToi` đều có giá trị và `NgaySinh > NgayRuaToi` (so sánh
   `GetDateFromString(..., false)` / `(..., true)` — với ngày đầy đủ ngày/tháng/năm thì đơn giản
   là so sánh `DateOnly` trực tiếp). Lý do: `"- Sai quan hệ ngày tháng"`.
   **Đã kiểm chứng số liệu thật** (`qlgx_thu`): 38 giáo dân — khớp con số 38 đã biết trước khi
   viết mã (xem mục 4).
3. **Rước lễ trước tuổi** (dòng 174-181) —
   `Memory.KiemTraTuoiKhongHopLe(NgaySinh, NgayRuocLe, GxConstants.TUOI_RUOCLE)` với
   `TUOI_RUOCLE=7` (`GxConstants.cs:108`). Công thức (`CMemory.cs:1426-1440`) **chỉ trừ NĂM**
   (`NgayRuocLe.Year - NgaySinh.Year < 7`), không phải tuổi đủ ngày/tháng — cùng công thức thô
   đã ghi ở `giao-dan-chi-tiet.md`/`GiaoDanService.TuoiKhongHopLe`. Bản web dùng lại đúng hàm đã
   có (`TuoiKhongHopLe`, `GiaoDanService.cs:213-216`) — không viết công thức tuổi thứ hai.
   Lý do: `"- Rước lễ trước 7 tuổi"`.
4. **Thuộc nhiều gia đình** (dòng 204-227) — trong `ThanhVienGiaDinh` (lọc `MaGiaDinh>-1`), nếu
   giáo dân xuất hiện ở ≥2 gia đình **khác nhau** (đếm số lần đổi `MaGiaDinh` khi duyệt tuần tự
   — cách đếm này có thể đếm sai nếu dữ liệu không sắp theo `MaGiaDinh`, nhưng ở quy mô một
   giáo dân thuộc rất ít gia đình thì tương đương "đếm số `MaGiaDinh` phân biệt"; bản web dùng
   trực tiếp "đếm số `gia_dinh_id` phân biệt > 1" — coi là tương đương, không tái hiện lỗi đếm
   tuần tự này vì nó chỉ là chi tiết cài đặt DataTable, không phải quy tắc nghiệp vụ). Lý do:
   `"- Thuộc nhiều gia đình (Mã GĐ: x; y)"`. Đây chính là công cụ dò ra hệ quả của bug đã ghi ở
   `can-review-sau.md` mục 2 (nhánh "tự xoá khỏi gia đình cũ" bị comment khi thêm giáo dân vào
   gia đình mới).
5. **Không thuộc gia đình nào** (dòng 193-203) — không có bản ghi `ThanhVienGiaDinh` nào cho
   giáo dân này (bất kể `MaGiaDinh`). Lý do: `"- Không thuộc gia đình nào"`.
6. **Có nhiều hôn phối** (dòng 234-255, dùng `SELECT_HONPHOI_CHECK_LIST`,
   `SqlConstants.cs:381-384`) — quy về: giáo dân xuất hiện ở ≥2 `HonPhoi` (`giao_dan_hon_phoi`)
   phân biệt. Lý do: `"- Có nhiều thông tin hôn phối"`.

Nếu một giáo dân vi phạm nhiều quy tắc, `NguyenNhan` nối các lý do lại (không có dấu phân cách
rõ ràng giữa các đoạn ngoài `\n` — đọc mã thấy `string.Concat` nối thẳng, có thể ra chuỗi hơi
dính nếu đoạn trước không kết thúc bằng `\n`; bản web nối các lý do đã chọn cách nhau bằng
xuống dòng, không cố tái hiện chi tiết ghép chuỗi này vì không quan sát được rõ nó có lỗi hay
không từ mã tĩnh — xem mục 9).

### 2.4 Số liệu đối chiếu thật (`qlgx_thu`, giáo xứ Vô Nhiễm, 2050 giáo dân, `da_xoa=false`)

| Quy tắc | Số giáo dân | Câu lệnh kiểm chứng |
|---|---|---|
| Sai quan hệ ngày tháng | **38** | `ngay_sinh is not null and ngay_rua_toi is not null and ngay_sinh > ngay_rua_toi` |
| Không có dữ liệu ngày tháng | **18** | cả 5 cột ngày đều `null` |
| Rước lễ trước 7 tuổi | **8** | `extract(year from ngay_ruoc_le) - extract(year from ngay_sinh) < 7` |
| Thuộc nhiều gia đình | **0** | `group by giao_dan_id having count(distinct gia_dinh_id) > 1` |
| Không thuộc gia đình nào | **1905** | không có dòng nào trong `thanh_vien_gia_dinh` |
| Có nhiều hôn phối | **11** | `group by giao_dan_id having count(distinct hon_phoi_id) > 1` |

"Thuộc nhiều gia đình" ra 0 ở bộ dữ liệu này (khác kỳ vọng ban đầu ở mục can-review-sau.md 2)
— bug đó có thể chưa xảy ra ở đúng giáo dân này, hoặc dữ liệu mẫu chưa tái hiện được; **không
suy luận thêm, chỉ ghi lại số đo được**. "Không thuộc gia đình nào" ra rất cao (93%) vì chỉ có
145 bản ghi `thanh_vien_gia_dinh` trên 2050 giáo dân — phần lớn giáo dân trong dữ liệu khảo sát
đơn giản là không được gắn vào hộ gia đình nào (dữ liệu mẫu chưa đầy đủ, không phải lỗi phần
mềm).

### 2.5 Nút và thao tác khác

- "Tải lại" (`ReloadButton`) — **tắt sẵn** (`Enabled=false`, dòng 70), chỉ bật khi đổi combo
  giáo họ (`Combo_SelectedIndexChanged`, dòng 93-97) — đúng ra vì lưới không tự tải gì lúc mở
  nên không có gì để "tải lại" cho tới khi người dùng đã từng chọn giáo họ.
- "Thêm" (`AddButton`) — **ẩn** (`Visible=false`, dòng 45): không thêm giáo dân mới từ đây.
- "Sửa" (`EditButton`) — hiện, gọi `gxGiaoDanList1.EditRow()` — mở thẳng `frmGiaoDan` của dòng
  đang chọn để sửa lỗi ngay tại chỗ.
- "Xóa" (`DeleteButton`) — hiện, xoá **cứng** (gọi `SqlConstants.DELETE_GIAODAN` trực tiếp,
  không qua xoá mềm) sau khi hỏi "Bạn có chắc muốn xóa giáo dân này?" (dòng 194).
- "In danh sách" — xuất lưới đang hiện ra Excel qua `GridEXExporter` rồi mở file (dòng 246-258).
- "Tìm kiếm trên lưới" — tìm theo cột hiện có, không có gì đặc biệt.
- Nút OK ẩn (`gxCommand1.OKButton.Visible=false`), chỉ có "Đóng".

### 2.6 Lưới kết quả

Cùng bộ cột với `GxGiaoDanList` (29 cột, xem `giao-dan-danh-sach.md`) **cộng 2 cột được thêm
động** lúc kiểm tra: `NguyenNhan` (lý do, kiểu chuỗi) và `KetQua` (tổng cờ bit, kiểu số — không
thấy `GxGiaoDanList` cấu hình ẩn cột này trong mã đã đọc, xem mục 9 — có thể vẫn hiện ra như một
cột số khó hiểu với người dùng cuối trên bản desktop).

## 3. Kiểm tra dữ liệu — gia đình (`frmKiemTraGiaDinhList.cs` + `ReviewGiaDinhProcess.cs`)

### 3.1 Bố cục và hành vi khi tải

Cùng khuôn với bên giáo dân (`frmKiemTraGiaDinhList.cs` kế thừa `frmGiaDinhList`, dùng lại
`GxGiaDinhLoiList`/`GxGiaoHoComboBox`): combo "Giáo họ" bắt buộc chọn (kể cả "Tất cả") trước khi
bấm "Bắt đầu kiểm tra" (`btnKiemTra_Click` dòng 290-297, cùng thông báo "Hãy chọn giáo họ cần
kiểm tra"). 4 ô tick, **mặc định đều tick** (`ReviewGiaDinhProcess` có 4 property
`kiemTraKhongNgayHP/kiemTraHonPhoiTruocTuoi/kiemTraKhoangCachTuoiConCai/cacVanDeKhac` khởi tạo
`= true`, dòng 43-73):

- "Không có ngày hôn phối" (`chkKhongCoNgayHP`)
- "Ngày hôn phối không hợp lệ" (`chkNgayHPKoHopLe`)
- "Khoảng cách tuổi giữa con cái và cha mẹ không hợp lệ (nhỏ hơn `KHOANGCACH_TUOI_CHAME_CONCAI`
  tuổi)" (`chkSaiTuoiConCaiChaMe`, nhãn dựng động ở `frmKiemTraGiaDinhList.cs:52` —
  `KHOANGCACH_TUOI_CHAME_CONCAI=16`, `GxConstants.cs:108`, xem 3.4 vì hằng số này KHÔNG được
  dùng trong quy tắc thật)
- "Các vấn đề khác" (`chkCacVanDeKhac` — chỉ có 1 quy tắc con: "nhiều vợ/chồng")

Phải tick ít nhất 1 ô, không thì báo "Hãy chọn ít nhất 1 loại kiểm tra" (dòng 303). Lưới ban đầu
trống, chỉ có dữ liệu sau khi bấm "Bắt đầu kiểm tra"; không tìm thấy lỗi thì xoá sạch lưới và báo
"Không tìm thấy lỗi dữ liệu của gia đình nào" (dòng 338).

### 3.2 Nguồn dữ liệu và phạm vi (`ReviewGiaDinhProcess.reViewData`, dòng 89-142)

- Tập gia đình được xét: `SELECT_GIADINH_LIST_CO_HONPHOI` (JOIN `ThanhVienGiaDinh(VaiTro 0/1)` →
  `GiaoDanHonPhoi` → `HonPhoi`, `SqlConstants.cs:237-241`) `AND DaXoa=0` + `AND MaGiaoHo=<x>` nếu
  chọn giáo họ cụ thể (khớp chính xác, không gồm giáo xóm con).
- Với mỗi gia đình, `coNgayThangLoi` tính 3 quy tắc đầu (dòng 144-232), `nhieuVoChong` tính quy
  tắc thứ 4 riêng — chỉ chạy MỘT LẦN mỗi `MaGiaDinh` (dòng 120-132, có `lstMaGiaDinh` chống chạy
  lặp do join có thể tạo nhiều dòng cho cùng một gia đình).
- **BUG THẬT — ghi đè `NguyenNhan`** (`nhieuVoChong` dòng 259): nếu quy tắc 4 khớp, dòng
  `row[NGUYEN_NHAN] = str.ToString()` GHI ĐÈ TOÀN BỘ giá trị `NguyenNhan` mà `coNgayThangLoi` đã
  ghi trước đó cho gia đình này (nếu có) — lý do của 3 quy tắc đầu bị XOÁ khỏi chuỗi hiển thị dù
  `KetQua` (cờ bit) vẫn cộng dồn đủ cả 2 phía. Bản web **tái hiện đúng bug này** (xem 3.5) —
  không tự sửa thành nối chuỗi.

### 3.3 Bốn quy tắc — trích dẫn mã, nguyên văn lý do

`ReviewGiaDinhType` (`GxConstants.cs:202-207`): `KhongCoNgayHonPhoi=1, HonPhoiTruocTuoi=2,
KhoangCachTuoiKhongHopLe=4, NhieuVoChong=8`.

1. **Không có ngày hôn phối** (dòng 151-158) — `HonPhoi.NgayHonPhoi` rỗng (đúng cho cả trường
   hợp gia đình không có bản ghi hôn phối nào gắn qua chồng/vợ — LEFT JOIN ra `NULL`, và trường
   hợp có bản ghi nhưng cột ngày rỗng). Lý do: `"- Không có ngày hôn phối"`.
2. **Hôn phối trước tuổi** (dòng 160-184) — với mỗi thành viên `VaiTro<=1` (chồng/vợ) của gia
   đình, so `NgaySinh` với `NgayHonPhoi` (của bản ghi hôn phối gắn với gia đình qua BẤT KỲ
   chồng/vợ nào) bằng `Memory.KiemTraTuoiKhongHopLe` (chỉ trừ năm), ngưỡng theo giới của CHÍNH
   người đó: Nam `TUOI_HON_PHOI_NAM=20`, Nữ `TUOI_HON_PHOI_NU=18` (`GxConstants.cs:110-112`).
   Dừng ở người đầu tiên vi phạm (`break`). Lý do: `"- Người {nam/nữ} hôn phối trước {N} tuổi"`.
3. **Khoảng cách tuổi cha/mẹ — con cái không hợp lệ** (dòng 186-221) — so `NgaySinh` của từng
   chồng/vợ với từng con (`VaiTro=2`), CÙNG công thức + CÙNG ngưỡng theo giới của **người
   cha/mẹ** (không phải của con) ở quy tắc 2 (20 nam / 18 nữ). Lý do:
   `"- Khoảng cách tuổi giữa {người cha/người mẹ} và con cái không hợp lý (nhỏ hơn {N} tuổi)"`.
4. **Nhiều vợ/chồng** (dòng 234-264, `nhieuVoChong`) — gia đình có ≥2 thành viên `VaiTro=0`
   (nhiều chồng) hoặc ≥2 thành viên `VaiTro=1` (nhiều vợ). Lý do kèm gợi ý sửa:
   `"- Gia đình có nhiều chồng. (do lỗi phiên bản trước. Hãy mở gia đình này lên, xem lại thông
   tin và bấm nút cập nhật để sửa lỗi)"` (và/hoặc câu tương tự cho "nhiều vợ"). **Ở schema
   PostgreSQL mới quy tắc này KHÔNG THỂ xảy ra**: ràng buộc
   `ux_thanh_vien_gia_dinh_mot_chong_mot_vo UNIQUE (gia_dinh_id, vai_tro) WHERE vai_tro IN (0,1)`
   chặn cứng ở tầng CSDL — luôn trả về 0 trên dữ liệu mới, chỉ còn ý nghĩa với dữ liệu import từ
   Access cũ chưa qua kiểm tra ràng buộc.

### 3.4 `KHOANGCACH_TUOI_CHAME_CONCAI` — xác nhận là nhãn lỗi thời, không phải bug tính toán

Đã đọc `GxConstants.cs:108`: `KHOANGCACH_TUOI_CHAME_CONCAI = 16`. Hằng số này **CHỈ dùng để dựng
nhãn ô tick** (`frmKiemTraGiaDinhList.cs:52`) — `ReviewGiaDinhProcess.cs` (mã THẬT SỰ chạy khi
bấm "Bắt đầu kiểm tra") **không hề tham chiếu** tới nó, dùng đúng `TUOI_HON_PHOI_NAM`/`_NU`
(20/18) đã ghi ở mục 3.3 quy tắc 3. Kết luận: nhãn ô tick nói "16 tuổi" nhưng số so sánh thật là
20 (nam)/18 (nữ) — **nhãn lỗi thời, không phải một bug tính toán mới cần điều tra thêm**. Bản
web migrate đúng mã thật (20/18), không đổi theo nhãn.

### 3.5 Số liệu đối chiếu thật (`qlgx_thu`, 40 gia đình, `da_xoa=false`, 2026-09-08)

| Quy tắc | Số gia đình | Câu lệnh kiểm chứng (rút gọn) |
|---|---|---|
| Không có ngày hôn phối | **10** | không có hôn phối nào (qua chồng/vợ) có `ngay_hon_phoi` khác null |
| Hôn phối trước tuổi (20 nam/18 nữ) | **5** | so `ngay_hon_phoi` với `ngay_sinh` chồng/vợ |
| Khoảng cách tuổi cha/mẹ – con | **3** | so `ngay_sinh` cha/mẹ với `ngay_sinh` con |
| Nhiều vợ/chồng | **0** | luôn 0 do ràng buộc UNIQUE PostgreSQL |
| **Hợp nhất cả 4 (chạy thật trên web, tick cả 4 ô, "Tất cả" giáo họ)** | **16** | `UNION` 3 tập đầu (đã kiểm bằng `psql`) |

Chạy thật trên trình duyệt: web hiện đúng **16 gia đình có lỗi**, khớp tuyệt đối. Ảnh
`176-kiem-tra-du-lieu-gia-dinh.png` ở `WebApp/anh-chup-kiem-thu/`.

## 4. Chuyển họ hàng loạt (`frmChuyenHoGiaoDan.cs` 170d + `frmChuyenHoGiaDinh.cs` 245d + `UpdateProcess.cs`)

**CÔNG CỤ SỬA DỮ LIỆU HÀNG LOẠT NGUY HIỂM NHẤT NHÓM** (xem đầu nhiệm vụ gốc và
`can-review-sau.md` mục 60) — bản web CỐ Ý làm khác desktop ở 3 điểm an toàn, xem 4.4.

### 4.1 `frmChuyenHoGiaoDan.cs` — chuyển giáo dân

Kế thừa `frmGiaoDanList` (dùng lại lưới + combo giáo họ đã có), thêm: combo "Giáo họ nguồn"
(chính `cbGiaoHo`, đổi nhãn), combo "Giáo họ đích" (`cbGiaoHoDich`, không có "Tất cả"), cột
checkbox "Chọn" chèn động vào lưới (`FormatGrid`, dòng 57-75), nút "Chọn / bỏ chọn tất cả"
(`btnChonGiaoDan_Click`), nút "Bắt đầu chuyển" (`btnBatDauChuyen_Click`, dòng 108-169):

- Bắt buộc chọn cả 2 combo (dòng 112-116: `"Xin vui lòng chọn đầy đủ giáo họ nguồn vào giáo họ
  đích"`), giáo họ đích ≠ giáo họ nguồn (dòng 118-122: `"Xin vui lòng chọn giáo họ đích khác
  giáo họ nguồn"`), lưới không rỗng (dòng 124-128: `"Không có dữ liệu làm việc"`), ít nhất 1 dòng
  được tick (dòng 136-140: `"Xin vui lòng chọn ít nhất 1 giáo dân để chuyển họ"`).
- Ghi: với mỗi dòng đã tick, gán `row[MaGiaoHo] = cbGiaoHoDich.SelectedValue` rồi
  `Memory.UpdateDataSet` MỘT LẦN cho cả `DataTable` (dòng 141-163) — **CHỈ đổi cột `MaGiaoHo`**,
  không đụng cột nào khác. Xong báo `"Chuyển họ thành công"` rồi tải lại lưới.

### 4.2 `frmChuyenHoGiaDinh.cs` + `UpdateProcess.chuyenHoGiaDinh` — chuyển gia đình

Cùng khuôn UI (`frmGiaDinhList`, cột "Chọn", combo nguồn/đích), nhưng thao tác ghi khác hẳn —
chuyển gia đình **kéo theo TẤT CẢ thành viên**:

- Cùng 4 điều kiện chặn như bên giáo dân, cộng hộp thoại xác nhận riêng (dòng 143-147):
  `"Nếu chuyển họ cho các gia đình được chọn, các thành viên trong các gia đình này cũng sẽ bị
  chuyển theo.\r\nBạn có chắc muốn thực hiện việc chuyển họ không?"`.
- Ghi qua `UpdateProcess.ChuyenHoGiaDinh` (`UpdateProcess.cs:197-272`):
  `chuyenHoGiaDinh(tbl, maGiaoHo)` — với mỗi gia đình đã tick: gán
  `row[GiaDinhConst.MaGiaoHo] = maGiaoHo`, rồi gọi `chuyenHoThanhVienGiaDinh(maGiaDinh, maGiaoHo)`
  (dòng 273-300) — truy vấn TOÀN BỘ `ThanhVienGiaDinh` của gia đình đó (**mọi `VaiTro`, không
  chỉ chồng/vợ**) và gán `row[GiaoDanConst.MaGiaoHo] = maGiaoHo` cho từng người, gộp vào một
  `DataTable` chung rồi `Memory.UpdateDataSet` MỘT LẦN cho cả 2 bảng (`GiaDinh` + `GiaoDan`,
  dòng 243-262). **CHỈ đổi đúng cột `MaGiaoHo` của 2 bảng đó**, không đụng cột nào khác.

### 4.3 Bản web đã làm

- Backend: `WebApp/src/Qlgx.Api/Services/ChuyenHoService.cs`,
  `WebApp/src/Qlgx.Api/Endpoints/ChuyenHoEndpoints.cs`,
  `WebApp/src/Qlgx.Api/Dtos/ChuyenHoDtos.cs`. Bốn endpoint:
  `POST /api/cong-cu-du-lieu/chuyen-ho/giao-dan/xem-truoc` (body `{giaoDanIds, giaoHoDichId}`,
  trả `{soLuongGiaoDan, tenGiaoHoDich}`), `POST .../giao-dan` (ghi thật, trả
  `{soLuongDaChuyen}`), và cặp tương tự `.../gia-dinh/xem-truoc` /
  `.../gia-dinh` (trả thêm `soLuongThanhVien(DaChuyen)`).
- Frontend: `WebApp/src/web/src/screens/ChuyenHoPage.tsx` (container, 2 tab) +
  `ChuyenHoGiaoDan.tsx` + `ChuyenHoGiaDinh.tsx` — bảng chọn (checkbox thủ công, KHÔNG dùng
  `GxGrid`/ag-grid vì cần chọn nhiều dòng mà `GxGrid` hiện chỉ hỗ trợ `rowSelection="single"`,
  xem `can-review-sau.md` mục 60), combo nguồn/đích, "Chọn tất cả"/"Bỏ chọn tất cả", nút "Xem
  trước & chuyển họ" → hộp xác nhận có con số cụ thể → "Xác nhận chuyển"/"Huỷ".
- Gộp 2 mục menu desktop (`itChuyenHoGiaoDan`/`itChuyenHoGiaDinh`) vào **một** mục điều hướng web
  (`chuyenHo`) có 2 tab con — quyết định tự đưa ra, xem `can-review-sau.md` mục 60.

### 4.4 Ba điểm CỐ Ý khác desktop (yêu cầu an toàn của nhiệm vụ, không phải "tự sửa" âm thầm)

1. **Có bước "Xem trước" tách riêng** gọi máy chủ lấy số liệu THẬT ngay trước khi ghi — desktop
   không có bước này.
2. **Hộp thoại xác nhận nêu con số cụ thể** lấy từ bước xem trước (ví dụ "Sẽ chuyển 342 giáo dân
   sang giáo họ X"), không tự bịa số ở phía trình duyệt.
3. **Toàn bộ thao tác ghi bọc trong MỘT `BeginTransactionAsync`** — desktop dùng
   `Memory.UpdateDataSet` (không có transaction rõ ràng, có thể dở dang nếu lỗi giữa chừng).

### 4.5 Chứng minh bằng chạy thật, đối chiếu `psql` (2026-09-08)

Tạo giáo họ tạm `TEST Chuyen ho (tam)` bằng UI để có đích khác nguồn (chỉ có 1 giáo họ thật
trong `qlgx_thu`), xoá ngay sau khi kiểm chứng xong:

| Bước | Kết quả web | `psql` |
|---|---|---|
| Xem trước chuyển 1 giáo dân (Nguyễn Đức Mạnh, mã 1) sang giáo họ tạm | "Sẽ chuyển 1 giáo dân sang giáo họ TEST Chuyen ho (tam)" | chưa đổi (kiểm trước khi xác nhận) |
| Xác nhận chuyển | thành công | `giao_ho_id` đổi đúng sang giáo họ tạm |
| Chuyển ngược lại về "Simon Phan Đắc Hòa" | thành công | `giao_ho_id` về đúng nguyên trạng |
| Xem trước chuyển 1 gia đình (Anna Nguyễn Thị Lan, mã 1, 2 thành viên) | "Sẽ chuyển 1 gia đình (2 thành viên) sang giáo họ TEST Chuyen ho (tam)" | chưa đổi |
| Xác nhận chuyển | thành công | CẢ gia đình LẪN 2 giáo dân thành viên đổi `giao_ho_id` |
| Chuyển ngược lại | thành công | cả 3 bản ghi về đúng giáo họ cũ |
| Xoá giáo họ tạm | — | không còn ai tham chiếu |
| Số liệu tổng sau khi xong | — | **2050 giáo dân / 40 gia đình / 145 thành viên / 1 giáo họ / 1108 đợt bí tích / 6150 bí tích chi tiết** — khớp nguyên trạng ban đầu |

Ảnh `177-chuyen-ho-*.png` (8 ảnh) ở `WebApp/anh-chup-kiem-thu/`.

## 5. Hai mục còn lại — CÓ THẬT trong desktop, CHƯA MIGRATE

Xác minh dứt điểm bằng cách rà `frmMain.Designer.cs` (26 `explorerBarItem`) đối chiếu
`frmMain.cs.LoadFunction` — chi tiết đầy đủ và bảng đối chiếu menu ở `can-review-sau.md` mục 60.

### 5.1 "Chuẩn hoá dữ liệu" = HAI mục desktop (`itChuanHoaDuLieuGiaoDan`/`itChuanHoaDuLieuGiaDinh`)

`frmMain.cs:365-370` gọi `chuanHoaDuLieu(ProcessOptions.AutoUpperFirstCharGiaoDan/GiaDinh)`
(`frmMain.cs:423-458`) — công cụ SỬA DỮ LIỆU HÀNG LOẠT: viết hoa chữ cái đầu mỗi từ, ký tự khác
chuyển thường, áp dụng "tất cả các dữ liệu được nhập" của giáo dân/gia đình, TRỪ ghi chú (nguyên
văn hộp thoại xác nhận `frmMain.cs:430-431`/`441-442`), chạy qua
`UpdateProcess.ProcessOptions.AutoUpperFirstCharGiaoDan/GiaDinh` (`UpdateProcess.cs:44-49`).
**Chưa đọc hết thân xử lý** để biết chính xác cột nào bị đổi — cần một lượt đọc riêng trước khi
viết spec đủ để migrate. **CHƯA MIGRATE lượt này.**

### 5.2 "Tạo danh sách bí tích tự động" = `itLapBiTichTuDong` → `frmTaoDotBiTich.cs` (77d) + `GenerateDotBiTichProcess.cs` (233d, `Source/GXControl/`)

Công cụ TỰ ĐỘNG gộp giáo dân vào "đợt bí tích" theo khoảng ngày — khác hẳn quy trình thủ công đã
migrate ở `so-bi-tich.md` (người dùng tự tạo đợt rồi tự thêm từng người):

- Form nhận: Loại bí tích (bỏ mục thứ 4 trong combo, `frmTaoDotBiTich.cs:26`), Linh mục, Nơi bí
  tích, khoảng Từ ngày–Đến ngày.
- `GenerateDotBiTichProcess.reViewData` (dòng 99-193): với loại bí tích đã chọn, xác định cột
  ngày/linh mục/nơi tương ứng (`NgayRuaToi/ChaRuaToi/NoiRuaToi` hoặc `NgayRuocLe/...` hoặc
  `NgayThemSuc/...`), quét MỌI giáo dân có ngày đó rơi vào khoảng Từ-Đến (và khớp Nơi/Linh mục
  nếu form có điền), với mỗi giáo dân: tìm (hoặc TẠO MỚI nếu chưa có) một `DotBiTich` khớp CHÍNH
  XÁC (cùng Linh mục + Loại bí tích + Ngày, `GetDotBiTich` dòng 196-231), rồi thêm giáo dân đó
  vào `BiTichChiTiet` của đợt (nếu chưa có). Kết quả trả về: tổng số đợt bí tích tạo mới + tổng
  số giáo dân được thêm vào sổ.
- Đây cũng là công cụ SỬA DỮ LIỆU HÀNG LOẠT (tạo mới đợt bí tích + thêm hàng loạt chi tiết bí
  tích) — khi migrate phải áp dụng đúng 4 nguyên tắc an toàn như "Chuyển họ hàng loạt" (xem mục
  4.4). **CHƯA MIGRATE lượt này** — hết thời gian cho lượt làm việc.

### 5.3 Quyết định giữ nguyên placeholder trên `SideNav.tsx`

Cả hai mục vẫn để dạng placeholder (chưa nối `id`, bấm vào chưa làm gì) — KHÔNG gỡ bỏ, vì nhiệm
vụ gốc chỉ yêu cầu gỡ nếu mục đó KHÔNG có thật; cả hai đều có thật trong bản desktop, chỉ chưa
kịp migrate. Đã thêm chú thích trong `SideNav.tsx` trỏ rõ nguồn desktop.

## 6. Chỗ chưa chắc

- Cách ghép chuỗi `NguyenNhan` khi giáo dân/gia đình vi phạm ≥2 quy tắc (`string.Concat` không
  đảm bảo luôn có `\n` giữa hai đoạn) — chưa quan sát được kết quả thật trên UI desktop để xác
  nhận có bị dính chữ hay không (khác bug ghi đè đã XÁC NHẬN của `nhieuVoChong`, mục 3.2 — đây
  là nghi vấn khác, nhẹ hơn, cho phần còn lại `coNgayThangLoi`).
- Cột `KetQua` (số nguyên cờ bit) có bị `GxGiaoDanList`/`GxGiaDinhList` ẩn khỏi lưới hiển thị hay
  không — không thấy cấu hình ẩn cột động theo tên trong các file đã đọc.
- Cách gộp hôn phối theo gia đình ở bản web (existential trên tập hợp tất cả `NgayHonPhoi` tìm
  được qua bất kỳ chồng/vợ nào) có thể khác desktop (lặp từng dòng JOIN riêng lẻ) khi một gia
  đình có ≥2 hôn phối khác nhau gắn qua các người khác nhau — trường hợp hiếm, không quan sát
  được ở `qlgx_thu`, xem `can-review-sau.md` mục 60.
- "Chuẩn hoá dữ liệu" — đã xác định đúng file nguồn (`UpdateProcess.cs`,
  `ProcessOptions.AutoUpperFirstCharGiaoDan/GiaDinh`) nhưng CHƯA đọc hết thân xử lý (danh sách
  chính xác cột nào bị chuẩn hoá) — cần một lượt đọc riêng trước khi migrate.
- "Tạo danh sách bí tích tự động" — đã đọc đủ sâu để viết spec (mục 5.2) nhưng CHƯA migrate.
- Hai mục "THIẾU thật sự" phát hiện khi đối chiếu menu (không thuộc nhóm Công cụ dữ liệu, ghi
  lại nhân tiện): **"Giáo xứ"** (sửa thông tin giáo xứ hiện tại, `frmGiaoXu`) và **"Tìm và thay
  thế"** (`frmReplace`, chưa đọc mã) — xem bảng đối chiếu đầy đủ ở `can-review-sau.md` mục 60.

## 7. Bản web đã làm

- **Kiểm tra dữ liệu — giáo dân** (lượt trước): `KiemTraDuLieuService.KiemTraGiaoDan`,
  `GET /api/cong-cu-du-lieu/kiem-tra-giao-dan`, `KiemTraDuLieuGiaoDan.tsx` — xem mục 2.
- **Kiểm tra dữ liệu — gia đình** (lượt này): `KiemTraDuLieuService.KiemTraGiaDinh`,
  `GET /api/cong-cu-du-lieu/kiem-tra-gia-dinh?giaoHoId=&khongCoNgayHonPhoi=&honPhoiTruocTuoi=&khoangCachTuoiConCai=&cacVanDeKhac=`
  (mỗi cờ mặc định `true`), `KiemTraDuLieuGiaDinh.tsx` — xem mục 3.5. Cùng giới hạn phạm vi với
  bên giáo dân: KHÔNG migrate Sửa/Xoá tại chỗ hay "In danh sách" (Excel) — "Xem chi tiết" điều
  hướng sang thẻ chi tiết gia đình đã có.
- **Chuyển họ hàng loạt** (lượt này): xem mục 4.

## 8. Chưa migrate lượt này

- Chuẩn hoá dữ liệu (mục 5.1)
- Tạo danh sách bí tích tự động (mục 5.2)
