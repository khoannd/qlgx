/** Ánh xạ 1-1 với GiaDinhListItemDto phía backend. */
export type GiaDinhListItem = {
  id: string
  maGiaDinhCu: number
  maGiaDinhRieng: string | null
  tenGiaDinh: string | null
  tenChong: string | null
  tenVo: string | null
  soLuong: number
  dienThoai: string | null
  dtChong: string | null
  dtVo: string | null
  diaChi: string | null
  tenGiaoHo: string | null
  dienGiaDinh: string | null
  ghiChu: string | null
  /** 0 = người nam đã qua đời, 1 = người nữ, 2 = cả hai, -1 = không gạch. */
  gach: number
  khongThongKe: boolean
}

/** Ánh xạ 1-1 với GiaoDanListItemDto phía backend. */
export type GiaoDanListItem = {
  id: string
  maGiaoDanCu: number
  tenThanh: string | null
  hoTen: string
  phai: string | null
  ngaySinh: string | null
  namSinh: string
  ngayRuaToi: string | null
  ngayRuocLe: string | null
  ngayThemSuc: string | null
  lapGd: boolean
  hoTenCha: string | null
  hoTenMe: string | null
  tanTong: boolean
  conHoc: boolean
  ngheNghiep: string | null
  ghiChu: string | null
  dienThoai: string | null
  diaChi: string | null
  tenGiaoHo: string | null
  daChuyenDi: boolean
  trinhDoVanHoa: string | null
  trinhDoChuyenMon: string | null
  bietNgoaiNgu: string | null
  quaDoi: boolean
  ngayQuaDoi: string | null
  noiAnTang: string | null
  noiSinh: string | null
  noiRuaToi: string | null
  noiRuocLe: string | null
  noiThemSuc: string | null
  /** Chỉ có giá trị khi lấy qua endpoint thành viên gia đình. */
  quanHe: string | null
  /** Mã gia đình giáo dân này thuộc về — null khi chưa gắn với gia đình nào. Dùng cho mục
   * menu chuột phải "Xem gia đình" (KHÔNG được dùng `id` của chính giáo dân để tra gia đình). */
  giaDinhId: string | null
  /** true khi giáo dân này KHÔNG được tính vào thống kê — khác khái niệm "Ngoài xứ": một
   * giáo dân ngoài xứ vẫn có thể được thống kê, nên không được suy ra từ `tenGiaoHo`. */
  khongThongKe: boolean
}

export type ThanhVien = {
  giaoDanId: string
  /** Quy ước đã chốt của dự án: 0 = Chồng, 1 = Vợ, 2 = Con. Dùng để tách "Người nam"/"Người
   * nữ" (0/1) khỏi lưới "Thành viên khác" (2) — KHÔNG dùng `chuHo` cho việc này, xem bên dưới. */
  vaiTro: number
  /** Đúng MỘT người trong gia đình có `chuHo = true` (chủ hộ theo hộ khẩu) — không nhất thiết
   * là người nam hay đã kết hôn. Không dùng trường này để suy ra "Người nam"/"Người nữ" hay để
   * lọc lưới thành viên; việc đó dựa vào `vaiTro`. */
  chuHo: boolean
  tenThanh: string | null
  hoTen: string
  phai: string | null
  ngaySinh: string | null
  quaDoi: boolean
  daXoa: boolean
}

export type GiaDinhDetail = {
  id: string
  maGiaDinhCu: number
  maGiaDinhRieng: string | null
  tenGiaDinh: string | null
  giaoHoId: string | null
  dienThoai: string | null
  diaChi: string | null
  soHoKhau: string | null
  dienGiaDinh: string | null
  ghiChu: string | null
  daChuyenXu: boolean
  ngayChuyen: string | null
  noiChuyen: string | null
  khongThongKe: boolean
  rowVersion: number
  thanhVien: ThanhVien[]
}

/** Các trường màn hình chi tiết giáo dân dùng tới; xem GiaoDanDetailDto phía backend. */
export type GiaoDanDetail = {
  id: string
  maGiaoDanCu: number
  hoTen: string
  tenThanh: string | null
  phai: string | null
  ngaySinh: string | null
  noiSinh: string | null
  cmnd: string | null
  danToc: string | null
  giaoHoId: string | null
  diaChi: string | null
  dienThoai: string | null
  email: string | null
  hoTenCha: string | null
  hoTenMe: string | null
  soRuaToi: string | null
  ngayRuaToi: string | null
  noiRuaToi: string | null
  chaRuaToi: string | null
  nguoiDoDauRuaToi: string | null
  soRuocLe: string | null
  ngayRuocLe: string | null
  noiRuocLe: string | null
  chaRuocLe: string | null
  soThemSuc: string | null
  ngayThemSuc: string | null
  noiThemSuc: string | null
  chaThemSuc: string | null
  nguoiDoDauThemSuc: string | null
  ngayXucDau: string | null
  nguoiXucDau: string | null
  tinhTrangXucDau: string | null
  ghiChuXucDau: string | null
  trinhDoVanHoa: string | null
  trinhDoChuyenMon: string | null
  bietNgoaiNgu: string | null
  ngheNghiep: string | null
  conHoc: boolean
  daCoGiaDinh: boolean
  tanTong: boolean
  khongThongKe: boolean
  quaDoi: boolean
  ngayQuaDoi: string | null
  noiQuaDoi: string | null
  soAnTang: string | null
  noiAnTang: string | null
  ghiChu: string | null
  giaDinhId: string | null
  tenGiaDinh: string | null
  vaiTro: number | null
  rowVersion: number
}
