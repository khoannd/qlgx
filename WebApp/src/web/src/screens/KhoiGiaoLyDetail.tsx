import { useEffect, useMemo, useState, type CSSProperties } from 'react'
import { api, LoiXungDot } from '../api/client'
import type { GiaoDanTimKiem, KhoiGiaoLy, LopGiaoLy } from '../api/types'
import { GxGrid } from '../components/GxGrid'
import { GxField } from '../components/GxField'
import { GxPicker } from '../components/GxPicker'
import { TrangThaiTai } from '../components/TrangThaiTai'
import { cotLopGiaoLy } from '../cot/cotGiaoLy'

type Props = {
  id: string | null
  onTieuDe?: (ten: string) => void
  onDaLuu?: () => void
  moLop: (lopId: string | null, khoiId: string, namMoi?: number) => void
  /** Nút "+" của `GxPicker` "Người quản lý" — mở một thẻ "Giáo dân mới" TÁCH BIỆT, tạo xong tự
   * đóng lại và điền ngược vào ô đang chọn — cùng cơ chế đã dùng ở `GiaDinhDetail`, xem
   * `GxPicker.tsx`, `App.moChiTietGiaoDan`, can-review-sau.md mục 19. */
  moGiaoDanMoiChoPicker?: (onTaoXong: (gd: GiaoDanTimKiem) => void) => void
}

const NAM_HIEN_TAI = new Date().getFullYear()

/**
 * Chi tiết một khối giáo lý (frmKhoiGiaoLy.cs) — khối "Tên khối/Người quản lý/Ghi chú" ở trên,
 * danh sách lớp thuộc khối (lọc theo năm) bên dưới. Xem
 * docs/superpowers/specs/man-hinh/giao-ly.md.
 *
 * Khác biệt cố ý so với bản gốc (giống hội đoàn — xem giao-ly.md mục 8): khối và lớp lưu RIÊNG,
 * mỗi thao tác gọi API ngay; bấm "Thêm lớp"/mở một lớp mở một THẺ TÀI LIỆU riêng
 * (`LopGiaoLyDetail`) thay vì hộp thoại modal lồng nhau như bản gốc.
 */
export function KhoiGiaoLyDetail({ id, onTieuDe, onDaLuu, moLop, moGiaoDanMoiChoPicker }: Props) {
  const [khoi, setKhoi] = useState<KhoiGiaoLy | null>(null)
  const [dangTai, setDangTai] = useState(!!id)
  const [loi, setLoi] = useState<string | null>(null)

  const [tenKhoi, setTenKhoi] = useState('')
  const [nguoiQuanLy, setNguoiQuanLy] = useState<{ id: string; ten: string } | null>(null)
  const [ghiChu, setGhiChu] = useState('')
  const [dangLuu, setDangLuu] = useState(false)
  const [loiLuu, setLoiLuu] = useState<string | null>(null)

  const [xacNhanXoa, setXacNhanXoa] = useState(false)
  const [dangXoa, setDangXoa] = useState(false)

  const [lop, setLop] = useState<LopGiaoLy[] | null>(null)
  const [loiLop, setLoiLop] = useState<string | null>(null)
  const [namLoc, setNamLoc] = useState<number | null>(NAM_HIEN_TAI)

  function taiLop(khoiId: string, nam: number | null) {
    api.giaoLy.lop(khoiId, nam)
      .then(setLop)
      .catch((e: unknown) => setLoiLop(e instanceof Error ? e.message : String(e)))
  }

  // Tái sử dụng ở cả tải lần đầu (effect) lẫn nút "Thử lại" của `TrangThaiTai` — trước đây nút
  // "Thử lại" gọi thẳng một closure rút gọn không hề `setLoi(null)`/`setDangTai(true)`, nên dù
  // tải lại thành công màn hình vẫn đứng yên ở nhánh lỗi (rà lại theo yêu cầu người dùng
  // 2026-09-08, review toàn nhánh "Nghiêm trọng #2"). Khuôn đúng lấy từ `GiaoDanDetailPage.tsx`.
  function tai() {
    if (!id) return
    setDangTai(true)
    setLoi(null)
    api.giaoLy.khoi()
      .then((ds) => {
        const dong = ds.find((k) => k.id === id)
        if (!dong) { setLoi('Không tìm thấy khối giáo lý này — có thể đã bị xoá.'); return }
        setKhoi(dong)
        setTenKhoi(dong.tenKhoi)
        setNguoiQuanLy(dong.nguoiQuanLyId ? { id: dong.nguoiQuanLyId, ten: dong.tenNguoiQuanLy ?? '' } : null)
        setGhiChu(dong.ghiChu ?? '')
        onTieuDe?.(dong.tenKhoi)
        taiLop(id, namLoc)
      })
      .catch((e: unknown) => setLoi(e instanceof Error ? e.message : String(e)))
      .finally(() => setDangTai(false))
  }

  useEffect(() => {
    if (!id) {
      onTieuDe?.('Khối giáo lý mới')
      return
    }
    tai()
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [id])

  function doiNamLoc(v: string) {
    const nam = v === '' ? null : Number(v)
    setNamLoc(nam)
    if (khoi) taiLop(khoi.id, nam)
  }

  const cacNam = useMemo(() => {
    const bo = new Set<number>()
    for (const l of lop ?? []) if (l.nam) bo.add(l.nam)
    bo.add(NAM_HIEN_TAI)
    return [...bo].sort((a, b) => b - a)
  }, [lop])

  async function luuKhoi() {
    if (tenKhoi.trim() === '') { setLoiLuu('Hãy nhập tên khối giáo lý'); return }
    if (!nguoiQuanLy) { setLoiLuu('Hãy chọn người quản lý'); return }
    setDangLuu(true)
    setLoiLuu(null)
    const than = { tenKhoi: tenKhoi.trim(), nguoiQuanLyId: nguoiQuanLy.id, ghiChu: ghiChu || null, rowVersion: khoi?.rowVersion ?? null }
    try {
      if (!khoi) {
        const moi = await api.giaoLy.themKhoi(than)
        const ds = await api.giaoLy.khoi()
        const dong = ds.find((k) => k.id === moi.id)!
        setKhoi(dong)
        onTieuDe?.(dong.tenKhoi)
        taiLop(dong.id, namLoc)
      } else {
        await api.giaoLy.suaKhoi(khoi.id, than)
        const ds = await api.giaoLy.khoi()
        const dong = ds.find((k) => k.id === khoi.id)!
        setKhoi(dong)
        onTieuDe?.(dong.tenKhoi)
      }
      onDaLuu?.()
    } catch (e) {
      setLoiLuu(e instanceof LoiXungDot ? e.message : e instanceof Error ? e.message : 'Lưu thất bại, thử lại sau.')
    } finally {
      setDangLuu(false)
    }
  }

  async function xoaKhoi() {
    if (!khoi) return
    setDangXoa(true)
    try {
      await api.giaoLy.xoaKhoi(khoi.id)
      onDaLuu?.()
    } catch (e) {
      setLoiLuu(e instanceof Error ? e.message : 'Xoá thất bại, thử lại sau.')
      setDangXoa(false)
      setXacNhanXoa(false)
    }
  }

  return (
    <TrangThaiTai dangTai={dangTai} loi={loi} onThuLai={tai}>
      <section className="page" style={{ overflowY: 'auto', display: 'block' }}>
        <div className="page-head">
          <h1>{khoi ? khoi.tenKhoi : 'Khối giáo lý mới'}</h1>
        </div>

        {/* Cùng lý do đổi `.form-grid`/`.field` sang `.frow`/`GxField` như `HoiDoanDetail.tsx`
            (rà thêm theo yêu cầu người dùng 2026-09-08) — xem chú thích dài ở đó. */}
        <div className="card glass" style={{ marginBottom: 12 }}>
          <GxField label="Tên khối" id="kgl-ten">
            <input id="kgl-ten" type="text" value={tenKhoi} onChange={(e) => setTenKhoi(e.target.value)} />
          </GxField>
          <GxField label="Người quản lý" id="kgl-nguoiql">
            <GxPicker
              id="kgl-nguoiql"
              value={nguoiQuanLy?.ten}
              onChon={(gd: GiaoDanTimKiem) => setNguoiQuanLy({ id: gd.id, ten: (gd.tenThanh ? gd.tenThanh + ' ' : '') + gd.hoTen })}
              onThemMoi={moGiaoDanMoiChoPicker
                ? () => moGiaoDanMoiChoPicker((gd) => setNguoiQuanLy({ id: gd.id, ten: (gd.tenThanh ? gd.tenThanh + ' ' : '') + gd.hoTen }))
                : undefined}
              onBoChon={() => setNguoiQuanLy(null)}
            />
          </GxField>
          <GxField label="Ghi chú" id="kgl-ghichu">
            <input id="kgl-ghichu" type="text" value={ghiChu} onChange={(e) => setGhiChu(e.target.value)} />
          </GxField>
          {loiLuu && <p className="hint" role="alert">{loiLuu}</p>}
          <div className="cmdbar">
            {khoi && (
              <button type="button" className="btn" disabled={dangXoa} onClick={() => setXacNhanXoa(true)}>
                Xóa khối
              </button>
            )}
            <div className="spacer" />
            <button type="button" className="btn btn-primary" disabled={dangLuu} onClick={() => { void luuKhoi() }}>
              {dangLuu ? 'Đang lưu…' : 'Cập nhật'}
            </button>
          </div>
          {xacNhanXoa && (
            <div className="card glass" role="alertdialog" style={{ marginTop: 8 }}>
              <p>Bạn có chắc muốn xóa khối giáo lý này? Các lớp giáo lý thuộc khối này sẽ bị xóa theo.</p>
              <div className="cmdbar">
                <button type="button" className="btn" disabled={dangXoa} onClick={() => setXacNhanXoa(false)}>Hủy bỏ</button>
                <div className="spacer" />
                <button type="button" className="btn btn-danger" disabled={dangXoa} onClick={() => { void xoaKhoi() }}>
                  {dangXoa ? 'Đang xoá…' : 'Xóa'}
                </button>
              </div>
            </div>
          )}
        </div>

        {khoi && (
          <div className="card glass" style={{ marginBottom: 12 }}>
            <div className="cmdbar" style={{ marginBottom: 8 }}>
              <b>Danh sách lớp giáo lý ({lop?.length ?? 0})</b>
              <label style={{ display: 'flex', alignItems: 'center', gap: 4, fontSize: 12.5 }}>
                Năm:
                <select value={namLoc ?? ''} onChange={(e) => doiNamLoc(e.target.value)}>
                  <option value="">Tất cả các năm</option>
                  {cacNam.map((n) => <option key={n} value={n}>{n}</option>)}
                </select>
              </label>
              <div className="spacer" />
              <button type="button" className="btn btn-primary" onClick={() => moLop(null, khoi.id, namLoc ?? NAM_HIEN_TAI)}>
                Thêm lớp
              </button>
            </div>
            {loiLop && <p className="hint" role="alert">{loiLop}</p>}
            {/* `.fixed-h-grid` (qlgx.css) — KHÔNG dùng inline `style={{ height }}` đơn thuần
                trên div bọc: gây "table-card cao ~3px" trong trang cuộn-cả-trang, xem chú
                thích dài trong qlgx.css. */}
            <div className="fixed-h-grid" style={{ '--fixed-h-grid': '340px' } as CSSProperties}>
              <GxGrid<LopGiaoLy>
                columnDefs={cotLopGiaoLy}
                rowData={lop ?? []}
                layId={(d) => d.id}
                onMo={(d) => moLop(d.id, khoi.id)}
              />
            </div>
          </div>
        )}
      </section>
    </TrangThaiTai>
  )
}
