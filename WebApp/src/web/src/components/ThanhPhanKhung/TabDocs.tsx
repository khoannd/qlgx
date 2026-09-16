import { useEffect, useRef } from 'react'
import type { TheTaiLieu } from '../../tabs/useTabDocs'

type Props = {
  danhSach: TheTaiLieu[]
  dangChon: string
  onChon: (id: string) => void
  onDong: (id: string) => void
  onDongTatCa: () => void
}

export function TabDocs({ danhSach, dangChon, onChon, onDong, onDongTatCa }: Props) {
  const boc = useRef<HTMLDivElement>(null)
  const theDangChon = useRef<HTMLDivElement>(null)

  // Mở một thẻ mới (nhấp đúp một dòng, hoặc điều hướng từ menu) luôn phải THẤY được thẻ đó
  // ngay — nếu không, thẻ mới chỉ nằm ngoài mép phải của thanh cuộn ngang mà không có gợi ý
  // gì cho biết nó đã mở, trông như click không có tác dụng. `scrollIntoView` với `inline:
  // 'nearest'` chỉ cuộn khi thẻ thật sự bị che, không giật thanh cuộn khi thẻ đã hiện sẵn.
  useEffect(() => {
    theDangChon.current?.scrollIntoView({ inline: 'nearest', block: 'nearest' })
  }, [dangChon])

  // Số thẻ đóng được — dưới 2 thẻ thì nút "Đóng tất cả" không có việc gì để làm, ẩn đi cho
  // gọn thay vì hiện một nút luôn vô hiệu.
  const soTheDongDuoc = danhSach.filter((t) => t.dongDuoc !== false).length

  return (
    <>
      <div className="tabstrip-row">
        <div className="tabstrip" role="tablist" ref={boc}>
          {danhSach.map((t) => (
            <div
              key={t.id}
              ref={t.id === dangChon ? theDangChon : undefined}
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
        {soTheDongDuoc >= 2 && (
          <button
            type="button"
            className="tabstrip-dong-tat-ca"
            title="Đóng tất cả thẻ đang mở"
            onClick={onDongTatCa}
          >
            Đóng tất cả
          </button>
        )}
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
