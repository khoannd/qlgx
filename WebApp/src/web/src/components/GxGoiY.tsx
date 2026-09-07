import { useId, useRef, useState, type CSSProperties, type FocusEvent, type KeyboardEvent } from 'react'
import { focusKeTiep } from '../lib/focusDieuHuong'
import { ghiNhanDaDung, gopGoiY } from '../lib/goiYNhapLieu'

type Props = {
  id?: string
  /** Tên trường gửi trong `FormData` của form bọc ngoài — giữ nguyên quy ước của mọi ô nhập
   * khác trong hai màn hình chi tiết (đọc bằng `FormData`/`querySelector('[name="…"]')`). */
  name?: string
  /** Khoá Ý NGHĨA NGHIỆP VỤ của trường (vd `"tenThanh"`, `"noiRuaToi"`) — dùng làm một phần
   * khoá `localStorage` (xem `lib/goiYNhapLieu.ts`). CỐ Ý không dùng `name`/id DOM cho việc
   * này: nhiều ô ở nhiều màn hình khác nhau có thể cùng Ý NGHĨA ("nơi rửa tội" chỉ xuất hiện ở
   * một chỗ, nhưng "linh mục chứng" xuất hiện cả ở tab Hôn phối của giáo dân lẫn màn hình gia
   * đình) — gộp gợi ý theo ý nghĩa hữu ích hơn tách theo từng ô, đúng đề xuất thiết kế ở
   * ho-tro-nhap-lieu.md mục 4. */
  truong: string
  /** Khoá giáo xứ đang đăng nhập (từ claim, xem `AuthContext.giaoXuId`) — `null` khi chưa xác
   * định được (ví dụ bài test dựng component độc lập không bọc `<AuthProvider>`): tắt hẳn phần
   * lịch sử `localStorage`, KHÔNG được suy đoán/dùng giá trị mặc định nào khác để tránh lẫn gợi
   * ý giữa hai giáo xứ. */
  giaoXuId: string | null
  /** Danh mục có sẵn dùng chung cho MỌI giáo xứ đang đăng nhập ở CHÍNH trường này (vd danh sách
   * Tên thánh từ `du_lieu_chung`) — rỗng với các trường không có danh mục tĩnh nào (chỉ còn lại
   * gợi ý từ lịch sử). */
  danhMuc?: readonly string[]
  defaultValue?: string | null
  ariaLabel?: string
  placeholder?: string
  style?: CSSProperties
  maxLength?: number
  disabled?: boolean
}

/**
 * Ô nhập có gợi ý theo tần suất dùng — thay `<input type="text">` thường ở các ô hay lặp lại
 * (tên thánh, tên linh mục, nơi chốn, người đỡ đầu…), xem
 * docs/superpowers/specs/man-hinh/ho-tro-nhap-lieu.md mục 4 và can-review-sau.md mục 50.
 *
 * Gõ giá trị MỚI hoàn toàn (chưa từng có trong lịch sử lẫn danh mục) vẫn hoạt động bình thường
 * — ô này KHÔNG BAO GIỜ ép chọn từ danh sách, chỉ hiện gợi ý khi có và luôn cho gõ tiếp/gõ đè.
 *
 * Khả năng tiếp cận: mũi tên lên/xuống duyệt gợi ý, Enter xác nhận (VÀ tự nhảy sang control kế
 * tiếp — dùng lại `focusKeTiep`, đúng cơ chế vừa làm cho `GxDate`/`<select>`), Esc đóng danh
 * sách mà KHÔNG xoá nội dung đang gõ. Không bẫy Tab.
 */
export function GxGoiY({
  id, name, truong, giaoXuId, danhMuc = [], defaultValue, ariaLabel, placeholder, style, maxLength,
  disabled,
}: Props) {
  const [gt, setGt] = useState(defaultValue ?? '')
  const [mo, setMo] = useState(false)
  const [chiSo, setChiSo] = useState(-1)
  const inputRef = useRef<HTMLInputElement>(null)
  // Vừa ghi nhận trong chon() rồi — bỏ qua LẦN GHI NHẬN TIẾP THEO ở xuLyBlur (nhayKeTiep() gọi
  // .focus() sang control khác, tự kích hoạt blur ngay sau đó) để không đếm trùng một lượt
  // chọn thành hai lần dùng.
  const vuaGhiNhanRef = useRef(false)
  const listId = useId()

  const goiY = mo ? gopGoiY(giaoXuId, truong, danhMuc, gt) : []

  function nhayKeTiep() {
    const el = inputRef.current
    if (!el) return
    // Chờ React commit giá trị mới rồi mới tìm control kế tiếp trong DOM.
    setTimeout(() => focusKeTiep(el), 0)
  }

  function chon(giaTri: string) {
    setGt(giaTri)
    setMo(false)
    setChiSo(-1)
    if (giaoXuId) {
      ghiNhanDaDung(giaoXuId, truong, giaTri)
      vuaGhiNhanRef.current = true
    }
    nhayKeTiep()
  }

  function xuLyBlur(_e: FocusEvent<HTMLInputElement>) {
    // Rời ô sau khi tự gõ (không qua danh sách gợi ý) cũng được tính là "đã dùng" — tương
    // đương thời điểm desktop ghi vào autocomplete.xml lúc đóng form (GxTextField.cs), chỉ khác
    // là ghi ngay khi rời Ô thay vì đợi đóng cả FORM (đơn giản hơn, không cần biết lúc nào form
    // đóng, và không mất gợi ý nếu người dùng đóng tab mà quên bấm Lưu).
    if (vuaGhiNhanRef.current) {
      vuaGhiNhanRef.current = false
    } else if (giaoXuId && gt.trim()) {
      ghiNhanDaDung(giaoXuId, truong, gt)
    }
    setMo(false)
    setChiSo(-1)
  }

  function xuLyPhim(e: KeyboardEvent<HTMLInputElement>) {
    if (e.key === 'ArrowDown') {
      e.preventDefault()
      if (!mo) { setMo(true); return }
      setChiSo((i) => Math.min(i + 1, goiY.length - 1))
      return
    }
    if (e.key === 'ArrowUp') {
      e.preventDefault()
      if (!mo) return
      setChiSo((i) => Math.max(i - 1, 0))
      return
    }
    if (e.key === 'Enter') {
      if (mo && chiSo >= 0 && chiSo < goiY.length) {
        e.preventDefault()
        chon(goiY[chiSo])
      }
      // Không chọn gợi ý nào (đang gõ giá trị mới) — để Enter làm việc bình thường của nó
      // (submit form nếu có), không can thiệp.
      return
    }
    if (e.key === 'Escape') {
      if (mo) { e.stopPropagation(); setMo(false); setChiSo(-1) }
    }
  }

  return (
    <span className="gx-goiy" style={{ position: 'relative', display: 'inline-block', ...style }}>
      <input
        ref={inputRef}
        id={id}
        name={name}
        type="text"
        role="combobox"
        aria-expanded={mo && goiY.length > 0}
        aria-controls={listId}
        aria-autocomplete="list"
        aria-label={ariaLabel}
        autoComplete="off"
        placeholder={placeholder}
        maxLength={maxLength}
        disabled={disabled}
        value={gt}
        onChange={(e) => { setGt(e.target.value); setMo(true); setChiSo(-1) }}
        onFocus={() => setMo(true)}
        onBlur={xuLyBlur}
        onKeyDown={xuLyPhim}
        style={{ width: '100%' }}
      />
      {mo && goiY.length > 0 && (
        <ul id={listId} role="listbox" className="gx-goiy-list">
          {goiY.map((g, i) => (
            <li
              key={g}
              role="option"
              aria-selected={i === chiSo}
              className={i === chiSo ? 'active' : undefined}
              // preventDefault trên mousedown: ngăn input mất focus trước khi onClick chạy, để
              // không kích hoạt xuLyBlur (ghi nhận giá trị ĐANG GÕ DỞ) trước khi chon() kịp thay
              // bằng giá trị vừa chọn.
              onMouseDown={(e) => e.preventDefault()}
              onClick={() => chon(g)}
            >
              {g}
            </li>
          ))}
        </ul>
      )}
    </span>
  )
}
