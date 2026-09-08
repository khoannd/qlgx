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

/** Ánh xạ 1-1 với KiemTraGiaoDanKetQuaDto phía backend — một dòng kết quả của "Công cụ dữ
 * liệu" → "Kiểm tra dữ liệu" (xem docs/superpowers/specs/man-hinh/cong-cu-du-lieu.md mục 2). */
export type KiemTraGiaoDanKetQua = {
  giaoDan: GiaoDanListItem
  /** Các lý do vi phạm, mỗi lý do một dòng bắt đầu bằng "- ", nối bằng "\n". */
  nguyenNhan: string
  /** Tổng cờ bit ReviewGiaoDanType đã vi phạm — chỉ giữ để đối chiếu, KHÔNG hiển thị trên lưới. */
  ketQua: number
}

/** Sáu tuỳ chọn của "Kiểm tra dữ liệu — giáo dân", đúng tên tham số truy vấn phía backend. */
export type KiemTraGiaoDanTuyChon = {
  khongCoNgayThang: boolean
  saiQuanHeNgayThang: boolean
  ruocLeTruocTuoi: boolean
  thuocNhieuGiaDinh: boolean
  khongThuocGiaDinhNao: boolean
  coNhieuHonPhoi: boolean
}

export type ThanhVien = {
  giaoDanId: string
  /** Mã giáo dân hệ cũ (Access) — khác `giaoDanId` (Guid). Lưới "Thành viên khác trong gia
   * đình" hiển thị cột "Mã GD" từ đây (người dùng làm việc theo mã cũ này). */
  maGiaoDanCu: number
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
  /** Ánh xạ 1-1 với `HonPhoiDto?` phía backend — hôn phối "hiện tại" của gia đình (chọn theo
   * `ChonHonPhoiHienTai`, xem GiaDinhService), `null` khi gia đình chưa có hôn phối nào (chưa
   * đủ Người nam + Người nữ, hoặc chưa từng lưu khối hôn phối). Trước đây bản web chỉ ĐỌC field
   * này chứ không hiện lên đâu cả — xem can-review-sau.md mục "Khối hôn phối". */
  honPhoi: {
    id: string
    soHonPhoi: string | null
    ngayHonPhoi: string | null
    noiHonPhoi: string | null
    linhMucChung: string | null
    nguoiChung1: string | null
    nguoiChung2: string | null
    cachThucHonPhoi: string | null
    ghiChu: string | null
    rowVersion: number
  } | null
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
/** LoaiBiTich phía backend — chỉ 3 giá trị có màn hình "Danh sách sổ bí tích" xử lý (Hôn
 * phối có màn hình riêng, xem hon-phoi.md; An táng/Xức dầu chưa có cột GiaoDan tương ứng). */
export type LoaiBiTich = 0 | 1 | 2

/** Một dòng trên lưới "Danh sách sổ bí tích" — xem so-bi-tich.md mục 6. */
export type DotBiTichListItem = {
  id: string
  maDotBiTichCu: number
  loaiBiTich: LoaiBiTich
  ngayBiTich: string | null
  moTa: string | null
  linhMuc: string | null
  noiBiTich: string | null
  soLuong: number
}

/** Một người trong danh sách nhận bí tích của một đợt — xem so-bi-tich.md mục 6. */
export type NguoiNhanBiTich = {
  giaoDanId: string
  maGiaoDanCu: number
  tenThanh: string | null
  hoTen: string
  phai: string | null
  ngaySinh: string | null
  soBiTich: string | null
  nguoiDoDau: string | null
  ghiChu: string | null
}

export type DotBiTichDetail = {
  id: string
  maDotBiTichCu: number
  loaiBiTich: LoaiBiTich
  ngayBiTich: string | null
  moTa: string | null
  linhMuc: string | null
  noiBiTich: string | null
  rowVersion: number
  nguoiNhan: NguoiNhanBiTich[]
}

/** Một dòng trên lưới "Danh sách rao hôn phối" — xem rao-hon-phoi.md mục 6. */
export type RaoHonPhoiListItem = {
  id: string
  maRaoHonPhoiCu: number
  tenRaoHonPhoi: string | null
  nguoi1: string | null
  nguoi2: string | null
  ngayRaoLan1: string | null
  ngayRaoLan2: string | null
  ngayRaoLan3: string | null
  ghiChu: string | null
}

/** Chi tiết đầy đủ 26 cột — xem rao-hon-phoi.md mục 2. */
export type RaoHonPhoiDetail = {
  id: string
  maRaoHonPhoiCu: number
  tenRaoHonPhoi: string | null
  giaoDan1Id: string | null
  tenGiaoDan1: string | null
  giaoDan2Id: string | null
  tenGiaoDan2: string | null
  ngayRaoLan1: string | null
  ngayRaoLan2: string | null
  ngayRaoLan3: string | null
  giaoXu1: string | null
  giaoPhan1: string | null
  giaoXuTruoc1: string | null
  giaoPhanTruoc1: string | null
  giaoXu2: string | null
  giaoPhan2: string | null
  giaoXuTruoc2: string | null
  giaoPhanTruoc2: string | null
  linhMucNhan: string | null
  giaoXuNhan: string | null
  ghiChu: string | null
  tam1: string | null
  tam2: string | null
  tam3: string | null
  giaoXuNQ1: string | null
  giaoPhanNQ1: string | null
  giaoXuNQ2: string | null
  giaoPhanNQ2: string | null
  rowVersion: number
}

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

/** Ánh xạ 1-1 với HoiDoanQuanLyDto — một dòng trên màn hình "Danh sách hội đoàn" (cấp quản lý
 * danh mục, khác HoiDoanDanhMuc/HoiDoanCuaGiaoDan ở trên — xem
 * docs/superpowers/specs/man-hinh/hoi-doan-danh-sach.md). */
export type HoiDoanQuanLy = {
  id: string
  maHoiDoanCu: number
  tenHoiDoan: string
  thanhBonMang: string | null
  ngayBonMang: string | null
  ngayThanhLap: string | null
  ghiChu: string | null
  soHoiVienDangHoatDong: number
  rowVersion: number
}

/** Ánh xạ 1-1 với ThanhVienHoiDoanDto — một hội viên trên lưới của màn hình chi tiết hội
 * đoàn. */
export type ThanhVienHoiDoan = {
  chiTietId: string
  giaoDanId: string
  hoTen: string
  tenThanh: string | null
  ngayVaoHoiDoan: string | null
  ngayRaHoiDoan: string | null
  vaiTro: string | null
  daRaKhoiHoiDoan: boolean
  rowVersion: number
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

/** Ánh xạ 1-1 với GiaoXuLuaChonDto — dùng khi tên đăng nhập trùng ở nhiều giáo xứ và máy chủ
 * yêu cầu người dùng chọn đúng giáo xứ trước khi đăng nhập (xem AuthService.DangNhap). */
export type GiaoXuLuaChon = { id: string; tenGiaoXu: string }

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

// --- Màn hình "Quản lý giáo phận/giáo hạt/giáo xứ" (policy "QuanTriHeThong", xem
// docs/superpowers/specs/man-hinh/quan-ly-giao-xu.md) — CỐ Ý xuyên giáo xứ, khác mọi type
// khác ở trên vốn luôn nằm trong phạm vi giáo xứ của người gọi. ---

/** Ánh xạ 1-1 với GiaoPhanDto. */
export type GiaoPhan = { id: string; tenGiaoPhan: string; ghiChu: string | null }

/** Ánh xạ 1-1 với GiaoHatDto. */
export type GiaoHatQuanLy = {
  id: string
  giaoPhanId: string
  tenGiaoPhan: string
  tenGiaoHat: string
  ghiChu: string | null
}

/** Ánh xạ 1-1 với GiaoXuDto. `coTrungTen` chỉ là cảnh báo (không chặn) khi trùng tên với giáo
 * xứ khác đã có — xem spec mục 4. `soTaiKhoan` cho biết giáo xứ đã có quản trị viên hay chưa. */
export type GiaoXuQuanLy = {
  id: string
  giaoHatId: string | null
  tenGiaoHat: string | null
  tenGiaoPhan: string | null
  tenGiaoXu: string
  diaChi: string | null
  dienThoai: string | null
  email: string | null
  website: string | null
  ghiChu: string | null
  coTrungTen: boolean
  soTaiKhoan: number
}

// --- Màn hình "Nhập dữ liệu Access" (policy "QuanTriHeThong", VIEC-TIEP-THEO.md mục 2.4) ---

/** Ánh xạ 1-1 với DongDoiChieuDto — một dòng đối chiếu số dòng nguồn/đích theo bảng. */
export type DongDoiChieu = { bang: string; soDongNguon: number; soDongDich: number; lech: boolean }

/** Ánh xạ 1-1 với BaoCaoXemTruocDto — kết quả CHẠY THỬ, KHÔNG ghi gì. */
export type BaoCaoXemTruoc = {
  tenGiaoXuNguon: string
  giaoXuDichDaCoDuLieu: boolean
  soGiaoDanDaCo: number
  doiChieu: DongDoiChieu[]
  canhBao: string[]
}

/** Ánh xạ 1-1 với TrangThaiNhapDuLieuDto — "DangChay" | "HoanThanh" | "Loi". */
export type TrangThaiNhapDuLieu = {
  jobId: string
  trangThai: 'DangChay' | 'HoanThanh' | 'Loi'
  doiChieu: DongDoiChieu[] | null
  canhBao: string[] | null
  loiThongBao: string | null
  batDauLuc: string
  ketThucLuc: string | null
}

/** Ánh xạ 1-1 với KhoiGiaoLyDto — một dòng trên màn hình "Danh mục khối giáo lý"
 * (`frmKhoiGiaoLyList.cs`) — xem docs/superpowers/specs/man-hinh/giao-ly.md. */
export type KhoiGiaoLy = {
  id: string
  maKhoiCu: number
  tenKhoi: string
  nguoiQuanLyId: string | null
  tenNguoiQuanLy: string | null
  ghiChu: string | null
  soLop: number
  rowVersion: number
}

/** Ánh xạ 1-1 với LopGiaoLyDto — một lớp thuộc một khối giáo lý (`frmLopGiaoLy.cs`). */
export type LopGiaoLy = {
  id: string
  maLopCu: number
  tenLop: string
  khoiGiaoLyId: string
  nam: number | null
  phongHoc: string | null
  ghiChu: string | null
  soHocVien: number
  tenGiaoLyVien: string | null
  rowVersion: number
}

/** Ánh xạ 1-1 với HocVienLopGiaoLyDto — một học viên trên lưới `gxHocSinhList1`. */
export type HocVienLopGiaoLy = {
  chiTietId: string
  giaoDanId: string
  soThuTu: number | null
  hoTen: string
  tenThanh: string | null
  phai: string | null
  ngaySinh: string | null
  hoanThanh: boolean
  ghiChuGLy: string | null
  rowVersion: number
}

/** Ánh xạ 1-1 với GiaoLyVienDto — một giáo lý viên phụ trách một lớp. */
export type GiaoLyVienLop = {
  id: string
  giaoDanId: string
  hoTen: string
  tenThanh: string | null
  rowVersion: number
}

// ============ Thống kê chung & Biểu đồ (xem docs/superpowers/specs/man-hinh/thong-ke-bieu-do.md) ============

/** 16 điều kiện trích xuất của tab "Thống kê chung" — ĐÚNG tên enum `DieuKienThongKe` phía
 * backend (thứ tự không quan trọng ở phía TypeScript, backend so theo tên chuỗi). */
export type DieuKienThongKe =
  | 'SinhRa' | 'RuaToi' | 'RuocLeLanDau' | 'ThemSuc' | 'HonPhoi' | 'QuaDoi'
  | 'TongSoGiaoDan' | 'TongSoGiaDinh' | 'TanTong' | 'ChuHo' | 'GiaTruong' | 'HienMau'
  | 'CaoNien' | 'GioiTre' | 'ThieuNhi' | 'KyNiemHonPhoi'

export type TrangThaiHonPhoiThongKe =
  'KhongPhanLoai' | 'Chuan' | 'HopThucHoa' | 'HopPhap' | 'LyDi' | 'LyThan'

/** Ánh xạ 1-1 với HonPhoiThongKeDto phía backend. */
export type HonPhoiThongKeItem = {
  id: string
  maHonPhoiCu: number
  tenHonPhoi: string | null
  soHonPhoi: string | null
  noiHonPhoi: string | null
  ngayHonPhoi: string | null
  linhMucChung: string | null
  cachThucHonPhoi: string | null
  ghiChu: string | null
  tenChong: string | null
  tenVo: string | null
  tenGiaoHo: string | null
}

/** Ánh xạ 1-1 với ThongKeChungKetQua — CHỈ MỘT trong ba mảng khác null, tương ứng đúng MỘT
 * trong ba lưới chồng nhau của bản desktop. */
export type ThongKeChungKetQua = {
  tongCong: number
  nhan: string
  giaoDan: GiaoDanListItem[] | null
  giaDinh: GiaDinhListItem[] | null
  honPhoi: HonPhoiThongKeItem[] | null
}

/** Ánh xạ 1-1 với OnGoiListItemDto — một dòng của tab "Thống kê ơn gọi tận hiến". */
export type OnGoiListItem = {
  id: string
  maGiaoDanCu: number
  tenThanh: string | null
  hoTen: string
  phai: string | null
  ngaySinh: string | null
  dienThoai: string | null
  diaChi: string | null
  tenGiaoHo: string | null
  ngayBatDau: string | null
  chucVu: string | null
  noiTu: string | null
  dongTu: string | null
  noiPhucVu: string | null
}

export type ThongKeOnGoiKetQua = { tongCong: number; rows: OnGoiListItem[] }

/** Một năm của biểu đồ "Tổng giáo dân" (luỹ kế) hoặc "Tổng hôn phối" (theo từng năm). */
export type BieuDoNam = { nam: number; soLuong: number }

/** Một năm của biểu đồ "Tình hình bí tích" — 4 chuỗi. */
export type BieuDoBiTichNam = {
  nam: number
  sinhRa: number
  ruaToi: number
  xtrlLanDau: number
  themSuc: number
}

/** 7 nhóm tuổi cố định của biểu đồ "So sánh độ tuổi" — `tren50` LUÔN bằng 0 cho tới năm 2041
 * (bug cận đảo ngược của bản gốc, migrate y hệt — xem thong-ke-bieu-do.md mục 4.6). */
export type BieuDoDoTuoi = {
  duoi7: number
  tu7Den12: number
  tu13Den16: number
  tu17Den25: number
  tu26Den30: number
  tu31Den50: number
  tren50: number
}

export type BieuDoGiaoHo = { tenGiaoHo: string; soLuong: number }
