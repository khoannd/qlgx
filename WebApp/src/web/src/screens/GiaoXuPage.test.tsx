import { render, screen, fireEvent, waitFor } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'
import { GiaoXuPage } from './GiaoXuPage'
import { api } from '../api/client'
import type { GiaoXuHienTai, LinhMuc } from '../api/types'

vi.mock('../api/client', () => ({
  api: {
    giaoXu: { layThongTin: vi.fn(), capNhat: vi.fn() },
    linhMuc: { danhSach: vi.fn(), them: vi.fn(), sua: vi.fn(), xoa: vi.fn() },
  },
}))

const tt = (p: Partial<GiaoXuHienTai> = {}): GiaoXuHienTai => ({
  id: 'x1', tenGiaoXu: 'Vô Nhiễm', diaChi: 'Đường ABC', dienThoai: '0123', email: null, website: null,
  ghiChu: null, tenGiaoPhan: null, tenGiaoHat: null, ...p,
})

const chaQuanXu = (p: Partial<LinhMuc> = {}): LinhMuc => ({
  id: 'lm1', maLinhMucCu: 1, tenThanh: 'Phanxicô Xaviê', hoTen: 'Võ Quang Thanh',
  ngaySinh: null, chucVu: 'Chánh xứ', tuNgay: '2004-07-12', denNgay: null,
  ghiChu: null, dienThoai: null, email: null, ...p,
})

describe('GiaoXuPage', () => {
  it('hien dung thong tin giao xu cua minh sau khi tai', async () => {
    vi.mocked(api.giaoXu.layThongTin).mockResolvedValue(tt())
    vi.mocked(api.linhMuc.danhSach).mockResolvedValue([])

    render(<GiaoXuPage />)

    expect(await screen.findByDisplayValue('Vô Nhiễm')).toBeDefined()
    expect(screen.getByDisplayValue('Đường ABC')).toBeDefined()
  })

  it('bam Cap nhat goi dung API voi du lieu da sua', async () => {
    vi.mocked(api.giaoXu.layThongTin).mockResolvedValue(tt())
    vi.mocked(api.giaoXu.capNhat).mockResolvedValue(undefined)
    vi.mocked(api.linhMuc.danhSach).mockResolvedValue([])

    render(<GiaoXuPage />)
    await screen.findByDisplayValue('Vô Nhiễm')
    fireEvent.change(screen.getByLabelText('Tên giáo xứ'), { target: { value: 'Giáo xứ Mới' } })
    fireEvent.click(screen.getByText('Cập nhật'))

    await waitFor(() => expect(api.giaoXu.capNhat).toHaveBeenCalledWith({
      tenGiaoXu: 'Giáo xứ Mới', diaChi: 'Đường ABC', dienThoai: '0123', email: null, website: null, ghiChu: null,
    }))
    expect(await screen.findByText('Đã cập nhật thông tin giáo xứ!')).toBeDefined()
  })

  it('bao loi ngay tren giao dien neu xoa trang ten giao xu, khong goi API', async () => {
    vi.mocked(api.giaoXu.layThongTin).mockResolvedValue(tt())
    vi.mocked(api.linhMuc.danhSach).mockResolvedValue([])

    render(<GiaoXuPage />)
    await screen.findByDisplayValue('Vô Nhiễm')
    fireEvent.change(screen.getByLabelText('Tên giáo xứ'), { target: { value: '' } })
    fireEvent.click(screen.getByText('Cập nhật'))

    expect(await screen.findByText('Hãy nhập tên giáo xứ!')).toBeDefined()
    expect(api.giaoXu.capNhat).not.toHaveBeenCalled()
  })

  // --- Giáo phận/Giáo hạt chỉ đọc (UX review 2026-09-08 mục 3, sửa 2026-09-10) ---

  it('chua duoc gan giao hat thi hien "Chua gan", khong phai o trong', async () => {
    vi.mocked(api.giaoXu.layThongTin).mockResolvedValue(tt({ tenGiaoPhan: null, tenGiaoHat: null }))
    vi.mocked(api.linhMuc.danhSach).mockResolvedValue([])

    render(<GiaoXuPage />)

    expect(await screen.findAllByDisplayValue('Chưa gán')).toHaveLength(2)
  })

  it('hien ten giao phan/giao hat CHI DOC, khong co o sua', async () => {
    vi.mocked(api.giaoXu.layThongTin).mockResolvedValue(
      tt({ tenGiaoPhan: 'Giáo phận Thử Nghiệm', tenGiaoHat: 'Giáo hạt Thử Nghiệm' }),
    )
    vi.mocked(api.linhMuc.danhSach).mockResolvedValue([])

    render(<GiaoXuPage />)

    const oGiaoPhan = await screen.findByDisplayValue('Giáo phận Thử Nghiệm')
    const oGiaoHat = screen.getByDisplayValue('Giáo hạt Thử Nghiệm')
    expect(oGiaoPhan).toHaveProperty('disabled', true)
    expect(oGiaoHat).toHaveProperty('disabled', true)
  })

  // --- Danh sách các cha quản xứ (UX review mục 3, thêm 2026-09-10) ---

  it('hien danh sach cac cha quan xu da co', async () => {
    vi.mocked(api.giaoXu.layThongTin).mockResolvedValue(tt())
    vi.mocked(api.linhMuc.danhSach).mockResolvedValue([chaQuanXu()])

    render(<GiaoXuPage />)

    expect(await screen.findByText('Võ Quang Thanh')).toBeDefined()
    expect(screen.getByText('Chánh xứ')).toBeDefined()
    expect(screen.getByText('12/07/2004')).toBeDefined()
  })

  it('danh sach rong thi bao "Chua co cha quan xu nao"', async () => {
    vi.mocked(api.giaoXu.layThongTin).mockResolvedValue(tt())
    vi.mocked(api.linhMuc.danhSach).mockResolvedValue([])

    render(<GiaoXuPage />)

    expect(await screen.findByText('Chưa có cha quản xứ nào.')).toBeDefined()
  })

  it('them cha quan xu moi goi dung API roi tai lai danh sach', async () => {
    vi.mocked(api.giaoXu.layThongTin).mockResolvedValue(tt())
    vi.mocked(api.linhMuc.danhSach).mockResolvedValueOnce([]).mockResolvedValueOnce([chaQuanXu()])
    vi.mocked(api.linhMuc.them).mockResolvedValue(undefined)

    render(<GiaoXuPage />)
    await screen.findByText('Chưa có cha quản xứ nào.')
    fireEvent.click(screen.getByText('+ Thêm'))
    fireEvent.change(screen.getByLabelText('Họ tên'), { target: { value: 'Võ Quang Thanh' } })
    fireEvent.click(screen.getByText('Lưu'))

    await waitFor(() => expect(api.linhMuc.them).toHaveBeenCalledWith(expect.objectContaining({
      hoTen: 'Võ Quang Thanh',
    })))
    expect(await screen.findByText('Võ Quang Thanh', { selector: 'td' })).toBeDefined()
  })

  it('xoa cha quan xu hoi xac nhan roi goi dung API', async () => {
    vi.mocked(api.giaoXu.layThongTin).mockResolvedValue(tt())
    vi.mocked(api.linhMuc.danhSach).mockResolvedValueOnce([chaQuanXu()]).mockResolvedValueOnce([])
    vi.mocked(api.linhMuc.xoa).mockResolvedValue(undefined)
    vi.spyOn(window, 'confirm').mockReturnValue(true)

    render(<GiaoXuPage />)
    await screen.findByText('Võ Quang Thanh')
    fireEvent.click(screen.getByText('Xoá'))

    await waitFor(() => expect(api.linhMuc.xoa).toHaveBeenCalledWith('lm1'))
  })
})
