import { defineConfig, devices } from '@playwright/test'
import path from 'node:path'
import { fileURLToPath } from 'node:url'
import { chuoiKetNoiGoc, runtimeEnv } from './runtimeEnv.ts'
import prepareDb from './prepareDb.ts'

const __dirname = path.dirname(fileURLToPath(import.meta.url))
const env = runtimeEnv()
// Đọc sớm để báo lỗi rõ ràng NGAY LÚC NẠP CẤU HÌNH nếu thiếu biến, thay vì để hai webServer
// khởi động rồi mới lỗi mập mờ.
const chuoiKetNoiGocGiaTri = chuoiKetNoiGoc()
const chuoiKetNoiDb = `${chuoiKetNoiGocGiaTri};Database=${env.tenDb}`

// Top-level await CỐ Ý thay cho tuỳ chọn `globalSetup` của Playwright — xem lời giải thích đầy
// đủ trong prepareDb.ts. Chạy ngay lúc nạp file cấu hình này, chắc chắn xong TRƯỚC khi
// Playwright đọc mảng `webServer` bên dưới để spawn tiến trình API/web.
await prepareDb()

export default defineConfig({
  testDir: './tests',
  fullyParallel: false, // cac test dung chung MOT database e2e — chay lan luot cho de doc loi
  workers: 1,
  retries: 0,
  reporter: [['list']],
  globalTeardown: './globalTeardown.ts',
  timeout: 30_000,
  use: {
    baseURL: `http://localhost:${env.congWeb}`,
    trace: 'retain-on-failure',
    screenshot: 'only-on-failure',
  },
  projects: [
    { name: 'chromium', use: { ...devices['Desktop Chrome'] } },
  ],
  webServer: [
    {
      command: 'dotnet run --no-launch-profile',
      cwd: path.resolve(__dirname, '../src/Qlgx.Api'),
      url: `http://localhost:${env.congApi}/api/suc-khoe`,
      timeout: 120_000,
      reuseExistingServer: false,
      stdout: 'pipe',
      stderr: 'pipe',
      env: {
        ASPNETCORE_URLS: `http://localhost:${env.congApi}`,
        ConnectionStrings__Qlgx: chuoiKetNoiDb,
        // Vo hai — migration da chay xong trong globalSetup (dotnet ef database update). Bat
        // lai o day de KIEM CHUNG luon duong dan "migration tu chay luc khoi dong" (voi khoa
        // advisory) that su khong loi khi chay tren mot CSDL da migrate day du — dung dung
        // duong san se dung trong Dockerfile/docker-compose.yml.
        Qlgx__ChayMigrationKhiKhoiDong: 'true',
        Qlgx__JwtKey: env.jwtKey,
      },
    },
    {
      command: `npm run dev -- --port ${env.congWeb} --strictPort`,
      cwd: path.resolve(__dirname, '../src/web'),
      url: `http://localhost:${env.congWeb}`,
      timeout: 60_000,
      reuseExistingServer: false,
      stdout: 'pipe',
      stderr: 'pipe',
      env: {
        VITE_API_PROXY_TARGET: `http://localhost:${env.congApi}`,
      },
    },
  ],
})
