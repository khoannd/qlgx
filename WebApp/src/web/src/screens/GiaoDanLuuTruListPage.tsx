import { useCallback, useEffect, useState } from 'react'
import { api } from '../api/client'
import type { GiaoDanListItem, GiaoHo } from '../api/types'
import { TrangThaiTai } from '../components/TrangThaiTai'
import { GiaoDanLuuTruList } from './GiaoDanLuuTruList'

type Props = {
  moGiaoDan: (id: string) => void
  moGiaDinh?: (id: string) => void
}

/** Container nối `GiaoDanLuuTruList` với `GET /api/giao-dan/luu-tru` — cùng khuôn với
 * `GiaoDanListPage`, xoá luôn VĨNH VIỄN (không có lựa chọn xoá mềm nào khác ở màn hình này). */
export function GiaoDanLuuTruListPage({ moGiaoDan, moGiaDinh }: Props) {
  const [rows, setRows] = useState<GiaoDanListItem[] | null>(null)
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
    api.giaoDan.danhSachLuuTru()
      .then((ds) => setRows(ds))
      .catch((e: unknown) => {
        console.error('Không tải được hồ sơ lưu trữ giáo dân', e)
        setLoi(e instanceof Error ? e.message : String(e))
      })
      .finally(() => setDangTai(false))
  }, [])

  useEffect(tai, [tai])

  async function xoa(id: string) {
    await api.giaoDan.xoa(id, true)
    tai()
  }

  return (
    <TrangThaiTai dangTai={dangTai} loi={loi} onThuLai={tai}>
      <GiaoDanLuuTruList
        rows={rows ?? []}
        moGiaoDan={moGiaoDan}
        moGiaDinh={moGiaDinh}
        onXoa={xoa}
        danhMucGiaoHo={danhMucGiaoHo}
        onTaiLai={tai}
      />
    </TrangThaiTai>
  )
}
