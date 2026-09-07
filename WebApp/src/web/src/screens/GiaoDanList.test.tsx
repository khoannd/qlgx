import { fireEvent, render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'
import { GiaoDanList } from './GiaoDanList'
import { api } from '../api/client'
import type { GiaoDanListItem } from '../api/types'

vi.mock('../api/client', () => ({
  api: {
    giaoDan: {
      xuatExcel: vi.fn(() => Promise.resolve()),
    },
  },
}))

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

  // --- Task "ghi giao dan": xoa giao dan --------------------------------------------------

  it('nut Xoa giao dan bi vo hieu hoa khi chua chon dong nao', async () => {
    render(<GiaoDanList rows={rows} moGiaoDan={vi.fn()} onXoa={vi.fn()} />)
    await screen.findByText('Trần Văn Bình')

    expect(screen.getByRole('button', { name: 'Xóa giáo dân' })).toHaveProperty('disabled', true)
  })

  it('chon mot dong roi bam Xoa hien hop thoai 3 lua chon dung ten giao dan', async () => {
    render(<GiaoDanList rows={rows} moGiaoDan={vi.fn()} onXoa={vi.fn()} />)
    const ten = await screen.findByText('Trần Văn Bình')
    fireEvent.click(ten)

    await waitFor(() => expect(screen.getByRole('button', { name: 'Xóa giáo dân' })).toHaveProperty('disabled', false))
    await userEvent.click(screen.getByRole('button', { name: 'Xóa giáo dân' }))

    const hopThoai = await screen.findByRole('alertdialog')
    expect(hopThoai.textContent).toContain('Trần Văn Bình')
    expect(screen.getByRole('button', { name: 'Xóa vĩnh viễn' })).toBeDefined()
    expect(screen.getByRole('button', { name: 'Đưa vào lưu trữ' })).toBeDefined()
    expect(screen.getByRole('button', { name: 'Hủy bỏ' })).toBeDefined()
  })

  it('bam Xoa vinh vien goi onXoa voi vinhVien=true', async () => {
    const onXoa = vi.fn().mockResolvedValue(undefined)
    render(<GiaoDanList rows={rows} moGiaoDan={vi.fn()} onXoa={onXoa} />)
    const ten = await screen.findByText('Trần Văn Bình')
    fireEvent.click(ten)
    await waitFor(() => expect(screen.getByRole('button', { name: 'Xóa giáo dân' })).toHaveProperty('disabled', false))
    await userEvent.click(screen.getByRole('button', { name: 'Xóa giáo dân' }))

    await userEvent.click(screen.getByRole('button', { name: 'Xóa vĩnh viễn' }))

    expect(onXoa).toHaveBeenCalledWith('p1', true)
  })

  it('bam Dua vao luu tru goi onXoa voi vinhVien=false', async () => {
    const onXoa = vi.fn().mockResolvedValue(undefined)
    render(<GiaoDanList rows={rows} moGiaoDan={vi.fn()} onXoa={onXoa} />)
    const ten = await screen.findByText('Trần Văn Bình')
    fireEvent.click(ten)
    await waitFor(() => expect(screen.getByRole('button', { name: 'Xóa giáo dân' })).toHaveProperty('disabled', false))
    await userEvent.click(screen.getByRole('button', { name: 'Xóa giáo dân' }))

    await userEvent.click(screen.getByRole('button', { name: 'Đưa vào lưu trữ' }))

    expect(onXoa).toHaveBeenCalledWith('p1', false)
  })

  it('bam Huy bo dong hop thoai khong goi onXoa', async () => {
    const onXoa = vi.fn()
    render(<GiaoDanList rows={rows} moGiaoDan={vi.fn()} onXoa={onXoa} />)
    const ten = await screen.findByText('Trần Văn Bình')
    fireEvent.click(ten)
    await waitFor(() => expect(screen.getByRole('button', { name: 'Xóa giáo dân' })).toHaveProperty('disabled', false))
    await userEvent.click(screen.getByRole('button', { name: 'Xóa giáo dân' }))

    await userEvent.click(screen.getByRole('button', { name: 'Hủy bỏ' }))

    expect(onXoa).not.toHaveBeenCalled()
    expect(screen.queryByRole('button', { name: 'Xóa vĩnh viễn' })).toBeNull()
  })

  it('loi tu onXoa hien thi lai trong hop thoai (khong dong lai)', async () => {
    const onXoa = vi.fn().mockRejectedValue(new Error(
      'Vui lòng xóa giáo dân ra khỏi gia đình trước khi xóa giáo dân này'))
    render(<GiaoDanList rows={rows} moGiaoDan={vi.fn()} onXoa={onXoa} />)
    const ten = await screen.findByText('Trần Văn Bình')
    fireEvent.click(ten)
    await waitFor(() => expect(screen.getByRole('button', { name: 'Xóa giáo dân' })).toHaveProperty('disabled', false))
    await userEvent.click(screen.getByRole('button', { name: 'Xóa giáo dân' }))

    await userEvent.click(screen.getByRole('button', { name: 'Xóa vĩnh viễn' }))

    expect(await screen.findByText(/Vui lòng xóa giáo dân ra khỏi gia đình/)).toBeDefined()
  })

  // --- Task "thanh cong cu": Tai lai + Xuat CSV -------------------------------------------

  it('nut Tai lai goi onTaiLai', async () => {
    const onTaiLai = vi.fn()
    render(<GiaoDanList rows={rows} moGiaoDan={vi.fn()} onTaiLai={onTaiLai} />)
    await screen.findByText('Trần Văn Bình')

    await userEvent.click(screen.getByRole('button', { name: 'Tải lại' }))

    expect(onTaiLai).toHaveBeenCalledOnce()
  })

  it('nut Xuat Excel goi api.giaoDan.xuatExcel voi dung bo loc dang ap dung', async () => {
    render(<GiaoDanList rows={rows} moGiaoDan={vi.fn()} hienCaDaMat={false}
      danhMucGiaoHo={[{ id: 'gh-1', tenGiaoHo: 'Giáo họ Thánh Tâm', maGiaoHoCu: 1, giaoHoChaId: null }]} />)
    await screen.findByText('Trần Văn Bình')

    await userEvent.click(screen.getByRole('button', { name: 'Xuất Excel' }))

    expect(api.giaoDan.xuatExcel).toHaveBeenCalledWith(undefined, false, false)
  })

  it('nut Xuat Excel truyen dung giaoHoId thuc khi da chon mot Giao ho', async () => {
    render(<GiaoDanList rows={rows} moGiaoDan={vi.fn()}
      danhMucGiaoHo={[{ id: 'gh-1', tenGiaoHo: 'Giáo họ Thánh Tâm', maGiaoHoCu: 1, giaoHoChaId: null }]} />)
    await screen.findByText('Trần Văn Bình')
    await userEvent.selectOptions(screen.getByLabelText('Giáo họ'), 'Giáo họ Thánh Tâm')

    await userEvent.click(screen.getByRole('button', { name: 'Xuất Excel' }))

    expect(api.giaoDan.xuatExcel).toHaveBeenCalledWith('gh-1', false, false)
  })

  it('nut In danh sach hien thong bao chua ho tro', async () => {
    const alertSpy = vi.spyOn(window, 'alert').mockImplementation(() => {})
    render(<GiaoDanList rows={rows} moGiaoDan={vi.fn()} />)
    await screen.findByText('Trần Văn Bình')

    await userEvent.click(screen.getByRole('button', { name: 'In danh sách' }))

    expect(alertSpy).toHaveBeenCalledOnce()
  })

  it('o tick Hien ca da mat goi onDoiHienCaDaMat', async () => {
    const onDoiHienCaDaMat = vi.fn()
    render(<GiaoDanList rows={rows} moGiaoDan={vi.fn()} hienCaDaMat={false} onDoiHienCaDaMat={onDoiHienCaDaMat} />)
    await screen.findByText('Trần Văn Bình')

    await userEvent.click(screen.getByLabelText('Hiện cả người đã qua đời / đã chuyển xứ'))

    expect(onDoiHienCaDaMat).toHaveBeenCalledWith(true)
  })
})
