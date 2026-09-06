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
}
