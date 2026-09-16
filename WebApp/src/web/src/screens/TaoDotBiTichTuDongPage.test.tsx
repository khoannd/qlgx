import { render, screen, fireEvent, waitFor } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'
import { TaoDotBiTichTuDongPage } from './TaoDotBiTichTuDongPage'
import { api } from '../api/client'
import type { TaoDotBiTichXemTruoc } from '../api/types'

vi.mock('../api/client', () => ({
  api: { taoDotBiTich: { xemTruoc: vi.fn(), ghi: vi.fn() } },
}))

const xemTruoc = (p: Partial<TaoDotBiTichXemTruoc> = {}): TaoDotBiTichXemTruoc => ({
  tongGiaoDanKhopDieuKien: 12, soDotMoiSeTao: 2, soGiaoDanMoiSeThem: 12,
  mauDotMoi: [{ ngay: '2021-03-10', linhMuc: 'Cha B', noiBiTich: 'Nhà thờ chính', soGiaoDan: 8 }],
  ...p,
})

describe('TaoDotBiTichTuDongPage', () => {
  it('bao loi neu chua chon khoang ngay, khong goi API', () => {
    render(<TaoDotBiTichTuDongPage />)

    fireEvent.click(screen.getByText('Xem trước'))

    expect(screen.getByText('Xin vui lòng chọn khoảng ngày cần tạo tự động.')).toBeDefined()
    expect(api.taoDotBiTich.xemTruoc).not.toHaveBeenCalled()
  })

  it('xem truoc hien dung con so va bang mau dot moi', async () => {
    vi.mocked(api.taoDotBiTich.xemTruoc).mockResolvedValue(xemTruoc())
    render(<TaoDotBiTichTuDongPage />)

    fireEvent.change(screen.getByLabelText('Từ ngày'), { target: { value: '2021-03-01' } })
    fireEvent.change(screen.getByLabelText('Đến ngày'), { target: { value: '2021-03-31' } })
    fireEvent.click(screen.getByText('Xem trước'))

    expect(await screen.findByText('Cha B')).toBeDefined()
    expect(screen.getAllByText('12').length).toBeGreaterThan(0)
    expect(api.taoDotBiTich.ghi).not.toHaveBeenCalled()
  })

  it('xac nhan goi ghi that va hien ket qua', async () => {
    vi.mocked(api.taoDotBiTich.xemTruoc).mockResolvedValue(xemTruoc())
    vi.mocked(api.taoDotBiTich.ghi).mockResolvedValue({ soDotDaTao: 2, soGiaoDanDaThem: 12 })
    render(<TaoDotBiTichTuDongPage />)

    fireEvent.change(screen.getByLabelText('Từ ngày'), { target: { value: '2021-03-01' } })
    fireEvent.change(screen.getByLabelText('Đến ngày'), { target: { value: '2021-03-31' } })
    fireEvent.click(screen.getByText('Xem trước'))
    await screen.findByText('Cha B')
    fireEvent.click(screen.getByText('Xác nhận tạo 2 đợt / thêm 12 giáo dân'))

    await waitFor(() => expect(api.taoDotBiTich.ghi).toHaveBeenCalled())
    expect(await screen.findByText(/Tổng số đợt bí tích được tạo: 2/)).toBeDefined()
  })
})
