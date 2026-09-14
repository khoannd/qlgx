#!/usr/bin/env bats
# Test cho nua dau cua install.sh: sinh_env, ghi_backup_env, la_cai_moi.
# Chi source script voi QLGX_CHI_NAP_HAM=1 -- khong chay bat ky lenh cai dat/tuong lua/git nao.

setup() {
  export QLGX_CHI_NAP_HAM=1
  # shellcheck disable=SC1090
  source "${BATS_TEST_DIRNAME}/../../scripts/install.sh"
  TEP="$BATS_TEST_TMPDIR/.env"
}

@test "sinh_env tao du moi khoa bat buoc" {
  sinh_env "$TEP"
  for k in POSTGRES_DB POSTGRES_USER POSTGRES_PASSWORD \
           QLGX_APP_DB_USER QLGX_APP_DB_PASSWORD \
           QLGX_ADMIN_DB_USER QLGX_ADMIN_DB_PASSWORD QLGX_JWT_KEY; do
    [ -n "$(doc_env_kv "$TEP" "$k")" ] || { echo "thieu khoa $k"; return 1; }
  done
}

@test "hai vai tro CSDL sinh ra PHAI khac ten" {
  sinh_env "$TEP"
  [ "$(doc_env_kv "$TEP" QLGX_APP_DB_USER)" != "$(doc_env_kv "$TEP" QLGX_ADMIN_DB_USER)" ]
}

@test "chay lai sinh_env KHONG doi bat ky bi mat nao" {
  sinh_env "$TEP"
  truoc=$(cat "$TEP")
  sinh_env "$TEP"
  [ "$truoc" = "$(cat "$TEP")" ]
}

@test "mat khau sinh ra du dai" {
  sinh_env "$TEP"
  [ "$(doc_env_kv "$TEP" POSTGRES_PASSWORD | wc -c)" -ge 20 ]
}

@test "tep .env co quyen 600" {
  sinh_env "$TEP"
  [ "$(stat -c '%a' "$TEP")" = "600" ]
}

@test "ghi_backup_env dat quyen 600 va du khoa" {
  export QLGX_R2_ENDPOINT="https://vd.r2.cloudflarestorage.com"
  export QLGX_R2_BUCKET="qlgx-sao-luu"
  export QLGX_R2_ACCESS_KEY_ID="k"
  export QLGX_R2_SECRET_ACCESS_KEY="s"
  BE="$BATS_TEST_TMPDIR/backup.env"
  ghi_backup_env "$BE"
  [ "$(stat -c '%a' "$BE")" = "600" ]
  [ -n "$(doc_env_kv "$BE" RESTIC_PASSWORD)" ]
  [[ "$(doc_env_kv "$BE" RESTIC_REPOSITORY)" == s3:* ]]
  [[ "$(doc_env_kv "$BE" RESTIC_REPOSITORY)" == *qlgx-sao-luu* ]]
}

@test "ghi_backup_env chay lai KHONG doi mat khau restic" {
  export QLGX_R2_ENDPOINT="https://vd.r2.cloudflarestorage.com"
  export QLGX_R2_BUCKET="qlgx-sao-luu"
  export QLGX_R2_ACCESS_KEY_ID="k"
  export QLGX_R2_SECRET_ACCESS_KEY="s"
  BE="$BATS_TEST_TMPDIR/backup.env"
  ghi_backup_env "$BE"
  cu=$(doc_env_kv "$BE" RESTIC_PASSWORD)
  ghi_backup_env "$BE"
  [ "$(doc_env_kv "$BE" RESTIC_PASSWORD)" = "$cu" ]
}

@test "lay_ma_nguon bao loi ro rang khi GOC_CHECKOUT la thu muc rac khong phai git repo" {
  export GOC_CHECKOUT="$BATS_TEST_TMPDIR/rac"
  mkdir -p "$GOC_CHECKOUT"
  touch "$GOC_CHECKOUT/mot-tep-la"
  export THU_MUC_CAU_HINH="$BATS_TEST_TMPDIR/etc-qlgx"
  export THU_MUC_SPOOL="$BATS_TEST_TMPDIR/spool"
  export THU_MUC_LOG="$BATS_TEST_TMPDIR/log"
  run lay_ma_nguon
  [ "$status" -ne 0 ]
  [[ "$output" == *"rm -rf"* ]]
}

@test "la_cai_moi dung theo su ton tai cua .env" {
  GOC_UNG_DUNG="$BATS_TEST_TMPDIR/ung-dung"
  mkdir -p "$GOC_UNG_DUNG"
  run la_cai_moi
  [ "$status" -eq 0 ]
  touch "$GOC_UNG_DUNG/.env"
  run la_cai_moi
  [ "$status" -ne 0 ]
}

# Task 16 (dep dat, dispatch xuyen-task sau khi dong Task 14/15): cho_san_sang() cua install.sh la
# ham chi em cua cho_api_san_sang trong qlgx-restore.sh, cung mac cung loi va duoc va cung mot cach
# -- xem phuc-hoi.bats de doi chieu. Gioi han phai la SO GIAY THAT (han chot theo dong ho), KHONG
# phai so vong lap: khi API crash-restart, moi lan `dc exec` tham do mat nhieu giay ngoai du kien.
@test "cho_san_sang nhan gioi han qua tham so va that bai khi khong bao gio san sang" {
  dc() { return 1; }
  sleep() { :; }
  run cho_san_sang 2
  [ "$status" -ne 0 ]
}

@test "cho_san_sang do bang DONG HO, khong phai bang so vong lap" {
  dc() { sleep 1; return 1; }   # moi lan tham do "ton" 1 giay
  bat_dau=$(date +%s)
  run cho_san_sang 3
  het=$(date +%s)
  [ "$status" -ne 0 ]
  troi_qua=$(( het - bat_dau ))
  # Neu dem theo vong lap thi phai mat >= 6 giay (3 vong x (1 giay dc + 1 giay sleep)).
  [ "$troi_qua" -lt 6 ]
  [ "$troi_qua" -ge 3 ]
}

@test "cho_san_sang tra ve thanh cong ngay khi dc bao san sang" {
  dc() { return 0; }
  run cho_san_sang 5
  [ "$status" -eq 0 ]
}

# Task 16, Viec 2: the phuc hoi (duong may trang) phai co buoc cai Docker TRUOC khi tai
# qlgx-restore.sh -- thieu buoc nay thi buoc goi qlgx-restore.sh that bai ngay voi "docker:
# command not found" (phat hien Critical 2 cua Task 14, hoan sua sang task nay).
@test "in_the_phuc_hoi: the phuc hoi co buoc cai Docker truoc buoc tai qlgx-restore.sh" {
  GOC_UNG_DUNG="$BATS_TEST_TMPDIR/ung-dung"
  THU_MUC_CAU_HINH="$BATS_TEST_TMPDIR/etc-qlgx"
  mkdir -p "$GOC_UNG_DUNG" "$THU_MUC_CAU_HINH"
  cat > "$GOC_UNG_DUNG/.env" <<'EOF'
POSTGRES_USER=u
POSTGRES_PASSWORD=p
QLGX_APP_DB_USER=au
QLGX_APP_DB_PASSWORD=ap
QLGX_ADMIN_DB_USER=du
QLGX_ADMIN_DB_PASSWORD=dp
EOF
  cat > "$THU_MUC_CAU_HINH/backup.env" <<'EOF'
RESTIC_PASSWORD=rp
RESTIC_REPOSITORY=s3:https://vd.r2.cloudflarestorage.com/qlgx-sao-luu
AWS_ACCESS_KEY_ID=id
AWS_SECRET_ACCESS_KEY=secret
EOF
  ghi_log() { :; }
  run in_the_phuc_hoi
  [ "$status" -eq 0 ]
  the="$THU_MUC_CAU_HINH/the-phuc-hoi.txt"
  [ -f "$the" ]
  dong_docker=$(grep -n 'get.docker.com' "$the" | cut -d: -f1)
  dong_tai_restore=$(grep -n 'qlgx-restore.sh -o qlgx-restore.sh' "$the" | cut -d: -f1)
  [ -n "$dong_docker" ]
  [ -n "$dong_tai_restore" ]
  [ "$dong_docker" -lt "$dong_tai_restore" ]
  # Cac buoc phai danh so lien tuc 1..5, khong trung khong thieu.
  buoc=$(grep -oE '^  [0-9]+\.' "$the" | grep -oE '[0-9]+' | tr '\n' ' ')
  [ "$buoc" = "1 2 3 4 5 " ]
}
