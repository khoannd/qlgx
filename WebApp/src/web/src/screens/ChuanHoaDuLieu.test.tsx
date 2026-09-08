import { render, screen, fireEvent, waitFor } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'
import { ChuanHoaDuLieu } from './ChuanHoaDuLieu'
import type { ChuanHoaXemTruoc } from '../api/types'

const xemTruoc = (p: Partial<ChuanHoaXemTruoc> = {}): ChuanHoaXemTruoc => ({
  tongSoBanGhiKiemTra: 2050, soBanGhiSeDoi: 1,
  mauThayDoi: [{ id: 'g1', nhanDien: 'nguyễn văn an', truong: [{ tenTruong: 'HoTen', giaTriCu: 'nguyễn văn an', giaTriMoi: 'Nguyễn Văn An' }] }],
  ...p,
})

describe('ChuanHoaDuLieu', () => {
  it('hien nut Xem truoc luc dau, chua goi ghi that', () => {
    const goiGhiThat = vi.fn()
    render(<ChuanHoaDuLieu nhan="giáo dân" moTaXacNhan="Xac nhan?" goiXemTruoc={vi.fn()} goiGhiThat={goiGhiThat} />)

    expect(screen.getByText('Xem trước')).toBeDefined()
    expect(goiGhiThat).not.toHaveBeenCalled()
  })

  it('bam Xem truoc hien dung con so va bang mau thay doi, chua ghi gi', async () => {
    const goiXemTruoc = vi.fn().mockResolvedValue(xemTruoc())
    const goiGhiThat = vi.fn()
    render(<ChuanHoaDuLieu nhan="giáo dân" moTaXacNhan="Xac nhan?" goiXemTruoc={goiXemTruoc} goiGhiThat={goiGhiThat} />)

    fireEvent.click(screen.getByText('Xem trước'))

    expect(await screen.findByText('2050')).toBeDefined()
    expect(screen.getByText('Nguyễn Văn An')).toBeDefined()
    expect(goiGhiThat).not.toHaveBeenCalled()
  })

  it('bam Xac nhan chuan hoa moi goi ghi that va hien so ban ghi da doi', async () => {
    const goiXemTruoc = vi.fn().mockResolvedValue(xemTruoc())
    const goiGhiThat = vi.fn().mockResolvedValue({ soBanGhiDaDoi: 1 })
    render(<ChuanHoaDuLieu nhan="giáo dân" moTaXacNhan="Xac nhan?" goiXemTruoc={goiXemTruoc} goiGhiThat={goiGhiThat} />)

    fireEvent.click(screen.getByText('Xem trước'))
    await screen.findByText('Nguyễn Văn An')
    fireEvent.click(screen.getByText('Xác nhận chuẩn hoá 1 bản ghi'))

    await waitFor(() => expect(goiGhiThat).toHaveBeenCalled())
    expect(await screen.findByText(/Đã chuẩn hoá xong/)).toBeDefined()
  })

  it('bam Huy khong goi ghi that', async () => {
    const goiXemTruoc = vi.fn().mockResolvedValue(xemTruoc())
    const goiGhiThat = vi.fn()
    render(<ChuanHoaDuLieu nhan="giáo dân" moTaXacNhan="Xac nhan?" goiXemTruoc={goiXemTruoc} goiGhiThat={goiGhiThat} />)

    fireEvent.click(screen.getByText('Xem trước'))
    await screen.findByText('Nguyễn Văn An')
    fireEvent.click(screen.getByText('Huỷ'))

    expect(screen.getByText('Xem trước')).toBeDefined()
    expect(goiGhiThat).not.toHaveBeenCalled()
  })
})
