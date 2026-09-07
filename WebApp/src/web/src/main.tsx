import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
// CSS của AG Grid PHẢI nạp TRƯỚC `qlgx.css`: Vite gộp CSS theo đúng thứ tự import gặp lần
// đầu khi duyệt cây module — nếu `GxGrid.tsx` là nơi đầu tiên import hai file này (như trước
// đây), chúng rơi vào ĐUÔI bundle CSS (sau `qlgx.css`, vì import ở component lazy-hơn), và mọi
// biến `--ag-*` khai trong `.ag-theme-quartz` ở qlgx.css (chiều cao dòng/tiêu đề, màu nền/chữ
// header — mục 2 can-review-sau.md, 2026-09-07) bị chính CSS gốc của AG Grid đè lại vì cùng
// độ đặc hiệu (cùng chọn `.ag-theme-quartz`), ai nạp SAU thắng. Phát hiện bằng đo
// `getBoundingClientRect()` thật trên trình duyệt: `--ag-row-height: 30px` khai rõ ràng nhưng
// dòng dữ liệu thật cao 42px (giá trị mặc định AG Grid) — bài test jsdom không dựng CSS nên
// không bao giờ bắt được lỗi này.
import 'ag-grid-community/styles/ag-grid.css'
import 'ag-grid-community/styles/ag-theme-quartz.css'
import './styles/qlgx.css'
import { AuthProvider } from './api/AuthContext.tsx'
import App from './App.tsx'

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <AuthProvider>
      <App />
    </AuthProvider>
  </StrictMode>,
)
