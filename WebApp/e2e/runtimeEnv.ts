import fs from 'node:fs'
import path from 'node:path'
import crypto from 'node:crypto'
import { fileURLToPath } from 'node:url'

const __dirname = path.dirname(fileURLToPath(import.meta.url))

// Ghi ra tệp thay vì chỉ giữ trong biến module — playwright.config.ts (tiến trình chính) và
// từng worker chạy test (tiến trình con RIÊNG) đều import module này; nếu chỉ sinh ngẫu nhiên
// trong bộ nhớ, mỗi tiến trình sẽ ra một bộ giá trị KHÁC nhau (tên database, cổng… lệch nhau).
// Đọc lại tệp nếu đã có đảm bảo toàn bộ tiến trình dùng chung đúng MỘT bộ giá trị cho một lần
// chạy `npx playwright test`. globalTeardown xoá tệp này ở cuối để lần chạy sau sinh lại mới.
const TEP_RUNTIME = path.join(__dirname, '.runtime.json')
const TEP_KHOA = path.join(__dirname, '.runtime.lock')

export type RuntimeEnv = {
  tenDb: string
  congApi: number
  congWeb: number
  jwtKey: string
  taiKhoanQuanTri: string
  matKhauQuanTri: string
  tenGiaoXu: string
}

export function runtimeEnv(): RuntimeEnv {
  if (fs.existsSync(TEP_RUNTIME)) {
    return JSON.parse(fs.readFileSync(TEP_RUNTIME, 'utf8')) as RuntimeEnv
  }

  const hau = crypto.randomBytes(5).toString('hex')
  const env: RuntimeEnv = {
    tenDb: `qlgx_e2e_${hau}`,
    // Cổng cố định lệch hẳn khỏi cổng dev mặc định (5096/5173) và khoảng port test khác trong
    // repo — tránh đụng độ khi chạy song song với `npm run dev` thủ công của lập trình viên.
    congApi: 58100 + (crypto.randomInt(0, 300)),
    congWeb: 58500 + (crypto.randomInt(0, 300)),
    jwtKey: crypto.randomBytes(32).toString('base64'),
    taiKhoanQuanTri: `e2e_qt_${hau}`,
    matKhauQuanTri: crypto.randomBytes(9).toString('base64url'),
    tenGiaoXu: `E2E Giao xu ${hau}`,
  }
  fs.writeFileSync(TEP_RUNTIME, JSON.stringify(env, null, 2))
  return env
}

export function xoaRuntimeEnv() {
  if (fs.existsSync(TEP_RUNTIME)) fs.unlinkSync(TEP_RUNTIME)
  if (fs.existsSync(TEP_KHOA)) fs.unlinkSync(TEP_KHOA)
}

/**
 * Playwright nạp lại `playwright.config.ts` (nên cả top-level await gọi `prepareDb()`) ở
 * NHIỀU tiến trình khác nhau cho cùng MỘT lần chạy `npx playwright test` — không chỉ tiến
 * trình CLI chính mà MỖI worker (kể cả khi `workers: 1`, tiến trình worker vẫn nạp lại config
 * để biết danh sách test) cũng nạp lại. Nếu không canh, mỗi tiến trình sẽ tự chạy
 * `CREATE DATABASE`/migration/tạo tài khoản MỘT LẦN NỮA — trùng tên database, lỗi ngay lập
 * tức (đã gặp thật khi viết bộ e2e này). Dùng tạo tệp KHOÁ nguyên tử (`wx` — thất bại nếu đã
 * tồn tại) làm "ai đến trước làm thật, ai đến sau chỉ chờ và dùng lại kết quả".
 */
export function coQuyenChuanBi(): boolean {
  try {
    fs.writeFileSync(TEP_KHOA, String(process.pid), { flag: 'wx' })
    return true
  } catch {
    return false
  }
}

/** Chờ tiến trình đang giữ khoá ghi xong `.runtime.json` rồi đọc lại — dùng ở các tiến trình
 * KHÔNG giành được khoá (xem coQuyenChuanBi). */
export async function choRuntimeEnvSanSang(): Promise<RuntimeEnv> {
  const gioiHan = Date.now() + 120_000
  while (!fs.existsSync(TEP_RUNTIME)) {
    if (Date.now() > gioiHan) {
      throw new Error('Cho .runtime.json qua 120s ma khong thay — tien trinh chuan bi co the da loi.')
    }
    await new Promise((r) => setTimeout(r, 200))
  }
  return runtimeEnv()
}

/** Chuỗi kết nối gốc (không có Database=) lấy từ QLGX_TEST_PG — dùng chung định dạng với bộ
 * test .NET (WebApp/tests) để một biến môi trường duy nhất phục vụ cả hai bộ test. */
export function chuoiKetNoiGoc(): string {
  const goc = process.env.QLGX_TEST_PG
  if (!goc) {
    throw new Error(
      'Chưa đặt biến môi trường QLGX_TEST_PG. Bộ e2e cần một PostgreSQL thật, ví dụ:\n' +
        '  export QLGX_TEST_PG="Host=localhost;Username=postgres;Password=<mật khẩu>"',
    )
  }
  return goc
}

/** Đọc chuỗi kết nối kiểu Npgsql ("Host=..;Username=..;Password=..") thành đối tượng cho `pg`. */
export function thanhPgConfig(chuoiKetNoi: string, tenDb?: string) {
  const phan: Record<string, string> = {}
  for (const doan of chuoiKetNoi.split(';')) {
    const i = doan.indexOf('=')
    if (i < 0) continue
    phan[doan.slice(0, i).trim().toLowerCase()] = doan.slice(i + 1).trim()
  }
  return {
    host: phan.host ?? 'localhost',
    port: phan.port ? Number(phan.port) : 5432,
    user: phan.username ?? phan['user id'] ?? 'postgres',
    password: phan.password ?? '',
    database: tenDb ?? phan.database ?? 'postgres',
  }
}
