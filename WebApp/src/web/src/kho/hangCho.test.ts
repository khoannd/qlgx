// jsdom không cài IndexedDB — polyfill bằng fake-indexeddb (xem moKho.test.ts, cùng khuôn mẫu).
import 'fake-indexeddb/auto'
import { describe, expect, it } from 'vitest'
import { moKho } from './moKho'
import { ghiVaXepHang } from './khoDuLieu'
import { demHangCho, docHangCho, xoaKhoiHangCho } from './hangCho'

let demTen = 0
function tenKhoRieng(): string {
  demTen += 1
  return `qlgx-test-hangCho-${demTen}`
}

describe('hangCho (Task 2 — doc/xoa/dem hang cho)', () => {
  it('kho rong thi doc ra mang rong va dem ra 0', async () => {
    const db = await moKho(tenKhoRieng())
    expect(await docHangCho(db)).toEqual([])
    expect(await demHangCho(db)).toBe(0)
    db.close()
  })

  it('docHangCho gioi han so dong tra ve, van theo dung thu tu chen', async () => {
    const db = await moKho(tenKhoRieng())
    await ghiVaXepHang(db, { khoa: 'a', giaTri: 1 }, { maThaoTac: 'tt-1' })
    await ghiVaXepHang(db, { khoa: 'b', giaTri: 2 }, { maThaoTac: 'tt-2' })
    await ghiVaXepHang(db, { khoa: 'c', giaTri: 3 }, { maThaoTac: 'tt-3' })

    const haiDongDau = await docHangCho(db, 2)
    expect(haiDongDau.map((d) => d.maThaoTac)).toEqual(['tt-1', 'tt-2'])

    db.close()
  })

  it('xoaKhoiHangCho chi xoa dung nhung ma thao tac duoc chi, giu nguyen cac dong khac', async () => {
    const db = await moKho(tenKhoRieng())
    await ghiVaXepHang(db, { khoa: 'a', giaTri: 1 }, { maThaoTac: 'tt-1' })
    await ghiVaXepHang(db, { khoa: 'b', giaTri: 2 }, { maThaoTac: 'tt-2' })
    await ghiVaXepHang(db, { khoa: 'c', giaTri: 3 }, { maThaoTac: 'tt-3' })

    await xoaKhoiHangCho(db, ['tt-1', 'tt-3'])

    const conLai = await docHangCho(db)
    expect(conLai.map((d) => d.maThaoTac)).toEqual(['tt-2'])
    expect(await demHangCho(db)).toBe(1)

    db.close()
  })

  it('xoaKhoiHangCho voi mang rong khong lam gi, khong nem loi', async () => {
    const db = await moKho(tenKhoRieng())
    await ghiVaXepHang(db, { khoa: 'a', giaTri: 1 }, { maThaoTac: 'tt-1' })

    await expect(xoaKhoiHangCho(db, [])).resolves.toBeUndefined()
    expect(await demHangCho(db)).toBe(1)

    db.close()
  })

  it('sau khi xoa bot dong dau, dong MOI ghi tiep theo van dung o CUOI (khong lay lai thu tu da xoa)', async () => {
    // Đây chính là cái bẫy nêu trong hangCho.ts: nếu lấy khoá kế tiếp bằng count() thay vì khoá lớn
    // nhất thực sự đang có, dòng mới sẽ nhận một khoá NHỎ HƠN dòng cũ còn lại — đảo ngược thứ tự.
    const db = await moKho(tenKhoRieng())
    await ghiVaXepHang(db, { khoa: 'a', giaTri: 1 }, { maThaoTac: 'tt-1' })
    await ghiVaXepHang(db, { khoa: 'b', giaTri: 2 }, { maThaoTac: 'tt-2' })
    await ghiVaXepHang(db, { khoa: 'c', giaTri: 3 }, { maThaoTac: 'tt-3' })

    // Xoá hai dòng đầu — count() bây giờ sẽ trả 1, dễ gây nhầm với "khoá kế tiếp = 1" (trùng/đứng
    // trước dòng "tt-3" còn lại).
    await xoaKhoiHangCho(db, ['tt-1', 'tt-2'])

    await ghiVaXepHang(db, { khoa: 'd', giaTri: 4 }, { maThaoTac: 'tt-4' })

    const thuTu = await docHangCho(db)
    expect(thuTu.map((d) => d.maThaoTac)).toEqual(['tt-3', 'tt-4'])

    db.close()
  })
})
