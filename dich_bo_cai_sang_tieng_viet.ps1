<#
====================================================================================
 Dịch giao diện bộ cài sang tiếng Việt và nạp bản điều khoản sử dụng
====================================================================================

 VẤN ĐỀ:
 Bộ cài cũ làm bằng Inno Setup có sẵn tiếng Việt và có màn hình chấp nhận điều
 khoản. Bộ cài mới làm bằng Visual Studio Installer Projects thì toàn tiếng Anh,
 mà người dùng của chương trình là quý cha, quý sơ - phần lớn không rành máy tính
 và không đọc được tiếng Anh.

 VÌ SAO PHẢI VÁ SAU KHI BUILD:
 Visual Studio chỉ có sẵn giao diện cài đặt cho 14 ngôn ngữ (xem thư mục
 ...\VSI\bin\VsdDialogs\), KHÔNG có mã 1066 của tiếng Việt. Đặt LanguageId = 1066
 thì build hỏng. Cách duy nhất là để nó build ra bản tiếng Anh rồi ghi đè chữ
 tiếng Việt thẳng vào bảng Control / RadioButton / Dialog của file MSI.

 PHÔNG CHỮ:
 Bộ cài mặc định dùng "MS Sans Serif" - phông ảnh đời cũ, không có dấu tiếng Việt,
 chữ sẽ hiện thành ô vuông. Script đổi sang Tahoma (có sẵn trên mọi máy Windows từ
 năm 2000 và đủ dấu tiếng Việt).

 BẢN ĐIỀU KHOẢN:
 Ô hiển thị điều khoản là loại ScrollableText, nội dung phải ở dạng RTF. Script tự
 sinh RTF từ nội dung bên dưới, mã hoá chữ có dấu bằng \uNNNN? nên không phụ thuộc
 vào bảng mã của máy.

 CÁCH DÙNG:
     .\dich_bo_cai_sang_tieng_viet.ps1 -Msi "...\qlgx_4_0_0.msi"
     .\dich_bo_cai_sang_tieng_viet.ps1 -Msi "...\qlgx_4_0_0.msi" -ChiKiemChung
====================================================================================
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][string]$Msi,
    [switch]$ChiKiemChung
)

$ErrorActionPreference = 'Stop'

$PHONG        = 'Tahoma'
$MSITRANSACT  = 1

# Ma phong chu ma bo cai dat truoc moi doan van ban. Giu nguyen, chi doi phan chu.
$F_THUONG = '{\VSI_MS_Sans_Serif13.0_0_0}'
$F_TIEUDE = '{\VSI_MS_Sans_Serif16.0_1_0}'
$F_SHELL  = '{\VSI_MS_Shell_Dlg13.0_0_0}'

# =============================================================== Ban dieu khoan
# Lay nguyen van tu bo cai Inno Setup cu (file license.txt) de nguoi dung quen mat.
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

# ============================================================ Bang dich Control
# Moi dong: Dialog, Control, chu tieng Viet (da kem ma phong o dau).
$X = "`r`n"
# Ghi chu: moi dong phai bat dau bang dau phay. Neu khong, PowerShell se duoi
# phang cac mang con thanh mot danh sach chuoi roi rac va vong lap ben duoi se
# doc nham tung KY TU thay vi tung dong.
$dsControl = @(
    # --- Nut bam dung chung ---
    ,@('WelcomeForm','NextButton',        $F_THUONG + '&Tiếp >')
    ,@('WelcomeForm','CancelButton',      $F_THUONG + 'Hủy bỏ')
    ,@('WelcomeForm','PreviousButton',    $F_THUONG + '< &Quay lại')
    ,@('WelcomeForm','BannerText',        $F_TIEUDE + 'Chào mừng quý vị cài đặt [ProductName]')
    ,@('WelcomeForm','WelcomeText',       $F_THUONG + 'Chương trình sẽ hướng dẫn quý vị từng bước để cài [ProductName] vào máy tính này.')
    ,@('WelcomeForm','CopyrightWarningText', $F_THUONG + 'Đây là chương trình miễn phí, quý vị được toàn quyền sử dụng và chia sẻ, nhưng không được mua bán dưới bất kỳ hình thức nào. Bản quyền thuộc về tác giả Nguyễn Đức Khoan.')

    # --- Man hinh dieu khoan su dung ---
    ,@('EulaForm','NextButton',           $F_THUONG + '&Tiếp >')
    ,@('EulaForm','CancelButton',         $F_THUONG + 'Hủy bỏ')
    ,@('EulaForm','PreviousButton',       $F_THUONG + '< &Quay lại')
    ,@('EulaForm','BannerText',           $F_TIEUDE + 'Điều khoản sử dụng')
    ,@('EulaForm','BodyText',             $F_THUONG + 'Xin quý vị dành ít phút đọc điều khoản sử dụng dưới đây. Nếu quý vị đồng ý, xin chọn "Tôi đồng ý" rồi bấm "Tiếp". Nếu không đồng ý, xin bấm "Hủy bỏ".')

    # --- Chon thu muc cai dat ---
    ,@('FolderForm','NextButton',         $F_THUONG + '&Tiếp >')
    ,@('FolderForm','CancelButton',       $F_THUONG + 'Hủy bỏ')
    ,@('FolderForm','PreviousButton',     $F_THUONG + '< &Quay lại')
    ,@('FolderForm','BannerText',         $F_TIEUDE + 'Chọn thư mục cài đặt')
    ,@('FolderForm','Body',               $F_THUONG + 'Chương trình sẽ cài [ProductName] vào thư mục dưới đây.' + $X + $X + 'Nếu đồng ý với thư mục này, quý vị bấm "Tiếp". Nếu muốn cài vào thư mục khác, quý vị gõ đường dẫn vào ô bên dưới hoặc bấm "Chọn thư mục".')
    ,@('FolderForm','AllUsersText',       $F_SHELL  + 'Cài [ProductName] cho riêng quý vị, hay cho mọi người dùng máy tính này:')
    ,@('FolderForm','DiskCostButton',     $F_THUONG + '&Dung lượng đĩa...')
    ,@('FolderForm','BrowseButton',       $F_THUONG + '&Chọn thư mục...')
    ,@('FolderForm','FolderLabel',        $F_THUONG + '&Thư mục:')

    # --- Bang dung luong dia ---
    ,@('DiskCost','OKButton',             $F_THUONG + 'Đồng ý')
    ,@('DiskCost','AvailableBodyText',    $F_THUONG + 'Danh sách dưới đây là các ổ đĩa mà quý vị có thể cài [ProductName] vào, kèm dung lượng còn trống và dung lượng cần dùng của từng ổ.')
    ,@('DiskCost','RequiredBodyText',     $F_THUONG + 'Dung lượng cần dùng vượt quá dung lượng còn trống. Các dòng được tô sáng là những ổ đĩa không đủ chỗ.')

    # --- Hop thoai duyet thu muc ---
    ,@('SelectFolderDialog','CancelButton', $F_THUONG + 'Hủy bỏ')
    ,@('SelectFolderDialog','OKButton',     $F_THUONG + 'Đồng ý')
    ,@('SelectFolderDialog','BrowseText',   $F_THUONG + '&Duyệt:')
    ,@('SelectFolderDialog','FolderText',   $F_THUONG + '&Thư mục:')

    # --- Xac nhan truoc khi cai ---
    ,@('ConfirmInstallForm','NextButton',     $F_THUONG + '&Tiếp >')
    ,@('ConfirmInstallForm','CancelButton',   $F_THUONG + 'Hủy bỏ')
    ,@('ConfirmInstallForm','PreviousButton', $F_THUONG + '< &Quay lại')
    ,@('ConfirmInstallForm','BannerText',     $F_TIEUDE + 'Xác nhận cài đặt')
    ,@('ConfirmInstallForm','BodyText1',      $F_THUONG + 'Chương trình đã sẵn sàng cài [ProductName] vào máy tính của quý vị.' + $X + $X + 'Xin bấm "Tiếp" để bắt đầu cài đặt.')

    # --- Dang cai ---
    ,@('ProgressForm','NextButton',           $F_THUONG + '&Tiếp >')
    ,@('ProgressForm','CancelButton',         $F_THUONG + 'Hủy bỏ')
    ,@('ProgressForm','PreviousButton',       $F_THUONG + '< &Quay lại')
    ,@('ProgressForm','InstalledBannerText',  $F_TIEUDE + 'Đang cài [ProductName]')
    ,@('ProgressForm','InstalledBody',        $F_THUONG + 'Chương trình đang được cài vào máy, xin quý vị chờ trong giây lát.')
    ,@('ProgressForm','ProgressLabel',        $F_THUONG + 'Xin chờ...')
    ,@('ProgressForm','RemoveBannerText',     $F_TIEUDE + 'Đang gỡ bỏ [ProductName]')
    ,@('ProgressForm','RemovedBody',          $F_THUONG + 'Chương trình đang được gỡ khỏi máy, xin quý vị chờ trong giây lát.')

    # --- Xong ---
    ,@('FinishedForm','CancelButton',     $F_THUONG + 'Hủy bỏ')
    ,@('FinishedForm','PreviousButton',   $F_THUONG + '< &Quay lại')
    ,@('FinishedForm','CloseButton',      $F_THUONG + 'Đón&g')
    ,@('FinishedForm','BannerText',       $F_TIEUDE + 'Cài đặt hoàn tất')
    ,@('FinishedForm','BodyText',         $F_THUONG + 'Đã cài xong [ProductName] vào máy tính của quý vị.' + $X + $X + 'Xin bấm "Đóng" để kết thúc.')
    ,@('FinishedForm','BodyTextRemove',   $F_THUONG + 'Đã gỡ xong [ProductName] khỏi máy tính của quý vị.' + $X + $X + 'Xin bấm "Đóng" để kết thúc.')
    ,@('FinishedForm','UpdateText',       $F_THUONG + 'Quý vị nên dùng Windows Update để cập nhật các bản vá quan trọng cho .NET Framework.')

    # --- Nguoi dung bam huy giua chung ---
    ,@('UserExitForm','CancelButton',     $F_THUONG + 'Hủy bỏ')
    ,@('UserExitForm','PreviousButton',   $F_THUONG + '< &Quay lại')
    ,@('UserExitForm','CloseButton',      $F_THUONG + 'Đón&g')
    ,@('UserExitForm','BannerText',       $F_TIEUDE + 'Đã dừng cài đặt')
    ,@('UserExitForm','BodyTextInstall',  $F_THUONG + 'Việc cài đặt đã bị dừng giữa chừng nên [ProductName] chưa được cài vào máy. Quý vị hãy chạy lại bộ cài để thử lần nữa.' + $X + $X + 'Xin bấm "Đóng" để thoát.')
    ,@('UserExitForm','BodyTextRemove',   $F_THUONG + 'Việc gỡ bỏ đã bị dừng giữa chừng nên [ProductName] chưa được gỡ khỏi máy. Quý vị hãy chạy lại bộ cài để thử lần nữa.' + $X + $X + 'Xin bấm "Đóng" để thoát.')

    # --- Loi nghiem trong ---
    ,@('FatalErrorForm','CancelButton',   $F_THUONG + 'Hủy bỏ')
    ,@('FatalErrorForm','PreviousButton', $F_THUONG + '< &Quay lại')
    ,@('FatalErrorForm','CloseButton',    $F_THUONG + 'Đón&g')
    ,@('FatalErrorForm','BannerText',     $F_TIEUDE + 'Cài đặt chưa hoàn tất')
    ,@('FatalErrorForm','BodyTextInstall',$F_THUONG + 'Việc cài đặt gặp trục trặc nên [ProductName] chưa được cài vào máy. Quý vị hãy chạy lại bộ cài để thử lần nữa.' + $X + $X + 'Xin bấm "Đóng" để thoát.')
    ,@('FatalErrorForm','BodyTextRemove', $F_THUONG + 'Việc gỡ bỏ gặp trục trặc nên [ProductName] chưa được gỡ khỏi máy. Quý vị hãy chạy lại bộ cài để thử lần nữa.' + $X + $X + 'Xin bấm "Đóng" để thoát.')

    # --- Sua chua / go bo khi da cai san ---
    ,@('MaintenanceForm','CancelButton',   $F_THUONG + 'Hủy bỏ')
    ,@('MaintenanceForm','PreviousButton', $F_THUONG + '< &Quay lại')
    ,@('MaintenanceForm','FinishButton',   $F_THUONG + '&Hoàn tất')
    ,@('MaintenanceForm','BannerText',     $F_TIEUDE + '[ProductName] đã có sẵn trên máy')
    ,@('MaintenanceForm','BodyText',       $F_THUONG + 'Quý vị muốn sửa chữa hay gỡ bỏ [ProductName]?')

    # --- Cai tiep lan do dang ---
    ,@('ResumeForm','CancelButton',   $F_THUONG + 'Hủy bỏ')
    ,@('ResumeForm','PreviousButton', $F_THUONG + '< &Quay lại')
    ,@('ResumeForm','FinishButton',   $F_THUONG + '&Hoàn tất')
    ,@('ResumeForm','BannerText',     $F_TIEUDE + 'Chào mừng quý vị cài đặt [ProductName]')
    ,@('ResumeForm','BodyText',       $F_THUONG + 'Chương trình sẽ cài tiếp [ProductName] vào máy tính của quý vị.' + $X + $X + 'Xin bấm "Hoàn tất" để tiếp tục.')

    # --- Hoi truoc khi thoat ---
    ,@('Cancel','BodyText',  $F_THUONG + 'Chương trình cài đặt chưa xong. Quý vị có chắc muốn thoát không?')
    ,@('Cancel','YesButton', $F_THUONG + '&Có')
    ,@('Cancel','NoButton',  $F_THUONG + '&Không')

    # --- Hoi truoc khi go bo ---
    ,@('ConfirmRemoveDialog','BodyText',  $F_THUONG + 'Quý vị đang chọn gỡ [ProductName] khỏi máy tính. Quý vị có chắc muốn gỡ không?')
    ,@('ConfirmRemoveDialog','YesButton', $F_THUONG + '&Có')
    ,@('ConfirmRemoveDialog','NoButton',  $F_THUONG + '&Không')

    # --- Hop thoai bao loi cua Windows Installer ---
    ,@('ErrorDialog','N', $F_THUONG + '&Không')
    ,@('ErrorDialog','Y', $F_THUONG + '&Có')
    ,@('ErrorDialog','A', $F_THUONG + 'T&hoát cài đặt')
    ,@('ErrorDialog','C', $F_THUONG + 'Hủy bỏ')
    ,@('ErrorDialog','I', $F_THUONG + 'Bỏ &qua')
    ,@('ErrorDialog','O', $F_THUONG + 'Đồng ý')
    ,@('ErrorDialog','R', $F_THUONG + 'Thử &lại')

    # --- File dang bi chuong trinh khac mo ---
    ,@('FilesInUse','ExitButton',      $F_THUONG + 'T&hoát cài đặt')
    ,@('FilesInUse','ContinueButton',  $F_THUONG + 'Tiế&p tục')
    ,@('FilesInUse','RetryButton',     $F_THUONG + 'Thử &lại')
    ,@('FilesInUse','InstallBodyText', $F_THUONG + 'Những chương trình dưới đây đang mở các tập tin mà bộ cài cần thay mới. Quý vị hãy đóng các chương trình đó rồi bấm "Thử lại", hoặc bấm "Tiếp tục" để bộ cài chạy tiếp và thay các tập tin này khi khởi động lại máy.')
    ,@('FilesInUse','RemoveBodyText',  $F_THUONG + 'Những chương trình dưới đây đang mở các tập tin mà bộ cài cần xoá. Quý vị hãy đóng các chương trình đó rồi bấm "Thử lại", hoặc bấm "Tiếp tục" để bộ cài chạy tiếp và xoá các tập tin này khi khởi động lại máy.')
)

# ======================================================== Bang dich RadioButton
# Moi dong: Property, Order, chu tieng Viet
$dsRadio = @(
    ,@('EulaForm_Property',     1, $F_THUONG + 'Tôi &không đồng ý')
    ,@('EulaForm_Property',     2, $F_THUONG + 'Tôi đồn&g ý')
    ,@('FolderForm_AllUsers',   1, $F_THUONG + '&Mọi người dùng máy này')
    ,@('FolderForm_AllUsers',   2, $F_THUONG + 'Chỉ &mình tôi')
    ,@('MaintenanceForm_Action',1, $F_THUONG + '&Sửa chữa [ProductName]')
    ,@('MaintenanceForm_Action',2, $F_THUONG + '&Gỡ bỏ [ProductName]')
)

# ============================================================= Bang dich tieu de
$dsTieuDe = @(
    ,@('DiskCost',            '[ProductName] - Dung lượng đĩa')
    ,@('SelectFolderDialog',  'Chọn thư mục')
    ,@('ConfirmRemoveDialog', 'Gỡ bỏ [ProductName]')
    ,@('FilesInUse',          '[ProductName] - Tập tin đang được sử dụng')
)

# ================================================================== Ham tro giup
function MoDb($duongDan, $cheDo) {
    $script:installer = New-Object -ComObject WindowsInstaller.Installer
    return $script:installer.GetType().InvokeMember('OpenDatabase', 'InvokeMethod', $null, $script:installer, @($duongDan, $cheDo))
}
function DongDb($db) {
    [Runtime.InteropServices.Marshal]::ReleaseComObject($db) | Out-Null
    [Runtime.InteropServices.Marshal]::ReleaseComObject($script:installer) | Out-Null
    [GC]::Collect(); [GC]::WaitForPendingFinalizers()
}
function TaoBanGhi([object[]]$giaTri) {
    $r = $script:installer.GetType().InvokeMember('CreateRecord', 'InvokeMethod', $null, $script:installer, @($giaTri.Count))
    # Ghi chu: ham trong PowerShell tra ve MOI THU ghi ra luong output, khong chi
    # gia tri sau tu khoa return. Neu khong dan cac loi goi duoi day vao Out-Null
    # thi ham se tra ve mot mang thay vi mot ban ghi, va Windows Installer bao loi
    # "Execute,Params" rat kho lan ra.
    for ($k = 0; $k -lt $giaTri.Count; $k++) {
        if ($giaTri[$k] -is [int]) {
            $r.GetType().InvokeMember('IntegerData', 'SetProperty', $null, $r, @(($k + 1), $giaTri[$k])) | Out-Null
        } else {
            $r.GetType().InvokeMember('StringData', 'SetProperty', $null, $r, @(($k + 1), [string]$giaTri[$k])) | Out-Null
        }
    }
    return $r
}
function ChayCoThamSo($db, [string]$sql, [object[]]$thamSo) {
    $v = $db.GetType().InvokeMember('OpenView', 'InvokeMethod', $null, $db, @($sql))
    $r = TaoBanGhi $thamSo
    $v.GetType().InvokeMember('Execute', 'InvokeMethod', $null, $v, @($r)) | Out-Null
    $v.GetType().InvokeMember('Close', 'InvokeMethod', $null, $v, $null) | Out-Null
}
function DocMotCot($db, [string]$sql) {
    $v = $db.GetType().InvokeMember('OpenView', 'InvokeMethod', $null, $db, @($sql))
    $v.GetType().InvokeMember('Execute', 'InvokeMethod', $null, $v, $null) | Out-Null
    $ds = @()
    while ($true) {
        $r = $v.GetType().InvokeMember('Fetch', 'InvokeMethod', $null, $v, $null)
        if ($null -eq $r) { break }
        $ds += [string]$r.GetType().InvokeMember('StringData', 'GetProperty', $null, $r, @(1))
    }
    $v.GetType().InvokeMember('Close', 'InvokeMethod', $null, $v, $null) | Out-Null
    return ,$ds
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

# Chuyen van ban thuong sang RTF. Chu co dau duoc ma hoa thanh \uNNNN? nen khong
# phu thuoc bang ma cua may nguoi dung.
function SangRtf([string[]]$cacDong) {
    $sb = New-Object Text.StringBuilder
    [void]$sb.Append('{\rtf1\ansi\deff0{\fonttbl{\f0\fswiss Tahoma;}}\fs18 ')
    for ($k = 0; $k -lt $cacDong.Count; $k++) {
        foreach ($ch in $cacDong[$k].ToCharArray()) {
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

if (-not (Test-Path $Msi)) { Write-Host "LOI: khong tim thay $Msi"; exit 1 }

# ==================================================================== Kiem chung
if ($ChiKiemChung) {
    $db = MoDb $Msi 0
    $thieu = @()
    foreach ($d in $dsControl) {
        $sql = "SELECT ``Text`` FROM ``Control`` WHERE ``Dialog_``='$($d[0])' AND ``Control``='$($d[1])'"
        if ((DocMotO $db $sql) -ne $d[2]) { $thieu += "Control $($d[0]).$($d[1])" }
    }
    foreach ($d in $dsRadio) {
        $sql = "SELECT ``Text`` FROM ``RadioButton`` WHERE ``Property``='$($d[0])' AND ``Order``=$($d[1])"
        if ((DocMotO $db $sql) -ne $d[2]) { $thieu += "RadioButton $($d[0]) #$($d[1])" }
    }
    foreach ($d in $dsTieuDe) {
        if ((DocMotO $db "SELECT ``Title`` FROM ``Dialog`` WHERE ``Dialog``='$($d[0])'") -ne $d[1]) { $thieu += "Title $($d[0])" }
    }
    $phongSai = DocMotO $db "SELECT ``TextStyle`` FROM ``TextStyle`` WHERE ``FaceName``<>'$PHONG'"
    if ($phongSai) { $thieu += "phong chu chua doi: $phongSai" }
    $eula = DocMotO $db "SELECT ``Text`` FROM ``Control`` WHERE ``Dialog_``='EulaForm' AND ``Control``='LicenseText'"
    if (-not $eula -or -not $eula.StartsWith('{\rtf1')) { $thieu += 'ban dieu khoan chua duoc nap' }
    DongDb $db
    if ($thieu.Count -gt 0) {
        Write-Host "LOI: con $($thieu.Count) muc chua dung:"
        $thieu | Select-Object -First 10 | ForEach-Object { Write-Host "  - $_" }
        exit 1
    }
    Write-Host 'KIEM_CHUNG_DAT'
    exit 0
}

# ========================================================================== Ghi
$db = MoDb $Msi $MSITRANSACT

# 1. Doi phong chu de chu tieng Viet khong bi mat dau.
# Windows Installer bat buoc cau UPDATE phai co WHERE chi ro khoa chinh, khong cho
# cap nhat ca bang mot luc - nen phai lay danh sach ra roi sua tung dong.
foreach ($ten in (DocMotCot $db 'SELECT `TextStyle` FROM `TextStyle`')) {
    ChayCoThamSo $db 'UPDATE `TextStyle` SET `FaceName` = ? WHERE `TextStyle` = ?' @($PHONG, $ten)
}

# 2. Chu tren cac nut va cac doan van ban
foreach ($d in $dsControl) {
    ChayCoThamSo $db 'UPDATE `Control` SET `Text` = ? WHERE `Dialog_` = ? AND `Control` = ?' @($d[2], $d[0], $d[1])
}

# 3. Cac o chon (radio)
foreach ($d in $dsRadio) {
    ChayCoThamSo $db 'UPDATE `RadioButton` SET `Text` = ? WHERE `Property` = ? AND `Order` = ?' @($d[2], $d[0], [int]$d[1])
}

# 4. Tieu de cua so
foreach ($d in $dsTieuDe) {
    ChayCoThamSo $db 'UPDATE `Dialog` SET `Title` = ? WHERE `Dialog` = ?' @($d[1], $d[0])
}

# 5. Ban dieu khoan su dung
ChayCoThamSo $db 'UPDATE `Control` SET `Text` = ? WHERE `Dialog_` = ? AND `Control` = ?' `
    @((SangRtf $DIEU_KHOAN), 'EulaForm', 'LicenseText')

$db.GetType().InvokeMember('Commit', 'InvokeMethod', $null, $db, $null) | Out-Null
DongDb $db

Write-Host "Da dich $($dsControl.Count) muc chu, $($dsRadio.Count) o chon, $($dsTieuDe.Count) tieu de; doi phong sang $PHONG; nap ban dieu khoan"
