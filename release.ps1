<#
====================================================================================
 QLGX - Script tự động hoá quy trình phát hành
====================================================================================

 Thay thế release.bat cũ (dùng Inno Setup). Bản này dùng Visual Studio Installer
 Projects (GXInstaller.vdproj).

 CÁCH DÙNG:
     1. Sửa BIN\VersionConfig.xml trước: cập nhật value, display, dateupdate và
        thêm mục mô tả thay đổi cho phiên bản mới. Đây là NGUỒN DUY NHẤT xác định
        số phiên bản; script đọc từ đây ra, không nhận tham số version riêng để
        tránh lệch số giữa file cấu hình và bộ cài.
     2. Sửa BIN\help\thong_tin_cap_nhat.htm cho khớp.
     3. Chạy:   .\release.ps1
        Xem trước không ghi gì:   .\release.ps1 -DryRun

 KẾT QUẢ (thư mục Release\):
     qlgx_<ver>.exe          bộ cài đầy đủ (kèm bootstrapper tự cài .NET 4.8)
     qlgx_<ver>.msi          gói MSI thuần
     qlgx_<ver>_update.zip   gói cập nhật cho AutoUpdate.exe tải về
     VersionConfig.xml       file cấu hình để đưa lên server
     thong_tin_cap_nhat.htm  trang lịch sử cập nhật để đưa lên server

 Script KHÔNG tự upload lên server. Việc đưa file lên mạng do bạn tự làm sau khi
 đã kiểm tra, giống như release.bat cũ (các lệnh FTP trong đó cũng chỉ để echo).
====================================================================================
#>

[CmdletBinding()]
param(
    [switch]$DryRun,          # Xem trước, không sửa file, không build
    [switch]$SkipBuild,       # Bỏ qua bước biên dịch (dùng lại BIN hiện có)
    [switch]$SkipInstaller,   # Chỉ tạo gói cập nhật, không build bộ cài
    [switch]$Force            # Bỏ qua cảnh báo git và cảnh báo app đang chạy
)

$ErrorActionPreference = 'Stop'

# ------------------------------------------------------------------ Đường dẫn
$Root       = Split-Path -Parent $MyInvocation.MyCommand.Path
$Source     = Join-Path $Root 'Source'
$Bin        = Join-Path $Root 'BIN'
$Sln        = Join-Path $Source 'GiaoXu.sln'
$VdprojRel  = 'GXInstaller\GXInstaller.vdproj'
$Vdproj     = Join-Path $Source $VdprojRel
$InstOutDir = Join-Path $Source 'GXInstaller\Release'
$ReleaseDir = Join-Path $Root 'Release'
$StageDir   = Join-Path $ReleaseDir '_staging'

$MSBuild = 'C:\Program Files\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\MSBuild.exe'

# --------------------------------------------- Thông tin nhà phát hành / sản phẩm
# Các giá trị này được ghi vào bộ cài mỗi lần phát hành, hiện lên ở:
#   - Hộp thoại cài đặt
#   - Mục "Apps & features" / "Programs and Features" của Windows
#   - Thuộc tính file khi bấm chuột phải > Properties
# MSI lấy trường Author từ Manufacturer, nên đặt Manufacturer là đủ cho cả hai.
$TenSanPham  = 'QLGX - Quản Lý Giáo Xứ'
$NhaSanXuat  = 'Nguyễn Đức Khoan'
$TrangChu    = 'https://quanlygiaoxu.net'

# Bản KHÔNG DẤU, chỉ dùng lúc build.
#
# Vì sao phải có hai bản: Visual Studio Installer Projects không xử lý được
# tiếng Việt có dấu trong file .vdproj. Đã thử cả ba bảng mã và đều hỏng:
#     1252  (Tây Âu)   -> nuốt mất chữ ả, ứ  => "Qun Ly Giao X"
#     1258  (Việt Nam) -> biến thành ký tự rác U+007F
#     65001 (UTF-8)    -> build thất bại hoàn toàn
# Nên cách làm là: build bằng tên không dấu cho chắc chắn, rồi ghi đè tiếng Việt
# có dấu thẳng vào file MSI sau khi build (xem hàm Ghi-TiengViet-VaoMSI).
$TenSanPhamAscii = 'QLGX - Quan Ly Giao Xu'
$NhaSanXuatAscii = 'Nguyen Duc Khoan'

# --------------------------------------------- Thư mục kho nhị phân (repo qlgx_bin)
# File cài và gói cập nhật được chép sang đây để đưa lên GitHub.
$BinRepo       = 'D:\Working\QLGX\qlgx_bin'
$BinRepoCai    = Join-Path $BinRepo 'Release'
$BinRepoUpdate = Join-Path $BinRepo 'Release\update'

# ------------------------------------------------ Danh sách file cho bộ cài đầy đủ
# Giữ đúng danh sách của release.bat cũ. Thêm file mới vào đây khi có.
$FullPackageFiles = @(
    'AutoComplete.dll'
    'AutoUpdate.exe'
    'AutoUpdate.exe.config'
    'avatar.jpg'
    'church.jpg'
    'ExcelReport.dll'
    'Giaoly.dll'
    'Giaoly.dll.config'
    'GiaoXu.exe'
    'GiaoXu.exe.config'
    'GXControl.dll'
    'GXGlobal.dll'
    'ICSharpCode.SharpZipLib.dll'
    'Janus.Data.v3.dll'
    'Janus.Windows.ButtonBar.v3.dll'
    'Janus.Windows.CalendarCombo.v3.dll'
    'Janus.Windows.Common.v3.dll'
    'Janus.Windows.ExplorerBar.v3.dll'
    'Janus.Windows.FilterEditor.v3.dll'
    'Janus.Windows.GridEX.v3.dll'
    'Janus.Windows.Ribbon.v3.dll'
    'Janus.Windows.Schedule.v3.dll'
    'Janus.Windows.TimeLine.v3.dll'
    'Janus.Windows.UI.v3.dll'
    'Microsoft.Office.Interop.Excel.dll'
    'Microsoft.Office.Interop.Word.dll'
    'Microsoft.Vbe.Interop.dll'
    'Newtonsoft.Json.dll'
    'Office.dll'
    'stdole.dll'
    'TabStrip.dll'
    'VersionConfig.xml'
    'vnConvert.dll'
)
$FullPackageFolders = @('help', 'Resources', 'Template')

# --------------------------------------- File LOẠI KHỎI gói cập nhật (không đưa vào zip)
#
#  QUAN TRỌNG: đây là các thư viện của bên thứ ba hầu như không bao giờ đổi, và các
#  file thuộc về dữ liệu người dùng. Chúng đã có sẵn trên máy user từ lần cài đầu,
#  nên không cần tải lại mỗi lần cập nhật (giữ gói zip nhỏ).
#
#  >>> NẾU SAU NÀY BẠN NÂNG CẤP MỘT THƯ VIỆN TRONG DANH SÁCH NÀY (ví dụ đổi phiên bản
#  >>> Janus hay Newtonsoft), PHẢI XOÁ NÓ KHỎI DANH SÁCH NÀY, nếu không máy user sẽ
#  >>> giữ lại bản DLL cũ và sinh lỗi rất khó lần ra.
#
#  giaoxu.mdb / avatar.jpg / church.jpg: dữ liệu và ảnh của giáo xứ,
#  TUYỆT ĐỐI không được ghi đè khi cập nhật.
#  AutoUpdate.exe: chính nó đang chạy lúc cập nhật nên không thể tự ghi đè.
$UpdateExcludeFiles = @(
    'ICSharpCode.SharpZipLib.dll'
    'Janus.Data.v3.dll'
    'Janus.Windows.ButtonBar.v3.dll'
    'Janus.Windows.CalendarCombo.v3.dll'
    'Janus.Windows.Common.v3.dll'
    'Janus.Windows.ExplorerBar.v3.dll'
    'Janus.Windows.FilterEditor.v3.dll'
    'Janus.Windows.GridEX.v3.dll'
    'Janus.Windows.Ribbon.v3.dll'
    'Janus.Windows.Schedule.v3.dll'
    'Janus.Windows.TimeLine.v3.dll'
    'Janus.Windows.UI.v3.dll'
    'Microsoft.Office.Interop.Excel.dll'
    'Microsoft.Office.Interop.Word.dll'
    'Microsoft.Vbe.Interop.dll'
    # 'Newtonsoft.Json.dll'   <-- DA BO: ban 4.0.0 nang tu v11 len v13, PHAI dua vao goi cap nhat
    'Office.dll'
    'stdole.dll'
    'AutoUpdate.exe'
    'AutoUpdate.exe.config'
    'giaoxu.mdb'
    'avatar.jpg'
    'church.jpg'
)

# ==================================================================== Hàm tiện ích
function Write-Buoc  ($m) { Write-Host "`n=== $m" -ForegroundColor Cyan }
function Write-Ok    ($m) { Write-Host "    [OK]  $m" -ForegroundColor Green }
function Write-Canh  ($m) { Write-Host "    [!]   $m" -ForegroundColor Yellow }
function Write-Loi   ($m) { Write-Host "    [LOI] $m" -ForegroundColor Red }

function Xac-Nhan ($cauHoi) {
    if ($Force) { return $true }
    $tl = Read-Host "$cauHoi (g/K)"
    return ($tl -eq 'g' -or $tl -eq 'G' -or $tl -eq 'y' -or $tl -eq 'Y')
}

# COM của Visual Studio hay trả về RPC_E_CALL_REJECTED khi đang bận -> bọc retry
function Invoke-Dte {
    param([scriptblock]$Action, [int]$RetrySeconds = 300)
    $deadline = (Get-Date).AddSeconds($RetrySeconds)
    while ($true) {
        try { return & $Action }
        catch {
            if ((Get-Date) -gt $deadline) { throw }
            Start-Sleep -Milliseconds 400
        }
    }
}

# Sau khi build, ép bảng mã của file MSI về UTF-8 rồi ghi lại tên sản phẩm và
# nhà sản xuất bằng tiếng Việt có dấu. Đây là cách duy nhất chạy được, vì bản
# thân Visual Studio Installer Projects không ghi được tiếng Việt (xem ghi chú
# ở phần khai báo $TenSanPhamAscii).
function Ghi-TiengViet-VaoMSI ($duongDanMsi, $tenSanPham, $nhaSanXuat) {
    $MSIMODIFY_UPDATE = 2
    $MSITRANSACT      = 1
    $installer = New-Object -ComObject WindowsInstaller.Installer
    $db = $installer.GetType().InvokeMember('OpenDatabase','InvokeMethod',$null,$installer,@($duongDanMsi, $MSITRANSACT))

    # Ép bảng mã sang UTF-8 bằng file .idt có chỉ thị _ForceCodepage
    $idtDir  = [IO.Path]::GetTempPath()
    $idtName = 'qlgx_forcecp.idt'
    $noiDung = "`r`n`r`n65001`t_ForceCodepage`r`n"
    [IO.File]::WriteAllText((Join-Path $idtDir $idtName), $noiDung, (New-Object Text.ASCIIEncoding))
    $db.GetType().InvokeMember('Import','InvokeMethod',$null,$db,@($idtDir, $idtName)) | Out-Null

    foreach ($cap in @(@('ProductName',$tenSanPham), @('Manufacturer',$nhaSanXuat))) {
        $view = $db.GetType().InvokeMember('OpenView','InvokeMethod',$null,$db,
                @("SELECT Property, Value FROM Property WHERE Property='$($cap[0])'"))
        $view.GetType().InvokeMember('Execute','InvokeMethod',$null,$view,$null) | Out-Null
        $rec = $view.GetType().InvokeMember('Fetch','InvokeMethod',$null,$view,$null)
        if ($null -ne $rec) {
            $rec.GetType().InvokeMember('StringData','SetProperty',$null,$rec,@(2, $cap[1])) | Out-Null
            $view.GetType().InvokeMember('Modify','InvokeMethod',$null,$view,@($MSIMODIFY_UPDATE, $rec)) | Out-Null
        }
        $view.GetType().InvokeMember('Close','InvokeMethod',$null,$view,$null) | Out-Null
    }
    $db.GetType().InvokeMember('Commit','InvokeMethod',$null,$db,$null) | Out-Null
    [System.Runtime.InteropServices.Marshal]::ReleaseComObject($db) | Out-Null
    [System.Runtime.InteropServices.Marshal]::ReleaseComObject($installer) | Out-Null
    [GC]::Collect(); [GC]::WaitForPendingFinalizers()
}

# Đọc ngược một thuộc tính từ file MSI (dùng để kiểm chứng sau khi ghi)
function Doc-ThuocTinhMSI ($duongDanMsi, $ten) {
    $i = New-Object -ComObject WindowsInstaller.Installer
    $db = $i.GetType().InvokeMember('OpenDatabase','InvokeMethod',$null,$i,@($duongDanMsi, 0))
    $v = $db.GetType().InvokeMember('OpenView','InvokeMethod',$null,$db,
         @("SELECT Value FROM Property WHERE Property='$ten'"))
    $v.GetType().InvokeMember('Execute','InvokeMethod',$null,$v,$null) | Out-Null
    $r = $v.GetType().InvokeMember('Fetch','InvokeMethod',$null,$v,$null)
    $kq = if ($null -ne $r) { [string]$r.GetType().InvokeMember('StringData','GetProperty',$null,$r,@(1)) } else { '' }
    $v.GetType().InvokeMember('Close','InvokeMethod',$null,$v,$null) | Out-Null
    [System.Runtime.InteropServices.Marshal]::ReleaseComObject($db) | Out-Null
    [System.Runtime.InteropServices.Marshal]::ReleaseComObject($i) | Out-Null
    [GC]::Collect(); [GC]::WaitForPendingFinalizers()
    return $kq
}

function Doc-FileGiuBom ($path) {
    $bytes = [IO.File]::ReadAllBytes($path)
    $coBom = ($bytes.Length -ge 3 -and $bytes[0] -eq 0xEF -and $bytes[1] -eq 0xBB -and $bytes[2] -eq 0xBF)
    $text  = [IO.File]::ReadAllText($path, (New-Object Text.UTF8Encoding($false)))
    return [PSCustomObject]@{ Text = $text; CoBom = $coBom }
}

function Ghi-FileGiuBom ($path, $text, $coBom) {
    [IO.File]::WriteAllText($path, $text, (New-Object Text.UTF8Encoding($coBom)))
}

# ==================================================================== BẮT ĐẦU
Write-Host "`n########################################################" -ForegroundColor White
Write-Host "#   QLGX - QUY TRINH PHAT HANH" -ForegroundColor White
Write-Host "########################################################" -ForegroundColor White
if ($DryRun) { Write-Canh "CHE DO XEM TRUOC (-DryRun): khong ghi file, khong build" }

# ---------------------------------------------------------- 1. Đọc số phiên bản
Write-Buoc '1. Doc so phien ban tu VersionConfig.xml'
$vcBin = Join-Path $Bin 'VersionConfig.xml'
$vcSrc = Join-Path $Source 'ChuongTrinh\VersionConfig.xml'
if (-not (Test-Path $vcBin)) { Write-Loi "Khong tim thay $vcBin"; exit 1 }

[xml]$xmlBin = Get-Content $vcBin -Raw -Encoding UTF8
$verValue   = $xmlBin.application.'version-info'.value      # vd 4.0.0.0
$verDisplay = $xmlBin.application.'version-info'.display    # vd 4.0.0
$verNgay    = $xmlBin.application.'version-info'.dateupdate

if ([string]::IsNullOrWhiteSpace($verDisplay)) { Write-Loi 'Thieu thuoc tinh display'; exit 1 }

# MSI chi doc 3 so dau cua ProductVersion
$parts = $verDisplay.Split('.')
if ($parts.Count -lt 3) { Write-Loi "display phai co dang X.Y.Z (dang la '$verDisplay')"; exit 1 }
$msiVersion  = "$($parts[0]).$($parts[1]).$($parts[2])"
$slug        = $msiVersion.Replace('.', '_')
$msiFileName = "qlgx_$slug.msi"

Write-Ok "value      = $verValue"
Write-Ok "display    = $verDisplay"
Write-Ok "dateupdate = $verNgay"
Write-Ok "ProductVersion cho MSI = $msiVersion"
Write-Ok "Ten file se tao        = qlgx_$slug.exe / qlgx_${slug}_update.zip"

# Canh bao neu 2 ban VersionConfig lech nhau
if (Test-Path $vcSrc) {
    [xml]$xmlSrc = Get-Content $vcSrc -Raw -Encoding UTF8
    if ($xmlSrc.application.'version-info'.value -ne $verValue) {
        Write-Canh "Source\ChuongTrinh\VersionConfig.xml co version khac: $($xmlSrc.application.'version-info'.value)"
        Write-Canh 'Luu y: build se COPY DE ban trong Source len BIN. Hai file nen giong nhau.'
        if (-not (Xac-Nhan 'Van tiep tuc?')) { exit 1 }
    } else { Write-Ok 'Hai ban VersionConfig.xml khop nhau' }
}

if (-not (Xac-Nhan "`nPhat hanh phien ban $verDisplay ?")) { Write-Canh 'Da huy'; exit 0 }

# ---------------------------------------------------------- 2. Kiểm tra git
Write-Buoc '2. Kiem tra git'
Push-Location $Root
try {
    $branch = (git rev-parse --abbrev-ref HEAD 2>$null)
    $dirty  = (git status --porcelain 2>$null)
    Write-Ok "Nhanh hien tai: $branch"
    if ($dirty) {
        Write-Canh 'Co thay doi CHUA COMMIT:'
        $dirty -split "`n" | Select-Object -First 15 | ForEach-Object { Write-Host "        $_" -ForegroundColor DarkYellow }
        if (-not (Xac-Nhan 'Van phat hanh voi cac thay doi chua commit?')) { exit 1 }
    } else { Write-Ok 'Cay lam viec sach' }
} finally { Pop-Location }

# ---------------------------------------------------------- 3. Đóng app đang chạy
Write-Buoc '3. Kiem tra chuong trinh dang chay'
$procs = Get-Process GiaoXu -ErrorAction SilentlyContinue
if ($procs) {
    Write-Canh "GiaoXu.exe dang chay (PID: $($procs.Id -join ', ')) - se lam build that bai vi DLL bi khoa"
    if (Xac-Nhan 'Dong chuong trinh ngay bay gio?') {
        if (-not $DryRun) { $procs | Stop-Process -Force; Start-Sleep -Seconds 2 }
        Write-Ok 'Da dong'
    } else { Write-Loi 'Khong the build khi chuong trinh dang chay'; exit 1 }
} else { Write-Ok 'Khong co tien trinh nao dang chay' }

# ---------------------------------------------------------- 4. Build Release
Write-Buoc '4. Bien dich toan bo solution (Release)'
if ($SkipBuild) { Write-Canh 'Bo qua theo tham so -SkipBuild' }
elseif ($DryRun) { Write-Canh 'DryRun: bo qua build' }
else {
    Write-Canh 'Luu y: build Release se GHI DE thu muc BIN\ (dung chung voi ban Debug)'
    & $MSBuild $Sln -p:Configuration=Release -p:Platform="Any CPU" -v:minimal -nologo
    if ($LASTEXITCODE -ne 0) { Write-Loi "MSBuild that bai (ma loi $LASTEXITCODE)"; exit 1 }
    Write-Ok 'Bien dich thanh cong'
}

# ------------------------------------------- 4b. Đưa các thư mục con vào bộ cài
Write-Buoc '4b. Dua Template / Resources / help vao bo cai'
if ($SkipInstaller -or $DryRun) { Write-Canh 'Bo qua' }
else {
    # Sinh lai tu noi dung BIN\ moi lan phat hanh, nen them bieu mau moi la tu co.
    $scriptTM = Join-Path $Root 'them_thu_muc_vao_bo_cai.ps1'
    if (-not (Test-Path $scriptTM)) { Write-Loi "Khong tim thay $scriptTM"; exit 1 }

    $kqTM = & powershell.exe -NoProfile -ExecutionPolicy Bypass -File $scriptTM 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Loi 'Dua thu muc vao bo cai that bai:'
        $kqTM | ForEach-Object { Write-Host "        $_" -ForegroundColor Red }
        exit 1
    }
    $kqTM | ForEach-Object { Write-Ok $_ }

    $kqTM2 = & powershell.exe -NoProfile -ExecutionPolicy Bypass -File $scriptTM -ChiKiemChung 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Loi 'Kiem chung danh sach thu muc that bai:'
        $kqTM2 | ForEach-Object { Write-Host "        $_" -ForegroundColor Red }
        exit 1
    }
}

# ---------------------------------------------------------- 5. Cập nhật vdproj
Write-Buoc '5. Cap nhat GXInstaller.vdproj'
$vd = Doc-FileGiuBom $Vdproj

# Doc gia tri hien tai
$curProductVersion = [regex]::Match($vd.Text, '"ProductVersion"\s*=\s*"8:([^"]*)"').Groups[1].Value
$curProductCode    = [regex]::Match($vd.Text, '"ProductCode"\s*=\s*"8:(\{[^}]*\})"').Groups[1].Value
$curUpgradeCode    = [regex]::Match($vd.Text, '"UpgradeCode"\s*=\s*"8:(\{[^}]*\})"').Groups[1].Value

Write-Host "        ProductVersion hien tai : $curProductVersion"
Write-Host "        ProductCode    hien tai : $curProductCode"
Write-Host "        UpgradeCode    hien tai : $curUpgradeCode"

if ([string]::IsNullOrWhiteSpace($curUpgradeCode)) { Write-Loi 'Khong doc duoc UpgradeCode'; exit 1 }

$newProductCode = '{' + ([guid]::NewGuid().ToString().ToUpper()) + '}'
Write-Ok "ProductVersion moi = $msiVersion"
Write-Ok "ProductCode    moi = $newProductCode  (bat buoc doi moi ban phat hanh)"
Write-Ok "UpgradeCode        = GIU NGUYEN (doi se lam hong nang cap cho user cu)"

if (-not $DryRun) {
    # Sao luu truoc khi sua
    $backup = "$Vdproj.bak_$(Get-Date -Format 'yyyyMMdd_HHmmss')"
    Copy-Item $Vdproj $backup
    Write-Ok "Da sao luu: $(Split-Path $backup -Leaf)"

    $moi = $vd.Text
    $moi = [regex]::Replace($moi, '("ProductVersion"\s*=\s*"8:)[^"]*(")', "`${1}$msiVersion`${2}")
    $moi = [regex]::Replace($moi, '("ProductCode"\s*=\s*"8:)\{[^}]*\}(")',  "`${1}$newProductCode`${2}")

    # Ghi thông tin nhà phát hành. Nếu không có, Windows hiện "Unknown publisher"
    # và mục gỡ cài đặt không có tên nhà sản xuất.
    $moi = [regex]::Replace($moi, '("ProductName"\s*=\s*"8:)[^"]*(")',  "`${1}$TenSanPhamAscii`${2}")
    $moi = [regex]::Replace($moi, '("Title"\s*=\s*"8:)[^"]*(")',        "`${1}$TenSanPhamAscii`${2}")
    $moi = [regex]::Replace($moi, '("Manufacturer"\s*=\s*"8:)[^"]*(")', "`${1}$NhaSanXuatAscii`${2}")
    $moi = [regex]::Replace($moi, '("ARPCONTACT"\s*=\s*"8:)[^"]*(")',   "`${1}$NhaSanXuatAscii`${2}")

    # Giữ bảng mã 1252 / tiếng Anh cho phần build. Tiếng Việt được ghi vào MSI
    # sau khi build xong (bước 6b).
    $moi = [regex]::Replace($moi, '("CodePage"\s*=\s*"3:)[^"]*(")',   "`${1}1252`${2}")
    $moi = [regex]::Replace($moi, '("LanguageId"\s*=\s*"3:)[^"]*(")', "`${1}1033`${2}")

    # Dat ten file MSI co kem version NGAY TU TRONG vdproj.
    # Bat buoc phai lam truoc khi build: bootstrapper setup.exe duoc sinh ra co
    # THAM CHIEU CUNG ten file MSI. Neu doi ten MSI sau khi build thi setup.exe
    # se khong tim thay file cai va bao loi tren may nguoi dung.
    # Chi doi cau hinh Release (gia tri bat dau bang "Release\\"), khong dung vao Debug.
    $moi = [regex]::Replace($moi, '("OutputFilename"\s*=\s*"8:Release\\\\)[^"]*(")', "`${1}$msiFileName`${2}")

    Ghi-FileGiuBom $Vdproj $moi $vd.CoBom

    # Kiem tra lai sau khi ghi - dac biet la UpgradeCode phai khong doi
    $kt = Doc-FileGiuBom $Vdproj
    $ktPV = [regex]::Match($kt.Text, '"ProductVersion"\s*=\s*"8:([^"]*)"').Groups[1].Value
    $ktPC = [regex]::Match($kt.Text, '"ProductCode"\s*=\s*"8:(\{[^}]*\})"').Groups[1].Value
    $ktUC = [regex]::Match($kt.Text, '"UpgradeCode"\s*=\s*"8:(\{[^}]*\})"').Groups[1].Value

    if ($ktPV -ne $msiVersion)     { Write-Loi "Ghi ProductVersion that bai (dang la '$ktPV')"; exit 1 }
    if ($ktPC -ne $newProductCode) { Write-Loi "Ghi ProductCode that bai"; exit 1 }
    if ($ktUC -ne $curUpgradeCode) { Write-Loi "UpgradeCode DA BI DOI! Khoi phuc tu $backup ngay"; exit 1 }

    $ktOut = [regex]::Match($kt.Text, '"OutputFilename"\s*=\s*"8:Release\\\\([^"]*)"').Groups[1].Value
    if ($ktOut -ne $msiFileName) { Write-Loi "Ghi OutputFilename that bai (dang la '$ktOut')"; exit 1 }

    $ktTen = [regex]::Match($kt.Text, '"ProductName"\s*=\s*"8:([^"]*)"').Groups[1].Value
    $ktNsx = [regex]::Match($kt.Text, '"Manufacturer"\s*=\s*"8:([^"]*)"').Groups[1].Value
    if ($ktTen -ne $TenSanPhamAscii) { Write-Loi "Ghi ProductName that bai"; exit 1 }
    if ($ktNsx -ne $NhaSanXuatAscii) { Write-Loi "Ghi Manufacturer that bai"; exit 1 }
    Write-Ok "Ten san pham (luc build) = $ktTen"
    Write-Ok "Nha san xuat (luc build) = $ktNsx"
    Write-Ok "Tieng Viet co dau se duoc ghi vao MSI o buoc 6b"
    Write-Ok "Ten file MSI se tao = $msiFileName"
    Write-Ok 'Da kiem tra lai: ProductVersion/ProductCode moi, UpgradeCode nguyen ven'
}

# ---------------------------------------------------------- 6. Build bộ cài
Write-Buoc '6. Build bo cai (im lang, khong mo giao dien)'
if ($SkipInstaller) { Write-Canh 'Bo qua theo tham so -SkipInstaller' }
elseif ($DryRun)    { Write-Canh 'DryRun: bo qua build bo cai' }
else {
    # Vi sao dung COM (DTE) chu khong dung devenv.com /Build:
    # devenv.com /Build voi .vdproj bao loi "HRESULT = 8000000A" (loi da biet cua
    # extension Installer Projects). Build qua giao dien thi chay duoc, nen o day
    # dieu khien dung code path do bang lap trinh va AN cua so di.
    $msiPath = Join-Path $InstOutDir $msiFileName
    $truoc = if (Test-Path $msiPath) { (Get-Item $msiPath).LastWriteTime } else { [datetime]::MinValue }

    Write-Host '        Dang khoi tao Visual Studio (an cua so)...'
    $dte = New-Object -ComObject 'VisualStudio.DTE.17.0'
    try {
        Invoke-Dte { $dte.MainWindow.Visible = $false }
        Invoke-Dte { $dte.SuppressUI = $true }
        Invoke-Dte { $dte.UserControl = $false }
        Invoke-Dte { $dte.Solution.Open($Sln) }

        $deadline = (Get-Date).AddSeconds(180)
        while ((Invoke-Dte { $dte.Solution.Projects.Count }) -lt 1 -and (Get-Date) -lt $deadline) {
            Start-Sleep -Milliseconds 500
        }

        Write-Host '        Dang build GXInstaller...'
        $sb = Invoke-Dte { $dte.Solution.SolutionBuild }
        Invoke-Dte { $sb.BuildProject('Release', $VdprojRel, $true) }
        $soLoi = Invoke-Dte { $sb.LastBuildInfo }

        if ($soLoi -ne 0) { Write-Loi "Build bo cai that bai ($soLoi project loi)"; exit 1 }

        $sau = if (Test-Path $msiPath) { (Get-Item $msiPath).LastWriteTime } else { [datetime]::MinValue }
        if ($sau -le $truoc) { Write-Loi 'MSI khong duoc tao moi - build co the da khong chay'; exit 1 }
        Write-Ok "Da tao bo cai luc $sau"

    }
    finally {
        try { Invoke-Dte { $dte.Solution.Close($false) } -RetrySeconds 30 } catch {}
        try { Invoke-Dte { $dte.Quit() } -RetrySeconds 30 } catch {}
        [System.Runtime.InteropServices.Marshal]::ReleaseComObject($dte) | Out-Null
    }
}

# ---------------------------------------------------------- 6a. Kiểm tra lối tắt
Write-Buoc '6a. Kiem tra loi tat trong bo cai'
if ($SkipInstaller -or $DryRun) { Write-Canh 'Bo qua' }
else {
    # Ban 4.0.0 co loi hai loi tat tro nham vao THU MUC chu khong phai GiaoXu.exe,
    # nguoi cai moi bam vao bieu tuong chi mo ra cua so Explorer. Kiem tra moi lan
    # phat hanh de khong tai dien.
    $msiPath = Join-Path $InstOutDir $msiFileName
    $scriptLt = Join-Path $Root 'kiem_tra_loi_tat.ps1'
    if (-not (Test-Path $scriptLt)) { Write-Loi "Khong tim thay $scriptLt"; exit 1 }

    $kqLt = & powershell.exe -NoProfile -ExecutionPolicy Bypass -File $scriptLt -Msi $msiPath 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Loi 'Loi tat trong bo cai khong dung:'
        $kqLt | ForEach-Object { Write-Host "        $_" -ForegroundColor Red }
        exit 1
    }
    $kqLt | ForEach-Object { Write-Ok $_ }
}

# ---------------------------------------------------------- 6b. Ghi tiếng Việt vào MSI
Write-Buoc '6b. Ghi ten tieng Viet co dau vao file MSI'
if ($SkipInstaller -or $DryRun) { Write-Canh 'Bo qua' }
else {
    $msiPath = Join-Path $InstOutDir $msiFileName

    # Gọi bằng TIẾN TRÌNH RIÊNG. Nếu gọi trong cùng tiến trình, COM của Windows
    # Installer báo lỗi "Type mismatch" do tiến trình này vừa dùng COM của
    # Visual Studio để build xong.
    $scriptGhi = Join-Path $Root 'ghi_ten_tieng_viet_vao_msi.ps1'
    if (-not (Test-Path $scriptGhi)) { Write-Loi "Khong tim thay $scriptGhi"; exit 1 }

    $kq = & powershell.exe -NoProfile -ExecutionPolicy Bypass -File $scriptGhi `
              -Msi $msiPath -TenSanPham $TenSanPham -NhaSanXuat $NhaSanXuat 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Loi 'Ghi tieng Viet vao MSI that bai:'
        $kq | ForEach-Object { Write-Host "        $_" -ForegroundColor Red }
        exit 1
    }
    Write-Ok "ProductName  = $TenSanPham"
    Write-Ok "Manufacturer = $NhaSanXuat  (dong thoi la Author cua goi MSI)"
    Write-Ok 'Da ghi tieng Viet vao MSI'

    # Kiem chung bang TIEN TRINH THU BA. Khong dung lai tien trinh vua ghi vi
    # sau khi Commit thi khong mo lai duoc file MSI trong cung tien trinh do.
    $kq2 = & powershell.exe -NoProfile -ExecutionPolicy Bypass -File $scriptGhi `
               -Msi $msiPath -TenSanPham $TenSanPham -NhaSanXuat $NhaSanXuat -ChiKiemChung 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Loi 'Kiem chung tieng Viet trong MSI that bai:'
        $kq2 | ForEach-Object { Write-Host "        $_" -ForegroundColor Red }
        exit 1
    }
    Write-Ok 'Da kiem chung lai bang tien trinh rieng: tieng Viet trong MSI dung'
}

# ---------------------------------------------------------- 6c. Dò thư mục đã cài trước đó
Write-Buoc '6c. Them kha nang tu do thu muc da cai truoc do'
if ($SkipInstaller -or $DryRun) { Write-Canh 'Bo qua' }
else {
    # Tu ban 4.0.0 thu muc mac dinh doi tu D: sang C: (nhieu may khong co o D:).
    # Nhung may DANG DUNG thi phai cai de len dung thu muc cu, neu khong nguoi dung
    # se thay du lieu trong tron. Buoc nay them vao MSI kha nang tu do thu muc cu.
    $scriptDo = Join-Path $Root 'them_do_tim_thu_muc_cu.ps1'
    if (-not (Test-Path $scriptDo)) { Write-Loi "Khong tim thay $scriptDo"; exit 1 }

    $kqDo = & powershell.exe -NoProfile -ExecutionPolicy Bypass -File $scriptDo -Msi $msiPath 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Loi 'Them phan do thu muc cu that bai:'
        $kqDo | ForEach-Object { Write-Host "        $_" -ForegroundColor Red }
        exit 1
    }
    $kqDo2 = & powershell.exe -NoProfile -ExecutionPolicy Bypass -File $scriptDo -Msi $msiPath -ChiKiemChung 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Loi 'Kiem chung phan do thu muc cu that bai:'
        $kqDo2 | ForEach-Object { Write-Host "        $_" -ForegroundColor Red }
        exit 1
    }
    Write-Ok 'Uu tien 1: thu muc trong registry cua ban Inno cu (ke ca khi user tu chon)'
    Write-Ok 'Uu tien 2: cung khoa do o nhanh registry 64-bit'
    Write-Ok 'Uu tien 3: D:\QuanLyGiaoXu neu con ton tai'
    Write-Ok 'Neu khong thay gi: dung mac dinh C:\QuanLyGiaoXu'
    Write-Ok 'Da ghi ARPINSTALLLOCATION de cac ban sau tu do duoc chinh minh'
}

# ------------------------------------------ 6d. Dịch giao diện bộ cài sang tiếng Việt
Write-Buoc '6d. Dich giao dien bo cai sang tieng Viet'
if ($SkipInstaller -or $DryRun) { Write-Canh 'Bo qua' }
else {
    # Visual Studio khong co san giao dien cai dat tieng Viet (khong co ma 1066),
    # nen phai ghi de chu tieng Viet vao file MSI sau khi build.
    $scriptDich = Join-Path $Root 'dich_bo_cai_sang_tieng_viet.ps1'
    if (-not (Test-Path $scriptDich)) { Write-Loi "Khong tim thay $scriptDich"; exit 1 }

    $kqDich = & powershell.exe -NoProfile -ExecutionPolicy Bypass -File $scriptDich -Msi $msiPath 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Loi 'Dich giao dien bo cai that bai:'
        $kqDich | ForEach-Object { Write-Host "        $_" -ForegroundColor Red }
        exit 1
    }
    $kqDich2 = & powershell.exe -NoProfile -ExecutionPolicy Bypass -File $scriptDich -Msi $msiPath -ChiKiemChung 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Loi 'Kiem chung ban dich that bai:'
        $kqDich2 | ForEach-Object { Write-Host "        $_" -ForegroundColor Red }
        exit 1
    }
    $kqDich | ForEach-Object { Write-Ok $_ }
    Write-Ok 'Da co man hinh Dieu khoan su dung nhu bo cai Inno cu'
}

# ------------------------------------------- 6e. Dọn dấu vết bản Inno cũ
Write-Buoc '6e. Don dau vet ban cai cu (Inno Setup)'
if ($SkipInstaller -or $DryRun) { Write-Canh 'Bo qua' }
else {
    # Xoa khoa dang ky va loi tat cua ban cu de may chi con MOT phan mem. Khong goi
    # unins000.exe vi no se xoa luon giaoxu.mdb - tuc la xoa sach du lieu giao xu.
    $scriptGo = Join-Path $Root 'go_dau_vet_ban_cu.ps1'
    if (-not (Test-Path $scriptGo)) { Write-Loi "Khong tim thay $scriptGo"; exit 1 }

    $kqGo = & powershell.exe -NoProfile -ExecutionPolicy Bypass -File $scriptGo -Msi $msiPath 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Loi 'Them buoc don dau vet ban cu that bai:'
        $kqGo | ForEach-Object { Write-Host "        $_" -ForegroundColor Red }
        exit 1
    }
    $kqGo2 = & powershell.exe -NoProfile -ExecutionPolicy Bypass -File $scriptGo -Msi $msiPath -ChiKiemChung 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Loi 'Kiem chung buoc don dau vet that bai:'
        $kqGo2 | ForEach-Object { Write-Host "        $_" -ForegroundColor Red }
        exit 1
    }
    Write-Ok 'Xoa khoa go cai dat cua Inno o ca hai nhanh registry'
    Write-Ok 'Xoa nhom Start Menu va loi tat Desktop cu ten GiaoXu'
    Write-Ok 'GIU NGUYEN moi file trong thu muc cai dat - khong dung toi giaoxu.mdb'
}

# ---------------------------------------------------------- 7. Gom file (staging)
Write-Buoc '7. Gom file cho goi cap nhat'
if ($DryRun) { Write-Canh 'DryRun: bo qua' }
else {
    if (Test-Path $StageDir) { Remove-Item $StageDir -Recurse -Force }
    New-Item -ItemType Directory -Path $StageDir -Force | Out-Null

    $thieu = @()
    foreach ($f in $FullPackageFiles) {
        $src = Join-Path $Bin $f
        if (Test-Path $src) { Copy-Item $src $StageDir -Force }
        else { $thieu += $f }
    }
    foreach ($d in $FullPackageFolders) {
        $src = Join-Path $Bin $d
        if (Test-Path $src) { Copy-Item $src $StageDir -Recurse -Force }
        else { $thieu += "$d\" }
    }

    if ($thieu.Count -gt 0) {
        Write-Canh "Khong tim thay trong BIN\: $($thieu -join ', ')"
        if (-not (Xac-Nhan 'Van tiep tuc dong goi?')) { exit 1 }
    }
    Write-Ok "Da gom $((Get-ChildItem $StageDir -Recurse -File).Count) file"

    # Loai cac file khong duoc dua vao goi cap nhat
    $daXoa = @()
    foreach ($f in $UpdateExcludeFiles) {
        $p = Join-Path $StageDir $f
        if (Test-Path $p) { Remove-Item $p -Force; $daXoa += $f }
    }
    Write-Ok "Da loai khoi goi cap nhat: $($daXoa.Count) file"
}

# ---------------------------------------------------------- 8. Tạo gói cập nhật
Write-Buoc '8. Tao goi cap nhat (.zip)'
$zipName = "qlgx_${slug}_update.zip"
$zipPath = Join-Path $ReleaseDir $zipName
if ($DryRun) { Write-Canh "DryRun: se tao $zipName" }
else {
    if (-not (Test-Path $ReleaseDir)) { New-Item -ItemType Directory -Path $ReleaseDir -Force | Out-Null }
    if (Test-Path $zipPath) { Remove-Item $zipPath -Force }
    Compress-Archive -Path (Join-Path $StageDir '*') -DestinationPath $zipPath -CompressionLevel Optimal
    Write-Ok "$zipName  ($([math]::Round((Get-Item $zipPath).Length/1MB,2)) MB)"
    Remove-Item $StageDir -Recurse -Force
}

# ---------------------------------------------------------- 9. Gom sản phẩm
Write-Buoc '9. Gom san pham phat hanh'
if ($DryRun) { Write-Canh 'DryRun: bo qua' }
else {
    # Gộp setup.exe và file .msi thành MỘT file .exe duy nhất.
    #
    # Bắt buộc phải gộp: setup.exe chỉ là vỏ mỏng, nó tham chiếu cứng tên file
    # .msi và tìm trong cùng thư mục. Nếu phát hành hai file riêng thì người
    # dùng (phần lớn là các cha xứ, không rành máy tính) rất dễ chỉ tải mỗi
    # file .exe rồi chạy và gặp lỗi không tìm thấy bộ cài.
    # Bản Inno Setup cũ cũng chỉ có một file, nên giữ đúng như vậy.
    $fileGop   = Join-Path $ReleaseDir "qlgx_$slug.exe"
    $scriptGop = Join-Path $Root 'gop_bo_cai_thanh_1_file.ps1'
    if (-not (Test-Path $scriptGop)) { Write-Loi "Khong tim thay $scriptGop"; exit 1 }

    if ($SkipInstaller) { Write-Canh 'Bo qua gop file vi -SkipInstaller' }
    else {
        $kqGop = & powershell.exe -NoProfile -ExecutionPolicy Bypass -File $scriptGop `
                     -ThuMucNguon $InstOutDir -TenFileMsi $msiFileName -FileDich $fileGop `
                     -TenHienThi $TenSanPhamAscii 2>&1
        if ($LASTEXITCODE -ne 0) {
            Write-Loi 'Gop bo cai thanh 1 file that bai:'
            $kqGop | ForEach-Object { Write-Host "        $_" -ForegroundColor Red }
            exit 1
        }
        $dungLuong = ($kqGop | Where-Object { $_ -like 'DA_GOP_XONG*' }) -replace 'DA_GOP_XONG ', ''
        Write-Ok "qlgx_$slug.exe   ($dungLuong MB) - MOT file duy nhat, nguoi dung chi can tai file nay"
        Write-Ok '  (da giai nen thu de kiem chung ben trong co du setup.exe va file .msi)'
    }

    Copy-Item $vcBin (Join-Path $ReleaseDir 'VersionConfig.xml') -Force
    Write-Ok 'VersionConfig.xml'

    $htm = Join-Path $Bin 'help\thong_tin_cap_nhat.htm'
    if (Test-Path $htm) {
        Copy-Item $htm (Join-Path $ReleaseDir 'thong_tin_cap_nhat.htm') -Force
        Write-Ok 'thong_tin_cap_nhat.htm'
    }
}

# ---------------------------------------------------------- 9b. Chép sang repo qlgx_bin
Write-Buoc '9b. Chep sang kho nhi phan (repo qlgx_bin)'
if ($DryRun) { Write-Canh "DryRun: se chep sang $BinRepoCai" }
elseif (-not (Test-Path $BinRepo)) { Write-Canh "Khong tim thay $BinRepo - bo qua buoc nay" }
else {
    if (-not (Test-Path $BinRepoUpdate)) { New-Item -ItemType Directory -Path $BinRepoUpdate -Force | Out-Null }

    # Chỉ chép file .exe đã gộp. Không chép file .msi riêng nữa vì nó đã nằm
    # bên trong file .exe rồi, để riêng chỉ làm người dùng phân vân tải cái nào.
    $nguonExe = Join-Path $ReleaseDir "qlgx_$slug.exe"
    if (Test-Path $nguonExe) { Copy-Item $nguonExe $BinRepoCai -Force; Write-Ok "Release\qlgx_$slug.exe" }
    $nguonZip = Join-Path $ReleaseDir $zipName
    if (Test-Path $nguonZip) { Copy-Item $nguonZip $BinRepoUpdate -Force; Write-Ok "Release\update\$zipName" }
    Write-Ok "Da chep sang $BinRepo"
    Write-Canh 'Nho commit va push repo qlgx_bin len GitHub'
}

# ---------------------------------------------------------- 10. Việc cần làm tiếp
Write-Buoc '10. VIEC CAN LAM TIEP (script khong tu upload)'
$dlPath = $xmlBin.application.'version-info'.downloadpath.'#text'
Write-Host @"

    Cac file da san sang trong: $ReleaseDir

    Dua len server theo dung thu tu sau:

      1. qlgx_${slug}_update.zip  ->  $dlPath
         (AutoUpdate.exe cua user se tai file nay ve)

      2. thong_tin_cap_nhat.htm   ->  /help/

      3. qlgx_$slug.exe           ->  /download/
         (MOT file duy nhat, da chua san bo cai ben trong)

      4. VersionConfig.xml        ->  thu muc goc  <<< LAM CUOI CUNG

    Vi sao VersionConfig.xml phai len sau cung: chuong trinh cua user doc file nay
    de biet co ban moi. Neu dua no len truoc khi goi zip san sang, user se nhan
    duoc thong bao co ban moi nhung tai ve that bai.

    NEN LAM TRUOC KHI PHAT HANH RONG RAI:
      - Cai thu qlgx_$slug.exe len mot may da co ban cu, kiem tra no nang cap
        duoc ma khong bao "This product is already installed".
      - Kiem tra chuc nang tu cap nhat tu ban cu len ban nay.

"@ -ForegroundColor Gray

Write-Host "########################################################" -ForegroundColor White
Write-Host "#   HOAN TAT - phien ban $verDisplay" -ForegroundColor White
Write-Host "########################################################`n" -ForegroundColor White
