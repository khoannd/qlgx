import { useCallback, useEffect, useState } from 'react'
import { api } from '../api/client'
import type { RaoHonPhoiListItem } from '../api/types'
import { TrangThaiTai } from '../components/TrangThaiTai'
import { RaoHonPhoiList } from './RaoHonPhoiList'

type Props = {
  moRao: (id: string | null) => void
}

/** Container nối `RaoHonPhoiList` với `GET /api/rao-hon-phoi`. */
export function RaoHonPhoiListPage({ moRao }: Props) {
  const [xemTatCa, setXemTatCa] = useState(false)
  const [rows, setRows] = useState<RaoHonPhoiListItem[] | null>(null)
  const [loi, setLoi] = useState<string | null>(null)
  const [dangTai, setDangTai] = useState(true)

  const tai = useCallback(() => {
    setDangTai(true)
    setLoi(null)
    api.raoHonPhoi.danhSach(xemTatCa)
      .then(setRows)
      .catch((e: unknown) => {
        console.error('Không tải được danh sách rao hôn phối', e)
        setLoi(e instanceof Error ? e.message : String(e))
      })
      .finally(() => setDangTai(false))
  }, [xemTatCa])

  useEffect(tai, [tai])

  async function xoa(id: string) {
    await api.raoHonPhoi.xoa(id)
    tai()
  }

  return (
    <TrangThaiTai dangTai={dangTai} loi={loi} onThuLai={tai}>
      <RaoHonPhoiList
        rows={rows ?? []}
        xemTatCa={xemTatCa}
        onDoiXemTatCa={setXemTatCa}
        moRao={moRao}
        onXoa={xoa}
        onTaiLai={tai}
      />
    </TrangThaiTai>
  )
}
