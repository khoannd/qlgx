import { useState } from 'react'
import { api } from '../api/client'
import { ChuanHoaDuLieu } from './ChuanHoaDuLieu'

/**
 * Container "Chuẩn hoá dữ liệu" — desktop có HAI mục menu riêng
 * (`itChuanHoaDuLieuGiaoDan`/`itChuanHoaDuLieuGiaDinh`, `frmMain.cs:365-370`). Bản web gộp vào
 * MỘT thẻ tài liệu với hai tab, cùng cách đã làm cho "Chuyển họ hàng loạt" — xem
 * docs/superpowers/specs/man-hinh/cong-cu-du-lieu.md mục 5.1 và can-review-sau.md mục 62.
 */
export function ChuanHoaDuLieuPage() {
  const [tab, setTab] = useState<'giaoDan' | 'giaDinh'>('giaoDan')
  // Khoá hai nút chuyển tab trong lúc tab con đang xem trước/đang ghi hàng loạt — đổi `tab` đổi
  // luôn `key` của `ChuanHoaDuLieu` bên dưới nên chuyển tab giữa chừng sẽ UNMOUNT component đang
  // chạy dở, làm mất luôn thông báo "Đã chuẩn hoá xong" dù việc ghi đã thành công ở máy chủ (rà
  // lại theo yêu cầu người dùng 2026-09-08, review toàn nhánh "Trung bình #1"). Xem
  // `ChuanHoaDuLieu.tsx` (`onDangXuLyChange`).
  const [dangXuLy, setDangXuLy] = useState(false)

  return (
    <div>
      <div className="filters-bar glass" style={{ gap: 8, marginBottom: 12 }}>
        <button type="button" className={tab === 'giaoDan' ? 'btn btn-primary' : 'btn'} disabled={dangXuLy}
          onClick={() => setTab('giaoDan')}>
          Giáo dân
        </button>
        <button type="button" className={tab === 'giaDinh' ? 'btn btn-primary' : 'btn'} disabled={dangXuLy}
          onClick={() => setTab('giaDinh')}>
          Gia đình
        </button>
      </div>
      {tab === 'giaoDan' ? (
        <ChuanHoaDuLieu
          key="giaoDan"
          nhan="giáo dân"
          moTaXacNhan="Bạn có chắc muốn thực hiện việc chuẩn hoá dữ liệu giáo dân không?"
          goiXemTruoc={api.chuanHoaDuLieu.xemTruocGiaoDan}
          goiGhiThat={api.chuanHoaDuLieu.ghiGiaoDan}
          onDangXuLyChange={setDangXuLy}
        />
      ) : (
        <ChuanHoaDuLieu
          key="giaDinh"
          nhan="gia đình"
          moTaXacNhan="Bạn có chắc muốn thực hiện việc chuẩn hoá dữ liệu gia đình không?"
          goiXemTruoc={api.chuanHoaDuLieu.xemTruocGiaDinh}
          goiGhiThat={api.chuanHoaDuLieu.ghiGiaDinh}
          onDangXuLyChange={setDangXuLy}
        />
      )}
    </div>
  )
}
