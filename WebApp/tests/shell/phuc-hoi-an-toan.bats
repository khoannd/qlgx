#!/usr/bin/env bats
# I3, I5, I6, I10 -- cac lo hong "tham lang" con lai cua duong phuc hoi.

setup() {
  export QLGX_CHI_NAP_HAM=1
  # shellcheck disable=SC1090
  source "${BATS_TEST_DIRNAME}/../../scripts/qlgx-restore.sh"
  GOC_UNG_DUNG="$BATS_TEST_TMPDIR/app"
  mkdir -p "$GOC_UNG_DUNG"
  NKY="$BATS_TEST_TMPDIR/da-lam"
  : > "$NKY"
}

# ============================== I3 ==============================
# globals.sql (pg_dumpall --globals-only) chua `ALTER ROLE ... PASSWORD` cho TAT CA vai tro cua
# cum -- gom ca postgres va qlgx_app -- ve mat khau TAI THOI DIEM CHUP. Dieu kien cu ("bat ky vai
# tro nao vang thi nap") de lot kich ban: mat MOT vai tro -> nap globals -> ghi de mat khau cua
# CA cac vai tro dang chay bang gia tri CU -> container API mat ket noi CSDL, va -- nghiem trong
# hon -- pg()/psql cung hong, tuc chinh buoc hoan doi ten [6/7] va duong dao nguoc [7/7] khong
# con chay duoc. Hong dung giua tay mot cuoc phuc hoi.
@test "I3: thieu MOT trong hai vai tro thi KHONG duoc nap globals.sql" {
  printf 'ALTER ROLE postgres PASSWORD md5cu;\n' > "$BATS_TEST_TMPDIR/globals.sql"
  env_ung_dung() {
    case "$1" in
      QLGX_APP_DB_USER)   printf 'qlgx_app' ;;
      QLGX_ADMIN_DB_USER) printf 'qlgx_admin' ;;
      *)                  printf 'qlgx_chu' ;;
    esac
  }
  pg1() { case "$*" in *qlgx_app*) printf '1' ;; *) printf '0' ;; esac; }   # thieu qlgx_admin
  dc() { printf 'DA NAP GLOBALS\n' >> "$NKY"; }
  run nap_globals_neu_thieu_vai_tro "$BATS_TEST_TMPDIR/globals.sql"
  [ "$status" -eq 0 ]
  [ ! -s "$NKY" ]
  [[ "$output" == *"KHONG nap"* ]]
}

@test "I3: CA HAI vai tro deu vang (may that su trang) thi VAN nap globals.sql" {
  printf 'CREATE ROLE qlgx_app;\n' > "$BATS_TEST_TMPDIR/globals.sql"
  env_ung_dung() {
    case "$1" in
      QLGX_APP_DB_USER)   printf 'qlgx_app' ;;
      QLGX_ADMIN_DB_USER) printf 'qlgx_admin' ;;
      *)                  printf 'qlgx_chu' ;;
    esac
  }
  pg1() { printf '0'; }
  dc() { printf 'DA NAP GLOBALS\n' >> "$NKY"; }
  run nap_globals_neu_thieu_vai_tro "$BATS_TEST_TMPDIR/globals.sql"
  [ "$status" -eq 0 ]
  grep -q 'DA NAP GLOBALS' "$NKY"
}

@test "I3: du ca hai vai tro thi KHONG nap globals.sql" {
  printf 'x\n' > "$BATS_TEST_TMPDIR/globals.sql"
  env_ung_dung() {
    case "$1" in
      QLGX_APP_DB_USER)   printf 'qlgx_app' ;;
      QLGX_ADMIN_DB_USER) printf 'qlgx_admin' ;;
      *)                  printf 'qlgx_chu' ;;
    esac
  }
  pg1() { printf '1'; }
  dc() { printf 'DA NAP GLOBALS\n' >> "$NKY"; }
  run nap_globals_neu_thieu_vai_tro "$BATS_TEST_TMPDIR/globals.sql"
  [ "$status" -eq 0 ]
  [ ! -s "$NKY" ]
}

# ============================== I5 ==============================
# Thiet ke muc 11: "Dia day do CSDL tam khi phuc hoi/dien tap -> Kiem dung luong trong truoc buoc
# 2". Phuc hoi tao mot ban sao THU HAI cua toan bo CSDL canh ban dang chay, cong ban dump giai nen
# o hai noi -- dinh diem khoang 3 lan kich thuoc CSDL. Dien tap Chu nhat 03:00 chay TU DONG,
# khong ai ngoi xem: day dia giua pg_restore lam chinh PostgreSQL DANG PHUC VU het cho ghi WAL.
@test "I5: dia trong khong du 3 lan kich thuoc CSDL thi phai DUNG" {
  pg1() { printf '12884901888'; }                        # CSDL 12 GB
  dc() { printf '10485760\n'; }                          # con 10 GB trong (10*1024*1024 KB)
  run kiem_dia_truoc_khi_nap qlgx
  [ "$status" -ne 0 ]
  [[ "$output" == *"khong du"* ]]
}

@test "I5: dia trong du thi di tiep" {
  pg1() { printf '1073741824'; }                         # CSDL 1 GB
  dc() { printf '52428800\n'; }                          # con 50 GB trong
  run kiem_dia_truoc_khi_nap qlgx
  [ "$status" -eq 0 ]
}

# Do khong duoc thi CANH BAO roi di tiep, KHONG chan: mot phep do that bai khong phai bang chung
# la dia day, va chan duong cuu ho vi mot phep do phu la hong vi ly do phu.
@test "I5: khong do duoc thi canh bao roi di tiep (khong chan duong cuu ho)" {
  pg1() { printf ''; }
  dc() { return 1; }
  run kiem_dia_truoc_khi_nap qlgx
  [ "$status" -eq 0 ]
  [[ "$output" == *"bo qua buoc kiem dia"* ]]
}

# ============================== I6 ==============================
# Thiet ke muc 7.1 buoc 7 hua "giu qlgx_truoc_phuc_hoi_<ts> them 7 ngay roi TU XOA". Truoc day
# don_csdl_cu chi chay o cuoi mot lan phuc hoi THANH CONG ke tiep -- khong timer nao, khong lenh
# CLI nao goi no. Hai ban sao day du nam lai vinh vien, an dan het dia.
@test "I6: --chi-don-csdl-cu chi don CSDL cu roi thoat, KHONG phuc hoi gi" {
  nap_cau_hinh_ph() { GOC_UNG_DUNG="$BATS_TEST_TMPDIR/app"; }
  kiem_ung_dung_da_co() { :; }
  chot_snapshot_latest() { :; }
  env_ung_dung() { printf 'qlgx'; }
  don_csdl_cu() { printf 'DA DON\n' >> "$NKY"; }
  phuc_hoi_that() { printf 'DA PHUC HOI\n' >> "$NKY"; }
  run main_phuc_hoi --chi-don-csdl-cu
  [ "$status" -eq 0 ]
  grep -q 'DA DON' "$NKY"
  ! grep -q 'DA PHUC HOI' "$NKY"
}

@test "I6: qlgx-verify.service co goi buoc don CSDL cu" {
  run grep -q 'chi-don-csdl-cu' "${BATS_TEST_DIRNAME}/../../scripts/systemd/qlgx-verify.service"
  [ "$status" -eq 0 ]
}

# ============================== I10 ==============================
# "0 giao dan + 0 gia dinh" KHONG dong nghia voi "khong co gi de mat": CSDL co the da co toan bo
# tai khoan nguoi dung, danh sach giao xu va cac ban ghi bi tich. Kich ban that: mot lan nhap lieu
# tu Access hong lam rong hai bang giao_dan/gia_dinh nhung van nguyen tai_khoan/giao_xu/bi_tich_*.
@test "I10: 0 giao dan/0 gia dinh NHUNG co tai khoan thi VAN phai sao luu" {
  pg1() {
    case "$*" in
      *pg_database*) printf '1' ;;
      *tai_khoan*)   printf '7' ;;
      *)             printf '0' ;;
    esac
  }
  run co_du_lieu_can_bao_ve qlgx
  [ "$status" -eq 0 ]     # 0 = CO du lieu can bao ve
}

@test "I10: 0 giao dan/0 gia dinh NHUNG co ban ghi bi tich thi VAN phai sao luu" {
  pg1() {
    case "$*" in
      *pg_database*)       printf '1' ;;
      *bi_tich_chi_tiet*)  printf '350' ;;
      *)                   printf '0' ;;
    esac
  }
  run co_du_lieu_can_bao_ve qlgx
  [ "$status" -eq 0 ]
}

@test "I10: MOI phep dem bang 0 (may vua cai xong that) thi moi duoc bo qua sao luu" {
  pg1() { case "$*" in *pg_database*) printf '1' ;; *) printf '0' ;; esac; }
  run co_du_lieu_can_bao_ve qlgx
  [ "$status" -eq 1 ]
}

@test "I10: khong dem duoc tai khoan/bi tich thi nga ve phia AN TOAN (van sao luu)" {
  pg1() {
    case "$*" in
      *pg_database*) printf '1' ;;
      *tai_khoan*)   printf '' ;;
      *)             printf '0' ;;
    esac
  }
  run co_du_lieu_can_bao_ve qlgx
  [ "$status" -eq 0 ]
  [[ "$output" == *"COI NHU CO du lieu"* ]]
}
