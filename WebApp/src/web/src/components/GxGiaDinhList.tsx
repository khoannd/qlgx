import { forwardRef } from 'react'
import type { Ref } from 'react'
import type { GiaDinhListItem } from '../api/types'
import { cotGiaDinh } from '../cot/cotGiaDinh'
import { chuaHoTro } from '../lib/thongBao'
import { GxGrid, type GxGridHandle, type MucMenu } from './GxGrid'

type Props = {
  rows: GiaDinhListItem[]
  onMo?: (dong: GiaDinhListItem) => void
  onChon?: (dong: GiaDinhListItem | null) => void
  menuChuotPhai?: MucMenu<GiaDinhListItem>[]
  hangLoc?: boolean
}

/** Đúng 5 mục và đúng thứ tự trong constructor của GxGiaDinhList bản desktop. In ấn thuộc
 * giai đoạn 3 (chưa làm) — "In phiếu gia đình" TỪNG bị nối nhầm vào `moChiTiet` (mở màn hình
 * chi tiết thay vì in gì cả, xem gia-dinh-danh-sach.md mục 10 "Ưu tiên cao #2"); đã gỡ nối sai
 * đó, giờ hiện đúng thông báo "chưa hỗ trợ" như bốn mục in/xem-vị-trí còn lại. */
export const menuGiaDinhMacDinh = (
  _moChiTiet: (d: GiaDinhListItem) => void,
): MucMenu<GiaDinhListItem>[] => [
  { nhan: 'In chứng nhận hôn phối', chay: chuaHoTro },
  { nhan: 'In phiếu gia đình', chay: chuaHoTro },
  { nhan: 'In lý lịch cá nhân', chay: chuaHoTro },
  { nhan: 'In giới thiệu chuyển xứ', chay: chuaHoTro },
  { nhan: 'Xem vị trí', chay: chuaHoTro },
]

/**
 * Tương đương UserControl GxGiaDinhList. Tự sở hữu bộ cột, ghi chú chân lưới và menu chuột
 * phải, giống hệt cách GxGiaoDanList tách khỏi màn hình — nơi nhúng chỉ truyền dữ liệu.
 *
 * Khác lưới giáo dân: gia đình không tô cả dòng khi có người đã qua đời hay chuyển xứ, mà
 * chỉ gạch từng ô Người nam / Người nữ theo cột `gach` — quy tắc đó nằm trong `cellClass`
 * của `cotGiaDinh.ts`, nên `toDo` ở đây luôn trả về false.
 *
 * Chuyển tiếp `ref` xuống `GxGrid` để nơi nhúng (thanh công cụ "Xuất dữ liệu") gọi được
 * `layCsv()`.
 */
function GxGiaDinhListTrong({ rows, ...phanConLai }: Props, ref: Ref<GxGridHandle>) {
  return (
    <GxGrid<GiaDinhListItem>
      ref={ref}
      columnDefs={cotGiaDinh}
      rowData={rows}
      layId={(d) => d.id}
      toDo={() => false}
      ghiChuChan="Gạch ngang đỏ: người nam / người nữ đã qua đời hoặc chuyển xứ"
      {...phanConLai}
    />
  )
}

export const GxGiaDinhList = forwardRef(GxGiaDinhListTrong)
