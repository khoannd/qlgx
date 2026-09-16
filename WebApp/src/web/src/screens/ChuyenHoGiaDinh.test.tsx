import { fireEvent, render, screen, waitFor } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'
import { ChuyenHoGiaDinh } from './ChuyenHoGiaDinh'
import { api } from '../api/client'
import type { GiaDinhListItem, GiaoHo } from '../api/types'

vi.mock('../api/client', () => ({
  api: {
    giaDinh: { danhSach: vi.fn() },
    chuyenHo: { xemTruocGiaDinh: vi.fn(), ghiGiaDinh: vi.fn() },
  },
}))

const giaDinh = (p: Partial<GiaDinhListItem> = {}): GiaDinhListItem => ({
  id: 'gd1', maGiaDinhCu: 501, maGiaDinhRieng: null, tenGiaDinh: 'Gia đình Ông A',
  tenChong: 'Nguyễn Văn A', tenVo: 'Trần Thị B', soLuong: 3, dienThoai: null,
  dtChong: null, dtVo: null, diaChi: null, tenGiaoHo: 'Giáo họ Nguồn',
  dienGiaDinh: null, ghiChu: null, gach: -1, khongThongKe: false, ...p,
})

const giaoHoDich: GiaoHo = { id: 'gh-dich', maGiaoHoCu: 2, tenGiaoHo: 'Giáo họ Đích', giaoHoChaId: null }

/**
 * "Chuyển họ hàng loạt — gia đình" kéo theo TẤT CẢ thành viên — bài test khoá đúng: xem trước
 * phải hiện số thành viên bị kéo theo (không chỉ số gia đình), và ghi thật chỉ chạy sau khi đã
 * xác nhận đúng con số đó.
 */
describe('ChuyenHoGiaDinh', () => {
  it('xem truoc hien ca so gia dinh lan so thanh vien bi keo theo truoc khi ghi that', async () => {
    vi.mocked(api.giaDinh.danhSach).mockResolvedValue([giaDinh()])
    vi.mocked(api.chuyenHo.xemTruocGiaDinh).mockResolvedValue({
      soLuongGiaDinh: 1, soLuongThanhVien: 3, tenGiaoHoDich: 'Giáo họ Đích',
    })
    vi.mocked(api.chuyenHo.ghiGiaDinh).mockResolvedValue({ soLuongGiaDinhDaChuyen: 1, soLuongThanhVienDaChuyen: 3 })

    render(<ChuyenHoGiaDinh danhMucGiaoHo={[giaoHoDich]} />)

    await screen.findByText('Gia đình Ông A')
    fireEvent.click(screen.getByRole('checkbox', { name: /Chọn Gia đình Ông A/ }))
    fireEvent.change(screen.getByLabelText('Giáo họ đích'), { target: { value: 'gh-dich' } })
    fireEvent.click(screen.getByRole('button', { name: 'Xem trước & chuyển họ' }))

    const dongXacNhan = await screen.findByText(/Sẽ chuyển/)
    expect(api.chuyenHo.ghiGiaDinh).not.toHaveBeenCalled()
    expect(dongXacNhan.textContent).toContain('1')
    expect(dongXacNhan.textContent).toContain('3')

    fireEvent.click(screen.getByRole('button', { name: 'Xác nhận chuyển' }))

    await waitFor(() => expect(api.chuyenHo.ghiGiaDinh).toHaveBeenCalledWith(['gd1'], 'gh-dich'))
    expect(await screen.findByText(/Đã chuyển 1 gia đình \(3 thành viên\)/)).toBeDefined()
  })

  it('loi mang khi doi bo loc giao ho nguon thi BAO LOI, khong am tham giu danh sach CU (Cao #2)', async () => {
    vi.mocked(api.giaDinh.danhSach)
      .mockResolvedValueOnce([giaDinh()]) // lan dau (Tất cả) tai OK
      .mockRejectedValueOnce(new Error('Mất kết nối mạng')) // doi sang giao ho nguon: loi

    render(<ChuyenHoGiaDinh danhMucGiaoHo={[giaoHoDich]} />)
    await screen.findByText('Gia đình Ông A')

    fireEvent.change(screen.getByLabelText('Giáo họ nguồn'), { target: { value: 'gh-dich' } })

    // TRƯỚC KHI SỬA: `.catch((e: unknown) => { console.error(...) })` chỉ log, không
    // `setLoi(...)`, và `danhSach` không được reset — bảng LẶNG LẼ vẫn hiện danh sách gia đình
    // của giáo họ TRƯỚC ĐÓ, dễ dẫn tới chuyển NHẦM gia đình của giáo họ cũ sang giáo họ đích
    // (xem review toàn nhánh 2026-09-08 "Cao #2"). Assertion dưới RED nếu quay lại catch chỉ
    // log.
    expect(await screen.findByRole('alert')).toBeDefined()
    await waitFor(() => expect(screen.queryByText('Gia đình Ông A')).toBeNull())
  })

  it('ghi thanh cong nhung tai lai danh sach loi thi CHI hien thong bao thanh cong, KHONG hien loi (Cao #3)', async () => {
    vi.mocked(api.giaDinh.danhSach)
      .mockResolvedValueOnce([giaDinh()])
      .mockRejectedValueOnce(new Error('Mất kết nối mạng'))
    vi.mocked(api.chuyenHo.xemTruocGiaDinh).mockResolvedValue({
      soLuongGiaDinh: 1, soLuongThanhVien: 3, tenGiaoHoDich: 'Giáo họ Đích',
    })
    vi.mocked(api.chuyenHo.ghiGiaDinh).mockResolvedValue({ soLuongGiaDinhDaChuyen: 1, soLuongThanhVienDaChuyen: 3 })

    render(<ChuyenHoGiaDinh danhMucGiaoHo={[giaoHoDich]} />)
    await screen.findByText('Gia đình Ông A')
    fireEvent.click(screen.getByRole('checkbox', { name: /Chọn Gia đình Ông A/ }))
    fireEvent.change(screen.getByLabelText('Giáo họ đích'), { target: { value: 'gh-dich' } })
    fireEvent.click(screen.getByRole('button', { name: 'Xem trước & chuyển họ' }))
    await screen.findByText(/Sẽ chuyển/)
    fireEvent.click(screen.getByRole('button', { name: 'Xác nhận chuyển' }))

    // TRƯỚC KHI SỬA: lệnh ghi và lệnh tải lại danh sách nằm CHUNG một `try` — nếu tải lại lỗi
    // sau khi ghi đã thành công, "Đã chuyển..." và "Chuyển họ thất bại..." hiện ĐỒNG THỜI (xem
    // review toàn nhánh 2026-09-08 "Cao #3"). Hai assertion dưới RED nếu gộp lại một `try`.
    expect(await screen.findByText(/Đã chuyển 1 gia đình/)).toBeDefined()
    expect(screen.queryByText('Chuyển họ thất bại, thử lại sau.')).toBeNull()
  })
})
