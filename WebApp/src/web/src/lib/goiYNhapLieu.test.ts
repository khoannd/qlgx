import { afterEach, describe, expect, it, vi } from 'vitest'
import { ghiNhanDaDung, gopGoiY, layGoiY } from './goiYNhapLieu'

describe('goiYNhapLieu (gợi ý nhập liệu theo tần suất — localStorage)', () => {
  afterEach(() => {
    localStorage.clear()
    vi.restoreAllMocks()
  })

  // --- Đếm tần suất, xếp hạng ---------------------------------------------------------------

  it('xep gia tri dung nhieu lan nhat len dau', () => {
    ghiNhanDaDung('gx1', 'noiSinh', 'Hà Nội')
    ghiNhanDaDung('gx1', 'noiSinh', 'Sài Gòn')
    ghiNhanDaDung('gx1', 'noiSinh', 'Sài Gòn')
    ghiNhanDaDung('gx1', 'noiSinh', 'Sài Gòn')

    expect(layGoiY('gx1', 'noiSinh')).toEqual(['Sài Gòn', 'Hà Nội'])
  })

  it('dung lai gia tri da co (khong phan biet hoa/thuong) chi tang bo dem, khong tao muc moi', () => {
    ghiNhanDaDung('gx1', 'tenThanh', 'Maria')
    ghiNhanDaDung('gx1', 'tenThanh', 'MARIA')
    ghiNhanDaDung('gx1', 'tenThanh', 'maria')

    const ds = layGoiY('gx1', 'tenThanh')
    expect(ds).toHaveLength(1)
    expect(ds[0]).toBe('Maria')
  })

  it('bo qua chuoi rong/toan khoang trang', () => {
    ghiNhanDaDung('gx1', 'noiSinh', '   ')
    ghiNhanDaDung('gx1', 'noiSinh', '')
    expect(layGoiY('gx1', 'noiSinh')).toEqual([])
  })

  it('cat bot khoang trang thua truoc khi ghi', () => {
    ghiNhanDaDung('gx1', 'noiSinh', '  Đà Lạt  ')
    expect(layGoiY('gx1', 'noiSinh')).toEqual(['Đà Lạt'])
  })

  // --- Điều 1: tách theo giáo xứ ------------------------------------------------------------

  it('gia tri cua giao xu A khong lo sang giao xu B', () => {
    ghiNhanDaDung('gx-A', 'noiSinh', 'Của giáo xứ A')
    ghiNhanDaDung('gx-B', 'noiSinh', 'Của giáo xứ B')

    expect(layGoiY('gx-A', 'noiSinh')).toEqual(['Của giáo xứ A'])
    expect(layGoiY('gx-B', 'noiSinh')).toEqual(['Của giáo xứ B'])
  })

  it('cac truong khac nhau khong lan gia tri vao nhau', () => {
    ghiNhanDaDung('gx1', 'noiSinh', 'Giá trị nơi sinh')
    ghiNhanDaDung('gx1', 'noiRuaToi', 'Giá trị nơi rửa tội')

    expect(layGoiY('gx1', 'noiSinh')).toEqual(['Giá trị nơi sinh'])
    expect(layGoiY('gx1', 'noiRuaToi')).toEqual(['Giá trị nơi rửa tội'])
  })

  // --- Điều 2: giới hạn dung lượng -----------------------------------------------------------

  it('vuot qua gioi han thi loai bo gia tri it dung nhat, giu lai gia tri hay dung', () => {
    // Ghi một giá trị "hay dùng" trước, dùng nhiều lần để chắc chắn đứng đầu.
    for (let i = 0; i < 5; i += 1) ghiNhanDaDung('gx1', 'demGioiHan', 'hay-dung-nhat')
    // Rồi lấp đầy vượt giới hạn (300) bằng các giá trị chỉ dùng một lần.
    for (let i = 0; i < 305; i += 1) ghiNhanDaDung('gx1', 'demGioiHan', `gia-tri-${i}`)

    const ds = layGoiY('gx1', 'demGioiHan')
    expect(ds.length).toBeLessThanOrEqual(300)
    expect(ds).toContain('hay-dung-nhat')
    expect(ds[0]).toBe('hay-dung-nhat')
    // Giá trị được ghi sớm nhất trong lô "chỉ dùng một lần" (gia-tri-0) bị loại trước tiên.
    expect(ds).not.toContain('gia-tri-0')
  })

  // --- Điều 3: không ném lỗi khi localStorage hỏng/bị chặn ------------------------------------

  it('localStorage nem loi luc ghi thi khong lam vo ung dung (nuot loi im lang)', () => {
    const spy = vi.spyOn(Storage.prototype, 'setItem').mockImplementation(() => {
      throw new DOMException('QuotaExceededError')
    })
    expect(() => ghiNhanDaDung('gx1', 'noiSinh', 'X')).not.toThrow()
    spy.mockRestore()
  })

  it('localStorage nem loi luc doc thi tra ve mang rong, khong nem loi', () => {
    const spy = vi.spyOn(Storage.prototype, 'getItem').mockImplementation(() => {
      throw new DOMException('SecurityError')
    })
    expect(() => layGoiY('gx1', 'noiSinh')).not.toThrow()
    expect(layGoiY('gx1', 'noiSinh')).toEqual([])
    spy.mockRestore()
  })

  it('du lieu JSON hong trong localStorage khong lam vo ung dung', () => {
    localStorage.setItem('qlgx.goiy.v1.gx1.noiSinh', '{ khong phai JSON hop le')
    expect(layGoiY('gx1', 'noiSinh')).toEqual([])
  })

  // --- gopGoiY: gộp lịch sử + danh mục, lọc theo đoạn đang gõ --------------------------------

  it('gopGoiY uu tien lich su TRUOC danh muc', () => {
    ghiNhanDaDung('gx1', 'tenThanh', 'Vinh Sơn')
    const ket = gopGoiY('gx1', 'tenThanh', ['Anna', 'Maria', 'Vinh Sơn'], '')
    expect(ket[0]).toBe('Vinh Sơn') // lịch sử đứng đầu dù không phải đầu bảng chữ cái danh mục
  })

  it('gopGoiY loc theo doan dang go, khong phan biet hoa/thuong', () => {
    const ket = gopGoiY('gx1', 'tenThanh', ['Anna', 'Antôn', 'Maria'], 'an')
    expect(ket).toEqual(['Anna', 'Antôn'])
  })

  it('gopGoiY loai trung giua lich su va danh muc', () => {
    ghiNhanDaDung('gx1', 'tenThanh', 'anna')
    const ket = gopGoiY('gx1', 'tenThanh', ['Anna'], '')
    expect(ket.filter((x) => x.toLowerCase() === 'anna')).toHaveLength(1)
  })

  it('gopGoiY gioi han so luong hien thi', () => {
    const danhMuc = Array.from({ length: 20 }, (_, i) => `Tên ${i}`)
    expect(gopGoiY('gx1', 'tenThanh', danhMuc, '', 5)).toHaveLength(5)
  })

  it('gopGoiY voi giaoXuId null van tra ve duoc danh muc (chi tat lich su)', () => {
    expect(gopGoiY(null, 'tenThanh', ['Anna', 'Maria'], '')).toEqual(['Anna', 'Maria'])
  })

  it('gopGoiY khong co gi khop doan dang go thi tra ve mang rong', () => {
    ghiNhanDaDung('gx1', 'tenThanh', 'Maria')
    expect(gopGoiY('gx1', 'tenThanh', ['Anna'], 'zzz-khong-ton-tai')).toEqual([])
  })
})
