import { render, screen, fireEvent } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'
import { KhoiGiaoLyListPage } from './KhoiGiaoLyListPage'
import { api } from '../api/client'
import type { KhoiGiaoLy } from '../api/types'

vi.mock('../api/client', () => ({
  api: { giaoLy: { khoi: vi.fn(), xoaKhoi: vi.fn() } },
}))

const khoi = (p: Partial<KhoiGiaoLy> = {}): KhoiGiaoLy => ({
  id: 'k1', maKhoiCu: 1, tenKhoi: 'Khai Tâm', nguoiQuanLyId: 'gd1',
  tenNguoiQuanLy: 'Cha Nguyễn Văn A', ghiChu: null, soLop: 2, rowVersion: 1, ...p,
})

describe('KhoiGiaoLyListPage', () => {
  it('hien danh sach that tu API', async () => {
    vi.mocked(api.giaoLy.khoi).mockResolvedValue([khoi()])

    render(<KhoiGiaoLyListPage moKhoi={vi.fn()} />)

    expect(await screen.findByText('Khai Tâm')).toBeDefined()
  })

  it('bam Thêm khối goi dung callback voi id null', async () => {
    vi.mocked(api.giaoLy.khoi).mockResolvedValue([])
    const moKhoi = vi.fn()

    render(<KhoiGiaoLyListPage moKhoi={moKhoi} />)
    await screen.findByText('Thêm khối')
    fireEvent.click(screen.getByText('Thêm khối'))

    expect(moKhoi).toHaveBeenCalledWith(null)
  })

  it('loi khi tai danh sach hien thong bao, khong am tham thanh rong', async () => {
    vi.mocked(api.giaoLy.khoi).mockRejectedValue(new Error('Không kết nối được máy chủ'))

    render(<KhoiGiaoLyListPage moKhoi={vi.fn()} />)

    expect(await screen.findByRole('alert')).toBeDefined()
  })
})
