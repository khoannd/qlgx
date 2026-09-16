import { useState, type FormEvent } from 'react'
import { useAuth } from '../api/AuthContext'
import { CanChonGiaoXu } from '../api/client'
import type { GiaoXuLuaChon } from '../api/types'

/**
 * Màn hình đăng nhập — thay `frmLogin.cs` (bản desktop). Không có màn hình nào khác dùng
 * được trước khi đăng nhập (App.tsx chỉ hiện màn hình này khi `useAuth().nguoiDung === null`).
 * Câu hỏi bí mật/"Quên mật khẩu" của bản desktop KHÔNG mang sang — xem
 * docs/superpowers/specs/man-hinh/quan-ly-tai-khoan.md mục 8.
 *
 * Chọn giáo xứ (review-cuoi.md mục M1): chỉ HIỆN RA khi máy chủ báo tên đăng nhập vừa gõ
 * trùng ở nhiều giáo xứ (`CanChonGiaoXu`) — giáo xứ pilot hiện chỉ có một giáo xứ nên tuyệt
 * đại đa số người dùng KHÔNG BAO GIỜ thấy bước này, đúng yêu cầu "đừng bắt chọn giáo xứ mỗi
 * lần đăng nhập nếu hệ thống chỉ có một".
 */
export function LoginPage() {
  const { dangNhap } = useAuth()
  const [tenTaiKhoan, setTenTaiKhoan] = useState('')
  const [matKhau, setMatKhau] = useState('')
  const [loi, setLoi] = useState<string | null>(null)
  const [dangGui, setDangGui] = useState(false)
  const [dsGiaoXu, setDsGiaoXu] = useState<GiaoXuLuaChon[] | null>(null)
  const [giaoXuId, setGiaoXuId] = useState('')

  async function onSubmit(e: FormEvent) {
    e.preventDefault()
    setLoi(null)
    setDangGui(true)
    try {
      await dangNhap(tenTaiKhoan.trim(), matKhau, giaoXuId || undefined)
    } catch (err) {
      if (err instanceof CanChonGiaoXu) {
        setDsGiaoXu(err.danhSachGiaoXu)
        setLoi(err.message)
      } else {
        setLoi(err instanceof Error ? err.message : String(err))
      }
    } finally {
      setDangGui(false)
    }
  }

  return (
    <div
      style={{
        minHeight: '100vh',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        background: 'linear-gradient(160deg, var(--brand-mist), #fff)',
      }}
    >
      <form
        onSubmit={onSubmit}
        className="glass glass-solid"
        style={{ width: 340, padding: '28px 26px', borderRadius: 'var(--r-card)', display: 'flex', flexDirection: 'column', gap: 14 }}
      >
        <div style={{ display: 'flex', alignItems: 'center', gap: 10, marginBottom: 4 }}>
          <span className="mark" style={{
            width: 34, height: 34, borderRadius: 10, display: 'inline-flex', alignItems: 'center',
            justifyContent: 'center', background: 'linear-gradient(145deg, var(--brand), var(--brand-deep))', color: '#fff',
          }}>
            <svg viewBox="0 0 24 24" fill="currentColor" width={19} height={19} aria-hidden="true">
              <path d="M11 2h2v3h3v2h-3v3.2l6 3.4V22h-6.5v-4a2.5 2.5 0 0 0-5 0v4H1v-8.4l6-3.4V7H4V5h3V2h4z" />
            </svg>
          </span>
          <div>
            <div className="name" style={{ fontWeight: 700 }}>QLGX</div>
            <div className="sub" style={{ fontSize: 11.5, color: 'var(--ink-faint)' }}>Quản lý giáo xứ</div>
          </div>
        </div>

        <label className="field" style={{ flexDirection: 'column', alignItems: 'stretch' }}>
          Tên đăng nhập
          <input
            type="text"
            value={tenTaiKhoan}
            onChange={(e) => setTenTaiKhoan(e.target.value)}
            autoFocus
            required
            disabled={dangGui}
          />
        </label>

        <label className="field" style={{ flexDirection: 'column', alignItems: 'stretch' }}>
          Mật khẩu
          <input
            type="password"
            value={matKhau}
            onChange={(e) => setMatKhau(e.target.value)}
            required
            disabled={dangGui}
          />
        </label>

        {dsGiaoXu && (
          <label className="field" style={{ flexDirection: 'column', alignItems: 'stretch' }}>
            Giáo xứ
            <select
              value={giaoXuId}
              onChange={(e) => setGiaoXuId(e.target.value)}
              required
              disabled={dangGui}
              autoFocus
            >
              <option value="">— Chọn giáo xứ —</option>
              {dsGiaoXu.map((gx) => (
                <option key={gx.id} value={gx.id}>{gx.tenGiaoXu}</option>
              ))}
            </select>
          </label>
        )}

        {loi && (
          <div style={{ color: 'var(--rose-ink)', fontSize: 12.5 }} role="alert">{loi}</div>
        )}

        <button type="submit" className="btn" disabled={dangGui || (dsGiaoXu !== null && !giaoXuId)} style={{
          background: 'linear-gradient(145deg, var(--brand), var(--brand-deep))', color: '#fff',
          border: 'none', borderRadius: 'var(--r-field)', padding: '9px 0', fontWeight: 600, cursor: 'pointer',
        }}>
          {dangGui ? 'Đang đăng nhập…' : 'Đăng nhập'}
        </button>
      </form>
    </div>
  )
}
