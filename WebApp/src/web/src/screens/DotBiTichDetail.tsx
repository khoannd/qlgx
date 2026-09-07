import { useEffect, useState } from 'react'
import { api, LoiXungDot } from '../api/client'
import type { DotBiTichDetail as DotBiTichDetailType, GiaoDanTimKiem, LoaiBiTich, NguoiNhanBiTich } from '../api/types'
import { GxGrid } from '../components/GxGrid'
import { GxDate } from '../components/GxDate'
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
}

/**
 * Chi tiết một đợt bí tích — khớp `frmBiTichChiTiet.cs`: Mô tả/Ngày/Linh mục/Nơi bí tích ở
 * đầu form, danh sách người nhận (lưới `gxBiTichChiTiet1`) bên dưới. Xem
 * docs/superpowers/specs/man-hinh/so-bi-tich.md.
 */
export function DotBiTichDetail({ id, loaiBiTich, onTieuDe, onDaLuu }: Props) {
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

  useEffect(() => {
    if (!id) {
      onTieuDe?.(`Đợt bí tích mới — ${TEN_LOAI[loaiBiTich]}`)
      return
    }
    setDangTai(true)
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
  }, [id, loaiBiTich, onTieuDe])

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
    <TrangThaiTai dangTai={dangTai} loi={loi} onThuLai={() => id && api.dotBiTich.chiTiet(id).then(setDot)}>
      <section className="page" style={{ overflowY: 'auto', display: 'block' }}>
        <div className="page-head">
          <h1>{TEN_LOAI[loaiBiTich]} {dot ? `— ${dot.moTa}` : '(đợt mới)'}</h1>
        </div>

        <div className="card glass" style={{ marginBottom: 12 }}>
          <div className="form-grid">
            <div className="field span-2">
              <label htmlFor="dbt-mota">Mô tả</label>
              <input id="dbt-mota" value={moTa} onChange={(e) => setMoTa(e.target.value)} />
            </div>
            <div className="field">
              <label htmlFor="dbt-ngay">Ngày bí tích</label>
              <GxDate id="dbt-ngay" defaultValue={ngayBiTich} onIsoChange={setNgayBiTich} />
            </div>
            <div className="field">
              <label htmlFor="dbt-linhmuc">Linh mục</label>
              <input id="dbt-linhmuc" value={linhMuc} onChange={(e) => setLinhMuc(e.target.value)} />
            </div>
            <div className="field span-2">
              <label htmlFor="dbt-noi">Nơi nhận bí tích</label>
              <input id="dbt-noi" value={noiBiTich} onChange={(e) => setNoiBiTich(e.target.value)} />
            </div>
          </div>
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
                <GxPicker onChon={(gd) => { void themNguoiNhan(gd) }} onBoChon={() => {}} />
              </div>
              {loiNguoiNhan && <p className="hint" role="alert">{loiNguoiNhan}</p>}
              <div style={{ height: 420 }}>
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
                <div className="form-grid" style={{ marginTop: 8 }}>
                  <div className="field">
                    <label htmlFor="nn-so">{NHAN_SO[loaiBiTich]}</label>
                    <input id="nn-so" value={soBiTich} onChange={(e) => setSoBiTich(e.target.value)} />
                  </div>
                  {CO_NGUOI_DO_DAU[loaiBiTich] && (
                    <div className="field">
                      <label htmlFor="nn-dodau">Người đỡ đầu</label>
                      <input id="nn-dodau" value={nguoiDoDau} onChange={(e) => setNguoiDoDau(e.target.value)} />
                    </div>
                  )}
                  <div className="field span-2">
                    <label htmlFor="nn-ghichu">Ghi chú</label>
                    <input id="nn-ghichu" value={ghiChu} onChange={(e) => setGhiChu(e.target.value)} />
                  </div>
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
