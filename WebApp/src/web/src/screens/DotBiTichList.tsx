import { useMemo, useState } from 'react'
import { api } from '../api/client'
import type { DotBiTichListItem, LoaiBiTich } from '../api/types'
import { GxGrid } from '../components/GxGrid'
import { GxToolbar } from '../components/GxToolbar'
import { cotDotBiTich } from '../cot/cotDotBiTich'

const TEN_LOAI: Record<LoaiBiTich, string> = { 0: 'Rửa tội', 1: 'Rước lễ (XTRL lần đầu)', 2: 'Thêm sức' }

type Props = {
  loaiBiTich: LoaiBiTich | null
  onDoiLoai: (loai: LoaiBiTich) => void
  tuNam: string
  denNam: string
  onDoiTuNam: (v: string) => void
  onDoiDenNam: (v: string) => void
  onTimKiem: () => void
  rows: DotBiTichListItem[] | null
  onMoDot: (id: string | null) => void
  onXoa: (id: string) => Promise<void>
  onTaiLai: () => void
}

/**
 * Danh sách sổ bí tích — khớp `frmDotBiTichList.cs`: chọn Loại bí tích rồi bấm "Tìm kiếm" mới
 * tải lưới (mục 4 so-bi-tich.md, "Xin vui lòng chọn một loại bí tích cần xem" khi chưa chọn).
 * Hôn phối (LoaiBiTich=3) có màn hình riêng (hon-phoi.md), KHÔNG có trong combo này.
 */
export function DotBiTichList({
  loaiBiTich, onDoiLoai, tuNam, denNam, onDoiTuNam, onDoiDenNam, onTimKiem,
  rows, onMoDot, onXoa, onTaiLai,
}: Props) {
  const [dongChon, setDongChon] = useState<DotBiTichListItem | null>(null)
  const [trangThaiXoa, setTrangThaiXoa] = useState<'hoi' | 'dang-xoa' | null>(null)
  const [loiXoa, setLoiXoa] = useState<string | null>(null)
  const [dangXuatExcel, setDangXuatExcel] = useState(false)

  // "Xuất Excel" (xem docs/superpowers/specs/man-hinh/in-an.md mục 8) — CÙNG bộ lọc đang áp
  // dụng trên màn hình (loaiBiTich/tuNam/denNam), giống cách GiaoDanList/GiaDinhList đã làm.
  // Chỉ bấm được khi đã "Tìm kiếm" (có loaiBiTich) — trước đó không có gì để xuất.
  async function xuatExcel() {
    if (loaiBiTich === null) return
    setDangXuatExcel(true)
    try {
      await api.dotBiTich.xuatExcel(loaiBiTich, Number(tuNam) || undefined, Number(denNam) || undefined)
    } catch (e) {
      console.error('Không xuất được Excel danh sách sổ bí tích', e)
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

  const daTimKiem = rows !== null
  const tongSoNguoi = useMemo(() => (rows ?? []).reduce((t, r) => t + r.soLuong, 0), [rows])

  return (
    <section className="page list-page">
      <div className="list-page-head">
        <div className="page-head">
          <h1>Danh sách sổ bí tích</h1>
          <div className="spacer" />
          {daTimKiem && (
            <span className="count-pill">
              <b>{rows!.length}</b> đợt · <b>{tongSoNguoi}</b> người
            </span>
          )}
        </div>

        <GxToolbar
          coDongDuocChon={!!dongChon}
          items={[
            { label: 'Tải lại', icon: 'reload', onClick: onTaiLai, title: 'Tải lại danh sách theo bộ lọc hiện tại' },
            '|',
            { label: 'Thêm đợt', icon: 'plus', kind: 'primary', onClick: () => onMoDot(null),
              title: 'Thêm đợt bí tích mới' },
            { label: 'Xóa đợt', icon: 'trash', needSel: true, onClick: () => setTrangThaiXoa('hoi'),
              title: 'Loại bỏ đợt bí tích này ra khỏi danh sách' },
            '>',
            { label: dangXuatExcel ? 'Đang xuất…' : 'Xuất Excel', icon: 'excel',
              onClick: !daTimKiem || dangXuatExcel ? undefined : () => { void xuatExcel() },
              title: 'Xuất danh sách đang hiện trên lưới ra tệp Excel (.xlsx)' },
          ]}
        />

        {trangThaiXoa && dongChon && (
          <div className="card glass" role="alertdialog" style={{ marginBottom: 12 }}>
            <p>Bạn có chắc muốn loại bỏ đợt bí tích <b>{dongChon.moTa}</b> ra khỏi danh sách?</p>
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
            <label htmlFor="dbt-loai">Loại bí tích</label>
            <select id="dbt-loai" value={loaiBiTich ?? ''}
              onChange={(e) => onDoiLoai(Number(e.target.value) as LoaiBiTich)}>
              <option value="" disabled>— Chọn loại —</option>
              {(Object.entries(TEN_LOAI) as [string, string][]).map(([k, ten]) => (
                <option key={k} value={k}>{ten}</option>
              ))}
            </select>
          </div>
          <div className="field">
            <label htmlFor="dbt-tunam">Từ năm</label>
            <input id="dbt-tunam" type="number" value={tuNam} onChange={(e) => onDoiTuNam(e.target.value)} style={{ width: 90 }} />
          </div>
          <div className="field">
            <label htmlFor="dbt-dennam">Đến năm</label>
            <input id="dbt-dennam" type="number" value={denNam} onChange={(e) => onDoiDenNam(e.target.value)} style={{ width: 90 }} />
          </div>
          <button type="button" className="btn btn-primary" onClick={onTimKiem}>Tìm kiếm</button>
          <div className="spacer" />
          <span className="muted" style={{ fontSize: 12.5 }}>Nhấp đúp một dòng để xem danh sách người nhận</span>
        </div>
      </div>

      {daTimKiem ? (
        <GxGrid<DotBiTichListItem>
          columnDefs={cotDotBiTich}
          rowData={rows!}
          layId={(d) => d.id}
          onMo={(d) => onMoDot(d.id)}
          onChon={setDongChon}
        />
      ) : (
        <p className="muted" style={{ padding: 16 }}>Xin vui lòng chọn một loại bí tích cần xem rồi bấm "Tìm kiếm".</p>
      )}
    </section>
  )
}
