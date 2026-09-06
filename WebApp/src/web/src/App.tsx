import { useEffect, useRef } from 'react'
import { AppShell } from './components/ThanhPhanKhung/AppShell'
import { TabDocs } from './components/ThanhPhanKhung/TabDocs'
import { useTabDocs } from './tabs/useTabDocs'
import { GiaDinhListPage } from './screens/GiaDinhListPage'
import { GiaDinhDetailPage } from './screens/GiaDinhDetailPage'
import { GiaoDanListPage } from './screens/GiaoDanListPage'
import { GiaoDanDetailPage } from './screens/GiaoDanDetailPage'
import { TaiKhoanListPage } from './screens/TaiKhoanListPage'
import { LoginPage } from './screens/LoginPage'
import { useAuth } from './api/AuthContext'

/** Chỗ giữ chỗ — màn hình Tổng quan thật sẽ được dựng ở task sau. */
function TongQuan() {
  return <p style={{ padding: 16 }}>Tổng quan — nội dung sẽ được dựng ở task sau.</p>
}

function App() {
  const { dangKiemTraPhien, nguoiDung } = useAuth()
  const { danhSach, dangChon, mo, chon, dong } = useTabDocs()
  // Đếm số bản ghi mới đang mở dở, giống biến moiDem của bản mẫu — mỗi lần bấm "Thêm mới"
  // là một thẻ nháp riêng, không trùng khoá với thẻ nháp khác đang mở.
  const moiDem = useRef(0)

  // Mở thẻ chi tiết gia đình: khoá thẻ theo mã bản ghi để mở lại đúng bản ghi thì chuyển tiêu
  // điểm thay vì tạo thẻ trùng (tương đương moChiTietGiaDinh của bản mẫu); id null tương ứng
  // nút "Thêm gia đình" nên luôn mở một thẻ nháp mới.
  // Tiêu đề thẻ chi tiết không còn biết trước tên bản ghi (dữ liệu giờ tải bất đồng bộ từ
  // API thay vì tra ngay trong mảng tĩnh) — dùng tiêu đề tạm rồi để chính `GiaDinhDetailPage`/
  // `GiaoDanDetailPage` hiển thị tên thật trong nội dung thẻ khi tải xong.
  function moChiTietGiaDinh(id: string | null) {
    const idThe = id ? `giaDinh:${id}` : `giaDinhMoi:${++moiDem.current}`
    mo({
      id: idThe,
      tieuDe: id ? 'Gia đình' : 'Gia đình mới',
      noiDung: (
        <GiaDinhDetailPage
          id={id}
          moGiaoDan={moChiTietGiaoDan}
          moDanhSachGiaDinh={moDanhSachGiaDinh}
        />
      ),
    })
  }

  function moChiTietGiaoDan(id: string | null) {
    const idThe = id ? `giaoDan:${id}` : `giaoDanMoi:${++moiDem.current}`
    mo({
      id: idThe,
      tieuDe: id ? 'Giáo dân' : 'Giáo dân mới',
      noiDung: (
        <GiaoDanDetailPage
          id={id}
          moGiaDinh={moChiTietGiaDinh}
          moDanhSachGiaoDan={moDanhSachGiaoDan}
          moGiaoDan={moChiTietGiaoDan}
        />
      ),
    })
  }

  function moDanhSachGiaDinh() {
    mo({
      id: 'giaDinhList',
      tieuDe: 'Danh sách gia đình',
      noiDung: <GiaDinhListPage moGiaDinh={moChiTietGiaDinh} />,
    })
  }

  function moDanhSachGiaoDan() {
    mo({
      id: 'giaoDanList',
      tieuDe: 'Danh sách giáo dân',
      noiDung: (
        <GiaoDanListPage
          moGiaoDan={moChiTietGiaoDan}
          moGiaDinh={moChiTietGiaDinh}
        />
      ),
    })
  }

  function moQuanLyTaiKhoan() {
    mo({ id: 'taiKhoanList', tieuDe: 'Quản lý tài khoản', noiDung: <TaiKhoanListPage /> })
  }

  // Khởi động giống frmMain: mở "Tổng quan" (không đóng được), rồi mở và chọn
  // "Danh sách gia đình" — xem cuối script của bản mẫu. CHỈ chạy sau khi đã đăng nhập —
  // gọi API trước khi có token chỉ để bị 401 rồi tự đăng xuất lại, vô ích.
  useEffect(() => {
    if (!nguoiDung) return
    mo({ id: 'home', tieuDe: 'Tổng quan', noiDung: <TongQuan />, dongDuoc: false })
    moDanhSachGiaDinh()
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [mo, nguoiDung])

  // Sidenav chỉ có các mục đã nối được thẻ tài liệu thật; nút bấm còn lại là chỗ giữ chỗ
  // (xem SideNav.tsx).
  function moTheoDieuHuong(id: string) {
    if (id === 'giaDinhList') moDanhSachGiaDinh()
    else if (id === 'giaoDanList') moDanhSachGiaoDan()
    else if (id === 'taiKhoanList') moQuanLyTaiKhoan()
  }

  // Đang kiểm tra token cũ (tải lại trang) — không hiện gì để tránh giật từ màn hình đăng
  // nhập sang màn hình chính hoặc ngược lại.
  if (dangKiemTraPhien) return null

  if (!nguoiDung) return <LoginPage />

  return (
    <AppShell dangChonNav={dangChon} onNavigate={moTheoDieuHuong} nguoiDung={nguoiDung}>
      <TabDocs danhSach={danhSach} dangChon={dangChon} onChon={chon} onDong={dong} />
    </AppShell>
  )
}

export default App
