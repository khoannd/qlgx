import { forwardRef } from 'react'
import type { Ref } from 'react'
import { api } from '../api/client'
import type { GiaDinhListItem } from '../api/types'
import { cotGiaDinh } from '../cot/cotGiaDinh'
import { moBanDo } from '../lib/xemViTri'
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

/** "In lý lịch cá nhân" bấm từ lưới GIA ĐÌNH — in CẢ gia đình (một trang PDF/thành viên, đúng
 * hành vi `item4_Click` của bản desktop — xem in-an.md mục 5f), KHÔNG phải hỏi chọn một người
 * như bản nháp trước đây từng lo ngại. */
function inLyLichCaNhan(d: GiaDinhListItem): void {
  api.giaDinh.inLyLichCaNhanGiaDinh(d.id).catch(baoLoiIn('in được lý lịch cá nhân', d.id))
}

/** Đúng 5 mục và đúng thứ tự trong constructor của GxGiaDinhList bản desktop. "In giới thiệu
 * chuyển xứ" mở GioiThieuModal để nhập bên nhận trước khi in (mẫu thứ tư của "Giấy giới
 * thiệu" — theo GIA ĐÌNH, xem in-an.md mục 5e). "Xem vị trí" mở Google Maps với đúng địa chỉ
 * gia đình — xem `lib/xemViTri.ts` (cùng cơ chế `Memory.ViewMap` của bản desktop, gửi địa chỉ
 * ra Google — xem can-review-sau.md). */
export const menuGiaDinhMacDinh = (
  _moChiTiet: (d: GiaDinhListItem) => void,
  moGioiThieuChuyenXu: (d: GiaDinhListItem) => void,
): MucMenu<GiaDinhListItem>[] => [
  { nhan: 'In chứng nhận hôn phối', chay: inChungNhanHonPhoi },
  { nhan: 'In phiếu gia đình', chay: inPhieuGiaDinh },
  { nhan: 'In lý lịch cá nhân', chay: inLyLichCaNhan },
  { nhan: 'In giới thiệu chuyển xứ', chay: moGioiThieuChuyenXu },
  { nhan: 'Xem vị trí', chay: (d) => moBanDo(d.diaChi, 'Gia đình này không có địa chỉ để xem bản đồ.') },
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
