import { useEffect, useRef } from 'react'
import { AppShell } from './components/ThanhPhanKhung/AppShell'
import { TabDocs } from './components/ThanhPhanKhung/TabDocs'
import { useTabDocs } from './tabs/useTabDocs'
import { GiaDinhListPage } from './screens/GiaDinhListPage'
import { GiaDinhDetailPage } from './screens/GiaDinhDetailPage'
import { GiaoDanListPage } from './screens/GiaoDanListPage'
import { GiaoDanDetailPage } from './screens/GiaoDanDetailPage'
import { GiaoDanLuuTruListPage } from './screens/GiaoDanLuuTruListPage'
import { GiaDinhLuuTruListPage } from './screens/GiaDinhLuuTruListPage'
import { TaiKhoanListPage } from './screens/TaiKhoanListPage'
import { GiaoHoListPage } from './screens/GiaoHoListPage'
import { HoiDoanListPage } from './screens/HoiDoanListPage'
import { HoiDoanDetail } from './screens/HoiDoanDetail'
import { KhoiGiaoLyListPage } from './screens/KhoiGiaoLyListPage'
import { KhoiGiaoLyDetail } from './screens/KhoiGiaoLyDetail'
import { LopGiaoLyDetail } from './screens/LopGiaoLyDetail'
import { DotBiTichListPage } from './screens/DotBiTichListPage'
import { DotBiTichDetail } from './screens/DotBiTichDetail'
import { RaoHonPhoiListPage } from './screens/RaoHonPhoiListPage'
import { RaoHonPhoiDetail } from './screens/RaoHonPhoiDetail'
import type { LoaiBiTich } from './api/types'
import { KiemTraDuLieuGiaoDanPage } from './screens/KiemTraDuLieuGiaoDanPage'
import { QuanLyGiaoXuPage } from './screens/QuanLyGiaoXuPage'
import { ThongKeChungPage } from './screens/ThongKeChungPage'
import { BieuDoPage } from './screens/BieuDoPage'
import { NhapDuLieuPage } from './screens/NhapDuLieuPage'
import { LoginPage } from './screens/LoginPage'
import { useAuth } from './api/AuthContext'
import { TrangThaiMangBanner } from './components/TrangThaiMangBanner'
import { CapNhatPWA } from './components/CapNhatPWA'
import { useTuNhayKhiChonDropdown } from './lib/focusDieuHuong'

/** Chỗ giữ chỗ — màn hình Tổng quan thật sẽ được dựng ở task sau. */
function TongQuan() {
  return <p style={{ padding: 16 }}>Tổng quan — nội dung sẽ được dựng ở task sau.</p>
}

function App() {
  const { dangKiemTraPhien, nguoiDung } = useAuth()
  // Tự nhảy sang control kế tiếp khi chọn xong một mục trong dropdown (Giáo họ, Phái, Tình
  // trạng hôn phối…) — gắn MỘT LẦN ở gốc ứng dụng để mọi màn hình/mọi ô mới thêm sau này đều tự
  // có, không cần sửa từng nơi. Xem lib/focusDieuHuong.ts.
  useTuNhayKhiChonDropdown()
  const { danhSach, dangChon, mo, chon, dong, suaTieuDe } = useTabDocs()
  // Đếm số bản ghi mới đang mở dở, giống biến moiDem của bản mẫu — mỗi lần bấm "Thêm mới"
  // là một thẻ nháp riêng, không trùng khoá với thẻ nháp khác đang mở.
  const moiDem = useRef(0)

  // Mở thẻ chi tiết gia đình: khoá thẻ theo mã bản ghi để mở lại đúng bản ghi thì chuyển tiêu
  // điểm thay vì tạo thẻ trùng (tương đương moChiTietGiaDinh của bản mẫu); id null tương ứng
  // nút "Thêm gia đình" nên luôn mở một thẻ nháp mới.
  // Tiêu đề thẻ chi tiết không biết trước tên bản ghi (dữ liệu tải bất đồng bộ từ API thay vì
  // tra ngay trong mảng tĩnh) — mở thẻ với tiêu đề tạm ("Gia đình"/"Giáo dân") rồi
  // `GiaDinhDetailPage`/`GiaoDanDetailPage` tự gọi `onTieuDe` (→ `suaTieuDe` của `useTabDocs`)
  // để đổi lại đúng tên thật khi tải xong — trước đây KHÔNG đổi, mọi thẻ cùng loại hiện y hệt
  // nhau, mở 4 gia đình ra 4 thẻ "Gia đình" không phân biệt nổi (can-review-sau.md).
  // Tên tài khoản đang đăng nhập — truyền xuống các form chi tiết để khoá bản nháp ngoại tuyến
  // theo tài khoản (Task 16, xem lib/banNhap.ts). Component chi tiết không tự gọi useAuth() để
  // giữ khả năng test độc lập (không bắt buộc bọc <AuthProvider> trong test).
  const tenTaiKhoan = nguoiDung?.tenTaiKhoan ?? null
  // Khoá giáo xứ đang đăng nhập — dùng để tách gợi ý nhập liệu theo tần suất lưu ở
  // `localStorage` (xem lib/goiYNhapLieu.ts), cùng cách truyền tenTaiKhoan ở trên.
  const giaoXuId = nguoiDung?.giaoXuId ?? null

  function moChiTietGiaDinh(id: string | null) {
    const idThe = id ? `giaDinh:${id}` : `giaDinhMoi:${++moiDem.current}`
    mo({
      id: idThe,
      tieuDe: id ? 'Gia đình' : 'Gia đình mới',
      noiDung: (
        <GiaDinhDetailPage
          id={id}
          // Truyền `idThe` (khoá thẻ gia đình NÀY) làm "tab nguồn" — giáo dân mở từ lưới
          // thành viên/ô Người nam/Người nữ của màn hình gia đình nhớ được mình mở ra từ
          // đâu, để nút "Quay về" đóng đúng thẻ giáo dân đó thay vì mở "Danh sách giáo dân"
          // (góp ý người dùng, 2026-09-07: "quay về" từ giáo dân mở qua gia đình lại nhảy
          // sang danh sách thay vì đóng về đúng thẻ gia đình đang xem). Xem moChiTietGiaoDan.
          moGiaoDan={(giaoDanId) => moChiTietGiaoDan(giaoDanId, idThe)}
          moDanhSachGiaDinh={moDanhSachGiaDinh}
          tenTaiKhoan={tenTaiKhoan}
          giaoXuId={giaoXuId}
          onTieuDe={(ten) => suaTieuDe(idThe, ten)}
        />
      ),
    })
  }

  // `nguonTabId`: khoá thẻ đã mở ra giáo dân này (ví dụ thẻ gia đình `giaDinh:9`) — CHỈ truyền
  // khi mở từ một ngữ cảnh "thuộc về" tab khác (hiện chỉ có gia đình, xem moChiTietGiaDinh ở
  // trên). Không truyền (mở trực tiếp từ "Danh sách giáo dân", hoặc giáo dân mới tạo xong ở
  // GiaoDanDetailPage.tao) thì giữ nguyên hành vi cũ: "Quay về" mở/focus "Danh sách giáo dân".
  function moChiTietGiaoDan(id: string | null, nguonTabId?: string) {
    const idThe = id ? `giaoDan:${id}` : `giaoDanMoi:${++moiDem.current}`
    // Đơn giản nhất mà vẫn đúng ý người dùng: "Quay về" ĐÓNG thẻ giáo dân hiện tại thay vì mở
    // thẻ khác — đóng thẻ tự nhiên lộ ra thẻ gia đình bên dưới nếu đó đúng là cách nó được mở
    // (xem `dong()` của useTabDocs, chuyển tiêu điểm sang thẻ cuối cùng còn lại). Nếu thẻ nguồn
    // đã bị người dùng đóng trước đó, `dong()` vẫn chọn ra một thẻ còn lại hợp lý — không văng
    // về "Danh sách giáo dân" một cách vô điều kiện như trước, nhưng cũng không cần dò xem thẻ
    // nguồn còn tồn tại hay không (đơn giản hoá theo đúng gợi ý trong đặc tả).
    const quayVe = nguonTabId ? () => dong(idThe) : undefined
    mo({
      id: idThe,
      tieuDe: id ? 'Giáo dân' : 'Giáo dân mới',
      noiDung: (
        <GiaoDanDetailPage
          id={id}
          moGiaDinh={moChiTietGiaDinh}
          moDanhSachGiaoDan={moDanhSachGiaoDan}
          moGiaoDan={moChiTietGiaoDan}
          onQuayVe={quayVe}
          tenTaiKhoan={tenTaiKhoan}
          giaoXuId={giaoXuId}
          onTieuDe={(ten) => suaTieuDe(idThe, ten)}
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

  // Hồ sơ lưu trữ giáo dân/gia đình (frmGiaoDanLuuTruList.cs/frmGiaDinhLuuTruList.cs) — nút
  // Sửa/double-click mở lại ĐÚNG cùng thẻ chi tiết giáo dân/gia đình mà danh sách đang hoạt
  // động dùng (`moChiTietGiaoDan`/`moChiTietGiaDinh`), không có màn hình chi tiết riêng nào
  // khác — bản desktop cũng dùng chung `frmGiaoDan`/`frmGiaDinh` cho cả hai màn hình.
  function moDanhSachHoSoLuuTruGiaoDan() {
    mo({
      id: 'giaoDanLuuTruList',
      tieuDe: 'Hồ sơ lưu trữ giáo dân',
      noiDung: (
        <GiaoDanLuuTruListPage
          moGiaoDan={(id) => moChiTietGiaoDan(id)}
          moGiaDinh={moChiTietGiaDinh}
        />
      ),
    })
  }

  function moDanhSachHoSoLuuTruGiaDinh() {
    mo({
      id: 'giaDinhLuuTruList',
      tieuDe: 'Hồ sơ lưu trữ gia đình',
      noiDung: <GiaDinhLuuTruListPage moGiaDinh={(id) => moChiTietGiaDinh(id)} />,
    })
  }

  function moDanhSachSoBiTich() {
    mo({ id: 'dotBiTichList', tieuDe: 'Danh sách sổ bí tích', noiDung: <DotBiTichListPage moDot={moChiTietDotBiTich} /> })
  }

  function moChiTietDotBiTich(id: string | null, loaiBiTich: LoaiBiTich) {
    const idThe = id ? `dotBiTich:${id}` : `dotBiTichMoi:${++moiDem.current}`
    mo({
      id: idThe,
      tieuDe: id ? 'Đợt bí tích' : 'Đợt bí tích mới',
      noiDung: (
        <DotBiTichDetail
          id={id}
          loaiBiTich={loaiBiTich}
          onTieuDe={(ten) => suaTieuDe(idThe, ten)}
          onDaLuu={moDanhSachSoBiTich}
        />
      ),
    })
  }

  function moDanhSachRaoHonPhoi() {
    mo({ id: 'raoHonPhoiList', tieuDe: 'Danh sách rao hôn phối', noiDung: <RaoHonPhoiListPage moRao={moChiTietRao} /> })
  }

  function moChiTietRao(id: string | null) {
    const idThe = id ? `raoHonPhoi:${id}` : `raoHonPhoiMoi:${++moiDem.current}`
    mo({
      id: idThe,
      tieuDe: id ? 'Đôi rao' : 'Đôi rao mới',
      noiDung: (
        <RaoHonPhoiDetail id={id} onTieuDe={(ten) => suaTieuDe(idThe, ten)} onDaLuu={moDanhSachRaoHonPhoi} />
      ),
    })
  }

  function moQuanLyTaiKhoan() {
    mo({ id: 'taiKhoanList', tieuDe: 'Quản lý tài khoản', noiDung: <TaiKhoanListPage /> })
  }

  function moGiaoHoList() {
    mo({ id: 'giaoHoList', tieuDe: 'Giáo họ', noiDung: <GiaoHoListPage /> })
  }

  function moDanhSachHoiDoan() {
    mo({ id: 'hoiDoanList', tieuDe: 'Danh sách hội đoàn', noiDung: <HoiDoanListPage moHoiDoan={moChiTietHoiDoan} /> })
  }

  function moChiTietHoiDoan(id: string | null) {
    const idThe = id ? `hoiDoan:${id}` : `hoiDoanMoi:${++moiDem.current}`
    mo({
      id: idThe,
      tieuDe: id ? 'Hội đoàn' : 'Hội đoàn mới',
      noiDung: (
        <HoiDoanDetail id={id} onTieuDe={(ten) => suaTieuDe(idThe, ten)} onDaLuu={moDanhSachHoiDoan} />
      ),
    })
  }

  function moDanhSachKhoiGiaoLy() {
    mo({ id: 'khoiGiaoLyList', tieuDe: 'Quản lý giáo lý', noiDung: <KhoiGiaoLyListPage moKhoi={moChiTietKhoiGiaoLy} /> })
  }

  function moChiTietKhoiGiaoLy(id: string | null) {
    const idThe = id ? `khoiGiaoLy:${id}` : `khoiGiaoLyMoi:${++moiDem.current}`
    mo({
      id: idThe,
      tieuDe: id ? 'Khối giáo lý' : 'Khối giáo lý mới',
      noiDung: (
        <KhoiGiaoLyDetail id={id} onTieuDe={(ten) => suaTieuDe(idThe, ten)} onDaLuu={moDanhSachKhoiGiaoLy}
          moLop={moChiTietLopGiaoLy} />
      ),
    })
  }

  function moChiTietLopGiaoLy(id: string | null, khoiId: string, namMoi?: number) {
    const idThe = id ? `lopGiaoLy:${id}` : `lopGiaoLyMoi:${++moiDem.current}`
    mo({
      id: idThe,
      tieuDe: id ? 'Lớp giáo lý' : 'Lớp giáo lý mới',
      noiDung: (
        <LopGiaoLyDetail id={id} khoiId={khoiId} namMoi={namMoi}
          onTieuDe={(ten) => suaTieuDe(idThe, ten)}
          onXoaThanhCong={() => dong(idThe)} />
      ),
    })
  }

  // "Công cụ dữ liệu" -> "Kiểm tra dữ liệu" (chỉ nửa giáo dân, xem cong-cu-du-lieu.md).
  // "Xem chi tiết" mở ĐÚNG cùng thẻ chi tiết giáo dân mà "Danh sách giáo dân" dùng — không có
  // màn hình chi tiết riêng, giống cách hồ sơ lưu trữ tái dùng moChiTietGiaoDan.
  function moKiemTraDuLieuGiaoDan() {
    mo({
      id: 'kiemTraDuLieuGiaoDan',
      tieuDe: 'Kiểm tra dữ liệu',
      noiDung: <KiemTraDuLieuGiaoDanPage moGiaoDan={(id) => moChiTietGiaoDan(id)} />,
    })
  }

  function moQuanLyGiaoXu() {
    mo({ id: 'quanLyGiaoXu', tieuDe: 'Quản lý giáo xứ', noiDung: <QuanLyGiaoXuPage /> })
  }

  function moNhapDuLieu() {
    mo({ id: 'nhapDuLieu', tieuDe: 'Nhập dữ liệu Access', noiDung: <NhapDuLieuPage /> })
  }

  function moThongKeChung() {
    mo({ id: 'thongKeChung', tieuDe: 'Thống kê chung', noiDung: <ThongKeChungPage /> })
  }

  function moBieuDo() {
    mo({ id: 'bieuDo', tieuDe: 'Biểu đồ', noiDung: <BieuDoPage /> })
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
    else if (id === 'giaoHoList') moGiaoHoList()
    else if (id === 'hoiDoanList') moDanhSachHoiDoan()
    else if (id === 'khoiGiaoLyList') moDanhSachKhoiGiaoLy()
    else if (id === 'giaoDanLuuTruList') moDanhSachHoSoLuuTruGiaoDan()
    else if (id === 'giaDinhLuuTruList') moDanhSachHoSoLuuTruGiaDinh()
    else if (id === 'dotBiTichList') moDanhSachSoBiTich()
    else if (id === 'raoHonPhoiList') moDanhSachRaoHonPhoi()
    else if (id === 'kiemTraDuLieuGiaoDan') moKiemTraDuLieuGiaoDan()
    else if (id === 'quanLyGiaoXu') moQuanLyGiaoXu()
    else if (id === 'nhapDuLieu') moNhapDuLieu()
    else if (id === 'thongKeChung') moThongKeChung()
    else if (id === 'bieuDo') moBieuDo()
  }

  // Đang kiểm tra token cũ (tải lại trang) — không hiện gì để tránh giật từ màn hình đăng
  // nhập sang màn hình chính hoặc ngược lại.
  if (dangKiemTraPhien) return null

  if (!nguoiDung) {
    return (
      <>
        <TrangThaiMangBanner />
        <LoginPage />
        <CapNhatPWA />
      </>
    )
  }

  return (
    <>
      <TrangThaiMangBanner />
      <AppShell dangChonNav={dangChon} onNavigate={moTheoDieuHuong} nguoiDung={nguoiDung}>
        <TabDocs danhSach={danhSach} dangChon={dangChon} onChon={chon} onDong={dong} />
      </AppShell>
      <CapNhatPWA />
    </>
  )
}

export default App
