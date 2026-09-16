import { render, screen, fireEvent } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'
import { HoiDoanListPage } from './HoiDoanListPage'
import { api } from '../api/client'
import type { HoiDoanQuanLy } from '../api/types'

vi.mock('../api/client', () => ({
  api: { hoiDoanQuanLy: { danhSach: vi.fn(), xoa: vi.fn() } },
}))

const hoiDoan = (p: Partial<HoiDoanQuanLy> = {}): HoiDoanQuanLy => ({
  id: 'hd1', maHoiDoanCu: 1, tenHoiDoan: 'Legio Mariae', thanhBonMang: 'Đức Mẹ',
  ngayBonMang: '2024-08-22', ngayThanhLap: '1990-01-01', ghiChu: null,
  soHoiVienDangHoatDong: 3, rowVersion: 1, ...p,
})

describe('HoiDoanListPage', () => {
  it('hien danh sach that tu API', async () => {
    vi.mocked(api.hoiDoanQuanLy.danhSach).mockResolvedValue([hoiDoan()])

    render(<HoiDoanListPage moHoiDoan={vi.fn()} />)

    expect(await screen.findByText('Legio Mariae')).toBeDefined()
  })

  it('bam Thêm hội đoàn goi dung callback voi id null', async () => {
    vi.mocked(api.hoiDoanQuanLy.danhSach).mockResolvedValue([])
    const moHoiDoan = vi.fn()

    render(<HoiDoanListPage moHoiDoan={moHoiDoan} />)
    await screen.findByText('Thêm hội đoàn')
    fireEvent.click(screen.getByText('Thêm hội đoàn'))

    expect(moHoiDoan).toHaveBeenCalledWith(null)
  })

  it('loi khi tai danh sach hien thong bao, khong am tham thanh rong', async () => {
    vi.mocked(api.hoiDoanQuanLy.danhSach).mockRejectedValue(new Error('Không kết nối được máy chủ'))

    render(<HoiDoanListPage moHoiDoan={vi.fn()} />)

    expect(await screen.findByRole('alert')).toBeDefined()
  })
})
