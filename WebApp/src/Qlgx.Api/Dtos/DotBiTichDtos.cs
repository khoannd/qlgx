using Qlgx.Domain;

namespace Qlgx.Api.Dtos;

/// <summary>
/// Một dòng trên lưới "Danh sách sổ bí tích" — khớp cột của <c>GxDotBiTichList.FormatGrid</c>
/// (Source/GXControl/GxDotBiTichList.cs:53-91): Ngày, Mô tả, Người ban bí tích, Nơi nhận bí
/// tích, Số lượng GD. SoLuong tính ở tầng dịch vụ (đếm BiTichChiTiet), không lưu cứng — bản
/// desktop tính bằng subquery SL trong SELECT_DOTBITICH_LIST.
/// </summary>
public record DotBiTichListItemDto(
    Guid Id, int MaDotBiTichCu, LoaiBiTich LoaiBiTich,
    DateOnly? NgayBiTich, string? MoTa, string? LinhMuc, string? NoiBiTich, int SoLuong);

/// <summary>
/// Một người trong danh sách nhận bí tích của một đợt — khớp cột của
/// <c>GxBiTichChiTiet.FormatGrid</c> (Source/GXControl/GxBiTichChiTiet.cs:273-401): Số bí
/// tích (nhãn đổi theo loại — "Số rửa tội"/"Số XTRL"/"Số thêm sức"), Tên thánh, Họ tên, Phái,
/// Ngày sinh, Người đỡ đầu (chỉ Rửa tội/Thêm sức), Ghi chú. SoBiTich/NguoiDoDau đọc/ghi thẳng
/// vào GiaoDan.SoRuaToi.../NguoiDoDauRuaToi... (không có trong BiTichChiTiet) — xem
/// docs/superpowers/specs/man-hinh/so-bi-tich.md mục 2.
/// </summary>
public record NguoiNhanBiTichDto(
    Guid GiaoDanId, int MaGiaoDanCu, string? TenThanh, string HoTen, string? Phai,
    DateOnly? NgaySinh, string? SoBiTich, string? NguoiDoDau, string? GhiChu);

public record DotBiTichDetailDto(
    Guid Id, int MaDotBiTichCu, LoaiBiTich LoaiBiTich, DateOnly? NgayBiTich,
    string? MoTa, string? LinhMuc, string? NoiBiTich, uint RowVersion,
    IReadOnlyList<NguoiNhanBiTichDto> NguoiNhan);

/// <summary>Tạo một đợt bí tích mới — khớp các trường nhập ở <c>frmBiTichChiTiet</c>
/// (txtLinhMuc, dtNgayBiTich, txtMoTa, txtNoiBiTich) cộng LoaiBiTich (chọn từ màn hình danh
/// sách, <c>cbLoaiBiTich.SelectedValue</c>, frmDotBiTichList.cs:93).</summary>
public record TaoDotBiTichRequest(
    LoaiBiTich LoaiBiTich, DateOnly? NgayBiTich, string MoTa, string? LinhMuc, string? NoiBiTich);

public record CapNhatDotBiTichRequest(
    DateOnly? NgayBiTich, string MoTa, string? LinhMuc, string? NoiBiTich, uint RowVersion);

/// <summary>Thêm một giáo dân vào danh sách nhận bí tích của đợt — khớp <c>addGiaoDan</c>
/// (frmBiTichChiTiet.cs:139-239). Ngày/Linh mục/Nơi KHÔNG nhập riêng cho từng người — đợt lưu
/// xong sẽ copy xuống GiaoDan của mọi người trong đợt (xem AssignDataSource + vòng lặp
/// getGridData, dòng 385-387).</summary>
public record ThemNguoiNhanRequest(Guid GiaoDanId, string? SoBiTich, string? NguoiDoDau, string? GhiChu);

public record SuaNguoiNhanRequest(string? SoBiTich, string? NguoiDoDau, string? GhiChu);
