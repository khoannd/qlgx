import { describe, expect, it, vi } from 'vitest'
import { TEN_KHOA_BAU_CHU, troThanhChuKhiCoTheChoDenKhiHuy, type NguonKhoa } from './bauChu'

/**
 * `NguonKhoa` GIẢ LẬP dùng riêng cho test — mô phỏng ĐÚNG ngữ nghĩa Web Locks API thật cho đúng một
 * đối tượng khoá dùng CHUNG giữa nhiều "tab" giả (nhiều lần gọi `request` trong CÙNG tiến trình
 * Node), vì jsdom/Vitest không có polyfill chuẩn cho `navigator.locks` (xem brief Task 5).
 *
 * Ba điểm ngữ nghĩa PHẢI đúng, sao chép từ hành vi thật của Web Locks API (không phải suy đoán):
 * 1. Hàng đợi FIFO theo TÊN khoá — mỗi tên khoá có một hàng đợi riêng, chỉ một bên "đang giữ" tại
 *    một thời điểm; bên tới sau xếp hàng, được gọi đúng thứ tự khi bên trước nhả.
 * 2. Khoá chỉ được nhả khi `xuLy()` (đại diện `khiLaChu`) RESOLVE hoặc REJECT — không có gì khác
 *    (kể cả `signal` abort) tự động buộc nó dừng: một khi đã "đang giữ", abort không tự nhả khoá,
 *    chính `xuLy()` phải tự biết dừng khi thấy `signal.aborted`.
 * 3. `signal` CHỈ có tác dụng huỷ khi request còn đang XẾP HÀNG (chưa được gọi `xuLy`) — giống thật:
 *    MDN ghi rõ "the request is dropped if it was not already granted". Abort một request ĐANG GIỮ
 *    khoá không có hiệu lực gì ở tầng `NguonKhoa` (đúng hành vi trình duyệt thật).
 */
function taoNguonKhoaGiaLap(): NguonKhoa {
  type TrangThaiKhoa = { dangGiu: boolean; hangDoi: Array<() => void> }
  const theoTen = new Map<string, TrangThaiKhoa>()

  function layTrangThai(tenKhoa: string): TrangThaiKhoa {
    let tt = theoTen.get(tenKhoa)
    if (!tt) {
      tt = { dangGiu: false, hangDoi: [] }
      theoTen.set(tenKhoa, tt)
    }
    return tt
  }

  function taoLoiHuy(): Error {
    const loi = new Error('The request was aborted.')
    loi.name = 'AbortError'
    return loi
  }

  return {
    request<T>(tenKhoa: string, tuyChon: { signal: AbortSignal }, xuLy: () => Promise<T>): Promise<T> {
      const tt = layTrangThai(tenKhoa)
      const { signal } = tuyChon

      return new Promise<T>((resolve, reject) => {
        // true tu luc tao cho toi khi duoc goi `chay()` — dung de phan biet "con xep hang" (abort
        // phai loai khoi hang doi + reject ngay) voi "da dang giu khoa that" (abort KHONG lam gi,
        // dung ngu nghia that cua Web Locks).
        let conXepHang = true

        function nhaChoBenTiepTheo(): void {
          tt.dangGiu = false
          const ke = tt.hangDoi.shift()
          if (ke) ke()
        }

        function chay(): void {
          conXepHang = false
          tt.dangGiu = true
          // Bọc bằng Promise.resolve().then(...) để `xuLy()` luôn chạy ở một microtask MỚI, giống
          // Web Locks thật không bao giờ gọi callback đồng bộ ngay trong lệnh gọi `request()`.
          Promise.resolve()
            .then(() => xuLy())
            .then(
              (ketQua) => {
                nhaChoBenTiepTheo()
                resolve(ketQua)
              },
              (loi: unknown) => {
                nhaChoBenTiepTheo()
                reject(loi)
              },
            )
        }

        function huyKhiConXepHang(): void {
          if (!conXepHang) return // da duoc goi xuLy (dang giu khoa that) - abort khong co hieu luc
          conXepHang = false
          const vt = tt.hangDoi.indexOf(chay)
          if (vt !== -1) tt.hangDoi.splice(vt, 1)
          reject(taoLoiHuy())
        }

        if (signal.aborted) {
          huyKhiConXepHang()
          return
        }
        signal.addEventListener('abort', huyKhiConXepHang, { once: true })

        if (!tt.dangGiu) chay()
        else tt.hangDoi.push(chay)
      })
    },
  }
}

/** Promise "mở" — giữ `khiLaChu` đang chạy (giữ khoá) cho tới khi test chủ động gọi `phanGiai`. */
function taoHoanTatThuCong(): { hua: Promise<void>; phanGiai: () => void } {
  let phanGiai!: () => void
  const hua = new Promise<void>((res) => {
    phanGiai = res
  })
  return { hua, phanGiai }
}

describe('troThanhChuKhiCoTheChoDenKhiHuy', () => {
  it('chi mot tab la chu tai mot thoi diem; tab kia cho toi khi tab dau nha khoa', async () => {
    const nguon = taoNguonKhoaGiaLap()
    const dieuKhienA = new AbortController()
    const dieuKhienB = new AbortController()

    const congViecA = taoHoanTatThuCong()
    const khiLaChuA = vi.fn(async () => {
      await congViecA.hua
    })
    // Ghi lai gia tri `laChu()` NGAY TAI THOI DIEM khiMatChu duoc goi — phai la `false` roi (khong
    // duoc phep dao thu tu: dat co truoc, goi khiMatChu sau, xem chu thich trong bauChu.ts). Neu cai
    // dat dao nguoc thu tu (goi khiMatChu TRUOC khi dat lai co), gia tri ghi lai o day se la `true`
    // sai lech — chinh nhanh quyet dinh nay la muc tieu mutation testing #4 trong bao cao.
    let laChuLucMatChuA: boolean | null = null
    const khiMatChuA = vi.fn(() => {
      laChuLucMatChuA = tabA.laChu()
    })

    // Cung nhu tab A, khiLaChuB phai duoc GIU MO bang mot promise thu cong — neu de no tra ve NGAY
    // (vi du `async () => {}`), khoi finally ben trong se nha khoa ngay lap tuc chinh trong microtask
    // dau tien no chay, khien phep thu KHONG kip quan sat duoc trang thai "B dang la chu" (rang buoc
    // gia tao, khong phan anh loi that cua ham dang kiem thu).
    const congViecB = taoHoanTatThuCong()
    const khiLaChuB = vi.fn(async () => {
      await congViecB.hua
    })
    const khiMatChuB = vi.fn()

    const tabA = troThanhChuKhiCoTheChoDenKhiHuy(khiLaChuA, khiMatChuA, dieuKhienA.signal, TEN_KHOA_BAU_CHU, nguon)
    const tabB = troThanhChuKhiCoTheChoDenKhiHuy(khiLaChuB, khiMatChuB, dieuKhienB.signal, TEN_KHOA_BAU_CHU, nguon)

    // Ham tra ve NGAY, khong doi gianh duoc khoa — nhung ngay sau vai microtask, dung mot tab phai
    // da la chu (Web Locks giai khoa cho tab den truoc trong hang doi FIFO khi ca hai gan nhu dong
    // thoi tranh mot khoa RONG).
    await vi.waitFor(() => expect(khiLaChuA).toHaveBeenCalledTimes(1))

    expect(tabA.laChu()).toBe(true)
    expect(tabB.laChu()).toBe(false)
    expect(khiLaChuB).not.toHaveBeenCalled() // tab B con xep hang, chua duoc goi

    // Cho vai vong microtask nua de chac chan tab B THUC SU khong tu nhien duoc goi trong luc cho.
    await Promise.resolve()
    await Promise.resolve()
    expect(tabB.laChu()).toBe(false)
    expect(khiLaChuB).not.toHaveBeenCalled()

    // Tab A hoan tat cong viec (nha khoa binh thuong, khong phai do huy).
    congViecA.phanGiai()
    await vi.waitFor(() => expect(khiLaChuB).toHaveBeenCalledTimes(1))

    expect(tabA.laChu()).toBe(false)
    expect(khiMatChuA).toHaveBeenCalledTimes(1)
    expect(laChuLucMatChuA).toBe(false) // khong duoc "lac hau mot nhip" - phai la false NGAY luc goi
    expect(tabB.laChu()).toBe(true)

    congViecB.phanGiai()
    await vi.waitFor(() => expect(khiMatChuB).toHaveBeenCalledTimes(1))
  })

  it('huy tab dang la chu: khiMatChu duoc goi, VA tab dang cho gio duoc goi khiLaChu (chuyen giao vai tro chu)', async () => {
    const nguon = taoNguonKhoaGiaLap()
    const dieuKhienA = new AbortController()
    const dieuKhienB = new AbortController()

    // khiLaChu cua tab A: mo phong mot vong dong bo dai, tu ket thuc (khong loi) khi thay signal bi
    // huy — dung theo dung mo ta trong brief: "callback giu khoa cho toi khi resolve/reject/abort".
    const khiLaChuA = vi.fn((tinHieuHuy: AbortSignal) => {
      return new Promise<void>((resolve) => {
        tinHieuHuy.addEventListener('abort', () => resolve(), { once: true })
      })
    })
    const khiMatChuA = vi.fn()

    // Cung ly do nhu tab B o phep thu tren: giu khoa mo bang mot promise thu cong de kip quan sat
    // trang thai "dang la chu" truoc khi no tu nha khoa.
    const congViecB = taoHoanTatThuCong()
    const khiLaChuB = vi.fn(async () => {
      await congViecB.hua
    })
    const khiMatChuB = vi.fn()

    const tabA = troThanhChuKhiCoTheChoDenKhiHuy(khiLaChuA, khiMatChuA, dieuKhienA.signal, TEN_KHOA_BAU_CHU, nguon)
    const tabB = troThanhChuKhiCoTheChoDenKhiHuy(khiLaChuB, khiMatChuB, dieuKhienB.signal, TEN_KHOA_BAU_CHU, nguon)

    await vi.waitFor(() => expect(khiLaChuA).toHaveBeenCalledTimes(1))
    expect(tabA.laChu()).toBe(true)
    expect(tabB.laChu()).toBe(false)

    dieuKhienA.abort()

    await vi.waitFor(() => expect(khiMatChuA).toHaveBeenCalledTimes(1))
    expect(tabA.laChu()).toBe(false)

    await vi.waitFor(() => expect(khiLaChuB).toHaveBeenCalledTimes(1))
    expect(tabB.laChu()).toBe(true)
    expect(khiMatChuB).not.toHaveBeenCalled() // B chua he mat khoa - dang la chu

    congViecB.phanGiai()
    await vi.waitFor(() => expect(khiMatChuB).toHaveBeenCalledTimes(1))
  })

  it('huy tab con dang XEP HANG (chua bao gio la chu): khong bao gio goi khiLaChu, khong goi khiMatChu', async () => {
    const nguon = taoNguonKhoaGiaLap()
    const dieuKhienA = new AbortController()
    const dieuKhienB = new AbortController()

    const congViecA = taoHoanTatThuCong()
    const khiLaChuA = vi.fn(async () => {
      await congViecA.hua
    })
    const khiMatChuA = vi.fn()

    const khiLaChuB = vi.fn(async () => {})
    const khiMatChuB = vi.fn()

    const tabA = troThanhChuKhiCoTheChoDenKhiHuy(khiLaChuA, khiMatChuA, dieuKhienA.signal, TEN_KHOA_BAU_CHU, nguon)
    troThanhChuKhiCoTheChoDenKhiHuy(khiLaChuB, khiMatChuB, dieuKhienB.signal, TEN_KHOA_BAU_CHU, nguon)

    await vi.waitFor(() => expect(khiLaChuA).toHaveBeenCalledTimes(1))
    expect(tabA.laChu()).toBe(true)

    // B con dang xep hang (chua bao gio duoc goi khiLaChu) - huy tin hieu cua B luc nay.
    dieuKhienB.abort()
    await Promise.resolve()
    await Promise.resolve()

    expect(khiLaChuB).not.toHaveBeenCalled()
    expect(khiMatChuB).not.toHaveBeenCalled() // B chua he la chu, khong co gi de "mat"

    // Tab A van tiep tuc la chu binh thuong, khong bi anh huong boi viec B huy trong luc cho.
    expect(tabA.laChu()).toBe(true)
    congViecA.phanGiai()
    await vi.waitFor(() => expect(khiMatChuA).toHaveBeenCalledTimes(1))
  })

  it('hang doi la FIFO thuc su voi BA tab - khong chi "mot doi mot": B roi C phai duoc goi DUNG thu tu da xep hang, khong dao', async () => {
    const nguon = taoNguonKhoaGiaLap()
    const dieuKhienA = new AbortController()
    const dieuKhienB = new AbortController()
    const dieuKhienC = new AbortController()
    const thuTuDaLaChu: string[] = []

    const congViecA = taoHoanTatThuCong()
    const congViecB = taoHoanTatThuCong()
    const congViecC = taoHoanTatThuCong()

    const khiLaChuA = vi.fn(async () => {
      thuTuDaLaChu.push('A')
      await congViecA.hua
    })
    const khiLaChuB = vi.fn(async () => {
      thuTuDaLaChu.push('B')
      await congViecB.hua
    })
    const khiLaChuC = vi.fn(async () => {
      thuTuDaLaChu.push('C')
      await congViecC.hua
    })

    troThanhChuKhiCoTheChoDenKhiHuy(khiLaChuA, vi.fn(), dieuKhienA.signal, TEN_KHOA_BAU_CHU, nguon)
    await vi.waitFor(() => expect(khiLaChuA).toHaveBeenCalledTimes(1))

    // B va C xep hang SAU khi A da la chu, theo DUNG thu tu B truoc, C sau.
    troThanhChuKhiCoTheChoDenKhiHuy(khiLaChuB, vi.fn(), dieuKhienB.signal, TEN_KHOA_BAU_CHU, nguon)
    troThanhChuKhiCoTheChoDenKhiHuy(khiLaChuC, vi.fn(), dieuKhienC.signal, TEN_KHOA_BAU_CHU, nguon)
    await Promise.resolve()
    await Promise.resolve()
    expect(khiLaChuB).not.toHaveBeenCalled()
    expect(khiLaChuC).not.toHaveBeenCalled()

    congViecA.phanGiai()
    await vi.waitFor(() => expect(khiLaChuB).toHaveBeenCalledTimes(1))
    expect(khiLaChuC).not.toHaveBeenCalled() // C phai doi B, KHONG duoc vuot len truoc

    congViecB.phanGiai()
    await vi.waitFor(() => expect(khiLaChuC).toHaveBeenCalledTimes(1))

    congViecC.phanGiai()
    await vi.waitFor(() => expect(thuTuDaLaChu).toEqual(['A', 'B', 'C']))
  })
})
