import { render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'
import { GiaDinhListPage } from './GiaDinhListPage'
import { api } from '../api/client'
import type { GiaDinhListItem } from '../api/types'

vi.mock('../api/client', () => ({
  api: { giaDinh: { danhSach: vi.fn() } },
}))

const giaDinh = (p: Partial<GiaDinhListItem> = {}): GiaDinhListItem => ({
  id: 'g1', maGiaDinhCu: 12, maGiaDinhRieng: null, tenGiaDinh: 'Bình - Lan',
  tenChong: 'Giuse Trần Văn Bình', tenVo: 'Maria Nguyễn Thị Lan', soLuong: 5,
  dienThoai: null, dtChong: null, dtVo: null, diaChi: null, tenGiaoHo: 'Giáo họ Thánh Tâm',
  dienGiaDinh: null, ghiChu: null, gach: -1, khongThongKe: false, ...p,
})

describe('GiaDinhListPage', () => {
  it('hien trang thai dang tai roi hien danh sach that tu API', async () => {
    vi.mocked(api.giaDinh.danhSach).mockResolvedValue([giaDinh()])

    render(<GiaDinhListPage moGiaDinh={vi.fn()} />)

    expect(screen.getByRole('status')).toBeDefined()
    expect(await screen.findByText('Bình - Lan')).toBeDefined()
    expect(api.giaDinh.danhSach).toHaveBeenCalled()
  })

  it('loi goi API hien thong bao tieng Viet, KHONG am tham thanh danh sach rong', async () => {
    vi.mocked(api.giaDinh.danhSach).mockRejectedValue(new Error('Không kết nối được máy chủ'))

    render(<GiaDinhListPage moGiaDinh={vi.fn()} />)

    expect(await screen.findByRole('alert')).toBeDefined()
    expect(screen.getByText(/Không kết nối được máy chủ/)).toBeDefined()
    expect(screen.queryByText('gia đình')).toBeNull()
  })
})
