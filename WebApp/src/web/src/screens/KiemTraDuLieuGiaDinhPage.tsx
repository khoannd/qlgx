import { useEffect, useState } from 'react'
import { api } from '../api/client'
import type { GiaoHo } from '../api/types'
import { KiemTraDuLieuGiaDinh } from './KiemTraDuLieuGiaDinh'

type Props = {
  moGiaDinh: (id: string) => void
}

/** Container nối `KiemTraDuLieuGiaDinh` với danh mục Giáo họ thật — cùng khuôn với
 * `KiemTraDuLieuGiaoDanPage`. */
export function KiemTraDuLieuGiaDinhPage({ moGiaDinh }: Props) {
  const [danhMucGiaoHo, setDanhMucGiaoHo] = useState<GiaoHo[]>([])

  useEffect(() => {
    api.giaoHo.danhMuc()
      .then(setDanhMucGiaoHo)
      .catch((e: unknown) => { console.error('Không tải được danh mục giáo họ', e) })
  }, [])

  return <KiemTraDuLieuGiaDinh danhMucGiaoHo={danhMucGiaoHo} moGiaDinh={moGiaDinh} />
}
