import { useEffect, useState, type CSSProperties } from 'react'
import { api, LoiXungDot } from '../api/client'
import type { DotBiTichDetail as DotBiTichDetailType, GiaoDanTimKiem, LoaiBiTich, NguoiNhanBiTich } from '../api/types'
import { GxGrid } from '../components/GxGrid'
import { GxDate } from '../components/GxDate'
import { GxField, GxInline } from '../components/GxField'
import { GxPicker } from '../components/GxPicker'
import { TrangThaiTai } from '../components/TrangThaiTai'
import { cotNguoiNhanBiTich } from '../cot/cotNguoiNhanBiTich'

const TEN_LOAI: Record<LoaiBiTich, string> = { 0: 'Rửa tội', 1: 'Rước lễ (XTRL lần đầu)', 2: 'Thêm sức' }
const NHAN_SO: Record<LoaiBiTich, string> = { 0: 'Số rửa tội', 1: 'Số XTRL', 2: 'Số thêm sức' }
const CO_NGUOI_DO_DAU: Record<LoaiBiTich, boolean> = { 0: true, 1: false, 2: true }

type Props = {
  id: string | null
  loaiBiTich: LoaiBiTich
  onTieuDe?: (ten: string) => void
  onDaLuu?: () => void
  /** Nút "+" của `GxPicker` "Danh sách người nhận" — mở một thẻ "Giáo dân mới" TÁCH BIỆT, tạo
   * xong tự đóng lại rồi thêm luôn người vừa tạo vào danh sách người nhận (giống hệt `onChon`
   * khi chọn từ danh sách có sẵn) — cùng cơ chế đã dùng ở `GiaDinhDetail`, xem `GxPicker.tsx`,
   * `App.moChiTietGiaoDan`, can-review-sau.md mục 19. */
  moGiaoDanMoiChoPicker?: (onTaoXong: (gd: GiaoDanTimKiem) => void) => void
}

/**
 * Chi tiết một đợt bí tích — khớp `frmBiTichChiTiet.cs`: Mô tả/Ngày/Linh mục/Nơi bí tích ở
 * đầu form, danh sách người nhận (lưới `gxBiTichChiTiet1`) bên dưới. Xem
 * docs/superpowers/specs/man-hinh/so-bi-tich.md.
 */
export function DotBiTichDetail({ id, loaiBiTich, onTieuDe, onDaLuu, moGiaoDanMoiChoPicker }: Props) {
  const [dot, setDot] = useState<DotBiTichDetailType | null>(null)
  const [dangTai, setDangTai] = useState(!!id)
  const [loi, setLoi] = useState<string | null>(null)

  const [moTa, setMoTa] = useState('')
  const [ngayBiTich, setNgayBiTich] = useState<string | null>(null)
  const [linhMuc, setLinhMuc] = useState('')
  const [noiBiTich, setNoiBiTich] = useState('')

  const [dongChon, setDongChon] = useState<NguoiNhanBiTich | null>(null)
  const [soBiTich, setSoBiTich] = useState('')
  const [nguoiDoDau, setNguoiDoDau] = useState('')
  const [ghiChu, setGhiChu] = useState('')

  const [dangLuu, setDangLuu] = useState(false)
  const [loiLuu, setLoiLuu] = useState<string | null>(null)
  const [dangLuuNguoiNhan, setDangLuuNguoiNhan] = useState(false)
  const [loiNguoiNhan, setLoiNguoiNhan] = useState<string | null>(null)

  // Tái sử dụng ở cả tải lần đầu (effect) lẫn nút "Thử lại" của `TrangThaiTai` — trước đây nút
  // "Thử lại" gọi thẳng một closure rút gọn (`api.dotBiTich.chiTiet(id).then(setDot)`) không hề
  // `setLoi(null)`/`setDangTai(true)`, nên dù tải lại thành công màn hình vẫn đứng yên ở nhánh
  // lỗi của `TrangThaiTai` (nó render theo `loi`, không phải theo dữ liệu đã có) — xem
  // can-review-sau.md, rà lại theo yêu cầu người dùng 2026-09-08 (review toàn nhánh, "Nghiêm
  // trọng #2"). Khuôn đúng lấy từ `GiaoDanDetailPage.tsx`.
  function tai() {
    if (!id) return
    setDangTai(true)
    setLoi(null)
    api.dotBiTich.chiTiet(id)
      .then((d) => {
        setDot(d)
        setMoTa(d.moTa ?? '')
        setNgayBiTich(d.ngayBiTich)
        setLinhMuc(d.linhMuc ?? '')
        setNoiBiTich(d.noiBiTich ?? '')
        onTieuDe?.(d.moTa || `Đợt bí tích #${d.maDotBiTichCu}`)
      })
      .catch((e: unknown) => setLoi(e instanceof Error ? e.message : String(e)))
      .finally(() => setDangTai(false))
  }

  useEffect(() => {
    if (!id) {
      onTieuDe?.(`Đợt bí tích mới — ${TEN_LOAI[loaiBiTich]}`)
      return
    }
    tai()
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [id, loaiBiTich])

  function chonDong(d: NguoiNhanBiTich | null) {
    setDongChon(d)
    setSoBiTich(d?.soBiTich ?? '')
    setNguoiDoDau(d?.nguoiDoDau ?? '')
    setGhiChu(d?.ghiChu ?? '')
    setLoiNguoiNhan(null)
  }

  async function luuDot() {
    if (moTa.trim() === '') {
      setLoiLuu('Hãy mô tả cho đợt bí tích này!')
      return
    }
    setDangLuu(true)
    setLoiLuu(null)
    try {
      if (!dot) {
        const moi = await api.dotBiTich.tao({ loaiBiTich, ngayBiTich, moTa, linhMuc: linhMuc || null, noiBiTich: noiBiTich || null })
        setDot(moi)
        onTieuDe?.(moi.moTa || `Đợt bí tích #${moi.maDotBiTichCu}`)
      } else {
        await api.dotBiTich.capNhat(dot.id, { ngayBiTich, moTa, linhMuc: linhMuc || null, noiBiTich: noiBiTich || null, rowVersion: dot.rowVersion })
        const lai = await api.dotBiTich.chiTiet(dot.id)
        setDot(lai)
      }
      onDaLuu?.()
    } catch (e) {
      setLoiLuu(e instanceof LoiXungDot ? e.message : e instanceof Error ? e.message : 'Lưu thất bại, thử lại sau.')
    } finally {
      setDangLuu(false)
    }
  }

  async function themNguoiNhan(gd: GiaoDanTimKiem) {
    if (!dot) return
    setDangLuuNguoiNhan(true)
    setLoiNguoiNhan(null)
    try {
      await api.dotBiTich.themNguoiNhan(dot.id, { giaoDanId: gd.id, soBiTich: null, nguoiDoDau: null, ghiChu: null })
      const lai = await api.dotBiTich.chiTiet(dot.id)
      setDot(lai)
    } catch (e) {
      setLoiNguoiNhan(e instanceof Error ? e.message : 'Không thêm được, thử lại sau.')
    } finally {
      setDangLuuNguoiNhan(false)
    }
  }

  async function luuNguoiNhan() {
    if (!dot || !dongChon) return
    setDangLuuNguoiNhan(true)
    setLoiNguoiNhan(null)
    try {
      await api.dotBiTich.suaNguoiNhan(dot.id, dongChon.giaoDanId, { soBiTich: soBiTich || null, nguoiDoDau: nguoiDoDau || null, ghiChu: ghiChu || null })
      const lai = await api.dotBiTich.chiTiet(dot.id)
      setDot(lai)
      chonDong(lai.nguoiNhan.find((n) => n.giaoDanId === dongChon.giaoDanId) ?? null)
    } catch (e) {
      setLoiNguoiNhan(e instanceof Error ? e.message : 'Không lưu được, thử lại sau.')
    } finally {
      setDangLuuNguoiNhan(false)
    }
  }

  async function xoaNguoiNhan() {
    if (!dot || !dongChon) return
    if (!window.confirm('Bạn có chắc muốn loại bỏ giáo dân này ra khỏi danh sách không?')) return
    const xoaCaThongTin = window.confirm(`Bạn có muốn xóa cả thông tin [${TEN_LOAI[loaiBiTich]}] của giáo dân này không?`)
    setDangLuuNguoiNhan(true)
    try {
      await api.dotBiTich.xoaNguoiNhan(dot.id, dongChon.giaoDanId, xoaCaThongTin)
      const lai = await api.dotBiTich.chiTiet(dot.id)
      setDot(lai)
      chonDong(null)
    } catch (e) {
      setLoiNguoiNhan(e instanceof Error ? e.message : 'Không xoá được, thử lại sau.')
    } finally {
      setDangLuuNguoiNhan(false)
    }
  }

  return (
    <TrangThaiTai dangTai={dangTai} loi={loi} onThuLai={tai}>
      <section className="page" style={{ overflowY: 'auto', display: 'block' }}>
        <div className="page-head">
          <h1>{TEN_LOAI[loaiBiTich]} {dot ? `— ${dot.moTa}` : '(đợt mới)'}</h1>
        </div>

        {/* Cùng lý do đổi `.form-grid`/`.field` sang `.frow`/`GxField` như `HoiDoanDetail.tsx`
            (rà thêm theo yêu cầu người dùng 2026-09-08 — "sổ bí tích" nằm trong danh sách các
            màn hình mới migrate cần rà) — xem chú thích dài ở đó. */}
        <div className="card glass" style={{ marginBottom: 12 }}>
          <GxField label="Mô tả" id="dbt-mota">
            <input id="dbt-mota" type="text" value={moTa} onChange={(e) => setMoTa(e.target.value)} />
          </GxField>
          <GxField label="Ngày bí tích" id="dbt-ngay">
            <GxDate id="dbt-ngay" defaultValue={ngayBiTich} onIsoChange={(iso) => setNgayBiTich(iso || null)} style={{ maxWidth: 170 }} />
            <GxInline>Linh mục</GxInline>
            <input aria-label="Linh mục" id="dbt-linhmuc" type="text" value={linhMuc} onChange={(e) => setLinhMuc(e.target.value)} />
          </GxField>
          <GxField label="Nơi nhận bí tích" id="dbt-noi">
            <input id="dbt-noi" type="text" value={noiBiTich} onChange={(e) => setNoiBiTich(e.target.value)} />
          </GxField>
          {loiLuu && <p className="hint" role="alert">{loiLuu}</p>}
          <div className="cmdbar">
            <div className="spacer" />
            <button type="button" className="btn btn-primary" disabled={dangLuu} onClick={() => { void luuDot() }}>
              {dangLuu ? 'Đang lưu…' : 'Cập nhật'}
            </button>
          </div>
        </div>

        {dot && (
          <>
            <div className="card glass" style={{ marginBottom: 12 }}>
              <div className="cmdbar" style={{ marginBottom: 8 }}>
                <b>Danh sách người nhận ({dot.nguoiNhan.length})</b>
                <div className="spacer" />
                <GxPicker onChon={(gd) => { void themNguoiNhan(gd) }} onBoChon={() => {}}
                  onThemMoi={moGiaoDanMoiChoPicker ? () => moGiaoDanMoiChoPicker((gd) => { void themNguoiNhan(gd) }) : undefined} />
              </div>
              {loiNguoiNhan && <p className="hint" role="alert">{loiNguoiNhan}</p>}
              {/* `.fixed-h-grid` (qlgx.css) — KHÔNG dùng inline `style={{ height }}` đơn thuần
                  trên div bọc: gây "table-card cao ~3px, ẩn hết dòng dữ liệu" trong trang
                  cuộn-cả-trang (`section.page` ở đây khai `display:'block'`, kéo `.card` con
                  cũng thành block) — đúng lớp lỗi đã phát hiện và sửa ở Hội đoàn/Giáo lý khối/
                  lớp cùng đợt commit này nhưng bị sót lại đúng ở đây, nơi PHÁT HIỆN RA lớp lỗi
                  này lần đầu — rà lại theo yêu cầu người dùng 2026-09-08 (review toàn nhánh,
                  "Cao #1"). Xem chú thích dài trong qlgx.css. */}
              <div className="fixed-h-grid" style={{ '--fixed-h-grid': '420px' } as CSSProperties}>
                <GxGrid<NguoiNhanBiTich>
                  columnDefs={cotNguoiNhanBiTich(NHAN_SO[loaiBiTich], CO_NGUOI_DO_DAU[loaiBiTich])}
                  rowData={dot.nguoiNhan}
                  layId={(d) => d.giaoDanId}
                  onChon={chonDong}
                />
              </div>
            </div>

            {dongChon && (
              <div className="card glass">
                <b>{(dongChon.tenThanh ? dongChon.tenThanh + ' ' : '') + dongChon.hoTen}</b>
                <div style={{ marginTop: 8 }}>
                  <GxField label={NHAN_SO[loaiBiTich]} id="nn-so">
                    <input id="nn-so" type="text" value={soBiTich} onChange={(e) => setSoBiTich(e.target.value)} style={{ maxWidth: 150 }} />
                    {CO_NGUOI_DO_DAU[loaiBiTich] && (
                      <>
                        <GxInline>Người đỡ đầu</GxInline>
                        <input aria-label="Người đỡ đầu" id="nn-dodau" type="text" value={nguoiDoDau} onChange={(e) => setNguoiDoDau(e.target.value)} />
                      </>
                    )}
                  </GxField>
                  <GxField label="Ghi chú" id="nn-ghichu">
                    <input id="nn-ghichu" type="text" value={ghiChu} onChange={(e) => setGhiChu(e.target.value)} />
                  </GxField>
                </div>
                <div className="cmdbar">
                  <button type="button" className="btn btn-danger" disabled={dangLuuNguoiNhan} onClick={() => { void xoaNguoiNhan() }}>
                    Xóa khỏi danh sách
                  </button>
                  <div className="spacer" />
                  <button type="button" className="btn btn-primary" disabled={dangLuuNguoiNhan} onClick={() => { void luuNguoiNhan() }}>
                    {dangLuuNguoiNhan ? 'Đang lưu…' : 'Lưu'}
                  </button>
                </div>
              </div>
            )}
          </>
        )}
      </section>
    </TrangThaiTai>
  )
}
