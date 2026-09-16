# Màn hình: Quản lý tài khoản (`frmAccoutList.cs`)

| | |
|---|---|
| Tệp nguồn | `Source/ChuongTrinh/frmAccoutList.cs` (440 dòng) |
| UserControl dùng lại | `gxListAccout1` (lưới tài khoản), `gxAddEdit` (thanh nút Thêm/Sửa/Xoá/Lưu/Thôi/Tải lại — không đọc riêng, hành vi suy ra từ cách form gọi), `cbPhanQuyen` (combo loại tài khoản) |
| Bảng dữ liệu đụng tới | `TaiKhoan`, `TenLoaiTaiKhoan` (chỉ đọc, nạp combo) |
| Trạng thái migrate | xong — `WebApp/src/web/src/screens/TaiKhoanListPage.tsx` |

## 1. Mục đích

Quản trị tài khoản đăng nhập của giáo xứ: xem danh sách, thêm/sửa/xoá tài khoản, gán loại
tài khoản (quyền). Mở từ menu `frmMain` (mục "Hệ thống" → mã `itListAccount`,
`frmMain.cs:377-379`).

## 2. Bố cục và các trường

| Nhãn | Cột CSDL | Kiểu | Bắt buộc | Mặc định | Ghi chú |
|---|---|---|---|---|---|
| Họ tên người dùng | `HoTenNguoiDung` | text | có (`checkInput`, dòng 133) | — | |
| Tên đăng nhập | `TenTaiKhoan` | text | có (dòng 140), khớp `^[a-zA-Z0-9]+$` (dòng 146-152) | — | không sửa được khi Sửa (`txtUserName.ReadOnly = true`, dòng 341) |
| Mật khẩu | `MatKhau` (Access) → **không migrate**, xem `MatKhauBam` | password | bắt buộc khi Thêm (dòng 161-166); khi Sửa để trống = không đổi (dòng 267-274) | — | ô nhập che ký tự (`PasswordChar='*'`, dòng 387) |
| Nhập lại mật khẩu | không lưu | password | phải khớp Mật khẩu khi có nhập (dòng 168-176) | — | |
| Email | `Email` | text | không | — | |
| Số điện thoại | `SoDienThoai` | text | không | — | |
| Loại tài khoản | `LoaiTaiKhoan` | combo (index = giá trị số) | có (dòng 177-182) | — | nạp từ `TenLoaiTaiKhoan` (0=Quản trị viên, 1=Người nhập 1, 2=Người nhập 2) |
| Câu hỏi bí mật | `CauHoiGoiY` | text | chỉ bắt buộc khi loại = Quản trị viên (dòng 183-190, 412-421) | — | **xem mục 8 — không mang sang bản web** |
| Câu trả lời bí mật | `CauTraLoiGoiY` | text | chỉ bắt buộc khi loại = Quản trị viên (dòng 191-196) | — | **xem mục 8** |

## 3. Hành vi khi tải

- `frmAccoutList_Load` (dòng 384-394): đặt ký tự che mật khẩu, ẩn `lbRcomPass`, khoá
  `grpInfor` (chưa cho sửa gì cho tới khi bấm Thêm/Sửa), gọi `changeSelection()` để nạp dòng
  đang chọn (nếu có) lên các ô.
- `cbPhanQuyen_Load` (dòng 395-402): nạp danh mục loại tài khoản từ
  `SELECT_LIST_LOAI_TAI_KHOAN`.
- `EnableWhenRowIsZezo` (dòng 83-96): lưới rỗng thì vô hiệu hoá Sửa/Xoá, xoá trắng form,
  khoá `grpInfor`.
- Không thấy sắp xếp/lọc mặc định nào được đặt riêng trong file này (khác với các danh sách
  khác đã migrate) — suy đoán lưới nạp theo thứ tự `SELECT_LIST_ACCOUNT` trả về
  (**chưa chắc** — không đọc `SqlConstants.cs` trong phạm vi task này để xác nhận `ORDER BY`).

## 4. Quy tắc nghiệp vụ và kiểm tra dữ liệu

Toàn bộ nằm trong `checkInput()` (dòng 129-199), theo đúng thứ tự kiểm:

1. Họ tên bắt buộc — lỗi "Vui lòng điền họ tên người dùng".
2. Tên đăng nhập bắt buộc — lỗi "Vui lòng điền tên đăng nhập".
3. Tên đăng nhập chỉ gồm chữ/số (`^[a-zA-Z0-9]+$`) — lỗi "Tên đăng nhập không hợp lệ".
4. Khi Thêm: tên đăng nhập không được trùng (`IsHasAccount`, dòng 54-59, tra theo
   `SELECT_ACCOUNT` tham số tên đăng nhập) — lỗi "Tên tài khoản đã tồn tại,thử một tên khác".
   **Bản gốc chỉ kiểm trùng toàn cục (không phân biệt giáo xứ vì mỗi bản cài chỉ có một giáo
   xứ) — bản web đổi phạm vi kiểm trùng thành TRONG một giáo xứ** (`TaiKhoanConfig`: chỉ mục
   duy nhất là `(GiaoXuId, TenTaiKhoan)`), vì tên đăng nhập có thể trùng giữa các giáo xứ khác
   nhau khi dùng chung máy chủ — xem mục 8.
5. Khi Thêm: mật khẩu bắt buộc — lỗi "Vui lòng điền mật khẩu".
6. Nếu có nhập mật khẩu (hoặc đang Thêm): mật khẩu và nhập lại phải khớp — lỗi "Mật khẩu và
   nhập lại mật khẩu không khớp".
7. Phải chọn loại tài khoản — lỗi "Vui lòng chọn loại tài khoản".
8. Nếu đang hiện khối câu hỏi bí mật (chỉ khi loại = Quản trị viên): câu hỏi và câu trả lời
   bắt buộc — lỗi tương ứng.

Xoá (`GxAddEdit_DeleteClick`, dòng 109-128): hộp thoại xác nhận "Bạn có chắc chắn muốn xóa ?"
(OK/Cancel) rồi `DELETE_ACCOUNT` (xoá **cứng**, không phải xoá mềm).

## 5. Thao tác người dùng

- **Thêm** (`GxAddEdit_AddClick`): bấm lần 1 vào chế độ thêm (mở form trống, focus Họ tên);
  bấm lần 2 (nút đổi chữ thành "&Thôi") thì huỷ (`cancelEdit`).
- **Sửa** (`GxAddEdit_EditClick`): tương tự, khoá `txtUserName` (không đổi được tên đăng
  nhập), hiện `lbRcomPass` (gợi ý "để trống nếu không đổi mật khẩu" — **chưa chắc nguyên văn**,
  không thấy gán `Text` cho nhãn này trong file).
- **Lưu** (`GxAddEdit_UpdateClick` → `GxAddEdit_UpdateClick`, dòng 68-73): kiểm tra
  `checkInput()`, gọi `UpdateData()`, khoá lại form.
- **Xoá**: xem mục 4.
- **Tải lại danh sách**: gọi lại `LoadData()` của lưới.
- Chọn dòng khác trên lưới (`GxListAccout1_SelectionChanged`) nạp dữ liệu dòng đó lên form và
  khoá form lại (không ở chế độ sửa).
- Đổi Loại tài khoản (`cbPhanQuyen_SelectedIndexChanged`, dòng 410-422): chọn "Quản trị viên"
  (index 0) thì HIỆN khối câu hỏi bí mật; chọn loại khác thì ẨN và xoá trắng hai ô đó.

## 6. Lưới dữ liệu

Không đọc được thiết kế cột của `gxListAccout1` trong phạm vi file này (nằm trong
`.Designer.cs` hoặc lớp UserControl riêng, không có trong 440 dòng đọc) — **chưa chắc** thứ tự
cột chính xác. Suy từ các cột được gán ở `changeSelection`/`UpdateData`: Họ tên, Tên tài
khoản, Email, Số điện thoại, Tên loại (chuỗi hiển thị, không phải mã số), Câu hỏi/Câu trả lời
bí mật (khả năng ẩn khỏi lưới hiển thị chính, **chưa chắc**).

## 7. Liên kết sang màn hình khác

Không mở màn hình khác. Là màn hình lá trong menu Hệ thống của `frmMain`.

## 8. Khác biệt cố ý ở bản web

1. **Không migrate cột `MatKhau`** (Access) sang bản web — quyết định bảo mật đã chốt từ
   trước Task 14 (xem `TaiKhoan.cs`). Không có "đổi mật khẩu lần đầu" tự động vì bảng `TaiKhoan`
   ban đầu rỗng — Task 14 tạo tài khoản quản trị đầu tiên bằng dòng lệnh
   (`dotnet run -- tao-tai-khoan-quan-tri`, đọc mật khẩu từ biến môi trường).
2. **Mật khẩu băm bằng `PasswordHasher<TaiKhoan>`** (PBKDF2, chuẩn ASP.NET Core Identity)
   thay vì `Memory.EnCodePassword` của bản cũ (không rõ thuật toán, khả năng là mã hoá/băm yếu
   — không đọc mã nguồn `Memory.EnCodePassword` trong phạm vi task này, nhưng quyết định bảo
   mật đã chốt là không mang sang bất kể thuật toán cũ là gì).
3. **Câu hỏi/câu trả lời bí mật (`CauHoiGoiY`/`CauTraLoiGoiY`) được GIỮ CỘT nhưng KHÔNG dùng
   để khôi phục mật khẩu** — cơ chế "câu hỏi bí mật" bị xem là yếu về bảo mật (dễ đoán, dễ bị
   kỹ thuật xã hội). Bản web ẩn hẳn hai ô này khỏi form (không migrate UI phần này) — quên mật
   khẩu thì phải nhờ Quản trị viên đặt lại (đúng tinh thần nút "Quên mật khẩu" của bản desktop,
   `frmLogin.cs:44` vốn đã nói rõ "Chức năng quên mật khẩu chỉ dành cho quản trị viên").
4. **Phân quyền vào chính màn hình này bị SIẾT LẠI**: bản desktop không chặn quyền — bất kỳ ai
   mở được menu "Hệ thống" đều vào được `frmAccoutList` và tự cấp quyền Quản trị viên cho tài
   khoản bất kỳ (`frmMain.cs` không kiểm `IsAdmin` trước khi mở `itListAccount`; `IsAdmin`
   được gán ở `frmLogin.cs:69` nhưng không tìm thấy nơi nào khác trong `Source/` đọc lại giá
   trị này để khoá chức năng — xem `grep -rn IsAdmin Source/`). Đây là một lỗ hổng của bản cũ,
   không phải hành vi cố ý cần giữ. **Bản web yêu cầu policy "QuanTri" (LoaiTaiKhoan=0)** cho
   toàn bộ `/api/tai-khoan/*` — quyết định ghi ở `can-review-sau.md`.
5. **Xoá tài khoản đổi từ xoá cứng sang xoá mềm** (`DaXoa=true`) — nhất quán với
   GiaoDan/GiaDinh/GiaoHo, tránh mất vết khi tài khoản đã dùng để ghi `CreatedAt`/audit sau
   này (Phase 1 chưa có audit log theo người dùng, nhưng không nên khoá chặn khả năng đó).
6. **Không migrate quy tắc "tên đăng nhập chỉ gồm chữ và số"**: giữ nguyên
   (kiểm tra lại ở API), không phải khác biệt — ghi ở đây để đối chiếu.
7. Không có màn hình "Reset mật khẩu qua câu hỏi bí mật" (`frmResetPWord`, tham chiếu ở
   `frmLogin.cs:47`) — cố ý bỏ, thay bằng: Quản trị viên sửa tài khoản người khác và đặt mật
   khẩu mới trực tiếp (ô "Mật khẩu mới" trong form Sửa của bản web).

## 9. Chỗ chưa chắc

- Thứ tự/độ rộng cột thật của `gxListAccout1` — không nằm trong `frmAccoutList.cs`.
- `ORDER BY` mặc định của `SELECT_LIST_ACCOUNT` — không đọc `SqlConstants.cs`.
- Thuật toán `Memory.EnCodePassword` — không đọc, không quan trọng vì quyết định là không
  mang sang dù thuật toán là gì.
- Nội dung chính xác của `lbRcomPass` (nhãn gợi ý khi Sửa) — không thấy gán `Text` trong file
  đọc được; bản web dùng placeholder tương đương "Để trống nếu không đổi mật khẩu".
