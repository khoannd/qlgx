# Sao lưu và phục hồi dữ liệu

Tài liệu này viết cho người vận hành máy chủ QLGX — không cần hiểu kỹ thuật, chỉ cần làm đúng
theo từng bước khi cần. Xem `CAI-DAT-MAY-CHU.md` nếu chưa cài phần mềm, và `THE-PHUC-HOI.md` để
biết cách đọc/cất tờ Thẻ phục hồi được nhắc tới nhiều lần dưới đây.

## 1. Hệ thống tự sao lưu khi nào

Sau khi cài đặt (và chọn "Có" ở câu hỏi bật sao lưu tự động), hệ thống **tự động sao lưu 4 lần
một ngày**, vào các giờ **00:00, 06:00, 12:00, 18:00** (giờ Việt Nam) — không cần bấm gì.

Ngoài bốn lần cố định đó, hệ thống còn **tự sao lưu thêm** ngay trước hai thời điểm nguy hiểm
nhất:
- Ngay trước mỗi lần **cập nhật phần mềm** (`qlgx update`).
- Ngay trước mỗi lần **phục hồi dữ liệu** — để nếu chọn nhầm bản sao, vẫn có đường quay lại.

**Điều cần biết:** vì khoảng cách giữa hai lần sao lưu tự động là 6 giờ, nên nếu máy chủ hỏng
đột ngột (cháy, hỏng ổ đĩa, mất trộm…) vào thời điểm xấu nhất, dữ liệu nhập trong **tối đa 6
giờ gần nhất** trước khi hỏng có thể mất — mọi thứ nhập trước đó đều an toàn. Nếu vừa nhập một
đợt dữ liệu lớn (ví dụ vừa chuyển đổi số liệu từ sổ giấy) và muốn chắc chắn có bản sao ngay, bấm
**"Sao lưu ngay"** trên màn hình web (xem mục 3) thay vì chờ tới mốc 6 giờ tiếp theo.

## 2. Cách đọc đèn trạng thái

Trên màn hình **Hệ thống → Sao lưu & Phục hồi** (chỉ tài khoản Quản trị hệ thống nhìn thấy mục
này), luôn có một đèn trạng thái ở đầu trang:

| Đèn | Ý nghĩa | Cần làm gì |
|---|---|---|
| 🟢 **Xanh** | Bình thường — đã có sao lưu gần đây, diễn tập phục hồi gần nhất (nếu có) đạt | Không cần làm gì |
| 🟡 **Vàng** | Đã hơn 8 tiếng chưa có bản sao lưu mới (quá thời hạn 6 tiếng dự kiến một chút) | Kiểm tra máy chủ còn bật, còn kết nối mạng; nếu vẫn vàng lâu, liên hệ người kỹ thuật hỗ trợ |
| 🔴 **Đỏ** | Một trong ba điều: (1) có lỗi sao lưu chưa được xử lý, (2) **chưa từng sao lưu lần nào**, hoặc (3) lần diễn tập phục hồi gần nhất **thất bại** | Đọc dòng thông báo lỗi ngay dưới đèn (nếu có) và báo ngay cho người kỹ thuật hỗ trợ — đèn đỏ nghĩa là nếu mất máy chủ lúc này, có nguy cơ **không phục hồi được** |

## 3. Sao lưu thủ công và tải bản sao về máy

Trên màn hình **Hệ thống → Sao lưu & Phục hồi**:

- Bấm **"Sao lưu ngay"** để tạo một bản sao lưu ngoài lịch tự động (ví dụ trước một đợt nhập
  liệu lớn, hoặc trước khi thử một thao tác chưa chắc chắn).
- Nhấp chuột phải vào một dòng trong "Danh sách bản sao lưu" → **"Tải bản sao này về máy"** để
  tải nguyên bản dữ liệu tại thời điểm đó về máy tính đang dùng.

**Cảnh báo quan trọng:** tệp tải về là bản dữ liệu **CHƯA MÃ HOÁ**, chứa toàn bộ thông tin giáo
dân (họ tên, ngày sinh, số căn cước…). Chỉ tải về một máy tính tin cậy, và **xoá ngay** tệp đó
sau khi dùng xong. Đừng gửi tệp này qua email hay lưu trên ổ đĩa dùng chung.

## 4. Khi nào NÊN phục hồi và khi nào KHÔNG

**Nên phục hồi khi:**
- Lỡ xoá hàng loạt dữ liệu (ví dụ xoá nhầm cả một giáo họ, một lớp giáo lý).
- Nhập nhầm dữ liệu của giáo xứ này vào giáo xứ khác trên cùng hệ thống.

**KHÔNG nên phục hồi khi:**
- Một người bị sai vài trường thông tin (ví dụ gõ nhầm ngày sinh, sai tên cha mẹ). Trường hợp
  này **sửa tay trực tiếp trên màn hình sẽ nhanh hơn nhiều**, và không làm mất dữ liệu của những
  người khác đã thay đổi sau đó.

**Lý do của quy tắc trên** — nói thẳng để hiểu rõ hậu quả trước khi bấm: phục hồi làm mất **mọi**
thay đổi của **mọi** giáo xứ đang dùng chung máy chủ, kể từ sau thời điểm của bản sao được chọn.
Đây không phải thao tác "sửa một chỗ" — đây là "quay ngược thời gian cả hệ thống".

## 5. Phục hồi trên web

Chỉ tài khoản **Quản trị hệ thống** (`LoaiTaiKhoan=9`) mới thấy và làm được thao tác này. Vào
**Hệ thống → Sao lưu & Phục hồi**, nhấp chuột phải vào bản sao muốn phục hồi → **"Phục hồi về
bản sao này…"**.

Màn hình sẽ đưa ra **bốn lớp rào** trước khi cho phép thực hiện — cố ý làm chậm lại và gây khó
chịu, vì đây là thao tác không thể hoàn tác:

1. **Chỉ tài khoản Quản trị hệ thống mới mở được màn hình này** — máy chủ tự kiểm tra lại quyền,
   không chỉ là ẩn nút trên giao diện.
2. **Bảng đối chiếu số liệu** — hộp thoại hiện rõ số giáo dân/số gia đình **hiện tại** và số
   **sau khi phục hồi**, tô đỏ những dòng sẽ bị **giảm** để thấy ngay hậu quả trước khi bấm.
3. **Phải gõ tay đúng một câu xác nhận** (không phải bấm "Đồng ý" trên hộp thoại — việc đó quá
   dễ bấm theo phản xạ mà không đọc kỹ).
4. **Máy chủ kiểm tra lại câu xác nhận đó lần nữa** — không chỉ tin vào giao diện.

Sau khi xác nhận, hệ thống tự làm mọi việc: sao lưu trạng thái hiện tại trước (đề phòng chọn
nhầm bản), nạp bản sao đã chọn, kiểm tra bản nạp có nguyên vẹn không, rồi mới thay thế dữ liệu
đang chạy. Cơ sở dữ liệu cũ **không bị xoá ngay** — được giữ thêm 7 ngày để có thể xử lý nếu có
vấn đề phát sinh, quá trình này theo dõi được trên chính màn hình (mục "Nhật ký công việc").

## 6. Cứu hộ khi mất máy chủ

Dùng danh sách các bước đánh số dưới đây khi cần bình tĩnh làm theo lúc đang hoảng loạn (mất máy
chủ, cháy nổ, mất trộm…). Cần có sẵn tờ **Thẻ phục hồi** (xem `THE-PHUC-HOI.md`) — không có tờ
này thì **không thể** làm tiếp bất kỳ bước nào dưới đây.

1. **Dựng một máy chủ Linux mới** (Ubuntu 24.04, cấu hình tương đương máy cũ — xem
   `CAI-DAT-MAY-CHU.md` mục 1).
2. **Lấy tờ Thẻ phục hồi** ra khỏi nơi đã cất giữ.
3. **Cài lại bộ ứng dụng lên máy mới** — chạy đúng lệnh cài ở `CAI-DAT-MAY-CHU.md` mục 3:
   ```bash
   curl -fsSL https://raw.githubusercontent.com/khoannd/qlgx/webapp-phase-1/WebApp/scripts/install.sh | sudo bash
   ```
   Khi được hỏi cấu hình kho R2, nhập **đúng** ba giá trị "KHO SAO LƯU"/"R2 KEY ID"/"R2 SECRET"
   ghi trên Thẻ phục hồi. Các câu hỏi khác (tên giáo xứ, tài khoản quản trị…) trả lời tạm bất kỳ
   — bước phục hồi ở dưới sẽ ghi đè toàn bộ bằng dữ liệu cũ.
4. **Tải kịch bản phục hồi** về máy mới:
   ```bash
   curl -fsSL https://raw.githubusercontent.com/khoannd/qlgx/webapp-phase-1/WebApp/scripts/qlgx-restore.sh -o qlgx-restore.sh
   ```
5. **Chạy KHÔNG có `--apply` trước** — lệnh này chỉ in ra kế hoạch sẽ làm, **chưa đổi gì**:
   ```bash
   sudo bash qlgx-restore.sh --card the-phuc-hoi.txt
   ```
   Đọc kỹ bảng "Đối chiếu số liệu" hiện ra — so sánh số giáo dân/gia đình có hợp lý không.
6. **Chạy lại có `--apply` để thực hiện thật:**
   ```bash
   sudo bash qlgx-restore.sh --card the-phuc-hoi.txt --apply
   ```
   Hệ thống tự nạp dữ liệu, tự kiểm tra, rồi mới đưa vào dùng — nếu bản nạp có vấn đề, hệ thống
   **tự động huỷ và giữ nguyên trạng thái an toàn**, không có gì bị mất thêm.
7. **Trỏ lại DNS** của tên miền giáo xứ về địa chỉ IP của máy chủ mới.
8. **Kiểm tra lại hệ thống:**
   ```bash
   qlgx status
   ```
   Mọi dòng phải là **DAT**. Mở trình duyệt, đăng nhập thử để xác nhận dữ liệu đã quay lại đầy
   đủ.

## 7. Diễn tập phục hồi

Hệ thống **tự động diễn tập phục hồi mỗi Chủ nhật** (rạng sáng, giờ ít người dùng nhất) — tự nạp
thử bản sao lưu mới nhất vào một nơi riêng, kiểm tra xong rồi xoá đi, **không đụng tới dữ liệu
đang chạy**. Kết quả (đạt/không đạt) hiện ngay trên đèn trạng thái ở mục 2.

Có thể tự bấm **"Diễn tập phục hồi"** trên màn hình web bất cứ lúc nào muốn kiểm tra ngay, không
cần chờ tới Chủ nhật.

**Vì sao việc này quan trọng:** một bản sao lưu **chưa từng được phục hồi thử** chỉ là một giả
định — có bản sao lưu nằm ở đó không có nghĩa là nó thực sự nạp lại được. Diễn tập hằng tuần là
cách duy nhất biết chắc rằng khi thật sự cần, bản sao lưu sẽ dùng được.
