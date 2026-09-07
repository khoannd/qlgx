import type { ColDef } from 'ag-grid-community'
import type { HoiDoanQuanLy, ThanhVienHoiDoan } from '../api/types'
import { dinhDangNgay } from '../lib/ngay'

/**
 * Đúng 6 cột và thứ tự của `GxListHoiDoan.FormatGrid` (Source/GXControl/GxListHoiDoan.cs:41-90):
 * Mã hội đoàn, Tên hội đoàn, Thánh bổn mạng, Ngày bổn mạng, Ngày thành lập, Ghi chú. Thêm cột
 * "Hội viên" (số hội viên đang hoạt động) — bản web mở rộng có chủ đích để không phải mở từng
 * hội đoàn mới biết có bao nhiêu người, xem hoi-doan-danh-sach.md mục 8.
 */
export const cotHoiDoan: ColDef<HoiDoanQuanLy>[] = [
  { field: 'maHoiDoanCu', headerName: 'Mã hội đoàn', width: 100, cellClass: 'num' },
  { field: 'tenHoiDoan', headerName: 'Tên hội đoàn', width: 220, cellClass: 'cell-strong' },
  { field: 'thanhBonMang', headerName: 'Thánh bổn mạng', width: 160 },
  { field: 'ngayBonMang', headerName: 'Ngày bổn mạng', width: 130, valueFormatter: (p) => dinhDangNgay(p.value as string | null) },
  { field: 'ngayThanhLap', headerName: 'Ngày thành lập', width: 130, valueFormatter: (p) => dinhDangNgay(p.value as string | null) },
  { field: 'soHoiVienDangHoatDong', headerName: 'Hội viên', width: 90, cellClass: 'num' },
  { field: 'ghiChu', headerName: 'Ghi chú', width: 220 },
]

/**
 * Lưới hội viên của một hội đoàn — khớp `gxGiaoDanList1` + 3 cột chèn thêm của `frmHoiDoan`
 * (Source/GXControl/frmHoiDoan.cs:63-98): Họ tên, (Tên thánh thêm cho dễ nhận), Ngày vào hội
 * đoàn, Ngày ra hội đoàn, Vai trò. Tô đỏ/gạch ngang các hội viên đã ra khỏi hội đoàn dùng `toDo`
 * của `GxGrid`, khớp `GridEXFormatCondition DaRa` (frmHoiDoan.cs:101-107).
 */
export const cotThanhVienHoiDoan: ColDef<ThanhVienHoiDoan>[] = [
  { field: 'tenThanh', headerName: 'Tên thánh', width: 110 },
  { field: 'hoTen', headerName: 'Họ tên', width: 200, cellClass: 'cell-strong' },
  { field: 'ngayVaoHoiDoan', headerName: 'Ngày vào hội đoàn', width: 140, valueFormatter: (p) => dinhDangNgay(p.value as string | null) },
  { field: 'ngayRaHoiDoan', headerName: 'Ngày ra hội đoàn', width: 140, valueFormatter: (p) => dinhDangNgay(p.value as string | null) },
  { field: 'vaiTro', headerName: 'Vai trò', width: 140 },
]
