import { useCallback, useState } from 'react'

type BaLuaChon = 'yes' | 'no' | 'cancel'

type TrangThaiHoi =
  | { loai: 'yesno'; thongDiep: string; giaiQuyet: (v: boolean) => void }
  | { loai: 'yesnocancel'; thongDiep: string; giaiQuyet: (v: BaLuaChon) => void }
  | { loai: 'bao'; thongDiep: string; giaiQuyet: () => void }

/**
 * Hộp thoại trong ứng dụng thay cho `window.confirm`/`window.alert` — dùng cho các chuỗi xác
 * nhận NHIỀU BƯỚC liên tiếp (cây quyết định `NguoiCu` khi đổi Người nam/Người nữ, xem
 * `lib/nguoiCu.ts`, và các cảnh báo Yes/No/Cancel khi thêm thành viên). `window.confirm` chặn
 * đồng bộ toàn bộ luồng JS nên khó kiểm thử (không `await` được, khó mock từng bước) và khó
 * đọc thông báo nhiều dòng — hộp thoại này hiện trong DOM thật, giữ nguyên xuống dòng `\r\n`
 * bằng `white-space: pre-line` (xem qlgx.css).
 *
 * Dùng qua hook `useHoiDap()`: gọi `hoi()`/`hoi3()`/`bao()`, `await` kết quả — hộp tự đóng khi
 * người dùng bấm nút. Render `<Dialog />` ở gốc màn hình gọi (ví dụ ngay trong JSX trả về của
 * `GiaDinhDetailPage`).
 */
export function useHoiDap() {
  const [trangThai, setTrangThai] = useState<TrangThaiHoi | null>(null)

  const hoi = useCallback(
    (thongDiep: string) => new Promise<boolean>((giaiQuyet) => setTrangThai({ loai: 'yesno', thongDiep, giaiQuyet })),
    [],
  )
  const hoi3 = useCallback(
    (thongDiep: string) =>
      new Promise<BaLuaChon>((giaiQuyet) => setTrangThai({ loai: 'yesnocancel', thongDiep, giaiQuyet })),
    [],
  )
  const bao = useCallback(
    (thongDiep: string) => new Promise<void>((giaiQuyet) => setTrangThai({ loai: 'bao', thongDiep, giaiQuyet })),
    [],
  )

  function dong(gia: boolean | BaLuaChon | undefined) {
    if (!trangThai) return
    ;(trangThai.giaiQuyet as (v: unknown) => void)(gia)
    setTrangThai(null)
  }

  const Dialog = trangThai && (
    <div className="hoidap-nen" role="presentation">
      <div className="hoidap-hop" role="alertdialog" aria-modal="true">
        <p className="hoidap-noidung">{trangThai.thongDiep}</p>
        <div className="hoidap-nut">
          {trangThai.loai === 'bao' && (
            <button type="button" className="btn btn-primary" onClick={() => dong(undefined)} autoFocus>
              OK
            </button>
          )}
          {trangThai.loai === 'yesno' && (
            <>
              <button type="button" className="btn btn-primary" onClick={() => dong(true)} autoFocus>
                Yes
              </button>
              <button type="button" className="btn btn-quiet" onClick={() => dong(false)}>
                No
              </button>
            </>
          )}
          {trangThai.loai === 'yesnocancel' && (
            <>
              <button type="button" className="btn btn-primary" onClick={() => dong('yes')} autoFocus>
                Yes
              </button>
              <button type="button" className="btn btn-quiet" onClick={() => dong('no')}>
                No
              </button>
              <button type="button" className="btn btn-quiet" onClick={() => dong('cancel')}>
                Cancel
              </button>
            </>
          )}
        </div>
      </div>
    </div>
  )

  return { hoi, hoi3, bao, Dialog }
}
