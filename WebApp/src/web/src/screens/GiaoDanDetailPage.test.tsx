import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'
import { GiaoDanDetailPage } from './GiaoDanDetailPage'
import { api, LoiXungDot } from '../api/client'
import type { GiaoDanDetail as ChiTiet } from '../api/types'

vi.mock('../api/client', async () => {
  const actual = await vi.importActual<typeof import('../api/client')>('../api/client')
  return {
    LoiXungDot: actual.LoiXungDot,
    api: {
      giaoDan: {
        chiTiet: vi.fn(), capNhat: vi.fn(),
        honPhoi: vi.fn().mockResolvedValue([]), capNhatHonPhoi: vi.fn(),
      },
    },
  }
})

const chiTiet = (p: Partial<ChiTiet> = {}): ChiTiet => ({
  id: 'p1', maGiaoDanCu: 4511, hoTen: 'Vũ Minh Trí', tenThanh: 'Giuse', phai: 'Nam',
  ngaySinh: '1996-04-02', noiSinh: null, cmnd: null, danToc: null, giaoHoId: null,
  diaChi: null, dienThoai: null, email: null, hoTenCha: null, hoTenMe: null,
  soRuaToi: null, ngayRuaToi: null, noiRuaToi: null, chaRuaToi: null, nguoiDoDauRuaToi: null,
  soRuocLe: null, ngayRuocLe: null, noiRuocLe: null, chaRuocLe: null,
  soThemSuc: null, ngayThemSuc: null, noiThemSuc: null, chaThemSuc: null, nguoiDoDauThemSuc: null,
  ngayXucDau: null, nguoiXucDau: null, tinhTrangXucDau: null, ghiChuXucDau: null,
  trinhDoVanHoa: null, trinhDoChuyenMon: null, bietNgoaiNgu: null, ngheNghiep: null,
  conHoc: false, daCoGiaDinh: false, tanTong: false, khongThongKe: false,
  quaDoi: false, ngayQuaDoi: null, noiQuaDoi: null, soAnTang: null, noiAnTang: null,
  ghiChu: null, giaDinhId: null, tenGiaDinh: null, vaiTro: null,
  rowVersion: 1, ...p,
} as ChiTiet)

describe('GiaoDanDetailPage', () => {
  it('tai chi tiet that tu API va hien dung ten', async () => {
    vi.mocked(api.giaoDan.chiTiet).mockResolvedValue(chiTiet())

    render(<GiaoDanDetailPage id="p1" />)

    expect(screen.getByRole('status')).toBeDefined()
    expect(await screen.findByRole('heading', { name: /Vũ Minh Trí/ })).toBeDefined()
    expect(api.giaoDan.chiTiet).toHaveBeenCalledWith('p1')
  })

  it('luu thanh cong thi goi PUT va tai lai chi tiet', async () => {
    vi.mocked(api.giaoDan.chiTiet).mockResolvedValue(chiTiet())
    vi.mocked(api.giaoDan.capNhat).mockResolvedValue(undefined)

    render(<GiaoDanDetailPage id="p1" />)
    await screen.findByRole('heading', { name: /Vũ Minh Trí/ })

    await userEvent.click(screen.getByRole('button', { name: 'Cập nhật' }))

    expect(api.giaoDan.capNhat).toHaveBeenCalledWith('p1', expect.objectContaining({ rowVersion: 1 }))
    expect(await screen.findByText('Đã lưu thành công.')).toBeDefined()
    expect(api.giaoDan.chiTiet).toHaveBeenCalledTimes(2)
  })

  it('xung dot RowVersion (409) hien thong bao tieng Viet', async () => {
    vi.mocked(api.giaoDan.chiTiet).mockResolvedValue(chiTiet())
    vi.mocked(api.giaoDan.capNhat).mockRejectedValue(
      new LoiXungDot('Giáo dân này vừa được người khác cập nhật.'),
    )

    render(<GiaoDanDetailPage id="p1" />)
    await screen.findByRole('heading', { name: /Vũ Minh Trí/ })

    await userEvent.click(screen.getByRole('button', { name: 'Cập nhật' }))

    expect(await screen.findByText('Giáo dân này vừa được người khác cập nhật.')).toBeDefined()
  })

  // --- Task 15: tab Hôn phối nối API thật ---

  it('tai danh sach hon phoi that tu API va hien trong tab', async () => {
    vi.mocked(api.giaoDan.chiTiet).mockResolvedValue(chiTiet())
    vi.mocked(api.giaoDan.honPhoi).mockResolvedValue([{
      id: 'hp1', tenHonPhoi: 'Giuse Trí - Maria Thu', soHonPhoi: '12/2018', ngayHonPhoi: '2018-05-01',
      noiHonPhoi: 'Nhà thờ Chính tòa', linhMucChung: null, nguoiChung1: null, nguoiChung2: null,
      cachThucHonPhoi: null, ghiChu: null, voChongId: 'vc1', tenVoChong: 'Maria Thu', rowVersion: 1,
    }])

    render(<GiaoDanDetailPage id="p1" />)
    await screen.findByRole('heading', { name: /Vũ Minh Trí/ })
    await userEvent.click(screen.getByRole('tab', { name: 'Hôn phối' }))

    expect(await screen.findByDisplayValue('12/2018')).toBeDefined()
    expect(api.giaoDan.honPhoi).toHaveBeenCalledWith('p1')
  })

  it('luu hon phoi thanh cong thi goi PUT dung id va tai lai danh sach', async () => {
    vi.mocked(api.giaoDan.chiTiet).mockResolvedValue(chiTiet())
    vi.mocked(api.giaoDan.honPhoi).mockResolvedValue([{
      id: 'hp1', tenHonPhoi: null, soHonPhoi: 'So cu', ngayHonPhoi: null, noiHonPhoi: null,
      linhMucChung: null, nguoiChung1: null, nguoiChung2: null, cachThucHonPhoi: null,
      ghiChu: null, voChongId: 'vc1', tenVoChong: 'Maria Thu', rowVersion: 1,
    }])
    vi.mocked(api.giaoDan.capNhatHonPhoi).mockResolvedValue(undefined)

    render(<GiaoDanDetailPage id="p1" />)
    await screen.findByRole('heading', { name: /Vũ Minh Trí/ })
    await userEvent.click(screen.getByRole('tab', { name: 'Hôn phối' }))
    await screen.findByDisplayValue('So cu')

    await userEvent.click(screen.getByRole('button', { name: 'Cập nhật hôn phối' }))

    expect(api.giaoDan.capNhatHonPhoi).toHaveBeenCalledWith('hp1',
      expect.objectContaining({ soHonPhoi: 'So cu', rowVersion: 1 }))
    expect(await screen.findByText('Đã lưu thành công.')).toBeDefined()
    expect(api.giaoDan.honPhoi).toHaveBeenCalledTimes(2)
  })

  it('xung dot RowVersion khi luu hon phoi hien thong bao tieng Viet', async () => {
    vi.mocked(api.giaoDan.chiTiet).mockResolvedValue(chiTiet())
    vi.mocked(api.giaoDan.honPhoi).mockResolvedValue([{
      id: 'hp1', tenHonPhoi: null, soHonPhoi: 'So cu', ngayHonPhoi: null, noiHonPhoi: null,
      linhMucChung: null, nguoiChung1: null, nguoiChung2: null, cachThucHonPhoi: null,
      ghiChu: null, voChongId: null, tenVoChong: null, rowVersion: 1,
    }])
    vi.mocked(api.giaoDan.capNhatHonPhoi).mockRejectedValue(
      new LoiXungDot('Thông tin hôn phối này vừa được người khác cập nhật.'),
    )

    render(<GiaoDanDetailPage id="p1" />)
    await screen.findByRole('heading', { name: /Vũ Minh Trí/ })
    await userEvent.click(screen.getByRole('tab', { name: 'Hôn phối' }))
    await screen.findByDisplayValue('So cu')

    await userEvent.click(screen.getByRole('button', { name: 'Cập nhật hôn phối' }))

    expect(await screen.findByText('Thông tin hôn phối này vừa được người khác cập nhật.')).toBeDefined()
  })
})
