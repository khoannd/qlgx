/**
 * Định dạng/diễn giải ngày tháng phía hiển thị (client). Toàn bộ dữ liệu ngày trao đổi với
 * máy chủ vẫn giữ nguyên dạng ISO `yyyy-MM-dd` (đúng serialize mặc định của `DateOnly` phía
 * .NET) — module này KHÔNG đổi hợp đồng API, chỉ đổi cách NGƯỜI DÙNG nhìn thấy/gõ ngày, đúng
 * `dd/MM/yyyy` như bản desktop (xem `Qlgx.Data/NgayThangText.cs` phía máy chủ, quy ước tương
 * đương cho dữ liệu Access cũ — KHÔNG viết lại logic đó ở đây, chỉ tuân theo cùng quy ước).
 *
 * Xem docs/superpowers/specs/man-hinh/can-review-sau.md mục W1.
 */

const RE_ISO = /^(\d{4})-(\d{2})-(\d{2})/
const RE_HIEN_THI = /^(\d{1,2})\/(\d{1,2})\/(\d{4})$/

/**
 * Đổi ISO `yyyy-MM-dd` (hoặc `yyyy-MM-ddTHH:mm:ss…`) sang `dd/MM/yyyy` để hiển thị.
 * Dữ liệu lỗi (chuỗi rỗng, chỉ có năm như `"1958"`, giá trị không phân giải được — xem cột
 * `du_lieu_loi` ở máy chủ) được giữ NGUYÊN VĂN, không ném lỗi, không làm sập màn hình.
 */
export function dinhDangNgay(iso?: string | null): string {
  if (!iso) return ''
  const m = RE_ISO.exec(iso)
  if (!m) return iso
  const [, yyyy, mm, dd] = m
  return `${dd}/${mm}/${yyyy}`
}

/**
 * Đổi chuỗi người dùng gõ theo `dd/MM/yyyy` (chấp nhận `d/M/yyyy` không cần số 0 đứng đầu)
 * sang ISO `yyyy-MM-dd` để gửi lên máy chủ. Trả `null` nếu chuỗi rỗng (nghĩa là "xoá ngày"),
 * trả `undefined` nếu chuỗi có nội dung nhưng KHÔNG phải một ngày hợp lệ (khác `null` để nơi
 * gọi phân biệt được "cố ý để trống" và "gõ sai" — không được âm thầm nuốt giá trị sai thành
 * rỗng).
 */
export function ngayTuHienThi(hienThi: string): string | null | undefined {
  const s = hienThi.trim()
  if (s === '') return null

  const m = RE_HIEN_THI.exec(s)
  if (!m) return undefined

  const ngay = Number(m[1])
  const thang = Number(m[2])
  const nam = Number(m[3])
  if (thang < 1 || thang > 12 || ngay < 1 || ngay > 31) return undefined

  // Kiểm tra ngày có thật trong tháng đó (vd 30/02 không tồn tại) bằng cách dựng lại Date UTC
  // rồi đối chiếu ngược — Date tự "tràn" ngày/tháng không hợp lệ sang tháng kế tiếp thay vì báo
  // lỗi, nên phải so sánh lại từng phần mới phát hiện được.
  const dt = new Date(Date.UTC(nam, thang - 1, ngay))
  if (dt.getUTCFullYear() !== nam || dt.getUTCMonth() !== thang - 1 || dt.getUTCDate() !== ngay) {
    return undefined
  }

  const p2 = (n: number) => String(n).padStart(2, '0')
  return `${nam}-${p2(thang)}-${p2(ngay)}`
}

// --- Control nhập ngày kiểu "mask" một ô (GxDate) ------------------------------------------
// Bản desktop dùng ba ô rời `dd`/`mm`/`yyyy`; bản web dùng MỘT ô văn bản nhưng giữ đúng cảm
// giác gõ liên tục không cần dấu `/`, tự nhảy giữa các phần — bằng cách luôn hiển thị đủ 10 ký
// tự theo khuôn `__/__/____` (ký tự thiếu là `_`), gõ số ở đúng vị trí con trỏ, dấu `/` cố
// định không gõ được. Xem docs/superpowers/specs/man-hinh/ho-tro-nhap-lieu.md mục A.2-A.4.

export const KHUON_NGAY_RONG = '__/__/____'

/** Vị trí ký tự có thể gõ số (bỏ qua hai dấu `/` cố định ở vị trí 2 và 5). */
const VI_TRI_SO = [0, 1, 3, 4, 6, 7, 8, 9] as const
/** Vị trí bắt đầu của từng phần (ngày/tháng/năm) — gõ số ở đây thì xoá sạch cả phần trước khi
 * ghi, giống `SelectAll()` khi focus vào ô có sẵn số ở bản desktop (mục A.2). */
const DAU_PHAN = new Set([0, 3, 6])

function doDaiPhan(dauPhan: number): number {
  return dauPhan === 6 ? 4 : 2
}

/** Đổi ISO `yyyy-MM-dd` sang khuôn 10 ký tự `dd/mm/yyyy`; rỗng/không phải ISO đầy đủ → khuôn
 * trống `__/__/____`. */
export function khuonTuIso(iso?: string | null): string {
  if (!iso) return KHUON_NGAY_RONG
  const m = RE_ISO.exec(iso)
  if (!m) return KHUON_NGAY_RONG
  const [, yyyy, mm, dd] = m
  return `${dd}/${mm}/${yyyy}`
}

/** Vị trí gõ số đầu tiên còn trống (dùng khi control nhận focus từ Tab, không phải click trực
 * tiếp vào một vị trí cụ thể) — trống hoàn toàn thì về đầu (vị trí 0), đã đủ thì cũng về đầu để
 * người dùng gõ đè lại từ đầu. */
export function viTriGoDauTien(khuon: string): number {
  const trong = VI_TRI_SO.find((p) => khuon[p] === '_')
  return trong ?? 0
}

/** Nếu `pos` rơi đúng vào dấu `/` (vị trí 2 hoặc 5), đẩy sang vị trí gõ số kế tiếp — dùng khi
 * người dùng click/focus vào giữa hai dấu `/`. */
export function chuanViTri(pos: number): number {
  if (pos === 2) return 3
  if (pos === 5) return 6
  if (pos > 9) return 9
  if (pos < 0) return 0
  return pos
}

/** Ghi một chữ số vào `khuon` tại `pos`. Nếu `pos` là đầu một phần (ngày/tháng/năm), xoá sạch
 * cả phần đó trước (xem `DAU_PHAN`). Trả về khuôn mới và vị trí gõ tiếp theo — `null` nghĩa là
 * vừa gõ xong chữ số cuối cùng của năm (đã hết chỗ gõ), nơi gọi nên thử chuẩn hoá + tự nhảy
 * sang control kế tiếp. */
export function goSoVaoKhuon(khuon: string, pos: number, so: string): { khuon: string; viTriKe: number | null } {
  const p = chuanViTri(pos)
  const mang = khuon.split('')
  if (DAU_PHAN.has(p)) {
    for (let i = 0; i < doDaiPhan(p); i++) mang[p + i] = '_'
  }
  mang[p] = so
  const idx = VI_TRI_SO.indexOf(p as (typeof VI_TRI_SO)[number])
  const ke = idx >= 0 && idx + 1 < VI_TRI_SO.length ? VI_TRI_SO[idx + 1] : null
  return { khuon: mang.join(''), viTriKe: ke }
}

/** Xử lý phím Backspace: xoá chữ số tại vị trí con trỏ hiện có (hoặc lùi về chữ số gần nhất
 * phía trước rồi xoá, nếu con trỏ đang ở dấu `/` hoặc ô rỗng). */
export function xoaLuiTrongKhuon(khuon: string, pos: number): { khuon: string; viTriKe: number } {
  let p = chuanViTri(pos)
  if (khuon[p] === '_') {
    const truoc = [...VI_TRI_SO].reverse().find((e) => e < p)
    if (truoc === undefined) return { khuon, viTriKe: p }
    p = truoc
  }
  const mang = khuon.split('')
  mang[p] = '_'
  return { khuon: mang.join(''), viTriKe: p }
}

/** Trả về vị trí bắt đầu của phần KẾ TIẾP so với phần chứa `pos` — dùng khi người dùng tự gõ
 * dấu `/` giữa chừng (coi là "xong phần này, sang phần sau"), hoặc `null` nếu đã ở phần cuối
 * (năm). Gõ `/` khi đang đứng NGAY ĐẦU một phần (chưa gõ gì) thì bỏ qua (không nhảy), tránh
 * nuốt mất phần đó khi người dùng gõ dư dấu `/` sau khi control đã tự nhảy sẵn. */
export function phanKeTiep(pos: number): number | null {
  if (pos === 0) return 3
  if (pos === 3) return 6
  return null
}

function toaDoPhan(pos: number): number {
  if (pos <= 1) return 0
  if (pos <= 4) return 3
  return 6
}

export function dangODauPhan(pos: number): boolean {
  return DAU_PHAN.has(chuanViTri(pos))
}

export function dauPhanCuaViTri(pos: number): number {
  return toaDoPhan(chuanViTri(pos))
}

/** Tách khuôn 10 ký tự thành ba chuỗi số ngày/tháng/năm (bỏ ký tự `_`, có thể rỗng). */
export function khuonSangPhan(khuon: string): { ngay: string; thang: string; nam: string } {
  const bo = (s: string) => s.replace(/_/g, '')
  return { ngay: bo(khuon.slice(0, 2)), thang: bo(khuon.slice(3, 5)), nam: bo(khuon.slice(6, 10)) }
}

/**
 * Áp dụng đúng quy tắc bản desktop cho ngày-tháng-CÓ-THỂ-thiếu (mục A.4 của spec): chỉ năm,
 * hoặc tháng+năm là hợp lệ; ngày một mình (không tháng) thì KHÔNG hợp lệ. Khác bản desktop ở
 * một điểm — bản desktop giữ nguyên phần thiếu trong chuỗi lưu xuống Access, còn bản web đã
 * chốt phương án "chuẩn hoá lúc nhập": thiếu ngày → điền `01`, thiếu cả ngày lẫn tháng → điền
 * `01/01` (xem `Qlgx.Data/NgayThangText.cs`, cùng quy tắc, chỉ khác nơi áp dụng — server áp
 * dụng lúc ĐỌC dữ liệu Access cũ, hàm này áp dụng lúc NGƯỜI DÙNG gõ trên web).
 *
 * Trả `null` khi cả ba phần đều rỗng (ô để trống — hợp lệ, nghĩa là "xoá ngày"), `undefined`
 * khi có nội dung nhưng không hợp lệ (năm chưa đủ 4 số, có ngày mà thiếu tháng, hoặc ngày/tháng
 * không tồn tại thật — ví dụ 31/02), ngược lại trả ISO `yyyy-MM-dd` đã chuẩn hoá.
 */
export function chuanHoaNgayThieu(ngay: string, thang: string, nam: string): string | null | undefined {
  const d = ngay.trim()
  const m = thang.trim()
  const y = nam.trim()
  if (d === '' && m === '' && y === '') return null
  if (y.length !== 4) return undefined
  if (d !== '' && m === '') return undefined

  const dd = d === '' ? '01' : d.padStart(2, '0')
  const mm = m === '' ? '01' : m.padStart(2, '0')
  return ngayTuHienThi(`${dd}/${mm}/${y}`)
}
