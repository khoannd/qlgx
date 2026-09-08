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
})
