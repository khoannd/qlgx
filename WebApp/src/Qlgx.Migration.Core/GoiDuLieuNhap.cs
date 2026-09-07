namespace Qlgx.Migration;

/// <summary>
/// Gói dữ liệu trung gian giữa hai bước của luồng nhập dữ liệu cho quản trị viên
/// (VIEC-TIEP-THEO.md mục 2.4, quyết định kiến trúc ghi ở can-review-sau.md mục 38). Máy chủ
/// web (Linux, Docker) KHÔNG đọc được file .mdb — ACE OLEDB chỉ chạy trên Windows. Vì vậy quản
/// trị viên chạy công cụ Qlgx.Migration (Windows, đã có sẵn DocAccess) tại máy của mình để
/// RÚT dữ liệu ra gói JSON này (không cần mạng tới PostgreSQL), rồi tải gói lên qua giao diện
/// web — máy chủ chỉ cần đọc JSON, không cần Windows, không cần mở cổng CSDL ra ngoài.
///
/// 26 danh sách bên dưới khớp 1-1 với 26 phương thức Doc* của <see cref="IDuLieuNguon"/> —
/// KHÔNG suy đoán lại cấu trúc, chỉ đóng gói nguyên văn kết quả DocAccess đã đọc được.
/// PhienBanGoi tăng lên nếu sau này đổi cấu trúc gói (thêm/bớt bảng) — máy chủ kiểm tra giá trị
/// này trước khi đọc để báo lỗi rõ ràng thay vì ném lỗi giải mã JSON khó hiểu.
/// </summary>
public record GoiDuLieuNhap(
    int PhienBanGoi,
    string TenGiaoXuNguon,
    DateTimeOffset XuatLuc,
    List<DongGiaoXu> GiaoXu,
    List<DongGiaoHo> GiaoHo,
    List<DongGiaDinh> GiaDinh,
    List<DongGiaoDan> GiaoDan,
    List<DongThanhVien> ThanhVien,
    List<DongHonPhoi> HonPhoi,
    List<DongGiaoDanHonPhoi> GiaoDanHonPhoi,
    List<DongGiaoPhan> GiaoPhan,
    List<DongGiaoHat> GiaoHat,
    List<DongCauHinh> CauHinh,
    List<DongDuLieuChung> DuLieuChung,
    List<DongVaiTro> VaiTro,
    List<DongTenLoaiTaiKhoan> TenLoaiTaiKhoan,
    List<DongTaiKhoan> TaiKhoan,
    List<DongDotBiTich> DotBiTich,
    List<DongBiTichChiTiet> BiTichChiTiet,
    List<DongChuyenXu> ChuyenXu,
    List<DongRaoHonPhoi> RaoHonPhoi,
    List<DongTanHien> TanHien,
    List<DongLinhMuc> LinhMuc,
    List<DongKhoiGiaoLy> KhoiGiaoLy,
    List<DongLopGiaoLy> LopGiaoLy,
    List<DongChiTietLopGiaoLy> ChiTietLopGiaoLy,
    List<DongGiaoLyVien> GiaoLyVien,
    List<DongHoiDoan> HoiDoan,
    List<DongChiTietHoiDoan> ChiTietHoiDoan)
{
    /// <summary>Phiên bản gói hiện tại — tăng lên khi đổi cấu trúc (xem ghi chú ở trên).</summary>
    public const int PhienBanHienTai = 1;

    /// <summary>Đóng gói toàn bộ 26 bảng đọc từ <paramref name="nguon"/> (thường là DocAccess).</summary>
    public static GoiDuLieuNhap TuNguon(IDuLieuNguon nguon, string tenGiaoXuNguon) => new(
        PhienBanHienTai, tenGiaoXuNguon, DateTimeOffset.UtcNow,
        nguon.DocGiaoXu().ToList(), nguon.DocGiaoHo().ToList(), nguon.DocGiaDinh().ToList(),
        nguon.DocGiaoDan().ToList(), nguon.DocThanhVien().ToList(), nguon.DocHonPhoi().ToList(),
        nguon.DocGiaoDanHonPhoi().ToList(), nguon.DocGiaoPhan().ToList(), nguon.DocGiaoHat().ToList(),
        nguon.DocCauHinh().ToList(), nguon.DocDuLieuChung().ToList(), nguon.DocVaiTro().ToList(),
        nguon.DocTenLoaiTaiKhoan().ToList(), nguon.DocTaiKhoan().ToList(), nguon.DocDotBiTich().ToList(),
        nguon.DocBiTichChiTiet().ToList(), nguon.DocChuyenXu().ToList(), nguon.DocRaoHonPhoi().ToList(),
        nguon.DocTanHien().ToList(), nguon.DocLinhMuc().ToList(), nguon.DocKhoiGiaoLy().ToList(),
        nguon.DocLopGiaoLy().ToList(), nguon.DocChiTietLopGiaoLy().ToList(), nguon.DocGiaoLyVien().ToList(),
        nguon.DocHoiDoan().ToList(), nguon.DocChiTietHoiDoan().ToList());
}

/// <summary>
/// Đọc lại một <see cref="GoiDuLieuNhap"/> đã giải mã JSON như một <see cref="IDuLieuNguon"/> —
/// để <c>ChuyenDoiDuLieu</c> (đã có sẵn, dùng CHUNG với công cụ dòng lệnh) chạy được thẳng trên
/// máy chủ mà không cần biết gì về .mdb/OleDb.
/// </summary>
public class DuLieuNguonTuGoi(GoiDuLieuNhap goi) : IDuLieuNguon
{
    public IEnumerable<DongGiaoXu> DocGiaoXu() => goi.GiaoXu;
    public IEnumerable<DongGiaoHo> DocGiaoHo() => goi.GiaoHo;
    public IEnumerable<DongGiaDinh> DocGiaDinh() => goi.GiaDinh;
    public IEnumerable<DongGiaoDan> DocGiaoDan() => goi.GiaoDan;
    public IEnumerable<DongThanhVien> DocThanhVien() => goi.ThanhVien;
    public IEnumerable<DongHonPhoi> DocHonPhoi() => goi.HonPhoi;
    public IEnumerable<DongGiaoDanHonPhoi> DocGiaoDanHonPhoi() => goi.GiaoDanHonPhoi;
    public IEnumerable<DongGiaoPhan> DocGiaoPhan() => goi.GiaoPhan;
    public IEnumerable<DongGiaoHat> DocGiaoHat() => goi.GiaoHat;
    public IEnumerable<DongCauHinh> DocCauHinh() => goi.CauHinh;
    public IEnumerable<DongDuLieuChung> DocDuLieuChung() => goi.DuLieuChung;
    public IEnumerable<DongVaiTro> DocVaiTro() => goi.VaiTro;
    public IEnumerable<DongTenLoaiTaiKhoan> DocTenLoaiTaiKhoan() => goi.TenLoaiTaiKhoan;
    public IEnumerable<DongTaiKhoan> DocTaiKhoan() => goi.TaiKhoan;
    public IEnumerable<DongDotBiTich> DocDotBiTich() => goi.DotBiTich;
    public IEnumerable<DongBiTichChiTiet> DocBiTichChiTiet() => goi.BiTichChiTiet;
    public IEnumerable<DongChuyenXu> DocChuyenXu() => goi.ChuyenXu;
    public IEnumerable<DongRaoHonPhoi> DocRaoHonPhoi() => goi.RaoHonPhoi;
    public IEnumerable<DongTanHien> DocTanHien() => goi.TanHien;
    public IEnumerable<DongLinhMuc> DocLinhMuc() => goi.LinhMuc;
    public IEnumerable<DongKhoiGiaoLy> DocKhoiGiaoLy() => goi.KhoiGiaoLy;
    public IEnumerable<DongLopGiaoLy> DocLopGiaoLy() => goi.LopGiaoLy;
    public IEnumerable<DongChiTietLopGiaoLy> DocChiTietLopGiaoLy() => goi.ChiTietLopGiaoLy;
    public IEnumerable<DongGiaoLyVien> DocGiaoLyVien() => goi.GiaoLyVien;
    public IEnumerable<DongHoiDoan> DocHoiDoan() => goi.HoiDoan;
    public IEnumerable<DongChiTietHoiDoan> DocChiTietHoiDoan() => goi.ChiTietHoiDoan;
}
