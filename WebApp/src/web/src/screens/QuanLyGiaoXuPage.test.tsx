import { render, screen, fireEvent } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'
import { QuanLyGiaoXuPage } from './QuanLyGiaoXuPage'
import { api } from '../api/client'
import type { GiaoPhan, GiaoHatQuanLy, GiaoXuQuanLy } from '../api/types'

vi.mock('../api/client', () => ({
  api: {
    quanTri: {
      giaoPhan: { danhSach: vi.fn(), tao: vi.fn(), sua: vi.fn() },
      giaoHat: { danhSach: vi.fn(), tao: vi.fn(), sua: vi.fn() },
      giaoXu: { danhSach: vi.fn(), tao: vi.fn(), sua: vi.fn(), taoTaiKhoan: vi.fn() },
    },
  },
}))

const phan = (p: Partial<GiaoPhan> = {}): GiaoPhan => ({ id: 'p1', tenGiaoPhan: 'Phan Thiết', ghiChu: null, ...p })
const hat = (p: Partial<GiaoHatQuanLy> = {}): GiaoHatQuanLy => ({
  id: 'h1', giaoPhanId: 'p1', tenGiaoPhan: 'Phan Thiết', tenGiaoHat: 'Đức Tánh', ghiChu: null, ...p,
})
const xu = (p: Partial<GiaoXuQuanLy> = {}): GiaoXuQuanLy => ({
  id: 'x1', giaoHatId: 'h1', tenGiaoHat: 'Đức Tánh', tenGiaoPhan: 'Phan Thiết',
  tenGiaoXu: 'Vô Nhiễm', diaChi: null, dienThoai: null, email: null, website: null, ghiChu: null,
  coTrungTen: false, soTaiKhoan: 1, ...p,
})

function moDanhSach() {
  vi.mocked(api.quanTri.giaoPhan.danhSach).mockResolvedValue([phan()])
  vi.mocked(api.quanTri.giaoHat.danhSach).mockResolvedValue([hat()])
  vi.mocked(api.quanTri.giaoXu.danhSach).mockResolvedValue([xu()])
}

describe('QuanLyGiaoXuPage', () => {
  it('hien du 3 cap phan cap tu API xuyen giao xu', async () => {
    moDanhSach()

    render(<QuanLyGiaoXuPage />)

    expect((await screen.findAllByText('Phan Thiết')).length).toBeGreaterThan(0)
    expect(screen.getAllByText('Đức Tánh').length).toBeGreaterThan(0)
    expect(screen.getByText('Vô Nhiễm')).toBeDefined()
  })

  it('khong co nut xoa nao o ca 3 cap (quyet dinh chan han, xem spec muc 4)', async () => {
    moDanhSach()

    render(<QuanLyGiaoXuPage />)
    await screen.findByText('Vô Nhiễm')

    expect(screen.queryByText('Xoá')).toBeNull()
  })

  it('nut Tao tai khoan quan tri mo form va goi dung API voi giaoXuId tu duong dan', async () => {
    moDanhSach()
    vi.mocked(api.quanTri.giaoXu.taoTaiKhoan).mockResolvedValue({ id: 'tk1' })

    render(<QuanLyGiaoXuPage />)
    await screen.findByText('Vô Nhiễm')
    fireEvent.click(screen.getByText('Tạo tài khoản quản trị'))
    fireEvent.change(screen.getByLabelText('Họ tên người dùng'), { target: { value: 'Văn phòng Xứ Mới' } })
    fireEvent.change(screen.getByLabelText('Tên đăng nhập'), { target: { value: 'xumoi' } })
    fireEvent.change(screen.getByLabelText('Mật khẩu'), { target: { value: 'MatKhauManh123' } })
    fireEvent.click(screen.getByText('Tạo tài khoản'))

    await vi.waitFor(() => expect(api.quanTri.giaoXu.taoTaiKhoan).toHaveBeenCalledWith('x1', {
      tenTaiKhoan: 'xumoi', matKhau: 'MatKhauManh123', hoTenNguoiDung: 'Văn phòng Xứ Mới',
      email: null, soDienThoai: null,
    }))
  })
})
