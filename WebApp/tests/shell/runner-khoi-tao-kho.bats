#!/usr/bin/env bats
# F1 (Critical, review giao dien muc C4) -- THIEU `restic init`: moi lan cai moi deu hong.
#
# `restic backup` KHONG tu khoi tao kho. Voi mot bucket R2 moi tinh no dung lai voi "unable to
# open config file ... Is there a repository at the following location?". Truoc ban sua nay,
# KHONG MOT script nao trong bo goi `restic init` (da grep toan bo scripts/), nen buoc "Chay sao
# luu dau tien de chung minh duong ong song that" cua install.sh that bai o MOI lan cai moi --
# va duoi `set -e` no giet script TRUOC in_the_phuc_hoi, nen nguoi van hanh khong nhan duoc The
# phuc hoi.
#
# F1 cung la nua con lai cua kich ban cuu ho (F3): neu kho DA CO nhung mat khau SAI, `restic init`
# phai bao loi RO RANG bang tieng Viet chi ra dung nguyen nhan, khong de nguoi van hanh doc mot
# thong bao tieng Anh kho hieu giua mot cuoc cuu ho.

setup() {
  export QLGX_CHI_NAP_HAM=1
  # shellcheck disable=SC1090
  source "${BATS_TEST_DIRNAME}/../../scripts/qlgx-runner.sh"
  RESTIC_REPOSITORY="s3:https://vd.r2.cloudflarestorage.com/qlgx-thu"
  TEP_BACKUP_ENV="$BATS_TEST_TMPDIR/backup.env"
  NKY="$BATS_TEST_TMPDIR/restic-da-goi"
  : > "$NKY"
}

@test "F1: kho CHUA ton tai thi phai chay restic init" {
  restic() {
    printf '%s\n' "$*" >> "$NKY"
    case "$1" in
      cat)  return 1 ;;   # chua mo duoc kho
      init) return 0 ;;
    esac
  }
  run lenh_khoi_tao_kho
  [ "$status" -eq 0 ]
  grep -q '^init' "$NKY"
}

@test "F1: kho DA ton tai thi KHONG duoc chay restic init lan nua" {
  restic() {
    printf '%s\n' "$*" >> "$NKY"
    case "$1" in cat) return 0 ;; *) return 0 ;; esac
  }
  run lenh_khoi_tao_kho
  [ "$status" -eq 0 ]
  ! grep -q '^init' "$NKY"
}

# Kich ban cuu ho: kho cu VAN CON tren R2 nhung may moi sinh mat khau restic moi. `restic cat
# config` that bai (sai mat khau) va `restic init` cung tu choi ghi de mot kho da co. Thong bao
# phai chi dung ra ca hai nguyen nhan co the, bang tieng Viet.
@test "F1: kho co san nhung sai mat khau -> bao loi tieng Viet chi ro nguyen nhan" {
  restic() { return 1; }
  run lenh_khoi_tao_kho
  [ "$status" -ne 0 ]
  [[ "$output" == *"MAT KHAU RESTIC"* ]]
  [[ "$output" == *"The phuc hoi"* ]]
}

# I4: thiet ke muc 4.5 doi bang tu kiem chung khang dinh kho "mo duoc VA co >= 1 snapshot".
@test "I4: dem-snapshot dem TAT CA snapshot, khong loc nhan" {
  restic() {
    cat <<'JSON'
[{"short_id":"a1","tags":["nhan=tu-dong"]},
 {"short_id":"a2","tags":[]},
 {"short_id":"a3","tags":["nhan=truoc-cap-nhat"]}]
JSON
  }
  run lenh_dem_snapshot
  [ "$status" -eq 0 ]
  [ "$output" -eq 3 ]
}

@test "I4: dem-snapshot BAO LOI (khong tra 0 gia) khi kho khong mo duoc" {
  restic() { return 1; }
  run lenh_dem_snapshot
  [ "$status" -ne 0 ]
  [ "$output" != "0" ]
}

# I12: install.sh can id cua ban sao 'truoc-cap-nhat' de in trong cau huong dan phuc hoi sau khi
# quay lui. restic in danh sach theo thu tu thoi gian tang dan -> ban CUOI CUNG la ban moi nhat.
@test "I12: id-snapshot-nhan tra ve ban MOI NHAT mang nhan do" {
  restic() {
    cat <<'JSON'
[{"short_id":"aaa11111","tags":["nhan=truoc-cap-nhat"]},
 {"short_id":"bbb22222","tags":["nhan=truoc-cap-nhat"]}]
JSON
  }
  run lenh_id_snapshot_nhan truoc-cap-nhat
  [ "$status" -eq 0 ]
  [ "$output" = "bbb22222" ]
}

@test "I12: id-snapshot-nhan bao loi khi thieu tham so" {
  run lenh_id_snapshot_nhan
  [ "$status" -ne 0 ]
}

@test "F1: khoi-tao-kho co trong danh sach lenh cua main_runner" {
  export TEP_BACKUP_ENV="$BATS_TEST_TMPDIR/backup.env"
  touch "$TEP_BACKUP_ENV"
  export THU_MUC_SPOOL="$BATS_TEST_TMPDIR/spool"
  export THU_MUC_LOG="$BATS_TEST_TMPDIR/log"
  run main_runner lenh-khong-ton-tai
  [ "$status" -ne 0 ]
  [[ "$output" == *"khoi-tao-kho"* ]]
  [[ "$output" == *"dem-snapshot"* ]]
  [[ "$output" == *"id-snapshot-nhan"* ]]
}
