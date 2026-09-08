import { useMemo, useState } from 'react'
import { api } from '../api/client'
import type { GiaDinhListItem, GiaoHo } from '../api/types'
import { GioiThieuModal } from '../components/GioiThieuModal'
import { GxGiaDinhList, menuGiaDinhMacDinh } from '../components/GxGiaDinhList'
import { GxToolbar } from '../components/GxToolbar'
import { useGioiThieuChuyenXu } from '../lib/useGioiThieuChuyenXu'

/** Sentinel hiển thị cho "Ngoài xứ" — cùng quy ước của GiaDinhList.tsx. */
const NGOAI_XU = 'Ngoài xứ'

type Props = {
  /** Đúng danh sách trả về từ GET /api/gia-dinh/luu-tru — gia đình đã xoá mềm HOẶC đã chuyển
   * xứ (OR — xem GxGiaoHo.LoadGridData dòng 293, IsLuuTru=true). Không có điều kiện "qua đời"
   * (gia đình không có cột này, khác giáo dân). */
  rows: GiaDinhListItem[]
  danhMucGiaoHo?: GiaoHo[]
  /** Mở thẻ chi tiết gia đình để SỬA — bản desktop KHÔNG có nút "Thêm" ở màn hình lưu trữ
   * (`gxAddEdit1.AddButton.Visible = false`, frmGiaDinhLuuTruList.cs:25). */
  moGiaDinh: (id: string) => void
  /** Xóa VĨNH VIỄN gia đình đang chọn — đúng (và DUY NHẤT) lựa chọn của
   * `gxAddEdit1_DeleteClick` (frmGiaDinhLuuTruList.cs:170-192): desktop chỉ hỏi Yes/No, không
   * có điều kiện chặn nào (khác giáo dân). */
  onXoa?: (id: string) => Promise<void>
  onTaiLai?: () => void
}

/**
 * Hồ sơ lưu trữ gia đình — biến thể của `GiaDinhList` (xem đó để biết khuôn chung), khác đúng
 * ba điểm của `frmGiaDinhLuuTruList.cs`: (1) nguồn dữ liệu là bản ghi ĐÃ lưu trữ, (2) không có
 * nút Thêm, (3) nút Xóa chỉ có một hành động — xóa vĩnh viễn. Lưới/cột/menu chuột phải dùng
 * lại nguyên vẹn `GxGiaDinhList`/`menuGiaDinhMacDinh`.
 */
export function GiaDinhLuuTruList({ rows, moGiaDinh, danhMucGiaoHo = [], onXoa, onTaiLai }: Props) {
  const [giaoHo, setGiaoHo] = useState('-1')
  const [chiKhongThongKe, setChiKhongThongKe] = useState(false)
  const [dongChon, setDongChon] = useState<GiaDinhListItem | null>(null)
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

  const gioiThieu = useGioiThieuChuyenXu()
  const menu = useMemo(
    () => menuGiaDinhMacDinh((d) => moGiaDinh(d.id), gioiThieu.mo),
    [moGiaDinh, gioiThieu.mo],
  )

  async function xuatExcel() {
    const idGiaoHo = giaoHo === '-1' || giaoHo === '0'
      ? undefined
      : danhMucGiaoHo.find((g) => g.tenGiaoHo === giaoHo)?.id
    setDangXuatExcel(true)
    try {
      await api.giaDinh.xuatExcelLuuTru(idGiaoHo, chiKhongThongKe)
    } catch (e) {
      console.error('Không xuất được Excel hồ sơ lưu trữ gia đình', e)
      window.alert(e instanceof Error ? e.message : 'Xuất Excel thất bại, thử lại sau.')
    } finally {
      setDangXuatExcel(false)
    }
  }

  return (
    <section className="page list-page">
      <div className="list-page-head">
      <div className="page-head">
        <h1>Hồ sơ lưu trữ gia đình</h1>
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
          { label: 'Xóa bỏ', icon: 'trash', needSel: true, onClick: () => setDangHoiXoa(true),
            title: 'Xóa vĩnh viễn khỏi chương trình' },
          '>',
          { label: 'In chứng nhận hôn phối', needSel: true,
            onClick: () => dongChon && api.giaDinh.inChungNhanHonPhoi(dongChon.id)
              .catch((e: unknown) => window.alert(e instanceof Error ? e.message : 'In thất bại, thử lại sau.')),
            title: 'In chứng nhận hôn phối cho gia đình đang chọn' },
          // "In sổ gia đình" (nút riêng của màn hình lưu trữ, `frmGiaDinhLuuTruList.cs` dòng
          // 33-35/68-70) gọi `XuatSoGiaDinh()` — CÙNG một hàm/mẫu Word "PhieuGiaDinh" mà "In
          // phiếu gia đình" của màn hình danh sách thường dùng (nghiên cứu mã nguồn xác nhận:
          // hai nhãn khác nhau, cùng một report — xem in-an.md mục 5f, can-review-sau.md).
          // Nhánh Excel riêng (SoGiaDinh.xls, khi cấu hình CF_MAU_SOGIADINH khác Word) và nhánh
          // gộp nhiều gia đình một tệp (chọn nhiều dòng) KHÔNG migrate — toolbar này chỉ chọn
          // được một dòng (`needSel`), không có lựa chọn nhiều dòng ở bản web.
          { label: 'In sổ gia đình', needSel: true,
            onClick: () => dongChon && api.giaDinh.inPhieuGiaDinh(dongChon.id)
              .catch((e: unknown) => window.alert(e instanceof Error ? e.message : 'In thất bại, thử lại sau.')),
            title: 'In sổ gia đình cho gia đình đang chọn' },
        ]}
      />

      {dangHoiXoa && dongChon && (
        <div className="card glass" role="alertdialog" style={{ marginBottom: 12 }}>
          <p>
            Nếu bạn chọn xóa gia đình khỏi hồ sơ lưu trữ, gia đình này sẽ vĩnh viễn bị xóa khỏi
            chương trình.<br />
            Bạn có chắc muốn xóa gia đình{' '}
            <b>{dongChon.tenChong || dongChon.tenVo || dongChon.tenGiaDinh || dongChon.maGiaDinhCu}</b> này?
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
          <label htmlFor="gdlt2-giaoho">Giáo họ</label>
          <select id="gdlt2-giaoho" value={giaoHo} onChange={(e) => setGiaoHo(e.target.value)}>
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
          Gia đình đã xóa mềm hoặc đã chuyển xứ · nhấp đúp một dòng để mở chi tiết
        </span>
      </div>
      </div>

      <GxGiaDinhList rows={rowsLoc} onMo={(d) => moGiaDinh(d.id)} onChon={setDongChon} menuChuotPhai={menu} />
      {gioiThieu.dangMo && (
        <GioiThieuModal tieuDe={gioiThieu.tieuDe} onXuat={gioiThieu.xuat} onDong={gioiThieu.dong} />
      )}
    </section>
  )
}
