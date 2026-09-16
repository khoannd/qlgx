#!/usr/bin/env bats
# C1(a) cua review backend -- JOB KET O TRANG THAI 'cho' KHONG BAO GIO DUOC DON.
#
# Cau SQL don job mo coi truoc day chi quet `WHERE trang_thai = 'dang_chay'`. Kich ban hong (da
# duoc phia C# xac nhan bang test): nguoi cai tra loi "khong" o cau hoi "bat sao luu tu dong?"
# (thiet ke muc 4.2 buoc 11 CHO PHEP dieu do). Cu bam "Sao luu ngay" dau tien tao mot dong 'cho'
# ma khong bo chay nao nhat. Guard "khong xep hang hai cong viec chong nhau" cua SaoLuuService
# (C#) tu do TU CHOI MOI thao tac tren man hinh Sao luu & Phuc hoi -- VINH VIEN. Khong co route
# nao huy cong viec, nen duong thoat duy nhat la SSH + psql: dung thu quy cha, quy so khong lam
# duoc.

setup() {
  export QLGX_CHI_NAP_HAM=1
  # shellcheck disable=SC1090
  source "${BATS_TEST_DIRNAME}/../../scripts/qlgx-runner.sh"
}

@test "C1a: cau don job mo coi phai quet CA trang thai 'cho'" {
  run sql_don_job_mo_coi
  [ "$status" -eq 0 ]
  [[ "$output" == *"trang_thai = 'cho'"* ]]
  [[ "$output" == *"trang_thai = 'dang_chay'"* ]]
}

# Nguong cua nhanh 'cho' PHAI trung SaoLuuService.PhutChoToiDa (C#) = 15 phut. Hai phia lech nhau
# se tao ra mot khoang thoi gian ma mot ben coi la mo coi con ben kia van chan hang doi.
@test "C1a: nguong 'cho' la 15 phut, trung voi SaoLuuService.PhutChoToiDa ben C#" {
  [ "$PHUT_JOB_CHO_QUA_HAN" -eq 15 ]
  run sql_don_job_mo_coi
  [[ "$output" == *"mins => 15"* ]]
  # Doi chieu TRUC TIEP voi hang so ben C# -- neu ai do doi mot ben ma quen ben kia, test do.
  hang_so=$(grep -o 'PhutChoToiDa = [0-9]*' \
    "${BATS_TEST_DIRNAME}/../../src/Qlgx.Api/Services/SaoLuuService.cs" | grep -o '[0-9]*')
  [ "$hang_so" = "15" ]
  [ "$hang_so" = "$PHUT_JOB_CHO_QUA_HAN" ]
}

@test "C1a: nhanh 'dang_chay' van giu nguong 2 gio (hai nguyen nhan khac han nhau)" {
  run sql_don_job_mo_coi
  [[ "$output" == *"hours => 2"* ]]
}

@test "C1a: hai nhanh co hai cau nhat ky rieng, noi dung nguyen nhan THAT" {
  run sql_don_job_mo_coi
  [[ "$output" == *"bi bo do"* ]]
  [[ "$output" == *"bo chay sao luu tren may chu khong hoat dong"* ]]
}

# Tham so la (do goi tay hay bien moi truong hong) khong duoc phep chen vao cau lenh SQL.
@test "C1a: tham so khong phai so nguyen ve gia tri mac dinh, khong chen vao SQL" {
  run sql_don_job_mo_coi "2; DROP TABLE cong_viec_sao_luu; --" "9; DELETE FROM ban_sao_luu; --"
  [ "$status" -eq 0 ]
  [[ "$output" != *"DROP TABLE"* ]]
  [[ "$output" != *"DELETE FROM ban_sao_luu"* ]]
  [[ "$output" == *"hours => 2"* ]]
  [[ "$output" == *"mins => 15"* ]]
}
