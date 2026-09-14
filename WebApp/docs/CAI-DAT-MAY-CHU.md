# Cài đặt QLGX lên máy chủ

Tài liệu này viết cho người sẽ cài phần mềm QLGX (bản web) lên một máy chủ, để nhiều giáo xứ
cùng dùng chung. Làm đúng theo thứ tự các bước dưới đây, không cần hiểu về kỹ thuật.

Xem thêm:
- Sao lưu và phục hồi dữ liệu sau khi đã cài xong: `SAO-LUU-PHUC-HOI.md`
- Thẻ phục hồi (tờ giấy quan trọng nhất sinh ra ở bước 4 dưới đây): `THE-PHUC-HOI.md`

## 1. Cần chuẩn bị gì

Trước khi bắt đầu, cần có sẵn:

- **Một máy chủ** chạy Ubuntu 24.04, cấu hình tối thiểu **2 vCPU / 4 GB RAM / 40 GB ổ đĩa
  SSD**. Có thể thuê ở bất kỳ nhà cung cấp VPS nào (Cloudflare không cung cấp máy chủ, chỉ dùng
  để chứa bản sao lưu — xem mục 2).
- **Một tên miền** đã trỏ bản ghi DNS (A record) về đúng địa chỉ IP của máy chủ trên. Không bắt
  buộc phải có ngay khi cài lần đầu, nhưng có tên miền thì hệ thống mới tự cấp được chứng chỉ
  HTTPS — nên chuẩn bị trước.
- **Một tài khoản Cloudflare** (miễn phí) để tạo kho sao lưu — xem mục 2.

## 2. Tạo kho sao lưu trên Cloudflare R2

Đây là nơi hệ thống tự động gửi bản sao lưu tới, 4 lần mỗi ngày. Làm một lần duy nhất, trước
khi cài phần mềm.

1. Đăng nhập vào [dash.cloudflare.com](https://dash.cloudflare.com), vào mục **R2 Object
   Storage** ở thanh bên.
2. Bấm **Create bucket**, đặt tên (ví dụ `qlgx-backup`), chọn vùng lưu trữ, bấm tạo.
3. Vào mục **R2 → Manage API Tokens → Create API Token**.
4. Đặt quyền cho token:
   - Chọn **Object Read & Write** (không chọn quyền quản trị/Admin).
   - **Chỉ áp dụng cho đúng một bucket** vừa tạo ở bước 2 — không chọn "Apply to all buckets".
   Đây là điểm quan trọng nhất của bước này: nếu máy chủ chẳng may bị chiếm quyền, kẻ xấu cầm
   được token này cũng **không** đụng được tới bucket khác hay tài khoản Cloudflare của bạn.
5. Bấm tạo token. Cloudflare chỉ hiện **một lần duy nhất** ba giá trị sau — copy ngay ra một
   nơi tạm an toàn (sẽ nhập lại ở bước 3 dưới đây, sau đó không cần nhớ nữa vì máy chủ tự lưu):
   - **Endpoint** (dạng `https://<mã-tài-khoản>.r2.cloudflarestorage.com`)
   - **Access Key ID**
   - **Secret Access Key**

**Khuyến nghị bật thêm hai tuỳ chọn trên bucket** (vào bucket vừa tạo → Settings):
- **Versioning** (giữ lại các phiên bản cũ của mỗi tệp)
- **Object Lock** (khoá không cho xoá/ghi đè trong một khoảng thời gian)

Hai tuỳ chọn này chống lại trường hợp kẻ xấu (hoặc chính người vận hành thao tác nhầm) xoá sạch
bản sao lưu trên R2. **Bộ cài không tự bật hai tuỳ chọn này** — chúng cần quyền quản trị bucket
cao hơn quyền mà token sao lưu ở bước 4 nên có (đúng nguyên tắc "quyền tối thiểu"). Tự vào
Cloudflare bật tay nếu muốn có lớp bảo vệ này.

## 3. Chạy một lệnh cài

Đăng nhập vào máy chủ qua SSH (quyền root hoặc `sudo`), chạy đúng một lệnh:

```bash
curl -fsSL https://raw.githubusercontent.com/khoannd/qlgx/webapp-phase-1/WebApp/scripts/install.sh | sudo bash
```

Script sẽ tự cài mọi thứ cần thiết (Docker, các gói phụ trợ), rồi lần lượt hỏi các câu sau —
trả lời theo gợi ý:

| Script hỏi | Trả lời |
|---|---|
| `Dia chi endpoint R2` | Endpoint đã lấy ở mục 2 (dạng `https://....r2.cloudflarestorage.com`) |
| `Ten bucket R2` | Tên bucket đã tạo ở mục 2 |
| `R2 Access Key ID` | Access Key ID đã lấy ở mục 2 |
| `R2 Secret Access Key` | Secret Access Key đã lấy ở mục 2 |
| `Ten giao xu` | Tên giáo xứ đầu tiên sẽ dùng hệ thống này |
| `Ten dang nhap quan tri he thong` | Đặt một tên đăng nhập cho tài khoản quản trị trung tâm (bạn dùng để quản lý toàn hệ thống, thêm giáo xứ mới sau này) |
| `Mat khau` | Mật khẩu cho tài khoản trên, tối thiểu 8 ký tự — đặt mật khẩu mạnh, đây là tài khoản có quyền cao nhất |
| `Ho ten hien thi` | Tên hiển thị của người quản trị |
| `Ten mien (de trong neu chua co)` | Tên miền đã trỏ DNS ở mục 1. Nếu chưa có tên miền, để trống — hệ thống chạy tạm qua HTTP, sửa lại sau khi có tên miền (liên hệ người kỹ thuật hỗ trợ) |
| `Bat sao luu tu dong 4 lan/ngay len R2? [Y/n]` | Bấm Enter (mặc định "Có") — **nên luôn chọn Có** |

Quá trình cài mất vài phút (đang dựng cơ sở dữ liệu, dựng máy chủ web). Đợi tới khi thấy dòng
**"HOAN TAT"**.

## 4. VIỆC PHẢI LÀM NGAY SAU KHI CÀI — ĐỪNG BỎ QUA

Cài xong, màn hình sẽ tự in ra một tờ gọi là **Thẻ phục hồi** — đây là **tờ giấy quan trọng
nhất** của toàn bộ hệ thống. Không có nó, sau này nếu máy chủ hỏng thì **không ai** — kể cả
Cloudflare, kể cả người viết phần mềm — phục hồi lại được dữ liệu.

Làm ngay ba việc sau, theo đúng thứ tự:

1. **In tờ Thẻ phục hồi ra giấy**, hoặc chép nội dung vào một trình quản lý mật khẩu đáng tin
   cậy. Xem `THE-PHUC-HOI.md` để biết cách đọc và cất giữ tờ này cho đúng.
2. **Cất tờ giấy (hoặc bản chép) ở một nơi NGOÀI máy chủ này** — két sắt/tủ hồ sơ của giáo xứ,
   hoặc trình quản lý mật khẩu như đã nói ở trên. Tuyệt đối không lưu file này trên chính máy
   chủ, không gửi qua email.
3. Sau khi đã cất xong, xoá tệp trên máy chủ:
   ```bash
   sudo rm /etc/qlgx/the-phuc-hoi.txt
   ```

Nếu bỏ qua bước này và sau đó mất máy chủ, sổ sách giáo xứ nhiều năm sẽ **mất vĩnh viễn**.

## 5. Kiểm tra hệ thống chạy đúng

```bash
qlgx status
```

Lệnh này in ra một bảng kiểm tra toàn hệ thống — mọi dòng phải là **DAT**. Nếu có dòng **KHONG
DAT**, xem mục 7 (Khi gặp trục trặc) bên dưới, hoặc liên hệ người hỗ trợ kỹ thuật.

Sau đó kiểm tra bằng mắt:

1. Mở trình duyệt, vào địa chỉ `https://<tên miền>` (hoặc `http://<địa chỉ IP máy chủ>` nếu
   chưa có tên miền).
2. Đăng nhập bằng tài khoản quản trị vừa tạo ở bước 3.
3. Thử vào một gia đình bất kỳ, bấm in một phiếu gia đình — nếu file PDF hiện ra bình thường
   thì phần in ấn (một phần hay gặp trục trặc) hoạt động tốt.

## 6. Cập nhật về sau

Khi có bản phần mềm mới, chạy:

```bash
qlgx update
```

Lệnh này **tự động sao lưu dữ liệu trước khi cập nhật**, rồi mới cài bản mới. Nếu bản mới có
vấn đề (không khởi động lên được), hệ thống **tự động quay lại bản cũ** — không cần thao tác gì
thêm, giáo xứ không bị gián đoạn lâu.

Mặc định hệ thống **không** tự động cập nhật — phải tự chạy `qlgx update` khi muốn. Theo dõi
kênh thông báo phiên bản mới của phần mềm để biết khi nào nên cập nhật.

## 7. Khi gặp trục trặc

| Triệu chứng | Nguyên nhân thường gặp | Cách xử lý |
|---|---|---|
| Cài đặt báo lỗi cổng 80/443 đang bận | Máy chủ đã có sẵn một web server khác (Apache, Nginx…) chiếm cổng 80/443 | Gỡ hoặc dừng web server đó trước khi cài QLGX. QLGX cần độc chiếm hai cổng này để phục vụ HTTPS |
| Có tên miền nhưng vẫn chạy HTTP, không lên được HTTPS | DNS của tên miền chưa trỏ đúng về địa chỉ IP máy chủ, nên hệ thống cấp chứng chỉ tự động (Let's Encrypt) không xin được chứng chỉ | Kiểm tra bản ghi A của tên miền trỏ đúng IP máy chủ (chờ vài phút tới vài giờ để DNS cập nhật), sau đó liên hệ người kỹ thuật để cấu hình lại HTTPS |
| `qlgx status` báo "het dia"/hết dung lượng, hoặc hệ thống chạy chậm bất thường | Ổ đĩa máy chủ gần đầy — dữ liệu giáo dân, ảnh đại diện, hoặc log tích luỹ lâu ngày | Liên hệ người kỹ thuật để dọn dẹp hoặc nâng cấp ổ đĩa. Không tự ý xoá file trong `/opt/qlgx` hay `/var/lib/qlgx` |
| `qlgx status` báo kho sao lưu (R2) từ chối khoá / không mở được | Access Key/Secret Key trên Cloudflare bị thu hồi, hết hạn, hoặc gõ sai lúc cài | Kiểm tra lại token trên Cloudflare (mục 2), tạo token mới nếu cần, rồi cập nhật `/etc/qlgx/backup.env` (cần người kỹ thuật hỗ trợ) |
| Trang web không mở được, hoặc mở lên báo lỗi | Container API chưa lên được (thường do lỗi cấu hình hoặc lỗi CSDL) | Xem log để biết chi tiết: <br>`qlgx logs api` <br>Đọc vài dòng cuối, thường có mô tả lỗi rõ ràng. Không tự sửa nếu không chắc — gửi log này cho người kỹ thuật hỗ trợ |

Với mọi trục trặc không có trong bảng trên, chạy `qlgx status` và gửi kết quả cho người kỹ
thuật hỗ trợ — bảng đó cho biết chính xác phần nào của hệ thống đang có vấn đề.
