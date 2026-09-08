import { useEffect, useState, type CSSProperties } from 'react'
import { api, LoiXungDot } from '../api/client'
import type {
  ChuyenLopXemTruoc, GiaoDanTimKiem, GiaoLyVienLop, HocVienLopGiaoLy, KhoiGiaoLy, LopGiaoLy,
  NhapHocVienXemTruoc,
} from '../api/types'
import { GxGrid } from '../components/GxGrid'
import { GxField, GxInline } from '../components/GxField'
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
 * "Chuyển lớp" (`frmChuyenLop.cs`) và "Nhập học viên hàng loạt" từ Excel
 * (`frmImportHocVien.cs`) — hoãn từ commit d4c4b27, làm ở lượt "giao-ly-2-quy-tac-11" — xem
 * can-review-sau.md và <see cref="GiaoLyService.ChuyenLop"/>/`NhapHocVienGiaoLyService`.
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

  // --- "Chuyển lớp" hàng loạt (frmChuyenLop.cs) ---
  const [dangMoChuyenLop, setDangMoChuyenLop] = useState(false)
  const [daChonCL, setDaChonCL] = useState<Set<string>>(new Set())
  const [danhMucKhoi, setDanhMucKhoi] = useState<KhoiGiaoLy[] | null>(null)
  const [khoiDichId, setKhoiDichId] = useState('')
  const [namDich, setNamDich] = useState<number | ''>('')
  const [lopDsDich, setLopDsDich] = useState<LopGiaoLy[]>([])
  const [lopDichId, setLopDichId] = useState('')
  const [xemTruocCL, setXemTruocCL] = useState<ChuyenLopXemTruoc | null>(null)
  const [dangXemTruocCL, setDangXemTruocCL] = useState(false)
  const [dangGhiCL, setDangGhiCL] = useState(false)
  const [loiCL, setLoiCL] = useState<string | null>(null)
  const [xongCL, setXongCL] = useState<string | null>(null)

  // --- "Nhập học viên hàng loạt" từ Excel (frmImportHocVien.cs) ---
  const [dangMoNhapHV, setDangMoNhapHV] = useState(false)
  const [tepNhapHV, setTepNhapHV] = useState<File | null>(null)
  const [xemTruocHV, setXemTruocHV] = useState<NhapHocVienXemTruoc | null>(null)
  const [dangXemTruocHV, setDangXemTruocHV] = useState(false)
  const [dangGhiHV, setDangGhiHV] = useState(false)
  const [loiHV2, setLoiHV2] = useState<string | null>(null)
  const [xongHV, setXongHV] = useState<string | null>(null)

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

  // --- "Chuyển lớp" hàng loạt (frmChuyenLop.cs) ---

  function moChuyenLop() {
    setDangMoChuyenLop(true)
    setDaChonCL(new Set())
    setKhoiDichId(''); setNamDich(''); setLopDsDich([]); setLopDichId('')
    setXemTruocCL(null); setLoiCL(null); setXongCL(null)
    if (!danhMucKhoi) api.giaoLy.khoi().then(setDanhMucKhoi).catch(() => {})
  }

  function toggleChonCL(chiTietId: string) {
    setXemTruocCL(null)
    setDaChonCL((s) => {
      const moi = new Set(s)
      if (moi.has(chiTietId)) moi.delete(chiTietId); else moi.add(chiTietId)
      return moi
    })
  }

  // Khối/Năm đích thay đổi → tải lại danh mục Lớp đích (đúng cbKhoi_SelectedIndexChanged /
  // cbNam_SelectedIndexChanged của frmChuyenLop.cs — Lớp đích không giới hạn trong khối/lớp
  // đang xem, có thể chuyển sang BẤT KỲ lớp nào của giáo xứ).
  useEffect(() => {
    setLopDichId(''); setXemTruocCL(null)
    if (!khoiDichId) { setLopDsDich([]); return }
    api.giaoLy.lop(khoiDichId, namDich === '' ? null : namDich).then(setLopDsDich).catch(() => setLopDsDich([]))
  }, [khoiDichId, namDich])

  async function xemTruocChuyenLop() {
    setLoiCL(null); setXongCL(null)
    if (daChonCL.size === 0) { setLoiCL('Hãy chọn ít nhất 1 học viên để chuyển lớp'); return }
    if (!lopDichId) { setLoiCL('Hãy chọn lớp giáo lý đích'); return }
    setDangXemTruocCL(true)
    try {
      setXemTruocCL(await api.giaoLy.xemTruocChuyenLop([...daChonCL], lopDichId))
    } catch (e) {
      setLoiCL(e instanceof Error ? e.message : 'Không xem trước được, thử lại sau.')
    } finally {
      setDangXemTruocCL(false)
    }
  }

  async function xacNhanChuyenLop() {
    if (!lop || !xemTruocCL) return
    setDangGhiCL(true); setLoiCL(null)
    try {
      const kq = await api.giaoLy.chuyenLop([...daChonCL], lopDichId)
      setXongCL(`Đã chuyển ${kq.soLuongDaChuyen} học viên sang lớp "${xemTruocCL.tenLopDich}". ` +
        'Học viên vẫn còn trong danh sách lớp này (bản gốc không xoá khỏi lớp nguồn khi "chuyển lớp").')
      setXemTruocCL(null); setDaChonCL(new Set())
      taiHocVien(lop.id)
    } catch (e) {
      setLoiCL(e instanceof Error ? e.message : 'Chuyển lớp thất bại, thử lại sau.')
    } finally {
      setDangGhiCL(false)
    }
  }

  // --- "Nhập học viên hàng loạt" từ Excel (frmImportHocVien.cs) ---

  function moNhapHV() {
    setDangMoNhapHV(true)
    setTepNhapHV(null); setXemTruocHV(null); setLoiHV2(null); setXongHV(null)
  }

  function chonTepNhapHV(f: File | null) {
    setTepNhapHV(f); setXemTruocHV(null); setLoiHV2(null); setXongHV(null)
  }

  async function xemTruocNhapHV() {
    if (!lop || !tepNhapHV) return
    setDangXemTruocHV(true); setLoiHV2(null); setXongHV(null)
    try {
      const kq = await api.giaoLy.xemTruocNhapHocVien(lop.id, tepNhapHV)
      setXemTruocHV(kq)
      if (!kq.tepHopLe) setLoiHV2(kq.loiTep)
    } catch (e) {
      setLoiHV2(e instanceof Error ? e.message : 'Không xem trước được, thử lại sau.')
    } finally {
      setDangXemTruocHV(false)
    }
  }

  async function xacNhanNhapHV() {
    if (!lop || !tepNhapHV) return
    setDangGhiHV(true); setLoiHV2(null)
    try {
      const kq = await api.giaoLy.nhapHocVien(lop.id, tepNhapHV)
      setXongHV(`Đã nhập ${kq.soDaNhap} học viên vào lớp, bỏ qua ${kq.soBiBoQua} dòng.`)
      setXemTruocHV(null); setTepNhapHV(null)
      taiHocVien(lop.id)
    } catch (e) {
      setLoiHV2(e instanceof Error ? e.message : 'Nhập học viên thất bại, thử lại sau.')
    } finally {
      setDangGhiHV(false)
    }
  }

  return (
    <TrangThaiTai dangTai={dangTai} loi={loi} onThuLai={() => id && api.giaoLy.lop(khoiId, null).then((ds) => setLop(ds.find((l) => l.id === id) ?? null))}>
      <section className="page" style={{ overflowY: 'auto', display: 'block' }}>
        <div className="page-head">
          <h1>{lop ? lop.tenLop : 'Lớp giáo lý mới'}</h1>
        </div>

        {/* Cùng lý do đổi `.form-grid`/`.field` sang `.frow`/`GxField` như `HoiDoanDetail.tsx`
            (rà thêm theo yêu cầu người dùng 2026-09-08 — "các màn hình mới migrate đêm qua"):
            `.form-grid`/`.field span-2` không có CSS nào định nghĩa, `.field` là lớp của hàng
            lọc (`.filters-bar .field`) dùng nhầm sang đây, và các `<input>` thiếu `type="text"`
            nên không khớp style ô nhập chung — xem chú thích dài ở `HoiDoanDetail.tsx`. */}
        <div className="card glass" style={{ marginBottom: 12 }}>
          <GxField label="Tên lớp" id="lgl-ten">
            <input id="lgl-ten" type="text" value={tenLop} onChange={(e) => setTenLop(e.target.value)} />
          </GxField>
          <GxField label="Năm" id="lgl-nam">
            <input id="lgl-nam" type="number" value={nam} onChange={(e) => setNam(e.target.value === '' ? '' : Number(e.target.value))}
              style={{ maxWidth: 100 }} />
            <GxInline>Phòng học</GxInline>
            <input aria-label="Phòng học" id="lgl-phonghoc" type="text" value={phongHoc} onChange={(e) => setPhongHoc(e.target.value)} />
          </GxField>
          <GxField label="Ghi chú" id="lgl-ghichu">
            <input id="lgl-ghichu" type="text" value={ghiChu} onChange={(e) => setGhiChu(e.target.value)} />
          </GxField>
          {loiLuu && <p className="hint" role="alert">{loiLuu}</p>}
          <div className="cmdbar">
            {lop && (
              <button type="button" className="btn" disabled={dangXoa} onClick={() => setXacNhanXoa(true)}>
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
                <button type="button" className="btn" onClick={moChuyenLop} disabled={!hocVien || hocVien.length === 0}>
                  Chuyển lớp
                </button>
                <button type="button" className="btn" onClick={moNhapHV}>Nhập học viên</button>
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

            {dangMoChuyenLop && (
              <div className="card glass" style={{ marginBottom: 12 }}>
                <div className="cmdbar" style={{ marginBottom: 8 }}>
                  <b>Chuyển lớp hàng loạt</b>
                  <div className="spacer" />
                  <button type="button" className="btn" onClick={() => setDangMoChuyenLop(false)}>Đóng</button>
                </div>
                {xongCL && <p className="hint" style={{ color: 'var(--mint-ink)' }}>{xongCL}</p>}
                <p className="muted" style={{ fontSize: 12.5, marginTop: 0 }}>
                  Chọn học viên muốn chuyển, rồi chọn Khối/Năm/Lớp đích. Đúng hành vi bản gốc
                  (frmChuyenLop.cs): học viên vẫn còn trong lớp hiện tại sau khi "chuyển lớp"
                  (không tự xoá khỏi lớp nguồn); học viên đã có sẵn trong lớp đích bị bỏ qua.
                </p>
                <div className="bang-chon-cuon glass" style={{ maxHeight: 240 }}>
                  <table className="bang-chon">
                    <thead>
                      <tr>
                        <th style={{ width: 40 }} />
                        <th>Họ tên</th>
                        <th>Ngày sinh</th>
                      </tr>
                    </thead>
                    <tbody>
                      {(hocVien ?? []).map((hv) => (
                        <tr key={hv.chiTietId}>
                          <td>
                            <input type="checkbox" checked={daChonCL.has(hv.chiTietId)}
                              onChange={() => toggleChonCL(hv.chiTietId)}
                              aria-label={`Chọn ${hv.hoTen}`} />
                          </td>
                          <td>{[hv.tenThanh, hv.hoTen].filter(Boolean).join(' ')}</td>
                          <td>{hv.ngaySinh ?? ''}</td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
                <div className="filters-bar glass" style={{ marginTop: 8, flexWrap: 'wrap', gap: 12 }}>
                  <span className="count-pill"><b>{daChonCL.size}</b> đã chọn</span>
                  <div className="field">
                    <label htmlFor="cl-khoi">Khối đích</label>
                    <select id="cl-khoi" value={khoiDichId} onChange={(e) => setKhoiDichId(e.target.value)}>
                      <option value="">— Chọn khối —</option>
                      {(danhMucKhoi ?? []).map((k) => <option key={k.id} value={k.id}>{k.tenKhoi}</option>)}
                    </select>
                  </div>
                  <div className="field">
                    <label htmlFor="cl-nam">Năm</label>
                    <input id="cl-nam" type="number" style={{ maxWidth: 100 }} value={namDich}
                      onChange={(e) => setNamDich(e.target.value === '' ? '' : Number(e.target.value))} />
                  </div>
                  <div className="field">
                    <label htmlFor="cl-lop">Lớp đích</label>
                    <select id="cl-lop" value={lopDichId} onChange={(e) => { setLopDichId(e.target.value); setXemTruocCL(null) }} disabled={lopDsDich.length === 0}>
                      <option value="">— Chọn lớp —</option>
                      {lopDsDich.map((l) => <option key={l.id} value={l.id}>{l.tenLop}</option>)}
                    </select>
                  </div>
                </div>
                {loiCL && <p className="hint" role="alert">{loiCL}</p>}
                <div className="cmdbar" style={{ marginTop: 8, justifyContent: 'flex-end' }}>
                  {xemTruocCL ? (
                    <>
                      <span>
                        Sẽ chuyển <b>{xemTruocCL.soLuongSeChuyen}</b> học viên từ lớp
                        "{xemTruocCL.tenLopNguon}" sang lớp "{xemTruocCL.tenLopDich}" (khối
                        {' '}{xemTruocCL.tenKhoiDich}{xemTruocCL.namDich ? `, năm ${xemTruocCL.namDich}` : ''}).
                        {xemTruocCL.soLuongDaCoODichRoi > 0 &&
                          ` Bỏ qua ${xemTruocCL.soLuongDaCoODichRoi} học viên đã có sẵn ở lớp đích.`}
                      </span>
                      <button type="button" className="btn" onClick={() => setXemTruocCL(null)} disabled={dangGhiCL}>Huỷ</button>
                      <button type="button" className="btn btn-primary" onClick={() => { void xacNhanChuyenLop() }} disabled={dangGhiCL}>
                        {dangGhiCL ? 'Đang chuyển…' : 'Xác nhận chuyển'}
                      </button>
                    </>
                  ) : (
                    <button type="button" className="btn btn-primary" onClick={() => { void xemTruocChuyenLop() }} disabled={dangXemTruocCL}>
                      {dangXemTruocCL ? 'Đang xem trước…' : 'Xem trước & chuyển lớp'}
                    </button>
                  )}
                </div>
              </div>
            )}

            {dangMoNhapHV && (
              <div className="card glass" style={{ marginBottom: 12 }}>
                <div className="cmdbar" style={{ marginBottom: 8 }}>
                  <b>Nhập học viên hàng loạt từ Excel</b>
                  <div className="spacer" />
                  <button type="button" className="btn" onClick={() => setDangMoNhapHV(false)}>Đóng</button>
                </div>
                {xongHV && <p className="hint" style={{ color: 'var(--mint-ink)' }}>{xongHV}</p>}
                <p className="muted" style={{ fontSize: 12.5, marginTop: 0 }}>
                  Cột bắt buộc: Họ tên, Phái, Ngày sinh (dd/MM/yyyy). Cột tuỳ chọn: Mã GD, Tên
                  thánh, Giáo họ, Ghi chú, Đã học xong.{' '}
                  <button type="button" className="btn" style={{ padding: '2px 8px', fontSize: 12.5 }}
                    onClick={() => { void api.giaoLy.mauExcelNhapHocVien() }}>
                    Tải mẫu Excel
                  </button>
                </p>
                <div className="filters-bar glass" style={{ gap: 12 }}>
                  <input type="file" accept=".xlsx" data-testid="chon-tep-hoc-vien"
                    onChange={(e) => chonTepNhapHV(e.target.files?.[0] ?? null)} />
                  <button type="button" className="btn" disabled={!tepNhapHV || dangXemTruocHV}
                    onClick={() => { void xemTruocNhapHV() }}>
                    {dangXemTruocHV ? 'Đang xem trước…' : 'Xem trước'}
                  </button>
                </div>
                {loiHV2 && <p className="hint" role="alert">{loiHV2}</p>}
                {xemTruocHV && xemTruocHV.tepHopLe && (
                  <>
                    <p style={{ fontSize: 12.5 }}>
                      Sẽ nhập <b>{xemTruocHV.soSeNhap}</b> học viên, bỏ qua{' '}
                      <b>{xemTruocHV.soBiBoQua}</b> dòng lỗi.
                    </p>
                    <div className="bang-chon-cuon glass" style={{ maxHeight: 280 }}>
                      <table className="bang-chon">
                        <thead>
                          <tr>
                            <th>Dòng</th><th>Họ tên</th><th>Phái</th><th>Ngày sinh</th>
                            <th>Giáo họ</th><th>Trạng thái</th>
                          </tr>
                        </thead>
                        <tbody>
                          {xemTruocHV.dong.map((d) => (
                            <tr key={d.soDong}>
                              <td>{d.soDong}</td>
                              <td>{[d.tenThanh, d.hoTen].filter(Boolean).join(' ')}</td>
                              <td>{d.phai ?? ''}</td>
                              <td>{d.ngaySinhHienThi ?? ''}</td>
                              <td>{d.giaoHo ?? ''}</td>
                              <td style={d.loi ? { color: 'var(--rose-ink)' } : undefined}>
                                {d.loi ?? (d.laGiaoDanMoi ? 'Sẽ tạo giáo dân mới' : 'Sẽ dùng giáo dân có sẵn')}
                              </td>
                            </tr>
                          ))}
                        </tbody>
                      </table>
                    </div>
                    <div className="cmdbar" style={{ marginTop: 8, justifyContent: 'flex-end' }}>
                      <button type="button" className="btn btn-primary" disabled={dangGhiHV || xemTruocHV.soSeNhap === 0}
                        onClick={() => { void xacNhanNhapHV() }}>
                        {dangGhiHV ? 'Đang nhập…' : `Xác nhận nhập ${xemTruocHV.soSeNhap} học viên`}
                      </button>
                    </div>
                  </>
                )}
              </div>
            )}

            {dongChonHV && (
              <div className="card glass" style={{ marginBottom: 12 }}>
                <b>{(dongChonHV.tenThanh ? dongChonHV.tenThanh + ' ' : '') + dongChonHV.hoTen}</b>
                <div style={{ marginTop: 8 }}>
                  <GxField label="Số thứ tự" id="hv-stt">
                    <input id="hv-stt" type="number" value={soThuTuSua} onChange={(e) => setSoThuTuSua(e.target.value)}
                      style={{ maxWidth: 100 }} />
                    <label className="toggle" style={{ marginLeft: 12 }}>
                      <input type="checkbox" checked={hoanThanhSua} onChange={(e) => setHoanThanhSua(e.target.checked)} />
                      Hoàn thành khóa học
                    </label>
                  </GxField>
                  <GxField label="Ghi chú" id="hv-ghichu">
                    <input id="hv-ghichu" type="text" value={ghiChuGLySua} onChange={(e) => setGhiChuGLySua(e.target.value)} />
                  </GxField>
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
