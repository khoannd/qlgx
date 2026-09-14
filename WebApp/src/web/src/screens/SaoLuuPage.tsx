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
  // Ma cong viec "tai_ve" gan nhat — dung de biet khi nao hien duoc lien ket tai tep.
  const [maTaiVe, setMaTaiVe] = useState<string | null>(null)

  const tai = useCallback(() => {
    setDangTai(true); setLoi(null)
    Promise.all([api.saoLuu.tinhTrang(), api.saoLuu.danhSach(), api.saoLuu.congViecGanDay()])
      .then(([tt, ds, cv]) => { setTinhTrang(tt); setBanSao(ds); setCongViec(cv) })
      .catch((e: unknown) => setLoi(e instanceof Error ? e.message : String(e)))
      .finally(() => setDangTai(false))
  }, [])

  useEffect(tai, [tai])
  useEffect(() => () => { if (hoLaiRef.current) clearInterval(hoLaiRef.current) }, [])

  function theoDoi(id: string) {
    if (hoLaiRef.current) clearInterval(hoLaiRef.current)
    soLoiLienTiepRef.current = 0
    hoLaiRef.current = setInterval(() => {
      api.saoLuu.congViec(id)
        .then((cv) => {
          soLoiLienTiepRef.current = 0
          setDangChay(cv)
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
      theoDoi(id)
      setMaTaiVe(id)
    } catch (e) { setLoiThaoTac(e instanceof Error ? e.message : String(e)) }
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
  ]

  const dangCoViecChay = dangChay !== null
    && dangChay.trangThai !== 'xong' && dangChay.trangThai !== 'loi'

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
              // Chua co API dem so ban ghi HIEN TAI cua may chu — dung tam so cua ban sao moi
              // nhat lam gan dung (banSao da sap theo thoi diem giam dan, xem api.saoLuu.danhSach).
              // Day chi la thong tin tham khao tren giao dien de nguoi dung thay xu huong tang/
              // giam, KHONG anh huong toi thao tac phuc hoi that (thao tac do dua vao snapshotId,
              // khong dua vao con so nay). Neu can con so chinh xac tuyet doi, phai them truong
              // moi vao TinhTrangSaoLuuDto phia may chu (Task 6) — co chu dinh KHONG lam trong task
              // nay vi day chi la tieu tiet hien thi.
              soGiaoDanHienTai={banSao[0]?.soGiaoDan ?? 0}
              soGiaDinhHienTai={banSao[0]?.soGiaDinh ?? 0}
              onDong={() => setBanSaoDangPhucHoi(null)}
              onXacNhan={(snapshotId, xacNhan) => {
                setBanSaoDangPhucHoi(null)
                void taoCongViec('phuc_hoi', { snapshotId, xacNhan })
              }}
            />
          )}

          {maTaiVe && dangChay?.id === maTaiVe && dangChay.trangThai === 'xong' && (
            <a className="btn" href={api.saoLuu.duongDanTaiVe(maTaiVe)}>
              Tải tệp đã chuẩn bị xong
            </a>
          )}
        </div>
      </section>
    </TrangThaiTai>
  )
}

function KhoiTinhTrang({ tinhTrang }: { tinhTrang: TinhTrangSaoLuu | null }) {
  if (!tinhTrang) return null
  const { den } = tinhTrang
  const nhan = den === 'xanh' ? 'Bình thường'
             : den === 'vang' ? 'Chưa có bản sao mới'
             : 'Có vấn đề với sao lưu'
  const mau = den === 'do' ? 'var(--rose-ink)' : undefined
  return (
    <section className="glass" role="status"
      style={{ padding: 16, borderRadius: 'var(--r-card)', display: 'flex',
               flexDirection: 'column', gap: 6 }}>
      <strong style={{ color: mau }}>{nhan}</strong>
      <div style={{ fontSize: 12.5 }}>
        Sao lưu gần nhất: <strong>{dinhDangNgayGio(tinhTrang.saoLuuGanNhat) || 'chưa có'}</strong>
        {' · '}{tinhTrang.soBanSao} bản sao
        {' · '}Diễn tập phục hồi gần nhất:{' '}
        <strong>{dinhDangNgayGio(tinhTrang.dienTapGanNhat) || 'chưa có'}</strong>
        {tinhTrang.dienTapGanNhat && (tinhTrang.dienTapDat ? ' — Đạt' : ' — KHÔNG ĐẠT')}
      </div>
      {tinhTrang.loiGanNhat && (
        <div style={{ color: 'var(--rose-ink)', fontSize: 12.5 }}>{tinhTrang.loiGanNhat}</div>
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
          <summary style={{ fontSize: 12.5, cursor: 'pointer' }}>Nhật ký chi tiết</summary>
          <pre style={{ fontSize: 11, whiteSpace: 'pre-wrap', margin: '6px 0 0' }}>
            {congViec.nhatKy}
          </pre>
        </details>
      )}
    </section>
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
          <li key={c.id}>
            {dinhDangNgayGio(c.taoLuc)} — {nhanLoaiCongViec(c.loai)} —{' '}
            <strong>{nhanTrangThaiCongViec(c.trangThai)}</strong>
          </li>
        ))}
      </ul>
    </details>
  )
}
