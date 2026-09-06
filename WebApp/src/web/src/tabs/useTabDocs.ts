import { useCallback, useState, type ReactNode } from 'react'

export type TheTaiLieu = {
  /** Khoá theo bản ghi, ví dụ "giaDinh:<uuid>". Mở lại cùng khoá thì chuyển tiêu điểm. */
  id: string
  tieuDe: string
  noiDung: ReactNode
  dongDuoc?: boolean
}

type TrangThaiThe = { danhSach: TheTaiLieu[]; dangChon: string }

/**
 * Tương đương FATabStrip cùng dictionary dicShows của frmMain: mỗi bản ghi mở ra một thẻ
 * riêng, mở lại bản ghi đang mở thì chuyển tiêu điểm thay vì tạo thẻ trùng.
 *
 * `danhSach` và `dangChon` gộp chung một state (thay vì hai `useState` riêng) để mỗi lần
 * cập nhật chỉ gọi một hàm cập nhật thuần duy nhất — hàm cập nhật của `setDanhSach` trước
 * đây gọi `setDangChon` ngay bên trong nó, vi phạm yêu cầu "không tác dụng phụ" của React
 * và bị gọi hai lần dưới `<StrictMode>`.
 */
export function useTabDocs() {
  const [trangThai, setTrangThai] = useState<TrangThaiThe>({ danhSach: [], dangChon: '' })

  const mo = useCallback((the: TheTaiLieu) => {
    setTrangThai((truoc) => ({
      danhSach: truoc.danhSach.some((t) => t.id === the.id)
        ? truoc.danhSach
        : [...truoc.danhSach, the],
      dangChon: the.id,
    }))
  }, [])

  const chon = useCallback((id: string) => {
    setTrangThai((truoc) => ({ ...truoc, dangChon: id }))
  }, [])

  const dong = useCallback((id: string) => {
    setTrangThai((truoc) => {
      const conLai = truoc.danhSach.filter((t) => t.id !== id)
      return {
        danhSach: conLai,
        dangChon: truoc.dangChon === id ? (conLai.at(-1)?.id ?? '') : truoc.dangChon,
      }
    })
  }, [])

  return { danhSach: trangThai.danhSach, dangChon: trangThai.dangChon, mo, chon, dong }
}
