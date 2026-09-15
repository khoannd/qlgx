// jsdom không cài IndexedDB — polyfill bằng fake-indexeddb (xem moKho.test.ts, cùng khuôn mẫu).
import 'fake-indexeddb/auto'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { dungOffline, khoiDongOffline, layTrangThaiOffline, _resetChoKiemThu } from './khoiDongOffline'
import * as moKhoModule from '../kho/moKho'
import * as hangChoModule from '../kho/hangCho'
import * as tepDuPhongModule from './tepDuPhong'

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

    const ketQua = await khoiDongOffline()

    expect(ketQua).not.toBeNull()
    expect(ketQua?.kho).toBeInstanceOf(IDBDatabase)
    expect(typeof ketQua?.dieuKhien.trangThai).toBe('function')
    expect(layTrangThaiOffline()).toBe(ketQua)
  })

  it('goi NHIEU LAN: dung mot Promise cache, KHONG mo kho/bat dau vong dong bo lan thu hai', async () => {
    const fetchGia = vi.fn(async () => new Response(JSON.stringify({ epoch: 'e1', conTroMoi: 0, conNua: false, dong: [] }), { status: 200 }))
    vi.stubGlobal('fetch', fetchGia)

    const [k1, k2, k3] = await Promise.all([khoiDongOffline(), khoiDongOffline(), khoiDongOffline()])

    expect(k1).toBe(k2)
    expect(k2).toBe(k3)
  })

  it('dungOffline(): dung vong dong bo + dong kho, lan khoiDongOffline() sau tao PHIEN MOI (khong dung lai cache cu)', async () => {
    const fetchGia = vi.fn(async () => new Response(JSON.stringify({ epoch: 'e1', conTroMoi: 0, conNua: false, dong: [] }), { status: 200 }))
    vi.stubGlobal('fetch', fetchGia)

    const phienDau = await khoiDongOffline()
    expect(phienDau).not.toBeNull()
    const dungSpy = vi.spyOn(phienDau!.dieuKhien, 'dung')

    dungOffline()

    expect(dungSpy).toHaveBeenCalledOnce()
    expect(layTrangThaiOffline()).toBeNull()

    const phienSau = await khoiDongOffline()
    expect(phienSau).not.toBeNull()
    expect(phienSau).not.toBe(phienDau)
  })

  // L4 (fix round Task 11, spec 7.10): canTuDongDuPhong() da co day du logic tu Task 9 nhung truoc
  // day KHONG noi nao GOI no — ma chet. khoiDongOffline() phai bat dau mot vong lap nen kiem tra
  // dinh ky (moi 5 phut).
  it('L4: bat dau vong lap dinh ky goi docHangCho/canTuDongDuPhong sau khi khoiDongOffline() chay', async () => {
    const fetchGia = vi.fn(async () => new Response(JSON.stringify({ epoch: 'e1', conTroMoi: 0, conNua: false, dong: [] }), { status: 200 }))
    vi.stubGlobal('fetch', fetchGia)

    const ketQua = await khoiDongOffline()
    expect(ketQua).not.toBeNull()

    // Spy SAU KHI khoiDongOffline() da chay xong — lan goi dau tien (ngay luc bat dau interval,
    // khong doi het chu ky) da chay bang ham THAT truoc do, ta chi can xac nhan CO them lan goi
    // MOI sau khi thoi gian troi qua mot chu ky (kiem tra dinh ky, khong phai chi goi mot lan).
    const docHangChoSpy = vi.spyOn(hangChoModule, 'docHangCho').mockResolvedValue([])
    const canTuDongDuPhongSpy = vi.spyOn(tepDuPhongModule, 'canTuDongDuPhong').mockReturnValue(false)

    vi.useFakeTimers()
    try {
      await vi.advanceTimersByTimeAsync(5 * 60 * 1000)
    } finally {
      vi.useRealTimers()
    }

    expect(docHangChoSpy).toHaveBeenCalled()
    expect(canTuDongDuPhongSpy).toHaveBeenCalled()
  })

  it('mo kho THAT BAI: tra ve null, KHONG nem loi ra ngoai, va layTrangThaiOffline() van la null', async () => {
    // Mo phong loi that bang cach lam factory.open nem loi dong bo — dung mot IDBFactory gia don
    // gian nhat co the (khoiDongOffline goi moKho() khong tham so nen dung indexedDB toan cuc,
    // spy truc tiep tren moKho de tra ve mot Promise reject thay vi mo that).
    vi.spyOn(moKhoModule, 'moKho').mockRejectedValue(new Error('mo phong loi mo kho that (Task 10 test)'))
    const loiConsole = vi.spyOn(console, 'error').mockImplementation(() => {})

    const ketQua = await khoiDongOffline()

    expect(ketQua).toBeNull()
    expect(layTrangThaiOffline()).toBeNull()
    expect(loiConsole).toHaveBeenCalled()
  })
})
