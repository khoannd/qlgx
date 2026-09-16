#!/usr/bin/env bats

setup() {
  # shellcheck disable=SC1090
  source "${BATS_TEST_DIRNAME}/../../scripts/os_adapter.sh"
}

@test "os_ho nhan dien debian tu ID_LIKE" {
  TEP_OS_RELEASE="$BATS_TEST_TMPDIR/os-release"
  printf 'ID=ubuntu\nID_LIKE=debian\n' > "$TEP_OS_RELEASE"
  [ "$(os_ho)" = "debian" ]
}

@test "os_ho nhan dien rhel tu ID_LIKE" {
  TEP_OS_RELEASE="$BATS_TEST_TMPDIR/os-release"
  printf 'ID=rocky\nID_LIKE="rhel centos fedora"\n' > "$TEP_OS_RELEASE"
  [ "$(os_ho)" = "rhel" ]
}

@test "os_ho nhan dien debian thuan tu ID" {
  TEP_OS_RELEASE="$BATS_TEST_TMPDIR/os-release"
  printf 'ID=debian\n' > "$TEP_OS_RELEASE"
  [ "$(os_ho)" = "debian" ]
}

@test "os_ho bao khong ro voi distro la" {
  TEP_OS_RELEASE="$BATS_TEST_TMPDIR/os-release"
  printf 'ID=plan9\n' > "$TEP_OS_RELEASE"
  run os_ho
  [ "$status" -ne 0 ]
}

@test "os_co_lenh dung voi lenh chac chan co va khong co" {
  run os_co_lenh sh
  [ "$status" -eq 0 ]
  run os_co_lenh lenh_khong_bao_gio_ton_tai_zzz
  [ "$status" -ne 0 ]
}
