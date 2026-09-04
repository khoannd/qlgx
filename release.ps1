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

# Bảng mã của gói MSI. BẮT BUỘC là 65001 (UTF-8) thì tiếng Việt có dấu mới hiện
# đúng. Giá trị mặc định 1252 (Tây Âu) sẽ làm hỏng dấu tiếng Việt.
$MaBangMaUTF8 = '65001'

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
    $moi = [regex]::Replace($moi, '("ProductName"\s*=\s*"8:)[^"]*(")',  "`${1}$TenSanPham`${2}")
    $moi = [regex]::Replace($moi, '("Title"\s*=\s*"8:)[^"]*(")',        "`${1}$TenSanPham`${2}")
    $moi = [regex]::Replace($moi, '("Manufacturer"\s*=\s*"8:)[^"]*(")', "`${1}$NhaSanXuat`${2}")
    $moi = [regex]::Replace($moi, '("ARPCONTACT"\s*=\s*"8:)[^"]*(")',   "`${1}$NhaSanXuat`${2}")

    # Đổi bảng mã sang UTF-8 để tiếng Việt có dấu không bị hỏng
    $moi = [regex]::Replace($moi, '("CodePage"\s*=\s*"3:)[^"]*(")', "`${1}$MaBangMaUTF8`${2}")

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
    $ktCp  = [regex]::Match($kt.Text, '"CodePage"\s*=\s*"3:([^"]*)"').Groups[1].Value
    if ($ktTen -ne $TenSanPham)   { Write-Loi "Ghi ProductName that bai"; exit 1 }
    if ($ktNsx -ne $NhaSanXuat)   { Write-Loi "Ghi Manufacturer that bai"; exit 1 }
    if ($ktCp  -ne $MaBangMaUTF8) { Write-Loi "Ghi CodePage that bai"; exit 1 }
    Write-Ok "Ten san pham   = $ktTen"
    Write-Ok "Nha san xuat   = $ktNsx  (dong thoi la Author cua goi MSI)"
    Write-Ok "Bang ma MSI    = $ktCp (UTF-8, de tieng Viet co dau khong bi hong)"
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

        # Doc nguoc tu chinh file MSI vua tao de chac chan thong tin ghi dung,
        # dac biet la tieng Viet co dau khong bi hong boi bang ma.
        try {
            $inst = New-Object -ComObject WindowsInstaller.Installer
            $db = $inst.GetType().InvokeMember('OpenDatabase','InvokeMethod',$null,$inst,@($msiPath,0))
            $view = $db.GetType().InvokeMember('OpenView','InvokeMethod',$null,$db,
                    @("SELECT Property, Value FROM Property WHERE Property='ProductName' OR Property='Manufacturer' OR Property='ProductVersion'"))
            $view.GetType().InvokeMember('Execute','InvokeMethod',$null,$view,$null) | Out-Null
            Write-Host '        --- Doc nguoc tu file MSI ---'
            while ($true) {
                $rec = $view.GetType().InvokeMember('Fetch','InvokeMethod',$null,$view,$null)
                if ($null -eq $rec) { break }
                $ten = [string]$rec.GetType().InvokeMember('StringData','GetProperty',$null,$rec,@(1))
                $gt  = [string]$rec.GetType().InvokeMember('StringData','GetProperty',$null,$rec,@(2))
                Write-Host ("            {0,-15} {1}" -f $ten, $gt)
            }
            $view.GetType().InvokeMember('Close','InvokeMethod',$null,$view,$null) | Out-Null
        } catch { Write-Canh "Khong doc nguoc duoc MSI de kiem tra: $($_.Exception.Message)" }
    }
    finally {
        try { Invoke-Dte { $dte.Solution.Close($false) } -RetrySeconds 30 } catch {}
        try { Invoke-Dte { $dte.Quit() } -RetrySeconds 30 } catch {}
        [System.Runtime.InteropServices.Marshal]::ReleaseComObject($dte) | Out-Null
    }
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
    # setup.exe va file MSI PHAI di cung nhau va giu dung ten, vi setup.exe
    # tham chieu ten MSI ben trong no.
    $srcExe = Join-Path $InstOutDir 'setup.exe'
    $srcMsi = Join-Path $InstOutDir $msiFileName
    if (Test-Path $srcExe) {
        Copy-Item $srcExe (Join-Path $ReleaseDir "qlgx_$slug.exe") -Force
        Write-Ok "qlgx_$slug.exe   (chay file nay de cai; tu cai .NET 4.8 neu may thieu)"
    } elseif (-not $SkipInstaller) { Write-Canh 'Khong tim thay setup.exe' }

    if (Test-Path $srcMsi) {
        Copy-Item $srcMsi (Join-Path $ReleaseDir $msiFileName) -Force
        Write-Ok "$msiFileName   (phai nam CUNG THU MUC voi file .exe o tren)"
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

    foreach ($ten in @("qlgx_$slug.exe", $msiFileName)) {
        $nguon = Join-Path $ReleaseDir $ten
        if (Test-Path $nguon) { Copy-Item $nguon $BinRepoCai -Force; Write-Ok "Release\$ten" }
    }
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
