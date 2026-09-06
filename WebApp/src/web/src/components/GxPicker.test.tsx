import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'
import { GxPicker } from './GxPicker'
import { api } from '../api/client'
import type { GiaoDanTimKiem } from '../api/types'

vi.mock('../api/client', () => ({
  api: { timKiem: { giaoDan: vi.fn() } },
}))

const ung = (p: Partial<GiaoDanTimKiem> = {}): GiaoDanTimKiem => ({
  id: 'gd1', maGiaoDanCu: 101, tenThanh: 'Giuse', hoTen: 'Nguyễn Văn A', phai: 'Nam',
  ngaySinh: '1990-01-01', ...p,
})

describe('GxPicker', () => {
  it('bam nut Chon mo hop tim kiem, go tu khoa thi goi API tim that', async () => {
    vi.mocked(api.timKiem.giaoDan).mockResolvedValue([ung()])
    const nguoiDung = userEvent.setup()

    render(<GxPicker value={null} />)
    await nguoiDung.click(screen.getByTitle('Chọn từ danh sách giáo dân'))
    await nguoiDung.type(screen.getByPlaceholderText('Gõ tên hoặc mã cũ để tìm…'), 'Van A')

    await waitFor(() => expect(api.timKiem.giaoDan).toHaveBeenCalledWith('Van A', 20), { timeout: 1000 })
    expect(await screen.findByText(/Nguyễn Văn A/)).toBeDefined()
  })

  it('chon mot ket qua thi goi onChon voi dung ban ghi va dong hop tim kiem', async () => {
    vi.mocked(api.timKiem.giaoDan).mockResolvedValue([ung()])
    const onChon = vi.fn()
    const nguoiDung = userEvent.setup()

    render(<GxPicker value={null} onChon={onChon} />)
    await nguoiDung.click(screen.getByTitle('Chọn từ danh sách giáo dân'))
    const dong = await screen.findByText(/Nguyễn Văn A/)
    await nguoiDung.click(dong)

    expect(onChon).toHaveBeenCalledWith(ung())
    expect(screen.queryByPlaceholderText('Gõ tên hoặc mã cũ để tìm…')).toBeNull()
  })

  it('bam nut Bo chon thi goi onBoChon', async () => {
    const onBoChon = vi.fn()
    const nguoiDung = userEvent.setup()

    render(<GxPicker value="Ai đó" onBoChon={onBoChon} />)
    await nguoiDung.click(screen.getByTitle('Bỏ chọn'))

    expect(onBoChon).toHaveBeenCalled()
  })

  it('hien ten da chon, chua chon thi hien dau gach ngang', () => {
    const { rerender } = render(<GxPicker value={null} />)
    expect(screen.getByText('—')).toBeDefined()

    rerender(<GxPicker value="Giuse Nguyễn Văn A" />)
    expect(screen.getByText('Giuse Nguyễn Văn A')).toBeDefined()
  })
})
