#!/usr/bin/env bash
# Kiem thu dau-cuoi CUC BO cua install.sh (nua sau: Task 11) -- KHONG Docker-in-Docker.
#
# Ban dau brief Task 11 de nghi mot container Ubuntu --privileged tu cai Docker ben trong no
# (dind). Cach do bi BAC BO cho moi truong nay: may nay dung CHUNG voi 34 container that cua
# cac du an khac (khong duoc dung toi) va dia chi con ~40GB trong tren 440GB -- container
# --privileged tu cai dockerd rieng se nhan doi cache anh va co nguy co day dia. Ngoai ra tren
# Docker Desktop/WSL2, chi mount socket Docker cua host vao MOT container dieu phoi roi goi
# `docker compose` tu ben trong no (docker-outside-of-docker) van co bay duong dan that: bind
# mount tuong doi trong compose duoc daemon (chay trong VM WSL2 rieng) dich theo he quy chieu
# CUA CHINH DAEMON, khong khop duong dan trong container dieu phoi lan o dia Windows that --
# ket qua la mot thu muc RONG duoc tao nham thay vi dung tep cau hinh.
#
# Cach lam O DAY: goi Docker CLI native qua Git Bash TREN CHINH MAY NAY (khong container dieu
# phoi long nhau), dung dung cach cac du an khac tren may nay build/chay Docker that. Script
# nap install.sh CHI DE LAY HAM (QLGX_CHI_NAP_HAM=1), roi tu goi tung ham can kiem thu -- KHONG
# goi lai toan bo main() qua `curl | bash` vi buoc do can may Linux that (kiem_tra_tien_de doc
# /proc/meminfo, cai_phu_thuoc goi apt-get, cau_hinh_tuong_lua goi ufw/firewall-cmd) -- gioi han
# da biet cua moi truong phat trien Windows, se kiem chung that o VPS Linux khi trien khai.
#
# GIOI HAN bat buoc tuan thu khi chay script nay:
#   1. KHONG bind cong 80/443 THAT cua host (443 da bi container khac chiem). Dung mot tep
#      docker-compose.override.test.yml TAM trong thu muc scratch (khong commit) doi cong
#      Caddy sang 18080/18443 CHI trong luc test.
#   2. Khong kiem chung dau-cuoi cac lenh he dieu hanh that (xem tren) -- da co bats don vi cho
#      logic re nhanh cua chung o Task 10.
#   3. Don dep triet de sau khi chay: `dc down -v` roi `rm -rf` scratch dir; kiem tra dung
#      luong dia truoc/sau, DUNG NGAY neu tut duoi 15GB.
set -euo pipefail

readonly THU_MUC_WEBAPP="$(cd "$(dirname "$0")/../.." && pwd)"
# Cho phep doi scratch dir qua bien moi truong khi can, mac dinh dung mot thu muc rieng NGOAI
# cay lam viec that (co the co phien Claude khac dang sua o do -- xem CLAUDE.md).
readonly SCRATCH="${QLGX_E2E_SCRATCH:-D:/Working/QLGX/qlgx-e2e-scratch-t11}"
readonly NGUONG_DIA_TOI_THIEU_GB=15

dung_gb_trong() {
  df -BG --output=avail /d 2>/dev/null | tail -1 | tr -dc '0-9' \
    || df -BG --output=avail / 2>/dev/null | tail -1 | tr -dc '0-9'
}

# `dc` dinh nghia trong install.sh dung CO DINH hai tep overlay (docker-compose.yml +
# docker-compose.prod.yml). Sau khi source, dinh nghia LAI o day de THEM tep override test --
# khong dung sua docker-compose.prod.yml THAT (tep da cam ket trong Files cua task nay).
#
# `-p "$TEN_DU_AN_THU"` BAT BUOC: Docker Compose mac dinh lay TEN DU AN tu basename cua
# --project-directory. Moi ban scratch/debug cua kiem thu nay deu co thu muc con ten "WebApp"
# (chi khac duong dan cha), nen NEU KHONG dat ten rieng, hai lan chay khac nhau (hoac mot lan
# chay that va mot lan go tay de debug) se DUNG CHUNG mot ten du an "webapp" -> dung chung volume
# "webapp_qlgx-postgres-data"/network "webapp_default". Da tung gap THAT: mot volume Postgres
# con sot lai tu mot phien go tay truoc bi mot lan chay khac tai su dung, khien lan chay do
# ket noi vao DU LIEU CU (mat khau vai tro qlgx_admin trong volume cu KHAC voi mat khau vua
# sinh trong .env moi) -> "password authentication failed" -- trong khi install.sh THAT tren
# VPS khong gap loi nay (moi VPS chi co DUNG MOT ban /opt/qlgx/WebApp, ten du an luon on dinh).
TEN_DU_AN_THU="qlgx-e2e-test-$$"
dc_thu() {
  docker compose -p "$TEN_DU_AN_THU" --project-directory "$SCRATCH/WebApp" \
    -f "$SCRATCH/WebApp/docker-compose.yml" \
    -f "$SCRATCH/WebApp/docker-compose.prod.yml" \
    -f "$SCRATCH/WebApp/docker-compose.override.test.yml" "$@"
}

don_dep() {
  echo "==> Don dep: dung/xoa container, image, va scratch dir cua chinh kiem thu nay"
  dc_thu down -v >/dev/null 2>&1 || true
  # `down -v` xoa container/volume nhung KHONG xoa image da build -- image co Chromium (~2.5GB)
  # se nam lai vinh vien tren dia neu khong xoa tay o day. Ten image mang tien to TEN_DU_AN_THU
  # (vi co `-p` o dc_thu -- xem ly do o dinh nghia TEN_DU_AN_THU). KHONG dung
  # `docker builder prune`/`docker system prune` (cam trong brief -- anh huong container/cache
  # cua du an khac tren may nay); chi xoa DUNG MOT image do chinh kiem thu nay build.
  docker rmi "${TEN_DU_AN_THU}-api:latest" >/dev/null 2>&1 || true
  rm -rf "$SCRATCH"
}
trap don_dep EXIT

echo "==> Kiem tra dung luong dia truoc khi chay"
truoc_gb=$(dung_gb_trong)
echo "    Dia trong truoc: ${truoc_gb:-?} GB"
if [ -n "$truoc_gb" ] && [ "$truoc_gb" -lt "$NGUONG_DIA_TOI_THIEU_GB" ]; then
  echo "BLOCKED: dia da duoi nguong ${NGUONG_DIA_TOI_THIEU_GB}GB TRUOC khi chay -- dung lai." >&2
  exit 1
fi

echo "==> Chuan bi thu muc scratch: $SCRATCH (sao chep tu cay lam viec hien tai, KHONG clone qua mang)"
rm -rf "$SCRATCH"
mkdir -p "$SCRATCH"
cp -r "$THU_MUC_WEBAPP" "$SCRATCH/WebApp"

# --- Tep override CHI de test: doi cong Caddy sang 18080/18443, KHONG cham docker-compose.prod.yml that.
# Dung "!override" (khong phai "!reset") cho khoa ports: neu chi de danh sach thuong, Compose se
# NOI danh sach cua ba tep lai (80:80 va 18080:80 cung ton tai) -- van bind ca cong that 80/443.
cat > "$SCRATCH/WebApp/docker-compose.override.test.yml" <<'EOF'
services:
  caddy:
    ports: !override
      - "18080:80"
      - "18443:443"
EOF

echo "==> Nap install.sh CHI DE LAY HAM (QLGX_CHI_NAP_HAM=1), tro moi thu muc trang thai vao scratch"
export GOC_CHECKOUT="$SCRATCH"
export GOC_UNG_DUNG="$SCRATCH/WebApp"
export THU_MUC_CAU_HINH="$SCRATCH/etc-qlgx"
export THU_MUC_SPOOL="$SCRATCH/spool"
export THU_MUC_LOG="$SCRATCH/log"
export QLGX_CHI_NAP_HAM=1
# shellcheck source=/dev/null
source "$SCRATCH/WebApp/scripts/install.sh"
# Ghi de dc() SAU khi nap: them tep override test vao chuoi -f (xem ly do o tren).
dc() { dc_thu "$@"; }

KHONG_TUONG_TAC=1   # khong doc /dev/tty trong hoi_hoac_bien -- moi bien can thiet da co qua env

mkdir -p "$THU_MUC_CAU_HINH" "$THU_MUC_SPOOL" "$THU_MUC_LOG"
chmod 700 "$THU_MUC_CAU_HINH" 2>/dev/null || true

# Bien moi truong gia lap dung nhu install-e2e cua brief goc: R2 gia (bo qua that su goi restic
# qua QLGX_BO_QUA_R2=1), mot giao xu thu nghiem, mot tai khoan quan tri he thong thu nghiem.
export QLGX_R2_ENDPOINT=http://khong-dung-that
export QLGX_R2_BUCKET=thu
export QLGX_R2_ACCESS_KEY_ID=k
export QLGX_R2_SECRET_ACCESS_KEY=s
export QLGX_GIAO_XU_TEN="Giao xu Thu Nghiem"
export QLGX_ADMIN_TEN_TAI_KHOAN=quantri_thu
export QLGX_ADMIN_MAT_KHAU=MatKhauThu12345
export QLGX_ADMIN_HO_TEN="Nguoi thu nghiem"
export QLGX_BO_QUA_R2=1

echo "==> Buoc 1: sinh_env + ghi_backup_env (lan 1 -- cai moi)"
sinh_env "$GOC_UNG_DUNG/.env"
ghi_backup_env "$THU_MUC_CAU_HINH/backup.env"

echo "==> Buoc 2: bat_postgres"
bat_postgres

echo "==> Buoc 3: tao_vai_tro_rls"
tao_vai_tro_rls

echo "==> Buoc 4: bat_api (build image mat vai phut -- co Chromium, xem Dockerfile)"
bat_api

echo "==> Buoc 5: khoi_tao_giao_xu_va_admin (lan 1)"
khoi_tao_giao_xu_va_admin

echo "==> Buoc 6: cau_hinh_https KHONG co ten mien (phai canh bao HTTP thuan, Caddyfile cu phap dung)"
cau_hinh_https
if ! grep -q '^:80 {' "$GOC_UNG_DUNG/Caddyfile"; then
  echo "THAT BAI: Caddyfile khong co khoi :80 khi khong co ten mien (xem $GOC_UNG_DUNG/Caddyfile)" >&2
  cat "$GOC_UNG_DUNG/Caddyfile" >&2
  exit 1
fi
if grep -Eq '^\{[[:space:]]*$|^[[:space:]]*\{[[:space:]]*$' "$GOC_UNG_DUNG/Caddyfile"; then
  echo "THAT BAI: Caddyfile co ve co khoi domain RONG (loi cu phap)" >&2
  exit 1
fi
# Xac nhan cu phap Caddyfile hop le bang chinh binh Caddy (validate, khong can chay that).
# MSYS_NO_PATHCONV=1: tren Git Bash/MSYS, mot doi so CLI trong "/etc/..." bi TU DONG dich thanh
# duong dan Windows that (vd "C:/Program Files/Git/etc/..."), lam docker exec truyen sai duong
# dan VAO TRONG container Linux. Chi anh huong doi so dong lenh (nhu o day), KHONG anh huong
# duong dan trong docker-compose*.yml (do Compose tu doc file, khong qua co che dich argv nay).
if ! MSYS_NO_PATHCONV=1 dc exec -T caddy caddy validate --config /etc/caddy/Caddyfile >/tmp/qlgx-caddy-validate.log 2>&1; then
  echo "THAT BAI: Caddy tu bao Caddyfile KHONG hop le cu phap:" >&2
  cat /tmp/qlgx-caddy-validate.log >&2
  exit 1
fi
echo "    Caddyfile hop le, dung HTTP thuan khi khong co ten mien -- DAT"

echo "==> Buoc 7: bang tu kiem chung LAN 1 (truoc khi pha gi)"
truoc_ket_qua=$(tu_kiem_chung 2>&1) && ma_truoc=0 || ma_truoc=$?
echo "$truoc_ket_qua"
echo "    Ma thoat lan 1: $ma_truoc"

so_khong_dat_truoc=$(printf '%s\n' "$truoc_ket_qua" | grep -c '^  KHONG DAT' || true)
echo "    So dong KHONG DAT lan 1: $so_khong_dat_truoc"

# GHI CHU MOI TRUONG: hai muc "quyen 600" (.env va backup.env) LUON bao KHONG DAT tren may nay
# vi chmod tren Git Bash/NTFS khong dat duoc bit quyen Unix that (da kiem chung rieng: chmod 600
# roi stat -c '%a' tra ve 644, khong phai loi cua install.sh) -- day la GIOI HAN MOI TRUONG WINDOWS
# da neu trong task brief, khong phai loi logic. Vi vay o day CHI doi hoi: khong co muc nao KHAC
# ngoai hai muc quyen do bi bao KHONG DAT o lan chay dau tien (tuc moi dieu kien THAT deu DAT).
cac_dong_khong_dat_truoc=$(printf '%s\n' "$truoc_ket_qua" | grep '^  KHONG DAT' || true)
so_muc_la_truoc=$(printf '%s\n' "$cac_dong_khong_dat_truoc" \
  | grep -Ev 'quyen 600' | grep -c '.' || true)
if [ "$so_muc_la_truoc" -ne 0 ]; then
  echo "THAT BAI: co muc KHONG DAT khong giai thich duoc (khong phai quyen tep) o lan kiem chung dau tien:" >&2
  printf '%s\n' "$cac_dong_khong_dat_truoc" | grep -Ev 'quyen 600' >&2
  exit 1
fi
echo "    Xac nhan: moi dieu kien THAT (postgres/api/RLS/vai-tro/PDF/khong-lo-khoa-R2) deu DAT."

echo "==> Buoc 8: chay lai lan 2 -- phai idempotent, KHONG doi bi mat nao"
truoc_hash=$(sha256sum "$GOC_UNG_DUNG/.env" "$THU_MUC_CAU_HINH/backup.env")
sinh_env "$GOC_UNG_DUNG/.env"
ghi_backup_env "$THU_MUC_CAU_HINH/backup.env"
sau_hash=$(sha256sum "$GOC_UNG_DUNG/.env" "$THU_MUC_CAU_HINH/backup.env")
[ "$truoc_hash" = "$sau_hash" ] || { echo "THAT BAI: chay lai da doi bi mat trong .env/backup.env" >&2; exit 1; }
echo "    .env va backup.env khong doi -- DAT"

# Chi lay CAC DONG BANG (bo qua dong ghi_log co timestamp doi moi lan goi) de so sanh giua hai
# lan chay tu_kiem_chung.
dong_bang() { printf '%s\n' "$1" | grep -E '^  (DAT|KHONG DAT)'; }

echo "==> Buoc 8b: goi lai CA BON ham dung dich vu (tao_vai_tro_rls, bat_postgres, bat_api, cau_hinh_https) -- phai idempotent"
tao_vai_tro_rls
bat_postgres
bat_api
cau_hinh_https
ket_qua_sau_8b=$(tu_kiem_chung 2>&1) && ma_8b=0 || ma_8b=$?
echo "$ket_qua_sau_8b"
if [ "$(dong_bang "$ket_qua_sau_8b")" != "$(dong_bang "$truoc_ket_qua")" ]; then
  echo "THAT BAI: goi lai 4 ham dung dich vu lam bang tu kiem chung khac di so voi lan dau (khong idempotent)" >&2
  diff <(dong_bang "$truoc_ket_qua") <(dong_bang "$ket_qua_sau_8b") >&2 || true
  exit 1
fi
echo "    Goi lai tao_vai_tro_rls/bat_postgres/bat_api/cau_hinh_https khong loi, bang tu kiem chung KHONG DOI -- DAT"

echo "==> Buoc 9: khoi_tao_giao_xu_va_admin LAN 2 -- cung ten tai khoan phai coi la THANH CONG (idempotent)"
# Idempotent duoc kiem qua TRANG THAI THAT cua CSDL (Loi 5 vong review 1: khong con so khop
# chuoi hien thi tieng Viet khong dau trong log CLI, thu de vo hieu neu ai do sua lai chinh ta).
if ! khoi_tao_giao_xu_va_admin; then
  echo "THAT BAI: goi lai khoi_tao_giao_xu_va_admin voi cung ten tai khoan phai duoc coi la thanh cong (bo qua), khong duoc thoat loi" >&2
  exit 1
fi
if ! grep -q "da san sang trong CSDL" /tmp/qlgx-tao-admin.log; then
  echo "CANH BAO: khong thay dong xac nhan CSDL trong log lan 2 -- kiem tra lai duong nhanh idempotent" >&2
fi
echo "    Goi lai voi cung ten tai khoan: CSDL xac nhan tai khoan ton tai, duoc coi la thanh cong -- DAT"

echo "==> Buoc 10: in_the_phuc_hoi -- phai co du thong tin de phuc hoi tu may trang"
in_the_phuc_hoi >/tmp/qlgx-the-phuc-hoi.out
tep_the="$THU_MUC_CAU_HINH/the-phuc-hoi.txt"
[ -f "$tep_the" ] || { echo "THAT BAI: khong thay tep the phuc hoi $tep_the" >&2; exit 1; }
thieu=0
for tu_khoa in "MAT KHAU RESTIC" "KHO SAO LUU" "R2 KEY ID" "R2 SECRET" "postgres " "qlgx_app " "qlgx_admin "; do
  grep -q "$tu_khoa" "$tep_the" || { echo "THIEU trong the phuc hoi: $tu_khoa" >&2; thieu=1; }
done
[ "$thieu" -eq 0 ] || { echo "THAT BAI: the phuc hoi thieu thong tin can de phuc hoi tu may trang" >&2; exit 1; }
echo "    The phuc hoi co du mat khau restic, kho sao luu, khoa R2, va ca ba bo mat khau CSDL -- DAT"

echo "==> Buoc 10b: pha DUNG MOT thuoc tinh that (BYPASSRLS tren vai tro nghiep vu) -- CSDL VAN SONG"
# Day la kich ban nguy hiem THAT: vai tro nghiep vu vo tinh co BYPASSRLS trong khi moi thu khac
# van chay binh thuong -- khac han voi "dung ca postgres" o Buoc 11 (lam ca chum muc doi trang
# thai theo day chuyen vi mat ket noi CSDL, khong chi rieng muc kiem vai tro). Phai chung minh
# bang tu kiem chung phan biet duoc DUNG loai loi, khong chi phan biet "CSDL song hay chet".
app_user_that=$(doc_env_kv "$GOC_UNG_DUNG/.env" QLGX_APP_DB_USER)
pg_user_that=$(doc_env_kv "$GOC_UNG_DUNG/.env" POSTGRES_USER)
pg_db_that=$(doc_env_kv "$GOC_UNG_DUNG/.env" POSTGRES_DB)
dc exec -T postgres psql -v ON_ERROR_STOP=1 -U "$pg_user_that" -d "$pg_db_that" \
  -c "ALTER ROLE \"$app_user_that\" BYPASSRLS;" >/dev/null

ket_qua_bypass=$(tu_kiem_chung 2>&1) && ma_bypass=0 || ma_bypass=$?
echo "$ket_qua_bypass"
[ "$ma_bypass" -ne 0 ] || { echo "THAT BAI: tu_kiem_chung van tra ve 0 sau khi bat BYPASSRLS tren vai tro nghiep vu" >&2; exit 1; }

khac_biet_bypass=$(diff <(dong_bang "$truoc_ket_qua") <(dong_bang "$ket_qua_bypass") || true)
echo "    Khac biet so voi truoc khi pha (phai DUNG hai dong: 1 cu, 1 moi, cung mot muc):"
echo "$khac_biet_bypass"
so_dong_khac_bypass=$(printf '%s\n' "$khac_biet_bypass" | grep -cE '^[<>]  ' || true)
if [ "$so_dong_khac_bypass" -ne 2 ]; then
  echo "THAT BAI: bat BYPASSRLS phai lam DUNG MOT dong doi trang thai (2 dong diff: cu+moi), thuc te $so_dong_khac_bypass dong khac -- ro ri sang cac muc khac (co the van con bay fail-open khac)" >&2
  exit 1
fi
printf '%s\n' "$khac_biet_bypass" | grep -q 'Vai tro nghiep vu KHONG co BYPASSRLS' \
  || { echo "THAT BAI: dong doi trang thai KHONG phai la 'Vai tro nghiep vu KHONG co BYPASSRLS' -- bang kiem chung bat sai muc" >&2; exit 1; }
printf '%s\n' "$ket_qua_bypass" | grep -q '^  KHONG DAT     Vai tro nghiep vu KHONG co BYPASSRLS' \
  || { echo "THAT BAI: sau khi bat BYPASSRLS, dong 'Vai tro nghiep vu KHONG co BYPASSRLS' phai la KHONG DAT" >&2; exit 1; }
echo "    Xac nhan: DUNG MOT dong ('Vai tro nghiep vu KHONG co BYPASSRLS') chuyen sang KHONG DAT, moi dong khac giu nguyen -- CSDL van song, bang kiem chung bat DUNG loai loi -- DAT"

echo "    Khoi phuc lai vai tro dung (goi lai tao_vai_tro_rls -- cung la kiem idempotent bo sung cho Buoc 8b)"
tao_vai_tro_rls
ket_qua_sau_khoi_phuc=$(tu_kiem_chung 2>&1) && ma_khoi_phuc=0 || ma_khoi_phuc=$?
if [ "$(dong_bang "$ket_qua_sau_khoi_phuc")" != "$(dong_bang "$truoc_ket_qua")" ]; then
  echo "THAT BAI: sau khi goi lai tao_vai_tro_rls, bang tu kiem chung khong tro ve dung trang thai ban dau" >&2
  diff <(dong_bang "$truoc_ket_qua") <(dong_bang "$ket_qua_sau_khoi_phuc") >&2 || true
  exit 1
fi
echo "    Da khoi phuc dung vai tro RLS -- bang tu kiem chung tro ve y het truoc khi pha -- DAT"

echo "==> Buoc 11: bang tu kiem chung PHAI BIET BAO LOI -- pha mot dieu kien that (dung container postgres)"
dc stop postgres >/dev/null

set +e
sau_ket_qua=$(tu_kiem_chung 2>&1)
ma_sau=$?
set -e
echo "$sau_ket_qua"
echo "    Ma thoat sau khi dung postgres: $ma_sau"

[ "$ma_sau" -ne 0 ] || { echo "THAT BAI: tu_kiem_chung van tra ve ma thoat 0 sau khi dung postgres" >&2; exit 1; }

if ! printf '%s\n' "$sau_ket_qua" | grep -q '^  KHONG DAT.*Container postgres dang chay'; then
  echo "THAT BAI: bang tu kiem chung KHONG bao 'Container postgres dang chay' la KHONG DAT du postgres da dung" >&2
  exit 1
fi
if ! printf '%s\n' "$sau_ket_qua" | grep -q '^  KHONG DAT.*API tra ve san sang'; then
  echo "THAT BAI: bang tu kiem chung KHONG bao 'API tra ve san sang' la KHONG DAT du postgres da dung (API can CSDL de tra loi /san-sang)" >&2
  exit 1
fi

so_khong_dat_sau=$(printf '%s\n' "$sau_ket_qua" | grep -c '^  KHONG DAT' || true)
echo "    So dong KHONG DAT: truoc=$so_khong_dat_truoc, sau=$so_khong_dat_sau"
if [ "$so_khong_dat_sau" -le "$so_khong_dat_truoc" ]; then
  echo "THAT BAI: so muc KHONG DAT khong tang len sau khi pha dieu kien postgres" >&2
  exit 1
fi
echo "    Bang tu kiem chung BIET BAO LOI: them dung cac muc lien quan postgres/API, ma thoat khac 0 -- DAT"

echo "==> Buoc 11b: dung LUON ca container api -- xac nhan muc 'KHONG biet khoa R2' KHONG con fail-open (Loi 1 vong review 1)"
# Truoc khi sua: '! dc exec ... | grep' fail-open khi dc exec that bai (container khong chay) --
# pipeline that bai, dau "!" dao thanh 0, in DAT du chang kiem duoc gi. Dung ca api de dc exec
# CHAC CHAN that bai, roi xac nhan muc nay bao dung KHONG DAT (khong con bao nham DAT).
dc stop api >/dev/null

set +e
ket_qua_ca_hai_dung=$(tu_kiem_chung 2>&1)
ma_ca_hai_dung=$?
set -e
echo "$ket_qua_ca_hai_dung"
[ "$ma_ca_hai_dung" -ne 0 ] || { echo "THAT BAI: tu_kiem_chung van tra 0 sau khi dung ca postgres lan api" >&2; exit 1; }

if ! printf '%s\n' "$ket_qua_ca_hai_dung" | grep -q '^  KHONG DAT     Container api dang chay'; then
  echo "THAT BAI: 'Container api dang chay' khong bao KHONG DAT du da dung container api" >&2
  exit 1
fi
if ! printf '%s\n' "$ket_qua_ca_hai_dung" | grep -q '^  KHONG DAT     Container API KHONG biet khoa R2'; then
  echo "THAT BAI: LOI 1 CHUA SUA -- 'Container API KHONG biet khoa R2' phai bao KHONG DAT khi khong doc duoc bien moi truong cua container api (dc exec that bai), dong thuc te:" >&2
  printf '%s\n' "$ket_qua_ca_hai_dung" | grep 'Container API KHONG biet khoa R2' >&2
  exit 1
fi
echo "    Xac nhan Loi 1 DA SUA: dc exec that bai (api dung) -> muc R2 bao dung KHONG DAT, khong con fail-open thanh DAT -- DAT"

echo "==> Kiem tra dung luong dia sau khi chay"
sau_gb=$(dung_gb_trong)
echo "    Dia trong sau: ${sau_gb:-?} GB (truoc: ${truoc_gb:-?} GB)"
if [ -n "$sau_gb" ] && [ "$sau_gb" -lt "$NGUONG_DIA_TOI_THIEU_GB" ]; then
  echo "CANH BAO: dia da tut duoi nguong ${NGUONG_DIA_TOI_THIEU_GB}GB sau khi chay -- se don dep ngay (trap EXIT)." >&2
fi

echo "==> DAT: install.sh (nua sau) dung duoc dich vu, idempotent, The phuc hoi day du, va bang tu kiem chung BIET BAO LOI."
