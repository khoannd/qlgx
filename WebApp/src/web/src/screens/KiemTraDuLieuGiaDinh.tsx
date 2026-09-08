import type { ColDef } from 'ag-grid-community'
import { useMemo, useState } from 'react'
import { api } from '../api/client'
import type { GiaoHo, KiemTraGiaDinhKetQua, KiemTraGiaDinhTuyChon } from '../api/types'
import { GxGrid } from '../components/GxGrid'
import { cotGiaDinh } from '../cot/cotGiaDinh'
import { cotNguyenNhan } from '../cot/cotGiaoDan'

// Đúng thứ tự và mặc định (đều tick sẵn) của 4 ô tick — frmKiemTraGiaDinhList.cs (Designer chưa
// đọc số dòng chính xác, nhưng cả 4 ô đều Checked=true trong constructor thực tế qua
// ReviewGiaDinhProcess mặc định các cờ = true, xem cong-cu-du-lieu.md mục 3).
const QUY_TAC: { co: keyof KiemTraGiaDinhTuyChon; nhan: string }[] = [
  { co: 'khongCoNgayHonPhoi', nhan: 'Không có ngày hôn phối' },
  { co: 'honPhoiTruocTuoi', nhan: 'Ngày hôn phối không hợp lệ' },
  // Nhãn nói "16 tuổi" (KHOANGCACH_TUOI_CHAME_CONCAI) nhưng mã thật chạy 20/18 theo giới —
  // xem spec mục 3.4 và can-review-sau.md, không tự sửa nhãn cho khớp mã.
  { co: 'khoangCachTuoiConCai', nhan: 'Khoảng cách tuổi giữa con cái và cha mẹ không hợp lệ' },
  { co: 'cacVanDeKhac', nhan: 'Các vấn đề khác (nhiều vợ/chồng)' },
]

const TUY_CHON_MAC_DINH: KiemTraGiaDinhTuyChon = {
  khongCoNgayHonPhoi: true,
  honPhoiTruocTuoi: true,
  khoangCachTuoiConCai: true,
  cacVanDeKhac: true,
}

type Row = KiemTraGiaDinhKetQua['giaDinh'] & { nguyenNhan: string }

type Props = {
  danhMucGiaoHo: GiaoHo[]
  moGiaDinh: (id: string) => void
}

/**
 * "Công cụ dữ liệu" → "Kiểm tra dữ liệu — gia đình" (frmKiemTraGiaDinhList.cs +
 * ReviewGiaDinhProcess.cs) — xem docs/superpowers/specs/man-hinh/cong-cu-du-lieu.md mục 3.
 * CHỈ ĐỌC — cùng lý do với "Kiểm tra dữ liệu — giáo dân" (không Sửa/Xoá/In tại chỗ, xem spec
 * mục 3.6): đây là công cụ dò lỗi, không phải màn hình nghiệp vụ ngày thường.
 */
export function KiemTraDuLieuGiaDinh({ danhMucGiaoHo, moGiaDinh }: Props) {
  const [giaoHoId, setGiaoHoId] = useState('')
  const [tuyChon, setTuyChon] = useState<KiemTraGiaDinhTuyChon>(TUY_CHON_MAC_DINH)
  const [ketQua, setKetQua] = useState<KiemTraGiaDinhKetQua[] | null>(null)
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
      const ds = await api.congCuDuLieu.kiemTraGiaDinh(giaoHoId || undefined, tuyChon)
      setKetQua(ds)
    } catch (e) {
      console.error('Không kiểm tra được dữ liệu gia đình', e)
      setLoi(e instanceof Error ? e.message : 'Kiểm tra thất bại, thử lại sau.')
    } finally {
      setDangKiemTra(false)
    }
  }

  const rows = useMemo<Row[]>(
    () => (ketQua ?? []).map((k) => ({ ...k.giaDinh, nguyenNhan: k.nguyenNhan })),
    [ketQua],
  )

  const columnDefs = useMemo(() => [...cotGiaDinh, cotNguyenNhan] as ColDef<Row>[], [])

  return (
    <section className="page list-page">
      <div className="list-page-head">
        <div className="page-head">
          <h1>Kiểm tra dữ liệu — gia đình</h1>
          <div className="spacer" />
          {ketQua !== null && (
            <span className="count-pill"><b>{rows.length}</b> gia đình có lỗi</span>
          )}
        </div>

        <div className="filters-bar glass" style={{ flexWrap: 'wrap', gap: 16 }}>
          <div className="field">
            <label htmlFor="ktdlgd-giaoho">Giáo họ cần kiểm tra</label>
            <select id="ktdlgd-giaoho" value={giaoHoId} onChange={(e) => setGiaoHoId(e.target.value)}>
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
          Chọn giáo họ và các loại kiểm tra cần dò, rồi bấm "Bắt đầu kiểm tra".
        </p>
      ) : rows.length === 0 ? (
        <p className="muted" style={{ padding: 16 }}>Không tìm thấy lỗi dữ liệu của gia đình nào.</p>
      ) : (
        <div className="fixed-h-grid">
          <GxGrid<Row>
            columnDefs={columnDefs}
            rowData={rows}
            layId={(d) => d.id}
            onMo={(d) => moGiaDinh(d.id)}
          />
        </div>
      )}
    </section>
  )
}
