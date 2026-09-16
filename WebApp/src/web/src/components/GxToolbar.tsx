import { Fragment, type ReactNode } from 'react'

export type MucToolbar =
  | '|'
  | '>'
  | { node: ReactNode }
  | {
      id?: string
      label: string
      icon?: 'plus' | 'excel' | 'reload' | 'print' | 'trash'
      kind?: 'primary' | 'quiet'
      /** Nút tự tắt khi lưới chưa có dòng đang chọn — truyền `coDongDuocChon` cho GxToolbar. */
      needSel?: boolean
      /** Tooltip tiếng Việt hiện khi rê chuột — theo đúng tinh thần tooltip của `GxAddEdit`
       * bản desktop (Thêm/Sửa/Loại bỏ khỏi danh sách trên lưới/…). Mặc định dùng `label`. */
      title?: string
      onClick?: () => void
    }

type Props = {
  items: MucToolbar[]
  /** Lưới bên dưới có đang chọn dòng hay không — quyết định các nút `needSel` bật/tắt. */
  coDongDuocChon?: boolean
}

const duongDan: Record<'plus' | 'excel' | 'reload' | 'print' | 'trash', string> = {
  plus: 'M12 5v14M5 12h14',
  excel: 'M12 3v12m0 0 4-4m-4 4-4-4M4 17v2a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2v-2',
  reload: 'M4 4v6h6M20 20v-6h-6M4 10a8 8 0 0 1 14.3-4.3M20 14a8 8 0 0 1-14.3 4.3',
  print: 'M6 9V3h12v6M6 18h12v3H6v-3ZM4 9h16v7H4V9Z',
  trash: 'M4 7h16M9 7V4h6v3m-8 0 1 13h8l1-13',
}

function IconSvg({ name }: { name: 'plus' | 'excel' | 'reload' | 'print' | 'trash' }) {
  return (
    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={2.2} aria-hidden="true">
      <path d={duongDan[name]} strokeLinecap="round" strokeLinejoin="round" />
    </svg>
  )
}

/** Tương đương hàm `GxToolbar` của bản mẫu: thanh nút thao tác trên mỗi lưới, giữ quy tắc
 * nút `needSel` tự tắt khi lưới bên dưới chưa chọn dòng nào. */
export function GxToolbar({ items, coDongDuocChon }: Props) {
  return (
    <div className="table-toolbar">
      {items.map((it, i) => {
        if (it === '|') return <span key={i} className="sep" />
        if (it === '>') return <span key={i} className="spacer" />
        if ('node' in it) return <Fragment key={i}>{it.node}</Fragment>

        const lop = ['btn', 'btn-sm', it.kind === 'primary' && 'btn-primary', it.kind === 'quiet' && 'btn-quiet']
          .filter(Boolean)
          .join(' ')
        return (
          <button
            key={it.id ?? it.label}
            type="button"
            className={lop}
            title={it.title ?? it.label}
            disabled={it.needSel ? !coDongDuocChon : false}
            onClick={it.onClick}
          >
            {it.icon && <IconSvg name={it.icon} />}
            {it.label}
          </button>
        )
      })}
    </div>
  )
}
