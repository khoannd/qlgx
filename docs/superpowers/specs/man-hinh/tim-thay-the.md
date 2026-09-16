# Màn hình: Tìm và thay thế

| | |
|---|---|
| Tệp nguồn | `Source/GXControl/frmReplace.cs` (145d, UTF-16LE) + `frmReplace.Designer.cs` (146d) |
| Bảng dữ liệu đụng tới | `GiaoDan` (20 cột chuỗi cho phép) hoặc `GiaDinh` (4 cột chuỗi cho phép) — SỬA đúng MỘT cột do người dùng chọn |
| Trạng thái migrate | **Đã xong** |

## 1. Mục đích

Thay thế HÀNG LOẠT một giá trị chuỗi bằng giá trị khác trên một cột của một bảng (Giáo dân hoặc
Gia đình) — ví dụ đổi toàn bộ "Sài Gòn" thành "TP. Hồ Chí Minh" ở cột "Nơi sinh". Đây là công cụ
**thay thế CHÍNH XÁC** (so khớp bằng, không phải tìm-một-phần/chứa), khác quy trình sửa từng bản
ghi thủ công.

Ở bản desktop, mục menu `itTimThayThe` nằm trong nhóm "Tìm kiếm" (cùng "Tìm giáo dân"/"Tìm gia
đình", `frmMain.cs` phần `LoadFunction`), KHÔNG cùng nhóm "Chuẩn hoá dữ liệu"/"Chuyển họ hàng
loạt". Bản web đặt vào nhóm "Công cụ dữ liệu" vì cùng bản chất "công cụ sửa dữ liệu hàng loạt" —
quyết định tự đưa ra, xem `can-review-sau.md` mục 64.

## 2. Bố cục và hành vi (đọc mã `frmReplace.cs`)

- Combo "Bảng" (`cbForm`, dòng 40-46): 2 lựa chọn "Gia đình" (`GiaDinhConst.TableName`) và "Giáo
  dân" (`GiaoDanConst.TableName`) — không có "Tất cả".
- Combo "Trường" (`cbField`) đổi danh sách theo bảng đã chọn
  (`cbForm_SelectedIndexChanged`, dòng 132-140):
  - **Giáo dân** (dòng 47-67, đúng thứ tự khai báo — `ThuocGiaoXu` xuất hiện HAI LẦN ở dòng 61
    và 68, bản web chỉ giữ 1): Tên thánh, Họ tên, Họ tên cha, Họ tên mẹ, Người ban bí tích rửa
    tội, Người ban bí tích xưng tội - rước lễ lần đầu, Người ban bí tích thêm sức, Người đỡ đầu
    rửa tội, Người ban đỡ đầu thêm sức, Nơi sinh, Nơi rửa tội, Nơi xưng tội - rước lễ lần đầu,
    Nơi thêm sức, Thuộc giáo xứ, Điện thoại, Địa chỉ, Email, Trình độ văn hóa, Nghề nghiệp, Ghi
    chú.
  - **Gia đình** (dòng 74-79, 3 dòng bị comment liên quan `HonPhoi` — bỏ qua vì `cbForm` không
    có lựa chọn bảng `HonPhoi`): Tên gia đình, Địa chỉ, Điện thoại, Ghi chú.
- Ô "Giá trị cần tìm" (`txtFind`) và "Giá trị thay thế" (`txtReplace`) — chuỗi tự do.
- Nút OK (`gxCommand1_OnOK`, dòng 90-129):
  1. `txtFind.Text.Trim() == ""` → báo lỗi (Exclamation)
     **"Hãy nhập giá trị cần tìm"** (nguyên văn, dòng 97-101) rồi dừng.
  2. Hỏi xác nhận: **"Bạn có chắc muốn thay thế các giá trị đã nhập?"** (YesNo, dòng 104-107) —
     chọn No thì dừng, không có gì xảy ra.
  3. Ghi: `sql = "UPDATE {bảng} SET {trường}=? WHERE {trường}=?"` (dòng 114-115) — **CHÍNH XÁC**,
     không có ký tự đại diện (không phải `LIKE '%...%'`), tham số hoá đúng 2 tham số (giá trị
     thay thế, giá trị tìm). `rs` = số dòng bị ảnh hưởng (từ `ExecuteSqlCommand`, tương đương
     `ExecuteNonQuery`).
  4. Báo **"Có {0} dữ liệu được thay thế"** (Information, dòng 127, `{0}`=`rs`) — desktop **KHÔNG
     báo gì nếu `rs <= 0`** (chỉ `if (rs >= 0)` — nhưng `ExecuteNonQuery` không bao giờ trả về
     âm nên thực chất luôn báo, kể cả khi `rs=0` "Có 0 dữ liệu được thay thế").
- Không có bước xem trước — desktop chạy thẳng câu UPDATE ngay sau khi người dùng bấm Yes, **không
  biết trước sẽ đổi bao nhiêu dòng** trước khi ghi thật.
- Sau khi ghi (`frmMain.frmReplace_OnOK`, dòng 513-530): mở thẳng danh sách (GiaoDan hoặc
  GiaDinh) lọc theo đúng điều kiện vừa thay thế (`WhereSQL = " AND {trường}='{giá trị thay
  thế}'"`) — để người dùng thấy ngay các bản ghi vừa đổi. **Bản web KHÔNG migrate bước điều
  hướng này** (mở màn hình chi tiết riêng biệt cho từng loại, không có cơ chế lọc theo whereSQL
  tuỳ ý như desktop) — xem mục 4.

## 3. Bốn nguyên tắc an toàn bắt buộc (CỐ Ý khác desktop)

Đây là công cụ SỬA DỮ LIỆU HÀNG LOẠT, cùng khuôn với "Chuyển họ hàng loạt"/"Chuẩn hoá dữ liệu"
(xem `cong-cu-du-lieu.md` mục 4.4):

1. **Bước "Xem trước" bắt buộc** (desktop không có) — gọi
   `POST /api/cong-cu-du-lieu/tim-thay-the/xem-truoc` đếm số bản ghi khớp CHÍNH XÁC THẬT trên
   CSDL tại thời điểm gọi, hiện rõ con số trước khi cho phép xác nhận.
2. **Xác nhận nêu con số cụ thể** — "Có N bản ghi khớp chính xác... Bạn có chắc muốn thay thế
   các giá trị đã nhập?" (giữ nguyên văn câu hỏi xác nhận desktop, thêm con số N).
3. **MỘT transaction** (`BeginTransactionAsync`/`CommitAsync`, desktop chạy 1 câu SQL đơn nên đã
   tự nhiên nguyên tử — bản web vẫn bọc tường minh cho nhất quán với các công cụ khác trong
   nhóm).
4. **Không mở rộng phạm vi cột/bảng** — danh sách cột cho phép đúng 20 (Giáo dân)/4 (Gia đình)
   cột như combo desktop, không thêm/bớt cột nào (kể cả `GhiChu` — có trong danh sách vì desktop
   CHO PHÉP, khác "Chuẩn hoá dữ liệu" luôn loại trừ `GhiChu`).

## 4. Bản web đã làm

- Backend: `WebApp/src/Qlgx.Api/Dtos/TimThayTheDtos.cs`, `Services/TimThayTheService.cs`,
  `Endpoints/TimThayTheEndpoints.cs` — `POST /api/cong-cu-du-lieu/tim-thay-the/xem-truoc` và
  `POST /api/cong-cu-du-lieu/tim-thay-the` (body `{Bang, Truong, GiaTriTim, GiaTriThay}`, `Bang`
  là enum `GiaoDan=0`/`GiaDinh=1`). Dùng `ExecuteUpdateAsync` của EF Core (một câu SQL, không
  tải bản ghi về bộ nhớ) — mỗi cột được ánh xạ tường minh qua `switch` (không dùng reflection)
  để tránh rủi ro tiêm chuỗi cột động.
- Frontend: `WebApp/src/web/src/screens/TimThayThePage.tsx` — 1 màn hình, không tách 2 tab như
  Chuyển họ/Chuẩn hoá (combo "Áp dụng cho" đổi cả bảng lẫn danh sách trường luôn).
- Test: `WebApp/tests/Qlgx.Api.Tests/TimThayTheTests.cs` (8 test, gồm test khẳng định KHÔNG đổi
  bản ghi "gần giống" mà chỉ khớp chính xác) + `WebApp/src/web/src/screens/TimThayThePage.test.tsx`
  (4 test).

## 5. Chỗ chưa chắc

- Không xác nhận được chính xác thông báo "Có {0} dữ liệu được thay thế" có hiện khi `rs=0`
  trên desktop thật hay không (đọc mã thấy điều kiện `if (rs >= 0)` luôn đúng với
  `ExecuteNonQuery`, nhưng không chạy thử trên máy Access thật để xác nhận 100%) — bản web LUÔN
  hiện thông báo kể cả khi 0 bản ghi khớp, coi là tương đương.
- Không migrate bước tự động mở danh sách lọc theo bản ghi vừa đổi sau khi ghi xong (mục 2) —
  cần một cơ chế điều hướng+lọc theo giá trị tuỳ ý mà các màn hình danh sách web hiện chưa hỗ
  trợ; có thể bổ sung sau nếu người dùng cần.
