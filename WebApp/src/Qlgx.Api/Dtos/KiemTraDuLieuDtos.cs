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

/// <summary>
/// Kết quả một dòng của "Kiểm tra dữ liệu — gia đình" (frmKiemTraGiaDinhList.cs +
/// ReviewGiaDinhProcess.cs — xem docs/superpowers/specs/man-hinh/cong-cu-du-lieu.md mục 3).
/// Bọc nguyên <see cref="GiaDinhListItemDto"/> giống cách làm bên giáo dân.
/// </summary>
public record KiemTraGiaDinhKetQuaDto(
    GiaDinhListItemDto GiaDinh,
    /// <summary>Các lý do vi phạm — TÁI HIỆN NGUYÊN VĂN một bug thật của
    /// ReviewGiaDinhProcess.nhieuVoChong (dòng 259): khi quy tắc "nhiều vợ/chồng" khớp, nó GHI
    /// ĐÈ toàn bộ NguyenNhan bằng đúng một câu của riêng nó, XOÁ MẤT lý do của 3 quy tắc kia dù
    /// KetQua (cờ bit) vẫn cộng dồn đủ. Xem spec mục 3.5 — không tự sửa.</summary>
    string NguyenNhan,
    /// <summary>Tổng các cờ bit ReviewGiaDinhType đã vi phạm (GxConstants.cs:202-207).</summary>
    int KetQua);

/// <summary>Tuỳ chọn 4 loại kiểm tra của "Kiểm tra dữ liệu — gia đình" — mặc định true nếu
/// không truyền (đúng "4 ô tick đều tick sẵn" của desktop).</summary>
public record KiemTraGiaDinhTuyChon(
    bool KhongCoNgayHonPhoi,
    bool HonPhoiTruocTuoi,
    bool KhoangCachTuoiConCai,
    bool CacVanDeKhac);
