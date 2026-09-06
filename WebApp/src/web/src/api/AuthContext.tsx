import { createContext, useCallback, useContext, useEffect, useMemo, useState, type ReactNode } from 'react'
import { api } from './client'
import { authStore } from './authStore'

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
  dangXuat: () => void
}

const AuthContext = createContext<AuthContextValue | null>(null)

export function AuthProvider({ children }: { children: ReactNode }) {
  const [dangKiemTraPhien, setDangKiemTraPhien] = useState(true)
  const [nguoiDung, setNguoiDung] = useState<NguoiDungHienTai | null>(null)

  const dangXuat = useCallback(() => {
    authStore.xoaToken()
    setNguoiDung(null)
  }, [])

  useEffect(() => {
    authStore.dangKy401(dangXuat)
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
