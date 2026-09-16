import { useState, type FormEvent } from 'react'
import { api } from '../../api/client'

type Props = {
  onDong: () => void
}

/**
 * Màn hình tự đổi mật khẩu (VIEC-TIEP-THEO.md mục 1.3) — trước đây chỉ Quản trị viên đặt lại
 * được mật khẩu người khác (xem TaiKhoanService.CapNhat), người dùng không tự đổi được. Bắt
 * buộc nhập đúng mật khẩu HIỆN TẠI (xác thực lại chính mình) trước khi ghi mật khẩu mới — nếu
 * không, ai mượn được máy đang mở phiên đăng nhập sẵn sẽ chiếm được tài khoản vĩnh viễn.
 *
 * Mở từ menu "Hệ thống" trên AppShell (nút "Đổi mật khẩu" cạnh "Đăng xuất"). Không lồng trong
 * `<form>` nào khác — AppShell không có `<form>` bao ngoài.
 */
export function DoiMatKhauModal({ onDong }: Props) {
  const [matKhauHienTai, setMatKhauHienTai] = useState('')
  const [matKhauMoi, setMatKhauMoi] = useState('')
  const [xacNhan, setXacNhan] = useState('')
  const [loi, setLoi] = useState<string | null>(null)
  const [dangGui, setDangGui] = useState(false)
  const [thanhCong, setThanhCong] = useState(false)

  async function onSubmit(e: FormEvent) {
    e.preventDefault()
    setLoi(null)

    if (matKhauMoi !== xacNhan) {
      setLoi('Xác nhận mật khẩu mới không khớp')
      return
    }
    if (matKhauMoi.length < 8) {
      setLoi('Mật khẩu mới phải có ít nhất 8 ký tự')
      return
    }

    setDangGui(true)
    try {
      await api.auth.doiMatKhau(matKhauHienTai, matKhauMoi)
      setThanhCong(true)
    } catch (err) {
      setLoi(err instanceof Error ? err.message : String(err))
    } finally {
      setDangGui(false)
    }
  }

  return (
    <div className="hoidap-nen" role="presentation">
      <div className="hoidap-hop" role="dialog" aria-modal="true" aria-label="Đổi mật khẩu">
        {thanhCong ? (
          <>
            <p className="hoidap-noidung">Đổi mật khẩu thành công. Lần đăng nhập sau hãy dùng mật khẩu mới.</p>
            <div className="hoidap-nut">
              <button type="button" className="btn btn-primary" onClick={onDong} autoFocus>Đóng</button>
            </div>
          </>
        ) : (
          <form onSubmit={onSubmit} style={{ display: 'flex', flexDirection: 'column', gap: 12 }}>
            <h2 style={{ margin: 0, fontSize: 15, color: 'var(--ink)' }}>Đổi mật khẩu</h2>

            <label className="field" style={{ flexDirection: 'column', alignItems: 'stretch' }}>
              Mật khẩu hiện tại
              <input
                type="password"
                value={matKhauHienTai}
                onChange={(e) => setMatKhauHienTai(e.target.value)}
                autoFocus
                required
                disabled={dangGui}
                autoComplete="current-password"
              />
            </label>

            <label className="field" style={{ flexDirection: 'column', alignItems: 'stretch' }}>
              Mật khẩu mới
              <input
                type="password"
                value={matKhauMoi}
                onChange={(e) => setMatKhauMoi(e.target.value)}
                required
                disabled={dangGui}
                autoComplete="new-password"
              />
            </label>

            <label className="field" style={{ flexDirection: 'column', alignItems: 'stretch' }}>
              Xác nhận mật khẩu mới
              <input
                type="password"
                value={xacNhan}
                onChange={(e) => setXacNhan(e.target.value)}
                required
                disabled={dangGui}
                autoComplete="new-password"
              />
            </label>

            {loi && (
              <div style={{ color: 'var(--rose-ink)', fontSize: 12.5 }} role="alert">{loi}</div>
            )}

            <div className="hoidap-nut">
              <button type="submit" className="btn btn-primary" disabled={dangGui}>
                {dangGui ? 'Đang lưu…' : 'Đổi mật khẩu'}
              </button>
              <button type="button" className="btn btn-quiet" onClick={onDong} disabled={dangGui}>
                Huỷ
              </button>
            </div>
          </form>
        )}
      </div>
    </div>
  )
}
