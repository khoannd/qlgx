import { useEffect } from 'react'
import { AppShell } from './components/ThanhPhanKhung/AppShell'
import { TabDocs } from './components/ThanhPhanKhung/TabDocs'
import { useTabDocs } from './tabs/useTabDocs'

/** Chỗ giữ chỗ — màn hình Tổng quan thật sẽ được dựng ở task sau. */
function TongQuan() {
  return <p style={{ padding: 16 }}>Tổng quan — nội dung sẽ được dựng ở task sau.</p>
}

/** Chỗ giữ chỗ — màn hình Danh sách gia đình thật (bảng ag-grid, bộ lọc…)
 * sẽ được dựng ở task sau. */
function DanhSachGiaDinh() {
  return <p style={{ padding: 16 }}>Danh sách gia đình — nội dung sẽ được dựng ở task sau.</p>
}

function App() {
  const { danhSach, dangChon, mo, chon, dong } = useTabDocs()

  // Khởi động giống frmMain: mở "Tổng quan" (không đóng được), rồi mở và chọn
  // "Danh sách gia đình" — xem cuối script của bản mẫu.
  useEffect(() => {
    mo({ id: 'home', tieuDe: 'Tổng quan', noiDung: <TongQuan />, dongDuoc: false })
    mo({ id: 'giaDinhList', tieuDe: 'Danh sách gia đình', noiDung: <DanhSachGiaDinh /> })
  }, [mo])

  // Chỉ "Danh sách gia đình" đã có màn hình ở task này; các mục điều hướng
  // khác sẽ nối vào thẻ tài liệu tương ứng khi màn hình của chúng ra đời.
  function moTheoDieuHuong(id: string) {
    if (id === 'giaDinhList') {
      mo({ id: 'giaDinhList', tieuDe: 'Danh sách gia đình', noiDung: <DanhSachGiaDinh /> })
    }
  }

  return (
    <AppShell dangChonNav={dangChon} onNavigate={moTheoDieuHuong}>
      <TabDocs danhSach={danhSach} dangChon={dangChon} onChon={chon} onDong={dong} />
    </AppShell>
  )
}

export default App
