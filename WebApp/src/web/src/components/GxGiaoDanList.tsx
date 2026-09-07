import { forwardRef, useMemo } from 'react'
import type { Ref } from 'react'
import { api } from '../api/client'
import type { GiaoDanListItem } from '../api/types'
import { cotGiaoDan, cotQuanHeGiaDinh } from '../cot/cotGiaoDan'
import { chuaHoTro } from '../lib/thongBao'
import { GxGrid, type GxGridHandle, type MucMenu } from './GxGrid'

/** "In lý lịch cá nhân" từ menu chuột phải — cùng lệnh gọi với nút ở màn hình chi tiết
 * (`GiaoDanDetailPage.inLyLichCaNhan`), nhưng gọi thẳng ở đây vì menu chuột phải của lưới
 * không đi qua trang chi tiết. Lỗi báo bằng alert() — nhất quán với `chuaHoTro()` cho các mục
 * menu khác, tránh thêm một kênh thông báo mới chỉ cho một mục. */
function inLyLichCaNhan(d: GiaoDanListItem): void {
  api.giaoDan.inLyLichCaNhan(d.id).catch((e: unknown) => {
    console.error(`Không in được lý lịch cá nhân của giáo dân ${d.id}`, e)
    window.alert(e instanceof Error ? e.message : 'In thất bại, thử lại sau.')
  })
}

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
  // "In lý lịch cá nhân": mẫu đầu tiên của hạ tầng in ấn thật (VIEC-TIEP-THEO.md mục 1.1, xem
  // docs/superpowers/specs/man-hinh/in-an.md) — tải PDF thật, không còn là mục chỉ có nhãn.
  { nhan: 'In lý lịch cá nhân', chay: inLyLichCaNhan },
  // 9 mục còn lại CHƯA làm ở lượt này (xem in-an.md mục phạm vi) — trước đây bấm không phản
  // hồi gì (chỉ có `nhan`, không có `chay`); nay báo rõ "chưa hỗ trợ" bằng `chuaHoTro`, đúng
  // yêu cầu "đừng để im lặng không phản hồi".
  { nhan: 'In chứng nhận bí tích', chay: chuaHoTro },
  { nhan: 'In giới thiệu hôn phối', chay: chuaHoTro },
  { nhan: 'In chứng nhận rửa tội', chay: chuaHoTro },
  { nhan: 'In chứng nhận xưng tội - rước lễ', chay: chuaHoTro },
  { nhan: 'In chứng nhận thêm sức', chay: chuaHoTro },
  { nhan: 'Xem gia đình', chay: xemGiaDinh, an: (d) => !d.giaDinhId },
  { nhan: 'In giấy giới thiệu chứng nhận rửa tội', chay: chuaHoTro },
  { nhan: 'In giấy giới thiệu giáo lý hôn phối', chay: chuaHoTro },
  { nhan: 'In giấy giới thiệu chứng nhận thêm sức', chay: chuaHoTro },
  { nhan: 'Xem vị trí', chay: chuaHoTro },
]

/**
 * Tương đương UserControl GxGiaoDanList. Tự sở hữu bộ cột, quy tắc tô đỏ và ghi chú chân
 * lưới, nên nơi nhúng chỉ truyền dữ liệu. Nhờ vậy lưới ở màn hình danh sách và lưới thành
 * viên trong form gia đình là cùng một component — kiểm thử một lần dùng được cả hai.
 *
 * Chuyển tiếp `ref` xuống `GxGrid` để nơi nhúng (thanh công cụ "Xuất dữ liệu") gọi được
 * `layCsv()` mà không cần biết chi tiết AG Grid bên trong.
 */
function GxGiaoDanListTrong(
  { rows, quanHeGiaDinh, ...phanConLai }: Props,
  ref: Ref<GxGridHandle>,
) {
  const columnDefs = useMemo(
    () =>
      quanHeGiaDinh
        ? [cotQuanHeGiaDinh, ...cotGiaoDan.filter((c) => c.field !== 'dienThoai')]
        : cotGiaoDan,
    [quanHeGiaDinh],
  )

  return (
    <GxGrid<GiaoDanListItem>
      ref={ref}
      columnDefs={columnDefs}
      rowData={rows}
      layId={(d) => d.id}
      // Bản desktop KHÔNG gạch ngang dòng nào trên lưới GIÁO DÂN (GxGiaoDanList.FormattingRow
      // để trống — xem can-review-sau.md mục 7): gạch ngang chỉ có ý nghĩa ở lưới GIA ĐÌNH
      // (cột GACH nội bộ của frmGiaDinh, dùng cho lưới "Thành viên khác" — quanHeGiaDinh=true).
      // Cố ý bỏ `lapGd` khỏi điều kiện (khác một bản nháp trước đó của web) — xem quyết định
      // ghi ở can-review-sau.md: gạch ngang người "đã lập gia đình" gây hiểu nhầm nghiêm trọng
      // (phần lớn giáo dân trưởng thành đã lập gia đình), trong khi quaDoi/daChuyenDi đúng
      // nghĩa "không còn sinh hoạt" và mặc định đã bị lọc ẩn khỏi lưới.
      toDo={quanHeGiaDinh ? (d) => d.quaDoi || d.daChuyenDi : undefined}
      ghiChuChan={quanHeGiaDinh ? 'Gạch ngang đỏ: đã qua đời hoặc đã chuyển xứ' : undefined}
      {...phanConLai}
    />
  )
}

export const GxGiaoDanList = forwardRef(GxGiaoDanListTrong)
