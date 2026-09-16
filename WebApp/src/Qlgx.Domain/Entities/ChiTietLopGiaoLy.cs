namespace Qlgx.Domain.Entities;

/// <summary>
/// Ánh xạ 5 cột của bảng ChiTietLopGiaoLy trong Access — học viên trong một lớp giáo lý. Khoá
/// gốc Access là cặp (MaLop, MaGiaoDan). Kế thừa ThucTheCoSo (giống BiTichChiTiet ở Task 17B)
/// thay vì dùng thẳng cặp khoá đó làm khoá chính vật lý, để tránh đụng khoá khi nhiều giáo xứ
/// dùng chung một database — cặp khoá cũ được ép về duy nhất qua chỉ mục
/// (GiaoXuId, LopGiaoLyId, GiaoDanId) thay vì khoá chính. Rỗng ở giáo xứ khảo sát nhưng giáo xứ
/// khác có dữ liệu.
/// </summary>
public class ChiTietLopGiaoLy : ThucTheCoSo
{
    public Guid LopGiaoLyId { get; set; }
    public Guid GiaoDanId { get; set; }
    public int? SoThuTu { get; set; }
    public bool HoanThanh { get; set; }
    public string? GhiChuGLy { get; set; }

    public LopGiaoLy? LopGiaoLy { get; set; }
    public GiaoDan? GiaoDan { get; set; }
}
