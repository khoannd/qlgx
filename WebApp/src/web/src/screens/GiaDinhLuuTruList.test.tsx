import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'
import { GiaDinhLuuTruList } from './GiaDinhLuuTruList'
import { api } from '../api/client'
import type { GiaDinhListItem } from '../api/types'

vi.mock('../api/client', () => ({
  api: {
    giaDinh: {
      xuatExcelLuuTru: vi.fn(() => Promise.resolve()),
      inChungNhanHonPhoi: vi.fn(() => Promise.resolve()),
      inPhieuGiaDinh: vi.fn(() => Promise.resolve()),
    },
  },
}))

const giaDinh = (p: Partial<GiaDinhListItem> = {}): GiaDinhListItem => ({
  id: 'g1', maGiaDinhCu: 12, maGiaDinhRieng: null, tenGiaDinh: 'Bình - Hoa',
  tenChong: 'Giuse Trần Văn Bình', tenVo: 'Maria Nguyễn Thị Hoa',
  soLuong: 4, dienThoai: null, dtChong: null, dtVo: null,
  diaChi: null, tenGiaoHo: 'Giáo họ Thánh Tâm', dienGiaDinh: null, ghiChu: null,
  gach: -1, khongThongKe: false, ...p,
})

const rows: GiaDinhListItem[] = [
  giaDinh(),
  giaDinh({ id: 'g2', maGiaDinhCu: 27, tenGiaDinh: 'Nguyễn Văn An', tenGiaoHo: 'Giáo họ Mân Côi' }),
]

describe('GiaDinhLuuTruList', () => {
  it('khong co nut Them gia dinh (khac danh sach dang hoat dong)', async () => {
    render(<GiaDinhLuuTruList rows={rows} moGiaDinh={vi.fn()} />)
    await screen.findByText('Bình - Hoa')

    expect(screen.queryByRole('button', { name: /Thêm gia đình/ })).toBeNull()
  })

  it('nhap dup mot dong thi mo dung ma gia dinh de sua', async () => {
    const moGiaDinh = vi.fn()
    render(<GiaDinhLuuTruList rows={rows} moGiaDinh={moGiaDinh} />)

    await userEvent.dblClick(await screen.findByText('Nguyễn Văn An'))

    expect(moGiaDinh).toHaveBeenCalledWith('g2')
  })

  it('bam Xoa bo hien dung hop thoai 1 lua chon (khong co Dua vao luu tru)', async () => {
    render(<GiaDinhLuuTruList rows={rows} moGiaDinh={vi.fn()} onXoa={vi.fn()} />)
    const ten = await screen.findByText('Bình - Hoa')
    await userEvent.click(ten)

    await waitFor(() => expect(screen.getByRole('button', { name: 'Xóa bỏ' })).toHaveProperty('disabled', false))
    await userEvent.click(screen.getByRole('button', { name: 'Xóa bỏ' }))

    const hopThoai = await screen.findByRole('alertdialog')
    expect(hopThoai.textContent).toContain('vĩnh viễn')
    expect(screen.getByRole('button', { name: 'Xóa vĩnh viễn' })).toBeDefined()
    expect(screen.queryByRole('button', { name: 'Đưa vào lưu trữ' })).toBeNull()
  })

  it('bam Xoa vinh vien goi onXoa CHI VOI id', async () => {
    const onXoa = vi.fn().mockResolvedValue(undefined)
    render(<GiaDinhLuuTruList rows={rows} moGiaDinh={vi.fn()} onXoa={onXoa} />)
    const ten = await screen.findByText('Bình - Hoa')
    await userEvent.click(ten)
    await waitFor(() => expect(screen.getByRole('button', { name: 'Xóa bỏ' })).toHaveProperty('disabled', false))
    await userEvent.click(screen.getByRole('button', { name: 'Xóa bỏ' }))

    await userEvent.click(screen.getByRole('button', { name: 'Xóa vĩnh viễn' }))

    expect(onXoa).toHaveBeenCalledWith('g1')
  })

  it('nut Xuat Excel goi api.giaDinh.xuatExcelLuuTru', async () => {
    render(<GiaDinhLuuTruList rows={rows} moGiaDinh={vi.fn()} />)
    await screen.findByText('Bình - Hoa')

    await userEvent.click(screen.getByRole('button', { name: 'Xuất Excel' }))

    expect(api.giaDinh.xuatExcelLuuTru).toHaveBeenCalledWith(undefined, false)
  })

  // "In sổ gia đình" (nút riêng của màn hình lưu trữ) reuse đúng endpoint/mẫu "Phiếu gia đình"
  // — xem in-an.md mục 5f, can-review-sau.md (nghiên cứu mã desktop xác nhận cùng report).
  it('nut In so gia dinh goi api.giaDinh.inPhieuGiaDinh voi dung id dang chon', async () => {
    render(<GiaDinhLuuTruList rows={rows} moGiaDinh={vi.fn()} />)
    const ten = await screen.findByText('Bình - Hoa')
    await userEvent.click(ten)
    await waitFor(() => expect(screen.getByRole('button', { name: 'In sổ gia đình' })).toHaveProperty('disabled', false))

    await userEvent.click(screen.getByRole('button', { name: 'In sổ gia đình' }))

    expect(api.giaDinh.inPhieuGiaDinh).toHaveBeenCalledWith('g1')
  })

  it('nut Tai lai goi onTaiLai', async () => {
    const onTaiLai = vi.fn()
    render(<GiaDinhLuuTruList rows={rows} moGiaDinh={vi.fn()} onTaiLai={onTaiLai} />)
    await screen.findByText('Bình - Hoa')

    await userEvent.click(screen.getByRole('button', { name: 'Tải lại' }))

    expect(onTaiLai).toHaveBeenCalledOnce()
  })
})
