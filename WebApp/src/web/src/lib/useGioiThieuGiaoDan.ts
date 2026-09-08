import { useState } from 'react'
import { api, type ThongTinBenNhanGioiThieu } from '../api/client'
import type { GiaoDanListItem } from '../api/types'
import type { LoaiGioiThieuGiaoDan } from '../components/GxGiaoDanList'

const TIEU_DE: Record<LoaiGioiThieuGiaoDan, string> = {
  RuaToi: 'In giấy giới thiệu chứng nhận rửa tội',
  ThemSuc: 'In giấy giới thiệu chứng nhận thêm sức',
  GiaoLyHonPhoi: 'In giấy giới thiệu giáo lý hôn phối',
}

const GOI_API: Record<LoaiGioiThieuGiaoDan, (id: string, b: ThongTinBenNhanGioiThieu) => Promise<void>> = {
  RuaToi: api.giaoDan.inGioiThieuRuaToi,
  ThemSuc: api.giaoDan.inGioiThieuThemSuc,
  GiaoLyHonPhoi: api.giaoDan.inGioiThieuGiaoLyHonPhoi,
}

/**
 * State + hành vi dùng CHUNG cho modal "Giấy giới thiệu" — ba mẫu theo giáo dân (rửa tội/thêm
 * sức/giáo lý hôn phối; mẫu thứ tư — chuyển xứ — theo gia đình, xem `useGioiThieuChuyenXu`).
 * Mỗi màn hình nhúng `GxGiaoDanList` chỉ cần gọi hook này, truyền `mo` vào `menuGiaoDanMacDinh`,
 * rồi render `<GioiThieuModal>` cạnh lưới khi `dangMo` — tránh lặp lại state ở 3 màn hình
 * (danh sách giáo dân, hồ sơ lưu trữ, lưới thành viên trong chi tiết gia đình).
 */
export function useGioiThieuGiaoDan() {
  const [dang, setDang] = useState<{ loai: LoaiGioiThieuGiaoDan; d: GiaoDanListItem } | null>(null)

  return {
    mo: (loai: LoaiGioiThieuGiaoDan, d: GiaoDanListItem) => setDang({ loai, d }),
    dong: () => setDang(null),
    dangMo: dang !== null,
    tieuDe: dang ? TIEU_DE[dang.loai] : '',
    xuat: (b: ThongTinBenNhanGioiThieu) => (dang ? GOI_API[dang.loai](dang.d.id, b) : Promise.resolve()),
  }
}
