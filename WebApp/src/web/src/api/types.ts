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
  /** Liên kết tới một giáo dân có sẵn khi Tên Cha/Mẹ được chọn qua GxPicker thật — null nếu
   * chỉ gõ tay hoặc dữ liệu cũ chưa gán. Dùng để Rule 15 (tuổi cha/mẹ) tính được ở máy chủ. */
  chaId: string | null
  meId: string | null
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
  // --- Tab "Giáo lý" (Bao đồng 1/2, Vào đời, Hôn nhân) — xem GiaoDanDetailDto phía backend.
  ngayBD1: string | null
  noiBD1: string | null
  ngayBD2: string | null
  noiBD2: string | null
  ngayTHVaoDoi: string | null
  noiTHVaoDoi: string | null
  ngayGLHN1: string | null
  ngayGLHN2: string | null
  noiGLHN: string | null
  nguoiChungNhanGLHN: string | null
  xepLoaiGLHN: string | null
  // --- Thông tin chuyển xứ (bảng `chuyen_xu`) — null nếu chưa từng chuyển xứ (mặc định "Ở tại
  // xứ"). Xem ChuyenXuDto phía backend.
  chuyenXu: {
    id: string
    /** 0 = Ở tại xứ, 1 = Chuyển đến, 2 = Chuyển đi — xem Qlgx.Domain.LoaiChuyenXu. */
    loaiChuyen: number
    ngayChuyen: string | null
    noiChuyen: string | null
    ghiChuChuyen: string | null
    rowVersion: number
  } | null
  giaDinhId: string | null
  tenGiaDinh: string | null
  vaiTro: number | null
  rowVersion: number
}

/** Ánh xạ 1-1 với HonPhoiCuaGiaoDanDto phía backend — MỘT hôn phối của giáo dân đang xem
 * (người này có thể có nhiều bản ghi theo thời gian, xem tab "Hôn phối" trong
 * GiaoDanDetail.tsx và docs/superpowers/specs/man-hinh/hon-phoi.md mục 8). */
export type HonPhoiCuaGiaoDan = {
  id: string
  tenHonPhoi: string | null
  soHonPhoi: string | null
  ngayHonPhoi: string | null
  noiHonPhoi: string | null
  linhMucChung: string | null
  nguoiChung1: string | null
  nguoiChung2: string | null
  cachThucHonPhoi: string | null
  ghiChu: string | null
  voChongId: string | null
  tenVoChong: string | null
  rowVersion: number
}

/** Ánh xạ 1-1 với TanHienCuaGiaoDanDto phía backend — MỘT bản ghi Ơn gọi tận hiến của giáo dân
 * đang xem. Bản desktop (`GxTanHien`) chỉ hỗ trợ MỘT dòng/giáo dân (xem
 * docs/superpowers/specs/man-hinh/tan-hien.md mục 3), bản web mở rộng có chủ đích thành danh
 * sách. */
export type TanHienCuaGiaoDan = {
  id: string
  ngayBatDau: string | null
  chucVu: string | null
  noiTu: string | null
  dongTu: string | null
  noiPhucVu: string | null
  diaChiPhucVu: string | null
  dienThoaiPhucVu: string | null
  emailPhucVu: string | null
  ghiChu: string | null
  daHoiTuc: boolean
  ngayVaoDCV: string | null
  ngayVaoNhaThu: string | null
  ngayVaoNhaTap: string | null
  ngayVaoKhanLanDau: string | null
  ngayVaoKhanTronDoi: string | null
  ngayPhoTe: string | null
  ngayThuPhongLM: string | null
  ngayBonMang: string | null
  rowVersion: number
}

/** Ánh xạ 1-1 với HoiDoanDanhMucDto — một hội đoàn trong danh mục của giáo xứ, dùng cho combo
 * "Tên hội đoàn" khi thêm một lượt tham gia mới. */
export type HoiDoanDanhMuc = {
  id: string
  tenHoiDoan: string
}

/** Ánh xạ 1-1 với HoiDoanCuaGiaoDanDto phía backend — MỘT lượt tham gia hội đoàn của giáo dân
 * đang xem. Bản desktop (`GxHistoryHoiDoan`) chỉ cho xem lịch sử và thêm mới (không sửa/xoá được
 * lượt đã có) — bản web mở rộng có chủ đích cho sửa, xem hoi-doan.md mục 8. */
export type HoiDoanCuaGiaoDan = {
  id: string
  hoiDoanId: string
  tenHoiDoan: string | null
  ngayVaoHoiDoan: string | null
  ngayRaHoiDoan: string | null
  vaiTro: string | null
  rowVersion: number
}

/** Ánh xạ 1-1 với GiaoHoDto — một dòng danh mục Giáo họ THẬT (thay data/giaoHoTam.ts hard-code
 * theo tên). MaGiaoHoCu chỉ để hiển thị/đối chiếu, không dùng làm khoá khi lưu — lưu bằng `id`. */
export type GiaoHo = {
  id: string
  maGiaoHoCu: number
  tenGiaoHo: string
  giaoHoChaId: string | null
}

/** Ánh xạ 1-1 với GiaoDanTimKiemDto — một kết quả tìm kiếm cho GxPicker thật (gõ để tìm Tên
 * Cha/Mẹ, Người nam/nữ…), KHÔNG phải bộ cột đầy đủ của lưới danh sách giáo dân. */
export type GiaoDanTimKiem = {
  id: string
  maGiaoDanCu: number
  tenThanh: string | null
  hoTen: string
  phai: string | null
  ngaySinh: string | null
}

/** Ánh xạ 1-1 với ThongTinNguoiDungDto (backend) — trả về sau đăng nhập thành công. */
export type ThongTinNguoiDung = {
  id: string
  tenTaiKhoan: string
  hoTen: string | null
  loaiTaiKhoan: number
  giaoXuId: string
  tenGiaoXu: string
}

/** Ánh xạ 1-1 với thân trả về của GET /api/suc-khoe — dùng để hiện đúng phiên bản bản web
 * thật ở chân thanh bên (SideNav), thay cho số hiệu bản desktop viết cứng cũ. */
export type SucKhoe = {
  trangThai: string
  phienBan: string
}

/** Ánh xạ 1-1 với thân trả về của POST /api/auth/dang-nhap. */
export type DangNhapKetQua = {
  token: string
  hetHanSau: number
  nguoiDung: ThongTinNguoiDung
}

/** Ánh xạ 1-1 với TaiKhoanItemDto — một dòng lưới màn hình Quản lý tài khoản. */
export type TaiKhoanItem = {
  id: string
  tenTaiKhoan: string
  hoTenNguoiDung: string | null
  email: string | null
  soDienThoai: string | null
  loaiTaiKhoan: number
  tenLoai: string | null
  rowVersion: number
}
