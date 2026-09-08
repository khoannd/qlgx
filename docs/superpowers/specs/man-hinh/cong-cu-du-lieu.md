# Màn hình: Công cụ dữ liệu — Kiểm tra dữ liệu, Chuẩn hoá dữ liệu, Chuyển họ hàng loạt, Tạo danh sách bí tích tự động

| | |
|---|---|
| Tệp nguồn | `Source/ChuongTrinh/frmKiemTraGiaoDanList.cs` (316d) + `ReviewGiaoDanProcess.cs` (257d) + `frmKiemTraGiaDinhList.cs` (346d) + `ReviewGiaDinhProcess.cs` (266d); `frmChuyenHoGiaDinh.cs` (245d), `frmChuyenHoGiaoDan.cs` (170d) đã đọc nhưng **chưa migrate lượt này**; "Chuẩn hoá dữ liệu" và "Tạo danh sách bí tích tự động" **chưa xác định được file nguồn** trong lượt này (xem mục 9) |
| UserControl dùng lại | `GxGiaoDanList`/`GxGiaDinhList` (lưới), `GxGiaoHoComboBox` (`cbGiaoHo`, có "Tất cả") |
| Bảng dữ liệu đụng tới | `giao_dan`, `thanh_vien_gia_dinh`, `giao_dan_hon_phoi`, `hon_phoi` (chỉ đọc) |
| Trạng thái migrate | **một phần**: chỉ "Kiểm tra dữ liệu giáo dân" xong (web: mục Công cụ dữ liệu → Kiểm tra dữ liệu). "Kiểm tra dữ liệu gia đình", "Chuẩn hoá dữ liệu", "Chuyển họ hàng loạt", "Tạo danh sách bí tích tự động" **chưa migrate** |

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

## 3. Kiểm tra dữ liệu — gia đình (`frmKiemTraGiaDinhList.cs` + `ReviewGiaDinhProcess.cs`) — CHƯA MIGRATE

Đã đọc mã, ghi lại để làm ở lượt sau, chưa migrate lượt này.

- 4 ô tick, mặc định đều tick: "Không có ngày hôn phối" (`chkKhongCoNgayHP`), "Ngày hôn phối
  không hợp lệ" (`chkNgayHPKoHopLe`), "Khoảng cách tuổi giữa con cái và cha mẹ không hợp lệ (nhỏ
  hơn `KHOANGCACH_TUOI_CHAME_CONCAI` tuổi)" (`chkSaiTuoiConCaiChaMe`, nhãn dựng động ở dòng 52
  — **hằng số `KHOANGCACH_TUOI_CHAME_CONCAI` chưa đọc được giá trị**, xem mục 9), "Các vấn đề
  khác" (`chkCacVanDeKhac` — chỉ có 1 quy tắc con: "nhiều vợ/chồng").
- Cùng ràng buộc bắt chọn giáo họ + ít nhất 1 loại kiểm tra như bên giáo dân
  (`btnKiemTra_Click`, dòng 290-321).
- 4 quy tắc (`ReviewGiaDinhProcess.cs`, enum `ReviewGiaDinhType`, `GxConstants.cs:202-207`:
  `KhongCoNgayHonPhoi=1, HonPhoiTruocTuoi=2, KhoangCachTuoiKhongHopLe=4, NhieuVoChong=8`):
  1. **Không có ngày hôn phối** — `HonPhoi.NgayHonPhoi` rỗng (dòng 151-158).
  2. **Hôn phối trước tuổi** (dòng 160-184) — với mỗi thành viên `VaiTro<=1` (chồng/vợ) của gia
     đình, so `NgaySinh` với `NgayHonPhoi` bằng cùng công thức `KiemTraTuoiKhongHopLe` (chỉ trừ
     năm), ngưỡng khác nhau theo giới: Nam `TUOI_HON_PHOI_NAM=20`, Nữ `TUOI_HON_PHOI_NU=18`
     (`GxConstants.cs:110-112`; hằng số dự phòng `TUOI_HON_PHOI=16` chỉ dùng khi không xác định
     được giới tính người đó, gần như không xảy ra vì `Phai` luôn có giá trị Nam/Nữ).
  3. **Khoảng cách tuổi cha/mẹ — con cái không hợp lệ** (dòng 186-221) — so `NgaySinh` của từng
     cặp (chồng/vợ, `VaiTro<=1`) với từng con (`VaiTro=2`), cùng công thức + cùng ngưỡng theo
     giới của **người cha/mẹ** (không phải của con) ở mục 2 (20 nam / 18 nữ) — đây có vẻ là
     **tái sử dụng nhầm** ngưỡng "tuổi kết hôn tối thiểu" cho ý nghĩa hoàn toàn khác ("khoảng
     cách tuổi cha mẹ – con hợp lý"), nhưng nhãn ô tick trên form lại nói tới hằng số thứ ba
     `KHOANGCACH_TUOI_CHAME_CONCAI` không hề xuất hiện trong `ReviewGiaDinhProcess.cs` — **nghi
     ngờ đây là bug/nhãn không khớp mã thật, cần xác nhận thêm giá trị hằng số trước khi migrate,
     xem mục 9**. Không tự ý "sửa cho đúng" — migrate y hệt code thật (dùng `TUOI_HON_PHOI_NAM`/
     `_NU`) khi tới lượt làm, đồng thời ghi rõ nhãn ô tick nói một đằng, mã chạy một nẻo.
  4. **Nhiều vợ/chồng** (dòng 234-264) — gia đình có ≥2 thành viên `VaiTro=0` (nhiều chồng) hoặc
     ≥2 thành viên `VaiTro=1` (nhiều vợ). Lý do kèm gợi ý sửa:
     `"Gia đình có nhiều chồng. (do lỗi phiên bản trước. Hãy mở gia đình này lên, xem lại thông
     tin và bấm nút cập nhật để sửa lỗi)"`. **Ở schema PostgreSQL mới, quy tắc này KHÔNG THỂ
     xảy ra nữa**: ràng buộc duy nhất
     `ux_thanh_vien_gia_dinh_mot_chong_mot_vo UNIQUE (gia_dinh_id, vai_tro) WHERE vai_tro IN
     (0,1)` chặn cứng ở tầng CSDL — quy tắc này khi migrate sẽ luôn trả về 0 kết quả trên dữ
     liệu mới, chỉ còn ý nghĩa với dữ liệu import từ Access cũ chưa qua kiểm tra ràng buộc.

## 4. Chuyển họ hàng loạt (`frmChuyenHoGiaoDan.cs`, 170d) — CHƯA MIGRATE

Đã đọc mã sơ bộ. Màn hình chuyển **một giáo dân** đơn lẻ sang một gia đình khác (không phải
"hàng loạt" theo đúng nghĩa nhiều bản ghi cùng lúc — tên "Chuyển họ hàng loạt" trong menu ám chỉ
công cụ này dùng để dọn nhiều trường hợp một-một liên tiếp, không phải một thao tác sửa N bản
ghi cùng lúc). Ghi chi tiết đầy đủ để làm ở lượt sau — **chưa đủ độ sâu để migrate ngay, cần đọc
lại kỹ UserControl chọn giáo dân/gia đình đích và các ràng buộc trước khi viết spec đầy đủ theo
đúng khuôn mẫu** (xem mục 9).

`frmChuyenHoGiaDinh.cs` (245d) tương tự nhưng cho **gia đình** (chuyển một gia đình sang giáo
họ khác, hoặc gộp gia đình?) — cũng chưa đọc đủ sâu để viết spec.

## 5. Tạo danh sách bí tích tự động — CHƯA XÁC ĐỊNH FILE NGUỒN

Chưa tìm ra file `.cs` tương ứng trong lượt tìm kiếm này (`Source/ChuongTrinh/` không có file
tên gợi ý rõ ràng như "TaoDanhSachBiTich"). Cần dò thêm ở `frmDotBiTichList.cs`/
`frmTaoDotBiTich.cs` (đã migrate một phần, xem `so-bi-tich.md`) xem có chức năng "tạo tự động"
ẩn trong đó không, hoặc màn hình này nằm ở một menu/toolbar khác chưa được rà tới. **Không đoán
— để trống, xem mục 9.**

## 6. Chỗ chưa chắc

- `KHOANGCACH_TUOI_CHAME_CONCAI` — giá trị hằng số chưa đọc; nhãn ô tick "Kiểm tra dữ liệu gia
  đình" tham chiếu hằng số này nhưng mã `ReviewGiaDinhProcess.cs` (quy tắc con thật sự chạy) lại
  dùng `TUOI_HON_PHOI_NAM`/`TUOI_HON_PHOI_NU` — chưa xác nhận đây là bug hay nhãn lỗi thời.
- Cách ghép chuỗi `NguyenNhan` khi giáo dân vi phạm ≥2 quy tắc (`string.Concat` không đảm bảo
  luôn có `\n` giữa hai đoạn) — chưa quan sát được kết quả thật trên UI desktop để xác nhận có
  bị dính chữ hay không.
- Cột `KetQua` (số nguyên cờ bit) có bị `GxGiaoDanList` ẩn khỏi lưới hiển thị hay không — không
  thấy cấu hình ẩn cột động theo tên trong các file đã đọc.
- "Chuyển họ hàng loạt" (`frmChuyenHoGiaoDan.cs`/`frmChuyenHoGiaDinh.cs`) và "Tạo danh sách bí
  tích tự động" — cả hai **chưa được nghiên cứu đủ sâu** để migrate; đây là quyết định có ý
  thức do giới hạn thời gian một lượt làm việc, không phải bỏ sót ngẫu nhiên. Việc chọn thứ tự
  ưu tiên (Kiểm tra dữ liệu → Chuẩn hoá → Chuyển hộ → Tạo ds bí tích) đã có sẵn trong nhiệm vụ
  gốc.
- "Chuẩn hoá dữ liệu" — chưa tìm và đọc file nguồn nào trong lượt này (gợi ý từ nhiệm vụ gốc:
  các khoá `CauHinh` `CHUANHOA_TRONGNGOAC/TUCHUANHOA/TUCHUYENMA/TUDOIDAU`, `Source/ConvertFont/`,
  `vnConvert.dll`) — cần một lượt đọc riêng trước khi viết spec.

## 7. Bản web đã làm (Kiểm tra dữ liệu — giáo dân)

- Backend: `WebApp/src/Qlgx.Api/Services/KiemTraDuLieuService.cs`,
  `WebApp/src/Qlgx.Api/Endpoints/KiemTraDuLieuEndpoints.cs`,
  `WebApp/src/Qlgx.Api/Dtos/KiemTraDuLieuDtos.cs`.
  `GET /api/cong-cu-du-lieu/kiem-tra-giao-dan?giaoHoId=&khongCoNgayThang=&saiQuanHeNgayThang=&ruocLeTruocTuoi=&thuocNhieuGiaDinh=&khongThuocGiaDinhNao=&coNhieuHonPhoi=`
  — mỗi cờ mặc định `true` nếu không truyền (khớp "mặc định tick sẵn" của desktop); trả về
  danh sách giáo dân vi phạm ít nhất 1 quy tắc đã chọn, kèm `nguyenNhan` (chuỗi các lý do nối
  bằng xuống dòng) và `ketQua` (tổng cờ bit).
- Không migrate thao tác Sửa/Xoá ngay trên lưới kết quả ở lượt này — bản web mở "Xem chi tiết"
  (điều hướng sang thẻ giáo dân đã có) thay cho việc mở dialog `frmGiaoDan` tại chỗ; xoá cứng
  trực tiếp trên lưới **không làm** (rủi ro cao, không có xác nhận số lượng như 4 nguyên tắc bắt
  buộc của nhiệm vụ) — người dùng muốn xoá phải mở "Danh sách giáo dân" hoặc "Hồ sơ lưu trữ".
- Không migrate "In danh sách" (xuất Excel) ở lượt này — dùng `chuaHoTro()` để không im lặng bỏ
  qua.
- Frontend: `WebApp/src/web/src/screens/KiemTraDuLieuGiaoDan.tsx` — bảng cờ 6 quy tắc + combo
  Giáo họ, nút "Bắt đầu kiểm tra", lưới kết quả (cột chuẩn + "Nguyên nhân"), tổng số dòng.

## 8. Chưa migrate lượt này

- Kiểm tra dữ liệu — gia đình (mục 3)
- Chuẩn hoá dữ liệu
- Chuyển họ hàng loạt
- Tạo danh sách bí tích tự động
