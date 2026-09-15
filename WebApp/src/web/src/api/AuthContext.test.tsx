import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { AuthProvider, useAuth } from './AuthContext'
import { api } from './client'
import { authStore } from './authStore'
import { banNhapKhoa, docBanNhap, luuBanNhap } from '../lib/banNhap'

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
      <div>{nguoiDung ? `giao-xu:${nguoiDung.tenGiaoXu}` : ''}</div>
      <button onClick={() => dangNhap('vanphong', 'matkhau')}>dang-nhap</button>
      <button onClick={() => dangXuat()}>dang-xuat</button>
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
      tenTaiKhoan: 'vanphong', hoTen: 'Van Phong', loaiTaiKhoan: 0, giaoXuId: 'x', tenGiaoXu: 'Vo Nhiem',
    })

    render(<AuthProvider><ThamDo /></AuthProvider>)

    expect(await screen.findByText('da-dang-nhap:vanphong')).toBeDefined()
    // can-review-sau.md muc 32: thanh tren (AppShell) phai hien dung ten giao xu THAT lay tu
    // /api/auth/toi, khong con viet cung "Giao xu Thanh Tam" nhu truoc.
    expect(screen.getByText('giao-xu:Vo Nhiem')).toBeDefined()
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
      nguoiDung: { id: '1', tenTaiKhoan: 'vanphong', hoTen: 'Van Phong', loaiTaiKhoan: 0, giaoXuId: 'x', tenGiaoXu: 'Vo Nhiem' },
    })
    render(<AuthProvider><ThamDo /></AuthProvider>)
    await screen.findByText('chua-dang-nhap')

    await userEvent.click(screen.getByText('dang-nhap'))

    await waitFor(() => expect(screen.getByText('da-dang-nhap:vanphong')).toBeDefined())
    expect(authStore.layToken()).toBe('token-moi')
    expect(screen.getByText('giao-xu:Vo Nhiem')).toBeDefined()
  })

  // Task 10 (brief muc "noi day" #2, spec offline muc 5): navigator.storage.persist() PHAI duoc
  // goi NGAY sau khi dang nhap thanh cong — thieu buoc nay trinh duyet co the tu y don du lieu
  // IndexedDB ngoai tuyen bat cu luc nao.
  it('dangNhap thanh cong tu goi navigator.storage.persist() de xin giu ben du lieu ngoai tuyen', async () => {
    vi.mocked(api.auth.dangNhap).mockResolvedValue({
      token: 'token-moi', hetHanSau: 28800,
      nguoiDung: { id: '1', tenTaiKhoan: 'vanphong', hoTen: 'Van Phong', loaiTaiKhoan: 0, giaoXuId: 'x', tenGiaoXu: 'Vo Nhiem' },
    })
    const persistGia = vi.fn().mockResolvedValue(true)
    vi.stubGlobal('navigator', { ...navigator, storage: { persist: persistGia } })

    render(<AuthProvider><ThamDo /></AuthProvider>)
    await screen.findByText('chua-dang-nhap')
    await userEvent.click(screen.getByText('dang-nhap'))

    await waitFor(() => expect(persistGia).toHaveBeenCalledTimes(1))
  })

  // Trinh duyet khong ho tro navigator.storage (Safari cu, mot so trinh duyet mang LAN giao xu) —
  // KHONG duoc lam hong luong dang nhap chinh du thieu API nay.
  it('trinh duyet KHONG co navigator.storage van dang nhap thanh cong binh thuong', async () => {
    vi.mocked(api.auth.dangNhap).mockResolvedValue({
      token: 'token-moi', hetHanSau: 28800,
      nguoiDung: { id: '1', tenTaiKhoan: 'vanphong', hoTen: 'Van Phong', loaiTaiKhoan: 0, giaoXuId: 'x', tenGiaoXu: 'Vo Nhiem' },
    })
    const { storage: _storage, ...navigatorKhongCoStorage } = navigator as Navigator & { storage?: unknown }
    vi.stubGlobal('navigator', navigatorKhongCoStorage)

    render(<AuthProvider><ThamDo /></AuthProvider>)
    await screen.findByText('chua-dang-nhap')
    await userEvent.click(screen.getByText('dang-nhap'))

    expect(await screen.findByText('da-dang-nhap:vanphong')).toBeDefined()
  })

  // I1 (fix round 1): mot so trinh duyet (Firefox) bat hop thoai xin phep khi goi
  // navigator.storage.persist() va treo Promise do cho toi khi nguoi dung tra loi — dangNhap KHONG
  // duoc await Promise nay, neu khong man hinh dang nhap se treo theo.
  it('persist() tra ve Promise KHONG BAO GIO resolve -> dangNhap van hoan tat (khong bi treo)', async () => {
    vi.mocked(api.auth.dangNhap).mockResolvedValue({
      token: 'token-moi', hetHanSau: 28800,
      nguoiDung: { id: '1', tenTaiKhoan: 'vanphong', hoTen: 'Van Phong', loaiTaiKhoan: 0, giaoXuId: 'x', tenGiaoXu: 'Vo Nhiem' },
    })
    const persistTreoMai = vi.fn().mockReturnValue(new Promise(() => {}))
    vi.stubGlobal('navigator', { ...navigator, storage: { persist: persistTreoMai } })

    render(<AuthProvider><ThamDo /></AuthProvider>)
    await screen.findByText('chua-dang-nhap')
    await userEvent.click(screen.getByText('dang-nhap'))

    expect(await screen.findByText('da-dang-nhap:vanphong')).toBeDefined()
  })

  it('dangXuat xoa token va tro ve chua dang nhap', async () => {
    vi.mocked(api.auth.dangNhap).mockResolvedValue({
      token: 'token-moi', hetHanSau: 28800,
      nguoiDung: { id: '1', tenTaiKhoan: 'vanphong', hoTen: null, loaiTaiKhoan: 1, giaoXuId: 'x', tenGiaoXu: 'Vo Nhiem' },
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
      nguoiDung: { id: '1', tenTaiKhoan: 'vanphong', hoTen: null, loaiTaiKhoan: 1, giaoXuId: 'x', tenGiaoXu: 'Vo Nhiem' },
    })
    render(<AuthProvider><ThamDo /></AuthProvider>)
    await screen.findByText('chua-dang-nhap')
    await userEvent.click(screen.getByText('dang-nhap'))
    await screen.findByText('da-dang-nhap:vanphong')

    authStore.baoHet401()

    expect(await screen.findByText('chua-dang-nhap')).toBeDefined()
  })

  // --- Task 16: bản nháp ngoại tuyến (lib/banNhap.ts) không được lẫn/mất khi đăng xuất ---

  it('dang xuat CHU DONG (bam nut) xoa het ban nhap cua tai khoan dang dung', async () => {
    vi.mocked(api.auth.dangNhap).mockResolvedValue({
      token: 'token-moi', hetHanSau: 28800,
      nguoiDung: { id: '1', tenTaiKhoan: 'vanphong', hoTen: null, loaiTaiKhoan: 1, giaoXuId: 'x', tenGiaoXu: 'Vo Nhiem' },
    })
    render(<AuthProvider><ThamDo /></AuthProvider>)
    await screen.findByText('chua-dang-nhap')
    await userEvent.click(screen.getByText('dang-nhap'))
    await screen.findByText('da-dang-nhap:vanphong')

    const khoa = banNhapKhoa('giaoDan', 'p1', 'vanphong')
    luuBanNhap(khoa, 'vanphong', { hoTen: 'Đang gõ dở' })

    await userEvent.click(screen.getByText('dang-xuat'))

    expect(await screen.findByText('chua-dang-nhap')).toBeDefined()
    expect(docBanNhap(khoa, 'vanphong')).toBeNull()
  })

  it('bi dang xuat BUOC vi token het han (401 luc mat mang) KHONG xoa ban nhap — dang nhap lai van khoi phuc duoc', async () => {
    vi.mocked(api.auth.dangNhap).mockResolvedValue({
      token: 'token-moi', hetHanSau: 28800,
      nguoiDung: { id: '1', tenTaiKhoan: 'vanphong', hoTen: null, loaiTaiKhoan: 1, giaoXuId: 'x', tenGiaoXu: 'Vo Nhiem' },
    })
    render(<AuthProvider><ThamDo /></AuthProvider>)
    await screen.findByText('chua-dang-nhap')
    await userEvent.click(screen.getByText('dang-nhap'))
    await screen.findByText('da-dang-nhap:vanphong')

    // Người dùng đang gõ dở form chi tiết, tự lưu nháp — ĐÚNG LÚC token 8 tiếng hết hạn khi
    // đang mất mạng (Task 14, JwtKey). goi() phát hiện 401 và gọi baoHet401() → dangXuat(false).
    const khoa = banNhapKhoa('giaoDan', 'p1', 'vanphong')
    luuBanNhap(khoa, 'vanphong', { hoTen: 'Đang gõ dở lúc hết hạn token' })

    authStore.baoHet401()
    expect(await screen.findByText('chua-dang-nhap')).toBeDefined()

    // Bản nháp còn nguyên trong lúc "chưa đăng nhập lại" — không hiện được ở UI (đúng, tránh lộ
    // dữ liệu trước khi xác thực lại) nhưng dữ liệu vẫn nằm đó chờ chủ nhân quay lại.
    expect(docBanNhap<{ hoTen: string }>(khoa, 'vanphong')?.duLieu).toEqual({
      hoTen: 'Đang gõ dở lúc hết hạn token',
    })

    // Đăng nhập lại — cùng tài khoản — bản nháp phải còn khôi phục được.
    await userEvent.click(screen.getByText('dang-nhap'))
    await screen.findByText('da-dang-nhap:vanphong')
    expect(docBanNhap<{ hoTen: string }>(khoa, 'vanphong')?.duLieu).toEqual({
      hoTen: 'Đang gõ dở lúc hết hạn token',
    })
  })
})
