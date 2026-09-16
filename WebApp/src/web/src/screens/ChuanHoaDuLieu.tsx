import { useState } from 'react'
import type { ChuanHoaXemTruoc } from '../api/types'

type Props = {
  nhan: string
  moTaXacNhan: string
  goiXemTruoc: () => Promise<ChuanHoaXemTruoc>
  goiGhiThat: () => Promise<{ soBanGhiDaDoi: number }>
  /** Báo cho nơi gọi biết đang xem trước/đang ghi hàng loạt — `ChuanHoaDuLieuPage` dùng để KHOÁ
   * hai nút chuyển tab "Giáo dân"/"Gia đình" trong lúc này. Trước đây không có cơ chế này: bấm
   * sang tab kia lúc lệnh ghi (có thể tới cả nghìn bản ghi) đang chạy → `key` đổi → component
   * bị unmount → `setThongBaoXong(...)` gọi trên component đã gỡ, im lặng không hiện gì dù việc
   * ghi đã xong ở máy chủ — người dùng tưởng thất bại rồi chạy lại (rà lại theo yêu cầu người
   * dùng 2026-09-08, review toàn nhánh "Trung bình #1"). */
  onDangXuLyChange?: (dang: boolean) => void
}

/**
 * Thân dùng chung cho hai nửa "Chuẩn hoá dữ liệu" (giáo dân/gia đình) — xem
 * docs/superpowers/specs/man-hinh/cong-cu-du-lieu.md mục 5.1. CÔNG CỤ SỬA DỮ LIỆU HÀNG LOẠT:
 * bước "Xem trước" bắt buộc trước, hộp xác nhận nêu con số cụ thể, ghi thật trong MỘT
 * transaction phía máy chủ (xem ChuanHoaDuLieuService) — cùng khuôn với ChuyenHoGiaoDan/GiaDinh.
 */
export function ChuanHoaDuLieu({ nhan, moTaXacNhan, goiXemTruoc, goiGhiThat, onDangXuLyChange }: Props) {
  const [xemTruoc, setXemTruoc] = useState<ChuanHoaXemTruoc | null>(null)
  const [dangXemTruoc, setDangXemTruoc] = useState(false)
  const [dangGhi, setDangGhi] = useState(false)
  const [loi, setLoi] = useState<string | null>(null)
  const [thongBaoXong, setThongBaoXong] = useState<string | null>(null)

  async function batDauXemTruoc() {
    setLoi(null); setThongBaoXong(null); setDangXemTruoc(true); onDangXuLyChange?.(true)
    try {
      setXemTruoc(await goiXemTruoc())
    } catch (e) {
      setLoi(e instanceof Error ? e.message : 'Không xem trước được, thử lại sau.')
    } finally {
      setDangXemTruoc(false); onDangXuLyChange?.(false)
    }
  }

  async function xacNhanGhi() {
    setDangGhi(true); setLoi(null); onDangXuLyChange?.(true)
    try {
      const kq = await goiGhiThat()
      setThongBaoXong(`Đã chuẩn hoá xong! ${kq.soBanGhiDaDoi} bản ghi đã được đổi.`)
      setXemTruoc(null)
    } catch (e) {
      setLoi(e instanceof Error ? e.message : 'Chuẩn hoá thất bại, thử lại sau.')
    } finally {
      setDangGhi(false); onDangXuLyChange?.(false)
    }
  }

  return (
    <section style={{ padding: 16, display: 'flex', flexDirection: 'column', gap: 12 }}>
      <p className="muted" style={{ margin: 0, fontSize: 12.5 }}>
        Viết hoa chữ cái đầu tiên mỗi từ, các ký tự khác trong từ chuyển thành chữ thường, áp dụng
        cho tất cả dữ liệu được nhập cho {nhan}, trừ các ghi chú. Áp dụng cho TOÀN BỘ {nhan}
        của giáo xứ (kể cả bản ghi đã xoá mềm) — đúng phạm vi bản gốc.
      </p>

      {loi && <p role="alert" style={{ color: 'var(--rose-ink)', margin: 0 }}>{loi}</p>}
      {thongBaoXong && <p style={{ color: 'var(--mint-ink)', margin: 0 }}>{thongBaoXong}</p>}

      {!xemTruoc && (
        <div>
          <button type="button" className="btn btn-primary" disabled={dangXemTruoc}
            onClick={() => { void batDauXemTruoc() }}>
            {dangXemTruoc ? 'Đang xem trước…' : 'Xem trước'}
          </button>
        </div>
      )}

      {xemTruoc && (
        <div style={{ display: 'flex', flexDirection: 'column', gap: 10 }}>
          <p style={{ margin: 0 }}>
            Đã kiểm tra <b>{xemTruoc.tongSoBanGhiKiemTra}</b> {nhan}. Sẽ có{' '}
            <b>{xemTruoc.soBanGhiSeDoi}</b> bản ghi bị thay đổi. {moTaXacNhan}
          </p>

          {xemTruoc.mauThayDoi.length > 0 && (
            <div style={{ overflowX: 'auto' }}>
              <table style={{ borderCollapse: 'collapse', fontSize: 12, width: '100%' }}>
                <thead>
                  <tr style={{ textAlign: 'left', borderBottom: '1px solid rgba(14,32,76,.13)' }}>
                    <th>Bản ghi</th><th>Trường</th><th>Giá trị cũ</th><th>Giá trị mới</th>
                  </tr>
                </thead>
                <tbody>
                  {xemTruoc.mauThayDoi.flatMap((dong) =>
                    dong.truong.map((t, i) => (
                      <tr key={`${dong.id}-${t.tenTruong}`} style={{ borderBottom: '1px solid rgba(14,32,76,.06)' }}>
                        <td>{i === 0 ? dong.nhanDien : ''}</td>
                        <td>{t.tenTruong}</td>
                        <td>{t.giaTriCu}</td>
                        <td>{t.giaTriMoi}</td>
                      </tr>
                    )),
                  )}
                </tbody>
              </table>
              {xemTruoc.soBanGhiSeDoi > xemTruoc.mauThayDoi.length && (
                <p className="muted" style={{ fontSize: 12, margin: '4px 0 0' }}>
                  Chỉ hiện {xemTruoc.mauThayDoi.length} bản ghi đầu tiên trong tổng số {xemTruoc.soBanGhiSeDoi} bản ghi sẽ đổi.
                </p>
              )}
            </div>
          )}

          <div style={{ display: 'flex', gap: 8 }}>
            <button type="button" className="btn" onClick={() => setXemTruoc(null)} disabled={dangGhi}>Huỷ</button>
            <button type="button" className="btn btn-primary" disabled={dangGhi || xemTruoc.soBanGhiSeDoi === 0}
              onClick={() => { void xacNhanGhi() }}>
              {dangGhi ? 'Đang chuẩn hoá…' : `Xác nhận chuẩn hoá ${xemTruoc.soBanGhiSeDoi} bản ghi`}
            </button>
          </div>
        </div>
      )}
    </section>
  )
}
