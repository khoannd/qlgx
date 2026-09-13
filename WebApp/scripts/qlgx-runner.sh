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

lenh_sao_luu() {
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
  dc exec -T postgres pg_dump -Fd -Z0 -j4 -U "$(env_ung_dung POSTGRES_USER)" \
      -d "$(env_ung_dung POSTGRES_DB)" -f "/tmp/$TEN_DUMP_CONTAINER"
  dc exec -T postgres tar -cf - -C /tmp "$TEN_DUMP_CONTAINER" | tar -xf - -C "$TAM_DUMP_SAO_LUU"
  # Doi ten lai thanh "qlgx-dump" co dinh TREN HOST (chi ten trong container la ngau nhien) de
  # phan con lai cua ham nay va lenh_tai_ve (glob `*/qlgx-dump`) khong doi.
  mv "$TAM_DUMP_SAO_LUU/$TEN_DUMP_CONTAINER" "$TAM_DUMP_SAO_LUU/qlgx-dump"
  dc exec -T postgres rm -rf "/tmp/$TEN_DUMP_CONTAINER"
  [ -s "$TAM_DUMP_SAO_LUU/qlgx-dump/toc.dat" ] \
    || that_bai_sao_luu "pg_dump khong tao ra muc luc (toc.dat) -- DUNG, khong sao luu."

  ghi_log thong-tin "Dump vai tro va quyen toan cuc"
  dc exec -T postgres pg_dumpall --globals-only -U "$(env_ung_dung POSTGRES_USER)" \
      > "$TAM_DUMP_SAO_LUU/globals.sql"
  [ -s "$TAM_DUMP_SAO_LUU/globals.sql" ] \
    || that_bai_sao_luu "pg_dumpall --globals-only tao ra tep rong -- DUNG."

  ghi_log thong-tin "Gom tep cau hinh"
  mkdir -p "$TAM_DUMP_SAO_LUU/cau-hinh"
  for f in .env docker-compose.yml docker-compose.prod.yml Caddyfile; do
    [ -f "$GOC_UNG_DUNG/$f" ] && cp "$GOC_UNG_DUNG/$f" "$TAM_DUMP_SAO_LUU/cau-hinh/"
  done
  # /etc/qlgx/backup.env CO Y khong nam trong ban sao luu: khong tu ma hoa mot tep bang chinh
  # khoa chua trong tep do. Do la ly do phai co The phuc hoi cat NGOAI may chu.

  # Dem so lieu de gan vao tag -- man hinh phuc hoi hien "quay ve day nghia la con 2050 giao dan".
  local so_gd so_gdinh
  so_gd=$(psql_quan_tri -c 'SELECT count(*) FROM giao_dan' | tr -d ' ' || echo 0)
  so_gdinh=$(psql_quan_tri -c 'SELECT count(*) FROM gia_dinh' | tr -d ' ' || echo 0)

  ghi_log thong-tin "Day len kho restic"
  restic backup "$TAM_DUMP_SAO_LUU" \
    --tag "nhan=${nhan:-tu-dong}" --tag "nguon=$nguon" \
    --tag "giao_dan=$so_gd" --tag "gia_dinh=$so_gdinh" \
    --host qlgx

  # Da day len kho restic xong -- xoa NGAY anh dump tho tren dia thay vi cho toi khi ham ket
  # thuc hay tien trinh thoat. Giam toi thieu thoi gian du lieu CHUA MA HOA ton tai tren dia.
  # (Thu muc ben trong container da duoc xoa ngay sau buoc tar o tren.)
  rm -rf "$TAM_DUMP_SAO_LUU"
  trap - EXIT INT TERM HUP

  ghi_log thong-tin "Kiem tra toan ven truoc khi don ban cu"
  restic check --read-data-subset=5%

  # THU TU CUNG: backup -> check -> forget. Khong bao gio xoa ban cu truoc khi ban moi duoc
  # xac nhan doc duoc.
  ghi_log thong-tin "Don ban cu theo chinh sach giu"
  # --group-by host (KHONG con "host,paths" mac dinh cua restic): moi lan chay dung mot thu muc
  # tam MOI (mktemp sinh hau to ngau nhien khac nhau) nen truong `paths` trong snapshot KHAC NHAU
  # MOI LAN -- neu de restic tu nhom theo ca "paths" nhu mac dinh, MOI snapshot roi vao MOT nhom
  # rieng chi co dung 1 phan tu, va --keep-last/--keep-daily/... se "giu" TAT CA vi moi nhom chi
  # co 1 thanh vien de giu -- chinh sach giu ban COI NHU KHONG TON TAI, kho R2 phinh vo han. Moi
  # snapshot QLGX deu cung MOT "host" logic (host cung dinh "qlgx" o lenh backup phia tren) du
  # duong dan tam khac nhau, nen nhom theo host la dung.
  # shellcheck disable=SC2086
  restic forget $GIU_LAI --group-by host --prune

  lenh_dong_bo_danh_sach
  psql_quan_tri -c "UPDATE trang_thai_sao_luu
                    SET sao_luu_gan_nhat = now(), loi_gan_nhat = NULL WHERE id = 1;" >/dev/null
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

lenh_dong_bo_danh_sach() {
  local tam; tam=$(mktemp)
  # Tu huy trap NGAY khi no chay: `trap ... RETURN` cua bash KHONG tu dong het hieu luc sau
  # LAN TRA VE DAU TIEN -- no o LAI va se chay LAI o moi lan tra ve tiep theo cua BAT KY ham
  # nao khac trong cung shell. Ham nay thuong duoc GOI LONG ben trong lenh_sao_luu (bien $tam
  # cung ten nhung khac o do) -- neu khong tu huy, trap se chay LAN NUA luc lenh_sao_luu tra
  # ve va dung nham bien $tam CUA lenh_sao_luu (thu muc dump, khong phai tep json nay), gay
  # loi that "rm: cannot remove ...: Is a directory" -- da tu kiem chung loi nay that su xay ra
  # (bang mot ham bash toi gian rieng) truoc khi sua bang cach tu go trap ngay trong than trap.
  trap 'rm -f "$tam"; trap - RETURN' RETURN
  restic snapshots --json > "$tam"
  # Nap lai TOAN BO bang dem trong mot giao dich -- don gian va luon dung, so snapshot chi vai
  # chuc dong nen khong can dong bo tang phan.
  {
    echo "BEGIN;"
    echo "DELETE FROM ban_sao_luu;"
    loc_snapshot_json "$tam" | while IFS=$'\t' read -r id thoi_diem nhan byte gd gdinh; do
      # Nhan doi dau nhay don TRONG NOI DUNG truoc khi bao vao nhay don SQL -- giong het cach
      # ghi_trang_thai_loi da lam o tren, ap dung LAI o day cho NHAT QUAN. `nhan` la noi dung tag
      # restic ma quan tri vien co the dat tuy y qua `--nhan` (ke ca goi tu giao dien qua Task
      # 6/17) -- ban truoc thieu buoc nay: mot nhan nhu "Truoc le Cha So's" da du pha vo cau
      # lenh SQL, co y thi la chen SQL chay duoi quyen chu CSDL qua `psql -f -`. id/thoi_diem lay
      # tu chinh restic (khong phai dau vao nguoi dung) nhung van escape cho chac, dong bo/nguon
      # suy tu enum co dinh (tinh_nguon_tu_nhan) nen von an toan nhung van escape cho nhat quan.
      local id_e="${id//\'/\'\'}" thoi_diem_e="${thoi_diem//\'/\'\'}" nhan_e="${nhan//\'/\'\'}"
      local nguon_e; nguon_e="$(tinh_nguon_tu_nhan "$nhan")"; nguon_e="${nguon_e//\'/\'\'}"
      printf "INSERT INTO ban_sao_luu (id,thoi_diem,nhan,kich_thuoc_byte,so_giao_dan,so_gia_dinh,nguon) VALUES ('%s','%s',%s,%s,%s,%s,'%s');\n" \
        "$id_e" "$thoi_diem_e" "$([ -n "$nhan_e" ] && printf "'%s'" "$nhan_e" || echo NULL)" \
        "${byte:-0}" "${gd:-0}" "${gdinh:-0}" "$nguon_e"
    done
    echo "COMMIT;"
  } | psql_quan_tri -f - >/dev/null
  ghi_log thong-tin "Da dong bo bang dem danh sach ban sao."
}

lenh_tai_ve() {
  local snapshot="$1" ma_job="$2"
  local dich="$THU_MUC_SPOOL/$ma_job"
  mkdir -p "$dich"; chmod 755 "$dich"
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

main_runner() {
  nap_cau_hinh
  local lenh="${1:-}"; shift || true
  case "$lenh" in
    sao-luu)           gianh_khoa_hoac_bo_qua "$lenh"; lenh_sao_luu "$@" ;;
    kiem-tra)          gianh_khoa_hoac_bo_qua "$lenh"; lenh_kiem_tra ;;
    dong-bo-danh-sach) gianh_khoa_hoac_bo_qua "$lenh"; lenh_dong_bo_danh_sach ;;
    tai-ve)            lenh_tai_ve "$@" ;;
    don-spool)         lenh_don_spool ;;
    *) bao_loi_va_thoat "Lenh khong hieu: '$lenh'. Dung: sao-luu|kiem-tra|dong-bo-danh-sach|tai-ve|don-spool" ;;
  esac
}

[ "${QLGX_CHI_NAP_HAM:-0}" = "1" ] && return 0
main_runner "$@"
