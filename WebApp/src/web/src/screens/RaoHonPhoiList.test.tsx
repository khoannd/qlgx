import { render, screen, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'
import { RaoHonPhoiList } from './RaoHonPhoiList'
import type { RaoHonPhoiListItem } from '../api/types'

const rao = (p: Partial<RaoHonPhoiListItem> = {}): RaoHonPhoiListItem => ({
  id: 'r1', maRaoHonPhoiCu: 1, tenRaoHonPhoi: 'Binh - Lan',
  nguoi1: 'Giuse Binh', nguoi2: 'Maria Lan',
  ngayRaoLan1: '2026-10-04', ngayRaoLan2: '2026-10-11', ngayRaoLan3: '2026-10-18', ghiChu: null, ...p,
})

describe('RaoHonPhoiList', () => {
  it('hien dung so doi rao va cac cot chinh', async () => {
    render(
      <RaoHonPhoiList rows={[rao()]} xemTatCa={false} onDoiXemTatCa={vi.fn()}
        moRao={vi.fn()} onXoa={vi.fn()} onTaiLai={vi.fn()} />,
    )
    expect(await screen.findByText('Binh - Lan')).toBeDefined()
    expect(screen.getByText('Giuse Binh')).toBeDefined()
    const pill = document.querySelector('.count-pill')
    expect(pill).not.toBeNull()
    expect(within(pill as HTMLElement).getByText('1')).toBeDefined()
  })

  it('doi combo Hien thi goi dung callback', async () => {
    const onDoi = vi.fn()
    render(
      <RaoHonPhoiList rows={[]} xemTatCa={false} onDoiXemTatCa={onDoi}
        moRao={vi.fn()} onXoa={vi.fn()} onTaiLai={vi.fn()} />,
    )
    await userEvent.selectOptions(screen.getByLabelText('Hiển thị'), 'Xem tất cả')
    expect(onDoi).toHaveBeenCalledWith(true)
  })

  it('bam Them doi rao goi moRao(null)', async () => {
    const moRao = vi.fn()
    render(
      <RaoHonPhoiList rows={[]} xemTatCa={false} onDoiXemTatCa={vi.fn()}
        moRao={moRao} onXoa={vi.fn()} onTaiLai={vi.fn()} />,
    )
    await userEvent.click(screen.getByText('Thêm đôi rao'))
    expect(moRao).toHaveBeenCalledWith(null)
  })
})
