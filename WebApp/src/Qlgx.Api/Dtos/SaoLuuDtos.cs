namespace Qlgx.Api.Dtos;

/// <summary>Thân yêu cầu POST /api/sao-luu/cong-viec.</summary>
/// <param name="Loai">Một trong LoaiCongViecSaoLuu.HopLe.</param>
/// <param name="SnapshotId">Bắt buộc với "phuc_hoi" và "tai_ve".</param>
/// <param name="XacNhan">Bắt buộc với "phuc_hoi": phải đúng nguyên văn "PHUC HOI TOAN BO".</param>
/// <param name="Nhan">Nhãn tuỳ chọn cho bản sao thủ công.</param>
public sealed record TaoCongViecYeuCau(string Loai, string? SnapshotId, string? XacNhan, string? Nhan);

public sealed record CongViecDto(
    Guid Id, string Loai, string TrangThai, string? BuocHienTai, string? NhatKy,
    DateTimeOffset TaoLuc, DateTimeOffset? BatDauLuc, DateTimeOffset? KetThucLuc);

public sealed record BanSaoLuuDto(
    string Id, DateTimeOffset ThoiDiem, string? Nhan, long KichThuocByte,
    int SoGiaoDan, int SoGiaDinh, string Nguon);

/// <param name="Den">"xanh" | "vang" | "do" — xem SaoLuuService.TinhDen.</param>
public sealed record TinhTrangSaoLuuDto(
    string Den, DateTimeOffset? SaoLuuGanNhat, int SoBanSao,
    DateTimeOffset? DienTapGanNhat, bool DienTapDat, string? LoiGanNhat);
