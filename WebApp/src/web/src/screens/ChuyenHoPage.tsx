import { useEffect, useState } from 'react'
import { api } from '../api/client'
import type { GiaoHo } from '../api/types'
import { ChuyenHoGiaoDan } from './ChuyenHoGiaoDan'
import { ChuyenHoGiaDinh } from './ChuyenHoGiaDinh'

/**
 * Container "Chuyển họ hàng loạt" — desktop có HAI mục menu riêng ("Chuyển họ cho giáo dân"
 * `itChuyenHoGiaoDan` và "Chuyển họ cho gia đình" `itChuyenHoGiaDinh`, xem frmMain.cs mục
 * LoadFunction). Bản web gộp vào MỘT thẻ tài liệu với hai tab để đỡ tốn một mục điều hướng —
 * xem docs/superpowers/specs/man-hinh/can-review-sau.md cho quyết định này.
 */
export function ChuyenHoPage() {
  const [danhMucGiaoHo, setDanhMucGiaoHo] = useState<GiaoHo[]>([])
  const [tab, setTab] = useState<'giaoDan' | 'giaDinh'>('giaoDan')

  useEffect(() => {
    api.giaoHo.danhMuc()
      .then(setDanhMucGiaoHo)
      .catch((e: unknown) => { console.error('Không tải được danh mục giáo họ', e) })
  }, [])

  return (
    <div>
      <div className="filters-bar glass" style={{ gap: 8, marginBottom: 12 }}>
        <button type="button" className={tab === 'giaoDan' ? 'btn btn-primary' : 'btn'} onClick={() => setTab('giaoDan')}>
          Giáo dân
        </button>
        <button type="button" className={tab === 'giaDinh' ? 'btn btn-primary' : 'btn'} onClick={() => setTab('giaDinh')}>
          Gia đình
        </button>
      </div>
      {tab === 'giaoDan'
        ? <ChuyenHoGiaoDan danhMucGiaoHo={danhMucGiaoHo} />
        : <ChuyenHoGiaDinh danhMucGiaoHo={danhMucGiaoHo} />}
    </div>
  )
}
