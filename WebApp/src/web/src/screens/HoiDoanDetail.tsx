import { useEffect, useState, type CSSProperties } from 'react'
import { api, LoiXungDot } from '../api/client'
import type { GiaoDanTimKiem, HoiDoanQuanLy, ThanhVienHoiDoan } from '../api/types'
import { GxGrid } from '../components/GxGrid'
import { GxDate } from '../components/GxDate'
import { GxField, GxInline } from '../components/GxField'
import { GxPicker } from '../components/GxPicker'
import { TrangThaiTai } from '../components/TrangThaiTai'
import { cotThanhVienHoiDoan } from '../cot/cotHoiDoan'

type Props = {
  id: string | null
  onTieuDe?: (ten: string) => void
  onDaLuu?: () => void
  /** Nút "+" của `GxPicker` danh sách hội viên — mở một thẻ "Giáo dân mới" TÁCH BIỆT, tạo xong
   * tự đóng lại rồi thêm luôn người vừa tạo làm hội viên (giống hệt `onChon`) — cùng cơ chế đã
   * dùng ở `GiaDinhDetail`, xem `GxPicker.tsx`, `App.moChiTietGiaoDan`, can-review-sau.md mục 19. */
  moGiaoDanMoiChoPicker?: (onTaoXong: (gd: GiaoDanTimKiem) => void) => void
}

/**
 * Chi tiết một hội đoàn (frmHoiDoan.cs) — khối "Tên hội đoàn/Thánh bổn mạng/Ngày bổn mạng/Ngày
 * thành lập/Ghi chú" ở trên, danh sách hội viên (lưới `gxGiaoDanList1`) bên dưới. Xem
 * docs/superpowers/specs/man-hinh/hoi-doan-danh-sach.md.
 *
 * Khác biệt cố ý so với bản gốc (ghi trong can-review-sau.md): hội đoàn và hội viên được lưu
 * RIÊNG (mỗi thao tác gọi API ngay), không gộp thành một giao dịch "OK" duy nhất như
 * `gxCommand1_OnOK` — vì bảng trống ở dữ liệu khảo sát nên không có ràng buộc "sửa xong mới
 * lưu" nào cần bảo toàn, và tách riêng giúp không mất dữ liệu nếu người dùng đóng tab giữa
 * chừng. Không migrate y hệt các hộp thoại Yes/No/Cancel phức tạp (kiểm tra hội trưởng duy
 * nhất, ngày không được ở tương lai, trùng tên hội đoàn...) — xem can-review-sau.md.
 */
export function HoiDoanDetail({ id, onTieuDe, onDaLuu, moGiaoDanMoiChoPicker }: Props) {
  const [hd, setHd] = useState<HoiDoanQuanLy | null>(null)
  const [dangTai, setDangTai] = useState(!!id)
  const [loi, setLoi] = useState<string | null>(null)

  const [tenHoiDoan, setTenHoiDoan] = useState('')
  const [thanhBonMang, setThanhBonMang] = useState('')
  const [ngayBonMang, setNgayBonMang] = useState<string | null>(null)
  const [ngayThanhLap, setNgayThanhLap] = useState<string | null>(null)
  const [ghiChu, setGhiChu] = useState('')
  const [dangLuu, setDangLuu] = useState(false)
  const [loiLuu, setLoiLuu] = useState<string | null>(null)

  const [xacNhanXoa, setXacNhanXoa] = useState(false)
  const [dangXoa, setDangXoa] = useState(false)

  const [thanhVien, setThanhVien] = useState<ThanhVienHoiDoan[] | null>(null)
  const [hienCaDaRa, setHienCaDaRa] = useState(false)
  const [dongChonTV, setDongChonTV] = useState<ThanhVienHoiDoan | null>(null)
  const [ngayVaoSua, setNgayVaoSua] = useState<string | null>(null)
  const [ngayRaSua, setNgayRaSua] = useState<string | null>(null)
  const [vaiTroSua, setVaiTroSua] = useState('')
  const [dangLuuTV, setDangLuuTV] = useState(false)
  const [loiTV, setLoiTV] = useState<string | null>(null)

  function taiThanhVien(hoiDoanId: string, chiXemHienTai: boolean) {
    api.hoiDoanQuanLy.thanhVien(hoiDoanId, chiXemHienTai)
      .then(setThanhVien)
      .catch((e: unknown) => setLoiTV(e instanceof Error ? e.message : String(e)))
  }

  // Tái sử dụng ở cả tải lần đầu (effect) lẫn nút "Thử lại" của `TrangThaiTai` — trước đây nút
  // "Thử lại" gọi thẳng một closure rút gọn (`api.hoiDoanQuanLy.danhSach().then(...)`) không hề
  // `setLoi(null)`/`setDangTai(true)`, nên dù tải lại thành công màn hình vẫn đứng yên ở nhánh
  // lỗi của `TrangThaiTai` (rà lại theo yêu cầu người dùng 2026-09-08, review toàn nhánh
  // "Nghiêm trọng #2"). Khuôn đúng lấy từ `GiaoDanDetailPage.tsx`.
  function tai() {
    if (!id) return
    setDangTai(true)
    setLoi(null)
    api.hoiDoanQuanLy.danhSach()
      .then((ds) => {
        const dong = ds.find((h) => h.id === id)
        if (!dong) { setLoi('Không tìm thấy hội đoàn này — có thể đã bị xoá.'); return }
        setHd(dong)
        setTenHoiDoan(dong.tenHoiDoan)
        setThanhBonMang(dong.thanhBonMang ?? '')
        setNgayBonMang(dong.ngayBonMang)
        setNgayThanhLap(dong.ngayThanhLap)
        setGhiChu(dong.ghiChu ?? '')
        onTieuDe?.(dong.tenHoiDoan)
        taiThanhVien(id, !hienCaDaRa)
      })
      .catch((e: unknown) => setLoi(e instanceof Error ? e.message : String(e)))
      .finally(() => setDangTai(false))
  }

  useEffect(() => {
    if (!id) {
      onTieuDe?.('Hội đoàn mới')
      return
    }
    tai()
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [id])

  function chonThanhVien(tv: ThanhVienHoiDoan | null) {
    setDongChonTV(tv)
    setNgayVaoSua(tv?.ngayVaoHoiDoan ?? null)
    setNgayRaSua(tv?.ngayRaHoiDoan ?? null)
    setVaiTroSua(tv?.vaiTro ?? '')
    setLoiTV(null)
  }

  function doiHienCaDaRa(v: boolean) {
    setHienCaDaRa(v)
    if (hd) taiThanhVien(hd.id, !v)
  }

  async function luuHoiDoan() {
    if (tenHoiDoan.trim() === '') {
      setLoiLuu('Vui lòng nhập tên hội đoàn')
      return
    }
    setDangLuu(true)
    setLoiLuu(null)
    const than = {
      tenHoiDoan: tenHoiDoan.trim(), thanhBonMang: thanhBonMang || null,
      ngayBonMang, ngayThanhLap, ghiChu: ghiChu || null,
      rowVersion: hd?.rowVersion ?? null,
    }
    try {
      if (!hd) {
        const moi = await api.hoiDoanQuanLy.them(than)
        const ds = await api.hoiDoanQuanLy.danhSach()
        const dong = ds.find((h) => h.id === moi.id)!
        setHd(dong)
        onTieuDe?.(dong.tenHoiDoan)
        taiThanhVien(dong.id, !hienCaDaRa)
      } else {
        await api.hoiDoanQuanLy.sua(hd.id, than)
        const ds = await api.hoiDoanQuanLy.danhSach()
        const dong = ds.find((h) => h.id === hd.id)!
        setHd(dong)
        onTieuDe?.(dong.tenHoiDoan)
      }
      onDaLuu?.()
    } catch (e) {
      setLoiLuu(e instanceof LoiXungDot ? e.message : e instanceof Error ? e.message : 'Lưu thất bại, thử lại sau.')
    } finally {
      setDangLuu(false)
    }
  }

  async function xoaHoiDoan() {
    if (!hd) return
    setDangXoa(true)
    try {
      await api.hoiDoanQuanLy.xoa(hd.id)
      onDaLuu?.()
    } catch (e) {
      setLoiLuu(e instanceof Error ? e.message : 'Xoá thất bại, thử lại sau.')
      setDangXoa(false)
      setXacNhanXoa(false)
    }
  }

  async function themThanhVien(gd: GiaoDanTimKiem) {
    if (!hd) return
    setDangLuuTV(true)
    setLoiTV(null)
    try {
      await api.hoiDoanQuanLy.themThanhVien(hd.id, {
        giaoDanId: gd.id, ngayVaoHoiDoan: null, ngayRaHoiDoan: null, vaiTro: null,
      })
      taiThanhVien(hd.id, !hienCaDaRa)
      const ds = await api.hoiDoanQuanLy.danhSach()
      setHd(ds.find((h) => h.id === hd.id) ?? hd)
    } catch (e) {
      setLoiTV(e instanceof Error ? e.message : 'Không thêm được, thử lại sau.')
    } finally {
      setDangLuuTV(false)
    }
  }

  async function luuThanhVien() {
    if (!hd || !dongChonTV) return
    setDangLuuTV(true)
    setLoiTV(null)
    try {
      await api.hoiDoanQuanLy.suaThanhVien(dongChonTV.chiTietId, {
        ngayVaoHoiDoan: ngayVaoSua, ngayRaHoiDoan: ngayRaSua,
        vaiTro: vaiTroSua.trim() || null, rowVersion: dongChonTV.rowVersion,
      })
      taiThanhVien(hd.id, !hienCaDaRa)
      chonThanhVien(null)
      const ds = await api.hoiDoanQuanLy.danhSach()
      setHd(ds.find((h) => h.id === hd.id) ?? hd)
    } catch (e) {
      setLoiTV(e instanceof LoiXungDot ? e.message : e instanceof Error ? e.message : 'Không lưu được, thử lại sau.')
    } finally {
      setDangLuuTV(false)
    }
  }

  async function xoaThanhVien() {
    if (!hd || !dongChonTV) return
    if (!window.confirm(
      `Bạn có thực sự muốn giáo dân [${dongChonTV.hoTen}] ra khỏi hội đoàn vĩnh viễn.\n` +
      'Nếu có chọn OK để xóa vĩnh viễn. Nếu không, chọn Hủy rồi tự sửa "Ngày ra hội đoàn" ' +
      'thành hôm nay để chỉ đánh dấu đã ra (vẫn giữ trong lịch sử).')) return
    setDangLuuTV(true)
    try {
      await api.hoiDoanQuanLy.xoaThanhVien(dongChonTV.chiTietId)
      taiThanhVien(hd.id, !hienCaDaRa)
      chonThanhVien(null)
      const ds = await api.hoiDoanQuanLy.danhSach()
      setHd(ds.find((h) => h.id === hd.id) ?? hd)
    } catch (e) {
      setLoiTV(e instanceof Error ? e.message : 'Không xoá được, thử lại sau.')
    } finally {
      setDangLuuTV(false)
    }
  }

  return (
    <TrangThaiTai dangTai={dangTai} loi={loi} onThuLai={tai}>
      <section className="page" style={{ overflowY: 'auto', display: 'block' }}>
        <div className="page-head">
          <h1>{hd ? hd.tenHoiDoan : 'Hội đoàn mới'}</h1>
        </div>

        {/* Góp ý người dùng (2026-09-08, ảnh chụp "Legio Mariae"): khối này trước đây dùng
            `.form-grid`/`.field span-2` — hai lớp KHÔNG HỀ có CSS nào định nghĩa (`grep` "form-
            grid"/"span-2" trong qlgx.css ra rỗng); `.field` thật ra là lớp của HÀNG LỌC
            (`.filters-bar .field`, nhãn+ô cùng một dòng, nhãn co theo đúng độ dài chữ) — dùng
            nhầm sang một form chi tiết khiến mỗi nhãn dài ngắn khác nhau đẩy ô nhập bắt đầu ở
            một vị trí khác nhau (nhãn và ô "không thẳng hàng, không theo lưới" — nguyên văn góp
            ý). Các `<input>` cũng THIẾU `type="text"` nên không khớp selector CSS
            `input[type="text"]` (yêu cầu đúng thuộc tính `type`) — hiện ra bằng đúng kiểu ô nhập
            mặc định của trình duyệt, không bo góc, không cỡ chữ 12px như mọi ô khác trong ứng
            dụng ("mỗi ô một kiểu"). Chuyển sang đúng khuôn `.frow`/`GxField` mà các màn hình
            chính đang dùng: nhãn trái CỐ ĐỊNH 132px, ô phải giãn hết bề ngang còn lại, thẳng
            hàng suốt khối — không phát minh khuôn mới. */}
        <div className="card glass" style={{ marginBottom: 12 }}>
          <GxField label="Tên hội đoàn" id="hd-ten">
            <input id="hd-ten" type="text" value={tenHoiDoan} onChange={(e) => setTenHoiDoan(e.target.value)} />
          </GxField>
          <GxField label="Thánh bổn mạng" id="hd-bonmang">
            <input id="hd-bonmang" type="text" value={thanhBonMang} onChange={(e) => setThanhBonMang(e.target.value)} />
            <GxInline>Ngày bổn mạng</GxInline>
            <GxDate id="hd-ngaybonmang" defaultValue={ngayBonMang} onIsoChange={(iso) => setNgayBonMang(iso || null)}
              style={{ maxWidth: 170, marginLeft: 'auto' }} />
          </GxField>
          <GxField label="Ngày thành lập" id="hd-ngaythanhlap">
            <GxDate id="hd-ngaythanhlap" defaultValue={ngayThanhLap} onIsoChange={(iso) => setNgayThanhLap(iso || null)}
              style={{ maxWidth: 170 }} />
          </GxField>
          <GxField label="Ghi chú" id="hd-ghichu">
            <input id="hd-ghichu" type="text" value={ghiChu} onChange={(e) => setGhiChu(e.target.value)} />
          </GxField>
          {loiLuu && <p className="hint" role="alert">{loiLuu}</p>}
          <div className="cmdbar">
            {hd && (
              // Góp ý người dùng: nút xoá đứng lẻ giữa các ô unstyled trông như điểm nổi bật
              // nhất trang — trong khi Ở NƠI KHÁC (`HoiDoanListPage.tsx`, nút "Xóa" của
              // `GxToolbar`), nút KHỞI ĐỘNG việc xoá luôn trung tính (`.btn` thường, không màu
              // đỏ) — chỉ nút XÁC NHẬN cuối cùng trong hộp thoại mới tô đỏ (`.btn-danger`, xem
              // ngay bên dưới). Đổi nút khởi động này về đúng quy ước đó cho kín đáo, không đổi
              // gì luồng xác nhận hai bước đã có.
              <button type="button" className="btn" disabled={dangXoa}
                onClick={() => setXacNhanXoa(true)}>
                Xóa hội đoàn
              </button>
            )}
            <div className="spacer" />
            <button type="button" className="btn btn-primary" disabled={dangLuu} onClick={() => { void luuHoiDoan() }}>
              {dangLuu ? 'Đang lưu…' : 'Cập nhật'}
            </button>
          </div>
          {xacNhanXoa && (
            <div className="card glass" role="alertdialog" style={{ marginTop: 8 }}>
              {/* Nguyên văn frmHoiDoanList.cs:26: "Bạn có thật sự muốn xóa hội đoàn này!" */}
              <p>Bạn có thật sự muốn xóa hội đoàn này! Toàn bộ danh sách hội viên của hội đoàn cũng sẽ bị xóa theo.</p>
              <div className="cmdbar">
                <button type="button" className="btn" disabled={dangXoa} onClick={() => setXacNhanXoa(false)}>Hủy bỏ</button>
                <div className="spacer" />
                <button type="button" className="btn btn-danger" disabled={dangXoa} onClick={() => { void xoaHoiDoan() }}>
                  {dangXoa ? 'Đang xoá…' : 'Xóa'}
                </button>
              </div>
            </div>
          )}
        </div>

        {hd && (
          <>
            <div className="card glass" style={{ marginBottom: 12 }}>
              <div className="cmdbar" style={{ marginBottom: 8 }}>
                <b>Danh sách hội viên ({thanhVien?.length ?? 0})</b>
                <label style={{ display: 'flex', alignItems: 'center', gap: 4, fontSize: 12.5 }}>
                  <input type="checkbox" checked={hienCaDaRa} onChange={(e) => doiHienCaDaRa(e.target.checked)} />
                  Hiện cả người đã ra khỏi hội đoàn
                </label>
                <div className="spacer" />
                <GxPicker onChon={(gd) => { void themThanhVien(gd) }} onBoChon={() => {}}
                  onThemMoi={moGiaoDanMoiChoPicker ? () => moGiaoDanMoiChoPicker((gd) => { void themThanhVien(gd) }) : undefined} />
              </div>
              {loiTV && <p className="hint" role="alert">{loiTV}</p>}
              {/* `.fixed-h-grid` (qlgx.css) — KHÔNG đổi lại thành inline `style={{ height }}`
                  đơn thuần: đã là lỗi thật "table-card cao ~3px" phát hiện lại 2026-09-08 khi
                  dựng phân hệ Giáo lý, xem chú thích dài trong qlgx.css. */}
              <div className="fixed-h-grid" style={{ '--fixed-h-grid': '380px' } as CSSProperties}>
                <GxGrid<ThanhVienHoiDoan>
                  columnDefs={cotThanhVienHoiDoan}
                  rowData={thanhVien ?? []}
                  layId={(d) => d.chiTietId}
                  onChon={chonThanhVien}
                  toDo={(d) => d.daRaKhoiHoiDoan}
                />
              </div>
            </div>

            {dongChonTV && (
              <div className="card glass">
                <b>{(dongChonTV.tenThanh ? dongChonTV.tenThanh + ' ' : '') + dongChonTV.hoTen}</b>
                <div style={{ marginTop: 8 }}>
                  <GxField label="Ngày vào hội đoàn" id="tv-ngayvao">
                    <GxDate id="tv-ngayvao" defaultValue={ngayVaoSua} onIsoChange={(iso) => setNgayVaoSua(iso || null)}
                      style={{ maxWidth: 170 }} />
                    <GxInline>Ngày ra hội đoàn</GxInline>
                    <GxDate id="tv-ngayra" defaultValue={ngayRaSua} onIsoChange={(iso) => setNgayRaSua(iso || null)}
                      style={{ maxWidth: 170, marginLeft: 'auto' }} />
                  </GxField>
                  <GxField label="Vai trò" id="tv-vaitro">
                    <input id="tv-vaitro" type="text" value={vaiTroSua} onChange={(e) => setVaiTroSua(e.target.value)}
                      placeholder="Hội viên" />
                  </GxField>
                </div>
                <div className="cmdbar">
                  <button type="button" className="btn btn-danger" disabled={dangLuuTV} onClick={() => { void xoaThanhVien() }}>
                    Xóa khỏi hội đoàn
                  </button>
                  <div className="spacer" />
                  <button type="button" className="btn btn-primary" disabled={dangLuuTV} onClick={() => { void luuThanhVien() }}>
                    {dangLuuTV ? 'Đang lưu…' : 'Lưu'}
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
