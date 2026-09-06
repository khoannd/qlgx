import type { ColDef } from 'ag-grid-community'
import type { GiaoDanListItem } from '../api/types'

const co = (field: keyof GiaoDanListItem, headerName: string, width = 110): ColDef<GiaoDanListItem> => ({
  field: field as ColDef<GiaoDanListItem>['field'],
  headerName,
  width,
  valueFormatter: (p) => (p.value === true ? '✓' : p.value === false ? '—' : (p.value ?? '—')),
})

/**
 * 29 cột đúng thứ tự của GxGiaoDanList.FormatGrid(). Tách khỏi màn hình vì cùng bộ cột này
 * được dùng ở hai nơi: danh sách giáo dân và lưới thành viên trong form gia đình.
 */
export const cotGiaoDan: ColDef<GiaoDanListItem>[] = [
  { field: 'maGiaoDanCu', headerName: 'Mã GD', width: 100, cellClass: 'cell-code' },
  { field: 'tenThanh', headerName: 'Tên thánh', width: 110 },
  { field: 'hoTen', headerName: 'Họ tên', width: 190, cellClass: 'cell-strong' },
  { field: 'phai', headerName: 'Phái', width: 80 },
  { field: 'ngaySinh', headerName: 'Ngày sinh', width: 115 },
  { field: 'ngayRuaToi', headerName: 'Ngày rửa tội', width: 125 },
  { field: 'ngayRuocLe', headerName: 'Ngày XTRL', width: 120 },
  { field: 'ngayThemSuc', headerName: 'Ngày Th.Sức', width: 125 },
  co('lapGd', 'Lập GĐ', 95),
  { field: 'hoTenCha', headerName: 'Cha', width: 160 },
  { field: 'hoTenMe', headerName: 'Mẹ', width: 160 },
  co('tanTong', 'Tân tòng', 100),
  co('conHoc', 'Còn học', 95),
  { field: 'ngheNghiep', headerName: 'Nghề nghiệp', width: 140 },
  { field: 'ghiChu', headerName: 'Ghi chú', width: 200 },
  { field: 'dienThoai', headerName: 'Điện thoại', width: 130 },
  { field: 'diaChi', headerName: 'Địa chỉ', width: 200 },
  { field: 'tenGiaoHo', headerName: 'Giáo họ', width: 150 },
  co('daChuyenDi', 'Đã chuyển đi', 130),
  { field: 'trinhDoVanHoa', headerName: 'Văn hóa', width: 110 },
  { field: 'trinhDoChuyenMon', headerName: 'Chuyên môn', width: 130 },
  { field: 'bietNgoaiNgu', headerName: 'Ngoại ngữ', width: 120 },
  co('quaDoi', 'Qua đời', 95),
  { field: 'ngayQuaDoi', headerName: 'Ngày qua đời', width: 130 },
  { field: 'noiAnTang', headerName: 'Nơi an táng', width: 150 },
  { field: 'noiSinh', headerName: 'Nơi sinh', width: 130 },
  { field: 'noiRuaToi', headerName: 'Nơi rửa tội', width: 140 },
  { field: 'noiRuocLe', headerName: 'Nơi XTRL', width: 130 },
  { field: 'noiThemSuc', headerName: 'Nơi thêm sức', width: 140 },
]

/** Cột chỉ có khi lưới nhúng trong form gia đình — frmGiaDinh chèn ở vị trí 0. */
export const cotQuanHeGiaDinh: ColDef<GiaoDanListItem> = {
  field: 'quanHe',
  headerName: 'Quan hệ GĐ',
  width: 130,
  editable: true,
  cellEditor: 'agSelectCellEditor',
  cellEditorParams: { values: ['Chồng', 'Vợ', 'Con'] },
}
