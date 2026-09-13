#!/usr/bin/env bash
# Cai dat VA cap nhat QLGX Web tren mot may chu Linux.
#
#   curl -fsSL https://raw.githubusercontent.com/khoannd/qlgx/webapp-phase-1/WebApp/scripts/install.sh | sudo bash
#
# Mot script duy nhat cho ca hai viec la CO CHU DICH: no loai bo loai loi kinh dien "duong cai
# moi thi dung, duong nang cap thieu buoc". Script tu nhan biet trang thai bang su ton tai cua
# $GOC_UNG_DUNG/.env.
set -euo pipefail

readonly NHANH_MAC_DINH="webapp-phase-1"
readonly KHO_GIT="https://github.com/khoannd/qlgx.git"
readonly GOC_CHECKOUT="/opt/qlgx"
GOC_UNG_DUNG="${GOC_UNG_DUNG:-$GOC_CHECKOUT/WebApp}"
THU_MUC_CAU_HINH="${THU_MUC_CAU_HINH:-/etc/qlgx}"
THU_MUC_SPOOL="${THU_MUC_SPOOL:-/var/lib/qlgx/spool}"
THU_MUC_LOG="${THU_MUC_LOG:-/var/log/qlgx}"

NHANH="$NHANH_MAC_DINH"
KHONG_TUONG_TAC=0
CHAY_THU=0
CHI_TRANG_THAI=0
BO_QUA_SAO_LUU=0
TEN_MIEN="${QLGX_TEN_MIEN:-}"

# Nap thu vien dung chung. Khi chay qua `curl | bash` thi hai tep nay chua co tren dia -- tai
# rieng chung ve thu muc tam truoc. Khi chay tu ban checkout thi nap thang.
nap_thu_vien() {
  local thu_muc_ke_ben
  thu_muc_ke_ben="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
  if [ -f "$thu_muc_ke_ben/chung.sh" ]; then
    # shellcheck source=/dev/null
    source "$thu_muc_ke_ben/chung.sh"
    # shellcheck source=/dev/null
    source "$thu_muc_ke_ben/os_adapter.sh"
    return
  fi

  # Chua co tren dia (truong hop curl | bash): tai rieng hai tep ve thu muc tam. Kiem tra
  # ma thoat cua tung lenh curl RIENG (khong dua vao set -e mot minh) de bao loi RO RANG khi
  # mat mang hoac go sai ten nhanh -- neu khong nguoi van hanh se thay mot loi cu phap kho
  # hieu o vai chuc dong sau, luc tep chung.sh/os_adapter.sh rong hoac la trang loi 404 cua
  # GitHub duoc source nhu the la shell script.
  local tam goc_raw
  tam=$(mktemp -d)
  goc_raw="https://raw.githubusercontent.com/khoannd/qlgx/$NHANH/WebApp/scripts"
  if ! curl -fsSL "$goc_raw/chung.sh" -o "$tam/chung.sh"; then
    echo "[X] Khong tai duoc chung.sh tu $goc_raw -- kiem tra mang hoac ten nhanh (--branch=$NHANH)." >&2
    exit 1
  fi
  if ! curl -fsSL "$goc_raw/os_adapter.sh" -o "$tam/os_adapter.sh"; then
    echo "[X] Khong tai duoc os_adapter.sh tu $goc_raw -- kiem tra mang hoac ten nhanh (--branch=$NHANH)." >&2
    exit 1
  fi
  # shellcheck source=/dev/null
  source "$tam/chung.sh"
  # shellcheck source=/dev/null
  source "$tam/os_adapter.sh"
}
nap_thu_vien

la_cai_moi() { [ ! -f "$GOC_UNG_DUNG/.env" ]; }

phan_tich_tham_so() {
  for t in "$@"; do
    case "$t" in
      --non-interactive) KHONG_TUONG_TAC=1 ;;
      --dry-run)         CHAY_THU=1 ;;
      --status)          CHI_TRANG_THAI=1 ;;
      --skip-backup)     BO_QUA_SAO_LUU=1 ;;
      --domain=*)        TEN_MIEN="${t#*=}" ;;
      --branch=*)        NHANH="${t#*=}" ;;
      *) bao_loi_va_thoat "Tham so khong hieu: $t" ;;
    esac
  done
}

kiem_tra_tien_de() {
  [ "$(id -u)" -eq 0 ] || bao_loi_va_thoat "Phai chay bang quyen root (dung sudo)."
  os_ho >/dev/null || bao_loi_va_thoat "Ban phan phoi Linux nay chua duoc ho tro."

  local so_cpu ram_mb dia_gb
  so_cpu=$(nproc)
  ram_mb=$(awk '/MemTotal/ {print int($2/1024)}' /proc/meminfo)
  dia_gb=$(df -BG --output=avail /opt 2>/dev/null | tail -1 | tr -dc '0-9')

  # CPU thap la CANH BAO (may van chay duoc, chi cham hon) -- KHONG chan cai dat. RAM/dia
  # duoi nguong la LOI CHAN CUNG: PostgreSQL + API + Docker se khong khoi dong noi hoac oom-
  # kill giua chung, va he thong het dia se hong ca sao luu lan CSDL dang chay.
  [ "$so_cpu" -ge 2 ]     || ghi_log canh-bao "Chi co $so_cpu CPU (khuyen nghi toi thieu 2)."
  [ "$ram_mb" -ge 3500 ]  || bao_loi_va_thoat "RAM $ram_mb MB, can toi thieu 4 GB."
  [ "${dia_gb:-0}" -ge 40 ] || bao_loi_va_thoat "Con ${dia_gb:-0} GB trong, can toi thieu 40 GB."
  ghi_log thong-tin "Tien de dat: $so_cpu CPU, $ram_mb MB RAM, ${dia_gb} GB trong."
}

cai_phu_thuoc() {
  local can=()
  os_co_lenh git    || can+=(git)
  os_co_lenh curl   || can+=(curl)
  os_co_lenh restic || can+=(restic)
  if [ ${#can[@]} -gt 0 ]; then
    ghi_log thong-tin "Cai goi: ${can[*]}"
    [ "$CHAY_THU" -eq 1 ] || os_cai_goi "${can[@]}"
  fi

  if ! os_co_lenh docker; then
    ghi_log thong-tin "Cai Docker Engine tu kho chinh thuc cua Docker."
    [ "$CHAY_THU" -eq 1 ] || curl -fsSL https://get.docker.com | sh
  fi
  docker compose version >/dev/null 2>&1 \
    || bao_loi_va_thoat "Thieu Docker Compose plugin (docker compose). Cai lai Docker Engine."
  [ "$CHAY_THU" -eq 1 ] || systemctl enable --now docker
}

cau_hinh_tuong_lua() {
  # Chi mo 22/80/443. Cong PostgreSQL KHONG BAO GIO mo ra ngoai -- docker-compose.yml cung
  # khong map cong 5432 ra host.
  if os_co_lenh ufw; then
    ufw allow 22/tcp >/dev/null; ufw allow 80/tcp >/dev/null; ufw allow 443/tcp >/dev/null
    ufw --force enable >/dev/null
    ghi_log thong-tin "Tuong lua ufw: chi mo 22, 80, 443."
  elif os_co_lenh firewall-cmd; then
    systemctl enable --now firewalld
    firewall-cmd --permanent --add-service=ssh --add-service=http --add-service=https >/dev/null
    firewall-cmd --reload >/dev/null
    ghi_log thong-tin "Tuong lua firewalld: chi mo ssh, http, https."
  else
    # Khong co ca hai cong cu: CANH BAO roi TIEP TUC, khong dung script lai -- nhieu may chu
    # (vd container, VPS da co tuong lua quan ly rieng o tang mang) khong co ufw/firewalld
    # nhung van an toan. Chan cung o day se lam hong ca duong cai dat lan duong nang cap.
    ghi_log canh-bao "Khong tim thay ufw/firewalld -- hay tu cau hinh tuong lua chi mo 22/80/443."
  fi
}

lay_ma_nguon() {
  mkdir -p "$THU_MUC_CAU_HINH" "$THU_MUC_SPOOL" "$THU_MUC_LOG"
  chmod 700 "$THU_MUC_CAU_HINH"

  if [ -d "$GOC_CHECKOUT/.git" ]; then
    ghi_log thong-tin "Da co ban checkout tai $GOC_CHECKOUT."
    return
  fi
  ghi_log thong-tin "Tai ma nguon nhanh $NHANH tu $KHO_GIT"
  # Sparse checkout: chi lay thu muc WebApp. BIN/, Source/, Release/ la ban desktop Windows,
  # vo dung tren may chu Linux va chiem hang tram MB nhi phan.
  git clone --depth 1 --filter=blob:none --sparse --branch "$NHANH" "$KHO_GIT" "$GOC_CHECKOUT"
  git -C "$GOC_CHECKOUT" sparse-checkout set WebApp
}

sinh_env() {
  local tep="$1"
  touch "$tep"; chmod 600 "$tep"

  dat_neu_chua_co "$tep" POSTGRES_DB            "qlgx"
  dat_neu_chua_co "$tep" POSTGRES_USER          "qlgx_chu"
  dat_neu_chua_co "$tep" POSTGRES_PASSWORD      "$(sinh_bi_mat 24)"
  # HAI vai tro RIENG BIET -- trung ten nghia la Row-Level Security bi vo hieu hoan toan, va
  # API se tu choi khoi dong o moi truong san xuat (xem KiemTraCauHinh.cs).
  dat_neu_chua_co "$tep" QLGX_APP_DB_USER       "qlgx_app"
  dat_neu_chua_co "$tep" QLGX_APP_DB_PASSWORD   "$(sinh_bi_mat 24)"
  dat_neu_chua_co "$tep" QLGX_ADMIN_DB_USER     "qlgx_admin"
  dat_neu_chua_co "$tep" QLGX_ADMIN_DB_PASSWORD "$(sinh_bi_mat 24)"
  dat_neu_chua_co "$tep" QLGX_JWT_KEY           "$(sinh_bi_mat 32)"
  dat_neu_chua_co "$tep" QLGX_API_PORT          "8080"
  dat_neu_chua_co "$tep" QLGX_THU_MUC_SPOOL     "$THU_MUC_SPOOL"
  dat_neu_chua_co "$tep" ASPNETCORE_ENVIRONMENT "Production"

  # dat_neu_chua_co, KHONG phai set_env_kv: chay lai script tren mot he thong dang song ma sinh
  # lai bi mat se dang xuat toan bo nguoi dung (JWT) va lam API mat ket noi vao chinh CSDL dang
  # chay (mat khau). Day la bat bien quan trong nhat cua toan bo script nay.
}

hoi_hoac_bien() {
  local ten_bien="$1" cau_hoi="$2" gia_tri="${!1:-}"
  if [ -n "$gia_tri" ]; then printf '%s' "$gia_tri"; return; fi
  # Che do khong tuong tac: bao loi RO RANG va thoat NGAY -- tuyet doi khong doc /dev/tty roi
  # treo may cho nguoi go, vi kich ban that su la mot phien SSH tu dong (cron, Ansible, CI)
  # khong co ai ngoi go ca.
  [ "$KHONG_TUONG_TAC" -eq 1 ] && bao_loi_va_thoat "Che do --non-interactive can bien $ten_bien."
  local tra_loi
  read -rp "$cau_hoi: " tra_loi </dev/tty
  printf '%s' "$tra_loi"
}

ghi_backup_env() {
  local tep="$1"
  local endpoint bucket khoa_id khoa_bi_mat
  endpoint=$(hoi_hoac_bien QLGX_R2_ENDPOINT "Dia chi endpoint R2 (vd https://<tai-khoan>.r2.cloudflarestorage.com)")
  bucket=$(hoi_hoac_bien QLGX_R2_BUCKET "Ten bucket R2")
  khoa_id=$(hoi_hoac_bien QLGX_R2_ACCESS_KEY_ID "R2 Access Key ID")
  khoa_bi_mat=$(hoi_hoac_bien QLGX_R2_SECRET_ACCESS_KEY "R2 Secret Access Key")

  touch "$tep"; chmod 600 "$tep"; chown root:root "$tep" 2>/dev/null || true

  set_env_kv "$tep" RESTIC_REPOSITORY        "s3:${endpoint%/}/$bucket"
  set_env_kv "$tep" AWS_ACCESS_KEY_ID        "$khoa_id"
  set_env_kv "$tep" AWS_SECRET_ACCESS_KEY    "$khoa_bi_mat"
  # Mat khau restic KHONG BAO GIO duoc sinh lai: doi no la moi ban sao luu cu tro thanh khong
  # doc duoc VINH VIEN. Day la ly do phai co the phuc hoi cat ngoai may chu.
  dat_neu_chua_co "$tep" RESTIC_PASSWORD     "$(sinh_bi_mat 32)"
  set_env_kv "$tep" QLGX_GIU_LAI             "--keep-last 8 --keep-daily 30 --keep-weekly 12 --keep-monthly 24"
  set_env_kv "$tep" QLGX_GOC_UNG_DUNG        "$GOC_UNG_DUNG"
  set_env_kv "$tep" QLGX_THU_MUC_SPOOL       "$THU_MUC_SPOOL"
  set_env_kv "$tep" QLGX_THU_MUC_LOG         "$THU_MUC_LOG"

  ghi_log thong-tin "Da ghi cau hinh sao luu vao $tep (chi root doc duoc)."
}

# Cho phep bo test nap file nay ma khong chay gi.
[ "${QLGX_CHI_NAP_HAM:-0}" = "1" ] && return 0
