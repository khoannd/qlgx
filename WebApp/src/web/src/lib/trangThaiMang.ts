import { useEffect, useState } from 'react'

/** Trạng thái mạng của trình duyệt — dựa vào `navigator.onLine` + sự kiện `online`/`offline`.
 * Đây là tín hiệu CỦA TRÌNH DUYỆT (biết đã kết nối được mạng LAN/Wi-Fi hay chưa), không phải
 * "máy chủ Qlgx.Api có phản hồi hay không" — hai việc khác nhau nhưng với giáo xứ vùng xa,
 * mất mạng gần như luôn đồng nghĩa mất luôn máy chủ (máy chủ tập trung, không cài tại chỗ). */
export function useTrangThaiMang(): boolean {
  const [online, setOnline] = useState(() => (typeof navigator === 'undefined' ? true : navigator.onLine))

  useEffect(() => {
    const len = () => setOnline(true)
    const lof = () => setOnline(false)
    window.addEventListener('online', len)
    window.addEventListener('offline', lof)
    return () => {
      window.removeEventListener('online', len)
      window.removeEventListener('offline', lof)
    }
  }, [])

  return online
}
