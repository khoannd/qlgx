// jsdom không cài IndexedDB — polyfill bằng fake-indexeddb (xem moKho.test.ts, cùng khuôn mẫu).
import 'fake-indexeddb/auto'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { moKho, KHO_HANG_CHO } from '../kho/moKho'
import { docHangCho, type DongHangCho } from '../kho/hangCho'
import { docConTro } from '../kho/conTro'
import { docTheoKhoang } from '../kho/soDaNhan'
import { chuanHoaMocMayChu } from './dauDongHo'
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
  type NhanVeKetQua,
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

/** Mẫu `NhanVeKetQua` — hình dạng riêng của `/thay-doi` (KHÁC `GuiLenKetQua`: có `dong`, không có
 * `ketQua`/`dongMoi`). Dùng lẫn lộn hai hình dạng là đúng lỗi C1 review vòng sửa 1 đã bắt được ở
 * test cũ (mock chung một hình dạng cho cả hai đầu vào). */
function nhanVeKetQuaMau(phanBiet: Partial<NhanVeKetQua> = {}): NhanVeKetQua {
  return { epoch: 'epoch-1', conTroMoi: 10, conNua: false, dong: [], ...phanBiet }
}

/**
 * Mock `fetch` PHÂN BIỆT theo URL — BẮT BUỘC từ vòng sửa 1 (C1): `/gui-len` và `/thay-doi` có hình
 * dạng phản hồi KHÁC NHAU (`GuiLenKetQua` vs `NhanVeKetQua`), và 409 "may-chu-di-lui" THẬT chỉ có
 * thể tới từ `/thay-doi` (xem chú thích `goiDongBo`/`DongBoService.cs` — `/gui-len` gọi `NhanVe` nội
 * bộ với `epoch: null`, không bao giờ kích hoạt LỚP 2). Một mock DÙNG CHUNG một hình dạng cho cả hai
 * URL (như bản trước review) rubber-stamp một hợp đồng máy chủ thật không bao giờ tạo ra được.
 */
function fetchGiaTheoDuong(
  xuLy: {
    guiLen?: (yc: GuiLenYeuCau) => GuiLenKetQua | Response
    thayDoi?: () => NhanVeKetQua | Response
  } = {},
) {
  return vi.fn(async (url: unknown, tuyChon?: RequestInit) => {
    const u = String(url)
    if (u.includes('/gui-len')) {
      const yc = JSON.parse(String(tuyChon?.body)) as GuiLenYeuCau
      const kq = xuLy.guiLen ? xuLy.guiLen(yc) : ketQuaGuiLenMau()
      return kq instanceof Response ? kq : new Response(JSON.stringify(kq), { status: 200 })
    }
    if (u.includes('/thay-doi')) {
      const kq = xuLy.thayDoi ? xuLy.thayDoi() : nhanVeKetQuaMau()
      return kq instanceof Response ? kq : new Response(JSON.stringify(kq), { status: 200 })
    }
    throw new Error('URL khong duoc mo phong trong test nay: ' + u)
  })
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
    it('tab an + hang cho CO gi: van goi ca /gui-len LAN /thay-doi (C1 — LOP 2 phai duoc kiem tra moi chu ky)', async () => {
      const db = await moKho(tenKhoRieng())
      const phuThuoc = await khoiTaoPhuThuoc(db)
      await themVaoHangCho(db, dongMau('tt-1'))

      const fetchGia = fetchGiaTheoDuong()
      vi.stubGlobal('fetch', fetchGia)

      await motLanDongBo(phuThuoc, /* tabAn */ true)

      // Hang cho co gi -> van phai /gui-len (Rang buoc 9), CONG THEM /thay-doi ngay sau do (C1) de
      // LOP 2 duoc kiem tra that su moi chu ky, khong phai ma chet.
      expect(fetchGia).toHaveBeenCalledTimes(2)
      const urlsGoi = fetchGia.mock.calls.map((c) => String(c[0]))
      expect(urlsGoi.some((u) => u.includes('/gui-len'))).toBe(true)
      expect(urlsGoi.some((u) => u.includes('/thay-doi'))).toBe(true)
      db.close()
    })

    it('tab an + hang cho RONG: khong goi mang (ngung hoi du lieu moi), KE CA /thay-doi', async () => {
      const db = await moKho(tenKhoRieng())
      const phuThuoc = await khoiTaoPhuThuoc(db)

      const fetchGia = fetchGiaTheoDuong()
      vi.stubGlobal('fetch', fetchGia)

      await motLanDongBo(phuThuoc, /* tabAn */ true)

      expect(fetchGia).not.toHaveBeenCalled()
      db.close()
    })

    it('tab HIEN + hang cho RONG: goi /thay-doi (hoi du lieu moi DUNG duong, khong con la /gui-len ron gia trang)', async () => {
      const db = await moKho(tenKhoRieng())
      const phuThuoc = await khoiTaoPhuThuoc(db)

      const fetchGia = fetchGiaTheoDuong()
      vi.stubGlobal('fetch', fetchGia)

      await motLanDongBo(phuThuoc, /* tabAn */ false)

      expect(fetchGia).toHaveBeenCalledTimes(1)
      expect(String(fetchGia.mock.calls[0][0])).toContain('/thay-doi')
      db.close()
    })
  })

  describe('ket qua tu_choi: XOA khoi hang cho, KHONG tu ghi vao kho rieng nao (Ruling A, vong sua 1)', () => {
    it('tu_choi: xoa dung dong khoi hang cho — may chu da tu ghi so cua no, Task 6 khong ghi them gi', async () => {
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

      // Xoa khoi hang cho, KHONG con gi khac de kiem tra (khong co kho canXemLai rieng nua).
      expect(await docHangCho(db)).toHaveLength(0)

      db.close()
    })

    it('ket qua "thua" cung xoa khoi hang cho binh thuong (ket qua gop, khong phai loi)', async () => {
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

      db.close()
    })

    it('I7: so khop maThaoTac KHONG PHAN BIET hoa/thuong voi khoa GOC cua hang cho (khong phai chuoi server tra ve)', async () => {
      const db = await moKho(tenKhoRieng())
      const phuThuoc = await khoiTaoPhuThuoc(db)
      // Khoa GOC ta luu chu HOA lan chu thuong lan hoa; server tra ve toan chu THUONG (mo phong lech
      // hoa/thuong co the xay ra, du hom nay khong xay ra that).
      await themVaoHangCho(db, dongMau('TT-Hoa-Thuong'))

      const fetchGia = vi.fn(
        async () =>
          new Response(
            JSON.stringify(ketQuaGuiLenMau({ ketQua: [{ maThaoTac: 'tt-hoa-thuong', ketQua: 'ap', thongBao: null }] })),
            { status: 200 },
          ),
      )
      vi.stubGlobal('fetch', fetchGia)

      const hangCho = (await docHangCho(db)) as DongHangChoDongBo[]
      await guiHangChoVaApDung(phuThuoc, hangCho)

      // Neu so khop DUNG chu HOA/thuong that (bug I7 cu), dong nay se KET LAI trong hang cho mai
      // mai. Phai xoa duoc, dung khoa GOC (TT-Hoa-Thuong), khong phai chuoi server tra ve.
      expect(await docHangCho(db)).toHaveLength(0)

      db.close()
    })
  })

  describe('RANG BUOC 3 — ap ket qua gui len la MOT giao dich (so da nhan + con tro + xoa hang cho)', () => {
    it('hong giua chung (giao dich abort) thi KHONG CO GI duoc ap — hang cho van con, con tro khong doi, so da nhan trong', async () => {
      const db = await moKho(tenKhoRieng())
      const phuThuoc = await khoiTaoPhuThuoc(db)
      await themVaoHangCho(db, dongMau('tt-abort'))

      const conTroTruoc = await docConTro(db)

      // Ep giao dich abort NGAY SAU KHI lenh xoa hang cho (lenh CUOI CUNG trong apDungGuiLenKetQua)
      // duoc goi — mo phong mat dien/het quota giua chung. Neu ba lenh ghi khong nam trong CUNG
      // mot giao dich, spy nay se khong bat duoc kich ban hong nay dung nhu mo ta.
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

  describe('capNhatDongHoTheoDongNhanVe (I5, vong sua 1) — khong con la ma chet, co test bao ve', () => {
    it('sau khi ap mot dong nhan ve co dongHoVatLy o tuong lai xa, dauCuoi* cua may nay duoc nang qua dung moc do', async () => {
      const db = await moKho(tenKhoRieng())
      const phuThuoc = await khoiTaoPhuThuoc(db)
      const mocTuongLai = '2030-01-01T00:00:00Z'

      const fetchGia = fetchGiaTheoDuong({
        thayDoi: () => nhanVeKetQuaMau({ dong: [dongHieuLucMau(1, { dongHoVatLy: mocTuongLai, dongHoLogic: 42 })] }),
      })
      vi.stubGlobal('fetch', fetchGia)

      await motLanDongBo(phuThuoc, /* tabAn */ false)

      const conTro = await docConTro(db)
      expect(conTro.dauCuoiVatLy).toBe(chuanHoaMocMayChu(mocTuongLai))
      expect(conTro.dauCuoiLogic as number).toBeGreaterThanOrEqual(42)
      db.close()
    })

    it('lo NHIEU dong, dong co dau LON NHAT KHONG nam o cuoi mang van duoc chon dung (reduce, khong phai lay phan tu cuoi)', async () => {
      const db = await moKho(tenKhoRieng())
      const phuThuoc = await khoiTaoPhuThuoc(db)
      const mocLonNhat = '2031-06-15T00:00:00Z'
      const mocGiua = '2029-01-01T00:00:00Z'
      const mocNho = '2020-01-01T00:00:00Z'

      const fetchGia = fetchGiaTheoDuong({
        thayDoi: () =>
          nhanVeKetQuaMau({
            dong: [
              dongHieuLucMau(1, { dongHoVatLy: mocNho, dongHoLogic: 1, giaoDichId: 'g1' }),
              dongHieuLucMau(2, { dongHoVatLy: mocLonNhat, dongHoLogic: 5, giaoDichId: 'g2' }),
              // Dau LON NHAT (mocLonNhat) o GIUA mang, KHONG phai phan tu cuoi (mocGiua) — neu ma
              // sai chi lay phan tu cuoi thay vi reduce dung, test nay se do.
              dongHieuLucMau(3, { dongHoVatLy: mocGiua, dongHoLogic: 3, giaoDichId: 'g3' }),
            ],
          }),
      })
      vi.stubGlobal('fetch', fetchGia)

      await motLanDongBo(phuThuoc, /* tabAn */ false)

      const conTro = await docConTro(db)
      expect(conTro.dauCuoiVatLy).toBe(chuanHoaMocMayChu(mocLonNhat))
      db.close()
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
      const fetchGia = fetchGiaTheoDuong()
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
      const fetchGia = fetchGiaTheoDuong()
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
      const fetchGia = fetchGiaTheoDuong()
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

    it('tab an: van gui khi hang cho co gi (2 fetch/chu ky: gui-len + thay-doi), du khong co su kien kich hoat nao', async () => {
      const db = await moKho(tenKhoRieng())
      await themVaoHangCho(db, dongMau('tt-tab-an'))
      const fetchGia = fetchGiaTheoDuong()
      vi.stubGlobal('fetch', fetchGia)
      const nguon = nguonSuKienGiaLap()
      nguon.anTab(true)
      const henGio = nguonHenGioGiaLap()

      const dieuKhien = batDauBoDongBo(db, nguon, henGio, nguonKhoaGiaLap)
      await vi.waitFor(() => expect(fetchGia).toHaveBeenCalledTimes(2))

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

    it('C1: hang cho CO gi nhung /thay-doi (goi THEM sau /gui-len) tra may-chu-di-lui - van phai DUNG HAN', async () => {
      // Chung minh LOP 2 khong con la ma chet: /gui-len thanh cong binh thuong, nhung /thay-doi (goi
      // THEM cung chu ky, sau /gui-len) phat hien may chu di lui - phai dung han dung nhu khi phat
      // hien qua hang cho rong. Neu (mutation nguoc) bo lai loi goi /thay-doi khi hang cho CO gi, con
      // duong nay khong bao gio duoc kich hoat va test nay se do (khong bao gio dat duoc trang thai
      // dung_do_may_chu_di_lui, vi.waitFor se het han).
      const db = await moKho(tenKhoRieng())
      await themVaoHangCho(db, dongMau('tt-c1'))
      const fetchGia = fetchGiaTheoDuong({
        guiLen: () => ketQuaGuiLenMau(),
        thayDoi: () => new Response(JSON.stringify({ type: 'may-chu-di-lui', title: 'x' }), { status: 409 }),
      })
      vi.stubGlobal('fetch', fetchGia)
      const nguon = nguonSuKienGiaLap()
      const henGio = nguonHenGioGiaLap()

      const dieuKhien = batDauBoDongBo(db, nguon, henGio, nguonKhoaGiaLap)
      await vi.waitFor(() => expect(dieuKhien.trangThai()).toBe('dung_do_may_chu_di_lui'))

      // Ca hai duong deu da duoc goi truoc khi dung (gui-len thanh cong, roi thay-doi moi lam dung).
      const urlsGoi = fetchGia.mock.calls.map((c) => String(c[0]))
      expect(urlsGoi.some((u) => u.includes('/gui-len'))).toBe(true)
      expect(urlsGoi.some((u) => u.includes('/thay-doi'))).toBe(true)

      dieuKhien.dung()
      db.close()
    })

    it('I2: tin hieu danh thuc ban ra DUNG LUC fetch dang treo (motLanDongBo dang cho mang) khong bi mat', async () => {
      const db = await moKho(tenKhoRieng())
      let phanGiai: (() => void) | null = null
      const fetchGia = vi.fn(async () => {
        // Fetch DAU TIEN dang treo (mo phong dang cho mang) - cac lan sau tra loi ngay.
        if (!phanGiai) {
          return new Promise<Response>((resolve) => {
            phanGiai = () => resolve(new Response(JSON.stringify(nhanVeKetQuaMau()), { status: 200 }))
          })
        }
        return new Response(JSON.stringify(nhanVeKetQuaMau()), { status: 200 })
      })
      vi.stubGlobal('fetch', fetchGia)
      const nguon = nguonSuKienGiaLap()
      const henGio = nguonHenGioGiaLap()

      const dieuKhien = batDauBoDongBo(db, nguon, henGio, nguonKhoaGiaLap)
      await vi.waitFor(() => expect(fetchGia).toHaveBeenCalledTimes(1))

      // Ban tin hieu danh thuc DUNG LUC nay: dang o giua await motLanDongBo (fetch dau tien chua
      // resolve) - danhThucVongHienTai dang la null (chua toi buoc Promise.race cho hen gio). Neu
      // khong co co doc lap (I2), tin hieu nay se goi vao null?.() va bien mat.
      nguon.kichHoat()

      // Cho fetch xong.
      phanGiai!()

      // Vong lap phai chay chu ky TIEP THEO NGAY sau khi fetch xong, KHONG doi hen gio het han (chua
      // he goi henGio.kichHoatHetHan() trong test nay) - neu tin hieu bi mat, se khong bao gio co
      // fetch lan hai va vi.waitFor se het han.
      await vi.waitFor(() => expect(fetchGia).toHaveBeenCalledTimes(2))

      dieuKhien.dung()
      db.close()
    })

    it('I3: conNua=true bo qua buoc cho hen gio, lap lai chu ky tiep theo NGAY', async () => {
      const db = await moKho(tenKhoRieng())
      let lanGoi = 0
      const fetchGia = vi.fn(async () => {
        lanGoi += 1
        // Chu ky DAU bao conNua=true (may chu con du lieu chua gui het); chu ky SAU bao false de
        // vong lap dung o buoc cho binh thuong (khong lap vo han trong test).
        return new Response(JSON.stringify(nhanVeKetQuaMau({ conNua: lanGoi === 1 })), { status: 200 })
      })
      vi.stubGlobal('fetch', fetchGia)
      const nguon = nguonSuKienGiaLap()
      const henGio = nguonHenGioGiaLap()

      const dieuKhien = batDauBoDongBo(db, nguon, henGio, nguonKhoaGiaLap)

      // Chu ky thu HAI phai chay NGAY (khong can nguon.kichHoat()/henGio.kichHoatHetHan()) vi
      // conNua=true o chu ky dau — neu mutation bo qua conNua, test nay se cho vi.waitFor het han.
      await vi.waitFor(() => expect(fetchGia).toHaveBeenCalledTimes(2))

      dieuKhien.dung()
      db.close()
    })

    // I9, duong (1): request() TU NO reject TRUOC KHI kip goi khiLaChu (mo phong navigator.locks
    // khong kha dung). nguonKhoaVoiBaoLoi (boDongBo.ts) bat va XU LY XONG loi nay tai cho (dat
    // trangThai + ghi log), KHONG nem lai — vi vay `npx vitest run` khong con in "Unhandled Errors"
    // cho duong nay nua (khac ban truoc review vong sua 1, xem N1 trong task-6-report.md).
    it('I9: nguonKhoa reject (loi that, mo phong navigator.locks khong kha dung) bao trang thai khong_chay_duoc', async () => {
      const db = await moKho(tenKhoRieng())
      const fetchGia = fetchGiaTheoDuong()
      vi.stubGlobal('fetch', fetchGia)
      const nguon = nguonSuKienGiaLap()
      const henGio = nguonHenGioGiaLap()
      const nguonKhoaLoiThat: NguonKhoa = {
        request: () => Promise.reject(new Error('navigator.locks khong kha dung (mo phong I9)')),
      }

      const dieuKhien = batDauBoDongBo(db, nguon, henGio, nguonKhoaLoiThat)

      await vi.waitFor(() => expect(dieuKhien.trangThai()).toBe('khong_chay_duoc'))
      // khiLaChu chua bao gio duoc goi (request() da reject truoc do) - khong co chu ky nao chay.
      expect(fetchGia).not.toHaveBeenCalled()

      dieuKhien.dung()
      db.close()
    })

    // I9, duong (2) — N2 tu review scoped: khiLaChu (khong phai request() ben ngoai) tu nem loi
    // THAT, vi du kich ban that "kho IndexedDB bi loi/bi chan giua chung o mot may giao xu". Duong
    // nay CHUA co test nao truoc ban sua N2 (chi duong (1) co) — mo phong bang cach dong `db` NGAY
    // TRUOC khi batDauBoDongBo goi khoiTaoPhuThuoc (doc conTro qua mot transaction tren kho da
    // dong se nem loi that, khong phai AbortError).
    it('I9: khiLaChu tu nem loi that (vi du kho IndexedDB hong giua chung) cung bao trang thai khong_chay_duoc', async () => {
      const db = await moKho(tenKhoRieng())
      const fetchGia = fetchGiaTheoDuong()
      vi.stubGlobal('fetch', fetchGia)
      const nguon = nguonSuKienGiaLap()
      const henGio = nguonHenGioGiaLap()
      const nguonKhoaThat: NguonKhoa = {
        request: (_ten, _tuyChon, xuLy) => xuLy(),
      }

      db.close() // kho da dong TRUOC khi khiLaChu kip khoi tao phu thuoc - moi thao tac tren no nem loi that

      const dieuKhien = batDauBoDongBo(db, nguon, henGio, nguonKhoaThat)

      await vi.waitFor(() => expect(dieuKhien.trangThai()).toBe('khong_chay_duoc'))
      expect(fetchGia).not.toHaveBeenCalled()

      dieuKhien.dung()
    })
  })
})
