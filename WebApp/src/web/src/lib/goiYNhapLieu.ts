/**
 * Gợi ý nhập liệu theo tần suất dùng — điểm đặc biệt của bản desktop mà người dùng yêu cầu tái
 * hiện (và làm tốt hơn) ở bản web, xem docs/superpowers/specs/man-hinh/ho-tro-nhap-lieu.md.
 *
 * Nghiên cứu mã nguồn desktop (mục C của spec trên) cho thấy cơ chế thật KHÔNG như người dùng
 * nhớ: `DuLieuChung` (Tên thánh) là danh mục tĩnh không đếm tần suất, còn `autocomplete.xml`
 * (các ô tự do khác) chỉ nhớ giá trị theo thứ tự lần đầu gõ, cũng không đếm tần suất. Bản web
 * làm ĐÚNG điều người dùng mô tả — đếm số lần dùng thật, xếp cái hay dùng lên đầu — bằng
 * `localStorage` (quyết định của người dùng: đơn giản, không thêm việc phía máy chủ, và tương
 * đương ĐÚNG hành vi desktop vốn cũng lưu cục bộ từng máy).
 *
 * BA điều bắt buộc khi dùng `localStorage` cho việc này (xem yêu cầu gốc):
 *  1. Tách theo GIÁO XỨ — khoá luôn kèm `giaoXuId` (xem `khoa()`). Đây là ranh giới dữ liệu
 *     thật: máy chủ phục vụ nhiều giáo xứ, hai người ở hai giáo xứ khác nhau dùng chung một
 *     trình duyệt (máy dùng chung ở văn phòng giáo xứ) KHÔNG được thấy gợi ý lẫn nhau. CỐ Ý
 *     KHÔNG tách thêm theo tài khoản — khác `lib/banNhap.ts` (bản nháp cá nhân, tách theo tài
 *     khoản vì mỗi người có bản nháp riêng): gợi ý nhập liệu (tên thánh, tên linh mục, địa danh
 *     quen thuộc của giáo xứ) là hiểu biết CHUNG hữu ích cho mọi nhân viên văn phòng của CÙNG
 *     một giáo xứ — tách theo tài khoản chỉ làm gợi ý học chậm hơn mà không thêm an toàn nào
 *     (họ vốn đã cùng được xem/sửa toàn bộ dữ liệu giáo dân của giáo xứ đó).
 *  2. GIỚI HẠN dung lượng — `localStorage` ~5MB, ném lỗi khi đầy. Tối đa `SO_MUC_TOI_DA` giá
 *     trị mỗi trường, loại bỏ giá trị ít dùng nhất (rồi cũ nhất) khi vượt.
 *  3. MỌI đọc/ghi bọc `try/catch` — chế độ riêng tư/trình duyệt chặn lưu trữ ném lỗi, ứng dụng
 *     vẫn phải chạy bình thường, chỉ là không có gợi ý (đúng khuôn `lib/banNhap.ts` đã có).
 *
 * KHÔNG lưu dữ liệu nhạy cảm ở đây — chỉ gọi `ghiNhanDaDung` cho các trường tái dùng được (tên
 * thánh, tên linh mục, địa danh), KHÔNG BAO GIỜ cho họ tên giáo dân, CMND/CCCD, điện thoại,
 * email, địa chỉ — xem nơi gọi ở `GiaoDanDetail.tsx`/`GiaDinhDetail.tsx`.
 */

export type MucGoiY = { giaTri: string; soLan: number; lanCuoi: number }

const TIEN_TO = 'qlgx.goiy.v1'
/** Tối đa mỗi trường (vd "tenThanh", "noiRuaToi"...) giữ bấy nhiêu giá trị khác nhau — một giáo
 * xứ vài nghìn giáo dân không cần tới vài trăm địa danh/tên linh mục khác nhau cho MỘT trường,
 * dư sức đủ dùng mà không lo đầy `localStorage`. */
const SO_MUC_TOI_DA = 300

function khoa(giaoXuId: string, truong: string): string {
  return `${TIEN_TO}.${encodeURIComponent(giaoXuId)}.${truong}`
}

function laMucGoiY(x: unknown): x is MucGoiY {
  const m = x as Partial<MucGoiY> | null
  return !!m && typeof m.giaTri === 'string' && typeof m.soLan === 'number' && typeof m.lanCuoi === 'number'
}

function docDanhSach(giaoXuId: string, truong: string): MucGoiY[] {
  try {
    const tho = window.localStorage.getItem(khoa(giaoXuId, truong))
    if (!tho) return []
    const ds: unknown = JSON.parse(tho)
    return Array.isArray(ds) ? ds.filter(laMucGoiY) : []
  } catch {
    return []
  }
}

function ghiDanhSach(giaoXuId: string, truong: string, ds: MucGoiY[]): void {
  try {
    window.localStorage.setItem(khoa(giaoXuId, truong), JSON.stringify(ds))
  } catch {
    // localStorage đầy hoặc bị chặn (chế độ riêng tư) — bỏ qua, ứng dụng vẫn hoạt động bình
    // thường, chỉ là lần này không lưu được gợi ý (điều 3 ở trên).
  }
}

function sapXep(ds: MucGoiY[]): MucGoiY[] {
  return [...ds].sort((a, b) => b.soLan - a.soLan || b.lanCuoi - a.lanCuoi)
}

/** Nguồn của `lanCuoi` — KHÔNG dùng thẳng `Date.now()` cho mỗi lần ghi: `Date.now()` chỉ chính
 * xác tới mili-giây, và nhiều lượt ghi liên tiếp trong cùng một mili-giây (vd người dùng gõ rất
 * nhanh, hoặc nhiều ô tự lưu nháp cùng lúc) sẽ TRÙNG giá trị, khiến việc xếp hạng/loại bỏ mục cũ
 * khi vượt giới hạn (điều 2) không còn phân biệt được đúng thứ tự trước/sau. Khởi tạo bằng
 * `Date.now()` lúc tải trang (SEED_LAN_CUOI) rồi TĂNG DẦN mỗi lần ghi — vẫn đơn điệu tăng ĐÚNG
 * theo thời gian thực xuyên suốt các phiên làm việc (mọi phiên sau luôn tải trang ở một thời
 * điểm thực muộn hơn, nên seed của phiên sau luôn lớn hơn mọi giá trị phiên trước để lại), mà
 * bên trong MỘT phiên thì không bao giờ trùng (mỗi lần ghi luôn tăng thêm ít nhất 1). */
let laLanCuoiKeTiep = (() => {
  try {
    return Date.now()
  } catch {
    return 0
  }
})()
function lanCuoiKeTiep(): number {
  laLanCuoiKeTiep += 1
  return laLanCuoiKeTiep
}

/** Ghi nhận một giá trị VỪA ĐƯỢC DÙNG (chọn từ gợi ý, hoặc rời ô sau khi gõ) — tăng bộ đếm nếu
 * đã có (so sánh không phân biệt hoa/thường, đúng cách desktop so khớp `autocomplete.xml`),
 * thêm mới với `soLan = 1` nếu chưa có. Chuỗi rỗng/toàn khoảng trắng bị bỏ qua. */
export function ghiNhanDaDung(giaoXuId: string, truong: string, giaTriTho: string): void {
  const giaTri = giaTriTho.trim()
  if (!giaTri) return
  const ds = docDanhSach(giaoXuId, truong)
  const idx = ds.findIndex((m) => m.giaTri.toLowerCase() === giaTri.toLowerCase())
  const bayGio = lanCuoiKeTiep()
  if (idx >= 0) {
    ds[idx] = { ...ds[idx], soLan: ds[idx].soLan + 1, lanCuoi: bayGio }
  } else {
    ds.push({ giaTri, soLan: 1, lanCuoi: bayGio })
  }
  const xepXong = sapXep(ds)
  // Vượt giới hạn — loại bỏ giá trị ÍT DÙNG NHẤT (đã xếp cuối danh sách), không phải giá trị
  // vừa thêm/sửa (luôn được giữ vì vừa được dùng).
  ghiDanhSach(giaoXuId, truong, xepXong.slice(0, SO_MUC_TOI_DA))
}

/** Lịch sử người dùng đã gõ cho một trường, xếp hay-dùng-nhất lên đầu (dùng chính lâu nhất khi
 * bằng số lần). */
export function layGoiY(giaoXuId: string, truong: string): string[] {
  return sapXep(docDanhSach(giaoXuId, truong)).map((m) => m.giaTri)
}

/**
 * Gộp NGUỒN gợi ý: lịch sử người dùng (localStorage, xếp theo tần suất — ưu tiên hiển thị
 * TRƯỚC, đúng yêu cầu "ưu tiên theo số lần người dùng đã dùng, rồi mới tới danh mục có sẵn") và
 * danh mục có sẵn (vd Tên thánh từ `du_lieu_chung` — rỗng với các trường không có danh mục
 * tĩnh nào). Lọc theo đoạn đang gõ (chứa ở bất kỳ đâu trong chuỗi, không phân biệt hoa/thường —
 * dễ dùng hơn so khớp đầu chuỗi khi người dùng nhớ một phần tên ở giữa), loại trùng giữa hai
 * nguồn, giới hạn số lượng hiển thị. `giaoXuId = null` (chưa xác định được tài khoản/giáo xứ)
 * tắt hẳn phần lịch sử, không tắt phần danh mục.
 */
export function gopGoiY(
  giaoXuId: string | null,
  truong: string,
  danhMuc: readonly string[],
  dangGo: string,
  gioiHan = 8,
): string[] {
  const tu = dangGo.trim().toLowerCase()
  const khop = (s: string) => tu === '' || s.toLowerCase().includes(tu)
  const daCo = new Set<string>()
  const ket: string[] = []

  function nap(nguon: readonly string[]) {
    for (const s of nguon) {
      if (ket.length >= gioiHan) return
      if (!khop(s)) continue
      const k = s.toLowerCase()
      if (daCo.has(k)) continue
      daCo.add(k)
      ket.push(s)
    }
  }

  if (giaoXuId) nap(layGoiY(giaoXuId, truong))
  nap(danhMuc)
  return ket
}
