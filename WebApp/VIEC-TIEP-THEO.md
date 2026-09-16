# Việc cần làm tiếp theo

Chốt ngày 2026-09-07, sau khi hoàn tất phần cài đặt giai đoạn 1 và ba đợt review độc lập.

> **Cập nhật 2026-09-07 (tối)**: đã làm xong **toàn bộ mức 1 và toàn bộ mức 2**. Xem dấu
> ✅ dưới đây. Test hiện tại: **244 backend + 239 front-end**, `npm run build` chạy được.
>
> Cột mốc quan trọng nhất: **đã thử thật với giáo xứ thứ hai** — tạo giáo xứ mới, nhập dữ
> liệu Access vào đó, đăng nhập bằng tài khoản của giáo xứ đó, xác nhận **không thấy dữ liệu
> giáo xứ Vô Nhiễm**. Đây là lần đầu mô hình nhiều giáo xứ dùng chung máy chủ được kiểm chứng
> bằng dữ liệu thật, không phải bằng test giả lập.
>
> **Cập nhật 2026-09-07 (tối)**: đã làm xong **mục 2.2** và **mục 2.3**. Test hiện tại:
> **235 backend + 235 front-end**. Chứng minh bằng chạy thật với một giáo xứ thứ hai THẬT
> (`Giáo xứ Thánh Gia`) — xem `docs/superpowers/specs/man-hinh/can-review-sau.md` mục 37 và
> `.superpowers/sdd/2026-09-06-qlgx-web-phase-1/task-quan-ly-giao-xu-report.md`.

> **Cập nhật 2026-09-08 (đêm)**: đã **migrate xong TOÀN BỘ màn hình** trong thanh điều hướng
> và **cả 8 mẫu in**. Test: **387 backend + 394 front-end**, `npm run build` chạy được.
> Dữ liệu `qlgx_thu` nguyên vẹn: 2050 giáo dân / 40 gia đình / 145 thành viên / 1 giáo họ /
> 1108 đợt bí tích / 6150 bí tích chi tiết.
>
> Mục 3.1 ("hơn 60 màn hình phụ") nay **đã xong** — xem mục "Màn hình đã migrate" bên dưới.

> **Cập nhật 2026-09-08 (chiều)**: đã **hoàn tất toàn bộ giai đoạn**. Không còn thao tác nào
> báo "chưa hỗ trợ" trên giao diện. Test: **417 backend + 412 front-end**, `npm run build`
> chạy được. Nhánh đã push lên GitHub.
>
> Mục 1.1 (in ấn) nay **đủ 13 mẫu**; mục 3.1 (màn hình phụ) và mục 3.3 (quy tắc 11) **đã xong**.

Xếp theo thứ tự nên làm. Lý do xếp hạng ghi ngay dưới mỗi mục — đừng đảo thứ tự nếu chưa
đọc lý do.

---

## Mức 1 — Chặn việc giáo xứ bỏ hẳn bản desktop

### 1.1 In ấn và chứng nhận  ← ✅ ĐÃ XONG (13 mẫu)

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

**✅ Bốn mẫu Giấy giới thiệu đã xong** (commit `984896d`). Vấn đề "thông tin bên thứ hai" đã
được giải: đọc mã desktop thấy `frmReport.cs` dùng **hai ô nhập tay** cho giáo xứ/giáo phận
người nhận (không tra bảng nào, vì bên kia thuộc giáo xứ khác), không lưu gì xuống CSDL.
Bản web làm theo đúng cách đó. Đáng chú ý: cả hai bản cài đặt Giấy giới thiệu trong desktop
đều là **mã chết**, chưa từng được gọi từ menu nào.

**✅ Toàn bộ đã xong.** Ngoài các mẫu trên còn có: In danh sách (khổ ngang), In lý lịch cá
nhân cho cả gia đình, In sổ gia đình, In giới thiệu hôn phối / kết quả rao hôn phối, và
**Phiếu gia đình khổ A3** (hạ tầng nay chọn được khổ giấy A4/A3). Tổng **13 mẫu** in được thật,
mỗi mẫu đều đã mở PDF ra kiểm bằng `PyMuPDF` — tiếng Việt có dấu đúng, đúng khổ giấy.

Cũng đã có **xuất Excel thật** (ClosedXML) cho danh sách giáo dân, gia đình, sổ bí tích và
rao hôn phối — thay cho CSV, theo yêu cầu của người dùng.

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

### 2.2 Vai trò CSDL riêng cho RLS  ← ✅ ĐÃ XONG  ← ✅ ĐÃ XONG (2026-09-07)

`.env.example` đã tách `QLGX_APP_DB_USER` (bị RLS hạn chế) và `QLGX_ADMIN_DB_USER`
(có `BYPASSRLS`), nhưng khi pilot một giáo xứ thì API đang dùng chung một vai trò.
**Phải tách thật** trước khi có giáo xứ thứ hai — xem `WebApp/TRIEN-KHAI.md`.

**Đã chạy thật**: tạo hai vai trò `qlgx_app`/`qlgx_admin` trên `qlgx_thu`, chạy `Qlgx.Api` với
hai vai trò tách biệt, xác nhận đăng nhập vẫn hoạt động (qua `qlgx_admin`) và nghiệp vụ hằng
ngày bị RLS đúng thiết kế (qua `qlgx_app`) — xem `TRIEN-KHAI.md` mục 5 (đã cập nhật) và
`docs/superpowers/specs/man-hinh/can-review-sau.md` mục 37c.

### 2.3 Quản lý giáo xứ theo giáo phận  ← ✅ ĐÃ XONG  ← ✅ ĐÃ XONG (2026-09-07)

Phân cấp Giáo phận → Giáo hạt → Giáo xứ **đã có trong CSDL** và nối đúng dữ liệu thật
(Phan Thiết → Đức Tánh → Vô Nhiễm), nhưng **chưa có màn hình quản lý**. Hiện phải thêm
giáo xứ bằng cách chèn thẳng vào CSDL.

**Đã có màn hình** (`docs/superpowers/specs/man-hinh/quan-ly-giao-xu.md`) — chỉ tài khoản
`LoaiTaiKhoan=9` "Quản trị hệ thống" mới vào được (policy "QuanTriHeThong", lớp phòng thủ DUY
NHẤT vì `GiaoPhan`/`GiaoHat`/`GiaoXu` không có `giao_xu_id` nên không có RLS). Đã tạo thật một
giáo xứ thứ hai ("Giáo xứ Thánh Gia") qua giao diện, tạo tài khoản cho giáo xứ đó, đăng nhập lại
và xác nhận KHÔNG thấy 2050 giáo dân của Vô Nhiễm — xem
`docs/superpowers/specs/man-hinh/can-review-sau.md` mục 37d, ảnh ở
`WebApp/anh-chup-kiem-thu/65`-`67`.

### 2.4 Chức năng nhập dữ liệu cho quản trị viên  ← ✅ ĐÃ XONG  ← ✅ ĐÃ XONG (2026-09-07)

Công cụ chuyển dữ liệu Access hiện chạy bằng dòng lệnh, cần người kỹ thuật. Quản trị viên
cần tự nhập được file `.mdb` của giáo xứ mới qua giao diện.

**✅ Đã làm (commit `8ebd4b2`)** — quy trình **hai bước**, vì máy chủ chạy Linux mà đọc `.mdb`
cần driver ACE OLEDB chỉ có trên Windows (đã kiểm chứng: `Qlgx.Migration.csproj` là
`net10.0-windows`, `Dockerfile` là image Linux):
1. Quản trị viên chạy `Qlgx.Migration <file.mdb> --xuat-goi=goi.json.gz` trên **máy Windows**
   của mình — rút dữ liệu ra gói JSON nén, dùng lại `DocAccess` đã có.
2. Tải gói đó lên qua màn hình **"Nhập dữ liệu Access"** (`/api/quan-tri/nhap-du-lieu/*`,
   chỉ Quản trị hệ thống). Máy chủ đọc thẳng từ luồng tải lên, **không ghi đĩa** (ràng buộc HA).
   Có chạy thử đối chiếu trước, chặn khi giáo xứ đích đã có dữ liệu, chạy nền theo dõi tiến độ
   qua bảng `NhapDuLieuJob` (không giữ trạng thái trong tiến trình).

**Một lỗi rất nghiêm trọng đã được phát hiện và sửa trong lúc kiểm thử thật**: khoá ánh xạ ID
cũ→mới (`BangAnhXaId`) **không phân theo giáo xứ**. Vô hại với công cụ dòng lệnh một giáo xứ,
nhưng khi nhập giáo xứ thứ hai thì nó **âm thầm gán lại toàn bộ 2050 giáo dân / 40 gia đình /
522 hôn phối / 6150 bí tích chi tiết của giáo xứ Vô Nhiễm sang giáo xứ mới**. Phát hiện bằng
`psql`, đã sửa (thêm `giaoXuId` vào khoá), khôi phục dữ liệu và chạy lại kiểm chứng.

**Đã xong theo kiến trúc hai bước** (máy chủ Linux không đọc được `.mdb`): quản trị viên chạy
`Qlgx.Migration <file.mdb> --xuat-goi=goi.json.gz` tại máy Windows của mình để rút gói dữ liệu
trung gian (JSON nén gzip, không cần mạng tới PostgreSQL), rồi tải GÓI đó lên màn hình "Nhập dữ
liệu Access" (`/api/quan-tri/nhap-du-lieu/*`, policy "QuanTriHeThong") — có chạy thử (đối chiếu
số dòng, không ghi gì), cảnh báo/chặn khi giáo xứ đích đã có dữ liệu, chạy nền có theo dõi tiến
độ, và đối chiếu số dòng sau khi nhập thật. **Tự phát hiện và sửa một lỗi nghiêm trọng giữa
chừng**: khoá chống trùng lặp gốc (`BangAnhXaId`, tái sử dụng từ công cụ dòng lệnh) không tách
theo giáo xứ, khiến nhập giáo xứ B có thể ĐÁNH CẮP dữ liệu giáo xứ A nếu hai giáo xứ có mã cũ
Access trùng nhau (gần như luôn đúng — mọi Access đều đánh số từ 1) — xem
`docs/superpowers/specs/man-hinh/can-review-sau.md` mục 38g. Đã sửa và chứng minh lại bằng chạy
thật: nhập `BIN/giaoxu.mdb` vào giáo xứ mới, khớp tuyệt đối GiaoHo 1/GiaDinh 40/GiaoDan 2050/
ThanhVienGiaDinh 145/HonPhoi 522/GiaoDanHonPhoi 1043/DotBiTich 1108/BiTichChiTiet 6150, Vô Nhiễm
giữ nguyên không lẫn lộn, nhập lại lần hai không tạo trùng — ảnh `68`-`72` trong
`WebApp/anh-chup-kiem-thu/`.

---

## Mức 3 — Còn nợ, không chặn

### 3.1 Hơn 60 màn hình phụ chưa migrate  ← ✅ ĐÃ XONG

Kiểm tra dữ liệu, chuẩn hoá dữ liệu, chuyển họ hàng loạt, thống kê, biểu đồ, hồ sơ lưu trữ,
toàn bộ phân hệ giáo lý (lớp, khối, học viên, giáo lý viên), rao hôn phối, sổ bí tích.

**✅ Đã xong hết ngày 2026-09-08**, mỗi màn hình đều qua đủ vòng: nghiên cứu mã desktop →
viết spec có trích dẫn dòng → migrate → test → chạy thật trên trình duyệt với dữ liệu thật.

| Màn hình | Commit |
|---|---|
| Sổ bí tích, Rao hôn phối | `aa4a2af` |
| Giáo họ, Danh sách hội đoàn | `7d27f64` |
| Hồ sơ lưu trữ giáo dân + gia đình | `d1631c9` |
| Phân hệ Giáo lý (khối/lớp/học viên/giáo lý viên) | `d4c4b27` |
| Thống kê chung, Biểu đồ | `7200c86` |
| Kiểm tra dữ liệu — giáo dân | `da622a2` |
| Kiểm tra dữ liệu — gia đình, Chuyển họ hàng loạt | `8f3dee5` |
| Giáo xứ (tự sửa thông tin xứ) | `d344162` |
| Chuẩn hoá dữ liệu | `9d25033` |
| Tạo danh sách bí tích tự động | `d9795ef` |
| Tìm và thay thế | `c91c883` |
| 4 mẫu Giấy giới thiệu | `984896d` |

**Bốn màn hình sửa dữ liệu hàng loạt** (chuẩn hoá, chuyển họ, tạo danh sách bí tích, tìm và
thay thế) đều có **bước xem trước bắt buộc** nêu số bản ghi sẽ đổi, xác nhận rõ ràng, và chạy
trong **một transaction**.

**Ba lỗi thật của bản desktop được phát hiện và TÁI HIỆN Y HỆT** (không tự sửa, đúng chỉ đạo):
số học tuổi bị đảo trong `Extract.cs` khiến nhóm "Giới trẻ"/"Thiếu nhi" luôn rỗng; mốc năm
`1990` viết cứng trong biểu đồ độ tuổi; và bug ghi đè `NguyenNhan` ở kiểm tra gia đình. Tất cả
đã khoá lại bằng test và ghi vào `can-review-sau.md`.

### 3.2 Thu hồi token chủ động

Hiện chỉ giảm nhẹ bằng thời hạn 8 tiếng. Đuổi một người dùng ra khỏi hệ thống ngay lập tức
thì chưa làm được.

### 3.3 Quy tắc 11 — trùng ngày chuyển xứ  ← ✅ ĐÃ XONG

Xem mục 19 trong `can-review-sau.md` (đã cập nhật). Đã kiểm chứng thật trên trình duyệt:
lưu bản ghi chuyển xứ thứ hai trùng ngày trả HTTP 400 kèm thông báo tiếng Việt đúng.

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
