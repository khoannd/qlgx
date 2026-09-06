namespace Qlgx.Domain.Entities;

public class GiaoHo : ThucTheCoSo
{
    public int MaGiaoHoCu { get; set; }
    public string TenGiaoHo { get; set; } = "";
    /// <summary>Giáo họ cha — bản Access thêm cột này ở phiên bản 2.1.1.2.</summary>
    public Guid? GiaoHoChaId { get; set; }
    public bool DaXoa { get; set; }
    public string? MaNhanDang { get; set; }
}
