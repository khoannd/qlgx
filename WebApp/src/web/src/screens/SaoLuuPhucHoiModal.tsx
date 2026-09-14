import { useState } from 'react'
import type { BanSaoLuu } from '../api/types'
import { dinhDangNgayGio } from '../lib/ngay'

/** Phải khớp NGUYÊN VĂN hằng SaoLuuService.ChuoiXacNhanPhucHoi phía máy chủ. Máy chủ kiểm lại
 * chuỗi này một lần nữa — giao diện chỉ là lớp rào thứ ba, không phải lớp bảo vệ duy nhất. */
export const CHUOI_XAC_NHAN_PHUC_HOI = 'PHUC HOI TOAN BO'

type Props = {
  banSao: BanSaoLuu
  soGiaoDanHienTai: number
  soGiaDinhHienTai: number
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
  const dungChuoi = goVao === CHUOI_XAC_NHAN_PHUC_HOI
  const giamGiaoDan = banSao.soGiaoDan < soGiaoDanHienTai
  const giamGiaDinh = banSao.soGiaDinh < soGiaDinhHienTai

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

        <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: 12.5 }}>
          <thead>
            <tr style={{ textAlign: 'left', borderBottom: '1px solid rgba(14,32,76,.13)' }}>
              <th /><th>Hiện tại</th><th>Sau khi phục hồi</th>
            </tr>
          </thead>
          <tbody>
            <tr {...(giamGiaoDan ? { 'data-testid': 'canh-bao-giam-giao-dan' } : {})}
              style={{ color: giamGiaoDan ? 'var(--rose-ink)' : undefined }}>
              <td>Giáo dân</td><td>{soGiaoDanHienTai}</td><td>{banSao.soGiaoDan}</td>
            </tr>
            <tr {...(giamGiaDinh ? { 'data-testid': 'canh-bao-giam-gia-dinh' } : {})}
              style={{ color: giamGiaDinh ? 'var(--rose-ink)' : undefined }}>
              <td>Gia đình</td><td>{soGiaDinhHienTai}</td><td>{banSao.soGiaDinh}</td>
            </tr>
          </tbody>
        </table>

        <label className="field" style={{ flexDirection: 'column', alignItems: 'stretch' }}>
          Để xác nhận, hãy gõ đúng: <code>{CHUOI_XAC_NHAN_PHUC_HOI}</code>
          <input value={goVao} onChange={(e) => setGoVao(e.target.value)} autoComplete="off" />
        </label>

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
