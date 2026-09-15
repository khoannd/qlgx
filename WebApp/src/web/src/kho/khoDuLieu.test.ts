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

    // Ép giao dịch ABORT ngay sau khi lệnh ghi thứ hai (dòng hàng chờ — dùng `add()`, xem
    // `khoDuLieu.ts`) được gửi đi, trước khi giao dịch kịp commit — mô phỏng mất điện/đứt nguồn
    // giữa hai thao tác ghi. Đây là fact bắt buộc theo brief: nếu ai đó tách `ghiVaXepHang` thành
    // HAI giao dịch riêng, spy này sẽ KHÔNG bắt được kịch bản hỏng giữa chừng đúng như mô tả nữa,
    // vì thao tác ghi bản ghi đầu tiên đã commit xong từ trước khi thao tác thứ hai (đã ở giao
    // dịch khác) kịp bị ép abort — dữ liệu hiển thị sẽ CÒN LẠI một mình trên màn hình, đúng trạng
    // thái tệ nhất mà ràng buộc 2 cấm.
    const addGoc = IDBObjectStore.prototype.add
    const spy = vi.spyOn(IDBObjectStore.prototype, 'add').mockImplementation(function (
      this: IDBObjectStore,
      ...doiSo: Parameters<IDBObjectStore['add']>
    ) {
      const yc = addGoc.apply(this, doiSo)
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

  it('abort KHONG qua loi (vi du het dia luc commit) van khong ghi gi ca — khong phai ma chet', async () => {
    // Khac voi ca tren (abort do MOT REQUEST loi), ca nay mo phong abort xay ra SAU KHI moi put()
    // da thanh cong (khong request nao loi) — dung kich ban trinh duyet that: het dung luong dia
    // luc COMMIT giao dich, sau khi tat ca cac put() rieng le da onsuccess. Trong tinh huong nay
    // IndexedDB chi phat 'abort' tren giao dich, KHONG BAO GIO phat 'error' tren request nao —
    // neu `gd.onabort` bi go bo (hoac vo tinh resolve thay vi reject), ham se AM THAM "thanh cong"
    // ma khong ghi gi vao ca hai kho — mat du lieu ma khong ai biet.
    const tenKho = tenKhoRieng()
    const db = await moKho(tenKho)

    const addGoc = IDBObjectStore.prototype.add
    const spy = vi.spyOn(IDBObjectStore.prototype, 'add').mockImplementation(function (
      this: IDBObjectStore,
      ...doiSo: Parameters<IDBObjectStore['add']>
    ) {
      const yc = addGoc.apply(this, doiSo)
      if (this.name === KHO_HANG_CHO) {
        // Abort ngay trong onsuccess cua chinh add() nay — luc nay KHONG con request nao dang
        // cho/loi, dung mo phong "abort luc commit" (khac han spy o test truoc goi abort() truoc
        // khi add() hang cho kip onsuccess).
        yc.onsuccess = () => this.transaction.abort()
      }
      return yc
    })

    await expect(
      ghiVaXepHang(db, { khoa: 'giaoDan:2', giaTri: { hoTen: 'Khong ai ca' } }, { maThaoTac: 'tt-abort-commit' }),
    ).rejects.toThrow()

    spy.mockRestore()

    const banGhiConLai = await docTuKho(db, KHO_BAN_GHI, 'giaoDan:2')
    expect(banGhiConLai).toBeUndefined()
    expect(await demHangCho(db)).toBe(0)

    db.close()
  })
})
