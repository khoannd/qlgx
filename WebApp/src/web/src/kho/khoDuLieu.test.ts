// jsdom không cài IndexedDB — polyfill bằng fake-indexeddb (xem moKho.test.ts, cùng khuôn mẫu).
import 'fake-indexeddb/auto'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { KHO_BAN_GHI, KHO_HANG_CHO, moKho } from './moKho'
import { ghiVaXepHang } from './khoDuLieu'
import { demHangCho, docHangCho } from './hangCho'

let demTen = 0
function tenKhoRieng(): string {
  demTen += 1
  return `qlgx-test-khoDuLieu-${demTen}`
}

function docTuKho(db: IDBDatabase, tenKhoCon: string, khoa: IDBValidKey): Promise<unknown> {
  return new Promise((resolve, reject) => {
    const yc = db.transaction(tenKhoCon, 'readonly').objectStore(tenKhoCon).get(khoa)
    yc.onsuccess = () => resolve(yc.result)
    yc.onerror = () => reject(yc.error)
  })
}

describe('ghiVaXepHang (Task 2 — ràng buộc sống còn: ghi bản ghi và xếp hàng chờ là MỘT giao dịch)', () => {
  afterEach(() => {
    vi.restoreAllMocks()
  })

  it('ghi binh thuong: ban ghi hien thi VA dong hang cho cung xuat hien', async () => {
    const tenKho = tenKhoRieng()
    const db = await moKho(tenKho)

    await ghiVaXepHang(
      db,
      { khoa: 'giaoDan:1', giaTri: { hoTen: 'Nguyen Van A' } },
      { maThaoTac: 'tt-1', bang: 'GiaoDan', banGhiId: '1' },
    )

    const banGhi = await docTuKho(db, KHO_BAN_GHI, 'giaoDan:1')
    expect(banGhi).toEqual({ hoTen: 'Nguyen Van A' })

    const hangCho = await docHangCho(db)
    expect(hangCho).toHaveLength(1)
    expect(hangCho[0]).toMatchObject({ maThaoTac: 'tt-1', bang: 'GiaoDan', banGhiId: '1' })

    db.close()
  })

  it('khong truyen doan thi mac dinh doan = 0', async () => {
    const tenKho = tenKhoRieng()
    const db = await moKho(tenKho)

    await ghiVaXepHang(db, { khoa: 'giaoDan:1', giaTri: {} }, { maThaoTac: 'tt-1' })

    const [dong] = await docHangCho(db)
    expect(dong.doan).toBe(0)

    db.close()
  })

  it('truyen doan tuong minh thi giu nguyen, khong bi de ve 0', async () => {
    const tenKho = tenKhoRieng()
    const db = await moKho(tenKho)

    await ghiVaXepHang(db, { khoa: 'giaoDan:1', giaTri: {} }, { maThaoTac: 'tt-1', doan: 7 })

    const [dong] = await docHangCho(db)
    expect(dong.doan).toBe(7)

    db.close()
  })

  it('ghi nhieu lan lien tiep thi hang cho giu DUNG THU TU chen', async () => {
    const tenKho = tenKhoRieng()
    const db = await moKho(tenKho)

    await ghiVaXepHang(db, { khoa: 'a', giaTri: 1 }, { maThaoTac: 'tt-1' })
    await ghiVaXepHang(db, { khoa: 'b', giaTri: 2 }, { maThaoTac: 'tt-2' })
    await ghiVaXepHang(db, { khoa: 'c', giaTri: 3 }, { maThaoTac: 'tt-3' })

    const hangCho = await docHangCho(db)
    expect(hangCho.map((d) => d.maThaoTac)).toEqual(['tt-1', 'tt-2', 'tt-3'])

    db.close()
  })

  it('ghi ban ghi va xep hang la MOT giao dich — hong giua chung thi ca hai cung khong co', async () => {
    const tenKho = tenKhoRieng()
    const db = await moKho(tenKho)

    // Ép giao dịch ABORT ngay sau khi lệnh put() thứ hai (dòng hàng chờ) được gửi đi, trước khi
    // giao dịch kịp commit — mô phỏng mất điện/đứt nguồn giữa hai thao tác ghi. Đây là fact bắt
    // buộc theo brief: nếu ai đó tách `ghiVaXepHang` thành HAI giao dịch riêng, spy này sẽ KHÔNG
    // bắt được kịch bản hỏng giữa chừng đúng như mô tả nữa, vì thao tác ghi bản ghi đầu tiên đã
    // commit xong từ trước khi thao tác thứ hai (đã ở giao dịch khác) kịp bị ép abort — dữ liệu
    // hiển thị sẽ CÒN LẠI một mình trên màn hình, đúng trạng thái tệ nhất mà ràng buộc 2 cấm.
    const putGoc = IDBObjectStore.prototype.put
    const spy = vi.spyOn(IDBObjectStore.prototype, 'put').mockImplementation(function (
      this: IDBObjectStore,
      ...doiSo: Parameters<IDBObjectStore['put']>
    ) {
      const yc = putGoc.apply(this, doiSo)
      if (this.name === KHO_HANG_CHO) {
        this.transaction.abort()
      }
      return yc
    })

    await expect(
      ghiVaXepHang(db, { khoa: 'giaoDan:1', giaTri: { hoTen: 'Vo danh' } }, { maThaoTac: 'tt-abort' }),
    ).rejects.toThrow()

    spy.mockRestore()

    // Ràng buộc sống còn: CẢ HAI bên đều không có gì, không phải chỉ một bên.
    const banGhiConLai = await docTuKho(db, KHO_BAN_GHI, 'giaoDan:1')
    expect(banGhiConLai).toBeUndefined()
    expect(await demHangCho(db)).toBe(0)

    db.close()
  })
})
