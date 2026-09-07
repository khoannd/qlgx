import type { ColDef } from 'ag-grid-community'
import type { DotBiTichListItem } from '../api/types'
import { dinhDangNgay } from '../lib/ngay'

/**
 * Đúng 5 cột và thứ tự của `GxDotBiTichList.FormatGrid`
 * (Source/GXControl/GxDotBiTichList.cs:53-91): Ngày, Mô tả, Người ban bí tích, Nơi nhận bí
 * tích, Số lượng GD.
 */
export const cotDotBiTich: ColDef<DotBiTichListItem>[] = [
  { field: 'ngayBiTich', headerName: 'Ngày', width: 110, valueFormatter: (p) => dinhDangNgay(p.value as string | null) },
  { field: 'moTa', headerName: 'Mô tả', width: 260, cellClass: 'cell-strong' },
  { field: 'linhMuc', headerName: 'Người ban bí tích', width: 200 },
  { field: 'noiBiTich', headerName: 'Nơi nhận bí tích', width: 200 },
  { field: 'soLuong', headerName: 'Số lượng GD', width: 120, cellClass: 'num' },
]
