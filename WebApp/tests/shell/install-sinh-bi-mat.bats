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

@test "la_cai_moi dung theo su ton tai cua .env" {
  GOC_UNG_DUNG="$BATS_TEST_TMPDIR/ung-dung"
  mkdir -p "$GOC_UNG_DUNG"
  run la_cai_moi
  [ "$status" -eq 0 ]
  touch "$GOC_UNG_DUNG/.env"
  run la_cai_moi
  [ "$status" -ne 0 ]
}
