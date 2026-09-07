import { useLayoutEffect, useState, type CSSProperties, type ReactNode, type RefObject } from 'react'
import { createPortal } from 'react-dom'

type Props = {
  /** Phần tử neo (thường là chính ô input) — danh sách nổi lên ngay dưới (hoặc trên) phần tử
   * này, cùng bề rộng. */
  anchorRef: RefObject<HTMLElement | null>
  open: boolean
  children: ReactNode
  className?: string
  /** Bề rộng tối thiểu (px) — mặc định danh sách nổi rộng bằng đúng ô neo (`r.width`), phù hợp
   * cho gợi ý/kết quả tìm kiếm; một số nội dung khác (ví dụ giải thích cảnh báo ngày tháng, neo
   * vào một nút biểu tượng nhỏ) cần rộng hơn hẳn bề rộng nút — dùng `Math.max(r.width, minWidth)`
   * khi có giá trị này, không đổi hành vi cũ khi bỏ trống. */
  minWidth?: number
}

/** Chiều cao ước lượng tối đa của danh sách nổi — phải khớp `max-height` ở CSS
 * (`.gx-goiy-list`/`.picker-dropdown`) để quyết định lật lên hay xuống có ý nghĩa. */
const CAO_UOC_LUONG = 240

/**
 * Danh sách gợi ý/kết quả nổi dùng chung cho `GxGoiY` và `GxPicker` — render qua React portal
 * thẳng vào `document.body` với `position: fixed` tính theo toạ độ THẬT của ô neo
 * (`getBoundingClientRect`), thay vì `position: absolute` lồng trong DOM tại chỗ.
 *
 * LÝ DO: các khối `.card.glass` dùng `backdrop-filter` — thuộc tính này TỰ TẠO một stacking
 * context mới (đúng đặc tả CSS). Khi ô neo (ví dụ ô "Người đỡ đầu" khối Rửa tội) nằm trong một
 * `.card.glass`, một danh sách `position: absolute` bên trong khối đó — DÙ z-index cao bao
 * nhiêu — KHÔNG BAO GIỜ thoát ra khỏi ranh giới của khối cha đó để nổi lên TRÊN một khối
 * `.card.glass` khác đứng SAU nó trong DOM (ví dụ khối "Thêm sức") — toàn bộ khối cha được vẽ
 * như MỘT lớp duy nhất theo đúng thứ tự DOM, bất kể z-index của con cháu bên trong. Đây đúng là
 * lỗi người dùng báo cáo ("panel suggestion bị hide bên dưới") — xem
 * docs/superpowers/specs/man-hinh/can-review-sau.md.
 *
 * Portal ra `document.body` thoát hẳn khỏi MỌI stacking context/`overflow` của tổ tiên, nên
 * luôn nổi trên mọi khối khác. Tự tính lại vị trí khi cuộn/đổi cỡ cửa sổ, và tự LẬT LÊN TRÊN ô
 * neo khi không đủ chỗ phía dưới (ô nằm sát đáy trang) — đúng hành vi combobox chuẩn.
 */
export function GxDropdownPortal({ anchorRef, open, children, className, minWidth }: Props) {
  const [style, setStyle] = useState<CSSProperties | null>(null)

  useLayoutEffect(() => {
    if (!open) {
      setStyle(null)
      return
    }
    function dat() {
      const el = anchorRef.current
      if (!el) return
      const r = el.getBoundingClientRect()
      const conDuoi = window.innerHeight - r.bottom
      const lenTren = conDuoi < CAO_UOC_LUONG && r.top > conDuoi
      setStyle({
        position: 'fixed',
        left: r.left,
        width: minWidth ? Math.max(r.width, minWidth) : r.width,
        zIndex: 1000,
        ...(lenTren ? { bottom: window.innerHeight - r.top + 2 } : { top: r.bottom + 2 }),
      })
    }
    dat()
    window.addEventListener('resize', dat)
    window.addEventListener('scroll', dat, true)
    return () => {
      window.removeEventListener('resize', dat)
      window.removeEventListener('scroll', dat, true)
    }
  }, [open, anchorRef, minWidth])

  if (!open || !style) return null
  return createPortal(
    <div className={className} style={style}>
      {children}
    </div>,
    document.body,
  )
}
