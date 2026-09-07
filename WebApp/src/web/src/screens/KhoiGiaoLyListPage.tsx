import { useCallback, useEffect, useState } from 'react'
import { api } from '../api/client'
import type { KhoiGiaoLy } from '../api/types'
import { GxGrid } from '../components/GxGrid'
import { GxToolbar } from '../components/GxToolbar'
import { TrangThaiTai } from '../components/TrangThaiTai'
import { cotKhoiGiaoLy } from '../cot/cotGiaoLy'

type Props = {
  moKhoi: (id: string | null) => void
}

/**
 * Danh mục khối giáo lý (frmKhoiGiaoLyList.cs) — cấp cao nhất của phân hệ Giáo lý: Khối → Lớp →
 * Học viên/Giáo lý viên. Xem docs/superpowers/specs/man-hinh/giao-ly.md. Combo năm của bản gốc
 * (`cbNam`) KHÔNG lọc lưới này (mã nguồn để trống, xem giao-ly.md mục 2) — không migrate bộ lọc
 * không hoạt động đó, năm chỉ có ý nghĩa ở màn hình chi tiết một khối (lọc lớp theo năm).
 */
export function KhoiGiaoLyListPage({ moKhoi }: Props) {
  const [rows, setRows] = useState<KhoiGiaoLy[] | null>(null)
  const [loi, setLoi] = useState<string | null>(null)
  const [dangTai, setDangTai] = useState(true)
  const [dongChon, setDongChon] = useState<KhoiGiaoLy | null>(null)
  const [trangThaiXoa, setTrangThaiXoa] = useState<'hoi' | 'dang-xoa' | null>(null)
  const [loiXoa, setLoiXoa] = useState<string | null>(null)

  const tai = useCallback(() => {
    setDangTai(true)
    setLoi(null)
    api.giaoLy.khoi()
      .then(setRows)
      .catch((e: unknown) => {
        console.error('Không tải được danh mục khối giáo lý', e)
        setLoi(e instanceof Error ? e.message : String(e))
      })
      .finally(() => setDangTai(false))
  }, [])

  useEffect(tai, [tai])

  async function thucHienXoa() {
    if (!dongChon) return
    setTrangThaiXoa('dang-xoa')
    setLoiXoa(null)
    try {
      await api.giaoLy.xoaKhoi(dongChon.id)
      setDongChon(null)
      setTrangThaiXoa(null)
      tai()
    } catch (e) {
      setLoiXoa(e instanceof Error ? e.message : 'Xoá thất bại, thử lại sau.')
      setTrangThaiXoa('hoi')
    }
  }

  return (
    <TrangThaiTai dangTai={dangTai} loi={loi} onThuLai={tai}>
      <section className="page list-page">
        <div className="list-page-head">
          <div className="page-head">
            <h1>Quản lý giáo lý</h1>
            <div className="spacer" />
            <span className="count-pill"><b>{rows?.length ?? 0}</b> khối giáo lý</span>
          </div>

          <GxToolbar
            coDongDuocChon={!!dongChon}
            items={[
              { label: 'Tải lại', icon: 'reload', onClick: tai, title: 'Tải lại danh sách' },
              '|',
              { label: 'Thêm khối', icon: 'plus', kind: 'primary', onClick: () => moKhoi(null), title: 'Thêm khối giáo lý' },
              { label: 'Xóa', icon: 'trash', needSel: true, onClick: () => setTrangThaiXoa('hoi'),
                title: 'Xóa khối giáo lý này' },
            ]}
          />

          {/* Nguyên văn thông báo gốc frmKhoiGiaoLyList.cs:127. */}
          {trangThaiXoa && dongChon && (
            <div className="card glass" role="alertdialog" style={{ marginBottom: 12 }}>
              <p>Bạn có chắc muốn xóa khối giáo lý "{dongChon.tenKhoi}" này? Các lớp giáo lý thuộc khối này sẽ bị xóa theo.</p>
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
            <span className="muted" style={{ fontSize: 12.5 }}>Nhấp đúp một dòng để xem — sửa khối và danh sách lớp giáo lý</span>
          </div>
        </div>

        <GxGrid<KhoiGiaoLy>
          columnDefs={cotKhoiGiaoLy}
          rowData={rows ?? []}
          layId={(d) => d.id}
          onMo={(d) => moKhoi(d.id)}
          onChon={setDongChon}
        />
      </section>
    </TrangThaiTai>
  )
}
