import { render, screen, fireEvent, waitFor } from '@testing-library/react'
import { describe, expect, it, vi, beforeEach } from 'vitest'
import { HoiDoanDetail } from './HoiDoanDetail'
import { api } from '../api/client'
import type { HoiDoanQuanLy, ThanhVienHoiDoan } from '../api/types'

vi.mock('../api/client', () => ({
  api: {
    hoiDoanQuanLy: {
      danhSach: vi.fn(), them: vi.fn(), sua: vi.fn(), xoa: vi.fn(),
      thanhVien: vi.fn(), themThanhVien: vi.fn(), suaThanhVien: vi.fn(), xoaThanhVien: vi.fn(),
    },
    timKiem: { giaoDan: vi.fn() },
  },
  LoiXungDot: class LoiXungDot extends Error {},
}))

const hoiDoan = (p: Partial<HoiDoanQuanLy> = {}): HoiDoanQuanLy => ({
  id: 'hd1', maHoiDoanCu: 1, tenHoiDoan: 'Legio Mariae', thanhBonMang: null,
  ngayBonMang: null, ngayThanhLap: null, ghiChu: null, soHoiVienDangHoatDong: 1, rowVersion: 1, ...p,
})

const thanhVien = (p: Partial<ThanhVienHoiDoan> = {}): ThanhVienHoiDoan => ({
  chiTietId: 'tv1', giaoDanId: 'gd1', hoTen: 'Nguyen Van A', tenThanh: 'Phêrô',
  ngayVaoHoiDoan: '2024-01-01', ngayRaHoiDoan: null, vaiTro: 'Hội viên',
  daRaKhoiHoiDoan: false, rowVersion: 1, ...p,
})

describe('HoiDoanDetail', () => {
  beforeEach(() => {
    vi.mocked(api.timKiem.giaoDan).mockResolvedValue([])
  })

  it('hoi doan moi (id null) hien form trong, chua co danh sach hoi vien', () => {
    render(<HoiDoanDetail id={null} />)
    expect(screen.getByText('Hội đoàn mới')).toBeDefined()
    expect(screen.queryByText(/Danh sách hội viên/)).toBeNull()
  })

  it('tai hoi doan da co: hien dung ten va danh sach hoi vien', async () => {
    vi.mocked(api.hoiDoanQuanLy.danhSach).mockResolvedValue([hoiDoan()])
    vi.mocked(api.hoiDoanQuanLy.thanhVien).mockResolvedValue([thanhVien()])

    render(<HoiDoanDetail id="hd1" />)

    expect(await screen.findByDisplayValue('Legio Mariae')).toBeDefined()
    expect(await screen.findByText('Nguyen Van A')).toBeDefined()
    expect(screen.getByText('Danh sách hội viên (1)')).toBeDefined()
  })

  it('thieu ten hoi doan thi bao loi tieng Viet, khong goi API luu', async () => {
    render(<HoiDoanDetail id={null} />)
    fireEvent.click(screen.getByText('Cập nhật'))
    expect(await screen.findByText('Vui lòng nhập tên hội đoàn')).toBeDefined()
    expect(api.hoiDoanQuanLy.them).not.toHaveBeenCalled()
  })

  it('tao hoi doan moi thanh cong thi goi API them dung tham so', async () => {
    vi.mocked(api.hoiDoanQuanLy.them).mockResolvedValue({ id: 'hd2' })
    vi.mocked(api.hoiDoanQuanLy.danhSach).mockResolvedValue([hoiDoan({ id: 'hd2', tenHoiDoan: 'Hiền Mẫu' })])
    vi.mocked(api.hoiDoanQuanLy.thanhVien).mockResolvedValue([])
    const onDaLuu = vi.fn()

    render(<HoiDoanDetail id={null} onDaLuu={onDaLuu} />)
    fireEvent.change(screen.getByLabelText('Tên hội đoàn'), { target: { value: 'Hiền Mẫu' } })
    fireEvent.click(screen.getByText('Cập nhật'))

    await vi.waitFor(() => expect(api.hoiDoanQuanLy.them).toHaveBeenCalledWith(
      expect.objectContaining({ tenHoiDoan: 'Hiền Mẫu', rowVersion: null }),
    ))
    await vi.waitFor(() => expect(onDaLuu).toHaveBeenCalled())
  })

  it('sua thanh vien: chon dong roi luu goi dung API voi rowVersion', async () => {
    vi.mocked(api.hoiDoanQuanLy.danhSach).mockResolvedValue([hoiDoan()])
    vi.mocked(api.hoiDoanQuanLy.thanhVien).mockResolvedValue([thanhVien()])
    vi.mocked(api.hoiDoanQuanLy.suaThanhVien).mockResolvedValue(undefined)

    render(<HoiDoanDetail id="hd1" />)
    fireEvent.click(await screen.findByText('Nguyen Van A'))
    fireEvent.change(await screen.findByLabelText('Vai trò'), { target: { value: 'Trưởng hội đoàn' } })
    fireEvent.click(screen.getByText('Lưu'))

    await vi.waitFor(() => expect(api.hoiDoanQuanLy.suaThanhVien).toHaveBeenCalledWith('tv1',
      expect.objectContaining({ vaiTro: 'Trưởng hội đoàn', rowVersion: 1 })))
  })

  it('xoa thanh vien: xac nhan qua window.confirm roi goi API', async () => {
    vi.mocked(api.hoiDoanQuanLy.danhSach).mockResolvedValue([hoiDoan()])
    vi.mocked(api.hoiDoanQuanLy.thanhVien).mockResolvedValue([thanhVien()])
    vi.mocked(api.hoiDoanQuanLy.xoaThanhVien).mockResolvedValue(undefined)
    vi.spyOn(window, 'confirm').mockReturnValue(true)

    render(<HoiDoanDetail id="hd1" />)
    fireEvent.click(await screen.findByText('Nguyen Van A'))
    fireEvent.click(await screen.findByText('Xóa khỏi hội đoàn'))

    await vi.waitFor(() => expect(api.hoiDoanQuanLy.xoaThanhVien).toHaveBeenCalledWith('tv1'))
  })

  it('tai loi, bam Thu lai va lan sau thanh cong thi THOAT khoi man hinh loi (Nghiem trong #2)', async () => {
    vi.mocked(api.hoiDoanQuanLy.danhSach)
      .mockRejectedValueOnce(new Error('Mất kết nối mạng'))
      .mockResolvedValueOnce([hoiDoan()])
    vi.mocked(api.hoiDoanQuanLy.thanhVien).mockResolvedValue([])

    render(<HoiDoanDetail id="hd1" />)

    expect(await screen.findByText(/Không tải được dữ liệu/)).toBeDefined()
    fireEvent.click(screen.getByRole('button', { name: 'Thử lại' }))

    // TRƯỚC KHI SỬA: `onThuLai` gọi thẳng một closure rút gọn không hề `setLoi(null)` — màn
    // hình đứng yên ở nhánh lỗi dù lần gọi lại thành công (xem review toàn nhánh 2026-09-08
    // "Nghiêm trọng #2"). Assertion dưới RED nếu quay lại closure cũ.
    await waitFor(() => expect(screen.queryByText(/Không tải được dữ liệu/)).toBeNull())
    expect(await screen.findByText('Legio Mariae')).toBeDefined()
  })
})
