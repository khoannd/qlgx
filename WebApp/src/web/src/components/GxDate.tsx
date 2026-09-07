import {
  useLayoutEffect,
  useRef,
  useState,
  type CSSProperties,
  type FocusEvent,
  type KeyboardEvent,
  type MouseEvent,
} from 'react'
import { flushSync } from 'react-dom'
import {
  chuanHoaNgayThieu,
  chuanViTri,
  dauPhanCuaViTri,
  dinhDangNgay,
  goSoVaoKhuon,
  khuonSangPhan,
  khuonTuIso,
  KHUON_NGAY_RONG,
  phanKeTiep,
  viTriGoDauTien,
  xoaLuiTrongKhuon,
} from '../lib/ngay'
import { focusKeTiep } from '../lib/focusDieuHuong'

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
  /** Gọi lại mỗi khi giá trị ISO thật sự đổi (gõ xong, chọn lịch, hoặc xoá về trống) — dùng cho
   * nơi gọi cần theo dõi giá trị SỐNG để tính cảnh báo liên-trường (ví dụ so ngày rửa tội với
   * ngày sinh, xem `lib/canhBaoNgayThang.ts`). Không gọi lúc mới dựng — nơi gọi đã có giá trị
   * ban đầu qua `defaultValue`. Ô này vẫn là input KHÔNG kiểm soát (`defaultValue`), callback
   * chỉ để ĐỌC, không dùng để áp giá trị mới xuống ô. */
  onIsoChange?: (iso: string) => void
}

/**
 * Ô nhập ngày dùng chung — thay `<input type="date">` gốc của trình duyệt, vốn hiển thị theo
 * locale máy người dùng (đã đo được `mm/dd/yyyy` trên máy kiểm thử) trong khi bản desktop và
 * toàn bộ dữ liệu Access đều dùng `dd/MM/yyyy` — xem
 * docs/superpowers/specs/man-hinh/can-review-sau.md mục W1.
 *
 * Ô nhập chính mô phỏng lại control ngày ba-ô của bản desktop (`GxDateInput`, xem
 * docs/superpowers/specs/man-hinh/ho-tro-nhap-lieu.md mục A) NHƯNG dựng bằng MỘT `<input
 * type="text">` duy nhất luôn hiển thị đủ khuôn 10 ký tự `dd/mm/yyyy` (thiếu là `_`, ví dụ
 * `__/__/____` lúc trống) — giữ cảm giác "ba ô rời, gõ liên tục không cần dấu `/`, tự nhảy
 * giữa các phần" của desktop mà không cần ba phần tử DOM riêng (đơn giản hơn khi cần focus
 * xuyên ra NGOÀI control, và Tab/đọc màn hình vẫn coi đây là một ô văn bản bình thường — không
 * bẫy focus). Logic mask thuần (không đụng DOM) nằm ở `lib/ngay.ts` để test độc lập.
 *
 * Cấu tạo còn lại:
 *  - Một nút tròn mở lịch bấm chọn bằng `showPicker()` của một `<input type="date">` ẩn khỏi
 *    mắt (không dùng `hidden`/`display:none` — hai cách đó chặn `showPicker()` ở một số trình
 *    duyệt) để vẫn chọn được bằng chuột như trước, không bắt gõ tay hoàn toàn.
 *  - `name`/giá trị ISO thật sự nằm trên CHÍNH ô lịch ẩn đó (không dựng thêm một
 *    `<input type="hidden">` riêng) — nơi gọi hiện có đọc giá trị qua `FormData`/
 *    `querySelector('[name="…"]')` không cần đổi gì khi chuyển sang dùng ô này.
 *
 * Chuẩn hoá ngày thiếu (chỉ năm, hoặc tháng+năm): áp dụng ngay khi gõ xong chữ số cuối của năm
 * (không cần rời ô) VÀ khi rời ô (blur, phòng trường hợp người dùng Tab đi giữa chừng) — đúng
 * quyết định người dùng đã chốt, phương án "chuẩn hoá lúc nhập" (xem `chuanHoaNgayThieu`).
 */
export function GxDate({ id, name, ariaLabel, defaultValue, style, disabled, onIsoChange }: Props) {
  const gtBanDau = defaultValue ?? ''
  // Dữ liệu lỗi cũ không map được vào khuôn 10 ký tự (ví dụ "1958" — chỉ có năm, xem
  // du_lieu_loi) được hiện NGUYÊN VĂN lúc mới dựng, không nhét ép vào khuôn — nhưng ngay khi
  // người dùng bắt đầu sửa (focus/gõ) thì coi như bắt đầu nhập lại từ đầu bằng khuôn trống,
  // không cố "vá" chuỗi lỗi cũ vào từng ô.
  const laIsoDayDu = gtBanDau === '' || /^\d{4}-\d{2}-\d{2}/.test(gtBanDau)
  const [dangTho, setDangTho] = useState(!laIsoDayDu)
  const [text, setText] = useState(laIsoDayDu ? khuonTuIso(gtBanDau) : gtBanDau)
  const [iso, setIso] = useState(laIsoDayDu ? gtBanDau : '')
  const [loi, setLoi] = useState<string | null>(null)
  const ngayRef = useRef<HTMLInputElement>(null)
  const inputRef = useRef<HTMLInputElement>(null)
  const viTriMongMuon = useRef<number | null>(null)

  function datCaretSau(pos: number | null) {
    // Input là controlled — value chỉ thật sự cập nhật trên DOM sau khi React commit xong, nên
    // không set vị trí con trỏ ngay ở đây mà ghi vào ref rồi để useLayoutEffect (chạy đồng bộ
    // NGAY SAU khi DOM cập nhật, trước khi trình duyệt vẽ lại) đọc và áp dụng — tránh phụ thuộc
    // timer/requestAnimationFrame (không đáng tin trong môi trường test).
    viTriMongMuon.current = pos
  }

  useLayoutEffect(() => {
    const el = inputRef.current
    if (el && viTriMongMuon.current !== null) {
      el.setSelectionRange(viTriMongMuon.current, viTriMongMuon.current)
      viTriMongMuon.current = null
    }
  }, [text])

  function thuChuanHoaVaCoTheNhay(khuonMoi: string, elGoc: HTMLElement, nhaySangKeTiep: boolean) {
    const { ngay, thang, nam } = khuonSangPhan(khuonMoi)
    const ket = chuanHoaNgayThieu(ngay, thang, nam)
    if (ket === undefined) {
      setLoi('Ngày không hợp lệ — nhập theo dd/mm/yyyy (chỉ năm, hoặc tháng và năm, cũng được)')
      return
    }
    // QUAN TRỌNG: dùng flushSync để BUỘC React vẽ lại (render + commit DOM) NGAY LẬP TỨC, ĐỒNG
    // BỘ, trước khi gọi focusKeTiep bên dưới — nếu không, focusKeTiep gọi el.focus() sang control
    // kế tiếp sẽ bắn sự kiện `blur` ĐỒNG BỘ ngay trong cùng tick, trong khi state vừa setLoi/
    // setIso/setText ở trên CHƯA kịp commit (React gộp các setState trong một handler rồi mới
    // flush). Handler `onBlur` (xuLyRoiO) khi đó vẫn là closure CŨ, đóng gói `text` từ TRƯỚC khi
    // gõ xong chữ số cuối (ví dụ "01/02/200_" thay vì "01/02/2003") — gọi lại
    // chuanHoaNgayThieu() với năm thiếu 1 chữ số, trả `undefined`, và tự đè `loi` bằng thông báo
    // "Ngày không hợp lệ" SAI dù giá trị đã nhập hoàn toàn đúng và đã lưu đúng xuống ô ISO ẩn.
    // Đây là nguyên nhân thật của lỗi người dùng báo cáo (không phải do ràng buộc nghiệp vụ nào
    // — xem docs/superpowers/specs/man-hinh/can-review-sau.md).
    flushSync(() => {
      setLoi(null)
      setIso(ket ?? '')
      if (ket !== null) setText(dinhDangNgay(ket))
    })
    onIsoChange?.(ket ?? '')
    if (ket !== null) {
      // Chỉ tự nhảy control kế tiếp khi hành động vừa rồi là "vừa gõ xong chữ số cuối của năm"
      // — không nhảy khi chuẩn hoá xảy ra lúc blur (người dùng đã tự chủ động rời ô bằng cách
      // khác, không nên bị đẩy thêm một lần nữa). Xem lib/focusDieuHuong.ts.
      if (nhaySangKeTiep) {
        const khung = elGoc.closest('.gx-date') as HTMLElement | null
        focusKeTiep(elGoc, khung)
      }
    }
  }

  function batDauNhapLaiNeuDangTho() {
    if (!dangTho) return false
    setDangTho(false)
    setText(KHUON_NGAY_RONG)
    return true
  }

  function xuLyFocus(e: FocusEvent<HTMLInputElement>) {
    if (batDauNhapLaiNeuDangTho()) {
      // Khuôn vừa đổi từ chuỗi lỗi thô sang khuôn trống — text sẽ đổi, useLayoutEffect([text])
      // lo phần đặt con trỏ.
      datCaretSau(0)
      return
    }
    // Focus từ Tab (không phải click — click sẽ tự đặt lại vị trí đúng ngay sau đó, xem
    // xuLyClick) — về vị trí gõ số đầu tiên còn trống, giống hành vi desktop khi GxDateInput
    // nhận focus từ control khác (GxDateInput.cs:362-365). `text` không đổi ở nhánh này nên
    // useLayoutEffect sẽ không tự chạy lại — đặt con trỏ trực tiếp ngay tại đây.
    const vt = viTriGoDauTien(text)
    e.currentTarget.setSelectionRange(vt, vt)
  }

  function xuLyClick(e: MouseEvent<HTMLInputElement>) {
    if (dangTho) return // vừa chuyển sang khuôn trống ở xuLyFocus, để nguyên vị trí click
    const el = e.currentTarget
    const pos = el.selectionStart ?? 0
    const chuan = chuanViTri(pos)
    if (chuan !== pos) el.setSelectionRange(chuan, chuan)
  }

  function xuLyKeyDown(e: KeyboardEvent<HTMLInputElement>) {
    if (disabled) return
    const el = e.currentTarget
    const khuonHienTai = dangTho ? KHUON_NGAY_RONG : text
    const pos = el.selectionStart ?? 0

    if (/^[0-9]$/.test(e.key)) {
      e.preventDefault()
      if (dangTho) batDauNhapLaiNeuDangTho()
      const { khuon, viTriKe } = goSoVaoKhuon(khuonHienTai, pos, e.key)
      setText(khuon)
      setDangTho(false)
      if (viTriKe === null) {
        // Vừa gõ xong chữ số cuối của năm — không còn chỗ gõ tiếp, thử chuẩn hoá + tự nhảy.
        thuChuanHoaVaCoTheNhay(khuon, el, true)
      } else {
        datCaretSau(viTriKe)
      }
      return
    }

    if (e.key === '/') {
      e.preventDefault()
      const dauPhan = dauPhanCuaViTri(pos)
      if (pos === dauPhan) return // dấu / dư (control đã tự nhảy sẵn) — bỏ qua, không nhảy hụt
      const ke = phanKeTiep(dauPhan)
      if (ke !== null) datCaretSau(ke)
      return
    }

    if (e.key === 'Backspace') {
      if (dangTho) return // để trình duyệt tự xoá chuỗi lỗi cũ theo cách thông thường
      e.preventDefault()
      const { khuon, viTriKe } = xoaLuiTrongKhuon(khuonHienTai, pos)
      setText(khuon)
      setLoi(null)
      // Xoá về hoàn toàn trống thì đồng bộ NGAY giá trị ISO gửi đi (ô lịch ẩn) về rỗng — không
      // đợi tới lúc rời ô (blur). Quan trọng: nếu không làm ngay, ISO cũ vẫn còn nằm trên ô ẩn
      // cho tới khi blur, và có tình huống blur không kịp xảy ra trước khi form được lưu (ví dụ
      // bấm thẳng nút Cập nhật) — lưu nhầm giá trị ngày ĐÃ XOÁ trên màn hình xuống CSDL.
      if (khuon === KHUON_NGAY_RONG) {
        setIso('')
        onIsoChange?.('')
      }
      datCaretSau(viTriKe)
    }
    // Mọi phím khác (Tab, mũi tên, Enter…) để trình duyệt xử lý bình thường — KHÔNG bẫy focus.
  }

  function xuLyRoiO() {
    if (dangTho) return // chưa động tới, giữ nguyên chuỗi lỗi cũ hiển thị
    if (inputRef.current) thuChuanHoaVaCoTheNhay(text, inputRef.current, false)
  }

  function xuLyChonLich(giaTriIso: string) {
    setDangTho(false)
    setIso(giaTriIso)
    setText(khuonTuIso(giaTriIso))
    setLoi(null)
    onIsoChange?.(giaTriIso)
  }

  return (
    <span className="gx-date" style={style}>
      <span className="gx-date-row">
        <input
          ref={inputRef}
          id={id}
          aria-label={ariaLabel}
          aria-invalid={loi ? true : undefined}
          type="text"
          inputMode="numeric"
          placeholder={KHUON_NGAY_RONG}
          value={text}
          disabled={disabled}
          onFocus={xuLyFocus}
          onClick={xuLyClick}
          onKeyDown={xuLyKeyDown}
          onBlur={xuLyRoiO}
          onChange={() => {
            /* Giá trị thay đổi hoàn toàn qua onKeyDown (bàn phím) — onChange chỉ cần khai báo
             * để React không cảnh báo "controlled input thiếu onChange"; nguồn nhập khác
             * (dán/kéo-thả) không được hỗ trợ ở ô này, đúng như control ba-ô của desktop cũng
             * không hỗ trợ dán. */
          }}
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
