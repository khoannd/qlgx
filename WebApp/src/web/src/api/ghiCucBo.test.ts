// jsdom không cài IndexedDB — polyfill bằng fake-indexeddb (xem moKho.test.ts, cùng khuôn mẫu).
import 'fake-indexeddb/auto'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { moKho, KHO_BAN_GHI, KHO_HANG_CHO } from '../kho/moKho'
import { docHangCho } from '../kho/hangCho'
import { khoiTaoPhuThuoc, type DongHangChoDongBo } from '../dongbo/boDongBo'
import { ghiCucBo } from './ghiCucBo'

let demTen = 0
function tenKhoRieng(): string {
  demTen += 1
  return `qlgx-test-ghiCucBo-${demTen}`
}

function docTuKho(db: IDBDatabase, tenKhoCon: string, khoa: IDBValidKey): Promise<unknown> {
  return new Promise((resolve, reject) => {
    const yc = db.transaction(tenKhoCon, 'readonly').objectStore(tenKhoCon).get(khoa)
    yc.onsuccess = () => resolve(yc.result)
    yc.onerror = () => reject(yc.error)
  })
}

describe('ghiCucBo (Task 7 — spec 7.1: đường ghi luôn-lưu-vào-máy-trước)', () => {
  afterEach(() => {
    vi.restoreAllMocks()
  })

  it('ghi MOT truong: xuat hien dung 1 dong hang cho DUNG hinh dang DongHangChoDongBo, va ban ghi hien thi', async () => {
    const db = await moKho(tenKhoRieng())
    const phuThuoc = await khoiTaoPhuThuoc(db)

    await ghiCucBo(db, phuThuoc, {
      loai: 'sua',
      bang: 'GiaoDan',
      banGhiId: 'gd-1',
      truong: [{ truong: 'HoTen', giaTri: 'Nguyen Van A' }],
      banGhi: { khoa: 'GiaoDan:gd-1', giaTri: { hoTen: 'Nguyen Van A' } },
    })

    const banGhi = await docTuKho(db, KHO_BAN_GHI, 'GiaoDan:gd-1')
    expect(banGhi).toEqual({ hoTen: 'Nguyen Van A' })

    const hangCho = (await docHangCho(db)) as DongHangChoDongBo[]
    expect(hangCho).toHaveLength(1)
    const dong = hangCho[0]
    expect(dong.loai).toBe('sua')
    expect(dong.bang).toBe('GiaoDan')
    expect(dong.banGhiId).toBe('gd-1')
    expect(dong.truong).toBe('HoTen')
    expect(dong.giaTri).toBe('Nguyen Van A')
    expect(dong.doan).toBe(0) // chua co nhay gio nao — doan mac dinh 0
    // maThaoTac/giaoDichId phai la Guid dang "D" chu thuong hop le (rang buoc 1, brief).
    expect(dong.maThaoTac).toMatch(/^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/)
    expect(dong.giaoDichId).toMatch(/^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/)
    // vatLy phai dung dinh dang co dinh cua he thong (xem dauDongHo.ts), KHONG phai gio he thong tho.
    expect(dong.vatLy).toMatch(/^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}\.\d{6}Z$/)
    expect(dong.logic).toBeTypeOf('number')

    db.close()
  })

  it('gui thang ghiVaXepHang khi CHI MOT truong (tai su dung diem ghi da kiem thu ky cua Task 2)', async () => {
    const db = await moKho(tenKhoRieng())
    const phuThuoc = await khoiTaoPhuThuoc(db)

    // Ep giao dich abort giua chung (dung lai ky thuat cua khoDuLieu.test.ts) — neu ghiCucBo THAT
    // SU goi ghiVaXepHang (mo mot giao dich bao ca banGhi lan hang cho), spy nay se bat duoc dung
    // kich ban "hong giua chung thi ca hai cung khong co gi".
    const addGoc = IDBObjectStore.prototype.add
    const spy = vi.spyOn(IDBObjectStore.prototype, 'add').mockImplementation(function (
      this: IDBObjectStore,
      ...doiSo: Parameters<IDBObjectStore['add']>
    ) {
      const yc = addGoc.apply(this, doiSo)
      if (this.name === KHO_HANG_CHO) this.transaction.abort()
      return yc
    })

    await expect(
      ghiCucBo(db, phuThuoc, {
        loai: 'sua',
        bang: 'GiaoDan',
        banGhiId: 'gd-abort',
        truong: [{ truong: 'HoTen', giaTri: 'Vo danh' }],
        banGhi: { khoa: 'GiaoDan:gd-abort', giaTri: { hoTen: 'Vo danh' } },
      }),
    ).rejects.toThrow()

    spy.mockRestore()
    expect(await docTuKho(db, KHO_BAN_GHI, 'GiaoDan:gd-abort')).toBeUndefined()
    expect(await docHangCho(db)).toHaveLength(0)

    db.close()
  })

  it('ghi NHIEU truong trong MOT lan luu: nhieu dong hang cho, CUNG giaoDichId, CUNG dau vat ly/logic, MOI dong mot maThaoTac rieng', async () => {
    const db = await moKho(tenKhoRieng())
    const phuThuoc = await khoiTaoPhuThuoc(db)

    await ghiCucBo(db, phuThuoc, {
      loai: 'sua',
      bang: 'GiaoDan',
      banGhiId: 'gd-2',
      truong: [
        { truong: 'HoTen', giaTri: 'Tran Thi B' },
        { truong: 'NgaySinh', giaTri: '2000-01-01' },
        { truong: 'DiaChi', giaTri: null },
      ],
      banGhi: { khoa: 'GiaoDan:gd-2', giaTri: { hoTen: 'Tran Thi B', ngaySinh: '2000-01-01', diaChi: null } },
    })

    const banGhi = await docTuKho(db, KHO_BAN_GHI, 'GiaoDan:gd-2')
    expect(banGhi).toEqual({ hoTen: 'Tran Thi B', ngaySinh: '2000-01-01', diaChi: null })

    const hangCho = (await docHangCho(db)) as DongHangChoDongBo[]
    expect(hangCho).toHaveLength(3)
    expect(hangCho.map((d) => d.truong)).toEqual(['HoTen', 'NgaySinh', 'DiaChi'])
    expect(hangCho.map((d) => d.giaTri)).toEqual(['Tran Thi B', '2000-01-01', null])

    // CUNG mot giaoDichId (mot lan luu = mot giao dich) — MOI dong mot maThaoTac RIENG (khoa nghiep
    // vu chong gui trung — rang buoc 9, boDongBo.ts).
    const giaoDichIds = new Set(hangCho.map((d) => d.giaoDichId))
    expect(giaoDichIds.size).toBe(1)
    const maThaoTacs = new Set(hangCho.map((d) => d.maThaoTac))
    expect(maThaoTacs.size).toBe(3)

    // CUNG mot dau dong ho (vat ly + logic) — mot lan phatDau() duy nhat cho ca lan luu, KHONG phai
    // moi truong mot dau rieng (xem bao cao Task 7 ve quyet dinh nay).
    const vatLys = new Set(hangCho.map((d) => d.vatLy))
    const logics = new Set(hangCho.map((d) => d.logic))
    expect(vatLys.size).toBe(1)
    expect(logics.size).toBe(1)

    db.close()
  })

  it('nhieu truong la MOT giao dich IndexedDB DUY NHAT: hong giua chung thi CA banGhi LAN moi dong hang cho deu khong co gi', async () => {
    const db = await moKho(tenKhoRieng())
    const phuThuoc = await khoiTaoPhuThuoc(db)

    // Ep abort SAU KHI dong hang cho THU HAI da add() thanh cong (con dong thu ba chua kip) — mo
    // phong mat dien giua luc dang ghi nhieu dong. Neu ghiCucBo khong mo MOT giao dich duy nhat cho
    // ca banGhi.put() lan N lan hangCho.add(), kich ban nay se de lai mot phan da ghi (vi du banGhi
    // da co du 3 truong nhung hang cho moi co 2 dong) — dung loi mat du lieu am tham ma chu thich
    // dau khoDuLieu.ts canh bao.
    let soLanAddHangCho = 0
    const addGoc = IDBObjectStore.prototype.add
    const spy = vi.spyOn(IDBObjectStore.prototype, 'add').mockImplementation(function (
      this: IDBObjectStore,
      ...doiSo: Parameters<IDBObjectStore['add']>
    ) {
      const yc = addGoc.apply(this, doiSo)
      if (this.name === KHO_HANG_CHO) {
        soLanAddHangCho += 1
        if (soLanAddHangCho === 2) this.transaction.abort()
      }
      return yc
    })

    await expect(
      ghiCucBo(db, phuThuoc, {
        loai: 'sua',
        bang: 'GiaoDan',
        banGhiId: 'gd-3',
        truong: [
          { truong: 'HoTen', giaTri: 'X' },
          { truong: 'NgaySinh', giaTri: 'Y' },
          { truong: 'DiaChi', giaTri: 'Z' },
        ],
        banGhi: { khoa: 'GiaoDan:gd-3', giaTri: { hoTen: 'X', ngaySinh: 'Y', diaChi: 'Z' } },
      }),
    ).rejects.toThrow()

    spy.mockRestore()
    // RANG BUOC SONG CON: CA HAI ben deu khong co gi — khong phai "banGhi co, hang cho thieu mot phan".
    expect(await docTuKho(db, KHO_BAN_GHI, 'GiaoDan:gd-3')).toBeUndefined()
    expect(await docHangCho(db)).toHaveLength(0)

    db.close()
  })

  it('nem loi ro rang neu goi voi mang truong RONG (loi goi, khong phai truong hop hop le)', async () => {
    const db = await moKho(tenKhoRieng())
    const phuThuoc = await khoiTaoPhuThuoc(db)

    await expect(
      ghiCucBo(db, phuThuoc, {
        loai: 'sua',
        bang: 'GiaoDan',
        banGhiId: 'gd-rong',
        truong: [],
        banGhi: { khoa: 'GiaoDan:gd-rong', giaTri: {} },
      }),
    ).rejects.toThrow()

    db.close()
  })

  it('ghi hai lan lien tiep: dau logic TANG DAN (khong bi dung dau, dung DongHoLogicMayCon that)', async () => {
    const db = await moKho(tenKhoRieng())
    const phuThuoc = await khoiTaoPhuThuoc(db)

    await ghiCucBo(db, phuThuoc, {
      loai: 'sua', bang: 'GiaoDan', banGhiId: 'gd-a',
      truong: [{ truong: 'HoTen', giaTri: 'A' }],
      banGhi: { khoa: 'GiaoDan:gd-a', giaTri: { hoTen: 'A' } },
    })
    await ghiCucBo(db, phuThuoc, {
      loai: 'sua', bang: 'GiaoDan', banGhiId: 'gd-b',
      truong: [{ truong: 'HoTen', giaTri: 'B' }],
      banGhi: { khoa: 'GiaoDan:gd-b', giaTri: { hoTen: 'B' } },
    })

    const hangCho = (await docHangCho(db)) as DongHangChoDongBo[]
    expect(hangCho).toHaveLength(2)
    // Dau thu hai phai TIEN so voi dau thu nhat (vat ly >, hoac vat ly bang va logic tang) — dung
    // phat hien tu chinh DongHoLogicMayCon that (Task 4), khong phai gia lap gio.
    const [d1, d2] = hangCho
    const tienHon = d2.vatLy > d1.vatLy || (d2.vatLy === d1.vatLy && d2.logic > d1.logic)
    expect(tienHon).toBe(true)

    db.close()
  })

  it('dung dung SO DOAN HIEN TAI tai luc ghi (goi qua phuThuoc.doan, khong tu tinh gio tho)', async () => {
    const db = await moKho(tenKhoRieng())
    const phuThuoc = await khoiTaoPhuThuoc(db)
    // Gia lap DoanDongHo da tung mo mot doan (2) — kiem tra ghiCucBo doc dung gia tri nay thay vi
    // luon gan cung 0.
    vi.spyOn(phuThuoc.doan, 'soDoanHienTai').mockResolvedValue(2)

    await ghiCucBo(db, phuThuoc, {
      loai: 'sua', bang: 'GiaoDan', banGhiId: 'gd-doan',
      truong: [{ truong: 'HoTen', giaTri: 'C' }],
      banGhi: { khoa: 'GiaoDan:gd-doan', giaTri: { hoTen: 'C' } },
    })

    const [dong] = (await docHangCho(db)) as DongHangChoDongBo[]
    expect(dong.doan).toBe(2)

    db.close()
  })

  it('I2 (review): soDoanHienTai() PHAI duoc goi TRUOC khi tinh vatLy — mot cu nhay gio phat hien trong buoc do phai duoc phan anh dung', async () => {
    // dongHoMayCon.ts: soDoanHienTai() co tac dung phu neoLai() DongHoDonDieu khi phat hien nhay
    // gio. Neu ghiCucBo lo goi phatDau() (dung donDieu.mocHienTaiMs()) TRUOC khi goi
    // soDoanHienTai(), dong hang cho se mang vatLy o he quy chieu CU (truoc khi neo lai) trong khi
    // lai gan so `doan` MOI — dong do vinh vien khong bao gio duoc hieu chinh dung boi guiMotLo
    // (guiMotLo chi hieu chinh theo doLech cua dung mot doan, gia dinh moi dong trong doan da o
    // dung he quy chieu cua doan do tai luc ghi). Mo phong bang cach cho soDoanHienTai() tu neo lai
    // donDieu vao mot moc THAT XA (nhu DoanDongHo that lam trong linh tinh vong dong bo that), roi
    // kiem tra vatLy ghi ra PHAI phan anh moc MOI, khong phai moc truoc khi neo.
    const db = await moKho(tenKhoRieng())
    const phuThuoc = await khoiTaoPhuThuoc(db)

    const mocMoiMs = Date.now() + 3 * 24 * 60 * 60 * 1000 // +3 ngay — mo phong admin sua gio
    vi.spyOn(phuThuoc.doan, 'soDoanHienTai').mockImplementation(async () => {
      await phuThuoc.donDieu.neoLai(mocMoiMs) // dung nhu DoanDongHo that lam khi phat hien nhay
      return 1
    })

    await ghiCucBo(db, phuThuoc, {
      loai: 'sua', bang: 'GiaoDan', banGhiId: 'gd-nhay-gio',
      truong: [{ truong: 'HoTen', giaTri: 'D' }],
      banGhi: { khoa: 'GiaoDan:gd-nhay-gio', giaTri: { hoTen: 'D' } },
    })

    const [dong] = (await docHangCho(db)) as DongHangChoDongBo[]
    expect(dong.doan).toBe(1)
    // vatLy phai o QUANH moc MOI (+3 ngay), khong phai moc he thong that (chua neo lai) — chenh
    // lech phai gan 3 ngay, khong gan 0.
    const vatLyMs = new Date(dong.vatLy).getTime()
    expect(Math.abs(vatLyMs - mocMoiMs)).toBeLessThan(5000)

    db.close()
  })
  // I4 (fix round cuoi): DongHangChoDongBo co them truong TUY CHON `taiKhoanId` — danh tinh tai
  // khoan da tao ra dong hang cho. Buoc CHUAN BI cho Task 7b (may chu HIEN CHUA dung truong nay).
  it('I4: truyen taiKhoanId -> MOI dong hang cho cua lan luu deu mang dung gia tri do', async () => {
    const db = await moKho(tenKhoRieng())
    const phuThuoc = await khoiTaoPhuThuoc(db)

    await ghiCucBo(db, phuThuoc, {
      loai: 'sua', bang: 'GiaoDan', banGhiId: 'gd-i4',
      truong: [{ truong: 'HoTen', giaTri: 'A' }, { truong: 'NgaySinh', giaTri: '1990-01-01' }],
      banGhi: { khoa: 'GiaoDan:gd-i4', giaTri: { hoTen: 'A' } },
      taiKhoanId: 'cha.an',
    })

    const hangCho = (await docHangCho(db)) as DongHangChoDongBo[]
    expect(hangCho).toHaveLength(2)
    expect(hangCho.every((d) => d.taiKhoanId === 'cha.an')).toBe(true)

    db.close()
  })

  it('I4: KHONG truyen taiKhoanId -> dong hang cho KHONG co truong do (khong bia danh tinh gia)', async () => {
    const db = await moKho(tenKhoRieng())
    const phuThuoc = await khoiTaoPhuThuoc(db)

    await ghiCucBo(db, phuThuoc, {
      loai: 'sua', bang: 'GiaoDan', banGhiId: 'gd-i4b',
      truong: [{ truong: 'HoTen', giaTri: 'B' }],
      banGhi: { khoa: 'GiaoDan:gd-i4b', giaTri: { hoTen: 'B' } },
    })

    const [dong] = (await docHangCho(db)) as DongHangChoDongBo[]
    expect('taiKhoanId' in dong).toBe(false)

    db.close()
  })
})
