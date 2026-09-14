import { render, screen, waitFor } from '@testing-library/react'
import { expect, it, vi } from 'vitest'
import { BangCanhBaoSaoLuu } from './BangCanhBaoSaoLuu'
import { api } from '../api/client'

vi.mock('../api/client', () => ({ api: { saoLuu: { tinhTrang: vi.fn() } } }))

const co = (den: 'xanh' | 'vang' | 'do', loi: string | null = null) => ({
  den, saoLuuGanNhat: null, soBanSao: 0, dienTapGanNhat: null, dienTapDat: false, loiGanNhat: loi,
})

it('KHONG goi API khi khong phai quan tri he thong', () => {
  render(<BangCanhBaoSaoLuu laQuanTriHeThong={false} onMoManHinh={vi.fn()} />)
  expect(api.saoLuu.tinhTrang).not.toHaveBeenCalled()
})

it('KHONG hien gi khi den xanh', async () => {
  vi.mocked(api.saoLuu.tinhTrang).mockResolvedValue(co('xanh'))
  const { container } = render(<BangCanhBaoSaoLuu laQuanTriHeThong onMoManHinh={vi.fn()} />)
  await waitFor(() => expect(api.saoLuu.tinhTrang).toHaveBeenCalled())
  expect(container.firstChild).toBeNull()
})

it('hien bang do khi den do', async () => {
  vi.mocked(api.saoLuu.tinhTrang).mockResolvedValue(co('do', 'restic check that bai'))
  render(<BangCanhBaoSaoLuu laQuanTriHeThong onMoManHinh={vi.fn()} />)
  const bang = await screen.findByRole('alert')
  expect(bang.textContent).toMatch(/restic check that bai/)
})

it('im lang khi API loi — khong hien bang bao dong gia', async () => {
  vi.mocked(api.saoLuu.tinhTrang).mockRejectedValue(new Error('mat mang'))
  const { container } = render(<BangCanhBaoSaoLuu laQuanTriHeThong onMoManHinh={vi.fn()} />)
  await waitFor(() => expect(api.saoLuu.tinhTrang).toHaveBeenCalled())
  expect(container.firstChild).toBeNull()
})
