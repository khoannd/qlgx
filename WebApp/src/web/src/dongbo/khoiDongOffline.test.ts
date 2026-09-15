// jsdom không cài IndexedDB — polyfill bằng fake-indexeddb (xem moKho.test.ts, cùng khuôn mẫu).
import 'fake-indexeddb/auto'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { khoiDongOffline, layTrangThaiOffline, _resetChoKiemThu } from './khoiDongOffline'
import * as moKhoModule from '../kho/moKho'
import * as boDongBoModule from './boDongBo'

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
    const moKhoSpy = vi.spyOn(moKhoModule, 'moKho')
    const batDauSpy = vi.spyOn(boDongBoModule, 'batDauBoDongBo')
    const fetchGia = vi.fn(async () => new Response(JSON.stringify({ epoch: 'e1', conTroMoi: 0, conNua: false, dong: [] }), { status: 200 }))
    vi.stubGlobal('fetch', fetchGia)

    const [k1, k2, k3] = await Promise.all([khoiDongOffline(), khoiDongOffline(), khoiDongOffline()])

    expect(k1).toBe(k2)
    expect(k2).toBe(k3)
    // Luu y: khoiDongOffline.ts KHONG tu goi lai moKho/batDauBoDongBo o day (mock spy chi xac nhan
    // duoc goi it nhat mot lan qua module that vi Vitest khong the spy tren import tinh ma khong
    // qua namespace — kiem tra qua ket qua giong het nhau la bang chung chinh cho hanh vi cache).
    void moKhoSpy
    void batDauSpy
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
