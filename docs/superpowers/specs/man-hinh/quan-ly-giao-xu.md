# Màn hình: Quản lý giáo phận / giáo hạt / giáo xứ (`frmGiaoXu.cs`)

| | |
|---|---|
| Tệp nguồn | `Source/ChuongTrinh/frmGiaoXu.cs` (280 dòng), `Source/ChuongTrinh/frmGiaoHo.cs` (598 dòng, tham khảo cho phần "Giáo họ" đi kèm) |
| UserControl dùng lại | `gxAddEdit1` (thanh nút Thêm/Sửa/Xoá/Lưu chung), `gxLinhMucList1` (lưới linh mục — NGOÀI PHẠM VI, xem mục 9) |
| Bảng dữ liệu đụng tới | `giao_phan`, `giao_hat`, `giao_xu`, `giao_ho`, `tai_khoan` (tạo tài khoản đầu tiên cho giáo xứ mới) |
| Trạng thái migrate | xong một phần — xem mục 8/9 cho phần cố ý bỏ (LinhMuc, ảnh giáo xứ) |

## 1. Mục đích

Bản desktop: mỗi file `.mdb` phục vụ ĐÚNG MỘT giáo xứ, nên `frmGiaoXu` chỉ là một **form sửa
một dòng duy nhất** (giáo phận + giáo hạt + giáo xứ của chính file này), không có khái niệm
"danh sách". Mở từ menu chính, dùng để sửa thông tin liên hệ của giáo xứ và quản lý danh sách
linh mục coi sóc.

Bản web: **một máy chủ phục vụ nhiều giáo xứ cùng lúc**, nên đây bắt buộc phải là màn hình
**danh sách + thêm + sửa**, đúng yêu cầu đã nêu từ đầu dự án: *"tương lai admin sẽ có chức
năng import và chức năng quản lý danh sách giáo xứ theo giáo phận theo mô hình tập trung"*.
Đối tượng làm việc chính của màn hình này CHÍNH LÀ các giáo xứ khác nhau — đây là ngoại lệ duy
nhất được phép trong toàn hệ thống cho quy tắc "không endpoint nào nhận GiaoXuId từ bên
ngoài", và vì vậy đây là màn hình **nhạy cảm nhất** trong toàn bộ Phase 1 — xem mục 4.

## 2. Bố cục và các trường

**Giáo phận** (`giao_phan`, KHÔNG có `giao_xu_id` — nằm trên cấp giáo xứ):
| Nhãn | Cột CSDL | Kiểu | Bắt buộc | Ghi chú |
|---|---|---|---|---|
| Tên giáo phận | `TenGiaoPhan` | text | có | `frmGiaoXu.cs:73-77` — "Xin nhập tên giáo phận" nếu rỗng |
| Ghi chú | `GhiChu` | text | không | mới thêm ở bản web, Access không có cột này cho `GiaoPhan` — giữ chỗ cho ghi chú nội bộ |

**Giáo hạt** (`giao_hat`, KHÔNG có `giao_xu_id`):
| Nhãn | Cột CSDL | Kiểu | Bắt buộc | Ghi chú |
|---|---|---|---|---|
| Giáo phận | `GiaoPhanId` | FK | có | chọn từ danh sách giáo phận đã có |
| Tên giáo hạt | `TenGiaoHat` | text | có | `frmGiaoXu.cs:80-84` — "Xin nhập tên giáo hạt" |
| Ghi chú | `GhiChu` | text | không | |

**Giáo xứ** (`giao_xu`, KHÔNG có `giao_xu_id` — đây chính LÀ bảng định danh giáo xứ):
| Nhãn | Cột CSDL | Kiểu | Bắt buộc | Ghi chú |
|---|---|---|---|---|
| Giáo hạt | `GiaoHatId` | FK | có (bản web) | desktop không bắt buộc rõ ràng vì luôn có sẵn 1 giáo hạt của chính file; bản web bắt buộc chọn vì tạo mới giữa nhiều giáo hạt |
| Tên giáo xứ | `TenGiaoXu` | text | có | `frmGiaoXu.cs:87-91` — "Xin nhập tên giáo xứ" |
| Địa chỉ | `DiaChi` | text | có (desktop) | `frmGiaoXu.cs:94-98` — "Xin nhập địa chỉ". Bản web: khuyến nghị nhưng không chặn cứng (một giáo xứ mới có thể chưa có địa chỉ đầy đủ ngay khi tạo) — quyết định nới lỏng, ghi ở `can-review-sau.md`. |
| Điện thoại | `DienThoai` | text | không | |
| Email | `Email` | text | không | |
| Website | `Website` | text | không | |
| Ghi chú | `GhiChu` | text (RTF ở desktop) | không | bản web lưu văn bản thuần, KHÔNG chuyển RTF — xem mục 8 |

Cột `Hinh` (logo giáo xứ, lưu tên file trên đĩa cục bộ máy chạy desktop) **không migrate** —
xem mục 8.

## 3. Hành vi khi tải

Desktop: nạp đúng 1 dòng duy nhất mỗi bảng lúc mở form (`frmGiaoXu_Load`, dòng 25-40).

Bản web: `GET /api/quan-tri/giao-phan`, `/api/quan-tri/giao-hat`, `/api/quan-tri/giao-xu` trả
**toàn bộ** danh sách trên máy chủ (không lọc theo giáo xứ nào — đúng bản chất "nhìn xuyên
giáo xứ" của ba bảng này), sắp theo tên. `giao-hat` kèm tên giáo phận, `giao-xu` kèm tên giáo
hạt + tên giáo phận (qua điều hướng `GiaoHat!.GiaoPhan!`) để hiển thị đường dẫn phân cấp đầy
đủ mà không cần gọi lại 3 API.

## 4. Quy tắc nghiệp vụ và kiểm tra dữ liệu — TRỌNG TÂM BẢO MẬT

`GiaoPhan`/`GiaoHat`/`GiaoXu` cố ý **không** có `giao_xu_id` và **không** có chính sách RLS
(chỉ 22 bảng có cột `giao_xu_id` mới được bật RLS — xem migration `BatRlsChoBangTheoGiaoXu`).
Nghĩa là **RLS của PostgreSQL không bảo vệ được ba bảng này** — bất kỳ vai trò CSDL nào đọc
được bảng cũng đọc được TOÀN BỘ dòng, không phân biệt giáo xứ. Lớp phòng thủ DUY NHẤT ở đây là
**tầng phân quyền của ứng dụng** (policy ASP.NET Core), không có lớp thứ hai ở CSDL như các
màn hình khác.

**Quyết định đã chốt** (không có sẵn trong 3 loại tài khoản cũ 0/1/2 — phải thêm):

- Thêm **`LoaiTaiKhoan = 9`, tên hiển thị "Quản trị hệ thống"**, KHÔNG lấy số kế tiếp 3 để
  tránh nhầm với dữ liệu di trú từ Access sau này (nếu một giáo xứ nào đó tự thêm loại tài
  khoản 3 trong file gốc của họ, dù hiện chưa thấy giáo xứ nào làm vậy). Đây là loại tài khoản
  **cao hơn** "Quản trị viên" (`0`, vốn chỉ quản lý được đúng giáo xứ của mình).
- Policy mới `"QuanTriHeThong"` (`Program.cs`) yêu cầu claim `loai_tai_khoan == "9"`. TOÀN BỘ
  endpoint `/api/quan-tri/*` (giáo phận/giáo hạt/giáo xứ + tạo tài khoản quản trị cho giáo xứ
  khác) đòi hỏi policy này — Quản trị viên thường (`0`) của bất kỳ giáo xứ nào bị từ chối
  `403 Forbidden`. Đây chính là ranh giới đóng lỗ hổng "giáo xứ A nhìn/sửa được giáo xứ B" mà
  nhiệm vụ yêu cầu — xem test đỏ→xanh ở `QuanLyGiaoXuTests.cs` và bằng chứng dán trong báo cáo
  nhiệm vụ.
- **Không có tài khoản `LoaiTaiKhoan=9` mặc định nào.** Tạo bằng CLI chính thức
  `dotnet Qlgx.Api.dll tao-tai-khoan-quan-tri` với biến môi trường mới
  `QLGX_ADMIN_LOAI_TAI_KHOAN=9` (mặc định vẫn là `0` nếu không đặt — không đổi hành vi cũ).
  Người vận hành máy chủ tự quyết ai được cấp loại tài khoản này; đề xuất: chỉ 1-2 người vận
  hành trung tâm, KHÔNG cấp cho quản trị viên của từng giáo xứ.
- **Sửa `GiaoXuId` của một tài khoản** (chuyển người dùng sang giáo xứ khác) — **không đưa vào
  phạm vi màn hình này**. `TaiKhoanService.CapNhat` hiện tại không nhận `GiaoXuId` từ bên
  ngoài và **giữ nguyên như vậy** — chuyển một tài khoản đang có dữ liệu (bí tích, gia đình họ
  quản lý) sang giáo xứ khác là một nghiệp vụ phức tạp, dễ gây lẫn dữ liệu giữa hai giáo xứ,
  ngoài phạm vi "thêm giáo xứ thứ hai". Nếu cần trong tương lai, phải là một quyết định thiết
  kế riêng, có cảnh báo rõ ràng.
- **Tạo tài khoản quản trị ĐẦU TIÊN cho một giáo xứ MỚI** (không phải sửa tài khoản có sẵn) —
  CÓ đưa vào phạm vi, vì đây chính là bước bắt buộc ngay sau khi thêm giáo xứ (nếu không có
  màn hình này, người vận hành vẫn phải quay lại CLI, phá vỡ mục tiêu "quản lý qua giao diện").
  `POST /api/quan-tri/giao-xu/{id}/tai-khoan` — policy `QuanTriHeThong`, nhận `GiaoXuId` NGẦM
  ĐỊNH từ `{id}` trên đường dẫn (không phải từ thân yêu cầu), luôn gán `LoaiTaiKhoan = 0` (chỉ
  tạo được quản trị viên THƯỜNG của giáo xứ đó — không tạo được tài khoản `LoaiTaiKhoan=9` qua
  API, chỉ CLI mới cấp được cấp hệ thống, để không lộ đường tự cấp quyền qua HTTP).
- **Xoá giáo xứ**: **chặn hẳn, không có nút xoá nào cho Giáo xứ/Giáo hạt/Giáo phận** ở Phase 1.
  Xoá một giáo xứ kéo theo toàn bộ giáo dân/gia đình/bí tích của họ — hậu quả quá lớn so với
  lợi ích của một nút xoá hiếm khi dùng. Muốn ngừng dùng một giáo xứ, người vận hành xử lý
  ngoài giao diện (ví dụ đổi tên thêm hậu tố "(ngừng)" — dùng được ngay bằng nút Sửa có sẵn).
  Cân nhắc khác: xoá mềm bằng cột `DaXoa` — bị loại vì `GiaoXu`/`GiaoHat`/`GiaoPhan` cố ý
  KHÔNG kế thừa `ThucTheCoSo` (xem `GiaoXu.cs`) nên không có sẵn cột này, thêm migration chỉ
  cho một nút xoá gần như không dùng là không đáng trong Phase 1.
- Tên giáo xứ **không bắt buộc duy nhất toàn máy chủ** (giữ nguyên hành vi Access — không có
  ràng buộc unique trên `TenGiaoXu`), nhưng API cảnh báo (không chặn) nếu trùng tên với giáo
  xứ đã có, để tránh gõ nhầm.

## 5. Thao tác người dùng

- Ba khối **Giáo phận / Giáo hạt / Giáo xứ** hiển thị dạng bảng lồng cấp (giáo hạt thu gọn
  dưới giáo phận, giáo xứ thu gọn dưới giáo hạt) hoặc ba tab riêng — quyết định UI cụ thể do
  người viết front-end chọn theo `qlgx.css` sẵn có, miễn giữ đúng phân cấp thị giác 3 cấp.
- Nút "Thêm giáo phận / giáo hạt / giáo xứ" mở form thêm cùng khối quy tắc mục 4.
- Nút "Sửa" trên mỗi dòng mở form sửa tại chỗ (không nút xoá — xem mục 4).
- Trên mỗi dòng giáo xứ: nút **"Tạo tài khoản quản trị"** — chỉ hiện nếu giáo xứ đó **chưa có
  tài khoản nào** (gọi `GET /api/tai-khoan` lọc theo giáo xứ đang chọn qua endpoint quản trị
  riêng, hoặc đơn giản hơn: luôn hiện, để người vận hành tự quyết có tạo thêm không — quyết
  định: **luôn hiện**, vì một giáo xứ có thể cần nhiều quản trị viên, không giới hạn 1).

## 6. Lưới dữ liệu

Không áp dụng — bản desktop không có lưới cho phần này (chỉ có lưới linh mục, ngoài phạm vi).
Bản web: ba danh sách phẳng, không phân trang (số giáo phận/giáo hạt/giáo xứ trên một máy chủ
pilot chỉ vài chục dòng), sắp theo tên.

## 7. Liên kết sang màn hình khác

- Sau khi tạo tài khoản quản trị cho giáo xứ mới, không điều hướng đi đâu — ở lại danh sách,
  hiện thông báo thành công kèm tên đăng nhập vừa tạo (KHÔNG hiện lại mật khẩu — người tạo đã
  tự gõ mật khẩu vào form, không phải máy chủ sinh hộ).
- Không liên kết tới "Quản lý tài khoản" hiện có (`/api/tai-khoan`) vì màn hình đó chỉ thao tác
  trong phạm vi giáo xứ đang đăng nhập của CHÍNH người dùng — khác phạm vi (xuyên giáo xứ) của
  màn hình này.

## 8. Khác biệt cố ý ở bản web

- **Không migrate cột `Hinh` (logo giáo xứ)**: Access lưu tên file trên đĩa cục bộ — máy chủ
  tập trung không ghi file cục bộ (ràng buộc HA). Có thể làm sau theo đúng mẫu `AnhDaiDien`
  (cột nhị phân trong CSDL) nếu cần, nhưng không có yêu cầu nào nêu logo giáo xứ là cấp thiết —
  để ở Mức 3.
- **`GhiChu` không giữ định dạng RTF**: Access lưu RTF thô (`txtGhiChu.Rtf`), bản web lưu văn
  bản thuần — nhất quán với toàn bộ các trường `GhiChu` khác đã migrate trong hệ thống (không
  có ô nào giữ RTF ở Phase 1).
- **Không có LinhMuc (linh mục coi sóc)**: bảng `LinhMuc` hiện có 0 dòng dữ liệu thật ở
  `qlgx_thu` (xem `TIEN-DO.md`) và đây là một phân hệ riêng biệt (nhiều linh mục, có lịch sử
  thuyên chuyển) — để nguyên trong Mức 3 "hơn 60 màn hình phụ chưa migrate", không kéo vào
  nhiệm vụ này.
- **Bắt buộc chọn Giáo hạt khi thêm Giáo xứ**: desktop không cần vì luôn có đúng 1 giáo hạt sẵn
  có trong file; bản web có nhiều giáo hạt nên bắt buộc chọn, tránh giáo xứ "mồ côi" không rõ
  thuộc giáo hạt nào.
- **Loại tài khoản cấp hệ thống (`LoaiTaiKhoan=9`)**: hoàn toàn mới, không có tương đương ở bản
  desktop (mỗi giáo xứ độc lập một file thì không có khái niệm "quản trị xuyên giáo xứ") — xem
  lý do đầy đủ ở mục 4 và `can-review-sau.md` mục 37.

## 9. Chỗ chưa chắc

- Chưa rõ liệu người giao việc muốn cho phép **sửa `GiaoHatId` của một giáo xứ đã có dữ liệu**
  (chuyển giáo xứ sang giáo hạt khác) — hiện CHO PHÉP sửa (giáo hạt chỉ là thông tin phân loại,
  không ảnh hưởng RLS/dữ liệu giáo dân), nhưng nếu sau này giáo hạt có ý nghĩa nghiệp vụ khác
  (ví dụ báo cáo tổng hợp theo giáo hạt) cần xem lại.
- Chưa có màn hình "nhập dữ liệu Access qua giao diện" (mục 2.4 `VIEC-TIEP-THEO.md`, khác
  nhiệm vụ này) — sau khi tạo giáo xứ mới bằng màn hình này, người vận hành vẫn phải dùng
  `Qlgx.Migration` bằng dòng lệnh để nạp dữ liệu cũ nếu có.
