namespace Qlgx.Migration;

public record DongGiaoXu(int MaGiaoXu, int? MaGiaoXuRieng, string TenGiaoXu, string? TenGiaoHat,
    string? TenGiaoPhan, string? DiaChi, string? DienThoai, string? Email, string? Website,
    string? GhiChu);

public record DongGiaoHo(int MaGiaoHo, string TenGiaoHo, int? MaGiaoHoCha, bool DaXoa,
    string? MaNhanDang);

public record DongGiaDinh(int MaGiaDinh, int? MaGiaoHo, string? TenGiaDinh, string? DiaChi,
    string? DienThoai, string? SoHoKhau, string? DienGiaDinh, bool DaXoa, bool DaChuyenXu,
    string? NgayChuyen, string? NoiChuyen, bool GiaDinhAo, string? MaNhanDang);

public record DongGiaoDan(int MaGiaoDan, string HoTen, string? TenThanh, string? Phai,
    int? MaGiaoHo, string? NgaySinh, string? NgayRuaToi, string? NgayRuocLe, string? NgayThemSuc,
    bool QuaDoi, string? NgayQuaDoi, bool DaXoa, string? MaNhanDang);

public record DongThanhVien(int MaGiaDinh, int MaGiaoDan, int VaiTro, bool ChuHo);

/// <summary>
/// Ánh xạ bảng HonPhoi trong Access. Không có cột xoá mềm — đúng như bản gốc, xem
/// Qlgx.Domain.Entities.HonPhoi.
/// </summary>
public record DongHonPhoi(int MaHonPhoi, string? TenHonPhoi, string? SoHonPhoi, string? NoiHonPhoi,
    string? NgayHonPhoi, string? LinhMucChung, string? NguoiChung1, string? NguoiChung2,
    string? CachThucHonPhoi, string? GhiChu, string? MaNhanDang);

/// <summary>Bảng nối giáo dân với hôn phối — khoá tổ hợp (MaGiaoDan, MaHonPhoi).</summary>
public record DongGiaoDanHonPhoi(int MaGiaoDan, int MaHonPhoi, int SoThuTu);

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
}
