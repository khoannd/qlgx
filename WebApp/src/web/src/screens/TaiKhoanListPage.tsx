import { useEffect, useState, type FormEvent } from 'react'
import { api } from '../api/client'
import type { TaiKhoanItem } from '../api/types'
import { TrangThaiTai } from '../components/TrangThaiTai'

const TEN_LOAI: { gia: number; nhan: string }[] = [
  { gia: 0, nhan: 'Quản trị viên' },
  { gia: 1, nhan: 'Người nhập 1' },
  { gia: 2, nhan: 'Người nhập 2' },
]

type FormState = {
  id: string | null
  tenTaiKhoan: string
  hoTenNguoiDung: string
  email: string
  soDienThoai: string
  loaiTaiKhoan: number
  matKhau: string
  rowVersion: number
}

const FORM_TRONG: FormState = {
  id: null, tenTaiKhoan: '', hoTenNguoiDung: '', email: '', soDienThoai: '', loaiTaiKhoan: 1,
  matKhau: '', rowVersion: 0,
}

/**
 * Màn hình Quản lý tài khoản — thay `frmAccoutList.cs` (bản desktop). Chỉ Quản trị viên vào
 * được (backend chặn bằng policy "QuanTri"; SideNav cũng chỉ hiện mục này cho Quản trị viên).
 * Xem docs/superpowers/specs/man-hinh/quan-ly-tai-khoan.md.
 *
 * Khác bản desktop: KHÔNG có khối "Câu hỏi/câu trả lời bí mật" (cố ý bỏ — cơ chế yếu, xem
 * spec mục 8); dùng bảng HTML đơn giản thay vì AG Grid vì danh sách tài khoản của một giáo xứ
 * chỉ vài dòng, không cần lọc/sắp xếp/nhóm phức tạp như các lưới nghiệp vụ chính.
 */
export function TaiKhoanListPage() {
  const [rows, setRows] = useState<TaiKhoanItem[] | null>(null)
  const [loi, setLoi] = useState<string | null>(null)
  const [dangTai, setDangTai] = useState(true)
  const [form, setForm] = useState<FormState | null>(null)
  const [dangLuu, setDangLuu] = useState(false)
  const [thongBao, setThongBao] = useState<string | null>(null)

  function tai() {
    setDangTai(true)
    setLoi(null)
    api.taiKhoan.danhSach()
      .then(setRows)
      .catch((e: unknown) => setLoi(e instanceof Error ? e.message : String(e)))
      .finally(() => setDangTai(false))
  }

  useEffect(tai, [])

  function moThem() {
    setThongBao(null)
    setForm({ ...FORM_TRONG })
  }

  function moSua(tk: TaiKhoanItem) {
    setThongBao(null)
    setForm({
      id: tk.id, tenTaiKhoan: tk.tenTaiKhoan, hoTenNguoiDung: tk.hoTenNguoiDung ?? '',
      email: tk.email ?? '', soDienThoai: tk.soDienThoai ?? '', loaiTaiKhoan: tk.loaiTaiKhoan,
      matKhau: '', rowVersion: tk.rowVersion,
    })
  }

  async function onSubmit(e: FormEvent) {
    e.preventDefault()
    if (!form) return
    setDangLuu(true)
    setThongBao(null)
    try {
      if (form.id === null) {
        await api.taiKhoan.tao({
          tenTaiKhoan: form.tenTaiKhoan.trim(),
          matKhau: form.matKhau,
          hoTenNguoiDung: form.hoTenNguoiDung.trim(),
          email: form.email.trim() || null,
          soDienThoai: form.soDienThoai.trim() || null,
          loaiTaiKhoan: form.loaiTaiKhoan,
        })
      } else {
        await api.taiKhoan.capNhat(form.id, {
          hoTenNguoiDung: form.hoTenNguoiDung.trim(),
          email: form.email.trim() || null,
          soDienThoai: form.soDienThoai.trim() || null,
          loaiTaiKhoan: form.loaiTaiKhoan,
          matKhauMoi: form.matKhau || null,
          rowVersion: form.rowVersion,
        })
      }
      setForm(null)
      tai()
    } catch (err) {
      setThongBao(err instanceof Error ? err.message : String(err))
    } finally {
      setDangLuu(false)
    }
  }

  async function onXoa(tk: TaiKhoanItem) {
    if (!confirm(`Xoá tài khoản "${tk.tenTaiKhoan}"?`)) return
    try {
      await api.taiKhoan.xoa(tk.id)
      tai()
    } catch (err) {
      setLoi(err instanceof Error ? err.message : String(err))
    }
  }

  return (
    <TrangThaiTai dangTai={dangTai} loi={loi} onThuLai={tai}>
      <div style={{ padding: 16, display: 'flex', flexDirection: 'column', gap: 14 }}>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
          <h2 style={{ margin: 0 }}>Quản lý tài khoản</h2>
          <button type="button" className="btn" onClick={moThem}>+ Thêm tài khoản</button>
        </div>

        <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: 12 }}>
          <thead>
            <tr style={{ textAlign: 'left', borderBottom: '1px solid rgba(14,32,76,.13)' }}>
              <th>Tên đăng nhập</th>
              <th>Họ tên</th>
              <th>Email</th>
              <th>Điện thoại</th>
              <th>Loại tài khoản</th>
              <th />
            </tr>
          </thead>
          <tbody>
            {(rows ?? []).map((tk) => (
              <tr key={tk.id} style={{ borderBottom: '1px solid rgba(14,32,76,.06)' }}>
                <td>{tk.tenTaiKhoan}</td>
                <td>{tk.hoTenNguoiDung}</td>
                <td>{tk.email}</td>
                <td>{tk.soDienThoai}</td>
                <td>{tk.tenLoai ?? TEN_LOAI.find((l) => l.gia === tk.loaiTaiKhoan)?.nhan}</td>
                <td>
                  <button type="button" onClick={() => moSua(tk)}>Sửa</button>{' '}
                  <button type="button" onClick={() => onXoa(tk)}>Xoá</button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>

        {form && (
          <form
            onSubmit={onSubmit}
            className="glass"
            style={{ padding: 16, borderRadius: 'var(--r-card)', display: 'flex', flexDirection: 'column', gap: 10, maxWidth: 420 }}
          >
            <h3 style={{ margin: 0 }}>{form.id === null ? 'Thêm tài khoản' : `Sửa tài khoản: ${form.tenTaiKhoan}`}</h3>

            <label className="field" style={{ flexDirection: 'column', alignItems: 'stretch' }}>
              Họ tên người dùng
              <input type="text" required value={form.hoTenNguoiDung}
                onChange={(e) => setForm({ ...form, hoTenNguoiDung: e.target.value })} />
            </label>

            <label className="field" style={{ flexDirection: 'column', alignItems: 'stretch' }}>
              Tên đăng nhập
              <input type="text" required disabled={form.id !== null} pattern="^[a-zA-Z0-9]+$"
                title="Chỉ gồm chữ và số" value={form.tenTaiKhoan}
                onChange={(e) => setForm({ ...form, tenTaiKhoan: e.target.value })} />
            </label>

            <label className="field" style={{ flexDirection: 'column', alignItems: 'stretch' }}>
              {form.id === null ? 'Mật khẩu' : 'Mật khẩu mới (để trống nếu không đổi)'}
              <input type="password" required={form.id === null} minLength={8} value={form.matKhau}
                onChange={(e) => setForm({ ...form, matKhau: e.target.value })} />
            </label>

            <label className="field" style={{ flexDirection: 'column', alignItems: 'stretch' }}>
              Email
              <input type="text" value={form.email} onChange={(e) => setForm({ ...form, email: e.target.value })} />
            </label>

            <label className="field" style={{ flexDirection: 'column', alignItems: 'stretch' }}>
              Số điện thoại
              <input type="text" value={form.soDienThoai}
                onChange={(e) => setForm({ ...form, soDienThoai: e.target.value })} />
            </label>

            <label className="field" style={{ flexDirection: 'column', alignItems: 'stretch' }}>
              Loại tài khoản
              <select value={form.loaiTaiKhoan}
                onChange={(e) => setForm({ ...form, loaiTaiKhoan: Number(e.target.value) })}>
                {TEN_LOAI.map((l) => <option key={l.gia} value={l.gia}>{l.nhan}</option>)}
              </select>
            </label>

            {thongBao && <div style={{ color: 'var(--rose-ink)', fontSize: 12.5 }}>{thongBao}</div>}

            <div style={{ display: 'flex', gap: 8 }}>
              <button type="submit" className="btn" disabled={dangLuu}>{dangLuu ? 'Đang lưu…' : 'Lưu'}</button>
              <button type="button" onClick={() => setForm(null)} disabled={dangLuu}>Thôi</button>
            </div>
          </form>
        )}
      </div>
    </TrangThaiTai>
  )
}
