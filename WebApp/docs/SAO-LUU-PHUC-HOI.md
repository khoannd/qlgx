# Sao lưu và phục hồi dữ liệu

Tài liệu này viết cho người vận hành máy chủ QLGX — không cần hiểu kỹ thuật, chỉ cần làm đúng
theo từng bước khi cần. Xem `CAI-DAT-MAY-CHU.md` nếu chưa cài phần mềm, và `THE-PHUC-HOI.md` để
biết cách đọc/cất tờ Thẻ phục hồi được nhắc tới nhiều lần dưới đây.

## 1. Hệ thống tự sao lưu khi nào

Sau khi cài đặt (và chọn "Có" ở câu hỏi bật sao lưu tự động), hệ thống **tự động sao lưu 4 lần
một ngày**, vào các giờ **00:00, 06:00, 12:00, 18:00** (giờ Việt Nam) — không cần bấm gì.

> **Về giờ hiển thị trên màn hình web:** các mốc thời gian trên màn hình được hiện theo **múi giờ
> của chính máy tính đang mở trình duyệt**. Nếu mở từ một máy đặt sai múi giờ, hoặc từ nước
> ngoài, giờ hiển thị sẽ lệch so với bốn mốc 00:00/06:00/12:00/18:00 nói trên — đó là do máy
> đang xem, không phải do hệ thống sao lưu sai giờ.

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
| 🟡 **Vàng** | Đang chậm trễ: hơn **8 tiếng** chưa có bản sao lưu mới (quá thời hạn 6 tiếng dự kiến một chút), **hoặc** hơn **14 ngày** chưa diễn tập phục hồi lại | Kiểm tra máy chủ còn bật, còn kết nối mạng; nếu vẫn vàng lâu, liên hệ người kỹ thuật hỗ trợ |
| 🔴 **Đỏ** | Một trong năm điều: (1) có lỗi sao lưu chưa được xử lý, (2) **chưa từng sao lưu lần nào**, (3) lần diễn tập phục hồi gần nhất **thất bại**, (4) hơn **24 tiếng** chưa có bản sao lưu mới, hoặc (5) hơn **30 ngày** chưa diễn tập phục hồi lại | Đọc dòng thông báo ngay dưới đèn và báo ngay cho người kỹ thuật hỗ trợ — đèn đỏ nghĩa là nếu mất máy chủ lúc này, có nguy cơ **không phục hồi được** |

Đèn vàng và đèn đỏ đều hiện thêm một **băng cảnh báo ở đầu mọi màn hình** (chỉ tài khoản Quản
trị hệ thống thấy), để không phải chủ động vào màn hình này mới biết có chuyện.

Chữ **"Đạt"** bên cạnh lần diễn tập gần nhất chỉ nói về **lần đó**, không nói rằng hôm nay vẫn
ổn — vì vậy hãy đọc cả **ngày** ghi kèm. Nếu ngày đó đã cũ hàng tháng, đèn tự chuyển vàng rồi đỏ
dù chữ vẫn là "Đạt".

## 3. Sao lưu thủ công và tải bản sao về máy

Trên màn hình **Hệ thống → Sao lưu & Phục hồi**:

- Bấm **"Sao lưu ngay"** để tạo một bản sao lưu ngoài lịch tự động (ví dụ trước một đợt nhập
  liệu lớn, hoặc trước khi thử một thao tác chưa chắc chắn).
- Bấm nút **"Tải về"** ở cuối dòng tương ứng trong "Danh sách bản sao lưu" (hoặc nhấp chuột phải
  vào dòng đó → **"Tải bản sao này về máy"**) để tải nguyên bản dữ liệu tại thời điểm đó về máy
  tính đang dùng. Hệ thống chuẩn bị tệp trong ít phút, xong sẽ hiện nút **"Tải tệp đã chuẩn bị
  xong"** ở cuối trang. Tệp đã chuẩn bị chỉ được giữ **24 giờ**; quá hạn thì bấm "Tải về" lại
  một lượt mới.

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
2. **Bảng đối chiếu số liệu** — hộp thoại hiện rõ số giáo dân/số gia đình **sau khi phục hồi**,
   đặt cạnh số **hiện tại** để thấy ngay hậu quả, và tô đỏ những dòng sẽ bị **giảm**.

   *Ở phiên bản này, cột "Hiện tại" ghi **"chưa tính được"***: hệ thống chưa có cách đếm số bản
   ghi của toàn máy chủ ngay tại thời điểm bấm, và thà không hiện con số còn hơn hiện một con số
   sai khiến người dùng yên tâm nhầm. Khi đó hộp thoại hiện thêm một dòng đỏ nhắc rằng **mọi thay
   đổi nhập sau thời điểm của bản sao đều sẽ mất** — hãy đọc dòng đó thay cho bảng số.
3. **Phải gõ tay đúng một câu xác nhận** (không phải bấm "Đồng ý" trên hộp thoại — việc đó quá
   dễ bấm theo phản xạ mà không đọc kỹ).
4. **Máy chủ kiểm tra lại câu xác nhận đó lần nữa** — không chỉ tin vào giao diện.

**Trước khi bấm, hãy báo các giáo xứ khác ngừng nhập liệu.** Trong lúc phục hồi, phần dữ liệu họ
nhập sẽ mất, và ở phiên bản này **màn hình của họ không hiện cảnh báo nào** — chỉ tab của người
bấm phục hồi bị chặn lại. Đây là hạn chế đã biết, ghi trong `WebApp/TRIEN-KHAI.md` mục 14.

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
   ghi trên Thẻ phục hồi.

   **Quan trọng — câu hỏi về mật khẩu Restic:** bộ cài sẽ hỏi đại ý *"Máy này đang phục hồi từ
   một kho sao lưu đã có? Nhập MẬT KHẨU RESTIC ghi trên Thẻ phục hồi (Enter để tạo kho mới)"*.
   Ở tình huống cứu hộ này, **phải nhập đúng mật khẩu Restic trên Thẻ phục hồi** — không được
   bỏ qua. Nếu để trống, máy mới sẽ tạo một mật khẩu mới ngẫu nhiên, và mật khẩu mới đó **không
   mở được kho sao lưu cũ**: bước phục hồi bên dưới sẽ thất bại, và kể cả khi phục hồi được
   bằng cách khác thì mọi lần sao lưu tự động về sau cũng hỏng.

   Các câu hỏi khác (tên giáo xứ, tài khoản quản trị…) trả lời tạm bất kỳ — bước phục hồi ở dưới
   sẽ ghi đè toàn bộ bằng dữ liệu cũ.
4. **Tải kịch bản phục hồi** về máy mới:
   ```bash
   curl -fsSL https://raw.githubusercontent.com/khoannd/qlgx/webapp-phase-1/WebApp/scripts/qlgx-restore.sh -o qlgx-restore.sh
   ```
5. **Gõ lại tờ Thẻ phục hồi thành tệp** `/root/the-phuc-hoi.txt` trên máy mới.

   Đây là bước **dễ bị bỏ sót nhất**: tấm thẻ được cất ở dạng **giấy** (đúng như hướng dẫn), còn
   kịch bản phục hồi thì đọc các khoá từ một **tệp**. Trên máy mới chưa hề có tệp đó.

   Gõ lệnh sau rồi chép nội dung vào (bấm `Ctrl+D` để lưu và thoát):

   ```bash
   cat > /root/the-phuc-hoi.txt <<'HET'
   MAT KHAU RESTIC:
     <gõ đúng chuỗi mật khẩu Restic ghi trên thẻ>

   KHO SAO LUU : <gõ đúng dòng KHO SAO LƯU trên thẻ, ví dụ s3:https://….r2.cloudflarestorage.com/qlgx-backup>
   R2 KEY ID   : <gõ đúng dòng R2 KEY ID trên thẻ>
   R2 SECRET   : <gõ đúng dòng R2 SECRET trên thẻ>
   HET
   chmod 600 /root/the-phuc-hoi.txt
   ```

   **Bốn điều phải đúng tuyệt đối**, vì kịch bản tìm khoá theo đúng khuôn này (xem hàm
   `doc_the_phuc_hoi` trong `qlgx-restore.sh`):

   - Dòng nhãn phải có đúng chữ **`MAT KHAU RESTIC`** (viết hoa, không dấu), và **mật khẩu nằm ở
     DÒNG RIÊNG ngay bên dưới** — không viết mật khẩu trên cùng dòng với nhãn.
   - Ba dòng còn lại phải **bắt đầu ngay đầu dòng** bằng đúng các chữ `KHO SAO LUU`, `R2 KEY ID`,
     `R2 SECRET` (viết hoa, không dấu, không thụt đầu dòng), rồi dấu `:`, rồi giá trị.
   - Chép **nguyên văn**, không thêm dấu nháy, không xuống dòng giữa chừng một giá trị.
   - Nếu tấm thẻ của bạn in tiếng Việt có dấu ở phần nhãn, vẫn gõ **không dấu** đúng như khuôn
     trên — kịch bản tìm theo chữ không dấu.

   Kiểm lại trước khi đi tiếp:

   ```bash
   grep -c 'MAT KHAU RESTIC\|KHO SAO LUU\|R2 KEY ID\|R2 SECRET' /root/the-phuc-hoi.txt
   ```

   Phải in ra **4**. Nếu ít hơn 4 là gõ sai nhãn — sửa lại rồi mới chạy bước sau, đừng chạy
   `--apply` với một tờ thẻ gõ sai.

   Nếu chạy bước 6 mà thấy báo `The phuc hoi thieu MAT KHAU RESTIC` thì lỗi nằm ở tệp này chứ
   không phải ở kho sao lưu — quay lại gõ đúng khuôn trên.
6. **Chạy KHÔNG có `--apply` trước** — lệnh này chỉ in ra kế hoạch sẽ làm, **chưa đổi gì**:
   ```bash
   sudo bash qlgx-restore.sh --card the-phuc-hoi.txt
   ```
   Đọc kỹ bảng "Đối chiếu số liệu" hiện ra — so sánh số giáo dân/gia đình có hợp lý không.
7. **Chạy lại có `--apply` để thực hiện thật:**
   ```bash
   sudo bash qlgx-restore.sh --card the-phuc-hoi.txt --apply
   ```
   Hệ thống tự nạp dữ liệu, tự kiểm tra, rồi mới đưa vào dùng — nếu bản nạp có vấn đề, hệ thống
   **tự động huỷ và giữ nguyên trạng thái an toàn**, không có gì bị mất thêm.
8. **Trỏ lại DNS** của tên miền giáo xứ về địa chỉ IP của máy chủ mới.
9. **Kiểm tra lại hệ thống:**
   ```bash
   qlgx status
   ```
   Mọi dòng phải là **DAT**. Mở trình duyệt, đăng nhập thử để xác nhận dữ liệu đã quay lại đầy
   đủ.
10. **Xoá tệp thẻ vừa gõ** khỏi máy chủ, vì để nguyên tức là mất máy chủ là mất luôn cả thẻ lẫn
    dữ liệu cùng lúc:
    ```bash
    rm /root/the-phuc-hoi.txt
    ```
    Tờ giấy gốc vẫn cất ở nơi cũ — đừng vứt đi.

> **Ghi chú về địa chỉ tải:** hai lệnh `curl` ở trên dùng dạng
> `https://raw.githubusercontent.com/...`. Dạng `https://github.com/khoannd/qlgx.git/raw/...`
> (có đuôi `.git`) là **sai** và trả về lỗi 404 — những tờ Thẻ phục hồi in ra bởi bản cài trước
> 16/09/2026 ghi dạng sai đó; hãy dùng dạng ở đây thay thế.
>
> **Tờ Thẻ phục hồi in ra không nhắc bước 5** (gõ lại thẻ thành tệp): thẻ nhảy thẳng từ lệnh
> `curl` sang `qlgx-restore.sh --card`. Đây là khác biệt đã biết giữa thẻ và tài liệu này — làm
> theo tài liệu này, và ghi tay dòng nhắc đó lên tờ thẻ ngay khi in ra.
>
> **Ghi chú về nhánh `webapp-phase-1` trong đường dẫn:** đây là tên nhánh mã nguồn tại thời điểm
> viết tài liệu. Nhánh này sẽ biến mất sau khi phần mềm được phát hành chính thức, và khi đó
> lệnh trên trả về 404 — kể cả trên những tờ Thẻ phục hồi đã in ra và cất trong két. **Trước khi
> phát hành**, phải đổi các đường dẫn trong tài liệu này, trong `CAI-DAT-MAY-CHU.md`, và trong
> `NHANH_MAC_DINH` của `WebApp/scripts/install.sh` sang `master` (hoặc một thẻ phiên bản cố
> định), rồi mới in thẻ cho giáo xứ. Đây là **việc còn nợ, chưa làm** (N8 của
> `.superpowers/review-sao-luu/review-frontend.md`).

## 7. Diễn tập phục hồi

Hệ thống **tự động diễn tập phục hồi mỗi Chủ nhật** (rạng sáng, giờ ít người dùng nhất) — tự nạp
thử bản sao lưu mới nhất vào một nơi riêng, kiểm tra xong rồi xoá đi, **không đụng tới dữ liệu
đang chạy**. Kết quả (đạt/không đạt) hiện ngay trên đèn trạng thái ở mục 2.

Có thể tự bấm **"Diễn tập phục hồi"** trên màn hình web bất cứ lúc nào muốn kiểm tra ngay, không
cần chờ tới Chủ nhật.

**Vì sao việc này quan trọng:** một bản sao lưu **chưa từng được phục hồi thử** chỉ là một giả
định — có bản sao lưu nằm ở đó không có nghĩa là nó thực sự nạp lại được. Diễn tập hằng tuần là
cách duy nhất biết chắc rằng khi thật sự cần, bản sao lưu sẽ dùng được.
