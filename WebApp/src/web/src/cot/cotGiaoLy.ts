import type { ColDef } from 'ag-grid-community'
import type { GiaoLyVienLop, HocVienLopGiaoLy, KhoiGiaoLy, LopGiaoLy } from '../api/types'
import { dinhDangNgay } from '../lib/ngay'

/**
 * Đúng 4 cột và thứ tự của `GxKhoiGiaoLyList.FormatGrid` (Source/Giaoly/GxKhoiGiaoLyList.cs:52-88):
 * Mã khối, Tên khối, Người quản lý, Ghi chú. Thêm cột "Số lớp" — bản web mở rộng có chủ đích,
 * xem docs/superpowers/specs/man-hinh/giao-ly.md mục 8.
 */
export const cotKhoiGiaoLy: ColDef<KhoiGiaoLy>[] = [
  { field: 'maKhoiCu', headerName: 'Mã khối', width: 90, cellClass: 'num' },
  { field: 'tenKhoi', headerName: 'Tên khối', width: 220, cellClass: 'cell-strong' },
  { field: 'tenNguoiQuanLy', headerName: 'Người quản lý', width: 220 },
  { field: 'soLop', headerName: 'Số lớp', width: 90, cellClass: 'num' },
  { field: 'ghiChu', headerName: 'Ghi chú', width: 220 },
]

/**
 * Đúng 5 cột và thứ tự của `GxLopGiaoLyList.FormatGrid` (Source/Giaoly/GxLopGiaoLyList.cs:66-114):
 * Mã Lớp, Tên lớp, Phòng học, Giáo lý viên, Ghi chú. Thêm cột "Năm" và "Học viên" — bản web mở
 * rộng có chủ đích, xem giao-ly.md mục 8.
 */
export const cotLopGiaoLy: ColDef<LopGiaoLy>[] = [
  { field: 'maLopCu', headerName: 'Mã lớp', width: 90, cellClass: 'num' },
  { field: 'tenLop', headerName: 'Tên lớp', width: 200, cellClass: 'cell-strong' },
  { field: 'nam', headerName: 'Năm', width: 80, cellClass: 'num' },
  { field: 'phongHoc', headerName: 'Phòng học', width: 110 },
  { field: 'tenGiaoLyVien', headerName: 'Giáo lý viên', width: 220 },
  { field: 'soHocVien', headerName: 'Học viên', width: 90, cellClass: 'num' },
  { field: 'ghiChu', headerName: 'Ghi chú', width: 180 },
]

/**
 * Đúng thứ tự các cột còn giữ lại của `GxHocSinh.FormatGrid` (Source/Giaoly/GxHocSinh.cs:275-330):
 * Số thứ tự, Tên thánh, Họ tên, Phái, Ngày sinh, Hoàn thành khóa học, Ghi chú — bỏ Ngày XTRLLĐ/
 * Tên Cha/Tên Mẹ (xem sẵn ở hồ sơ giáo dân đầy đủ, không lặp lại — giao-ly.md mục 2).
 */
export const cotHocVienGiaoLy: ColDef<HocVienLopGiaoLy>[] = [
  { field: 'soThuTu', headerName: 'STT', width: 70, cellClass: 'num' },
  { field: 'tenThanh', headerName: 'Tên thánh', width: 110 },
  { field: 'hoTen', headerName: 'Họ tên', width: 200, cellClass: 'cell-strong' },
  { field: 'phai', headerName: 'Phái', width: 70 },
  { field: 'ngaySinh', headerName: 'Ngày sinh', width: 110, valueFormatter: (p) => dinhDangNgay(p.value as string | null) },
  { field: 'hoanThanh', headerName: 'Hoàn thành', width: 100, valueFormatter: (p) => (p.value ? 'Đã hoàn thành' : 'Chưa') },
  { field: 'ghiChuGLy', headerName: 'Ghi chú', width: 200 },
]

export const cotGiaoLyVien: ColDef<GiaoLyVienLop>[] = [
  { field: 'tenThanh', headerName: 'Tên thánh', width: 110 },
  { field: 'hoTen', headerName: 'Họ tên', width: 220, cellClass: 'cell-strong' },
]
