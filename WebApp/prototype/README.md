# Prototype giao diện web QLGX

Bản mẫu giao diện (chỉ HTML/CSS/JS thuần, không có backend) dùng để chốt trải nghiệm
trước khi bắt tay xây dựng bản web thật.

## Mở xem

Mở `qlgx-prototype.html` bằng trình duyệt. Nếu phục vụ qua web server tĩnh, nhớ để
server trả header `Content-Type: text/html; charset=utf-8` — thiếu khai báo mã hoá thì
trình duyệt lùi về bảng mã cũ và toàn bộ dấu tiếng Việt sẽ vỡ.

```
python -c "
import http.server, socketserver
class H(http.server.SimpleHTTPRequestHandler):
    def guess_type(self, path):
        t = super().guess_type(path)
        return 'text/html; charset=utf-8' if t and t.startswith('text/html') else t
socketserver.TCPServer.allow_reuse_address = True
socketserver.TCPServer(('127.0.0.1', 8901), H).serve_forever()
"
```

## Phạm vi

Bản mẫu dựng lại đúng cấu trúc thông tin của bản desktop hiện tại (nhãn tiếng Việt,
thứ tự cột, thứ tự nút, menu chuột phải) nhưng khoác giao diện web hiện đại theo hệ
thiết kế kính mờ của trang giới thiệu trong `landing/`.

Màn hình có trong bản mẫu:

- Khung ứng dụng: thanh trên, điều hướng trái, thẻ tài liệu nhiều tab
- Danh sách gia đình
- Chi tiết gia đình
- Danh sách giáo dân
- Chi tiết giáo dân

## Nguồn đối chiếu trong mã nguồn desktop

| Màn hình bản mẫu | Mã nguồn WinForms tương ứng |
|---|---|
| Khung ứng dụng | `Source/ChuongTrinh/frmMain.cs` |
| Danh sách gia đình | `Source/ChuongTrinh/frmGiaDinhList.cs`, `Source/GXControl/GxGiaDinhList.cs` |
| Chi tiết gia đình | `Source/GXControl/frmGiaDinh.cs`, `Source/GXControl/GxHonPhoiGiaDinh.cs` |
| Danh sách giáo dân | `Source/ChuongTrinh/frmGiaoDanList.cs`, `Source/GXControl/GxGiaoDanList.cs` |
| Chi tiết giáo dân | `Source/GXControl/frmGiaoDan.cs` |

## Dữ liệu

Toàn bộ dữ liệu trong bản mẫu là **dữ liệu minh hoạ**, không phải dữ liệu thật của
giáo xứ nào.
