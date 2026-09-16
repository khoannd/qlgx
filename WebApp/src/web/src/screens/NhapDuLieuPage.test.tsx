import { render, screen, fireEvent, waitFor } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'
import { NhapDuLieuPage } from './NhapDuLieuPage'
import { api } from '../api/client'
import type { BaoCaoXemTruoc, GiaoXuQuanLy, TrangThaiNhapDuLieu } from '../api/types'

vi.mock('../api/client', () => ({
  api: {
    quanTri: {
      giaoXu: { danhSach: vi.fn() },
      nhapDuLieu: { xemTruoc: vi.fn(), batDau: vi.fn(), trangThai: vi.fn() },
    },
  },
}))

const xu = (p: Partial<GiaoXuQuanLy> = {}): GiaoXuQuanLy => ({
  id: 'x1', giaoHatId: 'h1', tenGiaoHat: 'Đức Tánh', tenGiaoPhan: 'Phan Thiết',
  tenGiaoXu: 'Giáo xứ mới', diaChi: null, dienThoai: null, email: null, website: null, ghiChu: null,
  coTrungTen: false, soTaiKhoan: 0, ...p,
})

const baoCao = (p: Partial<BaoCaoXemTruoc> = {}): BaoCaoXemTruoc => ({
  tenGiaoXuNguon: 'Giáo xứ Vô Nhiễm',
  giaoXuDichDaCoDuLieu: false,
  soGiaoDanDaCo: 0,
  doiChieu: [
    { bang: 'giao_dan', soDongNguon: 2050, soDongDich: -1, lech: true },
    { bang: 'gia_dinh', soDongNguon: 40, soDongDich: -1, lech: true },
  ],
  canhBao: [],
  ...p,
})

function chonTep() {
  const tep = new File(['gia'], 'goi.json.gz', { type: 'application/gzip' })
  fireEvent.change(screen.getByTestId('chon-tep-goi'), { target: { files: [tep] } })
  return tep
}

describe('NhapDuLieuPage', () => {
  it('tai danh sach giao xu va cho chay thu khi da chon tep', async () => {
    vi.mocked(api.quanTri.giaoXu.danhSach).mockResolvedValue([xu()])
    vi.mocked(api.quanTri.nhapDuLieu.xemTruoc).mockResolvedValue(baoCao())

    render(<NhapDuLieuPage />)
    await screen.findByText('Giáo xứ mới')

    const nutChayThu = screen.getByText('1. Chạy thử (không ghi gì)')
    expect(nutChayThu).toHaveProperty('disabled', true)

    chonTep()
    fireEvent.click(nutChayThu)

    await screen.findByText('Báo cáo chạy thử — nguồn: Giáo xứ Vô Nhiễm')
    expect(screen.getByText('2050')).toBeDefined()
    expect(api.quanTri.nhapDuLieu.xemTruoc).toHaveBeenCalledWith('x1', expect.any(File))
  })

  it('canh bao ro khi giao xu dich DA co du lieu', async () => {
    vi.mocked(api.quanTri.giaoXu.danhSach).mockResolvedValue([xu()])
    vi.mocked(api.quanTri.nhapDuLieu.xemTruoc).mockResolvedValue(
      baoCao({ giaoXuDichDaCoDuLieu: true, soGiaoDanDaCo: 2050 }))

    render(<NhapDuLieuPage />)
    await screen.findByText('Giáo xứ mới')
    chonTep()
    fireEvent.click(screen.getByText('1. Chạy thử (không ghi gì)'))

    expect(await screen.findByText(/ĐÃ có 2050 giáo dân/)).toBeDefined()
  })

  it('nut Nhap that chi bam duoc sau khi tich xac nhan, va goi dung API', async () => {
    vi.mocked(api.quanTri.giaoXu.danhSach).mockResolvedValue([xu()])
    vi.mocked(api.quanTri.nhapDuLieu.xemTruoc).mockResolvedValue(baoCao())
    vi.mocked(api.quanTri.nhapDuLieu.batDau).mockResolvedValue({ jobId: 'job1' })
    vi.mocked(api.quanTri.nhapDuLieu.trangThai).mockResolvedValue({
      jobId: 'job1', trangThai: 'HoanThanh',
      doiChieu: [{ bang: 'giao_dan', soDongNguon: 2050, soDongDich: 2050, lech: false }],
      canhBao: [], loiThongBao: null, batDauLuc: new Date().toISOString(), ketThucLuc: new Date().toISOString(),
    } satisfies TrangThaiNhapDuLieu)

    render(<NhapDuLieuPage />)
    await screen.findByText('Giáo xứ mới')
    chonTep()
    fireEvent.click(screen.getByText('1. Chạy thử (không ghi gì)'))
    await screen.findByText('Báo cáo chạy thử — nguồn: Giáo xứ Vô Nhiễm')

    const nutNhapThat = screen.getByText('2. Nhập thật')
    expect(nutNhapThat).toHaveProperty('disabled', true)

    fireEvent.click(screen.getByRole('checkbox'))
    fireEvent.click(nutNhapThat)

    await waitFor(() => expect(api.quanTri.nhapDuLieu.batDau).toHaveBeenCalledWith('x1', expect.any(File), true))
    await screen.findByText('Đã nhập xong', {}, { timeout: 3000 })
    expect(screen.getByText('Mọi bảng đã khớp tuyệt đối số dòng nguồn/đích.')).toBeDefined()
  })

  it('hien thong bao loi ro rang khi nhap that that bai', async () => {
    vi.mocked(api.quanTri.giaoXu.danhSach).mockResolvedValue([xu()])
    vi.mocked(api.quanTri.nhapDuLieu.xemTruoc).mockResolvedValue(baoCao())
    vi.mocked(api.quanTri.nhapDuLieu.batDau).mockResolvedValue({ jobId: 'job2' })
    vi.mocked(api.quanTri.nhapDuLieu.trangThai).mockResolvedValue({
      jobId: 'job2', trangThai: 'Loi', doiChieu: null, canhBao: null,
      loiThongBao: 'Mat ket noi CSDL giua chung', batDauLuc: new Date().toISOString(), ketThucLuc: new Date().toISOString(),
    } satisfies TrangThaiNhapDuLieu)

    render(<NhapDuLieuPage />)
    await screen.findByText('Giáo xứ mới')
    chonTep()
    fireEvent.click(screen.getByText('1. Chạy thử (không ghi gì)'))
    await screen.findByText('Báo cáo chạy thử — nguồn: Giáo xứ Vô Nhiễm')
    fireEvent.click(screen.getByRole('checkbox'))
    fireEvent.click(screen.getByText('2. Nhập thật'))

    await screen.findByText('Nhập thất bại', {}, { timeout: 3000 })
    expect(screen.getByText('Mat ket noi CSDL giua chung')).toBeDefined()
  })
})
