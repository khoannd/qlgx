import { useState } from 'react'
import { api } from '../api/client'
import type { RaoHonPhoiListItem } from '../api/types'
import { GxGrid } from '../components/GxGrid'
import { GxToolbar } from '../components/GxToolbar'
import { cotRaoHonPhoi } from '../cot/cotRaoHonPhoi'

type Props = {
  rows: RaoHonPhoiListItem[]
  xemTatCa: boolean
  onDoiXemTatCa: (v: boolean) => void
  moRao: (id: string | null) => void
  onXoa: (id: string) => Promise<void>
  onTaiLai: () => void
}

/**
 * Danh sách rao hôn phối — khớp `frmRaoHonPhoiList.cs`: combo "Chỉ xem những đôi rao chưa
 * hoàn tất" (mặc định)/"Xem tất cả" (mục 3 rao-hon-phoi.md).
 */
export function RaoHonPhoiList({ rows, xemTatCa, onDoiXemTatCa, moRao, onXoa, onTaiLai }: Props) {
  const [dongChon, setDongChon] = useState<RaoHonPhoiListItem | null>(null)
  const [trangThaiXoa, setTrangThaiXoa] = useState<'hoi' | 'dang-xoa' | null>(null)
  const [loiXoa, setLoiXoa] = useState<string | null>(null)
  const [dangXuatExcel, setDangXuatExcel] = useState(false)

  // "Xuất Excel" (xem docs/superpowers/specs/man-hinh/in-an.md mục 8) — CÙNG tham số lọc
  // `xemTatCa` đang áp dụng trên màn hình, giống cách GiaoDanList/GiaDinhList đã làm.
  async function xuatExcel() {
    setDangXuatExcel(true)
    try {
      await api.raoHonPhoi.xuatExcel(xemTatCa)
    } catch (e) {
      console.error('Không xuất được Excel danh sách rao hôn phối', e)
      window.alert(e instanceof Error ? e.message : 'Xuất Excel thất bại, thử lại sau.')
    } finally {
      setDangXuatExcel(false)
    }
  }

  async function thucHienXoa() {
    if (!dongChon) return
    setTrangThaiXoa('dang-xoa')
    setLoiXoa(null)
    try {
      await onXoa(dongChon.id)
      setDongChon(null)
      setTrangThaiXoa(null)
    } catch (e) {
      setLoiXoa(e instanceof Error ? e.message : 'Xoá thất bại, thử lại sau.')
      setTrangThaiXoa('hoi')
    }
  }

  return (
    <section className="page list-page">
      <div className="list-page-head">
        <div className="page-head">
          <h1>Danh sách rao hôn phối</h1>
          <div className="spacer" />
          <span className="count-pill"><b>{rows.length}</b> đôi rao</span>
        </div>

        <GxToolbar
          coDongDuocChon={!!dongChon}
          items={[
            { label: 'Tải lại', icon: 'reload', onClick: onTaiLai, title: 'Tải lại danh sách' },
            '|',
            { label: 'Thêm đôi rao', icon: 'plus', kind: 'primary', onClick: () => moRao(null), title: 'Thêm' },
            { label: 'Xóa', icon: 'trash', needSel: true, onClick: () => setTrangThaiXoa('hoi'),
              title: 'Xóa đôi rao được chọn' },
            '>',
            { label: dangXuatExcel ? 'Đang xuất…' : 'Xuất Excel', icon: 'excel',
              onClick: dangXuatExcel ? undefined : () => { void xuatExcel() },
              title: 'Xuất danh sách đang hiện trên lưới ra tệp Excel (.xlsx)' },
          ]}
        />

        {trangThaiXoa && dongChon && (
          <div className="card glass" role="alertdialog" style={{ marginBottom: 12 }}>
            <p>Bạn có chắc muốn xóa (các) đôi rao được chọn trong danh sách?</p>
            {loiXoa && <p className="hint" role="alert">{loiXoa}</p>}
            <div className="cmdbar">
              <button type="button" className="btn" disabled={trangThaiXoa === 'dang-xoa'}
                onClick={() => { setTrangThaiXoa(null); setLoiXoa(null) }}>
                Hủy bỏ
              </button>
              <div className="spacer" />
              <button type="button" className="btn btn-danger" disabled={trangThaiXoa === 'dang-xoa'}
                onClick={() => { void thucHienXoa() }}>
                Xóa
              </button>
            </div>
          </div>
        )}

        <div className="filters-bar glass">
          <div className="field">
            <label htmlFor="rhp-option">Hiển thị</label>
            <select id="rhp-option" value={xemTatCa ? '1' : '0'} onChange={(e) => onDoiXemTatCa(e.target.value === '1')}>
              <option value="0">Chỉ xem những đôi rao chưa hoàn tất</option>
              <option value="1">Xem tất cả</option>
            </select>
          </div>
          <div className="spacer" />
          <span className="muted" style={{ fontSize: 12.5 }}>Nhấp đúp một dòng để xem — sửa</span>
        </div>
      </div>

      <GxGrid<RaoHonPhoiListItem>
        columnDefs={cotRaoHonPhoi}
        rowData={rows}
        layId={(d) => d.id}
        onMo={(d) => moRao(d.id)}
        onChon={setDongChon}
      />
    </section>
  )
}
