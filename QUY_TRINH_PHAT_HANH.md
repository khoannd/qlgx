# Quy trình phát hành QLGX

Tài liệu này dành cho **bất kỳ phiên Claude nào** được yêu cầu phát hành một phiên bản
mới. Làm đúng theo đây thì ra bản cài chạy được; bỏ qua bước nào cũng đã từng gây ra
một bản phát hành hỏng.

Phần lớn công việc đã tự động hoá trong `release.ps1`. Việc của phiên làm việc là:
tăng số phiên bản, viết ghi chú, chạy script, **kiểm chứng độc lập**, rồi commit và đẩy
lên GitHub.

---

## 0. Trước khi bắt đầu

### 0.1. Đọc phụ lục A trước

[Phụ lục A](#phụ-lục-a--những-cái-bẫy-đã-trả-giá) liệt kê các cái bẫy đã làm hỏng bản
phát hành thật. Mỗi cái đều **im lặng** — không báo lỗi, chỉ ra bản cài sai. Đọc hết
trước khi sửa bất cứ script nào.

### 0.2. Kiểm tra nhánh

Công việc phát hành thuộc nhánh `master`. Nhưng thư mục làm việc này có thể đang ở
nhánh khác vì **có phiên Claude khác làm việc song song trên cùng thư mục**
(nhánh `webapp-phase-1` cho bản web).

```bash
git branch --show-current
git status --short
```

- Nếu đang ở `master` và cây sạch → làm bình thường.
- Nếu đang ở nhánh khác, hoặc có thay đổi của người khác → **không được `git checkout`**.
  Dùng worktree riêng khi commit (xem [bước 4](#4-commit-và-đẩy-lên-github)).
  Việc build không phụ thuộc nhánh nên cứ build tại chỗ.

### 0.3. Đóng chương trình đang chạy

`release.ps1` sẽ ghi đè thư mục `BIN\`. Nếu `GiaoXu.exe` đang chạy thì build hỏng.
Script tự kiểm tra ở bước 3 và dừng lại; đừng dùng `-Force` để lách nếu chương trình
thật sự đang chạy — hỏi người dùng đóng lại.

---

## 1. Tăng số phiên bản và viết ghi chú

Sửa **ba** file bằng tay (hai file còn lại trong `Release\` do script tự chép):

| File | Sửa gì |
|---|---|
| `BIN\VersionConfig.xml` | `value`, `display`, `dateupdate`, thêm mục ghi chú mới |
| `Source\ChuongTrinh\VersionConfig.xml` | y hệt file trên |
| `BIN\help\thong_tin_cap_nhat.htm` | thêm mục ghi chú mới |

Hai file `VersionConfig.xml` **phải giống hệt nhau** — script kiểm tra ở bước 1 và dừng
nếu lệch.

### Quy tắc đánh số

- `value` bốn phần (`4.0.2.0`), `display` ba phần (`4.0.2`).
- **Luôn tăng số ở mỗi lần phát hành**, kể cả khi chỉ build lại. Số phiên bản này được
  ghi vào từng file chương trình; nếu không tăng, máy người dùng sẽ không nhận bản mới
  (xem [bẫy #1](#1-số-phiên-bản-file-không-tăng--máy-người-dùng-giữ-nguyên-bản-cũ)).

### Viết ghi chú cho ai

Người đọc là quý cha, quý sơ — phần lớn không rành máy tính. Viết theo việc họ **thấy**,
không theo việc mình **sửa**:

- ✅ "Bộ cài nay chỉ còn một file duy nhất, quý vị tải về rồi chạy là xong."
- ❌ "Gộp setup.exe và MSI bằng IExpress."

Không nhắc tới lỗi của các bản phát hành nội bộ chưa ai dùng — hỏi người dùng nếu không
chắc bản trước đã đến tay ai chưa.

### Cẩn thận với XML

`VersionConfig.xml` là XML. Ký tự `&` phải viết `&amp;`, `<` viết `&lt;`. Kiểm tra ngay:

```bash
powershell.exe -NoProfile -Command "[xml](Get-Content 'BIN\VersionConfig.xml' -Raw) | Out-Null; 'XML hop le'"
```

---

## 2. Chạy quy trình phát hành

```bash
cd "D:\Working\QLGX\Github"
powershell.exe -NoProfile -ExecutionPolicy Bypass -File release.ps1
```

Thêm `-Force` nếu cần bỏ qua cảnh báo git (thường gặp khi phiên khác đang làm trên cùng
thư mục). Các tham số khác: `-DryRun` (xem trước), `-SkipBuild`, `-SkipInstaller`.

### Các bước script tự làm

| Bước | Việc | Bỏ qua được không |
|---|---|---|
| 1 | Đọc số phiên bản, đối chiếu hai file VersionConfig | không |
| 2 | Kiểm tra git | cảnh báo thôi |
| 3 | Kiểm tra chương trình đang chạy | không |
| **3b** | **Ghi số phiên bản vào 7 dự án** (`dat_phien_ban_file.ps1`) | **TUYỆT ĐỐI KHÔNG** |
| 4 | Biên dịch solution + đọc lại số phiên bản thật của file đã build | không |
| 4b | Đưa `Template\`, `Resources\`, `help\` vào bộ cài | không |
| 5 | Sửa `.vdproj`: ProductVersion, **ProductCode mới**, giữ nguyên UpgradeCode | không |
| 6 | Build bộ cài qua COM của Visual Studio (ẩn cửa sổ) | không |
| 6a | Kiểm tra hai lối tắt tên `GiaoXu` và trỏ đúng `GiaoXu.exe` | không |
| 6b | Ghi tên sản phẩm/nhà sản xuất tiếng Việt vào MSI, đặt bảng mã 65001 | không |
| 6c | Thêm khả năng tự dò thư mục đã cài trước đó | không |
| 6d | Dịch giao diện bộ cài sang tiếng Việt | xem ghi chú dưới |
| 6e | Nạp bản điều khoản sử dụng | không |
| 6f | Dọn dấu vết bản Inno cũ | không |
| 7–8 | Gom file, tạo gói cập nhật `.zip` | không |
| 9 | Gộp `setup.exe` + `.msi` thành **một** file `.exe` | không |
| 9b | Chép sang `D:\Working\QLGX\qlgx_bin\Release` | không |

> **Bước 6d (tiếng Việt) tính đến 07-09-2026 chưa được nhìn tận mắt.** Bản dịch đã kiểm
> chứng ở mức dữ liệu (150 ô chữ, không ô nào rỗng, 756 ký tự có dấu, phông Tahoma) nhưng
> chưa ai xác nhận nó **hiện lên** đúng. Trước khi phát hành rộng rãi, phải nhờ người dùng
> chạy thử file `.exe` và xem màn hình. Nếu hỏng, gỡ bước 6d khỏi `release.ps1` là quay
> lại giao diện tiếng Anh nguyên bản.

Script dừng ngay khi có bước nào thất bại. **Không được "sửa cho qua"** bằng cách bỏ bước
kiểm chứng — mỗi bước kiểm chứng đều sinh ra từ một lần hỏng thật.

---

## 3. Kiểm chứng độc lập sau khi build

`release.ps1` tự kiểm chứng trong lúc chạy, nhưng **phải kiểm lại lần nữa** trên đúng file
sẽ phát hành. Đặt `$MSI` là `Source\GXInstaller\Release\qlgx_<x_y_z>.msi`.

```bash
MSI="D:\Working\QLGX\Github\Source\GXInstaller\Release\qlgx_4_0_2.msi"
cd "D:\Working\QLGX\Github"
powershell.exe -NoProfile -ExecutionPolicy Bypass -File kiem_tra_loi_tat.ps1            -Msi "$MSI"
powershell.exe -NoProfile -ExecutionPolicy Bypass -File them_do_tim_thu_muc_cu.ps1      -Msi "$MSI" -ChiKiemChung
powershell.exe -NoProfile -ExecutionPolicy Bypass -File dich_bo_cai_sang_tieng_viet.ps1 -Msi "$MSI" -ChiKiemChung
powershell.exe -NoProfile -ExecutionPolicy Bypass -File nap_dieu_khoan_su_dung.ps1      -Msi "$MSI" -ChiKiemChung
powershell.exe -NoProfile -ExecutionPolicy Bypass -File go_dau_vet_ban_cu.ps1           -Msi "$MSI" -ChiKiemChung
powershell.exe -NoProfile -ExecutionPolicy Bypass -File them_thu_muc_vao_bo_cai.ps1     -ChiKiemChung
powershell.exe -NoProfile -ExecutionPolicy Bypass -File dat_phien_ban_file.ps1          -PhienBan 4.0.2.0 -ChiKiemChung
powershell.exe -NoProfile -ExecutionPolicy Bypass -File ghi_ten_tieng_viet_vao_msi.ps1  -Msi "$MSI" -TenSanPham "QLGX - Quản Lý Giáo Xứ" -NhaSanXuat "Nguyễn Đức Khoan" -ChiKiemChung
```

Tất cả phải in `KIEM_CHUNG_DAT`.

### 3.1. Đối chứng ngược — bắt buộc

Một script kiểm chứng luôn báo "đạt" thì vô dụng. Đã có lần một script báo đạt trong khi
bộ cài hỏng hoàn toàn (xem [bẫy #3](#3-script-kiểm-chứng-báo-đạt-trong-khi-thật-ra-hỏng)).
Vì vậy phải chứng minh được rằng nó **biết báo lỗi**.

Cách chắc chắn và dùng được cho mọi script: chép file MSI ra một bản riêng, **cố tình phá
hỏng đúng một thứ**, rồi chạy lại script đó — nó phải báo lỗi.

```powershell
# ví dụ với them_do_tim_thu_muc_cu.ps1: xoá một dòng RegLocator rồi kiểm lại
$thu = "$env:TEMP\thu_pha.msi"
Copy-Item '...\qlgx_4_0_2.msi' $thu -Force
$i  = New-Object -ComObject WindowsInstaller.Installer
$db = $i.GetType().InvokeMember('OpenDatabase','InvokeMethod',$null,$i,@($thu,1))
$v  = $db.GetType().InvokeMember('OpenView','InvokeMethod',$null,$db,
        @("DELETE FROM ``RegLocator`` WHERE ``Signature_``='QLGX_InnoDir64'"))
$v.GetType().InvokeMember('Execute','InvokeMethod',$null,$v,$null) | Out-Null
$v.GetType().InvokeMember('Close','InvokeMethod',$null,$v,$null)   | Out-Null
$db.GetType().InvokeMember('Commit','InvokeMethod',$null,$db,$null) | Out-Null
```

Nếu script vẫn báo đạt thì nó đang nói dối — sửa nó trước khi tin bất cứ kết quả nào.

> Đừng dùng một bộ cài của bản phát hành cũ làm đối chứng rồi kết luận vội. Bản 4.0.0 đã
> có sẵn phần tự dò thư mục, nên `them_do_tim_thu_muc_cu.ps1` báo đạt trên file đó là
> **đúng**, không phải script hỏng. Đối chứng chỉ có nghĩa khi file đối chứng thật sự
> thiếu đúng cái đang kiểm.

### 3.2. Kiểm tra gói phát hành

- `qlgx_<x_y_z>.exe` giải nén ra đúng `setup.exe` và file `.msi` (mã băm khớp bản đã vá).
- Gói cập nhật `.zip` **không được** chứa `giaoxu.mdb`, `avatar.jpg`, `church.jpg` —
  đó là dữ liệu và ảnh của giáo xứ.
- Gói cập nhật **phải** chứa mọi thư viện vừa nâng cấp phiên bản. Xem danh sách
  `$UpdateExcludeFiles` trong `release.ps1`: nếu nâng cấp một thư viện nằm trong danh sách
  đó thì phải bỏ nó ra khỏi danh sách, nếu không máy người dùng giữ lại bản DLL cũ.

### 3.3. Cài thử — cần xin phép

Cách kiểm chứng chắc chắn nhất là cài thật vào một thư mục có sẵn `GiaoXu.exe` bản cũ rồi
xem có bị chép đè không. **Nhưng phải xin phép người dùng trước** — xem
[phụ lục B](#phụ-lục-b--quy-tắc-an-toàn).

---

## 4. Commit và đẩy lên GitHub

Hai kho:

| Kho | Đường dẫn | Chứa gì |
|---|---|---|
| `qlgx` | `D:\Working\QLGX\Github` | mã nguồn + `BIN\` + `Release\` |
| `qlgx_bin` | `D:\Working\QLGX\qlgx_bin` | file phát hành cho người dùng tải |

### Nếu thư mục làm việc đang ở nhánh khác

Không `git checkout`. Dùng worktree tạm:

```bash
cd "D:\Working\QLGX\Github"
git worktree add "C:/Users/khoannd/AppData/Local/Temp/qlgx_wt" master
# chép các file đã thay đổi (trừ những file thuộc phiên khác) sang worktree
# commit, push, tag trong worktree
git worktree remove "C:/Users/khoannd/AppData/Local/Temp/qlgx_wt"
```

Sau khi đẩy xong, nếu thư mục làm việc còn giữ đúng các thay đổi đó dưới dạng chưa commit,
hãy merge `master` vào nhánh đang dùng để cây sạch trở lại — đối chiếu mã băm
(`git hash-object` với `git rev-parse master:<file>`) để chắc chắn nội dung giống nhau
trước khi bỏ đi bản trong thư mục làm việc.

### Nhánh và thẻ

```bash
git push origin master
git push origin master:net48          # net48 luôn đi cùng master
git tag -a release-<x.y.z> -m "QLGX <x.y.z> - <tóm tắt>"
git push origin release-<x.y.z>
```

Nhánh `net20` giữ nguyên bản .NET 2.0 cho người dùng Windows 7 — **không đụng tới**.

Làm y hệt cho kho `qlgx_bin` (chỉ có `master` và thẻ).

> Mạng tới GitHub thỉnh thoảng rớt giữa chừng. Nếu `push` báo
> `Failed to connect to github.com port 443`, chỉ cần chạy lại — không phải lỗi gì khác.

---

## 5. Đưa lên máy chủ

Máy chủ phải phục vụ đúng bốn đường dẫn mà phần mềm gọi tới, cùng các đường dẫn cũ của
những đời phần mềm trước. Xem `HOP_DONG_MAY_CHU_CAP_NHAT.md` — tài liệu đó mô tả đầy đủ
hợp đồng giữa phần mềm và backend, kể cả cái bẫy so sánh số phiên bản bằng chuỗi.

`release.ps1` **không** tự tải lên. Bước 10 của script in ra thứ tự bắt buộc:

1. `qlgx_<x_y_z>_update.zip` → thư mục ghi trong `downloadpath`
2. `thong_tin_cap_nhat.htm` → `/help/`
3. `qlgx_<x_y_z>.exe` → `/download/`
4. `VersionConfig.xml` → thư mục gốc — **làm cuối cùng**

Bước 4 phải cuối vì chương trình của người dùng đọc file này để biết có bản mới. Đưa nó
lên trước khi gói zip sẵn sàng thì người dùng nhận thông báo có bản mới rồi tải thất bại.

---

## Phụ lục A — Những cái bẫy đã trả giá

### 1. Số phiên bản file không tăng → máy người dùng giữ nguyên bản cũ

**Triệu chứng:** cài bộ cài mới đè lên máy đang dùng, bộ cài chạy xong bình thường, không
báo lỗi gì, nhưng mở chương trình lên vẫn là bản cũ, không có chức năng mới nào.

**Nguyên nhân:** Windows Installer chép đè một file **chỉ khi** file trong bộ cài có số
phiên bản lớn hơn file đang có trên máy. Bằng nhau hoặc cũ hơn thì giữ nguyên file cũ.
`GiaoXu.exe` để nguyên `AssemblyFileVersion = 1.0.0.3` suốt nhiều năm nên không bao giờ
được chép đè. Bộ cài Inno Setup cũ không dính vì nó dùng cờ `ignoreversion`.

**Cách tránh:** bước 3b của `release.ps1` (`dat_phien_ban_file.ps1`) ghi số phiên bản phát
hành vào `AssemblyFileVersion` của 7 dự án, rồi sau khi build đọc lại số phiên bản **thật**
của file trong `BIN\`. Không bao giờ bỏ bước này.

`AutoComplete.dll` và `TabStrip.dll` không nằm trong danh sách vì không do solution này
build ra. Nếu sau này có thay chúng, phải build lại với số phiên bản cao hơn.

### 2. Nối chuỗi trong mảng PowerShell mất chữ

**Triệu chứng:** toàn bộ màn hình cài đặt hiện ra trắng trơn, không có chữ nào.

**Nguyên nhân:** trong mảng, PowerShell tách `$PHONG + 'chữ'` thành **hai** phần tử riêng.
Dòng đó thành 4 phần tử, phần tử thứ ba chỉ còn thẻ phông rỗng, và bộ cài ghi chuỗi trống
vào mọi nút.

```powershell
,@('WelcomeForm','NextButton', ($F_THUONG + '&Tiếp >'))    # ĐÚNG
,@('WelcomeForm','NextButton',  $F_THUONG + '&Tiếp >' )    # SAI
```

Cùng họ với cái bẫy này: `@( @(...) \n @(...) )` viết các mảng con cách nhau bằng xuống
dòng mà **không có dấu phẩy đầu dòng** thì PowerShell duỗi phẳng hết thành một danh sách
chuỗi rời rạc.

**Cách tránh:** `dich_bo_cai_sang_tieng_viet.ps1` tự đếm số phần tử mỗi dòng khi khởi động.

### 3. Script kiểm chứng báo "đạt" trong khi thật ra hỏng

**Triệu chứng:** mọi bước kiểm chứng đều xanh, bản phát hành vẫn hỏng.

**Nguyên nhân:** script ghi chuỗi rỗng vào MSI rồi đọc lại cũng ra chuỗi rỗng, so sánh
thấy bằng nhau nên báo đạt.

**Cách tránh:** kiểm chứng phải đòi hỏi **tính chất tuyệt đối**, không chỉ so sánh hai đầu
với nhau — ví dụ "chữ đọc ra từ MSI phải còn ít nhất 100 ký tự có dấu tiếng Việt". Và luôn
chạy [đối chứng ngược](#31-đối-chứng-ngược--bắt-buộc).

### 4. Hàm PowerShell trả về mọi thứ, không chỉ giá trị sau `return`

**Triệu chứng:** Windows Installer báo `Execute,Params` hoặc `OpenView,Sql` rất khó lần ra.

**Nguyên nhân:** mọi lời gọi không được dẫn vào `Out-Null` bên trong hàm đều lọt vào giá
trị trả về, biến một bản ghi thành một mảng.

**Cách tránh:** trong hàm, mọi `InvokeMember(...)` không dùng kết quả đều phải `| Out-Null`.

### 5. Bảng mã của file `.ps1`

**Triệu chứng:** chữ tiếng Việt trong script thành ký tự lạ, hoặc PowerShell báo
`The ampersand (&) character is not allowed`.

**Nguyên nhân:** PowerShell 5.1 đọc file `.ps1` **không có BOM** theo bảng mã ANSI.

**Cách tránh:** mọi `.ps1` có chữ tiếng Việt phải lưu **UTF-8 có BOM** hoặc **UTF-16LE có
BOM**. Kiểm tra: `head -c 3 <file> | xxd` phải thấy `efbbbf`.

Nhiều file `.cs` của dự án lưu **UTF-16LE** — khi sửa phải giữ nguyên bảng mã, đừng ghi đè
bằng UTF-8.

### 6. Xuống dòng của file `.vdproj`

**Triệu chứng:** `them_thu_muc_vao_bo_cai.ps1` báo `khong tim thay khoi Folders cua
Application Folder`.

**Nguyên nhân:** file `.vdproj` bị đổi sang xuống dòng kiểu Unix (LF) qua tay git, sed hoặc
chính Visual Studio. Lệnh tách dòng theo CRLF trả về **một** dòng khổng lồ.

**Cách tránh:** script tự chuẩn hoá về CRLF khi đọc. Ngoài ra, đường dẫn trong `.vdproj`
phải viết `\\` — một dấu `\` làm build hỏng mà không báo lỗi rõ ràng.

### 7. Giới hạn của SQL trong Windows Installer

- Không có `LIKE`. Muốn xoá theo mẫu thì phải xoá theo tên chính xác từng cái.
- `UPDATE` bắt buộc có `WHERE` chỉ rõ khoá chính; không cập nhật cả bảng một lúc được.
- Truyền giá trị bằng dấu `?` kèm một bản ghi (`CreateRecord`), không nhét thẳng chuỗi.
- Sau khi `Commit`, **không mở lại được** file MSI trong cùng tiến trình. Vì vậy mỗi script
  vá MSI được gọi bằng một tiến trình `powershell.exe` riêng.

### 8. Jet/ACE chỉ có bản 32-bit

Mọi thao tác đọc ghi `.mdb` phải chạy bằng PowerShell 32-bit:

```bash
"C:/Windows/SysWOW64/WindowsPowerShell/v1.0/powershell.exe" -NoProfile -File <script>
```

PowerShell 64-bit sẽ báo `provider is not registered on the local machine`.

Tương tự, `GiaoXu.exe` là assembly x86 nên không nạp được bằng .NET 64-bit.

### 9. Không bao giờ gọi trình gỡ cài đặt của Inno

Bộ cài Inno cũ chép nguyên `Release\Temp\*` vào máy, nghĩa là `giaoxu.mdb` cũng do nó cài.
Chạy `unins000.exe` sẽ **xoá luôn dữ liệu giáo xứ**. Bước 6f chỉ xoá khoá đăng ký và các
lối tắt cũ, tuyệt đối không đụng tới file trong thư mục cài đặt.

### 10. ProductCode và UpgradeCode

- **ProductCode phải đổi mỗi lần build**, nếu không Windows báo
  *"This product is already installed"* khi cài đè.
- **UpgradeCode phải giữ nguyên** (`{E09DA5C1-9121-453D-80FC-0E9B83B030CF}`), nếu không
  đường nâng cấp hỏng và máy người dùng sẽ có hai bản song song.

Bước 5 của `release.ps1` tự làm và tự kiểm tra lại cả hai.

---

## Phụ lục B — Quy tắc an toàn

Người dùng là các cha xứ, dữ liệu là sổ sách giáo xứ nhiều năm. Mất là không lấy lại được.

1. **Không cài bộ cài lên máy người dùng khi chưa được cho phép.** Đã từng có lần chạy thử
   bộ cài để chụp màn hình và nó cài thẳng vào `D:\QuanLyGiaoXu` — thư mục thật — để lại
   một mục thừa trong danh sách phần mềm của Windows. Nếu cần cài thử, phải hỏi trước, và
   sao lưu `giaoxu.mdb` cùng thư mục `Images\`, `backup\` trước khi làm.

2. **Không gỡ cài đặt bất cứ thứ gì** nếu không được yêu cầu rõ ràng. Gỡ có thể xoá file
   chương trình của bản đang dùng.

3. **`giaoxu.mdb` không bao giờ nằm trong bộ cài hay gói cập nhật.** Kiểm tra lại mỗi lần.
   `avatar.jpg` và `church.jpg` là ảnh của giáo xứ — có trong bộ cài để máy mới có ảnh mặc
   định, nhưng phải nằm ngoài gói cập nhật.

4. **Build phải im lặng.** Không mở giao diện Visual Studio. `release.ps1` điều khiển bằng
   COM và ẩn cửa sổ.

5. **Không `git checkout` khi phiên khác đang dùng thư mục.** Dùng worktree.

---

## Phụ lục C — Những gì chưa kiểm chứng được

Ghi lại để phiên sau không tưởng nhầm là đã xong:

- **Nhánh dò registry của bản Inno cũ** (bước 6c) và **bước dọn dấu vết** (bước 6f) chưa
  chạy thử trên máy thật có bản Inno 3.3.7. Tạo khoá trong `HKLM` để giả lập cần quyền
  Administrator.
- **Giao diện cài đặt tiếng Việt** (bước 6d) chưa được nhìn tận mắt — xem ghi chú ở
  [bước 2](#2-chạy-quy-trình-phát-hành).
- **Bộ cài chưa ký số**, nên SmartScreen của Edge và Windows vẫn cảnh báo khi tải và chạy.
