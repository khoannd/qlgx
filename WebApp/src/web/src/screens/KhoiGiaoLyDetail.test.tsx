import { render, screen, fireEvent, waitFor } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'
import { KhoiGiaoLyDetail } from './KhoiGiaoLyDetail'
import { api } from '../api/client'
import type { KhoiGiaoLy } from '../api/types'

vi.mock('../api/client', () => ({
  api: {
    giaoLy: {
      khoi: vi.fn(), themKhoi: vi.fn(), suaKhoi: vi.fn(), xoaKhoi: vi.fn(), lop: vi.fn(),
    },
    timKiem: { giaoDan: vi.fn() },
  },
  LoiXungDot: class LoiXungDot extends Error {},
}))

const khoi = (p: Partial<KhoiGiaoLy> = {}): KhoiGiaoLy => ({
  id: 'khoi1', maKhoiCu: 1, tenKhoi: 'Khối Rước lễ', nguoiQuanLyId: 'gd1',
  tenNguoiQuanLy: 'Nguyễn Văn Quản', ghiChu: null, soLop: 0, rowVersion: 1, ...p,
})

describe('KhoiGiaoLyDetail — nut "Thu lai" khi tai loi (Nghiem trong #2)', () => {
  it('tai loi, bam Thu lai va lan sau thanh cong thi THOAT khoi man hinh loi', async () => {
    vi.mocked(api.giaoLy.khoi)
      .mockRejectedValueOnce(new Error('Mất kết nối mạng'))
      .mockResolvedValueOnce([khoi()])
    vi.mocked(api.giaoLy.lop).mockResolvedValue([])

    render(<KhoiGiaoLyDetail id="khoi1" onTieuDe={() => {}} moLop={() => {}} />)

    expect(await screen.findByText(/Không tải được dữ liệu/)).toBeDefined()
    fireEvent.click(screen.getByRole('button', { name: 'Thử lại' }))

    // TRƯỚC KHI SỬA: `onThuLai` gọi thẳng một closure rút gọn không hề `setLoi(null)` — màn
    // hình đứng yên ở nhánh lỗi dù lần gọi lại thành công (xem review toàn nhánh 2026-09-08
    // "Nghiêm trọng #2"). Assertion dưới RED nếu quay lại closure cũ.
    await waitFor(() => expect(screen.queryByText(/Không tải được dữ liệu/)).toBeNull())
    expect(await screen.findByText('Khối Rước lễ')).toBeDefined()
  })
})
