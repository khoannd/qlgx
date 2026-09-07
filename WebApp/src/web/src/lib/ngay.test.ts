import { describe, expect, it } from 'vitest'
import {
  chuanHoaNgayThieu,
  dinhDangNgay,
  goSoVaoKhuon,
  khuonSangPhan,
  khuonTuIso,
  KHUON_NGAY_RONG,
  ngayTuHienThi,
  phanKeTiep,
  viTriGoDauTien,
  xoaLuiTrongKhuon,
} from './ngay'

describe('dinhDangNgay', () => {
  it('doi ISO yyyy-MM-dd sang dd/MM/yyyy', () => {
    expect(dinhDangNgay('2015-04-25')).toBe('25/04/2015')
    expect(dinhDangNgay('1990-03-26')).toBe('26/03/1990')
  })

  it('rong/null thi tra chuoi rong, khong sap', () => {
    expect(dinhDangNgay(null)).toBe('')
    expect(dinhDangNgay(undefined)).toBe('')
    expect(dinhDangNgay('')).toBe('')
  })

  it('du lieu loi (chi co nam, khong phai ISO) giu nguyen van, khong nem loi', () => {
    expect(dinhDangNgay('1958')).toBe('1958')
    expect(dinhDangNgay('khong ro')).toBe('khong ro')
  })
})

describe('ngayTuHienThi', () => {
  it('doi dd/MM/yyyy hop le sang ISO', () => {
    expect(ngayTuHienThi('25/04/2015')).toBe('2015-04-25')
    expect(ngayTuHienThi('01/01/2000')).toBe('2000-01-01')
  })

  it('chap nhan khong can so 0 dung dau (d/M/yyyy)', () => {
    expect(ngayTuHienThi('5/4/2015')).toBe('2015-04-05')
  })

  it('chuoi rong tra null (nghia la xoa ngay)', () => {
    expect(ngayTuHienThi('')).toBeNull()
    expect(ngayTuHienThi('   ')).toBeNull()
  })

  it('sai dinh dang (kieu My mm/dd) tra undefined, khong doan bua', () => {
    // "25/13/2015" khong phai ngay hop le du dung dung thu tu dd/mm/yyyy
    expect(ngayTuHienThi('13/25/2015')).toBeUndefined()
  })

  it('ngay khong ton tai trong thang (30/02) tra undefined', () => {
    expect(ngayTuHienThi('30/02/2020')).toBeUndefined()
  })

  it('chuoi khong dung dang thuc tra undefined', () => {
    expect(ngayTuHienThi('2015-04-25')).toBeUndefined()
    expect(ngayTuHienThi('abc')).toBeUndefined()
  })

  it('nam nhuan tinh dung 29/02', () => {
    expect(ngayTuHienThi('29/02/2020')).toBe('2020-02-29')
    expect(ngayTuHienThi('29/02/2021')).toBeUndefined()
  })
})

describe('khuonTuIso', () => {
  it('rong/null tra khuon rong', () => {
    expect(khuonTuIso(null)).toBe(KHUON_NGAY_RONG)
    expect(khuonTuIso(undefined)).toBe(KHUON_NGAY_RONG)
    expect(khuonTuIso('')).toBe(KHUON_NGAY_RONG)
  })

  it('ISO hop le doi thanh khuon du 10 ky tu', () => {
    expect(khuonTuIso('2015-04-25')).toBe('25/04/2015')
  })

  it('gia tri khong phai ISO (du lieu loi cu) tra khuon rong', () => {
    expect(khuonTuIso('1958')).toBe(KHUON_NGAY_RONG)
  })
})

describe('goSoVaoKhuon — go lien tuc khong can dau /', () => {
  it('go lan luot 01021985 tu khuon rong ra dung 01/02/1985, tu nhay vi tri', () => {
    let khuon = KHUON_NGAY_RONG
    let vt = viTriGoDauTien(khuon)
    for (const so of '01021985') {
      const ket = goSoVaoKhuon(khuon, vt, so)
      khuon = ket.khuon
      if (ket.viTriKe !== null) vt = ket.viTriKe
    }
    expect(khuon).toBe('01/02/1985')
  })

  it('go vao dau mot phan da co san so thi xoa sach ca phan (giong SelectAll khi focus)', () => {
    const ket = goSoVaoKhuon('25/04/2015', 6, '1')
    expect(ket.khuon).toBe('25/04/1___')
  })

  it('go het chu so cuoi cua nam tra viTriKe la null (bao hieu da xong)', () => {
    const ket = goSoVaoKhuon('01/02/198', 9, '5')
    expect(ket.viTriKe).toBeNull()
    expect(ket.khuon).toBe('01/02/1985')
  })

  it('click thang vao o nam (vi tri 6) go luon khong can go ngay/thang truoc', () => {
    let khuon = KHUON_NGAY_RONG
    let vt = 6
    for (const so of '1985') {
      const ket = goSoVaoKhuon(khuon, vt, so)
      khuon = ket.khuon
      if (ket.viTriKe !== null) vt = ket.viTriKe
    }
    expect(khuon).toBe('__/__/1985')
  })
})

describe('xoaLuiTrongKhuon', () => {
  it('xoa chu so tai vi tri con tro', () => {
    expect(xoaLuiTrongKhuon('25/04/2015', 9).khuon).toBe('25/04/201_')
  })

  it('dang o dau/dau phan rong thi lui ve chu so gan nhat truoc do de xoa', () => {
    const ket = xoaLuiTrongKhuon('25/__/____', 3)
    expect(ket.khuon).toBe('2_/__/____')
    expect(ket.viTriKe).toBe(1)
  })
})

describe('phanKeTiep', () => {
  it('dau ngay (0) sang dau thang (3), dau thang (3) sang dau nam (6)', () => {
    expect(phanKeTiep(0)).toBe(3)
    expect(phanKeTiep(3)).toBe(6)
  })

  it('dau nam (6) khong con phan sau', () => {
    expect(phanKeTiep(6)).toBeNull()
  })
})

describe('khuonSangPhan', () => {
  it('tach khuon day du thanh ba phan', () => {
    expect(khuonSangPhan('25/04/2015')).toEqual({ ngay: '25', thang: '04', nam: '2015' })
  })

  it('tach khuon thieu, bo ky tu _', () => {
    expect(khuonSangPhan('__/__/1985')).toEqual({ ngay: '', thang: '', nam: '1985' })
    expect(khuonSangPhan('__/05/1985')).toEqual({ ngay: '', thang: '05', nam: '1985' })
  })
})

describe('chuanHoaNgayThieu', () => {
  it('rong ca ba phan tra null (xoa ngay)', () => {
    expect(chuanHoaNgayThieu('', '', '')).toBeNull()
  })

  it('chi co nam thi dien 01/01', () => {
    expect(chuanHoaNgayThieu('', '', '1985')).toBe('1985-01-01')
  })

  it('co thang + nam thi dien ngay 01', () => {
    expect(chuanHoaNgayThieu('', '05', '1985')).toBe('1985-05-01')
  })

  it('du ca ba thi giu nguyen', () => {
    expect(chuanHoaNgayThieu('09', '11', '1996')).toBe('1996-11-09')
  })

  it('co ngay ma thieu thang thi khong hop le', () => {
    expect(chuanHoaNgayThieu('09', '', '1996')).toBeUndefined()
  })

  it('nam chua du 4 so thi khong hop le (con dang go do)', () => {
    expect(chuanHoaNgayThieu('', '', '198')).toBeUndefined()
  })

  it('ngay/thang khong ton tai thi khong hop le', () => {
    expect(chuanHoaNgayThieu('31', '02', '2020')).toBeUndefined()
  })
})
