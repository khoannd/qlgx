#!/usr/bin/env bash
# Bo chay cong viec sao luu/phuc hoi cua QLGX -- chay TREN HOST, ngoai container.
#
# Vi sao ngoai container: (1) phuc hoi toan bo CSDL doi hoi ngat moi ket noi toi chinh CSDL ma
# ung dung dang dung roi doi ten no -- ung dung khong the tu lam viec do voi chinh minh;
# (2) khoa R2 nam o /etc/qlgx/backup.env chi root doc duoc, nen ung dung web bi chiem quyen
# cung khong xoa duoc ban sao luu ngoai R2.
set -euo pipefail

THU_MUC_CAU_HINH="${THU_MUC_CAU_HINH:-/etc/qlgx}"
TEP_BACKUP_ENV="${TEP_BACKUP_ENV:-$THU_MUC_CAU_HINH/backup.env}"
GIU_LAI_MAC_DINH="--keep-last 8 --keep-daily 30 --keep-weekly 12 --keep-monthly 24"
# Khoa NOI BO cua chinh qlgx-runner.sh -- KHONG dua vao flock o tang systemd unit (Task 15 co
# du dinh dat mot cai, nhung do CHI bao ve cac lan goi QUA systemd). qlgx-runner.sh duoc goi tu
# nhieu noi khac nhau: systemd timer, install.sh's cap_nhat/quay_lui goi THANG (khi admin bam
# cap nhat/quay lui), admin go tay, va sau nay hang doi cong viec cua Task 15 -- chi mot khoa NAM
# TRONG CHINH SCRIPT nay moi bao ve dong deu duoc TAT CA duong goi do. Cho phep ghi de qua bien
# moi truong de kiem chung tren may khong co /var/lock that (vd Windows qua Git Bash).
KHOA_RUNNER="${KHOA_RUNNER:-/var/lock/qlgx-runner.lock}"

nap_thu_vien_runner() {
  local d; d="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
  # shellcheck source=/dev/null
  source "$d/chung.sh"
}
nap_thu_vien_runner

nap_cau_hinh() {
  [ -r "$TEP_BACKUP_ENV" ] || bao_loi_va_thoat "Khong doc duoc $TEP_BACKUP_ENV (can quyen root)."
  # CO Y KHONG `source` nguyen tep nay (khac voi cach install.sh nap .env cua ung dung o cho
  # khac): ghi_backup_env (install.sh) ghi QLGX_GIU_LAI la MOT gia tri NHIEU TU co dau cach
  # ("--keep-last 8 --keep-daily 30 ..."), KHONG co dau nhay bao quanh. `source` cai do duoi
  # `set -euo pipefail` khien bash hieu tu thu hai tro di ("8", "--keep-daily"...) la MOT LENH
  # rieng, bao "8: command not found" va THOAT NGAY (ma 127) -- da tu kiem chung loi nay that su
  # xay ra truoc khi sua. Doc tung khoa bang doc_env_kv (an toan voi khoang trang, chi cat tai
  # dau '=' dau tien) roi tu export, khong dua ca tep qua bash source.
  local khoa gia_tri
  for khoa in RESTIC_REPOSITORY RESTIC_PASSWORD AWS_ACCESS_KEY_ID AWS_SECRET_ACCESS_KEY; do
    gia_tri="$(doc_env_kv "$TEP_BACKUP_ENV" "$khoa")"
    [ -n "$gia_tri" ] && export "$khoa=$gia_tri"
  done
  GOC_UNG_DUNG="$(doc_env_kv "$TEP_BACKUP_ENV" QLGX_GOC_UNG_DUNG)"
  GOC_UNG_DUNG="${GOC_UNG_DUNG:-/opt/qlgx/WebApp}"
  THU_MUC_SPOOL="$(doc_env_kv "$TEP_BACKUP_ENV" QLGX_THU_MUC_SPOOL)"
  THU_MUC_SPOOL="${THU_MUC_SPOOL:-/var/lib/qlgx/spool}"
  THU_MUC_LOG="$(doc_env_kv "$TEP_BACKUP_ENV" QLGX_THU_MUC_LOG)"
  THU_MUC_LOG="${THU_MUC_LOG:-/var/log/qlgx}"
  GIU_LAI="$(doc_env_kv "$TEP_BACKUP_ENV" QLGX_GIU_LAI)"
  GIU_LAI="${GIU_LAI:-$GIU_LAI_MAC_DINH}"
  mkdir -p "$THU_MUC_SPOOL" "$THU_MUC_LOG"
  # I2: loc_snapshot_json can python3. Thieu no thi buoc dong bo bang dem danh sach ban sao that
  # bai -- man hinh "Sao luu & Phuc hoi" se khong liet ke duoc ban sao nao du kho R2 day ap. Bao
  # SOM va RO o day thay vi de nguoi van hanh doan tu mot thong bao loi giua chung.
  command -v python3 >/dev/null 2>&1 \
    || ghi_log canh-bao "May nay KHONG co lenh 'python3' -- buoc cap nhat danh sach ban sao cho" \
                        "man hinh quan tri se that bai. Cai: apt-get install python3 (hoac dnf" \
                        "install python3)."
}

# Dinh nghia lai CUC BO o day (khong nap tu install.sh) de runner chay doc lap voi bo cai --
# tren VPS that, cron goi thang qlgx-runner.sh, khong dam bao install.sh con trong PATH/nap duoc.
dc() { docker compose --project-directory "$GOC_UNG_DUNG" \
         -f "$GOC_UNG_DUNG/docker-compose.yml" \
         -f "$GOC_UNG_DUNG/docker-compose.prod.yml" "$@"; }

env_ung_dung() { doc_env_kv "$GOC_UNG_DUNG/.env" "$1"; }

psql_quan_tri() {
  dc exec -T postgres psql -tAX -v ON_ERROR_STOP=1 \
    -U "$(env_ung_dung POSTGRES_USER)" -d "$(env_ung_dung POSTGRES_DB)" "$@";
}

tinh_nguon_tu_nhan() {
  case "$1" in
    truoc-cap-nhat) echo truoc_cap_nhat ;;
    truoc-phuc-hoi) echo truoc_phuc_hoi ;;
    thu-cong)       echo thu_cong ;;
    *)              echo tu_dong ;;
  esac
}

# Doc dau ra `restic snapshots --json` thanh dong TSV: id<TAB>thoi_diem<TAB>nhan<TAB>byte<TAB>
# so_giao_dan<TAB>so_gia_dinh. Dung python3 (co san tren moi ban phan phoi muc tieu) thay vi jq
# de khong them mot phu thuoc chi dung o mot cho.
loc_snapshot_json() {
  python3 - "$1" <<'PY'
import json, sys
def tag(tags, khoa, mac_dinh=""):
    for t in tags or []:
        if t.startswith(khoa + "="):
            return t.split("=", 1)[1]
    return mac_dinh
for s in json.load(open(sys.argv[1], encoding="utf-8")):
    print("\t".join([
        s.get("short_id", ""),
        s.get("time", ""),
        tag(s.get("tags"), "nhan"),
        str((s.get("summary") or {}).get("total_bytes_processed", 0) or 0),
        tag(s.get("tags"), "giao_dan", "0"),
        tag(s.get("tags"), "gia_dinh", "0"),
    ]))
PY
}

# CHU Y COT: bang ban_sao_luu/trang_thai_sao_luu dung EF Core voi quy uoc snake_case -- ten cot
# THAT trong CSDL la chu thuong snake_case (id, thoi_diem, kich_thuoc_byte, sao_luu_gan_nhat...),
# KHONG PHAI PascalCase. Postgres gap dinh danh khong dat trong ngoac kep thi tu ha chu thuong,
# nhung dinh danh CO dat trong ngoac kep ("Id") thi giu nguyen hoa/thuong va se KHONG khop cot
# that "id" -- da doi chieu truc tiep voi QlgxDbContextModelSnapshot.cs truoc khi viet cau lenh
# duoi day, khong doan.
# CHU Y ESCAPE: KHONG dung dollar-quoting ($$...$$) trong mot chuoi bash da nam trong dau
# ngoac kep -- bash tu dich "$$" thanh PID cua chinh no NGAY TRONG dau ngoac kep, khong con la
# hai dau dola that de Postgres hieu la dollar-quote nua (loi that su o ban brief dau tien).
# Dung cach an toan chuan cua SQL: nhan doi dau nhay don trong noi dung roi bao trong nhay don.
ghi_trang_thai_loi() {
  local thong_diep="${1//\'/\'\'}"
  psql_quan_tri -c "UPDATE trang_thai_sao_luu SET loi_gan_nhat = '$thong_diep' WHERE id = 1;" \
    >/dev/null 2>&1 || true
}

# Dung THAY CHO bao_loi_va_thoat o BEN TRONG lenh_sao_luu: bao_loi_va_thoat goi `exit` truc
# tiep, ma `exit` tuong minh KHONG lam `trap ... ERR` chay (da tu kiem chung) -- neu chi dua vao
# ERR trap cua lenh_sao_luu (bat loi cua LENH BEN NGOAI that bai), cac diem tu kiem tra trong
# ham (thieu toc.dat, globals.sql rong, tham so sai) se thoat MA KHONG ghi loi_gan_nhat, man
# hinh quan tri (Task 6) khong bao gio bao do o dung nhung truong hop nay. Ham nay ghi truoc roi
# moi thoat, dam bao MOI duong loi trong lenh_sao_luu deu qua duoc mot trong hai: ERR trap (lenh
# ngoai that bai) hoac ham nay (tu kiem tra that bai).
that_bai_sao_luu() {
  ghi_trang_thai_loi "$1"
  bao_loi_va_thoat "$1"
}

# Don dep dung cho lenh_sao_luu -- tach thanh ham rieng de dung LAM CHUNG cho ca trap don thu
# muc tam TREN HOST lan don thu muc dump TRONG CONTAINER, goi duoc tu nhieu tin hieu (xem ly do
# EXIT khong du o duoi). Bien tham chieu (TAM_DUMP_SAO_LUU, TEN_DUMP_CONTAINER) la bien TOAN CUC
# (khong phai `local` cua lenh_sao_luu) vi ham nay co the duoc goi tu trap SAU KHI lenh_sao_luu
# da tra ve -- neu bien la `local`, pham vi da mat, doc lai duoi `set -u` se loi "unbound
# variable" va KHONG con lam gi ca (chinh la loi that da tu kiem chung va sua trong ban truoc).
don_dep_sao_luu() {
  rm -rf "${TAM_DUMP_SAO_LUU:-}" 2>/dev/null || true
  [ -n "${TEN_DUMP_CONTAINER:-}" ] && dc exec -T postgres rm -rf "/tmp/$TEN_DUMP_CONTAINER" \
    >/dev/null 2>&1 || true
}

# Dem mot so lieu de gan vao tag snapshot. Tra ve '?' khi KHONG do duoc, KHONG tra ve 0 gia.
# (N2) Ban truoc viet `$(psql_quan_tri -c '...' | tr -d ' ' || echo 0)`: `||` gan vao CA DUONG ONG
# ma `tr` thi luon thanh cong, nen `echo 0` khong bao gio chay va psql loi cho ra chuoi RONG chu
# khong phai 0. Du the nao thi mot con so gia o day cung di thang vao tag snapshot, roi hien len
# bang doi chieu truoc-sau cua man hinh phuc hoi ("quay ve day nghia la con 0 giao dan") va lam
# nguoi van hanh tuong ban sao rong. kiem_chung_csdl_moi (qlgx-restore.sh) DA biet bo qua '?'.
dem_hoac_dau_hoi() {
  local ra
  ra=$(psql_quan_tri -c "$1" 2>/dev/null | tr -d ' \r') || ra=''
  case "$ra" in
    ''|*[!0-9]*) printf '?' ;;
    *)           printf '%s' "$ra" ;;
  esac
}

# C2/I11 -- VI SAO THAN HAM NAM TRONG MOT SUBSHELL, DOC KY TRUOC KHI BO DI:
#
# (1) Bash TAT `errexit` cho TOAN BO than mot ham khi ham do duoc goi lam ve trai cua mot danh
#     sach `||`. Duong goi tu hang doi web (lenh_chay_job: `lenh_sao_luu ... || ma_thoat=$?`) --
#     tuc la duong DUY NHAT mot quy cha thuc su dung khi bam nut "Sao luu ngay" -- chinh la mot
#     danh sach `||`. Hau qua da tu kiem chung tren bash that: `restic backup` loi thi ham VAN
#     CHAY TIEP, `restic check` loi cung VAN CHAY TIEP, va `restic forget --prune` -- lenh XOA
#     BLOB THAT trong kho -- chay tren mot kho vua bao hong. Do la vi pham truc dien rang buoc
#     cung cua thiet ke muc 5.2 ("chi don ban cu sau khi ban moi da duoc xac nhan doc duoc") va
#     rui ro mat TOAN BO lich su sao luu so sach giao xu.
#     Da thu va DEU KHONG CUU DUOC: `( set -e; f )`, `( f )`, `if ! ( f )`, `set +e; set -e` --
#     co "dang bo qua errexit" duoc ke thua qua ca subshell. Vi vay ta KHONG dua vao errexit nua:
#     MOI lenh rui ro trong thuc_hien_sao_luu deu duoc kiem ma thoat TUONG MINH bang `|| ...`.
# (2) Subshell con giai quyet I11: `that_bai_sao_luu` goi `exit`, va khi lenh_sao_luu chay CUNG
#     TIEN TRINH voi lenh_chay_job thi `exit` do giet luon ca runner -- doan xu ly mac thoat
#     (`ket_thuc_job "$ma" loi ...`) khong bao gio chay va dong cong viec ket o 'dang_chay' suot
#     2 gio, chan moi nut bam tren man hinh Sao luu & Phuc hoi. Trong subshell, `exit` chi ket
#     thuc subshell va ma thoat ve dung cho phia goi.
lenh_sao_luu() { ( thuc_hien_sao_luu "$@" ); }

thuc_hien_sao_luu() {
  # Bat truoc ca viec doc tham so: MOI duong loi trong ham nay (ke ca tham so sai) phai lam
  # trang_thai_sao_luu.loi_gan_nhat sang do -- day la o duy nhat man hinh quan tri doc de bao
  # sao luu hong (Task 6). `trap ... ERR` bat duoc LENH BEN NGOAI THAT BAI (pg_dump, restic,
  # psql,... tra ve khac 0 duoi `set -e`), nhung KHONG bat duoc `exit` tuong minh cua
  # bao_loi_va_thoat (da tu kiem chung: ERR trap khong chay khi ham goi `exit` truc tiep) --
  # vi vay cac diem goi bao_loi_va_thoat trong ham nay duoc doi sang that_bai_sao_luu o duoi,
  # ham do tu ghi loi_gan_nhat TRUOC khi thoat. Ca hai duong cong lai moi phu het "MOI duong loi".
  trap 'ghi_trang_thai_loi "sao luu that bai (lenh cuoi cung tra ve loi, xem log tren may chu de biet chi tiet)"' ERR

  local nhan="" nguon=""
  while [ $# -gt 0 ]; do
    case "$1" in
      --nhan)  nhan="$2"; shift 2 ;;
      --nguon) nguon="$2"; shift 2 ;;
      *) that_bai_sao_luu "Tham so khong hieu: $1" ;;
    esac
  done
  [ -n "$nguon" ] || nguon=$(tinh_nguon_tu_nhan "$nhan")

  # ${TMPDIR:-/var/tmp}: /var/tmp la thu muc that tren moi may Linux muc tieu (khong doi hanh
  # vi that). Chi cho phep ghi de qua TMPDIR de kiem chung tren may dev khong co /var/tmp that
  # (vd Windows qua Git Bash).
  TAM_DUMP_SAO_LUU=$(mktemp -d "${TMPDIR:-/var/tmp}/qlgx-sao-luu.XXXXXX")
  chmod 700 "$TAM_DUMP_SAO_LUU"
  # Ten thu muc dump BEN TRONG container postgres: lay chinh hau to ngau nhien cua thu muc tam
  # tren host lam hau to -- vua bao dam DUY NHAT cho moi lan chay (khong con la duong dan CO
  # DINH /tmp/qlgx-dump nhu ban truoc) nen hai lan chay CHONG NHAU (cron dem + "sao luu truoc
  # cap nhat" goi tu install.sh:521,573) khong con dung chung mot thu muc trong container --
  # vua khong can sinh them nguon ngau nhien rieng.
  TEN_DUMP_CONTAINER="qlgx-dump-$(basename "$TAM_DUMP_SAO_LUU")"
  # Ban dump THO chua toan bo du lieu giao dan, chua ma hoa -- khong bao gio de no nam lai tren
  # dia (ca ban tren host lan ban trong container). trap tren CA BON tin hieu: EXIT (thoat binh
  # thuong hoac qua `exit`/loi duoi set -e) KHONG tu bat SIGTERM (cron kill, `systemctl stop`)
  # hay SIGHUP (rot SSH) -- phai khai bao rieng INT/TERM/HUP thi bash moi chay trap khi tien
  # trinh nhan cac tin hieu do (da tu kiem chung: EXIT don thuan bo lot ca hai kich ban nay).
  trap don_dep_sao_luu EXIT INT TERM HUP

  # Don rac CUA LAN CHAY TRUOC neu co, TRONG CONTAINER, truoc khi pg_dump: neu lan truoc bi ngat
  # giua chung (mat mang, cron bi kill) va khong kip don, pg_dump -Fd se TU CHOI ghi vao thu muc
  # da ton tai va khong rong -- moi lan sao luu SAU DO se hong cho toi khi ai do vao container
  # don tay. Vi ten thu muc gio DA MANG hau to rieng cho tung lan chay, moi "qlgx-dump-*" con sot
  # lai luc nay chac chan la rac cua lan truoc (khong the la cua LAN NAY, ten LAN NAY con chua
  # sinh ra).
  dc exec -T postgres sh -c 'rm -rf /tmp/qlgx-dump-*' >/dev/null 2>&1 || true

  ghi_log thong-tin "Dump CSDL"
  # -Fd -Z0 (thu muc, KHONG tu nen) chu KHONG phai -Fc: -Fc nen san, mot byte doi o dau lam
  # toan bo luong nen doi theo -- restic khong khu trung lap duoc gi va moi snapshot luu gan nhu
  # mot ban day du moi. Anh dai dien nam trong bytea va gan nhu khong bao gio doi, nen day la
  # khac biet lon: de restic tu nen va khu trung lap theo khoi. -j4 dump song song 4 bang mot
  # luc. Xem thiet ke muc 13.3.
  # MOI lenh rui ro tu day tro xuong deu kiem ma thoat TUONG MINH (`|| that_bai_sao_luu ...`) --
  # KHONG dua vao `set -e`, vi errexit bi tat khi ham nay duoc goi tu duong hang doi web (xem
  # ghi chu dai o lenh_sao_luu).
  dc exec -T postgres pg_dump -Fd -Z0 -j4 -U "$(env_ung_dung POSTGRES_USER)" \
      -d "$(env_ung_dung POSTGRES_DB)" -f "/tmp/$TEN_DUMP_CONTAINER" \
    || that_bai_sao_luu "pg_dump that bai -- DUNG, khong sao luu va KHONG don ban cu."
  dc exec -T postgres tar -cf - -C /tmp "$TEN_DUMP_CONTAINER" | tar -xf - -C "$TAM_DUMP_SAO_LUU" \
    || that_bai_sao_luu "Khong chep duoc ban dump tu container ra host -- DUNG."
  # Doi ten lai thanh "qlgx-dump" co dinh TREN HOST (chi ten trong container la ngau nhien) de
  # phan con lai cua ham nay va lenh_tai_ve (glob `*/qlgx-dump`) khong doi.
  mv "$TAM_DUMP_SAO_LUU/$TEN_DUMP_CONTAINER" "$TAM_DUMP_SAO_LUU/qlgx-dump" \
    || that_bai_sao_luu "Khong doi duoc ten thu muc dump tren host -- DUNG."
  dc exec -T postgres rm -rf "/tmp/$TEN_DUMP_CONTAINER" || true
  [ -s "$TAM_DUMP_SAO_LUU/qlgx-dump/toc.dat" ] \
    || that_bai_sao_luu "pg_dump khong tao ra muc luc (toc.dat) -- DUNG, khong sao luu."

  ghi_log thong-tin "Dump vai tro va quyen toan cuc"
  dc exec -T postgres pg_dumpall --globals-only -U "$(env_ung_dung POSTGRES_USER)" \
      > "$TAM_DUMP_SAO_LUU/globals.sql" \
    || that_bai_sao_luu "pg_dumpall --globals-only that bai -- DUNG."
  [ -s "$TAM_DUMP_SAO_LUU/globals.sql" ] \
    || that_bai_sao_luu "pg_dumpall --globals-only tao ra tep rong -- DUNG."

  ghi_log thong-tin "Gom tep cau hinh"
  mkdir -p "$TAM_DUMP_SAO_LUU/cau-hinh" \
    || that_bai_sao_luu "Khong tao duoc thu muc cau-hinh trong ban sao -- DUNG."
  for f in .env docker-compose.yml docker-compose.prod.yml Caddyfile; do
    if [ -f "$GOC_UNG_DUNG/$f" ]; then
      cp "$GOC_UNG_DUNG/$f" "$TAM_DUMP_SAO_LUU/cau-hinh/" \
        || that_bai_sao_luu "Khong chep duoc '$f' vao ban sao -- DUNG."
    fi
  done
  # I1 -- CANH GAC CHO THANH PHAN "cau hinh". Thiet ke muc 5.2: mot snapshot chi hop le khi ca BA
  # thanh phan (db, globals, cau hinh) cung vao; "tha khong co snapshot con hon co snapshot
  # thieu". `db` da co canh gac (toc.dat) va `globals` cung co ([ -s ]), rieng vong lap chep cau
  # hinh o tren thi khong: neu QLGX_GOC_UNG_DUNG trong backup.env tro sai duong dan (vd sau khi
  # doi thu muc cai dat), moi `[ -f ]` deu sai, vong lap chep DUNG KHONG TEP NAO, va snapshot van
  # duoc day len binh thuong voi thu muc cau-hinh RONG. Hong chi lo ra 6 thang sau, giua mot cuoc
  # cuu ho: `--lay-cau-hinh` bao "Snapshot khong chua thu muc cau-hinh", va mat .env nghia la mat
  # mat khau ba vai tro CSDL -- ban dump vua phuc hoi khong khop quyen voi he thong moi cai.
  [ -s "$TAM_DUMP_SAO_LUU/cau-hinh/.env" ] \
    || that_bai_sao_luu "Khong gom duoc .env cua ung dung vao ban sao (tim tai '$GOC_UNG_DUNG')" \
                        "-- DUNG, khong day len mot snapshot THIEU cau hinh. Kiem tra" \
                        "QLGX_GOC_UNG_DUNG trong $TEP_BACKUP_ENV."
  # /etc/qlgx/backup.env CO Y khong nam trong ban sao luu: khong tu ma hoa mot tep bang chinh
  # khoa chua trong tep do. Do la ly do phai co The phuc hoi cat NGOAI may chu.

  # Dem so lieu de gan vao tag -- man hinh phuc hoi hien "quay ve day nghia la con 2050 giao dan".
  local so_gd so_gdinh
  so_gd=$(dem_hoac_dau_hoi 'SELECT count(*) FROM giao_dan')
  so_gdinh=$(dem_hoac_dau_hoi 'SELECT count(*) FROM gia_dinh')

  ghi_log thong-tin "Day len kho restic"
  restic backup "$TAM_DUMP_SAO_LUU" \
    --tag "nhan=${nhan:-tu-dong}" --tag "nguon=$nguon" \
    --tag "giao_dan=$so_gd" --tag "gia_dinh=$so_gdinh" \
    --host qlgx \
    || that_bai_sao_luu "restic backup THAT BAI (mat mang, R2 tu choi khoa, kho khong mo duoc?)" \
                        "-- DUNG. TUYET DOI khong chay 'restic forget --prune' sau mot lan backup" \
                        "that bai: ban cu la thu duy nhat con lai."

  # Da day len kho restic xong -- xoa NGAY anh dump tho tren dia thay vi cho toi khi ham ket
  # thuc hay tien trinh thoat. Giam toi thieu thoi gian du lieu CHUA MA HOA ton tai tren dia.
  # (Thu muc ben trong container da duoc xoa ngay sau buoc tar o tren.)
  rm -rf "$TAM_DUMP_SAO_LUU"
  trap - EXIT INT TERM HUP

  ghi_log thong-tin "Kiem tra toan ven truoc khi don ban cu"
  restic check --read-data-subset=5% \
    || that_bai_sao_luu "restic check THAT BAI -- kho co the dang hong. DUNG, KHONG chay" \
                        "'forget --prune'. Chay 'restic check --read-data' tay de xem xet truoc" \
                        "khi xoa bat cu thu gi."

  # THU TU CUNG: backup -> check -> forget. Khong bao gio xoa ban cu truoc khi ban moi duoc
  # xac nhan doc duoc. Rang buoc that su o day KHONG phai thu tu dong ma, ma la NHAN QUA: hai
  # lenh tren PHAI thanh cong thi dong duoi moi duoc chay -- xem hai `|| that_bai_sao_luu` tren.
  ghi_log thong-tin "Don ban cu theo chinh sach giu"
  # --group-by host (KHONG con "host,paths" mac dinh cua restic): moi lan chay dung mot thu muc
  # tam MOI (mktemp sinh hau to ngau nhien khac nhau) nen truong `paths` trong snapshot KHAC NHAU
  # MOI LAN -- neu de restic tu nhom theo ca "paths" nhu mac dinh, MOI snapshot roi vao MOT nhom
  # rieng chi co dung 1 phan tu, va --keep-last/--keep-daily/... se "giu" TAT CA vi moi nhom chi
  # co 1 thanh vien de giu -- chinh sach giu ban COI NHU KHONG TON TAI, kho R2 phinh vo han. Moi
  # snapshot QLGX deu cung MOT "host" logic (host cung dinh "qlgx" o lenh backup phia tren) du
  # duong dan tam khac nhau, nen nhom theo host la dung.
  # shellcheck disable=SC2086
  restic forget $GIU_LAI --group-by host --prune \
    || that_bai_sao_luu "restic forget --prune that bai -- ban sao MOI da len kho an toan, chi la" \
                        "buoc don ban cu chua xong. Kho co the con khoa sot lai."

  lenh_dong_bo_danh_sach \
    || that_bai_sao_luu "Khong dong bo duoc bang dem danh sach ban sao -- ban sao VAN da len kho" \
                        "an toan, nhung man hinh quan tri se hien thieu."
  psql_quan_tri -c "UPDATE trang_thai_sao_luu
                    SET sao_luu_gan_nhat = now(), loi_gan_nhat = NULL WHERE id = 1;" >/dev/null \
    || that_bai_sao_luu "Khong ghi duoc trang thai 'sao luu xong' vao CSDL."
  ghi_log thong-tin "Sao luu xong ($so_gd giao dan, $so_gdinh gia dinh)."
  trap - ERR
}

lenh_kiem_tra() {
  ghi_log thong-tin "restic check --read-data-subset=5%"
  if restic check --read-data-subset=5%; then
    psql_quan_tri -c "UPDATE trang_thai_sao_luu SET loi_gan_nhat = NULL WHERE id = 1;" >/dev/null
  else
    ghi_trang_thai_loi "restic check that bai"
    return 1
  fi
}

# "dem-snapshot-nhan <nhan>" -- in ra SO snapshot trong kho restic mang dung tag "nhan=<nhan>".
# Them subcommand nay (vong review cuoi cung, Finding 2) de LAM MOT NOI DUY NHAT biet cach doc
# ban dem restic dung KHOA THAT (qua nap_cau_hinh o tren, an toan voi QLGX_GIU_LAI nhieu tu -- xem
# ghi chu o nap_cau_hinh), thay vi de tung noi can "co that su sao luu moi khong" (install.sh's
# cap_nhat, qlgx-restore.sh's sao_luu_bat_buoc) tu doc backup.env/goi restic rieng va co nguy co
# mot ban trong so do troi ra khoi ban da hardened (dung loai loi da tung xay ra: sao_luu_bat_buoc
# da duoc vong review truoc va Task 14 sua ky, nhung install.sh's cap_nhat lai chua duoc sua theo
# vi no dong TRUOC khi loi do duoc phat hien).
#
# --no-lock: day la truy van CHI DOC, chay CO THE trung luc mot lenh sao-luu/kiem-tra/forget khac
# dang giu khoa GHI tren kho -- neu khong co --no-lock, restic tu choi ngay voi "repository is
# already locked" (retry-lock mac dinh la 0), gioi han nay da gap that trong qlgx-restore.sh (xem
# restic_doc o do). --no-lock bo qua viec xin khoa, an toan cho lenh chi doc.
#
# CO Y de restic TU BAO LOI (khong nuot bang `|| true` o day) neu chinh no khong mo duoc kho (sai
# khoa, mat mang, kho khong ton tai) -- nguoc lai mot cau lenh dem "0 snapshot" se khong phan biet
# duoc voi "kho hong hoan toan khong doc duoc gi", va noi goi (install.sh's cap_nhat) se tuong nham
# "dem duoc 0, dem lai van 0, khong tang -> DUNG cap nhat" trong khi ly do that la kho khong mo
# duoc tu dau -- van la DUNG dung (an toan), nhung thong diep loi se sai, gay kho gian khi go loi.
lenh_dem_snapshot_nhan() {
  local nhan="${1:-}"
  [ -n "$nhan" ] || bao_loi_va_thoat "dem-snapshot-nhan can mot tham so <nhan>."
  local json
  json=$(restic snapshots --json --no-lock) \
    || bao_loi_va_thoat "Khong doc duoc danh sach snapshot tu kho restic (kho khong mo duoc," \
                        "sai khoa, hoac mat mang) -- xem log restic o tren."
  local so
  so=$(printf '%s' "$json" | grep -o "nhan=${nhan}\"" | grep -c . || true)
  printf '%s\n' "${so:-0}"
}

# I2 -- VI SAO SINH SQL RA TEP TRUOC ROI MOI GUI CHO psql:
# Ban truoc gui thang mot nhom `{ echo BEGIN; echo DELETE; loc_snapshot_json | while ...; echo
# COMMIT; } | psql -f -`. Ma thoat cua mot NHOM la ma thoat cua lenh CUOI CUNG (`echo "COMMIT;"`,
# luon bang 0), va `pipefail` khong cuu duoc vi loi nam BEN TRONG nhom chu khong o ve cuoi cua
# duong ong ngoai. Neu loc_snapshot_json chet (thieu python3 tren mot ban phan phoi toi gian,
# restic doi dinh dang JSON o ban moi, restic in canh bao lan vao dau ra) thi cau lenh gui toi
# psql con dung ba dong: BEGIN; DELETE FROM ban_sao_luu; COMMIT; -- MOI lan sao luu THANH CONG lai
# XOA SACH bang dem danh sach ban sao. Man hinh "Sao luu & Phuc hoi" hien "khong co ban sao nao"
# trong khi kho R2 day ap, va dung luc khan cap thi khong chon duoc snapshot nao de phuc hoi.
# Hai test bats tung do vi thieu python3 chinh la bang chung loi nay xay ra that.
# "dem-snapshot" -- in ra TONG so snapshot trong kho, khong loc nhan.
# Thiet ke muc 4.5 doi bang tu kiem chung khang dinh "kho restic mo duoc VA co >= 1 snapshot".
# Chi kiem "mo duoc" thi mot may chu da chay ba thang ma chua he co ban sao nao (timer bi tat, R2
# tu choi ghi) van cho `qlgx status` toan DAT -- he thong bao thanh cong ve dung thu no khong lam.
lenh_dem_snapshot() {
  local json
  json=$(restic snapshots --json --no-lock) \
    || bao_loi_va_thoat "Khong doc duoc danh sach snapshot tu kho restic (kho khong mo duoc," \
                        "sai khoa, hoac mat mang)."
  local so
  so=$(printf '%s' "$json" | grep -o '"short_id"' | grep -c . || true)
  printf '%s\n' "${so:-0}"
}

# "id-snapshot-nhan <nhan>" -- in ra id cua snapshot MOI NHAT mang tag "nhan=<nhan>".
# restic in danh sach theo thu tu thoi gian tang dan nen ban cuoi cung la ban moi nhat. CO Y
# KHONG dung `--latest 1`: no nhom theo (host, PATHS) va moi lan sao luu dung mot thu muc tam
# khac nhau, nen moi snapshot roi vao mot nhom rieng va `head -1` nhat phai ban CU (bay nay da
# duoc ghi lai ky trong qlgx-restore.sh, xem json_toan_bo_snapshot o do).
# Dung cho install.sh de in ID ban sao 'truoc-cap-nhat' trong cau huong dan phuc hoi sau khi quay
# lui -- dung thu nguoi van hanh can nhat khi dang hoang.
lenh_id_snapshot_nhan() {
  local nhan="${1:-}"
  [ -n "$nhan" ] || bao_loi_va_thoat "id-snapshot-nhan can mot tham so <nhan>."
  local json
  json=$(restic snapshots --json --no-lock --tag "nhan=$nhan") \
    || bao_loi_va_thoat "Khong doc duoc danh sach snapshot tu kho restic."
  printf '%s' "$json" | grep -o '"short_id":"[0-9a-f]*"' | tail -1 | cut -d'"' -f4
}

# "khoi-tao-kho" -- mo kho restic, CHUA CO thi tao (restic init).
#
# `restic backup` KHONG tu khoi tao kho: voi mot bucket R2 moi tinh no dung lai voi "unable to
# open config file ... Is there a repository at the following location?". Truoc day khong mot
# script nao trong bo goi `restic init`, nen buoc "chay sao luu dau tien de chung minh duong ong
# song that" cua install.sh THAT BAI o MOI lan cai moi.
#
# Neu kho DA ton tai nhung mat khau SAI (kich ban cuu ho: may moi sinh RESTIC_PASSWORD moi trong
# khi kho cu la kho cu), `restic cat config` that bai va `restic init` cung se tu choi ghi de --
# bao ro rang bang tieng Viet thay vi de nguoi van hanh doc mot loi tieng Anh kho hieu.
lenh_khoi_tao_kho() {
  if restic cat config >/dev/null 2>&1; then
    ghi_log thong-tin "Kho restic da ton tai va mo duoc bang mat khau hien co."
    return 0
  fi
  ghi_log thong-tin "Chua mo duoc kho restic -- thu khoi tao kho moi (restic init)."
  if restic init; then
    ghi_log thong-tin "Da khoi tao kho restic moi tai $RESTIC_REPOSITORY."
    return 0
  fi
  bao_loi_va_thoat "Khong mo duoc kho restic va cung khong tao moi duoc." \
    "Hai nguyen nhan thuong gap:" \
    "(1) Kho o dia chi nay DA TON TAI nhung MAT KHAU RESTIC khong dung -- neu may nay dang cuu ho" \
    "mot giao xu cu, hay nhap lai dung MAT KHAU RESTIC ghi tren The phuc hoi (sua" \
    "RESTIC_PASSWORD trong $TEP_BACKUP_ENV roi chay lai)." \
    "(2) Khoa R2 sai hoac bucket khong ton tai -- kiem tra RESTIC_REPOSITORY/AWS_* trong" \
    "$TEP_BACKUP_ENV."
}

lenh_dong_bo_danh_sach() {
  local tam; tam=$(mktemp)
  # Tu huy trap NGAY khi no chay: `trap ... RETURN` cua bash KHONG tu dong het hieu luc sau
  # LAN TRA VE DAU TIEN -- no o LAI va se chay LAI o moi lan tra ve tiep theo cua BAT KY ham
  # nao khac trong cung shell. Ham nay thuong duoc GOI LONG ben trong lenh_sao_luu (bien $tam
  # cung ten nhung khac o do) -- neu khong tu huy, trap se chay LAN NUA luc lenh_sao_luu tra
  # ve va dung nham bien $tam CUA lenh_sao_luu (thu muc dump, khong phai tep json nay), gay
  # loi that "rm: cannot remove ...: Is a directory" -- da tu kiem chung loi nay that su xay ra
  # (bang mot ham bash toi gian rieng) truoc khi sua bang cach tu go trap ngay trong than trap.
  local tsv sql; tsv=$(mktemp); sql=$(mktemp)
  trap 'rm -f "$tam" "$tsv" "$sql"; trap - RETURN' RETURN
  restic snapshots --json > "$tam" \
    || { ghi_log loi "Khong doc duoc danh sach snapshot tu kho restic -- KHONG dung toi bang dem."; return 1; }
  # Loc TRUOC va kiem ma thoat TUONG MINH: chi khi buoc nay thanh cong thi moi duoc phep DELETE.
  loc_snapshot_json "$tam" > "$tsv" \
    || { ghi_log loi "Khong phan tich duoc danh sach snapshot (thieu python3, hoac restic doi" \
                     "dinh dang JSON?) -- KHONG dung toi bang dem danh sach ban sao."; return 1; }
  # Nap lai TOAN BO bang dem trong mot giao dich -- don gian va luon dung, so snapshot chi vai
  # chuc dong nen khong can dong bo tang phan.
  {
    echo "BEGIN;"
    echo "DELETE FROM ban_sao_luu;"
    while IFS=$'\t' read -r id thoi_diem nhan byte gd gdinh; do
      # Nhan doi dau nhay don TRONG NOI DUNG truoc khi bao vao nhay don SQL -- giong het cach
      # ghi_trang_thai_loi da lam o tren, ap dung LAI o day cho NHAT QUAN. `nhan` la noi dung tag
      # restic ma quan tri vien co the dat tuy y qua `--nhan` (ke ca goi tu giao dien qua Task
      # 6/17) -- ban truoc thieu buoc nay: mot nhan nhu "Truoc le Cha So's" da du pha vo cau
      # lenh SQL, co y thi la chen SQL chay duoi quyen chu CSDL qua `psql -f -`. id/thoi_diem lay
      # tu chinh restic (khong phai dau vao nguoi dung) nhung van escape cho chac, dong bo/nguon
      # suy tu enum co dinh (tinh_nguon_tu_nhan) nen von an toan nhung van escape cho nhat quan.
      local id_e="${id//\'/\'\'}" thoi_diem_e="${thoi_diem//\'/\'\'}" nhan_e="${nhan//\'/\'\'}"
      local nguon_e; nguon_e="$(tinh_nguon_tu_nhan "$nhan")"; nguon_e="${nguon_e//\'/\'\'}"
      # Ba cot so di THANG vao cau lenh (khong bao nhay don) nen phai la SO THAT. Tag snapshot co
      # the mang '?' khi luc sao luu khong dem duoc (xem dem_hoac_dau_hoi) -- de nguyen thi cau
      # lenh SQL hong va ca lan dong bo that bai. Ve 0 o BANG DEM (tag trong kho van giu '?', noi
      # kiem_chung_csdl_moi biet cach bo qua dung cach).
      case "${byte:-0}"  in ''|*[!0-9]*) byte=0 ;;  esac
      case "${gd:-0}"    in ''|*[!0-9]*) gd=0 ;;    esac
      case "${gdinh:-0}" in ''|*[!0-9]*) gdinh=0 ;; esac
      printf "INSERT INTO ban_sao_luu (id,thoi_diem,nhan,kich_thuoc_byte,so_giao_dan,so_gia_dinh,nguon) VALUES ('%s','%s',%s,%s,%s,%s,'%s');\n" \
        "$id_e" "$thoi_diem_e" "$([ -n "$nhan_e" ] && printf "'%s'" "$nhan_e" || echo NULL)" \
        "$byte" "$gd" "$gdinh" "$nguon_e"
    done < "$tsv"
    echo "COMMIT;"
  } > "$sql"
  psql_quan_tri -f "$sql" >/dev/null \
    || { ghi_log loi "Khong ghi duoc bang dem danh sach ban sao vao CSDL."; return 1; }
  ghi_log thong-tin "Da dong bo bang dem danh sach ban sao."
}

lenh_tai_ve() {
  local snapshot="$1" ma_job="$2"
  local dich="$THU_MUC_SPOOL/$ma_job"
  # I8 -- tep trong thu muc nay la ban dump THO, CHUA MA HOA: ho ten, ngay sinh, so can cuoc cua
  # hang nghin giao dan, dung thu thiet ke muc 1.2 diem 3 dat lam muc tieu bao mat. Quyen 755/644
  # (ban truoc) cho MOI nguoi dung tren may doc duoc trong suot 24 gio. Quyen 755 duoc dat de
  # container API doc duoc qua mount chi-doc; cach dung la cap quyen theo NHOM chu khong phai cho
  # "other". Neu khong kiem soat duoc GID cua container thi 751 (duyet duoc nhung KHONG liet ke
  # duoc) + ten tep kho doan van kin hon han 755.
  mkdir -p "$dich"
  chmod 751 "$dich"
  if [ -n "${QLGX_GID_API:-}" ]; then
    chgrp "$QLGX_GID_API" "$dich" 2>/dev/null && chmod 750 "$dich" 2>/dev/null || true
  fi
  restic restore "$snapshot" --target "$dich" --include '*/qlgx-dump'
  local thu_muc; thu_muc=$(find "$dich" -type d -name qlgx-dump | head -1)
  [ -n "$thu_muc" ] || bao_loi_va_thoat "Khong tim thay thu muc qlgx-dump trong snapshot $snapshot."
  # Dump la mot THU MUC (-Fd) -- dong goi thanh MOT tep de trinh duyet tai ve duoc. Nen bang gzip
  # o day (khac voi luc sao luu, noi ta co y KHONG nen de restic khu trung lap).
  local tep; tep="$dich/qlgx-$snapshot-$(date +%Y%m%d-%H%M).dump.tar.gz"
  tar -czf "$tep" -C "$(dirname "$thu_muc")" qlgx-dump
  rm -rf "$thu_muc"
  chmod 644 "$tep"
  ghi_log thong-tin "Da chuan bi tep tai ve tai $dich"
}

lenh_don_spool() {
  # Tep tai ve la ban dump CHUA MA HOA -- chi giu 24 gio.
  find "$THU_MUC_SPOOL" -mindepth 1 -maxdepth 1 -type d -mmin +1440 -exec rm -rf {} + 2>/dev/null || true
}

# Gianh khoa file KHONG CHO (flock -n) truoc khi chay mot lenh thay doi trang thai dung chung
# (kho restic, bang ban_sao_luu/trang_thai_sao_luu, va rieng sao-luu con ghi truc tiep vao he
# thong tep cua container postgres qua duong dan /tmp/qlgx-dump-*). Neu KHONG gianh duoc khoa --
# tuc la dang co mot luot khac chay -- THOAT NGAY VOI MA 0 (day la hanh vi MONG DOI khi hai lan
# goi trung thoi diem, KHONG PHAI loi): vi du cron dem dang chay sao-luu dung luc admin bam "cap
# nhat" (install.sh's cap_nhat goi lenh_sao_luu truc tiep, khong qua systemd) -- neu khong co
# khoa nay, ca hai se cung dung wildcard `/tmp/qlgx-dump-*` va ghi de/xoa dump cua nhau giua
# chung (da tung la mot hong that phat hien o vong review truoc, ngay sau khi them buoc don rac
# wildcard de sua Loi 2). `exec 9>"$KHOA_RUNNER"` mo/giu file descriptor 9 SUOT DOI tien trinh --
# flock tu nha khi tien trinh thoat (fd dong), khong can don tay o duong thanh cong lan duong loi.
#
# tai-ve/don-spool KHONG khoa: tai-ve thao tac tren thu muc RIENG theo ma job (khong dung chung
# duong dan voi bat ky lenh nao khac), don-spool chi xoa thu muc da qua 24 gio (khong dung nam
# giua mot lan tai-ve dang chay vi thu muc do luon con "tre" hon 24 gio) -- ca hai it rui ro
# giam dap hon nhieu so voi sao-luu/kiem-tra/dong-bo-danh-sach nen khong can them khoa.
gianh_khoa_hoac_bo_qua() {
  local ten_lenh="$1"
  mkdir -p "$(dirname "$KHOA_RUNNER")" 2>/dev/null || true
  exec 9>"$KHOA_RUNNER" || bao_loi_va_thoat "Khong mo duoc tep khoa $KHOA_RUNNER."
  if ! flock -n 9; then
    ghi_log canh-bao "Dang co mot luot '$ten_lenh' khac chay (giu khoa $KHOA_RUNNER) -- bo qua luot nay (khong phai loi)."
    exit 0
  fi
}

# ---------------------------------------------------------------------------------------------
# VONG LAY CONG VIEC -- noi bang cong_viec_sao_luu (nut bam tren giao dien web) voi cac lenh o
# tren. Giao dien web CHI biet INSERT mot dong vao bang; moi viec that xay ra o day.
# ---------------------------------------------------------------------------------------------

# Bao lau khong co tien trien thi coi mot cong viec 'dang_chay' la MO COI. 2 gio: du dai de mot
# luot sao luu/phuc hoi that hoan tat tren may chu cham nhat, du ngan de khong de lo vo thoi han.
GIO_JOB_QUA_HAN="${QLGX_GIO_JOB_QUA_HAN:-2}"
# Bao lau mot cong viec nam o 'cho' MA KHONG AI NHAN thi coi la bo chay khong hoat dong.
# PHAI trung voi SaoLuuService.PhutChoToiDa (C#) -- xem giai thich o sql_don_job_mo_coi.
PHUT_JOB_CHO_QUA_HAN="${QLGX_PHUT_JOB_CHO_QUA_HAN:-15}"

# CHU Y COT: ten cot THAT trong CSDL la snake_case chu thuong (id, loai, trang_thai,
# tham_so_json, tao_luc, bat_dau_luc, ket_thuc_luc, nhat_ky) -- da doi chieu truc tiep voi
# Migrations/20260913071238_ThemBangSaoLuu.cs truoc khi viet, khong doan. Dinh danh dat trong
# ngoac kep kieu "TrangThai" se KHONG khop cot that va cau lenh se loi ngay.
#
# FOR UPDATE SKIP LOCKED bao dam hai luot chay chong nhau (timer no khi luot truoc con chay)
# khong the lay TRUNG mot job: luot thu hai bo qua dong da bi khoa thay vi cho, va vi ta LIMIT 1
# nen no don gian khong lay duoc gi va thoat. Dieu kien "AND trang_thai = 'cho'" o menh de WHERE
# NGOAI la lop chan thu hai: neu mot tien trinh khac vua commit xong viec gianh dung dong do
# ngay giua hai buoc, UPDATE nay khop 0 dong thay vi cuop job dang chay cua ho.
#
# replace(..., E'\n', ' '): phia goi doc ket qua bang `head -1` + `IFS=$'\t' read`, nen mot
# tham_so_json lo co xuong dong se cat mat phan duoi. Ep ve mot dong ngay trong SQL.
#
# VI SAO BOC TRONG "WITH ... SELECT" chu khong UPDATE ... RETURNING tran (da tu kiem chung tren
# mot Postgres that, KHONG doan): `psql -tA -c "UPDATE ... RETURNING ..."` in ra CA cac dong
# RETURNING LAN dong the lenh "UPDATE 1"/"UPDATE 0" o cuoi -- che do tuples-only chi bo tieu de
# cot, khong bo the lenh. Hau qua that: khi hang doi RONG, dau ra khong phai chuoi rong ma la
# "UPDATE 0", nen `[ -n "$dong" ]` van dung va bo chay lao vao xu ly mot "cong viec" ma ma so la
# chuoi "UPDATE 0" -- moi phut mot lan, mai mai. Boc trong mot CTE roi SELECT thi lenh CUOI la
# SELECT, psql in dung cac dong (khong co dong nao thi khong in gi).
sql_gianh_job() {
  cat <<'SQL'
WITH gianh AS (
  UPDATE cong_viec_sao_luu SET trang_thai = 'dang_chay', bat_dau_luc = now()
  WHERE id = (SELECT id FROM cong_viec_sao_luu WHERE trang_thai = 'cho'
              ORDER BY tao_luc LIMIT 1 FOR UPDATE SKIP LOCKED)
    AND trang_thai = 'cho'
  RETURNING id, loai, tham_so_json)
SELECT id::text || E'\t' || loai || E'\t'
       || replace(coalesce(tham_so_json, '{}'), E'\n', ' ')
FROM gianh;
SQL
}

# VI SAO HAM NAY TON TAI -- doc ky truoc khi bo di:
# Guard "khong xep hang hai cong viec chong nhau" trong SaoLuuService.TaoCongViec (C#) coi BAT KY
# dong nao dang o 'cho' HOAC 'dang_chay' la du de TU CHOI tao cong viec moi. Neu bo chay tren host
# chet giua chung (mat dien, kill -9, OOM, systemd het TimeoutStartSec) thi mot dong 'dang_chay'
# se nam lai VINH VIEN, va tu do tro di khong ai tao duoc cong viec sao luu hay phuc hoi nao nua --
# khong co nut bam nao sua duoc, chi co vao thang CSDL go tay, ma nguoi van hanh that (quy cha,
# quy so) khong tu lam duoc. Do la mot cach mat du lieu so sach giao xu am tham: he thong tu bao
# "dang ban" trong nhieu thang.
# Vi vay: moi luot chay-job (systemd timer goi moi phut) tu quet va danh dau 'loi' cho cac dong
# ket qua han, KEM nhat ky giai thich de nguoi van hanh doc duoc tren giao dien.
sql_don_job_mo_coi() {
  local gio="${1:-$GIO_JOB_QUA_HAN}"
  # So gio di THANG vao cau lenh SQL -- chi chap nhan so nguyen, moi thu khac ve mac dinh 2.
  case "$gio" in ''|*[!0-9]*) gio=2 ;; esac
  [ "$gio" -gt 0 ] 2>/dev/null || gio=2
  local phut_cho="${2:-$PHUT_JOB_CHO_QUA_HAN}"
  case "$phut_cho" in ''|*[!0-9]*) phut_cho=15 ;; esac
  [ "$phut_cho" -gt 0 ] 2>/dev/null || phut_cho=15
  # Boc CTE vi dung ly do da giai thich o sql_gianh_job: UPDATE tran lam psql in them dong
  # "UPDATE 0" va bien "khong co job mo coi nao" thanh "co mot job mo coi ten 'UPDATE 0'" --
  # cu the hon, no lam ghi_trang_thai_loi chay MOI PHUT va man hinh quan tri do vinh vien.
  # HAI loai job mo coi, HAI nguyen nhan KHAC HAN nhau -- nen hai nguong va hai cau nhat ky rieng:
  #
  # (a) 'dang_chay' qua $gio gio: bo chay DA nhan viec roi chet giua chung (mat dien, kill -9,
  #     OOM). 2 gio du dai cho mot luot sao luu/phuc hoi that tren may chu cham nhat.
  #
  # (b) 'cho' qua $phut_cho phut: KHONG AI NHAN viec ca -- tuc la bo chay tren host khong hoat
  #     dong (nguoi cai tra loi "khong" o cau hoi bat sao luu tu dong, hoac dich vu da dung).
  #     Ban truoc CHI quet 'dang_chay', nen mot dong 'cho' khong ai nhat nam do VINH VIEN, va
  #     guard "khong xep hang hai cong viec chong nhau" cua SaoLuuService (C#) tu do TU CHOI MOI
  #     thao tac tren man hinh Sao luu & Phuc hoi. Khong co route nao huy cong viec, nen duong
  #     thoat duy nhat la SSH vao may chu chay psql -- dung thu quy cha, quy so khong lam duoc.
  #     15 phut lay DUNG tu SaoLuuService.PhutChoToiDa (C#) -- hai phia phai cung mot con so, neu
  #     khong se co khoang thoi gian mot ben coi la mo coi con ben kia van chan.
  cat <<SQL
WITH don AS (
  UPDATE cong_viec_sao_luu
  SET trang_thai = 'loi', ket_thuc_luc = now(),
      nhat_ky = coalesce(nhat_ky || E'\n', '')
                || CASE WHEN trang_thai = 'dang_chay'
                        THEN 'cong viec bi bo do, tu dong danh dau loi sau $gio gio khong co tien trien'
                        ELSE 'khong duoc bo chay sao luu tren may chu nhan sau $phut_cho phut nen da bi danh dau loi. Nguyen nhan thuong gap: bo chay sao luu tren may chu khong hoat dong (chua bat sao luu tu dong luc cai dat, hoac dich vu da dung).'
                   END
  WHERE (trang_thai = 'dang_chay'
         AND coalesce(bat_dau_luc, tao_luc) < now() - make_interval(hours => $gio))
     OR (trang_thai = 'cho'
         AND tao_luc < now() - make_interval(mins => $phut_cho))
  RETURNING id)
SELECT id::text FROM don;
SQL
}

la_uuid() { case "$1" in [0-9a-fA-F]*-*-*-*-*) return 0 ;; *) return 1 ;; esac; }

don_job_mo_coi() {
  local ket_qua ma so=0
  ket_qua=$(psql_quan_tri -c "$(sql_don_job_mo_coi)" 2>/dev/null || true)
  [ -n "$ket_qua" ] || return 0
  while IFS= read -r ma; do
    # Chi dem/bao nhung dong THAT SU la ma cong viec. Mot dong rac (loi psql, the lenh) khong
    # duoc phep lam man hinh quan tri do len -- xem ghi chu ve "UPDATE 0" o sql_gianh_job.
    la_uuid "$ma" || continue
    so=$((so + 1))
    ghi_log canh-bao "Cong viec $ma bi bo do (ket o 'dang_chay' qua $GIO_JOB_QUA_HAN gio, hoac o" \
                     "'cho' qua $PHUT_JOB_CHO_QUA_HAN phut ma khong ai nhan) -- tu dong danh dau" \
                     "'loi' de hang doi khong bi chan vinh vien."
  done <<< "$ket_qua"
  [ "$so" -gt 0 ] || return 0
  ghi_trang_thai_loi "Co cong viec sao luu bi bo do giua chung (bo chay bi ngat?), da tu dong danh dau loi."
}

# Doc snapshotId tu tham_so_json. CO Y khong dung python3/jq: gia tri nay di THANG vao dong lenh
# restic va qlgx-restore.sh, nen buoc quan trong khong phai la "phan tich JSON that chuan" ma la
# CHI CHO QUA mot ma snapshot hop le (chu va so). Moi thu khac -- dau nhay, dau cach, ';', JSON
# hong, khoa vang mat, gia tri null -- deu tra ve chuoi rong, va phia goi coi do la loi tham so.
doc_snapshot_tu_tham_so() {
  local json="${1:-}" gia_tri=""
  gia_tri=$(printf '%s' "$json" \
    | grep -o '"snapshotId"[[:space:]]*:[[:space:]]*"[^"]*"' | head -1 || true)
  gia_tri="${gia_tri%\"}"    # bo dau nhay dong cuoi
  gia_tri="${gia_tri##*\"}"  # bo moi thu toi dau nhay mo cua gia tri
  case "$gia_tri" in
    ''|*[!A-Za-z0-9]*) printf '' ;;
    *)                 printf '%s' "$gia_tri" ;;
  esac
}

# ket_thuc_job <ma> <trang_thai> <nhat_ky>
# Escape theo dung cach da dung o ghi_trang_thai_loi: nhan doi dau nhay don roi bao trong nhay
# don. TUYET DOI khong dung dollar-quoting ($$...$$) trong chuoi bash co ngoac kep -- bash doi
# "$$" thanh PID cua chinh no ngay trong ngoac kep (xem ghi chu o ghi_trang_thai_loi).
ket_thuc_job() {
  local ma="$1" trang_thai="$2" nhat_ky="${3:-}"
  la_uuid "$ma" \
    || { ghi_log canh-bao "Ma cong viec '$ma' khong phai uuid -- bo qua buoc ghi ket qua."; return 0; }
  nhat_ky="${nhat_ky//\'/\'\'}"
  psql_quan_tri -c "UPDATE cong_viec_sao_luu
    SET trang_thai = '$trang_thai', ket_thuc_luc = now(), nhat_ky = '$nhat_ky'
    WHERE id = '$ma';" >/dev/null 2>&1 || true
}

# Gianh khoa runner NHUNG khong thoat khi ban (khac gianh_khoa_hoac_bo_qua, ham do goi `exit 0`).
# Vong lay job can biet "dang ban" de BO QUA luot nay ma KHONG gianh job -- de job nam lai trang
# thai 'cho', phut sau timer goi lai va lay tiep.
gianh_khoa_neu_ranh() {
  command -v flock >/dev/null 2>&1 || {
    ghi_log canh-bao "May khong co lenh 'flock' -- vong lay job chay KHONG khoa."
    return 0
  }
  mkdir -p "$(dirname "$KHOA_RUNNER")" 2>/dev/null || true
  exec 9>"$KHOA_RUNNER" || return 1
  flock -n 9 || return 1
}

# Dong fd 9 => nha khoa. PHAI goi TRUOC khi chay qlgx-restore.sh: script do tu gianh chinh
# $KHOA_RUNNER nay, va con goi lai qlgx-runner.sh sao-luu (buoc sao luu bat buoc truoc phuc hoi)
# nhu mot tien trinh con -- tien trinh do cung can khoa. Giu khoa o day se lam ca hai that bai.
nha_khoa_runner() { exec 9>&- 2>/dev/null || true; }

lenh_chay_job() {
  # BUOC DAU TIEN, truoc ca viec gianh khoa va gianh job: xem sql_don_job_mo_coi de biet vi sao
  # day phai la viec dau tien chu khong phai mot lenh rieng ai do nho chay.
  don_job_mo_coi

  if ! gianh_khoa_neu_ranh; then
    ghi_log thong-tin "Dang co luot sao luu/phuc hoi khac chay -- de cong viec lai hang doi," \
                      "phut sau lay tiep."
    return 0
  fi

  local dong
  dong=$(psql_quan_tri -c "$(sql_gianh_job)" 2>/dev/null | head -1 || true)
  [ -n "$dong" ] || { nha_khoa_runner; return 0; }   # khong co viec gi -- truong hop binh thuong nhat

  local ma loai tham_so snapshot
  IFS=$'\t' read -r ma loai tham_so <<< "$dong"
  # Lop chan cuoi: khong bao gio coi mot dong khong phai uuid la cong viec (xem ghi chu "UPDATE 0"
  # o sql_gianh_job). Tha khong lam gi con hon tao ra tep nhat ky ten la va bao loi gia moi phut.
  if ! la_uuid "$ma"; then
    ghi_log canh-bao "Dau ra la tu cau gianh job, khong phai ma cong viec: '$dong' -- bo qua."
    nha_khoa_runner; return 0
  fi
  ghi_log thong-tin "Nhan cong viec $loai ($ma)"

  local nhat_ky_tep="$THU_MUC_LOG/job-$ma.log"
  local ma_thoat=0
  case "$loai" in
    sao_luu)           lenh_sao_luu --nhan thu-cong --nguon thu_cong >"$nhat_ky_tep" 2>&1 || ma_thoat=$? ;;
    kiem_tra)          lenh_kiem_tra                                 >"$nhat_ky_tep" 2>&1 || ma_thoat=$? ;;
    dong_bo_danh_sach) lenh_dong_bo_danh_sach                        >"$nhat_ky_tep" 2>&1 || ma_thoat=$? ;;
    tai_ve)
      snapshot="$(doc_snapshot_tu_tham_so "$tham_so")"
      if [ -z "$snapshot" ]; then
        ma_thoat=1
        echo "Thieu (hoac khong hop le) snapshotId trong tham so cong viec." >"$nhat_ky_tep"
      else
        lenh_tai_ve "$snapshot" "$ma" >"$nhat_ky_tep" 2>&1 || ma_thoat=$?
      fi ;;
    dien_tap)
      nha_khoa_runner
      lenh_dien_tap >"$nhat_ky_tep" 2>&1 || ma_thoat=$? ;;
    phuc_hoi)
      nha_khoa_runner
      snapshot="$(doc_snapshot_tu_tham_so "$tham_so")"
      if [ -z "$snapshot" ]; then
        ma_thoat=1
        echo "Thieu (hoac khong hop le) snapshotId trong tham so cong viec." >"$nhat_ky_tep"
      else
        # qlgx-restore.sh tu ghi ket qua vao bang cong viec cua CSDL MOI sau khi hoan doi -- ban
        # ghi trong CSDL cu bien mat cung CSDL do, nen KHONG goi ket_thuc_job o day khi thanh cong.
        "$GOC_UNG_DUNG/scripts/qlgx-restore.sh" \
          --snapshot "$snapshot" --ma-job "$ma" --apply >"$nhat_ky_tep" 2>&1 || ma_thoat=$?
        if [ "$ma_thoat" -eq 0 ]; then
          ghi_log thong-tin "Cong viec phuc hoi $ma xong (ket qua do qlgx-restore.sh ghi vao CSDL moi)."
          lenh_don_spool
          return 0
        fi
      fi ;;
    *) ma_thoat=1; printf 'Loai cong viec khong hieu: %s\n' "$loai" >"$nhat_ky_tep" ;;
  esac

  # Chi giu 8000 byte CUOI: nhat ky nay hien nguyen van tren man hinh quan tri, va phan huu ich
  # khi that bai luon nam o cuoi.
  local noi_dung; noi_dung=$(tail -c 8000 "$nhat_ky_tep" 2>/dev/null || echo "")
  if [ "$ma_thoat" -eq 0 ]; then
    ket_thuc_job "$ma" xong "$noi_dung"
    ghi_log thong-tin "Cong viec $ma ($loai) xong."
  else
    ket_thuc_job "$ma" loi "$noi_dung"
    ghi_trang_thai_loi "Cong viec $loai that bai"
    ghi_log loi "Cong viec $ma that bai (ma $ma_thoat). Xem $nhat_ky_tep"
  fi
  lenh_don_spool
}

lenh_dien_tap() {
  # Ranh gioi giua "co sao luu" va "co kha nang phuc hoi": mot ban sao chua tung duoc phuc hoi
  # thu chi la mot gia dinh.
  #
  # qlgx-restore.sh tu ghi dien_tap_gan_nhat/dien_tap_dat khi no chay toi buoc kiem chung
  # (ghi_ket_qua_dien_tap). Nhung neu no chet SOM hon the -- khong tai duoc snapshot, kho R2 hong,
  # het dia -- thi KHONG dong nao duoc ghi va man hinh quan tri se hien so lieu dien tap CU nhu
  # the khong co gi xay ra. Vi vay o day van ghi lai mot lan nua: ghi de bang cung gia tri khi
  # thanh cong (vo hai), va bit dung lo hong "that bai truoc khi kip ghi".
  if "$GOC_UNG_DUNG/scripts/qlgx-restore.sh" --snapshot latest --dien-tap; then
    psql_quan_tri -c "UPDATE trang_thai_sao_luu
      SET dien_tap_gan_nhat = now(), dien_tap_dat = true WHERE id = 1;" >/dev/null
    ghi_log thong-tin "Dien tap phuc hoi DAT."
  else
    psql_quan_tri -c "UPDATE trang_thai_sao_luu
      SET dien_tap_gan_nhat = now(), dien_tap_dat = false,
          loi_gan_nhat = 'Dien tap phuc hoi that bai' WHERE id = 1;" >/dev/null
    ghi_log loi "Dien tap phuc hoi THAT BAI."
    return 1
  fi
}

main_runner() {
  nap_cau_hinh
  local lenh="${1:-}"; shift || true
  case "$lenh" in
    sao-luu)           gianh_khoa_hoac_bo_qua "$lenh"; lenh_sao_luu "$@" ;;
    kiem-tra)          gianh_khoa_hoac_bo_qua "$lenh"; lenh_kiem_tra ;;
    dong-bo-danh-sach) gianh_khoa_hoac_bo_qua "$lenh"; lenh_dong_bo_danh_sach ;;
    tai-ve)            lenh_tai_ve "$@" ;;
    don-spool)         lenh_don_spool ;;
    # chay-job/dien-tap KHONG goi gianh_khoa_hoac_bo_qua o day: chay-job tu quan ly khoa (phai
    # nha truoc khi goi qlgx-restore.sh), con dien-tap chay qlgx-restore.sh -- script do tu gianh
    # dung tep khoa nay.
    chay-job)          lenh_chay_job ;;
    dien-tap)          lenh_dien_tap ;;
    # dem-snapshot-nhan CHI DOC (--no-lock) -- KHONG gianh_khoa_hoac_bo_qua, chay dung luc mot
    # lenh khac dang giu khoa GHI la tinh huong BINH THUONG can xu ly duoc (vd install.sh's
    # cap_nhat dang doi xac nhan snapshot vua tao boi chinh lenh_sao_luu no goi ngay truoc do).
    dem-snapshot-nhan) lenh_dem_snapshot_nhan "$@" ;;
    dem-snapshot)      lenh_dem_snapshot ;;
    id-snapshot-nhan)  lenh_id_snapshot_nhan "$@" ;;
    # khoi-tao-kho CO gianh khoa: `restic init` ghi vao kho, khong duoc chay chong mot luot
    # sao-luu/forget dang giu khoa ghi.
    khoi-tao-kho)      gianh_khoa_hoac_bo_qua "$lenh"; lenh_khoi_tao_kho ;;
    *) bao_loi_va_thoat "Lenh khong hieu: '$lenh'." \
         "Dung: sao-luu|kiem-tra|dong-bo-danh-sach|tai-ve|don-spool|chay-job|dien-tap|" \
         "dem-snapshot-nhan|dem-snapshot|id-snapshot-nhan|khoi-tao-kho" ;;
  esac
}

[ "${QLGX_CHI_NAP_HAM:-0}" = "1" ] && return 0
main_runner "$@"
