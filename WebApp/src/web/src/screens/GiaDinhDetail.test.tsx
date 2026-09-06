import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'
import { GiaDinhDetail } from './GiaDinhDetail'
import type { GiaDinhDetail as ChiTiet } from '../api/types'

const chiTiet = (p: Partial<ChiTiet> = {}): ChiTiet => ({
  id: 'g1', maGiaDinhCu: 12, maGiaDinhRieng: null, tenGiaDinh: 'Nguyễn Văn A',
  giaoHoId: null, dienThoai: null, diaChi: null, soHoKhau: null, dienGiaDinh: null,
  ghiChu: null, daChuyenXu: false, ngayChuyen: null, noiChuyen: null,
  khongThongKe: false, rowVersion: 1, thanhVien: [], ...p,
} as ChiTiet)

describe('GiaDinhDetail', () => {
  it('hien dung tieu de va cac truong chinh cua ho so gia dinh', () => {
    render(<GiaDinhDetail duLieu={chiTiet()} />)

    expect(screen.getByRole('heading', { name: /Nguyễn Văn A/ })).toBeDefined()
    expect(screen.getByLabelText('Tên gia đình')).toBeDefined()
    expect(screen.getByLabelText('Giáo họ')).toBeDefined()
    expect(screen.getByLabelText('Địa chỉ')).toBeDefined()
  })

  it('tick Da chuyen di xu khac thi hien Ngay chuyen va Noi chuyen', async () => {
    render(<GiaDinhDetail duLieu={chiTiet()} />)
    expect(screen.queryByLabelText('Ngày chuyển')).toBeNull()

    await userEvent.click(screen.getByLabelText('Đã chuyển đi xứ khác'))

    expect(screen.getByLabelText('Ngày chuyển')).toBeDefined()
    expect(screen.getByLabelText('Nơi chuyển')).toBeDefined()
  })

  it('nut Quay ve va Danh sach goi ham mo danh sach gia dinh', async () => {
    const moDanhSachGiaDinh = vi.fn()
    render(<GiaDinhDetail duLieu={chiTiet()} moDanhSachGiaDinh={moDanhSachGiaDinh} />)

    await userEvent.click(screen.getByRole('button', { name: '← Danh sách' }))
    await userEvent.click(screen.getByRole('button', { name: 'Quay về' }))

    expect(moDanhSachGiaDinh).toHaveBeenCalledTimes(2)
  })

  it('hien luoi thanh vien voi dung so nguoi', async () => {
    render(<GiaDinhDetail duLieu={chiTiet({
      thanhVien: [
        { giaoDanId: 'p1', vaiTro: 0, chuHo: true, tenThanh: 'Giuse', hoTen: 'Nguyễn Văn A', phai: 'Nam', ngaySinh: '1970-01-01', quaDoi: false, daXoa: false },
        { giaoDanId: 'p2', vaiTro: 2, chuHo: false, tenThanh: 'Maria', hoTen: 'Trần Thị B', phai: 'Nữ', ngaySinh: '1998-01-01', quaDoi: false, daXoa: false },
      ],
    })} />)

    expect(await screen.findByText('Trần Thị B')).toBeDefined()
  })

  it('luoi Thanh vien khac loai tru vo chong (chuHo), da hien rieng o Nguoi nam/Nguoi nu', async () => {
    render(<GiaDinhDetail duLieu={chiTiet({
      thanhVien: [
        { giaoDanId: 'p1', vaiTro: 0, chuHo: true, tenThanh: 'Giuse', hoTen: 'Nguyễn Văn A', phai: 'Nam', ngaySinh: '1970-01-01', quaDoi: false, daXoa: false },
        { giaoDanId: 'p2', vaiTro: 1, chuHo: true, tenThanh: 'Maria', hoTen: 'Trần Thị B', phai: 'Nữ', ngaySinh: '1975-01-01', quaDoi: false, daXoa: false },
        { giaoDanId: 'p3', vaiTro: 2, chuHo: false, tenThanh: 'Giuse', hoTen: 'Nguyễn Văn C', phai: 'Nam', ngaySinh: '2000-01-01', quaDoi: false, daXoa: false },
      ],
    })} />)

    expect(await screen.findByText('Nguyễn Văn C')).toBeDefined()
    expect(screen.queryByText('Nguyễn Văn A')).toBeNull()
    expect(screen.queryByText('Trần Thị B')).toBeNull()
  })

  it('Nguoi nam/Nguoi nu xac dinh theo vaiTro (0/1), khong theo chuHo', async () => {
    // chuHo chỉ đúng MỘT người (thường không phải người vợ ở đây) — nếu component còn lấy
    // theo chuHo thay vì vaiTro thì "Người nữ" sẽ hiện rỗng ("—") thay vì đúng tên vợ.
    const { container } = render(<GiaDinhDetail duLieu={chiTiet({
      thanhVien: [
        { giaoDanId: 'p1', vaiTro: 0, chuHo: true, tenThanh: 'Giuse', hoTen: 'Nguyễn Văn A', phai: 'Nam', ngaySinh: '1970-01-01', quaDoi: false, daXoa: false },
        { giaoDanId: 'p2', vaiTro: 1, chuHo: false, tenThanh: 'Maria', hoTen: 'Trần Thị B', phai: 'Nữ', ngaySinh: '1975-01-01', quaDoi: false, daXoa: false },
      ],
    })} />)

    // GxPicker dựng bằng <span id="..."> (không phải ô nhập) nên getByLabelText không tìm ra
    // được — tra trực tiếp qua id đã nối với nhãn bằng htmlFor.
    expect(container.querySelector('#gdinh-nguoinam')?.textContent).toContain('Nguyễn Văn A')
    expect(container.querySelector('#gdinh-nguoinu')?.textContent).toContain('Trần Thị B')
  })
})
