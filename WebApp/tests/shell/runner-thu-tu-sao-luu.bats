#!/usr/bin/env bats
# C2 -- THU TU NHAN QUA backup -> check -> forget --prune.
#
# Thiet ke muc 5.2 dat mot rang buoc CUNG: "Chi don ban cu sau khi ban moi da duoc xac nhan doc
# duoc. Thu tu cung: backup -> check -> forget --prune. Khong bao gio dao." Thu tu DONG MA luon
# dung, nhung rang buoc that su la NHAN QUA ("chi khi buoc truoc thanh cong") -- va rang buoc do
# tung bi mat sach tren dung duong goi tu hang doi web:
#
#   lenh_chay_job:  lenh_sao_luu ... || ma_thoat=$?
#
# Bash TAT errexit cho toan bo than mot ham khi ham do la ve trai cua mot danh sach `||`. Vi vay
# khi mot quy cha bam nut "Sao luu ngay", `restic backup` loi van chay tiep, `restic check` loi
# van chay tiep, va `restic forget --prune` -- lenh XOA BLOB THAT -- chay tren mot kho vua bao
# hong. Rui ro: mat TOAN BO lich su sao luu so sach giao xu.
#
# Bo test cu KHONG cham toi duong nay (bo e2e goi qua CLI, noi lenh_sao_luu duoc goi tran).
# `run` cua bats cung tat errexit, nen no tai hien DUNG boi canh cua duong hang doi web.

setup() {
  export QLGX_CHI_NAP_HAM=1
  # shellcheck disable=SC1090
  source "${BATS_TEST_DIRNAME}/../../scripts/qlgx-runner.sh"

  GOC_UNG_DUNG="$BATS_TEST_TMPDIR/app"
  TEP_BACKUP_ENV="$BATS_TEST_TMPDIR/backup.env"
  GIU_LAI="--keep-last 8"
  CT_TMP="$BATS_TEST_TMPDIR/container-tmp"        # dong vai /tmp BEN TRONG container postgres
  NKY_RESTIC="$BATS_TEST_TMPDIR/restic-da-goi"    # moi lenh con restic duoc goi, moi dong mot cai
  mkdir -p "$GOC_UNG_DUNG" "$CT_TMP"
  printf 'POSTGRES_DB=qlgx\n' > "$GOC_UNG_DUNG/.env"
  : > "$NKY_RESTIC"

  env_ung_dung() { printf 'qlgx'; }
  psql_quan_tri() { printf '2050\n'; }
  ghi_trang_thai_loi() { :; }
  lenh_dong_bo_danh_sach() { :; }

  # `dc` gia -- mo phong container postgres vua du de thuc_hien_sao_luu di duoc toi buoc restic.
  dc() {
    case "$*" in
      *pg_dumpall*) printf -- '-- globals gia\n'; return 0 ;;
      *pg_dump*)    mkdir -p "$CT_TMP/$TEN_DUMP_CONTAINER"
                    printf 'toc gia\n' > "$CT_TMP/$TEN_DUMP_CONTAINER/toc.dat"; return 0 ;;
      *"tar -cf -"*) tar -cf - -C "$CT_TMP" "$TEN_DUMP_CONTAINER"; return 0 ;;
      *) return 0 ;;
    esac
  }

  # `restic` gia -- ghi lai TUNG lenh con duoc goi, va cho phep tung test chon lenh nao that bai
  # qua bien LENH_RESTIC_HONG.
  restic() {
    printf '%s\n' "$1" >> "$NKY_RESTIC"
    [ "$1" != "${LENH_RESTIC_HONG:-}" ]
  }
  LENH_RESTIC_HONG=""
}

da_goi_restic() { grep -qx "$1" "$NKY_RESTIC"; }

@test "C2: restic backup THAT BAI thi KHONG BAO GIO chay check hay forget --prune" {
  LENH_RESTIC_HONG="backup"
  run lenh_sao_luu --nhan thu-cong --nguon thu_cong
  [ "$status" -ne 0 ]
  da_goi_restic backup
  ! da_goi_restic check
  ! da_goi_restic forget
}

@test "C2: restic check THAT BAI (kho nghi hong) thi KHONG BAO GIO chay forget --prune" {
  LENH_RESTIC_HONG="check"
  run lenh_sao_luu --nhan thu-cong --nguon thu_cong
  [ "$status" -ne 0 ]
  da_goi_restic backup
  da_goi_restic check
  ! da_goi_restic forget
}

@test "C2: ca backup va check deu dat thi moi duoc don ban cu" {
  run lenh_sao_luu --nhan thu-cong --nguon thu_cong
  [ "$status" -eq 0 ]
  da_goi_restic backup
  da_goi_restic check
  da_goi_restic forget
}

# Doi chung: cung mot kich ban nhung goi QUA lenh_sao_luu trong DUNG boi canh cua hang doi web
# (`... || ma_thoat=$?`), tuc la boi canh bash tat errexit. Day la duong ma nut "Sao luu ngay"
# di qua, va la duong bo e2e khong cham toi.
@test "C2: goi nhu hang doi web (lenh_sao_luu || ma=\$?) van khong duoc prune sau khi backup loi" {
  LENH_RESTIC_HONG="backup"
  ma_thoat=0
  lenh_sao_luu --nhan thu-cong --nguon thu_cong >/dev/null 2>&1 || ma_thoat=$?
  [ "$ma_thoat" -ne 0 ]
  ! da_goi_restic forget
}

# I11: that_bai_sao_luu goi `exit`. Khi lenh_sao_luu chay CUNG TIEN TRINH voi lenh_chay_job, cai
# `exit` do giet luon runner, nen `ket_thuc_job "$ma" loi ...` khong bao gio chay va dong cong
# viec ket o 'dang_chay' 2 gio -- chan moi nut tren man hinh Sao luu & Phuc hoi. Sau khi than ham
# duoc boc trong subshell, `exit` chi ket thuc subshell va tien trinh goi VAN SONG.
@test "I11: sao luu that bai KHONG duoc giet ca tien trinh goi (job phai ghi duoc ket qua)" {
  LENH_RESTIC_HONG="backup"
  ma_thoat=0
  lenh_sao_luu --nhan thu-cong --nguon thu_cong >/dev/null 2>&1 || ma_thoat=$?
  [ "$ma_thoat" -ne 0 ]
  # Dong nay chi chay duoc neu tien trinh con song sau khi sao luu that bai.
  printf 'con song' > "$BATS_TEST_TMPDIR/sau-khi-loi"
  [ "$(cat "$BATS_TEST_TMPDIR/sau-khi-loi")" = "con song" ]
}

# I1: thiet ke muc 5.2 doi CA BA thanh phan (db, globals, cau hinh) cung vao mot snapshot --
# "tha khong co snapshot con hon co snapshot thieu". Neu QLGX_GOC_UNG_DUNG tro sai duong dan thi
# vong lap gom cau hinh chep DUNG KHONG TEP NAO ma snapshot van len kho nhu thuong.
@test "I1: khong gom duoc .env vao ban sao thi KHONG duoc day snapshot len kho" {
  rm -f "$GOC_UNG_DUNG/.env"
  run lenh_sao_luu --nhan thu-cong --nguon thu_cong
  [ "$status" -ne 0 ]
  ! da_goi_restic backup
  [[ "$output" == *".env"* ]]
}

# N2: psql loi phai cho ra '?' chu KHONG phai 0 gia -- mot so 0 gia di thang vao tag snapshot roi
# hien len bang doi chieu truoc-sau cua man hinh phuc hoi ("quay ve day nghia la con 0 giao dan").
@test "N2: khong dem duoc giao dan thi tag mang '?' chu khong phai 0" {
  psql_quan_tri() { return 1; }
  [ "$(dem_hoac_dau_hoi 'SELECT count(*) FROM giao_dan')" = "?" ]
  psql_quan_tri() { printf 'khong phai so\n'; }
  [ "$(dem_hoac_dau_hoi 'SELECT count(*) FROM giao_dan')" = "?" ]
  psql_quan_tri() { printf ' 2050 \n'; }
  [ "$(dem_hoac_dau_hoi 'SELECT count(*) FROM giao_dan')" = "2050" ]
}
