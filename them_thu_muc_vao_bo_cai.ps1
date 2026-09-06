<#
====================================================================================
 Thêm các thư mục con của BIN\ vào file .vdproj của bộ cài
====================================================================================

 VẤN ĐỀ:
 Bộ cài MSI chỉ chứa những file nằm trực tiếp trong BIN\, thiếu hẳn ba thư mục con:
   Template\   - biểu mẫu Word/Excel để in giấy chứng nhận, sổ bí tích
   Resources\  - file Excel mẫu để nhập giáo dân, danh sách tên thánh
   help\       - toàn bộ trang hướng dẫn sử dụng (phím F1)
 Máy cài mới sẽ không in được biểu mẫu và không mở được hướng dẫn sử dụng.

 VÌ SAO PHẢI VIẾT SCRIPT:
 Visual Studio Installer Projects bắt thêm từng file một qua giao diện. Ba thư mục
 này có hơn 150 file và còn thay đổi theo thời gian (thêm biểu mẫu cho giáo phận
 mới chẳng hạn). Thêm tay một lần rồi sẽ quên mãi mãi.

 Script đọc thẳng nội dung BIN\ rồi sinh lại các mục tương ứng trong .vdproj, nên
 mỗi lần phát hành bộ cài tự có đúng những gì đang nằm trong BIN\.

 Các mục do script sinh ra đều mang dấu QLGX_TUDONG để lần chạy sau xoá sạch rồi
 sinh lại, không để sót rác của những file đã bị xoá khỏi BIN\.

 Khoá của mỗi mục được tính từ đường dẫn (băm MD5) chứ không sinh ngẫu nhiên, để
 giữa các lần phát hành cùng một file luôn có cùng một khoá. Nếu sinh ngẫu nhiên
 thì Windows Installer sẽ tưởng là file khác và cách nâng cấp sẽ hỏng.

 CÁCH DÙNG:
     .\them_thu_muc_vao_bo_cai.ps1
     .\them_thu_muc_vao_bo_cai.ps1 -ChiKiemChung
====================================================================================
#>

[CmdletBinding()]
param(
    [string]$Vdproj   = '',
    [string]$Bin      = '',
    [string[]]$ThuMuc = @('Template', 'Resources', 'help'),
    [switch]$ChiKiemChung
)

$ErrorActionPreference = 'Stop'

# $PSScriptRoot khong dung duoc trong khoi param() nen gan o day
$goc = Split-Path -Parent $MyInvocation.MyCommand.Path
if (-not $Vdproj) { $Vdproj = Join-Path $goc 'Source\GXInstaller\GXInstaller.vdproj' }
if (-not $Bin)    { $Bin    = Join-Path $goc 'BIN' }

# Ma loai muc trong file .vdproj, do Visual Studio quy dinh
$LOAI_FILE      = '{1FB2D0AE-D3B9-43D4-B9DD-F88EC61E35DE}'
$LOAI_THUMUC    = '{9EF0B969-E518-4E46-987F-47570745A589}'
$KHOA_TARGETDIR = '_EFB30720B033456796CD0F01FE7A6B1F'
$DAU = 'QLGX_TUDONG'

function KhoaTu([string]$chuoi) {
    $md5 = [Security.Cryptography.MD5]::Create()
    $b = $md5.ComputeHash([Text.Encoding]::UTF8.GetBytes($chuoi))
    return '_' + (([BitConverter]::ToString($b) -replace '-', '').ToUpper())
}

if (-not (Test-Path $Vdproj)) { Write-Host "LOI: khong tim thay $Vdproj"; exit 1 }
if (-not (Test-Path $Bin))    { Write-Host "LOI: khong tim thay $Bin"; exit 1 }
$Bin = (Resolve-Path $Bin).Path

# =================================================================== Thu thap
# Cay thu muc: moi nut giu ten, khoa, danh sach file va danh sach thu muc con.
function TaoNut($thuMuc) {
    $tuongDoi = $thuMuc.FullName.Substring($Bin.Length).TrimStart('\')
    return [PSCustomObject]@{
        Ten      = $thuMuc.Name
        TuongDoi = $tuongDoi
        Khoa     = KhoaTu "DIR|$tuongDoi"
        KhoaProp = KhoaTu "PROP|$tuongDoi"
        Con      = @(Get-ChildItem $thuMuc.FullName -Directory | ForEach-Object { TaoNut $_ })
        Files    = @(Get-ChildItem $thuMuc.FullName -File | ForEach-Object {
                        $td = $_.FullName.Substring($Bin.Length).TrimStart('\')
                        [PSCustomObject]@{
                            Ten        = $_.Name
                            Khoa       = KhoaTu "FILE|$td"
                            Nguon      = '..\..\BIN\' + $td
                            KhoaThuMuc = KhoaTu "DIR|$tuongDoi"
                        }
                    })
    }
}

$cay = @()
foreach ($goc in $ThuMuc) {
    $duongDan = Join-Path $Bin $goc
    if (-not (Test-Path $duongDan)) { Write-Host "LOI: khong tim thay $duongDan"; exit 1 }
    $cay += TaoNut (Get-Item $duongDan)
}

# Trai phang cay ra de dem va de sinh phan File / Hierarchy
function TraiPhang($nut) {
    $ds = @($nut)
    foreach ($c in $nut.Con) { $ds += TraiPhang $c }
    return $ds
}
$moiThuMuc = @(); foreach ($n in $cay) { $moiThuMuc += TraiPhang $n }
$moiFile = @(); foreach ($d in $moiThuMuc) { $moiFile += $d.Files }

$txt = [IO.File]::ReadAllText($Vdproj, (New-Object Text.UTF8Encoding $true))

# Chuan hoa ve xuong dong kieu Windows. File .vdproj do Visual Studio ghi ra luon
# dung CRLF, nhung qua tay cac cong cu khac (git, sed, chinh VS khi luu lai) no co
# the thanh LF. Neu khong chuan hoa thi cau lenh tach dong ben duoi tra ve DUNG MOT
# dong khong lo va moi phep tim kiem deu truot.
$txt = [regex]::Replace($txt, "?
", "`r`n")

# =================================================================== Kiem chung
if ($ChiKiemChung) {
    $thieu = @()
    foreach ($f in $moiFile)    { if (-not $txt.Contains($f.Khoa)) { $thieu += "file $($f.Ten)" } }
    foreach ($d in $moiThuMuc)  { if (-not $txt.Contains($d.Khoa)) { $thieu += "thu muc $($d.TuongDoi)" } }
    if ($thieu.Count -gt 0) {
        Write-Host "LOI: thieu $($thieu.Count) muc trong vdproj, vi du:"
        $thieu | Select-Object -First 5 | ForEach-Object { Write-Host "  - $_" }
        exit 1
    }
    Write-Host "KIEM_CHUNG_DAT ($($moiThuMuc.Count) thu muc, $($moiFile.Count) file)"
    exit 0
}

# ================================================== Xoa cac muc lan truoc da sinh
# Quet theo dong va dem ngoac de cat dung tron mot khoi. Dung regex cho viec nay
# rat de sai vi cac khoi long nhau nhieu tang.
$dong = $txt -split "`r`n"
$giu = New-Object Collections.Generic.List[string]
$soXoa = 0
$i = 0
while ($i -lt $dong.Count) {
    $batDauKhoi = ($dong[$i] -match '^\s*"(\{[0-9A-F-]+\}:_[0-9A-F]{32}|Entry)"\s*$') -and
                  ($i + 1 -lt $dong.Count) -and ($dong[$i + 1] -match '^\s*\{\s*$')
    if (-not $batDauKhoi) { $giu.Add($dong[$i]); $i++; continue }

    # Doc tron khoi
    $j = $i + 1
    $sau = 0
    do {
        if ($dong[$j] -match '\{\s*$') { $sau++ }
        elseif ($dong[$j] -match '^\s*\}\s*$') { $sau-- }
        $j++
    } while ($sau -gt 0 -and $j -lt $dong.Count)

    $khoi = $dong[$i..($j - 1)]

    # Chi tinh la muc CUA SCRIPT khi dau QLGX_TUDONG nam ngay o cap cua chinh khoi
    # nay. Neu bat ca dau nam sau trong cac khoi con thi se xoa nham ca thu muc
    # Application Folder - vi cac thu muc script sinh ra deu nam long ben trong no.
    $lot = ([regex]::Match($dong[$i + 1], '^\s*')).Value
    $laCuaScript = $false
    foreach ($d in $khoi) {
        if ($d -eq ($lot + '"Tag" = "8:' + $DAU + '"') -or
            $d -eq ($lot + '"Condition" = "8:' + $DAU + '"')) { $laCuaScript = $true; break }
        if ($d -match ('^' + [regex]::Escape($lot) + '"MsmKey" = "8:(_[0-9A-F]{32})"$') -and
            ($moiFile.Khoa + $moiThuMuc.Khoa) -contains $Matches[1]) { $laCuaScript = $true; break }
    }
    if ($laCuaScript) { $soXoa++ } else { $khoi | ForEach-Object { $giu.Add($_) } }
    $i = $j
}
$txt = ($giu -join "`r`n")

# Cac thu muc do script sinh ra nam LONG BEN TRONG khoi Application Folder, nen vong
# quet o tren khong nhin thay chung (no nuot ca khoi cha lam mot). Vi vay don rieng:
# xoa sach noi dung khoi "Folders" cua Application Folder. Khoi nay von rong, chi
# script nay moi ghi vao do.
$dong = $txt -split "`r`n"
$viTri = -1
for ($k = 0; $k -lt $dong.Count - 2; $k++) {
    if ($dong[$k] -match '"Property" = "8:TARGETDIR"' -and $dong[$k + 1] -match '"Folders"') { $viTri = $k + 2; break }
}
if ($viTri -lt 0) { Write-Host 'LOI: khong tim thay khoi Folders cua Application Folder'; exit 1 }
$sau = 0; $m = $viTri
do {
    if ($dong[$m] -match '\{\s*$') { $sau++ } elseif ($dong[$m] -match '^\s*\}\s*$') { $sau-- }
    $m++
} while ($sau -gt 0 -and $m -lt $dong.Count)
$soThuMucCu = ($dong[$viTri..($m - 1)] | Where-Object { $_ -match '"Condition" = "8:' + $DAU + '"' }).Count
$soXoa += $soThuMucCu
$txt = (@($dong[0..$viTri]) + @($dong[($m - 1)..($dong.Count - 1)])) -join "`r`n"

# =================================================================== Sinh noi dung
$nl = "`r`n"

function SinhThuMuc($nut, [int]$muc) {
    $lot = ' ' * $muc
    $s = ''
    $s += $lot + '"' + $LOAI_THUMUC + ':' + $nut.Khoa + '"' + $nl
    $s += $lot + '{' + $nl
    $s += $lot + '"Name" = "8:' + $nut.Ten + '"' + $nl
    $s += $lot + '"AlwaysCreate" = "11:FALSE"' + $nl
    $s += $lot + '"Condition" = "8:' + $DAU + '"' + $nl
    $s += $lot + '"Transitive" = "11:FALSE"' + $nl
    $s += $lot + '"Property" = "8:' + $nut.KhoaProp + '"' + $nl
    $s += $lot + '    "Folders"' + $nl
    $s += $lot + '    {' + $nl
    foreach ($c in $nut.Con) { $s += SinhThuMuc $c ($muc + 8) }
    $s += $lot + '    }' + $nl
    $s += $lot + '}' + $nl
    return $s
}

$khoiFolder = ''
foreach ($n in $cay) { $khoiFolder += SinhThuMuc $n 20 }

$khoiFile = ''
foreach ($f in $moiFile) {
    $khoiFile += '            "' + $LOAI_FILE + ':' + $f.Khoa + '"' + $nl
    $khoiFile += '            {' + $nl
    $khoiFile += '            "SourcePath" = "8:' + ($f.Nguon -replace '\\', '\\') + '"' + $nl
    $khoiFile += '            "TargetName" = "8:' + $f.Ten + '"' + $nl
    $khoiFile += '            "Tag" = "8:' + $DAU + '"' + $nl
    $khoiFile += '            "Folder" = "8:' + $f.KhoaThuMuc + '"' + $nl
    $khoiFile += '            "Condition" = "8:"' + $nl
    $khoiFile += '            "Transitive" = "11:FALSE"' + $nl
    $khoiFile += '            "Vital" = "11:TRUE"' + $nl
    $khoiFile += '            "ReadOnly" = "11:FALSE"' + $nl
    $khoiFile += '            "Hidden" = "11:FALSE"' + $nl
    $khoiFile += '            "System" = "11:FALSE"' + $nl
    $khoiFile += '            "Permanent" = "11:FALSE"' + $nl
    $khoiFile += '            "SharedLegacy" = "11:FALSE"' + $nl
    $khoiFile += '            "PackageAs" = "3:1"' + $nl
    $khoiFile += '            "Register" = "3:1"' + $nl
    $khoiFile += '            "Exclude" = "11:FALSE"' + $nl
    $khoiFile += '            "IsDependency" = "11:FALSE"' + $nl
    $khoiFile += '            "IsolateTo" = "8:"' + $nl
    $khoiFile += '            }' + $nl
}

$khoiHier = ''
foreach ($f in $moiFile) {
    $khoiHier += '        "Entry"' + $nl
    $khoiHier += '        {' + $nl
    $khoiHier += '        "MsmKey" = "8:' + $f.Khoa + '"' + $nl
    $khoiHier += '        "OwnerKey" = "8:_UNDEFINED"' + $nl
    $khoiHier += '        "MsmSig" = "8:_UNDEFINED"' + $nl
    $khoiHier += '        }' + $nl
}

# =================================================================== Chen vao
function ChenSau([string]$moc, [string]$noiDung, [string]$nhan) {
    $c = ([regex]::Matches($script:txt, [regex]::Escape($moc))).Count
    if ($c -ne 1) { Write-Host "LOI [$nhan]: tim thay moc $c lan (can dung 1)"; exit 1 }
    $script:txt = $script:txt.Replace($moc, $moc + $noiDung)
}

ChenSau ('    "Hierarchy"' + $nl + '    {' + $nl) $khoiHier 'Hierarchy'
ChenSau ('        "File"' + $nl + '        {' + $nl) $khoiFile 'File'
ChenSau ('"Property" = "8:TARGETDIR"' + $nl + '                "Folders"' + $nl + '                {' + $nl) $khoiFolder 'Folders cua TARGETDIR'

[IO.File]::WriteAllText($Vdproj, $txt, (New-Object Text.UTF8Encoding $true))
Write-Host "Da xoa $soXoa muc cu; them $($moiThuMuc.Count) thu muc va $($moiFile.Count) file vao vdproj"
