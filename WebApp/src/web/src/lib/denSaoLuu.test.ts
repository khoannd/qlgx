import { describe, expect, it } from 'vitest'
import { nhanDen, nhanLoiSaoLuu, thoiGianTuongDoi, tinhDenHieuLuc } from './denSaoLuu'
import type { TinhTrangSaoLuu } from '../api/types'

const BAY_GIO = new Date('2026-09-16T12:00:00Z')
const truoc = (gio: number) => new Date(BAY_GIO.getTime() - gio * 3_600_000).toISOString()

function tt(sua: Partial<TinhTrangSaoLuu> = {}): TinhTrangSaoLuu {
  return {
    den: 'xanh', saoLuuGanNhat: truoc(2), soBanSao: 47,
    dienTapGanNhat: truoc(48), dienTapDat: true, loiGanNhat: null, ...sua,
  }
}

describe('tinhDenHieuLuc', () => {
  it('moi thu tuoi moi thi xanh', () => {
    expect(tinhDenHieuLuc(tt(), BAY_GIO)).toBe('xanh')
  })

  // I2: sao luu tre.
  it('qua 8 gio chua co ban sao moi thi vang', () => {
    expect(tinhDenHieuLuc(tt({ saoLuuGanNhat: truoc(9) }), BAY_GIO)).toBe('vang')
  })

  it('qua 24 gio chua co ban sao moi thi LEO THANG len do', () => {
    expect(tinhDenHieuLuc(tt({ saoLuuGanNhat: truoc(30) }), BAY_GIO)).toBe('do')
  })

  // I3: day la loi that su nguy hiem — may chu tra ve 'xanh' vi DienTapDat=true, khong he xet
  // lan dien tap do da cu hang thang. Giao dien khong duoc tin theo.
  it('dien tap dat nhung da cu hon 14 ngay thi vang, du may chu bao xanh', () => {
    expect(tinhDenHieuLuc(tt({ den: 'xanh', dienTapGanNhat: truoc(15 * 24) }), BAY_GIO))
      .toBe('vang')
  })

  it('dien tap dat nhung da cu hon 30 ngay thi DO, du may chu bao xanh', () => {
    expect(tinhDenHieuLuc(tt({ den: 'xanh', dienTapGanNhat: truoc(45 * 24) }), BAY_GIO))
      .toBe('do')
  })

  it('chua tung dien tap lan nao thi KHONG tu leo thang (may moi cai, tranh bao dong gia)', () => {
    expect(tinhDenHieuLuc(tt({ dienTapGanNhat: null }), BAY_GIO)).toBe('xanh')
  })

  it('khong bao gio HA NHE den ma may chu da bao do', () => {
    expect(tinhDenHieuLuc(tt({ den: 'do' }), BAY_GIO)).toBe('do')
    expect(tinhDenHieuLuc(tt({ den: 'vang' }), BAY_GIO)).toBe('vang')
  })

  it('chuoi thoi gian hong thi bo qua, khong lam sap man hinh', () => {
    expect(tinhDenHieuLuc(tt({ saoLuuGanNhat: 'khong-phai-ngay' }), BAY_GIO)).toBe('xanh')
  })
})

describe('nhanDen', () => {
  it('vang phai KHAC xanh ca ve mau lan cham — khong duoc trong y het nhau', () => {
    expect(nhanDen('vang').mau).not.toBe(nhanDen('xanh').mau)
    expect(nhanDen('vang').cham).not.toBe(nhanDen('xanh').cham)
    expect(nhanDen('do').mau).not.toBe(nhanDen('vang').mau)
  })
})

describe('nhanLoiSaoLuu', () => {
  // I4: chuoi tho tu qlgx-runner.sh la tieng Viet KHONG DAU va co ten cong cu ky thuat.
  it('doi chuoi ky thuat sang tieng Viet co dau, khong con chu "restic"', () => {
    const cau = nhanLoiSaoLuu('restic check that bai')
    expect(cau).toMatch(/toàn vẹn/)
    expect(cau).not.toMatch(/restic/i)
  })

  it('nhan ra loi sao luu va loi dien tap', () => {
    expect(nhanLoiSaoLuu('sao luu that bai (lenh cuoi cung tra ve loi)')).toMatch(/sao lưu/i)
    expect(nhanLoiSaoLuu('Dien tap phuc hoi that bai')).toMatch(/diễn tập/i)
  })

  it('chuoi la van co cau tieng Viet tu te, khong im lang va khong lo nguyen van', () => {
    const cau = nhanLoiSaoLuu('pg_dump: error: connection to server failed')
    expect(cau).toMatch(/trục trặc/)
    expect(cau).not.toMatch(/pg_dump/)
  })

  it('khong co loi thi khong co cau nao', () => {
    expect(nhanLoiSaoLuu(null)).toBeNull()
    expect(nhanLoiSaoLuu('')).toBeNull()
  })
})

describe('thoiGianTuongDoi', () => {
  it('doc duoc bang tieng Viet, khong phai chu ky thuat', () => {
    expect(thoiGianTuongDoi(truoc(2), BAY_GIO)).toBe('2 giờ trước')
    expect(thoiGianTuongDoi(truoc(0.5), BAY_GIO)).toBe('30 phút trước')
    expect(thoiGianTuongDoi(truoc(72), BAY_GIO)).toBe('3 ngày trước')
    expect(thoiGianTuongDoi(null, BAY_GIO)).toBe('')
  })
})
