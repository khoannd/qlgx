import { render, screen, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'
import { DotBiTichList } from './DotBiTichList'
import type { DotBiTichListItem } from '../api/types'

const dot = (p: Partial<DotBiTichListItem> = {}): DotBiTichListItem => ({
  id: 'd1', maDotBiTichCu: 1, loaiBiTich: 0, ngayBiTich: '2024-01-01',
  moTa: 'Dot Rua Toi Giang Sinh', linhMuc: 'Lm. Nguyen Van A', noiBiTich: 'Nha tho xu', soLuong: 3, ...p,
})

const rows: DotBiTichListItem[] = [dot(), dot({ id: 'd2', maDotBiTichCu: 2, moTa: 'Dot Rua Toi Phuc Sinh', soLuong: 7 })]

describe('DotBiTichList', () => {
  it('chua chon loai bi tich thi khong hien luoi, chi hien loi nhac', () => {
    render(
      <DotBiTichList
        loaiBiTich={null} onDoiLoai={vi.fn()} tuNam="" denNam="" onDoiTuNam={vi.fn()} onDoiDenNam={vi.fn()}
        onTimKiem={vi.fn()} rows={null} onMoDot={vi.fn()} onXoa={vi.fn()} onTaiLai={vi.fn()}
      />,
    )
    expect(screen.getByText(/chọn một loại bí tích/)).toBeDefined()
  })

  it('da co ket qua thi hien luoi voi dung so dot va tong so nguoi', async () => {
    render(
      <DotBiTichList
        loaiBiTich={0} onDoiLoai={vi.fn()} tuNam="" denNam="2024" onDoiTuNam={vi.fn()} onDoiDenNam={vi.fn()}
        onTimKiem={vi.fn()} rows={rows} onMoDot={vi.fn()} onXoa={vi.fn()} onTaiLai={vi.fn()}
      />,
    )
    expect(await screen.findByText('Dot Rua Toi Giang Sinh')).toBeDefined()
    expect(screen.getByText('Dot Rua Toi Phuc Sinh')).toBeDefined()
    const pill = document.querySelector('.count-pill') as HTMLElement
    expect(within(pill).getByText('2')).toBeDefined() // 2 dot
    expect(within(pill).getByText('10')).toBeDefined() // tong 3+7 nguoi
  })

  it('bam Tim kiem goi dung callback', async () => {
    const onTimKiem = vi.fn()
    render(
      <DotBiTichList
        loaiBiTich={1} onDoiLoai={vi.fn()} tuNam="" denNam="" onDoiTuNam={vi.fn()} onDoiDenNam={vi.fn()}
        onTimKiem={onTimKiem} rows={null} onMoDot={vi.fn()} onXoa={vi.fn()} onTaiLai={vi.fn()}
      />,
    )
    await userEvent.click(screen.getByText('Tìm kiếm'))
    expect(onTimKiem).toHaveBeenCalled()
  })
})
