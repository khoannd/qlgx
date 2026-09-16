import type { TheTaiLieu } from '../../tabs/useTabDocs'

type Props = {
  danhSach: TheTaiLieu[]
  dangChon: string
  onChon: (id: string) => void
  onDong: (id: string) => void
}

export function TabDocs({ danhSach, dangChon, onChon, onDong }: Props) {
  return (
    <>
      <div className="tabstrip" role="tablist">
        {danhSach.map((t) => (
          <div
            key={t.id}
            className="tab"
            role="tab"
            tabIndex={0}
            aria-selected={t.id === dangChon}
            onClick={() => onChon(t.id)}
          >
            <span className="lbl" title={t.tieuDe}>{t.tieuDe}</span>
            {t.dongDuoc !== false && (
              <button
                className="x"
                type="button"
                title="Đóng thẻ"
                onClick={(e) => { e.stopPropagation(); onDong(t.id) }}
              >
                ×
              </button>
            )}
          </div>
        ))}
      </div>
      <div className="tabpages">
        {danhSach.map((t) => (
          <div key={t.id} hidden={t.id !== dangChon} style={{ height: '100%' }}>
            {t.noiDung}
          </div>
        ))}
      </div>
    </>
  )
}
