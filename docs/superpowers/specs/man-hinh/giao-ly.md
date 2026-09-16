# Màn hình: Giáo lý — Khối, Lớp, Học viên, Giáo lý viên (`Giaoly/frmKhoiGiaoLyList.cs` + `frmKhoiGiaoLy.cs` + `frmLopGiaoLy.cs` + `frmHocSinh.cs`)

| | |
|---|---|
| Tệp nguồn | `Source/Giaoly/frmKhoiGiaoLyList.cs` (215 dòng, UTF-16LE, danh mục khối) + `Source/Giaoly/frmKhoiGiaoLy.cs` (313 dòng, UTF-16LE, sửa một khối + danh sách lớp của khối) + `Source/Giaoly/frmLopGiaoLy.cs` (724 dòng, UTF-16LE, sửa một lớp + học viên + giáo lý viên — MÀN HÌNH LỚN NHẤT phân hệ) + `Source/Giaoly/frmHocSinh.cs` (177 dòng, sửa Hoàn thành/Ghi chú của MỘT học viên, hiện không còn được gọi — xem mục 9) |
| UserControl dùng lại | `GxAddEdit`×3 (thanh nút danh mục khối / học viên / giáo lý viên), `GxKhoiGiaoLyList : GxGrid` (lưới khối, `Source/GXControl` — thực ra nằm trong `Source/Giaoly`), `GxLopGiaoLyList : GxGrid` (lưới lớp lồng trong khối), `GxHocSinh : GxGrid` (lưới học viên), `txtNguoiQuanLy` (`GxGiaoDan`, chọn người quản lý khối) |
| Bảng dữ liệu đụng tới | `KhoiGiaoLy` (4 cột), `LopGiaoLy` (6 cột), `ChiTietLopGiaoLy` (5 cột — học viên), `GiaoLyVien` (2 cột) |
| Trạng thái migrate | Mới — trước task này mục "Quản lý giáo lý" có sẵn trong `SideNav` nhưng không có `id` nối tới đâu. Bốn bảng đã có ở CSDL Postgres (domain entities + EF config + bộ lọc GiaoXuId + migration đã có sẵn từ trước task này). Đã dựng `KhoiGiaoLyListPage.tsx` (danh mục khối) + `KhoiGiaoLyDetail.tsx` (sửa khối + danh sách lớp theo năm) + `LopGiaoLyDetail.tsx` (sửa lớp + học viên + giáo lý viên). Rỗng ở giáo xứ khảo sát (Vô Nhiễm chưa dùng chức năng giáo lý) → xem mục 10 cho bằng chứng kiểm thử bằng dữ liệu tự tạo. |

## 1. Mục đích

Quản lý cơ cấu giáo lý của giáo xứ theo ba cấp: **Khối giáo lý** (ví dụ Khai Tâm, Rước Lễ,
Thêm Sức...) → **Lớp giáo lý** (một khối có nhiều lớp, phân theo năm học) → **Học viên** (giáo
dân đã ghi danh vào một lớp cụ thể) + **Giáo lý viên** (giáo dân phụ trách dạy một lớp cụ thể).
Mở từ `frmMain.cs` (chưa xác định vị trí chính xác trong menu — chưa đọc file này, xem mục 9).

## 2. Bố cục và các trường

### Khối "Danh sách khối giáo lý" (`frmKhoiGiaoLyList`)

Lưới `gxKhoiGiaoLyList1` (`GxKhoiGiaoLyList.FormatGrid`, `Source/Giaoly/GxKhoiGiaoLyList.cs:52-88`),
4 cột theo thứ tự: Mã khối, Tên khối, Người quản lý, Ghi chú — xem mục 6. Có thêm combo `cbNam`
(2000-2049, mặc định năm hiện tại, `loadNam()` dòng 213-224) nhưng **combo này KHÔNG lọc lưới
khối** — `cbNam_SelectedIndexChanged` (dòng 226-230) hoàn toàn để trống (đã comment code lọc cũ).
Nó chỉ được dùng làm giá trị `NamGiaoLy` truyền xuống khi mở `frmKhoiGiaoLy` để Thêm/Sửa một khối
(dòng 82, 172) — tức năm chọn ở màn hình danh mục quyết định năm học của lớp được xem/thêm bên
trong khối đó.

### Khối "Sửa một khối giáo lý" (`frmKhoiGiaoLy`)

| Nhãn hiển thị | Control | Cột CSDL | Bắt buộc? | Mặc định | Ghi chú |
|---|---|---|---|---|---|
| "Mã khối" | `txtMaKhoi` (suy từ Designer) | `KhoiGiaoLy.MaKhoi` | có (tự sinh) | `Memory.Instance.GetNextId("KhoiGiaoLy", "MaKhoi", true)` lúc Thêm mới (`frmKhoiGiaoLy.cs:53`) | |
| "Tên khối" | `txtTenKhoi` | `KhoiGiaoLy.TenKhoi` | **có** | rỗng | `checkInput()` bắt buộc, thông báo **"Hãy nhập tên khối giáo lý"** (dòng 245-250) |
| "Người quản lý" | `txtNguoiQuanLy` (`GxGiaoDan`, lọc `AND Quadoi=0` — dòng 35) | `KhoiGiaoLy.NguoiQuanLy` | **có** (mâu thuẫn với mô hình dữ liệu — xem dưới) | rỗng | `checkInput()` bắt buộc, thông báo **"Hãy chọn người quản lý"** (dòng 252-257). Chỉ chọn được người **CHƯA qua đời** (Quadoi=0) — không loại người đã xoá mềm/đã chuyển xứ như các picker khác |
| "Ghi chú" | `txtGhiChu` | `KhoiGiaoLy.GhiChu` | không | rỗng | |
| Lưới lớp (chỉ đọc, chỉ hiện khi Sửa) | `gxLopGiaoLyList1` | — | — | — | Lọc `WHERE MaKhoi={Id} AND Nam={NamGiaoLy}` (dòng 271-274) — CHỈ hiện lớp của khối này TRONG năm đang chọn ở màn hình danh mục |

**Chỗ đáng chú ý**: cột `NguoiQuanLy` trong CSDL là khoá ngoại **NULLABLE** (mốc `-1`/`NULL` =
"chưa gán", xem ghi chú trong `KhoiGiaoLy.cs`), nhưng **màn hình `checkInput()` lại bắt buộc phải
chọn** — nghĩa là dữ liệu `NULL` chỉ có thể tồn tại nếu được ghi thẳng vào CSDL bằng cách khác
(import, sửa tay), không bao giờ phát sinh từ chính màn hình này. Bản web migrate ĐÚNG y hệt:
bắt buộc chọn Người quản lý khi thêm/sửa qua giao diện, nhưng cột CSDL vẫn nullable để không chặn
lớp dữ liệu cũ đã có sẵn giá trị NULL từ trước (xem mục 8).

### Khối "Sửa một lớp giáo lý" (`frmLopGiaoLy` — tên lớp C# là `frmLopGiaoLyList`, dễ nhầm với danh mục)

| Nhãn hiển thị | Control | Cột CSDL | Bắt buộc? | Mặc định | Ghi chú |
|---|---|---|---|---|---|
| "Mã lớp" | (suy từ Designer) | `LopGiaoLy.MaLop` | có (tự sinh) | Bug gốc: `Memory.Instance.GetNextId("KhoiGiaoLy", "MaGiaDinh", true)` (dòng 112) — SAI tên bảng/cột (đáng lẽ `"LopGiaoLy"`/`"MaLop"`, giống `AssignDataSource` dòng 466 dùng đúng `"LopGiaoLy"`/`"malop"`) — do dòng 112 gán `id` chỉ dùng để set tiêu đề form lúc ADD, giá trị mã lớp THẬT lại được sinh lại đúng bảng ở `AssignDataSource` khi lưu (dòng 466). Không có hậu quả thật (dead code kiểu gõ nhầm) — xem mục 8 |
| "Tên lớp" | `txtTenLop` | `LopGiaoLy.TenLop` | **có** | rỗng | `checkInput()`, thông báo **"Hãy nhập tên lớp giáo lý"** (dòng 444-449) |
| "Năm" | `cbNam` | `LopGiaoLy.Nam` | ngầm định có (luôn được gán) | `NamGiaoLy` truyền từ khối cha, **`cbNam.Enabled = false`** (dòng 606) | Không sửa được trực tiếp ở màn hình lớp — năm học lớp cố định theo năm đang chọn ở danh mục khối lúc mở form Thêm/Sửa lớp |
| "Phòng học" | `txtPhongHoc` | `LopGiaoLy.PhongHoc` | không | rỗng | |
| "Ghi chú" | `txtGhiChu` | `LopGiaoLy.GhiChu` | không | rỗng | |
| Lưới học viên | `gxHocSinhList1` (`GxHocSinh`) | `ChiTietLopGiaoLy.*` | — | — | Xem mục 6 |
| Lưới giáo lý viên | `gxGiaoLyVien1` | `GiaoLyVien.*` | — | — | Chỉ đọc (`AllowEdit = InheritableBoolean.False`, dòng 96), không filter (dòng 97) |

## 3. Hành vi khi tải

- `frmKhoiGiaoLyList_Load` (dòng 71-77): `FormatGrid()`, `loadNam()` (mặc định năm hiện tại), tải
  toàn bộ khối bằng `SELECT_KHOGIAOLY` — `SELECT KhoiGiaoLy.*, GiaoDan.TenThanh+' '+HoTen AS
  NguoiQuanLy FROM KhoiGiaoLy LEFT JOIN GiaoDan ON KhoiGiaoLy.NguoiQuanLy = GiaoDan.MaGiaoDan
  WHERE 1` (dòng 14-15) — `LEFT JOIN` nên khối có `NguoiQuanLy = NULL` (dữ liệu cũ, xem mục 2)
  vẫn hiện ra với cột Người quản lý rỗng, không bị loại khỏi danh sách.
- `frmKhoiGiaoLy_Load` (dòng 47-63): nếu Thêm mới → sinh mã, ẩn khối "lớp" (`uiGroupBox2.Visible
  = false`), thu nhỏ form; nếu Sửa → gọi `AssignControlData()` nạp Tên khối/Người quản lý/Ghi chú
  + lưới lớp lọc theo `MaKhoi` + `Nam` đang chọn.
- `frmLopGiaoLy_Load` (dòng 82-141): tải lưới học viên bằng câu SQL join `ChiTietLopGiaoLy` +
  `GiaoDan` + `LEFT OUTER JOIN GiaoHo`, lọc `DaXoa=0` (dòng 92) — nghĩa là học viên đã bị xoá
  MỀM khỏi hồ sơ giáo dân gốc tự động biến mất khỏi lưới lớp giáo lý dù bản ghi
  `ChiTietLopGiaoLy` vẫn còn (không dọn theo); tải lưới giáo lý viên (không lọc `DaXoa`); đếm
  tổng số học viên vào `lblTotal`; nút "Xem mẫu Excel"/"Nhập từ Excel" gắn vào `Button1`/`Button2`
  của `gxAddEdit1` (dòng 130-138, xem mục 8 — không migrate).

## 4. Quy tắc nghiệp vụ và kiểm tra dữ liệu

### Xoá một khối từ danh mục (`gxAddEdit1_DeleteClick`, `frmKhoiGiaoLyList.cs:120-152`)

- Xác nhận: **"Bạn có chắc muốn xóa khối giáo lý này? Các lớp giáo lý thuộc khối này sẽ bị xóa
  theo"** (tiêu đề "Xác nhận xóa", YesNo).
- Yes → với từng dòng chọn: `DELETE FROM ChiTietLopGiaoLy WHERE MaLop IN (SELECT MaLop FROM
  LopGiaoLy WHERE MaKhoi=?)` rồi `DELETE FROM LopGiaoLy WHERE MaKhoi=?` rồi `DELETE FROM
  KhoiGiaoLy WHERE MaKhoi=?` — xoá CỨNG cả ba tầng. Không xoá `GiaoLyVien` tường minh trong đoạn
  mã này (khả năng là lỗi sót — hoặc dựa vào ràng buộc CSDL Access đã có `ON DELETE CASCADE` từ
  `LopGiaoLy` xuống `GiaoLyVien`, chưa xác nhận được vì không có quyền truy cập schema Access
  gốc, xem mục 9). Bản web migrate Ý ĐỊNH ("xoá khối kéo xoá cả lớp/học viên/giáo lý viên của
  các lớp đó"), dùng ràng buộc khoá ngoại `ON DELETE CASCADE` cho `LopGiaoLy→{ChiTietLopGiaoLy,
  GiaoLyVien}` (đã cấu hình sẵn — xem `ChiTietLopGiaoLyConfig.cs`/`GiaoLyVienConfig.cs`) thay vì
  DELETE tay từng bảng, đảm bảo giáo lý viên chắc chắn được dọn theo (chặt hơn desktop nếu đúng
  là lỗi sót ở đó — xem mục 8).
- **Khác biệt cấu trúc quan trọng**: khoá ngoại `LopGiaoLy → KhoiGiaoLy` trong schema Postgres
  hiện tại là `DeleteBehavior.Restrict` (`LopGiaoLyConfig.cs`), KHÔNG cascade — nên service web
  phải tự xoá hết `LopGiaoLy` của khối đó TRƯỚC khi xoá `KhoiGiaoLy` (mỗi lần xoá một `LopGiaoLy`
  kéo cascade xoá `ChiTietLopGiaoLy`+`GiaoLyVien` của riêng lớp đó), giữ đúng "bán kính nổ" của
  bản gốc mà không cần đổi cấu hình cascade đã có.

### Xoá một lớp từ trong khối (`gxAddEdit1_DeleteClick` của `frmKhoiGiaoLy`, dòng 199-224)

- Xác nhận: **"Bạn có chắc muốn xóa lớp giáo lý này? Danh sách học sinh thuộc lớp giáo lý này sẽ
  bị xóa theo"**.
- Yes → `DELETE FROM ChiTietLopGiaoLy WHERE MaLop=?` rồi `DELETE FROM LopGiaoLy WHERE MaLop=?`.
  Không đụng `GiaoLyVien` (cùng nghi vấn lỗ hổng như trên). Bản web: xoá `LopGiaoLy` trực tiếp,
  để `ON DELETE CASCADE` dọn cả `ChiTietLopGiaoLy` lẫn `GiaoLyVien`.

### Lưu một khối (`updateKhoiGiaoLy`, `frmKhoiGiaoLy.cs:295-329`)

1. `checkInput()`: Tên khối rỗng → **"Hãy nhập tên khối giáo lý"**; Người quản lý rỗng →
   **"Hãy chọn người quản lý"**.
2. Không có kiểm tra trùng tên khối, không giao dịch nhiều bảng (chỉ ghi `KhoiGiaoLy`, KHÔNG ghi
   lại `LopGiaoLy` — lưới lớp trong form khối chỉ để XEM/mở form lớp riêng, không sửa trực tiếp
   trên lưới đó như hội đoàn).
3. Lỗi bất kỳ → hộp thoại Exception với tiêu đề "Lỗi Exception (frmKhoiGiaoLy, gxCommand1_OnOK)"
   (thông báo kỹ thuật thô, không phải câu tiếng Việt thân thiện — không migrate nguyên văn tiêu
   đề kỹ thuật này, xem mục 8).

### Thêm học viên vào lớp (`addGiaoDan`, `frmLopGiaoLy.cs:243-322`)

1. Giáo dân (theo `MaGiaoDan`) đã có trong lưới học viên đang sửa → **"Giáo dân này đã tồn tại
   trong danh sách"**.
2. Giáo dân đã thuộc về một lớp KHÁC **TRONG CÙNG KHỐI** (`SELECT * FROM ChiTietLopGiaoLy WHERE
   MaGiaoDan=? AND MaLop IN (SELECT MaLop FROM LopGiaoLy WHERE MaKhoi=?)`, dòng 267-268) →
   **"Giáo dân này đã thuộc về lớp khác"**. **Quan trọng**: kiểm tra CHỈ giới hạn trong PHẠM VI
   MỘT KHỐI — một giáo dân HOÀN TOÀN có thể học đồng thời ở lớp của khối Khai Tâm VÀ lớp của khối
   Rước Lễ (khác khối), không bị chặn. Không có ràng buộc UNIQUE toàn hệ thống (chỉ ràng buộc
   theo chỉ mục `(GiaoXuId, LopGiaoLyId, GiaoDanId)` ở tầng CSDL web, lỏng hơn — xem mục 8).
3. Giáo dân đã qua đời / đã chuyển xứ / đã xoá mềm → cảnh báo mềm YesNo: **"Giáo dân {tên} hiện
   tại {đã qua đời|đã chuyển xứ|đã xóa}. Bạn có muốn tiếp tục thêm giáo dân này.\r\nChọn [Yes] để
   tiếp tục.\r\nChọn [No] để hủy."** (nối nhiều lý do bằng dấu phẩy nếu nhiều điều kiện đúng cùng
   lúc, dòng 274-296). KHÔNG chặn cứng.
4. Qua hết → thêm dòng mới vào lưới, `HoanThanh=false`, `GhiChuGLy=""`, `SoThuTu` = số thứ tự kế
   tiếp tính từ MAX hiện có trên lưới +1 (`getNextSoThuTu`, dòng 679-691 — duyệt toàn bộ dòng
   đang hiện trên lưới, không phải truy vấn CSDL).
5. Hai nguồn chọn: nút "Thêm" mở `frmGiaoDan` (tạo giáo dân MỚI), nút "Chọn" mở `frmChonGiaoDan`
   (chọn giáo dân có sẵn) — sau khi thêm xong nút "Thêm" lặp lại vòng lặp `goto cont` để thêm
   liên tiếp nhiều giáo dân mới không cần đóng mở lại.

### Xoá học viên khỏi lớp (`gxAddEdit1_DeleteClick` của `frmLopGiaoLy`, dòng 421-442)

- Xác nhận đơn giản: **"Bạn có chắc muốn xóa?"** (không nêu tên như hội đoàn) — YesNo, No thì
  dừng. Yes → xoá CỨNG khỏi lưới (rồi khỏi CSDL khi bấm "Cập nhật" chính) — KHÔNG có khái niệm
  "đánh dấu đã ra" như hội đoàn, học viên chỉ có VÀO/RA hẳn khỏi lớp, không có lịch sử vào/ra
  nhiều lần.

### Thêm giáo lý viên vào lớp (`addGiaoLyVien`, dòng 386-427)

- Chỉ kiểm tra một điều: giáo dân đã có trong lưới giáo lý viên → **"Giáo lý viên này đã tồn tại
  trong danh sách"**. KHÔNG kiểm tra qua đời/chuyển xứ/xoá mềm như học viên (không nhất quán với
  học viên — bản web migrate y hệt, không thêm kiểm tra desktop không có, xem mục 8). KHÔNG kiểm
  tra một giáo lý viên có dạy nhiều lớp cùng lúc hay không (hợp lệ, một giáo lý viên có thể phụ
  trách nhiều lớp).

### Xoá giáo lý viên khỏi lớp (`gxAddEdit2_DeleteClick`, dòng 665-682)

- KHÔNG có hộp thoại xác nhận nào (khác xoá học viên/khối/lớp đều có) — xoá ngay khỏi lưới khi
  bấm nút. Bản web KHÔNG thêm xác nhận mà desktop không có (giữ nguyên, ghi vào mục 8/`can-review-
  sau.md` vì có thể gây xoá nhầm, nhưng không "âm thầm sửa cho đúng hơn").

### Lưu một lớp (`updateLopGiaoLy`, dòng 505-583)

1. `checkInput()`: chỉ kiểm tra Tên lớp — **"Hãy nhập tên lớp giáo lý"**.
2. Lưu `LopGiaoLy`, rồi TOÀN BỘ `ChiTietLopGiaoLy` hiện có trên lưới (gán lại `MaLop=id` cho từng
   dòng, dòng 545-550) qua MỘT `DataSet.UpdateDataSet` — bao gồm cả thêm/sửa/xoá theo trạng thái
   `RowState`.
3. XONG rồi mới xoá riêng từng học viên đã bấm xoá trước đó nhưng có thể chưa được xử lý bởi
   UpdateDataSet (`lstHSDel`, dòng 563-566) — có vẻ là lớp bảo hiểm kép/code cũ còn sót, vì
   `CurrentRow.Delete()` (dòng 434 lúc xoá trên lưới) đã tự đánh dấu `RowState=Deleted` để
   `UpdateDataSet` xử lý luôn ở bước 2; `lstHSUp`/`GhiChuGlyCls` tương tự là code chết vì đường
   gọi `frmHocSinh` để sửa Hoàn thành/Ghi chú qua form riêng đã bị COMMENT (dòng 208-238, thay
   bằng `gxHocSinhList1.EditRow()` sửa trực tiếp trên ô — xem mục 9).
4. Cập nhật `GiaoLyVien` tương tự (`DataSet.UpdateDataSet`) rồi xoá riêng theo `lstGLVDel`.
5. Lỗi bất kỳ → hộp thoại Exception kỹ thuật, tương tự khối.

## 5. Thao tác người dùng

| Thao tác | Điều kiện bật/tắt | Hành động |
|---|---|---|
| `frmKhoiGiaoLyList`: Thêm | Luôn bật | Mở `frmKhoiGiaoLy` trống, `NamGiaoLy` = năm đang chọn ở `cbNam` |
| `frmKhoiGiaoLyList`: Sửa | Chỉ bật khi có dòng chọn | Mở `frmKhoiGiaoLy` với dữ liệu dòng đó |
| `frmKhoiGiaoLyList`: Xoá | Chỉ bật khi có dòng chọn | Xem mục 4 |
| `frmKhoiGiaoLyList`: double-click dòng | — | Mở Sửa |
| `frmKhoiGiaoLyList`: In danh sách | Luôn bật | Xuất lưới khối ra `.xls` tạm rồi mở (`btnInDanhSach_Click`) |
| `frmKhoiGiaoLy`: Thêm lớp | Luôn bật | Mở `frmLopGiaoLyList` (thêm), truyền `IDKhoi` + `NamGiaoLy` |
| `frmKhoiGiaoLy`: Sửa lớp (nhãn nút đổi thành "Xem chi tiết") | Chỉ bật khi có dòng chọn | Mở `frmLopGiaoLyList` (sửa) |
| `frmKhoiGiaoLy`: Xoá lớp | Chỉ bật khi có dòng chọn | Xem mục 4 |
| `frmKhoiGiaoLy`: In | Luôn bật | Xuất lưới lớp ra `.xls` tạm |
| `frmLopGiaoLy`: Thêm học viên (nút "Thêm") | Luôn bật | Mở `frmGiaoDan` tạo mới rồi thêm luôn, lặp lại được |
| `frmLopGiaoLy`: Chọn học viên (nút "Chọn") | Luôn bật | Mở `frmChonGiaoDan` |
| `frmLopGiaoLy`: Sửa học viên (nhãn nút "Xem chi tiết") | Chỉ bật khi có dòng chọn | `gxHocSinhList1.EditRow()` — sửa trực tiếp trên ô lưới (`AllowEdit=True`), không mở form riêng |
| `frmLopGiaoLy`: Xoá học viên | Chỉ bật khi có dòng chọn | Xem mục 4 |
| `frmLopGiaoLy`: "Xem mẫu Excel" / "Nhập từ Excel" | Luôn bật | Tải file `.xls` mẫu / mở `frmImportHocVien` nhập hàng loạt — **ĐÃ MIGRATE (2026-09-08, task "giao-ly-2-quy-tac-11")**, xem mục 8 |
| `frmLopGiaoLy`: nút "Chuyển lớp" (`btn1`) | Luôn bật | Mở `frmChuyenLop` — **ĐÃ MIGRATE (2026-09-08, task "giao-ly-2-quy-tac-11")**, xem mục 8 |
| `frmLopGiaoLy`: Thêm/Chọn/Xoá giáo lý viên | Chọn cần dòng, Thêm/Chọn luôn bật | Xem mục 4 |
| `frmLopGiaoLy`: In (cả hai lưới) | Luôn bật | Xuất `.xls` tạm |

## 6. Lưới dữ liệu

### Danh mục khối (`gxKhoiGiaoLyList1`, `GxKhoiGiaoLyList.cs:52-88`)

| Thứ tự | Cột | Cột CSDL | Độ rộng |
|---|---|---|---|
| 1 | Mã khối | `KhoiGiaoLy.MaKhoi` | 50 |
| 2 | Tên khối | `KhoiGiaoLy.TenKhoi` | 250 |
| 3 | Người quản lý | `NguoiQuanLy` (tên ghép, tính sẵn ở SQL) | 250 |
| 4 | Ghi chú | `KhoiGiaoLy.GhiChu` | 200 |

### Lớp trong khối (`gxLopGiaoLyList1`, `GxLopGiaoLyList.cs:66-114`)

| Thứ tự | Cột | Cột CSDL | Độ rộng |
|---|---|---|---|
| 1 | Mã Lớp | `LopGiaoLy.MaLop` | 50 |
| 2 | Tên lớp | `LopGiaoLy.TenLop` | 200 |
| 3 | Phòng học | `LopGiaoLy.PhongHoc` | 80 |
| 4 | Giáo lý viên | tên ghép nhiều giáo lý viên nối `", "` (tính ở `LoadData`, không phải cột CSDL) | 250 |
| 5 | Ghi chú | `LopGiaoLy.GhiChu` | 200 |

### Học viên trong lớp (`gxHocSinhList1`, `GxHocSinh.cs:275-330`)

| Thứ tự | Cột | Cột CSDL | Độ rộng | Ghi chú |
|---|---|---|---|---|
| 1 | Số thứ tự | `ChiTietLopGiaoLy.SoThuTu` | 50 | Sửa được trực tiếp, phải là số (dòng 122-131 `GxHocSinh_UpdatingCell`: rỗng → **"Vui lòng nhập số thứ tự là số"**, huỷ sửa) |
| 2 | Tên thánh | `GiaoDan.TenThanh` | 90 | Chỉ đọc |
| 3 | Họ tên | `GiaoDan.HoTen` | 150 | Chỉ đọc |
| 4 | Phái | `GiaoDan.Phai` | 50 | Chỉ đọc |
| 5 | Ngày sinh | `GiaoDan.NgaySinh` | 80 | Chỉ đọc, `dd/MM/yyyy` |
| 5b | Ngày XTRLLĐ | `GiaoDan.NgayRuocLe` | 80 | Chỉ đọc — CHỈ có ở câu SQL `frmLopGiaoLy_Load`, không có ở `GxHocSinh.LoadData()` (hai nơi định nghĩa SQL khác nhau, một điểm không nhất quán — xem mục 9 |
| 6 | Tên Cha | `GiaoDan.HoTenCha` | 120 | Chỉ đọc |
| 7 | Tên Mẹ | `GiaoDan.HoTenMe` | 120 | Chỉ đọc |
| 8 | Hoàn thành khóa học | `ChiTietLopGiaoLy.HoanThanh` | 100 | Checkbox, sửa được trực tiếp |
| 9 | Ghi chú | `ChiTietLopGiaoLy.GhiChuGLy` | 180 | Sửa được trực tiếp |
| — | Mã lớp | `ChiTietLopGiaoLy.MaLop` | 0 (ẩn) | |

Sắp xếp mặc định theo Số thứ tự tăng dần (`SortKeys.Add`, dòng 315-317).

### Giáo lý viên trong lớp (`gxGiaoLyVien1`)

Không có `FormatGrid` riêng đọc được trong phạm vi khảo sát (không tìm thấy file định nghĩa lưới
này ngoài designer) — suy từ cách dùng (`sqlGiaoLyVien` join `GiaoLyVien`+`GiaoDan`) là cột Tên
thánh + Họ tên của giáo lý viên, chỉ đọc.

## 7. Liên kết sang màn hình khác

- `frmKhoiGiaoLyList` → `frmKhoiGiaoLy` (thêm/sửa khối).
- `frmKhoiGiaoLy` → `frmLopGiaoLyList` (thêm/sửa lớp của khối đó).
- `frmLopGiaoLy` → `frmGiaoDan` (tạo giáo dân mới làm học viên/giáo lý viên), `frmChonGiaoDan`
  (chọn giáo dân có sẵn), `frmChuyenLop` (chuyển học viên sang lớp khác — **đã migrate**, xem mục
  8), `frmImportHocVien` (nhập học viên hàng loạt từ Excel — **đã migrate**, xem mục 8).
- `frmHocSinh` hiện KHÔNG còn được gọi từ đâu (đường gọi duy nhất đã bị comment, thay bằng
  `EditRow()` sửa trực tiếp trên ô) — xem mục 9, không migrate màn hình này riêng (hành vi tương
  đương của nó — sửa Hoàn thành/Ghi chú — đã có sẵn qua sửa trực tiếp trên lưới học viên).

## 8. Khác biệt cố ý ở bản web

**Migrate y hệt** (kể cả chỗ desktop làm lạ):

- Người quản lý khối **bắt buộc chọn** dù cột CSDL nullable (mục 2) — giữ nguyên thông báo
  **"Hãy chọn người quản lý"**.
- Kiểm tra "học viên đã thuộc lớp khác" chỉ giới hạn TRONG CÙNG KHỐI, không toàn hệ thống (mục 4)
  — một giáo dân học đồng thời nhiều khối khác nhau là hợp lệ.
- Giáo lý viên KHÔNG kiểm tra qua đời/chuyển xứ/xoá mềm (khác học viên CÓ kiểm tra) — giữ nguyên
  sự không nhất quán này thay vì tự thêm kiểm tra cho "đều tay".
- Xoá giáo lý viên KHÔNG có hộp xác nhận (khác xoá học viên/khối/lớp) — giữ nguyên.
- Xoá học viên chỉ có "Vào/Ra hẳn", không có khái niệm "đánh dấu đã ra" giữ lịch sử như hội viên
  hội đoàn — giữ nguyên (`DELETE` cứng khỏi `ChiTietLopGiaoLy`).
- Số thứ tự (`SoThuTu`) học viên: tính bằng MAX(SoThuTu hiện có trong lớp) + 1, không dùng
  `SinhMaService` (không phải mã định danh cũ cần cấp phát nguyên tử toàn giáo xứ, chỉ là số thứ
  tự hiển thị trong PHẠM VI MỘT LỚP, tự do sửa lại được) — không có nguy cơ trùng khoá vì không
  phải khoá.

**ĐÃ MIGRATE (2026-09-08, task "giao-ly-2-quy-tac-11")** — hai chức năng từng hoãn ở commit
`d4c4b27`:

- **"Chuyển lớp"** (`frmChuyenLop.cs`, 203 dòng, `gxCommand1_OnOK` dòng 140-202) — chuyển một/
  nhiều học viên (chọn qua checkbox `Chon` trên lưới `gxHocSinhList1`) từ lớp hiện tại sang MỘT
  lớp đích (chọn qua ba combo Khối/Năm/Lớp, không giới hạn cùng khối — `loadComboLop` đọc
  `LopGiaoLy WHERE MaKhoi=? AND Nam=?` bất kể khối đang xem). Bug-for-bug migrate y hệt: tên
  "Chuyển lớp" nhưng thực chất chỉ THÊM một dòng `ChiTietLopGiaoLy` mới vào lớp đích cho mỗi học
  viên — **KHÔNG xoá học viên khỏi lớp nguồn** (`gxCommand1_OnOK` không có lệnh xoá nào). Học viên
  đã có sẵn ở lớp đích (theo `GiaoDanId`) bị BỎ QUA (đúng nhánh `MessageBox.Show("... đã tồn tại
  trong lớp ...")`, không chặn cả thao tác). `SoThuTu` dòng mới nối tiếp từ MAX hiện có ở lớp
  đích, đúng biến `soThuTuNext`. Bản gốc KHÔNG kiểm tra "học viên đã thuộc lớp khác cùng khối"
  (khác `ThemHocVien`) — bản web giữ nguyên, không tự thêm kiểm tra đó vào đường ghi hàng loạt.
  Khác bản gốc (yêu cầu an toàn bắt buộc của nhiệm vụ, không phải "sửa cho đúng"): CÓ bước xem
  trước tách riêng (nêu số học viên sẽ chuyển/sẽ bị bỏ qua) trước khi ghi, và ghi thật trong MỘT
  transaction (bản gốc chỉ gọi `Memory.UpdateDataSet(ds)` một lần, không transaction rõ ràng) —
  xem `GiaoLyService.XemTruocChuyenLop`/`ChuyenLop`, endpoint
  `POST /api/giao-ly/chuyen-lop/xem-truoc` và `POST /api/giao-ly/chuyen-lop`.
- **"Nhập học viên hàng loạt từ Excel"** (`frmImportHocVien.cs`, 225 dòng, mở hộp thoại chọn tệp
  rồi chạy nền) + logic thật ở `ImportData.ImportGiaoLy` (`Source/GXControl/ImportData.cs:76-
  226`) — đọc file Excel với các cột literal "Họ tên"/"Phái"/"Ngày sinh" (bắt buộc), "Mã GD"/"Tên
  thánh"/"Giáo họ"/"Ghi chú"/"Đã học xong" (tuỳ chọn), đối chiếu từng dòng với giáo dân có sẵn
  (theo Mã GD hoặc theo Họ tên+Tên thánh+Phái+Ngày sinh trùng), tạo giáo dân mới nếu không khớp,
  rồi thêm vào lớp. Bản web đọc bằng ClosedXML (hạ tầng có sẵn, KHÔNG dùng Office Interop như bản
  gốc), xử lý HOÀN TOÀN TRONG BỘ NHỚ (không ghi file lên đĩa máy chủ — ràng buộc HA), kiểm định
  dạng THẬT bằng cách thử mở workbook thay vì tin đuôi tệp. CÓ bước xem trước (yêu cầu an toàn bắt
  buộc, bản gốc không có) và ghi thật trong MỘT transaction. BA CHỖ THU HẸP PHẠM VI có chủ đích so
  với bản gốc (không phải "sửa cho đúng" — chỉ vì nền CSDL quan hệ có ràng buộc khoá ngoại thật,
  khác Access lỏng lẻo của bản gốc): (1) "Mã GD" phải khớp một giáo dân CÓ THẬT, không tự tạo dữ
  liệu mồ côi; (2) không tìm thấy giáo họ theo tên thì KHÔNG tự tạo giáo họ mới (bản gốc
  `getMaGiaoHo` âm thầm tạo mới) — chỉ cảnh báo, vẫn tạo giáo dân với `GiaoHoId=null`; (3) khi có
  nhiều giáo dân trùng, bản gốc dùng `Rows[0]` (thứ tự DataTable không xác định), bản web dùng
  `Id` nhỏ nhất để kết quả ổn định giữa các lần chạy. Xem `NhapHocVienGiaoLyService`, endpoint
  `GET /api/giao-ly/nhap-hoc-vien/mau-excel`, `POST /api/giao-ly/lop/{lopId}/nhap-hoc-vien/xem-
  truoc`, `POST /api/giao-ly/lop/{lopId}/nhap-hoc-vien`. Kiểm bằng dữ liệu tự tạo, xem
  `can-review-sau.md` mục 70 và `.superpowers/sdd/2026-09-06-qlgx-web-phase-1/task-giao-ly-2-quy-
  tac-11.md`.

**Không migrate**:

- Nút "In" xuất `.xls` tạm của cả ba lưới (khối/lớp/học viên) — chưa nối hạ tầng in ấn/xuất Excel
  chung (ClosedXML) cho phân hệ này, cùng tình trạng như "Danh sách hội đoàn" (`hoi-doan-danh-
  sach.md` mục 8).
- Hộp thoại lỗi kỹ thuật "Lỗi Exception (frmKhoiGiaoLy/frmLopGiaoLy, ...)" khi lưu thất bại —
  bản web trả lỗi HTTP kèm thông báo chung "Lưu thất bại, thử lại sau." (cùng quy ước các màn
  hình khác), không lộ tên hàm/class nội bộ.
- Nghi vấn thiếu xoá `GiaoLyVien` khi xoá khối/lớp ở mã desktop (mục 4) — bản web **chủ động xoá
  đầy đủ** qua cascade, chặt hơn bản gốc nếu đúng là thiếu sót ở đó (không chắc 100% vì có thể
  Access có cascade ở tầng schema mà mã C# không cần xử lý tay — xem mục 9).

**Cố ý MỞ RỘNG** so với bản gốc (cùng tinh thần đã áp dụng cho Hội đoàn — `hoi-doan-danh-
sach.md` mục 8):

- **Khối/Lớp/Học viên/Giáo lý viên lưu RIÊNG, mỗi thao tác gọi API ngay** — không gộp một giao
  dịch "Cập nhật" cho cả lớp+học viên+giáo lý viên như `updateLopGiaoLy`. Lý do giống hội đoàn:
  bảng trống ở dữ liệu khảo sát, mô hình tab không phải hộp thoại modal, tránh mất dữ liệu nếu
  đóng tab giữa chừng.
- **RowVersion chống ghi đè** cho cả bốn thực thể — bản desktop không có.
- **Năm học của lớp SỬA ĐƯỢC ở màn hình lớp** thay vì khoá cứng theo năm đang chọn ở danh mục
  khối (`cbNam.Enabled = false` ở bản gốc) — bản web coi "Năm" là một trường thông thường của lớp
  giáo lý (giống Phòng học/Ghi chú), sửa được sau khi tạo. Lý do: khoá cứng năm theo ngữ cảnh mở
  form (chỉ đọc được từ tham số truyền vào, không có ô riêng để sửa lại nếu nhập sai lúc tạo) là
  hạn chế của kiến trúc form modal desktop, không phải một quy tắc nghiệp vụ cần bảo toàn.
- **Bộ lọc Năm ở màn hình chi tiết khối** hiển thị TẤT CẢ các năm đã có lớp (không giới hạn
  2000-2049 cố định như combo desktop) — mặc định năm hiện tại, có mục "Tất cả các năm" — thân
  thiện hơn dò từng năm một.

## 9. Chỗ chưa chắc

- Vị trí mở từ `frmMain.cs` — chưa đọc file này.
- `ImportData.ImportGiaoLy` (`ImportData.cs:76-226`) — chưa xác nhận được liệu Access/Jet OLEDB có
  ràng buộc khoá ngoại chặn INSERT khi "Mã GD" trong tệp Excel không khớp giáo dân nào (bản gốc
  dùng thẳng số nhập, không kiểm tra tồn tại) hay không — nếu không có ràng buộc, bản gốc tạo ra
  dữ liệu mồ côi thật; nếu có, bản gốc sẽ báo lỗi INSERT (không đọc được thông báo cụ thể).
- `GxHocSinh.EditRow()` mở `frmGiaoDan` với `Operation = GxOperation.NONE` ("show for view
  only") — tức nút "Xem chi tiết" (đổi nhãn từ "Sửa") ở `frmLopGiaoLy` thực chất CHỈ XEM hồ sơ
  giáo dân, không sửa Hoàn thành/Ghi chú GLy qua đó — vậy Hoàn thành/Ghi chú GLy chỉ sửa được
  bằng cách bấm trực tiếp vào Ô trên lưới (`AllowEdit=True` ở cấp `gxHocSinhList1`, không phải ở
  `GxHocSinh.EditRow()`). Bản web migrate đúng tinh thần này: sửa trực tiếp trên lưới (dùng form
  sửa riêng theo mẫu Hội đoàn thay vì sửa-trên-ô AG Grid, cùng lý do đã áp dụng ở `hoi-doan-danh-
  sach.md` mục 8 — "cùng tinh thần cho sửa").
- Không tìm thấy định nghĩa `FormatGrid` của `gxGiaoLyVien1` trong phạm vi đã đọc — cột của lưới
  giáo lý viên suy từ câu SQL dùng để tải nó, chưa chắc chắn 100% có cột nào khác bị ẩn.
- Nghi vấn `GiaoLyVien` không bị xoá cứng tường minh khi xoá Khối/Lớp ở mã desktop (mục 4/8) —
  chưa xác nhận được có ràng buộc CSDL Access xử lý thay hay đây là lỗ hổng thật (dữ liệu mồ côi
  tồn tại vĩnh viễn trong bảng `GiaoLyVien` gốc). Không ảnh hưởng tới bản web vì bản web tự đảm
  bảo dọn sạch qua cascade.
- `SqlConstants.SELECT_KHOGIAOLY` gốc dùng `LEFT JOIN` nên khối với `NguoiQuanLy` trỏ tới một
  `MaGiaoDan` KHÔNG CÒN TỒN TẠI (giáo dân đã bị xoá cứng ở đâu đó — hiếm nhưng có thể) sẽ hiện ra
  với tên rỗng thay vì lỗi — bản web dùng FK thật (`OnDelete(SetNull)`) nên tình huống này không
  thể xảy ra ở dữ liệu mới tạo qua web, chỉ có thể còn sót lại từ dữ liệu Access cũ chuyển sang.

## 10. Kiểm thử bằng dữ liệu tự tạo (bảng rỗng ở giáo xứ khảo sát)

Xem báo cáo kiểm thử trong `.superpowers/sdd/2026-09-06-qlgx-web-phase-1/task-giao-ly.md` và ảnh
chụp `WebApp/anh-chup-kiem-thu/149-...` trở đi — tạo một khối giáo lý mẫu, một lớp thuộc khối đó,
vài học viên (giáo dân thật), một giáo lý viên, sửa, xoá, dọn sạch qua chính giao diện, đối chiếu
`psql` trước/sau.
