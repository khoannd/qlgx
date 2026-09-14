# Thẻ phục hồi — cách đọc và cất giữ

## Đây là tờ gì

Ngay sau khi cài xong phần mềm (xem `CAI-DAT-MAY-CHU.md` mục 4), hệ thống tự in ra một tờ có
dạng như sau (số liệu, mật khẩu chỉ là ví dụ):

```
================== THE PHUC HOI QLGX ==================
May chu   : giaoxu-server  (giaoxu.viduten.vn)
Lap ngay  : 14/09/2026 10:30

MAT KHAU RESTIC (khong co dong nay thi KHONG AI phuc hoi duoc,
ke ca Cloudflare -- day la ban chat cua ma hoa phia may chu):
  ZjkGpN0m3vQhX8...(chuỗi dài)

KHO SAO LUU : s3:https://<ma-tai-khoan>.r2.cloudflarestorage.com/qlgx-backup
R2 KEY ID   : <access key id>
R2 SECRET   : <secret access key>

MAT KHAU CSDL:
  postgres   : qlgx_chu / <mat khau>
  qlgx_app   : qlgx_app / <mat khau>
  qlgx_admin : qlgx_admin / <mat khau>

PHUC HOI TU MAY TRANG:
  1. Dung mot may chu Linux moi (Ubuntu 24.04)
  2. Cai lai bo ung dung (tu cai Docker luon, chua co du lieu giao xu nao):
       curl -fsSL https://raw.githubusercontent.com/khoannd/qlgx/webapp-phase-1/WebApp/scripts/install.sh | sudo bash
     Khi duoc hoi kho R2, nhap dung KHO SAO LUU / R2 KEY ID / R2 SECRET o tren.
  3. curl -fsSL https://github.com/khoannd/qlgx.git/raw/webapp-phase-1/WebApp/scripts/qlgx-restore.sh -o qlgx-restore.sh
  4. sudo bash qlgx-restore.sh --card the-phuc-hoi.txt          (in ra ke hoach)
  5. sudo bash qlgx-restore.sh --card the-phuc-hoi.txt --apply  (thuc hien)
=======================================================
```

Đây là tờ giấy **duy nhất** đủ để dựng lại toàn bộ hệ thống — với đúng dữ liệu giáo xứ — trên
một máy chủ hoàn toàn mới, kể cả khi máy chủ hiện tại bị cháy, mất, hay bị phá huỷ hoàn toàn.

## Giải thích từng dòng

| Dòng | Ý nghĩa |
|---|---|
| **Máy chủ / Lập ngày** | Tên máy và tên miền lúc tấm thẻ được in ra, để nhận biết đây là thẻ của hệ thống nào — nếu có nhiều giáo xứ/nhiều máy chủ, đừng lẫn thẻ giữa các máy |
| **MẬT KHẨU RESTIC** | Mật khẩu **mã hoá** bản sao lưu. Đây là dòng quan trọng nhất tờ giấy — không có nó, bản sao lưu trên Cloudflare chỉ là những khối dữ liệu vô nghĩa, không ai đọc lại được |
| **KHO SAO LƯU** | Địa chỉ bucket Cloudflare R2 nơi bản sao lưu được gửi tới |
| **R2 KEY ID / R2 SECRET** | Khoá dùng để hệ thống mới kết nối lại vào đúng kho sao lưu đó |
| **MẬT KHẨU CSDL** | Ba bộ tên đăng nhập/mật khẩu của cơ sở dữ liệu — chỉ dùng khi người kỹ thuật cần xử lý sự cố sâu, người vận hành thường không cần đụng tới |
| **PHỤC HỒI TỪ MÁY TRẮNG** | Năm bước làm khi mất hẳn máy chủ cũ — xem hướng dẫn chi tiết và giải thích từng bước ở `SAO-LUU-PHUC-HOI.md` mục "Cứu hộ khi mất máy chủ" |

## CẢNH BÁO — đọc kỹ trước khi cất tờ này

> Mất tấm thẻ này thì **không ai** phục hồi được bản sao lưu — kể cả Cloudflare, kể cả người
> viết phần mềm. Bản sao được mã hoá ngay trên máy chủ của bạn trước khi gửi đi; không có mật
> khẩu thì dữ liệu chỉ là những khối byte vô nghĩa. Đó là điều khiến bản sao an toàn, và cũng là
> điều khiến tấm thẻ này không thể thay thế.

Không có bản sao thứ hai của tấm thẻ này ở đâu khác — không ở Cloudflare, không ở máy chủ,
không ở nơi phát triển phần mềm. Nếu tờ giấy (hay bản chép) này mất, và máy chủ cũng hỏng cùng
lúc, thì mọi sổ sách giáo xứ trong hệ thống — của mọi giáo xứ đang dùng chung máy chủ đó — mất
vĩnh viễn, không có cách nào lấy lại.

## Nên cất ở đâu

Chọn **một trong hai** cách sau, không cần cả hai:

- **In ra giấy**, cất trong két sắt hoặc tủ hồ sơ của giáo xứ — nơi vẫn cất các giấy tờ quan
  trọng khác (sổ đỏ, hợp đồng…). Nếu có thể, cất thêm một bản sao ở một địa điểm khác (ví dụ nhà
  xứ và toà giám mục) để phòng trường hợp cháy/mất tại một nơi.
- **Lưu trong một trình quản lý mật khẩu** đáng tin cậy (ví dụ Bitwarden, 1Password) mà người
  phụ trách vận hành đang dùng, dưới dạng một "ghi chú an toàn" (secure note).

**Tuyệt đối không:**
- Lưu tệp này (hay ảnh chụp nó) trên chính máy chủ đang chạy phần mềm — máy chủ hỏng là mất
  luôn cả thẻ lẫn dữ liệu cùng một lúc.
- Gửi qua email — email có thể bị xem trộm, và nội dung sẽ nằm trong hộp thư mãi mãi mà không
  kiểm soát được ai đọc.

## Nếu tấm thẻ có nguy cơ bị lộ

Nếu nghi ngờ tấm thẻ đã bị người không phận sự nhìn thấy (mất giấy, máy tính quản lý mật khẩu
bị xâm nhập…), liên hệ ngay người kỹ thuật hỗ trợ để đổi mật khẩu Restic và tạo lại khoá R2 —
đừng đợi tới khi có sự cố mới xử lý.
