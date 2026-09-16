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
# KHONG readonly: cho phep bo test tro GOC_CHECKOUT sang mot thu muc tam, tranh dung cham
# vao /opt/qlgx that tren may chay test. Mac dinh khi chay that van la /opt/qlgx.
GOC_CHECKOUT="${GOC_CHECKOUT:-/opt/qlgx}"
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
# Co thoat hiem DUY NHAT cho phep cai xong ma khong co ten mien (HTTP thuan). Phai go tuong
# minh -- mac dinh la CHAN. Xem chan_neu_thieu_ten_mien().
CHO_PHEP_HTTP="${QLGX_CHO_PHEP_HTTP_KHONG_AN_TOAN:-0}"

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

# I13 -- PHAI quet --branch= TRUOC nap_thu_vien. nap_thu_vien chay o dong duoi day, con
# phan_tich_tham_so (noi xu ly --branch=) chi chay trong main() o cuoi tep. Nghia la khi cai bang
# `curl ... | bash -s -- --branch=abc`, chung.sh/os_adapter.sh van duoc tai tu nhanh MAC DINH
# trong khi ma nguon sau do lai duoc clone tu nhanh abc -- tron hai phien ban thu vien va script.
# Te hon, hai cau bao loi cua nap_thu_vien con khuyen "kiem tra ten nhanh (--branch=...)", tuc la
# goi y mot co VO HIEU o dung thoi diem do. Cho phep ca bien moi truong QLGX_NHANH (giong
# qlgx-restore.sh) de cac duong goi tu dong khong phai chen tham so.
quet_nhanh_som() {
  local t
  NHANH="${QLGX_NHANH:-$NHANH}"
  for t in "$@"; do
    case "$t" in --branch=*) NHANH="${t#*=}" ;; esac
  done
}
quet_nhanh_som "$@"
nap_thu_vien

# C3 -- CO HOAN TAT RIENG, KHONG dung .env de phan biet "cai moi" voi "cap nhat".
#
# .env duoc ghi o BUOC DAU TIEN cua luong cai moi, truoc bat_postgres/tao_vai_tro_rls/bat_api/
# khoi_tao_giao_xu_va_admin/cau_hinh_https/cai_dat_systemd/in_the_phuc_hoi. Neu lan cai dau bi
# dut giua chung (mang chap chon lam `dc build api` that bai la du), thi lan chay lai -- viec duy
# nhat hop ly ma mot nguoi khong ranh may tinh se lam -- se thay .env da co, di vao nhanh cap_nhat,
# thay HEAD == origin/$NHANH, in "Da la ban moi nhat -- khong lam gi" va THOAT VOI MA 0. Ket qua:
# may chu NUA CAI (khong vai tro RLS, khong Caddy, khong timer sao luu, khong tai khoan quan tri)
# ma script BAO THANH CONG, va The phuc hoi KHONG BAO GIO duoc in -- mat may chu la mat vinh vien
# kha nang giai ma kho R2. Cac buoc cai moi von da idempotent (install-e2e.sh buoc 8b chung minh),
# nen chay lai tu dau la an toan VA la hanh vi dung.
TEP_HOAN_TAT="$GOC_UNG_DUNG/.cai-dat-hoan-tat"

# Di tru cho cac may DA cai xong TRUOC khi co co hoan tat: khong co tep co, nhung co du dau hieu
# cua mot lan cai da di toi cuoi (backup.env do ghi_backup_env tao, va CLI qlgx do cai_tep_systemd
# cai o gan cuoi). Khong co buoc nay, ban cap nhat nay se keo moi may dang chay vao lai toan bo
# luong cai moi (hoi lai ten giao xu, hoi lai khoa R2) -- dung thu khong duoc phep xay ra.
cai_dat_cu_da_hoan_tat() {
  [ -f "$GOC_UNG_DUNG/.env" ] \
    && [ -f "$THU_MUC_CAU_HINH/backup.env" ] \
    && [ -x /usr/local/bin/qlgx ]
}
la_cai_moi() {
  [ ! -f "$TEP_HOAN_TAT" ] || return 1
  ! cai_dat_cu_da_hoan_tat
}
danh_dau_cai_xong() { date -Iseconds > "$TEP_HOAN_TAT" 2>/dev/null || true; }

phan_tich_tham_so() {
  for t in "$@"; do
    case "$t" in
      --non-interactive) KHONG_TUONG_TAC=1 ;;
      --dry-run)         CHAY_THU=1 ;;
      --status)          CHI_TRANG_THAI=1 ;;
      --skip-backup)     BO_QUA_SAO_LUU=1 ;;
      --domain=*)        TEN_MIEN="${t#*=}" ;;
      --cho-phep-http-khong-an-toan) CHO_PHEP_HTTP=1 ;;
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
  # N10: /opt co the chua ton tai tren mot may hoan toan trang. `df` tra ma khac 0, va duoi
  # `set -euo pipefail` phep gan nay se giet ca script MA KHONG in mot thong bao tieng Viet nao --
  # pha vo quy uoc "moi loi deu qua bao_loi_va_thoat" ma lay_ma_nguon da ky cong giu. Do phan
  # vung CHA gan nhat dang ton tai thay vi de no chet.
  local thu_muc_do="/opt"
  [ -d "$thu_muc_do" ] || thu_muc_do="/"
  dia_gb=$(df -BG --output=avail "$thu_muc_do" 2>/dev/null | tail -1 | tr -dc '0-9') || dia_gb=""
  [ -n "$dia_gb" ] \
    || bao_loi_va_thoat "Khong do duoc dung luong dia trong tai '$thu_muc_do'. Kiem tra lenh 'df'."

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
  # I2: qlgx-runner.sh's loc_snapshot_json can python3. Thieu no thi buoc cap nhat danh sach ban
  # sao that bai va man hinh "Sao luu & Phuc hoi" khong liet ke duoc ban sao nao du kho R2 day
  # ap. Co san tren Ubuntu/Debian mac dinh, NHUNG khong chac tren Rocky/Alma toi gian.
  os_co_lenh python3 || can+=(python3)
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

# Kho git duoc phat trien tren Windows, noi he thong tep KHONG luu duoc bit thuc thi -- git ghi
# nhan moi tep script la 100644. Sau `git clone` tren Linux, scripts/*.sh va scripts/qlgx deu
# KHONG thuc thi duoc, nen moi cho goi thang "$GOC_UNG_DUNG/scripts/qlgx-runner.sh" (install.sh,
# qlgx-restore.sh, va ExecStart= cua unit systemd) se bao "Permission denied". Dat lai bit thuc
# thi sau MOI lan lay/cap nhat ma nguon.
dat_bit_thuc_thi() {
  chmod +x "$GOC_UNG_DUNG"/scripts/*.sh "$GOC_UNG_DUNG/scripts/qlgx" 2>/dev/null || true
}

lay_ma_nguon() {
  mkdir -p "$THU_MUC_CAU_HINH" "$THU_MUC_SPOOL" "$THU_MUC_LOG"
  chmod 700 "$THU_MUC_CAU_HINH"
  # N8: nhat ky chua so lieu giao dan va thong diep loi CSDL -- khong de umask he thong (thuong
  # cho ra 755) quyet dinh ai doc duoc.
  chmod 750 "$THU_MUC_LOG" 2>/dev/null || true

  if [ -d "$GOC_CHECKOUT/.git" ]; then
    ghi_log thong-tin "Da co ban checkout tai $GOC_CHECKOUT."
    dat_bit_thuc_thi
    return
  fi

  # Thu muc ton tai, khong rong, nhung khong phai git repo: rat co the la rac cua mot lan
  # chay truoc bi ngat giua chung (mat dien, Ctrl-C, timeout) -- ngay sau khi thu muc duoc
  # tao nhung truoc khi `git clone` kip hoan tat. Neu cu de troi qua, `git clone` ben duoi se
  # tu bao loi tieng Anh goc cua git roi dung qua set -e, pha vo quy uoc "moi loi deu qua
  # bao_loi_va_thoat voi thong bao tieng Viet" cua toan bo script. KHONG tu y xoa -- thu muc
  # la co the chua thu gi do khong lien quan quan trong, de nguoi van hanh tu quyet dinh.
  if [ -e "$GOC_CHECKOUT" ] && [ -n "$(ls -A "$GOC_CHECKOUT" 2>/dev/null)" ]; then
    bao_loi_va_thoat "Thu muc $GOC_CHECKOUT da ton tai nhung khong phai ban checkout git hop le (co the do lan chay truoc bi ngat giua chung). Xoa thu muc nay roi chay lai: rm -rf $GOC_CHECKOUT"
  fi

  ghi_log thong-tin "Tai ma nguon nhanh $NHANH tu $KHO_GIT"
  # Sparse checkout: chi lay thu muc WebApp. BIN/, Source/, Release/ la ban desktop Windows,
  # vo dung tren may chu Linux va chiem hang tram MB nhi phan.
  git clone --depth 1 --filter=blob:none --sparse --branch "$NHANH" "$KHO_GIT" "$GOC_CHECKOUT"
  git -C "$GOC_CHECKOUT" sparse-checkout set WebApp
  dat_bit_thuc_thi
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

# hoi_hoac_bien <TEN_BIEN> <cau hoi> [ro|an] [bat_buoc|tuy_chon]
#   ro|an       : N5 -- "an" dung `read -rsp` de gia tri KHONG hien tren man hinh. Bat buoc cho
#                 R2 Secret Access Key, mat khau quan tri va mat khau restic: mot phien SSH luu
#                 scrollback, va nguoi van hanh thuong chia se man hinh khi nho nguoi khac giup.
#   bat_buoc|tuy_chon : "tuy_chon" cho phep de trong (va o che do --non-interactive thi tra ve
#                 chuoi rong thay vi bao loi) -- dung cho cau hoi "co phai dang cuu ho khong".
hoi_hoac_bien() {
  local ten_bien="$1" cau_hoi="$2" che_do="${3:-ro}" bat_buoc="${4:-bat_buoc}"
  local gia_tri="${!1:-}"
  if [ -n "$gia_tri" ]; then printf '%s' "$gia_tri"; return; fi
  # Che do khong tuong tac: bao loi RO RANG va thoat NGAY -- tuyet doi khong doc /dev/tty roi
  # treo may cho nguoi go, vi kich ban that su la mot phien SSH tu dong (cron, Ansible, CI)
  # khong co ai ngoi go ca.
  if [ "$KHONG_TUONG_TAC" -eq 1 ]; then
    [ "$bat_buoc" = "tuy_chon" ] && { printf ''; return; }
    bao_loi_va_thoat "Che do --non-interactive can bien $ten_bien."
  fi
  local tra_loi
  if [ "$che_do" = "an" ]; then
    # In loi nhac ra stderr (read -rsp cung in ra stderr) va tu xuong dong: `read -rs` khong in
    # ky tu nao ke ca Enter, khong co dong nay thi dong tiep theo dinh lien vao cau hoi.
    read -rsp "$cau_hoi: " tra_loi </dev/tty
    printf '\n' >&2
  else
    read -rp "$cau_hoi: " tra_loi </dev/tty
  fi
  printf '%s' "$tra_loi"
}

ghi_backup_env() {
  local tep="$1"
  local endpoint bucket khoa_id khoa_bi_mat
  endpoint=$(hoi_hoac_bien QLGX_R2_ENDPOINT "Dia chi endpoint R2 (vd https://<tai-khoan>.r2.cloudflarestorage.com)")
  bucket=$(hoi_hoac_bien QLGX_R2_BUCKET "Ten bucket R2")
  khoa_id=$(hoi_hoac_bien QLGX_R2_ACCESS_KEY_ID "R2 Access Key ID")
  khoa_bi_mat=$(hoi_hoac_bien QLGX_R2_SECRET_ACCESS_KEY "R2 Secret Access Key" an)

  touch "$tep"; chmod 600 "$tep"; chown root:root "$tep" 2>/dev/null || true

  set_env_kv "$tep" RESTIC_REPOSITORY        "s3:${endpoint%/}/$bucket"
  set_env_kv "$tep" AWS_ACCESS_KEY_ID        "$khoa_id"
  set_env_kv "$tep" AWS_SECRET_ACCESS_KEY    "$khoa_bi_mat"

  # MAT KHAU RESTIC -- KHONG BAO GIO duoc sinh lai khi da co: doi no la moi ban sao luu cu tro
  # thanh khong doc duoc VINH VIEN. Day la ly do phai co The phuc hoi cat ngoai may chu.
  #
  # KICH BAN CUU HO (bat buoc phai hoi): may chu giao xu chay, nguoi van hanh cai lai tren mot
  # VPS TRANG va -- theo dung huong dan tren The phuc hoi -- nhap lai KHO R2 CU. Neu ta cu sinh
  # mot mat khau MOI ngau nhien o day thi mat khau do KHONG mo duoc kho cu: buoc "sao luu dau
  # tien" ngay sau do that bai, `set -e` giet script TRUOC in_the_phuc_hoi, va nguoi van hanh
  # khong bao gio nhan duoc the. Te hon, backup.env tren may moi giu mat khau SAI vinh vien nen
  # moi lan sao luu tu dong ve sau deu hong ma khong ai noi cho ho biet.
  if [ -z "$(doc_env_kv "$tep" RESTIC_PASSWORD)" ]; then
    local mk_restic
    mk_restic=$(hoi_hoac_bien QLGX_RESTIC_PASSWORD \
      "May nay dang phuc hoi tu mot kho sao luu DA CO? Nhap MAT KHAU RESTIC ghi tren The phuc hoi (Enter de tao kho moi)" \
      an tuy_chon)
    if [ -n "$mk_restic" ]; then
      ghi_log thong-tin "Dung MAT KHAU RESTIC do nguoi van hanh nhap (kho sao luu da co san)."
    else
      mk_restic=$(sinh_bi_mat 32)
      ghi_log thong-tin "Tao MAT KHAU RESTIC moi cho mot kho sao luu moi."
    fi
    set_env_kv "$tep" RESTIC_PASSWORD "$mk_restic"
  fi
  set_env_kv "$tep" QLGX_GIU_LAI             "--keep-last 8 --keep-daily 30 --keep-weekly 12 --keep-monthly 24"
  set_env_kv "$tep" QLGX_GOC_UNG_DUNG        "$GOC_UNG_DUNG"
  set_env_kv "$tep" QLGX_THU_MUC_SPOOL       "$THU_MUC_SPOOL"
  set_env_kv "$tep" QLGX_THU_MUC_LOG         "$THU_MUC_LOG"

  ghi_log thong-tin "Da ghi cau hinh sao luu vao $tep (chi root doc duoc)."
}

# Boc docker compose: luon dung dung thu muc va dung hai tep overlay.
dc() { docker compose --project-directory "$GOC_UNG_DUNG" \
         -f "$GOC_UNG_DUNG/docker-compose.yml" \
         -f "$GOC_UNG_DUNG/docker-compose.prod.yml" "$@"; }

bat_postgres() {
  ghi_log thong-tin "Khoi dong PostgreSQL va cho san sang"
  dc up -d postgres
  local i
  for i in $(seq 1 60); do
    dc exec -T postgres pg_isready -q && return 0
    sleep 2
  done
  bao_loi_va_thoat "PostgreSQL khong san sang sau 120 giay. Xem: dc logs postgres"
}

tao_vai_tro_rls() {
  ghi_log thong-tin "Tao/cap nhat hai vai tro RLS"
  local db user
  db=$(doc_env_kv "$GOC_UNG_DUNG/.env" POSTGRES_DB)
  user=$(doc_env_kv "$GOC_UNG_DUNG/.env" POSTGRES_USER)
  # KHONG dung /docker-entrypoint-initdb.d: thu muc do chi chay khi volume du lieu con TRONG,
  # nen se bi bo qua o moi lan chay lai -- dung luc ta can tinh idempotent nhat.
  dc exec -T postgres psql -v ON_ERROR_STOP=1 -U "$user" -d "$db" \
    -v qlgx_app_user="$(doc_env_kv "$GOC_UNG_DUNG/.env" QLGX_APP_DB_USER)" \
    -v qlgx_app_password="$(doc_env_kv "$GOC_UNG_DUNG/.env" QLGX_APP_DB_PASSWORD)" \
    -v qlgx_admin_user="$(doc_env_kv "$GOC_UNG_DUNG/.env" QLGX_ADMIN_DB_USER)" \
    -v qlgx_admin_password="$(doc_env_kv "$GOC_UNG_DUNG/.env" QLGX_ADMIN_DB_PASSWORD)" \
    < "$GOC_UNG_DUNG/scripts/sql/00-vai-tro-rls.sql"
}

# I2 (review backend) -- siet quyen ba bang sao luu. PHAI goi SAU khi migration da tao bang (tuc
# sau bat_api), va goi lai o moi lan cap nhat de cac may da cai tu truoc cung duoc siet.
# Bang do EF Core migration tao ra, ma migration chay bang qlgx_app -> ba bang mac nhien thuoc
# vai tro it tin cay nhat, trong khi cong_viec_sao_luu la HANG DOI MA LENH ma qlgx-runner.sh thi
# hanh duoi quyen ROOT. Xem giai thich day du trong sql/01-bang-sao-luu-quyen.sql.
dat_quyen_bang_sao_luu() {
  local db user tep_sql="$GOC_UNG_DUNG/scripts/sql/01-bang-sao-luu-quyen.sql"
  [ -f "$tep_sql" ] || { ghi_log canh-bao "Khong tim thay $tep_sql -- bo qua buoc siet quyen."; return 0; }
  db=$(doc_env_kv "$GOC_UNG_DUNG/.env" POSTGRES_DB)
  user=$(doc_env_kv "$GOC_UNG_DUNG/.env" POSTGRES_USER)
  ghi_log thong-tin "Siet quyen ba bang sao luu (chu so huu = vai tro quan tri)"
  # Truyen ten vai tro qua set_config: mot khoi DO$$ khong doc duoc bien psql (-v), va noi suy
  # thang vao khoi $$ la mo duong chen SQL -- cung cach 00-vai-tro-rls.sql da dung.
  local vt_app vt_admin
  vt_app=$(doc_env_kv "$GOC_UNG_DUNG/.env" QLGX_APP_DB_USER | sed "s/'/''/g")
  vt_admin=$(doc_env_kv "$GOC_UNG_DUNG/.env" QLGX_ADMIN_DB_USER | sed "s/'/''/g")
  {
    printf "SELECT set_config('qlgx.app_user', '%s', false);\n" "$vt_app"
    printf "SELECT set_config('qlgx.admin_user', '%s', false);\n" "$vt_admin"
    cat "$tep_sql"
  } | dc exec -T postgres psql -v ON_ERROR_STOP=1 -U "$user" -d "$db" \
    || ghi_log canh-bao "Khong siet duoc quyen ba bang sao luu (xem loi ngay tren). He thong van" \
                        "chay duoc, nhung vai tro ung dung dang ghi duoc vao hang doi cong viec."
}

# Gioi han la SO GIAY THAT (han chot theo dong ho), KHONG phai so vong lap -- xem cho_api_san_sang
# trong qlgx-restore.sh (Task 14) de biet ly do: dem theo vong lap thi moi lan `dc exec` vao mot
# container dang crash-restart mat nhieu giay ngoai du kien, "180" hoa ra dai hon that nhieu.
cho_san_sang() {
  local gioi_han="${1:-180}" het_han
  het_han=$(( $(date +%s) + gioi_han ))
  while [ "$(date +%s)" -lt "$het_han" ]; do
    if dc exec -T api curl -fsS http://localhost:8080/api/suc-khoe/san-sang >/dev/null 2>&1; then
      return 0
    fi
    sleep 1
  done
  return 1
}

bat_api() {
  ghi_log thong-tin "Dung image va khoi dong API (migration tu chay luc khoi dong)"
  dc build api
  dc up -d api
  cho_san_sang 300 || bao_loi_va_thoat "API khong san sang. Xem: docker compose logs api"
  ghi_log thong-tin "API da san sang."
}

khoi_tao_giao_xu_va_admin() {
  local ten_giao_xu ten_tk mat_khau ho_ten
  ten_giao_xu=$(hoi_hoac_bien QLGX_GIAO_XU_TEN "Ten giao xu")
  ten_tk=$(hoi_hoac_bien QLGX_ADMIN_TEN_TAI_KHOAN "Ten dang nhap quan tri he thong")
  mat_khau=$(hoi_hoac_bien QLGX_ADMIN_MAT_KHAU "Mat khau (toi thieu 8 ky tu)" an)
  ho_ten=$(hoi_hoac_bien QLGX_ADMIN_HO_TEN "Ho ten hien thi")

  # I7: `>/tmp/qlgx-tao-admin.log` la mot TEN CO DINH trong thu muc ai cung ghi duoc, duoc root
  # ghi de bang `>`. Mot nguoi dung cuc bo dat san symlink tro toi /etc/shadow (hay
  # /etc/qlgx/backup.env) thi lan cai ke tiep root se cat trang va ghi de dung tep do (CWE-377).
  # `mktemp` sinh ten khong doan truoc va tu choi neu da ton tai.
  local nhat_ky_tao; nhat_ky_tao=$(mktemp "${TMPDIR:-/tmp}/qlgx-tao-admin.XXXXXX")
  chmod 600 "$nhat_ky_tao"
  # shellcheck disable=SC2064
  # Tu go trap ngay trong than no: `trap ... RETURN` cua bash KHONG tu het hieu luc sau lan tra
  # ve dau tien -- no o lai va chay LAI o moi lan tra ve cua bat ky ham nao khac (bai hoc da ghi
  # o lenh_dong_bo_danh_sach trong qlgx-runner.sh).
  trap "rm -f '$nhat_ky_tao'; trap - RETURN" RETURN

  # Goi CLI de THU tao. KHONG dua vao ma thoat hay so khop chuoi trong log cua no de quyet
  # dinh idempotent -- thong bao "da ton tai" trong TaoTaiKhoanQuanTri.cs hien khong dau, nhung
  # phan con lai cua ma nguon (TaiKhoanService.cs, QuanLyGiaoXuEndpoints.cs) dung "đã tồn tại"
  # CO DAU cho cung y nghia; chi mot lan ai do thong nhat lai chinh ta la grep nay vo lang lang
  # (dung lop loi Task 6/7 da tung mac: phan biet trang thai bang chuoi hien thi thay vi kiem
  # trang thai THAT). Vi vay: chay CLI, RIENG BIET tu hoi lai CSDL xem tai khoan co that hay
  # khong -- do moi la nguon su that duy nhat.
  dc exec -T \
      -e QLGX_ADMIN_GIAO_XU_TEN="$ten_giao_xu" \
      -e QLGX_ADMIN_TAO_GIAO_XU_NEU_CHUA_CO=true \
      -e QLGX_ADMIN_TEN_TAI_KHOAN="$ten_tk" \
      -e QLGX_ADMIN_MAT_KHAU="$mat_khau" \
      -e QLGX_ADMIN_HO_TEN="$ho_ten" \
      -e QLGX_ADMIN_LOAI_TAI_KHOAN=9 \
      api dotnet Qlgx.Api.dll tao-tai-khoan-quan-tri >"$nhat_ky_tao" 2>&1 || true
  cat "$nhat_ky_tao"

  local db user ten_giao_xu_sql ten_tk_sql da_co
  db=$(doc_env_kv "$GOC_UNG_DUNG/.env" POSTGRES_DB)
  user=$(doc_env_kv "$GOC_UNG_DUNG/.env" POSTGRES_USER)
  # Thoat dau nhay don kieu SQL (nhan doi) truoc khi ghep truc tiep vao cau lenh -- ten giao xu/
  # tai khoan la du lieu nguoi van hanh nhap, co the chua dau nhay don.
  ten_giao_xu_sql=$(printf '%s' "$ten_giao_xu" | sed "s/'/''/g")
  ten_tk_sql=$(printf '%s' "$ten_tk" | sed "s/'/''/g")
  da_co=$(dc exec -T postgres psql -tAX -U "$user" -d "$db" -c \
    "SELECT count(*) FROM tai_khoan t JOIN giao_xu g ON g.id = t.giao_xu_id
     WHERE g.ten_giao_xu = '$ten_giao_xu_sql' AND t.ten_tai_khoan = '$ten_tk_sql'
       AND NOT t.da_xoa;" 2>/dev/null | tr -d ' \r')

  if [ "${da_co:-0}" -ge 1 ] 2>/dev/null; then
    ghi_log thong-tin "Tai khoan '$ten_tk' da san sang trong CSDL (moi tao hoac da co san tu truoc)."
  else
    bao_loi_va_thoat "Khong tao duoc tai khoan quan tri (CSDL xac nhan tai khoan KHONG ton tai). Xem nhat ky in ngay ben tren."
  fi
}

# Ten mien co dung dang khong. Chan som cac gia tri se sinh ra mot Caddyfile hong am tham:
# "https://gx.example" (co scheme), "gx.example/" (co dau /), chuoi co dau cach, hay ten khong
# co dau cham (khong xin duoc chung chi Let's Encrypt).
ten_mien_hop_le() {
  local tm="$1"
  [[ "$tm" =~ ^[A-Za-z0-9]([A-Za-z0-9-]*[A-Za-z0-9])?(\.[A-Za-z0-9]([A-Za-z0-9-]*[A-Za-z0-9])?)+$ ]]
}

# C-4 -- CHAN cai dat khi khong co ten mien.
#
# Truoc day script chi CANH BAO roi van cai xong: giao xu "cai thu cho nhanh", bo qua buoc ten
# mien, roi nhap so sach that vao. Tu do toan bo ho ten, ngay sinh, so can cuoc cua hang nghin
# giao dan VA moi mat khau dang nhap di qua internet o dang doc duoc -- vinh vien, vi nguoi van
# hanh khong ranh may tinh se khong bao gio "cau hinh lai HTTPS thu cong". Mot canh bao ma khong
# ai doc thi khong phai mot lop bao ve.
#
# Duong thoat cho nguoi cai thu cuc bo: --cho-phep-http-khong-an-toan (hoac bien moi truong
# QLGX_CHO_PHEP_HTTP_KHONG_AN_TOAN=1). Phai go tuong minh, khong con la mac dinh.
chan_neu_thieu_ten_mien() {
  if [ -z "$TEN_MIEN" ] && [ "$KHONG_TUONG_TAC" -eq 0 ] && [ "$CHO_PHEP_HTTP" -eq 0 ]; then
    ghi_log thong-tin "Ten mien la BAT BUOC de bat HTTPS (chung chi Let's Encrypt tu dong)." \
                      "Vi du: giaoxu-abc.net. Ten mien do PHAI dang tro ve dia chi IP cua may nay."
    TEN_MIEN=$(hoi_hoac_bien QLGX_TEN_MIEN "Ten mien" ro tuy_chon)
  fi

  if [ -n "$TEN_MIEN" ]; then
    ten_mien_hop_le "$TEN_MIEN" || bao_loi_va_thoat \
      "Ten mien '$TEN_MIEN' khong hop le. Chi go ten mien tran, vi du: giaoxu-abc.net" \
      "(khong co 'https://', khong co dau '/' o cuoi, khong co dau cach)."
    return 0
  fi

  if [ "$CHO_PHEP_HTTP" -eq 1 ]; then
    ghi_log canh-bao "CHAY HTTP THUAN THEO YEU CAU (--cho-phep-http-khong-an-toan)." \
                     "Duong truyen KHONG duoc ma hoa: ho ten, ngay sinh, so can cuoc cua giao dan" \
                     "va mat khau dang nhap di qua mang o dang doc duoc. CHI dung de cai thu tren" \
                     "may cuc bo. TUYET DOI KHONG nhap so sach that vao may chay o che do nay."
    return 0
  fi

  bao_loi_va_thoat \
    "DUNG CAI DAT: chua co ten mien nen he thong se phai chay qua HTTP THUAN (khong ma hoa)." \
    "" \
    "Vi sao chan: khong co HTTPS thi ho ten, ngay sinh, so can cuoc cua giao dan va ca mat khau" \
    "dang nhap di qua internet o dang ai cung doc duoc. Day la so sach giao xu, mat la mat that." \
    "" \
    "Cach xu ly:" \
    "  1. Tro mot ten mien (vd giaoxu-abc.net) ve dia chi IP cua may chu nay, roi chay lai:" \
    "       sudo bash install.sh --domain=giaoxu-abc.net" \
    "     Chay lai voi --domain=... tren may DA cai cung co tac dung: Caddyfile se duoc sinh lai" \
    "     va bat HTTPS." \
    "  2. Chi khi cai thu tren may cuc bo (khong co du lieu that), them:" \
    "       --cho-phep-http-khong-an-toan"
}

# Sinh Caddyfile theo trang thai TEN_MIEN hien tai. Tach rieng de nhanh cai moi va nhanh cap nhat
# (--domain= tren may da cai) dung CHUNG mot ban sinh, khong bao gio lech nhau.
ghi_caddyfile() {
  if [ -z "$TEN_MIEN" ]; then
    cat > "$GOC_UNG_DUNG/Caddyfile" <<'EOF'
:80 {
	reverse_proxy api:8080
}
EOF
  else
    cat > "$GOC_UNG_DUNG/Caddyfile" <<EOF
$TEN_MIEN {
	reverse_proxy api:8080

	header {
		# HSTS: mot khi trinh duyet da vao bang HTTPS thi khong bao gio thu HTTP nua.
		Strict-Transport-Security "max-age=31536000; includeSubDomains"
		X-Content-Type-Options "nosniff"
		Referrer-Policy "strict-origin-when-cross-origin"
		X-Frame-Options "DENY"
		-Server
	}

	# Anh dai dien tai len toi da 8 MB (xem XuLyAnh.cs) -- chan som o day de yeu cau qua lon
	# khong di toi tan ung dung.
	request_body { max_size 10MB }

	encode gzip zstd
}
EOF
  fi
}

cau_hinh_https() {
  chan_neu_thieu_ten_mien
  ghi_caddyfile
  dc up -d caddy
  ghi_log thong-tin "Caddy da chay${TEN_MIEN:+ cho $TEN_MIEN (HTTPS tu dong)}."
}

# C-4, phan hai: lam cho `--domain=` CO TAC DUNG THAT tren may DA cai (nhanh cap nhat).
# Truoc day nhanh nay khong bao gio sinh lai Caddyfile, nen mot may lo cai o che do HTTP thuan
# thi khong con duong nao quay lai HTTPS ngoai sua tay -- tuc la khong bao gio.
cap_nhat_https_neu_co_ten_mien() {
  [ -n "$TEN_MIEN" ] || return 0
  ten_mien_hop_le "$TEN_MIEN" || bao_loi_va_thoat \
    "Ten mien '$TEN_MIEN' khong hop le. Chi go ten mien tran, vi du: giaoxu-abc.net."

  if grep -qx "$TEN_MIEN {" "$GOC_UNG_DUNG/Caddyfile" 2>/dev/null; then
    ghi_log thong-tin "Caddyfile da dung cho $TEN_MIEN -- khong sinh lai."
    return 0
  fi

  ghi_log thong-tin "Cau hinh lai HTTPS cho $TEN_MIEN (--domain)."
  ghi_caddyfile
  # Doi noi dung Caddyfile khong tu co hieu luc: phai tao lai container caddy.
  dc up -d --force-recreate caddy \
    || ghi_log loi "Khong khoi dong lai duoc caddy sau khi doi ten mien -- kiem tra: docker compose logs caddy"
  ghi_log thong-tin "Da bat HTTPS cho $TEN_MIEN. Ten mien nay PHAI dang tro ve IP may nay thi" \
                    "Let's Encrypt moi cap duoc chung chi."
}

in_the_phuc_hoi() {
  local tep="$THU_MUC_CAU_HINH/the-phuc-hoi.txt"
  local be="$THU_MUC_CAU_HINH/backup.env"
  local env="$GOC_UNG_DUNG/.env"
  # I9 -- TUYET DOI khong noi URL tai tep tu $KHO_GIT. $KHO_GIT ket thuc bang ".git" (dung cho
  # `git clone`) nen "$KHO_GIT/raw/$NHANH/..." sinh ra dia chi co ".git" nam GIUA duong dan. Da
  # kiem chung bang curl that: dang co ".git" tra ve 404, dang raw.githubusercontent.com tra ve
  # 200. Day la to giay DUY NHAT con lai khi may chu chay, va nguoi cam no dang hoang -- ho go
  # dung tung chu va nhan 404 thi khong co cach nao doan ra dia chi dung. Dung CHINH chuoi ma
  # nap_thu_vien o dau tep dang dung, de hai noi khong bao gio lech nhau nua.
  local goc_raw="https://raw.githubusercontent.com/khoannd/qlgx/$NHANH/WebApp/scripts"
  cat > "$tep" <<EOF
================== THE PHUC HOI QLGX ==================
May chu   : $(hostname)  ${TEN_MIEN:+($TEN_MIEN)}
Lap ngay  : $(date '+%d/%m/%Y %H:%M')

MAT KHAU RESTIC (khong co dong nay thi KHONG AI phuc hoi duoc,
ke ca Cloudflare -- day la ban chat cua ma hoa phia may chu):
  $(doc_env_kv "$be" RESTIC_PASSWORD)

KHO SAO LUU : $(doc_env_kv "$be" RESTIC_REPOSITORY)
R2 KEY ID   : $(doc_env_kv "$be" AWS_ACCESS_KEY_ID)
R2 SECRET   : $(doc_env_kv "$be" AWS_SECRET_ACCESS_KEY)

MAT KHAU CSDL:
  postgres   : $(doc_env_kv "$env" POSTGRES_USER) / $(doc_env_kv "$env" POSTGRES_PASSWORD)
  qlgx_app   : $(doc_env_kv "$env" QLGX_APP_DB_USER) / $(doc_env_kv "$env" QLGX_APP_DB_PASSWORD)
  qlgx_admin : $(doc_env_kv "$env" QLGX_ADMIN_DB_USER) / $(doc_env_kv "$env" QLGX_ADMIN_DB_PASSWORD)

PHUC HOI TU MAY TRANG:
  1. Dung mot may chu Linux moi (Ubuntu 24.04)
  2. Cai lai bo ung dung (tu cai Docker luon, chua co du lieu giao xu nao):
       curl -fsSL $goc_raw/install.sh | sudo bash
     Khi duoc hoi kho R2, nhap dung KHO SAO LUU / R2 KEY ID / R2 SECRET o tren,
     VA nhap dung MAT KHAU RESTIC o tren khi duoc hoi (rat quan trong: nhap sai
     thi may moi se khong doc duoc kho sao luu cu).
  3. curl -fsSL $goc_raw/qlgx-restore.sh -o qlgx-restore.sh
  4. sudo bash qlgx-restore.sh --card the-phuc-hoi.txt          (in ra ke hoach)
  5. sudo bash qlgx-restore.sh --card the-phuc-hoi.txt --apply  (thuc hien)
=======================================================
EOF
  chmod 600 "$tep"
  cat "$tep"
  ghi_log canh-bao "IN THE TREN RA GIAY hoac chep vao noi an toan NGOAI may chu nay, roi xoa: " \
                   "rm $tep -- de tren chinh may chu thi mat may la mat luon kha nang phuc hoi."
}

# C4 -- HAI NHOM MUC KIEM CHUNG, CHI NHOM COT LOI DUOC PHEP KICH HOAT QUAY LUI.
#
# Ban truoc gop chung DIEU KIEN CHUC NANG ("API san sang", "RLS dang bat") voi NHAC NHO QUY TRINH
# ("The phuc hoi da duoc cat ngoai may chu") va DIEU KIEN PHU THUOC MANG ("kho restic mo duoc"),
# roi cap_nhat() dung ket qua GOP do de quyet dinh quay lui. Kich ban gan nhu chac chan xay ra:
# cha xu in The phuc hoi ra giay nhung -- rat tu nhien -- khong xoa /etc/qlgx/the-phuc-hoi.txt.
# Sang ngay thu 8, dong do chuyen KHONG DAT VINH VIEN. Tu do MOI lan cap nhat deu: build thanh
# cong, ban moi chay tot, roi tu_kiem_chung tra 1 -> quay lui (ngung dich vu vai phut VO CO) ->
# ghi mot commit HOAN TOAN LANH LAN vao .phien-ban-hong -> tu do tu choi MOI lan cap nhat toi
# commit do. Giao xu bi KHOA CUNG o ban cu, khong bao gio nhan duoc ban va loi nao nua.
#
# Gio: `tu_kiem_chung` (day du) van in du bang va dung cho `--status`; `tu_kiem_chung_cot_loi` chi
# chay cac muc THAT SU noi len "ban moi nay chay duoc hay khong" -- va chi no moi duoc quyen lam
# cap_nhat quay lui. Cac muc con lai van duoc in ra de nguoi van hanh thay, nhung khong bao gio
# tu lam ngung dich vu.
tu_kiem_chung()         { _tu_kiem_chung day-du; }
tu_kiem_chung_cot_loi() { _tu_kiem_chung cot-loi; }

_tu_kiem_chung() {
  local che_do="${1:-day-du}"
  local so_loi=0
  local env="$GOC_UNG_DUNG/.env"
  bao() { # bao <ten> <lenh...> -- muc COT LOI: KHONG DAT o day la ly do chinh dang de quay lui.
    local ten="$1"; shift
    if "$@" >/dev/null 2>&1; then printf '  DAT           %s\n' "$ten"
    else printf '  KHONG DAT     %s\n' "$ten"; so_loi=$((so_loi + 1)); fi
  }
  # Muc BO SUNG: van in ra (che do day du) nhung KHONG BAO GIO duoc dung de quyet dinh quay lui.
  bao_phu() {
    [ "$che_do" = "cot-loi" ] && return 0
    local ten="$1"; shift
    if "$@" >/dev/null 2>&1; then printf '  DAT           %s\n' "$ten"
    else printf '  CAN XEM LAI   %s\n' "$ten"; so_phu=$((so_phu + 1)); fi
  }
  local so_phu=0
  local db user app_user admin_user
  db=$(doc_env_kv "$env" POSTGRES_DB);       user=$(doc_env_kv "$env" POSTGRES_USER)
  app_user=$(doc_env_kv "$env" QLGX_APP_DB_USER)
  admin_user=$(doc_env_kv "$env" QLGX_ADMIN_DB_USER)
  # KHONG dinh nghia ham roi goi qua `bash -c`: subshell moi KHONG ke thua ham cua shell cha,
  # moi kiem tra se im lang that bai. Moi muc duoi day tu goi thang mot lenh.
  psql_hoi() { dc exec -T postgres psql -tAX -U "$user" -d "$db" -c "$1" | tr -d ' \r'; }

  bang_bang() { [ "$(psql_hoi "$1")" = "$2" ]; }
  it_nhat()   { [ "$(psql_hoi "$1")" -ge "$2" ] 2>/dev/null; }
  # QUAN TRONG (dung `set -o pipefail` nhu ca script): "! lenh | grep" la BAY fail-open -- neu
  # `dc exec` that bai (container khong chay), pipeline tra khac 0, dau "!" DAO thanh 0 -> in
  # DAT du chang kiem duoc gi. Vi vay tach RIENG hai dieu kien: (1) doc duoc bien moi truong cua
  # container api PHAI thanh cong, (2) trong do KHONG duoc co AWS_SECRET_ACCESS_KEY -- ca hai
  # deu phai dung thi moi DAT.
  api_khong_lo_khoa_r2() {
    # I7: tep nay chua TOAN BO bien moi truong cua container API -- QLGX_JWT_KEY,
    # ConnectionStrings__Qlgx (co mat khau CSDL), POSTGRES_PASSWORD. Ban truoc dung
    # "/tmp/qlgx-env-api.$$" (ten doan duoc) va de `umask` he thong quyet dinh quyen (thuong 644,
    # moi nguoi dung tren may doc duoc) trong suot thoi gian chay `grep`. Tro treu thay, day
    # chinh la muc kiem tra "Container API KHONG biet khoa R2". mktemp + chmod 600 ngay.
    local tep; tep=$(mktemp "${TMPDIR:-/tmp}/qlgx-env-api.XXXXXX") || return 1
    chmod 600 "$tep"
    if ! dc exec -T api env > "$tep" 2>/dev/null; then rm -f "$tep"; return 1; fi
    if grep -q '^AWS_SECRET_ACCESS_KEY=' "$tep"; then rm -f "$tep"; return 1; fi
    rm -f "$tep"
  }

  echo "--- Bang tu kiem chung ---"
  # ===== NHOM COT LOI: "ban dang chay co lam vic duoc khong" =====
  bao "Container postgres dang chay"  eval 'dc ps --status running postgres | grep -q postgres'
  bao "Container api dang chay"       eval 'dc ps --status running api | grep -q api'
  bao "API tra ve san sang"           dc exec -T api curl -fsS http://localhost:8080/api/suc-khoe/san-sang
  bao "Vai tro nghiep vu KHONG co BYPASSRLS" \
      bang_bang "SELECT rolbypassrls FROM pg_roles WHERE rolname='$app_user'" "f"
  bao "Vai tro quan tri CO BYPASSRLS" \
      bang_bang "SELECT rolbypassrls FROM pg_roles WHERE rolname='$admin_user'" "t"
  bao "RLS dang bat tren cac bang nghiep vu" \
      it_nhat "SELECT count(*) FROM pg_class WHERE relrowsecurity" 20
  bao "Hai vai tro CSDL khac nhau"    test "$app_user" != "$admin_user"
  # I4: thiet ke muc 4.5 doi "migration da ap dung het". Mot ban cap nhat co migration chet giua
  # chung van co the co API len duoc, nhung luoc do CSDL thi da lech -- day dung la thu phai
  # kich hoat quay lui, nen no nam o nhom COT LOI.
  bao "Bang migration co ban ghi (__EFMigrationsHistory)" \
      it_nhat 'SELECT count(*) FROM "__EFMigrationsHistory"' 1

  # ===== NHOM BO SUNG: dung de nhac nguoi van hanh, KHONG BAO GIO de quay lui =====
  # I4: container caddy -- Caddy hong nghia la HTTPS hong, nhung quay lui ma nguon khong sua duoc
  # chuyen do, nen day la muc nhac chu khong phai muc chan.
  bao_phu "Container caddy dang chay" eval 'dc ps --status running caddy | grep -q caddy'
  bao_phu "Tep .env quyen 600"        test "$(stat -c '%a' "$env")" = "600"
  bao_phu "backup.env quyen 600, chu root" \
      test "$(stat -c '%a:%U' "$THU_MUC_CAU_HINH/backup.env")" = "600:root"
  bao_phu "Container API KHONG biet khoa R2" api_khong_lo_khoa_r2
  bao_phu "In duoc PDF (Chromium co trong image)" \
      dc exec -T api sh -c 'find "$PLAYWRIGHT_BROWSERS_PATH" \( -name headless_shell -o -name chrome \) | head -1 | grep -q .'
  # I4: kiem TU BEN NGOAI qua ten mien that. Cac muc tren deu goi localhost:8080 tu BEN TRONG
  # container api, nen Caddy hong, DNS sai hay chung chi het han van cho ra "DAT" -- trong khi
  # giao dan go dia chi vao trinh duyet thi khong vao duoc. Chi kiem khi da co ten mien.
  if [ -n "$TEN_MIEN" ]; then
    bao_phu "HTTPS tu ben ngoai tra ve 200" \
        curl -fsS --max-time 20 "https://$TEN_MIEN/api/suc-khoe/san-sang"
  fi
  # The phuc hoi con nam tren chinh may chu sau 7 ngay nghia la no chua duoc cat ra ngoai --
  # mat may chu la mat luon kha nang phuc hoi. Nhac nguoi van hanh lam not. CO Y la muc BO SUNG:
  # day la viec cua con nguoi, khong phai loi cua ban ma nguon dang chay (xem ghi chu C4 o tren).
  bao_phu "The phuc hoi da duoc cat ngoai may chu" \
      eval "[ ! -f '$THU_MUC_CAU_HINH/the-phuc-hoi.txt' ] || \
            [ -z \"\$(find '$THU_MUC_CAU_HINH/the-phuc-hoi.txt' -mtime +7)\" ]"
  if [ "${QLGX_BO_QUA_R2:-0}" != "1" ]; then
    # Finding 4 (Minor, vong review cuoi cung): ban truoc `source`/`eval` thang backup.env
    # ("set -a; . '$THU_MUC_CAU_HINH/backup.env'; set +a") -- DUNG chinh mau ma ca qlgx-runner.sh
    # lan qlgx-restore.sh CO GHI CHU CANH BAO ky (xem nap_cau_hinh o hai script do): QLGX_GIU_LAI
    # la mot gia tri NHIEU TU khong co dau nhay bao quanh, `source` no duoi `set -euo pipefail` se
    # lam bash hieu tu thu hai tro di la MOT LENH rieng va thoat ngay voi "8: command not found"
    # (chi "chay dung" truoc day vi cac khoa RESTIC_*/AWS_* dung TRUOC QLGX_GIU_LAI trong tep --
    # doi thu tu ghi la vo tinh hong ma khong ai biet, loi bi `eval`/`bao` nuot mat). Gio goi lai
    # dung subcommand moi dem-snapshot-nhan cua qlgx-runner.sh -- NOI DUY NHAT doc backup.env an
    # toan (qua nap_cau_hinh, tung khoa mot bang doc_env_kv) -- chi can no chay duoc (ma thoat 0)
    # la du de chung minh kho mo duoc, khong quan tam dem ra bao nhieu (dung mot nhan chac chan
    # khong ai dat that lam "mui do").
    bao_phu "Kho restic mo duoc" \
      "$GOC_UNG_DUNG/scripts/qlgx-runner.sh" dem-snapshot-nhan _qlgx_tu_kiem_chung_ping_
    # I4: thiet ke muc 4.5 doi "kho restic mo duoc VA co >= 1 snapshot". Chi kiem "mo duoc" thi
    # mot may chu da chay 3 thang ma CHUA HE co ban sao nao (timer bi tat, R2 tu choi ghi) van
    # cho `qlgx status` toan DAT -- dung loai im lang nguy hiem nhat.
    bao_phu "Kho restic co it nhat 1 ban sao" \
      eval '[ "$("'"$GOC_UNG_DUNG"'/scripts/qlgx-runner.sh" dem-snapshot)" -ge 1 ]'
  fi
  echo "--------------------------"
  if [ "$so_phu" -gt 0 ]; then
    ghi_log canh-bao "$so_phu muc CAN XEM LAI (khong chan he thong chay, va KHONG bao gio lam" \
                     "mot lan cap nhat tu quay lui)."
  fi
  if [ "$so_loi" -gt 0 ]; then
    ghi_log loi "$so_loi muc COT LOI KHONG DAT."
    return 1
  fi
  ghi_log thong-tin "Toan bo muc cot loi DAT."
}

# quay_lui <commit> [danh_dau_hong]
# danh_dau_hong = 1 (mac dinh) -> ghi commit vua roi khoi vao .phien-ban-hong, tuc la cap_nhat se
#   KHONG BAO GIO tu dong thu lai ban do nua. Chi dung khi ly do quay lui THAT SU la "ban moi
#   khong len duoc".
# danh_dau_hong = 0 -> quay lui nhung KHONG dan nhan hong. Dung cho moi ly do khac (C4: mot muc
#   kiem chung bo sung KHONG DAT khong chung minh ban ma nguon do hong -- dan nhan hong o do se
#   khoa cung giao xu o ban cu vinh vien).
quay_lui() {
  local commit="${1:-}" danh_dau_hong="${2:-1}"
  [ -n "$commit" ] || commit=$(cat "$GOC_UNG_DUNG/.phien-ban-truoc" 2>/dev/null || true)
  [ -n "$commit" ] || bao_loi_va_thoat "Khong biet quay lui ve dau (thieu .phien-ban-truoc)."
  # Ghi lai commit dang chay TRUOC khi checkout doi huong -- day chinh la "commit hong" ta dang
  # ROI KHOI (du goi tu cap_nhat ngay sau mot lan merge that bai, hay goi tay qua CLI sau nay) --
  # dung de danh dau .phien-ban-hong ben duoi, tranh cap_nhat tu dong thu lai dung ban da biet
  # hong o lan chay ke tiep (xem giai thich o cap_nhat).
  local commit_hong
  commit_hong=$(git -C "$GOC_CHECKOUT" rev-parse HEAD)
  ghi_log canh-bao "Quay lui ve $commit"
  git -C "$GOC_CHECKOUT" checkout -q "$commit" --
  # Boc chung dc build/up trong `if ! ( ... )` (giong cach cap_nhat da lam voi build ban moi o
  # duoi) thay vi de tran duoi set -euo pipefail: neu KHONG boc, `dc up -d api` that bai se bi
  # errexit giet script NGAY, bo qua ca cho_san_sang lan cau huong dan "dung qlgx restore" ben
  # duoi -- dung luc giao xu mat dich vu va can chi dan nhat thi thong diep lai bi nuot mat.
  if ! (dc build api && dc up -d api && cho_san_sang 300); then
    bao_loi_va_thoat "Quay lui roi ma he thong VAN khong len duoc. Dung 'qlgx restore' voi " \
                      "ban sao 'truoc-cap-nhat' vua tao."
  fi
  # Quay lui THANH CONG: .phien-ban-truoc khong con dung nua (no dang tro DUNG ve commit vua
  # quay ve -- neu de nguyen, mot lan `qlgx rollback` ke tiep se checkout/rebuild/restart vo ich
  # ve chinh ban dang chay roi bao "thanh cong" trong khi khong lui di dau ca). Xoa no di de lan
  # `qlgx rollback` sau bao dung loi "khong con gi de quay lui" (xem kiem tra dau ham nay).
  rm -f "$GOC_UNG_DUNG/.phien-ban-truoc"
  # Danh dau commit VUA ROI KHOI la "da biet hong", de cap_nhat khong tu dong thu lai no o lan
  # chay ke tiep (xem kiem tra .phien-ban-hong dau cap_nhat). CHI khi ly do quay lui that su la
  # "ban moi khong len duoc" -- xem ghi chu tham so o dau ham.
  if [ "$danh_dau_hong" = "1" ]; then
    echo "$commit_hong" > "$GOC_UNG_DUNG/.phien-ban-hong"
  else
    ghi_log thong-tin "KHONG danh dau $(echo "$commit_hong" | cut -c1-8) la HONG: ly do quay lui" \
                      "khong phai la 'ban moi khong len duoc'."
  fi
  ghi_log thong-tin "Da quay lui thanh cong ve $commit."
  # I12 -- thiet ke muc 4.3 buoc 5. Quay lui chi tra lai MA NGUON; migration cua ban moi (neu da
  # chay) da doi LUOC DO CSDL va khong co gi dao no lai. Ma cu chay tren luoc do moi la mot trang
  # thai chua tung duoc kiem thu, co the ghi du lieu sai am tham trong nhieu ngay. Truoc day cau
  # nhac nay CHI xuat hien khi chinh viec quay lui cung that bai -- tuc la dung o kich ban hay
  # xay ra nhat thi khong ai duoc nhac.
  ghi_log canh-bao "LUU Y: quay lui chi tra lai MA NGUON. Neu ban moi da chay migration thi LUOC" \
                   "DO CSDL van la cua ban moi. Neu thay bat thuong, hay phuc hoi CSDL ve ban sao" \
                   "'truoc-cap-nhat' vua tao:  qlgx restore --snapshot ${SNAPSHOT_TRUOC_CAP_NHAT:-<id ban truoc-cap-nhat>} --apply"
}

# Boc goi qlgx-runner.sh -- tach thanh ham RIENG (thay vi goi thang duong dan tai cho dung) de bo
# test thay the duoc bang mot ham cung ten (giong cach `dc()` da lam voi docker compose o tren),
# khong can dung toi mot qlgx-runner.sh that (Docker/Postgres/restic that).
runner_cmd() { "$GOC_UNG_DUNG/scripts/qlgx-runner.sh" "$@"; }

# Sao luu BAT BUOC truoc cap nhat, xac minh bang SU THAT thay vi tin vao ma thoat cua
# qlgx-runner.sh -- cung mot loai loi va cung mot cach sua nhu sao_luu_bat_buoc() trong
# qlgx-restore.sh (Task 14, xem ghi chu dai o do): gianh_khoa_hoac_bo_qua CO Y `exit 0` khi mot
# luot sao-luu/kiem-tra/dong-bo-danh-sach KHAC dang giu khoa runner (dung cho cron -- "bo qua luot
# nay" la hanh vi MONG DOI, khong phai loi) -- nhung "cap nhat" o day thi khac han: neu dung dua
# vao ma thoat 0 do de coi la "da co ban sao truoc-cap-nhat that", mot lan `qlgx update` trung
# dung luc timer sao-luu/kiem-tra dang chay se AM THAM bo qua ban sao an toan duy nhat truoc khi
# ghi de CSDL bang migration cua ban moi.
#
# VONG REVIEW CUOI CUNG (Finding 2, Important): ham nay tung KHONG TON TAI -- cap_nhat() truoc day
# goi thang qlgx-runner.sh sao-luu roi tin ngay ma thoat cua no, dung LO HONG y het cai da duoc
# phat hien va sua o qlgx-restore.sh boi Task 14 -- chi khac la Task 12 (noi cap_nhat() duoc
# viet) dong TRUOC khi Task 14 phat hien va sua loi do, nen khong duoc thua huong ban sua.
#
# TAI SAO DUNG "dem-snapshot-nhan" CUA qlgx-runner.sh THAY VI TU DOC backup.env/goi restic O DAY:
# qlgx-runner.sh la NOI DUY NHAT an toan doc /etc/qlgx/backup.env (QLGX_GIU_LAI la mot gia tri
# NHIEU TU khong co dau nhay bao quanh -- `source` thang tep do duoi `set -euo pipefail` se lam
# bash hieu tu thu hai tro di la MOT LENH rieng va thoat ngay voi "8: command not found", xem
# nap_cau_hinh trong qlgx-runner.sh). Tu viet lai buoc doc khoa/goi restic O DAY se la BAN THU HAI
# cua chinh nguy co da duoc ghi chu ky trong ca hai script kia -- de subcommand moi nay la NOI
# DUY NHAT biet dem snapshot, ca install.sh lan qlgx-restore.sh (neu sau nay can) deu goi lai no.
#
# KHONG hop nhat vao chinh sao_luu_bat_buoc() cua qlgx-restore.sh: ham do con phai chay duoc tren
# MOT VPS TRANG chi voi The phuc hoi (--card, KHONG co /etc/qlgx/backup.env, xem nap_cau_hinh_ph) --
# ep no goi qua qlgx-runner.sh (von LUON doi hoi backup.env qua nap_cau_hinh) se pha vo duong
# phuc hoi tren may trang. install.sh chi chay tren mot may DA CAI (luon co backup.env) nen khong
# vuong gioi han do.
sao_luu_bat_buoc_cap_nhat() {
  local nhan="truoc-cap-nhat" so_truoc so_sau
  so_truoc=$(runner_cmd dem-snapshot-nhan "$nhan") \
    || bao_loi_va_thoat "Khong dem duoc snapshot hien co (truoc khi sao luu) -- DUNG, khong cap" \
                        "nhat. Kiem tra kho restic/backup.env truoc khi thu lai."
  case "$so_truoc" in ''|*[!0-9]*) so_truoc=0 ;; esac

  runner_cmd sao-luu --nhan "$nhan" --nguon truoc_cap_nhat \
    || bao_loi_va_thoat "Sao luu truoc cap nhat THAT BAI — dung cap nhat. Sua sao luu truoc."

  # Thu lai vai lan: kho co the vua duoc mot lenh khac (timer sao-luu/kiem-tra) nha khoa, hoac
  # chi muc restic chua kip hien snapshot vua day len (cung ly do/cung so lan thu nhu
  # sao_luu_bat_buoc() trong qlgx-restore.sh).
  for _ in 1 2 3; do
    so_sau=$(runner_cmd dem-snapshot-nhan "$nhan") || so_sau=""
    case "$so_sau" in ''|*[!0-9]*) so_sau=0 ;; esac
    [ "$so_sau" -gt "$so_truoc" ] && break
    sleep 2
  done
  [ "${so_sau:-0}" -gt "$so_truoc" ] \
    || bao_loi_va_thoat "qlgx-runner.sh bao thanh cong nhung kho KHONG co them ban sao" \
                        "'$nhan' nao ($so_truoc -> ${so_sau:-0}) -- rat co the mot luot sao luu" \
                        "khac (timer) dang giu khoa runner nen luot nay bi bo qua lang le (xem" \
                        "gianh_khoa_hoac_bo_qua trong qlgx-runner.sh). DUNG cap nhat: khong co" \
                        "ban sao that thi khong co duong lui."
  # I12: GIU LAI id cua ban sao vua tao. Truoc day ham nay chi DEM roi vut so lieu di, nen cau
  # huong dan "chay qlgx restore voi ban truoc-cap-nhat vua tao" khong bao gio in duoc id cu the
  # -- dung thu nguoi van hanh can nhat khi dang hoang.
  SNAPSHOT_TRUOC_CAP_NHAT=$(runner_cmd id-snapshot-nhan "$nhan" 2>/dev/null || true)
  ghi_log thong-tin "Da xac nhan ban sao luu truoc cap nhat ton tai that trong kho" \
                    "(so ban '$nhan': $so_truoc -> $so_sau, ban moi nhat: ${SNAPSHOT_TRUOC_CAP_NHAT:-?})."
}

cap_nhat() {
  local hien_tai moi ban_hong
  git -C "$GOC_CHECKOUT" fetch --quiet origin "$NHANH"
  hien_tai=$(git -C "$GOC_CHECKOUT" rev-parse HEAD)
  moi=$(git -C "$GOC_CHECKOUT" rev-parse "origin/$NHANH")

  # Ban moi nhat tren GitHub trung voi mot ban DA TUNG bi quay lui vi hong -- KHONG tu dong thu
  # lai. Thieu kiem tra nay, Task 15 (systemd timer goi cap_nhat dinh ky) se lap VO HAN: moi lan
  # chay lai deu thay origin/$NHANH "moi hon" HEAD dang detached o ban cu, merge len, build lai
  # DUNG ban da biet hong, crash, tu quay lui, roi lap lai y het o lan ke tiep -- moi vong ngung
  # dich vu vai phut cho cho_san_sang that bai.
  ban_hong=$(cat "$GOC_UNG_DUNG/.phien-ban-hong" 2>/dev/null || true)
  if [ -n "$ban_hong" ] && [ "$moi" = "$ban_hong" ]; then
    ghi_log canh-bao "Ban moi nhat tren GitHub ($(echo "$moi" | cut -c1-8)) da bi danh dau HONG" \
                     "o lan cap nhat truoc — khong tu dong thu lai. Xoa .phien-ban-hong neu" \
                     "muon thu lai thu cong, hoac cho ban va moi hon."
    return 1
  fi

  if [ "$hien_tai" = "$moi" ]; then
    ghi_log thong-tin "Da la ban moi nhat ($(echo "$hien_tai" | cut -c1-8)) — khong lam gi."
    return 0
  fi
  ghi_log thong-tin "Co ban moi: $(echo "$hien_tai" | cut -c1-8) -> $(echo "$moi" | cut -c1-8)"

  # Sao luu BAT BUOC truoc khi cap nhat. Day la luoi an toan cuoi cung neu migration cua ban
  # moi lam hong lieu do — pg_advisory_lock chi chong hai tien trinh chay migration cung luc,
  # KHONG chong duoc mot migration sai.
  if [ "$BO_QUA_SAO_LUU" -eq 0 ] && [ "${QLGX_BO_QUA_R2:-0}" != "1" ]; then
    ghi_log thong-tin "Sao luu truoc khi cap nhat"
    sao_luu_bat_buoc_cap_nhat
  else
    ghi_log canh-bao "BO QUA sao luu truoc cap nhat theo yeu cau."
  fi

  echo "$hien_tai" > "$GOC_UNG_DUNG/.phien-ban-truoc"

  git -C "$GOC_CHECKOUT" merge --ff-only "origin/$NHANH" \
    || bao_loi_va_thoat "Khong merge fast-forward duoc — ban checkout da bi sua tay?"

  # Bo sung khoa .env moi neu ban moi can, KHONG dung toi khoa cu (xem sinh_env).
  sinh_env "$GOC_UNG_DUNG/.env"

  if ! (dc build api && dc up -d api && cho_san_sang 300); then
    ghi_log loi "Ban moi khong len duoc — dang quay lui."
    # Day la LY DO DUY NHAT chinh dang de dan nhan "ban nay hong" (danh_dau_hong = 1).
    quay_lui "$hien_tai" 1
    return 1
  fi

  # N3: ban moi co the doi Caddyfile hoac doi image Caddy. Khong lam moi container caddy thi thay
  # doi do khong co hieu luc cho toi lan khoi dong lai may -- va khong ai biet vi sao. KHONG goi
  # cau_hinh_https (ham do hoi ten mien va ghi de Caddyfile), chi dung lai container theo cau
  # hinh dang co. `|| true`: may khong co Caddy van cap nhat binh thuong.
  dc up -d caddy >/dev/null 2>&1 || ghi_log canh-bao "Khong lam moi duoc container caddy."

  # C-4: `install.sh --domain=<ten mien>` tren may DA cai gio sinh lai Caddyfile va bat HTTPS
  # that su. Khong truyen --domain thi khong dung gi toi cau hinh dang chay.
  cap_nhat_https_neu_co_ten_mien

  # I2: goi lai o moi lan cap nhat de cac may DA cai tu truoc cung duoc siet quyen, va de mot
  # migration moi vua tao them bang khong nam lai duoi quyen vai tro ung dung.
  dat_quyen_bang_sao_luu

  # C4: CHI nhom COT LOI duoc quyet dinh quay lui. Cac muc bo sung (The phuc hoi chua cat ra
  # ngoai, kho R2 tam thoi khong mo duoc, Chromium loi vat) van duoc in day du o duoi nhung
  # TUYET DOI khong duoc lam mot ban moi ĐANG CHAY TOT bi quay lui va dan nhan HONG vinh vien.
  if ! tu_kiem_chung_cot_loi; then
    ghi_log loi "Kiem chung COT LOI sau cap nhat KHONG DAT — quay lui."
    quay_lui "$hien_tai" 1
    return 1
  fi
  # Bang day du: chi de nguoi van hanh doc, khong anh huong quyet dinh nao.
  tu_kiem_chung || true

  # Lam moi CAC TEP unit va CLI `qlgx` theo ban vua merge -- ban moi co the doi lenh, doi duong
  # dan, hoac them mot timer. KHONG goi cai_dat_systemd (ham do hoi/bat/tat timer): lua chon bat
  # hay tat sao luu tu dong la cua nguoi van hanh, mot lan cap nhat khong duoc phep lat lai.
  # `|| true`: may khong co systemd van cap nhat binh thuong, chi khong co timer.
  cai_tep_systemd || true

  ghi_log thong-tin "Cap nhat xong: $(echo "$moi" | cut -c1-8)"
}

# Chep cac tep unit va CLI `qlgx` vao he thong. KHONG bat/tat timer nao -- tach rieng de goi
# duoc tu ca duong CAI MOI (sau do moi hoi va bat timer) lan duong CAP NHAT (chi lam moi tep,
# TUYET DOI khong dung toi lua chon bat/tat ma nguoi van hanh da tu quyet dinh truoc do).
#
# Cac tep unit ghi cung duong dan /opt/qlgx/WebApp (duong dan that khi cai binh thuong). Bo test
# tro GOC_CHECKOUT sang thu muc tam, nen phai thay lai cho khop -- neu khong, unit se tro toi mot
# duong dan khong ton tai ma khong ai phat hien cho toi luc timer chay lan dau.
# /run/systemd/system la cach kiem chuan cua chinh systemd de biet no DANG CHAY (khac voi "goi
# systemd co cai dat"). Can no vi bo kiem thu dau-cuoi chay install.sh BEN TRONG mot container
# khong co systemd: neu khong co kiem tra nay, `systemctl daemon-reload` that bai va `set -e`
# giet ca lan cai, lam hong mot bo test von khong lien quan gi toi timer.
co_systemd() { command -v systemctl >/dev/null 2>&1 && [ -d /run/systemd/system ]; }

cai_tep_systemd() {
  # Goi lai sau khi `git merge` cua cap_nhat co the da dua ve script moi (xem dat_bit_thuc_thi).
  dat_bit_thuc_thi
  # CLI `qlgx` cai bat ke co systemd hay khong -- no chi goi lai cac script, khong can timer.
  install -m 755 "$GOC_UNG_DUNG/scripts/qlgx" /usr/local/bin/qlgx

  co_systemd || {
    ghi_log canh-bao "May nay khong chay systemd -- BO QUA cai timer. Sao luu tu dong va hang" \
                     "doi cong viec (nut bam tren giao dien web) se KHONG chay."
    return 1
  }

  local thu_muc_unit="${THU_MUC_UNIT_SYSTEMD:-/etc/systemd/system}"
  local nguon="$GOC_UNG_DUNG/scripts/systemd"
  [ -d "$nguon" ] || bao_loi_va_thoat "Khong tim thay thu muc unit systemd: $nguon"
  mkdir -p "$thu_muc_unit"

  local tep ten
  for tep in "$nguon"/*.service "$nguon"/*.timer; do
    [ -f "$tep" ] || continue
    ten="$(basename "$tep")"
    if [ "$GOC_UNG_DUNG" = "/opt/qlgx/WebApp" ]; then
      install -m 644 "$tep" "$thu_muc_unit/$ten"
    else
      # Dung '|' lam dau phan cach cua sed vi duong dan chua '/'. Duong dan co chua '|' thi tu
      # choi han thay vi sinh ra mot unit hong am tham.
      case "$GOC_UNG_DUNG" in
        *'|'*) bao_loi_va_thoat "Duong dan ung dung chua ky tu '|' -- khong thay the duoc trong unit systemd." ;;
      esac
      sed "s|/opt/qlgx/WebApp|$GOC_UNG_DUNG|g" "$tep" > "$thu_muc_unit/$ten"
      chmod 644 "$thu_muc_unit/$ten"
    fi
  done

  # daemon-reload BAT BUOC sau moi lan doi noi dung unit: systemd giu ban da phan tich trong bo
  # nho, khong tu doc lai tep. Thieu buoc nay thi `systemctl enable` ben duoi bat DUNG ban cu.
  systemctl daemon-reload
}

cai_dat_systemd() {
  local bat_sao_luu="y"
  if [ "$KHONG_TUONG_TAC" -eq 0 ]; then
    read -rp "Bat sao luu tu dong 4 lan/ngay len R2? [Y/n]: " bat_sao_luu </dev/tty
    bat_sao_luu="${bat_sao_luu:-y}"
  fi

  # Khong co systemd thi khong con gi de bat/tat -- cai_tep_systemd da bao roi.
  cai_tep_systemd || return 0

  case "$bat_sao_luu" in
    [Yy]*|'')
      systemctl enable --now qlgx-runner.timer qlgx-backup.timer qlgx-verify.timer
      ghi_log thong-tin "Da bat: hang doi cong viec (moi phut), sao luu 4 lan/ngay," \
                        "dien tap phuc hoi hang tuan."
      ;;
    *)
      # Van bat runner.timer: khong co no thi nut bam tren giao dien web se khong bao gio chay --
      # nguoi dung bam "Sao luu ngay", dong cong viec nam mai o trang thai 'cho' va khong ai biet
      # tai sao. runner.timer khong tu sao luu gi, no chi lam viec da duoc yeu cau.
      systemctl enable --now qlgx-runner.timer
      ghi_log canh-bao "CHUA bat sao luu tu dong. Bat sau bang:" \
                       "systemctl enable --now qlgx-backup.timer qlgx-verify.timer"
      ;;
  esac

  # Cap nhat tu dong MAC DINH TAT: cap nhat khong giam sat tren du lieu so sach giao xu la rui
  # ro khong dang. Bat tay bang: systemctl enable --now qlgx-update.timer
  systemctl disable qlgx-update.timer >/dev/null 2>&1 || true
  ghi_log thong-tin "Cap nhat tu dong dang TAT. Bat bang: systemctl enable --now qlgx-update.timer"
}

main() {
  phan_tich_tham_so "$@"
  if [ "$CHI_TRANG_THAI" -eq 1 ]; then tu_kiem_chung; exit $?; fi

  kiem_tra_tien_de
  cai_phu_thuoc
  cau_hinh_tuong_lua
  lay_ma_nguon

  if la_cai_moi; then ghi_log thong-tin "=== CAI MOI ==="
  else ghi_log thong-tin "=== CAP NHAT ==="; cap_nhat; exit $?; fi

  # C-4: hoi/kiem ten mien NGAY DAU luong cai moi, truoc khi dung toi CSDL, tai khoan quan tri
  # hay kho R2. Neu phai dung thi dung khi may con sach, khong bo lai mot may cai do dang.
  # cau_hinh_https ben duoi van goi lai ham nay (khong hai: luc do TEN_MIEN da co).
  chan_neu_thieu_ten_mien

  sinh_env "$GOC_UNG_DUNG/.env"
  ghi_backup_env "$THU_MUC_CAU_HINH/backup.env"
  bat_postgres
  tao_vai_tro_rls
  bat_api
  # SAU bat_api: migration da chay va da tao ba bang sao luu, gio moi doi duoc chu so huu.
  dat_quyen_bang_sao_luu
  khoi_tao_giao_xu_va_admin
  cau_hinh_https
  cai_dat_systemd
  if [ "${QLGX_BO_QUA_R2:-0}" != "1" ]; then
    # PHAI co buoc nay TRUOC lan sao luu dau tien: `restic backup` KHONG tu khoi tao kho. Voi mot
    # bucket R2 moi tinh no dung lai voi "unable to open config file / Is there a repository at
    # the following location?" -- tuc la MOI lan cai moi deu that bai o dung dong duoi day.
    ghi_log thong-tin "Mo kho sao luu R2 (tao moi neu chua co)"
    # KHONG de ba lenh nay giet script duoi `set -e`: The phuc hoi PHAI duoc in ra bang moi gia.
    # Neu chet o day (khoa R2 sai, mat khau restic khong khop kho cu, mang chap chon) thi
    # in_the_phuc_hoi khong bao gio chay, va nguoi van hanh khong co duong nao lay lai mat khau
    # restic ngoai viec doc tay backup.env -- ma ho khong biet tep do ton tai. Bang tu kiem chung
    # ben duoi da co muc "Kho restic co it nhat 1 ban sao" de trang thai nay khong bi im lang.
    if "$GOC_UNG_DUNG/scripts/qlgx-runner.sh" khoi-tao-kho; then
      ghi_log thong-tin "Chay sao luu dau tien de chung minh duong ong song that"
      "$GOC_UNG_DUNG/scripts/qlgx-runner.sh" sao-luu --nhan "cai-dat-lan-dau" \
        || ghi_log loi "Sao luu dau tien THAT BAI -- xem bang tu kiem chung va nhat ky ben tren." \
                       "He thong van cai xong, nhung HAY SUA SAO LUU TRUOC KHI NHAP SO SACH."
      "$GOC_UNG_DUNG/scripts/qlgx-runner.sh" kiem-tra \
        || ghi_log loi "Kiem tra kho sao luu THAT BAI -- xem nhat ky ben tren."
    else
      ghi_log loi "KHONG mo duoc kho sao luu R2. He thong van cai xong va chay duoc, nhung HIEN" \
                  "CHUA CO SAO LUU NAO. Sua khoa R2/mat khau restic trong" \
                  "$THU_MUC_CAU_HINH/backup.env roi chay:  qlgx backup"
    fi
  fi
  # KHONG de `set -e` giet script neu tu_kiem_chung tra loi -- luu lai ma thoat THAY VI thoat
  # ngay, vi Thẻ phục hồi BAT BUOC phai duoc in ra du bang kiem chung co mot muc phu KHONG DAT
  # (vd Chromium thieu). Neu thoat truoc khi in the: .env da co san nen lan chay lai se di vao
  # nhanh cap_nhat (stub, chua lam gi) -- khong con duong nao lay lai the ngoai doc tay
  # backup.env. Dung if/else (khong phai `tu_kiem_chung || ...`) de tranh -e can thiep dang khac.
  local ket_qua_kiem_chung
  if tu_kiem_chung; then ket_qua_kiem_chung=0; else ket_qua_kiem_chung=1; fi
  in_the_phuc_hoi
  # CHI danh dau hoan tat SAU khi The phuc hoi da duoc in. Moi lan chay bi dut truoc diem nay deu
  # phai duoc coi la "chua cai xong" va chay lai tu dau (xem ghi chu o TEP_HOAN_TAT).
  danh_dau_cai_xong
  ghi_log thong-tin "HOAN TAT. Mo: ${TEN_MIEN:+https://$TEN_MIEN}${TEN_MIEN:-http://<dia-chi-ip-may-chu>}"
  exit "$ket_qua_kiem_chung"
}

# Cho phep bo test nap file nay ma khong chay gi.
[ "${QLGX_CHI_NAP_HAM:-0}" = "1" ] && return 0
main "$@"
