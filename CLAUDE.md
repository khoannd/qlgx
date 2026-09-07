# QLGX — Quản Lý Giáo Xứ

Phần mềm quản lý giáo xứ, dùng miễn phí trong nhiều giáo phận Việt Nam. Người dùng là quý
cha, quý sơ — phần lớn không rành máy tính. Dữ liệu là sổ sách giáo xứ nhiều năm, mất là
không lấy lại được. Mọi quyết định kỹ thuật đều phải ưu tiên hai điều đó.

Trả lời người dùng bằng **tiếng Việt**.

## Bố cục

| Thư mục | Nội dung |
|---|---|
| `Source\` | mã nguồn WinForms, .NET Framework 4.8, x86 (solution `Source\GiaoXu.sln`) |
| `BIN\` | thư mục build ra, cũng là bộ file chạy được |
| `Source\GXInstaller\` | dự án bộ cài (`.vdproj`, Visual Studio Installer Projects) |
| `Release\` | file phát hành đã ký nhận vào kho |
| `WebApp\` | bản web đang làm dở, nhánh riêng — **không đụng tới khi phát hành bản desktop** |
| `*.ps1` ở thư mục gốc | các script của quy trình phát hành |

Kho nhị phân cho người dùng tải nằm ở `D:\Working\QLGX\qlgx_bin` (repo `qlgx_bin`).

## Phát hành phiên bản mới

**Đọc `QUY_TRINH_PHAT_HANH.md` trước khi làm bất cứ việc gì liên quan tới phát hành.**
Tài liệu đó có đủ các bước, các lệnh kiểm chứng, và phụ lục ghi lại những cái bẫy đã làm
hỏng bản phát hành thật.

Tóm tắt: sửa số phiên bản trong ba file → chạy `release.ps1` → kiểm chứng độc lập →
commit và đẩy cả hai kho → đưa lên máy chủ theo đúng thứ tự.

Bốn điều tuyệt đối không được quên:

1. **Số phiên bản phải tăng mỗi lần phát hành.** Windows Installer chỉ chép đè file khi
   file mới có số phiên bản lớn hơn. Không tăng thì máy người dùng cài xong vẫn chạy bản
   cũ mà không báo lỗi gì.
2. **Không bao giờ đưa `giaoxu.mdb` vào bộ cài hay gói cập nhật**, và không bao giờ gọi
   `unins000.exe` của bộ cài Inno cũ — nó xoá luôn dữ liệu giáo xứ.
3. **Không cài thử bộ cài lên máy người dùng khi chưa được cho phép.**
4. **Bước kiểm chứng phải biết báo lỗi.** Chạy thử nó với một file cũ để chắc chắn nó
   không phải lúc nào cũng báo "đạt".

## Vài điều hay vấp khi sửa mã

- Nhiều file `.cs` lưu **UTF-16LE**. Giữ nguyên bảng mã khi sửa.
- File `.ps1` có chữ tiếng Việt phải lưu **UTF-8 có BOM** hoặc **UTF-16LE có BOM**, nếu
  không PowerShell 5.1 đọc theo ANSI và làm hỏng dấu.
- Thao tác với file `.mdb` (Jet/ACE) phải chạy bằng PowerShell **32-bit**
  (`C:\Windows\SysWOW64\WindowsPowerShell\v1.0\powershell.exe`).
- Trong mảng PowerShell, nối chuỗi phải bọc ngoặc đơn: `,@('a','b', ($X + 'y'))`.
- Có thể có **phiên Claude khác làm việc song song** trên cùng thư mục này ở nhánh khác.
  Kiểm tra `git branch --show-current` trước khi làm; không `git checkout`, dùng
  `git worktree` khi cần commit sang nhánh khác.
