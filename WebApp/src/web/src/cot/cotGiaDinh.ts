import type { ColDef } from 'ag-grid-community'
import type { GiaDinhListItem } from '../api/types'

/**
 * 12 cột đúng thứ tự và nhãn của bản mẫu desktop. Khác lưới giáo dân: quy tắc gạch đỏ ở đây
 * không tô cả dòng mà chỉ gạch từng ô — Người nam khi `gach` là 0 hoặc 2, Người nữ khi
 * `gach` là 1 hoặc 2 — nên xử lý bằng `cellClass` ngay tại cột thay vì `toDo` của GxGrid.
 */
export const cotGiaDinh: ColDef<GiaDinhListItem>[] = [
  { field: 'maGiaDinhCu', headerName: 'Mã GĐ', width: 100, cellClass: 'cell-code' },
  { field: 'tenGiaDinh', headerName: 'Tên gia đình', width: 170, cellClass: 'cell-strong' },
  {
    field: 'tenChong',
    headerName: 'Người nam',
    width: 190,
    cellClass: (p) => (p.data && (p.data.gach === 0 || p.data.gach === 2) ? 'struck' : ''),
  },
  {
    field: 'tenVo',
    headerName: 'Người nữ',
    width: 190,
    cellClass: (p) => (p.data && (p.data.gach === 1 || p.data.gach === 2) ? 'struck' : ''),
  },
  { field: 'soLuong', headerName: 'Số người', width: 100, cellClass: 'num' },
  { field: 'dienThoai', headerName: 'Điện thoại', width: 130 },
  { field: 'dtChong', headerName: 'ĐT chồng', width: 130 },
  { field: 'dtVo', headerName: 'ĐT vợ', width: 130 },
  { field: 'diaChi', headerName: 'Địa chỉ', width: 200 },
  { field: 'tenGiaoHo', headerName: 'Giáo họ', width: 150 },
  { field: 'dienGiaDinh', headerName: 'Diện gia đình', width: 130 },
  { field: 'ghiChu', headerName: 'Ghi chú', width: 200 },
]
