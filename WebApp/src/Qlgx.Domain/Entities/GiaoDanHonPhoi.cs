namespace Qlgx.Domain.Entities;

/// <summary>
/// Bảng nối giáo dân với hôn phối, giống ThanhVienGiaDinh: cố ý KHÔNG kế thừa ThucTheCoSo.
/// Khoá chính là bộ đôi (GiaoDanId, HonPhoiId) đúng như ràng buộc PK_GiaoDan_HonPhoi của
/// bản Access — một giáo dân chỉ tham gia một hôn phối cụ thể đúng một lần.
/// </summary>
public class GiaoDanHonPhoi
{
    public Guid GiaoDanId { get; set; }
    public Guid HonPhoiId { get; set; }
    public int SoThuTu { get; set; }
    public Guid GiaoXuId { get; set; }

    public GiaoDan? GiaoDan { get; set; }
    public HonPhoi? HonPhoi { get; set; }
}
