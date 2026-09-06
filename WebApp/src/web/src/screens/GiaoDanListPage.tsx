import { useCallback, useEffect, useState } from 'react'
import { api } from '../api/client'
import type { GiaoDanListItem } from '../api/types'
import { TrangThaiTai } from '../components/TrangThaiTai'
import { GiaoDanList } from './GiaoDanList'

type Props = {
  moGiaoDan: (id: string | null) => void
  moGiaDinh?: (id: string) => void
}

/** Container nối `GiaoDanList` với `GET /api/giao-dan` — cùng khuôn với `GiaDinhListPage`. */
export function GiaoDanListPage({ moGiaoDan, moGiaDinh }: Props) {
  const [rows, setRows] = useState<GiaoDanListItem[] | null>(null)
  const [loi, setLoi] = useState<string | null>(null)
  const [dangTai, setDangTai] = useState(true)

  const tai = useCallback(() => {
    setDangTai(true)
    setLoi(null)
    api.giaoDan.danhSach()
      .then((ds) => setRows(ds))
      .catch((e: unknown) => {
        console.error('Không tải được danh sách giáo dân', e)
        setLoi(e instanceof Error ? e.message : String(e))
      })
      .finally(() => setDangTai(false))
  }, [])

  useEffect(tai, [tai])

  return (
    <TrangThaiTai dangTai={dangTai} loi={loi} onThuLai={tai}>
      <GiaoDanList rows={rows ?? []} moGiaoDan={moGiaoDan} moGiaDinh={moGiaDinh} />
    </TrangThaiTai>
  )
}
