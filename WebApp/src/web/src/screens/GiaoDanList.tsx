import { useMemo, useState } from 'react'
import type { GiaoDanListItem, GiaoHo } from '../api/types'
import { GxGiaoDanList, menuGiaoDanMacDinh } from '../components/GxGiaoDanList'

/** Sentinel hiển thị cho "Ngoài xứ" — đúng quy ước `MaGiaoHo = 0` của bản desktop; ở bản web
 * ứng với `tenGiaoHo === "Ngoài xứ"` (xem GiaoDanService.DungDanhSach). */
const NGOAI_XU = 'Ngoài xứ'

type Props = {
  rows: GiaoDanListItem[]
  /** Danh mục Giáo họ THẬT (GET /api/giao-ho) — thay `data/giaoHoTam.ts` hard-code theo tên. */
  danhMucGiaoHo?: GiaoHo[]
  /** Mở thẻ chi tiết giáo dân — truyền `null` để mở bản ghi mới, đúng nút "Thêm giáo dân". */
  moGiaoDan: (id: string | null) => void
  /** Mục "Xem gia đình" trên menu chuột phải của lưới mở thẳng thẻ chi tiết gia đình. */
  moGiaDinh?: (id: string) => void
  /** Xem giao-dan-danh-sach.md mục 10 — bản desktop không có công tắc này ngay trong màn hình
   * (chỉ thấy được người đã mất/chuyển xứ qua các màn hình Tìm kiếm riêng, ngoài phạm vi migrate). */
  hienCaDaMat?: boolean
  onDoiHienCaDaMat?: (v: boolean) => void
  /** Xoá giáo dân đang chọn — đúng 2 lựa chọn [No]/[Yes] của hộp thoại gốc
   * (gxAddEdit1_DeleteClick, frmGiaoDanList.cs:223-286): `vinhVien=false` = xoá mềm ("đưa vào
   * hồ sơ lưu trữ"), `vinhVien=true` = xoá vĩnh viễn (có thể bị máy chủ chặn nếu đang thuộc
   * gia đình). */
  onXoa?: (id: string, vinhVien: boolean) => Promise<void>
}

/**
 * Danh sách giáo dân — cùng bố cục `page-head`/`filters-bar` với `GiaDinhList`, chỉ khác ô
 * tick là "Chỉ xem giáo dân không được thống kê" và lưới là `GxGiaoDanList`. Lọc theo Giáo họ
 * vẫn thực hiện trên máy khách (so khớp `tenGiaoHo`, không gọi lại API) dù danh mục Giáo họ
 * nay đã thật (`GET /api/giao-ho`, xem prop `danhMucGiaoHo`) — chấp nhận được ở quy mô hiện
 * tại (2050 giáo dân). Ô tick lọc theo `khongThongKe` — KHÔNG suy ra từ
 * `tenGiaoHo === "Ngoài xứ"`: hai khái niệm này khác nhau (một giáo dân ngoài xứ vẫn có thể
 * được thống kê, và ngược lại).
 */
export function GiaoDanList({
  rows, moGiaoDan, moGiaDinh, hienCaDaMat = false, onDoiHienCaDaMat, onXoa, danhMucGiaoHo = [],
}: Props) {
  const [giaoHo, setGiaoHo] = useState('-1')
  const [chiKhongThongKe, setChiKhongThongKe] = useState(false)
  const [dongChon, setDongChon] = useState<GiaoDanListItem | null>(null)
  // 'hoi' = đang hiện 3 lựa chọn Xoá vĩnh viễn/Xoá mềm/Huỷ (thay cho MessageBoxButtons.YesNoCancel
  // của desktop, xem giao-dan-danh-sach.md mục 4); 'dang-xoa' = đã bấm, chờ máy chủ trả lời.
  const [trangThaiXoa, setTrangThaiXoa] = useState<'hoi' | 'dang-xoa' | null>(null)
  const [loiXoa, setLoiXoa] = useState<string | null>(null)

  async function thucHienXoa(vinhVien: boolean) {
    if (!dongChon || !onXoa) return
    setTrangThaiXoa('dang-xoa')
    setLoiXoa(null)
    try {
      await onXoa(dongChon.id, vinhVien)
      setDongChon(null)
      setTrangThaiXoa(null)
    } catch (e) {
      setLoiXoa(e instanceof Error ? e.message : 'Xoá thất bại, thử lại sau.')
      setTrangThaiXoa('hoi')
    }
  }

  const dsGiaoHo = useMemo(() => {
    const tuDuLieu = rows.map((r) => r.tenGiaoHo).filter((t): t is string => !!t && t !== NGOAI_XU)
    const tuDanhMuc = danhMucGiaoHo.map((g) => g.tenGiaoHo)
    return Array.from(new Set([...tuDuLieu, ...tuDanhMuc])).sort((a, b) => a.localeCompare(b, 'vi'))
  }, [rows, danhMucGiaoHo])

  const rowsLoc = useMemo(
    () => rows.filter((r) => {
      if (chiKhongThongKe && !r.khongThongKe) return false
      if (giaoHo === '0') return r.tenGiaoHo === NGOAI_XU
      if (giaoHo !== '-1') return r.tenGiaoHo === giaoHo
      return true
    }),
    [rows, giaoHo, chiKhongThongKe],
  )

  // "Xem gia đình" phải tra theo `d.giaDinhId` (mã GIA ĐÌNH) — KHÔNG phải `d.id` (mã giáo
  // dân); dùng nhầm `d.id` từng khiến mục này mở ra một thẻ "Gia đình mới" trống vì không
  // tra được gia đình nào khớp. Mục menu tự ẩn khi `giaDinhId` null (xem `menuGiaoDanMacDinh`).
  const menu = useMemo(
    () => menuGiaoDanMacDinh((d) => moGiaoDan(d.id), (d) => { if (d.giaDinhId) moGiaDinh?.(d.giaDinhId) }),
    [moGiaoDan, moGiaDinh],
  )

  return (
    <section className="page list-page">
      <div className="page-head">
        <h1>Danh sách giáo dân</h1>
        <div className="spacer" />
        <span className="count-pill"><b>{rowsLoc.length}</b> giáo dân</span>
        <button type="button" className="btn" disabled={!dongChon || !onXoa} onClick={() => setTrangThaiXoa('hoi')}>
          Xóa giáo dân
        </button>
        <button type="button" className="btn btn-primary" onClick={() => moGiaoDan(null)}>
          Thêm giáo dân
        </button>
      </div>

      {trangThaiXoa && dongChon && (
        <div className="card glass" role="alertdialog" style={{ marginBottom: 12 }}>
          <p>
            Bạn muốn xóa vĩnh viễn giáo dân <b>{dongChon.hoTen}</b> được chọn không?<br />
            Chọn <b>Xóa vĩnh viễn</b> để xóa hẳn. Chọn <b>Đưa vào lưu trữ</b> để xóa mềm (khôi phục được sau).
            Chọn <b>Hủy bỏ</b> để không xóa gì cả.
          </p>
          {loiXoa && <p className="hint" role="alert">{loiXoa}</p>}
          <div className="cmdbar">
            <button type="button" className="btn" disabled={trangThaiXoa === 'dang-xoa'}
              onClick={() => { setTrangThaiXoa(null); setLoiXoa(null) }}>
              Hủy bỏ
            </button>
            <div className="spacer" />
            <button type="button" className="btn" disabled={trangThaiXoa === 'dang-xoa'}
              onClick={() => thucHienXoa(false)}>
              Đưa vào lưu trữ
            </button>
            <button type="button" className="btn btn-danger" disabled={trangThaiXoa === 'dang-xoa'}
              onClick={() => thucHienXoa(true)}>
              Xóa vĩnh viễn
            </button>
          </div>
        </div>
      )}

      <div className="filters-bar glass">
        <div className="field">
          <label htmlFor="gdanl-giaoho">Giáo họ</label>
          <select id="gdanl-giaoho" value={giaoHo} onChange={(e) => setGiaoHo(e.target.value)}>
            <option value="-1">Tất cả</option>
            <option value="0">{NGOAI_XU}</option>
            {dsGiaoHo.map((g) => <option key={g} value={g}>{g}</option>)}
          </select>
        </div>
        <label className="toggle">
          <input
            type="checkbox"
            checked={chiKhongThongKe}
            onChange={(e) => setChiKhongThongKe(e.target.checked)}
          />
          Chỉ xem giáo dân không được thống kê
        </label>
        <label className="toggle">
          <input
            type="checkbox"
            checked={hienCaDaMat}
            onChange={(e) => onDoiHienCaDaMat?.(e.target.checked)}
          />
          Hiện cả người đã qua đời / đã chuyển xứ
        </label>
        <div className="spacer" />
        <span className="muted" style={{ fontSize: 12.5 }}>
          Nhấp đúp một dòng để mở chi tiết · chuột phải để xem thao tác in ấn
        </span>
      </div>

      <GxGiaoDanList rows={rowsLoc} onMo={(d) => moGiaoDan(d.id)} onChon={setDongChon} menuChuotPhai={menu} />
    </section>
  )
}
