namespace Qlgx.Domain.Entities;

/// <summary>
/// Bảng gốc định danh giáo xứ, cố ý KHÔNG kế thừa ThucTheCoSo: nó không thể mang GiaoXuId
/// trỏ vào chính nó, và Phase 1 chưa có màn hình sửa thông tin giáo xứ nên chưa cần UpdatedAt/
/// RowVersion/SourceSystem. Khi Phase 2 thêm màn hình đó thì bổ sung các cột kiểm toán.
/// </summary>
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
