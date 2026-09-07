import { useEffect, useState, type FormEvent } from 'react'
import { api } from '../api/client'
import type { GiaoHo } from '../api/types'
import { TrangThaiTai } from '../components/TrangThaiTai'

type FormState = { id: string | null; tenGiaoHo: string; giaoHoChaId: string | null }
const FORM_TRONG: FormState = { id: null, tenGiaoHo: '', giaoHoChaId: null }

/**
 * Danh mục Giáo họ của giáo xứ đang đăng nhập — thay `data/giaoHoTam.ts` (tên cứng, không có
 * Id) trước đây. Khác màn hình "Quản lý giáo xứ" (giao phận/hạt/xứ, policy "QuanTriHeThong"):
 * Giáo họ CÓ giao_xu_id nên nằm trong phạm vi giáo xứ của người gọi, được cả bộ lọc EF lẫn RLS
 * bảo vệ — mọi tài khoản đã đăng nhập đều xem/thêm/sửa được, không cần quyền quản trị riêng
 * (đúng như bản desktop: đây là danh mục nghiệp vụ thường dùng, không phải chức năng quản trị).
 * Không có nút xoá ở Phase 1 — nhất quán với quyết định "chặn xoá xuyên phạm vi" của màn hình
 * Quản lý giáo xứ (một giáo họ có thể đã gắn với giáo dân/gia đình thật).
 *
 * "Giáo họ cha" (cột `giaoHoChaId`, khớp `MaGiaoHoCha` bản desktop — `frmGiaoHo.cs` tự mở lại
 * CHÍNH form này với `MaGiaoHoCha` đã đặt để quản lý "Giáo khu" con của một giáo họ, xem
 * giao-ho.md mục 5/8): bản web KHÔNG dựng lại UI đệ quy "mở form con" — thay bằng một select
 * chọn giáo họ cha ngay trong form phẳng hiện có (đơn giản hơn, đủ dùng vì dữ liệu thật hiện
 * chỉ có 1 giáo họ, không có giáo khu nào) và một cột "Giáo họ cha" trên lưới để nhận biết quan
 * hệ lồng nhau nếu về sau có nhập. Xem can-review-sau.md.
 */
export function GiaoHoListPage() {
  const [rows, setRows] = useState<GiaoHo[] | null>(null)
  const [loi, setLoi] = useState<string | null>(null)
  const [dangTai, setDangTai] = useState(true)
  const [form, setForm] = useState<FormState | null>(null)
  const [dangLuu, setDangLuu] = useState(false)
  const [thongBao, setThongBao] = useState<string | null>(null)

  function tai() {
    setDangTai(true)
    setLoi(null)
    api.giaoHo.danhMuc()
      .then(setRows)
      .catch((e: unknown) => setLoi(e instanceof Error ? e.message : String(e)))
      .finally(() => setDangTai(false))
  }

  useEffect(tai, [])

  function tenGiaoHoCha(id: string | null): string {
    if (!id) return ''
    return rows?.find((r) => r.id === id)?.tenGiaoHo ?? ''
  }

  async function onSubmit(e: FormEvent) {
    e.preventDefault()
    if (!form) return
    setDangLuu(true)
    setThongBao(null)
    try {
      const than = { tenGiaoHo: form.tenGiaoHo.trim(), giaoHoChaId: form.giaoHoChaId }
      if (form.id === null) await api.giaoHo.them(than)
      else await api.giaoHo.sua(form.id, than)
      setForm(null)
      tai()
    } catch (err) {
      setThongBao(err instanceof Error ? err.message : String(err))
    } finally {
      setDangLuu(false)
    }
  }

  return (
    <TrangThaiTai dangTai={dangTai} loi={loi} onThuLai={tai}>
      <div style={{ padding: 16, display: 'flex', flexDirection: 'column', gap: 14 }}>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
          <h2 style={{ margin: 0 }}>Giáo họ</h2>
          <button type="button" className="btn" onClick={() => { setThongBao(null); setForm({ ...FORM_TRONG }) }}>
            + Thêm giáo họ
          </button>
        </div>

        <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: 12 }}>
          <thead>
            <tr style={{ textAlign: 'left', borderBottom: '1px solid rgba(14,32,76,.13)' }}>
              <th>Mã cũ</th>
              <th>Tên giáo họ</th>
              <th>Giáo họ cha</th>
              <th />
            </tr>
          </thead>
          <tbody>
            {(rows ?? []).map((gh) => (
              <tr key={gh.id} style={{ borderBottom: '1px solid rgba(14,32,76,.06)' }}>
                <td>{gh.maGiaoHoCu}</td>
                <td>{gh.tenGiaoHo}</td>
                <td className="muted">{tenGiaoHoCha(gh.giaoHoChaId)}</td>
                <td>
                  <button type="button" onClick={() => { setThongBao(null); setForm({ id: gh.id, tenGiaoHo: gh.tenGiaoHo, giaoHoChaId: gh.giaoHoChaId }) }}>
                    Sửa
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>

        {form && (
          <form onSubmit={onSubmit} className="glass"
            style={{ padding: 16, borderRadius: 'var(--r-card)', display: 'flex', flexDirection: 'column', gap: 10, maxWidth: 380 }}>
            <h3 style={{ margin: 0 }}>{form.id === null ? 'Thêm giáo họ' : 'Sửa giáo họ'}</h3>
            <label className="field" style={{ flexDirection: 'column', alignItems: 'stretch' }}>
              Tên giáo họ
              <input type="text" required value={form.tenGiaoHo}
                onChange={(e) => setForm({ ...form, tenGiaoHo: e.target.value })} />
            </label>
            <label className="field" style={{ flexDirection: 'column', alignItems: 'stretch' }}>
              Giáo họ cha (để trống nếu đây là giáo họ cấp 1)
              <select value={form.giaoHoChaId ?? ''}
                onChange={(e) => setForm({ ...form, giaoHoChaId: e.target.value || null })}>
                <option value="">— Không có (giáo họ cấp 1) —</option>
                {(rows ?? []).filter((r) => r.id !== form.id).map((r) => (
                  <option key={r.id} value={r.id}>{r.tenGiaoHo}</option>
                ))}
              </select>
            </label>
            {thongBao && <div role="alert" style={{ color: 'var(--rose-ink)', fontSize: 12.5 }}>{thongBao}</div>}
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
