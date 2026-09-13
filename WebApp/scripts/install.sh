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

cho_san_sang() {
  local gioi_han="${1:-180}" i
  for i in $(seq 1 "$gioi_han"); do
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
  mat_khau=$(hoi_hoac_bien QLGX_ADMIN_MAT_KHAU "Mat khau (toi thieu 8 ky tu)")
  ho_ten=$(hoi_hoac_bien QLGX_ADMIN_HO_TEN "Ho ten hien thi")

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
      api dotnet Qlgx.Api.dll tao-tai-khoan-quan-tri >/tmp/qlgx-tao-admin.log 2>&1 || true
  cat /tmp/qlgx-tao-admin.log

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
    bao_loi_va_thoat "Khong tao duoc tai khoan quan tri (CSDL xac nhan tai khoan KHONG ton tai). Xem /tmp/qlgx-tao-admin.log"
  fi
}

cau_hinh_https() {
  if [ -z "$TEN_MIEN" ] && [ "$KHONG_TUONG_TAC" -eq 0 ]; then
    TEN_MIEN=$(hoi_hoac_bien QLGX_TEN_MIEN "Ten mien (de trong neu chua co)")
  fi
  if [ -z "$TEN_MIEN" ]; then
    # KHONG hua "chay lai script voi --domain=..." -- luc do .env da co san nen la_cai_moi tra
    # false, main() di vao nhanh cap_nhat (hien la stub chua lam gi, xem Task 12), Caddyfile
    # KHONG BAO GIO duoc sinh lai va giao xu se chay HTTP mai mai ma khong ai biet. Thong diep
    # phai trung thuc voi nhung gi lam duoc HOM NAY.
    ghi_log canh-bao "CHUA CO TEN MIEN -- he thong se chay qua HTTP THUAN, khong ma hoa duong " \
                     "truyen. Du lieu giao dan (ho ten, ngay sinh, so can cuoc) di qua mang o " \
                     "dang doc duoc. Khi co ten mien, can cau hinh lai HTTPS THU CONG (xem tai " \
                     "lieu van hanh) -- chay lai install.sh --domain=<ten mien> luc nay CHUA co " \
                     "tac dung (di vao duong cap nhat, hien chua xu ly lai HTTPS); tinh nang tu " \
                     "dong cau hinh lai qua --domain se co o ban cap nhat sau."
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
  dc up -d caddy
  ghi_log thong-tin "Caddy da chay${TEN_MIEN:+ cho $TEN_MIEN (HTTPS tu dong)}."
}

in_the_phuc_hoi() {
  local tep="$THU_MUC_CAU_HINH/the-phuc-hoi.txt"
  local be="$THU_MUC_CAU_HINH/backup.env"
  local env="$GOC_UNG_DUNG/.env"
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
  1. Dung mot may chu Linux moi
  2. curl -fsSL $KHO_GIT/raw/$NHANH/WebApp/scripts/qlgx-restore.sh -o qlgx-restore.sh
  3. sudo bash qlgx-restore.sh --card the-phuc-hoi.txt          (in ra ke hoach)
  4. sudo bash qlgx-restore.sh --card the-phuc-hoi.txt --apply  (thuc hien)
=======================================================
EOF
  chmod 600 "$tep"
  cat "$tep"
  ghi_log canh-bao "IN THE TREN RA GIAY hoac chep vao noi an toan NGOAI may chu nay, roi xoa: " \
                   "rm $tep -- de tren chinh may chu thi mat may la mat luon kha nang phuc hoi."
}

tu_kiem_chung() {
  local so_loi=0
  local env="$GOC_UNG_DUNG/.env"
  bao() { # bao <ten> <lenh...>
    local ten="$1"; shift
    if "$@" >/dev/null 2>&1; then printf '  DAT           %s\n' "$ten"
    else printf '  KHONG DAT     %s\n' "$ten"; so_loi=$((so_loi + 1)); fi
  }
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
    local tep="/tmp/qlgx-env-api.$$"
    if ! dc exec -T api env > "$tep" 2>/dev/null; then rm -f "$tep"; return 1; fi
    if grep -q '^AWS_SECRET_ACCESS_KEY=' "$tep"; then rm -f "$tep"; return 1; fi
    rm -f "$tep"
  }

  echo "--- Bang tu kiem chung ---"
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
  bao "Tep .env quyen 600"            test "$(stat -c '%a' "$env")" = "600"
  bao "backup.env quyen 600, chu root" \
      test "$(stat -c '%a:%U' "$THU_MUC_CAU_HINH/backup.env")" = "600:root"
  bao "Container API KHONG biet khoa R2" api_khong_lo_khoa_r2
  bao "In duoc PDF (Chromium co trong image)" \
      dc exec -T api sh -c 'find "$PLAYWRIGHT_BROWSERS_PATH" \( -name headless_shell -o -name chrome \) | head -1 | grep -q .'
  # The phuc hoi con nam tren chinh may chu sau 7 ngay nghia la no chua duoc cat ra ngoai --
  # mat may chu la mat luon kha nang phuc hoi. Bao KHONG DAT de nguoi van hanh nho lam not.
  bao "The phuc hoi da duoc cat ngoai may chu" \
      eval "[ ! -f '$THU_MUC_CAU_HINH/the-phuc-hoi.txt' ] || \
            [ -z \"\$(find '$THU_MUC_CAU_HINH/the-phuc-hoi.txt' -mtime +7)\" ]"
  if [ "${QLGX_BO_QUA_R2:-0}" != "1" ]; then
    bao "Kho restic mo duoc" \
      eval "set -a; . '$THU_MUC_CAU_HINH/backup.env'; set +a; restic snapshots --json >/dev/null"
  fi
  echo "--------------------------"
  if [ "$so_loi" -gt 0 ]; then
    ghi_log loi "$so_loi muc KHONG DAT."
    return 1
  fi
  ghi_log thong-tin "Toan bo muc kiem chung DAT."
}

quay_lui() {
  local commit="${1:-}"
  [ -n "$commit" ] || commit=$(cat "$GOC_UNG_DUNG/.phien-ban-truoc" 2>/dev/null || true)
  [ -n "$commit" ] || bao_loi_va_thoat "Khong biet quay lui ve dau (thieu .phien-ban-truoc)."
  ghi_log canh-bao "Quay lui ve $commit"
  git -C "$GOC_CHECKOUT" checkout -q "$commit" --
  dc build api && dc up -d api
  cho_san_sang 300 \
    || bao_loi_va_thoat "Quay lui roi ma he thong VAN khong len duoc. Dung 'qlgx restore' voi " \
                        "ban sao 'truoc-cap-nhat' vua tao."
  ghi_log thong-tin "Da quay lui thanh cong ve $commit."
}

cap_nhat() {
  local hien_tai moi
  git -C "$GOC_CHECKOUT" fetch --quiet origin "$NHANH"
  hien_tai=$(git -C "$GOC_CHECKOUT" rev-parse HEAD)
  moi=$(git -C "$GOC_CHECKOUT" rev-parse "origin/$NHANH")

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
    "$GOC_UNG_DUNG/scripts/qlgx-runner.sh" sao-luu --nhan "truoc-cap-nhat" --nguon truoc_cap_nhat \
      || bao_loi_va_thoat "Sao luu truoc cap nhat THAT BAI — dung cap nhat. Sua sao luu truoc."
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
    quay_lui "$hien_tai"
    return 1
  fi

  tu_kiem_chung || { ghi_log loi "Kiem chung sau cap nhat KHONG DAT — quay lui."; quay_lui "$hien_tai"; return 1; }
  ghi_log thong-tin "Cap nhat xong: $(echo "$moi" | cut -c1-8)"
}

# Task 15 se thay the ham nay bang logic tao don vi systemd that de he thong tu khoi dong lai
# dich vu sau khi may chu reboot. Tam thoi day chi la cho giu cho.
cai_dat_systemd() {
  ghi_log canh-bao "cai_dat_systemd: chua cai dat (se lam o Task 15)"
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

  sinh_env "$GOC_UNG_DUNG/.env"
  ghi_backup_env "$THU_MUC_CAU_HINH/backup.env"
  bat_postgres
  tao_vai_tro_rls
  bat_api
  khoi_tao_giao_xu_va_admin
  cau_hinh_https
  cai_dat_systemd
  if [ "${QLGX_BO_QUA_R2:-0}" != "1" ]; then
    ghi_log thong-tin "Chay sao luu dau tien de chung minh duong ong song that"
    "$GOC_UNG_DUNG/scripts/qlgx-runner.sh" sao-luu --nhan "cai-dat-lan-dau"
    "$GOC_UNG_DUNG/scripts/qlgx-runner.sh" kiem-tra
  fi
  # KHONG de `set -e` giet script neu tu_kiem_chung tra loi -- luu lai ma thoat THAY VI thoat
  # ngay, vi Thẻ phục hồi BAT BUOC phai duoc in ra du bang kiem chung co mot muc phu KHONG DAT
  # (vd Chromium thieu). Neu thoat truoc khi in the: .env da co san nen lan chay lai se di vao
  # nhanh cap_nhat (stub, chua lam gi) -- khong con duong nao lay lai the ngoai doc tay
  # backup.env. Dung if/else (khong phai `tu_kiem_chung || ...`) de tranh -e can thiep dang khac.
  local ket_qua_kiem_chung
  if tu_kiem_chung; then ket_qua_kiem_chung=0; else ket_qua_kiem_chung=1; fi
  in_the_phuc_hoi
  ghi_log thong-tin "HOAN TAT. Mo: ${TEN_MIEN:+https://$TEN_MIEN}${TEN_MIEN:-http://<dia-chi-ip-may-chu>}"
  exit "$ket_qua_kiem_chung"
}

# Cho phep bo test nap file nay ma khong chay gi.
[ "${QLGX_CHI_NAP_HAM:-0}" = "1" ] && return 0
main "$@"
