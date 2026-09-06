import { useCallback, useEffect, useState } from 'react'
import { api } from '../api/client'
import type { GiaDinhListItem } from '../api/types'
import { TrangThaiTai } from '../components/TrangThaiTai'
import { GiaDinhList } from './GiaDinhList'

type Props = {
  moGiaDinh: (id: string | null) => void
}

/**
 * Container nối `GiaDinhList` (thuần hiển thị, xem test của nó) với API thật
 * `GET /api/gia-dinh`. Giữ nguyên giao diện của `GiaDinhList` — chỉ thêm vòng đời tải dữ
 * liệu: đang tải / lỗi (không nuốt lỗi thành mảng rỗng) / rỗng / có dữ liệu.
 */
export function GiaDinhListPage({ moGiaDinh }: Props) {
  const [rows, setRows] = useState<GiaDinhListItem[] | null>(null)
  const [loi, setLoi] = useState<string | null>(null)
  const [dangTai, setDangTai] = useState(true)

  const tai = useCallback(() => {
    setDangTai(true)
    setLoi(null)
    api.giaDinh.danhSach()
      .then((ds) => setRows(ds))
      .catch((e: unknown) => {
        console.error('Không tải được danh sách gia đình', e)
        setLoi(e instanceof Error ? e.message : String(e))
      })
      .finally(() => setDangTai(false))
  }, [])

  useEffect(tai, [tai])

  return (
    <TrangThaiTai dangTai={dangTai} loi={loi} onThuLai={tai}>
      <GiaDinhList rows={rows ?? []} moGiaDinh={moGiaDinh} />
    </TrangThaiTai>
  )
}
