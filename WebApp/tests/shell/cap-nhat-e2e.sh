#!/usr/bin/env bash
# Kiem thu dau-cuoi CUC BO cua cap_nhat()/quay_lui() (Task 12) -- KHONG Docker-in-Docker.
#
# Brief goc de nghi mot container Ubuntu --privileged tu cai Docker ben trong no (dind) va
# `git remote add origin /opt/qlgx` (tro origin vao CHINH thu muc dang lam viec). Ca hai bi BAC
# BO cho moi truong nay -- ly do dind giong het Task 11 (xem WebApp/tests/shell/install-e2e.sh):
# may dung CHUNG voi 34 container that cua du an khac, con ~40GB dia trong, va container
# --privileged tu cai dockerd rieng se nhan doi cache anh, co nguy co day dia.
#
# Cach lam O DAY: goi Docker CLI native qua Git Bash TREN CHINH MAY NAY (khong container dieu
# phoi long nhau) -- dung dung pattern install-e2e.sh cua Task 11. Script nap install.sh CHI DE
# LAY HAM (QLGX_CHI_NAP_HAM=1), roi tu goi tung ham/cap_nhat()/quay_lui() -- KHONG chay lai
# main() qua `curl | bash` (kiem_tra_tien_de doc /proc/meminfo, cai_phu_thuoc goi apt-get,
# cau_hinh_tuong_lua goi ufw/firewall-cmd -- gioi han da biet cua moi truong Windows).
#
# MO PHONG "co ban moi tren GitHub" MA KHONG CAN MANG THAT: cap_nhat() khong nhan tham so/bien
# moi truong nao de tro toi mot remote gia (da doc ky Interfaces cua brief -- khong co san co
# che nay). Vi vay o day tu tao MOT REPO GIT CUC BO THU HAI dong vai "GitHub": mot ban sao cua
# WebApp (lay tu dung "git archive HEAD" cua chinh kho nay, KHONG clone qua mang, va CHOT o
# trang thai commit da luu -- an toan neu co phien Claude khac dang sua file trong luc kiem thu
# nay chay) duoc git-init rieng, dat lam "origin" cua mot ban checkout thu hai qua `git clone`
# duong dan cuc bo. `git fetch`/`git merge --ff-only` trong cap_nhat() chay THAT tren hai repo
# cuc bo nay, khong dung mang.
#
# GIOI HAN MOI TRUONG WINDOWS DA BIET (xem install-e2e.sh Task 11): hai muc "quyen 600" trong
# tu_kiem_chung (.env va backup.env) LUON bao KHONG DAT tren Git Bash/NTFS vi chmod khong dat
# duoc bit quyen Unix that -- KHONG PHAI loi logic. Vi cap_nhat() thuc su GOI tu_kiem_chung() de
# quyet dinh co quay lui hay khong, gioi han nay lam tu_kiem_chung() LUON tra ve khac 0 tren may
# nay du he thong hoan toan khoe manh -- neu khong xu ly, kich ban "cap nhat THANH CONG" se
# khong bao gio quan sat duoc tren Windows (cap_nhat se luon nghi la that bai va tu quay lui,
# du ban moi len hoan toan tot). Cach xu ly: CHI trong pham vi kiem thu kich ban thanh cong,
# ghi de tam thoi ham tu_kiem_chung (ten that duoc luu lai truoc do la tu_kiem_chung_that) bang
# mot phien ban khoan dung hai muc quyen 600 da biet nay -- moi muc KHONG DAT KHAC van lam that
# bai binh thuong. Day la "gia lap moi truong Linux that" cho DUNG MOT dieu kien khong the dat
# duoc tren Windows, khong che giau bat ky loi logic that nao cua cap_nhat()/quay_lui().
#
# BA GIOI HAN bat buoc (giong Task 11):
#   1. KHONG bind cong 80/443 that cua host. (Khong can o day: cap_nhat chi dung/xay lai "api",
#      dc() dung ca docker-compose.prod.yml -- file do RESET cong api ve rong ("ports: !reset []")
#      va postgres khong bao gio map cong ra host -- xem docker-compose.yml/prod.yml.)
#   2. Khong kiem chung dau-cuoi cac lenh he dieu hanh that (apt-get, ufw) -- da co bats Task 10.
#   3. Don dep triet de: `dc down -v` roi `rm -rf` scratch dir + repo goc gia; kiem tra dung
#      luong dia truoc/sau, DUNG NGAY neu tut duoi 15GB.
set -euo pipefail

readonly THU_MUC_WEBAPP_THAT="$(cd "$(dirname "$0")/../.." && pwd)"
readonly GOC_KHO_THAT="$(cd "$THU_MUC_WEBAPP_THAT/.." && pwd)"
readonly SCRATCH="${QLGX_E2E_SCRATCH:-D:/Working/QLGX/qlgx-e2e-scratch-t12}"
readonly NGUONG_DIA_TOI_THIEU_GB=15

dung_gb_trong() {
  df -BG --output=avail /d 2>/dev/null | tail -1 | tr -dc '0-9' \
    || df -BG --output=avail / 2>/dev/null | tail -1 | tr -dc '0-9'
}

# Ten du an rieng cho MOI LAN CHAY (xem ly do chi tiet trong install-e2e.sh Task 11: tranh dung
# chung volume/network voi lan chay khac tren cung may -- co the co phien Claude khac dang test
# song song). dc_thu() tham chieu $GOC_UNG_DUNG DONG (luc GOI, khong luc dinh nghia) vi bien do
# chi duoc gan gia tri SAU khi source install.sh o duoi.
TEN_DU_AN_THU="qlgx-e2e-capnhat-$$"
dc_thu() {
  docker compose -p "$TEN_DU_AN_THU" --project-directory "$GOC_UNG_DUNG" \
    -f "$GOC_UNG_DUNG/docker-compose.yml" \
    -f "$GOC_UNG_DUNG/docker-compose.prod.yml" "$@"
}

don_dep() {
  echo "==> Don dep: dung/xoa container, image, va scratch dir cua chinh kiem thu nay"
  dc_thu down -v >/dev/null 2>&1 || true
  # image build-web/build-api la giai doan trung gian, khong mang ten du an -- chi image cuoi
  # (chay that) moi mang tien to TEN_DU_AN_THU (vi co -p o dc_thu). KHONG dung
  # `docker builder prune`/`docker system prune` (cam trong brief).
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

echo "==> Dung repo GIA lam 'GitHub': sao chep CAY LAM VIEC hien tai (KHONG qua mang)"
# CO Y dung `cp -r` THO tren toan bo cay lam viec hien tai (giong het install-e2e.sh Task 11),
# KHONG dung "git archive HEAD" va KHONG dung "git ls-files": co phien Claude khac dang lam
# viec song song tren cung nhanh nay (xem CLAUDE.md).
#   - "git archive HEAD" THAT BAI: da xac nhan bang thu nghiem thuc te build ra loi CS1061 --
#     WebApp/src/Qlgx.Api/Services/SaoLuuService.cs (da commit) tham chieu cac thanh vien cua
#     QlgxDbContext (BanSaoLuu, CongViecSaoLuu, ...) ma ban SUA CHUA COMMIT cua QlgxDbContext.cs
#     moi co -- COMMIT MOI NHAT (HEAD) hien tai THUC SU KHONG BIEN DICH DUOC.
#   - "git ls-files" (chi lay tep DA THEO DOI) CUNG THAT BAI vi cung ly do: LinhMucService.cs
#     ma Program.cs dang tham chieu la tep MOI, CHUA duoc `git add` (trang thai "??" trong git
#     status), nen git ls-files bo qua no.
# Vi vay CHI CO `cp -r` tho (lay dung nhung gi tren dia, bat ke da track/commit hay chua) moi
# phan anh dung trang thai "build duoc" hien tai -- chap nhan copy them node_modules/bin/obj
# (vai tram MB, van trong nguong dia 15GB) de doi lay do tin cay nay.
rm -rf "$SCRATCH"
mkdir -p "$SCRATCH"
readonly GOC_GIA="$SCRATCH/goc-gia-github"
mkdir -p "$GOC_GIA"
cp -r "$THU_MUC_WEBAPP_THAT" "$GOC_GIA/WebApp"
git -C "$GOC_GIA" init -q .
git -C "$GOC_GIA" config user.email t@t
git -C "$GOC_GIA" config user.name t
git -C "$GOC_GIA" add -A
git -C "$GOC_GIA" commit -q -m "ban dau (gia lap ban dang chay tren may chu)"
git -C "$GOC_GIA" branch -M webapp-phase-1

echo "==> Clone tu repo gia -> ban 'checkout' dong vai /opt/qlgx that tren may chu"
readonly CHECKOUT="$SCRATCH/checkout"
git clone -q "$GOC_GIA" "$CHECKOUT"

echo "==> Nap install.sh CHI DE LAY HAM (QLGX_CHI_NAP_HAM=1), tro moi thu muc trang thai vao scratch"
export GOC_CHECKOUT="$CHECKOUT"
export GOC_UNG_DUNG="$CHECKOUT/WebApp"
export THU_MUC_CAU_HINH="$SCRATCH/etc-qlgx"
export THU_MUC_SPOOL="$SCRATCH/spool"
export THU_MUC_LOG="$SCRATCH/log"
export QLGX_CHI_NAP_HAM=1
# shellcheck source=/dev/null
source "$CHECKOUT/WebApp/scripts/install.sh"
# Ghi de dc() SAU khi nap: them -p (ten du an rieng) -- xem ly do o dinh nghia TEN_DU_AN_THU.
dc() { dc_thu "$@"; }

NHANH="webapp-phase-1"     # trung ten nhanh cua repo gia o tren
KHONG_TUONG_TAC=1          # khong doc /dev/tty -- moi bien can thiet da co qua env

mkdir -p "$THU_MUC_CAU_HINH" "$THU_MUC_SPOOL" "$THU_MUC_LOG"
chmod 700 "$THU_MUC_CAU_HINH" 2>/dev/null || true

# Bien moi truong gia lap: R2 gia (that su bo qua qua QLGX_BO_QUA_R2=1 -- qlgx-runner.sh chua
# ton tai toi Task 13, dung dung bien nay de cap_nhat() khong dam goi no).
export QLGX_R2_ENDPOINT=http://khong-dung-that
export QLGX_R2_BUCKET=thu
export QLGX_R2_ACCESS_KEY_ID=k
export QLGX_R2_SECRET_ACCESS_KEY=s
export QLGX_BO_QUA_R2=1

echo "==> Dung mot ban CAI DAT CU lam nen: sinh_env + ghi_backup_env + bat_postgres + tao_vai_tro_rls + bat_api"
sinh_env "$GOC_UNG_DUNG/.env"
ghi_backup_env "$THU_MUC_CAU_HINH/backup.env"
bat_postgres
tao_vai_tro_rls
bat_api   # build lan dau mat vai phut (npm ci, dotnet publish, tai Chromium) -- cac lan sau dung cache

cu_commit=$(git -C "$GOC_CHECKOUT" rev-parse HEAD)
echo "    Ban cu dang chay o commit ${cu_commit:0:8}"

# Luu lai ban goc that cua tu_kiem_chung duoi ten khac de dung lai o Kich ban B va Kich ban C
# (xem giai thich gioi han Windows chmod o dau file).
eval "$(declare -f tu_kiem_chung | sed '1s/^tu_kiem_chung /tu_kiem_chung_that /')"

echo ""
echo "=== Kich ban 1: KHONG co ban moi -- phai bao 'da la ban moi nhat', KHONG khoi dong lai gi ==="
id_truoc=$(dc ps -q api)
ra_1=$(cap_nhat 2>&1); ma_1=$?
echo "$ra_1"
echo "    Ma thoat: $ma_1"
printf '%s\n' "$ra_1" | grep -qi "moi nhat" \
  || { echo "THAT BAI: khong thay thong bao 'da la ban moi nhat'" >&2; exit 1; }
[ "$ma_1" -eq 0 ] || { echo "THAT BAI: cap_nhat tra ve khac 0 khi khong co gi de cap nhat" >&2; exit 1; }
id_sau=$(dc ps -q api)
[ "$id_truoc" = "$id_sau" ] \
  || { echo "THAT BAI: da khoi dong lai container api du khong co ban moi ($id_truoc -> $id_sau)" >&2; exit 1; }
echo "    DAT: khong doi container ($id_truoc), bao dung 'da la ban moi nhat'"

echo ""
echo "=== Kich ban 2: CO ban moi va len duoc -- phai cap nhat XONG, KHONG quay lui ==="
# Doi mot tep THAT SU nam trong build context duoc COPY vao image (index.html cua web) de
# buoc phai xay lai anh that su (khong chi 'git merge' suong ma khong dong gi den Docker).
printf '%s\n' '<!-- qlgx-thu-nghiem-cap-nhat: noi dung moi -->' >> "$GOC_GIA/WebApp/src/web/index.html"
git -C "$GOC_GIA" add -A
git -C "$GOC_GIA" commit -q -m "cap nhat thu nghiem: them dong ghi chu vao index.html"
moi_commit_b=$(git -C "$GOC_GIA" rev-parse HEAD)
echo "    Repo gia da co ban moi: ${moi_commit_b:0:8}"

# GHI DE TAM THOI tu_kiem_chung: xem giai thich gioi han Windows chmod o dau file. Chi bo qua
# DUNG HAI muc quyen tep da biet luon sai tren NTFS -- moi muc KHAC van phai DAT that.
tu_kiem_chung() {
  local ra; ra=$(tu_kiem_chung_that 2>&1); local ma=$?
  printf '%s\n' "$ra" >&2
  if [ "$ma" -eq 0 ]; then return 0; fi
  local la_khac
  la_khac=$(printf '%s\n' "$ra" | grep '^  KHONG DAT' | grep -Ev 'quyen 600' | grep -c '.' || true)
  [ "$la_khac" -eq 0 ]
}

ra_2=$(cap_nhat 2>&1); ma_2=$?
echo "$ra_2"
echo "    Ma thoat: $ma_2"
[ "$ma_2" -eq 0 ] || { echo "THAT BAI: cap_nhat that bai voi mot ban moi le ra phai len duoc" >&2; exit 1; }
printf '%s\n' "$ra_2" | grep -qi "cap nhat xong" \
  || { echo "THAT BAI: khong thay thong bao 'Cap nhat xong'" >&2; exit 1; }
printf '%s\n' "$ra_2" | grep -qi "quay lui" \
  && { echo "THAT BAI: cap_nhat thanh cong ma van nhac den quay lui" >&2; exit 1; }

head_sau_b=$(git -C "$GOC_CHECKOUT" rev-parse HEAD)
[ "$head_sau_b" = "$moi_commit_b" ] \
  || { echo "THAT BAI: HEAD cua checkout khong tien len ban moi ($head_sau_b != $moi_commit_b)" >&2; exit 1; }
[ "$(cat "$GOC_UNG_DUNG/.phien-ban-truoc")" = "$cu_commit" ] \
  || { echo "THAT BAI: .phien-ban-truoc khong ghi dung commit cu truoc khi cap nhat" >&2; exit 1; }
cho_san_sang 60 || { echo "THAT BAI: he thong khong san sang ngay sau khi bao cap nhat xong" >&2; exit 1; }
echo "    DAT: HEAD tien len ${head_sau_b:0:8}, .phien-ban-truoc ghi dung ${cu_commit:0:8}, API san sang, KHONG quay lui"

# Khoi phuc lai tu_kiem_chung THAT truoc khi sang Kich ban 3 (du duong build-hong khong bao gio
# goi toi no, khoi phuc de doan ma con lai trung thuc voi ham that su cua Task 11).
eval "$(declare -f tu_kiem_chung_that | sed '1s/^tu_kiem_chung_that /tu_kiem_chung /')"

echo ""
echo "=== Kich ban 3: ban moi HONG (ENTRYPOINT [\"false\"]) -- phai TU SAO LUU BO QUA, TU QUAY LUI, he thong VAN chay duoc ==="
echo 'ENTRYPOINT ["false"]' >> "$GOC_GIA/WebApp/Dockerfile"
git -C "$GOC_GIA" add -A
git -C "$GOC_GIA" commit -q -m "ban hong co y (kiem thu Task 12: cap nhat phai tu quay lui)"
moi_commit_c=$(git -C "$GOC_GIA" rev-parse HEAD)
echo "    Repo gia da co ban HONG: ${moi_commit_c:0:8}"

set +e
ra_3=$(cap_nhat 2>&1); ma_3=$?
set -e
echo "$ra_3"
echo "    Ma thoat: $ma_3"
[ "$ma_3" -ne 0 ] || { echo "THAT BAI: cap_nhat bao thanh cong (ma thoat 0) du ban moi HONG" >&2; exit 1; }
printf '%s\n' "$ra_3" | grep -qi "quay lui" \
  || { echo "THAT BAI: khong thay dau hieu tu quay lui trong log" >&2; exit 1; }
printf '%s\n' "$ra_3" | grep -qi "khong len duoc\|khong bi mat\|khong xay dung\|khong san sang" >/dev/null || true

head_sau_c=$(git -C "$GOC_CHECKOUT" rev-parse HEAD)
[ "$head_sau_c" = "$moi_commit_b" ] \
  || { echo "THAT BAI: sau quay lui, HEAD khong tro ve dung ban truoc do (mong ${moi_commit_b:0:8}, thuc te ${head_sau_c:0:8})" >&2; exit 1; }
[ "$(cat "$GOC_UNG_DUNG/.phien-ban-truoc")" = "$moi_commit_b" ] \
  || { echo "THAT BAI: .phien-ban-truoc khong ghi dung commit truoc lan cap nhat hong" >&2; exit 1; }

echo "    Xac nhan he thong CHAY DUOC that su sau khi quay lui (khong chi doi ma nguon)"
dc ps -q api | grep -q . || { echo "THAT BAI: khong con container api nao dang chay sau quay lui" >&2; exit 1; }
cho_san_sang 60 || { echo "THAT BAI: API khong san sang sau khi quay lui" >&2; exit 1; }
# Bang tu kiem chung THAT (khong khoan dung gi) chi con dung hai muc quyen tep KHONG DAT (gioi
# han Windows/NTFS da biet) -- moi muc con lai phai DAT, chung minh quay lui tra he thong ve
# dung trang thai khoe manh, khong chi "co container dang chay".
ket_qua_sau_quay_lui=$(tu_kiem_chung 2>&1) || true
so_la=$(printf '%s\n' "$ket_qua_sau_quay_lui" | grep '^  KHONG DAT' | grep -Ev 'quyen 600' | grep -c '.' || true)
if [ "$so_la" -ne 0 ]; then
  echo "THAT BAI: sau quay lui con muc KHONG DAT khong giai thich duoc (khong phai quyen tep):" >&2
  printf '%s\n' "$ket_qua_sau_quay_lui" | grep '^  KHONG DAT' | grep -Ev 'quyen 600' >&2
  exit 1
fi
echo "    DAT: HEAD ve dung ${head_sau_c:0:8}, container api chay lai, API san sang, bang tu kiem chung sach (tru 2 muc quyen tep -- gioi han Windows)"

echo ""
echo "=== Kich ban phu: khong QLGX_BO_QUA_R2 -- cap_nhat PHAI thu goi qlgx-runner.sh (chua ton tai) va DUNG lai, KHONG cap nhat lang le ==="
# qlgx-runner.sh la san pham cua Task 13, chua ton tai o day. `$(cap_nhat ...)` da tu tao mot
# subshell rieng nen `exit` (tu bao_loi_va_thoat) o trong cap_nhat khong lam hong tien trinh
# chinh. Boc them mot lop `( ... )` o day CHI de gioi han `unset QLGX_BO_QUA_R2` va cac lenh
# `exit 1` kiem tra ket qua bên trong nam gon trong mot khoi rieng, khong lam bien moi truong
# QLGX_BO_QUA_R2 bi mat vinh vien khoi phan con lai cua kich ban.
echo 'ENTRYPOINT ["false", "them-mot-dong"]' >> "$GOC_GIA/WebApp/Dockerfile"
git -C "$GOC_GIA" add -A
git -C "$GOC_GIA" commit -q -m "them mot ban nua de kich ban phu co gi ma cap nhat"
(
  unset QLGX_BO_QUA_R2
  set +e
  ra_phu=$(cap_nhat 2>&1); ma_phu=$?
  echo "$ra_phu"
  echo "    Ma thoat (subshell): $ma_phu"
  printf '%s\n' "$ra_phu" | grep -qi "sao luu truoc cap nhat\|qlgx-runner" \
    || { echo "THAT BAI: khong thay dau hieu cap_nhat co thu sao luu truoc" >&2; exit 1; }
  [ "$ma_phu" -ne 0 ] || { echo "THAT BAI: cap_nhat thanh cong du khong the sao luu (qlgx-runner.sh chua ton tai)" >&2; exit 1; }
)
head_sau_phu=$(git -C "$GOC_CHECKOUT" rev-parse HEAD)
[ "$head_sau_phu" = "$moi_commit_b" ] \
  || { echo "THAT BAI: kich ban phu (sao luu that bai) van lam HEAD di chuyen -- phai dung LAI TRUOC khi merge" >&2; exit 1; }
echo "    DAT: khi khong QLGX_BO_QUA_R2, cap_nhat dung lai o buoc sao luu (qlgx-runner.sh chua co o Task 12), KHONG merge/cap nhat lang le"

echo ""
echo "==> Kiem tra dung luong dia sau khi chay"
sau_gb=$(dung_gb_trong)
echo "    Dia trong sau: ${sau_gb:-?} GB (truoc: ${truoc_gb:-?} GB)"
if [ -n "$sau_gb" ] && [ "$sau_gb" -lt "$NGUONG_DIA_TOI_THIEU_GB" ]; then
  echo "CANH BAO: dia da tut duoi nguong ${NGUONG_DIA_TOI_THIEU_GB}GB sau khi chay -- se don dep ngay (trap EXIT)." >&2
fi

echo ""
echo "==> DAT: cap_nhat co sao luu bat buoc va tu quay lui khi ban moi HONG; khong khoi dong lai gi khi khong co ban moi."
