import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'
import { GiaoDanLuuTruList } from './GiaoDanLuuTruList'
import { api } from '../api/client'
import type { GiaoDanListItem } from '../api/types'

vi.mock('../api/client', () => ({
  api: {
    giaoDan: {
      xuatExcelLuuTru: vi.fn(() => Promise.resolve()),
      inChungNhanBiTich: vi.fn(() => Promise.resolve()),
    },
  },
}))

const nguoi = (p: Partial<GiaoDanListItem> = {}): GiaoDanListItem => ({
  id: 'p1', maGiaoDanCu: 4401, tenThanh: 'Giuse', hoTen: 'Trần Văn Bình',
  phai: 'Nam', ngaySinh: '1950-05-03', namSinh: '1950', ngayRuaToi: null,
  ngayRuocLe: null, ngayThemSuc: null, lapGd: true, hoTenCha: null, hoTenMe: null,
  tanTong: false, conHoc: false, ngheNghiep: null, ghiChu: null, dienThoai: null,
  diaChi: null, tenGiaoHo: 'Giáo họ Thánh Tâm', daChuyenDi: false,
  trinhDoVanHoa: null, trinhDoChuyenMon: null, bietNgoaiNgu: null,
  quaDoi: true, ngayQuaDoi: '2020-01-01', noiAnTang: null, noiSinh: null,
  noiRuaToi: null, noiRuocLe: null, noiThemSuc: null, quanHe: null,
  giaDinhId: null, khongThongKe: false, ...p,
})

const rows: GiaoDanListItem[] = [
  nguoi(),
  nguoi({ id: 'p2', maGiaoDanCu: 4402, hoTen: 'Nguyễn Thị Lan', phai: 'Nữ', tenGiaoHo: 'Giáo họ Mân Côi' }),
]

describe('GiaoDanLuuTruList', () => {
  it('khong co nut Them giao dan (khac danh sach dang hoat dong)', async () => {
    render(<GiaoDanLuuTruList rows={rows} moGiaoDan={vi.fn()} />)
    await screen.findByText('Trần Văn Bình')

    expect(screen.queryByRole('button', { name: /Thêm giáo dân/ })).toBeNull()
  })

  it('nhap dup mot dong thi mo dung ma giao dan de sua', async () => {
    const moGiaoDan = vi.fn()
    render(<GiaoDanLuuTruList rows={rows} moGiaoDan={moGiaoDan} />)

    await userEvent.dblClick(await screen.findByText('Nguyễn Thị Lan'))

    expect(moGiaoDan).toHaveBeenCalledWith('p2')
  })

  it('bam Xoa bo hien dung hop thoai 1 lua chon (khong co Dua vao luu tru)', async () => {
    render(<GiaoDanLuuTruList rows={rows} moGiaoDan={vi.fn()} onXoa={vi.fn()} />)
    const ten = await screen.findByText('Trần Văn Bình')
    await userEvent.click(ten)

    await waitFor(() => expect(screen.getByRole('button', { name: 'Xóa bỏ' })).toHaveProperty('disabled', false))
    await userEvent.click(screen.getByRole('button', { name: 'Xóa bỏ' }))

    const hopThoai = await screen.findByRole('alertdialog')
    expect(hopThoai.textContent).toContain('Trần Văn Bình')
    expect(hopThoai.textContent).toContain('vĩnh viễn')
    expect(screen.getByRole('button', { name: 'Xóa vĩnh viễn' })).toBeDefined()
    expect(screen.queryByRole('button', { name: 'Đưa vào lưu trữ' })).toBeNull()
  })

  it('bam Xoa vinh vien goi onXoa CHI VOI id (khong co tham so vinhVien)', async () => {
    const onXoa = vi.fn().mockResolvedValue(undefined)
    render(<GiaoDanLuuTruList rows={rows} moGiaoDan={vi.fn()} onXoa={onXoa} />)
    const ten = await screen.findByText('Trần Văn Bình')
    await userEvent.click(ten)
    await waitFor(() => expect(screen.getByRole('button', { name: 'Xóa bỏ' })).toHaveProperty('disabled', false))
    await userEvent.click(screen.getByRole('button', { name: 'Xóa bỏ' }))

    await userEvent.click(screen.getByRole('button', { name: 'Xóa vĩnh viễn' }))

    expect(onXoa).toHaveBeenCalledWith('p1')
  })

  it('loi tu onXoa (vi du con thuoc gia dinh) hien lai trong hop thoai', async () => {
    const onXoa = vi.fn().mockRejectedValue(new Error(
      'Vui lòng xóa giáo dân ra khỏi gia đình trước khi xóa giáo dân này'))
    render(<GiaoDanLuuTruList rows={rows} moGiaoDan={vi.fn()} onXoa={onXoa} />)
    const ten = await screen.findByText('Trần Văn Bình')
    await userEvent.click(ten)
    await waitFor(() => expect(screen.getByRole('button', { name: 'Xóa bỏ' })).toHaveProperty('disabled', false))
    await userEvent.click(screen.getByRole('button', { name: 'Xóa bỏ' }))
    await userEvent.click(screen.getByRole('button', { name: 'Xóa vĩnh viễn' }))

    expect(await screen.findByText(/Vui lòng xóa giáo dân ra khỏi gia đình/)).toBeDefined()
  })

  it('nut Xuat Excel goi api.giaoDan.xuatExcelLuuTru', async () => {
    render(<GiaoDanLuuTruList rows={rows} moGiaoDan={vi.fn()} />)
    await screen.findByText('Trần Văn Bình')

    await userEvent.click(screen.getByRole('button', { name: 'Xuất Excel' }))

    expect(api.giaoDan.xuatExcelLuuTru).toHaveBeenCalledWith(undefined, false)
  })

  it('nut Tai lai goi onTaiLai', async () => {
    const onTaiLai = vi.fn()
    render(<GiaoDanLuuTruList rows={rows} moGiaoDan={vi.fn()} onTaiLai={onTaiLai} />)
    await screen.findByText('Trần Văn Bình')

    await userEvent.click(screen.getByRole('button', { name: 'Tải lại' }))

    expect(onTaiLai).toHaveBeenCalledOnce()
  })
})
