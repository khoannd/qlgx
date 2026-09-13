#!/usr/bin/env bash
# Phuc hoi QLGX tu ban sao luu restic -- chay TREN HOST, ngoai container.
#
# NGUYEN TAC: KHONG BAO GIO ghi de tai cho. Cach sai (va la cach hau het huong dan tren mang
# chi) la `dropdb && createdb && pg_restore` -- giua `dropdb` va `pg_restore` thanh cong co mot
# khoang thoi gian KHONG TON TAI DU LIEU NAO; pg_restore loi giua chung la mat trang so sach
# giao xu nhieu nam.
#
# Cach o day: nap snapshot sang mot CSDL MOI, KIEM CHUNG no, roi HOAN DOI TEN. Hai lenh
# ALTER DATABASE ... RENAME gan nhu tuc thoi va dao nguoc duoc, nen cua so "he thong khong co
# du lieu" BANG KHONG va thoi gian ngung phuc vu chi bang thoi gian khoi dong lai container API
# (vai giay), khong phai thoi gian pg_restore (co the vai phut). CSDL cu KHONG bi xoa: no chi
# duoc DOI TEN sang <ten>_truoc_phuc_hoi_<dau thoi gian> va giu lai nhieu ngay.
#
# Chay duoc TREN MOT VPS TRANG chi voi The phuc hoi:  --card the-phuc-hoi.txt
# (khong can /etc/qlgx/backup.env, tuc khong can install.sh da tung ghi cau hinh sao luu tren
# may nay). Xem ham `nap_cau_hinh_ph` va lenh `--lay-cau-hinh` ben duoi.
#
# Ba che do:
#   qlgx-restore.sh --snapshot <id>            -> CHI IN KE HOACH, khong doi mot thu gi
#   qlgx-restore.sh --snapshot <id> --apply    -> thuc hien phuc hoi that
#   qlgx-restore.sh --dien-tap --snapshot latest -> nap vao CSDL rieng, kiem chung, roi XOA
set -euo pipefail

THU_MUC_CAU_HINH="${THU_MUC_CAU_HINH:-/etc/qlgx}"
SNAPSHOT=""
AP_DUNG=0
DIEN_TAP=0
TEP_THE=""
MA_JOB=""
LAY_CAU_HINH=""
GOC_UNG_DUNG_THAM_SO=""
TAM_PH=""
GIU_CSDL_CU_NGAY="${QLGX_GIU_CSDL_CU_NGAY:-7}"
# HAI khai niem "cho API len" KHAC NHAU, co y tach rieng:
#   - CHO_SAN_SANG_GIAY: cho luc khoi dong BINH THUONG (khong co ai dang doi), rong rai duoc.
#   - CHO_SAN_SANG_SAU_HOAN_DOI_GIAY: cho NGAY SAU khi hoan doi ten, tuc la trong dung khoang
#     thoi gian giao xu KHONG dung duoc phan mem. Moi giay cho o day la mot giay ngung phuc vu
#     tren mot CSDL co the dang hong, nen phai NGAN hon nhieu -- 90 giay du de phan biet "dang
#     khoi dong cham" voi "hong that" (API nay khoi dong het khoang 10-20 giay khi khoe manh),
#     va la tran tren cho thoi gian phat hien truoc khi bat dau dao nguoc.
CHO_SAN_SANG_GIAY="${QLGX_CHO_SAN_SANG_GIAY:-300}"
CHO_SAN_SANG_SAU_HOAN_DOI_GIAY="${QLGX_CHO_SAN_SANG_SAU_HOAN_DOI_GIAY:-90}"
# Nhanh dung de tu tai chung.sh khi chay tu MOT tep duy nhat (xem nap_thu_vien_ph).
NHANH_MAC_DINH="${QLGX_NHANH:-webapp-phase-1}"
# Dung CHUNG tep khoa voi qlgx-runner.sh: phuc hoi va sao luu deu ghi vao kho restic, vao thu
# muc tam trong container postgres, va vao bang ban_sao_luu/trang_thai_sao_luu -- hai viec do
# chay chong nhau la giam dap len nhau. Cho phep ghi de qua bien moi truong de kiem chung tren
# may khong co /var/lock that (vd Windows qua Git Bash).
KHOA_RUNNER="${KHOA_RUNNER:-/var/lock/qlgx-runner.lock}"
# Do dai toi da cua mot dinh danh Postgres. Vuot qua la Postgres TU CAT BOT MA KHONG BAO LOI --
# ten ta yeu cau va ten that se khac nhau, va lenh doi ten thu hai se tim khong ra CSDL.
readonly DAI_TOI_DA_TEN_DB=63
readonly HAU_TO_LUU="_truoc_phuc_hoi_"

# The phuc hoi huong dan nguoi van hanh tai DUNG MOT tep qlgx-restore.sh ve mot may TRANG bang
# curl -- luc do chung.sh KHONG nam canh no va `source` se chet ngay dong dau, tren dung may ma
# phan mem nay ton tai de cuu. Vi vay: co thi nap tai cho, khong co thi tu tai ve thu muc tam
# (dung cach install.sh's nap_thu_vien da lam). Kiem ma thoat cua curl RIENG de bao loi ro rang
# khi mat mang, thay vi de bash bao mot loi cu phap kho hieu luc source phai mot trang 404.
nap_thu_vien_ph() {
  local d; d="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
  if [ -f "$d/chung.sh" ]; then
    # shellcheck source=/dev/null
    source "$d/chung.sh"
    return
  fi
  local tam goc_raw
  tam=$(mktemp -d)
  goc_raw="https://raw.githubusercontent.com/khoannd/qlgx/$NHANH_MAC_DINH/WebApp/scripts"
  if ! command -v curl >/dev/null 2>&1; then
    echo "[X] Khong thay chung.sh canh $0 va cung khong co lenh curl de tai ve." >&2
    exit 1
  fi
  if ! curl -fsSL "$goc_raw/chung.sh" -o "$tam/chung.sh"; then
    echo "[X] Khong tai duoc chung.sh tu $goc_raw -- kiem tra mang hoac dat QLGX_NHANH." >&2
    exit 1
  fi
  # shellcheck source=/dev/null
  source "$tam/chung.sh"
}
nap_thu_vien_ph

phan_tich_tham_so_phuc_hoi() {
  AP_DUNG=0; DIEN_TAP=0; LAY_CAU_HINH=""
  while [ $# -gt 0 ]; do
    case "$1" in
      --snapshot)     [ $# -ge 2 ] || bao_loi_va_thoat "--snapshot can mot gia tri."
                      SNAPSHOT="$2"; shift 2 ;;
      --card)         [ $# -ge 2 ] || bao_loi_va_thoat "--card can duong dan tep the phuc hoi."
                      TEP_THE="$2"; shift 2 ;;
      --goc)          [ $# -ge 2 ] || bao_loi_va_thoat "--goc can duong dan thu muc ung dung."
                      GOC_UNG_DUNG_THAM_SO="$2"; shift 2 ;;
      --ma-job)       [ $# -ge 2 ] || bao_loi_va_thoat "--ma-job can mot gia tri."
                      MA_JOB="$2"; shift 2 ;;
      --lay-cau-hinh) [ $# -ge 2 ] || bao_loi_va_thoat "--lay-cau-hinh can thu muc dich."
                      LAY_CAU_HINH="$2"; shift 2 ;;
      --apply)        AP_DUNG=1; shift ;;
      --dien-tap)     DIEN_TAP=1; shift ;;
      *) bao_loi_va_thoat "Tham so khong hieu: $1" ;;
    esac
  done
}

# Lay phan gia tri sau dau ':' DAU TIEN cua dong dau tien bat dau bang <tien_to>.
# CO Y KHONG dung `awk -F': *'`: gia tri that la mot URL kho restic dang
# "s3:https://tai-khoan.r2.cloudflarestorage.com/bucket" -- no CHUA dau ':' nen cat theo truong
# se chi lay duoc "s3" roi vut phan con lai, va toan bo phuc hoi se tro tay khong. `sub` chi
# thay the LAN KHOP DAU TIEN nen giu nguyen moi dau ':' con lai trong gia tri.
gia_tri_dong_the() {
  local tep="$1" tien_to="$2"
  awk -v t="$tien_to" '
    index($0, t) == 1 { sub(/^[^:]*:[ \t]*/, ""); sub(/[ \t\r]+$/, ""); print; exit }
  ' "$tep"
}

# Trich khoa tu The phuc hoi (dinh dang do in_the_phuc_hoi trong install.sh sinh ra). Day la
# duong duy nhat de phuc hoi tren MOT MAY TRANG: khong co /etc/qlgx/backup.env o do.
doc_the_phuc_hoi() {
  local tep="$1"
  [ -r "$tep" ] || bao_loi_va_thoat "Khong doc duoc the phuc hoi: $tep"
  # Mat khau nam o DONG RIENG sau dong nhan "MAT KHAU RESTIC ...". Trong the that, cau giai
  # thich trong ngoac keo dai sang dong thu hai va dong do MOI ket thuc bang ':' -- nen quy tac
  # la: bo qua moi dong ket thuc bang ':' (van con la phan nhan), lay dong khac rong dau tien
  # con lai. The rut gon mot dong (dung trong bo test) cung thoa quy tac nay.
  RESTIC_PASSWORD=$(awk '
    /MAT KHAU RESTIC/ { doc = 1; next }
    doc && $0 ~ /[^ \t\r]/ {
      if ($0 ~ /:[ \t\r]*$/) next
      gsub(/^[ \t]+/, ""); gsub(/[ \t\r]+$/, ""); print; exit
    }' "$tep")
  RESTIC_REPOSITORY=$(gia_tri_dong_the "$tep" 'KHO SAO LUU')
  AWS_ACCESS_KEY_ID=$(gia_tri_dong_the "$tep" 'R2 KEY ID')
  AWS_SECRET_ACCESS_KEY=$(gia_tri_dong_the "$tep" 'R2 SECRET')
  [ -n "${RESTIC_PASSWORD:-}" ] || bao_loi_va_thoat "The phuc hoi thieu MAT KHAU RESTIC."
  # Chan truong hop the bi cat mat dong mat khau: luc do awk se vo tinh nhat dung dong
  # "KHO SAO LUU : ..." ke tiep lam mat khau, va nguoi van hanh se nhan mot loi "wrong password"
  # kho hieu thay vi loi that "the phuc hoi hong".
  case "$RESTIC_PASSWORD" in
    [A-Z]*:*) bao_loi_va_thoat "The phuc hoi thieu MAT KHAU RESTIC (dong ngay duoi nhan khong phai mat khau)." ;;
  esac
  [ -n "${RESTIC_REPOSITORY:-}" ] || bao_loi_va_thoat "The phuc hoi thieu KHO SAO LUU."
  export RESTIC_PASSWORD RESTIC_REPOSITORY AWS_ACCESS_KEY_ID AWS_SECRET_ACCESS_KEY
}

ten_db_tam() { printf '%s_%s' "$1" "$(date '+%Y%m%d_%H%M%S')"; }

# Ten CSDL cu sau khi hoan doi. Postgres CAT NGAM dinh danh dai qua 63 byte, nen tu cat phan
# GOC (khong phai phan hau to) de ten sinh ra luon <= 63 va van con nhan ra duoc bang mau LIKE
# o don_csdl_cu (hai ham dung CHUNG mot cach cat).
goc_da_cat_cho_luu() {
  local goc="$1" con_lai
  con_lai=$(( DAI_TOI_DA_TEN_DB - ${#HAU_TO_LUU} - 15 ))   # 15 = do dai 20260913_101010
  printf '%s' "${goc:0:$con_lai}"
}
ten_db_luu() { printf '%s%s%s' "$(goc_da_cat_cho_luu "$1")" "$HAU_TO_LUU" "$(date '+%Y%m%d_%H%M%S')"; }
mau_db_luu() { printf '%s%s%%' "$(goc_da_cat_cho_luu "$1")" "$HAU_TO_LUU"; }

# Ten CSDL di thang vao cau lenh SQL (khong the tham so hoa dinh danh trong Postgres). Chung
# den tu .env va tu ham sinh ten o tren -- van kiem lai truoc khi ghep chuoi.
kiem_ten_db_an_toan() {
  case "$1" in
    *[!A-Za-z0-9_]*|'') bao_loi_va_thoat "Ten CSDL khong hop le (chi cho phep chu, so, gach duoi): '$1'" ;;
  esac
}

nap_cau_hinh_ph() {
  if [ -n "$TEP_THE" ]; then
    doc_the_phuc_hoi "$TEP_THE"
    ghi_log thong-tin "Doc khoa sao luu tu The phuc hoi: $TEP_THE (khong dung $THU_MUC_CAU_HINH/backup.env)."
  else
    [ -r "$THU_MUC_CAU_HINH/backup.env" ] \
      || bao_loi_va_thoat "Khong co $THU_MUC_CAU_HINH/backup.env va cung khong co --card <the phuc hoi>."
    # CO Y KHONG `source` ca tep (giong qlgx-runner.sh): QLGX_GIU_LAI la mot gia tri nhieu tu
    # KHONG co dau nhay bao quanh, `source` no duoi `set -euo pipefail` se lam bash hieu tu thu
    # hai tro di la mot LENH rieng va thoat ngay voi "8: command not found".
    local khoa gia_tri
    for khoa in RESTIC_REPOSITORY RESTIC_PASSWORD AWS_ACCESS_KEY_ID AWS_SECRET_ACCESS_KEY; do
      gia_tri="$(doc_env_kv "$THU_MUC_CAU_HINH/backup.env" "$khoa")"
      # if/fi chu KHONG `[ ... ] && export ...`: duoi `set -e`, mot cau lenh "&&" o cuoi than
      # vong lap tra ve 1 (khoa vang mat) se giet ca script ngay tai do.
      if [ -n "$gia_tri" ]; then export "$khoa=$gia_tri"; fi
    done
    [ -n "${RESTIC_REPOSITORY:-}" ] || bao_loi_va_thoat "backup.env khong co RESTIC_REPOSITORY."
    [ -n "${RESTIC_PASSWORD:-}" ] || bao_loi_va_thoat "backup.env khong co RESTIC_PASSWORD."
  fi

  # Thu tu uu tien cua goc ung dung: --goc > bien moi truong > backup.env > mac dinh. Voi --card
  # tren mot may trang, hai nguon dau la duong duy nhat (backup.env khong ton tai o do).
  GOC_UNG_DUNG="$GOC_UNG_DUNG_THAM_SO"
  [ -n "$GOC_UNG_DUNG" ] || GOC_UNG_DUNG="${QLGX_GOC_UNG_DUNG:-}"
  if [ -z "$GOC_UNG_DUNG" ] && [ -r "$THU_MUC_CAU_HINH/backup.env" ]; then
    GOC_UNG_DUNG="$(doc_env_kv "$THU_MUC_CAU_HINH/backup.env" QLGX_GOC_UNG_DUNG)"
  fi
  GOC_UNG_DUNG="${GOC_UNG_DUNG:-/opt/qlgx/WebApp}"
  THU_MUC_LOG="${QLGX_THU_MUC_LOG:-}"
  if [ -z "$THU_MUC_LOG" ] && [ -r "$THU_MUC_CAU_HINH/backup.env" ]; then
    THU_MUC_LOG="$(doc_env_kv "$THU_MUC_CAU_HINH/backup.env" QLGX_THU_MUC_LOG)"
  fi
  THU_MUC_LOG="${THU_MUC_LOG:-/var/log/qlgx}"

  command -v restic >/dev/null 2>&1 \
    || bao_loi_va_thoat "Chua co lenh 'restic' tren may nay. Cai: apt-get install restic (hoac tai ban tinh tu github.com/restic/restic)."
}

# Kiem TRUOC khi dong toi bat cu thu gi: khong co bo ung dung thi khong co container postgres de
# nap vao. Tren mot VPS trang, viec nay phai lam bang install.sh TRUOC (bo cai dung ma nguon
# GitHub, khong nam trong ban sao luu), roi moi chay lenh nay voi --card de nap du lieu ve.
kiem_ung_dung_da_co() {
  if [ ! -f "$GOC_UNG_DUNG/docker-compose.yml" ] || [ ! -f "$GOC_UNG_DUNG/.env" ]; then
    ghi_log loi "Chua co bo ung dung dang chay tai $GOC_UNG_DUNG."
    cat >&2 <<EOF

PHUC HOI TU MOT MAY TRANG -- lam theo dung thu tu nay:
  1. Cai bo ung dung trang (ma nguon nam tren GitHub, KHONG nam trong ban sao luu):
       curl -fsSL https://raw.githubusercontent.com/khoannd/qlgx/$NHANH_MAC_DINH/WebApp/scripts/install.sh | sudo bash
     Khi duoc hoi kho R2, nhap lai dung KHO SAO LUU / R2 KEY ID / R2 SECRET tren The phuc hoi.
  2. Xem ke hoach phuc hoi (khong doi gi):
       sudo bash $0 --card <the-phuc-hoi.txt> --snapshot latest
  3. Nap du lieu cu ve:
       sudo bash $0 --card <the-phuc-hoi.txt> --snapshot latest --apply
  (Tuy chon) Lay lai cac tep cau hinh cu tu chinh ban sao luu:
       sudo bash $0 --card <the-phuc-hoi.txt> --lay-cau-hinh /root/cau-hinh-cu
  Neu bo ung dung nam o thu muc khac, them: --goc <thu muc>

EOF
    bao_loi_va_thoat "Dung lai: chua co gi de nap du lieu vao."
  fi
}

dc() { docker compose --project-directory "$GOC_UNG_DUNG" \
         -f "$GOC_UNG_DUNG/docker-compose.yml" \
         -f "$GOC_UNG_DUNG/docker-compose.prod.yml" "$@"; }
env_ung_dung() { doc_env_kv "$GOC_UNG_DUNG/.env" "$1"; }
pg() { dc exec -T postgres psql -tAX -v ON_ERROR_STOP=1 -U "$(env_ung_dung POSTGRES_USER)" "$@"; }
# Doc MOT gia tri vo huong: bo khoang trang va ky tu CR (Windows/Git Bash) de so sanh so hoc
# khong gap "8\r" khong phai so.
pg1() { pg "$@" | tr -d ' \r'; }

# MOI truy van restic CHI DOC deu phai co --no-lock. Ly do (da gap that trong kiem thu dau-cuoi):
# `restic check` va `restic forget --prune` cua qlgx-runner.sh giu khoa DOC QUYEN tren kho, va
# `--retry-lock` cua restic MAC DINH la 0 -- mot lenh doc chay ngay sau do se THAT BAI TUC THI
# voi "repository is already locked" chu khong cho. Neu ta lai nuot loi do (2>/dev/null) thi ket
# qua rong se bi hieu nham thanh "khong co snapshot nao", tuc la bao dong gia ngay giua mot lan
# phuc hoi. --no-lock bo han viec xin khoa (an toan cho lenh chi doc), con loi (neu con) duoc GHI
# RA nhat ky thay vi nuot.
TEP_LOI_RESTIC=""
restic_doc() {
  [ -n "$TEP_LOI_RESTIC" ] || TEP_LOI_RESTIC=$(mktemp "${TMPDIR:-/var/tmp}/qlgx-restic-loi.XXXXXX")
  restic "$@" --no-lock 2>"$TEP_LOI_RESTIC" || true
}
in_loi_restic_gan_nhat() {
  [ -n "$TEP_LOI_RESTIC" ] && [ -s "$TEP_LOI_RESTIC" ] || return 0
  ghi_log canh-bao "restic bao: $(tr '\n' ' ' < "$TEP_LOI_RESTIC" | cut -c1-300)"
}

# CO Y KHONG dung `restic snapshots --latest N`. Da tu kiem chung tren kho that: `--latest` nhom
# theo (host, PATHS) va tra ve N snapshot moi nhat CUA TUNG NHOM. qlgx-runner.sh sao luu tu mot
# thu muc tam MOI moi lan chay (mktemp), nen MOI snapshot roi vao mot nhom rieng -- `--latest 1`
# tra ve MOT DONG CHO MOI SNAPSHOT va `head -1` nhat phai ban CU. Hau qua that da quan sat duoc
# trong kiem thu dau-cuoi: buoc xac minh ben duoi tuong 'khong co snapshot moi' va tu choi phuc
# hoi du ban sao luu vua tao thanh cong. Vi vay doc TOAN BO danh sach (restic in theo thu tu
# thoi gian tang dan).
json_toan_bo_snapshot() { restic_doc snapshots --json; }
id_snapshot_cuoi_cung() { grep -o '"short_id":"[0-9a-f]*"' | tail -1 | cut -d'"' -f4; }
# Dem so snapshot mang mot nhan -- day moi la thuoc tinh ta thuc su can ("co THEM mot ban sao
# truoc-phuc-hoi hay khong"), khong phu thuoc vao cach restic nhom hay sap xep.
dem_snapshot_theo_nhan() {
  local nhan="$1" so
  so=$(json_toan_bo_snapshot | grep -o "nhan=${nhan}\"" | grep -c . || true)
  printf '%s' "${so:-0}"
}

# Co du lieu giao xu nao se bi ghi de khong? Tra ve 0 (co) / 1 (khong co gi de mat).
# Dung de quyet dinh buoc sao luu bat buoc co y nghia hay khong -- KHONG phai de "cho phep bo
# qua cho tien". Hai tinh huong that KHONG co gi de mat: (a) CSDL chua ton tai tren cum, (b) ban
# cai vua dung xong tren mot VPS TRANG (0 giao dan, 0 gia dinh) dang doi nap du lieu cu ve. Doc
# khong duoc thi COI NHU CO du lieu (an toan ve phia sao luu).
co_du_lieu_can_bao_ve() {
  local db="$1" co_db gd gdinh
  co_db=$(pg1 -d postgres -c "SELECT count(*) FROM pg_database WHERE datname = '${db//\'/\'\'}'" 2>/dev/null || echo '')
  # BA trang thai, KHONG phai hai. Ban truoc gop "khong doc duoc" chung voi "chac chan khong co"
  # (`[ "$co_db" != "1" ]` -> bo qua sao luu) -- do la mot hoi quy nghiem trong: chi can container
  # postgres dang khoi dong lai, `dc exec` truot mot nhip, hay POSTGRES_USER sai la phep do tra ve
  # chuoi RONG, va mot giao xu that voi 4000 giao dan se bi ghi de MA KHONG co ban sao ngoai may --
  # dung bao dam ma Critical 1 vua dung len. Gio: chi "0" (dem duoc, chac chan khong co CSDL do)
  # moi cho phep bo qua; moi gia tri khac deu la "khong chac" va di theo nhanh AN TOAN (van sao luu).
  case "$co_db" in
    0)
      ghi_log thong-tin "CSDL '$db' chua ton tai tren cum -- khong co du lieu nao bi ghi de."
      return 1 ;;
    1) : ;;
    *)
      ghi_log canh-bao "Khong do duoc su ton tai cua CSDL '$db' (ket qua: '${co_db}') --" \
                       "COI NHU CO du lieu, VAN sao luu truoc khi ghi de."
      return 0 ;;
  esac
  gd=$(pg1 -d "$db" -c 'SELECT count(*) FROM giao_dan' 2>/dev/null || echo 'loi')
  gdinh=$(pg1 -d "$db" -c 'SELECT count(*) FROM gia_dinh' 2>/dev/null || echo 'loi')
  # Bat ky gia tri nao KHONG phai mot so (rong, 'loi', thong diep la) deu duoc coi la "khong biet"
  # -> nga ve phia AN TOAN: van sao luu.
  case "$gd" in ''|*[!0-9]*) gd='loi' ;; esac
  case "$gdinh" in ''|*[!0-9]*) gdinh='loi' ;; esac
  if [ "$gd" = 'loi' ] || [ "$gdinh" = 'loi' ]; then
    ghi_log canh-bao "Khong dem duoc giao dan/gia dinh cua '$db' -- COI NHU CO du lieu, van sao luu."
    return 0
  fi
  if [ "$gd" -eq 0 ] && [ "$gdinh" -eq 0 ]; then
    ghi_log thong-tin "CSDL '$db' khong co giao dan/gia dinh nao (ban cai moi chua co so sach)."
    return 1
  fi
  return 0
}

# Buoc [1/7]. KHONG tin vao mot minh ma thoat cua qlgx-runner.sh: khi mot luot sao luu khac dang
# giu flock, `gianh_khoa_hoac_bo_qua` cua runner CO Y `exit 0` (bo qua luot nay la hanh vi dung
# cua no, vi day la cron chay lai duoc). Nhung o day thi khac han: ma 0 do se bi hieu nham la
# "da sao luu xong" va ta di thang vao hoan doi, trong khi ban sao restic -- duong lui DUY NHAT
# khi mat may chu -- vua bi bo qua ma khong ai nhin thay. Vi vay xac minh bang SU THAT: phai co
# mot snapshot MOI, mang dung nhan truoc-phuc-hoi, xuat hien trong kho.
sao_luu_bat_buoc() {
  local db_hien="$1"
  if ! co_du_lieu_can_bao_ve "$db_hien"; then
    ghi_log canh-bao "BO QUA sao luu truoc phuc hoi: khong co so sach giao xu nao bi ghi de. " \
                     "(Day la duong 'may trang': vua cai xong, dang nap du lieu cu ve.)"
    return 0
  fi
  local so_truoc so_sau id_moi i
  so_truoc=$(dem_snapshot_theo_nhan truoc-phuc-hoi)
  "$GOC_UNG_DUNG/scripts/qlgx-runner.sh" sao-luu --nhan truoc-phuc-hoi --nguon truoc_phuc_hoi \
    || bao_loi_va_thoat "Khong sao luu duoc trang thai hien tai -- DUNG, KHONG phuc hoi. He thong giu nguyen."
  # Thu lai vai lan: kho co the vua duoc mot lenh khac nha khoa, hoac chi muc chua kip hien ra.
  for i in 1 2 3; do
    so_sau=$(dem_snapshot_theo_nhan truoc-phuc-hoi)
    [ "${so_sau:-0}" -gt "${so_truoc:-0}" ] && break
    in_loi_restic_gan_nhat
    sleep 2
  done
  if [ "${so_sau:-0}" -le "${so_truoc:-0}" ]; then
    in_loi_restic_gan_nhat
    bao_loi_va_thoat "qlgx-runner.sh bao thanh cong nhung kho KHONG co them ban sao " \
                     "truoc-phuc-hoi nao ($so_truoc -> $so_sau) -- rat co the mot luot sao luu " \
                     "khac dang giu khoa nen luot nay bi bo qua lang le. DUNG, KHONG phuc hoi: " \
                     "khong co duong lui thi khong ghi de."
  fi
  id_moi=$(json_toan_bo_snapshot | id_snapshot_cuoi_cung)
  ghi_log thong-tin "Da xac nhan ban sao luu truoc phuc hoi ton tai that trong kho" \
                    "(so ban truoc-phuc-hoi: $so_truoc -> $so_sau, ban moi nhat: ${id_moi:-?})."
}

# Doc mot tag cua snapshot (vd giao_dan=2050) -- dung de doi chieu "hien tai" voi "trong ban sao"
# trong ke hoach, va de kiem chung sau khi nap. In '?' khi khong doc duoc.
doc_tag_snapshot() {
  local khoa="$1" ra=""
  ra=$(restic_doc snapshots --json "$SNAPSHOT" | tr ',' '\n' \
       | grep -o "\"${khoa}=[^\"]*\"" | head -1 | sed "s/^\"${khoa}=//; s/\"\$//") || true
  printf '%s' "${ra:-?}"
}

in_ke_hoach() {
  local db_hien="$1" db_moi="$2" gd_hien gdinh_hien gd_sn gdinh_sn nhan_sn
  gd_hien=$(pg1 -d "$db_hien" -c 'SELECT count(*) FROM giao_dan' 2>/dev/null || echo '?')
  gdinh_hien=$(pg1 -d "$db_hien" -c 'SELECT count(*) FROM gia_dinh' 2>/dev/null || echo '?')
  gd_sn=$(doc_tag_snapshot giao_dan)
  gdinh_sn=$(doc_tag_snapshot gia_dinh)
  nhan_sn=$(doc_tag_snapshot nhan)
  cat <<EOF

=========== KE HOACH PHUC HOI (chua thuc hien gi) ===========
Snapshot          : $SNAPSHOT  (nhan: $nhan_sn)
Kho sao luu       : $RESTIC_REPOSITORY
Goc ung dung      : $GOC_UNG_DUNG
CSDL hien tai     : $db_hien
CSDL nap tam vao  : $db_moi
CSDL cu se doi ten: $(goc_da_cat_cho_luu "$db_hien")${HAU_TO_LUU}<dau thoi gian>  (giu $GIU_CSDL_CU_NGAY ngay, KHONG xoa ngay)

Doi chieu so lieu:
                      hien tai        trong ban sao
  Giao dan        :   ${gd_hien}      ${gd_sn}
  Gia dinh        :   ${gdinh_hien}      ${gdinh_sn}

Cac buoc se lam:
  1. Sao luu ngay trang thai hien tai (--nhan truoc-phuc-hoi)  [BAT BUOC, khong tat duoc]
  2. Nap snapshot vao CSDL MOI: $db_moi  (CSDL dang chay KHONG bi dong toi)
  3. Kiem chung CSDL moi (dem giao dan/gia dinh, so giao xu, RLS con bat)
  4. Dung container api
  5. Hoan doi ten: $db_hien -> ban luu; $db_moi -> $db_hien
  6. Khoi dong api, cho san sang; KHONG len duoc thi DAO NGUOC doi ten trong vai giay

Them --apply de thuc hien. Them --dien-tap de chi tap duot (nap + kiem chung roi xoa).
=============================================================
EOF
}

kiem_chung_csdl_moi() {
  local db="$1" so_gd so_gdinh so_gx so_bang_rls gd_sn gdinh_sn
  so_gd=$(pg1 -d "$db" -c 'SELECT count(*) FROM giao_dan') || return 1
  so_gdinh=$(pg1 -d "$db" -c 'SELECT count(*) FROM gia_dinh') || return 1
  so_gx=$(pg1 -d "$db" -c 'SELECT count(*) FROM giao_xu') || return 1
  so_bang_rls=$(pg1 -d "$db" -c 'SELECT count(*) FROM pg_class WHERE relrowsecurity') || return 1
  ghi_log thong-tin "Kiem chung: $so_gd giao dan, $so_gdinh gia dinh, $so_gx giao xu, $so_bang_rls bang bat RLS."

  if [ "${so_gx:-0}" -lt 1 ]; then
    ghi_log loi "CSDL nap vao KHONG co giao xu nao -- ban sao hong."; return 1
  fi
  # Nguong 20 lay dung tu bang tu kiem chung cua install.sh: mot ban nap thieu chinh sach RLS
  # la mot ban nap co the ro ri du lieu giua cac giao xu -- tu choi hoan doi.
  if [ "${so_bang_rls:-0}" -lt 20 ]; then
    ghi_log loi "Chi $so_bang_rls bang bat RLS -- ban sao thieu chinh sach bao mat."; return 1
  fi
  # Doi chieu voi so lieu da ghi vao tag luc SAO LUU: bat duoc ca truong hop pg_restore "thanh
  # cong" nhung nap thieu bang/thieu dong (vd het dia giua chung o mot bang lon).
  gd_sn=$(doc_tag_snapshot giao_dan); gdinh_sn=$(doc_tag_snapshot gia_dinh)
  if [ "$gd_sn" != '?' ] && [ "$gd_sn" != "$so_gd" ]; then
    ghi_log loi "So giao dan nap duoc ($so_gd) khac voi so ghi trong ban sao ($gd_sn) -- ban nap khong tron ven."
    return 1
  fi
  if [ "$gdinh_sn" != '?' ] && [ "$gdinh_sn" != "$so_gdinh" ]; then
    ghi_log loi "So gia dinh nap duoc ($so_gdinh) khac voi so ghi trong ban sao ($gdinh_sn) -- ban nap khong tron ven."
    return 1
  fi
  return 0
}

# Tach rieng thanh ham de bo kiem thu dau-cuoi ghi de duoc (gia lap "API len nhung hong") va
# chung minh duong DAO NGUOC that su chay.
# Gioi han la SO GIAY THAT (han chot theo dong ho), KHONG phai so vong lap. Khac biet nay khong
# phai chuyen chu nghia: khi CSDL moi hong that, container api roi vao vong crash-restart, va moi
# lan `dc exec` vao mot container dang khoi dong lai mat NHIEU GIAY chu khong phai tuc thi -- dem
# theo vong lap thi "90" hoa ra 10-25 phut, dung trong khoang thoi gian giao xu khong dung duoc
# phan mem va ta dang cho de quyet dinh dao nguoc. Da quan sat that trong kiem thu kich ban 3b
# (api o trang thai "Restarting (139)"). Han chot theo dong ho cho dung dieu da hua.
cho_api_san_sang() {
  local gioi_han="${1:-$CHO_SAN_SANG_GIAY}" het_han
  het_han=$(( $(date +%s) + gioi_han ))
  while [ "$(date +%s)" -lt "$het_han" ]; do
    if dc exec -T api curl -fsS http://localhost:8080/api/suc-khoe/san-sang >/dev/null 2>&1; then
      return 0
    fi
    sleep 1
  done
  return 1
}

# Dong MOI ket noi con sot roi doi ten. ALTER DATABASE ... RENAME that bai neu con bat ky phien
# nao mo tren CSDL do -- mot yeu cau HTTP den muon, mot psql cua nguoi van hanh, hay chinh
# container api chua kip dung han deu du lam hong buoc hoan doi. Thu lai vai lan vi giua lan dong
# phien va lan doi ten van con mot khe hep de mot ket noi moi kip vao.
doi_ten_db() {
  local tu="$1" den="$2" i
  kiem_ten_db_an_toan "$tu"; kiem_ten_db_an_toan "$den"
  for i in 1 2 3 4 5; do
    pg -d postgres -c "SELECT pg_terminate_backend(pid) FROM pg_stat_activity
                       WHERE datname = '$tu' AND pid <> pg_backend_pid();" >/dev/null 2>&1 || true
    if pg -d postgres -c "ALTER DATABASE \"$tu\" RENAME TO \"$den\";" >/dev/null 2>&1; then
      return 0
    fi
    ghi_log canh-bao "Doi ten $tu -> $den chua duoc (lan $i/5), thu lai sau 2 giay."
    sleep 2
  done
  return 1
}

xoa_db() {
  kiem_ten_db_an_toan "$1"
  pg -d postgres -c "DROP DATABASE IF EXISTS \"$1\" WITH (FORCE);" >/dev/null 2>&1 || true
}

# Vai tro CSDL la doi tuong TOAN CUM (khong thuoc rieng mot CSDL nao), nen ban dump theo CSDL
# khong tao lai duoc chung. Tren may trang, thieu vai tro thi moi lenh GRANT trong ban dump deu
# hong. Nhung CHI nap globals.sql khi THUC SU thieu vai tro: nap no tren mot cum dang song se
# ALTER ROLE ... PASSWORD ve mat khau CU trong ban sao, lam container api dang chay mat ket noi
# ngay lap tuc -- dung thu nen lam "cho chac".
nap_globals_neu_thieu_vai_tro() {
  local tep_globals="$1" vai_tro thieu=0
  [ -s "$tep_globals" ] || { ghi_log canh-bao "Ban sao khong co globals.sql -- bo qua buoc vai tro."; return 0; }
  for vai_tro in "$(env_ung_dung QLGX_APP_DB_USER)" "$(env_ung_dung QLGX_ADMIN_DB_USER)"; do
    [ -n "$vai_tro" ] || continue
    local co; co=$(pg1 -d postgres -c "SELECT count(*) FROM pg_roles WHERE rolname = '${vai_tro//\'/\'\'}'" 2>/dev/null || echo 0)
    [ "${co:-0}" -ge 1 ] || { thieu=1; ghi_log canh-bao "Cum CSDL chua co vai tro '$vai_tro'."; }
  done
  if [ "$thieu" -eq 0 ]; then
    ghi_log thong-tin "Moi vai tro CSDL da co san -- KHONG nap globals.sql (tranh doi mat khau cua he thong dang chay)."
    return 0
  fi
  ghi_log thong-tin "Nap globals.sql de tao lai vai tro va quyen toan cum."
  # KHONG ON_ERROR_STOP: globals.sql luon chua ca cac vai tro da ton tai (vd postgres), loi
  # "role already exists" o day la binh thuong va vo hai.
  dc exec -T postgres psql -X -U "$(env_ung_dung POSTGRES_USER)" -d postgres < "$tep_globals" >/dev/null 2>&1 || true
}

# Lay lai cac tep cau hinh (.env, docker-compose*, Caddyfile) tu ban sao. Dung cho kich ban may
# TRANG: .env chua mat khau CSDL cu, ma bo ung dung moi cai dat se sinh mat khau MOI khac hoan
# toan. KHONG dong toi CSDL nao.
lenh_lay_cau_hinh() {
  local dich="$1"
  mkdir -p "$dich"; chmod 700 "$dich" 2>/dev/null || true
  restic restore "$SNAPSHOT" --target "$dich" --include '*/cau-hinh' \
    || bao_loi_va_thoat "Khong lay duoc snapshot '$SNAPSHOT' tu kho."
  local thu_muc; thu_muc=$(find "$dich" -type d -name cau-hinh | head -1)
  [ -n "$thu_muc" ] || bao_loi_va_thoat "Snapshot khong chua thu muc cau-hinh."
  ghi_log thong-tin "Da lay cau hinh cu ra $thu_muc (chua mat khau -- xoa sau khi dung xong)."
  ls -1 "$thu_muc"
}

nap_snapshot_vao_csdl_moi() {
  local db_moi="$1" tam="$2"
  local thu_muc_dump ten_trong_container
  thu_muc_dump=$(find "$tam" -type d -name qlgx-dump | head -1)
  [ -n "$thu_muc_dump" ] || { ghi_log loi "Snapshot khong chua thu muc qlgx-dump."; return 1; }
  [ -s "$thu_muc_dump/toc.dat" ] || { ghi_log loi "Ban dump thieu muc luc toc.dat -- ban sao hong."; return 1; }

  nap_globals_neu_thieu_vai_tro "$(dirname "$thu_muc_dump")/globals.sql"

  # template0 + UTF8: khong ke thua bat ky doi tuong nao cua template1 (may chu co the da co
  # extension/bang trong template1) va chac chan dung bang ma cua ban dump.
  pg -d postgres -c "CREATE DATABASE \"$db_moi\" TEMPLATE template0 ENCODING 'UTF8';" >/dev/null \
    || { ghi_log loi "Khong tao duoc CSDL tam $db_moi."; return 1; }

  # Dump o dinh dang THU MUC (-Fd, xem qlgx-runner.sh) -- khong the truyen qua stdin nhu -Fc,
  # phai chep ca thu muc vao container postgres. Ten trong container mang hau to rieng cho moi
  # lan chay: mot lan phuc hoi khac (hoac mot lan sao luu) dang chay khong duoc ghi de len.
  ten_trong_container="qlgx-nap-$(basename "$tam")"
  # Bung vao MOT thu muc bao ngoai roi tro pg_restore vao "<bao>/qlgx-dump" -- CO Y khong dung
  # `tar --strip-components`: image postgres:17-alpine dung tar cua BusyBox, khong bao dam co
  # tuy chon do, va mot tuy chon khong duoc ho tro o day nghia la ban phuc hoi that bai ngay
  # tren may giao xu.
  if ! tar -cf - -C "$(dirname "$thu_muc_dump")" qlgx-dump \
       | dc exec -T postgres sh -c "rm -rf '/tmp/$ten_trong_container' && mkdir -p '/tmp/$ten_trong_container' && tar -xf - -C '/tmp/$ten_trong_container'"; then
    ghi_log loi "Khong chep duoc ban dump vao container postgres."
    dc exec -T postgres rm -rf "/tmp/$ten_trong_container" >/dev/null 2>&1 || true
    xoa_db "$db_moi"
    return 1
  fi

  ghi_log thong-tin "pg_restore vao $db_moi (song song 4 luong)"
  # KHONG --no-owner/--role: ban dump giu nguyen chu so huu that (vai tro nghiep vu tao bang qua
  # migration) va ta ket noi bang POSTGRES_USER (sieu nguoi dung cua cum) nen nap lai duoc y
  # nguyen. Ep doi chu so huu o day se lam lech quyen so voi he thong dang chay.
  if ! dc exec -T postgres pg_restore -j4 --exit-on-error \
        -U "$(env_ung_dung POSTGRES_USER)" -d "$db_moi" "/tmp/$ten_trong_container/qlgx-dump"; then
    ghi_log loi "pg_restore that bai -- xoa CSDL tam, he thong hien tai VAN NGUYEN VEN."
    dc exec -T postgres rm -rf "/tmp/$ten_trong_container" >/dev/null 2>&1 || true
    xoa_db "$db_moi"
    return 1
  fi
  dc exec -T postgres rm -rf "/tmp/$ten_trong_container" >/dev/null 2>&1 || true
  return 0
}

# Ghi ket qua vao bang cong viec CUA CSDL DANG PHUC VU. Goi SAU khi hoan doi: ban ghi trong CSDL
# cu da di theo CSDL cu. Ten cot la snake_case THAT trong CSDL (id, loai, trang_thai, nhat_ky,
# tao_luc, ket_thuc_luc) -- doi chieu truc tiep voi migration ThemBangSaoLuu.cs, khong doan.
#
# PHAI la INSERT ... ON CONFLICT chu KHONG duoc la UPDATE thuan: CSDL vua hoan doi vao la ban
# chup CU, no KHONG THE chua dong cong viec ma API tao ra SAU thoi diem chup. Mot UPDATE o day
# khop 0 dong va im lang khong lam gi -- man hinh quan tri se treo mai o "dang chay" du viec da
# xong tu lau (se lo ngay khi noi voi hang doi cong viec o Task 15/17).
ghi_ket_qua_cong_viec() {
  local db="$1" trang_thai="$2" nhat_ky="$3" ket_qua
  [ -n "$MA_JOB" ] || return 0
  case "$MA_JOB" in
    [0-9a-fA-F]*-*-*-*-*) : ;;
    *) ghi_log canh-bao "--ma-job '$MA_JOB' khong phai uuid -- bo qua buoc ghi bang cong viec."; return 0 ;;
  esac
  ket_qua=$(pg1 -d "$db" -c "
    INSERT INTO cong_viec_sao_luu (id, loai, trang_thai, nhat_ky, tao_luc, bat_dau_luc, ket_thuc_luc)
    VALUES ('$MA_JOB', 'phuc_hoi', '${trang_thai//\'/\'\'}', '${nhat_ky//\'/\'\'}', now(), now(), now())
    ON CONFLICT (id) DO UPDATE
      SET trang_thai = EXCLUDED.trang_thai,
          nhat_ky    = EXCLUDED.nhat_ky,
          ket_thuc_luc = EXCLUDED.ket_thuc_luc
    RETURNING id;" 2>/dev/null || true)
  if [ -z "$ket_qua" ]; then
    ghi_log canh-bao "Khong ghi duoc ket qua cong viec '$MA_JOB' vao CSDL moi -- man hinh quan tri" \
                     "co the hien sai trang thai. Viec phuc hoi VAN da hoan tat (xem nhat ky)."
  fi
}

ghi_ket_qua_dien_tap() {
  local dat="$1"
  pg -d "$(env_ung_dung POSTGRES_DB)" -c \
    "UPDATE trang_thai_sao_luu SET dien_tap_gan_nhat = now(), dien_tap_dat = $dat WHERE id = 1;" \
    >/dev/null 2>&1 || true
}

# Gianh khoa dung chung voi qlgx-runner.sh. GOI SAU buoc sao luu bat buoc, KHONG goi truoc:
# buoc do chay qlgx-runner.sh nhu mot TIEN TRINH CON, va tien trinh do cung mo/khoa chinh tep
# nay -- giu khoa truoc thi chinh ban sao luu an toan cua ta se bi tu choi (flock -n that bai).
gianh_khoa_phuc_hoi() {
  if ! command -v flock >/dev/null 2>&1; then
    ghi_log canh-bao "May khong co lenh 'flock' -- chay KHONG khoa. Bao dam khong co lan sao luu nao chay cung luc."
    return 0
  fi
  mkdir -p "$(dirname "$KHOA_RUNNER")" 2>/dev/null || true
  exec 9>"$KHOA_RUNNER" || bao_loi_va_thoat "Khong mo duoc tep khoa $KHOA_RUNNER."
  # KHAC qlgx-runner.sh: o day KHONG "bo qua lang le" khi khong gianh duoc khoa. Phuc hoi la
  # thao tac co chu dich cua nguoi van hanh -- im lang bo qua se khien ho tuong da phuc hoi xong.
  flock -n 9 \
    || bao_loi_va_thoat "Dang co mot luot sao luu/phuc hoi khac chay (giu khoa $KHOA_RUNNER). Doi no xong roi chay lai."
}
nha_khoa_phuc_hoi() { exec 9>&- 2>/dev/null || true; }

# Don sach moi thu tam cua lan chay nay. Bien tham chieu phai la bien TOAN CUC (khong `local`):
# ham nay chay tu trap, co the SAU KHI ham tao ra chung da tra ve -- luc do pham vi `local` da
# mat va `set -u` se lam trap chet ma khong don duoc gi.
don_dep_ph() {
  rm -rf "${TAM_PH:-}" 2>/dev/null || true
  rm -f "${TEP_LOI_RESTIC:-}" 2>/dev/null || true
}

phuc_hoi_that() {
  local db_hien="$1" db_moi="$2"
  kiem_ten_db_an_toan "$db_hien"; kiem_ten_db_an_toan "$db_moi"
  mkdir -p "$THU_MUC_LOG" 2>/dev/null || true
  local nhat_ky; nhat_ky="$THU_MUC_LOG/phuc-hoi-$(date '+%Y%m%d-%H%M%S').log"
  # Ghi song song ra tep TREN HOST: chinh CSDL chua bang cong viec se bi thay the o buoc 5, nen
  # nhat ky ghi trong CSDL se bien mat cung CSDL cu neu khong ghi ra ngoai.
  exec > >(tee -a "$nhat_ky") 2>&1
  ghi_log thong-tin "Nhat ky phuc hoi: $nhat_ky"

  if [ "$DIEN_TAP" -eq 1 ]; then
    # Dien tap KHONG dong toi CSDL dang phuc vu (chi tao mot CSDL rieng roi xoa), nen khong can
    # -- va khong nen -- bat cum sao luu chay them mot luot moi lan tap duot.
    ghi_log thong-tin "[1/4] DIEN TAP: bo qua buoc sao luu (khong co gi bi ghi de)."
  else
    ghi_log thong-tin "[1/7] Sao luu trang thai hien tai truoc khi ghi de (BAT BUOC)"
    sao_luu_bat_buoc "$db_hien"
  fi
  gianh_khoa_phuc_hoi

  local buoc_tong=7
  if [ "$DIEN_TAP" -eq 1 ]; then buoc_tong=4; fi
  ghi_log thong-tin "[2/$buoc_tong] Lay snapshot $SNAPSHOT tu kho restic"
  # TAM_PH la bien TOAN CUC (khong `local`): trap ben duoi co the chay SAU KHI ham nay da tra
  # ve -- luc do pham vi cua bien `local` da mat, doc lai duoi `set -u` se loi "unbound
  # variable" va trap KHONG don duoc gi (dung bai hoc da ghi trong qlgx-runner.sh).
  TAM_PH=$(mktemp -d "${TMPDIR:-/var/tmp}/qlgx-ph.XXXXXX"); chmod 700 "$TAM_PH"
  # Ban dump THO chua toan bo du lieu giao dan, chua ma hoa -- khong bao gio de no nam lai tren
  # dia. Bat ca bon tin hieu (EXIT mot minh KHONG bat SIGTERM/SIGHUP -- xem qlgx-runner.sh).
  trap don_dep_ph EXIT INT TERM HUP
  restic restore "$SNAPSHOT" --target "$TAM_PH" --include '*/qlgx-dump' --include '*/globals.sql' \
    || bao_loi_va_thoat "Khong lay duoc snapshot '$SNAPSHOT' -- he thong hien tai KHONG bi dong toi."

  ghi_log thong-tin "[3/$buoc_tong] Nap vao CSDL MOI $db_moi (CSDL dang phuc vu khong bi dong toi)"
  nap_snapshot_vao_csdl_moi "$db_moi" "$TAM_PH" \
    || bao_loi_va_thoat "Nap ban sao that bai -- he thong hien tai VAN NGUYEN VEN, khong hoan doi gi."

  ghi_log thong-tin "[4/$buoc_tong] Kiem chung CSDL moi"
  if ! kiem_chung_csdl_moi "$db_moi"; then
    xoa_db "$db_moi"
    if [ "$DIEN_TAP" -eq 1 ]; then ghi_ket_qua_dien_tap false; fi
    bao_loi_va_thoat "Kiem chung CSDL moi KHONG DAT -- da xoa CSDL tam, KHONG hoan doi. He thong hien tai nguyen ven."
  fi

  if [ "$DIEN_TAP" -eq 1 ]; then
    xoa_db "$db_moi"
    ghi_ket_qua_dien_tap true
    ghi_log thong-tin "DIEN TAP DAT: ban sao $SNAPSHOT nap va kiem chung thanh cong. Da xoa CSDL tap duot, KHONG hoan doi gi."
    return 0
  fi

  local db_luu; db_luu=$(ten_db_luu "$db_hien")
  ghi_log thong-tin "[5/7] Dung container api"
  dc stop api || ghi_log canh-bao "Khong dung duoc container api (co the no chua chay)."

  ghi_log thong-tin "[6/7] Hoan doi ten: $db_hien -> $db_luu; $db_moi -> $db_hien"
  if ! doi_ten_db "$db_hien" "$db_luu"; then
    dc up -d api || true
    bao_loi_va_thoat "Khong doi duoc ten CSDL hien tai -- KHONG hoan doi gi, he thong giu nguyen (CSDL tam $db_moi con lai, xoa tay neu khong dung)."
  fi
  if ! doi_ten_db "$db_moi" "$db_hien"; then
    ghi_log loi "Doi ten CSDL moi that bai -- tra lai ten cu NGAY."
    doi_ten_db "$db_luu" "$db_hien" \
      || bao_loi_va_thoat "NGHIEM TRONG: khong tra lai duoc ten cu. Du lieu VAN CON nguyen o CSDL '$db_luu' -- doi ten tay: ALTER DATABASE \"$db_luu\" RENAME TO \"$db_hien\";"
    dc up -d api || true
    bao_loi_va_thoat "Da tra lai trang thai cu. Khong phuc hoi duoc lan nay."
  fi

  ghi_log thong-tin "[7/7] Khoi dong api va cho san sang (toi da $CHO_SAN_SANG_SAU_HOAN_DOI_GIAY giay -- het gio la DAO NGUOC)"
  dc up -d api || ghi_log loi "Khong khoi dong duoc container api."
  if ! cho_api_san_sang "$CHO_SAN_SANG_SAU_HOAN_DOI_GIAY"; then
    ghi_log loi "He thong KHONG len duoc sau khi hoan doi -- DAO NGUOC ve CSDL cu."
    dc stop api || true
    local db_hong="${db_moi}_hong"
    if doi_ten_db "$db_hien" "$db_hong" && doi_ten_db "$db_luu" "$db_hien"; then
      dc up -d api || true
      # Duong CUU HO: o day cho rong rai (CHO_SAN_SANG_GIAY, mac dinh 300) chu khong con siet nhu
      # luc quyet dinh dao nguoc -- he thong da tro ve CSDL cu, viec con lai chi la doi no len,
      # va bao "chua len" som o day se lam nguoi van hanh hoang mang vo co.
      if cho_api_san_sang "$CHO_SAN_SANG_GIAY"; then
        ghi_log thong-tin "Da dao nguoc: he thong chay lai voi DU LIEU CU. Ban nap hong giu o '$db_hong' de xem xet."
      else
        ghi_log loi "Da dao nguoc ten CSDL nhung API van chua len -- xem: docker compose logs api"
      fi
    else
      ghi_log loi "NGHIEM TRONG: dao nguoc that bai. Du lieu cu VAN CON o CSDL '$db_luu'."
    fi
    bao_loi_va_thoat "Phuc hoi THAT BAI va da dao nguoc. Xem $nhat_ky roi thu snapshot khac."
  fi

  ghi_log thong-tin "PHUC HOI XONG. CSDL cu KHONG bi xoa -- giu nguyen ven duoi ten '$db_luu' trong $GIU_CSDL_CU_NGAY ngay."
  ghi_ket_qua_cong_viec "$db_hien" xong \
    "Phuc hoi tu snapshot $SNAPSHOT. CSDL cu: $db_luu. Nhat ky: $nhat_ky"
  # Nha khoa TRUOC khi goi qlgx-runner.sh: no tu gianh dung tep khoa nay.
  nha_khoa_phuc_hoi
  "$GOC_UNG_DUNG/scripts/qlgx-runner.sh" dong-bo-danh-sach \
    || ghi_log canh-bao "Khong dong bo duoc bang dem danh sach ban sao (khong anh huong du lieu vua phuc hoi)."
}

# Xoa cac CSDL "truoc_phuc_hoi" da qua han giu. KHONG dung `psql | while read` (vong lap chay
# trong subshell cua duong ong, moi bien dat trong do deu mat khi thoat) -- doc ra mang truoc.
don_csdl_cu() {
  local db_hien; db_hien=$(env_ung_dung POSTGRES_DB)
  local ten ngay tuoi hom_nay
  hom_nay=$(date +%s)
  local danh_sach
  danh_sach=$(pg1 -d postgres -c \
    "SELECT datname FROM pg_database WHERE datname LIKE '$(mau_db_luu "$db_hien")'" 2>/dev/null || true)
  for ten in $danh_sach; do
    ngay=$(printf '%s' "$ten" | grep -o '[0-9]\{8\}_[0-9]\{6\}' | tail -1 | cut -d_ -f1) || true
    [ -n "$ngay" ] || continue
    tuoi=$(( ( hom_nay - $(date -d "$ngay" +%s 2>/dev/null || echo "$hom_nay") ) / 86400 ))
    if [ "$tuoi" -gt "$GIU_CSDL_CU_NGAY" ]; then
      ghi_log thong-tin "Xoa CSDL cu $ten (da $tuoi ngay, qua han giu $GIU_CSDL_CU_NGAY ngay)."
      xoa_db "$ten"
    else
      ghi_log thong-tin "Giu CSDL cu $ten (moi $tuoi ngay)."
    fi
  done
}

main_phuc_hoi() {
  phan_tich_tham_so_phuc_hoi "$@"
  trap don_dep_ph EXIT INT TERM HUP
  nap_cau_hinh_ph
  [ -n "$SNAPSHOT" ] || SNAPSHOT="latest"

  if [ -n "$LAY_CAU_HINH" ]; then
    lenh_lay_cau_hinh "$LAY_CAU_HINH"
    return 0
  fi

  kiem_ung_dung_da_co
  local db_hien db_moi
  db_hien=$(env_ung_dung POSTGRES_DB)
  [ -n "$db_hien" ] || bao_loi_va_thoat "Khong doc duoc POSTGRES_DB tu $GOC_UNG_DUNG/.env."
  if [ "$DIEN_TAP" -eq 1 ]; then db_moi=$(ten_db_tam qlgx_dien_tap)
  else db_moi=$(ten_db_tam qlgx_phuc_hoi); fi

  # MAC DINH LA KHONG LAM GI. Chi in ke hoach: khong tao thu muc, khong tao CSDL, khong dung
  # container, khong ghi mot dong nao vao CSDL nao.
  if [ "$AP_DUNG" -eq 0 ] && [ "$DIEN_TAP" -eq 0 ]; then
    in_ke_hoach "$db_hien" "$db_moi"
    return 0
  fi

  phuc_hoi_that "$db_hien" "$db_moi"
  [ "$DIEN_TAP" -eq 1 ] || don_csdl_cu
}

# Cho phep bo test nap file nay ma khong chay gi.
[ "${QLGX_CHI_NAP_HAM:-0}" = "1" ] && return 0
main_phuc_hoi "$@"
