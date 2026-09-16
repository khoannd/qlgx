namespace Qlgx.Domain.Entities;

/// <summary>
/// Chuỗi phát xuống — CHỈ những thay đổi đã thắng cuộc gộp, mang số thứ tự liên tục theo từng
/// giáo xứ. Đây là thứ duy nhất máy con kéo về.
/// </summary>
public class HieuLuc
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid GiaoXuId { get; set; }

    /// <summary>Tăng dần riêng theo từng giáo xứ. Cấp qua CapSoHieuLuc, không nơi nào khác.</summary>
    public long SoThuTu { get; set; }
    /// <summary>Danh tính của chuỗi số — đổi khi khôi phục hoặc dọn nhật ký.</summary>
    public Guid Epoch { get; set; }

    public string Bang { get; set; } = "";
    public Guid BanGhiId { get; set; }
    public string Truong { get; set; } = "";
    public string? GiaTri { get; set; }

    public DateTimeOffset DongHoVatLy { get; set; }
    public long DongHoLogic { get; set; }
    public Guid? ThietBiId { get; set; }

    public Guid GiaoDichId { get; set; }
}
