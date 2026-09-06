import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'
import { GiaDinhDetailPage } from './GiaDinhDetailPage'
import { api, LoiXungDot } from '../api/client'
import type { GiaDinhDetail as ChiTiet } from '../api/types'

vi.mock('../api/client', async () => {
  const actual = await vi.importActual<typeof import('../api/client')>('../api/client')
  return {
    LoiXungDot: actual.LoiXungDot,
    api: { giaDinh: { chiTiet: vi.fn(), capNhat: vi.fn() } },
  }
})

const chiTiet = (p: Partial<ChiTiet> = {}): ChiTiet => ({
  id: 'g1', maGiaDinhCu: 12, maGiaDinhRieng: null, tenGiaDinh: 'Nguyễn Văn A',
  giaoHoId: null, dienThoai: null, diaChi: null, soHoKhau: null, dienGiaDinh: null,
  ghiChu: null, daChuyenXu: false, ngayChuyen: null, noiChuyen: null,
  khongThongKe: false, rowVersion: 1, thanhVien: [], ...p,
} as ChiTiet)

describe('GiaDinhDetailPage', () => {
  it('tai chi tiet that tu API va hien dung ten', async () => {
    vi.mocked(api.giaDinh.chiTiet).mockResolvedValue(chiTiet())

    render(<GiaDinhDetailPage id="g1" />)

    expect(screen.getByRole('status')).toBeDefined()
    expect(await screen.findByRole('heading', { name: /Nguyễn Văn A/ })).toBeDefined()
    expect(api.giaDinh.chiTiet).toHaveBeenCalledWith('g1')
  })

  it('luu thanh cong thi goi PUT va tai lai chi tiet', async () => {
    vi.mocked(api.giaDinh.chiTiet).mockResolvedValue(chiTiet())
    vi.mocked(api.giaDinh.capNhat).mockResolvedValue(undefined)

    render(<GiaDinhDetailPage id="g1" />)
    await screen.findByLabelText('Tên gia đình')

    await userEvent.click(screen.getByRole('button', { name: 'Cập nhật' }))

    expect(api.giaDinh.capNhat).toHaveBeenCalledWith('g1', expect.objectContaining({ rowVersion: 1 }))
    expect(await screen.findByText('Đã lưu thành công.')).toBeDefined()
    expect(api.giaDinh.chiTiet).toHaveBeenCalledTimes(2)
  })

  it('xung dot RowVersion (409) hien thong bao tieng Viet, khong mat du lieu vua go', async () => {
    vi.mocked(api.giaDinh.chiTiet).mockResolvedValue(chiTiet())
    vi.mocked(api.giaDinh.capNhat).mockRejectedValue(
      new LoiXungDot('Gia đình này vừa được người khác cập nhật.'),
    )

    render(<GiaDinhDetailPage id="g1" />)
    const oTen = await screen.findByLabelText('Tên gia đình') as HTMLInputElement
    await userEvent.clear(oTen)
    await userEvent.type(oTen, 'Tên vừa sửa')

    await userEvent.click(screen.getByRole('button', { name: 'Cập nhật' }))

    expect(await screen.findByText('Gia đình này vừa được người khác cập nhật.')).toBeDefined()
    // Không bị điều hướng mất nội dung đang gõ dở
    expect(oTen.value).toBe('Tên vừa sửa')
  })
})
