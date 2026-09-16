import { render, screen, fireEvent } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'
import { GiaoHoListPage } from './GiaoHoListPage'
import { api } from '../api/client'
import type { GiaoHo } from '../api/types'

vi.mock('../api/client', () => ({
  api: { giaoHo: { danhMuc: vi.fn(), them: vi.fn(), sua: vi.fn() } },
}))

const giaoHo = (p: Partial<GiaoHo> = {}): GiaoHo => ({
  id: 'gh1', maGiaoHoCu: 1, tenGiaoHo: 'Giáo họ Thánh Tâm', giaoHoChaId: null, ...p,
})

describe('GiaoHoListPage', () => {
  it('hien danh sach that tu API', async () => {
    vi.mocked(api.giaoHo.danhMuc).mockResolvedValue([giaoHo()])

    render(<GiaoHoListPage />)

    expect(await screen.findByText('Giáo họ Thánh Tâm')).toBeDefined()
  })

  it('them giao ho moi goi dung API roi tai lai danh sach', async () => {
    vi.mocked(api.giaoHo.danhMuc).mockResolvedValue([])
    vi.mocked(api.giaoHo.them).mockResolvedValue(undefined)

    render(<GiaoHoListPage />)
    await screen.findByText('+ Thêm giáo họ')
    fireEvent.click(screen.getByText('+ Thêm giáo họ'))
    fireEvent.change(screen.getByLabelText('Tên giáo họ'), { target: { value: 'Giáo họ Mới' } })
    fireEvent.click(screen.getByText('Lưu'))

    await vi.waitFor(() => expect(api.giaoHo.them).toHaveBeenCalledWith({ tenGiaoHo: 'Giáo họ Mới', giaoHoChaId: null }))
  })

  it('them giao ho con chon dung giao ho cha tu select', async () => {
    vi.mocked(api.giaoHo.danhMuc).mockResolvedValue([giaoHo({ id: 'cha1', tenGiaoHo: 'Giáo họ Cha' })])
    vi.mocked(api.giaoHo.them).mockResolvedValue(undefined)

    render(<GiaoHoListPage />)
    await screen.findByText('Giáo họ Cha')
    fireEvent.click(screen.getByText('+ Thêm giáo họ'))
    fireEvent.change(screen.getByLabelText(/Tên giáo họ/), { target: { value: 'Giáo khu Con' } })
    fireEvent.change(screen.getByLabelText(/Giáo họ cha/), { target: { value: 'cha1' } })
    fireEvent.click(screen.getByText('Lưu'))

    await vi.waitFor(() => expect(api.giaoHo.them).toHaveBeenCalledWith(
      { tenGiaoHo: 'Giáo khu Con', giaoHoChaId: 'cha1' }))
  })

  it('loi khi tai danh sach hien thong bao, khong am tham thanh rong', async () => {
    vi.mocked(api.giaoHo.danhMuc).mockRejectedValue(new Error('Không kết nối được máy chủ'))

    render(<GiaoHoListPage />)

    expect(await screen.findByRole('alert')).toBeDefined()
  })
})
