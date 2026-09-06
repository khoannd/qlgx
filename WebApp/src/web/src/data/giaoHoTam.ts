/**
 * Danh mục Giáo họ — TẠM THỜI. Backend chưa có endpoint "Danh mục giáo họ" (nằm ngoài các
 * task 6–8 hiện có của Phase 1), nên các combobox "Giáo họ" ở bốn màn hình của task này tạm
 * dùng danh sách tên cứng, giống dữ liệu minh hoạ GIAO_HO của bản mẫu. Khi có endpoint thật,
 * thay `DANH_SACH_GIAO_HO_TAM` bằng dữ liệu nạp từ API và đổi các `<select>` sang dùng
 * `giaoHoId` thay vì so khớp theo tên.
 */
export const DANH_SACH_GIAO_HO_TAM: string[] = [
  'Giáo họ Antôn',
  'Giáo họ Fatima',
  'Giáo họ Lộ Đức',
  'Giáo họ Mân Côi',
  'Giáo họ Thánh Giuse',
  'Giáo họ Thánh Tâm',
]

/** Giá trị sentinel dùng trong các `<select>` Giáo họ khi bản ghi không thuộc giáo họ nào
 * (giaoHoId null) — đúng quy ước MaGiaoHo = 0 nghĩa là "Ngoài xứ" của `frmGiaoDan.cs`. */
export const NGOAI_XU = 'Ngoài xứ'
