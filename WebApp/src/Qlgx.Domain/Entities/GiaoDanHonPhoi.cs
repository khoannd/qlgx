namespace Qlgx.Domain.Entities;

/// <summary>
/// Bảng nối giáo dân với hôn phối. Ràng buộc PK_GiaoDan_HonPhoi của bản Access — một giáo dân
/// chỉ tham gia một hôn phối cụ thể đúng một lần — nay là CHỈ MỤC DUY NHẤT trên
/// (GiaoDanId, HonPhoiId), xem GiaoDanHonPhoiConfig.
///
/// Kế thừa ThucTheCoSo vì đúng lý do như ThanhVienGiaDinh: nhật ký thay đổi chỉ thấy các thực
/// thể kế thừa lớp này, và mỗi dòng nhật ký cần một khoá chính đơn để đánh địa chỉ bản ghi.
/// </summary>
public class GiaoDanHonPhoi : ThucTheCoSo
{
    public Guid GiaoDanId { get; set; }
    public Guid HonPhoiId { get; set; }
    public int SoThuTu { get; set; }

    public GiaoDan? GiaoDan { get; set; }
    public HonPhoi? HonPhoi { get; set; }
}
