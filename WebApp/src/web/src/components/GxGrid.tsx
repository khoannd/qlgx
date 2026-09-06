import { AgGridReact } from 'ag-grid-react'
import { AllCommunityModule, ModuleRegistry } from 'ag-grid-community'
import type { ColDef, GetRowIdParams, GridApi, RowClassParams } from 'ag-grid-community'
import 'ag-grid-community/styles/ag-grid.css'
import 'ag-grid-community/styles/ag-theme-quartz.css'
import { forwardRef, useCallback, useImperativeHandle, useMemo, useRef, useState } from 'react'
import type { Ref } from 'react'

// ag-grid từ bản 33 trở đi nạp theo module: thiếu dòng này thì lưới không vẽ được
// header/hàng và ném lỗi #200 (getRowClass, rowSelection, localeText…).
ModuleRegistry.registerModules([AllCommunityModule])

export type MucMenu<T> = {
  nhan: string
  chay?: (dong: T) => void
  /** Trả true để ẨN mục này khỏi menu chuột phải của một dòng cụ thể — dùng khi hành động
   * không áp dụng được cho dòng đó (ví dụ "Xem gia đình" khi giáo dân chưa gắn với gia đình
   * nào), tránh gọi `chay` với giá trị rỗng/không hợp lệ. */
  an?: (dong: T) => boolean
}

/** Tay cầm lộ ra ngoài qua `ref` để nơi nhúng (thanh công cụ `GxToolbar`) gọi được thao tác
 * trên lưới mà không cần biết chi tiết AG Grid bên trong. */
export type GxGridHandle = {
  /** Chuỗi CSV của dữ liệu đang hiển thị (đã áp bộ lọc/sắp xếp hiện tại trên lưới), dùng cho
   * nút "Xuất dữ liệu (CSV)". Trả `null` nếu lưới chưa sẵn sàng. */
  layCsv: () => string | null
}

type Props<T> = {
  columnDefs: ColDef<T>[]
  rowData: T[]
  layId: (dong: T) => string
  onMo?: (dong: T) => void
  onChon?: (dong: T | null) => void
  menuChuotPhai?: MucMenu<T>[]
  /** Trả true để tô đỏ và gạch ngang cả dòng, giống quy tắc IsRedGiaoDan của bản desktop. */
  toDo?: (dong: T) => boolean
  ghiChuChan?: string
  hangLoc?: boolean
}

/**
 * Lớp lưới cơ sở, tương đương GxGrid : GridEX của bản desktop. Gói sẵn hàng lọc từng cột,
 * sắp xếp theo header, chọn dòng, mở bằng nhấp đúp và menu chuột phải để các lưới nghiệp vụ
 * bên trên không phải khai báo lại.
 */
function GxGridTrong<T>(
  { columnDefs, rowData, layId, onMo, onChon, menuChuotPhai, toDo, ghiChuChan, hangLoc = true }: Props<T>,
  ref: Ref<GxGridHandle>,
) {
  const [menu, setMenu] = useState<{ x: number; y: number; dong: T } | null>(null)
  const boc = useRef<HTMLDivElement>(null)
  const apiRef = useRef<GridApi<T> | null>(null)

  useImperativeHandle(ref, () => ({
    layCsv: () => apiRef.current?.getDataAsCsv() ?? null,
  }))

  const defaultColDef = useMemo<ColDef<T>>(
    () => ({
      sortable: true,
      resizable: true,
      filter: hangLoc ? 'agTextColumnFilter' : false,
      // `floatingFiltersHeight` bên dưới chỉ đặt chiều cao vùng hàng lọc; phải bật riêng
      // `floatingFilter` trên từng cột thì ô lọc mới thực sự hiện ra dưới mỗi tiêu đề,
      // đúng như FilterMode.Automatic của Janus GridEX ở bản desktop.
      floatingFilter: hangLoc,
    }),
    [hangLoc],
  )

  const getRowClass = useCallback(
    (p: RowClassParams<T>) => (p.data && toDo?.(p.data) ? 'dong-gach-do' : ''),
    [toDo],
  )

  return (
    <div className="table-card glass" ref={boc}>
      <div
        className="grid-wrap ag-theme-quartz"
        onContextMenu={(e) => {
          if (!menuChuotPhai?.length) return
          const dong = (e.target as HTMLElement).closest('.ag-row')
          if (!dong) return
          e.preventDefault()
          const id = dong.getAttribute('row-id')
          const banGhi = rowData.find((r) => layId(r) === id)
          if (banGhi) setMenu({ x: e.clientX, y: e.clientY, dong: banGhi })
        }}
      >
        <AgGridReact<T>
          theme="legacy"
          columnDefs={columnDefs}
          rowData={rowData}
          defaultColDef={defaultColDef}
          floatingFiltersHeight={hangLoc ? 30 : 0}
          getRowId={(p: GetRowIdParams<T>) => layId(p.data)}
          getRowClass={getRowClass}
          rowSelection="single"
          onGridReady={(e) => { apiRef.current = e.api }}
          onRowDoubleClicked={(e) => e.data && onMo?.(e.data)}
          onSelectionChanged={(e) => onChon?.(e.api.getSelectedRows()[0] ?? null)}
          localeText={{ noRowsToShow: 'Không có dòng nào khớp điều kiện lọc.' }}
        />
      </div>

      {ghiChuChan && (
        <div className="table-foot">
          <span className="legend"><i />{ghiChuChan}</span>
        </div>
      )}

      {menu && (
        <div
          id="ctxmenu"
          style={{ left: menu.x, top: menu.y }}
          onMouseLeave={() => setMenu(null)}
        >
          {menuChuotPhai!.filter((m) => !m.an?.(menu.dong)).map((m) => (
            <button key={m.nhan} type="button"
              onClick={() => { m.chay?.(menu.dong); setMenu(null) }}>
              {m.nhan}
            </button>
          ))}
        </div>
      )}
    </div>
  )
}

// `forwardRef` xoá mất tham số kiểu generic `T` của hàm gốc — ép kiểu lại để nơi gọi
// (`GxGiaoDanList`, `GxGiaDinhList`) vẫn được suy luận `T` đúng theo `rowData` truyền vào.
export const GxGrid = forwardRef(GxGridTrong) as <T>(
  props: Props<T> & { ref?: Ref<GxGridHandle> },
) => ReturnType<typeof GxGridTrong>
