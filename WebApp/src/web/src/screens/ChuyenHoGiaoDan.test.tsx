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

  it('loi mang khi doi bo loc giao ho nguon thi BAO LOI, khong am tham giu danh sach CU (Cao #2)', async () => {
    vi.mocked(api.giaoDan.danhSach)
      .mockResolvedValueOnce([nguoi()]) // lan dau (Tất cả) tai OK
      .mockRejectedValueOnce(new Error('Mất kết nối mạng')) // doi sang giao ho nguon: loi

    render(<ChuyenHoGiaoDan danhMucGiaoHo={[giaoHoDich]} />)
    await screen.findByText('Giuse Trần Văn Bình')

    fireEvent.change(screen.getByLabelText('Giáo họ nguồn'), { target: { value: 'gh-dich' } })

    // TRƯỚC KHI SỬA: `.catch((e: unknown) => { console.error(...) })` chỉ log, không
    // `setLoi(...)`, và `danhSach` không được reset — bảng LẶNG LẼ vẫn hiện danh sách giáo dân
    // của giáo họ TRƯỚC ĐÓ, không có dấu hiệu gì báo lỗi, dễ dẫn tới chuyển NHẦM giáo dân của
    // giáo họ cũ sang giáo họ đích (xem review toàn nhánh 2026-09-08 "Cao #2", cùng lớp lỗi đã
    // sửa ở `GxPicker.tsx`). Assertion dưới RED nếu quay lại catch chỉ log.
    expect(await screen.findByRole('alert')).toBeDefined()
    await waitFor(() => expect(screen.queryByText('Giuse Trần Văn Bình')).toBeNull())
  })

  it('ghi thanh cong nhung tai lai danh sach loi thi CHI hien thong bao thanh cong, KHONG hien loi (Cao #3)', async () => {
    vi.mocked(api.giaoDan.danhSach)
      .mockResolvedValueOnce([nguoi()]) // tai lan dau
      .mockRejectedValueOnce(new Error('Mất kết nối mạng')) // tai lai sau khi ghi: loi
    vi.mocked(api.chuyenHo.xemTruocGiaoDan).mockResolvedValue({ soLuongGiaoDan: 1, tenGiaoHoDich: 'Giáo họ Đích' })
    vi.mocked(api.chuyenHo.ghiGiaoDan).mockResolvedValue({ soLuongDaChuyen: 1 })

    render(<ChuyenHoGiaoDan danhMucGiaoHo={[giaoHoDich]} />)
    await screen.findByText('Giuse Trần Văn Bình')
    fireEvent.click(screen.getByRole('checkbox', { name: /Chọn Trần Văn Bình/ }))
    fireEvent.change(screen.getByLabelText('Giáo họ đích'), { target: { value: 'gh-dich' } })
    fireEvent.click(screen.getByRole('button', { name: 'Xem trước & chuyển họ' }))
    await screen.findByText(/Sẽ chuyển/)
    fireEvent.click(screen.getByRole('button', { name: 'Xác nhận chuyển' }))

    // TRƯỚC KHI SỬA: lệnh ghi và lệnh tải lại danh sách nằm CHUNG một `try` — `ghiGiaoDan` chạy
    // xong đúng ở CSDL, `setThongBaoXong(...)` đã chạy, rồi tải lại danh sách lỗi thì nhảy vào
    // `catch` chung khiến "Đã chuyển..." và "Chuyển họ thất bại..." hiện ĐỒNG THỜI (xem review
    // toàn nhánh 2026-09-08 "Cao #3"). Hai assertion dưới RED nếu gộp lại một `try`.
    expect(await screen.findByText(/Đã chuyển 1 giáo dân/)).toBeDefined()
    expect(screen.queryByText('Chuyển họ thất bại, thử lại sau.')).toBeNull()
  })
})
