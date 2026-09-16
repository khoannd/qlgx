/**
 * Nhãn hiển thị cho `ThanhVienGiaDinh.VaiTro` — giữ NGUYÊN giá trị số thô (0..100) khi lưu/gửi
 * lên máy chủ (xem can-review-sau.md, task giao diện gia đình: "giữ nguyên giá trị VaiTro thô").
 * Danh sách 21 giá trị lấy từ khối chú thích cũ ở `Source/GXControl/frmGiaDinh.cs:992-1018`
 * (`Memory.GetQuanHeList()` thật không đọc được trong phạm vi nhiệm vụ — xem
 * docs/superpowers/specs/man-hinh/gia-dinh-chi-tiet.md mục 9). Dữ liệu thật `qlgx_thu` chỉ
 * dùng một tập con (0,1,2,3,8,18,100) — khớp với các giá trị 21 mục này, củng cố khả năng danh
 * sách chú thích vẫn còn đúng.
 */
export const VAI_TRO = {
  CHONG: 0, VO: 1, CON: 2, CHAU: 3, CHA: 4, ME: 5, ONG: 6, BA: 7,
  ANH: 8, CO: 9, CHU: 10, BAC: 11, CAU: 12, DI: 13, MO: 14, THIM: 15,
  DUONG: 16, CHI: 17, EM: 18, DAU: 19, RE: 20, CHUA_RO: 100,
} as const

const TEN_VAI_TRO: Record<number, string> = {
  0: 'Chồng', 1: 'Vợ', 2: 'Con', 3: 'Cháu', 4: 'Cha', 5: 'Mẹ', 6: 'Ông', 7: 'Bà',
  8: 'Anh', 9: 'Cô', 10: 'Chú', 11: 'Bác', 12: 'Cậu', 13: 'Dì', 14: 'Mợ', 15: 'Thím',
  16: 'Dượng', 17: 'Chị', 18: 'Em', 19: 'Dâu', 20: 'Rể', 100: 'Chưa rõ',
}

export const tenVaiTro = (vaiTro: number): string => TEN_VAI_TRO[vaiTro] ?? `#${vaiTro}`

/** Danh sách chọn vai trò khi thêm một "thành viên khác" (loại trừ Chồng/Vợ — hai vai trò đó
 * có ô chọn riêng "Người nam"/"Người nữ", không thêm qua đường này). */
export const DANH_SACH_VAI_TRO_THANH_VIEN = Object.entries(TEN_VAI_TRO)
  .map(([gt, nhan]) => ({ giaTri: Number(gt), nhan }))
  .filter((v) => v.giaTri !== VAI_TRO.CHONG && v.giaTri !== VAI_TRO.VO)
  .sort((a, b) => a.giaTri - b.giaTri)
