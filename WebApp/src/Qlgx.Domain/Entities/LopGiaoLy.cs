namespace Qlgx.Domain.Entities;

/// <summary>
/// Ánh xạ 6 cột của bảng LopGiaoLy trong Access — một lớp giáo lý, thuộc một khối
/// (Qlgx.Domain.Entities.KhoiGiaoLy). Rỗng ở giáo xứ khảo sát nhưng giáo xứ khác có dữ liệu.
/// MaKhoi là khoá ngoại bắt buộc (không có quy ước "0 = ngoài xứ" như GiaoHo/GiaDinh) — dòng
/// mồ côi (MaKhoi không tồn tại trong dữ liệu nguồn) bị bỏ qua kèm cảnh báo, không làm sập
/// lần chuyển (xem ChuyenDoiDuLieu.GhiLopGiaoLy).
/// </summary>
public class LopGiaoLy : ThucTheCoSo
{
    public int MaLopCu { get; set; }
    public string TenLop { get; set; } = "";
    public Guid KhoiGiaoLyId { get; set; }
    public int? Nam { get; set; }
    public string? PhongHoc { get; set; }
    public string? GhiChu { get; set; }

    public KhoiGiaoLy? KhoiGiaoLy { get; set; }
}
