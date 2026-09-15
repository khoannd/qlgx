// jsdom không cài IndexedDB — polyfill bằng fake-indexeddb (xem kho/moKho.test.ts, cùng khuôn mẫu).
import 'fake-indexeddb/auto'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { moKho } from '../kho/moKho'
import { docHangCho, type DongHangCho } from '../kho/hangCho'
import {
  canTuDongDuPhong,
  docNoiDungFileDuPhong,
  napLaiFileDuPhong,
  taiFileDuPhongXuong,
  taoNoiDungFileDuPhong,
} from './tepDuPhong'

let demTen = 0
function tenKhoRieng(): string {
  demTen += 1
  return `qlgx-test-tepDuPhong-${demTen}`
}

function dongMau(maThaoTac: string, phanConLai: Partial<DongHangCho> = {}): DongHangCho {
  return { maThaoTac, doan: 0, ...phanConLai }
}

describe('taoNoiDungFileDuPhong / docNoiDungFileDuPhong (Task 9 — vong tron JSON)', () => {
  it('doc lai dung nguyen mang hang cho da dua vao (round-trip)', () => {
    const hangCho: DongHangCho[] = [
      dongMau('tt-1', { bang: 'GiaoDan', giaTri: 'A' }),
      dongMau('tt-2', { vatLy: '2026-09-01T00:00:00.000Z' }),
    ]
    const noiDung = taoNoiDungFileDuPhong(hangCho)
    const docLai = docNoiDungFileDuPhong(noiDung)
    expect(docLai).toEqual(hangCho)
  })

  it('noi dung file la JSON co boc phienBanFile/taoLuc, khong phai mang tho', () => {
    const noiDung = taoNoiDungFileDuPhong([dongMau('tt-1')])
    const obj = JSON.parse(noiDung) as { phienBanFile: unknown; taoLuc: unknown; hangCho: unknown }
    expect(obj.phienBanFile).toBe(1)
    expect(typeof obj.taoLuc).toBe('string')
    // taoLuc phai la mot ISO timestamp doc duoc
    expect(Number.isNaN(Date.parse(obj.taoLuc as string))).toBe(false)
    expect(obj.hangCho).toEqual([{ maThaoTac: 'tt-1', doan: 0 }])
  })

  it('mang hang cho rong van tao va doc lai duoc, khong nem loi', () => {
    const noiDung = taoNoiDungFileDuPhong([])
    expect(docNoiDungFileDuPhong(noiDung)).toEqual([])
  })

  describe('file sai dinh dang bi tu choi RO RANG, khong am tham tra rong', () => {
    it('chuoi khong phai JSON', () => {
      expect(() => docNoiDungFileDuPhong('day khong phai json {{{')).toThrow()
    })

    it('JSON hop le nhung khong phai object (vi du mot con so)', () => {
      expect(() => docNoiDungFileDuPhong('42')).toThrow()
    })

    it('JSON la object nhung thieu truong "hangCho"', () => {
      expect(() => docNoiDungFileDuPhong(JSON.stringify({ phienBanFile: 1, taoLuc: 'x' }))).toThrow()
    })

    it('truong "hangCho" khong phai mang', () => {
      // Kiem tra dung THONG DIEP loi (khong chi "co nem loi") — mot doi tuong khong phai mang
      // nhung tinh co co san mot phuong thuc "forEach" (khong xay ra tu nhien voi JSON.parse, chi
      // gia lap de kiem chung) se KHONG tu nem loi neu thieu han kiem tra Array.isArray tuong minh.
      expect(() => docNoiDungFileDuPhong(JSON.stringify({ hangCho: 'khong-phai-mang' }))).toThrow(/mảng/)

      const gia = { hangCho: { forEach: () => {} } } as unknown as { hangCho: unknown }
      expect(() => docNoiDungFileDuPhong(JSON.stringify(gia))).toThrow(/mảng/)
    })

    it('mot dong trong "hangCho" thieu maThaoTac', () => {
      const vanBan = JSON.stringify({ hangCho: [{ doan: 0 }] })
      expect(() => docNoiDungFileDuPhong(vanBan)).toThrow()
    })

    it('mot dong trong "hangCho" co maThaoTac rong', () => {
      const vanBan = JSON.stringify({ hangCho: [{ maThaoTac: '', doan: 0 }] })
      expect(() => docNoiDungFileDuPhong(vanBan)).toThrow()
    })

    it('mot dong trong "hangCho" thieu doan (hoac sai kieu)', () => {
      const vanBan = JSON.stringify({ hangCho: [{ maThaoTac: 'tt-1', doan: 'khong-phai-so' }] })
      expect(() => docNoiDungFileDuPhong(vanBan)).toThrow()
    })

    it('mot dong trong "hangCho" khong phai object (vi du mot chuoi)', () => {
      const vanBan = JSON.stringify({ hangCho: ['khong-phai-object'] })
      expect(() => docNoiDungFileDuPhong(vanBan)).toThrow()
    })

    // Khong am tham tra mang rong o BAT KY nhanh loi nao o tren — xac nhan rieng cho nhanh de
    // nham lan nhat: nguoi dung chon nham mot file JSON hop le khac (vi du file cau hinh khac cua
    // may) khong lien quan gi toi hang cho.
    it('JSON hop le cua mot thu khac han (vi du { "cauHinh": true }) van bi tu choi, khong tra []', () => {
      let ketQua: DongHangCho[] | undefined
      let loiNem: unknown
      try {
        ketQua = docNoiDungFileDuPhong(JSON.stringify({ cauHinh: true }))
      } catch (loi) {
        loiNem = loi
      }
      expect(ketQua).toBeUndefined()
      expect(loiNem).toBeInstanceOf(Error)
    })
  })
})

describe('taiFileDuPhongXuong (Task 9 — kich tai file qua Blob + <a>)', () => {
  afterEach(() => {
    vi.restoreAllMocks()
  })

  it('tao Blob JSON dung noi dung, dat ten file dung mau qlgx-du-phong-<ISO>.json, click roi thu hoi URL', async () => {
    let blobDaTao: Blob | undefined
    let tenTepDaTai: string | undefined
    const click = vi.fn()

    vi.spyOn(URL, 'createObjectURL').mockImplementation((obj: Blob | MediaSource) => {
      blobDaTao = obj as Blob
      return 'blob:gia-lap'
    })
    const thuHoi = vi.spyOn(URL, 'revokeObjectURL').mockImplementation(() => {})
    const guiTao = document.createElement.bind(document)
    vi.spyOn(document, 'createElement').mockImplementation((tag: string) => {
      const el = guiTao(tag) as HTMLAnchorElement
      if (tag === 'a') {
        el.click = click
        Object.defineProperty(el, 'download', { get: () => tenTepDaTai, set: (v) => { tenTepDaTai = v } })
      }
      return el
    })

    taiFileDuPhongXuong([dongMau('tt-1')])

    expect(click).toHaveBeenCalledOnce()
    expect(tenTepDaTai).toMatch(/^qlgx-du-phong-.*\.json$/)
    // Khong con dau ':' (Windows/NTFS khong cho phep trong ten file)
    expect(tenTepDaTai).not.toContain(':')
    expect(thuHoi).toHaveBeenCalledWith('blob:gia-lap')

    expect(blobDaTao).toBeInstanceOf(Blob)
    const noiDung = JSON.parse(await blobDaTao!.text()) as { hangCho: unknown }
    expect(noiDung.hangCho).toEqual([{ maThaoTac: 'tt-1', doan: 0 }])
  })
})

describe('napLaiFileDuPhong (Task 9 — nap tro lai vao kho hangCho)', () => {
  it('ghi dung cac dong tu file vao hang cho, giu nguyen thu tu', async () => {
    const db = await moKho(tenKhoRieng())
    await napLaiFileDuPhong(db, [dongMau('tt-1'), dongMau('tt-2'), dongMau('tt-3')])

    const doc = await docHangCho(db)
    expect(doc.map((d) => d.maThaoTac)).toEqual(['tt-1', 'tt-2', 'tt-3'])
    db.close()
  })

  it('mang rong khong lam gi, khong nem loi', async () => {
    const db = await moKho(tenKhoRieng())
    await expect(napLaiFileDuPhong(db, [])).resolves.toBeUndefined()
    expect(await docHangCho(db)).toEqual([])
    db.close()
  })

  it('nap lai file KHONG tao ban trung: maThaoTac duoc giu nguyen tu file, khong sinh moi', async () => {
    const db = await moKho(tenKhoRieng())
    const tuFile = [dongMau('tt-giu-nguyen-1'), dongMau('tt-giu-nguyen-2')]
    await napLaiFileDuPhong(db, tuFile)

    const doc = await docHangCho(db)
    // Dung ĐUNG maThaoTac da co trong file — khong phai mot chuoi moi duoc sinh ra.
    expect(doc.map((d) => d.maThaoTac)).toEqual(['tt-giu-nguyen-1', 'tt-giu-nguyen-2'])
    db.close()
  })

  it('nap vao mot hang cho DANG CO san du lieu se noi tiep dung thu tu, khong de dong moi trung khoa voi dong cu', async () => {
    // Day chinh la bay tai lieu trong tepDuPhong.ts: goi voiKhoaKeTiep MOI LAN trong vong lap se
    // khien moi dong nhan trung mot khoa. Kiem tra gian tiep qua thu tu doc lai — neu khoa trung,
    // IndexedDB se GHI DE thay vi giu ca ba dong.
    const db = await moKho(tenKhoRieng())
    await napLaiFileDuPhong(db, [dongMau('tt-cu-1')])
    await napLaiFileDuPhong(db, [dongMau('tt-moi-1'), dongMau('tt-moi-2')])

    const doc = await docHangCho(db)
    expect(doc.map((d) => d.maThaoTac)).toEqual(['tt-cu-1', 'tt-moi-1', 'tt-moi-2'])
    db.close()
  })
})

describe('canTuDongDuPhong (Task 9 — quyet dinh > 2 ngay HOAC > 20 viec, huong (a))', () => {
  it('hang cho rong: khong bao gio can du phong', () => {
    expect(canTuDongDuPhong([], null)).toBe(false)
  })

  it('hang cho it viec, vatLy moi: khong can du phong', () => {
    const hangCho = [dongMau('tt-1', { vatLy: new Date().toISOString() })]
    expect(canTuDongDuPhong(hangCho, null)).toBe(false)
  })

  it('hang cho co dong vatLy cu hon 2 ngay: CAN du phong', () => {
    const baNgayTruoc = new Date(Date.now() - 3 * 24 * 60 * 60 * 1000).toISOString()
    const hangCho = [dongMau('tt-1', { vatLy: baNgayTruoc })]
    expect(canTuDongDuPhong(hangCho, null)).toBe(true)
  })

  it('hang cho co dong vatLy CHUA du 2 ngay (vi du 1 ngay 23 gio): CHUA can du phong', () => {
    const gioSap2Ngay = new Date(Date.now() - (2 * 24 * 60 * 60 * 1000 - 60 * 60 * 1000)).toISOString()
    const hangCho = [dongMau('tt-1', { vatLy: gioSap2Ngay })]
    expect(canTuDongDuPhong(hangCho, null)).toBe(false)
  })

  it('bien DUNG 2 ngay (khong VUOT qua): CHUA can du phong — dung Date.now() dong bang de so sanh chinh xac', () => {
    // Muc dich: phan biet dieu kien "> 2 ngay" (dung) voi ">= 2 ngay" (sai, sat qua ngay bien) —
    // dong bang thoi gian bang vi.setSystemTime de tuoi cua dong hang cho dung CHINH XAC bang
    // HAI_NGAY_MS, khong lech mot chut nao do thoi gian thuc troi qua giua luc tao du lieu va luc
    // canTuDongDuPhong doc Date.now().
    vi.useFakeTimers()
    try {
      const baseMs = Date.parse('2026-09-15T00:00:00.000Z')
      vi.setSystemTime(baseMs)
      const dungHaiNgayTruoc = new Date(baseMs - 2 * 24 * 60 * 60 * 1000).toISOString()
      const hangCho = [dongMau('tt-1', { vatLy: dungHaiNgayTruoc })]
      expect(canTuDongDuPhong(hangCho, null)).toBe(false)
    } finally {
      vi.useRealTimers()
    }
  })

  it('hang cho tren 20 viec du vatLy deu moi: CAN du phong', () => {
    const hangCho = Array.from({ length: 21 }, (_, i) => dongMau(`tt-${i}`, { vatLy: new Date().toISOString() }))
    expect(canTuDongDuPhong(hangCho, null)).toBe(true)
  })

  it('dung 20 viec (khong VUOT qua nguong) va vatLy moi: CHUA can du phong', () => {
    const hangCho = Array.from({ length: 20 }, (_, i) => dongMau(`tt-${i}`, { vatLy: new Date().toISOString() }))
    expect(canTuDongDuPhong(hangCho, null)).toBe(false)
  })

  it('cac dong khong co vatLy hop le: dieu kien "qua 2 ngay" khong tu kich hoat, chi con "qua 20 viec"', () => {
    const hangCho = [dongMau('tt-1'), dongMau('tt-2')] // khong gan vatLy
    expect(canTuDongDuPhong(hangCho, null)).toBe(false)

    const hangCho21 = Array.from({ length: 21 }, (_, i) => dongMau(`tt-${i}`))
    expect(canTuDongDuPhong(hangCho21, null)).toBe(true)
  })

  it('bien DUNG 2 ngay ke tu lan du phong truoc: da het han "ham", CAN du phong lai — dong bang thoi gian de so sanh chinh xac', () => {
    // Phan biet dieu kien ham "< 2 ngay" (dung) voi "<= 2 ngay" (sai, giu ham dai them dung 1 khoanh
    // khac o bien) — dong bang thoi gian de khoang cach ke tu lanDuPhongGanNhatIso dung CHINH XAC
    // bang HAI_NGAY_MS.
    vi.useFakeTimers()
    try {
      const baseMs = Date.parse('2026-09-15T00:00:00.000Z')
      vi.setSystemTime(baseMs)
      const baNgayTruoc = new Date(baseMs - 3 * 24 * 60 * 60 * 1000).toISOString()
      const hangCho = [dongMau('tt-1', { vatLy: baNgayTruoc })]
      const dungHaiNgayTruoc = new Date(baseMs - 2 * 24 * 60 * 60 * 1000).toISOString()
      expect(canTuDongDuPhong(hangCho, dungHaiNgayTruoc)).toBe(true)
    } finally {
      vi.useRealTimers()
    }
  })

  it('da du phong gan day (trong vong 2 ngay): KHONG doi hoi tai lai ngay, du dieu kien ton dong van dung', () => {
    const baNgayTruoc = new Date(Date.now() - 3 * 24 * 60 * 60 * 1000).toISOString()
    const hangCho = [dongMau('tt-1', { vatLy: baNgayTruoc })]
    const motGioTruoc = new Date(Date.now() - 60 * 60 * 1000).toISOString()
    expect(canTuDongDuPhong(hangCho, motGioTruoc)).toBe(false)
  })

  it('lan du phong gan nhat da QUA 2 ngay: van CAN du phong lai neu dieu kien ton dong con dung', () => {
    const baNgayTruoc = new Date(Date.now() - 3 * 24 * 60 * 60 * 1000).toISOString()
    const hangCho = [dongMau('tt-1', { vatLy: baNgayTruoc })]
    const baNgayRuoiTruoc = new Date(Date.now() - 3.5 * 24 * 60 * 60 * 1000).toISOString()
    expect(canTuDongDuPhong(hangCho, baNgayRuoiTruoc)).toBe(true)
  })

  it('lanDuPhongGanNhatIso la chuoi khong hop le: coi nhu chua tung du phong, van CAN neu ton dong', () => {
    const baNgayTruoc = new Date(Date.now() - 3 * 24 * 60 * 60 * 1000).toISOString()
    const hangCho = [dongMau('tt-1', { vatLy: baNgayTruoc })]
    expect(canTuDongDuPhong(hangCho, 'khong-phai-ngay-thang')).toBe(true)
  })

  it('I1 (review): dong ho he thong bi LUI sau lan du phong truoc KHONG duoc phep khoa "ham" vinh vien', () => {
    // Kich ban that: dat sai ngay, RTC hong, cai lai Windows... khien Date.now() hien tai NHO HON
    // lanDuPhongGanNhatIso da luu. `Date.now() - Date.parse(lanDuPhongGanNhatIso)` ra AM — mot so
    // am LUON nho hon HAI_NGAY_MS, neu khong loai truong hop nay rieng thi nhanh "ham" se coi day
    // la "con trong 2 ngay ke tu lan du phong truoc" MAI MAI, chan vinh vien CA dieu kien
    // quaSoLuong (von khong he phu thuoc dong ho). Dung 21 dong KHONG co vatLy (chi con dieu kien
    // qua-20-viec co the kich hoat, loai het anh huong cua quaThoiGian) de co lap dung nguyen nhan.
    const lanDuPhongGanNhat = new Date(Date.now() + 5 * 24 * 60 * 60 * 1000).toISOString() // "trong tuong lai" so voi bay gio -> mo phong dong ho da bi LUI ve sau do
    const hangCho21 = Array.from({ length: 21 }, (_, i) => dongMau(`tt-${i}`)) // khong co vatLy
    expect(canTuDongDuPhong(hangCho21, lanDuPhongGanNhat)).toBe(true)
  })
})
