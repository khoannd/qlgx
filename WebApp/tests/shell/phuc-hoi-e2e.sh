#!/usr/bin/env bash
# Kiem thu dau-cuoi THAT cua qlgx-restore.sh (Task 14) -- PostgreSQL that, API that, restic that.
#
# KHONG Docker-in-Docker. Brief goc de nghi mot container Ubuntu --privileged tu cai Docker ben
# trong no (`curl get.docker.com | sh` + `dockerd`). Bi BAC BO cho moi truong nay, cung ly do da
# ghi trong install-e2e.sh (Task 11) va cap-nhat-e2e.sh (Task 12): may nay dung CHUNG voi ~34
# container that cua du an khac, chi con ~40GB dia trong, va mot dockerd long trong container se
# nhan doi toan bo cache anh. Thay vao do: goi Docker CLI native qua Git Bash tren chinh may nay,
# voi COMPOSE_PROJECT_NAME rieng cho moi lan chay de khong dung cham container/volume cua ai.
#
# BA GIOI HAN bat buoc (giong Task 11/12/13):
#   1. KHONG bind cong 80/443 that cua host: chi `up` postgres va api, KHONG BAO GIO `up` caddy;
#      docker-compose.prod.yml con reset cong cua api ve rong ("ports: !reset []") nen ca api
#      cung khong map ra host. Kiem tra san sang di qua `dc exec api curl localhost:8080`.
#   2. Khong dong toi container/image nao khong do chinh kiem thu nay tao ra.
#   3. Don dep triet de: `down -v`, xoa image cua rieng du an thu, xoa scratch dir; do dia
#      truoc/sau va DUNG NGAY neu tut duoi 15GB.
#
# HAI CONG CU THIEU TREN GIT BASH (khong phai loi cua script, khong co tren Linux that):
#   - `restic`: tai binary tinh Windows tu GitHub releases vao scratch (cach da kiem chung o
#     Task 13), boc bang mot tep "restic" trong PATH rieng cua kiem thu.
#   - `flock`: Git Bash khong co util-linux. Dat mot BAN GIA trong PATH rieng, chi tra ve 0.
#     Dieu nay CHI anh huong toi co che chong chay chong (da co test rieng o tang khac); moi
#     duong logic phuc hoi ben duoi van chay that.
set -euo pipefail

# Git Bash TU DOI moi tham so trong giong duong dan tuyet doi POSIX thanh duong dan Windows truoc
# khi goi mot chuong trinh Windows (docker.exe). Duong dan "/tmp/qlgx-dump-..." ma qlgx-runner.sh
# va qlgx-restore.sh truyen cho pg_dump/pg_restore BEN TRONG container bi doi thanh
# "C:/Users/.../Temp/..." va lenh that bai ("could not create directory ... No such file or
# directory"). Hai bien duoi tat viec doi do. CHI can tren Windows -- tren may chu Linux that
# khong co co che nay.
export MSYS_NO_PATHCONV=1
export MSYS2_ARG_CONV_EXCL='*'

THU_MUC_WEBAPP_THAT="$(cd "$(dirname "$0")/../.." && pwd)"
readonly THU_MUC_WEBAPP_THAT
readonly SCRATCH="${QLGX_E2E_SCRATCH:-D:/Working/QLGX/qlgx-e2e-scratch-t14}"
readonly NGUONG_DIA_TOI_THIEU_GB=15
readonly RESTIC_URL="https://github.com/restic/restic/releases/download/v0.18.0/restic_0.18.0_windows_amd64.zip"

# Ten du an Compose rieng cho tung lan chay: KHONG dung chung volume/network voi phien Claude
# khac co the dang chay kiem thu cua ho tren cung may. Xuat ra moi truong de CA qlgx-restore.sh
# lan qlgx-runner.sh (chay nhu tien trinh con, dung `docker compose` khong co -p) deu roi vao
# dung du an nay.
export COMPOSE_PROJECT_NAME="qlgx-e2e-phuchoi-$$"

dung_gb_trong() {
  df -BG --output=avail /d 2>/dev/null | tail -1 | tr -dc '0-9' \
    || df -BG --output=avail / 2>/dev/null | tail -1 | tr -dc '0-9'
}

don_dep() {
  echo "==> Don dep: container/volume/image cua RIENG kiem thu nay, roi scratch dir"
  if [ -n "${GOC_UNG_DUNG:-}" ] && [ -f "${GOC_UNG_DUNG:-}/docker-compose.yml" ]; then
    docker compose --project-directory "$GOC_UNG_DUNG" \
      -f "$GOC_UNG_DUNG/docker-compose.yml" -f "$GOC_UNG_DUNG/docker-compose.prod.yml" \
      down -v >/dev/null 2>&1 || true
  fi
  docker rmi "${COMPOSE_PROJECT_NAME}-api:latest" >/dev/null 2>&1 || true
  # KHONG `docker builder prune` / `docker system prune` (cam trong brief).
  rm -rf "$SCRATCH"
}
trap don_dep EXIT

that_bai() { echo ""; echo "THAT BAI: $*" >&2; exit 1; }

echo "==> Kiem tra dung luong dia truoc khi chay"
truoc_gb=$(dung_gb_trong)
echo "    Dia trong truoc: ${truoc_gb:-?} GB"
if [ -n "$truoc_gb" ] && [ "$truoc_gb" -lt "$NGUONG_DIA_TOI_THIEU_GB" ]; then
  echo "BLOCKED: dia da duoi nguong ${NGUONG_DIA_TOI_THIEU_GB}GB TRUOC khi chay -- dung lai." >&2
  exit 1
fi

echo "==> Dung ban sao WebApp rieng trong scratch (khong dong vao cay lam viec that)"
# `cp -r` tho tren cay lam viec hien tai (KHONG `git archive`/`git ls-files`): co phien Claude
# khac dang sua tep tren cung nhanh, va ban DA COMMIT co the khong bien dich duoc -- bai hoc ghi
# ro trong install-e2e.sh/cap-nhat-e2e.sh.
rm -rf "$SCRATCH"
mkdir -p "$SCRATCH"
export GOC_UNG_DUNG="$SCRATCH/WebApp-t14"
cp -r "$THU_MUC_WEBAPP_THAT" "$GOC_UNG_DUNG"

export THU_MUC_CAU_HINH="$SCRATCH/etc-qlgx"
export THU_MUC_SPOOL="$SCRATCH/spool"
export THU_MUC_LOG="$SCRATCH/log"
export TMPDIR="$SCRATCH/tmp"
export KHOA_RUNNER="$SCRATCH/qlgx-runner.lock"
mkdir -p "$THU_MUC_CAU_HINH" "$THU_MUC_SPOOL" "$THU_MUC_LOG" "$TMPDIR" "$SCRATCH/bin"

echo "==> Chuan bi restic (binary tinh) va ban gia flock trong PATH rieng cua kiem thu"
curl -fsSL -o "$SCRATCH/bin/restic.zip" "$RESTIC_URL"
unzip -o -q "$SCRATCH/bin/restic.zip" -d "$SCRATCH/bin"
cat > "$SCRATCH/bin/restic" <<EOF
#!/usr/bin/env bash
exec "$SCRATCH/bin/restic_0.18.0_windows_amd64.exe" "\$@"
EOF
cat > "$SCRATCH/bin/flock" <<'EOF'
#!/usr/bin/env bash
# Ban GIA cho Git Bash (khong co util-linux). Luon "gianh duoc khoa".
exit 0
EOF
chmod +x "$SCRATCH/bin/restic" "$SCRATCH/bin/flock"
# PATH phai la duong dan kieu POSIX (/d/Working/...), KHONG phai kieu Windows (D:/Working/...):
# Git Bash tach PATH bang dau ':' nen mot muc co ky tu o dia bi cat lam doi va khong tim ra lenh
# nao (da gap that: "restic: command not found"). `cd ... && pwd` cho dung dang POSIX.
PATH="$(cd "$SCRATCH/bin" && pwd):$PATH"
export PATH
restic version

echo "==> Nap install.sh CHI DE LAY HAM roi dung mot he thong THAT (postgres + api that)"
# KHONG `export` QLGX_CHI_NAP_HAM: bien nay chi co y nghia cho lan `source` ngay duoi. Xuat no ra
# moi truong thi MOI tien trinh con (qlgx-runner.sh, qlgx-restore.sh) cung thay va tu `return`
# thay vi chay -- loi that da gap: "return: can only `return' from a function or sourced script".
# shellcheck disable=SC2034
QLGX_CHI_NAP_HAM=1
export QLGX_R2_ENDPOINT=http://khong-dung-that
export QLGX_R2_BUCKET=thu
export QLGX_R2_ACCESS_KEY_ID=k
export QLGX_R2_SECRET_ACCESS_KEY=s
export QLGX_GIAO_XU_TEN="Giao xu Thu Nghiem"
export QLGX_ADMIN_TEN_TAI_KHOAN=quantri
export QLGX_ADMIN_MAT_KHAU=MatKhau12345
export QLGX_ADMIN_HO_TEN="Quan Tri Thu"
# shellcheck source=/dev/null
source "$GOC_UNG_DUNG/scripts/install.sh"
unset QLGX_CHI_NAP_HAM
# PHAI dat SAU khi source: install.sh tu gan KHONG_TUONG_TAC=0 o muc tep. hoi_hoac_bien() doc
# bien nay de bao loi thay vi treo may cho nguoi go tren /dev/tty.
# shellcheck disable=SC2034
KHONG_TUONG_TAC=1

sinh_env "$GOC_UNG_DUNG/.env"
ghi_backup_env "$THU_MUC_CAU_HINH/backup.env"
# Kho restic tren dia, khong can R2 that cho bo kiem thu nay.
set_env_kv "$THU_MUC_CAU_HINH/backup.env" RESTIC_REPOSITORY "$SCRATCH/kho-thu"
set -a
RESTIC_REPOSITORY="$SCRATCH/kho-thu"
RESTIC_PASSWORD="$(doc_env_kv "$THU_MUC_CAU_HINH/backup.env" RESTIC_PASSWORD)"
set +a
restic init >/dev/null
echo "    Kho restic thu: $RESTIC_REPOSITORY"

bat_postgres
tao_vai_tro_rls
bat_api            # build lan dau mat vai phut (npm ci, dotnet publish, Chromium); sau do dung cache
khoi_tao_giao_xu_va_admin

TEN_DB=$(doc_env_kv "$GOC_UNG_DUNG/.env" POSTGRES_DB)
NGUOI_DB=$(doc_env_kv "$GOC_UNG_DUNG/.env" POSTGRES_USER)
psql_db() { # psql_db <ten_csdl> <cau lenh sql>
  dc exec -T postgres psql -tAX -v ON_ERROR_STOP=1 -U "$NGUOI_DB" -d "$1" -c "$2" | tr -d ' \r'
}
danh_sach_db() { psql_db postgres "SELECT datname FROM pg_database ORDER BY 1"; }
so_snapshot() { restic snapshots --json | grep -o '"short_id"' | grep -c . || true; }
snapshot_moi_nhat() {
  restic snapshots --json --latest 1 | grep -o '"short_id":"[0-9a-f]*"' | head -1 | cut -d'"' -f4
}
chay_restore() { "$GOC_UNG_DUNG/scripts/qlgx-restore.sh" "$@"; }

echo ""
echo "=== Chuan bi du lieu moc va mot ban sao luu THAT ==="
# Mot bang moc de nhan ra "du lieu da quay lai", VA du lieu nghiep vu THAT (1 giao dan, 1 gia
# dinh). Du lieu nghiep vu that la BAT BUOC cho vong review 1: buoc sao luu bat buoc chi co y
# nghia khi CO so sach de mat (xem co_du_lieu_can_bao_ve), va phep doi chieu "so giao dan nap
# duoc" voi tag cua snapshot chi thuc su duoc kiem khi con so khac 0.
psql_db "$TEN_DB" "CREATE TABLE moc(v text); INSERT INTO moc VALUES ('con-nguyen');" >/dev/null
GX_ID=$(psql_db "$TEN_DB" "SELECT id FROM giao_xu LIMIT 1")
them_du_lieu_nghiep_vu() {
  psql_db "$TEN_DB" "
    INSERT INTO gia_dinh (id, ma_gia_dinh_cu, da_xoa, da_chuyen_xu, khong_thong_ke, giao_xu_id,
                          created_at, updated_at)
    VALUES (gen_random_uuid(), 1, false, false, false, '$GX_ID', now(), now());
    INSERT INTO giao_dan (id, ma_giao_dan_cu, ho_ten, con_hoc, da_co_gia_dinh, tan_tong,
                          khong_thong_ke, qua_doi, da_xoa, giao_xu_id, created_at, updated_at)
    VALUES (gen_random_uuid(), 1, 'Nguyen Van Thu', false, false, false, false, false, false,
            '$GX_ID', now(), now());" >/dev/null
}
them_du_lieu_nghiep_vu
[ "$(psql_db "$TEN_DB" "SELECT count(*) FROM giao_dan")" = "1" ] || that_bai "khong chen duoc giao dan thu"
"$GOC_UNG_DUNG/scripts/qlgx-runner.sh" sao-luu --nhan thu-cong
SN_GOC=$(snapshot_moi_nhat)
[ -n "$SN_GOC" ] || that_bai "khong lay duoc id snapshot vua tao"
echo "    Snapshot goc (co bang moc): $SN_GOC"
psql_db "$TEN_DB" "DROP TABLE moc;" >/dev/null
[ "$(psql_db "$TEN_DB" "SELECT count(*) FROM pg_tables WHERE tablename='moc'")" = "0" ] \
  || that_bai "chua xoa duoc bang moc truoc khi thu phuc hoi"

echo ""
echo "=== Kich ban 1: KHONG co --apply thi CHI IN KE HOACH, khong duoc doi MOT THU GI ==="
db_truoc=$(danh_sach_db)
id_api_truoc=$(dc ps -q api)
id_pg_truoc=$(dc ps -q postgres)
sn_truoc=$(so_snapshot)
ra_1=$(chay_restore --snapshot "$SN_GOC" 2>&1) || that_bai "che do ke hoach tra ve ma loi"
echo "$ra_1"
printf '%s\n' "$ra_1" | grep -qi "KE HOACH PHUC HOI" || that_bai "khong in ra ke hoach"
printf '%s\n' "$ra_1" | grep -qi "Them --apply" || that_bai "ke hoach khong nhac cach thuc hien"
[ "$(psql_db "$TEN_DB" "SELECT count(*) FROM pg_tables WHERE tablename='moc'")" = "0" ] \
  || that_bai "che do ke hoach DA phuc hoi du lieu (bang moc quay lai)"
[ "$(danh_sach_db)" = "$db_truoc" ] \
  || { echo "$db_truoc"; danh_sach_db; that_bai "che do ke hoach da tao/xoa CSDL"; }
[ "$(dc ps -q api)" = "$id_api_truoc" ] || that_bai "che do ke hoach da khoi dong lai container api"
[ "$(dc ps -q postgres)" = "$id_pg_truoc" ] || that_bai "che do ke hoach da dung toi container postgres"
[ "$(so_snapshot)" = "$sn_truoc" ] || that_bai "che do ke hoach da tao them snapshot"
echo "    DAT: khong doi du lieu, khong doi danh sach CSDL, khong doi container, khong them snapshot"

echo ""
echo "=== Kich ban 1b (Critical 1): runner bao THANH CONG nhung KHONG tao snapshot -- phai TU CHOI phuc hoi ==="
# Tai sao kich ban nay ton tai: qlgx-runner.sh's gianh_khoa_hoac_bo_qua CO Y `exit 0` khi khong
# gianh duoc flock (mot luot sao luu khac -- cron dem, hay install.sh cap_nhat -- dang chay). Voi
# qlgx-restore.sh thi ma 0 do la mot LOI NOI DOI: "sao luu bat buoc" bi bo qua ma khong ai thay,
# roi ta di thang vao hoan doi ma khong con duong lui. Thay tam qlgx-runner.sh bang mot ban gia
# mo phong DUNG hanh vi do (in canh bao, exit 0, khong tao snapshot nao).
cp "$GOC_UNG_DUNG/scripts/qlgx-runner.sh" "$SCRATCH/qlgx-runner.that.sh"
cat > "$GOC_UNG_DUNG/scripts/qlgx-runner.sh" <<'EOF'
#!/usr/bin/env bash
# BAN GIA (chi dung trong kiem thu): mo phong nhanh "dang co luot khac giu khoa" cua
# gianh_khoa_hoac_bo_qua -- ghi canh bao roi thoat 0 MA KHONG tao snapshot nao.
echo "[!] Dang co mot luot 'sao-luu' khac chay -- bo qua luot nay (khong phai loi)." >&2
exit 0
EOF
chmod +x "$GOC_UNG_DUNG/scripts/qlgx-runner.sh"
sn_truoc=$(so_snapshot)
set +e
ra_1b=$(chay_restore --snapshot "$SN_GOC" --apply 2>&1)
ma_1b=$?
set -e
cp "$SCRATCH/qlgx-runner.that.sh" "$GOC_UNG_DUNG/scripts/qlgx-runner.sh"
printf '%s\n' "$ra_1b" | tail -4
[ "$ma_1b" -ne 0 ] || that_bai "phuc hoi van chay tiep du buoc sao luu bat buoc khong tao ra snapshot nao"
printf '%s\n' "$ra_1b" | grep -qi "KHONG co them ban sao" \
  || that_bai "khong bao dung ly do (thieu snapshot moi sau khi sao luu)"
[ "$(so_snapshot)" = "$sn_truoc" ] || that_bai "so snapshot doi trong kich ban nay (khong the)"
[ "$(psql_db "$TEN_DB" "SELECT count(*) FROM pg_tables WHERE tablename='moc'")" = "0" ]   || that_bai "da phuc hoi du KHONG co ban sao luu an toan"
if danh_sach_db | grep -q "^${TEN_DB}_truoc_phuc_hoi_"; then
  that_bai "da hoan doi CSDL du KHONG co ban sao luu an toan"
fi
cho_san_sang 120 || that_bai "he thong hong sau kich ban 1b"
echo "    DAT: tu choi phuc hoi khi khong xac minh duoc ban sao luu moi, he thong nguyen ven"

echo ""
echo "=== Kich ban 2: phuc hoi THAT -- du lieu quay lai, CSDL cu duoc GIU, he thong van chay ==="
sn_truoc=$(so_snapshot)
chay_restore --snapshot "$SN_GOC" --apply || that_bai "phuc hoi that bai"
[ "$(psql_db "$TEN_DB" "SELECT v FROM moc")" = "con-nguyen" ] \
  || that_bai "du lieu KHONG quay lai sau khi phuc hoi"
[ "$(psql_db "$TEN_DB" "SELECT count(*) FROM giao_xu")" -ge 1 ] \
  || that_bai "CSDL sau phuc hoi khong con giao xu nao"
[ "$(psql_db "$TEN_DB" "SELECT count(*) FROM giao_dan")" = "1" ] \
  || that_bai "so giao dan sau phuc hoi khong khop ban sao (kiem doi chieu voi tag snapshot)"
db_luu=$(danh_sach_db | grep "^${TEN_DB}_truoc_phuc_hoi_" | head -1)
[ -n "$db_luu" ] || { danh_sach_db; that_bai "KHONG giu lai CSDL cu de quay lui"; }
echo "    CSDL cu duoc giu lai: $db_luu"
[ "$(psql_db "$db_luu" "SELECT count(*) FROM pg_tables WHERE tablename='moc'")" = "0" ] \
  || that_bai "CSDL giu lai khong phai trang thai TRUOC phuc hoi"
if danh_sach_db | grep -q "^qlgx_phuc_hoi_"; then that_bai "CSDL tam khong duoc doi ten (con sot lai)"; fi
[ "$(so_snapshot)" -gt "$sn_truoc" ] || that_bai "khong co ban sao luu bat buoc truoc khi ghi de"
restic snapshots --json --latest 1 | grep -q "truoc-phuc-hoi" \
  || that_bai "ban sao luu truoc khi phuc hoi khong mang nhan truoc-phuc-hoi"
cho_san_sang 120 || that_bai "API khong san sang sau khi hoan doi CSDL"
echo "    DAT: du lieu quay lai, CSDL cu giu nguyen ven, co ban sao 'truoc-phuc-hoi', API san sang"

echo ""
echo "=== Kich ban 3: phuc hoi 'len nhung hong' -- phai TU DAO NGUOC, chay lai voi DU LIEU CU ==="
# Danh mot dau moc CHI CO trong trang thai hien tai (khong co trong snapshot $SN_GOC): sau khi
# dao nguoc, dau moc nay PHAI con -- do la bang chung he thong quay ve dung du lieu dang chay,
# khong phai du lieu cua ban nap hong.
psql_db "$TEN_DB" "INSERT INTO moc VALUES ('sau-phuc-hoi');" >/dev/null
# Gia lap "CSDL moi len nhung hong": ghi de cho_api_san_sang thanh luon that bai. Day la cach
# TRUNG THUC nhat de ep dung duong loi can kiem chung (API khong san sang sau hoan doi) ma khong
# pha hong image/CSDL that. Toan bo phan con lai cua qlgx-restore.sh chay y nguyen ban that.
set +e
ra_3=$(bash -c "
  set -euo pipefail
  QLGX_CHI_NAP_HAM=1
  source '$GOC_UNG_DUNG/scripts/qlgx-restore.sh'
  unset QLGX_CHI_NAP_HAM
  cho_api_san_sang() { return 1; }
  # THAM SO TEST: rut ngan de kich ban chay nhanh. Mac dinh THAT la 90 giay (xem
  # CHO_SAN_SANG_SAU_HOAN_DOI_GIAY trong qlgx-restore.sh) -- kich ban 3b duoi day chay voi
  # DUNG mac dinh do, khong rut ngan.
  CHO_SAN_SANG_SAU_HOAN_DOI_GIAY=3
  main_phuc_hoi --snapshot '$SN_GOC' --apply
" 2>&1)
ma_3=$?
set -e
echo "$ra_3"
[ "$ma_3" -ne 0 ] || that_bai "phuc hoi bao THANH CONG du API khong bao gio san sang"
printf '%s\n' "$ra_3" | grep -qi "DAO NGUOC" || that_bai "khong thay dau hieu tu dao nguoc"
[ "$(psql_db "$TEN_DB" "SELECT count(*) FROM moc WHERE v='sau-phuc-hoi'")" = "1" ] \
  || that_bai "sau dao nguoc, CSDL dang phuc vu KHONG phai du lieu cu"
[ "$(psql_db "$TEN_DB" "SELECT count(*) FROM moc")" = "2" ] \
  || that_bai "sau dao nguoc, du lieu cu khong con nguyen ven"
danh_sach_db | grep -q "_hong$" || { danh_sach_db; that_bai "ban nap hong khong duoc giu lai de xem xet"; }
dc up -d api >/dev/null 2>&1 || true
cho_san_sang 120 || that_bai "he thong khong chay lai duoc sau khi dao nguoc"
echo "    DAT: tu dao nguoc, he thong chay lai voi DU LIEU CU nguyen ven ($(psql_db "$TEN_DB" "SELECT count(*) FROM moc") dong moc)"

echo ""
echo "=== Kich ban 3b (Important 4): lam CSDL moi HONG THAT -- cho_api_san_sang phai TU phat hien ==="
# Kich ban 3 chung minh "neu phat hien that bai thi dao nguoc dung", nhung no MOCK hang
# cho_api_san_sang nen KHONG chung minh duoc "phat hien dung khi CSDL moi that su hong". O day
# KHONG mock hang do: chi chen mot hu hai THAT vao CSDL moi (xoa bang lich su migration cua EF
# Core) SAU khi kiem chung va TRUOC khi hoan doi. Hau qua that: API chay migration luc khoi dong
# thay toan bo migration "chua ap dung", di tao lai cac bang da ton tai roi chet, va
# /api/suc-khoe/san-sang khong bao gio tra ve san-sang (xem Program.cs). Cho toi da o day dung
# MAC DINH THAT (90 giay), khong rut ngan -- so giay do luong duoc la so giay that.
psql_db "$TEN_DB" "INSERT INTO moc VALUES ('truoc-kich-ban-3b');" >/dev/null
so_moc_truoc_3b=$(psql_db "$TEN_DB" "SELECT count(*) FROM moc")
t0_3b=$(date +%s)
set +e
ra_3b=$(bash -c "
  set -euo pipefail
  QLGX_CHI_NAP_HAM=1
  source '$GOC_UNG_DUNG/scripts/qlgx-restore.sh'
  unset QLGX_CHI_NAP_HAM
  # Boc kiem_chung_csdl_moi: chay ban THAT truoc (phai DAT), roi moi pha CSDL moi. KHONG dong
  # toi cho_api_san_sang -- hang phat hien van la hang that.
  eval \"\$(declare -f kiem_chung_csdl_moi | sed '1s/^kiem_chung_csdl_moi /kiem_chung_that /')\"
  kiem_chung_csdl_moi() {
    kiem_chung_that \"\$1\" || return 1
    ghi_log canh-bao 'KIEM THU: co y xoa bang lich su migration cua CSDL moi de no HONG THAT.'
    pg -d \"\$1\" -c 'DROP TABLE \"__EFMigrationsHistory\";'
  }
  main_phuc_hoi --snapshot '$SN_GOC' --apply
" 2>&1)
ma_3b=$?
set -e
t1_3b=$(date +%s)
printf '%s\n' "$ra_3b" | grep -vE "^ Container" | tail -10
echo "    (mat $((t1_3b - t0_3b)) giay, voi gioi han cho MAC DINH 90 giay)"
[ "$ma_3b" -ne 0 ] || that_bai "phuc hoi bao thanh cong du CSDL moi that su hong"
printf '%s\n' "$ra_3b" | grep -qi "KHONG len duoc sau khi hoan doi" \
  || that_bai "cho_api_san_sang KHONG tu phat hien duoc CSDL moi hong"
printf '%s\n' "$ra_3b" | grep -qi "DAO NGUOC" || that_bai "khong dao nguoc khi CSDL moi hong that"
[ "$(psql_db "$TEN_DB" "SELECT count(*) FROM moc")" = "$so_moc_truoc_3b" ] \
  || that_bai "sau dao nguoc, du lieu dang phuc vu khong con nguyen ven"
[ "$(psql_db "$TEN_DB" "SELECT count(*) FROM moc WHERE v='truoc-kich-ban-3b'")" = "1" ] \
  || that_bai "sau dao nguoc, CSDL dang phuc vu KHONG phai du lieu cu"
cho_san_sang 180 || that_bai "he thong khong chay lai duoc sau khi dao nguoc (hong that)"
echo "    DAT: phat hien hong THAT (khong mock), tu dao nguoc, he thong chay lai voi du lieu cu"

echo ""
echo "=== Kich ban 4: snapshot KHONG TON TAI -- phai dung lai, he thong van chay nguyen ven ==="
so_moc_truoc_4=$(psql_db "$TEN_DB" "SELECT count(*) FROM moc")
set +e
ra_4=$(chay_restore --snapshot khong_ton_tai_zzz --apply 2>&1)
ma_4=$?
set -e
echo "$ra_4" | tail -5
[ "$ma_4" -ne 0 ] || that_bai "phuc hoi tu snapshot khong ton tai ma van bao thanh cong"
[ "$(psql_db "$TEN_DB" "SELECT count(*) FROM moc")" = "$so_moc_truoc_4" ] \
  || that_bai "phuc hoi that bai da lam hong du lieu dang chay"
cho_san_sang 120 || that_bai "phuc hoi that bai da lam hong he thong dang chay"
echo "    DAT: dung lai dung cho, du lieu va dich vu nguyen ven"

echo ""
echo "=== Kich ban 5: --dien-tap -- nap that, kiem chung that, roi XOA, KHONG hoan doi ==="
db_truoc_5=$(danh_sach_db)
so_moc_truoc_5=$(psql_db "$TEN_DB" "SELECT count(*) FROM moc")
ra_5=$(chay_restore --dien-tap --snapshot "$SN_GOC" 2>&1) || that_bai "dien tap that bai"
echo "$ra_5" | tail -6
printf '%s\n' "$ra_5" | grep -qi "DIEN TAP DAT" || that_bai "dien tap khong bao ket qua dat"
[ "$(danh_sach_db)" = "$db_truoc_5" ] \
  || { danh_sach_db; that_bai "dien tap de lai CSDL hoac doi danh sach CSDL"; }
[ "$(psql_db "$TEN_DB" "SELECT count(*) FROM moc")" = "$so_moc_truoc_5" ] \
  || that_bai "dien tap da dong vao du lieu dang phuc vu"
[ "$(psql_db "$TEN_DB" "SELECT dien_tap_dat FROM trang_thai_sao_luu WHERE id=1")" = "t" ] \
  || that_bai "dien tap khong ghi ket qua vao trang_thai_sao_luu"
echo "    DAT: dien tap nap+kiem chung roi don sach, khong hoan doi, co ghi nhan ket qua"

echo ""
echo "=== Kich ban 6: --card (may TRANG, khong co backup.env) ==="
# Che do The phuc hoi: chay voi THU_MUC_CAU_HINH tro vao mot thu muc RONG, chung minh script
# khong ngam phu thuoc vao backup.env do install.sh de lai.
cat > "$SCRATCH/the-phuc-hoi.txt" <<EOF
================== THE PHUC HOI QLGX ==================
May chu   : may-thu
Lap ngay  : $(date '+%d/%m/%Y %H:%M')

MAT KHAU RESTIC (khong co dong nay thi KHONG AI phuc hoi duoc,
ke ca Cloudflare -- day la ban chat cua ma hoa phia may chu):
  $RESTIC_PASSWORD

KHO SAO LUU : $RESTIC_REPOSITORY
R2 KEY ID   : k
R2 SECRET   : s
=======================================================
EOF
mkdir -p "$SCRATCH/etc-rong"
ra_6=$(THU_MUC_CAU_HINH="$SCRATCH/etc-rong" QLGX_GOC_UNG_DUNG="$GOC_UNG_DUNG" \
       chay_restore --card "$SCRATCH/the-phuc-hoi.txt" --snapshot "$SN_GOC" 2>&1) \
  || that_bai "che do --card khong chay duoc khi khong co backup.env"
printf '%s\n' "$ra_6" | grep -qi "KE HOACH PHUC HOI" || that_bai "che do --card khong in duoc ke hoach"
printf '%s\n' "$ra_6" | grep -q "$RESTIC_REPOSITORY" || that_bai "che do --card khong doc dung kho tu the"
# Va lay lai duoc cau hinh cu (.env) tu chinh ban sao luu -- buoc dau tien tren mot may trang.
THU_MUC_CAU_HINH="$SCRATCH/etc-rong" QLGX_GOC_UNG_DUNG="$GOC_UNG_DUNG" \
  chay_restore --card "$SCRATCH/the-phuc-hoi.txt" --snapshot "$SN_GOC" \
               --lay-cau-hinh "$SCRATCH/cau-hinh-lay-ve" >/dev/null \
  || that_bai "khong lay duoc cau hinh cu tu ban sao"
find "$SCRATCH/cau-hinh-lay-ve" -name .env | grep -q . \
  || that_bai "khong thay .env trong cau hinh lay ve tu ban sao"
echo "    DAT: chay duoc chi voi The phuc hoi, va lay lai duoc .env cu tu ban sao"

echo ""
echo "=== Kich ban 6b (Critical 2): --card --apply THAT tren mot may 'trang' ==="
# Mo phong dung kich ban tham hoa: may chu giao xu chay, chi con The phuc hoi. Nguoi van hanh cai
# lai bo ung dung tu GitHub (o day: he thong dang chay san), roi XOA HET so sach hien co de mo
# phong "ban cai moi tinh chua co du lieu", va nap du lieu cu ve CHI bang The phuc hoi:
#   - THU_MUC_CAU_HINH tro vao mot thu muc RONG (khong co backup.env -- dung nhu may trang),
#   - nen buoc sao luu bat buoc KHONG THE chay qua qlgx-runner.sh; script phai tu nhan ra
#     "khong co so sach nao de mat" va bo qua co ghi nhat ky, thay vi dung lai va bao loi.
psql_db "$TEN_DB" "DELETE FROM giao_dan; DELETE FROM gia_dinh; DROP TABLE IF EXISTS moc;" >/dev/null
[ "$(psql_db "$TEN_DB" "SELECT count(*) FROM giao_dan")" = "0" ] || that_bai "chua xoa duoc du lieu"
sn_truoc_6b=$(so_snapshot)
set +e
ra_6b=$(THU_MUC_CAU_HINH="$SCRATCH/etc-rong" QLGX_GOC_UNG_DUNG="$GOC_UNG_DUNG" \
        chay_restore --card "$SCRATCH/the-phuc-hoi.txt" --snapshot "$SN_GOC" --apply 2>&1)
ma_6b=$?
set -e
printf '%s\n' "$ra_6b" | grep -vE "^ Container" | tail -12
[ "$ma_6b" -eq 0 ] || that_bai "--card --apply THAT BAI tren may trang (day la ly do ton tai cua script)"
printf '%s\n' "$ra_6b" | grep -qi "BO QUA sao luu truoc phuc hoi" \
  || that_bai "khong thay ghi nhan ly do bo qua buoc sao luu tren may trang"
[ "$(so_snapshot)" = "$sn_truoc_6b" ] \
  || that_bai "da tao snapshot du khong co gi de sao luu (khong the, backup.env khong ton tai)"
[ "$(psql_db "$TEN_DB" "SELECT v FROM moc")" = "con-nguyen" ] \
  || that_bai "du lieu cu KHONG quay ve tren duong may trang"
[ "$(psql_db "$TEN_DB" "SELECT count(*) FROM giao_dan")" = "1" ] \
  || that_bai "so giao dan KHONG quay ve tren duong may trang"
cho_san_sang 180 || that_bai "API khong san sang sau khi phuc hoi tren duong may trang"
echo "    DAT: chi voi The phuc hoi va mot thu muc cau hinh RONG, du lieu giao xu da quay ve day du"

echo ""
echo "=== Kich ban 7: CSDL cu con trong han giu thi KHONG bi don ==="
danh_sach_db | grep -q "^${TEN_DB}_truoc_phuc_hoi_" \
  || that_bai "CSDL cu da bi xoa mat du chua qua han giu (mac dinh 7 ngay)"
echo "    DAT: $(danh_sach_db | grep -c "^${TEN_DB}_truoc_phuc_hoi_") CSDL 'truoc_phuc_hoi' van con"

echo ""
echo "==> Kiem tra dung luong dia sau khi chay"
sau_gb=$(dung_gb_trong)
echo "    Dia trong sau: ${sau_gb:-?} GB (truoc: ${truoc_gb:-?} GB)"
if [ -n "$sau_gb" ] && [ "$sau_gb" -lt "$NGUONG_DIA_TOI_THIEU_GB" ]; then
  echo "CANH BAO: dia tut duoi nguong ${NGUONG_DIA_TOI_THIEU_GB}GB -- se don dep ngay (trap EXIT)." >&2
fi

echo ""
echo "==> DAT TOAN BO: ke hoach khong doi gi; phuc hoi thanh cong va giu CSDL cu; that bai thi tu"
echo "    dao nguoc ve du lieu cu; snapshot hong khong pha he thong; dien tap sach; chay duoc"
echo "    chi voi The phuc hoi."
