import { fireEvent, render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'
import { KiemTraDuLieuGiaoDan } from './KiemTraDuLieuGiaoDan'
import { api } from '../api/client'
import type { GiaoDanListItem, KiemTraGiaoDanKetQua } from '../api/types'

vi.mock('../api/client', () => ({
  api: { congCuDuLieu: { kiemTraGiaoDan: vi.fn() } },
}))

const nguoi = (p: Partial<GiaoDanListItem> = {}): GiaoDanListItem => ({
  id: 'p1', maGiaoDanCu: 4401, tenThanh: 'Giuse', hoTen: 'Trần Văn Bình',
  phai: 'Nam', ngaySinh: '1972-05-03', namSinh: '1972', ngayRuaToi: null,
  ngayRuocLe: null, ngayThemSuc: null, lapGd: true, hoTenCha: null, hoTenMe: null,
  tanTong: false, conHoc: false, ngheNghiep: null, ghiChu: null, dienThoai: null,
  diaChi: null, tenGiaoHo: 'Giáo họ Thánh Tâm', daChuyenDi: false,
  trinhDoVanHoa: null, trinhDoChuyenMon: null, bietNgoaiNgu: null,
  quaDoi: false, ngayQuaDoi: null, noiAnTang: null, noiSinh: null,
  noiRuaToi: null, noiRuocLe: null, noiThemSuc: null, quanHe: null,
  giaDinhId: null, khongThongKe: false, ...p,
})

const ket = (p: Partial<KiemTraGiaoDanKetQua> = {}): KiemTraGiaoDanKetQua => ({
  giaoDan: nguoi(), nguyenNhan: '- Sai quan hệ ngày tháng', ketQua: 2, ...p,
})

describe('KiemTraDuLieuGiaoDan', () => {
  it('luoi trong cho toi khi bam Bat dau kiem tra, roi hien ket qua tu API', async () => {
    vi.mocked(api.congCuDuLieu.kiemTraGiaoDan).mockResolvedValue([ket()])

    render(<KiemTraDuLieuGiaoDan danhMucGiaoHo={[]} moGiaoDan={vi.fn()} />)

    expect(screen.queryByText('Trần Văn Bình')).toBeNull()

    fireEvent.click(screen.getByRole('button', { name: 'Bắt đầu kiểm tra' }))

    expect(await screen.findByText('Trần Văn Bình')).toBeDefined()
    expect(api.congCuDuLieu.kiemTraGiaoDan).toHaveBeenCalledWith(undefined, {
      khongCoNgayThang: true, saiQuanHeNgayThang: true, ruocLeTruocTuoi: true,
      thuocNhieuGiaDinh: true, khongThuocGiaDinhNao: true, coNhieuHonPhoi: true,
    })
  })

  it('khong tim thay loi thi hien thong bao dung nhu desktop', async () => {
    vi.mocked(api.congCuDuLieu.kiemTraGiaoDan).mockResolvedValue([])

    render(<KiemTraDuLieuGiaoDan danhMucGiaoHo={[]} moGiaoDan={vi.fn()} />)
    fireEvent.click(screen.getByRole('button', { name: 'Bắt đầu kiểm tra' }))

    expect(await screen.findByText('Không tìm thấy lỗi dữ liệu của giáo dân nào.')).toBeDefined()
  })

  it('bo het 6 o tick thi bao phai chon it nhat 1 loai kiem tra, khong goi API', () => {
    render(<KiemTraDuLieuGiaoDan danhMucGiaoHo={[]} moGiaoDan={vi.fn()} />)

    for (const cb of screen.getAllByRole('checkbox')) fireEvent.click(cb)
    fireEvent.click(screen.getByRole('button', { name: 'Bắt đầu kiểm tra' }))

    expect(screen.getByRole('alert').textContent).toContain('Hãy chọn ít nhất 1 loại kiểm tra')
    expect(api.congCuDuLieu.kiemTraGiaoDan).not.toHaveBeenCalled()
  })
})
