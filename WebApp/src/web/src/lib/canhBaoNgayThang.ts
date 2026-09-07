import { dinhDangNgay } from './ngay'

/**
 * Cảnh báo MỀM về thứ tự thời gian giữa các mốc ngày của một giáo dân — KHÔNG chặn lưu, chỉ
 * gợi ý người dùng đối chiếu lại sổ gốc (yêu cầu trực tiếp người dùng 2026-09-07: "ngày rửa tội
 * trước ngày sinh, mặc dù vẫn cho phép nhưng nên có warning icon, click vào có giải thích").
 *
 * Khác hẳn quy tắc CHẶN CỨNG hiện có (Rule 6 của `GiaoDanService.KiemTraNghiepVu` — bắt buộc có
 * Ngày sinh) và cảnh báo Yes/No trước khi lưu (Rule 9 — CHỈ so Ngày sinh với Ngày rửa tội, đúng
 * BUG copy-paste của bản desktop đã ghi ở can-review-sau.md mục 1, `isValidDateInputRelations`,
 * `Source/GXControl/frmGiaoDan.cs:476-479`). Cảnh báo ở đây RỘNG HƠN có chủ đích — tái hiện đúng
 * tinh thần chuỗi mà bản desktop ĐỊNH kiểm tra ("Ngày sinh <= Ngày rửa tội <= Ngày rước lễ lần
 * đầu <= Ngày thêm sức", xem thông điệp Rule 9) cộng thêm Ngày xức dầu và Ngày qua đời, vì đây
 * là một tính năng MỚI được người dùng yêu cầu thêm — không phải migrate hành vi cũ, nên không
 * bị ràng buộc "giống hệt bản desktop, kể cả chỗ sai". 38 giáo dân thật trong `qlgx_thu` đang có
 * Ngày rửa tội trước Ngày sinh (dữ liệu sổ sách nhiều năm, có thể do ghi nhầm) — vì vậy đây CHỈ
 * LÀ CẢNH BÁO, tuyệt đối không chặn lưu, không đổi được `dungPayloadTuForm`.
 */

export type CacMocNgayGiaoDan = {
  ngaySinh?: string | null
  ngayRuaToi?: string | null
  ngayRuocLe?: string | null
  ngayThemSuc?: string | null
  ngayXucDau?: string | null
  ngayQuaDoi?: string | null
}

type Moc = keyof CacMocNgayGiaoDan

const NHAN: Record<Moc, string> = {
  ngaySinh: 'Ngày sinh',
  ngayRuaToi: 'Ngày rửa tội',
  ngayRuocLe: 'Ngày rước lễ',
  ngayThemSuc: 'Ngày thêm sức',
  ngayXucDau: 'Ngày xức dầu',
  ngayQuaDoi: 'Ngày qua đời',
}

/** Từng cặp [mốc SAU, mốc TRƯỚC] nên theo đúng thứ tự thời gian (SAU không được đứng trước
 * TRƯỚC). Đây là danh sách đã tự quyết — xem chú thích ở
 * docs/superpowers/specs/man-hinh/can-review-sau.md mục "Việc 1". */
const CAP_THU_TU: ReadonlyArray<readonly [Moc, Moc]> = [
  ['ngayRuaToi', 'ngaySinh'],
  ['ngayRuocLe', 'ngaySinh'],
  ['ngayRuocLe', 'ngayRuaToi'],
  ['ngayThemSuc', 'ngaySinh'],
  ['ngayThemSuc', 'ngayRuaToi'],
  ['ngayThemSuc', 'ngayRuocLe'],
  ['ngayXucDau', 'ngaySinh'],
  ['ngayXucDau', 'ngayRuaToi'],
  ['ngayQuaDoi', 'ngaySinh'],
  ['ngayQuaDoi', 'ngayRuaToi'],
  ['ngayQuaDoi', 'ngayRuocLe'],
  ['ngayQuaDoi', 'ngayThemSuc'],
  ['ngayQuaDoi', 'ngayXucDau'],
]

function chuThuong(s: string): string {
  return s.length === 0 ? s : s.charAt(0).toLowerCase() + s.slice(1)
}

/**
 * Trả về, cho mỗi mốc ngày, danh sách thông điệp cảnh báo (có con số ngày cụ thể) khi mốc đó
 * đứng TRƯỚC một mốc lẽ ra phải có trước nó. So sánh trực tiếp trên chuỗi ISO `yyyy-MM-dd`
 * (đúng thứ tự thời gian, không cần parse `Date`). Bỏ qua cặp nào thiếu một trong hai giá trị.
 */
export function tinhCanhBaoNgayThang(moc: CacMocNgayGiaoDan): Partial<Record<Moc, string[]>> {
  const ket: Partial<Record<Moc, string[]>> = {}
  for (const [sau, truoc] of CAP_THU_TU) {
    const isoSau = moc[sau]
    const isoTruoc = moc[truoc]
    if (!isoSau || !isoTruoc || isoSau >= isoTruoc) continue
    const thongDiep =
      `${NHAN[sau]} (${dinhDangNgay(isoSau)}) trước ${chuThuong(NHAN[truoc])} ` +
      `(${dinhDangNgay(isoTruoc)}). Thông tin vẫn được lưu — hãy đối chiếu lại với sổ gốc.`
    ;(ket[sau] ??= []).push(thongDiep)
  }
  return ket
}

/** Cảnh báo riêng cho Ngày hôn phối (tab "Hôn phối", nằm ở một component/state độc lập với các
 * mốc bí tích khác) so với Ngày sinh của chính giáo dân đó. */
export function canhBaoHonPhoiTruocSinh(ngayHonPhoi: string | null, ngaySinh: string | null): string | null {
  if (!ngayHonPhoi || !ngaySinh || ngayHonPhoi >= ngaySinh) return null
  return (
    `Ngày hôn phối (${dinhDangNgay(ngayHonPhoi)}) trước ngày sinh (${dinhDangNgay(ngaySinh)}). ` +
    'Thông tin vẫn được lưu — hãy đối chiếu lại với sổ gốc.'
  )
}
