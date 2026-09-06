import { useMemo, useState } from 'react'
import type { GiaDinhListItem } from '../api/types'
import { GxGiaDinhList, menuGiaDinhMacDinh } from '../components/GxGiaDinhList'
import { DANH_SACH_GIAO_HO_TAM, NGOAI_XU } from '../data/giaoHoTam'

type Props = {
  rows: GiaDinhListItem[]
  /** Mở thẻ chi tiết gia đình — truyền `null` để mở bản ghi mới, đúng nút "Thêm gia đình". */
  moGiaDinh: (id: string | null) => void
}

/**
 * Danh sách gia đình — `page-head` với tiêu đề và nút "Thêm gia đình", `filters-bar` có
 * combobox Giáo họ (kèm "Tất cả"/"Ngoài xứ") và ô tick "Chỉ xem gia đình không được thống
 * kê", `GxGiaDinhList` bên dưới. Lọc theo Giáo họ/ô tick thực hiện trên máy khách vì backend
 * chưa có danh mục Giáo họ để truyền `giaoHoId` thật cho endpoint (xem `data/giaoHoTam.ts`).
 */
export function GiaDinhList({ rows, moGiaDinh }: Props) {
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

  const menu = useMemo(() => menuGiaDinhMacDinh((d) => moGiaDinh(d.id)), [moGiaDinh])

  return (
    <section className="page list-page">
      <div className="page-head">
        <h1>Danh sách gia đình</h1>
        <div className="spacer" />
        <span className="count-pill"><b>{rowsLoc.length}</b> gia đình</span>
        <button type="button" className="btn btn-primary" onClick={() => moGiaDinh(null)}>
          Thêm gia đình
        </button>
      </div>

      <div className="filters-bar glass">
        <div className="field">
          <label htmlFor="gdl-giaoho">Giáo họ</label>
          <select id="gdl-giaoho" value={giaoHo} onChange={(e) => setGiaoHo(e.target.value)}>
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
          Chỉ xem gia đình không được thống kê
        </label>
        <div className="spacer" />
        <span className="muted" style={{ fontSize: 12.5 }}>
          Nhấp đúp một dòng để mở chi tiết · chuột phải để xem thao tác in ấn
        </span>
      </div>

      <GxGiaDinhList rows={rowsLoc} onMo={(d) => moGiaDinh(d.id)} menuChuotPhai={menu} />
    </section>
  )
}
