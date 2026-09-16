/** Dải hỏi "Khôi phục hay Bỏ qua" khi mở lại một form chi tiết mà có bản nháp chưa lưu nằm
 * sẵn trong `localStorage` (xem `lib/banNhap.ts`) — KHÔNG tự động khôi phục để tránh đè lên dữ
 * liệu vừa tải từ máy chủ (yêu cầu gốc, xem task-16-report.md). */
export function BanNhapBanner({
  thoiDiem, onKhoiPhuc, onBoQua,
}: {
  thoiDiem: string
  onKhoiPhuc: () => void
  onBoQua: () => void
}) {
  const gio = new Date(thoiDiem)
  const nhan = Number.isNaN(gio.getTime())
    ? 'trước đó'
    : `lúc ${gio.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' })} ngày ${gio.toLocaleDateString('vi-VN')}`

  return (
    <div className="bannhap-banner" role="status">
      <span>📝 Có bản nháp chưa lưu {nhan} — bạn muốn khôi phục hay bỏ qua?</span>
      <div className="spacer" />
      <button type="button" className="btn btn-sm btn-primary" onClick={onKhoiPhuc}>Khôi phục</button>
      <button type="button" className="btn btn-sm btn-quiet" onClick={onBoQua}>Bỏ qua</button>
    </div>
  )
}
