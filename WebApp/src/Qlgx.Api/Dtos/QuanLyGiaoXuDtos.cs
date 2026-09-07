namespace Qlgx.Api.Dtos;

/// <summary>
/// DTO cho màn hình "Quản lý giáo phận/giáo hạt/giáo xứ" (policy "QuanTriHeThong" — xem
/// docs/superpowers/specs/man-hinh/quan-ly-giao-xu.md mục 4). GiaoPhan/GiaoHat/GiaoXu KHÔNG có
/// giao_xu_id và KHÔNG có RLS bảo vệ, nên các bản ghi này CỐ Ý hiển thị TOÀN BỘ máy chủ, khác
/// với mọi DTO khác trong hệ thống vốn luôn bị lọc theo giáo xứ của người gọi.
/// </summary>
public record GiaoPhanDto(Guid Id, string TenGiaoPhan, string? GhiChu);

public record TaoGiaoPhanRequest(string TenGiaoPhan, string? GhiChu);
public record CapNhatGiaoPhanRequest(string TenGiaoPhan, string? GhiChu);

public record GiaoHatDto(Guid Id, Guid GiaoPhanId, string TenGiaoPhan, string TenGiaoHat, string? GhiChu);

public record TaoGiaoHatRequest(Guid GiaoPhanId, string TenGiaoHat, string? GhiChu);
public record CapNhatGiaoHatRequest(Guid GiaoPhanId, string TenGiaoHat, string? GhiChu);

public record GiaoXuDto(Guid Id, Guid? GiaoHatId, string? TenGiaoHat, string? TenGiaoPhan,
    string TenGiaoXu, string? DiaChi, string? DienThoai, string? Email, string? Website,
    string? GhiChu, bool CoTrungTen, int SoTaiKhoan);

public record TaoGiaoXuRequest(Guid GiaoHatId, string TenGiaoXu, string? DiaChi,
    string? DienThoai, string? Email, string? Website, string? GhiChu);

public record CapNhatGiaoXuRequest(Guid GiaoHatId, string TenGiaoXu, string? DiaChi,
    string? DienThoai, string? Email, string? Website, string? GhiChu);

/// <summary>Tạo tài khoản quản trị ĐẦU TIÊN cho một giáo xứ khác giáo xứ của người gọi — luôn
/// LoaiTaiKhoan=0 (quản trị viên thường), không nhận GiaoXuId từ thân yêu cầu (lấy từ đường
/// dẫn {id}) và không cấp được LoaiTaiKhoan=9 qua API (chỉ CLI mới cấp cấp hệ thống).</summary>
public record TaoTaiKhoanChoGiaoXuRequest(string TenTaiKhoan, string MatKhau, string HoTenNguoiDung,
    string? Email, string? SoDienThoai);
