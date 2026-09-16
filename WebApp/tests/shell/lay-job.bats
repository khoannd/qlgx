#!/usr/bin/env bats

setup() {
  export QLGX_CHI_NAP_HAM=1
  # shellcheck disable=SC1090
  source "${BATS_TEST_DIRNAME}/../../scripts/qlgx-runner.sh"
}

@test "cau SQL gianh job dung FOR UPDATE SKIP LOCKED" {
  run sql_gianh_job
  [[ "$output" == *"FOR UPDATE SKIP LOCKED"* ]]
  [[ "$output" == *"trang_thai"* ]] || [[ "$output" == *"TrangThai"* ]]
  [[ "$output" == *"RETURNING"* ]]
}

@test "cau SQL gianh job chi lay dung MOT dong" {
  run sql_gianh_job
  [[ "$output" == *"LIMIT 1"* ]]
}

# Hoi quy cho mot loi THAT da quan sat tren Postgres: `psql -tA -c "UPDATE ... RETURNING ..."`
# in them dong the lenh "UPDATE 0" o cuoi, nen hang doi RONG lai trong nhu co mot cong viec ten
# "UPDATE 0". Cach chua: boc trong CTE de lenh CUOI CUNG la SELECT.
@test "cau SQL gianh job ket thuc bang SELECT (khong de psql in the lenh UPDATE 0)" {
  run sql_gianh_job
  [[ "$output" == *"WITH gianh AS ("* ]]
  [[ "$output" == *"FROM gianh"* ]]
}

@test "cau SQL don job mo coi cung ket thuc bang SELECT" {
  run sql_don_job_mo_coi
  [[ "$output" == *"WITH don AS ("* ]]
  [[ "$output" == *"FROM don"* ]]
}

@test "la_uuid chi nhan ma cong viec that" {
  run la_uuid "80dd7ae2-feb9-41b4-8b01-7a8fca593247"
  [ "$status" -eq 0 ]
  run la_uuid "UPDATE 0"
  [ "$status" -ne 0 ]
  run la_uuid ""
  [ "$status" -ne 0 ]
}

# T-2 (review bao mat): ma cong viec la bien DUY NHAT duoc noi thang vao cau SQL o ket_thuc_job
# ma khong escape. Mau cu "[0-9a-fA-F]*-*-*-*-*" nhan ca chuoi co dau nhay don, tuc la lop chan
# nay chi con la hinh thuc. Hien gia tri luon den tu cot uuid cua CSDL nen chua khai thac duoc --
# cac ca duoi day de no khong bao gio tro thanh khai thac duoc.
@test "T-2: la_uuid tu choi chuoi gia dang uuid (co dau nhay don, sai do dai, ky tu la)" {
  run la_uuid "a'--x-y-z-w"
  [ "$status" -ne 0 ]
  run la_uuid "80dd7ae2-feb9-41b4-8b01-7a8fca593247'; DROP TABLE cong_viec_sao_luu; --"
  [ "$status" -ne 0 ]
  run la_uuid "80dd7ae2-feb9-41b4-8b01-7a8fca59324"      # thieu 1 ky tu
  [ "$status" -ne 0 ]
  run la_uuid "80dd7ae2-feb9-41b4-8b01-7a8fca5932477"    # thua 1 ky tu
  [ "$status" -ne 0 ]
  run la_uuid "80dd7ag2-feb9-41b4-8b01-7a8fca593247"     # 'g' khong phai hex
  [ "$status" -ne 0 ]
  run la_uuid "-------"
  [ "$status" -ne 0 ]
}

@test "T-2: la_uuid van nhan uuid that o ca hai kieu chu hoa/thuong" {
  run la_uuid "80DD7AE2-FEB9-41B4-8B01-7A8FCA593247"
  [ "$status" -eq 0 ]
  run la_uuid "00000000-0000-0000-0000-000000000000"
  [ "$status" -eq 0 ]
}

@test "doc_snapshot_tu_tham_so lay dung id" {
  [ "$(doc_snapshot_tu_tham_so '{"snapshotId":"ab12cd34","nhan":null}')" = "ab12cd34" ]
}

@test "doc_snapshot_tu_tham_so tra rong khi khong co" {
  [ -z "$(doc_snapshot_tu_tham_so '{"nhan":"x"}')" ]
}

@test "doc_snapshot_tu_tham_so chiu duoc JSON rong hoac hong" {
  [ -z "$(doc_snapshot_tu_tham_so '')" ]
  [ -z "$(doc_snapshot_tu_tham_so 'khong-phai-json')" ]
}

# snapshotId di THANG vao dong lenh restic/qlgx-restore.sh. Mot gia tri co dau nhay, dau cach
# hay dau ';' phai bi tu choi ngay tai day chu khong duoc de xuong toi do.
@test "doc_snapshot_tu_tham_so tu choi gia tri co ky tu la" {
  [ -z "$(doc_snapshot_tu_tham_so '{"snapshotId":"ab12; rm -rf /"}')" ]
  [ -z "$(doc_snapshot_tu_tham_so '{"snapshotId":"a b"}')" ]
  [ -z "$(doc_snapshot_tu_tham_so '{"snapshotId":null}')" ]
  [ "$(doc_snapshot_tu_tham_so '{"snapshotId":"latest"}')" = "latest" ]
}

# Khoa cua ca tinh nang: mot dong ket o 'dang_chay' chan MOI cong viec moi (guard cua
# SaoLuuService.TaoCongViec coi ca 'cho' lan 'dang_chay' la "dang ban"), nen phai co duong tu
# thoat -- neu khong, mot lan bo chay bi kill -9 la giao xu KHONG BAO GIO sao luu duoc nua.
@test "cau SQL don job mo coi nham dung dong dang_chay qua han" {
  run sql_don_job_mo_coi
  [[ "$output" == *"trang_thai = 'loi'"* ]]
  [[ "$output" == *"trang_thai = 'dang_chay'"* ]]
  [[ "$output" == *"make_interval"* ]]
  [[ "$output" == *"bi bo do"* ]]
}

@test "sql_don_job_mo_coi chi nhan so gio la so nguyen" {
  run sql_don_job_mo_coi "2; DROP TABLE cong_viec_sao_luu"
  [[ "$output" != *"DROP TABLE"* ]]
  [[ "$output" == *"make_interval(hours => 2)"* ]]

  run sql_don_job_mo_coi 6
  [[ "$output" == *"make_interval(hours => 6)"* ]]
}
