import { useState, type ReactNode } from 'react'

export type TabForm = { title: string; noiDung: ReactNode }

type Props = { tabs: TabForm[] }

/** Tương đương `GxFormTabs` của bản mẫu — tab nội dung bên trong một form, tương đương
 * `tabControl1` của `frmGiaoDan`. Có `role="tab"`/`aria-selected` để test truy cập được
 * bằng `getByRole('tab', { name })`. */
export function GxFormTabs({ tabs }: Props) {
  const [dangChon, setDangChon] = useState(0)

  return (
    <>
      <div className="form-tabs" role="tablist">
        {tabs.map((t, i) => (
          <button
            key={t.title}
            type="button"
            role="tab"
            aria-selected={i === dangChon}
            onClick={() => setDangChon(i)}
          >
            {t.title}
          </button>
        ))}
      </div>
      <div className="form-body">
        {tabs.map((t, i) => (
          <div
            key={t.title}
            className="form-body-panel"
            hidden={i !== dangChon}
            style={i === dangChon ? { display: 'grid', gap: 12 } : undefined}
          >
            {t.noiDung}
          </div>
        ))}
      </div>
    </>
  )
}
