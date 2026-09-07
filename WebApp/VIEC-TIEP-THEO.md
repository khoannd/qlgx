# Việc cần làm tiếp theo

Chốt ngày 2026-09-07, sau khi hoàn tất phần cài đặt giai đoạn 1 và ba đợt review độc lập.

> **Cập nhật 2026-09-07 (chiều)**: đã làm xong **toàn bộ mức 1** và **mục 2.1**. Xem dấu
> ✅ dưới đây. Test hiện tại: **222 backend + 229 front-end**, `npm run build` chạy được.

Xếp theo thứ tự nên làm. Lý do xếp hạng ghi ngay dưới mỗi mục — đừng đảo thứ tự nếu chưa
đọc lý do.

---

## Mức 1 — Chặn việc giáo xứ bỏ hẳn bản desktop

### 1.1 In ấn và chứng nhận  ← ✅ HẠ TẦNG + 4/5 MẪU ĐÃ XONG

Kế hoạch gốc xếp in ấn vào **giai đoạn 3**, nhưng thực tế đây là nghiệp vụ **hằng ngày**:
giấy chứng nhận rửa tội, rước lễ, thêm sức, hôn phối, giấy giới thiệu chuyển xứ, sổ gia đình,
lý lịch cá nhân. Hiện **10/12 mục menu chuột phải** của màn hình giáo dân chỉ có nhãn, bấm
không làm gì.

**Chừng nào chưa có, giáo xứ vẫn phải mở bản desktop mỗi ngày để in.** Mọi việc khác trong
tài liệu này đều kém quan trọng hơn mục này.

Hướng đã chốt trong thiết kế: mẫu HTML có chỗ thay thế (Scriban) → Playwright headless
Chromium → PDF; Excel thật bằng ClosedXML. **Tuyệt đối không dùng Office Interop phía máy chủ.**

Mã desktop tham khảo: `Source/ExcelReport/`, `Source/GXControl/frmReport.cs`,
`frmReportGioiThieuHP.cs`. Nhớ viết spec trước theo quy trình ở
`docs/superpowers/specs/man-hinh/README.md`.

**✅ Đã làm (commit `cb287fe`, `9733683`)** — spec ở `docs/superpowers/specs/man-hinh/in-an.md`:
- Hạ tầng: mẫu HTML nhúng trong assembly, thay chỗ trống `{{Khoá}}` có thoát ký tự,
  Chromium headless dùng chung qua Playwright, chọn mẫu theo giáo phận (mặc định `Chung`),
  sinh PDF trong bộ nhớ (không ghi đĩa — đúng ràng buộc HA). **Không dùng Office Interop.**
- **Bốn mẫu in được thật**, PDF mẫu ở `WebApp/anh-chup-kiem-thu/`: Lý lịch cá nhân,
  Chứng nhận bí tích (4 biến thể), Phiếu gia đình, Chứng nhận hôn phối. Ảnh đại diện đã
  vào Lý lịch cá nhân và Phiếu gia đình.

**Còn lại**: 4 mẫu **Giấy giới thiệu** (chuyển xứ, rửa tội, thêm sức, giáo lý hôn phối) —
cần màn hình nhập thông tin bên thứ hai, vì người kia thường **không phải giáo dân** của
giáo xứ này nên không có sẵn trong CSDL. Ngoài ra: rao hôn phối, xuất Excel thật (ClosedXML),
biểu đồ, và `PhieuGiaDinh-A3` (hạ tầng đang cố định khổ A4).

### 1.2 Ảnh đại diện  ← ✅ ĐÃ XONG

Giao diện có khung ảnh 3x4 nhưng **chưa tải ảnh lên được**. Cột `AnhDaiDien` trong Access là
**văn bản** (không phải nhị phân) — đã khảo sát. Cần quyết cách lưu trên máy chủ tập trung:
đừng ghi file xuống đĩa cục bộ (ràng buộc HA), dùng object storage hoặc cột nhị phân trong CSDL.

### 1.3 Màn hình tự đổi mật khẩu  ← ✅ ĐÃ XONG

Người dùng hiện không tự đổi được mật khẩu; chỉ quản trị viên đặt lại. Bắt buộc phải có
trước khi giao cho giáo xứ dùng thật.

---

## Mức 2 — Bắt buộc trước khi có giáo xứ THỨ HAI dùng chung máy chủ

Hiện chỉ có 1 giáo xứ nên chưa lộ, nhưng sẽ thành lỗ hổng thật ngay khi thêm giáo xứ.

### 2.1 Khoá đăng nhập đang khoá chéo giữa các giáo xứ  ← ✅ ĐÃ SỬA

Phát hiện ở đợt review cuối (`review-cuoi.md`, mức Trung bình). Bản sửa chống dò mật khẩu
tăng bộ đếm sai cho **mọi tài khoản trùng tên ở MỌI giáo xứ**, vì tên tài khoản chỉ duy nhất
trong phạm vi một giáo xứ chứ không duy nhất toàn máy chủ.

**Kịch bản hỏng**: kẻ xấu nhập sai mật khẩu 10 lần cho tên phổ biến như `vanphong` →
**khoá tài khoản đó ở TẤT CẢ giáo xứ cùng lúc**. Đây là lỗi từ chối dịch vụ chéo giáo xứ,
trước khi sửa thì không có (vì trước đó không có khoá nào cả).

**✅ Đã sửa (commit `fc75bd1`)**: đăng nhập thu hẹp theo **tên đăng nhập đang gõ** thay vì
theo "máy chủ có bao nhiêu giáo xứ". Tên chỉ tồn tại ở một giáo xứ → xử lý như cũ, không hỏi
gì (pilot một giáo xứ không bị làm phiền). Chỉ khi tên **thật sự trùng ở từ hai giáo xứ trở
lên** mới trả `400 CanChonGiaoXu` kèm danh sách, bắt gửi lại đúng `giaoXuId`. Nhờ chỉ mục
duy nhất `(GiaoXuId, TenTaiKhoan)`, sau khi lọc luôn còn 0-1 tài khoản — đóng lỗ hổng bằng
cấu trúc dữ liệu chứ không bằng luật nghiệp vụ. Có test dựng hai giáo xứ trùng tên tài khoản,
đã chứng minh đỏ trước khi sửa.

### 2.2 Vai trò CSDL riêng cho RLS

`.env.example` đã tách `QLGX_APP_DB_USER` (bị RLS hạn chế) và `QLGX_ADMIN_DB_USER`
(có `BYPASSRLS`), nhưng khi pilot một giáo xứ thì API đang dùng chung một vai trò.
**Phải tách thật** trước khi có giáo xứ thứ hai — xem `WebApp/TRIEN-KHAI.md`.

### 2.3 Quản lý giáo xứ theo giáo phận

Phân cấp Giáo phận → Giáo hạt → Giáo xứ **đã có trong CSDL** và nối đúng dữ liệu thật
(Phan Thiết → Đức Tánh → Vô Nhiễm), nhưng **chưa có màn hình quản lý**. Hiện phải thêm
giáo xứ bằng cách chèn thẳng vào CSDL.

### 2.4 Chức năng nhập dữ liệu cho quản trị viên

Công cụ chuyển dữ liệu Access hiện chạy bằng dòng lệnh, cần người kỹ thuật. Quản trị viên
cần tự nhập được file `.mdb` của giáo xứ mới qua giao diện.

---

## Mức 3 — Còn nợ, không chặn

### 3.1 Hơn 60 màn hình phụ chưa migrate

Kiểm tra dữ liệu, chuẩn hoá dữ liệu, chuyển họ hàng loạt, thống kê, biểu đồ, hồ sơ lưu trữ,
toàn bộ phân hệ giáo lý (lớp, khối, học viên, giáo lý viên), rao hôn phối, sổ bí tích.

Dữ liệu của tất cả các màn hình này **đã có sẵn** trong PostgreSQL (đủ 26/26 bảng Access),
nên chỉ còn phần giao diện và nghiệp vụ. Vẫn theo quy trình **spec trước, migrate sau**.

### 3.2 Thu hồi token chủ động

Hiện chỉ giảm nhẹ bằng thời hạn 8 tiếng. Đuổi một người dùng ra khỏi hệ thống ngay lập tức
thì chưa làm được.

### 3.3 Quy tắc 11 — trùng ngày chuyển xứ

Xem mục 19 trong `can-review-sau.md`.

### 3.4 Kiểm chứng HA thật

Chưa từng chạy thử nhiều bản API song song, PostgreSQL dịch vụ quản lý, hay reverse proxy
với HTTPS thật. Kiến trúc đã thiết kế cho HA nhưng **chưa có bằng chứng vận hành**.

---

## Mức 4 — Việc phải làm ngoài mã nguồn

### 4.1 Mật khẩu file `.mdb` đang lộ công khai trên GitHub

`Source/Giaoly/app.config`, `Source/Giaoly/Properties/Settings.Designer.cs`,
`BIN/Giaoly.dll.config` chứa nguyên văn mật khẩu mở file `giaoxu.mdb`. Ba tệp này **đã có
trên nhánh `master` công khai tại `github.com/khoannd/qlgx`** từ trước phiên làm việc này.

Đây là di sản của bản desktop, không phải do phần web tạo ra. Nhưng nó đang lộ thật:
ai tải repo về cũng mở được mọi file `.mdb` đi kèm.

Cần quyết: đổi mật khẩu các file `.mdb`, gỡ khỏi mã nguồn (đưa vào cấu hình lúc chạy),
và cân nhắc chuyển repo sang chế độ riêng tư.

### 4.2 Dọn 28 bảng rỗng tạo nhầm trong database `postgres`

Một lượt chạy `dotnet ef` bị trỏ nhầm vào database mặc định `postgres`. Đã kiểm: 27 bảng
nghiệp vụ **rỗng hoàn toàn**, chỉ `__EFMigrationsHistory` có 8 dòng. Không ảnh hưởng
`qlgx_thu`.

Cách dọn: dùng `psql` nối vào database `postgres`, xoá schema `public` rồi tạo lại schema
`public` trống. Câu lệnh cụ thể ghi ở mục 29j trong `docs/superpowers/specs/man-hinh/can-review-sau.md`.

### 4.3 Tách thư mục làm việc cho bản desktop

Suốt phiên này có tiến trình khác làm bản desktop 4.0.1 trong **cùng thư mục**, có lúc
`git checkout master` giữa chừng. Lần này không mất gì, nhưng đổi nhánh khi đang có agent
sửa file là rủi ro thật. Nên tách bằng `git worktree`.

---

## Trước khi giao cho giáo xứ dùng thật

Danh sách kiểm bắt buộc:

- [ ] In ấn hoạt động (mục 1.1)
- [ ] Đổi mật khẩu (mục 1.3)
- [ ] Chạy thử tải: 2050 giáo dân + 6150 bí tích chi tiết trên phần cứng thật
- [ ] Sao lưu tự động và **đã diễn tập phục hồi** (xem `WebApp/TRIEN-KHAI.md`)
- [ ] HTTPS sau reverse proxy
- [ ] Người dùng thật của giáo xứ ngồi thử nhập liệu và xác nhận trải nghiệm
- [ ] Anh/chị review xong `docs/superpowers/specs/man-hinh/can-review-sau.md` (31 mục)
