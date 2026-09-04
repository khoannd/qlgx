<#
====================================================================================
 Gộp bộ cài thành MỘT file .exe duy nhất
====================================================================================

 VÌ SAO CẦN:
 Visual Studio Installer Projects sinh ra HAI file phải đi cùng nhau:
     setup.exe          vỏ mỏng, lo cài .NET 4.8 nếu máy thiếu
     qlgx_x_y_z.msi     phần cài đặt thật
 setup.exe tham chiếu CỨNG tên file .msi và tìm nó trong cùng thư mục. Nếu người
 dùng chỉ tải mỗi file .exe về rồi chạy thì sẽ báo lỗi không tìm thấy bộ cài.

 Người dùng của phần mềm này phần lớn là các cha xứ, không rành máy tính, nên
 bắt tải hai file rồi phải để cùng thư mục là chắc chắn hỏng. Bản Inno Setup cũ
 chỉ có một file duy nhất, cần giữ đúng trải nghiệm đó.

 CÁCH LÀM:
 Dùng IExpress (có sẵn trong mọi bản Windows) đóng gói cả hai file vào một file
 .exe tự giải nén. Khi chạy, nó bung hai file ra thư mục tạm rồi gọi setup.exe,
 lúc đó file .msi nằm ngay cạnh nên mọi thứ hoạt động bình thường.

 CÁCH DÙNG:
     .\gop_bo_cai_thanh_1_file.ps1 -ThuMucNguon "...\GXInstaller\Release" `
                                   -TenFileMsi  "qlgx_4_0_0.msi" `
                                   -FileDich    "...\Release\qlgx_4_0_0.exe"
====================================================================================
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][string]$ThuMucNguon,
    [Parameter(Mandatory = $true)][string]$TenFileMsi,
    [Parameter(Mandatory = $true)][string]$FileDich,
    [string]$TenHienThi = 'QLGX - Quan Ly Giao Xu'
)

$ErrorActionPreference = 'Stop'

$setupExe = Join-Path $ThuMucNguon 'setup.exe'
$msiFile  = Join-Path $ThuMucNguon $TenFileMsi

if (-not (Test-Path $setupExe)) { Write-Host "LOI: khong tim thay $setupExe"; exit 1 }
if (-not (Test-Path $msiFile))  { Write-Host "LOI: khong tim thay $msiFile";  exit 1 }

if (Test-Path $FileDich) { Remove-Item $FileDich -Force }

# IExpress doi thu muc nguon phai co dau \ o cuoi
$nguon = $ThuMucNguon.TrimEnd('\') + '\'
$sed = Join-Path ([IO.Path]::GetTempPath()) 'qlgx_gop.sed'

# Ghi chu ve cac tuy chon quan trong:
#   ShowInstallProgramWindow=0  an cua so dong lenh khi chay setup.exe
#   HideExtractAnimation=1      an hoat anh giai nen cho gon
#   InsideCompressed=0          nen noi dung lai cho nho
#   AppLaunched=setup.exe       file duoc chay sau khi giai nen xong
#   PostInstallCmd=<None>       khong chay gi them sau do
$noiDung = @"
[Version]
Class=IEXPRESS
SEDVersion=3
[Options]
PackagePurpose=InstallApp
ShowInstallProgramWindow=0
HideExtractAnimation=1
UseLongFileName=1
InsideCompressed=0
CAB_FixedSize=0
CAB_ResvCodeSigning=0
RebootMode=N
InstallPrompt=%InstallPrompt%
DisplayLicense=%DisplayLicense%
FinishMessage=%FinishMessage%
TargetName=%TargetName%
FriendlyName=%FriendlyName%
AppLaunched=%AppLaunched%
PostInstallCmd=%PostInstallCmd%
AdminQuietInstCmd=%AdminQuietInstCmd%
UserQuietInstCmd=%UserQuietInstCmd%
SourceFiles=SourceFiles
[Strings]
InstallPrompt=
DisplayLicense=
FinishMessage=
TargetName=$FileDich
FriendlyName=$TenHienThi
AppLaunched=setup.exe
PostInstallCmd=<None>
AdminQuietInstCmd=
UserQuietInstCmd=
FILE0="setup.exe"
FILE1="$TenFileMsi"
[SourceFiles]
SourceFiles0=$nguon
[SourceFiles0]
%FILE0%=
%FILE1%=
"@

[IO.File]::WriteAllText($sed, $noiDung, (New-Object Text.ASCIIEncoding))

& "$env:WINDIR\System32\iexpress.exe" /N /Q $sed | Out-Null

# IExpress chay khong dong bo, phai cho file xuat hien
for ($i = 1; $i -le 30; $i++) {
    if (Test-Path $FileDich) { Start-Sleep -Milliseconds 500; break }
    Start-Sleep -Milliseconds 500
}
Remove-Item $sed -Force -ErrorAction SilentlyContinue

if (-not (Test-Path $FileDich)) { Write-Host 'LOI: IExpress khong tao duoc file gop'; exit 1 }

# Kiem chung: giai nen thu ra thu muc tam va doi chieu du hai file chua
$thuMucThu = Join-Path ([IO.Path]::GetTempPath()) ('qlgx_kiemtra_' + [guid]::NewGuid().ToString('N').Substring(0, 8))
New-Item -ItemType Directory -Path $thuMucThu -Force | Out-Null
& $FileDich "/T:$thuMucThu" /C | Out-Null
Start-Sleep -Seconds 2

$coSetup = Test-Path (Join-Path $thuMucThu 'setup.exe')
$coMsi   = Test-Path (Join-Path $thuMucThu $TenFileMsi)
Remove-Item $thuMucThu -Recurse -Force -ErrorAction SilentlyContinue

if (-not $coSetup) { Write-Host 'LOI: file gop thieu setup.exe'; exit 1 }
if (-not $coMsi)   { Write-Host "LOI: file gop thieu $TenFileMsi"; exit 1 }

$mb = [math]::Round((Get-Item $FileDich).Length / 1MB, 2)
Write-Host "DA_GOP_XONG $mb"
exit 0
