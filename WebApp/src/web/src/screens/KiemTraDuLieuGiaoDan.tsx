import type { ColDef } from 'ag-grid-community'
import { useMemo, useState } from 'react'
import { api } from '../api/client'
import type { GiaoHo, KiemTraGiaoDanKetQua, KiemTraGiaoDanTuyChon } from '../api/types'
import { GxGrid } from '../components/GxGrid'
import { cotGiaoDan, cotNguyenNhan } from '../cot/cotGiaoDan'

// Đúng thứ tự và mặc định (đều tick sẵn) của 6 ô tick
// frmKiemTraGiaoDanList.Designer.cs:213,225,237,249,261,289.
const QUY_TAC: { co: keyof KiemTraGiaoDanTuyChon; nhan: string }[] = [
  { co: 'khongCoNgayThang', nhan: 'Không có dữ liệu ngày tháng' },
  { co: 'saiQuanHeNgayThang', nhan: 'Sai quan hệ ngày tháng' },
  { co: 'khongThuocGiaDinhNao', nhan: 'Không thuộc gia đình nào' },
  { co: 'thuocNhieuGiaDinh', nhan: 'Thuộc nhiều gia đình' },
  { co: 'ruocLeTruocTuoi', nhan: 'Rước lễ trước tuổi' },
  { co: 'coNhieuHonPhoi', nhan: 'Có nhiều hôn phối' },
]

const TUY_CHON_MAC_DINH: KiemTraGiaoDanTuyChon = {
  khongCoNgayThang: true,
  saiQuanHeNgayThang: true,
  ruocLeTruocTuoi: true,
  thuocNhieuGiaDinh: true,
  khongThuocGiaDinhNao: true,
  coNhieuHonPhoi: true,
}

type Row = KiemTraGiaoDanKetQua['giaoDan'] & { nguyenNhan: string }

type Props = {
  danhMucGiaoHo: GiaoHo[]
  moGiaoDan: (id: string) => void
}

/**
 * "Công cụ dữ liệu" → "Kiểm tra dữ liệu — giáo dân" (frmKiemTraGiaoDanList.cs +
 * ReviewGiaoDanProcess.cs) — xem docs/superpowers/specs/man-hinh/cong-cu-du-lieu.md mục 2.
 * CHỈ ĐỌC: không có Thêm/Sửa/Xoá tại chỗ như bản desktop (rủi ro cao với một công cụ hàng
 * loạt, xem spec mục 7) — "Xem chi tiết" điều hướng sang thẻ giáo dân đã có sẵn.
 *
 * Bản desktop yêu cầu chọn Giáo họ (kể cả "Tất cả") trước khi kiểm tra — bản web mặc định sẵn
 * "Tất cả" nên luôn hợp lệ, không cần lặp lại hộp thoại báo lỗi đó.
 */
export function KiemTraDuLieuGiaoDan({ danhMucGiaoHo, moGiaoDan }: Props) {
  const [giaoHoId, setGiaoHoId] = useState('')
  const [tuyChon, setTuyChon] = useState<KiemTraGiaoDanTuyChon>(TUY_CHON_MAC_DINH)
  const [ketQua, setKetQua] = useState<KiemTraGiaoDanKetQua[] | null>(null)
  const [dangKiemTra, setDangKiemTra] = useState(false)
  const [loi, setLoi] = useState<string | null>(null)

  const coItNhat1 = Object.values(tuyChon).some(Boolean)

  async function batDauKiemTra() {
    if (!coItNhat1) {
      setLoi('Hãy chọn ít nhất 1 loại kiểm tra')
      return
    }
    setLoi(null)
    setDangKiemTra(true)
    try {
      const ds = await api.congCuDuLieu.kiemTraGiaoDan(giaoHoId || undefined, tuyChon)
      setKetQua(ds)
    } catch (e) {
      console.error('Không kiểm tra được dữ liệu giáo dân', e)
      setLoi(e instanceof Error ? e.message : 'Kiểm tra thất bại, thử lại sau.')
    } finally {
      setDangKiemTra(false)
    }
  }

  const rows = useMemo<Row[]>(
    () => (ketQua ?? []).map((k) => ({ ...k.giaoDan, nguyenNhan: k.nguyenNhan })),
    [ketQua],
  )

  const columnDefs = useMemo(() => [...cotGiaoDan, cotNguyenNhan] as ColDef<Row>[], [])

  return (
    <section className="page list-page">
      <div className="list-page-head">
        <div className="page-head">
          <h1>Kiểm tra dữ liệu — giáo dân</h1>
          <div className="spacer" />
          {ketQua !== null && (
            <span className="count-pill"><b>{rows.length}</b> giáo dân có lỗi</span>
          )}
        </div>

        <div className="filters-bar glass" style={{ flexWrap: 'wrap', gap: 16 }}>
          <div className="field">
            <label htmlFor="ktdl-giaoho">Giáo họ cần kiểm tra</label>
            <select id="ktdl-giaoho" value={giaoHoId} onChange={(e) => setGiaoHoId(e.target.value)}>
              <option value="">Tất cả</option>
              {danhMucGiaoHo.map((g) => <option key={g.id} value={g.id}>{g.tenGiaoHo}</option>)}
            </select>
          </div>

          {QUY_TAC.map(({ co, nhan }) => (
            <label className="toggle" key={co}>
              <input
                type="checkbox"
                checked={tuyChon[co]}
                onChange={(e) => setTuyChon((t) => ({ ...t, [co]: e.target.checked }))}
              />
              {nhan}
            </label>
          ))}

          <div className="spacer" />
          <button
            type="button"
            className="btn btn-primary"
            disabled={dangKiemTra}
            onClick={() => { void batDauKiemTra() }}
          >
            {dangKiemTra ? 'Đang kiểm tra…' : 'Bắt đầu kiểm tra'}
          </button>
        </div>
        {loi && <p className="hint" role="alert">{loi}</p>}
      </div>

      {ketQua === null ? (
        <p className="muted" style={{ padding: 16 }}>
          Chọn giáo họ và các loại kiểm tra cần dò, rồi bấm "Bắt đầu kiểm tra". Có thể mất vài
          giây với giáo xứ nhiều giáo dân.
        </p>
      ) : rows.length === 0 ? (
        <p className="muted" style={{ padding: 16 }}>Không tìm thấy lỗi dữ liệu của giáo dân nào.</p>
      ) : (
        <div className="fixed-h-grid">
          <GxGrid<Row>
            columnDefs={columnDefs}
            rowData={rows}
            layId={(d) => d.id}
            onMo={(d) => moGiaoDan(d.id)}
          />
        </div>
      )}
    </section>
  )
}
