Add-Type -AssemblyName System.Drawing

$src = "d:\Working\QLGX\Github\BIN\help\image"
$dst = "d:\Working\QLGX\Github\landing\public\images"
New-Item -ItemType Directory -Force -Path $dst | Out-Null

# file, cropTop, redactions "x,y,w,h,textX,textY"
$jobs = @(
  @{ f = "gia_dinh.jpg";                crop = 28; red = @() },
  @{ f = "them_giao_dan.jpg";           crop = 28; red = @("543,297,166,18,546,299", "543,408,166,18,546,410") },
  @{ f = "dot_bi_tich_chi_tiet.JPG";    crop = 24; red = @("117,149,445,19,120,151") },
  @{ f = "thong_ke.JPG";                crop = 26; red = @() },
  @{ f = "luoi_danh_sach.JPG";          crop = 0;  red = @() },
  @{ f = "rao_hon_phoi1.JPG";           crop = 0;  red = @() },
  @{ f = "report_dieu_tra_hon_phoi.JPG";crop = 0;  red = @() },
  @{ f = "sao_luu_khoi_phuc.JPG";       crop = 0;  red = @() },
  @{ f = "kiem_tra_giao_dan.JPG";       crop = 0;  red = @() }
)

$codec = [System.Drawing.Imaging.ImageCodecInfo]::GetImageEncoders() | Where-Object { $_.MimeType -eq 'image/jpeg' }
$ep = New-Object System.Drawing.Imaging.EncoderParameters(1)
$ep.Param[0] = New-Object System.Drawing.Imaging.EncoderParameter([System.Drawing.Imaging.Encoder]::Quality, 82)

foreach ($j in $jobs) {
  $in = Join-Path $src $j.f
  if (-not (Test-Path $in)) { Write-Output "MISSING $($j.f)"; continue }

  $orig = [System.Drawing.Image]::FromFile($in)
  $work = New-Object System.Drawing.Bitmap($orig.Width, $orig.Height)
  $g = [System.Drawing.Graphics]::FromImage($work)
  $g.DrawImage($orig, 0, 0, $orig.Width, $orig.Height)

  # Xoá tên giáo xứ trong các ô dữ liệu: lấy màu nền ngay trong ô rồi tô đè,
  # sau đó viết lại một tên chung. Dùng đúng font Tahoma của Windows Forms.
  foreach ($r in $j.red) {
    $p = $r.Split(',')
    $rx = [int]$p[0]; $ry = [int]$p[1]; $rw = [int]$p[2]; $rh = [int]$p[3]
    $tx = [int]$p[4]; $ty = [int]$p[5]
    $sample = $work.GetPixel($rx + $rw - 3, $ry + 2)
    $brush = New-Object System.Drawing.SolidBrush($sample)
    $g.FillRectangle($brush, $rx, $ry, $rw, $rh)
    $font = New-Object System.Drawing.Font("Tahoma", 8.25, [System.Drawing.FontStyle]::Regular, [System.Drawing.GraphicsUnit]::Point)
    $g.TextRenderingHint = [System.Drawing.Text.TextRenderingHint]::ClearTypeGridFit
    $g.DrawString("Giáo xứ ABC", $font, [System.Drawing.Brushes]::Black, $tx, $ty)
    $font.Dispose(); $brush.Dispose()
  }
  $g.Dispose()

  # Cắt bỏ thanh tiêu đề của cửa sổ Windows.
  # Tính sẵn ra biến: trong danh sách tham số, toán tử phẩy của PowerShell gộp
  # "$work.Width, $work.Height" thành mảng TRƯỚC khi trừ, gây lỗi op_Subtraction.
  $cropTop = [int]$j.crop
  $cw = [int]$work.Width
  $ch = [int]$work.Height - $cropTop
  $rect = New-Object System.Drawing.Rectangle 0, $cropTop, $cw, $ch
  $cropped = $work.Clone($rect, $work.PixelFormat)

  # Thu về tối đa 1200px chiều ngang
  $w = $cropped.Width; $h = $cropped.Height
  if ($w -gt 1200) { $h = [int]([math]::Round($h * 1200 / $w)); $w = 1200 }
  $out = New-Object System.Drawing.Bitmap($w, $h)
  $g2 = [System.Drawing.Graphics]::FromImage($out)
  $g2.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
  $g2.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
  $g2.DrawImage($cropped, 0, 0, $w, $h)
  $g2.Dispose()

  $name = [System.IO.Path]::GetFileNameWithoutExtension($j.f).ToLower() + ".jpg"
  $out.Save((Join-Path $dst $name), $codec, $ep)
  Write-Output "$name $w $h"

  $out.Dispose(); $cropped.Dispose(); $work.Dispose(); $orig.Dispose()
}
