import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { AuthProvider, useAuth } from './AuthContext'
import { api } from './client'
import { authStore } from './authStore'

vi.mock('./client', () => ({
  api: { auth: { dangNhap: vi.fn(), toi: vi.fn() } },
}))

/** Component thăm dò tối giản — hiện trạng thái đăng nhập và các nút gọi dangNhap/dangXuat,
 * để test được useAuth() mà không phải dựng lại toàn bộ App.tsx (đã có màn hình đăng nhập,
 * gọi API danh sách…, không liên quan tới điều đang kiểm ở đây). */
function ThamDo() {
  const { dangKiemTraPhien, nguoiDung, dangNhap, dangXuat } = useAuth()
  if (dangKiemTraPhien) return <div>dang-kiem-tra</div>
  return (
    <div>
      <div>{nguoiDung ? `da-dang-nhap:${nguoiDung.tenTaiKhoan}` : 'chua-dang-nhap'}</div>
      <button onClick={() => dangNhap('vanphong', 'matkhau')}>dang-nhap</button>
      <button onClick={dangXuat}>dang-xuat</button>
    </div>
  )
}

describe('AuthContext', () => {
  afterEach(() => {
    authStore.xoaToken()
    authStore.huy401()
    localStorage.clear()
    vi.clearAllMocks()
  })

  it('chua co token thi bao chua dang nhap ngay, khong goi api.auth.toi', async () => {
    render(<AuthProvider><ThamDo /></AuthProvider>)

    expect(await screen.findByText('chua-dang-nhap')).toBeDefined()
    expect(api.auth.toi).not.toHaveBeenCalled()
  })

  it('co token cu hop le thi tu dong vao thang trang thai da dang nhap', async () => {
    authStore.datToken('token-cu')
    vi.mocked(api.auth.toi).mockResolvedValue({
      tenTaiKhoan: 'vanphong', hoTen: 'Van Phong', loaiTaiKhoan: 0, giaoXuId: 'x',
    })

    render(<AuthProvider><ThamDo /></AuthProvider>)

    expect(await screen.findByText('da-dang-nhap:vanphong')).toBeDefined()
  })

  it('token cu khong con hop le thi xoa token va coi nhu chua dang nhap', async () => {
    authStore.datToken('token-het-han')
    vi.mocked(api.auth.toi).mockRejectedValue(new Error('401'))

    render(<AuthProvider><ThamDo /></AuthProvider>)

    expect(await screen.findByText('chua-dang-nhap')).toBeDefined()
    expect(authStore.layToken()).toBeNull()
  })

  it('dangNhap thanh cong luu token va cap nhat nguoiDung', async () => {
    vi.mocked(api.auth.dangNhap).mockResolvedValue({
      token: 'token-moi', hetHanSau: 28800,
      nguoiDung: { id: '1', tenTaiKhoan: 'vanphong', hoTen: 'Van Phong', loaiTaiKhoan: 0, giaoXuId: 'x' },
    })
    render(<AuthProvider><ThamDo /></AuthProvider>)
    await screen.findByText('chua-dang-nhap')

    await userEvent.click(screen.getByText('dang-nhap'))

    await waitFor(() => expect(screen.getByText('da-dang-nhap:vanphong')).toBeDefined())
    expect(authStore.layToken()).toBe('token-moi')
  })

  it('dangXuat xoa token va tro ve chua dang nhap', async () => {
    vi.mocked(api.auth.dangNhap).mockResolvedValue({
      token: 'token-moi', hetHanSau: 28800,
      nguoiDung: { id: '1', tenTaiKhoan: 'vanphong', hoTen: null, loaiTaiKhoan: 1, giaoXuId: 'x' },
    })
    render(<AuthProvider><ThamDo /></AuthProvider>)
    await screen.findByText('chua-dang-nhap')
    await userEvent.click(screen.getByText('dang-nhap'))
    await screen.findByText('da-dang-nhap:vanphong')

    await userEvent.click(screen.getByText('dang-xuat'))

    expect(await screen.findByText('chua-dang-nhap')).toBeDefined()
    expect(authStore.layToken()).toBeNull()
  })

  it('goi baoHet401 (tu goi() khi may chu tra 401) cung tu dong dang xuat', async () => {
    vi.mocked(api.auth.dangNhap).mockResolvedValue({
      token: 'token-moi', hetHanSau: 28800,
      nguoiDung: { id: '1', tenTaiKhoan: 'vanphong', hoTen: null, loaiTaiKhoan: 1, giaoXuId: 'x' },
    })
    render(<AuthProvider><ThamDo /></AuthProvider>)
    await screen.findByText('chua-dang-nhap')
    await userEvent.click(screen.getByText('dang-nhap'))
    await screen.findByText('da-dang-nhap:vanphong')

    authStore.baoHet401()

    expect(await screen.findByText('chua-dang-nhap')).toBeDefined()
  })
})
