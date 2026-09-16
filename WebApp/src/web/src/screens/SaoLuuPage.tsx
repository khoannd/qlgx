import { useCallback, useEffect, useRef, useState } from 'react'
import type { ColDef } from 'ag-grid-community'
import { api } from '../api/client'
import type {
  BanSaoLuu, CongViecSaoLuu, LoaiCongViecSaoLuu, TinhTrangSaoLuu, TrangThaiCongViec,
} from '../api/types'
import { GxGrid } from '../components/GxGrid'
import { SaoLuuPhucHoiModal } from './SaoLuuPhucHoiModal'
import { TrangThaiTai } from '../components/TrangThaiTai'
import { dinhDangNgayGio } from '../lib/ngay'
import { nhanDen, nhanLoiSaoLuu, thoiGianTuongDoi, tinhDenHieuLuc } from '../lib/denSaoLuu'

const MS_HOI_LAI = 2000

/** Chặn trên số lần hỏi lại tiến trình THẤT BẠI LIÊN TIẾP trước khi ngừng polling hẳn và báo
 * lỗi rõ ràng cho người dùng, thay vì để nút mãi bị khoá và màn hình mãi hiện "Đang chạy" mà
 * không ai biết máy chủ còn sống hay không. 60 lần × 2s = 2 phút — rộng rãi hơn nhiều so với
 * `CHO_SAN_SANG_SAU_HOAN_DOI_GIAY` (90 giây) phía máy chủ khi hoán đổi CSDL sau phục hồi, nên
 * không báo lỗi giả trong một lần hoán đổi bình thường, nhưng vẫn phát hiện được khi máy chủ
 * thật sự không quay lại nữa (job mất tích sau phục hồi, lỗi 404 dai dẳng…). */
const SO_LAN_LOI_TOI_DA = 60

export function nhanLoaiCongViec(loai: LoaiCongViecSaoLuu): string {
  switch (loai) {
    case 'phuc_hoi': return 'Phục hồi'
    case 'kiem_tra': return 'Kiểm tra'
    case 'dien_tap': return 'Diễn tập'
    case 'tai_ve': return 'Tải về'
    case 'dong_bo_danh_sach': return 'Đồng bộ danh sách'
    default: return 'Sao lưu'
  }
}

export function nhanTrangThaiCongViec(trangThai: TrangThaiCongViec): string {
  switch (trangThai) {
    case 'dang_chay': return 'Đang chạy'
    case 'xong': return 'Xong'
    case 'loi': return 'Lỗi'
    default: return 'Chờ'
  }
}

export function dinhDangKichThuoc(byte: number): string {
  if (byte <= 0) return '0 B'
  const donVi = ['B', 'KB', 'MB', 'GB', 'TB']
  const i = Math.min(Math.floor(Math.log(byte) / Math.log(1024)), donVi.length - 1)
  const so = byte / 1024 ** i
  return i === 0 ? `${so} B` : `${so.toFixed(1).replace('.', ',')} ${donVi[i]}`
}

/** Thời lượng một công việc, viết bằng tiếng Việt cho người đọc — chuỗi rỗng nếu chưa chạy xong
 * hoặc thiếu mốc thời gian. */
export function thoiLuong(congViec: Pick<CongViecSaoLuu, 'batDauLuc' | 'ketThucLuc'>): string {
  if (!congViec.batDauLuc || !congViec.ketThucLuc) return ''
  const giay = (new Date(congViec.ketThucLuc).getTime()
              - new Date(congViec.batDauLuc).getTime()) / 1000
  if (Number.isNaN(giay) || giay < 0) return ''
  if (giay < 60) return `${Math.round(giay)} giây`
  const phut = Math.floor(giay / 60)
  if (phut < 60) return `${phut} phút ${Math.round(giay % 60)} giây`
  return `${Math.floor(phut / 60)} giờ ${phut % 60} phút`
}

export function nhanNguon(nguon: BanSaoLuu['nguon']): string {
  switch (nguon) {
    case 'thu_cong': return 'Thủ công'
    case 'truoc_cap_nhat': return 'Trước cập nhật'
    case 'truoc_phuc_hoi': return 'Trước phục hồi'
    default: return 'Tự động'
  }
}

/**
 * Màn hình "Hệ thống → Sao lưu & Phục hồi" — CHỈ tài khoản Quản trị hệ thống (LoaiTaiKhoan=9)
 * thấy được, xem SideNav.tsx và policy "QuanTriHeThong" phía máy chủ.
 *
 * Mọi nút đều CHỈ tạo công việc rồi trả về ngay; bộ chạy trên host mới thực thi. Không có nút
 * nào chờ đồng bộ — dump một CSDL có ảnh bytea có thể mất vài phút, vượt timeout của reverse
 * proxy và làm treo một luồng của ứng dụng.
 *
 * Dùng polling 2 giây thay vì WebSocket: một người dùng, vài lần một tháng — thêm hạ tầng
 * realtime là phức tạp không cần thiết, và polling vẫn hoạt động khi container API vừa khởi
 * động lại sau bước hoán đổi CSDL, trong khi WebSocket thì đứt.
 */
export function SaoLuuPage() {
  const [tinhTrang, setTinhTrang] = useState<TinhTrangSaoLuu | null>(null)
  const [banSao, setBanSao] = useState<BanSaoLuu[]>([])
  const [congViec, setCongViec] = useState<CongViecSaoLuu[]>([])
  const [dangTai, setDangTai] = useState(true)
  const [loi, setLoi] = useState<string | null>(null)
  const [loiThaoTac, setLoiThaoTac] = useState<string | null>(null)
  const [dangChay, setDangChay] = useState<CongViecSaoLuu | null>(null)
  const [loiHoiLai, setLoiHoiLai] = useState<string | null>(null)
  const hoLaiRef = useRef<ReturnType<typeof setInterval> | null>(null)
  const soLoiLienTiepRef = useRef(0)
  const [banSaoDangPhucHoi, setBanSaoDangPhucHoi] = useState<BanSaoLuu | null>(null)
  /** Tep da chuan bi xong va con tai ve duoc. N5: TRUOC day nut tai chi hien khi
   * `dangChay?.id === maTaiVe`, nen chi can bam "Sao luu ngay" mot cai la nut BIEN MAT du tep
   * van con song 24 gio trong spool — muon tai phai tao lai mot luot `tai_ve` moi. Gio giu rieng,
   * khong phu thuoc cong viec nao dang chay. */
  const [tepSanSang, setTepSanSang] = useState<{ id: string; thoiDiem: string | null } | null>(null)
  /** Thoi diem cua ban sao ung voi cong viec `tai_ve` vua tao — chi de dat TEN TEP co dau thoi
   * gian khi may chu khong gui `Content-Disposition` (xem `tenTepBanSaoLuu`). */
  const thoiDiemTaiVeRef = useRef<{ id: string; thoiDiem: string } | null>(null)
  const [dangTaiTep, setDangTaiTep] = useState(false)

  const tai = useCallback(() => {
    setDangTai(true); setLoi(null)
    Promise.all([api.saoLuu.tinhTrang(), api.saoLuu.danhSach(), api.saoLuu.congViecGanDay()])
      .then(([tt, ds, cv]) => { setTinhTrang(tt); setBanSao(ds); setCongViec(cv) })
      .catch((e: unknown) => setLoi(e instanceof Error ? e.message : String(e)))
      .finally(() => setDangTai(false))
  }, [])

  useEffect(tai, [tai])
  useEffect(() => () => { if (hoLaiRef.current) clearInterval(hoLaiRef.current) }, [])

  /**
   * I8: bám theo công việc ĐANG CHẠY mà tab này không tạo ra.
   *
   * Trước đây `theoDoi()` chỉ được gọi từ `taoCongViec`/`taiVe`, nên: quản trị A bấm "Phục hồi"
   * rồi lỡ tay F5, hoặc quản trị B mở màn hình cùng lúc — họ thấy MỌI NÚT ĐANG BẬT, không có
   * khối tiến trình, không biết hệ thống đang phục hồi. Trong lúc phục hồi mà giao diện im lặng
   * là điều tệ nhất có thể xảy ra ở màn hình này.
   *
   * Điều kiện `hoLaiRef.current` đứng đầu: nếu đã có một vòng hỏi lại đang chạy thì không đụng
   * vào, tránh hai interval chồng nhau.
   */
  useEffect(() => {
    if (hoLaiRef.current) return
    const dangDo = congViec.find((c) => c.trangThai === 'cho' || c.trangThai === 'dang_chay')
    if (!dangDo) return
    setDangChay(dangDo)
    theoDoi(dangDo.id)
    // `theoDoi` đọc toàn ref/setState ổn định, cố ý không đưa vào deps để effect không chạy lại
    // mỗi lần render (sẽ tạo interval mới liên tục).
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [congViec])

  function theoDoi(id: string) {
    if (hoLaiRef.current) clearInterval(hoLaiRef.current)
    soLoiLienTiepRef.current = 0
    hoLaiRef.current = setInterval(() => {
      api.saoLuu.congViec(id)
        .then((cv) => {
          soLoiLienTiepRef.current = 0
          setDangChay(cv)
          if (cv.loai === 'tai_ve' && cv.trangThai === 'xong') {
            const ghi = thoiDiemTaiVeRef.current
            setTepSanSang({ id: cv.id, thoiDiem: ghi?.id === cv.id ? ghi.thoiDiem : null })
          }
          if (cv.trangThai === 'xong' || cv.trangThai === 'loi') {
            if (hoLaiRef.current) { clearInterval(hoLaiRef.current); hoLaiRef.current = null }
            tai()
          }
        })
        .catch(() => {
          // API co the dang khoi dong lai sau khi hoan doi CSDL — cu hoi lai, NHUNG chi trong
          // gioi han SO_LAN_LOI_TOI_DA lan lien tiep, khong de treo man hinh mai mai neu may
          // chu that su khong quay lai nua.
          soLoiLienTiepRef.current += 1
          if (soLoiLienTiepRef.current >= SO_LAN_LOI_TOI_DA) {
            if (hoLaiRef.current) { clearInterval(hoLaiRef.current); hoLaiRef.current = null }
            setLoiHoiLai('Không nhận được phản hồi từ máy chủ — hãy tải lại trang để kiểm tra.')
          }
        })
    }, MS_HOI_LAI)
  }

  async function taoCongViec(loai: LoaiCongViecSaoLuu, them?: Record<string, string>) {
    setLoiThaoTac(null)
    setLoiHoiLai(null)
    try {
      const { id } = await api.saoLuu.taoCongViec({ loai, ...them })
      setDangChay({
        id, loai, trangThai: 'cho', buocHienTai: null, nhatKy: null,
        taoLuc: new Date().toISOString(), batDauLuc: null, ketThucLuc: null,
      })
      theoDoi(id)
    } catch (e) {
      setLoiThaoTac(e instanceof Error ? e.message : String(e))
    }
  }

  async function taiVe(ban: BanSaoLuu) {
    setLoiThaoTac(null)
    try {
      const { id } = await api.saoLuu.taoCongViec({ loai: 'tai_ve', snapshotId: ban.id })
      setDangChay({
        id, loai: 'tai_ve', trangThai: 'cho', buocHienTai: 'Đang chuẩn bị tệp…',
        nhatKy: null, taoLuc: new Date().toISOString(), batDauLuc: null, ketThucLuc: null,
      })
      thoiDiemTaiVeRef.current = { id, thoiDiem: ban.thoiDiem }
      theoDoi(id)
    } catch (e) { setLoiThaoTac(e instanceof Error ? e.message : String(e)) }
  }

  async function taiTepDaChuanBi() {
    if (!tepSanSang) return
    setLoiThaoTac(null)
    setDangTaiTep(true)
    try {
      await api.saoLuu.taiBanSaoVe(tepSanSang.id, tepSanSang.thoiDiem)
    } catch (e) {
      setLoiThaoTac(e instanceof Error ? e.message : String(e))
    } finally {
      setDangTaiTep(false)
    }
  }

  const cot: ColDef<BanSaoLuu>[] = [
    { field: 'thoiDiem', headerName: 'Thời điểm', flex: 1.4,
      valueFormatter: (p) => dinhDangNgayGio(p.value as string) },
    { field: 'nhan', headerName: 'Nhãn', flex: 1 },
    { field: 'kichThuocByte', headerName: 'Kích thước', flex: 0.9,
      valueFormatter: (p) => dinhDangKichThuoc(p.value as number) },
    { field: 'soGiaoDan', headerName: 'Số giáo dân', flex: 0.9 },
    { field: 'soGiaDinh', headerName: 'Số gia đình', flex: 0.9 },
    { field: 'nguon', headerName: 'Nguồn', flex: 1,
      valueFormatter: (p) => nhanNguon(p.value as BanSaoLuu['nguon']) },
    // I7: "Tải về" phải có nút THẤY ĐƯỢC trên từng dòng. Người dùng của phần mềm này phần lớn
    // không rành máy tính, và trên máy tính bảng thì không có chuột phải — nằm trong menu chuột
    // phải nghĩa là coi như không tìm thấy. "Phục hồi" thì NGƯỢC LẠI: cố ý giữ kín trong menu
    // chuột phải, vì một nút đỏ ngay trên lưới là thứ người ta bấm nhầm.
    { headerName: '', flex: 0.8, sortable: false, filter: false, resizable: false,
      cellRenderer: (p: { data?: BanSaoLuu }) => (p.data ? (
        <button type="button" className="btn" style={{ padding: '0 10px', fontSize: 12 }}
          onClick={() => void taiVe(p.data!)}>Tải về</button>
      ) : null) },
  ]

  const dangCoViecChay = dangChay !== null
    && dangChay.trangThai !== 'xong' && dangChay.trangThai !== 'loi'
  const dangPhucHoi = dangCoViecChay && dangChay.loai === 'phuc_hoi'

  return (
    <TrangThaiTai dangTai={dangTai} loi={loi} onThuLai={tai}>
      <section className="page list-page">
        <div style={{ display: 'flex', flexDirection: 'column', gap: 16 }}>
          <KhoiTinhTrang tinhTrang={tinhTrang} />

          <div style={{ display: 'flex', gap: 8, flexWrap: 'wrap' }}>
            <button type="button" className="btn" disabled={dangCoViecChay}
              onClick={() => taoCongViec('sao_luu')}>Sao lưu ngay</button>
            <button type="button" className="btn" disabled={dangCoViecChay}
              onClick={() => taoCongViec('kiem_tra')}>Kiểm tra tính toàn vẹn</button>
            <button type="button" className="btn" disabled={dangCoViecChay}
              onClick={() => taoCongViec('dien_tap')}>Diễn tập phục hồi</button>
            <button type="button" className="btn" disabled={dangCoViecChay}
              onClick={() => taoCongViec('dong_bo_danh_sach')}>Tải lại</button>
          </div>

          {loiThaoTac && (
            <div role="alert" style={{ color: 'var(--rose-ink)', fontSize: 12.5 }}>{loiThaoTac}</div>
          )}

          {dangChay && <KhoiTienTrinh congViec={dangChay} />}
          {loiHoiLai && (
            <div role="alert" style={{ color: 'var(--rose-ink)', fontSize: 12.5 }}>{loiHoiLai}</div>
          )}

          <section style={{ display: 'flex', flexDirection: 'column', gap: 6 }}>
            <div className="page-head">
              <h3 style={{ margin: 0 }}>Danh sách bản sao lưu</h3>
              <div className="spacer" />
              <span className="count-pill"><b>{banSao.length}</b> bản sao</span>
            </div>
            <p style={{ margin: 0, fontSize: 12, color: 'var(--muted, #5b6a86)' }}>
              Tệp tải về là bản dữ liệu <strong>chưa mã hoá</strong>, chứa toàn bộ thông tin giáo
              dân. Chỉ tải về máy tin cậy và xoá ngay sau khi dùng xong.
            </p>
            <div style={{ height: 360 }}>
              <GxGrid<BanSaoLuu> columnDefs={cot} rowData={banSao} layId={(d) => d.id}
                ghiChuChan="Nhấp chuột phải vào một dòng để tải về hoặc phục hồi."
                menuChuotPhai={[
                  { nhan: 'Tải bản sao này về máy', chay: (d) => void taiVe(d) },
                  { nhan: 'Phục hồi về bản sao này…', chay: (d) => setBanSaoDangPhucHoi(d) },
                ]} />
            </div>
          </section>

          <KhoiNhatKy congViec={congViec} />

          {banSaoDangPhucHoi && (
            <SaoLuuPhucHoiModal
              banSao={banSaoDangPhucHoi}
              // `null` CO CHU DINH, dung sua thanh mot con so xap xi (C1 cua review-frontend.md).
              // Ban cu truyen `banSao[0]?.soGiaoDan` — so cua BAN SAO MOI NHAT — duoi nhan "Hien
              // tai". Khi phuc hoi ve chinh ban moi nhat (thao tac pho bien nhat) hai cot bang
              // nhau, khong dong nao to do, va lop rao 4 tran an SAI dung luc sap mat toi 6 gio
              // nhap lieu cua MOI giao xu. Frontend hien khong co nguon nao dem duoc so ban ghi
              // hien tai cua TOAN MAY CHU: `api.thongKe` chi tinh trong pham vi mot giao xu, con
              // `TinhTrangSaoLuuDto` chua tra ve so nao. Muon co so that: them
              // SoGiaoDanHienTai/SoGiaDinhHienTai (hai cau COUNT(*)) vao DTO do roi truyen vao
              // day — modal tu chay dung, khong phai sua gi them.
              soGiaoDanHienTai={null}
              soGiaDinhHienTai={null}
              onDong={() => setBanSaoDangPhucHoi(null)}
              onXacNhan={(snapshotId, xacNhan) => {
                setBanSaoDangPhucHoi(null)
                void taoCongViec('phuc_hoi', { snapshotId, xacNhan })
              }}
            />
          )}

          {tepSanSang && (
            <button type="button" className="btn" disabled={dangTaiTep} onClick={() => void taiTepDaChuanBi()}>
              {dangTaiTep ? 'Đang tải…' : 'Tải tệp đã chuẩn bị xong'}
            </button>
          )}
        </div>
        {dangPhucHoi && <ManChan congViec={dangChay} />}
      </section>
    </TrangThaiTai>
  )
}

/**
 * Màn chặn toàn trang trong lúc ĐANG PHỤC HỒI.
 *
 * KHOẢN NỢ ĐÃ BIẾT (I1 của review-frontend.md, spec 7.2): spec đòi màn chặn hiện cho MỌI người
 * dùng ở MỌI màn hình, rồi buộc tất cả đăng nhập lại sau khi hoán đổi xong. Việc đó chưa làm
 * được từ phía giao diện: cả nhóm `/api/sao-luu` đòi policy "QuanTriHeThong", nên tài khoản
 * thường không có cách nào biết có công việc phục hồi đang chạy — phải có một đường báo trạng
 * thái mở cho mọi tài khoản ở phía máy chủ. Xem `WebApp/TRIEN-KHAI.md` mục 14.
 *
 * Cái làm được ngay, và có giá trị thật: chặn chính tab của người vừa bấm phục hồi, để không ai
 * vừa bấm phục hồi vừa bấm tiếp thứ khác trên màn hình này.
 */
function ManChan({ congViec }: { congViec: CongViecSaoLuu }) {
  return (
    <div role="alertdialog" aria-modal="true" aria-label="Đang phục hồi dữ liệu"
      data-testid="man-chan-phuc-hoi"
      style={{ position: 'fixed', inset: 0, background: 'rgba(6,14,32,.72)', display: 'grid',
               placeItems: 'center', zIndex: 60, padding: 20 }}>
      <div className="glass" style={{ padding: 24, borderRadius: 'var(--r-card)', maxWidth: 520,
                                      display: 'flex', flexDirection: 'column', gap: 10 }}>
        <h3 style={{ margin: 0 }}>Đang phục hồi dữ liệu — xin đừng tắt trang</h3>
        <p style={{ margin: 0, fontSize: 12.5 }}>
          Hệ thống đang thay thế dữ liệu của <strong>toàn bộ máy chủ</strong>. Việc này thường mất
          vài phút. Xin đừng tắt trình duyệt, đừng tắt máy chủ, và hãy báo các giáo xứ khác{' '}
          <strong>ngừng nhập liệu</strong> cho tới khi xong.
        </p>
        {congViec.buocHienTai && (
          <div style={{ fontSize: 12.5 }}>Đang làm: {congViec.buocHienTai}</div>
        )}
      </div>
    </div>
  )
}

function KhoiTinhTrang({ tinhTrang }: { tinhTrang: TinhTrangSaoLuu | null }) {
  if (!tinhTrang) return null
  // I2/I3: đèn hiển thị có thể NẶNG HƠN đèn máy chủ trả về — xem lib/denSaoLuu.ts.
  const den = tinhDenHieuLuc(tinhTrang)
  const { chu, mau, cham } = nhanDen(den)
  const cauLoi = nhanLoiSaoLuu(tinhTrang.loiGanNhat)
  const tuongDoiSaoLuu = thoiGianTuongDoi(tinhTrang.saoLuuGanNhat)
  const tuongDoiDienTap = thoiGianTuongDoi(tinhTrang.dienTapGanNhat)
  return (
    <section className="glass" role="status" data-testid={`den-${den}`}
      style={{ padding: 16, borderRadius: 'var(--r-card)', display: 'flex',
               flexDirection: 'column', gap: 6 }}>
      <strong style={{ color: mau }}><span aria-hidden="true">{cham}</span> {chu}</strong>
      <div style={{ fontSize: 12.5 }}>
        Sao lưu gần nhất: <strong>{dinhDangNgayGio(tinhTrang.saoLuuGanNhat) || 'chưa có'}</strong>
        {tuongDoiSaoLuu && ` (${tuongDoiSaoLuu})`}
        {' · '}{tinhTrang.soBanSao} bản sao
        {' · '}Diễn tập phục hồi gần nhất:{' '}
        <strong>{dinhDangNgayGio(tinhTrang.dienTapGanNhat) || 'chưa có'}</strong>
        {tuongDoiDienTap && ` (${tuongDoiDienTap})`}
        {tinhTrang.dienTapGanNhat && (tinhTrang.dienTapDat ? ' — Đạt' : ' — KHÔNG ĐẠT')}
      </div>
      {den === 'vang' && !cauLoi && (
        <div style={{ color: 'var(--amber-ink, #8a5a00)', fontSize: 12.5 }}>
          Đã quá lâu chưa có bản sao lưu mới hoặc chưa diễn tập phục hồi lại. Hãy kiểm tra máy chủ
          còn bật và còn kết nối mạng; nếu vẫn vậy, hãy báo người kỹ thuật hỗ trợ.
        </div>
      )}
      {cauLoi && (
        <div style={{ color: 'var(--rose-ink)', fontSize: 12.5 }}>{cauLoi}</div>
      )}
      {/* I4: chuỗi kỹ thuật thô (tiếng Việt không dấu, tên công cụ) vẫn giữ lại — nhưng gấp vào
          trong, có nhãn nói rõ nó dành cho ai, thay vì đập vào mắt người vận hành. */}
      {tinhTrang.loiGanNhat && (
        <details>
          <summary style={{ fontSize: 12, cursor: 'pointer' }}>
            Thông tin cho người kỹ thuật
          </summary>
          <pre style={{ fontSize: 11, whiteSpace: 'pre-wrap', margin: '6px 0 0' }}>
            {tinhTrang.loiGanNhat}
          </pre>
        </details>
      )}
    </section>
  )
}

function KhoiTienTrinh({ congViec }: { congViec: CongViecSaoLuu }) {
  return (
    <section className="glass" style={{ padding: 12, borderRadius: 'var(--r-card)' }}>
      <strong>
        {congViec.trangThai === 'loi' ? 'Công việc thất bại'
          : congViec.trangThai === 'xong' ? 'Đã xong'
          : 'Đang chạy — đừng đóng tab'}
      </strong>
      {congViec.buocHienTai && (
        <div style={{ fontSize: 12.5 }}>Bước: {congViec.buocHienTai}</div>
      )}
      {congViec.nhatKy && (
        <details>
          {/* N7: nội dung là đầu ra thô của các công cụ trên máy chủ (không dấu, tiếng Anh).
              Chấp nhận được vì gấp sẵn, nhưng nhãn phải nói rõ nó dành cho ai. */}
          <summary style={{ fontSize: 12.5, cursor: 'pointer' }}>
            Nhật ký chi tiết (dành cho người kỹ thuật)
          </summary>
          <pre style={{ fontSize: 11, whiteSpace: 'pre-wrap', margin: '6px 0 0' }}>
            {congViec.nhatKy}
          </pre>
        </details>
      )}
    </section>
  )
}

/**
 * Nút "Xem nhật ký" của MỘT công việc trong danh sách — spec 8.2 khối 4.
 *
 * Nhật ký KHÔNG đi kèm trong danh sách: `SaoLuuService.ChieuDanhSach` cố ý bỏ `NhatKy` của các
 * công việc đã xong/đã lỗi (danh sách bị hỏi lại mỗi 2 giây; mỗi công việc giữ tới 8 000 byte
 * nhật ký, và nhật ký còn chứa số liệu giáo dân). Vì vậy phải gọi riêng đường xem chi tiết
 * `GET /api/sao-luu/cong-viec/{id}` khi người dùng thật sự bấm xem — chỉ hiện thẳng `nhatKy` có
 * sẵn nếu công việc đang chạy, vì lúc đó máy chủ có trả kèm.
 */
function NhatKyMotCongViec({ congViec }: { congViec: CongViecSaoLuu }) {
  const [noiDung, setNoiDung] = useState<string | null>(congViec.nhatKy)
  const [dangTai, setDangTai] = useState(false)
  const [loi, setLoi] = useState<string | null>(null)

  async function xem() {
    if (noiDung !== null || dangTai) return
    setDangTai(true); setLoi(null)
    try {
      const chiTiet = await api.saoLuu.congViec(congViec.id)
      setNoiDung(chiTiet.nhatKy ?? 'Công việc này không ghi lại nhật ký nào.')
    } catch (e) {
      setLoi(e instanceof Error ? e.message : String(e))
    } finally {
      setDangTai(false)
    }
  }

  return (
    <details onToggle={(e) => { if (e.currentTarget.open) void xem() }}>
      <summary style={{ cursor: 'pointer' }}>Xem nhật ký (dành cho người kỹ thuật)</summary>
      {dangTai && <div style={{ fontSize: 11 }}>Đang tải nhật ký…</div>}
      {loi && <div role="alert" style={{ fontSize: 11, color: 'var(--rose-ink)' }}>{loi}</div>}
      {noiDung !== null && (
        <pre style={{ fontSize: 11, whiteSpace: 'pre-wrap', margin: '4px 0 0' }}>{noiDung}</pre>
      )}
    </details>
  )
}

function KhoiNhatKy({ congViec }: { congViec: CongViecSaoLuu[] }) {
  if (congViec.length === 0) return null
  return (
    <details>
      <summary style={{ fontSize: 12.5, cursor: 'pointer' }}>
        Nhật ký công việc gần đây ({congViec.length})
      </summary>
      <ul style={{ fontSize: 12, margin: '6px 0 0', paddingLeft: 18 }}>
        {congViec.map((c) => (
          <li key={c.id} style={{ marginBottom: 4 }}>
            {dinhDangNgayGio(c.taoLuc)} — {nhanLoaiCongViec(c.loai)} —{' '}
            <strong>{nhanTrangThaiCongViec(c.trangThai)}</strong>
            {/* N2: thời lượng — spec 8.2 khối 4. Dữ liệu đã có sẵn, chỉ chưa dùng. */}
            {thoiLuong(c) && ` — chạy trong ${thoiLuong(c)}`}
            {' '}
            <NhatKyMotCongViec congViec={c} />
          </li>
        ))}
      </ul>
    </details>
  )
}
