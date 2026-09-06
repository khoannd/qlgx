import { VAI_TRO } from './vaiTroGiaDinh'

/** Một dòng "thành viên gia đình" tối giản — đủ để cây quyết định tra cứu, không cần toàn bộ
 * `ThanhVien` (tên/ngày sinh…). */
export type ThanhVienNhe = { giaoDanId: string; vaiTro: number }

export type YDinhNguoiCu = { xoa: true } | { xoa: false; vaiTroMoi: number }

export type KetQuaCayQuyetDinhNguoiCu = {
  /** Ý định cuối cho CHÍNH người cũ — gửi thẳng vào `XuLyNguoiCuDto` của
   * `PUT /api/gia-dinh/{id}/vo-chong/{vaiTro}`. */
  nguoiCu: YDinhNguoiCu
  /** Đổi vai trò HÀNG LOẠT cho các thành viên KHÁC đã có trong lưới (không phải người cũ) —
   * 'ongBa': Cha(4)->Ông(6), Mẹ(5)->Bà(7), mọi vai trò khác (trừ Ông/Bà) -> Chưa rõ(100).
   * 'chuaRo': TẤT CẢ -> Chưa rõ(100). `undefined` = không đổi ai khác. Máy chủ không có khái
   * niệm này (chỉ nhận ý định cho một người cũ) — nơi gọi (container) tự thực hiện bằng các
   * lệnh xoá+thêm riêng cho từng người bị ảnh hưởng, xem `keHoachDoiHangLoat`. */
  doiHangLoat?: 'ongBa' | 'chuaRo'
}

const coVaiTro = (ds: ThanhVienNhe[], id: string, vaiTro: number) =>
  ds.some((t) => t.giaoDanId === id && t.vaiTro === vaiTro)

/**
 * Tái hiện y hệt hàm `NguoiCu` (`Source/GXControl/frmGiaDinh.cs:649-919`) — cây quyết định hỏi
 * liên tiếp khi đổi (hoặc bỏ chọn) Người nam/Người nữ mà vai trò đó ĐANG có người khác. Xem
 * docs/superpowers/specs/man-hinh/gia-dinh-chi-tiet.md mục 4 và can-review-sau.md mục 3.
 *
 * Cố ý giữ nguyên các nhánh có vẻ "vô lý" của bản gốc (ví dụ kiểm tra vai trò VAITRO_CON trên
 * chính ID của người-còn-lại thay vì hỏi tổng quát "gia đình có con chưa") — đây là quyết định
 * chi phối "migrate y hệt", xem can-review-sau.md mục đầu file.
 *
 * @param thamSo.vaiTroDangDoi 0 = đang đổi/bỏ Người nam (Chồng), 1 = Người nữ (Vợ).
 * @param thamSo.tenNguoiCu Tên hiển thị của người đang bị thay/gỡ — chèn nguyên văn vào câu hỏi.
 * @param thamSo.idNguoiConLai Id người còn lại trong hai vai trò Chồng/Vợ (vợ nếu đang đổi
 *   chồng, ngược lại), `null` nếu gia đình chưa có ai ở vai trò đó.
 * @param thamSo.idNguoiMoi Id người MỚI được chọn thay thế, `null` nếu là "bỏ chọn" (nút X,
 *   không thay bằng ai).
 * @param thamSo.thanhVien Toàn bộ thành viên HIỆN CÓ của gia đình (kể cả Chồng/Vợ hiện tại),
 *   lấy từ `GET /api/gia-dinh/{id}` TRƯỚC khi đổi — tương đương `tbltvgd` của bản gốc.
 * @param hoi Hộp thoại Yes/No trong ứng dụng (xem `useHoiDap`) — `Promise<boolean>`, `true` =
 *   Yes. KHÔNG dùng `window.confirm`: chuỗi nhiều bước liên tiếp khó đọc/khó kiểm thử qua đó.
 */
export async function chayCayQuyetDinhNguoiCu(
  thamSo: {
    vaiTroDangDoi: 0 | 1
    tenNguoiCu: string
    idNguoiConLai: string | null
    idNguoiMoi: string | null
    thanhVien: ThanhVienNhe[]
  },
  hoi: (thongDiep: string) => Promise<boolean>,
): Promise<KetQuaCayQuyetDinhNguoiCu> {
  const { vaiTroDangDoi, tenNguoiCu, idNguoiConLai, idNguoiMoi, thanhVien } = thamSo

  // dòng 687-690: câu hỏi đầu tiên, luôn hỏi khi vai trò đang có người (điều kiện gọi hàm này).
  const xoaHan = await hoi(
    `Bạn có muốn xóa giáo dân ${tenNguoiCu} ra khỏi gia đình không??.\r\n` +
      'Nếu có chọn [Yes] để xóa.\r\nNếu không chọn [No] để chuyển người này xuống làm thành viên gia đình.',
  )
  if (xoaHan) return { nguoiCu: { xoa: true } }

  const laDoiChong = vaiTroDangDoi === VAI_TRO.CHONG
  const vaiTroChaMe = laDoiChong ? VAI_TRO.CHA : VAI_TRO.ME
  const tenChaMe = laDoiChong ? 'cha' : 'mẹ'
  // dòng 700 (nhánh Chồng) / 827 (nhánh Vợ): vai trò MONG ĐỢI của người còn lại trong CHÍNH vai
  // trò của họ (Vợ khi đang đổi Chồng, Chồng khi đang đổi Vợ).
  const vaiTroMongDoiConLai = laDoiChong ? VAI_TRO.VO : VAI_TRO.CHONG

  if (idNguoiConLai) {
    if (idNguoiMoi === null) {
      // dòng 698-716 (Chồng) / 807-825 (Vợ): bỏ chọn hẳn (X), còn người kia.
      if (coVaiTro(thanhVien, idNguoiConLai, VAI_TRO.CON)) {
        const yes = await hoi(
          'Bạn có muốn chương trình tự động cập nhập lại vai trò của ' +
            `${tenNguoiCu} thành ${tenChaMe} của gia đình không?\r\n` +
            'Nếu có chọn [Yes].\r\nNếu không chọn [No].',
        )
        if (yes) return { nguoiCu: { xoa: false, vaiTroMoi: vaiTroChaMe } }
      }
      return { nguoiCu: { xoa: false, vaiTroMoi: VAI_TRO.CHUA_RO } }
    }

    // dòng 717-778 (Chồng) / 827-891 (Vợ): có người mới thay thế, còn người kia.
    if (coVaiTro(thanhVien, idNguoiConLai, vaiTroMongDoiConLai)) {
      // người kia đang đúng vai trò của họ (trường hợp bình thường).
      if (coVaiTro(thanhVien, idNguoiMoi, VAI_TRO.CON)) {
        const yes = await hoi(
          'Bạn có muốn chương trình tự động cập nhập lại vai trò của cha mẹ thành ông bà và các ' +
            'thành viên khác là chưa rõ không?\r\nNếu có chọn [Yes].\r\nNếu không chọn [No].',
        )
        if (yes) return { nguoiCu: { xoa: false, vaiTroMoi: vaiTroChaMe }, doiHangLoat: 'ongBa' }
        return { nguoiCu: { xoa: false, vaiTroMoi: VAI_TRO.CHUA_RO } }
      }
      const yes = await hoi(
        'Bạn có muốn chương trình tự động cập nhập lại vai trò của các thành viên trong gia đình ' +
          'là chưa rõ không?\r\nNếu có chọn [Yes].\r\nNếu không chọn [No].',
      )
      if (yes) return { nguoiCu: { xoa: false, vaiTroMoi: VAI_TRO.CHUA_RO }, doiHangLoat: 'chuaRo' }
      return { nguoiCu: { xoa: false, vaiTroMoi: VAI_TRO.CHUA_RO } }
    }

    // người kia KHÔNG đúng vai trò của họ (dữ liệu bất thường — hiếm khi xảy ra với dữ liệu
    // sạch, giữ lại để đúng cấu trúc if/else của bản gốc).
    if (coVaiTro(thanhVien, idNguoiMoi, VAI_TRO.CON) || coVaiTro(thanhVien, idNguoiConLai, VAI_TRO.CON)) {
      const yes = await hoi(
        `Bạn có muốn chương trình tự động cập nhập lại vai trò của ${tenNguoiCu} thành ${tenChaMe} ` +
          'trong gia đình không?\r\nNếu có chọn [Yes].\r\nNếu không chọn [No].',
      )
      if (yes) return { nguoiCu: { xoa: false, vaiTroMoi: vaiTroChaMe } }
    }
    return { nguoiCu: { xoa: false, vaiTroMoi: VAI_TRO.CHUA_RO } }
  }

  // dòng 780-802 (Chồng) / 893-914 (Vợ): không có người kia ở vai trò đối diện.
  if (idNguoiMoi !== null && coVaiTro(thanhVien, idNguoiMoi, VAI_TRO.CON)) {
    const yes = await hoi(
      `Bạn có muốn chương trình tự động cập nhập lại vai trò của ${tenNguoiCu} thành ${tenChaMe} ` +
        'trong gia đình và các thành viên khác là chưa rõ không?\r\nNếu có chọn [Yes].\r\nNếu không chọn [No].',
    )
    if (yes) return { nguoiCu: { xoa: false, vaiTroMoi: vaiTroChaMe }, doiHangLoat: 'chuaRo' }
  }
  return { nguoiCu: { xoa: false, vaiTroMoi: VAI_TRO.CHUA_RO } }
}

/** Áp quy tắc đổi vai trò hàng loạt (dòng 728-736/840-847: 'ongBa'; dòng 752-755/865-867:
 * 'chuaRo') lên MỘT vai trò hiện có. */
export function apDungDoiHangLoat(vaiTroHienTai: number, kieu: 'ongBa' | 'chuaRo'): number {
  if (kieu === 'chuaRo') return VAI_TRO.CHUA_RO
  if (vaiTroHienTai === VAI_TRO.CHA) return VAI_TRO.ONG
  if (vaiTroHienTai === VAI_TRO.ME) return VAI_TRO.BA
  if (vaiTroHienTai === VAI_TRO.ONG || vaiTroHienTai === VAI_TRO.BA) return vaiTroHienTai
  return VAI_TRO.CHUA_RO
}

/** Chỉ những thành viên THỰC SỰ đổi vai trò (bỏ qua no-op) — nơi gọi (container) áp dụng bằng
 * xoá+thêm lại từng người (không có endpoint "sửa vai trò tại chỗ", xem báo cáo backend). */
export function keHoachDoiHangLoat(
  thanhVienKhac: ThanhVienNhe[],
  kieu: 'ongBa' | 'chuaRo',
): { giaoDanId: string; vaiTroMoi: number }[] {
  return thanhVienKhac
    .map((t) => ({ giaoDanId: t.giaoDanId, vaiTroCu: t.vaiTro, vaiTroMoi: apDungDoiHangLoat(t.vaiTro, kieu) }))
    .filter((t) => t.vaiTroMoi !== t.vaiTroCu)
    .map(({ giaoDanId, vaiTroMoi }) => ({ giaoDanId, vaiTroMoi }))
}
