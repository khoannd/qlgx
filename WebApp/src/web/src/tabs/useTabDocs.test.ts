import { act, renderHook } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { useTabDocs } from './useTabDocs'

const the = (id: string, tieuDe: string) => ({ id, tieuDe, noiDung: null })

describe('useTabDocs', () => {
  it('mo hai ban ghi khac nhau thi tao hai the', () => {
    const { result } = renderHook(() => useTabDocs())

    act(() => result.current.mo(the('giaDinh:00012', 'GĐ Bình - Lan')))
    act(() => result.current.mo(the('giaDinh:00027', 'GĐ Chính - Hạnh')))

    expect(result.current.danhSach.map((t) => t.id)).toEqual([
      'giaDinh:00012',
      'giaDinh:00027',
    ])
  })

  it('mo lai dung ban ghi dang mo thi chuyen tieu diem, khong tao the trung', () => {
    const { result } = renderHook(() => useTabDocs())
    act(() => result.current.mo(the('giaDinh:00012', 'GĐ Bình - Lan')))
    act(() => result.current.mo(the('giaDinh:00027', 'GĐ Chính - Hạnh')))

    act(() => result.current.mo(the('giaDinh:00012', 'GĐ Bình - Lan')))

    expect(result.current.danhSach).toHaveLength(2)
    expect(result.current.dangChon).toBe('giaDinh:00012')
  })

  it('dong the dang chon thi chuyen sang the con lai', () => {
    const { result } = renderHook(() => useTabDocs())
    act(() => result.current.mo(the('a', 'A')))
    act(() => result.current.mo(the('b', 'B')))

    act(() => result.current.dong('b'))

    expect(result.current.danhSach.map((t) => t.id)).toEqual(['a'])
    expect(result.current.dangChon).toBe('a')
  })

  it('dong the khong phai the dang chon thi dangChon giu nguyen', () => {
    const { result } = renderHook(() => useTabDocs())
    act(() => result.current.mo(the('a', 'A')))
    act(() => result.current.mo(the('b', 'B')))
    act(() => result.current.chon('a'))

    act(() => result.current.dong('b'))

    expect(result.current.danhSach.map((t) => t.id)).toEqual(['a'])
    expect(result.current.dangChon).toBe('a')
  })

  it('dong the cuoi cung thi danhSach rong va dangChon la chuoi rong', () => {
    const { result } = renderHook(() => useTabDocs())
    act(() => result.current.mo(the('a', 'A')))

    act(() => result.current.dong('a'))

    expect(result.current.danhSach).toEqual([])
    expect(result.current.dangChon).toBe('')
  })
})
