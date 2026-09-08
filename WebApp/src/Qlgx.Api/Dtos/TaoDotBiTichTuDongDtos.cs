using Qlgx.Domain;

namespace Qlgx.Api.Dtos;

/// <summary>"Tạo danh sách bí tích tự động" — xem
/// docs/superpowers/specs/man-hinh/cong-cu-du-lieu.md mục 5.2.</summary>
public record TaoDotBiTichTuDongRequest(
    LoaiBiTich LoaiBiTich, string? LinhMuc, string? NoiBiTich, DateOnly TuNgay, DateOnly DenNgay);

/// <summary>Một đợt bí tích MỚI dự kiến sẽ được tạo — chỉ liệt kê đợt MỚI (không liệt kê đợt đã
/// có sẵn dù có thêm giáo dân vào đó), tối đa 30 dòng đầu.</summary>
public record DuKienDotMoiDto(DateOnly Ngay, string? LinhMuc, string? NoiBiTich, int SoGiaoDan);

public record TaoDotBiTichXemTruocKetQua(
    int TongGiaoDanKhopDieuKien, int SoDotMoiSeTao, int SoGiaoDanMoiSeThem, List<DuKienDotMoiDto> MauDotMoi);

public record TaoDotBiTichTuDongKetQua(int SoDotDaTao, int SoGiaoDanDaThem);
