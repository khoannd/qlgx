#!/usr/bin/env bats
# C-4 (review bao mat) -- CAI DAT KHONG CO TEN MIEN PHAI BI CHAN.
#
# Truoc day script chi canh bao roi van cai xong, va tu thua nhan rang chay lai voi --domain=...
# KHONG co tac dung. Ket qua: mot giao xu "cai thu cho nhanh" roi nhap so sach that vao se chay
# HTTP thuan VINH VIEN -- ho ten, ngay sinh, so can cuoc cua hang nghin giao dan va moi mat khau
# dang nhap di qua internet o dang doc duoc.
#
# Hai hanh vi duoc canh o day:
#   1. Khong co ten mien  => bao_loi_va_thoat (khong cai tiep), tru khi go tuong minh
#      --cho-phep-http-khong-an-toan.
#   2. --domain= tren may DA cai (nhanh cap nhat) => sinh LAI Caddyfile co HTTPS.

setup() {
  export QLGX_CHI_NAP_HAM=1
  export GOC_CHECKOUT="$BATS_TEST_TMPDIR/opt-qlgx"
  export GOC_UNG_DUNG="$GOC_CHECKOUT/WebApp"
  export THU_MUC_CAU_HINH="$BATS_TEST_TMPDIR/etc-qlgx"
  mkdir -p "$GOC_UNG_DUNG" "$THU_MUC_CAU_HINH"
  # shellcheck disable=SC1090
  source "${BATS_TEST_DIRNAME}/../../scripts/install.sh"
  KHONG_TUONG_TAC=1   # khong co /dev/tty trong bo test
  TEN_MIEN=""
  CHO_PHEP_HTTP=0
}

# ===================== 1. Chan khi thieu ten mien =====================

@test "C-4: khong ten mien, khong co thoat hiem => DUNG cai dat" {
  run chan_neu_thieu_ten_mien
  [ "$status" -ne 0 ]
  # Phai noi ro bang tieng Viet vi sao bi chan VA cach di tiep.
  [[ "$output" == *"DUNG CAI DAT"* ]]
  [[ "$output" == *"HTTP THUAN"* ]]
  [[ "$output" == *"--domain="* ]]
  [[ "$output" == *"--cho-phep-http-khong-an-toan"* ]]
}

@test "C-4: co ten mien hop le => di tiep binh thuong" {
  TEN_MIEN="giaoxu-abc.net"
  run chan_neu_thieu_ten_mien
  [ "$status" -eq 0 ]
}

@test "C-4: co thoat hiem tuong minh => cho di tiep nhung phai CANH BAO to" {
  CHO_PHEP_HTTP=1
  run chan_neu_thieu_ten_mien
  [ "$status" -eq 0 ]
  [[ "$output" == *"HTTP THUAN"* ]]
  [[ "$output" == *"KHONG nhap so sach that"* ]]
}

@test "C-4: co --cho-phep-http-khong-an-toan duoc phan tich tham so nhan ra" {
  CHO_PHEP_HTTP=0
  phan_tich_tham_so --non-interactive --cho-phep-http-khong-an-toan
  [ "$CHO_PHEP_HTTP" -eq 1 ]
}

# ===================== 2. Kiem dang ten mien =====================

@test "C-4: ten mien dung dang thi hop le" {
  for tm in giaoxu-abc.net gx.giaophan.vn a.b sub.domain.co.uk; do
    run ten_mien_hop_le "$tm"
    [ "$status" -eq 0 ]
  done
}

@test "C-4: ten mien sai dang bi tu choi (khong sinh ra Caddyfile hong am tham)" {
  for tm in "https://gx.net" "gx.net/" "gx net" "khongcodaucham" "-gx.net" "gx..net" ""; do
    run ten_mien_hop_le "$tm"
    [ "$status" -ne 0 ]
  done
}

@test "C-4: ten mien sai dang lam cai dat DUNG LAI, khong cai tiep" {
  TEN_MIEN="https://gx.net/"
  run chan_neu_thieu_ten_mien
  [ "$status" -ne 0 ]
  [[ "$output" == *"khong hop le"* ]]
}

# ===================== 3. Caddyfile sinh ra dung =====================

@test "C-4: co ten mien => Caddyfile co khoi HTTPS va du cac header bao mat" {
  TEN_MIEN="giaoxu-abc.net"
  ghi_caddyfile
  grep -qx "giaoxu-abc.net {" "$GOC_UNG_DUNG/Caddyfile"
  grep -q "Strict-Transport-Security" "$GOC_UNG_DUNG/Caddyfile"
  grep -q "X-Content-Type-Options" "$GOC_UNG_DUNG/Caddyfile"
  grep -q "X-Frame-Options" "$GOC_UNG_DUNG/Caddyfile"
  grep -q "max_size 10MB" "$GOC_UNG_DUNG/Caddyfile"
  ! grep -q "^:80 {" "$GOC_UNG_DUNG/Caddyfile"
}

@test "C-4: khong ten mien (da go thoat hiem) => Caddyfile la khoi :80" {
  TEN_MIEN=""
  ghi_caddyfile
  grep -qx ":80 {" "$GOC_UNG_DUNG/Caddyfile"
}

# ===================== 4. --domain= co tac dung o nhanh cap nhat =====================

@test "C-4: cap nhat kem --domain= sinh LAI Caddyfile HTTPS tu ban :80 cu" {
  printf ':80 {\n\treverse_proxy api:8080\n}\n' > "$GOC_UNG_DUNG/Caddyfile"
  TEN_MIEN="giaoxu-abc.net"
  local goi_dc=0
  dc() { goi_dc=1; return 0; }
  run cap_nhat_https_neu_co_ten_mien
  [ "$status" -eq 0 ]
  grep -qx "giaoxu-abc.net {" "$GOC_UNG_DUNG/Caddyfile"
  grep -q "Strict-Transport-Security" "$GOC_UNG_DUNG/Caddyfile"
}

@test "C-4: cap nhat KHONG kem --domain= thi khong dung toi Caddyfile dang chay" {
  printf ':80 {\n\treverse_proxy api:8080\n}\n' > "$GOC_UNG_DUNG/Caddyfile"
  TEN_MIEN=""
  dc() { return 0; }
  run cap_nhat_https_neu_co_ten_mien
  [ "$status" -eq 0 ]
  grep -qx ":80 {" "$GOC_UNG_DUNG/Caddyfile"
}

@test "C-4: cap nhat voi ten mien DA dung san thi khong sinh lai, khong khoi dong lai caddy" {
  TEN_MIEN="giaoxu-abc.net"
  ghi_caddyfile
  dc() { echo "KHONG DUOC GOI dc"; return 1; }
  run cap_nhat_https_neu_co_ten_mien
  [ "$status" -eq 0 ]
  [[ "$output" != *"KHONG DUOC GOI dc"* ]]
}
