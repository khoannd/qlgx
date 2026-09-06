import type { ReactNode } from 'react'
import { SideNav } from './SideNav'

type Props = {
  dangChonNav: string
  onNavigate: (id: string) => void
  children: ReactNode
}

/** Khung ứng dụng: thanh trên (topbar) + điều hướng trái (SideNav) + vùng làm
 * việc bên phải (do `children` — thường là `<TabDocs>` — quyết định). Cấu
 * trúc thẻ HTML chép nguyên từ `WebApp/prototype/qlgx-prototype.html`
 * (phần `<header class="topbar">`), chỉ đổi `class` thành `className`. */
export function AppShell({ dangChonNav, onNavigate, children }: Props) {
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

        <button className="menubtn" type="button" data-menu="m1" aria-expanded="false">Hệ thống</button>
        <button className="menubtn" type="button" data-menu="m2" aria-expanded="false">Công cụ</button>
        <button className="menubtn" type="button" data-menu="m3" aria-expanded="false">Trợ giúp</button>

        <span className="demo-chip">Bản mẫu · dữ liệu minh hoạ</span>
        <span className="avatar" title="Quản trị viên: vanphong">VP</span>

        <div className="menu-pop" id="m1" hidden>
          <button type="button">Nhập dữ liệu từ cùng chương trình</button>
          <button type="button">Nhập dữ liệu từ MS Excel</button>
          <button type="button">Nhập dữ liệu từ chương trình MGC</button>
          <hr />
          <button type="button">Sao lưu dữ liệu chương trình</button>
          <button type="button">Xuất dữ liệu ra Excel</button>
          <button type="button">Khôi phục dữ liệu</button>
          <hr />
          <button type="button">Đổi mật khẩu</button>
          <button type="button">Đăng xuất</button>
        </div>
        <div className="menu-pop" id="m2" hidden>
          <button type="button">Tìm giáo dân</button>
          <button type="button">Tìm gia đình của một giáo dân</button>
          <button type="button">Tìm và thay thế</button>
          <hr />
          <button type="button">Tùy chọn</button>
        </div>
        <div className="menu-pop" id="m3" hidden>
          <button type="button">Kiểm tra phiên bản mới</button>
          <button type="button">Hướng dẫn sử dụng &nbsp;<span className="muted">F1</span></button>
          <hr />
          <button type="button">Thông tin phần mềm</button>
        </div>
      </header>

      <div className="body">
        <SideNav dangChonId={dangChonNav} onNavigate={onNavigate} />

        <main className="work glass">
          {children}
        </main>
      </div>
    </div>
  )
}
