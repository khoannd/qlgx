namespace Qlgx.Domain.Entities;

/// <summary>
/// Giáo hạt nằm TRÊN cấp giáo xứ, giữa GiaoPhan và GiaoXu — cố ý KHÔNG kế thừa ThucTheCoSo và
/// KHÔNG có GiaoXuId, cùng lý do đã ghi ở GiaoPhan.cs và GiaoXu.cs: một giáo hạt phục vụ nhiều
/// giáo xứ, gắn GiaoXuId sẽ nhân bản giáo hạt theo từng giáo xứ.
/// </summary>
public class GiaoHat
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public int MaGiaoHatCu { get; set; }
    public Guid GiaoPhanId { get; set; }
    public string TenGiaoHat { get; set; } = "";
    public string? GhiChu { get; set; }
    public int? MaGiaoHatRieng { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public GiaoPhan? GiaoPhan { get; set; }
}
