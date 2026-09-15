import 'fake-indexeddb/auto'
import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { ThanhTrangThai, tinhTrangThai } from './ThanhTrangThai'
import { moKho, KHO_HANG_CHO } from '../kho/moKho'
import { batDauBoDongBo } from '../dongbo/boDongBo'
import { _resetChoKiemThu } from '../dongbo/khoiDongOffline'
import * as khoiDongOfflineModule from '../dongbo/khoiDongOffline'
import { api } from '../api/client'
import type { NguonKhoa } from '../dongbo/bauChu'

vi.mock('../api/client', () => ({
  api: { canXemLai: { danhSach: vi.fn() } },
}))

describe('tinhTrangThai (Task 10, spec 9.1) — hàm thuần quyết định màu/câu chữ', () => {
  it('BAT BIEN CUNG: khong bao gio tra ve xanh khi soHangCho > 0, voi MOI to hop soCanXemLai/trangThaiBo', () => {
    for (const soCanXemLai of [0, 1, 5]) {
      for (const trangThaiBo of [null, 'dang_chay', 'dung_do_may_chu_di_lui', 'khong_chay_duoc'] as const) {
        for (const soHangCho of [1, 2, 100]) {
          const ketQua = tinhTrangThai(soHangCho, soCanXemLai, trangThaiBo)
          expect(ketQua.mau).not.toBe('xanh')
        }
      }
    }
  })

  it('hang cho rong, khong co xem lai, dang chay binh thuong -> xanh "Da luu an toan"', () => {
    expect(tinhTrangThai(0, 0, 'dang_chay')).toEqual({ mau: 'xanh', dongChu: 'Đã lưu an toàn' })
  })

  it('hang cho rong nhung trangThaiBo la null (chua biet) -> VAN xanh (mac dinh coi nhu dang chay binh thuong, khac han soHangCho)', () => {
    // Ghi chu: null CHI co y nghia "khong dung_do_may_chu_di_lui" o day - component (ThanhTrangThai)
    // la noi chiu trach nhiem KHONG goi ham nay cho toi khi co so lieu that (xem soHangCho === null
    // trong component) - ham thuan nay khong tu suy doan gi them ngoai tham so duoc truyen vao.
    expect(tinhTrangThai(0, 0, null).mau).toBe('xanh')
  })

  it('hang cho co viec, khong co xem lai, dang chay binh thuong -> vang, dung so trong cau chu', () => {
    expect(tinhTrangThai(3, 0, 'dang_chay')).toEqual({
      mau: 'vang', dongChu: 'Đã lưu ở máy này (3) — đang gửi về',
    })
  })

  it('co muc can xem lai (du hang cho rong) -> do, dung so trong cau chu', () => {
    expect(tinhTrangThai(0, 1, 'dang_chay')).toEqual({ mau: 'do', dongChu: 'Cần xem lại (1)' })
  })

  it('trangThaiBo la dung_do_may_chu_di_lui -> do, cau chu RIENG (khac han cau "can xem lai")', () => {
    const kq = tinhTrangThai(0, 0, 'dung_do_may_chu_di_lui')
    expect(kq.mau).toBe('do')
    expect(kq.dongChu).not.toContain('Cần xem lại')
    expect(kq.dongChu).toMatch(/máy chủ/i)
  })

  it('dung_do_may_chu_di_lui UU TIEN HON ca can xem lai (kiem tra thu tu nhanh quyet dinh)', () => {
    const kq = tinhTrangThai(0, 5, 'dung_do_may_chu_di_lui')
    expect(kq.dongChu).toMatch(/máy chủ/i)
    expect(kq.dongChu).not.toContain('Cần xem lại')
  })
})

describe('ThanhTrangThai (component) — doc tu tang offline that + /api/can-xem-lai', () => {
  let demTen = 0
  function tenKhoRieng() {
    demTen += 1
    return `qlgx-test-ThanhTrangThai-${demTen}`
  }

  const nguonKhoaGiaLap: NguonKhoa = { request: (_ten, _tuyChon, xuLy) => xuLy() }

  beforeEach(() => {
    _resetChoKiemThu()
    vi.mocked(api.canXemLai.danhSach).mockResolvedValue([])
  })
  afterEach(() => {
    vi.unstubAllGlobals()
    vi.restoreAllMocks()
    _resetChoKiemThu()
  })

  it('hang cho rong + khong co can xem lai -> hien cham xanh "Da luu an toan"', async () => {
    const db = await moKho(tenKhoRieng())
    const fetchGia = vi.fn(async () => new Response(JSON.stringify({ epoch: 'e', conTroMoi: 0, conNua: false, dong: [] }), { status: 200 }))
    vi.stubGlobal('fetch', fetchGia)
    const dieuKhien = batDauBoDongBo(db, undefined, undefined, nguonKhoaGiaLap)
    vi.spyOn(khoiDongOfflineModule, 'layTrangThaiOffline').mockReturnValue({ kho: db, dieuKhien })

    render(<ThanhTrangThai />)

    expect(await screen.findByText('Đã lưu an toàn')).toBeDefined()
    dieuKhien.dung()
  })

  it('hang cho CO viec chua gui -> khong bao gio hien "Da luu an toan", hien vang dung so', async () => {
    const db = await moKho(tenKhoRieng())
    await new Promise<void>((resolve, reject) => {
      const gd = db.transaction(KHO_HANG_CHO, 'readwrite')
      gd.objectStore(KHO_HANG_CHO).add({ maThaoTac: 'tt-1', doan: 0 }, 1)
      gd.oncomplete = () => resolve()
      gd.onerror = () => reject(gd.error)
    })
    const fetchGia = vi.fn(async () => new Response(JSON.stringify({ epoch: 'e', conTroMoi: 0, conNua: false, ketQua: [], dongMoi: [] }), { status: 200 }))
    vi.stubGlobal('fetch', fetchGia)
    const dieuKhien = batDauBoDongBo(db, undefined, undefined, nguonKhoaGiaLap)
    vi.spyOn(khoiDongOfflineModule, 'layTrangThaiOffline').mockReturnValue({ kho: db, dieuKhien })

    render(<ThanhTrangThai />)

    expect(await screen.findByText('Đã lưu ở máy này (1) — đang gửi về')).toBeDefined()
    expect(screen.queryByText('Đã lưu an toàn')).toBeNull()
    dieuKhien.dung()
  })

  it('co muc can xem lai -> hien do dung so, bam vao mo bang giai thich', async () => {
    const db = await moKho(tenKhoRieng())
    const fetchGia = vi.fn(async () => new Response(JSON.stringify({ epoch: 'e', conTroMoi: 0, conNua: false, dong: [] }), { status: 200 }))
    vi.stubGlobal('fetch', fetchGia)
    const dieuKhien = batDauBoDongBo(db, undefined, undefined, nguonKhoaGiaLap)
    vi.spyOn(khoiDongOfflineModule, 'layTrangThaiOffline').mockReturnValue({ kho: db, dieuKhien })
    vi.mocked(api.canXemLai.danhSach).mockResolvedValue([
      { id: '1', loai: 'xung_dot', bang: 'GiaoDan', banGhiId: 'gd-1', truong: 'ngaySinh', lyDo: null, giaTriA: 'a', giaTriB: 'b', giaTriDangDung: 'b', taoLuc: '2026-09-15T00:00:00Z' },
    ])

    render(<ThanhTrangThai />)

    expect(await screen.findByText('Cần xem lại (1)')).toBeDefined()

    await userEvent.click(screen.getByRole('button', { name: /Cần xem lại/ }))
    expect(await screen.findByRole('region', { name: 'Chi tiết trạng thái lưu dữ liệu' })).toBeDefined()

    dieuKhien.dung()
  })

  it('chua co tang offline san sang (layTrangThaiOffline tra null) -> KHONG hien gi ca (khong doan xanh)', async () => {
    vi.spyOn(khoiDongOfflineModule, 'layTrangThaiOffline').mockReturnValue(null)

    const { container } = render(<ThanhTrangThai />)
    await waitFor(() => expect(vi.mocked(api.canXemLai.danhSach)).toHaveBeenCalled())

    expect(container.textContent).toBe('')
  })
})
