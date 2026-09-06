# Màn hình: Hôn phối (`frmHonPhoi.cs`) + khối hôn phối trong gia đình (`GxHonPhoiGiaDinh.cs`)

| | |
|---|---|
| Tệp nguồn | `Source/GXControl/frmHonPhoi.cs` (422 dòng) + `Source/GXControl/frmHonPhoi.Designer.cs` (348 dòng); `Source/GXControl/GxHonPhoiGiaDinh.cs` (300 dòng) + `.Designer.cs`; `Source/GXControl/GxCachThucHonPhoi.cs` (combo "Tình trạng hôn phối" dùng riêng trong `GxHonPhoiGiaDinh`) |
| UserControl dùng lại | `GxGiaoDan` (picker Người nam/Người nữ, chỉ trong `frmHonPhoi`), `GxLinhMuc` (Linh mục chứng), `GxComboField`/`GxCachThucHonPhoi` (combo Tình trạng/Cách thức hôn phối), `GxDateField`, `GxTextField`, `GxCommand`, `GxGroupBox` |
| Bảng dữ liệu đụng tới | `HonPhoi` (12 cột), `GiaoDanHonPhoi` (bảng nối, 3 cột: `MaHonPhoi`, `MaGiaoDan`, `SoThuTu`) |
| Trạng thái migrate | đang → xem mục 10. Bản web đã có tầng dữ liệu đầy đủ (từ Task 7/spec `gia-dinh-chi-tiet.md`), Task 15 thêm endpoint theo giáo dân + UI tab "Hôn phối" ở `GiaoDanDetail.tsx` |

## 0. Có HAI nơi nhập hôn phối trong bản desktop, hành vi khác nhau

- **`frmHonPhoi.cs`** — form độc lập, có picker chọn Người nam/Người nữ (`txtNguoiChong`, `txtNguoiVo` kiểu `GxGiaoDan`), có Mã hôn phối, có Tên đôi hôn phối, có nút "In chứng nhận hôn phối". Chưa tìm thấy nơi nào trong mã đã đọc thực sự MỞ form này (không thấy `new frmHonPhoi()` trong `frmGiaDinh.cs` hay `frmGiaoDan.cs`) — có thể được gọi từ một màn hình danh sách hôn phối riêng (`GxHonPhoiList.cs`) chưa đọc trong nhiệm vụ này. Xem mục 9.
- **`GxHonPhoiGiaDinh.cs`** — `UserControl` nhúng thẳng trong `frmGiaDinh.cs` (biến `gxXemHonPhoi1`), **không có** picker chọn người: Người nam/Người nữ được gán từ ngoài vào (`NguoiChong`/`NguoiVo` = mã giáo dân đang chọn ở `txtNguoiChong`/`txtNguoiVo` của `frmGiaDinh`) mỗi khi người dùng đổi vợ/chồng ở form gia đình (`loadHonPhoi()`, `frmGiaDinh.cs:921-929`). Đây là nơi **thực sự đang chạy** khi người dùng sửa hôn phối từ màn hình gia đình — bản web migrate theo control này là chính xác vì backend (`GiaDinhService`) cũng được viết theo đúng luồng "hôn phối gắn theo cặp vợ chồng hiện có của gia đình", không có luồng "tạo hôn phối rồi chọn người" độc lập.

**Kết luận dùng cho Task 15:** tab "Hôn phối" trong màn hình chi tiết giáo dân migrate theo **hành vi của `GxHonPhoiGiaDinh`** (xem/sửa hôn phối đã gắn sẵn với người này), KHÔNG theo `frmHonPhoi` (chọn/đổi vợ chồng từ đầu) — lý do: tầng dữ liệu web (`GiaoDanHonPhoi`) chỉ có 1043 liên kết đã được chuyển sẵn từ Access, người dùng cần xem/sửa các bản ghi này trước; luồng "tạo hôn phối mới, chọn vợ/chồng" đòi hỏi toàn bộ cây kiểm tra nghiệp vụ phức tạp của `frmHonPhoi`/`checkNguoiNamNguoiNuTrongGiaDinhKhac` (tuổi kết hôn, trùng gia đình khác…) — xem mục 8, để dành cho một task sau.

## 1. Mục đích

Ghi lại thông tin một cuộc hôn phối (đám cưới theo nghi thức Công giáo) của một cặp vợ chồng: ngày, nơi, linh mục chứng, người làm chứng, tình trạng/cách thức hôn phối, ghi chú. Một giáo dân có thể có **nhiều** bản ghi hôn phối theo thời gian (goá rồi tái hôn — xem chú thích `ChonHonPhoiHienTai` trong `GiaDinhService.cs`, cũng như comment gốc của `SELECT_HONPHOI_THEO_MAGIAODAN` phía Access).

`frmHonPhoi` mở như hộp thoại độc lập (chưa xác định điểm gọi — mục 9); `GxHonPhoiGiaDinh` luôn hiện trong tab/khối "hôn phối" của form gia đình.

## 2. Bố cục và các trường

| Nhãn hiển thị | Control (`frmHonPhoi`) | Control (`GxHonPhoiGiaDinh`) | Cột CSDL | Bắt buộc? | Mặc định | Ghi chú |
|---|---|---|---|---|---|---|
| Mã hôn phối | `txtMaHonPhoi` (`ReadOnly`, `EditEnabled=false`, Designer dòng 259) | — (không có trong khối nhúng) | `HonPhoi.MaHonPhoi` | tự sinh | `Memory.Instance.GetNextId(...)` khi Thêm mới (`frmHonPhoi.cs:85`) | |
| Người nam | `txtNguoiChong` (`GxGiaoDan`, lọc `Phai="nam"`, `frmHonPhoi.cs:62`) | — (gán từ ngoài, không picker) | qua `GiaoDanHonPhoi` | có (để lưu) | | `ReadOnly=true` — chỉ chọn qua picker |
| Người nữ | `txtNguoiVo` (lọc `Phai="nu"`, dòng 63) | — (gán từ ngoài) | qua `GiaoDanHonPhoi` | có | | `ReadOnly=true` |
| Đôi hôn phối (tên) | `txtTenHonPhoi` | — (không có ô, gán thẳng property `TenHonPhoi` từ `frmGiaDinh.loadHonPhoi()`) | `HonPhoi.TenHonPhoi` | có (chỉ ở `frmHonPhoi`) | ghép "Tên nam - Tên nữ" khi chọn xong (dòng 107,115) | `Trim` bỏ `-` thừa khi lưu (dòng 238) |
| Số hôn phối | `txtSoHonPhoi` | `txtSoHonPhoi` | `HonPhoi.SoHonPhoi` | không | | Rỗng → lưu `DBNull` (dòng 241/150) |
| Ngày hôn phối | `dtNgayHonPhoi` | `dtNgayHonPhoi` | `HonPhoi.NgayHonPhoi` | không | `IsNullDate=true` cho phép để trống (dòng 61/21); Designer có giá trị mẫu "06/04/2009" (dòng 124) — chỉ là giá trị Designer-time, không có ý nghĩa nghiệp vụ | |
| Nơi hôn phối | `txtNoiHonPhoi` | `txtNoiHonPhoi` | `HonPhoi.NoiHonPhoi` | không | | |
| Linh mục chứng | `txtLinhMuc` (`GxLinhMuc`) | `txtLinhMuc` (`GxLinhMuc`) | `HonPhoi.LinhMucChung` | không | | Picker chọn linh mục từ danh mục, chưa đọc sâu control này |
| Người chứng 1 | `txtNguoiChung1` | `txtNguoiChung1` | `HonPhoi.NguoiChung1` | không | | Ô nhập tay tự do, không phải picker |
| Người chứng 2 | `txtNguoiChung2` | `txtNguoiChung2` | `HonPhoi.NguoiChung2` | không | | Ô nhập tay tự do |
| Tình trạng/Cách thức hôn phối | `cbCachThuc` (`GxComboField`, nhãn "Tình trạng hôn phối") | `cbCachThuc` (`GxCachThucHonPhoi`, nhãn "Tình trạng hôn phối") | `HonPhoi.CachThucHonPhoi` | không | item đầu = "" (rỗng), `SelectedIndex=0` khi Thêm mới | **Danh sách KHÁC NHAU giữa hai nơi — xem mục 4** |
| Ghi chú | `txtGhiChu` | `txtGhiChu` (nhãn "Ghi chú hôn phối") | `HonPhoi.GhiChu` | không | | |

Cột `HonPhoi.MaNhanDang` không có ô nhập — chỉ được gán tự động khi trống (`Memory.GetGiaDinhKey`, dòng 258/168), không migrate (bản web đã có quy tắc tương đương ở `GiaDinhService`, "không đụng MaNhanDang" — xem `gia-dinh-chi-tiet.md`).

## 3. Hành vi khi tải

- `frmHonPhoi`: khi Thêm mới, sinh `id` kế tiếp ngay lúc `Load`, đặt `cbCachThuc.SelectedIndex=0`, ẩn nút "In chứng nhận hôn phối" (dòng 83-88). Khi Sửa, hiện nút In; nếu `Operation==VIEW` ẩn cả nút Cập nhật lẫn nút In (dòng 92-96).
- `frmHonPhoi.GetHonPhoi(maGiaoDan)`: tra `GiaoDanHonPhoi` theo **MỘT** giáo dân (`WHERE MaGiaoDan=?`), lấy dòng ĐẦU TIÊN tìm được rồi gọi `GetData(MaHonPhoi)` — **không lọc theo cặp/không sắp xếp theo ngày** — nếu người này có nhiều hôn phối, hàm này lấy bừa bản ghi đầu tiên trả về từ CSDL (thứ tự không xác định). Đây là hành vi **kỳ quặc/tiềm ẩn lỗi** của bản gốc: không giống `GiaDinhService.ChonHonPhoiHienTai` (đã cố tình sắp `OrderByDescending(NgayHonPhoi)`), hàm desktop này không có logic chọn "hiện tại". Ghi vào `can-review-sau.md`.
- `GxHonPhoiGiaDinh.GetHonPhoi(maNguoiVo, maNguoiChong)`: tra theo **CẶP** (dùng `SELECT_HONPHOI_THEO_MAGIAODAN` cộng điều kiện `AND GiaoDanHonPhoi_1.MaGiaoDan = ?` — join hai lần bảng nối để khớp đúng cả hai người cùng một `MaHonPhoi`) — đây là cách tiếp cận đúng, khớp với `ChonHonPhoiHienTai` bên web (chọn theo cặp trước, occasion "chỉ một bên" mới lấy theo người). `frmGiaDinh.loadHonPhoi()` chỉ gọi hàm này khi **cả hai** người đã được chọn (`MaGiaoDan>0` VÀ `>-1`, dòng 923); nếu chưa đủ cặp thì chỉ gán `NguoiChong`/`NguoiVo` vào control, không tải dữ liệu hôn phối nào (dòng 930-932).
- Nếu `GetHonPhoi` không tìm thấy → gọi `ClearControlData()` và đặt `operation = GxOperation.ADD` (`GxHonPhoiGiaDinh.cs:102-104`).

## 4. Quy tắc nghiệp vụ và kiểm tra dữ liệu

### Chọn Người nam/Người nữ (chỉ `frmHonPhoi`, không áp dụng cho `GxHonPhoiGiaDinh` — control này không có picker)

- Chọn cho ô Người nữ mà giới tính Nam → chặn: **"Người vợ không thể là nam!"** (`frmHonPhoi.cs:124`).
- Chọn cho ô Người nam mà giới tính Nữ → chặn: **"Người chồng không thể là nữ!"** (dòng 168).
- Kiểm tra tuổi kết hôn qua `Memory.KiemTraTuoiKhongHopLe(NgaySinh, hômNay, tuổi)` với `tuổi = GxConstants.TUOI_HON_PHOI_NAM`/`TUOI_HON_PHOI_NU` (giá trị hằng số chưa đọc, xem mục 9). Không hợp lệ → hỏi xác nhận nguyên văn (đã giữ nguyên cả lỗi chính tả/khoảng trắng thừa trong mã gốc):
  > "Giáo dân được chọn chưa đến tuổi được phép kết hôn (vì nhỏ hơn {0} tuồi.\r\nB ạn có chắc chọn giáo dân này không?" (dòng 136-137, 180-181) — chọn **No** thì huỷ chọn.
  >
  > **Lưu ý lỗi chính tả trong bản gốc:** "tuồi" (đúng ra "tuổi"), thiếu dấu đóng ngoặc `)`, và "B ạn" có khoảng trắng thừa (đúng ra "Bạn"). Chép nguyên văn theo yêu cầu migrate y hệt — xem `can-review-sau.md`.
- Kiểm tra đã có vợ/chồng khác qua `Memory.KiemTraVoChong(mã)` — trả `0` thì huỷ chọn, không có thông báo riêng ở `frmHonPhoi` (hàm dùng chung tự hiện thông báo, nội dung chưa đọc — mục 9). Điều kiện gọi: chỉ kiểm tra khi người vừa chọn KHÁC người đang có trong ô đó hiện tại (`(int)row[...] != txtNguoiVo.MaGiaoDan`, dòng 144/187) — tức đổi sang người khác mới kiểm tra, giữ nguyên người cũ thì bỏ qua.
- Có đoạn code kiểm tra "đã qua đời/đã chuyển xứ thì tô đỏ + gạch ngang tên" nhưng bị **comment hoàn toàn** ở cả hai chỗ (dòng 149-159, 192-202) — tính năng này KHÔNG chạy trong bản build hiện tại. Ghi vào `can-review-sau.md`.

### Kiểm tra khi lưu — `frmHonPhoi.checkInput()` (dòng 205-229)

Thứ tự kiểm tra, dừng ở lỗi đầu tiên:
1. Đang Sửa (không phải Thêm mới) và `txtMaHonPhoi` không phải số → **"Mã hôn phối phải được nhập số"** (dòng 209).
2. Người nam HOẶC Người nữ trống → **"Hãy nhập đầy đủ người nam và người nữ!"** (dòng 215).
3. Tên đôi hôn phối trống → **"Hãy nhập tên đôi hôn phối!"** (dòng 223).

### Kiểm tra khi lưu — `GxHonPhoiGiaDinh.checkInput()` (dòng 271-298) — áp dụng cho khối nhúng trong form gia đình

- Nếu **TẤT CẢ** các trường (Số hôn phối, Ngày, Nơi, Linh mục, Người chứng 1, Người chứng 2, Cách thức, Ghi chú) đều trống/rỗng:
  - Nếu đang **Sửa** (`operation==EDIT`, tức đã từng có bản ghi hôn phối) → báo lỗi: **"Hãy nhập ít nhất một thông tin hôn phối"** (dòng 285) và không cho lưu.
  - Nếu đang **Thêm mới** (`operation==ADD`, chưa từng có hôn phối) → **không báo lỗi gì, chỉ âm thầm trả `false`** (không vào nhánh `else`) — tức bấm Cập nhật ở form gia đình khi chưa nhập gì cho hôn phối sẽ **không lưu hôn phối** mà cũng không báo cho người dùng biết vì sao. **Đây là hành vi kỳ quặc** (im lặng bỏ qua thay vì báo lỗi hoặc coi là "không có gì để lưu, OK"). Ghi vào `can-review-sau.md`.
- Ngược lại (có ít nhất 1 trường được nhập) mà `NguoiChong<1` hoặc `NguoiVo<1` (chưa đủ cặp vợ chồng) → **"Không thể lưu thông tin hôn phối vì không đủ thông tin người nam và người nữ!"** (dòng 293).

### Lưu (`UpdateHonPhoi`, `GxHonPhoiGiaDinh.cs:173-269`)

- Tìm hôn phối đã tồn tại cho đúng cặp qua `GxHonPhoi.HonPhoiExists(NguoiChong, NguoiVo)` (hàm tĩnh, chưa đọc riêng — mục 9). Có thì `operation=EDIT`, lấy `MaHonPhoi` từ đó.
- Không có thì `operation=ADD`, sinh mã mới qua `Memory.Instance.GetNextId(..., false)` (đối chiếu: `frmHonPhoi.AssignDataSource` cũng gọi `GetNextId` nhưng với tham số `true` — **khác `false`/`true` ở tham số thứ 3, ý nghĩa chưa đọc, có thể là "có cần tăng bộ đếm ngay hay không" — mục 9**).
- Khi Sửa: xoá sạch mọi dòng `GiaoDanHonPhoi` của `MaHonPhoi` đó rồi chèn lại đúng 2 dòng (vợ, chồng) — giống hệt cách `frmHonPhoi.updateHonPhoi()` làm (dòng 326-343) và cách `GiaDinhService.GhiHonPhoi` xoá/tạo lại quan hệ khi có sẵn hôn phối.
- `SoThuTu` của mỗi dòng `GiaoDanHonPhoi` lấy từ `GxHonPhoi.GetNextSoThuTu(mãGiáoDân)` (đếm số hôn phối trước đó của CHÍNH người đó cộng 1 — hàm tĩnh, chưa đọc sâu, suy từ tên — mục 9) — **khác** cách `frmHonPhoi.updateHonPhoi()` làm: ở đó `SoThuTu` **không được gán gì cả** (dòng 334-343 không có dòng gán `GiaoDanHonPhoiConst.SoThuTu`), nghĩa là cột này nhận giá trị mặc định của DataRow (có thể là 0 hoặc DBNull tuỳ định nghĩa bảng) khi lưu qua `frmHonPhoi`. Một quy tắc dữ liệu (số thứ tự hôn phối của một người) được cài đặt **khác nhau ở hai nơi lưu cùng bảng** — ghi vào `can-review-sau.md`.
- Lỗi ngoại lệ khi lưu → `MessageBox` tiêu đề **"Lỗi Exception (frmHonPhoi, gxCommand1_OnOK)"** / **"Lỗi Exception (GxHonPhoiGiaDinh, UpdateHonPhoi)"** kèm `ex.Message` (dòng 358/266) — bẫy lỗi hệ thống, không phải thông báo nghiệp vụ.

### Khi bấm Hủy (chỉ `frmHonPhoi`, `gxCommand1_OnCancel`, dòng 363-382)

Chỉ hỏi khi đang **Thêm mới** VÀ đã chọn ít nhất Người nam hoặc Người nữ:
> "Bạn có muốn lưu đôi hôn phối này không?\r\nChọn [Yes] để lưu và đóng màn hình.\r\nChọn [No] để đóng màn hình và không lưu.\r\nChọn [Cancel] để quay trở lại màn hình này và xem lại."

Chọn Yes → gọi thẳng `gxCommand1_OnOK` (chạy lại toàn bộ `checkInput`, có thể lỗi và không đóng được form dù người dùng đã chọn Yes ở hộp Hủy — không có xử lý ngoại lệ đặc biệt cho trường hợp này). Chọn No → đóng, `DialogResult=Cancel`. Chọn Cancel (hoặc đóng hộp thoại) → `DialogResult=None`, ở lại form.

## 5. Thao tác người dùng

| Thao tác | Điều kiện bật/tắt | Hành động |
|---|---|---|
| Nút "Cập nhật" (`frmHonPhoi`) | Ẩn khi `Operation==VIEW` (dòng 94) | `updateHonPhoi()`, đóng form nếu thành công |
| Nút "In chứng nhận hôn phối" (`frmHonPhoi.Button1`) | Ẩn khi Thêm mới (dòng 87), hiện lại khi Sửa | Gọi `GxGiaDinhList.ChungNhanHonPhoi(MaHonPhoi, false)` — xuất tài liệu ngoài, không đọc sâu |
| Đổi Người nam/Người nữ (`frmHonPhoi`) | | Ghép lại `txtTenHonPhoi` = "Tên nam - Tên nữ" (dòng 107,115) |
| `GxHonPhoiGiaDinh` không có nút riêng | | Được lưu gộp cùng lúc với `frmGiaDinh` khi bấm "Cập nhật" ở form gia đình (`gxXemHonPhoi1.UpdateHonPhoi()` gọi từ `frmGiaDinh.cs:1706`, xem `gia-dinh-chi-tiet.md`) |

## 6. Lưới dữ liệu

Không có — cả hai đều là form/khối nhập liệu một bản ghi, không có lưới.

## 7. Liên kết sang màn hình khác

- `frmHonPhoi` mở picker chọn giáo dân qua `GxGiaoDan` (control tự quản lý popup chọn, không phải mở form riêng biệt theo tên).
- Gọi `GxGiaDinhList.ChungNhanHonPhoi` (in chứng nhận, export ngoài).
- Trả `DialogResult.OK` + `dataReturn` (DataRow hôn phối) cho nơi gọi.
- `GxHonPhoiGiaDinh` không tự mở màn hình nào — chỉ được `frmGiaDinh` gọi các phương thức public (`GetHonPhoi`, `UpdateHonPhoi`, `ClearControlData`, gán `NguoiChong`/`NguoiVo`/`TenHonPhoi`).

## 8. Khác biệt cố ý ở bản web

- **Không migrate luồng chọn/đổi Người nam-Người nữ của `frmHonPhoi`** (mục 0) — bản web chỉ cho xem/sửa hôn phối đã có sẵn liên kết (`GiaoDanHonPhoi`) với giáo dân đang xem, giống hành vi `GxHonPhoiGiaDinh`. Lý do: dữ liệu thật đã có 1043 liên kết cần xem/sửa ngay; luồng tạo mới với picker đầy đủ kiểm tra nghiệp vụ (giới tính, tuổi, trùng gia đình khác...) là một khối lượng công việc riêng, chưa làm ở Task 15 — xem "Ưu tiên thiếu sót" trong `gia-dinh-chi-tiet.md` mục 10 (đã liệt kê là thiếu sót ưu tiên **Cao** từ trước, không phải bỏ sót mới).
- **Danh sách "Cách thức/Tình trạng hôn phối" dùng bản 9 giá trị của `GxCachThucHonPhoi`** (rỗng, Hợp pháp, Hợp thức hóa, Chuẩn, Không theo phép đạo, Ly thân, Ly dị, Đã được tháo gỡ, Không xác định) thay vì bản 6 giá trị cũ hơn của `frmHonPhoi` (thiếu Ly thân/Ly dị/Đã được tháo gỡ) — vì màn hình web migrate theo luồng của `GxHonPhoiGiaDinh` (mục 0), và danh sách 9 giá trị đầy đủ hơn, phù hợp hơn với dữliệu thật (có thể có hôn phối đã ly dị/ly thân). Ghi rõ đây là lựa chọn có ý thức, không phải sai sót — xem `can-review-sau.md` để người dùng xác nhận lại.
- **Một giáo dân hiển thị TẤT CẢ các bản ghi hôn phối của mình** (danh sách), không chỉ một bản ghi như cả hai control desktop — vì dữ liệu thật có trường hợp goá/tái hôn (theo đúng chú thích đã có sẵn trong `GiaDinhService.cs`). Đây là cải tiến có chủ đích so với bản desktop (cả `frmHonPhoi.GetHonPhoi(maGiaoDan)` lẫn `GxHonPhoiGiaDinh` đều chỉ xử lý một hôn phối tại một thời điểm).
- **RowVersion chống ghi đè** cho từng bản ghi hôn phối khi sửa qua tab giáo dân — bản desktop không có cơ chế này (giống nhận xét đã ghi ở `gia-dinh-chi-tiet.md` mục 10).
- Không migrate nút "In chứng nhận hôn phối" ở Task 15 (chưa có tính năng in ở web nói chung).

## 9. Chỗ chưa chắc

- Điểm gọi mở `frmHonPhoi` (form độc lập) chưa xác định trong phạm vi đã đọc — nghi ngờ nằm ở `GxHonPhoiList.cs`/`frmRaoHonPhoi.cs` (chưa đọc).
- Giá trị cụ thể của hằng số `GxConstants.TUOI_HON_PHOI_NAM`/`TUOI_HON_PHOI_NU` (tuổi tối thiểu kết hôn) — nằm ngoài phạm vi 2 file được giao.
- Nội dung thông báo bên trong `Memory.KiemTraVoChong` — dùng chung, chưa đọc.
- Ý nghĩa tham số thứ 3 (`true`/`false`) của `Memory.Instance.GetNextId(bảng, cột, ?)` khi sinh mã hôn phối mới — khác nhau giữa `frmHonPhoi` (`true`) và `GxHonPhoiGiaDinh` (`false`).
- Cài đặt cụ thể của `GxHonPhoi.HonPhoiExists` và `GxHonPhoi.GetNextSoThuTu` (lớp `GxHonPhoi`, không phải `GxHonPhoiGiaDinh`) — chỉ thấy được lời gọi, chưa đọc định nghĩa lớp `GxHonPhoi.cs`.
- Vì sao `frmHonPhoi.updateHonPhoi()` không gán `SoThuTu` khi tạo dòng `GiaoDanHonPhoi` trong khi `GxHonPhoiGiaDinh.UpdateHonPhoi()` có gán — có thể `frmHonPhoi` đã cũ/không dùng nữa (khớp với việc chưa tìm thấy nơi gọi nó — mục 9 dòng đầu), auto tăng dần theo desktop cũ.

## 10. Đối chiếu bản web hiện tại (sau khi migrate ở Task 15)

| Hành vi bản desktop | Bản web đã có? | Ghi chú |
|---|---|---|
| Xem hôn phối gắn với một giáo dân (`GxHonPhoiGiaDinh.GetHonPhoi` theo cặp / `frmHonPhoi.GetHonPhoi` theo một người) | **Có, mở rộng hơn** | `GET /api/giao-dan/{id}/hon-phoi` trả DANH SÁCH mọi hôn phối của người này (không chỉ 1 bản ghi) — xem mục 8 |
| Sửa các trường Số/Ngày/Nơi/Linh mục/Người chứng 1-2/Cách thức/Ghi chú | **Có** | `PUT /api/giao-dan/hon-phoi/{honPhoiId}`, dùng lại `CapNhatHonPhoiRequest` đã có sẵn từ Task 7, có kiểm tra `RowVersion` |
| Chọn/đổi Người nam, Người nữ (picker + toàn bộ kiểm tra giới tính/tuổi/trùng gia đình) | **Thiếu** (cố ý, xem mục 8) | Tab hiển thị tên vợ/chồng hiện có (đọc từ `GiaoDanHonPhoi`), không cho đổi người |
| Tạo hôn phối hoàn toàn mới cho một giáo dân chưa có hôn phối nào | **Thiếu** | Cần picker chọn người kia trước — nằm cùng nhóm thiếu sót với "Chọn/đổi Người nam/Người nữ" ở form gia đình (`gia-dinh-chi-tiet.md` mục 10, ưu tiên Cao) |
| In chứng nhận hôn phối | **Thiếu** | Chưa có tính năng in ở web |
| Validate "phải nhập đủ người nam/nữ", "tên đôi hôn phối" (`frmHonPhoi.checkInput`) | **Không áp dụng** | Không migrate vì không có luồng tạo mới qua picker |
| Validate "phải nhập ít nhất 1 thông tin khi Sửa" (`GxHonPhoiGiaDinh.checkInput`) | **Chưa làm** | Không thêm ở Task 15 để nhất quán với `GiaDinhService.GhiHonPhoi` (endpoint gia đình cũng KHÔNG có validate này) — xem `can-review-sau.md` |
