// jsdom không cài IndexedDB — polyfill bằng fake-indexeddb (xem moKho.test.ts, cùng khuôn mẫu).
import 'fake-indexeddb/auto'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { moKho, KHO_HANG_CHO } from '../kho/moKho'
import { docHangCho, type DongHangCho } from '../kho/hangCho'
import { docConTro } from '../kho/conTro'
import { docCanXemLai } from '../kho/canXemLai'
import { docTheoKhoang } from '../kho/soDaNhan'
import {
  batDauBoDongBo,
  dauTuDongHieuLuc,
  guiHangChoVaApDung,
  khoiTaoPhuThuoc,
  layThietBiId,
  motLanDongBo,
  CHU_KY_DANG_GO_MS,
  CHU_KY_YEN_TINH_MS,
  LoiEpochKhongKhop,
  LoiMayChuDiLui,
  type DongHieuLucDto,
  type DongHangChoDongBo,
  type GuiLenKetQua,
  type GuiLenYeuCau,
  type NguonSuKienDongBo,
} from './boDongBo'
import type { NguonKhoa } from './bauChu'

let demTen = 0
function tenKhoRieng(): string {
  demTen += 1
  return `qlgx-test-boDongBo-${demTen}`
}

/** Một dòng hàng chờ mẫu, đủ hình dạng `DongHangChoDongBo` — dùng lại trong nhiều test. */
function dongMau(maThaoTac: string, phanBiet: Partial<DongHangChoDongBo> = {}): DongHangChoDongBo {
  return {
    maThaoTac,
    doan: 0,
    loai: 'sua',
    bang: 'GiaoDan',
    banGhiId: 'gd-1',
    truong: 'hoTen',
    giaTri: 'Nguyen Van A',
    vatLy: '2026-09-15T10:00:00.000000Z',
    logic: 0,
    giaoDichId: 'gdich-1',
    ...phanBiet,
  }
}

/** Ghi thẳng một dòng vào hàng chờ mà không qua `ghiVaXepHang` (không cần bản ghi hiển thị đi kèm
 * cho các test ở đây, vốn chỉ quan tâm hành vi của `boDongBo`, không phải `khoDuLieu.ts`). */
function themVaoHangCho(db: IDBDatabase, dong: DongHangCho): Promise<void> {
  return new Promise((resolve, reject) => {
    const gd = db.transaction(KHO_HANG_CHO, 'readwrite')
    // Khoá tăng dần tự quản lý (xem hangCho.ts) — dùng số ngẫu nhiên đủ lớn để không đụng khoá đã
    // có trong các test khác dùng chung một kho (mỗi test đã có kho riêng qua tenKhoRieng() nên
    // không thật sự cần, nhưng an toàn hơn khi thêm nhiều dòng trong CÙNG một test).
    gd.objectStore(KHO_HANG_CHO).add(dong, Date.now() + Math.random())
    gd.oncomplete = () => resolve()
    gd.onerror = () => reject(gd.error)
  })
}

function dongHieuLucMau(soThuTu: number, phanBiet: Partial<DongHieuLucDto> = {}): DongHieuLucDto {
  return {
    soThuTu,
    bang: 'GiaoDan',
    banGhiId: 'gd-2',
    truong: 'hoTen',
    giaTri: 'Tran Thi B',
    dongHoVatLy: '2026-09-15T09:00:00Z',
    dongHoLogic: 5,
    thietBiId: 'thiet-bi-khac',
    giaoDichId: 'gdich-nhan-1',
    ...phanBiet,
  }
}

function ketQuaGuiLenMau(phanBiet: Partial<GuiLenKetQua> = {}): GuiLenKetQua {
  return { epoch: 'epoch-1', conTroMoi: 10, conNua: false, ketQua: [], dongMoi: [], ...phanBiet }
}

describe('boDongBo (Task 6 — bộ đồng bộ)', () => {
  beforeEach(() => {
    localStorage.clear()
    vi.restoreAllMocks()
  })
  afterEach(() => {
    vi.unstubAllGlobals()
    vi.restoreAllMocks()
  })

  describe('layThietBiId', () => {
    it('sinh mot lan roi luu lai, doc lai cung mot gia tri', () => {
      const id1 = layThietBiId()
      expect(id1).toMatch(/^[0-9a-f-]{36}$/)
      expect(localStorage.getItem('qlgx.thietBiId')).toBe(id1)
    })
  })

  describe('guiHangChoVaApDung — RANG BUOC 9: gui lai lo dung LAI maThaoTac cu, khong sinh moi', () => {
    it('gui hai lan lien tiep (mo phong gui lai do mang loi giua chung) mang DUNG mot maThaoTac cho ca hai lan', async () => {
      const db = await moKho(tenKhoRieng())
      const phuThuoc = await khoiTaoPhuThuoc(db)
      const dong = dongMau('tt-co-dinh')
      await themVaoHangCho(db, dong)

      const thanGuiDi: GuiLenYeuCau[] = []
      const fetchGia = vi.fn(async (_url: string, tuyChon?: RequestInit) => {
        thanGuiDi.push(JSON.parse(String(tuyChon?.body)) as GuiLenYeuCau)
        // LAN GUI DAU: mo phong mat mang giua chung — hang cho VAN CON nguyen (khong bi xoa).
        if (thanGuiDi.length === 1) throw new TypeError('network error')
        return new Response(JSON.stringify(ketQuaGuiLenMau({ ketQua: [{ maThaoTac: 'tt-co-dinh', ketQua: 'ap', thongBao: null }] })), {
          status: 200,
        })
      })
      vi.stubGlobal('fetch', fetchGia)

      // Lan 1: that bai vi mat mang — hang cho van con nguyen dong cu.
      const hangChoLan1 = (await docHangCho(db)) as DongHangChoDongBo[]
      await expect(guiHangChoVaApDung(phuThuoc, hangChoLan1)).rejects.toThrow()

      // Lan 2: doc lai hang cho (van con dong CU, chua bi xoa vi lan 1 that bai) roi gui lai.
      const hangChoLan2 = (await docHangCho(db)) as DongHangChoDongBo[]
      expect(hangChoLan2).toHaveLength(1)
      await guiHangChoVaApDung(phuThuoc, hangChoLan2)

      expect(thanGuiDi).toHaveLength(2)
      // CA HAI lan gui deu mang DUNG mot maThaoTac — khong sinh moi khi gui lai.
      expect(thanGuiDi[0].thaoTac[0].maThaoTac).toBe('tt-co-dinh')
      expect(thanGuiDi[1].thaoTac[0].maThaoTac).toBe('tt-co-dinh')
      expect(thanGuiDi[0].thaoTac[0].maThaoTac).toBe(thanGuiDi[1].thaoTac[0].maThaoTac)

      // Sau khi thanh cong, hang cho phai rong (dung MOT dong duoc ap, khong phai hai).
      expect(await docHangCho(db)).toHaveLength(0)

      db.close()
    })
  })

  describe('motLanDongBo — tab an van gui hang cho, nhung ngung hoi du lieu moi khi hang cho rong', () => {
    it('tab an + hang cho CO gi: van goi mang de gui', async () => {
      const db = await moKho(tenKhoRieng())
      const phuThuoc = await khoiTaoPhuThuoc(db)
      await themVaoHangCho(db, dongMau('tt-1'))

      const fetchGia = vi.fn(async () => new Response(JSON.stringify(ketQuaGuiLenMau()), { status: 200 }))
      vi.stubGlobal('fetch', fetchGia)

      await motLanDongBo(phuThuoc, /* tabAn */ true)

      expect(fetchGia).toHaveBeenCalledTimes(1)
      db.close()
    })

    it('tab an + hang cho RONG: khong goi mang (ngung hoi du lieu moi)', async () => {
      const db = await moKho(tenKhoRieng())
      const phuThuoc = await khoiTaoPhuThuoc(db)

      const fetchGia = vi.fn(async () => new Response(JSON.stringify(ketQuaGuiLenMau()), { status: 200 }))
      vi.stubGlobal('fetch', fetchGia)

      await motLanDongBo(phuThuoc, /* tabAn */ true)

      expect(fetchGia).not.toHaveBeenCalled()
      db.close()
    })

    it('tab HIEN + hang cho RONG: van goi mang (hoi du lieu moi mien phi qua GuiLen rong)', async () => {
      const db = await moKho(tenKhoRieng())
      const phuThuoc = await khoiTaoPhuThuoc(db)

      const fetchGia = vi.fn(async () => new Response(JSON.stringify(ketQuaGuiLenMau()), { status: 200 }))
      vi.stubGlobal('fetch', fetchGia)

      await motLanDongBo(phuThuoc, /* tabAn */ false)

      expect(fetchGia).toHaveBeenCalledTimes(1)
      db.close()
    })
  })

  describe('ket qua tu_choi: tao DUNG MOT muc can xem lai VA xoa khoi hang cho', () => {
    it('tu_choi: xoa khoi hang cho, ghi vao can xem lai voi dung thongBao va dong hang cho goc', async () => {
      const db = await moKho(tenKhoRieng())
      const phuThuoc = await khoiTaoPhuThuoc(db)
      const dong = dongMau('tt-tu-choi', { truong: 'ngaySinh', giaTri: '1999-99-99' })
      await themVaoHangCho(db, dong)

      const fetchGia = vi.fn(
        async () =>
          new Response(
            JSON.stringify(
              ketQuaGuiLenMau({
                ketQua: [{ maThaoTac: 'tt-tu-choi', ketQua: 'tu_choi', thongBao: 'Ngay sinh khong hop le' }],
              }),
            ),
            { status: 200 },
          ),
      )
      vi.stubGlobal('fetch', fetchGia)

      const hangCho = (await docHangCho(db)) as DongHangChoDongBo[]
      await guiHangChoVaApDung(phuThuoc, hangCho)

      // Xoa khoi hang cho.
      expect(await docHangCho(db)).toHaveLength(0)

      // Dung MOT muc can xem lai, giu nguyen dong hang cho goc.
      const canXemLai = await docCanXemLai(db)
      expect(canXemLai).toHaveLength(1)
      expect(canXemLai[0].maThaoTac).toBe('tt-tu-choi')
      expect(canXemLai[0].thongBao).toBe('Ngay sinh khong hop le')
      expect(canXemLai[0].dongHangChoGoc).toEqual(dong)

      db.close()
    })

    it('ket qua "thua" KHONG tao muc can xem lai (la ket qua gop binh thuong, khong phai loi)', async () => {
      const db = await moKho(tenKhoRieng())
      const phuThuoc = await khoiTaoPhuThuoc(db)
      await themVaoHangCho(db, dongMau('tt-thua'))

      const fetchGia = vi.fn(
        async () =>
          new Response(JSON.stringify(ketQuaGuiLenMau({ ketQua: [{ maThaoTac: 'tt-thua', ketQua: 'thua', thongBao: null }] })), {
            status: 200,
          }),
      )
      vi.stubGlobal('fetch', fetchGia)

      const hangCho = (await docHangCho(db)) as DongHangChoDongBo[]
      await guiHangChoVaApDung(phuThuoc, hangCho)

      expect(await docHangCho(db)).toHaveLength(0)
      expect(await docCanXemLai(db)).toHaveLength(0)

      db.close()
    })
  })

  describe('RANG BUOC 3 — ap ket qua gui len la MOT giao dich (so da nhan + con tro + can xem lai + xoa hang cho)', () => {
    it('hong giua chung (giao dich abort) thi KHONG CO GI duoc ap — hang cho van con, con tro khong doi, so da nhan trong', async () => {
      const db = await moKho(tenKhoRieng())
      const phuThuoc = await khoiTaoPhuThuoc(db)
      await themVaoHangCho(db, dongMau('tt-abort'))

      const conTroTruoc = await docConTro(db)

      // Ep giao dich abort NGAY SAU KHI lenh xoa hang cho (lenh CUOI CUNG trong apDungGuiLenKetQua)
      // duoc goi — mo phong mat dien/het quota giua chung. Neu bon lenh ghi khong nam trong CUNG
      // mot giao dich, spy nay se khong bat duoc kich ban hong nay dung nhu mo ta (xem muc "CHUNG
      // MINH rang buoc 3 biet bao loi" trong bao cao — da tach tam thoi thanh hai giao dich rieng
      // de xac nhan test nay THAT SU do khi thieu tinh nguyen tu).
      const fetchGia = vi.fn(
        async () =>
          new Response(
            JSON.stringify(
              ketQuaGuiLenMau({
                conTroMoi: 999,
                dongMoi: [dongHieuLucMau(1)],
                ketQua: [{ maThaoTac: 'tt-abort', ketQua: 'ap', thongBao: null }],
              }),
            ),
            { status: 200 },
          ),
      )
      vi.stubGlobal('fetch', fetchGia)

      // `xoaKhoiHangChoTrongGiaoDich` xoá qua CON TRỎ (`con.delete()`, tức `IDBCursor.prototype.delete`),
      // KHÔNG phải `IDBObjectStore.prototype.delete` — phải spy đúng chỗ này (xem `hangCho.ts`).
      const deleteGoc = IDBCursor.prototype.delete
      const spyDelete = vi.spyOn(IDBCursor.prototype, 'delete').mockImplementation(function (
        this: IDBCursor,
        ...doiSo: Parameters<IDBCursor['delete']>
      ) {
        const yc = deleteGoc.apply(this, doiSo)
        if (this.source instanceof IDBObjectStore && this.source.name === KHO_HANG_CHO) {
          this.source.transaction.abort()
        }
        return yc
      })

      const hangCho = (await docHangCho(db)) as DongHangChoDongBo[]
      await expect(guiHangChoVaApDung(phuThuoc, hangCho)).rejects.toThrow()

      spyDelete.mockRestore()

      // KHONG CO GI duoc ap: hang cho VAN CON dong cu, con tro KHONG doi, so da nhan TRONG.
      expect(await docHangCho(db)).toHaveLength(1)
      const conTroSau = await docConTro(db)
      expect(conTroSau.soThuTu).toBe(conTroTruoc.soThuTu)
      expect(await docTheoKhoang(db, 'epoch-1', 0)).toHaveLength(0)

      db.close()
    })
  })

  describe('goiDongBo (qua cac ham cong khai) — phan biet 410/409 may-chu-di-lui/loi khac', () => {
    it('410 Gone nem LoiEpochKhongKhop', async () => {
      const db = await moKho(tenKhoRieng())
      const phuThuoc = await khoiTaoPhuThuoc(db)
      vi.stubGlobal('fetch', vi.fn(async () => new Response(null, { status: 410 })))

      await expect(guiHangChoVaApDung(phuThuoc, [])).rejects.toBeInstanceOf(LoiEpochKhongKhop)
      db.close()
    })

    it('409 kem type "may-chu-di-lui" nem LoiMayChuDiLui, khong phai loi xung dot thuong', async () => {
      const db = await moKho(tenKhoRieng())
      const phuThuoc = await khoiTaoPhuThuoc(db)
      vi.stubGlobal(
        'fetch',
        vi.fn(
          async () =>
            new Response(JSON.stringify({ type: 'may-chu-di-lui', title: 'x', detail: 'chi tiet loi' }), { status: 409 }),
        ),
      )

      await expect(guiHangChoVaApDung(phuThuoc, [])).rejects.toBeInstanceOf(LoiMayChuDiLui)
      db.close()
    })

    it('loi mang (fetch nem) khong lam sap ung dung, chi nem Error thong thuong', async () => {
      const db = await moKho(tenKhoRieng())
      const phuThuoc = await khoiTaoPhuThuoc(db)
      vi.stubGlobal('fetch', vi.fn(async () => { throw new TypeError('failed to fetch') }))

      await expect(guiHangChoVaApDung(phuThuoc, [])).rejects.toThrow()
      db.close()
    })
  })

  describe('dauTuDongHieuLuc — BAT BUOC dung chuanHoaMocMayChu khi dung DauDongHo tu may chu', () => {
    it('chap nhan mot moc may chu dang +00:00 (khac Z) va chuan hoa dung ve dang co dinh', () => {
      const dau = dauTuDongHieuLuc(dongHieuLucMau(1, { dongHoVatLy: '2026-09-15T09:00:00+00:00' }))
      expect(dau.vatLy).toBe('2026-09-15T09:00:00.000000Z')
    })

    it('chap nhan moc co phan le giay ngan hon 6 chu so, dem them so 0', () => {
      const dau = dauTuDongHieuLuc(dongHieuLucMau(1, { dongHoVatLy: '2026-09-15T09:00:00.5Z' }))
      expect(dau.vatLy).toBe('2026-09-15T09:00:00.500000Z')
    })
  })

  describe('batDauBoDongBo — lich chay bon nguon kich hoat', () => {
    // KHONG dung vi.useFakeTimers() o day: cac thao tac kho (fake-indexeddb) dua vao hang doi su
    // kien THAT cua moi truong chay — bat fake timers toan cuc lam dung luon ca cac await cho
    // IndexedDB (da thu va xac nhan treo test, xem bao cao Task 6). Thay vao do tiem mot
    // `NguonHenGio` gia lap: `dat()` chi DUA vao hang doi noi bo, CHI kich hoat khi test chu dong
    // goi `kichHoatHetHan()` — nho vay kiem soat duoc dung luc vong lap "het chu ky dinh ky" ma
    // khong can cho that.
    function nguonSuKienGiaLap(): NguonSuKienDongBo & { anTab: (v: boolean) => void; kichHoat: () => void } {
      let an = false
      const nguoiNghe: Array<() => void> = []
      return {
        tabDangAn: () => an,
        dangKy: (fn) => {
          nguoiNghe.push(fn)
          return () => {
            const vt = nguoiNghe.indexOf(fn)
            if (vt !== -1) nguoiNghe.splice(vt, 1)
          }
        },
        anTab: (v) => {
          an = v
        },
        kichHoat: () => nguoiNghe.forEach((fn) => fn()),
      }
    }

    // jsdom khong co `navigator.locks` (can secure context — https/localhost) — thay bang mot
    // NguonKhoa gia lap CAP KHOA NGAY (chi mot "tab" trong cac test o day, khong can hang doi FIFO
    // that su nhu bauChu.test.ts).
    const nguonKhoaGiaLap: NguonKhoa = {
      request: (_ten, _tuyChon, xuLy) => xuLy(),
    }

    function nguonHenGioGiaLap() {
      let danhSach: Array<{ ms: number; fn: () => void; huyRoi: boolean }> = []
      const dat = vi.fn((ms: number, fn: () => void) => {
        const muc = { ms, fn, huyRoi: false }
        danhSach.push(muc)
        return muc
      })
      const huy = vi.fn((id: unknown) => {
        ;(id as { huyRoi: boolean }).huyRoi = true
      })
      return {
        dat,
        huy,
        /** Kích hoạt TẤT CẢ hẹn giờ đang chờ — mô phỏng "đã hết chu kỳ định kỳ" mà không cần chờ
         * thời gian thật trôi qua. */
        kichHoatHetHan: () => {
          const dangCho = danhSach.filter((m) => !m.huyRoi)
          danhSach = []
          for (const m of dangCho) m.fn()
        },
      }
    }

    it('goi mot chu ky ngay khi tro thanh chu, sau do doi den khi duoc danh thuc boi su kien', async () => {
      const db = await moKho(tenKhoRieng())
      const fetchGia = vi.fn(async () => new Response(JSON.stringify(ketQuaGuiLenMau()), { status: 200 }))
      vi.stubGlobal('fetch', fetchGia)
      const nguon = nguonSuKienGiaLap()
      const henGio = nguonHenGioGiaLap()

      const dieuKhien = batDauBoDongBo(db, nguon, henGio, nguonKhoaGiaLap)
      await vi.waitFor(() => expect(fetchGia).toHaveBeenCalledTimes(1))

      // Vong lap dang CHO (hen gio da duoc dat, chua kich hoat) - khong tu goi them.
      await vi.waitFor(() => expect(henGio.dat).toHaveBeenCalledTimes(1))
      await new Promise((r) => setTimeout(r, 20))
      expect(fetchGia).toHaveBeenCalledTimes(1)

      // Kich hoat su kien (visibilitychange/focus/online gia lap) - danh thuc NGAY, khong can hen gio het han.
      nguon.kichHoat()
      await vi.waitFor(() => expect(fetchGia).toHaveBeenCalledTimes(2))

      dieuKhien.dung()
      db.close()
    })

    it('het chu ky dinh ky (khong co su kien nao) van tu dong chay chu ky tiep theo', async () => {
      const db = await moKho(tenKhoRieng())
      const fetchGia = vi.fn(async () => new Response(JSON.stringify(ketQuaGuiLenMau()), { status: 200 }))
      vi.stubGlobal('fetch', fetchGia)
      const nguon = nguonSuKienGiaLap()
      const henGio = nguonHenGioGiaLap()

      const dieuKhien = batDauBoDongBo(db, nguon, henGio, nguonKhoaGiaLap)
      await vi.waitFor(() => expect(fetchGia).toHaveBeenCalledTimes(1))
      await vi.waitFor(() => expect(henGio.dat).toHaveBeenCalledTimes(1))
      expect(henGio.dat).toHaveBeenLastCalledWith(CHU_KY_YEN_TINH_MS, expect.any(Function))

      henGio.kichHoatHetHan()
      await vi.waitFor(() => expect(fetchGia).toHaveBeenCalledTimes(2))

      dieuKhien.dung()
      db.close()
    })

    it('baoDangGo dat lich cho tiep theo bang CHU_KY_DANG_GO_MS va danh thuc ngay vong dang cho', async () => {
      const db = await moKho(tenKhoRieng())
      const fetchGia = vi.fn(async () => new Response(JSON.stringify(ketQuaGuiLenMau()), { status: 200 }))
      vi.stubGlobal('fetch', fetchGia)
      const nguon = nguonSuKienGiaLap()
      const henGio = nguonHenGioGiaLap()

      const dieuKhien = batDauBoDongBo(db, nguon, henGio, nguonKhoaGiaLap)
      await vi.waitFor(() => expect(fetchGia).toHaveBeenCalledTimes(1))
      await vi.waitFor(() => expect(henGio.dat).toHaveBeenCalledTimes(1))
      // Chu ky dau tien (chua he co baoDangGo) phai dung CHU_KY_YEN_TINH_MS mac dinh.
      expect(henGio.dat).toHaveBeenNthCalledWith(1, CHU_KY_YEN_TINH_MS, expect.any(Function))

      // baoDangGo() danh thuc NGAY (khong can hen gio het han) va chu ky CHO SAU DO phai la
      // CHU_KY_DANG_GO_MS.
      dieuKhien.baoDangGo()
      await vi.waitFor(() => expect(fetchGia).toHaveBeenCalledTimes(2))
      await vi.waitFor(() => expect(henGio.dat).toHaveBeenCalledTimes(2))
      expect(henGio.dat).toHaveBeenNthCalledWith(2, CHU_KY_DANG_GO_MS, expect.any(Function))

      dieuKhien.dung()
      db.close()
    })

    it('tab an: van gui khi hang cho co gi, du khong co su kien kich hoat nao', async () => {
      const db = await moKho(tenKhoRieng())
      await themVaoHangCho(db, dongMau('tt-tab-an'))
      const fetchGia = vi.fn(async () => new Response(JSON.stringify(ketQuaGuiLenMau()), { status: 200 }))
      vi.stubGlobal('fetch', fetchGia)
      const nguon = nguonSuKienGiaLap()
      nguon.anTab(true)
      const henGio = nguonHenGioGiaLap()

      const dieuKhien = batDauBoDongBo(db, nguon, henGio, nguonKhoaGiaLap)
      await vi.waitFor(() => expect(fetchGia).toHaveBeenCalledTimes(1))

      dieuKhien.dung()
      db.close()
    })

    it('gap LoiMayChuDiLui thi DUNG HAN vong lap va bao trang thai dung_do_may_chu_di_lui', async () => {
      const db = await moKho(tenKhoRieng())
      const fetchGia = vi.fn(
        async () => new Response(JSON.stringify({ type: 'may-chu-di-lui', title: 'x' }), { status: 409 }),
      )
      vi.stubGlobal('fetch', fetchGia)
      const nguon = nguonSuKienGiaLap()
      const henGio = nguonHenGioGiaLap()

      const dieuKhien = batDauBoDongBo(db, nguon, henGio, nguonKhoaGiaLap)
      await vi.waitFor(() => expect(dieuKhien.trangThai()).toBe('dung_do_may_chu_di_lui'))

      // Vong lap da thoat han (return trong khiLaChu) - KHONG con hen gio nao dang cho, va kich
      // hoat het han (neu co) cung khong lam gi vi danh sach da rong.
      expect(henGio.dat).not.toHaveBeenCalled()
      const soLanGoiLucDung = fetchGia.mock.calls.length
      henGio.kichHoatHetHan()
      nguon.kichHoat()
      await new Promise((r) => setTimeout(r, 20))
      expect(fetchGia.mock.calls.length).toBe(soLanGoiLucDung)

      dieuKhien.dung()
      db.close()
    })
  })
})
