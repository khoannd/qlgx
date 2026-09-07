import { useRef, useState } from 'react'
import { GxDropdownPortal } from './GxDropdownPortal'

type Props = {
  /** Một hoặc nhiều thông điệp cảnh báo cho CÙNG một ô ngày (ví dụ vừa trước Ngày sinh vừa
   * trước Ngày rửa tội). Rỗng/`undefined` thì không dựng gì cả. */
  thongDiep?: string[]
  /** Nhãn ô ngày đang được cảnh báo — ghép vào `aria-label` của nút để người dùng đọc màn hình
   * biết cảnh báo này thuộc về ô nào. */
  nhanO: string
}

/**
 * Biểu tượng cảnh báo MỀM cạnh một ô ngày — KHÁC HẲN viền đỏ `.invalid` của `GxDate` (đó là lỗi
 * CHẶN CỨNG, xem `aria-invalid`/`.gx-date-loi`). Đây chỉ là NHẮC NHỞ: vẫn cho lưu bình thường,
 * bấm vào mới hiện giải thích cụ thể. Yêu cầu trực tiếp người dùng 2026-09-07, xem
 * docs/superpowers/specs/man-hinh/can-review-sau.md mục "Việc 1" và `lib/canhBaoNgayThang.ts`.
 *
 * Là `<button>` thật (không phải `<span onClick>`) để bấm được bằng bàn phím/Enter — người dùng
 * nhập liệu hàng nghìn bản ghi bằng bàn phím. Giải thích nổi qua `GxDropdownPortal` (giống danh
 * sách gợi ý `GxGoiY`/`GxPicker`) để không bị cắt bởi `overflow: hidden` của `.tabpages` hay
 * stacking context riêng của `.card.glass` (`backdrop-filter`) — xem chú thích ở
 * GxDropdownPortal.tsx.
 */
export function GxCanhBaoNgay({ thongDiep, nhanO }: Props) {
  const [mo, setMo] = useState(false)
  const nutRef = useRef<HTMLButtonElement>(null)
  if (!thongDiep || thongDiep.length === 0) return null

  return (
    <>
      <button
        ref={nutRef}
        type="button"
        className="gx-canhbao-ngay-nut"
        aria-label={`Cảnh báo ngày tháng — ${nhanO}: ${thongDiep.join(' ')}`}
        aria-expanded={mo}
        title="Ngày tháng có thể chưa đúng thứ tự — bấm để xem giải thích"
        onClick={() => setMo((v) => !v)}
      >
        ⚠
      </button>
      <GxDropdownPortal anchorRef={nutRef} open={mo} className="gx-canhbao-ngay-giaithich" minWidth={280}>
        <div role="note">
          {thongDiep.map((t) => <p key={t}>{t}</p>)}
        </div>
      </GxDropdownPortal>
    </>
  )
}
