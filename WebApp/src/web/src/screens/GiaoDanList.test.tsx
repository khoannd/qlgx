import { fireEvent, render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'
import { GiaoDanList } from './GiaoDanList'
import type { GiaoDanListItem } from '../api/types'

const nguoi = (p: Partial<GiaoDanListItem> = {}): GiaoDanListItem => ({
  id: 'p1', maGiaoDanCu: 4401, tenThanh: 'Giuse', hoTen: 'Trần Văn Bình',
  phai: 'Nam', ngaySinh: '1972-05-03', namSinh: '1972', ngayRuaToi: null,
  ngayRuocLe: null, ngayThemSuc: null, lapGd: true, hoTenCha: null, hoTenMe: null,
  tanTong: false, conHoc: false, ngheNghiep: null, ghiChu: null, dienThoai: null,
  diaChi: null, tenGiaoHo: 'Giáo họ Thánh Tâm', daChuyenDi: false,
  trinhDoVanHoa: null, trinhDoChuyenMon: null, bietNgoaiNgu: null,
  quaDoi: false, ngayQuaDoi: null, noiAnTang: null, noiSinh: null,
  noiRuaToi: null, noiRuocLe: null, noiThemSuc: null, quanHe: null,
  giaDinhId: '00012', khongThongKe: false, ...p,
})

const rows: GiaoDanListItem[] = [
  nguoi(),
  nguoi({ id: 'p2', maGiaoDanCu: 4402, hoTen: 'Nguyễn Thị Lan', phai: 'Nữ', tenGiaoHo: 'Giáo họ Mân Côi', giaDinhId: '00027' }),
  nguoi({ id: 'p3', maGiaoDanCu: 4701, hoTen: 'Vũ Thị Uyên', phai: 'Nữ', tenGiaoHo: 'Giáo họ Lộ Đức', giaDinhId: null, khongThongKe: true }),
]

describe('GiaoDanList', () => {
  it('loc theo Giao ho thu hep dung danh sach', async () => {
    render(<GiaoDanList rows={rows} moGiaoDan={vi.fn()} />)

    // ag-grid dựng hàng qua setTimeout(0) nội bộ nên phải chờ bằng findByText trước khi dùng
    // getByText/queryByText đồng bộ (xem cùng ghi chú trong GxGiaoDanList.test.tsx).
    expect(await screen.findByText('Trần Văn Bình')).toBeDefined()
    expect(screen.getByText('Nguyễn Thị Lan')).toBeDefined()

    await userEvent.selectOptions(screen.getByLabelText('Giáo họ'), 'Giáo họ Thánh Tâm')

    await waitFor(() => {
      expect(screen.getByText('Trần Văn Bình')).toBeDefined()
      expect(screen.queryByText('Nguyễn Thị Lan')).toBeNull()
      expect(screen.queryByText('Vũ Thị Uyên')).toBeNull()
    })
  })

  it('o tick Chi xem giao dan khong duoc thong ke loc theo khongThongKe, khong theo Ngoai xu', async () => {
    render(<GiaoDanList rows={rows} moGiaoDan={vi.fn()} />)
    await screen.findByText('Trần Văn Bình')

    await userEvent.click(screen.getByLabelText('Chỉ xem giáo dân không được thống kê'))

    // Vũ Thị Uyên co khongThongKe:true nhung Giao ho la "Giáo họ Lộ Đức" (khong phai "Ngoai
    // xu") — neu bo loc sai (suy tu tenGiaoHo === "Ngoai xu") thi se KHONG thay dong nay.
    await waitFor(() => {
      expect(screen.getByText('Vũ Thị Uyên')).toBeDefined()
      expect(screen.queryByText('Trần Văn Bình')).toBeNull()
      expect(screen.queryByText('Nguyễn Thị Lan')).toBeNull()
    })
  })

  it('nhap dup mot dong thi mo dung ma giao dan', async () => {
    const moGiaoDan = vi.fn()
    render(<GiaoDanList rows={rows} moGiaoDan={moGiaoDan} />)

    await userEvent.dblClick(await screen.findByText('Nguyễn Thị Lan'))

    expect(moGiaoDan).toHaveBeenCalledWith('p2')
  })

  it('muc menu Xem gia dinh mo dung MA GIA DINH, khong phai ma giao dan', async () => {
    const moGiaoDan = vi.fn()
    const moGiaDinh = vi.fn()
    render(<GiaoDanList rows={rows} moGiaoDan={moGiaoDan} moGiaDinh={moGiaDinh} />)

    const ten = await screen.findByText('Trần Văn Bình')
    const dong = ten.closest('.ag-row')!
    fireEvent.contextMenu(dong)

    await userEvent.click(await screen.findByText('Xem gia đình'))

    // p1.id = 'p1', p1.giaDinhId = '00012' — phải gọi voi '00012', tuyet doi khong phai 'p1'.
    expect(moGiaDinh).toHaveBeenCalledWith('00012')
    expect(moGiaDinh).not.toHaveBeenCalledWith('p1')
  })

  it('muc menu Xem gia dinh AN di khi giao dan chua gan voi gia dinh nao', async () => {
    render(<GiaoDanList rows={rows} moGiaoDan={vi.fn()} moGiaDinh={vi.fn()} />)

    const ten = await screen.findByText('Vũ Thị Uyên')
    const dong = ten.closest('.ag-row')!
    fireEvent.contextMenu(dong)

    expect(await screen.findByText('Xem chi tiết')).toBeDefined()
    expect(screen.queryByText('Xem gia đình')).toBeNull()
  })
})
