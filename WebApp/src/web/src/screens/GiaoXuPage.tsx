import { useEffect, useState, type CSSProperties, type FormEvent } from 'react'
import { api } from '../api/client'
import { TrangThaiTai } from '../components/TrangThaiTai'
import { GxDate } from '../components/GxDate'
import type { LinhMuc } from '../api/types'
import { dinhDangNgay } from '../lib/ngay'

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
 * CỐ Ý bỏ khỏi bản web so với desktop (ghi ở can-review-sau.md/giao-xu.md, không phải "quên"):
 * - Sửa tên giáo phận/giáo hạt: bản desktop sửa thẳng dòng GiaoPhan/GiaoHat duy nhất của
 *   database 1-giáo-xứ; ở mô hình nhiều giáo xứ dùng chung máy chủ, một Giáo hạt có thể có
 *   NHIỀU giáo xứ — cho một giáo xứ tự đổi tên giáo hạt sẽ vô tình đổi luôn tên hiển thị của
 *   giáo xứ khác cùng giáo hạt. Hiển thị CHỈ ĐỌC hai trường này (đã thêm 2026-09-10 theo UX
 *   review mục 3) — muốn đổi, dùng "Quản lý giáo xứ" (Quản trị hệ thống).
 * - Ảnh đại diện giáo xứ (txtHinh/btnBrowse — copy file .bmp vào thư mục cài đặt cục bộ): mô
 *   hình lưu ảnh của desktop (đường dẫn ổ đĩa cục bộ) không áp dụng được cho máy chủ web; cần
 *   thiết kế upload riêng như AnhDaiDienService — để lượt sau.
 *
 * "Danh sách các cha quản xứ" (thêm 2026-09-10): khác hai mục trên, bảng LinhMuc kế thừa
 * ThucTheCoSo nên CÓ RLS bảo vệ theo giáo xứ như mọi màn hình nghiệp vụ khác — an toàn để làm
 * ngay, không có rủi ro xuyên giáo xứ nào cần cân nhắc thêm.
 */
export function GiaoXuPage() {
  const [form, setForm] = useState<Form | null>(null)
  const [tenGiaoPhan, setTenGiaoPhan] = useState<string | null>(null)
  const [tenGiaoHat, setTenGiaoHat] = useState<string | null>(null)
  const [loi, setLoi] = useState<string | null>(null)
  const [dangTai, setDangTai] = useState(true)
  const [dangLuu, setDangLuu] = useState(false)
  const [thongBao, setThongBao] = useState<string | null>(null)
  const [loiLuu, setLoiLuu] = useState<string | null>(null)

  function tai() {
    setDangTai(true)
    setLoi(null)
    api.giaoXu.layThongTin()
      .then((tt) => {
        setForm({
          tenGiaoXu: tt.tenGiaoXu, diaChi: tt.diaChi ?? '', dienThoai: tt.dienThoai ?? '',
          email: tt.email ?? '', website: tt.website ?? '', ghiChu: tt.ghiChu ?? '',
        })
        setTenGiaoPhan(tt.tenGiaoPhan)
        setTenGiaoHat(tt.tenGiaoHat)
      })
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
        <div style={{ padding: 16, display: 'flex', flexDirection: 'column', gap: 24 }}>
          <form onSubmit={luu} style={{ display: 'flex', flexDirection: 'column', gap: 12, maxWidth: 460 }}>
            <p style={{ margin: 0, fontSize: 12.5, color: 'var(--muted, #5b6a86)' }}>
              Sửa thông tin của chính giáo xứ mình. Không thể xem hay sửa giáo xứ khác từ màn hình này.
            </p>

            {/* Chỉ đọc — xem lý do multi-tenant ở phần ghi chú đầu file. Luôn hiện cả khi null
                (chưa được "Quản lý giáo xứ" gán giáo hạt) để không trông như một lỗi tải dữ liệu. */}
            <label className="field" style={oFieldCot}>Giáo phận
              <input type="text" style={{ fontSize: 12 }} value={tenGiaoPhan ?? 'Chưa gán'} disabled /></label>
            <label className="field" style={oFieldCot}>Giáo hạt
              <input type="text" style={{ fontSize: 12 }} value={tenGiaoHat ?? 'Chưa gán'} disabled /></label>
            <p style={{ margin: 0, fontSize: 11.5, color: 'var(--muted, #5b6a86)' }}>
              Giáo phận/Giáo hạt chỉ xem được ở đây — một giáo hạt có nhiều giáo xứ dùng chung
              nên không tự đổi tên từ màn hình của một giáo xứ. Muốn đổi, liên hệ Quản trị hệ thống.
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

          <DanhSachChaQuanXu />
        </div>
      )}
    </TrangThaiTai>
  )
}

type FormLinhMuc = {
  id: string | null
  tenThanh: string
  hoTen: string
  ngaySinh: string | null
  chucVu: string
  tuNgay: string | null
  denNgay: string | null
  ghiChu: string
  dienThoai: string
  email: string
}

const LINH_MUC_TRONG: FormLinhMuc = {
  id: null, tenThanh: '', hoTen: '', ngaySinh: null, chucVu: '',
  tuNgay: null, denNgay: null, ghiChu: '', dienThoai: '', email: '',
}

/**
 * "Danh sách các cha quản xứ" — thay lưới `gxLinhMucList1` của `frmGiaoXu.cs`. Bảng đơn giản +
 * form thêm/sửa hiện/ẩn tại chỗ, cùng khuôn với `TaiKhoanListPage.tsx` (danh sách vài chục dòng
 * nhiều nhất, không cần AG Grid lọc/sắp xếp phức tạp).
 */
function DanhSachChaQuanXu() {
  const [rows, setRows] = useState<LinhMuc[] | null>(null)
  const [loi, setLoi] = useState<string | null>(null)
  const [dangTai, setDangTai] = useState(true)
  const [form, setForm] = useState<FormLinhMuc | null>(null)
  const [dangLuu, setDangLuu] = useState(false)
  const [thongBao, setThongBao] = useState<string | null>(null)

  function tai() {
    setDangTai(true)
    setLoi(null)
    api.linhMuc.danhSach()
      .then(setRows)
      .catch((e: unknown) => setLoi(e instanceof Error ? e.message : String(e)))
      .finally(() => setDangTai(false))
  }

  useEffect(tai, [])

  function moThem() {
    setThongBao(null)
    setForm({ ...LINH_MUC_TRONG })
  }

  function moSua(l: LinhMuc) {
    setThongBao(null)
    setForm({
      id: l.id, tenThanh: l.tenThanh ?? '', hoTen: l.hoTen, ngaySinh: l.ngaySinh,
      chucVu: l.chucVu ?? '', tuNgay: l.tuNgay, denNgay: l.denNgay,
      ghiChu: l.ghiChu ?? '', dienThoai: l.dienThoai ?? '', email: l.email ?? '',
    })
  }

  async function onSubmit(e: FormEvent) {
    e.preventDefault()
    if (!form) return
    setDangLuu(true)
    setThongBao(null)
    const than = {
      tenThanh: form.tenThanh.trim() || null,
      hoTen: form.hoTen.trim(),
      ngaySinh: form.ngaySinh,
      chucVu: form.chucVu.trim() || null,
      tuNgay: form.tuNgay,
      denNgay: form.denNgay,
      ghiChu: form.ghiChu.trim() || null,
      dienThoai: form.dienThoai.trim() || null,
      email: form.email.trim() || null,
    }
    try {
      if (form.id === null) await api.linhMuc.them(than)
      else await api.linhMuc.sua(form.id, than)
      setForm(null)
      tai()
    } catch (err) {
      setThongBao(err instanceof Error ? err.message : String(err))
    } finally {
      setDangLuu(false)
    }
  }

  async function onXoa(l: LinhMuc) {
    if (!confirm(`Xoá "${l.hoTen}" khỏi danh sách các cha quản xứ?`)) return
    try {
      await api.linhMuc.xoa(l.id)
      tai()
    } catch (err) {
      setLoi(err instanceof Error ? err.message : String(err))
    }
  }

  return (
    <TrangThaiTai dangTai={dangTai} loi={loi} onThuLai={tai}>
      <div style={{ display: 'flex', flexDirection: 'column', gap: 14, maxWidth: 720 }}>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
          <h2 style={{ margin: 0, fontSize: 15 }}>Danh sách các cha quản xứ</h2>
          <button type="button" className="btn" onClick={moThem}>+ Thêm</button>
        </div>

        <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: 12 }}>
          <thead>
            <tr style={{ textAlign: 'left', borderBottom: '1px solid rgba(14,32,76,.13)' }}>
              <th>Tên thánh</th>
              <th>Họ tên</th>
              <th>Chức vụ</th>
              <th>Từ ngày</th>
              <th>Đến ngày</th>
              <th />
            </tr>
          </thead>
          <tbody>
            {(rows ?? []).map((l) => (
              <tr key={l.id} style={{ borderBottom: '1px solid rgba(14,32,76,.06)' }}>
                <td>{l.tenThanh}</td>
                <td>{l.hoTen}</td>
                <td>{l.chucVu}</td>
                <td>{dinhDangNgay(l.tuNgay)}</td>
                <td>{dinhDangNgay(l.denNgay)}</td>
                <td>
                  <button type="button" className="btn btn-quiet btn-sm" onClick={() => moSua(l)}>Sửa</button>{' '}
                  <button type="button" className="btn btn-danger btn-sm" onClick={() => onXoa(l)}>Xoá</button>
                </td>
              </tr>
            ))}
            {rows?.length === 0 && (
              <tr><td colSpan={6} style={{ color: 'var(--muted, #5b6a86)', padding: '8px 0' }}>Chưa có cha quản xứ nào.</td></tr>
            )}
          </tbody>
        </table>

        {form && (
          <form
            onSubmit={onSubmit}
            className="glass"
            style={{ padding: 16, borderRadius: 'var(--r-card)', display: 'flex', flexDirection: 'column', gap: 10, maxWidth: 420 }}
          >
            <h3 style={{ margin: 0 }}>{form.id === null ? 'Thêm cha quản xứ' : `Sửa: ${form.hoTen}`}</h3>

            <label className="field" style={oFieldCot}>Tên thánh
              <input type="text" value={form.tenThanh} onChange={(e) => setForm({ ...form, tenThanh: e.target.value })} /></label>

            <label className="field" style={oFieldCot}>Họ tên
              <input type="text" required value={form.hoTen} onChange={(e) => setForm({ ...form, hoTen: e.target.value })} /></label>

            <label className="field" style={oFieldCot}>Ngày sinh
              <GxDate key={`ngaySinh-${form.id ?? 'moi'}`} id="lm-ngaysinh" defaultValue={form.ngaySinh}
                onIsoChange={(iso) => setForm({ ...form, ngaySinh: iso || null })} /></label>

            <label className="field" style={oFieldCot}>Chức vụ
              <input type="text" placeholder="Ví dụ: Chánh xứ, Phó xứ"
                value={form.chucVu} onChange={(e) => setForm({ ...form, chucVu: e.target.value })} /></label>

            <label className="field" style={oFieldCot}>Từ ngày (nhận xứ)
              <GxDate key={`tuNgay-${form.id ?? 'moi'}`} id="lm-tungay" defaultValue={form.tuNgay}
                onIsoChange={(iso) => setForm({ ...form, tuNgay: iso || null })} /></label>

            <label className="field" style={oFieldCot}>Đến ngày (mãn nhiệm — để trống nếu vẫn đang quản xứ)
              <GxDate key={`denNgay-${form.id ?? 'moi'}`} id="lm-denngay" defaultValue={form.denNgay}
                onIsoChange={(iso) => setForm({ ...form, denNgay: iso || null })} /></label>

            <label className="field" style={oFieldCot}>Điện thoại
              <input type="text" value={form.dienThoai} onChange={(e) => setForm({ ...form, dienThoai: e.target.value })} /></label>

            <label className="field" style={oFieldCot}>Email
              <input type="text" value={form.email} onChange={(e) => setForm({ ...form, email: e.target.value })} /></label>

            <label className="field" style={oFieldCot}>Ghi chú
              <textarea rows={3} value={form.ghiChu} onChange={(e) => setForm({ ...form, ghiChu: e.target.value })} /></label>

            {thongBao && <div style={{ color: 'var(--rose-ink)', fontSize: 12.5 }}>{thongBao}</div>}

            <div style={{ display: 'flex', gap: 8 }}>
              <button type="submit" className="btn" disabled={dangLuu}>{dangLuu ? 'Đang lưu…' : 'Lưu'}</button>
              <button type="button" className="btn btn-quiet" onClick={() => setForm(null)} disabled={dangLuu}>Thôi</button>
            </div>
          </form>
        )}
      </div>
    </TrangThaiTai>
  )
}
