namespace Qlgx.Domain.Entities;

/// <summary>
/// Giáo phận nằm TRÊN cấp giáo xứ — cố ý KHÔNG kế thừa ThucTheCoSo và KHÔNG có GiaoXuId,
/// giống tiền lệ của GiaoXu (xem ghi chú trong GiaoXu.cs). Một giáo phận phục vụ nhiều giáo
/// xứ; nếu gắn GiaoXuId thì mỗi giáo xứ sẽ có một bản sao giáo phận riêng, phá vỡ đúng thứ
/// phân cấp "quản lý danh sách giáo xứ theo giáo phận" mà mô hình tập trung cần có.
/// </summary>
public class GiaoPhan
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public int MaGiaoPhanCu { get; set; }
    public string TenGiaoPhan { get; set; } = "";
    public string? GhiChu { get; set; }
    public int? MaGiaoPhanRieng { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
