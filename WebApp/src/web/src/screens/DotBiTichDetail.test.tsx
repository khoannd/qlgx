import { render, screen, fireEvent, waitFor } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'
import { DotBiTichDetail } from './DotBiTichDetail'
import { api } from '../api/client'
import type { DotBiTichDetail as DotBiTichDetailType } from '../api/types'

vi.mock('../api/client', () => ({
  api: {
    dotBiTich: {
      chiTiet: vi.fn(),
      tao: vi.fn(),
      capNhat: vi.fn(() => Promise.resolve()),
      themNguoiNhan: vi.fn(),
      suaNguoiNhan: vi.fn(),
      xoaNguoiNhan: vi.fn(),
    },
  },
  LoiXungDot: class LoiXungDot extends Error {},
}))

const dot = (p: Partial<DotBiTichDetailType> = {}): DotBiTichDetailType => ({
  id: 'd1', maDotBiTichCu: 12, loaiBiTich: 0, ngayBiTich: '2024-03-10',
  moTa: 'Đợt rửa tội tháng 3', linhMuc: 'Cha Phêrô', noiBiTich: 'Nhà thờ xứ',
  rowVersion: 1, nguoiNhan: [], ...p,
})

describe('DotBiTichDetail — xoa ngay roi Luu (Nghiem trong #1)', () => {
  it('xoa trang o Ngay bi tich roi Cap nhat thi gui null, KHONG gui chuoi rong', async () => {
    vi.mocked(api.dotBiTich.chiTiet).mockResolvedValue(dot())

    render(<DotBiTichDetail id="d1" loaiBiTich={0} />)
    const oNgay = await screen.findByLabelText('Ngày bí tích')
    expect((oNgay as HTMLInputElement).value).toBe('10/03/2024')

    // Xoá trắng ô ngày: đặt con trỏ cuối ô rồi Backspace liên tục — đúng cách người dùng thật
    // xoá, tái hiện đúng thao tác N1 mô tả (xoá ô ngày trên bản ghi có sẵn rồi bấm Lưu).
    ;(oNgay as HTMLInputElement).focus()
    ;(oNgay as HTMLInputElement).setSelectionRange(10, 10)
    for (let i = 0; i < 10; i++) fireEvent.keyDown(oNgay, { key: 'Backspace' })
    expect((oNgay as HTMLInputElement).value).toBe('__/__/____')

    fireEvent.click(screen.getByText('Cập nhật'))

    await waitFor(() => expect(api.dotBiTich.capNhat).toHaveBeenCalled())
    const than = vi.mocked(api.dotBiTich.capNhat).mock.calls[0]![1] as { ngayBiTich: unknown }
    // TRƯỚC KHI SỬA: `onIsoChange={setNgayBiTich}` nhận thẳng chuỗi rỗng `''` từ GxDate — payload
    // gửi `ngayBiTich: ''` lên máy chủ, DTO backend `DateOnly? NgayBiTich` không bind được chuỗi
    // rỗng → 400 kỹ thuật, không nhắc gì tới "ngày" (xem can-review-sau.md, review toàn nhánh
    // 2026-09-08 "Nghiêm trọng #1"). Assertion dưới đây RED nếu quay lại `setNgayBiTich` trần.
    expect(than.ngayBiTich).toBeNull()
    expect(than.ngayBiTich).not.toBe('')
  })
})

describe('DotBiTichDetail — nut "Thu lai" khi tai loi (Nghiem trong #2)', () => {
  it('tai loi, bam Thu lai va lan sau thanh cong thi THOAT khoi man hinh loi', async () => {
    vi.mocked(api.dotBiTich.chiTiet)
      .mockRejectedValueOnce(new Error('Mất kết nối mạng'))
      .mockResolvedValueOnce(dot())

    render(<DotBiTichDetail id="d1" loaiBiTich={0} />)

    expect(await screen.findByText(/Không tải được dữ liệu/)).toBeDefined()
    fireEvent.click(screen.getByRole('button', { name: 'Thử lại' }))

    // TRƯỚC KHI SỬA: `onThuLai` gọi thẳng `api.dotBiTich.chiTiet(id).then(setDot)`, không hề
    // `setLoi(null)` — `TrangThaiTai` tiếp tục render nhánh lỗi mãi mãi dù lần gọi lại thành
    // công (xem review toàn nhánh 2026-09-08 "Nghiêm trọng #2"). Assertion dưới RED nếu quay
    // lại closure cũ.
    await waitFor(() => expect(screen.queryByText(/Không tải được dữ liệu/)).toBeNull())
    expect(await screen.findByText(/Đợt rửa tội tháng 3/)).toBeDefined()
  })
})

describe('DotBiTichDetail — luoi "Danh sach nguoi nhan" dung .fixed-h-grid (Cao #1)', () => {
  it('div boc luoi co class fixed-h-grid, khong phai chi style height rong', async () => {
    vi.mocked(api.dotBiTich.chiTiet).mockResolvedValue(dot({
      nguoiNhan: [{
        giaoDanId: 'gd1', maGiaoDanCu: 1, tenThanh: 'Giuse', hoTen: 'Nguyễn Văn A',
        phai: 'Nam', ngaySinh: null, soBiTich: null, nguoiDoDau: null, ghiChu: null,
      }],
    }))

    const { container } = render(<DotBiTichDetail id="d1" loaiBiTich={0} />)
    await screen.findByText('Danh sách người nhận (1)')

    // TRƯỚC KHI SỬA: div bọc chỉ có `style={{height:420}}` thường, KHÔNG có class
    // `fixed-h-grid` — `.table-card` (gốc của `GxGrid`) co về `height:auto` (~3px) trong trang
    // cuộn-cả-trang, ẩn hết dòng dữ liệu dù đếm đúng số dòng (xem qlgx.css:1682 và review toàn
    // nhánh 2026-09-08 "Cao #1"). jsdom không đo được chiều cao thật (không có layout engine) —
    // assertion này chỉ xác nhận ĐÚNG CLASS/selector CSS, bằng chứng chiều cao thật đo bằng
    // trình duyệt thật nằm trong báo cáo, không lặp lại ở đây.
    expect(container.querySelector('.fixed-h-grid')).not.toBeNull()
  })
})
