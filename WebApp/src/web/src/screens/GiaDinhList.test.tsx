import { fireEvent, render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'
import { GiaDinhList } from './GiaDinhList'
import type { GiaDinhListItem } from '../api/types'
import * as csv from '../lib/csv'

const giaDinh = (p: Partial<GiaDinhListItem> = {}): GiaDinhListItem => ({
  id: 'g1', maGiaDinhCu: 12, maGiaDinhRieng: null, tenGiaDinh: 'Bình - Lan',
  tenChong: 'Giuse Trần Văn Bình', tenVo: 'Maria Nguyễn Thị Lan', soLuong: 5,
  dienThoai: null, dtChong: null, dtVo: null, diaChi: null, tenGiaoHo: 'Giáo họ Thánh Tâm',
  dienGiaDinh: null, ghiChu: null, gach: -1, khongThongKe: false, ...p,
})

const rows: GiaDinhListItem[] = [
  giaDinh(),
  giaDinh({ id: 'g2', maGiaDinhCu: 27, tenGiaDinh: 'Chính - Hạnh', tenChong: 'Phêrô Lê Văn Chính', tenVo: 'Anna Phạm Thị Hạnh', tenGiaoHo: 'Giáo họ Mân Côi' }),
  giaDinh({ id: 'g3', maGiaDinhCu: 142, tenGiaDinh: 'Vinh - Diễm', tenChong: 'Giuse Ngô Quốc Vinh', tenVo: 'Anna Bùi Thị Diễm', tenGiaoHo: 'Ngoài xứ', khongThongKe: true }),
]

describe('GiaDinhList', () => {
  it('loc theo Giao ho thu hep dung danh sach', async () => {
    render(<GiaDinhList rows={rows} moGiaDinh={vi.fn()} />)

    // ag-grid dựng hàng qua setTimeout(0) nội bộ nên phải chờ bằng findByText trước khi dùng
    // getByText/queryByText đồng bộ (xem cùng ghi chú trong GxGiaoDanList.test.tsx).
    expect(await screen.findByText('Bình - Lan')).toBeDefined()
    expect(screen.getByText('Chính - Hạnh')).toBeDefined()

    await userEvent.selectOptions(screen.getByLabelText('Giáo họ'), 'Giáo họ Thánh Tâm')

    await waitFor(() => {
      expect(screen.getByText('Bình - Lan')).toBeDefined()
      expect(screen.queryByText('Chính - Hạnh')).toBeNull()
      expect(screen.queryByText('Vinh - Diễm')).toBeNull()
    })
  })

  it('o tick Chi xem gia dinh khong duoc thong ke loc dung', async () => {
    render(<GiaDinhList rows={rows} moGiaDinh={vi.fn()} />)
    await screen.findByText('Bình - Lan')

    await userEvent.click(screen.getByLabelText('Chỉ xem gia đình không được thống kê'))

    await waitFor(() => {
      expect(screen.getByText('Vinh - Diễm')).toBeDefined()
      expect(screen.queryByText('Bình - Lan')).toBeNull()
      expect(screen.queryByText('Chính - Hạnh')).toBeNull()
    })
  })

  it('nhap dup mot dong thi mo dung ma gia dinh', async () => {
    const moGiaDinh = vi.fn()
    render(<GiaDinhList rows={rows} moGiaDinh={moGiaDinh} />)

    await userEvent.dblClick(await screen.findByText('Chính - Hạnh'))

    expect(moGiaDinh).toHaveBeenCalledWith('g2')
  })

  it('nut Them gia dinh mo ban ghi moi (id null)', async () => {
    const moGiaDinh = vi.fn()
    render(<GiaDinhList rows={rows} moGiaDinh={moGiaDinh} />)

    await userEvent.click(screen.getByRole('button', { name: 'Thêm gia đình' }))

    expect(moGiaDinh).toHaveBeenCalledWith(null)
  })

  // --- Task "thanh cong cu": Tai lai + Xuat CSV + Xoa --------------------------------------

  it('nut Tai lai goi onTaiLai', async () => {
    const onTaiLai = vi.fn()
    render(<GiaDinhList rows={rows} moGiaDinh={vi.fn()} onTaiLai={onTaiLai} />)
    await screen.findByText('Bình - Lan')

    await userEvent.click(screen.getByRole('button', { name: 'Tải lại' }))

    expect(onTaiLai).toHaveBeenCalledOnce()
  })

  it('nut Xuat du lieu (CSV) goi taiXuongCsv voi noi dung tu luoi', async () => {
    const spy = vi.spyOn(csv, 'taiXuongCsv').mockImplementation(() => {})
    render(<GiaDinhList rows={rows} moGiaDinh={vi.fn()} />)
    await screen.findByText('Bình - Lan')

    await userEvent.click(screen.getByRole('button', { name: 'Xuất dữ liệu (CSV)' }))

    expect(spy).toHaveBeenCalledOnce()
    const [noiDung, tenTep] = spy.mock.calls[0]
    expect(typeof noiDung).toBe('string')
    expect(tenTep).toMatch(/^danh-sach-gia-dinh-.*\.csv$/)
  })

  it('nut Xoa gia dinh bi vo hieu hoa khi chua chon dong nao', async () => {
    render(<GiaDinhList rows={rows} moGiaDinh={vi.fn()} onXoa={vi.fn()} />)
    await screen.findByText('Bình - Lan')

    expect(screen.getByRole('button', { name: 'Xóa gia đình' })).toHaveProperty('disabled', true)
  })

  it('chon mot dong roi bam Xoa goi onXoa dung id khi Xoa vinh vien', async () => {
    const onXoa = vi.fn().mockResolvedValue(undefined)
    render(<GiaDinhList rows={rows} moGiaDinh={vi.fn()} onXoa={onXoa} />)
    const ten = await screen.findByText('Bình - Lan')
    fireEvent.click(ten)

    await waitFor(() => expect(screen.getByRole('button', { name: 'Xóa gia đình' })).toHaveProperty('disabled', false))
    await userEvent.click(screen.getByRole('button', { name: 'Xóa gia đình' }))
    await userEvent.click(screen.getByRole('button', { name: 'Xóa vĩnh viễn' }))

    expect(onXoa).toHaveBeenCalledWith('g1', true)
  })

  it('nut In danh sach hien thong bao chua ho tro', async () => {
    const alertSpy = vi.spyOn(window, 'alert').mockImplementation(() => {})
    render(<GiaDinhList rows={rows} moGiaDinh={vi.fn()} />)
    await screen.findByText('Bình - Lan')

    await userEvent.click(screen.getByRole('button', { name: 'In danh sách' }))

    expect(alertSpy).toHaveBeenCalledOnce()
  })
})
