#!/usr/bin/env bash
# Lop mong che khac biet giua cac ban phan phoi Linux. Moi noi khac trong bo script goi qua
# day, KHONG rai lenh apt/dnf khap noi -- de them mot distro chi phai sua mot tep.

# Cho phep bo test tro sang tep gia; mac dinh la duong dan that cua he thong.
TEP_OS_RELEASE="${TEP_OS_RELEASE:-/etc/os-release}"

os_ho() {
  [ -r "$TEP_OS_RELEASE" ] || { echo "Khong doc duoc $TEP_OS_RELEASE" >&2; return 1; }
  local id id_like
  id=$(awk -F= '$1=="ID"{gsub(/"/,"",$2); print $2}' "$TEP_OS_RELEASE")
  id_like=$(awk -F= '$1=="ID_LIKE"{gsub(/"/,"",$2); print $2}' "$TEP_OS_RELEASE")
  case " $id $id_like " in
    *" debian "*|*" ubuntu "*) echo debian; return 0 ;;
    *" rhel "*|*" fedora "*|*" centos "*) echo rhel; return 0 ;;
  esac
  echo "Ban phan phoi khong duoc ho tro: ID=$id ID_LIKE=$id_like" >&2
  return 1
}

os_co_lenh() { command -v "$1" >/dev/null 2>&1; }

os_cai_goi() {
  case "$(os_ho)" in
    debian)
      DEBIAN_FRONTEND=noninteractive apt-get update -qq
      DEBIAN_FRONTEND=noninteractive apt-get install -y --no-install-recommends "$@"
      ;;
    rhel) dnf install -y "$@" ;;
    *) return 1 ;;
  esac
}
