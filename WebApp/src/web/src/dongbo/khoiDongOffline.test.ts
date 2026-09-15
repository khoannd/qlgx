// jsdom không cài IndexedDB — polyfill bằng fake-indexeddb (xem moKho.test.ts, cùng khuôn mẫu).
import 'fake-indexeddb/auto'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { dungOffline, khoiDongOffline, layTrangThaiBuLai, layTrangThaiOffline, _resetChoKiemThu } from './khoiDongOffline'
import * as moKhoModule from '../kho/moKho'
import * as hangChoModule from '../kho/hangCho'
import * as soDaNhanModule from '../kho/soDaNhan'
import * as tepDuPhongModule from './tepDuPhong'
import * as buSauKhoiPhucModule from './buSauKhoiPhuc'

describe('khoiDongOffline (Task 10 — noi day tang offline vao app shell)', () => {
  beforeEach(() => {
    _resetChoKiemThu()
    vi.restoreAllMocks()
    // Khoa gia bang mot NguonKhoa cap ngay (jsdom khong co navigator.locks that) — batDauBoDongBo
    // nhan tham so thu tu la nguonKhoa, nhung khoiDongOffline khong lo tham so do ra ngoai nen phai
    // gia lap qua navigator.locks truc tiep.
    Object.defineProperty(globalThis.navigator, 'locks', {
      configurable: true,
      value: { request: (_ten: string, _tuyChon: unknown, xuLy: () => Promise<unknown>) => xuLy() },
    })
  })
  afterEach(() => {
    layTrangThaiOffline()?.dieuKhien.dung()
    _resetChoKiemThu()
    vi.unstubAllGlobals()
  })

  it('goi lan dau: mo kho THAT (moKho) va bat dau vong dong bo (batDauBoDongBo) dung mot lan', async () => {
    const fetchGia = vi.fn(async () => new Response(JSON.stringify({ epoch: 'e1', conTroMoi: 0, conNua: false, dong: [] }), { status: 200 }))
    vi.stubGlobal('fetch', fetchGia)

    const ketQua = await khoiDongOffline('giao-xu-test-1')

    expect(ketQua).not.toBeNull()
    expect(ketQua?.kho).toBeInstanceOf(IDBDatabase)
    expect(typeof ketQua?.dieuKhien.trangThai).toBe('function')
    expect(layTrangThaiOffline()).toBe(ketQua)
  })

  it('goi NHIEU LAN: dung mot Promise cache, KHONG mo kho/bat dau vong dong bo lan thu hai', async () => {
    const fetchGia = vi.fn(async () => new Response(JSON.stringify({ epoch: 'e1', conTroMoi: 0, conNua: false, dong: [] }), { status: 200 }))
    vi.stubGlobal('fetch', fetchGia)

    const [k1, k2, k3] = await Promise.all([khoiDongOffline('giao-xu-test-1'), khoiDongOffline('giao-xu-test-1'), khoiDongOffline('giao-xu-test-1')])

    expect(k1).toBe(k2)
    expect(k2).toBe(k3)
  })

  it('dungOffline(): dung vong dong bo + dong kho, lan khoiDongOffline() sau tao PHIEN MOI (khong dung lai cache cu)', async () => {
    const fetchGia = vi.fn(async () => new Response(JSON.stringify({ epoch: 'e1', conTroMoi: 0, conNua: false, dong: [] }), { status: 200 }))
    vi.stubGlobal('fetch', fetchGia)

    const phienDau = await khoiDongOffline('giao-xu-test-1')
    expect(phienDau).not.toBeNull()
    const dungSpy = vi.spyOn(phienDau!.dieuKhien, 'dung')

    dungOffline()

    expect(dungSpy).toHaveBeenCalledOnce()
    expect(layTrangThaiOffline()).toBeNull()

    const phienSau = await khoiDongOffline('giao-xu-test-1')
    expect(phienSau).not.toBeNull()
    expect(phienSau).not.toBe(phienDau)
  })

  // F1 (fix round Task 11 - round 2): neu buoc bu lai (xuLyEpochKhongKhopDonLuong) nem loi, cờ
  // trangThaiBuLai.dangBu phai duoc dat lai ve false TRUOC KHI loi duoc nem/log tiep - khong duoc
  // ket dangBu:true vinh vien (che mat nhanh "Can xem lai" trong ThanhTrangThai).
  it('F1: xuLyEpochKhongKhopDonLuong nem loi -> layTrangThaiBuLai().dangBu tro lai false (khong ket true)', async () => {
    const fetchGia = vi.fn(async () => new Response(null, { status: 410 }))
    vi.stubGlobal('fetch', fetchGia)
    vi.spyOn(buSauKhoiPhucModule, 'xuLyEpochKhongKhopDonLuong').mockRejectedValue(new Error('loi mo phong F1'))
    vi.spyOn(console, 'error').mockImplementation(() => {})

    const ketQua = await khoiDongOffline('giao-xu-test-1')
    expect(ketQua).not.toBeNull()

    // Vong dong bo chay ngam (khong await tu khoiDongOffline) - doi no bat 410, goi buoc bu lai,
    // bat loi va dat lai co.
    await vi.waitFor(() => {
      expect(buSauKhoiPhucModule.xuLyEpochKhongKhopDonLuong).toHaveBeenCalled()
    })
    await vi.waitFor(() => {
      expect(layTrangThaiBuLai().dangBu).toBe(false)
    })
    expect(layTrangThaiBuLai().soDongDaBu).toBeNull()
  })

  // F4 (fix round Task 11 - round 2): neu dungOffline() duoc goi GIUA luc khoiDongOffline() con
  // dang await moKho()/batDauBoDongBo(), truoc day than ham async cua khoiDongOffline() van tiep
  // tuc chay xong va GAN DE trangThaiHienTai/dat interval MOI - tang offline "song lai" sau khi
  // tuong da dung. Test nay mo phong dung race do: goi khoiDongOffline() KHONG await ngay, goi
  // dungOffline() truoc khi Promise dau resolve, roi moi await Promise dau.
  it('F4: dungOffline() xen vao giua luc khoiDongOffline() con dang mo kho -> khong "song lai" sau khi da dung', async () => {
    const fetchGia = vi.fn(async () => new Response(JSON.stringify({ epoch: 'e1', conTroMoi: 0, conNua: false, dong: [] }), { status: 200 }))
    vi.stubGlobal('fetch', fetchGia)
    const setIntervalSpy = vi.spyOn(globalThis, 'setInterval')

    const promiseDau = khoiDongOffline('giao-xu-test-1') // KHONG await ngay - con dang await moKho() ben trong
    dungOffline() // xen vao GIUA luc dang khoi dong
    const ketQua = await promiseDau

    // khoiDongOffline() phai TU NHAN RA da bi dung giua chung va tu don (dong kho, dung dieuKhien)
    // thay vi gan lam trang thai hien tai - KHONG duoc dat interval kiem tra du phong moi, va
    // layTrangThaiOffline() phai VAN la null (khong "song lai").
    expect(ketQua).toBeNull()
    expect(layTrangThaiOffline()).toBeNull()
    expect(setIntervalSpy).not.toHaveBeenCalled()

    // Sau race nay, mot lan khoiDongOffline() tiep theo (vi du dang nhap lai) phai van khoi dong
    // duoc binh thuong - khong bi ket cung boi co dangDungGiuaChung con sot lai tu lan truoc.
    const phienSau = await khoiDongOffline('giao-xu-test-1')
    expect(phienSau).not.toBeNull()
    expect(layTrangThaiOffline()).toBe(phienSau)
  })

  // L4 (fix round Task 11, spec 7.10): canTuDongDuPhong() da co day du logic tu Task 9 nhung truoc
  // day KHONG noi nao GOI no — ma chet. khoiDongOffline() phai bat dau mot vong lap nen kiem tra
  // dinh ky (moi 5 phut).
  // F2 (fix round Task 11 - round 2): test CU dung vi.useFakeTimers() SAU khi setInterval THAT da
  // dang ky (khoiDongOffline() da await xong roi moi bat fake timers) - advanceTimersByTimeAsync
  // khong the kich hoat mot interval THAT dang ky truoc do, nen phep assert cu chi "an theo" luot
  // goi TUC THOI (chay ngay luc khoi dong, khong phai vong lap dinh ky) - da tu kiem chung (xem
  // bao cao fix round 2): xoa dong `setInterval(...)` trong khoiDongOffline.ts, test CU van XANH.
  //
  // KHONG dung vi.useFakeTimers() bao trum ca doan goi khoiDongOffline() (dung y goi ban dau cua
  // brief) vi moKho()/batDauBoDongBo() dua vao fake-indexeddb, thu vien nay tu dung setTimeout NOI
  // BO de phat su kien - bat fake timers toan cuc tu truoc lam treo cac await do (da ghi chu dung
  // van de nay o boDongBo.test.ts:667-672, cung mot ly do). Thay vao do, bat CHINH XAC dieu F2 can
  // kiem chung — "interval dinh ky duoc dang ky voi dung callback goi kiemTraTuDongDuPhong" — bang
  // cach spy truc tiep len setInterval TOAN CUC, bat lay callback THAT ma khoiDongOffline() dang ky,
  // roi tu goi callback do (mo phong dung mot "tick" cua interval, khong can troi qua thoi gian
  // that/gia). Neu dong `setInterval(...)` trong khoiDongOffline.ts bi xoa, setIntervalSpy khong
  // duoc goi, `callback` la undefined -> test do (da tu kiem chung, xem bao cao).
  it('L4: setInterval duoc dang ky dung chu ky, callback cua no goi docHangCho/canTuDongDuPhong', async () => {
    const fetchGia = vi.fn(async () => new Response(JSON.stringify({ epoch: 'e1', conTroMoi: 0, conNua: false, dong: [] }), { status: 200 }))
    vi.stubGlobal('fetch', fetchGia)

    const setIntervalSpy = vi.spyOn(globalThis, 'setInterval')

    const ketQua = await khoiDongOffline('giao-xu-test-1')
    expect(ketQua).not.toBeNull()

    expect(setIntervalSpy).toHaveBeenCalledTimes(1)
    expect(setIntervalSpy.mock.calls[0]?.[1]).toBe(5 * 60 * 1000)
    const callback = setIntervalSpy.mock.calls[0]?.[0] as (() => void) | undefined
    expect(callback).toBeTypeOf('function')

    // Spy SAU KHI da bat duoc callback — chi can xac nhan LUOT TICK (goi thu cong callback nay, mo
    // phong dung mot chu ky dinh ky that su, KHONG phai luot goi tuc thi luc khoi dong) co goi toi
    // docHangCho/canTuDongDuPhong.
    const docHangChoSpy = vi.spyOn(hangChoModule, 'docHangCho').mockResolvedValue([])
    const canTuDongDuPhongSpy = vi.spyOn(tepDuPhongModule, 'canTuDongDuPhong').mockReturnValue(false)

    callback!()

    await vi.waitFor(() => {
      expect(docHangChoSpy).toHaveBeenCalled()
      expect(canTuDongDuPhongSpy).toHaveBeenCalled()
    })
  })

  // C1 (fix round cuoi - CRITICAL, spec 6.4): may chu phuc vu NHIEU giao xu tren mot CSDL, moi giao
  // xu co epoch/chuoi so_thu_tu RIENG. Truoc day ten kho la hang so 'qlgx' — doi tai khoan sang giao
  // xu KHAC tren cung trinh duyet se mo lai DUNG kho cu, mang nguyen hang cho/so da nhan/con tro cua
  // giao xu TRUOC sang giao xu MOI (lo du lieu so sach giua hai giao xu). Bat bien: hai giaoXuId
  // khac nhau -> hai TEN KHO khac nhau.
  it('C1: hai giao xu khac nhau tren cung trinh duyet -> moKho duoc goi voi HAI ten kho KHAC NHAU', async () => {
    const fetchGia = vi.fn(async () => new Response(JSON.stringify({ epoch: 'e1', conTroMoi: 0, conNua: false, dong: [] }), { status: 200 }))
    vi.stubGlobal('fetch', fetchGia)
    const moKhoSpy = vi.spyOn(moKhoModule, 'moKho')

    await khoiDongOffline('giao-xu-A')
    dungOffline()
    await khoiDongOffline('giao-xu-B')

    const tenKhoDaDung = moKhoSpy.mock.calls.map((c) => c[0])
    expect(tenKhoDaDung).toEqual(['qlgx-giao-xu-A', 'qlgx-giao-xu-B'])
    expect(tenKhoDaDung[0]).not.toBe(tenKhoDaDung[1])
  })

  // I1 (fix round cuoi, spec 4.8.2): donSoDaNhanCu() la ma chet truoc day — phai duoc goi trong
  // CHINH vong lap nen 5 phut da co (khong tao interval moi), voi moc "30 ngay truoc" dang chuoi ISO.
  it('I1: tick cua vong lap nen goi donSoDaNhanCu voi moc ISO 30 ngay truoc', async () => {
    const fetchGia = vi.fn(async () => new Response(JSON.stringify({ epoch: 'e1', conTroMoi: 0, conNua: false, dong: [] }), { status: 200 }))
    vi.stubGlobal('fetch', fetchGia)
    const setIntervalSpy = vi.spyOn(globalThis, 'setInterval')

    const ketQua = await khoiDongOffline('giao-xu-test-1')
    expect(ketQua).not.toBeNull()

    const callback = setIntervalSpy.mock.calls[0]?.[0] as (() => void) | undefined
    expect(callback).toBeTypeOf('function')

    const donSpy = vi.spyOn(soDaNhanModule, 'donSoDaNhanCu').mockResolvedValue(undefined)
    vi.spyOn(hangChoModule, 'docHangCho').mockResolvedValue([])
    vi.spyOn(tepDuPhongModule, 'canTuDongDuPhong').mockReturnValue(false)

    callback!()

    await vi.waitFor(() => expect(donSpy).toHaveBeenCalled())
    const moc = donSpy.mock.calls[0]?.[1] as string
    expect(typeof moc).toBe('string')
    // Chuoi ISO hop le (donSoDaNhanCu so sanh CHUOI theo thu tu tu dien — sai dinh dang la sai het).
    expect(moc).toMatch(/^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}\.\d{3}Z$/)
    const soNgay = (Date.now() - Date.parse(moc)) / (24 * 60 * 60 * 1000)
    expect(soNgay).toBeGreaterThan(29.9)
    expect(soNgay).toBeLessThan(30.1)
  })

  // I1: mot viec hong KHONG duoc keo viec kia chet theo trong cung luot chay (hai viec doc lap).
  it('I1: docHangCho nem loi -> donSoDaNhanCu VAN duoc goi (try/catch rieng cho tung viec)', async () => {
    const fetchGia = vi.fn(async () => new Response(JSON.stringify({ epoch: 'e1', conTroMoi: 0, conNua: false, dong: [] }), { status: 200 }))
    vi.stubGlobal('fetch', fetchGia)
    const setIntervalSpy = vi.spyOn(globalThis, 'setInterval')

    await khoiDongOffline('giao-xu-test-1')
    const callback = setIntervalSpy.mock.calls[0]?.[0] as (() => void) | undefined

    vi.spyOn(console, 'error').mockImplementation(() => {})
    vi.spyOn(hangChoModule, 'docHangCho').mockRejectedValue(new Error('mo phong loi doc hang cho'))
    const donSpy = vi.spyOn(soDaNhanModule, 'donSoDaNhanCu').mockResolvedValue(undefined)

    callback!()

    await vi.waitFor(() => expect(donSpy).toHaveBeenCalled())
  })

  it('mo kho THAT BAI: tra ve null, KHONG nem loi ra ngoai, va layTrangThaiOffline() van la null', async () => {
    // Mo phong loi that bang cach lam factory.open nem loi dong bo — dung mot IDBFactory gia don
    // gian nhat co the (khoiDongOffline goi moKho() khong tham so nen dung indexedDB toan cuc,
    // spy truc tiep tren moKho de tra ve mot Promise reject thay vi mo that).
    vi.spyOn(moKhoModule, 'moKho').mockRejectedValue(new Error('mo phong loi mo kho that (Task 10 test)'))
    const loiConsole = vi.spyOn(console, 'error').mockImplementation(() => {})

    const ketQua = await khoiDongOffline('giao-xu-test-1')

    expect(ketQua).toBeNull()
    expect(layTrangThaiOffline()).toBeNull()
    expect(loiConsole).toHaveBeenCalled()
  })
})
