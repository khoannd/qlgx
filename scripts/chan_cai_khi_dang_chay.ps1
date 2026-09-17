<#
====================================================================================
 Chặn cài đặt nếu GiaoXu.exe đang chạy
====================================================================================

 LỖI THẬT ĐÃ XẢY RA (báo cáo từ người dùng, xác nhận bằng cách đọc thật code và
 bảng MSI - không phải suy đoán):

 Cài bản mới đè lên máy đang chạy GiaoXu.exe thì bộ cài báo THÀNH CÔNG, nhưng mở
 chương trình lên KHÔNG CHẠY ĐƯỢC. Gỡ bản cũ rồi cài lại bản mới thì chạy được.
 Người dùng tự kiểm chứng thêm: chép đè tay các file trong BIN\ (không bị khoá gì)
 lên thư mục cài đặt cũng sửa được ngay.

 NGUYÊN NHÂN:
 Windows Installer cần ghi đè GiaoXu.exe và các DLL nó nạp vào bộ nhớ (GXControl.dll,
 GXGlobal.dll...). Nếu GiaoXu.exe đang chạy, các file này đang BỊ KHOÁ. Bộ cài (do
 Visual Studio Installer Projects tự sinh) có sẵn hộp thoại "Tập tin đang được sử
 dụng" (FilesInUse) với hai lựa chọn:
   - "Thử lại"   : đóng chương trình rồi bấm lại - AN TOÀN.
   - "Tiếp tục"  : bộ cài chạy tiếp NHƯNG CHỈ HOÃN việc thay các file bị khoá đến
                   LẦN KHỞI ĐỘNG LẠI MÁY TIẾP THEO - và vẫn báo "Cài đặt thành công".

 Người dùng của phần mềm này là quý cha, quý sơ - không hiểu ý nghĩa của "Tiếp tục"
 nên bấm nó. Bộ cài báo thành công, họ mở chương trình ngay (không khởi động lại
 máy) -> chạy với HỖN HỢP file cũ (bị khoá, chưa thay) và file mới (không bị khoá,
 đã thay) -> GiaoXu.exe mới gọi API không có trong GXControl.dll cũ (hoặc ngược
 lại) -> crash ngay khi mở, không có thông báo lỗi rõ ràng nào cho người dùng thấy.

 Gỡ bản cũ rồi cài lại "sửa" được vì lúc đó GiaoXu.exe đã dừng (crash hoặc người
 dùng đã đóng) nên không còn file nào bị khoá - bộ cài lần hai thay được TOÀN BỘ.

 ĐÃ LOẠI CÁC GIẢ THUYẾT KHÁC BẰNG CACH KIỂM CHỨng THẬT (không phải đoán):
   - GUID của Component: ỔN ĐỊNH giữa các lần build khác nhau (so sánh MSI thật
     của 4.0.1 và 4.0.2: 145/145 file trùng GUID). Không phải lỗi "component rule
     violation" giữa các bản.
   - Danh sách file trong 2 bản: GIỐNG HỆT NHAU, không file nào bị bỏ sót.
   - giaoxu.mdb: KHÔNG nằm trong bộ cài hay gói cập nhật.
   - Các file .config (GiaoXu.exe.config...) không có version, nhưng NỘI DUNG
     giống hệt nhau giữa các bản gần đây nên không phải nguyên nhân riêng.

 CÁCH SỬA:
 Thêm một Custom Action chạy RẤT SỚM trong InstallExecuteSequence (trước
 LaunchConditions, trước khi bất kỳ file nào bị đụng tới). Nếu phát hiện
 GiaoXu.exe đang chạy, HIỂN THỊ THÔNG BÁO RÕ RÀNG rồi HUỶ VIỆC CÀI ĐẶT NGAY -
 không cho đi tới màn hình "Tiếp tục" nguy hiểm kia nữa. Người dùng buộc phải
 đóng chương trình rồi chạy lại bộ cài - lúc đó không còn file nào bị khoá.

 VÌ SAO KHÔNG TỰ ĐỘNG ĐÓNG (KILL) CHƯƠNG TRÌNH THAY VÌ HUỶ CÀI ĐẶT:
 GiaoXu.exe đang mở kết nối tới giaoxu.mdb (Microsoft Access/Jet). Buộc đóng bằng
 taskkill /F có thể cắt ngang một lần ghi dữ liệu, làm hỏng file .mdb - dữ liệu là
 sổ sách giáo xứ nhiều năm, mất là không lấy lại được. Huỷ cài đặt và yêu cầu người
 dùng tự đóng chương trình an toàn hơn nhiều, dù kém tiện hơn một chút.

 VÌ SAO DÙNG VBSCRIPT (Type 6) CHỨ KHÔNG PHẢI DẠNG DEFERRED NHƯ CÁC CUSTOM ACTION
 KHÁC (vd go_dau_vet_ban_cu.ps1):
 Custom action này cần hiển thị MsgBox cho người dùng thấy NGAY, và cần huỷ được
 cả tiến trình cài đặt trước khi bất kỳ file nào bị đụng tới. Custom action dạng
 "deferred" (như go_dau_vet_ban_cu.ps1 dùng) chạy trong "installer script" đã lên
 lịch sẵn, không thể huỷ ngang tiến trình cài đặt giữa chừng theo cách này. Custom
 action Type 6 (VBScript, immediate) có quyền truy cập Session ngay lúc chạy, đọc
 được UILevel để biết cài đặt có đang chạy im lặng (/qn) hay không, và có thể huỷ
 cài đặt bằng cách gây lỗi (Err.Raise).

 KHÔNG HIỂN THỊ MsgBox KHI CÀI ĐẶT IM LẶNG (/qn):
 Nếu hiển thị MsgBox lúc cài /qn, hộp thoại sẽ treo vô thời hạn vì không có ai bấm.
 Script tự đọc UILevel để quyết định có hiện thông báo hay không; trong mọi trường
 hợp vẫn huỷ cài đặt nếu phát hiện chương trình đang chạy.

 CÁCH DÙNG:
     .\chan_cai_khi_dang_chay.ps1 -Msi "...\qlgx_4_0_3.msi"
     .\chan_cai_khi_dang_chay.ps1 -Msi "...\qlgx_4_0_3.msi" -ChiKiemChung
====================================================================================
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][string]$Msi,
    [switch]$ChiKiemChung
)

$ErrorActionPreference = 'Stop'

$TEN_CA      = 'QLGX_ChanKhiDangChay'
$MSITRANSACT = 1

# Type 38 = VBScript nam NGAY TRONG cot Target (khong tra cuu bang Binary/File).
# CAN THAN: 37 la JSCRIPT inline, 38 moi la VBSCRIPT inline - de nham lan (da tung
# nham thanh 37 luc dau, MSI bao "JavaScript compilation error" vi hieu script VBS
# thanh JS). Type 6 (msidbCustomActionTypeVBScript don thuan, khong cong them gi)
# nghia la khac han: "Source tro toi mot dong trong bang Binary" - dung Type 6 ma
# khong co dong Binary tuong ung se bao loi "not found in Binary table stream" (da
# tung gap dung loi nay khi thu nghiem, truoc khi phat hien phai dung 38).
# Khong cong them co deferred/no-impersonate/continue vi ta CAN loi nay lam THAT BAI
# ca tien trinh cai dat (huy cai dat), va can chay NGAY (immediate) de co Session
# (doc duoc UILevel) va hien MsgBox duoc - custom action dang deferred KHONG doc
# duoc Session.Property tuy y.
$KIEU_CA = 38

# Chan chinh minh khoi tai dien loi 37/38 (da tung xay ra that trong go_dau_vet_ban_cu.ps1).
if ($KIEU_CA -ne 38) { Write-Host 'LOI LAP TRINH: KIEU_CA phai la 38 (VBScript inline).'; exit 1 }

# Chay ngay sau AppSearch (100), TRUOC LaunchConditions (400) va TRUOC BAT KY buoc nao
# dung toi file. Dieu kien NOT Installed: dung cho ca cai moi lan dau LAN nang cap
# (vi ProductCode doi moi lan build nen "Installed" luon rong voi san pham moi).
$THU_TU    = 150
$DIEU_KIEN = 'NOT Installed'

$VBS = @'
Dim wmi, procs, soTienTrinh, uiLevel, thongBao

' CAN THAN: "On Error Resume Next" nuot TAT CA loi, KE CA Err.Raise cua chinh minh
' goi ben duoi - da tung gap dung loi nay khi thu nghiem that: script phat hien dung
' GiaoXu.exe dang chay (dem duoc procs.Count > 0) nhung lenh Err.Raise de huy cai dat
' bi nuot mat, cai dat van tiep tuc chay binh thuong nhu khong co gi xay ra!
'
' Vi vay CHI bat loi (On Error Resume Next) quanh phan DO TIM (de khong lam hong ca
' buoc cai dat neu may nao do WMI bi loi/tat - fail-open, cho cai tiep). Ngay khi DA
' XAC DINH duoc so tien trinh, chuyen ve che do bao loi binh thuong (On Error Goto 0)
' TRUOC KHI goi Err.Raise, de lenh huy cai dat chac chan co hieu luc.
soTienTrinh = 0
On Error Resume Next
Set wmi = GetObject("winmgmts:\\.\root\cimv2")
If Err.Number = 0 Then
    Set procs = wmi.ExecQuery("SELECT * FROM Win32_Process WHERE Name='GiaoXu.exe'")
    If Err.Number = 0 Then soTienTrinh = procs.Count
End If
On Error Goto 0

If soTienTrinh > 0 Then
    uiLevel = Session.Property("UILevel")
    thongBao = "Chuong trinh QLGX (GiaoXu.exe) dang chay tren may nay." & Chr(13) & Chr(10) & Chr(13) & Chr(10) & _
        "Xin vui long DONG chuong trinh lai truoc, roi chay lai bo cai nay." & Chr(13) & Chr(10) & Chr(13) & Chr(10) & _
        "(Neu tiep tuc cai dat trong khi chuong trinh dang mo, mot so tap tin se khong duoc thay moi dung cach, " & _
        "va chuong trinh co the khong mo len duoc sau khi cai xong.)"
    If CInt(uiLevel) >= 3 Then
        MsgBox thongBao, 16, "Khong the tiep tuc cai dat"
    End If
    Err.Raise vbObjectError + 1, "QLGX", "GiaoXu.exe dang chay - da huy cai dat de tranh lam hong tap tin chuong trinh"
End If
'@

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
            $r.GetType().InvokeMember('StringData', 'SetProperty', $null, $r, @(($k + 1), [string]$thamSo[$k])) | Out-Null
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
    $target = DocMotO $db "SELECT ``Target`` FROM ``CustomAction`` WHERE ``Action``='$TEN_CA'"
    if (-not $target -or -not $target.Contains('GiaoXu.exe')) { $thieu += 'noi dung VBScript khong kiem tra GiaoXu.exe' }
    if (-not $target -or -not $target.Contains('Err.Raise')) { $thieu += 'noi dung VBScript khong huy cai dat khi phat hien' }
    $dk = DocMotO $db "SELECT ``Condition`` FROM ``InstallExecuteSequence`` WHERE ``Action``='$TEN_CA'"
    if ($dk -ne $DIEU_KIEN) { $thieu += "dieu kien chay trong InstallExecuteSequence (doc duoc: '$dk')" }
    $tt = DocMotO $db "SELECT ``Sequence`` FROM ``InstallExecuteSequence`` WHERE ``Action``='$TEN_CA'"
    if ($tt -ne "$THU_TU") { $thieu += "thu tu chay trong InstallExecuteSequence (doc duoc: '$tt')" }
    # Phai chay TRUOC InstallFiles, neu khong thi kiem tra vo nghia (file da bi dung toi roi)
    $ttInstallFiles = DocMotO $db "SELECT ``Sequence`` FROM ``InstallExecuteSequence`` WHERE ``Action``='InstallFiles'"
    if ($ttInstallFiles -and [int]$tt -ge [int]$ttInstallFiles) { $thieu += "chay sau InstallFiles ($ttInstallFiles) - qua tre, file da bi dung toi" }
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

# Xoa dong cu neu chay lai lan hai
foreach ($bang in 'CustomAction', 'InstallExecuteSequence') {
    ChaySql $db "DELETE FROM ``$bang`` WHERE ``Action``='$TEN_CA'" $null
}

ChaySql $db 'INSERT INTO `CustomAction` (`Action`,`Type`,`Source`,`Target`) VALUES (?,?,?,?)' `
    @($TEN_CA, [int]$KIEU_CA, '', $VBS)
ChaySql $db 'INSERT INTO `InstallExecuteSequence` (`Action`,`Condition`,`Sequence`) VALUES (?,?,?)' `
    @($TEN_CA, $DIEU_KIEN, [int]$THU_TU)

$db.GetType().InvokeMember('Commit', 'InvokeMethod', $null, $db, $null) | Out-Null
DongDb $db

Write-Host "Da them buoc chan cai dat khi GiaoXu.exe dang chay (thu tu $THU_TU, truoc InstallFiles)"
