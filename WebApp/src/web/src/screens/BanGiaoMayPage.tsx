import { useCallback, useEffect, useState } from 'react'
import { demHangCho } from '../kho/hangCho'
import { api } from '../api/client'
import { layTrangThaiOffline } from '../dongbo/khoiDongOffline'

/**
 * Trang "Bàn giao máy này" — spec 9 (thanh trạng thái → bấm vào → "Bàn giao máy này"). Liệt kê số
 * việc chưa gửi (`demHangCho(kho)`, Task 2) và số mục cần xem lại (`GET /api/can-xem-lai`, Task 6/9)
 * — CHỈ khi CẢ HAI bằng 0 mới hiện nút xanh "Đã gửi về hết. Có thể gỡ máy này ra an toàn." — đúng
 * quy tắc cứng của brief: không được cho người dùng yên tâm gỡ máy khi còn dữ liệu chưa lên máy chủ.
 */
export function BanGiaoMayPage() {
  const [soHangCho, setSoHangCho] = useState<number | null>(null)
  const [soCanXemLai, setSoCanXemLai] = useState<number | null>(null)
  const [loi, setLoi] = useState<string | null>(null)

  const taiLai = useCallback(async () => {
    setLoi(null)
    const tt = layTrangThaiOffline()
    if (tt) {
      try {
        setSoHangCho(await demHangCho(tt.kho))
      } catch (e) {
        setLoi(e instanceof Error ? e.message : 'Không đếm được số việc chưa gửi.')
      }
    } else {
      // Tầng offline chưa sẵn sàng — KHÔNG được suy ra 0 (xem quy tắc cứng ở ThanhTrangThai.tsx,
      // cùng lý do áp dụng ở đây: một trang cho phép "gỡ máy an toàn" càng không được đoán mò).
      setSoHangCho(null)
    }
    try {
      const ds = await api.canXemLai.danhSach()
      setSoCanXemLai(ds.length)
    } catch (e) {
      setLoi(e instanceof Error ? e.message : 'Không tải được danh sách cần xem lại.')
    }
  }, [])

  useEffect(() => {
    taiLai()
  }, [taiLai])

  const dangTai = soHangCho === null || soCanXemLai === null
  const anToan = !dangTai && soHangCho === 0 && soCanXemLai === 0

  return (
    <div style={{ padding: 16 }}>
      <h2>Bàn giao máy này</h2>
      {loi && <p role="alert">{loi}</p>}
      {dangTai && !loi && <p>Đang kiểm tra…</p>}
      {!dangTai && (
        <>
          <p>Số việc chưa gửi lên máy chủ: {soHangCho}</p>
          <p>Số mục cần xem lại: {soCanXemLai}</p>
          {anToan ? (
            <button
              type="button"
              style={{ background: '#1a7f37', color: 'white', border: 'none', borderRadius: 6, padding: '8px 16px' }}
            >
              Đã gửi về hết. Có thể gỡ máy này ra an toàn.
            </button>
          ) : (
            <p>
              Máy này còn dữ liệu chưa gửi lên máy chủ hoặc còn việc cần xem lại — CHƯA nên gỡ máy
              này ra hoặc xoá dữ liệu, có thể mất những gì chưa gửi được.
            </p>
          )}
        </>
      )}
      <button type="button" onClick={taiLai}>Kiểm tra lại</button>
    </div>
  )
}
