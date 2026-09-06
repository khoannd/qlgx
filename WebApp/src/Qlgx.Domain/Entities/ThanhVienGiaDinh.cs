namespace Qlgx.Domain.Entities;

/// <summary>
/// Bảng nối giáo dân với gia đình. Bản Access không khai báo khoá chính và CHO PHÉP một
/// giáo dân thuộc nhiều gia đình (con ở nhà cha mẹ, đồng thời có gia đình riêng), nên
/// khoá ở đây là bộ ba (GiaDinhId, GiaoDanId, VaiTro).
/// </summary>
public class ThanhVienGiaDinh
{
    public Guid GiaDinhId { get; set; }
    public Guid GiaoDanId { get; set; }
    public VaiTroGiaDinh VaiTro { get; set; }
    public bool ChuHo { get; set; }
    public Guid GiaoXuId { get; set; }

    public GiaDinh? GiaDinh { get; set; }
    public GiaoDan? GiaoDan { get; set; }
}
