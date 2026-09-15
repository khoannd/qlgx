import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { AuthProvider, useAuth } from './AuthContext'
import { api, ChuaDangNhap } from './client'
import { authStore } from './authStore'
import { banNhapKhoa, docBanNhap, luuBanNhap } from '../lib/banNhap'

vi.mock('./client', () => ({
  api: { auth: { dangNhap: vi.fn(), toi: vi.fn() } },
  // L1: dung DUNG class ma AuthContext import (khong phai Error thuong) de test phan biet duoc
  // "may chu TU CHOI token" voi "loi mang" - xem client.ts's ChuaDangNhap that.
  ChuaDangNhap: class ChuaDangNhap extends Error {},
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

  // L1 (fix round Task 11): dung DUNG ChuaDangNhap (may chu TU CHOI token that su, vi du 401) -
  // truoc day test nay dung mot Error thuong, khong con dung voi hanh vi that cua client.ts's goi()
  // sau khi sua L1 (chi Error KIEU ChuaDangNhap moi lam xoa token, Error thuong = loi mang).
  it('token cu bi may chu TU CHOI (ChuaDangNhap, vi du 401 that) thi xoa token va coi nhu chua dang nhap', async () => {
    authStore.datToken('token-het-han')
    vi.mocked(api.auth.toi).mockRejectedValue(new ChuaDangNhap('Phien dang nhap da het han.'))

    render(<AuthProvider><ThamDo /></AuthProvider>)

    expect(await screen.findByText('chua-dang-nhap')).toBeDefined()
    expect(authStore.layToken()).toBeNull()
  })

  // L1 (CRITICAL, fix round Task 11): mat mang luc tai lai trang KHONG duoc xoa token oan - truoc
  // day .catch() khong phan biet duoc "may chu TU CHOI token" (ChuaDangNhap) voi "khong goi duoc may
  // chu vi mat mang" (Error thuong tu client.ts's goi()), xoa token trong CA HAI truong hop khien
  // nguoi dung mat mang bi da ra man hinh dang nhap va KHONG dang nhap lai duoc vi dang offline.
  it('mat mang luc tai lai trang, SAU KHI da tung dang nhap thanh cong -> van vao duoc app tu cache, token KHONG bi xoa', async () => {
    vi.mocked(api.auth.dangNhap).mockResolvedValue({
      token: 'token-cu-con-hieu-luc', hetHanSau: 28800,
      nguoiDung: { id: '1', tenTaiKhoan: 'vanphong', hoTen: 'Van Phong', loaiTaiKhoan: 0, giaoXuId: 'x', tenGiaoXu: 'Vo Nhiem' },
    })
    const { unmount } = render(<AuthProvider><ThamDo /></AuthProvider>)
    await screen.findByText('chua-dang-nhap')
    await userEvent.click(screen.getByText('dang-nhap'))
    await screen.findByText('da-dang-nhap:vanphong')
    unmount() // Mo phong dong tab — cache nguoiDung da duoc ghi luc dangNhap thanh cong o tren.

    // Tai lai trang: token con trong localStorage nhung /api/auth/toi khong goi duoc vi MAT MANG
    // (Error thuong, khong phai ChuaDangNhap).
    vi.mocked(api.auth.toi).mockRejectedValue(new Error('Mất kết nối mạng. Dữ liệu bạn đã nhập vẫn được giữ nguyên trên máy — hãy thử lại khi có mạng.'))
    render(<AuthProvider><ThamDo /></AuthProvider>)

    expect(await screen.findByText('da-dang-nhap:vanphong')).toBeDefined()
    expect(authStore.layToken()).toBe('token-cu-con-hieu-luc')
  })

  // L1: khong co cache nao (chua tung dang nhap thanh cong tren may nay) va mat mang luc tai trang
  // -> khong crash, khong hien app voi du lieu rong/sai, o lai trang thai chua dang nhap.
  it('mat mang luc tai lai trang, KHONG CO cache nao tu truoc -> van o trang thai chua dang nhap, khong crash', async () => {
    authStore.datToken('token-chua-tung-xac-nhan-duoc')
    vi.mocked(api.auth.toi).mockRejectedValue(new Error('Mất kết nối mạng.'))

    render(<AuthProvider><ThamDo /></AuthProvider>)

    expect(await screen.findByText('chua-dang-nhap')).toBeDefined()
    // Token GIU NGUYEN (khong phai ChuaDangNhap) — chi khong co gi de hien vi chua co cache.
    expect(authStore.layToken()).toBe('token-chua-tung-xac-nhan-duoc')
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

  // I1 (re-review fix round 1): navigator.storage.persist() co the tu NEM LOI DONG BO ngay luc goi
  // (khac voi Promise bi reject) — vi du extension/trinh duyet chan truy cap navigator.storage. Mot
  // .catch() don thuan KHONG bat duoc loi nem dong bo nay; can try/catch bao ngoai.
  it('persist() nem loi DONG BO ngay luc goi -> dangNhap van hoan tat (khong vo luong)', async () => {
    vi.mocked(api.auth.dangNhap).mockResolvedValue({
      token: 'token-moi', hetHanSau: 28800,
      nguoiDung: { id: '1', tenTaiKhoan: 'vanphong', hoTen: 'Van Phong', loaiTaiKhoan: 0, giaoXuId: 'x', tenGiaoXu: 'Vo Nhiem' },
    })
    const persistNemDongBo = vi.fn(() => {
      throw new Error('trinh duyet chan truy cap navigator.storage')
    })
    vi.stubGlobal('navigator', { ...navigator, storage: { persist: persistNemDongBo } })

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
