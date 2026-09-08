import { useEffect, useState, type CSSProperties } from 'react'
import { api, LoiXungDot } from '../api/client'
import type { GiaoDanTimKiem, GiaoLyVienLop, HocVienLopGiaoLy, LopGiaoLy } from '../api/types'
import { GxGrid } from '../components/GxGrid'
import { GxPicker } from '../components/GxPicker'
import { TrangThaiTai } from '../components/TrangThaiTai'
import { cotGiaoLyVien, cotHocVienGiaoLy } from '../cot/cotGiaoLy'

type Props = {
  id: string | null
  khoiId: string
  namMoi?: number
  onTieuDe?: (ten: string) => void
  /** Gọi khi XOÁ lớp thành công — đóng thẻ này (không có "quay về khối" tự động vì tên thẻ
   * khối cha có thể còn giữ khoá thẻ nháp cũ nếu vừa tạo mới trong cùng phiên, xem
   * can-review-sau.md). Lưu (thêm/sửa) KHÔNG điều hướng đi đâu — ở lại lớp để tiếp tục thêm
   * học viên/giáo lý viên, khác hội đoàn (nơi không có màn hình con cần ở lại). */
  onXoaThanhCong?: () => void
  /** Nút "+" của `GxPicker` danh sách học viên/giáo lý viên — mở một thẻ "Giáo dân mới" TÁCH
   * BIỆT, tạo xong tự đóng lại rồi thêm luôn người vừa tạo (giống hệt `onChon`) — cùng cơ chế
   * đã dùng ở `GiaDinhDetail`, xem `GxPicker.tsx`, `App.moChiTietGiaoDan`, can-review-sau.md
   * mục 19. */
  moGiaoDanMoiChoPicker?: (onTaoXong: (gd: GiaoDanTimKiem) => void) => void
}

/**
 * Chi tiết một lớp giáo lý (frmLopGiaoLy.cs, tên lớp C# là `frmLopGiaoLyList` — dễ nhầm với
 * danh mục, xem giao-ly.md) — khối "Tên lớp/Năm/Phòng học/Ghi chú" ở trên, danh sách học viên
 * và giáo lý viên bên dưới. Xem docs/superpowers/specs/man-hinh/giao-ly.md.
 *
 * KHÔNG migrate (thu hẹp phạm vi có chủ đích, xem mục 8 của spec): nút "Chuyển lớp"
 * (`frmChuyenLop.cs`), "Nhập từ Excel"/"Xem mẫu Excel" (`frmImportHocVien.cs`).
 */
export function LopGiaoLyDetail({ id, khoiId, namMoi, onTieuDe, onXoaThanhCong, moGiaoDanMoiChoPicker }: Props) {
  const [lop, setLop] = useState<LopGiaoLy | null>(null)
  const [dangTai, setDangTai] = useState(!!id)
  const [loi, setLoi] = useState<string | null>(null)

  const [tenLop, setTenLop] = useState('')
  const [nam, setNam] = useState<number | ''>(namMoi ?? new Date().getFullYear())
  const [phongHoc, setPhongHoc] = useState('')
  const [ghiChu, setGhiChu] = useState('')
  const [dangLuu, setDangLuu] = useState(false)
  const [loiLuu, setLoiLuu] = useState<string | null>(null)

  const [xacNhanXoa, setXacNhanXoa] = useState(false)
  const [dangXoa, setDangXoa] = useState(false)

  const [hocVien, setHocVien] = useState<HocVienLopGiaoLy[] | null>(null)
  const [loiHV, setLoiHV] = useState<string | null>(null)
  const [dongChonHV, setDongChonHV] = useState<HocVienLopGiaoLy | null>(null)
  const [soThuTuSua, setSoThuTuSua] = useState('')
  const [hoanThanhSua, setHoanThanhSua] = useState(false)
  const [ghiChuGLySua, setGhiChuGLySua] = useState('')
  const [dangLuuHV, setDangLuuHV] = useState(false)

  const [giaoLyVien, setGiaoLyVien] = useState<GiaoLyVienLop[] | null>(null)
  const [loiGLV, setLoiGLV] = useState<string | null>(null)
  const [dangLuuGLV, setDangLuuGLV] = useState(false)

  function taiHocVien(lopId: string) {
    api.giaoLy.hocVien(lopId).then(setHocVien)
      .catch((e: unknown) => setLoiHV(e instanceof Error ? e.message : String(e)))
  }
  function taiGiaoLyVien(lopId: string) {
    api.giaoLy.giaoLyVien(lopId).then(setGiaoLyVien)
      .catch((e: unknown) => setLoiGLV(e instanceof Error ? e.message : String(e)))
  }

  useEffect(() => {
    if (!id) {
      onTieuDe?.('Lớp giáo lý mới')
      return
    }
    setDangTai(true)
    api.giaoLy.lop(khoiId, null)
      .then((ds) => {
        const dong = ds.find((l) => l.id === id)
        if (!dong) { setLoi('Không tìm thấy lớp giáo lý này — có thể đã bị xoá.'); return }
        setLop(dong)
        setTenLop(dong.tenLop)
        setNam(dong.nam ?? '')
        setPhongHoc(dong.phongHoc ?? '')
        setGhiChu(dong.ghiChu ?? '')
        onTieuDe?.(dong.tenLop)
        taiHocVien(id)
        taiGiaoLyVien(id)
      })
      .catch((e: unknown) => setLoi(e instanceof Error ? e.message : String(e)))
      .finally(() => setDangTai(false))
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [id, khoiId])

  function chonHocVien(hv: HocVienLopGiaoLy | null) {
    setDongChonHV(hv)
    setSoThuTuSua(hv?.soThuTu != null ? String(hv.soThuTu) : '')
    setHoanThanhSua(hv?.hoanThanh ?? false)
    setGhiChuGLySua(hv?.ghiChuGLy ?? '')
    setLoiHV(null)
  }

  async function luuLop() {
    if (tenLop.trim() === '') { setLoiLuu('Hãy nhập tên lớp giáo lý'); return }
    setDangLuu(true)
    setLoiLuu(null)
    const than = { tenLop: tenLop.trim(), nam: nam === '' ? null : Number(nam), phongHoc: phongHoc || null, ghiChu: ghiChu || null, rowVersion: lop?.rowVersion ?? null }
    try {
      if (!lop) {
        const moi = await api.giaoLy.themLop(khoiId, than)
        const ds = await api.giaoLy.lop(khoiId, null)
        const dong = ds.find((l) => l.id === moi.id)!
        setLop(dong)
        onTieuDe?.(dong.tenLop)
        taiHocVien(dong.id)
        taiGiaoLyVien(dong.id)
      } else {
        await api.giaoLy.suaLop(lop.id, than)
        const ds = await api.giaoLy.lop(khoiId, null)
        const dong = ds.find((l) => l.id === lop.id)!
        setLop(dong)
        onTieuDe?.(dong.tenLop)
      }
    } catch (e) {
      setLoiLuu(e instanceof LoiXungDot ? e.message : e instanceof Error ? e.message : 'Lưu thất bại, thử lại sau.')
    } finally {
      setDangLuu(false)
    }
  }

  async function xoaLop() {
    if (!lop) return
    setDangXoa(true)
    try {
      await api.giaoLy.xoaLop(lop.id)
      onXoaThanhCong?.()
    } catch (e) {
      setLoiLuu(e instanceof Error ? e.message : 'Xoá thất bại, thử lại sau.')
      setDangXoa(false)
      setXacNhanXoa(false)
    }
  }

  async function themHocVien(gd: GiaoDanTimKiem) {
    if (!lop) return
    setDangLuuHV(true)
    setLoiHV(null)
    try {
      await api.giaoLy.themHocVien(lop.id, gd.id)
      taiHocVien(lop.id)
    } catch (e) {
      setLoiHV(e instanceof Error ? e.message : 'Không thêm được, thử lại sau.')
    } finally {
      setDangLuuHV(false)
    }
  }

  async function luuHocVien() {
    if (!lop || !dongChonHV) return
    setDangLuuHV(true)
    setLoiHV(null)
    try {
      await api.giaoLy.suaHocVien(dongChonHV.chiTietId, {
        soThuTu: soThuTuSua === '' ? null : Number(soThuTuSua),
        hoanThanh: hoanThanhSua, ghiChuGLy: ghiChuGLySua || null, rowVersion: dongChonHV.rowVersion,
      })
      taiHocVien(lop.id)
      chonHocVien(null)
    } catch (e) {
      setLoiHV(e instanceof LoiXungDot ? e.message : e instanceof Error ? e.message : 'Không lưu được, thử lại sau.')
    } finally {
      setDangLuuHV(false)
    }
  }

  async function xoaHocVien() {
    if (!lop || !dongChonHV) return
    // Nguyên văn xác nhận gốc frmLopGiaoLy.cs:421 ("Bạn có chắc muốn xóa?" — không nêu tên,
    // khác hội đoàn có nêu tên).
    if (!window.confirm('Bạn có chắc muốn xóa?')) return
    setDangLuuHV(true)
    try {
      await api.giaoLy.xoaHocVien(dongChonHV.chiTietId)
      taiHocVien(lop.id)
      chonHocVien(null)
    } catch (e) {
      setLoiHV(e instanceof Error ? e.message : 'Không xoá được, thử lại sau.')
    } finally {
      setDangLuuHV(false)
    }
  }

  async function themGiaoLyVien(gd: GiaoDanTimKiem) {
    if (!lop) return
    setDangLuuGLV(true)
    setLoiGLV(null)
    try {
      await api.giaoLy.themGiaoLyVien(lop.id, gd.id)
      taiGiaoLyVien(lop.id)
    } catch (e) {
      setLoiGLV(e instanceof Error ? e.message : 'Không thêm được, thử lại sau.')
    } finally {
      setDangLuuGLV(false)
    }
  }

  // Khớp bản gốc: KHÔNG hỏi xác nhận khi xoá giáo lý viên (`gxAddEdit2_DeleteClick`, xem
  // giao-ly.md mục 4) — khác xoá học viên/khối/lớp.
  async function xoaGiaoLyVien(glvId: string) {
    if (!lop) return
    setDangLuuGLV(true)
    try {
      await api.giaoLy.xoaGiaoLyVien(glvId)
      taiGiaoLyVien(lop.id)
    } catch (e) {
      setLoiGLV(e instanceof Error ? e.message : 'Không xoá được, thử lại sau.')
    } finally {
      setDangLuuGLV(false)
    }
  }

  return (
    <TrangThaiTai dangTai={dangTai} loi={loi} onThuLai={() => id && api.giaoLy.lop(khoiId, null).then((ds) => setLop(ds.find((l) => l.id === id) ?? null))}>
      <section className="page" style={{ overflowY: 'auto', display: 'block' }}>
        <div className="page-head">
          <h1>{lop ? lop.tenLop : 'Lớp giáo lý mới'}</h1>
        </div>

        <div className="card glass" style={{ marginBottom: 12 }}>
          <div className="form-grid">
            <div className="field span-2">
              <label htmlFor="lgl-ten">Tên lớp</label>
              <input id="lgl-ten" value={tenLop} onChange={(e) => setTenLop(e.target.value)} />
            </div>
            <div className="field">
              <label htmlFor="lgl-nam">Năm</label>
              <input id="lgl-nam" type="number" value={nam} onChange={(e) => setNam(e.target.value === '' ? '' : Number(e.target.value))} />
            </div>
            <div className="field">
              <label htmlFor="lgl-phonghoc">Phòng học</label>
              <input id="lgl-phonghoc" value={phongHoc} onChange={(e) => setPhongHoc(e.target.value)} />
            </div>
            <div className="field span-2">
              <label htmlFor="lgl-ghichu">Ghi chú</label>
              <input id="lgl-ghichu" value={ghiChu} onChange={(e) => setGhiChu(e.target.value)} />
            </div>
          </div>
          {loiLuu && <p className="hint" role="alert">{loiLuu}</p>}
          <div className="cmdbar">
            {lop && (
              <button type="button" className="btn btn-danger" disabled={dangXoa} onClick={() => setXacNhanXoa(true)}>
                Xóa lớp
              </button>
            )}
            <div className="spacer" />
            <button type="button" className="btn btn-primary" disabled={dangLuu} onClick={() => { void luuLop() }}>
              {dangLuu ? 'Đang lưu…' : 'Cập nhật'}
            </button>
          </div>
          {xacNhanXoa && (
            <div className="card glass" role="alertdialog" style={{ marginTop: 8 }}>
              <p>Bạn có chắc muốn xóa lớp giáo lý này? Danh sách học sinh thuộc lớp giáo lý này sẽ bị xóa theo.</p>
              <div className="cmdbar">
                <button type="button" className="btn" disabled={dangXoa} onClick={() => setXacNhanXoa(false)}>Hủy bỏ</button>
                <div className="spacer" />
                <button type="button" className="btn btn-danger" disabled={dangXoa} onClick={() => { void xoaLop() }}>
                  {dangXoa ? 'Đang xoá…' : 'Xóa'}
                </button>
              </div>
            </div>
          )}
        </div>

        {lop && (
          <>
            <div className="card glass" style={{ marginBottom: 12 }}>
              <div className="cmdbar" style={{ marginBottom: 8 }}>
                <b>Danh sách học viên ({hocVien?.length ?? 0})</b>
                <div className="spacer" />
                <GxPicker onChon={(gd) => { void themHocVien(gd) }} onBoChon={() => {}}
                  onThemMoi={moGiaoDanMoiChoPicker ? () => moGiaoDanMoiChoPicker((gd) => { void themHocVien(gd) }) : undefined} />
              </div>
              {loiHV && <p className="hint" role="alert">{loiHV}</p>}
              {/* `.fixed-h-grid` (qlgx.css) — KHÔNG dùng inline `style={{ height }}` đơn thuần
                  trên div bọc: gây "table-card cao ~3px" trong trang cuộn-cả-trang, xem chú
                  thích dài trong qlgx.css. */}
              <div className="fixed-h-grid" style={{ '--fixed-h-grid': '320px' } as CSSProperties}>
                <GxGrid<HocVienLopGiaoLy>
                  columnDefs={cotHocVienGiaoLy}
                  rowData={hocVien ?? []}
                  layId={(d) => d.chiTietId}
                  onChon={chonHocVien}
                />
              </div>
            </div>

            {dongChonHV && (
              <div className="card glass" style={{ marginBottom: 12 }}>
                <b>{(dongChonHV.tenThanh ? dongChonHV.tenThanh + ' ' : '') + dongChonHV.hoTen}</b>
                <div className="form-grid" style={{ marginTop: 8 }}>
                  <div className="field">
                    <label htmlFor="hv-stt">Số thứ tự</label>
                    <input id="hv-stt" type="number" value={soThuTuSua} onChange={(e) => setSoThuTuSua(e.target.value)} />
                  </div>
                  <div className="field">
                    <label style={{ display: 'flex', alignItems: 'center', gap: 6, marginTop: 18 }}>
                      <input type="checkbox" checked={hoanThanhSua} onChange={(e) => setHoanThanhSua(e.target.checked)} />
                      Hoàn thành khóa học
                    </label>
                  </div>
                  <div className="field span-2">
                    <label htmlFor="hv-ghichu">Ghi chú</label>
                    <input id="hv-ghichu" value={ghiChuGLySua} onChange={(e) => setGhiChuGLySua(e.target.value)} />
                  </div>
                </div>
                <div className="cmdbar">
                  <button type="button" className="btn btn-danger" disabled={dangLuuHV} onClick={() => { void xoaHocVien() }}>
                    Xóa khỏi lớp
                  </button>
                  <div className="spacer" />
                  <button type="button" className="btn btn-primary" disabled={dangLuuHV} onClick={() => { void luuHocVien() }}>
                    {dangLuuHV ? 'Đang lưu…' : 'Lưu'}
                  </button>
                </div>
              </div>
            )}

            <div className="card glass">
              <div className="cmdbar" style={{ marginBottom: 8 }}>
                <b>Giáo lý viên ({giaoLyVien?.length ?? 0})</b>
                <div className="spacer" />
                <GxPicker onChon={(gd) => { void themGiaoLyVien(gd) }} onBoChon={() => {}}
                  onThemMoi={moGiaoDanMoiChoPicker ? () => moGiaoDanMoiChoPicker((gd) => { void themGiaoLyVien(gd) }) : undefined} />
              </div>
              {loiGLV && <p className="hint" role="alert">{loiGLV}</p>}
              <div className="fixed-h-grid" style={{ '--fixed-h-grid': '180px' } as CSSProperties}>
                <GxGrid<GiaoLyVienLop>
                  columnDefs={cotGiaoLyVien}
                  rowData={giaoLyVien ?? []}
                  layId={(d) => d.id}
                  menuChuotPhai={[
                    { nhan: 'Xoá khỏi lớp', chay: (d) => { void xoaGiaoLyVien(d.id) } },
                  ]}
                />
              </div>
              {dangLuuGLV && <p className="muted" style={{ fontSize: 12.5 }}>Đang xử lý…</p>}
            </div>
          </>
        )}
      </section>
    </TrangThaiTai>
  )
}
