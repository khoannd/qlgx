<#
====================================================================================
 Thêm khả năng tự dò thư mục đã cài trước đó vào file MSI
====================================================================================

 VẤN ĐỀ:
 Từ bản 4.0.0 thư mục mặc định đổi từ D:\QuanLyGiaoXu sang C:\QuanLyGiaoXu, vì
 rất nhiều máy (nhất là laptop) không có ổ D:, và bộ cài báo lỗi
 "'QuanLyGiaoXu' is not a valid short file name".

 Nhưng nếu cứ thế cài sang C: thì những máy ĐANG DÙNG sẽ gặp cảnh: chương trình
 mới nằm ở C:, còn dữ liệu cũ (giaoxu.mdb) vẫn nằm ở D: - người dùng mở lên thấy
 trống trơn và tưởng mất sạch dữ liệu giáo xứ.

 CÁCH GIẢI QUYẾT:
 Cho bộ cài tự dò xem máy đã cài ở đâu rồi cài đè lên đúng chỗ đó. Chỉ dùng
 C:\QuanLyGiaoXu khi là máy hoàn toàn mới.

 Dò theo ba nguồn, ưu tiên từ trên xuống:
   1. Khoá registry do bản Inno Setup cũ để lại (bao được cả khi người dùng tự
      chọn thư mục khác lúc cài):
          HKLM\SOFTWARE\...\Uninstall\{695008D3-...}_is1  ->  InstallLocation
   2. Cùng khoá đó nhưng đọc ở nhánh registry 64-bit.
   3. Thư mục D:\QuanLyGiaoXu nếu còn tồn tại (bao các máy đã cài bản MSI 4.0.0
      đầu tiên, vì bản đó ghi InstallLocation rỗng nên không tự nhận ra được).

 Ngoài ra ghi luôn ARPINSTALLLOCATION để các bản sau này tự dò được chính mình.

 Visual Studio Installer Projects không cho cấu hình những thứ này, nên phải can
 thiệp thẳng vào file MSI sau khi build.

 CÁCH DÙNG:
     .\them_do_tim_thu_muc_cu.ps1 -Msi "...\qlgx_4_0_0.msi"
     .\them_do_tim_thu_muc_cu.ps1 -Msi "...\qlgx_4_0_0.msi" -ChiKiemChung
====================================================================================
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][string]$Msi,
    [switch]$ChiKiemChung
)

$ErrorActionPreference = 'Stop'

# AppId cua bo cai Inno Setup cu - lay tu Setup_new.iss
$KHOA_INNO = 'SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{695008D3-7C22-4103-9CA6-107D525C82C5}_is1'
$THUMUC_CU_D = 'D:\QuanLyGiaoXu'

# Ma so cua Windows Installer
$HKLM                   = 2
$LOC_TYPE_DIRECTORY     = 0    # gia tri registry la duong dan thu muc, co kiem tra ton tai
$LOC_TYPE_64BIT         = 16   # doc o nhanh registry 64-bit
$CA_TYPE_SET_PROPERTY   = 51   # custom action gan gia tri cho mot thuoc tinh
$MSITRANSACT            = 1

# Cac dong se them vao. Dat ten bat dau bang QLGX_ de de nhan ra va xoa lai.
$dsRegLocator = @(
    @('QLGX_InnoDir',   $HKLM, $KHOA_INNO, 'InstallLocation', $LOC_TYPE_DIRECTORY),
    @('QLGX_InnoDir64', $HKLM, $KHOA_INNO, 'InstallLocation', ($LOC_TYPE_DIRECTORY + $LOC_TYPE_64BIT))
)
$dsAppSearch = @(
    @('QLGXPREVDIR',   'QLGX_InnoDir'),
    @('QLGXPREVDIR64', 'QLGX_InnoDir64'),
    @('QLGXPREVDIRD',  'QLGX_DirD')
)
$dsCustomAction = @(
    # (Ten, Type, Source, Target)
    @('QLGX_UsePrevDir',   $CA_TYPE_SET_PROPERTY, 'TARGETDIR',          '[QLGXPREVDIR]'),
    @('QLGX_UsePrevDir64', $CA_TYPE_SET_PROPERTY, 'TARGETDIR',          '[QLGXPREVDIR64]'),
    @('QLGX_UsePrevDirD',  $CA_TYPE_SET_PROPERTY, 'TARGETDIR',          '[QLGXPREVDIRD]'),
    @('QLGX_SetArpLoc',    $CA_TYPE_SET_PROPERTY, 'ARPINSTALLLOCATION', '[TARGETDIR]')
)
# (Ten, Dieu kien, Thu tu) - phai chay SAU AppSearch (100) va TRUOC DIRCA_TARGETDIR (750)
$dsSequence = @(
    @('QLGX_UsePrevDir',   'QLGXPREVDIR',                                          740),
    @('QLGX_UsePrevDir64', 'QLGXPREVDIR64 AND NOT QLGXPREVDIR',                     741),
    @('QLGX_UsePrevDirD',  'QLGXPREVDIRD AND NOT QLGXPREVDIR AND NOT QLGXPREVDIR64', 742)
)

function MoDb($duongDan, $cheDo) {
    $script:installer = New-Object -ComObject WindowsInstaller.Installer
    return $script:installer.GetType().InvokeMember('OpenDatabase', 'InvokeMethod', $null, $script:installer, @($duongDan, $cheDo))
}
function ChaySql($db, $sql) {
    $v = $db.GetType().InvokeMember('OpenView', 'InvokeMethod', $null, $db, @($sql))
    $v.GetType().InvokeMember('Execute', 'InvokeMethod', $null, $v, $null) | Out-Null
    $v.GetType().InvokeMember('Close', 'InvokeMethod', $null, $v, $null) | Out-Null
}
function DemSql($db, $sql) {
    $v = $db.GetType().InvokeMember('OpenView', 'InvokeMethod', $null, $db, @($sql))
    $v.GetType().InvokeMember('Execute', 'InvokeMethod', $null, $v, $null) | Out-Null
    $n = 0
    while ($true) {
        $r = $v.GetType().InvokeMember('Fetch', 'InvokeMethod', $null, $v, $null)
        if ($null -eq $r) { break }
        $n++
    }
    $v.GetType().InvokeMember('Close', 'InvokeMethod', $null, $v, $null) | Out-Null
    return $n
}
function DongDb($db) {
    [Runtime.InteropServices.Marshal]::ReleaseComObject($db) | Out-Null
    [Runtime.InteropServices.Marshal]::ReleaseComObject($script:installer) | Out-Null
    [GC]::Collect(); [GC]::WaitForPendingFinalizers()
}

if (-not (Test-Path $Msi)) { Write-Host "LOI: khong tim thay $Msi"; exit 1 }

try {
    # ------------------------------------------------------------ Kiểm chứng
    if ($ChiKiemChung) {
        $db = MoDb $Msi 0
        $thieu = @()
        if ((DemSql $db "SELECT Signature_ FROM RegLocator WHERE Signature_='QLGX_InnoDir'") -ne 1)   { $thieu += 'RegLocator QLGX_InnoDir' }
        if ((DemSql $db "SELECT Signature_ FROM RegLocator WHERE Signature_='QLGX_InnoDir64'") -ne 1) { $thieu += 'RegLocator QLGX_InnoDir64' }
        if ((DemSql $db "SELECT Signature_ FROM DrLocator WHERE Signature_='QLGX_DirD'") -ne 1)       { $thieu += 'DrLocator QLGX_DirD' }
        if ((DemSql $db "SELECT Property FROM AppSearch WHERE Property='QLGXPREVDIR'") -ne 1)         { $thieu += 'AppSearch QLGXPREVDIR' }
        if ((DemSql $db "SELECT Action FROM CustomAction WHERE Action='QLGX_UsePrevDir'") -ne 1)      { $thieu += 'CustomAction QLGX_UsePrevDir' }
        if ((DemSql $db "SELECT Action FROM CustomAction WHERE Action='QLGX_SetArpLoc'") -ne 1)       { $thieu += 'CustomAction QLGX_SetArpLoc' }
        foreach ($seq in 'InstallUISequence', 'InstallExecuteSequence') {
            if ((DemSql $db "SELECT Action FROM $seq WHERE Action='QLGX_UsePrevDir'") -ne 1) { $thieu += "$seq QLGX_UsePrevDir" }
        }
        if ((DemSql $db "SELECT Action FROM InstallExecuteSequence WHERE Action='QLGX_SetArpLoc'") -ne 1) { $thieu += 'InstallExecuteSequence QLGX_SetArpLoc' }
        DongDb $db
        if ($thieu.Count -gt 0) {
            Write-Host 'LOI: thieu cac muc sau trong MSI:'
            foreach ($t in $thieu) { Write-Host "  - $t" }
            exit 1
        }
        Write-Host 'KIEM_CHUNG_DAT'
        exit 0
    }

    # ------------------------------------------------------------------ Ghi
    $db = MoDb $Msi $MSITRANSACT

    # Xoa cac dong cu (neu chay lai lan hai) de khong bi trung.
    # Luu y: SQL cua Windows Installer KHONG ho tro LIKE, phai xoa theo ten chinh xac.
    foreach ($s in 'QLGX_InnoDir', 'QLGX_InnoDir64') { ChaySql $db "DELETE FROM ``RegLocator`` WHERE ``Signature_``='$s'" }
    ChaySql $db "DELETE FROM ``DrLocator`` WHERE ``Signature_``='QLGX_DirD'"
    foreach ($p in 'QLGXPREVDIR', 'QLGXPREVDIR64', 'QLGXPREVDIRD') { ChaySql $db "DELETE FROM ``AppSearch`` WHERE ``Property``='$p'" }
    foreach ($a in 'QLGX_UsePrevDir', 'QLGX_UsePrevDir64', 'QLGX_UsePrevDirD', 'QLGX_SetArpLoc') {
        foreach ($b in 'CustomAction', 'InstallUISequence', 'InstallExecuteSequence') {
            ChaySql $db "DELETE FROM ``$b`` WHERE ``Action``='$a'"
        }
    }

    foreach ($r in $dsRegLocator) {
        ChaySql $db ("INSERT INTO ``RegLocator`` (``Signature_``,``Root``,``Key``,``Name``,``Type``) VALUES ('{0}',{1},'{2}','{3}',{4})" -f $r[0], $r[1], $r[2], $r[3], $r[4])
    }
    # DrLocator khong co dong tuong ung trong bang Signature -> Windows Installer
    # hieu day la tim THU MUC (chu khong phai tim file), va tra ve chinh thu muc do.
    ChaySql $db ("INSERT INTO ``DrLocator`` (``Signature_``,``Parent``,``Path``,``Depth``) VALUES ('QLGX_DirD','','{0}',0)" -f $THUMUC_CU_D)

    foreach ($r in $dsAppSearch) {
        ChaySql $db ("INSERT INTO ``AppSearch`` (``Property``,``Signature_``) VALUES ('{0}','{1}')" -f $r[0], $r[1])
    }
    foreach ($r in $dsCustomAction) {
        ChaySql $db ("INSERT INTO ``CustomAction`` (``Action``,``Type``,``Source``,``Target``) VALUES ('{0}',{1},'{2}','{3}')" -f $r[0], $r[1], $r[2], $r[3])
    }
    foreach ($seq in @('InstallUISequence', 'InstallExecuteSequence')) {
        foreach ($r in $dsSequence) {
            ChaySql $db ("INSERT INTO ``$seq`` (``Action``,``Condition``,``Sequence``) VALUES ('{0}','{1}',{2})" -f $r[0], $r[1], $r[2])
        }
    }
    # Ghi ARPINSTALLLOCATION sau CostFinalize (1000) de co duong dan da phan giai
    ChaySql $db "INSERT INTO ``InstallExecuteSequence`` (``Action``,``Condition``,``Sequence``) VALUES ('QLGX_SetArpLoc','',1005)"

    $db.GetType().InvokeMember('Commit', 'InvokeMethod', $null, $db, $null) | Out-Null
    DongDb $db

    Write-Host 'DA_THEM_XONG'
    exit 0
}
catch {
    Write-Host "LOI: $($_.Exception.Message)"
    Write-Host "  tai dong $($_.InvocationInfo.ScriptLineNumber): $($_.InvocationInfo.Line.Trim())"
    exit 1
}
