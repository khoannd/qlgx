import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { SaoLuuPage, nhanNguon, dinhDangKichThuoc } from './SaoLuuPage'
import { api } from '../api/client'

vi.mock('../api/client', () => ({
  api: { saoLuu: {
    tinhTrang: vi.fn(), danhSach: vi.fn(), congViecGanDay: vi.fn(),
    taoCongViec: vi.fn(), congViec: vi.fn(),
    duongDanTaiVe: (id: string) => `/api/sao-luu/tai-ve/${id}`,
  } },
}))

const tinhTrangXanh = {
  den: 'xanh' as const, saoLuuGanNhat: '2026-09-13T06:00:00Z', soBanSao: 47,
  dienTapGanNhat: '2026-09-08T03:00:00Z', dienTapDat: true, loiGanNhat: null,
}
const motBanSao = {
  id: 'ab12cd34', thoiDiem: '2026-09-13T06:00:00Z', nhan: 'tu-dong',
  kichThuocByte: 12_582_912, soGiaoDan: 2050, soGiaDinh: 40, nguon: 'tu_dong' as const,
}

beforeEach(() => {
  vi.mocked(api.saoLuu.tinhTrang).mockResolvedValue(tinhTrangXanh)
  vi.mocked(api.saoLuu.danhSach).mockResolvedValue([motBanSao])
  vi.mocked(api.saoLuu.congViecGanDay).mockResolvedValue([])
})

describe('dinhDangKichThuoc', () => {
  it('doi byte sang don vi doc duoc', () => {
    expect(dinhDangKichThuoc(0)).toBe('0 B')
    expect(dinhDangKichThuoc(12_582_912)).toBe('12,0 MB')
  })
})

describe('nhanNguon', () => {
  it('dich du bon nguon sang tieng Viet', () => {
    expect(nhanNguon('tu_dong')).toBe('Tự động')
    expect(nhanNguon('thu_cong')).toBe('Thủ công')
    expect(nhanNguon('truoc_cap_nhat')).toBe('Trước cập nhật')
    expect(nhanNguon('truoc_phuc_hoi')).toBe('Trước phục hồi')
  })
})

describe('SaoLuuPage', () => {
  it('hien den xanh va so ban sao', async () => {
    render(<SaoLuuPage />)
    expect(await screen.findByText(/Bình thường/)).toBeDefined()
    expect(screen.getByText(/47 bản sao/)).toBeDefined()
  })

  it('hien thoi diem theo dd/MM/yyyy HH:mm, KHONG phai ISO', async () => {
    render(<SaoLuuPage />)
    await screen.findByText(/Bình thường/)
    expect(screen.queryByText(/2026-09-13T/)).toBeNull()
    expect(screen.getAllByText(/13\/09\/2026 \d{2}:\d{2}/).length).toBeGreaterThan(0)
  })

  it('den do hien canh bao noi bat', async () => {
    vi.mocked(api.saoLuu.tinhTrang).mockResolvedValue(
      { ...tinhTrangXanh, den: 'do', loiGanNhat: 'restic check that bai' })
    render(<SaoLuuPage />)
    expect(await screen.findByText(/restic check that bai/)).toBeDefined()
  })

  it('bam "Sao luu ngay" thi tao cong viec loai sao_luu', async () => {
    vi.mocked(api.saoLuu.taoCongViec).mockResolvedValue({ id: 'job-1' })
    vi.mocked(api.saoLuu.congViec).mockResolvedValue({
      id: 'job-1', loai: 'sao_luu', trangThai: 'xong', buocHienTai: null,
      nhatKy: null, taoLuc: '2026-09-13T07:00:00Z', batDauLuc: null, ketThucLuc: null,
    })
    render(<SaoLuuPage />)
    await screen.findByText(/Bình thường/)

    await userEvent.click(screen.getByRole('button', { name: /Sao lưu ngay/ }))

    await waitFor(() => expect(api.saoLuu.taoCongViec)
      .toHaveBeenCalledWith(expect.objectContaining({ loai: 'sao_luu' })))
  })

  it('bao loi ro rang khi tao cong viec that bai, khong im lang', async () => {
    vi.mocked(api.saoLuu.taoCongViec).mockRejectedValue(new Error('Đang có một công việc chạy dở'))
    render(<SaoLuuPage />)
    await screen.findByText(/Bình thường/)

    await userEvent.click(screen.getByRole('button', { name: /Sao lưu ngay/ }))

    expect(await screen.findByText(/Đang có một công việc chạy dở/)).toBeDefined()
  })

  it('canh bao ro rang ngay tai nut tai ve rang tep chua ma hoa', async () => {
    render(<SaoLuuPage />)
    await screen.findByText(/Bình thường/)
    expect(screen.getByText(/chưa mã hoá/i)).toBeDefined()
  })
})
