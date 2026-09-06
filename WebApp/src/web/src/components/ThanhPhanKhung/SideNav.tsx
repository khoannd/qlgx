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
      { nhan: 'Danh sách hội đoàn' },
    ],
  },
  {
    nhan: 'Bí tích',
    muc: [
      { nhan: 'Danh sách sổ bí tích' },
      { nhan: 'Danh sách rao hôn phối' },
    ],
  },
  {
    nhan: 'Thông tin giáo xứ',
    muc: [
      { nhan: 'Giáo xứ' },
      { nhan: 'Giáo họ' },
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
}

export function SideNav({ dangChonId, onNavigate, laQuanTri }: Props) {
  const danhSachDieuHuong = laQuanTri
    ? [
        ...DANH_SACH_DIEU_HUONG,
        { nhan: 'Hệ thống', muc: [{ id: 'taiKhoanList', nhan: 'Quản lý tài khoản' }] },
      ]
    : DANH_SACH_DIEU_HUONG

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

      <div className="nav-foot">
        <b>Bản 4.0.0 · dữ liệu cục bộ</b>
        <br />
        Sao lưu gần nhất: hôm nay 06:15
      </div>
    </nav>
  )
}
