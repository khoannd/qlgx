import { useCallback, useEffect, useState } from 'react'
import { api } from '../api/client'
import type { HoiDoanQuanLy } from '../api/types'
import { GxGrid } from '../components/GxGrid'
import { GxToolbar } from '../components/GxToolbar'
import { TrangThaiTai } from '../components/TrangThaiTai'
import { cotHoiDoan } from '../cot/cotHoiDoan'

type Props = {
  moHoiDoan: (id: string | null) => void
}

/**
 * Danh sách hội đoàn (frmHoiDoanList.cs) — cấp quản lý danh mục hội đoàn, khác tab "Hội đoàn"
 * của chi tiết giáo dân. Xem docs/superpowers/specs/man-hinh/hoi-doan-danh-sach.md. Tự tải khi
 * mở (khác Danh sách sổ bí tích/Rao hôn phối vốn cần chọn bộ lọc trước) — khớp
 * `frmHoiDoanList_Load` gọi `reloadGrid()`/`loaddata()` ngay khi mở, không có bộ lọc nào.
 */
export function HoiDoanListPage({ moHoiDoan }: Props) {
  const [rows, setRows] = useState<HoiDoanQuanLy[] | null>(null)
  const [loi, setLoi] = useState<string | null>(null)
  const [dangTai, setDangTai] = useState(true)
  const [dongChon, setDongChon] = useState<HoiDoanQuanLy | null>(null)
  const [trangThaiXoa, setTrangThaiXoa] = useState<'hoi' | 'dang-xoa' | null>(null)
  const [loiXoa, setLoiXoa] = useState<string | null>(null)

  const tai = useCallback(() => {
    setDangTai(true)
    setLoi(null)
    api.hoiDoanQuanLy.danhSach()
      .then(setRows)
      .catch((e: unknown) => {
        console.error('Không tải được danh sách hội đoàn', e)
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
      await api.hoiDoanQuanLy.xoa(dongChon.id)
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
            <h1>Danh sách hội đoàn</h1>
            <div className="spacer" />
            <span className="count-pill"><b>{rows?.length ?? 0}</b> hội đoàn</span>
          </div>

          <GxToolbar
            coDongDuocChon={!!dongChon}
            items={[
              { label: 'Tải lại', icon: 'reload', onClick: tai, title: 'Tải lại danh sách' },
              '|',
              { label: 'Thêm hội đoàn', icon: 'plus', kind: 'primary', onClick: () => moHoiDoan(null), title: 'Thêm' },
              { label: 'Xóa', icon: 'trash', needSel: true, onClick: () => setTrangThaiXoa('hoi'),
                title: 'Xóa hội đoàn này' },
            ]}
          />

          {/* Nguyên văn thông báo gốc frmHoiDoanList.cs:26-31 (đổi Yes/No mập mờ thành hai nút
              tường minh — cùng cách RaoHonPhoiList/DotBiTichList đã làm). */}
          {trangThaiXoa && dongChon && (
            <div className="card glass" role="alertdialog" style={{ marginBottom: 12 }}>
              <p>Bạn có thật sự muốn xóa hội đoàn "{dongChon.tenHoiDoan}" này! Toàn bộ danh sách hội viên của
                hội đoàn cũng sẽ bị xóa theo.</p>
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
            <span className="muted" style={{ fontSize: 12.5 }}>Nhấp đúp một dòng để xem — sửa hội đoàn và danh sách hội viên</span>
          </div>
        </div>

        <GxGrid<HoiDoanQuanLy>
          columnDefs={cotHoiDoan}
          rowData={rows ?? []}
          layId={(d) => d.id}
          onMo={(d) => moHoiDoan(d.id)}
          onChon={setDongChon}
        />
      </section>
    </TrangThaiTai>
  )
}
