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

# N9: loc_snapshot_json dung python3 (co san tren moi ban phan phoi may chu muc tieu, va gio da
# nam trong danh sach phu thuoc cua install.sh) nhung KHONG co trong container bats. Mot suite do
# mac dinh se nhanh chong bi bo qua, va luc do mot loi THAT se lan vao ma khong ai nhin ky -- nen
# bo qua CO DIEU KIEN va noi ro ly do, thay vi de do.
can_python3() { command -v python3 >/dev/null 2>&1 || skip "can python3 (khong co trong container bats)"; }

@test "loc_snapshot_json doc dung id, thoi diem va so lieu tu tags" {
  can_python3
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
  can_python3
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

# Finding 2 cua vong review cuoi cung: install.sh's cap_nhat() tung tin thang ma thoat cua
# qlgx-runner.sh de coi la "da co ban sao truoc-cap-nhat that", cung mot lop loi dung Task 14 da
# vá cho qlgx-restore.sh's sao_luu_bat_buoc(). subcommand nay ("dem-snapshot-nhan") la noi DUY
# NHAT ca install.sh lan qlgx-restore.sh (neu sau nay can) goi de dem dung SU THAT trong kho,
# thay vi moi noi tu doc backup.env/goi restic rieng. Mock `restic` bang MOT HAM bash cung ten
# (restic duoc goi TRUC TIEP, khong qua duong dan tuyet doi, nen ham cung ten trong cung shell se
# thang PATH that -- giong cach `dc()`/`pg1()` da duoc mock o cac bats khac trong bo nay).
@test "lenh_dem_snapshot_nhan dem dung so snapshot mang dung nhan, bo qua nhan khac" {
  restic() {
    cat <<'JSON'
[{"short_id":"a1","tags":["nhan=truoc-cap-nhat"]},
 {"short_id":"a2","tags":["nhan=tu-dong"]},
 {"short_id":"a3","tags":["nhan=truoc-cap-nhat"]}]
JSON
  }
  run lenh_dem_snapshot_nhan truoc-cap-nhat
  [ "$status" -eq 0 ]
  [ "$output" -eq 2 ]
}

@test "lenh_dem_snapshot_nhan tra ve 0 (khong loi) khi kho mo duoc nhung khong co snapshot nao mang nhan" {
  restic() { echo '[]'; }
  run lenh_dem_snapshot_nhan truoc-cap-nhat
  [ "$status" -eq 0 ]
  [ "$output" -eq 0 ]
}

# CO Y KHONG duoc tra ve "0" gia khi restic THAT SU khong mo duoc kho (sai khoa, mat mang, kho
# khong ton tai) -- neu lam vay, noi goi (install.sh) se khong phan biet duoc "dem duoc 0 that su"
# voi "khong dem duoc gi ca", va co the ket luan sai "khong tang -> DUNG cap nhat" voi ly do loi
# (kho khong mo duoc tu dau, chua he thu sao luu) thay vi ly do dung (sao luu bi bo qua lang le).
@test "lenh_dem_snapshot_nhan BAO LOI RO RANG (khong tra ve 0 gia) khi restic khong mo duoc kho" {
  restic() { return 1; }
  run lenh_dem_snapshot_nhan truoc-cap-nhat
  [ "$status" -ne 0 ]
  [ "$output" != "0" ]
}

@test "lenh_dem_snapshot_nhan bao loi khi thieu tham so nhan" {
  run lenh_dem_snapshot_nhan
  [ "$status" -ne 0 ]
}

@test "main_runner: dem-snapshot-nhan noi len duoc trong thong bao lenh khong hieu (khong bi quen khai bao)" {
  export TEP_BACKUP_ENV="$BATS_TEST_TMPDIR/backup.env"
  touch "$TEP_BACKUP_ENV"
  export THU_MUC_SPOOL="$BATS_TEST_TMPDIR/spool"
  export THU_MUC_LOG="$BATS_TEST_TMPDIR/log"
  run main_runner lenh-khong-ton-tai
  [ "$status" -ne 0 ]
  [[ "$output" == *"dem-snapshot-nhan"* ]]
}
