import { render, screen, fireEvent, waitFor } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'
import { TimThayThePage } from './TimThayThePage'
import { api } from '../api/client'

vi.mock('../api/client', () => ({
  api: { timThayThe: { xemTruoc: vi.fn(), ghi: vi.fn() } },
}))

describe('TimThayThePage', () => {
  it('bao loi neu chua nhap gia tri can tim, khong goi API', () => {
    render(<TimThayThePage />)

    fireEvent.click(screen.getByText('Xem trước'))

    expect(screen.getByText('Hãy nhập giá trị cần tìm')).toBeDefined()
    expect(api.timThayThe.xemTruoc).not.toHaveBeenCalled()
  })

  it('xem truoc hien dung so ban ghi khop, chua goi ghi that', async () => {
    vi.mocked(api.timThayThe.xemTruoc).mockResolvedValue({ soBanGhiKhop: 3 })
    render(<TimThayThePage />)

    fireEvent.change(screen.getByLabelText('Giá trị cần tìm'), { target: { value: 'Sài Gòn' } })
    fireEvent.change(screen.getByLabelText('Giá trị thay thế'), { target: { value: 'TP. Hồ Chí Minh' } })
    fireEvent.click(screen.getByText('Xem trước'))

    expect(await screen.findByText('3')).toBeDefined()
    expect(api.timThayThe.ghi).not.toHaveBeenCalled()
  })

  it('xac nhan goi ghi that va hien dung thong bao nguyen van desktop', async () => {
    vi.mocked(api.timThayThe.xemTruoc).mockResolvedValue({ soBanGhiKhop: 3 })
    vi.mocked(api.timThayThe.ghi).mockResolvedValue({ soBanGhiDaThay: 3 })
    render(<TimThayThePage />)

    fireEvent.change(screen.getByLabelText('Giá trị cần tìm'), { target: { value: 'Sài Gòn' } })
    fireEvent.click(screen.getByText('Xem trước'))
    await screen.findByText('3')
    fireEvent.click(screen.getByText('Xác nhận thay thế 3 bản ghi'))

    await waitFor(() => expect(api.timThayThe.ghi).toHaveBeenCalled())
    expect(await screen.findByText('Có 3 dữ liệu được thay thế')).toBeDefined()
  })

  it('doi bang ap dung thi doi lai danh sach truong va huy xem truoc cu', async () => {
    vi.mocked(api.timThayThe.xemTruoc).mockResolvedValue({ soBanGhiKhop: 1 })
    render(<TimThayThePage />)

    expect(screen.getByRole('option', { name: 'Tên gia đình' })).toBeDefined()
    fireEvent.change(screen.getByLabelText('Áp dụng cho'), { target: { value: '0' } })

    expect(screen.getByRole('option', { name: 'Họ tên' })).toBeDefined()
  })
})
