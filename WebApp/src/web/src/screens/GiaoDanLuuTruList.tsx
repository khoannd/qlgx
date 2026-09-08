import { useMemo, useState } from 'react'
import { api } from '../api/client'
import type { GiaoDanListItem, GiaoHo } from '../api/types'
import { GioiThieuModal } from '../components/GioiThieuModal'
import { GxGiaoDanList, menuGiaoDanMacDinh } from '../components/GxGiaoDanList'
import { GxToolbar } from '../components/GxToolbar'
import { chuaHoTro } from '../lib/thongBao'
import { useGioiThieuGiaoDan } from '../lib/useGioiThieuGiaoDan'

/** Sentinel hiển thị cho "Ngoài xứ" — cùng quy ước của GiaoDanList.tsx. */
const NGOAI_XU = 'Ngoài xứ'

type Props = {
  /** Đúng danh sách trả về từ GET /api/giao-dan/luu-tru — giáo dân đã xoá mềm, HOẶC đã qua
   * đời, HOẶC đã chuyển xứ (OR — xem GxGiaoHo.LoadGridData dòng 268, IsLuuTru=true). */
  rows: GiaoDanListItem[]
  danhMucGiaoHo?: GiaoHo[]
  /** Mở thẻ chi tiết giáo dân để SỬA (nút Sửa/double-click) — bản desktop KHÔNG có nút "Thêm"
   * ở màn hình lưu trữ (`gxAddEdit1.AddButton.Visible = false`,
   * frmGiaoDanLuuTruList.cs:43) nên không có tham số mở-mới ở đây. */
  moGiaoDan: (id: string) => void
  moGiaDinh?: (id: string) => void
  /** Xóa VĨNH VIỄN giáo dân đang chọn — đúng (và DUY NHẤT) lựa chọn của
   * `gxAddEdit1_DeleteClick` (frmGiaoDanLuuTruList.cs:157-202): desktop chỉ hỏi Yes/No, không
   * có lựa chọn "xóa mềm" nào nữa (bản ghi đã ở trong hồ sơ lưu trữ). Có thể bị máy chủ chặn
   * (409) nếu giáo dân đang thuộc gia đình nào — đúng `checkGiaoDanTrongGiaDinh`. */
  onXoa?: (id: string) => Promise<void>
  onTaiLai?: () => void
}

/**
 * Hồ sơ lưu trữ giáo dân — biến thể của `GiaoDanList` (xem đó để biết khuôn chung), khác ở ba
 * điểm đúng như `frmGiaoDanLuuTruList.cs`: (1) nguồn dữ liệu là bản ghi ĐÃ lưu trữ (xem prop
 * `rows`), (2) không có nút Thêm, (3) nút Xóa chỉ có một hành động — xóa vĩnh viễn, không có
 * lựa chọn xóa mềm/lưu trữ nào khác (đã ở trong hồ sơ lưu trữ rồi). Lưới, cột, và menu chuột
 * phải (12 mục) dùng lại NGUYÊN VẸN `GxGiaoDanList`/`menuGiaoDanMacDinh` — cùng UserControl
 * `GxGiaoDanList` mà bản desktop dùng ở cả hai màn hình.
 */
export function GiaoDanLuuTruList({
  rows, moGiaoDan, moGiaDinh, onXoa, danhMucGiaoHo = [], onTaiLai,
}: Props) {
  const [giaoHo, setGiaoHo] = useState('-1')
  const [chiKhongThongKe, setChiKhongThongKe] = useState(false)
  const [dongChon, setDongChon] = useState<GiaoDanListItem | null>(null)
  const [dangHoiXoa, setDangHoiXoa] = useState(false)
  const [dangXoa, setDangXoa] = useState(false)
  const [loiXoa, setLoiXoa] = useState<string | null>(null)
  const [dangXuatExcel, setDangXuatExcel] = useState(false)

  async function xacNhanXoa() {
    if (!dongChon || !onXoa) return
    setDangXoa(true)
    setLoiXoa(null)
    try {
      await onXoa(dongChon.id)
      setDongChon(null)
      setDangHoiXoa(false)
    } catch (e) {
      setLoiXoa(e instanceof Error ? e.message : 'Xoá thất bại, thử lại sau.')
    } finally {
      setDangXoa(false)
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

  const gioiThieu = useGioiThieuGiaoDan()
  const menu = useMemo(
    () => menuGiaoDanMacDinh((d) => moGiaoDan(d.id), (d) => { if (d.giaDinhId) moGiaDinh?.(d.giaDinhId) }, gioiThieu.mo),
    [moGiaoDan, moGiaDinh, gioiThieu.mo],
  )

  async function xuatExcel() {
    const idGiaoHo = giaoHo === '-1' || giaoHo === '0'
      ? undefined
      : danhMucGiaoHo.find((g) => g.tenGiaoHo === giaoHo)?.id
    setDangXuatExcel(true)
    try {
      await api.giaoDan.xuatExcelLuuTru(idGiaoHo, chiKhongThongKe)
    } catch (e) {
      console.error('Không xuất được Excel hồ sơ lưu trữ giáo dân', e)
      window.alert(e instanceof Error ? e.message : 'Xuất Excel thất bại, thử lại sau.')
    } finally {
      setDangXuatExcel(false)
    }
  }

  return (
    <section className="page list-page">
      <div className="list-page-head">
      <div className="page-head">
        <h1>Hồ sơ lưu trữ giáo dân</h1>
        <div className="spacer" />
        <span className="count-pill"><b>{rowsLoc.length}</b> giáo dân</span>
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
          { label: 'Xóa bỏ', icon: 'trash', needSel: true, onClick: () => setDangHoiXoa(true),
            title: 'Xóa vĩnh viễn khỏi chương trình' },
          '>',
          { label: 'In chứng nhận bí tích', needSel: true,
            onClick: () => dongChon && api.giaoDan.inChungNhanBiTich(dongChon.id)
              .catch((e: unknown) => window.alert(e instanceof Error ? e.message : 'In thất bại, thử lại sau.')),
            title: 'In chứng nhận bí tích cho giáo dân đang chọn' },
          { label: 'In giới thiệu hôn phối', needSel: true, onClick: chuaHoTro },
        ]}
      />

      {dangHoiXoa && dongChon && (
        <div className="card glass" role="alertdialog" style={{ marginBottom: 12 }}>
          <p>
            Nếu bạn chọn xóa giáo dân khỏi hồ sơ lưu trữ, giáo dân này sẽ vĩnh viễn bị xóa khỏi
            chương trình.<br />
            Bạn có chắc muốn xóa giáo dân <b>{dongChon.hoTen}</b> này?
          </p>
          {loiXoa && <p className="hint" role="alert">{loiXoa}</p>}
          <div className="cmdbar">
            <button type="button" className="btn" disabled={dangXoa}
              onClick={() => { setDangHoiXoa(false); setLoiXoa(null) }}>
              Hủy bỏ
            </button>
            <div className="spacer" />
            <button type="button" className="btn btn-danger" disabled={dangXoa}
              onClick={() => { void xacNhanXoa() }}>
              Xóa vĩnh viễn
            </button>
          </div>
        </div>
      )}

      <div className="filters-bar glass">
        <div className="field">
          <label htmlFor="gdlt-giaoho">Giáo họ</label>
          <select id="gdlt-giaoho" value={giaoHo} onChange={(e) => setGiaoHo(e.target.value)}>
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
        <div className="spacer" />
        <span className="muted" style={{ fontSize: 12.5 }}>
          Giáo dân đã xóa mềm, đã qua đời hoặc đã chuyển xứ · nhấp đúp một dòng để mở chi tiết
        </span>
      </div>
      </div>

      <GxGiaoDanList rows={rowsLoc} onMo={(d) => moGiaoDan(d.id)} onChon={setDongChon} menuChuotPhai={menu} />
      {gioiThieu.dangMo && (
        <GioiThieuModal tieuDe={gioiThieu.tieuDe} onXuat={gioiThieu.xuat} onDong={gioiThieu.dong} />
      )}
    </section>
  )
}
