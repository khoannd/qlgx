namespace Qlgx.Domain.Entities;

/// <summary>
/// Ánh xạ 12 cột của bảng LinhMuc trong Access — danh sách linh mục của giáo xứ (không phải
/// giáo dân, không có khoá ngoại tới GiaoDan). Có 11 thuộc tính riêng vì UpdateDate ánh xạ vào
/// ThucTheCoSo.UpdatedAt. Rỗng ở giáo xứ khảo sát nhưng giáo xứ khác có dữ liệu.
/// </summary>
public class LinhMuc : ThucTheCoSo
{
    public int MaLinhMucCu { get; set; }
    public string? TenThanh { get; set; }
    public string HoTen { get; set; } = "";
    public DateOnly? NgaySinh { get; set; }
    public string? ChucVu { get; set; }
    public DateOnly? TuNgay { get; set; }
    public DateOnly? DenNgay { get; set; }
    public string? GhiChu { get; set; }
    public string? DienThoai { get; set; }
    public string? Email { get; set; }
    public bool DaXoa { get; set; }
}
