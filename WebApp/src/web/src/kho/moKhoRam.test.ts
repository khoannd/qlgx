// jsdom không cài IndexedDB — polyfill bằng fake-indexeddb (xem moKho.test.ts, cùng khuôn mẫu).
// Đây là bài kiểm thử DUY NHẤT trong dự án dùng `fake-indexeddb` như MỘT PHẦN CỦA MÃ ĐANG KIỂM THỬ
// (moKhoRam.ts), không chỉ như polyfill nền cho jsdom — nên KHÔNG import '/auto' ở đây để tránh lẫn
// factory toàn cục (giả lập `indexedDB` chung) với factory RIÊNG mà `moKhoRam` tự tạo.
import 'fake-indexeddb/auto'
import { IDBFactory } from 'fake-indexeddb'
import { describe, expect, it } from 'vitest'
import { KHO_BAN_GHI, KHO_CAN_XEM_LAI, KHO_CON_TRO, KHO_HANG_CHO, KHO_SO_DA_NHAN } from './moKho'
import { moKhoRam } from './moKhoRam'

function ghiVaoKho(db: IDBDatabase, tenKhoCon: string, khoa: string, giaTri: unknown): Promise<void> {
  return new Promise((resolve, reject) => {
    const gd = db.transaction(tenKhoCon, 'readwrite')
    gd.objectStore(tenKhoCon).put(giaTri, khoa)
    gd.oncomplete = () => resolve()
    gd.onerror = () => reject(gd.error)
  })
}

function docTuKho(db: IDBDatabase, tenKhoCon: string, khoa: string): Promise<unknown> {
  return new Promise((resolve, reject) => {
    const yc = db.transaction(tenKhoCon, 'readonly').objectStore(tenKhoCon).get(khoa)
    yc.onsuccess = () => resolve(yc.result)
    yc.onerror = () => reject(yc.error)
  })
}

describe('moKhoRam (Task 7 — lớp lưu trữ RAM cho chế độ tắt offline, spec 6.6)', () => {
  it('tao du 5 kho con GIONG HET moKho() that — dung chung schema, khong rieng logic', async () => {
    const db = await moKhoRam()
    try {
      expect(db.objectStoreNames.contains(KHO_BAN_GHI)).toBe(true)
      expect(db.objectStoreNames.contains(KHO_HANG_CHO)).toBe(true)
      expect(db.objectStoreNames.contains(KHO_SO_DA_NHAN)).toBe(true)
      expect(db.objectStoreNames.contains(KHO_CON_TRO)).toBe(true)
      expect(db.objectStoreNames.contains(KHO_CAN_XEM_LAI)).toBe(true)
      expect(db.objectStoreNames.length).toBe(5)
    } finally {
      db.close()
    }
  })

  it('ghi roi doc lai TRONG CUNG mot factory thi thay du lieu (hoat dong nhu mot kho IndexedDB that)', async () => {
    const factory = new IDBFactory()
    const db1 = await moKhoRam('kho-chung', undefined, factory)
    await ghiVaoKho(db1, KHO_HANG_CHO, 'viec-1', { noiDung: 'chua gui' })
    db1.close()

    const db2 = await moKhoRam('kho-chung', undefined, factory)
    const doc = await docTuKho(db2, KHO_HANG_CHO, 'viec-1')
    db2.close()
    expect(doc).toEqual({ noiDung: 'chua gui' })
  })

  it('hai lan goi moKhoRam() KHONG truyen factory la HAI bo nho HOAN TOAN RIENG', async () => {
    // Dung ten kho GIONG NHAU nhung KHONG truyen factory — moi lan goi tu tao mot IDBFactory moi
    // (xem moKhoRam.ts), nen du trung ten van khong thay du lieu cua lan truoc: dung y "mot hop
    // dung moi moi khi can" cua che do tat offline (khong ro ri du lieu giua cac lan bat/tat).
    const dbA = await moKhoRam('kho-rieng')
    await ghiVaoKho(dbA, KHO_HANG_CHO, 'viec-A', { noiDung: 'cua A' })
    dbA.close()

    const dbB = await moKhoRam('kho-rieng')
    const docTuB = await docTuKho(dbB, KHO_HANG_CHO, 'viec-A')
    dbB.close()
    expect(docTuB).toBeUndefined()
  })

  it('KHONG dung chung factory voi indexedDB toan cuc (khong ro ri sang kho that)', async () => {
    const tenKho = 'kho-ram-khong-le-thay-o-that'
    const dbRam = await moKhoRam(tenKho)
    await ghiVaoKho(dbRam, KHO_HANG_CHO, 'viec-ram', { noiDung: 'chi trong RAM' })
    dbRam.close()

    // Mo CUNG ten kho do qua indexedDB toan cuc (factory that/polyfill '/auto' cua moi truong test)
    // — khong duoc thay du lieu vua ghi vao ban RAM o tren.
    const dbThat = await new Promise<IDBDatabase>((resolve, reject) => {
      const yc = indexedDB.open(tenKho, 1)
      yc.onupgradeneeded = () => {
        const db = yc.result
        for (const ten of [KHO_BAN_GHI, KHO_HANG_CHO, KHO_SO_DA_NHAN, KHO_CON_TRO, KHO_CAN_XEM_LAI]) {
          if (!db.objectStoreNames.contains(ten)) db.createObjectStore(ten)
        }
      }
      yc.onsuccess = () => resolve(yc.result)
      yc.onerror = () => reject(yc.error)
    })
    const doc = await docTuKho(dbThat, KHO_HANG_CHO, 'viec-ram')
    dbThat.close()
    expect(doc).toBeUndefined()
  })
})
