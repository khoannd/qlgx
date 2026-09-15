import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'
import { CanXemLaiPage } from './CanXemLaiPage'
import { api } from '../api/client'
import type { CanXemLai } from '../api/types'

vi.mock('../api/client', () => ({
  api: { canXemLai: { danhSach: vi.fn(), chon: vi.fn(), danhDauDaXuLy: vi.fn() } },
}))

const mucMau = (p: Partial<CanXemLai> = {}): CanXemLai => ({
  id: 'm1', loai: 'xung_dot', bang: 'GiaoDan', banGhiId: 'gd-1', truong: 'ngaySinh',
  lyDo: null, giaTriA: '12/03/1985', giaTriB: '12/03/1986', giaTriDangDung: '12/03/1986',
  taoLuc: '2026-09-15T00:00:00Z', ...p,
})

describe('CanXemLaiPage (Task 10, spec 9.2)', () => {
  it('khong co muc nao -> hien thong bao khong co gi can xem lai', async () => {
    vi.mocked(api.canXemLai.danhSach).mockResolvedValue([])

    render(<CanXemLaiPage />)

    expect(await screen.findByText('Không có việc gì cần xem lại.')).toBeDefined()
  })

  it('mot muc co cap gia tri A/B -> hien ca hai gia tri va gia tri dang dung', async () => {
    vi.mocked(api.canXemLai.danhSach).mockResolvedValue([mucMau()])

    render(<CanXemLaiPage />)

    expect(await screen.findByText('Giá trị A: 12/03/1985')).toBeDefined()
    expect(screen.getByText('Giá trị B: 12/03/1986')).toBeDefined()
    expect(screen.getByText('Hiện đang dùng: 12/03/1986')).toBeDefined()
  })

  it('bam "Giu gia tri A" goi dung api.canXemLai.chon(id, "A") roi tai lai danh sach', async () => {
    vi.mocked(api.canXemLai.danhSach)
      .mockResolvedValueOnce([mucMau()])
      .mockResolvedValueOnce([])
    vi.mocked(api.canXemLai.chon).mockResolvedValue(undefined)

    render(<CanXemLaiPage />)
    await screen.findByText('Giá trị A: 12/03/1985')

    await userEvent.click(screen.getByRole('button', { name: 'Giữ giá trị A' }))

    await waitFor(() => expect(api.canXemLai.chon).toHaveBeenCalledWith('m1', 'A'))
    expect(await screen.findByText('Không có việc gì cần xem lại.')).toBeDefined()
  })

  it('muc KHONG co cap A/B (chi co lyDo) -> hien nut "Danh dau da xu ly", goi dung endpoint', async () => {
    const muc = mucMau({ id: 'm2', loai: 'khong_luu_duoc', giaTriA: null, giaTriB: null, lyDo: 'May chu tu choi vi ban ghi da bi xoa.' })
    vi.mocked(api.canXemLai.danhSach)
      .mockResolvedValueOnce([muc])
      .mockResolvedValueOnce([])
    vi.mocked(api.canXemLai.danhDauDaXuLy).mockResolvedValue(undefined)

    render(<CanXemLaiPage />)
    await screen.findByText('May chu tu choi vi ban ghi da bi xoa.')
    expect(screen.queryByText(/Giá trị A/)).toBeNull()

    await userEvent.click(screen.getByRole('button', { name: 'Đánh dấu đã xử lý' }))

    await waitFor(() => expect(api.canXemLai.danhDauDaXuLy).toHaveBeenCalledWith('m2'))
    expect(await screen.findByText('Không có việc gì cần xem lại.')).toBeDefined()
  })

  it('loi khi chon gia tri (vi du 409 KhongApDuocNua) -> hien thong bao loi, KHONG lam mat muc khoi danh sach', async () => {
    vi.mocked(api.canXemLai.danhSach).mockResolvedValue([mucMau()])
    vi.mocked(api.canXemLai.chon).mockRejectedValue(new Error('Hồ sơ liên quan đã bị xoá, không áp được nữa.'))

    render(<CanXemLaiPage />)
    await screen.findByText('Giá trị A: 12/03/1985')

    await userEvent.click(screen.getByRole('button', { name: 'Giữ giá trị A' }))

    expect(await screen.findByText('Hồ sơ liên quan đã bị xoá, không áp được nữa.')).toBeDefined()
    expect(screen.getByText('Giá trị A: 12/03/1985')).toBeDefined()
  })

  it('chi mot trong hai gia tri A/B la null (du du lieu bat thuong) -> van coi la KHONG co cap, hien nut "Danh dau da xu ly"', async () => {
    // Muc dich mutation testing: bat mutant doi `&&` thanh `||` trong dieu kien `coCapAB` — voi du
    // lieu nay (chi mot ben null), `&&` cho false (danh dau da xu ly) con `||` cho true (hien cap
    // A/B, trong do mot ben la "null" hien thi thanh chu rong/khong ro nghia).
    const muc = mucMau({ id: 'm3', giaTriA: '12/03/1985', giaTriB: null, lyDo: 'Du lieu khong dong bo' })
    vi.mocked(api.canXemLai.danhSach).mockResolvedValue([muc])

    render(<CanXemLaiPage />)

    expect(await screen.findByRole('button', { name: 'Đánh dấu đã xử lý' })).toBeDefined()
    expect(screen.queryByText(/Giá trị A/)).toBeNull()
  })

  it('loi khi tai danh sach lan dau -> hien thong bao loi kem nut Thu lai', async () => {
    vi.mocked(api.canXemLai.danhSach).mockRejectedValue(new Error('Mất kết nối mạng.'))

    render(<CanXemLaiPage />)

    expect(await screen.findByRole('alert')).toHaveProperty('textContent', 'Mất kết nối mạng.')
  })
})
