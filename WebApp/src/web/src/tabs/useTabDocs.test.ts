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

  // Loi so 2 (kiem thu nguoi dung 2026-09-07): tieu de the luon ghi tinh "Gia dinh"/"Giao dan"
  // du mo may ban ghi khac nhau, khong phan biet noi. `suaTieuDe` doi lai tieu de mot the DANG
  // MO khi du lieu that tai xong (xem GiaDinhDetailPage/GiaoDanDetailPage).
  it('suaTieuDe doi dung tieu de cua the co id khop, khong dung the khac', () => {
    const { result } = renderHook(() => useTabDocs())
    act(() => result.current.mo(the('giaDinh:1', 'Gia đình')))
    act(() => result.current.mo(the('giaDinh:2', 'Gia đình')))

    act(() => result.current.suaTieuDe('giaDinh:1', 'Paul Trần Văn Thái'))

    expect(result.current.danhSach.map((t) => t.tieuDe)).toEqual(['Paul Trần Văn Thái', 'Gia đình'])
  })

  it('suaTieuDe voi id khong ton tai (the da dong) thi khong lam gi, khong nem loi', () => {
    const { result } = renderHook(() => useTabDocs())
    act(() => result.current.mo(the('giaDinh:1', 'Gia đình')))
    act(() => result.current.dong('giaDinh:1'))

    act(() => result.current.suaTieuDe('giaDinh:1', 'Tên tải xong sau khi đã đóng thẻ'))

    expect(result.current.danhSach).toEqual([])
  })
})
