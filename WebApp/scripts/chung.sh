#!/usr/bin/env bash
# Thu vien ham dung chung cho moi script van hanh QLGX. Chi dinh nghia ham, KHONG chay gi khi
# duoc source -- de bo test bats nap vao va goi tung ham rieng le.

ghi_log() {
  local muc="$1"; shift
  local dau
  case "$muc" in
    thong-tin) dau="[ ]" ;;
    canh-bao)  dau="[!]" ;;
    loi)       dau="[X]" ;;
    *)         dau="[ ]" ;;
  esac
  printf '%s %s %s\n' "$(date '+%H:%M:%S')" "$dau" "$*" >&2
}

bao_loi_va_thoat() { ghi_log loi "$*"; exit 1; }

# Sinh mot bi mat ngau nhien dang base64. Doc thang tu /dev/urandom thay vi goi openssl de
# chay duoc tren may chua cai openssl (buoc dau cua install.sh, truoc khi cai phu thuoc).
sinh_bi_mat() {
  local so_byte="${1:-32}"
  head -c "$so_byte" /dev/urandom | base64 | tr -d '\n'
}

doc_env_kv() {
  local tep="$1" khoa="$2"
  [ -f "$tep" ] || { printf ''; return 0; }
  # Cat dung mot lan o dau '=' dau tien -- gia tri co the chua dau '=' (base64 co the ket thuc
  # bang '='). Dung khop chinh xac tu dau dong de khong nham khoa co tien to giong nhau.
  local dong
  dong=$(grep -m1 "^${khoa}=" "$tep" || true)
  printf '%s' "${dong#*=}"
}

# Dat mot khoa trong tep .env. CO Y khong dung sed -i voi gia tri nguoi dung: gia tri co the
# chua '/', '&', '|' -- moi ky tu do deu co nghia dac biet trong sed va se lam hong mat khau.
# Thay vao do doc-loc-ghi bang awk voi gia tri truyen qua bien, khong qua mau thay the.
set_env_kv() {
  local tep="$1" khoa="$2" gia_tri="$3"
  local tep_moi=0
  [ -e "$tep" ] || tep_moi=1
  touch "$tep"
  # Tep MOI tao (chua tung ton tai) phai duoc ep quyen 600 NGAY, khong duoc de umask he
  # thong quyet dinh. Tep nay se chua bi mat (mat khau CSDL, khoa JWT) -- kich ban that la
  # install.sh tao .env lan dau, umask mac dinh cua he thong (thuong la 022) se tao tep 644,
  # doc duoc boi moi nguoi dung tren may. Tep DA CO san thi giu nguyen quyen nguoi van hanh
  # da chon, khong tu y sua.
  if [ "$tep_moi" -eq 1 ]; then chmod 600 "$tep"; fi

  # Bao dam tep ket thuc bang xuong dong TRUOC khi noi them -- thieu buoc nay thi khoa moi bi
  # dinh vao cuoi dong cuoi, tao ra mot dong hong va lam mat ca hai gia tri.
  [ -s "$tep" ] && [ "$(tail -c1 "$tep" | wc -l)" -eq 0 ] && printf '\n' >> "$tep"

  # KHONG doc quyen bang stat roi chmod cho tep tam nhu truoc: stat -c la cu phap GNU
  # coreutils, that bai lang le tren BusyBox/Alpine, va nhanh du phong khi do se ep quyen ve
  # 600 -- ghi de len quyen mot tep da co san ma nguoi van hanh co the co y dat khac 600.
  # Thay vao do: ghi noi dung THANG vao dung inode cua $tep dang co san bang "cat > ", vi
  # ghi de noi dung mot inode co san thi quyen cua inode do tu dong giu nguyen -- khong can
  # doc, khong can copy, khong co khe ho nao cho stat that bai. Van phai dung tep tam truoc
  # (awk khong the vua doc vua ghi cung mot tep), nhung buoc cuoi la "cat" chu khong phai
  # "mv" (mv thay ca inode nen moi can copy quyen thu cong).
  local tam="${tep}.tam.$$"
  awk -v k="$khoa" -v v="$gia_tri" '
    BEGIN { da_ghi = 0 }
    index($0, k "=") == 1 { if (!da_ghi) { print k "=" v; da_ghi = 1 } ; next }
    { print }
    END { if (!da_ghi) print k "=" v }
  ' "$tep" > "$tam"
  cat "$tam" > "$tep"
  rm -f "$tam"
}

# Chi ghi khi khoa CHUA CO hoac dang rong. Day la nen tang cua tinh idempotent: chay lai
# install.sh tren mot he thong dang song KHONG duoc phep sinh lai bi mat nao -- doi
# QLGX_JWT_KEY se dang xuat toan bo nguoi dung, doi mat khau CSDL se lam API mat ket noi vao
# chinh CSDL dang chay.
dat_neu_chua_co() {
  local tep="$1" khoa="$2" gia_tri="$3"
  local hien_tai
  hien_tai=$(doc_env_kv "$tep" "$khoa")
  [ -n "$hien_tai" ] && return 0
  set_env_kv "$tep" "$khoa" "$gia_tri"
}
