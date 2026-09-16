import { useEffect, useRef } from 'react'

/**
 * Bản nháp ngoại tuyến — tự lưu nội dung đang gõ dở của form chi tiết (giáo dân/gia đình) vào
 * `localStorage` theo chu kỳ, để mất mạng lúc bấm Lưu (hoặc mất điện/đóng nhầm tab) không làm
 * mất công gõ. Xem yêu cầu gốc ở
 * docs/superpowers/specs/2026-09-06-qlgx-web-migration-design.md mục 3 và quyết định chi tiết ở
 * docs/superpowers/specs/man-hinh/can-review-sau.md (mục "Task 16").
 *
 * BA điều bắt buộc (đã đo được là dễ sai nhất — xem yêu cầu gốc):
 *  1. `localStorage` là theo TRÌNH DUYỆT, không theo tài khoản — khoá lưu LUÔN gắn kèm tên tài
 *     khoản đang đăng nhập (`banNhapKhoa`), và `docBanNhap` từ chối trả về bản nháp không khớp
 *     tài khoản hiện tại (không xoá — chỉ ẩn, để tài khoản kia còn thấy lại khi họ đăng nhập).
 *  2. Đọc/ghi `localStorage` có thể ném lỗi (chế độ riêng tư, trang bị chặn lưu dữ liệu) — MỌI
 *     lần đọc/ghi ở đây đều bọc try/catch, im lặng bỏ qua, ứng dụng vẫn chạy bình thường.
 *  3. Cấu trúc form có thể đổi giữa các lần triển khai — mỗi bản nháp mang `phienBan`; gặp bản
 *     nháp phiên bản cũ thì coi như không có (xoá luôn cho gọn), KHÔNG cố áp dụng để tránh làm
 *     hỏng form.
 */

/** Tăng số này khi đổi hình dạng payload của MỘT trong các form đang dùng bản nháp (thêm/bớt
 * trường theo cách không tương thích ngược) — bản nháp cũ sẽ tự bị bỏ qua thay vì áp dụng nhầm. */
export const PHIEN_BAN_BAN_NHAP = 1

const TIEN_TO = 'qlgx.bannhap'

type PhongBiBanNhap<T> = {
  phienBan: number
  tenTaiKhoan: string
  /** ISO 8601 — thời điểm lưu nháp gần nhất, hiện cho người dùng biết đang khôi phục bản nào. */
  thoiDiem: string
  duLieu: T
}

/** Khoá `localStorage` cho một form chi tiết — LUÔN gắn tên tài khoản để tránh lẫn nháp giữa
 * hai người dùng chung một máy/trình duyệt (xem điều 1 ở trên). `id === null` ứng với bản ghi
 * đang tạo mới (chưa có id thật) — mọi thẻ "mới" cùng loại dùng chung một khoá "moi"; mở nhiều
 * thẻ nháp cùng lúc thì các thẻ ghi đè nháp của nhau, coi là đánh đổi chấp nhận được (xem
 * can-review-sau.md). */
export function banNhapKhoa(loaiForm: string, id: string | null, tenTaiKhoan: string): string {
  return `${TIEN_TO}.v${PHIEN_BAN_BAN_NHAP}.${encodeURIComponent(tenTaiKhoan)}.${loaiForm}.${id ?? 'moi'}`
}

export function luuBanNhap<T>(khoa: string, tenTaiKhoan: string, duLieu: T): void {
  try {
    const goi: PhongBiBanNhap<T> = {
      phienBan: PHIEN_BAN_BAN_NHAP, tenTaiKhoan, thoiDiem: new Date().toISOString(), duLieu,
    }
    localStorage.setItem(khoa, JSON.stringify(goi))
  } catch {
    // localStorage bị chặn/đầy — bỏ qua, không có bản nháp lần này nhưng form vẫn hoạt động
    // bình thường (điều 2 ở trên).
  }
}

/** Đọc bản nháp — trả `null` nếu không có, phiên bản không khớp (xoá luôn), hoặc tài khoản
 * không khớp người đang đăng nhập (KHÔNG xoá — vẫn còn đó cho đúng chủ nhân sau này). */
export function docBanNhap<T>(khoa: string, tenTaiKhoan: string): { duLieu: T; thoiDiem: string } | null {
  try {
    const tho = localStorage.getItem(khoa)
    if (!tho) return null
    const goi = JSON.parse(tho) as Partial<PhongBiBanNhap<T>>
    if (goi.phienBan !== PHIEN_BAN_BAN_NHAP) {
      localStorage.removeItem(khoa)
      return null
    }
    if (goi.tenTaiKhoan !== tenTaiKhoan) return null
    if (goi.duLieu === undefined || typeof goi.thoiDiem !== 'string') return null
    return { duLieu: goi.duLieu, thoiDiem: goi.thoiDiem }
  } catch {
    return null
  }
}

export function xoaBanNhap(khoa: string): void {
  try {
    localStorage.removeItem(khoa)
  } catch {
    // xem điều 2 ở trên
  }
}

/** Xoá TOÀN BỘ bản nháp của một tài khoản — gọi khi người dùng chủ động đăng xuất (không gọi
 * khi bị đăng xuất do token hết hạn trong lúc mất mạng: người dùng cần đăng nhập lại rồi vẫn
 * khôi phục được nháp, xem AuthContext.dangXuat). */
export function xoaTatCaBanNhapCuaTaiKhoan(tenTaiKhoan: string): void {
  try {
    const canXoa: string[] = []
    for (let i = 0; i < localStorage.length; i++) {
      const k = localStorage.key(i)
      if (k && k.startsWith(`${TIEN_TO}.`) && k.includes(`.${encodeURIComponent(tenTaiKhoan)}.`)) {
        canXoa.push(k)
      }
    }
    canXoa.forEach((k) => localStorage.removeItem(k))
  } catch {
    // xem điều 2 ở trên
  }
}

/**
 * Tự lưu nháp theo chu kỳ (mặc định 5 giây) — KHÔNG ghi mỗi phím gõ (yêu cầu gốc), và bỏ qua
 * lần ghi nếu nội dung không đổi so với "mốc gốc" để đỡ tốn `localStorage` khi người dùng
 * ngừng gõ nhưng chưa rời trang. `layPayload` trả về `null` nghĩa là "chưa có gì đáng lưu" (ví
 * dụ khi form chưa dựng xong) — bỏ qua lượt đó, không xoá nháp đang có.
 *
 * "Mốc gốc" (`noiDungCuoiRef`) được chốt ngay từ giá trị `layPayload()` trả về ở LẦN CHẠY ĐẦU
 * TIÊN (đúng bằng dữ liệu vừa tải từ máy chủ, chưa ai sửa gì) — nếu không có bước chốt này, lần
 * hẹn giờ đầu tiên sẽ ghi một "bản nháp ma" giống hệt dữ liệu máy chủ chỉ vì người dùng mở form
 * ra xem mà chưa sửa gì, khiến lần sau mở lại bị hỏi "khôi phục" một cách vô nghĩa (đã đo được
 * bằng kiểm thử thật trên `qlgx_thu`, xem task-16-report.md).
 */
export function useTuDongLuuBanNhap<T>(
  khoa: string | null,
  tenTaiKhoan: string | null,
  layPayload: () => T | null,
  chuKyMs = 5000,
): void {
  const layPayloadRef = useRef(layPayload)
  useEffect(() => {
    layPayloadRef.current = layPayload
  })

  const noiDungCuoiRef = useRef<string | null>(null)
  const daChotMocGocRef = useRef(false)

  useEffect(() => {
    if (!khoa || !tenTaiKhoan) return
    if (!daChotMocGocRef.current) {
      const banDau = layPayloadRef.current()
      noiDungCuoiRef.current = banDau === null ? null : JSON.stringify(banDau)
      daChotMocGocRef.current = true
    }
    const dinhKy = setInterval(() => {
      const payload = layPayloadRef.current()
      if (payload === null) return
      const json = JSON.stringify(payload)
      if (json === noiDungCuoiRef.current) return
      noiDungCuoiRef.current = json
      luuBanNhap(khoa, tenTaiKhoan, payload)
    }, chuKyMs)
    return () => clearInterval(dinhKy)
  }, [khoa, tenTaiKhoan, chuKyMs])
}
