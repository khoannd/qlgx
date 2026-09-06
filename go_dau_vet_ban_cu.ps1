<#
====================================================================================
 Dọn dấu vết của bản cài cũ (Inno Setup) khi cài bản mới
====================================================================================

 VẤN ĐỀ:
 Bản 3.3.7 trở về trước được cài bằng Inno Setup, đăng ký với Windows dưới tên
 "GiaoXu". Bản 4.0.0 cài bằng Windows Installer, đăng ký dưới tên
 "QLGX - Quản Lý Giáo Xứ". Cài bản mới đè lên máy cũ thì trong "Apps & features"
 hiện ra HAI phần mềm, trên Desktop có HAI biểu tượng - người dùng không biết cái
 nào là thật.

 CÁCH LÀM VÀ LÝ DO KHÔNG GỌI TRÌNH GỠ CÀI ĐẶT CŨ:
 Cách hiển nhiên là chạy unins000.exe của Inno. NHƯNG bộ cài Inno cũ chép nguyên
 thư mục Release\Temp\* vào máy, nghĩa là giaoxu.mdb cũng do nó cài. Gỡ bằng
 unins000.exe sẽ XOÁ LUÔN giaoxu.mdb - tức là xoá sạch dữ liệu giáo xứ của người
 dùng. Với người dùng của chương trình này thì đó là mất mát không cứu được.

 Nên ở đây chỉ xoá những gì làm người dùng bối rối, tuyệt đối không đụng tới file
 trong thư mục cài đặt:
   1. Khoá đăng ký gỡ cài đặt của Inno  -> "Apps & features" chỉ còn một phần mềm
   2. Nhóm Start Menu "GiaoXu" của Inno -> Start Menu chỉ còn một mục
   3. Lối tắt Desktop "GiaoXu" của Inno -> Desktop chỉ còn một biểu tượng
 File unins000.exe còn nằm lại trong thư mục nhưng vô hại vì không ai thấy nó.

 Bản mới đặt tên lối tắt đúng bằng "GiaoXu" như bản cũ, nên sau khi cài xong người
 dùng thấy y hệt như trước.

 VÌ SAO PHẢI DÙNG CUSTOM ACTION CHỨ KHÔNG DÙNG BẢNG RemoveRegistry:
 Gói cài là 32-bit, nên mọi thao tác registry qua bảng RemoveRegistry đều bị
 Windows chuyển hướng sang nhánh WOW6432Node. Khoá của Inno lại nằm ở nhánh
 64-bit. Chỉ có cách gọi reg.exe với tham số /reg:64 mới với tới được.

 CÁCH DÙNG:
     .\go_dau_vet_ban_cu.ps1 -Msi "...\qlgx_4_0_0.msi"
     .\go_dau_vet_ban_cu.ps1 -Msi "...\qlgx_4_0_0.msi" -ChiKiemChung
====================================================================================
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][string]$Msi,
    [switch]$ChiKiemChung
)

$ErrorActionPreference = 'Stop'

$TEN_CA      = 'QLGX_GoDauVetBanCu'
$KHOA_INNO   = 'HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{695008D3-7C22-4103-9CA6-107D525C82C5}_is1'
$MSITRANSACT = 1

# Kieu custom action = 37 + 1024 + 2048 + 64
#   37   : ma VBScript nam ngay trong cot Target
#   1024 : hoan lai (deferred) - chay trong luc cai that su, khong phai luc chuan bi
#   2048 : khong mao danh (no impersonate) - can quyen he thong de xoa khoa HKLM
#   64   : gap loi thi bo qua, cai tiep - don dep that bai khong duoc lam hong ca ban cai
$KIEU_CA = 37 + 1024 + 2048 + 64

# Chay sau InstallInitialize (1500) va TRUOC CreateShortcuts (4500). Neu chay sau
# thi se xoa nham chinh loi tat vua tao, vi ban moi cung dat ten la "GiaoXu".
$THU_TU = 1550

# Chi don khi that su tim thay ban cu. Hai thuoc tinh nay do them_do_tim_thu_muc_cu.ps1
# tao ra, mang gia tri la thu muc cai dat cu doc tu registry cua Inno.
$DIEU_KIEN = 'QLGXPREVDIR OR QLGXPREVDIR64'

$VBS = @"
Dim sh, fso, duongDan
Set sh = CreateObject("WScript.Shell")
Set fso = CreateObject("Scripting.FileSystemObject")
On Error Resume Next
sh.Run "reg.exe delete ""$KHOA_INNO"" /f /reg:64", 0, True
sh.Run "reg.exe delete ""$KHOA_INNO"" /f /reg:32", 0, True
duongDan = sh.ExpandEnvironmentStrings("%ALLUSERSPROFILE%") & "\Microsoft\Windows\Start Menu\Programs\GiaoXu"
If fso.FolderExists(duongDan) Then fso.DeleteFolder duongDan, True
duongDan = sh.ExpandEnvironmentStrings("%PUBLIC%") & "\Desktop\GiaoXu.lnk"
If fso.FileExists(duongDan) Then fso.DeleteFile duongDan, True
"@

function MoDb($duongDan, $cheDo) {
    $script:installer = New-Object -ComObject WindowsInstaller.Installer
    return $script:installer.GetType().InvokeMember('OpenDatabase', 'InvokeMethod', $null, $script:installer, @($duongDan, $cheDo))
}
function DongDb($db) {
    [Runtime.InteropServices.Marshal]::ReleaseComObject($db) | Out-Null
    [Runtime.InteropServices.Marshal]::ReleaseComObject($script:installer) | Out-Null
    [GC]::Collect(); [GC]::WaitForPendingFinalizers()
}
function ChaySql($db, [string]$sql, [object[]]$thamSo) {
    $v = $db.GetType().InvokeMember('OpenView', 'InvokeMethod', $null, $db, @($sql))
    if ($null -eq $thamSo) {
        $v.GetType().InvokeMember('Execute', 'InvokeMethod', $null, $v, $null) | Out-Null
    } else {
        $r = $script:installer.GetType().InvokeMember('CreateRecord', 'InvokeMethod', $null, $script:installer, @($thamSo.Count))
        for ($k = 0; $k -lt $thamSo.Count; $k++) {
            if ($thamSo[$k] -is [int]) {
                $r.GetType().InvokeMember('IntegerData', 'SetProperty', $null, $r, @(($k + 1), $thamSo[$k])) | Out-Null
            } else {
                $r.GetType().InvokeMember('StringData', 'SetProperty', $null, $r, @(($k + 1), [string]$thamSo[$k])) | Out-Null
            }
        }
        $v.GetType().InvokeMember('Execute', 'InvokeMethod', $null, $v, @($r)) | Out-Null
    }
    $v.GetType().InvokeMember('Close', 'InvokeMethod', $null, $v, $null) | Out-Null
}
function DocMotO($db, [string]$sql) {
    $v = $db.GetType().InvokeMember('OpenView', 'InvokeMethod', $null, $db, @($sql))
    $v.GetType().InvokeMember('Execute', 'InvokeMethod', $null, $v, $null) | Out-Null
    $r = $v.GetType().InvokeMember('Fetch', 'InvokeMethod', $null, $v, $null)
    $kq = $null
    if ($null -ne $r) { $kq = [string]$r.GetType().InvokeMember('StringData', 'GetProperty', $null, $r, @(1)) }
    $v.GetType().InvokeMember('Close', 'InvokeMethod', $null, $v, $null) | Out-Null
    return $kq
}

if (-not (Test-Path $Msi)) { Write-Host "LOI: khong tim thay $Msi"; exit 1 }

# ---------------------------------------------------------------- Kiem chung
if ($ChiKiemChung) {
    $db = MoDb $Msi 0
    $thieu = @()
    $ma = DocMotO $db "SELECT ``Type`` FROM ``CustomAction`` WHERE ``Action``='$TEN_CA'"
    if ($ma -ne "$KIEU_CA") { $thieu += "CustomAction $TEN_CA (kieu doc duoc: '$ma', can '$KIEU_CA')" }
    $ma = DocMotO $db "SELECT ``Target`` FROM ``CustomAction`` WHERE ``Action``='$TEN_CA'"
    if (-not $ma -or -not $ma.Contains('695008D3')) { $thieu += 'noi dung VBScript' }
    $dk = DocMotO $db "SELECT ``Condition`` FROM ``InstallExecuteSequence`` WHERE ``Action``='$TEN_CA'"
    if ($dk -ne $DIEU_KIEN) { $thieu += "dieu kien chay (doc duoc: '$dk')" }
    $tt = DocMotO $db "SELECT ``Sequence`` FROM ``InstallExecuteSequence`` WHERE ``Action``='$TEN_CA'"
    if ($tt -ne "$THU_TU") { $thieu += "thu tu chay (doc duoc: '$tt')" }
    # Phai chay TRUOC CreateShortcuts, neu khong se xoa nham loi tat vua tao
    $ttLoiTat = DocMotO $db "SELECT ``Sequence`` FROM ``InstallExecuteSequence`` WHERE ``Action``='CreateShortcuts'"
    if ($ttLoiTat -and [int]$ttLoiTat -le $THU_TU) { $thieu += "chay sau CreateShortcuts ($ttLoiTat) - se xoa nham loi tat moi" }
    DongDb $db
    if ($thieu.Count -gt 0) {
        Write-Host 'LOI: thieu hoac sai cac muc sau:'
        foreach ($t in $thieu) { Write-Host "  - $t" }
        exit 1
    }
    Write-Host 'KIEM_CHUNG_DAT'
    exit 0
}

# ----------------------------------------------------------------------- Ghi
$db = MoDb $Msi $MSITRANSACT

# Xoa dong cu neu chay lai lan hai. SQL cua Windows Installer khong ho tro LIKE
# nen phai xoa theo ten chinh xac.
foreach ($bang in 'CustomAction', 'InstallExecuteSequence') {
    ChaySql $db "DELETE FROM ``$bang`` WHERE ``Action``='$TEN_CA'" $null
}

ChaySql $db 'INSERT INTO `CustomAction` (`Action`,`Type`,`Source`,`Target`) VALUES (?,?,?,?)' `
    @($TEN_CA, [int]$KIEU_CA, '', $VBS)
ChaySql $db 'INSERT INTO `InstallExecuteSequence` (`Action`,`Condition`,`Sequence`) VALUES (?,?,?)' `
    @($TEN_CA, $DIEU_KIEN, [int]$THU_TU)

$db.GetType().InvokeMember('Commit', 'InvokeMethod', $null, $db, $null) | Out-Null
DongDb $db

Write-Host "Da them buoc don dau vet ban cu (chay o thu tu $THU_TU, dieu kien: $DIEU_KIEN)"
