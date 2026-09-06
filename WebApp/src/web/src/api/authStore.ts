/**
 * Nơi giữ token đăng nhập — tách khỏi React để `client.ts` (hàm `goi` thuần, không phải
 * hook) đọc/ghi được mà không phải truyền token qua từng lời gọi API. Token lưu ở
 * `localStorage` (không phải state trong tiến trình máy chủ — token tự chứa, xem
 * TokenService.cs phía backend) để còn sống sót qua lần tải lại trang.
 *
 * `dangLangNghe401` cho AuthProvider biết "token vừa bị máy chủ từ chối" (hết hạn hoặc bị thu
 * hồi) để tự động đăng xuất và hiện lại màn hình đăng nhập — KHÔNG xoá dữ liệu form đang gõ,
 * đó là việc của cơ chế bản nháp `localStorage` ở từng form chi tiết.
 */
const KHOA_TOKEN = 'qlgx.token'

let nghe401: (() => void) | null = null

export const authStore = {
  layToken(): string | null {
    try {
      return localStorage.getItem(KHOA_TOKEN)
    } catch {
      return null
    }
  },
  datToken(token: string) {
    try {
      localStorage.setItem(KHOA_TOKEN, token)
    } catch {
      // localStorage có thể bị chặn (chế độ riêng tư nghiêm ngặt) — phiên vẫn chạy được
      // trong bộ nhớ của tab hiện tại, chỉ mất khi tải lại trang.
    }
  },
  xoaToken() {
    try {
      localStorage.removeItem(KHOA_TOKEN)
    } catch { /* xem ghi chú ở datToken */ }
  },
  dangKy401(fn: () => void) {
    nghe401 = fn
  },
  huy401() {
    nghe401 = null
  },
  baoHet401() {
    nghe401?.()
  },
}
