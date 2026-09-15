import 'fake-indexeddb/auto'
import { render, screen } from '@testing-library/react'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { BanGiaoMayPage } from './BanGiaoMayPage'
import { api } from '../api/client'
import { moKho, KHO_HANG_CHO } from '../kho/moKho'
import { _resetChoKiemThu } from '../dongbo/khoiDongOffline'
import * as khoiDongOfflineModule from '../dongbo/khoiDongOffline'

vi.mock('../api/client', () => ({
  api: { canXemLai: { danhSach: vi.fn() } },
}))

let demTen = 0
function tenKhoRieng() {
  demTen += 1
  return `qlgx-test-BanGiaoMay-${demTen}`
}

describe('BanGiaoMayPage (Task 10)', () => {
  beforeEach(() => {
    _resetChoKiemThu()
  })
  afterEach(() => {
    vi.restoreAllMocks()
    _resetChoKiemThu()
  })

  it('CA HAI bang 0 -> hien nut xanh "Da gui ve het..."', async () => {
    const db = await moKho(tenKhoRieng())
    vi.spyOn(khoiDongOfflineModule, 'layTrangThaiOffline').mockReturnValue({
      kho: db, dieuKhien: { dung: () => {}, baoDangGo: () => {}, danhThucNgay: () => {}, layThayDoiNgay: async () => null, trangThai: () => 'dang_chay' },
    })
    vi.mocked(api.canXemLai.danhSach).mockResolvedValue([])

    render(<BanGiaoMayPage />)

    expect(await screen.findByRole('button', { name: /Đã gửi về hết/ })).toBeDefined()
  })

  it('hang cho CON viec chua gui -> KHONG hien nut xanh, hien canh bao', async () => {
    const db = await moKho(tenKhoRieng())
    await new Promise<void>((resolve, reject) => {
      const gd = db.transaction(KHO_HANG_CHO, 'readwrite')
      gd.objectStore(KHO_HANG_CHO).add({ maThaoTac: 'tt-1', doan: 0 }, 1)
      gd.oncomplete = () => resolve()
      gd.onerror = () => reject(gd.error)
    })
    vi.spyOn(khoiDongOfflineModule, 'layTrangThaiOffline').mockReturnValue({
      kho: db, dieuKhien: { dung: () => {}, baoDangGo: () => {}, danhThucNgay: () => {}, layThayDoiNgay: async () => null, trangThai: () => 'dang_chay' },
    })
    vi.mocked(api.canXemLai.danhSach).mockResolvedValue([])

    render(<BanGiaoMayPage />)

    await screen.findByText('Số việc chưa gửi lên máy chủ: 1')
    expect(screen.queryByRole('button', { name: /Đã gửi về hết/ })).toBeNull()
  })

  it('hang cho rong nhung CON muc can xem lai -> KHONG hien nut xanh', async () => {
    const db = await moKho(tenKhoRieng())
    vi.spyOn(khoiDongOfflineModule, 'layTrangThaiOffline').mockReturnValue({
      kho: db, dieuKhien: { dung: () => {}, baoDangGo: () => {}, danhThucNgay: () => {}, layThayDoiNgay: async () => null, trangThai: () => 'dang_chay' },
    })
    vi.mocked(api.canXemLai.danhSach).mockResolvedValue([
      { id: '1', loai: 'xung_dot', bang: 'GiaoDan', banGhiId: 'gd-1', truong: 'ngaySinh', lyDo: null, giaTriA: 'a', giaTriB: 'b', giaTriDangDung: 'b', taoLuc: '2026-09-15T00:00:00Z' },
    ])

    render(<BanGiaoMayPage />)

    await screen.findByText('Số mục cần xem lại: 1')
    expect(screen.queryByRole('button', { name: /Đã gửi về hết/ })).toBeNull()
  })

  // I2 (fix round cuoi): hai con so bang 0 CHUA du de noi "go may nay an toan" — LOP 2
  // (dung_do_may_chu_di_lui) nghia la bo dong bo da TU DUNG, hang cho rong chi vi khong con gui gi
  // nua, khong phai vi da gui xong; may nay co the la ban sao cuoi cung con du lieu.
  it('I2: LOP 2 (dung_do_may_chu_di_lui) bat -> KHONG hien nut xanh du CA HAI con so bang 0', async () => {
    const db = await moKho(tenKhoRieng())
    vi.spyOn(khoiDongOfflineModule, 'layTrangThaiOffline').mockReturnValue({
      kho: db, dieuKhien: { dung: () => {}, baoDangGo: () => {}, danhThucNgay: () => {}, layThayDoiNgay: async () => null, trangThai: () => 'dung_do_may_chu_di_lui' },
    })
    vi.mocked(api.canXemLai.danhSach).mockResolvedValue([])

    render(<BanGiaoMayPage />)

    await screen.findByText('Số mục cần xem lại: 0')
    expect(screen.queryByRole('button', { name: /Đã gửi về hết/ })).toBeNull()
    expect(screen.getByText(/đưa về bản cũ/)).toBeDefined()
  })

  // I2: dang bu lai sau khi may chu duoc khoi phuc -> con so doc duoc chi la anh chup giua chung.
  it('I2: dang bu lai (layTrangThaiBuLai.dangBu=true) -> KHONG hien nut xanh du CA HAI con so bang 0', async () => {
    const db = await moKho(tenKhoRieng())
    vi.spyOn(khoiDongOfflineModule, 'layTrangThaiOffline').mockReturnValue({
      kho: db, dieuKhien: { dung: () => {}, baoDangGo: () => {}, danhThucNgay: () => {}, layThayDoiNgay: async () => null, trangThai: () => 'dang_chay' },
    })
    vi.spyOn(khoiDongOfflineModule, 'layTrangThaiBuLai').mockReturnValue({ dangBu: true, soDongDaBu: null })
    vi.mocked(api.canXemLai.danhSach).mockResolvedValue([])

    render(<BanGiaoMayPage />)

    await screen.findByText('Số mục cần xem lại: 0')
    expect(screen.queryByRole('button', { name: /Đã gửi về hết/ })).toBeNull()
    expect(screen.getByText(/Đang gửi lại dữ liệu/)).toBeDefined()
  })

  // N1 (fix round cuoi): loi mang khi hoi /api/can-xem-lai -> dat lai ve "chua biet" (null), KHONG
  // giu con so cu — nut xanh khong duoc hien dua tren so lieu chua xac nhan lai duoc.
  it('N1: loi mang khi hoi can-xem-lai -> tro ve "Dang kiem tra", KHONG hien nut xanh', async () => {
    const db = await moKho(tenKhoRieng())
    vi.spyOn(khoiDongOfflineModule, 'layTrangThaiOffline').mockReturnValue({
      kho: db, dieuKhien: { dung: () => {}, baoDangGo: () => {}, danhThucNgay: () => {}, layThayDoiNgay: async () => null, trangThai: () => 'dang_chay' },
    })
    vi.mocked(api.canXemLai.danhSach).mockRejectedValue(new Error('mat mang'))

    render(<BanGiaoMayPage />)

    expect(await screen.findByRole('alert')).toBeDefined()
    expect(screen.queryByRole('button', { name: /Đã gửi về hết/ })).toBeNull()
    expect(screen.queryByText(/Số mục cần xem lại/)).toBeNull()
  })

  it('tang offline chua san sang (layTrangThaiOffline null) -> KHONG hien nut xanh, hien dang kiem tra', async () => {
    vi.spyOn(khoiDongOfflineModule, 'layTrangThaiOffline').mockReturnValue(null)
    vi.mocked(api.canXemLai.danhSach).mockResolvedValue([])

    render(<BanGiaoMayPage />)

    await screen.findByText('Đang kiểm tra…')
    expect(screen.queryByRole('button', { name: /Đã gửi về hết/ })).toBeNull()
  })
})
