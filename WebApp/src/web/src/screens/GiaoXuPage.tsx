import { useEffect, useState, type CSSProperties, type FormEvent } from 'react'
import { api } from '../api/client'
import { TrangThaiTai } from '../components/TrangThaiTai'

type Form = {
  tenGiaoXu: string; diaChi: string; dienThoai: string; email: string; website: string; ghiChu: string
}

const oFieldCot: CSSProperties = { flexDirection: 'column', alignItems: 'stretch' }

/**
 * Màn hình "Giáo xứ" — văn phòng giáo xứ tự sửa thông tin xứ mình, thay `frmGiaoXu.cs` (bản
 * desktop, 280 dòng). Xem docs/superpowers/specs/man-hinh/giao-xu.md.
 *
 * PHÂN BIỆT RÕ với "Quản lý giáo xứ" (`QuanLyGiaoXuPage`, chỉ Quản trị hệ thống thấy được):
 * màn hình đó xem/sửa MỌI giáo xứ trên máy chủ; màn hình NÀY chỉ cho sửa đúng giáo xứ của tài
 * khoản đang đăng nhập (giao_xu_id từ claim) — không có ô chọn giáo xứ nào trên form vì không
 * cần và không được phép chọn giáo xứ khác.
 *
 * CỐ Ý bỏ khỏi bản web so với desktop (ghi ở can-review-sau.md, không phải "quên"):
 * - Sửa tên giáo phận/giáo hạt: bản desktop sửa thẳng dòng GiaoPhan/GiaoHat duy nhất của
 *   database 1-giáo-xứ; ở mô hình nhiều giáo xứ dùng chung máy chủ, một Giáo hạt có thể có
 *   NHIỀU giáo xứ — cho một giáo xứ tự đổi tên giáo hạt sẽ vô tình đổi luôn tên hiển thị của
 *   giáo xứ khác cùng giáo hạt. Muốn đổi, dùng "Quản lý giáo xứ" (Quản trị hệ thống).
 * - Quản lý danh sách Linh mục (gxLinhMucList1 của form gốc): không nằm trong yêu cầu migrate
 *   lượt này; bảng LinhMuc đã có RLS theo giáo xứ nên an toàn để làm ở một màn hình riêng sau.
 * - Ảnh đại diện giáo xứ (txtHinh/btnBrowse — copy file .bmp vào thư mục cài đặt cục bộ): mô
 *   hình lưu ảnh của desktop (đường dẫn ổ đĩa cục bộ) không áp dụng được cho máy chủ web.
 */
export function GiaoXuPage() {
  const [form, setForm] = useState<Form | null>(null)
  const [loi, setLoi] = useState<string | null>(null)
  const [dangTai, setDangTai] = useState(true)
  const [dangLuu, setDangLuu] = useState(false)
  const [thongBao, setThongBao] = useState<string | null>(null)
  const [loiLuu, setLoiLuu] = useState<string | null>(null)

  function tai() {
    setDangTai(true)
    setLoi(null)
    api.giaoXu.layThongTin()
      .then((tt) => setForm({
        tenGiaoXu: tt.tenGiaoXu, diaChi: tt.diaChi ?? '', dienThoai: tt.dienThoai ?? '',
        email: tt.email ?? '', website: tt.website ?? '', ghiChu: tt.ghiChu ?? '',
      }))
      .catch((e: unknown) => setLoi(e instanceof Error ? e.message : String(e)))
      .finally(() => setDangTai(false))
  }

  useEffect(tai, [])

  // Cùng 2 điều kiện bắt buộc như bản desktop (frmGiaoXu.cs:70-90, hai thông báo nguyên văn):
  // "Hãy nhập tên giáo xứ!" và "Hãy nhập địa chỉ giáo xứ!" — bản web kiểm ở cả hai phía (đã
  // thấy backend chặn 400, đây là kiểm trước để không mất công gọi máy chủ).
  async function luu(e: FormEvent) {
    e.preventDefault()
    if (!form) return
    setLoiLuu(null)
    if (form.tenGiaoXu.trim() === '') { setLoiLuu('Hãy nhập tên giáo xứ!'); return }
    if (form.diaChi.trim() === '') { setLoiLuu('Hãy nhập địa chỉ giáo xứ!'); return }
    setDangLuu(true); setThongBao(null)
    try {
      await api.giaoXu.capNhat({
        tenGiaoXu: form.tenGiaoXu.trim(), diaChi: form.diaChi.trim() || null,
        dienThoai: form.dienThoai.trim() || null, email: form.email.trim() || null,
        website: form.website.trim() || null, ghiChu: form.ghiChu.trim() || null,
      })
      // Nguyên văn thông báo desktop (frmGiaoXu.cs:169): "Đã cập nhật thông tin giáo xứ!"
      setThongBao('Đã cập nhật thông tin giáo xứ!')
      tai()
    } catch (err) {
      setLoiLuu(err instanceof Error ? err.message : String(err))
    } finally {
      setDangLuu(false)
    }
  }

  return (
    <TrangThaiTai dangTai={dangTai} loi={loi} onThuLai={tai}>
      {form && (
        <form onSubmit={luu} style={{ padding: 16, display: 'flex', flexDirection: 'column', gap: 12, maxWidth: 460 }}>
          <p style={{ margin: 0, fontSize: 12.5, color: 'var(--muted, #5b6a86)' }}>
            Sửa thông tin của chính giáo xứ mình. Không thể xem hay sửa giáo xứ khác từ màn hình này.
          </p>
          <label className="field" style={oFieldCot}>Tên giáo xứ
            <input type="text" style={{ fontSize: 12 }}
              value={form.tenGiaoXu} onChange={(e) => setForm({ ...form, tenGiaoXu: e.target.value })} /></label>
          <label className="field" style={oFieldCot}>Địa chỉ
            <input type="text" style={{ fontSize: 12 }}
              value={form.diaChi} onChange={(e) => setForm({ ...form, diaChi: e.target.value })} /></label>
          <label className="field" style={oFieldCot}>Điện thoại
            <input type="text" style={{ fontSize: 12 }}
              value={form.dienThoai} onChange={(e) => setForm({ ...form, dienThoai: e.target.value })} /></label>
          <label className="field" style={oFieldCot}>Email
            <input type="text" style={{ fontSize: 12 }}
              value={form.email} onChange={(e) => setForm({ ...form, email: e.target.value })} /></label>
          <label className="field" style={oFieldCot}>Website
            <input type="text" style={{ fontSize: 12 }}
              value={form.website} onChange={(e) => setForm({ ...form, website: e.target.value })} /></label>
          <label className="field" style={oFieldCot}>Ghi chú
            <textarea rows={4} style={{ fontSize: 12 }}
              value={form.ghiChu} onChange={(e) => setForm({ ...form, ghiChu: e.target.value })} /></label>

          {loiLuu && <div style={{ color: 'var(--rose-ink)', fontSize: 12.5 }}>{loiLuu}</div>}
          {thongBao && <div style={{ color: 'var(--muted, #5b6a86)', fontSize: 12.5 }}>{thongBao}</div>}

          <div>
            <button type="submit" className="btn" disabled={dangLuu}>{dangLuu ? 'Đang lưu…' : 'Cập nhật'}</button>
          </div>
        </form>
      )}
    </TrangThaiTai>
  )
}
