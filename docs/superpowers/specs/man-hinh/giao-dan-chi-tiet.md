# Màn hình: Chi tiết giáo dân (`frmGiaoDan.cs`)

| | |
|---|---|
| Tệp nguồn | `Source/GXControl/frmGiaoDan.cs` (1547 dòng) + `frmGiaoDan.Designer.cs` (1877 dòng) |
| UserControl dùng lại | `GxHonPhoi` (tab Hôn phối), `GxTanHien` (tab Ơn gọi tận hiến, biến `gxTanHien1`), `GxHistoryHoiDoan` (tab Hội đoàn), `GxPictureField` (ảnh đại diện), `GxGiaoHo` (combo giáo họ), các `GxText`/`GxDateField`/`GxComboBox` dùng chung (bao gồm `txtTenCha`/`txtTenMe` là ô chọn giáo dân kiêm nhập tay), `GxCommand` (thanh nút Cập nhật/Đóng/Xem gia đình) |
| Bảng dữ liệu đụng tới | `GiaoDan` (63 cột, bảng chính), `ChuyenXu` (thông tin chuyển xứ 1-1), `ThanhVienGiaDinh`, `BiTichChiTiet`, `HonPhoi` (qua `GxHonPhoi`), lịch sử Hội đoàn (qua `GxHistoryHoiDoan`) |
| Trạng thái migrate | xong (web: `GiaoDanDetail.tsx` + `GiaoDanDetailPage.tsx`) — nhưng phần lớn tab và toàn bộ nghiệp vụ kiểm tra dữ liệu chưa có, xem mục 10 |

## 1. Mục đích

Thêm mới / sửa / xem chi tiết một giáo dân: thông tin cá nhân, các bí tích (rửa tội, rước lễ,
thêm sức, xức dầu), thông tin chuyển xứ, hôn phối, ơn gọi tận hiến, hội đoàn, giáo lý (bao đồng,
vào đời, hôn nhân). Mở từ `frmGiaoDanList` (Thêm/Sửa/double-click), từ `frmGiaDinh` (thêm/sửa
thành viên gia đình), và các nơi khác chưa thuộc phạm vi 2 màn hình được giao (menu chính, tìm
kiếm...).

## 2. Bố cục và các trường

Form có `tabControl1` với 5 tab, theo đúng thứ tự khai báo trong Designer
(`frmGiaoDan.Designer.cs:35,102,121,123,126`, tiêu đề tại các dòng 210, 1348, 1644, 1673, 1710):

1. **Cá nhân** (`tabCaNhan`)
2. **Giáo lý** (`tabGiaoLy`)
3. **Hôn phối** (`tabHonPhoi`) — nội dung do `GxHonPhoi` tự quản lý, không đọc chi tiết trong
   phạm vi buổi này
4. **Ơn gọi tận hiến** (`tabOnGoi`) — nội dung do `GxTanHien` tự quản lý
5. **Hội đoàn** (`tabHoiDoan`) — nội dung do `GxHistoryHoiDoan` tự quản lý

Bảng dưới liệt kê các trường trong tab **Cá nhân** và **Giáo lý** (đọc trực tiếp từ
`AssignControlData`/`AssignDataSource`, `frmGiaoDan.cs:953-1040` và `1082-1208`, đối chiếu nhãn ở
Designer). GiaoDan có 63 cột CSDL; nhóm theo nghiệp vụ:

### Thông tin cá nhân cơ bản

| Nhãn | Cột CSDL | Kiểu | Bắt buộc? | Mặc định | Ghi chú |
|---|---|---|---|---|---|
| Mã giáo dân | `MaGiaoDan` | Text, `MaxLength=12` | Có (phải là số) | tự sinh (`Memory.Instance.GetNextId`) | Chỉ nhập tay được nếu cấu hình `CF_TUNHAP_MAGIAODAN=CF_TRUE` **và** đang thêm mới (`frmGiaoDan.cs:193-199`) |
| Giới tính | `Phai` | Combo cố định "Nam"/"Nữ" | Có | — | Đổi giới tính bị chặn nếu giáo dân đã là vợ/chồng trong gia đình hoặc hôn phối nào đó — xem mục 4 |
| Ngày sinh | `NgaySinh` | Date | Có | rỗng khi thêm mới | |
| Tên thánh | `TenThanh` | Text | Không bắt buộc cứng, nhưng cảnh báo nếu đã rửa tội mà bỏ trống | | |
| Họ tên | `HoTen` | Text, `MaxLength=255` | Có | | |
| Nơi sinh | `NoiSinh` | Text, `MaxLength=255` | Không | | |
| Tên Cha | `HoTenCha` | Text kiêm picker chọn giáo dân nam (`txtTenCha`) | Không | tự điền từ hồ sơ gia đình nếu để trống | |
| Tên Mẹ | `HoTenMe` | Text kiêm picker chọn giáo dân nữ (`txtTenMe`) | Không | tự điền tương tự | |
| Giáo họ | `MaGiaoHo` | Combo `GxGiaoHo` | Có (không được để trống lựa chọn) | | Chọn "Ngoài xứ" (giá trị 0) hiện thêm Giáo xứ/Giáo phận, ẩn khối "Thông tin chuyển xứ" |
| Giáo xứ (khi Ngoài xứ) | `ThuocGiaoXu` | Text, `MaxLength=255` | Không | | |
| Giáo phận (khi Ngoài xứ) | `ThuocGiaoPhan` | Text, `MaxLength=255` | Không | | |
| CMND/CCCD | `CMND` | Text, `MaxLength=255` | Không | | |
| Là giáo dân không được thống kê | `GiaoDanAo` | Checkbox (`chkGiaoDanAo`) | — | không tick | Tick vào thì tự đặt `cbGiaoHo.SelectedValue=0` (Ngoài xứ) — `chkGiaoDanAo_CheckedChanged`, dòng 1311-1317 |
| Đã xóa | `DaXoa` | Checkbox (`chkDelete`, "Đã xóa") | — | ẩn | Chỉ **hiện lên** khi bản ghi đang tải có `DaXoa=true` (dòng 1138-1142); nếu hiện, người dùng có thể bỏ tick để phục hồi bản ghi đã xóa mềm |

### Thông tin khác (nhóm `uiGroupBox5`, "Thông tin khác")

| Nhãn | Cột CSDL | Ghi chú |
|---|---|---|
| Trình độ văn hóa | `TrinhDoVanHoa` | Combo tự do (không còn danh sách mẫu cố định — các item mẫu Mẫu giáo/Cấp I.../Đại học bị comment hết ở constructor, dòng 30-37) |
| Trình độ ch.môn | `TrinhDoChuyenMon` | thêm 2018-07-17 |
| Biết ngoại ngữ | `BietNgoaiNgu` | thêm 2018-07-17 |
| Nghề nghiệp | `NgheNghiep` | Combo có 6 gợi ý dựng cứng trong code: Nông dân, Công nhân, Nhân viên, Buôn bán, Nội trợ, Nghề tự do (dòng 39-44) |
| Điện thoại | `DienThoai` | `MaxLength=20` |
| Email | `Email` | `MaxLength=255` |
| Địa chỉ | `DiaChi` | `MaxLength=255`; có nút xem bản đồ (`pictureBox1_Click`) |
| Dân tộc | `DanToc` | `MaxLength=255` |
| Ghi chú chung | `GhiChu` | `MaxLength=32767` (multiline) |
| Qua đời | `QuaDoi` | Checkbox; tick thì hiện Ngày qua đời/Nơi qua đời/Số sổ/Nơi an táng và **tự bỏ tick Còn học** |
| Còn học | `ConHoc` | Checkbox; tick thì **tự bỏ tick Qua đời** và xóa `NgayQuaDoi` |
| Tân tòng | `TanTong` | Checkbox |
| Có gia đình | `DaCoGiaDinh` | Checkbox — tự động bật/tắt theo dữ liệu Hôn phối (`gxHonPhoi1_HonPhoiChanged`), người dùng **không được phép** bỏ tick tay nếu đang có hôn phối còn hiệu lực — xem mục 4 |
| Ngày qua đời | `NgayQuaDoi` | ẩn/hiện theo `QuaDoi` |
| Nơi qua đời | `NoiQuaDoi` | `MaxLength=255`, ẩn/hiện theo `QuaDoi` |
| Số sổ (an táng) | `SoAnTang` | `MaxLength=255`, ẩn/hiện theo `QuaDoi` |
| Nơi an táng | `NoiAnTang` | `MaxLength=255`, ẩn/hiện theo `QuaDoi` |

### Thông tin chuyển xứ (`uiGroupBox6`, ẩn khi Ngoài xứ)

| Nhãn | Cột CSDL | Ghi chú |
|---|---|---|
| Thông tin hiện tại | `ChuyenXu.LoaiChuyen` | Combo 3 lựa chọn: "Tại xứ" (0, mặc định ẩn dòng comment), "Đến" (`LOAI_CHUYENXU_DEN`), "Đi" (`LOAI_CHUYENXU_DI`) — hằng số text lấy từ `GxConstants` |
| Ngày chuyển | `ChuyenXu.NgayChuyen` | |
| Giáo xứ | `ChuyenXu.NoiChuyen` (`txtGiaoXuChuyen`) | `MaxLength=255` |
| Ghi chú | `ChuyenXu.GhiChuChuyen` | `MaxLength=255` |
| (nút) Xem lịch sử chuyển xứ | — | `btnChuyenXu`, nhưng bị ẩn hẳn ở constructor (`btnChuyenXu.Visible = false`, dòng 79) — **chức năng chết trên UI** dù nút và nhãn vẫn tồn tại trong Designer |

### Bí tích Rửa tội

| Nhãn | Cột CSDL |
|---|---|
| Ngày rửa tội | `NgayRuaToi` |
| Số sổ | `SoRuaToi` |
| Người ban bí tích | `ChaRuaToi` |
| Người đỡ đầu | `NguoiDoDauRuaToi` |
| Nơi rửa tội | `NoiRuaToi` |

### Bí tích Rước lễ (lần đầu)

| Nhãn | Cột CSDL |
|---|---|
| Ngày rước lễ | `NgayRuocLe` |
| Số sổ | `SoRuocLe` |
| Người ban bí tích | `ChaRuocLe` |
| Nơi rước lễ | `NoiRuocLe` |

### Bí tích Thêm sức

| Nhãn | Cột CSDL |
|---|---|
| Ngày thêm sức | `NgayThemSuc` |
| Số sổ | `SoThemSuc` |
| Người ban bí tích | `ChaThemSuc` |
| Người đỡ đầu | `NguoiDoDauThemSuc` |
| Nơi thêm sức | `NoiThemSuc` |

### Xức dầu (bệnh nhân)

| Nhãn | Cột CSDL | Ghi chú |
|---|---|---|
| Ngày xức dầu | `NgayXucDau` | |
| Tình trạng | `TinhTrangXucDau` | Combo cố định 2 giá trị: "Nguy tử", "Thông thường" (dòng 104-106) |
| Người ban bí tích | `NguoiXucDau` | |
| Ghi chú | `GhiChuXucDau` | `MaxLength=255` |

### Tab Giáo lý — Bao đồng 1 / Bao đồng 2 / Vào đời / Hôn nhân

| Nhãn | Cột CSDL |
|---|---|
| Ngày kết thúc khóa học (BĐ1) | `NgayBD1` |
| Tại giáo xứ (BĐ1) | `NoiBD1` |
| Ngày rước lễ trọng thể (BĐ2) | `NgayBD2` |
| Tại giáo xứ (BĐ2) | `NoiBD2` |
| Ngày tuyên hứa (Vào đời) | `NgayTHVaoDoi` |
| Tại giáo xứ (Vào đời) | `NoiTHVaoDoi` |
| Khóa học từ ngày (GL hôn nhân) | `NgayGLHN1` |
| đến ngày (GL hôn nhân) | `NgayGLHN2` |
| Tại giáo xứ (GL hôn nhân) | `NoiGLHN` |
| Người cấp chứng nhận (GL hôn nhân) | `NguoiChungNhanGLHN` |
| Xếp loại (GL hôn nhân) | `XepLoaiGLHN` | Combo cố định: Trung Bình, Khá, Giỏi |

Trường nội bộ không hiển thị trực tiếp nhưng được gán tự động: `UpdateDate` (thời điểm cập nhật
server), `MaNhanDang` (khóa đồng bộ, chỉ sinh khi rỗng — xem `Memory.GetGiaoDanKey`,
`frmGiaoDan.cs:1008-1011`), `AnhDaiDien` (đường dẫn file ảnh tương đối so với `Memory.AppPath`).

## 3. Hành vi khi tải

- **Thêm mới** (`GxOperation.ADD`, `frmGiaoDan_Load`, dòng 201-210): sinh sẵn `MaGiaoDan` kế tiếp
  qua `Memory.Instance.GetNextId`, đặt các trường ngày về "rỗng" (`IsNullDate=true`), ẩn nút
  "Xem gia đình" (`gxCommand1.btn1.Visible = false`).
- **Sửa** (`GxOperation.EDIT`, dòng 220-237): nạp dữ liệu hôn phối (`gxHonPhoi1.LoadData()`),
  nạp dữ liệu ơn gọi tận hiến (`gxTanHien1.AssignControlData()`), nạp lịch sử hội đoàn
  (`gxHistoryHoiDoan1.loaddata(id)`) **ngay khi mở form**, không đợi người dùng chuyển tab.
- **Xem** (`GxOperation.VIEW`): ẩn nút OK (Cập nhật) — chỉ đọc.
- Con trỏ: focus vào ô Tên thánh (`txtTenThanh.Combobox.Focus()`), có gửi thêm phím Tab
  (`SendKeys.Send("\t")`) nếu focus chưa vào đúng chỗ (dòng 250-252) — cách làm khá "vá lỗi",
  không chắc lý do gốc.
- Không có sắp xếp/bộ lọc (đây là form 1 bản ghi, không phải lưới).

## 4. Quy tắc nghiệp vụ và kiểm tra dữ liệu

Toàn bộ kiểm tra chạy trong `checkInput()` (`frmGiaoDan.cs:256-465`), được gọi từ
`updateDataGiaoDan()` mỗi khi bấm Cập nhật hoặc khi chuyển tab lúc đang thêm mới. Theo đúng thứ
tự trong mã:

1. **Ràng buộc ngày không được sau ngày hiện tại**: áp cho `NgaySinh`, `NgayRuaToi`, `NgayRuocLe`,
   `NgayThemSuc`, `NgayQuaDoi`, `NgayXucDau`, `NgayBD1`, `NgayBD2`, `NgayVaoDoi`, `GLHNTuNgay`,
   `GLHNDenNgay` — không được lớn hơn "ngày hiện tại" (dòng 261-265). Nếu đã có `NgayQuaDoi`, cận
   trên đổi thành chính `NgayQuaDoi` thay vì hôm nay (dòng 278-285).
2. **Ràng buộc ngày không được trước ngày sinh**: các ngày trên (trừ `NgaySinh`) không được nhỏ
   hơn `NgaySinh` (dòng 267-272); riêng `NgayBD2 >= NgayBD1`, `NgayVaoDoi >= NgayBD2`,
   `GLHNDenNgay >= GLHNTuNgay` (dòng 274-276).
3. **Mã giáo dân phải là số**: nếu không → `"Mã giáo dân phải được nhập số"`, lỗi (dòng 288-293).
4. **Họ tên bắt buộc**: `"Hãy nhập Họ tên"` (dòng 294-299).
5. **Giới tính bắt buộc**: `"Hãy nhập giới tính"` (dòng 301-306).
6. **Ngày sinh phải hợp lệ**: `"Hãy nhập ngày sinh hợp lệ"` (dòng 308-313).
7. **Phải chọn giáo họ**: `"Hãy chọn một giáo họ!"` (dòng 315-320, không có tiêu đề hộp thoại
   riêng, dùng tiêu đề mặc định của `MessageBox`).
8. **Cảnh báo lệch giữa Giáo họ và cờ "không thống kê"** — hai cảnh báo dạng Yes/No, chọn No thì
   quay lại nhập tiếp, chọn Yes thì bỏ qua và lưu:
   - Chọn giáo họ thật (không phải Ngoài xứ) mà **không** tick "không được thống kê":
     > "Thường thì chỉ có giáo dân không được thống kê mới chọn giáo họ là [Ngoài xứ]\r\nBạn có
     > chắc chọn giáo họ [`<tên giáo họ>`] cho giáo dân không?\r\nChọn [Yes] đóng màn hình và lưu
     > thông tin đã nhập\r\nChọn [No] không đóng màn hình và nhập lại thông tin" (dòng 324-325)
     — **câu này đọc ngược nghĩa gốc**: logic thật là nếu **không** chọn Ngoài xứ và **không**
     tick ảo thì mới hỏi, nhưng nội dung câu hỏi lại nói theo hướng "giáo dân ảo mới nên chọn
     Ngoài xứ" — dễ gây hiểu lầm cho người dùng vì tình huống thực tế (không phải ảo + không
     phải ngoài xứ) là **bình thường**, không có gì đáng cảnh báo. Đây có thể là lỗi logic
     hoặc lỗi soạn câu chữ khi viết, xem "Chỗ chưa chắc".
   - Ngược lại — tick "không được thống kê" mà lại chọn giáo họ thật (không phải Ngoài xứ):
     > "Thường thì giáo dân không được thống kê sẽ được chọn giáo họ là [Ngoài xứ]\r\nBạn có chắc
     > chọn giáo họ [`<tên giáo họ>`] cho giáo dân không được thống kê này không?..." (dòng
     > 332-335)
9. **Ràng buộc thứ tự Sinh ≤ Rửa tội ≤ Rước lễ ≤ Thêm sức** — cảnh báo (không chặn cứng):
   > "Hãy đảm bảo Ngày sinh <= Ngày rửa tội <= Ngày rước lễ lần đầu <= Ngày thêm sức.\r\nBạn có
   > chắc muốn lưu thông tin giáo dân này không?" (Yes tiếp tục lưu, No quay lại — dòng 377-382)
   **BUG đã xác nhận bằng đọc mã**: hàm `isValidDateInputRelations`
   (`frmGiaoDan.cs:467-492`) build danh sách `lstDate` để so sánh từng cặp, nhưng dòng 476-479
   copy-paste sai — cả 4 dòng đều thêm `ngayRuaToi` vào danh sách thay vì lần lượt
   `ngayRuaToi`, `ngayRuocLe`, `ngayRuocLe`, `ngayThemSuc` (hoặc tương tự đúng ý đồ):
   ```
   if (ngayRuaToi != DBNull.Value) lstDate.Add(ngayRuaToi.ToString());
   if (ngayRuaToi != DBNull.Value) lstDate.Add(ngayRuaToi.ToString());
   if (ngayRuaToi != DBNull.Value) lstDate.Add(ngayRuaToi.ToString());
   if (ngayRuaToi != DBNull.Value) lstDate.Add(ngayRuaToi.ToString());
   ```
   Kết quả: **quy tắc chỉ thật sự so sánh Ngày sinh với Ngày rửa tội** (3 lần so sánh còn lại là
   Ngày rửa tội với chính nó, luôn đúng). Tham số `ngayRuocLe` và `ngayThemSuc` của hàm **hoàn
   toàn không được dùng** dù được truyền vào. Người dùng có thể nhập Ngày thêm sức trước Ngày
   rước lễ mà không hề bị cảnh báo — trái với thông báo hiển thị. Bản web cần quyết định: sửa
   đúng logic gốc (so sánh đủ 4 mốc) hay giữ nguyên hành vi lỗi này — đây là quyết định của
   người dùng/giáo xứ, không tự ý sửa.
10. **Cảnh báo rước lễ khi chưa đủ tuổi** — `Memory.KiemTraTuoiKhongHopLe(NgaySinh, NgayRuocLe,
    GxConstants.TUOI_RUOCLE)`:
    > "Giáo dân này rước lễ lần đầu khi chưa được `<TUOI_RUOCLE>` tuổi.\r\nBạn có chắc muốn lưu
    > thông tin giáo dân này không?" (Yes/No, dòng 391-396) — giá trị `TUOI_RUOCLE` chưa xác nhận
    > (xem mục 9).
11. **Trùng ngày chuyển xứ**: khi đang sửa và có chọn loại chuyển xứ mới khác loại đã lưu, kiểm
    tra đã tồn tại bản ghi `ChuyenXu` cùng `MaGiaoDan` + `NgayChuyen` chưa:
    > "Đã có ngày chuyển xứ của giáo dân này trùng với ngày chuyển xứ bạn nhập\r\nXin vui lòng
    > nhập ngày khác" (lỗi cứng, dòng 407-412).
12. **Đã rửa tội mà chưa có Tên thánh** — cảnh báo mềm:
    > "Giáo dân này đã rửa tội nhưng chưa được nhập Tên Thánh.\r\nBạn có chắc muốn lưu thông tin
    > giáo dân này không?" (Yes/No, dòng 417-424).
13. **Nếu đã tick "Có gia đình" và có Ngày sinh** (dòng 431-434): hàm `checkInput()` **kết thúc
    ngay** bằng kết quả của `Memory.checkTuoiKetHon(NgaySinh)` (kiểm tra tuổi kết hôn hợp lệ, tự
    hiển thị thông báo lỗi riêng nếu có — nội dung chưa đọc, xem mục 9) — **bỏ qua hẳn bước kiểm
    tra mã giáo dân trùng (`checkGiaoDan()`, dòng 464)** trong trường hợp này. Nếu **chưa** tick
    "Có gia đình" (trường hợp phổ biến nhất), luồng chạy tiếp bình thường xuống `checkGiaoDan()`
    ở dòng 464.
14. **Kiểm tra mã giáo dân trùng** (`checkGiaoDan()`, dòng 510-531) — chỉ áp dụng khi cấu hình cho
    phép tự nhập mã (`CF_TUNHAP_MAGIAODAN`) và đang thêm mới, hoặc đang sửa mà đổi mã: kiểm tra
    trùng ở cả 4 bảng `GiaoDan`, `ChuyenXu`, `ThanhVienGiaDinh`, `BiTichChiTiet`:
    > "Mã giáo dân đã tồn tại. Vui lòng nhập mã khác!" (dòng 524).
15. **Kiểm tra tuổi cha/mẹ hợp lệ so với con** (`CheckTuoiChaMe`, dòng 586-607) — chạy trong
    `updateDataGiaoDan()` trước khi lưu, nếu đã chọn Tên Cha/Tên Mẹ là một giáo dân có sẵn
    (`MaGiaoDan > 0`): tuổi cha/mẹ trừ tuổi con phải ≥ `GxConstants.TUOI_CHO_PHEP_CO_CON`, nếu
    không:
    > "Vui lòng xem lại.\r\n`<tên phụ huynh>` chưa đủ 15 tuổi để có con. Tuổi phụ huynh phải lớn
    > hơn tuổi của giáo dân ít nhất là 15" (lỗi cứng, dòng 600) — **văn bản ghi cứng "15 tuổi"**
    > dù giá trị thật lấy từ hằng số `TUOI_CHO_PHEP_CO_CON` — nếu hằng số này không phải 15 thì
    > thông báo sẽ sai lệch so với giá trị thực sự dùng để so sánh.
16. **Kiểm tra trùng dữ liệu (Họ tên + Tên thánh + Ngày sinh)** trước khi ghi (dòng 673-708):
    nếu trùng, hỏi 3 lựa chọn:
    > "Đã có giáo dân cùng họ tên, tên thánh và ngày sinh trong hệ thống.\r\nBạn có muốn xem lại
    > thông tin của giáo dân đã lưu trong chương trình trùng thông tin bạn nhập?\r\n - Nhấp nút
    > [Yes] để chương trình hiển thị thông tin của người đã tồn tại trong hệ thống có thông tin
    > trùng thông tin bạn nhập, thông tin bạn vừa nhập sẽ bị bỏ qua\r\n - Nhấp nút [No] để lưu
    > thông tin bạn vừa nhập thành một giáo dân mới và chấp nhận trùng thông tin giáo dân\r\n -
    > Nhấp nút [Cancel] để quay lại màn hình nhập giáo dân để bạn kiểm tra lại thông tin và không
    > lưu gì cả" (dòng 677-681) — chọn Yes sẽ **nạp lại toàn bộ form bằng dữ liệu của bản ghi cũ
    > đã tồn tại, hủy bỏ những gì vừa nhập** (`Id = ...; AssignControlData(); return false;`).
17. **Đổi giới tính bị chặn** nếu giáo dân đã là vợ/chồng trong một gia đình hoặc hôn phối
    (`cbPhai_SelectedIndexChanged`, dòng 1482-1495):
    > "Giáo dân này đã được nhập là vợ/chồng trong một gia đình hoặc hôn phối. Không thể thay đổi
    > giới tính cho giáo dân này.\r\nĐể thay đổi giới tính, bạn phải tìm tất cả các gia đình hoặc
    > hôn phối mà giáo dân này là vợ/chồng và bỏ đi quan hệ đó trước" — tự động **đảo ngược lại**
    > lựa chọn vừa chọn (dòng 1491-1492).
18. **Không cho bỏ tick "Có gia đình"** nếu đang có hôn phối còn hiệu lực (người còn sống)
    (`chkDaCoGiaDinh_CheckedChanged`, dòng 1516-1535):
    > "Người này đang có thông tin hôn phối.\r\nBạn chỉ có thể bỏ mục chọn này nếu danh sách hôn
    > phối của người này là trống hoặc bạn đời của người này đã qua đời." — tự tick lại
    > (dòng 1528-1531).
19. **Chuyển tab khi đang thêm mới**: nếu nhảy sang tab Hôn phối/Ơn gọi/Hội đoàn mà chưa lưu
    thông tin cá nhân, hỏi có muốn lưu trước không — nếu đồng ý mà lưu thất bại thì **quay lại
    tab Cá nhân** và hủy việc chuyển tab (`tabControl1_SelectedIndexChanged`, dòng 1363-1471). Ba
    thông báo dùng chung khuôn:
    > "Phải lưu thông tin cá nhân trước khi nhập thông tin hôn phối/ơn gọi/hội đoàn. Bạn có muốn
    > lưu thông tin cá nhân không?" (thay từ tương ứng theo tab).
20. **Kiểm tra đủ tuổi kết hôn khi mở tab Hôn phối** (dù đã lưu rồi): nếu có Ngày sinh mà chưa đủ
    tuổi (`Memory.checkTuoiKetHon`), bật lại về tab Cá nhân (dòng 1459-1464).
21. Khi lưu mà `chkQuaDoi.Checked == false`: tự xóa `NgayQuaDoi`, `SoAnTang`, `NoiAnTang`,
    `NoiQuaDoi` (dòng 636-642) — đảm bảo dữ liệu qua đời không "sót lại" khi người dùng bỏ tick.
22. Khi tick/bỏ tick "Qua đời" lúc đang sửa, có đổi so với giá trị đã tải lúc mở form: hệ thống tự
    điều chỉnh trạng thái "có gia đình" của **người còn lại trong hôn phối cuối cùng** (dòng
    646-670) — logic khá ngầm, không hiển thị cho người dùng biết đang tự động sửa dữ liệu người
    khác.
23. Khi lưu xong (EDIT) và đang tick Qua đời, nếu có hôn phối đang hoạt động (`SELECT_HONPHOI_ACTIVE`):
    tự cập nhật `DaCoGiaDinh=0` cho người vợ/chồng còn lại (dòng 843-856) — tương tự bước 22 nhưng
    theo hướng khác (chạy **sau khi** lưu thành công, không phải trước).
24. Đổi Mã giáo dân khi sửa: nếu mã mới khác mã cũ, cascade update `MaGiaoDan` sang 3 bảng liên
    quan (`ThanhVienGiaDinh`, `BiTichChiTiet`, `ChuyenXu`) rồi **kết thúc lưu ngay, bỏ qua toàn bộ
    phần cập nhật dữ liệu còn lại của `ds`** (dòng 827-841, `goto exitHandler`) — nghĩa là nếu vừa
    đổi mã vừa đổi các trường khác trong cùng một lần lưu, **các trường khác sẽ không được lưu**
    trong lượt bấm Cập nhật đó (chỉ mã được đổi).

## 5. Thao tác người dùng

- **Cập nhật** (`gxCommand1.OKButton`, nhãn "&Cập nhật"): `btnOK_Click` (dòng 549-585) — lưu hôn
  phối nếu đang sửa dở (`gxHonPhoi1.IsChanging`), lưu hội đoàn nếu đang sửa dở
  (`gxHistoryHoiDoan1.IsChanging`), rồi `updateDataGiaoDan()`, rồi `gxTanHien1.UpdateData()`. Nếu
  bất kỳ bước nào thất bại, form **không đóng** và tự chuyển về đúng tab có lỗi.
- **Xem gia đình** (`gxCommand1.btn1`, nhãn "&Xem gia đinh" — thiếu dấu, giữ nguyên chính tả gốc):
  chỉ hiện khi đang Sửa (ẩn khi Thêm mới) — mở `GxGiaoDanList.showGiaDinh(maGiaoDan)` cùng logic
  1-gia-đình/nhiều-gia-đình như ở màn hình danh sách (`gxCommand1_Button1Click`, dòng 1542-1546).
- **Đóng**: đóng form, không lưu (không thấy xử lý cảnh báo "có thay đổi chưa lưu" khi Cancel).
- **Xem bản đồ** (icon `pictureBox1`): nếu Địa chỉ rỗng → "Xin vui lòng nhập địa chỉ để xem bản
  đồ" (Thông báo, dòng 1509); nếu có địa chỉ → `Memory.ViewMap(txtDiaChi.Text)`.
- **Chọn Tên Cha / Tên Mẹ qua picker** (`txtTenCha_OnSelected`/`txtTenMe_OnSelected`, dòng
  1265-1309): khi chọn một người làm Cha, nếu Tên Mẹ đang trống thì tự động điền Tên Mẹ (và
  ngược lại) dựa trên gia đình mà người được chọn đang là vợ/chồng, đồng thời tự điền Địa chỉ nếu
  Địa chỉ đang trống (tô nền vàng để báo hiệu tự điền — `txtDiaChi.TextBox.BackColor =
  Color.Yellow`, `getDiaChiGiaDinh`, dòng 1281-1293).

## 6. Lưới dữ liệu

Không áp dụng — đây là form nhập liệu 1 bản ghi, không có lưới riêng trong phạm vi tab Cá
nhân/Giáo lý. (Tab Hôn phối/Hội đoàn có thể chứa lưới con bên trong `GxHonPhoi`/
`GxHistoryHoiDoan` — không thuộc phạm vi 2 file được giao đọc kỹ trong buổi này.)

## 7. Liên kết sang màn hình khác

- "Xem gia đình" → `frmGiaDinh` hoặc `frmXemGiaDinhGiaoDan` (như mục 7 của
  `giao-dan-danh-sach.md`).
- Xem bản đồ → `Memory.ViewMap` (chưa rõ cài đặt, xem mục 9).
- Đóng → quay về màn hình đã mở nó (`frmGiaoDanList` hoặc `frmGiaDinh`) với `DataReturn` (bản ghi
  vừa lưu) để nơi gọi cập nhật lại dòng tương ứng trên lưới của nó.

## 8. Khác biệt cố ý ở bản web

Không có khác biệt "cố ý" nào được ghi nhận rõ ràng trong mã nguồn web hiện tại — phần lớn khác
biệt ở mục 10 là **thiếu sót chưa triển khai** chứ không phải quyết định thiết kế có chủ đích. Một
khác biệt hợp lý và nên giữ: bản web xử lý xung đột ghi đè bằng `RowVersion` (lạc quan, trả 409
khi xung đột) — bản desktop dùng Access đơn người dùng nên không cần cơ chế này.

## 9. Chỗ chưa chắc

- Giá trị thật của các hằng số `GxConstants.TUOI_RUOCLE`, `GxConstants.TUOI_CHO_PHEP_CO_CON` —
  chưa mở `GxConstants.cs` để tra đúng con số (chỉ tra `DangTimKiemGiaDinhGiaoDan` trong buổi
  này); thông báo lỗi ở quy tắc 15 (mục 4) ghi cứng "15 tuổi" trong văn bản nên khả năng cao là
  15, nhưng chưa xác nhận bằng dòng mã.
- Nội dung chính xác và thông báo lỗi của `Memory.checkTuoiKetHon`, `Memory.KiemTraTuoiKhongHopLe`
  — chưa đọc file định nghĩa các hàm này (thuộc lớp `Memory`, nằm ngoài 2 file được giao).
  Chỉ biết chữ ký và nơi gọi.
- Ý đồ gốc của quy tắc 8 (mục 4) — hai thông báo cảnh báo lệch Giáo họ/"không thống kê" — có
  đúng là lỗi soạn câu hay là chủ đích của tác giả gốc (có thể "Ngoài xứ" trong ngữ cảnh giáo xứ
  cụ thể có nghĩa khác với suy luận thông thường). Cần hỏi người dùng có kinh nghiệm vận hành
  phần mềm trước khi quyết định bản web có giữ đúng nguyên văn logic/câu chữ này không.
  Xem thêm: các gợi ý mẫu cho "Trình độ văn hóa" (Mẫu giáo, Cấp I, II, III...) đã bị comment tắt
  trong code (dòng 30-37) — chưa rõ vì sao (có thể đổi ý nhưng quên xóa hẳn), bản web hiện không
  có danh sách gợi ý nào cho trường này.
- `Memory.ViewMap(string)` — không rõ mở bằng cách nào (trình duyệt, ứng dụng cài sẵn, API bên
  ngoài).
- Nội dung SQL của `SqlConstants.SELECT_HONPHOI_ACTIVE`, `SELECT_CHECK_VOCHONG`,
  `SELECT_CHECK_GIAODAN_TONTAI`, `SELECT_GIAODAN_HONPHOI_WITH_ID` — dùng trong các quy tắc 16, 22,
  23 nhưng chưa đọc trực tiếp nội dung câu SQL, chỉ suy theo tên hằng số và ngữ cảnh dùng.
- Nội dung tab Hôn phối/Ơn gọi tận hiến/Hội đoàn ở mức UserControl (`GxHonPhoi.cs`,
  `GxTanHien.cs`, `GxHistoryHoiDoan.cs`) chưa được đọc trong buổi này — spec này chỉ mô tả cách
  `frmGiaoDan` gọi và phối hợp với chúng (nạp dữ liệu khi nào, lưu khi nào), không mô tả hành vi
  chi tiết bên trong 3 control đó.

## 10. Đối chiếu bản web hiện tại

Đã đọc: `WebApp/src/web/src/screens/GiaoDanDetail.tsx`, `GiaoDanDetailPage.tsx`,
`WebApp/src/Qlgx.Api/Services/GiaoDanService.cs`, `Endpoints/GiaoDanEndpoints.cs`,
`Dtos/GiaoDanDtos.cs`.

| Hành vi bản desktop | Bản web đã có? | Ghi chú |
|---|---|---|
| 5 tab đúng tên | **Có** | `GiaoDanDetail.tsx` dựng đúng 5 tab: Cá nhân, Giáo lý, Hôn phối, Ơn gọi tận hiến, Hội đoàn (`GxFormTabs`) |
| Các trường tab Cá nhân (Rửa tội/Rước lễ/Thêm sức/Xức dầu/Thông tin khác/Chuyển xứ) | Phần lớn có mặt trên UI và có `name` để submit | Đối chiếu với `CapNhatGiaoDanRequest` — khớp gần đủ các trường bí tích cơ bản |
| Thông tin liên hệ (Điện thoại, Email, Địa chỉ) | **Có nhập liệu, nhưng không nổi bật** | Ba trường này nằm lẫn trong khối "Thông tin khác" ở cuối tab (dòng 271-280 của `GiaoDanDetail.tsx`), và dòng tiêu đề tóm tắt đầu trang (`head-sub`, dòng 461-464) **không hiển thị điện thoại**, chỉ có mã GD/giáo họ/ngày sinh — đúng như người dùng đã phát hiện: thông tin liên hệ không được làm nổi bật dù dữ liệu có tồn tại trong form |
| Ảnh đại diện | **Chưa làm** | Chỉ có khung tĩnh "Chưa có hình / Nhấp để tải ảnh lên" (dòng 180-182), không có `<input type="file">`, không upload, không hiển thị ảnh đã có |
| Tên Cha/Tên Mẹ — picker chọn giáo dân có sẵn, tự đồng bộ 2 chiều, tự điền địa chỉ | **Chưa làm** | `GxPicker` chỉ hiển thị giá trị (`value={p.hoTenCha}`), không có ô nhập/chọn thật; giữ nguyên giá trị cũ khi lưu (comment ở đầu file xác nhận điều này) — mất hẳn liên động tự điền Tên Mẹ/Địa chỉ khi chọn Tên Cha |
| Người ban bí tích (rửa tội/rước lễ/thêm sức/xức dầu) — picker chọn linh mục | **Chưa làm** | Cùng tình trạng `GxPicker` chỉ hiển thị, không chỉnh sửa được |
| Validate: 24 quy tắc liệt kê ở mục 4 | **Một phần — đã thêm ở phiên "ghi giáo dân"** | `GiaoDanService.KiemTraNghiepVu` (dùng chung cho `Tao`/`CapNhat`) nay kiểm tra: bắt buộc Họ tên/Giới tính/Ngày sinh (rule 4-6); tuổi kết hôn khi tick "Có gia đình" (rule 13, chặn cứng <14 tuổi, cảnh báo 14-17); Ngày sinh vs Ngày rửa tội bug-for-bug (rule 9); tuổi rước lễ (rule 10); rửa tội chưa có Tên thánh (rule 12); trùng Họ tên+Tên thánh+Ngày sinh (rule 16); chặn đổi giới tính khi đang là vợ/chồng (rule 17, `CapNhat` riêng). **Vẫn chưa làm được**: rule 8 (giáo họ/không thống kê — cần Giáo họ chọn thật), rule 11 (trùng ngày chuyển xứ), rule 15 (tuổi cha/mẹ — cần picker Tên Cha/Mẹ thật), rule 18 (chặn bỏ tick có-gia-đình), rule 22/23 (tự sửa DaCoGiaDinh người còn lại) — xem `can-review-sau.md` mục 19. Client vẫn không có validate HTML (`required`) — chỉ dựa vào máy chủ |
| Liên động Qua đời ⇄ Còn học (loại trừ lẫn nhau) | **Có** | `doiQuaDoi`/`doiConHoc` trong `GiaoDanDetail.tsx:114-115`, đúng tinh thần 2 chiều của desktop |
| Liên động chọn "Ngoài xứ" ẩn/hiện Giáo xứ, Giáo phận, khối Chuyển xứ | **Có** | `ngoaiXu` state điều khiển hiển thị (dòng 118, 160-166, 183-195) |
| Liên động tick "không thống kê" → tự chọn Ngoài xứ | **Có** | `doiGiaoDanAo` (dòng 116) |
| Thêm giáo dân mới | **Đã làm** | `POST /api/giao-dan` (`GiaoDanService.Tao`, mã cũ sinh qua `SinhMaService`, `MaNhanDang` mới sinh). Nút "Thêm giáo dân" ở `GiaoDanDetail.tsx` nay hoạt động thật, đi qua cùng `KiemTraNghiepVu`; còn cảnh báo chưa xác nhận thì `GiaoDanDetailPage` gộp lại thành một `window.confirm` — xem `can-review-sau.md` mục 20 |
| Tab Giáo lý (Bao đồng 1/2, Vào đời, Giáo lý hôn nhân) | **Chỉ hiển thị UI tĩnh, không đọc/không ghi dữ liệu thật** | Toàn bộ input trong `tabGiaoLy` không có `defaultValue` từ `p.*` và không có `name` — không đọc dữ liệu đã lưu (`NgayBD1`, `NoiBD1`...) lên form dù `GiaoDanDetailDto`/`GiaoDanService.LayChiTiet` **đã có** các trường này (dòng 108-109 của `GiaoDanService.cs`); và `CapNhatGiaoDanRequest` **không có** các trường này nên dù có sửa cũng không lưu được |
| Tab Hôn phối | **Chỉ là khung UI tĩnh** | Không gọi API hôn phối nào, không đọc/ghi `HonPhoi`; các ô Nơi hôn phối/Linh mục chứng không có `name` |
| Tab Ơn gọi tận hiến | **Chỉ là khung UI tĩnh** | Tương tự — không có dữ liệu, không có API |
| Tab Hội đoàn | **Chỉ là khung UI tĩnh, chỉ có form "Thêm vào hội đoàn"** | Không hiển thị lịch sử hội đoàn đã có (khác hẳn desktop tự nạp `gxHistoryHoiDoan1.loaddata(id)` khi mở form Sửa), không có API lưu |
| Nút "In lý lịch cá nhân" | **Có nút, không hoạt động** | `<button type="button" className="btn">In lý lịch cá nhân</button>` không gắn `onClick` (dòng 487 `GiaoDanDetail.tsx`) |
| Nút "Xem gia đình" | **Có, hoạt động đúng nếu có `giaDinhId`** | `disabled={!p.giaDinhId}` khớp tinh thần "chỉ hiện khi đang Sửa và có gia đình" của desktop, dù cơ chế khác (desktop ẩn nút khi Thêm mới, web disable khi không có gia đình) |
| Xem bản đồ theo địa chỉ | **Thiếu** | Không thấy nút/hành động nào trong `GiaoDanDetail.tsx` |
| Xử lý xung đột khi 2 người cùng sửa | **Có, và tốt hơn desktop** | `RowVersion` + `LoiXungDot` (409) — desktop không có cơ chế này (Access, gần như luôn ghi đè) |

### Ưu tiên khắc phục (ảnh hưởng tới việc bỏ hẳn bản desktop)

- **Cao — chặn hoàn toàn việc bỏ bản desktop:**
  1. ~~Không tạo được giáo dân mới qua web.~~ **Đã làm** ở phiên "ghi giáo dân" —
     `POST /api/giao-dan`.
  2. **Tab Hôn phối, Ơn gọi tận hiến, Hội đoàn không đọc/không ghi dữ liệu thật** — đây là 3
     mảng nghiệp vụ lớn (tình trạng hôn nhân, ơn gọi tu trì, sinh hoạt hội đoàn) hoàn toàn không
     dùng được trên web. (Đã có API đọc/ghi từ Task 15/16 khác — xem `hon-phoi.md`/`tan-hien.md`/
     `hoi-doan.md`; dòng này của bảng đối chiếu ở trên có thể đã lỗi thời, chưa xác nhận lại
     trong phiên này.)
  3. **Tab Giáo lý không đọc/không ghi được** dù dữ liệu đã có sẵn trong CSDL và trong DTO đọc —
     chỉ thiếu phần ghi và phần hiển thị giá trị đã lưu. (Ngoài phạm vi phiên "ghi giáo dân".)
  4. ~~Không có validate nghiệp vụ nào phía server~~ — **đã thêm một phần** (xem hàng "Validate:
     24 quy tắc" ở trên và `can-review-sau.md` mục 19 cho các quy tắc còn thiếu).
- **Trung bình:**
  5. Không có picker chọn thật cho Tên Cha/Tên Mẹ/Người ban bí tích — mất khả năng liên kết
     đúng với hồ sơ giáo dân khác, dễ gõ sai chính tả so với desktop.
  6. Chưa upload/hiển thị được ảnh đại diện.
  7. Không có nút in lý lịch cá nhân, xem bản đồ hoạt động.
  8. Thông tin liên hệ (điện thoại) không hiển thị ở phần tóm tắt đầu trang — đúng như người
     dùng đã phát hiện.
- **Thấp:**
  9. Gợi ý combo cố định (nghề nghiệp, tình trạng xức dầu, xếp loại GLHN) của desktop chưa được
     đưa vào web dưới dạng danh sách chọn nhanh (hiện là ô nhập tự do hoặc thiếu hẳn).
