import { useMemo } from 'react'
import type { GiaoDanListItem } from '../api/types'
import { cotGiaoDan, cotQuanHeGiaDinh } from '../cot/cotGiaoDan'
import { GxGrid, type MucMenu } from './GxGrid'

type Props = {
  rows: GiaoDanListItem[]
  /** Nhúng trong form gia đình: thêm cột Quan hệ GĐ, bỏ cột Điện thoại. */
  quanHeGiaDinh?: boolean
  onMo?: (dong: GiaoDanListItem) => void
  onChon?: (dong: GiaoDanListItem | null) => void
  menuChuotPhai?: MucMenu<GiaoDanListItem>[]
  hangLoc?: boolean
}

/** Đúng 12 mục và đúng thứ tự trong constructor của GxGiaoDanList bản desktop. "Xem gia đình"
 * tự ẩn khi dòng chưa gắn với gia đình nào (`giaDinhId` null) — tránh gọi `xemGiaDinh` với
 * giá trị rỗng, vốn từng khiến mục này mở nhầm một thẻ "Gia đình mới" trống. */
export const menuGiaoDanMacDinh = (
  moChiTiet: (d: GiaoDanListItem) => void,
  xemGiaDinh: (d: GiaoDanListItem) => void,
): MucMenu<GiaoDanListItem>[] => [
  { nhan: 'Xem chi tiết', chay: moChiTiet },
  { nhan: 'In lý lịch cá nhân' },
  { nhan: 'In chứng nhận bí tích' },
  { nhan: 'In giới thiệu hôn phối' },
  { nhan: 'In chứng nhận rửa tội' },
  { nhan: 'In chứng nhận xưng tội - rước lễ' },
  { nhan: 'In chứng nhận thêm sức' },
  { nhan: 'Xem gia đình', chay: xemGiaDinh, an: (d) => !d.giaDinhId },
  { nhan: 'In giấy giới thiệu chứng nhận rửa tội' },
  { nhan: 'In giấy giới thiệu giáo lý hôn phối' },
  { nhan: 'In giấy giới thiệu chứng nhận thêm sức' },
  { nhan: 'Xem vị trí' },
]

/**
 * Tương đương UserControl GxGiaoDanList. Tự sở hữu bộ cột, quy tắc tô đỏ và ghi chú chân
 * lưới, nên nơi nhúng chỉ truyền dữ liệu. Nhờ vậy lưới ở màn hình danh sách và lưới thành
 * viên trong form gia đình là cùng một component — kiểm thử một lần dùng được cả hai.
 */
export function GxGiaoDanList({ rows, quanHeGiaDinh, ...phanConLai }: Props) {
  const columnDefs = useMemo(
    () =>
      quanHeGiaDinh
        ? [cotQuanHeGiaDinh, ...cotGiaoDan.filter((c) => c.field !== 'dienThoai')]
        : cotGiaoDan,
    [quanHeGiaDinh],
  )

  return (
    <GxGrid<GiaoDanListItem>
      columnDefs={columnDefs}
      rowData={rows}
      layId={(d) => d.id}
      toDo={(d) => d.quaDoi || d.daChuyenDi || d.lapGd}
      ghiChuChan="Gạch ngang đỏ: đã qua đời, chuyển xứ hoặc lập gia đình riêng"
      {...phanConLai}
    />
  )
}
