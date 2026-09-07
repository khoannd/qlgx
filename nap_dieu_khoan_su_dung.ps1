<#
====================================================================================
 Nạp bản điều khoản sử dụng vào màn hình cài đặt
====================================================================================

 MỤC ĐÍCH:
 Bộ cài Inno Setup cũ có màn hình bắt người dùng đọc và chấp nhận điều khoản
 trước khi cài. Bộ cài mới cũng có màn hình đó (hộp thoại EulaForm, thêm vào từ
 file .vdproj), nhưng phần nội dung để trống - Visual Studio chỉ đặt sẵn chữ giữ
 chỗ "MsiLabel". Script này ghi nội dung thật vào.

 CHỈ GHI PHẦN NỘI DUNG ĐIỀU KHOẢN:
 Toàn bộ chữ còn lại của bộ cài (nút Next/Back/Cancel, tiêu đề các bước...) giữ
 nguyên tiếng Anh như Visual Studio sinh ra.

 VÌ SAO KHÔNG DỊCH CẢ GIAO DIỆN SANG TIẾNG VIỆT:
 Đã thử ở bản 4.0.1 và hỏng: toàn bộ màn hình cài đặt hiện ra trống trơn. Nguyên
 nhân KHÔNG phải bảng mã mà là một lỗi cú pháp PowerShell - trong mảng viết dạng
 @('a', 'b', $PHONG + 'chữ') thì PowerShell tách $PHONG và 'chữ' thành HAI phần
 tử riêng, nên bảng dịch chỉ còn lại thẻ phông rỗng và bộ cài ghi chuỗi trống vào
 mọi nút. Muốn nối chuỗi trong mảng thì phải bọc ngoặc đơn: ($PHONG + 'chữ').

 VÌ SAO NỘI DUNG ĐIỀU KHOẢN KHÔNG BỊ LỖI BẢNG MÃ:
 Ô hiển thị điều khoản là loại ScrollableText, nội dung phải ở dạng RTF. Trong
 RTF, chữ có dấu được mã hoá thành \uNNNN? - toàn ký tự ASCII. Nên chuỗi ghi vào
 file MSI không có ký tự nào ngoài ASCII và không phụ thuộc bảng mã của gói cài.

 CÁCH DÙNG:
     .\nap_dieu_khoan_su_dung.ps1 -Msi "...\qlgx_4_0_2.msi"
     .\nap_dieu_khoan_su_dung.ps1 -Msi "...\qlgx_4_0_2.msi" -ChiKiemChung
====================================================================================
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][string]$Msi,
    [switch]$ChiKiemChung
)

$ErrorActionPreference = 'Stop'
$MSITRANSACT = 1

# Lay nguyen van tu file license.txt cua bo cai Inno Setup cu.
$DIEU_KHOAN = @(
    'ĐIỀU KHOẢN SỬ DỤNG CHƯƠNG TRÌNH QUẢN LÝ GIÁO XỨ'
    ''
    '- Đây là chương trình miễn phí được phân phối từ website http://quanlygiaoxu.net'
    ''
    '- Bản quyền thuộc Nguyễn Đức Khoan (hotro@quanlygiaoxu.net).'
    ''
    '- Quý vị có toàn quyền sử dụng và chia sẻ nhưng không được phép mua bán chương trình này dưới bất kỳ hình thức nào.'
    ''
    '- Dữ liệu chương trình của quý vị do quý vị tự chịu trách nhiệm về tính đúng đắn và vấn đề pháp lý.'
    ''
    '- Mọi thắc mắc và góp ý, xin vui lòng gởi về địa chỉ email của tác giả (hotro@quanlygiaoxu.net).'
    ''
    '- Mong quý vị vui lòng thường xuyên ghé thăm website chương trình để cập nhật các thông tin mới nhất về chương trình.'
    ''
    '- Xin chân thành cảm ơn quý vị đã sử dụng chương trình này!'
)

# Chuyen van ban thuong sang RTF, ma hoa chu co dau thanh \uNNNN? de khong phu
# thuoc bang ma. Dung phong Tahoma vi phong mac dinh MS Sans Serif khong co dau
# tieng Viet.
function SangRtf([string[]]$cacDong) {
    $sb = New-Object Text.StringBuilder
    [void]$sb.Append('{\rtf1\ansi\deff0{\fonttbl{\f0\fswiss Tahoma;}}\fs18 ')
    foreach ($dong in $cacDong) {
        foreach ($ch in $dong.ToCharArray()) {
            $ma = [int]$ch
            if ($ch -eq '\' -or $ch -eq '{' -or $ch -eq '}') { [void]$sb.Append('\' + $ch) }
            elseif ($ma -lt 128) { [void]$sb.Append($ch) }
            else { [void]$sb.Append('\u' + $ma + '?') }
        }
        [void]$sb.Append('\par' + "`r`n")
    }
    [void]$sb.Append('}')
    return $sb.ToString()
}

function MoDb($duongDan, $cheDo) {
    $script:installer = New-Object -ComObject WindowsInstaller.Installer
    return $script:installer.GetType().InvokeMember('OpenDatabase', 'InvokeMethod', $null, $script:installer, @($duongDan, $cheDo))
}
function DongDb($db) {
    [Runtime.InteropServices.Marshal]::ReleaseComObject($db) | Out-Null
    [Runtime.InteropServices.Marshal]::ReleaseComObject($script:installer) | Out-Null
    [GC]::Collect(); [GC]::WaitForPendingFinalizers()
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

$rtf = SangRtf $DIEU_KHOAN
$SQL_DOC = 'SELECT `Text` FROM `Control` WHERE `Dialog_`=''EulaForm'' AND `Control`=''LicenseText'''

# Kiem tra ngay tren chuoi vua sinh ra, TRUOC khi ghi. Buoc nay de bat dung lop
# loi da lam hong ban 4.0.1: luc do script ghi chuoi rong vao MSI roi doc lai cung
# ra chuoi rong, so sanh thay bang nhau nen bao "dat" trong khi that ra da hong.
$loi = @()
if (-not $rtf.StartsWith('{\rtf1'))        { $loi += 'chuoi sinh ra khong phai RTF' }
if ($rtf.Length -lt 1000)                  { $loi += "chuoi sinh ra qua ngan ($($rtf.Length) ky tu)" }
if ($rtf -notmatch '\\u272\?')             { $loi += 'thieu chu Đ da ma hoa - dau tieng Viet bi mat' }
if (@($rtf.ToCharArray() | Where-Object { [int]$_ -gt 127 }).Count -gt 0) { $loi += 'con ky tu ngoai ASCII, se bi bang ma lam hong' }
if ($loi.Count -gt 0) {
    Write-Host 'LOI: noi dung dieu khoan sinh ra khong dung:'
    foreach ($l in $loi) { Write-Host "  - $l" }
    exit 1
}

# ------------------------------------------------------------------ Kiem chung
if ($ChiKiemChung) {
    $db = MoDb $Msi 0
    $doc = DocMotO $db $SQL_DOC
    DongDb $db
    if ($doc -ne $rtf) {
        Write-Host "LOI: noi dung trong MSI khong khop (doc duoc $(([string]$doc).Length) ky tu, can $($rtf.Length))"
        exit 1
    }
    Write-Host "KIEM_CHUNG_DAT (ban dieu khoan $($rtf.Length) ky tu)"
    exit 0
}

# ------------------------------------------------------------------------- Ghi
$db = MoDb $Msi $MSITRANSACT
$v = $db.GetType().InvokeMember('OpenView', 'InvokeMethod', $null, $db,
        @('UPDATE `Control` SET `Text` = ? WHERE `Dialog_` = ''EulaForm'' AND `Control` = ''LicenseText'''))
$r = $script:installer.GetType().InvokeMember('CreateRecord', 'InvokeMethod', $null, $script:installer, @(1))
$r.GetType().InvokeMember('StringData', 'SetProperty', $null, $r, @(1, $rtf)) | Out-Null
$v.GetType().InvokeMember('Execute', 'InvokeMethod', $null, $v, @($r)) | Out-Null
$v.GetType().InvokeMember('Close', 'InvokeMethod', $null, $v, $null) | Out-Null
$db.GetType().InvokeMember('Commit', 'InvokeMethod', $null, $db, $null) | Out-Null
DongDb $db

Write-Host "Da nap ban dieu khoan su dung ($($rtf.Length) ky tu RTF); phan chu con lai giu nguyen tieng Anh"
