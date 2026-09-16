import type { ColDef } from 'ag-grid-community'
import type { NguoiNhanBiTich } from '../api/types'
import { dinhDangNgay } from '../lib/ngay'

/**
 * Cột của lưới "người nhận bí tích" trong một đợt — khớp `GxBiTichChiTiet.FormatGrid`
 * (Source/GXControl/GxBiTichChiTiet.cs:273-401). Nhãn cột đầu ("Số rửa tội"/"Số XTRL"/
 * "Số thêm sức") và việc có/không có cột "Người đỡ đầu" đổi theo loại bí tích — truyền vào
 * qua tham số thay vì cố định trong mảng.
 */
export function cotNguoiNhanBiTich(nhanSoBiTich: string, coNguoiDoDau: boolean): ColDef<NguoiNhanBiTich>[] {
  const cot: ColDef<NguoiNhanBiTich>[] = [
    { field: 'soBiTich', headerName: nhanSoBiTich, width: 90 },
    { field: 'tenThanh', headerName: 'Tên thánh', width: 110 },
    { field: 'hoTen', headerName: 'Họ tên', width: 190, cellClass: 'cell-strong' },
    { field: 'phai', headerName: 'Phái', width: 70 },
    { field: 'ngaySinh', headerName: 'Ngày sinh', width: 100, valueFormatter: (p) => dinhDangNgay(p.value as string | null) },
  ]
  if (coNguoiDoDau) cot.push({ field: 'nguoiDoDau', headerName: 'Người đỡ đầu', width: 170 })
  cot.push({ field: 'ghiChu', headerName: 'Ghi chú', width: 220 })
  return cot
}
