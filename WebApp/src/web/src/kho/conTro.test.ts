// jsdom không cài IndexedDB — polyfill bằng fake-indexeddb (xem moKho.test.ts, cùng khuôn mẫu).
import 'fake-indexeddb/auto'
import { describe, expect, it } from 'vitest'
import { moKho } from './moKho'
import { docConTro, ghiConTro } from './conTro'

let demTen = 0
function tenKhoRieng(): string {
  demTen += 1
  return `qlgx-test-conTro-${demTen}`
}

describe('conTro (Task 2 — con tro dong bo, mot dong duy nhat, mo rong duoc)', () => {
  it('chua tung ghi thi doc ra gia tri mac dinh (epoch null, soThuTu 0)', async () => {
    const db = await moKho(tenKhoRieng())
    expect(await docConTro(db)).toEqual({ epoch: null, soThuTu: 0 })
    db.close()
  })

  it('ghiConTro roi docConTro doc lai dung gia tri vua ghi', async () => {
    const db = await moKho(tenKhoRieng())
    await ghiConTro(db, { epoch: 'abc-123', soThuTu: 42 })
    expect(await docConTro(db)).toEqual({ epoch: 'abc-123', soThuTu: 42 })
    db.close()
  })

  it('ghiConTro CHI sua truong co mat trong doi so, giu nguyen truong khac (ke ca truong khong biet truoc)', async () => {
    const db = await moKho(tenKhoRieng())
    // Mô phỏng Task 6 ghi epoch/soThuTu trước.
    await ghiConTro(db, { epoch: 'epoch-1', soThuTu: 100 })
    // Mô phỏng Task 4 ghi thêm trường đồng hồ của riêng nó, KHÔNG nhắc gì tới epoch/soThuTu.
    await ghiConTro(db, { mocNeo: 123456, doanHienTai: 2 })

    const ct = await docConTro(db)
    // Trường của Task 6 KHÔNG bị Task 4 xoá mất.
    expect(ct.epoch).toBe('epoch-1')
    expect(ct.soThuTu).toBe(100)
    // Trường mới của Task 4 có mặt, dù kiểu ConTro khai ở Task 2 không biết trước nó.
    expect(ct.mocNeo).toBe(123456)
    expect(ct.doanHienTai).toBe(2)

    // Rồi Task 6 tiến soThuTu lên, KHÔNG được xoá mất trường của Task 4.
    await ghiConTro(db, { soThuTu: 101 })
    const ct2 = await docConTro(db)
    expect(ct2.soThuTu).toBe(101)
    expect(ct2.mocNeo).toBe(123456)
    expect(ct2.doanHienTai).toBe(2)

    db.close()
  })

  it('ghi con tro va doc lai la MOT dong duy nhat, khong phai danh sach nhieu dong', async () => {
    const db = await moKho(tenKhoRieng())
    await ghiConTro(db, { epoch: 'e1', soThuTu: 1 })
    await ghiConTro(db, { epoch: 'e2', soThuTu: 2 })

    const dem = await new Promise<number>((resolve, reject) => {
      const yc = db.transaction('conTro', 'readonly').objectStore('conTro').count()
      yc.onsuccess = () => resolve(yc.result)
      yc.onerror = () => reject(yc.error)
    })
    expect(dem).toBe(1)

    // Ghi sau cùng thắng, đúng ngữ nghĩa "một dòng duy nhất".
    expect(await docConTro(db)).toEqual({ epoch: 'e2', soThuTu: 2 })

    db.close()
  })
})
