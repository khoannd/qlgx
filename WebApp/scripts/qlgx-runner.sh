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

lenh_sao_luu() {
  local nhan="" nguon=""
  while [ $# -gt 0 ]; do
    case "$1" in
      --nhan)  nhan="$2"; shift 2 ;;
      --nguon) nguon="$2"; shift 2 ;;
      *) bao_loi_va_thoat "Tham so khong hieu: $1" ;;
    esac
  done
  [ -n "$nguon" ] || nguon=$(tinh_nguon_tu_nhan "$nhan")

  # ${TMPDIR:-/var/tmp}: /var/tmp la thu muc that tren moi may Linux muc tieu (khong doi hanh
  # vi that). Chi cho phep ghi de qua TMPDIR de kiem chung tren may dev khong co /var/tmp that
  # (vd Windows qua Git Bash).
  #
  # CHU Y BIEN TOAN CUC (khong phai loi go nham `local`): trap EXIT chi THAT SU chay luc TIEN
  # TRINH thoat, tuc la SAU KHI ham nay da tra ve tu lau. Neu bien giu duong dan thu muc tam la
  # `local`, pham vi cua no da bi huy khi trap chay -- duoi `set -u` viec doc lai bien se bao
  # loi "unbound variable" va CAU LENH rm -rf KHONG BAO GIO duoc thuc thi, de lai nguyen anh dump
  # CSDL CHUA MA HOA tren dia MAI MAI (da tu kiem chung that bang mot ham bash toi gian, day la
  # loi that trong ban dau cua brief nay). Vi vay TAM_DUMP_SAO_LUU phai la bien TOAN CUC.
  TAM_DUMP_SAO_LUU=$(mktemp -d "${TMPDIR:-/var/tmp}/qlgx-sao-luu.XXXXXX")
  chmod 700 "$TAM_DUMP_SAO_LUU"
  # Ban dump THO chua toan bo du lieu giao dan, chua ma hoa -- khong bao gio de no nam lai tren
  # dia. trap xoa ca khi loi hoac bi ngat (Ctrl-C, cron kill, mat ket noi SSH).
  trap 'rm -rf "$TAM_DUMP_SAO_LUU"' EXIT

  ghi_log thong-tin "Dump CSDL"
  # -Fd -Z0 (thu muc, KHONG tu nen) chu KHONG phai -Fc: -Fc nen san, mot byte doi o dau lam
  # toan bo luong nen doi theo -- restic khong khu trung lap duoc gi va moi snapshot luu gan nhu
  # mot ban day du moi. Anh dai dien nam trong bytea va gan nhu khong bao gio doi, nen day la
  # khac biet lon: de restic tu nen va khu trung lap theo khoi. -j4 dump song song 4 bang mot
  # luc. Xem thiet ke muc 13.3.
  dc exec -T postgres pg_dump -Fd -Z0 -j4 -U "$(env_ung_dung POSTGRES_USER)" \
      -d "$(env_ung_dung POSTGRES_DB)" -f /tmp/qlgx-dump
  dc exec -T postgres tar -cf - -C /tmp qlgx-dump | tar -xf - -C "$TAM_DUMP_SAO_LUU"
  dc exec -T postgres rm -rf /tmp/qlgx-dump
  [ -s "$TAM_DUMP_SAO_LUU/qlgx-dump/toc.dat" ] \
    || bao_loi_va_thoat "pg_dump khong tao ra muc luc (toc.dat) -- DUNG, khong sao luu."

  ghi_log thong-tin "Dump vai tro va quyen toan cuc"
  dc exec -T postgres pg_dumpall --globals-only -U "$(env_ung_dung POSTGRES_USER)" \
      > "$TAM_DUMP_SAO_LUU/globals.sql"
  [ -s "$TAM_DUMP_SAO_LUU/globals.sql" ] \
    || bao_loi_va_thoat "pg_dumpall --globals-only tao ra tep rong -- DUNG."

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
  rm -rf "$TAM_DUMP_SAO_LUU"
  trap - EXIT

  ghi_log thong-tin "Kiem tra toan ven truoc khi don ban cu"
  restic check --read-data-subset=5%

  # THU TU CUNG: backup -> check -> forget. Khong bao gio xoa ban cu truoc khi ban moi duoc
  # xac nhan doc duoc.
  ghi_log thong-tin "Don ban cu theo chinh sach giu"
  # shellcheck disable=SC2086
  restic forget $GIU_LAI --prune

  lenh_dong_bo_danh_sach
  psql_quan_tri -c "UPDATE trang_thai_sao_luu
                    SET sao_luu_gan_nhat = now(), loi_gan_nhat = NULL WHERE id = 1;" >/dev/null
  ghi_log thong-tin "Sao luu xong ($so_gd giao dan, $so_gdinh gia dinh)."
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
      printf "INSERT INTO ban_sao_luu (id,thoi_diem,nhan,kich_thuoc_byte,so_giao_dan,so_gia_dinh,nguon) VALUES ('%s','%s',%s,%s,%s,%s,'%s');\n" \
        "$id" "$thoi_diem" "$([ -n "$nhan" ] && printf "'%s'" "$nhan" || echo NULL)" \
        "${byte:-0}" "${gd:-0}" "${gdinh:-0}" "$(tinh_nguon_tu_nhan "$nhan")"
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

main_runner() {
  nap_cau_hinh
  local lenh="${1:-}"; shift || true
  case "$lenh" in
    sao-luu)           lenh_sao_luu "$@" ;;
    kiem-tra)          lenh_kiem_tra ;;
    dong-bo-danh-sach) lenh_dong_bo_danh_sach ;;
    tai-ve)            lenh_tai_ve "$@" ;;
    don-spool)         lenh_don_spool ;;
    *) bao_loi_va_thoat "Lenh khong hieu: '$lenh'. Dung: sao-luu|kiem-tra|dong-bo-danh-sach|tai-ve|don-spool" ;;
  esac
}

[ "${QLGX_CHI_NAP_HAM:-0}" = "1" ] && return 0
main_runner "$@"
