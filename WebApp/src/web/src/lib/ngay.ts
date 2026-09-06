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
