// jsdom không cài IndexedDB — polyfill bằng fake-indexeddb (xem kho/moKho.test.ts, cùng khuôn mẫu).
import 'fake-indexeddb/auto'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { moKho, KHO_HANG_CHO } from '../kho/moKho'
import { docConTro, ghiConTro } from '../kho/conTro'
import { ghiSoDaNhan, type DongDaNhan } from '../kho/soDaNhan'
import { docHangCho } from '../kho/hangCho'
import { khoiTaoPhuThuoc, type ToanBoDaGiaiNen } from './boDongBo'
import { xuLyEpochKhongKhop } from './buSauKhoiPhuc'

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

function toanBoDaGiaiNenMau(phanBiet: Partial<ToanBoDaGiaiNen> = {}): ToanBoDaGiaiNen {
  return { epoch: 'epoch-moi', conTro: 4900, chupLuc: '2026-09-15T00:00:00.000Z', duLieu: {}, ...phanBiet }
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
})
