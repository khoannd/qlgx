$out = 'C:\Users\khoannd\AppData\Local\Temp\verify_out.txt'
$sb = New-Object System.Text.StringBuilder

# EKU cho phep ky ma nguon (Authenticode)
$OID_CODE_SIGNING = '1.3.6.1.5.5.7.3.3'

function Xem-Kho($duongDan, $tenKho) {
    [void]$sb.AppendLine("=== $tenKho ($duongDan) ===")
    $certs = Get-ChildItem $duongDan -ErrorAction SilentlyContinue
    if (-not $certs) { [void]$sb.AppendLine('  (trong)'); [void]$sb.AppendLine(''); return }

    foreach ($c in $certs) {
        # Liet ke EKU
        $ekus = @()
        foreach ($ext in $c.Extensions) {
            if ($ext.Oid.Value -eq '2.5.29.37') {
                $eku = $ext -as [Security.Cryptography.X509Certificates.X509EnhancedKeyUsageExtension]
                if ($eku) { foreach ($o in $eku.EnhancedKeyUsages) { $ekus += "$($o.FriendlyName) [$($o.Value)]" } }
            }
        }
        $koKyMa = ($ekus | Where-Object { $_ -match [regex]::Escape($OID_CODE_SIGNING) }).Count -gt 0
        $conLai = ($c.NotAfter - (Get-Date)).Days

        [void]$sb.AppendLine("  Subject : $($c.Subject)")
        [void]$sb.AppendLine("  Issuer  : $($c.Issuer)")
        [void]$sb.AppendLine("  Han     : $($c.NotAfter.ToString('yyyy-MM-dd')) ($(if($conLai -lt 0){'DA HET HAN'}else{"con $conLai ngay"}))")
        [void]$sb.AppendLine("  Co khoa bi mat : $($c.HasPrivateKey)")
        if ($ekus.Count -gt 0) {
            [void]$sb.AppendLine('  Muc dich su dung (EKU):')
            foreach ($e in $ekus) { [void]$sb.AppendLine("      - $e") }
        } else {
            [void]$sb.AppendLine('  Muc dich su dung (EKU): (khong gioi han)')
        }
        [void]$sb.AppendLine("  >>> KY DUOC PHAN MEM WINDOWS? $(if($koKyMa){'CO (co EKU Code Signing)'}else{'KHONG (thieu EKU Code Signing)'})")

        # Kiem tra chuoi tin cay
        $chain = New-Object Security.Cryptography.X509Certificates.X509Chain
        $chain.ChainPolicy.RevocationMode = 'NoCheck'
        $ok = $chain.Build($c)
        [void]$sb.AppendLine("  Chuoi tin cay hop le tren may nay: $ok")
        if (-not $ok -and $chain.ChainStatus.Count -gt 0) {
            foreach ($st in $chain.ChainStatus) { [void]$sb.AppendLine("      ly do: $($st.StatusInformation.Trim())") }
        }
        if ($chain.ChainElements.Count -gt 0) {
            $root = $chain.ChainElements[$chain.ChainElements.Count - 1].Certificate
            [void]$sb.AppendLine("  Root cuoi chuoi: $($root.Subject)")
        }
        [void]$sb.AppendLine('')
    }
}

Xem-Kho 'Cert:\CurrentUser\My'  'Chung thu ca nhan cua user'
Xem-Kho 'Cert:\LocalMachine\My' 'Chung thu cua may'

# Tim moi chung thu lien quan VINA / Viet Nam trong cac kho root
[void]$sb.AppendLine('=== Do tim CA Viet Nam trong kho Root/Trung gian ===')
$timThay = $false
foreach ($kho in 'Cert:\LocalMachine\Root', 'Cert:\LocalMachine\CA', 'Cert:\CurrentUser\Root', 'Cert:\CurrentUser\CA') {
    Get-ChildItem $kho -ErrorAction SilentlyContinue | Where-Object {
        $_.Subject -match 'VINA|Vietnam|Viet Nam|VNPT|Viettel|FPT-CA|BKAV|NEAC|MIC'
    } | ForEach-Object {
        $timThay = $true
        [void]$sb.AppendLine("  [$kho] $($_.Subject)")
    }
}
if (-not $timThay) { [void]$sb.AppendLine('  Khong tim thay CA Viet Nam nao trong kho tin cay cua may nay') }

[IO.File]::WriteAllText($out, $sb.ToString(), (New-Object Text.UTF8Encoding $false))
