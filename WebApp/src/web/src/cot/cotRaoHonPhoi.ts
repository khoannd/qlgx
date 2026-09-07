import type { ColDef } from 'ag-grid-community'
import type { RaoHonPhoiListItem } from '../api/types'
import { dinhDangNgay } from '../lib/ngay'

/**
 * Đúng 7 cột và thứ tự của `GxRaoHonPhoiList.FormatGrid`
 * (Source/GXControl/GxRaoHonPhoiList.cs:95-164): Mã rao, Đôi rao, Người thứ nhất, Người thứ
 * hai, Rao lần 1/2/3, Ghi chú.
 */
export const cotRaoHonPhoi: ColDef<RaoHonPhoiListItem>[] = [
  { field: 'maRaoHonPhoiCu', headerName: 'Mã rao', width: 80, cellClass: 'cell-code' },
  { field: 'tenRaoHonPhoi', headerName: 'Đôi rao', width: 170, cellClass: 'cell-strong' },
  { field: 'nguoi1', headerName: 'Người thứ nhất', width: 170 },
  { field: 'nguoi2', headerName: 'Người thứ hai', width: 170 },
  { field: 'ngayRaoLan1', headerName: 'Rao lần 1', width: 100, valueFormatter: (p) => dinhDangNgay(p.value as string | null) },
  { field: 'ngayRaoLan2', headerName: 'Rao lần 2', width: 100, valueFormatter: (p) => dinhDangNgay(p.value as string | null) },
  { field: 'ngayRaoLan3', headerName: 'Rao lần 3', width: 100, valueFormatter: (p) => dinhDangNgay(p.value as string | null) },
  { field: 'ghiChu', headerName: 'Ghi chú', width: 200 },
]
