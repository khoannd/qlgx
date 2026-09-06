import { useRef, useState, type CSSProperties } from 'react'
import { dinhDangNgay, ngayTuHienThi } from '../lib/ngay'

type Props = {
  id?: string
  /** Tên trường gửi trong `FormData` của form bọc ngoài (giữ nguyên ISO `yyyy-MM-dd`, đúng
   * hợp đồng API hiện có) — bỏ trống thì ô chỉ hiển thị, không tham gia submit, giống các ô
   * ngày tĩnh chưa nối API ở tab "Giáo lý". */
  name?: string
  ariaLabel?: string
  /** Giá trị ban đầu — ISO `yyyy-MM-dd`/`null`/`''`/dữ liệu lỗi giữ nguyên văn (xem
   * `du_lieu_loi`). Đây là input KHÔNG kiểm soát (giống `defaultValue` của input gốc) —
   * đổi `defaultValue` sau khi đã dựng không cập nhật lại ô, đúng hành vi input gốc mà toàn
   * bộ hai màn hình chi tiết đang dựa vào (đọc bằng `FormData`/`querySelector` lúc lưu, không
   * bằng state React). */
  defaultValue?: string | null
  style?: CSSProperties
  disabled?: boolean
}

/**
 * Ô nhập ngày dùng chung — thay `<input type="date">` gốc của trình duyệt, vốn hiển thị theo
 * locale máy người dùng (đã đo được `mm/dd/yyyy` trên máy kiểm thử) trong khi bản desktop và
 * toàn bộ dữ liệu Access đều dùng `dd/MM/yyyy` — xem
 * docs/superpowers/specs/man-hinh/can-review-sau.md mục W1.
 *
 * Cấu tạo ba phần:
 *  - Một ô văn bản hiển thị/nhập `dd/MM/yyyy` thật (không phụ thuộc locale) — người dùng gõ
 *    thẳng được, sai định dạng thì báo lỗi rõ ràng ngay dưới ô, KHÔNG âm thầm nuốt giá trị.
 *  - Một nút tròn mở lịch bấm chọn bằng `showPicker()` của một `<input type="date">` ẩn khỏi
 *    mắt (không dùng `hidden`/`display:none` — hai cách đó chặn `showPicker()` ở một số trình
 *    duyệt) để vẫn chọn được bằng chuột như trước, không bắt gõ tay hoàn toàn.
 *  - `name`/giá trị ISO thật sự nằm trên CHÍNH ô lịch ẩn đó (không dựng thêm một
 *    `<input type="hidden">` riêng) — nơi gọi hiện có đọc giá trị qua `FormData`/
 *    `querySelector('[name="…"]')` không cần đổi gì khi chuyển sang dùng ô này. Không tách
 *    thành hai input cùng mang giá trị ISO để tránh trùng lặp (từng khiến
 *    `getByDisplayValue` trong bài test khớp nhầm hai phần tử).
 */
export function GxDate({ id, name, ariaLabel, defaultValue, style, disabled }: Props) {
  const gtBanDau = defaultValue ?? ''
  const [iso, setIso] = useState(gtBanDau)
  const [text, setText] = useState(dinhDangNgay(gtBanDau))
  const [loi, setLoi] = useState<string | null>(null)
  const ngayRef = useRef<HTMLInputElement>(null)

  function xuLyGoTay(giaTri: string) {
    setText(giaTri)
    const ket = ngayTuHienThi(giaTri)
    // `undefined` = đang gõ dở/sai định dạng: chưa cập nhật ISO, cũng chưa báo lỗi vội (tránh
    // chớp lỗi ngay từ ký tự đầu tiên) — chỉ báo khi rời khỏi ô (xuLyRoiO).
    if (ket !== undefined) {
      setIso(ket ?? '')
      setLoi(null)
    }
  }

  function xuLyRoiO() {
    const ket = ngayTuHienThi(text)
    if (ket === undefined) {
      setLoi('Ngày không hợp lệ — nhập theo dd/mm/yyyy')
      return
    }
    setLoi(null)
    setIso(ket ?? '')
    setText(dinhDangNgay(ket))
  }

  function xuLyChonLich(giaTriIso: string) {
    setIso(giaTriIso)
    setText(dinhDangNgay(giaTriIso))
    setLoi(null)
  }

  return (
    <span className="gx-date" style={style}>
      <span className="gx-date-row">
        <input
          id={id}
          aria-label={ariaLabel}
          aria-invalid={loi ? true : undefined}
          type="text"
          inputMode="numeric"
          placeholder="dd/mm/yyyy"
          value={text}
          disabled={disabled}
          onChange={(e) => xuLyGoTay(e.target.value)}
          onBlur={xuLyRoiO}
          className={loi ? 'invalid' : undefined}
        />
        <button
          type="button"
          className="mini gx-date-btn"
          title="Chọn ngày từ lịch"
          disabled={disabled}
          tabIndex={-1}
          onClick={() => {
            const el = ngayRef.current
            if (!el) return
            if (typeof el.showPicker === 'function') el.showPicker()
            else el.focus()
          }}
        >
          📅
        </button>
        <input
          ref={ngayRef}
          type="date"
          name={name}
          className="gx-date-native"
          tabIndex={-1}
          aria-hidden="true"
          disabled={disabled}
          value={iso}
          onChange={(e) => xuLyChonLich(e.target.value)}
        />
      </span>
      {loi && <span className="gx-date-loi" role="alert">{loi}</span>}
    </span>
  )
}
