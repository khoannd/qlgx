import type { ColDef } from 'ag-grid-community'
import type { OnGoiListItem } from '../api/types'
import { dinhDangNgay } from '../lib/ngay'

/** Cột lưới của tab "Thống kê ơn gọi tận hiến" (`GxThongKeOnGoi.cs:86-96`) — cột giáo dân rút
 * gọn + 5 cột từ TanHien. */
export const cotOnGoi: ColDef<OnGoiListItem>[] = [
  { field: 'maGiaoDanCu', headerName: 'Mã GD', width: 90, cellClass: 'cell-code' },
  { field: 'tenThanh', headerName: 'Tên thánh', width: 110 },
  { field: 'hoTen', headerName: 'Họ tên', width: 180, cellClass: 'cell-strong' },
  { field: 'phai', headerName: 'Phái', width: 70 },
  {
    field: 'ngaySinh', headerName: 'Ngày sinh', width: 110,
    valueFormatter: (p) => dinhDangNgay(p.value as string | null),
  },
  {
    field: 'ngayBatDau', headerName: 'Ngày bắt đầu ơn gọi', width: 150,
    valueFormatter: (p) => dinhDangNgay(p.value as string | null),
  },
  { field: 'chucVu', headerName: 'Chức vụ', width: 110 },
  { field: 'noiTu', headerName: 'Nơi tu', width: 150 },
  { field: 'dongTu', headerName: 'Dòng tu', width: 150 },
  { field: 'noiPhucVu', headerName: 'Nơi phục vụ', width: 170 },
  { field: 'dienThoai', headerName: 'Điện thoại', width: 130 },
  { field: 'diaChi', headerName: 'Địa chỉ', width: 200 },
  { field: 'tenGiaoHo', headerName: 'Giáo họ', width: 150 },
]
