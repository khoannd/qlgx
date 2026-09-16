// jsdom không cài IndexedDB — polyfill bằng fake-indexeddb (xem kho/moKho.test.ts, cùng khuôn mẫu).
import 'fake-indexeddb/auto'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { moKho, KHO_HANG_CHO } from '../kho/moKho'
import { docConTro, ghiConTro } from '../kho/conTro'
import { ghiSoDaNhan, type DongDaNhan } from '../kho/soDaNhan'
import { docHangCho, type DongHangCho } from '../kho/hangCho'
import { khoiTaoPhuThuoc, type ToanBoDaGiaiNen } from './boDongBo'
import {
  LoiDangDungViMayChuDiLui,
  LoiThieuMocXoayEpoch,
  xuLyEpochKhongKhop,
  xuLyEpochKhongKhopDonLuong,
} from './buSauKhoiPhuc'

// `taiToanBoVaGiaiNen` (boDongBo.ts, Task 6) giải nén bằng `new Blob([...]).stream()` +
// `DecompressionStream('gzip')` — cả hai đều tồn tại trong TRÌNH DUYỆT THẬT, nhưng jsdom (môi
// trường test ở đây) KHÔNG cài `Blob.prototype.stream` (đã kiểm chứng riêng:
// `typeof new Blob([]).stream === 'undefined'` trong môi trường này) — gọi hàm thật sẽ ném lỗi
// thuộc về một khoảng trống của MÔI TRƯỜNG TEST (jsdom), không phải lỗi của module Task 8 đang
// kiểm thử ở đây, và cũng không phải lỗi thật của Task 6 (trình duyệt thật có cả hai API này).
// Xem "concern" trong báo cáo Task 8. Mock thẳng `taiToanBoVaGiaiNen` để các test dưới đây chỉ
// kiểm tra ĐÚNG logic của `buSauKhoiPhuc.ts` (lọc/dựng dòng bù/thứ tự sao lưu-nạp/tôn trọng LỚP
// 2) — không phụ thuộc việc giải nén gzip có chạy được trong jsdom hay không (trách nhiệm đã
// đóng của Task 6, ngoài phạm vi module này).
const taiToanBoVaGiaiNenGia = vi.fn<() => Promise<ToanBoDaGiaiNen>>()
vi.mock('./boDongBo', async (importOriginal) => {
  const thuc = await importOriginal<typeof import('./boDongBo')>()
  return { ...thuc, taiToanBoVaGiaiNen: () => taiToanBoVaGiaiNenGia() }
})

let demTen = 0
function tenKhoRieng(): string {
  demTen += 1
  return `qlgx-test-buSauKhoiPhuc-${demTen}`
}

/** Một dòng đã nhận mẫu — đủ hình dạng `DongDaNhan` (kho `soDaNhan`, Task 3). */
function dongDaNhanMau(epoch: string, soThuTu: number, phanBiet: Partial<DongDaNhan> = {}): DongDaNhan {
  return {
    epoch,
    soThuTu,
    bang: 'GiaoDan',
    banGhiId: 'gd-1',
    truong: 'hoTen',
    giaTri: 'Nguyen Van A',
    dongHoVatLy: '2026-09-01T08:00:00.000000Z',
    dongHoLogic: 3,
    thietBiId: 'thiet-bi-khac',
    giaoDichId: 'gdich-goc-1',
    ngayNhan: '2026-09-01T08:00:01.000Z',
    ...phanBiet,
  }
}

/** Mac dinh `soThuTuLucXoayGanNhat` TRUNG voi `conTro` mac dinh (4900) — cac test C2 rieng ben
 * duoi se ghi de CA HAI khac nhau de mo phong dung kich ban "co ai ghi them SAU khi xoay epoch". */
function toanBoDaGiaiNenMau(phanBiet: Partial<ToanBoDaGiaiNen> = {}): ToanBoDaGiaiNen {
  return {
    epoch: 'epoch-moi',
    conTro: 4900,
    chupLuc: '2026-09-15T00:00:00.000Z',
    duLieu: {},
    soThuTuLucXoayGanNhat: 4900,
    ...phanBiet,
  }
}

/** Giả lập việc tải file dự phòng (Task 9, `tepDuPhong.ts`) không đụng ổ đĩa thật: mock
 * `URL.createObjectURL`/`document.createElement('a')`/`click()` — cùng khuôn mẫu
 * `tepDuPhong.test.ts`. Trả về mảng ghi lại các lần `createObjectURL` được gọi (đại diện cho
 * "một lần sao lưu đã xảy ra"), để test có thể xác nhận thứ tự/số lần. */
function moPhongTaiFile(thuTuGoi: string[]): void {
  vi.spyOn(URL, 'createObjectURL').mockImplementation(() => {
    thuTuGoi.push('sao-luu')
    return 'blob:gia'
  })
  vi.spyOn(URL, 'revokeObjectURL').mockImplementation(() => {})
  const aGoc = document.createElement.bind(document)
  vi.spyOn(document, 'createElement').mockImplementation((tag: string) => {
    const el = aGoc(tag)
    if (tag === 'a') vi.spyOn(el, 'click').mockImplementation(() => {})
    return el
  })
}

describe('buSauKhoiPhuc (Task 8 — bu lai du lieu sau khi may chu duoc khoi phuc, spec 4.8.5)', () => {
  afterEach(() => {
    vi.restoreAllMocks()
    taiToanBoVaGiaiNenGia.mockReset()
  })

  it('phat hien qua epoch doi: loc dung cac dong so_thu_tu > so lon nhat moi, dung epoch cu', async () => {
    const db = await moKho(tenKhoRieng())
    const phuThuoc = await khoiTaoPhuThuoc(db)

    // Con tro hien tai cua may nay: epoch CU, da nhan toi so_thu_tu 5000.
    await ghiConTro(db, { epoch: 'epoch-cu', soThuTu: 5000 })

    // So da nhan giu ca cac dong TRUOC va SAU moc soLonNhat=4900 ma may chu bao, VA mot dong o
    // mot epoch khac han (khong lien quan, phai bi loai).
    await ghiSoDaNhan(db, [
      dongDaNhanMau('epoch-cu', 4890, { banGhiId: 'gd-con-tren-may-chu' }),
      dongDaNhanMau('epoch-cu', 4900, { banGhiId: 'gd-dung-bang-moc-khong-mat' }),
      dongDaNhanMau('epoch-cu', 4901, { banGhiId: 'gd-mat-1', giaTri: 'Gia tri mat 1' }),
      dongDaNhanMau('epoch-cu', 4950, { banGhiId: 'gd-mat-2', giaTri: 'Gia tri mat 2' }),
      dongDaNhanMau('epoch-khac-han', 9999, { banGhiId: 'gd-epoch-khac' }),
    ])

    taiToanBoVaGiaiNenGia.mockResolvedValue(toanBoDaGiaiNenMau({ epoch: 'epoch-moi', conTro: 4900 }))
    moPhongTaiFile([])

    const ketQua = await xuLyEpochKhongKhop(phuThuoc)

    expect(ketQua.soDongDaBu).toBe(2)

    const hangCho = await docHangCho(db)
    expect(hangCho).toHaveLength(2)
    const banGhiIds = hangCho.map((d) => (d as unknown as { banGhiId: string }).banGhiId).sort()
    expect(banGhiIds).toEqual(['gd-mat-1', 'gd-mat-2'])

    // Con tro phai duoc dua sang epoch_moi/soLonNhat sau khi bu xong.
    const conTroMoi = await docConTro(db)
    expect(conTroMoi).toMatchObject({ epoch: 'epoch-moi', soThuTu: 4900 })

    db.close()
  })

  it('dang o trang thai dung_do_may_chu_di_lui (LOP 2 Task 6 da phat hien): KHONG tu chay bu lai', async () => {
    const db = await moKho(tenKhoRieng())
    const phuThuoc = await khoiTaoPhuThuoc(db)
    await ghiConTro(db, { epoch: 'epoch-cu', soThuTu: 5000 })
    await ghiSoDaNhan(db, [dongDaNhanMau('epoch-cu', 4901)])

    await expect(xuLyEpochKhongKhop(phuThuoc, () => 'dung_do_may_chu_di_lui')).rejects.toThrow()

    // TU CHOI CHAY, khong chi nem loi suong: khong duoc goi taiToanBoVaGiaiNen (khong "hoi" may
    // chu gi ca), khong duoc dong gi vao hang cho, va conTro phai con NGUYEN (chua bi doi epoch).
    expect(taiToanBoVaGiaiNenGia).not.toHaveBeenCalled()
    expect(await docHangCho(db)).toHaveLength(0)
    expect(await docConTro(db)).toMatchObject({ epoch: 'epoch-cu', soThuTu: 5000 })

    db.close()
  })

  it('luon sao luu file du phong TRUOC khi nap vao hang cho (ca hai kha nang lay_lai/bo_han)', async () => {
    // Module nay KHONG re nhanh theo lay_lai/bo_han — no khong the biet truoc (xem chu thich dau
    // buSauKhoiPhuc.ts). Vi vay CHI CAN mot kich ban chung minh thu tu "sao luu TRUOC khi nap" la
    // du de bao dam ca hai kha nang: ca hai deu di qua DUNG con duong nay, khong co nhanh nao bo
    // qua buoc sao luu.
    const db = await moKho(tenKhoRieng())
    const phuThuoc = await khoiTaoPhuThuoc(db)
    await ghiConTro(db, { epoch: 'epoch-cu', soThuTu: 5000 })
    await ghiSoDaNhan(db, [dongDaNhanMau('epoch-cu', 4901)])

    taiToanBoVaGiaiNenGia.mockResolvedValue(toanBoDaGiaiNenMau({ epoch: 'epoch-moi', conTro: 4900 }))

    const thuTuGoi: string[] = []
    moPhongTaiFile(thuTuGoi)

    // napLaiFileDuPhong ghi vao KHO_HANG_CHO bang IDBObjectStore.put — bat lan PUT DAU TIEN vao
    // dung kho do de biet thoi diem "nap" xay ra.
    const putGoc = IDBObjectStore.prototype.put
    vi.spyOn(IDBObjectStore.prototype, 'put').mockImplementation(function (
      this: IDBObjectStore,
      ...doiSo: Parameters<IDBObjectStore['put']>
    ) {
      if (this.name === KHO_HANG_CHO) thuTuGoi.push('nap')
      return putGoc.apply(this, doiSo)
    })

    await xuLyEpochKhongKhop(phuThuoc)

    expect(thuTuGoi[0]).toBe('sao-luu')
    expect(thuTuGoi).toContain('nap')
    expect(thuTuGoi.indexOf('sao-luu')).toBeLessThan(thuTuGoi.indexOf('nap'))

    db.close()
  })

  it('cac dong bu lai giu NGUYEN vatLy/logic goc, mang dung nguonGocEpoch+nguonGocSoThuTu, KHONG qua phatDau', async () => {
    const db = await moKho(tenKhoRieng())
    const phuThuoc = await khoiTaoPhuThuoc(db)
    await ghiConTro(db, { epoch: 'epoch-cu', soThuTu: 5000 })

    const dongGoc = dongDaNhanMau('epoch-cu', 4901, {
      dongHoVatLy: '2026-08-20T03:04:05.000000Z',
      dongHoLogic: 42,
      giaTri: 'Gia tri lich su',
    })
    await ghiSoDaNhan(db, [dongGoc])

    taiToanBoVaGiaiNenGia.mockResolvedValue(toanBoDaGiaiNenMau({ epoch: 'epoch-moi', conTro: 4900 }))
    moPhongTaiFile([]) // chi can khong nem loi — khong lien quan fact nay

    const spyPhatDau = vi.spyOn(phuThuoc.dongHoLogic, 'phatDau')

    const ketQua = await xuLyEpochKhongKhop(phuThuoc)
    expect(ketQua.soDongDaBu).toBe(1)

    const hangCho = await docHangCho(db)
    expect(hangCho).toHaveLength(1)
    const dong = hangCho[0] as unknown as {
      vatLy: string
      logic: number
      nguonGocEpoch: string
      nguonGocSoThuTu: number
      loai: string
      giaTri: string
    }
    // GIU NGUYEN, khong bi cat/doi dinh dang lai bang mot duong nao khac.
    expect(dong.vatLy).toBe('2026-08-20T03:04:05.000000Z')
    expect(dong.logic).toBe(42)
    expect(dong.giaTri).toBe('Gia tri lich su')
    expect(dong.nguonGocEpoch).toBe('epoch-cu')
    expect(dong.nguonGocSoThuTu).toBe(4901)
    expect(dong.loai).toBe('sua')

    // KHONG qua phatDau — day la su that lich su, khong phai thao tac moi cua may nay.
    expect(spyPhatDau).not.toHaveBeenCalled()

    db.close()
  })

  it('C1 (BAT BUOC, review vong sua 1): dong da nhan co truong === "" (thao tac goc la "tao") duoc bu lai voi loai "tao", KHONG phai "sua"', async () => {
    const db = await moKho(tenKhoRieng())
    const phuThuoc = await khoiTaoPhuThuoc(db)
    await ghiConTro(db, { epoch: 'epoch-cu', soThuTu: 5000 })

    // Mo phong DUNG hinh dang mot dong "tao" thuc su ghi boi ApThaoTacTao (DongBoService.cs): mot
    // dong hieu_luc DUY NHAT voi Truong = "" va GiaTri = JSON CA THUC THE.
    const dongTao = dongDaNhanMau('epoch-cu', 4901, {
      banGhiId: 'gd-moi-1',
      truong: '',
      giaTri: '{"hoTen":"Nguyen Van Moi"}',
    })
    await ghiSoDaNhan(db, [dongTao])

    taiToanBoVaGiaiNenGia.mockResolvedValue(toanBoDaGiaiNenMau({ epoch: 'epoch-moi', conTro: 4900 }))
    moPhongTaiFile([])

    const ketQua = await xuLyEpochKhongKhop(phuThuoc)
    expect(ketQua.soDongDaBu).toBe(1)

    const hangCho = await docHangCho(db)
    const dong = hangCho[0] as unknown as { loai: string; truong: string; giaTri: string }
    expect(dong.loai).toBe('tao')
    expect(dong.truong).toBe('')
    expect(dong.giaTri).toBe('{"hoTen":"Nguyen Van Moi"}')

    db.close()
  })

  it('C1 (review vong sua 1): giu dung THU TU dong "tao" TRUOC dong "sua" cung mot banGhiId trong dongCanBu', async () => {
    const db = await moKho(tenKhoRieng())
    const phuThuoc = await khoiTaoPhuThuoc(db)
    await ghiConTro(db, { epoch: 'epoch-cu', soThuTu: 5000 })

    // "tao" luon co soThuTu NHO HON cac dong "sua" cung mot ban ghi (spec: tao truoc, sua sau) —
    // docTheoKhoang tra ve sap xep tang dan theo soThuTu, va guiHangChoVaApDung (Task 6) giu
    // nguyen thu tu chen trong cung mot doan — nen thu tu nay phai duoc bao toan qua ca duong bu
    // lai (Task 8).
    await ghiSoDaNhan(db, [
      dongDaNhanMau('epoch-cu', 4901, { banGhiId: 'gd-x', truong: '', giaTri: '{"hoTen":"A"}' }),
      dongDaNhanMau('epoch-cu', 4902, { banGhiId: 'gd-x', truong: 'hoTen', giaTri: '"B"' }),
    ])

    taiToanBoVaGiaiNenGia.mockResolvedValue(toanBoDaGiaiNenMau({ epoch: 'epoch-moi', conTro: 4900 }))
    moPhongTaiFile([])

    const ketQua = await xuLyEpochKhongKhop(phuThuoc)
    expect(ketQua.soDongDaBu).toBe(2)

    const hangCho = await docHangCho(db)
    const dong = hangCho as unknown as { loai: string; truong: string; banGhiId: string }[]
    expect(dong).toHaveLength(2)
    expect(dong[0].banGhiId).toBe('gd-x')
    expect(dong[0].loai).toBe('tao')
    expect(dong[0].truong).toBe('')
    expect(dong[1].banGhiId).toBe('gd-x')
    expect(dong[1].loai).toBe('sua')
    expect(dong[1].truong).toBe('hoTen')

    db.close()
  })

  it('I3 (review vong sua 1): hai DongDaNhan CUNG giaoDichId goc (hai truong cua MOT lan luu) dung THANG giaoDichId goc, khac maThaoTac', async () => {
    const db = await moKho(tenKhoRieng())
    const phuThuoc = await khoiTaoPhuThuoc(db)
    await ghiConTro(db, { epoch: 'epoch-cu', soThuTu: 5000 })

    await ghiSoDaNhan(db, [
      dongDaNhanMau('epoch-cu', 4901, {
        banGhiId: 'gd-y', truong: 'hoTen', giaTri: '"A"', giaoDichId: 'gdich-mot-lan-luu',
      }),
      dongDaNhanMau('epoch-cu', 4902, {
        banGhiId: 'gd-y', truong: 'ngaySinh', giaTri: '"2000-01-01"', giaoDichId: 'gdich-mot-lan-luu',
      }),
    ])

    taiToanBoVaGiaiNenGia.mockResolvedValue(toanBoDaGiaiNenMau({ epoch: 'epoch-moi', conTro: 4900 }))
    moPhongTaiFile([])

    await xuLyEpochKhongKhop(phuThuoc)

    const hangCho = await docHangCho(db)
    const dong = hangCho as unknown as { giaoDichId: string; maThaoTac: string }[]
    expect(dong).toHaveLength(2)
    expect(dong[0].giaoDichId).toBe('gdich-mot-lan-luu')
    expect(dong[1].giaoDichId).toBe('gdich-mot-lan-luu')
    expect(dong[0].maThaoTac).not.toBe(dong[1].maThaoTac)

    db.close()
  })

  it('C2 (BAT BUOC, review vong sua 1): dung soThuTuLucXoayGanNhat (KHONG phai conTro) lam nguong loc — bat duoc dong mat nam GIUA hai moc', async () => {
    const db = await moKho(tenKhoRieng())
    const phuThuoc = await khoiTaoPhuThuoc(db)
    await ghiConTro(db, { epoch: 'epoch-cu', soThuTu: 5000 })

    // Mo phong dung kich ban C2: ai do da ghi them SAU khi may chu xoay epoch nhung TRUOC KHI may
    // con nay kip hoi — conTro (con tro HIEN TAI) da tien len 4950, nhung nguong loc DUNG (moc LUC
    // XOAY) van con dung o 4900. Dong 4901-4950 la phan may chu DA MAT that su (nam GIUA hai moc).
    await ghiSoDaNhan(db, [
      dongDaNhanMau('epoch-cu', 4900, { banGhiId: 'gd-khong-mat' }), // <= nguong loc: KHONG mat
      dongDaNhanMau('epoch-cu', 4925, { banGhiId: 'gd-mat-giua-hai-moc' }), // > nguong, <= conTro: MAT THAT
      dongDaNhanMau('epoch-cu', 4950, { banGhiId: 'gd-day-du-conTro' }), // = conTro: van la du lieu MAT
    ])

    taiToanBoVaGiaiNenGia.mockResolvedValue(
      toanBoDaGiaiNenMau({ epoch: 'epoch-moi', conTro: 4950, soThuTuLucXoayGanNhat: 4900 }),
    )
    moPhongTaiFile([])

    const ketQua = await xuLyEpochKhongKhop(phuThuoc)

    // Neu dung NHAM conTro (4950) lam nguong, ca hai dong 4925 VA 4950 se bi loai het (vi
    // docTheoKhoang can so_thu_tu > nguong — dung dung conTro se khong con dong nao > 4950). Dung
    // DUNG nguongLoc (4900) thi CA HAI dong 4925 va 4950 phai duoc bu.
    expect(ketQua.soDongDaBu).toBe(2)
    const hangCho = await docHangCho(db)
    const banGhiIds = (hangCho as unknown as { banGhiId: string }[]).map((d) => d.banGhiId).sort()
    expect(banGhiIds).toEqual(['gd-day-du-conTro', 'gd-mat-giua-hai-moc'])

    // conTro cuc bo van duoc cap nhat theo CON TRO HIEN TAI cua may chu (4950), khong phai
    // nguongLoc — hai gia tri nay phuc vu hai muc dich khac nhau (xem chu thich dau file).
    expect(await docConTro(db)).toMatchObject({ epoch: 'epoch-moi', soThuTu: 4950 })

    db.close()
  })

  it('C2 (review vong sua 1): soThuTuLucXoayGanNhat === null nem LoiThieuMocXoayEpoch, KHONG tu doan nguong 0/conTro', async () => {
    const db = await moKho(tenKhoRieng())
    const phuThuoc = await khoiTaoPhuThuoc(db)
    await ghiConTro(db, { epoch: 'epoch-cu', soThuTu: 5000 })
    await ghiSoDaNhan(db, [dongDaNhanMau('epoch-cu', 4901, { banGhiId: 'gd-mat-1' })])

    taiToanBoVaGiaiNenGia.mockResolvedValue(
      toanBoDaGiaiNenMau({ epoch: 'epoch-moi', conTro: 4900, soThuTuLucXoayGanNhat: null }),
    )
    const thuTuGoi: string[] = []
    moPhongTaiFile(thuTuGoi)

    await expect(xuLyEpochKhongKhop(phuThuoc)).rejects.toBeInstanceOf(LoiThieuMocXoayEpoch)

    // KHONG duoc lam gi ca — khong sao luu, khong dong hang cho, conTro giu nguyen.
    expect(thuTuGoi).toHaveLength(0)
    expect(await docHangCho(db)).toHaveLength(0)
    expect(await docConTro(db)).toMatchObject({ epoch: 'epoch-cu', soThuTu: 5000 })

    db.close()
  })

  it('I4 (review vong sua 1): LOP 2 nem dung LoiDangDungViMayChuDiLui, khong phai Error tran', async () => {
    const db = await moKho(tenKhoRieng())
    const phuThuoc = await khoiTaoPhuThuoc(db)
    await ghiConTro(db, { epoch: 'epoch-cu', soThuTu: 5000 })

    await expect(
      xuLyEpochKhongKhop(phuThuoc, () => 'dung_do_may_chu_di_lui'),
    ).rejects.toBeInstanceOf(LoiDangDungViMayChuDiLui)

    db.close()
  })

  it('I2 (review vong sua 1): dong da co SAN trong hang cho (cung nguonGocEpoch/nguonGocSoThuTu) bi loai, khong bi bu trung', async () => {
    const db = await moKho(tenKhoRieng())
    const phuThuoc = await khoiTaoPhuThuoc(db)
    await ghiConTro(db, { epoch: 'epoch-cu', soThuTu: 5000 })

    await ghiSoDaNhan(db, [
      dongDaNhanMau('epoch-cu', 4901, { banGhiId: 'gd-mat-1' }),
      dongDaNhanMau('epoch-cu', 4902, { banGhiId: 'gd-mat-2' }),
    ])

    // Mot dong bu lai cua 4901 DA CO SAN trong hang cho (vi du mot lan chay TRUOC da nap roi
    // nhung chua kip may chu xac nhan xong).
    const themVaoHangChoTruoc = (dong: Partial<DongHangCho> & Record<string, unknown>): Promise<void> =>
      new Promise((resolve, reject) => {
        const gd = db.transaction(KHO_HANG_CHO, 'readwrite')
        gd.objectStore(KHO_HANG_CHO).add(dong, 1)
        gd.oncomplete = () => resolve()
        gd.onerror = () => reject(gd.error)
      })
    await themVaoHangChoTruoc({
      maThaoTac: 'tt-da-co-san', doan: 0, loai: 'sua', bang: 'GiaoDan', banGhiId: 'gd-mat-1',
      truong: 'hoTen', giaTri: 'x', vatLy: '2026-09-01T00:00:00.000000Z', logic: 0,
      giaoDichId: 'gdich-da-co-san', nguonGocEpoch: 'epoch-cu', nguonGocSoThuTu: 4901,
    })

    taiToanBoVaGiaiNenGia.mockResolvedValue(toanBoDaGiaiNenMau({ epoch: 'epoch-moi', conTro: 4900 }))
    const thuTuGoi: string[] = []
    moPhongTaiFile(thuTuGoi)

    const ketQua = await xuLyEpochKhongKhop(phuThuoc)

    // CHI dong 4902 (chua co trong hang cho) duoc bu them — 4901 bi loai vi da co san.
    expect(ketQua.soDongDaBu).toBe(1)
    const hangCho = await docHangCho(db)
    expect(hangCho).toHaveLength(2) // dong cu (4901) + dong moi bu (4902)
    const banGhiIds = (hangCho as unknown as { banGhiId: string }[]).map((d) => d.banGhiId).sort()
    expect(banGhiIds).toEqual(['gd-mat-1', 'gd-mat-2'])
    // Van co sao luu (vi con it nhat mot dong moi de bu).
    expect(thuTuGoi).toContain('sao-luu')

    db.close()
  })

  it('I2 (review vong sua 1): MOI dong can bu DEU da co san trong hang cho — bo qua han sao luu, tra ve 0', async () => {
    const db = await moKho(tenKhoRieng())
    const phuThuoc = await khoiTaoPhuThuoc(db)
    await ghiConTro(db, { epoch: 'epoch-cu', soThuTu: 5000 })
    await ghiSoDaNhan(db, [dongDaNhanMau('epoch-cu', 4901, { banGhiId: 'gd-mat-1' })])

    const themVaoHangChoTruoc = (dong: Record<string, unknown>): Promise<void> =>
      new Promise((resolve, reject) => {
        const gd = db.transaction(KHO_HANG_CHO, 'readwrite')
        gd.objectStore(KHO_HANG_CHO).add(dong, 1)
        gd.oncomplete = () => resolve()
        gd.onerror = () => reject(gd.error)
      })
    await themVaoHangChoTruoc({
      maThaoTac: 'tt-da-co-san', doan: 0, loai: 'sua', bang: 'GiaoDan', banGhiId: 'gd-mat-1',
      truong: 'hoTen', giaTri: 'x', vatLy: '2026-09-01T00:00:00.000000Z', logic: 0,
      giaoDichId: 'gdich-da-co-san', nguonGocEpoch: 'epoch-cu', nguonGocSoThuTu: 4901,
    })

    taiToanBoVaGiaiNenGia.mockResolvedValue(toanBoDaGiaiNenMau({ epoch: 'epoch-moi', conTro: 4900 }))
    const thuTuGoi: string[] = []
    moPhongTaiFile(thuTuGoi)

    const ketQua = await xuLyEpochKhongKhop(phuThuoc)

    expect(ketQua.soDongDaBu).toBe(0)
    expect(thuTuGoi).toHaveLength(0) // KHONG tai file du phong khi khong con gi moi de bu.
    expect(await docHangCho(db)).toHaveLength(1) // dong cu van con nguyen, khong nhan them.
    expect(await docConTro(db)).toMatchObject({ epoch: 'epoch-moi', soThuTu: 4900 })

    db.close()
  })

  it('I2 (review vong sua 1): xuLyEpochKhongKhopDonLuong goi hai lan gan nhu dong thoi — chi MOT lan tai file du phong, hang cho cuoi khong trung', async () => {
    const db = await moKho(tenKhoRieng())
    const phuThuoc = await khoiTaoPhuThuoc(db)
    await ghiConTro(db, { epoch: 'epoch-cu', soThuTu: 5000 })
    await ghiSoDaNhan(db, [dongDaNhanMau('epoch-cu', 4901, { banGhiId: 'gd-mat-1' })])

    taiToanBoVaGiaiNenGia.mockResolvedValue(toanBoDaGiaiNenMau({ epoch: 'epoch-moi', conTro: 4900 }))
    const thuTuGoi: string[] = []
    moPhongTaiFile(thuTuGoi)

    // Hai loi goi GAN NHU DONG THOI (khong await lan dau truoc khi goi lan hai) — mo phong hai
    // LoiEpochKhongKhop bi bat gan nhu dong thoi trong cung mot chu ky (vd tu /thay-doi VA
    // /gui-len, xem chu thich C3 trong boDongBo.ts).
    const [ketQua1, ketQua2] = await Promise.all([
      xuLyEpochKhongKhopDonLuong(phuThuoc),
      xuLyEpochKhongKhopDonLuong(phuThuoc),
    ])

    // Ca hai deu nhan lai DUNG MOT ket qua (tai su dung cung mot Promise).
    expect(ketQua1).toEqual(ketQua2)
    expect(ketQua1.soDongDaBu).toBe(1)

    // CHI mot lan sao luu duoc thuc hien, du goi ham hai lan.
    expect(thuTuGoi.filter((g) => g === 'sao-luu')).toHaveLength(1)

    // Hang cho cuoi cung KHONG co dong trung (nguonGocEpoch, nguonGocSoThuTu).
    const hangCho = await docHangCho(db)
    const danhTinh = (hangCho as unknown as { nguonGocEpoch: string; nguonGocSoThuTu: number }[]).map(
      (d) => `${d.nguonGocEpoch}:${d.nguonGocSoThuTu}`,
    )
    expect(new Set(danhTinh).size).toBe(danhTinh.length)

    db.close()
  })
})
