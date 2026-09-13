-- Tao hai vai tro co so du lieu bat buoc cho mo hinh Row-Level Security cua QLGX.
--
-- Vi sao phai la hai vai tro rieng: RLS chi co tac dung voi vai tro KHONG co BYPASSRLS. Vai tro
-- nghiep vu (qlgx_app) phuc vu moi truy van da xac thuc nen bat buoc bi RLS chan — do la lop
-- phong thu thu hai chong ro ri du lieu giua cac giao xu (xem migration BatRlsChoBangTheoGiaoXu).
-- Nhung nam duong dan hop le van can doc/ghi cheo giao xu (dang nhap, tao tai khoan quan tri dau
-- tien, chuyen du lieu, quan ly giao xu, nhap du lieu Access) nen can mot vai tro thu hai CO
-- BYPASSRLS — xem ChuoiKetNoiQuanTri.cs va TRIEN-KHAI.md muc 5.
--
-- Idempotent: chay lai nhieu lan khong loi, va LUON dat lai dung thuoc tinh BYPASSRLS ke ca khi
-- vai tro da ton tai voi thuoc tinh sai (ALTER ROLE ghi de toan bo, khong chi doi mat khau).
--
-- QUAN TRONG ve ALTER DEFAULT PRIVILEGES: lenh nay CHI ap dung cho doi tuong do VAI TRO DANG
-- CHAY LENH tao ra, tru khi chi dinh "FOR ROLE <ten>". Script nay thuong duoc goi boi mot sieu
-- nguoi dung (vi du "postgres" — dung nhu cach docker-compose/TRIEN-KHAI.md huong dan), nhung
-- ban than sieu nguoi dung do KHONG tao bang nghiep vu nao ca. Nguoi tao bang la vai tro chay
-- migration EF Core luc container API khoi dong — theo docker-compose.yml
-- (ConnectionStrings__Qlgx tro toi QLGX_APP_DB_USER), do CHINH LA vai tro nghiep vu (qlgx_app).
-- Vi vay o duoi chi dinh ro "FOR ROLE <ten_app>" (va ca "FOR ROLE <ten_admin>" de phong truong
-- hop mot ngay nao do migration duoc chay bang chuoi ket noi quan tri) — thieu "FOR ROLE" thi
-- moi lan EF Core them bang moi, vai tro CON LAI (khong phai chu bang) se KHONG doc/ghi duoc
-- bang do cho toi khi cap quyen lai bang tay, va loi nay se im lang cho den khi co nguoi dung
-- that gap loi "permission denied" o mot man hinh dung bang moi.
--
-- Cach goi:
--   psql -v ON_ERROR_STOP=1 -U postgres -d qlgx \
--        -v qlgx_app_user=... -v qlgx_app_password=... \
--        -v qlgx_admin_user=... -v qlgx_admin_password=... -f 00-vai-tro-rls.sql

\set ON_ERROR_STOP on

-- psql KHONG thay the bien ":'ten'" ben trong khoi trich dan $$ ... $$ (dollar-quoting duoc
-- xem la mot chuoi van ban dac biet, giong nhu chuoi nhay don — psql khong doi noi dung ben
-- trong). Vi vay phai chuyen gia tri vao truoc qua set_config() (bien phien lam viec, con goi
-- la GUC tam), roi doc lai bang current_setting() TRONG khoi PL/pgSQL — day la cach lam pho
-- bien va an toan de dua tham so tu psql vao mot khoi DO.
SELECT
    set_config('qlgx.ten_app',   :'qlgx_app_user',      false),
    set_config('qlgx.mk_app',    :'qlgx_app_password',  false),
    set_config('qlgx.ten_admin', :'qlgx_admin_user',     false),
    set_config('qlgx.mk_admin',  :'qlgx_admin_password', false);

-- Dung format(%I/%L) de ten vai tro va mat khau duoc trich dan dung chuan, khong ghep chuoi tho:
-- %I trich dan dinh danh (ten vai tro), %L trich dan gia tri chuoi (mat khau) va tu dong nhan
-- doi dau nhay don ben trong — mat khau sinh ngau nhien co the chua ky tu do van an toan.
DO $$
DECLARE
    ten_app   text := current_setting('qlgx.ten_app');
    mk_app    text := current_setting('qlgx.mk_app');
    ten_admin text := current_setting('qlgx.ten_admin');
    mk_admin  text := current_setting('qlgx.mk_admin');
    ten_vt    text;
BEGIN
    IF ten_app = ten_admin THEN
        RAISE EXCEPTION 'Vai tro nghiep vu va vai tro quan tri KHONG duoc trung ten (%). '
                        'Trung ten nghia la Row-Level Security bi vo hieu hoan toan.', ten_app;
    END IF;

    -- Vai tro nghiep vu: KHONG BYPASSRLS. ALTER lai toan bo thuoc tinh moi lan chay, khong chi
    -- mat khau, de dam bao dung thuoc tinh ke ca khi vai tro da ton tai truoc do voi cau hinh sai.
    IF EXISTS (SELECT 1 FROM pg_roles WHERE rolname = ten_app) THEN
        EXECUTE format('ALTER ROLE %I LOGIN PASSWORD %L NOSUPERUSER NOBYPASSRLS', ten_app, mk_app);
    ELSE
        EXECUTE format('CREATE ROLE %I LOGIN PASSWORD %L NOSUPERUSER NOBYPASSRLS', ten_app, mk_app);
    END IF;

    -- Vai tro quan tri: CO BYPASSRLS, nhung van KHONG phai superuser.
    IF EXISTS (SELECT 1 FROM pg_roles WHERE rolname = ten_admin) THEN
        EXECUTE format('ALTER ROLE %I LOGIN PASSWORD %L NOSUPERUSER BYPASSRLS', ten_admin, mk_admin);
    ELSE
        EXECUTE format('CREATE ROLE %I LOGIN PASSWORD %L NOSUPERUSER BYPASSRLS', ten_admin, mk_admin);
    END IF;

    FOREACH ten_vt IN ARRAY ARRAY[ten_app, ten_admin] LOOP
        -- Tu PostgreSQL 15, schema "public" KHONG con tu dong cap CREATE cho vai tro khong phai
        -- chu schema (truoc day PUBLIC duoc cap san). Thieu dong nay, EF Core migration (chay
        -- bang qlgx_app) se bao loi "permission denied for schema public" ngay khi tao bang dau
        -- tien — da bat duoc loi nay bang test bats "vai tro quan tri ghi duoc vao bang do vai
        -- tro chay migration (qlgx_app) tao ra" luc viet script nay.
        EXECUTE format('GRANT USAGE, CREATE ON SCHEMA public TO %I', ten_vt);
        -- Bang/sequence DA TON TAI tu truoc (lan chay lai sau khi migration da tao bang) cung
        -- duoc cap lai o day, phong truong hop default privileges o lan chay truoc bi thieu.
        EXECUTE format('GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO %I', ten_vt);
        EXECUTE format('GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public TO %I', ten_vt);

        -- Bang/sequence duoc tao SAU lenh nay (boi migration EF Core) cung phai tu dong co
        -- quyen — chi dinh FOR ROLE cho CA HAI vai tro nghiep vu/quan tri lam nguoi tao, de
        -- dung du du lieu bat ke ben nao thuc su chay migration tao bang (xem ghi chu dau file).
        EXECUTE format(
            'ALTER DEFAULT PRIVILEGES FOR ROLE %I IN SCHEMA public GRANT ALL PRIVILEGES ON TABLES TO %I',
            ten_app, ten_vt);
        EXECUTE format(
            'ALTER DEFAULT PRIVILEGES FOR ROLE %I IN SCHEMA public GRANT ALL PRIVILEGES ON SEQUENCES TO %I',
            ten_app, ten_vt);
        EXECUTE format(
            'ALTER DEFAULT PRIVILEGES FOR ROLE %I IN SCHEMA public GRANT ALL PRIVILEGES ON TABLES TO %I',
            ten_admin, ten_vt);
        EXECUTE format(
            'ALTER DEFAULT PRIVILEGES FOR ROLE %I IN SCHEMA public GRANT ALL PRIVILEGES ON SEQUENCES TO %I',
            ten_admin, ten_vt);
    END LOOP;
END
$$;
