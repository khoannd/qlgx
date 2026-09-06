import { describe, expect, it } from 'vitest'
import { dinhDangNgay, ngayTuHienThi } from './ngay'

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
