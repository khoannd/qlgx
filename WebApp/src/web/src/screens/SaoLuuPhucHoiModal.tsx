import { useEffect, useRef, useState } from 'react'
import type { BanSaoLuu } from '../api/types'
import { dinhDangNgayGio } from '../lib/ngay'

/** Phải khớp NGUYÊN VĂN hằng SaoLuuService.ChuoiXacNhanPhucHoi phía máy chủ. Máy chủ kiểm lại
 * chuỗi này một lần nữa — giao diện chỉ là lớp rào thứ ba, không phải lớp bảo vệ duy nhất.
 *
 * HỢP ĐỒNG HAI CHIỀU: nếu đổi chuỗi này thì PHẢI đổi cả `SaoLuuService.ChuoiXacNhanPhucHoi`
 * (`WebApp/src/Qlgx.Api/Services/SaoLuuService.cs`), và ngược lại. Hai phía có hai test riêng so
 * với literal viết lại, nên đổi một phía mà quên phía kia thì CẢ HAI bộ test vẫn xanh — chỉ
 * người dùng gặp lỗi "câu xác nhận không đúng" mà không hiểu vì sao. */
export const CHUOI_XAC_NHAN_PHUC_HOI = 'PHUC HOI TOAN BO'

type Props = {
  banSao: BanSaoLuu
  /** Số giáo dân HIỆN TẠI của toàn máy chủ, hoặc `null` khi chưa có nguồn số liệu đúng nghĩa
   * "hiện tại". `null` KHÔNG được thay bằng một con số xấp xỉ: xem ghi chú ở `BangDoiChieu`. */
  soGiaoDanHienTai: number | null
  soGiaDinhHienTai: number | null
  onDong: () => void
  onXacNhan: (snapshotId: string, xacNhan: string) => void
}

/**
 * Bốn lớp rào trước một thao tác KHÔNG THỂ HOÀN TÁC bằng giao diện:
 *   1. Chỉ tài khoản Quản trị hệ thống mở được màn hình này (kiểm ở tầng API, không chỉ ẩn nút)
 *   2. Chọn bản sao có ngữ cảnh — thời điểm và số bản ghi tại thời điểm đó
 *   3. Gõ tay chuỗi xác nhận, không dùng hộp thoại "bạn có chắc không?" (bấm OK theo phản xạ)
 *   4. Bảng đối chiếu trước–sau, tô đỏ những dòng sẽ GIẢM
 *
 * Cố ý gây khó chịu. Dữ liệu là sổ sách giáo xứ nhiều năm, mất là không lấy lại được.
 */
export function SaoLuuPhucHoiModal(
  { banSao, soGiaoDanHienTai, soGiaDinhHienTai, onDong, onXacNhan }: Props,
) {
  const [goVao, setGoVao] = useState('')
  const [baoDan, setBaoDan] = useState(false)
  const oNhapRef = useRef<HTMLInputElement | null>(null)
  const dungChuoi = goVao === CHUOI_XAC_NHAN_PHUC_HOI

  // N4: Esc đóng hộp thoại. KHÔNG đóng khi nhấp ra nền — ở đây nhấp nhầm ra ngoài rồi phải mở
  // lại, gõ lại chuỗi xác nhận là cái giá quá đắt so với lợi ích; Esc và nút "Huỷ" là đủ.
  useEffect(() => {
    const nghe = (e: KeyboardEvent) => { if (e.key === 'Escape') onDong() }
    window.addEventListener('keydown', nghe)
    oNhapRef.current?.focus()
    return () => window.removeEventListener('keydown', nghe)
  }, [onDong])

  return (
    <div role="dialog" aria-modal="true" aria-label="Xác nhận phục hồi dữ liệu"
      style={{ position: 'fixed', inset: 0, background: 'rgba(6,14,32,.45)', display: 'grid',
               placeItems: 'center', zIndex: 50 }}>
      <div className="glass" style={{ padding: 20, borderRadius: 'var(--r-card)', maxWidth: 560,
                                      display: 'flex', flexDirection: 'column', gap: 12 }}>
        <h3 style={{ margin: 0, color: 'var(--rose-ink)' }}>Phục hồi dữ liệu</h3>

        <p style={{ margin: 0, fontSize: 12.5 }}>
          Thao tác này thay thế <strong>toàn bộ máy chủ</strong> — dữ liệu của <em>mọi</em> giáo
          xứ, không riêng giáo xứ nào — bằng nội dung của bản sao lưu lúc{' '}
          <strong>{dinhDangNgayGio(banSao.thoiDiem)}</strong>. Mọi thay đổi nhập sau thời điểm
          đó sẽ mất. Hệ thống tự sao lưu trạng thái hiện tại trước khi ghi đè, và giữ cơ sở dữ
          liệu cũ thêm 7 ngày.
        </p>

        {/* I1: chưa có màn hình chặn toàn trang cho MỌI người dùng (xem TRIEN-KHAI.md mục 14 —
            khoản nợ đã biết). Tối thiểu phải nói thẳng cho người sắp bấm rằng họ đang làm gián
            đoạn công việc của các giáo xứ khác. */}
        <p style={{ margin: 0, fontSize: 12.5, color: 'var(--rose-ink)' }}>
          Trong vài phút tới, <strong>mọi người đang dùng hệ thống phải ngừng nhập liệu</strong> —
          phần họ nhập trong lúc phục hồi sẽ mất và không có cảnh báo nào hiện ra trên máy họ.
          Hãy báo các giáo xứ trước khi bấm.
        </p>

        <BangDoiChieu banSao={banSao}
          soGiaoDanHienTai={soGiaoDanHienTai} soGiaDinhHienTai={soGiaDinhHienTai} />

        <label className="field" style={{ flexDirection: 'column', alignItems: 'stretch' }}>
          Để xác nhận, hãy gõ đúng: <code>{CHUOI_XAC_NHAN_PHUC_HOI}</code>
          <input ref={oNhapRef} value={goVao} onChange={(e) => setGoVao(e.target.value)}
            autoComplete="off"
            // N3: chặn DÁN. Cả lớp rào 3 nằm ở chỗ buộc người dùng phải chậm lại và gõ từng chữ;
            // chuỗi lại hiện ngay phía trên, bôi đen + Ctrl+C + Ctrl+V mất 2 giây là xong. Vẫn
            // hiện chuỗi để người dùng biết phải gõ gì — chặn dán là đủ.
            onPaste={(e) => { e.preventDefault(); setBaoDan(true) }} />
        </label>
        {baoDan && (
          <div role="alert" style={{ fontSize: 12, color: 'var(--rose-ink)' }}>
            Hãy gõ tay câu xác nhận, không dán — đây là bước cố ý làm chậm lại để chắc chắn bạn
            hiểu việc sắp làm.
          </div>
        )}

        <div style={{ display: 'flex', gap: 8, justifyContent: 'flex-end' }}>
          <button type="button" className="btn" onClick={onDong}>Huỷ</button>
          <button type="button" className="btn" disabled={!dungChuoi}
            onClick={() => onXacNhan(banSao.id, goVao)}>
            Phục hồi về {dinhDangNgayGio(banSao.thoiDiem)}
          </button>
        </div>
      </div>
    </div>
  )
}

/**
 * Lớp rào thứ 4 — bảng đối chiếu trước–sau.
 *
 * KHÔNG BAO GIỜ hiện một con số xấp xỉ dưới nhãn "Hiện tại" (C1 của review-frontend.md): bản
 * cũ lấy số của BẢN SAO MỚI NHẤT làm "hiện tại", nên khi phục hồi về chính bản mới nhất — thao
 * tác phổ biến nhất — hai cột luôn bằng nhau, không dòng nào tô đỏ, và người dùng được trấn an
 * rằng "không mất gì" ngay lúc sắp mất tới 6 giờ nhập liệu của mọi giáo xứ.
 *
 * Thà không hiện con số còn hơn hiện số sai: khi `soGiaoDanHienTai` là `null`, cột ghi rõ "chưa
 * tính được" và cảnh báo mất dữ liệu hiện LUÔN LUÔN. Muốn có số thật phải thêm
 * `SoGiaoDanHienTai`/`SoGiaDinhHienTai` vào `TinhTrangSaoLuuDto` phía máy chủ (hai câu
 * `COUNT(*)`); lúc đó truyền số thật vào đây là bảng tự hoạt động đúng, không cần sửa gì thêm.
 */
function BangDoiChieu(
  { banSao, soGiaoDanHienTai, soGiaDinhHienTai }:
  { banSao: BanSaoLuu; soGiaoDanHienTai: number | null; soGiaDinhHienTai: number | null },
) {
  const chuaBiet = soGiaoDanHienTai === null || soGiaDinhHienTai === null
  const giamGiaoDan = soGiaoDanHienTai !== null && banSao.soGiaoDan < soGiaoDanHienTai
  const giamGiaDinh = soGiaDinhHienTai !== null && banSao.soGiaDinh < soGiaDinhHienTai
  const oHienTai = (so: number | null) => (so === null ? 'chưa tính được' : String(so))

  return (
    <>
      <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: 12.5 }}>
        <thead>
          <tr style={{ textAlign: 'left', borderBottom: '1px solid rgba(14,32,76,.13)' }}>
            <th /><th>Hiện tại</th><th>Sau khi phục hồi</th>
          </tr>
        </thead>
        <tbody>
          <tr {...(giamGiaoDan ? { 'data-testid': 'canh-bao-giam-giao-dan' } : {})}
            style={{ color: giamGiaoDan ? 'var(--rose-ink)' : undefined }}>
            <td>Giáo dân</td><td>{oHienTai(soGiaoDanHienTai)}</td><td>{banSao.soGiaoDan}</td>
          </tr>
          <tr {...(giamGiaDinh ? { 'data-testid': 'canh-bao-giam-gia-dinh' } : {})}
            style={{ color: giamGiaDinh ? 'var(--rose-ink)' : undefined }}>
            <td>Gia đình</td><td>{oHienTai(soGiaDinhHienTai)}</td><td>{banSao.soGiaDinh}</td>
          </tr>
        </tbody>
      </table>

      {chuaBiet && (
        <div data-testid="chua-tinh-duoc-so-hien-tai" role="alert"
          style={{ fontSize: 12.5, color: 'var(--rose-ink)' }}>
          Hệ thống <strong>chưa tính được</strong> số liệu thật của chính lúc này, nên bảng trên
          không cho biết sẽ mất bao nhiêu. Hãy coi như <strong>mọi thay đổi nhập sau{' '}
          {dinhDangNgayGio(banSao.thoiDiem)} đều sẽ mất</strong>, ở tất cả các giáo xứ.
        </div>
      )}
    </>
  )
}
