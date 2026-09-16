import { useState, type FormEvent } from 'react'
import type { ThongTinBenNhanGioiThieu } from '../api/client'

type Props = {
  tieuDe: string
  onXuat: (benNhan: ThongTinBenNhanGioiThieu) => Promise<void>
  onDong: () => void
}

/**
 * Ô nhập "bên nhận" dùng CHUNG cho cả bốn mẫu Giấy giới thiệu (chuyển xứ/rửa tội/thêm sức/
 * giáo lý hôn phối) — tái hiện đúng màn hình `frmReport.cs` của bản desktop
 * (`txtGiaoPhan`/`txtGiaoXu` nhập tự do + `cbLinhMuc` chọn linh mục ký tên), xem ghi chú đầu
 * khối "Giấy giới thiệu" trong `Qlgx.Api/Services/InAnService.cs`.
 *
 * "Linh mục giới thiệu" ở đây là Ô NHẬP TỰ DO (không phải danh mục chọn như `cbLinhMuc` bản
 * desktop) — bản web hiện KHÔNG có màn hình quản lý danh mục Linh mục nào cả (bảng `LinhMuc`
 * tồn tại trong CSDL nhưng chưa có API/màn hình), và các trường "tên cha …" khác của GiaoDan
 * (ChaRuaToi, ChaThemSuc…) trong toàn hệ thống cũng đều là chuỗi tự do — dùng ô nhập tự do ở
 * đây nhất quán với quy ước đó, xem can-review-sau.md.
 */
export function GioiThieuModal({ tieuDe, onXuat, onDong }: Props) {
  const [giaoPhan2, setGiaoPhan2] = useState('')
  const [giaoXu2, setGiaoXu2] = useState('')
  const [tenLinhMuc, setTenLinhMuc] = useState('')
  const [loi, setLoi] = useState<string | null>(null)
  const [dangIn, setDangIn] = useState(false)

  async function onSubmit(e: FormEvent) {
    e.preventDefault()
    setLoi(null)
    if (!giaoXu2.trim()) {
      setLoi('Hãy nhập giáo xứ nhận giấy giới thiệu.')
      return
    }
    setDangIn(true)
    try {
      await onXuat({ giaoPhan2: giaoPhan2.trim(), giaoXu2: giaoXu2.trim(), tenLinhMuc: tenLinhMuc.trim() })
      onDong()
    } catch (err) {
      setLoi(err instanceof Error ? err.message : 'In thất bại, thử lại sau.')
    } finally {
      setDangIn(false)
    }
  }

  return (
    <div className="hoidap-nen" role="presentation">
      <div className="hoidap-hop" role="dialog" aria-modal="true" aria-label={tieuDe}>
        <form onSubmit={onSubmit} style={{ display: 'flex', flexDirection: 'column', gap: 12 }}>
          <h2 style={{ margin: 0, fontSize: 15, color: 'var(--ink)' }}>{tieuDe}</h2>
          <p className="hoidap-noidung" style={{ margin: 0, fontSize: 12.5 }}>
            Nhập thông tin giáo xứ/giáo phận NHẬN giấy giới thiệu — giáo xứ khác thường không có
            trong dữ liệu của giáo xứ này nên phải gõ tay ở đây, đúng cách bản desktop làm.
          </p>

          <label className="field" style={{ flexDirection: 'column', alignItems: 'stretch' }}>
            Giáo phận nhận
            <input
              type="text"
              value={giaoPhan2}
              onChange={(e) => setGiaoPhan2(e.target.value)}
              autoFocus
              disabled={dangIn}
            />
          </label>

          <label className="field" style={{ flexDirection: 'column', alignItems: 'stretch' }}>
            Giáo xứ nhận <span style={{ color: 'var(--rose-ink)' }}>*</span>
            <input
              type="text"
              value={giaoXu2}
              onChange={(e) => setGiaoXu2(e.target.value)}
              required
              disabled={dangIn}
            />
          </label>

          <label className="field" style={{ flexDirection: 'column', alignItems: 'stretch' }}>
            Linh mục giới thiệu (ký tên)
            <input
              type="text"
              value={tenLinhMuc}
              onChange={(e) => setTenLinhMuc(e.target.value)}
              disabled={dangIn}
            />
          </label>

          {loi && (
            <div style={{ color: 'var(--rose-ink)', fontSize: 12.5 }} role="alert">{loi}</div>
          )}

          <div className="hoidap-nut">
            <button type="submit" className="btn btn-primary" disabled={dangIn}>
              {dangIn ? 'Đang in…' : 'Xuất giấy giới thiệu'}
            </button>
            <button type="button" className="btn btn-quiet" onClick={onDong} disabled={dangIn}>
              Huỷ
            </button>
          </div>
        </form>
      </div>
    </div>
  )
}
