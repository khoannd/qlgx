#!/usr/bin/env bats

setup() {
  export QLGX_CHI_NAP_HAM=1
  # shellcheck disable=SC1090
  source "${BATS_TEST_DIRNAME}/../../scripts/qlgx-runner.sh"
}

@test "tinh_nguon_tu_nhan anh xa dung bon nguon" {
  [ "$(tinh_nguon_tu_nhan truoc-cap-nhat)" = "truoc_cap_nhat" ]
  [ "$(tinh_nguon_tu_nhan truoc-phuc-hoi)" = "truoc_phuc_hoi" ]
  [ "$(tinh_nguon_tu_nhan thu-cong)" = "thu_cong" ]
  [ "$(tinh_nguon_tu_nhan '')" = "tu_dong" ]
}

@test "loc_snapshot_json doc dung id, thoi diem va so lieu tu tags" {
  cat > "$BATS_TEST_TMPDIR/sn.json" <<'EOF'
[{"short_id":"ab12cd34","time":"2026-09-13T06:00:00.123456Z",
  "tags":["nhan=tu-dong","giao_dan=2050","gia_dinh=40"],"summary":{"total_bytes_processed":123456}}]
EOF
  run loc_snapshot_json "$BATS_TEST_TMPDIR/sn.json"
  [ "$status" -eq 0 ]
  [[ "$output" == *"ab12cd34"* ]]
  [[ "$output" == *"2050"* ]]
  [[ "$output" == *"40"* ]]
  [[ "$output" == *"123456"* ]]
}

@test "loc_snapshot_json chiu duoc snapshot thieu tags" {
  cat > "$BATS_TEST_TMPDIR/sn.json" <<'EOF'
[{"short_id":"ff00ff00","time":"2026-09-13T06:00:00Z","tags":[],"summary":{}}]
EOF
  run loc_snapshot_json "$BATS_TEST_TMPDIR/sn.json"
  [ "$status" -eq 0 ]
  [[ "$output" == *"ff00ff00"* ]]
}

@test "chuoi giu lai dung dung gia tri da chot trong thiet ke" {
  [ "$GIU_LAI_MAC_DINH" = "--keep-last 8 --keep-daily 30 --keep-weekly 12 --keep-monthly 24" ]
}
