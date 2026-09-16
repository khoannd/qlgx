import { useEffect, useState } from 'react'
import { api } from '../api/client'
import type { TinhTrangSaoLuu } from '../api/types'
import { nhanLoiSaoLuu, tinhDenHieuLuc } from '../lib/denSaoLuu'

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

  if (!tinhTrang) return null
  // I2: đèn VÀNG cũng phải có băng cảnh báo. `qlgx-runner.timer` bị tắt, host hết đĩa, hay mạng
  // R2 chết đều dẫn tới vàng — mà băng cũ chỉ hiện với đỏ, nên cách hỏng phổ biến nhất lại là
  // cách duy nhất không báo cho ai. Xem thêm `tinhDenHieuLuc`: quá 24 giờ không có bản sao mới
  // thì vàng tự leo thang thành đỏ.
  const den = tinhDenHieuLuc(tinhTrang)
  if (den === 'xanh') return null

  const laDo = den === 'do'
  const cauLoi = nhanLoiSaoLuu(tinhTrang.loiGanNhat)
  const cau = laDo
    ? (cauLoi ?? 'Chưa có bản sao lưu nào thành công — nếu mất máy chủ lúc này thì không phục '
              + 'hồi được.')
    : (cauLoi ?? 'Đã quá lâu chưa có bản sao lưu mới hoặc chưa diễn tập phục hồi lại.')

  return (
    <div role="alert"
      style={{ background: laDo ? 'var(--rose-bg, #fde8ec)' : 'var(--amber-bg, #fdf1d8)',
               color: laDo ? 'var(--rose-ink)' : 'var(--amber-ink, #8a5a00)',
               padding: '8px 14px', fontSize: 12.5, display: 'flex',
               gap: 10, alignItems: 'center' }}>
      <strong>
        <span aria-hidden="true">{laDo ? '🔴' : '🟡'}</span>{' '}
        {laDo ? 'Sao lưu đang có vấn đề.' : 'Sao lưu đang chậm trễ.'}
      </strong>
      <span>{cau}</span>
      <button type="button" className="btn" onClick={onMoManHinh}>Xem chi tiết</button>
    </div>
  )
}
