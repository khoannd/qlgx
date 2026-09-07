import { forwardRef } from 'react'
import type { Ref } from 'react'
import { api } from '../api/client'
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

/** "In chứng nhận hôn phối" / "In phiếu gia đình" từ menu chuột phải — xem
 * docs/superpowers/specs/man-hinh/in-an.md. Lỗi báo bằng alert(), nhất quán với cách
 * GxGiaoDanList báo lỗi in. */
function baoLoiIn(hanhDong: string, giaDinhId: string) {
  return (e: unknown) => {
    console.error(`Không ${hanhDong} của gia đình ${giaDinhId}`, e)
    window.alert(e instanceof Error ? e.message : 'In thất bại, thử lại sau.')
  }
}

function inChungNhanHonPhoi(d: GiaDinhListItem): void {
  api.giaDinh.inChungNhanHonPhoi(d.id).catch(baoLoiIn('in được chứng nhận hôn phối', d.id))
}

function inPhieuGiaDinh(d: GiaDinhListItem): void {
  api.giaDinh.inPhieuGiaDinh(d.id).catch(baoLoiIn('in được phiếu gia đình', d.id))
}

/** Đúng 5 mục và đúng thứ tự trong constructor của GxGiaDinhList bản desktop. "In lý lịch cá
 * nhân" ở lưới GIA ĐÌNH vẫn báo "chưa hỗ trợ" — không rõ in cho thành viên nào (dùng menu
 * chuột phải trên lưới THÀNH VIÊN — GxGiaoDanList.inLyLichCaNhan — thay vì mục này); "In giới
 * thiệu chuyển xứ" và "Xem vị trí" cũng chưa làm ở lượt này. */
export const menuGiaDinhMacDinh = (
  _moChiTiet: (d: GiaDinhListItem) => void,
): MucMenu<GiaDinhListItem>[] => [
  { nhan: 'In chứng nhận hôn phối', chay: inChungNhanHonPhoi },
  { nhan: 'In phiếu gia đình', chay: inPhieuGiaDinh },
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
