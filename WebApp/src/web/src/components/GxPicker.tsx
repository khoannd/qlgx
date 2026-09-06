type Props = {
  value?: string | null
  id?: string
  onChon?: () => void
  onThemMoi?: () => void
  onBoChon?: () => void
}

/** Ô chọn giáo dân — tương đương UserControl `GxGiaoDan` của bản desktop: ô chỉ đọc hiển thị
 * tên cùng ba nút tròn (chọn từ danh sách / thêm mới / bỏ chọn). Ba nút chưa nối API thật vì
 * các màn hình chọn giáo dân là việc của task khác; giữ callback rỗng để nơi gọi tự nối sau. */
export function GxPicker({ value, id, onChon, onThemMoi, onBoChon }: Props) {
  return (
    <span className="picker" id={id}>
      <span className={'who' + (value ? '' : ' empty')}>{value || '—'}</span>
      <button type="button" className="mini" title="Chọn từ danh sách giáo dân" onClick={onChon}>
        &#9678;
      </button>
      <button type="button" className="mini" title="Thêm giáo dân mới" onClick={onThemMoi}>
        +
      </button>
      <button type="button" className="mini" title="Bỏ chọn" onClick={onBoChon}>
        &times;
      </button>
    </span>
  )
}
