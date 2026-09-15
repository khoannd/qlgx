// jsdom (môi trường test mặc định của dự án — xem vite.config.ts) không cài IndexedDB, nên phải
// giả lập bằng `fake-indexeddb`. Import "/auto" gắn `indexedDB`/`IDBKeyRange`... vào `globalThis`
// đúng như trình duyệt thật, để `moKho.ts` không cần biết gì khác giữa test và chạy thật.
import 'fake-indexeddb/auto'
import { afterEach, describe, expect, it, vi } from 'vitest'
import {
  KHO_BAN_GHI, KHO_CAN_XEM_LAI, KHO_CON_TRO, KHO_HANG_CHO, KHO_SO_DA_NHAN, LoiKhoMoiHonMa, PHIEN_BAN_KHO, moKho,
} from './moKho'

// Mỗi ca kiểm thử dùng một tên kho RIÊNG để không lẫn dữ liệu/kết nối với ca khác (moKho() nhận
// tham số tên/phiên bản tuỳ chọn chính là để phục vụ việc này — xem chú thích trong moKho.ts).
let demTen = 0
function tenKhoRieng(): string {
  demTen += 1
  return `qlgx-test-${demTen}`
}

/** Mở kho ở một phiên bản cụ thể bằng API IndexedDB thô — dùng để TỰ DỰNG kịch bản (kho đã có sẵn
 * ở phiên bản X) trước khi gọi `moKho` thật, mô phỏng những gì đã có sẵn trên máy người dùng. */
function moThoODung(tenKho: string, phienBan: number): Promise<IDBDatabase> {
  return new Promise((resolve, reject) => {
    const yc = indexedDB.open(tenKho, phienBan)
    yc.onupgradeneeded = () => {
      // Tạo đủ 5 kho con như moKho thật, để test không phải quan tâm object store nào có sẵn.
      const db = yc.result
      for (const ten of [KHO_BAN_GHI, KHO_HANG_CHO, KHO_SO_DA_NHAN, KHO_CON_TRO, KHO_CAN_XEM_LAI]) {
        if (!db.objectStoreNames.contains(ten)) db.createObjectStore(ten)
      }
    }
    yc.onsuccess = () => resolve(yc.result)
    yc.onerror = () => reject(yc.error)
  })
}

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

describe('moKho (Task 1 — kho IndexedDB và các ràng buộc mở kho)', () => {
  afterEach(() => {
    vi.restoreAllMocks()
  })

  it('tao du cac kho con', async () => {
    const tenKho = tenKhoRieng()
    const db = await moKho(tenKho)
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

  it('kho MOI HON ma nguon dang chay thi NEM VersionError, tuyet doi khong xoa kho', async () => {
    // Người dùng mở lại bản cũ sau khi service worker đã chạy bản mới, hoặc hai tab lệch phiên
    // bản. Xoá-và-tạo-lại ở đây là mất trắng hàng chờ của cả tuần làm việc.
    const tenKho = tenKhoRieng()

    // Dựng sẵn tình huống: kho trên máy đã ở phiên bản 5, và ĐANG GIỮ một việc trong hàng chờ
    // (mô phỏng hàng chờ chưa gửi lên máy chủ — thứ tuyệt đối không được mất).
    const dbCu = await moThoODung(tenKho, 5)
    await ghiVaoKho(dbCu, KHO_HANG_CHO, 'viec-1', { noiDung: 'ban ghi chua gui' })
    dbCu.close()

    const xoaKho = vi.spyOn(indexedDB, 'deleteDatabase')

    // Mã đang chạy chỉ biết tới phiên bản 1 (thấp hơn 5 đã có trên máy) — đúng kịch bản VersionError.
    await expect(moKho(tenKho, 1)).rejects.toBeInstanceOf(LoiKhoMoiHonMa)

    // Ràng buộc sống còn: KHÔNG BAO GIỜ được gọi indexedDB.deleteDatabase() ở nhánh lỗi này.
    expect(xoaKho).not.toHaveBeenCalled()

    // Và dữ liệu hàng chờ vẫn còn nguyên trên máy sau khi mở lỗi (kho không hề bị đụng tới).
    const dbKiemTra = await moThoODung(tenKho, 5)
    const conLai = await docTuKho(dbKiemTra, KHO_HANG_CHO, 'viec-1')
    dbKiemTra.close()
    expect(conLai).toEqual({ noiDung: 'ban ghi chua gui' })
  })

  it('nang cap phien ban KHONG lam mat hang cho', async () => {
    const tenKho = tenKhoRieng()

    // Mô phỏng máy đã dùng qua bản phiên bản 1 và có một việc trong hàng chờ.
    const dbCu = await moKho(tenKho, 1)
    await ghiVaoKho(dbCu, KHO_HANG_CHO, 'viec-cu', { noiDung: 'viec chua gui truoc khi nang cap' })
    dbCu.close()

    // Mô phỏng một bản phát hành sau tăng PHIEN_BAN_KHO (ở đây dùng 2 thay vì hằng số thật, vì
    // hằng số thật hiện tại là 1 — bài kiểm thử này xác nhận CƠ CHẾ nâng cấp an toàn, không phụ
    // thuộc con số PHIEN_BAN_KHO hiện tại là bao nhiêu).
    const dbMoi = await moKho(tenKho, 2)
    try {
      const conLai = await docTuKho(dbMoi, KHO_HANG_CHO, 'viec-cu')
      expect(conLai).toEqual({ noiDung: 'viec chua gui truoc khi nang cap' })
      // Nâng cấp không được xoá mất kho con nào đã có, kể cả những kho không liên quan trực tiếp.
      expect(dbMoi.objectStoreNames.length).toBe(5)
    } finally {
      dbMoi.close()
    }
  })

  it('mot ket noi khac giu kho o phien ban cu thi moKho() TU CHOI thay vi treo mai', async () => {
    // Quy so mo hai tab QLGX. Tab A dang mo ket noi kho o phien ban cu VA KHONG DONG (vi du tab
    // do dang xu ly do dang lam, hoac don gian chi la mo). Tab B tai ban moi (PHIEN_BAN_KHO tang)
    // va goi moKho() de nang cap. IndexedDB se KHONG chay onupgradeneeded cho toi khi tab A dong
    // ket noi cu — day la `onblocked`. Neu moKho() khong xu ly nhanh nay, promise treo VO THOI
    // HAN: khong resolve, khong reject, man hinh tab B dung loading mai mai.
    const tenKho = tenKhoRieng()

    // Dung san kho o phien ban 1, va GIU MOT KET NOI MO toi kho do — mo phong tab A khong dong.
    const ketNoiCu = await moKho(tenKho, 1)

    // Tab B co PHIEN_BAN_KHO cao hon, se kich hoat nang cap va bi `onblocked` do ketNoiCu con mo.
    const ketQua = moKho(tenKho, 2)

    // NEU dua thang `ketQua` vao `expect(...).rejects...` thi bai test se TREO CHO TOI KHI mot
    // trong hai thu xay ra — mot nguong thoi gian rieng (KHONG duoc phep tu no cung reject, chi
    // duoc RESOLVE ve mot gia tri linh canh) moi phan biet duoc "moKho() tu bat duoc onblocked va
    // reject nhanh" voi "moKho() treo mai, chi vitest timeout mac dinh moi cuu duoc bai test".
    const linhCanhTreo = Symbol('treo')
    const nguongThoiGian = new Promise((giaiQuyet) => setTimeout(() => giaiQuyet(linhCanhTreo), 500))
    const ketQuaCoBatLoi = ketQua.catch((loi: unknown) => loi)

    const aiToiTruoc = await Promise.race([ketQuaCoBatLoi, nguongThoiGian])

    ketNoiCu.close()

    expect(aiToiTruoc).not.toBe(linhCanhTreo)
    expect(aiToiTruoc).toBeInstanceOf(Error)
  })

  it('khong truyen phien ban thi dung dung PHIEN_BAN_KHO da cong bo', async () => {
    // moKho(tenKho) không truyền phiên bản — phải mở đúng bằng PHIEN_BAN_KHO hiện tại, không phải
    // một số cứng nào khác (nếu ai đó sau này tăng PHIEN_BAN_KHO mà quên đổi giá trị mặc định của
    // tham số thứ hai trong moKho.ts, test này sẽ đỏ).
    const mo = vi.spyOn(indexedDB, 'open')
    const tenKho = tenKhoRieng()
    const db = await moKho(tenKho)
    db.close()
    expect(mo).toHaveBeenCalledWith(tenKho, PHIEN_BAN_KHO)
  })
})
