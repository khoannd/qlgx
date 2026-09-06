namespace Qlgx.Domain.Entities;

/// <summary>
/// Ánh xạ đủ 2 cột của bảng GiaoLyVien trong Access — giáo lý viên phụ trách một lớp giáo lý.
/// Khoá gốc Access là cặp (MaLop, MaGiaoDan), giống ChiTietLopGiaoLy — cùng lý do kế thừa
/// ThucTheCoSo thay vì dùng cặp khoá đó làm khoá chính vật lý (tránh đụng khoá đa giáo xứ),
/// ép duy nhất qua chỉ mục (GiaoXuId, LopGiaoLyId, GiaoDanId). Rỗng ở giáo xứ khảo sát nhưng
/// giáo xứ khác có dữ liệu.
/// </summary>
public class GiaoLyVien : ThucTheCoSo
{
    public Guid LopGiaoLyId { get; set; }
    public Guid GiaoDanId { get; set; }

    public LopGiaoLy? LopGiaoLy { get; set; }
    public GiaoDan? GiaoDan { get; set; }
}
