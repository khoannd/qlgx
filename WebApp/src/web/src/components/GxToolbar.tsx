import { Fragment, type ReactNode } from 'react'

export type MucToolbar =
  | '|'
  | '>'
  | { node: ReactNode }
  | {
      id?: string
      label: string
      icon?: 'plus' | 'excel'
      kind?: 'primary' | 'quiet'
      /** Nút tự tắt khi lưới chưa có dòng đang chọn — truyền `coDongDuocChon` cho GxToolbar. */
      needSel?: boolean
      onClick?: () => void
    }

type Props = {
  items: MucToolbar[]
  /** Lưới bên dưới có đang chọn dòng hay không — quyết định các nút `needSel` bật/tắt. */
  coDongDuocChon?: boolean
}

const duongDan: Record<'plus' | 'excel', string> = {
  plus: 'M12 5v14M5 12h14',
  excel: 'M12 3v12m0 0 4-4m-4 4-4-4M4 17v2a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2v-2',
}

function IconSvg({ name }: { name: 'plus' | 'excel' }) {
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
