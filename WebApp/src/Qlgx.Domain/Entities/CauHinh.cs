namespace Qlgx.Domain.Entities;

/// <summary>
/// Cấu hình theo giáo xứ. Bản Access dùng khoá chính là chuỗi (MaCauHinh); trong mô hình tập
/// trung nhiều giáo xứ dùng chung một database, khoá đó có thể trùng giữa các giáo xứ khác
/// nhau, nên khoá thật là Id (Guid, kế thừa từ ThucTheCoSo) kèm ràng buộc duy nhất trên tổ hợp
/// (GiaoXuId, MaCauHinh) — xem CauHinhConfig.
/// </summary>
public class CauHinh : ThucTheCoSo
{
    public string MaCauHinh { get; set; } = "";
    public string? GiaTri { get; set; }
    public string? MoTa { get; set; }
}
