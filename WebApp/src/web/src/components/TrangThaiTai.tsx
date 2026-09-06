import type { ReactNode } from 'react'

type Props = {
  dangTai: boolean
  loi: string | null
  onThuLai?: () => void
  rong?: boolean
  thongBaoRong?: string
  children: ReactNode
}

/**
 * Bọc ba trạng thái chuẩn khi gọi API: đang tải / lỗi / rỗng — dùng chung cho bốn màn hình
 * gia đình và giáo dân để không lặp lại cùng một khối JSX ở mỗi nơi. Lỗi hiển thị nguyên văn
 * thông báo tiếng Việt lấy từ tầng gọi API (xem `api/client.ts`) kèm nút "Thử lại", KHÔNG bao
 * giờ nuốt lỗi rồi coi như danh sách rỗng — người dùng cần phân biệt được "chưa có dữ liệu"
 * với "gọi API thất bại".
 */
export function TrangThaiTai({ dangTai, loi, onThuLai, rong, thongBaoRong, children }: Props) {
  if (dangTai) {
    return (
      <div className="trang-thai-tai" role="status" style={{ padding: 24 }}>
        Đang tải dữ liệu…
      </div>
    )
  }

  if (loi) {
    return (
      <div className="trang-thai-loi" role="alert" style={{ padding: 24 }}>
        <p>Không tải được dữ liệu: {loi}</p>
        {onThuLai && (
          <button type="button" className="btn" onClick={onThuLai}>
            Thử lại
          </button>
        )}
      </div>
    )
  }

  if (rong) {
    return (
      <div className="trang-thai-rong" style={{ padding: 24 }}>
        {thongBaoRong ?? 'Chưa có dữ liệu.'}
      </div>
    )
  }

  return <>{children}</>
}
