namespace Qlgx.Domain.Entities;

/// <summary>
/// Ánh xạ 4 cột của bảng KhoiGiaoLy trong Access — khối giáo lý (ví dụ Khai Tâm, Rước Lễ,
/// Thêm Sức...). Rỗng ở giáo xứ khảo sát nhưng giáo xứ khác có dữ liệu.
///
/// NguoiQuanLy (Int32) LÀ khoá ngoại tới GiaoDan — đã đối chiếu Source/Giaoly/frmKhoiGiaoLy.cs
/// (dòng "row["nguoiquanly"] = txtNguoiQuanLy.MaGiaoDan;" và
/// "txtNguoiQuanLy.MaGiaoDan = (int)tbl.Rows[0]["NguoiQuanLy"];") và
/// Source/Giaoly/frmKhoiGiaoLy.Designer.cs (txtNguoiQuanLy là GxControl.GxGiaoDan, một control
/// chọn giáo dân, mặc định MaGiaoDan=-1 khi chưa chọn) — NguoiQuanLy lưu MaGiaoDan của người
/// quản lý khối, giá trị -1 nghĩa là chưa gán ai (xem task-17c-report.md).
/// </summary>
public class KhoiGiaoLy : ThucTheCoSo
{
    public int MaKhoiCu { get; set; }
    public string TenKhoi { get; set; } = "";
    public Guid? NguoiQuanLyId { get; set; }
    public string? GhiChu { get; set; }

    public GiaoDan? NguoiQuanLy { get; set; }
}
