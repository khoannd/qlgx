import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'
import { RaoHonPhoiDetail } from './RaoHonPhoiDetail'
import { api } from '../api/client'
import type { RaoHonPhoiDetail as RaoHonPhoiDetailType } from '../api/types'

vi.mock('../api/client', () => ({
  api: {
    raoHonPhoi: {
      chiTiet: vi.fn(),
      capNhat: vi.fn(() => Promise.resolve()),
      inKetQua: vi.fn(() => Promise.resolve()),
    },
  },
  LoiXungDot: class LoiXungDot extends Error {},
}))

const chiTiet = (p: Partial<RaoHonPhoiDetailType> = {}): RaoHonPhoiDetailType => ({
  id: 'r1', maRaoHonPhoiCu: 71, tenRaoHonPhoi: 'Doi rao thu nghiem',
  giaoDan1Id: 'g1', tenGiaoDan1: 'Nguoi Mot', giaoDan2Id: 'g2', tenGiaoDan2: 'Nguoi Hai',
  ngayRaoLan1: null, ngayRaoLan2: null, ngayRaoLan3: null,
  giaoXu1: null, giaoPhan1: null, giaoXuTruoc1: null, giaoPhanTruoc1: null,
  giaoXu2: null, giaoPhan2: null, giaoXuTruoc2: null, giaoPhanTruoc2: null,
  linhMucNhan: null, giaoXuNhan: null, ghiChu: null,
  tam1: null, tam2: null, tam3: null,
  giaoXuNQ1: null, giaoPhanNQ1: null, giaoXuNQ2: null, giaoPhanNQ2: null,
  rowVersion: 1, ...p,
})

describe('RaoHonPhoiDetail — In ket qua rao hon phoi (in-an.md muc 8)', () => {
  it('nut In ket qua bi vo hieu hoa khi la doi rao moi (chua co id)', () => {
    render(<RaoHonPhoiDetail id={null} />)

    expect((screen.getByRole('button', { name: 'In kết quả rao hôn phối' }) as HTMLButtonElement).disabled).toBe(true)
  })

  it('bam In ket qua thi goi api.raoHonPhoi.inKetQua voi dung id', async () => {
    vi.mocked(api.raoHonPhoi.chiTiet).mockResolvedValue(chiTiet({ id: 'r9' }))
    render(<RaoHonPhoiDetail id="r9" />)

    await waitFor(() =>
      expect((screen.getByRole('button', { name: 'In kết quả rao hôn phối' }) as HTMLButtonElement).disabled).toBe(false))
    await userEvent.click(screen.getByRole('button', { name: 'In kết quả rao hôn phối' }))

    expect(api.raoHonPhoi.inKetQua).toHaveBeenCalledWith('r9')
  })
})
