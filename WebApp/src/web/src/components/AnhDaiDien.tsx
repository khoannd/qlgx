import { useEffect, useLayoutEffect, useRef, useState } from 'react'

type Props = {
  /** `null` = bản ghi chưa lưu (chưa có id) — tắt hẳn khả năng tải/xoá ảnh, cùng cách các tab
   * phụ khác (Hôn phối/Tận hiến/Hội đoàn) chỉ dùng được sau khi đã lưu lần đầu. */
  id: string | null
  onLayAnh?: (id: string) => Promise<string | null>
  onTaiLen?: (id: string, tep: File) => Promise<void>
  onXoa?: (id: string) => Promise<void>
  /** Nhãn dòng hai khi chưa có ảnh — "ảnh 3x4" cho giáo dân, để trống dùng chung một câu cho
   * gia đình. */
  nhan?: string
  /** true: tự đo chiều cao khung ảnh SAU KHI `align-items: stretch` của `.canhan-top`
   * (GiaoDanDetail) đã giãn xong, rồi đặt bề rộng = cao × 0,75 qua `ResizeObserver` — ép đúng
   * tỉ lệ thẻ 3×4 (rộng:cao = 3:4). KHÔNG dùng CSS `aspect-ratio` thuần cho việc này: đã thử
   * trên trình duyệt thật (Chromium), flex row với `align-items: stretch` cho ra tỉ lệ ~0,72
   * thay vì đúng 0,75 (bề rộng được tính TRƯỚC khi bề cao chốt xong theo sibling, không phải
   * SAU — thứ tự ngược với điều `aspect-ratio` cần). Đo bằng JS sau khi bề cao đã chốt tránh
   * hẳn vòng lặp phụ thuộc đó. CHỈ bật ở đây (GiaoDanDetail) qua prop này — mặc định `false` để
   * không đổi hành vi khung ảnh vuông ở GiaDinhDetail (`.col-stack > .card > .photo-slot`). */
  tiLe34?: boolean
}

/** Ô "Ảnh đại diện" dùng chung cho màn hình chi tiết giáo dân VÀ gia đình (Task 1.2
 * VIEC-TIEP-THEO.md — khung ảnh trước đây bấm vào không làm gì, xem can-review-sau.md mục 36).
 *
 * Tự quản lý toàn bộ vòng đời của một object URL blob (tạo khi tải ảnh về, revoke khi ảnh đổi/
 * component gỡ) — nơi dùng (GiaoDanDetail/GiaDinhDetail) không cần biết gì về blob URL, chỉ
 * truyền `id` + ba hàm gọi API (đã tiêm sẵn đường dẫn đúng loại thực thể ở *DetailPage.tsx).
 */
/** Không tải/lưu/xoá được gì — chỉ dùng khi component dựng trong một bài test mock `api`
 * KHÔNG khai báo các hàm ảnh (layAnh/taiAnhLen/xoaAnh), để tránh "onLayAnh is not a function"
 * thay vì bắt MỌI bài test hiện có phải cập nhật mock riêng cho một tính năng không liên quan
 * tới điều đang kiểm thử. Màn hình thật LUÔN nối `api.giaoDan.*`/`api.giaDinh.*` thật, không
 * bao giờ rơi vào nhánh này. */
const KHONG_CO_ANH = async () => null
const KHONG_LAM_GI = async () => {}

export function AnhDaiDien({
  id, onLayAnh = KHONG_CO_ANH, onTaiLen = KHONG_LAM_GI, onXoa = KHONG_LAM_GI, nhan = 'ảnh',
  tiLe34 = false,
}: Props) {
  const [anhUrl, setAnhUrl] = useState<string | null>(null)
  const [dangTai, setDangTai] = useState(false)
  const [dangXuLy, setDangXuLy] = useState(false)
  const [loi, setLoi] = useState<string | null>(null)
  const inputRef = useRef<HTMLInputElement>(null)
  const khungRef = useRef<HTMLDivElement>(null)
  // Bề rộng tính từ bề cao đã giãn (chỉ khi `tiLe34`) — `null` = chưa đo xong, dùng CSS
  // `width: 92px` tĩnh làm chỗ dựa tạm cho tới lần đo đầu tiên (tránh nhấp nháy về 0).
  const [rongTiLe, setRongTiLe] = useState<number | null>(null)

  // `useLayoutEffect` (không phải `useEffect`) để đo VÀ áp bề rộng mới trước khi trình duyệt vẽ
  // khung hình kế tiếp — giảm nhấp nháy giữa bề rộng CSS tĩnh (92px) và bề rộng tính từ JS.
  useLayoutEffect(() => {
    if (!tiLe34 || !khungRef.current) return
    const el = khungRef.current
    const doVaDat = () => setRongTiLe(Math.round(el.getBoundingClientRect().height * 0.75))
    doVaDat()
    // jsdom (test-setup.ts) giả `ResizeObserver` bằng lớp rỗng (observe() không làm gì, không
    // bao giờ gọi callback) — an toàn: `rongTiLe` giữ nguyên giá trị đo được từ `doVaDat()` ở
    // trên, ảnh vẫn hiện đúng, chỉ không "sống" theo kích thước cửa sổ trong môi trường test.
    const quanSat = new ResizeObserver(doVaDat)
    quanSat.observe(el)
    return () => quanSat.disconnect()
  }, [tiLe34])
  // Object URL hiện tại — giữ trong ref để revoke đúng cái CŨ khi effect chạy lại/unmount, độc
  // lập với state (state có thể chưa kịp cập nhật khi cleanup chạy).
  const anhUrlHienTai = useRef<string | null>(null)

  // Revoke object URL cuối cùng khi component gỡ hẳn (không chỉ khi `id` đổi — effect bên dưới
  // đã lo phần đó qua `anhUrlHienTai.current` mỗi lần tải ảnh mới).
  useEffect(() => () => {
    if (anhUrlHienTai.current) URL.revokeObjectURL(anhUrlHienTai.current)
  }, [])

  useEffect(() => {
    if (!id) { setAnhUrl(null); return }
    let huy = false
    setDangTai(true)
    onLayAnh(id).then((url) => {
      if (huy) { if (url) URL.revokeObjectURL(url); return }
      if (anhUrlHienTai.current) URL.revokeObjectURL(anhUrlHienTai.current)
      anhUrlHienTai.current = url
      setAnhUrl(url)
    }).finally(() => { if (!huy) setDangTai(false) })
    return () => { huy = true }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [id])

  async function chonTep(tep: File | undefined) {
    if (!id || !tep) return
    setDangXuLy(true)
    setLoi(null)
    try {
      await onTaiLen(id, tep)
      const url = await onLayAnh(id)
      if (anhUrlHienTai.current) URL.revokeObjectURL(anhUrlHienTai.current)
      anhUrlHienTai.current = url
      setAnhUrl(url)
    } catch (e) {
      console.error('Không tải ảnh lên được', e)
      setLoi(e instanceof Error ? e.message : 'Tải ảnh lên thất bại, thử lại sau.')
    } finally {
      setDangXuLy(false)
      if (inputRef.current) inputRef.current.value = ''
    }
  }

  async function xoaAnh(e: React.MouseEvent) {
    e.stopPropagation()
    if (!id) return
    if (!window.confirm('Xoá ảnh đại diện này?')) return
    setDangXuLy(true)
    setLoi(null)
    try {
      await onXoa(id)
      if (anhUrlHienTai.current) URL.revokeObjectURL(anhUrlHienTai.current)
      anhUrlHienTai.current = null
      setAnhUrl(null)
    } catch (e2) {
      console.error('Không xoá được ảnh đại diện', e2)
      setLoi(e2 instanceof Error ? e2.message : 'Xoá ảnh thất bại, thử lại sau.')
    } finally {
      setDangXuLy(false)
    }
  }

  const vohieuHoa = !id
  const chuThich = dangTai ? 'Đang tải…'
    : dangXuLy ? 'Đang xử lý…'
    : vohieuHoa ? `Lưu lần đầu để thêm ${nhan}`
    : loi ?? `Chưa có hình\nNhấp để tải ${nhan} lên`

  return (
    <div
      ref={khungRef}
      className="photo-slot"
      style={{
        ...(anhUrl ? { padding: 0, overflow: 'hidden', position: 'relative', color: undefined } : undefined),
        ...(rongTiLe ? { width: rongTiLe } : undefined),
      }}
      onClick={() => { if (!vohieuHoa && !dangXuLy) inputRef.current?.click() }}
      role="button"
      aria-label={anhUrl ? `Đổi ${nhan}` : `Tải ${nhan} lên`}
      title={loi ?? undefined}
    >
      <input
        ref={inputRef}
        type="file"
        accept="image/jpeg,image/png,image/webp"
        style={{ display: 'none' }}
        disabled={vohieuHoa || dangXuLy}
        onChange={(e) => { void chonTep(e.target.files?.[0]) }}
      />
      {anhUrl ? (
        <>
          <img src={anhUrl} alt={`Ảnh đại diện`} style={{ width: '100%', height: '100%', objectFit: 'cover', display: 'block' }} />
          <button
            type="button"
            className="btn btn-sm btn-quiet"
            onClick={xoaAnh}
            disabled={dangXuLy}
            style={{ position: 'absolute', top: 4, right: 4, padding: '2px 8px', fontSize: 11 }}
          >
            Xoá
          </button>
          {/* Lỗi khi THAY một ảnh đã có (ví dụ chọn nhầm tệp không phải ảnh) — ảnh CŨ vẫn còn
              nguyên (không đè lên khi máy chủ từ chối), nhưng người dùng vẫn cần thấy lý do bị
              từ chối rõ ràng, không chỉ qua `title` (tooltip khi rê chuột, dễ bị bỏ sót). */}
          {loi && (
            // pointer-events: none — khung ảnh 3x4 nhỏ (92px ở canhan-top), một thông báo lỗi
            // dài có thể xuống nhiều dòng và phủ gần hết khung; KHÔNG được chặn mất nút "Xoá"
            // ở góc trên khi việc đó xảy ra.
            <div style={{
              position: 'absolute', left: 0, right: 0, bottom: 0, background: 'rgba(192,57,43,.92)',
              color: '#fff', fontSize: 10.5, lineHeight: 1.3, padding: '3px 5px', whiteSpace: 'pre-line',
              pointerEvents: 'none', maxHeight: '70%', overflow: 'auto',
            }}>
              {loi}
            </div>
          )}
        </>
      ) : (
        <span style={{ whiteSpace: 'pre-line', color: loi ? 'var(--danger, #c0392b)' : undefined }}>{chuThich}</span>
      )}
    </div>
  )
}
