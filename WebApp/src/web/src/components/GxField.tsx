import type { ReactNode } from 'react'

type Props = {
  /** Rỗng ("") vẫn dựng nhãn trống, đúng cấu trúc `GxField("", …)` của bản mẫu — dùng khi
   * hàng chỉ chứa các ô tick không cần nhãn riêng. */
  label: string
  /** Nối với ô nhập của `children` qua `htmlFor`/`id` để `getByLabelText` tìm thấy. */
  id?: string
  children: ReactNode
  /** Phần phụ đặt sau ô nhập chính trong cùng hàng `.val`, ví dụ ô "Số hộ khẩu" đi kèm
   * "Mã gia đình", hay ô tick "Không tính vào thống kê" đi kèm ô CMND. */
  extra?: ReactNode
}

/** Tương đương hàm `GxField` của bản mẫu: một hàng `.frow` gồm nhãn và vùng `.val`. */
export function GxField({ label, id, children, extra }: Props) {
  return (
    <div className="frow">
      <label htmlFor={id}>{label}</label>
      <div className="val">
        {children}
        {extra}
      </div>
    </div>
  )
}

/** Tương đương `GxInline`: nhãn phụ chèn giữa hai ô trên cùng một hàng. */
export function GxInline({ children }: { children: ReactNode }) {
  return <span className="inline-label">{children}</span>
}
