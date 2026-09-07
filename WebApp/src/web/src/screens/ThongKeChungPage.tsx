import { useEffect, useState, type CSSProperties } from 'react'
import { api } from '../api/client'
import type {
  DieuKienThongKe, GiaoHo, ThongKeChungKetQua, ThongKeOnGoiKetQua, TrangThaiHonPhoiThongKe,
} from '../api/types'
import { GxDate } from '../components/GxDate'
import { GxFormTabs } from '../components/GxFormTabs'
import { GxGrid } from '../components/GxGrid'
import { cotGiaoDan } from '../cot/cotGiaoDan'
import { cotGiaDinh } from '../cot/cotGiaDinh'
import { cotHonPhoiThongKe } from '../cot/cotHonPhoiThongKe'
import { cotOnGoi } from '../cot/cotOnGoi'

// Đúng 16 điều kiện + nhãn của gxCbCondition (GxThongKeChung.cs:532-551), giữ đúng thứ tự.
const DIEU_KIEN: { gt: DieuKienThongKe; nhan: string }[] = [
  { gt: 'SinhRa', nhan: 'Sinh ra' },
  { gt: 'RuaToi', nhan: 'Rửa tội' },
  { gt: 'RuocLeLanDau', nhan: 'Xưng tội - rước lễ lần đầu' },
  { gt: 'ThemSuc', nhan: 'Thêm sức' },
  { gt: 'HonPhoi', nhan: 'Hôn phối' },
  { gt: 'QuaDoi', nhan: 'Qua đời' },
  { gt: 'TongSoGiaoDan', nhan: 'Tổng số giáo dân' },
  { gt: 'TongSoGiaDinh', nhan: 'Tổng số gia đình (không phụ thuộc năm thống kê)' },
  { gt: 'TanTong', nhan: 'Tân tòng (thống kê theo ngày rửa tội)' },
  { gt: 'ChuHo', nhan: 'Chủ hộ' },
  { gt: 'GiaTruong', nhan: 'Gia trưởng' },
  { gt: 'HienMau', nhan: 'Hiền mẫu' },
  { gt: 'CaoNien', nhan: 'Cao niên' },
  { gt: 'GioiTre', nhan: 'Giới trẻ' },
  { gt: 'ThieuNhi', nhan: 'Thiếu nhi' },
  { gt: 'KyNiemHonPhoi', nhan: 'Kỷ niệm hôn phối' },
]

const DIEU_KIEN_TUOI: DieuKienThongKe[] = ['ChuHo', 'GiaTruong', 'HienMau', 'CaoNien', 'GioiTre', 'ThieuNhi']
const DIEU_KIEN_KHONG_LOC_NGAY: DieuKienThongKe[] = ['TongSoGiaDinh']
const DIEU_KIEN_HON_PHOI: DieuKienThongKe[] = ['HonPhoi', 'KyNiemHonPhoi']
// EnableChk (GxThongKeChung.cs:764-779) — chỉ các điều kiện này mới bật được "Tính cả dữ liệu
// không có ngày tháng".
const DIEU_KIEN_CO_KHONG_CO_NGAY: DieuKienThongKe[] = [
  'QuaDoi', 'SinhRa', 'HonPhoi', 'KyNiemHonPhoi', 'TongSoGiaoDan', 'TanTong', 'ChuHo', 'GiaTruong', 'HienMau',
]

const TRANG_THAI_HON_PHOI: { gt: TrangThaiHonPhoiThongKe; nhan: string }[] = [
  { gt: 'KhongPhanLoai', nhan: 'Không phân loại' },
  { gt: 'Chuan', nhan: 'Chuẩn' },
  { gt: 'HopThucHoa', nhan: 'Hợp thức hóa' },
  { gt: 'HopPhap', nhan: 'Hợp pháp' },
  { gt: 'LyDi', nhan: 'Ly dị' },
  { gt: 'LyThan', nhan: 'Ly thân' },
]

const macDinhTuoi: Partial<Record<DieuKienThongKe, [number | '', number | '']>> = {
  CaoNien: [60, ''],
  GioiTre: [18, 30],
  ThieuNhi: [5, 17],
}

/**
 * Tab "Thống kê chung" (`GxThongKeChung.cs`) — xem
 * docs/superpowers/specs/man-hinh/thong-ke-bieu-do.md. 16 điều kiện, ba dạng khoảng lọc (ngày,
 * tuổi, hoặc không lọc), kết quả đổ vào MỘT trong ba lưới tuỳ điều kiện.
 */
function ThongKeChungTab() {
  const [dieuKien, setDieuKien] = useState<DieuKienThongKe>('SinhRa')
  const [giaoHoId, setGiaoHoId] = useState('')
  const [danhMucGiaoHo, setDanhMucGiaoHo] = useState<GiaoHo[]>([])
  const namNay = new Date().getFullYear()
  const [tuNgay, setTuNgay] = useState(`${namNay}-01-01`)
  const [denNgay, setDenNgay] = useState(`${namNay}-12-31`)
  const [tuTuoi, setTuTuoi] = useState<number | ''>('')
  const [denTuoi, setDenTuoi] = useState<number | ''>('')
  const [luuTru, setLuuTru] = useState(false)
  const [khongCoNgay, setKhongCoNgay] = useState(false)
  const [trangThaiHonPhoi, setTrangThaiHonPhoi] = useState<TrangThaiHonPhoiThongKe | ''>('')
  const [ketQua, setKetQua] = useState<ThongKeChungKetQua | null>(null)
  const [dangTai, setDangTai] = useState(false)
  const [loi, setLoi] = useState<string | null>(null)

  useEffect(() => {
    api.giaoHo.danhMuc().then(setDanhMucGiaoHo).catch(() => {})
  }, [])

  function doiDieuKien(gt: DieuKienThongKe) {
    setDieuKien(gt)
    setKetQua(null)
    setLoi(null)
    const macDinh = macDinhTuoi[gt]
    setTuTuoi(macDinh ? macDinh[0] : '')
    setDenTuoi(macDinh ? macDinh[1] : '')
    if (DIEU_KIEN_HON_PHOI.includes(gt)) setTrangThaiHonPhoi('KhongPhanLoai')
    if (!DIEU_KIEN_CO_KHONG_CO_NGAY.includes(gt)) setKhongCoNgay(false)
  }

  const laTuoi = DIEU_KIEN_TUOI.includes(dieuKien)
  const laKhongLocNgay = DIEU_KIEN_KHONG_LOC_NGAY.includes(dieuKien)
  const laHonPhoi = DIEU_KIEN_HON_PHOI.includes(dieuKien)
  const laCaoNien = dieuKien === 'CaoNien'

  async function timKiem() {
    setLoi(null)
    if (laTuoi && (tuTuoi === '' || (denTuoi === '' && !laCaoNien))) {
      setLoi(laCaoNien ? 'Vui lòng chọn Từ tuổi' : 'Vui lòng chọn Từ tuổi/Đến tuổi')
      return
    }
    if (laTuoi && !laCaoNien && tuTuoi !== '' && denTuoi !== '' && Number(tuTuoi) > Number(denTuoi)) {
      setLoi('Từ tuổi phải nhỏ hơn hoặc bằng đến tuổi')
      return
    }
    if (laHonPhoi && !trangThaiHonPhoi) {
      setLoi('Vui lòng chọn 1 trạng thái hôn nhân')
      return
    }
    if (!laTuoi && !laKhongLocNgay && (!tuNgay || !denNgay)) {
      setLoi('Hãy nhập từ ngày')
      return
    }
    if (!laTuoi && !laKhongLocNgay && tuNgay > denNgay) {
      setLoi('Từ ngày không thể lớn hơn đến ngày')
      return
    }
    setDangTai(true)
    try {
      const res = await api.thongKe.chung({
        dieuKien,
        giaoHoId: giaoHoId || undefined,
        tuNgay: laTuoi || laKhongLocNgay ? undefined : tuNgay,
        denNgay: laTuoi || laKhongLocNgay ? undefined : denNgay,
        tuTuoi: laTuoi && tuTuoi !== '' ? tuTuoi : undefined,
        denTuoi: laTuoi && denTuoi !== '' ? denTuoi : undefined,
        luuTru,
        khongCoNgay,
        trangThaiHonPhoi: laHonPhoi && trangThaiHonPhoi !== '' ? trangThaiHonPhoi : undefined,
      })
      setKetQua(res)
    } catch (e) {
      setLoi(e instanceof Error ? e.message : String(e))
    } finally {
      setDangTai(false)
    }
  }

  return (
    <section>
      <div className="filters-bar glass" style={{ flexWrap: 'wrap', gap: 12 }}>
        <div className="field">
          <label htmlFor="tkc-dieukien">Điều kiện</label>
          <select id="tkc-dieukien" value={dieuKien} onChange={(e) => doiDieuKien(e.target.value as DieuKienThongKe)}>
            {DIEU_KIEN.map((d) => <option key={d.gt} value={d.gt}>{d.nhan}</option>)}
          </select>
        </div>
        <div className="field">
          <label htmlFor="tkc-giaoho">Giáo họ</label>
          <select id="tkc-giaoho" value={giaoHoId} onChange={(e) => setGiaoHoId(e.target.value)}>
            <option value="">Tất cả</option>
            {danhMucGiaoHo.map((g) => <option key={g.id} value={g.id}>{g.tenGiaoHo}</option>)}
          </select>
        </div>

        {!laTuoi && !laKhongLocNgay && (
          <>
            <div className="field"><label>Từ ngày</label><GxDate defaultValue={tuNgay} onIsoChange={setTuNgay} /></div>
            <div className="field"><label>Đến ngày</label><GxDate defaultValue={denNgay} onIsoChange={setDenNgay} /></div>
          </>
        )}
        {laTuoi && (
          <>
            <div className="field">
              <label htmlFor="tkc-tutuoi">Từ tuổi</label>
              <input id="tkc-tutuoi" type="number" min={1} max={149} value={tuTuoi}
                onChange={(e) => setTuTuoi(e.target.value === '' ? '' : Number(e.target.value))} style={{ width: 80 }} />
            </div>
            {!laCaoNien && (
              <div className="field">
                <label htmlFor="tkc-dentuoi">Đến tuổi</label>
                <input id="tkc-dentuoi" type="number" min={1} max={149} value={denTuoi}
                  onChange={(e) => setDenTuoi(e.target.value === '' ? '' : Number(e.target.value))} style={{ width: 80 }} />
              </div>
            )}
          </>
        )}
        {laHonPhoi && (
          <div className="field">
            <label htmlFor="tkc-trangthai">Trạng thái hôn phối</label>
            <select id="tkc-trangthai" value={trangThaiHonPhoi}
              onChange={(e) => setTrangThaiHonPhoi(e.target.value as TrangThaiHonPhoiThongKe)}>
              <option value="" disabled>— Chọn —</option>
              {TRANG_THAI_HON_PHOI.map((t) => <option key={t.gt} value={t.gt}>{t.nhan}</option>)}
            </select>
          </div>
        )}

        <label className="toggle">
          <input type="checkbox" checked={luuTru} onChange={(e) => setLuuTru(e.target.checked)} />
          Tính cả trong hồ sơ lưu trữ
        </label>
        <label className="toggle" title={DIEU_KIEN_CO_KHONG_CO_NGAY.includes(dieuKien) ? undefined
          : 'Không áp dụng cho điều kiện này'}>
          <input type="checkbox" checked={khongCoNgay} disabled={!DIEU_KIEN_CO_KHONG_CO_NGAY.includes(dieuKien)}
            onChange={(e) => setKhongCoNgay(e.target.checked)} />
          Tính cả những dữ liệu không có ngày tháng
        </label>

        <button type="button" className="btn btn-primary" onClick={() => { void timKiem() }} disabled={dangTai}>
          {dangTai ? 'Đang tìm…' : 'Tìm kiếm'}
        </button>
        {ketQua && (
          <span className="count-pill"><b>{ketQua.tongCong}</b>{ketQua.nhan}</span>
        )}
      </div>
      {loi && <p className="hint" role="alert" style={{ padding: '4px' }}>{loi}</p>}

      {ketQua && (
        <div className="fixed-h-grid" style={{ '--fixed-h-grid': '460px', marginTop: 12 } as CSSProperties}>
          {ketQua.giaoDan && (
            <GxGrid columnDefs={cotGiaoDan} rowData={ketQua.giaoDan} layId={(d) => d.id} />
          )}
          {ketQua.giaDinh && (
            <GxGrid columnDefs={cotGiaDinh} rowData={ketQua.giaDinh} layId={(d) => d.id} />
          )}
          {ketQua.honPhoi && (
            <GxGrid columnDefs={cotHonPhoiThongKe} rowData={ketQua.honPhoi} layId={(d) => d.id} />
          )}
        </div>
      )}
    </section>
  )
}

/** Tab "Thống kê ơn gọi tận hiến" (`GxThongKeOnGoi.cs`) — `cbGiaoHo` cố ý KHÔNG có trên bản
 * web (không nối logic ở bản gốc, xem thong-ke-bieu-do.md mục 9). */
function ThongKeOnGoiTab() {
  const namNay = new Date().getFullYear()
  const [tuNgay, setTuNgay] = useState(`${namNay}-01-01`)
  const [denNgay, setDenNgay] = useState(`${namNay}-12-31`)
  const [chucVu, setChucVu] = useState('')
  const [noiTu, setNoiTu] = useState('')
  const [dongTu, setDongTu] = useState('')
  const [noiPhucVu, setNoiPhucVu] = useState('')
  const [luuTru, setLuuTru] = useState(false)
  const [khongCoNgay, setKhongCoNgay] = useState(false)
  const [ketQua, setKetQua] = useState<ThongKeOnGoiKetQua | null>(null)
  const [dangTai, setDangTai] = useState(false)
  const [loi, setLoi] = useState<string | null>(null)

  async function timKiem() {
    setLoi(null)
    if (!tuNgay) { setLoi('Hãy nhập từ ngày'); return }
    if (denNgay && tuNgay > denNgay) { setLoi('Từ ngày không thể lớn hơn đến ngày'); return }
    setDangTai(true)
    try {
      const res = await api.thongKe.onGoi({
        tuNgay, denNgay: denNgay || undefined,
        chucVu: chucVu || undefined, noiTu: noiTu || undefined, dongTu: dongTu || undefined,
        noiPhucVu: noiPhucVu || undefined, luuTru, khongCoNgay,
      })
      setKetQua(res)
    } catch (e) {
      setLoi(e instanceof Error ? e.message : String(e))
    } finally {
      setDangTai(false)
    }
  }

  return (
    <section>
      <div className="filters-bar glass" style={{ flexWrap: 'wrap', gap: 12 }}>
        <div className="field"><label>Từ ngày</label><GxDate defaultValue={tuNgay} onIsoChange={setTuNgay} /></div>
        <div className="field"><label>Đến ngày</label><GxDate defaultValue={denNgay} onIsoChange={setDenNgay} /></div>
        <div className="field">
          <label htmlFor="tko-chucvu">Chức vụ</label>
          <select id="tko-chucvu" value={chucVu} onChange={(e) => setChucVu(e.target.value)}>
            <option value="">— Tất cả —</option>
            {['Tu sĩ', 'Chủng sinh', 'Phó tế', 'Linh mục', 'Giám mục', 'Khấn trọn', 'Khác'].map((c) => (
              <option key={c} value={c}>{c}</option>
            ))}
          </select>
        </div>
        <div className="field">
          <label htmlFor="tko-noitu">Nơi tu</label>
          <input id="tko-noitu" value={noiTu} onChange={(e) => setNoiTu(e.target.value)} />
        </div>
        <div className="field">
          <label htmlFor="tko-dongtu">Dòng tu</label>
          <input id="tko-dongtu" value={dongTu} onChange={(e) => setDongTu(e.target.value)} />
        </div>
        <div className="field">
          <label htmlFor="tko-noiphucvu">Nơi phục vụ</label>
          <input id="tko-noiphucvu" value={noiPhucVu} onChange={(e) => setNoiPhucVu(e.target.value)} />
        </div>
        <label className="toggle">
          <input type="checkbox" checked={luuTru} onChange={(e) => setLuuTru(e.target.checked)} />
          Tính cả trong hồ sơ lưu trữ
        </label>
        <label className="toggle">
          <input type="checkbox" checked={khongCoNgay} onChange={(e) => setKhongCoNgay(e.target.checked)} />
          Tính cả những dữ liệu không có ngày tháng
        </label>
        <button type="button" className="btn btn-primary" onClick={() => { void timKiem() }} disabled={dangTai}>
          {dangTai ? 'Đang tìm…' : 'Tìm kiếm'}
        </button>
        {ketQua && <span className="count-pill"><b>{ketQua.tongCong}</b> người theo ơn gọi</span>}
      </div>
      {loi && <p className="hint" role="alert" style={{ padding: '4px' }}>{loi}</p>}

      {ketQua && (
        <div className="fixed-h-grid" style={{ '--fixed-h-grid': '460px', marginTop: 12 } as CSSProperties}>
          <GxGrid columnDefs={cotOnGoi} rowData={ketQua.rows} layId={(d) => d.id} />
        </div>
      )}
    </section>
  )
}

/** Màn hình "Thống kê chung" (`frmThongKeChung.cs`) — hai tab, xem
 * docs/superpowers/specs/man-hinh/thong-ke-bieu-do.md. */
export function ThongKeChungPage() {
  return (
    <div className="page">
      <div className="page-head"><h1>Thống kê chung</h1></div>
      <GxFormTabs
        tabs={[
          { title: 'Thống kê chung', noiDung: <ThongKeChungTab /> },
          { title: 'Thống kê ơn gọi tận hiến', noiDung: <ThongKeOnGoiTab /> },
        ]}
      />
    </div>
  )
}
