namespace Qlgx.Migration;

/// <summary>
/// Ánh xạ đủ 11 cột thật của bảng GiaoXu. KHÔNG có cột TenGiaoHat/TenGiaoPhan trong Access —
/// hai tên đó không tồn tại; cột thật là MaGiaoHat (số, ánh xạ vào GiaoXu.MaGiaoHatCu — khoá
/// tới danh mục giáo hạt hệ cũ, không tách bảng riêng trong phạm vi chuyển đổi này). Hinh là
/// văn bản (không phải ảnh nhị phân); LastUpload là ngày giờ thật của Access. Xem
/// schema-access-that.md.
/// </summary>
public record DongGiaoXu(int MaGiaoXu, int? MaGiaoHat, string TenGiaoXu, string? DiaChi,
    string? DienThoai, string? Email, string? Website, string? Hinh, string? GhiChu,
    int? MaGiaoXuRieng, DateTime? LastUpload);

public record DongGiaoHo(int MaGiaoHo, string TenGiaoHo, int? MaGiaoHoCha, bool DaXoa,
    string? MaNhanDang, DateTime? UpdateDate);

public record DongGiaDinh(int MaGiaDinh, int? MaGiaoHo, string? TenGiaDinh, string? GhiChu,
    string? DiaChi, string? DienThoai, string? SoHoKhau, string? DienGiaDinh, bool DaXoa,
    bool DaChuyenXu, string? NgayChuyen, string? NoiChuyen, bool GiaDinhAo, string? MaNhanDang,
    string? MaGiaDinhRieng, string? AnhDaiDien, DateTime? UpdateDate);

/// <summary>
/// Ánh xạ đủ 63 cột thật của bảng GiaoDan (xem schema-access-that.md). UpdateDate ánh xạ vào
/// ThucTheCoSo.UpdatedAt; GiaoDanAo ánh xạ vào GiaoDan.KhongThongKe.
/// </summary>
public record DongGiaoDan(
    int MaGiaoDan, string HoTen, int? MaGiaoHo, string? Phai, string? TenThanh, string? NgaySinh,
    string? NoiSinh, string? SoRuaToi, string? NgayRuaToi, string? NoiRuaToi, string? ChaRuaToi,
    string? NguoiDoDauRuaToi, string? NgayRuocLe, string? NoiRuocLe, string? ChaRuocLe,
    string? SoThemSuc, string? NgayThemSuc, string? NoiThemSuc, string? ChaThemSuc,
    string? NguoiDoDauThemSuc, string? TrinhDoVanHoa, string? NgheNghiep, bool ConHoc,
    bool QuaDoi, string? NgayQuaDoi, string? DienThoai, string? Email, bool DaXoa,
    string? GhiChu, DateTime? UpdateDate, string? SoRuocLe, string? HoTenCha, string? HoTenMe,
    bool DaCoGiaDinh, bool GiaoDanAo, bool TanTong, string? MaNhanDang, string? ThuocGiaoXu,
    string? ThuocGiaoPhan, string? DiaChi, string? DanToc, string? NoiQuaDoi, string? SoAnTang,
    string? NoiAnTang, string? AnhDaiDien, string? CMND, string? TrinhDoChuyenMon,
    string? BietNgoaiNgu, string? NgayXucDau, string? NguoiXucDau, string? TinhTrangXucDau,
    string? GhiChuXucDau, string? NgayBD1, string? NoiBD1, string? NgayBD2, string? NoiBD2,
    string? NgayTHVaoDoi, string? NoiTHVaoDoi, string? NgayGLHN1, string? NgayGLHN2,
    string? NoiGLHN, string? NguoiChungNhanGLHN, string? XepLoaiGLHN);

public record DongThanhVien(int MaGiaDinh, int MaGiaoDan, int VaiTro, bool ChuHo);

/// <summary>
/// Ánh xạ bảng HonPhoi trong Access. Không có cột xoá mềm — đúng như bản gốc, xem
/// Qlgx.Domain.Entities.HonPhoi. UpdateDate ánh xạ vào ThucTheCoSo.UpdatedAt.
/// </summary>
public record DongHonPhoi(int MaHonPhoi, string? TenHonPhoi, string? SoHonPhoi, string? NoiHonPhoi,
    string? NgayHonPhoi, string? LinhMucChung, string? NguoiChung1, string? NguoiChung2,
    string? CachThucHonPhoi, string? GhiChu, string? MaNhanDang, DateTime? UpdateDate);

/// <summary>Bảng nối giáo dân với hôn phối — khoá tổ hợp (MaGiaoDan, MaHonPhoi).</summary>
public record DongGiaoDanHonPhoi(int MaGiaoDan, int MaHonPhoi, int SoThuTu);

/// <summary>Giáo phận — trên cấp giáo xứ (xem Qlgx.Domain.Entities.GiaoPhan).</summary>
public record DongGiaoPhan(int MaGiaoPhan, string TenGiaoPhan, string? GhiChu, int? MaGiaoPhanRieng);

/// <summary>Giáo hạt — trên cấp giáo xứ, thuộc một giáo phận (xem Qlgx.Domain.Entities.GiaoHat).</summary>
public record DongGiaoHat(int MaGiaoHat, int MaGiaoPhan, string TenGiaoHat, string? GhiChu,
    int? MaGiaoHatRieng);

/// <summary>
/// Cấu hình theo giáo xứ. GiaTri là LongText trong Access (có thể dài, ví dụ TEMPLATE_FOLDER
/// là đường dẫn thư mục cục bộ của máy chạy bản desktop) — vẫn chuyển nguyên văn để không mất
/// dữ liệu, nhưng máy chủ tập trung KHÔNG được dùng giá trị đó để ghi file (xem ChuyenDoiDuLieu).
/// </summary>
public record DongCauHinh(string MaCauHinh, string? GiaTri, string? MoTa, DateTime? UpdateDate);

public record DongDuLieuChung(int ID, int LoaiDuLieu, string? MaDuLieu, string? DuLieu1, string? DuLieu2);

public record DongVaiTro(int ID, string? Value);

public record DongTenLoaiTaiKhoan(int ID, string? TenLoai);

/// <summary>
/// KHÔNG có cột MatKhau — quyết định bảo mật đã chốt là không chuyển mật khẩu cũ sang hệ mới
/// (xem Qlgx.Domain.Entities.TaiKhoan).
/// </summary>
public record DongTaiKhoan(string? HoTenNguoiDung, string TenTaiKhoan, string? Email,
    string? SoDienThoai, int LoaiTaiKhoan, string? CauHoiGoiY, string? CauTraLoiGoiY, bool DaXoa);

/// <summary>
/// Đợt cử hành bí tích. Có 6 thuộc tính riêng vì UpdateDate ánh xạ vào ThucTheCoSo.UpdatedAt.
/// LoaiBiTich là số nguyên thô khớp enum Qlgx.Domain.LoaiBiTich (đã đối chiếu Source/ để xác
/// nhận: 0=RuaToi, 1=RuocLe, 2=ThemSuc).
/// </summary>
public record DongDotBiTich(int MaDotBiTich, string? NgayBiTich, string? MoTa, string? LinhMuc,
    int LoaiBiTich, string? NoiBiTich, DateTime? UpdateDate);

/// <summary>
/// Bảng lớn nhất CSDL (6150 dòng thật) — ai nhận bí tích trong đợt nào. Khoá gốc Access là cặp
/// (MaDotBiTich, MaGiaoDan), đã xác nhận không trùng lặp trên dữ liệu thật.
/// </summary>
public record DongBiTichChiTiet(int MaDotBiTich, int MaGiaoDan, string? GhiChu, DateTime? UpdateDate);

/// <summary>
/// Giáo dân chuyển xứ. LoaiChuyen khớp enum Qlgx.Domain.LoaiChuyenXu (đã đối chiếu Source/:
/// 0=TaiXu, 1=ChuyenDen, 2=ChuyenDi).
/// </summary>
public record DongChuyenXu(int MaChuyenXu, int MaGiaoDan, string? NgayChuyen, string? NoiChuyen,
    int LoaiChuyen, string? GhiChuChuyen, DateTime? UpdateDate);

/// <summary>
/// Rao hôn phối — 26 cột Access, có 25 thuộc tính riêng vì UpdateDate ánh xạ vào
/// ThucTheCoSo.UpdatedAt. MaGiaoDan1/MaGiaoDan2 có thể null/0 khi một bên không phải giáo dân
/// của xứ này (xem Qlgx.Domain.Entities.RaoHonPhoi).
/// </summary>
public record DongRaoHonPhoi(int MaRaoHonPhoi, string? TenRaoHonPhoi, int? MaGiaoDan1,
    int? MaGiaoDan2, string? NgayRaoLan1, string? NgayRaoLan2, string? NgayRaoLan3,
    string? GiaoXu1, string? GiaoPhan1, string? GiaoXuTruoc1, string? GiaoPhanTruoc1,
    string? GiaoXu2, string? GiaoPhan2, string? GiaoXuTruoc2, string? GiaoPhanTruoc2,
    string? LinhMucNhan, string? GiaoXuNhan, string? GhiChu, string? Tam1, string? Tam2,
    string? Tam3, DateTime? UpdateDate, string? GiaoXuNQ1, string? GiaoPhanNQ1,
    string? GiaoXuNQ2, string? GiaoPhanNQ2);

/// <summary>
/// Tận hiến (tu sĩ, linh mục xuất thân từ giáo xứ) — 20 cột Access. KHÔNG có UpdateDate trong
/// Access (xem Qlgx.Domain.Entities.TanHien).
/// </summary>
public record DongTanHien(int MaTanHien, int MaGiaoDan, string? NgayBatDau, string? ChucVu,
    string? NoiTu, string? DongTu, string? NoiPhucVu, string? DiaChiPhucVu,
    string? DienThoaiPhucVu, string? EmailPhucVu, string? GhiChu, bool DaHoiTuc,
    string? NgayVaoDCV, string? NgayVaoNhaThu, string? NgayVaoNhaTap, string? NgayVaoKhanLanDau,
    string? NgayVaoKhanTronDoi, string? NgayPhoTe, string? NgayThuPhongLM, string? NgayBonMang);

/// <summary>Danh sách linh mục của giáo xứ — 12 cột Access, 11 thuộc tính riêng (UpdateDate).</summary>
public record DongLinhMuc(int MaLinhMuc, string? TenThanh, string HoTen, string? NgaySinh,
    string? ChucVu, string? TuNgay, string? DenNgay, string? GhiChu, string? DienThoai,
    string? Email, bool DaXoa, DateTime? UpdateDate);

/// <summary>
/// Khối giáo lý — 4 cột Access. KHÔNG có UpdateDate (xem schema-19-bang-con-lai.md).
/// NguoiQuanLy là MaGiaoDan của người quản lý khối, -1 hoặc 0 nghĩa là chưa gán ai (xem
/// Qlgx.Domain.Entities.KhoiGiaoLy).
/// </summary>
public record DongKhoiGiaoLy(int MaKhoi, string TenKhoi, int NguoiQuanLy, string? GhiChu);

/// <summary>Lớp giáo lý — 6 cột Access, thuộc một khối. KHÔNG có UpdateDate.</summary>
public record DongLopGiaoLy(int MaLop, string TenLop, int MaKhoi, int? Nam, string? PhongHoc,
    string? GhiChu);

/// <summary>
/// Học viên trong một lớp giáo lý — 5 cột Access. Khoá gốc là cặp (MaLop, MaGiaoDan). KHÔNG có
/// UpdateDate.
/// </summary>
public record DongChiTietLopGiaoLy(int MaLop, int MaGiaoDan, int? SoThuTu, bool HoanThanh,
    string? GhiChuGLy);

/// <summary>
/// Giáo lý viên phụ trách lớp — chỉ 2 cột Access, khoá gốc là cặp (MaLop, MaGiaoDan). KHÔNG có
/// UpdateDate.
/// </summary>
public record DongGiaoLyVien(int MaLop, int MaGiaoDan);

/// <summary>Hội đoàn — 6 cột Access. KHÔNG có UpdateDate.</summary>
public record DongHoiDoan(int MaHoiDoan, string TenHoiDoan, string? ThanhBonMang,
    string? NgayBonMang, string? NgayThanhLap, string? GhiChu);

/// <summary>
/// Thành viên hội đoàn — 6 cột Access, có cột ID riêng (không phải khoá tổ hợp). VaiTro ở đây
/// là Text (chức vụ trong hội đoàn) — KHÁC HẲN DongThanhVien.VaiTro (số, vai trò trong gia
/// đình). KHÔNG có UpdateDate.
/// </summary>
public record DongChiTietHoiDoan(int ID, int MaHoiDoan, int MaGiaoDan, string? NgayVaoHoiDoan,
    string? NgayRaHoiDoan, string? VaiTro);

/// <summary>
/// Trừu tượng hoá nguồn để bộ chuyển đổi kiểm thử được mà không cần file .mdb và Access
/// Database Engine trên máy chạy test.
/// </summary>
public interface IDuLieuNguon
{
    IEnumerable<DongGiaoXu> DocGiaoXu();
    IEnumerable<DongGiaoHo> DocGiaoHo();
    IEnumerable<DongGiaDinh> DocGiaDinh();
    IEnumerable<DongGiaoDan> DocGiaoDan();
    IEnumerable<DongThanhVien> DocThanhVien();
    IEnumerable<DongHonPhoi> DocHonPhoi();
    IEnumerable<DongGiaoDanHonPhoi> DocGiaoDanHonPhoi();
    IEnumerable<DongGiaoPhan> DocGiaoPhan();
    IEnumerable<DongGiaoHat> DocGiaoHat();
    IEnumerable<DongCauHinh> DocCauHinh();
    IEnumerable<DongDuLieuChung> DocDuLieuChung();
    IEnumerable<DongVaiTro> DocVaiTro();
    IEnumerable<DongTenLoaiTaiKhoan> DocTenLoaiTaiKhoan();
    IEnumerable<DongTaiKhoan> DocTaiKhoan();
    IEnumerable<DongDotBiTich> DocDotBiTich();
    IEnumerable<DongBiTichChiTiet> DocBiTichChiTiet();
    IEnumerable<DongChuyenXu> DocChuyenXu();
    IEnumerable<DongRaoHonPhoi> DocRaoHonPhoi();
    IEnumerable<DongTanHien> DocTanHien();
    IEnumerable<DongLinhMuc> DocLinhMuc();
    IEnumerable<DongKhoiGiaoLy> DocKhoiGiaoLy();
    IEnumerable<DongLopGiaoLy> DocLopGiaoLy();
    IEnumerable<DongChiTietLopGiaoLy> DocChiTietLopGiaoLy();
    IEnumerable<DongGiaoLyVien> DocGiaoLyVien();
    IEnumerable<DongHoiDoan> DocHoiDoan();
    IEnumerable<DongChiTietHoiDoan> DocChiTietHoiDoan();
}
