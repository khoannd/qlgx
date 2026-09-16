import type { TinhTrangSaoLuu } from '../api/types'

export type DenSaoLuu = 'xanh' | 'vang' | 'do'

/** Quá mốc này chưa có bản sao lưu mới → vàng. Lịch tự động là 6 giờ một lần, cộng dư 2 giờ. */
const GIO_SAO_LUU_TRE_VANG = 8
/** Quá mốc này chưa có bản sao lưu mới → ĐỎ. Một ngày trọn vẹn không sao lưu được nghĩa là
 * đường ống đã hỏng thật, không còn là chậm trễ nhất thời (spec mục 5.3 xếp việc này là đỏ). */
const GIO_SAO_LUU_TRE_DO = 24
/** Diễn tập phục hồi tự động chạy mỗi Chủ nhật. Quá hai tuần không có lần nào → vàng. */
const NGAY_DIEN_TAP_CU_VANG = 14
/** Quá một tháng không diễn tập → đỏ: lúc này "có bản sao lưu" không còn đồng nghĩa với
 * "phục hồi được", mà đó mới là điều thật sự cần biết. */
const NGAY_DIEN_TAP_CU_DO = 30

const MUC_DO: Record<DenSaoLuu, number> = { xanh: 0, vang: 1, do: 2 }

function nangHon(a: DenSaoLuu, b: DenSaoLuu): DenSaoLuu {
  return MUC_DO[b] > MUC_DO[a] ? b : a
}

function soGio(iso: string | null, bayGio: Date): number | null {
  if (!iso) return null
  const t = new Date(iso).getTime()
  if (Number.isNaN(t)) return null
  return (bayGio.getTime() - t) / 3_600_000
}

/**
 * Đèn THẬT SỰ hiển thị cho người dùng — có thể nặng hơn đèn máy chủ trả về.
 *
 * Vì sao phải leo thang thêm ở giao diện (I2/I3 của review-frontend.md):
 *
 * - `SaoLuuService.TinhDen` phía máy chủ chỉ xét `DienTapDat`, **không xét `DienTapGanNhat` cũ
 *   tới mức nào**. Nếu `qlgx-verify.timer` bị tắt từ tháng 3 thì giao diện vẫn báo "Bình thường —
 *   Diễn tập phục hồi gần nhất: 08/03/2026 — Đạt" suốt nhiều tháng. Người không rành máy tính
 *   đọc chữ "Đạt", không đọc ngày.
 * - Một bản sao lưu trễ 8 giờ (vàng) và trễ ba ngày là hai chuyện hoàn toàn khác nhau, nhưng máy
 *   chủ trả về cùng một màu.
 *
 * Hàm thuần, nhận `bayGio` làm tham số để test được mà không phải giả lập đồng hồ.
 *
 * KHÔNG leo thang khi `dienTapGanNhat` là `null`: máy mới cài chưa tới Chủ nhật đầu tiên thì
 * chưa có lần diễn tập nào — báo đỏ ngay là báo động giả, và một cảnh báo bị bỏ qua thì tệ hơn
 * không có cảnh báo. Trường hợp "chưa từng sao lưu lần nào" đã do máy chủ báo đỏ.
 */
export function tinhDenHieuLuc(tinhTrang: TinhTrangSaoLuu, bayGio: Date = new Date()): DenSaoLuu {
  let den: DenSaoLuu = tinhTrang.den

  const gioTuSaoLuu = soGio(tinhTrang.saoLuuGanNhat, bayGio)
  if (gioTuSaoLuu !== null) {
    if (gioTuSaoLuu >= GIO_SAO_LUU_TRE_DO) den = nangHon(den, 'do')
    else if (gioTuSaoLuu >= GIO_SAO_LUU_TRE_VANG) den = nangHon(den, 'vang')
  }

  const gioTuDienTap = soGio(tinhTrang.dienTapGanNhat, bayGio)
  if (gioTuDienTap !== null) {
    if (gioTuDienTap >= NGAY_DIEN_TAP_CU_DO * 24) den = nangHon(den, 'do')
    else if (gioTuDienTap >= NGAY_DIEN_TAP_CU_VANG * 24) den = nangHon(den, 'vang')
  }

  return den
}

/** Nhãn, màu và chấm màu của đèn. Vàng PHẢI khác xanh bằng cả màu lẫn chấm — bản cũ chỉ đặt màu
 * cho đèn đỏ, nên vàng trông y hệt xanh và người dùng không nhận ra khác biệt (I2). */
export function nhanDen(den: DenSaoLuu): { chu: string; mau: string; cham: string } {
  switch (den) {
    case 'do':
      return { chu: 'Sao lưu đang có vấn đề', mau: 'var(--rose-ink, #a1263f)', cham: '🔴' }
    case 'vang':
      return { chu: 'Sao lưu đang chậm trễ', mau: 'var(--amber-ink, #8a5a00)', cham: '🟡' }
    default:
      return { chu: 'Bình thường', mau: 'var(--green-ink, #1d6b45)', cham: '🟢' }
  }
}

/**
 * Câu giải thích lý do đèn không xanh, viết cho quý cha/quý sơ.
 *
 * `tinhTrang.loiGanNhat` đi thẳng từ `qlgx-runner.sh` lên màn hình, và nội dung thật là tiếng
 * Việt KHÔNG DẤU lẫn thuật ngữ kỹ thuật: "restic check that bai", "sao luu that bai (lenh cuoi
 * cung tra ve loi...)", "Dien tap phuc hoi that bai" (I4). Với người dùng của phần mềm này,
 * "restic" là một từ vô nghĩa.
 *
 * Ánh xạ ở đây dựa trên chuỗi máy chủ ghi ra. Cách đúng về lâu dài là runner ghi thêm một MÃ LỖI
 * (`kiem_tra_that_bai`/`sao_luu_that_bai`/`dien_tap_that_bai`) và DTO trả mã đó — lúc này không
 * sửa được vì `WebApp/scripts/` và các dự án `WebApp/src/Qlgx.*` do phiên khác phụ trách. Vì vậy
 * hàm phải
 * có câu mặc định tử tế cho MỌI chuỗi lạ, không được im lặng.
 */
export function nhanLoiSaoLuu(loiGanNhat: string | null | undefined): string | null {
  if (!loiGanNhat) return null
  const t = loiGanNhat.toLowerCase()
  if (t.includes('check')) {
    return 'Kho sao lưu bị lỗi khi kiểm tra tính toàn vẹn — bản sao có thể không dùng lại được. '
         + 'Hãy báo người kỹ thuật hỗ trợ.'
  }
  if (t.includes('dien tap') || t.includes('diễn tập')) {
    return 'Lần diễn tập phục hồi gần nhất KHÔNG thành công — chưa chắc phục hồi được khi cần. '
         + 'Hãy báo người kỹ thuật hỗ trợ.'
  }
  if (t.includes('sao luu') || t.includes('sao lưu') || t.includes('backup')) {
    return 'Lần sao lưu gần nhất thất bại — dữ liệu nhập gần đây có thể chưa được sao lưu. '
         + 'Hãy báo người kỹ thuật hỗ trợ.'
  }
  return 'Hệ thống sao lưu đang gặp trục trặc. Hãy báo người kỹ thuật hỗ trợ và gửi kèm phần '
       + '"Thông tin cho người kỹ thuật" bên dưới.'
}

/**
 * "(2 giờ trước)" — mockup spec 8.2. Đi KÈM ngày giờ đầy đủ chứ không thay thế: người dùng cần
 * con số tuyệt đối để đối chiếu sổ sách, còn thời gian tương đối là để nhận ra ngay "đã ba ngày
 * chưa sao lưu" mà không phải tự tính.
 */
export function thoiGianTuongDoi(iso: string | null | undefined, bayGio: Date = new Date()): string {
  const gio = soGio(iso ?? null, bayGio)
  if (gio === null || gio < 0) return ''
  if (gio < 1) {
    const phut = Math.floor(gio * 60)
    return phut <= 1 ? 'vừa xong' : `${phut} phút trước`
  }
  if (gio < 48) return `${Math.floor(gio)} giờ trước`
  return `${Math.floor(gio / 24)} ngày trước`
}
