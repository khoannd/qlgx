import { render, screen, fireEvent, waitFor } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'
import { ChuanHoaDuLieuPage } from './ChuanHoaDuLieuPage'
import { api } from '../api/client'
import type { ChuanHoaXemTruoc } from '../api/types'

vi.mock('../api/client', () => ({
  api: {
    chuanHoaDuLieu: {
      xemTruocGiaoDan: vi.fn(), ghiGiaoDan: vi.fn(),
      xemTruocGiaDinh: vi.fn(), ghiGiaDinh: vi.fn(),
    },
  },
}))

const xemTruoc = (p: Partial<ChuanHoaXemTruoc> = {}): ChuanHoaXemTruoc => ({
  tongSoBanGhiKiemTra: 2050, soBanGhiSeDoi: 1,
  mauThayDoi: [{ id: 'g1', nhanDien: 'nguyễn văn an', truong: [{ tenTruong: 'HoTen', giaTriCu: 'nguyễn văn an', giaTriMoi: 'Nguyễn Văn An' }] }],
  ...p,
})

/**
 * Trung bình #1 (review toàn nhánh 2026-09-08): chuyển tab "Giáo dân"/"Gia đình" lúc tab con
 * đang xem trước/đang ghi hàng loạt khiến `key` đổi → `ChuanHoaDuLieu` bị UNMOUNT giữa chừng —
 * `setThongBaoXong(...)` gọi trên component đã gỡ, "Đã chuẩn hoá xong" không bao giờ hiện dù
 * việc ghi đã xong ở máy chủ. Bài test khoá đúng: hai nút tab phải bị VÔ HIỆU HOÁ trong lúc
 * đang xem trước/đang ghi.
 */
describe('ChuanHoaDuLieuPage — khoa nut chuyen tab luc dang xu ly', () => {
  it('dang xem truoc thi hai nut tab bi vo hieu hoa, het xem truoc thi mo lai', async () => {
    let moKhoa: (v: ChuanHoaXemTruoc) => void = () => {}
    vi.mocked(api.chuanHoaDuLieu.xemTruocGiaoDan).mockImplementation(
      () => new Promise((resolve) => { moKhoa = resolve }),
    )

    render(<ChuanHoaDuLieuPage />)
    fireEvent.click(screen.getByText('Xem trước'))

    await waitFor(() => expect((screen.getByText('Giáo dân') as HTMLButtonElement).disabled).toBe(true))
    expect((screen.getByText('Gia đình') as HTMLButtonElement).disabled).toBe(true)

    moKhoa(xemTruoc())
    await screen.findByText('Nguyễn Văn An')

    expect((screen.getByText('Giáo dân') as HTMLButtonElement).disabled).toBe(false)
    expect((screen.getByText('Gia đình') as HTMLButtonElement).disabled).toBe(false)
  })

  it('dang ghi hang loat thi hai nut tab bi vo hieu hoa, ghi xong thi mo lai va thong bao hien dung (khong bi mat do unmount)', async () => {
    vi.mocked(api.chuanHoaDuLieu.xemTruocGiaoDan).mockResolvedValue(xemTruoc())
    let moKhoaGhi: (v: { soBanGhiDaDoi: number }) => void = () => {}
    vi.mocked(api.chuanHoaDuLieu.ghiGiaoDan).mockImplementation(
      () => new Promise((resolve) => { moKhoaGhi = resolve }),
    )

    render(<ChuanHoaDuLieuPage />)
    fireEvent.click(screen.getByText('Xem trước'))
    await screen.findByText('Nguyễn Văn An')
    fireEvent.click(screen.getByText('Xác nhận chuẩn hoá 1 bản ghi'))

    await waitFor(() => expect((screen.getByText('Gia đình') as HTMLButtonElement).disabled).toBe(true))

    // TRƯỚC KHI SỬA: không có cơ chế khoá tab nào — bấm "Gia đình" ngay lúc này (nút vẫn bật)
    // sẽ đổi `key`, unmount `ChuanHoaDuLieu` đang chạy dở, và `moKhoaGhi(...)` bên dưới gọi
    // `setThongBaoXong` trên component đã gỡ — assertion cuối cùng RED nếu quay lại không khoá.
    moKhoaGhi({ soBanGhiDaDoi: 1 })

    await waitFor(() => expect((screen.getByText('Gia đình') as HTMLButtonElement).disabled).toBe(false))
    expect(await screen.findByText(/Đã chuẩn hoá xong/)).toBeDefined()
  })
})
