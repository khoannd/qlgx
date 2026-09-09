# Hợp đồng máy chủ cập nhật QLGX

Tài liệu này dành cho người (hoặc phiên Claude) viết backend mới cho
`quanlygiaoxu.net`. Nó mô tả **chính xác** những gì phần mềm QLGX trên máy người dùng
sẽ gọi, và những gì máy chủ phải trả về.

Phần mềm là ứng dụng Windows đã phát hành, **không sửa được nữa** trên các máy đang
chạy. Vì vậy đây là hợp đồng một chiều: backend phải chiều theo phần mềm, không ngược lại.

> ## ĐANG TẠM DỪNG (từ 2026-09-09)
>
> `version.txt` (cả `/capnhat/`, gốc, `/4.0/`) đang cố ý trả **chuỗi rỗng** —
> mọi máy tạm coi như không có bản mới. Bật/tắt bằng hằng số
> `TAM_DUNG_THONG_BAO_CAP_NHAT` trong `landing/src/lib/update-server.ts`.
>
> **Lý do (phát hiện thật từ báo cáo người dùng):** máy chạy bản 3.3.7 trở về
> trước và 4.0.0–4.0.1 dùng .NET Framework cũ trên Windows XP–7, không nói
> được TLS 1.2. Các máy này vẫn nhận đúng `version.txt` mới hơn qua `http://`
> thuần (không lỗi gì — đúng như hợp đồng mục 3.1 yêu cầu), nên báo "có bản
> mới" đúng. Nhưng `VersionConfig.xml` hiện trả **nguyên văn cùng một**
> `<downloadpath>` cho MỌI nhóm đường dẫn: `https://quanlygiaoxu.net/capnhat/`.
> Máy đời cũ không kết nối `https` được, nên bấm "Cập nhật" luôn thất bại dù
> vừa được báo đúng là có bản mới — đúng kiểu lỗi mục 3.1 đã cảnh báo trước,
> chỉ là ở lớp `<downloadpath>` chứ không phải ở lớp kết nối tới chính
> `version.txt`/`VersionConfig.xml`.
>
> **Hướng sửa thật (chưa làm):** `<downloadpath>` phải khác nhau theo NHÓM
> đường dẫn đang được gọi, không phải một giá trị chung:
> - `/capnhat/VersionConfig.xml` (máy từ 4.0.2, TLS 1.2 tốt) → giữ nguyên
>   `https://quanlygiaoxu.net/capnhat/`.
> - `VersionConfig.xml` phục vụ ở nhóm gốc (`/`) và `/4.0/` (máy đời cũ) →
>   phải trả một địa chỉ **`http://`** — hợp lý nhất là trỏ về lại đúng nhóm
>   đường dẫn mà máy đó đang gọi (`http://quanlygiaoxu.net/` hoặc
>   `http://quanlygiaoxu.net/4.0/`), để máy cũ tự cập nhật vòng qua HTTP chứ
>   không nhảy sang `https` giữa chừng. Cần sửa `versionConfigXmlResponse()`
>   nhận thêm tham số nhóm gọi (giống `downloadUpdateResponse(request, kenh)`
>   đã làm) rồi dựng `<downloadpath>` khác nhau tương ứng, thay vì chỉ chép
>   nguyên xi một bản duy nhất.
> - Sau khi sửa xong, nhớ nghĩ lại mục 3 (Các địa chỉ cũ) và bảng "địa chỉ gốc
>   nằm trên máy" — có thể cần thêm một hàng mới nếu quyết định tạo nhóm
>   đường dẫn HTTP lâu dài riêng cho việc này thay vì tái dùng nhóm cũ.

---

## 1. Địa chỉ gốc

Từ bản 4.0.2, phần mềm mang sẵn địa chỉ gốc:

```
https://quanlygiaoxu.net/capnhat/
```

Địa chỉ này nằm trong file `VersionConfig.xml` cài kèm phần mềm:

```xml
<downloadpath value="download-update">https://quanlygiaoxu.net/capnhat/</downloadpath>
```

Phần mềm **nối chuỗi trực tiếp**, không thêm dấu gì. Nên địa chỉ gốc **bắt buộc kết thúc
bằng dấu `/`**.

> **Đây là một cam kết vĩnh viễn.** Máy nào cài bản 4.0.2 sẽ hỏi đúng địa chỉ này mãi
> mãi. Không bao giờ được bỏ đường dẫn `/capnhat/`.

---

## 2. Bốn đường dẫn máy chủ phải phục vụ

| # | Đường dẫn | Trả về | Ai gọi |
|---|---|---|---|
| 1 | `GET /capnhat/version.txt` | số phiên bản mới nhất, văn bản thuần | kiểm tra nhanh mỗi lần mở chương trình |
| 2 | `GET /capnhat/VersionConfig.xml` | bản mô tả đầy đủ | sau khi (1) báo có bản mới |
| 3 | `GET /capnhat/download-update` | nội dung file `.zip` | khi người dùng đồng ý cập nhật |
| 4 | `GET /capnhat/help/thong_tin_cap_nhat.htm` | trang HTML ghi chú phát hành | khi người dùng bấm "Xem chi tiết" |

Tất cả đều là `GET` ẩn danh, không cookie, không đăng nhập, không chuyển hướng sang
trang đăng nhập.

### 2.1. `version.txt`

Nội dung **chỉ** gồm chữ số và dấu chấm, đúng **bốn** phần:

```
4.0.2.0
```

Yêu cầu bắt buộc:

- **Không có BOM.** Ghi UTF-8 không BOM hoặc ASCII. Phần mềm có đoạn cắt BOM bị giải mã
  sai (`ï»¿`) — dấu vết của một lần đã hỏng vì chuyện này.
- Không có chữ, không có dòng trống thừa (phần mềm có `Trim()` nên khoảng trắng đầu cuối
  thì chấp nhận được).
- `Content-Type: text/plain`.
- Phải **luôn khớp** với `value` trong `VersionConfig.xml`. Lệch nhau thì người dùng thấy
  báo có bản mới rồi tải về không có gì đổi.

### 2.2. `VersionConfig.xml`

Chính là file `Release/VersionConfig.xml` trong kho **`qlgx_bin`** (không phải `BIN/` của
kho `qlgx` — xem hộp cảnh báo dưới đây) của bản phát hành mới nhất, chép nguyên xi. Cấu
trúc rút gọn:

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

- Giữ nguyên namespace `urn:newversion-schema`. Phần mềm tìm node bằng namespace này;
  thiếu là hỏng.
- Giữ nguyên các thuộc tính `value`, `display`, `exename`.
- **`<downloadpath>` phải chứa đúng địa chỉ gốc mới.** Sau khi cập nhật xong, phần mềm
  **xoá file `VersionConfig.xml` trên máy và tải file này về thay vào**. Nghĩa là nội dung
  `<downloadpath>` của máy chủ trở thành địa chỉ gốc của máy người dùng từ đó về sau.
  Đây cũng là cách các máy dùng địa chỉ cũ tự chuyển sang địa chỉ mới (xem mục 3).
- `Content-Type: application/xml` hoặc `text/xml`.
- **Xuống dòng trong `<info>` phải là CRLF (`\r\n`), không phải LF (`\n`) đơn thuần.**
  Hộp thoại "Đã có phiên bản mới" của phần mềm hiển thị đoạn `<info>` bằng một control
  WinForms chỉ nhận `\r\n` làm dấu xuống dòng — `\n` đơn thuần bị bỏ qua, làm cả đoạn ghi
  chú dồn thành một khối chữ không xuống dòng. **Đã xảy ra thật**: file trên GitHub (kho
  `qlgx_bin`) lưu xuống dòng Unix vì git tự chuẩn hoá lúc commit trên Windows
  (`core.autocrlf`), nên máy chủ đọc thẳng về mà không xử lý gì thì sẽ dính đúng lỗi này.

> **Vì sao đọc từ `qlgx_bin` chứ không phải `BIN/` của `qlgx`:** `BIN/` là thư mục build,
> đổi liên tục khi phát triển bình thường (mỗi lần build local đều ghi đè). Nếu máy chủ
> đọc thẳng từ đó, một commit `BIN/VersionConfig.xml` bất kỳ trên `master` — kể cả khi
> chưa hề có ý định phát hành — sẽ lập tức khiến **mọi máy đã cài QLGX** nhận thông báo
> "có bản mới", trong khi file `.zip` tương ứng còn chưa tồn tại. `qlgx_bin` chỉ nhận
> commit đúng lúc phát hành thật (xem lịch sử commit của repo đó), nên ổn định hơn hẳn làm
> nguồn cho một API công khai, gọi liên tục bởi mọi máy người dùng.
>
> **Hệ quả cho quy trình phát hành:** phải chép `Release/VersionConfig.xml` VÀ
> `Release/thong_tin_cap_nhat.htm` từ kho `qlgx` sang `qlgx_bin` rồi commit — xem
> `QUY_TRINH_PHAT_HANH.md` mục 5.1. Quên bước này thì máy chủ vẫn báo bản cũ dù kho `qlgx`
> đã có bản mới.

### 2.3. `download-update`

Trả về **nội dung nhị phân của file `.zip`**, không phải trang HTML, không phải trang
chuyển hướng dạng "bấm vào đây để tải".

- Đường dẫn **không có đuôi `.zip`** — đó là chủ ý, phần mềm ghép
  `<downloadpath>` + thuộc tính `value` (`download-update`).
- Được phép trả `302` chuyển hướng sang file thật có phiên bản trong tên, ví dụ
  `/tai-ve/qlgx_4_0_2_update.zip`. Phần mềm dùng `WebClient.DownloadFile`, tự đi theo
  chuyển hướng.
- `Content-Type: application/zip`.
- Nội dung phải là gói cập nhật của **đúng phiên bản** ghi trong `version.txt` và
  `VersionConfig.xml`.
- Thuộc tính `value` do **máy chủ** quyết định: phần mềm đọc `value` từ file
  `VersionConfig.xml` **tải về từ máy chủ**, không phải từ file trên máy. Nên nếu sau này
  muốn đổi đường dẫn tải, chỉ cần sửa `value` trong file máy chủ phục vụ — không cần phát
  hành lại phần mềm.

### 2.4. `help/thong_tin_cap_nhat.htm`

Trang HTML ghi chú phát hành, mở bằng trình duyệt mặc định. Chính là file
`Release/thong_tin_cap_nhat.htm` trong kho **`qlgx_bin`** — cùng lý do với mục 2.2.

---

## 3. Các địa chỉ cũ — bắt buộc giữ lại

Phần mềm đã phát hành từ nhiều năm, mỗi đời mang một địa chỉ gốc khác nhau **nằm cứng
trong máy người dùng**. Nếu backend mới không phục vụ các đường dẫn này thì những máy đó
**vĩnh viễn không cập nhật được** và người dùng phải cài tay.

| Đời phần mềm | Địa chỉ gốc nằm trên máy | Số máy |
|---|---|---|
| 3.3.7 trở về trước | `http://quanlygiaoxu.net/` | phần lớn người dùng |
| 4.0.0 – 4.0.1 | `http://quanlygiaoxu.net/4.0/` | rất ít |
| 4.0.2 trở đi | `https://quanlygiaoxu.net/capnhat/` | từ nay |

Nên phải phục vụ đủ các đường dẫn sau, nội dung **y hệt** nhóm `/capnhat/`:

```
/version.txt                        /4.0/version.txt
/VersionConfig.xml                  /4.0/VersionConfig.xml
/download.asp?opt=update            /4.0/download-update
/help/thong_tin_cap_nhat.htm        /4.0/help/thong_tin_cap_nhat.htm
```

Lưu ý `/download.asp?opt=update` — đời cũ dùng thuộc tính
`value="download.asp?opt=update"` với địa chỉ gốc `http://quanlygiaoxu.net/`.

**Sau lần cập nhật đầu tiên, các máy này tự chuyển sang địa chỉ mới**, vì phần mềm thay
file `VersionConfig.xml` trên máy bằng file của máy chủ (mục 2.2). Nhưng các đường dẫn cũ
vẫn phải sống lâu dài, vì luôn còn máy nhiều năm không mở.

### 3.1. Cảnh báo về HTTPS

Đời cũ chạy trên **.NET Framework 2.0, Windows XP/7** — không nói được TLS 1.2. Nếu máy
chủ ép chuyển hướng **mọi** truy cập `http://` sang `https://` thì những máy đó không tải
được gì nữa.

Vì vậy: các đường dẫn cũ trong bảng trên **phải trả lời được qua `http://` thuần**, không
ép chuyển sang `https`. Nhóm `/capnhat/` thì dùng `https` bình thường — phần mềm từ 4.0.2
chạy .NET Framework 4.8 trên Windows 10/11, nói TLS 1.2 tốt.

---

## 4. Cái bẫy về số phiên bản

Phần mềm so sánh phiên bản bằng **so sánh chuỗi**, không phải so sánh số:

```csharp
int check = string.Compare(serverVersion, Memory.CurrentVersion);
```

Nghĩa là `"4.0.10.0"` bị coi là **cũ hơn** `"4.0.9.0"`, vì ký tự `1` đứng trước `9`.

**Quy tắc:** không bao giờ để một thành phần của số phiên bản chạm tới hai chữ số. Sau
`4.0.9` phải nhảy sang `4.1.0`, không được dùng `4.0.10`.

---

## 5. Trình tự khi phát hành một bản mới

Thứ tự này quan trọng. Sai thứ tự thì người dùng nhận thông báo có bản mới rồi tải thất
bại.

1. Đưa file `.zip` lên, để `/capnhat/download-update` trả về được.
2. Đưa `help/thong_tin_cap_nhat.htm` lên.
3. Đưa file cài `.exe` lên trang tải về.
4. **Cuối cùng** mới cập nhật `version.txt` và `VersionConfig.xml`.

---

## 6. Cách thử nhanh

```bash
curl -i  https://quanlygiaoxu.net/capnhat/version.txt          # phải ra 4.0.2.0, không BOM
curl -i  https://quanlygiaoxu.net/capnhat/VersionConfig.xml    # phải là XML đúng namespace
curl -IL https://quanlygiaoxu.net/capnhat/download-update      # phải ra application/zip
curl -I  http://quanlygiaoxu.net/version.txt                   # đường dẫn cũ, http thuần
curl -I  http://quanlygiaoxu.net/4.0/version.txt               # đường dẫn cũ, http thuần
```

Thử thật sự: cài bản cũ lên một máy, mở chương trình, vào menu
**Trợ giúp → Kiểm tra phiên bản mới**, xem nó có tải và cập nhật trọn vẹn không.
