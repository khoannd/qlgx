import { useCallback, useEffect, useState } from 'react'
import { api } from '../api/client'
import type { GiaoDanListItem, GiaoHo } from '../api/types'
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
  // Mặc định false — GiaoDanService.LayDanhSach áp nền lọc "AND DaXoa=0 AND DaChuyenXu=0 AND
  // QuaDoi=0" đúng như GxGiaoHo.LoadGridData của desktop (xem giao-dan-danh-sach.md mục 10).
  // Bản desktop không có công tắc bật/tắt lọc này ngay trong frmGiaoDanList (chỉ xem được qua
  // các màn hình Tìm kiếm riêng, ngoài phạm vi migrate) — cờ này là khả năng MỚI để không mất
  // hẳn cách xem người đã mất/chuyển xứ trên web.
  const [hienCaDaMat, setHienCaDaMat] = useState(false)
  const [danhMucGiaoHo, setDanhMucGiaoHo] = useState<GiaoHo[]>([])

  useEffect(() => {
    api.giaoHo.danhMuc()
      .then(setDanhMucGiaoHo)
      .catch((e: unknown) => { console.error('Không tải được danh mục giáo họ', e) })
  }, [])

  const tai = useCallback(() => {
    setDangTai(true)
    setLoi(null)
    api.giaoDan.danhSach(undefined, undefined, hienCaDaMat)
      .then((ds) => setRows(ds))
      .catch((e: unknown) => {
        console.error('Không tải được danh sách giáo dân', e)
        setLoi(e instanceof Error ? e.message : String(e))
      })
      .finally(() => setDangTai(false))
  }, [hienCaDaMat])

  useEffect(tai, [tai])

  async function xoa(id: string, vinhVien: boolean) {
    await api.giaoDan.xoa(id, vinhVien)
    tai()
  }

  return (
    <TrangThaiTai dangTai={dangTai} loi={loi} onThuLai={tai}>
      <GiaoDanList
        rows={rows ?? []}
        moGiaoDan={moGiaoDan}
        moGiaDinh={moGiaDinh}
        hienCaDaMat={hienCaDaMat}
        onDoiHienCaDaMat={setHienCaDaMat}
        onXoa={xoa}
        danhMucGiaoHo={danhMucGiaoHo}
      />
    </TrangThaiTai>
  )
}
