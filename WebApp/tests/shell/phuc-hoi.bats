#!/usr/bin/env bats
# Kiem thu cac HAM THUAN cua qlgx-restore.sh -- khong dong toi Docker, Postgres hay restic.

setup() {
  export QLGX_CHI_NAP_HAM=1
  # shellcheck disable=SC1090
  source "${BATS_TEST_DIRNAME}/../../scripts/qlgx-restore.sh"
}

@test "doc_the_phuc_hoi trich dung khoa tu the (ban rut gon)" {
  cat > "$BATS_TEST_TMPDIR/the.txt" <<'EOF'
================== THE PHUC HOI QLGX ==================
MAT KHAU RESTIC (khong co dong nay thi KHONG AI phuc hoi duoc):
  bi-mat-restic-abc123
KHO SAO LUU : s3:https://vd.r2.cloudflarestorage.com/qlgx-sao-luu
R2 KEY ID   : ID_ABC
R2 SECRET   : SECRET_XYZ
EOF
  doc_the_phuc_hoi "$BATS_TEST_TMPDIR/the.txt"
  [ "$RESTIC_PASSWORD" = "bi-mat-restic-abc123" ]
  [ "$RESTIC_REPOSITORY" = "s3:https://vd.r2.cloudflarestorage.com/qlgx-sao-luu" ]
  [ "$AWS_ACCESS_KEY_ID" = "ID_ABC" ]
  [ "$AWS_SECRET_ACCESS_KEY" = "SECRET_XYZ" ]
}

# Bai hoc that: the phuc hoi THAT (in_the_phuc_hoi trong install.sh) co cau giai thich keo dai
# sang DONG THU HAI truoc khi toi dong mat khau, va gia tri kho la mot URL CO CHUA dau ':'
# ("s3:https://..."). Mot ban trich xuat ngay tho (awk -F': *' + "lay dong ke tiep") doc sai CA
# HAI: mat khau thanh dong giai thich, kho thanh "s3". Test nay chot dung dinh dang THAT.
@test "doc_the_phuc_hoi doc dung the THAT do install.sh sinh ra" {
  cat > "$BATS_TEST_TMPDIR/that.txt" <<'EOF'
================== THE PHUC HOI QLGX ==================
May chu   : vd-may-chu  (giaoxu.example.net)
Lap ngay  : 13/09/2026 10:00

MAT KHAU RESTIC (khong co dong nay thi KHONG AI phuc hoi duoc,
ke ca Cloudflare -- day la ban chat cua ma hoa phia may chu):
  aB3+xy/Zq9==

KHO SAO LUU : s3:https://abc123.r2.cloudflarestorage.com/qlgx-sao-luu
R2 KEY ID   : KEYID001
R2 SECRET   : sec:ret/voi+ky=tu-la

MAT KHAU CSDL:
  postgres   : qlgx_chu / mkchu
=======================================================
EOF
  doc_the_phuc_hoi "$BATS_TEST_TMPDIR/that.txt"
  [ "$RESTIC_PASSWORD" = "aB3+xy/Zq9==" ]
  [ "$RESTIC_REPOSITORY" = "s3:https://abc123.r2.cloudflarestorage.com/qlgx-sao-luu" ]
  [ "$AWS_ACCESS_KEY_ID" = "KEYID001" ]
  [ "$AWS_SECRET_ACCESS_KEY" = "sec:ret/voi+ky=tu-la" ]
}

@test "doc_the_phuc_hoi bao loi khi the thieu mat khau restic" {
  printf 'KHO SAO LUU : s3:x\n' > "$BATS_TEST_TMPDIR/hong.txt"
  run doc_the_phuc_hoi "$BATS_TEST_TMPDIR/hong.txt"
  [ "$status" -ne 0 ]
}

@test "doc_the_phuc_hoi bao loi khi dong mat khau bi cat mat (khong nhat nham dong ke tiep)" {
  cat > "$BATS_TEST_TMPDIR/cut.txt" <<'EOF'
MAT KHAU RESTIC (khong co dong nay thi KHONG AI phuc hoi duoc):
KHO SAO LUU : s3:https://vd/qlgx
EOF
  run doc_the_phuc_hoi "$BATS_TEST_TMPDIR/cut.txt"
  [ "$status" -ne 0 ]
}

@test "doc_the_phuc_hoi bao loi khi khong doc duoc tep" {
  run doc_the_phuc_hoi "$BATS_TEST_TMPDIR/khong-co-tep-nay.txt"
  [ "$status" -ne 0 ]
}

@test "ten_db_tam sinh ten hop le va khac nhau" {
  a=$(ten_db_tam qlgx_phuc_hoi); sleep 1; b=$(ten_db_tam qlgx_phuc_hoi)
  [ "$a" != "$b" ]
  [[ "$a" =~ ^qlgx_phuc_hoi_[0-9]{8}_[0-9]{6}$ ]]
}

@test "mac dinh la CHI IN KE HOACH, khong co --apply thi ap_dung=0" {
  phan_tich_tham_so_phuc_hoi --snapshot ab12cd34
  [ "$AP_DUNG" -eq 0 ]
  phan_tich_tham_so_phuc_hoi --snapshot ab12cd34 --apply
  [ "$AP_DUNG" -eq 1 ]
}

@test "phan_tich_tham_so_phuc_hoi doc du cac tham so va tu dat lai AP_DUNG moi lan" {
  phan_tich_tham_so_phuc_hoi --snapshot s1 --card /tmp/the.txt --goc /opt/qlgx/WebApp \
    --ma-job 11111111-2222-3333-4444-555555555555 --dien-tap
  [ "$SNAPSHOT" = "s1" ]
  [ "$TEP_THE" = "/tmp/the.txt" ]
  [ "$GOC_UNG_DUNG_THAM_SO" = "/opt/qlgx/WebApp" ]
  [ "$MA_JOB" = "11111111-2222-3333-4444-555555555555" ]
  [ "$DIEN_TAP" -eq 1 ]
  [ "$AP_DUNG" -eq 0 ]
  # Goi lai KHONG co --dien-tap/--apply: hai co nay phai ve 0, khong duoc "dinh" lai tu lan truoc.
  phan_tich_tham_so_phuc_hoi --snapshot s2
  [ "$DIEN_TAP" -eq 0 ]
  [ "$AP_DUNG" -eq 0 ]
}

@test "phan_tich_tham_so_phuc_hoi bao loi voi tham so la va voi co thieu gia tri" {
  run phan_tich_tham_so_phuc_hoi --khong-co-tham-so-nay
  [ "$status" -ne 0 ]
  run phan_tich_tham_so_phuc_hoi --snapshot
  [ "$status" -ne 0 ]
}

# Postgres CAT NGAM dinh danh dai qua 63 byte. Neu ten CSDL luu vuot qua do, lenh doi ten thu
# nhat "thanh cong" voi mot ten KHAC ten ta nghi, va don_csdl_cu/quay lui se tim khong ra no.
@test "ten_db_luu khong bao gio vuot 63 ky tu va van khop mau don dep" {
  dai=$(printf 'q%.0s' $(seq 1 60))
  ten=$(ten_db_luu "$dai")
  [ "${#ten}" -le 63 ]
  mau=$(mau_db_luu "$dai")
  [[ "$ten" == "${mau%\%}"* ]]
}

@test "ten_db_luu giu nguyen goc ngan va mang hau to truoc_phuc_hoi" {
  ten=$(ten_db_luu qlgx)
  [[ "$ten" =~ ^qlgx_truoc_phuc_hoi_[0-9]{8}_[0-9]{6}$ ]]
  [ "$(mau_db_luu qlgx)" = "qlgx_truoc_phuc_hoi_%" ]
}

@test "kiem_ten_db_an_toan tu choi ten co ky tu la (chong chen SQL vao lenh doi ten)" {
  run kiem_ten_db_an_toan 'qlgx"; DROP DATABASE qlgx; --'
  [ "$status" -ne 0 ]
  run kiem_ten_db_an_toan ''
  [ "$status" -ne 0 ]
  run kiem_ten_db_an_toan qlgx_phuc_hoi_20260913_101010
  [ "$status" -eq 0 ]
}

# Vong review 1, Important 3: "cho toi da sau khi hoan doi" la thoi gian giao xu KHONG dung duoc
# phan mem, phai NGAN hon nhieu so voi cho luc khoi dong binh thuong. Chot lai bang test de khong
# ai vo tinh gop hai con so nay lam mot.
@test "cho sau hoan doi phai ngan hon nhieu so voi cho khoi dong binh thuong" {
  [ "$CHO_SAN_SANG_GIAY" -eq 300 ]
  [ "$CHO_SAN_SANG_SAU_HOAN_DOI_GIAY" -eq 90 ]
  [ "$CHO_SAN_SANG_SAU_HOAN_DOI_GIAY" -lt "$CHO_SAN_SANG_GIAY" ]
}

@test "cho_api_san_sang nhan gioi han qua tham so va that bai khi khong bao gio san sang" {
  # Ghi de lenh goi ra ngoai de test khong dung toi Docker: dc() gia luon that bai.
  dc() { return 1; }
  sleep() { :; }
  run cho_api_san_sang 2
  [ "$status" -ne 0 ]
}

# Vong review 1 (phat hien tu kich ban 3b): khi CSDL moi hong that, container api crash-restart va
# MOI lan tham do `dc exec` mat nhieu giay. Neu gioi han duoc dem theo SO VONG LAP thi "90" hoa ra
# hang chuc phut -- dung trong luc giao xu mat dich vu. Test nay chot lai rang gioi han la SO GIAY
# THAT: mot ham `dc` gia "cham" (moi lan 1 giay) van phai lam ham tra ve trong khoang gioi han,
# chu khong phai gioi-han-nhan-thoi-gian-moi-lan.
@test "cho_api_san_sang do bang DONG HO, khong phai bang so vong lap" {
  dc() { sleep 1; return 1; }   # moi lan tham do "ton" 1 giay
  bat_dau=$(date +%s)
  run cho_api_san_sang 3
  het=$(date +%s)
  [ "$status" -ne 0 ]
  troi_qua=$(( het - bat_dau ))
  # Neu dem theo vong lap thi phai mat >= 6 giay (3 vong x (1 giay dc + 1 giay sleep)).
  [ "$troi_qua" -lt 6 ]
  [ "$troi_qua" -ge 3 ]
}

@test "gia_tri_dong_the giu nguyen moi dau hai cham trong gia tri" {
  printf 'KHO SAO LUU : s3:https://a.b/c:d\n' > "$BATS_TEST_TMPDIR/x.txt"
  [ "$(gia_tri_dong_the "$BATS_TEST_TMPDIR/x.txt" 'KHO SAO LUU')" = "s3:https://a.b/c:d" ]
}
