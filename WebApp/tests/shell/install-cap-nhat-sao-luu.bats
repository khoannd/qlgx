#!/usr/bin/env bats
# Kiem thu HAM THUAN sao_luu_bat_buoc_cap_nhat() cua install.sh (Finding 2, vong review cuoi
# cung cua ke hoach sao luu/phuc hoi) -- KHONG dung Docker/Postgres/restic that. Mock `runner_cmd`
# (ham boc goi qlgx-runner.sh, xem dinh nghia trong install.sh) de kiem chung dung LOGIC dem-truoc/
# sao-luu/dem-sau/thu-lai/DUNG-neu-khong-tang, giong cach `dc()`/`pg1()` da duoc mock o cac bats
# khac trong bo nay (install-sinh-bi-mat.bats, phuc-hoi.bats).
#
# Boi canh: cap_nhat() TUNG tin thang ma thoat cua qlgx-runner.sh sao-luu de coi la "da co ban
# sao truoc-cap-nhat that" -- dung LOP LOI ma qlgx-restore.sh's sao_luu_bat_buoc() da tung mac va
# duoc Task 14 vá (gianh_khoa_hoac_bo_qua CO Y exit 0 khi mot luot khac dang giu khoa runner, dung
# cho cron nhung SAI neu hieu la "da sao luu xong" truoc mot thao tac PHA HUY nhu cap nhat/phuc
# hoi). install.sh chua duoc vá theo vi Task 12 (noi cap_nhat duoc viet) dong TRUOC khi Task 14
# phat hien loi nay.

setup() {
  export QLGX_CHI_NAP_HAM=1
  # shellcheck disable=SC1090
  source "${BATS_TEST_DIRNAME}/../../scripts/install.sh"
  # KHONG mock ghi_log: bao_loi_va_thoat ("$*" cua thong diep loi ta kiem tra ben duoi) IN RA
  # QUA CHINH ghi_log -- mock no thanh no-op se lam $output rong va cac test kiem noi dung thong
  # diep loi phia duoi luon that bai du logic dung.
}

@test "sao_luu_bat_buoc_cap_nhat: thanh cong ngay khi so snapshot tang dung mot lan sau sao-luu" {
  DEM=0
  runner_cmd() {
    case "$1" in
      dem-snapshot-nhan) echo "$DEM" ;;
      sao-luu)           DEM=$((DEM + 1)); return 0 ;;
    esac
  }
  run sao_luu_bat_buoc_cap_nhat
  [ "$status" -eq 0 ]
}

# Day la kich ban CHINH cua Finding 2: qlgx-runner.sh sao-luu bao "thanh cong" (ma thoat 0 -- vi
# du no vua tu `exit 0` trong gianh_khoa_hoac_bo_qua vi mot luot khac dang giu khoa) NHUNG kho
# THAT SU khong co them snapshot nao. Phai DUNG cap nhat, khong duoc coi day la da sao luu xong.
@test "sao_luu_bat_buoc_cap_nhat: DUNG (bao loi ro rang) khi so snapshot KHONG tang du qlgx-runner.sh bao thanh cong" {
  sleep() { :; }   # bo qua 3 lan thu lai x2s cho test nhanh
  runner_cmd() {
    case "$1" in
      dem-snapshot-nhan) echo "5" ;;   # luon la 5 -- khong bao gio tang
      sao-luu)           return 0 ;;   # nhung van bao "thanh cong"
    esac
  }
  run sao_luu_bat_buoc_cap_nhat
  [ "$status" -ne 0 ]
  [[ "$output" == *"KHONG co them ban sao"* ]]
}

@test "sao_luu_bat_buoc_cap_nhat: DUNG ngay tu dau neu khong dem duoc snapshot (kho khong mo duoc)" {
  runner_cmd() { case "$1" in dem-snapshot-nhan) return 1 ;; esac; }
  run sao_luu_bat_buoc_cap_nhat
  [ "$status" -ne 0 ]
  [[ "$output" == *"Khong dem duoc snapshot"* ]]
}

@test "sao_luu_bat_buoc_cap_nhat: DUNG khi chinh lenh sao-luu that bai (ma thoat khac 0)" {
  runner_cmd() {
    case "$1" in
      dem-snapshot-nhan) echo 0 ;;
      sao-luu)           return 1 ;;
    esac
  }
  run sao_luu_bat_buoc_cap_nhat
  [ "$status" -ne 0 ]
  [[ "$output" == *"Sao luu truoc cap nhat THAT BAI"* ]]
}

# Giong het ly do sao_luu_bat_buoc() trong qlgx-restore.sh thu lai vai lan: chi muc restic co the
# chua kip hien snapshot vua day len ngay lap tuc. Kich ban nay: lan dem dau tien (sau sao-luu)
# CHUA thay tang, phai lan thu lai thu hai moi thay -- sao_luu_bat_buoc_cap_nhat phai THANH CONG,
# khong duoc bao loi som.
@test "sao_luu_bat_buoc_cap_nhat: thanh cong sau vai lan thu lai khi chi muc chua kip hien snapshot moi" {
  sleep() { :; }
  # Dem so lan goi qua MOT TEP (khong phai bien shell thuong): `so_truoc=$(runner_cmd ...)` chay
  # runner_cmd trong MOT SUBSHELL rieng cua phep the lenh -- mot bien toan cuc bi sua trong do
  # khong "thoat" ra ngoai duoc, chi tep moi song sot qua ranh gioi subshell nay.
  echo 0 > "$BATS_TEST_TMPDIR/lan"
  runner_cmd() {
    case "$1" in
      dem-snapshot-nhan)
        local lan; lan=$(($(cat "$BATS_TEST_TMPDIR/lan") + 1))
        echo "$lan" > "$BATS_TEST_TMPDIR/lan"
        if [ "$lan" -le 2 ]; then echo 3; else echo 4; fi ;;
      sao-luu) return 0 ;;
    esac
  }
  run sao_luu_bat_buoc_cap_nhat
  [ "$status" -eq 0 ]
}
