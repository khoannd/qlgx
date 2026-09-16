import { fireEvent, render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'
import { KiemTraDuLieuGiaDinh } from './KiemTraDuLieuGiaDinh'
import { api } from '../api/client'
import type { GiaDinhListItem, KiemTraGiaDinhKetQua } from '../api/types'

vi.mock('../api/client', () => ({
  api: { congCuDuLieu: { kiemTraGiaDinh: vi.fn() } },
}))

const giaDinh = (p: Partial<GiaDinhListItem> = {}): GiaDinhListItem => ({
  id: 'gd1', maGiaDinhCu: 501, maGiaDinhRieng: null, tenGiaDinh: 'Nguyễn Văn A',
  tenChong: 'Nguyễn Văn A', tenVo: 'Trần Thị B', soLuong: 3, dienThoai: null,
  dtChong: null, dtVo: null, diaChi: null, tenGiaoHo: 'Giáo họ Thánh Tâm',
  dienGiaDinh: null, ghiChu: null, gach: -1, khongThongKe: false, ...p,
})

const ket = (p: Partial<KiemTraGiaDinhKetQua> = {}): KiemTraGiaDinhKetQua => ({
  giaDinh: giaDinh(), nguyenNhan: '- Không có ngày hôn phối', ketQua: 1, ...p,
})

describe('KiemTraDuLieuGiaDinh', () => {
  it('luoi trong cho toi khi bam Bat dau kiem tra, roi hien ket qua tu API', async () => {
    vi.mocked(api.congCuDuLieu.kiemTraGiaDinh).mockResolvedValue([ket()])

    render(<KiemTraDuLieuGiaDinh danhMucGiaoHo={[]} moGiaDinh={vi.fn()} />)
    expect(screen.queryByText('Nguyễn Văn A')).toBeNull()

    fireEvent.click(screen.getByRole('button', { name: 'Bắt đầu kiểm tra' }))

    expect(await screen.findByText('- Không có ngày hôn phối')).toBeDefined()
    expect(api.congCuDuLieu.kiemTraGiaDinh).toHaveBeenCalledWith(undefined, {
      khongCoNgayHonPhoi: true, honPhoiTruocTuoi: true,
      khoangCachTuoiConCai: true, cacVanDeKhac: true,
    })
  })

  it('khong tim thay loi thi hien thong bao dung nhu desktop', async () => {
    vi.mocked(api.congCuDuLieu.kiemTraGiaDinh).mockResolvedValue([])

    render(<KiemTraDuLieuGiaDinh danhMucGiaoHo={[]} moGiaDinh={vi.fn()} />)
    fireEvent.click(screen.getByRole('button', { name: 'Bắt đầu kiểm tra' }))

    expect(await screen.findByText('Không tìm thấy lỗi dữ liệu của gia đình nào.')).toBeDefined()
  })

  it('bo het 4 o tick thi bao phai chon it nhat 1 loai kiem tra, khong goi API', () => {
    render(<KiemTraDuLieuGiaDinh danhMucGiaoHo={[]} moGiaDinh={vi.fn()} />)

    for (const cb of screen.getAllByRole('checkbox')) fireEvent.click(cb)
    fireEvent.click(screen.getByRole('button', { name: 'Bắt đầu kiểm tra' }))

    expect(screen.getByRole('alert').textContent).toContain('Hãy chọn ít nhất 1 loại kiểm tra')
    expect(api.congCuDuLieu.kiemTraGiaDinh).not.toHaveBeenCalled()
  })
})
