namespace Qlgx.Domain.Entities;

/// <summary>
/// Bảng nối giáo dân với gia đình. Bản Access không khai báo khoá chính và CHO PHÉP một
/// giáo dân thuộc nhiều gia đình (con ở nhà cha mẹ, đồng thời có gia đình riêng). Khoá phức cũ
/// của bản Access nay là CHỈ MỤC DUY NHẤT trên BỘ BA (GiaDinhId, GiaoDanId, VaiTro) — đúng bộ ba,
/// KHÔNG phải bộ đôi (GiaDinhId, GiaoDanId): một người có thể mang nhiều vai trò trong cùng một
/// gia đình, nên siết xuống bộ đôi sẽ làm mất dòng khi nhập dữ liệu Access. Xem chú thích dài ở
/// ThanhVienGiaDinhConfig để biết lý do đầy đủ — đừng "sửa cho khớp" theo hướng bộ đôi.
/// Ngoài chỉ mục bộ ba đó còn một chỉ mục lọc riêng (GiaDinhId, VaiTro) WHERE vai_tro IN (0,1)
/// giữ luật "mỗi gia đình tối đa một Chồng, một Vợ" — hai ràng buộc khác nhau, đừng lẫn.
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
