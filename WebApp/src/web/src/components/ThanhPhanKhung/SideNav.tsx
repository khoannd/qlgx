import { useEffect, useState } from 'react'
import { api } from '../../api/client'

/** Một mục điều hướng. `id` chỉ có mặt khi mục đó đã nối được vào một thẻ tài
 * liệu thật (xem `onNavigate` trong `App.tsx`) — các mục còn lại là chỗ giữ
 * chỗ cho màn hình sẽ dựng ở task sau, bấm vào chưa làm gì. */
type MucDieuHuong = {
  id?: string
  nhan: string
}

type NhomDieuHuong = {
  nhan: string
  muc: MucDieuHuong[]
}

const DANH_SACH_DIEU_HUONG: NhomDieuHuong[] = [
  {
    nhan: 'Giáo dân & gia đình',
    muc: [
      { id: 'giaoDanList', nhan: 'Danh sách giáo dân' },
      { id: 'giaDinhList', nhan: 'Danh sách gia đình' },
      { id: 'hoiDoanList', nhan: 'Danh sách hội đoàn' },
    ],
  },
  {
    nhan: 'Bí tích',
    muc: [
      { id: 'dotBiTichList', nhan: 'Danh sách sổ bí tích' },
      { id: 'raoHonPhoiList', nhan: 'Danh sách rao hôn phối' },
    ],
  },
  {
    nhan: 'Thông tin giáo xứ',
    muc: [
      { id: 'giaoHoList', nhan: 'Giáo họ' },
      { nhan: 'Quản lý giáo lý' },
    ],
  },
  {
    nhan: 'Thống kê',
    muc: [
      { nhan: 'Thống kê chung' },
      { nhan: 'Biểu đồ' },
    ],
  },
  {
    nhan: 'Công cụ dữ liệu',
    muc: [
      { nhan: 'Kiểm tra dữ liệu' },
      { nhan: 'Chuẩn hoá dữ liệu' },
      { nhan: 'Chuyển họ hàng loạt' },
      { nhan: 'Tạo danh sách bí tích tự động' },
    ],
  },
  {
    nhan: 'Hồ sơ lưu trữ',
    muc: [
      { nhan: 'Hồ sơ lưu trữ giáo dân' },
      { nhan: 'Hồ sơ lưu trữ gia đình' },
    ],
  },
]

type Props = {
  dangChonId: string
  onNavigate: (id: string) => void
  /** Chỉ Quản trị viên thấy mục "Quản lý tài khoản" — xem policy "QuanTri" phía backend và
   * quyết định ghi ở can-review-sau.md (bản desktop không chặn quyền này, bản web chặn). */
  laQuanTri: boolean
  /** Chỉ tài khoản "Quản trị hệ thống" (LoaiTaiKhoan=9) thấy mục "Quản lý giáo xứ" — màn hình
   * duy nhất nhìn xuyên TOÀN BỘ máy chủ, policy "QuanTriHeThong" phía backend (xem
   * docs/superpowers/specs/man-hinh/quan-ly-giao-xu.md mục 4). Mặc định false để không phá
   * các nơi gọi <SideNav> cũ chưa truyền prop này. */
  laQuanTriHeThong?: boolean
}

export function SideNav({ dangChonId, onNavigate, laQuanTri, laQuanTriHeThong }: Props) {
  const mucHeThong = [
    ...(laQuanTri ? [{ id: 'taiKhoanList', nhan: 'Quản lý tài khoản' }] : []),
    ...(laQuanTriHeThong ? [{ id: 'quanLyGiaoXu', nhan: 'Quản lý giáo xứ' }] : []),
    ...(laQuanTriHeThong ? [{ id: 'nhapDuLieu', nhan: 'Nhập dữ liệu Access' }] : []),
  ]
  const danhSachDieuHuong = mucHeThong.length > 0
    ? [...DANH_SACH_DIEU_HUONG, { nhan: 'Hệ thống', muc: mucHeThong }]
    : DANH_SACH_DIEU_HUONG

  // Phiên bản THẬT của bản web (GET /api/suc-khoe, anonymous) — trước đây viết cứng
  // "Bản 4.0.0 · dữ liệu cục bộ": "4.0.0" là số hiệu bản DESKTOP, và "dữ liệu cục bộ" mâu
  // thuẫn thẳng với mô hình đã chốt (một máy chủ tập trung phục vụ nhiều giáo xứ, xem
  // WebApp/TIEN-DO.md mục "Bốn thay đổi lớn") — xem can-review-sau.md mục 32. `null` khi
  // chưa tải xong hoặc gọi lỗi — hiện chữ trung lập thay vì để trống đột ngột.
  const [phienBan, setPhienBan] = useState<string | null>(null)
  useEffect(() => {
    let huy = false
    api.he.sucKhoe()
      .then((tt) => { if (!huy) setPhienBan(tt.phienBan) })
      .catch(() => { if (!huy) setPhienBan(null) })
    return () => { huy = true }
  }, [])

  return (
    <nav className="sidenav glass">
      <div className="nav-scroll">
        {danhSachDieuHuong.map((nhom) => (
          <div className="nav-group" key={nhom.nhan}>
            <span className="eyebrow">{nhom.nhan}</span>
            {nhom.muc.map((m) => (
              <button
                key={m.nhan}
                type="button"
                className={m.id !== undefined && m.id === dangChonId ? 'on' : undefined}
                onClick={m.id ? () => onNavigate(m.id!) : undefined}
              >
                <span className="ico" />
                {m.nhan}
              </button>
            ))}
          </div>
        ))}
      </div>

      {/* "Sao lưu gần nhất: hôm nay 06:15" cũ là chữ tĩnh bịa ra — chưa có API trạng thái sao
          lưu nào, thà không hiện gì còn hơn hiện một lời hứa sai (can-review-sau.md mục 32). */}
      <div className="nav-foot">
        <b>Bản web{phienBan ? ` ${phienBan}` : ''}</b>
      </div>
    </nav>
  )
}
