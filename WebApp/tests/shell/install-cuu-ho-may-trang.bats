#!/usr/bin/env bats
# F3 (Critical, review giao dien muc C3b) -- LUONG CUU HO TU MAY TRANG KHONG CHAY DUOC.
#
# Kich ban that: may chu giao xu chay/mat. Nguoi van hanh cam The phuc hoi, dung mot VPS trang,
# cai lai, va -- theo dung huong dan tren the -- nhap lai KHO R2 CU. Truoc ban sua nay install.sh
# van SINH MOT MAT KHAU RESTIC MOI ngau nhien (dung o kich ban may moi hoan toan, sai o day):
#   - mat khau moi khong mo duoc kho cu -> buoc sao luu dau tien that bai -> `set -e` giet script
#     TRUOC in_the_phuc_hoi -> nguoi van hanh khong bao gio nhan duoc The phuc hoi moi;
#   - va backup.env tren may moi giu mat khau SAI VINH VIEN, nen moi lan sao luu tu dong ve sau
#     deu hong ma khong script nao ghi mat khau tu the nguoc lai.

setup() {
  export QLGX_CHI_NAP_HAM=1
  export GOC_CHECKOUT="$BATS_TEST_TMPDIR/opt-qlgx"
  export GOC_UNG_DUNG="$GOC_CHECKOUT/WebApp"
  export THU_MUC_CAU_HINH="$BATS_TEST_TMPDIR/etc-qlgx"
  mkdir -p "$GOC_UNG_DUNG" "$THU_MUC_CAU_HINH"
  # shellcheck disable=SC1090
  source "${BATS_TEST_DIRNAME}/../../scripts/install.sh"
  BE="$THU_MUC_CAU_HINH/backup.env"
  export QLGX_R2_ENDPOINT="https://vd.r2.cloudflarestorage.com"
  export QLGX_R2_BUCKET="qlgx-sao-luu"
  export QLGX_R2_ACCESS_KEY_ID="KEYID001"
  export QLGX_R2_SECRET_ACCESS_KEY="SECRET_XYZ"
  KHONG_TUONG_TAC=1
}

@test "F3: nhap MAT KHAU RESTIC tu The phuc hoi thi backup.env phai giu DUNG mat khau do" {
  export QLGX_RESTIC_PASSWORD="mat-khau-tu-the-phuc-hoi"
  run ghi_backup_env "$BE"
  [ "$status" -eq 0 ]
  [ "$(doc_env_kv "$BE" RESTIC_PASSWORD)" = "mat-khau-tu-the-phuc-hoi" ]
}

@test "F3: khong nhap gi (may hoan toan moi) thi van sinh mat khau ngau nhien" {
  unset QLGX_RESTIC_PASSWORD
  run ghi_backup_env "$BE"
  [ "$status" -eq 0 ]
  mk=$(doc_env_kv "$BE" RESTIC_PASSWORD)
  [ -n "$mk" ]
  [ "${#mk}" -ge 20 ]
}

# Bat bien quan trong nhat cua toan bo script: chay lai KHONG duoc sinh lai bi mat nao. Doi
# RESTIC_PASSWORD la moi ban sao luu cu tro thanh khong doc duoc VINH VIEN.
@test "F3: chay lai KHONG duoc ghi de RESTIC_PASSWORD da co, ke ca khi bien moi truong khac" {
  export QLGX_RESTIC_PASSWORD="mat-khau-dau-tien"
  ghi_backup_env "$BE"
  export QLGX_RESTIC_PASSWORD="mat-khau-khac-hoan-toan"
  ghi_backup_env "$BE"
  [ "$(doc_env_kv "$BE" RESTIC_PASSWORD)" = "mat-khau-dau-tien" ]
}

# N5: R2 Secret Access Key va mat khau quan tri KHONG duoc hien tren man hinh khi go -- phien SSH
# luu scrollback, va nguoi van hanh thuong chia se man hinh khi nho nguoi khac giup.
@test "N5: hoi_hoac_bien co che do an (read -rsp) cho cac truong bi mat" {
  # Khong the go phim trong test; kiem rang ham NHAN duoc tham so che do va van tra dung gia tri
  # khi da co bien moi truong (duong khong hoi).
  export THU_BI_MAT="gia-tri-bi-mat"
  [ "$(hoi_hoac_bien THU_BI_MAT "Cau hoi" an)" = "gia-tri-bi-mat" ]
  unset THU_BI_MAT
  # Che do tuy_chon: --non-interactive tra ve chuoi rong thay vi bao loi va thoat.
  KHONG_TUONG_TAC=1
  run hoi_hoac_bien THU_BI_MAT "Cau hoi" an tuy_chon
  [ "$status" -eq 0 ]
  [ -z "$output" ]
  # Che do bat_buoc (mac dinh): van phai bao loi ro rang.
  run hoi_hoac_bien THU_BI_MAT "Cau hoi" an
  [ "$status" -ne 0 ]
  [[ "$output" == *"--non-interactive"* ]]
}

# N10: /opt co the chua ton tai tren mot may hoan toan trang. `df` tra ma khac 0 va duoi
# `set -euo pipefail` phep gan se giet ca script MA KHONG in mot thong bao tieng Viet nao.
@test "N10: khong do duoc dia trong thi bao loi tieng Viet, khong chet lang le" {
  id() { printf '0'; }
  os_ho() { printf 'ubuntu'; }
  nproc() { printf '4'; }
  df() { return 1; }
  run kiem_tra_tien_de
  [ "$status" -ne 0 ]
  [[ "$output" == *"dung luong dia"* ]] || [[ "$output" == *"RAM"* ]]
}
