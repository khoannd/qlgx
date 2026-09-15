import { useCallback, useEffect, useState } from 'react'
import { api } from '../api/client'
import type { CanXemLai } from '../api/types'

/**
 * Hộp "cần xem lại" — spec 9.2. Đọc TRỰC TIẾP từ máy chủ (`GET /api/can-xem-lai`), KHÔNG phải kho
 * IndexedDB cục bộ (Ruling A của Task 6 — máy chủ tự giữ bền vững mục "cần xem lại", xem
 * task-10-brief.md mục "Nguồn dữ liệu cho từng phần UI").
 *
 * Mỗi mục có hai hình dạng, tuỳ `giaTriA`/`giaTriB` có mặt hay không (xem `CanXemLaiDto` phía máy
 * chủ):
 * - CÓ cặp A/B (xung đột thật, ví dụ hai máy cùng sửa một trường trong lúc mất mạng): hiện cả hai
 *   giá trị, người dùng CHỌN một trong hai (`POST .../chon`, thân `{ chon: "A" | "B" }`).
 * - KHÔNG có cặp A/B (ví dụ `loai === "khong_luu_duoc"` — máy chủ từ chối thẳng một thao tác,
 *   không có gì để "chọn"): chỉ hiện `lyDo`, người dùng bấm "Đánh dấu đã xử lý"
 *   (`POST .../danh-dau-da-xu-ly`) để đóng mục sau khi đã đọc/xử lý thủ công.
 *
 * GIỚI HẠN PHẠM VI CỐ Ý (ghi lại cho lượt sau): `truong`/`bang` hiện RAW (tên cột/bảng kỹ thuật),
 * chưa có bảng dịch sang nhãn tiếng Việt thân thiện ("Ngày sinh", "Số điện thoại"...) — DTO máy chủ
 * cố ý không dịch (xem chú thích `CanXemLaiDto.Truong`), để dành cho màn hình này, nhưng bảng dịch
 * đó không nằm trong phạm vi thời gian của Task 10 (brief ưu tiên phần "nối dây" + thanh trạng thái
 * hơn). Không phải một lỗi bị bỏ sót — cần một Task riêng nếu muốn làm đẹp câu chữ này.
 */
export function CanXemLaiPage() {
  const [danhSach, setDanhSach] = useState<CanXemLai[] | null>(null)
  const [dangXuLy, setDangXuLy] = useState<Record<string, boolean>>({})
  const [loiTheoMuc, setLoiTheoMuc] = useState<Record<string, string>>({})
  const [loiTaiDanhSach, setLoiTaiDanhSach] = useState<string | null>(null)

  const taiLai = useCallback(async () => {
    try {
      const ds = await api.canXemLai.danhSach()
      setDanhSach(ds)
      setLoiTaiDanhSach(null)
    } catch (loi) {
      setLoiTaiDanhSach(loi instanceof Error ? loi.message : 'Không tải được danh sách.')
    }
  }, [])

  useEffect(() => {
    taiLai()
  }, [taiLai])

  async function chon(muc: CanXemLai, gia: 'A' | 'B') {
    setDangXuLy((d) => ({ ...d, [muc.id]: true }))
    setLoiTheoMuc((d) => ({ ...d, [muc.id]: '' }))
    try {
      await api.canXemLai.chon(muc.id, gia)
      await taiLai()
    } catch (loi) {
      setLoiTheoMuc((d) => ({ ...d, [muc.id]: loi instanceof Error ? loi.message : 'Không xử lý được.' }))
    } finally {
      setDangXuLy((d) => ({ ...d, [muc.id]: false }))
    }
  }

  async function danhDauDaXuLy(muc: CanXemLai) {
    setDangXuLy((d) => ({ ...d, [muc.id]: true }))
    setLoiTheoMuc((d) => ({ ...d, [muc.id]: '' }))
    try {
      await api.canXemLai.danhDauDaXuLy(muc.id)
      await taiLai()
    } catch (loi) {
      setLoiTheoMuc((d) => ({ ...d, [muc.id]: loi instanceof Error ? loi.message : 'Không xử lý được.' }))
    } finally {
      setDangXuLy((d) => ({ ...d, [muc.id]: false }))
    }
  }

  if (loiTaiDanhSach) {
    return (
      <div style={{ padding: 16 }}>
        <p role="alert">{loiTaiDanhSach}</p>
        <button type="button" onClick={taiLai}>Thử lại</button>
      </div>
    )
  }

  if (danhSach === null) return <p style={{ padding: 16 }}>Đang tải…</p>

  if (danhSach.length === 0) {
    return <p style={{ padding: 16 }}>Không có việc gì cần xem lại.</p>
  }

  return (
    <div style={{ padding: 16 }}>
      <h2>Cần xem lại ({danhSach.length})</h2>
      <ul style={{ listStyle: 'none', padding: 0 }}>
        {danhSach.map((muc) => {
          const coCapAB = muc.giaTriA !== null && muc.giaTriB !== null
          return (
            <li key={muc.id} style={{ border: '1px solid #ddd', borderRadius: 6, padding: 12, marginBottom: 8 }}>
              <p style={{ fontWeight: 600 }}>{muc.bang} — {muc.truong}</p>
              {muc.lyDo && <p>{muc.lyDo}</p>}
              {coCapAB && (
                <>
                  <p>Giá trị A: {muc.giaTriA}</p>
                  <p>Giá trị B: {muc.giaTriB}</p>
                  <p>Hiện đang dùng: {muc.giaTriDangDung}</p>
                  <button type="button" disabled={dangXuLy[muc.id]} onClick={() => chon(muc, 'A')}>
                    Giữ giá trị A
                  </button>{' '}
                  <button type="button" disabled={dangXuLy[muc.id]} onClick={() => chon(muc, 'B')}>
                    Đổi lại thành giá trị B
                  </button>
                </>
              )}
              {!coCapAB && (
                <button type="button" disabled={dangXuLy[muc.id]} onClick={() => danhDauDaXuLy(muc)}>
                  Đánh dấu đã xử lý
                </button>
              )}
              {loiTheoMuc[muc.id] && <p role="alert">{loiTheoMuc[muc.id]}</p>}
            </li>
          )
        })}
      </ul>
    </div>
  )
}
