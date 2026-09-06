import { useRegisterSW } from 'virtual:pwa-register/react'

/**
 * Đăng ký service worker (PWA, Task 16) và hỏi người dùng trước khi tải lại trang lấy bản mới
 * — KHÔNG tự ý reload giữa chừng (có thể làm mất dữ liệu đang gõ ở form khác đang mở trong
 * cùng tab), đúng yêu cầu gốc "báo có bản mới, tải lại" thay vì âm thầm chuyển bản.
 *
 * `useRegisterSW` tới từ `vite-plugin-pwa/client` (ảo hoá lúc build, xem vite.config.ts —
 * `injectRegister: false` vì component này tự quản lý việc đăng ký thay vì để plugin chèn sẵn
 * script). Không kết xuất gì khi chưa có cập nhật — chỉ hiện dải nhỏ góc dưới bên phải lúc có
 * bản mới sẵn sàng, hoặc xác nhận ngắn khi lần đầu cài đặt xong (dùng được ngoại tuyến).
 */
export function CapNhatPWA() {
  const {
    needRefresh: [needRefresh, setNeedRefresh],
    offlineReady: [offlineReady, setOfflineReady],
    updateServiceWorker,
  } = useRegisterSW({
    onRegisterError(loi) {
      console.error('Không đăng ký được service worker (PWA)', loi)
    },
  })

  function dong() {
    setNeedRefresh(false)
    setOfflineReady(false)
  }

  if (needRefresh) {
    return (
      <div className="capnhat-pwa" role="status">
        <span>🔄 Có bản cập nhật mới của QLGX.</span>
        <button type="button" className="btn btn-sm btn-primary" onClick={() => updateServiceWorker(true)}>
          Tải lại
        </button>
        <button type="button" className="btn btn-sm btn-quiet" onClick={dong}>Để sau</button>
      </div>
    )
  }

  if (offlineReady) {
    return (
      <div className="capnhat-pwa" role="status">
        <span>✓ QLGX đã sẵn sàng dùng khi mất mạng.</span>
        <button type="button" className="btn btn-sm btn-quiet" onClick={dong}>Đóng</button>
      </div>
    )
  }

  return null
}
