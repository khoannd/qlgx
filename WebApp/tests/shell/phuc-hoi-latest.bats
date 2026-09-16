#!/usr/bin/env bats
# C1 -- "--snapshot latest --apply" khong duoc phuc hoi CHINH BAN VUA TU CHUP.
#
# Vi sao can bo test RIENG nay: `latest` khong phai ten mot snapshot, restic phan giai no thanh
# ban moi nhat TAI THOI DIEM GOI. Buoc [1/7] (sao luu bat buoc) tao ra dung mot snapshot MOI ngay
# TRUOC buoc [2/7] (restic restore). Neu chu "latest" di nguyen ven toi buoc [2/7], he thong nap
# lai chinh trang thai dang hong, moi buoc kiem chung deu DAT (vi doi chieu voi tag cua CHINH
# snapshot do) va in "PHUC HOI XONG" -- that bai THAM LANG tren dung dong lenh in o The phuc hoi.
#
# Bo test cu KHONG cham toi duong nay: phuc-hoi-e2e.sh moi kich ban --apply deu truyen id tuong
# minh, con "latest" chi duoc dung cho --dien-tap (khong sao luu nen khong bi anh huong).

setup() {
  export QLGX_CHI_NAP_HAM=1
  # shellcheck disable=SC1090
  source "${BATS_TEST_DIRNAME}/../../scripts/qlgx-restore.sh"

  # Kho gia: ban dau co HAI snapshot (aaa11111 cu, bbb22222 moi nhat). Khi buoc sao luu bat buoc
  # chay, mot ban THU BA (ccc33333 -- chinh trang thai dang hong) xuat hien va tro thanh "latest".
  echo 0 > "$BATS_TEST_TMPDIR/da-sao-luu"
  json_toan_bo_snapshot() {
    if [ "$(cat "$BATS_TEST_TMPDIR/da-sao-luu")" = "1" ]; then
      printf '%s\n' '[{"short_id":"aaa11111"},{"short_id":"bbb22222"},{"short_id":"ccc33333"}]'
    else
      printf '%s\n' '[{"short_id":"aaa11111"},{"short_id":"bbb22222"}]'
    fi
  }
  restic_doc() { printf ''; }          # doc_tag_snapshot -> '?' , khong dung restic that
  nap_cau_hinh_ph() { GOC_UNG_DUNG="$BATS_TEST_TMPDIR/app"; THU_MUC_LOG="$BATS_TEST_TMPDIR/log"; }
  kiem_ung_dung_da_co() { :; }
  env_ung_dung() { printf 'qlgx'; }
  don_csdl_cu() { :; }

  # phuc_hoi_that GIA -- mo phong DUNG hai buoc dau cua ban that, theo dung thu tu:
  #   [1/7] sao luu bat buoc  -> them mot snapshot moi vao kho
  #   [2/7] restic restore "$SNAPSHOT" -> neu $SNAPSHOT van la "latest", restic phan giai LUC NAY
  # Ghi ra tep id THUC SU duoc nap, de test khang dinh do la ban CU chu khong phai ban vua chup.
  phuc_hoi_that() {
    echo 1 > "$BATS_TEST_TMPDIR/da-sao-luu"
    local sn="$SNAPSHOT"
    if [ "$sn" = "latest" ]; then sn=$(json_toan_bo_snapshot | id_snapshot_cuoi_cung); fi
    printf '%s' "$sn" > "$BATS_TEST_TMPDIR/da-nap"
  }
}

@test "C1: --snapshot latest --apply phai nap ban CU (truoc khi sao luu bat buoc), khong phai ban vua chup" {
  run main_phuc_hoi --snapshot latest --apply
  [ "$status" -eq 0 ]
  [ "$(cat "$BATS_TEST_TMPDIR/da-nap")" = "bbb22222" ]
  [ "$(cat "$BATS_TEST_TMPDIR/da-nap")" != "ccc33333" ]
}

@test "C1: khong co --snapshot (mac dinh latest) cung phai duoc chot truoc khi sao luu" {
  run main_phuc_hoi --apply
  [ "$status" -eq 0 ]
  [ "$(cat "$BATS_TEST_TMPDIR/da-nap")" = "bbb22222" ]
}

@test "C1: che do in ke hoach va che do --apply phai noi ve CUNG MOT snapshot" {
  # In ke hoach truoc (khong sao luu gi), ghi lai id da chot...
  in_ke_hoach() { printf '%s' "$SNAPSHOT" > "$BATS_TEST_TMPDIR/ke-hoach"; }
  run main_phuc_hoi --snapshot latest
  [ "$status" -eq 0 ]
  # ...roi chay that: id duoc nap phai trung voi id da in trong ke hoach.
  run main_phuc_hoi --snapshot latest --apply
  [ "$status" -eq 0 ]
  [ "$(cat "$BATS_TEST_TMPDIR/ke-hoach")" = "$(cat "$BATS_TEST_TMPDIR/da-nap")" ]
}

@test "C1: kho rong thi DUNG voi thong bao ro rang, khong di tiep voi chu 'latest'" {
  json_toan_bo_snapshot() { printf '%s\n' '[]'; }
  run main_phuc_hoi --snapshot latest --apply
  [ "$status" -ne 0 ]
  [[ "$output" == *"khong co snapshot nao"* ]]
  [ ! -f "$BATS_TEST_TMPDIR/da-nap" ]
}

@test "C1: --snapshot <id> tuong minh khong bi dong toi" {
  run main_phuc_hoi --snapshot aaa11111 --apply
  [ "$status" -eq 0 ]
  [ "$(cat "$BATS_TEST_TMPDIR/da-nap")" = "aaa11111" ]
}
