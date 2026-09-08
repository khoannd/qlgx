import { useState } from 'react'
import { api, type ThongTinBenNhanGioiThieu } from '../api/client'
import type { GiaDinhListItem } from '../api/types'

/** State + hành vi cho modal "In giới thiệu chuyển xứ" (mẫu thứ tư của "Giấy giới thiệu" —
 * theo GIA ĐÌNH, xem `useGioiThieuGiaoDan` cho ba mẫu còn lại theo giáo dân). Dùng ở màn hình
 * "Danh sách gia đình" và "Hồ sơ lưu trữ gia đình". */
export function useGioiThieuChuyenXu() {
  const [dang, setDang] = useState<GiaDinhListItem | null>(null)

  return {
    mo: (d: GiaDinhListItem) => setDang(d),
    dong: () => setDang(null),
    dangMo: dang !== null,
    tieuDe: 'In giới thiệu chuyển xứ',
    xuat: (b: ThongTinBenNhanGioiThieu) => (dang ? api.giaDinh.inGioiThieuChuyenXu(dang.id, b) : Promise.resolve()),
  }
}
