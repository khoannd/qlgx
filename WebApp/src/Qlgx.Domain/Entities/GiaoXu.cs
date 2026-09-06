namespace Qlgx.Domain.Entities;

public class GiaoXu
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public int MaGiaoXuCu { get; set; }
    public int? MaGiaoXuRieng { get; set; }
    public string TenGiaoXu { get; set; } = "";
    public string? TenGiaoHat { get; set; }
    public string? TenGiaoPhan { get; set; }
    public string? DiaChi { get; set; }
    public string? DienThoai { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }
    public string? GhiChu { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
