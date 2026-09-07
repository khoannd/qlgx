import { useCallback, useState } from 'react'
import { api } from '../api/client'
import type { DotBiTichListItem, LoaiBiTich } from '../api/types'
import { TrangThaiTai } from '../components/TrangThaiTai'
import { DotBiTichList } from './DotBiTichList'

type Props = {
  moDot: (id: string | null, loaiBiTich: LoaiBiTich) => void
}

/**
 * Container nối `DotBiTichList` (thuần hiển thị) với `GET /api/dot-bi-tich`. Chỉ tìm kiếm khi
 * người dùng đã chọn Loại bí tích và bấm "Tìm kiếm" — khớp hành vi `frmDotBiTichList.cs`
 * (không tự tải lưới lúc mở màn hình).
 */
export function DotBiTichListPage({ moDot }: Props) {
  const [loaiBiTich, setLoaiBiTich] = useState<LoaiBiTich | null>(null)
  const [tuNam, setTuNam] = useState('')
  // Mặc định "Đến năm" = năm hiện tại — khớp frmDotBiTichList_Load (frmDotBiTichList.cs:77-81).
  const [denNam, setDenNam] = useState(() => String(new Date().getFullYear()))
  const [rows, setRows] = useState<DotBiTichListItem[] | null>(null)
  const [loi, setLoi] = useState<string | null>(null)
  const [dangTai, setDangTai] = useState(false)

  const timKiem = useCallback(() => {
    if (loaiBiTich === null) {
      window.alert('Xin vui lòng chọn một loại bí tích cần xem')
      return
    }
    setDangTai(true)
    setLoi(null)
    api.dotBiTich.danhSach(loaiBiTich, tuNam ? Number(tuNam) : undefined, denNam ? Number(denNam) : undefined)
      .then(setRows)
      .catch((e: unknown) => {
        console.error('Không tải được danh sách sổ bí tích', e)
        setLoi(e instanceof Error ? e.message : String(e))
      })
      .finally(() => setDangTai(false))
  }, [loaiBiTich, tuNam, denNam])

  async function xoa(id: string) {
    await api.dotBiTich.xoa(id)
    timKiem()
  }

  return (
    <TrangThaiTai dangTai={dangTai} loi={loi} onThuLai={timKiem}>
      <DotBiTichList
        loaiBiTich={loaiBiTich}
        onDoiLoai={setLoaiBiTich}
        tuNam={tuNam}
        denNam={denNam}
        onDoiTuNam={setTuNam}
        onDoiDenNam={setDenNam}
        onTimKiem={timKiem}
        rows={rows}
        onMoDot={(id) => moDot(id, loaiBiTich ?? 0)}
        onXoa={xoa}
        onTaiLai={timKiem}
      />
    </TrangThaiTai>
  )
}
