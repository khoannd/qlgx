import { fireEvent, render, screen, waitFor } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'
import { ChuyenHoGiaoDan } from './ChuyenHoGiaoDan'
import { api } from '../api/client'
import type { GiaoDanListItem, GiaoHo } from '../api/types'

vi.mock('../api/client', () => ({
  api: {
    giaoDan: { danhSach: vi.fn() },
    chuyenHo: { xemTruocGiaoDan: vi.fn(), ghiGiaoDan: vi.fn() },
  },
}))

const nguoi = (p: Partial<GiaoDanListItem> = {}): GiaoDanListItem => ({
  id: 'p1', maGiaoDanCu: 4401, tenThanh: 'Giuse', hoTen: 'Trần Văn Bình',
  phai: 'Nam', ngaySinh: null, namSinh: '', ngayRuaToi: null,
  ngayRuocLe: null, ngayThemSuc: null, lapGd: true, hoTenCha: null, hoTenMe: null,
  tanTong: false, conHoc: false, ngheNghiep: null, ghiChu: null, dienThoai: null,
  diaChi: null, tenGiaoHo: 'Giáo họ Nguồn', daChuyenDi: false,
  trinhDoVanHoa: null, trinhDoChuyenMon: null, bietNgoaiNgu: null,
  quaDoi: false, ngayQuaDoi: null, noiAnTang: null, noiSinh: null,
  noiRuaToi: null, noiRuocLe: null, noiThemSuc: null, quanHe: null,
  giaDinhId: null, khongThongKe: false, ...p,
})

const giaoHoDich: GiaoHo = { id: 'gh-dich', maGiaoHoCu: 2, tenGiaoHo: 'Giáo họ Đích', giaoHoChaId: null }

/**
 * "Chuyển họ hàng loạt — giáo dân" là công cụ sửa dữ liệu hàng loạt — bài test khoá đúng thứ
 * tự bắt buộc: KHÔNG BAO GIỜ được gọi ghiGiaoDan trước khi đã gọi xemTruocGiaoDan và người
 * dùng bấm "Xác nhận chuyển" ở đúng con số máy chủ trả về.
 */
describe('ChuyenHoGiaoDan', () => {
  it('phai xem truoc va xac nhan dung con so truoc khi goi API ghi that', async () => {
    vi.mocked(api.giaoDan.danhSach).mockResolvedValue([nguoi()])
    vi.mocked(api.chuyenHo.xemTruocGiaoDan).mockResolvedValue({ soLuongGiaoDan: 1, tenGiaoHoDich: 'Giáo họ Đích' })
    vi.mocked(api.chuyenHo.ghiGiaoDan).mockResolvedValue({ soLuongDaChuyen: 1 })

    render(<ChuyenHoGiaoDan danhMucGiaoHo={[giaoHoDich]} />)

    await screen.findByText('Giuse Trần Văn Bình')
    fireEvent.click(screen.getByRole('checkbox', { name: /Chọn Trần Văn Bình/ }))
    fireEvent.change(screen.getByLabelText('Giáo họ đích'), { target: { value: 'gh-dich' } })

    fireEvent.click(screen.getByRole('button', { name: 'Xem trước & chuyển họ' }))
    await screen.findByText(/Sẽ chuyển/)
    expect(api.chuyenHo.ghiGiaoDan).not.toHaveBeenCalled()
    expect(screen.getByText(/Sẽ chuyển/).textContent).toContain('1')

    fireEvent.click(screen.getByRole('button', { name: 'Xác nhận chuyển' }))

    await waitFor(() => expect(api.chuyenHo.ghiGiaoDan).toHaveBeenCalledWith(['p1'], 'gh-dich'))
    expect(await screen.findByText(/Đã chuyển 1 giáo dân/)).toBeDefined()
  })

  it('chua chon giao ho dich thi bao loi, khong goi xem truoc', async () => {
    vi.mocked(api.giaoDan.danhSach).mockResolvedValue([nguoi()])

    render(<ChuyenHoGiaoDan danhMucGiaoHo={[giaoHoDich]} />)
    await screen.findByText('Giuse Trần Văn Bình')
    fireEvent.click(screen.getByRole('checkbox', { name: /Chọn Trần Văn Bình/ }))

    fireEvent.click(screen.getByRole('button', { name: 'Xem trước & chuyển họ' }))

    expect(screen.getByRole('alert').textContent).toContain('Xin vui lòng chọn giáo họ đích')
    expect(api.chuyenHo.xemTruocGiaoDan).not.toHaveBeenCalled()
  })

  it('chua chon giao dan nao thi bao loi, khong goi xem truoc', async () => {
    vi.mocked(api.giaoDan.danhSach).mockResolvedValue([nguoi()])

    render(<ChuyenHoGiaoDan danhMucGiaoHo={[giaoHoDich]} />)
    await screen.findByText('Giuse Trần Văn Bình')
    fireEvent.change(screen.getByLabelText('Giáo họ đích'), { target: { value: 'gh-dich' } })

    fireEvent.click(screen.getByRole('button', { name: 'Xem trước & chuyển họ' }))

    expect(screen.getByRole('alert').textContent).toContain('chọn ít nhất 1 giáo dân')
    expect(api.chuyenHo.xemTruocGiaoDan).not.toHaveBeenCalled()
  })
})
