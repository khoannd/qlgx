import { afterEach, describe, expect, it, vi } from 'vitest'
import {
  PHIEN_BAN_BAN_NHAP, banNhapKhoa, docBanNhap, luuBanNhap, xoaBanNhap,
  xoaTatCaBanNhapCuaTaiKhoan,
} from './banNhap'

describe('banNhap (bản nháp ngoại tuyến — Task 16)', () => {
  afterEach(() => {
    localStorage.clear()
    vi.restoreAllMocks()
  })

  it('luu roi doc lai dung du lieu va khong lam mat gi', () => {
    const khoa = banNhapKhoa('giaoDan', 'p1', 'vanphong')
    luuBanNhap(khoa, 'vanphong', { hoTen: 'Nguyễn Văn A' })

    const ket = docBanNhap<{ hoTen: string }>(khoa, 'vanphong')

    expect(ket?.duLieu).toEqual({ hoTen: 'Nguyễn Văn A' })
    expect(ket?.thoiDiem).toBeTruthy()
  })

  it('chua co ban nhap nao thi tra ve null', () => {
    const khoa = banNhapKhoa('giaoDan', 'p1', 'vanphong')
    expect(docBanNhap(khoa, 'vanphong')).toBeNull()
  })

  // --- Điều 1: khoá theo tài khoản, không lẫn giữa hai người dùng chung máy ---

  it('ban nhap cua tai khoan A khong lo sang tai khoan B dung chung khoa loai form + id', () => {
    const khoaA = banNhapKhoa('giaoDan', 'p1', 'nguoiA')
    const khoaB = banNhapKhoa('giaoDan', 'p1', 'nguoiB')
    expect(khoaA).not.toBe(khoaB)

    luuBanNhap(khoaA, 'nguoiA', { hoTen: 'Của người A' })

    // nguoiB không đọc được (kể cả nếu đoán đúng khoá của A — nhưng khoá đã khác nhau ở trên).
    expect(docBanNhap(khoaA, 'nguoiB')).toBeNull()
    // nguoiA vẫn đọc được bình thường.
    expect(docBanNhap<{ hoTen: string }>(khoaA, 'nguoiA')?.duLieu).toEqual({ hoTen: 'Của người A' })
  })

  it('doc ban nhap khong khop tai khoan thi KHONG xoa (con nguyen cho chu nhan that)', () => {
    const khoa = banNhapKhoa('giaoDan', 'p1', 'nguoiA')
    luuBanNhap(khoa, 'nguoiA', { x: 1 })

    expect(docBanNhap(khoa, 'ke-khac')).toBeNull()
    expect(docBanNhap<{ x: number }>(khoa, 'nguoiA')?.duLieu).toEqual({ x: 1 })
  })

  it('xoaTatCaBanNhapCuaTaiKhoan chi xoa dung nhung khoa cua tai khoan do', () => {
    const khoaA1 = banNhapKhoa('giaoDan', 'p1', 'nguoiA')
    const khoaA2 = banNhapKhoa('giaDinh', 'g1', 'nguoiA')
    const khoaB = banNhapKhoa('giaoDan', 'p1', 'nguoiB')
    luuBanNhap(khoaA1, 'nguoiA', { a: 1 })
    luuBanNhap(khoaA2, 'nguoiA', { a: 2 })
    luuBanNhap(khoaB, 'nguoiB', { b: 1 })

    xoaTatCaBanNhapCuaTaiKhoan('nguoiA')

    expect(localStorage.getItem(khoaA1)).toBeNull()
    expect(localStorage.getItem(khoaA2)).toBeNull()
    expect(docBanNhap<{ b: number }>(khoaB, 'nguoiB')?.duLieu).toEqual({ b: 1 })
  })

  // --- Điều 2: localStorage có thể ném lỗi (chế độ riêng tư…) — ứng dụng vẫn chạy được ---

  it('localStorage.setItem nem loi thi luuBanNhap khong nem lai (nuot loi, ung dung van chay)', () => {
    vi.spyOn(Storage.prototype, 'setItem').mockImplementation(() => {
      throw new DOMException('bi chan')
    })
    expect(() => luuBanNhap('khoa-bat-ky', 'ai-do', { a: 1 })).not.toThrow()
  })

  it('localStorage.getItem nem loi thi docBanNhap tra ve null thay vi nem loi', () => {
    vi.spyOn(Storage.prototype, 'getItem').mockImplementation(() => {
      throw new DOMException('bi chan')
    })
    expect(docBanNhap('khoa-bat-ky', 'ai-do')).toBeNull()
  })

  it('localStorage.removeItem nem loi thi xoaBanNhap khong nem lai', () => {
    vi.spyOn(Storage.prototype, 'removeItem').mockImplementation(() => {
      throw new DOMException('bi chan')
    })
    expect(() => xoaBanNhap('khoa-bat-ky')).not.toThrow()
  })

  it('du lieu trong localStorage khong phai JSON hop le thi doc tra ve null, khong sap', () => {
    const khoa = banNhapKhoa('giaoDan', 'p1', 'vanphong')
    localStorage.setItem(khoa, '{ khong phai json')
    expect(docBanNhap(khoa, 'vanphong')).toBeNull()
  })

  // --- Điều 3: bản nháp lệch phiên bản bị bỏ qua an toàn ---

  it('ban nhap phien ban cu bi bo qua (tra ve null) VA duoc xoa luon', () => {
    const khoa = banNhapKhoa('giaoDan', 'p1', 'vanphong')
    localStorage.setItem(khoa, JSON.stringify({
      phienBan: PHIEN_BAN_BAN_NHAP - 1, tenTaiKhoan: 'vanphong',
      thoiDiem: new Date().toISOString(), duLieu: { hoTen: 'Cũ' },
    }))

    expect(docBanNhap(khoa, 'vanphong')).toBeNull()
    expect(localStorage.getItem(khoa)).toBeNull()
  })

  it('xoaBanNhap xoa dung mot khoa, khong dung cham khoa khac', () => {
    const khoa1 = banNhapKhoa('giaoDan', 'p1', 'vanphong')
    const khoa2 = banNhapKhoa('giaoDan', 'p2', 'vanphong')
    luuBanNhap(khoa1, 'vanphong', { a: 1 })
    luuBanNhap(khoa2, 'vanphong', { a: 2 })

    xoaBanNhap(khoa1)

    expect(docBanNhap(khoa1, 'vanphong')).toBeNull()
    expect(docBanNhap<{ a: number }>(khoa2, 'vanphong')?.duLieu).toEqual({ a: 2 })
  })

  it('banNhapKhoa cho id=null van la mot khoa on dinh, doc ghi binh thuong', () => {
    const khoaMoi = banNhapKhoa('giaoDan', null, 'vanphong')
    luuBanNhap(khoaMoi, 'vanphong', { a: 1 })
    expect(docBanNhap<{ a: number }>(khoaMoi, 'vanphong')?.duLieu).toEqual({ a: 1 })
    expect(banNhapKhoa('giaoDan', null, 'vanphong')).toBe(khoaMoi) // gọi lại cho cùng tham số ra cùng khoá
  })

  it('banNhapKhoa phan biet loai form va id khac nhau', () => {
    expect(banNhapKhoa('giaoDan', 'p1', 'vp')).not.toBe(banNhapKhoa('giaDinh', 'p1', 'vp'))
    expect(banNhapKhoa('giaoDan', 'p1', 'vp')).not.toBe(banNhapKhoa('giaoDan', 'p2', 'vp'))
  })
})
