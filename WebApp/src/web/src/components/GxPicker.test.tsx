import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'
import { GxPicker } from './GxPicker'
import { api } from '../api/client'
import type { GiaoDanTimKiem } from '../api/types'

vi.mock('../api/client', () => ({
  api: { timKiem: { giaoDan: vi.fn() } },
}))

const ung = (p: Partial<GiaoDanTimKiem> = {}): GiaoDanTimKiem => ({
  id: 'gd1', maGiaoDanCu: 101, tenThanh: 'Giuse', hoTen: 'Nguyễn Văn A', phai: 'Nam',
  ngaySinh: '1990-01-01', ...p,
})

describe('GxPicker', () => {
  it('bam nut Chon mo hop tim kiem, go tu khoa thi goi API tim that', async () => {
    vi.mocked(api.timKiem.giaoDan).mockResolvedValue([ung()])
    const nguoiDung = userEvent.setup()

    render(<GxPicker value={null} />)
    await nguoiDung.click(screen.getByTitle('Chọn từ danh sách giáo dân'))
    await nguoiDung.type(screen.getByPlaceholderText('Gõ tên hoặc mã cũ để tìm…'), 'Van A')

    await waitFor(() => expect(api.timKiem.giaoDan).toHaveBeenCalledWith('Van A', 20), { timeout: 1000 })
    expect(await screen.findByText(/Nguyễn Văn A/)).toBeDefined()
  })

  it('chon mot ket qua thi goi onChon voi dung ban ghi va dong hop tim kiem', async () => {
    vi.mocked(api.timKiem.giaoDan).mockResolvedValue([ung()])
    const onChon = vi.fn()
    const nguoiDung = userEvent.setup()

    render(<GxPicker value={null} onChon={onChon} />)
    await nguoiDung.click(screen.getByTitle('Chọn từ danh sách giáo dân'))
    const dong = await screen.findByText(/Nguyễn Văn A/)
    await nguoiDung.click(dong)

    expect(onChon).toHaveBeenCalledWith(ung())
    expect(screen.queryByPlaceholderText('Gõ tên hoặc mã cũ để tìm…')).toBeNull()
  })

  // Kiem thu kham pha 2026-09-07 muc 5: nut "+" truoc day im lang khong lam gi khi bam, nguoi
  // dung khong biet la hong hay chua ho tro. Vo hieu hoa han kem tooltip ro rang khi khong co
  // noi goi thuc su noi onThemMoi.
  it('nut Them moi bi vo hieu hoa kem tooltip "chua ho tro" khi chua co onThemMoi', () => {
    render(<GxPicker value={null} />)

    const nutThemMoi = screen.getByTitle('Thêm giáo dân mới — chưa hỗ trợ') as HTMLButtonElement
    expect(nutThemMoi.disabled).toBe(true)
  })

  it('nut Them moi goi duoc onThemMoi khi noi goi co truyen vao', async () => {
    const onThemMoi = vi.fn()
    const nguoiDung = userEvent.setup()

    render(<GxPicker value={null} onThemMoi={onThemMoi} />)
    // Tooltip đổi sang KHÔNG còn hậu tố "— chưa hỗ trợ" khi nơi gọi đã nối `onThemMoi` thật —
    // giữ hậu tố đó thì tooltip nói dối lúc chức năng đã chạy được (xem GxPicker.tsx).
    const nutThemMoi = screen.getByTitle('Thêm giáo dân mới') as HTMLButtonElement
    expect(nutThemMoi.disabled).toBe(false)
    await nguoiDung.click(nutThemMoi)

    expect(onThemMoi).toHaveBeenCalled()
  })

  it('bam nut Bo chon thi goi onBoChon', async () => {
    const onBoChon = vi.fn()
    const nguoiDung = userEvent.setup()

    render(<GxPicker value="Ai đó" onBoChon={onBoChon} />)
    await nguoiDung.click(screen.getByTitle('Bỏ chọn'))

    expect(onBoChon).toHaveBeenCalled()
  })

  // Loi so 6 (kiem thu nguoi dung 2026-09-07): o Nguoi nam/Nguoi nu tren man hinh gia dinh
  // chi co nut chon/them/xoa, thieu nut mo ho so giao dan trong the moi.
  it('co onXem va da chon nguoi: hien nut "Mo ho so trong the moi", bam thi goi onXem', async () => {
    const onXem = vi.fn()
    const nguoiDung = userEvent.setup()

    render(<GxPicker value="Giuse Nguyễn Văn A" onXem={onXem} />)
    await nguoiDung.click(screen.getByTitle('Mở hồ sơ trong thẻ mới'))

    expect(onXem).toHaveBeenCalled()
  })

  it('co onXem nhung CHUA chon ai: nut "Mo ho so trong the moi" bi vo hieu hoa', () => {
    render(<GxPicker value={null} onXem={vi.fn()} />)

    const nut = screen.getByTitle('Mở hồ sơ trong thẻ mới') as HTMLButtonElement
    expect(nut.disabled).toBe(true)
  })

  it('khong truyen onXem thi KHONG hien nut "Mo ho so trong the moi"', () => {
    render(<GxPicker value="Giuse Nguyễn Văn A" />)

    expect(screen.queryByTitle('Mở hồ sơ trong thẻ mới')).toBeNull()
  })

  it('hien ten da chon, chua chon thi hien dau gach ngang', () => {
    const { rerender } = render(<GxPicker value={null} />)
    expect(screen.getByText('—')).toBeDefined()

    rerender(<GxPicker value="Giuse Nguyễn Văn A" />)
    expect(screen.getByText('Giuse Nguyễn Văn A')).toBeDefined()
  })

  // Review-frontend mục "Cao #1": lỗi mạng KHÔNG được hiện y hệt "không tìm thấy" — nếu không
  // phân biệt, nhân viên tưởng người đó chưa có trong hệ thống rồi tạo bản ghi trùng.
  it('loi mang khi tim PHAI hien thong bao loi ro rang, KHONG duoc hien "khong tim thay"', async () => {
    vi.mocked(api.timKiem.giaoDan).mockRejectedValue(
      new Error('Mất kết nối mạng. Dữ liệu bạn đã nhập vẫn được giữ nguyên trên máy — hãy thử lại khi có mạng.'),
    )
    const nguoiDung = userEvent.setup()

    render(<GxPicker value={null} />)
    await nguoiDung.click(screen.getByTitle('Chọn từ danh sách giáo dân'))
    await nguoiDung.type(screen.getByPlaceholderText('Gõ tên hoặc mã cũ để tìm…'), 'Van A')

    expect((await screen.findByRole('alert')).textContent).toContain('Mất kết nối mạng')
    expect(screen.queryByText('Không tìm thấy giáo dân nào')).toBeNull()
  })

  it('tim that su khong ra ket qua (0 phan tu, khong loi) thi hien dung "khong tim thay"', async () => {
    vi.mocked(api.timKiem.giaoDan).mockResolvedValue([])
    const nguoiDung = userEvent.setup()

    render(<GxPicker value={null} />)
    await nguoiDung.click(screen.getByTitle('Chọn từ danh sách giáo dân'))
    await nguoiDung.type(screen.getByPlaceholderText('Gõ tên hoặc mã cũ để tìm…'), 'Khong ai ten nay')

    expect(await screen.findByText('Không tìm thấy giáo dân nào')).toBeDefined()
    expect(screen.queryByRole('alert')).toBeNull()
  })

  it('bam Thu lai sau loi mang thi goi lai API tim kiem', async () => {
    vi.mocked(api.timKiem.giaoDan).mockRejectedValueOnce(new Error('Không kết nối được máy chủ'))
    const nguoiDung = userEvent.setup()

    render(<GxPicker value={null} />)
    await nguoiDung.click(screen.getByTitle('Chọn từ danh sách giáo dân'))
    await nguoiDung.type(screen.getByPlaceholderText('Gõ tên hoặc mã cũ để tìm…'), 'Van A')
    await screen.findByRole('alert')

    vi.mocked(api.timKiem.giaoDan).mockResolvedValueOnce([ung()])
    await nguoiDung.click(screen.getByText('Thử lại'))

    expect(await screen.findByText(/Nguyễn Văn A/)).toBeDefined()
    expect(api.timKiem.giaoDan).toHaveBeenCalledTimes(2)
  })
})
