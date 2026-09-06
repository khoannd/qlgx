import { useEffect, useState } from 'react'
import { useTrangThaiMang } from '../lib/trangThaiMang'

/**
 * Dải báo cố định trên cùng khi mất mạng — hiện toàn ứng dụng (kể cả màn hình đăng nhập), vì
 * giáo xứ vùng xa mạng chập chờn và người dùng cần biết ngay để yên tâm là dữ liệu đang gõ
 * KHÔNG bị mất (xem cơ chế bản nháp ở `lib/banNhap.ts`). Khi mạng trở lại, đổi sang thông báo
 * xanh ngắn rồi tự ẩn — không cần bấm tắt.
 */
export function TrangThaiMangBanner() {
  const online = useTrangThaiMang()
  const [hienLaiMang, setHienLaiMang] = useState(false)
  const [tungMatMang, setTungMatMang] = useState(false)

  useEffect(() => {
    if (!online) {
      setTungMatMang(true)
      setHienLaiMang(false)
      return
    }
    if (tungMatMang) {
      setHienLaiMang(true)
      const t = setTimeout(() => setHienLaiMang(false), 4000)
      return () => clearTimeout(t)
    }
  }, [online, tungMatMang])

  if (!online) {
    return (
      <div className="mang-banner mang-banner-off" role="status">
        ⚠ Mất kết nối mạng — dữ liệu bạn đang nhập vẫn được giữ trên máy này, sẽ không bị mất.
        Bấm Lưu vẫn được, hãy thử lại khi có mạng.
      </div>
    )
  }
  if (hienLaiMang) {
    return (
      <div className="mang-banner mang-banner-on" role="status">
        ✓ Đã có mạng trở lại.
      </div>
    )
  }
  return null
}
