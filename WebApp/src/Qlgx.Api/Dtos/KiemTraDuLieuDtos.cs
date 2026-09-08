namespace Qlgx.Api.Dtos;

/// <summary>
/// Kết quả một dòng của "Kiểm tra dữ liệu — giáo dân" (frmKiemTraGiaoDanList.cs +
/// ReviewGiaoDanProcess.cs — xem docs/superpowers/specs/man-hinh/cong-cu-du-lieu.md mục 2).
/// Bọc nguyên <see cref="GiaoDanListItemDto"/> thay vì chép lại 30 trường — lưới kết quả web
/// dùng lại đúng bộ cột của "Danh sách giáo dân", chỉ thêm hai trường ở đây.
/// </summary>
public record KiemTraGiaoDanKetQuaDto(
    GiaoDanListItemDto GiaoDan,
    /// <summary>Các lý do vi phạm, mỗi lý do một dòng — nối bằng "\n" đúng cách
    /// ReviewGiaoDanProcess.coNgayThangLoi/kiemTraThanhVienGiaDinh/kiemTraHonPhoi ghi vào cột
    /// NguyenNhan (mỗi đoạn bắt đầu bằng "- ").</summary>
    string NguyenNhan,
    /// <summary>Tổng các cờ bit ReviewGiaoDanType đã vi phạm (GxConstants.cs:192-199) — giữ lại
    /// để đối chiếu, KHÔNG hiển thị trên lưới web (xem "Chỗ chưa chắc" mục 6 của spec: bản
    /// desktop có thể cũng lỡ hiện cột số khó hiểu này, bản web cố ý không lặp lại).</summary>
    int KetQua);

/// <summary>Tuỳ chọn 6 loại kiểm tra của màn hình "Kiểm tra dữ liệu — giáo dân" — mỗi cờ mặc
/// định true nếu người gọi không truyền (đúng "mặc định đều tick sẵn" của desktop,
/// frmKiemTraGiaoDanList.Designer.cs:213,225,237,249,261,289).</summary>
public record KiemTraGiaoDanTuyChon(
    bool KhongCoNgayThang,
    bool SaiQuanHeNgayThang,
    bool RuocLeTruocTuoi,
    bool ThuocNhieuGiaDinh,
    bool KhongThuocGiaDinhNao,
    bool CoNhieuHonPhoi);
