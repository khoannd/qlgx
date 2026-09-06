namespace Qlgx.Domain.Entities;

/// <summary>
/// Cột chung của mọi bảng nghiệp vụ. GiaoXuId có mặt ngay cả khi mỗi database hiện chỉ
/// chứa một giáo xứ — nhờ vậy sau này gộp nhiều giáo xứ vào một database chỉ cần bật
/// Row-Level Security theo cột này, không phải sửa tầng truy cập dữ liệu.
/// </summary>
public abstract class ThucTheCoSo
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid GiaoXuId { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>Ánh xạ tới cột hệ thống xmin của PostgreSQL, dùng cho khoá lạc quan.</summary>
    public uint RowVersion { get; set; }

    /// <summary>Đánh dấu bản ghi đến từ đâu, ví dụ "access-2026-09-06".</summary>
    public string? SourceSystem { get; set; }

    /// <summary>
    /// Giá trị gốc không phân giải được khi chuyển đổi, dạng {"NgaySinh":"32/13/2005"}.
    /// Có để dữ liệu hỏng vẫn được giữ lại thay vì mất im lặng.
    /// </summary>
    public string? DuLieuLoi { get; set; }
}
