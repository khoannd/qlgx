import { useMemo, useState } from 'react'
import { api } from '../api/client'
import type { GiaDinhListItem, GiaoHo } from '../api/types'
import { GxGiaDinhList, menuGiaDinhMacDinh } from '../components/GxGiaDinhList'
import { GxToolbar } from '../components/GxToolbar'
import { chuaHoTro } from '../lib/thongBao'

/** Sentinel hiển thị cho "Ngoài xứ" — xem cùng hằng số ở GiaoDanList.tsx. */
const NGOAI_XU = 'Ngoài xứ'

type Props = {
  rows: GiaDinhListItem[]
  /** Mở thẻ chi tiết gia đình — truyền `null` để mở bản ghi mới, đúng nút "Thêm gia đình". */
  moGiaDinh: (id: string | null) => void
  /** Danh mục Giáo họ THẬT (GET /api/giao-ho) — thay `data/giaoHoTam.ts` hard-code theo tên. */
  danhMucGiaoHo?: GiaoHo[]
  /** Xoá gia đình đang chọn — đúng 2 lựa chọn [No]/[Yes] của hộp thoại gốc (xem
   * `GiaDinhService.Xoa`): `vinhVien=false` = xoá mềm ("đưa vào lưu trữ"), `vinhVien=true` =
   * xoá vĩnh viễn. Khác giáo dân: backend không có điều kiện chặn nào ở thao tác này. */
  onXoa?: (id: string, vinhVien: boolean) => Promise<void>
  /** Tải lại toàn bộ danh sách từ máy chủ — nút "Tải lại" trên thanh công cụ. */
  onTaiLai?: () => void
}

/**
 * Danh sách gia đình — `page-head` chỉ còn tiêu đề/số đếm, thanh công cụ `GxToolbar` (Tải lại,
 * Xuất CSV, Thêm, Xóa, In danh sách) nằm ngay dưới, rồi `filters-bar` có combobox Giáo họ
 * (kèm "Tất cả"/"Ngoài xứ", tên lấy từ danh mục THẬT — `GET /api/giao-ho`, xem prop
 * `danhMucGiaoHo`) và ô tick "Chỉ xem gia đình không được thống kê", `GxGiaDinhList` bên dưới.
 * Lọc theo Giáo họ/ô tick vẫn thực hiện trên máy khách (so khớp `tenGiaoHo`, KHÔNG gọi lại API
 * theo `giaoHoId`) — chấp nhận được ở quy mô hiện tại (40 gia đình); xem gia-dinh-danh-sach.md
 * mục 10 "Ưu tiên trung bình #4" nếu cần chuyển sang lọc phía máy chủ.
 */
export function GiaDinhList({ rows, moGiaDinh, danhMucGiaoHo = [], onXoa, onTaiLai }: Props) {
  const [giaoHo, setGiaoHo] = useState('-1')
  const [chiKhongThongKe, setChiKhongThongKe] = useState(false)
  const [dongChon, setDongChon] = useState<GiaDinhListItem | null>(null)
  const [trangThaiXoa, setTrangThaiXoa] = useState<'hoi' | 'dang-xoa' | null>(null)
  const [loiXoa, setLoiXoa] = useState<string | null>(null)
  const [dangXuatExcel, setDangXuatExcel] = useState(false)

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

  const menu = useMemo(() => menuGiaDinhMacDinh((d) => moGiaDinh(d.id)), [moGiaDinh])

  // "Xuất Excel" (thay CSV cũ) — cùng lý do/thiết kế với GiaoDanList.tsx: gọi thẳng máy chủ
  // (ClosedXML) với đúng hai tham số lọc đang áp dụng trên màn hình, thay vì đọc dữ liệu qua
  // `layCsv()` của lưới. Gia đình không có công tắc "hiện cả người đã mất/chuyển xứ" nào (xem
  // `api.giaDinh.danhSach` — chỉ nhận giaoHoId/chiKhongThongKe) nên không có tham số thứ ba.
  async function xuatExcel() {
    const idGiaoHo = giaoHo === '-1' || giaoHo === '0'
      ? undefined
      : danhMucGiaoHo.find((g) => g.tenGiaoHo === giaoHo)?.id
    setDangXuatExcel(true)
    try {
      await api.giaDinh.xuatExcel(idGiaoHo, chiKhongThongKe)
    } catch (e) {
      console.error('Không xuất được Excel danh sách gia đình', e)
      window.alert(e instanceof Error ? e.message : 'Xuất Excel thất bại, thử lại sau.')
    } finally {
      setDangXuatExcel(false)
    }
  }

  return (
    <section className="page list-page">
      <div className="list-page-head">
      <div className="page-head">
        <h1>Danh sách gia đình</h1>
        <div className="spacer" />
        <span className="count-pill"><b>{rowsLoc.length}</b> gia đình</span>
      </div>

      <GxToolbar
        coDongDuocChon={!!dongChon}
        items={[
          { label: 'Tải lại', icon: 'reload', onClick: onTaiLai,
            title: 'Lấy lại dữ liệu trong chương trình và hiện lên lưới như khi chưa thực hiện tìm kiếm' },
          { label: dangXuatExcel ? 'Đang xuất…' : 'Xuất Excel', icon: 'excel',
            onClick: dangXuatExcel ? undefined : () => { void xuatExcel() },
            title: 'Xuất danh sách đang hiện trên lưới ra tệp Excel (.xlsx)' },
          '|',
          { label: 'Thêm gia đình', icon: 'plus', kind: 'primary', onClick: () => moGiaDinh(null), title: 'Thêm' },
          { label: 'Xóa gia đình', icon: 'trash', needSel: true, onClick: () => setTrangThaiXoa('hoi'),
            title: 'Loại bỏ khỏi danh sách trên lưới' },
          '>',
          { label: 'In danh sách', icon: 'print', onClick: chuaHoTro, title: 'In danh sách trên lưới' },
        ]}
      />

      {trangThaiXoa && dongChon && (
        <div className="card glass" role="alertdialog" style={{ marginBottom: 12 }}>
          <p>
            Bạn muốn xóa vĩnh viễn gia đình <b>{dongChon.tenChong || dongChon.tenVo || dongChon.tenGiaDinh || dongChon.maGiaDinhCu}</b> được chọn không?<br />
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
          <label htmlFor="gdl-giaoho">Giáo họ</label>
          <select id="gdl-giaoho" value={giaoHo} onChange={(e) => setGiaoHo(e.target.value)}>
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
          Chỉ xem gia đình không được thống kê
        </label>
        <div className="spacer" />
        <span className="muted" style={{ fontSize: 12.5 }}>
          Nhấp đúp một dòng để mở chi tiết · chuột phải để xem thao tác in ấn
        </span>
      </div>
      </div>

      <GxGiaDinhList rows={rowsLoc} onMo={(d) => moGiaDinh(d.id)} onChon={setDongChon} menuChuotPhai={menu} />
    </section>
  )
}
