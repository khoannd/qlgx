# Màn hình: Giáo xứ (tự sửa thông tin giáo xứ)

| | |
|---|---|
| Tệp nguồn | `Source/ChuongTrinh/frmGiaoXu.cs` (280d, UTF-16LE) + `frmGiaoXu.Designer.cs` (403d) |
| Bảng dữ liệu đụng tới | `GiaoXu` (cột `TenGiaoXu`, `DiaChi`, `DienThoai`, `Email`, `Website`, `GhiChu`). Desktop CÒN đụng `GiaoPhan`/`GiaoHat` (xem mục 3) — bản web KHÔNG migrate phần đó |
| Trạng thái migrate | **Đã xong** phần thông tin giáo xứ cốt lõi (mục 1-2). CỐ Ý bỏ ba phần desktop có (mục 3) |

## 1. Mục đích

Văn phòng giáo xứ tự sửa thông tin của chính giáo xứ mình (tên, địa chỉ, điện thoại, email,
website, ghi chú). Đây là màn hình **thiếu hẳn** ở bản web, phát hiện khi đối chiếu menu desktop
với thanh điều hướng web ở lượt trước (`can-review-sau.md` mục 60).

**Phân biệt rõ với "Quản lý giáo xứ"** (`quan-ly-giao-xu.md`, `QuanLyGiaoXuPage.tsx`): màn hình
đó dành cho tài khoản **Quản trị hệ thống** (`LoaiTaiKhoan=9`), xem/sửa **mọi giáo xứ** trên máy
chủ, dùng kết nối CSDL riêng có BYPASSRLS. Màn hình **này** dành cho **mọi tài khoản** của một
giáo xứ, chỉ được sửa **đúng giáo xứ của mình**.

## 2. Bố cục và hành vi (đọc mã `frmGiaoXu.cs`)

- Tải (`frmGiaoXu_Load`, dòng 26-42): `SELECT_GIAOXU` (không lọc gì — desktop là CSDL 1-giáo-xứ
  nên bảng `GiaoXu` chỉ có đúng 1 dòng), gán vào các ô: `txtTenGiaoPhan`, `txtTenGiaoHat`,
  `txtTenGiaoXu`, `txtDiaChi`, `txtWebsite`, `txtDienThoai`, `txtEmail`, `txtHinh` (tên file
  ảnh), `txtGhiChu` (RTF).
- Nút "Cập nhật" (`btnUpdate_Click`, dòng 70-166) — bắt buộc 4 ô không rỗng, THEO ĐÚNG THỨ TỰ
  kiểm (dừng ở lỗi đầu tiên gặp, focus vào đúng ô đó):
  1. `txtTenGiaoPhan` rỗng → `"Hãy nhập tên giáo phận!"` (dòng 72-76)
  2. `txtTenGiaoHat` rỗng → `"Hãy nhập tên giáo hạt!"` (dòng 78-82)
  3. `txtTenGiaoXu` rỗng → `"Hãy nhập tên giáo xứ!"` (dòng 84-88)
  4. `txtDiaChi` rỗng → `"Hãy nhập địa chỉ giáo xứ!"` (dòng 90-94)
- Ghi (`Memory.UpdateDataSet`, một `DataSet` gồm 3 bảng `GiaoPhan`+`GiaoHat`+`GiaoXu`, không có
  transaction tường minh phía ứng dụng — dựa vào `Memory.UpdateDataSet` xử lý ngầm): xong báo
  `"Đã cập nhật thông tin giáo xứ!"` (dòng 169, nguyên văn).
- Ảnh đại diện (`btnBrowse_Click` + `AssignGXData` dòng 172-193): chọn file qua
  `openFileDialog1`, copy vào `Memory.AppPath + txtHinh.Text` (thư mục cài đặt CỤC BỘ của máy
  đang chạy desktop), rồi gán `pictureBox1.BackgroundImage` của `frmMain` — mô hình ổ đĩa cục
  bộ, không áp dụng được cho máy chủ web nhiều giáo xứ.
- Cuối form còn quản lý danh sách Linh mục (`gxLinhMucList1` + `gxAddEdit1`, dòng 44-68,
  196-232) — thêm/sửa/xoá `LinhMuc` qua `frmLinhMuc`, xoá gọi `DELETE_LINHMUC_THEO_ID` sau khi
  hỏi `"Bạn có chắc muốn xóa thông tin Linh mục này?"`.
- Đóng form (`OnClosing`, dòng 234-238): gọi `UpdateProcess.sendGiaoXuInfo()` — theo tên hàm có
  vẻ gửi thông tin giáo xứ lên máy chủ trung tâm nào đó của bản desktop (không đọc sâu, không
  liên quan mô hình web tập trung).

## 3. Bản web migrate PHẦN NÀO, cố ý bỏ phần nào

**Migrate**: đọc/sửa đúng 6 cột `TenGiaoXu`/`DiaChi`/`DienThoai`/`Email`/`Website`/`GhiChu` của
dòng `GiaoXu` khớp `GiaoXuId` trong claim đăng nhập. Hai điều kiện bắt buộc "Hãy nhập tên giáo
xứ!"/"Hãy nhập địa chỉ giáo xứ!" tái hiện nguyên văn (nửa sau của 4 điều kiện desktop).

**CỐ Ý KHÔNG migrate** (quyết định tự đưa ra do khác biệt kiến trúc 1-giáo-xứ/CSDL-riêng của
desktop so với nhiều-giáo-xứ/một-CSDL-chung của web — không phải "quên", ghi vào
`can-review-sau.md`):

1. **Sửa tên Giáo phận/Giáo hạt** (2 điều kiện đầu + phần ghi `GiaoPhan`/`GiaoHat` của
   `btnUpdate_Click`) — ở web, `GiaoHat` có thể có NHIỀU `GiaoXu` cùng trỏ vào (khác desktop:
   một CSDL desktop chỉ có đúng 1 giáo xứ nên sửa tên giáo hạt không ảnh hưởng ai khác). Cho
   một giáo xứ tự đổi tên giáo hạt của mình sẽ vô tình đổi tên hiển thị của MỌI giáo xứ khác
   dùng chung giáo hạt đó — rủi ro cao hơn lợi ích. Muốn đổi giáo phận/giáo hạt, dùng "Quản lý
   giáo xứ" (chỉ Quản trị hệ thống).
2. **Danh sách Linh mục** (`gxLinhMucList1`) — không nằm trong yêu cầu migrate lượt này (nhiệm
   vụ gốc chỉ liệt kê "tên, địa chỉ, điện thoại, email, website, ghi chú"). Bảng `LinhMuc` đã có
   `GiaoXuId` + được RLS bảo vệ (`BatRlsChoBangTheoGiaoXu` liệt `linh_muc`) nên an toàn để làm ở
   một màn hình/lượt riêng sau — không phải vấn đề bảo mật cấp bách như chính màn hình Giáo xứ.
3. **Ảnh đại diện giáo xứ** (`txtHinh`/`btnBrowse`) — mô hình lưu file cục bộ của desktop không
   áp dụng được; muốn làm ở web cần thiết kế upload ảnh riêng (như đã làm cho ảnh đại diện giáo
   dân/gia đình, `AnhDaiDienService`) — để lượt sau nếu cần.

## 4. Vì sao bộ lọc toàn cục KHÔNG bảo vệ màn hình này — lớp phòng thủ thật

`GiaoXu` (`Qlgx.Domain/Entities/GiaoXu.cs`) **cố ý không có cột `GiaoXuId`** và **không có mặt**
trong:
- Danh sách `HasQueryFilter` của `QlgxDbContext.OnModelCreating` (bộ lọc tenant tầng EF Core).
- Mảng `BangTheoGiaoXu` của migration `BatRlsChoBangTheoGiaoXu` (RLS tầng PostgreSQL).

Nghĩa là `db.GiaoXu` qua `QlgxDbContext` tiêm bình thường (không phải kết nối
`ChuoiKetNoiQuanTri`) trả về **TẤT CẢ** giáo xứ trên máy chủ, không tự lọc gì. Lớp phòng thủ DUY
NHẤT của màn hình này là `GiaoXuService` (`WebApp/src/Qlgx.Api/Services/GiaoXuService.cs`) —
MỌI câu lệnh đọc/ghi đều tự thêm `Where(x => x.Id == boiCanh.GiaoXuId)` với `GiaoXuId` lấy từ
`IBoiCanhGiaoXu` (claim đăng nhập), **không bao giờ** nhận `id` làm tham số route/query/body từ
trình duyệt — hai endpoint `GET /api/giao-xu` và `PUT /api/giao-xu` đều KHÔNG có `{id}` trên
đường dẫn, khác hẳn `PUT /api/quan-tri/giao-xu/{id}` của màn hình quản trị hệ thống.

`GiaoXuTests.cs` (`WebApp/tests/Qlgx.Api.Tests/`) chứng minh: dựng 2 giáo xứ A (của claim đăng
nhập) và B, đăng nhập A, gọi PUT sửa "thông tin của mình" — B tuyệt đối không đổi
(`Dang_nhap_giao_xu_A_sua_khong_lam_doi_thong_tin_giao_xu_B`).

## 5. Bản web đã làm

- Backend: `WebApp/src/Qlgx.Api/Dtos/GiaoXuDtos.cs`, `Services/GiaoXuService.cs`,
  `Endpoints/GiaoXuEndpoints.cs` — `GET /api/giao-xu`, `PUT /api/giao-xu` (cả hai
  `RequireAuthorization()`, không cần policy đặc biệt — mọi tài khoản của giáo xứ đều được sửa
  thông tin xứ mình, giống desktop không phân quyền gì thêm ở màn hình này).
- Frontend: `WebApp/src/web/src/screens/GiaoXuPage.tsx` — form 1 cấp (khác `QuanLyGiaoXuPage`
  3 cấp), không có ô chọn giáo xứ nào (không cần, không được phép). Thêm mục "Giáo xứ" vào nhóm
  "Thông tin giáo xứ" ở `SideNav.tsx`, nối route ở `App.tsx` (`moGiaoXu`).
- Test: `WebApp/tests/Qlgx.Api.Tests/GiaoXuTests.cs` (7 test, gồm test bảo mật xuyên giáo xứ ở
  mục 4) + `WebApp/src/web/src/screens/GiaoXuPage.test.tsx` (3 test).

## 6. Chỗ chưa chắc

- Thứ tự dừng-ở-lỗi-đầu-tiên của desktop (giáo phận → giáo hạt → giáo xứ → địa chỉ) không còn ý
  nghĩa ở web vì đã bỏ 2 điều kiện đầu — bản web chỉ còn kiểm tên giáo xứ rồi địa chỉ, đúng thứ
  tự còn lại của desktop.
- Không rõ `UpdateProcess.sendGiaoXuInfo()` (gọi khi đóng form desktop) làm gì — không đọc sâu
  vì tên hàm gợi ý liên quan hạ tầng đồng bộ dữ liệu cũ của desktop, không áp dụng cho web.
