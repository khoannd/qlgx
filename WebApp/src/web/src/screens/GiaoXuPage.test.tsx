import { render, screen, fireEvent, waitFor } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'
import { GiaoXuPage } from './GiaoXuPage'
import { api } from '../api/client'
import type { GiaoXuHienTai } from '../api/types'

vi.mock('../api/client', () => ({
  api: {
    giaoXu: { layThongTin: vi.fn(), capNhat: vi.fn() },
  },
}))

const tt = (p: Partial<GiaoXuHienTai> = {}): GiaoXuHienTai => ({
  id: 'x1', tenGiaoXu: 'Vô Nhiễm', diaChi: 'Đường ABC', dienThoai: '0123', email: null, website: null,
  ghiChu: null, ...p,
})

describe('GiaoXuPage', () => {
  it('hien dung thong tin giao xu cua minh sau khi tai', async () => {
    vi.mocked(api.giaoXu.layThongTin).mockResolvedValue(tt())

    render(<GiaoXuPage />)

    expect(await screen.findByDisplayValue('Vô Nhiễm')).toBeDefined()
    expect(screen.getByDisplayValue('Đường ABC')).toBeDefined()
  })

  it('bam Cap nhat goi dung API voi du lieu da sua', async () => {
    vi.mocked(api.giaoXu.layThongTin).mockResolvedValue(tt())
    vi.mocked(api.giaoXu.capNhat).mockResolvedValue(undefined)

    render(<GiaoXuPage />)
    await screen.findByDisplayValue('Vô Nhiễm')
    fireEvent.change(screen.getByLabelText('Tên giáo xứ'), { target: { value: 'Giáo xứ Mới' } })
    fireEvent.click(screen.getByText('Cập nhật'))

    await waitFor(() => expect(api.giaoXu.capNhat).toHaveBeenCalledWith({
      tenGiaoXu: 'Giáo xứ Mới', diaChi: 'Đường ABC', dienThoai: '0123', email: null, website: null, ghiChu: null,
    }))
    expect(await screen.findByText('Đã cập nhật thông tin giáo xứ!')).toBeDefined()
  })

  it('bao loi ngay tren giao dien neu xoa trang ten giao xu, khong goi API', async () => {
    vi.mocked(api.giaoXu.layThongTin).mockResolvedValue(tt())

    render(<GiaoXuPage />)
    await screen.findByDisplayValue('Vô Nhiễm')
    fireEvent.change(screen.getByLabelText('Tên giáo xứ'), { target: { value: '' } })
    fireEvent.click(screen.getByText('Cập nhật'))

    expect(await screen.findByText('Hãy nhập tên giáo xứ!')).toBeDefined()
    expect(api.giaoXu.capNhat).not.toHaveBeenCalled()
  })
})
