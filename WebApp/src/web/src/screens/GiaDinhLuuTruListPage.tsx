import { useCallback, useEffect, useState } from 'react'
import { api } from '../api/client'
import type { GiaDinhListItem, GiaoHo } from '../api/types'
import { TrangThaiTai } from '../components/TrangThaiTai'
import { GiaDinhLuuTruList } from './GiaDinhLuuTruList'

type Props = {
  moGiaDinh: (id: string) => void
}

/** Container nối `GiaDinhLuuTruList` với `GET /api/gia-dinh/luu-tru` — cùng khuôn với
 * `GiaDinhListPage`, xoá luôn VĨNH VIỄN. */
export function GiaDinhLuuTruListPage({ moGiaDinh }: Props) {
  const [rows, setRows] = useState<GiaDinhListItem[] | null>(null)
  const [loi, setLoi] = useState<string | null>(null)
  const [dangTai, setDangTai] = useState(true)
  const [danhMucGiaoHo, setDanhMucGiaoHo] = useState<GiaoHo[]>([])

  useEffect(() => {
    api.giaoHo.danhMuc()
      .then(setDanhMucGiaoHo)
      .catch((e: unknown) => { console.error('Không tải được danh mục giáo họ', e) })
  }, [])

  const tai = useCallback(() => {
    setDangTai(true)
    setLoi(null)
    api.giaDinh.danhSachLuuTru()
      .then((ds) => setRows(ds))
      .catch((e: unknown) => {
        console.error('Không tải được hồ sơ lưu trữ gia đình', e)
        setLoi(e instanceof Error ? e.message : String(e))
      })
      .finally(() => setDangTai(false))
  }, [])

  useEffect(tai, [tai])

  async function xoa(id: string) {
    await api.giaDinh.xoa(id, true)
    tai()
  }

  return (
    <TrangThaiTai dangTai={dangTai} loi={loi} onThuLai={tai}>
      <GiaDinhLuuTruList
        rows={rows ?? []}
        moGiaDinh={moGiaDinh}
        danhMucGiaoHo={danhMucGiaoHo}
        onXoa={xoa}
        onTaiLai={tai}
      />
    </TrangThaiTai>
  )
}
