namespace Qlgx.Domain.Entities;

/// <summary>
/// Ánh xạ 7 cột của bảng ChuyenXu trong Access — lịch sử chuyển xứ của một giáo dân. Có 6
/// thuộc tính riêng vì UpdateDate ánh xạ vào ThucTheCoSo.UpdatedAt. Rỗng ở giáo xứ khảo sát
/// (Vô Nhiễm) nhưng giáo xứ khác có dữ liệu — xem Qlgx.Domain.LoaiChuyenXu để biết bằng chứng
/// xác minh giá trị của LoaiChuyen.
/// </summary>
public class ChuyenXu : ThucTheCoSo
{
    public int MaChuyenXuCu { get; set; }
    public Guid GiaoDanId { get; set; }
    public DateOnly? NgayChuyen { get; set; }
    public string? NoiChuyen { get; set; }
    public LoaiChuyenXu LoaiChuyen { get; set; }
    public string? GhiChuChuyen { get; set; }

    public GiaoDan? GiaoDan { get; set; }
}
