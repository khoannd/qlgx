<#
====================================================================================
 Kiểm tra hai lối tắt trong bộ cài
====================================================================================

 VẤN ĐỀ ĐÃ GẶP:
 Trong bản 4.0.0, hai lối tắt (Desktop và Start Menu) trỏ nhầm vào THƯ MỤC chứ
 không phải vào GiaoXu.exe. Người cài mới bấm vào biểu tượng thì chỉ mở ra một
 cửa sổ Explorer, tưởng chương trình hỏng. Lỗi này nằm trong file .vdproj và
 không có gì báo lúc build, nên phải tự kiểm tra mỗi lần phát hành.

 Ngoài ra tên lối tắt phải là "GiaoXu" - đúng như các bản Inno Setup trước đây -
 để người dùng không thấy lạ khi nâng cấp.

 CÁCH DÙNG:
     .\kiem_tra_loi_tat.ps1 -Msi "...\qlgx_4_0_1.msi"
====================================================================================
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][string]$Msi
)

$ErrorActionPreference = 'Stop'

if (-not (Test-Path $Msi)) { Write-Host "LOI: khong tim thay $Msi"; exit 1 }

$i = New-Object -ComObject WindowsInstaller.Installer
$db = $i.GetType().InvokeMember('OpenDatabase', 'InvokeMethod', $null, $i, @($Msi, 0))
$v = $db.GetType().InvokeMember('OpenView', 'InvokeMethod', $null, $db, @('SELECT `Name`, `Target`, `Directory_` FROM `Shortcut`'))
$v.GetType().InvokeMember('Execute', 'InvokeMethod', $null, $v, $null) | Out-Null

$dsLoi = @()
$soLoiTat = 0
while ($true) {
    $r = $v.GetType().InvokeMember('Fetch', 'InvokeMethod', $null, $v, $null)
    if ($null -eq $r) { break }
    $soLoiTat++
    $ten  = [string]$r.GetType().InvokeMember('StringData', 'GetProperty', $null, $r, @(1))
    $dich = [string]$r.GetType().InvokeMember('StringData', 'GetProperty', $null, $r, @(2))
    $noi  = [string]$r.GetType().InvokeMember('StringData', 'GetProperty', $null, $r, @(3))

    # Cot Name co dang "TENNGAN|Ten day du" (ten 8.3 va ten dai)
    $tenHien = ($ten -split '\|')[-1]
    if ($tenHien -ne 'GiaoXu') { $dsLoi += "loi tat o $noi ten la '$tenHien', can la 'GiaoXu'" }

    # Neu Target bat dau bang '[' thi no la mot THU MUC chu khong phai file chuong trinh
    if ($dich -match '^\[') { $dsLoi += "loi tat '$tenHien' tro vao thu muc $dich chu khong phai GiaoXu.exe" }
}
$v.GetType().InvokeMember('Close', 'InvokeMethod', $null, $v, $null) | Out-Null

if ($soLoiTat -lt 2) { $dsLoi += "chi tim thay $soLoiTat loi tat, can co 2 (Desktop va Start Menu)" }

if ($dsLoi.Count -gt 0) {
    Write-Host 'LOI: loi tat trong bo cai khong dung:'
    foreach ($d in $dsLoi) { Write-Host "  - $d" }
    exit 1
}
Write-Host "KIEM_CHUNG_DAT ($soLoiTat loi tat ten GiaoXu, tro dung vao GiaoXu.exe)"
