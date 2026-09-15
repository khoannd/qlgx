// jsdom không cài IndexedDB — polyfill bằng fake-indexeddb (xem moKho.test.ts, cùng khuôn mẫu).
import 'fake-indexeddb/auto'
import { describe, expect, it } from 'vitest'
import { moKho } from './moKho'
import {
  demCanXemLai,
  docCanXemLai,
  ghiCanXemLai,
  xoaKhoiCanXemLai,
  type DongCanXemLai,
} from './canXemLai'
import type { DongHangCho } from './hangCho'

let demTen = 0
function tenKhoRieng(): string {
  demTen += 1
  return `qlgx-test-canXemLai-${demTen}`
}

function dongHangChoMau(maThaoTac: string): DongHangCho {
  return { maThaoTac, doan: 0, loai: 'sua', bang: 'GiaoDan', banGhiId: 'gd-1', truong: 'hoTen', giaTri: 'A' }
}

describe('canXemLai (Task 6 — hộp cần xem lại cho kết quả tu_choi)', () => {
  it('kho rong thi doc tra ve mang rong va dem tra ve 0', async () => {
    const db = await moKho(tenKhoRieng())
    expect(await docCanXemLai(db)).toEqual([])
    expect(await demCanXemLai(db)).toBe(0)
    db.close()
  })

  it('ghi mot muc roi doc lai giu nguyen VEN dong hang cho goc', async () => {
    const db = await moKho(tenKhoRieng())
    const dongGoc = dongHangChoMau('tt-1')
    const muc: DongCanXemLai = {
      maThaoTac: 'tt-1',
      thongBao: 'Vi pham rao chan tat dinh',
      dongHangChoGoc: dongGoc,
      taoLuc: '2026-09-15T10:00:00.000Z',
    }

    await ghiCanXemLai(db, [muc])

    const ds = await docCanXemLai(db)
    expect(ds).toHaveLength(1)
    expect(ds[0]).toEqual(muc)
    expect(ds[0].dongHangChoGoc).toEqual(dongGoc)
    expect(await demCanXemLai(db)).toBe(1)

    db.close()
  })

  it('thongBao null van ghi va doc dung', async () => {
    const db = await moKho(tenKhoRieng())
    const muc: DongCanXemLai = {
      maThaoTac: 'tt-2',
      thongBao: null,
      dongHangChoGoc: dongHangChoMau('tt-2'),
      taoLuc: '2026-09-15T10:00:00.000Z',
    }
    await ghiCanXemLai(db, [muc])
    const ds = await docCanXemLai(db)
    expect(ds[0].thongBao).toBeNull()
    db.close()
  })

  it('xoaKhoiCanXemLai xoa dung mot muc, giu nguyen muc khac', async () => {
    const db = await moKho(tenKhoRieng())
    await ghiCanXemLai(db, [
      { maThaoTac: 'tt-1', thongBao: null, dongHangChoGoc: dongHangChoMau('tt-1'), taoLuc: 't1' },
      { maThaoTac: 'tt-2', thongBao: null, dongHangChoGoc: dongHangChoMau('tt-2'), taoLuc: 't2' },
    ])

    await xoaKhoiCanXemLai(db, 'tt-1')

    const ds = await docCanXemLai(db)
    expect(ds).toHaveLength(1)
    expect(ds[0].maThaoTac).toBe('tt-2')

    db.close()
  })

  it('ghi lai cung maThaoTac thi ghi de (put), khong tao ban ghi thu hai', async () => {
    const db = await moKho(tenKhoRieng())
    await ghiCanXemLai(db, [{ maThaoTac: 'tt-1', thongBao: 'Lan 1', dongHangChoGoc: dongHangChoMau('tt-1'), taoLuc: 't1' }])
    await ghiCanXemLai(db, [{ maThaoTac: 'tt-1', thongBao: 'Lan 2', dongHangChoGoc: dongHangChoMau('tt-1'), taoLuc: 't2' }])

    const ds = await docCanXemLai(db)
    expect(ds).toHaveLength(1)
    expect(ds[0].thongBao).toBe('Lan 2')

    db.close()
  })

  it('ghiCanXemLai voi mang rong khong lam gi va khong nem loi', async () => {
    const db = await moKho(tenKhoRieng())
    await expect(ghiCanXemLai(db, [])).resolves.toBeUndefined()
    expect(await demCanXemLai(db)).toBe(0)
    db.close()
  })
})
