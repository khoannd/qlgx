-- Siet quyen tren BA BANG SAO LUU. Chay bang SIEU NGUOI DUNG cua cum (install.sh), KHONG chay
-- qua migration EF Core.
--
-- VI SAO CAN TEP NAY (review backend, muc I2):
-- Ba bang cong_viec_sao_luu / ban_sao_luu / trang_thai_sao_luu do EF Core migration tao ra, va
-- migration chay bang ConnectionStrings__Qlgx -- tuc la bang vai tro qlgx_app. Trong Postgres,
-- nguoi tao bang la CHU bang, nen ca ba bang mac nhien thuoc qlgx_app va vai tro do ghi duoc.
--
-- Do la mot lo hong LEO THANG DAC QUYEN: bang cong_viec_sao_luu la mot HANG DOI MA LENH, va thu
-- thi hanh no la qlgx-runner.sh chay duoi quyen ROOT tren host. Neu vai tro ung dung (vai tro it
-- tin cay nhat, la thu mot ke tan cong qua web se chiem duoc truoc tien) ghi duoc vao hang doi do,
-- thi ke do dieu khien duoc mot tien trinh root. Thiet ke muc 7.4 noi ro: ba bang nay "chi truy
-- cap duoc qua vai tro qlgx_admin".
--
-- VI SAO KHONG DUA VAO MIGRATION: migration chay bang qlgx_app, va mot buoc ALTER TABLE ... OWNER
-- TO that bai se lam container API cua mot giao xu DANG CHAY khong khoi dong len duoc. Doi chu so
-- huu phai do sieu nguoi dung lam, ngoai duong migration.
--
-- HE QUA CAN NHO: sau tep nay, qlgx_app KHONG con quyen tren ba bang do. Mot migration EF ve sau
-- dung toi chung se that bai luc khoi dong API. Do la danh doi CO CHU DICH va co luoi an toan:
-- cho_san_sang() cua install.sh se khong thay API len, va duong quay lui tu dong se dua giao xu
-- ve ban cu. Neu can doi luoc do ba bang nay, hay sua CHINH TEP NAY va de install.sh chay no.
--
-- IDEMPOTENT: chay lai bao nhieu lan cung duoc. Va phai chiu duoc truong hop BA BANG CHUA TON TAI
-- (lan cai moi dau tien, install.sh chay SQL nay truoc khi migration kip tao bang) -- nen moi thao
-- tac deu nam trong mot vong lap co kiem to_regclass, khong phai ALTER tran.

\set ON_ERROR_STOP on

DO $$
DECLARE
    ten_app   text := current_setting('qlgx.app_user',   true);
    ten_admin text := current_setting('qlgx.admin_user', true);
    ten_bang  text;
    reg       regclass;
    seq       text;
BEGIN
    -- psql truyen tham so qua -v; DO block khong doc duoc bien psql, nen install.sh dat chung
    -- qua set_config truoc khi chay tep nay (cung cach 00-vai-tro-rls.sql da dung).
    IF ten_app IS NULL OR ten_app = '' OR ten_admin IS NULL OR ten_admin = '' THEN
        RAISE EXCEPTION 'Thieu ten vai tro: can qlgx.app_user va qlgx.admin_user.';
    END IF;
    IF ten_app = ten_admin THEN
        RAISE EXCEPTION 'Hai vai tro CSDL trung ten (%) -- RLS se bi vo hieu hoan toan.', ten_app;
    END IF;

    FOREACH ten_bang IN ARRAY ARRAY['cong_viec_sao_luu', 'ban_sao_luu', 'trang_thai_sao_luu'] LOOP
        reg := to_regclass('public.' || quote_ident(ten_bang));
        IF reg IS NULL THEN
            -- Lan cai moi dau tien: migration chua chay nen bang chua co. KHONG phai loi -- bo
            -- qua va de lan goi sau (install.sh goi lai ham nay SAU khi API da len) lam not.
            RAISE NOTICE 'Bang % chua ton tai -- bo qua (se siet quyen o lan goi sau).', ten_bang;
            CONTINUE;
        END IF;

        -- Chu so huu la vai tro QUAN TRI. Chu bang co quyen ngam va khong the bi thu hoi hoan
        -- toan, nen doi chu so huu la buoc DUY NHAT thuc su kin -- REVOKE mot minh la khong du:
        -- mot qlgx_app dang lam CHU co the tu GRANT lai quyen cho chinh no.
        EXECUTE format('ALTER TABLE public.%I OWNER TO %I', ten_bang, ten_admin);

        -- Thu hoi sach quyen cua vai tro nghiep vu va cua PUBLIC.
        EXECUTE format('REVOKE ALL PRIVILEGES ON public.%I FROM %I', ten_bang, ten_app);
        EXECUTE format('REVOKE ALL PRIVILEGES ON public.%I FROM PUBLIC', ten_bang);
        EXECUTE format('GRANT ALL PRIVILEGES ON public.%I TO %I', ten_bang, ten_admin);

        -- Sequence di kem (cot id kieu identity/serial) cung phai doi chu, neu khong thi INSERT
        -- cua qlgx_admin se bao "permission denied for sequence".
        FOR seq IN
            SELECT quote_ident(n.nspname) || '.' || quote_ident(c.relname)
            FROM pg_class c
            JOIN pg_namespace n ON n.oid = c.relnamespace
            JOIN pg_depend d ON d.objid = c.oid AND d.classid = 'pg_class'::regclass
            WHERE c.relkind = 'S' AND d.refobjid = reg
        LOOP
            EXECUTE format('ALTER SEQUENCE %s OWNER TO %I', seq, ten_admin);
            EXECUTE format('REVOKE ALL PRIVILEGES ON SEQUENCE %s FROM %I', seq, ten_app);
            EXECUTE format('GRANT ALL PRIVILEGES ON SEQUENCE %s TO %I', seq, ten_admin);
        END LOOP;

        RAISE NOTICE 'Da siet quyen bang %: chu so huu = %, thu hoi quyen cua %.',
                     ten_bang, ten_admin, ten_app;
    END LOOP;
END
$$;
