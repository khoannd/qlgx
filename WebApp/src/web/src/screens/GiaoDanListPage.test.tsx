import { render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'
import { GiaoDanListPage } from './GiaoDanListPage'
import { api } from '../api/client'
import type { GiaoDanListItem } from '../api/types'

vi.mock('../api/client', () => ({
  api: { giaoDan: { danhSach: vi.fn() } },
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
  giaDinhId: '00012', khongThongKe: false, ...p,
})

describe('GiaoDanListPage', () => {
  it('hien danh sach that tu API sau khi tai xong', async () => {
    vi.mocked(api.giaoDan.danhSach).mockResolvedValue([nguoi()])

    render(<GiaoDanListPage moGiaoDan={vi.fn()} />)

    expect(await screen.findByText('Trần Văn Bình')).toBeDefined()
  })

  it('loi goi API hien banner loi kem nut Thu lai, khong nuot loi', async () => {
    vi.mocked(api.giaoDan.danhSach).mockRejectedValue(new Error('500 khi gọi /api/giao-dan'))

    render(<GiaoDanListPage moGiaoDan={vi.fn()} />)

    expect(await screen.findByRole('alert')).toBeDefined()
    const nutThuLai = screen.getByRole('button', { name: 'Thử lại' })

    vi.mocked(api.giaoDan.danhSach).mockResolvedValue([nguoi()])
    nutThuLai.click()

    expect(await screen.findByText('Trần Văn Bình')).toBeDefined()
  })
})
