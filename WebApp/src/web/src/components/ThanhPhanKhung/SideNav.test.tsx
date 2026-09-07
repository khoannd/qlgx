import { render, screen, waitFor } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'
import { SideNav } from './SideNav'
import { api } from '../../api/client'

vi.mock('../../api/client', () => ({
  api: { he: { sucKhoe: vi.fn() } },
}))

describe('SideNav', () => {
  // can-review-sau.md muc 32: chan trang truoc day viet cung "Ban 4.0.0 · du lieu cuc bo" —
  // "4.0.0" la so hieu ban DESKTOP va "du lieu cuc bo" mau thuan voi mo hinh mot may chu tap
  // trung da chot. Phai hien dung phien ban BAN WEB, lay tu GET /api/suc-khoe.
  it('hien dung phien ban that cua ban web, khong con so hieu ban desktop', async () => {
    vi.mocked(api.he.sucKhoe).mockResolvedValue({ trangThai: 'ok', phienBan: '1.2.3' })

    render(<SideNav dangChonId="giaoDanList" onNavigate={() => {}} laQuanTri={false} />)

    expect(await screen.findByText('Bản web 1.2.3')).toBeDefined()
    expect(screen.queryByText(/4\.0\.0/)).toBeNull()
    expect(screen.queryByText(/dữ liệu cục bộ/)).toBeNull()
  })

  // Cau "Sao luu gan nhat: hom nay 06:15" cu la chu tinh bia ra, khong phan anh trang thai
  // sao luu that nao — da bo hoan toan, khong duoc thay bang mot loi hua sai khac.
  it('khong con dong Sao luu gan nhat bia dat', async () => {
    vi.mocked(api.he.sucKhoe).mockResolvedValue({ trangThai: 'ok', phienBan: '1.2.3' })

    render(<SideNav dangChonId="giaoDanList" onNavigate={() => {}} laQuanTri={false} />)

    await waitFor(() => expect(api.he.sucKhoe).toHaveBeenCalled())
    expect(screen.queryByText(/Sao lưu gần nhất/)).toBeNull()
  })

  it('goi suc-khoe loi thi khong crash, chi khong hien so phien ban', async () => {
    vi.mocked(api.he.sucKhoe).mockRejectedValue(new Error('mang loi'))

    render(<SideNav dangChonId="giaoDanList" onNavigate={() => {}} laQuanTri={false} />)

    await waitFor(() => expect(api.he.sucKhoe).toHaveBeenCalled())
    expect(screen.getByText('Bản web')).toBeDefined()
  })
})
