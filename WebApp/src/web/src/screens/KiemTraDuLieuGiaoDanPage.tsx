import { useEffect, useState } from 'react'
import { api } from '../api/client'
import type { GiaoHo } from '../api/types'
import { KiemTraDuLieuGiaoDan } from './KiemTraDuLieuGiaoDan'

type Props = {
  moGiaoDan: (id: string) => void
}

/** Container nối `KiemTraDuLieuGiaoDan` với danh mục Giáo họ thật — cùng khuôn với
 * `GiaoDanListPage`. Không tự tải kết quả kiểm tra lúc mở (đúng desktop: lưới trống cho tới
 * khi bấm "Bắt đầu kiểm tra"). */
export function KiemTraDuLieuGiaoDanPage({ moGiaoDan }: Props) {
  const [danhMucGiaoHo, setDanhMucGiaoHo] = useState<GiaoHo[]>([])

  useEffect(() => {
    api.giaoHo.danhMuc()
      .then(setDanhMucGiaoHo)
      .catch((e: unknown) => { console.error('Không tải được danh mục giáo họ', e) })
  }, [])

  return <KiemTraDuLieuGiaoDan danhMucGiaoHo={danhMucGiaoHo} moGiaoDan={moGiaoDan} />
}
