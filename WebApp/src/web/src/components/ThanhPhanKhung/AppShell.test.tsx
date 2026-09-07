import { render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'
import { AppShell } from './AppShell'

vi.mock('../../api/AuthContext', () => ({
  useAuth: () => ({ dangXuat: vi.fn() }),
}))

describe('AppShell', () => {
  // can-review-sau.md muc 32: thanh tren truoc day viet cung "Giao xu Thanh Tam" — sai tren
  // mot may chu phuc vu nhieu giao xu. Phai hien dung ten giao xu THAT cua nguoi dang dang
  // nhap, khong con chuoi tinh nao khac.
  it('hien dung ten giao xu that cua nguoi dang nhap, khong con chuoi tinh cu', () => {
    render(
      <AppShell dangChonNav="giaoDanList" onNavigate={() => {}} nguoiDung={{
        tenTaiKhoan: 'quantri', hoTen: 'Quan Tri', loaiTaiKhoan: 0, tenGiaoXu: 'Vô Nhiễm',
      }}>
        <div>noi-dung</div>
      </AppShell>,
    )

    expect(screen.getByText('Vô Nhiễm')).toBeDefined()
    expect(screen.queryByText(/Thánh Tâm/)).toBeNull()
  })

  // Chua co chuc nang chuyen giao xu (may chu hien chi phuc vu mot giao xu cho moi tai
  // khoan) — hinh tam giac tha xuong cu gay hieu nham la co chuc nang do, da bo di.
  it('khong con hinh tam giac tha xuong goi y chuc nang chuyen giao xu chua co', () => {
    render(
      <AppShell dangChonNav="giaoDanList" onNavigate={() => {}} nguoiDung={{
        tenTaiKhoan: 'quantri', hoTen: 'Quan Tri', loaiTaiKhoan: 0, tenGiaoXu: 'Vô Nhiễm',
      }}>
        <div>noi-dung</div>
      </AppShell>,
    )

    expect(screen.queryByText('▼')).toBeNull()
  })
})
