import { createContext, useCallback, useContext, useEffect, useMemo, useRef, useState, type ReactNode } from 'react'
import { api } from './client'
import { authStore } from './authStore'
import { xoaTatCaBanNhapCuaTaiKhoan } from '../lib/banNhap'

type NguoiDungHienTai = {
  tenTaiKhoan: string
  hoTen: string | null
  loaiTaiKhoan: number | null
}

type AuthContextValue = {
  /** null = chưa xác định xong (đang kiểm token cũ lúc tải trang) — hiện màn hình chờ, KHÔNG
   * được hiện màn hình đăng nhập nhầm rồi lại nhảy sang đã đăng nhập (giật màn hình). */
  dangKiemTraPhien: boolean
  nguoiDung: NguoiDungHienTai | null
  dangNhap: (tenTaiKhoan: string, matKhau: string) => Promise<void>
  /** `xoaCaBanNhap` (mặc định `true`) quyết định có xoá bản nháp ngoại tuyến (`lib/banNhap.ts`)
   * của tài khoản này hay không — `true` cho hành động đăng xuất CHỦ ĐỘNG (nút "Đăng xuất"),
   * `false` khi bị đăng xuất BUỘC vì token hết hạn (401, xem `authStore.dangKy401` bên dưới):
   * mất mạng đúng lúc token hết hạn 8 tiếng là ca có thật (JWT — Task 14), người dùng đăng
   * nhập lại xong vẫn phải khôi phục được nháp, không được mất trắng vì bị đăng xuất ngoài ý
   * muốn. */
  dangXuat: (xoaCaBanNhap?: boolean) => void
}

const AuthContext = createContext<AuthContextValue | null>(null)

export function AuthProvider({ children }: { children: ReactNode }) {
  const [dangKiemTraPhien, setDangKiemTraPhien] = useState(true)
  const [nguoiDung, setNguoiDung] = useState<NguoiDungHienTai | null>(null)
  const nguoiDungRef = useRef<NguoiDungHienTai | null>(null)
  useEffect(() => { nguoiDungRef.current = nguoiDung }, [nguoiDung])

  const dangXuat = useCallback((xoaCaBanNhap = true) => {
    if (xoaCaBanNhap && nguoiDungRef.current) xoaTatCaBanNhapCuaTaiKhoan(nguoiDungRef.current.tenTaiKhoan)
    authStore.xoaToken()
    setNguoiDung(null)
  }, [])

  useEffect(() => {
    // 401 từ goi() = bị máy chủ từ chối (hết hạn/thu hồi) — KHÔNG xoá bản nháp, xem chú thích
    // ở kiểu dangXuat.
    authStore.dangKy401(() => dangXuat(false))
    return () => authStore.huy401()
  }, [dangXuat])

  useEffect(() => {
    const token = authStore.layToken()
    if (!token) {
      setDangKiemTraPhien(false)
      return
    }
    // Token có trong localStorage từ phiên trước — xác nhận nó còn hợp lệ (chưa hết hạn) và
    // lấy lại họ tên/vai trò mới nhất trước khi cho vào thẳng ứng dụng.
    api.auth.toi()
      .then((tt) => setNguoiDung({ tenTaiKhoan: tt.tenTaiKhoan, hoTen: tt.hoTen, loaiTaiKhoan: tt.loaiTaiKhoan }))
      .catch(() => { authStore.xoaToken() })
      .finally(() => setDangKiemTraPhien(false))
  }, [])

  const dangNhap = useCallback(async (tenTaiKhoan: string, matKhau: string) => {
    const ketQua = await api.auth.dangNhap(tenTaiKhoan, matKhau)
    authStore.datToken(ketQua.token)
    setNguoiDung({
      tenTaiKhoan: ketQua.nguoiDung.tenTaiKhoan,
      hoTen: ketQua.nguoiDung.hoTen,
      loaiTaiKhoan: ketQua.nguoiDung.loaiTaiKhoan,
    })
  }, [])

  const value = useMemo(
    () => ({ dangKiemTraPhien, nguoiDung, dangNhap, dangXuat }),
    [dangKiemTraPhien, nguoiDung, dangNhap, dangXuat],
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

export function useAuth(): AuthContextValue {
  const ctx = useContext(AuthContext)
  if (!ctx) throw new Error('useAuth() phải gọi bên trong <AuthProvider>')
  return ctx
}
