/**
 * `POST /api/gia-dinh/{id}/thanh-vien` và `PUT /api/gia-dinh/{id}/vo-chong/{vaiTro}` đều trả
 * `{giaoDanId: null, canhBao: [...]}` cho HAI tình huống khác nhau (xem báo cáo backend "ghi
 * gia đình", mục ghi chú cuối): (a) cảnh báo nghiệp vụ THẬT cần xác nhận Yes/No một lượt
 * (`boQuaCanhBao=true` rồi gọi lại), hoặc (b) một câu CỐ ĐỊNH (sentinel) yêu cầu client hỏi
 * thêm trước khi có thể tiếp tục — client phân biệt hai loại này bằng NỘI DUNG câu cảnh báo
 * (server không có trường `loai` riêng — hạn chế đã biết, xem báo cáo).
 */

/** `GanVoChong` trả đúng câu này khi vai trò đang có người khác — cần chạy cây quyết định
 * `NguoiCu` (xem `lib/nguoiCu.ts`) trước khi gửi lại kèm `xuLyNguoiCu`. */
export const canQuyetDinhNguoiCu = (canhBao: string[]): boolean =>
  canhBao.length === 1 && canhBao[0].startsWith('Gia đình này đã có người ở vai trò này')

/** `ThemThanhVien` trả đúng câu này khi người được chọn đã chuyển xứ đi — cần hỏi 3 lựa chọn
 * Yes/No/Cancel (`muonChuyenVeXu`) trước khi gửi lại. */
export const canQuyetDinhChuyenXu = (canhBao: string[]): boolean =>
  canhBao.length === 1 && canhBao[0].includes('đã chuyển đi xứ khác')
