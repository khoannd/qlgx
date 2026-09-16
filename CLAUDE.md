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
| `WebApp\` | bản viết lại phần mềm desktop thành web app, nhánh riêng — **không đụng tới khi phát hành bản desktop** |
| `landing\` | trang **quanlygiaoxu.net** thật (Next.js, chạy trên Cloudflare Workers) — gồm cả trang giới thiệu VÀ API máy chủ cập nhật mà chính phần mềm desktop tự gọi (`/capnhat/*`) |
| `*.ps1` ở thư mục gốc | các script của quy trình phát hành |

Kho nhị phân cho người dùng tải nằm ở `D:\Working\QLGX\qlgx_bin` (repo `qlgx_bin`).

`quanlygiaoxu.net` không còn là hosting cũ — từ 07-09-2026 là Worker Cloudflare
(`landing/`, xem `landing/README.md`). Đừng nói tới việc "tải file lên FTP/hosting" nữa.

## Phát hành phiên bản mới

**Đọc `QUY_TRINH_PHAT_HANH.md` trước khi làm bất cứ việc gì liên quan tới phát hành.**
Tài liệu đó có đủ các bước, các lệnh kiểm chứng, và phụ lục ghi lại những cái bẫy đã làm
hỏng bản phát hành thật.

Tóm tắt: sửa số phiên bản trong ba file → chạy `release.ps1` → kiểm chứng độc lập →
commit và đẩy cả hai kho (`qlgx` và `qlgx_bin`). Máy chủ cập nhật (`quanlygiaoxu.net/capnhat/*`)
đọc thẳng từ GitHub, **tự lên trong vài phút sau khi push — không cần thao tác gì thêm**.
Riêng nội dung marketing trên trang chủ (`landing/`) vẫn phải sửa tay và deploy riêng —
xem `QUY_TRINH_PHAT_HANH.md` mục 5.2.

Năm điều tuyệt đối không được quên:

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
   nhật được nữa. Xem `HOP_DONG_MAY_CHU_CAP_NHAT.md`.

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

## Cách làm việc: commit, review, và "thế nào là xong"

**Commit theo từng nhóm thay đổi** — mỗi task, mỗi phase trong kế hoạch là một commit riêng,
đừng dồn cả chục việc vào một commit khổng lồ. Kho dễ theo dõi và **review code mới khả thi**.
Đang có phiên khác làm cùng thư mục thì `git add` từng file cụ thể của mình, tuyệt đối không
`git add -A`/`git add .` (sẽ kéo theo việc dở dang của người khác).

**Review code sau mỗi task/phase**, không để dồn tới cuối.

**Một kế hoạch chỉ coi là XONG khi đủ hai điều kiện:**

1. **Đã kiểm thử qua trình duyệt thật** những chức năng kiểm được trên trình duyệt — không chỉ
   chạy test tự động, không chỉ gọi API bằng `curl`. Nhiều lỗi chỉ lộ ra khi bấm bằng tay
   (xem lỗi "in cả gia đình ra giấy trắng" và "luôn báo Chưa có thay đổi" — cả hai đều xanh
   hết mọi bài test tự động mà vẫn hỏng với người dùng thật).
2. **Đã review code toàn bộ nhánh** ở bước cuối cùng.

## Bản web (WebApp/) — những thứ hay mất thời gian dò lại

- **CSDL phát triển tên là `qlgx_thu`** (không phải `qlgx`). Chuỗi kết nối thường dùng:
  `Host=localhost;Database=qlgx_thu;Username=postgres;Password=<mật khẩu cục bộ>`.
  Các database `qlgx_api_*`, `qlgx_test_*`, `qlgx_data_*` là của test tự sinh, đừng đụng vào.
- **Máy chủ API dev chạy ở cổng 5096**, giao diện Vite ở **5173** (Vite proxy `/api` sang 5096).
- Khi chạy API bằng tay cần ba biến môi trường: `ConnectionStrings__Qlgx`,
  `Qlgx__JwtKey` (**phải là chuỗi Base-64 hợp lệ**, không phải văn bản thường — sai thì mọi lượt
  đăng nhập trả 500 với lỗi `FormatException`), và `ASPNETCORE_URLS`. Muốn API tự áp migration
  lúc khởi động thì thêm `Qlgx__ChayMigrationKhiKhoiDong=true` — **không bật cờ này thì bảng mới
  không bao giờ được tạo** dù migration đã có trong mã.
- **Đổi `Qlgx__JwtKey` làm mọi phiên đăng nhập hiện có hết hiệu lực** (kể cả của phiên Claude
  khác đang thử nghiệm) — cân nhắc trước khi khởi động lại API bằng khoá mới.
- **Khi máy chủ dev đang chạy, nó khoá `bin/Debug`**, nên `dotnet build`/`dotnet test`/`dotnet ef`
  sẽ đỏ vì không chép đè được DLL. Cách đi vòng (KHÔNG cần dừng máy chủ của phiên khác): build và
  test ra thư mục riêng — `dotnet build <csproj> -o <thư mục tạm>` và `dotnet test -o <thư mục tạm khác>`.
  `dotnet ef migrations add` không đi vòng được như vậy: khi bị khoá thì viết tay file migration
  cùng file `.Designer.cs` (chép từ migration gần nhất, đổi tên lớp và thuộc tính `[Migration]`).
- Đọc nội dung PDF để kiểm chứng bản in: dùng PyMuPDF (`import fitz`), và nhớ đặt
  `PYTHONIOENCODING=utf-8` khi in ra màn hình, nếu không console Windows (cp1252) sẽ ném
  `UnicodeEncodeError` với chữ tiếng Việt.
- Hai bảng `mau_in_tuy_chinh` và `cach_hien_thi_dung_sai` có `giao_xu_id` **cho phép NULL** với
  nghĩa "dòng cấp hệ thống, dùng chung cho mọi giáo xứ". Chúng **không** nằm trong bộ lọc toàn
  cục của EF (phải lọc tay, tường minh ở service) và dùng **policy RLS riêng nhận biết NULL** —
  policy chung `giao_xu_id::text = current_setting(...)` sẽ giấu mất dòng dùng chung.
