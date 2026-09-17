# QLGX — Quản Lý Giáo Xứ

Phần mềm quản lý giáo xứ, dùng miễn phí trong nhiều giáo phận Việt Nam. Người dùng là quý
cha, quý sơ — phần lớn không rành máy tính. Dữ liệu là sổ sách giáo xứ nhiều năm, mất là
không lấy lại được. Mọi quyết định kỹ thuật đều phải ưu tiên hai điều đó.

Trả lời người dùng bằng **tiếng Việt**.

**Máy này (`D:\Working\QLGX`) là máy dev.** Dữ liệu giáo xứ đang có trong phần mềm quản
lý giáo xứ trên máy này (file `.mdb`, bản cài thử...) là dữ liệu giáo xứ gửi về để test
hoặc dữ liệu tự tạo để test — không có người thật đang làm việc trên phần mềm ở máy này.
Được phép cài đặt/gỡ/chạy thử bộ cài, kể cả bộ cài bản cũ, ngay trên máy này để kiểm thử
(khác hẳn với "máy người dùng thật" nói ở mục dưới — mục đó nói tới máy của giáo xứ khác,
không phải máy này).

**Không `git push` (kể cả tạo/đẩy thẻ) khi chưa được người dùng đồng ý rõ ràng cho lần
đó.** Được phép `git commit` cục bộ tuỳ ý, kể cả nhiều commit liên tiếp; cứ chuẩn bị sẵn
commit rồi hỏi trước khi đẩy lên GitHub. Áp dụng cho cả hai kho (`qlgx` và `qlgx_bin`),
mọi nhánh.

**Không tạo `git worktree` mới khi chưa được người dùng đồng ý rõ ràng cho lần đó** — kể
cả khi mục đích là để commit sang nhánh khác (xem mục "phiên Claude khác" bên dưới). Hỏi
trước, nói rõ sẽ tạo worktree ở đâu và để làm gì.

**Không được làm bất cứ điều gì ảnh hưởng tới người dùng đang chạy phần mềm, cho tới
khi được duyệt để phát hành rộng rãi.** Cụ thể:
- Được phép chạy `scripts\release.ps1` để build và tự kiểm thử cục bộ (xem
  `docs/QUY_TRINH_PHAT_HANH.md`) — build ra file nằm trên máy này, chưa ai tải được.
- **Không `git push`** file build đó lên `qlgx`/`qlgx_bin` (đây là kho mà máy chủ cập
  nhật đọc thẳng — push tức là phát hành cho toàn bộ người dùng đang có phần mềm).
- Không sửa `VersionConfig.xml`/`version.txt` trên máy chủ thật (`landing/`) hay bất cứ
  gì khiến máy người dùng tưởng có bản mới.
- Không cài thử bộ cài lên máy người dùng thật khi chưa được cho phép (đã có ở dưới).
- Hỏi rõ ràng và chờ được đồng ý trước khi làm bất cứ bước nào trong các bước trên.

## Bố cục

| Thư mục | Nội dung |
|---|---|
| `Source\` | mã nguồn WinForms, .NET Framework 4.8, x86 (solution `Source\GiaoXu.sln`) |
| `BIN\` | thư mục build ra, cũng là bộ file chạy được |
| `Source\GXInstaller\` | dự án bộ cài (`.vdproj`, Visual Studio Installer Projects) |
| `Release\` | file phát hành đã ký nhận vào kho |
| `WebApp\` | bản viết lại phần mềm desktop thành web app, nhánh riêng — **không đụng tới khi phát hành bản desktop** |
| `landing\` | trang **quanlygiaoxu.net** thật (Next.js, chạy trên Cloudflare Workers) — gồm cả trang giới thiệu VÀ API máy chủ cập nhật mà chính phần mềm desktop tự gọi (`/capnhat/*`) |
| `scripts\` | các script PowerShell của quy trình phát hành (`release.ps1` và các script nó gọi) |
| `docs\` | tài liệu vận hành (`QUY_TRINH_PHAT_HANH.md`, `HOP_DONG_MAY_CHU_CAP_NHAT.md`, `PROMPT_VIET_API_CAP_NHAT.md`) |

Kho nhị phân cho người dùng tải nằm ở `D:\Working\QLGX\qlgx_bin` (repo `qlgx_bin`).

`quanlygiaoxu.net` không còn là hosting cũ — từ 07-09-2026 là Worker Cloudflare
(`landing/`, xem `landing/README.md`). Đừng nói tới việc "tải file lên FTP/hosting" nữa.

## Phát hành phiên bản mới

**Đọc `docs/QUY_TRINH_PHAT_HANH.md` trước khi làm bất cứ việc gì liên quan tới phát hành.**
Tài liệu đó có đủ các bước, các lệnh kiểm chứng, và phụ lục ghi lại những cái bẫy đã làm
hỏng bản phát hành thật.

Tóm tắt: sửa số phiên bản trong ba file → chạy `scripts\release.ps1` → kiểm chứng độc lập →
commit và đẩy cả hai kho (`qlgx` và `qlgx_bin`). Máy chủ cập nhật (`quanlygiaoxu.net/capnhat/*`)
đọc thẳng từ GitHub, **tự lên trong vài phút sau khi push — không cần thao tác gì thêm**.
Riêng nội dung marketing trên trang chủ (`landing/`) vẫn phải sửa tay và deploy riêng —
xem `docs/QUY_TRINH_PHAT_HANH.md` mục 5.2.

Sáu điều tuyệt đối không được quên:

1. **Số phiên bản phải tăng mỗi lần phát hành.** Windows Installer chỉ chép đè file khi
   file mới có số phiên bản lớn hơn. Không tăng thì máy người dùng cài xong vẫn chạy bản
   cũ mà không báo lỗi gì.
2. **Không bao giờ đưa `giaoxu.mdb` vào bộ cài hay gói cập nhật**, và không bao giờ gọi
   `unins000.exe` của bộ cài Inno cũ — nó xoá luôn dữ liệu giáo xứ.
3. **Không cài thử bộ cài lên máy người dùng khi chưa được cho phép.**
4. **Bước kiểm chứng phải biết báo lỗi.** Chạy thử nó với một file cũ để chắc chắn nó
   không phải lúc nào cũng báo "đạt".
5. **Không bao giờ xoá hay đổi các đường dẫn cũ của máy chủ cập nhật** (`/version.txt`,
   `/VersionConfig.xml`, `/download.asp`, `/help/thong_tin_cap_nhat.htm`, và bản sao dưới
   `/4.0/`), và các đường dẫn đó **phải luôn trả lời được qua `http://` thuần**, không
   được ép sang `https`. Máy chạy bản 3.3.7 trở về trước (phần lớn người dùng) và bản
   4.0.0–4.0.1 nằm cứng các địa chỉ này — hỏng chỗ nào là máy đó vĩnh viễn không tự cập
   nhật được nữa. Xem `docs/HOP_DONG_MAY_CHU_CAP_NHAT.md`.
6. **Bộ cài phải chặn (không tự động đóng) khi thấy `GiaoXu.exe` đang chạy trên máy
   người dùng.** Cài đè lên khi chương trình đang mở làm Windows Installer hoãn thay
   các file bị khoá tới lần khởi động lại máy — bộ cài vẫn báo "thành công" nhưng
   chương trình sau đó không mở lên được (đã xảy ra thật, xem
   `docs/QUY_TRINH_PHAT_HANH.md` bẫy #11). Không tự kill tiến trình để tránh làm hỏng
   `giaoxu.mdb` đang mở.

## Vài điều hay vấp khi sửa mã

- Nhiều file `.cs` lưu **UTF-16LE**. Giữ nguyên bảng mã khi sửa.
- File `.ps1` có chữ tiếng Việt phải lưu **UTF-8 có BOM** hoặc **UTF-16LE có BOM**, nếu
  không PowerShell 5.1 đọc theo ANSI và làm hỏng dấu.
- Thao tác với file `.mdb` (Jet/ACE) phải chạy bằng PowerShell **32-bit**
  (`C:\Windows\SysWOW64\WindowsPowerShell\v1.0\powershell.exe`).
- Trong mảng PowerShell, nối chuỗi phải bọc ngoặc đơn: `,@('a','b', ($X + 'y'))`.
- Custom Action của MSI có script nằm ngay trong Target: **Type 37 = JScript, Type 38 =
  VBScript** — rất dễ nhầm. Và `On Error Resume Next` nuốt cả `Err.Raise` của chính
  mình, nên muốn huỷ cài đặt phải `On Error Goto 0` trước khi gọi `Err.Raise`.
- Có thể có **phiên Claude khác làm việc song song** trên cùng thư mục này ở nhánh khác.
  Kiểm tra `git branch --show-current` trước khi làm; không `git checkout`. Khi cần commit
  sang nhánh khác, dùng `git worktree` NHƯNG phải xin phép trước (xem mục ở đầu file) —
  worktree tạo ra vẫn dùng chung `.git` với phiên kia, tạo/xoá worktree không đúng lúc có
  thể ảnh hưởng tới việc họ đang làm.
- **Ảnh chụp màn hình khi test (Playwright, kiểm thử web...) phải lưu vào thư mục
  `/screenshots/`**, không được để rớt ra thẳng thư mục gốc của repo — thư mục gốc đã nhiều
  lần bị vương vãi ảnh chụp màn hình của các lần test trước. `/screenshots/` đã được đưa vào
  `.gitignore`.
- Vì có nhiều phiên chạy song song, **các thay đổi kiểu migration** (schema D1/SQL, đổi
  cấu trúc file cấu hình dùng chung, đổi hợp đồng API giữa `landing/` và phần mềm desktop,
  ...) cần kiểm tra kỹ xem phiên khác có đang đụng vào cùng chỗ không, để tránh xung đột
  hoặc migrate chồng lên nhau. Nếu cần hỏi thêm thông tin hoặc phối hợp với phiên khác
  (ví dụ phiên đang làm `WebApp/` trên nhánh `webapp-phase-1`), có thể chủ động liên lạc
  qua các công cụ giao tiếp phiên (`ListAgents`/gửi tin nhắn) thay vì đoán hoặc làm liều.
