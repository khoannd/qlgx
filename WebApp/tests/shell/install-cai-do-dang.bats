#!/usr/bin/env bats
# C3 -- CAI DO DANG phai duoc nhan ra la "chua cai xong", khong phai "da cai roi".
# C4 -- CHI nhom kiem chung COT LOI moi duoc quyen lam mot ban cap nhat tu quay lui.
# I9 -- URL tren The phuc hoi phai la dang raw.githubusercontent.com (dang co ".git" tra 404).
# I13 -- --branch= phai co tac dung TRUOC khi nap thu vien.

setup() {
  export QLGX_CHI_NAP_HAM=1
  export GOC_CHECKOUT="$BATS_TEST_TMPDIR/opt-qlgx"
  export GOC_UNG_DUNG="$GOC_CHECKOUT/WebApp"
  export THU_MUC_CAU_HINH="$BATS_TEST_TMPDIR/etc-qlgx"
  mkdir -p "$GOC_UNG_DUNG" "$THU_MUC_CAU_HINH"
  # shellcheck disable=SC1090
  source "${BATS_TEST_DIRNAME}/../../scripts/install.sh"
  # source lai sau khi da dat bien: cac bien o dau tep doc GOC_UNG_DUNG tu moi truong.
}

# ============================== C3 ==============================
# Kich ban that: VPS mang chap chon, `dc build api` that bai -> script chet o buoc 8/12. Nguoi van
# hanh (khong ranh may tinh) lam dieu duy nhat hop ly: chay lai dung lenh cai dat. Neu .env la co
# duy nhat phan biet cai moi voi cap nhat thi lan nay `la_cai_moi` tra false -> vao `cap_nhat` ->
# thay HEAD == origin -> in "Da la ban moi nhat" -> exit 0. May chu NUA CAI ma script BAO THANH
# CONG, va The phuc hoi KHONG BAO GIO duoc in -- mat may chu la mat vinh vien kha nang giai ma R2.
@test "C3: cai do dang (moi co .env) van phai duoc coi la CAI MOI" {
  printf 'POSTGRES_DB=qlgx\n' > "$GOC_UNG_DUNG/.env"
  run la_cai_moi
  [ "$status" -eq 0 ]   # 0 = dung, day la cai moi -> chay lai tu dau
}

@test "C3: cai do dang co ca .env lan backup.env nhung chua cai CLI van la CAI MOI" {
  printf 'POSTGRES_DB=qlgx\n' > "$GOC_UNG_DUNG/.env"
  printf 'RESTIC_REPOSITORY=s3:vd\n' > "$THU_MUC_CAU_HINH/backup.env"
  run la_cai_moi
  [ "$status" -eq 0 ]
}

@test "C3: co hoan tat ton tai thi KHONG phai cai moi (di duong cap nhat)" {
  printf 'POSTGRES_DB=qlgx\n' > "$GOC_UNG_DUNG/.env"
  danh_dau_cai_xong
  [ -f "$TEP_HOAN_TAT" ]
  run la_cai_moi
  [ "$status" -ne 0 ]
}

# Di tru: cac may DA cai xong TRUOC khi co co hoan tat khong duoc phep bi keo vao lai toan bo
# luong cai moi (hoi lai ten giao xu, hoi lai khoa R2) chi vi thieu mot tep danh dau.
@test "C3: may da cai xong tu truoc (chua co tep danh dau) van di duong cap nhat" {
  printf 'POSTGRES_DB=qlgx\n' > "$GOC_UNG_DUNG/.env"
  printf 'RESTIC_REPOSITORY=s3:vd\n' > "$THU_MUC_CAU_HINH/backup.env"
  cai_dat_cu_da_hoan_tat() { return 0; }   # gia lap /usr/local/bin/qlgx da co
  run la_cai_moi
  [ "$status" -ne 0 ]
}

# ============================== C4 ==============================
# Kich ban gan nhu chac chan xay ra: cha xu in The phuc hoi ra giay nhung khong xoa
# /etc/qlgx/the-phuc-hoi.txt. Sang ngay thu 8 dong do KHONG DAT vinh vien. Neu muc do duoc phep
# kich hoat quay lui thi MOI lan cap nhat ve sau deu: build tot -> quay lui vo co (ngung dich vu
# vai phut) -> dan nhan HONG cho mot commit lanh lan -> tu choi moi lan cap nhat toi commit do.
# Giao xu bi khoa cung o ban cu, khong bao gio nhan duoc ban va loi nao nua.
# May chu GIA: moi muc COT LOI deu dat, moi muc BO SUNG deu truot.
may_chu_cot_loi_dat_phu_truot() {
  dc() {
    case "$*" in
      *"ps --status running postgres"*) printf 'qlgx-postgres-1 postgres running\n' ;;
      *"ps --status running api"*)      printf 'qlgx-api-1 api running\n' ;;
      *"ps --status running caddy"*)    return 1 ;;          # BO SUNG: truot
      *suc-khoe*)                       return 0 ;;
      *"rolname='qlgx_app'"*)           printf 'f\n' ;;
      *"rolname='qlgx_admin'"*)         printf 't\n' ;;
      *relrowsecurity*)                 printf '40\n' ;;
      *EFMigrationsHistory*)            printf '12\n' ;;
      *)                                return 0 ;;
    esac
  }
  stat() { printf 'khac-600'; }                               # BO SUNG: quyen tep truot
  curl() { return 1; }                                        # BO SUNG: HTTPS ngoai truot
  doc_env_kv() {
    case "$2" in
      QLGX_APP_DB_USER)   printf 'qlgx_app' ;;
      QLGX_ADMIN_DB_USER) printf 'qlgx_admin' ;;
      *)                  printf 'qlgx' ;;
    esac
  }
  # BO SUNG: The phuc hoi van nam tren may chu va da qua 7 ngay -- dung kich ban da mo ta o C4.
  printf 'the\n' > "$THU_MUC_CAU_HINH/the-phuc-hoi.txt"
  touch -d '30 days ago' "$THU_MUC_CAU_HINH/the-phuc-hoi.txt" 2>/dev/null || true
  export QLGX_BO_QUA_R2=1
  TEN_MIEN="giaoxu.example.net"
}

@test "C4: nhieu muc BO SUNG truot van KHONG duoc lam nhom cot loi truot" {
  may_chu_cot_loi_dat_phu_truot
  run tu_kiem_chung_cot_loi
  [ "$status" -eq 0 ]
  # Che do cot loi khong duoc in cac muc bo sung ra bang.
  [[ "$output" != *"The phuc hoi da duoc cat"* ]]
  [[ "$output" != *"Container caddy"* ]]
}

@test "C4: bang DAY DU van phai bao cac muc bo sung do (khong im lang bo qua)" {
  may_chu_cot_loi_dat_phu_truot
  run tu_kiem_chung
  [[ "$output" == *"CAN XEM LAI"* ]]
  [[ "$output" == *"The phuc hoi da duoc cat"* ]]
  [[ "$output" == *"Container caddy"* ]]
}

# Doi chung: mot muc COT LOI that su truot (RLS bi tat) van phai lam tu_kiem_chung_cot_loi truot.
# Khong co test nay thi "chi nhom cot loi moi chan" co the bi cai dat thanh "khong bao gio chan".
@test "C4: muc COT LOI truot (RLS bi tat) VAN phai lam kiem chung cot loi truot" {
  may_chu_cot_loi_dat_phu_truot
  dc() {
    case "$*" in
      *"ps --status running postgres"*) printf 'postgres\n' ;;
      *"ps --status running api"*)      printf 'api\n' ;;
      *suc-khoe*)                       return 0 ;;
      *"rolname='qlgx_app'"*)           printf 'f\n' ;;
      *"rolname='qlgx_admin'"*)         printf 't\n' ;;
      *relrowsecurity*)                 printf '0\n' ;;   # COT LOI: RLS tat het
      *EFMigrationsHistory*)            printf '12\n' ;;
      *)                                return 0 ;;
    esac
  }
  run tu_kiem_chung_cot_loi
  [ "$status" -ne 0 ]
  [[ "$output" == *"KHONG DAT"* ]]
}

@test "C4: quay_lui khong dan nhan HONG khi ly do khong phai 'ban moi khong len duoc'" {
  git() {
    case "$*" in
      *"rev-parse HEAD"*) printf 'cafe1234cafe1234\n' ;;
      *) return 0 ;;
    esac
  }
  dc() { return 0; }
  cho_san_sang() { return 0; }
  printf 'abc\n' > "$GOC_UNG_DUNG/.phien-ban-truoc"

  run quay_lui deadbeef 0
  [ "$status" -eq 0 ]
  [ ! -f "$GOC_UNG_DUNG/.phien-ban-hong" ]

  run quay_lui deadbeef 1
  [ "$status" -eq 0 ]
  [ -f "$GOC_UNG_DUNG/.phien-ban-hong" ]
}

# I12: sau khi quay lui THANH CONG van phai nhac phuc hoi CSDL -- migration cua ban moi (neu da
# chay) da doi luoc do va quay lui ma nguon khong dao no lai.
@test "I12: quay lui thanh cong van phai nhac chay qlgx restore" {
  git() { case "$*" in *"rev-parse HEAD"*) printf 'cafe1234\n' ;; *) return 0 ;; esac; }
  dc() { return 0; }
  cho_san_sang() { return 0; }
  run quay_lui deadbeef 1
  [ "$status" -eq 0 ]
  [[ "$output" == *"qlgx restore"* ]]
  [[ "$output" == *"LUOC"* ]]
}

# ============================== I9 ==============================
@test "I9: URL tren The phuc hoi phai la raw.githubusercontent.com, KHONG duoc co '.git'" {
  printf 'RESTIC_PASSWORD=mk\nRESTIC_REPOSITORY=s3:vd\n' > "$THU_MUC_CAU_HINH/backup.env"
  printf 'POSTGRES_USER=qlgx_chu\n' > "$GOC_UNG_DUNG/.env"
  run in_the_phuc_hoi
  [ "$status" -eq 0 ]
  [[ "$output" == *"raw.githubusercontent.com/khoannd/qlgx/$NHANH/WebApp/scripts/install.sh"* ]]
  [[ "$output" == *"raw.githubusercontent.com/khoannd/qlgx/$NHANH/WebApp/scripts/qlgx-restore.sh"* ]]
  # Dang co ".git" nam giua duong dan da duoc kiem chung bang curl that la tra ve 404.
  [[ "$output" != *"qlgx.git/raw/"* ]]
}

# ============================== I13 ==============================
@test "I13: --branch= duoc quet TRUOC khi nap thu vien" {
  NHANH="$NHANH_MAC_DINH"
  quet_nhanh_som --non-interactive --branch=nhanh-thu --domain=a.b
  [ "$NHANH" = "nhanh-thu" ]
  NHANH="$NHANH_MAC_DINH"
  QLGX_NHANH="tu-bien-moi-truong" quet_nhanh_som --non-interactive
  [ "$NHANH" = "tu-bien-moi-truong" ]
}
