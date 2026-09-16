namespace Qlgx.Domain.Entities;

/// <summary>
/// Ánh xạ 12 cột của bảng HonPhoi trong Access. Lớp này có 11 thuộc tính riêng vì cột
/// UpdateDate của Access được thể hiện bằng ThucTheCoSo.UpdatedAt. Bản Access không có
/// cột xoá mềm cho bảng này, nên xoá là xoá cứng.
/// </summary>
public class HonPhoi : ThucTheCoSo
{
    public int MaHonPhoiCu { get; set; }
    public string? TenHonPhoi { get; set; }
    public string? SoHonPhoi { get; set; }
    public string? NoiHonPhoi { get; set; }
    public DateOnly? NgayHonPhoi { get; set; }
    public string? LinhMucChung { get; set; }
    public string? NguoiChung1 { get; set; }
    public string? NguoiChung2 { get; set; }
    public string? CachThucHonPhoi { get; set; }
    public string? GhiChu { get; set; }
    public string? MaNhanDang { get; set; }

    public List<GiaoDanHonPhoi> GiaoDanThamGia { get; set; } = [];
}
