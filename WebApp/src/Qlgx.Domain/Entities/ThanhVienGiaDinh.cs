namespace Qlgx.Domain.Entities;

/// <summary>
/// Bảng nối giáo dân với gia đình. Bản Access không khai báo khoá chính và CHO PHÉP một
/// giáo dân thuộc nhiều gia đình (con ở nhà cha mẹ, đồng thời có gia đình riêng) — ràng buộc
/// đó nay là CHỈ MỤC DUY NHẤT trên (GiaDinhId, GiaoDanId), xem ThanhVienGiaDinhConfig.
///
/// Kế thừa ThucTheCoSo để có khoá chính Guid riêng: bộ sinh nhật ký duyệt
/// ChangeTracker.Entries&lt;ThucTheCoSo&gt;() và mỗi dòng nhật ký phải đánh địa chỉ được đúng
/// MỘT bản ghi qua BanGhiId. Trước đây bảng này đứng ngoài nhật ký, nên việc chuyển một giáo
/// dân sang gia đình khác không sinh dòng nào và sổ gia đình phân kỳ vĩnh viễn giữa các máy.
/// </summary>
public class ThanhVienGiaDinh : ThucTheCoSo
{
    public Guid GiaDinhId { get; set; }
    public Guid GiaoDanId { get; set; }
    public VaiTroGiaDinh VaiTro { get; set; }
    public bool ChuHo { get; set; }

    public GiaDinh? GiaDinh { get; set; }
    public GiaoDan? GiaoDan { get; set; }
}
