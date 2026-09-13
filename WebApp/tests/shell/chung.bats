#!/usr/bin/env bats

setup() {
  load_path="${BATS_TEST_DIRNAME}/../../scripts/chung.sh"
  # shellcheck disable=SC1090
  source "$load_path"
  TEP="$BATS_TEST_TMPDIR/thu.env"
}

@test "set_env_kv them khoa moi vao tep rong" {
  : > "$TEP"
  set_env_kv "$TEP" "A" "1"
  run doc_env_kv "$TEP" "A"
  [ "$output" = "1" ]
}

@test "set_env_kv ghi de khoa da co, khong nhan doi dong" {
  printf 'A=1\nB=2\n' > "$TEP"
  set_env_kv "$TEP" "A" "9"
  [ "$(doc_env_kv "$TEP" A)" = "9" ]
  [ "$(doc_env_kv "$TEP" B)" = "2" ]
  [ "$(grep -c '^A=' "$TEP")" -eq 1 ]
}

@test "set_env_kv xu ly dung tep KHONG ket thuc bang xuong dong" {
  # Day chinh la loi da lam mat hai bi mat o mot du an khac: khoa moi bi noi duoi khoa cu
  # thanh mot dong hong.
  printf 'A=1' > "$TEP"
  set_env_kv "$TEP" "B" "2"
  [ "$(doc_env_kv "$TEP" A)" = "1" ]
  [ "$(doc_env_kv "$TEP" B)" = "2" ]
}

@test "set_env_kv khong lam hong gia tri co ky tu dac biet" {
  : > "$TEP"
  set_env_kv "$TEP" "P" 'a/b&c$d|e'
  [ "$(doc_env_kv "$TEP" P)" = 'a/b&c$d|e' ]
}

@test "doc_env_kv khong nham khoa co tien to giong nhau" {
  printf 'AB=2\nA=1\n' > "$TEP"
  [ "$(doc_env_kv "$TEP" A)" = "1" ]
  [ "$(doc_env_kv "$TEP" AB)" = "2" ]
}

@test "dat_neu_chua_co GIU NGUYEN gia tri da co" {
  printf 'K=bimat-cu\n' > "$TEP"
  dat_neu_chua_co "$TEP" "K" "bimat-moi"
  [ "$(doc_env_kv "$TEP" K)" = "bimat-cu" ]
}

@test "dat_neu_chua_co ghi khi khoa rong" {
  printf 'K=\n' > "$TEP"
  dat_neu_chua_co "$TEP" "K" "gia-tri-moi"
  [ "$(doc_env_kv "$TEP" K)" = "gia-tri-moi" ]
}

@test "sinh_bi_mat tra ve chuoi khac nhau moi lan va du dai" {
  a=$(sinh_bi_mat 32); b=$(sinh_bi_mat 32)
  [ "$a" != "$b" ]
  [ "${#a}" -ge 40 ]
}

@test "bao_loi_va_thoat thoat voi ma khac 0" {
  run bao_loi_va_thoat "co loi"
  [ "$status" -ne 0 ]
  [[ "$output" == *"co loi"* ]]
}
