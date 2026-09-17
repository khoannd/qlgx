# Prompt để đưa cho phiên Claude viết website quanlygiaoxu.net

Sao chép toàn bộ phần trong khung dưới đây và dán vào phiên Claude đang làm website.

---

Bạn cần viết API cập nhật cho website `quanlygiaoxu.net`.

## Bối cảnh

QLGX là phần mềm quản lý giáo xứ chạy trên Windows, dùng miễn phí trong nhiều giáo phận
Việt Nam. Người dùng là quý cha, quý sơ — phần lớn không rành máy tính. Phần mềm có chức
năng tự kiểm tra và tải bản cập nhật về.

**Ràng buộc quan trọng nhất:** phần mềm đã phát hành và **đang chạy trên máy người dùng,
không sửa được nữa**. Đây là hợp đồng một chiều — backend phải chiều theo phần mềm, không
được đòi phần mềm đổi theo mình. Nếu bạn thấy thiết kế dưới đây có chỗ kỳ quặc (đường dẫn
không có đuôi `.zip`, so sánh phiên bản bằng chuỗi...), đó là chuyện đã rồi, cứ làm theo.

## Nhiệm vụ

Viết backend phục vụ **bốn** đường dẫn dưới đây. Không thêm đường dẫn nào khác, không đổi
tên, không thêm tầng `/api/` hay số phiên bản API.

Địa chỉ gốc: `https://quanlygiaoxu.net/capnhat/`

| # | Đường dẫn | Trả về |
|---|---|---|
| 1 | `GET /capnhat/version.txt` | số phiên bản mới nhất, văn bản thuần |
| 2 | `GET /capnhat/VersionConfig.xml` | bản mô tả phiên bản, XML |
| 3 | `GET /capnhat/download-update` | nội dung nhị phân file `.zip` cập nhật |
| 4 | `GET /capnhat/help/thong_tin_cap_nhat.htm` | trang HTML ghi chú phát hành |

Tất cả là `GET` ẩn danh: không cookie, không đăng nhập, không chuyển hướng sang trang
đăng nhập, không chặn bằng CAPTCHA hay rate-limit gắt (phần mềm gọi mỗi lần mở lên).

### 1. `version.txt`

Nội dung **chỉ** gồm chữ số và dấu chấm, đúng **bốn** phần:

```
4.0.2.0
```

- **Tuyệt đối không có BOM.** Ghi UTF-8 không BOM hoặc ASCII thuần. Phần mềm có đoạn code
  cắt BOM bị giải mã sai (`ï»¿`) — dấu vết của một lần đã hỏng vì chuyện này.
- Không có chữ, không dòng thừa. Phần mềm có `Trim()` nên khoảng trắng đầu/cuối thì được.
- `Content-Type: text/plain`
- Phải **luôn khớp** với thuộc tính `value` trong `VersionConfig.xml`. Lệch nhau thì người
  dùng thấy báo có bản mới, tải về xong lại không thấy gì đổi.

### 2. `VersionConfig.xml`

Trả nguyên văn file `VersionConfig.xml` của bản phát hành mới nhất. Cấu trúc:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<application value="GiaoXu" xmlns="urn:newversion-schema">
  <version-info value="4.0.2.0" display="4.0.2" exename="GiaoXu.exe">
    <info>… ghi chú phát hành, hiện thẳng cho người dùng đọc …</info>
    <downloadpath value="download-update">https://quanlygiaoxu.net/capnhat/</downloadpath>
    <dateupdate>2026-09-07</dateupdate>
    <size>8.67</size>
  </version-info>
</application>
```

Bắt buộc:

- Giữ nguyên namespace `urn:newversion-schema`. Phần mềm tìm node theo namespace này,
  thiếu là hỏng ngay.
- Giữ nguyên các thuộc tính `value`, `display`, `exename`.
- `<downloadpath>` phải chứa **đúng** `https://quanlygiaoxu.net/capnhat/` (kết thúc bằng
  dấu `/`). Lý do ở mục "Tự chuyển đổi" bên dưới.
- `Content-Type: application/xml` hoặc `text/xml`.

### 3. `download-update`

Trả về **nội dung nhị phân của file `.zip`**. Không phải trang HTML, không phải trang
"bấm vào đây để tải".

- Đường dẫn **không có đuôi `.zip`** — đó là chủ ý. Phần mềm ghép
  `<downloadpath>` + thuộc tính `value` (`download-update`).
- Được phép trả `302` chuyển hướng sang file thật có phiên bản trong tên, ví dụ
  `/tai-ve/qlgx_4_0_2_update.zip`. Phần mềm dùng `WebClient.DownloadFile` của .NET, tự đi
  theo chuyển hướng.
- `Content-Type: application/zip`
- Nội dung phải đúng phiên bản ghi trong `version.txt` và `VersionConfig.xml`.

### 4. `help/thong_tin_cap_nhat.htm`

Trang HTML ghi chú phát hành, mở bằng trình duyệt mặc định khi người dùng bấm
"Xem chi tiết".

## Nguồn file

Các bản phát hành nằm sẵn trên GitHub trong kho `khoannd/qlgx_bin`:

```
Release/qlgx_4_0_2.exe                  <- bộ cài đầy đủ, cho người tải mới
Release/update/qlgx_4_0_2_update.zip    <- gói cập nhật, chính là cái download-update trả về
```

Bạn có thể chọn một trong hai cách, tuỳ hạ tầng website:

- **Cách A:** chép file lên máy chủ bằng tay mỗi lần phát hành, backend phục vụ file tĩnh.
- **Cách B:** backend đọc thẳng từ kho `qlgx_bin` trên GitHub và trả `302` về đó. Cách này
  hay hơn vì quy trình phát hành hiện tại đã tự đẩy file lên kho đó rồi, nên phát hành chỉ
  còn là `git push`. Nhớ đặt cache hợp lý và có phương án dự phòng khi GitHub lỗi.

Hãy hỏi trước khi chọn, đừng tự quyết.

## Tự chuyển đổi — chỗ này quan trọng, đừng bỏ qua

Sau khi cập nhật xong, phần mềm **xoá file `VersionConfig.xml` trên máy và tải file của
máy chủ về thay vào**. Nghĩa là nội dung `<downloadpath>` mà máy chủ trả về sẽ **trở
thành** địa chỉ gốc của máy người dùng từ đó về sau.

Hệ quả: nếu bạn lỡ để `<downloadpath>` sai, toàn bộ máy đã cập nhật sẽ trỏ vào chỗ sai và
**không tự sửa lại được** — phải đi cài tay từng máy.

Mặt tốt: đây chính là cách các máy đời cũ tự chuyển sang địa chỉ mới ngay sau lần cập nhật
đầu tiên.

## Các địa chỉ cũ — bắt buộc giữ lại

Phần mềm phát hành nhiều năm, mỗi đời mang một địa chỉ gốc khác nhau **nằm cứng trong máy
người dùng**. Nếu backend mới không phục vụ những đường dẫn này thì các máy đó **vĩnh viễn
không cập nhật được**, người dùng phải cài tay.

| Đời phần mềm | Địa chỉ gốc trên máy | Số máy |
|---|---|---|
| 3.3.7 trở về trước | `http://quanlygiaoxu.net/` | **phần lớn người dùng** |
| 4.0.0 – 4.0.1 | `http://quanlygiaoxu.net/4.0/` | rất ít |
| 4.0.2 trở đi | `https://quanlygiaoxu.net/capnhat/` | từ nay |

Phải phục vụ đủ, nội dung **y hệt** nhóm `/capnhat/`:

```
/version.txt                        /4.0/version.txt
/VersionConfig.xml                  /4.0/VersionConfig.xml
/download.asp?opt=update            /4.0/download-update
/help/thong_tin_cap_nhat.htm        /4.0/help/thong_tin_cap_nhat.htm
```

Chú ý `/download.asp?opt=update` — đời cũ dùng thuộc tính `value="download.asp?opt=update"`
với địa chỉ gốc `http://quanlygiaoxu.net/`. Đường dẫn có đuôi `.asp` và có query string,
nhưng backend mới không cần là ASP, chỉ cần trả đúng nội dung ở đúng đường dẫn đó.

### Cảnh báo về HTTPS

Đời cũ chạy **.NET Framework 2.0 trên Windows XP/7** — không nói được TLS 1.2. Nếu máy chủ
ép chuyển hướng **mọi** truy cập `http://` sang `https://` thì các máy đó không tải được gì
nữa và chức năng cập nhật chết hẳn.

Vì vậy: các đường dẫn cũ trong bảng trên **phải trả lời được qua `http://` thuần**, không
ép lên `https`. Riêng nhóm `/capnhat/` thì dùng `https` bình thường — phần mềm từ 4.0.2
chạy .NET Framework 4.8 trên Windows 10/11, nói TLS 1.2 tốt.

## Cái bẫy về số phiên bản

Phần mềm so sánh phiên bản bằng **so sánh chuỗi**, không phải so sánh số:

```csharp
int check = string.Compare(serverVersion, Memory.CurrentVersion);
```

Nên `"4.0.10.0"` bị coi là **cũ hơn** `"4.0.9.0"`, vì ký tự `1` đứng trước `9`.

Nếu backend của bạn có chỗ nào tự sinh hay tự sắp xếp số phiên bản, phải dùng đúng quy tắc
này. Và ghi vào tài liệu vận hành: sau `4.0.9` phải nhảy sang `4.1.0`, không được dùng
`4.0.10`.

## Trình tự khi phát hành một bản mới

Thứ tự này quan trọng. Sai thứ tự thì người dùng nhận thông báo có bản mới rồi tải thất bại.

1. Đưa file `.zip` lên, để `/capnhat/download-update` trả về được.
2. Đưa `help/thong_tin_cap_nhat.htm` lên.
3. Đưa file cài `.exe` lên trang tải về.
4. **Cuối cùng** mới cập nhật `version.txt` và `VersionConfig.xml`.

Nếu làm được thao tác đổi nguyên tử (đổi cả bốn cùng lúc) thì càng tốt.

## Coi như xong khi nào

Viết test tự động cho các điểm sau, đừng chỉ thử bằng tay:

1. `curl https://quanlygiaoxu.net/capnhat/version.txt` → đúng `4.0.2.0`, **byte đầu tiên
   không phải BOM**, `Content-Type: text/plain`.
2. `curl https://quanlygiaoxu.net/capnhat/VersionConfig.xml` → XML hợp lệ, có namespace
   `urn:newversion-schema`, `version-info/@value` **bằng đúng** nội dung `version.txt`,
   và `downloadpath` đúng bằng `https://quanlygiaoxu.net/capnhat/`.
3. `curl -L https://quanlygiaoxu.net/capnhat/download-update` → tải được, là file zip hợp lệ
   (kiểm tra 2 byte đầu là `PK`), giải nén ra có `GiaoXu.exe`.
4. `curl https://quanlygiaoxu.net/capnhat/help/thong_tin_cap_nhat.htm` → 200, HTML.
5. Cả bốn đường dẫn cũ trong bảng trên trả về **cùng nội dung**, và trả lời được qua
   `http://` thuần không bị ép sang `https`.
6. Không đường dẫn nào đòi cookie/đăng nhập: thử bằng client sạch, không cookie.

Ngoài ra hãy làm thêm một phép thử thật: cài phần mềm bản cũ lên một máy Windows, mở lên,
vào menu **Trợ giúp → Kiểm tra phiên bản mới**, xem nó tải và cập nhật trọn vẹn không.
Đây là phép thử duy nhất chứng minh hợp đồng đúng.

## Đừng làm

- Đừng đổi tên hay cấu trúc bốn đường dẫn trên.
- Đừng thêm xác thực, API key, rate-limit gắt.
- Đừng trả JSON — phần mềm chỉ đọc được văn bản thuần và XML đúng định dạng trên.
- Đừng bỏ các đường dẫn cũ, dù thấy chúng xấu.
- Đừng ép `https` trên các đường dẫn cũ.

Nếu có chỗ nào trong yêu cầu này mâu thuẫn hoặc bạn thấy thiếu thông tin, hãy hỏi trước
khi viết code.
