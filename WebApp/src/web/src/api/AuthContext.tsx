import { createContext, useCallback, useContext, useEffect, useMemo, useRef, useState, type ReactNode } from 'react'
import { api, ChuaDangNhap } from './client'
import { authStore } from './authStore'
import { xoaTatCaBanNhapCuaTaiKhoan } from '../lib/banNhap'
import { dungOffline, layTrangThaiOffline } from '../dongbo/khoiDongOffline'
import { docHangCho } from '../kho/hangCho'

type NguoiDungHienTai = {
  tenTaiKhoan: string
  hoTen: string | null
  loaiTaiKhoan: number | null
  /** Tên giáo xứ thật của tài khoản đang đăng nhập — hiện ở thanh trên (AppShell), thay cho
   * chữ viết cứng "Giáo xứ Thánh Tâm" trước đây (can-review-sau.md mục 32). `null` chỉ trong
   * lúc dữ liệu chưa kịp tải (không nên xảy ra vì backend luôn trả kèm ngay từ lúc đăng nhập). */
  tenGiaoXu: string | null
  /** Khoá (không phải tên) của giáo xứ đang đăng nhập — dùng để TÁCH gợi ý nhập liệu lưu ở
   * `localStorage` theo từng giáo xứ (xem `lib/goiYNhapLieu.ts` và can-review-sau.md mục 50):
   * máy chủ phục vụ nhiều giáo xứ, một trình duyệt dùng chung bởi hai người ở hai giáo xứ khác
   * nhau KHÔNG được thấy gợi ý lẫn nhau. `null` chỉ trong lúc dữ liệu chưa kịp tải, cùng lý do
   * `tenGiaoXu`. */
  giaoXuId: string | null
}

// L1 (fix round Task 11): cache "ai vừa đăng nhập" ở localStorage, riêng khỏi token — dùng để vẫn
// cho vào app (ở trạng thái "biết ai đang dùng") khi tải lại trang lúc MẤT MẠNG, thay vì đá về màn
// hình đăng nhập oan chỉ vì không gọi được /api/auth/toi (xem effect kiểm tra phiên cũ bên dưới).
// Bọc try/catch cùng khuôn `authStore.ts` — localStorage có thể bị chặn (chế độ riêng tư nghiêm
// ngặt), khi đó cache chỉ mất tác dụng, không được làm hỏng luồng chính.
const KHOA_CACHE = 'qlgx.nguoiDungCache'

function docCacheNguoiDung(): NguoiDungHienTai | null {
  try {
    const tho = localStorage.getItem(KHOA_CACHE)
    return tho ? (JSON.parse(tho) as NguoiDungHienTai) : null
  } catch {
    return null
  }
}

function ghiCacheNguoiDung(nd: NguoiDungHienTai) {
  try {
    localStorage.setItem(KHOA_CACHE, JSON.stringify(nd))
  } catch {
    // F5 (fix round Task 11 - round 2): nếu ghi thất bại (vd. quota đầy) NGAY SAU KHI đăng nhập
    // bằng tài khoản MỚI (authStore.datToken đã đổi token trước đó), cache CŨ (của tài khoản
    // trước) sẽ còn nguyên trong khi token đã đổi — tải lại trang lúc mất mạng sau đó sẽ hiện
    // NHẦM danh tính/giaoXuId của tài khoản CŨ. Xoá hẳn cache thay vì im lặng bỏ qua — thà không
    // có cache còn hơn có cache sai người.
    xoaCacheNguoiDung()
  }
}

function xoaCacheNguoiDung() {
  try {
    localStorage.removeItem(KHOA_CACHE)
  } catch {
    // Xem ghi chú ở ghiCacheNguoiDung.
  }
}

/**
 * I4 (fix round cuối) — lớp CẢNH BÁO cục bộ: hàng chờ còn dòng do một tài khoản KHÁC tạo ra mà
 * người đang đăng xuất không phải người đó. Không chặn đăng xuất (spec không đòi chặn cứng), chỉ
 * không được IM LẶNG: người hỗ trợ từ xa cần manh mối này khi đi tìm "vì sao dữ liệu của cha X
 * lại được gửi lên dưới tên sơ Y".
 *
 * KHÔNG `await` (và `dangXuat` KHÔNG được thành `async` — mọi nơi gọi `dangXuat()` đều không
 * `await`): bắn-rồi-quên. Gọi `docHangCho(kho)` NGAY (đồng bộ) trước `dungOffline()` là đủ an
 * toàn — `docHangCho` mở giao dịch IndexedDB ngay tại lời gọi, và `IDBDatabase.close()` chờ các
 * giao dịch đang treo chạy xong mới thật sự đóng.
 */
function canhBaoHangChoKhacTaiKhoan(tenTaiKhoanDangThoat: string | null) {
  const tt = layTrangThaiOffline()
  if (!tt || !tenTaiKhoanDangThoat) return
  docHangCho(tt.kho)
    .then((ds) => {
      const dongLa = ds.filter((d) => typeof d.taiKhoanId === 'string' && d.taiKhoanId !== tenTaiKhoanDangThoat)
      if (dongLa.length === 0) return
      const cacTaiKhoan = [...new Set(dongLa.map((d) => String(d.taiKhoanId)))]
      console.warn(
        `Dang xuat "${tenTaiKhoanDangThoat}" trong khi hang cho con ${dongLa.length} viec chua gui ` +
          `do tai khoan KHAC tao ra (${cacTaiKhoan.join(', ')}). Nhung viec nay van nam trong may ` +
          'va se duoc gui len o lan dang nhap sau — khong mat du lieu, nhung nguoi gui len se khong ' +
          'phai nguoi da tao ra chung.',
      )
    })
    .catch((loi) => {
      console.error('Khong doc duoc hang cho de kiem tra tai khoan luc dang xuat', loi)
    })
}

type AuthContextValue = {
  /** null = chưa xác định xong (đang kiểm token cũ lúc tải trang) — hiện màn hình chờ, KHÔNG
   * được hiện màn hình đăng nhập nhầm rồi lại nhảy sang đã đăng nhập (giật màn hình). */
  dangKiemTraPhien: boolean
  nguoiDung: NguoiDungHienTai | null
  /** `giaoXuId` chỉ cần truyền khi lượt gọi trước đó ném `CanChonGiaoXu` (tên đăng nhập
   * trùng ở nhiều giáo xứ) — xem `api/client.ts`. */
  dangNhap: (tenTaiKhoan: string, matKhau: string, giaoXuId?: string) => Promise<void>
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
    // I4 (fix round cuối): đọc hàng chờ để cảnh báo (bắn-rồi-quên, KHÔNG chặn đăng xuất) NGAY
    // TRƯỚC `dungOffline()` — xem JSDoc `canhBaoHangChoKhacTaiKhoan` ở đầu file.
    canhBaoHangChoKhacTaiKhoan(nguoiDungRef.current?.tenTaiKhoan ?? null)
    // I4 (fix round 1): dừng vòng đồng bộ + đóng kho IndexedDB TRƯỚC khi xoá token — nếu không,
    // vòng đồng bộ/kho đã mở của tài khoản/giáo xứ CŨ có thể còn hoạt động trong lúc token đã đổi
    // sang tài khoản MỚI (đăng xuất rồi đăng nhập lại bằng tài khoản khác trên cùng tab), rủi ro
    // trộn dữ liệu hai giáo xứ. Xem `dungOffline()` (`dongbo/khoiDongOffline.ts`) — chỉ ĐÓNG kho,
    // không xoá dữ liệu, lần đăng nhập sau mở lại đúng kho đó.
    dungOffline()
    if (xoaCaBanNhap && nguoiDungRef.current) xoaTatCaBanNhapCuaTaiKhoan(nguoiDungRef.current.tenTaiKhoan)
    authStore.xoaToken()
    // L1: chỉ xoá cache người dùng khi đăng xuất CHỦ ĐỘNG — giữ nguyên khi bị đăng xuất BUỘC do
    // 401 (xoaCaBanNhap === false, xem chú thích JSDoc của tham số này) vì lỡ token hết hạn đúng
    // lúc mất mạng, cache vẫn còn hữu ích để vào app ở chế độ offline cho tới khi đăng nhập lại được.
    if (xoaCaBanNhap) xoaCacheNguoiDung()
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
      .then((tt) => {
        const nd: NguoiDungHienTai = {
          tenTaiKhoan: tt.tenTaiKhoan, hoTen: tt.hoTen, loaiTaiKhoan: tt.loaiTaiKhoan, tenGiaoXu: tt.tenGiaoXu,
          giaoXuId: tt.giaoXuId,
        }
        setNguoiDung(nd)
        ghiCacheNguoiDung(nd)
      })
      .catch((loi) => {
        // L1 (CRITICAL): phải phân biệt "máy chủ TỪ CHỐI token" (401 thật, ChuaDangNhap) với
        // "không gọi được máy chủ vì mất mạng" (Error thường, client.ts's goi()) — trước đây xoá
        // token trong CẢ HAI trường hợp, nghĩa là tải lại trang lúc mất mạng mất token OAN, đá ra
        // màn hình đăng nhập, không đăng nhập lại được vì đang offline (vô hiệu hoá toàn bộ câu
        // chuyện offline-first dù hàng chờ IndexedDB vẫn còn nguyên).
        if (loi instanceof ChuaDangNhap) {
          authStore.xoaToken()
          return
        }
        // Lỗi khác (mạng, máy chủ chết) — GIỮ NGUYÊN token, và nếu có cache người dùng từ lần
        // đăng nhập/kiểm tra thành công gần nhất thì vẫn cho vào app ở chế độ "biết ai đang dùng,
        // dùng dữ liệu cache" dù chưa xác nhận lại được với máy chủ. Không có cache (phiên đầu
        // tiên trên máy, chưa từng đăng nhập thành công lúc còn mạng) thì đành chịu, giữ
        // nguoiDung = null — không có gì để hiện, giới hạn hợp lý.
        const cache = docCacheNguoiDung()
        if (cache) setNguoiDung(cache)
      })
      .finally(() => setDangKiemTraPhien(false))
  }, [])

  const dangNhap = useCallback(async (tenTaiKhoan: string, matKhau: string, giaoXuId?: string) => {
    const ketQua = await api.auth.dangNhap(tenTaiKhoan, matKhau, giaoXuId)
    authStore.datToken(ketQua.token)
    const nd: NguoiDungHienTai = {
      tenTaiKhoan: ketQua.nguoiDung.tenTaiKhoan,
      hoTen: ketQua.nguoiDung.hoTen,
      loaiTaiKhoan: ketQua.nguoiDung.loaiTaiKhoan,
      tenGiaoXu: ketQua.nguoiDung.tenGiaoXu,
      giaoXuId: ketQua.nguoiDung.giaoXuId,
    }
    setNguoiDung(nd)
    ghiCacheNguoiDung(nd)
    // Task 10 (brief mục "nối dây" #2, spec offline mục 5 — RÀNG BUỘC CỨNG): xin trình duyệt "giữ
    // bền" (persistent) bộ nhớ IndexedDB ngay sau khi đăng nhập thành công — không có bước này,
    // trình duyệt có thể tự ý dọn dữ liệu ngoại tuyến (hàng chờ, sổ đã nhận...) bất cứ lúc nào nó
    // thấy máy thiếu dung lượng, đúng thứ toàn bộ kế hoạch offline-first sinh ra để tránh.
    //
    // I1 (fix round 1): KHÔNG được `await` — ở một số trình duyệt (Firefox) `persist()` có thể bật
    // hộp thoại xin phép và treo tới khi người dùng trả lời; `await` bên trong `dangNhap` sẽ treo
    // luôn màn hình đăng nhập theo. Bắn-rồi-quên bằng `.catch()` chỉ hứng được Promise bị reject —
    // KHÔNG hứng được lỗi `navigator.storage?.persist?.()` tự NÉM ĐỒNG BỘ lúc gọi (ví dụ trình
    // duyệt/extension chặn truy cập `navigator.storage`); vì vậy vẫn cần try/catch bao ngoài. Một
    // lời từ chối/lỗi ở đây TUYỆT ĐỐI không được làm hỏng luồng đăng nhập chính, đây chỉ là một lớp
    // tăng cường, không phải điều kiện để vào app.
    try {
      void Promise.resolve(navigator.storage?.persist?.()).catch(() => {
        // Im lặng bỏ qua — xem chú thích ở trên.
      })
    } catch {
      // Im lặng bỏ qua — truy cập navigator.storage tự ném lỗi đồng bộ.
    }
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
