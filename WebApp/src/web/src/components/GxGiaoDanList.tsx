import { forwardRef, useMemo } from 'react'
import type { Ref } from 'react'
import { api } from '../api/client'
import type { GiaoDanListItem } from '../api/types'
import { cotGiaoDan, cotQuanHeGiaDinh } from '../cot/cotGiaoDan'
import { chuaHoTro } from '../lib/thongBao'
import { GxGrid, type GxGridHandle, type MucMenu } from './GxGrid'

/** "In lý lịch cá nhân" / "In chứng nhận bí tích..." từ menu chuột phải — cùng lệnh gọi với
 * nút ở màn hình chi tiết, nhưng gọi thẳng ở đây vì menu chuột phải của lưới không đi qua
 * trang chi tiết. Lỗi báo bằng alert() — nhất quán cho mọi mục in trên lưới này, tránh thêm
 * một kênh thông báo mới chỉ cho một mục. */
function baoLoiIn(hanhDong: string, giaoDanId: string) {
  return (e: unknown) => {
    console.error(`Không ${hanhDong} của giáo dân ${giaoDanId}`, e)
    window.alert(e instanceof Error ? e.message : 'In thất bại, thử lại sau.')
  }
}

function inLyLichCaNhan(d: GiaoDanListItem): void {
  api.giaoDan.inLyLichCaNhan(d.id).catch(baoLoiIn('in được lý lịch cá nhân', d.id))
}

/** Bốn mục "In chứng nhận bí tích/rửa tội/xưng tội-rước lễ/thêm sức" dùng CHUNG một hàm, chỉ
 * khác `loai` gửi lên — xem InAnService.XuatChungNhanBiTich (mẫu HTML dùng chung, chỉ đổi tiêu
 * đề và dòng bí tích được liệt kê theo `loai`). */
function inChungNhanBiTich(loai?: 'RuaToi' | 'RuocLe' | 'ThemSuc') {
  return (d: GiaoDanListItem): void => {
    api.giaoDan.inChungNhanBiTich(d.id, loai).catch(baoLoiIn('in được chứng nhận bí tích', d.id))
  }
}

/** Ba loại "Giấy giới thiệu" theo giáo dân (mẫu thứ tư — chuyển xứ — theo gia đình, xem
 * GxGiaDinhList.tsx) — khác các mục in phía trên, cần MỞ MỘT MÀN HÌNH nhập tay thông tin bên
 * nhận trước khi in được (xem GioiThieuModal.tsx), nên nơi gọi truyền vào một hàm mở modal
 * thay vì gọi thẳng api. */
export type LoaiGioiThieuGiaoDan = 'RuaToi' | 'ThemSuc' | 'GiaoLyHonPhoi'

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
  moGioiThieu: (loai: LoaiGioiThieuGiaoDan, d: GiaoDanListItem) => void,
): MucMenu<GiaoDanListItem>[] => [
  { nhan: 'Xem chi tiết', chay: moChiTiet },
  // "In lý lịch cá nhân": mẫu đầu tiên của hạ tầng in ấn thật (VIEC-TIEP-THEO.md mục 1.1, xem
  // docs/superpowers/specs/man-hinh/in-an.md) — tải PDF thật, không còn là mục chỉ có nhãn.
  { nhan: 'In lý lịch cá nhân', chay: inLyLichCaNhan },
  // 4 mục "chứng nhận bí tích" — dùng CHUNG một endpoint, khác nhau ở `loai`.
  { nhan: 'In chứng nhận bí tích', chay: inChungNhanBiTich() },
  // "In giới thiệu hôn phối" KHÔNG thuộc 4 mẫu "Giấy giới thiệu" của lượt này — đây là giấy
  // RAO hôn phối (ReportRaoHP.cs, xem in-an.md mục 8 "CHƯA làm"), vẫn báo "chưa hỗ trợ".
  { nhan: 'In giới thiệu hôn phối', chay: chuaHoTro },
  { nhan: 'In chứng nhận rửa tội', chay: inChungNhanBiTich('RuaToi') },
  { nhan: 'In chứng nhận xưng tội - rước lễ', chay: inChungNhanBiTich('RuocLe') },
  { nhan: 'In chứng nhận thêm sức', chay: inChungNhanBiTich('ThemSuc') },
  { nhan: 'Xem gia đình', chay: xemGiaDinh, an: (d) => !d.giaDinhId },
  // Ba mẫu "Giấy giới thiệu" theo giáo dân (mẫu thứ tư — chuyển xứ — theo gia đình, xem
  // GxGiaDinhList.tsx) — mở GioiThieuModal để nhập bên nhận trước khi in, xem in-an.md mục 5e.
  { nhan: 'In giấy giới thiệu chứng nhận rửa tội', chay: (d) => moGioiThieu('RuaToi', d) },
  { nhan: 'In giấy giới thiệu giáo lý hôn phối', chay: (d) => moGioiThieu('GiaoLyHonPhoi', d) },
  { nhan: 'In giấy giới thiệu chứng nhận thêm sức', chay: (d) => moGioiThieu('ThemSuc', d) },
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
