import { useCallback, useState, type ReactNode } from 'react'

export type TheTaiLieu = {
  /** Khoá theo bản ghi, ví dụ "giaDinh:<uuid>". Mở lại cùng khoá thì chuyển tiêu điểm. */
  id: string
  tieuDe: string
  noiDung: ReactNode
  dongDuoc?: boolean
}

/**
 * Tương đương FATabStrip cùng dictionary dicShows của frmMain: mỗi bản ghi mở ra một thẻ
 * riêng, mở lại bản ghi đang mở thì chuyển tiêu điểm thay vì tạo thẻ trùng.
 */
export function useTabDocs() {
  const [danhSach, setDanhSach] = useState<TheTaiLieu[]>([])
  const [dangChon, setDangChon] = useState('')

  const mo = useCallback((the: TheTaiLieu) => {
    setDanhSach((truoc) =>
      truoc.some((t) => t.id === the.id) ? truoc : [...truoc, the],
    )
    setDangChon(the.id)
  }, [])

  const chon = useCallback((id: string) => setDangChon(id), [])

  const dong = useCallback((id: string) => {
    setDanhSach((truoc) => {
      const conLai = truoc.filter((t) => t.id !== id)
      setDangChon((hienTai) =>
        hienTai === id ? (conLai.at(-1)?.id ?? '') : hienTai,
      )
      return conLai
    })
  }, [])

  return { danhSach, dangChon, mo, chon, dong }
}
