<#
====================================================================================
 Ghi tên sản phẩm và nhà sản xuất bằng tiếng Việt có dấu vào file MSI
====================================================================================

 VÌ SAO CẦN FILE NÀY:
 Visual Studio Installer Projects không ghi được tiếng Việt có dấu vào .vdproj.
 Đã thử cả ba bảng mã và đều hỏng:
     CodePage 1252  (Tây Âu)   -> nuốt mất chữ "ả", "ứ"  => "QLGX - Qun Ly Giao X"
     CodePage 1258  (Việt Nam) -> biến thành ký tự rác U+007F
     CodePage 65001 (UTF-8)    -> build thất bại hoàn toàn

 Nên cách làm là: build bộ cài bằng tên KHÔNG DẤU cho chắc chắn, rồi dùng script
 này ép bảng mã của file MSI về UTF-8 và ghi đè tên tiếng Việt có dấu vào.

 PHẢI CHẠY BẰNG TIẾN TRÌNH RIÊNG, vì hai lý do:
   1. Nếu gọi trong tiến trình vừa dùng COM của Visual Studio để build thì
      Windows Installer báo lỗi "Type mismatch".
   2. Sau khi ghi xong (Commit), KHÔNG mở lại được file MSI trong cùng tiến
      trình đó nữa - đã thử chờ 10 giây vẫn không được. Nên bước kiểm chứng
      phải chạy bằng một tiến trình khác, qua tham số -ChiKiemChung.

 CÁCH DÙNG:
     # Bước 1 - ghi
     .\ghi_ten_tieng_viet_vao_msi.ps1 -Msi "...\qlgx_4_0_0.msi" `
         -TenSanPham "QLGX - Quản Lý Giáo Xứ" -NhaSanXuat "Nguyễn Đức Khoan"

     # Bước 2 - kiểm chứng (tiến trình khác)
     .\ghi_ten_tieng_viet_vao_msi.ps1 -Msi "...\qlgx_4_0_0.msi" `
         -TenSanPham "QLGX - Quản Lý Giáo Xứ" -NhaSanXuat "Nguyễn Đức Khoan" `
         -ChiKiemChung

 Thoát với mã 0 nếu thành công, mã 1 nếu thất bại.
====================================================================================
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][string]$Msi,
    [Parameter(Mandatory = $true)][string]$TenSanPham,
    [Parameter(Mandatory = $true)][string]$NhaSanXuat,
    [switch]$ChiKiemChung
)

$ErrorActionPreference = 'Stop'

function MaKyTu ($s) {
    if ($null -eq $s) { return '(rong)' }
    return (($s.ToCharArray() | ForEach-Object { 'U+{0:X4}' -f [int]$_ }) -join ' ')
}

if (-not (Test-Path $Msi)) { Write-Host "LOI: khong tim thay $Msi"; exit 1 }

$MSIMODIFY_UPDATE = 2
$MSITRANSACT      = 1

try {
    if ($ChiKiemChung) {
        # ------------------------------------------------------ Chế độ kiểm chứng
        $i = New-Object -ComObject WindowsInstaller.Installer
        $db = $i.GetType().InvokeMember('OpenDatabase', 'InvokeMethod', $null, $i, @($Msi, 0))
        $doc = @{}
        $v = $db.GetType().InvokeMember('OpenView', 'InvokeMethod', $null, $db,
             @("SELECT Property, Value FROM Property WHERE Property='ProductName' OR Property='Manufacturer'"))
        $v.GetType().InvokeMember('Execute', 'InvokeMethod', $null, $v, $null) | Out-Null
        while ($true) {
            $r = $v.GetType().InvokeMember('Fetch', 'InvokeMethod', $null, $v, $null)
            if ($null -eq $r) { break }
            $k = [string]$r.GetType().InvokeMember('StringData', 'GetProperty', $null, $r, @(1))
            $g = [string]$r.GetType().InvokeMember('StringData', 'GetProperty', $null, $r, @(2))
            $doc[$k] = $g
        }
        $v.GetType().InvokeMember('Close', 'InvokeMethod', $null, $v, $null) | Out-Null

        if ($doc['ProductName'] -cne $TenSanPham) {
            Write-Host 'LOI: ProductName trong MSI khong dung'
            Write-Host ('  mong doi: ' + (MaKyTu $TenSanPham))
            Write-Host ('  thuc te : ' + (MaKyTu $doc['ProductName']))
            exit 1
        }
        if ($doc['Manufacturer'] -cne $NhaSanXuat) {
            Write-Host 'LOI: Manufacturer trong MSI khong dung'
            Write-Host ('  mong doi: ' + (MaKyTu $NhaSanXuat))
            Write-Host ('  thuc te : ' + (MaKyTu $doc['Manufacturer']))
            exit 1
        }
        Write-Host 'KIEM_CHUNG_DAT'
        exit 0
    }

    # ---------------------------------------------------------------- Chế độ ghi
    $installer = New-Object -ComObject WindowsInstaller.Installer
    $db = $installer.GetType().InvokeMember('OpenDatabase', 'InvokeMethod', $null, $installer, @($Msi, $MSITRANSACT))

    # Ép bảng mã của MSI sang UTF-8 bằng file .idt có chỉ thị _ForceCodepage.
    # Định dạng .idt: hai dòng đầu để trống, dòng thứ ba là chỉ thị.
    $idtDir  = [IO.Path]::GetTempPath()
    $idtName = 'qlgx_forcecp.idt'
    $noiDung = "`r`n`r`n65001`t_ForceCodepage`r`n"
    [IO.File]::WriteAllText((Join-Path $idtDir $idtName), $noiDung, (New-Object Text.ASCIIEncoding))
    $db.GetType().InvokeMember('Import', 'InvokeMethod', $null, $db, @($idtDir, $idtName)) | Out-Null

    foreach ($cap in @(@('ProductName', $TenSanPham), @('Manufacturer', $NhaSanXuat))) {
        $view = $db.GetType().InvokeMember('OpenView', 'InvokeMethod', $null, $db,
                @("SELECT Property, Value FROM Property WHERE Property='$($cap[0])'"))
        $view.GetType().InvokeMember('Execute', 'InvokeMethod', $null, $view, $null) | Out-Null
        $rec = $view.GetType().InvokeMember('Fetch', 'InvokeMethod', $null, $view, $null)
        if ($null -ne $rec) {
            $rec.GetType().InvokeMember('StringData', 'SetProperty', $null, $rec, @(2, $cap[1])) | Out-Null
            $view.GetType().InvokeMember('Modify', 'InvokeMethod', $null, $view, @($MSIMODIFY_UPDATE, $rec)) | Out-Null
        }
        $view.GetType().InvokeMember('Close', 'InvokeMethod', $null, $view, $null) | Out-Null
    }

    $db.GetType().InvokeMember('Commit', 'InvokeMethod', $null, $db, $null) | Out-Null
    [Runtime.InteropServices.Marshal]::ReleaseComObject($db) | Out-Null
    [Runtime.InteropServices.Marshal]::ReleaseComObject($installer) | Out-Null
    [GC]::Collect(); [GC]::WaitForPendingFinalizers()

    Write-Host 'DA_GHI_XONG'
    exit 0
}
catch {
    Write-Host "LOI: $($_.Exception.Message)"
    Write-Host "  tai dong $($_.InvocationInfo.ScriptLineNumber): $($_.InvocationInfo.Line.Trim())"
    exit 1
}
