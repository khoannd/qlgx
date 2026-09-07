/**
 * Tự động chuyển tiêu điểm (focus) sang control kế tiếp trên form — dùng chung cho control
 * ngày tháng (`GxDate`, gõ xong ngày/tháng/năm) và cho các ô chọn sẵn (`<select>`, chọn xong
 * một mục trong dropdown), đúng yêu cầu người dùng ("khi chọn 1 item trong dropdown... tự nhảy
 * sang mục tiếp theo") và tương đương cơ chế `GetNextControl` của bản desktop
 * (`Source/GXControl/GxDateInput.cs:477,485,579`, xem
 * docs/superpowers/specs/man-hinh/ho-tro-nhap-lieu.md mục B).
 *
 * QUAN TRỌNG (khả năng tiếp cận — xem mục 3 của spec trên): hàm này chỉ được gọi ở đúng thời
 * điểm người dùng vừa hoàn tất một hành động xác nhận rõ ràng (gõ xong số cuối, chọn xong một
 * mục) — KHÔNG được gọi khi rời ô bằng Tab thủ công hay click chuột nơi khác, để không tự động
 * đá văng người dùng khỏi nơi họ chủ động click/Tab tới (anti-pattern WCAG "thay đổi ngữ cảnh
 * ngoài ý muốn"). `Tab` vẫn hoạt động bình thường ở mọi nơi — hàm này không can thiệp gì vào
 * hành vi Tab của trình duyệt, chỉ chủ động focus thêm SAU MỘT hành động khác.
 */

import { useEffect } from 'react'

const CO_THE_FOCUS = 'input, select, textarea, button, a[href], [tabindex]'

function laFocusDuoc(el: HTMLElement): boolean {
  if (el.hasAttribute('disabled')) return false
  if (el.tabIndex === -1) return false
  if (el.closest('[hidden]')) return false
  if (typeof window !== 'undefined' && typeof window.getComputedStyle === 'function') {
    const kieu = window.getComputedStyle(el)
    if (kieu.display === 'none' || kieu.visibility === 'hidden') return false
  }
  return true
}

/**
 * Tìm và focus control kế tiếp theo đúng thứ tự xuất hiện trong DOM, trong phạm vi `<form>`
 * bao quanh `hienTai` (hoặc `document.body` nếu không nằm trong `<form>` nào).
 *
 * `vungLoaiTru`: phần tử (thường là khung bọc ngoài của CHÍNH control đang đứng, ví dụ
 * `.gx-date`) bị loại khỏi danh sách ứng viên — để không tự nhảy lung tung giữa các ô con của
 * cùng một control (nút mở lịch, ô ISO ẩn...). Mặc định là `hienTai`.
 */
/**
 * Hook gắn MỘT LẦN ở gốc ứng dụng (`App.tsx`) — tự động chuyển tiêu điểm sang control kế tiếp
 * mỗi khi người dùng CHỌN XONG một mục trong một ô `<select>` bất kỳ nằm trong `<form>` (đúng
 * yêu cầu người dùng: "khi chọn 1 item trong dropdown, suggestion thì nó cũng sẽ tự nhảy sang
 * mục tiếp theo"). Làm ở tầng dùng chung bằng event delegation trên `document` — mọi `<select>`
 * hiện có VÀ mọi ô mới thêm sau này trong một `<form>` đều tự có hành vi này, không cần sửa
 * từng màn hình.
 *
 * Chỉ áp dụng cho `<select>` có `form` (nằm trong một `<form>`) — các ô lọc/chọn nhanh ở đầu
 * lưới danh sách (không bọc `<form>`) không bị ảnh hưởng, tránh đá focus khỏi thao tác lọc dữ
 * liệu ngoài ý muốn. Sự kiện `change` của `<select>` chỉ bắn ra khi người dùng đã THẬT SỰ chọn
 * xong một mục (bàn phím Enter/mũi tên rồi rời ra, hoặc click chuột) — không bắn khi chỉ di
 * chuột qua hay đang mở danh sách xem trước, nên không có nguy cơ nhảy focus khi chưa xác nhận.
 */
export function useTuNhayKhiChonDropdown() {
  useEffect(() => {
    function xuLy(e: Event) {
      const target = e.target
      if (!(target instanceof HTMLSelectElement)) return
      if (!target.form) return
      focusKeTiep(target)
    }
    document.addEventListener('change', xuLy)
    return () => document.removeEventListener('change', xuLy)
  }, [])
}

export function focusKeTiep(hienTai: HTMLElement, vungLoaiTru?: HTMLElement | null) {
  const vung = vungLoaiTru ?? hienTai
  const container = hienTai.closest('form') ?? document.body
  const list = Array.from(container.querySelectorAll<HTMLElement>(CO_THE_FOCUS))

  for (const el of list) {
    if (vung.contains(el)) continue
    if (!laFocusDuoc(el)) continue
    // eslint-disable-next-line no-bitwise
    if (vung.compareDocumentPosition(el) & Node.DOCUMENT_POSITION_FOLLOWING) {
      el.focus()
      return
    }
  }
}
