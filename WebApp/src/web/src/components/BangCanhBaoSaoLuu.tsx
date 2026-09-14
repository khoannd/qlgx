import { useEffect, useState } from 'react'
import { api } from '../api/client'
import type { TinhTrangSaoLuu } from '../api/types'

/**
 * Băng cảnh báo hiện trên đầu MỌI màn hình cho tài khoản Quản trị hệ thống khi sao lưu đang có
 * vấn đề.
 *
 * Vì sao không để yên trong màn hình "Sao lưu & Phục hồi": kiểu hỏng nguy hiểm nhất của sao lưu
 * là hỏng ÂM THẦM sáu tháng rồi mới lộ ra đúng hôm cần dùng. Không ai chủ động vào kiểm tra một
 * màn hình mà mọi thứ vẫn đang bình thường.
 *
 * Lỗi gọi API thì im lặng: một băng báo động đỏ chỉ vì mạng chập chờn sẽ nhanh chóng bị bỏ qua,
 * và một cảnh báo bị bỏ qua thì tệ hơn không có cảnh báo.
 */
export function BangCanhBaoSaoLuu(
  { laQuanTriHeThong, onMoManHinh }: { laQuanTriHeThong: boolean; onMoManHinh: () => void },
) {
  const [tinhTrang, setTinhTrang] = useState<TinhTrangSaoLuu | null>(null)

  useEffect(() => {
    if (!laQuanTriHeThong) return
    let huy = false
    const tai = () => api.saoLuu.tinhTrang()
      .then((tt) => { if (!huy) setTinhTrang(tt) })
      .catch(() => { if (!huy) setTinhTrang(null) })
    void tai()
    // 5 phút một lần: đủ sớm để không bỏ lỡ cả ngày, đủ thưa để không thêm tải vô ích.
    const dinhKy = setInterval(() => void tai(), 5 * 60 * 1000)
    return () => { huy = true; clearInterval(dinhKy) }
  }, [laQuanTriHeThong])

  if (!tinhTrang || tinhTrang.den !== 'do') return null

  return (
    <div role="alert" style={{ background: 'var(--rose-bg, #fde8ec)', color: 'var(--rose-ink)',
                               padding: '8px 14px', fontSize: 12.5, display: 'flex',
                               gap: 10, alignItems: 'center' }}>
      <strong>⚠ Sao lưu đang có vấn đề.</strong>
      <span>{tinhTrang.loiGanNhat ?? 'Chưa có bản sao lưu nào thành công.'}</span>
      <button type="button" className="btn" onClick={onMoManHinh}>Xem chi tiết</button>
    </div>
  )
}
