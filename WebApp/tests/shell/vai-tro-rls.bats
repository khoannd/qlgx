#!/usr/bin/env bats
# Chay THAT tren mot PostgreSQL that (container dung mot lan) — kiem tra thuoc tinh vai tro,
# vi day chinh la thu de sai ma khong ai thay: mot vai tro nghiep vu lo co BYPASSRLS thi RLS
# vo hieu hoan toan ma he thong van chay binh thuong.

# N9: bo test nay chay THAT tren mot PostgreSQL that, nen can `docker` -- khong co ben trong
# container bats. Bo qua CO DIEU KIEN (kem ly do) thay vi de setup_file do: mot suite do mac dinh
# se nhanh chong bi bo qua, va luc do mot loi THAT se lan vao ma khong ai nhin ky.
can_docker() { command -v docker >/dev/null 2>&1 || skip "can lenh docker (chay bo test nay tren host)"; }

setup_file() {
  command -v docker >/dev/null 2>&1 || return 0
  export TEN_CT="qlgx-thu-rls-$$"
  docker run -d --name "$TEN_CT" -e POSTGRES_PASSWORD=thu_nghiem_tam \
    -e POSTGRES_DB=qlgx postgres:17-alpine >/dev/null
  for _ in $(seq 1 30); do
    docker exec "$TEN_CT" pg_isready -U postgres -d qlgx >/dev/null 2>&1 && break
    sleep 1
  done
}

teardown_file() { command -v docker >/dev/null 2>&1 && docker rm -f "${TEN_CT:-}" >/dev/null 2>&1 || true; }

chay_sql() {
  docker exec -i "$TEN_CT" psql -v ON_ERROR_STOP=1 -U postgres -d qlgx \
    -v qlgx_app_user=qlgx_app -v qlgx_app_password=mk_app \
    -v qlgx_admin_user=qlgx_admin -v qlgx_admin_password=mk_admin \
    -f - < "${BATS_TEST_DIRNAME}/../../scripts/sql/00-vai-tro-rls.sql"
}

hoi() { docker exec "$TEN_CT" psql -tAX -U postgres -d qlgx -c "$1"; }

@test "tao duoc hai vai tro voi dung thuoc tinh bypassrls" {
  can_docker
  run chay_sql
  [ "$status" -eq 0 ]
  [ "$(hoi "SELECT rolbypassrls FROM pg_roles WHERE rolname='qlgx_app'")" = "f" ]
  [ "$(hoi "SELECT rolbypassrls FROM pg_roles WHERE rolname='qlgx_admin'")" = "t" ]
  [ "$(hoi "SELECT rolcanlogin FROM pg_roles WHERE rolname='qlgx_app'")" = "t" ]
  [ "$(hoi "SELECT rolsuper FROM pg_roles WHERE rolname='qlgx_admin'")" = "f" ]
}

@test "chay lai lan hai khong loi va khong doi thuoc tinh" {
  can_docker
  chay_sql
  run chay_sql
  [ "$status" -eq 0 ]
  [ "$(hoi "SELECT rolbypassrls FROM pg_roles WHERE rolname='qlgx_app'")" = "f" ]
}

@test "vai tro quan tri ghi duoc vao bang do vai tro chay migration (qlgx_app) tao ra" {
  can_docker
  # QUAN TRONG: kich ban that trong trien khai chinh thuc la EF Core migration chay LUC API
  # khoi dong, dung dung ConnectionStrings__Qlgx tro toi qlgx_app (xem docker-compose.yml) —
  # nghia la BANG MOI luon do qlgx_app so huu, khong phai do sieu nguoi dung "postgres" tao.
  # Neu test nay tao bang bang "postgres" (nhu ban dau) thi no se DAT ngay ca khi
  # ALTER DEFAULT PRIVILEGES thieu "FOR ROLE qlgx_app", vi luc do "postgres" (nguoi chay
  # 00-vai-tro-rls.sql) trung voi "postgres" (nguoi tao bang) — mot phep thu gia, khong bat
  # duoc loi that. Phai tao bang BANG chinh qlgx_app moi phu dung kich ban san xuat, va phai
  # kiem qlgx_admin (KHONG phai chu bang) ghi duoc, vi do la vai tro thuc su can quyen chua tung
  # duoc cap tay.
  chay_sql
  docker exec "$TEN_CT" psql -tAX -U qlgx_app -d qlgx -c "CREATE TABLE bang_moi_sau(id int)"
  run docker exec "$TEN_CT" psql -tAX -U qlgx_admin -d qlgx -c "INSERT INTO bang_moi_sau VALUES (1)"
  [ "$status" -eq 0 ]
}

@test "tu choi neu ten hai vai tro trung nhau" {
  can_docker
  # Trung ten nghia la RLS vo hieu hoan toan (chi con mot vai tro, tat nhien khong the vua co
  # vua khong co BYPASSRLS) — SQL phai bao loi ro rang thay vi am tham tao mot vai tro lon xon.
  run docker exec -i "$TEN_CT" psql -v ON_ERROR_STOP=1 -U postgres -d qlgx \
    -v qlgx_app_user=trung_ten -v qlgx_app_password=mk_app \
    -v qlgx_admin_user=trung_ten -v qlgx_admin_password=mk_admin \
    -f - < "${BATS_TEST_DIRNAME}/../../scripts/sql/00-vai-tro-rls.sql"
  [ "$status" -ne 0 ]
}
