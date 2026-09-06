import { useEffect, useRef, useState } from 'react'
import { api } from '../api/client'
import type { GiaoDanTimKiem } from '../api/types'

type Props = {
  /** Tên hiển thị hiện tại (đã chọn hoặc gõ tay từ dữ liệu cũ) — chỉ đọc, ô picker luôn
   * ReadOnly giống bản desktop (Designer.cs: `txtNguoiChong.ReadOnly = true`). */
  value?: string | null
  id?: string
  /** Gọi khi người dùng CHỌN một giáo dân thật từ danh sách tìm kiếm — mang cả bản ghi để nơi
   * gọi tự quyết định gán id thật (vd `chaId`) lẫn cập nhật tên hiển thị. */
  onChon?: (gd: GiaoDanTimKiem) => void
  onThemMoi?: () => void
  onBoChon?: () => void
}

/**
 * Ô chọn giáo dân — tương đương UserControl `GxGiaoDan` của bản desktop: ô chỉ đọc hiển thị
 * tên cùng ba nút tròn (chọn từ danh sách / thêm mới / bỏ chọn). Nút "Chọn" mở một hộp tìm
 * kiếm thật (gõ để tìm theo tên hoặc mã cũ, gọi `GET /api/giao-dan/tim`, kết quả giới hạn —
 * xem GiaoDanService.TimKiem) thay vì chỉ hiển thị tĩnh như trước (xem
 * docs/superpowers/specs/man-hinh/can-review-sau.md mục 19). Nút "Thêm mới" (mở `frmGiaoDan`
 * đầy đủ ở bản desktop) chưa nối — việc đó là màn hình chi tiết giáo dân đầy đủ, ngoài phạm vi
 * hạ tầng picker này; giữ callback rỗng để nơi gọi tự nối sau nếu cần.
 */
export function GxPicker({ value, id, onChon, onThemMoi, onBoChon }: Props) {
  const [dangMo, setDangMo] = useState(false)
  const [tuKhoa, setTuKhoa] = useState('')
  const [ketQua, setKetQua] = useState<GiaoDanTimKiem[]>([])
  const [dangTai, setDangTai] = useState(false)
  const hopRef = useRef<HTMLDivElement>(null)

  useEffect(() => {
    if (!dangMo) return
    const timer = setTimeout(() => {
      setDangTai(true)
      api.timKiem
        .giaoDan(tuKhoa, 20)
        .then(setKetQua)
        .catch(() => setKetQua([]))
        .finally(() => setDangTai(false))
    }, 250)
    return () => clearTimeout(timer)
  }, [dangMo, tuKhoa])

  useEffect(() => {
    if (!dangMo) return
    function ngoaiHop(e: MouseEvent) {
      if (hopRef.current && !hopRef.current.contains(e.target as Node)) setDangMo(false)
    }
    document.addEventListener('mousedown', ngoaiHop)
    return () => document.removeEventListener('mousedown', ngoaiHop)
  }, [dangMo])

  function chon(gd: GiaoDanTimKiem) {
    onChon?.(gd)
    setDangMo(false)
    setTuKhoa('')
  }

  return (
    <span className="picker" id={id} style={{ position: 'relative' }}>
      <span className={'who' + (value ? '' : ' empty')}>{value || '—'}</span>
      <button type="button" className="mini" title="Chọn từ danh sách giáo dân"
        onClick={() => setDangMo((m) => !m)}>
        &#9678;
      </button>
      <button type="button" className="mini" title="Thêm giáo dân mới" onClick={onThemMoi}>
        +
      </button>
      <button type="button" className="mini" title="Bỏ chọn" onClick={onBoChon}>
        &times;
      </button>

      {dangMo && (
        <div ref={hopRef} className="picker-dropdown" role="listbox"
          style={{
            position: 'absolute', top: '100%', left: 0, zIndex: 20, minWidth: 260,
            background: 'var(--bg, #fff)', border: '1px solid #ccc', borderRadius: 6,
            boxShadow: '0 4px 12px rgba(0,0,0,.15)', padding: 6,
          }}>
          <input
            type="text"
            autoFocus
            placeholder="Gõ tên hoặc mã cũ để tìm…"
            value={tuKhoa}
            onChange={(e) => setTuKhoa(e.target.value)}
            style={{ width: '100%', boxSizing: 'border-box', marginBottom: 4 }}
          />
          {dangTai && <div className="muted" style={{ fontSize: 12.5, padding: 4 }}>Đang tìm…</div>}
          {!dangTai && ketQua.length === 0 && (
            <div className="muted" style={{ fontSize: 12.5, padding: 4 }}>Không tìm thấy giáo dân nào</div>
          )}
          {!dangTai && ketQua.length > 0 && (
            <ul style={{ listStyle: 'none', margin: 0, padding: 0, maxHeight: 220, overflowY: 'auto' }}>
              {ketQua.map((gd) => (
                <li key={gd.id}>
                  <button type="button" onClick={() => chon(gd)}
                    style={{
                      width: '100%', textAlign: 'left', background: 'none', border: 'none',
                      padding: '4px 6px', cursor: 'pointer', borderRadius: 4,
                    }}>
                    {(gd.tenThanh ? gd.tenThanh + ' ' : '') + gd.hoTen}
                    <span className="muted" style={{ fontSize: 11.5, marginLeft: 6 }}>
                      #{gd.maGiaoDanCu} · {gd.phai ?? '?'} {gd.ngaySinh ? '· ' + gd.ngaySinh.slice(0, 4) : ''}
                    </span>
                  </button>
                </li>
              ))}
            </ul>
          )}
        </div>
      )}
    </span>
  )
}
