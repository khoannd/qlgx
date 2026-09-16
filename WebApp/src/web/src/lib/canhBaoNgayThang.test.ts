import { describe, expect, it } from 'vitest'
import { canhBaoHonPhoiTruocSinh, tinhCanhBaoNgayThang } from './canhBaoNgayThang'

describe('tinhCanhBaoNgayThang', () => {
  it('ngay rua toi truoc ngay sinh thi canh bao co con so cu the, gan vao ngayRuaToi', () => {
    const ket = tinhCanhBaoNgayThang({ ngaySinh: '1992-07-25', ngayRuaToi: '1990-03-12' })
    expect(ket.ngayRuaToi).toEqual([
      'Ngày rửa tội (12/03/1990) trước ngày sinh (25/07/1992). ' +
      'Thông tin vẫn được lưu — hãy đối chiếu lại với sổ gốc.',
    ])
  })

  it('thu tu hop le thi khong canh bao gi ca', () => {
    const ket = tinhCanhBaoNgayThang({
      ngaySinh: '1990-01-01', ngayRuaToi: '1990-02-01', ngayRuocLe: '1998-01-01',
      ngayThemSuc: '2004-01-01', ngayXucDau: '2020-01-01', ngayQuaDoi: '2023-01-01',
    })
    expect(ket).toEqual({})
  })

  it('thieu mot trong hai moc thi khong tinh duoc, khong canh bao', () => {
    const ket = tinhCanhBaoNgayThang({ ngaySinh: '1990-01-01', ngayRuaToi: null })
    expect(ket).toEqual({})
  })

  it('mot moc co the vi pham nhieu cap cung luc (nhieu thong diep)', () => {
    // Rước lễ trước cả Ngày sinh LẪN Ngày rửa tội — hai thông điệp khác nhau, cùng gắn vào
    // ngayRuocLe.
    const ket = tinhCanhBaoNgayThang({
      ngaySinh: '1990-06-01', ngayRuaToi: '1990-07-01', ngayRuocLe: '1990-01-01',
    })
    expect(ket.ngayRuocLe).toHaveLength(2)
  })

  it('ngay qua doi truoc ngay sinh thi canh bao', () => {
    const ket = tinhCanhBaoNgayThang({ ngaySinh: '2000-01-01', ngayQuaDoi: '1999-01-01' })
    expect(ket.ngayQuaDoi?.[0]).toMatch(/Ngày qua đời .* trước ngày sinh/)
  })

  it('ngay bang nhau khong tinh la vi pham (>=)', () => {
    const ket = tinhCanhBaoNgayThang({ ngaySinh: '1990-01-01', ngayRuaToi: '1990-01-01' })
    expect(ket).toEqual({})
  })
})

describe('canhBaoHonPhoiTruocSinh', () => {
  it('ngay hon phoi truoc ngay sinh thi tra ve thong diep co con so', () => {
    const tb = canhBaoHonPhoiTruocSinh('1990-01-01', '1995-01-01')
    expect(tb).toBe(
      'Ngày hôn phối (01/01/1990) trước ngày sinh (01/01/1995). ' +
      'Thông tin vẫn được lưu — hãy đối chiếu lại với sổ gốc.',
    )
  })

  it('thu tu hop le hoac thieu du lieu thi tra ve null', () => {
    expect(canhBaoHonPhoiTruocSinh('2018-05-01', '1990-01-01')).toBeNull()
    expect(canhBaoHonPhoiTruocSinh(null, '1990-01-01')).toBeNull()
    expect(canhBaoHonPhoiTruocSinh('2018-05-01', null)).toBeNull()
  })
})
