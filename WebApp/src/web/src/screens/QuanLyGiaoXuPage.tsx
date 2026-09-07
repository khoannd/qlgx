import { useEffect, useState, type CSSProperties, type FormEvent } from 'react'
import { api } from '../api/client'
import type { GiaoPhan, GiaoHatQuanLy, GiaoXuQuanLy } from '../api/types'
import { TrangThaiTai } from '../components/TrangThaiTai'

type FormGiaoPhan = { id: string | null; tenGiaoPhan: string; ghiChu: string }
type FormGiaoHat = { id: string | null; giaoPhanId: string; tenGiaoHat: string; ghiChu: string }
type FormGiaoXu = {
  id: string | null; giaoHatId: string; tenGiaoXu: string; diaChi: string
  dienThoai: string; email: string; website: string; ghiChu: string
}
type FormTaiKhoan = {
  giaoXuId: string; tenGiaoXu: string
  tenTaiKhoan: string; matKhau: string; hoTenNguoiDung: string; email: string; soDienThoai: string
}

const oFieldCot: CSSProperties = { flexDirection: 'column', alignItems: 'stretch' }

/**
 * Màn hình "Quản lý giáo phận / giáo hạt / giáo xứ" — thay `frmGiaoXu.cs` (bản desktop, chỉ
 * sửa đúng 1 dòng của chính file .mdb). Bản web phục vụ nhiều giáo xứ trên cùng máy chủ nên
 * đây là danh sách + thêm + sửa CHO TOÀN BỘ máy chủ. CHỈ tài khoản "Quản trị hệ thống"
 * (LoaiTaiKhoan=9) mới thấy được mục này ở SideNav VÀ mới gọi được các API `/api/quan-tri/*`
 * (backend chặn 403 nếu không đúng — đây là lớp phòng thủ DUY NHẤT, vì GiaoPhan/GiaoHat/GiaoXu
 * không có giao_xu_id nên KHÔNG có RLS bảo vệ). Xem
 * docs/superpowers/specs/man-hinh/quan-ly-giao-xu.md mục 4.
 *
 * KHÔNG có nút xoá cho cả ba cấp (spec mục 4) — xoá một giáo xứ kéo theo toàn bộ dữ liệu giáo
 * dân của họ, hậu quả quá lớn so với lợi ích một nút xoá hiếm dùng.
 */
export function QuanLyGiaoXuPage() {
  const [giaoPhan, setGiaoPhan] = useState<GiaoPhan[] | null>(null)
  const [giaoHat, setGiaoHat] = useState<GiaoHatQuanLy[] | null>(null)
  const [giaoXu, setGiaoXu] = useState<GiaoXuQuanLy[] | null>(null)
  const [loi, setLoi] = useState<string | null>(null)
  const [dangTai, setDangTai] = useState(true)

  const [formPhan, setFormPhan] = useState<FormGiaoPhan | null>(null)
  const [formHat, setFormHat] = useState<FormGiaoHat | null>(null)
  const [formXu, setFormXu] = useState<FormGiaoXu | null>(null)
  const [formTk, setFormTk] = useState<FormTaiKhoan | null>(null)
  const [dangLuu, setDangLuu] = useState(false)
  const [thongBao, setThongBao] = useState<string | null>(null)
  const [thongBaoTk, setThongBaoTk] = useState<string | null>(null)

  function tai() {
    setDangTai(true)
    setLoi(null)
    Promise.all([api.quanTri.giaoPhan.danhSach(), api.quanTri.giaoHat.danhSach(), api.quanTri.giaoXu.danhSach()])
      .then(([p, h, x]) => { setGiaoPhan(p); setGiaoHat(h); setGiaoXu(x) })
      .catch((e: unknown) => setLoi(e instanceof Error ? e.message : String(e)))
      .finally(() => setDangTai(false))
  }

  useEffect(tai, [])

  async function luuGiaoPhan(e: FormEvent) {
    e.preventDefault()
    if (!formPhan) return
    setDangLuu(true); setThongBao(null)
    try {
      const than = { tenGiaoPhan: formPhan.tenGiaoPhan.trim(), ghiChu: formPhan.ghiChu.trim() || null }
      if (formPhan.id === null) await api.quanTri.giaoPhan.tao(than)
      else await api.quanTri.giaoPhan.sua(formPhan.id, than)
      setFormPhan(null); tai()
    } catch (err) { setThongBao(err instanceof Error ? err.message : String(err)) }
    finally { setDangLuu(false) }
  }

  async function luuGiaoHat(e: FormEvent) {
    e.preventDefault()
    if (!formHat) return
    setDangLuu(true); setThongBao(null)
    try {
      const than = { giaoPhanId: formHat.giaoPhanId, tenGiaoHat: formHat.tenGiaoHat.trim(), ghiChu: formHat.ghiChu.trim() || null }
      if (formHat.id === null) await api.quanTri.giaoHat.tao(than)
      else await api.quanTri.giaoHat.sua(formHat.id, than)
      setFormHat(null); tai()
    } catch (err) { setThongBao(err instanceof Error ? err.message : String(err)) }
    finally { setDangLuu(false) }
  }

  async function luuGiaoXu(e: FormEvent) {
    e.preventDefault()
    if (!formXu) return
    setDangLuu(true); setThongBao(null)
    try {
      const than = {
        giaoHatId: formXu.giaoHatId, tenGiaoXu: formXu.tenGiaoXu.trim(),
        diaChi: formXu.diaChi.trim() || null, dienThoai: formXu.dienThoai.trim() || null,
        email: formXu.email.trim() || null, website: formXu.website.trim() || null,
        ghiChu: formXu.ghiChu.trim() || null,
      }
      if (formXu.id === null) await api.quanTri.giaoXu.tao(than)
      else await api.quanTri.giaoXu.sua(formXu.id, than)
      setFormXu(null); tai()
    } catch (err) { setThongBao(err instanceof Error ? err.message : String(err)) }
    finally { setDangLuu(false) }
  }

  async function luuTaiKhoan(e: FormEvent) {
    e.preventDefault()
    if (!formTk) return
    setDangLuu(true); setThongBaoTk(null)
    try {
      await api.quanTri.giaoXu.taoTaiKhoan(formTk.giaoXuId, {
        tenTaiKhoan: formTk.tenTaiKhoan.trim(), matKhau: formTk.matKhau,
        hoTenNguoiDung: formTk.hoTenNguoiDung.trim(),
        email: formTk.email.trim() || null, soDienThoai: formTk.soDienThoai.trim() || null,
      })
      setThongBaoTk(`Đã tạo tài khoản "${formTk.tenTaiKhoan.trim()}" cho giáo xứ "${formTk.tenGiaoXu}".`)
      tai()
    } catch (err) { setThongBaoTk(err instanceof Error ? err.message : String(err)) }
    finally { setDangLuu(false) }
  }

  return (
    <TrangThaiTai dangTai={dangTai} loi={loi} onThuLai={tai}>
      <div style={{ padding: 16, display: 'flex', flexDirection: 'column', gap: 24 }}>
        <p style={{ margin: 0, fontSize: 12.5, color: 'var(--muted, #5b6a86)' }}>
          Danh sách xuyên TOÀN BỘ máy chủ — chỉ tài khoản Quản trị hệ thống mới xem/sửa được.
          Không có nút xoá; muốn ngừng dùng một giáo xứ, đổi tên thêm hậu tố "(ngừng)".
        </p>

        {/* --- Giáo phận --- */}
        <section style={{ display: 'flex', flexDirection: 'column', gap: 10 }}>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
            <h2 style={{ margin: 0 }}>Giáo phận</h2>
            <button type="button" className="btn"
              onClick={() => { setThongBao(null); setFormPhan({ id: null, tenGiaoPhan: '', ghiChu: '' }) }}>
              + Thêm giáo phận
            </button>
          </div>
          <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: 12 }}>
            <thead><tr style={{ textAlign: 'left', borderBottom: '1px solid rgba(14,32,76,.13)' }}>
              <th>Tên giáo phận</th><th>Ghi chú</th><th /></tr></thead>
            <tbody>
              {(giaoPhan ?? []).map((p) => (
                <tr key={p.id} style={{ borderBottom: '1px solid rgba(14,32,76,.06)' }}>
                  <td>{p.tenGiaoPhan}</td><td>{p.ghiChu}</td>
                  <td><button type="button" onClick={() => { setThongBao(null); setFormPhan({ id: p.id, tenGiaoPhan: p.tenGiaoPhan, ghiChu: p.ghiChu ?? '' }) }}>Sửa</button></td>
                </tr>
              ))}
            </tbody>
          </table>
          {formPhan && (
            <form onSubmit={luuGiaoPhan} className="glass" style={{ padding: 16, borderRadius: 'var(--r-card)', display: 'flex', flexDirection: 'column', gap: 10, maxWidth: 380 }}>
              <h3 style={{ margin: 0 }}>{formPhan.id === null ? 'Thêm giáo phận' : 'Sửa giáo phận'}</h3>
              <label className="field" style={oFieldCot}>Tên giáo phận
                <input type="text" required value={formPhan.tenGiaoPhan} onChange={(e) => setFormPhan({ ...formPhan, tenGiaoPhan: e.target.value })} /></label>
              <label className="field" style={oFieldCot}>Ghi chú
                <input type="text" value={formPhan.ghiChu} onChange={(e) => setFormPhan({ ...formPhan, ghiChu: e.target.value })} /></label>
              {thongBao && <div style={{ color: 'var(--rose-ink)', fontSize: 12.5 }}>{thongBao}</div>}
              <div style={{ display: 'flex', gap: 8 }}>
                <button type="submit" className="btn" disabled={dangLuu}>{dangLuu ? 'Đang lưu…' : 'Lưu'}</button>
                <button type="button" onClick={() => setFormPhan(null)} disabled={dangLuu}>Thôi</button>
              </div>
            </form>
          )}
        </section>

        {/* --- Giáo hạt --- */}
        <section style={{ display: 'flex', flexDirection: 'column', gap: 10 }}>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
            <h2 style={{ margin: 0 }}>Giáo hạt</h2>
            <button type="button" className="btn" disabled={(giaoPhan?.length ?? 0) === 0}
              onClick={() => { setThongBao(null); setFormHat({ id: null, giaoPhanId: giaoPhan![0]!.id, tenGiaoHat: '', ghiChu: '' }) }}>
              + Thêm giáo hạt
            </button>
          </div>
          <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: 12 }}>
            <thead><tr style={{ textAlign: 'left', borderBottom: '1px solid rgba(14,32,76,.13)' }}>
              <th>Giáo phận</th><th>Tên giáo hạt</th><th>Ghi chú</th><th /></tr></thead>
            <tbody>
              {(giaoHat ?? []).map((h) => (
                <tr key={h.id} style={{ borderBottom: '1px solid rgba(14,32,76,.06)' }}>
                  <td>{h.tenGiaoPhan}</td><td>{h.tenGiaoHat}</td><td>{h.ghiChu}</td>
                  <td><button type="button" onClick={() => { setThongBao(null); setFormHat({ id: h.id, giaoPhanId: h.giaoPhanId, tenGiaoHat: h.tenGiaoHat, ghiChu: h.ghiChu ?? '' }) }}>Sửa</button></td>
                </tr>
              ))}
            </tbody>
          </table>
          {formHat && (
            <form onSubmit={luuGiaoHat} className="glass" style={{ padding: 16, borderRadius: 'var(--r-card)', display: 'flex', flexDirection: 'column', gap: 10, maxWidth: 380 }}>
              <h3 style={{ margin: 0 }}>{formHat.id === null ? 'Thêm giáo hạt' : 'Sửa giáo hạt'}</h3>
              <label className="field" style={oFieldCot}>Giáo phận
                <select required value={formHat.giaoPhanId} onChange={(e) => setFormHat({ ...formHat, giaoPhanId: e.target.value })}>
                  {(giaoPhan ?? []).map((p) => <option key={p.id} value={p.id}>{p.tenGiaoPhan}</option>)}
                </select></label>
              <label className="field" style={oFieldCot}>Tên giáo hạt
                <input type="text" required value={formHat.tenGiaoHat} onChange={(e) => setFormHat({ ...formHat, tenGiaoHat: e.target.value })} /></label>
              <label className="field" style={oFieldCot}>Ghi chú
                <input type="text" value={formHat.ghiChu} onChange={(e) => setFormHat({ ...formHat, ghiChu: e.target.value })} /></label>
              {thongBao && <div style={{ color: 'var(--rose-ink)', fontSize: 12.5 }}>{thongBao}</div>}
              <div style={{ display: 'flex', gap: 8 }}>
                <button type="submit" className="btn" disabled={dangLuu}>{dangLuu ? 'Đang lưu…' : 'Lưu'}</button>
                <button type="button" onClick={() => setFormHat(null)} disabled={dangLuu}>Thôi</button>
              </div>
            </form>
          )}
        </section>

        {/* --- Giáo xứ --- */}
        <section style={{ display: 'flex', flexDirection: 'column', gap: 10 }}>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
            <h2 style={{ margin: 0 }}>Giáo xứ</h2>
            <button type="button" className="btn" disabled={(giaoHat?.length ?? 0) === 0}
              onClick={() => { setThongBao(null); setFormXu({ id: null, giaoHatId: giaoHat![0]!.id, tenGiaoXu: '', diaChi: '', dienThoai: '', email: '', website: '', ghiChu: '' }) }}>
              + Thêm giáo xứ
            </button>
          </div>
          <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: 12 }}>
            <thead><tr style={{ textAlign: 'left', borderBottom: '1px solid rgba(14,32,76,.13)' }}>
              <th>Giáo hạt</th><th>Tên giáo xứ</th><th>Địa chỉ</th><th>Số tài khoản</th><th /></tr></thead>
            <tbody>
              {(giaoXu ?? []).map((x) => (
                <tr key={x.id} style={{ borderBottom: '1px solid rgba(14,32,76,.06)' }}>
                  <td>{x.tenGiaoHat}</td>
                  <td>{x.tenGiaoXu}{x.coTrungTen && <span title="Trùng tên với giáo xứ khác" style={{ color: 'var(--rose-ink)' }}> ⚠</span>}</td>
                  <td>{x.diaChi}</td>
                  <td>{x.soTaiKhoan}</td>
                  <td>
                    <button type="button" onClick={() => { setThongBao(null); setFormXu({ id: x.id, giaoHatId: x.giaoHatId ?? '', tenGiaoXu: x.tenGiaoXu, diaChi: x.diaChi ?? '', dienThoai: x.dienThoai ?? '', email: x.email ?? '', website: x.website ?? '', ghiChu: x.ghiChu ?? '' }) }}>Sửa</button>{' '}
                    <button type="button" onClick={() => { setThongBaoTk(null); setFormTk({ giaoXuId: x.id, tenGiaoXu: x.tenGiaoXu, tenTaiKhoan: '', matKhau: '', hoTenNguoiDung: '', email: '', soDienThoai: '' }) }}>Tạo tài khoản quản trị</button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
          {formXu && (
            <form onSubmit={luuGiaoXu} className="glass" style={{ padding: 16, borderRadius: 'var(--r-card)', display: 'flex', flexDirection: 'column', gap: 10, maxWidth: 420 }}>
              <h3 style={{ margin: 0 }}>{formXu.id === null ? 'Thêm giáo xứ' : `Sửa giáo xứ: ${formXu.tenGiaoXu}`}</h3>
              <label className="field" style={oFieldCot}>Giáo hạt
                <select required value={formXu.giaoHatId} onChange={(e) => setFormXu({ ...formXu, giaoHatId: e.target.value })}>
                  {(giaoHat ?? []).map((h) => <option key={h.id} value={h.id}>{h.tenGiaoHat} ({h.tenGiaoPhan})</option>)}
                </select></label>
              <label className="field" style={oFieldCot}>Tên giáo xứ
                <input type="text" required value={formXu.tenGiaoXu} onChange={(e) => setFormXu({ ...formXu, tenGiaoXu: e.target.value })} /></label>
              <label className="field" style={oFieldCot}>Địa chỉ
                <input type="text" value={formXu.diaChi} onChange={(e) => setFormXu({ ...formXu, diaChi: e.target.value })} /></label>
              <label className="field" style={oFieldCot}>Điện thoại
                <input type="text" value={formXu.dienThoai} onChange={(e) => setFormXu({ ...formXu, dienThoai: e.target.value })} /></label>
              <label className="field" style={oFieldCot}>Email
                <input type="text" value={formXu.email} onChange={(e) => setFormXu({ ...formXu, email: e.target.value })} /></label>
              <label className="field" style={oFieldCot}>Website
                <input type="text" value={formXu.website} onChange={(e) => setFormXu({ ...formXu, website: e.target.value })} /></label>
              <label className="field" style={oFieldCot}>Ghi chú
                <input type="text" value={formXu.ghiChu} onChange={(e) => setFormXu({ ...formXu, ghiChu: e.target.value })} /></label>
              {thongBao && <div style={{ color: 'var(--rose-ink)', fontSize: 12.5 }}>{thongBao}</div>}
              <div style={{ display: 'flex', gap: 8 }}>
                <button type="submit" className="btn" disabled={dangLuu}>{dangLuu ? 'Đang lưu…' : 'Lưu'}</button>
                <button type="button" onClick={() => setFormXu(null)} disabled={dangLuu}>Thôi</button>
              </div>
            </form>
          )}

          {formTk && (
            <form onSubmit={luuTaiKhoan} className="glass" style={{ padding: 16, borderRadius: 'var(--r-card)', display: 'flex', flexDirection: 'column', gap: 10, maxWidth: 420 }}>
              <h3 style={{ margin: 0 }}>Tạo tài khoản quản trị cho: {formTk.tenGiaoXu}</h3>
              <label className="field" style={oFieldCot}>Họ tên người dùng
                <input type="text" required value={formTk.hoTenNguoiDung} onChange={(e) => setFormTk({ ...formTk, hoTenNguoiDung: e.target.value })} /></label>
              <label className="field" style={oFieldCot}>Tên đăng nhập
                <input type="text" required pattern="^[a-zA-Z0-9]+$" title="Chỉ gồm chữ và số" value={formTk.tenTaiKhoan} onChange={(e) => setFormTk({ ...formTk, tenTaiKhoan: e.target.value })} /></label>
              <label className="field" style={oFieldCot}>Mật khẩu
                <input type="password" required minLength={8} value={formTk.matKhau} onChange={(e) => setFormTk({ ...formTk, matKhau: e.target.value })} /></label>
              <label className="field" style={oFieldCot}>Email
                <input type="text" value={formTk.email} onChange={(e) => setFormTk({ ...formTk, email: e.target.value })} /></label>
              <label className="field" style={oFieldCot}>Số điện thoại
                <input type="text" value={formTk.soDienThoai} onChange={(e) => setFormTk({ ...formTk, soDienThoai: e.target.value })} /></label>
              {thongBaoTk && <div style={{ fontSize: 12.5 }}>{thongBaoTk}</div>}
              <div style={{ display: 'flex', gap: 8 }}>
                <button type="submit" className="btn" disabled={dangLuu}>{dangLuu ? 'Đang tạo…' : 'Tạo tài khoản'}</button>
                <button type="button" onClick={() => setFormTk(null)} disabled={dangLuu}>Đóng</button>
              </div>
            </form>
          )}
        </section>
      </div>
    </TrangThaiTai>
  )
}
