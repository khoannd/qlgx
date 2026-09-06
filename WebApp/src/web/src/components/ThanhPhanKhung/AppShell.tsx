import { useEffect, useRef, useState, type ReactNode } from 'react'
import { SideNav } from './SideNav'
import { useAuth } from '../../api/AuthContext'

type NguoiDungHienTai = { tenTaiKhoan: string; hoTen: string | null; loaiTaiKhoan: number | null }

type Props = {
  dangChonNav: string
  onNavigate: (id: string) => void
  children: ReactNode
  nguoiDung: NguoiDungHienTai
}

const TEN_LOAI_TAI_KHOAN: Record<number, string> = {
  0: 'Quản trị viên',
  1: 'Người nhập 1',
  2: 'Người nhập 2',
}

/** Hai chữ cái đầu để hiện trong avatar tròn — giống "VP" cũ nhưng suy từ tên thật thay vì
 * viết cứng. Không có họ tên thì rơi về hai ký tự đầu tên đăng nhập. */
function chuVietTat(nguoiDung: NguoiDungHienTai): string {
  const nguon = nguoiDung.hoTen?.trim() || nguoiDung.tenTaiKhoan
  const tuList = nguon.split(/\s+/).filter(Boolean)
  const tat = tuList.length >= 2
    ? tuList[tuList.length - 2]![0] + tuList[tuList.length - 1]![0]
    : nguon.slice(0, 2)
  return tat.toUpperCase()
}

/** Khung ứng dụng: thanh trên (topbar) + điều hướng trái (SideNav) + vùng làm
 * việc bên phải (do `children` — thường là `<TabDocs>` — quyết định). Cấu
 * trúc thẻ HTML chép nguyên từ `WebApp/prototype/qlgx-prototype.html`
 * (phần `<header class="topbar">`), chỉ đổi `class` thành `className`. */
export function AppShell({ dangChonNav, onNavigate, children, nguoiDung }: Props) {
  const { dangXuat } = useAuth()
  const tenLoai = nguoiDung.loaiTaiKhoan !== null ? TEN_LOAI_TAI_KHOAN[nguoiDung.loaiTaiKhoan] : undefined
  const laQuanTri = nguoiDung.loaiTaiKhoan === 0

  // Ba menu "Hệ thống"/"Công cụ"/"Trợ giúp" chép từ bản mẫu tĩnh (qlgx-prototype.html) —
  // phần lớn mục bên trong vẫn là chỗ giữ chỗ (task sau), NHƯNG "Đăng xuất" trong menu "Hệ
  // thống" phải bấm được thật ngay từ Task 14. Bật/tắt bằng state đơn giản, đóng khi bấm ra
  // ngoài — không cần thư viện menu nào cho ba nút này.
  const [menuMo, setMenuMo] = useState<string | null>(null)
  const khungRef = useRef<HTMLDivElement>(null)
  useEffect(() => {
    function onClickNgoai(e: MouseEvent) {
      if (khungRef.current && !khungRef.current.contains(e.target as Node)) setMenuMo(null)
    }
    document.addEventListener('mousedown', onClickNgoai)
    return () => document.removeEventListener('mousedown', onClickNgoai)
  }, [])

  return (
    <div className="app">
      <header className="topbar glass glass-solid">
        <div className="brand">
          <span className="mark">
            <svg viewBox="0 0 24 24" fill="currentColor" aria-hidden="true">
              <path d="M11 2h2v3h3v2h-3v3.2l6 3.4V22h-6.5v-4a2.5 2.5 0 0 0-5 0v4H1v-8.4l6-3.4V7H4V5h3V2h4z" />
            </svg>
          </span>
          <span>
            <span className="name">QLGX</span>
            <span className="sub">Quản lý giáo xứ</span>
          </span>
        </div>

        <button className="parish-chip" type="button">
          <span className="dot" />
          <b>Giáo xứ Thánh Tâm</b>
          <span className="caret">&#9660;</span>
        </button>

        <label className="searchbox">
          <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.2" aria-hidden="true">
            <circle cx="11" cy="11" r="7" />
            <path d="m20 20-3.6-3.6" strokeLinecap="round" />
          </svg>
          <input type="text" placeholder="Tìm giáo dân, gia đình, sổ bí tích…" aria-label="Tìm kiếm toàn hệ thống" />
          <kbd>Ctrl K</kbd>
        </label>

        <div className="spacer" />

        <div ref={khungRef} style={{ display: 'contents' }}>
          <button className="menubtn" type="button" aria-expanded={menuMo === 'm1'}
            onClick={() => setMenuMo(menuMo === 'm1' ? null : 'm1')}>Hệ thống</button>
          <button className="menubtn" type="button" aria-expanded={menuMo === 'm2'}
            onClick={() => setMenuMo(menuMo === 'm2' ? null : 'm2')}>Công cụ</button>
          <button className="menubtn" type="button" aria-expanded={menuMo === 'm3'}
            onClick={() => setMenuMo(menuMo === 'm3' ? null : 'm3')}>Trợ giúp</button>

          <span className="demo-chip">Môi trường thử nghiệm</span>
          <span className="avatar" title={tenLoai ? `${tenLoai}: ${nguoiDung.tenTaiKhoan}` : nguoiDung.tenTaiKhoan}>
            {chuVietTat(nguoiDung)}
          </span>

          <div className="menu-pop" id="m1" hidden={menuMo !== 'm1'}>
            <button type="button" disabled>Nhập dữ liệu từ cùng chương trình</button>
            <button type="button" disabled>Nhập dữ liệu từ MS Excel</button>
            <button type="button" disabled>Nhập dữ liệu từ chương trình MGC</button>
            <hr />
            <button type="button" disabled>Sao lưu dữ liệu chương trình</button>
            <button type="button" disabled>Xuất dữ liệu ra Excel</button>
            <button type="button" disabled>Khôi phục dữ liệu</button>
            <hr />
            <button type="button" disabled>Đổi mật khẩu</button>
            <button type="button" onClick={() => { setMenuMo(null); dangXuat() }}>Đăng xuất</button>
          </div>
          <div className="menu-pop" id="m2" hidden={menuMo !== 'm2'}>
            <button type="button" disabled>Tìm giáo dân</button>
            <button type="button" disabled>Tìm gia đình của một giáo dân</button>
            <button type="button" disabled>Tìm và thay thế</button>
            <hr />
            <button type="button" disabled>Tùy chọn</button>
          </div>
          <div className="menu-pop" id="m3" hidden={menuMo !== 'm3'}>
            <button type="button" disabled>Kiểm tra phiên bản mới</button>
            <button type="button" disabled>Hướng dẫn sử dụng &nbsp;<span className="muted">F1</span></button>
            <hr />
            <button type="button" disabled>Thông tin phần mềm</button>
          </div>
        </div>
      </header>

      <div className="body">
        <SideNav dangChonId={dangChonNav} onNavigate={onNavigate} laQuanTri={laQuanTri} />

        <main className="work glass">
          {children}
        </main>
      </div>
    </div>
  )
}
