/// <reference types="vitest/config" />
import react from '@vitejs/plugin-react'
import { defineConfig, loadEnv } from 'vite'
import { VitePWA } from 'vite-plugin-pwa'

// https://vite.dev/config/
export default defineConfig(({ mode }) => {
  // Địa chỉ Qlgx.Api khi chạy `npm run dev` — đọc từ biến môi trường Vite VITE_API_PROXY_TARGET
  // (đặt trong `.env.local`, không commit) để không viết cứng cổng máy chủ API; mặc định khớp
  // cổng launchSettings.json profile "http" của Qlgx.Api khi không đặt biến này.
  const env = loadEnv(mode, process.cwd(), '')
  const target = env.VITE_API_PROXY_TARGET || 'http://localhost:5096'

  return {
    plugins: [
      react(),
      // PWA (Task 16) — CHỈ cache vỏ ứng dụng (JS/CSS/HTML/icon đã build), KHÔNG cache bất kỳ
      // phản hồi /api/* nào: không khai báo `runtimeCaching` cho /api nghĩa là mọi gọi API vẫn
      // đi thẳng ra mạng như cũ, lỗi mạng vẫn ném lỗi thật (xem client.ts) thay vì âm thầm trả
      // dữ liệu cũ như dữ liệu thật — hồ sơ giáo dân hiện sai mà không cảnh báo còn nguy hiểm
      // hơn báo lỗi thẳng (yêu cầu gốc, xem task-16-report.md).
      VitePWA({
        registerType: 'prompt', // KHÔNG tự ý reload — CapNhatPWA.tsx hỏi người dùng trước.
        // Tự đăng ký service worker bằng hook `useRegisterSW` (virtual:pwa-register/react)
        // trong CapNhatPWA.tsx thay vì để plugin chèn sẵn script — cần vậy để hiện banner
        // "có bản mới" TRONG ứng dụng thay vì window.confirm mặc định của plugin.
        injectRegister: false,
        includeAssets: ['icons/icon-64.png'],
        manifest: {
          name: 'QLGX — Quản lý giáo xứ',
          short_name: 'QLGX',
          description: 'Phần mềm quản lý giáo xứ — bản web',
          start_url: '/',
          scope: '/',
          display: 'standalone',
          background_color: '#eff5ff',
          theme_color: '#1d5ddb',
          icons: [
            { src: '/icons/icon-192.png', sizes: '192x192', type: 'image/png' },
            { src: '/icons/icon-512.png', sizes: '512x512', type: 'image/png' },
            { src: '/icons/icon-512-maskable.png', sizes: '512x512', type: 'image/png', purpose: 'maskable' },
          ],
        },
        workbox: {
          // Mặc định của Workbox chỉ precache các tệp khớp glob này (build output) — KHÔNG
          // đụng tới /api/* vì đó là điều hướng/gọi mạng lúc chạy, không nằm trong build output.
          globPatterns: ['**/*.{js,css,html,svg,png,ico,woff2}'],
          navigateFallbackDenylist: [/^\/api\//],
        },
      }),
    ],
    server: {
      proxy: { '/api': { target, changeOrigin: true } },
    },
    test: {
      environment: 'jsdom',
      globals: true,
      setupFiles: ['./src/test-setup.ts'],
    },
  }
})
