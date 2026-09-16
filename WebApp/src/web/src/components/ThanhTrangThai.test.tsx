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
  api: { canXemLai: { danhSach: vi.fn() }, he: { sucKhoe: vi.fn() } },
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

  // L3 (fix round Task 11, spec 4.8.6): dang bu lai sau khi may chu vua duoc khoi phuc.
  it('dangBuLai=true -> vang, dung cau chu spec 4.8.6, dung soHangCho trong cau', () => {
    const kq = tinhTrangThai(47, 0, 'dang_chay', true)
    expect(kq).toEqual({
      mau: 'vang',
      dongChu: 'Máy chủ vừa được khôi phục. Đang gửi lại 47 thay đổi mà máy này còn giữ.',
    })
  })

  it('dangBuLai=true nhung trangThaiBo la dung_do_may_chu_di_lui -> VAN do (uu tien cao hon)', () => {
    const kq = tinhTrangThai(3, 0, 'dung_do_may_chu_di_lui', true)
    expect(kq.mau).toBe('do')
    expect(kq.dongChu).toMatch(/máy chủ/i)
    expect(kq.dongChu).not.toContain('Đang gửi lại')
  })

  // N2 (fix round cuoi): DAO thu tu so voi truoc — soCanXemLai > 0 (do) UU TIEN HON dangBuLai
  // (vang). Canh bao do bao co viec can nguoi dung QUYET DINH, quan trong hon mot thong bao tien
  // trinh dang tu chay; truoc day dangBuLai CHE mat canh bao do trong suot pha bu.
  it('N2: soCanXemLai > 0 UU TIEN HON dangBuLai -> do "Can xem lai", KHONG phai vang "dang bu"', () => {
    const kq = tinhTrangThai(5, 3, 'dang_chay', true)
    expect(kq.mau).toBe('do')
    expect(kq.dongChu).toBe('Cần xem lại (3)')
  })

  it('N2: dangBuLai=true nhung soCanXemLai = 0 -> VAN vang "May chu vua duoc khoi phuc" (uu tien hon hang cho thuong)', () => {
    const kq = tinhTrangThai(5, 0, 'dang_chay', true)
    expect(kq.mau).toBe('vang')
    expect(kq.dongChu).toContain('Máy chủ vừa được khôi phục')
  })

  it('khong truyen dangBuLai (mac dinh false) -> giu nguyen hanh vi cu', () => {
    expect(tinhTrangThai(3, 0, 'dang_chay')).toEqual({
      mau: 'vang', dongChu: 'Đã lưu ở máy này (3) — đang gửi về',
    })
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
    vi.mocked(api.he.sucKhoe).mockResolvedValue({ trangThai: 'ok', phienBan: '1.2.3' })
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

  // L3 (fix round Task 11, spec 4.8.6): dang bu lai -> hien vang dung cau, va sau khi bu xong hien
  // dong tong ket trong bang giai thich.
  it('dang bu lai (layTrangThaiBuLai.dangBu=true) -> hien vang voi cau "May chu vua duoc khoi phuc..."', async () => {
    const db = await moKho(tenKhoRieng())
    const fetchGia = vi.fn(async () => new Response(JSON.stringify({ epoch: 'e', conTroMoi: 0, conNua: false, dong: [] }), { status: 200 }))
    vi.stubGlobal('fetch', fetchGia)
    const dieuKhien = batDauBoDongBo(db, undefined, undefined, nguonKhoaGiaLap)
    vi.spyOn(khoiDongOfflineModule, 'layTrangThaiOffline').mockReturnValue({ kho: db, dieuKhien })
    vi.spyOn(khoiDongOfflineModule, 'layTrangThaiBuLai').mockReturnValue({ dangBu: true, soDongDaBu: null })

    render(<ThanhTrangThai />)

    expect(await screen.findByText(/Máy chủ vừa được khôi phục/)).toBeDefined()
    dieuKhien.dung()
  })

  // I3 (fix round cuoi): cau tran an trong bang giai thich PHAI bam theo mau/trangThaiBo — truoc day
  // render vo dieu kien, nen ngay duoi mot cham DO van hien "se tu gui len... khong can lam gi",
  // mau thuan voi chinh cham do phia tren va SAI SU THAT o trang thai do.
  it('I3: hang cho co viec (vang) -> bang giai thich VAN hien cau tran an cu (dung su that o trang thai nay)', async () => {
    const db = await moKho(tenKhoRieng())
    await new Promise<void>((resolve, reject) => {
      const gd = db.transaction(KHO_HANG_CHO, 'readwrite')
      gd.objectStore(KHO_HANG_CHO).add({ maThaoTac: 'tt-i3', doan: 0 }, 1)
      gd.oncomplete = () => resolve()
      gd.onerror = () => reject(gd.error)
    })
    const fetchGia = vi.fn(async () => new Response(JSON.stringify({ epoch: 'e', conTroMoi: 0, conNua: false, ketQua: [], dongMoi: [] }), { status: 200 }))
    vi.stubGlobal('fetch', fetchGia)
    const dieuKhien = batDauBoDongBo(db, undefined, undefined, nguonKhoaGiaLap)
    vi.spyOn(khoiDongOfflineModule, 'layTrangThaiOffline').mockReturnValue({ kho: db, dieuKhien })

    render(<ThanhTrangThai />)
    await userEvent.click(await screen.findByRole('button', { name: /Đã lưu ở máy này/ }))

    expect(await screen.findByText(/sẽ tự gửi lên khi có mạng/)).toBeDefined()
    dieuKhien.dung()
  })

  it('I3: co muc can xem lai (do) -> KHONG hien cau "khong can lam gi", hien cau nhac co viec can xem lai', async () => {
    const db = await moKho(tenKhoRieng())
    const fetchGia = vi.fn(async () => new Response(JSON.stringify({ epoch: 'e', conTroMoi: 0, conNua: false, dong: [] }), { status: 200 }))
    vi.stubGlobal('fetch', fetchGia)
    const dieuKhien = batDauBoDongBo(db, undefined, undefined, nguonKhoaGiaLap)
    vi.spyOn(khoiDongOfflineModule, 'layTrangThaiOffline').mockReturnValue({ kho: db, dieuKhien })
    vi.mocked(api.canXemLai.danhSach).mockResolvedValue([
      { id: '1', loai: 'xung_dot', bang: 'GiaoDan', banGhiId: 'gd-1', truong: 'ngaySinh', lyDo: null, giaTriA: 'a', giaTriB: 'b', giaTriDangDung: 'b', taoLuc: '2026-09-15T00:00:00Z' },
    ])

    render(<ThanhTrangThai />)
    await userEvent.click(await screen.findByRole('button', { name: /Cần xem lại/ }))

    await screen.findByRole('region', { name: 'Chi tiết trạng thái lưu dữ liệu' })
    expect(screen.queryByText(/không cần làm gì/)).toBeNull()
    expect(screen.getByText(/cần anh\/chị xem lại/)).toBeDefined()
    dieuKhien.dung()
  })

  it('I3: LOP 2 (dung_do_may_chu_di_lui) -> cau khac han, KHONG noi "se tu gui len"/"khong can lam gi"', async () => {
    const db = await moKho(tenKhoRieng())
    const dieuKhienGia = {
      dung: () => {}, baoDangGo: () => {}, danhThucNgay: () => {}, layThayDoiNgay: async () => null,
      trangThai: () => 'dung_do_may_chu_di_lui' as const,
    }
    vi.spyOn(khoiDongOfflineModule, 'layTrangThaiOffline').mockReturnValue({ kho: db, dieuKhien: dieuKhienGia })

    render(<ThanhTrangThai />)
    await userEvent.click(await screen.findByRole('button', { name: /đưa về bản cũ/ }))

    await screen.findByRole('region', { name: 'Chi tiết trạng thái lưu dữ liệu' })
    expect(screen.queryByText(/sẽ tự gửi lên khi có mạng/)).toBeNull()
    expect(screen.queryByText(/không cần làm gì/)).toBeNull()
    expect(screen.getByText(/báo người hỗ trợ/)).toBeDefined()
  })

  it('I3: xanh (khong co gi cho) -> KHONG hien cau tran an nao ca', async () => {
    const db = await moKho(tenKhoRieng())
    const fetchGia = vi.fn(async () => new Response(JSON.stringify({ epoch: 'e', conTroMoi: 0, conNua: false, dong: [] }), { status: 200 }))
    vi.stubGlobal('fetch', fetchGia)
    const dieuKhien = batDauBoDongBo(db, undefined, undefined, nguonKhoaGiaLap)
    vi.spyOn(khoiDongOfflineModule, 'layTrangThaiOffline').mockReturnValue({ kho: db, dieuKhien })

    render(<ThanhTrangThai />)
    await userEvent.click(await screen.findByRole('button', { name: /Đã lưu an toàn/ }))

    await screen.findByRole('region', { name: 'Chi tiết trạng thái lưu dữ liệu' })
    expect(screen.queryByText(/sẽ tự gửi lên khi có mạng/)).toBeNull()
    expect(screen.queryByText(/cần anh\/chị xem lại/)).toBeNull()
    dieuKhien.dung()
  })

  it('bu xong (dangBu chuyen false, co soDongDaBu) -> mo bang giai thich hien dong tong ket "Da gui lai N thay doi."', async () => {
    const db = await moKho(tenKhoRieng())
    const fetchGia = vi.fn(async () => new Response(JSON.stringify({ epoch: 'e', conTroMoi: 0, conNua: false, dong: [] }), { status: 200 }))
    vi.stubGlobal('fetch', fetchGia)
    const dieuKhien = batDauBoDongBo(db, undefined, undefined, nguonKhoaGiaLap)
    vi.spyOn(khoiDongOfflineModule, 'layTrangThaiOffline').mockReturnValue({ kho: db, dieuKhien })
    vi.spyOn(khoiDongOfflineModule, 'layTrangThaiBuLai').mockReturnValue({ dangBu: false, soDongDaBu: 5 })

    render(<ThanhTrangThai />)
    await screen.findByText('Đã lưu an toàn')

    await userEvent.click(screen.getByRole('button', { name: /Đã lưu an toàn/ }))
    expect(await screen.findByText('Đã gửi lại 5 thay đổi.')).toBeDefined()

    dieuKhien.dung()
  })
})
