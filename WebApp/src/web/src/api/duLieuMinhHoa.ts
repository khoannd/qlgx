/**
 * Dữ liệu minh hoạ — TẠM THỜI. Backend (`/api/gia-dinh`, `/api/giao-dan` của Task 6–8) hiện bị
 * chặn vì thiếu mật khẩu CSDL, nên bốn màn hình nghiệp vụ của task này tạm nạp từ đây khi chạy
 * `App.tsx` để có cái xem trong lúc chưa nối API thật. Chép NGUYÊN VĂN hai mảng `FAMILIES` và
 * `PEOPLE` của `WebApp/prototype/qlgx-prototype.html` (giữ đúng tên trường, đúng nội dung tiếng
 * Việt), chỉ thêm các hàm ánh xạ sang các kiểu của `api/types.ts` bên dưới.
 *
 * Khi Task 6–8 xong, xoá file này và các chỗ gọi tới nó trong `App.tsx`, thay bằng `api.*`.
 */
import type {
  GiaDinhDetail, GiaDinhListItem, GiaoDanDetail, GiaoDanListItem, ThanhVien,
} from './types'

type FamilyRaw = {
  ma: string; ten: string; chong: string; vo: string; so: number; dt: string; dtc: string
  dtv: string; diachi: string; giaoho: string; dien: string; ghichu: string
  gach: number | null; ao?: boolean
}

/* Nguyên văn mảng FAMILIES của bản mẫu (gach: 0 = người nam đã qua đời/chuyển · 1 = người nữ
 * · 2 = cả hai). */
const FAMILIES: FamilyRaw[] = [
  { ma: '00012', ten: 'Bình - Lan', chong: 'Giuse Trần Văn Bình', vo: 'Maria Nguyễn Thị Lan', so: 5, dt: '028 3891 4472', dtc: '0912 345 678', dtv: '0987 114 220', diachi: '12/4 Nguyễn Trãi, KP 3', giaoho: 'Giáo họ Thánh Tâm', dien: '', ghichu: '', gach: null },
  { ma: '00027', ten: 'Chính - Hạnh', chong: 'Phêrô Lê Văn Chính', vo: 'Anna Phạm Thị Hạnh', so: 4, dt: '', dtc: '0908 221 774', dtv: '', diachi: '55 Lê Lợi', giaoho: 'Giáo họ Mân Côi', dien: 'Cận nghèo', ghichu: '', gach: null },
  { ma: '00035', ten: 'Dũng - Thu', chong: 'Đaminh Vũ Tiến Dũng', vo: 'Têrêsa Ngô Thị Thu', so: 6, dt: '028 3775 1120', dtc: '0933 561 908', dtv: '0902 447 511', diachi: '7 Hẻm 24 Trần Phú', giaoho: 'Giáo họ Fatima', dien: '', ghichu: 'Có con đi tu', gach: null },
  { ma: '00048', ten: 'Hoà - Mai', chong: 'Gioan B. Nguyễn Văn Hoà', vo: 'Maria Đỗ Thị Mai', so: 3, dt: '', dtc: '', dtv: '0973 882 145', diachi: '118 Cách Mạng Tháng 8', giaoho: 'Giáo họ Lộ Đức', dien: '', ghichu: '', gach: 0 },
  { ma: '00051', ten: 'Khanh - Nhung', chong: 'Antôn Bùi Đức Khanh', vo: 'Maria Trịnh Thị Nhung', so: 5, dt: '028 3812 6640', dtc: '0919 003 214', dtv: '0938 771 002', diachi: '3B Hoàng Văn Thụ', giaoho: 'Giáo họ Thánh Giuse', dien: '', ghichu: '', gach: null },
  { ma: '00063', ten: 'Long - Phượng', chong: 'Giuse Phan Thành Long', vo: 'Anna Lý Thị Phượng', so: 2, dt: '', dtc: '0906 554 118', dtv: '', diachi: '29 Nguyễn Huệ', giaoho: 'Giáo họ Antôn', dien: 'Neo đơn', ghichu: 'Ông bà lớn tuổi', gach: null },
  { ma: '00074', ten: 'Minh - Quyên', chong: 'Phêrô Hoàng Nhật Minh', vo: 'Maria Trần Thị Quyên', so: 4, dt: '028 3990 2277', dtc: '0977 123 456', dtv: '0911 664 730', diachi: '210 Phan Đình Phùng', giaoho: 'Giáo họ Thánh Tâm', dien: '', ghichu: '', gach: null },
  { ma: '00082', ten: 'Nam - Sương', chong: 'Giuse Đặng Hoài Nam', vo: 'Têrêsa Lâm Thị Sương', so: 5, dt: '', dtc: '', dtv: '0965 220 887', diachi: '66 Lý Thường Kiệt', giaoho: 'Giáo họ Mân Côi', dien: 'Nghèo', ghichu: '', gach: 2 },
  { ma: '00095', ten: 'Phúc - Trâm', chong: 'Đaminh Trương Hữu Phúc', vo: 'Maria Cao Thị Trâm', so: 7, dt: '028 3663 8890', dtc: '0944 512 663', dtv: '0988 342 116', diachi: '14 Nguyễn Đình Chiểu', giaoho: 'Giáo họ Fatima', dien: '', ghichu: 'Gia đình đông con', gach: null },
  { ma: '00108', ten: 'Quang - Uyên', chong: 'Gioan Nguyễn Đăng Quang', vo: 'Anna Vũ Thị Uyên', so: 3, dt: '', dtc: '0902 887 431', dtv: '', diachi: '81 Trường Chinh', giaoho: 'Giáo họ Lộ Đức', dien: '', ghichu: '', gach: 1 },
  { ma: '00117', ten: 'Sơn - Vân', chong: 'Phêrô Tạ Ngọc Sơn', vo: 'Maria Hồ Thị Vân', so: 4, dt: '028 3554 7712', dtc: '0913 776 220', dtv: '0906 118 553', diachi: '9 Bà Huyện Thanh Quan', giaoho: 'Giáo họ Thánh Giuse', dien: '', ghichu: '', gach: null },
  { ma: '00126', ten: 'Thắng - Xuân', chong: 'Giuse Mai Quang Thắng', vo: 'Têrêsa Đinh Thị Xuân', so: 6, dt: '', dtc: '0935 004 771', dtv: '0972 663 118', diachi: '47 Võ Thị Sáu', giaoho: 'Giáo họ Antôn', dien: '', ghichu: '', gach: null },
  { ma: '00134', ten: 'Tuấn - Yến', chong: 'Antôn Dương Anh Tuấn', vo: 'Maria Nguyễn Thị Yến', so: 5, dt: '028 3221 9008', dtc: '0908 664 220', dtv: '0917 553 884', diachi: '92 Hai Bà Trưng', giaoho: 'Giáo họ Thánh Tâm', dien: '', ghichu: '', gach: null },
  { ma: '00142', ten: 'Vinh - Diễm', chong: 'Giuse Ngô Quốc Vinh', vo: 'Anna Bùi Thị Diễm', so: 2, dt: '', dtc: '', dtv: '0983 117 664', diachi: '25 Điện Biên Phủ', giaoho: 'Ngoài xứ', dien: '', ghichu: 'Tạm trú, không thống kê', gach: null, ao: true },
]

type PersonRaw = {
  ma: string; tenThanh: string; hoTen: string; phai: string; ngaySinh: string
  noiSinh: string; ngayRuaToi: string; noiRuaToi: string; ngayXtrl: string; noiXtrl: string
  ngayThemSuc: string; noiThemSuc: string; lapGd: boolean; cha: string; me: string
  tanTong: boolean; conHoc: boolean; ngheNghiep: string; ghiChu: string; dienThoai: string
  diaChi: string; giaoHo: string; daChuyenDi: boolean; vanHoa: string; chuyenMon: string
  ngoaiNgu: string; quaDoi: boolean; ngayQuaDoi: string; noiAnTang: string; maGiaDinh: string
  quanHe: string
  /** Không có trong bản mẫu gốc (bản mẫu không phân biệt "Ngoài xứ" với "không được thống
   * kê") — thêm ở đây để có dữ liệu minh hoạ phân biệt được hai khái niệm khi lọc. */
  khongThongKe?: boolean
}

/** Tương đương hàm `P(o)` của bản mẫu: điền mặc định cho các trường ít dùng. */
function P(o: Partial<PersonRaw> & { ma: string; hoTen: string }): PersonRaw {
  return {
    tenThanh: '', phai: 'Nam', ngaySinh: '', noiSinh: 'TP.HCM',
    ngayRuaToi: '', noiRuaToi: 'GX Thánh Tâm', ngayXtrl: '', noiXtrl: 'GX Thánh Tâm',
    ngayThemSuc: '', noiThemSuc: 'GX Thánh Tâm', lapGd: false, cha: '', me: '',
    tanTong: false, conHoc: false, ngheNghiep: '', ghiChu: '', dienThoai: '', diaChi: '',
    giaoHo: 'Giáo họ Thánh Tâm', daChuyenDi: false, vanHoa: '12/12', chuyenMon: '',
    ngoaiNgu: '', quaDoi: false, ngayQuaDoi: '', noiAnTang: '', maGiaDinh: '', quanHe: '',
    khongThongKe: false,
    ...o,
  }
}

/* Nguyên văn mảng PEOPLE của bản mẫu. */
const PEOPLE: PersonRaw[] = [
  P({ ma: '04401', tenThanh: 'Giuse', hoTen: 'Trần Văn Bình', phai: 'Nam', ngaySinh: '03/05/1972', ngayRuaToi: '20/05/1972', ngayXtrl: '12/06/1980', ngayThemSuc: '18/10/1986', lapGd: true, ngheNghiep: 'Kỹ sư', dienThoai: '0912 345 678', diaChi: '12/4 Nguyễn Trãi, KP 3', maGiaDinh: '00012', quanHe: 'Chồng', chuyenMon: 'Đại học' }),
  P({ ma: '04402', tenThanh: 'Maria', hoTen: 'Nguyễn Thị Lan', phai: 'Nữ', ngaySinh: '11/09/1975', ngayRuaToi: '02/10/1975', ngayXtrl: '14/05/1983', ngayThemSuc: '20/11/1989', lapGd: true, ngheNghiep: 'Giáo viên', dienThoai: '0987 114 220', diaChi: '12/4 Nguyễn Trãi, KP 3', maGiaDinh: '00012', quanHe: 'Vợ', chuyenMon: 'Cao đẳng' }),
  P({ ma: '04412', tenThanh: 'Maria', hoTen: 'Trần Thị Khánh Ngọc', phai: 'Nữ', ngaySinh: '14/03/2005', ngayRuaToi: '02/04/2005', ngayXtrl: '18/05/2013', ngayThemSuc: '21/11/2018', conHoc: true, cha: 'Trần Văn Bình', me: 'Nguyễn Thị Lan', ngheNghiep: 'Sinh viên', diaChi: '12/4 Nguyễn Trãi, KP 3', maGiaDinh: '00012', quanHe: 'Con' }),
  P({ ma: '04413', tenThanh: 'Giuse', hoTen: 'Trần Minh Khôi', phai: 'Nam', ngaySinh: '09/09/2008', ngayRuaToi: '28/09/2008', ngayXtrl: '12/05/2016', conHoc: true, cha: 'Trần Văn Bình', me: 'Nguyễn Thị Lan', ngheNghiep: 'Học sinh', ghiChu: 'Giúp lễ', diaChi: '12/4 Nguyễn Trãi, KP 3', maGiaDinh: '00012', quanHe: 'Con', vanHoa: '9/12' }),
  P({ ma: '04414', tenThanh: 'Anna', hoTen: 'Trần Thị Thanh Hương', phai: 'Nữ', ngaySinh: '22/07/1999', ngayRuaToi: '15/08/1999', ngayXtrl: '04/06/2008', ngayThemSuc: '19/10/2013', lapGd: true, cha: 'Trần Văn Bình', me: 'Nguyễn Thị Lan', ngheNghiep: 'Kế toán', ghiChu: 'Đã lập gia đình riêng', diaChi: '33 Lê Văn Sỹ', giaoHo: 'Giáo họ Mân Côi', maGiaDinh: '00012', quanHe: 'Con' }),
  P({ ma: '04398', tenThanh: 'Maria', hoTen: 'Phạm Thị Tần', phai: 'Nữ', ngaySinh: '11/02/1941', ngayRuaToi: '03/03/1941', ngheNghiep: 'Nội trợ', quaDoi: true, ngayQuaDoi: '08/06/2023', noiAnTang: 'Đất thánh giáo xứ', ghiChu: 'Đã qua đời', diaChi: '12/4 Nguyễn Trãi, KP 3', maGiaDinh: '00012', quanHe: 'Mẹ chồng', vanHoa: '', khongThongKe: true }),
  P({ ma: '04455', tenThanh: 'Phêrô', hoTen: 'Trần Gia Bảo', phai: 'Nam', ngaySinh: '30/12/2019', ngayRuaToi: '19/01/2020', cha: 'Trần Minh Khôi', diaChi: '12/4 Nguyễn Trãi, KP 3', maGiaDinh: '00012', quanHe: 'Cháu', vanHoa: '' }),

  P({ ma: '04501', tenThanh: 'Đaminh', hoTen: 'Vũ Tiến Dũng', phai: 'Nam', ngaySinh: '17/01/1968', ngayRuaToi: '04/02/1968', ngayXtrl: '11/05/1976', ngayThemSuc: '22/10/1982', lapGd: true, ngheNghiep: 'Thợ mộc', dienThoai: '0933 561 908', diaChi: '7 Hẻm 24 Trần Phú', giaoHo: 'Giáo họ Fatima', maGiaDinh: '00035', quanHe: 'Chồng', vanHoa: '9/12' }),
  P({ ma: '04502', tenThanh: 'Têrêsa', hoTen: 'Ngô Thị Thu', phai: 'Nữ', ngaySinh: '25/08/1971', ngayRuaToi: '12/09/1971', ngayXtrl: '08/05/1979', ngayThemSuc: '17/11/1985', lapGd: true, ngheNghiep: 'Buôn bán', dienThoai: '0902 447 511', diaChi: '7 Hẻm 24 Trần Phú', giaoHo: 'Giáo họ Fatima', maGiaDinh: '00035', quanHe: 'Vợ' }),
  P({ ma: '04511', tenThanh: 'Giuse', hoTen: 'Vũ Minh Trí', phai: 'Nam', ngaySinh: '02/04/1996', ngayRuaToi: '21/04/1996', ngayXtrl: '09/06/2004', ngayThemSuc: '12/11/2010', cha: 'Vũ Tiến Dũng', me: 'Ngô Thị Thu', ngheNghiep: 'Chủng sinh', ghiChu: 'Đang tu học tại Đại chủng viện', diaChi: '7 Hẻm 24 Trần Phú', giaoHo: 'Giáo họ Fatima', maGiaDinh: '00035', quanHe: 'Con', chuyenMon: 'Đại học', ngoaiNgu: 'Anh, Latinh' }),
  P({ ma: '04512', tenThanh: 'Maria', hoTen: 'Vũ Thị Ngọc Hà', phai: 'Nữ', ngaySinh: '19/11/2001', ngayRuaToi: '09/12/2001', ngayXtrl: '05/06/2010', ngayThemSuc: '23/10/2015', cha: 'Vũ Tiến Dũng', me: 'Ngô Thị Thu', ngheNghiep: 'Điều dưỡng', diaChi: '7 Hẻm 24 Trần Phú', giaoHo: 'Giáo họ Fatima', maGiaDinh: '00035', quanHe: 'Con', chuyenMon: 'Cao đẳng' }),
  P({ ma: '04513', tenThanh: 'Antôn', hoTen: 'Vũ Đức Duy', phai: 'Nam', ngaySinh: '06/06/2010', ngayRuaToi: '27/06/2010', ngayXtrl: '14/05/2018', conHoc: true, cha: 'Vũ Tiến Dũng', me: 'Ngô Thị Thu', ngheNghiep: 'Học sinh', diaChi: '7 Hẻm 24 Trần Phú', giaoHo: 'Giáo họ Fatima', maGiaDinh: '00035', quanHe: 'Con', vanHoa: '8/12' }),
  P({ ma: '04520', tenThanh: 'Gioan B.', hoTen: 'Ngô Văn Cẩn', phai: 'Nam', ngaySinh: '14/07/1944', ngayRuaToi: '01/08/1944', ngheNghiep: 'Nghỉ hưu', diaChi: '7 Hẻm 24 Trần Phú', giaoHo: 'Giáo họ Fatima', maGiaDinh: '00035', quanHe: 'Cha', vanHoa: '7/12' }),

  P({ ma: '04601', tenThanh: 'Gioan B.', hoTen: 'Nguyễn Văn Hoà', phai: 'Nam', ngaySinh: '08/03/1965', ngayRuaToi: '28/03/1965', quaDoi: true, ngayQuaDoi: '12/02/2024', noiAnTang: 'Đất thánh giáo xứ', ngheNghiep: 'Tài xế', diaChi: '118 Cách Mạng Tháng 8', giaoHo: 'Giáo họ Lộ Đức', maGiaDinh: '00048', quanHe: 'Chồng' }),
  P({ ma: '04602', tenThanh: 'Maria', hoTen: 'Đỗ Thị Mai', phai: 'Nữ', ngaySinh: '30/10/1969', ngayRuaToi: '19/11/1969', ngayThemSuc: '08/11/1984', lapGd: true, ngheNghiep: 'Nội trợ', dienThoai: '0973 882 145', diaChi: '118 Cách Mạng Tháng 8', giaoHo: 'Giáo họ Lộ Đức', maGiaDinh: '00048', quanHe: 'Vợ' }),
  P({ ma: '04701', tenThanh: 'Anna', hoTen: 'Vũ Thị Uyên', phai: 'Nữ', ngaySinh: '21/05/1978', ngayRuaToi: '10/06/1978', daChuyenDi: true, ngheNghiep: 'Công nhân', ghiChu: 'Đã chuyển đến GX Tân Định', diaChi: '81 Trường Chinh', giaoHo: 'Giáo họ Lộ Đức', maGiaDinh: '00108', quanHe: 'Vợ', khongThongKe: true }),
  P({ ma: '04801', tenThanh: 'Têrêsa', hoTen: 'Lâm Thị Sương', phai: 'Nữ', ngaySinh: '12/12/1966', ngayRuaToi: '01/01/1967', quaDoi: true, ngayQuaDoi: '03/09/2022', noiAnTang: 'Đất thánh giáo xứ', ngheNghiep: 'Nội trợ', diaChi: '66 Lý Thường Kiệt', giaoHo: 'Giáo họ Mân Côi', maGiaDinh: '00082', quanHe: 'Vợ' }),
]

/** "dd/mm/yyyy" của bản mẫu → "yyyy-mm-dd" cho `<input type="date">`; rỗng → null. */
function iso(dmy: string): string | null {
  if (!dmy) return null
  const [d, m, y] = dmy.split('/')
  return `${y}-${m}-${d}`
}

export const danhSachGiaDinh: GiaDinhListItem[] = FAMILIES.map((f) => ({
  id: f.ma,
  maGiaDinhCu: Number(f.ma),
  maGiaDinhRieng: null,
  tenGiaDinh: f.ten,
  tenChong: f.chong || null,
  tenVo: f.vo || null,
  soLuong: f.so,
  dienThoai: f.dt || null,
  dtChong: f.dtc || null,
  dtVo: f.dtv || null,
  diaChi: f.diachi || null,
  tenGiaoHo: f.giaoho,
  dienGiaDinh: f.dien || null,
  ghiChu: f.ghichu || null,
  gach: f.gach ?? -1,
  khongThongKe: !!f.ao,
}))

export const danhSachGiaoDan: GiaoDanListItem[] = PEOPLE.map((p) => ({
  id: p.ma,
  maGiaoDanCu: Number(p.ma),
  tenThanh: p.tenThanh || null,
  hoTen: p.hoTen,
  phai: p.phai,
  ngaySinh: iso(p.ngaySinh),
  namSinh: p.ngaySinh ? p.ngaySinh.split('/')[2] : '',
  ngayRuaToi: iso(p.ngayRuaToi),
  ngayRuocLe: iso(p.ngayXtrl),
  ngayThemSuc: iso(p.ngayThemSuc),
  lapGd: p.lapGd,
  hoTenCha: p.cha || null,
  hoTenMe: p.me || null,
  tanTong: p.tanTong,
  conHoc: p.conHoc,
  ngheNghiep: p.ngheNghiep || null,
  ghiChu: p.ghiChu || null,
  dienThoai: p.dienThoai || null,
  diaChi: p.diaChi || null,
  tenGiaoHo: p.giaoHo,
  daChuyenDi: p.daChuyenDi,
  trinhDoVanHoa: p.vanHoa || null,
  trinhDoChuyenMon: p.chuyenMon || null,
  bietNgoaiNgu: p.ngoaiNgu || null,
  quaDoi: p.quaDoi,
  ngayQuaDoi: iso(p.ngayQuaDoi),
  noiAnTang: p.noiAnTang || null,
  noiSinh: p.noiSinh || null,
  noiRuaToi: p.noiRuaToi || null,
  noiRuocLe: p.noiXtrl || null,
  noiThemSuc: p.noiThemSuc || null,
  quanHe: p.quanHe || null,
  giaDinhId: p.maGiaDinh || null,
  khongThongKe: !!p.khongThongKe,
}))

/** Ánh xạ `quanHe` chữ của bản mẫu sang `vaiTro` số theo quy ước đã chốt của dự án: 0 =
 * Chồng, 1 = Vợ, 2 = Con (và mọi quan hệ khác — cháu, cha mẹ chồng… — cũng xếp vào 2 vì lưới
 * "Thành viên khác" chỉ cần phân biệt "là vợ/chồng hay không"). */
function vaiTroCua(quanHe: string): number {
  if (quanHe === 'Chồng') return 0
  if (quanHe === 'Vợ') return 1
  return 2
}

/** Chi tiết gia đình dựng tạm cho một mã gia đình — thành viên lấy từ `PEOPLE` có cùng
 * `maGiaDinh`. Đúng MỘT người (ưu tiên người có quanHe "Chồng") được đánh dấu `chuHo`, đúng
 * ngữ nghĩa chủ hộ theo hộ khẩu của bản desktop (`ImportData.cs` chỉ gán `ChuHo = true` cho
 * đúng một mã giáo dân) — KHÔNG đánh dấu cả hai vợ chồng như trước. */
export function timChiTietGiaDinh(id: string): GiaDinhDetail | undefined {
  const f = FAMILIES.find((x) => x.ma === id)
  if (!f) return undefined

  const thanhVienGiaDinh = PEOPLE.filter((p) => p.maGiaDinh === f.ma)
  const chuHoMa = thanhVienGiaDinh.find((p) => p.quanHe === 'Chồng')?.ma
    ?? thanhVienGiaDinh.find((p) => p.quanHe === 'Vợ')?.ma

  const thanhVien: ThanhVien[] = thanhVienGiaDinh.map((p) => ({
    giaoDanId: p.ma,
    vaiTro: vaiTroCua(p.quanHe),
    chuHo: p.ma === chuHoMa,
    tenThanh: p.tenThanh || null,
    hoTen: p.hoTen,
    phai: p.phai,
    ngaySinh: iso(p.ngaySinh),
    quaDoi: p.quaDoi,
    daXoa: false,
  }))

  return {
    id: f.ma,
    maGiaDinhCu: Number(f.ma),
    maGiaDinhRieng: null,
    tenGiaDinh: f.ten,
    giaoHoId: f.giaoho === 'Ngoài xứ' ? null : f.giaoho,
    dienThoai: f.dt || null,
    diaChi: f.diachi || null,
    soHoKhau: `HK-${f.ma}`,
    dienGiaDinh: f.dien || null,
    ghiChu: f.ghichu || null,
    daChuyenXu: false,
    ngayChuyen: null,
    noiChuyen: null,
    khongThongKe: !!f.ao,
    rowVersion: 1,
    thanhVien,
  }
}

/** Chi tiết giáo dân dựng tạm cho một mã giáo dân, kèm mã/tên gia đình để nút "Xem gia đình"
 * hoạt động được trong lúc chưa có backend. */
export function timChiTietGiaoDan(id: string): GiaoDanDetail | undefined {
  const p = PEOPLE.find((x) => x.ma === id)
  if (!p) return undefined
  const gd = FAMILIES.find((f) => f.ma === p.maGiaDinh)

  return {
    id: p.ma,
    maGiaoDanCu: Number(p.ma),
    hoTen: p.hoTen,
    tenThanh: p.tenThanh || null,
    phai: p.phai,
    ngaySinh: iso(p.ngaySinh),
    noiSinh: p.noiSinh || null,
    cmnd: null,
    danToc: null,
    giaoHoId: p.giaoHo === 'Ngoài xứ' ? null : p.giaoHo,
    diaChi: p.diaChi || null,
    dienThoai: p.dienThoai || null,
    email: null,
    hoTenCha: p.cha || null,
    hoTenMe: p.me || null,
    soRuaToi: null,
    ngayRuaToi: iso(p.ngayRuaToi),
    noiRuaToi: p.noiRuaToi || null,
    chaRuaToi: null,
    nguoiDoDauRuaToi: null,
    soRuocLe: null,
    ngayRuocLe: iso(p.ngayXtrl),
    noiRuocLe: p.noiXtrl || null,
    chaRuocLe: null,
    soThemSuc: null,
    ngayThemSuc: iso(p.ngayThemSuc),
    noiThemSuc: p.noiThemSuc || null,
    chaThemSuc: null,
    nguoiDoDauThemSuc: null,
    ngayXucDau: null,
    nguoiXucDau: null,
    tinhTrangXucDau: null,
    ghiChuXucDau: null,
    trinhDoVanHoa: p.vanHoa || null,
    trinhDoChuyenMon: p.chuyenMon || null,
    bietNgoaiNgu: p.ngoaiNgu || null,
    ngheNghiep: p.ngheNghiep || null,
    conHoc: p.conHoc,
    daCoGiaDinh: p.lapGd,
    tanTong: p.tanTong,
    khongThongKe: !!p.khongThongKe,
    quaDoi: p.quaDoi,
    ngayQuaDoi: iso(p.ngayQuaDoi),
    noiQuaDoi: null,
    soAnTang: null,
    noiAnTang: p.noiAnTang || null,
    ghiChu: p.ghiChu || null,
    giaDinhId: gd?.ma ?? null,
    tenGiaDinh: gd?.ten ?? null,
    vaiTro: null,
    rowVersion: 1,
  }
}
