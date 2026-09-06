import { execFileSync } from 'node:child_process'
import path from 'node:path'
import { fileURLToPath } from 'node:url'
import { Client } from 'pg'
import { chuoiKetNoiGoc, coQuyenChuanBi, choRuntimeEnvSanSang, runtimeEnv, thanhPgConfig } from './runtimeEnv.ts'

const __dirname = path.dirname(fileURLToPath(import.meta.url))
const THU_MUC_API = path.resolve(__dirname, '../src/Qlgx.Api')
const THU_MUC_DATA = path.resolve(__dirname, '../src/Qlgx.Data')

/**
 * Dựng toàn bộ dữ liệu e2e cần trước khi chạy: một database RIÊNG (KHÔNG đụng `qlgx_thu`),
 * chạy migration EF Core thật (kể cả migration bật Row-Level Security), rồi tạo một tài khoản
 * quản trị đầu tiên bằng ĐÚNG lệnh vận hành thật `dotnet run -- tao-tai-khoan-quan-tri` (xem
 * TRIEN-KHAI.md) — không chèn thẳng vào CSDL bằng SQL tay, để việc này cũng gián tiếp kiểm
 * chứng lệnh vận hành đó còn chạy đúng.
 *
 * Được gọi bằng TOP-LEVEL AWAIT ngay trong `playwright.config.ts`, KHÔNG dùng cơ chế
 * `globalSetup` của Playwright — đã thử `globalSetup` trước và phát hiện Playwright khởi động
 * (spawn) các tiến trình `webServer` TRƯỚC khi chạy `globalSetup` (API kết nối database chưa
 * tồn tại, sập ngay khi khởi động). Gọi thẳng trong lúc NẠP file cấu hình đảm bảo database đã
 * sẵn sàng trước khi Playwright dù chỉ mới ĐỌC xong mảng `webServer` để spawn tiến trình.
 */
export default async function prepareDb() {
  // Playwright nạp lại file cấu hình này ở NHIỀU tiến trình cho cùng một lần chạy (CLI chính +
  // từng worker) — chỉ tiến trình giành được khoá mới thực sự tạo database/chạy migration/tạo
  // tài khoản; các tiến trình còn lại chờ rồi dùng lại đúng bộ giá trị đã ghi. Xem
  // runtimeEnv.ts để biết vì sao (đã gặp lỗi "database already exists" thật khi chưa có bước
  // này — nhiều tiến trình cùng chạy CREATE DATABASE với cùng tên).
  if (!coQuyenChuanBi()) {
    await choRuntimeEnvSanSang()
    console.log('[e2e] Mot tien trinh khac da chuan bi database e2e — dung lai ket qua do.')
    return
  }

  const env = runtimeEnv()
  const goc = chuoiKetNoiGoc()

  const clientGoc = new Client(thanhPgConfig(goc, 'postgres'))
  await clientGoc.connect()
  try {
    await clientGoc.query(`CREATE DATABASE "${env.tenDb}"`)
  } finally {
    await clientGoc.end()
  }

  const chuoiKetNoiDb = `${goc};Database=${env.tenDb}`

  // CANH BAO da tra gia dat: `dotnet ef` KHONG doc "ConnectionStrings__Qlgx" — no goi thang
  // QlgxDbContextFactory (design-time), factory nay CHI doc bien QLGX_TEST_PG truc tiep (xem
  // QlgxDbContextFactory.cs). Lan dau viet script nay da truyen nham ConnectionStrings__Qlgx,
  // khien `dotnet ef database update` boi qua no, roi te vao nhanh QLGX_TEST_PG (chuoi GOC,
  // KHONG co ";Database=") — Npgsql mac dinh noi vao database TRUNG TEN VOI USERNAME khi thieu
  // "Database=", tuc la thang vao database "postgres" dung chung cua ca may. Phai GHI DE dung
  // QLGX_TEST_PG (kem ";Database=<ten db e2e>") cho tien trinh con nay, KHONG duoc thieu.
  execFileSync('dotnet', ['ef', 'database', 'update', '--startup-project', '.'], {
    cwd: THU_MUC_DATA,
    env: { ...process.env, QLGX_TEST_PG: chuoiKetNoiDb },
    stdio: 'inherit',
  })

  // `tao-tai-khoan-quan-tri` (QLGX_ADMIN_GIAO_XU_TEN) tra một GiaoXu đã CÓ SẴN theo tên, không
  // tự tạo — đúng hành vi thật (giáo xứ được thêm thủ công vào CSDL trước, xem TRIEN-KHAI.md
  // mục "Thêm một giáo xứ mới"). Chèn một dòng GiaoXu tối thiểu ở đây để mô phỏng đúng bước đó.
  const clientDb = new Client(thanhPgConfig(chuoiKetNoiDb, env.tenDb))
  await clientDb.connect()
  try {
    await clientDb.query(
      'INSERT INTO giao_xu (id, ma_giao_xu_cu, ten_giao_xu, created_at) VALUES (gen_random_uuid(), 0, $1, now())',
      [env.tenGiaoXu],
    )
  } finally {
    await clientDb.end()
  }

  execFileSync(
    'dotnet',
    ['run', '--project', THU_MUC_API, '--no-launch-profile', '--', 'tao-tai-khoan-quan-tri'],
    {
      cwd: THU_MUC_API,
      env: {
        ...process.env,
        ConnectionStrings__Qlgx: chuoiKetNoiDb,
        QLGX_ADMIN_TEN_TAI_KHOAN: env.taiKhoanQuanTri,
        QLGX_ADMIN_MAT_KHAU: env.matKhauQuanTri,
        QLGX_ADMIN_HO_TEN: 'Quản trị E2E',
        QLGX_ADMIN_GIAO_XU_TEN: env.tenGiaoXu,
      },
      stdio: 'inherit',
    },
  )

  console.log(`[e2e] Da chuan bi database "${env.tenDb}" va tai khoan "${env.taiKhoanQuanTri}".`)
}
