/// <reference types="vitest/config" />
import react from '@vitejs/plugin-react'
import { defineConfig, loadEnv } from 'vite'

// https://vite.dev/config/
export default defineConfig(({ mode }) => {
  // Địa chỉ Qlgx.Api khi chạy `npm run dev` — đọc từ biến môi trường Vite VITE_API_PROXY_TARGET
  // (đặt trong `.env.local`, không commit) để không viết cứng cổng máy chủ API; mặc định khớp
  // cổng launchSettings.json profile "http" của Qlgx.Api khi không đặt biến này.
  const env = loadEnv(mode, process.cwd(), '')
  const target = env.VITE_API_PROXY_TARGET || 'http://localhost:5096'

  return {
    plugins: [react()],
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
