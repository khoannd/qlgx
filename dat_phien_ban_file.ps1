<#
====================================================================================
 Đặt số phiên bản cho từng file chương trình
====================================================================================

 VẤN ĐỀ NGHIÊM TRỌNG ĐÃ GẶP (bản 4.0.0 và 4.0.1):
 Cài bộ cài mới đè lên máy đang có QLGX thì chương trình VẪN LÀ BẢN CŨ. Không có
 thông báo lỗi nào, bộ cài chạy xong bình thường, nhưng mở lên không thấy chức
 năng mới.

 NGUYÊN NHÂN:
 Số phiên bản của file GiaoXu.exe bị để nguyên "1.0.0.3" suốt nhiều năm, trong
 khi số phiên bản sản phẩm vẫn tăng đều (3.3.7 -> 4.0.0 -> 4.0.1). Windows
 Installer quyết định có chép đè một file hay không bằng cách SO SÁNH SỐ PHIÊN BẢN
 CỦA FILE:
     file trong bộ cài MỚI HƠN  -> chép đè
     bằng nhau hoặc cũ hơn      -> GIỮ NGUYÊN file đang có
 Vì cả hai bên đều là 1.0.0.3 nên Windows giữ nguyên chương trình cũ.

 Bộ cài Inno Setup trước đây không có vấn đề này vì nó dùng cờ "ignoreversion",
 tức là chép đè bất kể phiên bản. Chuyển sang Windows Installer thì mất đặc tính
 đó, và lỗi nằm im suốt hai lần phát hành.

 CÁCH SỬA:
 Mỗi lần phát hành, ghi số phiên bản của bản phát hành vào AssemblyFileVersion của
 tất cả các dự án. Nhờ vậy file luôn mới hơn file đang có trên máy người dùng.

 Chỉ đổi AssemblyFileVersion, KHÔNG đụng tới AssemblyVersion, vì AssemblyVersion
 là danh tính mà .NET dùng để nạp thư viện; đổi nó có thể làm hỏng tham chiếu giữa
 các dll. Windows Installer chỉ đọc AssemblyFileVersion.

 CÁCH DÙNG:
     .\dat_phien_ban_file.ps1 -PhienBan 4.0.2.0                 (chạy TRƯỚC khi build)
     .\dat_phien_ban_file.ps1 -PhienBan 4.0.2.0 -ChiKiemChung   (chạy SAU khi build)

 Chế độ kiểm chứng đọc số phiên bản THẬT của các file đã build trong BIN\, chứ
 không đọc lại mã nguồn - để chắc chắn trình biên dịch đã nhận số mới.
====================================================================================
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][string]$PhienBan,
    [switch]$ChiKiemChung
)

$ErrorActionPreference = 'Stop'
$goc = Split-Path -Parent $MyInvocation.MyCommand.Path

if ($PhienBan -notmatch '^\d+\.\d+\.\d+\.\d+$') {
    Write-Host "LOI: so phien ban phai co dang x.y.z.w, nhan duoc '$PhienBan'"
    exit 1
}

# Tung du an va file chuong trinh no sinh ra
$duAn = @(
    ,@('Source\ChuongTrinh\Properties\AssemblyInfo.cs',            'GiaoXu.exe')
    ,@('Source\GXControl\Properties\AssemblyInfo.cs',              'GXControl.dll')
    ,@('Source\DBAccess\Properties\AssemblyInfo.cs',               'GXGlobal.dll')
    ,@('Source\ExcelReport\Properties\AssemblyInfo.cs',            'ExcelReport.dll')
    ,@('Source\Giaoly\Properties\AssemblyInfo.cs',                 'Giaoly.dll')
    ,@('Source\AutoUpdate\Properties\AssemblyInfo.cs',             'AutoUpdate.exe')
    ,@('Source\ConvertFont\AssemblyInfo.cs',                       'vnConvert.dll')
)

# KHONG co trong danh sach tren: AutoComplete.dll va TabStrip.dll. Hai file nay
# khong do solution nay build ra - chung la thu vien dung san nam san trong BIN\.
# Chung khong bao gio thay doi giua cac ban phat hanh nen viec Windows Installer
# giu nguyen file cu tren may la vo hai. Neu sau nay co thay chung, phai build lai
# chung voi so phien ban cao hon, neu khong may nguoi dung se giu ban cu.

# ------------------------------------------------------------------ Kiem chung
if ($ChiKiemChung) {
    $bin = Join-Path $goc 'BIN'
    $sai = @()
    foreach ($d in $duAn) {
        $f = Join-Path $bin $d[1]
        if (-not (Test-Path $f)) { $sai += "$($d[1]): khong tim thay trong BIN\"; continue }
        $pb = (Get-Item $f).VersionInfo.FileVersion
        # FileVersion doc ra co the co dang "4, 0, 2, 0"
        $pbChuan = ($pb -replace '[^\d.]', '')
        if ($pbChuan -ne $PhienBan) { $sai += "$($d[1]): dang la '$pb', can '$PhienBan'" }
    }
    if ($sai.Count -gt 0) {
        Write-Host 'LOI: cac file sau chua mang dung so phien ban:'
        foreach ($s in $sai) { Write-Host "  - $s" }
        exit 1
    }
    Write-Host "KIEM_CHUNG_DAT ($($duAn.Count) file deu mang phien ban $PhienBan)"
    exit 0
}

# ------------------------------------------------------------------------- Ghi
function DocGiuBangMa([string]$duongDan) {
    $b = [IO.File]::ReadAllBytes($duongDan)
    if ($b.Length -ge 2 -and $b[0] -eq 0xFF -and $b[1] -eq 0xFE) {
        return @{ Chu = [Text.Encoding]::Unicode.GetString($b, 2, $b.Length - 2); Ma = 'utf16' }
    }
    if ($b.Length -ge 3 -and $b[0] -eq 0xEF -and $b[1] -eq 0xBB -and $b[2] -eq 0xBF) {
        return @{ Chu = [Text.Encoding]::UTF8.GetString($b, 3, $b.Length - 3); Ma = 'utf8bom' }
    }
    return @{ Chu = [Text.Encoding]::UTF8.GetString($b); Ma = 'utf8' }
}
function GhiGiuBangMa([string]$duongDan, [string]$chu, [string]$ma) {
    switch ($ma) {
        'utf16'   { [IO.File]::WriteAllText($duongDan, $chu, (New-Object Text.UnicodeEncoding($false, $true))) }
        'utf8bom' { [IO.File]::WriteAllText($duongDan, $chu, (New-Object Text.UTF8Encoding $true)) }
        default   { [IO.File]::WriteAllText($duongDan, $chu, (New-Object Text.UTF8Encoding $false)) }
    }
}

$daSua = 0
$daThem = 0
foreach ($d in $duAn) {
    $f = Join-Path $goc $d[0]
    if (-not (Test-Path $f)) { Write-Host "LOI: khong tim thay $f"; exit 1 }

    $doc = DocGiuBangMa $f
    $chu = $doc.Chu

    # Chap nhan ca hai cach viet: AssemblyFileVersion va AssemblyFileVersionAttribute
    $mau = '(\[assembly:\s*AssemblyFileVersion(?:Attribute)?\s*\(\s*")[^"]*("\s*\)\s*\])'
    if ($chu -match $mau) {
        $chu = [regex]::Replace($chu, $mau, "`${1}$PhienBan`${2}")
        $daSua++
    } else {
        # Du an nao chua khai bao thi them vao cuoi file
        $xuong = if ($chu.Contains("`r`n")) { "`r`n" } else { "`n" }
        $chu = $chu.TrimEnd() + $xuong + "[assembly: AssemblyFileVersion(`"$PhienBan`")]" + $xuong
        $daThem++
    }
    GhiGiuBangMa $f $chu $doc.Ma

    # Doc lai de chac chan da ghi dung
    $ktra = (DocGiuBangMa $f).Chu
    if ($ktra -notmatch [regex]::Escape("AssemblyFileVersion") + '(?:Attribute)?\s*\(\s*"' + [regex]::Escape($PhienBan) + '"') {
        Write-Host "LOI: ghi phien ban vao $($d[0]) that bai"
        exit 1
    }
}

Write-Host "Da dat phien ban $PhienBan cho $($duAn.Count) du an (sua $daSua, them moi $daThem)"
