import { useMemo, useState } from 'react'
import type { GiaoDanListItem } from '../api/types'
import { GxGiaoDanList, menuGiaoDanMacDinh } from '../components/GxGiaoDanList'
import { DANH_SACH_GIAO_HO_TAM, NGOAI_XU } from '../data/giaoHoTam'

type Props = {
  rows: GiaoDanListItem[]
  /** Mở thẻ chi tiết giáo dân — truyền `null` để mở bản ghi mới, đúng nút "Thêm giáo dân". */
  moGiaoDan: (id: string | null) => void
  /** Mục "Xem gia đình" trên menu chuột phải của lưới mở thẳng thẻ chi tiết gia đình. */
  moGiaDinh?: (id: string) => void
}

/**
 * Danh sách giáo dân — cùng bố cục `page-head`/`filters-bar` với `GiaDinhList`, chỉ khác ô
 * tick là "Chỉ xem giáo dân không được thống kê" và lưới là `GxGiaoDanList`. Lọc theo Giáo họ
 * thực hiện trên máy khách vì backend chưa có danh mục Giáo họ (xem `data/giaoHoTam.ts`). Ô
 * tick lọc theo `khongThongKe` — KHÔNG suy ra từ `tenGiaoHo === "Ngoài xứ"`: hai khái niệm
 * này khác nhau (một giáo dân ngoài xứ vẫn có thể được thống kê, và ngược lại).
 */
export function GiaoDanList({ rows, moGiaoDan, moGiaDinh }: Props) {
  const [giaoHo, setGiaoHo] = useState('-1')
  const [chiKhongThongKe, setChiKhongThongKe] = useState(false)

  const dsGiaoHo = useMemo(() => {
    const tuDuLieu = rows.map((r) => r.tenGiaoHo).filter((t): t is string => !!t && t !== NGOAI_XU)
    return Array.from(new Set([...tuDuLieu, ...DANH_SACH_GIAO_HO_TAM])).sort((a, b) => a.localeCompare(b, 'vi'))
  }, [rows])

  const rowsLoc = useMemo(
    () => rows.filter((r) => {
      if (chiKhongThongKe && !r.khongThongKe) return false
      if (giaoHo === '0') return r.tenGiaoHo === NGOAI_XU
      if (giaoHo !== '-1') return r.tenGiaoHo === giaoHo
      return true
    }),
    [rows, giaoHo, chiKhongThongKe],
  )

  // "Xem gia đình" phải tra theo `d.giaDinhId` (mã GIA ĐÌNH) — KHÔNG phải `d.id` (mã giáo
  // dân); dùng nhầm `d.id` từng khiến mục này mở ra một thẻ "Gia đình mới" trống vì không
  // tra được gia đình nào khớp. Mục menu tự ẩn khi `giaDinhId` null (xem `menuGiaoDanMacDinh`).
  const menu = useMemo(
    () => menuGiaoDanMacDinh((d) => moGiaoDan(d.id), (d) => { if (d.giaDinhId) moGiaDinh?.(d.giaDinhId) }),
    [moGiaoDan, moGiaDinh],
  )

  return (
    <section className="page list-page">
      <div className="page-head">
        <h1>Danh sách giáo dân</h1>
        <div className="spacer" />
        <span className="count-pill"><b>{rowsLoc.length}</b> giáo dân</span>
        <button type="button" className="btn btn-primary" onClick={() => moGiaoDan(null)}>
          Thêm giáo dân
        </button>
      </div>

      <div className="filters-bar glass">
        <div className="field">
          <label htmlFor="gdanl-giaoho">Giáo họ</label>
          <select id="gdanl-giaoho" value={giaoHo} onChange={(e) => setGiaoHo(e.target.value)}>
            <option value="-1">Tất cả</option>
            <option value="0">{NGOAI_XU}</option>
            {dsGiaoHo.map((g) => <option key={g} value={g}>{g}</option>)}
          </select>
        </div>
        <label className="toggle">
          <input
            type="checkbox"
            checked={chiKhongThongKe}
            onChange={(e) => setChiKhongThongKe(e.target.checked)}
          />
          Chỉ xem giáo dân không được thống kê
        </label>
        <div className="spacer" />
        <span className="muted" style={{ fontSize: 12.5 }}>
          Nhấp đúp một dòng để mở chi tiết · chuột phải để xem thao tác in ấn
        </span>
      </div>

      <GxGiaoDanList rows={rowsLoc} onMo={(d) => moGiaoDan(d.id)} menuChuotPhai={menu} />
    </section>
  )
}
