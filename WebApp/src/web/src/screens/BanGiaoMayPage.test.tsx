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

  it('tang offline chua san sang (layTrangThaiOffline null) -> KHONG hien nut xanh, hien dang kiem tra', async () => {
    vi.spyOn(khoiDongOfflineModule, 'layTrangThaiOffline').mockReturnValue(null)
    vi.mocked(api.canXemLai.danhSach).mockResolvedValue([])

    render(<BanGiaoMayPage />)

    await screen.findByText('Đang kiểm tra…')
    expect(screen.queryByRole('button', { name: /Đã gửi về hết/ })).toBeNull()
  })
})
