import { render, screen, fireEvent, waitFor } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'
import { MauInListPage } from './MauInListPage'
import { api } from '../api/client'
import { useAuth } from '../api/AuthContext'
import type { MauInDanhSachItem, MauInChiTiet, ThongTinNguoiDung } from '../api/types'

vi.mock('../api/client', () => ({
  api: {
    mauIn: {
      danhSach: vi.fn(),
      xemThu: vi.fn(),
      rieng: { layChiTiet: vi.fn(), luu: vi.fn(), khoiPhuc: vi.fn() },
      heThong: { layChiTiet: vi.fn(), luu: vi.fn(), khoiPhuc: vi.fn() },
    },
  },
  LoiXungDot: class LoiXungDot extends Error {},
}))
vi.mock('../api/AuthContext', () => ({ useAuth: vi.fn() }))

const nguoiDung = (loaiTaiKhoan: number): ThongTinNguoiDung => ({
  id: 'u1', tenTaiKhoan: 'test', hoTen: 'Test', loaiTaiKhoan, giaoXuId: 'gx1', tenGiaoXu: 'GX Test',
})

const dong = (p: Partial<MauInDanhSachItem> = {}): MauInDanhSachItem => ({
  tenMau: 'LyLichCaNhan', tenHienThi: 'Lý lịch cá nhân', capDangDung: 'MacDinh',
  choTrong: [{ key: 'HoTen', nhan: 'Họ và tên' }, { key: 'NgaySinh', nhan: 'Ngày sinh' }],
  ...p,
})

const chiTiet = (p: Partial<MauInChiTiet> = {}): MauInChiTiet => ({
  tenMau: 'LyLichCaNhan', tenHienThi: 'Lý lịch cá nhân', daTuyChinh: false,
  noiDungHtml: '<html><body><h1>{{HoTen}}</h1></body></html>', rowVersion: 0,
  choTrong: [{ key: 'HoTen', nhan: 'Họ và tên' }, { key: 'NgaySinh', nhan: 'Ngày sinh' }],
  ...p,
})

describe('MauInListPage', () => {
  it('hien danh sach mau kem huy hieu cap dang dung', async () => {
    vi.mocked(useAuth).mockReturnValue({ nguoiDung: nguoiDung(1) } as unknown as ReturnType<typeof useAuth>)
    vi.mocked(api.mauIn.danhSach).mockResolvedValue([
      dong({ tenMau: 'LyLichCaNhan', tenHienThi: 'Lý lịch cá nhân', capDangDung: 'MacDinh' }),
      dong({ tenMau: 'PhieuGiaDinh', tenHienThi: 'Phiếu gia đình', capDangDung: 'TuyChinhGiaoXu' }),
    ])

    render(<MauInListPage />)

    expect(await screen.findByText('Lý lịch cá nhân')).toBeDefined()
    expect(screen.getByText('Phiếu gia đình')).toBeDefined()
    expect(screen.getByText('Mặc định gốc')).toBeDefined()
    expect(screen.getByText('Đã tuỳ chỉnh (riêng giáo xứ)')).toBeDefined()
  })

  it('tai khoan thuong (khong phai QuanTri/QuanTriHeThong) khong thay nut Sua', async () => {
    vi.mocked(useAuth).mockReturnValue({ nguoiDung: nguoiDung(1) } as unknown as ReturnType<typeof useAuth>)
    vi.mocked(api.mauIn.danhSach).mockResolvedValue([dong()])

    render(<MauInListPage />)

    await screen.findByText('Lý lịch cá nhân')
    expect(screen.queryByText('Sửa')).toBeNull()
  })

  it('QuanTri giao xu bam Sua mo trinh soan mau RIENG', async () => {
    vi.mocked(useAuth).mockReturnValue({ nguoiDung: nguoiDung(0) } as unknown as ReturnType<typeof useAuth>)
    vi.mocked(api.mauIn.danhSach).mockResolvedValue([dong()])
    vi.mocked(api.mauIn.rieng.layChiTiet).mockResolvedValue(chiTiet())

    render(<MauInListPage />)
    await screen.findByText('Lý lịch cá nhân')
    fireEvent.click(screen.getByText('Sửa'))

    await waitFor(() => expect(api.mauIn.rieng.layChiTiet).toHaveBeenCalledWith('LyLichCaNhan'))
    expect(api.mauIn.heThong.layChiTiet).not.toHaveBeenCalled()
    expect(await screen.findByText('Sửa mẫu: Lý lịch cá nhân')).toBeDefined()
  })

  it('QuanTriHeThong bam Sua mo trinh soan mau HE THONG', async () => {
    vi.mocked(useAuth).mockReturnValue({ nguoiDung: nguoiDung(9) } as unknown as ReturnType<typeof useAuth>)
    vi.mocked(api.mauIn.danhSach).mockResolvedValue([dong()])
    vi.mocked(api.mauIn.heThong.layChiTiet).mockResolvedValue(chiTiet())

    render(<MauInListPage />)
    await screen.findByText('Lý lịch cá nhân')
    fireEvent.click(screen.getByText('Sửa'))

    await waitFor(() => expect(api.mauIn.heThong.layChiTiet).toHaveBeenCalledWith('LyLichCaNhan'))
    expect(api.mauIn.rieng.layChiTiet).not.toHaveBeenCalled()
  })

  it('bam Xem thu goi dung API voi noi dung dang go', async () => {
    vi.mocked(useAuth).mockReturnValue({ nguoiDung: nguoiDung(0) } as unknown as ReturnType<typeof useAuth>)
    vi.mocked(api.mauIn.danhSach).mockResolvedValue([dong()])
    vi.mocked(api.mauIn.rieng.layChiTiet).mockResolvedValue(chiTiet())
    vi.mocked(api.mauIn.xemThu).mockResolvedValue(undefined)

    render(<MauInListPage />)
    await screen.findByText('Lý lịch cá nhân')
    fireEvent.click(screen.getByText('Sửa'))
    await screen.findByText('Sửa mẫu: Lý lịch cá nhân')
    fireEvent.click(screen.getByText('Xem thử'))

    await waitFor(() => expect(api.mauIn.xemThu).toHaveBeenCalledWith('LyLichCaNhan', expect.stringContaining('HoTen')))
  })

  it('bam Luu goi dung API roi bao Da luu', async () => {
    vi.mocked(useAuth).mockReturnValue({ nguoiDung: nguoiDung(0) } as unknown as ReturnType<typeof useAuth>)
    vi.mocked(api.mauIn.danhSach).mockResolvedValue([dong()])
    vi.mocked(api.mauIn.rieng.layChiTiet).mockResolvedValue(chiTiet())
    vi.mocked(api.mauIn.rieng.luu).mockResolvedValue(undefined)

    render(<MauInListPage />)
    await screen.findByText('Lý lịch cá nhân')
    fireEvent.click(screen.getByText('Sửa'))
    await screen.findByText('Sửa mẫu: Lý lịch cá nhân')
    fireEvent.click(screen.getByText('Lưu'))

    await waitFor(() => expect(api.mauIn.rieng.luu).toHaveBeenCalledWith('LyLichCaNhan', expect.any(String), 0))
    expect(await screen.findByText('Đã lưu.')).toBeDefined()
  })

  it('chua tuy chinh thi khong hien nut Khoi phuc; da tuy chinh thi hien va bam duoc', async () => {
    vi.mocked(useAuth).mockReturnValue({ nguoiDung: nguoiDung(0) } as unknown as ReturnType<typeof useAuth>)
    vi.mocked(api.mauIn.danhSach).mockResolvedValue([dong({ capDangDung: 'TuyChinhGiaoXu' })])
    vi.mocked(api.mauIn.rieng.layChiTiet).mockResolvedValue(chiTiet({ daTuyChinh: true }))
    vi.mocked(api.mauIn.rieng.khoiPhuc).mockResolvedValue(undefined)
    vi.spyOn(window, 'confirm').mockReturnValue(true)

    render(<MauInListPage />)
    await screen.findByText('Lý lịch cá nhân')
    fireEvent.click(screen.getByText('Sửa'))
    await screen.findByText('Sửa mẫu: Lý lịch cá nhân')

    fireEvent.click(screen.getByText('Khôi phục về mặc định'))
    await waitFor(() => expect(api.mauIn.rieng.khoiPhuc).toHaveBeenCalledWith('LyLichCaNhan'))
  })
})
