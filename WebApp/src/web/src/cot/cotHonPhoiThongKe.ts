import type { ColDef } from 'ag-grid-community'
import type { HonPhoiThongKeItem } from '../api/types'
import { dinhDangNgay } from '../lib/ngay'

/**
 * Cột lưới hôn phối cho tab "Thống kê chung" (điều kiện Hôn phối/Kỷ niệm hôn phối) — DTO MỚI
 * `HonPhoiThongKeDto`, bản web KHÔNG có endpoint "danh sách hôn phối" tổng quát nào sẵn để dùng
 * lại (xem docs/superpowers/specs/man-hinh/thong-ke-bieu-do.md mục 6/8.4).
 */
export const cotHonPhoiThongKe: ColDef<HonPhoiThongKeItem>[] = [
  { field: 'maHonPhoiCu', headerName: 'Mã HP', width: 90, cellClass: 'cell-code' },
  { field: 'tenChong', headerName: 'Người nam', width: 180, cellClass: 'cell-strong' },
  { field: 'tenVo', headerName: 'Người nữ', width: 180, cellClass: 'cell-strong' },
  {
    field: 'ngayHonPhoi', headerName: 'Ngày hôn phối', width: 130,
    valueFormatter: (p) => dinhDangNgay(p.value as string | null),
  },
  { field: 'noiHonPhoi', headerName: 'Nơi hôn phối', width: 170 },
  { field: 'soHonPhoi', headerName: 'Sổ HP', width: 100 },
  { field: 'linhMucChung', headerName: 'Linh mục chứng', width: 160 },
  { field: 'cachThucHonPhoi', headerName: 'Cách thức', width: 120 },
  { field: 'tenGiaoHo', headerName: 'Giáo họ', width: 150 },
  { field: 'ghiChu', headerName: 'Ghi chú', width: 200 },
]
